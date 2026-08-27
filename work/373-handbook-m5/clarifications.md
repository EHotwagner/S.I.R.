---
schemaVersion: 1
workId: 373-handbook-m5
title: Handbook M5
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/373-handbook-m5/spec.md
publicOrToolFacingImpact: true
---

# Handbook M5 Clarifications

## Source Specification
- work/373-handbook-m5/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: How does M5 reuse current replay machinery without duplicating an interpreter or broadening its claims?
- CQ-002 [AMB:AMB-002]: Which statuses accurately describe one-to-one, aggregate, external, presentation-only, and absent correspondence?
- CQ-003 [AMB:AMB-003]: How can the first-divergence lesson observe a real red while both authorities remain unchanged?

## Answers
- CQ-001 → derive generated Quint and ITF only from the literate model; run exact named fixtures plus the existing deterministic sampled corpus through the real F# replay interpreter, and state each boundary explicitly.
- CQ-002 → use `exact`, `aggregate`, `external-contract`, `presentation-only`, and `missing`; every status requires named runtime/evidence subjects or an explicit reason for absence.
- CQ-003 → mutate a temporary expected ITF observation only, compare it to the untouched runtime result, assert the earliest structured divergence, then regenerate the expected data from untouched authority and rerun green.

## Decisions
- DEC-001 [CQ-001] [AMB:AMB-001] [FR-002] [FR-003] [FR-004]: Reuse the production Q4 extraction and replay path; generated projections are disposable, exact fixtures and sampled corpora remain separately scoped, and no parallel interpreter is added.
- DEC-002 [CQ-002] [AMB:AMB-002] [FR-001] [FR-007]: Every correspondence row uses one controlled status and supplies its required F#/evidence details; `missing` is valid and visible rather than silently upgraded.
- DEC-003 [CQ-003] [AMB:AMB-003] [FR-005]: The negative control changes only an ephemeral expected observation and must report the earliest structured mismatch before untouched inputs restore green.

## Accepted Deferrals
- None. M6, M6V, and M7 are roadmap exclusions, not unresolved M5 obligations.

## Remaining Ambiguity
- None. AMB-001 through AMB-003 are resolved by DEC-001 through DEC-003.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 373-handbook-m5`.
