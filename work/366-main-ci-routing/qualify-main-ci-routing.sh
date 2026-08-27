#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)
cd "$repo_root"

node scripts/test-ci-route.mjs
./scripts/test-ci-route-mutations.sh
node scripts/test-ci-integrity-plan.mjs
node scripts/test-protected-stage-receipts.mjs
node scripts/test-pages-qualified-handoff.mjs
node scripts/test-ci-cost-report.mjs
node scripts/test-workflow-action-contract.mjs
git diff --check
echo "Main-push routing qualification passed: exact landed-diff routing, conservative CI fallback, focused/full protected joins, route-gated Pages handoff, mutation controls, workflow pins, and documentation links."
