---
title: Editor and Simulator Modal Input Proposal
category: Engineering
categoryindex: 6
index: 9
status: proposed
decision-status: proposed
document-type: design-proposal
version: "0.2"
last-updated: 2026-07-30
description: Apply the archived keyboard-input algebra to the Editor and Simulator with a state-first modal display and optional context-sensitive input help.
related:
  - docs/map-editor.md
  - docs/interactive-rules-lab.md
  - docs/keyboardInput/README.md
  - docs/keyboardInput/context/design-and-controls.md
---

# Editor and Simulator Modal Input Proposal

S.I.R should treat keyboard input in the Editor and Simulator as a
context-sensitive command language. The interface should always present the
current modal state in a compact status strip. The inputs valid in that exact
state should be available through an optional disclosure, generated from the
same binding catalog used for dispatch. Existing editor and simulator state
remains authoritative; this proposal does not introduce a second mode model.

The complete proposed key map, submode transitions, commit/cancel rules, and
conflict matrix are specified in the
[Editor and Simulator Modal Key Vocabulary](editor-simulator-modal-key-vocabulary.md).

## Status

This is a proposal, not an implemented contract. Existing shortcuts and
pointer behavior remain authoritative until the migration and its qualification
tests land.

## Problem

The current application already behaves modally, but expresses that behavior in
several places:

- `App.update` contains separate workspace-specific `KeyPressed` branches;
- `MapEditorState.Tool` selects the active editor tool;
- `MapEditorState.Gesture` records an in-progress rectangle, line, move,
  selection, terrain, or edge operation;
- `EditorToolPanel` records the active authoring domain;
- `EditorSpacePressed` acts as a temporary held pan mode;
- `SimulatorHandoff.IsRunning` distinguishes paused and running simulation;
- `SimulatorHandoff.PreviewDestination` distinguishes ordinary inspection from
  a pending movement preview;
- toolbar labels, documentation tables, and keyboard-help prose repeat parts of
  the binding information.

This creates an implicit modal system without a single projection that can
answer two basic user questions:

1. What state is the interface in now?
2. What can I do from this state?

A static shortcut sheet cannot answer the second question accurately. For
example, `Enter` may begin an operation, commit an operation, accept a simulator
preview, or activate a focused native control depending on context.

## Proposed experience

### Always show the current modal state

The primary input UI is a compact, always-visible status strip attached to the
bottom of the Editor or Simulator map stage. It belongs to the document layout;
it must not cover map content.

The left side is visually dominant:

```text
EDITOR / TERRAIN / RECTANGLE
Defining area — anchor B4, cursor D7
```

or:

```text
SIMULATOR / ROUTE PREVIEW
Unit 3 → F6 — route clear, 4,000 mm
```

The right side contains a secondary disclosure:

```text
[ Inputs · ? ]
```

The strip should show state, not a permanently expanded wall of shortcut
badges. A user should be able to understand the active context without learning
the keyboard scheme.

### Make possible inputs optional

Activating `Inputs` or pressing `?` opens a context panel listing only inputs
that are meaningful in the current modal state. The panel closes through its
button, `Escape`, workspace change, or loss of application focus.

Example for an active terrain rectangle:

| Input | Meaning now |
|---|---|
| Arrow keys | Move the keyboard cursor |
| `Shift` + Arrow keys | Extend the rectangle preview |
| `Enter` | Commit the rectangle |
| `Escape` | Cancel the rectangle |
| Pointer release | Commit the current drag |

Example for a simulator route preview:

| Input | Meaning now |
|---|---|
| Arrow keys | Move the preview destination |
| `Enter` | Commit the route |
| `Escape` | Discard the preview |
| `Space` or `K` | Start or pause the simulator |
| `F2` | Show or hide simulator controls |

The expanded panel is a projection of the live resolver. It is not separately
authored help text.

### Responsive form

