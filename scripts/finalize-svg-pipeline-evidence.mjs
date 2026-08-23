import { readFileSync, writeFileSync } from "node:fs";
import { resolve } from "node:path";
import { digest, evaluateArtifactVerdict, evaluateRunFrameVerdict, percentile, stableJson, validateDefinitions } from "./lib/svg-pipeline-measurement.mjs";

const summaryPath = resolve(process.argv[2] || "artifacts/svg-pipeline/summary.json");
const root = resolve(new URL("..", import.meta.url).pathname);
const work = resolve(root, "work/231-svg-pipeline-measurement");
const definitions = validateDefinitions(JSON.parse(readFileSync(resolve(root, "scripts/svg-pipeline-fixtures.v1.json"))));
const summary = JSON.parse(readFileSync(summaryPath));
const manifest = JSON.parse(readFileSync(resolve(work, "raw-trace-manifest.json")));
const expectedRunCount = definitions.fixtures.length * definitions.journeys.length;
if (summary?.schema !== "sir.svg-pipeline-measurement/1" || summary.runs?.length !== expectedRunCount) throw new Error("production summary is not the complete matrix");

// Re-derive the verdict here rather than trusting summary.result. The defect this finalizer sat beside
// was a consumer gating on a field the producer wrote as a constant, so the consumer now recomputes the
// verdict from the measured values and the declared budget, and refuses BOTH a genuine breach and any
// disagreement with what the artifact claims about itself.
const rederivedRuns = summary.runs.map((run) => ({ ...run, frameBudget: evaluateRunFrameVerdict(run.frameHealth, run.journey, definitions.frameBudget) }));
const rederived = evaluateArtifactVerdict(rederivedRuns);
if (rederived.result !== "pass") throw new Error(`production summary breaches the declared frame budget and is refused: ${rederived.reason}`);
const disagreeing = rederivedRuns.filter((run, index) => summary.runs[index].frameBudget?.result !== undefined && run.frameBudget.result !== summary.runs[index].frameBudget.result);
if (disagreeing.length) throw new Error(`production summary run verdicts disagree with the budget re-derived from their own measurements: ${disagreeing.map((run) => `${run.fixture}/${run.journey} claims ${summary.runs.find((c) => c.fixture === run.fixture && c.journey === run.journey)?.frameBudget?.result} but measures ${run.frameBudget.result}`).slice(0, 5).join("; ")}`);
if (summary.result !== "pass") throw new Error(`production summary is not a passing matrix: it declares result "${summary.result}"`);
if (summary.rawTraceManifest?.sha256 !== digest(manifest)) throw new Error("production summary does not bind the retained raw-trace manifest");
const fixtureIds = definitions.fixtures.map((fixture) => fixture.id);
if (stableJson(summary.selection?.fixtures) !== stableJson(fixtureIds) || stableJson(summary.selection?.journeys) !== stableJson(definitions.journeys)) throw new Error("production summary selection is incomplete or reordered");
const expectedRuns = fixtureIds.flatMap((fixture) => definitions.journeys.map((journey) => `${fixture}\u0000${journey}`));
const observedRuns = summary.runs.map((run) => `${run.fixture}\u0000${run.journey}`);
if (stableJson(observedRuns) !== stableJson(expectedRuns)) throw new Error("production summary run order is incomplete");
if (stableJson(summary.runs.map((run) => ({ fixture: run.fixture, journey: run.journey, sha256: run.trace?.sha256 }))) !== stableJson(manifest.runs.map(({ fixture, journey, sha256 }) => ({ fixture, journey, sha256 })))) throw new Error("summary and retained trace manifest disagree");

writeFileSync(resolve(work, "production-chromium-summary.json"), stableJson(summary));
const rawSummarySha256 = digest(summary);
const rawTraceManifestSha256 = digest(manifest);
const orderedTraceDigestSha256 = digest(manifest.runs.map(({ fixture, journey, sha256 }) => ({ fixture, journey, sha256 })));
const authority = {
  schema: "sir.svg-pipeline-measurement-authority/1",
  candidate: summary.candidate,
  buildIdentity: summary.buildIdentity,
  fixtureDefinitionSha256: summary.fixtureDefinition.sha256,
  rawSummarySha256,
  rawTraceManifestSha256,
  orderedTraceDigestSha256,
  runCount: expectedRunCount,
};
writeFileSync(resolve(work, "production-chromium-authority.json"), stableJson(authority));

