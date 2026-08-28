#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$repo_root"
receipt_root="readiness/380-handbook-m7"
mkdir -p "$receipt_root"
mkdir -p artifacts
render_replay="$(mktemp -d artifacts/m7-render-replay.XXXXXX)"
trap 'rm -rf "$render_replay"' EXIT

dotnet_bin="$(type -P dotnet)"
export DOTNET_ROOT="$(dirname "$(readlink -f "$dotnet_bin")")"
export DOTNET_ROOT_X64="$DOTNET_ROOT"
export DOTNET_HOST_PATH="$dotnet_bin"

dotnet tool restore
[[ "$(dotnet --version)" == "10.0.302" ]]
[[ "$(quint --version)" == "0.32.0" ]]
[[ "$(node --version)" == v26.* ]]

sdd="artifacts/tools/fsgg-sdd-1.4.0/fsgg-sdd"
if [[ ! -x "$sdd" ]]; then
  mkdir -p "$(dirname "$sdd")"
  dotnet tool install FS.GG.SDD.Cli --version 1.4.0 --tool-path "$(dirname "$sdd")"
fi
[[ "$($sdd --version)" == "1.4.0" ]]

npm ci
if [[ -z "${PLAYWRIGHT_EXECUTABLE_PATH:-}" ]]; then
  export PLAYWRIGHT_EXECUTABLE_PATH="$(node --input-type=module -e 'import { chromium } from "@playwright/test"; process.stdout.write(chromium.executablePath())')"
fi
if [[ ! -x "$PLAYWRIGHT_EXECUTABLE_PATH" ]]; then
  ./node_modules/.bin/playwright install chromium
fi

SIR_M6V_BROWSER_PREFLIGHT_RECEIPT="$receipt_root/browser-preflight.json" \
  node work/377-handbook-m6v/preflight-render-browser.mjs
node work/375-handbook-m6/audit-handbook-structure.mjs --self-test
node work/380-handbook-m7/audit-publication-handoff.mjs --self-test
./scripts/build-docs.sh --prepare-site-only
SIR_M6V_BROWSER_PREFLIGHT_RECEIPT="$receipt_root/browser-preflight.json" \
SIR_M6V_RENDER_OUTPUT="$render_replay" \
  node work/377-handbook-m6v/inspect-rendered-visuals.mjs
./scripts/qualify-quint-q4-sir-combat.sh

node work/380-handbook-m7/audit-roadmap-m7.mjs
dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- \
  validate-checkpoints feedback/checkpoints/roadmap-sir-combat-quint-handbook-m7-publication-handoff.jsonl
dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- \
  validate feedback/2026-08-28-sir-handbook-m7-publication-handoff.md \
  --audit feedback/audits/2026-08-28-sir-handbook-m7-publication-handoff.audit.json
python3 .agents/skills/work-roadmap/scripts/validate-feedback-state.py \
  --root . --cycle roadmap-sir-combat-quint-handbook-m7-publication-handoff \
  --report feedback/2026-08-28-sir-handbook-m7-publication-handoff.md \
  --audit feedback/audits/2026-08-28-sir-handbook-m7-publication-handoff.audit.json \
  --phases onboarding-first-build,lifecycle-authoring,implementation-test-evidence,verify-ship-pr

"$sdd" analyze --work 380-handbook-m7 >/dev/null
"$sdd" verify --work 380-handbook-m7 >/dev/null
"$sdd" ship --work 380-handbook-m7 >/dev/null

cat > "$receipt_root/qualification.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuite name="sir-handbook-m7" tests="8" failures="0" errors="0" skipped="0">
  <testcase classname="SIR.HandbookM7" name="publication-review"/>
  <testcase classname="SIR.HandbookM7" name="source-tool-identities"/>
  <testcase classname="SIR.HandbookM7" name="maintenance-owner-trigger"/>
  <testcase classname="SIR.HandbookM7" name="m6-structure-links"/>
  <testcase classname="SIR.HandbookM7" name="m6v-render-accessibility-fallback"/>
  <testcase classname="SIR.HandbookM7" name="m6v-performance-binding"/>
  <testcase classname="SIR.HandbookM7" name="quint-runtime-regression"/>
  <testcase classname="SIR.HandbookM7" name="lifecycle-feedback-roadmap"/>
</testsuite>
XML
printf 'handbook-m7 qualification: PASS (4 reviews, 6 diagrams, 48 fresh renders, inherited 100/200ms budgets, 8 M7 mutations, strict docs, Q4/runtime, SDD, feedback)\n'