On a wide viewport, the possible-input panel opens immediately above the status
strip and remains constrained to the map stage. At narrow widths or 400% zoom,
it becomes an in-flow disclosure below the current-state summary. It must never
require horizontal page scrolling.

## Modal hierarchy

The current context is projected as a stack from durable application state.
Lower entries provide general behavior; higher entries refine or temporarily
override it.

```text
workspace
  └─ domain or simulator lifecycle
       └─ active tool
            └─ active gesture/preview
                 └─ held layer or popup
```

The proposed precedence, highest first, is:

1. Native text-entry and platform/browser reservations.
2. An open input-help popup.
3. A held layer such as Editor pan.
4. An active editor gesture or simulator preview.
5. The selected editor tool or simulator run state.
6. The active workspace's global commands.

Only the highest matching binding executes. Catalog validation must reject two
bindings with the same gesture and equal precedence in an overlapping context.

### Editor contexts

The first implementation should project at least:

- `Editor / Select`;
- `Editor / Terrain / Pencil`;
- `Editor / Terrain / Rectangle`;
- `Editor / Terrain / Line`;
- `Editor / Terrain / Flood fill`;
- `Editor / Terrain / Eyedropper`;
- `Editor / Terrain / Erase`;
- `Editor / Units / Place`;
- `Editor / Units / Move preview`;
- `Editor / Edges`;
- `Editor / Edges / Polyline`;
- `Editor / Zones`;
- `Editor / Document`;
- `Editor / Box selection`;
- `Editor / Pan held`;
- `Editor / Destructive confirmation`.

The detail line should expose the useful parameters already held by the model:
selected terrain, brush size, anchor and cursor, staged segment count, selected
unit count, and pending destructive action.

### Simulator contexts

The first implementation should project at least:

- `Simulator / Paused`;
- `Simulator / Running`;
- `Simulator / Route preview`;
- `Simulator / Revision stale`;
- `Simulator / No handoff`.

The detail line should expose the immutable revision identity, current tick,
selected unit, preview destination, and collision/result summary where
available. `Revision stale` is a status qualifier, not a separate simulation
state; it may appear alongside paused or running.

## State model

### Do not duplicate authoritative state

The archived input package maintained its own `ModeStack`. Reproducing that
literally would allow input state to disagree with the editor or simulator.
S.I.R instead needs a derived stack:

```fsharp
type ModalContext =
    | EditorBase
    | EditorDomain of EditorDomain
    | EditorTool of MapEditorTool
    | EditorGesture of EditorGestureKind
    | EditorPanHeld
    | SimulatorPaused
    | SimulatorRunning
    | SimulatorRoutePreview
    | InputHelpPopup

type ModalProjection =
    { Contexts: ModalContext list
      Breadcrumb: string list
      Headline: string
      Detail: string
      PossibleInputs: PossibleInput list }
```

`ModalProjection` is calculated from the existing application model on every
render. It is not stored.

Only genuinely transient input facts may be added to the application model:

```fsharp
type ModalInputSession =
    { HelpExpanded: bool
      HeldKeys: Set<NormalizedKey> }
```

In the first release, `HeldKeys` is needed only to replace
`EditorSpacePressed` with a general held-input representation. A future
multi-step sequence feature may add `PendingSequence`, but it is outside the
initial scope.

## Binding catalog

One binding record must drive resolution, display, conflict validation, and
test enumeration:

```fsharp
type InputPhase =
    | KeyDown
    | KeyUp

type RepeatPolicy =
    | IgnoreRepeat
    | AllowRepeat

type InputGesture =
    { Key: NormalizedKey
      Modifiers: KeyModifiers
      Phase: InputPhase }

type ModalBinding<'command> =
    { Id: string
      Context: ModalContext -> bool
      Gesture: InputGesture
      Label: string
      Group: string
      Repeat: RepeatPolicy
      Command: 'command }
```

The actual implementation may use a closed context selector instead of a
function if that produces clearer equality and conflict diagnostics. The
important contract is that a binding is declarative and inspectable.

