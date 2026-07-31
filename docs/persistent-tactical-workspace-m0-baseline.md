---
title: Persistent Tactical Workspace Corrective Baseline
category: Engineering
categoryindex: 6
index: 12
status: accepted
decision-status: characterization
document-type: test-baseline
version: "1.0"
last-updated: 2026-07-31
description: Milestone 0 inventory and executable known-failure baseline for the corrective persistent SVG workscreen migration.
related:
  - docs/2026-07-31-0840-vscode-style-persistent-tactical-workspace-design-report.md
  - docs/unified-tactical-workspace-roadmap.md
---

# Persistent Tactical Workspace Corrective Baseline

This is the migration ledger for the presentation that exists before the
persistent-SVG correction. It inventories capability, not desired placement.
Later milestones may move or consolidate an entry only after preserving its
command authority, focus behavior, camera/selection semantics, and responsive
access.

The common shell currently retains
`#unified-tactical-workspace`, `#tactical-battlefield-viewport`, the modality
strip, shared timeline, contextual action surface, and binding dialog. The
viewport is only a wrapper. Its first battlefield child changes on every
modality render branch.

## Render-path and battlefield-element inventory

| Modality | Current root below the retained viewport | Actual battlefield | Elements/layers that must migrate |
|---|---|---|---|
| Editor | `section.editor-canvas[aria-label="SVG tactical map workspace"]` | `svg#editor-map-stage.editor-battlefield-svg[role=application]` | local raster background, terrain, terrain preview, grid, edges/doors/windows, edge preview, regions and handles, units/glyphs, unit preview, effects, overlays, keyboard cursor, selection/box selection, validation overlay, gesture/placement/movement previews, guides, context HUD |
| Plan | `section.panel.planning-battlefield[aria-label="Battlefield route authoring"]` | `.planning-cell-grid` of native cell buttons; there is no SVG | roster occupants, selected unit, route-waypoint activation, current planning tool and authored cell coordinates |
| Simulate | `section.panel.battlefield-panel[aria-label="Editable simulation SVG battlefield"]` | `svg.battlefield-svg[role=application]` | heading/tick status; pan/zoom/reset controls; terrain override, grid, edges, unit glyphs/health/faction, route preview, overlays, combat-effect/action traces; palette; semantic zoom; exact-tick and reduced-motion switches; semantic replay lanes; overlay-budget notices; legend; unit-inspector sidecar; camera transform and disclosure |
| Review | `section.panel.battlefield-panel[aria-label="Loaded replay SVG battlefield"]` (static demonstration when unloaded) | a newly reconciled `svg.battlefield-svg[role=application]` | heading/tick/interpolation status; pan/zoom/reset controls; disclosed terrain/grid/edges/units, selected-unit overlays, combat-effect/action traces; palette; exact/interpolated frame state; exact-tick and reduced-motion switches; semantic replay lanes; overlay-budget notices; legend; unit-inspector sidecar; perspective/disclosure, semantic zoom and camera transform |

The child replacement is structural, not just changed content: Editor supplies
`editorBattlefield`, Plan supplies `planningBattlefield`, and Simulate/Review
invoke `battlefieldView`. Plan replaces an SVG with a button grid. Simulate and
Review call the same renderer function but React receives it through separate
modality branches; neither preserves the Editor SVG object.

## Panel and landmark inventory

Shared landmarks are the tactical modality navigation, persistent wrapper,
`.tactical-workspace-content`, unified tactical timeline and transport,
current-action status/help, binding dialog, and supporting-section navigation.
Rules/Data and Samples still replace the tactical shell and are explicitly
outside this milestone.

| Modality | Current panels and landmarks |
|---|---|
| Editor | map-editor document strip; File/Edit/View/Help menus; quick toolbar; tool ribbon; Terrain, Units, Edges, Zones, and Document tool tabs and their contextual controls; layers; saved views; document/revision/dirty/autosave status; validation issues; interchange review/confirmation overlays; selected-object context HUD; map inspector; object list; status/input strip; `editor-map-stage` |
| Plan | authored/predicted/accepted/committed status header and worker status; planning tools; roster; route-authoring battlefield; inspector; per-unit timeline lanes; validation navigation; deterministic plan artifact/details; export-review action |
| Simulate | simulator document/menu/toolbar; Controllers, Events, and Samples tabs; controller/movement command panel; immutable revision/staleness status; simulator map stage; simulation battlefield; battlefield sidecar/legend/unit inspector; status/input strip |
| Review | source/file-load panel; replay play/pause, previous/next event, back/step and cancel controls; direct range seek; checkpoint seek buttons; worker progress; direct playback-speed selector; replay battlefield heading/camera controls; palette, exact-tick/reduced-motion, semantic lanes, overlay notices, combat legend, battlefield legend and unit-inspector sidecar; compact Replay inspector board, event list, attack formula, checkpoint hashes and perspective hash; worker/verification/source identity |

