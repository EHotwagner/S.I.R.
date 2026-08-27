#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
work_id="365-handbook-m4"
readiness_dir="$repo_root/readiness/$work_id"
receipt="$readiness_dir/lifecycle.junit.xml"
tmp_dir="$(mktemp -d)"
trap 'rm -rf "$tmp_dir"' EXIT

cd "$repo_root"
for pass in first converged; do
  fsgg-sdd analyze --work "$work_id" --json > "$tmp_dir/$pass-analyze.json" || true
  fsgg-sdd evidence --work "$work_id" --json > "$tmp_dir/$pass-evidence.json" || true
  fsgg-sdd verify --work "$work_id" --json > "$tmp_dir/$pass-verify.json" || true
  fsgg-sdd ship --work "$work_id" --json > "$tmp_dir/$pass-ship.json" || true
done

jq -e '.outcome == "noChange" and ([.changedArtifacts[].operation] | all(. == "noChange")) and .analysis.blockingCount == 0 and .analysis.staleSourceCount == 0' "$tmp_dir/converged-analyze.json" >/dev/null
jq -e '.outcome == "noChange" and ([.changedArtifacts[].operation] | all(. == "noChange")) and .evidence.blockingCount == 0 and .evidence.staleCount == 0 and .evidence.syntheticCount == 0' "$tmp_dir/converged-evidence.json" >/dev/null
jq -e '.outcome == "noChange" and ([.changedArtifacts[].operation] | all(. == "noChange")) and .verification.blockingCount == 0 and .verification.evidenceStaleCount == 0 and .verification.evidenceSyntheticCount == 0' "$tmp_dir/converged-verify.json" >/dev/null
jq -e '.outcome == "noChange" and ([.changedArtifacts[].operation] | all(. == "noChange")) and .ship.blockingCount == 0 and .ship.generatedViewState == "current"' "$tmp_dir/converged-ship.json" >/dev/null

printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m4-lifecycle" tests="1" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM4" name="analyze-evidence-verify-ship-converged"/>' \
  '</testsuite>' > "$receipt"
printf 'handbook-m4 lifecycle: PASS (two-pass convergence; zero blocking, stale, or synthetic evidence)\n'
