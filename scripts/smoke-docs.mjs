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

class SmokeWorker {
  postMessage(message) {
    workerMessages.push(message);
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
  'main[aria-label="Replay and rules laboratory application"]',
);
const status = mount?.querySelector('[role="status"]');
const catalog = mount?.querySelector('[aria-label="Design scenario catalog"]');

if (!application || mount?.querySelector("header, h1")) {
  throw new Error("The Fable application did not mount inside the fsdocs page.");
}

if (!status?.textContent.includes("Ready — choose a scenario or load a replay")) {
  throw new Error("The mounted documentation application has no scenario call to action.");
}

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
  !result?.textContent.includes("Simulation result") ||
  rulesData?.querySelectorAll("table").length !== 7 ||
  !rulesData?.textContent.includes("Point Man") ||
  !rulesData?.textContent.includes("Rifle")
) {
  throw new Error("The generated site omitted the immediate result or rules-data tables.");
}

console.log(
  "Documentation browser smoke passed: the Fable application has no duplicate masthead, exposes six explicit simulation actions and an immediate-result panel, and includes seven unit/perk/weapon/equipment tables.",
);

window.happyDOM.close();
