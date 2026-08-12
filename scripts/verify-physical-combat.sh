#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_tmp=$(mktemp -d)
trap 'rm -rf "$task_tmp"' EXIT
cd "$repo_root"

dotnet build SIR.slnx -c Release --no-restore
dotnet test tests/SIR.Server.Tests/SIR.Server.Tests.fsproj -c Release --no-restore
dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-build -- --print-combat > "$task_tmp/dotnet.hex"
dotnet fable tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj --outDir "$task_tmp/fable" --noCache
node "$task_tmp/fable/SIR.Conformance.Shared/Program.js" --print-combat > "$task_tmp/fable.hex"
cmp "$task_tmp/dotnet.hex" "$task_tmp/fable.hex"

if dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-build -- --inject-combat-divergence > "$task_tmp/divergence.log" 2>&1; then
  echo "The native physical-combat divergence guard unexpectedly passed." >&2
  exit 1
fi
grep -q 'first divergence: fixture=physical-combat byte=0' "$task_tmp/divergence.log"

scripts/test-physical-combat-subject-mutations.sh
dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-build -- --print-combat-performance | tee "$task_tmp/performance.log"
grep -Eq 'representative-ms=([0-9]|1[0-9])/20' "$task_tmp/performance.log"
grep -Eq 'stress-ms=([0-9]|[1-4][0-9])/50' "$task_tmp/performance.log"

scripts/build-client.sh
if rg -n 'Combat\.resolve|Combat\.parameters|PhysicalCombatServices' src/SIR.Client.Web -g '*.fs'; then
  echo "Web presentation source contains physical-combat authority." >&2
  exit 1
fi
if grep -lF 'Attack and attacker identifiers are required.' artifacts/client/content/sir-client/v1/*.js; then
  echo "A presentation bundle contains the physical-combat evaluator." >&2
  exit 1
fi
grep -qF 'Attack and attacker identifiers are required.' artifacts/client/engines/*/worker.js

dotnet publish src/SIR.Server/SIR.Server.fsproj -c Release --no-restore -o artifacts/publish
SIR_JUNIT_OUTPUT=artifacts/test-results/item-181-browser.junit.xml \
  npx playwright test --config tests/SIR.Browser.Tests/playwright.config.js \
  tests/SIR.Browser.Tests/visible-workflows.spec.js tests/SIR.Browser.Tests/production-delivery.spec.js \
  --grep 'player-visible Rules explorer|Release delivery'

printf 'Physical combat verified: exact native/Fable bytes, mutations, bounds, authority route, presentation scan, performance, and browser delivery.\n'
