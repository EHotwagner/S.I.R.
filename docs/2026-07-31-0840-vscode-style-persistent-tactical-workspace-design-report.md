---
title: VS Code-Style Persistent Tactical Workspace Design Report
category: Tools & Evidence
categoryindex: 5
index: 12
status: proposed
decision-status: accepted
document-type: timestamped-design-report
version: "1.1"
created-at: 2026-07-31T08:40:09+02:00
last-updated: 2026-07-31
description: Corrective design and delivery roadmap for one persistent SVG workscreen with compact chrome, configurable sidebars, and modality-specific projections.
related:
  - docs/unified-tactical-workspace-roadmap.md
  - docs/unified-tactical-workspace-baseline.md
  - docs/unified-tactical-workspace.md
  - docs/map-editor.md
  - docs/planning-workspace.md
  - docs/svg-replay-player.md
---

# VS Code-Style Persistent Tactical Workspace Design Report

**Report timestamp:** 2026-07-31 08:40:09 CEST (UTC+02:00)

**Status:** design accepted; Milestones 0–5 complete

## Executive decision

S.I.R. will replace the present modality-specific battlefield and workspace
render paths with a VS Code-style application shell built around one persistent
SVG workscreen. Editor, Plan, Simulate, and Review remain modalities, but they
will no longer select different battlefield components or page-sized content
trees. A modality change will update the projection, visible layers, available
commands, inspectors, and editing rules inside the same mounted SVG and shell.

The application chrome will consist of:

- one compact toolbar at the top;
- configurable left and right sidebars whose panels can be moved, reordered,
  collapsed, shown, or hidden;
- one persistent SVG workscreen between the sidebars;
- one resizable, collapsible timeline panel below the workscreen; and
- one compact contextual status and input surface.

Review is a modality of this same workscreen. Rules, Data, and Samples become
configurable sidebar panels or temporary overlay panels and do not replace the
workscreen. After feature parity is proven, the legacy Editor, Planner,
Simulator, and Review render paths will be removed.

The accepted visual and density target is **Field Focus**: the battlefield
receives the maximum practical area, supporting sidebars are narrow by default,
the toolbar remains compact, and the shared timeline opens at a shallow useful
height. Wider sidebars and a deeper timeline remain user-configurable analysis
arrangements; they are not separate workspaces or modality-specific layouts.
Implementation is not accepted until it achieves the Field Focus target rather
than merely reproducing the current interface inside new shell markup.

This decision corrects the presentation boundary accepted in the first unified
tactical workspace implementation. The current code preserves
`#tactical-battlefield-viewport`, but conditionally replaces its battlefield
child and surrounds it with modality-specific workspace trees. That proves a
persistent wrapper, not a persistent workscreen.

## Context and problem

The accepted unified-workspace roadmap correctly separated authoritative
ownership:

- the editor owns the mutable authored map and immutable map revisions;
- the planner owns authored plan revisions and validation state;
- the simulator owns disposable runtime copied from an immutable revision;
- replay packages own committed history; and
- the tactical projection owns modality, time cursor, and disclosed timeline
  channels without becoming a second authority.

Those boundaries remain sound. The rendering implementation does not yet
express them as one spatial workspace. `App.fs` currently branches from
`WorkspaceMode` into separate Editor, Planning, Simulator, and Replay content.
Within the retained battlefield wrapper it also selects among
`editorBattlefield`, `planningBattlefield`, and `battlefieldView`. Existing
browser qualification checks the wrapper node by reference, so a test can pass
while the actual SVG work surface is replaced.

The consequences are visible:

- modality controls still behave like page tabs;
- tools and inspectors move between unrelated layouts;
- renderer-specific coordinate, selection, and pointer paths remain;
- modality changes can lose spatial and focus continuity below the wrapper;
- duplicated rendering paths can drift; and
- the user must re-parse the interface when changing the purpose of the same
  battlefield.

The target is not one merged domain model. It is one stable presentation
surface over deliberately separate domain owners.

## Binding decisions

### Field Focus is the canonical goal

The [interactive Field Focus mockup](assets/persistent-workspace-mockups/index.html)
is the selected baseline for information density and spatial priority. It is a
design target, not a pixel-exact specification, but the following outcomes are
mandatory:

- the workscreen is visually and dimensionally dominant at the reference
  desktop viewport;
