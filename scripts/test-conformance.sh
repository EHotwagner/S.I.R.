#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_tmp=$(mktemp -d)
trap 'rm -rf -- "$task_tmp"' EXIT
export NUGET_PACKAGES="$task_tmp/nuget-packages"

cd "$repo_root"

search_fixed() {
  local pattern=$1
  local file=$2

  if command -v rg >/dev/null 2>&1; then
    rg -F "$pattern" "$file" >/dev/null
  else
    grep -F -- "$pattern" "$file" >/dev/null
  fi
}

dotnet tool restore
dotnet restore SIR.slnx --locked-mode
dotnet build SIR.slnx --no-restore

dotnet_output=$(dotnet run \
  --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj \
  --no-build \
  --no-restore)

dotnet run \
  --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj \
  --no-build \
  --no-restore

match_output=$(dotnet run \
  --project tests/SIR.Match.Tests/SIR.Match.Tests.fsproj \
  --no-build \
  --no-restore)

browser_wasm_output=$(./scripts/test-browser-wasm-verification.sh)

dotnet fable tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj \
  --outDir "$task_tmp/fable" \
  --noCache

fable_entry="$task_tmp/fable/SIR.Conformance.Shared/Program.js"
fable_output=$(node "$fable_entry")

if [[ "$dotnet_output" != "$fable_output" ]]; then
  echo ".NET/Fable canonical vector mismatch" >&2
  diff -u <(printf '%s\n' "$dotnet_output") <(printf '%s\n' "$fable_output") >&2 || true
  exit 1
fi

divergence_fixture="bounded-add-overflow-saturates"
divergence_pattern="first divergence: fixture=$divergence_fixture byte=0"

if dotnet run \
  --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj \
  --no-build \
  --no-restore \
  -- --inject-divergence "$divergence_fixture" >"$task_tmp/dotnet-divergence.log" 2>&1; then
  echo "The .NET divergence guard accepted a deliberately changed fixture" >&2
  exit 1
fi

if ! search_fixed "$divergence_pattern" "$task_tmp/dotnet-divergence.log"; then
  echo "The .NET divergence guard did not identify the first changed fixture" >&2
  sed -n '1,80p' "$task_tmp/dotnet-divergence.log" >&2
  exit 1
fi

if node "$fable_entry" \
  --inject-divergence "$divergence_fixture" >"$task_tmp/fable-divergence.log" 2>&1; then
  echo "The Fable divergence guard accepted a deliberately changed fixture" >&2
  exit 1
fi

if ! search_fixed "$divergence_pattern" "$task_tmp/fable-divergence.log"; then
  echo "The Fable divergence guard did not identify the first changed fixture" >&2
  sed -n '1,80p' "$task_tmp/fable-divergence.log" >&2
  exit 1
fi

simulation_phase="movement"
simulation_pattern="first divergence: tick=1 phase=$simulation_phase byte=0"

if dotnet run \
  --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj \
  --no-build \
  --no-restore \
  -- --inject-simulation-divergence "$simulation_phase" \
  >"$task_tmp/dotnet-simulation-divergence.log" 2>&1; then
  echo "The .NET simulation divergence guard accepted a deliberately changed checkpoint" >&2
  exit 1
fi

if ! search_fixed "$simulation_pattern" "$task_tmp/dotnet-simulation-divergence.log"; then
  echo "The .NET simulation divergence guard did not identify the first changed tick and phase" >&2
  sed -n '1,80p' "$task_tmp/dotnet-simulation-divergence.log" >&2
  exit 1
fi

if node "$fable_entry" \
  --inject-simulation-divergence "$simulation_phase" \
  >"$task_tmp/fable-simulation-divergence.log" 2>&1; then
  echo "The Fable simulation divergence guard accepted a deliberately changed checkpoint" >&2
  exit 1
fi

if ! search_fixed "$simulation_pattern" "$task_tmp/fable-simulation-divergence.log"; then
  echo "The Fable simulation divergence guard did not identify the first changed tick and phase" >&2
  sed -n '1,80p' "$task_tmp/fable-simulation-divergence.log" >&2
  exit 1
fi

if command -v rg >/dev/null 2>&1; then
  floating_source=$(rg -n '\b(float|float32|double|decimal)\b' src --glob '*.fs' || true)
else
  floating_source=$(
    grep -RInE \
      --include='*.fs' \
      '(^|[^[:alnum:]_])(float|float32|double|decimal)([^[:alnum:]_]|$)' \
      src || true
  )
fi

if [[ -n "$floating_source" ]]; then
  printf '%s\n' "$floating_source" >&2
  echo "Authoritative source contains floating-point state or operations" >&2
  exit 1
fi

npm ci
./scripts/build-client.sh
node scripts/smoke-client.mjs
worker_measurement=$(node scripts/measure-worker.mjs)

printf 'Conformance passed: %d bytes agree across .NET and Fable/Node.\n' \
  "$(( ${#dotnet_output} / 2 ))"
printf 'Divergence guard passed: %s failed first at byte 0 in both runtimes.\n' \
  "$divergence_fixture"
printf 'Simulation divergence guard passed: tick 1 phase %s failed first at byte 0 in both runtimes.\n' \
  "$simulation_phase"
printf 'Replay gate passed: format v1, SHA-256, checkpoint seeks, safety limits, disclosure boundaries, and verification levels agree.\n'
printf '%s\n' "$match_output"
printf '%s\n' "$browser_wasm_output"
printf 'Elmish and rules-lab gate passed: modes, immutable baseline/fork comparison, typed validation, deterministic sweep, reproducible fixture export, stale operations, cancellation, Fable compilation, production bundle, and browser mount agree.\n'
printf 'Worker gate passed: %s\n' "$worker_measurement"
