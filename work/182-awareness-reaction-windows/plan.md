---
schemaVersion: 1
workId: 182-awareness-reaction-windows
title: Awareness Reaction Windows
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/182-awareness-reaction-windows/spec.md
sourceClarifications: work/182-awareness-reaction-windows/clarifications.md
sourceChecklist: work/182-awareness-reaction-windows/checklist.md
publicOrToolFacingImpact: true
---

# Awareness Reaction Windows Plan

Prose status: planned

## Source Snapshot
- spec: work/182-awareness-reaction-windows/spec.md sha256:31e99abef43be0a0841b455787a01cff600420c4485b8b05f46a36025122d011 schemaVersion:1
- clarifications: work/182-awareness-reaction-windows/clarifications.md sha256:a163dce40fa899af7d6ce3f90018ee90af6c2b2212cf407c0324b8d18514396a schemaVersion:1
- checklist: work/182-awareness-reaction-windows/checklist.md sha256:0ddafe86ac0265cc0dab434f877951d85e88aad4d7c52bfc7e67092c5fa30fba schemaVersion:1

## Plan Scope
- Declare schema-v1 awareness/reaction surfaces in `SIR.Simulation`, compose them into the authoritative tick/physical-combat/replay route, filter them through Match control observations, and project them through Client/Web without duplicate geometry or knowledge policy.
- Declare `.fsi` surfaces and versioned contracts before implementation; preserve Domain → Simulation → Match → Client dependency direction and package-only Game.Core consumption through `SpatialQuery`.
- Implement tests/workloads before the production behavior they guard, then retain protected-subject and unreadable-input mutation receipts for each new or modified refusal gate.
- Bind evidence to exact candidate, workload, package/profile, runtime, browser, replay, and host identities; synthetic stand-ins do not satisfy ship obligations.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: Declare `AwarenessReaction.fsi` first with independent orientation/posture/movement fields and stable canonical encoding; integrate existing orientation types instead of adding parallel direction enums.
- PD-002 [AC-001] [FR-002] [DEC-001] complete: Evaluate sector/range before invoking `SpatialQuery.evaluate` for exact visual LOS, carry canonical spatial evidence/reasons, cap four exposure samples, and prohibit direct Game.Core or geometry calls in Match/Client/Web.
- PD-003 [AC-002] [FR-003] [DEC-001] [DEC-002] complete: Derive bounded factual `Stimulus` values from authoritative world transitions, then pass them into a separate pure per-observer knowledge update; no LOS result directly writes identification.
- PD-004 [AC-002] [FR-004] [DEC-001] [DEC-002] complete: Implement integer unknown/suspected/acquired/lost-contact transitions with 4/2/1 sector contributions, threshold 8, decay 2, 20-tick retention, canonical observer/subject ordering, and explicit reasons/counters.
- PD-005 [AC-003] [FR-005] [DEC-003] complete: Add exactly one optional engagement per unit with stable id and `KnownUnit`, sorted unique `CoveredArea`, or canonical `GuardedEdge` target; validate local knowledge, 256-cell cap, edge revision, and duplicate/empty/unreadable input.
- PD-006 [AC-003] [FR-006] [DEC-003] complete: Advance engagement preparation, active coverage, eligibility, commitment, resolution, interruption, and recovery through a pure bounded transition using 2/1/1/4 v1 durations and posture/attention prerequisites.
- PD-007 [AC-004] [FR-007] [DEC-003] [DEC-004] complete: Build reaction candidates from committed movement/spatial evidence for area entry/edge crossing and from newly valid acquired exposure; require active coverage and return typed ineligibility rather than implicit non-events.
- PD-008 [AC-004] [AC-005] [FR-008] [DEC-004] complete: Compose the public tick order in `Simulation`: inputs, movement transition, stimuli/awareness, engagement maintenance, eligibility snapshot, canonically sorted commitment, physical reaction resolution, ordinary physical actions, event emission, then recovery.
- PD-009 [AC-005] [FR-009] [DEC-004] complete: Revalidate every pending reaction immediately before physical resolution; emit deterministic interruption/non-trigger facts for incapacity, attention/posture/knowledge/target/area/edge/spatial invalidation or blocked fire and never rewind movement.
- PD-010 [AC-006] [FR-010] [DEC-002] complete: Extend Control ABI/Match observation additively with bounded locally known sectors, stimuli, awareness, engagement/reaction timing and reason codes; disclosure tests compare two observers over one world and scan for forbidden truth fields.
- PD-011 [AC-007] [FR-011] [DEC-007] complete: Version canonical awareness/reaction state/fact/event bytes and replay bindings; preserve legacy decode meaning, bind spatial/Game.Core profile/package identity, and exact-compare .NET/Fable/Node/browser fixture bytes with first-divergence diagnostics.
- PD-012 [AC-008] [FR-012] [DEC-005] complete: Extend the existing planning/simulator production UI with player controls for attention and area/edge coverage, authoritative overlays/reasons, and timeline reconstruction; Playwright boots the published app and drives only DOM/player input.
- PD-013 [AC-009] [FR-013] [DEC-006] complete: Add the exact workload contract and Release harness for representative scenarios plus 200 moving contact units; measure the production tick, structural counters, allocation/GC and 5/20/50 ms posture, and refuse dirty/stale/unreadable evidence.
- PD-014 [AC-010] [FR-014] [DEC-005] [DEC-006] [DEC-007] complete: Add semantic/integration/disclosure/replay/cross-runtime/browser/performance gates, subject mutations for each protected boundary, synchronized public surface/docs, schema-v2 feedback, SDD evidence/verify/ship/refresh, exact-head CI, independent review, and guarded delivery.

