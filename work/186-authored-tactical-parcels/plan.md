---
schemaVersion: 1
workId: 186-authored-tactical-parcels
title: Authored Tactical Parcels
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/186-authored-tactical-parcels/spec.md
sourceClarifications: work/186-authored-tactical-parcels/clarifications.md
sourceChecklist: work/186-authored-tactical-parcels/checklist.md
publicOrToolFacingImpact: true
---

# Authored Tactical Parcels Plan

Prose status: planned

## Source Snapshot
- spec: work/186-authored-tactical-parcels/spec.md sha256:913940d4b9d86eef4dfa845028e0a53ffc2001ab20c24dd495c8d9893ba83874 schemaVersion:1
- clarifications: work/186-authored-tactical-parcels/clarifications.md sha256:32d22a8a41846f92365aa9267f0b6c39d7dcaa262b0556c63861998e40111af3 schemaVersion:1
- checklist: work/186-authored-tactical-parcels/checklist.md sha256:974fdc8551c75e438487eef93bd25abd8455f84073f824d21b92a41cd1ff593f schemaVersion:1

## Plan Scope
- Add public schema-v1 parcel/environment values in `SIR.Domain`, pure assembly/validation/transition/query-adapter services in `SIR.Simulation`, and additive MapEditor/Simulator/Web projection and input seams.
- Declare `.fsi` surfaces first and keep Domain → Simulation → Client/Web dependency direction; use existing canonical encoding/hash, `SpatialQuery` dependency receipts, and physical-combat cover behavior rather than duplicating authority.
- Bind fixtures, tests, browser replay, performance, mutations, docs, feedback, and SDD evidence to the exact candidate with no synthetic satisfaction.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: Declare `TacticalEnvironment.fsi` in Domain with stable ids, cells/edges, plot/slot/connector/parcel/variant/placement, modality permeability, feature/state, directional cover, capability, knowledge, action, assembly, validation, and cost-counter records.
- PD-002 [AC-001] [FR-002] [DEC-001] complete: Implement canonical schema-v1 encoding/hash and deterministic assembly by sorted ids, explicit quarter-turn transforms, compatibility filtering, and exactly one product-owned SHA-256 addressed draw from seed plus stable slot id per selectable slot.
- PD-003 [AC-002] [FR-003] complete: Validate bounds/connectivity/footprints/connectors/objectives/permeability/cover/routes/dependencies with stable codes/order and hard caps before assembly acceptance.
- PD-004 [AC-003] [FR-004] [DEC-002] complete: Preserve seven independent modality fields and map committed environment edges into existing movement/vision/projectile `SpatialBoundary` projections without a general passability shortcut.
- PD-005 [AC-003] [FR-005] [DEC-002] [DEC-006] complete: Encode feature-specific legal transitions and knowledge-filtered capability descriptors; rejected actions return typed reasons and make no revision change.
- PD-006 [AC-004] [FR-006] [DEC-003] complete: Model directional cover material/integrity/penetration/protected directions and adapt its committed projectile-blocking state to physical combat cover resolution.
- PD-007 [AC-004] [FR-007] [DEC-003] complete: Apply one bounded targeted action, saturate damage, forbid propagation, increment revision once on change, and return sorted dependency keys plus inspected/changed/invalidated counters.
- PD-008 [AC-005] [FR-008] [DEC-004] complete: Add MapEditor canonical parcel import/export/validation/preview helpers and history-compatible commands using existing editor state boundaries; format-v4 maps remain readable and parcel schema is independently versioned.
- PD-009 [AC-005] [FR-009] [DEC-004] complete: Wire additive Web controls/projections through the production MapEditor reducer and Simulator handoff, then retain a replayable browser scenario using real events.
- PD-010 [AC-006] [FR-010] [DEC-005] complete: Commit canonical exterior and interior/breach fixtures and validate their cover, objectives, required routes, connectors, and breach opportunities.
- PD-011 [AC-007] [FR-011] [DEC-006] complete: Canonically encode capability observations only after knowledge projection, making indistinguishable hidden states byte-identical.
- PD-012 [AC-008] [FR-012] [DEC-005] complete: Add Release workload counters and host-qualified observations for 64 slots, 32 variants/role, 512 findings, 256 cache dependencies, 100 units, editor preview, and combat queries; retain red mutations for state/hash/locality/destruction bounds.
- PD-013 [AC-008] [FR-012] [DEC-007] complete: Bind initial-boot budget v2 at 1,250,000 bytes in the static artifact and throttled production-browser route, preserve deferred RulesExplorer loading, and retain tightened-static plus oversized-browser mutations that fail. Treat later growth as a defer-or-explicit-rebaseline decision, not as silent budget erosion.

