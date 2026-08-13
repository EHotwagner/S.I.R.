---
schemaVersion: 1
workId: 182-awareness-reaction-windows
title: "Directional awareness, area engagements, and reaction windows"
stage: charter
changeTier: tier1
status: chartered
policyPointers:
  - .fsgg/sdd.yml
  - .fsgg/agents.yml
  - .fsgg/policy.yml
  - .fsgg/capabilities.yml
  - .fsgg/tooling.yml
---

# Directional awareness, area engagements, and reaction windows Charter

## Identity
Deliver S.I.R.'s first deterministic awareness and reaction slice: body facing, attention, sensor sectors, factual stimuli, local knowledge, preparation, area/sector coverage, trigger windows, interruption, and recovery become distinct authoritative facts visible in replay and the production browser.

## Principles
- World geometry, factual stimuli, unit knowledge, awareness state, and player disclosure remain separate authority layers; exact LOS never implies immediate identification.
- Body facing, attention/sensor direction, weapon posture, and movement direction remain independently typed and replayed.
- One engagement per unit targets either a known unit or a declared bounded area/sector and advances through explicit tick phases.
- Simultaneous movement and reaction use one public canonical ordering, bounded integer state, stable identifiers, and replay-safe facts.
- Authoritative spatial work consumes only S.I.R.'s `SpatialQuery` adapter over published `FS.GG.Game.Core` LockstepExact surfaces; clients project results and never infer awareness or eligibility.
- Real-entry player journeys, exact .NET/Fable bytes, Release performance evidence, protected-subject mutations, docs, lifecycle evidence, and schema-v2 feedback ship together.

## Scope Boundaries
- In: forward/peripheral/rear sensor sectors; exact LOS and occlusion; stimulus accumulation, delayed acquisition, decay and lost contact; locally known sectors; unit/area/edge engagements; preparation, active coverage, eligibility, commitment, resolution, interruption and recovery; simultaneous event ordering; control observations; replay/events/diagnostics; browser timeline journey; performance and mutation evidence.
- In: additive integration with existing orientation, spatial-query, physical-combat, simulation, match-control, client projection, and browser surfaces.
- Out: full communications topology, morale, a stealth-equipment catalog, executions, final perception tuning, new upstream Game.Core semantics, client-side authority, omniscient LOS, and unconditional rear-hit damage multipliers.

## Policy Pointers
- Honor constitution I-III through specification-first work, declared `.fsi` surfaces, versioned contracts, and synchronized signatures/tests/docs.
- Honor constitution IV-V through plain F# records/unions and pure Model-Update-Effect awareness/reaction transitions behind explicit spatial/combat edges.
- Honor constitution VI-VIII through real package/runtime/player evidence, subject mutations, deterministic bounds/counters, and typed failure for malformed, stale, or unavailable inputs.
- Apply `.fsgg/sdd.yml`, `.fsgg/agents.yml`, `docs/performance-budget.md`, `docs/game-governance.md`, and profile `fs-gg-game-core-fable-lockstep-v1`; Governance remains optional compatibility metadata.

## Lifecycle Notes
- Tier 1: this changes public F# simulation/control/replay state, canonical event ordering, browser behavior, and performance contracts.
- The producer-owned performance baseline is the 20 Hz/50 ms hard tick ceiling, ~20 ms working aggregate target at 100v100, the measured perception posture, and existing spatial/combat/client budgets; this item will not invent an FPS contract.
- No issue requirement is silently deferred. Any distinct dependency must be recorded through the owning board rather than absorbed into this item.
- Next lifecycle action: `fsgg-sdd specify --work 182-awareness-reaction-windows`.
