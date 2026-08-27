#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$repo_root"

unset DOTNET_HOST_PATH DOTNET_ROOT_X64

dotnet restore SIR.slnx --locked-mode
dotnet build SIR.slnx --configuration Release --no-restore
./scripts/build-docs.sh --prepare-site-only

receipt_root="readiness/363-handbook-m3"
mkdir -p "$receipt_root"
printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m3-docs" tests="1" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM3" name="locked-release-strict-docs-build"/>' \
  '</testsuite>' > "$receipt_root/docs-build.junit.xml"

node work/359-handbook-m1/audit-handbook-links.mjs
printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m3-links" tests="1" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM3" name="structural-link-vocabulary-audit"/>' \
  '</testsuite>' > "$receipt_root/link-audit.junit.xml"

node work/363-handbook-m3/audit-complete-rules.mjs --require-rendered
SIR_Q4_JUNIT_OUT="$receipt_root/sir-combat-q4.junit.xml" ./scripts/qualify-quint-q4-sir-combat.sh
node work/363-handbook-m3/audit-roadmap-ledger.mjs
analysis_report="$(mktemp)"
trap 'rm -f "$analysis_report"' EXIT
fsgg-sdd analyze --work 363-handbook-m3 --json > "$analysis_report"
jq -e '.analysis.readiness == "implementationReady" and .analysis.blockingCount == 0 and .analysis.staleSourceCount == 0' "$analysis_report" >/dev/null
printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m3-analysis" tests="1" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM3" name="sdd-analysis-current-and-implementation-ready"/>' \
  '</testsuite>' > "$receipt_root/analysis-preflight.junit.xml"

printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m3-qualification" tests="6" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM3" name="locked-release-strict-docs-build"/>' \
  '  <testcase classname="SIR.HandbookM3" name="structural-link-vocabulary-audit"/>' \
  '  <testcase classname="SIR.HandbookM3" name="sixteen-rule-authority-reference-traceability-and-focused-runs"/>' \
  '  <testcase classname="SIR.HandbookM3" name="full-q4-model-and-runtime-qualification"/>' \
  '  <testcase classname="SIR.HandbookM3" name="roadmap-ledger-only-m3-checked"/>' \
  '  <testcase classname="SIR.HandbookM3" name="sdd-analysis-current-and-implementation-ready"/>' \
  '</testsuite>' > "$receipt_root/qualification.junit.xml"

printf 'handbook-m3 qualification: PASS (docs, links, focused sixteen-rule model, full Q4/runtime, roadmap ledger, SDD analysis)\n'
