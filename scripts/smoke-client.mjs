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
  !/@media\s*\(prefers-reduced-motion:reduce\)/.test(styles) ||
  !/@media\s*\(forced-colors:active\)/.test(styles) ||
  !/\.tactical-layout-frame\{[^}]*grid-template-columns:var\(--tactical-left-width,208px\) minmax\(32rem,\s*1fr\) var\(--tactical-right-width,224px\)/.test(styles) ||
  !/@media\s*\((?:max-width:48rem|width<=48rem)\)[\s\S]*?\.tactical-layout-frame\{[^}]*grid-template-columns:minmax\(0,1fr\)/.test(styles)
) {
  throw new Error("Production shell, accessibility, or responsive safeguards are missing.");
}

const workerMessages = [];
let planningWorker;
let planningValidationCount = 0;
const reviewProjection = (tick = 0) => ({
  Tick: tick,
  BoardMinimumColumn: 0,
  BoardMinimumRow: 0,
  BoardMaximumColumn: 11,
  BoardMaximumRow: 7,
  Units: [1, 2, 3, 4].map((Id, index) => ({
    Id,
    Side: Id < 3 ? "Blue" : "Red",
    Column: index * 2,
    Row: index,
    Health: 12,
    HealthMaximum: 12,
  })),
  Edges: [
    {
      Id: "edge-0",
      Kind: "wall",
      State: "solid",
      StartColumn: 1,
      StartRow: 0,
      EndColumn: 2,
      EndRow: 0,
    },
  ],
  Events: [
    {
      Id: 0,
      Tick: 1,
      Source: "Accepted WASM output",
      Summary: "unit 1 attacks unit 3",
      SourceUnitId: 1,
      TargetUnitId: 3,
    },
  ],
  Checkpoints: [
    { Tick: 0, StateHash: "000000000000", EventHash: "000000000000" },
    { Tick: 2, StateHash: "222222222222", EventHash: "222222222222" },
  ],
  PerspectiveHash: undefined,
});
const scenarioResponse = (message) => {
  const identity = message.Request.fields[0];
  const result = {
    Input: {
      ScenarioIdentity: identity,
      ScenarioRevision: 1,
      EngineIdentity:
        "0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20",
      RulesetIdentity:
        "6d31302d72756c65732d6c61622d763100000000000000000000000000000000",
      Parameters: [
        { Key: "attack-power", Value: 25 },
        { Key: "attack-count", Value: 4 },
      ],
    },
    ResultIdentity: "0123456789abcdef",
    Metrics: [
      { Key: "attack-events", Value: 4 },
      { Key: "remaining-health", Value: 0 },
      { Key: "total-damage", Value: 100 },
    ],
  };
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
          Parameters: [
            {
              Key: "attack-power",
              Label: "Attack power",
              Minimum: 1,
              Maximum: 100,
              Step: 1,
              DefaultValue: 25,
            },
            {
              Key: "attack-count",
              Label: "Attack count",
              Minimum: 1,
              Maximum: 8,
              Step: 1,
              DefaultValue: 4,
            },
          ],
        },
        {
          Baseline: result,
          Fork: result,
          Delta: result.Metrics.map(({ Key }) => ({ Key, Value: 0 })),
          Sweep: undefined,
          EvidenceLabel: "Exploratory balance evidence — not accepted balance",
        },
        reviewProjection(0),
      ],
    },
  };
};
class SmokeWorker {
  postMessage(message) {
    workerMessages.push(message);
    if (message.Kind === "sir-simulator-session") {
      planningWorker = this;
      const requestTag = message.Request?.tag;
      let response;
      if (requestTag === 0) {
        response = {
          tag: 0,
          fields: [
            {
              IsSnapshot: true,
              Projection: message.Request.fields[0].InitialProjection,
            },
          ],
        };
      } else if (requestTag === 1) {
        planningValidationCount += 1;
        response = {
          tag: 1,
          fields:
            planningValidationCount === 1
              ? [
                  undefined,
                  [
                    {
                      Code: "SIR.PLAN.SMOKE.REVIEW",
                      Field: undefined,
                      CommandId: undefined,
                      Fields: [],
                      Detail: "Resolve the live qualification annotation before commit.",
                    },
                  ],
                ]
              : [message.Correlation.PlanRevision, []],
        };
      } else if (requestTag === 2) {
        const predicted = reviewProjection(message.Correlation.Tick);
        predicted.Units = [];
        predicted.Edges = [];
        predicted.Events = [];
        predicted.Checkpoints = [];
        response = {
          tag: 2,
          fields: [
            { tag: 2, fields: [] },
            ["Intent only: M5 authored-plan disclosure"],
            [{ IsSnapshot: true, Projection: predicted }],
          ],
        };
      } else if (requestTag === 3) {
        const progressTick = 300;
        queueMicrotask(() =>
          this.onmessage?.({
            data: structuredClone({
              Kind: message.Kind,
              ProtocolVersion: 1,
              Correlation: message.Correlation,
              CurrentTick: progressTick,
              Response: {
                tag: 5,
                fields: [
                  1,
                  {
                    IsSnapshot: false,
                    Projection: reviewProjection(progressTick),
                  },
                ],
              },
            }),
          }),
        );
        setTimeout(
          () =>
            this.onmessage?.({
              data: structuredClone({
                Kind: message.Kind,
                ProtocolVersion: 1,
                Correlation: message.Correlation,
                CurrentTick: message.Request.fields[0].HorizonTicks,
                Response: {
                  tag: 3,
                  fields: [message.Correlation.PlanRevision],
                },
              }),
            }),
          80,
        );
        return;
      }
      if (response) {
        queueMicrotask(() =>
          this.onmessage?.({
            data: structuredClone({
              Kind: message.Kind,
              ProtocolVersion: 1,
              Correlation: message.Correlation,
              CurrentTick: message.Correlation.Tick,
              Response: response,
            }),
          }),
        );
      }
      return;
    }
    if (message.Request?.tag === 0) {
      queueMicrotask(() =>
        this.onmessage?.({
          data: structuredClone({
            ProtocolVersion: 3,
            Operation: message.Operation,
            Response: {
              tag: 0,
              fields: [
                {
                  SourceName: "m3-review.sirr",
                  SourceIdentity: "m3-review",
                  EngineIdentity: "010203040506",
                  FinalTick: 2,
                  Kind: 0,
                },
                { tag: 0 },
                reviewProjection(0),
              ],
            },
          }),
        }),
      );
      return;
    }
    if (message.Request?.tag === 2) {
      const target = message.Request.fields[0];
      queueMicrotask(() =>
        this.onmessage?.({
          data: structuredClone({
            ProtocolVersion: 3,
            Operation: message.Operation,
            Response: {
              tag: 2,
              fields: [target, reviewProjection(target)],
            },
          }),
        }),
      );
      return;
    }
    if (message.Request?.tag === 4) {
      queueMicrotask(() =>
        this.onmessage?.({ data: structuredClone(scenarioResponse(message)) }),
      );
    }
  }
  terminate() {}
}

