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
    node .github/scripts/test-npm-audit.mjs
    node .github/scripts/check-npm-audit.mjs
    ./scripts/verify-fable-game-governance.sh
    dotnet fsgg-sdd dependency-surface --check --param packageId=FS.GG.Game.Core --param version=0.13.0 --root . --text
    ;;
  prepare)
    restore_started=$(date +%s%3N)
    dotnet tool restore
    dotnet restore SIR.slnx --locked-mode
    restore_completed=$(date +%s%3N)
    classification=$(jq -r '.classification' "$route_path")
    outputs=()
    transport_paths=()
    task_names=()
    task_pids=()
    start_task() {
      local name=$1
      shift
      "$@" >"$ci_root/$name.log" 2>&1 &
      task_names+=("$name")
      task_pids+=("$!")
    }
    build_started=$(date +%s%3N)
    if [[ "$classification" == domain || "$classification" == cross-cutting ]]; then
      start_task solution-debug dotnet build SIR.slnx --no-restore
      start_task domain-release dotnet build tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-restore
      start_task domain-fable dotnet fable tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj --outDir "$ci_root/prepared/domain-fable" --noCache
      start_task modal-fable dotnet fable tests/SIR.ModalInput.Fable.Tests/SIR.ModalInput.Fable.Tests.fsproj --outDir "$ci_root/prepared/modal-fable" --noCache
      outputs+=(
        --output domain-tests=tests/SIR.Domain.Tests/bin
        --output client-tests=tests/SIR.Client.Tests/bin
        --output modal-tests=tests/SIR.ModalInput.Tests/bin
        --output match-tests=tests/SIR.Match.Tests/bin
        --output domain-fable=artifacts/ci/prepared/domain-fable
        --output modal-fable=artifacts/ci/prepared/modal-fable
      )
      while IFS= read -r directory; do transport_paths+=("$directory"); done < <(find src tests -type d \( -name bin -o -name obj \) -prune -print | LC_ALL=C sort)
      transport_paths+=(artifacts/ci/prepared/domain-fable artifacts/ci/prepared/modal-fable)
    fi
    if [[ "$classification" == documentation || "$classification" == browser || "$classification" == cross-cutting ]]; then
      start_task production-client ./scripts/build-client.sh
      outputs+=(--output main-fable=src/SIR.Client.Web/.fable --output rules-fable=src/SIR.Client.Web/.fable-rules --output production-client=artifacts/client)
      transport_paths+=(src/SIR.Client.Web/.fable src/SIR.Client.Web/.fable-rules artifacts/client)
    fi
    if [[ "$classification" == browser || "$classification" == cross-cutting ]]; then
      start_task server-publish dotnet publish src/SIR.Server/SIR.Server.fsproj -c Release -o artifacts/publish --no-restore
      outputs+=(--output server=artifacts/publish)
      transport_paths+=(artifacts/publish)
    fi
    task_failed=0
    for index in "${!task_pids[@]}"; do
      if ! wait "${task_pids[$index]}"; then task_failed=1; fi
      sed -n '1,240p' "$ci_root/${task_names[$index]}.log"
    done
    [[ $task_failed -eq 0 ]] || { echo "qualify-pr: one or more parallel prepare tasks failed" >&2; exit 1; }
    build_completed=$(date +%s%3N)
    [[ ${#outputs[@]} -gt 0 ]] || { echo "qualify-pr: prepare has no build outputs for $classification" >&2; exit 2; }
    node scripts/production-build-receipt.mjs create \
      --owner-command scripts/qualify-pr.sh \
      --input src --input scripts --input tests --input .github/workflows/ci.yml \
      --input package.json --input package-lock.json --input global.json --input .config/dotnet-tools.json \
      --input Directory.Build.props --input Directory.Packages.props --input SIR.slnx \
      "${outputs[@]}" \
      --pointer "$pointer"
    receipt=$(<"$pointer")
    rm -f -- "$archive_path"
    tar --sort=name --mtime=@0 --owner=0 --group=0 --numeric-owner -cf "$archive_path" "${transport_paths[@]}" "${pointer#$repo_root/}" "$receipt"
    node scripts/ci-artifact-manifest.mjs create --route "$route_path" --build-receipt "$receipt" --archive "$archive_path" --pointer "$manifest_pointer"
    node - "$phase_path" "$restore_started" "$restore_completed" "$build_started" "$build_completed" <<'NODE'
const { writeFileSync } = require("node:fs");
const [path, restoreStarted, restoreCompleted, buildStarted, buildCompleted] = process.argv.slice(2).map((value, index) => index === 0 ? value : Number(value));
writeFileSync(path, `${JSON.stringify({ restore: restoreCompleted - restoreStarted, build: buildCompleted - buildStarted })}\n`);
NODE
    ;;
  extract-prepared)
    manifest=$(<"$manifest_pointer")
    node scripts/ci-artifact-manifest.mjs verify-transport --route "$route_path" --archive "$archive_path" --manifest "$manifest"
    tar -xf "$archive_path"
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
        dotnet restore SIR.slnx --locked-mode
        restore_completed=$(date +%s%3N)
        npm run verify:scaffold
        dotnet fsgg-sdd verify --work 138-sir-fable-game-scaffold --root . --text
        dotnet fsgg-sdd doctor --root . --text
        node - "$phase_path" "$restore_started" "$restore_completed" <<'NODE'
const { writeFileSync } = require("node:fs");
const [path, started, completed] = process.argv.slice(2);
writeFileSync(path, `${JSON.stringify({ restore: Number(completed) - Number(started), build: 0 })}\n`);
NODE
        ;;
      *) echo "qualify-pr: unknown gate: $gate" >&2; exit 2 ;;
    esac
    if [[ "$gate" != evidence ]]; then
      "$0" verify-prepared >/dev/null
    fi
    ;;
  *)
    echo "qualify-pr: usage route PATHS|integrity|prepare|extract-prepared|verify-prepared|gate ID" >&2
    exit 2
    ;;
esac
