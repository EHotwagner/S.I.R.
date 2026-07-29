---
title: VTT-Inspired Map Editor Design Report
category: Tools & Evidence
categoryindex: 5
index: 10
status: in-progress
document-type: timestamped-design-report
version: "1.9"
created-at: 2026-07-29T12:30:56+02:00
last-updated: 2026-07-29
related:
  - docs/map-editor.md
  - docs/interactive-rules-lab.md
  - docs/fable-client-and-documentation.md
  - docs/svg-replay-player.md
  - docs/simulation-core-architecture.md
---

# VTT-Inspired Map Editor Design Report

**Report timestamp:** 2026-07-29 12:30:56 CEST (UTC+02:00)

**Status:** implementation in progress; Milestones 0–7 complete

## Executive decision

Replace the current form-and-grid editor with an SVG-first tactical map
authoring application inspired by Roll20, Battlegrounds: RPG Edition, Fantasy
Grounds Unity, and Foundry Virtual Tabletop. The battlefield occupies the
available application area. A compact left tool rail selects an editing
domain, a contextual palette exposes the tools for that domain, and a
collapsible right inspector edits the current selection. Persistent application
tabs continue to separate **Simulator**, **Editor**, **Replay**, and **Rules and
data**.

The design adopts the interaction strengths of virtual tabletops without
turning S.I.R. into a generic tabletop or image compositor. Authoritative maps
remain deterministic semantic documents. Terrain, edges, units, objectives,
deployment zones, and simulation configuration are data. Background images,
annotations, guides, selection boxes, measurement lines, and animation are
presentation or authoring aids and cannot silently become authoritative rules.

SVG is the selected renderer for this documentation editor, not a temporary
prototype on the way to HTML Canvas or WebGL. S.I.R. already has a normalized
glyph catalog and an SVG replay battlefield. The editor benefits directly from
SVG event targets, pointer capture, focus, `<title>` and `<desc>`, tooltips,
CSS state, animation, reusable definitions, inspectable markup, and sanitized
evidence export. The supported map scale does not justify duplicating those
facilities around an opaque pixel buffer. The production client remains a
separate Babylon.js application and may optimize for a different rendering
problem.

The first implementation target is a fast, reversible editor for square-grid
tactical maps:

- pan and zoom without changing tools;
- click, drag, box-select, and keyboard manipulation;
- brush, rectangle, line, flood-fill, and erase gestures;
- direct unit placement using canonical class presets and square footprints;
- continuous wall drawing with door and window conversion;
- visible layer state, locking, and per-layer interaction;
- contextual selection HUD plus a complete inspector;
- undo, redo, clipboard, duplicate, and deterministic autosave;
- live validation before a command is committed;
- explicit handoff from an immutable map revision to the simulator; and
- keyboard, touch, reduced-motion, and screen-reader alternatives.

## Scope

This report designs the documentation-hosted map editor and its relationship to
the simulator. It covers interaction architecture, state boundaries, document
evolution, accessibility, performance, validation, and delivery sequencing.

It does not design:

- multiplayer collaborative editing;
- a general-purpose raster or vector art package;
- arbitrary user scripts with browser authority;
- a marketplace or remote asset repository;
- the production Babylon.js game client;
- hex or gridless authoritative movement;
- final perception, lighting, or fog rules before those rules exist in the
  shared kernel; or
- replacement of canonical square unit symbols with portrait tokens.

## Research method

Research was performed on 2026-07-29 against official product documentation,
official product pages, and current first-party manuals. The comparison focuses
on map authoring and map operation rather than character sheets, dice, chat,
audio, or commercial content.

The phrase “fantasy battlegrounds” is ambiguous. This report includes both
**Battlegrounds: RPG Edition**, a distinct VTT product, and **Fantasy Grounds
Unity**, a major VTT with an integrated map builder. No product is treated as a
specification. Each is evidence for an interaction pattern that must still fit
S.I.R.’s deterministic architecture.

## Comparative findings

### Roll20

Roll20 separates map background, tokens, GM-only information, foreground art,
and dynamic-lighting geometry into layers. Its documentation states the core
reason directly: layer separation prevents moving the map while intending to
manipulate tokens. It provides direct selection, right-drag panning, drawing,
text, measurement, pins, fog controls, and keyboard shortcuts for frequent
layer changes. Map alignment is a dedicated workflow rather than an incidental
property of image placement.

Useful patterns:

- a compact, persistent vertical toolbar;
- explicit active layer with layer-specific selection;
- direct manipulation on the canvas;
- shortcuts for high-frequency mode changes;
- a dedicated grid-alignment operation;
- GM-only information kept separate from player-visible content; and
- perspective preview as a first-class action.