- both sidebars can remain open without reducing the workscreen to a secondary
  pane;
- default sidebars are narrow and show compact, task-relevant information;
- the top toolbar fits document, modality, essential transport, view, and
  command access without becoming a ribbon;
- the default timeline is shallow enough to preserve battlefield height while
  keeping its cursor and primary lanes readable;
- expanding panels or the timeline produces the Operations Analysis
  arrangement through the same persisted layout system;
- hiding supporting panels produces a more canvas-focused arrangement without
  entering another workspace; and
- changing density, panel width, timeline height, or modality never remounts
  the SVG.

The implementation may refine type, spacing, icons, and exact dimensions
through accessibility and browser qualification. It may not use those
refinements to revert to wide permanent panels, page-sized modality content, or
a battlefield that is subordinate to application chrome.

### One exact workscreen identity

The workscreen will be one mounted SVG node for the lifetime of the tactical
shell. Editor, Plan, Simulate, and Review transitions MUST preserve reference
identity of that SVG node. Changing a React/Feliz component name, element key,
or conditional branch must not replace it.

The requirement applies to the work-surface root, not to every descendant.
Terrain, unit, route, prediction, annotation, selection, fog, and event layers
may update their children as their projections change. A literal single DOM
element with no descendants would require Canvas and would discard the
project's useful SVG accessibility, inspectability, event targeting, evidence
export, and styling facilities.

Qualification will retain a reference to the SVG itself, switch through every
modality and supporting-panel transition, and assert strict reference equality.
Checking only an outer section or querying an equal ID is insufficient.

### One scene projection and renderer

The shared renderer consumes a presentation-only `TacticalSceneProjection`.
The projection normalizes the information required to draw the scene without
moving authority out of the owning subsystem.

```fsharp
type TacticalSceneProjection =
    { Revision: string
      Modality: TacticalModality
      Cursor: int64
      Camera: TacticalCamera
      Terrain: TerrainPrimitive array
      Units: UnitPrimitive array
      Routes: RoutePrimitive array
      Annotations: AnnotationPrimitive array
      Selection: TacticalSelection
      Disclosure: TacticalDisclosure
      EditableLayers: Set<TacticalLayer> }
```

This sketch is illustrative rather than a public contract. Implementation may
refine the primitive types, but the following boundaries are mandatory:

- it is derived, replaceable presentation data;
- it does not mutate an editor revision, plan, simulator runtime, or replay;
- hidden or undisclosed authoritative state cannot enter it;
- stable semantic IDs allow SVG descendants to reconcile without remounting
  unrelated geometry; and
- camera and selection live above modality-specific adapters.

Adapters project each owner into this common vocabulary:

```text
MapEditorState ───────┐
PlanningWorkspaceState├──> TacticalSceneProjection ──> one persistent SVG
SimulatorHandoff ────┤
Replay projection ───┘
```

The modality selects capabilities, not the renderer:

| Modality | Additional projection | Editable behavior |
|---|---|---|
| Editor | authored terrain, edges, regions, placement previews | map objects at the authored revision |
| Plan | authored intent, predicted routes, validation annotations | uncommitted plan intervals |
| Simulate | runtime positions, route previews, controller state | disposable simulator commands |
| Review | committed frames, disclosed events, inspection overlays | read-only |

### Compact toolbar

The top toolbar contains only high-frequency global and contextual actions:

- document identity and dirty/revision state;
- Editor, Plan, Simulate, and Review modality selection;
- undo and redo when the active owner permits them;
- play/pause and essential transport state;
- sidebar and timeline visibility controls;
- command palette or contextual action help; and
- overflow access to less frequent commands.

It does not reproduce complete tool panels. Buttons use the same authoritative
command registry as keyboard dispatch, menus, help, binding configuration, and
availability checks.

### Configurable sidebars

Every tool, roster, layer, validation, inspector, Rules, Data, or Samples panel
has a stable panel ID and declarative placement:

```fsharp
type SidebarSide = Left | Right

type PanelPlacement =
    { PanelId: string
      Side: SidebarSide
      Order: int
      Visible: bool
      Collapsed: bool }
```

Users can move panels between left and right, reorder them within a sidebar,
collapse them, and show or hide them independently. The layout is persisted
locally using a versioned, strictly parsed schema. Unknown panels and future
schema versions fail safely; newly introduced panels receive deterministic
defaults without destroying recognized user choices.

