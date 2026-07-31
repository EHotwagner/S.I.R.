import { readFile } from "node:fs/promises";
import { createHash } from "node:crypto";

const [
  app,
  styles,
  smoke,
  modalInput,
  tacticalWorkspace,
  battlefield,
  planningWorkspace,
  baseline,
  roadmap,
  packageJson,
  inventoryText,
] =
  await Promise.all([
    readFile("src/SIR.Client.Web/App.fs", "utf8"),
    readFile("src/SIR.Client.Web/styles.css", "utf8"),
    readFile("scripts/smoke-client.mjs", "utf8"),
    readFile("src/SIR.Client/ModalInput.fs", "utf8"),
    readFile("src/SIR.Client/UnifiedTacticalWorkspace.fs", "utf8"),
    readFile("src/SIR.Client/Battlefield.fs", "utf8"),
    readFile("src/SIR.Client/PlanningWorkspace.fs", "utf8"),
    readFile("docs/persistent-tactical-workspace-m0-baseline.md", "utf8"),
    readFile("docs/unified-tactical-workspace-roadmap.md", "utf8"),
    readFile("package.json", "utf8"),
    readFile(
      "tests/fixtures/persistent-workspace-m0-inventory.json",
      "utf8",
    ),
  ]);
const inventory = JSON.parse(inventoryText);

const requireText = (source, token, message) => {
  if (!source.includes(token)) throw new Error(message);
};

const sortedUnique = (values) => [...new Set(values)].sort();
const extracted = (source, expression) =>
  sortedUnique([...source.matchAll(expression)].map((match) => match[1]));
const requireExact = (actual, expected, message) => {
  if (JSON.stringify(actual) !== JSON.stringify(sortedUnique(expected))) {
    throw new Error(
      `${message}\nexpected=${sortedUnique(expected).join(",")}\nactual=${actual.join(",")}`,
    );
  }
};

requireExact(
  extracted(tacticalWorkspace, /\bcommand\s+"([^"]+)"/g),
  inventory.staticRegistryCommands,
  "M0 static tactical command fixture drifted.",
);
requireExact(
  extracted(modalInput, /\bbinding\s+"([^"]+)"/gs),
  inventory.literalModalCommands,
  "M0 literal modal command fixture drifted.",
);
const knownCommandSource = modalInput.slice(
  modalInput.indexOf("let isKnownCommandId"),
  modalInput.indexOf("let private key"),
);
const knownModalCommands = [];
for (const family of knownCommandSource.matchAll(
  /suffixIn\s+"([^"]+)"\s+\[([\s\S]*?)\]/g,
)) {
  for (const suffix of family[2].matchAll(/"([^"]+)"/g)) {
    knownModalCommands.push(family[1] + suffix[1]);
  }
}
for (const single of knownCommandSource.matchAll(/\bid\s*=\s*"([^"]+)"/g)) {
  knownModalCommands.push(single[1]);
}
const exactKnownModalCommands = sortedUnique(knownModalCommands);
const knownModalCommandsSha256 = createHash("sha256")
  .update(exactKnownModalCommands.join("\n"))
  .digest("hex");
if (
  exactKnownModalCommands.length !== inventory.knownModalCommandsCount ||
  knownModalCommandsSha256 !== inventory.knownModalCommandsSha256
) {
  throw new Error(
    `M0 exhaustive modal allowlist fixture drifted: count=${exactKnownModalCommands.length}, sha256=${knownModalCommandsSha256}.`,
  );
}

for (const command of [
  ...inventory.staticRegistryCommands,
  ...inventory.literalModalCommands,
  ...inventory.dynamicCommandTemplates,
]) {
  requireText(
    baseline,
    `\`${command}\``,
    `M0 human command inventory omitted exact ID/template: ${command}`,
  );
}

for (const token of inventory.renderAndControlTokens) {
  requireText(
    app,
    token,
    `M0 render/control/selection fixture lost App source token: ${token}`,
  );
}
for (const token of inventory.battlefieldStateTokens) {
  requireText(
    battlefield,
    token,
    `M0 battlefield camera/selection fixture lost source token: ${token}`,
  );
}
for (const token of inventory.planningStateTokens) {
  requireText(
    planningWorkspace,
    token,
    `M0 planning selection/focus fixture lost source token: ${token}`,
  );
}
for (const token of [
  "scene.FocusedUnit = Some unit.Id",
  "BattlefieldChanged(FocusUnit(Some unit.Id))",
  "BattlefieldChanged(FocusDirection(-1, 0))",
  "BattlefieldChanged(FocusDirection(1, 0))",
  "BattlefieldChanged(FocusDirection(0, -1))",
  "BattlefieldChanged(FocusDirection(0, 1))",
  "state.FocusedIssue = Some index",
]) {
  requireText(
    app,
    token,
    `M0 rendered focus-path fixture lost App source token: ${token}`,
  );
}