Limitations to avoid:

- important tools hidden behind hover-only menus;
- semantic walls reduced to drawing colors;
- object properties spread across unrelated menus; and
- subscription or network concepts leaking into a local deterministic editor.

### Battlegrounds: RPG Edition

Battlegrounds emphasizes full-screen maps, high-resolution tokens, and an
interface that hides when it is not needed. Its feature set combines automatic
and manual fog, token states, measurement, drawing, map collections, and saved
views. The GM can view player fog while retaining access to concealed units.

Useful patterns:

- maximize the map and minimize permanent chrome;
- saved camera views for quick navigation;
- a clear GM-versus-player visibility preview;
- manual reveal tools alongside derived visibility;
- presentation presets for token state; and
- map browsing with thumbnails.

Limitations to avoid:

- keyboard behavior that depends on which floating window has focus;
- snap behavior that changes at particular zoom levels;
- hidden state stored only in presentation pixels; and
- full-screen minimalism that removes discoverability.

### Fantasy Grounds Unity

Fantasy Grounds provides three complementary authoring paths: quick maps,
tile stamping, and painting. Tiles inherit a configured grid scale; a placed
tile remains selected for immediate move, resize, or rotation. Painting
supports freehand, line, rectangle, ellipse, image brushes, alpha and color
adjustment, and conversion of paint geometry into line-of-sight walls. Its
line-of-sight workflow separates selection, shape, and semantic type, with
snapping and explicit completion gestures.

Useful patterns:

- presets make a first map possible in a few clicks;
- tool settings remain adjacent to the active tool;
- drag from an asset browser and stamp repeatedly;
- placement previews preserve the last-used scale and orientation;
- line, rectangle, ellipse, and freehand share one gesture grammar;
- paint geometry can seed semantic geometry without making pixels
  authoritative; and
- keyboard modifiers provide coarse, fine, snapped, and unsnapped adjustment.

Limitations to avoid:

- deeply nested mode bars;
- exposing every image adjustment in the default path;
- radial context menus without a linear accessible equivalent; and
- duplicating geometry without a review step.

### Foundry Virtual Tabletop

Foundry treats the canvas as a set of specialized layers: actors, tiles, walls,
lighting, notes, effects, and regions. Each layer supplies its own small tool
palette. Tokens support multiselection, a quick HUD, deeper configuration,
prototype defaults, placed-instance overrides, dimensions in grid units, and
resource/status overlays. Tiles support drag-and-drop, handles, z-order,
locking, visibility, and configured asset grid size. Walls independently
restrict movement, sight, light, and sound, while regions combine shapes with
behaviors. Scene configuration separates grid, background, lighting, and
ambience.

Useful patterns:

- a canvas layer is a coherent editing domain, not merely a z-index;
- prototypes provide reusable defaults while placed instances remain editable;
- single-click HUD actions and a full inspector serve different frequencies;
- objects can be locked, hidden, grouped, and box-selected;
- wall semantics are independent axes rather than colors;
- regions separate geometry from behavior;
- drag movement can preview distance and visibility; and
- the current scene, active scene, and viewed scene are distinct concepts.

Limitations to avoid:

- generic extensibility before S.I.R.’s own semantic model is stable;
- large configuration dialogs for routine changes;
- dozens of layers visible at once;
- arbitrary macros executing as trusted map behavior; and
- visual richness that obscures grid occupancy.

## Synthesis

| Pattern | Evidence | S.I.R. decision |
|---|---|---|
| Dominant editing surface | All four products | Adopt; the SVG battlefield fills the Editor tab |
| Tool rail and contextual palette | Roll20, Foundry, Fantasy Grounds | Adopt; one active domain and one active tool |
| Layer isolation | Roll20, Foundry | Adopt as semantic editing domains with lock and visibility |
| Direct manipulation | All four products | Adopt with command previews and validation |
| Prototype/preset placement | Foundry, Fantasy Grounds | Adopt for canonical unit and terrain presets |
| Quick HUD plus inspector | Foundry, Roll20 | Adopt; HUD for frequent actions, inspector for complete fields |
| Background and tile art | All four products | Presentation-only first; never infer rules from pixels |
| Walls and doors | Roll20, Foundry, Fantasy Grounds | Adopt as semantic edges, not colored drawings |
| Regions | Foundry | Adopt later for objectives, deployment, hazards, and triggers |
| Fog and player preview | All four products | Adopt only after authoritative perception projection exists |
| Saved views | Battlegrounds, Foundry scenes | Adopt as local authoring metadata |
| Generic scripting | Foundry ecosystem | Reject for the documentation editor |
| Arbitrary grids | Roll20, Foundry | Reject initially; S.I.R. remains a square 0.5 m cell grid |

