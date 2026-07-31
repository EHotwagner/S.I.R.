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
  throw new Error("Build the production client before generating review boards.");
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

const worksurface = () =>
  window.document.querySelector("svg#persistent-tactical-svg");
const buttonByText = (text, root = window.document) =>
  [...root.querySelectorAll("button")].find(
    (button) => button.textContent.trim() === text,
  );
const setFile = (input, file) => {
  if (!input) throw new Error(`Required review input for ${file.name} is unavailable.`);
  Object.defineProperty(input, "files", {
    configurable: true,
    value: {
      0: file,
      length: 1,
      item: (index) => (index === 0 ? file : null),
      [Symbol.iterator]: function* () {
        yield file;
      },
    },
  });
  input.dispatchEvent(new window.Event("change", { bubbles: true }));
};
const settleFile = async () => {
  await new Promise((done) => setTimeout(done, 20));
  await window.happyDOM.waitUntilComplete();
};
const ensureDocumentPanel = async () => {
  let validation = window.document.querySelector(
    '[aria-label="Map validation issues"]',
  );
  if (!validation) {
    buttonByText(
      "Map file",
      window.document.querySelector('[aria-label="Map editor tool groups"]'),
    )?.click();
    await window.happyDOM.waitUntilComplete();
    validation = window.document.querySelector(
      '[aria-label="Map validation issues"]',
    );
  }
  if (!validation) {
    throw new Error("Map file controls did not expose the validation region.");
  }
  return validation;
};
const files = [];
const hash = (bytes) => createHash("sha256").update(bytes).digest("hex");
const writeBoard = async (name, keepLayers, description, evidenceText = "") => {
  const source = worksurface();
  if (!source) throw new Error(`The ${name} persistent review SVG did not render.`);
  const svg = source.cloneNode(true);
  svg.setAttribute("xmlns", "http://www.w3.org/2000/svg");
  svg.setAttribute("width", "960");
  svg.setAttribute("height", "640");
  svg.setAttribute("data-map-editor-review", name);
  svg.setAttribute("style", "background:#0b100f");
  for (const layer of svg.querySelectorAll(
    "#persistent-scene-camera > [data-scene-layer]",
  )) {
    if (!keepLayers.includes(layer.getAttribute("data-scene-layer"))) {
      layer.setAttribute("display", "none");
    }
  }
  if (evidenceText) {
    const note = window.document.createElementNS(
      "http://www.w3.org/2000/svg",
      "text",
    );
    note.setAttribute("x", "12");
    note.setAttribute("y", "500");
    note.setAttribute("fill", "#ffd166");
    note.setAttribute("font-size", "14");
    note.setAttribute("data-evidence-control-state", name);
    note.textContent = evidenceText.replace(/\s+/g, " ").trim().slice(0, 180);
    svg.append(note);
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

await ensureDocumentPanel();
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
unit 2 blue medic 3 0 1 12 12 manual -
unit 3 red goblin 8 1 1 35 35 general -
unit 4 red troll 8 4 3 240 240 general -
`;
setFile(
  window.document.querySelector('input[aria-label="Import SIR map"]'),
  new window.File([validMap], "qualification.sir-map", {
    type: "text/plain",
  }),
);
await settleFile();

await writeBoard(
  "terrain",
  ["terrain"],
  "Shared authored terrain projected through the persistent M3 renderer.",
);
await writeBoard(
  "edges",
  ["edges"],
  "Shared semantic edges projected through the persistent M3 renderer.",
);
await writeBoard(
  "units",
  ["units"],
  "Canonical square-unit footprints in the shared Editor projection.",
);
await writeBoard(
  "zones",
  ["annotations"],
  "Imported objective and deployment regions projected as positioned annotations.",
);

const png = Buffer.from(
  "iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAYAAABytg0kAAAAFElEQVR42mNkYGD4z8DAwMDEAAUADikBA8NqN1EAAAAASUVORK5CYII=",
  "base64",
);
setFile(
  window.document.querySelector(
    'input[aria-label="Choose local raster map background"]',
  ),
  new window.File([png], "qualification.png", { type: "image/png" }),
);
await settleFile();
const backgroundState =
  window.document.querySelector('[aria-label="Editor non-workscreen controls"]')
    ?.textContent ?? "";
await writeBoard(
  "background",
  ["terrain", "annotations"],
  "Signature-validated local background control state beside the retained authored projection; raster drawing migrates in M4.",
  backgroundState,
);

const unit = worksurface()?.querySelector(
  '[data-unit-id="2"][data-command-available="true"]',
);
unit?.dispatchEvent(
  new window.MouseEvent("click", { bubbles: true, cancelable: true }),
);
await window.happyDOM.waitUntilComplete();
[...window.document.querySelectorAll(
  '[aria-label="Map editor quick access"] button',
)]
  .find((button) => button.textContent.trim() === "Simulate")
  ?.click();
await window.happyDOM.waitUntilComplete();
if (worksurface()?.getAttribute("data-scene-owner") !== "SimulatorScene") {
  throw new Error("The registry-routed simulator handoff did not project.");
}
await writeBoard(
  "simulator-handoff",
  ["terrain", "edges", "routes", "units", "selection", "annotations"],
  "Immutable simulator handoff projected into the same retained SVG.",
);

buttonByText("Editor")?.click();
await window.happyDOM.waitUntilComplete();
await ensureDocumentPanel();
const gapMap = validMap.replace(
  "edge 4 2 east door open",
  "edge 4 4 east door open",
);
setFile(
  window.document.querySelector('input[aria-label="Import SIR map"]'),
  new window.File([gapMap], "validation.sir-map", { type: "text/plain" }),
);
await settleFile();
const validationRegion = await ensureDocumentPanel();
const validationState = validationRegion.textContent;
if (!validationState.includes("EDGE-GAP")) {
  throw new Error(
    `Expected EDGE-GAP validation issue was absent: ${validationState}`,
  );
}
await writeBoard(
  "validation",
  ["terrain", "edges", "units", "selection", "annotations"],
  "Imported EDGE-GAP validation state shown with the authoritative persistent projection.",
  validationState,
);

const manifest = {
  format: "sir-map-editor-review-v1",
  generatedFrom: "production persistent Fable/React SVG workscreen at the M3 boundary",
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
  `# Persistent tactical Editor review boards

These deterministic SVG/PNG pairs are generated from the production retained
Fable/React workscreen by \`node scripts/generate-map-editor-review.mjs\`.
The manifest pins the production bundle and every artifact hash. At Milestone 3
the boards prove the renderer boundary and shared layers; full Editor parity is
assigned to Milestone 4.

Domains: imported terrain, semantic edges, units, positioned regions,
signature-validated background control state, imported validation state, and
immutable simulator handoff.
`,
  "utf8",
);

console.log(`Generated ${files.length} deterministic persistent SVG/PNG review pairs.`);
window.happyDOM.close();
