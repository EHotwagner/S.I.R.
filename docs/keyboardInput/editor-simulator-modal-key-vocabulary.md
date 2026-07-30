---
title: Editor and Simulator Modal Key Vocabulary
category: Engineering
categoryindex: 6
index: 10
status: proposed
decision-status: proposed
document-type: interaction-contract
version: "0.1"
last-updated: 2026-07-30
description: Complete modal keyboard vocabulary for map authoring, unit placement and manipulation, semantic edges, regions, and simulator control.
related:
  - docs/keyboardInput/editor-simulator-modal-input-proposal.md
  - docs/keyboardInput/README.md
  - docs/map-editor.md
---

# Editor and Simulator Modal Key Vocabulary

This document defines the proposed keyboard language for fully operating the
S.I.R Editor and Simulator. It is an interaction contract subordinate to the
[modal input architecture proposal](editor-simulator-modal-input-proposal.md).
Every map-authoring operation—including terrain, units, edges, regions,
selection, movement, duplication, and deletion—receives an explicit modal
route. Pointer, touch, menus, toolbars, inspectors, and native form controls
remain equivalent paths into the same commands.

## Design rules

1. **State before shortcut.** The meaning of a key is determined by the
   displayed modal state.
2. **One primary action.** `Enter` begins, advances, or commits the current
   operation.
3. **One unwind action.** `Escape` closes or cancels only the highest temporary
   state.
4. **Arrows move the active cursor or preview.** They never silently mutate a
   selected object from a neutral state.
5. **Shift extends or toggles.** `Shift` + Arrow extends geometry;
   `Shift` + `Enter` performs an explicitly labelled alternate commit.
6. **Space is a held pan layer.** On the map stage it does not place or commit
   content.
7. **Destructive commands remain explicit.** `Delete`/`Backspace` deletes a
   selection; destructive document replacement requires confirmation.
8. **Platform editing conventions remain intact.** `Ctrl`/`Cmd` combinations
   own undo, redo, copy, paste, duplicate, and select all.
9. **Every key has a visible equivalent.** Modal completeness does not remove
   buttons, menus, inspectors, or the object-list route.
10. **The live catalog is the documentation.** The optional possible-input
    panel renders these bindings from the resolver's catalog.

## Focus boundary

The modal vocabulary is active when the Editor or Simulator map stage—or its
mode strip—owns application focus.

The resolver does not handle keystrokes originating in:

- text inputs, text areas, selects, content-editable regions, or file pickers;
- an open browser or operating-system dialog;
- native buttons while they are handling `Enter` or `Space`;
- browser-reserved key combinations.

Workspace mode keys may be recognized outside the map stage when focus is on
non-text application chrome. Cursor movement, geometry construction, and
preview commands require map-stage ownership.

## Key notation

| Notation | Meaning |
|---|---|
| `Enter` | Primary begin/advance/commit action |
| `Shift+Enter` | Alternate commit named by the active mode |
| `Esc` | Unwind the highest temporary mode |
| `← ↑ → ↓` | Move cursor, selection, or preview |
| `Shift+Arrow` | Extend current preview |
| `Space held` | Push temporary pan mode |
| `Space released` | Pop temporary pan mode |
| `⌘/Ctrl` | Platform Command on macOS or Control elsewhere |

Letters are matched using the user's produced key (`KeyboardEvent.key`) so
mnemonics follow the active keyboard layout. The normalized event should retain
`KeyboardEvent.code` for diagnostics and future physical-position rebinding,
but the initial vocabulary does not impose QWERTY physical positions.

## Universal editor bindings

These bindings sit at the bottom of every Editor modal stack. A higher mode may
override an unmodified letter, but not the platform editing combinations.