## Product principles

### SVG workspace first

The map is the editor. Panels support the SVG workspace and may collapse; the
map is not a preview inside a form.

### SVG is the documentation renderer

SVG is canonical for the documentation editor, simulator, and replay
battlefield. This is an architectural choice, not only an implementation
default.

- Canonical class glyphs reuse one normalized vector grammar.
- Interactive groups receive pointer, keyboard, focus, hover, and tooltip
  behavior without maintaining a second hit-test object tree.
- `<title>`, `<desc>`, labels, and the parallel object list provide accessible
  identity and state.
- CSS classes express selection, validation, layer visibility, reduced motion,
  and semantic zoom.
- `<defs>`, `<symbol>`, and safe `<use>` references may reduce repetition when
  the accessible name remains on the outer interactive object.
- Deterministic SVG snapshots are inspectable review evidence and the source
  for sanitized PNG export.
- Explanatory animation can attach to semantic objects while committed map
  state remains discrete and authoritative.

HTML Canvas and WebGL are not planned render targets for this editor. Their
principal advantage here would be higher rendering throughput. In exchange,
the application would need replacement systems for object hit testing, focus,
tooltips, accessible naming, semantic inspection, export, and test evidence.
That trade is not justified for a bounded abstract map. Babylon.js owns the
high-fidelity real-client rendering problem and remains independent.

### One gesture grammar

Select, draw, and place tools use consistent phases:

```text
pointer down → preview → pointer move → validated preview → pointer up → command
```

`Escape` cancels the preview. A rejected command leaves the map unchanged and
explains why at the affected geometry.

### Semantic before decorative

An edge is a wall, door, or window because the map document says so. Terrain is
rough because its cell value says so. An image may illustrate these facts, but
no gameplay rule is recovered from color, pixels, filenames, or SVG geometry.

### Reversible by default

Every editor mutation is a command with an inverse or a saved pre-state.
Destructive bulk operations require confirmation and remain undoable within the
current session.

### Canonical size is not visual scale

A unit’s square footprint determines occupancy. Its canonical square class
symbol fits that footprint. Optional art may be inset or hidden, but cannot
change the base. Goblins, orcs, trolls, humans, drones, and future units use the
same rule.

### Editor and simulator share a map, not mutable runtime state

The editor produces a validated immutable map revision. Starting or resetting
the simulator snapshots a chosen revision. Simulation ticks never rewrite the
authored document.

## Experience architecture

### Application tabs

```text
┌ Documentation sidebar ┬ Simulator | Editor | Replay | Rules and data ┐
│                       │                                               │
│                       │              active application               │
│                       │                                               │
└───────────────────────┴───────────────────────────────────────────────┘
```

The **Editor** tab owns authoring. The **Simulator** tab owns controller
assignment, run controls, events, and simulation inspection. Switching tabs
preserves editor selection, camera, and unsaved draft state.

### Desktop editor

```text
┌─ top bar: map name · revision · undo/redo · validate · save · simulate ──────┐
├──────┬──────────────────────────────────────────────────────────┬─────────────┤
│ tool │                                                          │ inspector   │
│ rail │                 tactical SVG workspace                   │ selection   │
│      │                                                          │ properties  │
│      │                                                          │ validation  │
├──────┴──────────────────────────────────────────────────────────┴─────────────┤
│ contextual palette / assets / layers                            status · zoom │
└───────────────────────────────────────────────────────────────────────────────┘
```

The tool rail is icon-and-label, not icon-only. Selecting a domain opens its
contextual palette. The inspector is resizable and collapsible. The lower
palette may become a left drawer after usability testing; its contract matters
more than its exact edge.

### Narrow editor

The SVG workspace remains first. Tools become a horizontally scrollable tab
row.
Layers, assets, and inspector are mutually exclusive bottom sheets. The
selection HUD stays near the selection but never covers its complete
footprint. A persistent status row reports tool, coordinates, zoom, snap, and
validation.

### Tool domains

| Domain | Initial tools | Later tools |
|---|---|---|
| Navigate | Select, box select, pan, measure | saved views, camera rotation |
| Terrain | Pencil, rectangle, line, flood fill, eyedropper, erase | stamp patterns, elevation regions |
| Units | Select, place preset, move, duplicate, delete | formation placement, deployment roster |
| Edges | Wall polyline, door, window, select, erase | split, join, bulk convert |
| Zones | Objective rectangle/polygon, deployment zone | hazards, portals, triggers |
| Notes | Pin, label, measurement guide | linked documentation |
| Map | Resize, validate, import, export | background alignment, revision browser |

