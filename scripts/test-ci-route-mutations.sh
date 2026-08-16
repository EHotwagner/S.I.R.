#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
temporary=$(mktemp -d /tmp/sir-ci-route-mutations.XXXXXX)
trap 'rm -rf -- "$temporary"' EXIT

expect_red() {
  local name=$1
  local mutation=$2
  rm -rf -- "$temporary/case"
  mkdir -p "$temporary/case/scripts" "$temporary/case/.github/workflows"
  cp "$repo_root/scripts/ci-route.mjs" "$temporary/case/scripts/ci-route.mjs"
  cp "$repo_root/scripts/test-ci-route.mjs" "$temporary/case/scripts/test-ci-route.mjs"
  cp "$repo_root/.github/workflows/ci.yml" "$temporary/case/.github/workflows/ci.yml"
  sed -i "$mutation" "$temporary/case/$name"
  if node "$temporary/case/scripts/test-ci-route.mjs" >"$temporary/mutation.log" 2>&1; then
    echo "ci-route mutation unexpectedly passed: $name" >&2
    exit 1
  fi
}

expect_red scripts/ci-route.mjs 's/return { classification: "cross-cutting", rule: "RP-005-unknown-conservative" };/return { classification: "browser", rule: "RP-005-unknown-conservative" };/;'
expect_red scripts/ci-route.mjs 's/if (!result) throw new Error(`ci-route: missing-gate-result:${gate}`);/if (!result) continue;/;'
expect_red .github/workflows/ci.yml 's/^  full-qualification:/  omitted-full-qualification:/;'

echo "CI route policy, missing-result join, and full-workflow topology mutations failed red in isolated fixtures."
