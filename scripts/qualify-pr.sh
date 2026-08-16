#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
mode=${1:-}
shift || true
ci_root="$repo_root/artifacts/ci"
route_path=${SIR_CI_ROUTE:-$ci_root/route.json}
pointer="$ci_root/build-receipt.path"
manifest_pointer="$ci_root/artifact-manifest.path"
archive_path="$repo_root/artifacts/prepared-candidate.tar"
phase_path="$ci_root/phase-timing.json"

mkdir -p "$ci_root/results" "$ci_root/prepared"
cd "$repo_root"

case "$mode" in
  route)
    paths_file=${1:?qualify-pr route requires a changed-path file}
    commit=$(git rev-parse HEAD)
    tree=$(git rev-parse "${commit}^{tree}")
    node scripts/ci-route.mjs route --paths-file "$paths_file" --commit "$commit" --tree "$tree" --output "$route_path"
    ;;
  integrity)
    node scripts/test-ci-route.mjs
    ./scripts/test-ci-evidence-mutation.sh
    ./scripts/test-ci-failure-timing-mutation.sh
    node .github/scripts/test-npm-audit.mjs
    node .github/scripts/check-npm-audit.mjs
    ./scripts/verify-fable-game-governance.sh
    dotnet fsgg-sdd dependency-surface --check --param packageId=FS.GG.Game.Core --param version=0.13.0 --root . --text
    ;;
  prepare-part)
    part=${1:?qualify-pr prepare-part requires native|fable|web|server}
    case "$part" in
      native|fable|web|server) ;;
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
      exit "$part_status"
    }
    trap write_part_timing EXIT
    dotnet tool restore
    dotnet restore SIR.slnx --locked-mode
    restore_completed=$(date +%s%3N)
    failure_stage=build
    build_started=$(date +%s%3N)
    case "$part" in
      native)
        dotnet build SIR.slnx --no-restore >"$part_root/solution-debug.log" 2>&1 &
        solution_pid=$!
        dotnet build tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-restore >"$part_root/domain-release.log" 2>&1 &
        release_pid=$!
        part_failed=0
        wait "$solution_pid" || part_failed=1
        wait "$release_pid" || part_failed=1
        sed -n '1,240p' "$part_root/solution-debug.log"
        sed -n '1,240p' "$part_root/domain-release.log"
        [[ $part_failed -eq 0 ]] || { echo "qualify-pr: native prepare part failed" >&2; exit 1; }
        mapfile -t part_paths < <(find src tests -type d \( -name bin -o -name obj \) -prune -print | LC_ALL=C sort)
        ;;
      fable)
        dotnet fable tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj --outDir "$ci_root/prepared/domain-fable" --noCache >"$part_root/domain-fable.log" 2>&1 &
        domain_pid=$!
        dotnet fable tests/SIR.ModalInput.Fable.Tests/SIR.ModalInput.Fable.Tests.fsproj --outDir "$ci_root/prepared/modal-fable" --noCache >"$part_root/modal-fable.log" 2>&1 &
        modal_pid=$!
        part_failed=0
        wait "$domain_pid" || part_failed=1
        wait "$modal_pid" || part_failed=1
        sed -n '1,240p' "$part_root/domain-fable.log"
        sed -n '1,240p' "$part_root/modal-fable.log"
        [[ $part_failed -eq 0 ]] || { echo "qualify-pr: Fable prepare part failed" >&2; exit 1; }
        part_paths=(artifacts/ci/prepared/domain-fable artifacts/ci/prepared/modal-fable)
        ;;
      web)
        ./scripts/build-client.sh
        part_paths=(src/SIR.Client.Web/.fable src/SIR.Client.Web/.fable-rules artifacts/client)
        ;;
      server)
        dotnet publish src/SIR.Server/SIR.Server.fsproj -c Release -o artifacts/publish --no-restore
        part_paths=(artifacts/publish)
        ;;
    esac
    build_completed=$(date +%s%3N)
    [[ ${#part_paths[@]} -gt 0 ]] || { echo "qualify-pr: prepare part has no completed outputs: $part" >&2; exit 1; }
    failure_stage=transport
    tar --sort=name --mtime=@0 --owner=0 --group=0 --numeric-owner -cf "$part_root/$part.tar" "${part_paths[@]}"
    failure_stage=null
    ;;
  prepare)
    classification=$(jq -r '.classification' "$route_path")
    final_started=$(date +%s%3N)
    node - "$phase_path" "$final_started" <<'NODE'
const { writeFileSync } = require("node:fs");
writeFileSync(process.argv[2], `${JSON.stringify({ startedAtMilliseconds: Number(process.argv[3]), setup: 0, restore: 0, build: 0, transport: 0, failureStage: "transport" })}\n`);
NODE
    required_parts=()
    outputs=()
    if [[ "$classification" == domain || "$classification" == cross-cutting ]]; then
      required_parts+=(native fable)
      outputs+=(
        --output domain-tests=tests/SIR.Domain.Tests/bin
        --output client-tests=tests/SIR.Client.Tests/bin
        --output modal-tests=tests/SIR.ModalInput.Tests/bin
        --output match-tests=tests/SIR.Match.Tests/bin
        --output domain-fable=artifacts/ci/prepared/domain-fable
        --output modal-fable=artifacts/ci/prepared/modal-fable
      )
    fi
    if [[ "$classification" == documentation || "$classification" == browser || "$classification" == cross-cutting ]]; then
      required_parts+=(web)
      outputs+=(--output main-fable=src/SIR.Client.Web/.fable --output rules-fable=src/SIR.Client.Web/.fable-rules --output production-client=artifacts/client)
    fi
    if [[ "$classification" == browser || "$classification" == cross-cutting ]]; then
      required_parts+=(server)
      outputs+=(--output server=artifacts/publish)
    fi
    [[ ${#outputs[@]} -gt 0 ]] || { echo "qualify-pr: prepare has no build outputs for $classification" >&2; exit 2; }
    for part in "${required_parts[@]}"; do
      [[ -f "$ci_root/parts/$part.tar" && -f "$ci_root/parts/$part.timing.json" ]] || { echo "qualify-pr: missing prepared part:$part" >&2; exit 1; }
      tar -xf "$ci_root/parts/$part.tar"
    done
    obj_index=0
    while IFS= read -r directory; do
      outputs+=(--output "obj-$obj_index=$directory")
      obj_index=$((obj_index + 1))
    done < <(find src tests -type d -name obj -prune -print | LC_ALL=C sort)
    node scripts/production-build-receipt.mjs create \
      --owner-command scripts/qualify-pr.sh \
      --input src --input scripts --input tests --input .github/workflows/ci.yml \
      --input package.json --input package-lock.json --input global.json --input .config/dotnet-tools.json \
      --input Directory.Build.props --input Directory.Packages.props --input SIR.slnx \
      "${outputs[@]}" \
      --pointer "$pointer"
    receipt=$(<"$pointer")
    mapfile -t transport_paths < <(node - "$receipt" <<'NODE'
const { readFileSync } = require("node:fs");
const receipt = JSON.parse(readFileSync(process.argv[2], "utf8"));
for (const output of receipt.outputs) console.log(output.path);
NODE
    )
    rm -f -- "$archive_path"
    tar --sort=name --mtime=@0 --owner=0 --group=0 --numeric-owner -cf "$archive_path" "${transport_paths[@]}" "${pointer#$repo_root/}" "$receipt"
    node scripts/ci-artifact-manifest.mjs create --route "$route_path" --build-receipt "$receipt" --archive "$archive_path" --pointer "$manifest_pointer"
    final_completed=$(date +%s%3N)
    node - "$phase_path" "$final_started" "$final_completed" "${required_parts[@]/#/$ci_root/parts/}" <<'NODE'
const { writeFileSync } = require("node:fs");
const { readFileSync } = require("node:fs");
const [path, finalStartedText, finalCompletedText, ...partPrefixes] = process.argv.slice(2);
const parts = partPrefixes.map((prefix) => JSON.parse(readFileSync(`${prefix}.timing.json`, "utf8")));
const critical = parts.toSorted((left, right) => (right.completedAtMilliseconds - right.startedAtMilliseconds) - (left.completedAtMilliseconds - left.startedAtMilliseconds))[0];
const finalStarted = Number(finalStartedText);
const finalCompleted = Number(finalCompletedText);
writeFileSync(path, `${JSON.stringify({ startedAtMilliseconds: Math.min(...parts.map((part) => part.startedAtMilliseconds)), setup: critical.setup, restore: critical.restore, build: critical.build, transport: critical.transport + finalCompleted - finalStarted, failureStage: null })}\n`);
NODE
    ;;
  extract-prepared)
    extract_started=$(date +%s%3N)
    manifest=$(<"$manifest_pointer")
    node scripts/ci-artifact-manifest.mjs verify-transport --route "$route_path" --archive "$archive_path" --manifest "$manifest"
    tar -xf "$archive_path"
    extract_completed=$(date +%s%3N)
    node - "$ci_root/extract-timing.json" "$extract_started" "$extract_completed" <<'NODE'
const { writeFileSync } = require("node:fs");
writeFileSync(process.argv[2], `${JSON.stringify({ transport: Number(process.argv[4]) - Number(process.argv[3]) })}\n`);
NODE
    ;;
  verify-prepared)
    receipt=$(<"$pointer")
    manifest=$(<"$manifest_pointer")
    node scripts/ci-artifact-manifest.mjs verify --route "$route_path" --build-receipt "$receipt" --archive "$archive_path" --manifest "$manifest"
    ;;
  gate)
    gate=${1:?qualify-pr gate requires a gate id}
    if [[ "$gate" != evidence ]]; then
      receipt=$(<"$pointer")
      "$0" verify-prepared >/dev/null
    fi
    case "$gate" in
      rules) ./scripts/verify-rules-corpus.sh ;;
      spatial) ./scripts/verify-spatial-query.sh --reuse-pr-build-receipt "$receipt" --prepared-fable "$ci_root/prepared/domain-fable" ;;
      cancellation) ./scripts/test-worker-cancellation-subject-mutation.sh ;;
      cross-runtime)
        ./scripts/test-conformance.sh \
          --reuse-pr-build-receipt "$receipt" \
          --prepared-fable "$ci_root/prepared/domain-fable" "$ci_root/prepared/modal-fable" \
          --domain-only
        ;;
      browser)
        npm run test:browser
        ;;
      documentation)
        ./scripts/build-docs.sh --reuse-build-receipt "$receipt" --reuse-build-owner scripts/qualify-pr.sh --prepared-pr
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
            for work_id in "${work_ids[@]}"; do
              [[ -f "work/$work_id/evidence.yml" ]] || continue
              dotnet fsgg-sdd verify --work "$work_id" --root . --text
              evidence_status=$?
              [[ $evidence_status -eq 0 ]] || break
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
    if [[ "$gate" != evidence ]]; then
      "$0" verify-prepared >/dev/null
    fi
    ;;
  *)
    echo "qualify-pr: usage route PATHS|integrity|prepare-part ID|prepare|extract-prepared|verify-prepared|gate ID" >&2
    exit 2
    ;;
esac
