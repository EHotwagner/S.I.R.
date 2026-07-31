import { createHash } from "node:crypto";
import { execFileSync } from "node:child_process";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { auditPersistentWorkspaceBrowser } from "./lib/persistent-workspace-browser-audit.mjs";

const clientOutput = resolve(process.argv[2] ?? "artifacts/client");
const html = await readFile(resolve(clientOutput, "index.html"), "utf8");
const scriptMatch = html.match(/<script[^>]+src="([^"]+\.js)"/);
if (!scriptMatch) throw new Error("M9 acceptance requires a production client build.");
const bundlePath = resolve(clientOutput, scriptMatch[1].replace(/^\.\//, ""));
const bundleBytes = await readFile(bundlePath);
const bundle = bundleBytes.toString("utf8");
const builtStylesPath = resolve(clientOutput, "content/sir-client/v1/styles.css");
const builtStylesBytes = await readFile(builtStylesPath);
const builtStyles = builtStylesBytes.toString("utf8");

const [app, styles, smoke, accessibility, layoutTests, projectionTests, report, workspace, roadmap, reviewManifestText, reviewGeometrySvg, reviewPng, mockup] =
  await Promise.all([
    readFile("src/SIR.Client.Web/App.fs", "utf8"),
    readFile("src/SIR.Client.Web/styles.css", "utf8"),
    readFile("scripts/smoke-client.mjs", "utf8"),
    readFile("scripts/test-docs-accessibility.mjs", "utf8"),
    readFile("tests/SIR.Client.Tests/TacticalWorkspaceLayoutQualification.fs", "utf8"),
    readFile("tests/SIR.Client.Tests/TacticalSceneProjectionQualification.fs", "utf8"),
    readFile("docs/2026-07-31-0840-vscode-style-persistent-tactical-workspace-design-report.md", "utf8"),
    readFile("docs/unified-tactical-workspace.md", "utf8"),
    readFile("docs/unified-tactical-workspace-roadmap.md", "utf8"),
    readFile("docs/assets/persistent-workspace-m9-review/manifest.json", "utf8"),
    readFile("docs/assets/persistent-workspace-m9-review/field-focus-geometry.svg"),
    readFile("docs/assets/persistent-workspace-m9-review/field-focus.png"),
    readFile("docs/assets/persistent-workspace-mockups/index.html"),
  ]);
const reviewManifest = JSON.parse(reviewManifestText);
const hash = (bytes) => createHash("sha256").update(bytes).digest("hex");
const require = (condition, message) => {
  if (!condition) throw new Error(`Persistent workspace M9 acceptance failed: ${message}`);
};
const liveBrowserAudit = await auditPersistentWorkspaceBrowser({ clientRoot: clientOutput });

for (const obsolete of [
  "editorBattlefield",
  "planningBattlefield",
  "battlefieldView",
  "battlefieldInspector",
  "tacticalPersistentBattlefield",
  'prop.id "tactical-battlefield-viewport"',
  'prop.className "tactical-compatibility-surface"',
  'prop.custom ("data-migration-boundary"',
  'prop.className "tactical-workspace-content"',
  'prop.className "editor-workspace"',
  "editor-ribbon",
  "RulesWorkspace",
  "SamplesWorkspace",
]) {
  require(!app.includes(obsolete), `superseded source path remains: ${obsolete}`);
}
for (const obsolete of [
  ".tactical-battlefield-viewport",
  ".tactical-compatibility-surface",
  ".tactical-workspace-content",
  ".editor-workspace",
  ".editor-ribbon",
  ".planning-workspace",
  ".simulator-workspace",
  ".battlefield-panel",
  ".battlefield-layout",
  ".battlefield-svg",
  ".dashboard",
  ".samples-workspace",
]) {
  require(!styles.includes(obsolete), `superseded CSS layout remains: ${obsolete}`);
  require(!builtStyles.includes(obsolete), `superseded CSS reached the production bundle: ${obsolete}`);
}
for (const obsolete of [
  "tactical-battlefield-viewport",
  "tactical-compatibility-surface",
  "data-migration-boundary",
  "SVG tactical map workspace",
  "Editable map grid",
  "Battlefield route authoring",
  "Editable simulation SVG battlefield",
  "Loaded replay SVG battlefield",
  "editor-workspace",
  "editor-ribbon",
  "planning-workspace",
  "simulator-workspace",
]) {
  require(!bundle.includes(obsolete), `alternate/legacy landmark reached production JavaScript: ${obsolete}`);
}

require(
  (app.match(/let private persistentSceneSvg/g) ?? []).length === 1 &&
    (app.match(/svg\.id "persistent-tactical-svg"/g) ?? []).length === 1 &&
    (app.match(/persistentSceneSvg model projection presentationAlpha dispatch/g) ?? []).length === 1 &&
    (app.match(/tacticalWorkscreenRegion model dispatch/g) ?? []).length === 2 &&
    (app.match(/tacticalShell model dispatch transientContent/g) ?? []).length === 2,
  "source does not have exactly one renderer definition, SVG root, renderer call, region definition/call, and shell call",
);
for (const required of [
  'prop.id "tactical-workscreen-region"',
  'svg.custom ("data-work-surface-root", "persistent-svg")',
  'prop.className "tactical-compact-toolbar"',
  'prop.className "tactical-supporting-controls"',
  'else "panel editor-tools editor-tools-panel"',
]) {
  require(app.includes(required), `accepted single-shell token is missing: ${required}`);
}

for (const required of [
  "var(--tactical-left-width, 208px)",
  "minmax(32rem, 1fr)",
  "var(--tactical-right-width, 224px)",
  "min-height: 32rem",
  "height: var(--tactical-bottom-height, 152px)",
  "@media (max-width: 48rem)",
]) {
  require(styles.includes(required), `Field Focus/responsive CSS token is missing: ${required}`);
}
for (const required of [
  "Field Focus defaults do not keep the workscreen dimensionally dominant with both sidebars open.",
  "defaultWorkscreenWidth / referenceContentWidth < 0.68",
  'window.happyDOM.setInnerWidth(320)',
  "Layout/supporting-panel/400% operations changed Plan authority",
  "for (const source of Object.keys(ownerByModality))",
  "assertCamera(operation)",
  "assertSelection(operation, expectedSelection)",
  'window.document.querySelector("#persistent-tactical-svg") !== worksurface',
  "window.document.querySelectorAll(\"svg[role='application']\").length !== 1",
  "planAuthorityBeforeLayout",
]) {
  require(smoke.includes(required), `workscreen/Field Focus/spatial/state/responsive evidence is missing: ${required}`);
}
for (const required of [
  "Context help did not receive focus",
  "The command-binding modal did not establish modal focus.",
  "Closing the command-binding modal did not restore its invoking focus.",
  "Hiding a focused supporting panel did not restore focus to its toggle.",
  "Keyboard timeline resize did not persist once and restore separator focus.",
]) {
  require(smoke.includes(required), `focus evidence is missing: ${required}`);
}
for (const required of [
  "Bottom-panel resizing did not clamp or persist deterministically.",
  "Panel show/hide, collapse, move, order, drawer, timeline, or reset diverged.",
  "Version-zero layout did not migrate deterministically.",
  "Future, unknown, invalid, duplicate, trailing-comma, malformed, or truncated layout input was accepted.",
]) {
  require(layoutTests.includes(required), `panel configuration evidence is missing: ${required}`);
}
for (const required of [
  "Perspective review exposed an entity or retained an invisible selection.",
  "Review interpolation crossed disclosure/semantic-owner guards or leaked a previous-frame entity.",
  "Simulator projection mutated its handoff or crossed its revision boundary.",
  "Planning projection mutated input or disclosed editor/runtime-only unit state.",
  "Planning projection shared mutable route geometry with authority state.",
]) {
  require(projectionTests.toLowerCase().includes(required.toLowerCase()), `projection authority/disclosure evidence is missing: ${required}`);
}
for (const required of [
  "Shared SVG keyboard intent bypassed registry availability.",
  "data-command-available",
  "tacticalCommandAvailable",
  "Native Rules panel input leaked into tactical shortcuts.",
]) {
  require(smoke.includes(required) || app.includes(required), `command-authority evidence is missing: ${required}`);
}
for (const required of [
  "Pointer timeline resize was not coalesced or changed unified timeline authority",
  "projection-only-runtime-tick-unchanged",
  'ariaLabel "Authored, predicted, accepted, and committed timeline segments"',
]) {
  require(smoke.includes(required) || app.includes(required), `timeline authority evidence is missing: ${required}`);
}
require(
  accessibility.includes("one titled/described application workscreen") &&
    accessibility.includes("no legacy Editor or migration UI"),
  "documentation accessibility no longer fails closed on singleton/removal semantics",
);

require(reviewManifest.schema === "sir-persistent-workspace-m9-live-review-v2", "review manifest schema drifted");
require(reviewManifest.captureKind === "actual-production-shell-chromium-screenshot", "review is not an actual full production shell capture");
require(reviewManifest.productionBundleSha256 === hash(bundleBytes), "review is not bound to the production bundle");
require(reviewManifest.productionStylesSha256 === hash(builtStylesBytes), "review is not bound to production CSS");
require(reviewManifest.acceptedMockupSha256 === hash(mockup), "review is not bound to the accepted mockup");
require(
  typeof reviewManifest.chromiumExecutable === "string" && reviewManifest.chromiumExecutable.length > 0 &&
    typeof reviewManifest.chromiumUserAgent === "string" && reviewManifest.chromiumUserAgent.includes("Chrome/") &&
    typeof liveBrowserAudit.chromiumExecutable === "string" && liveBrowserAudit.chromiumExecutable.length > 0 &&
    liveBrowserAudit.chromium.includes("Chrome/"),
  "review/live audit does not identify a real Chromium executable and user agent",
);
require(reviewManifest.geometrySvgSha256 === hash(reviewGeometrySvg) && reviewManifest.pngSha256 === hash(reviewPng), "review asset hashes drifted");
require(reviewPng.readUInt32BE(16) === 1440 && reviewPng.readUInt32BE(20) === 900, "actual Chromium screenshot is not 1440×900");
require(
  JSON.stringify(reviewManifest.fieldFocus) === JSON.stringify(liveBrowserAudit.wide) &&
    JSON.stringify(reviewManifest.narrow400PercentEquivalent) === JSON.stringify(liveBrowserAudit.narrow),
  "stored review metrics do not exactly match a fresh real-Chromium audit",
);
require(
  reviewManifest.fieldFocus.rectangles.left.width === 208 &&
    reviewManifest.fieldFocus.rectangles.right.width === 224 &&
    reviewManifest.fieldFocus.rectangles.bottom.height === 152 &&
    reviewManifest.fieldFocus.fieldFocusShare >= 0.68 &&
    reviewManifest.fieldFocus.rectangles.workscreen.width > 208 + 224 &&
    reviewManifest.fieldFocus.rectangles.workscreen.height > 3 * 152 &&
    reviewManifest.fieldFocus.counts.worksurfaceRoots === 1 &&
    reviewManifest.fieldFocus.counts.applicationLandmarks === 1 &&
    reviewManifest.fieldFocus.styles.toolsPosition !== "absolute" &&
    reviewManifest.narrow400PercentEquivalent.document.scrollWidth === 320,
  "Field Focus review does not prove dominant workscreen dimensions and singleton ownership",
);

require(report.includes("status: implemented") && report.includes("decision-status: implemented"), "design report is not implemented");
require((report.match(/### Milestone 9[\s\S]*?(?=## Acceptance evidence)/)?.[0].match(/- \[x\]/g) ?? []).length === 5, "not every M9 report box is checked");
require(report.includes("Persistent workspace M9 acceptance evidence"), "report does not link M9 evidence");
require(workspace.includes("M9 acceptance evidence"), "living workspace documentation omits final acceptance");
require(roadmap.includes("Corrective acceptance is complete"), "living roadmap is not closed");

for (const qualification of [
  "scripts/test-persistent-workspace-m0-baseline.mjs",
  "scripts/test-map-editor-qualification.mjs",
  "scripts/test-planning-workspace-m5-qualification.mjs",
  "scripts/test-simulator-workspace-m6-qualification.mjs",
  "scripts/test-review-workspace-m7-qualification.mjs",
  "scripts/test-timeline-supporting-panels-m8-qualification.mjs",
]) {
  execFileSync(process.execPath, [qualification], { stdio: "inherit" });
}

console.log(
  "Persistent workspace M9 acceptance passed: source, production JavaScript/CSS, DOM, accessibility, projection/layout tests, prior migration gates, and hash-bound Field Focus review prove one retained SVG renderer/workscreen, dominant default geometry with both sidebars, spatial and state continuity, disclosure, deterministic panels, focus restoration, timeline authority, 400% access, shared command availability, and complete legacy removal.",
);
