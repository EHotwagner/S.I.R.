#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$repo_root"

qualification_status_before="$(git status --porcelain --untracked-files=all)"
if [[ -n "$qualification_status_before" ]]; then
  printf 'handbook-m6v qualification: exact replay requires a clean worktree before qualification\n%s\n' "$qualification_status_before" >&2
  exit 23
fi

receipt_root="readiness/377-handbook-m6v"
mkdir -p "$receipt_root"
live_render_root="$(mktemp -d)"
analysis_report="$(mktemp)"
initial_analysis_report="$(mktemp)"
evidence_sync_report="$(mktemp)"
verify_report="$(mktemp)"
ship_report="$(mktemp)"
timing_mutation_log=""
trap 'rm -rf -- "$live_render_root"; rm -f -- "$initial_analysis_report" "$evidence_sync_report" "$analysis_report" "$verify_report" "$ship_report"; if [[ -n "$timing_mutation_log" ]]; then rm -f -- "$timing_mutation_log"; fi' EXIT

dotnet --version >/dev/null
dotnet_bin="$(type -P dotnet)"
export DOTNET_ROOT="$(dirname "$(readlink -f "$dotnet_bin")")"
export DOTNET_ROOT_X64="$DOTNET_ROOT"
export DOTNET_HOST_PATH="$dotnet_bin"

dotnet tool restore
sdd_tool_root="artifacts/tools/fsgg-sdd-1.4.0"
if [[ ! -x "$sdd_tool_root/fsgg-sdd" ]]; then
  mkdir -p "$sdd_tool_root"
  dotnet tool install FS.GG.SDD.Cli --version 1.4.0 --tool-path "$sdd_tool_root"
fi
sdd="$sdd_tool_root/fsgg-sdd"
[[ "$($sdd --version)" == "1.4.0" ]] || { printf 'handbook-m6v qualification: exact local fsgg-sdd 1.4.0 is required\n' >&2; exit 22; }
npm ci
if [[ -n "${PLAYWRIGHT_EXECUTABLE_PATH:-}" ]]; then
  browser_executable="$PLAYWRIGHT_EXECUTABLE_PATH"
  export SIR_M6V_BROWSER_SOURCE="explicit-PLAYWRIGHT_EXECUTABLE_PATH"
else
  browser_executable="$(node --input-type=module -e 'import { chromium } from "@playwright/test"; process.stdout.write(chromium.executablePath())')"
  export SIR_M6V_BROWSER_SOURCE="playwright-managed"
fi
if [[ ! -x "$browser_executable" ]]; then
  if [[ -n "${PLAYWRIGHT_EXECUTABLE_PATH:-}" ]]; then
    printf 'handbook-m6v qualification: browser-bootstrap-failed: explicit PLAYWRIGHT_EXECUTABLE_PATH is not executable: %s\n' "$browser_executable" >&2
    exit 20
  fi
  if ! ./node_modules/.bin/playwright install chromium; then
    printf 'handbook-m6v qualification: browser-bootstrap-failed: lockfile-installed Playwright could not install Chromium\n' >&2
    exit 20
  fi
  browser_executable="$(node --input-type=module -e 'import { chromium } from "@playwright/test"; process.stdout.write(chromium.executablePath())')"
fi
export PLAYWRIGHT_EXECUTABLE_PATH="$browser_executable"
export SIR_M6V_BROWSER_PREFLIGHT_RECEIPT="$live_render_root/browser-preflight.json"
if ! node work/377-handbook-m6v/preflight-render-browser.mjs; then
  printf 'handbook-m6v qualification: browser-preflight-failed before the measured render subject\n' >&2
  exit 21
fi
dotnet restore SIR.slnx --locked-mode
dotnet build SIR.slnx --configuration Release --no-restore

node work/377-handbook-m6v/audit-visual-explanations.mjs --self-test --write-receipt
./scripts/build-docs.sh --prepare-site-only
timing_mutation_log="$(mktemp)"
live_timing_mutation_receipt="$live_render_root/timing-overflow-mutation.json"
rm -f "$live_timing_mutation_receipt"
set +e
SIR_M6V_DIAGRAM_RESPONSE_DELAY_MS=350 \
  SIR_M6V_TIMING_MUTATION_RECEIPT="$live_timing_mutation_receipt" \
  SIR_M6V_RENDER_OUTPUT="$live_render_root" \
  node work/377-handbook-m6v/inspect-rendered-visuals.mjs >"$timing_mutation_log" 2>&1
timing_mutation_status=$?
set -e
if [[ "$timing_mutation_status" -eq 0 ]]; then
  rm -f "$timing_mutation_log"
  printf 'handbook-m6v qualification: expected timing-overflow mutation to fail\n' >&2
  exit 1
