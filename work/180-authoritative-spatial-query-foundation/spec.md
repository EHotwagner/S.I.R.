---
schemaVersion: 1
workId: 180-authoritative-spatial-query-foundation
title: Authoritative Spatial Query Foundation
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Authoritative Spatial Query Foundation Specification

Prose status: specified

## User Value
Players, simulation, control modules, and clients receive one deterministic, explainable, knowledge-safe answer for footprint-aware movement, visibility, paths, exposure, and cover instead of contradictory feature-local geometry.

## Scope
- SB-001: Add versioned S.I.R.-owned query inputs/results for footprint-aware line traces, exact LOS, bounded paths, reachability, movement cost, crossed cells/edges, cover contributors, and exposure directions.
- SB-002: Evaluate complete square footprints and transition envelopes against terrain, diagonal corner rules, occupancy, and modality-specific semantic-edge permeability.
- SB-003: Bind evaluation and caches to immutable map/ruleset identity, spatial revision, stance/height placeholders, facing, movement/sensor profile, requester knowledge, and relevant dynamic state.
- SB-004: Adapt only published package-classified `FS.GG.Game.Core` primitives, expose bounded renderer-neutral explanations/services, and qualify the same portable F# semantics across .NET, Fable/Node, and a production browser route.

## Non-Goals
- SB-005: Do not implement physical damage resolution, visual overlay styling, arbitrary 3D levels, or full multi-agent route scheduling.
- SB-006: Do not broaden `FS.GG.Game.Core` compatibility claims, add sibling project references, copy its algorithms, or create TypeScript/JavaScript geometry authority.
- SB-007: Do not expose omniscient state through a diagnostic convenience path; privileged development inspection must be explicit and unavailable to ordinary requester contexts.

## User Stories
- US-001 (P1): As a player, I receive movement, visibility, and cover behavior that accounts for my unit's entire footprint and the battlefield's semantic edges.
- US-002 (P1): As a control-module author, I can request bounded routes, reachability, LOS, exposure, and costs through one machine-readable service with stable failure/exhaustion results.
- US-003 (P1): As a client user, I can inspect the selected unit's exact authoritative path/LOS/cover explanation without the browser recomputing geometry.
- US-004 (P1): As a maintainer, I can change spatial state locally, observe bounded cache invalidation, and prove cached/uncached plus .NET/Fable results remain canonical-byte equal.
- US-005 (P1): As a player with partial knowledge, I cannot infer unknown doors, units, breaches, or obstructions from results, explanations, diagnostics, timing classes, or cache metadata.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001] [FR-002]: Given 1x1 and multi-cell actors, when traces, LOS, movement, and paths are evaluated across walls, doors, windows, terrain, diagonals, and partial exposure, then every footprint cell/transition edge follows the declared modality rule and deterministic tie-break.
- AC-002 [US-002] [FR-003] [FR-004]: Given reachable, unreachable, and budget-exhausted goals, when a control module submits a bounded query, then it receives stable typed paths, costs, crossed cells/edges, cover/exposure inputs, or a typed bounded failure with no unbounded work.
- AC-003 [US-004] [FR-005] [FR-006]: Given identical inputs, when queries execute cached and uncached, then their canonical result/explanation bytes match; changing one spatial revision invalidates only dependent static/dynamic entries and stale keys cannot hit.
- AC-004 [US-005] [FR-007]: Given two authoritative worlds indistinguishable under requester knowledge, when every query/result/failure/diagnostic/cache observation is compared, then the knowledge-filtered public bytes and disclosed timing class are identical.
- AC-005 [US-004] [FR-008]: Given every adopted package primitive and S.I.R. adapter fixture, when clean package-only .NET, Fable/Node, and browser qualification runs under pinned identities, then complete canonical byte streams match and no sibling/copy authority is present.
- AC-006 [US-003] [FR-009] [FR-010]: Given the production browser entry and player-emittable controls, when a unit is selected and spatial diagnostics are opened, then exact route/LOS/cover explanations identify inputs, footprint samples, crossed edges, decisions, truncation, revision, and knowledge policy without browser geometry code.
- AC-007 [US-004] [FR-011]: Given representative, maximum-map, invalidation, and 100/200-unit workloads, when Release qualification runs, then structural caps and declared latency budgets pass with environment/capability facts recorded.
- AC-008 [US-004] [FR-012]: Given focused mutations that ignore a footprint cell, semantic edge, knowledge filter, revision key, dynamic invalidation dependency, deterministic tie-break, or package adapter, when the owning gate runs, then it fails for the mutated subject and passes restored.
- AC-009 [US-001] [US-002] [FR-013]: Given the existing simulation/control boundaries, when spatial queries are integrated, then legacy ad hoc LOS/path decisions in the migrated slice are removed or explicitly outside authority and all consumers use typed services rather than renderer state.
- AC-010 [US-004] [FR-014]: Given an exact candidate commit, when lifecycle and release gates run, then signatures, schema/fixtures, docs, feedback, evidence, verify, ship, full conformance, docs, and hosted CI are current and passing.

