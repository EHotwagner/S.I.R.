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

const controllerPanel = window.document.querySelector(
  '[aria-label="Simulation controllers"]',
);
const editorBattlefield = window.document.querySelector(
  '[aria-label="Editable simulation SVG battlefield"] svg[role="application"]',
);
if (
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
if (
  !editorWorkspace ||
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

buttonByText("Map file")?.click();
await window.happyDOM.waitUntilComplete();
if (
  !window.document.querySelector('input[aria-label="Import SIR map"]') ||
  !buttonByText("Export map")
) {
  throw new Error("The editor Map file sub-tab did not expose import and export.");
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

console.log(
  `Browser smoke passed: separate full-width Simulator and Editor tabs rendered four canonical square-unit symbols, all controller modes executed, replay inspection remained intact, and ${scenarioButtons.length} rules scenarios with ${rulesTables.length} data tables completed.`,
);

window.happyDOM.close();
