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

if (!application || mount?.querySelector("header, h1")) {
  throw new Error("The Fable application did not mount inside the fsdocs page.");
}

const simulateButton = [
  ...mount.querySelectorAll('[aria-label="Map editor menu and toolbar"] button'),
].find(
  (button) => button.textContent.trim() === "Simulate",
);
if (!simulateButton) {
  throw new Error("The generated editor omitted the explicit simulator revision handoff.");
}
simulateButton.click();
await window.happyDOM.waitUntilComplete();

const controlsButton = [...mount.querySelectorAll("button")].find(
  (button) => button.textContent.trim() === "Controls",
);
controlsButton?.click();
await window.happyDOM.waitUntilComplete();

if (
  !mount?.querySelector('[aria-label="Simulator menu and toolbar"]') ||
  !mount?.querySelector('[aria-label="Simulator command panel"]') ||
  !mount?.textContent.includes("Manual") ||
  !mount?.textContent.includes("Scripted AI") ||
  !mount?.textContent.includes("General AI") ||
  !mount?.querySelector('[aria-label="Editable simulation SVG battlefield"]')
) {
  throw new Error("The generated site did not hand the immutable revision to the desktop simulator.");
}

const editorButton = [...mount.querySelectorAll("button")].find(
  (button) => button.textContent.trim() === "Editor",
);
editorButton?.click();
await window.happyDOM.waitUntilComplete();

const editorWorkspace = mount?.querySelector(
  '[aria-label="SVG tactical map workspace"] svg[role="application"]',
);
const objectList = mount?.querySelector(
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
  mount?.querySelector('[aria-label="Editable map grid"]')
) {
  throw new Error(
    "The generated Editor tab did not use the SVG workspace, canonical square-unit symbols, and object-list fallback.",
  );
}

const rulesButton = [...mount.querySelectorAll("button")].find(
  (button) => button.textContent.trim() === "Rules and data",
);
rulesButton?.click();
await window.happyDOM.waitUntilComplete();

const catalog = mount?.querySelector('[aria-label="Design scenario catalog"]');
const scenarioButtons = [
  ...catalog?.querySelectorAll('button[aria-label^="Simulate design scenario"]') ?? [],
];

if (
  scenarioButtons.length !== 6 ||
  !catalog?.textContent.includes("Lethality threshold")
) {
  throw new Error("The mounted documentation application has no runnable scenario gallery.");
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
const rulesData = mount?.querySelector('[aria-label="Rules data tables"]');

if (
  !result?.textContent.includes(
    "4 attacks resolved · 100 damage · target finishes on 0 HP",
  ) ||
  status?.textContent.includes("Loading replay") ||
  rulesData?.querySelectorAll("table").length !== 7 ||
  !rulesData?.textContent.includes("Point Man") ||
  !rulesData?.textContent.includes("Rifle")
) {
  throw new Error("The generated site omitted the immediate result or rules-data tables.");
}

console.log(
  "Documentation browser smoke passed: the unified tactical workspace uses the available width, the editor exposes canonical unit symbols, and the rules workspace retains six scenarios and seven data tables.",
);

window.happyDOM.close();