| Input | Command | Availability |
|---|---|---|
| `?` | Open or close possible inputs | Outside text entry |
| `Esc` | Unwind the highest temporary mode | Always |
| `F2` | Show/hide the active command panel | Always |
| `F3` | Show/hide selected-object inspector | Always |
| `Space` down | Push Pan Held | Map stage |
| `Space` up | Return from Pan Held | Pan Held |
| `⌘/Ctrl+Z` | Undo | Undo history non-empty |
| `⌘/Ctrl+Shift+Z` or `⌘/Ctrl+Y` | Redo | Redo history non-empty |
| `⌘/Ctrl+C` | Copy selected objects | Copyable selection |
| `⌘/Ctrl+V` | Paste as validated preview | Clipboard available |
| `⌘/Ctrl+D` | Duplicate selected objects | Duplicable selection |
| `⌘/Ctrl+A` | Select all in active domain | Domain contains objects |
| `Delete` or `Backspace` | Delete selected objects | Deletable selection |
| `0` | Fit complete map | Map stage |
| `1` | Reset camera to 100% | Except Terrain value selection |
| `F` | Frame selection | Selection non-empty |

`Esc` follows this strict unwind order:

```text
possible-input popup
→ confirmation/picker popup
→ held layer
→ active gesture or preview
→ active submode
→ clear selection
→ no-op
```

It never exits the workspace or discards a document.

## Editor mode entry

The following keys enter or return to a primary Editor domain:

| Input | Resulting mode | Display |
|---|---|---|
| `V` | Select | `EDITOR / SELECT` |
| `T` | Terrain, preserving last terrain tool | `EDITOR / TERRAIN / <TOOL>` |
| `U` | Unit preset browser | `EDITOR / UNITS / BROWSE` |
| `E` | Semantic edges | `EDITOR / EDGES / <KIND>` |
| `Z` | Zones and regions | `EDITOR / ZONES` |
| `M` | Map document | `EDITOR / DOCUMENT` |

Entering a domain opens its contextual panel and retains any compatible
selection. Entering a different domain cancels an incompatible uncommitted
preview before switching.

## Select mode

### Select idle

Display:

```text
EDITOR / SELECT
Cursor C5 — 2 units selected
```

| Input | Command |
|---|---|
| Arrows | Move map cursor one cell |
| `Enter` | Select the topmost object at the cursor |
| `Shift+Enter` | Toggle the topmost object at the cursor in multiselection |
| `Tab` | Move focus into the keyboard object list |
| `N` | Select next object at the cursor |
| `P` | Select previous object at the cursor |
| `B` | Begin box selection at the cursor |
| `A` | Select all in the current object domain |
| `M` | Begin moving selected units |
| `Enter` with an existing selection and no cursor object | Open selected-object actions |
| `Delete` | Delete selected units/region |
| `Esc` | Clear selection |

Objects at one cursor position use a deterministic cycle:

```text
units by ascending ID
→ regions by ascending ID
→ semantic edge
→ terrain cell
```

The current object and cycle position appear in the detail line.

### Box selection

Entry: `B` from Select.

Display:

```text
EDITOR / SELECT / BOX
Anchor B4 — current F8 — 3 units enclosed
```

| Input | Command |
|---|---|
| Arrows | Move current corner |
| `Shift+Arrow` | Equivalent extension; accepted for consistency |
| `Enter` | Replace selection with enclosed units |
| `Shift+Enter` | Add enclosed units to existing selection |
| `Esc` | Cancel and restore prior selection |

### Selected-object actions

Entry: `Enter` on an existing selection when the cursor has no new object, or
the visible Actions button.

Display:

```text
EDITOR / SELECT / ACTIONS
2 units selected
```

| Input | Command |
|---|---|
| `M` | Move selected units |
| `C` | Copy |
| `D` | Duplicate |
| `Delete` | Delete |
| `I` | Open inspector |
| `Esc` | Close actions, preserve selection |

Single-letter copy and duplicate exist only inside this explicit action popup;
the universal forms remain `Ctrl/Cmd+C` and `Ctrl/Cmd+D`.

