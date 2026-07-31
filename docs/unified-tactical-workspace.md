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

> **Corrective implementation status (2026-07-31):** the tactical application
> now has the versioned Field Focus shell and layout system described by the
> [persistent-workspace design report](2026-07-31-0840-vscode-style-persistent-tactical-workspace-design-report.md).
> Its compact toolbar, configurable sidebars, responsive drawers,
> and independently visible/collapsible bottom-panel profile surround the
> existing battlefield paths. Invalid or future persisted layout profiles fail
> closed to canonical Field Focus with an announced diagnostic. Editor tools,
> layers, selection inspector, validation, and document state now occupy real
> registered panels; later milestones migrate the other modalities. The canonical
> workscreen is now one retained, keyboard-focusable SVG with stable camera,
> terrain, edge, route, unit, selection, and annotation layers. Exact SVG and
> layer references survive modality, panel, timeline, resize, overlay, and
> playback updates. The labelled compatibility disclosure contains only
> non-workscreen, registry-routed migration guidance; alternate modality
> battlefield and grid roots are not mounted. Existing Plan, Simulator, and
> Review control panels remain reachable beside the shell while later
> milestones move them into final sidebar placements. A shared,
> presentation-only scene projection now gives Editor, Plan, Simulate, and
> Review stable terrain/unit/route/annotation/layer identities while preserving
> complete visible semantic selection. A shell-owned semantic unit identity is
> retained only when visible in the target projection, while each owner keeps
> its separate authoritative selection. Review accepts only an opaque validated
> replay owner; relabelled perspective data with hidden entities is rejected.
> Editor background, regions, guides, previews, selection gestures, validation,
> pointer handling, and camera interaction now use the retained SVG; the old
> Editor battlefield/object-grid renderer and CSS were removed after parity
> qualification. The projection remains ready for later modality migrations.

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
