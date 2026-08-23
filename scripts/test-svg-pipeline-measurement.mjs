import assert from "node:assert/strict";
import { copyFileSync, mkdirSync, mkdtempSync, readdirSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { dirname, resolve } from "node:path";
import { tmpdir } from "node:os";
import { spawnSync } from "node:child_process";
import { gunzipSync } from "node:zlib";
import { byteDigest, digest, evaluateArtifactVerdict, evaluateRunFrameVerdict, extractFrameHealth, fixtureIdentityDigest, measurementReport, extractInputToPaint, extractJourneyTrace, extractStages, makeMap, summarize, validateDefinitions, validateEvidenceReceipt, validateObservedControls, validateProductionSummary, validateRetainedRawEvidence, workloadRecipe } from "./lib/svg-pipeline-measurement.mjs";
import { documentedFrameCeilingCell, documentedTacticalBudgetRows, tacticalBudgetSurfaces, tacticalBudgetTableDocumentation, tacticalFrameBudget, tacticalFrameBudgetDocumentation, tacticalFrameCadenceBudget, tacticalFrameCadenceBudgetReason, tacticalInputToPaintBudgetReason, tacticalReviewManifestBudgets, tacticalRuntimeEffectCap, tacticalStructuralBudgetReason, tacticalWorkloadBudgetAtScale, tacticalWorkloadBudgetFor, tacticalWorkloadBudgetList } from "./lib/performance-budget.mjs";

const source = JSON.parse(readFileSync(new URL("./svg-pipeline-fixtures.v1.json", import.meta.url)));
// validateDefinitions COMPOSES the budget: workload policy from the fixture file, ceiling from the
// single declaration. `source.frameBudget` is the raw file and carries no ceiling at all.
const definitions = validateDefinitions(source);
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
assert.deepEqual(extractFrameHealth(journeyTrace, definitions.frameBudget).intervalsMilliseconds, [16, 31]);
assert.equal(extractFrameHealth(journeyTrace, definitions.frameBudget).droppedFrames, 1,
  "of the 16/31/10 ms frames only the 31 ms one exceeds the declared ceiling; if this count moved, the drop threshold moved with the declaration, which is the intended derivation -- update the declaration deliberately, not this number");
assert.throws(() => extractFrameHealth(journeyTrace), /undeclared threshold/,
  "frame health must refuse to count dropped frames with no declared ceiling rather than fall back to a literal");
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

// --- #268: the verdict must be derived from measurements, and must be able to say "fail" ---
// These gates exist because `result` was a literal "pass" in the producer and the finalizer gated on it.
// Each one is paired with a subject mutation recorded in PR #300; predicate inversion is not evidence here.
const rawBudget = source.frameBudget;          // what the fixture FILE says
const budget = definitions.frameBudget;        // what validateDefinitions composed
assert.ok(rawBudget, "the fixture contract must declare a frame budget block");

// --- #299: ONE declaration, derived everywhere else -------------------------------------------
// There is deliberately no numeric literal in this section. A literal here is what would let a
// broken derivation stay green: the suite would pin the number it was supposed to be checking, and
// a consumer that had stopped deriving would still agree with it by coincidence.
assert.ok(!("callbackMillisecondsCeiling" in rawBudget),
  "the fixture file must not restate the ceiling; it is derived from the single declaration");
assert.equal(budget.callbackMillisecondsCeiling, tacticalFrameBudget.callbackMillisecondsCeiling,
  "the composed budget's ceiling must BE the declared one, not a copy that agrees");
assert.equal(budget.droppedFrameCeilingMilliseconds, tacticalFrameBudget.callbackMillisecondsCeiling,
  "the dropped-frame threshold and the budget ceiling are one number, not two that agree");
// and the fixture file may not smuggle it back in
assert.throws(() => validateDefinitions({ ...source, frameBudget: { ...rawBudget, callbackMillisecondsCeiling: 12 } }),
  /must not restate callbackMillisecondsCeiling/,
  "a fixture file that restates the ceiling must be refused, not silently preferred or ignored");

// The published prose table is a PROJECTION of the declaration, and this gate is what makes that
// true rather than aspirational. It fails closed: a table it cannot find is a failure, not a pass.
const budgetDoc = readFileSync(new URL(`../${tacticalFrameBudgetDocumentation.path}`, import.meta.url), "utf8");
const headingIndex = budgetDoc.indexOf(`## ${tacticalFrameBudgetDocumentation.tableHeading}`);
assert.ok(headingIndex >= 0, `${tacticalFrameBudgetDocumentation.path} has no "${tacticalFrameBudgetDocumentation.tableHeading}" section`);
// Take the FIRST contiguous pipe-table under that heading. Filtering the whole remainder of the file
// would silently sweep in every later table in the document and compare the wrong cells.
const linesAfterHeading = budgetDoc.slice(headingIndex).split("\n");
const tableStart = linesAfterHeading.findIndex((line) => line.trim().startsWith("|"));
assert.ok(tableStart >= 0, `no table found under "${tacticalFrameBudgetDocumentation.tableHeading}"`);
let tableEnd = tableStart;
while (tableEnd < linesAfterHeading.length && linesAfterHeading[tableEnd].trim().startsWith("|")) tableEnd += 1;
const budgetRows = linesAfterHeading.slice(tableStart, tableEnd);
assert.ok(budgetRows.length >= 3, "the tactical visual-system budget table must have a header, a separator and at least one row");
const headerCells = budgetRows[0].split("|").map((cell) => cell.trim());
const ceilingColumn = headerCells.indexOf(tacticalFrameBudgetDocumentation.column);
assert.ok(ceilingColumn > 0, `the budget table has no "${tacticalFrameBudgetDocumentation.column}" column`);
const publishedCeilings = budgetRows.slice(2).map((row) => row.split("|").map((cell) => cell.trim())[ceilingColumn]);
assert.ok(publishedCeilings.length > 0, "the budget table declares no workload rows");
for (const cell of publishedCeilings)
  assert.equal(cell, documentedFrameCeilingCell(),
    `${tacticalFrameBudgetDocumentation.path} publishes "${cell}" for the declared ceiling; the declaration says "${documentedFrameCeilingCell()}". The document is a projection of scripts/lib/performance-budget.mjs and must follow it.`);

// A dropped frame is the SAME quantity as a budget breach, so the two thresholds cannot diverge.
// This is the case the old inline 25 ms literal got wrong, and the case the pre-existing trace
// fixture could not see: with durations of 16/31/10 ms, the old and the declared threshold both
// count exactly one frame, so it could not have discriminated them.
const overCeiling = tacticalFrameBudget.callbackMillisecondsCeiling + 3.33;   // breaches; was NOT counted at 25 ms
const underCeiling = tacticalFrameBudget.callbackMillisecondsCeiling - 0.67;  // conforms; must never be counted
const discriminating = { traceEvents: [
  { name: "thread_name", tid: 1, args: { name: "CrRendererMain" } },
  { name: "clock_sync", ts: 500, args: { sync_id: "sir-journey-start" } },
  { name: "AnimationFrame", ph: "b", tid: 1, ts: 3000, args: { animation_frame_timing_info: { duration_ms: underCeiling } } },
  { name: "AnimationFrame", ph: "b", tid: 1, ts: 19000, args: { animation_frame_timing_info: { duration_ms: overCeiling } } },
  { name: "clock_sync", ts: 60000, args: { sync_id: "sir-journey-end" } },
] };
const discriminatingHealth = extractFrameHealth(extractJourneyTrace(discriminating), budget);
assert.equal(discriminatingHealth.droppedFrames, 1,
  "a frame that breaches the declared ceiling must be counted as dropped; under the old undeclared 25 ms threshold this frame breached the budget and was reported as zero drops");
assert.equal(evaluateRunFrameVerdict(discriminatingHealth, "zoom", budget).result, "fail",
  "and the verdict must agree with the drop count on the very same frame -- one number, one answer");
// No consumer may RESTATE the ceiling. This is what keeps sites the in-process assertions above
// cannot reach -- the Chromium review generator and the Playwright spec, both of which need a browser
// to execute -- from drifting back into their own copy of the number. A file that contains no literal
// ceiling cannot hold a stale one, whatever it does at runtime.
//
// The consumer set is DERIVED from the tree, not enumerated. It was a hand-maintained list, and it
// silently omitted finalize-svg-pipeline-evidence.mjs -- one of only two scripts that receive the
// composed budget -- so a stale ceiling planted there was invisible to this scan (S.I.R.#299 F1). A
// hand-maintained list of consumers, inside the gate that exists to stop hand-maintained copies of a
// number, is the same disease one level up. Anything that imports the declaration or receives the
// composed budget is scanned, whether or not anyone remembered to add it.
const declaredCeilingLiteral = String(tacticalFrameBudget.callbackMillisecondsCeiling);
const consumesBudget = /performance-budget\.mjs|definitions\.frameBudget|validateDefinitions\(/;
const sourceRoots = [["./", ".mjs"], ["./lib/", ".mjs"], ["../tests/SIR.Browser.Tests/", ".spec.js"]];
const budgetConsumers = sourceRoots.flatMap(([dir, extension]) => readdirSync(new URL(dir, import.meta.url))
  .filter((name) => name.endsWith(extension))
  .map((name) => `${dir}${name}`)
  // the declaration itself is the ONE place the literal belongs
  .filter((path) => path !== "./lib/performance-budget.mjs")
  .filter((path) => consumesBudget.test(readFileSync(new URL(path, import.meta.url), "utf8"))));

assert.ok(budgetConsumers.length >= 5, `the consumer sweep found only ${budgetConsumers.length} files; it is not reaching the tree and would pass vacuously`);
for (const consumer of budgetConsumers) {
  const text = readFileSync(new URL(consumer, import.meta.url), "utf8");
  assert.ok(!text.includes(declaredCeilingLiteral),
    `${consumer} restates the declared ceiling as the literal ${declaredCeilingLiteral}. Import it from lib/performance-budget.mjs instead; a second copy that agrees today is exactly the defect S.I.R.#299 removed.`);
}
// the fixture CONTRACT is data rather than code, so it is checked structurally instead: validateDefinitions
// refuses a restated ceiling outright, and the raw-file assertion above proves the key is absent.
assert.ok(!readFileSync(new URL("./svg-pipeline-fixtures.v1.json", import.meta.url), "utf8").includes(declaredCeilingLiteral),
  "the fixture contract must not restate the declared ceiling");
// the two that cannot be exercised in-process must actually READ the declaration.
//
// S.I.R.#318 REPLACED a by-name match on `tacticalFrameBudget.callbackMillisecondsCeiling` here. That
// spelling asserted one route to the number and would have reported a consumer BROKEN for deriving it
// through a different declared export -- which is what the review generator now does, receiving the
// whole published budget block from the declaration and naming no field of it. Matching the spelling
// was the weaker claim in both directions: it also passes on a consumer that MENTIONS the expression
// in a comment and gates on something else. So the derivation is now RESOLVED rather than matched:
// the bindings each consumer imports from the declaration are looked up in the module and at least one
// must actually carry the declared ceiling.
const declarationModule = await import("./lib/performance-budget.mjs");
const carriesValue = (value, target, seen = new Set()) => {
  if (value === target) return true;
  if (typeof value !== "object" || value === null || seen.has(value)) return false;
  seen.add(value);
  return Object.values(value).some((nested) => carriesValue(nested, target, seen));
};
for (const consumer of ["./generate-tactical-visual-review.mjs", "../tests/SIR.Browser.Tests/visible-workflows.spec.js"]) {
  assert.ok(budgetConsumers.includes(consumer), `${consumer} must be reached by the consumer sweep`);
  const text = readFileSync(new URL(consumer, import.meta.url), "utf8");
  const importMatch = text.match(/import\s*\{([^}]*)\}\s*from\s*["'][^"']*performance-budget\.mjs["']/);
  assert.ok(importMatch, `${consumer} must import the single declaration`);
  const importedBindings = importMatch[1].split(",").map((binding) => binding.trim().split(/\s+as\s+/)[0]).filter(Boolean);
  assert.ok(importedBindings.length > 0, `${consumer} imports the declaration module but binds nothing from it`);
  for (const binding of importedBindings)
    assert.ok(binding in declarationModule, `${consumer} imports ${binding} from the declaration, which exports no such binding`);
  assert.ok(importedBindings.some((binding) => carriesValue(declarationModule[binding], tacticalFrameBudget.callbackMillisecondsCeiling)),
    `${consumer} imports [${importedBindings.join(", ")}] from the declaration, and none of them carries the declared ceiling ${tacticalFrameBudget.callbackMillisecondsCeiling}. This consumer cannot be exercised in-process, so this resolution is the only evidence that it derives the number rather than restating it.`);
}
// and the consumer this gate MISSED must now be reached by it, by derivation rather than by memory
assert.ok(budgetConsumers.includes("./finalize-svg-pipeline-evidence.mjs"),
  "the finalizer receives the composed budget and must be reached by the consumer sweep");
console.log(`JUSTIFIED frame-budget-no-restatement: ${budgetConsumers.length} budget consumers DERIVED from the tree contain no literal ${declaredCeilingLiteral}, the fixture contract restates nothing, and the two browser-route consumers read the declaration by name`);
console.log(`JUSTIFIED frame-budget-single-declaration: the fixture file, the composed budget, the drop threshold and ${publishedCeilings.length} published table cell(s) all resolve to the one declaration, a restated ceiling is refused, and a frame ${overCeiling} ms long -- uncounted under the old undeclared threshold -- is counted as dropped and fails its verdict`);

// the declared ceiling discriminates, in BOTH directions -- a gate that only ever reds is as useless as
// one that only ever greens, and just-below vs just-above is the boundary real runs actually sit on.
// S.I.R.#299: these were literals pinning the ceiling. They are now taken FROM it, so they follow the
// declaration instead of quietly becoming a fourth statement of it.
const ceiling = tacticalFrameBudget.callbackMillisecondsCeiling;
assert.equal(evaluateRunFrameVerdict({ frameDurationsMilliseconds: [ceiling - 0.67] }, "zoom", budget).result, "pass");
assert.equal(evaluateRunFrameVerdict({ frameDurationsMilliseconds: [ceiling] }, "zoom", budget).result, "pass",
  "the ceiling is inclusive: a frame exactly at it conforms");
assert.equal(evaluateRunFrameVerdict({ frameDurationsMilliseconds: [ceiling + 0.33] }, "zoom", budget).result, "fail");

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
assert.throws(() => validateDefinitions({ ...source, frameBudget: { ...rawBudget, evaluatedPercentiles: [] } }), /evaluatedPercentiles/);

// the workload binding must NOT move when only the budget changes, or historical evidence is falsely
// reported as coming from different fixtures and can never be re-evaluated against a corrected budget
assert.equal(fixtureIdentityDigest(source), fixtureIdentityDigest({ ...source, frameBudget: { ...rawBudget, callbackMillisecondsCeiling: 99 } }),
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

// The CONSUMER side, exercised as a process rather than as a library.
//
// Removing the finalizer's refusal left this suite green while the finalizer accepted a matrix with a
// 231 ms frame: the refusal was demonstrated by hand and asserted nowhere. That is the same shape as
// .github#304 -- a check whose evidence read a manual run to decide rather than the thing under test.
// So this runs the real script, in a sandbox root it resolves for itself, and reads its exit status.
const sandbox = mkdtempSync(resolve(tmpdir(), "sir-svg-finalizer-"));
mkdirSync(resolve(sandbox, "scripts/lib"), { recursive: true });
mkdirSync(resolve(sandbox, "work/231-svg-pipeline-measurement"), { recursive: true });
for (const file of ["finalize-svg-pipeline-evidence.mjs", "svg-pipeline-fixtures.v1.json", "lib/svg-pipeline-measurement.mjs", "lib/performance-budget.mjs"])
  copyFileSync(new URL(`./${file}`, import.meta.url), resolve(sandbox, "scripts", file));
copyFileSync(new URL("../work/231-svg-pipeline-measurement/raw-trace-manifest.json", import.meta.url),
  resolve(sandbox, "work/231-svg-pipeline-measurement/raw-trace-manifest.json"));

const finalize = (summary) => {
  const path = resolve(sandbox, "candidate.json");
  writeFileSync(path, JSON.stringify(summary));
  return spawnSync(process.execPath, [resolve(sandbox, "scripts/finalize-svg-pipeline-evidence.mjs"), path], { encoding: "utf8" });
};

// the retained production matrix, unmodified: it breaches, so the finalizer must REFUSE it
const breaching = finalize(budgetMatrix);
assert.notEqual(breaching.status, 0, "the finalizer must refuse a matrix that breaches the declared budget");
assert.match(`${breaching.stderr}`, /breaches the declared frame budget/,
  "and it must refuse it FOR the budget, not incidentally for some other reason");

// the same matrix with one dimension changed -- every frame brought within the ceiling -- must be ACCEPTED,
// so the refusal is shown discriminating rather than refusing everything handed to it
const conforming = structuredClone(budgetMatrix);
for (const run of conforming.runs) {
  const durations = run.frameHealth?.frameDurationsMilliseconds;
  if (Array.isArray(durations) && durations.length) run.frameHealth.frameDurationsMilliseconds = durations.map((value) => Math.min(value, 12));
}
const accepted = finalize(conforming);
assert.equal(accepted.status, 0, `a conforming matrix must finalize, got: ${accepted.stderr}`);

// F1: NEITHER fixture above can observe the threshold this refusal is NAMED for. `breaching` carries a
// 231 ms frame that breaches EVERY candidate ceiling; `conforming` clamps to 12 ms, which conforms to
// EVERY candidate. Both sit outside the discriminating band, so the pair returns the same verdict
// whether the finalizer gates on the declared ceiling or on the undeclared 25 ms literal this item
// removed -- the refusal was decorative with respect to its own number, while asserting it refused
// "FOR the budget". That is the same defect this suite repaired for extractFrameHealth, one file over.
//
// The superseded literal is named ON PURPOSE: a band fixture is only meaningful relative to the
// alternative it must exclude, and without it the band value is just another magic number.
const supersededDropThresholdMilliseconds = 25;
const bandMilliseconds = tacticalFrameBudget.callbackMillisecondsCeiling + 3.33;
assert.ok(tacticalFrameBudget.callbackMillisecondsCeiling < bandMilliseconds && bandMilliseconds < supersededDropThresholdMilliseconds,
  `the band fixture must lie strictly between the declared ceiling and the superseded ${supersededDropThresholdMilliseconds} ms literal, or it discriminates nothing`);

const inBand = structuredClone(conforming);
for (const run of inBand.runs) {
  const durations = run.frameHealth?.frameDurationsMilliseconds;
  if (Array.isArray(durations) && durations.length) run.frameHealth.frameDurationsMilliseconds = durations.map(() => bandMilliseconds);
}
// Prove the two sides CAN differ before trusting the refusal: the SAME matrix must be judged fail by the
// declared budget and pass by the superseded one. Without this the refusal below could be firing for a
// reason unrelated to the ceiling, and this gate would measure nothing.
const bandUnderDeclared = evaluateArtifactVerdict(inBand.runs.map((run) => ({ frameBudget: evaluateRunFrameVerdict(run.frameHealth, run.journey, budget) })));
const bandUnderSuperseded = evaluateArtifactVerdict(inBand.runs.map((run) => ({ frameBudget: evaluateRunFrameVerdict(run.frameHealth, run.journey, { ...budget, callbackMillisecondsCeiling: supersededDropThresholdMilliseconds }) })));
assert.equal(bandUnderDeclared.result, "fail", "the band matrix must BREACH the declared ceiling");
assert.equal(bandUnderSuperseded.result, "pass",
  `the band matrix must CONFORM to the superseded ${supersededDropThresholdMilliseconds} ms literal -- if it failed under both, this fixture would discriminate nothing`);

// and now the real consumer, as a process: it must refuse the band matrix, and refuse it FOR the budget
const bandRefused = finalize(inBand);
assert.notEqual(bandRefused.status, 0,
  `the finalizer must refuse a matrix that breaches the DECLARED ceiling but would pass the superseded ${supersededDropThresholdMilliseconds} ms literal; accepting it means the finalizer is gating on a threshold no document declares`);
assert.match(`${bandRefused.stderr}`, /breaches the declared frame budget/,
  "and it must refuse the band matrix FOR the budget, not incidentally for some other reason");
console.log(`JUSTIFIED frame-budget-finalizer-discriminates: the finalizer refuses a matrix at ${bandMilliseconds} ms, which breaches the declared ceiling and conforms to the superseded ${supersededDropThresholdMilliseconds} ms literal, so its refusal observes the declared number rather than agreeing with it by accident`);

// and an artifact that CLAIMS a pass its own measurements do not support must be refused, because the
// finalizer re-derives instead of trusting the field -- this is the route a bad merge reintroduces
// The finalizer has THREE independent refusals and each needs a fixture that actually reaches it.
// An earlier version of this block built its contradiction case from the BREACHING matrix, so it was
// refused for the budget at the first check and never reached the integrity checks it was named for --
// the assertion named for contradiction-refusal was provided by the budget refusal. Each case below is
// therefore built from the CONFORMING matrix, so the budget check passes and the next one is reached,
// and each matches its own error text rather than merely asserting a non-zero status.
const perRunContradiction = structuredClone(conforming);
const contradicted = perRunContradiction.runs.find((run) => (run.frameHealth?.frameDurationsMilliseconds || []).length);
contradicted.frameBudget = { result: "fail" };
const perRunRefusal = finalize(perRunContradiction);
assert.notEqual(perRunRefusal.status, 0, "a run claiming a verdict its own measurements contradict must be refused");
assert.match(`${perRunRefusal.stderr}`, /run verdicts disagree with the budget re-derived from their own measurements/,
  "and refused FOR the disagreement, not incidentally for the budget");
assert.match(`${perRunRefusal.stderr}`, new RegExp(`${contradicted.fixture}/${contradicted.journey} claims fail but measures pass`),
  "naming the run that disagrees");

const artifactContradiction = structuredClone(conforming);
artifactContradiction.result = "fail";
const artifactRefusal = finalize(artifactContradiction);
assert.notEqual(artifactRefusal.status, 0, "an artifact whose own result contradicts its conforming runs must be refused");
assert.match(`${artifactRefusal.stderr}`, /is not a passing matrix: it declares result "fail"/,
  "and refused by the artifact-result check, which is a different refusal from the per-run one");
rmSync(sandbox, { recursive: true, force: true });
console.log("JUSTIFIED frame-budget-finalizer: each of the three refusals reached by a fixture that gets past the preceding one, and each matched by its own error text");

console.log("JUSTIFIED frame-budget-verdict: derived per-run and artifact verdicts red on the retained breaching matrix, green on its conforming runs, and fail closed on unmeasured input");

// --- #268 F2: the surfaces an operator reads must carry the derived verdict, not a literal ---
// The artifact said `fail` while the exit code, the printed line and the JUnit report all said pass
// unconditionally. All three now read one derivation, so they are asserted here rather than in a browser.
const failingRun = { fixture: "controlled-baseline", journey: "playback", result: "fail", frameBudget: { reason: `measured p95=124 ms against the declared ceiling of ${tacticalFrameBudget.callbackMillisecondsCeiling} ms` } };
const passingRun = { fixture: "controlled-baseline", journey: "selection", result: "pass", frameBudget: { reason: "within ceiling" } };
const exemptRun = { fixture: "controlled-baseline", journey: "idle", result: "unevaluated", frameBudget: { reason: "declared frame-exempt and produced no AnimationFrame records" } };

const failingReport = measurementReport([failingRun, passingRun, exemptRun], evaluateArtifactVerdict([
  { frameBudget: { result: "fail" } }, { frameBudget: { result: "pass" } }, { frameBudget: { result: "unevaluated" } }]));
assert.equal(failingReport.exitCode, 1, "a breaching matrix must exit non-zero, not report success to the shell");
assert.match(failingReport.summaryLine, /^svg-pipeline: FAIL /, "the printed line must say FAIL when the verdict is fail");
assert.match(failingReport.junitXml, /failures="1"/, "the JUnit report must count the failing run");
assert.match(failingReport.junitXml, /skipped="1"/, "and must count the unevaluated run as skipped, not passed");
assert.match(failingReport.junitXml, /<failure message="[^"]*p95=124 ms/, "the failing testcase must carry its measured reason");
assert.match(failingReport.junitXml, /name="idle"><skipped/, "the unevaluated run must be skipped rather than silently passing");

const passingReport = measurementReport([passingRun], evaluateArtifactVerdict([{ frameBudget: { result: "pass" } }]));
assert.equal(passingReport.exitCode, 0, "a conforming matrix must still exit zero, so the gate is not merely inverted");
assert.match(passingReport.summaryLine, /^svg-pipeline: PASS /);
assert.match(passingReport.junitXml, /failures="0"/);
assert.doesNotMatch(passingReport.junitXml, /<failure/, "a passing run must carry no failure element");

// a matrix of nothing but exemptions is not a pass, and the surfaces must say so too
const exemptOnly = measurementReport([exemptRun], evaluateArtifactVerdict([{ frameBudget: { result: "unevaluated" } }]));
assert.equal(exemptOnly.exitCode, 1, "an all-exempt matrix claims no pass, so it must not exit zero");
assert.throws(() => measurementReport([passingRun], undefined), /derived artifact verdict is required/,
  "reporting without a derived verdict must fail closed rather than invent one");
console.log("JUSTIFIED frame-budget-report-surfaces: exit status, printed line and JUnit report all derive from the one verdict");


// --- #318: the rest of the tactical budget table, declared once and derived everywhere ----------
// #299 brought ONE column of the published table here. These gates bring the rest, and the two
// figures that had no row in it at all. As above, there is deliberately no numeric budget literal in
// this section: a literal here would pin the number this section exists to check.

// 1. THE PUBLISHED TABLE IS A PROJECTION OF THE DECLARATION, COLUMN BY COLUMN AND ROW BY ROW.
// This reuses the table already located and bounded above (`budgetRows`, `headerCells`), so the two
// gates cannot come to disagree about which table they are reading. It fails closed in BOTH
// directions: a declared column the document does not publish, a declared row the document does not
// carry, and a published cell the declaration does not say are each a failure.
const documentedRows = documentedTacticalBudgetRows();
assert.equal(tacticalBudgetTableDocumentation.tableHeading, tacticalFrameBudgetDocumentation.tableHeading,
  "both declarations must name the same published table, or these gates read different documents");
const publishedRows = budgetRows.slice(2).map((row) => row.split("|").map((cell) => cell.trim()));
assert.equal(publishedRows.length, documentedRows.length,
  `${tacticalBudgetTableDocumentation.path} publishes ${publishedRows.length} workload row(s); the declaration declares ${documentedRows.length}`);
let boundCells = 0;
for (const [rowIndex, declaredRow] of documentedRows.entries()) {
  const workloadColumn = headerCells.indexOf(tacticalBudgetTableDocumentation.workloadColumn);
  assert.ok(workloadColumn > 0, `the budget table has no "${tacticalBudgetTableDocumentation.workloadColumn}" column`);
  assert.equal(publishedRows[rowIndex][workloadColumn], declaredRow[tacticalBudgetTableDocumentation.workloadColumn],
    "the published rows must be the declared workloads, in the declared order");
  for (const [column, declaredCell] of Object.entries(declaredRow)) {
    const columnIndex = headerCells.indexOf(column);
    assert.ok(columnIndex > 0, `${tacticalBudgetTableDocumentation.path} publishes no "${column}" column, but the declaration projects one into it`);
    assert.equal(publishedRows[rowIndex][columnIndex], declaredCell,
      `${tacticalBudgetTableDocumentation.path} publishes "${publishedRows[rowIndex][columnIndex]}" in "${column}" for ${declaredRow[tacticalBudgetTableDocumentation.workloadColumn]}; the declaration says "${declaredCell}". The document is a projection of scripts/lib/performance-budget.mjs and must follow it.`);
    boundCells += 1;
  }
}
console.log(`JUSTIFIED tactical-budget-document-binding: ${boundCells} published cell(s) across ${documentedRows.length} row(s) resolve to the declaration, and a column or row the declaration projects that the document does not publish is a failure rather than a skip`);

// 2. THE RUNTIME EFFECT CAP IS THE SAME NUMBER, NOT ONE THAT AGREES.
// F# cannot import the declaration, so the binding is a gate, exactly as for the prose table. It
// fails closed on a source it cannot read or cannot find the binding in: an unanswerable question is
// not agreement (#266).
const projectionSource = readFileSync(new URL(`../${tacticalRuntimeEffectCap.path}`, import.meta.url), "utf8");
const capMatches = [...projectionSource.matchAll(new RegExp(`let\\s+(?:private\\s+)?${tacticalRuntimeEffectCap.binding}\\s*=\\s*(\\d+)`, "g"))];
assert.equal(capMatches.length, 1,
  `${tacticalRuntimeEffectCap.path} must bind ${tacticalRuntimeEffectCap.binding} exactly once for this gate to read it; found ${capMatches.length}`);
assert.equal(Number(capMatches[0][1]), tacticalRuntimeEffectCap.maximumEffectInstances,
  `${tacticalRuntimeEffectCap.path} enforces ${tacticalRuntimeEffectCap.binding} = ${capMatches[0][1]}, and the declaration publishes ${tacticalRuntimeEffectCap.maximumEffectInstances} as the stress-row effect ceiling. These are ONE number -- the product truncates to it and surfaces it as ${tacticalRuntimeEffectCap.surfacedAs} -- so a divergence in either direction is a failure.`);
assert.ok(projectionSource.includes(tacticalRuntimeEffectCap.surfacedAs) || readFileSync(new URL("../src/SIR.Client.Web/App.fs", import.meta.url), "utf8").includes(tacticalRuntimeEffectCap.surfacedAs),
  `${tacticalRuntimeEffectCap.surfacedAs} must still be surfaced, or the browser spec's derivation reads an attribute nothing emits`);
console.log(`JUSTIFIED tactical-budget-runtime-effect-cap: the declared stress-row effect ceiling IS ${tacticalRuntimeEffectCap.path}'s ${tacticalRuntimeEffectCap.binding}, re-read from the F# source, and ${tacticalRuntimeEffectCap.surfacedAs} is still emitted for the consumer that derives from the live DOM`);

// 3. NO CONSUMER MAY RESTATE A TACTICAL BUDGET.
// Two independent rules, because neither catches the other's case, and the LIMITS of each are stated
// rather than implied.
//
// Rule A -- POSITION. A budget field name may appear in a consumer only as a READ. Assigning a
// numeric literal to a declared budget key is a second declaration wherever it sits, and this is
// checked on the KEY, so it catches a restated value that happens to agree.
//
// Rule B -- VALUE. A declared budget value may not appear as a standalone numeric token anywhere in
// a consumer. This catches the shape Rule A cannot see: a keyless literal, which is exactly how the
// browser spec restated the node cap, as a bare two-tier ternary over the two published caps.
//
// Rule B CANNOT carry every declared value, and that limit is declared here rather than left for a
// reader to discover. The representative row's node cap and its input-to-paint ceiling each occur in
// these consumers for reasons that have nothing to do with a budget -- a readiness timeout in the
// review generator, a trace fixture duration in this suite, an unrelated overlay-layer
// node bound, a Playwright wait, a percentile computation, a percentage assertion string -- so
// sweeping them by value would red on correct code. They are covered by Rule A only, and a keyless
// restatement of either ALONE would therefore escape both rules. A restatement of the node cap or of
// the effect ceiling as a two-tier PAIR cannot escape, because the stress row's node cap and both
// effect ceilings are swept -- and a single-tier copy of either quantity is not a statement of the
// published budget, which has two tiers. That is the gate's exact bound; it is not claimed wider.
const tacticalConsumersByName = ["./generate-tactical-visual-review.mjs", "./test-tactical-visual-review.mjs", "../tests/SIR.Browser.Tests/visible-workflows.spec.js"];
for (const consumer of tacticalConsumersByName)
  assert.ok(budgetConsumers.includes(consumer), `${consumer} must be reached by the DERIVED consumer sweep`);

const declaredBudgetKeys = [...new Set(Object.values(tacticalReviewManifestBudgets).flatMap((row) => Object.keys(row)))];
assert.ok(declaredBudgetKeys.length >= 4, `the declared budget key set is ${declaredBudgetKeys.length}; it is not reaching the declaration and Rule A would pass vacuously`);
// The key S.I.R.#318 REMOVED. It named a millisecond nothing declared and was added to the ceiling at
// the call site; a consumer that reintroduces it under any value has reintroduced the defect.
// Spelled as a bare const rather than inside an array literal ON PURPOSE: readsKey below detects the
// `budget["key"]` form, and a bracketed string literal here would make this line trip its own rule.
const retiredBudgetKey = "measurementToleranceMilliseconds";
const retiredBudgetKeys = [retiredBudgetKey];

// Matched on CODE POSITION, never on the bare word. A gate that forbids a name outright also reds on
// the comment that explains why the name was retired, and the fix for that is never to start
// enumerating the shapes prose takes -- it is to match the structure of the thing being asserted,
// which here is a definition (`key:` / `key =`) or a member read (`.key`).
const definesKey = (text, key) => new RegExp(`${key}\\s*[:=](?!=)`).test(text);
const readsKey = (text, key) => new RegExp(`\\.\\s*${key}\\b|\\[["'\`]${key}["'\`]\\]`).test(text);
const restatesKey = (text) => [
  ...declaredBudgetKeys.filter((key) => new RegExp(`${key}\\s*[:=]\\s*-?\\d`).test(text)),
  ...retiredBudgetKeys.filter((key) => definesKey(text, key) || readsKey(text, key)),
];
// A budget WIDENED by arithmetic at a call site is a second, undeclared budget reached by addition
// instead of by a literal -- the exact shape of the removed tolerance. No consumer needs to add to a
// budget, so the operator is refused on a budget field outright.
// Scoped to the MAXIMUM-bearing manifest fields -- the ones a consumer compares a measurement
// against -- and deliberately NOT to the raw declared constants. Building a fixture relative to the
// declared ceiling (`ceiling + 3.33`, as the band fixture above does) is how a discriminating
// measurement gets constructed; widening the ceiling a consumer then GATES on is the defect. Those
// are different structures and only the second is refused.
const widensBudget = (text) => declaredBudgetKeys
  .filter((key) => new RegExp(`\\.${key}\\s*\\+(?!\\+)|\\+\\s*[A-Za-z_$][\\w$.]*\\.${key}\\b`).test(text));

const representativeWorkload = tacticalWorkloadBudgetList[0];
const stressWorkload = tacticalWorkloadBudgetList[tacticalWorkloadBudgetList.length - 1];
const sweptTacticalValues = [
  stressWorkload.maximumDomNodes,
  representativeWorkload.maximumEffects,
  stressWorkload.maximumEffects,
  stressWorkload.maximumInputToPaintMilliseconds,
  tacticalFrameCadenceBudget.intervalCeilingMilliseconds,
];
const unsweepableTacticalValues = new Map([
  [representativeWorkload.maximumDomNodes, "also a readiness timeout in the review generator, a trace-fixture duration in this suite, and an unrelated overlay-layer node bound in the browser spec"],
  [representativeWorkload.maximumInputToPaintMilliseconds, "also a Playwright wait, a percentile computation, a declared workload's unit count, and a \"100%\" assertion string"],
]);
// EVERY declared value is accounted for: swept by Rule B, or named unsweepable WITH a reason. A new
// budget figure added to the declaration and reachable by neither rule fails here rather than
// silently joining the unswept set.
const allDeclaredTacticalValues = [...new Set(tacticalWorkloadBudgetList.flatMap((workload) => [workload.maximumDomNodes, workload.maximumEffects, workload.maximumInputToPaintMilliseconds]).concat([tacticalFrameCadenceBudget.intervalCeilingMilliseconds]))];
for (const value of allDeclaredTacticalValues)
  assert.ok(sweptTacticalValues.includes(value) || unsweepableTacticalValues.has(value),
    `the declared budget value ${value} is neither swept by value nor recorded as unsweepable with a reason; add it to one or the other rather than letting it join the unswept set silently`);
for (const value of unsweepableTacticalValues.keys())
  assert.ok(allDeclaredTacticalValues.includes(value), `${value} is recorded as an unsweepable budget value but is no longer declared; the exemption is stale`);
assert.ok(sweptTacticalValues.length >= 4, `Rule B sweeps only ${sweptTacticalValues.length} value(s); it is not reaching the declaration`);

const restatesValue = (text) => sweptTacticalValues.filter((value) => new RegExp(`(?<![\\d.\\w])${String(value).replace(".", "\\.")}(?![\\d.\\w])`).test(text));
// The rules are self-tested against planted text before being trusted against the tree: a rule that
// cannot fire is not a rule that found nothing.
// The planted fixtures are BUILT from the declared names rather than typed out, so this file does not
// itself carry the code positions it forbids -- a self-test that trips its own gate is a gate nobody
// can keep.
const [nodeKey, effectKey, inputKey, frameKey] = declaredBudgetKeys;
const [retiredKey] = retiredBudgetKeys;
assert.deepEqual(restatesKey(`const budgets = { ${nodeKey}: ${stressWorkload.maximumDomNodes} };`), [nodeKey], "Rule A must catch a budget key assigned a literal");
assert.deepEqual(restatesKey(`const t = { ${retiredKey}: 1 };`), [retiredKey], "Rule A must catch the retired tolerance key being defined again");
assert.deepEqual(restatesKey(`ceiling + budget.${retiredKey}`), [retiredKey], "Rule A must catch the retired tolerance key being read again");
assert.deepEqual(restatesKey(`the ${retiredKey} key was removed by S.I.R.#318`), [], "Rule A must not fire on prose that merely names the retired key");
assert.deepEqual(restatesKey(`if (measured > budget.${nodeKey}) fail();`), [], "Rule A must not fire on a READ of a budget field");
assert.deepEqual(widensBudget(`budget.${frameKey} + budget.${retiredKey}`), [frameKey], "the arithmetic rule must catch a ceiling widened at the call site");
assert.deepEqual(widensBudget(`if (measured < budget.${inputKey}) return null;`), [], "the arithmetic rule must not fire on a plain comparison");
assert.ok([nodeKey, effectKey, inputKey, frameKey].every(Boolean), "the declared key set must supply every name these fixtures are built from");
for (const value of sweptTacticalValues)
  assert.deepEqual(restatesValue(`expect(nodeEstimate).toBeLessThanOrEqual(${value});`), [value], `Rule B must catch a planted ${value}`);
assert.deepEqual(restatesValue(`const id = "sha256"; const ts = 1${sweptTacticalValues[0]};`), [], "Rule B must not fire on a digit run that merely contains a swept value");

// THE ARITHMETIC RULE'S ONE EXEMPTION, AND ITS EXACT BOUND. This suite is where budgets are
// deliberately probed off their boundary: every measurement it builds is `budget + 1` or
// `ceiling + 3.33` BY CONSTRUCTION, and a text rule cannot tell a widened ceiling on the budget side
// of a comparison from a measurement built one unit past it on the measured side. So this file is
// exempt from THAT rule and from that rule only -- the key rule and the value rule still apply to it
// in full, and it gates no production route. What proves the enforcement itself carries no slack is
// not text at all but the semantic boundary assertions below, which red the moment a comparison
// admits anything past the declared number.
const arithmeticExemptConsumer = "./test-svg-pipeline-measurement.mjs";
assert.ok(budgetConsumers.includes(arithmeticExemptConsumer), "the arithmetic-rule exemption names a file the sweep does not reach; it is stale");
for (const consumer of budgetConsumers) {
  const text = readFileSync(new URL(consumer, import.meta.url), "utf8");
  assert.deepEqual(restatesKey(text), [], `${consumer} declares a tactical budget key instead of reading one. Import it from lib/performance-budget.mjs; a second declaration that agrees today is exactly the defect S.I.R.#318 removed.`);
  if (consumer !== arithmeticExemptConsumer)
    assert.deepEqual(widensBudget(text), [], `${consumer} widens a declared budget by arithmetic. A ceiling plus a slack is a SECOND budget, and it is the one CI would then enforce while naming the first.`);
  assert.deepEqual(restatesValue(text), [], `${consumer} restates a declared tactical budget value as a literal. Import it instead.`);
}
// and the two that cannot be exercised in-process must actually READ the declaration
for (const consumer of tacticalConsumersByName) {
  const text = readFileSync(new URL(consumer, import.meta.url), "utf8");
  assert.match(text, /performance-budget\.mjs/, `${consumer} must import the single declaration`);
}

// Rule C -- SUBJECT. Rules A and B are both keyed off something the DECLARATION says: a budget key,
// or a budget value. Neither sees a consumer that bounds a measured subject with a number the
// declaration never contained -- and that is not a hypothetical gap, it is the other half of this
// item's root cause: a threshold with no row in the table, invented at the point of use. It is also
// the mutant that escaped the first draft of this gate (`nodeEstimate <= 9500` survived Rules A and
// B outright), which is why this rule exists at all.
//
// So the third rule is keyed off the SUBJECT instead. Where a consumer reads a budgeted quantity off
// the live DOM, the identifier it binds that measurement to may not meet a numeric literal on any
// line: the only thing a measurement may be compared against is the declaration. It fails closed --
// a declared surface no consumer reads at all is a failure, not a skip, because a rule with nothing
// to check would pass vacuously forever.
let surfaceBoundedIdentifiers = 0;
for (const surface of tacticalBudgetSurfaces) {
  let readingConsumers = 0;
  for (const consumer of tacticalConsumersByName) {
    const text = readFileSync(new URL(consumer, import.meta.url), "utf8");
    const binding = text.match(new RegExp(`(?:const|let|var)\\s+([A-Za-z_$][\\w$]*)[^\\n]*getAttribute\\(\\s*["'\`]${surface.attribute}["'\`]`));
    if (!binding) continue;
    readingConsumers += 1;
    surfaceBoundedIdentifiers += 1;
    const [, identifier] = binding;
    for (const line of text.split("\n")) {
      if (!new RegExp(`\\b${identifier}\\b`).test(line)) continue;
      assert.doesNotMatch(line, /(?<![\w.$])\d/,
        `${consumer} bounds ${identifier} -- the measurement it read from ${surface.attribute} -- with a numeric literal: ${line.trim()}. A measurement may only be compared against the declaration; a number written here is either a restated budget or a threshold this repository declares nowhere, which is the defect S.I.R.#318 removed.`);
    }
  }
  assert.ok(readingConsumers > 0,
    `no tactical consumer reads ${surface.attribute}, so the subject rule for ${surface.quantity} checks nothing and would pass vacuously`);
}
assert.ok(surfaceBoundedIdentifiers >= tacticalBudgetSurfaces.length, "the subject rule bound fewer identifiers than there are declared surfaces");
console.log(`JUSTIFIED tactical-budget-no-restatement: ${budgetConsumers.length} DERIVED consumers declare none of the ${declaredBudgetKeys.length} budget keys, widen no budget by arithmetic, and carry none of the ${sweptTacticalValues.length} sweepable declared values; ${unsweepableTacticalValues.size} declared value(s) are covered by the key rule alone, each with a recorded reason; and ${surfaceBoundedIdentifiers} measurement(s) read from the ${tacticalBudgetSurfaces.length} declared DOM surface(s) meet no numeric literal at all, so an INVENTED bound is refused as readily as a copied one`);

// 4. THE THRESHOLDS CI ENFORCES CAN FAIL, AND THEY FAIL AT THE DECLARED NUMBER.
// scripts/test-tactical-visual-review.mjs needs a built client and a browser, so its comparisons
// live in the declaration and are inverted here instead. Boundaries are taken FROM the declaration:
// a literal here would be a fourth statement of the number.
const representative = representativeWorkload;
const stress = stressWorkload;
assert.equal(tacticalStructuralBudgetReason(representative, { domNodes: representative.maximumDomNodes, effects: representative.maximumEffects }), null,
  "the structural ceilings are inclusive: a scene exactly at them conforms");
assert.match(`${tacticalStructuralBudgetReason(representative, { domNodes: representative.maximumDomNodes + 1, effects: representative.maximumEffects })}`, /SVG node budget exceeded/);
assert.match(`${tacticalStructuralBudgetReason(representative, { domNodes: representative.maximumDomNodes, effects: representative.maximumEffects + 1 })}`, /active-effect budget exceeded/);
assert.equal(tacticalInputToPaintBudgetReason(stress, stress.maximumInputToPaintMilliseconds - 1), null);
assert.match(`${tacticalInputToPaintBudgetReason(stress, stress.maximumInputToPaintMilliseconds)}`, /input-to-paint budget exceeded/,
  "the input-to-paint ceiling is exclusive, as the published `< N ms` cell says");
assert.equal(tacticalFrameCadenceBudgetReason(stress, tacticalFrameBudget.callbackMillisecondsCeiling), null);
assert.match(`${tacticalFrameCadenceBudgetReason(stress, tacticalFrameCadenceBudget.intervalCeilingMilliseconds)}`, /frame cadence budget exceeded/,
  "an interval that has reached two frame periods has dropped a vsync and must be refused");
// a non-answer must never be reported as a confident answer
for (const unmeasured of [undefined, null, Number.NaN, "17"]) {
  assert.throws(() => tacticalInputToPaintBudgetReason(stress, unmeasured), /was not measured/,
    "an unmeasured input-to-paint value must be refused, not passed");
  assert.throws(() => tacticalFrameCadenceBudgetReason(stress, unmeasured), /was not measured/,
    "an unmeasured cadence value must be refused, not passed");
  assert.throws(() => tacticalStructuralBudgetReason(stress, { domNodes: unmeasured, effects: 1 }), /was not measured/);
}
assert.throws(() => tacticalWorkloadBudgetFor(stress.units + 1), /no declared tactical workload budget/,
  "a workload the table does not declare must be refused, not bucketed into whichever row is nearest");
assert.throws(() => tacticalWorkloadBudgetAtScale("many"), /unreadable scale is refused/);
console.log("JUSTIFIED tactical-budget-enforcement: every threshold scripts/test-tactical-visual-review.mjs applies reds one unit past its declared boundary, greens at it, and refuses an unmeasured or undeclared input rather than deciding about it");

// 5. THE REMOVED TOLERANCE WAS ADMITTING A REAL BREACH, MEASURED ON THE RETAINED ARTIFACT.
// This is the discriminating case, and it is taken from the production telemetry this repo actually
// ships rather than from a fixture invented to make the point. Prove the two sides CAN differ first:
// the retained measurement must be judged differently by the declared frame ceiling and by the
// superseded `ceiling + 1` composite, or this comparison measures nothing.
const supersededToleranceMilliseconds = 1;
const retainedTelemetry = JSON.parse(readFileSync(new URL("../docs/assets/tactical-visual-system-review/telemetry.json", import.meta.url), "utf8"));
const retainedIntervals = retainedTelemetry.densityScenes.map(({ animationFrameIntervalMilliseconds }) => animationFrameIntervalMilliseconds);
assert.ok(retainedIntervals.length > 0, "the retained telemetry declares no scenes; this comparison would be vacuous");
for (const measured of retainedIntervals) {
  assert.ok(measured > tacticalFrameBudget.callbackMillisecondsCeiling,
    `the retained production measurement ${measured} ms must BREACH the declared frame ceiling of ${tacticalFrameBudget.callbackMillisecondsCeiling} ms, or this gate is asserting something the artifact does not show`);
  assert.ok(measured <= tacticalFrameBudget.callbackMillisecondsCeiling + supersededToleranceMilliseconds,
    `and it must CONFORM to the superseded ceiling+${supersededToleranceMilliseconds} composite -- if it failed under both, this pair would discriminate nothing and the removed tolerance would not have been load-bearing`);
  assert.equal(tacticalFrameCadenceBudgetReason(stress, measured), null,
    "under the declared cadence budget the same measurement conforms to a ceiling it does not breach, which is the whole repair: the gate no longer reports green about a number the measurement exceeded");
}
assert.ok(tacticalFrameCadenceBudget.intervalCeilingMilliseconds > tacticalFrameBudget.callbackMillisecondsCeiling + supersededToleranceMilliseconds,
  "the declared cadence ceiling must sit above the superseded composite, or the two are indistinguishable on these measurements");
assert.match(`${tacticalFrameCadenceBudgetReason(stress, tacticalFrameBudget.callbackMillisecondsCeiling * (tacticalFrameCadenceBudget.maximumElapsedVsyncsPerFrame + 1))}`, /frame cadence budget exceeded/,
  "and a dropped vsync -- the failure this budget exists to catch -- must still red");
console.log(`JUSTIFIED tactical-cadence-discriminates: every retained production interval (${retainedIntervals.join(", ")} ms) breaches the declared ${tacticalFrameBudget.callbackMillisecondsCeiling} ms frame ceiling and conformed to the superseded ceiling+${supersededToleranceMilliseconds} composite, so the removed tolerance was the only reason CI reported green; the declared cadence budget of ${tacticalFrameCadenceBudget.intervalCeilingMilliseconds} ms judges the same measurement without claiming anything false, and still reds on a dropped vsync`);

// The JUnit report and the PASS line are written HERE, after every assertion above, and nowhere earlier.
// They used to be written mid-file: under a mutation the suite exited 1 having ALREADY published a green
// record, which is worse than publishing none -- a green artifact that outlives a red run. Node exits on
// the first failed assertion, so reaching this line is what makes the report true.
const report = process.env.SIR_SVG_PIPELINE_JUNIT || "artifacts/test-results/svg-pipeline.junit.xml";
mkdirSync(dirname(report), { recursive: true });
writeFileSync(report, '<?xml version="1.0" encoding="UTF-8"?>\n<testsuites tests="39" failures="0" errors="0" skipped="0"><testsuite name="svg-pipeline-measurement" tests="39" failures="0" errors="0" skipped="0"><testcase name="schema"/><testcase name="journey-inventory"/><testcase name="memory-cycle"/><testcase name="controlled-global-pair"/><testcase name="axis-value"/><testcase name="fixture-capacity"/><testcase name="axis-inventory"/><testcase name="trace-timing"/><testcase name="trace-window"/><testcase name="observed-run"/><testcase name="map-extent-control"/><testcase name="visible-density-control"/><testcase name="global-unit-control"/><testcase name="overlay-control"/><testcase name="event-rate-control"/><testcase name="supporting-list-control"/><testcase name="unique-unit-cells"/><testcase name="production-visible-observation"/><testcase name="production-global-observation"/><testcase name="evidence-candidate-binding"/><testcase name="evidence-digest-binding"/><testcase name="raw-trace-binding"/><testcase name="raw-trace-missing"/><testcase name="raw-trace-changed"/><testcase name="unreadable-input"/><testcase name="frame-budget-declaration"/><testcase name="frame-budget-boundary"/><testcase name="frame-budget-fail-closed"/><testcase name="frame-budget-closed-domain"/><testcase name="frame-budget-artifact-verdict"/><testcase name="frame-budget-workload-identity"/><testcase name="frame-budget-retained-matrix"/><testcase name="frame-budget-finalizer"/><testcase name="frame-budget-report-surfaces"/><testcase name="tactical-budget-document-binding"/><testcase name="tactical-budget-runtime-effect-cap"/><testcase name="tactical-budget-no-restatement"/><testcase name="tactical-budget-enforcement"/><testcase name="tactical-cadence-discriminates"/></testsuite></testsuites>\n');
console.log("svg-pipeline measurement unit gates: PASS");
