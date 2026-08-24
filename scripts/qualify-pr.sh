#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
mode=${1:-}
shift || true
ci_root="$repo_root/artifacts/ci"
route_path=${SIR_CI_ROUTE:-$ci_root/route.json}
phase_path=${SIR_CI_PHASE_PATH:-$ci_root/phase-timing.json}

mkdir -p "$ci_root/results" "$ci_root/prepared"
cd "$repo_root"

start_dotnet_trace() {
  trace_dir=$(mktemp -d /tmp/sir-pr-dotnet-trace.XXXXXX)
  trace_log="$trace_dir/invocations.log"
  real_dotnet=$(command -v dotnet)
  ln -s "$repo_root/scripts/dotnet-invocation-trace.sh" "$trace_dir/dotnet"
  export SIR_REAL_DOTNET="$real_dotnet"
  export SIR_DOTNET_INVOCATION_LOG="$trace_log"
  export SIR_DOTNET_TRACE_ROOT="$repo_root"
  export PATH="$trace_dir:$PATH"
}

append_traced_builds() {
  local -n target=$1
  [[ -f "${trace_log:-}" ]] || return 0
  while IFS=$'\t' read -r kind project identity _started _completed; do
    [[ -n "$kind" && -n "$project" ]] || continue
    invocation="$kind:$project"
    [[ "$identity" == "-" ]] || invocation="$invocation:$identity"
    target+=(--build "$invocation")
  done < <(sort "$trace_log")
}

# S.I.R.#272 — whether a work package still owes evidence is a THREE-valued question: owed, not
# owed, or impossible to determine. Answering it with two values is how this gate was silenced
# twice. The original defect pre-conditioned the check on `work/<id>/evidence.yml` existing, so
# deleting that file removed the check. The first repair moved the load-bearing existence condition
# one file to the left, onto `work/<id>/tasks.yml` — `rm work/<id>/tasks.yml` restored the escape in
# one extra command — and its text match (`$2 == "done"`) disagreed with the tool it claimed parity
# with on `status: "done"`, a legal and identical YAML scalar. Both are one class: a mechanism
# returning a confident answer about an input it never evaluated.
#
# So this function does not read tasks.yml. It asks the tool that owns the format and reads the count
# that tool reports. The answer rests on a POSITIVE fact — an integer `tasks.doneCount` in fsgg-sdd's
# own report — and never on the absence of a diagnostic. Anything that stops the tool producing that
# integer (tasks.yml deleted, unreadable, unparseable, a report schema that changes under us, output
# that is not JSON, a toolchain that is not installed at all) therefore arrives here as "cannot
# determine" WITHOUT this code enumerating those shapes — which is the property the enumeration it
# replaced could not have, because a fourth shape always exists. `--dry-run` keeps the question
# read-only. The tool's exit code is deliberately NOT consulted: it is a verdict about lifecycle
# readiness, not a statement about whether tasks.yml could be parsed, and conflating those two is
# the confusion this gate exists to prevent.
#
#   0 = evidence is owed   ·   1 = evidence is not owed   ·   2 = cannot determine (fails closed)
work_declares_completed_implementation() {
  local work_id=$1 report_path status=0
  report_path=$(mktemp "${TMPDIR:-/tmp}/sir-evidence-owed.XXXXXX")
  dotnet fsgg-sdd verify --work "$work_id" --root . --json --dry-run >"$report_path" 2>/dev/null || true
  node - "$report_path" <<'NODE' || status=$?
const { readFileSync } = require("node:fs");
let report;
try {
  report = JSON.parse(readFileSync(process.argv[2], "utf8"));
} catch {
  process.exit(2);
}
const tasks = report !== null && typeof report === "object" ? report.tasks : null;
if (tasks === null || typeof tasks !== "object") process.exit(2);
const done = tasks.doneCount;
if (!Number.isSafeInteger(done) || done < 0) process.exit(2);
process.exit(done > 0 ? 0 : 1);
NODE
  rm -f -- "$report_path"
  return "$status"
}