The catalog's command should remain semantic. It can wrap the application's
existing action unions:

```fsharp
type ModalCommand =
    | EditorCommand of MapEditorAction
    | EditorWorkspaceCommand of EditorWorkspaceAction
    | EditorPanelCommand of EditorToolPanel
    | SimulatorCommand of SimulatorAction
    | SimulatorPanelCommand of SimulatorToolPanel
    | ToggleInputHelp
```

The web application lowers `ModalCommand` into its existing `Msg` union.
Neither the catalog nor the pure resolver performs browser I/O.

### Availability

A binding can belong to the active mode but still be unavailable. For example,
`Delete selection` is meaningful in Select mode but unavailable when nothing is
selected.

The resolver should distinguish:

```fsharp
type BindingAvailability =
    | Available
    | Unavailable of reason: string
```

The compact possible-input list shows only `Available` entries. An optional
“Why unavailable?” subsection may show disabled commands with their reason,
but unavailable commands must never dispatch.

### Stable command identity

Every binding receives a stable semantic ID such as:

```text
editor.history.undo
editor.tool.terrain.rectangle
editor.gesture.commit
editor.gesture.cancel
editor.camera.pan-held
simulator.run.toggle
simulator.preview.move-east
simulator.preview.commit
```

The ID is independent of its key and visible label. This preserves the useful
mechanism/policy separation from the archived design and leaves room for later
rebinding without making rebinding part of this proposal.

## Resolution rules

The browser edge continues to normalize DOM keyboard events, but it sends a
complete gesture to the resolver:

```text
DOM KeyboardEvent
  → normalized key/modifiers/down-or-up/repeat
  → derive modal stack from current model
  → select highest-precedence available binding
  → ModalCommand
  → existing Elmish Msg
```

Rules:

- Inputs originating in `input`, `textarea`, `select`, or editable content do
  not enter the application resolver.
- Platform editing commands retain precedence in text-entry controls.
- Browser-reserved combinations are not bound.
- Movement and cursor bindings may allow native key repeat.
- toggles, commits, destructive commands, and popup transitions ignore repeated
  key-down events;
- key-up is routed even if focus or mode changed after key-down;
- focus loss clears held layers;
- `Escape` closes the highest transient context first: input help, then a
  gesture/preview, then selection where current policy permits;
- changing workspace clears popup and held input state;
- pointer and touch commands update the same durable model, so the modal display
  changes identically regardless of input device.

## Initial binding policy

The first migration preserves all accepted bindings documented in the
[Map Editor Reference](../map-editor.md). It is a structural migration, not an
opportunity to redesign the key layout.

The only proposed additive binding is:

| Gesture | Command |
|---|---|
| `?` outside text entry | Toggle possible inputs for the current modal state |

If `?` conflicts with a later text or command-entry mode, the focused control
wins and the global help binding is suppressed.

True multi-key sequences and leader keys should be deferred. The retired
system's types represented them, but its resolver did not implement them. S.I.R
should add sequences only in response to a concrete command-density problem and
with explicit prefix/timeout behavior.

## Visual and accessibility contract

The state strip is not merely decorative:

- use a labelled region such as `aria-label="Current input mode"`;
- expose mode changes through a polite live region;
- do not announce cursor coordinates on every repeated key through the mode
  live region—the existing cursor announcement owns that detail;
- render `Inputs` as a native button with `aria-expanded` and
  `aria-controls`;
- render the expanded possible-input set as a semantic list or definition list;
- expose shortcut text visually and through `aria-keyshortcuts` where the
  syntax can represent it;
- retain a visible button, menu, toolbar, inspector, or object-list equivalent
  for every keyboard command;
- preserve forced-colors boundaries, reduced-motion behavior, minimum target
  sizes, and 400% reflow;
- never use color alone to distinguish mode, availability, preview, or error.