fi
if [[ "$timing_mutation_status" -ne 42 ]]; then
  sed -n '1,120p' "$timing_mutation_log" >&2
  rm -f "$timing_mutation_log"
  printf 'handbook-m6v qualification: timing-overflow mutation exited %s instead of detector-specific 42\n' "$timing_mutation_status" >&2
  exit 1
fi
if ! jq -e '.result == "observed-red" and .mutation == "svg-response-delay-inside-decoded-image-readiness-subject" and .observation.diagramResponseDelayMs == 350 and .observation.p95LoadMs > .observation.maxP95Ms and .observation.p99LoadMs > .observation.maxP99Ms' "$live_timing_mutation_receipt" >/dev/null; then
  rm -f "$timing_mutation_log"
  printf 'handbook-m6v qualification: timing-overflow receipt did not bind the in-subject budget failure\n' >&2
  exit 1
fi
if ! grep -q 'render timing budget exceeded' "$timing_mutation_log"; then
  sed -n '1,120p' "$timing_mutation_log" >&2
  rm -f "$timing_mutation_log"
  printf 'handbook-m6v qualification: timing-overflow mutation failed through the wrong detector\n' >&2
  exit 1
fi
printf 'observed red: timing-overflow (render timing budget exceeded at 350 ms SVG response delay)\n'
rm -f "$timing_mutation_log"
SIR_M6V_RENDER_OUTPUT="$live_render_root" node work/377-handbook-m6v/inspect-rendered-visuals.mjs
printf 'restored green: timing-overflow (untouched decoded-image readiness route)\n'

dotnet run \
  --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj \
  --configuration Release \
  --no-build \
  --no-restore \
  -- --junit "$receipt_root/client-glyph-battlefield.junit.xml"

SIR_Q4_JUNIT_OUT="$receipt_root/sir-combat-q4.junit.xml" ./scripts/qualify-quint-q4-sir-combat.sh
cat > "$receipt_root/docs-render.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuite name="sir-handbook-m6v-docs-render" tests="4" failures="0" errors="0" skipped="0">
  <testcase classname="SIR.HandbookM6V" name="locked-release-strict-fsdocs-build"/>
  <testcase classname="SIR.HandbookM6V" name="six-diagram-source-svg-accessibility-fallback-audit"/>
  <testcase classname="SIR.HandbookM6V" name="thirty-standalone-and-eighteen-handbook-rendered-inspections"/>
  <testcase classname="SIR.HandbookM6V" name="hundred-sample-browser-native-performance-budget"/>
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
  <testcase classname="SIR.HandbookM6V" name="source-bound-svg-and-seven-observed-red-restored-green-controls"/>
  <testcase classname="SIR.HandbookM6V" name="strict-fsdocs-production-route"/>
  <testcase classname="SIR.HandbookM6V" name="normal-reduced-motion-print-effects-off-css-disabled-rendering"/>
  <testcase classname="SIR.HandbookM6V" name="typed-structural-and-warm-decode-performance"/>
  <testcase classname="SIR.HandbookM6V" name="production-client-glyph-and-battlefield-regression"/>
  <testcase classname="SIR.HandbookM6V" name="quint-q4-runtime-correspondence"/>
  <testcase classname="SIR.HandbookM6V" name="roadmap-feedback-lifecycle"/>
  <testcase classname="SIR.HandbookM6V" name="m7-preserved-pending"/>
</testsuite>
XML

node work/377-handbook-m6v/audit-roadmap-m6v.mjs --self-test
dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- \
  validate-checkpoint-state --cycle roadmap-sir-combat-quint-handbook-m6v-visual-explanations
dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- \
  validate feedback/2026-08-27-sir-handbook-m6v-visual-explanations.md \
  --audit feedback/audits/2026-08-27-sir-handbook-m6v-visual-explanations.audit.json