Only the first four domains are required for the initial complete editor.
Notes and decorative backgrounds remain authoring metadata until their
document contracts are accepted.

## Core interactions

### Camera

- Mouse wheel or trackpad gesture zooms around the pointer.
- Middle-drag, right-drag, or held `Space` pans from any tool.
- `0` fits the board; `1` returns to 100%; `F` frames the selection.
- Zoom is bounded and never changes authoritative coordinates.
- Saved views store camera position and zoom in local authoring metadata.
- A minimap appears only when the board exceeds the visible viewport by a
  tested threshold.

### Selection

- Click selects the highest-priority object in the active editing domain.
- `Shift` adds or removes from selection.
- Dragging empty SVG space box-selects objects in the active domain.
- Repeated click cycles coincident candidates through a visible chooser.
- `Escape` clears selection or cancels an active gesture.
- Selection handles meet a minimum screen-space target independent of zoom.
- Selection never changes merely because the pointer passes over an object.

### Unit placement and movement

- The unit palette is searchable and grouped by faction and role.
- Each preset shows canonical glyph, class, side, and square footprint.
- Pointer movement previews the entire footprint and every occupied cell.
- Valid previews use a non-color outline and `aria-live` summary.
- Invalid previews identify out-of-bounds, blocked, overlap, or edge conflicts.
- Click commits one unit; click-drag may place a line or formation only in a
  later milestone.
- Dragging a selected unit previews its route, distance, destination, and
  leading-edge collisions.
- Arrow keys move one cell; modifiers support selection movement without
  stealing page navigation when the SVG workspace is unfocused.

### Terrain painting

- Pencil paints a configurable square brush.
- Rectangle and line preview exact affected cells.
- Flood fill operates on contiguous cells of the source terrain.
- Eyedropper selects the terrain under the pointer.
- Erase restores the canonical open-terrain default rather than storing an
  explicit open record.
- A stroke is one undoable command, even when it affects many cells.
- Painting blocked terrain over a unit is rejected before commit.

### Semantic edges

Edges remain stored once on east or south cell boundaries. The editor hides
that normalization:

- wall mode draws a continuous snapped polyline over grid edges;
- pointer direction may be arbitrary, but preview resolves to exact canonical
  edge keys;
- double-click, `Enter`, or switching tools finishes a polyline;
- `Escape` removes the last preview segment, then cancels;
- door and window tools convert an existing wall segment or create the required
  segment atomically;
- open/closed is a property of a door, not a separate drawing;
- selection exposes move-blocking and future perception properties separately;
- gaps, overlaps, duplicate edges, and invalid border edges are linted.

### Context HUD and inspector

The context HUD contains at most six frequent actions:

- move or reselect;
- duplicate;
- rotate/facing when supported;
- side or terrain preset;
- lock;
- delete.

Every HUD action has a keyboard and inspector equivalent. The inspector
contains complete typed properties and applies changes through the same command
path as SVG gestures. Mixed multiselection values display as **Multiple**;
changing one field applies that field to every compatible selected object.

### Clipboard and duplication

Copy serializes a deterministic editor fragment, not HTML or SVG. Paste places
a preview at the pointer or viewport center, assigns new identifiers in stable
order, and validates the group atomically. Duplicate is copy-plus-offset.
External text paste accepts only a versioned S.I.R. fragment envelope.

## Layer model

The editor uses layers as editing domains and visibility controls:

| Layer | Authoritative | Default interaction |
|---|---|---|
| Background reference | No | visible, locked |
| Terrain | Yes | visible, editable in Terrain domain |
| Zones and objectives | Yes when supported | visible, locked initially |
| Semantic edges | Yes | visible, editable in Edges domain |
| Units | Yes | visible, editable in Units domain |
| Notes and guides | No | visible, locked initially |
| Validation | Derived | visible, never directly editable |

Layers can be visible, dimmed, hidden, or locked. Hiding an authoritative layer
does not remove it from validation or simulation. The active domain may
temporarily dim unrelated layers but cannot falsify their existence.

## State and command architecture

### Current boundary

The current `MapDefinition` is already a suitable canonical core:

