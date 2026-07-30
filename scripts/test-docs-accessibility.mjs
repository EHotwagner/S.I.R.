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

const replayButton = [...window.document.querySelectorAll(
  "#sir-replay-app button",
)].find((button) => button.textContent.trim() === "Review");
replayButton?.click();
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

const battlefield = window.document.querySelector(
  '#sir-replay-app svg[role="application"]',
);
const units = [...(battlefield?.querySelectorAll("[data-unit-id]") ?? [])];
require(Boolean(battlefield?.querySelector("title")), "the battlefield has no title");
require(Boolean(battlefield?.querySelector("desc")), "the battlefield has no description");
require(
  battlefield?.getAttribute("aria-label")?.includes("exact tick 24"),
  "the battlefield accessible name omits its exact committed tick",
);
require(units.length === 6, "the representative battlefield does not expose six units");
require(
  units.every(
    (unit) =>
      unit.getAttribute("role") === "button" &&
      Boolean(unit.getAttribute("aria-label")),
  ),
  "a disclosed SVG unit has no interactive role or accessible name",
);
require(
  units.filter((unit) => unit.getAttribute("tabindex") === "0").length === 1,
  "the battlefield does not have exactly one roving tab stop",
);
require(
  window.document.querySelector(
    '[aria-label="Battlefield unit inspector"]',
  ),
  "the SVG unit information has no equivalent HTML inspector",
);
require(
  window.document.querySelector('[aria-label="Battlefield legend"]'),
  "the battlefield channel legend is missing",
);
require(
  battlefield?.querySelectorAll("[data-secondary-heading][aria-label]").length ===
    2,
  "an explicitly sourced second heading lacks accessible source text",
);
require(
  window.document.querySelector(
    '[aria-label="Semantic replay timeline lanes"]',
  )?.querySelectorAll("[data-timeline-lane]").length === 3,
  "the semantic event lanes are not represented in accessible HTML",
);
require(
  [...window.document.querySelectorAll(".battlefield-sidecar label")].some(
    (label) => label.textContent.includes("Reduced motion"),
  ),
  "the explicit reduced-motion setting is unavailable",
);
require(
  Boolean(
    window.document.querySelector(
      'button[aria-label="Step backward one committed replay tick"]',
    ),
  ),
  "the exact reverse-step control is missing",
);
require(
  Boolean(
    window.document.querySelector(
      'button[aria-label="Go to previous disclosed replay event"]',
    ),
  ) &&
    Boolean(
      window.document.querySelector(
        'button[aria-label="Go to next disclosed replay event"]',
      ),
    ),
  "event navigation does not expose equivalent accessible controls",
);
require(
  Boolean(
    window.document.querySelector(
      '[aria-label="Static SVG battlefield demonstration"]',
    ),
  ),
  "the unloaded static demonstration is not explicitly separated from playback",
);

const editorButton = [...window.document.querySelectorAll(
  "#sir-replay-app button",
)].find((button) => button.textContent.trim() === "Editor");
editorButton?.click();
await window.happyDOM.waitUntilComplete();
const mapFileButton = [...window.document.querySelectorAll(
  "#sir-replay-app button",
)].find((button) => button.textContent.trim() === "Map file");
mapFileButton?.click();
await window.happyDOM.waitUntilComplete();

require(
  Boolean(window.document.querySelector('[aria-label="Editing layer states"]')),
  "editing layer states have no accessible group",
);
require(
  Boolean(window.document.querySelector('label[for="map-name"]')),
  "map authoring name has no explicit label",
);
require(
  Boolean(window.document.querySelector('[aria-label="Map validation issues"]')),
  "validation issues have no accessible HTML panel",
);
require(
  [...window.document.querySelectorAll("#sir-replay-app button")].some(
    (button) => button.textContent.trim() === "Previous",
  ) &&
    [...window.document.querySelectorAll("#sir-replay-app button")].some(
      (button) => button.textContent.trim() === "Next",
    ),
  "validation issues lack previous/next controls",
);
const clearMapButton = [...window.document.querySelectorAll(
  "#sir-replay-app button",
)].find((button) => button.textContent.trim() === "Clear");
clearMapButton?.click();
await window.happyDOM.waitUntilComplete();
require(
  Boolean(
    [...window.document.querySelectorAll('[role="alertdialog"]')].find((dialog) =>
      dialog.textContent.includes("Confirmation required"),
    ),
  ),
  "destructive map changes do not expose an explicit alert dialog",
);

if (failures.length > 0) {
  throw new Error(`Accessibility gate failed: ${failures.join("; ")}.`);
}

console.log(
  "Documentation accessibility passed: language, title, fallback, live status, exact step/event controls, separated static demonstration, application regions, SVG title/description, six named units, roving focus, HTML inspector, and legend.",
);
window.happyDOM.close();
