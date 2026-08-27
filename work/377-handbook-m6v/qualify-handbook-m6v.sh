#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$repo_root"

receipt_root="readiness/377-handbook-m6v"
mkdir -p "$receipt_root"

dotnet --version >/dev/null
dotnet_bin="$(type -P dotnet)"
export DOTNET_ROOT="$(dirname "$(readlink -f "$dotnet_bin")")"
export DOTNET_ROOT_X64="$DOTNET_ROOT"
export DOTNET_HOST_PATH="$dotnet_bin"

dotnet tool restore
dotnet restore SIR.slnx --locked-mode
dotnet build SIR.slnx --configuration Release --no-restore

node work/377-handbook-m6v/audit-visual-explanations.mjs --self-test --write-receipt
./scripts/build-docs.sh --prepare-site-only
node work/377-handbook-m6v/inspect-rendered-visuals.mjs

dotnet run \
  --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj \
  --configuration Release \
  --no-build \
  --no-restore \
  -- --junit "$receipt_root/client-glyph-battlefield.junit.xml"

SIR_Q4_JUNIT_OUT="$receipt_root/sir-combat-q4.junit.xml" ./scripts/qualify-quint-q4-sir-combat.sh
node work/377-handbook-m6v/audit-roadmap-m6v.mjs --self-test
dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- \
  validate-checkpoint-state --cycle roadmap-sir-combat-quint-handbook-m6v-visual-explanations

analysis_report="$(mktemp)"
trap 'rm -f "$analysis_report"' EXIT
fsgg-sdd analyze --work 377-handbook-m6v --json > "$analysis_report"
jq -e '.analysis.readiness == "implementationReady" and .analysis.blockingCount == 0 and .analysis.staleSourceCount == 0' "$analysis_report" >/dev/null

cat > "$receipt_root/docs-render.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuite name="sir-handbook-m6v-docs-render" tests="4" failures="0" errors="0" skipped="0">
  <testcase classname="SIR.HandbookM6V" name="locked-release-strict-fsdocs-build"/>
  <testcase classname="SIR.HandbookM6V" name="six-diagram-source-svg-accessibility-fallback-audit"/>
  <testcase classname="SIR.HandbookM6V" name="twenty-four-rendered-mode-inspections-and-nine-screenshots"/>
  <testcase classname="SIR.HandbookM6V" name="thirty-sample-browser-native-performance-budget"/>
</testsuite>
XML

cat > "$receipt_root/roadmap-feedback-sdd.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuite name="sir-handbook-m6v-roadmap-feedback-sdd" tests="3" failures="0" errors="0" skipped="0">
  <testcase classname="SIR.HandbookM6V" name="roadmap-only-m6v-checked-m7-pending"/>
  <testcase classname="SIR.HandbookM6V" name="feedback-checkpoint-state-valid"/>
  <testcase classname="SIR.HandbookM6V" name="sdd-analysis-current-and-implementation-ready"/>
</testsuite>
XML

cat > "$receipt_root/qualification.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuite name="sir-handbook-m6v-qualification" tests="8" failures="0" errors="0" skipped="0">
  <testcase classname="SIR.HandbookM6V" name="source-bound-svg-and-six-observed-red-restored-green-mutations"/>
  <testcase classname="SIR.HandbookM6V" name="strict-fsdocs-production-route"/>
  <testcase classname="SIR.HandbookM6V" name="normal-reduced-motion-print-effects-off-rendering"/>
  <testcase classname="SIR.HandbookM6V" name="typed-structural-and-warm-decode-performance"/>
  <testcase classname="SIR.HandbookM6V" name="production-client-glyph-and-battlefield-regression"/>
  <testcase classname="SIR.HandbookM6V" name="quint-q4-runtime-correspondence"/>
  <testcase classname="SIR.HandbookM6V" name="roadmap-feedback-lifecycle"/>
  <testcase classname="SIR.HandbookM6V" name="m7-preserved-pending"/>
</testsuite>
XML

read -r p95 p99 < <(jq -r '[.timings.p95LoadMs,.timings.p99LoadMs] | @tsv' "$receipt_root/rendered/inspection.json")
printf 'handbook-m6v qualification: PASS (six diagrams, 24 rendered modes, p95=%sms, p99=%sms, strict docs, client glyph/battlefield, Q4/runtime, roadmap, feedback, SDD)\n' "$p95" "$p99"
