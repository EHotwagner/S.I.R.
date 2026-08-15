---
schemaVersion: 1
workId: 186-authored-tactical-parcels
title: Authored Tactical Parcels
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Authored Tactical Parcels Specification

Prose status: specified

## User Value
Designers can author deterministic cover-dense exterior and interior parcel maps whose doors, windows, walls, cover, and breaches behave consistently in play.

## Scope
- SB-001: Define versioned authored plots, parcel slots, connectivity, roles, variants, seeded selection, validation, immutable content identity, and two representative parcel sets.
- SB-002: Extend environment volumes and semantic edges with stable feature/state/permeability/cover/capability contracts consumed by authoritative spatial and combat services.
- SB-003: Add bounded targeted environment transitions, dependency-indexed invalidation, editor authoring/migration/history/preview, and a production browser journey with exact replay.
- SB-004: Qualify portable deterministic semantics, content identity, production-route performance, accessibility, documentation, and fail-capable gates.

## Non-Goals
- SB-005: Do not add arbitrary procedural maps, unconstrained parcel synthesis, emergent structural collapse, physics debris, unbounded vertical levels, or final art production.
- SB-006: Do not move geometry or combat authority into Client/Web code, duplicate Game.Core algorithms, or interpret stale content under a new schema.

## User Stories
- US-001 (P1): As a designer, I can compose validated plots from seeded parcel variants and receive stable content identities and actionable failures.
- US-002 (P1): As a player, I can open, damage, breach, and destroy declared environment features and immediately observe consistent movement, sight, projectile, effect, sound, cover, and interaction behavior.
- US-003 (P1): As a map-editor user, I can author, migrate, undo, redo, preview, export, and import environment content through accessible keyboard and pointer routes.
- US-004 (P1): As a maintainer, I can prove local invalidation, bounded destruction cost, deterministic replay, cross-runtime equality, and representative performance with gates that fail on semantic regressions.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001] [FR-002]: Given one versioned plot, parcel catalog, and seed, when assembly runs repeatedly in .NET and Fable, then canonical bytes and immutable content hashes are identical and stale or mismatched identities are rejected.
- AC-002 [US-001] [FR-003]: Given disconnected slots, impossible footprints, blocked objectives, cover gaps, invalid permeability, or unreachable required routes, when validation runs, then it returns stable bounded findings and rejects the content.
- AC-003 [US-002] [FR-004] [FR-005]: Given doors, windows, walls, and cover in each supported state, when every modality queries them, then movement, sight, projectile, area/effect, sound, cover, and capability-specific interaction follow the declared permeability and observation policy.
- AC-004 [US-002] [FR-006] [FR-007]: Given an opening, breach, cover-damage, or cover-destruction action, when it commits, then spatial revision advances once, path/LOS/trace/cover answers change, and only query entries declaring the changed dependencies are invalidated.
- AC-005 [US-003] [FR-008] [FR-009]: Given the production map editor, when keyboard and pointer users author environment state, undo/redo, export, import/migrate, preview, assemble, and use it in play, then the exact canonical result replays without a test-only route.
- AC-006 [US-001] [US-002] [FR-010]: Given the exterior and interior/breach fixture catalogs, when scenario qualification runs, then each supplies valid cover-dense routes, objectives, interactions, and bounded breach opportunities.
- AC-007 [US-002] [FR-011]: Given a requester with partial knowledge, when capability descriptors and observations are projected, then every interactable is machine-readable and versioned while hidden feature state and unavailable actions do not leak.
- AC-008 [US-004] [FR-012]: Given representative and maximum assembly, validation, preview, invalidation, combat-query, and production initial-boot workloads, when Release qualification and protected-subject mutations run, then structural/timing/versioned delivery budgets pass and ignoring edge state, content identity, dependency locality, destruction bounds, or the initial-route byte ceiling makes the owning gate red.

