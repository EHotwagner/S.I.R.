import { execFileSync } from "node:child_process";
import { createHash } from "node:crypto";
import { mkdir, readFile, writeFile } from "node:fs/promises";
import { resolve } from "node:path";
import { pathToFileURL } from "node:url";
import { Window } from "happy-dom";

const clientOutput = resolve("artifacts/client");
const reviewOutput = resolve("docs/assets/map-editor-review");
const html = await readFile(resolve(clientOutput, "index.html"), "utf8");
const scriptMatch = html.match(/<script[^>]+src="([^"]+\.js)"/);

if (!scriptMatch) {
  throw new Error("Build the production client before generating map-editor review boards.");
}

const window = new Window({ url: "https://sir.invalid/map-editor-review/" });
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

const bundle = resolve(clientOutput, scriptMatch[1].replace(/^\.\//, ""));
const productionBundleSha256 = createHash("sha256")
  .update(await readFile(bundle))
  .digest("hex");
await import(pathToFileURL(bundle));
await window.happyDOM.waitUntilComplete();
await mkdir(reviewOutput, { recursive: true });

const buttonByText = (text) =>
  [...window.document.querySelectorAll("button")].find(
    (button) => button.textContent.trim() === text,
  );
const workspace = () =>
  window.document.querySelector(
    '[aria-label="SVG tactical map workspace"] svg[role="application"]',
  );
const setFile = (input, file) => {
  Object.defineProperty(input, "files", {
    configurable: true,
    value: {
      0: file,
      length: 1,
      item(index) {
        return index === 0 ? file : null;
      },
    },
  });
  input.dispatchEvent(new window.Event("change", { bubbles: true }));
};
const settleFile = async () => {
  await new Promise((done) => setTimeout(done, 0));
  await window.happyDOM.waitUntilComplete();
};

buttonByText("Map file")?.click();
await window.happyDOM.waitUntilComplete();

const validMap = `SIR-MAP 2
size 12 8
terrain 2 2 rough
terrain 3 2 rough
terrain 2 3 objective
terrain 3 3 blocked
edge 4 1 east wall closed
edge 4 2 east door open
edge 4 3 east window closed
zone 1 objective rectangle 6 2 3 2
zone 2 deployment blue polygon 0,5 4,5 4,8 0,8
unit 1 blue rifleman 0 0 2 12 12 manual -
unit 2 red goblin 8 1 1 35 35 general -
unit 3 red troll 8 4 3 240 240 general -
`;
const importInput = window.document.querySelector(
  'input[aria-label="Import SIR map"]',
);
setFile(
  importInput,
  new window.File([validMap], "qualification.sir-map", {
    type: "text/plain",
  }),
);
await settleFile();

const png = Buffer.from(
  "iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAYAAABytg0kAAAAFElEQVR42mNkYGD4z8DAwMDEAAUADikBA8NqN1EAAAAASUVORK5CYII=",
  "base64",
);
const backgroundInput = window.document.querySelector(
  'input[aria-label="Choose local raster map background"]',
);
setFile(
  backgroundInput,
  new window.File([png], "qualification.png", { type: "image/png" }),
);
await settleFile();

const files = [];
const hash = (bytes) => createHash("sha256").update(bytes).digest("hex");
const writeBoard = async (name, source, keepLayers, description) => {
  if (!source) throw new Error(`The ${name} production review SVG did not render.`);
  const svg = source.cloneNode(true);
  svg.setAttribute("xmlns", "http://www.w3.org/2000/svg");
  svg.setAttribute("width", "960");
  svg.setAttribute("height", "640");
  svg.setAttribute("data-map-editor-review", name);
  svg.setAttribute("style", "background:#0b100f");
  if (keepLayers) {
    for (const layer of svg.querySelectorAll("[data-layer]")) {
      if (!keepLayers.includes(layer.getAttribute("data-layer"))) {
        layer.setAttribute("display", "none");
      }
    }
  }
  const svgName = `${name}.svg`;
  const pngName = `${name}.png`;
  const svgPath = resolve(reviewOutput, svgName);
  const pngPath = resolve(reviewOutput, pngName);
  await writeFile(
    svgPath,
    `<?xml version="1.0" encoding="UTF-8"?>\n${svg.outerHTML}\n`,
    "utf8",
  );
  execFileSync(
    "/usr/sbin/rsvg-convert",
    ["--width", "960", "--height", "640", "--output", pngPath, svgPath],
    { stdio: "inherit" },
  );
  const svgBytes = await readFile(svgPath);
  const pngBytes = await readFile(pngPath);
  files.push({
    domain: name,
    description,
    svg: svgName,
    png: pngName,
    svgSha256: hash(svgBytes),
    pngSha256: hash(pngBytes),
  });
};

await writeBoard(
  "terrain",
  workspace(),
  ["grid", "terrain"],
  "Open, rough, objective, and blocked terrain with grid context.",
);
await writeBoard(
  "edges",
  workspace(),
  ["grid", "edges"],
  "Wall, open door, and window semantic edge meanings.",
);
await writeBoard(
  "units",
  workspace(),
  ["grid", "units"],
  "Canonical 1×1, 2×2, and 3×3 square unit footprints.",
);
await writeBoard(
  "zones",
  workspace(),
  ["grid", "regions"],
  "Objective rectangle and blue deployment polygon.",
);
await writeBoard(
  "background",
  workspace(),
  ["local-raster-background", "grid"],
  "Signature-validated local raster alignment beneath the grid.",
);

buttonByText("Simulate this revision")?.click();
await window.happyDOM.waitUntilComplete();
await writeBoard(
  "simulator-handoff",
  window.document.querySelector(
    '[aria-label="Editable simulation SVG battlefield"] svg[role="application"]',
  ),
  null,
  "Immutable authored revision rendered by the simulator.",
);

buttonByText("Editor")?.click();
await window.happyDOM.waitUntilComplete();
buttonByText("Map file")?.click();
await window.happyDOM.waitUntilComplete();
const gapMap = validMap.replace(
  "edge 4 2 east door open",
  "edge 4 4 east door open",
);
setFile(
  window.document.querySelector('input[aria-label="Import SIR map"]'),
  new window.File([gapMap], "validation.sir-map", { type: "text/plain" }),
);
await settleFile();
await writeBoard(
  "validation",
  workspace(),
  ["grid", "terrain", "edges", "units", "regions", "validation-overlay"],
  "EDGE-GAP issue projected without hiding authoritative domains.",
);

const manifest = {
  format: "sir-map-editor-review-v1",
  generatedFrom: "production Fable/React SVG workspaces",
  productionBundleSha256,
  rasterizer: "rsvg-convert 2.62.3",
  width: 960,
  height: 640,
  files,
};
await writeFile(
  resolve(reviewOutput, "manifest.json"),
  `${JSON.stringify(manifest, null, 2)}\n`,
  "utf8",
);
await writeFile(
  resolve(reviewOutput, "README.md"),
  `# Map editor review boards

These deterministic SVG/PNG pairs are generated from the production Fable/React
editor and simulator by \`node scripts/generate-map-editor-review.mjs\`.
The manifest pins the production bundle and every artifact hash. The boards are
presentation evidence only; the canonical SIR-MAP document remains authoritative.

Domains: terrain, semantic edges, units, zones, local background, validation,
and immutable simulator handoff.
`,
  "utf8",
);

console.log(`Generated ${files.length} deterministic map-editor SVG/PNG review pairs.`);
window.happyDOM.close();
