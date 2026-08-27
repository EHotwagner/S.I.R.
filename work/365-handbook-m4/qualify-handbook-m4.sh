#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$repo_root"

unset DOTNET_HOST_PATH DOTNET_ROOT_X64

dotnet restore SIR.slnx --locked-mode
dotnet build SIR.slnx --configuration Release --no-restore
./scripts/build-docs.sh --prepare-site-only

receipt_root="readiness/365-handbook-m4"
mkdir -p "$receipt_root"
printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m4-docs" tests="1" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM4" name="locked-release-strict-docs-build"/>' \
  '</testsuite>' > "$receipt_root/docs-build.junit.xml"

node work/359-handbook-m1/audit-handbook-links.mjs
printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m4-links" tests="1" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM4" name="structural-link-vocabulary-audit"/>' \
  '</testsuite>' > "$receipt_root/link-audit.junit.xml"

node work/365-handbook-m4/audit-formal-reasoning.mjs --require-rendered
SIR_Q4_JUNIT_OUT="$receipt_root/sir-combat-q4.junit.xml" ./scripts/qualify-quint-q4-sir-combat.sh
node work/365-handbook-m4/audit-roadmap-ledger.mjs
analysis_report="$(mktemp)"
trap 'rm -f "$analysis_report"' EXIT
fsgg-sdd analyze --work 365-handbook-m4 --json > "$analysis_report"
jq -e '.analysis.readiness == "implementationReady" and .analysis.blockingCount == 0 and .analysis.staleSourceCount == 0' "$analysis_report" >/dev/null
printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m4-analysis" tests="1" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM4" name="sdd-analysis-current-and-implementation-ready"/>' \
  '</testsuite>' > "$receipt_root/analysis-preflight.junit.xml"

printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m4-qualification" tests="6" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM4" name="locked-release-strict-docs-build"/>' \
  '  <testcase classname="SIR.HandbookM4" name="structural-link-vocabulary-audit"/>' \
  '  <testcase classname="SIR.HandbookM4" name="formal-claims-six-mutation-pairs-action-reachability-and-state-bindings"/>' \
  '  <testcase classname="SIR.HandbookM4" name="full-q4-model-and-runtime-qualification"/>' \
  '  <testcase classname="SIR.HandbookM4" name="roadmap-ledger-only-m4-checked"/>' \
  '  <testcase classname="SIR.HandbookM4" name="sdd-analysis-current-and-implementation-ready"/>' \
  '</testsuite>' > "$receipt_root/qualification.junit.xml"

printf 'handbook-m4 qualification: PASS (docs, links, focused formal reasoning/mutations, full Q4/runtime, roadmap ledger, SDD analysis)\n'
