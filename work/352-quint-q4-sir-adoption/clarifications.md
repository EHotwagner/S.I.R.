---
schemaVersion: 1
workId: 352-quint-q4-sir-adoption
title: Quint Q4 complete SIR combat adoption
stage: clarify
changeTier: tier1
status: clarified
sourceSpec: work/352-quint-q4-sir-adoption/spec.md
publicOrToolFacingImpact: true
---

# Quint Q4 complete SIR combat adoption clarifications

## Source Specification
- `work/352-quint-q4-sir-adoption/spec.md`

## Clarification Questions
- CQ-001: Does “everything” mean the bounded sixteen-rule `CombatRules.registry` corpus? Answered yes by the user's explicit Q4 scope expansion; unrelated S.I.R. application behavior remains outside this rule-authority migration.
- CQ-002: Should Q4 deliberately demonstrate multiple Quint domains and granularity levels? Answered yes: facts, pure formulae, external algorithm contracts, focused transition semantics, and aggregate stateful behavior are acceptance concerns.
- CQ-003: Is the proposed concrete Quint type/state and atomicity sketch an accurate abstraction of the runtime? Answered yes by the user on 2026-08-27.
- CQ-004: Which identities are compatibility-frozen? Legacy rule/application/rollback subjects remain exact where contractually represented; manifest-v2 authority, compiled-contract, binding, projection, and receipt identities are new deterministic source-bound artifacts.

## Decisions
- DEC-001 [CQ-001]: Q4 migrates all sixteen current combat registry entries, not only `COMBAT-DAMAGE-001`.
- DEC-002 [CQ-002]: The model will use the smallest faithful abstraction at each layer and document why each Quint construct was chosen.
- DEC-003 [CQ-004]: Compatibility is byte-exact for named legacy subjects and semantic-plus-fingerprint exact for new manifest-v2 products.
- DEC-004: `Los.lineOfSightBy` stays a registered implementation. Quint owns its bounded contract and visible/total/result relation, not supercover traversal.
- DEC-005: The aggregate consequence resolution is one atomic action because the real interpreter publishes only the completed result; focused rule semantics remain independently readable/testable as pure helpers and ordered observations.
- DEC-006 [CQ-003]: Use raw scale-10,000 fixed-point integers; `NoWound | MinorWound | MajorWound`; cohesive `CombatState`, `AttackInput`, and `Observation` records; one atomic consequence action; and separate cover-impact and suppression-recovery actions.
- DEC-007: FS.GG.SDD#932 owns the general consumer-model profile required for canonical integration. Q4 may author, typecheck, test, and correspondence-check the standalone model before that published dependency lands, but may not impersonate a successful Typed SDD migration.

## Remaining Ambiguity
No blocking ambiguity remains.

## Lifecycle Notes
- Continue through checklist, plan, tasks, and analyze before model implementation.

## Accepted Deferrals
No accepted deferrals recorded.

## Answers
No clarification answers recorded.