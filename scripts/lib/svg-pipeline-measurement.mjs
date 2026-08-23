import { createHash } from "node:crypto";

export const schema = "sir.svg-pipeline-measurement/1";
export const stageNames = [
  "workerCompute", "workerTransfer", "projectionAllocation", "elmishReact", "mainThreadScript",
  "style", "layout", "paint", "compositor",
];

export function canonical(value) {
  if (Array.isArray(value)) return value.map(canonical);
  if (value && typeof value === "object") return Object.fromEntries(Object.keys(value).sort().map((key) => [key, canonical(value[key])]));
  return value;
}

export function stableJson(value) { return `${JSON.stringify(canonical(value), null, 2)}\n`; }
export function digest(value) { return createHash("sha256").update(typeof value === "string" ? value : stableJson(value)).digest("hex"); }

export function byteDigest(value) { return createHash("sha256").update(value).digest("hex"); }

export function validateDefinitions(definition) {
  if (definition?.schema !== "sir.svg-pipeline-fixtures/1") throw new Error("unsupported fixture schema");
  if (!Array.isArray(definition.journeys) || definition.journeys.length !== 7) throw new Error("journey inventory must contain seven production journeys");
  const requiredJourneys = ["idle", "playback", "pan", "zoom", "selection", "modality-transition", "dense-overlay"];
  if (requiredJourneys.some((name) => !definition.journeys.includes(name))) throw new Error("journey inventory is incomplete");
  if (!(definition.materialShareThreshold > 0 && definition.materialShareThreshold < 1)) throw new Error("material share threshold must be between zero and one");
  if (!(definition.warmupCycles > 0 && definition.stabilizationCycles > definition.warmupCycles)) throw new Error("memory cycle contract is invalid");
  const budget = definition.frameBudget;
  if (!budget || typeof budget !== "object") throw new Error("fixture definition declares no frameBudget; a verdict cannot be derived without a declared budget");
  if (!(budget.callbackMillisecondsCeiling > 0)) throw new Error("frameBudget.callbackMillisecondsCeiling must be a positive number of milliseconds");
  if (!Array.isArray(budget.evaluatedPercentiles) || budget.evaluatedPercentiles.length === 0
      || budget.evaluatedPercentiles.some((value) => !(value > 0 && value <= 1)))
    throw new Error("frameBudget.evaluatedPercentiles must be a non-empty list of proportions in (0, 1]");
  if (!Array.isArray(budget.exemptJourneys) || budget.exemptJourneys.some((name) => !definition.journeys.includes(name)))
    throw new Error("frameBudget.exemptJourneys must list declared journeys");
  if (typeof budget.source !== "string" || budget.source.length === 0)
    throw new Error("frameBudget.source must cite where the budget is declared");
  const ids = new Set();
  for (const fixture of definition.fixtures || []) {
    if (!fixture.id || ids.has(fixture.id)) throw new Error("fixture ids must be unique and non-empty");
    ids.add(fixture.id);
    for (const axis of ["visibleDensity", "globalUnitCount", "routeOverlayComplexity", "eventRateHz", "supportingListSize"])
      if (!Number.isInteger(fixture[axis]) || fixture[axis] < 0) throw new Error(`fixture ${fixture.id} has invalid ${axis}`);
    if (fixture.visibleDensity > fixture.globalUnitCount) throw new Error(`fixture ${fixture.id} visibleDensity exceeds globalUnitCount`);
    if (!Array.isArray(fixture.mapExtent) || fixture.mapExtent.length !== 2 || fixture.mapExtent.some((n) => !Number.isInteger(n) || n <= 0)) throw new Error(`fixture ${fixture.id} has invalid mapExtent`);
    if (!Array.isArray(fixture.viewport) || fixture.viewport.length !== 2 || fixture.viewport.some((n) => !Number.isInteger(n) || n <= 0)) throw new Error(`fixture ${fixture.id} has invalid viewport`);
    if (fixture.globalUnitCount > fixture.mapExtent[0] * fixture.mapExtent[1]) throw new Error(`fixture ${fixture.id} exceeds unique map-cell capacity`);
  }
  const axes = ["mapExtent", "visibleDensity", "globalUnitCount", "routeOverlayComplexity", "eventRateHz", "supportingListSize"];
  if (stableJson(Object.keys(definition.controlledAxes || {}).sort()) !== stableJson([...axes].sort())) throw new Error("controlled-axis inventory must cover all six workload axes");
  const byId = new Map(definition.fixtures.map((fixture) => [fixture.id, fixture]));
  for (const axis of axes) {
    const pairIds = definition.controlledAxes[axis];
    if (!Array.isArray(pairIds) || pairIds.length !== 2 || pairIds[0] === pairIds[1]) throw new Error(`${axis} comparison must name two distinct fixtures`);
    const pair = pairIds.map((id) => byId.get(id));
    if (pair.some((fixture) => !fixture)) throw new Error(`${axis} comparison names an unknown fixture`);
    const controlled = (fixture) => Object.fromEntries(Object.entries(fixture).filter(([key]) => !["id", axis].includes(key)));
    if (stableJson(pair[0][axis]) === stableJson(pair[1][axis]) || stableJson(controlled(pair[0])) !== stableJson(controlled(pair[1]))) throw new Error(`${axis} comparison is not one-factor controlled`);
  }
  const pair = (definition.globalScalePair || []).map((id) => byId.get(id));
  const area = (fixture) => fixture.mapExtent[0] * fixture.mapExtent[1];
  if (pair.length !== 2 || pair.some((fixture) => !fixture) || stableJson(pair[0].viewport) !== stableJson(pair[1].viewport) || pair[0].visibleDensity !== pair[1].visibleDensity || pair[0].globalUnitCount !== pair[1].globalUnitCount || pair[0].eventRateHz !== pair[1].eventRateHz || pair[0].routeOverlayComplexity !== pair[1].routeOverlayComplexity || area(pair[0]) >= area(pair[1]) || pair[0].supportingListSize >= pair[1].supportingListSize) throw new Error("large-project/small-viewport comparison is not controlled");
  return definition;
}

