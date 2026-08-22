#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
protected_mode=false
paired_mode=false
while [[ $# -gt 0 ]]; do
  case "$1" in
    --protected) protected_mode=true; shift ;;
    --paired-optimization) paired_mode=true; shift ;;
    *) echo "qualify-production: unknown argument: $1" >&2; exit 2 ;;
  esac
done
[[ "$protected_mode" != true || "$paired_mode" != true ]] || {
  echo "qualify-production: --protected and --paired-optimization are mutually exclusive" >&2
  exit 2
}
baseline_receipt=${SIR_QUALIFICATION_BASELINE_RECEIPT:-readiness/215-single-pass-qualification/paired-baseline.json}
qualification_root="$repo_root/artifacts/qualification"
qualification_packages="$qualification_root/nuget-packages"
pointer="$qualification_root/build-receipt.path"
conformance_pointer="$qualification_root/conformance-receipt.path"
site_pointer="$qualification_root/site-receipt.path"
fable_log="$qualification_root/fable-invocations.log"
timing_receipt="$qualification_root/single-pass-timing.json"
candidate_source_receipt="$qualification_root/candidate-source.json"
host_receipt="$qualification_root/host.json"
trace_bin="$qualification_root/trace-bin"

cd "$repo_root"
mkdir -p "$qualification_root"
if [[ "$protected_mode" == true ]]; then rm -rf -- "$qualification_packages"; fi
mkdir -p "$qualification_packages"
mkdir -p "$trace_bin"
export NUGET_PACKAGES="$qualification_packages"
node scripts/qualification-provenance.mjs source > "$candidate_source_receipt"
node scripts/qualification-provenance.mjs host > "$host_receipt"
if [[ "$paired_mode" == true ]]; then
node - "$baseline_receipt" "$host_receipt" <<'NODE'
const { readFileSync } = require("node:fs");
const [baselinePath, hostPath] = process.argv.slice(2);
const baseline = JSON.parse(readFileSync(baselinePath, "utf8"));
const host = JSON.parse(readFileSync(hostPath, "utf8"));
if (baseline.schema !== "sir.production-qualification-baseline/v1" || baseline.result !== "pass") throw new Error("qualify-production: invalid paired baseline receipt");
if (!baseline.source?.commit || !baseline.source?.tree || baseline.source.clean !== true) throw new Error("qualify-production: baseline source is not provably clean");
if (baseline.host?.digest !== host.digest || JSON.stringify(baseline.host) !== JSON.stringify(host)) throw new Error("qualify-production: baseline and candidate host fingerprints differ");
NODE
  baseline_ms=$(node -e 'process.stdout.write(String(JSON.parse(require("node:fs").readFileSync(process.argv[1], "utf8")).wallMilliseconds))' "$baseline_receipt")
else
  baseline_ms=0
fi
real_dotnet=$(command -v dotnet)
start_ns=$(date +%s%N)

if [[ "$protected_mode" == true ]]; then
  dotnet tool restore
  dotnet restore SIR.slnx --locked-mode
fi
ln -sfn "$repo_root/scripts/dotnet-invocation-trace.sh" "$trace_bin/dotnet"
export SIR_REAL_DOTNET="$real_dotnet"
export SIR_DOTNET_INVOCATION_LOG="$fable_log"
export PATH="$trace_bin:$PATH"
: > "$fable_log"

if [[ "$protected_mode" == true ]]; then
  if [[ -n ${SIR_PROTECTED_PREFLIGHT_RECEIPT:-} ]]; then
    node scripts/protected-stage-receipt.mjs verify \
      --stage preflight \
      --receipt "$SIR_PROTECTED_PREFLIGHT_RECEIPT"
  else
    ./scripts/verify-rules-corpus.sh
    ./scripts/verify-spatial-query.sh --static-only
    ./scripts/test-worker-cancellation-subject-mutation.sh
  fi
fi

SIR_BUILD_RECEIPT_POINTER="$pointer" ./scripts/test-conformance.sh

if [[ "$protected_mode" == true ]]; then
  npm run verify:scaffold
  ./scripts/test-spatial-diagnostic-subject-mutation.sh
  npm run test:browser
  dotnet fsgg-sdd evidence --work 138-sir-fable-game-scaffold --sync-observed-run artifacts/test-results/browser.junit.xml --root . --text
  dotnet fsgg-sdd verify --work 138-sir-fable-game-scaffold --root . --text
  dotnet fsgg-sdd doctor --root . --text
fi

build_receipt=$(<"$pointer")
node scripts/production-build-receipt.mjs verify \
  --owner-command scripts/qualify-production.sh \
  --receipt "$build_receipt"

node scripts/production-build-receipt.mjs create \
  --owner-command scripts/test-conformance.sh \
  --input src \
  --input scripts \
  --input tests \
  --input package.json \
  --input package-lock.json \
  --input global.json \
  --input .config/dotnet-tools.json \
  --input Directory.Build.props \
  --input Directory.Packages.props \
  --input SIR.slnx \
  --output "build-receipt=$build_receipt" \
  --output feature-bundle-graphs=docs/evidence/client-feature-bundle-graph-v1 \
  --output browser-junit=artifacts/test-results/browser.junit.xml \
  --output browser-diagnostics-junit=artifacts/test-results/browser-diagnostics-child.junit.xml \
  --output delivery-junit=artifacts/test-results/production-delivery.junit.xml \
  --output client-feature-trx=work/214-client-feature-loader/test-results/client-feature-loader.trx \
  --pointer "$conformance_pointer"
conformance_receipt=$(<"$conformance_pointer")

./scripts/build-docs.sh \
  --reuse-build-receipt "$build_receipt" \
  --reuse-conformance-receipt "$conformance_receipt"

