#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_tmp=$(mktemp -d)
trap 'rm -rf -- "$task_tmp"' EXIT
reuse_build_receipt=""
prepared_fable=""
static_only=false
prepared_pr=false
while [[ $# -gt 0 ]]; do
  case "$1" in
    --reuse-pr-build-receipt)
      [[ $# -ge 2 ]] || { echo "verify-spatial-query: --reuse-pr-build-receipt requires a path" >&2; exit 2; }
      reuse_build_receipt=$2
      shift 2
      ;;
    --prepared-fable)
      [[ $# -ge 2 ]] || { echo "verify-spatial-query: --prepared-fable requires a path" >&2; exit 2; }
      prepared_fable=$2
      shift 2
      ;;
    --static-only)
      static_only=true
      shift
      ;;
    --prepared-pr)
      prepared_pr=true
      shift
      ;;
    *) echo "verify-spatial-query: unknown argument: $1" >&2; exit 2 ;;
  esac
done

cd "$repo_root"

if [[ "$prepared_pr" == true ]]; then
  "$repo_root/scripts/test-spatial-subject-mutations.sh" --prepared-pr
else
  "$repo_root/scripts/test-spatial-subject-mutations.sh"
fi

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

require_clean_scan() {
  local description=$1
  shift
  local status
  if "$@"; then
    echo "$description contains forbidden spatial authority" >&2
    exit 1
  else
    status=$?
  fi
  if [[ $status -ne 1 ]]; then
    echo "$description could not be read (search exited $status)" >&2
    exit 1
  fi
}

expect_unreadable_client_scan_error() {
  local source=src/SIR.Client.Web/RulesExplorer.fs
  local original_mode
  local status
  original_mode=$(stat -c '%a' "$source")
  chmod 000 "$source"
  if client_has_authority_calls; then
    status=0
  else
    status=$?
  fi
  chmod "$original_mode" "$source"
  if [[ $status -le 1 ]]; then
    echo "Client authority scan did not fail closed for unreadable source (search exited $status)" >&2
    exit 1
  fi
}

expect_unreadable_client_scan_error
SIR_SPATIAL_FORCE_GREP=1 expect_unreadable_client_scan_error
require_clean_scan "Client code" client_has_authority_calls
require_clean_scan "JavaScript/TypeScript" javascript_has_spatial_authority
if [[ "$static_only" == true ]]; then
  echo "Spatial query static verification passed: mutations, unreadable-source guards, and authority scans."
  exit 0
fi

if [[ -n "$reuse_build_receipt" ]]; then
  [[ -n "$prepared_fable" ]] || { echo "verify-spatial-query: prepared reuse requires a Fable fixture root" >&2; exit 2; }
  node scripts/production-build-receipt.mjs verify --owner-command scripts/qualify-pr.sh --receipt "$reuse_build_receipt"
else
  dotnet build tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-restore
fi
dotnet_output=$(dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-build --no-restore -- --print-spatial-query)

if [[ -n "$reuse_build_receipt" ]]; then
  fable_entry="$prepared_fable/SIR.Conformance.Shared/Program.js"
else
  dotnet fable tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj --outDir "$task_tmp/fable" --noCache
  fable_entry="$task_tmp/fable/SIR.Conformance.Shared/Program.js"
fi
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

dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-build --no-restore -- --print-spatial-performance

echo "Spatial query verification passed: exact .NET/Fable bytes, divergence guards, authority scan, and Release budgets."