export function makeMap(fixture) {
  const [width, height] = fixture.mapExtent;
  const lines = ["SIR-MAP 2", `size ${width} ${height}`];
  for (let index = 0; index < Math.min(width * height, fixture.routeOverlayComplexity); index += 1) lines.push(`terrain ${index % width} ${Math.floor(index / width) % height} ${index % 5 === 0 ? "objective" : "rough"}`);
  const visibleWidth = Math.min(width, Math.max(1, Math.ceil(Math.sqrt(fixture.visibleDensity))));
  const visibleHeight = Math.max(1, Math.ceil(fixture.visibleDensity / visibleWidth));
  const visibleOrigin = [Math.floor((width - visibleWidth) / 2), Math.floor((height - visibleHeight) / 2)];
  const occupied = new Set();
  const positions = [];
  for (let index = 0; index < fixture.globalUnitCount; index += 1) {
    const visible = index < fixture.visibleDensity;
    if (visible) {
      const column = visibleOrigin[0] + index % visibleWidth;
      const row = visibleOrigin[1] + Math.floor(index / visibleWidth);
      positions.push([column, row]);
      occupied.add(`${column},${row}`);
    } else {
      let cell = width * height - 1;
      while (occupied.has(`${cell % width},${Math.floor(cell / width)}`)) cell -= 1;
      const column = cell % width;
      const row = Math.floor(cell / width);
      positions.push([column, row]);
      occupied.add(`${column},${row}`);
    }
  }
  for (let index = 0; index < fixture.globalUnitCount; index += 1) {
    const [column, row] = positions[index];
    const scriptSize = Math.floor(fixture.supportingListSize / fixture.globalUnitCount) + (index < fixture.supportingListSize % fixture.globalUnitCount ? 1 : 0);
    const script = scriptSize > 0 ? Array(scriptSize).fill("E").join(",") : "-";
    lines.push(`unit ${index + 1} ${index % 2 ? "red" : "blue"} rifleman ${column} ${row} 1 12 12 scripted ${script}`);
  }
  return `${lines.join("\n")}\n`;
}

export function workloadRecipe(fixture) {
  const playbackWindowMilliseconds = 250;
  const eventIntervalMilliseconds = 1000 / fixture.eventRateHz;
  return { targetVisibleUnits: fixture.visibleDensity, eventRateHz: fixture.eventRateHz, eventIntervalMilliseconds, playbackWindowMilliseconds, playbackSteps: Math.floor(playbackWindowMilliseconds / eventIntervalMilliseconds) + 1, supportingRecords: fixture.supportingListSize };
}

