#!/usr/bin/env bash
set -euo pipefail

project=""
output=""
previous=""
for argument in "$@"; do
  if [[ "$argument" == *.fsproj ]]; then project=$argument; fi
  if [[ "$previous" == "--outDir" ]]; then output=$argument; fi
  previous=$argument
done
[[ -n "$project" && -n "$output" ]] || exit 2
sleep 1
if [[ "${SIR_FAKE_FAIL_BOTH:-}" == "1" ]]; then
  [[ "$project" == *Replay.Web* ]] && exit 7
  exit 9
fi
mkdir -p "$output"
printf '%s\n' "$project" > "$output/result.txt"
