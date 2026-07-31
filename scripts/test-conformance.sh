#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_tmp=$(mktemp -d)
trap 'rm -rf -- "$task_tmp"' EXIT
export NUGET_PACKAGES="$task_tmp/nuget-packages"

cd "$repo_root"

node scripts/verify-fable-client-baseline.mjs
node scripts/test-persistent-workspace-m0-baseline.mjs

control_abi_generated_before=$(
  sha256sum \
    src/SIR.Domain/ControlAbiV1.Generated.fs \
    generated/control-abi-v1.mjs
)
node scripts/generate-control-abi.mjs
control_abi_generated_after=$(
  sha256sum \
    src/SIR.Domain/ControlAbiV1.Generated.fs \
    generated/control-abi-v1.mjs
)

if [[ "$control_abi_generated_before" != "$control_abi_generated_after" ]]; then
  echo "Generated Control ABI v1 bindings were stale" >&2
  exit 1
fi

control_abi_fixture=$(tr -d '[:space:]' < tests/fixtures/control-abi-v1-output.hex)
control_abi_decoded=$(
  node scripts/decode-control-abi-v1.mjs \
    tests/fixtures/control-abi-v1-output.hex
)

if [[ "$control_abi_fixture" != "$control_abi_decoded" ]]; then
  echo "Standalone Control ABI v1 decoder changed the frozen bytes" >&2
  exit 1
fi

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

modal_dotnet_output=$(dotnet run \
  --project tests/SIR.ModalInput.Tests/SIR.ModalInput.Tests.fsproj \
  --no-build \
  --no-restore)

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

dotnet fable tests/SIR.ModalInput.Fable.Tests/SIR.ModalInput.Fable.Tests.fsproj \
  --outDir "$task_tmp/modal-fable" \
  --noCache

modal_fable_entry="$task_tmp/modal-fable/SIR.ModalInput.Shared/Program.js"
modal_fable_output=$(node "$modal_fable_entry")

if [[ "$modal_dotnet_output" != "$modal_fable_output" ]]; then
  echo ".NET/Fable modal-input resolver fixture mismatch" >&2
  diff -u \
    <(printf '%s\n' "$modal_dotnet_output") \
    <(printf '%s\n' "$modal_fable_output") >&2 || true
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

authoritative_roots=(
  src/SIR.Domain
  src/SIR.Simulation
  src/SIR.Match
)

if command -v rg >/dev/null 2>&1; then
  floating_source=$(
    rg -n \
      '\b(float|float32|double|decimal)\b' \
      "${authoritative_roots[@]}" \
      --glob '*.fs' || true
  )
  client_reference=$(
    rg -n \
      'ProjectReference[^>]+SIR\.Client' \
      src/SIR.Domain/SIR.Domain.fsproj \
      src/SIR.Simulation/SIR.Simulation.fsproj \
      src/SIR.Match/SIR.Match.fsproj || true
  )
  input_presentation_leak=$(
    rg -n \
      '\b(SimulatorSelectedUnit|SimulatorControllerSelection|InputHelpExpanded|HeldInputs)\b' \
      "${authoritative_roots[@]}" \
      src/SIR.Client/SimulatorWorkerProtocol.fs \
      src/SIR.Client/MapEditor.fs \
      src/SIR.Client/MapEditorInterchange.fs \
      src/SIR.Client/MapEditorSimulator.fs \
      src/SIR.Client/ReplayPresentation.fs || true
  )
else
  floating_source=$(
    grep -RInE \
      --include='*.fs' \
      '(^|[^[:alnum:]_])(float|float32|double|decimal)([^[:alnum:]_]|$)' \
      "${authoritative_roots[@]}" || true
  )
  client_reference=$(
    grep -nE \
      'ProjectReference[^>]+SIR\.Client' \
      src/SIR.Domain/SIR.Domain.fsproj \
      src/SIR.Simulation/SIR.Simulation.fsproj \
      src/SIR.Match/SIR.Match.fsproj || true
  )
  input_presentation_leak=$(
    grep -RInE \
      --include='*.fs' \
      '(^|[^[:alnum:]_])(SimulatorSelectedUnit|SimulatorControllerSelection|InputHelpExpanded|HeldInputs)([^[:alnum:]_]|$)' \
      "${authoritative_roots[@]}" \
      src/SIR.Client/SimulatorWorkerProtocol.fs \
      src/SIR.Client/MapEditor.fs \
      src/SIR.Client/MapEditorInterchange.fs \
      src/SIR.Client/MapEditorSimulator.fs \
      src/SIR.Client/ReplayPresentation.fs || true
  )
fi

if [[ -n "$floating_source" ]]; then
  printf '%s\n' "$floating_source" >&2
  echo "Authoritative source contains floating-point state or operations" >&2
  exit 1
fi

if [[ -n "$client_reference" ]]; then
  printf '%s\n' "$client_reference" >&2
  echo "An authoritative project references the presentation-only client" >&2
  exit 1
fi

if [[ -n "$input_presentation_leak" ]]; then
  printf '%s\n' "$input_presentation_leak" >&2
  echo "Modal input presentation state entered authority, replay, map serialization, simulator handoff, or a public protocol payload" >&2
  exit 1
fi

# The only locked dependency with an install script is optional macOS fsevents;
# none of the Linux conformance/build dependencies require lifecycle scripts.
# Remove an inherited npm 12 allow-scripts value and keep CI installs inert.
env \
  -u npm_config_allow_scripts \
  -u NPM_CONFIG_ALLOW_SCRIPTS \
  npm ci --ignore-scripts
./scripts/build-client.sh
node scripts/smoke-client.mjs
node scripts/test-map-editor-qualification.mjs
node scripts/test-planning-workspace-m5-qualification.mjs
node scripts/test-simulator-workspace-m6-qualification.mjs
node scripts/test-review-workspace-m7-qualification.mjs
worker_measurement=$(node scripts/measure-worker.mjs)

printf 'Conformance passed: %d bytes agree across .NET and Fable/Node.\n' \
  "$(( ${#dotnet_output} / 2 ))"
printf 'Modal input gate passed: resolver commands, projections, repeat policy, availability, and conflict diagnostics agree across .NET and Fable/Node.\n'
printf 'Control ABI v1 gate passed: F#, Fable, and the standalone decoder agree on the frozen bytes.\n'
printf 'Divergence guard passed: %s failed first at byte 0 in both runtimes.\n' \
  "$divergence_fixture"
printf 'Simulation divergence guard passed: tick 1 phase %s failed first at byte 0 in both runtimes.\n' \
  "$simulation_phase"
printf 'Replay gate passed: formats v1-v2, SHA-256, checkpoint seeks, safety limits, disclosure boundaries, and verification levels agree.\n'
printf 'Numeric authority gate passed: Domain, Simulation, and Match contain no floating-point state and do not reference the presentation-only client.\n'
printf 'Modal input boundary gate passed: transient selection, controller-choice, help, and held-input state remain outside authority, replay, map serialization, simulator handoff, and public protocol payloads.\n'
printf '%s\n' "$match_output"
printf '%s\n' "$browser_wasm_output"
printf 'Elmish and rules-lab gate passed: modes, immutable baseline/fork comparison, typed validation, deterministic sweep, reproducible fixture export, stale operations, cancellation, Fable compilation, production bundle, and browser mount agree.\n'
printf 'Worker gate passed: clone-safe replay/lab and simulator-session round trips, stale/cancellation/disclosure guards, bounded planning progress; %s\n' "$worker_measurement"
