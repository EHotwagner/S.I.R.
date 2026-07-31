---
title: Unified Tactical Workspace Roadmap
category: Engineering
categoryindex: 6
index: 10
status: accepted
decision-status: accepted
document-type: design-decision
version: "1.1"
last-updated: 2026-07-31
description: Unify map authoring, plan authoring, prediction, execution, and replay around one persistent battlefield and one scrub-able timeline.
related:
  - docs/planning-workspace.md
  - docs/svg-replay-player.md
  - docs/keyboardInput/editor-simulator-modal-input-proposal.md
  - docs/2026-07-31-0840-vscode-style-persistent-tactical-workspace-design-report.md
---

# Unified Tactical Workspace Roadmap

## Product decision

S.I.R. will present Editor, Planner, Simulator, and Replay as modalities of one
tactical workspace rather than as separate application pages. The battlefield,
selection, camera, time cursor, and timeline stay mounted. Changing modality
changes the available tools, inspectors, command bindings, and timeline editing
rules without replacing the user's spatial context.

Rules, data, and samples remain supporting application sections. They are not
battlefield modalities.

The completed milestones below record the first unification pass. They do not
complete the corrective persistent-workscreen goal defined by the
[VS Code-Style Persistent Tactical Workspace Design Report](2026-07-31-0840-vscode-style-persistent-tactical-workspace-design-report.md).
That report is the active successor for presentation architecture and release
acceptance.

The workspace has one time model:

- **authored** commands occupy unit lanes over a bounded planning horizon;
- **predicted** state is an explicitly labelled, non-authoritative projection
  of the authored revision at the current time cursor;
- **accepted** state identifies the worker-validated authored revision;
- **committed** state records authoritative execution and becomes immutable
  history; and
- the same time cursor can scrub backward and forward through authored,
  predicted, and committed portions without mutating any of them.

Playback advances the cursor. Scrubbing never commits a plan. Editing an
already committed interval is prohibited; branching or planning begins at the
next editable boundary.

## Interaction precedents

The design borrows interaction structure, not visual styling:

- Autodesk Maya keeps a draggable current-time indicator in a persistent Time
  Slider and exposes keys and bookmarks on that same axis:
  <https://help.autodesk.com/cloudhelp/2024/ENU/Maya-Basics/files/GUID-827ED8CD-C6AA-4495-8B5E-2FC98C8D49EE.htm>.
- 3ds Max workspaces restore tool layouts and can load workspace-specific
  hotkeys while leaving the scene/viewport concept intact:
  <https://help.autodesk.com/cloudhelp/2026/ENU/3DSMax-Basics/files/GUID-CC462067-0788-4EB2-ACE9-D3897085E5F3.htm>.
- Frozen Synapse separates leisurely order authoring from simultaneous turn
  resolution. S.I.R. retains that separation while making time navigation
  continuous in the workspace.
- Laser Squad Nemesis makes the useful distinction between issuing orders,
  testing orders against a non-moving opponent, submitting them, and reviewing
  results with VCR-like playback. S.I.R.'s authored/predicted/accepted/committed
  channels preserve the same disclosure boundary:
  <https://www.gamespot.com/reviews/laser-squad-nemesis-review/1900-6123714/>.

## Non-negotiable command contract

One live command registry is authoritative for:

- keyboard dispatch;
- pointer/menu activation;
- contextual help;
- conflict detection;
- editable bindings;
- accessible shortcut metadata; and
- .NET/Fable parity tests.

Every command has a stable identifier, label, category, applicable contexts,
default gesture, current effective gesture, precedence, and availability
predicate. Context help opened by `?` lists every currently executable action,
including actions without a keyboard binding, and distinguishes pointer-only,
unbound, and rebound actions. It must never be maintained as parallel prose.

Bindings are configurable in a native-controls dialog reachable from the help
panel. Rebinding detects exact and contextual conflicts before saving. Users
can clear a binding, restore one command, restore one modality, or restore all
defaults. Overrides are versioned, validated, stored locally, and imported or
exported as deterministic JSON. Reserved browser/platform and text-entry
gestures cannot be captured silently.

## Target layout

```text
┌ modality + document controls + prediction/validation/commit state ┐
├ roster/outliner ┬──────── persistent battlefield ───────┬ inspector ┤
│ units/layers    │ authored + predicted + actual overlay │ context   │
├─────────────────┴───────────────────────────────────────┴───────────┤
│ transport | time ruler/current-time cursor | per-unit command lanes │
├ contextual state strip ─────────────────────────────── [ Inputs ? ] ┤
```

On narrow viewports the panels become in-flow drawers, but battlefield,
timeline, mode selector, time cursor, and contextual state remain available.

## Mandatory corrective goal — Field Focus

**Field Focus** is the accepted default layout and MUST be achieved before the
corrective redesign is considered complete. The implementation must provide:

