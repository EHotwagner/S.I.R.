import { readFile } from "node:fs/promises";

const [appRoot, styles, projection, planning, tacticalSharedControls, tacticalScenePresentation] = await Promise.all([
  readFile("src/SIR.Client.Web/App.fs", "utf8"),
  readFile("src/SIR.Client.Web/styles.css", "utf8"),
  readFile("src/SIR.Client/TacticalSceneProjection.fs", "utf8"),
  readFile("src/SIR.Client/PlanningWorkspace.fs", "utf8"),
  readFile("src/SIR.Client.Web/TacticalSharedControls.fs", "utf8"),
  readFile("src/SIR.Client.Web/TacticalScenePresentation.fs", "utf8"),
]);

// The tactical scene owner is an explicit view boundary extracted from App
// (SIR.Client.Web.fsproj compiles it just before App.fs).  Qualify the composed
// ownership surface, exactly as M0 and M7 already do, so relocating a view can
// never mask a removed control while App.fs remains the root Elmish shell.
const app = `${appRoot}\n${tacticalSharedControls}\n${tacticalScenePresentation}`;

const require = (condition, message) => {
  if (!condition) throw new Error(`Planning M5 qualification failed: ${message}`);
};

for (const obsolete of [
  "let private planningBattlefield",
  "let private planningWorkspace",
  'prop.className "planning-owner-status"',
  'prop.className "planning-workspace"',
  'prop.className "planning-battlefield"',
  'prop.className "planning-cell-grid"',
  'prop.className "planning-timeline"',
]) {
  require(!app.includes(obsolete), `legacy planner renderer remains: ${obsolete}`);
}
for (const obsolete of [
  ".planning-workspace",
  ".planning-battlefield",
  ".planning-cell-grid",
  ".planning-timeline",
  ".planning-lane",
]) {
  require(!styles.includes(obsolete), `legacy planner page CSS remains: ${obsolete}`);
}
for (const required of [
  "planningPanelBody",
  'panelId = "roster"',
  'panelId = "tools"',
  'panelId = "selection"',
  'panelId = "validation"',
  'panelId = "document"',
  'svg.id "persistent-layer-routes"',
  'svg.id "persistent-layer-units"',
  'svg.id "persistent-layer-annotations"',
  'svg.custom ("data-route-kind", route.Kind)',
  'svg.custom ("data-annotation-kind", annotation.Kind)',
]) {
  require(app.includes(required), `persistent planner ownership token is missing: ${required}`);
}
for (const required of [
  'Kind = "validation"',
  "PlannedFacing direction",
  "PlannedAttention direction",
  "PlannedStance value",
  "PlannedHold",
  "PlannedEngagement _",
  "PlannedSynchronization _",
]) {
  require(projection.includes(required), `planning scene projection token is missing: ${required}`);
}
require(
  planning.includes("PendingRequest: PendingPlanningRequest option") &&
    planning.includes("envelope.Correlation = expected.Correlation") &&
    planning.includes("envelope.Kind = SimulatorProtocol.Kind") &&
    planning.includes("envelope.ProtocolVersion = SimulatorProtocol.CurrentVersion") &&
    planning.includes("preservesCommittedHistory"),
  "complete pending-request correlation or committed-history protection is missing",
);

await import("./smoke-client.mjs");

console.log(
  "Planning M5 qualification passed: registered panels own planner controls and worker status once; shared persistent units, authored routes, and annotations expose intent-safe prediction and validation; complete pending-request correlation, exact undo/redo, Preview→Validate→Commit, changed-document revalidation, committed protection, accessibility, and singleton SVG parity remain qualified; legacy planner renderer/CSS is absent.",
);
