---
schemaVersion: 1
workId: 365-handbook-m4
title: Handbook M4
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/365-handbook-m4/spec.md
publicOrToolFacingImpact: true
---

# Handbook M4 Clarifications

## Source Specification
- work/365-handbook-m4/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: How can the laboratory create real semantic defects without editing or forking the literate authority?
- CQ-002 [AMB:AMB-002]: Which wording separates sampled traces, bounded exhaustive verification, witnesses, and invariants accurately?
- CQ-003 [AMB:AMB-003]: Which detector owns each mutation family, especially constant catalogue integrity versus transition behavior?

## Answers
- CQ-001 → extract the authoritative Quint module into a temporary fixture, apply a single named mutation to that fixture only, run its focused detector, then delete/replace it and rerun the unmodified extracted authority through the same route.
- CQ-002 → label `run` output as sampled execution, named successful action traces as reachable witnesses, invariant predicates as universal claims over checked states, and `verify` results as exhaustive only within the explicitly stated bounded initial/action state space.
- CQ-003 → threshold uses a boundary witness/property detector; bounds uses `BoundedCombatState`; suppression uses `SuppressionRequiresDamage`; cover uses `DestroyedCoverIsPermeable`; collateral uses `FactionNeutralCollateral`; catalogue integrity uses `SixteenRulesDeclared` plus exact ID/dependency checks.

## Decisions
- DEC-001 [CQ-001] [AMB:AMB-001] [FR-004] [FR-007]: Keep `docs/rules/sir-combat.md` immutable; build every mutant in an ephemeral extracted fixture, assert observed red, and rerun the untouched extraction through the identical detector for restored green.
- DEC-002 [CQ-002] [AMB:AMB-002] [FR-001] [FR-002]: Pair every formal evidence label with explicit scope language; never call sampled execution proof and never present bounded verification as an unbounded theorem.
- DEC-003 [CQ-003] [AMB:AMB-003] [FR-004] [FR-005] [FR-006]: Give each mutation one named primary detector and keep catalogue, transition, observation, and invariant checks distinct; the aggregate qualification may summarize them but cannot substitute for their scoped receipts.

## Accepted Deferrals
- None. M5, M6, M6V, and M7 are roadmap exclusions, not unresolved M4 obligations.

## Remaining Ambiguity
- None. AMB-001 through AMB-003 are resolved by DEC-001 through DEC-003.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 365-handbook-m4`.
