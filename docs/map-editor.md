---
title: Map Editor Reference
category: Tools & Evidence
categoryindex: 5
index: 2
status: accepted
decision-status: implemented
document-type: reference
version: "1.3"
last-updated: 2026-07-29
description: Map records, terrain, semantic edges, square unit footprints, controller modes, and deterministic execution.
related:
  - docs/interactive-rules-lab.md
  - docs/svg-replay-player.md
  - docs/fable-client-and-documentation.md
---

# Map Editor Reference

The browser application separates full-width **Editor** and **Simulator** tabs.
The editor supports terrain, semantic edges, square units, and versioned map
files. The simulator provides manual control, repeatable scripts, and a bundled
general controller.

Open the [simulator](interactive-rules-lab.md) to use it.

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
- `SIR-MAP 1` remains the portable format until an accepted authoritative
  concept requires a tested successor. Editor-only state does not trigger a
  format bump.

## Map

| Field | Constraint |
|---|---|
| Width and height | 4–40 cells |
| Coordinates | Zero-based integers |
| Terrain | Open, rough, blocked, or objective |
| Edges | East or south edge of a cell |
| Edge kinds | Wall, door, or window |
| Units | Positive identifier, square footprint, current and maximum HP, controller |

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
| `drone` | `observation-drone` | 1×1 |

The human size applies to the initial human personnel classes; `rifleman` is
the palette representative, not a claim that other human classes have different
occupancy. Observation and relay drones share the drone size. A new class does
not inherit a preset by name or image: it must be assigned deliberately.

## Initial input contract

The SVG workspace uses application-level keys only while it has focus.
Otherwise browser and documentation-page keys retain their normal behavior.
`Tab` reaches tools, the inspector, and a parallel object list; it does not
visit every empty cell. A keyboard map cursor and the object list provide the
non-pointer path for canvas commands.

### Pointer and touch gestures

