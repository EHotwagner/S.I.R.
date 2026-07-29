import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { pathToFileURL } from "node:url";
import { Window } from "happy-dom";

const clientOutput = resolve(process.argv[2] ?? "artifacts/client");
const html = await readFile(resolve(clientOutput, "index.html"), "utf8");
const scriptMatch = html.match(/<script[^>]+src="([^"]+\.js)"/);
const stylesMatch = html.match(/<link[^>]+href="([^"]+\.css)"/);
if (!scriptMatch || !stylesMatch) {
  throw new Error("The production client assets are missing.");
}
const styles = await readFile(
  resolve(clientOutput, stylesMatch[1].replace(/^\.\//, "")),
  "utf8",
);
const window = new Window({ url: "https://sir.invalid/qualification/" });
window.document.body.innerHTML = '<div id="sir-replay-app"></div>';
class QualificationWorker {
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
  Worker: QualificationWorker,
});
await import(
  pathToFileURL(resolve(clientOutput, scriptMatch[1].replace(/^\.\//, "")))
);
await window.happyDOM.waitUntilComplete();

const failures = [];
const require = (condition, message) => {
  if (!condition) failures.push(message);
};
const buttonByText = (text) =>
  [...window.document.querySelectorAll("button")].find(
    (button) => button.textContent.trim() === text,
  );
const workspace = () =>
  window.document.querySelector(
    '[aria-label="SVG tactical map workspace"] svg[role="application"]',
  );

require(Boolean(workspace()?.querySelector("title")), "workspace title is missing");
require(Boolean(workspace()?.querySelector("desc")), "workspace description is missing");
require(workspace()?.getAttribute("tabindex") === "0", "workspace is not one keyboard stop");
require(
  workspace()?.querySelectorAll('[data-editor-unit-id][role="button"]').length === 4,
  "semantic units are not exposed as four named SVG buttons",
);
require(
  workspace()?.querySelectorAll('[data-editor-unit-id][tabindex="0"]').length === 1,
  "SVG unit focus is not roving",
);
require(
  Boolean(window.document.querySelector('[aria-label="Map object list fallback"]')),
  "parallel keyboard/screen-reader object list is missing",
);
require(
  Boolean(window.document.querySelector('[aria-live="polite"]')),
  "editor changes have no polite announcement channel",
);

for (const control of window.document.querySelectorAll(
  "#sir-replay-app button, #sir-replay-app input, #sir-replay-app select",
)) {
  const id = control.getAttribute("id");
  require(
    Boolean(
      control.getAttribute("aria-label") ||
        control.textContent.trim() ||
        (id && window.document.querySelector(`label[for="${id}"]`)),
    ),
    `${control.tagName.toLowerCase()} ${id ?? "(without id)"} has no accessible name`,
  );
}

window.dispatchEvent(
  new window.KeyboardEvent("keydown", { key: "t", bubbles: true }),
);
await window.happyDOM.waitUntilComplete();
require(
  Boolean(window.document.querySelector('[aria-label="Terrain palette"]')),
  "keyboard-only T shortcut did not expose terrain tools",
);
require(
  styles.includes("touch-action:none"),
  "touch gestures can be intercepted by page scrolling",
);
require(
  /@media\s*\(forced-colors:?active\)/.test(styles) &&
    styles.includes("forced-color-adjust:auto"),
  "forced-colors support is missing",
);
require(
  /@media\s*\(prefers-reduced-motion:?reduce\)/.test(styles) &&
    styles.includes("animation-duration:.01ms!important"),
  "reduced-motion override is missing",
);
require(
  /@media\s*\((max-width:)?48rem|width<=48rem\)/.test(styles) &&
    styles.includes(".editor-workspace{grid-template-columns:1fr}"),
  "the 400% zoom/narrow-layout collapse is missing",
);
require(
  styles.includes("min-width:2.75rem") || styles.includes("min-height:2.75rem"),
  "44 CSS pixel target sizing is not represented",
);

buttonByText("Map file")?.click();
await window.happyDOM.waitUntilComplete();
require(
  Boolean(window.document.querySelector('[aria-label="Map validation issues"]')),
  "validation issues have no HTML assistive-technology authority",
);
require(
  Boolean(window.document.querySelector('[aria-label="Editing layer states"]')),
  "layer state controls have no accessible grouping",
);

const reviewRoot = resolve("docs/assets/map-editor-review");
const manifest = JSON.parse(
  await readFile(resolve(reviewRoot, "manifest.json"), "utf8"),
);
const currentBundle = await readFile(
  resolve(clientOutput, scriptMatch[1].replace(/^\.\//, "")),
);
require(
  createHash("sha256").update(currentBundle).digest("hex") ===
    manifest.productionBundleSha256,
  "review artifacts were not regenerated from the current production bundle",
);
require(manifest.files.length === 7, "review manifest does not cover seven domains");
for (const file of manifest.files) {
  const svg = await readFile(resolve(reviewRoot, file.svg));
  const png = await readFile(resolve(reviewRoot, file.png));
  require(
    createHash("sha256").update(svg).digest("hex") === file.svgSha256,
    `${file.domain} SVG hash drifted`,
  );
  require(
    createHash("sha256").update(png).digest("hex") === file.pngSha256,
    `${file.domain} PNG hash drifted`,
  );
}

if (failures.length > 0) {
  throw new Error(`Map-editor qualification failed: ${failures.join("; ")}.`);
}
console.log(
  "Map-editor automated qualification passed: keyboard structure, screen-reader semantics, touch CSS, forced colors, 400% responsive collapse, reduced motion, target sizing, and seven hashed SVG/PNG domain pairs.",
);
window.happyDOM.close();
