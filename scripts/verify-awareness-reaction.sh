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

for mutation in los-awareness facing-attention preparation ordering unserviced-expiry edge-removal edge-revision; do
  if dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-build --no-restore -- --inject-awareness-mutation "$mutation" >"$task_tmp/$mutation.log" 2>&1; then
    echo "Awareness mutation survived: $mutation" >&2
    exit 1
  fi
done
for mutation in version hash bounds posture cursor input-vocabulary; do
  if dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-build --no-restore -- --inject-replay-mutation "$mutation" >"$task_tmp/replay-$mutation.log" 2>&1; then
    echo "Replay mutation survived: $mutation" >&2
    exit 1
  fi
done

dotnet run --project tests/SIR.PhysicalCombat.Performance/SIR.PhysicalCombat.Performance.fsproj -c Release --no-build --no-restore -- --awareness
jq '.stress.units = 201' work/182-awareness-reaction-windows/contracts/awareness-reaction-performance-workload-v1.json > "$task_tmp/workload-201.json"
if SIR_AWARENESS_WORKLOAD="$task_tmp/workload-201.json" SIR_AWARENESS_PERF_RECEIPT="$task_tmp/perf-workload-201-receipt.json" dotnet run --project tests/SIR.PhysicalCombat.Performance/SIR.PhysicalCombat.Performance.fsproj -c Release --no-build --no-restore -- --awareness >"$task_tmp/perf-workload-201.log" 2>&1; then
  echo "Awareness workload identity mutation survived: stress.units 200 -> 201" >&2
  exit 1
fi
for mutation in cursor-reset no-movement no-engagements allocation; do
  if SIR_AWARENESS_PERF_MUTATE_SUBJECT="$mutation" SIR_AWARENESS_PERF_RECEIPT="$task_tmp/perf-$mutation-receipt.json" dotnet run --project tests/SIR.PhysicalCombat.Performance/SIR.PhysicalCombat.Performance.fsproj -c Release --no-build --no-restore -- --awareness >"$task_tmp/perf-$mutation.log" 2>&1; then
    echo "Performance mutation survived: $mutation" >&2
    exit 1
  fi
done

npm run build:client
node scripts/smoke-worker-roundtrip.mjs
if SIR_WORKER_ROUNDTRIP_INJECT_RESPONSE_TAG=1 node scripts/smoke-worker-roundtrip.mjs >"$task_tmp/worker-response-mutation.log" 2>&1; then
  echo "Worker round-trip accepted a mutated response tag." >&2
  exit 1
fi
if SIR_DELIVERY_BUDGET_MAX_SPATIAL_RAW=61285 node scripts/test-production-delivery-budget.mjs >"$task_tmp/delivery-budget-mutation.log" 2>&1; then
  echo "Deferred-route delivery budget mutation survived." >&2
  exit 1
fi
node scripts/test-production-delivery-budget.mjs
if SIR_DELIVERY_MUTATE_ARTIFACT=spatial node scripts/test-production-delivery-budget.mjs >"$task_tmp/delivery-artifact-mutation.log" 2>&1; then
  echo "Deferred-route mutated artifact survived production budget." >&2
  exit 1
fi
if rg -n 'AwarenessReaction\.(evaluateVisualStimulus|advanceContact|advanceEngagement)|SpatialQuery\.evaluate|Combat\.resolve' src/SIR.Client.Web/RulesExplorer.fs; then
  echo "Observer-local Web route contains authority evaluation." >&2
  exit 1
fi
dotnet publish src/SIR.Server/SIR.Server.fsproj -c Release --no-restore -o artifacts/publish
SIR_JUNIT_OUTPUT=readiness/182-awareness-reaction-windows/browser-awareness.junit.xml \
  npx playwright test --config tests/SIR.Browser.Tests/playwright.config.js \
  tests/SIR.Browser.Tests/visible-workflows.spec.js --grep 'observer-local awareness'
if SIR_AWARENESS_BROWSER_MUTATE_SUBJECT=projection npx playwright test --config tests/SIR.Browser.Tests/playwright.config.js tests/SIR.Browser.Tests/visible-workflows.spec.js --grep 'observer-local awareness' >"$task_tmp/browser-subject-mutation.log" 2>&1; then
  echo "Production browser journey accepted a mutated local projection." >&2
  exit 1
fi
for mutation in source replay; do
  if SIR_RULES_EXPLORER_MUTATE_SUBJECT="$mutation" npx playwright test --config tests/SIR.Browser.Tests/playwright.config.js tests/SIR.Browser.Tests/visible-workflows.spec.js --grep 'player-visible Rules explorer' >"$task_tmp/rules-explorer-$mutation.log" 2>&1; then
    echo "RulesExplorer accepted mutated $mutation subject." >&2
    exit 1
  fi
done
if SIR_DELIVERY_BROWSER_MUTATE_SUBJECT=deferred-bytes npx playwright test --config tests/SIR.Browser.Tests/playwright.config.js tests/SIR.Browser.Tests/production-delivery.spec.js >"$task_tmp/delivery-browser-mutation.log" 2>&1; then
  echo "Production delivery browser gate accepted mutated deferred bytes." >&2
  exit 1
fi

receipt_before=$(sha256sum readiness/182-awareness-reaction-windows/awareness-reaction-all.junit.xml 2>/dev/null | cut -d' ' -f1 || true)
if SIR_ITEM_182_EVIDENCE_MUTATE_SUBJECT=core ./scripts/generate-item-182-evidence.sh >"$task_tmp/evidence-atomic-mutation.log" 2>&1; then
  echo "Atomic evidence collector accepted a failed protected core subject." >&2
  exit 1
fi
receipt_after=$(sha256sum readiness/182-awareness-reaction-windows/awareness-reaction-all.junit.xml 2>/dev/null | cut -d' ' -f1 || true)
test "$receipt_before" = "$receipt_after" || { echo "Atomic evidence collector published partial receipts." >&2; exit 1; }

printf 'Awareness/reaction verified: native/Fable bytes, authority/replay/performance mutations, disclosure boundary, production build, and browser journey pass.\n'