## Contract Impact
- PC-001 [PD-001] [PD-003] [PD-004] [PD-005] [PD-006] publicSurface: `contracts/awareness-reaction-v1.md` and `.fsi` files declare sensor, stimulus, knowledge, engagement, reaction, reason, limits, counters, update, and canonical-encoding surfaces.
- PC-002 [PD-002] spatialAuthority: Awareness consumes `SIR.Simulation.SpatialQuery.evaluate` and canonical result identity/evidence only; sector culling is S.I.R.-owned semantic policy and no consumer duplicates package algorithms.
- PC-003 [PD-007] [PD-008] [PD-009] tickContract: The versioned tick-order contract composes movement evidence, awareness, engagement/reaction, and physical combat with one stable event sort and typed interruption.
- PC-004 [PD-010] observationSchema: Control ABI and Match observations add bounded local awareness/reaction fields and explicit reason codes without authority-only world truth.
- PC-005 [PD-011] replaySchema: `contracts/awareness-reaction-replay-v1.md` binds canonical state/fact/event bytes, sensor/order/spatial/package identities, legacy meaning, and typed unavailable history.
- PC-006 [PD-012] browserProjection: Client/Web exposes player-emittable attention/coverage controls and projection-only sector/awareness/reaction/timeline state from the real entry.
- PC-007 [PD-013] performanceContract: `contracts/awareness-reaction-performance-v1.md` plus its JSON workload definition owns scale, digest, caps, counters, host/candidate facts, samples, capability limits, and 5/20/50 ms verdicts.
- PC-008 [PD-002] framework: FS.GG.Game.Core@0.13.0#Los.lineOfSightBy
- PC-009 [PD-002] framework: FS.GG.Game.Core@0.13.0#Edges.edgeBetween

