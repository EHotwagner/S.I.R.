#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
subject=${1:?run-ci-gate: subject id is required}
output=${2:?run-ci-gate: output path is required}
route_path=${SIR_CI_ROUTE:-$repo_root/artifacts/ci/route.json}
phase_path="$repo_root/artifacts/ci/phase-timing.json"
runner_started=${SIR_CI_RUNNER_START_MS:-$(date +%s%3N)}
command_started=$(date +%s%3N)
rm -f -- "$phase_path"
pre_transport_ms=0
if [[ -f "$repo_root/artifacts/ci/extract-timing.json" ]]; then
  pre_transport_ms=$(node -e 'process.stdout.write(String(JSON.parse(require("node:fs").readFileSync(process.argv[1], "utf8")).transport ?? 0))' "$repo_root/artifacts/ci/extract-timing.json")
fi

readarray -t route_binding < <(node - "$route_path" <<'NODE'
const { readFileSync } = require("node:fs");
const route = JSON.parse(readFileSync(process.argv[2], "utf8"));
console.log(route.source.commit);
console.log(route.source.tree);
console.log(route.digest);
NODE
)

status=pass
set +e
case "$subject" in
  integrity) "$repo_root/scripts/qualify-pr.sh" integrity ;;
  prepare) "$repo_root/scripts/qualify-pr.sh" prepare ;;
  *) "$repo_root/scripts/qualify-pr.sh" gate "$subject" ;;
esac
exit_code=$?
set -e
if [[ $exit_code -ne 0 ]]; then status=fail; fi
completed=$(date +%s%3N)

restore_ms=0
build_ms=0
transport_ms=$pre_transport_ms
phase_setup_ms=0
phase_started=0
failure_stage=test
if [[ -f "$phase_path" ]]; then
  readarray -t phases < <(node - "$phase_path" <<'NODE'
const { readFileSync } = require("node:fs");
const value = JSON.parse(readFileSync(process.argv[2], "utf8"));
console.log(Number.isSafeInteger(value.restore) ? value.restore : 0);
console.log(Number.isSafeInteger(value.build) ? value.build : 0);
console.log(Number.isSafeInteger(value.transport) ? value.transport : 0);
console.log(Number.isSafeInteger(value.setup) ? value.setup : 0);
console.log(Number.isSafeInteger(value.startedAtMilliseconds) ? value.startedAtMilliseconds : 0);
console.log(value.failureStage ?? "test");
NODE
  )
  restore_ms=${phases[0]}
  build_ms=${phases[1]}
  transport_ms=$((pre_transport_ms + phases[2]))
  phase_setup_ms=${phases[3]}
  phase_started=${phases[4]}
  failure_stage=${phases[5]}
fi
setup_ms=$((command_started - runner_started))
setup_ms=$((setup_ms - pre_transport_ms))
if (( setup_ms < 0 )); then setup_ms=0; fi
if (( phase_setup_ms > setup_ms )); then setup_ms=$phase_setup_ms; fi
effective_started=$runner_started
if (( phase_started > 0 && phase_started < effective_started )); then effective_started=$phase_started; fi
total_ms=$((completed - effective_started))
test_ms=$((total_ms - setup_ms - restore_ms - build_ms - transport_ms))
if (( test_ms < 0 )); then test_ms=0; fi

artifact_digest=none
receipt_reused=false
build_args=()
if [[ "$subject" == prepare && -f "$repo_root/artifacts/ci/artifact-manifest.path" ]]; then
  manifest=$(<"$repo_root/artifacts/ci/artifact-manifest.path")
  artifact_digest=$(sha256sum "$repo_root/$manifest" | cut -d' ' -f1)
  if [[ -f "$repo_root/artifacts/ci/build-receipt.path" ]]; then
    receipt=$(<"$repo_root/artifacts/ci/build-receipt.path")
    while IFS= read -r output_id; do build_args+=(--build "$output_id"); done < <(node - "$repo_root/$receipt" <<'NODE'
const { readFileSync } = require("node:fs");
const receipt = JSON.parse(readFileSync(process.argv[2], "utf8"));
for (const output of receipt.outputs ?? []) console.log(output.id);
NODE
    )
  fi
elif [[ "$subject" != integrity && "$subject" != evidence && -f "$repo_root/artifacts/ci/artifact-manifest.path" ]]; then
  manifest=$(<"$repo_root/artifacts/ci/artifact-manifest.path")
  artifact_digest=$(sha256sum "$repo_root/$manifest" | cut -d' ' -f1)
  receipt_reused=true
fi

mkdir -p "$(dirname "$output")"
node "$repo_root/scripts/ci-route.mjs" gate \
  --gate "$subject" \
  --status "$status" \
  --commit "${route_binding[0]}" \
  --tree "${route_binding[1]}" \
  --route-digest "${route_binding[2]}" \
  --artifact-digest "$artifact_digest" \
  --queue-ms unknown \
  --setup-ms "$setup_ms" \
  --restore-ms "$restore_ms" \
  --build-ms "$build_ms" \
  --transport-ms "$transport_ms" \
  --test-ms "$test_ms" \
  --total-ms "$total_ms" \
  --cache-hit "${SIR_CI_CACHE_HIT:-false}" \
  --receipt-reused "$receipt_reused" \
  --failure-stage "$failure_stage" \
  "${build_args[@]}" \
  --output "$output"
exit "$exit_code"
