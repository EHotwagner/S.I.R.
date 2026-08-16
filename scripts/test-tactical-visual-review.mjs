import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";

const hash = (bytes) => createHash("sha256").update(bytes).digest("hex");
const root = resolve("docs/assets/tactical-visual-system-review");
const manifest = JSON.parse(await readFile(resolve(root, "manifest.json"), "utf8"));
const html = await readFile(resolve("artifacts/client/index.html"), "utf8");
const scriptMatch = html.match(/<script[^>]+src="([^"]+\.js)"/);
if (!scriptMatch) throw new Error("Production client entry is missing.");
const bundle = await readFile(resolve("artifacts/client", scriptMatch[1].replace(/^\.\//, "")));
const requireTruth = (condition, message) => { if (!condition) throw new Error(message); };
requireTruth(manifest.schema === "sir-tactical-visual-review-v1", "review schema drifted");
requireTruth(manifest.productionBundleSha256 === hash(bundle), "review is not bound to the production bundle");
requireTruth(manifest.visualSystem.identity === "tactical-visual-system-v1", "visual registry identity drifted");
requireTruth(manifest.visualSystem.effectLimit === 256, "effect ceiling drifted");
requireTruth(manifest.densityPrototypes.scenes.map(({ units }) => units).join(",") === "20,100,200", "density fixtures drifted");
for (const artifact of [manifest.after, { path: manifest.densityPrototypes.svg, sha256: manifest.densityPrototypes.svgSha256 }, { path: manifest.densityPrototypes.png, sha256: manifest.densityPrototypes.pngSha256 }]) {
  requireTruth(hash(await readFile(resolve(root, artifact.path))) === artifact.sha256, `review artifact hash drifted: ${artifact.path}`);
}
console.log("Tactical visual review is exact-bundle-bound with deterministic 20/100/200 density fixtures.");
