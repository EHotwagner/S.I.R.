import { execFileSync } from "node:child_process";
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
const afterPath = resolve(reviewOutput, "after-production.png");
const audit = await auditPersistentWorkspaceBrowser({ clientRoot: clientOutput, screenshotPath: afterPath });
const system = audit.wide.visualSystem;
if (system.identity !== "tactical-visual-system-v1" || system.effectLimit !== 256) {
  throw new Error(`Production visual registry did not render: ${JSON.stringify(system)}`);
}
if (system.layerOrder !== "terrain>edges>routes>units>effects>selection>tactical-overlays>annotations") {
  throw new Error(`Production layer order drifted: ${system.layerOrder}`);
}

const scenes = [
  { name: "Ordinary", units: 20, columns: 5 },
  { name: "Dense", units: 100, columns: 10 },
  { name: "Stress", units: 200, columns: 20 },
];
const piece = (scene, index, panel) => {
  const spacing = scene.units > 100 ? 11 : scene.units > 20 ? 20 : 34;
  const size = scene.units > 100 ? 7 : scene.units > 20 ? 13 : 24;
  const row = Math.floor(index / scene.columns);
  const column = index % scene.columns;
  const x = panel + 28 + column * spacing;
  const y = 70 + row * spacing;
  const faction = index % 3 === 0 ? system.tokens["--sir-impact"] : system.tokens["--sir-focus"];
  return `<g data-prototype-unit="${index}"><rect x="${x}" y="${y}" width="${size}" height="${size}" rx="${Math.max(1, size / 5)}" fill="${system.tokens["--sir-canvas"]}" stroke="${faction}" stroke-width="2"/><path d="M${x + size / 2} ${y + size - 2}v${Math.max(3, size / 4)}" stroke="${faction}" stroke-width="2"/></g>`;
};
const prototypeSvg = `<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" width="1080" height="360" viewBox="0 0 1080 360" role="img" aria-labelledby="title description">
<title id="title">Tactical visual density prototypes</title><desc id="description">Ordinary, dense, and stress box-piece scenes using the production visual token registry.</desc>
<rect width="1080" height="360" fill="${system.tokens["--sir-canvas"]}"/>
${scenes.map((scene, sceneIndex) => {
  const panel = sceneIndex * 360;
  const pieces = Array.from({ length: scene.units }, (_, index) => piece(scene, index, panel)).join("");
  return `<g data-density-prototype="${scene.name.toLowerCase()}"><rect x="${panel + 10}" y="10" width="340" height="340" rx="12" fill="none" stroke="${system.tokens["--sir-grid"]}"/><text x="${panel + 28}" y="42" fill="${system.tokens["--sir-text"]}" font-family="sans-serif" font-size="18" font-weight="700">${scene.name} · ${scene.units} units</text><path d="M${panel + 30} 320L${panel + 330} 85" stroke="${system.tokens["--sir-intent"]}" stroke-width="3" stroke-dasharray="8 6" fill="none"/><circle cx="${panel + 280}" cy="150" r="16" fill="none" stroke="${system.tokens["--sir-suppression"]}" stroke-width="4"/>${pieces}</g>`;
}).join("")}
</svg>
`;
const prototypeSvgPath = resolve(reviewOutput, "density-prototypes.svg");
const prototypePngPath = resolve(reviewOutput, "density-prototypes.png");
await writeFile(prototypeSvgPath, prototypeSvg, "utf8");
execFileSync("/usr/sbin/rsvg-convert", ["--width", "1080", "--height", "360", "--output", prototypePngPath, prototypeSvgPath]);

const [afterBytes, prototypeSvgBytes, prototypePngBytes] = await Promise.all([
  readFile(afterPath), readFile(prototypeSvgPath), readFile(prototypePngPath),
]);
const manifest = {
  schema: "sir-tactical-visual-review-v1",
  productionBundleSha256: hash(bundleBytes),
  productionStylesSha256: hash(stylesBytes),
  before: { path: "../persistent-workspace-m9-review/field-focus.png", sha256: hash(baselineBytes) },
  after: { path: "after-production.png", sha256: hash(afterBytes), captureKind: "actual-production-shell-chromium-screenshot" },
  densityPrototypes: {
    svg: "density-prototypes.svg", svgSha256: hash(prototypeSvgBytes),
    png: "density-prototypes.png", pngSha256: hash(prototypePngBytes),
    scenes: scenes.map(({ name, units }) => ({ name, units })),
  },
  visualSystem: system,
  budgets: {
    representative100: { maximumEstimatedSvgNodes: 5000, maximumEffects: 128, releaseP95Milliseconds: 4 },
    stress200: { maximumEstimatedSvgNodes: 9000, maximumEffects: 256, releaseP95Milliseconds: 8 },
    browserAnimationFrameMilliseconds: 16.67,
  },
};
await writeFile(resolve(reviewOutput, "manifest.json"), `${JSON.stringify(manifest, null, 2)}\n`, "utf8");
console.log(`Captured tactical visual review for ${system.identity} and ${scenes.length} density prototypes.`);
