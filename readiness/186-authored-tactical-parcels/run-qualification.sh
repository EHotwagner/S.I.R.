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

cat > readiness/186-authored-tactical-parcels/qualification.junit.xml <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="1" failures="0" errors="0" skipped="0">
  <testsuite name="SIR.186.AuthoredTacticalParcels" tests="1" failures="0" errors="0" skipped="0">
    <testcase classname="SIR.186.Qualification" name="native fable browser docs performance and mutations pass" />
  </testsuite>
</testsuites>
XML