The mode announcement should occur only when its semantic headline changes.
Opening the input panel moves focus only when explicitly requested by keyboard;
pointer activation may leave focus on the disclosure button.

## Example projections

| Existing model facts | Main display | Optional inputs |
|---|---|---|
| Editor, `Terrain RectangleTool`, idle | `EDITOR / TERRAIN / RECTANGLE` — Ready to set first corner | Arrows, Shift+Arrows, Enter, Escape, Space+drag, F2 |
| Editor, `TerrainGesture(RectangleTool, B4, D7, …)` | `EDITOR / TERRAIN / RECTANGLE` — Defining area from B4 to D7 | Shift+Arrows extend, Enter commit, Escape cancel |
| Editor, `EdgePolylineGesture(_, 3 segments)` | `EDITOR / EDGES / POLYLINE` — 3 segments staged | Arrows, Enter finish, Escape backtrack/cancel |
| Editor, held Space | `EDITOR / PAN HELD` — Active tool preserved: Rectangle | Pointer drag pan, release Space return |
| Simulator paused, no preview | `SIMULATOR / PAUSED` — Revision 12 at tick 240 | Arrows begin preview, Space/K run, F2 controls |
| Simulator paused with preview | `SIMULATOR / ROUTE PREVIEW` — Unit 3 → F6, route clear | Arrows move, Enter commit, Escape discard |
| Simulator running | `SIMULATOR / RUNNING` — Tick 241 | Space/K pause, F2 controls; preview commit inputs omitted |

## Placement in the codebase

The proposed responsibility split is:

```text
SIR.Client/ModalInput.fs
  pure gesture vocabulary, binding resolution, conflict validation,
  context and possible-input projections

SIR.Client.Web/App.fs
  derive contexts from the complete application model,
  assemble S.I.R commands, lower commands to Msg,
  subscribe to DOM events, render the mode strip

SIR.Client.Web/styles.css
  status strip, expanded context panel, responsive and forced-color rules
```

If `EditorToolPanel` and `SimulatorToolPanel` prevent the catalog from living in
`SIR.Client`, either move those small UI-state unions to the client library or
keep the final `ModalCommand` adapter in `SIR.Client.Web`. Do not move DOM types
into the pure client library.

## Implementation roadmap

The checkboxes below track merged implementation, not work in progress. A
milestone is checked only when all of its child items and exit gate have landed
on the default branch. Milestones proceed in order unless an earlier milestone
explicitly provides the stable contract needed for safe parallel work.

- [ ] **M0 — Characterize the current behavior**
  - [ ] Enumerate every current Editor and Simulator `KeyPressed` branch in
        characterization tests.
  - [ ] Cover text-entry exclusion, modifier precedence, repeated keys,
        `Escape`, `Space` down/up, focus loss, and workspace changes.
  - [ ] Capture the pointer, touch, toolbar, inspector, and object-list commands
        that must remain equivalent to keyboard commands.
  - [ ] Record intentional differences between current behavior and the
        [complete modal vocabulary](editor-simulator-modal-key-vocabulary.md),
        including replacement of immediate `Alt+Arrow` unit movement.
  - **Exit gate:** the existing behavior has a passing, reviewable baseline
    whose failures expose accidental migration regressions.

- [ ] **M1 — Land the pure modal-input kernel**
  - [ ] Add normalized gestures, input phases, repeat policies, modal contexts,
        binding availability, stable command IDs, and semantic commands.
  - [ ] Derive the modal stack from authoritative Editor and Simulator state;
        do not store a competing durable mode stack.
  - [ ] Implement deterministic highest-precedence resolution and explicit
        no-match results.
  - [ ] Add catalog validation for duplicate IDs and equal-precedence gesture
        conflicts in overlapping contexts.
  - [ ] Run the same resolver fixtures under .NET and Fable.
  - **Exit gate:** the pure resolver produces identical commands and conflict
    diagnostics in both runtimes without browser or simulation dependencies.

