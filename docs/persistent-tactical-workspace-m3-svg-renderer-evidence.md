---
title: Persistent Tactical Workspace Milestone 3 Evidence
category: Engineering
categoryindex: 6
index: 15
status: accepted
decision-status: implementation-evidence
document-type: test-evidence
version: "1.0"
last-updated: 2026-07-31
description: Persistent shared SVG mounting, stable scene layers, registry-routed intent, spatial continuity, and browser reference-identity evidence for Milestone 3.
related:
  - docs/2026-07-31-0840-vscode-style-persistent-tactical-workspace-design-report.md
  - docs/persistent-tactical-workspace-m2-scene-projection-evidence.md
---

# Persistent Tactical Workspace Milestone 3 Evidence

Milestone 3 mounts the renderer boundary that later modality migrations use.
It does not claim the Editor, Planner, Simulator, or Review parity work assigned
to Milestones 4–7.

## One retained production work surface

`svg#persistent-tactical-svg` is the first and invariant child work surface of
`#tactical-battlefield-viewport`. The SVG is created outside the modality
compatibility match and has no modality, revision, tick, panel, or overlay key.
React therefore reconciles the same SVG element while only its projection
attributes and primitive children change.

No modality-specific battlefield, grid, or application root is mounted in the
tactical shell. The labelled `Modality-specific compatibility tools`
disclosure contains only non-workscreen migration guidance and a
registry-routed command-help control. Opening and closing it cannot introduce a
second workscreen or application landmark. Owner-specific parity remains
assigned to Milestones 4–7.

Substantial owner capability is not deferred with the drawing roots. Existing
non-workscreen controls are mounted below the persistent shell frame:

| Owner | Capability retained at M3 | Excluded root |
| --- | --- | --- |
| Editor | file/import/export menus, undo/redo, tool alternatives, terrain palette and brush controls, layers, validation, inspector, document/background controls, destructive confirmations, modal-state strip, and shared-camera controls | legacy Editor SVG and HTML object grid |
| Plan | roster, authored/predicted/accepted/committed status, timeline lanes, inspector, validation navigation, worker actions, and authoring alternatives | planning battlefield cell grid |
| Simulator | run/pause, step, reset confirmation, controller configuration, direction script, movement alternatives, event/runtime diagnostics, samples, modal-state strip, revision status, and shared-camera controls | legacy simulation battlefield and application stage |
| Review | source, transport, status, worker status, and inspector controls | legacy replay battlefield |

Editor-to-Simulator handoff and all Editor/Simulator camera alternatives now
invoke dynamic registry commands. Existing owner panels continue to use their
authoritative owner messages for domain edits, with modal keyboard and shared
scene pointer intent still resolved through registry availability. Milestones
4–7 move these controls into final sidebar placements without making them
unreachable during the migration.

## Stable shared layers

The retained SVG always owns the same seven groups:

| Stable layer | Shared projection input |
| --- | --- |
| camera | pan, zoom, and spatial transform |
| terrain | semantic terrain cells |
| edges | disclosed/authored edge geometry |
| routes | planned or simulator route geometry |
| units | disclosed unit position, footprint, faction, and label |
| selection | semantic primitive selection rings |
| annotations | region, event, and command annotation text |

Primitive children use `ScenePrimitiveId` values as React keys and
`data-primitive-id` values. Layer visibility is applied without removing the
stable layer group. Editor, Plan, Simulate, and accepted Review scenes are
obtained only through the Milestone 2 adapters. All four use the retained
editor workspace camera during this compatibility milestone, preserving pan
and zoom across modality transitions.

The shell owns one semantic unit-selection identity above the adapters.
Owner-specific selections remain separate and authoritative. On a transition,
the shell retains its selection only when that unit exists in the target
projection; otherwise it deterministically reconciles to the target owner’s
valid selection or first visible unit. The renderer adds the valid focused unit
to its presentation-only selection ring without writing it back into another
owner. An unavailable Review projection proves stale selection is filtered,
and a non-default unit survives every applicable directed transition after
accepted Review and Simulator inputs exist.

## Intent boundary

The SVG is a named, keyboard-focusable application region with a title and
description. Unit and cell pointer intents dispatch `InvokeTacticalCommand`;
they never dispatch domain updates directly. Dynamic Editor, Plan, Simulator,
and Review scene commands are present in `activeTacticalRegistry`, and
`InvokeTacticalCommand` rechecks modality membership and
`tacticalCommandAvailable` before execution.

Keyboard events are claimed only when an effective binding exists for an
available command in the same registry. Claimed events are prevented, then
sent through the existing `KeyPressed`/`KeyReleased` resolver. Browser
qualification clicks an availability-qualified shared unit, observes semantic
selection, sends the help binding to the SVG, and observes the registry-owned
supporting overlay.

## Strict DOM identity qualification

Production browser qualification retains the exact JavaScript object references
for the SVG and all seven layer groups. It asserts strict reference equality,
connectivity, containment, exactly one workscreen root, exactly one application
landmark, and absence of connected legacy Editor, Plan, Simulator, or Review
battlefield/grid roots after:

- opening and closing the compatibility disclosure;
- an unavailable Review projection and accepted full-Review input;
- a real Editor-to-Simulator handoff;
- every one of the 12 directed cross-modality transitions and each return;
- panel collapse, move, reorder, hide, and restore;
- timeline playback, expansion, hide, and restore;
- responsive resize; and
- the registry-owned context-help overlay.

Every directed transition uses a control scoped to the modality toolbar,
asserts its pressed state, the viewport modality, and expected
`data-scene-owner`, then compares the retained SVG/layer references, camera
attributes, and valid semantic selection.

## Validation record

- focused production Fable compilation: passed;
- focused production client build and persistent-reference browser smoke:
  passed;
- pointer/keyboard registry availability and spatial-continuity browser
  qualification: passed;
- public .NET Web build: passed;
- deterministic persistent-renderer SVG/PNG review-board regeneration: passed;
- retained Editor terrain/modal/camera/import/validation/layer/destructive
  workflow qualification, including atomic terrain rectangle, edge, and region
  commits with exact undo/redo, native document-input isolation, and help-close
  focus restoration: passed;
- retained Simulator handoff, Step, real Run-to-Pause tick progression and
  stabilization, reset, controller/native-input, route-preview/reset/cancel,
  event, and camera workflow qualification: passed;
- accepted Review edge/event/checkpoint presentation, event transport, and
  worker-backed seek qualification: passed;
- retained Plan shared-registry route authoring, Authored revision,
  undo/redo, worker-validated Accepted channel, worker-committed Committed
  boundary, committed-interval edit immutability, roster/timeline/inspector,
  and validation actions: passed;
- supporting Rules (six runnable scenarios and seven tables) plus Samples
  (three map/simulation and two replay samples, with map-to-Editor return)
  qualification: passed;
- sequential `./fake.sh build -t Dev`, `Test`, and `Verify`: passed;
- `git diff --check`: passed.
