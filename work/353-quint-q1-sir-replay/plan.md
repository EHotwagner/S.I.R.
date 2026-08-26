---
schemaVersion: 1
workId: 353-quint-q1-sir-replay
title: Quint Q1 Sir Replay
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/353-quint-q1-sir-replay/spec.md
sourceClarifications: work/353-quint-q1-sir-replay/clarifications.md
sourceChecklist: work/353-quint-q1-sir-replay/checklist.md
publicOrToolFacingImpact: true
---

# Quint Q1 Sir Replay Plan

Prose status: planned

## Source Snapshot
- spec: work/353-quint-q1-sir-replay/spec.md sha256:343d95f2c729b37b70bcd223f078189a7d848940e7728db40d2417fd2facc5b4 schemaVersion:1
- clarifications: work/353-quint-q1-sir-replay/clarifications.md sha256:5231406d3d817d66871cda6b3811f1cd09f80971061131f4ac862c58205b2a81 schemaVersion:1
- checklist: work/353-quint-q1-sir-replay/checklist.md sha256:bb7cdbbf3cccd4db6b6dca3a8593b4805e1465e504a28eec47a6e06dd69a4302 schemaVersion:1

## Plan Scope
- Work item 353-quint-q1-sir-replay is planned from the current specification, clarification, and checklist facts.
- Requirement count: 1.
- Clarification decision count: 0.
- Checklist result count: 1.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Add a native-only shared conformance fixture that parses the exact committed Q1 replay envelope, maps `Initialize` and `ApplyDamage(amount)` to a thin adapter over `CombatRules.resolveConsequences`, and compares only `hitPoints`, `lastAction`, and `lastAmount` after each transition.

## Contract Impact
- PC-001 [PD-001] fixture contract: The committed producer receipt, replay envelope, normalized ITF, adapter/implementation source digests, seed, and bounds form one fail-closed correspondence receipt; no production API or package surface changes.

## Verification Obligations
- VO-001 [PD-001] [PC-001] semanticTest: Run the exact witness successfully, then require wrong action mapping, omitted action, wrong observable field, stale expected state, and bypassed combat-boundary mutations to fail with the first transition index and stable action name.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] testOnly: Keep the adapter and corpus non-authoritative and test-only; Q4 alone may migrate canonical combat authority or add native/Fable/package/rollback obligations.

## Generated View Impact
- GV-001 [PD-001] evidenceViews: Refresh only the SDD-owned analysis, evidence, verify, and ship views after focused replay evidence is observed.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 353-quint-q1-sir-replay`.
