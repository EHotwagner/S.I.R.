#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
expected_sdk="10.0.302"
project="$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"
dll="$repo_root/tests/SIR.Domain.Tests/bin/Release/net10.0/SIR.Domain.Tests.dll"
run_root="$(mktemp -d /tmp/sir-quint-q1.XXXXXX)"
trap 'rm -rf -- "$run_root"' EXIT

fail() {
  printf 'SIR-Q1-REFUSAL: %s\n' "$*" >&2
  return 1
}

selected_by_path() {
  case "$1" in
    src/SIR.Simulation/CombatRules.fs|tests/SIR.Conformance.Shared/QuintReplayFixtures.fs|tests/fixtures/rules-corpus/quint-q1/*|scripts/qualify-quint-q1-sir-replay.sh)
      return 0 ;;
    *) return 1 ;;
  esac
}

if [[ "${1:-}" == "--changed-path" ]]; then
  changed_path="${2:?--changed-path requires one repository-relative path}"
  if selected_by_path "$changed_path"; then
    printf 'SIR-Q1-SELECTED: %s\n' "$changed_path"
    exit 0
  fi
  printf 'SIR-Q1-SKIPPED: unrelated path %s\n' "$changed_path"
  exit 0
fi

dotnet_bin="${DOTNET_BIN:?set DOTNET_BIN to the repository-pinned .NET 10.0.302 muxer}"
[[ "$($dotnet_bin --version)" == "$expected_sdk" ]] || fail "SDK pin mismatch"
selected_by_path src/SIR.Simulation/CombatRules.fs || fail "combat implementation change did not select replay"
if selected_by_path src/SIR.Client/UnrelatedView.fs; then
  fail "unrelated implementation change selected replay"
fi

"$dotnet_bin" restore "$project" --locked-mode >/dev/null
"$dotnet_bin" build "$project" --no-restore --configuration Release >/dev/null

"$dotnet_bin" "$dll" --quint-q1-replay >"$run_root/receipt-a.json"
"$dotnet_bin" "$dll" --quint-q1-replay >"$run_root/receipt-b.json"
cmp "$run_root/receipt-a.json" "$run_root/receipt-b.json" >/dev/null \
  || fail "exact replay receipt is not deterministic"
jq -e '
  .producerCandidateCommit == "3a0eced13305b146df2febd96698e38335cae99c" and
  .producerReceiptHead == "6cf3f1f0746c817e1171cd3a7b63865c25c1e346" and
  .seed == "92220" and .transitionBound == 2 and .traceCount == 1 and
  .verdict == "accept-exact-runtime-correspondence"
' "$run_root/receipt-a.json" >/dev/null || fail "runtime receipt lost an exact Q1 binding"

mutations=(
  wrong-action-mapping
  omitted-action
  wrong-observable-field
  stale-expected-state
  combat-boundary-bypass
)

for mutation in "${mutations[@]}"; do
  log="$run_root/$mutation.log"
  if "$dotnet_bin" "$dll" --inject-quint-q1-mutation "$mutation" >"$log" 2>&1; then
    fail "$mutation unexpectedly passed"
  fi
  grep -F 'first divergence: transition=' "$log" >/dev/null \
    || { cat "$log" >&2; fail "$mutation did not report the first transition"; }
  grep -F "mutation=$mutation" "$log" >/dev/null \
    || { cat "$log" >&2; fail "$mutation diagnostic did not name its binding"; }
  printf 'SIR-Q1-MUTATION-PASS: %s\n' "$mutation"
done

if [[ -n "${SIR_Q1_RECEIPT_OUT:-}" ]]; then
  mkdir -p "$(dirname "$SIR_Q1_RECEIPT_OUT")"
  cp "$run_root/receipt-a.json" "$SIR_Q1_RECEIPT_OUT"
fi

if [[ -n "${SIR_Q1_JUNIT_OUT:-}" ]]; then
  mkdir -p "$(dirname "$SIR_Q1_JUNIT_OUT")"
  junit_tmp="$SIR_Q1_JUNIT_OUT.tmp"
  {
    printf '%s\n' '<?xml version="1.0" encoding="UTF-8"?>'
    printf '%s\n' '<testsuite name="sir-quint-q1-runtime-replay" tests="6" failures="0" errors="0" skipped="0">'
    printf '%s\n' '  <testcase classname="SIR.QuintQ1" name="exact-runtime-replay"/>'
    for mutation in "${mutations[@]}"; do
      printf '  <testcase classname="SIR.QuintQ1" name="mutation-%s"/>\n' "$mutation"
    done
    printf '%s\n' '</testsuite>'
  } >"$junit_tmp"
  mv "$junit_tmp" "$SIR_Q1_JUNIT_OUT"
fi

printf 'SIR-Q1-QUALIFIED: 1 exact trace, 3 states, 5 mapping/implementation mutations\n'
