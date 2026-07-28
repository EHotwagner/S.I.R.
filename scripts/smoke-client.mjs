import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { pathToFileURL } from "node:url";
import { Window } from "happy-dom";

const output = resolve("artifacts/client");
const html = await readFile(resolve(output, "index.html"), "utf8");
const scriptMatch = html.match(/<script[^>]+src="([^"]+\.js)"/);

if (!scriptMatch) {
  throw new Error("The production HTML does not reference a JavaScript bundle.");
}

const window = new Window({ url: "https://sir.invalid/replay/" });
window.document.body.innerHTML = '<div id="sir-replay-app"></div>';

Object.assign(globalThis, {
  window,
  document: window.document,
  Node: window.Node,
  Element: window.Element,
  HTMLElement: window.HTMLElement,
  Event: window.Event,
  KeyboardEvent: window.KeyboardEvent,
  MouseEvent: window.MouseEvent,
});

const bundle = resolve(output, scriptMatch[1].replace(/^\.\//, ""));
await import(pathToFileURL(bundle));
await window.happyDOM.waitUntilComplete();

const heading = window.document.querySelector("h1");
const status = window.document.querySelector('[role="status"]');
const fileInput = window.document.querySelector('input[type="file"]');
const labelledButtons = [...window.document.querySelectorAll("button")].filter(
  (button) => button.getAttribute("aria-label"),
);

if (heading?.textContent !== "Replay shell") {
  throw new Error("The React replay shell did not mount.");
}

if (!status?.textContent.includes("No replay loaded")) {
  throw new Error("The initial verification status is missing.");
}

if (fileInput?.getAttribute("aria-label") !== "Choose replay package") {
  throw new Error("The replay file control has no accessible name.");
}

if (labelledButtons.length < 6) {
  throw new Error("Primary replay controls are missing accessible names.");
}

console.log(
  `Browser smoke passed: React mounted with ${labelledButtons.length} labelled controls and a live verification status.`,
);

window.happyDOM.close();
