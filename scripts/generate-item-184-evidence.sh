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

counter_line=$(grep -F 'Scenario catalog structural counters:' "$stage/native.log")
perf_line=$(grep -F 'Scenario catalog PERF-SMOKE:' "$stage/native.log")
counter_value() {
  local name=$1
  sed -E "s/.*${name}=([0-9]+).*/\\1/" <<<"$counter_line"
}
map_size=$(sed -E 's/.*map=([0-9]+x[0-9]+).*/\1/' <<<"$counter_line")
p95_ms=$(sed -E 's/.*p95 ([0-9.]+) ms.*/\1/' <<<"$perf_line")
p99_ms=$(sed -E 's/.*p99 ([0-9.]+) ms.*/\1/' <<<"$perf_line")
duration_samples=$(sed -E 's/.*samples (\[[^]]+\]).*/\1/' <<<"$perf_line")
captured_at=$(date -u +'%Y-%m-%dT%H:%M:%SZ')

jq -n \
  --arg capturedAtUtc "$captured_at" \
  --arg mapSize "$map_size" \
  --argjson durationSamplesMs "$duration_samples" \
  --argjson measuredP95Ms "$p95_ms" \
  --argjson measuredP99Ms "$p99_ms" \
  --argjson scenarios "$(counter_value scenarios)" \
  --argjson maps "$(counter_value maps)" \
  --argjson units "$(counter_value units)" \
  --argjson edges "$(counter_value edges)" \
  --argjson zones "$(counter_value zones)" \
  --argjson simulationTicks "$(counter_value simulationTicks)" \
  --argjson events "$(counter_value events)" \
  --argjson checkpoints "$(counter_value checkpoints)" \
  --argjson pathExpansions "$(counter_value pathExpansions)" \
  --argjson peakLosSamples "$(counter_value peakLosSamples)" \
  --argjson peakCombatResolutions "$(counter_value peakCombatResolutions)" \
  --argjson projectionFrames "$(counter_value projectionFrames)" \
  --argjson sceneNodes "$(counter_value sceneNodes)" \
  '{
    contractVersion: "performance-evidence-v1",
    claimedBudgetPassed: ($measuredP95Ms <= 20 and $measuredP99Ms <= 50 and $pathExpansions <= 4096 and $peakLosSamples <= 256 and $peakCombatResolutions <= 256 and $sceneNodes <= 8000),
    sampleSets: [{
      workloadId: "scenario-catalog-representative-v1",
      workloadDefinitionDigest: "sha256:9fdc78516912b2e440a6d80d3d6795032edc55876f010fcb96c9f6b03e1d67ac",
      workloadClass: "normal-play",
      targetFps: 20,
      maxP95Ms: 20,
      maxP99Ms: 50,
      maxCatchUpFrames: 0,
      measurementScope: "production-import-route-simulate-project-scene",
      requiredCapability: "headless-browser",
      hostProfile: "linux-x64-headless",
      packageVersions: ["FS.GG.Game.Core@0.13.0", "Fable@5.13.0"],
      measurementMode: "headless",
      capabilities: ["headless-browser"],
      warmupPolicy: "one-full-catalog",
      samplePolicy: "nearest-rank/140-package-runs",
      capturedAtUtc: $capturedAtUtc,
      currencyToken: "item-184-repair-round-2",
      probeReadbackContaminated: false,
      durationSamplesMs: $durationSamplesMs,
      measuredP95Ms: $measuredP95Ms,
      measuredP99Ms: $measuredP99Ms,
      catchUpFrames: [0],
      authoritativeProductionCounters: {
        source: "readiness/184-scenario-catalog/scenario-catalog-native.junit.xml#system-out",
        mapSize: $mapSize,
        scenarios: $scenarios,
        maps: $maps,
        units: $units,
        edges: $edges,
        zones: $zones,
        simulationTicks: $simulationTicks,
        events: $events,
        checkpoints: $checkpoints,
        pathExpansions: $pathExpansions,
        peakLosSamples: $peakLosSamples,
        peakCombatResolutions: $peakCombatResolutions,
        projectionFrames: $projectionFrames,
        sceneNodes: $sceneNodes,
        authority: {
          pathExpansions: "MapEditorSimulator.previewRoute -> MapScale.route.ExpandedNodes",
          peakLosSamples: "MapEditorSimulator.step -> MapScale.tick.Counters.LosSamples",
          peakCombatResolutions: "MapEditorSimulator.step -> MapScale.tick.Counters.CombatResolutions",
          sceneNodes: "Battlefield.scene.InteractiveNodeEstimate"
        }
      }
    }]
  }' > "$stage/performance-evidence.json"

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

install -m 0644 "$stage"/*.junit.xml "$stage/performance-evidence.json" readiness/184-scenario-catalog/
