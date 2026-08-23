import { createHash } from "node:crypto";
import { execFileSync } from "node:child_process";
import { mkdtemp, readFile, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import { resolve } from "node:path";
// Every budget this file gates on is DERIVED, and every comparison it makes is declared with the
// budget rather than written here (S.I.R.#318). This script cannot run without a built client and a
// browser, so it cannot be inverted in-process; keeping the predicates in the declaration module is
// what lets scripts/test-svg-pipeline-measurement.mjs break the subject and observe the refusal.
import { tacticalFrameCadenceBudget, tacticalFrameCadenceBudgetReason, tacticalInputToPaintBudgetReason, tacticalReviewManifestBudgets as declaredManifestBudgets, tacticalRuntimeEffectCap, tacticalStructuralBudgetReason, tacticalWorkloadBudgetFor } from "./lib/performance-budget.mjs";

const hash = (bytes) => createHash("sha256").update(bytes).digest("hex");
const requireTruth = (condition, message) => { if (!condition) throw new Error(message); };
const manifestDelta = (expectedBytes, actualBytes) => {
  const expected = JSON.parse(expectedBytes.toString("utf8"));
  const actual = JSON.parse(actualBytes.toString("utf8"));
  const differences = [];
  const compare = (left, right, path = "manifest") => {
    if (Object.is(left, right)) return;
    if (
      left && right && typeof left === "object" && typeof right === "object" &&
      Array.isArray(left) === Array.isArray(right)
    ) {
      const keys = [...new Set([...Object.keys(left), ...Object.keys(right)])].sort();
      for (const key of keys) compare(left[key], right[key], `${path}.${key}`);
      return;
    }
    differences.push(`${path}: retained=${JSON.stringify(left)}, reproduced=${JSON.stringify(right)}`);
  };
  compare(expected, actual);
  return differences.length > 0 ? differences.join(" | ") : "JSON values agree but serialized bytes differ";
};
const exactMismatch = (relativePath, retained, reproduced, label) => {
  const semanticDelta = relativePath === "manifest.json" ? `; delta=${manifestDelta(retained, reproduced)}` : "";
  return `${label}: ${relativePath}; retained sha256=${hash(retained)} bytes=${retained.length}; reproduced sha256=${hash(reproduced)} bytes=${reproduced.length}${semanticDelta}`;
};
let clientRoot = "artifacts/client";
let reviewRoot = "docs/assets/tactical-visual-system-review";
for (let index = 2; index < process.argv.length; index += 2) {
  if (process.argv[index] === "--client-root") clientRoot = process.argv[index + 1];
  else if (process.argv[index] === "--review-root") reviewRoot = process.argv[index + 1];
  else throw new Error(`Unknown argument: ${process.argv[index]}`);
}
const root = resolve(reviewRoot);
const manifest = JSON.parse(await readFile(resolve(root, "manifest.json"), "utf8"));
const telemetry = JSON.parse(await readFile(resolve(root, "telemetry.json"), "utf8"));
const html = await readFile(resolve(clientRoot, "index.html"), "utf8");
const scriptMatch = html.match(/<script[^>]+src="([^"]+\.js)"/);
if (!scriptMatch) throw new Error("Production client entry is missing.");
const [bundle, styles, reviewFontRegular, reviewFontBold] = await Promise.all([
  readFile(resolve(clientRoot, scriptMatch[1].replace(/^\.\//, ""))),
  readFile(resolve(clientRoot, "content/sir-client/v1/styles.css")),
  readFile(resolve("scripts/assets/tactical-visual-review-font/SIRReviewMono-Regular.woff2")),
  readFile(resolve("scripts/assets/tactical-visual-review-font/SIRReviewMono-Bold.woff2")),
]);
const stylesText = styles.toString("utf8");
requireTruth(manifest.schema === "sir-tactical-visual-review-v3", "review schema drifted");
requireTruth(manifest.productionBundleSha256 === hash(bundle), "review is not bound to the production bundle");
requireTruth(manifest.productionStylesSha256 === hash(styles), "review is not bound to the production stylesheet");
requireTruth(
  manifest.captureInputs?.schema === "sir-tactical-capture-inputs-v1" &&
    manifest.captureInputs.viewport?.width === 1440 &&
    manifest.captureInputs.viewport?.height === 900 &&
    manifest.captureInputs.viewport?.deviceScaleFactor === 1 &&
    manifest.captureInputs.locale === "en-US" &&
    manifest.captureInputs.timezone === "UTC" &&
    manifest.captureInputs.colorScheme === "dark" &&
    manifest.captureInputs.reducedMotion === true &&
    manifest.captureInputs.colorProfile === "srgb" &&
    manifest.captureInputs.gpu === "disabled" &&
    manifest.captureInputs.rasterThreads === 1 &&
    manifest.captureInputs.rasterMode === "complete-in-process" &&
    manifest.captureInputs.fontHinting === "none" &&
    manifest.captureInputs.lcdText === false &&
    manifest.captureInputs.browserUserAgent.includes("HeadlessChrome/151.0.0.0") &&
    manifest.captureInputs.browserVersion === "Chrome/151.0.7922.34",
  "review capture inputs are not fully frozen",
);
requireTruth(manifest.captureInputs.fonts?.family === "SIR Review Mono", "review capture font family drifted");
requireTruth(manifest.captureInputs.fonts.regularSha256 === hash(reviewFontRegular), "review capture regular font bytes drifted");
requireTruth(manifest.captureInputs.fonts.boldSha256 === hash(reviewFontBold), "review capture bold font bytes drifted");
for (const declaration of ["--sir-canvas:#10161d", "animation-duration:var(--sir-effect-ms,.12s)!important"]) {
  requireTruth(stylesText.includes(declaration), `critical visual declaration drifted: ${declaration}`);
}
requireTruth(manifest.visualSystem.identity === "tactical-visual-system-v1", "visual registry identity drifted");
requireTruth(manifest.visualSystem.effectLimit === tacticalRuntimeEffectCap.maximumEffectInstances, `effect ceiling drifted from the declared runtime cap (${tacticalRuntimeEffectCap.path} ${tacticalRuntimeEffectCap.binding})`);
requireTruth(manifest.visualSystem.effectCount > 0, "production after-state lost causal effects");
requireTruth(manifest.visualSystem.layerOrder === manifest.visualSystem.paintedLayerOrder, "declared and painted layers diverged");
requireTruth(manifest.visualSystem.effectKinds.length > 0 && manifest.visualSystem.effectLifecycles.length > 0, "effect kind/lifecycle semantics disappeared");
requireTruth(manifest.densityScenes.map(({ units }) => units).join(",") === "100,200", "production density fixtures drifted");
requireTruth(telemetry.schema === "sir-tactical-visual-telemetry-v1" && telemetry.densityScenes.map(({ units }) => units).join(",") === "100,200", "production telemetry drifted");
for (const scene of manifest.densityScenes) {
  // The DECLARED budget for this workload, not the manifest's copy of it. Reading the generated
  // manifest back would compare an artifact against itself: a generator that published the wrong
  // ceiling would be gated by the wrong ceiling and report green.
  const budget = tacticalWorkloadBudgetFor(scene.units);
  const measured = telemetry.densityScenes.find(({ units }) => units === scene.units);
  requireTruth(scene.workload.renderedUnits === scene.units, `${scene.units}-unit production render drifted`);
  requireTruth(
    scene.workload.effects > 0 &&
      scene.workload.currentAttackEffects > 0 &&
      scene.workload.routes >= 2 &&
      scene.workload.plannedRouteUnits >= 2 &&
      scene.workload.routeGeometries.length >= 2 &&
      new Set(scene.workload.routeGeometries).size >= 2 &&
      scene.workload.overlays > 0 &&
      scene.workload.terrainCells > 0,
    `${scene.units}-unit final simultaneous workload lost distinct route, attack, movement, lifecycle, overlay, or terrain content`,
  );
  requireTruth(["attack", "movement"].every((kind) => scene.workload.effectKinds.includes(kind)), `${scene.units}-unit production effect kinds drifted`);
  requireTruth(["accepted", "committed", "predicted"].every((lifecycle) => scene.workload.effectLifecycles.includes(lifecycle)), `${scene.units}-unit final simultaneous lifecycle state drifted`);
  const structuralReason = tacticalStructuralBudgetReason(budget, scene.workload);
  requireTruth(!structuralReason, `${structuralReason}`);
  requireTruth(measured, `${scene.units}-unit telemetry is missing; an unmeasured budget is refused, not passed`);
  const inputToPaintReason = tacticalInputToPaintBudgetReason(budget, measured.inputToPaintMilliseconds);
  requireTruth(!inputToPaintReason, `${inputToPaintReason}`);
  const cadenceReason = tacticalFrameCadenceBudgetReason(budget, measured.animationFrameIntervalMilliseconds);
  requireTruth(!cadenceReason, `${cadenceReason}`);
  // The published manifest must carry the SAME numbers this gate just enforced, or the artifact a
  // reviewer reads and the budget CI applies are two different things.
  const publishedBudget = manifest.budgets[budget.key];
  requireTruth(publishedBudget, `the manifest publishes no budget for ${budget.key}`);
  for (const [field, declared] of Object.entries(declaredManifestBudgets[budget.key]))
    requireTruth(publishedBudget[field] === declared, `manifest budgets.${budget.key}.${field} publishes ${JSON.stringify(publishedBudget[field])}; the declaration says ${JSON.stringify(declared)}`);
  requireTruth(Object.keys(publishedBudget).length === Object.keys(declaredManifestBudgets[budget.key]).length, `manifest budgets.${budget.key} publishes fields the declaration does not: ${Object.keys(publishedBudget).join(", ")}`);
  requireTruth(hash(await readFile(resolve(root, scene.path))) === scene.sha256, `production density image drifted: ${scene.path}`);
}
requireTruth(hash(await readFile(resolve(root, manifest.after.path))) === manifest.after.sha256, "production after screenshot drifted");
const reproductionRoots = await Promise.all([
  mkdtemp(resolve(tmpdir(), "sir-tactical-review-reproduction-a-")),
  mkdtemp(resolve(tmpdir(), "sir-tactical-review-reproduction-b-")),
]);
let reproductionAccepted = false;
try {
  for (const reproductionRoot of reproductionRoots) {
    execFileSync(process.execPath, [resolve("scripts/generate-tactical-visual-review.mjs"), "--client-root", resolve(clientRoot), "--review-root", reproductionRoot], { cwd: process.cwd(), stdio: "pipe" });
  }
  for (const relativePath of ["manifest.json", "after-production.png", "production-density-100.png", "production-density-200.png"]) {
    const [expected, reproducedA, reproducedB] = await Promise.all([
      readFile(resolve(root, relativePath)),
      readFile(resolve(reproductionRoots[0], relativePath)),
      readFile(resolve(reproductionRoots[1], relativePath)),
    ]);
    requireTruth(expected.equals(reproducedA), exactMismatch(relativePath, expected, reproducedA, "production review did not reproduce byte-for-byte"));
    requireTruth(reproducedA.equals(reproducedB), exactMismatch(relativePath, reproducedA, reproducedB, "independent frozen production captures diverged"));
  }
  const reproducedMeasurements = [];
  for (const [reproductionIndex, reproductionRoot] of reproductionRoots.entries()) {
    const reproducedTelemetry = JSON.parse(await readFile(resolve(reproductionRoot, "telemetry.json"), "utf8"));
    for (const scene of reproducedTelemetry.densityScenes) {
      const budget = tacticalWorkloadBudgetFor(scene.units);
      reproducedMeasurements.push({
        reproduction: reproductionIndex === 0 ? "A" : "B",
        root: reproductionRoot,
        units: scene.units,
        inputToPaintMilliseconds: scene.inputToPaintMilliseconds,
        maximumInputToPaintMilliseconds: budget.maximumInputToPaintMilliseconds,
        animationFrameIntervalMilliseconds: scene.animationFrameIntervalMilliseconds,
        maximumAnimationFrameIntervalMilliseconds: tacticalFrameCadenceBudget.intervalCeilingMilliseconds,
      });
    }
  }
  await new Promise((resolveWrite, rejectWrite) => process.stdout.write(
    `Tactical reproduced telemetry before assertions: ${JSON.stringify(reproducedMeasurements)}\n`,
    (error) => error ? rejectWrite(error) : resolveWrite(),
  ));
  for (const measurement of reproducedMeasurements) {
    const budget = tacticalWorkloadBudgetFor(measurement.units);
    const reproducedInputToPaint = tacticalInputToPaintBudgetReason(budget, measurement.inputToPaintMilliseconds);
    requireTruth(!reproducedInputToPaint, `reproduced ${reproducedInputToPaint}: reproduction=${measurement.reproduction}; telemetry=${measurement.root}/telemetry.json`);
    const reproducedCadence = tacticalFrameCadenceBudgetReason(budget, measurement.animationFrameIntervalMilliseconds);
    requireTruth(!reproducedCadence, `reproduced ${reproducedCadence}: reproduction=${measurement.reproduction}; telemetry=${measurement.root}/telemetry.json`);
  }
  reproductionAccepted = true;
} finally {
  if (reproductionAccepted) {
    await Promise.all(reproductionRoots.map((path) => rm(path, { recursive: true, force: true })));
  } else {
    process.stderr.write(`Preserved tactical reproduction roots after failure: ${reproductionRoots.join(", ")}\n`);
  }
}
console.log("Tactical visual review is bundle/style-bound to effectful production 100/200-unit semantic and visual subjects.");