Stable panel/landmark strings are qualified by
`scripts/test-persistent-workspace-m0-baseline.mjs`; existing browser smoke
qualifies the interactive representatives. This ledger deliberately retains
both the shared tactical timeline and Plan's duplicate per-unit timeline
because dropping either before the Milestone 8 convergence would lose current
capability.

## Command and focus inventory

The common registry currently owns these exact stable commands:

- modality: `workspace.editor`, `workspace.plan`, `workspace.simulate`,
  `workspace.review`;
- timeline: `timeline.play-toggle`, `timeline.step-back`,
  `timeline.step-forward`, `timeline.home`, `timeline.end`,
  `timeline.move-command`, `timeline.remove-command`;
- planning: `planning.undo`, `planning.redo`, `planning.route`,
  `planning.facing`, `planning.attention`, `planning.stance`,
  `planning.hold`, `planning.engagement`, `planning.synchronization`,
  `planning.validate`, `planning.preview`, `planning.commit`,
  `planning.issue.previous`, `planning.issue.next`;
- review: `review.previous-event`, `review.next-event`, `review.cancel`; and
- help: `input.help`, `input.help.close`, `input.bindings`.

The contextual registry additionally creates one command for every Plan roster
unit (`planning.roster.select.<unit>`), authored timeline command
(`planning.timeline.select.<id>`), validation issue
(`planning.issue.focus.<index>`), battlefield cell
(`planning.battlefield.cell.<column>.<row>`), tool-specific inspector choice
(`planning.inspector.waypoint|facing|attention|stance|hold|engagement|synchronization...`),
and Simulator controller/script/movement button
(`simulator.pointer.controller.*`, `simulator.pointer.script.set`,
`simulator.pointer.movement.*`).

The modal catalog is also command authority and therefore part of the
inventory. Its exhaustive stable namespaces are:

- Editor camera (`fit`, `frame-selection`, held/released/cancelled and
  small/large cardinal pan, reset), cursor movement/object cycling, history,
  destructive confirmation, gesture cancel/commit, help, panel and inspector;
- Editor document (`new`, `clear`, `import`, `export`, `bundle`, `background`,
  `resize`, `layers`, `views`, `exit`) and domain selection (terrain, units,
  edges, zones, document);
- Editor selection (single/toggle/all/domain/clear, box begin/add/cardinal
  extension, copy/paste/duplicate/delete/move/inspector actions);
- Editor terrain (activate, value choice, cursor/gesture paint, brush size,
  exit/reset) and pencil/erase/eyedropper/flood-fill/line/rectangle tools;
- Editor units (preset browsing/search/faction navigation, placement
  move/commit/cancel, selected-unit movement, and validation navigation);
- Editor edges (kind, activation, cardinal cursor, polyline, erase/join/split,
  door toggle and orientation);
- Editor regions (selection, rectangle/polygon creation, purpose, move,
  resize, vertex editing, commit/cancel/reset/backtrack/delete); and
- Simulator help, tool-panel/tabs, unit cycling, step, run/pause, reset,
  route-preview cardinal/fast movement/commit/cancel/reset, and controller
  begin/select/commit/cancel.

The literal modal IDs currently emitted directly by `binding` are
`editor.camera.pan-cancel`, `editor.camera.pan-held`,
`editor.camera.pan-release`, `editor.confirmation.cancel`,
`editor.confirmation.confirm`, `editor.cursor.next-object`,
`editor.cursor.previous-object`, `editor.edge.polyline.backtrack`,
`editor.gesture.cancel`, `editor.gesture.commit`, `editor.help.close`,
`editor.selection.actions.copy`, `editor.selection.actions.delete`,
`editor.selection.actions.duplicate`, `editor.selection.actions.inspector`,
`editor.selection.actions.move`, `editor.selection.all-domain`,
`editor.selection.box.add`, `editor.selection.box.begin`,
`editor.selection.clear`, `editor.selection.single`,
`editor.selection.toggle`, `editor.terrain.activate`,
`editor.terrain.brush.decrease`, `editor.terrain.brush.increase`,
`editor.terrain.exit`, `editor.terrain.gesture.reset`,
`editor.unit.move.begin`, `editor.unit.move.reset`,
`editor.unit.place.commit`, `editor.unit.place.commit-return`,
`editor.unit.place.next-preset`, `editor.unit.place.previous-preset`,
`editor.unit.preset.arm`, `editor.unit.preset.exit`,
`editor.unit.preset.first`, `editor.unit.preset.last`,
`editor.unit.preset.next-faction`, `editor.unit.preset.previous-faction`,
`editor.unit.preset.search`, `simulator.controller.cancel`,
`simulator.controller.commit`, `simulator.controller.general`,
`simulator.controller.manual`, `simulator.controller.scripted`,
`simulator.help.toggle`, `simulator.preview.cancel`,
`simulator.preview.commit`, and `simulator.preview.reset`.

