#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
evidence_root="$repo_root/readiness/181-physical-combat-slice"
task_tmp=$(mktemp -d)
trap 'rm -rf "$task_tmp"' EXIT
cd "$repo_root"

receipts=(
  physical-combat-core.junit.xml
  physical-combat-rules.junit.xml
  physical-combat-conformance.junit.xml
  physical-combat-docs.junit.xml
)
mkdir -p "$evidence_root"

xml_escape() {
  sed -e 's/&/\&amp;/g' -e 's/</\&lt;/g' -e 's/>/\&gt;/g' -e 's/"/\&quot;/g' -e "s/'/\&apos;/g"
}

write_receipt() {
  local output=$1
  local suite=$2
  local testcase=$3
  local command=$4
  local log=$5
  local digest
  digest=$(sha256sum "$log" | awk '{print $1}')
  {
    printf '%s\n' '<?xml version="1.0" encoding="utf-8"?>'
    printf '<testsuite name="%s" tests="1" failures="0">\n' "$suite"
    printf '  <testcase classname="Item181Evidence" name="%s"/>\n' "$testcase"
    printf '  <system-out>command=%s; log-sha256=%s</system-out>\n' "$(printf '%s' "$command" | xml_escape)" "$digest"
    printf '%s\n' '</testsuite>'
  } > "$output"
}

# A clean checkout must be independently provisionable. SIR.Server.Tests is a
# focused boundary project outside SIR.slnx, so restore it explicitly before
# any verifier is allowed to claim an observed run.
dotnet restore SIR.slnx
dotnet restore tests/SIR.Server.Tests/SIR.Server.Tests.fsproj
npm ci

core_command='./scripts/verify-physical-combat.sh'
$core_command 2>&1 | tee "$task_tmp/core.log"
write_receipt "$task_tmp/physical-combat-core.junit.xml" \
  item-181-physical-combat-core \
  'authoritative combat fixtures, mutations, server boundary, performance, native/Fable, and browser journey pass' \
  "$core_command" "$task_tmp/core.log"

rules_command='./scripts/verify-rules-corpus.sh && SIR_RULES_FORCE_GREP=1 ./scripts/verify-rules-corpus.sh'
bash -c "$rules_command" 2>&1 | tee "$task_tmp/rules.log"
write_receipt "$task_tmp/physical-combat-rules.junit.xml" \
  item-181-physical-combat-rules \
  'normal and forced rules generation, source correspondence, and identity mutations pass' \
  "$rules_command" "$task_tmp/rules.log"

conformance_command='./scripts/test-conformance.sh'
$conformance_command 2>&1 | tee "$task_tmp/conformance.log"
write_receipt "$task_tmp/physical-combat-conformance.junit.xml" \
  item-181-physical-combat-conformance \
  'full native/Fable replay, seek, worker, WASM, browser, and delivery conformance passes' \
  "$conformance_command" "$task_tmp/conformance.log"

docs_command='./scripts/build-docs.sh'
$docs_command 2>&1 | tee "$task_tmp/docs.log"
write_receipt "$task_tmp/physical-combat-docs.junit.xml" \
  item-181-physical-combat-docs \
  'documentation build, integrity, experience, browser smoke, and accessibility pass' \
  "$docs_command" "$task_tmp/docs.log"

for receipt in "${receipts[@]}"; do
  cp "$task_tmp/$receipt" "$evidence_root/$receipt"
done
printf '%s\n' "${receipts[@]/#/$evidence_root/}"