- one exact persistent SVG workscreen across Editor, Plan, Simulate, and Review;
- a compact top toolbar rather than page-sized or modality-specific chrome;
- narrow, configurable left and right sidebars that can be moved, reordered,
  collapsed, shown, and hidden;
- a shallow, resizable bottom timeline;
- a workscreen that remains visually and dimensionally dominant with both
  default sidebars open; and
- richer analysis layouts through the same persisted panel system, never
  through another workspace or renderer.

The [interactive Field Focus mockup](assets/persistent-workspace-mockups/index.html)
is the selected visual-density reference. It is not a substitute for
accessibility or browser qualification, but divergence from its spatial
priority requires a new accepted design decision.

- [ ] Complete Milestones 0–9 in the corrective design report.
- [ ] Pass strict SVG reference-identity qualification across modality, panel,
  timeline, overlay, resize, and playback transitions.
- [ ] Pass Field Focus visual review at the reference desktop viewport with
  both default sidebars open.
- [ ] Remove all legacy modality-specific battlefield render paths.

## Milestones

Milestones are sequential. A milestone is complete only when its implementation,
focused tests, documentation, and roadmap evidence are present.

### M0 — Contract and regression characterization

- [x] Record the current Editor, Planner, Simulator, and Replay state ownership,
  lifecycle transitions, DOM landmarks, and keyboard behavior.
- [x] Add regression tests that preserve authored/predicted/accepted/committed
  separation and current Editor/Simulator modal resolution.
- [x] Define unified modality, time-cursor, timeline-segment, and command
  registry contracts without duplicating authoritative simulation state.

### M1 — Persistent tactical shell

- [x] Replace four battlefield workspace pages with one mounted tactical shell.
- [x] Add Editor, Plan, Simulate, and Review modality controls using native
  buttons with pressed state.
- [x] Preserve battlefield camera, selection where valid, and time cursor when
  modality changes.
- [x] Keep Rules, data, and Samples as separate supporting sections.

### M2 — One movable timeline

- [x] Introduce one bounded time ruler and current-time cursor shared by all
  tactical modalities.
- [x] Render per-unit authored commands, predicted availability, committed
  history, validation issues, and event markers on the same axis.
- [x] Support pointer dragging, native range input, direct time entry, stepping,
  Home/End, and play/pause with deterministic clamping.
- [x] Make backward/forward scrubbing projection-only and prove it cannot
  commit, edit history, or alter authoritative ticks.

### M3 — Plan authoring on the battlefield

- [x] Move roster, tools, inspector, validation, and revision controls into
  docked panels around the persistent battlefield.
- [x] Author route, facing, attention, stance, hold, engagement, and
  synchronization commands at the current editable time.
- [x] Select, move in time, and remove timeline commands while preserving exact
  undo/redo identity and monotonic revision allocation.
- [x] Overlay authored routes and intent plus labelled predicted state directly
  on the battlefield.
- [x] Preserve Preview → Validate → Commit and stale-response rejection through
  the retained simulator worker.

### M4 — Unified execution and review

- [x] Use the shared transport and time cursor for predicted playback and
  committed execution/replay.
- [x] Visually distinguish authored, predicted, accepted, and committed ranges.
- [x] Prevent editing committed time and expose the next editable boundary.
- [x] Reconcile selection and viewport without remounting when execution starts,
  pauses, completes, or returns to planning.

### M5 — Configurable command bindings

- [x] Generalize the modal catalog into the live command registry for all four
  tactical modalities.
- [x] Add versioned local binding overrides with deterministic import/export.
- [x] Add capture, conflict diagnosis, clear, restore-command, restore-modality,
  and restore-all flows using native controls.
- [x] Preserve text-entry and browser/platform reservations and require an
  explicit decision before replacing a contextual conflict.
- [x] Add .NET/Fable parity, migration, malformed-storage, and conflict tests.

### M6 — Complete contextual action help

- [x] Make `?` open help in every tactical modality and transient state.
- [x] Project all currently executable registry actions, not only actions with
  shortcuts.
- [x] Show effective gestures, rebound/default status, unbound state, action
  category, and an entry point to binding configuration.
- [x] Close with Escape, restore focus, announce context changes, and emit
  `aria-keyshortcuts` from effective bindings only.
- [x] Prove help and dispatch use the same availability result for every
  qualified context.

### M7 — Responsive, accessible DCC-style workspace

- [x] Meet 44 CSS-pixel pointer targets, 400% reflow, forced-colors,
  reduced-motion, screen-reader landmarks, and keyboard-only operation.
- [x] Keep the timeline and current modal state visible while panels collapse
  into drawers at narrow widths.
- [x] Ensure native descendants do not bubble into battlefield commands and
  pointer capture recovers on blur/cancel.
- [x] Add production-browser tests for mode switching without remount, timeline
  scrubbing, plan editing, rebinding, and contextual help.

