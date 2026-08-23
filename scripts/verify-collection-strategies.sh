#!/usr/bin/env bash
# Collection-strategy regression gate (S.I.R.#249).
#
# These are RATIO assertions, not absolute budgets: absolute nanoseconds vary by host, but the
# ordering between strategies does not. The gate fails when a shape that was measured as
# super-linear stops looking super-linear — which means either the benchmark stopped measuring
# anything, or someone reintroduced the slow shape and the comparison collapsed.
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
project="$repo_root/tests/SIR.PhysicalCombat.Performance/SIR.PhysicalCombat.Performance.fsproj"
receipt="${SIR_COLLECTIONS_RECEIPT:-$repo_root/artifacts/test-results/item-249-collection-strategies.json}"

# Tiered compilation adds startup-dependent noise that swamps small-n comparisons.
export DOTNET_TieredCompilation="${DOTNET_TieredCompilation:-0}"

dotnet restore "$project" --locked-mode
dotnet build "$project" -c Release --no-restore
dotnet run --project "$project" -c Release --no-build -- \
  --collections \
  --collections-receipt "$receipt"
