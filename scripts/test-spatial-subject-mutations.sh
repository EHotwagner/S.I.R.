#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
subject="$repo_root/src/SIR.Simulation/SpatialQuery.fs"
project="$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"
temporary_dir=$(mktemp -d /tmp/sir-spatial-subject-mutations.XXXXXX)
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
  if dotnet run --project "$project" -c Release -- --print-spatial-query >"$log" 2>&1; then
    echo "spatial subject mutation unexpectedly passed: $name" >&2
    exit 1
  fi
  grep -F -- "$expected" "$log" >/dev/null || {
    echo "spatial subject mutation failed for the wrong reason: $name" >&2
    cat "$log" >&2
    exit 1
  }
}

sed -i 's/{ RevisionTokens = world.DisclosedRevisionTokens }/{ RevisionTokens = Set.empty }/' "$subject"
expect_mutation_failure dependency-receipt "A disclosed blocker was not retained as a dynamic cache dependency."

restore_subject
sed -i 's/|{request.Profile.Stance}|{request.Profile.HeightBand}|{directionCode request.Profile.Facing}//' "$subject"
expect_mutation_failure profile-cache-key "Cache identity omitted stance, height, or facing."

restore_subject
sed -i 's/if (pairs |> List.sumBy (fun (origin, target) -> lineStepCount origin target + 1L)) > maximumWork then/if false then/' "$subject"
expect_mutation_failure trace-work-bound "Trace work was materialized beyond MaximumCrossedItems."

restore_subject
dotnet build "$project" -c Release --no-restore >/dev/null
echo "Spatial subject mutations failed closed: dependency receipt, profile cache key, and trace work bound."
