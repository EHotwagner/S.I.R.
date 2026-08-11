---
schemaVersion: 1
workId: 145-desktop-command-surface
title: Desktop Command Surface
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/145-desktop-command-surface/spec.md
sourceClarifications: work/145-desktop-command-surface/clarifications.md
sourceChecklist: work/145-desktop-command-surface/checklist.md
publicOrToolFacingImpact: true
---

# Desktop Command Surface Plan

Prose status: planned

## Source Snapshot
- spec: work/145-desktop-command-surface/spec.md sha256:c80d235c8e68ac8cdd8d9a3f29c1100afbf8d15ef2a6cc56393a2547a2f90594 schemaVersion:1
- clarifications: work/145-desktop-command-surface/clarifications.md sha256:ff1e721ad79c6e4e4c3d0cc0151a6a14639ef5a8d61d4e731bdb988770eaac97 schemaVersion:1
- checklist: work/145-desktop-command-surface/checklist.md sha256:8b7e8421334a0605a03284dedeba51d23273833e3624228d57a02c7364e3d5d5 schemaVersion:1

## Plan Scope
- Work item 145-desktop-command-surface is planned from the current specification, clarification, and checklist facts.
- Requirement count: 6.
- Clarification decision count: 3.
- Checklist result count: 6.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: Render the stable six-group desktop menu bar from registry commands and registry-derived shortcut presentation.
- PD-002 [AC-002] [FR-002] [DEC-001] complete: Extend browser menu behavior with a focused active-menu state, directional focus, Enter invocation, and Escape close that intercepts the keyboard event.
- PD-003 [AC-003] [FR-003] [DEC-002] complete: Add validated order-only local toolbar persistence, add/remove/reorder controls, and reset-to-default behavior.
- PD-004 [AC-004] [FR-004] [DEC-001] complete: Apply toolbar/menu roles, accessible labels, and #143 ARIA shortcut metadata via the common registry renderer.
- PD-005 [AC-005] [FR-005] [DEC-003] complete: Keep command chrome fixed around the current work surface, adapt availability by active registry projection, and provide a compact overflow disclosure.
- PD-006 [AC-006] [FR-006] [DEC-001] complete: Invoke menu and toolbar commands exclusively by command id through the existing `InvokeTacticalCommand` path.

## Contract Impact
- PC-001 [PD-001] [PD-003] [PD-004] publicSurface: Declare toolbar-layout parsing/default helpers in the client surface and preserve missing/invalid persisted values as the documented default.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-003] [PD-004] [PD-005] [PD-006] [PC-001] semanticTest: Add focused client tests and Playwright production-route checks for menu keyboard behavior, command dispatch, persistence/reset, ARIA semantics, and compact overflow; mutate each added subject assertion to prove it reds.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] compatible: `sir.desktop-toolbar.v1` is an additive browser-local preference; absent, malformed, duplicate, or unavailable command ids fall back to the default registry order.

## Generated View Impact
- GV-001 [PD-001] [PD-003] [PD-006] workModel: Refresh analysis and generated guidance after authored lifecycle changes so the final evidence binds the exact implementation and browser artifact.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 145-desktop-command-surface`.