const window = new Window({ url: "https://sir.invalid/replay/" });
window.document.head.innerHTML = `<style>${styles}</style>`;
window.document.body.innerHTML = '<div id="sir-replay-app"></div>';
let confirmationCalls = 0;
window.confirm = () => {
  confirmationCalls += 1;
  return false;
};
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
const shell = window.document.querySelector(
  '#unified-tactical-workspace[data-mounted-shell="persistent"]',
);
const viewport = window.document.querySelector(
  '#tactical-battlefield-viewport[data-viewport-lifecycle="shared"]',
);
const worksurface = viewport?.querySelector(
  'svg#persistent-tactical-svg[data-work-surface-root="persistent-svg"]',
);
const layers = new Map(
  ["camera", "terrain", "edges", "routes", "units", "selection", "annotations"].map(
    (name) => [name, worksurface?.querySelector(`[data-scene-layer="${name}"]`)],
  ),
);
const timeline = shell?.querySelector('[aria-label="Unified tactical timeline"]');
const modalityControl = shell?.querySelector('[aria-label="Tactical modality"]');
const modalityButtons = new Map(
  [...(modalityControl?.querySelectorAll("button") ?? [])].map((button) => [
    button.textContent.trim(),
    button,
  ]),
);

if (
  !application ||
  !shell ||
  !viewport ||
  !worksurface ||
  !timeline ||
  [...layers.values()].some((layer) => !layer) ||
  JSON.stringify([...modalityButtons.keys()]) !==
    JSON.stringify(["Editor", "Plan", "Simulate", "Review"])
) {
  throw new Error("The persistent tactical shell, SVG layers, modality controls, or timeline did not mount.");
}

const legacyRootSelectors = [
  "#editor-map-stage",
  ".editor-map-stage",
  "#simulator-map-stage",
  ".simulator-map-stage",
  '[aria-label="SVG tactical map workspace"]',
  '[aria-label="Editable map grid"]',
  '[aria-label="Battlefield route authoring"]',
  '[aria-label="Editable simulation SVG battlefield"]',
  '[aria-label="Loaded replay SVG battlefield"]',
];
const assertSingleWorksurface = (operation) => {
  if (
    window.document.querySelector("#unified-tactical-workspace") !== shell ||
    window.document.querySelector("#tactical-battlefield-viewport") !== viewport ||
    window.document.querySelector("#persistent-tactical-svg") !== worksurface ||
    !worksurface.isConnected ||
    window.document.querySelectorAll("[data-work-surface-root]").length !== 1 ||
    window.document.querySelectorAll("svg[role='application']").length !== 1 ||
    window.document.querySelectorAll("[role='application']").length !== 1 ||
    legacyRootSelectors.some((selector) => window.document.querySelector(selector))
  ) {
    throw new Error(`Shared workscreen singleton contract failed during ${operation}.`);
  }
  for (const [name, reference] of layers) {
    if (
      !reference?.isConnected ||
      worksurface.querySelector(`[data-scene-layer="${name}"]`) !== reference
    ) {
      throw new Error(`Persistent ${name} layer identity changed during ${operation}.`);
    }
  }
};

let expectedCamera = [
  worksurface.getAttribute("data-camera-pan-x"),
  worksurface.getAttribute("data-camera-pan-y"),
  worksurface.getAttribute("data-camera-zoom"),
];
const assertCamera = (operation) => {
  const current = [
    worksurface.getAttribute("data-camera-pan-x"),
    worksurface.getAttribute("data-camera-pan-y"),
    worksurface.getAttribute("data-camera-zoom"),
  ];
  if (JSON.stringify(current) !== JSON.stringify(expectedCamera)) {
    throw new Error(`Shared camera changed during ${operation}.`);
  }
};
const assertSelection = (operation, expectedUnit) => {
  const semantic = worksurface.getAttribute("data-semantic-selection-unit");
  const primitive = expectedUnit === null ? null : `unit:${expectedUnit}`;
  if (
    semantic !== (expectedUnit === null ? "" : String(expectedUnit)) ||
    (primitive !== null &&
      (!worksurface.querySelector(`[data-unit-id="${expectedUnit}"]`) ||
        !worksurface.querySelector(`[data-selection-for="${primitive}"]`))) ||
    (primitive === null &&
      worksurface.querySelector("[data-selection-for^='unit:']"))
  ) {
    throw new Error(
      `Semantic selection reconciliation failed during ${operation}: ${semantic}.`,
    );
  }
};
const ownerByModality = {
  Editor: ["Editor", "EditorScene"],
  Plan: ["Plan", "PlanningScene"],
  Simulate: ["Simulate", "SimulatorScene"],
  Review: ["Review", "ReviewScene"],
};
const assertModality = (label, operation) => {
  const [modality, owner] = ownerByModality[label];
  if (
    modalityButtons.get(label)?.getAttribute("aria-pressed") !== "true" ||
    viewport.getAttribute("data-active-modality") !== modality ||
    worksurface.getAttribute("data-scene-owner") !== owner
  ) {
    throw new Error(
      `Directed ${operation} did not activate ${modality}/${owner}: ` +
        `${viewport.getAttribute("data-active-modality")}/${worksurface.getAttribute("data-scene-owner")}.`,
    );
  }
};
const clickModality = async (label, operation, expectedSelection = 2) => {
  const scopedButton = modalityButtons.get(label);
  if (!scopedButton || !modalityControl.contains(scopedButton)) {
    throw new Error(`Scoped modality control ${label} is unavailable.`);
  }
  scopedButton.click();
  await window.happyDOM.waitUntilComplete();
  assertModality(label, operation);
  assertSingleWorksurface(operation);
  assertCamera(operation);
  assertSelection(operation, expectedSelection);
};
const buttonByText = (container, text) =>
  [...(container?.querySelectorAll("button") ?? [])].find(
    (button) => button.textContent.trim() === text,
  );
