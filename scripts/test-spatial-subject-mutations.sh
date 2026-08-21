#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
subject="$repo_root/src/SIR.Simulation/SpatialQuery.fs"
project="$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"
simulation_project="$repo_root/src/SIR.Simulation/SIR.Simulation.fsproj"
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

mutation_expected() {
  case "$1" in
    dependency-receipt) printf '%s' "An inspected empty cell was not retained as an occupancy-addition dependency." ;;
    footprint-envelope) printf '%s' "Footprint evaluation ignored an occupied sample." ;;
    semantic-edge) printf '%s' "A multi-cell diagonal cut through the blocked transition envelope." ;;
    knowledge-cache-key) printf '%s' "Cache identity omitted requester knowledge identity or revision." ;;
    spatial-revision-key) printf '%s' "Cache identity omitted spatial revision." ;;
    deterministic-ordering) printf '%s' "Footprint normalization lost deterministic cell ordering." ;;
    package-adapter) printf '%s' "Package Pathfinding.astar adapter changed." ;;
    profile-cache-key) printf '%s' "Cache identity omitted stance, height, or facing." ;;
    trace-work-bound) printf '%s' "Trace work was materialized beyond MaximumCrossedItems." ;;
    *) echo "unknown spatial mutation: $1" >&2; return 2 ;;
  esac
}

write_mutant() {
  local name=$1
  local target=$2
  cp -p "$original" "$target"
  case "$name" in
    dependency-receipt) sed -i '/observeToken $"occupancy:{position.Col}:{position.Row}"/d' "$target" ;;
    footprint-envelope) sed -i 's/footprint |> List.map (addCell anchor)/footprint |> List.truncate 1 |> List.map (addCell anchor)/' "$target" ;;
    semantic-edge) sed -i 's/| SpatialModality.GroundMovement -> value.Ground/| SpatialModality.GroundMovement -> true/' "$target" ;;
    knowledge-cache-key) sed -i 's/|{world.Identity.KnowledgeIdentity}|{world.Identity.KnowledgeRevision}//' "$target" ;;
    spatial-revision-key) sed -i 's/|{world.Identity.SpatialRevision}//' "$target" ;;
    deterministic-ordering) sed -i 's/List.sortBy cellKey/List.sortByDescending cellKey/' "$target" ;;
    package-adapter) sed -i '/let packagePointPath/{n;s/maximumExpansions/(maximumExpansions - maximumExpansions)/;}' "$target" ;;
    profile-cache-key) sed -i 's/|{request.Profile.Stance}|{request.Profile.HeightBand}|{directionCode request.Profile.Facing}//' "$target" ;;
    trace-work-bound) sed -i 's/if (pairs |> List.sumBy (fun (origin, target) -> lineStepCount origin target + 1L)) > maximumWork then/if false then/' "$target" ;;
  esac
}

run_prepared_mutation() {
  local name=$1
  local mutation_root="$temporary_dir/mutations/$name"
  local mutant="$mutation_root/SpatialQuery.fs"
  local artifacts="$mutation_root/artifacts"
  local runtime="$mutation_root/runtime"
  local log="$mutation_root/mutation.log"
  local expected
  expected=$(mutation_expected "$name")
  mkdir -p "$mutation_root"
  cp -p "$repo_root/src/SIR.Simulation/SpatialQuery.fsi" "$mutation_root/SpatialQuery.fsi"
  write_mutant "$name" "$mutant"
  cp -al "$temporary_dir/runtime-base" "$runtime"
  if ! dotnet restore "$simulation_project" --locked-mode --artifacts-path "$artifacts" \
    -p:SpatialQueryImplementation="$mutant" >"$log" 2>&1; then
    echo "spatial subject mutation could not restore: $name" >&2
    cat "$log" >&2
    return 1
  fi
  if ! SIR_BUILD_EXCEPTION="spatial-$name" dotnet build "$simulation_project" -c Release --no-restore \
    --artifacts-path "$artifacts" -p:SpatialQueryImplementation="$mutant" >>"$log" 2>&1; then
    echo "spatial subject mutation could not build: $name" >&2
    cat "$log" >&2
    return 1
  fi
  cp --remove-destination "$artifacts/bin/SIR.Simulation/release/SIR.Simulation.dll" "$runtime/SIR.Simulation.dll"
  if { SIR_BUILD_EXCEPTION="spatial-$name" dotnet "$runtime/SIR.Domain.Tests.dll" --print-spatial-query; } >>"$log" 2>&1; then
    echo "spatial subject mutation unexpectedly passed: $name" >&2
    return 1
  fi
  grep -F -- "$expected" "$log" >/dev/null || {
    echo "spatial subject mutation failed for the wrong reason: $name" >&2
    cat "$log" >&2
    return 1
  }
}

if [[ "$prepared_pr" == true ]]; then
  mutation_names=(
    dependency-receipt footprint-envelope semantic-edge knowledge-cache-key spatial-revision-key
    deterministic-ordering package-adapter profile-cache-key trace-work-bound
  )
  concurrency=${SIR_SPATIAL_MUTATION_CONCURRENCY:-3}
  [[ "$concurrency" =~ ^[1-9][0-9]*$ && $concurrency -le ${#mutation_names[@]} ]] || {
    echo "SIR_SPATIAL_MUTATION_CONCURRENCY must be between 1 and ${#mutation_names[@]}" >&2
    exit 2
  }
  mkdir -p "$temporary_dir/runtime-base"
  cp -a "$repo_root/tests/SIR.Domain.Tests/bin/Release/net10.0/." "$temporary_dir/runtime-base/"
  active_pids=()
  failed=0
  wait_batch() {
    local pid
    for pid in "${active_pids[@]}"; do wait "$pid" || failed=1; done
    active_pids=()
  }
  for name in "${mutation_names[@]}"; do
    run_prepared_mutation "$name" &
    active_pids+=("$!")
    if [[ ${#active_pids[@]} -eq $concurrency ]]; then wait_batch; fi
  done
  if [[ ${#active_pids[@]} -gt 0 ]]; then wait_batch; fi
  [[ $failed -eq 0 ]] || exit 1
  echo "Spatial subject mutations failed closed: dependency receipt, footprint, edge, knowledge, revision, ordering, package adapter, profile key, and trace bound."
  exit 0
fi

expect_mutation_failure() {
  local name=$1
  local expected=$2
  local log="$temporary_dir/$name.log"
  if [[ "$prepared_pr" == true ]]; then
    if ! SIR_BUILD_EXCEPTION="spatial-$name" dotnet build "$simulation_project" -c Release --no-restore --artifacts-path "$temporary_dir/artifacts" >"$log" 2>&1; then
      echo "spatial subject mutation could not build: $name" >&2
      cat "$log" >&2
      exit 1
    fi
    cp "$temporary_dir/artifacts/bin/SIR.Simulation/release/SIR.Simulation.dll" "$temporary_dir/runtime/SIR.Simulation.dll"
    if SIR_BUILD_EXCEPTION="spatial-$name" dotnet "$temporary_dir/runtime/SIR.Domain.Tests.dll" --print-spatial-query >>"$log" 2>&1; then
      echo "spatial subject mutation unexpectedly passed: $name" >&2
      exit 1
    fi
  elif SIR_BUILD_EXCEPTION="spatial-$name" dotnet run --project "$project" -c Release -- --print-spatial-query >"$log" 2>&1; then
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
