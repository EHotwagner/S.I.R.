import { createHash } from "node:crypto";
import { execFileSync } from "node:child_process";
import { mkdtemp, readFile, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
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
const telemetry = JSON.parse(await readFile(resolve(root, "telemetry.json"), "utf8"));
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
requireTruth(telemetry.schema === "sir-tactical-visual-telemetry-v1" && telemetry.densityScenes.map(({ units }) => units).join(",") === "100,200", "production telemetry drifted");
for (const scene of manifest.densityScenes) {
  const budget = scene.units === 100 ? manifest.budgets.representative100 : manifest.budgets.stress200;
  const measured = telemetry.densityScenes.find(({ units }) => units === scene.units);
  requireTruth(scene.workload.renderedUnits === scene.units, `${scene.units}-unit production render drifted`);
  requireTruth(scene.workload.effects > 0 && scene.workload.currentAttackEffects > 0 && scene.workload.routes > 0 && scene.workload.plannedRouteUnits >= 1 && scene.workload.overlays > 0 && scene.workload.terrainCells > 0, `${scene.units}-unit final simultaneous workload lost tactical content`);
  requireTruth(["attack", "movement"].every((kind) => scene.workload.effectKinds.includes(kind)), `${scene.units}-unit production effect kinds drifted`);
  requireTruth(["accepted", "committed", "predicted"].every((lifecycle) => scene.workload.effectLifecycles.includes(lifecycle)), `${scene.units}-unit final simultaneous lifecycle state drifted`);
  requireTruth(scene.workload.domNodes <= budget.maximumDomNodes && scene.workload.effects <= budget.maximumEffects, `${scene.units}-unit structural budget exceeded`);
  requireTruth(measured && measured.inputToPaintMilliseconds < budget.maximumInputToPaintMilliseconds, `${scene.units}-unit input-to-paint budget exceeded`);
  requireTruth(measured.animationFrameIntervalMilliseconds <= budget.targetAnimationFrameMilliseconds + budget.measurementToleranceMilliseconds, `${scene.units}-unit frame interval budget exceeded`);
  requireTruth(hash(await readFile(resolve(root, scene.path))) === scene.sha256, `production density image drifted: ${scene.path}`);
}
requireTruth(hash(await readFile(resolve(root, manifest.after.path))) === manifest.after.sha256, "production after screenshot drifted");
const reproductionRoots = await Promise.all([
  mkdtemp(resolve(tmpdir(), "sir-tactical-review-reproduction-a-")),
  mkdtemp(resolve(tmpdir(), "sir-tactical-review-reproduction-b-")),
]);
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
    requireTruth(expected.equals(reproducedA), `production review did not reproduce byte-for-byte: ${relativePath}`);
    requireTruth(reproducedA.equals(reproducedB), `independent frozen production captures diverged: ${relativePath}`);
  }
  for (const reproductionRoot of reproductionRoots) {
    const reproducedTelemetry = JSON.parse(await readFile(resolve(reproductionRoot, "telemetry.json"), "utf8"));
    for (const scene of reproducedTelemetry.densityScenes) {
      const budget = scene.units === 100 ? manifest.budgets.representative100 : manifest.budgets.stress200;
      requireTruth(scene.inputToPaintMilliseconds < budget.maximumInputToPaintMilliseconds, `${scene.units}-unit reproduced input-to-paint budget exceeded`);
      requireTruth(scene.animationFrameIntervalMilliseconds <= budget.targetAnimationFrameMilliseconds + budget.measurementToleranceMilliseconds, `${scene.units}-unit reproduced frame interval budget exceeded`);
    }
  }
} finally {
  await Promise.all(reproductionRoots.map((path) => rm(path, { recursive: true, force: true })));
}
console.log("Tactical visual review is bundle/style-bound to effectful production 100/200-unit semantic and visual subjects.");
