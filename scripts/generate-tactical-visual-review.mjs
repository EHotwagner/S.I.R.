import { createHash } from "node:crypto";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { resolve } from "node:path";
import { auditPersistentWorkspaceBrowser } from "./lib/persistent-workspace-browser-audit.mjs";

const hash = (bytes) => createHash("sha256").update(bytes).digest("hex");
const clientOutput = resolve("artifacts/client");
const reviewOutput = resolve("docs/assets/tactical-visual-system-review");
const html = await readFile(resolve(clientOutput, "index.html"), "utf8");
const scriptMatch = html.match(/<script[^>]+src="([^"]+\.js)"/);
if (!scriptMatch) throw new Error("Build the production client before tactical visual review generation.");

const bundlePath = resolve(clientOutput, scriptMatch[1].replace(/^\.\//, ""));
const stylesPath = resolve(clientOutput, "content/sir-client/v1/styles.css");
const baselinePath = resolve("docs/assets/persistent-workspace-m9-review/field-focus.png");
const [bundleBytes, stylesBytes, baselineBytes] = await Promise.all([
  readFile(bundlePath), readFile(stylesPath), readFile(baselinePath),
]);

await mkdir(reviewOutput, { recursive: true });
const delaySource = `const wait = (milliseconds) => new Promise((resolve) => setTimeout(resolve, milliseconds));`;
const clickButtonSource = `const clickButton = (label) => { const button = [...document.querySelectorAll("button")].find((candidate) => candidate.textContent.trim() === label || candidate.getAttribute("aria-label") === label); if (!button) throw new Error("Missing production control: " + label); button.click(); };`;
const maintainedSimulation = `(async () => { ${delaySource} ${clickButtonSource} clickButton("Simulate"); await wait(50); clickButton("Show contextual actions"); await wait(50); clickButton("Open simulator samples"); await wait(50); const sample = document.querySelector('[aria-label^="Load simulation sample:"]'); if (!sample) throw new Error("Missing maintained simulation sample"); sample.click(); await wait(100); clickButton("Advance the map simulation one tick"); await wait(100); })()`;
const afterPath = resolve(reviewOutput, "after-production.png");
const audit = await auditPersistentWorkspaceBrowser({ clientRoot: clientOutput, screenshotPath: afterPath, prepareExpression: maintainedSimulation });
const system = audit.wide.visualSystem;
if (system.identity !== "tactical-visual-system-v1" || system.effectLimit !== 256 || system.effectCount < 1) {
  throw new Error(`Production visual registry/effects did not render: ${JSON.stringify(system)}`);
}
if (system.layerOrder !== system.paintedLayerOrder) {
  throw new Error(`Production layer order drifted: declared=${system.layerOrder}; painted=${system.paintedLayerOrder}`);
}

const workloadExpression = (units) => `(async () => {
  ${delaySource} ${clickButtonSource}
  globalThis.__sirTacticalStage = "samples";
  clickButton("Samples"); await wait(80);
  const card = [...document.querySelectorAll("details.sample-card")].find((candidate) => candidate.textContent.includes("Tactical density ${units}"));
  if (!card) throw new Error("Missing production tactical density ${units} sample");
  card.querySelector("summary").click(); await wait(40);
  globalThis.__sirTacticalStage = "simulate";
  const run = card.querySelector('[aria-label="Run Tactical density ${units} in Simulator"]');
  if (!run) throw new Error("Missing density sample simulator route");
  run.click(); await wait(180);
  globalThis.__sirTacticalStage = "routes";
  const routeUnits = [...document.querySelectorAll("#persistent-layer-units [data-unit-id]")].slice(0, 2);
  for (const unit of routeUnits) {
    unit.dispatchEvent(new MouseEvent("click", { bubbles: true })); await wait(25);
    clickButton("Move route preview up"); clickButton("Move route preview up"); await wait(25);
    clickButton("Commit clear route preview"); await wait(25);
  }
  const svg = document.querySelector("#persistent-tactical-svg"); const beforeTick = svg.getAttribute("data-scene-tick");
  globalThis.__sirTacticalStage = "step"; const started = performance.now(); clickButton("Advance the map simulation one tick");
  while (svg.getAttribute("data-scene-tick") === beforeTick && performance.now() - started < 2000) await wait(5);
  const inputToPaintMilliseconds = performance.now() - started;
  const frameStarted = await new Promise((resolve) => requestAnimationFrame(resolve));
  const frameEnded = await new Promise((resolve) => requestAnimationFrame(resolve));
  globalThis.__sirTacticalStage = "measure"; globalThis.__sirTacticalWorkload = {
    requestedUnits: ${units}, renderedUnits: svg.querySelectorAll("[data-unit-id]").length,
    terrainCells: svg.querySelectorAll("#persistent-layer-terrain > *").length,
    routes: svg.querySelectorAll("#persistent-layer-routes > *").length,
    plannedRouteUnits: [...svg.querySelectorAll("[data-unit-status]")].filter((unit) => unit.getAttribute("data-unit-status").includes("route-planned")).length,
    overlays: svg.querySelectorAll("[data-overlay-id]").length,
    effects: svg.querySelectorAll("[data-effect-event]").length,
    domNodes: svg.querySelectorAll("*").length,
    inputToPaintMilliseconds, animationFrameIntervalMilliseconds: frameEnded - frameStarted,
    usedJsHeapBytes: performance.memory?.usedJSHeapSize ?? null,
  };
})()`;

const densityAudits = [];
for (const units of [100, 200]) {
  const path = resolve(reviewOutput, `production-density-${units}.png`);
  const result = await auditPersistentWorkspaceBrowser({ clientRoot: clientOutput, screenshotPath: path, prepareExpression: workloadExpression(units) });
  const workload = result.wide.visualSystem.workload;
  if (workload.renderedUnits !== units || workload.effects < 1 || workload.overlays < 1) throw new Error(`Production density workload ${units} is incomplete: ${JSON.stringify(workload)}`);
  densityAudits.push({ units, path: `production-density-${units}.png`, sha256: hash(await readFile(path)), workload });
}

const afterBytes = await readFile(afterPath);
const manifest = {
  schema: "sir-tactical-visual-review-v2",
  productionBundleSha256: hash(bundleBytes), productionStylesSha256: hash(stylesBytes),
  before: { path: "../persistent-workspace-m9-review/field-focus.png", sha256: hash(baselineBytes) },
  after: { path: "after-production.png", sha256: hash(afterBytes), captureKind: "actual-production-shell-chromium-screenshot", semantic: system },
  densityScenes: densityAudits,
  visualSystem: system,
  budgets: {
    representative100: { maximumDomNodes: 5000, maximumEffects: 128, maximumInputToPaintMilliseconds: 100, targetAnimationFrameMilliseconds: 16.67, measurementToleranceMilliseconds: 1 },
    stress200: { maximumDomNodes: 9000, maximumEffects: 256, maximumInputToPaintMilliseconds: 150, targetAnimationFrameMilliseconds: 16.67, measurementToleranceMilliseconds: 1 },
  },
};
await writeFile(resolve(reviewOutput, "manifest.json"), `${JSON.stringify(manifest, null, 2)}\n`, "utf8");
console.log("Captured exact production tactical visual review with effectful after-state and 100/200-unit workloads.");
