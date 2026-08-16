#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
temporary=$(mktemp -d /tmp/sir-ci-route-mutations.XXXXXX)
trap 'rm -rf -- "$temporary"' EXIT

expect_red() {
  local name=$1
  local mutation=$2
  rm -rf -- "$temporary/case"
  mkdir -p "$temporary/case/scripts" "$temporary/case/.github/workflows" "$temporary/case/tests/fixtures/ci-qualification/v1"
  cp "$repo_root/scripts/ci-route.mjs" "$temporary/case/scripts/ci-route.mjs"
  cp "$repo_root/scripts/test-ci-route.mjs" "$temporary/case/scripts/test-ci-route.mjs"
  cp "$repo_root/.github/workflows/ci.yml" "$temporary/case/.github/workflows/ci.yml"
  cp "$repo_root/scripts/qualify-production.sh" "$temporary/case/scripts/qualify-production.sh"
  cp "$repo_root/tests/fixtures/ci-qualification/v1/contracts.json" "$temporary/case/tests/fixtures/ci-qualification/v1/contracts.json"
  sed -i "$mutation" "$temporary/case/$name"
  if node "$temporary/case/scripts/test-ci-route.mjs" >"$temporary/mutation.log" 2>&1; then
    echo "ci-route mutation unexpectedly passed: $name" >&2
    exit 1
  fi
}

expect_red scripts/ci-route.mjs 's/return { classification: "cross-cutting", rule: "RP-005-unknown-conservative" };/return { classification: "browser", rule: "RP-005-unknown-conservative" };/;'
expect_red scripts/ci-route.mjs 's/const computedRouteDigest = routeDigest(route);/const computedRouteDigest = route?.digest;/;'
expect_red .github/workflows/ci.yml 's/^  full-qualification:/  omitted-full-qualification:/;'
expect_red .github/workflows/ci.yml 's#\./scripts/qualify-production.sh --protected#./scripts/qualify-production.sh#;'

echo "CI route policy, recomputed digest, scheduled/protected edge, and full-workflow topology mutations failed red in isolated fixtures."
