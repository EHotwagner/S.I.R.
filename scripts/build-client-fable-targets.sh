#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
fable_output=${1:?build-client-fable-targets: replay output directory is required}
rules_fable_output=${2:?build-client-fable-targets: rules output directory is required}
fable_logs=$(mktemp -d "${TMPDIR:-/tmp}/sir-client-fable.XXXXXXXX")
trap 'rm -rf "$fable_logs"' EXIT

cd "$repo_root"
dotnet fable src/SIR.Replay.Web/SIR.Replay.Web.fsproj \
  --outDir "$fable_output" \
  --define SIR_WEB_CLIENT \
  --noCache >"$fable_logs/replay.log" 2>&1 &
replay_pid=$!

dotnet fable src/SIR.Client.Web/SIR.RulesExplorer.Web.fsproj \
  --outDir "$rules_fable_output" \
  --noCache >"$fable_logs/rules.log" 2>&1 &
rules_pid=$!

set +e
wait "$replay_pid"
replay_status=$?
wait "$rules_pid"
rules_status=$?
set -e
cat "$fable_logs/replay.log"
cat "$fable_logs/rules.log"
if (( replay_status != 0 || rules_status != 0 )); then
  printf 'build-client: parallel Fable targets failed: replay=%s rules=%s\n' "$replay_status" "$rules_status" >&2
  exit 1
fi
