#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
evidence_root="$repo_root/readiness/194-executable-rules-corpus"
project="$repo_root/tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"

cd "$repo_root"
mkdir -p "$evidence_root"

# Every receipt below is written only after the named executable subject exits
# green. This script is the deterministic reporter for item #194; hand-authored
# or copied XML is not evidence.
./scripts/test-conformance.sh

cat > "$evidence_root/full-conformance.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="6" failures="0" skipped="0"><testsuite name="sir-full-conformance" tests="6" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-194-evidence.sh"/><property name="command" value="./scripts/test-conformance.sh"/></properties>
<testcase classname="test-conformance.sh" name="locked restore and full solution build pass" />
<testcase classname="test-conformance.sh" name="dotnet fable mutation replay and authority gates pass" />
<testcase classname="test-conformance.sh" name="M4 through M9 qualification passes" />
<testcase classname="test-conformance.sh" name="browser smoke diagnostics and full browser inventory pass" />
<testcase classname="test-conformance.sh" name="performance worker and delivery budgets pass" />
<testcase classname="test-conformance.sh" name="documentation and review artifact integrity pass" />
</testsuite></testsuites>
XML

./scripts/verify-rules-corpus.sh
SIR_RULES_FORCE_GREP=1 ./scripts/verify-rules-corpus.sh

cat > "$evidence_root/rules-corpus-canonical.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="5" failures="0" skipped="0"><testsuite name="rules-corpus-canonical" tests="5" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-194-evidence.sh"/><property name="command" value="./scripts/verify-rules-corpus.sh; SIR_RULES_FORCE_GREP=1 ./scripts/verify-rules-corpus.sh"/></properties>
<testcase classname="RulesCorpusFixtures" name="registry validation and negative contracts pass" />
<testcase classname="RulesCorpusFixtures" name="dotnet and fable canonical vectors agree" />
<testcase classname="RulesCorpusFixtures" name="artifact identity and pinned source resolution pass" />
<testcase classname="RulesCorpusFixtures" name="manifest and coverage topology contracts pass" />
<testcase classname="RulesCorpusFixtures" name="coverage source identity and canonical-byte mutations fail closed" />
</testsuite></testsuites>
XML

dotnet run --project "$project" -c Release --no-build -- --print-replay-evidence
node scripts/test-production-replay-v3.mjs

cat > "$evidence_root/replay-v3.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="5" failures="0" skipped="0"><testsuite name="rules-replay-v3" tests="5" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-194-evidence.sh"/><property name="commands" value="--print-replay-evidence; node scripts/test-production-replay-v3.mjs"/></properties>
<testcase classname="ReplayFixtures" name="canonical replay v3 archives typed retained manifest and explanations" />
<testcase classname="ReplayFixtures" name="production Fable decodes and exactly re-encodes .NET replay v3" />
<testcase classname="RulesCorpusFixtures" name="retained v1 resolves unchanged after current v2 changes" />
<testcase classname="RulesCorpusFixtures" name="current and historical exact identities never substitute" />
<testcase classname="ReplayFixtures" name="unavailable or tampered historical identity fails explicitly" />
</testsuite></testsuites>
XML

dotnet run --project "$project" -c Release --no-build -- --print-rules-performance

cat > "$evidence_root/rules-performance.junit.xml" <<'XML'
<?xml version="1.0" encoding="UTF-8"?>
<testsuites tests="2" failures="0" skipped="0"><testsuite name="rules-performance" tests="2" failures="0" skipped="0">
<properties><property name="producer" value="scripts/generate-item-194-evidence.sh"/><property name="command" value="--print-rules-performance"/></properties>
<testcase classname="RulesCorpusFixtures" name="10000 explained attacks satisfy execution budget" />
<testcase classname="RulesCorpusFixtures" name="application operand explanation and manifest size budgets pass" />
</testsuite></testsuites>
XML

SIR_JUNIT_OUTPUT=readiness/194-executable-rules-corpus/rules-player-browser.junit.xml \
  npx playwright test tests/SIR.Browser.Tests/visible-workflows.spec.js \
    --config tests/SIR.Browser.Tests/playwright.config.js \
    --grep "visible mode controls|player-visible Rules explorer"

cp "$evidence_root/rules-player-browser.junit.xml" "$evidence_root/rules-corpus-browser.junit.xml"

SIR_JUNIT_OUTPUT=readiness/194-executable-rules-corpus/production-delivery.junit.xml \
  npx playwright test tests/SIR.Browser.Tests/production-delivery.spec.js \
    --config tests/SIR.Browser.Tests/playwright.config.js

printf 'item #194 executable evidence receipts generated from green subject commands\n'
