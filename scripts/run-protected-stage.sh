#!/usr/bin/env bash
set -uo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
stage=${1:-}
output=${2:-}
shift 2 || true

case "$stage" in preflight|core) ;; *) echo "run-protected-stage: expected preflight or core" >&2; exit 2 ;; esac
[[ -n "$output" && ${1:-} == -- && $# -ge 2 ]] || { echo "run-protected-stage: usage <stage> <output> -- <command> [args...]" >&2; exit 2; }
shift

started=$(date +%s%3N)
"$@"
status=$?
completed=$(date +%s%3N)
mkdir -p "$(dirname "$output")"
subjects=()
if [[ "$stage" == preflight ]]; then
  subjects=(--subject cancellation-mutation --subject rules-corpus --subject spatial-static)
  [[ ${SIR_PROTECTED_SCHEDULED:-false} != true ]] || subjects+=(--subject scheduled-full-route-mutation)
else
  subjects=(--subject conformance --subject documentation --subject production-browser --subject sdd-verify --subject stale-reuse-mutations)
fi
node "$repo_root/scripts/protected-stage-receipt.mjs" create \
  --stage "$stage" \
  --status "$([[ $status -eq 0 ]] && echo pass || echo fail)" \
  --started-ms "$started" \
  --completed-ms "$completed" \
  --failure-stage command \
  --output "$output" \
  "${subjects[@]}"
exit "$status"
