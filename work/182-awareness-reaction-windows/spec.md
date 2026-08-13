---
schemaVersion: 1
workId: 182-awareness-reaction-windows
title: Awareness Reaction Windows
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Awareness Reaction Windows Specification

Prose status: specified

## User Value
Players can deliberately orient attention, prepare a covered unit/area/edge, move through threatened space, and inspect why an awareness or reaction event did or did not occur without omniscient spotting or automatic rear-hit bonuses.

## Scope
- SB-001: Add versioned directional sensor, stimulus, local-knowledge, awareness, engagement, reaction-window, ordered-event, and diagnostic contracts to the authoritative simulation.
- SB-002: Integrate the contracts with the existing orientation, `SpatialQuery`, physical-combat, deterministic tick, replay, match-control observation, Client projection, and production browser routes.
- SB-003: Qualify front/peripheral/rear observation, occlusion, delayed acquisition, decay/lost contact, prepared unit/area/guarded-edge triggers, interruption/recovery, and simultaneous movement/reaction ordering.
- SB-004: Ship exact .NET/Fable canonical bytes, a real-entry browser journey, a 100v100 moving-contact performance workload, fail-capable subject mutations, documentation, and lifecycle evidence.

## Non-Goals
- SB-005: Do not implement full communications topology, morale, a stealth-equipment catalog, executions, final perception tuning, or every weapon/posture combination.
- SB-006: Do not equate geometric LOS with identification, make knowledge global, collapse body/attention/posture/movement direction, add unconditional side/rear damage multipliers, or move authority into Client/Web code.
- SB-007: Do not broaden the published Game.Core Fable profile, copy package algorithms, add floating-point authoritative geometry/randomness, or reinterpret retained replay through current schemas.

## User Stories
- US-001 (P1): As a tactical player, I can orient a unit's attention independently of its body and movement so front, peripheral, and rear observation have visible consequences.
- US-002 (P1): As a player, I can prepare one unit or bounded sector/edge and understand preparation, coverage, eligibility, commitment, resolution, interruption, and recovery over ticks.
- US-003 (P1): As a control-module author, I receive only locally known sectors, stimuli, awareness, engagements, and typed reasons rather than hidden world truth.
- US-004 (P1): As a replay/browser user, I can scrub deterministic awareness and reaction events and see why acquisition, loss, trigger, or interruption occurred.
- US-005 (P1): As an operator, I can qualify the 100v100 production tick route with deterministic counters and exact native/Fable/browser evidence inside producer-owned budgets.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001] [FR-002]: Given identical positions with different body, attention, posture, and movement directions, when sensor sectors are evaluated, then the fields remain distinct and front/peripheral/rear eligibility changes only through the declared sensor profile and exact LOS.
- AC-002 [US-001] [US-003] [FR-003] [FR-004]: Given visible, occluded, newly exposed, and departed targets, when ticks advance, then factual stimuli are recorded separately from local knowledge, acquisition is delayed, lost contact decays deterministically, and LOS alone never identifies a target.
- AC-003 [US-002] [FR-005] [FR-006]: Given a unit-target, area/sector, or guarded-edge engagement, when preparation advances, then exactly one engagement per unit becomes actively covered only after declared preparation and exposes its bounded target geometry and reasons.
- AC-004 [US-002] [FR-007] [FR-008]: Given entry into a covered area, crossing a guarded edge/door, or exposing a valid known target, when the trigger becomes eligible, then the reaction window commits/resolves in canonical tick order or emits a typed non-trigger/interruption reason.
- AC-005 [US-002] [FR-008] [FR-009]: Given simultaneous movement, exposure, reaction, physical effects, loss of eligibility, and recovery, when one authoritative tick resolves, then the public ordering is stable, replay-safe, and produces identical state/events for identical inputs.
- AC-006 [US-003] [FR-010]: Given two units with different local knowledge over the same world, when control observations are encoded, then locally known sectors, stimuli, awareness, engagement state, and reasons differ appropriately while undisclosed world truth is absent.
- AC-007 [US-004] [FR-011]: Given canonical awareness/reaction fixtures and timeline seeks, when native and Fable replay reconstruct them, then exact schema/profile/package identities produce byte-identical state/facts/events and unavailable historical identities fail explicitly.
- AC-008 [US-004] [FR-012]: Given the built production browser at its real entry, when player-emittable controls rotate attention, establish a covered sector, and move another unit through it, then the reaction and its reasons are visible and timeline scrubbing reconstructs the same outcome without test-only messages.
- AC-009 [US-005] [FR-013]: Given 200 moving units in contact on the declared bounded workload, when the Release production tick route runs, then structural caps and deterministic counters pass, awareness remains within its declared sub-budget, and aggregate work remains below the 20 ms target/50 ms ceiling posture on the qualification host.
- AC-010 [US-005] [FR-014]: Given mutations that equate LOS with awareness, collapse facing/attention, bypass preparation, fire without eligibility, reorder simultaneous events, leak knowledge, ignore replay/runtime divergence, or make evidence unreadable, when owning gates run, then each gate rejects its protected-subject mutation and exact-candidate lifecycle/CI evidence remains current.