```fsharp
type MapDefinition =
    { Width: int32
      Height: int32
      Terrain: Map<int32 * int32, MapTerrain>
      Edges: Map<int32 * int32 * MapEdgeDirection, MapEdgeKind * bool>
      Units: Map<int32, EditorUnit>
      NextUnitId: int32 }
```

The current editor directly maps cell activation to `MapEditorAction`. It has
no gesture preview, multiselection, command history, clipboard, layer state, or
camera-aware SVG editing. These belong in editor state, not in authoritative
simulation state.

### Proposed split

```fsharp
type MapRevision =
    { Id: MapRevisionId
      Parent: MapRevisionId option
      Document: MapDefinition
      Digest: MapDigest }

type EditorCommand =
    | PaintCells of MapTerrain * CellAddress array
    | ReplaceEdges of MapEdgeChange array
    | AddUnits of UnitDraft array
    | MoveUnits of UnitMove array
    | UpdateUnits of UnitPatch array
    | RemoveUnits of UnitId array
    | ResizeMap of width: int32 * height: int32
    | ApplyFragment of EditorFragment

type Gesture =
    | Idle
    | BoxSelecting of origin: ScreenPoint * current: ScreenPoint
    | Painting of MapTerrain * Set<CellAddress>
    | DrawingEdges of MapEdgeKind * MapEdgeAddress list
    | PlacingUnits of UnitDraft array
    | MovingUnits of UnitMove array
```

`Gesture` is disposable UI state. `EditorCommand` is deterministic intent.
Applying a command is pure:

```fsharp
validateCommand : MapDefinition -> EditorCommand -> Result<ValidatedCommand, MapIssue list>
applyCommand : ValidatedCommand -> MapDefinition -> MapDefinition
```

Undo stores the prior document or a verified inverse command. The initial
implementation should prefer bounded prior-document snapshots because
`MapDefinition` is immutable and small. A later measured optimization may use
inverse commands.

### Elmish flow

```text
DOM pointer/keyboard event
  → screen-to-cell/edge projection
  → transient gesture preview
  → semantic EditorCommand
  → pure validation
  → immutable MapDefinition
  → revision digest
  → SVG projection
```

DOM event frequency, pointer capture, animation frames, and floating-point
camera values stay in the browser host. Only committed integer addresses and
typed values enter the map document.

## Document format

### Version 1

The existing `SIR-MAP 1` format remains readable and writable until a versioned
successor has complete migration tests. Stable sorting and exact text
round-trips remain required.

### Version 2 trigger

Create `SIR-MAP 2` only when at least one accepted authoritative concept cannot
be represented in version 1, such as deployment zones or typed regions. Do not
bump the format for camera, panel, palette, selection, or local autosave state.

Potential sectioning:

```text
SIR-MAP 2
size 40 24
terrain 5 3 rough
edge 6 2 south door closed
zone 1 objective polygon 8,4 12,4 12,8 8,8
unit 7 red troll 20 10 3 240 240 general -
```

Authoring metadata uses a separate versioned envelope:

```text
SIR-EDITOR 1
map-digest <sha256>
camera 18.5 10.0 1.75
layer background dimmed locked
view command-post 12.0 6.0 2.25
```

An editor metadata file cannot change simulation behavior.

## Validation and feedback

Validation operates at three levels:

1. **Gesture:** can the current preview become a legal command?
2. **Command:** does the atomic change preserve map invariants?
3. **Document:** is the complete map ready to simulate or export?

Issues have stable codes, severity, object addresses, and remediation text:

```fsharp
type MapIssue =
    { Code: MapIssueCode
      Severity: IssueSeverity
      Subject: MapSubject
      Message: string
      SuggestedTool: EditorTool option }
```

The validation layer outlines affected cells, edges, or units. The issues panel
groups errors and warnings, supports next/previous navigation, and never relies
on color alone. **Simulate** is disabled only by errors that make deterministic
execution invalid; warnings remain inspectable.

## Simulator handoff

The top bar displays:

- draft state;
- last saved revision;
- last simulated revision; and
- whether the simulator is stale.

Selecting **Simulate this revision** validates, creates an immutable revision,
switches to the Simulator tab, and resets the sandbox to that revision.
Returning to the Editor preserves later draft changes. The simulator never
pulls uncommitted pointer previews or rewrites editor history.

Controller assignment belongs in Simulator unless it is accepted as reusable
scenario authoring data. The editor may display assigned controllers but
should not duplicate the complete runtime control panel.

## Persistence and recovery

- Explicit export remains the portable source of truth.
- Autosave writes a bounded local draft keyed by document digest.
- Autosave is debounced after committed commands, never during pointer motion.
- Loading a newer local draft requires an explicit recovery choice.
- Import parses into a temporary value, reports all bounded errors, then
  replaces the current draft atomically.
