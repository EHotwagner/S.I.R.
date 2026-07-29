import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { pathToFileURL } from "node:url";
import { Window } from "happy-dom";

const output = resolve("artifacts/client");
const html = await readFile(resolve(output, "index.html"), "utf8");
const scriptMatch = html.match(/<script[^>]+src="([^"]+\.js)"/);
const stylesMatch = html.match(/<link[^>]+href="([^"]+\.css)"/);

if (!scriptMatch || !stylesMatch) {
  throw new Error("The production HTML does not reference its application assets.");
}

const styles = await readFile(
  resolve(output, stylesMatch[1].replace(/^\.\//, "")),
  "utf8",
);

if (
  !styles.includes("#sir-replay-app{") ||
  !styles.includes("isolation:isolate") ||
  styles.includes(".app-shell>header")
) {
  throw new Error(
    "The production styles do not contain the documentation-layout isolation or still contain the removed masthead overlay.",
  );
}

const window = new Window({ url: "https://sir.invalid/replay/" });
window.document.body.innerHTML = '<div id="sir-replay-app"></div>';

const workerMessages = [];

const replayProjection = (tick, disclosed) => ({
  Tick: tick,
  BoardMinimumColumn: 0,
  BoardMinimumRow: 0,
  BoardMaximumColumn: 2,
  BoardMaximumRow: 1,
  Units: disclosed
    ? [
        {
          Id: 10,
          Side: "Red",
          Column: 0,
          Row: 0,
          Health: 100,
          HealthMaximum: 100,
        },
        {
          Id: 20,
          Side: "Blue",
          Column: 2,
          Row: 0,
          Health: 75,
          HealthMaximum: 100,
        },
      ]
    : [],
  Edges: disclosed
    ? [
        {
          Id: "edge-0",
          Kind: "wall",
          State: "solid",
          StartColumn: 1,
          StartRow: 0,
          EndColumn: 2,
          EndRow: 0,
        },
      ]
    : [],
  Events: disclosed
    ? [
        {
          Id: 0,
          Tick: 1,
          Source: "Accepted WASM output",
          Summary: "unit 10 attacks unit 20",
          SourceUnitId: 10,
          TargetUnitId: 20,
        },
      ]
    : [],
  Checkpoints: [
    { Tick: 0, StateHash: "000000000000", EventHash: "000000000000" },
    { Tick: 2, StateHash: "222222222222", EventHash: "222222222222" },
  ],
  PerspectiveHash: undefined,
});

const loadedReplayResponse = (message) => ({
  ProtocolVersion: 3,
  Operation: message.Operation,
  Response: {
    tag: 0,
    fields: [
      {
        SourceName: "smoke.sirr",
        SourceIdentity: "smoke-replay",
        EngineIdentity: "010203040506",
        FinalTick: 2,
        Kind: 0,
      },
      { tag: 0 },
      replayProjection(0, true),
    ],
  },
});

const scenarioDefaults = {
  "adjacent-duel": [25, 4],
  "short-duel": [25, 2],
  "single-heavy-strike": [60, 1],
  "rapid-chip-damage": [8, 8],
  "lethality-threshold": [34, 3],
  "near-threshold": [33, 3],
};

const scenarioResponse = (message) => {
  const identity = message.Request.fields[0];
  const [power, count] = scenarioDefaults[identity];
  const remaining = Math.max(0, 100 - power * count);
  const parameters = [
    { Key: "attack-power", Value: power },
    { Key: "attack-count", Value: count },
  ];
  const metrics = [
    { Key: "attack-events", Value: count },
    { Key: "remaining-health", Value: remaining },
    { Key: "total-damage", Value: 100 - remaining },
  ];
  const result = {
    Input: {
      ScenarioIdentity: identity,
      ScenarioRevision: 1,
      EngineIdentity:
        "0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20",
      RulesetIdentity:
        "6d31302d72756c65732d6c61622d763100000000000000000000000000000000",
      Parameters: parameters,
    },
    ResultIdentity: "0123456789abcdef",
    Metrics: metrics,
  };
  const report = {
    Baseline: result,
    Fork: result,
    Delta: metrics.map(({ Key }) => ({ Key, Value: 0 })),
    Sweep: undefined,
    EvidenceLabel: "Exploratory balance evidence — not accepted balance",
  };
  const parameterDefinitions = [
    {
      Key: "attack-power",
      Label: "Attack power",
      Minimum: 1,
      Maximum: 100,
      Step: 1,
      DefaultValue: power,
    },
    {
      Key: "attack-count",
      Label: "Attack count",
      Minimum: 1,
      Maximum: 8,
      Step: 1,
      DefaultValue: count,
    },
  ];

  return {
    ProtocolVersion: 3,
    Operation: message.Operation,
    Response: {
      tag: 4,
      fields: [
        {
          SourceName: `${identity}.sir-scenario`,
          SourceIdentity: result.ResultIdentity,
          EngineIdentity: result.Input.EngineIdentity,
          FinalTick: 1,
          Kind: 2,
        },
        {
          Identity: identity,
          Revision: 1,
          Title: identity,
          Description: "Smoke scenario",
          EngineIdentity: result.Input.EngineIdentity,
          RulesetIdentity: result.Input.RulesetIdentity,
          Parameters: parameterDefinitions,
        },
        report,
        {
          Tick: 0,
          BoardMinimumColumn: 0,
          BoardMinimumRow: 0,
          BoardMaximumColumn: 0,
          BoardMaximumRow: 0,
          Units: [],
          Edges: [],
          Events: [],
          Checkpoints: [],
          PerspectiveHash: undefined,
        },
      ],
    },
  };
};

class SmokeWorker {
  postMessage(message) {
    workerMessages.push(message);
    if (
      message.Request?.tag === 0 ||
      (Array.isArray(message.Request) && message.Request[0] === "LoadPackage")
    ) {
      queueMicrotask(() =>
        this.onmessage?.({ data: structuredClone(loadedReplayResponse(message)) }),
      );
    } else if (
      message.Request?.tag === 2 ||
      (Array.isArray(message.Request) && message.Request[0] === "Seek")
    ) {
      const target = Array.isArray(message.Request)
        ? message.Request[1]
        : message.Request.fields[0];
      queueMicrotask(() =>
        this.onmessage?.({
          data: structuredClone({
            ProtocolVersion: 3,
            Operation: message.Operation,
            Response: {
              tag: 2,
              fields: [target, replayProjection(target, target < 2)],
            },
          }),
        }),
      );
    } else if (message.Request?.tag === 4) {
      queueMicrotask(() =>
        this.onmessage?.({ data: structuredClone(scenarioResponse(message)) }),
      );
    }
  }
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
  Worker: SmokeWorker,
});

const bundle = resolve(output, scriptMatch[1].replace(/^\.\//, ""));
await import(pathToFileURL(bundle));
await window.happyDOM.waitUntilComplete();

const application = window.document.querySelector(
  'main[aria-label="S.I.R. simulator and editor"]',
);

if (!application || application.querySelector("header, h1")) {
  throw new Error("The React simulator did not mount.");
}

const buttonByText = (text) =>
  [...window.document.querySelectorAll("button")].find(
    (button) => button.textContent.trim() === text,
  );

const viewMenu = [...window.document.querySelectorAll(
  '[aria-label="Map editor menus"] summary',
)].find((summary) => summary.textContent.trim() === "View");
viewMenu?.click();
await window.happyDOM.waitUntilComplete();
if (!viewMenu?.closest("details")?.open) {
  throw new Error("The classical View menu did not open.");
}
window.document
  .querySelector('[aria-label="SVG tactical map workspace"]')
  ?.dispatchEvent(new window.MouseEvent("click", { bubbles: true }));
await window.happyDOM.waitUntilComplete();
if (viewMenu.closest("details")?.open) {
  throw new Error("The classical editor menu did not close after clicking elsewhere.");
}

const newMapButton = buttonByText("New");
newMapButton?.click();
await window.happyDOM.waitUntilComplete();
const newMapDialog = window.document.querySelector(
  '[role="alertdialog"][aria-label="Confirm destructive map command"]',
);
if (
  !newMapDialog?.textContent.includes("Create “Untitled battlefield” at 12×8")
) {
  throw new Error("The File/New Map workflow did not request destructive confirmation.");
}
buttonByText("Cancel")?.click();
await window.happyDOM.waitUntilComplete();

const handoffButton = buttonByText("Simulate");
if (
  !handoffButton ||
  !window.document
    .querySelector('[aria-label="Map editor menu and toolbar"]')
    ?.textContent.includes("Not in Simulator")
) {
  throw new Error("The editor did not expose the explicit immutable simulator handoff.");
}
handoffButton.click();
await window.happyDOM.waitUntilComplete();

buttonByText("Controls")?.click();
await window.happyDOM.waitUntilComplete();
const controllerPanel = window.document.querySelector(
  '[aria-label="Simulation controllers"]',
);
const editorBattlefield = window.document.querySelector(
  '[aria-label="Editable simulation SVG battlefield"] svg[role="application"]',
);
if (
  !window.document.querySelector('[aria-label="Simulator menu and toolbar"]') ||
  !window.document.querySelector('[aria-label="Simulator command panel"]') ||
  !controllerPanel?.textContent.includes("Manual") ||
  !controllerPanel.textContent.includes("Scripted AI") ||
  !controllerPanel.textContent.includes("General AI") ||
  editorBattlefield?.querySelectorAll("[data-unit-id]").length !== 4 ||
  editorBattlefield?.querySelectorAll('[data-terrain="objective"]').length !== 2 ||
  editorBattlefield?.querySelectorAll('[data-terrain="rough"]').length !== 4
) {
  throw new Error("The full-width simulator or its controller modes did not mount.");
}

controllerPanel
  .querySelector('button[aria-label="Advance the map simulation one tick"]')
  ?.click();
await window.happyDOM.waitUntilComplete();
if (
  !window.document
    .querySelector('[aria-label="Editable simulation SVG battlefield"] svg')
    ?.getAttribute("aria-label")
    ?.includes("exact tick 1")
) {
  throw new Error("The simulator did not advance one deterministic tick.");
}

buttonByText("Editor")?.click();
await window.happyDOM.waitUntilComplete();

const editorWorkspace = window.document.querySelector(
  '[aria-label="SVG tactical map workspace"] svg[role="application"]',
);
const objectList = window.document.querySelector(
  '[aria-label="Map object list fallback"]',
);
const editorSymbols =
  editorWorkspace?.querySelectorAll("[data-editor-unit-id]") ?? [];
const editorCanvas = window.document.querySelector(
  '[aria-label="SVG tactical map workspace"]',
);
const initialEditorDigest =
  "eab8b1c053454ed4d944c3b73f30e84c7025d13664d88f52e27d68e92c046608";
if (
  !editorWorkspace ||
  editorCanvas?.getAttribute("data-editor-revision") !== initialEditorDigest ||
  editorCanvas?.getAttribute("data-editor-revision-state") !== "SavedRevision" ||
  !objectList ||
  objectList.querySelectorAll("[data-map-column]").length !== 96 ||
  editorSymbols.length !== 4 ||
  [...editorSymbols].some(
    (unit) => unit.querySelectorAll("[data-class-id]").length !== 1,
  ) ||
  window.document.querySelector('[aria-label="Editable map grid"]') ||
  window.document.querySelector('[aria-label="Simulation controllers"]')
) {
  throw new Error(
    "The editor tab did not use the SVG workspace, canonical square-unit symbols, and object-list fallback.",
  );
}

objectList
  ?.querySelectorAll("button")[2]
  ?.dispatchEvent(new window.MouseEvent("click", { bubbles: true }));
await window.happyDOM.waitUntilComplete();
buttonByText("Copy")?.click();
await window.happyDOM.waitUntilComplete();
buttonByText("Paste")?.click();
await window.happyDOM.waitUntilComplete();
const pastedDigest = editorCanvas?.getAttribute("data-editor-revision");
if (
  pastedDigest === initialEditorDigest ||
  editorWorkspace.querySelectorAll("[data-editor-unit-id]").length !== 5
) {
  throw new Error("Fable did not commit a copied formation as one immutable revision.");
}
editorWorkspace.dispatchEvent(
  new window.KeyboardEvent("keydown", {
    key: "z",
    ctrlKey: true,
    bubbles: true,
  }),
);
await window.happyDOM.waitUntilComplete();
if (
  editorCanvas?.getAttribute("data-editor-revision") !== initialEditorDigest ||
  editorWorkspace.querySelectorAll("[data-editor-unit-id]").length !== 4
) {
  throw new Error(
    `Fable undo did not round-trip to the .NET revision digest: ${editorCanvas?.getAttribute("data-editor-revision")} / ${editorWorkspace.querySelectorAll("[data-editor-unit-id]").length}.`,
  );
}
editorWorkspace.dispatchEvent(
  new window.KeyboardEvent("keydown", {
    key: "z",
    ctrlKey: true,
    shiftKey: true,
    bubbles: true,
  }),
);
await window.happyDOM.waitUntilComplete();
if (
  editorCanvas?.getAttribute("data-editor-revision") !== pastedDigest ||
  editorWorkspace.querySelectorAll("[data-editor-unit-id]").length !== 5
) {
  throw new Error("Fable redo did not round-trip the immutable map revision.");
}

buttonByText("Terrain")?.click();
await window.happyDOM.waitUntilComplete();
if (
  !window.document.querySelector(
    'button[aria-label="rough terrain, diagonal hatch, shortcut Shift+2"]',
  ) ||
  !window.document.querySelector(
    'button[aria-label="blocked terrain, cross hatch, shortcut Shift+3"]',
  ) ||
  !window.document.querySelector("#terrain-brush-size")
) {
  throw new Error("The accessible terrain palette, patterns, or integer brush control did not mount.");
}
buttonByText("Pencil (P)")?.click();
await window.happyDOM.waitUntilComplete();
window.document
  .querySelector(
    'button[aria-label="blocked terrain, cross hatch, shortcut Shift+3"]',
  )
  ?.click();
await window.happyDOM.waitUntilComplete();
window.document
  .querySelector('[aria-label="SVG tactical map workspace"] svg[role="application"]')
  ?.dispatchEvent(
  new window.KeyboardEvent("keydown", { key: "Enter", bubbles: true }),
  );
await window.happyDOM.waitUntilComplete();
const previewWorkspace = window.document.querySelector(
  '[aria-label="SVG tactical map workspace"] svg[role="application"]',
);
if (
  previewWorkspace?.querySelectorAll('[data-preview-terrain="blocked"]').length !== 1 ||
  !editorCanvas?.textContent.includes("1 terrain cell previewed")
) {
  throw new Error("The keyboard terrain cursor did not create and announce an exact preview.");
}
previewWorkspace?.dispatchEvent(
  new window.KeyboardEvent("keydown", { key: "Enter", bubbles: true }),
);
await window.happyDOM.waitUntilComplete();
const terrainDigest = editorCanvas?.getAttribute("data-editor-revision");
const committedTerrainWorkspace = window.document.querySelector(
  '[aria-label="SVG tactical map workspace"] svg[role="application"]',
);
if (
  terrainDigest === pastedDigest ||
  committedTerrainWorkspace?.querySelectorAll('[data-terrain="blocked"]').length !== 1 ||
  !editorCanvas?.textContent.includes("Painted 1 terrain cell")
) {
  throw new Error("The keyboard terrain gesture did not commit one announced revision.");
}
buttonByText("Undo")?.click();
await window.happyDOM.waitUntilComplete();
if (
  editorCanvas?.getAttribute("data-editor-revision") !== pastedDigest ||
  window.document.querySelector(
    '[aria-label="SVG tactical map workspace"] [data-terrain="blocked"]',
  )
) {
  throw new Error("Undo did not remove the complete keyboard terrain stroke atomically.");
}

buttonByText("Edges")?.click();
await window.happyDOM.waitUntilComplete();
buttonByText("East wall")?.click();
await window.happyDOM.waitUntilComplete();
editorWorkspace?.dispatchEvent(
  new window.KeyboardEvent("keydown", { key: "Enter", bubbles: true }),
);
await window.happyDOM.waitUntilComplete();
editorWorkspace?.dispatchEvent(
  new window.KeyboardEvent("keydown", {
    key: "ArrowDown",
    shiftKey: true,
    bubbles: true,
  }),
);
await window.happyDOM.waitUntilComplete();
if (
  editorWorkspace?.querySelectorAll('[data-edge-preview="Wall"]').length !== 2 ||
  !editorCanvas?.textContent.includes("2 wall polyline segments previewed")
) {
  throw new Error("Keyboard edge placement did not expose an announced two-segment preview.");
}
editorWorkspace?.dispatchEvent(
  new window.KeyboardEvent("keydown", { key: "Enter", bubbles: true }),
);
await window.happyDOM.waitUntilComplete();
const edgeDigest = editorCanvas?.getAttribute("data-editor-revision");
if (
  edgeDigest === pastedDigest ||
  editorWorkspace?.querySelectorAll('[data-edge-kind="Wall"]').length < 3 ||
  !editorCanvas?.textContent.includes("Committed 2-segment wall polyline")
) {
  throw new Error("Keyboard edge completion did not commit one announced revision.");
}
buttonByText("Door")?.click();
await window.happyDOM.waitUntilComplete();
buttonByText("Open/close")?.click();
await window.happyDOM.waitUntilComplete();
if (
  !editorWorkspace?.querySelector(
    '[data-edge-kind="Door"][data-edge-state="open"]',
  ) ||
  !editorCanvas?.textContent.includes("Door opened")
) {
  throw new Error("The accessible edge actions did not convert and open the cursor door.");
}

buttonByText("Zones")?.click();
await window.happyDOM.waitUntilComplete();
buttonByText("Objective rectangle")?.click();
await window.happyDOM.waitUntilComplete();
const regionDigest = editorCanvas?.getAttribute("data-editor-revision");
const regionShape = editorWorkspace?.querySelector(
  '[data-layer="regions"] [data-region-id="1"]',
);
const regionList = objectList?.querySelector(
  '[aria-label="Authoritative map regions"]',
);
if (
  regionDigest === edgeDigest ||
  regionShape?.getAttribute("role") !== "button" ||
  !regionList?.textContent.includes("Region 1 · Objective · rectangle") ||
  !editorCanvas?.textContent.includes("Objective rectangle created as region 1")
) {
  throw new Error(
    "The accessible region workflow did not create, project, list, and announce an authoritative objective rectangle.",
  );
}
window.document
  .querySelector('button[aria-label="Move selected region right"]')
  ?.click();
await window.happyDOM.waitUntilComplete();
if (editorCanvas?.getAttribute("data-editor-revision") === regionDigest) {
  throw new Error("The selected region edit did not create an immutable revision.");
}

buttonByText("Map file")?.click();
await window.happyDOM.waitUntilComplete();
if (
  !window.document.querySelector(
    '.editor-context-palette input[aria-label="Import SIR map"]',
  ) ||
  !buttonByText("Export map") ||
  !buttonByText("Repository bundle") ||
  !window.document.querySelector(
    'input[aria-label="Choose local raster map background"]',
  )
) {
  throw new Error(
    "The editor Map file sub-tab did not expose import, export, and accessible local-background controls.",
  );
}

window.document
  .querySelector('[aria-label="SVG tactical map workspace"] svg[role="application"]')
  ?.dispatchEvent(
  new window.KeyboardEvent("keydown", { key: "F2", bubbles: true }),
);
await window.happyDOM.waitUntilComplete();
if (
  window.document.querySelector(
    '.editor-context-palette input[aria-label="Import SIR map"]',
  )
) {
  throw new Error("F2 did not hide the active contextual ribbon.");
}
window.document
  .querySelector('[aria-label="SVG tactical map workspace"] svg[role="application"]')
  ?.dispatchEvent(
  new window.KeyboardEvent("keydown", { key: "F2", bubbles: true }),
);
await window.happyDOM.waitUntilComplete();
if (
  !window.document.querySelector(
    '.editor-context-palette input[aria-label="Import SIR map"]',
  )
) {
  throw new Error("F2 did not restore the active contextual ribbon.");
}

const inspectorBeforeF3 = window.document.querySelector(
  '[aria-label="Map editor inspector"]',
);
window.document
  .querySelector('[aria-label="SVG tactical map workspace"] svg[role="application"]')
  ?.dispatchEvent(
  new window.KeyboardEvent("keydown", { key: "F3", bubbles: true }),
);
await window.happyDOM.waitUntilComplete();
if (
  Boolean(
    window.document.querySelector('[aria-label="Map editor inspector"]'),
  ) === Boolean(inspectorBeforeF3)
) {
  throw new Error("F3 did not toggle the map editor inspector overlay.");
}
window.document
  .querySelector('[aria-label="SVG tactical map workspace"] svg[role="application"]')
  ?.dispatchEvent(
  new window.KeyboardEvent("keydown", { key: "F3", bubbles: true }),
);
await window.happyDOM.waitUntilComplete();

const digestBeforeBackground = editorCanvas?.getAttribute("data-editor-revision");
const backgroundInput = window.document.querySelector(
  'input[aria-label="Choose local raster map background"]',
);
const pngHeader = new Uint8Array([
  137, 80, 78, 71, 13, 10, 26, 10,
  0, 0, 0, 13, 73, 72, 68, 82,
  0, 0, 4, 0, 0, 0, 2, 0,
]);
const backgroundFile = new window.File([pngHeader], "review.png", {
  type: "image/png",
});
Object.defineProperty(backgroundInput, "files", {
  configurable: true,
  value: {
    0: backgroundFile,
    length: 1,
    item(index) {
      return index === 0 ? backgroundFile : null;
    },
  },
});
backgroundInput.dispatchEvent(new window.Event("change", { bubbles: true }));
await new Promise((resolveWait) => setTimeout(resolveWait, 0));
await window.happyDOM.waitUntilComplete();
if (
  !editorWorkspace?.querySelector('[data-layer="local-raster-background"]') ||
  !buttonByText("Unlock") ||
  editorCanvas?.getAttribute("data-editor-revision") !== digestBeforeBackground
) {
  throw new Error(
    "A validated local raster did not render as locked presentation-only state.",
  );
}

const importInput = window.document.querySelector(
  'input[aria-label="Import SIR map"]',
);
const uvttFile = new window.File(
  [
    JSON.stringify({
      format: 0.3,
      resolution: {
        map_size: { x: 6, y: 4 },
        pixels_per_grid: 100,
        map_origin: { x: 0, y: 0 },
      },
      line_of_sight: [[{ x: 100, y: 0 }, { x: 100, y: 100 }]],
      portals: [],
      lights: [{ color: "#fff" }],
    }),
  ],
  "review.dd2vtt",
  { type: "application/json" },
);
Object.defineProperty(importInput, "files", {
  configurable: true,
  value: {
    0: uvttFile,
    length: 1,
    item(index) {
      return index === 0 ? uvttFile : null;
    },
  },
});
importInput.dispatchEvent(new window.Event("change", { bubbles: true }));
await new Promise((resolveWait) => setTimeout(resolveWait, 0));
await window.happyDOM.waitUntilComplete();
const review = window.document.querySelector(
  '[aria-label="Interchange import review"]',
);
if (
  !review?.textContent.includes("lights[0].color") ||
  !review?.textContent.includes("Ignored") ||
  buttonByText("Accept reviewed import")?.disabled ||
  editorCanvas?.getAttribute("data-editor-revision") !== digestBeforeBackground
) {
  throw new Error(
    "Universal VTT selection did not present every ignored field before preserving the current map.",
  );
}
buttonByText("Cancel import")?.click();
await window.happyDOM.waitUntilComplete();
if (
  window.document.querySelector('[aria-label="Interchange import review"]') ||
  editorCanvas?.getAttribute("data-editor-revision") !== digestBeforeBackground
) {
  throw new Error(
    "Cancelling the reviewed interchange import changed the authoritative map.",
  );
}

buttonByText("Replay")?.click();
await window.happyDOM.waitUntilComplete();

const status = window.document.querySelector('[role="status"]');
const fileInput = window.document.querySelector(
  'input[aria-label="Choose replay package"]',
);
const labelledButtons = [...window.document.querySelectorAll("button")].filter(
  (button) => button.getAttribute("aria-label"),
);
const inspector = window.document.querySelector('[aria-label="Replay inspector"]');
const workerStatus = window.document.querySelector(".worker-status");

if (
  !status?.textContent.includes("Ready — choose a scenario or load a replay") ||
  !status.textContent.includes(
    "Authoritative verification is available only from .NET exact-artifact WASM re-execution.",
  ) ||
  !fileInput ||
  labelledButtons.length < 4 ||
  !inspector?.textContent.includes("Timeline and events") ||
  !workerStatus?.textContent.includes("protocol 3")
) {
  throw new Error("The replay workspace or its verification boundary is incomplete.");
}

const battlefield = window.document.querySelector(
  'svg[role="application"][aria-label*="exact tick 24"]',
);
const battlefieldUnits = battlefield?.querySelectorAll("[data-unit-id]") ?? [];
const healthPositions =
  battlefield?.querySelectorAll("[data-health-position]") ?? [];
const exactOverlay = battlefield?.querySelector(
  '[data-overlay-id="selected-los-1"][data-overlay-disposition="exact"][data-path-segments="3"]',
);
const squareFootprints = [
  ...(battlefield?.querySelectorAll("[data-authoritative-footprint]") ?? []),
].every(
  (footprint) =>
    footprint.getAttribute("width") === footprint.getAttribute("height"),
);
const fittedSquareSymbols = [...battlefieldUnits].every((unit) => {
  const footprint = unit.querySelector("[data-authoritative-footprint]");
  const symbol = unit.querySelector("[data-unit-symbol]");
  if (!footprint || !symbol) return false;

  const footprintWidth = Number(footprint.getAttribute("width"));
  const footprintHeight = Number(footprint.getAttribute("height"));
  const symbolWidth = Number(symbol.getAttribute("width"));
  const symbolHeight = Number(symbol.getAttribute("height"));

  return (
    symbolWidth === symbolHeight &&
    footprintWidth === footprintHeight &&
    symbolWidth === footprintWidth - 8
  );
});
const representativeHumanFootprints = ["1", "2", "3"].every((unitId) => {
  const footprint = battlefield?.querySelector(
    `[data-unit-id="${unitId}"] [data-authoritative-footprint]`,
  );

  return (
    footprint?.getAttribute("width") === "92" &&
    footprint?.getAttribute("height") === "92"
  );
});

if (
  !battlefield ||
  battlefieldUnits.length !== 6 ||
  battlefield.querySelectorAll("[data-authoritative-footprint]").length !== 6 ||
  !squareFootprints ||
  !fittedSquareSymbols ||
  !representativeHumanFootprints ||
  battlefield.querySelectorAll("[data-facing-wedge]").length !== 6 ||
  healthPositions.length !== 72 ||
  battlefield.querySelectorAll("[data-elevation-label]").length !== 2 ||
  battlefield.querySelectorAll("[data-stance-mark]").length !== 5 ||
  battlefield.querySelectorAll("[data-secondary-heading]").length !== 2 ||
  battlefield.querySelectorAll("[data-action-trace]").length !== 2 ||
  !exactOverlay ||
  window.document.querySelectorAll("[data-timeline-lane]").length !== 3
) {
  throw new Error(
    "The detailed static SVG omitted symbols, exact overlays, typed second headings, action traces, or semantic timeline lanes.",
  );
}

const motionSettings = [
  ...window.document.querySelectorAll(
    '.battlefield-sidecar input[type="checkbox"]',
  ),
];
if (
  motionSettings.length !== 2 ||
  motionSettings.some((setting) => setting.checked)
) {
  throw new Error("Exact-tick and reduced-motion browser controls are incomplete.");
}
motionSettings[1].click();
await window.happyDOM.waitUntilComplete();
if (!motionSettings[1].checked) {
  throw new Error("Reduced-motion mode did not become active.");
}

const selectedTroll = battlefield.querySelector('[data-unit-id="6"]');
selectedTroll?.dispatchEvent(
  new window.MouseEvent("click", { bubbles: true }),
);
await window.happyDOM.waitUntilComplete();
if (
  !window.document
    .querySelector('[aria-label="Battlefield unit inspector"]')
    ?.textContent.includes("Arcane troll Stone")
) {
  throw new Error("SVG selection did not update the equivalent HTML inspector.");
}

const firstUnit = battlefield.querySelector('[data-unit-id="1"]');
firstUnit?.dispatchEvent(new window.Event("focus", { bubbles: true }));
firstUnit?.dispatchEvent(
  new window.KeyboardEvent("keydown", { key: "ArrowRight", bubbles: true }),
);
await window.happyDOM.waitUntilComplete();
if (
  battlefield.querySelector('[data-unit-id="2"]')?.getAttribute("tabindex") !== "0"
) {
  throw new Error("Arrow-key roving focus did not move to the nearest unit.");
}

const zoomOut = window.document.querySelector(
  'button[aria-label="Zoom battlefield out"]',
);
zoomOut?.click();
await window.happyDOM.waitUntilComplete();
if (
  !window.document.querySelector(".semantic-zoom")?.textContent.includes("Standard") ||
  battlefield.querySelector("[data-elevation-label]") ||
  battlefield.querySelector("[data-stance-mark]")
) {
  throw new Error("Semantic zoom did not remove detailed-only labels and stance.");
}

const palette = window.document.querySelector("#battlefield-palette");
palette.value = "high-contrast";
palette.dispatchEvent(new window.Event("change", { bubbles: true }));
await window.happyDOM.waitUntilComplete();
if (palette.value !== "high-contrast" || battlefieldUnits.length !== 6) {
  throw new Error("Palette selection changed geometry or failed to update.");
}

const replayFile = new window.File([new Uint8Array([1, 2, 3])], "smoke.sirr", {
  type: "application/octet-stream",
});
Object.defineProperty(fileInput, "files", {
  configurable: true,
  value: {
    0: replayFile,
    length: 1,
    item(index) {
      return index === 0 ? replayFile : null;
    },
  },
});
fileInput.dispatchEvent(new window.Event("change", { bubbles: true }));
await new Promise((resolveWait) => setTimeout(resolveWait, 0));
await window.happyDOM.waitUntilComplete();

const loadedBattlefield = window.document.querySelector(
  '[aria-label="Loaded replay SVG battlefield"] svg[role="application"]',
);
const loadedUnits = loadedBattlefield?.querySelectorAll("[data-unit-id]") ?? [];
if (
  !loadedBattlefield ||
  !loadedBattlefield.getAttribute("aria-label")?.includes("exact tick 0") ||
  loadedUnits.length !== 2 ||
  loadedBattlefield.querySelectorAll("[data-authoritative-footprint]").length !== 2 ||
  !window.document
    .querySelector('[aria-label="Replay checkpoint markers"]')
    ?.textContent.includes("T2")
) {
  throw new Error(
    "The loaded bounded worker projection did not replace the static SVG or expose checkpoints. "
      + `status=${status?.textContent} messages=${JSON.stringify(workerMessages.slice(-2))}`,
  );
}

const contact = loadedBattlefield.querySelector('[data-unit-id="10"]');
contact?.dispatchEvent(new window.MouseEvent("click", { bubbles: true }));
contact?.focus();
window.document
  .querySelector('button[aria-label="Seek to checkpoint at tick 2"]')
  ?.click();
await new Promise((resolveWait) => setTimeout(resolveWait, 0));
await window.happyDOM.waitUntilComplete();

const lostContactBattlefield = window.document.querySelector(
  '[aria-label="Loaded replay SVG battlefield"] svg[role="application"]',
);
const replayInspector = window.document.querySelector(
  '[aria-label="Replay inspector"]',
);
if (
  !lostContactBattlefield?.getAttribute("aria-label")?.includes("exact tick 2") ||
  lostContactBattlefield.querySelector("[data-unit-id]") ||
  lostContactBattlefield.querySelector("[data-authoritative-footprint]") ||
  lostContactBattlefield.querySelector('[role="button"]') ||
  [...lostContactBattlefield.querySelectorAll("[aria-label]")].some((node) =>
    node.getAttribute("aria-label")?.includes("unit 10"),
  ) ||
  replayInspector?.querySelector(".unit-token") ||
  replayInspector?.querySelector(".event-list button") ||
  !replayInspector?.textContent.includes("UnitNone") ||
  !replayInspector?.textContent.includes("EventNone") ||
  contact?.isConnected ||
  window.document.activeElement === contact
) {
  throw new Error(
    "Lost contact left visual, DOM/accessibility, event-link, selection/focus, or hit-target residue. "
      + `svg=${lostContactBattlefield?.getAttribute("aria-label")} units=${lostContactBattlefield?.querySelectorAll("[data-unit-id]").length} `
      + `inspector=${replayInspector?.textContent} connected=${contact?.isConnected} active=${window.document.activeElement?.outerHTML}`,
  );
}

buttonByText("Rules and data")?.click();
await window.happyDOM.waitUntilComplete();

const catalog = window.document.querySelector('[aria-label="Design scenario catalog"]');
const labResults = window.document.querySelector('[aria-label="Laboratory results"]');
if (
  !catalog?.textContent.includes("Four-hit baseline") ||
  !catalog.textContent.includes("Single heavy strike") ||
  !catalog.textContent.includes("Near-threshold survivor") ||
  !labResults?.textContent.includes("Simulation result") ||
  !labResults.textContent.includes("Click “Simulate now” on any scenario above.")
) {
  throw new Error("The rules workspace did not mount.");
}

const scenarioButtons = [
  ...catalog.querySelectorAll('button[aria-label^="Simulate design scenario"]'),
];
if (scenarioButtons.length !== 6) {
  throw new Error(`Expected 6 runnable scenarios, found ${scenarioButtons.length}.`);
}

const heavyStrike = scenarioButtons.find(
  (button) =>
    button.getAttribute("aria-label") ===
    "Simulate design scenario Single heavy strike",
);
heavyStrike?.click();
await window.happyDOM.waitUntilComplete();

if (
  !workerMessages.some((message) =>
    JSON.stringify(message).includes("single-heavy-strike"),
  )
) {
  throw new Error("Selecting a scenario did not send a worker request.");
}

if (
  !labResults?.textContent.includes(
    "1 attacks resolved · 60 damage · target finishes on 40 HP",
  ) ||
  status?.textContent.includes("Loading replay")
) {
  throw new Error(
    "The structured-cloned worker response did not correlate with the pending operation.",
  );
}

const comparison = window.document.querySelector(
  '[aria-label="Linked baseline and fork comparison"]',
);
const comparisonViewport = comparison?.querySelector(".comparison-viewport");
const comparisonBoards = [
  ...(comparison?.querySelectorAll("svg[data-comparison-camera]") ?? []),
];
if (
  !comparison?.textContent.includes(
    "Immutable baseline — exploratory simulation",
  ) ||
  !comparison?.textContent.includes(
    "Derived fork — exploratory simulation, not verified replay",
  ) ||
  !comparison?.textContent.includes(
    "Neither side is a verified replay",
  ) ||
  comparisonViewport?.getAttribute("data-linked-camera") !== "true" ||
  comparisonViewport?.getAttribute("data-linked-selection") !== "true" ||
  comparisonViewport?.getAttribute("data-linked-overlays") !== "true" ||
  comparisonBoards.length !== 2 ||
  comparisonBoards[0].getAttribute("data-comparison-camera") !==
    comparisonBoards[1].getAttribute("data-comparison-camera") ||
  comparisonBoards.some(
    (board) => board.querySelectorAll("[data-comparison-unit]").length !== 2,
  ) ||
  !comparison?.textContent.includes("Evidence provenance: source") ||
  !comparison?.textContent.includes("sir-safe-svg-renderer-v1")
) {
  throw new Error(
    "The persistently labelled linked comparison or evidence provenance is incomplete.",
  );
}

comparison
  ?.querySelector('button[aria-label="Use difference overlay comparison"]')
  ?.click();
comparison
  ?.querySelector('button[aria-label="Bookmark linked comparison tick"]')
  ?.click();
await window.happyDOM.waitUntilComplete();
if (
  !comparison?.querySelector(".comparison-difference") ||
  !comparison
    ?.querySelector('[aria-label="Comparison bookmarks"]')
    ?.textContent.includes("Linked comparison at tick")
) {
  throw new Error("Comparison view switching or persistent bookmarks failed.");
}

for (const exportLabel of [
  "Export sanitized SVG evidence with provenance",
  "Export PNG evidence rasterized from sanitized SVG",
]) {
  if (!comparison?.querySelector(`button[aria-label="${exportLabel}"]`)) {
    throw new Error(`Missing evidence export control: ${exportLabel}.`);
  }
}

const evidenceBlobs = [];
const originalCreateObjectUrl = URL.createObjectURL;
URL.createObjectURL = function captureEvidence(blob) {
  evidenceBlobs.push(blob);
  return originalCreateObjectUrl.call(URL, blob);
};
comparison
  ?.querySelector(
    'button[aria-label="Export sanitized SVG evidence with provenance"]',
  )
  ?.click();
await window.happyDOM.waitUntilComplete();
URL.createObjectURL = originalCreateObjectUrl;
const exportedSvg = await evidenceBlobs.at(-1)?.text();
if (
  !exportedSvg?.includes("DERIVED SIMULATION — NOT VERIFIED REPLAY") ||
  !exportedSvg.includes("source=") ||
  !exportedSvg.includes("replay=") ||
  !exportedSvg.includes("projection=") ||
  !exportedSvg.includes("palette=high-contrast") ||
  !exportedSvg.includes("renderer=sir-safe-svg-renderer-v1") ||
  /<script|onload=|onclick=|onerror=|<foreignObject|href=|url\(|<style|<path| id=/i.test(
    exportedSvg,
  )
) {
  throw new Error("The browser SVG download is missing provenance or is not closed.");
}

const rulesData = window.document.querySelector(
  '[aria-label="Rules data tables"]',
);
const rulesTables = rulesData?.querySelectorAll("table") ?? [];

if (
  rulesTables.length !== 7 ||
  !rulesData?.textContent.includes("Point Man") ||
  !rulesData?.textContent.includes("Anti-armor launcher") ||
  !rulesData?.textContent.includes("Armored troll")
) {
  throw new Error("The unit, perk, weapon, armor, and equipment catalog is incomplete.");
}

buttonByText("Samples")?.click();
await window.happyDOM.waitUntilComplete();
const samplesWorkspace = window.document.querySelector(
  '[aria-label="Curated maps simulations and replays"]',
);
const trollSampleCard = [...(samplesWorkspace?.querySelectorAll(".sample-card") ?? [])]
  .find((card) => card.textContent.includes("Troll assault"));
if (
  !samplesWorkspace ||
  trollSampleCard?.tagName !== "DETAILS" ||
  !trollSampleCard?.textContent.includes("240 HP armored troll") ||
  !samplesWorkspace.textContent.includes("Troll reaches the line") ||
  !samplesWorkspace.textContent.includes("Closed-door stalemate")
) {
  throw new Error("The expandable curated sample lists did not mount.");
}
trollSampleCard.querySelector("summary")?.click();
await window.happyDOM.waitUntilComplete();
trollSampleCard
  .querySelector('button[aria-label*="Run Troll assault"]')
  ?.click();
await window.happyDOM.waitUntilComplete();
if (
  !window.document
    .querySelector('[aria-label="Simulator menu and toolbar"]')
    ?.textContent.includes("Troll assault") ||
  window.document
    .querySelectorAll(
      '[aria-label="Editable simulation SVG battlefield"] [data-unit-id]',
    ).length !== 4
) {
  throw new Error("The troll assault sample did not open in the compact simulator.");
}
buttonByText("Controls")?.click();
await window.happyDOM.waitUntilComplete();
if (
  !window.document
    .querySelector('[aria-label="Simulator command panel"]')
    ?.textContent.includes("Movement 1500 mm/s") ||
  !window.document
    .querySelector('[aria-label="Simulator command panel"]')
    ?.textContent.includes("Route planner")
) {
  throw new Error("The simulator did not expose timed movement and path planning.");
}
const rifleFootprint = () =>
  window.document.querySelector(
    '[aria-label="Editable simulation SVG battlefield"] [data-unit-id="1"] [data-authoritative-footprint="true"]',
  );
for (const label of ["Use exact-tick playback", "Use reduced motion"]) {
  const option = window.document.querySelector(`input[aria-label="${label}"]`);
  if (option?.checked) {
    option.click();
    await window.happyDOM.waitUntilComplete();
  }
}
const initialRifleX = Number(rifleFootprint()?.getAttribute("x"));
buttonByText("Step")?.click();
await window.happyDOM.waitUntilComplete();
const progressingRifleX = Number(rifleFootprint()?.getAttribute("x"));
if (
  !Number.isFinite(initialRifleX) ||
  !Number.isFinite(progressingRifleX) ||
  progressingRifleX <= initialRifleX ||
  progressingRifleX >= initialRifleX + 48
) {
  throw new Error(
    `Movement credit did not produce fractional open-ground motion (${initialRifleX} -> ${progressingRifleX}).`,
  );
}
for (let tick = 1; tick < 20; tick += 1) {
  buttonByText("Step")?.click();
}
await window.happyDOM.waitUntilComplete();
buttonByText("Events")?.click();
await window.happyDOM.waitUntilComplete();
if (
  !window.document.querySelector(
    '[data-combat-indicator="combat-projectile"] [data-projectile="true"]',
  ) ||
  !window.document
    .querySelector('[aria-label="Combat indicator legend"]')
    ?.textContent.includes("Melee strike") ||
  !window.document
    .querySelector('[aria-label="Simulator command panel"]')
    ?.textContent.includes("ranged attack")
) {
  throw new Error("Riflemen did not resolve and display typed ranged attacks.");
}
buttonByText("Samples")?.click();
await window.happyDOM.waitUntilComplete();
const objectiveSample = [...window.document.querySelectorAll(".sample-card")]
  .find((card) => card.textContent.includes("Objective crossing"));
objectiveSample?.querySelector("summary")?.click();
await window.happyDOM.waitUntilComplete();
objectiveSample
  ?.querySelector('button[aria-label*="Run Objective crossing"]')
  ?.click();
await window.happyDOM.waitUntilComplete();
const unitPosition = (unitId) => {
  const footprint = window.document.querySelector(
    `[aria-label="Editable simulation SVG battlefield"] [data-unit-id="${unitId}"] [data-authoritative-footprint="true"]`,
  );
  return [
    Number(footprint?.getAttribute("x")),
    Number(footprint?.getAttribute("y")),
  ];
};
const objectivePositions = new Map([
  [1, [unitPosition(1)]],
  [3, [unitPosition(3)]],
]);
for (let tick = 0; tick < 40; tick += 1) {
  buttonByText("Step")?.click();
  await window.happyDOM.waitUntilComplete();
  objectivePositions.get(1).push(unitPosition(1));
  objectivePositions.get(3).push(unitPosition(3));
}
for (const unitId of [1, 3]) {
  const positions = objectivePositions.get(unitId);
  const largestJump = positions.slice(1).reduce((largest, position, index) => {
    const previous = positions[index];
    return Math.max(
      largest,
      Math.abs(position[0] - previous[0]),
      Math.abs(position[1] - previous[1]),
    );
  }, 0);
  if (!Number.isFinite(largestJump) || largestJump > 15) {
    throw new Error(
      `Objective-crossing unit ${unitId} changed movement segment discontinuously (${largestJump}px).`,
    );
  }
}
buttonByText("Samples")?.click();
await window.happyDOM.waitUntilComplete();
const breachSample = [...window.document.querySelectorAll(".sample-card")]
  .find((card) => card.textContent.includes("Breach corridor"));
breachSample?.querySelector("summary")?.click();
await window.happyDOM.waitUntilComplete();
breachSample
  ?.querySelector('button[aria-label*="Run Breach corridor"]')
  ?.click();
await window.happyDOM.waitUntilComplete();
const breachBlueStart = unitPosition(1)[0];
const breachRedStart = unitPosition(3)[0];
for (let tick = 0; tick < 8; tick += 1) {
  buttonByText("Step")?.click();
}
await window.happyDOM.waitUntilComplete();
if (
  unitPosition(1)[0] <= breachBlueStart ||
  unitPosition(3)[0] >= breachRedStart
) {
  throw new Error("Breach units did not approach the nearest reachable side of the closed door.");
}
buttonByText("Samples")?.click();
await window.happyDOM.waitUntilComplete();
const trollReplayCard = [...window.document.querySelectorAll(".sample-card")]
  .find((card) => card.textContent.includes("Troll reaches the line"));
trollReplayCard?.querySelector("summary")?.click();
await window.happyDOM.waitUntilComplete();
trollReplayCard
  ?.querySelector('button[aria-label*="Troll reaches the line"]')
  ?.click();
await window.happyDOM.waitUntilComplete();
const sampleReplayStatus = window.document.querySelector(
  '[aria-label="Replay verification status"]',
);
if (
  !sampleReplayStatus?.textContent.includes("not authoritative") ||
  !window.document
    .querySelector('[aria-label="Replay source"]')
    ?.textContent.includes("Troll reaches the line") ||
  !window.document
    .querySelector('[aria-label="Loaded replay SVG battlefield"]')
) {
  throw new Error("The curated replay walkthrough did not open as sandbox evidence.");
}
buttonByText("Step")?.click();
await window.happyDOM.waitUntilComplete();
if (
  !window.document
    .querySelector('[aria-label="Replay controls"]')
    ?.textContent.includes("Tick 1")
) {
  throw new Error("The curated replay walkthrough did not navigate locally.");
}

console.log(
  `Browser smoke passed: dismissible desktop menus, compact Editor and Simulator shells, curated maps/simulations/replays, four canonical square-unit symbols, all controller modes, replay inspection, ${scenarioButtons.length} rules scenarios, and ${rulesTables.length} data tables completed.`,
);

window.happyDOM.close();