| Intent | Mouse or pen | Touch | Commit and cancellation |
|---|---|---|---|
| Select | Primary click an object | Tap an object | Replaces selection |
| Toggle selection | `Shift` + primary click | Select-mode toggle, then tap | Adds or removes one object |
| Box select | Primary drag from empty workspace | Select-mode drag | Pointer/touch release commits; `Escape` cancels |
| Pan | Middle-drag, right-drag, or hold `Space` while primary-dragging | Two-finger drag | Camera-only; never commits map state |
| Zoom | Wheel or trackpad scroll around pointer | Two-finger pinch around midpoint | Camera-only and bounded |
| Place unit | Move for full-footprint preview, then primary click | Move map cursor, then tap **Place** | One unit per activation; `Escape` cancels preview |
| Paint terrain | Primary drag | One-finger drag in an active paint tool | One drag is one command; release commits |
| Rectangle or line | Primary drag from anchor | Tap anchor, move cursor, tap **Apply** | Release/**Apply** commits; `Escape` cancels |
| Flood fill or eyedrop | Primary click | Tap | Commits one fill or selects one terrain value |
| Draw edge polyline | Primary click/drag across snapped edges | Tap successive snapped edges | Double-click/**Finish** commits; `Escape` removes the last segment, then cancels |
| Move selection | Primary drag selected unit(s) | Tap **Move**, reposition cursor, tap **Apply** | Full route and footprint preview; `Escape` cancels |
| Open object actions | Secondary click or context button | Long-press or context button | Opens the same linear action menu used by keyboard |

Pointer capture belongs to an active drag and must be released on commit,
cancel, lost capture, tool switch, or unmount. Hover is never required.

### Keyboard gestures

| Scope | Keys | Frozen behavior |
|---|---|---|
| Global editor | `Ctrl/Cmd+Z`, `Ctrl/Cmd+Shift+Z` | Undo and redo one committed command |
| Global editor | `Ctrl/Cmd+C`, `Ctrl/Cmd+V`, `Ctrl/Cmd+D` | Copy, paste as a validated preview, and duplicate |
| Global editor | `Delete` or `Backspace` | Delete selection through an undoable command |
| Global editor | `Escape` | Cancel active preview first, otherwise clear selection |
| Tools | `V`, `T`, `U`, `E` | Select the Select, Terrain, Units, or Edges domain |
| Camera | `0`, `1`, `F` | Fit board, reset to 100%, or frame selection |
| Camera | `Space` + pointer drag | Temporarily pan without changing the active tool |
| Object list | Arrow keys, `Home`, `End` | Move list focus without moving map objects |
| Object list | `Enter`; `Shift+Enter` | Select focused object; toggle it in multiselection |
| Map cursor | Arrow keys | Move the cursor one cell or snapped edge |
| Map cursor | `Shift` + Arrow keys | Extend the current box, line, rectangle, or edge preview |
| Map cursor | `Enter` or standalone `Space` | Start/commit the active tool at the cursor |
| Selected units | `Alt` + Arrow keys | Preview and commit a one-cell unit move |
| Edge polyline | `Enter` | Finish the current polyline |
| Edge polyline | `Escape` | Remove the last preview segment; when empty, cancel |

Platform menu bindings win when focus is in a text field. Repeated movement keys
produce separate deterministic one-cell commands. No shortcut depends on
keyboard focus being inside a floating window, and every shortcut also has a
visible button or inspector/object-list equivalent.

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
- middle-drag, right-drag, and `Space` + primary-drag panning;
- two-pointer touch pan and pinch around the touch midpoint;
- board fit (`0`), 100% reset (`1`), and selected-unit framing (`F`);
- viewport resize without changing board coordinates; and
- immediate camera updates under reduced-motion preferences.

Screen coordinates are transformed through the inverse camera before map
activation. Cell hits use the transformed cell coordinate. Edge hits compare
their distance in screen pixels against a nine-pixel tolerance, so the target
does not become harder to acquire as zoom changes. Every accepted edge hit is
normalized to an east or south `MapEdgeDirection` record before dispatch.

An active drag owns pointer capture. Commit, pointer release, lost capture,
`Escape`, reset, and workspace disposal all clear captured pointer state.
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
lowercase SHA-256 digest of the canonical UTF-8 `SIR-MAP 1` document. Camera,
selection, clipboard, gestures, panels, runtime ticks, and animation never
contribute to that digest.

Pointer click replaces the active unit selection. `Shift`-click and
`Shift+Enter` in the object list add or remove one unit. Dragging from empty
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

## Semantic edges

Edges are stored once:

```text
edge <column> <row> <east|south> <wall|door|window> <open|closed>
```

Wall and window edges block movement. Closed doors block movement; open doors
do not. Selecting the same door cycles `closed → open → removed`.

## Controllers

### Manual

A manual unit changes state only after an explicit direction:

```fsharp
nextPosition = currentPosition + directionDelta
```

The move is rejected if the destination is outside the map, blocked terrain,
occupied, or separated by a blocking edge.

### Scripted AI

A script is a comma-separated sequence from:

```text
N, NE, E, SE, S, SW, W, NW
```

On each automatic tick:

```fsharp
direction = script[scriptIndex % scriptLength]
scriptIndex = scriptIndex + 1
```

The index advances even when movement is blocked. This keeps execution
deterministic and prevents an obstruction from changing the subsequent script
phase.

### General AI

The current reference controller is deterministic:

1. Select the nearest hostile by Chebyshev distance between square footprints.
2. Break equal-distance ties by unit identifier.
3. Attack for one damage when adjacent.
4. Otherwise move one step toward the target.
5. Hold if no valid move or hostile exists.

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

## File format

The export format is line-oriented UTF-8:

```text
SIR-MAP 1
size 12 8
terrain 5 3 objective
edge 6 2 south door closed
unit 1 blue rifleman 1 1 2 12 12 manual -
unit 2 blue medic 1 5 2 12 12 scripted E,E,N
unit 3 red goblin 9 1 1 12 12 general -
```

Exports sort terrain, edges, and units. Import validates the version, bounds,
identifiers, health, controller names, scripts, footprints, terrain, and edge
records before replacing the current map.

## Authority

The editor is a browser sandbox. It projects map state into the shared SVG
battlefield but does not host an authoritative match, execute player WASM, or
grant replay verification. Exported maps are design inputs, not accepted match
state.
