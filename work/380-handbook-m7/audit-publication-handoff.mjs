#!/usr/bin/env node
import fs from "node:fs";
import crypto from "node:crypto";
import { execFileSync } from "node:child_process";

const root = process.cwd();
const read = path => fs.readFileSync(path, "utf8");
const json = path => JSON.parse(read(path));
const gitBlob = path => execFileSync("git", ["hash-object", path], { encoding: "utf8" }).trim();
const fail = message => { throw new Error(`handbook-m7 audit: ${message}`); };
const need = (condition, message) => { if (!condition) fail(message); };

const paths = {
  handbook: "docs/sir-combat-quint-handbook.md",
  model: "docs/rules/sir-combat.md",
  maintenance: "docs/rules/sir-combat-handbook-maintenance.md",
  record: "work/380-handbook-m7/publication-record.json",
  reviews: "work/380-handbook-m7/publication-reviews.json",
  diagrams: "docs/sir-combat-quint-diagrams.json",
  performance: "readiness/377-handbook-m6v/performance-evidence.json",
  inspection: "readiness/377-handbook-m6v/rendered/inspection.json"
};

function verify(overrides = new Map(), options = {}) {
  const text = path => overrides.has(path) ? overrides.get(path) : read(path);
  const parsed = path => JSON.parse(text(path));
  const handbook = text(paths.handbook);
  const model = text(paths.model);
  const record = parsed(paths.record);
  const reviews = parsed(paths.reviews);
  const diagrams = parsed(paths.diagrams);
  const performance = parsed(paths.performance);
  const inspection = parsed(paths.inspection);

  need(/^status: maintained$/m.test(handbook), "handbook is not maintained");
  need(!handbook.includes("*Scheduled content:*"), "scheduled placeholder remains");
  need(handbook.includes("#### Maintenance and owner handoff"), "handbook maintenance handoff missing");
  const maintenance = text(paths.maintenance);
  need(maintenance.includes("# Combat in Quint handbook maintenance trigger"), "model-adjacent trigger missing");
  need(maintenance.includes("**Owner:** the S.I.R. repository maintainer"), "explicit maintenance owner missing");
  need(maintenance.includes("existing M6V render/performance evidence"), "model trigger does not retain M6V evidence intent");

  need(record.schemaVersion === 1, "publication record schema mismatch");
  need(record.authorityPosture.includes("authorities remain distinct"), "publication record merges authorities");
  need(record.verificationBase === "318f07a7ae30e98d86a5d5780126cab1e105b7f2", "verification base drift");
  need(record.owner?.triggerLocation === paths.maintenance, "owner trigger location drift");
  for (const source of record.sourceBlobs) {
    const actual = overrides.has(source.path)
      ? crypto.createHash("sha1").update(`blob ${Buffer.byteLength(text(source.path))}\0`).update(text(source.path)).digest("hex")
      : gitBlob(source.path);
    need(source.gitBlob === actual, `stale source blob ${source.path}`);
  }

  const expectedTools = new Map([
    [".NET SDK", json("global.json").sdk.version],
    ["Node.js", json("package.json").engines.node],
    ["Quint", "0.32.0"],
    ["FsDocs", json(".config/dotnet-tools.json").tools["fsdocs-tool"].version],
    ["Playwright", json("package-lock.json").packages["node_modules/@playwright/test"].version],
    ["FS.GG.SDD.Cli", "1.4.0"],
    ["FS.GG.Coord.Cli", json(".config/dotnet-tools.json").tools["fs.gg.coord.cli"].version]
  ]);
  need(record.toolchain.length === expectedTools.size, "toolchain cardinality drift");
  for (const tool of record.toolchain) need(expectedTools.get(tool.tool) === tool.version, `tool identity drift: ${tool.tool}`);

  for (const evidence of Object.values(record.inheritedEvidence)) {
    need(evidence.path && evidence.gitBlob === gitBlob(evidence.path), `inherited evidence drift: ${evidence.path}`);
  }
  const sample = performance.sampleSets?.find(item => item.workloadId === "handbook-m6v-six-diagram-render-v1");
  need(sample && sample.maxP95Ms === 100 && sample.maxP99Ms === 200, "M6V typed performance budgets changed");
  need(inspection.capability?.liveCompositor === false && inspection.capability?.framePacingMeasured === false, "retained inspection overclaims compositor or frame pacing evidence");
  need(record.inheritedEvidence.m6vPerformance.maxP95Ms === 100 && record.inheritedEvidence.m6vPerformance.maxP99Ms === 200, "publication record weakens M6V budgets");
  need(record.inheritedEvidence.m6vPerformance.liveCompositorRequired === false, "publication record overclaims compositor evidence");
  need(diagrams.diagrams?.length === 6, "diagram manifest no longer contains six diagrams");
  need(record.inheritedEvidence.m6vRenderedInspection.diagramCount === 6, "rendered evidence diagram count drift");
  need(record.inheritedEvidence.m6vRenderedInspection.standaloneRenders === 30, "standalone render count drift");
  need(record.inheritedEvidence.m6vRenderedInspection.handbookRenders === 18, "handbook render count drift");
  need(Array.isArray(inspection.modes) || JSON.stringify(inspection).includes("css-disabled"), "render inspection lacks fallback evidence");
  for (const evidence of [record.currentRenderEvidence.inspection, record.currentRenderEvidence.performance]) {
    need(evidence.gitBlob === gitBlob(evidence.path), `current render evidence drift: ${evidence.path}`);
  }
  const currentInspection = parsed(record.currentRenderEvidence.inspection.path);
  const currentPerformance = parsed(record.currentRenderEvidence.performance.path);
  const currentSample = currentPerformance.sampleSets?.find(item => item.workloadId === "handbook-m6v-six-diagram-render-v1");
  need(currentInspection.provenance?.candidateSourceInputsSha256 === record.currentRenderEvidence.candidateSourceInputsSha256, "current inspection source digest drift");
  need(currentSample?.candidateSourceInputsSha256 === record.currentRenderEvidence.candidateSourceInputsSha256, "current performance source digest drift");
  need(currentInspection.timings?.p95LoadMs === record.currentRenderEvidence.p95LoadMs && currentInspection.timings?.p99LoadMs === record.currentRenderEvidence.p99LoadMs, "current render metric record drift");
  need(record.currentRenderEvidence.diagrams === 6 && record.currentRenderEvidence.modes === 5 && record.currentRenderEvidence.screenshots === 48, "current render evidence cardinality drift");
  need(record.currentRenderEvidence.maxP95Ms === 100 && record.currentRenderEvidence.maxP99Ms === 200, "current render budgets drift");
  need(record.currentRenderEvidence.p95LoadMs <= 100 && record.currentRenderEvidence.p99LoadMs <= 200, "current render measurements exceed budget");
  need(record.currentRenderEvidence.liveCompositor === false && record.currentRenderEvidence.framePacingMeasured === false, "current render record overclaims compositor evidence");
  const validity = parsed(record.measurementValidity.path);
  need(validity.schema === record.measurementValidity.schema && validity.status === "pass", "render measurement validity receipt is not passing");
  need(validity.observedNode === record.measurementValidity.observedNode && validity.observedNode === validity.policy?.exactNode && validity.policy.exactNode === record.measurementValidity.exactNode, "render validity observed Node identity drift");
  need(validity.policy?.batches === record.measurementValidity.batches, "render validity batch cardinality drift");
  need(validity.policy?.preflightSamples === record.measurementValidity.preflightSamples && validity.policy?.preflightIntervalMs === record.measurementValidity.preflightIntervalMs, "render validity preflight policy drift");
  need(validity.policy?.maxCpuUtilization === record.measurementValidity.maxCpuUtilization && validity.policy?.maxCpuPressureSomeAvg10 === record.measurementValidity.maxCpuPressureSomeAvg10, "render validity host threshold drift");
  need(validity.policy?.competingBrowserOrRenderProcessesAllowed === record.measurementValidity.competingBrowserOrRenderProcessesAllowed && validity.policy.competingBrowserOrRenderProcessesAllowed === false, "render validity competitor policy drift");
  need(validity.policy?.maxP95Ms === record.measurementValidity.maxP95Ms && validity.policy?.maxP99Ms === record.measurementValidity.maxP99Ms, "render validity budgets drift");
  need(validity.policy?.performanceIntent === record.measurementValidity.performanceIntent, "render validity invented a new performance intent");
  need(validity.batches?.length === 2, "render validity requires two comparable batches");
  for (const batch of validity.batches) {
    need(batch.candidateSourceInputsSha256 === record.currentRenderEvidence.candidateSourceInputsSha256, `render validity source drift: ${batch.id}`);
    need(batch.sampleCount === record.measurementValidity.samplesPerBatch && batch.warmupNavigations === record.measurementValidity.warmupsPerBatch, `render validity workload drift: ${batch.id}`);
    need(batch.p95LoadMs <= 100 && batch.p99LoadMs <= 200, `render validity budget exceeded: ${batch.id}`);
    need(batch.environment?.invalidSampleCount === 0 && batch.environment?.competitors?.length === 0, `render validity host was not quiescent: ${batch.id}`);
    need(batch.preflight?.length === record.measurementValidity.preflightSamples, `render validity preflight sample count drift: ${batch.id}`);
    need(batch.preflight.every(sample => sample.cpuUtilization <= record.measurementValidity.maxCpuUtilization && sample.cpuPressureSomeAvg10 <= record.measurementValidity.maxCpuPressureSomeAvg10 && sample.competitors?.length === 0), `render validity preflight was not quiescent: ${batch.id}`);
    need(batch.environment.maxCpuUtilization <= record.measurementValidity.maxCpuUtilization && batch.environment.maxCpuPressureSomeAvg10 <= record.measurementValidity.maxCpuPressureSomeAvg10, `render validity batch host threshold exceeded: ${batch.id}`);
  }

  need(reviews.schemaVersion === 1, "review manifest schema mismatch");
  need(reviews.reviewedSourceBlobs.handbook === record.sourceBlobs.find(x => x.path === paths.handbook).gitBlob, "review handbook binding drift");
  need(reviews.reviewedSourceBlobs.model === record.sourceBlobs.find(x => x.path === paths.model).gitBlob, "review model binding drift");
  need(reviews.reviewedSourceBlobs.diagramManifest === record.sourceBlobs.find(x => x.path === paths.diagrams).gitBlob, "review diagram binding drift");
  const subjects = new Set(record.reviewSubjects);
  need(reviews.reviews.length === 4, "exactly four publication reviews required");
  need(new Set(reviews.reviews.map(review => review.subject)).size === 4, "publication review subjects duplicate");
  for (const review of reviews.reviews) {
    need(subjects.delete(review.subject), `unexpected review subject ${review.subject}`);
    need(review.verdict === "approve", `review not approved: ${review.subject}`);
    need(typeof review.reviewer === "string" && review.reviewer.length > 0, `reviewer missing: ${review.subject}`);
    need(Array.isArray(review.checked) && review.checked.length >= 3, `review scope insubstantial: ${review.subject}`);
    need(Array.isArray(review.evidence) && review.evidence.length >= 1, `review evidence missing: ${review.subject}`);
    need(Array.isArray(review.limits) && review.limits.length >= 1, `review limits missing: ${review.subject}`);
  }
  need(subjects.size === 0, "required review subject missing");
  const currentInspectionPath = "readiness/380-handbook-m7/rendered/inspection.json";
  if (!options.preRender && fs.existsSync(currentInspectionPath)) {
    const current = parsed(currentInspectionPath);
    need(current.result === "pass" && current.workloadId === "handbook-m6v-six-diagram-render-v1", "current M7 render replay invalid");
    need(current.observations?.length === 5, "current M7 render replay lacks five modes");
    need(current.observations.every(mode => mode.diagrams?.length === 6), "current M7 render replay lacks six diagrams per mode");
    need(current.timings?.p95LoadMs <= 100 && current.timings?.p99LoadMs <= 200, "current M7 decoded-readiness budget exceeded");
    need(current.capability?.liveCompositor === false && current.capability?.framePacingMeasured === false, "current M7 replay overclaims compositor evidence");
  }
  return { reviews: reviews.reviews.length, diagrams: diagrams.diagrams.length, sourceBlobs: record.sourceBlobs.length, toolchain: record.toolchain.length };
}

