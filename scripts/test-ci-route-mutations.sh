#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
temporary=$(mktemp -d /tmp/sir-ci-route-mutations.XXXXXX)
trap 'rm -rf -- "$temporary"' EXIT

# The fixture must carry EVERY file the route contract loads. Until S.I.R.#280 it carried
# seven, and `scripts/test-ci-route.mjs` imports `browser-shard-capacity.mjs` and
# `browser-junit.mjs` and reads twenty sibling sources, so every case died on
# ERR_MODULE_NOT_FOUND before its mutation was ever evaluated. `expect_red` is satisfied by a
# nonzero exit, so an UNMUTATED tree satisfied it and the suite proved nothing. The
# null-mutation control below is what makes this list self-policing: add a dependency to the
# contract without adding it here and the control goes red and names itself.
fixture_scripts=(
  ci-route.mjs test-ci-route.mjs browser-shard-capacity.mjs browser-junit.mjs
  qualify-pr.sh qualify-production.sh build-docs.sh run-ci-gate.sh
  smoke-worker-roundtrip.mjs test-browser-global-merge.mjs test-browser-shards.mjs
  test-conformance.sh test-spatial-subject-mutations.sh
  test-worker-cancellation-subject-mutation.sh verify-spatial-query.sh
)
fixture_files=(
  .github/workflows/ci.yml
  package.json
  tests/fixtures/ci-qualification/v1/contracts.json
  src/SIR.Client.Web/LiveSession.fs
  src/SIR.Simulation/SIR.Simulation.fsproj
  tests/SIR.Browser.Tests/playwright.config.js
  tests/SIR.Browser.Tests/production-delivery.spec.js
  tests/SIR.Match.Tests/Program.fs
)

build_fixture() {
  local dest=$1
  rm -rf -- "$dest"
  mkdir -p "$dest/scripts"
  local script
  for script in "${fixture_scripts[@]}"; do
    cp "$repo_root/scripts/$script" "$dest/scripts/$script"
  done
  local file
  for file in "${fixture_files[@]}"; do
    mkdir -p "$dest/$(dirname "$file")"
    cp "$repo_root/$file" "$dest/$file"
  done
}

# The control: an unmutated fixture MUST pass. Without this, "the suite went red" and "the
# fixture cannot start" are indistinguishable, which is exactly how the broken state survived.
build_fixture "$temporary/control"
if ! node "$temporary/control/scripts/test-ci-route.mjs" >"$temporary/control.log" 2>&1; then
  echo "ci-route mutation harness: NULL-MUTATION CONTROL FAILED — the unmutated fixture does not pass," >&2
  echo "so every expect_red below would be satisfied by the fixture being broken, not by its mutation." >&2
  head -30 "$temporary/control.log" >&2
  exit 1
fi

expect_red() {
  local name=$1
  local mutation=$2
  build_fixture "$temporary/case"
  cp "$temporary/case/$name" "$temporary/before"
  sed -i "$mutation" "$temporary/case/$name"
  # A sed that matches nothing leaves the fixture identical to the control, which passes.
  # Such a case asserts nothing at all, so it is a harness defect, not a silent success.
  if cmp -s "$temporary/before" "$temporary/case/$name"; then
    echo "ci-route mutation did not apply — the sed matched nothing: $name :: $mutation" >&2
    exit 1
  fi
  if node "$temporary/case/scripts/test-ci-route.mjs" >"$temporary/mutation.log" 2>&1; then
    echo "ci-route mutation unexpectedly passed: $name :: $mutation" >&2
    exit 1
  fi
  # Red is not enough: check WHICH branch fired. A mutation that trips a syntax error or a
  # missing module has not exercised the guard it claims to exercise.
  if ! grep -q "AssertionError" "$temporary/mutation.log"; then
    echo "ci-route mutation failed for the WRONG reason (no AssertionError): $name :: $mutation" >&2
    head -20 "$temporary/mutation.log" >&2
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
# Was `/run_delivery=true/d`, which matched nothing: that variable was refactored out of ci.yml
# and the string survived only inside this harness. Same intent, expressed against live code --
# domain-conformance must still run the browser-delivery gate when the route selects browser.
expect_red .github/workflows/ci.yml '/run-ci-gate.sh browser-delivery artifacts\/ci\/results\/browser-delivery.json/d;'
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
