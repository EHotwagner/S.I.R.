---
schemaVersion: 1
workId: 361-handbook-m2
title: Handbook M2
stage: clarify
changeTier: tier1
status: clarified
sourceSpec: work/361-handbook-m2/spec.md
publicOrToolFacingImpact: true
---

# Handbook M2 Clarifications

## Source Specification
- work/361-handbook-m2/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001] blocking open: Resolve source ambiguity AMB-001 before checklist.
- CQ-002 [AMB:AMB-002] blocking open: Resolve source ambiguity AMB-002 before checklist.
- CQ-003 [AMB:AMB-003] blocking open: Resolve source ambiguity AMB-003 before checklist.

## Answers
- CQ-001 → teach saturation where `saturateInt32` is called, then call out that `roundedDamage` deliberately applies `wrapInt32(rawDamage + SCALE / 2)` before integer division; the representative bounded path does not approach that overflow edge.
- CQ-002 → include only a narrow representative mapping table and cite the current focused qualification; reserve complete declaration/runtime mapping, replay mechanics, and first-divergence instruction for M5.
- CQ-003 → show concise declarations copied from the authority and make a focused audit extract the complete `.qnt`, require each shown fence to be an exact substring, and execute the extracted model.

## Decisions
- **DEC-001** [CQ-001] [AMB:AMB-001] [FR-001] [FR-006]: Describe Q4 scale/rounding layer by layer and explicitly distinguish saturation helpers from `roundedDamage`'s signed int32 wrap at its pre-division addition.
- **DEC-002** [CQ-002] [AMB:AMB-002] [FR-005]: M2 maps only representative damage subjects and cites exact/scoped Q4 qualification; M5 retains full correspondence and replay teaching.
- **DEC-003** [CQ-003] [AMB:AMB-003] [FR-006]: Every `quint authority=sir-combat` handbook fence must exactly match the deterministic extraction from `docs/rules/sir-combat.md`, and that extraction is the only model executed.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
- None.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 361-handbook-m2`.