- Clearing, resizing with loss, and importing over a dirty draft require
  confirmation.
- The history budget is bounded by command count and approximate serialized
  bytes.
- No user-provided map or asset is uploaded by the static GitHub Pages host.

## Accessibility

- All tool buttons have visible labels and programmatic names.
- A keyboard user can reach every command without traversing every cell.
- The SVG workspace exposes an application-level keyboard model only while
  focused.
- A list/tree alternative exposes layers, units, edges, and issues.
- Selection and placement changes are announced through a concise live region.
- Focus remains on the invoked tool after commands unless a dialog opens.
- Pointer targets remain at least 44 CSS pixels where practical; small edge
  handles receive larger transparent hit regions.
- Patterns, outlines, labels, and glyphs supplement color.
- Reduced motion disables animated previews without removing state changes.
- Zoom up to 400% does not force two-dimensional page scrolling around fixed
  panels; panels collapse before the SVG workspace becomes unusable.
- Touch supports one-finger selection and two-finger camera gestures without
  making hover necessary.

## Performance budgets

Initial budgets at the supported 40×40 map limit:

| Operation | Target |
|---|---:|
| Pointer preview update | under 8 ms p95 |
| Pan/zoom visual update | 60 Hz on reference desktop, 30 Hz minimum fallback |
| Single command validation | under 16 ms p95 |
| Full document validation | under 100 ms p95 |
| Undo or redo | under 50 ms p95 |
| Map import/export | under 250 ms for maximum supported document |
| Interactive DOM nodes | below 8,000 |

SVG performance is a guardrail, not a renderer-selection gate. The editor must
not render one focusable DOM node per empty cell. Grid lines, terrain runs, and
preview geometry should be batched; pointer events should be delegated at
stable semantic groups; hidden layers may be detached when they are neither
visible nor interactive. If a budget is missed, optimize SVG projection,
grouping, event delegation, and update granularity. Do not introduce an HTML
Canvas or WebGL battlefield into the documentation editor.

## Security and trust

- Imported text is parsed as data; it is never inserted as markup.
- Image references use object URLs with explicit type and size limits.
- SVG imports are not accepted as executable DOM.
- Map notes are plain text with bounded length.
- Clipboard fragments use a strict schema and size limit.
- External asset URLs are not fetched by default.
- Editor output cannot grant replay verification or authoritative-match claims.
- Player-perspective preview uses only a disclosure-safe projection supplied by
  the shared rules boundary.

## Alternatives considered

### Keep the current cell-button grid

Rejected as the target experience. It is testable and accessible but scales
poorly to direct manipulation, panning, multiselection, continuous edges, and
large maps. Its deterministic model and list semantics should be retained as a
fallback and testing surface.

### Embed a general VTT

Rejected. Roll20, Battlegrounds, Fantasy Grounds, and Foundry solve session
hosting, media, chat, character, marketplace, and ruleset problems that do not
belong in S.I.R.’s static documentation client. Integration would weaken the
shared F# model and offline publication boundary.

### Render the documentation editor with HTML Canvas or WebGL

Rejected. Increased rendering throughput is not a controlling requirement for
the bounded, abstract documentation map. Either renderer would discard the
existing SVG glyph and battlefield pipeline and require parallel structures for
events, hit testing, focus, tooltips, accessible descriptions, inspection, and
evidence export. A hybrid SVG overlay would retain most DOM cost while adding
coordinate synchronization and two render trees. The separate Babylon.js
client is the appropriate place for a high-throughput real-time renderer.

### Import Foundry or Universal VTT scenes first

Deferred. Interchange is valuable only after S.I.R.’s semantic model has stable
mappings for walls, doors, terrain, units, regions, and disclosure. A lossy
importer before that point would turn guesses into map rules.

### Make all VTT layers authoritative

Rejected. Art, notes, fog paint, and guides have different trust and replay
requirements from terrain, units, and semantic edges.

## Roadmap

Each milestone must finish with tests, documentation, keyboard coverage, and a
deterministic review fixture. A milestone is complete only when every checkbox
within it is checked.

### Milestone 0 — Research and accepted interaction contract

- [x] Compare Roll20, Battlegrounds: RPG Edition, Fantasy Grounds Unity, and
  Foundry VTT using first-party sources.
- [x] Inventory the current `MapDefinition`, `MapEditorAction`, SVG battlefield,
  import/export, controller, and GitHub Pages boundaries.