const buttonByLabel = (container, label) =>
  container?.querySelector(`button[aria-label="${label}"]`);
const waitFor = async (description, predicate, timeoutMilliseconds = 1500) => {
  const deadline = Date.now() + timeoutMilliseconds;
  while (Date.now() < deadline) {
    if (predicate()) return;
    await new Promise((resolveWait) => setTimeout(resolveWait, 10));
  }
  throw new Error(`Timed out waiting for ${description}.`);
};

assertSingleWorksurface("initial mount");
assertModality("Editor", "initial mount");

const disclosure = viewport.querySelector(
  "details.tactical-compatibility-surface",
);
if (!disclosure) {
  throw new Error("The non-workscreen compatibility disclosure is missing.");
}
disclosure.querySelector("summary")?.click();
await window.happyDOM.waitUntilComplete();
if (!disclosure.open) throw new Error("Compatibility disclosure did not open.");
assertSingleWorksurface("compatibility disclosure open");
disclosure.querySelector("summary")?.click();
await window.happyDOM.waitUntilComplete();
if (disclosure.open) throw new Error("Compatibility disclosure did not close.");
assertSingleWorksurface("compatibility disclosure close");

const nonDefaultUnit = worksurface.querySelector(
  '#persistent-layer-units [data-unit-id="2"][data-command-available="true"]',
);
if (!nonDefaultUnit) {
  throw new Error("The non-default shared-scene unit is not availability-qualified.");
}
nonDefaultUnit.dispatchEvent(
  new window.MouseEvent("click", { bubbles: true, cancelable: true }),
);
await window.happyDOM.waitUntilComplete();
assertSelection("non-default Editor selection", 2);

// Review has no accepted frame yet: the shell-owned selection must be filtered
// out of an unavailable projection, then deterministically restored from the
// authoritative Editor owner when returning.
modalityButtons.get("Review").click();
await window.happyDOM.waitUntilComplete();
if (
  viewport.getAttribute("data-active-modality") !== "Review" ||
  worksurface.getAttribute("data-scene-owner") !== "Unavailable"
) {
  throw new Error("Empty Review did not expose its unavailable projection explicitly.");
}
assertSingleWorksurface("stale selection filtering");
assertSelection("stale selection filtering", null);
await clickModality("Editor", "return from unavailable Review");

if (!shell.querySelector('[data-panel-id="document"]')) {
  shell.querySelector("#layout-show-document")?.click();
  await window.happyDOM.waitUntilComplete();
}
const editorDocumentPanel = shell.querySelector('[data-panel-id="document"]');
if (editorDocumentPanel?.classList.contains("is-collapsed")) {
  editorDocumentPanel.querySelector("#layout-panel-document-collapse")?.click();
  await window.happyDOM.waitUntilComplete();
}
buttonByText(
  shell.querySelector('[aria-label="Map editor document actions"]'),
  "Simulate revision",
)?.click();
await window.happyDOM.waitUntilComplete();
assertModality("Simulate", "simulator handoff");
assertSingleWorksurface("simulator handoff");
assertSelection("simulator handoff", 2);
const simulatorToolbar = shell.querySelector(
  '[aria-label="Simulator quick access"]',
);
const currentSimulatorToolbar = () =>
  shell.querySelector('[aria-label="Simulator quick access"]');
if (
  !simulatorToolbar ||
  !shell.querySelector('[aria-label="Simulator menu and toolbar"]') ||
  !shell.querySelector('[aria-label="Simulator command panel"]') ||
  !shell.querySelector(".simulator-revision-status[role='status']")
) {
  throw new Error("Simulator non-workscreen controls or revision diagnostics were dropped.");
}
buttonByText(simulatorToolbar, "Controls")?.click();
await window.happyDOM.waitUntilComplete();
const controllerPanel = shell.querySelector(
  '[aria-label="Simulation controllers"]',
);
const currentControllerPanel = () =>
  shell.querySelector('[aria-label="Simulation controllers"]');
