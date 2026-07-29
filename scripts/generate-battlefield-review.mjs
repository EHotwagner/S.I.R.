import { execFileSync } from "node:child_process";
import { createHash } from "node:crypto";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { resolve } from "node:path";
import { pathToFileURL } from "node:url";
import { Window } from "happy-dom";

const clientOutput = resolve("artifacts/client");
const reviewOutput = resolve("docs/assets/svg-player-review");
const html = await readFile(resolve(clientOutput, "index.html"), "utf8");
const scriptMatch = html.match(/<script[^>]+src="([^"]+\.js)"/);

if (!scriptMatch) {
  throw new Error("Build the production client before generating review boards.");
}

const window = new Window({ url: "https://sir.invalid/review/" });
window.document.body.innerHTML = '<div id="sir-replay-app"></div>';

class ReviewWorker {
  postMessage() {}
  terminate() {}
}

Object.assign(globalThis, {
  window,
  document: window.document,
  Node: window.Node,
  Element: window.Element,
  HTMLElement: window.HTMLElement,
  Event: window.Event,
  KeyboardEvent: window.KeyboardEvent,
  MouseEvent: window.MouseEvent,
  Worker: ReviewWorker,
});

const bundle = resolve(
  clientOutput,
  scriptMatch[1].replace(/^\.\//, ""),
);
const productionBundleSha256 = createHash("sha256")
  .update(await readFile(bundle))
  .digest("hex");
await import(pathToFileURL(bundle));
await window.happyDOM.waitUntilComplete();
await mkdir(reviewOutput, { recursive: true });

const palettes = [
  "accessible-default",
  "high-contrast",
  "monochrome-pattern",
];
const files = [];

for (const paletteId of palettes) {
  const select = window.document.querySelector("#battlefield-palette");
  select.value = paletteId;
  select.dispatchEvent(new window.Event("change", { bubbles: true }));
  await window.happyDOM.waitUntilComplete();

  const svg = window.document.querySelector(
    'svg[role="application"][aria-label*="exact tick 24"]',
  );
  if (!svg || select.value !== paletteId) {
    throw new Error(`The ${paletteId} production battlefield did not render.`);
  }

  svg.setAttribute("xmlns", "http://www.w3.org/2000/svg");
  svg.setAttribute("width", "768");
  svg.setAttribute("height", "768");
  svg.setAttribute("data-review-palette", paletteId);
  svg.setAttribute("style", "background:#0b100f");

  const svgName = `${paletteId}.svg`;
  const pngName = `${paletteId}.png`;
  const svgPath = resolve(reviewOutput, svgName);
  const pngPath = resolve(reviewOutput, pngName);
  const serialized = `<?xml version="1.0" encoding="UTF-8"?>\n${svg.outerHTML}\n`;

  await writeFile(svgPath, serialized, "utf8");
  execFileSync(
    "/usr/sbin/rsvg-convert",
    ["--width", "768", "--height", "768", "--output", pngPath, svgPath],
    { stdio: "inherit" },
  );

  const svgBytes = await readFile(svgPath);
  const pngBytes = await readFile(pngPath);
  files.push({
    palette: paletteId,
    svg: svgName,
    png: pngName,
    svgSha256: createHash("sha256").update(svgBytes).digest("hex"),
    pngSha256: createHash("sha256").update(pngBytes).digest("hex"),
  });
}

const manifest = {
  format: "sir-svg-player-review-v1",
  renderer: "SIR.Client.Web production Fable bundle",
  productionBundleSha256,
  rasterizer: "rsvg-convert 2.62.3",
  committedTick: 24,
  board: {
    minimumColumn: 0,
    minimumRow: 0,
    maximumColumn: 5,
    maximumRow: 5,
  },
  viewport: { width: 768, height: 768 },
  projectedCellPixels: 48,
  semanticZoom: {
    thresholdsPixels: [24, 48],
    hysteresisPercent: 10,
    capturedTier: "Detailed",
    note: "The initial detailed tier is retained at exactly 48 px by the hysteresis dead band.",
  },
  interpolation: false,
  phase3Review: {
    selectedExactOverlay: {
      id: "selected-los-1",
      geometryRevision: 1,
      pathSegments: 3,
      disposition: "exact",
    },
    secondaryHeadings: [
      { unitId: 1, source: "weapon", relationship: "offset from body facing" },
      { unitId: 2, source: "sensor", relationship: "opposed to body facing" },
    ],
    actionTraceCount: 2,
    semanticTimelineLanes: 3,
    wholeForceSegmentLimit: 8000,
    selectedOverlayWarningSegmentLimit: 2000,
  },
  phase4Review: {
    comparisonDefault: "linked split",
    persistentLabels: [
      "Immutable baseline — exploratory simulation",
      "Derived fork — exploratory simulation, not verified replay",
    ],
    linkedState: ["camera", "selection", "tick", "overlays"],
    inspection: ["first divergent event", "first differing disclosed field", "metric deltas"],
    bookmarks: true,
    evidenceExport: {
      svgRenderer: "sir-safe-svg-renderer-v1",
      pngSource: "the sanitized SVG evidence snapshot",
      requiredProvenance: [
        "source",
        "replay",
        "projection",
        "engine",
        "ruleset",
        "tick",
        "mode",
        "palette",
        "renderer",
      ],
      forbiddenReplayPayloads: [
        "paths",
        "styles",
        "ids",
        "scripts",
        "event handlers",
        "foreignObject",
        "external references",
        "URLs",
      ],
    },
  },
  files,
};

await writeFile(
  resolve(reviewOutput, "manifest.json"),
  `${JSON.stringify(manifest, null, 2)}\n`,
  "utf8",
);

console.log(
  `Generated ${files.length} deterministic SVG/PNG palette review pairs at tick 24.`,
);
window.happyDOM.close();
