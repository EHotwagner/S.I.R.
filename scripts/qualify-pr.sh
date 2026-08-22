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
    dotnet tool restore
    dotnet restore SIR.slnx --locked-mode
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
      spatial) ./scripts/verify-spatial-query.sh --reuse-pr-build-receipt "$receipt" --prepared-fable "$ci_root/prepared/domain-fable" --prepared-pr --external-mutation-proof ;;
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
      evidence)
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
            for work_id in "${work_ids[@]}"; do
              [[ -f "work/$work_id/evidence.yml" ]] || continue
              dotnet fsgg-sdd verify --work "$work_id" --root . --text
              evidence_status=$?
              [[ $evidence_status -eq 0 ]] || break
              hosted_verification="work/$work_id/hosted-verification.sh"
              if [[ -e "$hosted_verification" || -L "$hosted_verification" || ${routed_hosted_verification[$work_id]:-false} == true ]]; then
                if [[ ! -f "$hosted_verification" ]]; then
                  echo "qualify-pr: routed hosted verification is missing or not a regular file: $hosted_verification" >&2
                  evidence_status=1
                  break
                fi
                if [[ ! -r "$hosted_verification" ]]; then
                  echo "qualify-pr: routed hosted verification is not readable: $hosted_verification" >&2
                  evidence_status=1
                  break
                fi
                if [[ ! -x "$hosted_verification" ]]; then
                  echo "qualify-pr: routed hosted verification is not executable: $hosted_verification" >&2
                  evidence_status=1
                  break
                fi
                "$hosted_verification"
                evidence_status=$?
                [[ $evidence_status -eq 0 ]] || break
              fi
            done
          fi
          set -e
        fi
        test_completed=$(date +%s%3N)
        [[ $evidence_status -eq 0 ]] && failure_stage=null
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
