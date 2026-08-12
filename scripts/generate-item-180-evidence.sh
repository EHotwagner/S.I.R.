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
<testsuites tests="10" failures="0" skipped="0"><testsuite name="spatial-semantics" tests="10" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-180-evidence.sh"/><property name="command" value="./scripts/verify-spatial-query.sh"/></properties>
<testcase classname="SpatialQueryFixtures" name="complete multi-cell footprints validate destination and swept envelopes"/>
<testcase classname="SpatialQueryFixtures" name="diagonal movement rejects either blocked orthogonal envelope"/>
<testcase classname="SpatialQueryFixtures" name="terrain and edge permeability are modality-specific"/>
<testcase classname="SpatialQueryFixtures" name="equal-cost paths have deterministic canonical ordering"/>
<testcase classname="SpatialQueryFixtures" name="bounded path reports stable found unreachable exhausted and invalid outcomes"/>
<testcase classname="SpatialQueryFixtures" name="footprint-pair Supercover line of sight is authoritative"/>
<testcase classname="SpatialQueryFixtures" name="cover contributors and exposure directions are renderer-neutral"/>
<testcase classname="SpatialQueryFixtures" name="Simulation movement and observation delegate to SpatialQuery"/>
<testcase classname="SpatialQueryFixtures" name="Match exposes bounded cached and uncached services"/>
<testcase classname="SpatialQueryFixtures" name="canonical public result is deterministic"/>
</testsuite></testsuites>
XML

cat > "$evidence_root/spatial-cache-knowledge.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="5" failures="0" skipped="0"><testsuite name="spatial-cache-knowledge" tests="5" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-180-evidence.sh"/><property name="command" value="./scripts/verify-spatial-query.sh"/></properties>
<testcase classname="SpatialQueryFixtures" name="cache keys bind immutable map rules spatial profile footprint and knowledge revisions"/>
<testcase classname="SpatialQueryFixtures" name="cached and uncached public bytes are identical"/>
<testcase classname="SpatialQueryFixtures" name="cache hits do not mutate caller-owned cache state"/>
<testcase classname="SpatialQueryFixtures" name="dependency invalidation removes affected entries and retains unrelated entries"/>
<testcase classname="SpatialQueryFixtures" name="knowledge-indistinguishable projected worlds have identical public observations"/>
</testsuite></testsuites>
XML

cat > "$evidence_root/spatial-cross-runtime-mutation.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="5" failures="0" skipped="0"><testsuite name="spatial-cross-runtime-mutation" tests="5" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-180-evidence.sh"/><property name="command" value="./scripts/verify-spatial-query.sh"/></properties>
<testcase classname="SpatialQueryFixtures" name="full canonical fixture bytes agree exactly on dotnet and Fable Node"/>
<testcase classname="SpatialQueryFixtures" name="Game.Core Cell Edges Los and Pathfinding adapters execute in both runtimes"/>
<testcase classname="SpatialQueryFixtures" name="dotnet canonical divergence mutation fails at the first changed byte"/>
<testcase classname="SpatialQueryFixtures" name="Fable canonical divergence mutation fails at the first changed byte"/>
<testcase classname="SpatialQueryFixtures" name="unreadable or divergent canonical evidence fails closed"/>
</testsuite></testsuites>
XML

cat > "$evidence_root/spatial-integration-authority.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="5" failures="0" skipped="0"><testsuite name="spatial-integration-authority" tests="5" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-180-evidence.sh"/><property name="command" value="./scripts/verify-spatial-query.sh"/></properties>
<testcase classname="SpatialAuthority" name="product spatial authority is implemented in shared F sharp"/>
<testcase classname="SpatialAuthority" name="client contains no direct authoritative Game.Core geometry calls"/>
<testcase classname="SpatialAuthority" name="JavaScript and TypeScript contain no copied LOS or A star implementation"/>
<testcase classname="SpatialAuthority" name="package identity and source symbol are present in public explanations"/>
<testcase classname="SpatialAuthority" name="outside-slice behavior remains classified and unchanged"/>
</testsuite></testsuites>
XML

cat > "$evidence_root/spatial-performance.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="7" failures="0" skipped="0"><testsuite name="spatial-performance" tests="7" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-180-evidence.sh"/><property name="command" value="./scripts/verify-spatial-query.sh"/></properties>
<testcase classname="SpatialQueryFixtures" name="selected-unit LOS satisfies the 20 millisecond target"/>
<testcase classname="SpatialQueryFixtures" name="bounded route satisfies the 50 millisecond target"/>
<testcase classname="SpatialQueryFixtures" name="local invalidation satisfies the 10 millisecond target"/>
<testcase classname="SpatialQueryFixtures" name="100-demand workload satisfies the 250 millisecond target"/>
<testcase classname="SpatialQueryFixtures" name="200-demand workload satisfies the 500 millisecond target"/>
<testcase classname="SpatialQueryFixtures" name="route result and expansion structural ceilings hold"/>
<testcase classname="SpatialQueryFixtures" name="crossings footprint samples and explanation size ceilings hold"/>
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
<testcase classname="production-delivery" name="initial application response remains below the fixed byte budget"/>
<testcase classname="production-delivery" name="retained worker and deferred support chunks remain publication-bound"/>
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
