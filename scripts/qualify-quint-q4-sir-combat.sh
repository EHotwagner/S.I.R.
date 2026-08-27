#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
authority="$repo_root/docs/rules/sir-combat.md"
task_tmp="$(mktemp -d)"
trap 'rm -rf "$task_tmp"' EXIT

fail() {
  echo "quint-q4-sir-combat: $*" >&2
  exit 1
}

test -f "$authority" || fail "missing authority: docs/rules/sir-combat.md"
command -v quint >/dev/null 2>&1 || fail "quint is not installed"
test "$(quint --version)" = "0.32.0" || fail "expected Quint 0.32.0"

extract() {
  local output="$1"
  awk '
    /^```quint sir-combat.qnt \+=$/ { inside = 1; found = 1; next }
    /^```quint / { if (!inside) unexpected = 1 }
    /^```$/ { if (inside) { inside = 0; next } }
    inside { print }
    END {
      if (!found || inside || unexpected) exit 1
    }
  ' "$authority" > "$output"
}

extract "$task_tmp/first.qnt" || fail "authority fences are missing, unterminated, or target an unexpected module"
extract "$task_tmp/second.qnt" || fail "second clean extraction failed"
cmp -s "$task_tmp/first.qnt" "$task_tmp/second.qnt" || fail "two clean extractions differ"

verify_projection() {
  cmp -s "$task_tmp/first.qnt" "$1"
}

verify_contract_identity() {
  local model="$1"
  grep -Fx 'module SirCombat {' "$model" >/dev/null \
    && grep -Fx '    implementation: "FS.GG.Game.Core.Los.lineOfSightBy",' "$model" >/dev/null \
    && grep -Fx '    fingerprint: "FS.GG.Game.Core@0.13.0:Los.lineOfSightBy:Supercover",' "$model" >/dev/null
}

verify_projection "$task_tmp/second.qnt" || fail "generated projection is stale"
verify_contract_identity "$task_tmp/first.qnt" || fail "model identity or external algorithm contract drifted"

quint typecheck "$task_tmp/first.qnt" --out "$task_tmp/typed-effect.json"

witnesses='representativeDamageIsTwenty|damageRoundingPreservesInt32Wrap|woundThresholdsAreExact|zeroHealthMeansIncapacitated|suppressionNeedsPositiveDamageAndRecoversFive|destroyingCoverConsumesCurrentCollision|collateralOutcomeIgnoresFaction'
quint test "$task_tmp/first.qnt" \
  --main SirCombatTests \
  --backend rust \
  --seed 352 \
  --match "$witnesses" \
  --verbosity 3 > "$task_tmp/tests.log"

quint run "$task_tmp/first.qnt" \
  --main SirCombat \
  --backend rust \
  --seed 352 \
  --max-samples 64 \
  --max-steps 8 \
  --invariants \
    sixteenRulesDeclared \
    boundedCombatState \
    incapacityMatchesHealth \
    destroyedCoverIsPermeable \
    validTraceObservation \
    suppressionRequiresDamage \
    factionNeutralCollateral \
  --verbosity 1 > "$task_tmp/run.log"

grep -F '7 passing' "$task_tmp/tests.log" >/dev/null || fail "the seven named witnesses did not all pass"
grep -F '[ok] No violation found' "$task_tmp/run.log" >/dev/null || fail "seeded invariant simulation did not pass"

sed '0,/armorRetentionRaw: 8000/s//armorRetentionRaw: 7000/' \
  "$task_tmp/first.qnt" > "$task_tmp/wrong-retention.qnt"
if quint test "$task_tmp/wrong-retention.qnt" --main SirCombatTests --backend rust --seed 352 \
  --match representativeDamageIsTwenty --verbosity 1 > "$task_tmp/wrong-retention.log" 2>&1; then
  fail "changed representative retention unexpectedly passed"
fi

sed '0,/if (damage > 0) maximum(0, requestedDelta) else 0/s//maximum(0, requestedDelta)/' \
  "$task_tmp/first.qnt" > "$task_tmp/missing-suppression-guard.qnt"
if quint test "$task_tmp/missing-suppression-guard.qnt" --main SirCombatTests --backend rust --seed 352 \
  --match suppressionNeedsPositiveDamageAndRecoversFive --verbosity 1 > "$task_tmp/missing-suppression-guard.log" 2>&1; then
  fail "removed suppression guard unexpectedly passed"
fi

cp "$task_tmp/first.qnt" "$task_tmp/stale-generated.qnt"
printf '\n// deliberately stale generated projection\n' >> "$task_tmp/stale-generated.qnt"
if verify_projection "$task_tmp/stale-generated.qnt"; then
  fail "changed generated projection was not detected"
fi

sed '0,/module SirCombat {/s//module WrongIdentity {/' \
  "$task_tmp/first.qnt" > "$task_tmp/wrong-identity.qnt"
if verify_contract_identity "$task_tmp/wrong-identity.qnt"; then
  fail "wrong model identity unexpectedly passed"
fi

sed '0,/FS.GG.Game.Core@0.13.0:Los.lineOfSightBy:Supercover/s//wrong-contract/' \
  "$task_tmp/first.qnt" > "$task_tmp/wrong-contract.qnt"
if verify_contract_identity "$task_tmp/wrong-contract.qnt"; then
  fail "wrong external algorithm contract unexpectedly passed"
fi

runtime_summary="model-only"

if [[ "${1:-}" != "--model-only" ]]; then
  dotnet_bin="${DOTNET_BIN:-$(command -v dotnet || true)}"
  test -n "$dotnet_bin" || fail "dotnet is required for runtime correspondence; pass --model-only to skip it explicitly"
  test "$($dotnet_bin --version)" = "10.0.302" || fail "runtime correspondence requires repository SDK 10.0.302"

  trace_root="$task_tmp/itf"
  mkdir -p "$trace_root"
  quint run "$task_tmp/first.qnt" \
    --main SirCombat \
    --backend rust \
    --seed 352 \
    --max-samples 16 \
    --n-traces 16 \
    --max-steps 8 \
    --invariants boundedCombatState \
    --out-itf "$trace_root/trace_{seq}.itf.json" \
    --verbosity 1 > "$task_tmp/itf-run.log"

  for trace in "$trace_root"/trace_*.itf.json; do
    jq 'del(."#meta".description, ."#meta".timestamp) | ."#meta".source = "sir-combat.qnt"' \
      "$trace" > "$trace.normalized"
    mv "$trace.normalized" "$trace"
  done

  project="$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"
  dll="$repo_root/tests/SIR.Domain.Tests/bin/Release/net10.0/SIR.Domain.Tests.dll"
  "$dotnet_bin" restore "$project" --locked-mode >/dev/null
  "$dotnet_bin" build "$project" --configuration Release --no-restore >/dev/null
  exact_root="$repo_root/tests/fixtures/rules-corpus/quint-q4"
  exact_summary="$($dotnet_bin "$dll" --quint-q4-exact "$exact_root" 1)"
  grep -F 'SIR-Q4-EXACT-ACCEPT: traces=1 states=9' <<< "$exact_summary" >/dev/null \
    || fail "committed exact trace did not replay through the real interpreter"
  runtime_summary="$($dotnet_bin "$dll" --quint-q4-sampled "$trace_root" 16)"
  grep -F 'SIR-Q4-SAMPLED-ACCEPT: traces=16 states=144' <<< "$runtime_summary" >/dev/null \
    || fail "real-interpreter correspondence did not accept all 16 traces and 144 states"

  jq -s -e '[.[].states[].last.eventId] | any(. == "attack:representative")' "$trace_root"/trace_*.itf.json >/dev/null \
    || fail "sampled traces do not exercise the representative interpreter boundary"

  for mutation in wrong-action-mapping wrong-observable-field combat-boundary-defect; do
    mutation_log="$task_tmp/$mutation.log"
    if "$dotnet_bin" "$dll" --inject-quint-q4-mutation "$mutation" "$trace_root" 16 > "$mutation_log" 2>&1; then
      fail "$mutation unexpectedly passed runtime correspondence"
    fi
    grep -F 'Q4 first divergence:' "$mutation_log" >/dev/null \
      || fail "$mutation did not identify the first divergence"
    grep -F 'adapter=tests/SIR.Conformance.Shared/QuintQ4ReplayFixtures.fs:applyModelAction' "$mutation_log" >/dev/null \
      || fail "$mutation did not identify the adapter source"
    grep -F 'implementation=src/SIR.Simulation/CombatRules.fs:CombatRules' "$mutation_log" >/dev/null \
      || fail "$mutation did not identify the implementation source"
  done

  restored_exact_summary="$($dotnet_bin "$dll" --quint-q4-exact "$exact_root" 1)"
  grep -F 'SIR-Q4-EXACT-ACCEPT: traces=1 states=9' <<< "$restored_exact_summary" >/dev/null \
    || fail "untouched exact replay did not restore green after runtime correspondence mutations"
  restored_runtime_summary="$($dotnet_bin "$dll" --quint-q4-sampled "$trace_root" 16)"
  grep -F 'SIR-Q4-SAMPLED-ACCEPT: traces=16 states=144' <<< "$restored_runtime_summary" >/dev/null \
    || fail "untouched sampled replay did not restore green after runtime correspondence mutations"
fi

if [[ -n "${SIR_Q4_JUNIT_OUT:-}" ]]; then
  mkdir -p "$(dirname "$SIR_Q4_JUNIT_OUT")"
  junit_tmp="$SIR_Q4_JUNIT_OUT.tmp"
  {
    printf '%s\n' '<?xml version="1.0" encoding="UTF-8"?>'
    printf '%s\n' '<testsuite name="sir-quint-q4-combat" tests="20" failures="0" errors="0" skipped="0">'
    for witness in \
      representativeDamageIsTwenty \
      damageRoundingPreservesInt32Wrap \
      woundThresholdsAreExact \
      zeroHealthMeansIncapacitated \
      suppressionNeedsPositiveDamageAndRecoversFive \
      destroyingCoverConsumesCurrentCollision \
      collateralOutcomeIgnoresFaction; do
      printf '  <testcase classname="SIR.QuintQ4" name="witness-%s"/>\n' "$witness"
    done
    printf '%s\n' '  <testcase classname="SIR.QuintQ4" name="seeded-invariant-simulation"/>'
    printf '%s\n' '  <testcase classname="SIR.QuintQ4" name="exact-runtime-correspondence"/>'
    printf '%s\n' '  <testcase classname="SIR.QuintQ4" name="sampled-runtime-correspondence"/>'
    printf '%s\n' '  <testcase classname="SIR.QuintQ4" name="restored-exact-runtime-correspondence"/>'
    printf '%s\n' '  <testcase classname="SIR.QuintQ4" name="restored-sampled-runtime-correspondence"/>'
    for mutation in \
      changed-armor-retention \
      removed-suppression-guard \
      stale-generated-module \
      wrong-model-identity \
      wrong-external-algorithm-contract \
      wrong-action-mapping \
      wrong-observable-field \
      corrupted-interpreter-boundary-result; do
      printf '  <testcase classname="SIR.QuintQ4" name="mutation-%s"/>\n' "$mutation"
    done
    printf '%s\n' '</testsuite>'
  } >"$junit_tmp"
  mv "$junit_tmp" "$SIR_Q4_JUNIT_OUT"
fi

echo "quint-q4-sir-combat: PASS"
echo "authority=$(sha256sum "$authority" | cut -d' ' -f1)"
echo "generated=$(sha256sum "$task_tmp/first.qnt" | cut -d' ' -f1)"
echo "typedEffect=$(sha256sum "$task_tmp/typed-effect.json" | cut -d' ' -f1)"
echo "quint=$(quint --version)"
echo "witnesses=7"
echo "mutations=8"
echo "simulationSeed=352 samples=64 steps=8"
echo "runtime=$runtime_summary"