if (
  !controllerPanel?.textContent.includes("Manual") ||
  !controllerPanel.textContent.includes("Scripted AI") ||
  !controllerPanel.textContent.includes("General AI")
) {
  throw new Error("Simulator controller configuration is not reachable.");
}
const unitController = controllerPanel.querySelector("#unit-controller");
const unitScript = controllerPanel.querySelector("#unit-script");
if (!unitController || !unitScript) {
  throw new Error("Simulator native controller inputs are not reachable.");
}
const nativeKey = new window.KeyboardEvent("keydown", {
  key: "t",
  bubbles: true,
  cancelable: true,
});
unitScript.dispatchEvent(nativeKey);
await window.happyDOM.waitUntilComplete();
if (nativeKey.defaultPrevented) {
  throw new Error("Simulator native text input leaked into tactical keyboard handling.");
}
unitController.value = "Scripted AI";
unitController.dispatchEvent(new window.Event("change", { bubbles: true }));
await window.happyDOM.waitUntilComplete();
if (unitController.value !== "Scripted AI") {
  throw new Error("Simulator controller selection did not route through its registry command.");
}
buttonByLabel(currentControllerPanel(), "Move route preview right")?.click();
await window.happyDOM.waitUntilComplete();
if (
  !worksurface.querySelector("#persistent-layer-routes polyline") ||
  !currentControllerPanel()?.textContent.includes("route clear")
) {
  throw new Error("Simulator route preview did not render in the persistent routes layer.");
}
buttonByText(currentControllerPanel(), "Reset route")?.click();
await window.happyDOM.waitUntilComplete();
if (!currentControllerPanel()?.textContent.includes("Distance 0 steps")) {
  throw new Error(
    `Simulator route reset did not return the preview to its origin: ${currentControllerPanel()?.textContent}`,
  );
}
buttonByText(currentControllerPanel(), "Cancel route")?.click();
await window.happyDOM.waitUntilComplete();
if (
  worksurface.querySelector("#persistent-layer-routes polyline") ||
  !currentControllerPanel()?.textContent.includes("No route preview.")
) {
  throw new Error("Simulator route cancel did not clear the persistent route layer.");
}
const simulatorTickBefore = Number(worksurface.getAttribute("data-scene-tick"));
buttonByText(simulatorToolbar, "Step")?.click();
await window.happyDOM.waitUntilComplete();
if (
  Number(worksurface.getAttribute("data-scene-tick")) !==
  simulatorTickBefore + 1
) {
  throw new Error("Simulator Step did not update the persistent runtime projection.");
}
const simulatorTickBeforeRun = Number(
  worksurface.getAttribute("data-scene-tick"),
);
buttonByText(currentSimulatorToolbar(), "Run")?.click();
await waitFor(
  "Simulator Run state and runtime progression",
  () =>
    Boolean(buttonByText(currentSimulatorToolbar(), "Pause")) &&
    Number(worksurface.getAttribute("data-scene-tick")) > simulatorTickBeforeRun,
);
const simulatorTickWhileRunning = Number(
  worksurface.getAttribute("data-scene-tick"),
);
assertSingleWorksurface("Simulator running");
assertCamera("Simulator running");
assertSelection("Simulator running", 2);
buttonByText(currentSimulatorToolbar(), "Pause")?.click();
await waitFor(
  "Simulator paused state",
  () => Boolean(buttonByText(currentSimulatorToolbar(), "Run")),
);
const simulatorPausedTick = Number(
  worksurface.getAttribute("data-scene-tick"),
);
await new Promise((resolveWait) => setTimeout(resolveWait, 130));
if (
  simulatorPausedTick < simulatorTickWhileRunning ||
  Number(worksurface.getAttribute("data-scene-tick")) !== simulatorPausedTick
) {
  throw new Error("Simulator Pause did not stabilize the authoritative runtime tick.");
}
assertSingleWorksurface("Simulator paused");
assertCamera("Simulator paused");
assertSelection("Simulator paused", 2);
buttonByText(simulatorToolbar, "Events")?.click();
await window.happyDOM.waitUntilComplete();
if (!shell.textContent.includes("Runtime roster")) {
  throw new Error("Simulator event and runtime diagnostics are not reachable.");
}
const simulatorZoomBefore = Number(
  worksurface.getAttribute("data-camera-zoom"),
);
buttonByText(simulatorToolbar, "+")?.click();
await window.happyDOM.waitUntilComplete();
if (
  Number(worksurface.getAttribute("data-camera-zoom")) <= simulatorZoomBefore
) {
  throw new Error("Simulator camera alternative did not route to the shared camera.");
}
buttonByText(simulatorToolbar, "Fit")?.click();
await window.happyDOM.waitUntilComplete();
expectedCamera = [
  worksurface.getAttribute("data-camera-pan-x"),
  worksurface.getAttribute("data-camera-pan-y"),
  worksurface.getAttribute("data-camera-zoom"),
];
// Reset is availability-guarded and uses the existing explicit confirmation.
buttonByText(simulatorToolbar, "Reset")?.click();
await window.happyDOM.waitUntilComplete();
if (confirmationCalls !== 1) {
  throw new Error("Simulator reset confirmation workflow is unreachable.");
}
await clickModality("Editor", "return after simulator handoff");

