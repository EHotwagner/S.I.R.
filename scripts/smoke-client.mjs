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

if (
  !/@media\s*\(prefers-reduced-motion:reduce\)/.test(styles) ||
  !/@media\s*\(forced-colors:active\)/.test(styles) ||
  !/@media\s*\((?:max-width:48rem|width<=48rem)\)[\s\S]*?\.modal-input-panel\{[^}]*position:static/.test(
    styles,
  ) ||
  !/\.modal-input-toggle\{[^}]*min-height:2\.75rem/.test(styles)
) {
  throw new Error(
    "Modal input acceptance styles lost reduced-motion, forced-colors, 400%-reflow, or 44px target-size safeguards.",
  );
}
if (
  !/\.tactical-layout-frame\{[^}]*grid-template-columns:var\(--tactical-left-width,208px\) minmax\(32rem,\s*1fr\) var\(--tactical-right-width,224px\)/.test(
    styles,
  ) ||
  !/\.tactical-drawer-toggle\{display:none\}/.test(styles) ||
  !/@media\s*\((?:max-width:48rem|width<=48rem)\)[\s\S]*?\.tactical-drawer-toggle\{display:inline-flex\}/.test(
    styles,
  ) ||
  !/@media\s*\((?:max-width:48rem|width<=48rem)\)[\s\S]*?\.tactical-layout-frame\{[^}]*grid-template-columns:minmax\(0,1fr\)/.test(
    styles,
  ) ||
  !/@media\s*\((?:max-width:48rem|width<=48rem)\)[\s\S]*?\.tactical-sidebar\{[^}]*visibility:hidden[^}]*pointer-events:none/.test(
    styles,
  ) ||
  !/@media\s*\((?:max-width:48rem|width<=48rem)\)[\s\S]*?\.tactical-sidebar\.is-drawer-open\{[^}]*visibility:visible[^}]*pointer-events:auto[^}]*transform:translate(?:X)?\(0\)/.test(
    styles,
  )
) {
  throw new Error(
    "Field Focus desktop columns or responsive sidebar drawer safeguards are missing.",
  );
}

