#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
cd "$repo_root"

# The aggregate is valid from a fresh detached checkout: restore locked .NET
# dependencies and replace any ambient JS tree with the lockfile-defined tree
# before the first command that deliberately disables restore.
dotnet restore SIR.slnx --locked-mode
npm ci

dotnet build SIR.slnx -c Release --no-restore
dotnet run --project tests/SIR.Match.Tests/SIR.Match.Tests.fsproj -c Release --no-build
dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release --no-build
mkdir -p src/SIR.Client.Web/.fable/fable_modules/FS.GG.Game.Core.0.13.0
npm run build:client
npm run review:map-editor
npm run review:persistent-workspace-m9
npm run build:docs
git diff --exit-code -- \
  docs/assets/map-editor-review \
  docs/assets/persistent-workspace-m9-review

native_hex=$(dotnet run --project tests/SIR.Match.Tests/SIR.Match.Tests.fsproj -c Release --no-build -- --print-tactical-environment)
fable_hex=$(node --input-type=module - <<'NODE'
import { TacticalEnvironment_exteriorParcelSet, TacticalEnvironment_assemble } from './src/SIR.Client.Web/.fable/SIR.Simulation/TacticalEnvironment.js';
import { TacticalEnvironment_canonicalBytes } from './src/SIR.Client.Web/.fable/SIR.Domain/TacticalEnvironment.js';
const result = TacticalEnvironment_assemble(390n, TacticalEnvironment_exteriorParcelSet[0], TacticalEnvironment_exteriorParcelSet[1]);
if (result.tag !== 0) throw new Error('Fable tactical assembly failed');
process.stdout.write(Buffer.from(TacticalEnvironment_canonicalBytes(result.fields[0])).toString('hex'));
NODE
)
test "$native_hex" = "$fable_hex"

dotnet publish src/SIR.Server/SIR.Server.fsproj -c Release -o artifacts/publish --no-restore

node scripts/test-production-delivery-budget.mjs
if SIR_DELIVERY_BUDGET_MAX_APP_RAW=1000000 node scripts/test-production-delivery-budget.mjs >/dev/null 2>&1; then
  echo "Static initial-entry budget mutation unexpectedly passed" >&2
  exit 1
fi
if SIR_DELIVERY_BROWSER_MUTATE_SUBJECT=initial-bytes \
  npx playwright test --config tests/SIR.Browser.Tests/playwright.config.js \
  tests/SIR.Browser.Tests/production-delivery.spec.js >/dev/null 2>&1; then
  echo "Browser initial-route budget mutation unexpectedly passed" >&2
  exit 1
fi
node scripts/test-production-delivery-evidence.mjs

SIR_JUNIT_OUTPUT=artifacts/test-results/186-tactical-browser.junit.xml \
  npx playwright test --config tests/SIR.Browser.Tests/playwright.config.js \
  tests/SIR.Browser.Tests/tactical-environment.spec.js

for subject in EDGE_STATE CONTENT_IDENTITY DEPENDENCY_LOCALITY DESTRUCTION_BOUND; do
  variable="SIR_TACTICAL_MUTATE_${subject}"
  if env "$variable=1" dotnet run --project tests/SIR.Match.Tests/SIR.Match.Tests.fsproj -c Release --no-build >/dev/null 2>&1; then
    echo "Mutation ${subject} unexpectedly passed" >&2
    exit 1
  fi
done

# Invert each representative production workload observation while retaining
# its named predicate and executing the real combat/environment path.
declare -A representative_mutations=(
  [SOURCE_UNITS]="changed its 100-unit source workload"
  [FINAL_UNITS]="changed its 100-unit final workload"
  [PARTICIPANTS]="did not retain exactly 100 participants"
  [PROPAGATED]="propagated changes beyond each targeted feature"
  [QUERIES]="did not execute exactly 50 spatial queries"
  [CROSSED]="did not traverse any spatial cells"
  [TIMING]="exceeded its 50 ms timing budget"
)
for subject in SOURCE_UNITS FINAL_UNITS PARTICIPANTS PROPAGATED QUERIES CROSSED TIMING; do
  variable="SIR_TACTICAL_MUTATE_REP_${subject}"
  mutation_log=$(mktemp)
  if env "$variable=1" dotnet run --project tests/SIR.Match.Tests/SIR.Match.Tests.fsproj -c Release --no-build >"$mutation_log" 2>&1; then
    echo "Representative mutation ${subject} unexpectedly passed" >&2
    exit 1
  fi
  if ! grep -q "${representative_mutations[$subject]}" "$mutation_log"; then
    echo "Representative mutation ${subject} failed outside its owning assertion" >&2
    cat "$mutation_log" >&2
    exit 1
  fi
  rm -f "$mutation_log"