modalityButtons.get("Review").click();
await window.happyDOM.waitUntilComplete();
if (
  viewport.getAttribute("data-active-modality") !== "Review" ||
  worksurface.getAttribute("data-scene-owner") !== "Unavailable"
) {
  throw new Error("Review file setup did not remain on its unavailable projection.");
}
assertSingleWorksurface("open Review for accepted file");
assertSelection("open Review for accepted file", null);
const replayInput = shell.querySelector('input[type="file"]');
if (!replayInput) {
  throw new Error("Review did not expose the full-replay file boundary.");
}
const replayFile = new window.File(
  [new Uint8Array([1, 2, 3, 4])],
  "m3-review.sirr",
  { type: "application/octet-stream" },
);
Object.defineProperty(replayInput, "files", {
  configurable: true,
  value: {
    0: replayFile,
    length: 1,
    item: (index) => (index === 0 ? replayFile : null),
    [Symbol.iterator]: function* () {
      yield replayFile;
    },
  },
});
replayInput.dispatchEvent(new window.Event("change", { bubbles: true }));
await new Promise((resolveWait) => setTimeout(resolveWait, 20));
await window.happyDOM.waitUntilComplete();
assertModality("Review", "accepted review sample");
assertSingleWorksurface("accepted review sample");
const reviewUnit = worksurface.querySelector(
  '#persistent-layer-units [data-unit-id="2"][data-command-available="true"]',
);
if (!reviewUnit) {
  throw new Error("Accepted Review did not disclose the common non-default unit.");
}
reviewUnit.dispatchEvent(
  new window.MouseEvent("click", { bubbles: true, cancelable: true }),
);
await window.happyDOM.waitUntilComplete();
assertSelection("accepted review sample", 2);
if (
  !shell.querySelector('[aria-label="Replay source"]') ||
  !shell.querySelector('[aria-label="Replay controls"]') ||
  !shell.querySelector('[aria-label="Replay inspector"]') ||
  !shell.querySelector('[aria-label="Replay verification status"]')
) {
  throw new Error("Review source, transport, verification, or inspector capability was dropped.");
}
const replayControls = shell.querySelector('[aria-label="Replay controls"]');
if (
  worksurface.querySelectorAll("#persistent-layer-units [data-unit-id]").length !== 4 ||
  worksurface.querySelectorAll("#persistent-layer-edges line").length !== 1 ||
  !worksurface
    .querySelector("#persistent-layer-annotations")
    ?.textContent.includes("unit 1 attacks unit 3") ||
  replayControls?.querySelectorAll(
    '[aria-label^="Seek to checkpoint at tick"]',
  ).length !== 2 ||
  buttonByText(replayControls, "Previous event")?.disabled ||
  buttonByText(replayControls, "Next event")?.disabled
) {
  throw new Error("Accepted Review did not present edges, events, checkpoints, and event transport.");
}
buttonByText(replayControls, "Step")?.click();
await window.happyDOM.waitUntilComplete();
if (
  worksurface.getAttribute("data-scene-tick") !== "1" ||
  !workerMessages.some(
    (message) => message.Request?.tag === 2 && message.Request.fields[0] === 1,
  )
) {
  throw new Error("Replay Step did not seek the accepted projection through the worker.");
}
assertSingleWorksurface("accepted replay step");
assertSelection("accepted replay step", 2);
await clickModality("Editor", "return after accepted review sample");

for (const source of Object.keys(ownerByModality)) {
  await clickModality(source, `activate ${source}`);
  for (const target of Object.keys(ownerByModality)) {
    if (target === source) continue;
    await clickModality(target, `${source} to ${target}`);
    await clickModality(source, `${source} return from ${target}`);
  }
}

await clickModality("Plan", "Plan playback setup");
for (const panelId of ["roster", "tools", "selection", "validation", "document"]) {
  if (!shell.querySelector(`[data-panel-id="${panelId}"]`)) {
    shell.querySelector(`#layout-show-${panelId}`)?.click();
    await window.happyDOM.waitUntilComplete();
  }
  const panel = shell.querySelector(`[data-panel-id="${panelId}"]`);
  if (panel?.classList.contains("is-collapsed")) {
    panel.querySelector(`#layout-panel-${panelId}-collapse`)?.click();
    await window.happyDOM.waitUntilComplete();
  }
}
if (
  !shell.querySelector('[aria-label^="Planning roster"]') ||
  !shell.querySelector('[aria-label="Planning inspector"]') ||
  !shell.querySelector('[aria-label="Planning validation navigation"]') ||
  !shell.querySelector('[aria-label="Planning revision state"]') ||
  !buttonByText(shell, "Preview") ||
  !buttonByText(shell, "Validate") ||
  shell.querySelectorAll('[aria-label^="Planning roster"]').length !== 1 ||
  shell.querySelectorAll('[aria-label="Battlefield planning tools"]').length !== 1 ||
  shell.querySelectorAll('[aria-label="Planning inspector"]').length !== 1 ||
  shell.querySelectorAll('[aria-label="Planning validation navigation"]').length !== 1 ||
  shell.querySelectorAll('[aria-label="Planning revision state"]').length !== 1 ||
  shell.querySelectorAll(".planning-worker-status").length !== 1 ||
  shell.querySelector(".planning-owner-status") ||
  shell.querySelector(".planning-workspace") ||
  shell.querySelector('[aria-label="Battlefield route authoring"]')
) {
  throw new Error("Registered Plan panels are incomplete, duplicated, or accompanied by a legacy page renderer.");
}
const currentPlanningTools = () =>
  shell.querySelector('[aria-label="Battlefield planning tools"]');
const currentPlanningTimeline = () =>
  shell.querySelector('[aria-label="Unified tactical timeline"]');
const currentPlanningInspector = () =>
  shell.querySelector('[aria-label="Planning inspector"]');
const currentPlanningValidation = () =>
  shell.querySelector('[aria-label="Planning validation navigation"]');
const currentPlanningRevision = () =>
  shell.querySelector('[aria-label="Planning revision state"]');
const planningChannel = (label) =>
  [...(currentPlanningRevision()?.querySelectorAll(".planning-status > div") ?? [])]
    .find((item) => item.querySelector(".eyebrow")?.textContent.trim() === label)
    ?.querySelector("strong")?.textContent.trim();

worksurface
  .querySelector('#persistent-layer-units [data-unit-id="2"][data-command-available="true"]')
  ?.dispatchEvent(new window.MouseEvent("click", { bubbles: true, cancelable: true }));
await window.happyDOM.waitUntilComplete();
assertSelection("Plan roster selection", 2);
const authoredBeforeRoute = planningChannel("Authored");
const planningCell = worksurface.querySelector(
  '#persistent-layer-terrain [aria-label="Activate cell 4,4"][data-command-available="true"]',
);
if (!planningCell) {
  throw new Error("Plan did not expose an availability-qualified shared-scene cell command.");
}
planningCell.dispatchEvent(
  new window.MouseEvent("click", { bubbles: true, cancelable: true }),
);
await window.happyDOM.waitUntilComplete();
const authoredAfterRoute = planningChannel("Authored");
const routeRevisionIdentity = worksurface.getAttribute("data-scene-revision");
const routeGeometry = worksurface
  .querySelector("#persistent-layer-routes polyline")
  ?.getAttribute("points");
