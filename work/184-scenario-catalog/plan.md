---
schemaVersion: 1
workId: 184-scenario-catalog
title: Scenario Catalog
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/184-scenario-catalog/spec.md
sourceClarifications: work/184-scenario-catalog/clarifications.md
sourceChecklist: work/184-scenario-catalog/checklist.md
publicOrToolFacingImpact: true
---

# Scenario Catalog Plan

Prose status: planned

## Source Snapshot
- spec: work/184-scenario-catalog/spec.md sha256:df753bdd69fd96f05df2f5f4104a71ffe3ae1614f18919c6682558dd3d4376b6 schemaVersion:1
- clarifications: work/184-scenario-catalog/clarifications.md sha256:434360f751282979e153d11783089eb1247b18286ed3f200c1c41445af27bf42 schemaVersion:1
- checklist: work/184-scenario-catalog/checklist.md sha256:9944f9a18feab1c3bf91568070b93ca0e16217d83bcd46261fc3378b463e44fc schemaVersion:1

## Plan Scope
- Tier-1 additive Client/Web/catalog change over the existing map editor, simulator, replay presentation, shared command registry, and production browser composition.
- Declare the scenario package/catalog/validation/canonical-cost surface beside `ExperienceSamples`, compose seven authored packages and retained replay bindings, then project them through existing Samples controls.
- Tests and fixtures own native/Fable fingerprints, package rejection, map-family coverage, performance workload/counters, browser matrix/journey, and protected-subject mutations.

## Technical Context
- `SIR.Client` is shared by .NET and Fable. Immutable F# records/unions and deterministic ordinal encoding keep package values portable; `SIR-MAP 2` stays the editor/simulator payload.
- `ExperienceSamples.editorState`, `MapEditorSimulator.tryHandoff/update`, replay projections, and Web `App`/`PanelViews` are the existing production routes; no new authority or test-only entry is introduced.
- The workload runs package validation, map handoff, fixed simulator steps, semantic checkpoint projection, and scene/view projection after warm-up, reporting deterministic counts separately from elapsed Release samples.

## Constitution Check
- I/III: SDD analysis precedes code; additive public package types/functions are declared in a new `ExperienceSamples.fsi` before implementation and tested/documented together.
- IV/V: records, discriminated diagnostics, canonicalization, validation, cost counting, and update/projection remain pure; browser interaction stays at the Web edge.
- VI/VIII: real package/browser fixtures, native/Fable equality, production journey, and one fail-capable mutation per new gate make success and stale identity observable.

## Plan Decisions
- PD-001 [AC-001] [AC-003] [FR-001] [DEC-001] complete: Declare schema-v1 scenario family, package identity, force/loadout, knowledge, plan, objective, checkpoint, replay-binding, design-note, validation-error, and cost records plus canonical/validate/import functions in `ExperienceSamples.fsi`.
- PD-002 [AC-001] [AC-002] [FR-002] [DEC-002] complete: Replace the three narrow sample entries with one fast-start package and six stable family packages, preserving the existing IDs where useful and authoring 12-or-more-unit 32×24–64×48 maps for composed families.
- PD-003 [AC-001] [FR-003] [DEC-003] complete: Adapt existing `maps`, `replays`, `tryMap`, `tryReplay`, `editorState`, simulator, and replay functions from package data so File → Samples continues through the production route and exposes family/lesson/design notes.
- PD-004 [AC-002] [FR-004] [DEC-002] [DEC-003] complete: Key metadata to stable unit/objective identities and validate required terrain, edges, zones, unit variety, facing/attention/knowledge, plans, lesson, and alternative solutions without treating prose as simulator authority.
- PD-005 [AC-003] [FR-005] [DEC-001] complete: Canonically encode the full immutable record in fixed field/list order, calculate a stable package digest, and atomically reject unsupported schema or mismatched engine/ruleset/content/map/replay bindings with deterministic diagnostics.
- PD-006 [AC-004] [FR-006] [DEC-006] complete: Build semantic checkpoints from production simulator frames at declared ticks, bind retained replay identity to package identity/digest, and retain event/checkpoint canonical streams rather than pixel-only expected output.
- PD-007 [AC-004] [FR-007] [DEC-006] complete: Emit one catalog fingerprint from shared Client code in native and built Fable qualification, compare exact bytes, and keep addressed seed plus canonical sort order in the fingerprint.
- PD-008 [AC-005] [FR-008] [DEC-006] complete: Add isolated missing-unit, geometry-line, event-order, replay-binding, and unreadable-receipt mutations; each script must observe its owning gate red before the unmodified candidate is re-run green.
- PD-009 [AC-006] [FR-009] [DEC-004] complete: Add representative seven-package and deterministic 200-unit stress workloads over validation/load/update/spatial/combat/timeline/view, enforce typed performance intent, emit counters and Release p95/p99/capability data, and preserve the explicit headless compositor limitation.
- PD-010 [AC-007] [FR-010] [DEC-005] complete: Extend Playwright to boot the shipped entry, enumerate/load all scenarios and read lessons, then use real simulator/timeline controls for a named checkpoint; retain screenshots/semantic assertions and assemble exact-candidate lifecycle/feedback/review evidence.