## Contract Impact
- PC-001 [PD-001] [PD-002] publicSurface: `src/SIR.Domain/TacticalEnvironment.fsi` declares schema-v1 authored content, semantic state, canonical identity, capability, and action-result values before implementation.
- PC-002 [PD-003] [PD-004] [PD-007] simulationSurface: `src/SIR.Simulation/TacticalEnvironment.fsi` declares bounded assembly, validation, transition, dependency invalidation, spatial projection, fixture, and workload functions.
- PC-003 [PD-004] spatialAuthority: Environment projection feeds `SpatialQuery.evaluate` through `SpatialBoundary`; Game.Core `Cell`, `Edges`, `Los`, `Pathfinding`, and `MapAnalysis` semantics remain package-owned and are not copied.
- PC-004 [PD-006] combatAuthority: Environment cover state supplies existing `CombatRules.resolveCoverImpact` inputs and consumes its integrity outcome; no second mitigation pipeline is introduced.
- PC-005 [PD-008] editorSchema: `work/186-authored-tactical-parcels/contracts/tactical-environment-v1.md` defines additive parcel exchange/migration and existing map-format coexistence.
- PC-006 [PD-009] productionJourney: Browser actions map to production editor/simulator messages and canonical replay state; no direct JavaScript authority or test-only init exists.
- PC-007 [PD-012] performanceContract: `work/186-authored-tactical-parcels/contracts/environment-performance-v1.md` owns workload definitions, scales, structural budgets, host facts, and capability limits.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-003] [PC-001] semanticTest: Test canonical identity, seed replay, different-seed selection, transform placement, stale identity rejection, every validation finding, stable ordering, and degenerate bounds.
- VO-002 [PD-004] [PD-005] [PD-006] [PD-007] [PC-002] [PC-003] [PC-004] integrationTest: Test every feature state/modality, legal and rejected transitions, revision locality, cache dependency invalidation, path/LOS/projectile/cover changes, cover damage, and no propagation.
- VO-003 [PD-008] [PD-009] [PC-005] [PC-006] editorJourneyTest: Test versioned import/export/migration, preview, undo/redo, accessible keyboard/pointer mapping, production boot-to-edit-to-play journey, and exact event replay.
- VO-004 [PD-010] fixtureTest: Validate exterior and interior/breach catalogs, objective reachability, cover density, required routes, and canonical fixture bytes.
- VO-005 [PD-011] knowledgeTest: Compare canonical capability/observation bytes for indistinguishable knowledge and reject stale descriptor identity.
- VO-006 [PD-012] [PC-007] performanceTest: Run exact Release workloads, enforce structural ceilings, record counters/host facts/timing separately, and retain a deliberately naive subject that exceeds locality or action bounds.
- VO-007 [PD-012] mutationTest: Invert edge-state use, content-hash verification, dependency intersection, and one-target destruction bounds; require intended non-zero gates, then restore.
- VO-008 [PD-012] lifecycleTest: Run focused/full solution, native/Fable/browser, docs, feedback validation/invalidation, SDD evidence/verify/ship/refresh, exact-head CI, paths, and independent review gates.
- VO-009 [PD-013] deliveryTest: Measure the unmangled production initial route under the declared throttle, enforce the versioned 1,250,000-byte static/browser ceiling, load the deferred diagnostic path through normal UI, and require static-limit plus oversized-browser mutations to fail.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additive-versioned: Introduce environment schema v1 without reinterpreting legacy edge tuples; migration maps legacy wall/door/window values to explicit default semantic state and rejects unsupported future versions.
- PM-002 [PC-003] authority-preserving: Spatial queries continue to own path/LOS/trace/cache semantics; environment provides data and dependency tokens only.
- PM-003 [PC-005] coexistence: Existing MapEditor format v4 remains loadable/exportable while parcel content uses a separate schema-v1 canonical envelope.

## Generated View Impact
- GV-001 [PD-002] [PD-010] canonicalFixtures: Assembly, catalog, cross-runtime, replay, and mutation receipts regenerate from exact schema/source/seed identities.
- GV-002 [PD-012] evidenceViews: Performance, browser journey, tests, feedback, analysis, work model, summary, equivalent agent guidance, verify, and ship views bind current sources.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- The seeded SDD spec has no populated typed performance-intent front matter; DEC-005/FR-012 and the executable performance contract own the required declaration and baseline.
- Headless Release observations cover production update/projection routes but make no live-compositor claim.
- Persistence guidance informs versioned import/export purity only; this item does not add a filesystem save backend.
- Optional Governance pointers remain compatibility facts only; SDD reports readiness and does not substitute for runtime evidence.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 186-authored-tactical-parcels`.