if (
  !authoredAfterRoute ||
  authoredAfterRoute === authoredBeforeRoute ||
  !currentPlanningTimeline()?.textContent.includes("Route") ||
  !worksurface.querySelector("#persistent-layer-routes polyline")
) {
  throw new Error("Shared registry cell activation did not author a routed Plan revision.");
}
assertSingleWorksurface("Plan route authored");
assertCamera("Plan route authored");
assertSelection("Plan route authored", 2);

buttonByText(currentPlanningTools(), "Undo")?.click();
await window.happyDOM.waitUntilComplete();
if (
  currentPlanningTimeline()?.textContent.includes("Route") ||
  worksurface.querySelector("#persistent-layer-routes polyline")
) {
  throw new Error("Planning Undo did not remove the authoritative authored route.");
}
buttonByText(currentPlanningTools(), "Redo")?.click();
await window.happyDOM.waitUntilComplete();
if (
  !currentPlanningTimeline()?.textContent.includes("Route") ||
  !worksurface.querySelector("#persistent-layer-routes polyline") ||
  planningChannel("Authored") !== authoredAfterRoute ||
  worksurface.getAttribute("data-scene-revision") !== routeRevisionIdentity ||
  worksurface.querySelector("#persistent-layer-routes polyline")?.getAttribute("points") !== routeGeometry
) {
  throw new Error("Planning Redo did not restore the exact authored revision identity and shared route geometry.");
}
const routeCommand = [...currentPlanningInspector().querySelectorAll("button")].find(
  (button) => button.textContent.includes("Route"),
);
routeCommand?.click();
await window.happyDOM.waitUntilComplete();

const choosePlanningTool = async (label) => {
  const target = [...currentPlanningTools().querySelectorAll("button")].find((button) =>
    button.textContent.startsWith(label),
  );
  if (!target) throw new Error(`Planning tool ${label} is not panel-owned.`);
  target.click();
  await window.happyDOM.waitUntilComplete();
};
const planningInspectorAction = async (label, startsWith = false) => {
  const target = [...currentPlanningInspector().querySelectorAll("button")].find((button) =>
    startsWith
      ? button.textContent.startsWith(label)
      : button.textContent.trim() === label,
  );
  if (!target) throw new Error(`Planning inspector action ${label} is not reachable.`);
  target.click();
  await window.happyDOM.waitUntilComplete();
};
await choosePlanningTool("Facing");
await planningInspectorAction("E");
await choosePlanningTool("Attention");
await planningInspectorAction("NE");
await choosePlanningTool("Stance");
await planningInspectorAction("Crouched");
await choosePlanningTool("Hold");
await planningInspectorAction("Add hold");
await choosePlanningTool("Engage");
await planningInspectorAction("Engage ", true);
await choosePlanningTool("Sync");
await planningInspectorAction("Add synchronization marker");
const selectedPlanningUnit = worksurface.querySelector('#persistent-layer-units [data-unit-id="2"]');
for (const kind of ["facing", "attention", "stance", "hold", "engagement", "synchronization"]) {
  if (!worksurface.querySelector(`#persistent-layer-annotations [data-annotation-kind="${kind}"]`)) {
    throw new Error(`Authored ${kind} was not projected through the shared annotation layer.`);
  }
}
if (
  selectedPlanningUnit?.getAttribute("data-unit-stance") !== "crouched" ||
  !selectedPlanningUnit.querySelector('[data-unit-heading="facing"]') ||
  !selectedPlanningUnit.querySelector('[data-unit-heading="attention"]') ||
  !selectedPlanningUnit.getAttribute("data-unit-status")?.includes("hold") ||
  !selectedPlanningUnit.getAttribute("data-unit-status")?.includes("engagement") ||
  !selectedPlanningUnit.getAttribute("data-unit-status")?.includes("synchronization")
) {
  throw new Error(
    `Authored unit semantics did not enrich the shared unit layer (stance=${selectedPlanningUnit?.getAttribute("data-unit-stance")}; headings=${selectedPlanningUnit?.querySelectorAll("[data-unit-heading]").length}; status=${selectedPlanningUnit?.getAttribute("data-unit-status")}).`,
  );
}

const staleCorrelation = workerMessages.find(
  (message) => message.Kind === "sir-simulator-session" && message.Request?.tag === 0,
)?.Correlation;
const authoredBeforeStale = planningChannel("Authored");
planningWorker?.onmessage?.({
  data: structuredClone({
    Kind: "sir-simulator-session",
    ProtocolVersion: 1,
    Correlation: staleCorrelation,
    CurrentTick: 0,
    Response: { tag: 1, fields: [staleCorrelation?.PlanRevision, []] },
  }),
});
await window.happyDOM.waitUntilComplete();
if (
  planningChannel("Authored") !== authoredBeforeStale ||
  planningChannel("Accepted") !== "Not validated"
) {
  throw new Error("A stale planning worker response crossed the exact revision boundary.");
}

