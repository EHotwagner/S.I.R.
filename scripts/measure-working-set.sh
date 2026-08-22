#!/usr/bin/env bash
# Interleaved before/after measurement of the S.I.R.#249 working-set workloads.
#
# ONE WORKLOAD PER PROCESS, AND THAT IS THE WHOLE POINT. Running the four workloads together in a
# single process made this measurement LIE: the trace and path workloads allocate a boundary index per
# evaluation, and the heap state they left behind slowed whatever ran after them by roughly 2x. That
# read as a regression in the cache and tick workloads - in code those workloads never execute. Four
# interleaved rounds agreed with each other and were still wrong, and `GC.Collect` between workloads
# did not settle it. Every workload therefore gets its own process, and the two sides are interleaved
# so that machine load falls on both equally rather than on whichever ran second.
#
# Usage: scripts/measure-working-set.sh <before-ref> [rounds]
#   <before-ref>  a Git ref supplying the comparison implementation, e.g. origin/main
#   [rounds]      interleaved repetitions per workload (default 3)
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
before_ref=${1:-origin/main}
rounds=${2:-3}
[[ "$rounds" =~ ^[1-9][0-9]*$ ]] || { echo "measure-working-set: rounds must be a positive integer" >&2; exit 2; }

project="$repo_root/tests/SIR.PhysicalCombat.Performance/SIR.PhysicalCombat.Performance.fsproj"
bin="$repo_root/tests/SIR.PhysicalCombat.Performance/bin/Release/net10.0"
temporary_dir=$(mktemp -d /tmp/sir-working-set.XXXXXX)

sources=(src/SIR.Simulation/SpatialQuery.fs src/SIR.Simulation/Simulation.fs)
for source in "${sources[@]}"; do
  cp -p "$repo_root/$source" "$temporary_dir/$(basename "$source").after"
done

restore() {
  for source in "${sources[@]}"; do
    cp -p "$temporary_dir/$(basename "$source").after" "$repo_root/$source"
  done
}
cleanup() {
  restore
  rm -rf -- "$temporary_dir"
}
trap cleanup EXIT

build_side() {
  local label=$1
  dotnet build "$project" -c Release >/dev/null
  cp "$bin/SIR.Simulation.dll" "$temporary_dir/SIR.Simulation.$label.dll"
}

# The harness binary is built once and never swapped, so both sides run identical measurement code;
# only the implementation assembly under test changes.
build_side after
for source in "${sources[@]}"; do
  git -C "$repo_root" show "$before_ref:$source" > "$repo_root/$source"
done
build_side before
restore
dotnet build "$project" -c Release >/dev/null

for round in $(seq 1 "$rounds"); do
  for workload in f2f5 f4 f3 f1f6; do
    for side in before after; do
      cp --remove-destination "$temporary_dir/SIR.Simulation.$side.dll" "$bin/SIR.Simulation.dll"
      dotnet "$bin/SIR.PhysicalCombat.Performance.dll" --working-set "$workload" \
        | sed "s/^working-set /round$round $side /"
    done
  done
done

cp --remove-destination "$temporary_dir/SIR.Simulation.after.dll" "$bin/SIR.Simulation.dll"
echo "Working-set measurement complete: $rounds interleaved round(s), one workload per process, before=$before_ref."