## Functional Requirements
- FR-001: The system MUST define schema-v1 authored plot, slot, connection, role, parcel, variant, transform, placement, assembly, validation, and immutable content-identity values with canonical ordering and migration posture. (Stories: US-001; Acceptance: AC-001)
- FR-002: Assembly MUST select compatible variants from a caller-supplied seeded stream using stable total ordering, enforce footprints/connectors/roles, return explicit bounds/counters, and produce canonical bytes plus a content hash that is identical across .NET and Fable and rejects stale identity. (Stories: US-001, US-004; Acceptance: AC-001)
- FR-003: Validation MUST boundedly detect duplicate or disconnected slots, impossible footprints/transforms, connector mismatches, blocked objectives, invalid permeability, directional-cover gaps, unreachable required routes, and invalid dependency references with ordered actionable findings. (Stories: US-001; Acceptance: AC-002)
- FR-004: Environment volumes and semantic edges MUST carry stable feature type and state plus independent permeability for movement, sight, projectile, area/effect, sound, cover, and named capability interaction; callers MUST NOT collapse these modalities to one passability flag. (Stories: US-002; Acceptance: AC-003)
- FR-005: Doors, windows, walls, and cover MUST expose only valid open, closed, damaged, breached, and destroyed transitions for their feature type and MUST provide versioned knowledge-filtered capability descriptors and observations. (Stories: US-002; Acceptance: AC-003)
- FR-006: Directional cover MUST declare material, integrity, penetration resistance, protected directions, damage behavior, and query dependents; physical combat MUST consume the same committed state used by projectile and cover queries. (Stories: US-002; Acceptance: AC-004)
- FR-007: Targeted environment actions MUST be pure, bounded, idempotency-safe transitions that never propagate collapse, advance spatial revision exactly once on change, and return the exact invalidation dependency set and cost counters. (Stories: US-002, US-004; Acceptance: AC-004)
- FR-008: The map editor MUST author plots, slots, parcels, feature states, modality permeability, cover, and interactions with validation and preview, and every mutation MUST participate in bounded undo/redo plus versioned import/export/migration. (Stories: US-003; Acceptance: AC-005)
- FR-009: Production browser keyboard and pointer routes MUST create or load parcel content, edit an environment feature, preview/assemble it, use door/breach/destruction actions in play, and replay the exact canonical state without bypassing production mapping, dispatch, update, or fixed-step seams. (Stories: US-003, US-004; Acceptance: AC-005)
- FR-010: The repository MUST include at least one cover-dense exterior parcel set and one interior/breach parcel set whose canonical fixtures satisfy the scenario catalog and validation rules. (Stories: US-001, US-002; Acceptance: AC-006)
- FR-011: Every interactable MUST expose a stable schema-v1 capability descriptor and requester-knowledge-filtered observation; unknown state, capabilities, dependencies, and invalidation counts MUST not leak through public bytes or diagnostics. (Stories: US-002; Acceptance: AC-007)
- FR-012: Release qualification MUST traverse production assembly, validation, editor preview, local invalidation, representative combat-query, and production initial-boot routes with declared maximum scales, structural counters, host facts, latency budgets, and a versioned 1,250,000-byte initial-route ceiling; this ceiling is not a global lifetime limit, so later growth MUST defer code or explicitly rebaseline the versioned contract, and each added or modified gate MUST retain a mutation proving it fails when edge state, content hash, bounded destruction cost, local dependency invalidation, or initial-route size is ignored. (Stories: US-004; Acceptance: AC-008)

## Ambiguities
- AMB-001: What canonical ordering, transform set, seed consumption rule, and content-hash payload define assembly identity?
- AMB-002: What feature/state transition matrix and modality-permeability defaults are legal for doors, windows, walls, and cover?
- AMB-003: How are cover damage, breach cost, revision advancement, dependency keys, and local invalidation bounded and ordered?
- AMB-004: Which editor operations and production controls constitute the required accessible author-to-play journey and replay boundary?
- AMB-005: What exact fixture scales, structural ceilings, timing budgets, runtime equality route, and mutations qualify acceptance?

## Public Or Tool-Facing Impact
- Tier 1: new public Domain/Simulation environment schema and transition surfaces, extended map-editor schema/workflows, browser controls/projections, canonical fixtures, migrations, performance contract, documentation, and cross-runtime evidence.
- Existing spatial-query and physical-combat schemas remain version-compatible and consume additive environment projections rather than acquiring a second geometry authority.

## Lifecycle Notes
- All five ambiguities must be resolved; no issue requirement is accepted as a downstream deferral.
- The performance-first gate requires declared workloads and a focused baseline before implementation.
- Next lifecycle action: `fsgg-sdd clarify --work 186-authored-tactical-parcels`.
