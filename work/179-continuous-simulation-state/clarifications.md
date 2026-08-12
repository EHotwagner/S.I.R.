---
schemaVersion: 1
workId: 179-continuous-simulation-state
title: Continuous Simulation State
stage: clarify
changeTier: tier1
status: clarified
sourceSpec: work/179-continuous-simulation-state/spec.md
publicOrToolFacingImpact: true
---

# Continuous Simulation State Clarifications

## Source Specification
- work/179-continuous-simulation-state/spec.md

## Clarification Questions
No clarification questions recorded.

## Answers
No clarification answers recorded.

## Decisions
- DEC-001 [FR-001] [AC-001]: An edit is compatible only when it adds units and leaves geometry,
  terrain, edges, and all previously simulated unit definitions unchanged.
- DEC-002 [FR-002] [AC-002]: Each reconciled addition stores the current simulation tick as its
  activation tick; seeking rebuilds from the pinned baseline and replays to the requested tick.
- DEC-003 [FR-003] [AC-003]: Incompatible valid edits rebuild at tick zero and expose the first
  deterministic compatibility reason in the user-visible status.
- DEC-004 [FR-004] [AC-004]: Modality changes preserve simulator, camera, selection, focus, and
  cursor; playback only changes simulation time.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
No blocking ambiguity remains.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 179-continuous-simulation-state`.
