import assert from "node:assert/strict";
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname } from "node:path";
import { byteDigest, digest, extractStages, makeMap, summarize, validateDefinitions, validateEvidenceReceipt, workloadRecipe } from "./lib/svg-pipeline-measurement.mjs";

const source = JSON.parse(readFileSync(new URL("./svg-pipeline-fixtures.v1.json", import.meta.url)));
validateDefinitions(source);
assert.equal(digest(source).length, 64);
assert.match(makeMap(source.fixtures[0]), /^SIR-MAP 2\nsize 20 20\n/);
assert.equal(byteDigest(Buffer.from("abc")), "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");

const trace = { traceEvents: [
  { name: "thread_name", tid: 1, args: { name: "CrRendererMain" } },
  { name: "thread_name", tid: 2, args: { name: "DedicatedWorker thread" } },
  { name: "FunctionCall", tid: 1, dur: 3000 }, { name: "RunTask", tid: 2, dur: 2000 },
  { name: "UpdateLayoutTree", tid: 1, dur: 1000 }, { name: "Layout", tid: 1, dur: 4000 },
  { name: "Paint", tid: 1, dur: 5000 }, { name: "DrawFrame", tid: 1, dur: 1000 },
] };
const stages = extractStages(trace);
assert.equal(stages.layout.milliseconds, 4);
assert.equal(stages.workerTransfer.available, false);
assert.equal(stages.elmishReact.available, false);
assert.equal(stages.mainThreadScript.milliseconds, 3);
const summary = summarize([{ stages }], source.materialShareThreshold);
assert.equal(summary.nextBottleneck.stage, "paint");
assert.equal(summary.dispositions.packedTransport, "unresolved");
assert.match(summary.interpretation, /not a permanent supported-size ceiling/);

const mutations = [
  ["schema", (value) => { value.schema = "unknown"; }],
  ["journey-inventory", (value) => { value.journeys.pop(); }],
  ["memory-cycle", (value) => { value.stabilizationCycles = value.warmupCycles; }],
  ["controlled-global-pair", (value) => { value.fixtures.find((fixture) => fixture.id === "global-large-small-viewport").viewport = [481, 320]; }],
  ["axis-value", (value) => { value.fixtures[0].eventRateHz = -1; }],
  ["event-rate-matrix", (value) => { for (const fixture of value.fixtures) fixture.eventRateHz = 20; }],
  ["event-rate-control", (value) => { value.fixtures.find((fixture) => fixture.id === "representative-visible-100-high-rate").supportingListSize += 1; }],
];
for (const [name, mutate] of mutations) {
  const mutant = structuredClone(source);
  mutate(mutant);
  assert.throws(() => validateDefinitions(mutant), undefined, `${name} subject mutation must fail`);
  console.log(`JUSTIFIED ${name}: subject mutation rejected`);
}
assert.throws(() => summarize([], source.materialShareThreshold), /observed run/, "empty evidence must fail");
console.log("JUSTIFIED observed-run: empty evidence rejected");
for (const [axis, mutate] of [
  ["visible-density-workload", (value) => { value.visibleDensity -= 1; }],
  ["event-rate-workload", (value) => { value.eventRateHz += 10; }],
  ["supporting-list-workload", (value) => { value.supportingListSize += 1; }],
]) {
  const fixture = structuredClone(source.fixtures[0]);
  const before = digest({ map: makeMap(fixture), recipe: workloadRecipe(fixture) });
  mutate(fixture);
  assert.notEqual(digest({ map: makeMap(fixture), recipe: workloadRecipe(fixture) }), before, `${axis} must change the executed workload`);
  console.log(`JUSTIFIED ${axis}: workload mutation changed the executed subject`);
}
const authority = JSON.parse(readFileSync(new URL("../work/231-svg-pipeline-measurement/production-chromium-authority.json", import.meta.url)));
const evidence = validateEvidenceReceipt(JSON.parse(readFileSync(new URL("../work/231-svg-pipeline-measurement/production-chromium-evidence.json", import.meta.url))), source, authority);
const reseal = (value) => { const { bindingSha256: _, ...bound } = value; value.bindingSha256 = digest(bound); return value; };
const unboundCandidate = structuredClone(evidence); unboundCandidate.candidate.commit = "f".repeat(40); reseal(unboundCandidate);
assert.throws(() => validateEvidenceReceipt(unboundCandidate, source, authority), /authority binding/, "coordinated candidate reseal must fail");
const unboundDigest = structuredClone(evidence); unboundDigest.matrix.rawSummarySha256 = "f".repeat(64); reseal(unboundDigest);
assert.throws(() => validateEvidenceReceipt(unboundDigest, source, authority), /authority binding/, "coordinated digest reseal must fail");
console.log("JUSTIFIED evidence-binding: coordinated candidate and digest reseals rejected by tracked authority");
const report = process.env.SIR_SVG_PIPELINE_JUNIT || "artifacts/test-results/svg-pipeline.junit.xml";
mkdirSync(dirname(report), { recursive: true });
writeFileSync(report, '<?xml version="1.0" encoding="UTF-8"?>\n<testsuites tests="13" failures="0" errors="0" skipped="0"><testsuite name="svg-pipeline-measurement" tests="13" failures="0" errors="0" skipped="0"><testcase name="schema"/><testcase name="journey-inventory"/><testcase name="memory-cycle"/><testcase name="controlled-global-pair"/><testcase name="axis-value"/><testcase name="event-rate-matrix"/><testcase name="event-rate-control"/><testcase name="observed-run"/><testcase name="visible-density-workload"/><testcase name="event-rate-workload"/><testcase name="supporting-list-workload"/><testcase name="evidence-candidate-binding"/><testcase name="evidence-digest-binding"/></testsuite></testsuites>\n');
console.log("svg-pipeline measurement unit gates: PASS");
