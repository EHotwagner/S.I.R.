import { createHash } from "node:crypto";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { resolve } from "node:path";
import { auditPersistentWorkspaceBrowser } from "./lib/persistent-workspace-browser-audit.mjs";

const hash = (bytes) => createHash("sha256").update(bytes).digest("hex");
let clientRoot = "artifacts/client";
let reviewRoot = "docs/assets/tactical-visual-system-review";
for (let index = 2; index < process.argv.length; index += 2) {
  if (process.argv[index] === "--client-root") clientRoot = process.argv[index + 1];
  else if (process.argv[index] === "--review-root") reviewRoot = process.argv[index + 1];
  else throw new Error(`Unknown argument: ${process.argv[index]}`);
}
const clientOutput = resolve(clientRoot);
const reviewOutput = resolve(reviewRoot);
const html = await readFile(resolve(clientOutput, "index.html"), "utf8");
const scriptMatch = html.match(/<script[^>]+src="([^"]+\.js)"/);
if (!scriptMatch) throw new Error("Build the production client before tactical visual review generation.");

const bundlePath = resolve(clientOutput, scriptMatch[1].replace(/^\.\//, ""));
const stylesPath = resolve(clientOutput, "content/sir-client/v1/styles.css");
const baselinePath = resolve("docs/assets/persistent-workspace-m9-review/field-focus.png");
const reviewFontRegularPath = resolve("scripts/assets/tactical-visual-review-font/SIRReviewMono-Regular.woff2");
const reviewFontBoldPath = resolve("scripts/assets/tactical-visual-review-font/SIRReviewMono-Bold.woff2");
const [bundleBytes, stylesBytes, baselineBytes, reviewFontRegularBytes, reviewFontBoldBytes] = await Promise.all([
  readFile(bundlePath), readFile(stylesPath), readFile(baselinePath),
  readFile(reviewFontRegularPath), readFile(reviewFontBoldPath),
]);

const reviewFontCss = `
@font-face { font-family: "SIR Review Mono"; src: url("data:font/woff2;base64,${reviewFontRegularBytes.toString("base64")}") format("woff2"); font-style: normal; font-weight: 100 600; font-display: block; }
@font-face { font-family: "SIR Review Mono"; src: url("data:font/woff2;base64,${reviewFontBoldBytes.toString("base64")}") format("woff2"); font-style: normal; font-weight: 601 900; font-display: block; }
#sir-replay-app, #sir-replay-app * { font-family: "SIR Review Mono" !important; }
`;
const captureInputs = {
  schema: "sir-tactical-capture-inputs-v1",
  viewport: { width: 1440, height: 900, deviceScaleFactor: 1 },
  locale: "en-US", timezone: "UTC", colorScheme: "dark", reducedMotion: true,
  colorProfile: "srgb", gpu: "disabled", rasterThreads: 1,
  rasterMode: "complete-in-process", fontHinting: "none", lcdText: false,
  fonts: {
    family: "SIR Review Mono",
    regularSha256: hash(reviewFontRegularBytes), boldSha256: hash(reviewFontBoldBytes),
  },
};

await mkdir(reviewOutput, { recursive: true });
const delaySource = `const wait = (milliseconds) => new Promise((resolve) => setTimeout(resolve, milliseconds));`;
const readinessSource = `const waitFor = async (description, predicate, timeoutMilliseconds = 5000) => { const deadline = performance.now() + timeoutMilliseconds; while (performance.now() < deadline) { const ready = predicate(); if (ready) return ready; await wait(10); } throw new Error("Timed out waiting for " + description); };`;
const clickButtonSource = `const clickButton = (label) => { const button = [...document.querySelectorAll("button")].find((candidate) => candidate.textContent.trim() === label || candidate.getAttribute("aria-label") === label); if (!button) throw new Error("Missing production control: " + label); button.click(); };`;
const settleCaptureSource = `const settleCapture = async () => { let style = document.querySelector("#sir-deterministic-review-style"); if (!style) { style = document.createElement("style"); style.id = "sir-deterministic-review-style"; style.textContent = "html{scroll-behavior:auto!important}*,*::before,*::after{animation:none!important;transition:none!important;caret-color:transparent!important}"; document.head.appendChild(style); } document.documentElement.dataset.reviewCaptureState = "frozen-reduced-motion"; document.activeElement?.blur(); for (const animation of document.getAnimations()) animation.cancel(); for (const element of document.querySelectorAll("*")) { element.scrollTop = 0; element.scrollLeft = 0; } window.scrollTo(0, 0); await document.fonts.ready; await new Promise(requestAnimationFrame); await new Promise(requestAnimationFrame); await new Promise(requestAnimationFrame); await new Promise(requestAnimationFrame); };`;
const maintainedSimulation = `(async () => { ${delaySource} ${readinessSource} ${clickButtonSource} ${settleCaptureSource} clickButton("Simulate"); await wait(50); clickButton("Show contextual actions"); await wait(50); clickButton("Open simulator samples"); await waitFor("deferred Curated samples feature", () => document.querySelector('[aria-label="Curated samples feature"]')); const card = await waitFor("maintained Troll assault sample", () => [...document.querySelectorAll("details.sample-card")].find((candidate) => candidate.textContent.includes("Troll assault"))); card.querySelector("summary").click(); await wait(20); clickButton("Run Troll assault in Simulator"); await waitFor("maintained simulator scene", () => document.querySelector('#persistent-tactical-svg[data-scene-owner="SimulatorScene"] [data-unit-id]')); clickButton("Advance the map simulation one tick"); await wait(100); await settleCapture(); })()`;
const afterPath = resolve(reviewOutput, "after-production.png");
const audit = await auditPersistentWorkspaceBrowser({ clientRoot: clientOutput, screenshotPath: afterPath, prepareExpression: maintainedSimulation, captureStyleText: reviewFontCss, reducedMotion: true });
captureInputs.browserUserAgent = audit.chromium;
captureInputs.browserVersion = audit.chromiumVersion;
const { workload: ignoredAfterWorkload, ...system } = audit.wide.visualSystem;
if (system.identity !== "tactical-visual-system-v1" || system.effectLimit !== 256 || system.effectCount < 1) {
  throw new Error(`Production visual registry/effects did not render: ${JSON.stringify(system)}`);
}
if (system.layerOrder !== system.paintedLayerOrder) {
  throw new Error(`Production layer order drifted: declared=${system.layerOrder}; painted=${system.paintedLayerOrder}`);
}

const workloadExpression = (units) => `(async () => {
  ${delaySource} ${clickButtonSource} ${settleCaptureSource}
  ${readinessSource}
  globalThis.__sirTacticalStage = "samples"; clickButton("Simulate"); await wait(50); clickButton("Open simulator samples");
  await waitFor("deferred Curated samples feature", () => document.querySelector('[aria-label="Curated samples feature"]'));
  globalThis.__sirTacticalStage = "simulate"; if (!globalThis.__sirSamplesFeature.runVisualQualificationSample(${units})) throw new Error("Missing density qualification route");
  const svg = document.querySelector("#persistent-tactical-svg");
  await waitFor("production tactical density ${units} simulator scene", () => svg.getAttribute("data-scene-owner") === "SimulatorScene" && svg.querySelectorAll("[data-unit-id]").length === ${units});
  await settleCapture();
  const percentile80 = (values) => [...values].sort((left, right) => left - right)[3];
  const inputToPaintSamples = [];
  for (let sample = 0; sample < 5; sample += 1) {
    const beforeTick = svg.getAttribute("data-scene-tick");
    globalThis.__sirTacticalStage = "step"; const started = performance.now(); clickButton("Advance the map simulation one tick");
    while (svg.getAttribute("data-scene-tick") === beforeTick && performance.now() - started < 2000) await wait(5);
    inputToPaintSamples.push(performance.now() - started);
  }
  const inputToPaintMilliseconds = percentile80(inputToPaintSamples);
  globalThis.__sirTacticalStage = "routes";
  const routeUnits = [...document.querySelectorAll("#persistent-layer-units [data-unit-id]")].slice(0, 2);
  routeUnits[0].dispatchEvent(new MouseEvent("click", { bubbles: true })); await wait(25);
  clickButton("Move route preview right"); await wait(25);
  clickButton("Cancel route preview"); await wait(25);
  for (const [routeIndex, unit] of routeUnits.entries()) {
    unit.dispatchEvent(new MouseEvent("click", { bubbles: true })); await wait(25);
    clickButton("Move route preview up"); clickButton("Move route preview up"); await wait(25);
    clickButton("Commit clear route preview");
    await waitFor("committed route " + (routeIndex + 1), () => svg.querySelectorAll("#persistent-layer-routes > polyline").length >= routeIndex + 1);
  }
  const frameTimes = [];
  for (let sample = 0; sample < 6; sample += 1) frameTimes.push(await new Promise((resolve) => requestAnimationFrame(resolve)));
  const animationFrameIntervalMilliseconds = percentile80(frameTimes.slice(1).map((value, index) => value - frameTimes[index]));
  await settleCapture();
  const currentEffects = [...svg.querySelectorAll("[data-effect-kind]")];
  const currentKinds = [...new Set(currentEffects.map((effect) => effect.getAttribute("data-effect-kind")).filter(Boolean))].sort();
  const currentLifecycles = [...new Set(currentEffects.map((effect) => effect.getAttribute("data-effect-lifecycle")).filter(Boolean))].sort();
  globalThis.__sirTacticalStage = "measure"; globalThis.__sirTacticalWorkload = {
    requestedUnits: ${units}, renderedUnits: svg.querySelectorAll("[data-unit-id]").length,
    terrainCells: svg.querySelectorAll("#persistent-layer-terrain > *").length,
    routes: svg.querySelectorAll("#persistent-layer-routes > *").length,
    routeGeometries: [...svg.querySelectorAll("#persistent-layer-routes > polyline")].map((route) => route.getAttribute("points")),
    plannedRouteUnits: [...svg.querySelectorAll("[data-unit-status]")].filter((unit) => unit.getAttribute("data-unit-status").includes("route-planned")).length,
    overlays: svg.querySelectorAll("[data-overlay-id]").length,
    effects: svg.querySelectorAll("[data-effect-event]").length,
    currentAttackEffects: currentEffects.filter((effect) => effect.getAttribute("data-effect-kind") === "attack").length,
    effectKinds: currentKinds, effectLifecycles: currentLifecycles,
    domNodes: svg.querySelectorAll("*").length,
    inputToPaintMilliseconds, animationFrameIntervalMilliseconds,
    usedJsHeapBytes: performance.memory?.usedJSHeapSize ?? null,
  };
})()`;

const densityAudits = [];
const telemetryScenes = [];
for (const units of [100, 200]) {
  const path = resolve(reviewOutput, `production-density-${units}.png`);
  const result = await auditPersistentWorkspaceBrowser({ clientRoot: clientOutput, screenshotPath: path, prepareExpression: workloadExpression(units), captureStyleText: reviewFontCss, reducedMotion: true });
  const workload = result.wide.visualSystem.workload;
  if (workload.renderedUnits !== units || workload.effects < 1 || workload.overlays < 1 || workload.currentAttackEffects < 1) throw new Error(`Production density workload ${units} is incomplete: ${JSON.stringify(workload)}`);
  const { inputToPaintMilliseconds, animationFrameIntervalMilliseconds, usedJsHeapBytes, ...structure } = workload;
  densityAudits.push({ units, path: `production-density-${units}.png`, sha256: hash(await readFile(path)), workload: structure });
  telemetryScenes.push({ units, inputToPaintMilliseconds, animationFrameIntervalMilliseconds, usedJsHeapBytes });
}

const afterBytes = await readFile(afterPath);
const manifest = {
  schema: "sir-tactical-visual-review-v2",
  captureInputs,
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
await writeFile(resolve(reviewOutput, "telemetry.json"), `${JSON.stringify({ schema: "sir-tactical-visual-telemetry-v1", densityScenes: telemetryScenes }, null, 2)}\n`, "utf8");
console.log("Captured exact production tactical visual review with effectful after-state and 100/200-unit workloads.");
