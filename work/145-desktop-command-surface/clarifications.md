---
schemaVersion: 1
workId: 145-desktop-command-surface
title: Desktop Command Surface
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/145-desktop-command-surface/spec.md
publicOrToolFacingImpact: true
---

# Desktop Command Surface Clarifications

## Source Specification
- work/145-desktop-command-surface/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: Which component owns the shared desktop command chrome?
- CQ-002 [AMB:AMB-002]: How will toolbar preference persist without duplicating #143 bindings?
- CQ-003 [AMB:AMB-003]: Which production evidence establishes compact overflow?

## Answers
- CQ-001 → `tacticalLayoutToolbar`, `activeTacticalRegistry`, and `tacticalCommandButton` are the browser seam: retain registry resolution and compose a stable menu/toolbar shell around it.
- CQ-002 → a separate versioned browser-local toolbar-order payload stores only command ids; binding presentation stays in `TacticalBindingProfile` and registry helpers.
- CQ-003 → Playwright loads the browser app at a compact viewport and inspects the rendered overflow control and command activation route.

## Decisions
- DEC-001 [CQ-001] [AMB:AMB-001] [FR-001] [FR-002] [FR-004] [FR-006]: Build menu and toolbar controls from `activeTacticalRegistry`, using the existing command-button and keyboard-dispatch authority rather than a second action map.
- DEC-002 [CQ-002] [AMB:AMB-002] [FR-003] [FR-006]: Persist an order-only `sir.desktop-toolbar.v1` browser value, validate ids against registry defaults, and reset invalid or requested layouts to the default list.
- DEC-003 [CQ-003] [AMB:AMB-003] [FR-005]: Use a compact overflow disclosure rendered in the production shell and assert it with a real browser test; no numeric performance target is introduced.

## Accepted Deferrals
None.

## Remaining Ambiguity
None.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 145-desktop-command-surface`.