The application provides **Reset layout**. The default layout is:

- left: roster/outliner, tools, layers;
- right: selection inspector, validation, document/revision state;
- hidden until requested: Rules, Data, Samples, detailed diagnostics; and
- bottom: timeline collapsed in Editor and expanded in Plan, Simulate, and
  Review.

Hiding a panel removes its controls from tab order and command availability.
Moving, collapsing, or hiding a panel must not clear battlefield selection,
camera, timeline position, or an unrelated active operation. If a panel owns
focus when hidden, focus returns predictably to its toggle or the workscreen.

### Timeline as a bottom panel

The existing authored, predicted, accepted, and committed time channels remain
one model. Their presentation moves into a resizable bottom panel spanning the
work area beneath both sidebars and the workscreen.

The panel:

- is expanded by default in Plan, Simulate, and Review;
- is available but collapsed by default in Editor;
- persists visibility and height with the layout profile;
- does not remount the workscreen when resized or toggled;
- preserves projection-only scrubbing; and
- continues to prohibit edits in committed intervals.

### Supporting content

Rules, Data, and Samples stop being application pages. They open as sidebar
panels when their content fits inspection or selection. Larger material may
expand into a resizable temporary overlay panel, but the SVG remains mounted
beneath it and retains camera and selection state.

An overlay is not a new workspace. It has explicit close and focus-restoration
behavior, traps focus only when modal, and cannot intercept battlefield
shortcuts when focus is inside native text entry.

## Application layout

```text
┌ compact toolbar: document | modality | transport | view | actions ┐
├ configurable left ┬────── persistent SVG workscreen ──────┬ configurable right ┤
│ tools / roster    │ stable camera, scene, selection       │ inspector / issues │
│ layers / support  │ modality changes layers + capability  │ document / support │
├───────────────────┴───────────────────────────────────────┴────────────────────┤
│ collapsible, resizable shared timeline: authored → predicted → committed       │
├ contextual status and input help ──────────────────────────────────────────────┤
```

On narrow viewports, sidebars become in-flow or overlay drawers. Their
responsive presentation must not change panel ownership or remount the SVG.
The compact toolbar, active modality, workscreen, and a way to reveal the
timeline remain available at 400% reflow.

## State and event boundaries

The shell owns presentation state only:

- active modality;
- shared camera and valid cross-modality selection;
- panel placement, visibility, collapse, size, and focus return target;
- bottom-panel visibility and size;
- transient menus, overlays, drag state, and contextual help; and
- the existing tactical time cursor and binding profile.

Each domain owner continues to validate its commands. Pointer and keyboard
events enter one modality-aware command boundary:

```text
DOM event
  → normalize gesture or pointer intent
  → resolve command from active modality + focus + capability
  → evaluate authoritative availability
  → dispatch to editor / planner / simulator / replay owner
  → derive a new scene projection
  → reconcile layers inside the same SVG
```

Modality transitions cancel incomplete pointer gestures and close transient
menus. They do not implicitly accept a plan, start simulation, commit history,
change a document revision, reset the camera, or discard a still-valid
selection.

## Accessibility and interaction requirements

- The SVG has one stable accessible name and exposes modality and cursor changes
  through live status outside the work surface.
- All pointer operations retain keyboard-operable command equivalents or native
  controls in a sidebar.
- Panel headers and move/show/hide controls are native buttons with explicit
  accessible names and pressed/expanded states.
- Sidebar reordering has a non-drag alternative.
- Resizers have keyboard adjustment and expose their current value.
- Focus never remains in a hidden, collapsed, or removed panel subtree.
- Reduced motion, forced colors, 44 CSS-pixel targets, text-entry reservation,
  and browser/platform shortcut reservation remain qualified.
- Supporting overlays restore focus and never create a second application
  landmark.

## Performance and lifecycle requirements

- Modality switching performs no SVG root remount and no camera reinitialization.
- Projection adapters avoid allocating or serializing authoritative state that
  is not visible at the current cursor and disclosure level.
- Stable primitive IDs limit reconciliation to changed layers and objects.
- Simulator progress and replay playback update the scene projection without
  rebuilding shell chrome or sidebars.
- Panel layout changes are local presentation operations and do not invoke
  simulation or plan workers.
- Persisted layout writes are bounded and coalesced rather than emitted for
  every pointer pixel during resize.