## Verification Obligations
- VO-001 [PD-001] [PD-003] [PD-004] [PD-005] [PD-006] [PC-001] semanticTest: Canonical tests cover independent directions, 4/2/1 sectors, occlusion, delayed acquisition, decay/lost contact, one unit/area/edge engagement, every phase, bounds, saturation, and typed malformed input.
- VO-002 [PD-002] [PD-007] [PC-002] integrationTest: Assert sector culling precedes exact inherited LOS, movement evidence drives entry/crossing/exposure triggers, spatial/profile identities are retained, and consumer scans find no duplicate geometry authority.
- VO-003 [PD-008] [PD-009] [PC-003] orderingTest: Golden-test simultaneous movement/reaction/physical-action ordering, canonical multi-reactor sorting, revalidation, interruption reasons, no movement rollback, and identical replay reconstruction.
- VO-004 [PD-010] [PC-004] disclosureTest: Compare knowledge-scoped observations for differently informed units, enforce bounds/reason vocabularies, and prove malformed/unreadable or authority-leaking projections fail closed.
- VO-005 [PD-011] [PC-005] crossRuntimeReplayTest: Compare exact .NET/Fable/Node/browser canonical fixtures, legacy/current replay seeks, identity mismatch/unavailable history, and first-divergence mutations.
- VO-006 [PD-012] [PC-006] playerJourneyTest: Build/publish the production browser, boot its real entry, rotate attention, prepare an area/edge, move an opposing unit through it, inspect the authoritative reason/event, and scrub the timeline using DOM/player controls only.
- VO-007 [PD-013] [PC-007] performanceTest: Run representative and exact 200-unit Release workloads after warm-up; enforce 20,000/5,000/4,096/256/262,144 structural caps, report deterministic counters/allocation/GC/host facts, and enforce 5 ms awareness p95, 20 ms representative p95, and 50 ms stress worst-tick gates.
- VO-008 [PD-014] gateMutationTest: Break each added/modified gate's protected subject once—including LOS-awareness separation, orientation separation, preparation, eligibility, ordering, disclosure, identity, runtime equality, every structural cap, and unreadable evidence—require non-zero, restore, and retain receipts.
- VO-009 [PD-014] lifecycleTest: Run public-surface, focused/full solution, package-only conformance, browser production delivery, docs, schema-v2 feedback, SDD evidence/verify/ship/refresh/agents, exact-head CI, path/claim, independent-review, and delivery validators.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additive-versioned: Extend current unit/simulation/event contracts with explicit schema-v1 awareness/reaction fields and reject unknown variants before mutation.
- PM-002 [PC-002] authority-preserving: Existing orientation/spatial evidence is reused; any current attention-to-engagement shortcut is replaced by the versioned state machine rather than retained as alternate authority.
- PM-003 [PC-004] disclosure-additive: Control observations add bounded optional fields under a new ABI/schema identity; older consumers keep their historical projection and cannot infer omitted truth.
- PM-004 [PC-005] retained-identity: Current canonical bytes gain a new explicit identity while old replay packages keep old meaning and never synthesize awareness state.
- PM-005 [PC-006] projection-only: Browser additions are renderer-neutral projections and ordinary controls; no test-only or Web authority seam is introduced.

## Generated View Impact
- GV-001 [PD-010] [PD-011] canonicalArtifacts: Control ABI code/manifest, canonical fixture bytes, replay packages, public surfaces, and Client projection fixtures regenerate deterministically from exact sources.
- GV-002 [PD-013] evidenceViews: Release performance, cross-runtime, replay, browser, mutation, production-delivery, and documentation receipts bind the exact candidate and reject missing/stale/unreadable inputs.
- GV-003 [PD-014] lifecycleViews: Analysis, work model, summary, equivalent Claude/Codex guidance, verify, ship, optional Governance handoff, and ship verdict refresh from current authored sources/evidence.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- The CLI's generated `Performance Intent` section is empty because this repository carries product performance authority in `docs/performance-budget.md` and executable per-work contracts; DEC-006/FR-013/PC-007/VO-007 bind that authority without inventing an FPS target.
- Framework references name the inherited package surfaces beneath `SpatialQuery`; awareness code calls the S.I.R. adapter, not Game.Core directly.
- Required live browser behavior is a production-route journey; the Release headless workload is authoritative-tick evidence and makes no compositor qualification claim.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 182-awareness-reaction-windows`.