- [ ] **M2 — Show current state and live possible inputs**
  - [ ] Render the always-visible Editor and Simulator modal-state strip.
  - [ ] Project breadcrumb, headline, detail, and availability from the current
        application model.
  - [ ] Add the collapsed-by-default `Inputs` disclosure and `?` binding.
  - [ ] Render the disclosed rows from the live catalog rather than duplicated
        help prose.
  - [ ] Implement `F2`, `F3`, popup focus behavior, polite mode announcements,
        and narrow/400%-zoom in-flow layout.
  - **Exit gate:** every displayed available input resolves in the displayed
    context, and every resolvable visible command appears in the disclosure.

- [ ] **M3 — Deliver Select, camera, and Terrain keyboard operation**
  - [ ] Add the deterministic keyboard map cursor and object cycling.
  - [ ] Implement single, toggle, box, and select-all routes plus selected-object
        actions.
  - [ ] Implement held-Space pan, arrow-key pan, frame selection, fit map, and
        camera reset without leaking authoring commands through the held layer.
  - [ ] Implement Terrain tool/value/brush selection and pencil, eraser,
        rectangle, line, flood-fill, and eyedropper flows.
  - [ ] Make every preview explicitly commit, reset, or cancel and produce the
        documented undo granularity.
  - **Exit gate:** terrain can be authored and selected entirely by keyboard,
    while pointer and touch routes still dispatch equivalent map actions.

- [ ] **M4 — Deliver complete Unit keyboard operation**
  - [ ] Add deterministic preset browsing, faction paging, filtering, and
        placement arming.
  - [ ] Implement validated footprint previews, repeated placement, alternate
        place-and-return, and invalid-placement explanations.
  - [ ] Implement deterministic unit selection and multiselection.
  - [ ] Replace immediate movement mutation with resettable, cancellable,
        multi-unit movement previews.
  - [ ] Implement copy, paste preview, duplicate, delete, and conditional bulk
        confirmation without allowing key repeat on destructive actions.
  - **Exit gate:** units can be found, placed, selected, moved, duplicated, and
    deleted entirely by keyboard with one undoable command per documented
    commit.

- [ ] **M5 — Deliver Edges, Zones, and Document keyboard operation**
  - [ ] Implement semantic-edge kind/orientation selection, snapped cursor,
        polyline construction, conversion, door toggle, erase, split, and join.
  - [ ] Implement nested region purpose and shape selection.
  - [ ] Implement rectangle and polygon creation plus region move, resize,
        vertex editing, purpose changes, and deletion.
  - [ ] Implement document new, clear, export, import, design bundle, validation
        traversal, and native confirmation/focus handoffs.
  - [ ] Keep text fields, file pickers, selects, and browser-reserved
        combinations outside modal dispatch.
  - **Exit gate:** every Editor command in the complete vocabulary has a
    keyboard route and a visible non-keyboard equivalent.

- [ ] **M6 — Deliver Simulator keyboard operation**
  - [ ] Project paused, running, route-preview, revision-stale, and no-handoff
        qualifiers from simulator state.
  - [ ] Implement unit traversal, deterministic single-step, start/pause, panel
        selection, and sandbox-reset confirmation.
  - [ ] Implement route-preview begin, movement, reset, commit, cancel, and
        run-transition behavior.
  - [ ] Implement controller selection while preserving native script text
        editing.
  - [ ] Prove that input presentation state never enters authoritative
        simulation, replay, map serialization, or public protocol payloads.
  - **Exit gate:** the paused and running Simulator can be operated entirely by
    keyboard with unavailable mutations omitted from live possible inputs.

- [ ] **M7 — Make the catalog authoritative**
  - [ ] Compare catalog resolution with legacy branches across the complete
        characterization corpus.
  - [ ] Route Editor and Simulator keyboard events through the resolver.
  - [ ] Generalize `EditorSpacePressed` into the held-input session and recover
        it on key-up, focus loss, and workspace change.
  - [ ] Remove superseded hand-written keyboard branches and static shortcut
        prose.
  - [ ] Retain compatibility aliases only when they are labelled, tested, and
        assigned a removal milestone.
  - **Exit gate:** one catalog drives dispatch, possible-input display, conflict
    checks, and test enumeration in production.

