---
schemaVersion: 1
workId: 179-continuous-simulation-state
title: Continuous Simulation State
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Continuous Simulation State Specification

Prose status: specified

## User Value
Tactical authors continue simulation across compatible authored edits and timeline navigation.

## Scope
- SB-001: Reconcile compatible valid map edits, preserve runtime state and UI context, reconstruct
  deterministic timeline state, and expose the behavior through every tactical modality.

## Non-Goals
- SB-002: No LOS, cover, armor, weapon rules, scenario content, or manual simulator-handoff ceremony.

## User Stories
- US-001 (P1): As a tactical author, I can edit a valid map without losing compatible live simulation.
- US-002 (P1): As a tactical author, I can seek a timeline and see the real state at that tick.
- US-003 (P1): As a tactical author, I can retain the same board interaction context in every modality.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given a valid simulator at tick N, when a unit is added, then existing runtime position, HP, facing, attention, movement, combat recovery, playback state, selection, and camera remain unchanged.
- AC-002 [US-002] [FR-002]: Given a unit introduced at tick N, when the author seeks before and at N, then the unit is absent before N and present at N in reconstructed deterministic state.
- AC-003 [US-001] [FR-003]: Given terrain, geometry, topology, removal, or mutation of an existing simulated unit, when the edit becomes valid, then simulation rebuilds deterministically at tick zero and discloses the specific reason.
- AC-004 [US-003] [FR-004]: Given any tactical modality with a valid simulator, when the author plays, pauses, steps, pans, zooms, selects, focuses roster, or seeks, then the command remains available where applicable and context/tick remain truthful.
- AC-005 [US-001] [FR-005]: Given the production browser, when the continuous-state journey is exercised, then obsolete handoff wording is absent and visible controls prove preservation and fallback behavior.

## Functional Requirements
- FR-001: The system MUST reconcile an additions-only valid edit at tick N without changing existing runtime or UI context. (covers AC-001)
- FR-002: The system MUST record an explicit activation tick and reconstruct deterministic simulation state for timeline seeking. (covers AC-002)
- FR-003: The system MUST classify incompatible edits and perform a tick-zero rebuild with a visible specific explanation. (covers AC-003)
- FR-004: The system MUST retain Play/Pause/Step and symmetric board interaction across Editor, Plan, Simulate, and Review without silently changing modality. (covers AC-004)
- FR-005: The system MUST prove the behavior through focused .NET tests, mutation evidence, and a production-browser journey without user-facing handoff chrome. (covers AC-005)

## Ambiguities
No material ambiguities recorded.

## Public Or Tool-Facing Impact
- This specification is an SDD lifecycle artifact and command-report contract input.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 179-continuous-simulation-state`.
