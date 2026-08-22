import { execFile } from "node:child_process";
import { promisify } from "node:util";
import { readFile } from "node:fs/promises";

const execFileAsync = promisify(execFile);
const [appRoot, styles, projection, projectionContract, simulator, protocol, runner, clientTests, projectionTests, tacticalSharedControls, tacticalScenePresentation] =
  await Promise.all([
    readFile("src/SIR.Client.Web/App.fs", "utf8"),
    readFile("src/SIR.Client.Web/styles.css", "utf8"),
    readFile("src/SIR.Client/TacticalSceneProjection.fs", "utf8"),
    readFile("src/SIR.Client/TacticalSceneProjection.fsi", "utf8"),
    readFile("src/SIR.Client/MapEditorSimulator.fs", "utf8"),
    readFile("src/SIR.Client/SimulatorWorkerProtocol.fs", "utf8"),
    readFile("src/SIR.Client.Web/Runner.fs", "utf8"),
    readFile("tests/SIR.Client.Tests/Program.fs", "utf8"),
    readFile("tests/SIR.Client.Tests/TacticalSceneProjectionQualification.fs", "utf8"),
    readFile("src/SIR.Client.Web/TacticalSharedControls.fs", "utf8"),
    readFile("src/SIR.Client.Web/TacticalScenePresentation.fs", "utf8"),
  ]);

// The tactical scene owner is an explicit view boundary extracted from App
// (SIR.Client.Web.fsproj compiles it just before App.fs).  Qualify the composed
// ownership surface, exactly as M0 and M7 already do, so relocating a view can
// never mask a removed control while App.fs remains the root Elmish shell.
const app = `${appRoot}\n${tacticalSharedControls}\n${tacticalScenePresentation}`;

const require = (condition, message) => {
  if (!condition) throw new Error(`Simulator M6 qualification failed: ${message}`);
};

for (const obsolete of [
  "simulatorDesktopChrome",
  "simulatorDock",
  "SimulatorToolPanel",
  'prop.className "simulator-workspace"',
  'prop.className "simulator-owner-controls"',
  'prop.className "simulator-revision-status"',
  'prop.ariaLabel "Simulator menu and toolbar"',
  'prop.ariaLabel "Simulator command panel"',
]) {
  require(!app.includes(obsolete), `legacy simulator renderer/control remains: ${obsolete}`);
}
for (const obsolete of [
  ".simulator-workspace",
  ".simulator-revision-status",
  ".simulator-map-stage",
  ".simulator-ribbon",
  ".simulator-context-palette",
]) {
  require(!styles.includes(obsolete), `legacy simulator page CSS remains: ${obsolete}`);
}

for (const required of [
  "simulatorPanelBody",
  'prop.ariaLabel "Simulator runtime roster"',
  'prop.ariaLabel "Simulator runtime tools"',
  'prop.ariaLabel "Simulation controllers"',
  'prop.ariaLabel "Simulator runtime diagnostics"',
  'prop.ariaLabel "Simulator maintained revision state"',
  'svg.id "persistent-layer-routes"',
  'svg.id "persistent-layer-units"',
  'svg.id "persistent-layer-annotations"',
  '"data-presentation-column"',
  '"data-presentation-row"',
  '"data-scene-disclosure"',
]) {
  require(app.includes(required), `registered/shared simulator token is missing: ${required}`);
}

const resetBranch = app.slice(
  app.indexOf("| ResetSimulator ->"),
  app.indexOf("| PlanningChanged action ->"),
);
require(
  resetBranch.includes("MapEditorSimulator.reset current") &&
    !resetBranch.includes("model.Editor") &&
    simulator.includes("let reset (handoff: SimulatorHandoff)") &&
    simulator.includes("fromRevision handoff.Revision") &&
    projectionTests.includes("resetRuntime = simulatorBase") &&
    projectionTests.includes("exact pinned immutable handoff baseline"),
  "reset can recapture the mutable Editor or lacks an exact pinned-baseline regression",
);
for (const required of [
  "PresentationColumn: float",
  "PresentationRow: float",
]) {
  require(projectionContract.includes(required), `public projection contract is missing: ${required}`);
}
for (const required of [
  "PresentationPositions",
  'yield "moving"',
  'yield "movement-intent"',
  'yield "route-planned"',
  'Kind = "simulator-state"',
  "Disclosure = disclosure SandboxDisclosure",
]) {
  require(projection.includes(required), `runtime projection token is missing: ${required}`);
}

for (const required of [
  "envelope.Kind = Kind",
  "envelope.ProtocolVersion = CurrentVersion",
  "active.Operation = envelope.Correlation.Operation",
  "active.Session = envelope.Correlation.Session",
  "active.MapRevision = envelope.Correlation.MapRevision",
  "active.PlanRevision = envelope.Correlation.PlanRevision",
  "active.Tick = envelope.Correlation.Tick",
]) {
  require(protocol.includes(required), `exact worker correlation guard is missing: ${required}`);
}
require(
  runner.includes("if SimulatorProtocol.accepts envelope simulatorGuard then") &&
    runner.includes("SimulatorProtocol.completeOperation"),
  "worker responses do not fail closed before UI dispatch or remain pending after terminal completion",
);
for (const required of [
  "staleSessionEnvelope",
  "staleOperationEnvelope",
  "wrongKindEnvelope",
  "wrongVersionEnvelope",
  "completedGuard",
]) {
  require(clientTests.includes(required), `adversarial correlation test is missing: ${required}`);
}

await import("./smoke-client.mjs");
const worker = await execFileAsync("node", ["scripts/smoke-worker-roundtrip.mjs"], {
  cwd: process.cwd(),
  maxBuffer: 4 * 1024 * 1024,
});
process.stdout.write(worker.stdout);
process.stderr.write(worker.stderr);

console.log(
  "Simulator M6 qualification passed: registered panels uniquely own runtime tools, controller configuration, diagnostics, revision state, and samples; the singleton shared SVG projects disposable runtime positions, movement, routes, controller state, annotations, and Sandbox disclosure; cancelled reset is inert and accepted reset restores the exact pinned handoff without consulting a newer Editor; local deterministic run/step/reset, authoritative runtime tick, projection-only scrubbing, and exact real-worker correlation remain fail-closed; legacy simulator renderer/CSS is absent.",
);