## Migration strategy

Migration proceeds vertically. A compatibility layer may adapt existing
Editor, Planner, Simulator, and Replay projections into the common scene
vocabulary, but there will never be two simultaneously supported production
workscreen roots.

During transition:

1. introduce shell and projection contracts behind the current application
   entry point;
2. mount the persistent SVG before moving modality-specific drawing layers;
3. migrate one semantic layer at a time into the common renderer;
4. move controls into registered panels without duplicating commands;
5. strengthen identity tests against the SVG root;
6. reach parity for all four modalities and supporting content; and
7. delete the legacy render branches, CSS layouts, landmarks, and obsolete
   wrapper-only qualification.

If a feature cannot yet project through the shared renderer, that milestone is
not complete. Reintroducing a temporary modality-specific battlefield is not
an acceptable shortcut.

## Roadmap and milestones

Milestones are sequential. A milestone is complete only when implementation,
focused tests, browser qualification, documentation, and review evidence are
present.

### Milestone 0 — Corrective baseline

- [x] Inventory every current Editor, Plan, Simulate, and Review battlefield
  element, panel, command, focus target, camera property, and selection path.
- [x] Add a failing browser characterization proving that the outer viewport
  survives while its actual battlefield child changes.
- [x] Record current panel and responsive landmarks so migration cannot silently
  drop capability.
- [x] Mark the wrapper-level persistence claim in the earlier roadmap as
  superseded by SVG-root identity qualification.

Milestone 0 evidence is recorded in the
[corrective baseline](persistent-tactical-workspace-m0-baseline.md). Its
production-browser characterization is intentionally opt-in and red while the
known child-remount defect exists; accepted build and smoke gates remain green.

### Milestone 1 — Shell and layout contracts

- [x] Add versioned panel registry, placement, sidebar, bottom-panel, and layout
  profile contracts.
- [x] Implement strict deterministic layout import, export, persistence,
  migration, and reset.
- [x] Render the compact top toolbar and empty configurable sidebars around the
  existing battlefield without duplicating command authority.
- [x] Establish Field Focus as the deterministic default layout: narrow left
  and right sidebars, dominant workscreen, compact toolbar, and shallow
  timeline.
- [x] Qualify panel show/hide, move, reorder, collapse, focus restoration, and
  responsive drawer behavior.

Milestone 1 contracts, boundary rationale, focused tests, browser
qualification, and review evidence are recorded in the
[shell and layout evidence](persistent-tactical-workspace-m1-shell-layout-evidence.md).
Independent review corrections additionally qualify trailing-comma and
truncated-JSON rejection, strict ASCII JSON integer grammar, bottom-panel
visibility as distinct from collapse, fresh-mount persistence/fallback,
responsive-only drawer disclosures, and focus restoration for reorder, reset,
timeline collapse, and responsive drawer close.

### Milestone 2 — Shared scene projection

- [x] Define presentation-only terrain, unit, route, annotation, disclosure,
  camera, selection, and layer projection types.
- [x] Add pure adapters for Editor, Plan, Simulate, and Review.
- [x] Prove adapters cannot mutate or disclose state outside their owning
  subsystem and perspective.
- [x] Establish stable semantic primitive identities and projection performance
  budgets.

Milestone 2 contracts, ownership/disclosure rationale, focused tests, identity,
and performance/allocation evidence are recorded in the
[shared scene projection evidence](persistent-tactical-workspace-m2-scene-projection-evidence.md).
Independent review corrections additionally require an opaque validated replay
owner (including rejection of relabelled perspective data), complete semantic
selection for unit/region/command/event owners, source-stable revision identity,
and stable/unique ID plus dense performance evidence for all four modalities.

### Milestone 3 — Persistent SVG renderer

- [x] Mount one SVG work surface outside modality render branches.
- [x] Render shared terrain, edges, units, selection, annotations, and camera
  transforms as stable layers.
- [x] Route modality-aware pointer and keyboard intent through the existing
  command registry and availability boundary.
- [x] Assert strict SVG reference identity across every modality transition,
  panel operation, timeline toggle, resize, supporting overlay, and playback
  update.