export function validateEvidenceReceipt(receipt, definitions, authority) {
  const hex = /^[0-9a-f]{64}$/;
  if (receipt?.schema !== "sir.svg-pipeline-measurement-evidence/1") throw new Error("unsupported evidence schema");
  const { bindingSha256, ...boundReceipt } = receipt;
  if (!hex.test(bindingSha256 || "") || bindingSha256 !== digest(boundReceipt)) throw new Error("evidence receipt binding is stale");
  if (authority?.schema !== "sir.svg-pipeline-measurement-authority/1") throw new Error("evidence authority is missing");
  if (stableJson(receipt.candidate) !== stableJson(authority.candidate)
      || stableJson(receipt.buildIdentity) !== stableJson(authority.buildIdentity)
      || receipt.fixtureDefinition?.sha256 !== authority.fixtureDefinitionSha256
      || receipt.matrix?.rawSummarySha256 !== authority.rawSummarySha256
      || receipt.matrix?.rawTraceManifestSha256 !== authority.rawTraceManifestSha256
      || receipt.matrix?.orderedTraceDigestSha256 !== authority.orderedTraceDigestSha256
      || receipt.matrix?.runCount !== authority.runCount) throw new Error("evidence authority binding is stale");
  for (const value of [receipt.candidate?.commit, receipt.candidate?.tree]) if (!/^[0-9a-f]{40}$/.test(value || "") || /^0+$/.test(value)) throw new Error("evidence candidate binding is missing");
  for (const value of [receipt.buildIdentity?.clientManifestSha256, receipt.buildIdentity?.serverAssemblySha256, receipt.matrix?.rawSummarySha256, receipt.matrix?.rawTraceManifestSha256, receipt.matrix?.orderedTraceDigestSha256]) if (!hex.test(value || "") || /^0+$/.test(value)) throw new Error("evidence digest binding is missing");
  if (receipt.fixtureDefinition?.sha256 !== fixtureIdentityDigest(definitions)) throw new Error("evidence fixture binding is stale");
  if (receipt.matrix?.result !== "pass"
      || receipt.matrix?.fixtureCount !== definitions.fixtures.length
      || receipt.matrix?.journeyCount !== definitions.journeys.length
      || stableJson(receipt.matrix?.fixtureIds) !== stableJson(definitions.fixtures.map((fixture) => fixture.id))
      || stableJson(receipt.matrix?.journeys) !== stableJson(definitions.journeys)
      || receipt.matrix?.runCount !== definitions.fixtures.length * definitions.journeys.length) throw new Error("evidence matrix is incomplete");
  if (!Number.isFinite(Date.parse(receipt.matrix.startedAt)) || !Number.isFinite(Date.parse(receipt.matrix.completedAt)) || Date.parse(receipt.matrix.completedAt) < Date.parse(receipt.matrix.startedAt)) throw new Error("evidence timestamps are invalid");
  return receipt;
}

export function validateRetainedRawEvidence(receipt, authority, manifest, readRawTrace) {
  const expectedPath = (sha256) => `work/231-svg-pipeline-measurement/raw-traces/${sha256}.trace.json.gz`;
  if (manifest?.schema !== "sir.svg-pipeline-raw-trace-manifest/1") throw new Error("raw trace manifest schema is unsupported");
  if (digest(manifest) !== receipt.matrix.rawTraceManifestSha256 || digest(manifest) !== authority.rawTraceManifestSha256) throw new Error("raw trace manifest binding is stale");
  if (stableJson(manifest.candidate) !== stableJson(receipt.candidate) || manifest.fixtureDefinitionSha256 !== receipt.fixtureDefinition.sha256) throw new Error("raw trace manifest candidate or fixture binding is stale");
  if (!Array.isArray(manifest.runs) || manifest.runs.length !== receipt.matrix.runCount || manifest.runs.length !== authority.runCount) throw new Error("raw trace manifest run inventory is incomplete");
  const identities = new Set();
  for (const run of manifest.runs) {
    const identity = `${run.fixture}\u0000${run.journey}`;
    if (identities.has(identity)) throw new Error("raw trace manifest contains a duplicate run");
    identities.add(identity);
    if (!/^[0-9a-f]{64}$/.test(run.sha256 || "") || run.path !== expectedPath(run.sha256)) throw new Error("raw trace content address is invalid");
    let raw;
    try { raw = readRawTrace(run.path); } catch { throw new Error(`raw trace is unreadable: ${run.path}`); }
    if (byteDigest(raw) !== run.sha256) throw new Error(`raw trace digest is stale: ${run.path}`);
    try {
      const parsed = JSON.parse(Buffer.isBuffer(raw) ? raw.toString("utf8") : String(raw));
      if (!Array.isArray(parsed.traceEvents)) throw new Error("missing traceEvents");
    } catch { throw new Error(`raw trace is malformed: ${run.path}`); }
  }
  const expectedIdentities = receipt.matrix.fixtureIds.flatMap((fixture) => receipt.matrix.journeys.map((journey) => `${fixture}\u0000${journey}`));
  if (stableJson([...identities].sort()) !== stableJson(expectedIdentities.sort())) throw new Error("raw trace manifest fixture/journey coverage is incomplete");
  if (digest(manifest.runs.map(({ fixture, journey, sha256 }) => ({ fixture, journey, sha256 }))) !== receipt.matrix.orderedTraceDigestSha256) throw new Error("ordered raw trace digest is stale");
  return manifest;
}

