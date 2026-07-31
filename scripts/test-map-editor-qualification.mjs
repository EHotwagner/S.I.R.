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
const appSource = await readFile(resolve("src/SIR.Client.Web/App.fs"), "utf8");
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
window.Element.prototype.setPointerCapture = () => {};
window.Element.prototype.releasePointerCapture = () => {};
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
const setFile = (input, file) => {
  Object.defineProperty(input, "files", {
    configurable: true,
    value: { 0: file, length: 1, item: (index) => (index === 0 ? file : null) },
  });
  input.dispatchEvent(new window.Event("change", { bubbles: true }));
};
const settleFile = async () => {
  await new Promise((done) => setTimeout(done, 20));
  await window.happyDOM.waitUntilComplete();
};
const worksurface = window.document.querySelector(
  'svg#persistent-tactical-svg[role="application"]',
);
const stableSceneLayers = new Map(
  ["terrain", "edges", "routes", "units", "selection", "annotations"].map((layer) => [
    layer,
    worksurface?.querySelector(`[data-scene-layer="${layer}"]`),
  ]),
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
  Boolean(worksurface?.querySelector("#persistent-editor-background")) &&
    Boolean(worksurface?.querySelector("#persistent-editor-migrated-layers")) &&
    [...["guides", "regions", "cursor-guide"]].every(
      (layer) => worksurface?.querySelector(`[data-editor-layer="${layer}"]`),
    ),
  "persistent SVG does not own its stable Editor-specific visual layers",
);
require(
  !window.document.querySelector(".editor-owner-controls") &&
    !window.document.querySelector(".tactical-compatibility-surface [aria-label='Map editor tool groups']"),
  "Editor controls remain mounted in the compatibility surface",
);
for (const [panelId, ownedSelector] of [
  ["tools", "[aria-label='Map editor tool groups']"],
  ["layers", "#editor-layer-controls"],
  ["selection", "[aria-label='Selected unit properties']"],
  ["validation", "[aria-label='Map validation issues']"],
  ["document", "[aria-label='Map document controls']"],
]) {
  if (!window.document.querySelector(`[data-panel-id="${panelId}"]`)) {
    window.document.querySelector(`#layout-show-${panelId}`)?.click();
    await window.happyDOM.waitUntilComplete();
  }
  const panel = window.document.querySelector(`[data-panel-id="${panelId}"]`);
  if (panel?.classList.contains("is-collapsed")) {
    panel.querySelector(`#layout-panel-${panelId}-collapse`)?.click();
    await window.happyDOM.waitUntilComplete();
  }
  require(
    Boolean(window.document.querySelector(`#layout-panel-${panelId}-body ${ownedSelector}`)) &&
      !window.document.querySelector(`#layout-panel-${panelId}-body .tactical-layout-panel-placeholder`),
    `registered ${panelId} panel does not own its Editor capability`,
  );
}
for (const selector of [
  "#editor-layer-controls",
  "#map-name",
  '[aria-label="Map validation issues"]',
  '[aria-label="Map editor tool groups"]',
  '[aria-label="Map document controls"]',
  '[aria-label="Map editor document actions"]',
]) {
  require(
    window.document.querySelectorAll(selector).length === 1,
    `Editor capability is duplicated outside its registered panel: ${selector}`,
  );
}
require(
  !window.document.querySelector(".editor-desktop-chrome") &&
    !window.document.querySelector('[aria-label="Map editor quick access"]') &&
    !window.document.querySelector('[aria-label="Map editor menus"]'),
  "legacy document/menu/quick-action chrome remains below the tactical shell",
);
const layerChoice = (domain, label) => {
  const fieldset = [...window.document.querySelectorAll("#editor-layer-controls fieldset")]
    .find((candidate) => candidate.querySelector("legend")?.textContent.trim() === domain);
  return buttonByText(label, fieldset);
};
require(
  worksurface.querySelectorAll("#persistent-layer-units [data-unit-glyph]").length === 4 &&
    [...worksurface.querySelectorAll("#persistent-layer-units [data-unit-id]")].every((unit) =>
      unit.getAttribute("aria-label")?.includes(unit.getAttribute("data-unit-class")) &&
      unit.getAttribute("aria-label")?.includes(" by "),
    ),
  "canonical unit glyphs or class/footprint accessible names are missing",
);
layerChoice("TerrainDomain", "Dimmed")?.click();
await window.happyDOM.waitUntilComplete();
require(
  worksurface.querySelector("#persistent-layer-terrain")?.getAttribute("opacity") === "0.28",
  "Dimmed terrain layer did not affect the shared terrain projection",
);
layerChoice("TerrainDomain", "Visible")?.click();
await window.happyDOM.waitUntilComplete();
for (const token of [
  "let private editorBattlefield",
  "let private editorGrid",
  'svg.id "editor-map-stage"',
]) {
  require(!appSource.includes(token), `dead Editor renderer source remains: ${token}`);
}
for (const token of [".editor-battlefield-svg", ".editor-map-stage", ".editor-canvas", ".map-unit-symbol", ".map-cell"]) {
  require(!styles.includes(token), `dead Editor renderer CSS remains: ${token}`);
}
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
const sendPointer = async (target, type, options) => {
  const event = new window.MouseEvent(type, {
    bubbles: true,
    cancelable: true,
    button: options.button ?? 0,
    clientX: options.clientX,
    clientY: options.clientY,
    shiftKey: options.shiftKey ?? false,
  });
  Object.defineProperties(event, {
    pointerId: { value: options.pointerId ?? 41 },
    pointerType: { value: options.pointerType ?? "mouse" },
  });
  target.dispatchEvent(event);
  await window.happyDOM.waitUntilComplete();
};
worksurface.getBoundingClientRect = () => ({
  x: 0, y: 0, left: 0, top: 0, right: 960, bottom: 640,
  width: 960, height: 640, toJSON: () => ({}),
});
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
require(
  Boolean(worksurface.querySelector('[data-editor-layer="terrain-preview"]')),
  "terrain rectangle did not mount a live persistent preview layer",
);
layerChoice("TerrainDomain", "Dimmed")?.click();
await window.happyDOM.waitUntilComplete();
require(
  worksurface.querySelector('[data-editor-layer="terrain-preview"]')?.getAttribute("opacity") === "0.28",
  "terrain preview did not honor the Dimmed domain state",
);
layerChoice("TerrainDomain", "Hidden")?.click();
await window.happyDOM.waitUntilComplete();
require(
  worksurface.querySelector("#persistent-layer-terrain")?.getAttribute("display") === "none" &&
    worksurface.querySelector('[data-editor-layer="terrain-preview"]')?.getAttribute("display") === "none",
  "terrain and its active preview did not fail closed when the domain was hidden",
);
layerChoice("TerrainDomain", "Visible")?.click();
await window.happyDOM.waitUntilComplete();
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
buttonByText("East wall", window.document.querySelector("#layout-panel-tools-body"))?.click();
await window.happyDOM.waitUntilComplete();
await sendEditorKey("Enter");
require(
  Boolean(worksurface.querySelector('[data-editor-layer="edge-preview"] [data-edge-preview]')),
  "edge authoring did not mount a live persistent polyline preview",
);
layerChoice("EdgeDomain", "Dimmed")?.click();
await window.happyDOM.waitUntilComplete();
require(
  worksurface.querySelector('[data-editor-layer="edge-preview"]')?.getAttribute("opacity") === "0.28",
  "edge preview did not honor the Dimmed domain state",
);
layerChoice("EdgeDomain", "Hidden")?.click();
await window.happyDOM.waitUntilComplete();
require(
  worksurface.querySelector("#persistent-layer-edges")?.getAttribute("display") === "none" &&
    worksurface.querySelector('[data-editor-layer="edge-preview"]')?.getAttribute("display") === "none",
  "edge layer and preview did not fail closed when hidden",
);
layerChoice("EdgeDomain", "Visible")?.click();
await window.happyDOM.waitUntilComplete();
await sendEditorKey("Escape");
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
const regionNode = worksurface.querySelector(
  '[data-editor-layer="regions"] [data-region-id]',
);
regionNode?.dispatchEvent(new window.KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
await window.happyDOM.waitUntilComplete();
require(
  regionNode?.getAttribute("data-selected") === "false",
  "region Escape did not clear semantic selection",
);
regionNode?.dispatchEvent(new window.KeyboardEvent("keydown", { key: "Enter", bubbles: true }));
await window.happyDOM.waitUntilComplete();
require(
  regionNode?.isConnected && regionNode === worksurface.querySelector(`[data-region-id="${regionNode?.getAttribute("data-region-id")}"]`) &&
    regionNode?.getAttribute("data-selected") === "true",
  "region Enter did not select through a stable keyed semantic primitive",
);
regionNode?.dispatchEvent(new window.KeyboardEvent("keydown", { key: " ", bubbles: true }));
await window.happyDOM.waitUntilComplete();
require(regionNode?.getAttribute("data-selected") === "true", "region Space did not preserve selection");
const regionGeometryLayer = worksurface.querySelector('[data-editor-layer="regions"]');
const regionAnnotationLayer = worksurface.querySelector("#persistent-layer-annotations");
layerChoice("RegionDomain", "Dimmed")?.click();
await window.happyDOM.waitUntilComplete();
require(
  regionGeometryLayer?.getAttribute("opacity") === "0.28" &&
    regionAnnotationLayer?.getAttribute("opacity") === "0.28",
  "Dimmed region domain did not consistently affect region geometry and annotations",
);
layerChoice("RegionDomain", "Hidden")?.click();
await window.happyDOM.waitUntilComplete();
require(
  regionGeometryLayer?.getAttribute("display") === "none" &&
    regionAnnotationLayer?.getAttribute("display") === "none",
  "Hidden region domain did not fail closed for region geometry and annotations",
);
layerChoice("RegionDomain", "Visible")?.click();
await window.happyDOM.waitUntilComplete();
require(
  regionGeometryLayer?.getAttribute("display") === "inline" &&
    regionGeometryLayer?.getAttribute("opacity") === "1" &&
    regionAnnotationLayer?.getAttribute("display") === "inline" &&
    regionAnnotationLayer?.getAttribute("opacity") === "1",
  "Visible region domain did not restore region geometry and annotations",
);
await sendEditorKey("Escape");
await sendEditorKey("Escape");

await sendEditorKey("t");
await sendEditorKey("v");
let pointerUnit = worksurface.querySelector('#persistent-layer-units [data-unit-id="1"]');
pointerUnit?.dispatchEvent(new window.MouseEvent("click", { bubbles: true, cancelable: true }));
await window.happyDOM.waitUntilComplete();
const pointerRect = pointerUnit?.querySelector("rect");
const pointerOriginalX = Number(pointerRect?.getAttribute("x"));
const pointerStartX = Number(pointerRect?.getAttribute("x")) + 12;
const pointerStartY = Number(pointerRect?.getAttribute("y")) + 12;
const pointerRevision = worksurface.getAttribute("data-scene-revision");
await sendPointer(pointerUnit, "pointerdown", { clientX: pointerStartX, clientY: pointerStartY });
await sendPointer(worksurface, "pointermove", { clientX: pointerStartX + 64, clientY: pointerStartY });
require(
  Boolean(worksurface.querySelector('[data-editor-layer="placement-preview"] [data-preview-unit]')),
  "pointer drag did not project a real unit movement preview",
);
layerChoice("UnitDomain", "Dimmed")?.click();
await window.happyDOM.waitUntilComplete();
require(
  worksurface.querySelector("#persistent-layer-units")?.getAttribute("opacity") === "0.28" &&
    worksurface.querySelector('[data-editor-layer="placement-preview"]')?.getAttribute("opacity") === "0.28",
  "unit movement preview did not honor Dimmed layer state",
);
layerChoice("UnitDomain", "Hidden")?.click();
await window.happyDOM.waitUntilComplete();
require(
  worksurface.querySelector("#persistent-layer-units")?.getAttribute("display") === "none" &&
    worksurface.querySelector('[data-editor-layer="placement-preview"]')?.getAttribute("display") === "none",
  "unit layer and movement preview did not fail closed when hidden",
);
layerChoice("UnitDomain", "Visible")?.click();
await window.happyDOM.waitUntilComplete();
await sendPointer(worksurface, "pointerup", { clientX: pointerStartX + 64, clientY: pointerStartY });
pointerUnit = worksurface.querySelector('#persistent-layer-units [data-unit-id="1"]');
require(
  worksurface.getAttribute("data-scene-revision") !== pointerRevision &&
    Number(pointerUnit?.querySelector("rect")?.getAttribute("x")) > pointerOriginalX,
  "pointer unit movement did not commit an authored revision",
);
const stableUnitOne = pointerUnit;
const unitTwo = worksurface.querySelector('#persistent-layer-units [data-unit-id="2"]');
unitTwo?.dispatchEvent(new window.MouseEvent("click", { bubbles: true, cancelable: true, shiftKey: true }));
await window.happyDOM.waitUntilComplete();
require(
  worksurface.querySelectorAll("#persistent-layer-selection [data-selection-for]").length === 2 &&
    stableUnitOne === worksurface.querySelector('#persistent-layer-units [data-unit-id="1"]'),
  "Shift-click did not add a second unit without replacing stable unit identity",
);
unitTwo?.dispatchEvent(new window.KeyboardEvent("keydown", { key: " ", bubbles: true, shiftKey: true }));
await window.happyDOM.waitUntilComplete();
require(
  worksurface.querySelectorAll("#persistent-layer-selection [data-selection-for]").length === 1,
  "Shift-Space did not toggle multi-selection through the unit keyboard handler",
);
await sendEditorKey("z", { ctrlKey: true });
await sendPointer(worksurface, "pointerdown", { pointerId: 42, clientX: 610, clientY: 420 });
await sendPointer(worksurface, "pointermove", { pointerId: 42, clientX: 690, clientY: 485 });
require(
  Boolean(worksurface.querySelector('[data-editor-layer="selection-gesture"][data-editor-gesture="box-selection"]')),
  "pointer box selection did not mount its persistent gesture layer",
);
await sendPointer(worksurface, "pointerup", { pointerId: 42, clientX: 690, clientY: 485 });
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

const unitCountBeforeClipboard = worksurface.querySelectorAll(
  "#persistent-layer-units [data-unit-id]",
).length;
buttonByText("Copy")?.click();
await window.happyDOM.waitUntilComplete();
buttonByText("Paste")?.click();
await window.happyDOM.waitUntilComplete();
require(
  Boolean(worksurface.querySelector('[data-editor-layer="placement-preview"]')),
  "pasting the Editor clipboard did not project its placement preview",
);
await sendEditorKey("Enter");
require(
  worksurface.querySelectorAll("#persistent-layer-units [data-unit-id]").length ===
    unitCountBeforeClipboard + 1,
  "clipboard paste did not commit through the persistent renderer",
);
await sendEditorKey("z", { ctrlKey: true });
require(
  worksurface.querySelectorAll("#persistent-layer-units [data-unit-id]").length ===
    unitCountBeforeClipboard,
  "clipboard paste did not remain one undoable transaction",
);
const autosaveName = window.document.querySelector("#map-name");
autosaveName.value = "Autosave qualification";
autosaveName.dispatchEvent(new window.Event("input", { bubbles: true }));
autosaveName.dispatchEvent(new window.Event("change", { bubbles: true }));
await window.happyDOM.waitUntilComplete();
await new Promise((done) => setTimeout(done, 700));
require(
  /^SIR-MAP [123]\n/.test(window.localStorage.getItem("sir.map-editor.autosave.v1") ?? ""),
  `authored changes did not reach observed autosave storage (timer ${Boolean(window.__sirMapAutosaveTimer)}, name ${window.document.querySelector("#map-name")?.value})`,
);

const cameraBefore = Number(worksurface.getAttribute("data-camera-zoom"));
buttonByText(
  "Zoom in",
  window.document.querySelector('[aria-label="Map editor document actions"]'),
)?.click();
await window.happyDOM.waitUntilComplete();
require(
  Number(worksurface.getAttribute("data-camera-zoom")) > cameraBefore,
  "the reachable Editor zoom control did not update the retained camera",
);
buttonByText(
  "Fit",
  window.document.querySelector('[aria-label="Map editor document actions"]'),
)?.click();
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

buttonByText("Clear")?.click();
await window.happyDOM.waitUntilComplete();
buttonByText("Confirm")?.click();
await window.happyDOM.waitUntilComplete();
require(
  worksurface.querySelectorAll("#persistent-layer-units [data-unit-id]").length === 0,
  "destructive confirmation did not execute the confirmed clear",
);
const gapMap = `SIR-MAP 2
size 6 4
edge 1 0 east wall closed
edge 1 2 east wall closed
unit 1 blue rifleman 0 0 1 12 12 manual -
`;
setFile(
  window.document.querySelector('input[aria-label="Import SIR map"]'),
  new window.File([gapMap], "gap.sir-map", { type: "text/plain" }),
);
await settleFile();
buttonByText("Next", window.document.querySelector('[aria-label="Map validation issues"]'))?.click();
await window.happyDOM.waitUntilComplete();
require(
  worksurface.querySelector('[data-editor-layer="validation-overlay"]')?.textContent.includes("EDGE-GAP"),
  "active validation did not mount a live persistent validation overlay",
);

const universalVtt = `{
  "format": 0.3,
  "resolution": {
    "map_size": { "x": 6, "y": 4 },
    "pixels_per_grid": 100,
    "map_origin": { "x": 0, "y": 0 }
  },
  "line_of_sight": [[{"x":100,"y":0},{"x":100,"y":200}]],
  "portals": [{"bounds":[{"x":100,"y":200},{"x":200,"y":200}],"closed":false,"secret":true}],
  "lights": [{"position":{"x":50,"y":50},"range":300}]
}`;
const revisionBeforeReview = worksurface.getAttribute("data-scene-revision");
setFile(
  window.document.querySelector('input[aria-label="Import SIR map"]'),
  new window.File([universalVtt], "review.dd2vtt", { type: "application/json" }),
);
await settleFile();
require(
  Boolean(window.document.querySelector('[aria-label="Interchange import review"]')),
  "non-native import bypassed the required review surface",
);
buttonByText("Cancel import")?.click();
await window.happyDOM.waitUntilComplete();
require(
  !window.document.querySelector('[aria-label="Interchange import review"]') &&
    worksurface.getAttribute("data-scene-revision") === revisionBeforeReview,
  "cancelled reviewed import changed the authoritative map",
);
setFile(
  window.document.querySelector('input[aria-label="Import SIR map"]'),
  new window.File([universalVtt], "review.dd2vtt", { type: "application/json" }),
);
await settleFile();
buttonByText("Accept reviewed import")?.click();
await window.happyDOM.waitUntilComplete();
require(
  !window.document.querySelector('[aria-label="Interchange import review"]') &&
    worksurface.getAttribute("data-scene-revision") !== revisionBeforeReview,
  "accepted reviewed import did not commit its deterministic candidate",
);
const visualMap = `SIR-MAP 2
size 8 4
terrain 1 1 rough
terrain 2 1 blocked
terrain 3 1 objective
edge 1 0 east wall closed
edge 2 0 east door closed
edge 3 0 east door open
edge 4 0 east window closed
unit 1 blue rifleman 0 0 1 12 12 manual -
`;
setFile(
  window.document.querySelector('input[aria-label="Import SIR map"]'),
  new window.File([visualMap], "visual-parity.sir-map", { type: "text/plain" }),
);
await settleFile();
require(
  [...["diagonal-hatch", "cross-hatch", "inset-ring"]].every((pattern) =>
    worksurface.querySelector(`[data-terrain-pattern="${pattern}"]`),
  ),
  "authored terrain hatch/ring patterns are absent from the live SVG",
);
const wall = worksurface.querySelector('#persistent-layer-edges [data-edge-kind="wall"]');
const closedDoor = worksurface.querySelector('#persistent-layer-edges [data-edge-kind="door"][data-edge-state="closed"]');
const openDoor = worksurface.querySelector('#persistent-layer-edges [data-edge-kind="door"][data-edge-state="open"]');
const windowEdge = worksurface.querySelector('#persistent-layer-edges [data-edge-kind="window"]');
require(
  Boolean(wall && closedDoor && openDoor && windowEdge) &&
    wall.getAttribute("stroke") !== closedDoor.getAttribute("stroke") &&
    closedDoor.getAttribute("stroke-dasharray") === "none" &&
    openDoor.getAttribute("stroke-dasharray") === "8 5" &&
    windowEdge.getAttribute("stroke-dasharray") === "3 3",
  "wall, closed/open door, and window rendering are not semantically distinct",
);
require(
  [...stableSceneLayers].every(([layer, reference]) =>
    reference === worksurface.querySelector(`[data-scene-layer="${layer}"]`),
  ),
  "a stable shared scene layer remounted during Editor parity workflows",
);

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
  "Map-editor M4 qualification passed: one accessible persistent SVG owns all Editor layers and pointer intent; registered tools/layers/selection/validation/document panels own Editor controls; atomic editing, undo-redo, camera, modal input, validation, import and accessibility parity remain intact; legacy renderer source/CSS is absent; and seven domain-truthful review boards are hash-bound to the production bundle.",
);
window.happyDOM.close();
