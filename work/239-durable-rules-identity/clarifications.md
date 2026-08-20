---
schemaVersion: 1
workId: 239-durable-rules-identity
title: Durable rules corpus source identity
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/239-durable-rules-identity/spec.md
publicOrToolFacingImpact: true
---

# Durable Rules Corpus Source Identity Clarifications

## Source Specification
- work/239-durable-rules-identity/spec.md

## Clarification Questions
- **CQ-001** (AMB-001): Which Git ref is the canonical reachability boundary in both full clones and hosted checkout ref shapes?
- **CQ-002** (AMB-002): Which SDD ship artifact must the resealed protected-boundary receipt bind?

## Answers
- CQ-001 → use explicit `refs/remotes/origin/main`; do not depend on optional `refs/remotes/origin/HEAD`.
- CQ-002 → bind `readiness/239-durable-rules-identity/ship.json`, produced by the current required delivery route; the historical work 198 ship cannot attest the changed identity.

## Decisions
- **DEC-001** [CQ-001] [AMB:AMB-001] [FR-002] [AC-002] [AC-003]: Source durability means the declared commit exists and is an ancestor of explicit `refs/remotes/origin/main`; absence of either object or ref is an early actionable refusal.
- **DEC-002** [CQ-002] [AMB:AMB-002] [FR-004] [AC-005] [AC-006]: Governance resealing binds this work item's current ship artifact while retaining the established rules-governance receipt location.

## Accepted Deferrals
- None.

## Remaining Ambiguity
- None. AMB-001 and AMB-002 are resolved by DEC-001 and DEC-002.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 239-durable-rules-identity`.
