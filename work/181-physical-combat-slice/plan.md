---
schemaVersion: 1
workId: 181-physical-combat-slice
title: Physical Combat Slice
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/181-physical-combat-slice/spec.md
sourceClarifications: work/181-physical-combat-slice/clarifications.md
sourceChecklist: work/181-physical-combat-slice/checklist.md
publicOrToolFacingImpact: true
---

# Physical Combat Slice Plan

Prose status: planned

## Source Snapshot
- spec: work/181-physical-combat-slice/spec.md sha256:5cc62476900c0f3a9d7b05366135c6d423db3f092779ab7f0b22d13edad0c669 schemaVersion:1
- clarifications: work/181-physical-combat-slice/clarifications.md sha256:660e99add891a9950f6d6c869985c949e5dce71bdcaf3e2ef840944fb1b3b0e7 schemaVersion:1
- checklist: work/181-physical-combat-slice/checklist.md sha256:99b81ad6a9d31fd81ee9671c9de61df2b4e0cdf80cd2743b493ffd777ad3285f schemaVersion:1

## Plan Scope
- Implement a schema-v1 physical combat aggregate in `SIR.Simulation`, integrate it into the deterministic tick/replay and bounded Match service, and expose renderer-neutral Client/Web projections and a real player journey.
- Declare `.fsi` contracts first, retain Domain → Simulation → Match → Client dependency direction, and keep `SpatialQuery` plus the executable rules registry as the only geometry/rule authorities.
- Bind all evidence to exact package, source, fixture, runtime, production artifact, and workload identities; no synthetic pass or implicit deferral is accepted.

## Plan Decisions
- PD-001 [AC-001] [AC-005] [FR-001] [DEC-001] complete: Declare `CombatModel.fsi` with versioned profiles, damage/armor/wound/suppression/cover state, attack requests, ordered facts, explanations, results, limits, and pure resolution/recovery functions before implementation.
- PD-002 [AC-001] [FR-002] [DEC-002] complete: Build direct-delivery evidence only through `SpatialQuery.evaluate` using ProjectileTrace/Cover requests; join occupants/destructibles onto ordered crossed cells/edges and fail typed when spatial evidence is invalid/exhausted.
- PD-003 [AC-002] [FR-003] [DEC-003] complete: Resolve directional armor after contact/cover and before HP using body-facing relative arc, integrity-scaled rating, damage type, penetration, and bounded retained-effect formula; emit all effective operands.
- PD-004 [AC-002] [AC-008] [FR-004] [DEC-003] complete: Centralize bounded integer/fixed-point helpers, canonical sort keys, saturating arithmetic, rounding, and explicit limits; scan authoritative code for float/ambient RNG and prove deterministic overflow behavior.
- PD-005 [AC-003] [FR-005] [DEC-001] [DEC-003] complete: Extend `UnitState` additively with armor, wound list, incapacity, and suppression while retaining HP; apply damage, wound threshold, and incapacity in ordered consequence facts without aliasing fields.
- PD-006 [AC-003] [FR-006] [DEC-003] complete: Accumulate suppression independently, derive typed capability/timing bands, and recover five bounded points during commit after attack consequences; encode state/events so replay sees each distinction.
- PD-007 [AC-004] [FR-007] [DEC-003] complete: Define exactly four canonical weapon profiles and route their point/direct, direct-area, anti-armor, and lobbed-area semantics through one resolver and one versioned rules registry.
- PD-008 [AC-004] [FR-008] [DEC-002] complete: Enumerate recipients by trace index/cell/entity-kind/id and areas by distance/row/column; permit friendly/civilian hits, apply cover-object integrity, and increment spatial revision/remove projectile blocking only after destruction commits.
- PD-009 [AC-005] [FR-009] [DEC-004] complete: Emit a bounded ordered `CombatFact` list plus root `RuleApplication`, spatial evidence, effective profile/cover/armor/consequence operands, source symbols, and complete current rules identity through renderer-neutral projections.
- PD-010 [AC-006] [FR-010] [DEC-004] complete: Version canonical combat state/event/fact bytes and replay bindings; retain v1/v2/v3 readers, bind current combat/spatial/rules identities, and reject mismatched/unavailable historical packages without reinterpretation.
- PD-011 [AC-007] [FR-011] [DEC-005] complete: Add Client projection and a production Web combat panel/controls reachable from the real app entry; the browser journey starts a scenario, chooses profiles, attacks, and inspects every required result using DOM/player controls only.
- PD-012 [AC-008] [FR-012] [DEC-006] complete: Add an exact Release workload for the canonical scenario matrix and a 100-unit/50-attack stress tick, record counters/host facts, enforce structural ceilings, and compare observations to 20/50-ms targets without compositor claims.
- PD-013 [AC-009] [FR-013] [DEC-006] complete: Add focused semantic, integration, replay, cross-runtime, browser, and performance gates plus scripted protected-subject/unreadable-input mutations for collision, cover, armor, suppression, ordering, identity, and runtime equality.
- PD-014 [AC-010] [FR-014] [DEC-004] complete: Rebind every changed `CombatRules` source/implementation/package identity to the exact candidate, regenerate manifests/fixtures/public surface/docs, and complete package-only conformance, feedback, SDD, production delivery, docs, CI, independent review, and landing gates.

