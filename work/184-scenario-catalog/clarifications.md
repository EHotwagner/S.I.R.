---
schemaVersion: 1
workId: 184-scenario-catalog
title: Scenario Catalog
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/184-scenario-catalog/spec.md
publicOrToolFacingImpact: true
---

# Scenario Catalog Clarifications

## Source Specification
- work/184-scenario-catalog/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: What exact v1 package shape and digest boundary is authoritative?
- CQ-002 [AMB:AMB-002]: Which scenario matrix and scale satisfy the required tactical families honestly?
- CQ-003 [AMB:AMB-003]: How do richer authored facts compose with existing map text and simulator handoff?
- CQ-004 [AMB:AMB-004]: What representative/stress workloads and budgets apply?
- CQ-005 [AMB:AMB-005]: Which production controls prove player reachability?
- CQ-006 [AMB:AMB-006]: Which cross-runtime and mutation subjects make the gates fail-capable?

## Answers
- CQ-001 → schema v1 is an additive immutable F# record. Its canonical text orders every scalar/list and includes schema, scenario/catalog IDs, family, engine/ruleset/content/map revision, seed, forces/loadouts, initial knowledge, plans, objectives, checkpoints, replay binding, design notes, and map text. Import validates the whole value before returning it.
- CQ-002 → ship one 16×10 four-unit teaching scenario and six named composed scenarios on 32×24 through 64×48 maps with at least 12 units each; document mechanics already represented by map/simulator state and label aspirational tactical approaches as design notes rather than inventing unshipped authority.
- CQ-003 → retain `SIR-MAP 2` as the authoritative editor/simulator map payload. Package metadata adds facing, attention, knowledge, capabilities/loadouts, plans, objectives, and lessons keyed by stable unit/objective IDs; map parsing and simulator handoff remain unchanged.
- CQ-004 → representative means the seven shipped packages; stress means deterministic repetition to 200 units on an 80×80 bound. Preserve existing 4,096 expansion/crossing, 256 LOS sample, 64 KiB explanation, 100/200-unit, 20/50 ms focused budgets and the established 8,000-node production cap, while labeling compositor evidence unavailable on headless hosts.
- CQ-005 → Playwright boots the shipped Web entry, opens the real Samples panel, loads each named scenario through its displayed button, reads the lesson, and for the teaching scenario uses ordinary run/step/timeline controls to reach and scrub a declared checkpoint.
- CQ-006 → one canonical catalog/checkpoint fingerprint is emitted by native and built Fable routes. Mutations independently remove a unit, alter a geometry line, swap event order, stale the replay/scenario binding, and corrupt an evidence receipt; each owning gate must reject exactly its subject.

## Decisions
- DEC-001 [CQ-001] [AMB:AMB-001] [FR-001] [FR-005]: Use additive immutable schema-v1 records and canonical text whose complete ordered semantic content is hashed; validation is atomic and returns stable typed diagnostics.
- DEC-002 [CQ-002] [AMB:AMB-002] [FR-002] [FR-004]: Ship one four-unit teaching sample plus six 12-or-more-unit families on 32×24–64×48 authored maps; catalog metadata names honest lessons and alternative plans without adding fictional simulator mechanics.
- DEC-003 [CQ-003] [AMB:AMB-003] [FR-003] [FR-004]: Keep `SIR-MAP 2` as the map handoff and layer typed package metadata keyed by stable identities; opening a package continues through `MapEditor.initial`, `LoadMapText`, and `MapEditorSimulator.tryHandoff`.
- DEC-004 [CQ-004] [AMB:AMB-004] [FR-009]: Bind PERF-PLAN to the producer’s existing 80×80, 100/200-unit, 4,096-expansion/crossing, 256-LOS-sample, 64-KiB, 20/50-ms posture; representative is all shipped scenarios, stress is 200 units, structural counters are authoritative, and headless evidence never claims compositor coverage.
- DEC-005 [CQ-005] [AMB:AMB-005] [FR-003] [FR-010]: Browser acceptance starts at the shipped entry and uses displayed Samples, load, simulator, advance, and timeline controls only; no direct message dispatch or seeded model can satisfy the journey.
- DEC-006 [CQ-006] [AMB:AMB-006] [FR-006] [FR-007] [FR-008]: Native/Fable canonical equality and five independent protected-subject mutations are blocking gates, with semantic checkpoints authoritative over optional visual artifacts.
- DEC-007 [FR-002] [FR-004]: The user explicitly authorized deliberate migration of the three existing sample layouts and their deterministic expectations. Keep their public sample/replay IDs, replace their small layouts with the composed family layouts, and update invariant-based tests; do not retain old layouts solely as compatibility fixtures.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
- None. All blocking ambiguities are resolved by DEC-001 through DEC-006; DEC-007 records the subsequent user-authorized migration boundary.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 184-scenario-catalog`.
