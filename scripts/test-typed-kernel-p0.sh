#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_tmp=$(mktemp -d /tmp/sir-typed-kernel-p0-mutations.XXXXXX)
trap 'rm -rf -- "$task_tmp"' EXIT

"$repo_root/scripts/capture-typed-kernel-p0.sh" --check --skip-parity

expect_red() {
  local name=$1
  local expected=$2
  local selection=$3
  local baseline=$4
  if SIR_TYPED_KERNEL_P0_SELECTION="$selection" SIR_TYPED_KERNEL_P0_BASELINE="$baseline" \
    "$repo_root/scripts/capture-typed-kernel-p0.sh" --check --skip-parity >"$task_tmp/$name.log" 2>&1; then
    echo "typed-kernel P0 mutation unexpectedly passed: $name" >&2
    exit 1
  fi
  rg -F "$expected" "$task_tmp/$name.log" >/dev/null || {
    echo "typed-kernel P0 mutation failed without its named diagnostic: $name" >&2
    sed -n '1,80p' "$task_tmp/$name.log" >&2
    exit 1
  }
}

selection="$repo_root/tests/fixtures/typed-kernel-p0/selection.json"
baseline="$repo_root/tests/fixtures/typed-kernel-p0/baseline.json"

jq 'del(.surfaces[] | select(.class == "registered-algorithm"))' "$selection" > "$task_tmp/missing-class.json"
expect_red missing-class "omitted required surface class: registered-algorithm" "$task_tmp/missing-class.json" "$baseline"

jq '(.surfaces[] | select(.id == "COMBAT-DAMAGE-001") | .expectedKind) = "fact"' "$selection" > "$task_tmp/wrong-kind.json"
expect_red wrong-kind "kind mismatch for COMBAT-DAMAGE-001" "$task_tmp/wrong-kind.json" "$baseline"

jq '.artifacts[0].sha256 = "0000000000000000000000000000000000000000000000000000000000000000"' "$baseline" > "$task_tmp/stale-baseline.json"
expect_red stale-baseline "baseline is stale" "$selection" "$task_tmp/stale-baseline.json"

printf 'Typed-kernel P0 mutation controls verified: missing class, wrong kind, and stale baseline all red.\n'
