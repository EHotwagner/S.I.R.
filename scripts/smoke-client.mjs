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
    ProtocolVersion: 2,
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

const bundle = resolve(output, scriptMatch[1].replace(/^\.\//, ""));
await import(pathToFileURL(bundle));
await window.happyDOM.waitUntilComplete();

const application = window.document.querySelector(
  'main[aria-label="Replay and rules laboratory application"]',
);
const status = window.document.querySelector('[role="status"]');
const fileInput = window.document.querySelector('input[type="file"]');
const labelledButtons = [...window.document.querySelectorAll("button")].filter(
  (button) => button.getAttribute("aria-label"),
);

if (!application || application.querySelector("header, h1")) {
  throw new Error("The React replay shell did not mount.");
}

if (!status?.textContent.includes("Ready — choose a scenario or load a replay")) {
  throw new Error("The initial scenario/replay call to action is missing.");
}

if (!status?.textContent.includes("Authoritative verification is available only from .NET exact-artifact WASM re-execution.")) {
  throw new Error("The authoritative verification boundary is missing.");
}

if (fileInput?.getAttribute("aria-label") !== "Choose replay package") {
  throw new Error("The replay file control has no accessible name.");
}

if (labelledButtons.length < 4) {
  throw new Error("Primary replay controls are missing accessible names.");
}

const inspector = window.document.querySelector('[aria-label="Replay inspector"]');
const workerStatus = window.document.querySelector(".worker-status");
const catalog = window.document.querySelector('[aria-label="Design scenario catalog"]');
const labResults = window.document.querySelector('[aria-label="Laboratory results"]');

if (!inspector?.textContent.includes("Timeline and events")) {
  throw new Error("The responsive replay inspectors did not mount.");
}

if (!workerStatus?.textContent.includes("protocol 2")) {
  throw new Error("The worker protocol disclosure is missing.");
}

if (
  !catalog?.textContent.includes("Four-hit baseline") ||
  !catalog?.textContent.includes("Single heavy strike") ||
  !catalog?.textContent.includes("Near-threshold survivor")
) {
  throw new Error("The interactive design-scenario gallery did not mount.");
}

if (
  !labResults?.textContent.includes("Simulation result") ||
  !labResults?.textContent.includes(
    "Click “Simulate now” on any scenario above.",
  )
) {
  throw new Error("The laboratory comparison surface did not mount.");
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
  `Browser smoke passed: React mounted without the duplicate masthead, exposed ${scenarioButtons.length} explicit simulation actions and an immediate-result panel, rendered ${rulesTables.length} rules-data tables, and sent the selected scenario to the worker.`,
);

window.happyDOM.close();
