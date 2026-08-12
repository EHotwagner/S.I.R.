---
schemaVersion: 1
workId: 180-authoritative-spatial-query-foundation
title: Authoritative Spatial Query Foundation
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/180-authoritative-spatial-query-foundation/spec.md
sourceClarifications: work/180-authoritative-spatial-query-foundation/clarifications.md
sourceChecklist: work/180-authoritative-spatial-query-foundation/checklist.md
publicOrToolFacingImpact: true
---

# Authoritative Spatial Query Foundation Plan

Prose status: planned

## Source Snapshot
- spec: work/180-authoritative-spatial-query-foundation/spec.md sha256:e8d67175a4ed8f11e22bb39ab3dccb9769169285497d4e986ad827ac759ca80f schemaVersion:1
- clarifications: work/180-authoritative-spatial-query-foundation/clarifications.md sha256:b2cae02393f9d49369f0c9a1fc83e1d89c6979c72e548be96a00b37a4c8a06bf schemaVersion:1
- checklist: work/180-authoritative-spatial-query-foundation/checklist.md sha256:6d6ee8823b465ec09abc646b1a384acaea5df9203cbb230355e7d2a903a8aeec schemaVersion:1

## Plan Scope
- Declare portable identity and canonical-codec types in `SIR.Domain`, then public schema-v1 requests/results/explanations, a pure evaluator, package adapters, and cache state transitions in `SIR.Simulation` signature-first.
- Integrate existing movement/observation and bounded `SIR.Match` control access through the spatial service; keep the client and web route projection-only.
- Qualify native/Fable/Node/browser equality, knowledge non-interference, cached/uncached equivalence, local invalidation, fail-capable mutation gates, player reachability, and exact Release performance workloads.

## Plan Decisions
- PD-001 [AC-001] [AC-002] [FR-001] [DEC-001] complete: Add `SpatialQuery.fsi` before implementation with versioned IDs, modalities, profiles, footprints, projected world, requests, results, failures, explanations, dependencies, cache state, and canonical encoders; keep package-free IDs in Domain.
- PD-002 [AC-001] [FR-002] [DEC-002] complete: Normalize footprints to sorted unique relative cells and implement complete destination/swept-envelope validation with orthogonal semantic-edge checks for every footprint cell; diagonal steps evaluate both orthogonal decompositions and reject either blocked envelope.
- PD-003 [AC-002] [FR-003] [DEC-002] complete: Implement bounded deterministic A* through the package adapter, with prevalidated bounds, canonical neighbour/tie ordering, explicit expansion/result caps, and stable Found/Unreachable/Exhausted/InvalidInput outcomes.
- PD-004 [AC-001] [AC-002] [FR-004] [DEC-002] complete: Implement footprint-pair Supercover LOS/trace sampling, cover contributor extraction from crossed opaque edges/cells, and eight-way exposure directions while retaining stance/height as typed schema-v1 placeholders.
- PD-005 [AC-003] [FR-005] [DEC-004] complete: Construct canonical query keys from immutable identities, revisions, profiles, normalized footprints/endpoints, and requester-knowledge identity; construct dynamic dependency receipts only from disclosed occupancy/edge tokens actually traversed.
- PD-006 [AC-003] [FR-006] [DEC-004] complete: Model cache as immutable static/dynamic maps with pure lookup/store/invalidate functions; compare canonical evaluation payloads while excluding private hit/key/dependency metadata from public bytes.
- PD-007 [AC-004] [FR-007] [DEC-003] complete: Project the world to requester knowledge before validation/traversal and normalize public outcome/diagnostic/workload-class bytes; add paired indistinguishable-world fixtures and fixed-work padding/classification where secret state could otherwise alter observation.
- PD-008 [AC-005] [FR-008] complete: Use framework: FS.GG.Game.Core@0.13.0#Los.lineOfSightBy, framework: FS.GG.Game.Core@0.13.0#Pathfinding.astar, framework: FS.GG.Game.Core@0.13.0#Edges.edgeBetween, and package `Cell` only behind semantic adapters; compile shared product fixtures and clean package consumers for exact .NET/Fable/Node/browser bytes.
- PD-009 [AC-006] [AC-009] [FR-009] [DEC-001] complete: Replace migrated `Simulation` movement/observation decisions with the spatial service, expose a bounded Match request boundary, and give Client a renderer-neutral `SpatialDiagnosticProjection` with no map/edge algorithm.
- PD-010 [AC-006] [FR-010] [DEC-005] complete: Add a visible View → Spatial diagnostics command/panel reached from the real production entry after unit selection; render only authoritative explanation records and stable source/package identity.
- PD-011 [AC-007] [FR-011] [DEC-005] complete: Add an exact Release performance harness for 32x32 and 80x80 maps, selected-unit LOS, 64-step/4096-expansion route, one local invalidation, and deterministic 100/200-unit demand; assert structural ceilings and report latency/capability facts against 20/50/10/250/500 ms targets.
- PD-012 [AC-008] [FR-012] complete: Build a mutation harness that corrupts each footprint/edge/knowledge/revision/invalidation/order/package protected subject and unreadable fixture, observes a focused non-zero gate, restores the candidate, and records receipts.
- PD-013 [AC-009] [FR-013] complete: Emit an authority inventory and static scan rejecting copied JS/TS geometry, direct migrated package calls outside adapters, renderer-state authority, and undeclared legacy LOS/path code; preserve outside-slice behavior explicitly.
- PD-014 [AC-010] [FR-014] complete: Publish signatures/schema contracts/fixtures/docs, run focused and full conformance/docs/player/browser gates, update public-surface and Fable baselines, finalize feedback/evidence, refresh/verify/ship, and require exact-head hosted CI before review.

