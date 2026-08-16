#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
gate=${1:?run-ci-gate: gate id is required}
output=${2:?run-ci-gate: output path is required}
started=$(date +%s%3N)
status=pass
set +e
"$repo_root/scripts/qualify-pr.sh" gate "$gate"
exit_code=$?
set -e
if [[ $exit_code -ne 0 ]]; then
  status=fail
fi
completed=$(date +%s%3N)
node "$repo_root/scripts/ci-route.mjs" gate \
  --gate "$gate" \
  --status "$status" \
  --started-ms "$started" \
  --completed-ms "$completed" \
  --receipt-reused true \
  --output "$output"
exit "$exit_code"
