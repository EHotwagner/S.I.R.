---
schemaVersion: 1
workId: 239-durable-rules-identity
title: Durable Rules Identity
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/239-durable-rules-identity/spec.md
sourceClarifications: work/239-durable-rules-identity/clarifications.md
sourceChecklist: work/239-durable-rules-identity/checklist.md
publicOrToolFacingImpact: true
---

# Durable Rules Corpus Source Identity Plan

Prose status: planned

## Source Snapshot
- spec: work/239-durable-rules-identity/spec.md sha256:2585aa4de12d508fbeed1fb3b9040035e7c6a9bd94e94b816648bbd3c068a9a1 schemaVersion:1
- clarifications: work/239-durable-rules-identity/clarifications.md sha256:aae5eb9f63f27ea8396dbfbe81dcc2f330004db3467a8599e1fca7ce8dd2f42b schemaVersion:1
- checklist: work/239-durable-rules-identity/checklist.md sha256:4aac1b20b51087652579e9a80e531ecb558fe67c7fe5be518e5eebf41dd6f524 schemaVersion:1

## Plan Scope
- Work item 239-durable-rules-identity is planned from the current specification, clarification, and checklist facts.
- Requirement count: 4.
- Clarification decision count: 2.
- Checklist result count: 4.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Pin the intended normalized implementation sources to squash commit `eb0b2c29a80f0bf3b400ce4415bf8587b4645083`, then regenerate the implementation digest, manifest identity, coverage, and representative application from that one source identity.
- PD-002 [AC-002] [AC-003] [FR-002] complete: Add an early verifier boundary that validates forty-hex syntax, commit-object existence, explicit `refs/remotes/origin/main` existence, and ancestry in that order; run isolated subject mutations for malformed, absent, local-only, and missing-canonical-ref cases.
- PD-003 [AC-004] [FR-003] complete: Clone the exact pushed candidate from GitHub with `git clone --no-local`, assert `.git/objects/info/alternates` is absent, and run the production verifier inside that clone.
- PD-004 [AC-005] [AC-006] [FR-004] complete: Complete and ship this SDD package, rebind the established rules-governance evidence cone and protected boundary to current corpus identities plus `readiness/239-durable-rules-identity/ship.json`, then require hosted `rules` and aggregate verdict checks on the exact head.

## Contract Impact
- PC-001 [PD-001] [PD-002] artifact identity: `implementation-sources.json`, `manifest.json`, retained generated fixtures, and the verifier's source-durability diagnostics change as one compatibility-preserving corpus contract; no gameplay-rule ids or semantics change.
- PC-002 [PD-004] protected boundary: Governance receipts retain their established output location but bind the current SDD ship artifact and regenerated corpus identities.

## Verification Obligations
- VO-001 [PD-001] [PC-001] semanticTest: Run `scripts/verify-rules-corpus.sh` and require generation, source, digest, coverage, representative-application, and embedded mutation checks to pass.
- VO-002 [PD-002] [PC-001] mutationTest: Mutate isolated checkout subjects for malformed, missing, local-only/non-ancestor, and missing `refs/remotes/origin/main` identities; require non-zero refusal with the expected actionable class before generation.
- VO-003 [PD-003] [PC-001] productionJourney: In a fresh full GitHub network clone of the exact candidate, prove no alternates and require `scripts/verify-rules-corpus.sh` to pass.
- VO-004 [PD-004] [PC-002] integrationTest: Require the Governance generator/check path to report no effective blocking findings and hosted exact-head `rules` plus aggregate verdicts to pass.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] replaceIdentity: The unreachable pre-squash commit and its derived digests are replaced atomically; historical local object retention is not preserved as a fallback.
- PM-002 [PC-002] rebindReceipt: Existing rules-governance receipt paths remain stable while their content is regenerated against the new manifest, semantic evidence, retained artifacts, and current work ship identity.

## Generated View Impact
- GV-001 [PD-001] [PD-004] corpusAndGovernance: Regenerate v2 corpus fixtures, this work item's readiness views, and the affected Governance binding/verdict/protected-boundary receipts; byte or digest drift blocks verification.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 239-durable-rules-identity`.
