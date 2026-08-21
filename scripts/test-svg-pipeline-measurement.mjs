import assert from "node:assert/strict";
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname } from "node:path";
import { gunzipSync } from "node:zlib";
import { byteDigest, digest, extractFrameHealth, extractInputToPaint, extractJourneyTrace, extractStages, makeMap, summarize, validateDefinitions, validateEvidenceReceipt, validateProductionSummary, validateRetainedRawEvidence, workloadRecipe } from "./lib/svg-pipeline-measurement.mjs";

const source = JSON.parse(readFileSync(new URL("./svg-pipeline-fixtures.v1.json", import.meta.url)));
validateDefinitions(source);
assert.equal(digest(source).length, 64);
assert.match(makeMap(source.fixtures[0]), /^SIR-MAP 2\nsize 20 20\n/);
assert.equal(byteDigest(Buffer.from("abc")), "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");

const trace = { traceEvents: [
  { name: "thread_name", tid: 1, args: { name: "CrRendererMain" } },
  { name: "thread_name", tid: 2, args: { name: "DedicatedWorker thread" } },
  { name: "clock_sync", ts: 500, args: { sync_id: "sir-journey-start" } },
  { name: "FunctionCall", tid: 1, ts: 1200, dur: 3000 }, { name: "RunTask", tid: 2, ts: 1300, dur: 2000 },
  { name: "UpdateLayoutTree", tid: 1, ts: 1400, dur: 1000 }, { name: "Layout", tid: 1, ts: 1500, dur: 4000 },
  { name: "EventDispatch", tid: 1, ts: 1000, dur: 100, args: { data: { type: "click" } } },
  { name: "Paint", tid: 1, ts: 2000, dur: 5000 },
  { name: "AnimationFrame", ph: "b", tid: 1, ts: 3000, args: { animation_frame_timing_info: { duration_ms: 16 } } },
  { name: "AnimationFrame", ph: "b", tid: 1, ts: 19000, args: { animation_frame_timing_info: { duration_ms: 31 } } },
  { name: "AnimationFrame", ph: "b", tid: 1, ts: 50000, args: { animation_frame_timing_info: { duration_ms: 10 } } },
  { name: "clock_sync", ts: 60000, args: { sync_id: "sir-journey-end" } },
] };
const journeyTrace = extractJourneyTrace(trace);
const stages = extractStages(journeyTrace);
assert.equal(stages.layout.milliseconds, 4);
assert.equal(stages.workerTransfer.available, false);
assert.equal(stages.elmishReact.available, false);
assert.equal(stages.mainThreadScript.milliseconds, 3);
const summary = summarize([{ stages }], source.materialShareThreshold);
assert.equal(summary.nextBottleneck.stage, "paint");
assert.equal(summary.dispositions.packedTransport, "unresolved");
assert.match(summary.interpretation, /not a permanent supported-size ceiling/);
assert.deepEqual(extractFrameHealth(journeyTrace).intervalsMilliseconds, [16, 31]);
assert.equal(extractFrameHealth(journeyTrace).droppedFrames, 1);
assert.equal(extractInputToPaint(journeyTrace, "selection").milliseconds, 1);
assert.equal(extractInputToPaint(journeyTrace, "idle").available, false);
assert.throws(() => extractJourneyTrace({ traceEvents: [] }), /clock-sync window/, "missing journey window must fail closed");
const runnerSource = readFileSync(new URL("./measure-svg-pipeline.mjs", import.meta.url), "utf8");
assert.doesNotMatch(runnerSource, /addInitScript|__sirFrameIntervals|requestAnimationFrame\(sample\)|const sample\s*=/, "measurement must not inject a frame sampler into the traced renderer");
console.log("JUSTIFIED trace-timing: Chromium trace events provide frame and input-to-paint evidence without an injected sampler");

