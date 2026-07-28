import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { pathToFileURL } from "node:url";
import { Window } from "happy-dom";

const site = resolve(process.argv[2] ?? "artifacts/site");
const html = await readFile(resolve(site, "interactive-rules-lab.html"), "utf8");
const window = new Window({
  url: "https://ehotwagner.github.io/S.I.R./interactive-rules-lab.html",
});
window.document.write(html);

class AccessibilityWorker {
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
  Worker: AccessibilityWorker,
});

await import(pathToFileURL(resolve(site, "content/sir-client/v1/app.js")));
await window.happyDOM.waitUntilComplete();

const failures = [];
const require = (condition, message) => {
  if (!condition) failures.push(message);
};

require(window.document.documentElement.lang === "en", "the page language is not English");
require(Boolean(window.document.title.trim()), "the page title is empty");
require(
  Boolean(window.document.querySelector("noscript")),
  "the no-JavaScript alternative is missing",
);
require(
  window.document.querySelector('[role="status"]')?.getAttribute("aria-live") ===
    "polite",
  "verification changes are not announced politely",
);

for (const button of window.document.querySelectorAll("#sir-replay-app button")) {
  require(
    Boolean(button.getAttribute("aria-label") || button.textContent.trim()),
    "a button has no accessible name",
  );
}

for (const input of window.document.querySelectorAll(
  "#sir-replay-app input, #sir-replay-app select",
)) {
  const id = input.getAttribute("id");
  require(
    Boolean(
      input.getAttribute("aria-label") ||
        (id && window.document.querySelector(`label[for="${id}"]`)),
    ),
    `${input.tagName.toLowerCase()} control ${id ?? "(without id)"} has no label`,
  );
}

for (const section of window.document.querySelectorAll("#sir-replay-app section")) {
  require(
    Boolean(
      section.getAttribute("aria-label") ||
        section.querySelector("h1, h2, h3"),
    ),
    "an application section has no accessible name or heading",
  );
}

if (failures.length > 0) {
  throw new Error(`Accessibility gate failed: ${failures.join("; ")}.`);
}

console.log(
  "Documentation accessibility passed: language, title, fallback, live status, controls, and application regions are named.",
);
window.happyDOM.close();
