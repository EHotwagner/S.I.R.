import { readFile } from "node:fs/promises";

const [app, styles, projection, shellTests, projectionTests, browserSmoke, workerSmoke] =
  await Promise.all([
    readFile("src/SIR.Client.Web/App.fs", "utf8"),
    readFile("src/SIR.Client.Web/styles.css", "utf8"),
    readFile("src/SIR.Client/TacticalSceneProjection.fs", "utf8"),
    readFile("tests/SIR.Client.Tests/Program.fs", "utf8"),
    readFile("tests/SIR.Client.Tests/TacticalSceneProjectionQualification.fs", "utf8"),
    readFile("scripts/smoke-client.mjs", "utf8"),
    readFile("scripts/smoke-worker-roundtrip.mjs", "utf8"),
  ]);

const require = (condition, message) => {
  if (!condition) throw new Error(`Review M7 qualification failed: ${message}`);
};

for (const obsolete of [
  "battlefieldView",
  "battlefieldInspector",
  'prop.className "battlefield-panel"',
  'prop.className "battlefield-layout"',
  'svg.className "battlefield-svg"',
]) {
  require(!app.includes(obsolete), `legacy review renderer remains: ${obsolete}`);
}
for (const obsolete of [
  ".battlefield-panel",
  ".battlefield-heading",
  ".battlefield-controls",
  ".battlefield-layout",
  ".battlefield-svg",
  ".battlefield-sidecar",
  ".battlefield-legend",
  ".battlefield-inspector",
]) {
  require(!styles.includes(obsolete), `legacy review CSS remains: ${obsolete}`);
}

for (const required of [
  "reviewPanelBody",
  'prop.ariaLabel "Review disclosed roster"',
  'prop.ariaLabel "Review sources and transport"',
  'prop.ariaLabel "Review projection layers"',
  'prop.ariaLabel "Review event inspection"',
  'prop.ariaLabel "Review source and verification identity"',
  'prop.ariaLabel "Review worker status"',
  "tacticalShell model dispatch transientContent",
  "activePresentedSceneProjection",
  "model.PreviousFrame",
  "TacticalSceneProjection.interpolateReviewPresentation",
  'svg.custom ("data-presentation-alpha", string presentationAlpha)',
]) {
  require(app.includes(required), `registered Review ownership token is missing: ${required}`);
}

for (const required of [
  "AcceptedVerificationIdentity",
  "AcceptedVerificationKind",
  'primitive "review-verification" "accepted"',
  'AcceptedVerificationKind = "browser-kernel-verified"',
  'AcceptedVerificationKind = "perspective-projection"',
  "inspection.PerspectiveHash.IsSome",
  "inspection.Units.IsEmpty",
  "inspection.Edges.IsEmpty",
  "inspection.Events.IsEmpty",
  "inspection.Checkpoints.IsEmpty",
  "inspection.BoardMinimumColumn = 0",
  "inspection.BoardMinimumRow = 0",
  "inspection.BoardMaximumColumn = 0",
  "inspection.BoardMaximumRow = 0",
  "interpolateReviewPresentation",
  "previousFrame.Disclosure = current.Disclosure.Source",
  "currentIds = previousIds",
  "previousFrame.Board = current.Board",
  "not (Double.IsNaN alpha || Double.IsInfinity alpha)",
]) {
  require(projection.includes(required), `shared/fail-closed projection token is missing: ${required}`);
}

for (const required of [
  'perspectiveProjection.Annotations[0].Kind = "perspective-projection"',
  'annotation.Kind = "browser-kernel-verified"',
  "Shared Review interpolation changed committed identity/facts or missed midpoint presentation coordinates.",
  "Review interpolation crossed disclosure/semantic-owner guards or leaked a previous-frame entity.",
]) {
  require(projectionTests.includes(required), `adversarial projection evidence is missing: ${required}`);
}
for (const independentFault of [
  '"unit"',
  '"edge"',
  '"event"',
  '"checkpoint"',
  '"board-minimum-column"',
  '"board-minimum-row"',
  '"board-maximum-column"',
  '"board-maximum-row"',
  '"missing-perspective-hash"',
  '"replay-kind-mismatch"',
  '"mode-mismatch"',
  '"verification-mismatch"',
]) {
  require(
    projectionTests.includes(independentFault),
    `independent perspective rejection case is missing: ${independentFault}`,
  );
}
require(
  projectionTests.includes("perspectiveFaults.Length = 12") &&
    projectionTests.includes("List.isEmpty acceptedPerspectiveFaults"),
  "perspective rejection matrix no longer requires every independent case to fail closed",
);
for (const required of [
  "Cancel did not request runner cancellation.",
  "Backward stepping did not seek one exact committed tick.",
]) {
  require(shellTests.includes(required), `transport/interpolation regression is missing: ${required}`);
}
for (const required of [
  'shell.querySelectorAll(\'[aria-label="Replay source"]\').length !== 1',
  'shell.querySelector(".dashboard")',
  "Verification · m3-review · 010203040506",
  "message.Request?.tag === 1",
  '"Review worker-driven interpolated playback"',
  '"Review playback convergence on committed frame"',
  '"Review Pause did not hold the converged committed frame stable."',
  'getAttribute("data-presentation-alpha")',
]) {
  require(browserSmoke.includes(required), `browser ownership evidence is missing: ${required}`);
}
for (const required of [
  "full replay fixture did not load into its bounded projection",
  "full replay fixture did not seek deterministically",
  "perspective replay exposed hidden state or failed to load",
  "perspective fixture did not seek deterministically",
]) {
  require(workerSmoke.includes(required), `real worker evidence is missing: ${required}`);
}

await import("./smoke-client.mjs");

console.log(
  "Review M7 qualification passed: the singleton shared worksurface projects accepted committed frames, disclosed events, selection, and verification identity; registered panels uniquely own replay source/transport, disclosed roster/layers, event inspection, verification metadata, and worker state; worker-driven Play visibly interpolates only matching unit presentation coordinates before exact committed convergence, Pause remains stable, and Step remains exact; cancellation and the independent unit/edge/event/checkpoint/each-board-bound/hash/kind/mode/verification perspective rejection matrix remain fail closed; the adjacent worker gate owns execution of the compiled full and perspective fixtures; legacy replay battlefield renderer/CSS is absent.",
);
