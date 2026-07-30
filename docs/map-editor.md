---
title: Map Editor Reference
category: Tools & Evidence
categoryindex: 5
index: 2
status: accepted
decision-status: implemented
document-type: reference
version: "1.11"
last-updated: 2026-07-29
description: Map records, terrain, semantic edges, square unit footprints, controller modes, and deterministic execution.
related:
  - docs/interactive-rules-lab.md
  - docs/svg-replay-player.md
  - docs/fable-client-and-documentation.md
---

# Map Editor Reference

The browser application presents **Editor**, **Plan**, **Simulate**, and
**Review** as modalities of one
[unified tactical workspace](unified-tactical-workspace.md). The battlefield,
camera, selection where valid, and time cursor remain mounted while the active
tooling changes. The editor supports terrain, semantic edges, square units, and
versioned map files. Simulate provides manual control, repeatable scripts, and
a bundled general controller.

Use the Simulate modality for the immutable sandbox handoff.

## Accepted editor contract

The [VTT-inspired experience report](2026-07-29-1230-map-editor-vtt-experience-design-report.md)
is accepted as the implementation direction, with the footprint presets and
input tables below as its initial frozen amendments. The contract keeps these
authority boundaries:

- `MapDefinition` and a validated `SIR-MAP` document contain authoritative
  terrain, semantic edges, units, and square occupancy.
- Camera, panels, selection, pointer previews, guides, and background art are
  disposable UI state or separately versioned authoring metadata.
- The Editor produces immutable revisions; controller execution and ticks
  remain Simulator concerns and cannot rewrite an authored revision.
- SVG is the documentation editor renderer and review surface. It does not
  become a second source of map rules.
- The static GitHub Pages client reads local inputs and exports local evidence.
  It is not an authoritative match host and does not upload maps or assets.
- `Repository bundle` exports editor and simulator design state for explicit
  import into a version-controlled checkout. The static client never stores a
  repository credential.
- `SIR-MAP 3` is the canonical portable format. It retains the typed zones
  introduced in version 2 and adds explicit body-facing and attention
  directions to unit records. Versions 1 and 2 remain readable; units imported
  from either receive deterministic `North` body-facing and `North` attention
  defaults before canonical version 3 export.

## Map

| Field | Constraint |
|---|---|
| Width and height | 4–40 cells |
| Coordinates | Zero-based integers |
| Terrain | Open, rough, blocked, or objective |
| Edges | East or south edge of a cell |
| Regions | Positive-ID rectangle or simple polygon; objective or blue/red deployment purpose |
| Edge kinds | Wall, door, or window |
| Units | Positive identifier, square footprint, current and maximum HP, controller |

Qualification safety limits bound untrusted work before semantic validation:
native and interchange inputs are at most 2,000,000 UTF-8 bytes, class IDs are
at most 128 characters, polygons are at most 256 vertices, a document contains
at most 1,600 regions, and the internal unit clipboard contains at most 256
units. Local rasters retain the separate 10,000,000-byte and 8,192-pixel
dimension limits. Exceeding a limit rejects the complete operation without
changing the authored revision.

Blocked terrain cannot contain a unit. Unit footprints cannot overlap or extend
outside the map. A selected unit exposes editable side, class identifier,
square size, and HP fields. Controller and script fields are in the Simulator
tab.

## Square unit geometry

Every unit base is square. `size` defines both dimensions:

```fsharp
footprintWidth = size
footprintDepth = size
```

The editor and simulator both resolve symbols through the canonical unit glyph
catalog. One symbol uses the same square bounds as its base. A size-1 unit
occupies 1×1 cells; a size-2 unit occupies 2×2 cells. Movement checks every
crossed edge along the complete leading side of that square.

### Canonical footprint presets

These presets are the initial placement defaults. Their order is stable review
evidence, and their dimensions are also recorded by
`MapEditor.canonicalFootprintPresets`. A placed unit may expose an explicit size
override only when scenario validation permits it; changing art never changes
occupancy.

| Preset ID | Default class ID | Footprint |
|---|---|---:|
| `goblin` | `goblin` | 1×1 |
| `orc` | `orc` | 2×2 |
| `troll` | `troll` | 3×3 |
| `human` | `rifleman` | 2×2 |
| `human-gunner` | `gunner` | 2×2 |
| `human-marksman` | `marksman` | 2×2 |
| `human-engineer` | `engineer` | 2×2 |
| `human-medic` | `medic` | 2×2 |
| `human-signaller` | `signaller` | 2×2 |
| `drone` | `observation-drone` | 1×1 |
| `relay-drone` | `relay-drone` | 1×1 |

The initial human personnel classes deliberately share the same body and
occupancy defaults while retaining distinct role glyphs. Observation and relay
drones likewise share a footprint and body profile. A new class does not
inherit a preset by name or image: it must be assigned deliberately.

## Initial input contract

The editor uses a compact, map-first desktop-authoring shell patterned after
established document and modelling applications:

