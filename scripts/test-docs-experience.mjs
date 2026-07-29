import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { pathToFileURL } from "node:url";
import { Window } from "happy-dom";

const site = resolve(process.argv[2] ?? "artifacts/site");
const html = await readFile(resolve(site, "index.html"), "utf8");
const window = new Window({
  url: "https://ehotwagner.github.io/S.I.R./index.html",
});

window.matchMedia = () => ({
  matches: false,
  addEventListener() {},
  removeEventListener() {},
});
window.document.write(html);

Object.assign(globalThis, {
  window,
  document: window.document,
  Event: window.Event,
  KeyboardEvent: window.KeyboardEvent,
  requestAnimationFrame: window.requestAnimationFrame.bind(window),
});

await import(pathToFileURL(resolve(site, "content/sir-docs.js")));
window.document.dispatchEvent(new window.Event("DOMContentLoaded"));
await window.happyDOM.waitUntilComplete();

const failures = [];
const require = (condition, message) => {
  if (!condition) failures.push(message);
};

const explainer = window.document.querySelector("[data-svg-explainer]");
const motion = explainer?.querySelector(".sir-motion-toggle");
const targets = [...(explainer?.querySelectorAll("[data-svg-tip]") ?? [])];

require(
  window.document.querySelector(".sir-brand-subtitle")?.textContent ===
    "Field Manual",
  "the documentation brand subtitle is missing",
);
require(Boolean(explainer?.querySelector("title")), "the model SVG has no title");
require(Boolean(explainer?.querySelector("desc")), "the model SVG has no description");
require(targets.length === 7, "the model SVG does not expose seven explained stages");
require(!explainer?.querySelector("pre"), "SVG markup was rendered as a code block");

motion?.click();
require(explainer?.classList.contains("is-paused"), "motion cannot be paused");
require(motion?.getAttribute("aria-pressed") === "true", "pause state is not announced");
motion?.click();
require(!explainer?.classList.contains("is-paused"), "motion cannot be resumed");

targets[0]?.dispatchEvent(new window.Event("mouseenter"));
await window.happyDOM.waitUntilComplete();
const tooltip = window.document.querySelector(".sir-svg-tooltip");
require(!tooltip?.hidden, "the SVG tooltip did not open");
require(
  tooltip?.textContent.includes("Bounded numbers"),
  "the SVG tooltip did not describe the focused stage",
);
targets[0]?.dispatchEvent(new window.Event("mouseleave"));
require(tooltip?.hidden, "the SVG tooltip did not close");

if (failures.length > 0) {
  throw new Error(`Documentation experience gate failed: ${failures.join("; ")}.`);
}

console.log(
  "Documentation experience passed: structured brand, seven-stage SVG, accessible tooltip, and motion pause/resume.",
);
window.happyDOM.close();