export function validateObservedControls(summary, definitions) {
  const fixtures = new Map(definitions.fixtures.map((fixture) => [fixture.id, fixture]));
  const byIdentity = new Map((summary.runs || []).map((run) => [`${run.fixture}\u0000${run.journey}`, run]));
  const runsForPair = (axis) => {
    const [baselineId, variantId] = definitions.controlledAxes[axis];
    return definitions.journeys.map((journey) => {
      const baseline = byIdentity.get(`${baselineId}\u0000${journey}`);
      const variant = byIdentity.get(`${variantId}\u0000${journey}`);
      if (!baseline || !variant) throw new Error(`production ${axis} observation is incomplete`);
      return { baseline, variant };
    });
  };
  for (const { baseline, variant } of runsForPair("visibleDensity")) {
    if (baseline.structural?.visible?.visualUnits === variant.structural?.visible?.visualUnits
        || stableJson(baseline.structural?.cameraControl) !== stableJson(variant.structural?.cameraControl)) throw new Error("production visible-density observation is uncontrolled");
  }
  for (const { baseline, variant } of runsForPair("globalUnitCount")) {
    if (baseline.structural?.visible?.visualUnits !== variant.structural?.visible?.visualUnits
        || baseline.structural?.visible?.projectedUnits === variant.structural?.visible?.projectedUnits
        || stableJson(baseline.structural?.cameraControl) !== stableJson(variant.structural?.cameraControl)) throw new Error("production global-unit observation changes visible density or fails to change projected count");
  }
  for (const run of summary.runs || []) {
    const fixture = fixtures.get(run.fixture);
    if (!fixture) throw new Error(`production structural observation names unknown fixture ${run.fixture}`);
    if (run.structural?.visible?.visualUnits !== fixture.visibleDensity) throw new Error(`production viewport observation is stale for ${run.fixture}`);
    if (run.structural?.visible?.projectedUnits !== fixture.globalUnitCount) throw new Error(`production projected-unit observation is stale for ${run.fixture}`);
  }
  return summary;
}

export function validateProductionSummary(receipt, authority, manifest, summary, definitions) {
  if (summary?.schema !== schema || summary.result !== "pass") throw new Error("production summary schema or result is invalid");
  if (digest(summary) !== receipt.matrix.rawSummarySha256 || digest(summary) !== authority.rawSummarySha256) throw new Error("production summary binding is stale");
  if (stableJson(summary.candidate) !== stableJson(receipt.candidate)
      || stableJson(summary.buildIdentity) !== stableJson(receipt.buildIdentity)
      || summary.fixtureDefinition?.sha256 !== receipt.fixtureDefinition.sha256
      || summary.rawTraceManifest?.sha256 !== digest(manifest)) throw new Error("production summary candidate, build, fixture, or trace-manifest binding is stale");
  if (!Array.isArray(summary.runs) || summary.runs.length !== receipt.matrix.runCount) throw new Error("production summary run inventory is incomplete");
  const summarized = summary.runs.map((run) => ({ fixture: run.fixture, journey: run.journey, sha256: run.trace?.sha256 }));
  const retained = manifest.runs.map(({ fixture, journey, sha256 }) => ({ fixture, journey, sha256 }));
  if (stableJson(summarized) !== stableJson(retained)) throw new Error("production summary raw-trace inventory is stale");
  validateObservedControls(summary, definitions);
  return summary;
}