Milestone 3 renderer structure, compatibility boundary, registry-routed intent,
spatial continuity, accessibility, and strict production DOM reference evidence
are recorded in the
[persistent SVG renderer evidence](persistent-tactical-workspace-m3-svg-renderer-evidence.md).
The labelled compatibility disclosure contains only non-workscreen,
registry-routed migration guidance. Existing non-workscreen Editor, Plan,
Simulator, and Review controls remain mounted as companion panels so capability
is not lost during sequential migration. Shared-camera and simulator-handoff
surface intents use the command registry. No alternate modality battlefield,
grid, workscreen, or application landmark is mounted; final sidebar placement
and specialized parity work remain assigned to Milestones 4–7.

### Milestone 4 — Editor migration

- [x] Move editor terrain, edge, region, placement, gesture preview, background,
  guide, and selection layers into the persistent renderer.
- [x] Move editor tools, layers, document state, and inspector into registered
  side panels.
- [x] Preserve undo/redo, clipboard, autosave, import review, validation,
  camera, modal input, and accessibility parity.
- [x] Remove the editor-specific battlefield renderer after parity evidence
  passes.

Milestone 4 renderer/control mapping, parity qualification, hashed visual
review, and legacy-removal proof are recorded in the
[Editor migration evidence](persistent-tactical-workspace-m4-editor-migration-evidence.md).

### Milestone 5 — Planner migration

- [x] Project authored routes, facing, attention, stance, hold, engagement,
  synchronization, predictions, and validation annotations into shared layers.
- [x] Move roster, plan tools, validation, revision state, and inspector into
  registered panels.
- [x] Preserve exact revision identity, undo/redo, stale-response rejection,
  Preview → Validate → Commit, and committed-interval protection.
- [x] Remove the planning-specific battlefield and page-sized layout.

Milestone 5 projection/control mapping, live worker and revision parity,
singleton-panel qualification, and legacy-removal proof are recorded in the
[Planner migration evidence](persistent-tactical-workspace-m5-planner-migration-evidence.md).

### Milestone 6 — Simulator migration

- [x] Project disposable runtime units, movement, route previews, controller
  state, and simulator disclosure through shared layers.
- [x] Move simulator tools, controller configuration, diagnostics, and revision
  state into registered panels.
- [x] Preserve immutable handoff, run/step/reset, worker correlation,
  authoritative runtime tick, and projection-only scrubbing.
- [x] Remove the simulator-specific battlefield and map-stage layout.

Milestone 6 shared runtime projection, registered-panel ownership, immutable
handoff/time boundaries, real-worker correlation, singleton browser trace, and
legacy-removal proof are recorded in the
[Simulator migration evidence](persistent-tactical-workspace-m6-simulator-migration-evidence.md).

### Milestone 7 — Review migration

- [x] Project committed replay frames, disclosed events, inspection state, and
  verification annotations through shared layers.
- [x] Move sources, transport detail, event inspection, and worker status into
  registered panels.
- [x] Preserve read-only committed history, perspective filtering, cancellation,
  verification identity, and playback interpolation.
- [x] Remove the replay-specific battlefield and dashboard layout.

Milestone 7 opaque accepted-owner projection, verification/disclosure mapping,
registered-panel ownership, real-worker full/perspective evidence, singleton
browser trace, and legacy-removal proof are recorded in the
[Review migration evidence](persistent-tactical-workspace-m7-review-migration-evidence.md).

### Milestone 8 — Timeline and supporting panels

- [ ] Move the unified timeline into a resizable, collapsible bottom panel with
  persisted height and modality defaults.
- [ ] Preserve one cursor and authored, predicted, accepted, and committed
  channels without duplicating planning lanes.
- [ ] Convert Rules, Data, and Samples into registered side or temporary overlay
  panels.
- [ ] Prove no timeline or supporting-content operation remounts the SVG or
  clears camera and valid selection.

### Milestone 9 — Legacy removal and acceptance

- [ ] Delete superseded modality-specific battlefield functions, workspace
  branches, CSS layouts, DOM landmarks, and wrapper-only lifecycle assertions.
- [ ] Prove there is exactly one production renderer and one mounted SVG
  workscreen path.
- [ ] Complete visual and interaction review against the Field Focus mockup and
  prove the workscreen remains dominant with both default sidebars open.
- [ ] Run .NET/Fable parity, client conformance, production browser smoke,
  accessibility, documentation, review-image, and sequential FAKE gates.
- [ ] Update living user and architecture documentation, record migration
  evidence, and mark this report implemented.