node scripts/production-build-receipt.mjs create \
  --owner-command scripts/qualify-production.sh \
  --input src \
  --input scripts \
  --input docs \
  --input package.json \
  --input package-lock.json \
  --input global.json \
  --input .config/dotnet-tools.json \
  --input Directory.Build.props \
  --input Directory.Packages.props \
  --input SIR.slnx \
  --output "build-receipt=$build_receipt" \
  --output "conformance-receipt=$conformance_receipt" \
  --output documentation-site=artifacts/site \
  --pointer "$site_pointer"
site_receipt=$(<"$site_pointer")
node scripts/production-build-receipt.mjs verify \
  --owner-command scripts/qualify-production.sh \
  --receipt "$site_receipt"

node scripts/production-build-receipt.mjs mutate-stale-reuse \
  --owner-command scripts/qualify-production.sh \
  --receipt "$build_receipt"
node scripts/production-build-receipt.mjs mutate-stale-reuse \
  --owner-command scripts/qualify-production.sh \
  --receipt "$site_receipt" \
  --mutation-output-id documentation-site
node scripts/production-build-receipt.mjs mutate-missing-reuse \
  --owner-command scripts/qualify-production.sh \
  --receipt "$site_receipt" \
  --mutation-output-id documentation-site

if [[ "$protected_mode" == true ]]; then
  protected_main_fable_builds=3
  if [[ -n ${SIR_PROTECTED_PREFLIGHT_RECEIPT:-} ]]; then
    # Hosted protected preflight owns the two cancellation-mutation client
    # builds in a separate job. Only the core production build belongs to this
    # invocation trace; the integrated local route retains all three.
    protected_main_fable_builds=1
  fi
  fable_inventory=$(node scripts/verify-fable-invocations.mjs "$fable_log" \
    --expect tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj=1 \
    --expect tests/SIR.ModalInput.Fable.Tests/SIR.ModalInput.Fable.Tests.fsproj=1 \
    --expect tests/SIR.Client.Tests/ScenarioCatalogRuntime.fsproj=1 \
    --expect src/SIR.Replay.Web/SIR.Replay.Web.fsproj="$protected_main_fable_builds" \
    --expect src/SIR.Client.Web/SIR.RulesExplorer.Web.fsproj="$protected_main_fable_builds")
else
  fable_inventory=$(node scripts/verify-fable-invocations.mjs "$fable_log")
fi
fable_target_builds=$(node -e 'process.stdout.write(String(JSON.parse(process.argv[1]).total))' "$fable_inventory")

end_ns=$(date +%s%N)
candidate_ms=$(((end_ns - start_ns) / 1000000))
if [[ "$paired_mode" == true ]]; then
  reduction_basis_points=$(((baseline_ms - candidate_ms) * 10000 / baseline_ms))
  (( reduction_basis_points >= 2000 )) || {
    echo "qualify-production: wall-time reduction ${reduction_basis_points}bp is below the 2000bp material threshold" >&2
    exit 1
  }
else
  reduction_basis_points=0
fi

node - "$timing_receipt" "$baseline_receipt" "$candidate_source_receipt" "$host_receipt" "$candidate_ms" "$reduction_basis_points" "$build_receipt" "$conformance_receipt" "$site_receipt" "$protected_mode" "$paired_mode" "$fable_target_builds" <<'NODE'
const { writeFileSync } = require("node:fs");
const { readFileSync } = require("node:fs");
const [path, baselinePath, candidateSourcePath, hostPath, candidate, reduction, buildReceipt, conformanceReceipt, siteReceipt, protectedMode, pairedMode, fableTargetBuilds] = process.argv.slice(2);
const protectedRoute = protectedMode === "true";
const pairedRoute = pairedMode === "true";
const baseline = pairedRoute ? JSON.parse(readFileSync(baselinePath, "utf8")) : null;
const candidateSource = JSON.parse(readFileSync(candidateSourcePath, "utf8"));
const host = JSON.parse(readFileSync(hostPath, "utf8"));
if (!candidateSource.clean) throw new Error(`qualify-production: candidate source is not clean: ${candidateSource.changes.join(",")}`);
const value = {
  schema: pairedRoute ? "sir.production-qualification-timing/v2" : "sir.production-qualification-timing/v3",
  result: "pass",
  route: protectedRoute ? "protected-clean-room" : pairedRoute ? "paired-optimization" : "local-clean-room",
  host,
  baseline,
  candidate: { command: "./scripts/qualify-production.sh", wallMilliseconds: Number(candidate), fableTargetBuilds: Number(fableTargetBuilds), source: candidateSource, buildReceipt, conformanceReceipt, siteReceipt },
  reductionBasisPoints: pairedRoute ? Number(reduction) : null,
  minimumReductionBasisPoints: pairedRoute ? 2000 : null,
  retainedSubjects: ["rules", "spatial", "cancellation", "conformance", "cross-runtime", "historical-compatibility", "governance", "client-loader", "delivery-budget", "delivery-evidence", "browser-diagnostics", "spatial-diagnostic-mutation", "production-browser", "documentation", "accessibility", "performance", "sdd-verify", "sdd-doctor", "stale-reuse-mutation", "missing-site-mutation"],
};
writeFileSync(path, `${JSON.stringify(value, null, 2)}\n`);
NODE

if [[ "$protected_mode" == true ]]; then route=protected-clean-room
elif [[ "$paired_mode" == true ]]; then route=paired-optimization
else route=local-clean-room
fi
printf 'Single-pass production qualification passed: route=%s baseline=%sms candidate=%sms reduction=%sbp build=%s site=%s\n' \
  "$route" "$baseline_ms" "$candidate_ms" "$reduction_basis_points" "$build_receipt" "$site_receipt"