const validateButton = buttonByText(currentPlanningTools(), "Validate");
if (!validateButton?.disabled) {
  throw new Error("Validate became available before the required intent-only Preview step.");
}
const previewButton = buttonByText(currentPlanningTools(), "Preview");
if (!previewButton || previewButton.disabled) {
  throw new Error("Preview is unavailable at an editable authored revision.");
}
previewButton.click();
await waitFor(
  "intent-only planning prediction",
  () => planningChannel("Predicted") !== "Not previewed",
);
if (
  worksurface.querySelector('#persistent-layer-routes [data-route-kind="predicted"]') ||
  !worksurface.querySelector('#persistent-layer-annotations [data-annotation-kind="prediction"]') ||
  !worksurface.querySelector('#persistent-layer-annotations [data-annotation-kind="prediction"]')?.textContent.includes("Intent only:")
) {
  throw new Error(
    `Intent-only disclosure was not projected truthfully as annotation-only prediction (routes=${worksurface.querySelectorAll('#persistent-layer-routes [data-route-kind="predicted"]').length}; annotations=${worksurface.querySelectorAll('#persistent-layer-annotations [data-annotation-kind="prediction"]').length}).`,
  );
}
if (buttonByText(currentPlanningTools(), "Validate")?.disabled) {
  throw new Error("Validate did not become available after Preview for the exact authored revision.");
}
const previewRequest = [...workerMessages].reverse().find(
  (message) => message.Kind === "sir-simulator-session" && message.Request?.tag === 2,
);
buttonByText(currentPlanningTools(), "Validate")?.click();
planningWorker?.onmessage?.({
  data: structuredClone({
    Kind: "sir-simulator-session",
    ProtocolVersion: 1,
    Correlation: previewRequest?.Correlation,
    CurrentTick: previewRequest?.Correlation?.Tick ?? 0,
    Response: {
      tag: 2,
      fields: [
        { tag: 2, fields: [] },
        ["OUT_OF_ORDER_MUST_REJECT"],
        [
          {
            IsSnapshot: true,
            Projection: {
              ...reviewProjection(0),
              Units: [],
              Edges: [],
              Events: [],
              Checkpoints: [],
            },
          },
        ],
      ],
    },
  }),
});
await waitFor(
  "planning validation annotation",
  () =>
    currentPlanningValidation()?.textContent.includes("SIR.PLAN.SMOKE.REVIEW") &&
    Boolean(worksurface.querySelector('#persistent-layer-annotations [data-annotation-kind="validation"]')),
);
if (worksurface.textContent.includes("OUT_OF_ORDER_MUST_REJECT")) {
  throw new Error("A superseded same-revision preview response overwrote the active validation request.");
}
if (planningChannel("Accepted") !== "Not validated") {
  throw new Error("A plan with live validation issues was accepted for commit.");
}
const issueRevision = planningChannel("Authored");
buttonByText(currentPlanningInspector(), "Remove selected command")?.click();
await window.happyDOM.waitUntilComplete();
if (
  planningChannel("Authored") === issueRevision ||
  worksurface.querySelector('#persistent-layer-annotations [data-annotation-kind="validation"]') ||
  planningChannel("Predicted") !== "Not previewed" ||
  !buttonByText(currentPlanningTools(), "Validate")?.disabled
) {
  throw new Error("Changing the diagnosed authored document did not invalidate its issue, prediction, and validation availability.");
}
buttonByText(currentPlanningTools(), "Preview")?.click();
await waitFor(
  "changed authored revision preview",
  () => planningChannel("Predicted") !== "Not previewed",
);
buttonByText(currentPlanningTools(), "Validate")?.click();
await waitFor(
  "worker-accepted planning revision",
  () =>
    planningChannel("Accepted") === planningChannel("Authored") &&
    currentPlanningRevision()?.textContent.includes(
      "Revision accepted by worker validation",
    ) &&
    !worksurface.querySelector('#persistent-layer-annotations [data-annotation-kind="validation"]'),
);
if (
  !timeline.querySelector('[data-time-channel="Accepted"]') ||
  !workerMessages.some(
    (message) =>
      message.Kind === "sir-simulator-session" && message.Request?.tag === 1,
  )
) {
  throw new Error("Validated Plan did not project its Accepted channel or worker request.");
}
assertSingleWorksurface("Plan accepted");

await choosePlanningTool("Route");
buttonByText(currentPlanningTools(), "Commit")?.click();
await waitFor(
  "worker-committed planning revision",
  () =>
    planningChannel("Committed")?.startsWith(planningChannel("Authored")) &&
    currentPlanningRevision()?.textContent.includes(
      "Plan committed to simulator session",
    ),
);
if (
  !timeline.querySelector('[data-time-channel="Committed"]') ||
  Number(timeline.getAttribute("data-committed-through")) <= 0 ||
  !workerMessages.some(
    (message) =>
      message.Kind === "sir-simulator-session" && message.Request?.tag === 3,
  )
) {
  throw new Error("Committed Plan did not cross the worker and shared timeline boundary.");
}
assertSingleWorksurface("Plan committed");
assertCamera("Plan committed");
assertSelection("Plan committed", 2);

const committedPlanSnapshot = {
  authored: planningChannel("Authored"),
  committed: planningChannel("Committed"),
  timeline: currentPlanningTimeline().textContent,
  revision: worksurface.getAttribute("data-scene-revision"),
  route: worksurface
    .querySelector("#persistent-layer-routes polyline")
    ?.getAttribute("points"),
};
const assertCommittedPlanSnapshot = (operation) => {
  if (
    planningChannel("Authored") !== committedPlanSnapshot.authored ||
    planningChannel("Committed") !== committedPlanSnapshot.committed ||
    currentPlanningTimeline().textContent !== committedPlanSnapshot.timeline ||
    worksurface.getAttribute("data-scene-revision") !==
      committedPlanSnapshot.revision ||
    worksurface
      .querySelector("#persistent-layer-routes polyline")
      ?.getAttribute("points") !== committedPlanSnapshot.route
  ) {
    throw new Error(`${operation} mutated the committed planning interval.`);
  }
  assertSingleWorksurface(`Plan committed lock: ${operation}`);
  assertCamera(`Plan committed lock: ${operation}`);
  assertSelection(`Plan committed lock: ${operation}`, 2);
};
const committedMutationTargets = [
  ["shared scene cell", planningCell],
  [
    "Waypoint east",
    buttonByText(currentPlanningInspector(), "Waypoint east"),
  ],
  ["Undo", buttonByText(currentPlanningTools(), "Undo")],
  ["Redo", buttonByText(currentPlanningTools(), "Redo")],
  [
    "Remove selected command",
    buttonByText(currentPlanningInspector(), "Remove selected command"),
  ],
];
for (const [label, target] of committedMutationTargets) {
  if (!target) {
    throw new Error(`Committed Plan mutation target is missing: ${label}.`);
  }
  if (typeof target.click === "function") {
    target.click();
  } else {
    target.dispatchEvent(
      new window.MouseEvent("click", { bubbles: true, cancelable: true }),
    );
  }
  await window.happyDOM.waitUntilComplete();
  assertCommittedPlanSnapshot(label);
}

