---
title: Editor and Simulator Current Input Baseline
category: Engineering
categoryindex: 6
index: 11
status: current
decision-status: characterized
document-type: qualification-baseline
version: "1.0"
last-updated: 2026-07-30
description: Review baseline for the Editor and Simulator behavior that predates the modal-input migration.
related:
  - docs/keyboardInput/editor-simulator-modal-input-proposal.md
  - docs/keyboardInput/editor-simulator-modal-key-vocabulary.md
  - docs/map-editor.md
---

# Editor and Simulator Current Input Baseline

This document locks the behavior being migrated by M0 of the
[modal-input proposal](editor-simulator-modal-input-proposal.md). It describes
the application as it behaves before the proposed catalog becomes
authoritative. `CurrentModalInput.fs` is the executable characterization
surface and `CurrentModalInputCharacterization.fs` exercises every branch in
that surface.

This is not the desired key vocabulary. The differences at the end of this
document are intentional migration work, not omissions from the baseline.

## Window-level Editor key dispatch

The window subscription combines Control and Command as `ControlOrMeta`.
Unless a row says otherwise, either letter case is accepted and Shift is
ignored.

| Input | Current command |
|---|---|
| `Ctrl/Cmd+Shift+Z` | Redo |
| `Ctrl/Cmd+Z` | Undo |
| `Ctrl/Cmd+Y` | Redo |
| `Ctrl/Cmd+C` | Copy selected units |
| `Ctrl/Cmd+V` | Paste editor clipboard |
| `Ctrl/Cmd+D` | Duplicate selected units |
| `Ctrl/Cmd+A` | Select all in the active domain |
| `Delete` or `Backspace` without Control/Command | Delete selection |
| `[` / `]` without Control/Command | Previous / next validation issue |
| `Space` down without Control/Command | Set the held-pan flag |
| `Shift+1` / `Shift+2` / `Shift+3` / `Shift+4` | Choose Open / Rough / Blocked / Objective terrain |
| `!` / `@` / `#` / `$` | Choose Open / Rough / Blocked / Objective terrain |
| `0` without Control/Command | Fit the board |
| `1` without Control/Command or Shift | Reset the camera |
| `F` without Control/Command | Frame selection |
| `V` without Control/Command | Choose Select, show the Terrain panel |
| `T` / `U` / `E` / `Z` / `M` without Control/Command | Choose Terrain / Units / Edges / Zones / Document panel |
| `P` / `R` / `L` / `G` / `I` / `X` without Control/Command | Choose Pencil / Rectangle / Line / Flood fill / Eyedropper / Erase |
| `F2` without Control/Command | Toggle the Editor tool panel |
| `F3` without Control/Command | Toggle the Editor inspector |
| `Escape` without Control/Command | Cancel pointer state, then cancel the editor gesture if one exists; otherwise clear selected units |

The order above is precedence. For example, `Ctrl/Cmd+V` pastes rather than
choosing Select, `Ctrl/Cmd+Shift+Z` redoes rather than choosing the Zones
panel, and `Shift+1` chooses Open terrain rather than resetting the camera.
An otherwise unrecognized Control/Command chord does not fall through to its
unmodified command.

## Window-level Simulator key dispatch

| Input | Current command |
|---|---|
| `F2` | Toggle Simulator tool panel |
| Arrow keys | Move the route-preview destination one cell |
| `Enter` | Commit the route preview |
| `Escape` | Reset the route preview |
| `Space`, `K` | Toggle Simulator run/pause |

The Simulator branch currently ignores Control/Command and Shift, so modified
forms dispatch the same commands.

## Event-edge behavior

- Key-down from an HTML `input` (including file input), `textarea`, or `select`
  is excluded before dispatch. Content-editable regions and native buttons are
  not explicitly excluded by the current predicate.
- Key-up is not subject to the text-entry or workspace filter. A literal
  `Space` key-up clears the Editor held-pan flag from any workspace.
- Initial and repeated key-down events dispatch identically for every command,
  including toggles, destructive actions, and commits. The characterization
  adapter now forwards `KeyboardEvent.repeat` so this legacy policy is
  explicit and testable, but deliberately ignores its value.
- Workspace changes clear the held-Space flag and cancel captured Editor
  pointers when leaving the Editor. Changes to Replay, Rules, or Samples also
  cancel an editor gesture.