Generated contextual namespaces are exactly
`planning.roster.select.`, `planning.timeline.select.`,
`planning.issue.focus.`, `planning.battlefield.cell.`,
`planning.inspector.waypoint.`, `planning.inspector.facing.`,
`planning.inspector.attention.`, `planning.inspector.stance.`,
`planning.inspector.hold`, `planning.inspector.engagement`,
`planning.inspector.synchronization`, `simulator.pointer.controller.`,
`simulator.pointer.script.set`, and `simulator.pointer.movement.`.

Not every control dispatches a registry command. The direct-dispatch inventory
that must also survive migration is:

- Editor SVG pointer/wheel/pointer-capture and keyboard events, direct toolbar,
  menu, file/import/export, dialog, inspector, object-list, layer, saved-view
  and map-size actions dispatch `EditorChanged`, `EditorWorkspaceChanged`,
  file/interchange messages, or simulator handoff;
- Plan cell, roster, issue, inspector and lane controls use generated registry
  IDs, while artifact export dispatches `ExportPlanningReview`;
- Simulate battlefield camera/palette/exact-tick/reduced-motion and SVG unit
  activation dispatch `BattlefieldChanged`; desktop/panel/controller controls
  dispatch simulator messages and modal commands; and
- Review file input dispatches `FileSelected`; its range and checkpoint
  controls dispatch `SeekRequested` directly; speed dispatches `SpeedChanged`;
  compact inspector units, events and formula dispatch `UnitSelected`,
  `EventSelected` and `FormulaSelected`; battlefield camera, palette,
  exact-tick, reduced-motion and unit activation dispatch
  `BattlefieldChanged`. Play/event/step/cancel controls use registry IDs.

`ModalInput.isKnownCommandId` is the strict exhaustive 252-ID persisted
allowlist; the source-backed inventory fixture pins its sorted SHA-256 as well
as its count. `editorCatalog` and `simulatorCatalog` are the
availability/gesture inventory. The later command migration must compare
against those live catalogs, not against duplicated prose.

Current focus targets are:

- Editor SVG (`tabindex=0`), focusable unit/region/object descendants, native
  toolbar/ribbon/menu/inspector/object-list controls, confirmation and
  interchange dialogs;
- every Plan grid cell, roster item, tool, inspector action, timeline command,
  validation issue and transport control;
- Simulator battlefield units and all native menu, panel, controller,
  movement, sidecar and transport controls;
- Review battlefield units, source inputs, transport/event navigation,
  cancellation and inspector controls;
- shared modality buttons, numeric/range time inputs,
  `#tactical-input-toggle`, `#tactical-input-panel` (`tabindex=-1`), and the
  native binding dialog inputs/buttons.

Closing contextual help restores focus to `#tactical-input-toggle`; opening it
can focus `#tactical-input-panel`. Workspace changes close help, clear held
input, cancel Editor pointer capture, and clear transient Simulator controller
selection. Any future panel hiding/moving must preserve or deliberately restore
these focus destinations.

## Camera and selection paths

There are currently separate spatial paths:

| Modality | Camera | Selection and activation |
|---|---|---|
| Editor | `EditorView.Camera` (`PanX`, `PanY`, `Zoom`), viewport width/height, reduced-motion flag, captured pointers; wheel zoom-at-pointer, middle/right/touch pan, keyboard/buttons, fit/frame/reset and saved views | `MapEditorState.SelectedUnit`, `SelectedUnits`, `SelectedRegion`, `KeyboardCursor` and `KeyboardObject`; SVG pointer hit testing through `tryHitCell`/`tryHitEdge`, focusable objects, object list, inspector and modal commands |
| Plan | no camera; the scroll position of `.planning-cell-grid` is incidental DOM state | `PlanningWorkspaceState.SelectedUnit`, `SelectedCommand`, and independent `FocusedIssue`; roster and command-lane activation set the first two, while validation previous/next/direct issue navigation updates `FocusedIssue` and may reconcile unit/command selection |
| Simulate | shared `BattlefieldViewState.Camera` (`PanX`, `PanY`, `Zoom`) and semantic-zoom/exact-tick/reduced-motion presentation flags | `SimulatorSelectedUnit` drives the runtime frame; independently, `BattlefieldViewState.SelectedUnit` drives selected styling/overlays and the battlefield inspector, while `BattlefieldViewState.FocusedUnit` owns roving `tabindex` keyboard focus. SVG focus dispatches `FocusUnit`; arrow keys dispatch `FocusDirection`; click/Enter dispatch `SelectUnit`. Controller-panel and modal unit-cycle state is separate again |
| Review | `BattlefieldViewState.Camera` and presentation interpolation state (`PreviousFrame`, `PresentationAlpha`) | three current paths: `BattlefieldViewState.SelectedUnit` drives selected styling/overlays and battlefield inspector; independent `BattlefieldViewState.FocusedUnit` owns roving SVG keyboard focus via `FocusUnit`/`FocusDirection`; replay `model.Selection.Unit`, `model.Selection.Event`, and `model.Selection.Formula` drive the compact inspector board/event/formula controls. Direct replay seek/checkpoint/speed controls alter playback projection, not selection |

