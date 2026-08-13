#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
project="$repo_root/tests/SIR.PhysicalCombat.Performance/SIR.PhysicalCombat.Performance.fsproj"
workload="$repo_root/work/181-physical-combat-slice/contracts/combat-performance-workload-v1.json"
receipt="$repo_root/artifacts/test-results/item-181-physical-combat-performance.json"
candidate_commit=$(git -C "$repo_root" rev-parse HEAD)
workload_digest=$(sha256sum "$workload" | cut -d' ' -f1)
source_tree_state=clean
if [[ -n "$(git -C "$repo_root" status --porcelain --untracked-files=all -- . ':(exclude)artifacts')" ]]; then
  if [[ "${SIR_COMBAT_PERF_ALLOW_DIRTY:-}" != "1" ]]; then
    echo "Physical combat performance qualification requires a clean exact candidate; commit the tracked source first." >&2
    exit 2
  fi
  source_tree_state=dirty-development
fi

dotnet restore "$project" --locked-mode
dotnet build "$project" -c Release --no-restore
dotnet run --project "$project" -c Release --no-build -- \
  --receipt "$receipt" \
  --candidate-commit "$candidate_commit" \
  --source-tree-state "$source_tree_state" \
  --workload-definition "$workload" \
  --workload-digest "$workload_digest"

for mutation in trace area recipients facts evidence-bytes; do
  mutation_receipt="$repo_root/artifacts/test-results/item-181-physical-combat-performance-mutation-$mutation.json"
  if SIR_COMBAT_PERF_MUTATE_CAP="$mutation" dotnet run --project "$project" -c Release --no-build -- \
    --receipt "$mutation_receipt" \
    --candidate-commit "$candidate_commit" \
    --source-tree-state "$source_tree_state" \
    --workload-definition "$workload" \
    --workload-digest "$workload_digest"; then
    echo "The $mutation structural-cap mutation unexpectedly passed." >&2
    exit 1
  fi
  grep -q '"outcome": "fail"' "$mutation_receipt"
done

printf 'Physical combat Release performance receipt and five structural-cap mutations verified: %s\n' "$receipt"