## Functional Requirements
- FR-001: The system MUST declare body facing, attention/sensor direction, weapon posture, and movement direction as separate versioned fields whose canonical encodings and update rules cannot alias one another. (Stories: US-001; Acceptance: AC-001)
- FR-002: The system MUST evaluate bounded forward, peripheral, and rear observation sectors through declared integer sensor profiles and exact `SpatialQuery` LOS evidence, with canonical sector/range/occlusion reasons and no client-side geometry authority. (Stories: US-001; Acceptance: AC-001)
- FR-003: The system MUST represent factual stimuli independently from per-unit knowledge and awareness, bind each observation to modality/source/tick/spatial evidence, and prohibit geometric LOS from implying immediate identification. (Stories: US-001, US-003; Acceptance: AC-002)
- FR-004: Awareness MUST accumulate and decay through bounded integer thresholds over ticks, distinguish unknown/suspected/acquired/lost-contact states, retain last-known facts without hidden updates, and emit deterministic transition reasons. (Stories: US-001, US-003; Acceptance: AC-002)
- FR-005: Each unit MUST own at most one versioned engagement targeting either a locally known unit or a declared bounded area/sector/semantic edge, with stable identity and no implicit omniscient retargeting. (Stories: US-002; Acceptance: AC-003)
- FR-006: Engagement state MUST expose preparation, active coverage, trigger eligibility, commitment, resolution, interruption, and recovery as explicit tick phases with bounded durations, posture/attention requirements, and typed reasons. (Stories: US-002; Acceptance: AC-003)
- FR-007: Reaction windows MUST support entry into a covered area, crossing a guarded semantic edge/door, and exposure of a valid locally known target, consuming authoritative movement/spatial evidence and never firing before active preparation. (Stories: US-002; Acceptance: AC-004)
- FR-008: The authoritative tick MUST publish one canonical ordering for movement intent, spatial transition evidence, stimuli/awareness transitions, trigger eligibility, reaction commitment, physical resolution, interruption, ordinary action resolution, event emission, and recovery. (Stories: US-002, US-004; Acceptance: AC-004, AC-005)
- FR-009: Eligibility loss, attention/posture change, incapacitation, invalidated target/area/edge, blocked fire, and state change during a window MUST produce deterministic interruption/non-trigger outcomes rather than silent cancellation or stale resolution. (Stories: US-002, US-004; Acceptance: AC-005)
- FR-010: Control observations MUST expose machine-readable locally known sectors, bounded stimuli, awareness states, engagement phase/target, trigger/recovery timing, and explanatory reasons while excluding undisclosed world truth and authority-only geometry. (Stories: US-003; Acceptance: AC-006)
- FR-011: Replay/timeline contracts MUST bind awareness schema, sensor profile, spatial profile/package, reaction ordering, ordered inputs/facts/effects, and exact historical identity; native and Fable canonical bytes MUST agree or report the first divergence. (Stories: US-004; Acceptance: AC-007)
- FR-012: Client and browser code MUST project authoritative awareness/reaction results without recomputation, and a bot-driven journey MUST boot the real product entry and use player-emittable controls to rotate attention, prepare coverage, cross it, inspect the reaction, and scrub the timeline. (Stories: US-004; Acceptance: AC-008)
- FR-013: Before implementation acceptance, a versioned performance contract MUST declare and Release-measure the real authoritative update route for 200 moving units in contact, including candidate/sector/LOS/acquisition/engagement/trigger/reaction/event/allocation counters, bounded scale/caps, workload digest, host facts, and the 20 ms working-target/50 ms ceiling posture. (Stories: US-005; Acceptance: AC-009)
- FR-014: The change MUST ship declared `.fsi` surfaces, schemas/fixtures, docs, schema-v2 feedback, SDD readiness, package-only .NET/Fable/Node/browser evidence, protected-subject and unreadable-input mutations for every added/modified gate, production delivery, exact-head CI, independent review, and guarded landing. (Stories: US-005; Acceptance: AC-010)

## Ambiguities
- AMB-001: Which exact sensor-sector geometry, profile values, awareness thresholds, decay, and last-known retention define v1 without importing the research spike as unversioned authority?
- AMB-002: Which typed stimulus, knowledge, awareness, engagement, reaction-window, event, reason, and control-observation surfaces extend current simulation/replay additively?
- AMB-003: How do area/sector and guarded-edge targets bind to authoritative spatial evidence and stable identities while enforcing one engagement per unit?
- AMB-004: What exact simultaneous-tick ordering and interruption rules compose movement, awareness, reaction, and existing physical combat without a second resolver?
- AMB-005: Which player-emittable production controls and renderer-neutral projections make the required journey genuinely reachable and timeline-scrubbable?
- AMB-006: What workload/counters/caps and explicit awareness sub-budget preserve the producer-owned 100v100 performance posture without treating headless evidence as compositor proof?
- AMB-007: How are current and retained replay/runtime identities versioned so additive awareness/reaction data does not reinterpret older packages?

## Public Or Tool-Facing Impact
- Additive public F# records/unions/functions in Simulation, Match control observations, replay/events, Client projections, and Web input/projection bindings.
- Versioned awareness/reaction schema, workload definition/receipt, canonical native/Fable fixtures, browser journey evidence, and explanatory diagnostics.
- Documentation updates describe the authority split, tactical semantics, control-module vocabulary, event ordering, performance posture, and non-goals.

## Lifecycle Notes
- Tier 1 contracted change: public surfaces, canonical replay/event bytes, production behavior, performance contracts, and cross-runtime evidence change together.
- Game.Core use remains package-only and limited to classified `LockstepExact` spatial operations through S.I.R.'s existing adapter; S.I.R. owns awareness, knowledge, reaction, replay, protocol, and UI meaning.
- Next lifecycle action: `fsgg-sdd clarify --work 182-awareness-reaction-windows`.
