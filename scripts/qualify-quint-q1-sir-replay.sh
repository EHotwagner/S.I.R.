#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
expected_sdk="10.0.302"
project="$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"
dll="$repo_root/tests/SIR.Domain.Tests/bin/Release/net10.0/SIR.Domain.Tests.dll"
model="$repo_root/tests/fixtures/rules-corpus/quint-q1/sir-damage.qnt"
run_root="$(mktemp -d /tmp/sir-quint-q1.XXXXXX)"
trap 'rm -rf -- "$run_root"' EXIT

fail() {
  printf 'SIR-Q1-REFUSAL: %s\n' "$*" >&2
  return 1
}

selected_by_path() {
  case "$1" in
    src/SIR.Simulation/CombatRules.fs|tests/SIR.Conformance.Shared/QuintReplayFixtures.fs|tests/fixtures/rules-corpus/quint-q1/*|scripts/qualify-quint-q1-sir-replay.sh|scripts/test-conformance.sh)
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
dotnet_bin="$(realpath "$dotnet_bin")"
dotnet_root="$(dirname "$dotnet_bin")"
sdk_entry="$dotnet_root/sdk/$expected_sdk/dotnet.dll"
hostfxr="$(find "$dotnet_root/host/fxr" -type f -name 'libhostfxr.so' -print | sort -V | tail -1)"
runtime_root="$(find "$dotnet_root/shared/Microsoft.NETCore.App" -mindepth 1 -maxdepth 1 -type d -name '10.0.*' -print | sort -V | tail -1)"
[[ -f "$sdk_entry" && -f "$hostfxr" && -d "$runtime_root" ]] || fail "executed .NET toolchain closure is incomplete"

tree_digest() {
  local root=$1
  (cd "$root" && find . -type f -print0 | sort -z | xargs -0 sha256sum | sha256sum | cut -d' ' -f1)
}

lock_digest="$({
  for lock in \
    "$repo_root/tests/SIR.Domain.Tests/packages.lock.json" \
    "$repo_root/src/SIR.Domain/packages.lock.json" \
    "$repo_root/src/SIR.Simulation/packages.lock.json"
  do
    printf '%s  %s\n' "$(sha256sum "$lock" | cut -d' ' -f1)" "${lock#$repo_root/}"
  done
} | sha256sum | cut -d' ' -f1)"
toolchain_json="$(jq -n \
  --arg muxerPath "$dotnet_bin" \
  --arg muxerSha256 "$(sha256sum "$dotnet_bin" | cut -d' ' -f1)" \
  --arg sdkVersion "$expected_sdk" \
  --arg sdkEntrySha256 "$(sha256sum "$sdk_entry" | cut -d' ' -f1)" \
  --arg hostfxrPath "$hostfxr" \
  --arg hostfxrSha256 "$(sha256sum "$hostfxr" | cut -d' ' -f1)" \
  --arg runtimePath "$runtime_root" \
  --arg runtimeTreeSha256 "$(tree_digest "$runtime_root")" \
  --arg packageLocksSha256 "$lock_digest" \
  '{muxerPath:$muxerPath,muxerSha256:$muxerSha256,sdkVersion:$sdkVersion,sdkEntrySha256:$sdkEntrySha256,hostfxrPath:$hostfxrPath,hostfxrSha256:$hostfxrSha256,runtimePath:$runtimePath,runtimeTreeSha256:$runtimeTreeSha256,packageLocksSha256:$packageLocksSha256}')"
selected_by_path src/SIR.Simulation/CombatRules.fs || fail "combat implementation change did not select replay"
if selected_by_path src/SIR.Client/UnrelatedView.fs; then
  fail "unrelated implementation change selected replay"
fi

"$dotnet_bin" restore "$project" --locked-mode >/dev/null
"$dotnet_bin" build "$project" --no-restore --configuration Release >/dev/null

"$dotnet_bin" "$dll" --quint-q1-replay >"$run_root/receipt-a.raw.json"
"$dotnet_bin" "$dll" --quint-q1-replay >"$run_root/receipt-b.raw.json"
jq --argjson toolchain "$toolchain_json" '.toolchain = $toolchain' "$run_root/receipt-a.raw.json" >"$run_root/receipt-a.json"
jq --argjson toolchain "$toolchain_json" '.toolchain = $toolchain' "$run_root/receipt-b.raw.json" >"$run_root/receipt-b.json"
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
  grep -F 'first divergence: fixture=' "$log" >/dev/null \
    || { cat "$log" >&2; fail "$mutation did not report the first transition"; }
  grep -F ' transition=' "$log" >/dev/null \
    || { cat "$log" >&2; fail "$mutation did not identify the transition index"; }
  grep -F "mutation=$mutation" "$log" >/dev/null \
    || { cat "$log" >&2; fail "$mutation diagnostic did not name its binding"; }
  grep -F 'fixture=tests/fixtures/rules-corpus/quint-q1/' "$log" >/dev/null \
    || { cat "$log" >&2; fail "$mutation diagnostic did not locate its fixture"; }
  grep -F 'pointer=/states/' "$log" >/dev/null \
    || { cat "$log" >&2; fail "$mutation diagnostic did not name its JSON pointer"; }
  grep -F 'adapter=tests/SIR.Conformance.Shared/QuintReplayFixtures.fs:applyDamageWith' "$log" >/dev/null \
    || { cat "$log" >&2; fail "$mutation diagnostic did not name its adapter source"; }
  grep -F 'implementation=src/SIR.Simulation/CombatRules.fs:CombatRules.resolveConsequences' "$log" >/dev/null \
    || { cat "$log" >&2; fail "$mutation diagnostic did not name its implementation source"; }
  printf 'SIR-Q1-MUTATION-PASS: %s\n' "$mutation"
done

if [[ -n "${SIR_Q1_CI_SAMPLES:-}" ]]; then
  sample_count="$SIR_Q1_CI_SAMPLES"
  [[ "$sample_count" =~ ^[1-9][0-9]*$ ]] || fail "SIR_Q1_CI_SAMPLES must be a positive integer"
  [[ "$(uname -s)/$(uname -m)" == "Linux/x86_64" ]] || fail "sampled Q1 toolchain supports only Linux/x86_64"
  quint_sha="939b64095b706017f2f202c6f99c860c40be7c31bddc2b98557316e50f42cd7f"
  evaluator_archive_sha="61755a09d5052d93a4e75e840059edfd0d3674aeda164b9d2464be3d6e21b1c2"
  evaluator_sha="b2efdeac5713d153e41bf2143b94ed75d888fdd5637f4a5d61a04c695313510a"
  quint_bin="${QUINT_BIN:-$run_root/quint-linux-amd64}"
  if [[ -z "${QUINT_BIN:-}" ]]; then
    curl --fail --silent --show-error --location \
      https://github.com/quint-co/quint/releases/download/v0.32.0/quint-linux-amd64 \
      --output "$quint_bin"
    chmod +x "$quint_bin"
  fi
  [[ "$(sha256sum "$quint_bin" | cut -d' ' -f1)" == "$quint_sha" ]] || fail "sampled Quint binary digest mismatch"
  [[ "$(sha256sum "$model" | cut -d' ' -f1)" == "39b06159db6f0a76b8cc1d7b60eed6155a33539f9c47cb4d4079a915ad4a72d5" ]] \
    || fail "sampled producer model digest mismatch"

  export QUINT_HOME="$run_root/quint-home"
  evaluator_dir="$QUINT_HOME/rust-evaluator-v0.6.0"
  mkdir -p "$evaluator_dir"
  evaluator_bin="${QUINT_RUST_EVALUATOR_BIN:-$evaluator_dir/quint_evaluator}"
  if [[ -z "${QUINT_RUST_EVALUATOR_BIN:-}" ]]; then
    evaluator_archive="$run_root/quint-evaluator.tar.gz"
    curl --fail --silent --show-error --location \
      https://github.com/quint-co/quint/releases/download/evaluator/v0.6.0/quint_evaluator-x86_64-unknown-linux-gnu.tar.gz \
      --output "$evaluator_archive"
    [[ "$(sha256sum "$evaluator_archive" | cut -d' ' -f1)" == "$evaluator_archive_sha" ]] \
      || fail "sampled evaluator archive digest mismatch"
    tar -xzf "$evaluator_archive" -C "$evaluator_dir"
  else
    ln -s "$(realpath "$evaluator_bin")" "$evaluator_dir/quint_evaluator"
    evaluator_bin="$evaluator_dir/quint_evaluator"
  fi
  [[ "$(sha256sum "$evaluator_bin" | cut -d' ' -f1)" == "$evaluator_sha" ]] || fail "sampled evaluator digest mismatch"

  sample_root="$run_root/sampled-itf"
  mkdir -p "$sample_root"
  "$quint_bin" run "$model" --main SirDamageSlice --max-samples "$sample_count" \
    --n-traces "$sample_count" --max-steps 8 --seed 92220 \
    --invariants nonNegativeHitPoints knownLastAction --backend rust \
    --out-itf "$sample_root/sample_{seq}.itf.json" --verbosity 1 >"$run_root/quint-sampled.log" 2>&1
  for sample in "$sample_root"/sample_*.itf.json; do
    jq 'del(."#meta".description, ."#meta".timestamp) | ."#meta".source = (."#meta".source | split("/") | last)' \
      "$sample" >"$sample.normalized"
    mv "$sample.normalized" "$sample"
  done
  sampled_output="$("$dotnet_bin" "$dll" --quint-q1-sampled "$sample_root" "$sample_count")"
  sampled_states="$(printf '%s\n' "$sampled_output" | sed -n 's/.* states=\([0-9][0-9]*\).*/\1/p')"
  [[ -n "$sampled_states" ]] || fail "sampled runtime replay emitted no state count"
  sampled_corpus_sha="$(cd "$sample_root" && sha256sum sample_*.itf.json | sha256sum | cut -d' ' -f1)"
  sampled_receipt="$(jq -n \
    --arg schema 'fsgg.quint.sir-sampled-itf-replay-receipt/q1' \
    --arg quintSha256 "$quint_sha" --arg evaluatorSha256 "$evaluator_sha" \
    --arg modelSha256 '39b06159db6f0a76b8cc1d7b60eed6155a33539f9c47cb4d4079a915ad4a72d5' \
    --arg corpusSha256 "$sampled_corpus_sha" --arg seed '92220' \
    --argjson traces "$sample_count" --argjson states "$sampled_states" \
    '{schema:$schema,quintVersion:"0.32.0",quintSha256:$quintSha256,evaluatorVersion:"0.6.0",evaluatorSha256:$evaluatorSha256,modelSha256:$modelSha256,corpusSha256:$corpusSha256,seed:$seed,maxSteps:8,traces:$traces,states:$states,verdict:"accept-sampled-runtime-correspondence"}')"
  if [[ -n "${SIR_Q1_SAMPLED_RECEIPT_OUT:-}" ]]; then
    mkdir -p "$(dirname "$SIR_Q1_SAMPLED_RECEIPT_OUT")"
    printf '%s\n' "$sampled_receipt" >"$SIR_Q1_SAMPLED_RECEIPT_OUT"
  fi
  printf 'SIR-Q1-SAMPLED-QUALIFIED: traces=%s states=%s\n' "$sample_count" "$sampled_states"
fi

if [[ -n "${SIR_Q1_RECEIPT_OUT:-}" ]]; then
  mkdir -p "$(dirname "$SIR_Q1_RECEIPT_OUT")"
  cp "$run_root/receipt-a.json" "$SIR_Q1_RECEIPT_OUT"
fi

if [[ -n "${SIR_Q1_JUNIT_OUT:-}" ]]; then
  mkdir -p "$(dirname "$SIR_Q1_JUNIT_OUT")"
  junit_tmp="$SIR_Q1_JUNIT_OUT.tmp"
  {
    printf '%s\n' '<?xml version="1.0" encoding="UTF-8"?>'
    junit_tests=6
    [[ -n "${SIR_Q1_CI_SAMPLES:-}" ]] && junit_tests=7
    printf '<testsuite name="sir-quint-q1-runtime-replay" tests="%d" failures="0" errors="0" skipped="0">\n' "$junit_tests"
    printf '%s\n' '  <testcase classname="SIR.QuintQ1" name="exact-runtime-replay"/>'
    for mutation in "${mutations[@]}"; do
      printf '  <testcase classname="SIR.QuintQ1" name="mutation-%s"/>\n' "$mutation"
    done
    if [[ -n "${SIR_Q1_CI_SAMPLES:-}" ]]; then
      printf '%s\n' '  <testcase classname="SIR.QuintQ1" name="sampled-itf-runtime-replay"/>'
    fi
    printf '%s\n' '</testsuite>'
  } >"$junit_tmp"
  mv "$junit_tmp" "$SIR_Q1_JUNIT_OUT"
fi

printf 'SIR-Q1-QUALIFIED: 1 exact trace, 3 states, 5 mapping/implementation mutations\n'