function mutated(path, change) {
  const value = read(path);
  return new Map([[path, change(value)]]);
}

const preRender = process.argv.includes("--pre-render");
if (process.argv.includes("--self-test")) {
  const reviews = json(paths.reviews);
  const record = json(paths.record);
  const cases = [
    ["missing-domain-review", new Map([[paths.reviews, JSON.stringify({...reviews, reviews: reviews.reviews.filter(x => x.subject !== "domain")})]])],
    ["rejected-model-review", new Map([[paths.reviews, JSON.stringify({...reviews, reviews: reviews.reviews.map(x => x.subject === "quint-modeling" ? {...x, verdict: "changes-required"} : x)})]])],
    ["stale-handbook-identity", new Map([[paths.record, JSON.stringify({...record, sourceBlobs: record.sourceBlobs.map(x => x.path === paths.handbook ? {...x, gitBlob: "0".repeat(40)} : x)})]])],
    ["missing-owner-trigger", mutated(paths.maintenance, value => value.replace("# Combat in Quint handbook maintenance trigger", "# Removed trigger"))],
    ["scheduled-placeholder", mutated(paths.handbook, value => value + "\n*Scheduled content:* deliberate defect\n")],
    ["weakened-m6v-budget", new Map([[paths.record, JSON.stringify({...record, inheritedEvidence: {...record.inheritedEvidence, m6vPerformance: {...record.inheritedEvidence.m6vPerformance, maxP95Ms: 200}}})]])],
    ["invented-compositor-claim", new Map([[paths.record, JSON.stringify({...record, inheritedEvidence: {...record.inheritedEvidence, m6vPerformance: {...record.inheritedEvidence.m6vPerformance, liveCompositorRequired: true}}})]])],
    ["missing-m6-binding", new Map([[paths.record, JSON.stringify({...record, inheritedEvidence: {...record.inheritedEvidence, m6StructureAudit: {...record.inheritedEvidence.m6StructureAudit, gitBlob: "0".repeat(40)}}})]])]
  ];
  for (const [name, override] of cases) {
    let red = false;
    try { verify(override); } catch { red = true; }
    need(red, `mutation did not observe red: ${name}`);
    verify(new Map(), { preRender });
  }
  console.log(`handbook-m7 audit: PASS (${cases.length} observed-red/restored-green controls)`);
} else {
  const result = verify(new Map(), { preRender });
  console.log(`handbook-m7 audit: PASS (${result.reviews} reviews, ${result.diagrams} diagrams, ${result.sourceBlobs} source blobs, ${result.toolchain} tools)`);
}