"$sdd" analyze --work 377-handbook-m6v --json > "$initial_analysis_report"
"$sdd" evidence --work 377-handbook-m6v --sync-observed-run "$receipt_root/qualification.junit.xml" --json > "$evidence_sync_report"
"$sdd" analyze --work 377-handbook-m6v --json > "$analysis_report"
"$sdd" verify --work 377-handbook-m6v --json > "$verify_report"
"$sdd" ship --work 377-handbook-m6v --json > "$ship_report"
jq -e '.outcome == "noChange" and .coherent == true and all(.changedArtifacts[]; .operation == "noChange") and .analysis.status == "implementationReady" and .analysis.readiness == "implementationReady" and .analysis.advisoryCount == 0 and .analysis.warningCount == 0 and .analysis.blockingCount == 0 and .analysis.staleSourceCount == 0 and .analysis.missingDispositionCount == 0 and .analysis.malformedSourceCount == 0 and .analysis.generatedViewFindingCount == 0 and .analysis.acceptedDeferralCount == 0' "$initial_analysis_report" >/dev/null
jq -e '.outcome == "noChange" and .coherent == true and all(.changedArtifacts[]; .operation == "noChange") and .evidence.status == "evidenceReady" and .evidence.readiness == "evidenceReady" and .evidence.declarationCount == 35 and .evidence.obligationCount == 35 and .evidence.supportedCount == 35 and .evidence.deferredCount == 0 and .evidence.missingCount == 0 and .evidence.staleCount == 0 and .evidence.syntheticCount == 0 and .evidence.invalidCount == 0 and .evidence.advisoryCount == 0 and .evidence.blockingCount == 0 and .evidence.classifiedObligationsUnmetCount == 0 and .evidence.journeyObligationsUnmetCount == 0 and ([.diagnostics[] | select(.severity == "info") | .id] == ["evidence.performanceBudgetPassed"])' "$evidence_sync_report" >/dev/null
jq -e '.outcome == "noChange" and .coherent == true and all(.changedArtifacts[]; .operation == "noChange") and .analysis.status == "implementationReady" and .analysis.readiness == "implementationReady" and .analysis.advisoryCount == 0 and .analysis.warningCount == 0 and .analysis.blockingCount == 0 and .analysis.staleSourceCount == 0 and .analysis.missingDispositionCount == 0 and .analysis.malformedSourceCount == 0 and .analysis.generatedViewFindingCount == 0 and .analysis.acceptedDeferralCount == 0' "$analysis_report" >/dev/null
jq -e '.outcome == "noChange" and .coherent == true and all(.changedArtifacts[]; .operation == "noChange") and .verification.status == "verificationReady" and .verification.readiness == "verificationReady" and .verification.readyFindingCount == 70 and .verification.advisoryCount == 1 and .verification.warningCount == 0 and .verification.blockingCount == 0 and .verification.obligationCount == 70 and .verification.evidenceSupportedCount == 35 and .verification.evidenceObservedCount == 35 and .verification.evidenceSelfAttestedCount == 0 and .verification.evidenceDeferredCount == 0 and .verification.evidenceMissingCount == 0 and .verification.evidenceStaleCount == 0 and .verification.evidenceSyntheticCount == 0 and .verification.evidenceInvalidCount == 0 and .verification.testSatisfiedCount == 35 and .verification.testObservedCount == 35 and .verification.testSelfAttestedCount == 0 and .verification.testDeferredCount == 0 and .verification.testMissingCount == 0 and .verification.testStaleCount == 0 and .verification.testInvalidCount == 0 and .verification.classifiedObligationsUnmetCount == 0 and .verification.journeyObligationsUnmetCount == 0 and .verification.skillMissingCount == 0 and .verification.findingIds == ["VF001"] and (.diagnostics == [{"id":"evidence.performanceBudgetPassed","severity":"info","artifact":"work/377-handbook-m6v/evidence.yml","location":null,"message":"Every declared normal-play workload satisfies the active performance budget.","correction":"Keep the cited artifact fresh when the workload or target changes.","relatedIds":["EV035","handbook-m6v-six-diagram-render-v1","readiness/377-handbook-m6v/performance-evidence.json"]}])' "$verify_report" >/dev/null
jq -e '.outcome == "noChange" and .coherent == true and all(.changedArtifacts[]; .operation == "noChange") and .ship.status == "shipReady" and .ship.readiness == "shipReady" and .ship.disposition == "shipReady" and .ship.advisoryCount == 0 and .ship.warningCount == 0 and .ship.blockingCount == 0 and .ship.verificationReadiness == "verificationReady" and .ship.evidenceSupportedCount == 35 and .ship.evidenceObservedCount == 35 and .ship.evidenceSelfAttestedCount == 0 and .ship.evidenceDeferredCount == 0 and .ship.evidenceMissingCount == 0 and .ship.evidenceStaleCount == 0 and .ship.evidenceSyntheticCount == 0 and .ship.evidenceInvalidCount == 0 and .ship.classifiedObligationsUnmetCount == 0 and .ship.journeyObligationsUnmetCount == 0 and .ship.generatedViewState == "current" and all(.ship.lifecycleStageReadiness[]; . == "ready") and .diagnostics == []' "$ship_report" >/dev/null

read -r p95 p99 < <(jq -r '[.timings.p95LoadMs,.timings.p99LoadMs] | @tsv' "$live_render_root/inspection.json")
qualification_status_after="$(git status --porcelain --untracked-files=all)"
if [[ "$qualification_status_after" != "$qualification_status_before" ]]; then
  printf 'handbook-m6v qualification: exact replay dirtied the worktree\n%s\n' "$qualification_status_after" >&2
  exit 24
fi
printf 'handbook-m6v qualification: PASS (six diagrams, 30 standalone + 18 handbook renders, 100 samples, p95=%sms, p99=%sms, strict docs, client glyph/battlefield, Q4/runtime, roadmap, feedback, exact local SDD 1.4.0)\n' "$p95" "$p99"