const sumDuration = (events, names, predicate = () => true) => events.reduce((total, event) => total + (names.has(event.name) && predicate(event) ? Number(event.dur || 0) / 1000 : 0), 0);

export function extractStages(trace) {
  const events = Array.isArray(trace?.traceEvents) ? trace.traceEvents : [];
  const threads = new Map(events.filter((event) => event.name === "thread_name").map((event) => [event.tid, event.args?.name || ""]));
  const worker = (event) => /worker/i.test(threads.get(event.tid) || "");
  const available = (value, source) => ({ available: true, milliseconds: Number(value.toFixed(3)), source });
  const unavailable = (reason) => ({ available: false, reason });
  const values = {
    workerCompute: available(sumDuration(events, new Set(["FunctionCall", "EvaluateScript"]), worker), "Chromium worker-thread script trace events"),
    workerTransfer: unavailable("Chromium exposes transfer inside task slices but the production client has no dedicated transfer User Timing mark yet"),
    projectionAllocation: unavailable("The production client has no projection/allocation User Timing mark; GC remains separately visible in the raw trace"),
    elmishReact: unavailable("The production client has no dedicated Elmish/React User Timing marks; generic main-thread script is reported separately"),
    mainThreadScript: available(sumDuration(events, new Set(["FunctionCall", "EvaluateScript"]), (event) => !worker(event)), "Chromium main-thread script trace events; not source-symbol isolated"),
    style: available(sumDuration(events, new Set(["UpdateLayoutTree", "RecalculateStyles"])), "Blink trace events"),
    layout: available(sumDuration(events, new Set(["Layout"])), "Blink trace events"),
    paint: available(sumDuration(events, new Set(["Paint", "PaintImage"])), "Blink trace events"),
    compositor: available(sumDuration(events, new Set(["CompositeLayers", "DrawFrame"])), "Chromium compositor trace events"),
  };
  for (const name of stageNames) if (!values[name]) values[name] = unavailable("not observed");
  return values;
}

const traceTimestamp = (event) => Number(event.ts || 0);

export function extractJourneyTrace(trace, startSyncId = "sir-journey-start", endSyncId = "sir-journey-end") {
  const events = Array.isArray(trace?.traceEvents) ? trace.traceEvents : [];
  const marker = (syncId) => events.find((event) => event.args?.sync_id === syncId && Number.isFinite(traceTimestamp(event)) && traceTimestamp(event) > 0);
  const start = marker(startSyncId);
  const end = marker(endSyncId);
  if (!start || !end || traceTimestamp(end) <= traceTimestamp(start)) throw new Error("journey trace clock-sync window is missing or invalid");
  return { ...trace, traceEvents: events.filter((event) => event.name === "thread_name" || (traceTimestamp(event) >= traceTimestamp(start) && traceTimestamp(event) <= traceTimestamp(end))) };
}

export function extractFrameHealth(trace) {
  const events = Array.isArray(trace?.traceEvents) ? trace.traceEvents : [];
  const frames = events.filter((event) => event.name === "AnimationFrame" && event.ph === "b" && Number.isFinite(traceTimestamp(event)) && traceTimestamp(event) > 0).sort((a, b) => traceTimestamp(a) - traceTimestamp(b));
  const intervals = frames.slice(1).map((event, index) => (traceTimestamp(event) - traceTimestamp(frames[index])) / 1000);
  const durations = frames.map((event) => Number(event.args?.animation_frame_timing_info?.duration_ms)).filter(Number.isFinite);
  const longTasks = events.filter((event) => event.name === "RunTask" && Number(event.dur || 0) >= 50_000).map((event) => Number((event.dur / 1000).toFixed(3)));
  return { samples: frames.length, droppedFrames: durations.filter((value) => value > 25).length, frameDurationsMilliseconds: durations.map((value) => Number(value.toFixed(3))), intervalsMilliseconds: intervals.map((value) => Number(value.toFixed(3))), longTasks, source: "Chromium AnimationFrame timing records and renderer RunTask slices; no injected frame observer", droppedFrameNote: "droppedFrames counts frames longer than an inline 25 ms threshold that NO budget document declares. It is a reported diagnostic and is deliberately not gated on; the declared frame budget is evaluated separately in each run's frameBudget block." };
}