- There is no window-blur/focus-loss handler. If focus is lost before Space
  key-up, held pan remains set until a later Space key-up or workspace change.
- Editor `Escape` unwinds pointer state and then either the gesture or unit
  selection. It has no input-help context to close. Simulator `Escape` always
  resets the route preview.

## Map-stage keyboard behavior outside `KeyPressed`

The Editor SVG has a local key handler that stops propagation for handled
keys. It duplicates the history/clipboard/select-all, panel, delete, `F2`, and
`F3` routes above and additionally implements:

| Context/input | Current command |
|---|---|
| `Alt+Arrow` with the SVG focused | Immediately mutate selected-unit positions by one cell |
| Terrain tool + Arrow | Move the terrain cursor; Shift extends the active gesture |
| Terrain tool + `Enter` or `Space` | Activate the terrain cursor |
| Edge tool + Arrow | Move the edge cursor; Shift extends the active gesture |
| Edge tool + `Enter` | Activate the edge cursor, or finish an active polyline |
| Edge tool + `Space` | Activate the edge cursor |
| Focused SVG unit + `Enter`/`Space` | Select it; Shift toggles it |
| Focused SVG unit + `Escape` | Clear unit selection |
| Object-list Arrow/Home/End | Move native focus only |
| Object-list unit `Enter` | Select it; Shift toggles it |
| Object-list region `Enter` | Select it |

## Equivalent visible and direct-manipulation routes

The modal migration must keep these routes converging on the same durable
actions. “Pointer” includes mouse, pen, and touch where the Pointer Events
conditions permit the operation.

| Route | Durable commands to preserve |
|---|---|
| Toolbar and menus | Choose tool/domain/terrain; undo, redo, copy, paste, duplicate, delete, select all; fit, actual size, frame selection; toggle panels and inspector; validation traversal |
| Selected-unit HUD | Immediate cardinal `MoveSelected`, duplicate, delete |
| Map unit or object-list unit | Single selection and Shift-toggle selection |
| Object-list region | Region selection |
| Cell list | `ActivateCell`, using the active tool |
| Pointer/touch map activation | Activate cells and edges; begin/extend/commit terrain gestures; begin/extend/commit box selection and unit movement; preview placement |
| Pointer/touch camera | Wheel zoom; middle/right/held-Space drag pan; touch pan; pointer-capture loss cancels active Select/Terrain gestures |
| Terrain/edge panels | Commit/cancel gestures; finish/backtrack edge polyline |
| Region panel and inspector | Create rectangle/polygon regions; select, move, reclassify, and remove regions |
| Unit inspector | Edit selected side, class, size, health, controller, and script; remove selected units |
| Simulator controller panel | Controller/script changes; immediate eight-direction movement; preview movement, commit/reset; run/pause and single step |

These are equivalence requirements at the semantic action boundary. The input
devices need not share event mechanics.

## Intentional differences in the complete vocabulary

The proposed vocabulary deliberately changes or adds the following behavior:

- Replace immediate `Alt+Arrow` `MoveSelected` mutation with a resettable,
  cancellable movement preview and one undoable commit. Neutral Arrow keys
  move a cursor or preview and never silently mutate selected objects.
- Treat Space as a held pan layer on the map stage. It must no longer activate
  Terrain or Edge content, and focus loss must clear the held layer.
- Introduce explicit repeat policy: movement/cursor commands may repeat;
  toggles, commits, destructive commands, and popup transitions must ignore
  repeat.
- Explicitly preserve file-input exclusion and add content-editable regions,
  native control activation, and reserved platform/browser chords to the
  modal-dispatch boundary.
- Route key-up even after a mode change, while clearing held and popup state on
  workspace changes and focus loss.
- Make `Escape` close only the highest transient context: input help, then
  gesture/preview, then selection where policy permits.
- Add `?` for live input help, `F3` for Simulator events, deterministic
  keyboard cursor/object traversal, selection and box-selection routes, camera
  arrows/reset, complete unit placement/editing, complete edge/zone/document
  operation, simulator unit traversal and step/reset commands.
- Gate commands by current context and availability. Today many window-level
  Editor shortcuts dispatch regardless of the selected domain, and Simulator
  modified keys are accepted indiscriminately.
- Generate visible input help and dispatch from one catalog instead of
  retaining the duplicated window and SVG key branches.

Failures in the executable characterization suite indicate a baseline change
that must either be preserved during migration or recorded here as another
intentional difference.
