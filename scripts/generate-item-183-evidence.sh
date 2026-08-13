#!/usr/bin/env bash
set -euo pipefail

repo_root=$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)
stage=$(mktemp -d)
trap 'rm -rf -- "$stage"' EXIT
cd "$repo_root"

junit_pass() {
  local path=$1 name=$2 seconds=$3 detail=${4:-}
  local system_out=""
  if [[ -n "$detail" ]]; then
    system_out="<system-out><![CDATA[$detail]]></system-out>"
  fi
  printf '%s\n' '<?xml version="1.0" encoding="UTF-8"?>' \
    "<testsuites name=\"$name\" tests=\"1\" failures=\"0\" errors=\"0\" skipped=\"0\" time=\"$seconds\"><testsuite name=\"$name\" tests=\"1\" failures=\"0\" errors=\"0\" skipped=\"0\" time=\"$seconds\"><testcase name=\"$name\" classname=\"SIR.Item183\" time=\"$seconds\"/>$system_out</testsuite></testsuites>" > "$path"
}

start=$SECONDS
dotnet restore tests/SIR.Client.Tests/SIR.Client.Tests.fsproj
dotnet build tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release --no-restore
dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj -c Release --no-build | tee "$stage/native.log"
native_summary=$(grep 'Tactical scene projection qualification passed:' "$stage/native.log")
junit_pass "$stage/tactical-overlays-native.junit.xml" tactical-overlays-native "$((SECONDS-start))" "$native_summary"

start=$SECONDS
npm run build:client
junit_pass "$stage/tactical-overlays-fable-production.junit.xml" tactical-overlays-fable-production "$((SECONDS-start))"

dotnet restore src/SIR.Server/SIR.Server.fsproj
dotnet publish src/SIR.Server/SIR.Server.fsproj -c Release -o artifacts/publish --no-restore
SIR_JUNIT_OUTPUT="$stage/tactical-overlays-browser.junit.xml" npx playwright test \
  --config tests/SIR.Browser.Tests/playwright.config.js \
  tests/SIR.Browser.Tests/visible-workflows.spec.js -g 'analysis overlays'

start=$SECONDS
./fake.sh build -t Test
junit_pass "$stage/tactical-overlays-aggregate.junit.xml" tactical-overlays-aggregate "$((SECONDS-start))"

install -m 0644 "$stage"/*.junit.xml readiness/183-tactical-overlays/