// Nearest-rank percentile. This is the convention finalize-svg-pipeline-evidence.mjs already used;
// it is shared here so the producer and the consumer cannot drift into two different definitions.
// The fixture binding threaded through artifact -> raw-trace manifest -> authority -> evidence receipt
// identifies the WORKLOAD that produced a measurement: the maps, densities, journeys and cycle counts.
// frameBudget is deliberately excluded from it, because a budget is the policy applied to a result, not
// part of the workload's identity. Keeping them separate is what makes it coherent to re-evaluate an
// existing measurement against a corrected budget -- if a budget edit changed the workload digest, every
// historical artifact would instead be reported as coming from different fixtures, which is false.
// The budget actually in force is recorded in the artifact's own frameBudget.declared block.
export function fixtureIdentityDigest(definitions) {
  const { frameBudget, ...workload } = definitions || {};
  return digest(workload);
}

export function percentile(values, proportion) {
  if (!Array.isArray(values) || values.length === 0) return null;
  const sorted = [...values].sort((left, right) => left - right);
  return sorted[Math.min(sorted.length - 1, Math.ceil(sorted.length * proportion) - 1)];
}

// Derive ONE run's verdict from its measured frame durations against the DECLARED budget.
//
// Three outcomes, deliberately. "unevaluated" exists because the idle journey emits no AnimationFrame
// records at all, and a gate that answers "pass" about a quantity it never measured is a non-answer
// reported as a confident answer. A journey that is NOT declared exempt and still produced no frames is
// a FAILURE, so a broken measurement can never be laundered into an exemption.
export function evaluateRunFrameVerdict(frameHealth, journey, budget) {
  if (!budget) throw new Error("a declared frameBudget is required to derive a run verdict");
  const ceiling = budget.callbackMillisecondsCeiling;
  const raw = Array.isArray(frameHealth?.frameDurationsMilliseconds) ? frameHealth.frameDurationsMilliseconds : [];
  const durations = raw.filter((value) => Number.isFinite(value));
  const exempt = (budget.exemptJourneys || []).includes(journey);
  // A duration this cannot read is not a duration under the ceiling. `null > ceiling` is false, so an
  // unreadable value would otherwise be counted as conforming -- the precise shape of reporting a
  // non-answer as a confident pass.
  if (durations.length !== raw.length)
    return { ceilingMilliseconds: ceiling, sampleCount: raw.length, percentiles: {}, maximumMilliseconds: null, budgetSource: budget.source, result: "fail", reason: `${raw.length - durations.length} of ${raw.length} frame durations are not finite numbers and cannot be evaluated against the declared ceiling` };
  const percentiles = Object.fromEntries((budget.evaluatedPercentiles || []).map((proportion) => [`p${Math.round(proportion * 100)}`, percentile(durations, proportion)]));
  const base = { ceilingMilliseconds: ceiling, sampleCount: durations.length, percentiles, maximumMilliseconds: durations.length ? Math.max(...durations) : null, budgetSource: budget.source };
  if (exempt && durations.length === 0)
    return { ...base, result: "unevaluated", reason: `the ${journey} journey is declared frame-exempt and produced no AnimationFrame records; no frame verdict is claimed` };
  if (durations.length === 0)
    return { ...base, result: "fail", reason: `the ${journey} journey is not declared frame-exempt but produced no AnimationFrame records, so its budget could not be evaluated` };
  const breaches = Object.entries(percentiles).filter(([, value]) => value !== null && value > ceiling).map(([name, value]) => `${name}=${value} ms`);
  if (breaches.length)
    return { ...base, result: "fail", reason: `measured ${breaches.join(", ")} against the declared ceiling of ${ceiling} ms` };
  return { ...base, result: "pass", reason: `every evaluated percentile is within the declared ceiling of ${ceiling} ms` };
}

