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

# Restoring the SOURCES is not enough: the last mutant this harness built is still sitting in
# `bin/`, and any later step that runs with `--no-build` will happily execute it. That is not
# hypothetical - `verify-spatial-query.sh` runs `--print-spatial-performance --no-build` after
# calling this script, and is safe today only because an unrelated `dotnet build` happens to sit
# between them. A harness that leaves a booby-trapped binary behind for the next step to trip over
# is not self-contained, so rebuild the restored sources before handing control back.
cleanup() {
  restore
  dotnet build "$conformance" -c Release >/dev/null 2>&1 || true
  dotnet build "$match" -c Release >/dev/null 2>&1 || true
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
  shift 3
  local log="$temporary_dir/$name.log"
  if dotnet run --project "$project" -c Release -- "$@" >"$log" 2>&1; then
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

# AC 4 - F4: `neighbours` is evaluated ONCE per expansion in `boundedPath`'s fallback loop. This
# property has NO result-level signature. `neighbours` is deterministic and side-effect free, so
# evaluating it twice leaves the path, the cost, the expansion count and the canonical bytes
# byte-identical - every existing conformance assertion stays green. ALLOCATION is the signature it
# does have, which is why the gate is a byte budget rather than an equality.
#
# The mutation below is the exact regression the budget exists to catch: restore the second full
# `neighbours` evaluation that F4 removed, in the `nextBest` fold. Measured, this moves the workload
# from 15,656,832 bytes to 26,831,424 (+71%) while boundaries=184, expansions=545, path-cells=35,
# cost=34 and outcome=Found ALL stay identical - so this case is also the standing proof that the
# structural counters cannot see it and the budget can.
python3 - "$spatial" <<'PYTHON'
import sys
path = sys.argv[1]
with open(path, encoding="utf-8") as handle:
    text = handle.read()
old = """                    let nextBest =
                        expanded"""
if text.count(old) != 1:
    sys.exit("neighbours-once-per-expansion mutation could not locate its subject")
new = """                    let nextBest =
                        neighbours observeCell observeBoundary resolveBoundary world request footprint current"""
with open(path, "w", encoding="utf-8") as handle:
    handle.write(text.replace(old, new))
PYTHON
require_mutated neighbours-once-per-expansion "$spatial"
expect_failure neighbours-once-per-expansion "$conformance" \
  "Spatial bounded-path allocation budget exceeded" --print-spatial-performance
restore

# AC 1 - F1 on the PRODUCTION route. `world-construction-count` above injects its own world factory
# into `observationPhaseWith`, so it proves the HELPER hoists and is blind to how the helper is
# WIRED. This mutation reverts F1 in the private `observationPhase` that `runTick` actually calls,
# leaving `observationPhaseWith` byte-identical. Measured: the whole canonical conformance corpus
# stays byte-identical and exits 0, `world-construction-count` still sees one construction, and
# nothing reds - except the allocation budget, which moves 17,038,984 -> 20,847,776 (+22%) while
# board-edges, observations, observed and events all stay identical.
python3 - "$simulation" <<'PYTHON'
import sys
path = sys.argv[1]
with open(path, encoding="utf-8") as handle:
    text = handle.read()
old = """    let private observationPhase (state: SimulationState) inputs =
        observationPhaseWith (fun () -> spatialWorld state.Tick state.Board) state inputs"""
if text.count(old) != 1:
    sys.exit("production-route world-construction mutation could not locate its subject")
new = """    let private observationPhase (state: SimulationState) inputs =
        let observations =
            inputs
            |> List.choose (function
                | Observe(observerId, targetId) -> Some(observerId, targetId)
                | _ -> None)
        if List.isEmpty observations then state, []
        else
        ((state, []), observations)
        ||> List.fold (fun (current, events) (observerId, targetId) ->
            match tryUnit observerId current, tryUnit targetId current with
            | Some observer, Some target ->
                let request = spatialRequest "simulation-observation" SpatialQueryKind.ExactLineOfSight SpatialModality.Vision observer.Cell target.Cell
                let visibility, _ = SpatialQuery.evaluate (spatialWorld state.Tick state.Board) request
                let visible = visibility.Outcome = SpatialOutcome.Found && visibility.Visible
                if visible then
                    let distance = chebyshevDistance observer.Cell target.Cell |> int32
                    let observed = Set.add (observerId, targetId) current.Observations
                    { current with Observations = observed },
                    UnitObserved(observerId, targetId, distance) :: events
                else current, events
            | _ -> current, events)
        |> fun (next, events) -> next, List.rev events"""
with open(path, "w", encoding="utf-8") as handle:
    handle.write(text.replace(old, new))
PYTHON
require_mutated production-route-world-construction "$simulation"
expect_failure production-route-world-construction "$conformance" \
  "Authoritative tick allocation budget exceeded" --print-spatial-performance
restore

echo "Working-set gate mutations failed closed: world-construction count, production-route world construction, boundary first-wins, dynamic cache bound, neighbours once per expansion."
