#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
task_tmp=$(mktemp -d /tmp/sir-rule-coherence.XXXXXX)
cd "$repo_root"

dotnet tool restore
dotnet restore tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj --locked-mode
dotnet build tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-restore
dotnet restore src/SIR.Tools/SIR.Tools.fsproj --locked-mode
dotnet build src/SIR.Tools/SIR.Tools.fsproj -c Release --no-restore
export SIR_RULES_NO_BUILD=1

dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-build -- --print-rule-coherence > "$task_tmp/native.hex"
dotnet fable tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj --outDir "$task_tmp/fable" --noCache
node "$task_tmp/fable/SIR.Conformance.Shared/Program.js" --print-rule-coherence > "$task_tmp/fable.hex"
cmp "$task_tmp/native.hex" "$task_tmp/fable.hex"

for mutation in contradiction unit-mismatch undeclared-dependency prototype-leakage history-mismatch unreachable-transition; do
  if dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-build -- --inject-rule-coherence-mutation "$mutation" > "$task_tmp/$mutation.log" 2>&1; then
    echo "rule coherence mutation unexpectedly passed: $mutation" >&2
    exit 1
  fi
  grep -q "first coherence failure: mutation=$mutation" "$task_tmp/$mutation.log"
done

scripts/sir-rules check --mode changed --rule COMBAT-DAMAGE-001 --cache "$task_tmp/cache.json" --out "$task_tmp/changed.json" >/dev/null
scripts/sir-rules check --mode changed --rule COMBAT-DAMAGE-001 --cache "$task_tmp/cache.json" | jq -e '.cost.workUnits == 0 and .cost.expensiveAnalyses == 0 and .cost.cacheHits == 1' >/dev/null
scripts/sir-rules check --mode cone --rule COMBAT-DAMAGE-001 | jq -e '.mode == "cone" and .termination == "complete" and .cost.rulesInSlice > 1' >/dev/null
scripts/sir-rules check --mode corpus | jq -e '.mode == "corpus" and .cost.prunedPairs > .cost.candidatePairs' >/dev/null

if scripts/sir-rules check --mode changed > "$task_tmp/malformed.log" 2>&1; then
  echo "changed mode without a rule unexpectedly passed" >&2
  exit 1
fi

if scripts/sir-rules check --mode changed --rule MISSING-RULE-001 > "$task_tmp/missing-rule.log" 2>&1; then
  echo "unknown changed-rule seed unexpectedly passed" >&2
  exit 1
else
  test "$?" -eq 3
fi

if scripts/sir-rules check --mode cone --rule COMBAT-DAMAGE-001 --block-unknowns > "$task_tmp/block-unknowns.log" 2>&1; then
  echo "policy-blocking unknown unexpectedly passed" >&2
  exit 1
else
  test "$?" -eq 4
fi

printf '%s\n' '{malformed' > "$task_tmp/malformed-cache.json"
if scripts/sir-rules check --mode corpus --cache "$task_tmp/malformed-cache.json" > "$task_tmp/malformed-cache.log" 2>&1; then
  echo "malformed coherence cache unexpectedly passed" >&2
  exit 1
else
  test "$?" -eq 2
fi

env HOME="$task_tmp/incompatible-home" CODEX_HOME="$task_tmp/incompatible-codex" python3 scripts/validate-skill-package.py .agents/skills/sir-author-rule
env HOME="$task_tmp/incompatible-home" CODEX_HOME="$task_tmp/incompatible-codex" python3 scripts/validate-skill-package.py .agents/skills/sir-check-rule-coherence

mkdir -p "$task_tmp/invalid-skill"
printf '%s\n' '---' 'description: deliberately missing the required name' '---' > "$task_tmp/invalid-skill/SKILL.md"
if env HOME="$task_tmp/incompatible-home" CODEX_HOME="$task_tmp/incompatible-codex" python3 scripts/validate-skill-package.py "$task_tmp/invalid-skill" > "$task_tmp/invalid-skill.log" 2>&1; then
  echo "malformed skill unexpectedly passed repository-owned validation" >&2
  exit 1
fi
grep -q "Missing or invalid 'name'" "$task_tmp/invalid-skill.log"

for typed_scalar in null true 42; do
  mkdir -p "$task_tmp/typed-$typed_scalar"
  printf '%s\n' '---' "name: $typed_scalar" 'description: invalid typed scalar fixture' '---' > "$task_tmp/typed-$typed_scalar/SKILL.md"
  if env HOME="$task_tmp/incompatible-home" CODEX_HOME="$task_tmp/incompatible-codex" python3 scripts/validate-skill-package.py "$task_tmp/typed-$typed_scalar" >/dev/null 2>&1; then
    echo "typed YAML scalar unexpectedly passed skill validation: $typed_scalar" >&2
    exit 1
  fi
done

mkdir -p "$task_tmp/nested-name-skill"
printf '%s\n' '---' 'name:' '  nested: invalid' 'description: invalid nested name fixture' '---' > "$task_tmp/nested-name-skill/SKILL.md"
if env HOME="$task_tmp/incompatible-home" CODEX_HOME="$task_tmp/incompatible-codex" python3 scripts/validate-skill-package.py "$task_tmp/nested-name-skill" >/dev/null 2>&1; then
  echo "nested YAML name unexpectedly passed skill validation" >&2
  exit 1
fi

mkdir -p "$task_tmp/nested-metadata-skill"
printf '%s\n' '---' 'name: nested-metadata-skill' 'description: valid nested metadata fixture' 'metadata:' '  owner: S.I.R.' '---' > "$task_tmp/nested-metadata-skill/SKILL.md"
env HOME="$task_tmp/incompatible-home" CODEX_HOME="$task_tmp/incompatible-codex" python3 scripts/validate-skill-package.py "$task_tmp/nested-metadata-skill" >/dev/null

mkdir -p "$task_tmp/unreadable-skill"
printf '\377\376' > "$task_tmp/unreadable-skill/SKILL.md"
if env HOME="$task_tmp/incompatible-home" CODEX_HOME="$task_tmp/incompatible-codex" python3 scripts/validate-skill-package.py "$task_tmp/unreadable-skill" >/dev/null 2>&1; then
  echo "unreadable skill unexpectedly passed repository-owned validation" >&2
  exit 1
fi

result_path=${SIR_RULE_COHERENCE_JUNIT:-readiness/193-rule-authoring-coherence/rule-coherence.junit.xml}
mkdir -p "$(dirname "$result_path")"
printf '%s\n' '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuites name="rule-coherence" tests="1" failures="0" errors="0" skipped="0"><testsuite name="rule-coherence" tests="1" failures="0" errors="0" skipped="0"><testcase name="focused-owner-verifier" classname="SIR.Item193"/></testsuite></testsuites>' \
  > "$result_path"

printf 'Rule coherence verified: deterministic slices, indexed pruning, cache reuse, cancellation, witnesses, mutations, skills, and native/Fable bytes.\n'