## Acceptance evidence

The redesign is accepted only when all of the following are demonstrated:

| Concern | Required evidence |
|---|---|
| Workscreen identity | one retained SVG object remains strictly equal across all four modalities |
| Renderer uniqueness | source and bundle inspection find no alternate production battlefield root |
| Field Focus goal | compact toolbar, narrow default sidebars, shallow timeline, and a dimensionally dominant workscreen match the accepted target |
| Spatial continuity | camera transform and valid selection survive every modality round trip |
| State authority | modality and layout changes do not mutate map, plan, runtime, or replay ownership |
| Disclosure | Plan, Simulate, and Review projections expose only allowed knowledge |
| Panel configuration | move, reorder, collapse, hide, restore, persist, migrate, and reset are deterministic |
| Focus | hidden/moved panels and overlays restore focus without trapping keyboard users |
| Timeline | bottom-panel changes preserve cursor and never commit through scrubbing |
| Responsiveness | 400% reflow retains modality, workscreen, sidebar access, and timeline access |
| Command authority | toolbar, panels, pointer, keyboard, help, and bindings share availability |
| Removal | legacy render paths and modality-page CSS are absent after parity |

The browser smoke must retain the SVG object itself:

```javascript
const workscreen = document.querySelector(
  '#tactical-workscreen[aria-label="Tactical workscreen"]',
);

for (const modality of ["Plan", "Simulate", "Review", "Editor"]) {
  activateModality(modality);
  await settle();
  if (document.querySelector("#tactical-workscreen") !== workscreen) {
    throw new Error(`Tactical workscreen remounted in ${modality}.`);
  }
}
```

This assertion replaces the current weaker check against
`#tactical-battlefield-viewport`.

## Consequences

### Positive

- The interface matches the user's spatial task: one battlefield, many
  purposes.
- Camera, selection, focus, and time become genuinely persistent rather than
  wrapper-level promises.
- Rendering, hit testing, accessibility, and visual evidence converge on one
  path.
- Panels can evolve independently without creating new pages or battlefield
  implementations.
- Mode changes become capability changes and no longer resemble navigation
  between tools.
- Removal of duplicate renderers reduces drift and makes lifecycle tests
  meaningful.

### Costs and risks

- Existing Editor SVG and Simulator/Replay battlefield vocabularies must be
  normalized without weakening domain ownership.
- Pointer gestures, coordinate transforms, selection, and camera code require
  careful convergence.
- A configurable layout introduces persistence, focus, drag/reorder, migration,
  and responsive complexity.
- Temporary compatibility adapters may increase code before legacy deletion
  reduces it.
- Review evidence must cover every migrated capability; visual similarity alone
  is insufficient.

These costs are accepted because retaining multiple battlefield implementations
would continue to contradict the product decision and compound future work.

## Alternatives considered

### Keep the current persistent wrapper

Rejected. It preserves an outer DOM node while replacing the work surface and
large portions of the interface. It meets the letter of the old smoke test but
not the intended interaction model.

### Keep separate renderers behind identical styling

Rejected. Styling cannot preserve element identity, camera ownership, focus,
hit testing, or renderer parity. It would make the architectural duplication
less visible without removing it.

### Use one Canvas element

Rejected for the present product. Canvas would satisfy literal single-element
rendering but would discard existing SVG semantics, accessible structure,
inspectable evidence, CSS state, and direct event targets. The supported scale
does not justify that regression.

### Merge all authoritative state into one workspace model

Rejected. Presentation unification does not authorize the editor, planner,
simulator, and replay to share mutation authority. Their separate lifecycle and
disclosure boundaries are essential.

### Keep Rules, Data, and Samples as replacement pages

Rejected. Replacing the tactical shell contradicts the persistent-workscreen
goal. Registered panels and temporary overlays provide access without spatial
discontinuity.

## Decision summary

S.I.R. will have one VS Code-style tactical shell and one persistent SVG
workscreen. Modality changes will alter projection, layers, tools, commands,
inspectors, and editability inside that stable surface. Side panels and the
timeline will be configurable and persisted. Supporting content will appear in
panels or overlays. Field Focus is the mandatory default density and spatial
goal: the workscreen dominates while compact supporting chrome stays available.
Legacy modality-specific render paths will be deleted after measured parity,
and strict SVG reference identity plus Field Focus review will be release gates.
