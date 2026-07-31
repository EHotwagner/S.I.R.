import { readFile } from "node:fs/promises";

const [app, styles, layout, layoutInterface, layoutTests, browserSmoke, docsSmoke] =
  await Promise.all([
    readFile("src/SIR.Client.Web/App.fs", "utf8"),
    readFile("src/SIR.Client.Web/styles.css", "utf8"),
    readFile("src/SIR.Client/TacticalWorkspaceLayout.fs", "utf8"),
    readFile("src/SIR.Client/TacticalWorkspaceLayout.fsi", "utf8"),
    readFile("tests/SIR.Client.Tests/TacticalWorkspaceLayoutQualification.fs", "utf8"),
    readFile("scripts/smoke-client.mjs", "utf8"),
    readFile("scripts/smoke-docs.mjs", "utf8"),
  ]);

const require = (condition, message) => {
  if (!condition) throw new Error(`Timeline/supporting-panel M8 qualification failed: ${message}`);
};

for (const obsolete of ["RulesWorkspace", "SamplesWorkspace"]) {
  require(!app.includes(obsolete), `obsolete replacement-page branch remains: ${obsolete}`);
}
for (const obsolete of [".dashboard", ".samples-workspace"]) {
  require(!styles.includes(obsolete), `obsolete supporting-page CSS remains: ${obsolete}`);
}

for (const required of [
  'prop.ariaLabel "Unified tactical timeline"',
  'prop.ariaLabel "Authored, predicted, accepted, and committed timeline segments"',
  'prop.id "tactical-bottom-panel"',
  "prop.hidden (not bottomVisible)",
  'prop.id "tactical-bottom-panel-resize"',
  'prop.className "tactical-bottom-panel-content"',
  "prop.hidden bottomCollapsed",
  "prop.role.separator",
  'prop.custom ("aria-orientation", "horizontal")',
  "BeginLayoutBottomPanelResize",
  "ResizeLayoutBottomPanelKeyboard",
  "OpenSupportingPanel panelId",
  'if panelId = "rules" then',
  'elif panelId = "data" then',
  'elif panelId = "samples" then',
  'item "Rules" "rules"',
  'item "Data" "data"',
  'item "Samples" "samples"',
]) {
  require(app.includes(required), `persistent ownership token is missing: ${required}`);
}
require(
  (app.match(/tacticalTimeline model dispatch/g) ?? []).length === 2 &&
    (app.match(/let private tacticalTimeline model dispatch/g) ?? []).length === 1,
  "the unified timeline does not have exactly one definition and one production call site",
);
require(
  (app.match(/ariaLabel "Unified tactical timeline"/g) ?? []).length === 1,
  "the unified timeline landmark is duplicated",
);

for (const required of [
  "CollapsedInEditor = true",
  "CollapsedOutsideEditor = false",
  "Height = 152",
  "let resizeBottomPanel height profile",
  "Height = max 96 (min 480 height)",
]) {
  require(layout.includes(required), `layout persistence/default token is missing: ${required}`);
}
require(
  layoutInterface.includes("val resizeBottomPanel: int -> TacticalLayoutProfile -> TacticalLayoutProfile"),
  "bottom-panel resize is absent from the public client boundary",
);
for (const required of [
  "resizeBottomPanel 327",
  "resizeBottomPanel 1",
  "resizeBottomPanel 999",
  "resizedRoundTrip = Ok resized",
]) {
  require(layoutTests.includes(required), `resize clamp/round-trip regression is missing: ${required}`);
}

const resizeUpdateStart = app.indexOf("| ResizeLayoutBottomPanel height when");
const liveResize = app.slice(
  resizeUpdateStart,
  app.indexOf("| ResizeLayoutBottomPanel _", resizeUpdateStart),
);
require(
  liveResize.includes("Cmd.none") && !liveResize.includes("writeTacticalLayout"),
  "pointer-move resize writes persistence instead of coalescing to pointer end",
);
const resizeEnd = app.slice(
  app.indexOf("| EndLayoutBottomPanelResize", resizeUpdateStart),
  app.indexOf("| ResizeLayoutBottomPanelKeyboard", resizeUpdateStart),
);
require(
  resizeEnd.includes("writeTacticalLayout") && resizeEnd.includes("BottomPanelResizeActive"),
  "pointer resize does not persist exactly at the active session boundary",
);

for (const required of [
  "const storagePrototype = Object.getPrototypeOf(window.localStorage)",
  'key === "sir.tactical-layout.v1"',
  'getAttribute("aria-valuenow") !== "304"',
  "tacticalLayoutWrites !== writesBeforePointerResize + 1",
  'getAttribute("aria-valuenow") !== "320"',
  "window.document.activeElement !== resizeHandle",
  'window.document.querySelector(\'[aria-label="Unified tactical timeline"]\') !== timeline',
  "!timeline.isConnected",
  'viewport.getAttribute("data-active-modality") !== "Plan"',
  'window.document.activeElement !== rulesPanel.querySelector(".tactical-layout-panel-body")',
  'window.document.activeElement !== window.document.querySelector("#layout-show-rules")',
  'window.happyDOM.setInnerWidth(320)',
  'window.document.querySelector(".samples-workspace")',
  'reopenedSvg !== worksurface',
]) {
  require(browserSmoke.includes(required), `browser fail-closed evidence is missing: ${required}`);
}

for (const required of [
  'button.textContent.trim() === "Rules"',
  'button.textContent.trim() === "Data"',
  'mount?.querySelector(\'[data-panel-id="rules"]\')',
  'mount?.querySelector(\'[data-panel-id="data"]\')',
  'mount?.querySelector("#persistent-tactical-svg") !== persistentWorksurface',
]) {
  require(docsSmoke.includes(required), `generated-site panel evidence is missing: ${required}`);
}

await import("./smoke-client.mjs");

console.log(
  "Timeline/supporting-panel M8 qualification passed: one mounted timeline retains one cursor and authored/predicted/accepted/committed lanes; pointer resize coalesces persistence, keyboard resize persists and restores focus, modality collapse defaults and strict height round-trips remain deterministic; Rules, Data, and Samples are registered panels; native input, narrow responsive access, exact SVG/timeline identity, camera, and valid selection survive all qualified operations; obsolete replacement-page branches and CSS are absent.",
);