const play = buttonByText(timeline, "Play");
if (!play) throw new Error("The unified timeline did not expose Plan playback.");
play.click();
await new Promise((resolveWait) => setTimeout(resolveWait, 130));
await window.happyDOM.waitUntilComplete();
buttonByText(timeline, "Pause")?.click();
await window.happyDOM.waitUntilComplete();
assertSingleWorksurface("Plan playback update");
assertCamera("Plan playback update");
assertSelection("Plan playback update", 2);

const helpKey = new window.KeyboardEvent("keydown", {
  key: "?",
  shiftKey: true,
  bubbles: true,
  cancelable: true,
});
worksurface.dispatchEvent(helpKey);
await window.happyDOM.waitUntilComplete();
if (!helpKey.defaultPrevented || !window.document.querySelector("#tactical-input-panel")) {
  throw new Error("Shared SVG keyboard intent bypassed registry availability.");
}
assertSingleWorksurface("context-help overlay");

window.document.querySelector("#layout-panel-tools-collapse")?.click();
await window.happyDOM.waitUntilComplete();
const toolsPanel = window.document.querySelector('[data-panel-id="tools"]');
buttonByText(toolsPanel, "→")?.click();
await window.happyDOM.waitUntilComplete();
const movedTools = window.document.querySelector('[data-panel-id="tools"]');
[...movedTools.querySelectorAll("button")]
  .find((button) => button.getAttribute("aria-label") === "Move Tools panel up")
  ?.click();
await window.happyDOM.waitUntilComplete();
[...window.document.querySelectorAll('[data-panel-id="tools"] button')]
  .find((button) => button.getAttribute("aria-label") === "Hide Tools panel")
  ?.click();
await window.happyDOM.waitUntilComplete();
window.document.querySelector("#layout-show-tools")?.click();
await window.happyDOM.waitUntilComplete();
window.document.querySelector("#layout-timeline-toggle")?.click();
await window.happyDOM.waitUntilComplete();
window.document.querySelector("#layout-timeline-visibility-toggle")?.click();
await window.happyDOM.waitUntilComplete();
window.document.querySelector("#layout-timeline-visibility-toggle")?.click();
await window.happyDOM.waitUntilComplete();
window.happyDOM.setInnerWidth(600);
assertSingleWorksurface("panel, timeline, and responsive changes");
assertCamera("panel, timeline, and responsive changes");
assertSelection("panel, timeline, and responsive changes", 2);

if (!workerMessages.some((message) => message.Kind === "sir-simulator-session")) {
  throw new Error("Plan modality did not initialize through the real worker boundary.");
}

buttonByText(application, "Rules and data")?.click();
await window.happyDOM.waitUntilComplete();
const scenarioCatalog = window.document.querySelector(
  '[aria-label="Design scenario catalog"]',
);
const scenarioButtons = scenarioCatalog?.querySelectorAll(
  'button[aria-label^="Simulate design scenario"]',
);
const rulesTables = window.document.querySelectorAll(
  '[aria-label="Rules data tables"] table',
);
if (scenarioButtons?.length !== 6 || rulesTables.length !== 7) {
  throw new Error(
    `Rules workspace lost runnable scenarios or data tables: ${scenarioButtons?.length}/${rulesTables.length}.`,
  );
}
scenarioButtons[0].click();
await window.happyDOM.waitUntilComplete();
if (!workerMessages.some((message) => message.Request?.tag === 4)) {
  throw new Error("Rules scenario execution did not cross the worker boundary.");
}

buttonByText(application, "Samples")?.click();
await window.happyDOM.waitUntilComplete();
const samplesWorkspace = window.document.querySelector(
  '[aria-label="Curated maps simulations and replays"]',
);
const sampleKinds = [
  ...(samplesWorkspace?.querySelectorAll(".sample-kind") ?? []),
].map((item) => item.textContent.trim());
if (
  !samplesWorkspace ||
  sampleKinds.filter((kind) => kind === "Map · Simulation").length !== 3 ||
  sampleKinds.filter((kind) => kind === "Replay").length !== 2
) {
  throw new Error("Curated map, simulation, and replay sample coverage is incomplete.");
}
const firstMapSample = [
  ...samplesWorkspace.querySelectorAll("details.sample-card"),
].find((card) => card.textContent.includes("Map · Simulation"));
firstMapSample?.querySelector("summary")?.click();
await window.happyDOM.waitUntilComplete();
buttonByText(firstMapSample, "Open map")?.click();
await window.happyDOM.waitUntilComplete();
const reopenedShell = window.document.querySelector(
  '#unified-tactical-workspace[data-mounted-shell="persistent"]',
);
const reopenedSvg = window.document.querySelector(
  'svg#persistent-tactical-svg[data-work-surface-root="persistent-svg"]',
);
if (
  !reopenedShell ||
  !reopenedSvg ||
  reopenedSvg.getAttribute("data-scene-owner") !== "EditorScene" ||
  reopenedSvg.querySelectorAll("[data-scene-layer]").length !== 7 ||
  !reopenedShell.querySelector('[data-panel-id="tools"]') ||
  !reopenedShell.querySelector('[data-panel-id="document"]')
) {
  throw new Error("Opening a curated map did not return to the persistent Editor workscreen.");
}

console.log(
  "Browser smoke passed: the retained SVG survived tactical transitions and broad Editor/Plan/Simulator/Review workflows; Rules and Samples remained fully reachable.",
);
