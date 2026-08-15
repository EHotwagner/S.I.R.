#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
cd "$repo_root"

dotnet build SIR.slnx -c Release --no-restore
dotnet run --project tests/SIR.Match.Tests/SIR.Match.Tests.fsproj -c Release --no-build
dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release --no-build
npm run build:client
npm run review:map-editor
npm run review:persistent-workspace-m9
npm run build:docs

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
