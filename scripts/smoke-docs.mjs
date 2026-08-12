import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { pathToFileURL } from "node:url";
import { Window } from "happy-dom";

const site = resolve(process.argv[2] ?? "artifacts/site");
const html = await readFile(resolve(site, "interactive-rules-lab.html"), "utf8");
const body = html.match(/<body[^>]*>([\s\S]*)<\/body>/i)?.[1];

if (!body) {
  throw new Error("The generated interactive page has no body.");
}

const window = new Window({
  url: "https://ehotwagner.github.io/S.I.R./interactive-rules-lab.html",
});
window.document.body.innerHTML = body;

const workerMessages = [];

const scenarioResponse = (message) => {
  const identity = message.Request.fields[0];
  const parameters = [
    { Key: "attack-power", Value: 25 },
    { Key: "attack-count", Value: 4 },
  ];
  const metrics = [
    { Key: "attack-events", Value: 4 },
    { Key: "remaining-health", Value: 0 },
    { Key: "total-damage", Value: 100 },
  ];
  const input = {
    ScenarioIdentity: identity,
    ScenarioRevision: 1,
    EngineIdentity:
      "0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f20",
    RulesetIdentity:
      "6d31302d72756c65732d6c61622d763100000000000000000000000000000000",
    Parameters: parameters,
  };
  const result = {
    Input: input,
    ResultIdentity: "0123456789abcdef",
    Metrics: metrics,
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
          EngineIdentity: input.EngineIdentity,
          FinalTick: 1,
          Kind: 2,
        },
        {
          Identity: identity,
          Revision: 1,
          Title: "Four-hit baseline",
          Description: "Smoke scenario",
          EngineIdentity: input.EngineIdentity,
          RulesetIdentity: input.RulesetIdentity,
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
          Delta: metrics.map(({ Key }) => ({ Key, Value: 0 })),
          Sweep: undefined,
          EvidenceLabel:
            "Exploratory balance evidence — not accepted balance",
        },
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
    if (message.Request?.tag === 4) {
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

const bundle = resolve(site, "content/sir-client/v1/app.js");
await import(pathToFileURL(bundle));
await window.happyDOM.waitUntilComplete();

const mount = window.document.getElementById("sir-replay-app");
const application = mount?.querySelector(
  'main[aria-label="S.I.R. simulator and editor"]',
);
const status = mount?.querySelector('[role="status"]');

if (!application || mount?.querySelector("h1")) {
  throw new Error("The Fable application did not mount inside the fsdocs page.");
}

const persistentWorksurface = mount?.querySelector(
  "svg#persistent-tactical-svg[role='application']",
);
if (!mount.querySelector('[data-panel-id="document"]')) {
  mount.querySelector("#layout-show-document")?.click();
  await window.happyDOM.waitUntilComplete();
}
const documentPanel = mount.querySelector('[data-panel-id="document"]');
if (documentPanel?.classList.contains("is-collapsed")) {
  documentPanel.querySelector("#layout-panel-document-collapse")?.click();
  await window.happyDOM.waitUntilComplete();
}
if ([...mount.querySelectorAll("button")].some(
  (button) => button.textContent.trim() === "Simulate revision",
)) {
  throw new Error("The generated shell retained the obsolete manual simulator handoff.");
}
const editorPlay = [...mount.querySelectorAll('[aria-label="Unified tactical timeline"] button')]
  .find((button) => button.textContent.trim() === "Play");
if (!editorPlay || editorPlay.disabled) {
  throw new Error("The generated shell did not maintain simulation transport in Editor.");
}
const simulateButton = [...mount.querySelectorAll('[aria-label="Tactical modality"] button')]
  .find((button) => button.textContent.trim() === "Simulate");
if (!simulateButton) throw new Error("The generated shell omitted the Simulate modality.");
simulateButton.click();
await window.happyDOM.waitUntilComplete();

if (
  mount?.querySelector("#persistent-tactical-svg") !== persistentWorksurface ||
  persistentWorksurface?.getAttribute("data-scene-owner") !== "SimulatorScene" ||
  mount?.querySelectorAll("[role='application']").length !== 1 ||
  mount?.querySelector("[aria-label='Editable simulation SVG battlefield']")
) {
  throw new Error("The generated site did not project the maintained simulation into the retained SVG.");
}

const editorButton = [...mount.querySelectorAll("button")].find(
  (button) => button.textContent.trim() === "Editor",
);
editorButton?.click();
await window.happyDOM.waitUntilComplete();

const editorSymbols =
  persistentWorksurface?.querySelectorAll(
    '#persistent-layer-units [data-unit-id][role="button"]',
  ) ?? [];
if (
  mount?.querySelector("#persistent-tactical-svg") !== persistentWorksurface ||
  persistentWorksurface?.getAttribute("data-scene-owner") !== "EditorScene" ||
  editorSymbols.length !== 4 ||
  mount?.querySelector('[aria-label="Editable map grid"]') ||
  mount?.querySelector('[aria-label="SVG tactical map workspace"]')
) {
  throw new Error(
    "The generated Editor projection did not retain one SVG with four semantic units and no legacy root.",
  );
}

const rulesButton = [...mount.querySelectorAll("button")].find(
  (button) => button.textContent.trim() === "Rules",
);
rulesButton?.click();
await window.happyDOM.waitUntilComplete();

const rulesPanel = mount?.querySelector('[data-panel-id="rules"]');
const catalog = mount?.querySelector('[aria-label="Design scenario catalog"]');
const scenarioButtons = [
  ...catalog?.querySelectorAll('button[aria-label^="Simulate design scenario"]') ?? [],
];

if (
  !rulesPanel ||
  mount?.querySelector("#persistent-tactical-svg") !== persistentWorksurface ||
  scenarioButtons.length !== 6 ||
  !catalog?.textContent.includes("Lethality threshold")
) {
  throw new Error("The mounted documentation application has no registered runnable Rules panel.");
}

scenarioButtons[0].click();
await window.happyDOM.waitUntilComplete();

if (
  !workerMessages.some((message) =>
    JSON.stringify(message).includes("adjacent-duel"),
  )
) {
  throw new Error("The generated-site scenario action did not reach the worker.");
}

const result = mount?.querySelector('[aria-label="Laboratory results"]');
const dataButton = [...mount.querySelectorAll("button")].find(
  (button) => button.textContent.trim() === "Data",
);
dataButton?.click();
await window.happyDOM.waitUntilComplete();
const rulesData = mount?.querySelector('[aria-label="Rules data tables"]');
const rulesTableCount = rulesData?.querySelectorAll("table").length ?? 0;
const deferredRulesDataIsValid =
  rulesTableCount === 0 ||
  (rulesTableCount === 7 &&
    rulesData?.textContent.includes("Point Man") &&
    rulesData?.textContent.includes("Rifle"));

if (
  mount?.querySelector("#persistent-tactical-svg") !== persistentWorksurface ||
  !mount?.querySelector('[data-panel-id="data"]') ||
  !result?.textContent.includes(
    "4 attacks resolved · 100 damage · target finishes on 0 HP",
  ) ||
  status?.textContent.includes("Loading replay") ||
  !deferredRulesDataIsValid
) {
  throw new Error(
    "The generated site omitted the immediate result or violated the deferred rules-data boundary.",
  );
}

console.log(
  `Documentation browser smoke passed: the unified tactical workspace uses the available width, the editor exposes canonical unit symbols, and registered Rules/Data panels retain six scenarios plus a valid deferred data boundary (${rulesTableCount} eagerly rendered tables) without replacing the SVG.`,
);

window.happyDOM.close();