- [x] Review and accept, amend, or reject this report.
- [x] Record canonical goblin, orc, troll, human, and drone footprint presets.
- [x] Freeze the initial keyboard and pointer gesture table.

### Milestone 1 — SVG workspace and camera

- [x] Replace the HTML cell-button board with the shared SVG tactical
  battlefield in the Editor tab.
- [x] Add pointer-centered zoom, pan, fit-board, frame-selection, and reset
  camera controls.
- [x] Add screen-to-cell and screen-to-edge hit testing with zoom-independent
  tolerances.
- [x] Add the compact tool rail, contextual palette, status row, and collapsible
  inspector.
- [x] Preserve a non-SVG object-list fallback for keyboard and assistive
  technology.
- [x] Add camera, resize, pointer-capture, touch, and reduced-motion tests.

### Milestone 2 — Command history and selection

- [x] Introduce transient gestures, validated `EditorCommand`, immutable map
  revisions, and stable revision digests.
- [x] Add click, additive, box, and object-list selection.
- [x] Add undo, redo, duplicate, delete, copy, paste, and select-all within the
  active domain.
- [x] Bound history by command count and serialized size.
- [x] Add dirty, saved, simulated, and recovered revision states.
- [x] Prove undo/redo round-trips with property and cross-runtime tests.

### Milestone 3 — Terrain authoring

- [x] Implement pencil, rectangle, line, flood fill, eyedropper, and erase.
- [x] Add configurable integer brush size and deterministic cell previews.
- [x] Treat a complete stroke as one atomic command.
- [x] Reject blocked terrain over occupied footprints before commit.
- [x] Add terrain palette labels, patterns, shortcuts, and screen-reader
  announcements.
- [x] Add maximum-map gesture and validation performance evidence.

### Milestone 4 — Unit palette and direct manipulation

- [x] Add searchable canonical unit presets grouped by faction and role.
- [x] Make preset footprint, glyph, side, HP defaults, and class explicit.
- [x] Implement placement preview, drag movement, multiselection movement, and
  keyboard movement.
- [x] Add a six-action maximum context HUD and complete inspector.
- [x] Fit one canonical square symbol to every square base at every zoom.
- [x] Add formation-safe atomic copy/paste and duplicate behavior.
- [x] Test `1×1`, `2×2`, `3×3`, and maximum supported footprints at borders,
  obstacles, edges, and overlaps.

### Milestone 5 — Semantic edge authoring

- [x] Implement continuous wall polylines with click, double-click, `Enter`, and
  `Escape` completion semantics.
- [x] Add door and window conversion, open/closed editing, erase, split, and
  join.
- [x] Normalize every gesture to one east/south edge record.
- [x] Add gap, duplicate, overlap, border, and leading-side movement linting.
- [x] Add keyboard edge placement and zoom-independent edge hit targets.
- [x] Verify map round-trips preserve exact edge meaning and order.

### Milestone 6 — Layers, validation, and map lifecycle

- [x] Add visible, dimmed, hidden, and locked state for every editing domain.
- [x] Add stable validation codes, a validation overlay, and an issues panel
  with next/previous navigation.
- [x] Add safe resize with loss preview, clear confirmation, atomic import, and
  deterministic export.
- [x] Add debounced local autosave and explicit crash-recovery choice.
- [x] Add map naming, saved views, revision identity, and map thumbnail
  generation as authoring metadata.
- [x] Ensure hidden layers still participate in validation and simulation.

### Milestone 7 — Zones, objectives, and deployment

- [x] Define authoritative rectangle and polygon region geometry.
- [x] Separate region geometry, purpose, and future behavior.
- [x] Implement objective and deployment-zone creation, selection, editing, and
  validation.
- [x] Decide whether the accepted region model requires `SIR-MAP 2`.
- [x] If required, add v1-to-v2 migration, canonical v2 serialization, and
  retained v1 loading.
- [x] Reject arbitrary trusted macros and unversioned behaviors.

### Milestone 8 — Background references and interchange

- [ ] Add local raster background references with type, dimension, and size
  limits.
- [ ] Add lock, opacity, fit, crop, grid offset, and grid-alignment tools.
- [ ] Store background and camera facts outside authoritative map state.
- [ ] Evaluate Universal VTT and selected Foundry/Fantasy Grounds exports
  against explicit semantic mappings.
- [ ] Implement import only for mappings that are deterministic and reviewable.
- [ ] Report every ignored or lossy field before accepting an imported map.

### Milestone 9 — Simulator handoff and perspective preview

