---
title: Unified Tactical Workspace Baseline
category: Engineering
categoryindex: 6
index: 11
status: accepted
decision-status: characterization
document-type: test-baseline
version: "1.0"
last-updated: 2026-07-30
description: Pre-unification ownership, lifecycle, DOM, and keyboard baseline for the four tactical workspaces.
related:
  - docs/unified-tactical-workspace-roadmap.md
---

# Unified Tactical Workspace Baseline

This baseline records the behavior protected while Editor, Planner, Simulator,
and Replay become modalities of one mounted tactical workspace.

| Concern | Authoritative owner | Projection or presentation owner |
|---|---|---|
| Authored map | `MapEditorState.Map` and immutable `MapRevision` | Editor SVG, object list, modal projection |
| Authored plan | `PlanningWorkspaceState` revision/history | Plan lanes, validation, intent overlay |
| Predicted plan | Worker response correlated to the authored revision | Prediction overlay and review artifact |
| Accepted plan | Worker-validated planning revision | Acceptance status and immutable correlation |
| Simulation | `SimulatorHandoff` runtime copied from an immutable map revision | Simulator controls and route preview |
| Committed replay | Replay package, verification identity, and replay shell | Battlefield frame, inspector, transport |
| Camera and selection | Editor/Battlefield presentation state | Mounted battlefield viewport |
| Input help and held keys | Transient application presentation state | State strip and contextual help |

Before unification, `WorkspaceMode` selected four separate render branches:
`EditorWorkspace`, `PlanningWorkspace`, `SimulatorWorkspace`, and
`ReplayWorkspace`. Each branch supplied its own landmark and battlefield
surface. Rules/data and Samples were separate supporting sections and remain
outside the tactical modality set.

Lifecycle transitions preserve these boundaries:

- Editor changes allocate immutable authored revisions and history entries.
- Planning initialization binds to the current editor revision; stale worker
  responses are rejected by correlation identity.
- Simulate creates a disposable `SimulatorHandoff`; runtime ticks and previews
  never rewrite the editor revision.
- Replay loading selects a verified package and projects committed frames
  without entering editor, planning, or simulator state.
- Workspace changes clear popup/held input state and cancel pointer gestures,
  but never implicitly accept, commit, or execute authored content.

The retained DOM landmarks are the application-section navigation, Editor map
stage, planning battlefield and lanes, Simulator map stage, replay battlefield,
mode strip, native tool panels, inspectors, and transport controls. M1 replaces
the four tactical page landmarks with one `Unified tactical workspace`
landmark; the supporting-section navigation remains.

The retained keyboard baseline is executable:

- Editor and Simulator resolve through the authoritative modal catalog.
- Planner retains platform undo/redo, selected-command deletion, issue
  traversal, and tool mnemonics.
- Replay retains play/pause, stepping, event traversal, and cancellation.
- Native text entry and browser/platform reservations precede application
  dispatch.

`UnifiedTacticalWorkspaceQualification` protects the new pure contract:
modality changes preserve the time cursor, scrubbing is projection-only,
predicted segments remain separate, accepted segments can become committed,
and committed intervals reject edits.