## Contract Impact
- PC-001 [PD-001] [PD-003] [PD-005] [PD-006] publicSurface: `work/181-physical-combat-slice/contracts/physical-combat-v1.md` and `.fsi` files declare typed schema-v1 state, request/result/fact, limits, resolver, recovery, and simulation event surfaces.
- PC-002 [PD-002] [PD-008] spatialAuthority: Combat consumes `SIR.Simulation.SpatialQuery.evaluate`, `canonicalResultBytes`, schema/profile/package identity, crossed cells/edges, and cover contributors; no duplicate Game.Core call or geometry algorithm enters combat.
- PC-003 [PD-007] [PD-009] [PD-014] rulesIdentity: Expand `CombatRules` registry/coverage/current-source metadata, derive identities from exact implementation artifacts, and preserve the #194 semantic-vs-documentation digest matrix and non-recursive manifest contract.
- PC-004 [PD-010] replaySchema: `work/181-physical-combat-slice/contracts/combat-replay-v1.md` defines versioned combat state/event/fact bytes plus exact combat/spatial/rules bindings and typed historical-unavailable behavior.
- PC-005 [PD-011] browserProjection: Client/Web receives only authoritative projections and exposes a real input path for profile selection, attack, scene/result state, and ordered explanation/source navigation.
- PC-006 [PD-012] performanceContract: `work/181-physical-combat-slice/contracts/combat-performance-v1.md` owns workload definitions, digests, caps, counters, host facts, observations, and capability limits.
- PC-007 [PD-002] framework: FS.GG.Game.Core@0.13.0#Los.lineOfSightBy
- PC-008 [PD-002] framework: FS.GG.Game.Core@0.13.0#Edges.edgeBetween

## Verification Obligations
- VO-001 [PD-001] [PD-003] [PD-004] [PD-005] [PD-006] [PD-007] [PC-001] semanticTest: Run canonical scenario tests for phase order, four profiles, direction/integrity/penetration, separate HP/wounds/incapacity/suppression, recovery, bounds, saturation, and typed failures.
- VO-002 [PD-002] [PD-008] [PC-002] integrationTest: Assert every direct/cover decision cites inherited spatial canonical evidence, intervening/collateral/destructible ordering is stable, and no combat/client source duplicates geometry or calls unclassified package surfaces.
- VO-003 [PD-007] [PD-009] [PD-014] [PC-003] rulesTest: Validate registry coverage/source links, exact identity-change matrix, manifest self-hash non-recursion, current-source correspondence, deterministic generation, and original #194 retained-package fixtures.
- VO-004 [PD-010] [PC-004] replayTest: Golden-test schema-v1 combat bytes, native/Fable replay and timeline seeking, current/retained package resolution, identity mismatches, malformed limits, and typed unavailable historical packages.
- VO-005 [PD-011] [PC-005] playerJourneyTest: Boot the built production browser at the real entry and use player controls to demonstrate all four profiles plus visible trace/contact/cover/armor/HP/wound/incapacity/suppression/cover-integrity/explanation/source state.
- VO-006 [PD-012] [PC-006] performanceTest: Run the exact Release scenario matrix and 100-unit/50-attack workload, enforce 256/256/256/4096/64-KiB structural caps, and record environment-qualified 20/50-ms observations and 50-ms tick posture.
- VO-007 [PD-013] gateMutationTest: Break every added/modified gate's protected subject once, include unreadable input, require the intended non-zero result, restore, and retain machine-readable mutation receipts.
- VO-008 [PD-014] lifecycleTest: Run public-surface, solution, full conformance, package-only native/Fable/Node/browser, production delivery, docs, schema-v2 feedback, SDD evidence/verify/ship/refresh, exact-head CI, path/claim, and independent-review validators.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additive-versioned: Extend current unit/board/event/replay contracts with explicit schema-v1 combat fields; retain older replay decoders and reject unsupported new variants before mutation.
- PM-002 [PC-002] authority-preserving: Replace the current adjacency/transparent-function attack shortcut with inherited spatial evidence; old behavior is not retained as an alternate authority.
- PM-003 [PC-003] exact-rebind: A semantic implementation change intentionally changes implementation/semantic/manifest identities and all current fixtures/source links together; retained historical packages remain immutable.
- PM-004 [PC-005] projection-only: Browser state is additive and renderer-neutral; no Web/TypeScript combat calculation or test-only route is introduced.

## Generated View Impact
- GV-001 [PD-009] [PD-010] [PD-014] canonicalArtifacts: Rule manifests/coverage/source index, combat fixture bytes, replay packages, public surface, and browser projection fixtures regenerate deterministically from exact implementation sources.
- GV-002 [PD-012] evidenceViews: Release performance, native/Fable equality, replay, browser, mutation, production-delivery, and documentation receipts bind the exact candidate and report malformed/stale inputs.
- GV-003 [PD-014] lifecycleViews: Analysis, work model, summary, equivalent Claude/Codex guidance, verify, ship, Governance handoff, and committed ship verdict refresh from current authored sources/evidence.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Performance is owned by DEC-006/FR-012 and the executable performance contract because the seeded SDD spec has no populated typed performance-intent front matter; the contract still binds exact workload/caps/capability/evidence before implementation acceptance.
- The framework references document inherited package-derived authority through `SpatialQuery`; combat must not call or copy those algorithms directly.
- Optional Governance pointers remain compatibility facts only; SDD reports readiness and does not substitute for package/browser/player-route evidence.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 181-physical-combat-slice`.
