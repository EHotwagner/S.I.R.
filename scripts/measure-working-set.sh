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
# THE BASE IS REQUIRED, AND THE TWO SIDES MUST ACTUALLY DIFFER. This script used to default
# `<before-ref>` to `origin/main` and resolve it at read time, with nothing checking that the two
# sides were different code. That is a latent lie with a fuse on it: the moment this branch merges,
# `origin/main` CONTAINS the change, `before` becomes byte-identical to `after`, every workload
# reports ~1.00x, and the script still exits 0. A harness that reports "no difference" when it is
# comparing something to itself does not fail - it silently succeeds, and the absence of a difference
# reads as a result. So the base is now mandatory, and the guard below refuses the degenerate case.
#
# THE GUARD IS ON BLOBS, NOT ON COMMITS, and that distinction is load-bearing. Two different commits
# routinely carry identical text for these two files - the S.I.R.#249 review found the swapped
# sources unchanged across 190 commits - so "the refs differ" does NOT imply "the sides differ".
# What makes before/after meaningful is that the SWAPPED SOURCES differ, which is what is checked.
#
# Usage: scripts/measure-working-set.sh <before-ref> [rounds]
#   <before-ref>  REQUIRED. A Git ref supplying the comparison implementation, e.g. origin/main.
#                 Refused if its copies of the swapped sources are identical to the working tree's.
#   [rounds]      interleaved repetitions per workload (default 3)
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
if [[ $# -lt 1 || -z "${1:-}" ]]; then
  echo "measure-working-set: a <before-ref> is REQUIRED - refusing to guess a baseline" >&2
  echo "  usage: scripts/measure-working-set.sh <before-ref> [rounds]" >&2
  echo "  e.g.:  scripts/measure-working-set.sh origin/main 3" >&2
  exit 2
fi
before_ref=$1
rounds=${2:-3}
[[ "$rounds" =~ ^[1-9][0-9]*$ ]] || { echo "measure-working-set: rounds must be a positive integer" >&2; exit 2; }

project="$repo_root/tests/SIR.PhysicalCombat.Performance/SIR.PhysicalCombat.Performance.fsproj"
bin="$repo_root/tests/SIR.PhysicalCombat.Performance/bin/Release/net10.0"
temporary_dir=$(mktemp -d /tmp/sir-working-set.XXXXXX)

sources=(src/SIR.Simulation/SpatialQuery.fs src/SIR.Simulation/Simulation.fs)

# Resolve the base ONCE, to an immutable commit id, so a moving ref cannot mean two different things
# between the guard below and the build above, and so the recorded provenance names a commit rather
# than a ref that will have moved by the time anyone reads the numbers.
before_sha=$(git -C "$repo_root" rev-parse --verify "$before_ref^{commit}" 2>/dev/null) || {
  echo "measure-working-set: cannot resolve <before-ref> '$before_ref' to a commit" >&2
  exit 2
}
head_sha=$(git -C "$repo_root" rev-parse --verify HEAD^{commit})

# The degenerate-comparison guard. Compare the BLOBS of the swapped sources, because those - and only
# those - are what the two sides actually differ by. A base whose copies of both files match the
# working tree produces before == after and a uniform ~1.00x that reads as "no regression".
differing=()
for source in "${sources[@]}"; do
  base_blob=$(git -C "$repo_root" rev-parse --verify --quiet "$before_sha:$source") || {
    echo "measure-working-set: '$source' does not exist at $before_ref ($before_sha)" >&2
    exit 2
  }
  tree_blob=$(git -C "$repo_root" hash-object "$repo_root/$source")
  if [[ "$base_blob" != "$tree_blob" ]]; then differing+=("$source"); fi
done
if [[ ${#differing[@]} -eq 0 ]]; then
  echo "measure-working-set: REFUSING a degenerate comparison - the working tree and $before_ref" >&2
  echo "  ($before_sha) carry byte-identical copies of every swapped source:" >&2
  for source in "${sources[@]}"; do echo "    $source" >&2; done
  echo "  'before' would be the same code as 'after', every workload would report ~1.00x, and that" >&2
  echo "  absence of a difference would read as a result. Name a base from BEFORE the change." >&2
  exit 3
fi

# Provenance FIRST, so the numbers below are re-derivable even if the run is interrupted. Without a
# recorded base SHA the figures cannot be reproduced by anyone who did not run them.
echo "working-set-measurement base-ref=$before_ref base-sha=$before_sha head-sha=$head_sha rounds=$rounds"
for source in "${sources[@]}"; do
  echo "working-set-measurement swapped-source=$source base-blob=$(git -C "$repo_root" rev-parse "$before_sha:$source") tree-blob=$(git -C "$repo_root" hash-object "$repo_root/$source")"
done
echo "working-set-measurement differing-sources=${#differing[@]}/${#sources[@]}"
for source in "${sources[@]}"; do
  cp -p "$repo_root/$source" "$temporary_dir/$(basename "$source").after"
done

restore() {
  for source in "${sources[@]}"; do
    # NOT `cp -p`. Preserving the original mtime here restores a file that looks OLDER than the
    # build outputs already on disk, so the NEXT incremental build considers it up to date and
    # quietly keeps the previous side's assembly. That is how a repeated run degenerates into
    # measuring one side against itself, which reports ~1.00x and exits 0.
    cp "$temporary_dir/$(basename "$source").after" "$repo_root/$source"
    touch "$repo_root/$source"
  done
}
cleanup() {
  restore
  rm -rf -- "$temporary_dir"
}
trap cleanup EXIT

build_side() {
  local label=$1
  # Force the swapped sources to be newer than any existing output, so the incremental build cannot
  # decide this side is already built and hand back the other side's assembly.
  touch "${sources[@]/#/$repo_root/}"
  dotnet build "$project" -c Release >/dev/null
  cp "$bin/SIR.Simulation.dll" "$temporary_dir/SIR.Simulation.$label.dll"
  printf 'working-set-measurement side=%s assembly-sha256=%s\n' \
    "$label" "$(sha256sum "$temporary_dir/SIR.Simulation.$label.dll" | cut -d" " -f1)"
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

# THE GUARD THAT MATTERS, because it is on the thing actually executed. The source-blob check above
# proves the two checkouts differ; it cannot prove the two BUILDS differ. An incremental build that
# decided a side was already up to date, or a dependency copy that was skipped because the
# destination looked newer, yields two identical assemblies from two different sources - and every
# workload then reports ~1.00x with a perfectly valid base. Compare the assemblies themselves.
before_dll_sha=$(sha256sum "$temporary_dir/SIR.Simulation.before.dll" | cut -d" " -f1)
after_dll_sha=$(sha256sum "$temporary_dir/SIR.Simulation.after.dll" | cut -d" " -f1)
if [[ "$before_dll_sha" == "$after_dll_sha" ]]; then
  echo "measure-working-set: REFUSING - the two sides built to the SAME assembly ($before_dll_sha)." >&2
  echo "  The sources differ, so this is a stale or skipped build, not a degenerate base. Every" >&2
  echo "  workload would report ~1.00x and that would read as 'no regression'. Re-run after a" >&2
  echo "  'dotnet clean -c Release' on $project." >&2
  exit 4
fi
echo "working-set-measurement before-assembly=$before_dll_sha after-assembly=$after_dll_sha assemblies-differ=yes"

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
echo "Working-set measurement complete: $rounds interleaved round(s), one workload per process, before=$before_ref ($before_sha), head=$head_sha."