## Functional Requirements
- FR-001: The system MUST declare stable typed F# identities, profiles, keys, requests, results, failures, explanations, cells, crossed edges, cover contributors, and exposure directions for line trace, exact LOS, bounded path, reachability, movement cost, cover, and exposure queries. (Stories: US-001, US-002; Acceptance: AC-001, AC-002)
- FR-002: Evaluation MUST inspect every square-footprint origin/target/transition cell, enforce diagonal corner and transition-envelope rules, apply terrain plus modality-specific semantic-edge permeability, and use a documented deterministic ordering/tie-break. (Stories: US-001; Acceptance: AC-001)
- FR-003: Path and reachability evaluation MUST accept explicit expansion/cost/result bounds and return stable `Found`, `Unreachable`, `Exhausted`, or invalid-input outcomes with ordered paths, costs, crossed cells/edges, and no unbounded allocation or search. (Stories: US-002; Acceptance: AC-002)
- FR-004: LOS, trace, cover, and exposure evaluation MUST account for complete origin/target footprints, stance/height placeholders, facing and sensor profiles, partial visibility, crossed semantic edges, and deterministic cover/exposure contributors without resolving physical damage. (Stories: US-001, US-002; Acceptance: AC-001, AC-002)
- FR-005: Every query key MUST include immutable map and ruleset identity, spatial revision, normalized footprint/position/facing, movement or sensor profile, requester knowledge identity/revision, and only the dynamic occupancy/edge dependencies relevant to that query. (Stories: US-004; Acceptance: AC-003)
- FR-006: Static geometry caching MUST be separated from locally invalidated dynamic occupancy/edge state; cache hits and misses MUST emit byte-identical public results/explanations, reject stale revisions, and disclose only bounded non-secret cache classifications. (Stories: US-004; Acceptance: AC-003)
- FR-007: A requester-knowledge projection MUST be applied before public query evaluation/encoding so unknown doors, units, breaches, obstructions, and their absence cannot be inferred through values, path shape, failure variants, explanations, diagnostics, timing classes, cache keys, hit metadata, or invalidation counts. (Stories: US-005; Acceptance: AC-004)
- FR-008: The portable implementation MUST consume `FS.GG.Game.Core@0.13.0` package-only and use only explicitly `LockstepExact` `Cell`, `Edges.edgeBetween`, `Los.lineOfSightBy`, and `Pathfinding.astar` surfaces behind S.I.R. semantic adapters, with exact package/profile/fixture/toolchain identities and clean-consumer .NET/Fable/Node/browser canonical evidence. (Stories: US-004; Acceptance: AC-005)
- FR-009: Simulation and control modules MUST consume bounded machine-readable spatial services; client projections MUST consume renderer-neutral explanation/result records and MUST NOT calculate authoritative geometry in JavaScript or presentation code. (Stories: US-002, US-003; Acceptance: AC-006, AC-009)
- FR-010: The production browser MUST expose a player-reachable selected-unit spatial diagnostics route showing query kind, normalized inputs, authoritative result, footprint samples, crossed cells/edges, cover/exposure contributors, revision/knowledge policy, truncation/failure, and source/package identity. (Stories: US-003; Acceptance: AC-006)
- FR-011: Release qualification MUST measure representative and maximum-map selected-unit LOS, bounded route preview, local invalidation, and 100/200-unit demand; declare deterministic structural ceilings and environment-qualified latency budgets before implementation acceptance. (Stories: US-004; Acceptance: AC-007)
- FR-012: Each added or modified spatial gate MUST retain a protected-subject mutation proving red for ignored footprint, edge permeability, knowledge filtering, revision identity, local invalidation, ordering, and package/runtime divergence, including unreadable/malformed fixture failures. (Stories: US-004; Acceptance: AC-008)
- FR-013: The migrated authoritative slice MUST replace or delegate existing ad hoc LOS/path logic, publish an authority inventory, and fail qualification on copied TypeScript/JavaScript semantics, unclassified Game.Core use, renderer-state dependence, or an undeclared legacy decision. (Stories: US-001, US-002; Acceptance: AC-009)
- FR-014: The change MUST ship public signatures, versioned canonical schemas/fixtures, design/performance/browser documentation, deterministic generated evidence, current SDD views, schema-v2 feedback, full native/Fable/browser/docs conformance, and exact-head hosted CI. (Stories: US-004; Acceptance: AC-010)

## Ambiguities
- AMB-001: Which project owns the portable public spatial contracts/evaluator while preserving Domain→Simulation→Match→Client dependency direction and Fable compilation?
- AMB-002: What exact semantic-edge modalities, diagonal transition envelope, footprint anchoring, and equal-cost path ordering define the first authoritative schema?
- AMB-003: How is requester knowledge normalized so hidden authoritative differences cannot leak while still returning useful bounded reasons and explanations?
- AMB-004: Which cache partition/key/dependency receipt enables local invalidation without exposing secret occupancy/edge changes or permitting stale hits?
- AMB-005: What exact representative/maximum-map workloads, structural ceilings, latency budgets, and browser diagnostic route qualify the foundation?

## Public Or Tool-Facing Impact
- Tier 1: new public F# spatial-query signatures, canonical result/explanation/cache schemas, service integration, browser diagnostics, fixtures, performance budgets, and exact multi-runtime CI gates.
- Existing replay/control protocols remain compatible unless explicitly extended with versioned optional spatial diagnostics; presentation receives projections, never authority.

## Lifecycle Notes
- Resolve all five ambiguities in `clarify`; no ambiguity is accepted as an unowned deferral.
- The performance-first gate is binding before implementation: the plan must name bounded algorithms, allocation/result caps, invalidation granularity, workloads, and measured thresholds.
- Next lifecycle action: `fsgg-sdd clarify --work 180-authoritative-spatial-query-foundation`.