done

# Delay the production terrain-preview subject while retaining the versioned
# 12 ms dense maximum-map predicate. The owning client gate must turn red, then
# source restoration, rebuild, and rerun must be drift-free and green.
preview_subject="src/SIR.Client/MapEditor.fs"
preview_backup=$(mktemp)
preview_log=$(mktemp)
restore_preview_subject() {
  cp "$preview_backup" "$preview_subject"
  rm -f "$preview_backup" "$preview_log"
}
cp "$preview_subject" "$preview_backup"
trap restore_preview_subject EXIT
node - "$preview_subject" <<'NODE'
import fs from 'node:fs';

const sourcePath = process.argv[2];
const source = fs.readFileSync(sourcePath, 'utf8');
const needle = '    let terrainPreview state =\n';
const replacement = `${needle}        System.Threading.Thread.Sleep 20\n`;
if (!source.includes(needle) || source.includes('System.Threading.Thread.Sleep 20')) {
  throw new Error('dense pointer-preview subject mutation anchor is missing or duplicated');
}
fs.writeFileSync(sourcePath, source.replace(needle, replacement));
NODE
dotnet build tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release --no-restore >/dev/null
if dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release --no-build >"$preview_log" 2>&1; then
  echo "Production-subject dense pointer-preview mutation unexpectedly passed" >&2
  exit 1
fi
if ! grep -q "Maximum-document editor budgets failed" "$preview_log" ||
   ! grep -q "Dense maximum-map budgets: preview" "$preview_log"; then
  echo "Production-subject dense pointer-preview mutation failed outside its owning assertion" >&2
  cat "$preview_log" >&2
  exit 1
fi
restore_preview_subject
trap - EXIT
dotnet build tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release --no-restore >/dev/null
git diff --exit-code -- "$preview_subject"
dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release --no-build >/dev/null

# Break the production identity writer, not its assertion: retain an additional
# 14 MB allocation inside the measured subject while the identity, counters,
# and 16 MB test bound remain unchanged. Restore and rebuild even on failure.
allocation_subject="src/SIR.Simulation/TacticalEnvironment.fs"
allocation_backup=$(mktemp)
allocation_log=$(mktemp)
restore_allocation_subject() {
  cp "$allocation_backup" "$allocation_subject"
  rm -f "$allocation_backup" "$allocation_log"
}
cp "$allocation_subject" "$allocation_backup"
trap restore_allocation_subject EXIT
node - "$allocation_subject" <<'NODE'
import fs from 'node:fs';

const sourcePath = process.argv[2];
const source = fs.readFileSync(sourcePath, 'utf8');
const needle = '        let bytes = Array.zeroCreate<byte> exactByteCount\n';
const replacement = `${needle}        let allocationRegression = Array.zeroCreate<byte> 14_000_000\n        GC.KeepAlive allocationRegression\n`;
if (!source.includes(needle) || source.includes('let allocationRegression =')) {
  throw new Error('preview allocation subject mutation anchor is missing or duplicated');
}
fs.writeFileSync(sourcePath, source.replace(needle, replacement));
NODE
dotnet build tests/SIR.Match.Tests/SIR.Match.Tests.fsproj -c Release --no-restore >/dev/null
if dotnet run --project tests/SIR.Match.Tests/SIR.Match.Tests.fsproj -c Release --no-build >"$allocation_log" 2>&1; then
  echo "Production-subject preview allocation mutation unexpectedly passed" >&2
  exit 1
fi
if ! grep -q "exceeded its 16000000-byte allocation bound" "$allocation_log"; then
  echo "Production-subject preview allocation mutation failed outside its owning assertion" >&2
  cat "$allocation_log" >&2
  exit 1
fi
restore_allocation_subject
trap - EXIT
dotnet build tests/SIR.Match.Tests/SIR.Match.Tests.fsproj -c Release --no-restore >/dev/null
git diff --exit-code -- "$allocation_subject"
dotnet run --project tests/SIR.Match.Tests/SIR.Match.Tests.fsproj -c Release --no-build >/dev/null

cat > readiness/186-authored-tactical-parcels/qualification.junit.xml <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="1" failures="0" errors="0" skipped="0">
  <testsuite name="SIR.186.AuthoredTacticalParcels" tests="1" failures="0" errors="0" skipped="0">
    <testcase classname="SIR.186.Qualification" name="native fable browser docs performance and mutations pass" />
  </testsuite>
</testsuites>
XML