### M8 — Acceptance and migration

- [x] Remove superseded top-level Planner/Simulator/Replay page paths and static
  shortcut prose.
- [x] Update user, architecture, planning, replay, map-editor, and keyboard
  documentation to the accepted unified contract.
- [x] Run `npm test`, `npm run build:docs`, browser smoke, and
  `./fake.sh build -t Dev`, `Test`, and `Verify` sequentially.
- [x] Record review evidence, mark this decision accepted, and verify a clean
  production build with no catalog conflicts or accessibility violations.

### Implementation evidence

- `npm test`: passed, including .NET/Fable command-binding parity, production
  build, browser smoke, map-editor accessibility qualification, worker,
  conformance, replay, ABI, and modal-boundary gates.
- `npm run build:docs`: passed, including generated-site browser smoke and
  accessibility verification.
- Explicit browser smoke: passed with persistent-shell modality switching,
  projection-only timeline movement, plan authoring/undo/redo, native binding
  capture and persisted rebound dispatch, and contextual action help.
- `./fake.sh build -t Dev`, `Test`, and `Verify`: passed sequentially.
- Seven deterministic map-editor SVG/PNG review pairs were regenerated from the
  current production bundle.
- Corrective review coverage proves that `#tactical-battlefield-viewport`
  remains the same DOM node across Editor, Plan, Simulate, and Review; the
  shared ruler seeks the loaded Replay projection; and planner-worker Validate
  and Commit responses populate the shared tactical Accepted and Committed
  segments.
- Command-authority coverage clears the Editor panel binding and proves its
  former default no longer dispatches, then rebinds it and proves the effective
  gesture drives both dispatch and help. Plan help is checked for worker,
  roster, and current inspector actions.
- Binding-profile qualification uses a strict parser in both .NET and Fable and
  rejects future schemas, duplicate fields, duplicate command IDs, unknown
  fields, malformed values, and trailing content.
- Review transport availability is evaluated from the active Replay or
  Simulator transport before a command enters dispatch or contextual help, so
  unavailable timeline actions are not advertised as executable.
- The third corrective pass makes unified playback subscribe in Editor and
  Plan as well as Review, synchronizes every Simulator update back to the
  tactical cursor/play state, and labels paused Simulator scrubbing as a
  projection-only operation that leaves its authoritative runtime tick
  unchanged. Browser coverage exercises all three behaviors.
- Every pointer and keyboard command now enters the same availability-checked
  stable-ID invocation boundary, including planning battlefield cells and
  Simulator controller, script, movement, preview, run, step, reset, and panel
  controls. Clearing or rebinding a keyboard gesture does not disable its
  native pointer action. Authoritative worker progress advances the shared
  committed boundary, committed snapshots cannot be undone or redone across
  that boundary, and committed Move/Remove/Undo/Redo pointer controls are
  disabled and proven non-mutating.
- Modal binding conflicts preserve the original context selector, precedence,
  and key phase. .NET/Fable fixtures reject two simultaneously active modal
  commands rebound to one gesture while retaining disjoint and key-up/key-down
  bindings.
- Persisted modal command IDs are checked against the complete qualified
  Editor/Simulator ID vocabulary. A typo is rejected, while a valid command
  from an inactive transient context imports and survives until that context
  becomes active.
- Availability is shared by help, keyboard resolution, pointer invocation, and
  browser-default prevention. Browser coverage proves an unloaded Review
  transport gesture is neither advertised nor default-prevented, then proves
  the same actions appear after a replay is loaded. It also rebinds an
  unavailable running-Simulator command and proves it is absent from help,
  performs no action, and leaves `defaultPrevented = false`, while the rebound
  command's native pointer control remains available.
- Simulator pointer-only controller, script, and movement commands are listed
  in contextual help by stable ID with an unbound/pointer-only label exactly
  while executable; modal IDs already projected by the current context are not
  duplicated.
- Editor and Simulator stage-local default prevention resolve the same adapted
  binding catalog as dispatch. Browser coverage clears or rebinds commands in
  both stages, proving old defaults neither act nor prevent, while effective
  replacement gestures still act and prevent as resolved commands.
- Independent review approved the exact final state after five read-only review
  passes and four corrective implementation passes. The final residual check
  also verified that pointer-only Simulator actions remain in contextual help
  but are not misleadingly exposed as configurable keyboard bindings.

## Review gate

Before merge, an independent reviewer must verify:

1. there is one mounted battlefield and one timeline for all tactical modes;
2. scrubbing is reversible projection and cannot mutate committed state;
3. plan preview remains explicitly non-authoritative;
4. every executable action appears in contextual help;
5. effective bindings drive both dispatch and displayed help;
6. binding overrides survive reload and malformed overrides fail safely;
7. native controls, text entry, browser shortcuts, and accessibility behavior
   remain protected; and
8. all requested build, test, documentation, and browser gates pass.
