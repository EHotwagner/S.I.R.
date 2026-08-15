#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_tmp=$(mktemp -d)
trap 'rm -rf -- "$task_tmp"' EXIT
cd "$repo_root"

dotnet restore tests/SIR.Client.Tests/ScenarioCatalogRuntime.fsproj --locked-mode
dotnet_output=$(dotnet run --project tests/SIR.Client.Tests/ScenarioCatalogRuntime.fsproj --no-restore)
dotnet fable tests/SIR.Client.Tests/ScenarioCatalogRuntime.fsproj --outDir "$task_tmp/fable" --noCache >/dev/null
fable_output=$(node "$task_tmp/fable/ScenarioCatalogRuntime.js")

if [[ "$dotnet_output" != "$fable_output" ]]; then
  echo "Scenario catalog .NET/Fable runtime fingerprint mismatch" >&2
  diff -u <(printf '%s\n' "$dotnet_output") <(printf '%s\n' "$fable_output") >&2 || true
  exit 1
fi

printf 'Scenario catalog cross-runtime gate passed: %d exact catalog/event/checkpoint bytes.\n' "${#dotnet_output}"
