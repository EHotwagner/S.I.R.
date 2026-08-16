#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
baseline_ms=${SIR_QUALIFICATION_BASELINE_MS:-373848}
qualification_root="$repo_root/artifacts/qualification"
qualification_packages="$qualification_root/nuget-packages"
pointer="$qualification_root/build-receipt.path"
fable_log="$qualification_root/fable-invocations.log"
timing_receipt="$qualification_root/single-pass-timing.json"

cd "$repo_root"
mkdir -p "$qualification_root"
mkdir -p "$qualification_packages"
export NUGET_PACKAGES="$qualification_packages"
: > "$fable_log"
start_ns=$(date +%s%N)

SIR_BUILD_RECEIPT_POINTER="$pointer" \
SIR_FABLE_INVOCATION_LOG="$fable_log" \
  ./scripts/test-conformance.sh

build_receipt=$(<"$pointer")
node scripts/production-build-receipt.mjs verify \
  --owner-command scripts/qualify-production.sh \
  --receipt "$build_receipt"

./scripts/build-docs.sh --reuse-build-receipt "$build_receipt"

node scripts/production-build-receipt.mjs mutate-stale-reuse \
  --owner-command scripts/qualify-production.sh \
  --receipt "$build_receipt"

mapfile -t fable_invocations < "$fable_log"
expected=(main-fable rules-fable)
[[ "${fable_invocations[*]}" == "${expected[*]}" ]] || {
  echo "qualify-production: expected one main and one Rules Fable invocation; got ${fable_invocations[*]}" >&2
  exit 1
}

end_ns=$(date +%s%N)
candidate_ms=$(((end_ns - start_ns) / 1000000))
reduction_basis_points=$(((baseline_ms - candidate_ms) * 10000 / baseline_ms))
(( reduction_basis_points >= 2000 )) || {
  echo "qualify-production: wall-time reduction ${reduction_basis_points}bp is below the 2000bp material threshold" >&2
  exit 1
}

node - "$timing_receipt" "$baseline_ms" "$candidate_ms" "$reduction_basis_points" "$build_receipt" <<'NODE'
const { writeFileSync } = require("node:fs");
const [path, baseline, candidate, reduction, receipt] = process.argv.slice(2);
const value = {
  schema: "sir.production-qualification-timing/v1",
  result: "pass",
  baseline: { command: "./scripts/test-conformance.sh && ./scripts/build-docs.sh", wallMilliseconds: Number(baseline), fableTargetBuilds: 4 },
  candidate: { command: "./scripts/qualify-production.sh", wallMilliseconds: Number(candidate), fableTargetBuilds: 2, buildReceipt: receipt },
  reductionBasisPoints: Number(reduction),
  minimumReductionBasisPoints: 2000,
  retainedSubjects: ["conformance", "client-loader", "delivery-budget", "delivery-evidence", "browser-diagnostics", "production-browser", "documentation", "accessibility", "stale-reuse-mutation"],
};
writeFileSync(path, `${JSON.stringify(value, null, 2)}\n`);
NODE

printf 'Single-pass production qualification passed: baseline=%sms candidate=%sms reduction=%sbp receipt=%s\n' \
  "$baseline_ms" "$candidate_ms" "$reduction_basis_points" "$build_receipt"
