import { createHash } from "node:crypto";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { resolve } from "node:path";
import { auditPersistentWorkspaceBrowser } from "./lib/persistent-workspace-browser-audit.mjs";

const clientOutput = resolve("artifacts/client");
const reviewOutput = resolve("docs/assets/persistent-workspace-m9-review");
const html = await readFile(resolve(clientOutput, "index.html"), "utf8");
const scriptMatch = html.match(/<script[^>]+src="([^"]+\.js)"/);
if (!scriptMatch) throw new Error("Build the production client before M9 review generation.");

const bundlePath = resolve(clientOutput, scriptMatch[1].replace(/^\.\//, ""));
const stylesPath = resolve(clientOutput, "content/sir-client/v1/styles.css");
const mockupPath = resolve("docs/assets/persistent-workspace-mockups/index.html");
const hash = (bytes) => createHash("sha256").update(bytes).digest("hex");
const [bundleBytes, stylesBytes, mockupBytes] = await Promise.all([
  readFile(bundlePath), readFile(stylesPath), readFile(mockupPath),
]);

await mkdir(reviewOutput, { recursive: true });
const pngPath = resolve(reviewOutput, "field-focus.png");
const audit = await auditPersistentWorkspaceBrowser({ clientRoot: clientOutput, screenshotPath: pngPath });
const workscreenSvg = await readFile(
  resolve(clientOutput, "index.html"),
  "utf8",
).then(async () => {
  // The full-shell PNG is the review authority. This companion file records
  // measured live geometry rather than pretending to be a shell screenshot.
  const geometry = audit.wide;
  const escaped = JSON.stringify(geometry, null, 2)
    .replaceAll("&", "&amp;").replaceAll("<", "&lt;").replaceAll(">", "&gt;");
  return `<?xml version="1.0" encoding="UTF-8"?>\n` +
    `<svg xmlns="http://www.w3.org/2000/svg" width="1440" height="900" viewBox="0 0 1440 900" role="img" aria-labelledby="title description">` +
    `<title id="title">Measured production Field Focus geometry</title>` +
    `<desc id="description">Machine-readable companion to the actual Chromium full-shell screenshot. It is not a reconstructed interface.</desc>` +
    `<rect width="1440" height="900" fill="#101715"/>` +
    `<text x="32" y="48" fill="#ffd166" font-family="sans-serif" font-size="22">Measured production geometry companion</text>` +
    `<foreignObject x="32" y="72" width="1376" height="796"><pre xmlns="http://www.w3.org/1999/xhtml" style="color:#eef7f2;font:12px monospace;white-space:pre-wrap">${escaped}</pre></foreignObject>` +
    `</svg>\n`;
});
const svgPath = resolve(reviewOutput, "field-focus-geometry.svg");
await writeFile(svgPath, workscreenSvg, "utf8");
const [pngBytes, svgBytes] = await Promise.all([readFile(pngPath), readFile(svgPath)]);

const manifest = {
  schema: "sir-persistent-workspace-m9-live-review-v2",
  captureKind: "actual-production-shell-chromium-screenshot",
  productionBundleSha256: hash(bundleBytes),
  productionStylesSha256: hash(stylesBytes),
  acceptedMockupSha256: hash(mockupBytes),
  chromiumUserAgent: audit.chromium,
  fieldFocus: audit.wide,
  narrow400PercentEquivalent: audit.narrow,
  png: "field-focus.png",
  geometrySvg: "field-focus-geometry.svg",
  pngSha256: hash(pngBytes),
  geometrySvgSha256: hash(svgBytes),
};
await writeFile(resolve(reviewOutput, "manifest.json"), `${JSON.stringify(manifest, null, 2)}\n`, "utf8");
console.log(
  `Captured actual production shell in Chromium: workscreen ${audit.wide.rectangles.workscreen.width}px × ${audit.wide.rectangles.workscreen.height}px, ` +
  `${(audit.wide.fieldFocusShare * 100).toFixed(1)}% of layout; 320px overflow ${audit.narrow.document.scrollWidth - audit.narrow.document.clientWidth}px.`,
);
