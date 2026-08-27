---
schemaVersion: 1
workId: 356-handbook-m0
title: Handbook M0
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/356-handbook-m0/spec.md
sourceClarifications: work/356-handbook-m0/clarifications.md
sourceChecklist: work/356-handbook-m0/checklist.md
publicOrToolFacingImpact: true
---

# Handbook M0 Plan

Prose status: planned

## Source Snapshot
- spec: work/356-handbook-m0/spec.md sha256:c847ecd7b032f08539f4a67fd4a06ed4e148a6b1ee21965c03b140c6cc56d4e7 schemaVersion:1
- clarifications: work/356-handbook-m0/clarifications.md sha256:9e87e8cf8bd197c582205f7e0dfda87897e5cded6eb5659c4e602a90948c24ca schemaVersion:1
- checklist: work/356-handbook-m0/checklist.md sha256:0a76d20777551d833c46f72a559857c01e1daace1476954f716b246f570f13c6 schemaVersion:1

## Plan Scope
- Work item 356-handbook-m0 is planned from the current specification, clarification, and checklist facts.
- Requirement count: 6.
- Clarification decision count: 2.
- Checklist result count: 6.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: Build a source map from current tracked ADR/design/runtime/evidence files plus commit-bound cross-project design and Q4 candidate sources; label status and precedence explicitly.
- PD-002 [AC-002] [FR-002] [DEC-001] complete: Transcribe the sixteen-entry registry once into the ledger with direct dependencies and verify the inventory mechanically for uniqueness, count, and required IDs.
- PD-003 [AC-003] [FR-003] [DEC-001] complete: Derive the top-level declaration and property inventory from PR #355's literate Quint blocks, excluding local `val` bindings, and classify each entry for the future definition index.
- PD-004 [AC-004] [FR-004] complete: Seed four controlled-vocabulary classes with canonical spellings and intended definition categories without attempting M6 occurrence-link enforcement.
- PD-005 [AC-005] [FR-005] [DEC-001] complete: Record exclusions and a disagreement register whose resolution/status column states whether state shape or action granularity is affected.
- PD-006 [AC-006] [FR-006] [DEC-002] complete: Import the M0-M7 ledger wording, reserve the handbook target path, and append M0 completion evidence without creating M1 content.

## Contract Impact
- PC-001 [PD-001] [PD-002] [PD-003] [PD-004] [PD-005] [PD-006] documentation contract: `docs/sir-combat-quint-handbook-roadmap.md` is the durable roadmap ledger; milestone wording and checkbox history are preserved while completion evidence is append-only beneath each milestone.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-003] [PD-004] [PD-005] [PD-006] [PC-001] semanticTest: Run a focused ledger audit that asserts exactly one checked milestone (M0), seven unchecked milestones, sixteen unique required rule IDs, all required inventory headings, the candidate/current authority distinction, and the reserved handbook target; also run repository documentation qualification.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] documentationOnly: No model/runtime migration occurs; PR #355 remains candidate authority and the handbook remains uncreated until M1.

## Generated View Impact
- GV-001 [PD-001] [PD-002] [PD-003] [PD-004] [PD-005] [PD-006] workModel: Refresh analysis, evidence, verify, ship, summary, and agent guidance from the complete authored lifecycle package.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 356-handbook-m0`.
