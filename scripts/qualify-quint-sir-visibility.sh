#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
authority="$repo_root/docs/rules/sir-visibility.md"
task_tmp="$(mktemp -d)"
trap 'rm -rf "$task_tmp"' EXIT

fail() {
  echo "quint-sir-visibility: $*" >&2
  exit 1
}

test -f "$authority" || fail "missing literate authority"
command -v quint >/dev/null 2>&1 || fail "quint is not installed"
test "$(quint --version)" = "0.32.0" || fail "expected Quint 0.32.0"
grep -F '<PackageVersion Include="FS.GG.Game.Core" Version="[0.13.0]" />' \
  "$repo_root/Directory.Packages.props" >/dev/null || fail "expected FS.GG.Game.Core 0.13.0"
grep -F 'Los.lineOfSightBy Supercover transparent origin target' \
  "$repo_root/src/SIR.Simulation/SpatialQuery.fs" >/dev/null || fail "production supercover call drifted"
grep -F 'let private lineCells origin target =' \
  "$repo_root/src/SIR.Simulation/SpatialQuery.fs" >/dev/null || fail "production sampled-line boundary drifted"

diagram_ids=(blocker-semantics canonical-symmetry evaluator-pipeline footprint-exposure supercover-walk two-traces work-bound)
tooltip_hotspots=0
for id in "${diagram_ids[@]}"; do
  svg="$repo_root/docs/assets/sir-visibility-quint/$id.svg"
  test -f "$svg" || fail "missing visibility diagram: $id"
  grep -F 'role="img"' "$svg" >/dev/null || fail "diagram lacks image role: $id"
  grep -F 'aria-labelledby=' "$svg" >/dev/null || fail "diagram lacks accessible binding: $id"
  grep -F '<title id=' "$svg" >/dev/null || fail "diagram lacks title: $id"
  grep -F '<desc id=' "$svg" >/dev/null || fail "diagram lacks description: $id"
  grep -F 'prefers-reduced-motion:reduce' "$svg" >/dev/null || fail "diagram lacks reduced-motion fallback: $id"
  grep -F '@media print' "$svg" >/dev/null || fail "diagram lacks print fallback: $id"
  grep -F 'id="effects-off"' "$svg" >/dev/null || fail "diagram lacks effects-off target: $id"
  tooltip_count="$(awk '{ count += gsub(/data-tooltip=/, "") } END { print count + 0 }' "$svg")"
  title_count="$(awk '{ count += gsub(/<title([ >])/, "") } END { print count + 0 }' "$svg")"
  (( tooltip_count >= 8 )) || fail "diagram lacks dense tooltip coverage: $id ($tooltip_count; expected at least 8)"
  (( title_count >= tooltip_count + 1 )) \
    || fail "diagram tooltip titles are incomplete: $id ($title_count titles for $tooltip_count hotspots)"
  tooltip_hotspots=$((tooltip_hotspots + tooltip_count))
  grep -F "data-diagram-embed=\"$id\"" "$repo_root/docs/sir-combat-quint-handbook.md" >/dev/null \
    || fail "handbook embed missing: $id"
  grep -F "<object type=\"image/svg+xml\" data=\"assets/sir-visibility-quint/$id.svg\"" \
    "$repo_root/docs/sir-combat-quint-handbook.md" >/dev/null \
    || fail "handbook interactive SVG object missing: $id"
  if grep -F "<img src=\"assets/sir-visibility-quint/$id.svg\"" \
      "$repo_root/docs/sir-combat-quint-handbook.md" >/dev/null; then
    fail "handbook uses a non-interactive image embed: $id"
  fi
  grep -F "id=\"diagram-transcript-$id\"" "$repo_root/docs/sir-combat-quint-handbook.md" >/dev/null \
    || fail "handbook transcript missing: $id"
done
(( tooltip_hotspots >= 60 )) \
  || fail "visibility diagrams lack dense aggregate tooltip coverage: $tooltip_hotspots (expected at least 60)"

