#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$repo_root"

# The host may export a system dotnet path that conflicts with the repository-pinned SDK.
unset DOTNET_HOST_PATH DOTNET_ROOT_X64

dotnet restore SIR.slnx --locked-mode
dotnet build SIR.slnx --configuration Release --no-restore
./scripts/build-docs.sh --prepare-site-only

receipt_root="readiness/361-handbook-m2"
mkdir -p "$receipt_root"
printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m2-docs" tests="1" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM2" name="locked-release-strict-docs-build"/>' \
  '</testsuite>' > "$receipt_root/docs-build.junit.xml"

node work/359-handbook-m1/audit-handbook-links.mjs
printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m2-links" tests="1" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM2" name="structural-link-vocabulary-audit"/>' \
  '</testsuite>' > "$receipt_root/link-audit.junit.xml"

node work/361-handbook-m2/audit-representative-attack.mjs --require-rendered
SIR_Q4_JUNIT_OUT="$receipt_root/sir-combat-q4.junit.xml" ./scripts/qualify-quint-q4-sir-combat.sh

printf '%s\n' \
  '<?xml version="1.0" encoding="UTF-8"?>' \
  '<testsuite name="sir-handbook-m2-qualification" tests="4" failures="0" errors="0" skipped="0">' \
  '  <testcase classname="SIR.HandbookM2" name="locked-release-strict-docs-build"/>' \
  '  <testcase classname="SIR.HandbookM2" name="structural-link-vocabulary-audit"/>' \
  '  <testcase classname="SIR.HandbookM2" name="authority-excerpts-mutation-and-restored-green"/>' \
  '  <testcase classname="SIR.HandbookM2" name="full-q4-model-and-runtime-qualification"/>' \
  '</testsuite>' > "$receipt_root/qualification.junit.xml"
