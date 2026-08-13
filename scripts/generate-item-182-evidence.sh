#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
evidence_root="$repo_root/readiness/182-awareness-reaction-windows"
staged_root="$task_tmp/staged"
task_tmp=$(mktemp -d)
trap 'rm -rf "$task_tmp"' EXIT
cd "$repo_root"
mkdir -p "$evidence_root" "$staged_root"

xml_escape() { sed -e 's/&/\&amp;/g' -e 's/</\&lt;/g' -e 's/>/\&gt;/g' -e 's/"/\&quot;/g' -e "s/'/\&apos;/g"; }
write_receipt() {
  local output=$1 suite=$2 testcase=$3 command=$4 log=$5
  local digest
  digest=$(sha256sum "$log" | awk '{print $1}')
  {
    printf '%s\n' '<?xml version="1.0" encoding="utf-8"?>'
    printf '<testsuite name="%s" tests="1" failures="0">\n' "$suite"
    printf '  <testcase classname="Item182Evidence" name="%s"/>\n' "$testcase"
    printf '  <system-out>command=%s; log-sha256=%s</system-out>\n' "$(printf '%s' "$command" | xml_escape)" "$digest"
    printf '%s\n' '</testsuite>'
  } > "$output"
}

run_receipt() {
  local name=$1 suite=$2 testcase=$3 command=$4
  bash -c "$command" 2>&1 | tee "$task_tmp/$name.log"
  write_receipt "$staged_root/$name.junit.xml" "$suite" "$testcase" "$command" "$task_tmp/$name.log"
}

run_receipt awareness-reaction-core item-182-core \
  'awareness, reaction, replay, performance, disclosure, and browser gates pass' \
  './scripts/verify-awareness-reaction.sh'
run_receipt awareness-reaction-rules item-182-rules \
  'normal and forced-fallback rules generation, source correspondence, and identity mutations pass' \
  './scripts/verify-rules-corpus.sh && SIR_RULES_FORCE_GREP=1 ./scripts/verify-rules-corpus.sh'
run_receipt awareness-reaction-conformance item-182-conformance \
  'complete native/Fable, replay, worker, WASM, server, browser, and delivery conformance passes' \
  './scripts/test-conformance.sh'
run_receipt awareness-reaction-docs item-182-docs \
  'documentation build, integrity, experience, browser smoke, and accessibility pass' \
  './scripts/build-docs.sh'

cp "$staged_root"/*.junit.xml "$evidence_root/"
printf '%s\n' "$evidence_root"/*.junit.xml
