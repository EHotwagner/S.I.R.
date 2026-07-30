---
title: Unified Tactical Workspace Roadmap
category: Engineering
categoryindex: 6
index: 10
status: proposed
decision-status: implementation-roadmap
document-type: design-decision
version: "0.1"
last-updated: 2026-07-30
description: Unify map authoring, plan authoring, prediction, execution, and replay around one persistent battlefield and one scrub-able timeline.
related:
  - docs/planning-workspace.md
  - docs/svg-replay-player.md
  - docs/keyboardInput/editor-simulator-modal-input-proposal.md
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

## Milestones

Milestones are sequential. A milestone is complete only when its implementation,
focused tests, documentation, and roadmap evidence are present.

### M0 — Contract and regression characterization

- [ ] Record the current Editor, Planner, Simulator, and Replay state ownership,
  lifecycle transitions, DOM landmarks, and keyboard behavior.
- [ ] Add regression tests that preserve authored/predicted/accepted/committed
  separation and current Editor/Simulator modal resolution.
- [ ] Define unified modality, time-cursor, timeline-segment, and command
  registry contracts without duplicating authoritative simulation state.

### M1 — Persistent tactical shell

- [ ] Replace four battlefield workspace pages with one mounted tactical shell.
- [ ] Add Editor, Plan, Simulate, and Review modality controls using native
  buttons with pressed state.
- [ ] Preserve battlefield camera, selection where valid, and time cursor when
  modality changes.
- [ ] Keep Rules, data, and Samples as separate supporting sections.

### M2 — One movable timeline

- [ ] Introduce one bounded time ruler and current-time cursor shared by all
  tactical modalities.
- [ ] Render per-unit authored commands, predicted availability, committed
  history, validation issues, and event markers on the same axis.
- [ ] Support pointer dragging, native range input, direct time entry, stepping,
  Home/End, and play/pause with deterministic clamping.
- [ ] Make backward/forward scrubbing projection-only and prove it cannot
  commit, edit history, or alter authoritative ticks.

### M3 — Plan authoring on the battlefield

- [ ] Move roster, tools, inspector, validation, and revision controls into
  docked panels around the persistent battlefield.
- [ ] Author route, facing, attention, stance, hold, engagement, and
  synchronization commands at the current editable time.
- [ ] Select, move in time, and remove timeline commands while preserving exact
  undo/redo identity and monotonic revision allocation.
- [ ] Overlay authored routes and intent plus labelled predicted state directly
  on the battlefield.
- [ ] Preserve Preview → Validate → Commit and stale-response rejection through
  the retained simulator worker.

### M4 — Unified execution and review

- [ ] Use the shared transport and time cursor for predicted playback and
  committed execution/replay.
- [ ] Visually distinguish authored, predicted, accepted, and committed ranges.
- [ ] Prevent editing committed time and expose the next editable boundary.
- [ ] Reconcile selection and viewport without remounting when execution starts,
  pauses, completes, or returns to planning.

### M5 — Configurable command bindings

- [ ] Generalize the modal catalog into the live command registry for all four
  tactical modalities.
- [ ] Add versioned local binding overrides with deterministic import/export.
- [ ] Add capture, conflict diagnosis, clear, restore-command, restore-modality,
  and restore-all flows using native controls.
- [ ] Preserve text-entry and browser/platform reservations and require an
  explicit decision before replacing a contextual conflict.
- [ ] Add .NET/Fable parity, migration, malformed-storage, and conflict tests.

### M6 — Complete contextual action help

- [ ] Make `?` open help in every tactical modality and transient state.
- [ ] Project all currently executable registry actions, not only actions with
  shortcuts.
- [ ] Show effective gestures, rebound/default status, unbound state, action
  category, and an entry point to binding configuration.
- [ ] Close with Escape, restore focus, announce context changes, and emit
  `aria-keyshortcuts` from effective bindings only.
- [ ] Prove help and dispatch use the same availability result for every
  qualified context.

### M7 — Responsive, accessible DCC-style workspace

- [ ] Meet 44 CSS-pixel pointer targets, 400% reflow, forced-colors,
  reduced-motion, screen-reader landmarks, and keyboard-only operation.
- [ ] Keep the timeline and current modal state visible while panels collapse
  into drawers at narrow widths.
- [ ] Ensure native descendants do not bubble into battlefield commands and
  pointer capture recovers on blur/cancel.
- [ ] Add production-browser tests for mode switching without remount, timeline
  scrubbing, plan editing, rebinding, and contextual help.

### M8 — Acceptance and migration

- [ ] Remove superseded top-level Planner/Simulator/Replay page paths and static
  shortcut prose.
- [ ] Update user, architecture, planning, replay, map-editor, and keyboard
  documentation to the accepted unified contract.
- [ ] Run `npm test`, `npm run build:docs`, browser smoke, and
  `./fake.sh build -t Dev`, `Test`, and `Verify` sequentially.
- [ ] Record review evidence, mark this decision accepted, and verify a clean
  production build with no catalog conflicts or accessibility violations.

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
