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
  const ids = new Set();
  for (const fixture of definition.fixtures || []) {
    if (!fixture.id || ids.has(fixture.id)) throw new Error("fixture ids must be unique and non-empty");
    ids.add(fixture.id);
    for (const axis of ["visibleDensity", "globalUnitCount", "routeOverlayComplexity", "eventRateHz", "supportingListSize"])
      if (!Number.isInteger(fixture[axis]) || fixture[axis] < 0) throw new Error(`fixture ${fixture.id} has invalid ${axis}`);
    if (!Array.isArray(fixture.mapExtent) || fixture.mapExtent.length !== 2 || fixture.mapExtent.some((n) => !Number.isInteger(n) || n <= 0)) throw new Error(`fixture ${fixture.id} has invalid mapExtent`);
    if (!Array.isArray(fixture.viewport) || fixture.viewport.length !== 2 || fixture.viewport.some((n) => !Number.isInteger(n) || n <= 0)) throw new Error(`fixture ${fixture.id} has invalid viewport`);
  }
  if (new Set(definition.fixtures.map((fixture) => fixture.eventRateHz)).size < 2) throw new Error("fixture matrix must vary eventRateHz");
  const pair = definition.fixtures.filter((fixture) => fixture.comparisonGroup === "global-scale-small-viewport");
  const area = (fixture) => fixture.mapExtent[0] * fixture.mapExtent[1];
  if (pair.length !== 2 || stableJson(pair[0].viewport) !== stableJson(pair[1].viewport) || pair[0].visibleDensity !== pair[1].visibleDensity || pair[0].globalUnitCount !== pair[1].globalUnitCount || area(pair[0]) >= area(pair[1]) || pair[0].supportingListSize >= pair[1].supportingListSize) throw new Error("large-project/small-viewport comparison is not controlled");
  return definition;
}

export function makeMap(fixture) {
  const [width, height] = fixture.mapExtent;
  const lines = ["SIR-MAP 2", `size ${width} ${height}`];
  for (let index = 0; index < Math.min(width, fixture.routeOverlayComplexity); index += 1) lines.push(`terrain ${index} ${index % height} ${index % 5 === 0 ? "objective" : "rough"}`);
  const visibleWidth = Math.min(width, Math.max(1, Math.ceil(Math.sqrt(fixture.visibleDensity))));
  const visibleHeight = Math.max(1, Math.ceil(fixture.visibleDensity / visibleWidth));
  const visibleOrigin = [Math.floor((width - visibleWidth) / 2), Math.floor((height - visibleHeight) / 2)];
  for (let index = 0; index < fixture.globalUnitCount; index += 1) {
    const visible = index < fixture.visibleDensity;
    const relative = visible ? index : index - fixture.visibleDensity;
    const column = visible ? visibleOrigin[0] + relative % visibleWidth : relative % width;
    const row = visible ? visibleOrigin[1] + Math.floor(relative / visibleWidth) : Math.floor(relative / width) % height;
    const scriptSize = Math.floor(fixture.supportingListSize / fixture.globalUnitCount) + (index < fixture.supportingListSize % fixture.globalUnitCount ? 1 : 0);
    const script = scriptSize > 0 ? Array(scriptSize).fill("E").join(",") : "-";
    lines.push(`unit ${index + 1} ${index % 2 ? "red" : "blue"} rifleman ${column} ${row} 1 12 12 scripted ${script}`);
  }
  return `${lines.join("\n")}\n`;
}

export function workloadRecipe(fixture) {
  return { targetVisibleUnits: fixture.visibleDensity, playbackSteps: Math.max(1, Math.ceil(fixture.eventRateHz / 10)), supportingRecords: fixture.supportingListSize };
}

export function validateEvidenceReceipt(receipt, definitions) {
  const hex = /^[0-9a-f]{64}$/;
  if (receipt?.schema !== "sir.svg-pipeline-measurement-evidence/1") throw new Error("unsupported evidence schema");
  const { bindingSha256, ...boundReceipt } = receipt;
  if (!hex.test(bindingSha256 || "") || bindingSha256 !== digest(boundReceipt)) throw new Error("evidence receipt binding is stale");
  for (const value of [receipt.candidate?.commit, receipt.candidate?.tree]) if (!/^[0-9a-f]{40}$/.test(value || "") || /^0+$/.test(value)) throw new Error("evidence candidate binding is missing");
  for (const value of [receipt.buildIdentity?.clientManifestSha256, receipt.buildIdentity?.serverAssemblySha256, receipt.matrix?.rawSummarySha256, receipt.matrix?.orderedTraceDigestSha256]) if (!hex.test(value || "") || /^0+$/.test(value)) throw new Error("evidence digest binding is missing");
  if (receipt.fixtureDefinition?.sha256 !== digest(definitions)) throw new Error("evidence fixture binding is stale");
  if (receipt.matrix?.result !== "pass" || receipt.matrix?.runCount !== definitions.fixtures.length * definitions.journeys.length) throw new Error("evidence matrix is incomplete");
  if (!Number.isFinite(Date.parse(receipt.matrix.startedAt)) || !Number.isFinite(Date.parse(receipt.matrix.completedAt)) || Date.parse(receipt.matrix.completedAt) < Date.parse(receipt.matrix.startedAt)) throw new Error("evidence timestamps are invalid");
  return receipt;
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
