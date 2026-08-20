import { createHash } from "node:crypto";

export const schema = "sir.svg-pipeline-measurement/1";
export const stageNames = [
  "workerCompute", "workerTransfer", "projectionAllocation", "elmishReact",
  "style", "layout", "paint", "compositor",
];

export function canonical(value) {
  if (Array.isArray(value)) return value.map(canonical);
  if (value && typeof value === "object") return Object.fromEntries(Object.keys(value).sort().map((key) => [key, canonical(value[key])]));
  return value;
}

export function stableJson(value) { return `${JSON.stringify(canonical(value), null, 2)}\n`; }
export function digest(value) { return createHash("sha256").update(typeof value === "string" ? value : stableJson(value)).digest("hex"); }

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
  const pair = definition.fixtures.filter((fixture) => fixture.comparisonGroup === "global-scale-small-viewport");
  if (pair.length !== 2 || stableJson(pair[0].viewport) !== stableJson(pair[1].viewport) || pair[0].visibleDensity !== pair[1].visibleDensity || pair[0].globalUnitCount >= pair[1].globalUnitCount) throw new Error("large-project/small-viewport comparison is not controlled");
  return definition;
}

export function makeMap(fixture) {
  const [width, height] = fixture.mapExtent;
  const lines = ["SIR-MAP 2", `size ${width} ${height}`];
  for (let index = 0; index < Math.min(width, fixture.routeOverlayComplexity); index += 1) lines.push(`terrain ${index} ${index % height} ${index % 5 === 0 ? "objective" : "rough"}`);
  for (let index = 0; index < fixture.globalUnitCount; index += 1) lines.push(`unit ${index + 1} ${index % 2 ? "red" : "blue"} rifleman ${index % width} ${Math.floor(index / width) % height} 1 12 12 general -`);
  return `${lines.join("\n")}\n`;
}

const sumDuration = (events, names, predicate = () => true) => events.reduce((total, event) => total + (names.has(event.name) && predicate(event) ? Number(event.dur || 0) / 1000 : 0), 0);

export function extractStages(trace) {
  const events = Array.isArray(trace?.traceEvents) ? trace.traceEvents : [];
  const threads = new Map(events.filter((event) => event.name === "thread_name").map((event) => [event.tid, event.args?.name || ""]));
  const worker = (event) => /worker/i.test(threads.get(event.tid) || "");
  const available = (value, source) => ({ available: true, milliseconds: Number(value.toFixed(3)), source });
  const unavailable = (reason) => ({ available: false, reason });
  const values = {
    workerCompute: available(sumDuration(events, new Set(["RunTask", "FunctionCall"]), worker), "Chromium worker-thread trace events"),
    workerTransfer: unavailable("Chromium exposes transfer inside task slices but the production client has no dedicated transfer User Timing mark yet"),
    projectionAllocation: unavailable("The production client has no projection/allocation User Timing mark; GC remains separately visible in the raw trace"),
    elmishReact: available(sumDuration(events, new Set(["FunctionCall", "EvaluateScript"]), (event) => !worker(event)), "Chromium main-thread script trace events"),
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
  return {
    schema,
    interpretation: "Versioned regression workloads; not a permanent supported-size ceiling.",
    nextBottleneck: top ? { stage: top.stage, evidence: `largest measured aggregate at ${top.milliseconds} ms (${(top.share * 100).toFixed(1)}%)` } : { stage: "inconclusive", evidence: "no available duration observations" },
    ranking,
    dispositions: {
      packedTransport: share("workerTransfer") >= threshold ? "required" : "deferred",
      typedBuffers: Math.max(share("workerTransfer"), share("projectionAllocation")) >= threshold ? "required" : "deferred",
      furtherAllocationWork: share("projectionAllocation") >= threshold ? "required" : "deferred",
    },
  };
}
