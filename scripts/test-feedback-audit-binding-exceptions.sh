#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
test_root=$(mktemp -d /tmp/sir-feedback-binding-exceptions.XXXXXX)
trap 'rm -rf -- "$test_root"' EXIT
cd "$repo_root"

mkdir -p "$test_root/feedback/audits" "$test_root/scripts"
cp "$repo_root/feedback/audits/2026-08-13-sir-item-183-tactical-overlays.audit.json" "$test_root/feedback/audits/"
cp "$repo_root/feedback/audits/2026-08-15-sir-item-184-elaborate-tactical-sample-2.audit.json" "$test_root/feedback/audits/"
cp "$repo_root/scripts/audit-binding-exceptions.json" "$test_root/scripts/"
cp --parents \
  tests/SIR.Browser.Tests/visible-workflows.spec.js \
  scripts/test-conformance.sh \
  readiness/184-scenario-catalog/scenario-catalog-native.junit.xml \
  readiness/184-scenario-catalog/scenario-catalog-browser.junit.xml \
  readiness/184-scenario-catalog/scenario-catalog-cross-runtime.junit.xml \
  readiness/184-scenario-catalog/scenario-catalog-rules.junit.xml \
  "$test_root"

tool="$repo_root/.agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx"
changed="tests/SIR.Browser.Tests/visible-workflows.spec.js;scripts/test-conformance.sh;readiness/184-scenario-catalog/scenario-catalog-native.junit.xml;readiness/184-scenario-catalog/scenario-catalog-browser.junit.xml;readiness/184-scenario-catalog/scenario-catalog-cross-runtime.junit.xml;readiness/184-scenario-catalog/scenario-catalog-rules.junit.xml"
ledger="$test_root/scripts/audit-binding-exceptions.json"
pristine="$test_root/scripts/audit-binding-exceptions.pristine.json"
cp "$ledger" "$pristine"

run_check() {
  dotnet fsi "$tool" -- check-invalidation --changed "$changed" --root "$test_root"
}

run_mutant() {
  local name=$1 expected=$2 filter=$3
  cp "$pristine" "$ledger"
  jq "$filter" "$ledger" > "$ledger.next"
  mv "$ledger.next" "$ledger"
  if run_check > "$test_root/$name.log" 2>&1; then
    echo "feedback audit-binding $name mutant unexpectedly passed" >&2
    exit 1
  fi
  grep -F "$expected" "$test_root/$name.log" >/dev/null || {
    echo "feedback audit-binding $name mutant failed without '$expected'" >&2
    cat "$test_root/$name.log" >&2
    exit 1
  }
}

run_check >/dev/null

printf '{' > "$ledger"
if run_check > "$test_root/malformed.log" 2>&1; then
  echo "feedback audit-binding malformed-ledger mutant unexpectedly passed" >&2
  exit 1
fi
grep -F "malformed exception ledger" "$test_root/malformed.log" >/dev/null

run_mutant stale "replacement evidence is stale" '.exceptions[0].replacementSha256 = ("0" * 64)'
run_mutant mismatched "previous digest is mismatched" '.exceptions[0].previousSha256 = ("0" * 64)'
run_mutant duplicate "duplicate exception ledger binding" '.exceptions += [.exceptions[0]]'
run_mutant overbroad "overbroad or mismatched exception" '.exceptions += [(.exceptions[0] | .findingId = "§9.9")]'

cp "$pristine" "$ledger"
run_check >/dev/null
echo "feedback audit-binding exception gate passed with malformed, stale, mismatched, duplicate, and overbroad mutants rejected"