## Terrain mode

Terrain mode always displays the active authoring tool, terrain value, brush
size, and keyboard cursor.

```text
EDITOR / TERRAIN / PENCIL
Open terrain — 1×1 brush — cursor C5
```

### Tool selection

| Input | Tool |
|---|---|
| `P` | Pencil |
| `R` | Rectangle |
| `L` | Line |
| `G` | Flood fill |
| `I` | Eyedropper |
| `X` | Eraser |

### Terrain value and brush

| Input | Command |
|---|---|
| `1` | Open terrain |
| `2` | Rough terrain |
| `3` | Blocked terrain |
| `4` | Objective terrain |
| `[` | Decrease square brush size |
| `]` | Increase square brush size |

Brush size is clamped to the supported `1..9` range. Value bindings override
the global camera `1` binding while Terrain is active.

### Pencil and eraser

| Input | Command |
|---|---|
| Arrows | Move terrain cursor |
| `Enter` | Apply once at the cursor |
| `Shift+Arrow` | Paint each traversed cell as one repeatable keyboard command |
| `Esc` | Return to Select only when no preview is active |

Keyboard painting produces discrete undo entries unless an explicit keyboard
stroke mode is later introduced. It must never infer a continuous stroke from
key-repeat timing.

### Rectangle and line

Idle:

| Input | Command |
|---|---|
| Arrows | Move terrain cursor |
| `Enter` | Set anchor and begin preview |

Preview:

| Input | Command |
|---|---|
| Arrows | Move current endpoint |
| `Shift+Arrow` | Same movement, retained for cross-tool consistency |
| `Enter` | Commit preview as one undoable command |
| `Backspace` | Return endpoint to anchor |
| `Esc` | Cancel preview |

Display example:

```text
EDITOR / TERRAIN / RECTANGLE
Rough terrain — anchor B4, endpoint D7 — 12 cells
```

### Flood fill

| Input | Command |
|---|---|
| Arrows | Move terrain cursor |
| `Enter` | Fill the connected component at the cursor |
| `Esc` | Return to Select |

Before dispatch, the detail line reports the source terrain under the cursor.
The fill remains one undoable command.

### Eyedropper

| Input | Command |
|---|---|
| Arrows | Move terrain cursor |
| `Enter` | Choose terrain under cursor, then return to the last paint tool |
| `Esc` | Return to the last paint tool without changing terrain selection |

## Unit modes

Unit authoring is a three-stage language:

```text
Browse preset → Place preview → Placed (remain armed)
                         ↘ Select / Move
```

### Unit preset browser

Entry: `U`.

Display:

```text
EDITOR / UNITS / BROWSE
Human / Rifle squad — 2×2 — 100 HP — preset 3 of 18
```

| Input | Command |
|---|---|
| `↑` / `↓` | Previous/next visible preset |
| `PageUp` / `PageDown` | Previous/next faction group |
| `Home` / `End` | First/last visible preset |
| `[` / `]` | Previous/next visible preset |
| `/` | Focus preset search field |
| `Enter` | Arm selected preset and enter Place |
| `Esc` | Return to Select |

Search remains ordinary text entry. `Enter` from the search field accepts its
currently highlighted result; `Escape` leaves the field and returns focus to
the map stage without leaving Unit Browse.

The highlighted preset is transient UI state, not map data:

```fsharp
type UnitPaletteCursor =
    { PresetId: string option
      FactionIndex: int
      ResultIndex: int }
```

Filtering preserves the highlighted preset when it remains visible; otherwise
it selects the first result deterministically.

### Unit placement

Display:

```text
EDITOR / UNITS / PLACE
Rifle squad — 2×2 — preview at E6 — valid
```

