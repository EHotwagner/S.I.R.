---
schemaVersion: 1
workId: 179-continuous-simulation-state
title: Continuous Simulation State
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/179-continuous-simulation-state/spec.md
sourceClarifications: work/179-continuous-simulation-state/clarifications.md
sourceChecklist: work/179-continuous-simulation-state/checklist.md
publicOrToolFacingImpact: true
---

# Continuous Simulation State Plan

Prose status: planned

## Source Snapshot
- spec: work/179-continuous-simulation-state/spec.md sha256:db8be8bc52d7d6ea1037f394f339bfa89562b805e882f1f173a21ba113d4b74d schemaVersion:1
- clarifications: work/179-continuous-simulation-state/clarifications.md sha256:96d82baf789995b515dbd3b2c00aef1f9cc129ac82ecfbfe37e37882d1692bba schemaVersion:1
- checklist: work/179-continuous-simulation-state/checklist.md sha256:8f3246788ffc6b9cefabf2a82a5a0bbb754c868e803ef7e34415584646ad7210 schemaVersion:1

## Plan Scope
- Add reconciliation and replay history at the simulator boundary, then consume that one state in the
  editor/workspace projection so modalities cannot diverge.
- Extend focused qualification tests and production browser journeys; remove obsolete handoff wording.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: Compare revisions, merge additions at the current tick,
  and preserve the existing kernel/runtime and presentation state.
- PD-002 [AC-002] [FR-002] [DEC-002] complete: Store baseline and activation history, replay deterministically
  for a requested tick, and bind the visible cursor to reconstructed simulation tick.
- PD-003 [AC-003] [FR-003] [DEC-003] complete: Detect incompatible map/unit edits, rebuild from tick zero,
  and carry a specific visible status reason.
- PD-004 [AC-004] [FR-004] [DEC-004] complete: Route one reconciled simulator through Editor, Plan,
  Simulate, and Review while retaining viewport/selection/focus and playback commands.
- PD-005 [AC-005] [FR-005] complete: Add focused .NET and visible Playwright journeys, exercise subject
  mutations, and update roadmap evidence without exposing a manual handoff action.

## Contract Impact
- PC-001 [PD-001] [PD-002] simulator state: `SimulatorHandoff` owns baseline, activation history,
  reconciliation, and seek state; UI surfaces only consume it.

## Verification Obligations
- VO-001 [PD-005] [PC-001] verification: Run focused client qualifications and production Playwright
  journeys, retain TRX/JUnit evidence, and demonstrate each changed test reds for a subject mutation.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] compatibility: Preserve existing serialized map and browser entry contracts; new runtime
  history is an in-memory projection rebuilt from the authored revision.

## Generated View Impact
- GV-001 [PD-005] workModel: Refresh analysis, work model, verify, ship, and generated agent guidance after
  authored task/evidence changes.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 179-continuous-simulation-state`.
