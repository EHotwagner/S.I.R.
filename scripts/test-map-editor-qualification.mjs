import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { pathToFileURL } from "node:url";
import { Window } from "happy-dom";

const clientOutput = resolve(process.argv[2] ?? "artifacts/client");
const html = await readFile(resolve(clientOutput, "index.html"), "utf8");
const scriptMatch = html.match(/<script[^>]+src="([^"]+\.js)"/);
const stylesMatch = html.match(/<link[^>]+href="([^"]+\.css)"/);
if (!scriptMatch || !stylesMatch) {
  throw new Error("The production client assets are missing.");
}
const styles = await readFile(
  resolve(clientOutput, stylesMatch[1].replace(/^\.\//, "")),
  "utf8",
);
const window = new Window({ url: "https://sir.invalid/qualification/" });
window.document.body.innerHTML = '<div id="sir-replay-app"></div>';
class QualificationWorker {
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
  Worker: QualificationWorker,
});
await import(
  pathToFileURL(resolve(clientOutput, scriptMatch[1].replace(/^\.\//, "")))
);
await window.happyDOM.waitUntilComplete();

const failures = [];
const require = (condition, message) => {
  if (!condition) failures.push(message);
};
const buttonByText = (text, root = window.document) =>
  [...root.querySelectorAll("button")].find(
    (button) => button.textContent.trim() === text,
  );
const worksurface = window.document.querySelector(
  'svg#persistent-tactical-svg[role="application"]',
);
require(Boolean(worksurface?.querySelector("title")), "workspace title is missing");
require(Boolean(worksurface?.querySelector("desc")), "workspace description is missing");
require(worksurface?.getAttribute("tabindex") === "0", "workspace is not one keyboard stop");
require(
  worksurface?.querySelectorAll(
    '#persistent-layer-units [data-unit-id][role="button"]',
  ).length === 4,
  "shared Editor projection does not expose four named SVG unit buttons",
);
require(
  worksurface?.querySelectorAll(
    '#persistent-layer-units [data-unit-id][tabindex="0"]',
  ).length === 1,
  "shared SVG unit focus is not roving",
);
require(
  window.document.querySelectorAll("[role='application']").length === 1 &&
    window.document.querySelectorAll("[data-work-surface-root]").length === 1,
  "Editor mounted more than one application or workscreen root",
);
for (const selector of [
  "#editor-map-stage",
  ".editor-map-stage",
  '[aria-label="SVG tactical map workspace"]',
  '[aria-label="Editable map grid"]',
  '[aria-label="Map object list fallback"]',
]) {
  require(
    !window.document.querySelector(selector),
    `legacy Editor root remains connected: ${selector}`,
  );
}
require(
  [...["camera", "terrain", "edges", "routes", "units", "selection", "annotations"]].every(
    (layer) => worksurface?.querySelector(`[data-scene-layer="${layer}"]`),
  ),
  "persistent Editor projection is missing a stable semantic layer",
);
require(
  Boolean(window.document.querySelector('[aria-live="polite"]')),
  "editor changes have no polite announcement channel",
);

const modalToggle = window.document.querySelector("#tactical-input-toggle");
require(
  modalToggle?.getAttribute("aria-expanded") === "false" &&
    !window.document.querySelector("#tactical-input-panel"),
  "possible inputs are not collapsed by default",
);
const editable = window.document.createElement("div");
editable.contentEditable = "true";
editable.textContent = "Editable qualification target";
window.document.body.append(editable);
editable.focus();
editable.dispatchEvent(
  new window.KeyboardEvent("keydown", {
    key: "?",
    shiftKey: true,
    bubbles: true,
  }),
);
await window.happyDOM.waitUntilComplete();
require(
  modalToggle?.getAttribute("aria-expanded") === "false" &&
    !window.document.querySelector("#tactical-input-panel"),
  "content-editable keydown entered modal dispatch",
);
editable.remove();
modalToggle?.focus();
modalToggle?.click();
await window.happyDOM.waitUntilComplete();
require(
  modalToggle?.getAttribute("aria-expanded") === "true" &&
    window.document.querySelector("#tactical-input-panel")?.getAttribute("tabindex") ===
      "-1" &&
    window.document.activeElement?.id === "tactical-input-toggle",
  "pointer-opened possible inputs changed focus or lost its focus target",
);
require(
  Boolean(
    window.document.querySelector('[data-modal-command="editor.panel.toggle"]'),
  ) &&
    Boolean(
      window.document.querySelector('[data-modal-command="editor.inspector.toggle"]'),
    ),
  "the live Editor command catalog omitted its owner-specific migration commands",
);
const modalClose = window.document.querySelector(
  "#tactical-input-panel button:last-child",
);
modalClose?.focus();
modalClose?.click();
await new Promise((done) => setTimeout(done, 0));
await window.happyDOM.waitUntilComplete();
require(
  modalToggle?.getAttribute("aria-expanded") === "false" &&
    window.document.activeElement?.id === "tactical-input-toggle",
  "help close did not collapse and restore disclosure focus",
);

const modalRegion = () =>
  window.document.querySelector('[aria-label="Current input mode"]');
const sendEditorKey = async (key, options = {}) => {
  worksurface.dispatchEvent(
    new window.KeyboardEvent(options.phase ?? "keydown", {
      key,
      bubbles: true,
      shiftKey: options.shiftKey ?? false,
      ctrlKey: options.ctrlKey ?? false,
      repeat: options.repeat ?? false,
    }),
  );
  await window.happyDOM.waitUntilComplete();
};
require(
  modalRegion()?.textContent.includes("EDITOR / SELECT"),
  "the mounted Editor controls do not project the live Select mode",
);
await sendEditorKey("t");
require(
  Boolean(window.document.querySelector('[aria-label="Terrain palette"]')) &&
    modalRegion()?.textContent.includes("EDITOR / TERRAIN / PENCIL"),
  "keyboard T did not expose the terrain controls and enter Pencil mode",
);
await sendEditorKey("2");
await sendEditorKey("]");
require(
  modalRegion()?.textContent.includes("Rough terrain") &&
    modalRegion()?.textContent.includes("2×2 brush"),
  "terrain value and brush alternatives are not projected",
);
const revisionBeforePencil = worksurface.getAttribute("data-scene-revision");
await sendEditorKey("Enter");
const revisionAfterPencil = worksurface.getAttribute("data-scene-revision");
require(
  revisionAfterPencil && revisionAfterPencil !== revisionBeforePencil,
  "keyboard Pencil did not commit an authored terrain revision",
);
await sendEditorKey("r");
await sendEditorKey("Enter");
await sendEditorKey("ArrowRight");
await sendEditorKey("ArrowDown", { shiftKey: true });
await sendEditorKey("Backspace");
require(
  modalRegion()?.textContent.includes("anchor A1, endpoint A1"),
  "terrain rectangle Backspace did not reset its preview endpoint",
);
await sendEditorKey("ArrowRight");
await sendEditorKey("Enter");
require(
  !modalRegion()?.textContent.includes("anchor"),
  "terrain rectangle did not commit atomically and exit its preview",
);
const rectangleRevision = worksurface.getAttribute("data-scene-revision");
await sendEditorKey("z", { ctrlKey: true });
const rectangleUndoRevision = worksurface.getAttribute("data-scene-revision");
await sendEditorKey("y", { ctrlKey: true });
require(
  rectangleUndoRevision !== rectangleRevision &&
    worksurface.getAttribute("data-scene-revision") === rectangleRevision,
  "terrain rectangle did not undo and redo as one exact authored revision",
);

await sendEditorKey("e");
require(
  modalRegion()?.textContent.includes("EDITOR / EDGES"),
  "keyboard E did not enter edge authoring",
);
const edgeCountBefore = worksurface.querySelectorAll(
  "#persistent-layer-edges line",
).length;
const edgeRevisionBefore = worksurface.getAttribute("data-scene-revision");
for (let index = 0; index < 6; index += 1) {
  await sendEditorKey("ArrowRight");
  await sendEditorKey("ArrowDown");
}
await sendEditorKey("d");
const edgeCountAfter = worksurface.querySelectorAll(
  "#persistent-layer-edges line",
).length;
const edgeRevisionAfter = worksurface.getAttribute("data-scene-revision");
require(
  edgeCountAfter === edgeCountBefore + 1 &&
    edgeRevisionAfter !== edgeRevisionBefore,
  `edge kind command did not commit one edge into the persistent edges layer (${edgeCountBefore} -> ${edgeCountAfter}; revision changed ${edgeRevisionAfter !== edgeRevisionBefore}; ${modalRegion()?.textContent})`,
);
await sendEditorKey("z", { ctrlKey: true });
require(
  worksurface.querySelectorAll("#persistent-layer-edges line").length ===
    edgeCountBefore,
  "edge commit was not one undoable transaction",
);
await sendEditorKey("y", { ctrlKey: true });
require(
  worksurface.querySelectorAll("#persistent-layer-edges line").length ===
    edgeCountAfter &&
    worksurface.getAttribute("data-scene-revision") === edgeRevisionAfter,
  "edge redo did not restore the exact authored revision",
);
await sendEditorKey("Escape");

await sendEditorKey("z");
require(
  modalRegion()?.textContent.includes("EDITOR / ZONES"),
  "keyboard Z did not enter region authoring",
);
const regionCountBefore = worksurface.querySelectorAll(
  '#persistent-layer-annotations [data-primitive-id^="region:"]',
).length;
await sendEditorKey("n");
await sendEditorKey("o");
await sendEditorKey("r");
await sendEditorKey("Enter");
await sendEditorKey("ArrowRight");
await sendEditorKey("ArrowDown");
await sendEditorKey("Enter");
const regionCountAfter = worksurface.querySelectorAll(
  '#persistent-layer-annotations [data-primitive-id^="region:"]',
).length;
const regionRevisionAfter = worksurface.getAttribute("data-scene-revision");
require(
  regionCountAfter === regionCountBefore + 1,
  "region rectangle workflow did not commit one persistent annotation",
);
await sendEditorKey("z", { ctrlKey: true });
require(
  worksurface.querySelectorAll(
    '#persistent-layer-annotations [data-primitive-id^="region:"]',
  ).length === regionCountBefore,
  "region commit was not one undoable transaction",
);
await sendEditorKey("y", { ctrlKey: true });
require(
  worksurface.querySelectorAll(
    '#persistent-layer-annotations [data-primitive-id^="region:"]',
  ).length === regionCountAfter &&
    worksurface.getAttribute("data-scene-revision") === regionRevisionAfter,
  "region redo did not restore the exact authored revision",
);
await sendEditorKey("Escape");
await sendEditorKey("Escape");

await sendEditorKey("t");
await sendEditorKey("v");
for (let index = 0; index < 12; index += 1) {
  await sendEditorKey("ArrowLeft");
  await sendEditorKey("ArrowUp");
}
await sendEditorKey("ArrowRight");
await sendEditorKey("ArrowDown");
require(
  modalRegion()?.textContent.includes("EDITOR / SELECT") &&
    modalRegion()?.textContent.includes("Cursor B2"),
  `Select cursor movement is not deterministic (${modalRegion()?.textContent})`,
);
await sendEditorKey("Enter");
await sendEditorKey("Enter");
require(
  modalRegion()?.textContent.includes("EDITOR / SELECT / ACTIONS") &&
    modalRegion()?.textContent.includes("1 unit selected") &&
    worksurface.getAttribute("data-semantic-selection-unit") === "1",
  `Select Enter did not select exact topmost unit 1 and expose its action mode (${modalRegion()?.textContent}; semantic ${worksurface.getAttribute("data-semantic-selection-unit")})`,
);
await sendEditorKey(" ");
require(
  modalRegion()?.textContent.includes("EDITOR / PAN HELD"),
  "Space key-down did not enter Pan Held",
);
await sendEditorKey("p");
require(
  modalRegion()?.textContent.includes("EDITOR / PAN HELD"),
  "an authoring command leaked through Pan Held",
);
await sendEditorKey(" ", { phase: "keyup" });
require(
  modalRegion()?.textContent.includes("EDITOR / SELECT"),
  `Space key-up did not restore Select mode (${modalRegion()?.textContent})`,
);

const cameraBefore = Number(worksurface.getAttribute("data-camera-zoom"));
buttonByText(
  "+",
  window.document.querySelector('[aria-label="Map editor quick access"]'),
)?.click();
await window.happyDOM.waitUntilComplete();
require(
  Number(worksurface.getAttribute("data-camera-zoom")) > cameraBefore,
  "the reachable Editor zoom control did not update the retained camera",
);
buttonByText(
  "Fit",
  window.document.querySelector('[aria-label="Map editor quick access"]'),
)?.click();
await window.happyDOM.waitUntilComplete();

buttonByText("Map file")?.click();
await window.happyDOM.waitUntilComplete();
require(
  Boolean(window.document.querySelector('[aria-label="Editing layer states"]')),
  "editing layer states are not reachable beside the persistent workscreen",
);
require(
  Boolean(window.document.querySelector('[aria-label="Map validation issues"]')),
  "validation issues have no reachable HTML authority",
);
require(
  Boolean(window.document.querySelector('label[for="map-name"]')),
  "map authoring metadata has no explicit label",
);
const mapName = window.document.querySelector("#map-name");
const documentModeBefore = modalRegion()?.textContent;
const mapNameKey = new window.KeyboardEvent("keydown", {
  key: "t",
  bubbles: true,
  cancelable: true,
});
mapName?.dispatchEvent(mapNameKey);
await window.happyDOM.waitUntilComplete();
require(
  !mapNameKey.defaultPrevented &&
    modalRegion()?.textContent === documentModeBefore,
  "native map metadata input leaked into tactical keyboard dispatch",
);
buttonByText("Clear")?.click();
await window.happyDOM.waitUntilComplete();
require(
  Boolean(
    [...window.document.querySelectorAll('[role="alertdialog"]')].find(
      (dialog) => dialog.textContent.includes("Confirmation required"),
    ),
  ),
  "destructive map commands do not expose their confirmation workflow",
);
buttonByText("Cancel")?.click();
await window.happyDOM.waitUntilComplete();

for (const control of window.document.querySelectorAll(
  "#sir-replay-app button, #sir-replay-app input, #sir-replay-app select",
)) {
  const id = control.getAttribute("id");
  require(
    Boolean(
      control.getAttribute("aria-label") ||
        control.textContent.trim() ||
        (id && window.document.querySelector(`label[for="${id}"]`)),
    ),
    `${control.tagName.toLowerCase()} ${id ?? "(without id)"} has no accessible name`,
  );
}
require(styles.includes("touch-action:none"), "touch-action protection is missing");
require(
  /@media\s*\(forced-colors:?active\)/.test(styles) &&
    styles.includes("forced-color-adjust:auto"),
  "forced-colors support is missing",
);
require(
  /@media\s*\(prefers-reduced-motion:?reduce\)/.test(styles) &&
    styles.includes("animation-duration:.01ms!important"),
  "reduced-motion override is missing",
);
require(
  /@media\s*\((max-width:)?48rem|width<=48rem\)/.test(styles) &&
    /\.modal-input-panel\{[^}]*position:static/.test(styles),
  "the 400% zoom/narrow-layout collapse is missing",
);
require(
  styles.includes("min-width:2.75rem") || styles.includes("min-height:2.75rem"),
  "44 CSS pixel target sizing is not represented",
);

const reviewRoot = resolve("docs/assets/map-editor-review");
const manifest = JSON.parse(
  await readFile(resolve(reviewRoot, "manifest.json"), "utf8"),
);
const currentBundle = await readFile(
  resolve(clientOutput, scriptMatch[1].replace(/^\.\//, "")),
);
require(
  createHash("sha256").update(currentBundle).digest("hex") ===
    manifest.productionBundleSha256,
  "review artifacts were not regenerated from the current production bundle",
);
require(manifest.files.length === 7, "review manifest does not cover seven domains");
for (const file of manifest.files) {
  const svg = await readFile(resolve(reviewRoot, file.svg));
  const png = await readFile(resolve(reviewRoot, file.png));
  require(
    createHash("sha256").update(svg).digest("hex") === file.svgSha256,
    `${file.domain} SVG hash drifted`,
  );
  require(
    createHash("sha256").update(png).digest("hex") === file.pngSha256,
    `${file.domain} PNG hash drifted`,
  );
}

if (failures.length > 0) {
  throw new Error(`Map-editor qualification failed: ${failures.join("; ")}.`);
}
console.log(
  "Map-editor automated qualification passed at the M3 boundary: one accessible persistent application workscreen, atomic terrain/edge/region editing with undo-redo, selection/Pan and native-input boundaries, shared camera controls, modal help/focus behavior, reachable layers/validation/document/destructive workflows, no connected legacy Editor roots, accessibility CSS, and seven domain-truthful hashed review boards.",
);
window.happyDOM.close();
