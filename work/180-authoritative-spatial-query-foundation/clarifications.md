---
schemaVersion: 1
workId: 180-authoritative-spatial-query-foundation
title: Authoritative Spatial Query Foundation
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/180-authoritative-spatial-query-foundation/spec.md
publicOrToolFacingImpact: true
---

# Authoritative Spatial Query Foundation Clarifications

## Source Specification
- work/180-authoritative-spatial-query-foundation/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001] blocking answered: Where do portable contracts, evaluation, and package adapters live?
- CQ-002 [AMB:AMB-002] blocking answered: What are the first-schema modality, footprint, diagonal, and ordering rules?
- CQ-003 [AMB:AMB-003] blocking answered: At what boundary is requester knowledge applied and what can failures disclose?
- CQ-004 [AMB:AMB-004] blocking answered: How are static/dynamic cache identity, dependencies, and invalidation represented?
- CQ-005 [AMB:AMB-005] blocking answered: Which exact workloads, budgets, and player/browser route qualify the feature?

## Answers
- CQ-001: Product-neutral identity/profile/value records live in `SIR.Domain`; the public spatial schema, pure evaluator, cache state/update functions, and Game.Core semantic adapters live in `SIR.Simulation` because it already owns the package dependency. `SIR.Match` exposes bounded control services and `SIR.Client`/Web only project results.
- CQ-002: Schema v1 supports GroundMovement, Vision, and ProjectileTrace modalities. Footprints are normalized sorted unique square cells relative to a top-left anchor. A transition is legal only when every destination cell is in bounds/passable/unoccupied and every swept orthogonal edge permits the modality; diagonal movement checks both orthogonal envelopes and forbids corner cutting. Equal-cost ordering is cost, row, column, then canonical predecessor bytes.
- CQ-003: Callers provide a normalized `RequesterKnowledge` projection/identity/revision. The service evaluates only that projected world; unknown features become the same public `Unknown` cells/edges before traversal. Public failures are limited to invalid input, found, unreachable, and exhausted with fixed diagnostic vocabulary and workload class. Omniscient developer inspection is a separately typed privileged context and is not reachable from the player route.
- CQ-004: Static geometry cache entries key map/ruleset identities, spatial revision, normalized static cells/edges, modality/profile, and normalized endpoints/footprints. Dynamic entries additionally carry a dependency receipt of disclosed occupancy/edge revision tokens; invalidation intersects changed disclosed tokens. Public explanations expose revision/knowledge identity and `Cached`/`Uncached` evaluation equivalence but no internal key, hit timing, secret dependency, bucket size, or invalidation count.
- CQ-005: Qualify 32x32 representative and 80x80 maximum maps; selected-unit LOS and a 64-step/4096-expansion route preview; one-cell/edge local invalidation; deterministic batches for 100 and 200 units. Structural ceilings are 4096 expansions, 4096 crossed items, 64 route cells, 256 footprint samples, and 64 KiB canonical explanation. Release observations target 20 ms LOS, 50 ms route, 10 ms invalidation, 250 ms/500 ms 100/200-unit batches on the recorded host. The player route is View → Spatial diagnostics after selecting a unit in the real simulator.

## Decisions
- DEC-001: [AMB:AMB-001] complete. Use layered `SIR.Domain` identities, `SIR.Simulation.SpatialQuery` public signatures/evaluator/cache, `SIR.Match` bounded service, and renderer-neutral client projections; package types do not leak into Domain.
- DEC-002: [AMB:AMB-002] complete. Adopt schema-v1 GroundMovement/Vision/ProjectileTrace modality rules, sorted square footprints, complete transition envelopes, strict no-corner-cutting diagonals, and canonical cost/row/column/predecessor ordering.
- DEC-003: [AMB:AMB-003] complete. Normalize requester knowledge before evaluation, encode only fixed public outcomes/vocabulary/workload class, and make privileged omniscience an explicit non-player context.
- DEC-004: [AMB:AMB-004] complete. Partition static geometry from dynamic occupancy/edge cache state and invalidate only entries whose declared, knowledge-filtered dependency receipts intersect changed revision tokens; hide all secret/internal cache observations.
- DEC-005: [AMB:AMB-005] complete. Bind the 32x32/80x80, 64-step/4096-expansion, local invalidation, and 100/200-unit workloads plus structural/latency ceilings above; qualify the real View → Spatial diagnostics player route.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
None. AMB-001 through AMB-005 are resolved by DEC-001 through DEC-005.

## Lifecycle Notes
- No accepted deferrals; all issue acceptance obligations remain in this work item.
- Next lifecycle action: `fsgg-sdd checklist --work 180-authoritative-spatial-query-foundation`.
