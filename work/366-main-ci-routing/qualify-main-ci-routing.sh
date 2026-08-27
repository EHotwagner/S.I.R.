#!/usr/bin/env bash
set -uo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
cd "$repo_root"

run_suite() {
  node scripts/test-ci-route.mjs &&
    ./scripts/test-ci-route-mutations.sh &&
    node scripts/test-ci-integrity-plan.mjs &&
    node scripts/test-protected-stage-receipts.mjs &&
    node scripts/test-pages-qualified-handoff.mjs &&
    node scripts/test-ci-cost-report.mjs &&
    node scripts/test-workflow-action-contract.mjs &&
    git diff --check
}

started=$(date +%s%3N)
run_suite
status=$?
completed=$(date +%s%3N)
report=${SIR_JUNIT_OUTPUT:-readiness/366-main-ci-routing/qualification.junit.xml}
mkdir -p "$(dirname "$report")"
node - "$report" "$status" "$started" "$completed" <<'NODE'
const { writeFileSync } = require("node:fs");
const [path, rawStatus, rawStarted, rawCompleted] = process.argv.slice(2);
const status = Number(rawStatus);
const seconds = Math.max(0, Number(rawCompleted) - Number(rawStarted)) / 1000;
const failure = status === 0 ? "" : `<failure message="main CI routing qualification exited ${status}"/>`;
writeFileSync(path, `<?xml version="1.0" encoding="UTF-8"?>\n<testsuite name="main-ci-routing" tests="1" failures="${status === 0 ? 0 : 1}" skipped="0" time="${seconds.toFixed(3)}"><testcase classname="SIR.CI" name="focused-main-routing" time="${seconds.toFixed(3)}">${failure}</testcase></testsuite>\n`);
NODE

if [[ $status -eq 0 ]]; then
  echo "Main-push routing qualification passed: exact landed-diff routing, conservative CI fallback, focused/full protected joins, route-gated Pages handoff, mutation controls, workflow pins, and documentation links."
fi
exit "$status"
