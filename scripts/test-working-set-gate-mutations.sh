#!/usr/bin/env bash
# Gate-inversion evidence for the working-set gates added by S.I.R.#249.
#
# WHY THIS EXISTS. `pnext-item` §3 requires that every gate a change adds ships with evidence it can
# FAIL, and a gate whose inversion survives is a material review finding by definition. That evidence
# is worth nothing if it lives only in a pull-request body: a table recording what its author once ran
# is a record, not a gate, and nobody else can re-run it. `scripts/test-spatial-subject-mutations.sh`
# is the committed form for this repository's pre-existing spatial gates; this is the same thing for
# the three gates #249 added.
#
# Each case mutates ONE source line so the property under test becomes false, runs the suite that owns
# the assertion, and requires that it fails WITH THE EXPECTED MESSAGE. A mutation that fails for some
# other reason is treated as a failure of this harness, not as a pass, because it would not prove the
# gate detected anything.
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
simulation="$repo_root/src/SIR.Simulation/Simulation.fs"
spatial="$repo_root/src/SIR.Simulation/SpatialQuery.fs"
temporary_dir=$(mktemp -d /tmp/sir-working-set-gate-mutations.XXXXXX)

cp -p "$simulation" "$temporary_dir/Simulation.fs"
cp -p "$spatial" "$temporary_dir/SpatialQuery.fs"

restore() {
  cp -p "$temporary_dir/Simulation.fs" "$simulation"
  cp -p "$temporary_dir/SpatialQuery.fs" "$spatial"
  touch "$simulation" "$spatial"
}

cleanup() {
  restore
  rm -rf -- "$temporary_dir"
}
trap cleanup EXIT

conformance="$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"
match="$repo_root/tests/SIR.Match.Tests/SIR.Match.Tests.fsproj"

# A mutation that did not change its subject tests NOTHING, and would then "pass" this harness by
# simply running the unmutated suite and watching it stay green. Refuse that outcome loudly: a sed or
# patch whose pattern has drifted away from the source must report itself invalid, never quietly
# succeed. This is the failure mode that makes a mutation harness worse than none, because it reports
# coverage it does not have.
require_mutated() {
  local name=$1 subject=$2
  if cmp -s "$subject" "$temporary_dir/$(basename "$subject")"; then
    echo "working-set gate mutation did not modify its subject: $name ($subject)" >&2
    echo "  the mutation pattern no longer matches the source; fix the pattern, do not skip the case" >&2
    exit 1
  fi
}

expect_failure() {
  local name=$1 project=$2 expected=$3
  local log="$temporary_dir/$name.log"
  if dotnet run --project "$project" -c Release -- >"$log" 2>&1; then
    echo "working-set gate mutation unexpectedly passed: $name" >&2
    exit 1
  fi
  grep -F -- "$expected" "$log" >/dev/null || {
    echo "working-set gate mutation failed for the wrong reason: $name" >&2
    echo "  expected to find: $expected" >&2
    tail -20 "$log" >&2
    exit 1
  }
  echo "  $name: failed closed"
}

# AC 1 - the projected spatial world is constructed at most once per observation phase. Inverting the
# hoist so the fold builds a world per observation pair must be detected by the counting fixture.
sed -i 's|let visibility, _ = SpatialQuery.evaluate world request|let visibility, _ = SpatialQuery.evaluate (worldFor ()) request|' "$simulation"
require_mutated world-construction-count "$simulation"
expect_failure world-construction-count "$conformance" "The observation phase constructed the spatial world"
restore

# AC 2 - boundary indexing must answer first-declaration-wins, exactly as the `List.tryFind` scan it
# replaces. Building the index last-wins - which is what `Map.ofList` would do - must be detected.
sed -i 's|if Map.containsKey boundary.Edge index then index else Map.add boundary.Edge boundary index)|Map.add boundary.Edge boundary index)|' "$spatial"
require_mutated boundary-index-first-wins "$spatial"
expect_failure boundary-index-first-wins "$conformance" "Indexed boundary resolution stopped answering first-declaration-wins."
restore

# AC 3 - the dynamic cache tier is bounded. Removing the truncation must be detected by the fixture
# that drives the tier past its ceiling.
python3 - "$spatial" <<'PYTHON'
import sys
path = sys.argv[1]
with open(path, encoding="utf-8") as handle:
    text = handle.read()
old = """                        if exceedsCapacity dynamicCacheCapacity grown
                        then List.truncate dynamicCacheCapacity grown
                        else grown"""
if text.count(old) != 1:
    sys.exit("dynamic-tier bound mutation could not locate its subject")
with open(path, "w", encoding="utf-8") as handle:
    handle.write(text.replace(old, "                        grown"))
PYTHON
require_mutated dynamic-cache-bound "$spatial"
expect_failure dynamic-cache-bound "$match" "The dynamic spatial cache tier was not bounded"
restore

echo "Working-set gate mutations failed closed: world-construction count, boundary first-wins, dynamic cache bound."
