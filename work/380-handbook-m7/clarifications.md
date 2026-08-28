---
schemaVersion: 1
workId: 380-handbook-m7
title: Handbook M7
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/380-handbook-m7/spec.md
publicOrToolFacingImpact: true
---

# Handbook M7 Clarifications

## Source Specification
- work/380-handbook-m7/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: What evidence shape makes each review attributable, exact-source, scoped, and machine-checkable?
- CQ-002 [AMB:AMB-002]: Which identities are published and how are they kept current?
- CQ-003 [AMB:AMB-003]: Does M7 rerun, bind, or redefine M6V render/performance evidence?

## Answers
- CQ-001 → commit one schema-v1 review manifest with four named subject approvals, exact reviewed source digests, reviewer identity, verdict, limits, and evidence routes; the M7 audit independently recomputes every binding.
- CQ-002 → record the exact candidate Git tree and authoritative source blob identities plus versions read from committed pins/locks and actual binaries; the audit regenerates and compares the complete record.
- CQ-003 → rerun M6V structural/accessibility/render inspection and its existing typed workload, or verify retained exact-source receipts; preserve the six-diagram workload, 100/200 ms budgets, and no-compositor/FPS limit verbatim.

## Decisions
- **DEC-001** [CQ-001] [AMB:AMB-001] [FR-001] [FR-002] [FR-003]: Four structured approvals are committed as review evidence, not as semantic authority; each is independently authored and exact-source bound.
- **DEC-002** [CQ-002] [AMB:AMB-002] [FR-004] [FR-005]: The publication record is mechanically regenerated from Git and pinned tool manifests, and the model-adjacent trigger points maintainers to that record and owner checklist.
- **DEC-003** [CQ-003] [AMB:AMB-003] [FR-006]: M7 consumes M6V by exact binding and replay under its existing declared route; it adds no budget, performance intent, or stronger capability claim.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
- None. All publication-shape ambiguities are resolved above.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 380-handbook-m7`.
