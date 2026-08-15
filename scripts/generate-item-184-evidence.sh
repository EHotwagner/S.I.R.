#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
stage=$(mktemp -d)
trap 'rm -rf -- "$stage"' EXIT
cd "$repo_root"

junit_pass() {
  local path=$1 name=$2 seconds=$3 detail=$4
  printf '%s\n' '<?xml version="1.0" encoding="UTF-8"?>' \
    "<testsuites name=\"$name\" tests=\"1\" failures=\"0\" errors=\"0\" skipped=\"0\" time=\"$seconds\"><testsuite name=\"$name\" tests=\"1\" failures=\"0\" errors=\"0\" skipped=\"0\" time=\"$seconds\"><testcase name=\"$name\" classname=\"SIR.Item184\" time=\"$seconds\"/><system-out><![CDATA[$detail]]></system-out></testsuite></testsuites>" > "$path"
}

start=$SECONDS
dotnet restore tests/SIR.Client.Tests/SIR.Client.Tests.fsproj --locked-mode
dotnet build tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release --no-restore
dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release --no-build --no-restore | tee "$stage/native.log"
junit_pass "$stage/scenario-catalog-native.junit.xml" scenario-catalog-native "$((SECONDS-start))" "$(grep -E 'Scenario catalog (qualification|structural counters|PERF-SMOKE)' "$stage/native.log")"

start=$SECONDS
./scripts/verify-scenario-catalog-cross-runtime.sh | tee "$stage/parity.log"
junit_pass "$stage/scenario-catalog-cross-runtime.junit.xml" scenario-catalog-cross-runtime "$((SECONDS-start))" "$(cat "$stage/parity.log")"

npm run build:client
dotnet restore src/SIR.Server/SIR.Server.fsproj --locked-mode
dotnet publish src/SIR.Server/SIR.Server.fsproj -c Release -o artifacts/publish --no-restore
SIR_JUNIT_OUTPUT="$stage/scenario-catalog-browser.junit.xml" npx playwright test \
  --config tests/SIR.Browser.Tests/playwright.config.js \
  tests/SIR.Browser.Tests/tactical-samples.spec.js

start=$SECONDS
./scripts/verify-rules-corpus.sh | tee "$stage/rules.log"
junit_pass "$stage/scenario-catalog-rules.junit.xml" scenario-catalog-rules "$((SECONDS-start))" "$(cat "$stage/rules.log")"

install -m 0644 "$stage"/*.junit.xml readiness/184-scenario-catalog/