// Derive the ARTIFACT verdict from the per-run verdicts. These are different questions: a run asks
// whether one journey met the budget, the artifact asks whether the matrix as a whole did. The artifact
// passes only when nothing failed AND at least one run was genuinely evaluated -- a matrix of nothing but
// exemptions is not a pass.
export function evaluateArtifactVerdict(runs) {
  const verdicts = (runs || []).map((run) => run.frameBudget?.result);
  const failed = verdicts.filter((value) => value === "fail").length;
  const passed = verdicts.filter((value) => value === "pass").length;
  const unevaluated = verdicts.filter((value) => value === "unevaluated").length;
  if (verdicts.some((value) => value === undefined)) throw new Error("every run must carry a derived frameBudget verdict before the artifact verdict is computed");
  if (failed > 0) return { result: "fail", passedRunCount: passed, failedRunCount: failed, unevaluatedRunCount: unevaluated, reason: `${failed} of ${verdicts.length} runs breached the declared frame budget` };
  if (passed === 0) return { result: "fail", passedRunCount: passed, failedRunCount: failed, unevaluatedRunCount: unevaluated, reason: "no run was evaluated against the declared frame budget, so no pass is claimed" };
  return { result: "pass", passedRunCount: passed, failedRunCount: failed, unevaluatedRunCount: unevaluated, reason: `${passed} runs met the declared frame budget and none breached it` };
}

export function extractInputToPaint(trace, journey) {
  if (journey === "idle") return { available: false, reason: "The idle journey has no input event; its observation window is not interaction latency" };
  const events = Array.isArray(trace?.traceEvents) ? trace.traceEvents : [];
  const userInputTypes = new Set(["pointerdown", "mousedown", "wheel", "keydown", "click"]);
  const input = events.filter((event) => event.name === "EventDispatch" && userInputTypes.has(event.args?.data?.type) && Number.isFinite(traceTimestamp(event)) && traceTimestamp(event) > 0).sort((a, b) => traceTimestamp(a) - traceTimestamp(b))[0];
  if (!input) return { available: false, reason: "Chromium did not expose an EventDispatch slice for the journey" };
  const inputCompleted = traceTimestamp(input) + Number(input.dur || 0);
  const paint = events.filter((event) => (event.name === "Paint" || (event.name === "AnimationFrame::Presentation" && event.ph === "n")) && traceTimestamp(event) >= inputCompleted).sort((a, b) => traceTimestamp(a) - traceTimestamp(b))[0];
  if (!paint) return { available: false, reason: "Chromium exposed user input but no subsequent Paint or AnimationFrame presentation inside the journey trace" };
  return { available: true, milliseconds: Number(((traceTimestamp(paint) - traceTimestamp(input)) / 1000).toFixed(3)), inputType: input.args?.data?.type || "unknown", paintEvent: paint.name, source: "Chromium user EventDispatch start to the first subsequent Paint or AnimationFrame presentation trace event" };
}

export function summarize(runs, threshold) {
  if (!Array.isArray(runs) || runs.length === 0) throw new Error("at least one observed run is required");
  const totals = Object.fromEntries(stageNames.map((name) => [name, runs.reduce((sum, run) => sum + (run.stages[name]?.available ? run.stages[name].milliseconds : 0), 0)]));
  const measuredTotal = Object.values(totals).reduce((sum, value) => sum + value, 0);
  const ranking = Object.entries(totals).filter(([, value]) => value > 0).sort((a, b) => b[1] - a[1] || a[0].localeCompare(b[0])).map(([stage, milliseconds]) => ({ stage, milliseconds: Number(milliseconds.toFixed(3)), share: measuredTotal ? Number((milliseconds / measuredTotal).toFixed(4)) : 0 }));
  const top = ranking[0] || null;
  const share = (name) => ranking.find((row) => row.stage === name)?.share || 0;
  const observed = (name) => runs.some((run) => run.stages[name]?.available);
  return {
    schema,
    interpretation: "Versioned regression workloads; not a permanent supported-size ceiling.",
    nextBottleneck: top ? { stage: top.stage, evidence: `largest measured aggregate at ${top.milliseconds} ms (${(top.share * 100).toFixed(1)}%)` } : { stage: "inconclusive", evidence: "no available duration observations" },
    ranking,
    dispositions: {
      packedTransport: observed("workerTransfer") ? (share("workerTransfer") >= threshold ? "required" : "deferred") : "unresolved",
      typedBuffers: observed("workerTransfer") && observed("projectionAllocation") ? (Math.max(share("workerTransfer"), share("projectionAllocation")) >= threshold ? "required" : "deferred") : "unresolved",
      furtherAllocationWork: observed("projectionAllocation") ? (share("projectionAllocation") >= threshold ? "required" : "deferred") : "unresolved",
    },
  };
}
