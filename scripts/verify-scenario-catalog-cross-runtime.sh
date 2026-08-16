#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_tmp=$(mktemp -d)
trap 'rm -rf -- "$task_tmp"' EXIT
prepared_native=""
prepared_fable=""
while [[ $# -gt 0 ]]; do
  case "$1" in
    --prepared-native)
      [[ $# -ge 2 ]] || { echo "verify-scenario-catalog-cross-runtime: --prepared-native requires a path" >&2; exit 2; }
      prepared_native=$2
      shift 2
      ;;
    --prepared-fable)
      [[ $# -ge 2 ]] || { echo "verify-scenario-catalog-cross-runtime: --prepared-fable requires a path" >&2; exit 2; }
      prepared_fable=$2
      shift 2
      ;;
    *)
      echo "verify-scenario-catalog-cross-runtime: unknown argument: $1" >&2
      exit 2
      ;;
  esac
done
if [[ -n "$prepared_native" || -n "$prepared_fable" ]]; then
  [[ -n "$prepared_native" && -n "$prepared_fable" ]] || { echo "verify-scenario-catalog-cross-runtime: prepared reuse requires both runtime paths" >&2; exit 2; }
fi
cd "$repo_root"

if [[ -n "$prepared_native" ]]; then
  dotnet_output=$(dotnet "$prepared_native")
  fable_output=$(node "$prepared_fable")
else
  dotnet restore tests/SIR.Client.Tests/ScenarioCatalogRuntime.fsproj --locked-mode
  dotnet_output=$(dotnet run --project tests/SIR.Client.Tests/ScenarioCatalogRuntime.fsproj --no-restore)
  dotnet fable tests/SIR.Client.Tests/ScenarioCatalogRuntime.fsproj --outDir "$task_tmp/fable" --noCache >/dev/null
  fable_output=$(node "$task_tmp/fable/ScenarioCatalogRuntime.js")
fi

if [[ "$dotnet_output" != "$fable_output" ]]; then
  echo "Scenario catalog .NET/Fable runtime fingerprint mismatch" >&2
  diff -u <(printf '%s\n' "$dotnet_output") <(printf '%s\n' "$fable_output") >&2 || true
  exit 1
fi

printf 'Scenario catalog cross-runtime gate passed: %d exact catalog/event/checkpoint bytes.\n' "${#dotnet_output}"