- A `File`, `Edit`, `View`, and `Map` menu bar provides a stable home for the
  complete and growing command set.
- A dense quick-access toolbar keeps New, Open, Save, history, primary tools,
  camera controls, and simulation handoff one click away.
- The `Terrain`, `Units`, `Edges`, `Zones`, and `Map file` command rail is
  docked to the map's right border. Only one contextual panel is shown at a
  time, preserving canvas width as more tools are added.

Activating a command-rail group opens its contextual panel in place; choosing
it again hides the panel. The contextual panel and inspector start hidden so
the remaining workspace is the map. Keyboard bindings are presented from the
live modal catalog rather than duplicated in static toolbar or documentation
copy.

`File`, `Edit`, `View`, and `Map` menus close when focus moves to another menu
or the user clicks outside the menu. The right-edge group labels remain visible
when a contextual panel opens, so the active label can close the same panel
without searching for a separate dismiss control.

`File > New map` and the quick-toolbar `New` command request confirmation
before replacing the draft with a named, empty 12×8 map. This is distinct from
`Clear`, which preserves the current document dimensions and authoring
identity. `Open / Import…` reads a local map file; `Save map file` downloads
the canonical portable map; and `Repository bundle` downloads editor and
simulator design work for explicit import into a version-controlled checkout.

`Rules and data` is a separate application workspace. It shares canonical
domain types and rendering infrastructure with the rest of the client, but its
scenario laboratory does not mutate the editor draft or simulator handoff.

The SVG workspace uses application-level input only while it has focus.
Otherwise browser and documentation-page input retains its normal behavior.
Standard focus navigation reaches tools, the inspector, and a parallel object
list without visiting every empty cell. A keyboard map cursor and the object
list provide the non-pointer path for canvas commands; the live catalog
discloses their current bindings.

### Version-controlled design transfer

`Repository bundle` downloads a `*.sir-design.json` file containing the
canonical editor map, revision identity, and—when one exists—the simulator's
immutable runtime map and tick. A browser cannot write into a checkout merely
because the site was opened from that checkout.

Import a downloaded bundle from the repository root:

```console
npm run import:map-design -- ~/Downloads/<name>.sir-design.json
```

The importer validates the format and writes reviewable files under
`designs/map-editor/<map-name>/`. Those files then follow the normal branch,
commit, review, and pull-request workflow. Direct browser-to-GitHub saving
requires a GitHub App or OAuth service with explicit repository contents
permission; credentials must not be embedded in the static client.

### Pointer and touch gestures

