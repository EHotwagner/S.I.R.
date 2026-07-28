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

class SmokeWorker {
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
  Worker: SmokeWorker,
});

const bundle = resolve(site, "content/sir-client/v1/app.js");
await import(pathToFileURL(bundle));
await window.happyDOM.waitUntilComplete();

const mount = window.document.getElementById("sir-replay-app");
const heading = mount?.querySelector("h1");
const status = mount?.querySelector('[role="status"]');
const catalog = mount?.querySelector('[aria-label="Design scenario catalog"]');

if (heading?.textContent !== "Replay and rules laboratory") {
  throw new Error("The Fable application did not mount inside the fsdocs page.");
}

if (!status?.textContent.includes("No replay loaded")) {
  throw new Error("The mounted documentation application has no live status.");
}

if (!catalog?.textContent.includes("Adjacent duel")) {
  throw new Error("The mounted documentation application has no scenario catalog.");
}

console.log(
  "Documentation browser smoke passed: the Fable application mounted inside the generated fsdocs page with live status and the scenario catalog.",
);

window.happyDOM.close();