## Contract Impact
- PC-001 [PD-001] [PD-005] publicSurface: `src/SIR.Client/ExperienceSamples.fsi` declares package schema, validation/canonicalization/import, catalog lookup, simulator/replay, and deterministic cost surfaces before `.fs` implementation.
- PC-002 [PD-002] [PD-004] catalogContract: Seven stable packages cover the teaching and six-family matrix; metadata is keyed to map unit/objective IDs and map text remains the authoritative editor handoff.
- PC-003 [PD-003] productionRoute: Existing Samples map/replay projections consume packages without a parallel test-only loader; package editability follows the existing editor state.
- PC-004 [PD-005] identityContract: Atomic validation binds schema, engine, ruleset, content, map revision, package digest, and replay identity with stable diagnostics.
- PC-005 [PD-006] [PD-007] replayConformance: Ordered semantic event/checkpoint canonical text is identical across native/Fable and retained replay lookup rejects a different package binding.
- PC-006 [PD-009] performanceContract: Typed intent and `docs/performance-budget.md` own workload ids/digests, scales, structural limits, 20/50 ms posture, capability, and compositor limitation.

## Verification Obligations
- VO-001 [PD-001] [PD-004] [PC-001] [PC-002] packageTest: Assert exact schema/catalog/family inventory, unique stable identities, nonempty metadata, unit/objective references, scenario scale, and canonical determinism.
- VO-002 [PD-003] [PC-003] productionRouteTest: For every package, invoke `editorState`, simulator handoff, normal stepping, and replay projection; assert editable map identity plus declared checkpoints.
- VO-003 [PD-005] [PC-004] identityTest: Import unchanged packages and reject schema, engine, ruleset, content, map revision, package digest, and replay-binding mutations atomically with exact diagnostics.
- VO-004 [PD-006] [PD-007] [PC-005] conformanceTest: Native and built Fable catalog/event/checkpoint fingerprints match byte-for-byte for all packages and addressed seeds.
- VO-005 [PD-008] gateMutationTest: Run five protected-subject mutations plus restoration; capture expected nonzero code/red message and final green for each owning gate.
- VO-006 [PD-009] [PC-006] performanceTest: Baseline and exact-candidate Release workload receipts bind both definition digests, warm-up/sample policy, counters, p95/p99/catch-up, host/browser capability, and structural/timing verdicts.
- VO-007 [PD-010] browserJourneyTest: Built Playwright opens every package from the real Samples UI, sees each lesson, runs/scrubs the teaching sample through player controls, and verifies selected semantic/visual checkpoints.
- VO-008 [PD-010] lifecycleTest: Run focused/full Dev/Test/Verify, docs, feedback checkpoint/report/audit validators, SDD evidence/verify/ship/refresh/agents, exact-head CI/path/claim/delivery, and independent review.

## Performance Intent
- id: scenario-catalog-v1
- disposition: active
- targetFps: 20
- workloadIds: [scenario-catalog-representative-v1]
- workloadDefinitionDigests: [scenario-catalog-representative-v1=sha256:9fdc78516912b2e440a6d80d3d6795032edc55876f010fcb96c9f6b03e1d67ac]
- maximumExpectedScale: 80x80 map; 200 units; seven scenario packages
- maxP95Ms: 20
- maxP99Ms: 50
- maxCatchUpFrames: 0
- structuralCostBudgets: [combat-resolutions<=256, los-samples<=256, path-expansions<=4096, scene-nodes<=8000]
- requiredCapability: headless-browser
- liveCompositorRequired: false

## Migration Posture
- PM-001 [PC-001] additive-v1: Existing map/replay sample APIs remain available as projections of packages; callers migrate incrementally to richer package lookup.
- PM-002 [PC-002] stableIds: Retain current teaching/breach/objective identifiers where they remain semantically valid; new family/package IDs are immutable after fixture publication.
- PM-003 [PC-004] reject-stale: No implicit upgrade of unsupported or identity-stale packages; future schemas require an explicit migration function and new fixture revision.
- PM-004 [PC-005] retainedReplay: A replay remains readable only through its exact retained engine/package binding; current code never reinterprets stale events as the new package.

## Generated View Impact
- GV-001 [PD-007] [PD-009] evidenceViews: Catalog/conformance/performance receipts bind source/workload digests and exact candidate, and fail on missing/stale/unreadable artifacts.
- GV-002 [PD-010] lifecycleViews: Analysis, work model, summary, equivalent Claude/Codex guidance, verify, ship, feedback report/audit, and review/delivery receipts refresh from current sources and exact head.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- The focused game skills guide authored map/spatial/combat/awareness composition; this product continues consuming existing authoritative implementations rather than duplicating algorithms.
- Browser structural/timing evidence is valid on this host; live-compositor frame pacing is not required by the typed intent and is not claimed.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 184-scenario-catalog`.
