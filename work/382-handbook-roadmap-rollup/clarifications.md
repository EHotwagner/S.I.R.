---
schemaVersion: 1
workId: 382-handbook-roadmap-rollup
title: Handbook Roadmap Rollup
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/382-handbook-roadmap-rollup/spec.md
publicOrToolFacingImpact: true
---

# Handbook Roadmap Rollup Clarifications

## Source Specification
- work/382-handbook-roadmap-rollup/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001] blocking: Which repository facts define the complete cycle set without hard-coding the expected twelve as authority?
- CQ-002 [AMB:AMB-002] blocking: How should delivery evidence that is not committed in the cycle report be represented?
- CQ-003 [AMB:AMB-003] blocking: What key binds each disposition to one immutable checkpoint record without restating that record as a new authority?

## Answers
- CQ-001 → enumerate `feedback/checkpoints/roadmap-sir-combat-quint-handbook-*.jsonl`, require one report whose front-matter `cycle` equals each basename, require the matching stem audit, and fail if either direction has an unmatched cycle.
- CQ-002 → cite exact issue, PR, main run, and Pages run identifiers only where repository text or a retained GitHub query supplies them; otherwise write `not retained` rather than infer.
- CQ-003 → use the immutable pair `(cycle, one-based JSONL line)` and require the report row to carry the checkpoint's phase, kind, exact summary, evidence, and one permitted disposition.

## Decisions
- DEC-001 [CQ-001] [AMB:AMB-001] [FR-001] [AC-001]: Filesystem enumeration is authoritative for membership; the observed total of twelve is a checked result and a mismatched report-front-matter inventory is an error.
- DEC-002 [CQ-002] [AMB:AMB-002] [FR-004] [AC-004]: Delivery cells use exact retained identifiers or the literal `not retained`; absence is never upgraded into proof.
- DEC-003 [CQ-003] [AMB:AMB-003] [FR-003] [AC-003]: Each disposition row is keyed by cycle plus one-based JSONL line and copies only machine-compared checkpoint fields; rationale is explicitly roll-up judgement.
- DEC-004 [FR-008] [AC-008]: The terminal item participates in SDD and review but creates no feedback artifacts whose cycle matches the roadmap prefix.

## Accepted Deferrals
- None.

## Remaining Ambiguity
- None. All three ambiguities are resolved by DEC-001 through DEC-003.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 382-handbook-roadmap-rollup`.
