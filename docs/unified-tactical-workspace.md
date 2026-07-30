---
title: Unified Tactical Workspace
category: Tools & Evidence
categoryindex: 5
index: 1
status: accepted
decision-status: implemented
version: "1.0"
last-updated: 2026-07-30
---

# Unified Tactical Workspace

Editor, Plan, Simulate, and Review are modalities of one tactical workspace.
Use the four pressed-state buttons or their effective bindings to change tools
without discarding the battlefield, selection, camera, or current time. Rules
and data and Samples remain separate supporting sections.

The single timeline shows authored, predicted, accepted, and committed
channels together. Drag its range control, enter an exact time, use Home/End,
step, or play to move the cursor. Seeking is projection-only: it neither edits
a plan nor commits execution. Committed intervals are immutable; new planning
starts at the displayed next-editable boundary.

Plan authors route, facing, attention, stance, hold, engagement, and
synchronization commands at the current editable time. Selected commands can
be moved to the cursor or removed with exact undo/redo identity. Preview is
labelled non-authoritative, validation accepts an exact revision, and commit
advances authoritative history.

Press `?` in any modality to open the live action list. It is generated from
the same availability and binding data as dispatch and includes pointer-only
and unbound actions. The Configure bindings dialog captures shortcuts with
native controls, diagnoses conflicts before replacement, and supports clear,
per-command restore, per-modality restore, restore-all, and deterministic JSON
import/export. Browser-reserved shortcuts and typing inside native text
controls are not intercepted.

At narrow widths the panels reflow in document order while modality, timeline,
cursor, and contextual state remain available. All tactical controls use
native keyboard-operable descendants and 44 CSS-pixel targets.
