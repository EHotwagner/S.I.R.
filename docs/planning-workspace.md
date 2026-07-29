---
title: Coordinated Planning Workspace
category: architecture
categoryindex: 3
index: 8
status: proposed
decision-status: implemented
---

# Coordinated Planning Workspace

The browser planning workspace authors coordinated intent against an immutable
map revision. It is a client of the retained simulator worker protocol, not a
second simulation engine.

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
  label;
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
are discarded before they can update the active workspace.

## Authoring surface

The workspace provides:

- a selectable roster;
- battlefield route waypoints;
- body-facing and attention direction controls;
- stance, hold, point-engagement, and synchronization commands;
- per-unit timeline lanes and selected-command removal;
- validation issue navigation with buttons and bracket-key shortcuts; and
- validation, intent-only preview, and commit actions sent through the real
  retained worker.

All pointer targets are native buttons and therefore activate with Enter or
Space. Inspector controls provide an equivalent for battlefield waypoint and
direction operations. Tool shortcuts are `R`, `F`, `A`, `S`, `H`, `E`, and
`M`; undo and redo use the platform `Ctrl`/`Command` conventions.

## Accessibility and responsive behavior

Interactive planning targets have a minimum 44-by-44 CSS-pixel hit area.
The layout collapses from three columns to one below 48 rem and therefore
remains reflowed at 400% zoom without a page-wide fixed canvas. Roster and
battlefield collections scroll within their labelled regions. The global
reduced-motion rule removes transitions and animation, and forced-colors mode
preserves panel, lane, focus, and control boundaries using system colors.

## Deterministic review evidence

`SIR-PLANNING-REVIEW 1` exports the pinned map identity, authored revision and
digest, labelled predicted revision, accepted revision, committed
revision/tick, conflict count, and canonical command lines. The same model
renders this artifact in the workspace and downloads it as
`sir-planning-review.sir-planning-review`.

The client qualification authors a representative command set across the
intended 200-unit roster, verifies exact undo/redo identity, keeps all four
state channels distinct, and asserts the worker document remains below its
262,144-byte bound. Browser smoke verifies the production Planner mounts all
five panes, authors through a native grid button, restores the route through
undo/redo, and sends initialization through the simulator worker boundary.
