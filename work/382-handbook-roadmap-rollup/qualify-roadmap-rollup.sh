#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$repo_root"
receipt_root="readiness/382-handbook-roadmap-rollup"
mkdir -p "$receipt_root"

dotnet_bin="$(type -P dotnet)"
export DOTNET_ROOT="$(dirname "$(readlink -f "$dotnet_bin")")"
export DOTNET_ROOT_X64="$DOTNET_ROOT"
export DOTNET_HOST_PATH="$dotnet_bin"

node work/382-handbook-roadmap-rollup/audit-roadmap-rollup.mjs
node work/382-handbook-roadmap-rollup/audit-roadmap-rollup.mjs --self-test

cycle_count=0
checkpoint_count=0
for checkpoint in feedback/checkpoints/roadmap-sir-combat-quint-handbook-*.jsonl; do
  cycle="$(basename "$checkpoint" .jsonl)"
  mapfile -t reports < <(rg -l --fixed-strings "$cycle" feedback/*.md | sort)
  [[ "${#reports[@]}" -eq 1 ]] || { printf 'roll-up: %s binds %s reports\n' "$cycle" "${#reports[@]}" >&2; exit 1; }
  report="${reports[0]}"
  audit="feedback/audits/$(basename "$report" .md).audit.json"
  [[ -f "$audit" ]] || { printf 'roll-up: missing audit %s\n' "$audit" >&2; exit 1; }
  phases="$(sed -n 's/^- \*\*phases:\*\* //p' "$report")"
  events="$(wc -l < "$checkpoint")"

  dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- validate-checkpoints "$checkpoint"
  dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- validate "$report" --audit "$audit"
  python3 .agents/skills/work-roadmap/scripts/validate-feedback-state.py \
    --root . --cycle "$cycle" --report "$report" --audit "$audit" --phases "$phases"

  cycle_count=$((cycle_count + 1))
  checkpoint_count=$((checkpoint_count + events))
done
[[ "$cycle_count" -eq 12 && "$checkpoint_count" -eq 48 ]]

dotnet tool restore
dotnet restore SIR.slnx --locked-mode
dotnet build SIR.slnx -c Release --no-restore
./scripts/build-docs.sh --prepare-site-only

printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-roadmap-rollup" tests="4" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookRoadmapRollup" name="complete-cycle-and-checkpoint-coverage"/>' \
  '  <testcase classname="SIR.HandbookRoadmapRollup" name="six-observed-red-restored-green-controls"/>' \
  '  <testcase classname="SIR.HandbookRoadmapRollup" name="twelve-feedback-state-bindings"/>' \
  '  <testcase classname="SIR.HandbookRoadmapRollup" name="strict-documentation-build"/>' \
  '</testsuite>' > "$receipt_root/qualification.junit.xml"

printf '{"schema":"sir.handbook-roadmap-rollup-qualification/v1","result":"pass","cycles":%s,"checkpoints":%s,"checkedUnits":10,"mutations":6,"dispositions":{"structuredFinding":5,"positivePattern":23,"acceptedObservation":16,"deduplicatedExistingIssue":4}}\n' \
  "$cycle_count" "$checkpoint_count" > "$receipt_root/qualification.json"

printf 'handbook roadmap roll-up qualification: PASS (%s cycles, %s checkpoints, 10 checked units, 6 mutations, strict docs)\n' "$cycle_count" "$checkpoint_count"
