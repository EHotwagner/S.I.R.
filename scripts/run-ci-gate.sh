#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
subject=${1:?run-ci-gate: subject id is required}
output=${2:?run-ci-gate: output path is required}
route_path=${SIR_CI_ROUTE:-$repo_root/artifacts/ci/route.json}
phase_path=${SIR_CI_PHASE_PATH:-$repo_root/artifacts/ci/phase-$subject.json}
extract_path=${SIR_CI_EXTRACT_TIMING_PATH:-$repo_root/artifacts/ci/extract-$subject.json}
export SIR_CI_PHASE_PATH="$phase_path"
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
  cancellation) preflight_parts=(native web) ;;
  browser|browser-general-helper|browser-general-helper-2|browser-general-helper-3|browser-delivery) preflight_parts=(web native) ;;
  documentation) preflight_parts=(web native) ;;
esac
if [[ ${#preflight_parts[@]} -gt 0 && "${SIR_CI_PREFLIGHT_REUSED:-false}" != true ]]; then
  set +e
  SIR_CI_EXTRACT_TIMING_PATH="$extract_path" "$repo_root/scripts/qualify-pr.sh" extract-parts "${preflight_parts[@]}"
  preflight_status=$?
  set -e
fi
pre_transport_ms=0
if [[ -f "$extract_path" ]]; then
  pre_transport_ms=$(node -e 'process.stdout.write(String(JSON.parse(require("node:fs").readFileSync(process.argv[1], "utf8")).transport ?? 0))' "$extract_path")
elif [[ $preflight_status -ne 0 ]]; then
  pre_transport_ms=$(( $(date +%s%3N) - command_started ))
  node - "$phase_path" "$pre_transport_ms" <<'NODE'
const { writeFileSync } = require("node:fs");
writeFileSync(process.argv[2], `${JSON.stringify({ restore: 0, build: 0, transport: 0, failureStage: "transport" })}\n`);
NODE
fi
rm -f -- "$extract_path"

trace_dir=""
if [[ "$subject" != integrity && "$subject" != evidence ]]; then
  trace_dir=$(mktemp -d /tmp/sir-ci-gate-dotnet-trace.XXXXXX)
  trace_log="$trace_dir/invocations.log"
  real_dotnet=$(command -v dotnet)
  ln -s "$repo_root/scripts/dotnet-invocation-trace.sh" "$trace_dir/dotnet"
  export SIR_REAL_DOTNET="$real_dotnet"
  export SIR_DOTNET_INVOCATION_LOG="$trace_log"
  export SIR_DOTNET_TRACE_ROOT="$repo_root"
  export PATH="$trace_dir:$PATH"
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
post_transport_ms=0
if [[ -f "$extract_path" ]]; then
  post_transport_ms=$(node -e 'process.stdout.write(String(JSON.parse(require("node:fs").readFileSync(process.argv[1], "utf8")).transport ?? 0))' "$extract_path")
fi
transport_ms=$((pre_transport_ms + post_transport_ms))
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
if [[ -f "${trace_log:-}" ]]; then
  while IFS=$'\t' read -r kind project identity _started _completed; do
    [[ -n "$kind" && -n "$project" ]] || continue
    invocation="$kind:$project"
    [[ "$identity" == "-" ]] || invocation="$invocation:$identity"
    build_args+=(--build "$invocation")
  done < <(sort "$trace_log")
  traced_build_ms=$(node - "$trace_log" <<'NODE'
const { readFileSync } = require("node:fs");
const intervals = readFileSync(process.argv[2], "utf8").trim().split("\n").filter(Boolean).map((line) => {
  const fields = line.split("\t");
  return [Number(fields[3]), Number(fields[4])];
}).filter(([start, end]) => Number.isSafeInteger(start) && Number.isSafeInteger(end) && end >= start).sort(([left], [right]) => left - right);
let total = 0;
let activeStart = 0;
let activeEnd = 0;
for (const [start, end] of intervals) {
  if (start > activeEnd) { total += activeEnd - activeStart; activeStart = start; activeEnd = end; }
  else if (end > activeEnd) activeEnd = end;
}
total += activeEnd - activeStart;
process.stdout.write(String(total));
NODE
  )
  if (( traced_build_ms > build_ms )); then build_ms=$traced_build_ms; fi
fi
if [[ "$subject" != integrity && "$subject" != evidence ]]; then
  while IFS= read -r binding; do
    [[ -n "$binding" ]] && artifact_args+=(--artifact-binding "$binding")
  done < <("$repo_root/scripts/ci-gate-artifact-bindings.sh" "$repo_root" "${preflight_parts[@]}")
  if [[ ${#artifact_args[@]} -gt 0 ]]; then artifact_digest=auto; fi
  receipt_reused=true
fi

test_ms=$((total_ms - setup_ms - restore_ms - build_ms - transport_ms))
if (( test_ms < 0 )); then test_ms=0; fi

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
if [[ -n "$trace_dir" ]]; then rm -rf -- "$trace_dir"; fi
exit "$exit_code"
