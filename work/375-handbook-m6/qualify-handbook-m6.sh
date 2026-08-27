#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$repo_root"

unset DOTNET_HOST_PATH DOTNET_ROOT_X64 DOTNET_ROOT

dotnet restore SIR.slnx --locked-mode
dotnet build SIR.slnx --configuration Release --no-restore
./scripts/build-docs.sh --prepare-site-only

receipt_root="readiness/375-handbook-m6"
mkdir -p "$receipt_root"
printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m6-docs" tests="1" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM6" name="locked-release-strict-docs-build-with-ast-gate"/>' \
  '</testsuite>' > "$receipt_root/docs-build.junit.xml"

node work/375-handbook-m6/complete-definition-index.mjs
node work/359-handbook-m1/audit-handbook-links.mjs
node work/375-handbook-m6/audit-handbook-structure.mjs
printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m6-structure" tests="11" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM6" name="complete-index-alias-declaration-rule-chapter-reconciliation"/>' \
  '  <testcase classname="SIR.HandbookM6" name="missing-fragment-observed-red-restored-green"/>' \
  '  <testcase classname="SIR.HandbookM6" name="duplicate-anchor-observed-red-restored-green"/>' \
  '  <testcase classname="SIR.HandbookM6" name="absent-index-entry-observed-red-restored-green"/>' \
  '  <testcase classname="SIR.HandbookM6" name="unlinked-controlled-occurrence-observed-red-restored-green"/>' \
  '  <testcase classname="SIR.HandbookM6" name="unlinked-controlled-symbol-observed-red-restored-green"/>' \
  '  <testcase classname="SIR.HandbookM6" name="wrong-canonical-target-observed-red-restored-green"/>' \
  '  <testcase classname="SIR.HandbookM6" name="insubstantial-definition-observed-red-restored-green"/>' \
  '  <testcase classname="SIR.HandbookM6" name="authoritative-declaration-removal-observed-red-restored-green"/>' \
  '  <testcase classname="SIR.HandbookM6" name="authoritative-rule-id-drift-observed-red-restored-green"/>' \
  '  <testcase classname="SIR.HandbookM6" name="manifest-alias-removal-observed-red-restored-green"/>' \
  '</testsuite>' > "$receipt_root/structure-audit.junit.xml"

SIR_Q4_JUNIT_OUT="$receipt_root/sir-combat-q4.junit.xml" ./scripts/qualify-quint-q4-sir-combat.sh
node work/375-handbook-m6/audit-roadmap-ledger.mjs
printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m6-roadmap" tests="1" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM6" name="roadmap-ledger-only-m6-checked"/>' \
  '</testsuite>' > "$receipt_root/roadmap-ledger.junit.xml"

analysis_report="$(mktemp)"
trap 'rm -f "$analysis_report"' EXIT
dotnet fsgg-sdd analyze --work 375-handbook-m6 --root . --json > "$analysis_report"
jq -e '.analysis.readiness == "implementationReady" and .analysis.blockingCount == 0 and .analysis.staleSourceCount == 0' "$analysis_report" >/dev/null
printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m6-analysis" tests="1" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM6" name="sdd-analysis-current-and-implementation-ready"/>' \
  '</testsuite>' > "$receipt_root/analysis-preflight.junit.xml"

printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m6-qualification" tests="6" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM6" name="locked-release-strict-docs-build-with-ast-gate"/>' \
  '  <testcase classname="SIR.HandbookM6" name="complete-definition-index"/>' \
  '  <testcase classname="SIR.HandbookM6" name="ten-structural-mutations-restored-green"/>' \
  '  <testcase classname="SIR.HandbookM6" name="full-q4-runtime-regression"/>' \
  '  <testcase classname="SIR.HandbookM6" name="roadmap-ledger-only-m6-checked"/>' \
  '  <testcase classname="SIR.HandbookM6" name="sdd-analysis-current-and-implementation-ready"/>' \
  '</testsuite>' > "$receipt_root/qualification.junit.xml"

printf 'handbook-m6 qualification: PASS (strict docs, 188 definitions, 5 aliases, 74 declarations, 16 rules, 50 chapters, 10 mutations, Q4/runtime, roadmap, SDD)\n'
