---
title: Coordinated Planning Workspace
category: architecture
categoryindex: 3
index: 8
status: implemented
decision-status: implemented
version: "1.0"
last-updated: 2026-07-31
---

# Coordinated Planning Workspace

Final single-renderer, state-authority, disclosure, timeline, responsive, and
Field Focus acceptance is recorded in the
[M9 acceptance evidence](persistent-tactical-workspace-m9-acceptance-evidence.md).

The browser planning workspace authors coordinated intent against an immutable
map revision. It is a client of the retained simulator worker protocol, not a
second simulation engine.

Plan is now a modality of the
[unified tactical workspace](unified-tactical-workspace.md). Its registered
roster, tools, inspector, validation, and revision panels surround the one
persistent SVG and shared time cursor used by Editor, Simulate, and Review. Commands are
authored at the current editable cursor; moving the cursor alone remains a
projection and never creates a revision.

## Decision-checkpoint review

Before implementation, the inert map/simulator interaction prototype and the
real simulator worker responses were reviewed together. The useful interaction
shape was retained: roster selection, battlefield tools, per-unit timeline
lanes, an inspector, and issue navigation. The data boundary comes from the
worker:

- every request and response carries operation, session, map revision, plan
  revision, and tick correlation;
- validation returns an accepted revision or structured diagnostics;
- preview returns an explicit deterministic, assumption-based, or intent-only
  label and, for the current intent-only contract, canonical authored-intent
  disclosures with no entity or event geometry;
- commit identifies the revision accepted by the simulator session; and
- execution responses identify the current committed tick.

The current worker validates the canonical envelope, bounds, and disclosure
classification. Native `SIR-PLAN 1` command compilation and authoritative
shared-kernel execution remain the Milestone 9 integration boundary.

## State channels

The workspace never substitutes one state channel for another:

| Channel | Source | Invalidated by an authored edit |
|---|---|---|
| Authored | local immutable command revision and semantic digest | replaced by the new authored revision |
| Predicted | labelled worker preview for one exact revision | yes |
| Accepted | successful worker validation revision | yes |
| Committed | simulator-session revision and exact tick | no; it remains historical until a later commit |

Undo and redo restore the exact command collection, revision number, and
digest. A post-commit edit creates a new authored revision while the previous
committed identity stays visible. Revision allocation remains monotonic when an
undo is followed by a different edit, so distinct authored content cannot
reuse an abandoned revision number. Responses for an older authored revision
are discarded before they can update the active workspace. The planner keeps
one explicit pending request and also rejects a wrong operation, correlation
tick, response class, envelope kind/version, a superseded request for the same
revision, or any response when no request is pending.

## Authoring surface

The workspace provides:

- a selectable roster;
- battlefield route waypoints;
- body-facing and attention direction controls;
- stance, hold, point-engagement, and synchronization commands;
- shared authored/predicted/accepted/committed timeline segments and
  selected-command removal;
- validation issue navigation with buttons and bracket-key shortcuts; and
- validation, intent-only preview, and commit actions sent through the real
  retained worker.

All panel targets are native buttons and therefore activate with Enter or
Space. The persistent SVG exposes registry-qualified unit and cell actions;
inspector controls provide an equivalent for waypoint and direction operations.
Effective bindings, including restored defaults or local
rebounds, are shown by the live `?` action panel rather than maintained as a
second static shortcut table.

## Accessibility and responsive behavior

Interactive planning targets have a minimum 44-by-44 CSS-pixel hit area.
At narrow widths the same registered panels become responsive drawers without
changing ownership or remounting the SVG. Roster content scrolls within its
labelled region. The global
reduced-motion rule removes transitions and animation, and forced-colors mode
preserves panel, focus, shared-layer, and control boundaries using system colors.

## Deterministic review evidence

`SIR-PLANNING-REVIEW 1` exports the pinned map identity, authored revision and
digest, labelled predicted revision, accepted revision, committed
revision/tick, conflict count, and canonical command lines. The same model
renders this artifact in the workspace and downloads it as
`sir-planning-review.sir-planning-review`.

The client qualification authors a representative command set across the
intended 200-unit roster, verifies exact undo/redo identity, keeps all four
state channels distinct, and asserts the worker document remains below its
262,144-byte bound. Browser qualification verifies the five registered owners,
all shared planning projections, exact route restoration, stale-response
rejection, Preview → Validate → Commit, committed protection, and the simulator
worker boundary while retaining the one SVG object.