| Intent | Mouse or pen | Touch | Commit and cancellation |
|---|---|---|---|
| Select | Primary click an object | Tap an object | Replaces selection |
| Toggle selection | Modified primary click | Select-mode toggle, then tap | Adds or removes one object |
| Box select | Primary drag from empty workspace | Select-mode drag | Pointer/touch release commits; the cancel action discards |
| Pan | Middle-drag or right-drag | Two-finger drag | Camera-only; never commits map state |
| Zoom | Wheel or trackpad scroll around pointer | Two-finger pinch around midpoint | Camera-only and bounded |
| Place unit | Move for full-footprint preview, then primary click | Move map cursor, then tap **Place** | One unit per activation; the cancel action discards the preview |
| Paint terrain | Primary drag | One-finger drag in an active paint tool | One drag is one command; release commits |
| Rectangle or line | Primary drag from anchor | Tap anchor, move cursor, tap **Apply** | Release/**Apply** commits; the cancel action discards |
| Flood fill or eyedrop | Primary click | Tap | Commits one fill or selects one terrain value |
| Draw edge polyline | Primary click/drag across snapped edges | Tap successive snapped edges | Double-click/**Finish** commits; **Back** removes the last segment before cancellation |
| Move selection | Primary drag selected unit(s) | Tap **Move**, reposition cursor, tap **Apply** | Full route and footprint preview; the cancel action discards |
| Open object actions | Secondary click or context button | Long-press or context button | Opens the same linear action menu used by every input path |

Pointer coordinates are mapped through the SVG's centered aspect-fit viewport.
Horizontal or vertical letterboxing therefore remains non-interactive padding
and cannot shift a click into a neighboring logical cell.

Pointer capture belongs to an active drag and must be released on commit,
cancel, lost capture, tool switch, or unmount. Hover is never required.

### Keyboard gestures

The current-mode strip and its **Possible inputs** disclosure are the
authoritative keyboard reference. They are projected from the same catalog
that resolves production input, so only executable commands for the current
tool, gesture, held layer, or transient popup are shown. Native text fields
retain platform editing bindings. Repeated movement inputs remain
deterministic, and every catalog command has a visible control or
inspector/object-list equivalent.

The [accepted Editor and Simulator modal input design](keyboardInput/editor-simulator-modal-input-proposal.md)
and its [complete key vocabulary](keyboardInput/editor-simulator-modal-key-vocabulary.md)
are the supported interaction contract. Acceptance qualification covers every
derived mode and declared repeat policy, complete production-browser Editor and
Simulator workflows, popup and held-layer recovery, pointer/touch equivalence,
canonical output and undo/redo, deterministic simulation, worker transport,
screen-reader semantics, forced colors, reduced motion, minimum targets, and
400% reflow. Migration-only shifted and symbol compatibility aliases were
removed; only bindings listed by the live catalog are supported.

## SVG workspace and camera

The Editor tab renders the authoritative map through the shared tactical SVG
grammar: the same `Battlefield.CellSize`, accessible replay palette, canonical
unit glyph catalog, square footprint geometry, terrain semantics, and
east/south edge normalization used by the simulator and replay surfaces. The
former HTML cell-button board is retained only as a collapsible object-list
fallback. It is not visible map presentation and does not create a second map
model.

Camera and panel state live in `EditorWorkspaceState`, outside
`MapDefinition`. The camera supports:

- pointer-centered wheel and trackpad zoom between `0.25×` and `6×`;
- middle-drag and right-drag panning, plus the held-pan mode disclosed by the live input catalog;
- two-pointer touch pan and pinch around the touch midpoint;
- board fit, 100% reset, and selected-unit framing controls;
- viewport resize without changing board coordinates; and
- immediate camera updates under reduced-motion preferences.

Screen coordinates are transformed through the inverse camera before map
activation. Cell hits use the transformed cell coordinate. Edge hits compare
their distance in screen pixels against a nine-pixel tolerance, so the target
does not become harder to acquire as zoom changes. Every accepted edge hit is
normalized to an east or south `MapEdgeDirection` record before dispatch.

An active drag owns pointer capture. Commit, pointer release, lost capture,
the current cancel action, reset, and workspace disposal all clear captured
pointer state.
The SVG itself exposes only semantic units as keyboard stops. A collapsible
linear object list exposes every unit and cell with conventional buttons for
keyboard and assistive technology. Tool groups, the contextual palette, camera
buttons, status row, and collapsible inspector remain ordinary HTML controls.

The deterministic
`tests/SIR.Client.Tests/fixtures/map-editor-milestone-1-camera.txt` review
fixture records fit, zoom, frame-selection, cell-hit, and low/high-zoom
edge-hit results. Pure tests also cover resize, pointer capture and loss,
mouse drag, touch pinch, release cleanup, and reduced-motion state.

## Commands, selection, and revision identity

Committed authoring changes pass through a typed `EditorCommand`, pure
validation, and immutable `MapRevision`. A revision contains the complete
`MapDefinition`, its parent digest, a monotonic local revision number, and the
lowercase SHA-256 digest of the canonical UTF-8 `SIR-MAP 3` document. Camera,
selection, clipboard, gestures, panels, runtime ticks, and animation never
contribute to that digest.

Pointer click replaces the active unit selection. Modified pointer activation
and the object-list toggle action add or remove one unit. Dragging from empty
workspace in Select mode previews a disposable box and selects every unit
footprint intersecting the committed box. The object list and visible command
buttons provide the same non-pointer path. Select-all is scoped to the active
domain; until other domains expose selectable semantic objects, it selects
units only in Select or Unit mode.

Undo and redo restore bounded prior-document snapshots. The newest 100
commands are retained while their canonical before/after documents remain
within a combined 2,000,000-byte budget; the older tail is discarded as soon
as either bound is reached. A new command clears redo. Copy does not create a
revision. Paste and duplicate allocate fresh IDs, retain formation offsets,
search deterministic positive diagonal offsets, validate the complete
formation atomically, and commit one revision or none. Delete removes the
complete active-domain selection as one undoable command.

Revision lifecycle labels are explicit:

- **Dirty** means the current digest differs from the last explicitly saved
  revision.
- **Saved** records the digest exported as a map document.
- **Simulated** records the exact revision selected when entering Simulator.
- **Recovered** retains the source autosave digest without changing canonical
  map identity.

The deterministic
`tests/SIR.Client.Tests/fixtures/map-editor-milestone-2-history.txt` fixture
freezes the initial cross-runtime digest, history limits, selection order, and
revision-state labels. Generated command tests prove `.NET` undo/redo
round-trips over 64 cell choices. The Fable/Node browser smoke test checks the
same initial digest, commits a copied unit, and proves undo and redo return to
the same digests and object counts.

## Simulator handoff and previews

**Simulate** validates the current authored document and creates
a `SimulatorHandoff` containing that exact immutable `MapRevision` plus a
separate runtime map, tick, event list, run state, and disposable route
preview. Opening the Simulator tab alone never creates or refreshes a handoff.
Editing after a handoff leaves the sandbox intact and displays **Simulator
behind editor draft** until the author explicitly hands off another revision.

Runtime controller assignment, scripts, movement, damage, ticks, and route
previews update only the handoff. They do not change `MapDefinition`,
`MapRevision`, the draft digest, undo/redo entries, saved state, or autosave.
Re-running **Simulate** deliberately resets the sandbox to a new
copy of the selected revision.

The Simulator uses the same desktop-authoring structure as the Editor:

- `File`, `View`, `Simulation`, and `Samples` menus hold the complete command
  set.
- Run/Pause, Step, Reset, panel selection, zoom, and camera reset remain in a
  dense quick-access toolbar.
- The battlefield is the primary surface. `Controls`, `Events`, and `Samples`
  form a labeled right-edge rail; the active panel opens to the rail's left,
  and the labels remain available to close or switch it.
- The live modal input strip presents panel and simulation commands from the
  production catalog. Reset restores the immutable handed-off revision rather
  than rewriting the Editor draft.

The route planner uses deterministic weighted A* over eight directions. It
routes around blocked terrain, semantic walls, windows, closed doors, and
occupied square footprints. Diagonal traversal cannot cut across a blocked
orthogonal corner. Open-ground cardinal and diagonal steps cost 500 mm and
707 mm respectively; entering rough ground costs 1.5 times as much movement
credit. Stable direction, row, column, and unit-ID ordering resolve equal-cost
choices.

The planned route reports step count, physical distance, and movement-credit
cost. Its presentation-only overlay remains visible after acceptance while
steps remain queued. The labelled route controls move the destination, commit
a clear route, reset it, or discard the preview. The live input catalog
projects the equivalent commands for the current Simulator mode. A committed
route never teleports the unit.
Once a unit starts accumulating credit toward an adjacent cell, that segment
remains locked until it commits or a new collision invalidates it. Controllers
therefore replan only at cell boundaries instead of changing the projected
direction between ticks.
**Run/Pause** and **Step**, together with their current live-catalog commands,
advance the same deterministic sandbox without entering authored history.

### Fixed-timestep movement

The editor sandbox delegates each step and route query to the shared
`SIR.Simulation.MapScale` kernel. The editor simulator converts immutable map
revisions into kernel state, forwards controller intent, and projects returned
events and interpolation only; it does not own numeric movement, collision, or
combat rules.

The kernel uses the canonical 20 Hz simulation rate: one tick is 50 ms and
one map cell represents 500 mm. Movement stays in fixed-point integer
millimetres:

```text
earned credit per tick = speed millimetres per second / 20
cardinal cell cost     = 500 mm
diagonal cell cost     = 707 mm
rough-ground cost      = base cost × 1.5
```

The shared prototype movement profiles are 2.0 m/s for goblins, 1.5 m/s for
riflemen and fallback units, 1.2 m/s for orcs, 1.0 m/s for trolls, and
3.0 m/s for observation drones. Credits are capped at 1,060 mm, preventing
unbounded stored movement while still permitting a rough diagonal step.

A 1.5 m/s rifleman earns 75 mm per tick. It therefore commits its first 500 mm
open-ground cardinal step on tick seven and retains 25 mm. Runtime state never
contains fractional cells. During movement, the browser projects the unit
between its committed origin and intended adjacent destination according to
the accumulated credit fraction. At 20 updates per second this produces
continuous open-ground motion before the cell boundary is committed, instead
of a delayed post-commit slide. Reduced-motion and exact-tick modes suppress
the fractional projection. Presentation coordinates never feed back into
collision or simulation state.

The **Run** preview schedules one authoritative tick every 50 ms, matching
simulated real time. **Step** advances exactly one 50 ms tick.

Player-perspective preview accepts only a `RenderFrame` explicitly labelled
`PerspectiveDisclosure`. The editor has no accepted disclosure-filtered
projection producer, so its player-perspective control is disabled with a
programmatic explanation. A sandbox or full-replay frame is never relabelled
or filtered in the UI.

The shared simulation kernel does not yet expose accepted perception rules for
editor maps. Manual reveal and derived visibility overlays are therefore
disabled with an explicit unavailable status. The editor does not invent
line-of-sight, fog, or retention behavior from the perception spike. Those
overlays may be enabled only when an accepted shared-kernel result can supply
their geometry and disclosure.

## Curated samples

The top-level **Samples** workspace groups reviewable examples by use:

- **Troll assault:** three General-controller riflemen face a 240 HP, 3×3
  armored troll across open and rough ground.
- **Breach corridor:** humans and goblins converge on a closed semantic door.
- **Objective crossing:** mixed footprints contest typed objective and
  deployment regions through rough and blocked terrain.
- **Replay walkthroughs:** precomputed tick-by-tick versions of the troll
  contact and closed-door stalemate scenarios.

Each expandable map entry can replace the current draft in Editor or start a
new immutable Simulator sandbox. Replay entries open locally navigable
projections in Replay.
They are explicitly labelled deterministic sandbox evidence and are not
cryptographically verified match replay packages.

The General controller now uses a small typed attack profile for each unit
class. Riflemen fire 12-damage projectiles at up to eight cells; trolls, orcs,
goblins, and unrecognized classes retain class-specific or fallback melee
profiles. The troll assault therefore answers its intended question visibly:
projectile lines, moving shot marks, and impact rings show every rifle volley,
while solid strike lines and crossed impact marks distinguish melee attacks.
Projectile paths are deterministically intercepted by blocked terrain, walls,
windows, and closed doors, so the breach sample remains a true stalemate.
Attack profiles also define deterministic recovery. Riflemen recover for ten
ticks between shots, trolls and orcs for twenty ticks between strikes, and
goblins for sixteen ticks. A unit in recovery cannot attack again every 50 ms;
this lets movement and engagement geometry remain visible in the assault
sample.

Combat events carry typed delivery values rather than relying on their display
text. `MeleeDelivery` and `ProjectileDelivery` are implemented.
`LobbedAreaDelivery` and `SpellAreaDelivery`, together with unit or area
targets, reserve the presentation contract needed by future grenades and
spells. Area targets are typed as burst, cone, ray, or rectangle templates
rather than an ambiguous point and radius. Those future deliveries still
require accepted trajectory, area-of-effect, line-of-sight, resistance, and
friendly-fire rules before they can resolve damage.

Samples are presented as compact expandable lists rather than a card grid.
The collapsed row keeps type, name, and summary scannable; expanding a row
reveals its mechanics and the commands to open the map, run the simulation, or
inspect the replay.

The deterministic
`tests/SIR.Client.Tests/fixtures/map-editor-milestone-9-simulator.txt` review
fixture freezes the handoff digest, stale state, runtime tick isolation,
route/distance/collision results, and the two gated-unavailable explanations.

## Local backgrounds and interchange review

Raster backgrounds are local presentation references in `EditorWorkspaceState`.
They are not fields in `MapDefinition`, `SIR-MAP 3`, its canonical digest,
autosave, simulation input, or exported evidence. The browser never fetches a
background URL. A file must pass signature inspection as PNG, JPEG, or WebP,
match its declared media type, contain dimensions from 1 through 8,192 pixels
on each axis, and contain no more than 10,000,000 bytes. SVG is rejected even
when presented as an image because it may contain executable or externally
referenced content.

The Document tool exposes a labelled local-file input and ordinary controls for
lock, opacity, fit-inside, fill-and-crop, stretch, source crop, grid offset, and
source pixels per cell. Backgrounds start locked. Unlocking is required before
nudging, cropping, scaling, or aligning. Every pointer-oriented adjustment has
a focusable button or numeric input, its result is announced through a polite
live region, and the raster is `aria-hidden` because canonical terrain and
objects remain available through the SVG and parallel object list. Camera and
background changes cannot create an undo entry or change the map revision.

External maps use a two-stage workflow:

1. Select a local `.dd2vtt`, `.uvtt`, Foundry `.json`, or Fantasy Grounds
   `.xml` export. The file is parsed locally; external asset paths are never
   opened.
2. Inspect the complete field report. Every source leaf is classified as
   **mapped**, **ignored**, **lossy**, or **rejected**. **Accept reviewed
   import** remains disabled when no deterministic candidate exists. Acceptance
   atomically replaces the map through the normal validated `SIR-MAP 3` import.

The currently reviewed mappings are deliberately narrow:

| Source | Deterministic mapping | Ignored or lossy |
|---|---|---|
| Universal VTT 0.3-style JSON | Integral `resolution.map_size`, positive `pixels_per_grid`, `map_origin`, axis-aligned on-grid `line_of_sight` to closed walls, and two-point `portals` to doors with open/closed state | Images, lights, environment, textures, and unknown fields are reported and ignored. Curves, diagonals, off-grid segments, and unrepresentable top/left border geometry are reported as lossy. |
| Foundry scene JSON with square grid | Integral `width`/`height` divided by `grid.size`; axis-aligned on-grid `walls[].c`; `door` and `ds=1` to semantic open doors | Tokens, actors, tiles, notes, regions, lighting, vision, sounds, remote background paths, wall restrictions, and unknown flags are reported and ignored. Locked doors are reported as lossy and become closed doors. |
| Fantasy Grounds image/campaign XML | None | Evaluation found no stable portable contract across campaign/database schemas and extensions. Import is rejected with an explanation; export through Universal VTT or recreate semantic edges in S.I.R. |

No pixels, filenames, light polygons, token art, wall colors, scripts, macros,
or remote asset paths infer terrain, occupancy, units, objectives, deployment,
or behavior. Duplicate source JSON keys and conflicting duplicate semantic
edges reject the complete import instead of depending on parser or file order.
The deterministic
`tests/SIR.Client.Tests/fixtures/map-editor-milestone-8-interchange.txt` fixture
freezes raster validation, transform state, unchanged authority digest,
Universal VTT mappings, Foundry loss reporting, ignored remote paths, and the
Fantasy Grounds rejection.

## Terrain authoring

Terrain authoring uses pencil, rectangle, line, flood-fill, eyedropper, and
erase tools. Pencil, line, and rectangle expand their exact cell geometry by a
configurable integer square brush from `1×1` through `9×9`; even-sized brushes
bias the additional cell toward increasing columns and rows. Previews are
deduplicated and sorted by row then column, so identical gestures produce the
same `PaintCells` payload across runtimes. Line geometry uses an integer
Bresenham walk. Flood fill visits the four orthogonal neighbors of contiguous
source terrain.

Pointer release or **Apply preview** validates and commits the whole gesture as
one `PaintCells` command and one immutable revision. Undo therefore removes a
complete stroke, not its individual pointer samples. Blocked previews that
intersect any occupied square footprint fail document validation before the
map, revision, or history changes. Erase removes explicit terrain records and
restores canonical open terrain. Eyedropper changes only the selected palette
value.

Every terrain value has a text label and non-color pattern: open/plain,
rough/diagonal hatch, blocked/cross hatch, and objective/inset ring. The SVG
preview uses a dashed outline, changes to a short red dash for invalid
geometry, and mirrors its cell count, sample, commit, or rejection through a
polite live region. The exact cursor, extension, activation, and cancellation
bindings and their visible equivalents come from the live modal catalog.

The deterministic
`tests/SIR.Client.Tests/fixtures/map-editor-milestone-3-terrain.txt` fixture
freezes tool order, catalog command identities, patterns, row-major pencil and line geometry,
boundary clipping, and the maximum `40×40` map contract. The client test suite
alternates 80 flood-fill and diagonal-line preview-plus-validation gestures on
a `40×40` map and enforces a 50 ms p95 guardrail. On the 2026-07-29 .NET 10
conformance run in the repository environment, the measured p95 was 26.123 ms.

## Unit palette and direct manipulation

The Units palette searches canonical presets by faction, tactical role, class,
glyph, or display name, then groups matches by faction. A preset explicitly
declares its class ID, normalized glyph ID, default side, square footprint,
current HP, and maximum HP. The three arcane bodies, six human personnel
classes, and two drone roles therefore do not depend on hidden placement
defaults.

Pointer movement with a placement tool previews the complete occupied square.
The outline remains a constant screen-space width at every camera zoom and
uses different dash shapes as well as color for valid and invalid states.
Click commits the preview as one `AddUnits` command. Dragging selected units
previews a translated `UpdateUnits` command; release commits the complete
selection or none of it. The focused SVG workspace also exposes the same
atomic one-cell formation move through the live input catalog. Route checks
include every selected unit's leading edge, and final validation includes map
borders, blocked terrain, and all occupied footprints.

The context HUD is a linear six-action toolbar: four orthogonal movement
actions, duplicate, and delete. Every action has a keyboard or inspector
equivalent. The complete inspector edits side, class, square size, current and
maximum HP, controller, and direction script through the command/revision
path. With multiselection, compatible field edits apply to the entire
selection as one command.

Copy stores sorted semantic units rather than SVG or HTML. Paste allocates IDs
in source-ID order, retains every relative formation offset, searches stable
positive diagonal offsets, and validates the whole fragment before creating
one revision. Duplicate is the same copy-plus-paste operation. A failure leaves
the map, selection, ID counter, revision, and history unchanged.

One normalized 24×24 glyph from `UnitGlyphCatalog` is centered and scaled
inside each square base; the base and symbol transform together for `1×1`
through the maximum supported `8×8` footprint at every camera zoom. The
deterministic
`tests/SIR.Client.Tests/fixtures/map-editor-milestone-4-units.txt` fixture
freezes palette grouping and every preset default. Pure tests cover `1×1`,
`2×2`, `3×3`, and `8×8` footprints at exact borders and across borders,
blocked terrain, overlap, blocking edges, atomic multiselection movement, and
formation-safe copy/paste.

## Semantic edges

Every physical grid segment is normalized to one authoritative east or south
record:

```text
edge <column> <row> <east|south> <wall|door|window> <open|closed>
```

Vertical gestures belong to the east edge of the cell on their west; horizontal
gestures belong to the south edge of the cell on their north. A gesture on the
north or west outer border is rejected because it has no owning cell. The
normalizer never creates both a leading and trailing representation.

Wall clicks accumulate a disposable polyline preview. Double-click, **Finish**,
**Finish**, catalog activation, or selecting another tool commits all unique
segments as one `ReplaceEdges` command and immutable revision. Catalog
backtrack removes the last preview segment; repeating it after the preview is
empty cancels. The same
nine-CSS-pixel screen-space tolerance is applied before inverse camera
projection at every zoom.

Door and window activation converts an existing segment or creates it
atomically. Inspector-equivalent buttons convert to wall, door, or window,
toggle a door open/closed, erase, split a run by removing one segment, and join
a run with one wall segment. Each operation uses the command/revision path and
is independently undoable. Wall and window edges block movement. Closed doors
block movement; open doors do not.

Pure validation rejects duplicate or overlapping canonical addresses in one
command, invalid owner/border records, and open state on non-door edges.
Non-destructive lint identifies enclosed gaps in collinear runs. Movement lint
checks every cell of a square footprint's leading side.

Canonical export orders edge records by their structural map key. Import
reconstructs the same edge kind, door state, direction, and key, and a second
export is byte-identical. The deterministic
`tests/SIR.Client.Tests/fixtures/map-editor-milestone-5-edges.txt` fixture
freezes normalization, polyline order, conversions, gap and leading-side lint
codes, and exact round-trip behavior.

## Zones, objectives, and deployment

Regions are authoritative records with three independent fields:

- `Geometry` is either a positive-size cell rectangle or a simple polygon of
  at least three unique integer grid-intersection vertices.
- `Purpose` is objective, blue deployment, or red deployment.
- `Behavior` is the closed `NoRegionBehavior` case in version 2.

Rectangles must remain inside the cell bounds. Polygon vertices may lie on the
outer grid boundary, but the polygon must have non-zero area and cannot
self-intersect. Stable validation codes cover identity, purpose, bounds,
rectangle size, vertex count, duplicate vertices, area, and self-intersection.

Create, select, translate, change-purpose, move-polygon-vertex, and delete
operations all use `AddRegions`, `UpdateRegions`, or `RemoveRegions`; therefore
they participate in pure validation, revision digests, bounded undo/redo,
layer locks, resize-loss preview, crash recovery, and deterministic export.
The SVG region shapes and the HTML object list both expose selection. The
Zones panel supplies labelled creation and editing controls, a polite live
announcement reports changes, and the object list supports conventional
roving focus and activation without adding a tab stop for every grid cell.

`SIR-MAP 3` accepts only the declared line grammar. It has no macro, script,
callback, expression, or trusted behavior record. Unknown records and
unversioned behavior tokens fail the complete atomic import. Any future region
behavior requires a new reviewed union case and a versioned format change.
Legacy `SIR-MAP 1` and `SIR-MAP 2` documents remain loadable and migrate to the
same immutable model before their next canonical version 3 export.
The deterministic
`tests/SIR.Client.Tests/fixtures/map-editor-milestone-7-zones.txt` fixture
freezes rectangle/polygon editing, v1 loading and v2 canonicalization,
round-trip order, stable invalid-geometry codes, lock behavior, and rejection
of macro/behavior records.

## Controllers

### Manual

A direction button queues a movement intent. A route commitment queues its
path. The unit accumulates movement credit on subsequent ticks and commits the
next step only when it can pay that step's terrain-adjusted cost:

```fsharp
credit = min maximumCredit (credit + speed / ticksPerSecond)
if credit >= stepCost && collision = RouteClear then
    position = nextPosition
    credit = credit - stepCost
```

An invalidated queued step is rejected and the route is cleared rather than
allowing a unit to overlap, leave the map, or cross a blocking edge.

### Scripted AI

A script is a comma-separated sequence from:

```text
N, NE, E, SE, S, SW, W, NW
```

The current script direction remains active while movement credit accumulates:

```fsharp
direction = script[scriptIndex % scriptLength]
if movement attempt resolves then
    scriptIndex = scriptIndex + 1
```

The index advances after either a committed move or a collision rejection, but
not on ticks spent accumulating credit.

### General AI

The current reference controller is deterministic:

1. Select the nearest hostile by Chebyshev distance between square footprints.
2. Break equal-distance ties by unit identifier.
3. Use the unit's typed attack profile when the target is in range and the
   delivery path is clear. Riflemen currently fire an eight-cell, twelve-damage
   projectile; trolls, orcs, and goblins use one-cell melee attacks.
4. Otherwise plan a minimum-cost collision-free path to a position from which
   the attack can resolve.
5. If no complete attack route exists, choose the reachable position that
   minimizes distance to the target. This makes units approach a closed breach
   or other obstruction and stop at its near side instead of remaining at
   their spawn point.
6. Accumulate movement credit and commit at most the next adjacent path step,
   keeping that segment stable until it resolves.
7. Hold if no reachable approach or hostile exists.

This is a bundled test policy. It is not an external service, a language model,
or the final player-AI contract.

## Tick order

Automatic execution visits living units in ascending identifier order. Each
unit observes changes committed by earlier units in the same tick.

```fsharp
for unitId in livingUnitIds |> List.sort do
    world <- executeController unitId world
```

The editor increments the tick once after all eligible units execute.

## Layers, validation, and lifecycle

Terrain, semantic edges, units, and document/grid presentation each expose
**Visible**, **Dimmed**, **Hidden**, and **Locked** controls. Dimmed and hidden
change only the SVG projection. Locked prevents commands in that editing
domain. None of these authoring controls enter `MapDefinition`, the `SIR-MAP`
document, or its revision digest. Hidden terrain, edges, and units still
participate in complete-document validation and Simulator execution.

Validation returns stable, sorted codes such as `MAP-DIMENSIONS`,
`TERRAIN-OUTSIDE`, `EDGE-GAP`, `EDGE-OVERLAP`, `UNIT-IDENTITY`, and
`UNIT-PLACEMENT`. The issues panel is an ordinary accessible HTML region with
polite current-issue announcements and previous/next buttons. The same
navigation is available with `[` and `]`. The current issue is mirrored in a
non-interactive SVG validation overlay; the HTML issues panel remains the
assistive-technology authority.

Shrinking a map first produces an exact loss preview for terrain cells, edges,
units, and regions. **Confirm** applies the filtered document as one validated revision;
**Cancel** preserves the complete prior revision. Clear uses the same explicit
confirmation path. Import parses and validates the entire input before a
single `ReplaceDocument` command, so malformed input cannot partially replace
the map. Export remains the deterministic canonical `SIR-MAP 3` ordering
described below.

The browser debounces local autosave for 500 ms. On startup, a different valid
local draft produces an explicit **Recover draft** or **Discard draft** choice;
the editor never silently replaces the loaded map. Recovery records the source
digest in revision lifecycle state. Invalid autosaves are ignored.

Map name, named camera views, current authoring revision identity, and the
deterministic SVG thumbnail are `MapAuthoringMetadata`. They are intentionally
outside `MapDefinition`: changing any of them leaves canonical export and
revision digest unchanged. Thumbnail generation is a safe presentation
projection over sorted canonical terrain and units, not serialized DOM.

The deterministic
`tests/SIR.Client.Tests/fixtures/map-editor-milestone-6-lifecycle.txt` fixture
freezes layer ordering, hidden-layer validation/simulation, lock enforcement,
issue navigation, resize loss, atomic import, recovery choice, metadata
isolation, thumbnail generation, and clear confirmation.

## File format

The export format is line-oriented UTF-8:

```text
SIR-MAP 3
size 12 8
terrain 5 3 objective
edge 6 2 south door closed
zone 1 objective rectangle 4 2 3 2
zone 2 deployment blue polygon 0,0 4,0 2,3
unit 1 blue rifleman 1 1 2 12 12 manual - N N
unit 2 blue medic 1 5 2 12 12 scripted E,E,N N W
unit 3 red goblin 9 1 1 12 12 general - S S
```

The final two unit fields are body facing and attention. Their codes are the
closed `N, NE, E, SE, S, SW, W, NW` set shared with simulation and replay.
Exports sort terrain, edges, zones, and units. Import accepts versions 1, 2,
and 3, validates the complete document, and always exports canonical version 3.

## Authority

The editor is a browser sandbox. It projects map state into the shared SVG
battlefield but does not host an authoritative match, execute player WASM, or
grant replay verification. Exported maps are design inputs, not accepted match
state.

## Qualification evidence

The automated release gate executes the accepted task path through the real
pure editor model: create and resize a 24×16 map, paint and undo/redo a rough
rectangle, place canonical goblin/orc/troll footprints, add deployment
geometry, author a wall run, convert and open a door, save and canonically
reload, recover a newer draft, and hand one immutable revision to the
simulator. It also tests v1 migration, malformed and oversized documents,
bounded clipboard behavior, hostile interchange text, executable SVG
rejection, raster signature mismatch, and oversized assets.

A representative dense 40×40 qualification document contains 1,600 terrain
records, 3,120 semantic edges, 200 units, and 200 regions. The test enforces
the design budgets directly: preview below 8 ms p95, pan/zoom below one 60 Hz
frame, command validation below 16 ms p95, full validation below 100 ms p95,
undo/redo below 50 ms p95, import and export below 250 ms p95 each, and fewer
than 8,000 estimated interactive nodes. The evidence run on 2026-07-29 measured
2.385 ms preview, effectively zero pure pan/zoom work, 2.293 ms command
validation, 2.844 ms full validation, 12.807 ms undo/redo, 12.705 ms import,
0.982 ms export, and 7,136 estimated nodes. Runtime output remains the
authoritative measurement because hardware and scheduling vary.

`scripts/test-map-editor-qualification.mjs` audits the production browser
structure for keyboard reachability, accessible names, SVG title/description
and roving semantic focus, the HTML object-list and issues alternatives,
touch-action ownership, forced-colors support, 400% narrow-layout collapse,
reduced motion, and 44 CSS-pixel target rules. Pure workspace tests exercise
two-pointer pinch/pan and capture cleanup. The same gate verifies hashes for
seven production-rendered review pairs under
`docs/assets/map-editor-review/`: terrain, edges, units, zones, local
background, validation, and simulator handoff.

Automated conformance is not evidence that a new human completed the workflow
in under five minutes or that a particular screen reader and browser pair
worked comfortably. Those claims require the reproducible assisted protocol in
[Map Editor Human Qualification](map-editor-qualification.md). Until a dated
human session is recorded there, human usability and real assistive-technology
qualification remain release blockers even though the automated gate passes.
