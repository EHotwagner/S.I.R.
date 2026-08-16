import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";

const hash = (bytes) => createHash("sha256").update(bytes).digest("hex");
const requireTruth = (condition, message) => { if (!condition) throw new Error(message); };
let clientRoot = "artifacts/client";
let reviewRoot = "docs/assets/tactical-visual-system-review";
for (let index = 2; index < process.argv.length; index += 2) {
  if (process.argv[index] === "--client-root") clientRoot = process.argv[index + 1];
  else if (process.argv[index] === "--review-root") reviewRoot = process.argv[index + 1];
  else throw new Error(`Unknown argument: ${process.argv[index]}`);
}
const root = resolve(reviewRoot);
const manifest = JSON.parse(await readFile(resolve(root, "manifest.json"), "utf8"));
const html = await readFile(resolve(clientRoot, "index.html"), "utf8");
const scriptMatch = html.match(/<script[^>]+src="([^"]+\.js)"/);
if (!scriptMatch) throw new Error("Production client entry is missing.");
const [bundle, styles] = await Promise.all([
  readFile(resolve(clientRoot, scriptMatch[1].replace(/^\.\//, ""))),
  readFile(resolve(clientRoot, "content/sir-client/v1/styles.css")),
]);
const stylesText = styles.toString("utf8");
requireTruth(manifest.schema === "sir-tactical-visual-review-v2", "review schema drifted");
requireTruth(manifest.productionBundleSha256 === hash(bundle), "review is not bound to the production bundle");
requireTruth(manifest.productionStylesSha256 === hash(styles), "review is not bound to the production stylesheet");
for (const declaration of ["--sir-canvas:#10161d", "animation-duration:var(--sir-effect-ms,.12s)!important"]) {
  requireTruth(stylesText.includes(declaration), `critical visual declaration drifted: ${declaration}`);
}
requireTruth(manifest.visualSystem.identity === "tactical-visual-system-v1", "visual registry identity drifted");
requireTruth(manifest.visualSystem.effectLimit === 256, "effect ceiling drifted");
requireTruth(manifest.visualSystem.effectCount > 0, "production after-state lost causal effects");
requireTruth(manifest.visualSystem.layerOrder === manifest.visualSystem.paintedLayerOrder, "declared and painted layers diverged");
requireTruth(manifest.visualSystem.effectKinds.length > 0 && manifest.visualSystem.effectLifecycles.length > 0, "effect kind/lifecycle semantics disappeared");
requireTruth(manifest.densityScenes.map(({ units }) => units).join(",") === "100,200", "production density fixtures drifted");
for (const scene of manifest.densityScenes) {
  const budget = scene.units === 100 ? manifest.budgets.representative100 : manifest.budgets.stress200;
  requireTruth(scene.workload.renderedUnits === scene.units, `${scene.units}-unit production render drifted`);
  requireTruth(scene.workload.effects > 0 && scene.workload.routes > 0 && scene.workload.plannedRouteUnits >= 2 && scene.workload.overlays > 0 && scene.workload.terrainCells > 0, `${scene.units}-unit combined workload lost tactical content`);
  requireTruth(scene.workload.domNodes <= budget.maximumDomNodes && scene.workload.effects <= budget.maximumEffects, `${scene.units}-unit structural budget exceeded`);
  requireTruth(scene.workload.inputToPaintMilliseconds < budget.maximumInputToPaintMilliseconds, `${scene.units}-unit input-to-paint budget exceeded`);
  requireTruth(scene.workload.animationFrameIntervalMilliseconds <= budget.targetAnimationFrameMilliseconds + budget.measurementToleranceMilliseconds, `${scene.units}-unit frame interval budget exceeded`);
  requireTruth(hash(await readFile(resolve(root, scene.path))) === scene.sha256, `production density image drifted: ${scene.path}`);
}
requireTruth(hash(await readFile(resolve(root, manifest.after.path))) === manifest.after.sha256, "production after screenshot drifted");
console.log("Tactical visual review is bundle/style-bound to effectful production 100/200-unit semantic and visual subjects.");
