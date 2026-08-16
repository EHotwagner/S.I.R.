#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
subject="$repo_root/src/SIR.Simulation/SpatialQuery.fs"
project="$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"
temporary_dir=$(mktemp -d /tmp/sir-spatial-subject-mutations.XXXXXX)
prepared_pr=false
if [[ "${1:-}" == "--prepared-pr" ]]; then
  prepared_pr=true
  shift
fi
[[ $# -eq 0 ]] || { echo "test-spatial-subject-mutations: usage [--prepared-pr]" >&2; exit 2; }
original="$temporary_dir/SpatialQuery.fs"
cp -p "$subject" "$original"

restore_subject() {
  cp -p "$original" "$subject"
  touch "$subject"
}

cleanup() {
  restore_subject
  rm -rf -- "$temporary_dir"
}
trap cleanup EXIT

expect_mutation_failure() {
  local name=$1
  local expected=$2
  local log="$temporary_dir/$name.log"
  isolated_args=()
  if [[ "$prepared_pr" == true ]]; then isolated_args=(--artifacts-path "$temporary_dir/artifacts/$name"); fi
  if dotnet run --project "$project" -c Release "${isolated_args[@]}" -- --print-spatial-query >"$log" 2>&1; then
    echo "spatial subject mutation unexpectedly passed: $name" >&2
    exit 1
  fi
  grep -F -- "$expected" "$log" >/dev/null || {
    echo "spatial subject mutation failed for the wrong reason: $name" >&2
    cat "$log" >&2
    exit 1
  }
}

sed -i '/observeToken $"occupancy:{position.Col}:{position.Row}"/d' "$subject"
expect_mutation_failure dependency-receipt "An inspected empty cell was not retained as an occupancy-addition dependency."

restore_subject
sed -i 's/footprint |> List.map (addCell anchor)/footprint |> List.truncate 1 |> List.map (addCell anchor)/' "$subject"
expect_mutation_failure footprint-envelope "Footprint evaluation ignored an occupied sample."

restore_subject
sed -i 's/| SpatialModality.GroundMovement -> value.Ground/| SpatialModality.GroundMovement -> true/' "$subject"
expect_mutation_failure semantic-edge "A multi-cell diagonal cut through the blocked transition envelope."

restore_subject
sed -i 's/|{world.Identity.KnowledgeIdentity}|{world.Identity.KnowledgeRevision}//' "$subject"
expect_mutation_failure knowledge-cache-key "Cache identity omitted requester knowledge identity or revision."

restore_subject
sed -i 's/|{world.Identity.SpatialRevision}//' "$subject"
expect_mutation_failure spatial-revision-key "Cache identity omitted spatial revision."

restore_subject
sed -i 's/List.sortBy cellKey/List.sortByDescending cellKey/' "$subject"
expect_mutation_failure deterministic-ordering "Footprint normalization lost deterministic cell ordering."

restore_subject
sed -i '/let packagePointPath/{n;s/maximumExpansions/(maximumExpansions - maximumExpansions)/;}' "$subject"
expect_mutation_failure package-adapter "Package Pathfinding.astar adapter changed."

restore_subject
sed -i 's/|{request.Profile.Stance}|{request.Profile.HeightBand}|{directionCode request.Profile.Facing}//' "$subject"
expect_mutation_failure profile-cache-key "Cache identity omitted stance, height, or facing."

restore_subject
sed -i 's/if (pairs |> List.sumBy (fun (origin, target) -> lineStepCount origin target + 1L)) > maximumWork then/if false then/' "$subject"
expect_mutation_failure trace-work-bound "Trace work was materialized beyond MaximumCrossedItems."

restore_subject
if [[ "$prepared_pr" == false ]]; then dotnet build "$project" -c Release --no-restore >/dev/null; fi
echo "Spatial subject mutations failed closed: dependency receipt, footprint, edge, knowledge, revision, ordering, package adapter, profile key, and trace bound."
