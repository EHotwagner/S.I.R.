#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
project="$repo_root/src/SIR.Simulation/Governance.Tool/SIR.Rules.Governance.Tool.fsproj"
temporary_dir=$(mktemp -d /tmp/sir-rules-governance-mutations.XXXXXX)
implementation="$repo_root/tests/fixtures/rules-corpus/v2/implementation-sources.json"
replay="$repo_root/readiness/194-executable-rules-corpus/replay-v3.junit.xml"
cp "$implementation" "$temporary_dir/implementation.json"
cp "$replay" "$temporary_dir/replay.xml"
restore_subjects() {
  cp "$temporary_dir/implementation.json" "$implementation"
  cp "$temporary_dir/replay.xml" "$replay"
  rm -rf -- "$temporary_dir"
}
trap restore_subjects EXIT

run_args=()
if [[ "${SIR_RULES_PREPARED_PR:-0}" == 1 ]]; then run_args=(--no-build --no-restore); fi

jq '.sourceCommit = ("0" * 40) | .packageSha256 = ("f" * 64)' "$implementation" > "$implementation.next"
mv "$implementation.next" "$implementation"
if dotnet run --project "$project" -c Release "${run_args[@]}" -- generate "$repo_root" "$temporary_dir/binding-receipt.json" "$temporary_dir/binding-verdict.json"; then
  echo "foreign evidence binding mutation unexpectedly passed" >&2
  exit 1
fi
jq -e '.payload.evidence[] | select(.artifact == "tests/fixtures/rules-corpus/v2/implementation-sources.json" and .state == "stale" and .packageManifestDigest == null and .semanticDigest == null)' "$temporary_dir/binding-receipt.json" >/dev/null
jq -e '.blocked == true' "$temporary_dir/binding-verdict.json" >/dev/null
cp "$temporary_dir/implementation.json" "$implementation"

printf '<testsuites tests="5" failures="0" errors="1"><malformed' > "$replay"
if dotnet run --project "$project" -c Release "${run_args[@]}" -- generate "$repo_root" "$temporary_dir/junit-receipt.json" "$temporary_dir/junit-verdict.json"; then
  echo "malformed JUnit mutation unexpectedly passed" >&2
  exit 1
fi
jq -e '.payload.evidence[] | select(.artifact == "readiness/194-executable-rules-corpus/replay-v3.junit.xml" and .state == "malformed")' "$temporary_dir/junit-receipt.json" >/dev/null
jq -e '.blocked == true' "$temporary_dir/junit-verdict.json" >/dev/null
cp "$temporary_dir/replay.xml" "$replay"

printf '<testsuites tests="1" failures="0" errors="1"><testsuite tests="1" failures="0" errors="1"><testcase name="errored"/></testsuite></testsuites>' > "$replay"
if dotnet run --project "$project" -c Release "${run_args[@]}" -- generate "$repo_root" "$temporary_dir/errored-receipt.json" "$temporary_dir/errored-verdict.json"; then
  echo "errored JUnit mutation unexpectedly passed" >&2
  exit 1
fi
jq -e '.payload.evidence[] | select(.artifact == "readiness/194-executable-rules-corpus/replay-v3.junit.xml" and .state == "current-fail")' "$temporary_dir/errored-receipt.json" >/dev/null
jq -e '.blocked == true' "$temporary_dir/errored-verdict.json" >/dev/null
cp "$temporary_dir/replay.xml" "$replay"

printf '{"schemaVersion":99,"stage":"ship","readiness":"shipReady","disposition":{"state":"shipReady","blockingFindingIds":[]},"verificationReadiness":{"status":"verificationReady","blockingFindingIds":[]}}' > "$temporary_dir/fake-sdd.json"
printf '{"schema":"wrong","boundary":"ship","profile":"standard","blocked":false,"findings":[]}' > "$temporary_dir/fake-verdict.json"
if dotnet run --project "$project" -c Release "${run_args[@]}" -- join "$temporary_dir/fake-sdd.json" "$temporary_dir/fake-verdict.json" "$temporary_dir/fake-boundary.json" fake-sdd fake-verdict; then
  echo "invalid protected-boundary authorities unexpectedly joined" >&2
  exit 1
fi

jq '.blocked = false | .findings[0].verdict = "fail" | .findings[0].effectiveBlocking = true' "$repo_root/readiness/198-rules-governance-receipts/rules-governance-verdict.json" > "$temporary_dir/contradictory-verdict.json"
if dotnet run --project "$project" -c Release "${run_args[@]}" -- join "$repo_root/readiness/198-rules-governance-receipts/ship.json" "$temporary_dir/contradictory-verdict.json" "$temporary_dir/contradictory-boundary.json" ship contradictory-verdict; then
  echo "contradictory protected-boundary verdict unexpectedly joined" >&2
  exit 1
fi

echo "rules governance binding, malformed-JUnit, and protected-boundary mutations rejected"