`WorkspaceChanged` retains the Editor camera record but cancels captured
pointers outside Editor. It retains `BattlefieldViewState`, including its
separate selected and focused unit IDs, across Simulate/Review; initializes or
retains Planning unit/command/issue focus by map revision; and does not
normalize the Editor, Planning, Simulator, battlefield-selected,
battlefield-focused, or replay-inspector stores. `Battlefield.reconcile` keeps
selected and focused IDs independently only while the projected unit remains.
Thus the present code preserves some state records while replacing their
rendered owners; it does not yet provide one cross-modality camera, selection,
or keyboard-focus path.

## Responsive baseline

The existing breakpoint is `@media (max-width: 48rem)`, the proxy used by the
current 400% reflow qualification. At and below it:

- modality controls become sticky; timeline and input status remain in flow;
- transport labels become full-width and the ruler can shrink;
- binding rows collapse to one column and the dialog uses a small fixed inset;
- Simulate/Review battlefield headings become block layout, camera controls
  left-align with bottom spacing, battlefield/sidecar collapse to one column,
  and the compact Replay inspector loses any multi-column span;
- Editor becomes one column while ribbon and inspector become positioned
  overlays; the document strip wraps with identity on its own row; the menu bar
  scrolls horizontally; ribbon-open inspector position/width changes; the SVG
  retains a 22rem minimum height; map-size controls become two columns; section
  headings become block layout; and object list follows the map;
- Plan changes from three columns and named grid areas to the sequence status,
  tools, roster, battlefield, inspector, timeline, validation, artifact;
  status and lanes become one column and roster/grid heights are bounded; and
- contextual action help becomes in-flow and single-column.

Forced-colors qualification covers native controls, Editor layers, planning
panels, timeline and binding surfaces. Reduced-motion globally suppresses
animation/transition duration. Current pointer targets for tactical,
planning, Editor and Simulator controls are at least 2.75rem where explicitly
qualified.

These landmarks are a capability floor, not the target layout. Milestone 1 may
replace them with compact sidebars/drawers only when all listed controls remain
reachable and the workscreen remains mounted.

## Known-failure browser characterization

`npm run characterize:persistent-workscreen` builds the production client,
retains the outer viewport and exact initial work-surface root
`svg#editor-map-stage`, proves that SVG is non-null, connected, and contained by
the viewport, activates Plan, proves the outer object is strictly equal, then
proves the exact `.planning-cell-grid` root is non-null, connected, contained
by that same viewport, and unequal to the initial SVG.

On this baseline it intentionally exits non-zero with:

```text
KNOWN M0 FAILURE: #tactical-battlefield-viewport survived Editor → Plan, but
its actual work-surface root was replaced (svg#editor-map-stage →
.planning-cell-grid).
```

The normal production smoke remains green because the stronger assertion is
enabled only by `SIR_CHARACTERIZE_PERSISTENT_WORKSCREEN=1`. This is executable
evidence of the known flaw, not a permanently broken accepted gate. Milestone
3 must replace this expected failure with a strict reference-equality pass
against `#tactical-workscreen[aria-label="Tactical workscreen"]`.

## Milestone 0 review evidence

- focused source/inventory gate:
  `npm run test:persistent-workspace-m0` — passed;
- existing browser regression:
  `npm run build:client && node scripts/smoke-client.mjs` — passed;
- expected known failure:
  `SIR_CHARACTERIZE_PERSISTENT_WORKSCREEN=1 node scripts/smoke-client.mjs` —
  exited 1 with the expected `KNOWN M0 FAILURE`, after the outer-identity
  assertion passed;
- sequential product gates: `./fake.sh build -t Dev`, then `Test`, then
  `Verify` — all passed.

Milestone 0 changes characterization, tests, and documentation only. It does
not claim to implement the persistent SVG.
