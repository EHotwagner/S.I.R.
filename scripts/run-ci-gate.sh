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
readarray -t route_binding < <(node - "$route_path" <<'NODE'
const { readFileSync } = require("node:fs");
const route = JSON.parse(readFileSync(process.argv[2], "utf8"));
console.log(route.source.commit);
console.log(route.source.tree);
console.log(route.digest);
NODE
)

status=pass
preflight_status=0
preflight_parts=()
case "$subject" in
  rules) preflight_parts=(native) ;;
  spatial|cross-runtime) preflight_parts=(native fable) ;;
  browser) preflight_parts=(web server) ;;
  documentation) preflight_parts=(web docs) ;;
esac
if [[ ${#preflight_parts[@]} -gt 0 ]]; then
  set +e
  "$repo_root/scripts/qualify-pr.sh" extract-parts "${preflight_parts[@]}"
  preflight_status=$?
  set -e
fi
pre_transport_ms=0
if [[ -f "$repo_root/artifacts/ci/extract-timing.json" ]]; then
  pre_transport_ms=$(node -e 'process.stdout.write(String(JSON.parse(require("node:fs").readFileSync(process.argv[1], "utf8")).transport ?? 0))' "$repo_root/artifacts/ci/extract-timing.json")
elif [[ $preflight_status -ne 0 ]]; then
  pre_transport_ms=$(( $(date +%s%3N) - command_started ))
  node - "$phase_path" "$pre_transport_ms" <<'NODE'
const { writeFileSync } = require("node:fs");
writeFileSync(process.argv[2], `${JSON.stringify({ restore: 0, build: 0, transport: 0, failureStage: "transport" })}\n`);
NODE
fi

set +e
if [[ $preflight_status -ne 0 ]]; then
  exit_code=$preflight_status
else
  case "$subject" in
    integrity) "$repo_root/scripts/qualify-pr.sh" integrity ;;
    *) "$repo_root/scripts/qualify-pr.sh" gate "$subject" ;;
  esac
  exit_code=$?
fi
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
if (( phase_setup_ms > setup_ms )); then setup_ms=$phase_setup_ms; fi
effective_started=$runner_started
if (( phase_started > 0 && phase_started < effective_started )); then effective_started=$phase_started; fi
total_ms=$((completed - effective_started))
test_ms=$((total_ms - setup_ms - restore_ms - build_ms - transport_ms))
if (( test_ms < 0 )); then test_ms=0; fi

artifact_digest=none
artifact_args=()
receipt_reused=false
build_args=()
if [[ "$subject" != integrity && "$subject" != evidence ]]; then
  for pointer in "$repo_root"/artifacts/ci/parts/*.manifest.path; do
    [[ -f "$pointer" ]] || continue
    part=$(basename "$pointer" .manifest.path)
    manifest=$(<"$pointer")
    artifact_args+=(--artifact-binding "$part=$(sha256sum "$repo_root/$manifest" | cut -d' ' -f1)")
  done
  if [[ ${#artifact_args[@]} -gt 0 ]]; then artifact_digest=auto; fi
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
  "${artifact_args[@]}" \
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
