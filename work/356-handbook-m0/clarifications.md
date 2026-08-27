---
schemaVersion: 1
workId: 356-handbook-m0
title: Handbook M0
stage: clarify
changeTier: tier1
status: clarified
sourceSpec: work/356-handbook-m0/spec.md
publicOrToolFacingImpact: true
---

# Handbook M0 Clarifications

## Source Specification
- work/356-handbook-m0/spec.md

## Clarification Questions
- CQ-001: Can an unmerged Q4 model be inventoried as if it were current repository authority?
- CQ-002: Does importing the ledger require creating the M1 handbook skeleton?

## Answers
- CQ-001 → No. Inventory its complete candidate surface, bind it to PR #355 head `2d41880356997cd0e265180941ffc094e49dd1f9`, and label it candidate until merged.
- CQ-002 → No. Establish the future filename and roadmap only; the handbook hierarchy belongs to M1.

## Decisions
- **DEC-001** [CQ-001] [FR-001] [FR-002] [FR-003] [FR-005]: Separate current authority (`origin/main` at `77e56d11867a5e2e7ad99f4d61b0f0c9fff61a5f`) from Q4 candidate authority (PR #355 head `2d41880356997cd0e265180941ffc094e49dd1f9`) in every affected inventory.
- **DEC-002** [CQ-002] [FR-006]: Preserve all M0-M7 milestone wording in the imported ledger, mark only M0 complete, and reserve `docs/sir-combat-quint-handbook.md` for M1 rather than creating it now.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
No blocking ambiguity remains.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 356-handbook-m0`.
