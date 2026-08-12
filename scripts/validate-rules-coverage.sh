#!/usr/bin/env bash
set -euo pipefail

coverage=${1:?usage: scripts/validate-rules-coverage.sh COVERAGE_JSON}

jq -e '
  [.nodes[].identity] as $identities
  | ($identities | length) == ($identities | unique | length)
    and all(.edges[]; (.from as $endpoint | $identities | index($endpoint)) != null)
    and all(.edges[]; (.to as $endpoint | $identities | index($endpoint)) != null)
' "$coverage" >/dev/null || {
  echo "rules coverage contains duplicate node identities or dangling edge endpoints" >&2
  exit 1
}