## Contract Impact
- PC-001 [PD-001] [PD-003] [PD-004] F# API: `SIR.Simulation.SpatialQuery` schema-v1 signatures declare stable request/result/evaluator/cache/service surfaces while product-neutral identities remain in `SIR.Domain`.
- PC-002 [PD-002] [PD-003] [PD-004] data-contract: `work/180-authoritative-spatial-query-foundation/contracts/spatial-query-v1.md` defines footprint, modality, transition, LOS/path/cover/exposure, ordering, bounds, and canonical-byte semantics.
- PC-003 [PD-005] [PD-006] [PD-007] data-contract: `work/180-authoritative-spatial-query-foundation/contracts/spatial-cache-knowledge-v1.md` defines key/dependency/invalidation and knowledge-safe public observation boundaries.
- PC-004 [PD-010] [PD-011] diagnostic-contract: `work/180-authoritative-spatial-query-foundation/contracts/spatial-diagnostics-performance-v1.md` defines the player route, renderer-neutral explanation fields, workloads, structural ceilings, latency targets, and capability receipt.
- PC-005 [PD-008] framework: FS.GG.Game.Core@0.13.0#Los.lineOfSightBy
- PC-006 [PD-008] framework: FS.GG.Game.Core@0.13.0#Pathfinding.astar
- PC-007 [PD-008] framework: FS.GG.Game.Core@0.13.0#Edges.edgeBetween

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-003] [PD-004] [PC-001] [PC-002] semanticTest: Run focused native tests for footprints, transition envelopes, terrain/edge modalities, diagonals, equal-cost ordering, bounds, LOS/trace, cover, exposure, and typed failures.
- VO-002 [PD-005] [PD-006] [PD-007] [PC-003] knowledgeCacheTest: Compare cached/uncached canonical bytes, prove stale revision rejection and dependency-local invalidation, and compare paired knowledge-indistinguishable worlds across every public observation.
- VO-003 [PD-008] [PC-005] [PC-006] [PC-007] crossRuntimeTest: Restore the exact package into clean caches and compare full package-adapter plus S.I.R. spatial fixture bytes on .NET, Fable/Node, and headless browser under pinned toolchain identities.
- VO-004 [PD-009] [PD-013] integrationTest: Prove Simulation movement/observation and Match control services delegate to the spatial authority; scan product/client sources for copied semantics, forbidden direct package calls, renderer calculation, and unclassified legacy authority.
- VO-005 [PD-010] [PC-004] playerJourneyTest: Boot the production browser, select a unit and open View → Spatial diagnostics using only player-emittable controls, then assert path/LOS/cover explanation/source/package content with no direct message injection or seeded state.
- VO-006 [PD-011] [PC-004] performanceTest: Run the exact Release candidate workloads, assert the 4096/4096/64/256/64-KiB structural ceilings, and record environment-qualified 20/50/10/250/500-ms observations.
- VO-007 [PD-012] gateMutationTest: Invert every added/modified gate's protected subject plus unreadable input, require the intended non-zero result, restore, and retain machine-readable mutation receipts.
- VO-008 [PD-014] lifecycleTest: Run full conformance, docs, public-surface and Fable baselines, deterministic fixture regeneration, schema-v2 feedback validation, SDD evidence/verify/ship, exact-head hosted CI, and path/claim validators.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] compatible-additive: Add schema-v1 APIs and delegate the existing minimal movement/observation slice without changing input/event wire forms; new diagnostics are additive projections.
- PM-002 [PC-002] bounded-authority: Only the declared movement/LOS/path/cover/exposure slice is authoritative; physical damage and multi-agent scheduling remain outside, and every remaining ad hoc geometry path is inventoried as legacy or rejected.
- PM-003 [PC-003] cache-transparent: Cache state is an optimization with no public semantic/timing/detail distinction; persistence is not introduced and old state needs no migration.

## Generated View Impact
- GV-001 [PD-008] canonicalFixtures: Spatial package-adapter and product fixture bytes, identities, and first-divergence reports regenerate deterministically from exact package/product sources.
- GV-002 [PD-010] diagnosticViews: Client/web selected-unit diagnostics project the authoritative explanation contract and report unavailable/invalid results explicitly.
- GV-003 [PD-014] lifecycleViews: Analysis, work model, summary, equivalent Claude/Codex guidance, verify, ship, and committed receipts refresh from current authored sources/evidence.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Performance intent is carried by DEC-005, FR-011, PC-004, and VO-006 because this scaffold has no populated typed performance-intent front matter; the executable Release receipt remains binding.
- Optional Governance pointers remain compatibility facts only; SDD readiness never substitutes for runtime/player/package evidence.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 180-authoritative-spatial-query-foundation`.
