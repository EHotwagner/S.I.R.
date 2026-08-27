---
schemaVersion: 1
workId: 361-handbook-m2
title: Handbook M2
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/361-handbook-m2/spec.md
sourceClarifications: work/361-handbook-m2/clarifications.md
sourceChecklist: work/361-handbook-m2/checklist.md
publicOrToolFacingImpact: true
---

# Handbook M2 Plan

Prose status: planned

## Source Snapshot
- spec: work/361-handbook-m2/spec.md sha256:9c116950fb1c871bf288183e06224bf851d64f2b0632a00f646b54b2e7932c1a schemaVersion:1
- clarifications: work/361-handbook-m2/clarifications.md sha256:5033519f19f00ab49faa4264b2044b282ad9dabac0e8fbf8a727c9b345746f91 schemaVersion:1
- checklist: work/361-handbook-m2/checklist.md sha256:455a461651b128524fdc548bfbaa1b0dfbd98ac91cf7392db79ad3fbab95dfb5 schemaVersion:1

## Plan Scope
- Work item 361-handbook-m2 is planned from the current specification, clarification, and checklist facts.
- Requirement count: 7.
- Clarification decision count: 3.
- Checklist result count: 7.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Replace only M2-assigned placeholders with a layered representative attack spine: domain pipeline, raw Q4 table, trace/retention/damage/rounding derivation, and explicit int32 claim boundary.
- PD-002 [AC-002] [FR-002] complete: Teach the three record roles with compact field tables, then connect pure helpers to the guarded atomic `resolveConsequences` action and its completed observation.
- PD-003 [AC-003] [FR-003] complete: Center the tutorial on predict-run-observe-explain, show the authoritative representative attack/action/run excerpts, and decode a two-state execution trace field by field.
- PD-004 [AC-004] [FR-004] complete: Add one focused audit that extracts authority, runs the named witness green, mutates only retention `8000` to `7000`, requires the witness red with actual damage `18`, and re-extracts/runs green.
- PD-005 [AC-005] [FR-005] complete: Add a bounded representative correspondence map to current F# subjects and existing exact/sampled qualification, with separate model/runtime evidence labels and no generalized equivalence claim.
- PD-006 [AC-006] [FR-006] complete: Reuse M1's structural link audit, add an M2 provenance/content/Quint audit and JUnit receipt, run the full Q4 qualification, and render the strict docs site.
- PD-007 [AC-007] [FR-007] complete: Update only M2's ledger checkbox after verification and bind it to the handbook audit, Q4 gate, lifecycle artifacts, schema-v2 feedback, review, PR, and merge.

## Contract Impact
- PC-001 [PD-001] documentation contract: Existing handbook anchors and vocabulary manifest identifiers remain stable; M2 replaces selected placeholders and pending definitions without renaming the public fragments.

## Verification Obligations
- VO-001 [PD-004] [PD-006] [PC-001] semanticTest: Run `node work/361-handbook-m2/audit-representative-attack.mjs`, `node work/359-handbook-m1/audit-handbook-links.mjs`, `./scripts/qualify-quint-q4-sir-combat.sh`, and `./scripts/build-docs.sh --prepare-site-only`; publish a focused JUnit receipt.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additive: M2 extends the existing one-file publication without changing stable routes or fragments; later milestones replace remaining placeholders in place.

## Generated View Impact
- GV-001 [PD-006] workModel: Refresh SDD analysis/verification/ship views from current authored sources; docs render and extracted Quint files remain ephemeral outputs.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 361-handbook-m2`.
