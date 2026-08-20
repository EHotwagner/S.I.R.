import assert from "node:assert/strict";
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname } from "node:path";
import { digest, extractStages, makeMap, summarize, validateDefinitions } from "./lib/svg-pipeline-measurement.mjs";

const source = JSON.parse(readFileSync(new URL("./svg-pipeline-fixtures.v1.json", import.meta.url)));
validateDefinitions(source);
assert.equal(digest(source).length, 64);
assert.match(makeMap(source.fixtures[0]), /^SIR-MAP 2\nsize 20 20\n/);

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
const summary = summarize([{ stages }], source.materialShareThreshold);
assert.equal(summary.nextBottleneck.stage, "paint");
assert.equal(summary.dispositions.packedTransport, "deferred");
assert.match(summary.interpretation, /not a permanent supported-size ceiling/);

const mutations = [
  ["schema", (value) => { value.schema = "unknown"; }],
  ["journey-inventory", (value) => { value.journeys.pop(); }],
  ["memory-cycle", (value) => { value.stabilizationCycles = value.warmupCycles; }],
  ["controlled-global-pair", (value) => { value.fixtures.find((fixture) => fixture.id === "global-large-small-viewport").viewport = [481, 320]; }],
  ["axis-value", (value) => { value.fixtures[0].eventRateHz = -1; }],
];
for (const [name, mutate] of mutations) {
  const mutant = structuredClone(source);
  mutate(mutant);
  assert.throws(() => validateDefinitions(mutant), undefined, `${name} subject mutation must fail`);
  console.log(`JUSTIFIED ${name}: subject mutation rejected`);
}
assert.throws(() => summarize([], source.materialShareThreshold), /observed run/, "empty evidence must fail");
console.log("JUSTIFIED observed-run: empty evidence rejected");
const report = process.env.SIR_SVG_PIPELINE_JUNIT || "artifacts/test-results/svg-pipeline.junit.xml";
mkdirSync(dirname(report), { recursive: true });
writeFileSync(report, '<?xml version="1.0" encoding="UTF-8"?>\n<testsuites tests="7" failures="0" errors="0" skipped="0"><testsuite name="svg-pipeline-measurement" tests="7" failures="0" errors="0" skipped="0"><testcase name="schema"/><testcase name="journey-inventory"/><testcase name="memory-cycle"/><testcase name="controlled-global-pair"/><testcase name="axis-value"/><testcase name="observed-run"/><testcase name="ranking"/></testsuite></testsuites>\n');
console.log("svg-pipeline measurement unit gates: PASS");
