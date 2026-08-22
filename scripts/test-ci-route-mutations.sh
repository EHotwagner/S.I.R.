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
  cp "$repo_root/scripts/qualify-pr.sh" "$temporary/case/scripts/qualify-pr.sh"
  cp "$repo_root/scripts/qualify-production.sh" "$temporary/case/scripts/qualify-production.sh"
  cp "$repo_root/scripts/build-docs.sh" "$temporary/case/scripts/build-docs.sh"
  cp "$repo_root/tests/fixtures/ci-qualification/v1/contracts.json" "$temporary/case/tests/fixtures/ci-qualification/v1/contracts.json"
  sed -i "$mutation" "$temporary/case/$name"
  if node "$temporary/case/scripts/test-ci-route.mjs" >"$temporary/mutation.log" 2>&1; then
    echo "ci-route mutation unexpectedly passed: $name" >&2
    exit 1
  fi
}

expect_red scripts/ci-route.mjs 's/return { classification: "cross-cutting", rule: "RP-005-unknown-conservative" };/return { classification: "browser", rule: "RP-005-unknown-conservative" };/;'
expect_red scripts/ci-route.mjs 's/const computedRouteDigest = routeDigest(route);/const computedRouteDigest = route?.digest;/;'
expect_red scripts/ci-route.mjs 's/\["cross-cutting", "performance"\]/["cross-cutting"]/;'
expect_red scripts/ci-route.mjs 's/  "scripts\/finalize-svg-pipeline-evidence.mjs",/  "scripts\/ci-route.mjs",\n  "scripts\/finalize-svg-pipeline-evidence.mjs",/;'
expect_red scripts/ci-route.mjs 's/ || (productionReviewRequired && gate === "documentation")//;'
expect_red scripts/ci-route.mjs 's/ || classification === "cross-cutting"//;'
expect_red .github/workflows/ci.yml "s/needs.route.outputs.documentation == 'true'/needs.route.outputs.classification == 'documentation'/;"
expect_red scripts/build-docs.sh '/test-production-review-freshness-mutations.sh/d;'
expect_red .github/workflows/ci.yml "s/ || needs.route.outputs.classification == 'performance'//g;"
expect_red .github/workflows/ci.yml 's/^  full-qualification:/  omitted-full-qualification:/;'
expect_red .github/workflows/ci.yml 's#\./scripts/qualify-production.sh --protected#./scripts/qualify-production.sh#;'
expect_red scripts/qualify-pr.sh '/node_modules\/playwright-core/d;'
expect_red .github/workflows/ci.yml '/^  browser:/,/^  browser-general-helper:/ s|uses: actions/download-artifact@[0-9a-f]* # v8.0.1|run: npm ci --ignore-scripts|;'
expect_red .github/workflows/ci.yml '/gates+=(spatial)/d;'
expect_red .github/workflows/ci.yml '/run_delivery=true/d;'
expect_red .github/workflows/ci.yml '/run-ci-gate.sh cross-runtime artifacts\/ci\/results\/cross-runtime.json/d;'
expect_red .github/workflows/ci.yml '/SIR_CI_PREFLIGHT_REUSED:/d;'
expect_red scripts/qualify-pr.sh '/artifacts\/publish/d;'
expect_red scripts/qualify-pr.sh '/build-docs.sh --prepare-site-only/d;'
expect_red scripts/qualify-pr.sh 's/ --reuse-site-build//;'
expect_red .github/workflows/ci.yml "/^  browser:/,/^  browser-general-helper:/ {/Microsoft.NETCore.App 10.0./d;}"
expect_red scripts/qualify-pr.sh '/SIR_BROWSER_SHARD_INDEX="$second_index"/d;'
expect_red scripts/qualify-pr.sh '/wait "$second_pid"/d;'

echo "CI route policy, production-review freshness/preparer wiring, performance scope/headroom, prepared Playwright runtime reuse, co-scheduled rules/spatial receipts, recomputed digest, scheduled/protected edge, and full-workflow topology mutations failed red in isolated fixtures."
