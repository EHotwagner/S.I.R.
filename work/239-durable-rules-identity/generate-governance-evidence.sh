#!/usr/bin/env bash
set -uo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
receipt="$repo_root/readiness/239-durable-rules-identity/rules-governance.junit.xml"

generate_rc=0
mutations_rc=0
"$repo_root/scripts/generate-rules-governance.sh" --check >/dev/null 2>&1 || generate_rc=$?
"$repo_root/scripts/test-rules-governance-tool-mutations.sh" >/dev/null 2>&1 || mutations_rc=$?

failures=0
if ((generate_rc != 0)); then failures=$((failures + 1)); fi
if ((mutations_rc != 0)); then failures=$((failures + 1)); fi

{
  printf '%s\n' '<?xml version="1.0" encoding="UTF-8"?>'
  printf '<testsuites name="rules-governance" tests="2" failures="%d" errors="0" skipped="0">\n' "$failures"
  printf '  <testsuite name="rules-governance" tests="2" failures="%d" errors="0" skipped="0">\n' "$failures"
  printf '    <testcase classname="SIR.RulesGovernance" name="deterministic-current-protected-boundary">'
  if ((generate_rc != 0)); then printf '<failure message="governance receipt or protected boundary is stale" />'; fi
  printf '%s\n' '</testcase>'
  printf '    <testcase classname="SIR.RulesGovernance" name="fail-closed-subject-mutations">'
  if ((mutations_rc != 0)); then printf '<failure message="governance subject mutation did not reach its expected refusal" />'; fi
  printf '%s\n' '</testcase>'
  printf '%s\n' '  </testsuite>' '</testsuites>'
} > "$receipt"

if ((failures != 0)); then
  echo "rules governance evidence failed: currentness=$generate_rc mutations=$mutations_rc" >&2
  exit 1
fi

echo "rules governance observed evidence passed"