| Input | Command |
|---|---|
| Arrows | Move placement cursor and recompute full-footprint preview |
| `[` / `]` | Arm previous/next preset without leaving Place |
| `Enter` | Place one unit and remain in Place |
| `Shift+Enter` | Place one unit and return to Unit Browse |
| `B` | Return to Unit Browse without placing |
| `Esc` | Cancel placement and return to Unit Browse |

An invalid preview does not dispatch. The mode remains active and its detail
line gives the blocking reason: outside map, blocked terrain, blocking edge, or
occupied footprint.

Each placement is one undoable command. Key repeat on `Enter` is ignored.

### Unit selection

Units can be selected in Select mode, from the object list, or by pressing
`Enter` while the Unit cursor is over an existing unit and no placement preset
is armed.

The selected-unit detail includes:

```text
2 units — IDs 3, 7 — mixed class — movement available
```

### Unit movement preview

Entry: `M` with one or more selected units.

Display:

```text
EDITOR / UNITS / MOVE
2 units — offset +2 east, −1 north — valid
```

| Input | Command |
|---|---|
| Arrows | Move the formation preview one cell |
| `Shift+Arrow` | Move five cells, clamped through validation |
| `Enter` | Commit all selected units as one undoable command |
| `Backspace` | Reset preview to original positions |
| `Esc` | Cancel and restore original positions |

Movement never commits directly from neutral Select mode. This replaces the
current `Alt+Arrow` immediate mutation with an inspectable preview. The old
binding may remain as a compatibility alias during migration, but should be
removed once the modal route is accepted.

### Unit deletion

`Delete` or `Backspace` from Select or Unit selection opens a confirmation only
when the selection exceeds a configurable bulk threshold or includes a unit
with authored planning data. Otherwise it executes as one undoable command and
announces the count.

Bulk confirmation:

```text
EDITOR / UNITS / CONFIRM DELETE
Delete 8 units and their attached authoring data?
```

| Input | Command |
|---|---|
| `Enter` | Confirm |
| `Esc` | Cancel |

No `Y` single-key confirmation is used.

## Semantic edge modes

Display:

```text
EDITOR / EDGES / WALL / EAST
Cursor edge C5 east — no edge
```

### Edge kind and orientation

| Input | Command |
|---|---|
| `W` | Wall |
| `D` | Closed door |
| `N` | Window |
| `R` | Rotate orientation East ↔ South |

### Edge cursor and construction

| Input | Command |
|---|---|
| Arrows | Move snapped edge cursor |
| `Enter` | Apply selected edge or begin a wall polyline |
| `Shift+Arrow` | Extend active wall polyline |
| `Enter` while polyline active | Finish polyline |
| `Backspace` while polyline active | Remove last segment |
| `Esc` while polyline active | Cancel remaining polyline |

### Existing-edge actions

| Input | Command |
|---|---|
| `W` | Convert cursor edge to wall |
| `D` | Convert cursor edge to closed door |
| `N` | Convert cursor edge to window |
| `O` | Toggle door open/closed |
| `X` | Erase cursor edge |
| `S` | Split edge run |
| `J` | Join compatible edge run |

The detail line identifies the edge under the cursor, so the user knows whether
`W`, `D`, or `N` will create or convert.

## Zone and region modes

Zone construction uses explicit nested modes rather than assigning every
purpose/shape combination a global shortcut.

### Zone idle

Display:

```text
EDITOR / ZONES
Cursor C5 — no region selected
```

| Input | Command |
|---|---|
| Arrows | Move region cursor |
| `Enter` | Select region under cursor |
| `N` | Begin New Region |
| `M` | Move selected region |
| `R` | Resize selected rectangle |
| `V` | Edit selected polygon vertices |
| `P` | Change selected region purpose |
| `Delete` | Delete selected region |
| `Esc` | Clear region selection, then return to Select |

### New region: purpose

Entry: `N`.

Display:

```text
EDITOR / ZONES / NEW / PURPOSE
Choose region purpose
```

| Input | Purpose |
|---|---|
| `O` | Objective |
| `B` | Blue deployment |
| `R` | Red deployment |
| `Esc` | Cancel |