extract() {
  local output="$1"
  awk '
    /^```quint sir-visibility.qnt \+=$/ { inside = 1; found = 1; next }
    /^```quint / { if (!inside) unexpected = 1 }
    /^```$/ { if (inside) { inside = 0; next } }
    inside { print }
    END { if (!found || inside || unexpected) exit 1 }
  ' "$authority" > "$output"
}

extract "$task_tmp/first.qnt" || fail "literate fences are incomplete or mixed"
extract "$task_tmp/second.qnt" || fail "second deterministic extraction failed"
cmp -s "$task_tmp/first.qnt" "$task_tmp/second.qnt" || fail "two clean extractions differ"

model="$task_tmp/first.qnt"
quint typecheck "$model" > "$task_tmp/typecheck.log"

tests='^(openLineIsVisible|xFirstCornerTieIsStable|interiorCellBlocks|endpointsDoNotBlock|opaqueEdgeBlocks|boundaryChecksUseTheSampledLine|supercoverClosesCornerGap|canonicalizationMakesVisibilitySymmetric|oneFootprintPairCanExposeTarget|crossedItemBoundExhaustsBeforeEvaluation|structuralPropertiesHold|workAccountingBoundaryIsExplicit|boundedGridPropertiesHold)$'
quint test "$model" --main=SirVisibilityTests --backend=rust --match="$tests" --verbosity=2 \
  > "$task_tmp/tests.log"
grep -F '13 passing' "$task_tmp/tests.log" >/dev/null || fail "the thirteen named model tests did not pass"

quint run "$model" --main=SirVisibilitySimulation --backend=rust --seed=384 \
  --max-samples=256 --max-steps=6 \
  --invariants storedResultMatchesPureEvaluation truncatedResultIsNeverVisible \
  --witnesses visibleEvaluationReached blockedEvaluationReached exhaustedEvaluationReached \
  --verbosity=2 > "$task_tmp/run.log"
grep -F '[ok] No violation found' "$task_tmp/run.log" >/dev/null || fail "sampled invariants failed"
for witness in visibleEvaluationReached blockedEvaluationReached exhaustedEvaluationReached; do
  grep -F "$witness was witnessed" "$task_tmp/run.log" >/dev/null || fail "$witness was not reached"
done

expect_model_test_red() {
  local mutated="$1"
  local named_test="$2"
  if quint test "$mutated" --main=SirVisibilityTests --backend=rust --match="^${named_test}$" \
      --verbosity=1 > "$mutated.log" 2>&1; then
    fail "mutation for $named_test unexpectedly passed"
  fi
}

sed '0,/comparison <= 0/s//comparison < 0/' "$model" > "$task_tmp/wrong-corner-tie.qnt"
expect_model_test_red "$task_tmp/wrong-corner-tie.qnt" xFirstCornerTieIsStable

sed '0,/index == 0/s//false/' "$model" > "$task_tmp/non-exempt-endpoint.qnt"
expect_model_test_red "$task_tmp/non-exempt-endpoint.qnt" endpointsDoNotBlock

sed '0,/or not(opaqueCells.contains(cells.nth(index))))/s//or true)/' \
  "$model" > "$task_tmp/ignored-terrain.qnt"
expect_model_test_red "$task_tmp/ignored-terrain.qnt" interiorCellBlocks

sed '0,/edgesTransparent(reportedCells, opaqueEdges)/s//edgesTransparent(supercoverCells, opaqueEdges)/' \
  "$model" > "$task_tmp/conflated-traces.qnt"
expect_model_test_red "$task_tmp/conflated-traces.qnt" boundaryChecksUseTheSampledLine

sed '0,/and visibleSamples > 0/s//and visibleSamples == pairs.size()/' \
  "$model" > "$task_tmp/unanimous-footprints.qnt"
expect_model_test_red "$task_tmp/unanimous-footprints.qnt" oneFootprintPairCanExposeTarget

sed '0,/declared > query.maximumCrossedItems/s//false/' \
  "$model" > "$task_tmp/ignored-work-bound.qnt"
expect_model_test_red "$task_tmp/ignored-work-bound.qnt" crossedItemBoundExhaustsBeforeEvaluation

sed '0,/or not(opaqueEdges.contains(canonicalEdge(cells.nth(index), cells.nth(index + 1)))))/s//or true)/' \
  "$model" > "$task_tmp/ignored-boundary.qnt"
expect_model_test_red "$task_tmp/ignored-boundary.qnt" opaqueEdgeBlocks

verification="not-requested"
if [[ "${1:-}" == "--with-verify" ]]; then
  command -v java >/dev/null 2>&1 || fail "--with-verify requires Java 17 or newer for Apalache"
  quint verify "$model" --main=SirVisibilitySimulation --init=init --step=step --max-steps=6 \
    --invariants storedResultMatchesPureEvaluation truncatedResultIsNeverVisible --verbosity=1 \
    > "$task_tmp/verify.log"
  grep -F '[ok] No violation found' "$task_tmp/verify.log" >/dev/null || fail "bounded exhaustive verification failed"
  verification="apalache-bounded"
fi

echo "quint-sir-visibility: PASS (Quint 0.32.0; 13 tests including 625 bounded cell pairs; 2 sampled invariants; 3 witnesses; 7 observed-red mutations; 7 accessible animated SVGs with $tooltip_hotspots detailed tooltip hotspots; verification=$verification)"
