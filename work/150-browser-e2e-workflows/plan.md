---
schemaVersion: 1
workId: 150-browser-e2e-workflows
title: Browser E2e Workflows
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/150-browser-e2e-workflows/spec.md
sourceClarifications: work/150-browser-e2e-workflows/clarifications.md
sourceChecklist: work/150-browser-e2e-workflows/checklist.md
publicOrToolFacingImpact: true
---

# Browser E2e Workflows Plan

Prose status: planned

## Source Snapshot
- spec: work/150-browser-e2e-workflows/spec.md sha256:4e483c7e3bfbe1f94bc45198137a4bfc40ac6c9b25c21046cbb41e719510f541 schemaVersion:1
- clarifications: work/150-browser-e2e-workflows/clarifications.md sha256:cfc9df4975bdeb72bfc1be56581b33fc5e2da543d57a84461d7bedfee1d7ed9c schemaVersion:1
- checklist: work/150-browser-e2e-workflows/checklist.md sha256:ccf9b9b31f17f8ecac773f8a2346870ab66ed0dd59f878fe57c927d22fda75b7 schemaVersion:1

## Plan Scope
- Add a reusable Playwright journey harness under `tests/SIR.Browser.Tests` that captures browser diagnostics and exposes only accessible/visible interaction helpers.
- Extend the current client only where it lacks an accessible label, visible unavailable reason, or observable result necessary to test its existing behavior honestly.
- Keep browser-test commands deterministic and serial where the shared development live-authority principal makes concurrent startup nondeterministic.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Build a visible mode-transition journey that selects a sample, uses the mode controls, and compares rendered scene/selection context at each transition.
- PD-002 [AC-002] [FR-002] complete: Drive simulator load and playback controls through their accessible names; assert rendered timeline and status changes rather than internal session state.
- PD-003 [AC-003] [FR-003] complete: Cover disabled command state and its exposed reason for no sample, invalid state, or unavailable authority.
- PD-004 [AC-004] [FR-004] complete: Use keyboard focus, arrow navigation, Enter/Escape, and displayed shortcut chords to exercise the desktop menu and toolbar command surface.
- PD-005 [AC-005] [FR-005] complete: Test layout resizing, reload persistence, reset, narrow width, and 400 percent zoom through visible panel/control outcomes.
- PD-006 [AC-006] [FR-006] complete: Use the public file chooser to import supported map/replay/background fixtures and assert the visible success or rejection message.
- PD-007 [AC-007] [FR-007] complete: Exercise the normal authorized live-session route and assert a rendered battlefield change after authoritative advance and reconnect.
- PD-008 [AC-008] [FR-008] complete: Install per-page console/page/request-failure collection with a narrow explicit rejection allowance and fail the scenario in teardown on unexplained diagnostics.

## Contract Impact
- PC-001 [PD-001] [PD-008] test architecture: Browser test helpers are public user-interface interactions and diagnostics collection; no new runtime API or private browser hook is introduced.

## Verification Obligations
- VO-001 [PD-001] [PD-008] chromium: Run the full Chromium suite against the production client route, retain its JUnit/report artifact, and prove each added or modified gate reds on a subject mutation.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] compatibility: Preserve the existing browser-test entry point and add scenarios without changing production tactical data formats or the live-authority protocol.

## Generated View Impact
- GV-001 [PD-001] workModel: Refresh the SDD work model and guidance after authored evidence or task state changes; retain current analysis/verify/ship receipts for the exact candidate.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 150-browser-e2e-workflows`.