Choosing a purpose pushes the Shape mode.

### New region: shape

Display:

```text
EDITOR / ZONES / NEW / SHAPE
Blue deployment — choose geometry
```

| Input | Shape |
|---|---|
| `R` | Rectangle |
| `P` | Polygon |
| `Esc` | Return to Purpose |

### Rectangle construction

| Input | Command |
|---|---|
| Arrows | Move cursor |
| `Enter` | Set first corner |
| Arrows after anchor | Move opposite corner |
| `Enter` | Commit rectangle |
| `Backspace` | Clear anchor |
| `Esc` | Cancel geometry and return to Shape |

### Polygon construction

| Input | Command |
|---|---|
| Arrows | Move cursor |
| `Enter` | Add vertex |
| `Backspace` | Remove last vertex |
| `Shift+Enter` | Close and commit polygon when valid |
| `Esc` | Cancel geometry and return to Shape |

The detail line reports vertex count and validity. A polygon cannot commit with
fewer than three valid vertices.

### Move selected region

| Input | Command |
|---|---|
| Arrows | Move preview one cell |
| `Shift+Arrow` | Move preview five cells |
| `Enter` | Commit |
| `Backspace` | Reset preview |
| `Esc` | Cancel |

### Resize selected rectangle

| Input | Command |
|---|---|
| `←` / `→` | Decrease/increase width |
| `↑` / `↓` | Decrease/increase height |
| `Shift+Arrow` | Resize from the opposite origin edge |
| `Enter` | Commit |
| `Backspace` | Reset preview |
| `Esc` | Cancel |

Dimensions never fall below one cell or exceed the map.

### Edit polygon vertices

| Input | Command |
|---|---|
| `[` / `]` | Previous/next vertex |
| Arrows | Move active vertex |
| `Shift+Arrow` | Move active vertex five cells |
| `Enter` | Commit all vertex edits |
| `Backspace` | Reset active vertex |
| `Esc` | Cancel all vertex edits |

### Change region purpose

The Purpose popup uses `O`, `B`, and `R` as above. `Enter` commits the
highlighted purpose; `Esc` preserves the existing purpose.

## Document mode

Document mode exposes commands while leaving detailed values to native,
labelled controls.

Display:

```text
EDITOR / DOCUMENT
Map “Crossing” — revision 12 — dirty
```

| Input | Command |
|---|---|
| `N` | Request new map |
| `C` | Request clear map |
| `S` | Save/export canonical map |
| `I` | Open import file picker |
| `B` | Export repository design bundle |
| `L` | Focus layer-state controls |
| `G` | Focus local background controls |
| `R` | Focus map dimensions |
| `V` | Focus saved views |
| `[` / `]` | Previous/next validation issue |
| `Esc` | Return to Select |

New, clear, lossy resize, and reviewed imports use an explicit confirmation
mode:

| Input | Command |
|---|---|
| `Tab` / `Shift+Tab` | Move between native confirmation choices |
| `Enter` | Activate focused choice |
| `Esc` | Cancel |

The vocabulary intentionally does not assign a single-letter destructive
confirmation.

## Pan Held

`Space` down pushes Pan Held above every non-text Editor context while
preserving the underlying mode.

Display:

```text
EDITOR / PAN HELD
Underlying mode: Terrain / Rectangle
```

| Input | Command |
|---|---|
| Pointer drag | Pan camera |
| Arrows | Pan camera by a fixed screen-space step |
| `Shift+Arrow` | Pan by a larger step |
| `Space` up | Pop Pan Held and restore underlying context |
| `Esc` | Pop Pan Held |

Focus loss and workspace change also pop Pan Held. No map-authoring command can
execute while it is topmost.

## Simulator vocabulary

Simulator modes are derived from run state and route-preview state.

### Universal Simulator bindings