- [ ] **M8 — Qualify and accept the modal system**
  - [ ] Exercise every vocabulary mode, transition, commit, reset, cancellation,
        conflict, and repeat policy in automated tests.
  - [ ] Extend browser smoke coverage across complete Editor and Simulator
        keyboard workflows.
  - [ ] Verify screen-reader semantics, focus restoration, forced colors,
        reduced motion, touch alternatives, target sizing, and 400% reflow.
  - [ ] Verify no regression in pointer/touch behavior, undo/redo, canonical map
        output, deterministic simulation, or worker transport.
  - [ ] Update `docs/map-editor.md`, mark the proposal accepted, and record any
        intentionally deferred vocabulary.
  - **Exit gate:** all acceptance criteria below pass in CI and the modal
    vocabulary is the documented, supported interaction contract.

## Acceptance criteria

The proposal is implemented when:

1. Editor and Simulator always show a correct current modal-state headline.
2. The possible-input panel is collapsed by default and user-toggleable.
3. Every displayed possible input is executable in the displayed context.
4. Every catalog-resolvable input is present in the expanded projection unless
   deliberately classified as hidden with a tested rationale.
5. One catalog drives dispatch, help, conflict checks, and test enumeration.
6. Existing accepted shortcut behavior is preserved.
7. Text entry, platform commands, pointer/touch alternatives, and native
   control activation retain precedence.
8. Held and popup contexts recover on key-up, focus loss, and workspace change.
9. Identical model plus gesture produces identical command resolution in .NET
   and Fable.
10. No modal-input state enters authoritative simulation, replay, or map
    serialization.

## Consequences

### Benefits

- The interface explains its current behavior instead of requiring shortcut
  memorization.
- Help cannot silently drift from dispatch.
- Modal tools and temporary layers become explicit and testable.
- Future rebinding or leader-key design has stable semantic command IDs to
  build upon.
- Pointer, touch, and keyboard remain different routes into the same model.

### Costs

- Current shortcut branches must be converted into catalog data.
- Context projection needs careful characterization of editor gestures.
- `Escape` precedence becomes an explicit contract and may expose existing
  inconsistencies.
- A generic resolver can become abstract machinery if expanded beyond actual
  S.I.R needs.

## Alternatives considered

### Keep static shortcut prose

Rejected because it cannot express state-dependent meaning and already
duplicates dispatch logic.

### Port the retired `FS.GG.UI.Input` package unchanged

Rejected because it would duplicate editor/simulator state, bring an unused
YAML configuration boundary into the product, and reintroduce incomplete
sequence machinery. S.I.R should preserve its algebraic ideas, not its orphaned
runtime.

### Show all shortcuts permanently

Rejected because the current state should remain the primary information.
Permanent shortcut grids consume map space and become increasingly noisy as
the command set grows.

### Store a separate mutable mode stack

Rejected because tool, gesture, preview, and run state already exist in the
application model. A second stack can drift from the state it is meant to
describe.

### Add multi-key sequences immediately

Deferred. There is no demonstrated command-density problem that requires them,
and prefix ambiguity, timeout, cancellation, display, and accessibility need a
separate decision.

## Relationship to the archive

This proposal adopts the archive's strongest ideas:

- semantic commands independent of physical keys;
- modes, states, held layers, and closed outcomes;
- pure resolution returning commands rather than performing I/O;
- inspectable bindings that can drive a state display;
- explicit conflict diagnostics.

It deliberately changes one architectural choice: the modal stack is projected
from S.I.R's existing Editor and Simulator state rather than becoming a second
runtime authority. See the [keyboard input algebra archive](README.md) for the
historical source, limitations, and provenance.
