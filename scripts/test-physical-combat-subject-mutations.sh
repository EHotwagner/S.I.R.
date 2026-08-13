#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
subject="$repo_root/src/SIR.Simulation/CombatModel.fs"
project="$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"
task_tmp=$(mktemp -d /tmp/sir-combat-subject-mutations.XXXXXX)
original="$task_tmp/CombatModel.fs"
cp -p "$subject" "$original"

restore_subject() {
  cp -p "$original" "$subject"
  touch "$subject"
}

cleanup() {
  restore_subject
  rm -rf -- "$task_tmp"
}
trap cleanup EXIT

expect_failure() {
  local name=$1
  local expected=$2
  local log="$task_tmp/$name.log"
  if dotnet run --project "$project" -c Release -- --print-combat >"$log" 2>&1; then
    echo "physical combat subject mutation unexpectedly passed: $name" >&2
    exit 1
  fi
  grep -F -- "$expected" "$log" >/dev/null || {
    echo "physical combat mutation failed for the wrong reason: $name" >&2
    cat "$log" >&2
    exit 1
  }
}

sed -i 's/apps, p.AreaRadius = 0)/apps, false)/' "$subject"
expect_failure intervening-collision "Intervening collision did not stop at the first unit."

restore_subject
sed -i 's/Some coverId, 50/Some coverId, 100/' "$subject"
expect_failure cover-retention "Partial cover did not halve retained rifle damage."

restore_subject
sed -i 's/else max 10 (penetration \* 100 \/ effective)/else 100/' "$subject"
expect_failure directional-armor "Directional armor or penetration changed."

restore_subject
sed -i 's/resolveConsequences target.Health target.Suppression p.Suppression/resolveConsequences target.Health target.Suppression 0/' "$subject"
expect_failure suppression "Friendly area recipient received implicit immunity or missed suppression."

restore_subject
sed -i 's/let orderedFacts = List.rev facts/let orderedFacts = facts/' "$subject"
expect_failure consequence-ordering "Combat consequence facts lost canonical cover/armor/health/suppression ordering."

restore_subject
sed -i 's/RuleApplications = List.rev apps/RuleApplications = []/' "$subject"
expect_failure rules-identity "Combat result was not bound to the executable rules identity."

restore_subject
dotnet build "$project" -c Release --no-restore >/dev/null
echo "Physical combat subject mutations failed closed: collision, cover, armor, suppression, consequence ordering, and rules identity."
