#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_tmp=$(mktemp -d)
trap 'rm -rf "$task_tmp"' EXIT
cd "$repo_root"

dotnet restore SIR.slnx
dotnet restore tests/SIR.Server.Tests/SIR.Server.Tests.fsproj
npm ci --ignore-scripts
dotnet build SIR.slnx -c Release --no-restore
dotnet test tests/SIR.Server.Tests/SIR.Server.Tests.fsproj -c Release --no-restore

dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-build --no-restore -- --print-awareness-reaction > "$task_tmp/native.hex"
dotnet fable tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj --outDir "$task_tmp/fable" --noCache
node "$task_tmp/fable/SIR.Conformance.Shared/Program.js" --print-awareness-reaction > "$task_tmp/fable.hex"
cmp "$task_tmp/native.hex" "$task_tmp/fable.hex"

for mutation in los-awareness facing-attention preparation ordering; do
  if dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-build --no-restore -- --inject-awareness-mutation "$mutation" >"$task_tmp/$mutation.log" 2>&1; then
    echo "Awareness mutation survived: $mutation" >&2
    exit 1
  fi
done
for mutation in version hash bounds; do
  if dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-build --no-restore -- --inject-replay-mutation "$mutation" >"$task_tmp/replay-$mutation.log" 2>&1; then
    echo "Replay mutation survived: $mutation" >&2
    exit 1
  fi
done

dotnet run --project tests/SIR.PhysicalCombat.Performance/SIR.PhysicalCombat.Performance.fsproj -c Release --no-build --no-restore -- --awareness
for mutation in candidates los episodes evidence-bytes timing; do
  if SIR_AWARENESS_PERF_MUTATE_CAP="$mutation" dotnet run --project tests/SIR.PhysicalCombat.Performance/SIR.PhysicalCombat.Performance.fsproj -c Release --no-build --no-restore -- --awareness >"$task_tmp/perf-$mutation.log" 2>&1; then
    echo "Performance mutation survived: $mutation" >&2
    exit 1
  fi
done

npm run build:client
if SIR_DELIVERY_BUDGET_MAX_SPATIAL_RAW=61285 node scripts/test-production-delivery-budget.mjs >"$task_tmp/delivery-budget-mutation.log" 2>&1; then
  echo "Deferred-route delivery budget mutation survived." >&2
  exit 1
fi
node scripts/test-production-delivery-budget.mjs
if rg -n 'AwarenessReaction\.(evaluateVisualStimulus|advanceContact|advanceEngagement)|SpatialQuery\.evaluate|Combat\.resolve' src/SIR.Client.Web/RulesExplorer.fs; then
  echo "Observer-local Web route contains authority evaluation." >&2
  exit 1
fi
dotnet publish src/SIR.Server/SIR.Server.fsproj -c Release --no-restore -o artifacts/publish
SIR_JUNIT_OUTPUT=readiness/182-awareness-reaction-windows/browser-awareness.junit.xml \
  npx playwright test --config tests/SIR.Browser.Tests/playwright.config.js \
  tests/SIR.Browser.Tests/visible-workflows.spec.js --grep 'observer-local awareness'

printf 'Awareness/reaction verified: native/Fable bytes, authority/replay/performance mutations, disclosure boundary, production build, and browser journey pass.\n'
