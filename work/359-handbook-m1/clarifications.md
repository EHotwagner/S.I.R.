---
schemaVersion: 1
workId: 359-handbook-m1
title: Handbook M1 linked skeleton
stage: clarify
changeTier: tier1
status: clarified
sourceSpec: work/359-handbook-m1/spec.md
publicOrToolFacingImpact: true
---

# Handbook M1 linked skeleton Clarifications

## Source Specification
- work/359-handbook-m1/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001] blocking open: Resolve source ambiguity AMB-001 before checklist.
- CQ-002 [AMB:AMB-002] blocking open: Resolve source ambiguity AMB-002 before checklist.
- CQ-003 [AMB:AMB-003] blocking open: Resolve source ambiguity AMB-003 before checklist.

## Answers
- CQ-001 → keep the checked manifest beside the handbook at `docs/sir-combat-quint-vocabulary.json`.
- CQ-002 → use a dependency-free structural tokenizer/state machine that distinguishes front matter, fenced code, headings, links, and explicit HTML anchors.
- CQ-003 → keep solutions in an appendix within the single handbook.

## Decisions
- **DEC-001** [CQ-001] [AMB:AMB-001] [FR-002] [FR-004]: The checked vocabulary manifest is `docs/sir-combat-quint-vocabulary.json`; M6 may generate or integrate it without breaking its contract.
- **DEC-002** [CQ-002] [AMB:AMB-002] [FR-004]: M1 uses a structurally aware, dependency-free audit prototype and reserves full docs-qualification enforcement for M6.
- **DEC-003** [CQ-003] [AMB:AMB-003] [FR-001]: Exercise solutions remain an appendix in the first edition's one-file handbook.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
- None.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 359-handbook-m1`.
