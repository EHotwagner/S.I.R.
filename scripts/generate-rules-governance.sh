#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
mode=${1:---check}
output_root="$repo_root/readiness/198-rules-governance-receipts"
receipt="$output_root/rules-governance.json"
verdict="$output_root/rules-governance-verdict.json"
boundary="$output_root/protected-boundary.json"
sdd_ship="$repo_root/readiness/239-durable-rules-identity/ship.json"
project="$repo_root/src/SIR.Simulation/Governance.Tool/SIR.Rules.Governance.Tool.fsproj"
temporary_dir=$(mktemp -d /tmp/sir-rules-governance.XXXXXX)
trap 'rm -rf "$temporary_dir"' EXIT

run_args=()
if [[ "${SIR_RULES_PREPARED_PR:-0}" == 1 ]]; then run_args=(--no-build --no-restore); fi

dotnet run --project "$project" -c Release "${run_args[@]}" -- \
  generate "$repo_root" "$temporary_dir/rules-governance.json" "$temporary_dir/rules-governance-verdict.json"
dotnet run --project "$project" -c Release "${run_args[@]}" -- \
  join "$sdd_ship" "$temporary_dir/rules-governance-verdict.json" "$temporary_dir/protected-boundary.json" \
  "readiness/239-durable-rules-identity/ship.json" \
  "readiness/198-rules-governance-receipts/rules-governance-verdict.json"

for gameplay_project in "$repo_root/src/SIR.Domain/SIR.Domain.fsproj" "$repo_root/src/SIR.Simulation/SIR.Simulation.fsproj"; do
  if grep -Eq 'PackageReference Include="FS\.GG\.Governance\.(Kernel|Adapters\.Spi)"' "$gameplay_project"; then
    echo "gameplay runtime references Governance adapter/kernel: ${gameplay_project#"$repo_root/"}" >&2
    exit 1
  fi
done

case "$mode" in
  --write)
    mkdir -p "$output_root"
    cp "$temporary_dir/rules-governance.json" "$receipt"
    cp "$temporary_dir/rules-governance-verdict.json" "$verdict"
    cp "$temporary_dir/protected-boundary.json" "$boundary"
    ;;
  --check)
    cmp "$temporary_dir/rules-governance.json" "$receipt"
    cmp "$temporary_dir/rules-governance-verdict.json" "$verdict"
    cmp "$temporary_dir/protected-boundary.json" "$boundary"
    ;;
  *)
    echo "usage: scripts/generate-rules-governance.sh [--check|--write]" >&2
    exit 2
    ;;
esac

printf 'rules governance receipt and verdict are deterministic, current, and gameplay-runtime isolated\n'
