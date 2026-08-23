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


# --- S.I.R.#280: the feedback-timing ceiling must stay a live gate. -----------------------
# Every way of "fixing" the unsatisfiable cross-cutting deadline by widening or removing the
# check turns this repair into a disablement, and each one below would leave the PR that made
# it green. They must all fail red.

# Delete the outer budget refusal.
expect_red scripts/ci-route.mjs '/code: "feedback-budget-exceeded",/,+2d;'
# Delete the acceptance-target refusal.
expect_red scripts/ci-route.mjs 's/if (enforceBudget \&\& attributableCriticalPathMilliseconds > acceptanceTargetMilliseconds) failures.push({/if (false) failures.push({/;'
# Disarm the whole check by defaulting enforcement off.
expect_red scripts/ci-route.mjs 's/enforceBudget = true/enforceBudget = false/;'
# Buy headroom by inflating the per-wave allowance.
expect_red scripts/ci-route.mjs 's/export const feedbackWaveBudgetMilliseconds = 180_000;/export const feedbackWaveBudgetMilliseconds = 600_000;/;'
# Buy headroom by inflating the fixed end-job overhead.
expect_red scripts/ci-route.mjs 's/export const feedbackPipelineOverheadMilliseconds = 30_000;/export const feedbackPipelineOverheadMilliseconds = 300_000;/;'
# Give the reserve back, which is what left the widest route on the shortest deadline.
expect_red scripts/ci-route.mjs 's/export const feedbackHeadroomBasisPoints = 2_000;/export const feedbackHeadroomBasisPoints = 0;/;'
# Score Math.max over every gate instead of summing the per-wave maxima. This is the specific
# loosening that looks principled, cites an existing field, and flattens the dependency graph
# into its single longest node -- roughly a 2.3x weaker ceiling on the measured run.
expect_red scripts/ci-route.mjs 's/return totals.length === 0 ? 0 : Math.max(...totals);/return 0;/;'
# Collapse the wave partition so second-wave work stops adding to the path.
expect_red scripts/ci-route.mjs 's/((gateParts\[subject\] ?? \[\]).length > 0 ? 2 : 1)/1/;'
# Restore wall-clock scoring, which is what let fleet saturation decide the verdict.
expect_red scripts/ci-route.mjs 's/const attributableCriticalPathMilliseconds = feedbackPipelineOverheadMilliseconds/const attributableCriticalPathMilliseconds = 0 * feedbackPipelineOverheadMilliseconds/;'
# Let a receipt with no usable duration silently drop out of the measurement.
expect_red scripts/ci-route.mjs 's/if (byGate.has(subject) \&\& usableTotal(subject) === null) failures.push({ code: "missing-feedback-timing", subject });/void subject;/;'
# Let the documented contract drift from the enforced one.
expect_red tests/fixtures/ci-qualification/v1/contracts.json 's/"feedbackWaveBudgetMilliseconds": 180000/"feedbackWaveBudgetMilliseconds": 900000/;'

echo "CI route policy, feedback-timing ceiling inversions, production-review freshness/preparer wiring, performance scope/headroom, prepared Playwright runtime reuse, co-scheduled rules/spatial receipts, recomputed digest, scheduled/protected edge, and full-workflow topology mutations failed red in isolated fixtures."