for (const token of [
  'prop.id "tactical-battlefield-viewport"',
  "| EditorWorkspace ->",
  'svg.id "persistent-tactical-svg"',
  'svg.id "persistent-editor-migrated-layers"',
  "| PlanningWorkspace ->",
  'svg.id "persistent-layer-routes"',
  'svg.id "persistent-layer-annotations"',
  "| SimulatorWorkspace ->",
  "battlefieldView",
  "| ReplayWorkspace ->",
  "EditorView: EditorWorkspaceState",
  "Battlefield: BattlefieldViewState",
  "SimulatorSelectedUnit: int32 option",
  "Planning: PlanningWorkspaceState option",
  "CancelEditorPointers",
  '("planning.roster.select."',
  '("planning.timeline.select."',
  '("planning.issue.focus."',
  '("planning.battlefield.cell."',
  '"planning.inspector." + channel',
  '("simulator.pointer.controller."',
  '"simulator.pointer.script.set"',
  '("simulator.pointer.movement."',
  "editorToolbar",
  "editorUnitPanel",
  'prop.ariaLabel (if activePanel = DocumentTools then "Map document controls" else "Map editing tools")',
  'prop.className "tactical-layout-panel-body"',
  "simulatorPanelBody",
  'prop.ariaLabel "Simulator runtime tools"',
  'prop.ariaLabel "Simulator runtime diagnostics"',
  'prop.ariaLabel "Simulator immutable revision state"',
  "planningPanelBody",
  "sourcePanel shell dispatch",
  "controls shell dispatch",
  "inspector shell dispatch",
  '"scene.camera.zoom-out"',
  '"scene.camera.zoom-in"',
  '"scene.camera.fit"',
  '"editor.scene.create-simulator-handoff"',
]) {
  requireText(app, token, `M0 render/dynamic-path inventory lost source token: ${token}`);
}

requireText(
  styles,
  "@media (max-width: 48rem)",
  "M0 responsive breakpoint disappeared.",
);
const responsive = styles.slice(styles.indexOf("@media (max-width: 48rem)"));
for (const token of inventory.responsiveTokens) {
  requireText(styles, token, `M0 responsive landmark inventory lost CSS token: ${token}`);
  requireText(
    responsive,
    token,
    `M0 responsive token moved outside the 48rem qualification: ${token}`,
  );
}

for (const heading of [
  "## Render-path and battlefield-element inventory",
  "## Panel and landmark inventory",
  "## Command and focus inventory",
  "## Camera and selection paths",
  "## Responsive baseline",
  "## Known-failure browser characterization",
]) {
  requireText(baseline, heading, `M0 baseline lost required section: ${heading}`);
}

requireText(
  roadmap,
  "Superseded persistence claim",
  "Earlier roadmap does not mark wrapper persistence as superseded.",
);
requireText(
  packageJson,
  '"characterize:persistent-workscreen"',
  "Persistent-workscreen characterization command is not exposed.",
);
requireText(
  smoke,
  'querySelector("#persistent-tactical-svg")',
  "Milestone 3 persistent SVG characterization is missing.",
);
requireText(
  smoke,
  "assertSingleWorksurface",
  "Milestone 3 browser characterization lost strict reference equality.",
);
for (const token of [
  'data-work-surface-root="persistent-svg"',
  "window.document.querySelectorAll(\"[data-work-surface-root]\").length !== 1",
  "window.document.querySelectorAll(\"[role='application']\").length !== 1",
  "legacyRootSelectors",
  "for (const source of Object.keys(ownerByModality))",
  "data-semantic-selection-unit",
  "Shared SVG keyboard intent",
]) {
  requireText(
    smoke,
    token,
    `Persistent SVG characterization lost exact-root evidence: ${token}`,
  );
}

console.log(
  `Persistent tactical workspace Milestone 0 inventory passed: ${inventory.staticRegistryCommands.length} static registry commands, ${inventory.knownModalCommandsCount} exhaustive persisted modal IDs (${inventory.literalModalCommands.length} literal bindings), ${inventory.dynamicCommandTemplates.length} contextual namespaces, ${inventory.renderAndControlTokens.length} render/control tokens, ${inventory.battlefieldStateTokens.length + inventory.planningStateTokens.length} selection/focus state tokens, and ${inventory.responsiveTokens.length} responsive tokens.`,
);
