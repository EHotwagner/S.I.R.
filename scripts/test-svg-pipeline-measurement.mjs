import assert from "node:assert/strict";
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname } from "node:path";
import { gunzipSync } from "node:zlib";
import { byteDigest, digest, evaluateArtifactVerdict, evaluateRunFrameVerdict, extractFrameHealth, fixtureIdentityDigest, extractInputToPaint, extractJourneyTrace, extractStages, makeMap, summarize, validateDefinitions, validateEvidenceReceipt, validateObservedControls, validateProductionSummary, validateRetainedRawEvidence, workloadRecipe } from "./lib/svg-pipeline-measurement.mjs";

const source = JSON.parse(readFileSync(new URL("./svg-pipeline-fixtures.v1.json", import.meta.url)));
validateDefinitions(source);
assert.equal(digest(source).length, 64);
assert.match(makeMap(source.fixtures[0]), /^SIR-MAP 2\nsize 30 30\n/);
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
assert.match(runnerSource, /sceneRevisionBeforeImport[\s\S]*revision !== previous/, "measurement must wait for the imported production scene, not only its announcement");
console.log("JUSTIFIED trace-timing: Chromium trace events provide frame and input-to-paint evidence without an injected sampler");

const mutations = [
  ["schema", (value) => { value.schema = "unknown"; }],
  ["journey-inventory", (value) => { value.journeys.pop(); }],
  ["memory-cycle", (value) => { value.stabilizationCycles = value.warmupCycles; }],
  ["controlled-global-pair", (value) => { value.fixtures.find((fixture) => fixture.id === "global-large-small-viewport").viewport = [481, 320]; }],
  ["axis-value", (value) => { value.fixtures[0].eventRateHz = -1; }],
  ["fixture-capacity", (value) => { value.fixtures.find((fixture) => fixture.id === "controlled-global-units").globalUnitCount = 901; }],
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
for (const fixture of source.fixtures) {
  const coordinates = makeMap(fixture).split("\n").filter((line) => line.startsWith("unit ")).map((line) => line.split(" ").slice(4, 6).join(","));
  assert.equal(new Set(coordinates).size, fixture.globalUnitCount, `${fixture.id} must generate one unique cell per unit`);
}
console.log("JUSTIFIED unique-unit-cells: every workload unit occupies a distinct production map cell");
const observedSummary = { runs: source.fixtures.flatMap((fixture) => source.journeys.map((journey) => ({
  fixture: fixture.id,
  journey,
  structural: {
    visible: { visualUnits: fixture.visibleDensity, projectedUnits: fixture.globalUnitCount },
    cameraControl: { fitCompleteMap: true, viewport: fixture.viewport, centerAnchoredWheelSteps: 15, wheelDeltaY: -240 },
  },
}))) };
validateObservedControls(observedSummary, source);
const visibleEscape = structuredClone(observedSummary);
for (const run of visibleEscape.runs.filter((value) => value.fixture === "controlled-visible-density")) run.structural.visible.visualUnits = 40;
assert.throws(() => validateObservedControls(visibleEscape, source), /visible-density observation is uncontrolled/, "a declared-only visible-density pair must fail");
const globalEscape = structuredClone(observedSummary);
for (const run of globalEscape.runs.filter((value) => value.fixture === "controlled-global-units")) run.structural.visible.visualUnits = 200;
assert.throws(() => validateObservedControls(globalEscape, source), /global-unit observation changes visible density/, "a global-count pair that changes the visible set must fail");
console.log("JUSTIFIED production-observed-controls: the exact former declared-only visible-density and global-count escapes are rejected");
const authority = JSON.parse(readFileSync(new URL("../work/231-svg-pipeline-measurement/production-chromium-authority.json", import.meta.url)));
const evidence = validateEvidenceReceipt(JSON.parse(readFileSync(new URL("../work/231-svg-pipeline-measurement/production-chromium-evidence.json", import.meta.url))), source, authority);
const rawManifest = JSON.parse(readFileSync(new URL("../work/231-svg-pipeline-measurement/raw-trace-manifest.json", import.meta.url)));
const productionSummary = JSON.parse(readFileSync(new URL("../work/231-svg-pipeline-measurement/production-chromium-summary.json", import.meta.url)));
const readRetained = (path) => gunzipSync(readFileSync(new URL(`../${path}`, import.meta.url)));
validateRetainedRawEvidence(evidence, authority, rawManifest, readRetained);
validateProductionSummary(evidence, authority, rawManifest, productionSummary, source);
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
writeFileSync(report, '<?xml version="1.0" encoding="UTF-8"?>\n<testsuites tests="25" failures="0" errors="0" skipped="0"><testsuite name="svg-pipeline-measurement" tests="25" failures="0" errors="0" skipped="0"><testcase name="schema"/><testcase name="journey-inventory"/><testcase name="memory-cycle"/><testcase name="controlled-global-pair"/><testcase name="axis-value"/><testcase name="fixture-capacity"/><testcase name="axis-inventory"/><testcase name="trace-timing"/><testcase name="trace-window"/><testcase name="observed-run"/><testcase name="map-extent-control"/><testcase name="visible-density-control"/><testcase name="global-unit-control"/><testcase name="overlay-control"/><testcase name="event-rate-control"/><testcase name="supporting-list-control"/><testcase name="unique-unit-cells"/><testcase name="production-visible-observation"/><testcase name="production-global-observation"/><testcase name="evidence-candidate-binding"/><testcase name="evidence-digest-binding"/><testcase name="raw-trace-binding"/><testcase name="raw-trace-missing"/><testcase name="raw-trace-changed"/><testcase name="unreadable-input"/></testsuite></testsuites>\n');
console.log("svg-pipeline measurement unit gates: PASS");

// --- #268: the verdict must be derived from measurements, and must be able to say "fail" ---
// These gates exist because `result` was a literal "pass" in the producer and the finalizer gated on it.
// Each one is paired with a subject mutation recorded in PR #300; predicate inversion is not evidence here.
const budget = source.frameBudget;
assert.ok(budget, "the fixture contract must declare a frame budget");
assert.equal(budget.callbackMillisecondsCeiling, 16.67);

// the declared ceiling discriminates, in BOTH directions -- a gate that only ever reds is as useless as
// one that only ever greens, and 16 vs 17 ms is the boundary real runs actually sit on
assert.equal(evaluateRunFrameVerdict({ frameDurationsMilliseconds: [16.0] }, "zoom", budget).result, "pass");
assert.equal(evaluateRunFrameVerdict({ frameDurationsMilliseconds: [16.67] }, "zoom", budget).result, "pass");
assert.equal(evaluateRunFrameVerdict({ frameDurationsMilliseconds: [17.0] }, "zoom", budget).result, "fail");

// a non-answer must never be reported as a confident answer
assert.equal(evaluateRunFrameVerdict({ frameDurationsMilliseconds: [] }, "idle", budget).result, "unevaluated",
  "idle emits no AnimationFrame records; it must not claim a verdict it did not measure");
assert.equal(evaluateRunFrameVerdict({ frameDurationsMilliseconds: [] }, "playback", budget).result, "fail",
  "a journey that is NOT declared exempt and produced no frames is a broken measurement, not an exemption");
assert.equal(evaluateRunFrameVerdict(undefined, "pan", budget).result, "fail",
  "an entirely absent frameHealth must fail closed");
// A duration the gate cannot read is not a duration under the ceiling: `null > ceiling` is false, so
// without this an unreadable value counts as conforming -- this item's own defect class, in miniature.
assert.equal(evaluateRunFrameVerdict({ frameDurationsMilliseconds: [12, null] }, "zoom", budget).result, "fail",
  "a null frame duration cannot be evaluated and must not be counted as conforming");
assert.equal(evaluateRunFrameVerdict({ frameDurationsMilliseconds: [12, Number.NaN] }, "zoom", budget).result, "fail",
  "a NaN frame duration must fail closed");
assert.equal(evaluateRunFrameVerdict({ frameDurationsMilliseconds: [12, "3"] }, "zoom", budget).result, "fail",
  "a non-numeric frame duration must fail closed");
assert.equal(evaluateRunFrameVerdict({ frameDurationsMilliseconds: [12, 13] }, "zoom", budget).result, "pass",
  "and the all-finite control must still pass, so the guard is not simply reddening everything");

// the artifact verdict answers a different question from a run verdict
assert.equal(evaluateArtifactVerdict([{ frameBudget: { result: "pass" } }, { frameBudget: { result: "fail" } }]).result, "fail");
assert.equal(evaluateArtifactVerdict([{ frameBudget: { result: "pass" } }, { frameBudget: { result: "unevaluated" } }]).result, "pass");
assert.equal(evaluateArtifactVerdict([{ frameBudget: { result: "unevaluated" } }]).result, "fail",
  "a matrix of nothing but exemptions is not a pass");
assert.throws(() => evaluateArtifactVerdict([{}]), /derived frameBudget verdict/,
  "a run with no derived verdict must fail closed rather than be counted as passing");

// the contract refuses to operate with no declared budget, rather than inventing one
assert.throws(() => validateDefinitions({ ...source, frameBudget: undefined }), /declares no frameBudget/);
assert.throws(() => validateDefinitions({ ...source, frameBudget: { ...budget, evaluatedPercentiles: [] } }), /evaluatedPercentiles/);
assert.throws(() => validateDefinitions({ ...source, frameBudget: { ...budget, callbackMillisecondsCeiling: 0 } }), /callbackMillisecondsCeiling/);

// the workload binding must NOT move when only the budget changes, or historical evidence is falsely
// reported as coming from different fixtures and can never be re-evaluated against a corrected budget
assert.equal(fixtureIdentityDigest(source), fixtureIdentityDigest({ ...source, frameBudget: { ...budget, callbackMillisecondsCeiling: 99 } }),
  "the fixture identity binds the workload, not the budget applied to it");
assert.notEqual(fixtureIdentityDigest(source), fixtureIdentityDigest({ ...source, warmupCycles: source.warmupCycles + 1 }),
  "a real workload change must still move the fixture identity");

// the real production matrix in the tree breaches the declared budget and must say so
const budgetMatrix = JSON.parse(readFileSync(new URL("../work/231-svg-pipeline-measurement/production-chromium-summary.json", import.meta.url)));
const budgetVerdicts = budgetMatrix.runs.map((run) => ({ frameBudget: evaluateRunFrameVerdict(run.frameHealth, run.journey, budget) }));
assert.equal(evaluateArtifactVerdict(budgetVerdicts).result, "fail",
  "the retained 231 ms / 123-dropped-frame matrix must not finalize as a passing matrix");
assert.equal(budgetVerdicts.filter((run) => run.frameBudget.result === "fail").length, 36);
assert.equal(budgetVerdicts.filter((run) => run.frameBudget.result === "pass").length, 12,
  "genuinely conforming runs must still pass, so the gate is not merely inverted");
// Closed domain, not a sample. The declared journey set is finite and lives in the fixture contract, so
// every member is exercised rather than the four that happened to be interesting. A repair that
// enumerated only the journeys someone had observed would pass a sampled suite and fail this one.
const verdictStates = new Set(["pass", "fail", "unevaluated"]);
for (const journey of source.journeys) {
  const exempt = budget.exemptJourneys.includes(journey);
  const starved = evaluateRunFrameVerdict({ frameDurationsMilliseconds: [] }, journey, budget);
  assert.equal(starved.result, exempt ? "unevaluated" : "fail",
    `${journey}: a journey with no frames must be ${exempt ? "unevaluated because it is declared exempt" : "a failure because it is not"}`);
  // exemption is about ABSENT frames, not about the journey being unmeasurable: give it frames and it
  // is judged like any other, so an exempt journey can never launder a real breach.
  assert.equal(evaluateRunFrameVerdict({ frameDurationsMilliseconds: [17.0] }, journey, budget).result, "fail",
    `${journey}: a breaching frame must fail even for a declared-exempt journey`);
  assert.equal(evaluateRunFrameVerdict({ frameDurationsMilliseconds: [12.0] }, journey, budget).result, "pass",
    `${journey}: a conforming frame must pass`);
  assert.ok(verdictStates.has(starved.result), `${journey}: verdict must be one of the three declared states`);
}
assert.equal(source.journeys.length, 7, "the closed journey domain this loop asserts over must not silently shrink");
assert.ok(budget.exemptJourneys.every((journey) => source.journeys.includes(journey)),
  "an exempt journey that is not a declared journey would exempt nothing");

console.log("JUSTIFIED frame-budget-verdict: derived per-run and artifact verdicts red on the retained breaching matrix, green on its conforming runs, and fail closed on unmeasured input");