| Input | Command |
|---|---|
| `?` | Toggle possible inputs |
| `F2` | Show/hide active simulator panel |
| `C` | Controls panel |
| `E` | Events panel |
| `A` | Samples panel |
| `[` / `]` | Previous/next unit |
| `Space` or `K` | Start/pause |
| `.` | Advance exactly one tick while paused |
| `Esc` | Cancel highest preview/popup |

### Simulator paused

Display:

```text
SIMULATOR / PAUSED
Revision 12 — tick 240 — unit 3 selected
```

| Input | Command |
|---|---|
| Arrows | Begin route preview one cell from the selected unit |
| `Enter` | Open selected-unit simulator actions |
| `Space` or `K` | Start simulation |
| `.` | Step one deterministic tick |
| `[` / `]` | Select previous/next unit |
| `R` | Request sandbox reset |

Sandbox reset requires native confirmation because it discards runtime-only
progress.

### Simulator route preview

Display:

```text
SIMULATOR / ROUTE PREVIEW
Unit 3 → F6 — route clear — 4,000 mm
```

| Input | Command |
|---|---|
| Arrows | Move destination one cell |
| `Shift+Arrow` | Move destination five cells |
| `Enter` | Commit route |
| `Backspace` | Return destination to unit origin |
| `Esc` | Cancel route preview |
| `Space` or `K` | Commit nothing; start/pause remains available |

Starting the simulator with a pending preview does not implicitly commit it.
The preview remains visible while paused and is cleared when running begins
unless a later decision explicitly allows live route authoring.

### Simulator running

Display:

```text
SIMULATOR / RUNNING
Revision 12 — tick 241 — deterministic execution
```

| Input | Command |
|---|---|
| `Space` or `K` | Pause |
| `C` | Controls panel |
| `E` | Events panel |
| `[` / `]` | Change inspected unit without changing simulation |
| `F2` | Toggle simulator panel |

Route-preview movement, controller mutation, reset, and single-step commands
are unavailable while running. Their omission from possible inputs is the
visible evidence of that restriction.

### Simulator controller popup

Entry: `Enter` from Paused selected-unit actions or `C` from the Controls panel.

Display:

```text
SIMULATOR / CONTROLLER
Unit 3 — Manual
```

| Input | Controller |
|---|---|
| `M` | Manual |
| `S` | Scripted |
| `G` | General AI |
| `Enter` | Commit highlighted choice |
| `Esc` | Cancel |

Script editing remains a native text-entry subflow. Modal keys resume only
after focus leaves the editor.

## Input help popup

`?` pushes Input Help above the current mode without changing the underlying
tool, gesture, selection, or simulator state.

Display:

```text
EDITOR / INPUTS
For Terrain / Rectangle / Preview
```

| Input | Command |
|---|---|
| `?` | Close |
| `Esc` | Close |
| `↑` / `↓` | Move through possible inputs |
| `Home` / `End` | First/last possible input |
| `Enter` | Invoke highlighted available command when safe |

Direct invocation is forbidden for destructive actions and commands requiring
pointer coordinates. Those rows focus their visible equivalent instead.

## Conflict and precedence matrix

Repeated letters are intentional because modes disambiguate them:

| Key | Select | Terrain | Units | Edges | Zones | Document | Simulator |
|---|---|---|---|---|---|---|---|
| `M` | Move selection | — | Move selection | — | Move region | Map document entry is already active | Manual controller in popup |
| `R` | — | Rectangle tool | — | Rotate edge | Red purpose / resize by submode | Focus resize | Reset request while paused |
| `P` | Previous object | Pencil tool | — | — | Polygon / purpose by submode | — | — |
| `D` | Duplicate in Actions | — | — | Door | — | — | — |
| `N` | Next object | — | — | Window | New region | New map | — |
| `1` | Camera 100% | Open terrain | — | — | — | — | — |

Resolver precedence makes each row unambiguous:

