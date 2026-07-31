---
title: Persistent Tactical Workspace Milestone 1 Evidence
category: Engineering
categoryindex: 6
index: 13
status: accepted
decision-status: implementation-evidence
document-type: test-evidence
version: "1.0"
last-updated: 2026-07-31
description: Shell, layout-contract, persistence, interaction, responsive, and Field Focus evidence for corrective Milestone 1.
related:
  - docs/2026-07-31-0840-vscode-style-persistent-tactical-workspace-design-report.md
  - docs/persistent-tactical-workspace-m0-baseline.md
---

# Persistent Tactical Workspace Milestone 1 Evidence

Milestone 1 introduces presentation-only layout ownership around the existing
battlefield render paths. It does not move domain controls into the new panels
and does not claim SVG-root persistence; those remain later milestones.

## Contracts and deterministic persistence

`TacticalWorkspaceLayout.fsi` publishes:

- stable panel definitions and `PanelPlacement` with side, order, visibility,
  and collapsed state;
- independent left/right `SidebarLayout` width and responsive drawer state;
- `BottomPanelLayout` visibility, height, Editor collapse default, and
  non-Editor collapse default;
- one versioned `TacticalLayoutProfile`; and
- explicit diagnostics for malformed, future, unknown, duplicate, and
  invalid-dimension input.

The registry has ten stable IDs. Field Focus defaults place Roster/Outliner,
Tools, and Layers on the left; Selection, Validation, and Document/Revision on
the right; and Rules, Data, Samples, and Diagnostics hidden. Sidebars are 208
and 224 CSS pixels. At this milestone the timeline was 152 CSS pixels and
collapsed in Editor and expanded in Plan, Simulate, and Review; corrective M9
acceptance later expanded the shallow timeline by default in every modality.

Export is canonical JSON with deterministic side/order/ID sorting. Import uses
a strict parser: missing or unknown object fields, duplicate JSON fields,
array/object trailing commas, malformed or truncated JSON, unknown/duplicate
panel IDs, future schemas, invalid sides, and dimensions outside accepted
bounds fail closed. Integer tokens accept ASCII digits only, allow an optional
minus, and reject leading zeroes except the literal zero; only JSON's four
defined whitespace characters are skipped. Schema zero migrates deterministically.
When a known current-schema profile omits a newly registered panel, that panel
receives its registry default without changing recognized placements.

The browser persists canonical profiles under `sir.tactical-layout.v1`.
Malformed or future-schema stored content restores Field Focus, replaces the
bad value with the canonical default, and emits a status diagnostic. Every
show/hide, collapse, move, reorder, drawer, timeline visibility, timeline
collapse, and reset operation writes one canonical profile.

## Shell and command boundary

The compact toolbar contains document/revision status, the existing four
modality controls, essential play/pause, panel/drawer/timeline controls, reset,
and contextual Actions. Modality, transport, and Actions dispatch the existing
`InvokeTacticalCommand` IDs. No second command catalog or domain mutation path
was introduced.

The left and right sidebars render registered panel hosts with layout controls
and migration placeholders only. Actual Editor, Plan, Simulate, and Review
controls remain in their current content trees until their migration
milestones. Therefore panel configuration changes only `TacticalLayoutProfile`;
they do not alter map, plan, simulator, replay, camera, selection, or timeline
authority.

The existing battlefield remains between both sidebars. The current shared
timeline is hosted by a bottom panel spanning the full frame. Editor's
collapsed default retains the mounted DOM during this compatibility milestone,
while Plan, Simulate, and Review expose it at the shallow Field Focus height.
The separate visibility control removes the complete bottom-panel subtree and
restores it without changing its modality-specific collapsed state.

## Interaction and focus

Every visible panel provides native non-drag controls for collapse/expand,
move up/down, move to the opposite sidebar, and hide. The toolbar panel menu
shows or hides every registered panel.

- collapse, move, reorder, and show restore focus to the panel header control;
- hide removes the complete panel subtree from the DOM and restores focus to
  its toolbar toggle;
- Reset layout restores Field Focus and returns focus to Reset layout;
- drawer open and close return focus to the controlling toolbar button;
- responsive drawer controls are absent from desktop layout and accessibility
  exposure, then become available at the 48rem mobile breakpoint;
- timeline collapse/expand round trips return focus to the collapse control;
- bottom-panel hide/show removes/restores its subtree and returns focus to its
  visibility control;
- hidden panel controls are therefore absent from tab order; and
- all operations preserve unrelated application model fields by construction.

At the 48rem reflow breakpoint the central frame becomes one column. Sidebars
become off-canvas drawers, remain unavailable to pointer interaction while
closed, and enter from their respective edge only when their native toolbar
button reports `aria-expanded=true`. The compact toolbar wraps and remains
sticky; workscreen and timeline access remain present.

## Focused and browser qualification

`TacticalWorkspaceLayoutQualification` proves:

- exact Field Focus dimensions, placements, and modality collapse defaults;
- canonical export/import equality;
- schema-zero migration;
- deterministic defaults for newly introduced panels;
- rejection of future schemas, unknown/duplicate IDs, unknown fields, and
  invalid dimensions, non-ASCII or leading-zero integers, malformed negative
  integers, trailing commas, malformed JSON, and truncated JSON; and
- show/hide, collapse, move, reorder, drawer, timeline visibility/collapse,
  and reset operations, including false/true visibility round trips.

Production browser smoke proves:

- compact toolbar, 3+3 visible default sidebars, CSS dimensions, and collapsed
  Editor timeline;
- panel collapse, cross-sidebar move, non-drag reorder, hide, and show;
- focus restoration after each structural operation;
- hidden side-panel and bottom-panel subtree removal;
- canonical local-storage writes and deterministic reset with restored focus;
- isolated fresh mounts applying a customized valid profile;
- isolated malformed and future-schema mounts restoring diagnostic, canonical
  Field Focus;
- drawer open/close state synchronized between class and `aria-expanded`, with
  closed responsive drawers unavailable by CSS and focus returned to the
  toggle;
- desktop-hidden/mobile-visible drawer disclosure CSS, desktop computed state,
  matched mobile viewport, and functional mobile disclosure behavior;
- timeline visibility and modality-collapse round trips with focus restored to
  their respective controls; and
- desktop Field Focus columns plus responsive one-column/off-canvas CSS.

The existing production smoke continues to qualify modality switching,
timeline behavior, planning, Editor, Simulator, Review, bindings, help,
supporting sections, and the known Milestone 0 wrapper lifecycle.

## Validation record

- focused .NET layout qualification: passed;
- production Fable build and expanded browser smoke: passed;
- documentation build, browser smoke, and accessibility qualification: passed;
- seven deterministic map-editor review pairs regenerated against the current
  production bundle and their manifest qualification passed;
- `git diff --check`: passed;
- sequential `./fake.sh build -t Dev`, `Test`, and `Verify`: passed.
