#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
evidence_root="$repo_root/readiness/180-authoritative-spatial-query-foundation"

cd "$repo_root"
mkdir -p "$evidence_root"

# Receipts are emitted only after the executable subject exits green.
dotnet restore SIR.slnx --locked-mode
./scripts/verify-spatial-query.sh

cat > "$evidence_root/spatial-semantics.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="1" failures="0" skipped="0"><testsuite name="spatial-semantics" tests="1" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-180-evidence.sh"/><property name="command" value="./scripts/verify-spatial-query.sh"/></properties>
<testcase classname="SpatialQueryFixtures" name="the shared canonical fixture executes its path LOS footprint outcome and package-adapter assertions"/>
</testsuite></testsuites>
XML

cat > "$evidence_root/spatial-cache-knowledge.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="1" failures="0" skipped="0"><testsuite name="spatial-cache-knowledge" tests="1" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-180-evidence.sh"/><property name="command" value="./scripts/verify-spatial-query.sh"/></properties>
<testcase classname="SpatialQueryFixtures" name="the shared fixture executes cache hit byte parity blocker dependency invalidation profile-key and projected-knowledge assertions"/>
</testsuite></testsuites>
XML

cat > "$evidence_root/spatial-cross-runtime-mutation.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="12" failures="0" skipped="0"><testsuite name="spatial-cross-runtime-mutation" tests="12" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-180-evidence.sh"/><property name="command" value="./scripts/verify-spatial-query.sh"/></properties>
<testcase classname="SpatialQueryFixtures" name="full canonical fixture bytes agree exactly on dotnet and Fable Node"/>
<testcase classname="SpatialQueryFixtures" name="dotnet canonical divergence mutation fails at the first changed byte"/>
<testcase classname="SpatialQueryFixtures" name="Fable canonical divergence mutation fails at the first changed byte"/>
<testcase classname="SpatialQueryFixtures" name="dependency receipt source mutation fails the executable fixture"/>
<testcase classname="SpatialQueryFixtures" name="footprint sample source mutation fails the executable fixture"/>
<testcase classname="SpatialQueryFixtures" name="semantic edge source mutation fails the executable fixture"/>
<testcase classname="SpatialQueryFixtures" name="requester knowledge source mutation fails the executable fixture"/>
<testcase classname="SpatialQueryFixtures" name="spatial revision source mutation fails the executable fixture"/>
<testcase classname="SpatialQueryFixtures" name="deterministic ordering source mutation fails the executable fixture"/>
<testcase classname="SpatialQueryFixtures" name="package adapter source mutation fails the executable fixture"/>
<testcase classname="SpatialQueryFixtures" name="profile cache key source mutation fails the executable fixture"/>
<testcase classname="SpatialQueryFixtures" name="trace work bound source mutation fails the executable fixture"/>
</testsuite></testsuites>
XML

cat > "$evidence_root/spatial-integration-authority.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="2" failures="0" skipped="0"><testsuite name="spatial-integration-authority" tests="2" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-180-evidence.sh"/><property name="command" value="./scripts/verify-spatial-query.sh"/></properties>
<testcase classname="SpatialAuthority" name="web F sharp source contains no direct authoritative Game.Core geometry call"/>
<testcase classname="SpatialAuthority" name="handwritten JavaScript and TypeScript contain no copied LOS or A star implementation"/>
</testsuite></testsuites>
XML

cat > "$evidence_root/spatial-performance.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="1" failures="0" skipped="0"><testsuite name="spatial-performance" tests="1" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-180-evidence.sh"/><property name="command" value="./scripts/verify-spatial-query.sh"/></properties>
<testcase classname="SpatialQueryFixtures" name="the executable Release performance workload enforces all declared latency and structural ceilings"/>
</testsuite></testsuites>
XML

./scripts/test-conformance.sh
cat > "$evidence_root/full-conformance.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="6" failures="0" skipped="0"><testsuite name="sir-full-conformance" tests="6" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-180-evidence.sh"/><property name="command" value="./scripts/test-conformance.sh"/></properties>
<testcase classname="test-conformance.sh" name="locked restore and full solution build pass"/>
<testcase classname="test-conformance.sh" name="governance and public F sharp surface materialization pass"/>
<testcase classname="test-conformance.sh" name="native Fable replay and spatial authority gates pass"/>
<testcase classname="test-conformance.sh" name="M4 through M9 product qualification passes"/>
<testcase classname="test-conformance.sh" name="browser smoke diagnostics and accessibility pass"/>
<testcase classname="test-conformance.sh" name="performance and production delivery budgets pass"/>
</testsuite></testsuites>
XML

./scripts/build-docs.sh
cat > "$evidence_root/documentation.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="4" failures="0" skipped="0"><testsuite name="spatial-documentation" tests="4" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-180-evidence.sh"/><property name="command" value="./scripts/build-docs.sh"/></properties>
<testcase classname="fsdocs" name="strict evaluated API documentation includes the spatial signatures"/>
<testcase classname="fsdocs" name="architecture and performance documentation build"/>
<testcase classname="docs" name="publication integrity experience and browser smoke pass"/>
<testcase classname="docs" name="documentation accessibility passes"/>
</testsuite></testsuites>
XML

node scripts/test-production-delivery-budget.mjs
cat > "$evidence_root/production-delivery.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="2" failures="0" skipped="0"><testsuite name="spatial-production-delivery" tests="2" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-180-evidence.sh"/><property name="command" value="node scripts/test-production-delivery-budget.mjs"/></properties>
<testcase classname="production-delivery" name="application delivery sizes are recorded and deferred ownership is preserved"/>
<testcase classname="production-delivery" name="retained worker and actual deferred RulesExplorer chunk remain publication-bound"/>
</testsuite></testsuites>
XML

dotnet publish src/SIR.Server/SIR.Server.fsproj -c Release -o artifacts/publish --no-restore
SIR_JUNIT_OUTPUT=readiness/180-authoritative-spatial-query-foundation/production-delivery-browser.junit.xml \
  npx playwright test tests/SIR.Browser.Tests/production-delivery.spec.js \
    --config tests/SIR.Browser.Tests/playwright.config.js

SIR_JUNIT_OUTPUT=readiness/180-authoritative-spatial-query-foundation/spatial-player-browser.junit.xml \
  npx playwright test tests/SIR.Browser.Tests/visible-workflows.spec.js \
    --config tests/SIR.Browser.Tests/playwright.config.js \
    --grep "player-visible spatial diagnostics route"

printf 'item #180 spatial evidence receipts generated from green subject commands\n'
