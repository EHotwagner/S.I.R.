---
schemaVersion: 1
workId: 363-handbook-m3
title: Handbook M3
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/363-handbook-m3/spec.md
publicOrToolFacingImpact: true
---

# Handbook M3 Clarifications

## Source Specification
- work/363-handbook-m3/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: How should rules sharing `resolveConsequences` be taught without implying extra state transitions?
- CQ-002 [AMB:AMB-002]: What makes penetration executable when it has no standalone state-changing action?
- CQ-003 [AMB:AMB-003]: How far may M3 exercises and checks go without consuming M4's mutation/counterexample scope?

## Answers
- CQ-001 → use one atomic action/run for the completed consequence, then expose each rule through the exact pure helper, observation field, explanation-order entry, or state property that the authority already provides.
- CQ-002 → teach penetration at formula/observation granularity through `retainedEffect`, `damageForAttack`, `retentionRaw`, and the catalogue dependency; do not invent a penetration action.
- CQ-003 → M3 may use positive named runs and structural negative controls that prove coverage checks, but learner-facing deliberate semantic mutations and counterexample interpretation remain M4.

## Decisions
- **DEC-001** [CQ-001] [AMB:AMB-001] [FR-002] [FR-003]: Preserve the authority's action granularity: atomic consequences stay atomic, while helper values, observation fields, catalogue subjects, and properties explain participating rules.
- **DEC-002** [CQ-002] [AMB:AMB-002] [FR-003] [FR-005]: Document penetration as retained-effect formula and observation behavior inside aggregate resolution, explicitly stating that no standalone runtime-visible transition exists.
- **DEC-003** [CQ-003] [AMB:AMB-003] [FR-006] [FR-007]: Limit M3 exercises to prediction, positive execution, dependency/trace interpretation, and bounded modeling design; focused structural negative controls may test M3 audits, while semantic mutation lessons remain M4.

## Accepted Deferrals
- None. M4 and M5 scope boundaries are exclusions inherited from the roadmap, not unresolved M3 obligations.

## Remaining Ambiguity
- None. AMB-001 through AMB-003 are resolved by DEC-001 through DEC-003.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 363-handbook-m3`.
