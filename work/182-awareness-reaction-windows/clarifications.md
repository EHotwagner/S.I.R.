---
schemaVersion: 1
workId: 182-awareness-reaction-windows
title: Awareness Reaction Windows
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/182-awareness-reaction-windows/spec.md
publicOrToolFacingImpact: true
---

# Awareness Reaction Windows Clarifications

## Source Specification
- work/182-awareness-reaction-windows/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: Which exact sector/profile/acquisition values form awareness v1?
- CQ-002 [AMB:AMB-002]: Which public types and authority layers extend current simulation and control observations?
- CQ-003 [AMB:AMB-003]: How are unit, area/sector, and guarded-edge engagements represented and bounded?
- CQ-004 [AMB:AMB-004]: What tick ordering and interruption rules compose reaction with movement and physical combat?
- CQ-005 [AMB:AMB-005]: Which production controls and projections satisfy the real-entry browser journey?
- CQ-006 [AMB:AMB-006]: Which executable workload, counters, caps, and timing budgets qualify the feature?
- CQ-007 [AMB:AMB-007]: How do awareness/reaction replay identities coexist with retained packages?

## Answers
- CQ-001 → v1 uses eight-direction integer sectors over attention direction: forward is direction distance 0-1, peripheral is distance 2, rear is distance 3-4. A declared profile supplies range, per-sector acquisition contribution, threshold, decay, last-known retention, and exposure-sample cap; the canonical infantry profile uses range 60 cells, contributions 4/2/1, threshold 8, decay 2, retention 20 ticks, and at most four samples. Exact LOS is required for visual contribution but never directly changes knowledge state.
- CQ-002 → add versioned records/unions in `SIR.Simulation` for sensor profile, stimulus, awareness/last-known state, engagement target/phase, reaction trigger/window/fact/reason, limits and counters; extend simulation state/events/replay and Match control observations additively. `SpatialQuery` owns geometry, Simulation owns world facts/knowledge/reaction, Match filters local observation, and Client/Web only project authority.
- CQ-003 → one `EngagementState option` per unit targets `KnownUnit`, a canonical bounded cell sector, or one canonical semantic edge. Area cells are unique and ordered row/column with a 256-cell cap; edge identity is its canonical endpoint pair plus spatial revision. Preparation is two eligible ticks, commitment one tick, reaction resolution one tick, and recovery four ticks for v1; invalid/unknown targets are typed rejections.
- CQ-004 → each tick orders: canonical input admission; movement intent and authoritative spatial transition; factual stimulus derivation; local awareness accumulation/decay; engagement preparation/maintenance; trigger eligibility snapshot; reaction commitments sorted by reactor id, engagement id, trigger kind, and source id; reaction physical resolution in that order with later invalidations becoming interruptions; ordinary physical actions; ordered event emission; recovery. Movement is not rewound, reactions never predate entry/exposure, and incapacitation/attention/posture/target/spatial invalidation interrupts unresolved windows.
- CQ-005 → extend the existing real planning/simulator route: player controls set attention, choose area/edge coverage, execute the plan, then the opposing player-emittable movement crosses it. Renderer-neutral projections expose sector/coverage glyphs, awareness/engagement/reaction facts and reasons; the existing replay timeline controls scrub the same authoritative events. Direct `Msg` injection or seeded mid-game state does not qualify.
- CQ-006 → add workload `sir-awareness-reaction-authoritative-tick-v1`: representative canonical scenarios plus a stress run of 200 moving units (100v100) in contact on an 80×80 two-level bounded map, 60-cell range, four samples, prepared unit/area/edge engagements, and simultaneous crossings. Record candidate pairs, sector survivors, LOS evaluations/crossed evidence, stimuli, episodes, engagements, triggers, commitments/resolutions/interruptions, facts/events/canonical bytes, allocation/GC, samples and host facts. Structural caps are 20,000 candidates, 5,000 LOS evaluations, 4,096 stimuli/episodes/facts/events, 256 area cells, and 262,144 canonical bytes per tick. After warm-up, awareness/reaction phase p95 MUST be at most 5 ms, representative full-tick p95 at most 20 ms, and stress worst tick below the 50 ms hard ceiling on the qualification host; headless evidence makes no compositor claim.
- CQ-007 → schema/profile/order/workload identities are explicit in canonical state/events and retained replay headers. Additive current v1 data uses a new awareness/reaction identity; old packages decode with no awareness payload and are never silently upgraded. Missing or mismatched retained identities return typed unavailable/mismatch results. Game.Core profile/package identity remains the inherited spatial binding rather than an awareness schema authority.

## Decisions
- **DEC-001** [CQ-001] [AMB:AMB-001] [FR-001] [FR-002] [FR-003] [FR-004]: Adopt the integer eight-direction sector/profile and bounded acquisition/decay/retention values stated in CQ-001; geometry evidence and knowledge transitions remain separate.
- **DEC-002** [CQ-002] [AMB:AMB-002] [FR-003] [FR-004] [FR-010]: Declare additive Simulation authority types first, extend replay/events and knowledge-filtered Match observations, and keep Client/Web projection-only.
- **DEC-003** [CQ-003] [AMB:AMB-003] [FR-005] [FR-006] [FR-007]: Permit exactly one unit, canonical-area, or canonical-edge engagement per unit with the stated bounds and 2/1/1/4 tick phase durations.
- **DEC-004** [CQ-004] [AMB:AMB-004] [FR-007] [FR-008] [FR-009]: Adopt the public tick order and canonical reaction sort from CQ-004; resolve reactions through existing physical combat and emit typed interruptions rather than rollback or stale fire.
- **DEC-005** [CQ-005] [AMB:AMB-005] [FR-012]: Qualify only the real planning/simulator/browser entry, player-emittable controls, authoritative projection, and timeline scrub route; test-only injection and mid-game seeds are excluded.
- **DEC-006** [CQ-006] [AMB:AMB-006] [FR-013] [FR-014]: Bind the exact 200-unit moving-contact workload, structural caps, 5/20/50 ms posture, host/candidate/workload identities, and protected-subject mutations before implementation acceptance.
- **DEC-007** [CQ-007] [AMB:AMB-007] [FR-011] [FR-014]: Version awareness/reaction canonical bytes and identities additively, preserve old replay meaning, and fail typed for unavailable/mismatched retained identities.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
- None. All seven source ambiguities are resolved by DEC-001 through DEC-007.

## Lifecycle Notes
- No accepted deferrals. Every issue acceptance boundary remains in this work item.
- Next lifecycle action: `fsgg-sdd checklist --work 182-awareness-reaction-windows`.