run_browser_shard_pair() {
  local first_index=$1
  local second_index=$2
  local first_pid second_pid status=0
  [[ -n "${SIR_JUNIT_OUTPUT:-}" && -n "${SIR_JUNIT_OUTPUT_2:-}" ]] || {
    echo "qualify-pr: paired browser shards require both JUnit output paths" >&2
    return 2
  }
  SIR_BROWSER_SHARDS=4 SIR_BROWSER_SHARD_INDEX="$first_index" SIR_BROWSER_COHORT=general \
    SIR_JUNIT_OUTPUT="$SIR_JUNIT_OUTPUT" npm run test:browser &
  first_pid=$!
  SIR_BROWSER_SHARDS=4 SIR_BROWSER_SHARD_INDEX="$second_index" SIR_BROWSER_COHORT=general \
    SIR_JUNIT_OUTPUT="$SIR_JUNIT_OUTPUT_2" npm run test:browser &
  second_pid=$!
  wait "$first_pid" || status=1
  wait "$second_pid" || status=1
  return "$status"
}

case "$mode" in
  route)
    paths_file=${1:?qualify-pr route requires a changed-path file}
    commit=$(git rev-parse HEAD)
    tree=$(git rev-parse "${commit}^{tree}")
    node scripts/ci-route.mjs route --paths-file "$paths_file" --commit "$commit" --tree "$tree" --output "$route_path"
    ;;
  integrity)
    node scripts/test-ci-route.mjs
    ./scripts/test-ci-route-mutations.sh
    node scripts/test-protected-stage-receipts.mjs
    node scripts/test-pages-qualified-handoff.mjs
    node scripts/test-ci-cost-report.mjs
    node scripts/test-linux-runtime-closure.mjs
    node scripts/test-workflow-action-contract.mjs
    node scripts/test-ci-integrity-plan.mjs
    ./scripts/test-ci-gate-artifact-isolation.sh
    ./scripts/test-ci-evidence-mutation.sh
    ./scripts/test-ci-failure-timing-mutation.sh
    node scripts/ci-integrity-plan.mjs --route "$route_path" --output "$ci_root/results/integrity-plan.json" >/dev/null
    integrity_runs() { jq -e --arg id "$1" '.subjects[] | select(.id == $id and .run == true)' "$ci_root/results/integrity-plan.json" >/dev/null; }
    if integrity_runs npm-audit; then
      node .github/scripts/test-npm-audit.mjs
      node .github/scripts/check-npm-audit.mjs
    fi
    if integrity_runs governance; then ./scripts/verify-fable-game-governance.sh; fi
    if integrity_runs dependency-surface; then
      dotnet fsgg-sdd dependency-surface --check --param packageId=FS.GG.Game.Core --param version=0.13.0 --root . --text
    fi
    if integrity_runs sdd-byte-stability; then ./scripts/test-item-184-sdd-byte-stability.sh; fi
    if integrity_runs feedback-audit; then ./scripts/test-feedback-audit-binding-exceptions.sh; fi
    # S.I.R.#265. Holds docs/coordination-engine-contracts.md to the coordination engine it
    # documents. It is pure — it loads FS.GG.Coord.Core.dll and runs `facts`, performs no board IO,
    # and needs no prepared part — so it dispatches directly here rather than as a `gate` subject.
    # An earlier attempt (S.I.R.#255, reverted at c3b10be) routed it through `run-ci-gate.sh` as a
    # gate subject, where `ci-route.mjs` refuses an unknown subject and the route exited 1 on a
    # correct document and 1 on a falsified one alike. Here the subject is planned and gated exactly
    # like the five above it, so the sweep covers it for free and per-PR cost stays path-conditional.
    if integrity_runs review-contract; then ./scripts/test-review-contract-coherence.sh; fi
    ;;
  prepare-part)
    part=${1:?qualify-pr prepare-part requires native|fable|web|server|docs}
    case "$part" in
      native|fable|web|server|docs) ;;
      *) echo "qualify-pr: unknown prepare part: $part" >&2; exit 2 ;;
    esac
    part_root="$ci_root/parts"
    mkdir -p "$part_root"
    runner_started=${SIR_CI_RUNNER_START_MS:-$(date +%s%3N)}
    command_started=$(date +%s%3N)
    restore_started=$(date +%s%3N)
    restore_completed=$restore_started
    build_started=$restore_started
    build_completed=$restore_started
    failure_stage=restore
    write_part_timing() {
      part_status=$?
      trap - EXIT
      completed=$(date +%s%3N)
      [[ $part_status -eq 0 ]] && failure_stage=null
      if [[ "$failure_stage" == restore ]]; then
        restore_completed=$completed
        build_started=$completed
        build_completed=$completed
      elif [[ "$failure_stage" == build ]]; then
        build_completed=$completed
      fi
      node - "$part_root/$part.timing.json" "$runner_started" "$command_started" "$restore_started" "$restore_completed" "$build_started" "$build_completed" "$completed" "$failure_stage" <<'NODE'
const { writeFileSync } = require("node:fs");
const [path, runner, command, restoreStarted, restoreCompleted, buildStarted, buildCompleted, completed, failureStage] = process.argv.slice(2);
writeFileSync(path, `${JSON.stringify({ startedAtMilliseconds: Number(runner), completedAtMilliseconds: Number(completed), setup: Number(command) - Number(runner), restore: Number(restoreCompleted) - Number(restoreStarted), build: Number(buildCompleted) - Number(buildStarted), transport: Number(completed) - Number(buildCompleted), failureStage: failureStage === "null" ? null : failureStage })}\n`);
NODE
      readarray -t route_binding < <(node - "$route_path" <<'NODE'
const { readFileSync } = require("node:fs");
const route = JSON.parse(readFileSync(process.argv[2], "utf8"));
console.log(route.source.commit); console.log(route.source.tree); console.log(route.digest);
NODE
      )
      status=pass
      [[ $part_status -eq 0 ]] || status=fail
      artifact_digest=none
      if [[ -f "$part_root/$part.manifest.path" ]]; then
        part_manifest=$(<"$part_root/$part.manifest.path")
        artifact_digest=$(sha256sum "$part_manifest" | cut -d' ' -f1)
      fi
      build_args=(--build "producer:$part")
      append_traced_builds build_args
      node scripts/ci-route.mjs gate --gate "prepare-$part" --status "$status" \
        --commit "${route_binding[0]}" --tree "${route_binding[1]}" --route-digest "${route_binding[2]}" \
        --artifact-digest "$artifact_digest" --queue-ms unknown \
        --setup-ms "$((command_started - runner_started))" \
        --restore-ms "$((restore_completed - restore_started))" \
        --build-ms "$((build_completed - build_started))" \
        --transport-ms "$((completed - build_completed))" --test-ms 0 \
        --total-ms "$((completed - runner_started))" --failure-stage "$failure_stage" \
        "${build_args[@]}" \
        --output "$ci_root/results/prepare-$part.json" >/dev/null
      if [[ -n "${trace_dir:-}" ]]; then rm -rf -- "$trace_dir"; fi
      exit "$part_status"
    }
    trap write_part_timing EXIT
    start_dotnet_trace
    case "$part" in
      fable|web|docs) dotnet tool restore ;;
    esac
    # Restore only the graph owned by this producer. Each root has a committed
    # packages.lock.json and project restore traverses its project references,
    # so locked dependency validation remains fail-closed without making every
    # parallel producer evaluate the entire solution.
    case "$part" in
      native) dotnet restore SIR.slnx --locked-mode ;;
      fable)
        dotnet restore tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj --locked-mode
        dotnet restore tests/SIR.ModalInput.Fable.Tests/SIR.ModalInput.Fable.Tests.fsproj --locked-mode
        dotnet restore tests/SIR.Client.Tests/ScenarioCatalogRuntime.fsproj --locked-mode
        ;;
      web)
        dotnet restore src/SIR.Replay.Web/SIR.Replay.Web.fsproj --locked-mode
        dotnet restore src/SIR.Client.Web/SIR.RulesExplorer.Web.fsproj --locked-mode
        ;;
      server) dotnet restore src/SIR.Server/SIR.Server.fsproj --locked-mode ;;
      docs)
        dotnet restore src/SIR.Match/SIR.Match.fsproj --locked-mode
        dotnet restore src/SIR.Client/SIR.Client.fsproj --locked-mode
        ;;
    esac
    restore_completed=$(date +%s%3N)
    failure_stage=build
    build_started=$(date +%s%3N)
    case "$part" in
      native)
        # One Release solution graph is the native producer boundary. It compiles
        # every declared project once and lets later solution growth remain one
        # invocation instead of accumulating serialized owner builds.
        dotnet build SIR.slnx -c Release --no-restore
        mkdir -p artifacts/publish
        cp -a src/SIR.Server/bin/Release/net10.0/. artifacts/publish/
        node scripts/prune-linux-runtime-closure.mjs \
          --root artifacts/publish \
          --output "$part_root/native-server-runtime-closure.json"
        node scripts/prune-linux-runtime-closure.mjs \
          --root tests/SIR.Match.Tests/bin/Release/net10.0 \
          --output "$part_root/native-runtime-closure.json"
        part_paths=(
          tests/SIR.Domain.Tests/bin/Release/net10.0
          tests/SIR.Rules.Governance.Tests/bin/Release/net10.0
          src/SIR.Simulation/Governance.Tool/bin/Release/net10.0
          tests/SIR.Client.Tests/bin/Release/net10.0
          tests/SIR.Client.Tests/bin/ScenarioCatalogRuntime/Release/net10.0
          tests/SIR.ModalInput.Tests/bin/Release/net10.0
          tests/SIR.Match.Tests/bin/Release/net10.0
          artifacts/publish
          src/SIR.Domain/bin/Release/net10.0
          src/SIR.Simulation/bin/Release/net10.0
          src/SIR.Wasm/bin/Release/net10.0
          src/SIR.Match/bin/Release/net10.0
          src/SIR.Client/bin/Release/net10.0
        )
        ;;
      fable)
        dotnet fable tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj --outDir "$ci_root/prepared/domain-fable" --noCache >"$part_root/domain-fable.log" 2>&1 &
        domain_pid=$!
        dotnet fable tests/SIR.ModalInput.Fable.Tests/SIR.ModalInput.Fable.Tests.fsproj --outDir "$ci_root/prepared/modal-fable" --noCache >"$part_root/modal-fable.log" 2>&1 &
        modal_pid=$!
        dotnet fable tests/SIR.Client.Tests/ScenarioCatalogRuntime.fsproj --outDir "$ci_root/prepared/scenario-catalog-fable" --noCache >"$part_root/scenario-catalog-fable.log" 2>&1 &
        scenario_catalog_pid=$!
        part_failed=0
        wait "$domain_pid" || part_failed=1
        wait "$modal_pid" || part_failed=1
        wait "$scenario_catalog_pid" || part_failed=1
        sed -n '1,240p' "$part_root/domain-fable.log"
        sed -n '1,240p' "$part_root/modal-fable.log"
        sed -n '1,240p' "$part_root/scenario-catalog-fable.log"
        [[ $part_failed -eq 0 ]] || { echo "qualify-pr: Fable prepare part failed" >&2; exit 1; }
        part_paths=(artifacts/ci/prepared/domain-fable artifacts/ci/prepared/modal-fable artifacts/ci/prepared/scenario-catalog-fable)
        ;;
      web)
        ./scripts/build-client.sh
        # Browser consumers use the exact Playwright runtime bytes installed from
        # package-lock.json by this producer. Shipping the three-package runtime
        # avoids repeating a full npm ci on every browser shard while the build
        # receipt and artifact manifest still bind every transported byte.
        part_paths=(
          src/SIR.Client.Web/.fable
          src/SIR.Client.Web/.fable-rules
          artifacts/client
          node_modules/@playwright/test
          node_modules/playwright
          node_modules/playwright-core
        )
        ;;
      server)
        dotnet publish src/SIR.Server/SIR.Server.fsproj -c Release -o artifacts/publish --no-restore
        node scripts/prune-linux-runtime-closure.mjs \
          --root artifacts/publish \
          --output "$part_root/server-runtime-closure.json"
        part_paths=(artifacts/publish)
        ;;
      docs)
        dotnet build src/SIR.Match/SIR.Match.fsproj -c Release --no-restore
        dotnet build src/SIR.Client/SIR.Client.fsproj -c Release --no-restore
        ./scripts/build-docs.sh --prepare-site-only
        part_paths=(artifacts/site)
        ;;
    esac
    build_completed=$(date +%s%3N)
    [[ ${#part_paths[@]} -gt 0 ]] || { echo "qualify-pr: prepare part has no completed outputs: $part" >&2; exit 1; }
    failure_stage=transport
    output_args=()
    output_index=0
    for path in "${part_paths[@]}"; do
      output_args+=(--output "$part-$output_index=$path")
      output_index=$((output_index + 1))
    done
    node scripts/production-build-receipt.mjs create \
      --owner-command scripts/qualify-pr.sh \
      --input src --input scripts --input tests --input .github/workflows/ci.yml \
      --input package.json --input package-lock.json --input global.json --input .config/dotnet-tools.json \
      --input Directory.Build.props --input Directory.Packages.props --input SIR.slnx \
      "${output_args[@]}" --receipt-directory "$part_root/receipts" --pointer "$part_root/$part.receipt.path"
    receipt=$(<"$part_root/$part.receipt.path")
    node scripts/ci-artifact-manifest.mjs pack \
      --build-receipt "$receipt" \
      --store "$part_root/content-store-$part" \
      --archive "$part_root/$part.tar" \
      --content-index "$part_root/$part.tar.index.json"
    node scripts/ci-artifact-manifest.mjs create --route "$route_path" --build-receipt "$receipt" \
      --archive "$part_root/$part.tar" --content-index "$part_root/$part.tar.index.json" \
      --directory "$part_root/manifests" --pointer "$part_root/$part.manifest.path"
    failure_stage=null
    ;;
  extract-parts)
    extract_started=$(date +%s%3N)
    shift_status=0
    [[ $# -gt 0 ]] || { echo "qualify-pr: extract-parts requires at least one producer" >&2; exit 2; }
    for part in "$@"; do
      [[ -f "$ci_root/parts/$part.tar" && -f "$ci_root/parts/$part.manifest.path" && -f "$ci_root/parts/$part.receipt.path" ]] || { echo "qualify-pr: missing prepared part:$part" >&2; exit 1; }
      manifest=$(<"$ci_root/parts/$part.manifest.path")
      node scripts/ci-artifact-manifest.mjs verify-transport --route "$route_path" --archive "$ci_root/parts/$part.tar" --manifest "$manifest"
      receipt=$(<"$ci_root/parts/$part.receipt.path")
      store="$ci_root/staging/$part-store"
      stage="$ci_root/staging/$part"
      rm -rf -- "$store" "$stage"
      mkdir -p "$store"
      tar -xf "$ci_root/parts/$part.tar" -C "$store"
      node scripts/ci-artifact-manifest.mjs reconstruct --manifest "$manifest" --store "$store" --destination "$stage" >/dev/null
      node scripts/ci-artifact-manifest.mjs verify-staged --root "$repo_root" --build-receipt "$receipt" --manifest "$manifest" --stage "$stage" >/dev/null
      while IFS= read -r output_path; do
        target="$repo_root/$output_path"
        rm -rf -- "$target"
        mkdir -p "$(dirname "$target")"
        cp -a "$stage/$output_path" "$target"
      done < <(jq -r '.outputs[].path' "$receipt")
    done
    extract_completed=$(date +%s%3N)
      extract_timing_path=${SIR_CI_EXTRACT_TIMING_PATH:-$ci_root/extract-timing.json}
      node - "$extract_timing_path" "$extract_started" "$extract_completed" <<'NODE'
const { writeFileSync } = require("node:fs");
writeFileSync(process.argv[2], `${JSON.stringify({ transport: Number(process.argv[4]) - Number(process.argv[3]) })}\n`);
NODE
    ;;
  verify-parts)
    for part in "$@"; do
      receipt=$(<"$ci_root/parts/$part.receipt.path")
      manifest=$(<"$ci_root/parts/$part.manifest.path")
      node scripts/ci-artifact-manifest.mjs verify-transport --route "$route_path" --archive "$ci_root/parts/$part.tar" --manifest "$manifest" >/dev/null
      node scripts/ci-artifact-manifest.mjs verify-staged --root "$repo_root" --build-receipt "$receipt" --manifest "$manifest" --stage "$ci_root/staging/$part"
    done
    ;;
  compose-browser)
    web_manifest=$(<"$ci_root/parts/web.manifest.path")
    server_manifest=$(<"$ci_root/parts/native.manifest.path")
    rm -rf -- artifacts/publish/wwwroot
    mkdir -p artifacts/publish/wwwroot
    cp -a artifacts/client/. artifacts/publish/wwwroot/
    node scripts/ci-artifact-manifest.mjs create-browser-composition \
      --web-manifest "$web_manifest" --server-manifest "$server_manifest" \
      --client artifacts/client --publish artifacts/publish --output "$ci_root/browser-composition.json"
    ;;
  verify-browser-composition)
    web_manifest=$(<"$ci_root/parts/web.manifest.path")
    server_manifest=$(<"$ci_root/parts/native.manifest.path")
    node scripts/ci-artifact-manifest.mjs verify-browser-composition \
      --web-manifest "$web_manifest" --server-manifest "$server_manifest" \
      --client artifacts/client --publish artifacts/publish --output "$ci_root/browser-composition.json"
    ;;
  gate)
    gate=${1:?qualify-pr gate requires a gate id}
    gate_parts=()
    case "$gate" in
      rules) gate_parts=(native) ;;
      spatial|cross-runtime) gate_parts=(native fable) ;;
      cancellation) gate_parts=(native web) ;;
      browser|browser-general-helper|browser-delivery) gate_parts=(web native) ;;
      documentation) gate_parts=(web docs) ;;
      collection-strategies) gate_parts=(native) ;;
    esac
    if [[ ${#gate_parts[@]} -gt 0 ]]; then
      receipt_part=${gate_parts[0]}
      [[ "$gate" == documentation ]] && receipt_part=docs
      receipt=$(<"$ci_root/parts/$receipt_part.receipt.path")
      "$0" verify-parts "${gate_parts[@]}" >/dev/null
    fi
    case "$gate" in
      spatial-mutations)
        restore_started=$(date +%s%3N)
        set +e
        dotnet restore tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj --locked-mode
        spatial_mutation_status=$?
        set -e
        restore_completed=$(date +%s%3N)
        build_started=$restore_completed
        failure_stage=restore
        if [[ $spatial_mutation_status -eq 0 ]]; then
          failure_stage=build
          set +e
          SIR_BUILD_EXCEPTION=spatial-mutation-base dotnet build tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-restore
          spatial_mutation_status=$?
          if [[ $spatial_mutation_status -eq 0 ]]; then
            ./scripts/test-spatial-subject-mutations.sh --prepared-pr
            spatial_mutation_status=$?
          fi
          set -e
        fi
        build_completed=$(date +%s%3N)
        [[ $spatial_mutation_status -eq 0 ]] && failure_stage=null
        node - "$phase_path" "$restore_started" "$restore_completed" "$build_started" "$build_completed" "$failure_stage" <<'NODE'
const { writeFileSync } = require("node:fs");
const [path, restoreStarted, restoreCompleted, buildStarted, buildCompleted, failureStage] = process.argv.slice(2);
writeFileSync(path, `${JSON.stringify({ restore: Number(restoreCompleted) - Number(restoreStarted), build: Number(buildCompleted) - Number(buildStarted), transport: 0, test: 0, failureStage: failureStage === "null" ? null : failureStage })}\n`);
NODE
        exit "$spatial_mutation_status"
        ;;
      cancellation-mutations) ./scripts/test-worker-cancellation-subject-mutation.sh --mutation-only ;;
      rules)
        SIR_RULES_PREPARED_PR=1 ./scripts/verify-rules-corpus.sh
        SIR_RULES_PREPARED_PR=1 dotnet run --project tests/SIR.Rules.Governance.Tests/SIR.Rules.Governance.Tests.fsproj -c Release --no-build --no-restore
        SIR_RULES_PREPARED_PR=1 ./scripts/test-rules-governance-tool-mutations.sh
        SIR_RULES_PREPARED_PR=1 ./scripts/generate-rules-governance.sh --check
        ;;
      spatial) ./scripts/verify-spatial-query.sh --reuse-pr-build-receipt "$receipt" --prepared-fable "$ci_root/prepared/domain-fable" --prepared-pr --prepared-parts-verified --external-mutation-proof ;;
      cancellation) node ./scripts/smoke-worker-roundtrip.mjs ;;
      cross-runtime)
        ./scripts/test-conformance.sh \
          --reuse-pr-build-receipt "$receipt" \
          --prepared-fable "$ci_root/prepared/domain-fable" "$ci_root/prepared/modal-fable" "$ci_root/prepared/scenario-catalog-fable" \
          --domain-only \
          --ordinary-pr-functional
        ;;
      browser)
        "$0" compose-browser >/dev/null
        run_browser_shard_pair 1 2
        ;;
      browser-general-helper)
        "$0" compose-browser >/dev/null
        run_browser_shard_pair 3 4
        ;;
      browser-delivery)
        "$0" compose-browser >/dev/null
        SIR_BROWSER_SHARDS=1 SIR_BROWSER_COHORT=production-delivery npm run test:browser
        ;;
      documentation)
        ./scripts/build-docs.sh --reuse-build-receipt "$receipt" --reuse-build-owner scripts/qualify-pr.sh --prepared-pr --reuse-site-build
        ;;
      # S.I.R.#263. The collection-strategy regression gate. ONE statement, and its status is this
      # arm's status: the script ends with the harness's own exit code and carries no `|| true`, so
      # a red benchmark becomes `status: fail` in the sir.ci-gate-result/v1 receipt pr-verdict
      # joins rather than a FAILED line inside a green log (the escape S.I.R.#265 measured).
      collection-strategies) ./scripts/verify-collection-strategies.sh ;;
      evidence)
        # Every routed work item gets exactly one recorded outcome, whether or not it was checked, so
        # that "checked and passed" and "not checked" can never again be the same CI result (#272).
        evidence_coverage_path="$ci_root/results/evidence-coverage.json"
        evidence_coverage_reached=false
        evidence_coverage_rows=()
        rm -f -- "$evidence_coverage_path"
        record_evidence_coverage() {
          evidence_coverage_rows+=("$1"$'\t'"$2"$'\t'"$3"$'\t'"$4"$'\t'"$5")
          echo "qualify-pr: evidence coverage $2: work/$1 — $5"
        }
        write_evidence_coverage() {
          mkdir -p "$(dirname "$evidence_coverage_path")"
          node - "$evidence_coverage_path" "$evidence_coverage_reached" \
            ${evidence_coverage_rows[@]+"${evidence_coverage_rows[@]}"} <<'NODE'
const { writeFileSync } = require("node:fs");
const [path, reached, ...rows] = process.argv.slice(2);
const items = rows.map((row) => {
  const [workId, outcome, checked, fatal, detail] = row.split("\t");
  return { workId, outcome, checked: checked === "true", fatal: fatal === "true", detail };
});
writeFileSync(path, `${JSON.stringify({
  schema: "sir.ci.evidence-coverage/v1",
  reachedWorkItems: reached === "true",
  routedWorkItems: items.length,
  checked: items.filter((item) => item.checked).length,
  notChecked: items.filter((item) => !item.checked).length,
  items
}, null, 2)}\n`);
NODE
        }
        restore_started=$(date +%s%3N)
        set +e
        dotnet restore SIR.slnx --locked-mode
        evidence_status=$?
        set -e
        restore_completed=$(date +%s%3N)
        failure_stage=restore
        test_started=$(date +%s%3N)
        if [[ $evidence_status -eq 0 ]]; then
          failure_stage=test
          set +e
          npm run verify:scaffold
          evidence_status=$?
          if [[ $evidence_status -eq 0 ]]; then
            mapfile -t work_ids < <(node - "$route_path" <<'NODE'
const { readFileSync } = require("node:fs");
const route = JSON.parse(readFileSync(process.argv[2], "utf8"));
const ids = new Set(route.paths.map((path) => /^work\/([^/]+)\//u.exec(path)?.[1]).filter(Boolean));
for (const id of [...ids].sort()) console.log(id);
NODE
            )
            mapfile -t routed_hosted_verification_ids < <(node - "$route_path" <<'NODE'
const { readFileSync } = require("node:fs");
const route = JSON.parse(readFileSync(process.argv[2], "utf8"));
const ids = new Set(route.paths.map((path) => /^work\/([^/]+)\/hosted-verification\.sh$/u.exec(path)?.[1]).filter(Boolean));
for (const id of [...ids].sort()) console.log(id);
NODE
            )
            declare -A routed_hosted_verification=()
            for work_id in "${routed_hosted_verification_ids[@]}"; do
              routed_hosted_verification["$work_id"]=true
            done
            evidence_coverage_reached=true
            # A routed work item is never skipped in silence. Absence is classified, reported on the
            # gate's output, and recorded in artifacts/ci/results/evidence-coverage.json. Absence is
            # fatal when the package still declares completed implementation, which is the state
            # `fsgg-sdd verify` itself blocks on, AND when whether it does so cannot be determined at
            # all; a package that has genuinely not reached the evidence stage passes with its
            # non-coverage recorded rather than failed outright. Those three outcomes are distinct
            # rows in the coverage artifact, so "not checked", "checked and passed" and "could not be
            # evaluated" are never the same CI result (#272).
            for work_id in "${work_ids[@]}"; do
              if [[ $evidence_status -ne 0 ]]; then
                record_evidence_coverage "$work_id" not-reached false false \
                  "an earlier routed work item failed, so this item was never checked"
                continue
              fi
              if [[ ! -d "work/$work_id" ]]; then
                record_evidence_coverage "$work_id" work-package-removed false false \
                  "the routed work package directory is absent in this tree"
                continue
              fi
              if [[ ! -f "work/$work_id/evidence.yml" ]]; then
                evidence_owed=0
                work_declares_completed_implementation "$work_id" || evidence_owed=$?
                case $evidence_owed in
                  0)
                    echo "qualify-pr: routed evidence declaration is missing while tasks still declare completed implementation: work/$work_id/evidence.yml" >&2
                    record_evidence_coverage "$work_id" not-evidenced false true \
                      "evidence.yml is absent while work/$work_id/tasks.yml still declares completed implementation"
                    evidence_status=1
                    ;;
                  1)
                    record_evidence_coverage "$work_id" not-evidenced false false \
                      "evidence.yml is absent and work/$work_id/tasks.yml declares no completed implementation, so evidence is not yet owed"
                    ;;
                  *)
                    echo "qualify-pr: cannot determine whether work/$work_id owes evidence: fsgg-sdd reported no task count for work/$work_id/tasks.yml" >&2
                    record_evidence_coverage "$work_id" evidence-owed-indeterminate false true \
                      "evidence.yml is absent and whether work/$work_id/tasks.yml declares completed implementation could not be determined, so the gate refuses rather than reporting that evidence is not owed"
                    evidence_status=1
                    ;;
                esac
                continue
              fi
              dotnet fsgg-sdd verify --work "$work_id" --root . --text
              evidence_status=$?
              if [[ $evidence_status -ne 0 ]]; then
                record_evidence_coverage "$work_id" failed true true \
                  "fsgg-sdd verify exited $evidence_status"
                continue
              fi
              hosted_verification="work/$work_id/hosted-verification.sh"
              if [[ -e "$hosted_verification" || -L "$hosted_verification" || ${routed_hosted_verification[$work_id]:-false} == true ]]; then
                if [[ ! -f "$hosted_verification" ]]; then
                  echo "qualify-pr: routed hosted verification is missing or not a regular file: $hosted_verification" >&2
                  evidence_status=1
                  record_evidence_coverage "$work_id" failed true true \
                    "routed hosted verification is missing or not a regular file"
                  continue
                fi
                if [[ ! -r "$hosted_verification" ]]; then
                  echo "qualify-pr: routed hosted verification is not readable: $hosted_verification" >&2
                  evidence_status=1
                  record_evidence_coverage "$work_id" failed true true \
                    "routed hosted verification is not readable"
                  continue
                fi
                if [[ ! -x "$hosted_verification" ]]; then
                  echo "qualify-pr: routed hosted verification is not executable: $hosted_verification" >&2
                  evidence_status=1
                  record_evidence_coverage "$work_id" failed true true \
                    "routed hosted verification is not executable"
                  continue
                fi
                "$hosted_verification"
                evidence_status=$?
                if [[ $evidence_status -ne 0 ]]; then
                  record_evidence_coverage "$work_id" failed true true \
                    "hosted verification exited $evidence_status"
                  continue
                fi
              fi
              record_evidence_coverage "$work_id" verified true false \
                "fsgg-sdd verify passed and every routed hosted verification ran"
            done
          fi
          set -e
        fi
        test_completed=$(date +%s%3N)
        [[ $evidence_status -eq 0 ]] && failure_stage=null
        write_evidence_coverage
        node - "$phase_path" "$restore_started" "$restore_completed" "$test_started" "$test_completed" "$failure_stage" <<'NODE'
const { writeFileSync } = require("node:fs");
const [path, restoreStarted, restoreCompleted, testStarted, testCompleted, failureStage] = process.argv.slice(2);
writeFileSync(path, `${JSON.stringify({ restore: Number(restoreCompleted) - Number(restoreStarted), build: 0, transport: 0, test: Number(testCompleted) - Number(testStarted), failureStage: failureStage === "null" ? null : failureStage })}\n`);
NODE
        exit "$evidence_status"
        ;;
      *) echo "qualify-pr: unknown gate: $gate" >&2; exit 2 ;;
    esac
    if [[ ${#gate_parts[@]} -gt 0 ]]; then
      "$0" verify-parts "${gate_parts[@]}" >/dev/null
    fi
    if [[ "$gate" == browser || "$gate" == browser-general-helper || "$gate" == browser-delivery ]]; then "$0" verify-browser-composition >/dev/null; fi
    ;;
  *)
    echo "qualify-pr: usage route PATHS|integrity|prepare-part ID|extract-parts IDS...|verify-parts IDS...|compose-browser|verify-browser-composition|gate ID" >&2
    exit 2
    ;;
esac