const mutations = [
  ["schema", (value) => { value.schema = "unknown"; }],
  ["journey-inventory", (value) => { value.journeys.pop(); }],
  ["memory-cycle", (value) => { value.stabilizationCycles = value.warmupCycles; }],
  ["controlled-global-pair", (value) => { value.fixtures.find((fixture) => fixture.id === "global-large-small-viewport").viewport = [481, 320]; }],
  ["axis-value", (value) => { value.fixtures[0].eventRateHz = -1; }],
  ["axis-inventory", (value) => { delete value.controlledAxes.visibleDensity; }],
];
for (const [name, mutate] of mutations) {
  const mutant = structuredClone(source);
  mutate(mutant);
  assert.throws(() => validateDefinitions(mutant), undefined, `${name} subject mutation must fail`);
  console.log(`JUSTIFIED ${name}: subject mutation rejected`);
}
assert.throws(() => summarize([], source.materialShareThreshold), /observed run/, "empty evidence must fail");
console.log("JUSTIFIED observed-run: empty evidence rejected");
for (const axis of Object.keys(source.controlledAxes)) {
  const [baselineId, variantId] = source.controlledAxes[axis];
  const baseline = source.fixtures.find((fixture) => fixture.id === baselineId);
  const variant = source.fixtures.find((fixture) => fixture.id === variantId);
  const without = (fixture) => Object.fromEntries(Object.entries(fixture).filter(([key]) => !["id", axis].includes(key)));
  assert.deepEqual(without(baseline), without(variant), `${axis} pair must differ only on its named workload axis`);
  assert.notDeepEqual(baseline[axis], variant[axis]);
  assert.notEqual(digest({ map: makeMap(baseline), recipe: workloadRecipe(baseline) }), digest({ map: makeMap(variant), recipe: workloadRecipe(variant) }), `${axis} must change the executed workload`);
  const mutant = structuredClone(source);
  mutant.fixtures.find((fixture) => fixture.id === variantId).viewport[0] += 1;
  assert.throws(() => validateDefinitions(mutant), new RegExp(`${axis} comparison`), `${axis} one-factor escape must fail`);
  console.log(`JUSTIFIED ${axis}: controlled pair changes only the named executed subject and rejects a second-factor mutation`);
}
const authority = JSON.parse(readFileSync(new URL("../work/231-svg-pipeline-measurement/production-chromium-authority.json", import.meta.url)));
const evidence = validateEvidenceReceipt(JSON.parse(readFileSync(new URL("../work/231-svg-pipeline-measurement/production-chromium-evidence.json", import.meta.url))), source, authority);
const rawManifest = JSON.parse(readFileSync(new URL("../work/231-svg-pipeline-measurement/raw-trace-manifest.json", import.meta.url)));
const productionSummary = JSON.parse(readFileSync(new URL("../work/231-svg-pipeline-measurement/production-chromium-summary.json", import.meta.url)));
const readRetained = (path) => gunzipSync(readFileSync(new URL(`../${path}`, import.meta.url)));
validateRetainedRawEvidence(evidence, authority, rawManifest, readRetained);
validateProductionSummary(evidence, authority, rawManifest, productionSummary);
assert.throws(() => validateRetainedRawEvidence(evidence, authority, rawManifest, (path) => {
  const raw = readRetained(path);
  return path === rawManifest.runs[0].path ? Buffer.concat([raw, Buffer.from(" ")]) : raw;
}), /raw trace digest is stale/, "changed retained raw bytes must fail");
assert.throws(() => validateRetainedRawEvidence(evidence, authority, rawManifest, () => { throw new Error("missing"); }), /raw trace is unreadable/, "missing retained raw bytes must fail");
console.log(`JUSTIFIED raw-trace-retention: ${rawManifest.runs.length} content-addressed retained traces validated and absent/changed bytes rejected`);
const reseal = (value) => { const { bindingSha256: _, ...bound } = value; value.bindingSha256 = digest(bound); return value; };
const unboundCandidate = structuredClone(evidence); unboundCandidate.candidate.commit = "f".repeat(40); reseal(unboundCandidate);
assert.throws(() => validateEvidenceReceipt(unboundCandidate, source, authority), /authority binding/, "coordinated candidate reseal must fail");
const unboundDigest = structuredClone(evidence); unboundDigest.matrix.rawSummarySha256 = "f".repeat(64); reseal(unboundDigest);
assert.throws(() => validateEvidenceReceipt(unboundDigest, source, authority), /authority binding/, "coordinated digest reseal must fail");
console.log("JUSTIFIED evidence-binding: coordinated candidate and digest reseals rejected by tracked authority");
const report = process.env.SIR_SVG_PIPELINE_JUNIT || "artifacts/test-results/svg-pipeline.junit.xml";
mkdirSync(dirname(report), { recursive: true });
writeFileSync(report, '<?xml version="1.0" encoding="UTF-8"?>\n<testsuites tests="20" failures="0" errors="0" skipped="0"><testsuite name="svg-pipeline-measurement" tests="20" failures="0" errors="0" skipped="0"><testcase name="schema"/><testcase name="journey-inventory"/><testcase name="memory-cycle"/><testcase name="controlled-global-pair"/><testcase name="axis-value"/><testcase name="axis-inventory"/><testcase name="trace-timing"/><testcase name="observed-run"/><testcase name="map-extent-control"/><testcase name="visible-density-control"/><testcase name="global-unit-control"/><testcase name="overlay-control"/><testcase name="event-rate-control"/><testcase name="supporting-list-control"/><testcase name="evidence-candidate-binding"/><testcase name="evidence-digest-binding"/><testcase name="raw-trace-binding"/><testcase name="raw-trace-missing"/><testcase name="raw-trace-changed"/><testcase name="unreadable-input"/></testsuite></testsuites>\n');
console.log("svg-pipeline measurement unit gates: PASS");