const latencies = summary.runs.filter((run) => run.inputLatency?.available).map((run) => run.inputLatency.milliseconds).sort((a, b) => a - b);
const stageUnavailable = Object.fromEntries(Object.entries(summary.runs[0].stages).filter(([, value]) => !value.available).map(([name, value]) => [name, value.reason]));
const pairRows = (ids, journey) => ids.map((id) => summary.runs.find((run) => run.fixture === id && run.journey === journey));
const eventRateComparison = pairRows(definitions.controlledAxes.eventRateHz, "playback").map((run) => ({
  fixture: run.fixture,
  eventRateHz: run.workload.eventRateHz,
  eventIntervalMilliseconds: run.workload.eventIntervalMilliseconds,
  playbackWindowMilliseconds: run.workload.playbackWindowMilliseconds,
  playbackSteps: run.workload.playbackSteps,
  mainScriptMilliseconds: run.stages.mainThreadScript.milliseconds,
  inputToPaint: run.inputLatency,
  frameSamples: run.frameHealth.samples,
  droppedFrames: run.frameHealth.droppedFrames,
}));
const globalSmallViewportComparison = pairRows(definitions.globalScalePair, "idle").map((run) => ({
  fixture: run.fixture,
  global: run.structural.global,
  visible: run.structural.visible,
  mainScriptMilliseconds: run.stages.mainThreadScript.milliseconds,
  inputLatency: run.inputLatency,
  memory: { warmUsed: run.memory.warm.usedSize, stabilizedUsed: run.memory.stabilized.usedSize, delta: run.memory.usedDelta, warmupCycles: run.memory.warmupCycles, stabilizationCycles: run.memory.stabilizationCycles },
}));
const evidence = {
  schema: "sir.svg-pipeline-measurement-evidence/1",
  candidate: summary.candidate,
  buildIdentity: summary.buildIdentity,
  fixtureDefinition: summary.fixtureDefinition,
  environment: { browser: summary.environment.browserVersion, node: summary.environment.node, platform: `${summary.environment.platform} ${summary.environment.release}`, cpuCount: summary.environment.cpuCount },
  matrix: {
    fixtureCount: definitions.fixtures.length,
    fixtureIds,
    journeyCount: definitions.journeys.length,
    runCount: expectedRunCount,
    result: "pass",
    startedAt: summary.runs[0].startedAt,
    completedAt: summary.runs.at(-1).completedAt,
    journeys: definitions.journeys,
    rawSummarySha256,
    rawTraceManifestSha256,
    orderedTraceDigestSha256,
    inputToPaintMilliseconds: { availableRunCount: latencies.length, unavailableRunCount: expectedRunCount - latencies.length, minimum: latencies[0] ?? null, p95: percentile(latencies, 0.95), maximum: latencies.at(-1) ?? null },
  },
  pipeline: {
    nextBottleneck: summary.summary.nextBottleneck.stage,
    evidence: `${summary.summary.nextBottleneck.evidence}; generic main-thread script is not source-symbol isolated`,
    ranking: summary.summary.ranking,
    unavailable: stageUnavailable,
  },
  controlledAxes: Object.fromEntries(Object.entries(definitions.controlledAxes).map(([axis, ids]) => [axis, ids.map((id) => ({ fixture: id, value: definitions.fixtures.find((fixture) => fixture.id === id)[axis] }))])),
  eventRateComparison,
  globalSmallViewportComparison,
  dispositions: summary.summary.dispositions,
  interpretation: summary.summary.interpretation,
};
evidence.bindingSha256 = digest(evidence);
writeFileSync(resolve(work, "production-chromium-evidence.json"), stableJson(evidence));
console.log(`svg-pipeline evidence finalized: runs=${expectedRunCount} candidate=${summary.candidate.commit} raw=${manifest.runs.length}`);