const window = new Window({ url: "https://sir.invalid/replay/" });
window.document.head.innerHTML = `<style>${styles}</style>`;
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
    if (message.Kind === "sir-simulator-session") {
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
        response = {
          tag: 1,
          fields: [message.Correlation.PlanRevision, []],
        };
      } else if (requestTag === 2) {
        response = {
          tag: 2,
          fields: [{ tag: 2, fields: [] }, ["intent-only smoke disclosure"], []],
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
                    Projection: replayProjection(progressTick, true),
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
        const currentTick =
          requestTag === 3 ? message.Request.fields[0].HorizonTicks : message.Correlation.Tick;
        queueMicrotask(() =>
          this.onmessage?.({
            data: structuredClone({
              Kind: message.Kind,
              ProtocolVersion: 1,
              Correlation: message.Correlation,
              CurrentTick: currentTick,
              Response: response,
            }),
          }),
        );
      }
    } else if (
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

const bundle = resolve(output, scriptMatch[1].replace(/^\.\//, ""));
const installBrowserGlobals = (browserWindow) => Object.assign(globalThis, {
  window: browserWindow,
  document: browserWindow.document,
  Node: browserWindow.Node,
  Element: browserWindow.Element,
  HTMLElement: browserWindow.HTMLElement,
  Event: browserWindow.Event,
  KeyboardEvent: browserWindow.KeyboardEvent,
  MouseEvent: browserWindow.MouseEvent,
  Worker: SmokeWorker,
});

const fieldFocusProfile = {
  schemaVersion: 1,
  placements: [
    { panelId: "roster", side: "left", order: 0, visible: true, collapsed: false },
    { panelId: "tools", side: "left", order: 1, visible: true, collapsed: false },
    { panelId: "layers", side: "left", order: 2, visible: true, collapsed: true },
    { panelId: "samples", side: "left", order: 3, visible: false, collapsed: false },
    { panelId: "selection", side: "right", order: 0, visible: true, collapsed: false },
    { panelId: "validation", side: "right", order: 1, visible: true, collapsed: true },
    { panelId: "document", side: "right", order: 2, visible: true, collapsed: true },
    { panelId: "rules", side: "right", order: 3, visible: false, collapsed: false },
    { panelId: "data", side: "right", order: 4, visible: false, collapsed: false },
    { panelId: "diagnostics", side: "right", order: 5, visible: false, collapsed: true },
  ],
  leftSidebar: { width: 208, drawerOpen: false },
  rightSidebar: { width: 224, drawerOpen: false },
  bottomPanel: {
    visible: true,
    height: 152,
    collapsedInEditor: true,
    collapsedOutsideEditor: false,
  },
};

const mountLayoutCase = async (name, storedProfile) => {
  const isolated = new Window({ url: `https://sir.invalid/layout-${name}/` });
  isolated.document.head.innerHTML = `<style>${styles}</style>`;
  isolated.document.body.innerHTML = '<div id="sir-replay-app"></div>';
  isolated.localStorage.setItem("sir.tactical-layout.v1", storedProfile);
  installBrowserGlobals(isolated);
  await import(`${pathToFileURL(bundle).href}?layout-case=${name}`);
  await isolated.happyDOM.waitUntilComplete();
  return isolated;
};

const customizedProfile = structuredClone(fieldFocusProfile);
customizedProfile.leftSidebar.width = 260;
customizedProfile.rightSidebar.width = 280;
customizedProfile.bottomPanel.visible = false;
customizedProfile.bottomPanel.collapsedInEditor = false;
const customizedLayoutWindow = await mountLayoutCase(
  "customized",
  JSON.stringify(customizedProfile),
);
const customizedShell = customizedLayoutWindow.document.querySelector(
  '[aria-label="Unified tactical workspace"]',
);
if (
  customizedShell?.style.getPropertyValue("--tactical-left-width") !== "260px" ||
  customizedShell?.style.getPropertyValue("--tactical-right-width") !== "280px" ||
  customizedLayoutWindow.document.querySelector("#tactical-bottom-panel") ||
  customizedLayoutWindow.document
    .querySelector("#layout-timeline-visibility-toggle")
    ?.getAttribute("aria-pressed") !== "false" ||
  !customizedLayoutWindow.document
    .querySelector("#layout-timeline-toggle")
    ?.hasAttribute("disabled")
) {
  throw new Error(
    "A fresh mount did not apply customized persisted dimensions and hidden bottom-panel state.",
  );
}
customizedLayoutWindow.close();

for (const [name, storedProfile] of [
  ["malformed", '{"schemaVersion":1,}'],
  [
    "future",
    JSON.stringify({ ...fieldFocusProfile, schemaVersion: 99 }),
  ],
]) {
  const fallbackWindow = await mountLayoutCase(name, storedProfile);
  const fallbackShell = fallbackWindow.document.querySelector(
    '[aria-label="Unified tactical workspace"]',
  );
  const canonicalFallback = JSON.parse(
    fallbackWindow.localStorage.getItem("sir.tactical-layout.v1"),
  );
  if (
    fallbackShell?.style.getPropertyValue("--tactical-left-width") !== "208px" ||
    !fallbackWindow.document.querySelector("#tactical-bottom-panel") ||
    !fallbackWindow.document
      .querySelector(".tactical-layout-diagnostics")
      ?.textContent.includes("Field Focus was restored") ||
    canonicalFallback.schemaVersion !== 1 ||
    canonicalFallback.bottomPanel.visible !== true
  ) {
    throw new Error(
      `The ${name} persisted layout did not fail closed to diagnostic, canonical Field Focus state.`,
    );
  }
  fallbackWindow.close();
}

installBrowserGlobals(window);
await import(pathToFileURL(bundle));
await window.happyDOM.waitUntilComplete();

const application = window.document.querySelector(
  'main[aria-label="S.I.R. simulator and editor"]',
);

if (!application || application.querySelector("h1")) {
  throw new Error("The React simulator did not mount.");
}

const persistentTacticalShell = window.document.querySelector(
  '[aria-label="Unified tactical workspace"][data-mounted-shell="persistent"]',
);
const persistentBattlefieldViewport = window.document.querySelector(
  '#tactical-battlefield-viewport[data-viewport-lifecycle="shared"]',
);
// Milestone 0 deliberately keeps this stronger characterization opt-in while
// the known defect exists. The normal smoke gate documents the current
// wrapper contract; `npm run characterize:persistent-workscreen` turns on the
// successor SVG-root contract and is expected to fail until Milestone 3.
const characterizePersistentWorkscreen =
  process.env.SIR_CHARACTERIZE_PERSISTENT_WORKSCREEN === "1";
const initialEditorWorksurface =
  persistentBattlefieldViewport?.querySelector("svg#editor-map-stage");
const initialTimeline = persistentTacticalShell?.querySelector(
  '[aria-label="Unified tactical timeline"]',
);
const modalityLabels = [
  ...(persistentTacticalShell?.querySelectorAll(
    '[aria-label="Tactical modality"] button',
  ) ?? []),
].map((button) => button.textContent.trim());
if (
  !persistentTacticalShell ||
  !persistentBattlefieldViewport ||
  !initialEditorWorksurface ||
  !initialEditorWorksurface.isConnected ||
  !persistentBattlefieldViewport.contains(initialEditorWorksurface) ||
  window.document.querySelectorAll("#tactical-battlefield-viewport").length !== 1 ||
  !initialTimeline ||
  JSON.stringify(modalityLabels) !==
    JSON.stringify(["Editor", "Plan", "Simulate", "Review"]) ||
  initialTimeline.querySelectorAll('input[type="range"]').length !== 1
) {
  throw new Error(
    "The mounted tactical shell, four native modality controls, or unified time ruler is missing.",
  );
}
const compactToolbar = persistentTacticalShell.querySelector(
  '[aria-label="Tactical workspace toolbar"]',
);
const leftSidebar = persistentTacticalShell.querySelector(
  '#tactical-sidebar-left[aria-label="Left tactical sidebar"]',
);
const rightSidebar = persistentTacticalShell.querySelector(
  '#tactical-sidebar-right[aria-label="Right tactical sidebar"]',
);
const bottomPanel = persistentTacticalShell.querySelector(
  '#tactical-bottom-panel[aria-label="Tactical bottom panel"]',
);
if (
  !compactToolbar ||
  leftSidebar?.querySelectorAll("[data-panel-id]").length !== 3 ||
  rightSidebar?.querySelectorAll("[data-panel-id]").length !== 3 ||
  !bottomPanel?.classList.contains("is-collapsed") ||
  persistentTacticalShell.style.getPropertyValue("--tactical-left-width") !==
    "208px" ||
  persistentTacticalShell.style.getPropertyValue("--tactical-right-width") !==
    "224px" ||
  persistentTacticalShell.style.getPropertyValue("--tactical-bottom-height") !==
    "152px"
) {
  throw new Error(
    "Field Focus did not mount a compact toolbar, narrow 3+3 sidebars, dominant workscreen frame, and shallow collapsed Editor timeline.",
  );
}

const toolsCollapse = window.document.querySelector(
  "#layout-panel-tools-collapse",
);
toolsCollapse?.click();
await window.happyDOM.waitUntilComplete();
if (
  window.document.querySelector("#layout-panel-tools-collapse")?.getAttribute(
    "aria-expanded",
  ) !== "false" ||
  window.document.activeElement?.id !== "layout-panel-tools-collapse"
) {
  throw new Error("Panel collapse did not update state and restore header focus.");
}
[
  ...window.document.querySelectorAll("#layout-panel-tools button"),
]
  .find((button) => button.getAttribute("aria-label") === "Move Tools panel to right sidebar")
  ?.click();
await window.happyDOM.waitUntilComplete();
const movedTools = window.document.querySelector('[data-panel-id="tools"]');
if (
  movedTools?.getAttribute("data-panel-side") !== "right" ||
  window.document.activeElement?.id !== "layout-panel-tools-collapse"
) {
  throw new Error("Panel move did not preserve its identity or restore focus.");
}
const movedOrder = Number(movedTools.getAttribute("data-panel-order"));
[
  ...movedTools.querySelectorAll("button"),
]
  .find((button) => button.getAttribute("aria-label") === "Move Tools panel up")
  ?.click();
await window.happyDOM.waitUntilComplete();
if (
  Number(
    window.document
      .querySelector('[data-panel-id="tools"]')
      ?.getAttribute("data-panel-order"),
  ) >= movedOrder ||
  window.document.activeElement?.id !== "layout-panel-tools-collapse"
) {
  throw new Error(
    "Non-drag panel reordering did not change deterministic order and restore focus.",
  );
}
[
  ...window.document.querySelectorAll('[data-panel-id="tools"] button'),
]
  .find((button) => button.getAttribute("aria-label") === "Hide Tools panel")
  ?.click();
await window.happyDOM.waitUntilComplete();
if (
  window.document.querySelector('[data-panel-id="tools"]') ||
  window.document.activeElement?.id !== "layout-show-tools"
) {
  throw new Error("Panel hide did not remove controls from the DOM and restore toggle focus.");
}
window.document.querySelector("#layout-show-tools")?.click();
await window.happyDOM.waitUntilComplete();
if (
  !window.document.querySelector('[data-panel-id="tools"]') ||
  window.document.activeElement?.id !== "layout-panel-tools-collapse"
) {
  throw new Error("Panel show did not restore the panel and focus its header.");
}
const leftDrawerToggle = window.document.querySelector(
  "#layout-left-drawer-toggle.tactical-drawer-toggle",
);
window.happyDOM.setInnerWidth(1200);
if (
  window.getComputedStyle(leftDrawerToggle).display !== "none" ||
  leftDrawerToggle?.getAttribute("aria-controls") !== "tactical-sidebar-left"
) {
  throw new Error(
    "Responsive drawer disclosure was exposed in the desktop layout or lost its controlled region.",
  );
}
window.happyDOM.setInnerWidth(600);
if (!window.matchMedia("(max-width: 48rem)").matches) {
  throw new Error("The drawer qualification did not enter the mobile viewport.");
}
leftDrawerToggle?.click();
await window.happyDOM.waitUntilComplete();
if (
  window.document
    .querySelector("#layout-left-drawer-toggle")
    ?.getAttribute("aria-expanded") !== "true" ||
  !window.document
    .querySelector("#tactical-sidebar-left")
    ?.classList.contains("is-drawer-open") ||
  window.document.activeElement?.id !== "layout-left-drawer-toggle"
) {
  throw new Error("Responsive left drawer open state, disclosure, or focus diverged.");
}
leftDrawerToggle?.click();
await window.happyDOM.waitUntilComplete();
if (
  window.document
    .querySelector("#layout-left-drawer-toggle")
    ?.getAttribute("aria-expanded") !== "false" ||
  window.document
    .querySelector("#tactical-sidebar-left")
    ?.classList.contains("is-drawer-open") ||
  window.document.activeElement?.id !== "layout-left-drawer-toggle"
) {
  throw new Error(
    "Responsive left drawer close did not remove availability and preserve toggle focus.",
  );
}
window.happyDOM.setInnerWidth(1200);
if (
  window.matchMedia("(max-width: 48rem)").matches ||
  window.getComputedStyle(leftDrawerToggle).display !== "none"
) {
  throw new Error("Closed drawer disclosure did not leave the desktop accessibility layout.");
}
window.document.querySelector("#layout-timeline-toggle")?.click();
await window.happyDOM.waitUntilComplete();
if (
  window.document
    .querySelector("#layout-timeline-toggle")
    ?.getAttribute("aria-expanded") !== "true" ||
  window.document
    .querySelector("#tactical-bottom-panel")
    ?.classList.contains("is-collapsed") ||
  window.document.activeElement?.id !== "layout-timeline-toggle"
) {
  throw new Error("Bottom-panel expand state or focus restoration diverged.");
}
window.document.querySelector("#layout-timeline-toggle")?.click();
await window.happyDOM.waitUntilComplete();
if (
  window.document
    .querySelector("#layout-timeline-toggle")
    ?.getAttribute("aria-expanded") !== "false" ||
  !window.document
    .querySelector("#tactical-bottom-panel")
    ?.classList.contains("is-collapsed") ||
  window.document.activeElement?.id !== "layout-timeline-toggle"
) {
  throw new Error("Bottom-panel collapse state or focus restoration diverged.");
}
window.document.querySelector("#layout-timeline-toggle")?.click();
await window.happyDOM.waitUntilComplete();
if (
  window.document
    .querySelector("#layout-timeline-toggle")
    ?.getAttribute("aria-expanded") !== "true" ||
  window.document
    .querySelector("#tactical-bottom-panel")
    ?.classList.contains("is-collapsed") ||
  window.document.activeElement?.id !== "layout-timeline-toggle"
) {
  throw new Error("Bottom-panel collapse round trip did not restore expansion and focus.");
}
window.document.querySelector("#layout-timeline-visibility-toggle")?.click();
await window.happyDOM.waitUntilComplete();
if (
  window.document.querySelector("#tactical-bottom-panel") ||
  window.document
    .querySelector("#layout-timeline-visibility-toggle")
    ?.getAttribute("aria-pressed") !== "false" ||
  window.document.activeElement?.id !== "layout-timeline-visibility-toggle" ||
  JSON.parse(
    window.localStorage.getItem("sir.tactical-layout.v1"),
  ).bottomPanel.visible !== false
) {
  throw new Error(
    "Bottom-panel hide did not remove its subtree, restore focus, and persist false visibility.",
  );
}
window.document.querySelector("#layout-timeline-visibility-toggle")?.click();
await window.happyDOM.waitUntilComplete();
if (
  !window.document.querySelector("#tactical-bottom-panel") ||
  window.document
    .querySelector("#layout-timeline-visibility-toggle")
    ?.getAttribute("aria-pressed") !== "true" ||
  window.document.activeElement?.id !== "layout-timeline-visibility-toggle" ||
  JSON.parse(
    window.localStorage.getItem("sir.tactical-layout.v1"),
  ).bottomPanel.visible !== true
) {
  throw new Error(
    "Bottom-panel show did not restore its subtree, focus, and persisted true visibility.",
  );
}
window.document.querySelector("#layout-reset")?.click();
await window.happyDOM.waitUntilComplete();
if (
  !window.document
    .querySelector("#tactical-bottom-panel")
    ?.classList.contains("is-collapsed") ||
  window.document
    .querySelector("#tactical-sidebar-left")
    ?.classList.contains("is-drawer-open") ||
  window.document.querySelector('[data-panel-id="tools"]')?.getAttribute(
    "data-panel-side",
  ) !== "left" ||
  window.document.activeElement?.id !== "layout-reset" ||
  JSON.parse(
    window.localStorage.getItem("sir.tactical-layout.v1"),
  ).bottomPanel.visible !== true
) {
  throw new Error(
    "Reset layout did not restore focus and persist deterministic visible Field Focus.",
  );
}
const editorRevisionBeforeScrub = window.document
  .querySelector('[aria-label="SVG tactical map workspace"]')
  ?.getAttribute("data-editor-revision");
const timelineStepForward = [
  ...window.document
    .querySelector('[aria-label="Unified tactical timeline"]')
    .querySelectorAll("button"),
].find((button) => button.textContent.trim() === "+1");
for (let index = 0; index < 17; index += 1) {
  timelineStepForward.click();
  await window.happyDOM.waitUntilComplete();
}
if (
  Number(
    window.document
      .querySelector('[aria-label="Unified tactical timeline"]')
      ?.getAttribute("data-time-cursor"),
  ) !== 17 ||
  window.document
    .querySelector('[aria-label="SVG tactical map workspace"]')
    ?.getAttribute("data-editor-revision") !== editorRevisionBeforeScrub
) {
  throw new Error(
    `Timeline scrub failed projection-only guard: cursor=${window.document
      .querySelector('[aria-label="Unified tactical timeline"]')
      ?.getAttribute("data-time-cursor")}, revision=${editorRevisionBeforeScrub}->${window.document
      .querySelector('[aria-label="SVG tactical map workspace"]')
      ?.getAttribute("data-editor-revision")}.`,
  );
}

const buttonByText = (text) =>
  [...window.document.querySelectorAll("button")].find(
    (button) => button.textContent.trim() === text,
  );

buttonByText("Plan")?.click();
await window.happyDOM.waitUntilComplete();
const planner = window.document.querySelector(
  '[aria-label="Coordinated planning workspace"]',
);
if (characterizePersistentWorkscreen) {
  const currentViewport = window.document.querySelector(
    "#tactical-battlefield-viewport",
  );
  const planningWorksurface =
    currentViewport?.querySelector(".planning-cell-grid");
  if (currentViewport !== persistentBattlefieldViewport) {
    throw new Error(
      "Corrective characterization setup failed: the outer tactical viewport did not survive Editor → Plan.",
    );
  }
  if (
    !planningWorksurface ||
    !planningWorksurface.isConnected ||
    !currentViewport.contains(planningWorksurface)
  ) {
    throw new Error(
      "Corrective characterization setup failed: Plan did not mount its exact .planning-cell-grid work surface inside the retained viewport.",
    );
  }
  if (initialEditorWorksurface === planningWorksurface) {
    throw new Error(
      "Corrective characterization setup failed: unlike work-surface roots compared as equal.",
    );
  }
  throw new Error(
    "KNOWN M0 FAILURE: #tactical-battlefield-viewport survived Editor → Plan, but its actual work-surface root was replaced (svg#editor-map-stage → .planning-cell-grid).",
  );
}
const stateLabels = [...planner?.querySelectorAll(".planning-status .eyebrow") ?? []]
  .map((element) => element.textContent.trim());
if (
  !planner ||
  window.document.querySelector("#unified-tactical-workspace") !==
    persistentTacticalShell ||
  window.document.querySelector("#tactical-battlefield-viewport") !==
    persistentBattlefieldViewport ||
  Number(
    window.document
      .querySelector('[aria-label="Unified tactical timeline"]')
      ?.getAttribute("data-time-cursor"),
  ) !== 17 ||
  !["Authored", "Predicted", "Accepted", "Committed"].every((label) =>
    stateLabels.includes(label)
  ) ||
  !planner.querySelector('[aria-label^="Planning roster"]') ||
  !persistentBattlefieldViewport.querySelector('[aria-label="Battlefield route authoring"]') ||
  !planner.querySelector('[aria-label="Planning timeline lanes"]') ||
  !planner.querySelector('[aria-label="Planning inspector"]') ||
  !planner.querySelector('[aria-label="Planning validation navigation"]') ||
  !workerMessages.some((message) => message.Kind === "sir-simulator-session")
) {
  throw new Error(
    "The planning workspace did not mount its distinct state channels, five coordinated panes, and real worker initialization.",
  );
}
window.document.querySelector("#tactical-input-toggle")?.click();
await window.happyDOM.waitUntilComplete();
const planHelpIds = [
  ...window.document.querySelectorAll(
    "#tactical-input-panel [data-tactical-command]",
  ),
].map((item) => item.getAttribute("data-tactical-command"));
if (
  ![
    "planning.preview",
    "planning.validate",
    "planning.roster.select.1",
    "planning.inspector.waypoint.west",
    "planning.battlefield.cell.0.0",
  ].every((id) => planHelpIds.includes(id))
) {
  throw new Error(
    `Plan contextual help omitted executable worker, roster, or inspector actions: ${planHelpIds.join(",")}.`,
  );
}
window.document.querySelector("#tactical-input-panel button:last-child")?.click();
await window.happyDOM.waitUntilComplete();
const planCursorBeforePlayback = Number(
  window.document
    .querySelector('[aria-label="Unified tactical timeline"]')
    ?.getAttribute("data-time-cursor"),
);
[
  ...window.document.querySelectorAll(
    '[aria-label="Unified tactical timeline"] button',
  ),
]
  .find((button) => button.textContent.trim() === "Play")
  ?.click();
await new Promise((resolveWait) => setTimeout(resolveWait, 130));
await window.happyDOM.waitUntilComplete();
const planCursorAfterPlayback = Number(
  window.document
    .querySelector('[aria-label="Unified tactical timeline"]')
    ?.getAttribute("data-time-cursor"),
);
if (planCursorAfterPlayback <= planCursorBeforePlayback) {
  throw new Error(
    `Unified Plan playback did not advance its tactical cursor: ${planCursorBeforePlayback} -> ${planCursorAfterPlayback}.`,
  );
}
[
  ...window.document.querySelectorAll(
    '[aria-label="Unified tactical timeline"] button',
  ),
]
  .find((button) => button.textContent.trim() === "Pause")
  ?.click();
await window.happyDOM.waitUntilComplete();
const authoredBefore = planner.querySelector(".planning-status strong")?.textContent;
persistentBattlefieldViewport
  .querySelector("[data-planning-column][data-planning-row]")
  ?.click();
await window.happyDOM.waitUntilComplete();
const authoredAfter = window.document
  .querySelector('[aria-label="Coordinated planning workspace"] .planning-status strong')
  ?.textContent;
if (
  authoredBefore === authoredAfter ||
  !window.document
    .querySelector('[aria-label="Planning timeline lanes"]')
    ?.textContent.includes("Route")
) {
  throw new Error("Pointer/keyboard cell activation did not author a revision in its timeline lane.");
}
buttonByText("Undo")?.click();
await window.happyDOM.waitUntilComplete();
buttonByText("Redo")?.click();
await window.happyDOM.waitUntilComplete();
if (
  !window.document
    .querySelector('[aria-label="Planning timeline lanes"]')
    ?.textContent.includes("Route")
) {
  throw new Error("Planning undo/redo did not restore the authored command.");
}
const planningCells = [
  ...persistentBattlefieldViewport.querySelectorAll(
    "[data-planning-column][data-planning-row]",
  ),
];
planningCells[1]?.click();
await window.happyDOM.waitUntilComplete();
buttonByText("Undo")?.click();
await window.happyDOM.waitUntilComplete();
buttonByText("Validate")?.click();
await window.happyDOM.waitUntilComplete();
if (
  !window.document.querySelector(
    '[aria-label="Unified tactical timeline"] [data-time-channel="Accepted"]',
  ) ||
  !window.document
    .querySelector('[aria-label="Coordinated planning workspace"]')
    ?.textContent.includes("Revision accepted by worker validation")
) {
  throw new Error(
    "A worker-accepted planning revision did not project into the shared tactical segments.",
  );
}
buttonByText("Commit")?.click();
await new Promise((resolveWait) => setTimeout(resolveWait, 10));
const progressBoundary = Number(
  window.document
    .querySelector('[aria-label="Unified tactical timeline"]')
    ?.getAttribute("data-committed-through"),
);
if (progressBoundary !== 300) {
  throw new Error(
    `Authoritative SimulatorProgress did not advance App's shared boundary before PlanCommitted (${progressBoundary}).`,
  );
}
await new Promise((resolveWait) => setTimeout(resolveWait, 100));
await window.happyDOM.waitUntilComplete();
if (
  !window.document.querySelector(
    '[aria-label="Unified tactical timeline"] [data-time-channel="Committed"]',
  ) ||
  !window.document
    .querySelector('[aria-label="Coordinated planning workspace"]')
    ?.textContent.includes("Plan committed to simulator session")
) {
  throw new Error(
    "A worker-committed planning revision did not project into the shared tactical boundary and segments.",
  );
}
const committedRevisionText = window.document
  .querySelector('[aria-label="Coordinated planning workspace"] .planning-status strong')
  ?.textContent;
const committedUndo = buttonByText("Undo");
const committedRedo = buttonByText("Redo");
const committedMove = buttonByText("Move command here");
const committedRemove = buttonByText("Remove command");
if (
  !committedUndo?.disabled ||
  !committedRedo?.disabled ||
  !committedMove?.disabled ||
  !committedRemove?.disabled
) {
  throw new Error(
    "Committed planning history left Undo, Redo, Move, or Remove pointer actions available.",
  );
}
for (const guarded of [committedUndo, committedRedo, committedMove, committedRemove]) {
  guarded?.click();
}
await window.happyDOM.waitUntilComplete();
if (
  window.document
    .querySelector('[aria-label="Coordinated planning workspace"] .planning-status strong')
    ?.textContent !== committedRevisionText ||
  !window.document
    .querySelector('[aria-label="Planning timeline lanes"]')
    ?.textContent.includes("Route")
) {
  throw new Error("A pointer action mutated committed planning history.");
}
for (const modality of ["Simulate", "Review", "Editor", "Plan"]) {
  buttonByText(modality)?.click();
  await window.happyDOM.waitUntilComplete();
  if (
    window.document.querySelector("#tactical-battlefield-viewport") !==
      persistentBattlefieldViewport ||
    window.document.querySelectorAll("#tactical-battlefield-viewport").length !== 1
  ) {
    throw new Error(`The tactical battlefield viewport remounted in ${modality}.`);
  }
}
buttonByText("Editor")?.click();
await window.happyDOM.waitUntilComplete();

window.document.querySelector("#tactical-input-toggle")?.click();
await window.happyDOM.waitUntilComplete();
buttonByText("Configure bindings")?.click();
await window.happyDOM.waitUntilComplete();
const reviewBindingRow = [
  ...window.document.querySelectorAll(".tactical-binding-list li"),
].find((item) => item.textContent.includes("Switch to Review"));
const reviewBindingInput = reviewBindingRow?.querySelector("input");
if (!reviewBindingRow || !reviewBindingInput) {
  throw new Error(
    `The native binding configuration omitted Review (row=${Boolean(reviewBindingRow)}, inputs=${window.document.querySelectorAll(".tactical-binding-dialog input").length}): ${window.document.querySelector(".tactical-binding-dialog")?.textContent ?? "dialog closed"}`,
  );
}
reviewBindingInput.focus();
reviewBindingInput.dispatchEvent(
  new window.KeyboardEvent("keydown", {
    key: "F10",
    bubbles: true,
    cancelable: true,
  }),
);
await window.happyDOM.waitUntilComplete();
[
  ...window.document.querySelectorAll(".tactical-binding-list li"),
]
  .find((item) => item.textContent.includes("Switch to Review"))
  ?.querySelector("button")
  ?.click();
await window.happyDOM.waitUntilComplete();
if (!window.localStorage.getItem("sir.tactical-bindings.v1")) {
  throw new Error(
    `Binding apply did not persist: ${window.document.querySelector(".tactical-binding-dialog")?.textContent}`,
  );
}
window.document.querySelector(".tactical-binding-dialog button")?.click();
await window.happyDOM.waitUntilComplete();
window.document
  .querySelector('[aria-label="SVG tactical map workspace"] svg[role="application"]')
  ?.dispatchEvent(
  new window.KeyboardEvent("keydown", {
    key: "F10",
    bubbles: true,
    cancelable: true,
  }),
  );
await window.happyDOM.waitUntilComplete();
if (
  buttonByText("Review")?.getAttribute("aria-pressed") !== "true" ||
  !window.localStorage.getItem("sir.tactical-bindings.v1")?.includes("F10")
) {
  throw new Error(
    `A rebound effective gesture did not drive dispatch or durable local storage (review=${buttonByText("Review")?.getAttribute("aria-pressed")}, storage=${window.localStorage.getItem("sir.tactical-bindings.v1")}, diagnostics=${window.document.querySelector(".tactical-binding-dialog")?.textContent}).`,
  );
}
buttonByText("Editor")?.click();
await window.happyDOM.waitUntilComplete();
if (window.document.querySelector("#tactical-input-panel")) {
  window.document.querySelector("#tactical-input-toggle")?.click();
  await window.happyDOM.waitUntilComplete();
}
window.document.querySelector("#tactical-input-toggle")?.click();
await window.happyDOM.waitUntilComplete();
buttonByText("Configure bindings")?.click();
await window.happyDOM.waitUntilComplete();
let panelBindingRow = [
  ...window.document.querySelectorAll(".tactical-binding-list li"),
].find((item) => item.textContent.includes("Show or hide the active command panel"));
[...panelBindingRow.querySelectorAll("button")]
  .find((item) => item.textContent.trim() === "Clear")
  ?.click();
await window.happyDOM.waitUntilComplete();
window.document.querySelector(".tactical-binding-dialog button")?.click();
await window.happyDOM.waitUntilComplete();
const editorRibbonWasCollapsed = window.document
  .querySelector(".editor-ribbon")
  ?.classList.contains("is-collapsed");
const dispatchEditorModalKey = (key) => {
  const event = new window.KeyboardEvent("keydown", {
    key,
    bubbles: true,
    cancelable: true,
  });
  window.document
    .querySelector('[aria-label="SVG tactical map workspace"] svg[role="application"]')
    ?.dispatchEvent(event);
  return event;
};
const clearedEditorDefault = dispatchEditorModalKey("F2");
await window.happyDOM.waitUntilComplete();
if (
  window.document.querySelector(".editor-ribbon")?.classList.contains("is-collapsed") !==
    editorRibbonWasCollapsed ||
  clearedEditorDefault.defaultPrevented
) {
  throw new Error(
    "Clearing an Editor modal binding left its legacy F2 action or stage-local default prevention active.",
  );
}
buttonByText("Configure bindings")?.click();
await window.happyDOM.waitUntilComplete();
panelBindingRow = [
  ...window.document.querySelectorAll(".tactical-binding-list li"),
].find((item) => item.textContent.includes("Show or hide the active command panel"));
const panelBindingInput = panelBindingRow?.querySelector("input");
panelBindingInput?.focus();
panelBindingInput?.dispatchEvent(
  new window.KeyboardEvent("keydown", {
    key: "F11",
    bubbles: true,
    cancelable: true,
  }),
);
await window.happyDOM.waitUntilComplete();
panelBindingRow = [
  ...window.document.querySelectorAll(".tactical-binding-list li"),
].find((item) => item.textContent.includes("Show or hide the active command panel"));
[...panelBindingRow.querySelectorAll("button")]
  .find((item) => item.textContent.trim() === "Apply")
  ?.click();
await window.happyDOM.waitUntilComplete();
window.document.querySelector(".tactical-binding-dialog button")?.click();
await window.happyDOM.waitUntilComplete();
const reboundEditorEffective = dispatchEditorModalKey("F11");
await window.happyDOM.waitUntilComplete();
if (
  window.document.querySelector(".editor-ribbon")?.classList.contains("is-collapsed") ===
    editorRibbonWasCollapsed ||
  !reboundEditorEffective.defaultPrevented
) {
  throw new Error(
    "A rebound Editor modal command did not replace its cleared default and stage-local cancellation.",
  );
}
dispatchEditorModalKey("F11");
await window.happyDOM.waitUntilComplete();
if (window.document.querySelector("#tactical-input-panel")) {
  window.document.querySelector("#tactical-input-toggle")?.click();
  await window.happyDOM.waitUntilComplete();
}

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

const handoffButton = [
  ...window.document.querySelectorAll(
    '[aria-label="Map editor menu and toolbar"] button',
  ),
].find((button) => button.textContent.trim() === "Simulate");
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

window.document
  .querySelector(
    '[aria-label="Simulator menu and toolbar"] button[aria-label="Toggle simulator controls panel"]',
  )
  ?.click();
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
  throw new Error(
    `The full-width simulator or its controller modes did not mount: menu=${Boolean(window.document.querySelector('[aria-label="Simulator menu and toolbar"]'))}, panel=${Boolean(window.document.querySelector('[aria-label="Simulator command panel"]'))}, controllers=${controllerPanel?.textContent}, units=${editorBattlefield?.querySelectorAll("[data-unit-id]").length}.`,
  );
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
if (
  window.document
    .querySelector('[aria-label="Unified tactical timeline"]')
    ?.getAttribute("data-time-cursor") !== "1"
) {
  throw new Error("Simulator stepping did not synchronize the unified tactical cursor.");
}
const simulatorRuler = window.document.querySelector(
  '[aria-label="Unified tactical timeline"] input[type="range"]',
);
Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, "value").set.call(
  simulatorRuler,
  "10",
);
simulatorRuler.dispatchEvent(new window.Event("input", { bubbles: true }));
await window.happyDOM.waitUntilComplete();
if (
  window.document
    .querySelector('[aria-label="Unified tactical timeline"]')
    ?.getAttribute("data-time-cursor") !== "10" ||
  window.document
    .querySelector('[aria-label="Unified tactical timeline"]')
    ?.getAttribute("data-scrub-semantics") !==
    "projection-only-runtime-tick-unchanged" ||
  !window.document
    .querySelector('[aria-label="Editable simulation SVG battlefield"] svg')
    ?.getAttribute("aria-label")
    ?.includes("exact tick 1")
) {
  throw new Error("Simulator scrubbing mutated its authoritative runtime tick.");
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
  "d4171834d6286d6c143cf3ec71b84b58ea7448f52665a05e7d19f9188462a860";
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
if (
  editorCanvas?.getAttribute("data-editor-revision") !== initialEditorDigest ||
  editorWorkspace.querySelectorAll("[data-editor-unit-id]").length !== 4
) {
  throw new Error("Fable paste mutated the map before its explicit preview commit.");
}
editorWorkspace.dispatchEvent(
  new window.KeyboardEvent("keydown", {
    key: "Enter",
    bubbles: true,
  }),
);
await window.happyDOM.waitUntilComplete();
const pastedDigest = editorCanvas?.getAttribute("data-editor-revision");
if (
  pastedDigest === initialEditorDigest ||
  editorWorkspace.querySelectorAll("[data-editor-unit-id]").length !== 5
) {
  throw new Error(
    `Fable did not commit a copied formation as one immutable revision (connected=${editorWorkspace.isConnected}, digest=${editorCanvas?.getAttribute("data-editor-revision")}, liveDigest=${window.document.querySelector('[aria-label="SVG tactical map workspace"]')?.getAttribute("data-editor-revision")}, units=${window.document.querySelectorAll("[data-editor-unit-id]").length}).`,
  );
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
    'button[aria-label="rough terrain, diagonal hatch"]',
  ) ||
  !window.document.querySelector(
    'button[aria-label="blocked terrain, cross hatch"]',
  ) ||
  !window.document.querySelector("#terrain-brush-size")
) {
  throw new Error("The accessible terrain palette, patterns, or integer brush control did not mount.");
}
buttonByText("Pencil")?.click();
await window.happyDOM.waitUntilComplete();
window.document
  .querySelector(
    'button[aria-label="blocked terrain, cross hatch"]',
  )
  ?.click();
await window.happyDOM.waitUntilComplete();
window.document
  .querySelector('[aria-label="SVG tactical map workspace"] svg[role="application"]')
  ?.dispatchEvent(
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
  throw new Error("The keyboard Pencil did not commit one announced revision.");
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

const currentEditorDigest = () =>
  window.document
    .querySelector('[aria-label="SVG tactical map workspace"]')
    ?.getAttribute("data-editor-revision");
const keyboardRegionStartDigest = currentEditorDigest();
const dispatchCurrentEditorKey = (options) =>
  window.document
    .querySelector('[aria-label="SVG tactical map workspace"] svg[role="application"]')
    ?.dispatchEvent(
      new window.KeyboardEvent("keydown", {
        ...options,
        bubbles: true,
        cancelable: true,
      }),
    );

const editorModalStrip = window.document.querySelector(
  'section.modal-input-strip[aria-label="Current tactical actions"]',
);
const editorModalToggle = window.document.querySelector("#tactical-input-toggle");
const editorModeStatus = editorModalStrip?.querySelector(
  '[role="status"][aria-live="polite"][aria-atomic="true"]',
);
if (
  !editorModalStrip ||
  editorModalToggle?.tagName !== "BUTTON" ||
  editorModalToggle.getAttribute("aria-expanded") !== "false" ||
  editorModalToggle.getAttribute("aria-controls") !== "tactical-input-panel" ||
  !editorModeStatus
) {
  throw new Error(
    "The Editor modal strip lost its labelled region, live status, or native disclosure semantics.",
  );
}

window.document
  .querySelector('[aria-label="SVG tactical map workspace"] svg[role="application"]')
  ?.focus();
dispatchCurrentEditorKey({ key: "?", shiftKey: true });
await window.happyDOM.waitUntilComplete();
const keyboardOpenedPanel = window.document.querySelector("#tactical-input-panel");
if (
  !keyboardOpenedPanel ||
  window.document.activeElement !== keyboardOpenedPanel ||
  !keyboardOpenedPanel.getAttribute("aria-label")?.startsWith("Executable actions for ") ||
  [...keyboardOpenedPanel.querySelectorAll("[data-modal-command]")].some(
    (item) => !item.getAttribute("aria-keyshortcuts"),
  )
) {
  throw new Error(
    "Keyboard-opened Editor input help lost focus, labelling, or shortcut semantics.",
  );
}
dispatchCurrentEditorKey({ key: "Escape" });
await window.happyDOM.waitUntilComplete();
if (
  window.document.querySelector("#tactical-input-panel") ||
  window.document.activeElement?.id !== "tactical-input-toggle"
) {
  throw new Error("Closing Editor input help did not restore disclosure focus.");
}

window.document.querySelector("#tactical-input-toggle")?.click();
await window.happyDOM.waitUntilComplete();
const regionInputIds = [
  ...window.document.querySelectorAll("[data-modal-command]"),
].map((item) => item.getAttribute("data-modal-command"));
window.document.querySelector("#tactical-input-toggle")?.click();
await window.happyDOM.waitUntilComplete();
if (!regionInputIds.includes("editor.region.create.begin")) {
  throw new Error(`The live Zone disclosure omitted New Region: ${regionInputIds.join(",")}.`);
}
dispatchCurrentEditorKey({ key: "n" });
await window.happyDOM.waitUntilComplete();
if (!window.document.querySelector(".modal-input-state-strip")?.textContent.includes("NEW / PURPOSE")) {
  throw new Error(
    `The live N route did not enter region purpose selection: ${window.document.querySelector(".modal-input-state-strip")?.textContent}.`,
  );
}
for (const key of ["b", "p", "Enter", "ArrowRight", "ArrowRight", "Enter", "ArrowDown", "ArrowDown", "Enter"]) {
  dispatchCurrentEditorKey({ key });
  await window.happyDOM.waitUntilComplete();
}
if (
  !window.document
    .querySelector(".modal-input-state-strip")
    ?.textContent.includes("3 vertices")
) {
  throw new Error(
    `The live region polygon mode did not stage three keyboard vertices: ${window.document
      .querySelector(".modal-input-state-strip")
      ?.textContent}.`,
  );
}
dispatchCurrentEditorKey({ key: "Enter", shiftKey: true });
await window.happyDOM.waitUntilComplete();
await window.happyDOM.waitUntilComplete();
const keyboardRegionDigest = currentEditorDigest();
if (
  keyboardRegionDigest === keyboardRegionStartDigest ||
  window.document.querySelectorAll(
    '[aria-label="SVG tactical map workspace"] [data-region-id]',
  ).length !== 2
) {
  throw new Error("The nested keyboard region workflow did not commit one polygon revision.");
}
dispatchCurrentEditorKey({ key: "m" });
await window.happyDOM.waitUntilComplete();
dispatchCurrentEditorKey({ key: "ArrowRight", shiftKey: true });
await window.happyDOM.waitUntilComplete();
if (currentEditorDigest() !== keyboardRegionDigest) {
  throw new Error(
    `The keyboard region move preview mutated the document before Enter: ${keyboardRegionDigest} -> ${currentEditorDigest()} / ${window.document.querySelector(".modal-input-state-strip")?.textContent}.`,
  );
}
dispatchCurrentEditorKey({ key: "Backspace" });
await window.happyDOM.waitUntilComplete();
dispatchCurrentEditorKey({ key: "ArrowRight" });
await window.happyDOM.waitUntilComplete();
dispatchCurrentEditorKey({ key: "Enter" });
await window.happyDOM.waitUntilComplete();
if (currentEditorDigest() === keyboardRegionDigest) {
  throw new Error("The resettable keyboard region move did not commit atomically.");
}
dispatchCurrentEditorKey({ key: "p" });
await window.happyDOM.waitUntilComplete();
dispatchCurrentEditorKey({ key: "r" });
await window.happyDOM.waitUntilComplete();
dispatchCurrentEditorKey({ key: "Enter" });
await window.happyDOM.waitUntilComplete();
if (!objectList?.textContent.includes("Region 2 · Red deployment · polygon")) {
  throw new Error("The keyboard purpose popup did not apply the highlighted region purpose.");
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

const mapNameInput = window.document.querySelector("#map-name");
mapNameInput?.focus();
mapNameInput?.dispatchEvent(
  new window.KeyboardEvent("keydown", { key: "c", bubbles: true }),
);
await window.happyDOM.waitUntilComplete();
if (window.document.querySelector('[role="alertdialog"]')) {
  throw new Error("A native map-name field leaked a document key into modal dispatch.");
}
window.document.querySelector("#editor-map-stage")?.focus();
dispatchCurrentEditorKey({ key: "r", altKey: true });
await window.happyDOM.waitUntilComplete();
if (window.document.activeElement?.id === "map-width") {
  throw new Error("An Alt browser-reserved combination leaked into document modal dispatch.");
}
dispatchCurrentEditorKey({ key: "r" });
await window.happyDOM.waitUntilComplete();
if (window.document.activeElement?.id !== "map-width") {
  throw new Error("Document R did not hand focus to the native map dimension control.");
}
window.document.querySelector("#editor-map-stage")?.focus();
let importPickerInvoked = false;
const mapImport = window.document.querySelector("#editor-map-import");
mapImport?.addEventListener("click", () => {
  importPickerInvoked = true;
});
dispatchCurrentEditorKey({ key: "i" });
await window.happyDOM.waitUntilComplete();
if (!importPickerInvoked) {
  throw new Error("Document I did not invoke the visible native import picker.");
}
window.document.querySelector("#editor-map-stage")?.focus();
dispatchCurrentEditorKey({ key: "n" });
await window.happyDOM.waitUntilComplete();
if (
  !window.document.querySelector('[role="alertdialog"]') ||
  window.document.activeElement?.id !== "editor-destructive-confirmation"
) {
  throw new Error("Document N did not open and focus the native destructive confirmation.");
}
dispatchCurrentEditorKey({ key: "Escape" });
await window.happyDOM.waitUntilComplete();
await new Promise((resolve) => setTimeout(resolve, 0));
await window.happyDOM.waitUntilComplete();
if (
  window.document.querySelector('[role="alertdialog"]') ||
  window.document.activeElement?.id !== "editor-map-stage"
) {
  throw new Error(
    `Document confirmation cancellation did not restore map focus: dialog=${Boolean(window.document.querySelector('[role="alertdialog"]'))}, focus=${window.document.activeElement?.id}.`,
  );
}

window.document
  .querySelector('[aria-label="SVG tactical map workspace"] svg[role="application"]')
  ?.dispatchEvent(
  new window.KeyboardEvent("keydown", { key: "F11", bubbles: true }),
);
await window.happyDOM.waitUntilComplete();
if (
  window.document.querySelector(
    '.editor-context-palette input[aria-label="Import SIR map"]',
  )
) {
  throw new Error("The rebound F11 did not hide the active contextual ribbon.");
}
window.document
  .querySelector('[aria-label="SVG tactical map workspace"] svg[role="application"]')
  ?.dispatchEvent(
  new window.KeyboardEvent("keydown", { key: "F11", bubbles: true }),
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

buttonByText("Review")?.click();
await window.happyDOM.waitUntilComplete();

const unloadedReviewGesture = new window.KeyboardEvent("keydown", {
  key: "ArrowRight",
  ctrlKey: true,
  bubbles: true,
  cancelable: true,
});
window.document
  .querySelector("#unified-tactical-workspace")
  ?.dispatchEvent(unloadedReviewGesture);
await window.happyDOM.waitUntilComplete();
if (unloadedReviewGesture.defaultPrevented) {
  throw new Error(
    "An unavailable unloaded-Review transport gesture prevented the browser default.",
  );
}
window.document.querySelector("#tactical-input-toggle")?.click();
await window.happyDOM.waitUntilComplete();
const reviewHelpIdsAtEnd = [
  ...window.document.querySelectorAll(
    "#tactical-input-panel [data-tactical-command]",
  ),
].map((item) => item.getAttribute("data-tactical-command"));
if (
  [
    "timeline.play-toggle",
    "timeline.step-back",
    "timeline.step-forward",
    "timeline.home",
    "timeline.end",
  ].some((id) => reviewHelpIdsAtEnd.includes(id))
) {
  throw new Error(
    `Review help advertised timeline transport actions before a replay was loaded: ${reviewHelpIdsAtEnd.join(",")}.`,
  );
}
window.document.querySelector("#tactical-input-panel button:last-child")?.click();
await window.happyDOM.waitUntilComplete();

const status = [...window.document.querySelectorAll('[role="status"]')].find(
  (element) =>
    element.textContent.includes("Ready — choose a scenario or load a replay"),
);
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
const sharedReplayRuler = window.document.querySelector(
  '[aria-label="Unified tactical timeline"] input[type="range"]',
);
Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, "value").set.call(
  sharedReplayRuler,
  "1",
);
sharedReplayRuler.dispatchEvent(new window.Event("input", { bubbles: true }));
await new Promise((resolveWait) => setTimeout(resolveWait, 0));
await window.happyDOM.waitUntilComplete();
if (
  !window.document
    .querySelector('[aria-label="Loaded replay SVG battlefield"] svg[role="application"]')
    ?.getAttribute("aria-label")
    ?.includes("exact tick 1") ||
  window.document
    .querySelector('[aria-label="Unified tactical timeline"]')
    ?.getAttribute("data-time-cursor") !== "1"
) {
  throw new Error(
    `The shared tactical ruler did not seek the actual Review projection (cursor=${window.document.querySelector('[aria-label="Unified tactical timeline"]')?.getAttribute("data-time-cursor")}, battlefield=${window.document.querySelector('[aria-label="Loaded replay SVG battlefield"] svg[role="application"]')?.getAttribute("aria-label")}, connected=${sharedReplayRuler.isConnected}).`,
  );
}
window.document.querySelector("#tactical-input-toggle")?.click();
await window.happyDOM.waitUntilComplete();
const loadedReviewHelpIds = [
  ...window.document.querySelectorAll(
    "#tactical-input-panel [data-tactical-command]",
  ),
].map((item) => item.getAttribute("data-tactical-command"));
if (
  ![
    "timeline.play-toggle",
    "timeline.step-back",
    "timeline.step-forward",
    "timeline.home",
    "timeline.end",
  ].every((id) => loadedReviewHelpIds.includes(id))
) {
  throw new Error(
    `Loaded Review help omitted executable transport actions: ${loadedReviewHelpIds.join(",")}.`,
  );
}
window.document.querySelector("#tactical-input-panel button:last-child")?.click();
await window.happyDOM.waitUntilComplete();

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
const sendSimulatorKey = async (key, options = {}, target = null) => {
  (
    target ??
    window.document.querySelector("#simulator-map-stage")
  ).dispatchEvent(
    new window.KeyboardEvent("keydown", { key, bubbles: true, ...options }),
  );
  await new Promise((done) => setTimeout(done, 10));
};
const simulatorModalState = () =>
  window.document.querySelector(".simulator-workspace .modal-input-state");
const simulatorTick = () => {
  const match = simulatorModalState()?.textContent.match(/tick (\d+)/i);
  return match ? Number(match[1]) : Number.NaN;
};

window.document.querySelector("#tactical-input-toggle")?.click();
await window.happyDOM.waitUntilComplete();
const pausedSimulatorInputIds = [
  ...window.document.querySelectorAll(
    "#tactical-input-panel [data-modal-command]",
  ),
].map((item) => item.getAttribute("data-modal-command"));
const pausedSimulatorPointerIds = [
  ...window.document.querySelectorAll(
    "#tactical-input-panel [data-tactical-command]",
  ),
].map((item) => item.getAttribute("data-tactical-command"));
window.document
  .querySelector("#tactical-input-panel button:last-child")
  ?.click();
await window.happyDOM.waitUntilComplete();
if (
  !pausedSimulatorInputIds.includes("simulator.unit.next") ||
  !pausedSimulatorInputIds.includes("simulator.step") ||
  !pausedSimulatorInputIds.includes("simulator.reset.request") ||
  ![
    "simulator.pointer.controller.manual",
    "simulator.pointer.script.set",
    "simulator.pointer.movement.north",
  ].every((id) => pausedSimulatorPointerIds.includes(id))
) {
  throw new Error(
    `Paused Simulator omitted keyboard or pointer-only lifecycle inputs: modal=${pausedSimulatorInputIds.join(",")}; pointer=${pausedSimulatorPointerIds.join(",")}.`,
  );
}
window.document.querySelector("#tactical-input-toggle")?.click();
await window.happyDOM.waitUntilComplete();
buttonByText("Configure bindings")?.click();
await window.happyDOM.waitUntilComplete();
const pointerOnlyBindingRows = window.document.querySelectorAll(
  '.tactical-binding-list [data-binding-command^="simulator.pointer."]',
);
if (pointerOnlyBindingRows.length !== 0) {
  throw new Error(
    "Pointer-only Simulator actions were incorrectly exposed as configurable keyboard bindings.",
  );
}
const resetBindingRow = [
  ...window.document.querySelectorAll(".tactical-binding-list li"),
].find((item) => item.textContent.includes("Reset the simulator sandbox"));
const resetBindingInput = resetBindingRow?.querySelector("input");
resetBindingInput?.focus();
resetBindingInput?.dispatchEvent(
  new window.KeyboardEvent("keydown", {
    key: "F12",
    bubbles: true,
    cancelable: true,
  }),
);
await window.happyDOM.waitUntilComplete();
[
  ...window.document.querySelectorAll(".tactical-binding-list li"),
]
  .find((item) => item.textContent.includes("Reset the simulator sandbox"))
  ?.querySelector("button")
  ?.click();
await window.happyDOM.waitUntilComplete();
window.document.querySelector(".tactical-binding-dialog button")?.click();
await window.happyDOM.waitUntilComplete();
window.document
  .querySelector("#tactical-input-panel button:last-child")
  ?.click();
await window.happyDOM.waitUntilComplete();
let stageLocalResetConfirmCalls = 0;
const stageLocalPreviousConfirm = window.confirm;
window.confirm = () => {
  stageLocalResetConfirmCalls += 1;
  return false;
};
const clearedSimulatorDefault = new window.KeyboardEvent("keydown", {
  key: "r",
  bubbles: true,
  cancelable: true,
});
window.document
  .querySelector("#simulator-map-stage")
  ?.dispatchEvent(clearedSimulatorDefault);
await new Promise((done) => setTimeout(done, 10));
const reboundSimulatorEffective = new window.KeyboardEvent("keydown", {
  key: "F12",
  bubbles: true,
  cancelable: true,
});
window.document
  .querySelector("#simulator-map-stage")
  ?.dispatchEvent(reboundSimulatorEffective);
await new Promise((done) => setTimeout(done, 10));
window.confirm = stageLocalPreviousConfirm;
if (
  clearedSimulatorDefault.defaultPrevented ||
  !reboundSimulatorEffective.defaultPrevented ||
  stageLocalResetConfirmCalls !== 1
) {
  throw new Error(
    `Simulator stage-local adapted binding diverged (old-prevented=${clearedSimulatorDefault.defaultPrevented}, new-prevented=${reboundSimulatorEffective.defaultPrevented}, actions=${stageLocalResetConfirmCalls}).`,
  );
}
await sendSimulatorKey("k");
window.document.querySelector("#tactical-input-toggle")?.click();
await new Promise((done) => setTimeout(done, 10));
const runningHelpIds = [
  ...window.document.querySelectorAll(
    "#tactical-input-panel [data-modal-command]",
  ),
].map((item) => item.getAttribute("data-modal-command"));
const runningPointerHelpIds = [
  ...window.document.querySelectorAll(
    "#tactical-input-panel [data-tactical-command]",
  ),
].map((item) => item.getAttribute("data-tactical-command"));
window.document
  .querySelector("#tactical-input-panel button:last-child")
  ?.click();
await new Promise((done) => setTimeout(done, 10));
let unavailableResetConfirmCalls = 0;
const previousConfirm = window.confirm;
window.confirm = () => {
  unavailableResetConfirmCalls += 1;
  return false;
};
const unavailableAdaptedModal = new window.KeyboardEvent("keydown", {
  key: "F12",
  bubbles: true,
  cancelable: true,
});
window.document
  .querySelector("#simulator-map-stage")
  ?.dispatchEvent(unavailableAdaptedModal);
await new Promise((done) => setTimeout(done, 10));
window.confirm = previousConfirm;
await sendSimulatorKey("k");
let reboundPointerConfirmCalls = 0;
window.confirm = () => {
  reboundPointerConfirmCalls += 1;
  return false;
};
window.document
  .querySelector(
    '[aria-label="Simulator menu and toolbar"] button[aria-label="Reset simulation to its immutable revision"]',
  )
  ?.click();
await new Promise((done) => setTimeout(done, 10));
window.confirm = previousConfirm;
if (
  runningHelpIds.includes("simulator.reset.request") ||
  [
    "simulator.pointer.controller.manual",
    "simulator.pointer.script.set",
    "simulator.pointer.movement.north",
  ].some((id) => runningPointerHelpIds.includes(id)) ||
  unavailableResetConfirmCalls !== 0 ||
  reboundPointerConfirmCalls !== 1 ||
  ![
    ...window.document.querySelectorAll(
      '[aria-label="Simulator menu and toolbar"] button[aria-label="Run or pause deterministic simulation"]',
    ),
  ].some((button) => button.textContent.trim() === "Run") ||
  unavailableAdaptedModal.defaultPrevented
) {
  throw new Error(
    `Unavailable adapted Simulator binding or rebound pointer authority diverged (help=${runningHelpIds.join(",")}, unavailable-actions=${unavailableResetConfirmCalls}, pointer-actions=${reboundPointerConfirmCalls}, prevented=${unavailableAdaptedModal.defaultPrevented}).`,
  );
}
window.document.querySelector("#tactical-input-toggle")?.click();
await window.happyDOM.waitUntilComplete();
buttonByText("Configure bindings")?.click();
await window.happyDOM.waitUntilComplete();
const restoredResetBindingRow = [
  ...window.document.querySelectorAll(".tactical-binding-list li"),
].find((item) => item.textContent.includes("Reset the simulator sandbox"));
[...(restoredResetBindingRow?.querySelectorAll("button") ?? [])]
  .find((item) => item.textContent.trim() === "Restore")
  ?.click();
await window.happyDOM.waitUntilComplete();
window.document.querySelector(".tactical-binding-dialog button")?.click();
await window.happyDOM.waitUntilComplete();
window.document
  .querySelector("#tactical-input-panel button:last-child")
  ?.click();
await window.happyDOM.waitUntilComplete();
for (const [key, modifiers] of [
  ["k", { ctrlKey: true }],
  ["k", { metaKey: true }],
  ["ArrowRight", { altKey: true }],
  ["?", { altKey: true }],
]) {
  const reservedEvent = new window.KeyboardEvent("keydown", {
    key,
    bubbles: true,
    cancelable: true,
    ...modifiers,
  });
  const dispatched = window.document
    .querySelector("#simulator-map-stage")
    ?.dispatchEvent(reservedEvent);
  await new Promise((done) => setTimeout(done, 10));
  if (!dispatched || reservedEvent.defaultPrevented) {
    throw new Error(
      `Simulator canceled a browser-reserved ${key} modifier combination.`,
    );
  }
}
if (
  window.document
    .querySelector("#tactical-input-toggle")
    ?.getAttribute("aria-expanded") !== "false"
) {
  throw new Error("Alt+? entered Simulator input-help modal dispatch.");
}
window.document.querySelector("#tactical-input-toggle")?.click();
await window.happyDOM.waitUntilComplete();
const altEscape = new window.KeyboardEvent("keydown", {
  key: "Escape",
  altKey: true,
  bubbles: true,
  cancelable: true,
});
const altEscapeDispatched = window.document
  .querySelector("#simulator-map-stage")
  ?.dispatchEvent(altEscape);
await new Promise((done) => setTimeout(done, 10));
if (
  !altEscapeDispatched ||
  altEscape.defaultPrevented ||
  window.document
    .querySelector("#tactical-input-toggle")
    ?.getAttribute("aria-expanded") !== "true"
) {
  throw new Error("Alt+Escape closed Simulator input help or was canceled.");
}
window.document
  .querySelector("#tactical-input-panel button:last-child")
  ?.click();
await window.happyDOM.waitUntilComplete();
const selectedBeforeTraversal = simulatorModalState()?.textContent;
await sendSimulatorKey("]");
if (
  simulatorModalState()?.textContent === selectedBeforeTraversal ||
  !simulatorModalState()?.textContent.includes("selected")
) {
  throw new Error(
    `Simulator ] did not deterministically traverse the inspected unit (${selectedBeforeTraversal} -> ${simulatorModalState()?.textContent}).`,
  );
}
const tickBeforeKeyboardStep = simulatorTick();
await sendSimulatorKey(".");
if (simulatorTick() !== tickBeforeKeyboardStep + 1) {
  throw new Error("Simulator . did not advance exactly one paused tick.");
}
await sendSimulatorKey("ArrowRight");
if (!simulatorModalState()?.textContent.includes("SIMULATOR / ROUTE PREVIEW")) {
  throw new Error("Simulator ArrowRight did not begin a live route preview.");
}
await sendSimulatorKey("Backspace");
if (!simulatorModalState()?.textContent.includes("SIMULATOR / ROUTE PREVIEW")) {
  throw new Error("Simulator Backspace cancelled instead of resetting the route preview.");
}
await sendSimulatorKey("ArrowRight");
await sendSimulatorKey("Enter");
if (!simulatorModalState()?.textContent.includes("SIMULATOR / PAUSED")) {
  throw new Error("Simulator Enter did not commit the route preview.");
}
await sendSimulatorKey("ArrowRight");
await sendSimulatorKey("Escape");
if (!simulatorModalState()?.textContent.includes("SIMULATOR / PAUSED")) {
  throw new Error("Simulator Escape did not cancel the route preview.");
}
await sendSimulatorKey("Enter");
await sendSimulatorKey("g");
if (
  !simulatorModalState()?.textContent.includes("SIMULATOR / CONTROLLER") ||
  !simulatorModalState()?.textContent.includes("General AI")
) {
  throw new Error("Simulator controller selection did not expose the highlighted keyboard choice.");
}
await sendSimulatorKey("m");
await sendSimulatorKey("Enter");
if (!simulatorModalState()?.textContent.includes("SIMULATOR / PAUSED")) {
  throw new Error(
    `Simulator Enter did not commit controller selection: ${simulatorModalState()?.textContent}.`,
  );
}
await sendSimulatorKey("c");
const scriptInput = window.document.querySelector("#unit-script");
if (!scriptInput) {
  throw new Error("Simulator C did not expose the native controller script field.");
}
scriptInput?.focus();
const tickBeforeNativeScriptKey = simulatorTick();
await sendSimulatorKey("k", {}, scriptInput);
if (
  simulatorTick() !== tickBeforeNativeScriptKey ||
  !simulatorModalState()?.textContent.includes("SIMULATOR / PAUSED")
) {
  throw new Error("Native simulator script editing leaked into modal run dispatch.");
}
const nativeStepButton = [
  ...window.document.querySelectorAll(
    "#simulator-map-stage .simulation-controls button",
  ),
].find((item) => item.textContent.trim() === "Step");
if (!nativeStepButton) {
  throw new Error("The Simulator native Step control was not rendered.");
}
await sendSimulatorKey("Enter", {}, nativeStepButton);
await sendSimulatorKey(" ", {}, nativeStepButton);
if (
  !simulatorModalState()?.textContent.includes("SIMULATOR / PAUSED") ||
  simulatorModalState()?.textContent.includes("SIMULATOR / CONTROLLER")
) {
  throw new Error("A bubbled native Simulator button key also entered modal dispatch.");
}
window.document.querySelector(".simulator-map-stage")?.focus();
await sendSimulatorKey("ArrowRight");
if (!simulatorModalState()?.textContent.includes("SIMULATOR / ROUTE PREVIEW")) {
  throw new Error(
    `Simulator did not resume modal keys after native script focus left: ${simulatorModalState()?.textContent}.`,
  );
}
await sendSimulatorKey("k");
if (
  !simulatorModalState()?.textContent.includes("SIMULATOR / RUNNING") ||
  simulatorModalState()?.textContent.includes("ROUTE PREVIEW")
) {
  throw new Error(
    `Starting Simulator did not clear the uncommitted route preview: ${simulatorModalState()?.textContent}.`,
  );
}
await sendSimulatorKey("?", { shiftKey: true });
const runningInputIds = [
  ...window.document.querySelectorAll(
    "#tactical-input-panel [data-modal-command]",
  ),
].map((item) => item.getAttribute("data-modal-command"));
if (
  runningInputIds.includes("simulator.step") ||
  runningInputIds.includes("simulator.reset.request") ||
  runningInputIds.includes("simulator.preview.east") ||
  !runningInputIds.includes("simulator.run.toggle-space")
) {
  throw new Error("Running Simulator possible inputs disclosed unavailable mutations.");
}
await sendSimulatorKey("Escape");
await sendSimulatorKey("k");
const tickBeforeReset = simulatorTick();
let resetConfirmed = false;
window.confirm = () => {
  resetConfirmed = true;
  return true;
};
await sendSimulatorKey("r");
if (!resetConfirmed || simulatorTick() >= tickBeforeReset) {
  throw new Error("Simulator R did not use native confirmation and reset sandbox progress.");
}
await sendSimulatorKey("e");
if (
  !window.document
    .querySelector('[aria-label="Simulator command panel"]')
    ?.textContent.includes("events")
) {
  throw new Error("Simulator E did not select the Events panel.");
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