- [ ] Add **Simulate this revision** with immutable revision handoff.
- [ ] Display when the simulator is behind the editor draft.
- [ ] Keep runtime ticks and controller effects out of authored history.
- [ ] Add deterministic route, distance, and collision preview.
- [ ] Add player-perspective preview only through the accepted
  disclosure-filtered projection.
- [ ] Add manual and derived visibility overlays only after shared-kernel
  perception rules are available.

### Milestone 10 — Qualification and release

- [ ] Run task-based usability tests for first map, unit deployment, walling,
  correction, import, and simulation handoff.
- [ ] Meet the performance budgets on maximum supported maps.
- [ ] Pass keyboard-only, screen-reader, touch, high-contrast, 400% zoom, and
  reduced-motion audits.
- [ ] Add deterministic SVG/PNG review artifacts for all editor domains.
- [ ] Add migration, recovery, malformed-input, clipboard, and hostile-asset
  tests.
- [ ] Update the living map-editor reference and mark this report implemented.

## Qualification tasks

The release candidate must support these tasks without documentation:

1. Create a 24×16 map and fit it to the viewport.
2. Paint a rough-terrain rectangle, undo it, and redo it.
3. Place goblins, orcs, and a troll using their canonical square footprints.
4. Box-select a group, duplicate it, and resolve an invalid overlap.
5. Draw a room as one wall gesture, convert one segment to a door, and open it.
6. Find and correct every validation error from the issues panel.
7. Save, reload, recover a newer autosave, and obtain the same map digest.
8. Simulate one immutable revision, edit the map, and identify that the
   simulator is stale.
9. Complete the same essential tasks using only the keyboard and object list.

Target measures:

- first valid map in under five minutes for a new user;
- no more than one mode error per qualification task;
- undo recovery from every accidental mutation;
- zero unexplained import data loss;
- zero divergence between map digest before export and after re-import; and
- no authoritative state inferred from pixels or transient UI state.

## Research sources

All sources were accessed 2026-07-29.

### Roll20

- [Layers](https://help.roll20.net/hc/en-us/articles/360039675053-Layers)
- [Toolbar overview](https://help.roll20.net/hc/en-us/articles/360039674753-Toolbar-Overview)
- [Sizing and aligning maps](https://help.roll20.net/hc/en-us/articles/360039243994-Sizing-and-Aligning-Maps)
- [Page toolbar and folders](https://help.roll20.net/hc/en-us/articles/360039675413-Page-Toolbar-Folders)

### Battlegrounds: RPG Edition

- [BRPG in a Nutshell](https://battlegroundsgames.com/battlegrounds-rpg-edition/)
- [BRPG screenshots and fog workflow](https://battlegroundsgames.com/battlegrounds-rpg-edition/screenshots/)
- [BRPG user manual](https://www.battlegroundsgames.com/BRPG%20User%20Manual.pdf)

### Fantasy Grounds Unity

- [Creating maps](https://fantasygroundsunity.atlassian.net/wiki/spaces/FGCP/pages/2400190472/Creating+Maps)
- [Creating maps with tiles](https://fantasygroundsunity.atlassian.net/wiki/spaces/FGCP/pages/996640673/Creating+Maps+with+Tiles)
- [Creating maps with the brush tool](https://fantasygroundsunity.atlassian.net/wiki/spaces/FGCP/pages/996640720/Creating+Maps+with+the+Brush+Tool)
- [Map line-of-sight style guide](https://fantasygroundsunity.atlassian.net/wiki/spaces/FGCP/pages/996640584)
- [Using tokens](https://fantasygroundsunity.atlassian.net/wiki/spaces/FGCP/pages/996640745/Using+Tokens)

### Foundry Virtual Tabletop

- [Canvas layers](https://foundryvtt.com/article/canvas-layers/)
- [Scenes](https://foundryvtt.com/article/scenes/)
- [Tokens](https://foundryvtt.com/article/tokens/)
- [Tiles](https://foundryvtt.com/article/tiles/)
- [Walls](https://foundryvtt.com/article/walls/)
- [Lighting](https://foundryvtt.com/article/lighting/)
- [Scene regions](https://foundryvtt.com/article/scene-regions/)
- [Map notes](https://foundryvtt.com/article/map-notes/)
- [Game controls](https://foundryvtt.com/article/controls/)

## Related S.I.R. documents

- [Map Editor Reference](map-editor.md)
- [Simulator](interactive-rules-lab.md)
- [Fable Client and Documentation Architecture](fable-client-and-documentation.md)
- [SVG Replay and Simulation Player](svg-replay-player.md)
- [Simulation Core Architecture](simulation-core-architecture.md)
