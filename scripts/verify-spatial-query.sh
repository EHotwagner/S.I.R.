#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_tmp=$(mktemp -d)
trap 'rm -rf -- "$task_tmp"' EXIT

cd "$repo_root"

"$repo_root/scripts/test-spatial-subject-mutations.sh"

search_fixed_quiet() {
  local pattern=$1
  local file=$2
  if command -v rg >/dev/null 2>&1 && [[ "${SIR_SPATIAL_FORCE_GREP:-0}" != 1 ]]; then
    rg -F "$pattern" "$file" >/dev/null
  else
    grep -F -- "$pattern" "$file" >/dev/null
  fi
}

client_has_authority_calls() {
  if command -v rg >/dev/null 2>&1 && [[ "${SIR_SPATIAL_FORCE_GREP:-0}" != 1 ]]; then
    rg -n 'Los\.lineOfSightBy|Pathfinding\.astar|Edges\.edgeBetween|SpatialQuery\.evaluate' src/SIR.Client.Web -g '*.fs' >/dev/null
  else
    grep -RInE --include='*.fs' --exclude-dir='.fable' --exclude-dir='.fable-rules' --exclude-dir=bin --exclude-dir=obj \
      'Los\.lineOfSightBy|Pathfinding\.astar|Edges\.edgeBetween|SpatialQuery\.evaluate' src/SIR.Client.Web >/dev/null
  fi
}

javascript_has_spatial_authority() {
  if command -v rg >/dev/null 2>&1 && [[ "${SIR_SPATIAL_FORCE_GREP:-0}" != 1 ]]; then
    rg -n 'function .*lineOfSight|function .*astar|const .*lineOfSight|const .*astar' src tests/SIR.Browser.Tests -g '*.js' -g '*.ts' >/dev/null
  else
    grep -RInE --include='*.js' --include='*.ts' --exclude-dir='.fable' --exclude-dir='.fable-rules' --exclude-dir=bin --exclude-dir=obj \
      'function .*lineOfSight|function .*astar|const .*lineOfSight|const .*astar' src tests/SIR.Browser.Tests >/dev/null
  fi
}

dotnet build tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-restore
dotnet_output=$(dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-build --no-restore -- --print-spatial-query)

dotnet fable tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj --outDir "$task_tmp/fable" --noCache
fable_entry="$task_tmp/fable/SIR.Conformance.Shared/Program.js"
fable_output=$(node "$fable_entry" --print-spatial-query)

if [[ "$dotnet_output" != "$fable_output" ]]; then
  echo "Spatial query .NET/Fable canonical fixture mismatch" >&2
  exit 1
fi

for runtime in dotnet fable; do
  mutation_log="$task_tmp/$runtime-mutation.log"
  if [[ "$runtime" == dotnet ]]; then
    if dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-build --no-restore -- --inject-spatial-query-divergence >"$mutation_log" 2>&1; then
      echo "The .NET spatial mutation unexpectedly passed" >&2
      exit 1
    fi
  elif node "$fable_entry" --inject-spatial-query-divergence >"$mutation_log" 2>&1; then
    echo "The Fable spatial mutation unexpectedly passed" >&2
    exit 1
  fi
  search_fixed_quiet "first divergence: fixture=spatial-query byte=0" "$mutation_log"
done

if client_has_authority_calls; then
  echo "Client code contains direct authoritative Game.Core geometry calls" >&2
  exit 1
fi

if javascript_has_spatial_authority; then
  echo "JavaScript/TypeScript contains copied spatial authority" >&2
  exit 1
fi

dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-build --no-restore -- --print-spatial-performance

echo "Spatial query verification passed: exact .NET/Fable bytes, divergence guards, authority scan, and Release budgets."
