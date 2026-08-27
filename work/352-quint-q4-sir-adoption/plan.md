---
schemaVersion: 1
workId: 352-quint-q4-sir-adoption
title: Quint Q4 complete SIR combat adoption
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/352-quint-q4-sir-adoption/spec.md
sourceClarifications: work/352-quint-q4-sir-adoption/clarifications.md
sourceChecklist: work/352-quint-q4-sir-adoption/checklist.md
publicOrToolFacingImpact: true
---

# Quint Q4 complete SIR combat adoption Plan

Prose status: planned

## Source Snapshot
- spec: work/352-quint-q4-sir-adoption/spec.md sha256:323c1a23fb6655c3efc53af84ca3b4a632633d92321992216ea995ef713966ad schemaVersion:1
- clarifications: work/352-quint-q4-sir-adoption/clarifications.md sha256:82d64422132539f2e703419ac0a1aa362ba2ffe7ce85c61d0d3f9ad564a74b23 schemaVersion:1
- checklist: work/352-quint-q4-sir-adoption/checklist.md sha256:dd25291d8ad2a2a0eb36eeee73b9969b6ab6132fff5290d57dbfe8c009488911 schemaVersion:1

## Plan Scope
- Work item 352-quint-q4-sir-adoption is planned from the current specification, clarification, and checklist facts.
- Requirement count: 12.
- Clarification decision count: 7.
- Checklist result count: 12.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Author one reviewer-facing Markdown authority under `docs/rules/` with ordered `quint sir-combat.qnt +=` fences; generated modules live only under readiness/Typed SDD outputs.
- PD-002 [AC-001] [AC-004] [FR-002] complete: Define a typed rule catalogue containing all sixteen stable IDs, semantic kinds, sorted dependencies, reads/effects/events, source symbols, and ordered explanation observations so the compiled contract has no prose-only rule.
- PD-003 [AC-002] [AC-004] [FR-003] complete: Represent Q4 values as raw `int` with scale 10,000 and pure helpers for int32 saturation, ties-away-from-zero division, saturating multiplication/addition/subtraction, ratio construction, clamping, preparation, trace, retention, and damage rounding.
- PD-004 [AC-002] [AC-004] [FR-004] complete: Catalogue the trace rule as an external algorithm with exact `Los.lineOfSightBy` symbol/fingerprint; model only the guard `0 <= visible <= total && total > 0` and its raw ratio result.
- PD-005 [AC-002] [AC-004] [FR-005] complete: Use one `CombatState` variable and one ordered `Observation`; make consequences atomic, with separate cover-impact and suppression-recovery actions. Focused rules are pure helpers used by those actions, avoiding invented intermediate runtime commits.
- PD-006 [AC-002] [FR-006] complete: Express bounds and equivalences as named state invariants and catalogue each property with stable identity and affected subjects.
- PD-007 [AC-002] [FR-007] complete: Add named Quint `run` witnesses for representative damage, wound boundaries, zero health, suppression eligibility/recovery, cover destruction/current collision, and faction neutrality; run typecheck, test, and seeded simulation without claiming model-checking.
- PD-008 [AC-001] [AC-004] [FR-008] complete: Keep `CombatRules.resolveAttack`, `resolveConsequences`, `resolveCoverImpact`, and `resolveRecovery` as the interpreter; replace only registry authoring values with generated bindings after FS.GG.SDD#932 publishes them.
- PD-009 [AC-003] [FR-009] complete: Maintain 1.4.0 as the compatibility floor, then pin the exact stable coherent Artifacts/CLI version published by FS.GG.SDD#932 and restore only affected package locks from configured feeds.
- PD-010 [AC-005] [FR-010] complete: Expand the Q1 adapter to initialize/project `CombatState` and map each modeled action to the real interpreter; compare every observation in exact and seeded ITF traces and report the first transition/field divergence.
- PD-011 [AC-006] [FR-011] complete: Use Typed SDD migration rather than direct author replacement, retain the authenticated v1 inventory, and exercise rollback plus interruption/tamper controls through the published lifecycle.
- PD-012 [AC-007] [FR-012] complete: Build one qualification entry point selected by authority/interpreter/adapter/generated-product changes, with independent mutations for Quint semantics, generated freshness, producer identity, contract/source binding, mapping, and interpreter results.

## Contract Impact
- PC-001 [PD-001] [PD-002] [PD-008] typedAuthority: The public contract changes from F#-authored `RuleDefinition` values to generated F#/Fable bindings compiled from canonical literate Quint; rule IDs, kinds, dependency/read/effect/event metadata, canonical compatibility payload, and interpreter APIs remain stable.
- PC-002 [PD-004] registeredAlgorithm: `COMBAT-TRACE-002` retains implementation symbol `FS.GG.Game.Core.Los.lineOfSightBy` and fingerprint `FS.GG.Game.Core@0.13.0:Los.lineOfSightBy:Supercover`; generated bindings describe but never implement it.
- PC-003 [PD-009] packageConsumer: S.I.R. consumes a byte-identical dual-feed FS.GG.SDD coherent set by exact package/tool pins with locked graphs and no source checkout edge.
- PC-004 [PD-010] runtimeCorrespondence: Stable Quint action names and observation fields map explicitly to real interpreter calls and exact int/string/list projections.

## Verification Obligations
- VO-001 [PD-001] [PD-003] [PD-007] [PC-001] semanticTest: Typecheck the generated module, execute every named Quint witness, and run deterministic seeded simulations under Quint 0.32.0; retain command/version/source receipts.
- VO-002 [PD-002] [PD-008] [PC-001] compatibility: Regenerate the frozen corpus and prove stable identities, dependency graph, canonical compatibility bytes, explanation order, and native/Fable equality.
- VO-003 [PD-010] [PC-004] correspondence: Replay exact witnesses and sampled ITF traces through real interpreter entry points, then demonstrate independent mapping and implementation mutations fail at the first divergence.
- VO-004 [PD-009] [PC-003] packageOnly: Restore/build/test from clean configured feeds using exact pins and locks; reject source-project or checkout-relative producer references.
- VO-005 [PD-011] [PC-001] rollback: Prove accepted migration/rollback, transaction interruption recovery, idempotent retry, and stale/missing/substituted/forged inventory refusal.
- VO-006 [PD-012] qualification: Observe each negative control red, restore the subject, and require the full focused gate to return green before evidence is accepted.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] explicitMigration: Analyze the existing v1 authority first, accept only a lossless general-profile migration from FS.GG.SDD#932, retain authenticated original bytes, and never use `author --accept` to bypass migration.
- PM-002 [PC-003] publishBeforeAdopt: Standalone model and correspondence work may land only if clearly non-canonical; final generated-binding adoption waits for the published producer package and then updates exact pins/locks.

## Generated View Impact
- GV-001 [PD-001] workModel: `readiness/352-quint-q4-sir-adoption/work-model.json` refreshes from current lifecycle sources or reports stale generated evidence.
- GV-002 [PD-001] [PD-008] typedAuthority: fence manifest, generated `.qnt`, source map, typed-effect JSON, compiled contract, F#/Fable bindings, compilation receipt, rollback inventory, and manifest v2 are generated and freshness-checked.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 352-quint-q4-sir-adoption`.