```text
popup
> held layer
> active preview/gesture
> domain/tool
> workspace universal
```

The catalog validator must prove that no two available bindings share the same
normalized gesture at the same precedence in overlapping contexts.

## Repeat policy

| Binding family | Repeat |
|---|---|
| Cursor movement | Allowed |
| Camera pan | Allowed |
| Preview movement/resize | Allowed |
| Preset/object/vertex cycling | Allowed |
| Mode entry | Ignored |
| Begin/commit | Ignored |
| Delete/duplicate/paste | Ignored |
| Run toggle/reset | Ignored |
| Popup transition | Ignored |

Repeat is taken from `KeyboardEvent.repeat`, not inferred from timing.

## Required model additions

The fully modal vocabulary requires transient presentation state that does not
belong in `MapDefinition` or `SimulatorHandoff`:

```fsharp
type EditorKeyboardCursor =
    { Cell: EditorCellAddress
      ObjectCycleIndex: int }

type UnitPaletteCursor =
    { PresetId: string option
      FactionIndex: int
      ResultIndex: int }

type RegionDraft =
    | RectangleDraft of purpose: RegionPurpose * anchor: EditorCellAddress * current: EditorCellAddress
    | PolygonDraft of purpose: RegionPurpose * vertices: EditorCellAddress list * current: EditorCellAddress

type ModalInputSession =
    { HelpExpanded: bool
      HeldKeys: Set<NormalizedKey>
      UnitPalette: UnitPaletteCursor
      RegionDraft: RegionDraft option }
```

Unit-move, region-move, resize, and vertex previews should use validated
commands or dedicated preview records and commit through existing
`MapEditorAction` operations. None of these values enter saved map data,
authoritative simulation, replay, or public protocol payloads.

## Command identity groups

Stable IDs should use these families:

```text
editor.mode.*
editor.cursor.*
editor.selection.*
editor.terrain.tool.*
editor.terrain.value.*
editor.terrain.gesture.*
editor.unit.preset.*
editor.unit.place.*
editor.unit.move.*
editor.edge.*
editor.region.create.*
editor.region.edit.*
editor.document.*
editor.camera.*
simulator.lifecycle.*
simulator.selection.*
simulator.preview.*
simulator.controller.*
input.help.*
```

Visible labels may change without changing these IDs. Physical bindings may
later be rebound without changing command identity.

## Qualification matrix

Implementation must test at least:

- every mode entry from Editor Select;
- every tool and terrain-value binding;
- rectangle and line begin, extend, commit, reset, and cancel;
- unit preset traversal, filtered traversal, placement, repeated placement,
  invalid placement, and exit;
- selected-unit move preview, bulk move, commit, reset, cancel, duplicate, and
  delete;
- every edge kind, orientation, conversion, door toggle, erase, split, join,
  polyline backtrack, finish, and cancel;
- region purpose and shape nesting, rectangle and polygon construction, move,
  resize, vertex edit, purpose change, and delete;
- document confirmation and text/file-control focus exclusion;
- held pan key-down, key-up, focus-loss, and workspace-change recovery;
- simulator paused, preview, running, controller, reset, and panel modes;
- `Escape` at every stack depth;
- repeat-allowed and repeat-forbidden commands;
- deliberate same-precedence conflicts rejected by catalog validation;
- equality between resolvable commands and displayed possible inputs;
- .NET and Fable equality for the same model/context/gesture corpus;
- browser accessibility for the mode strip and possible-input popup.

## Adoption boundary

This vocabulary is complete enough to implement keyboard-first Editor and
Simulator operation. It does not yet define:

- user rebinding;
- multi-key leader sequences;
- internationalized command labels;
- gamepad or switch-device bindings;
- Replay and Planning workspace modal vocabularies;
- text editing inside scripts, map names, or search fields.

Those capabilities can reuse the same semantic command IDs and resolver, but
each needs its own explicit design decision.
