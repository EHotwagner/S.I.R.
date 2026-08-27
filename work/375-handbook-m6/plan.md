---
schemaVersion: 1
workId: 375-handbook-m6
title: Handbook M6
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/375-handbook-m6/spec.md
sourceClarifications: work/375-handbook-m6/clarifications.md
sourceChecklist: work/375-handbook-m6/checklist.md
publicOrToolFacingImpact: true
---

# Handbook M6 Plan

Prose status: planned

## Source Snapshot
- spec: work/375-handbook-m6/spec.md sha256:897f09910e0b30ead33a3379da7555c681c8198dfb236b8d3d82ba3511ef7017 schemaVersion:1
- clarifications: work/375-handbook-m6/clarifications.md sha256:afb0ebe6ba83cc3e26ccfeb279761ab6be620eb0fa63035e9d84f4c30c28cd2a schemaVersion:1
- checklist: work/375-handbook-m6/checklist.md sha256:7feaa7d08686dffb7a4bfafcb445c7009046bd35fd32266b7e5865c438d232fd schemaVersion:1

## Plan Scope
- Work item 375-handbook-m6 is planned from the current specification, clarification, and checklist facts.
- Requirement count: 7.
- Clarification decision count: 3.
- Checklist result count: 7.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-003] complete: Replace every planned/Pending index entry with a concise complete definition, declaration locus, related canonical links, and scoped runtime statement pointing back to named authorities.
- PD-002 [AC-002] [FR-002] complete: Extend the vocabulary manifest with canonical alias records; validate one-to-one alias resolution and expose aliases in their canonical definitions.
- PD-003 [AC-003] [FR-003] complete: Derive Quint declarations from the literate fenced model and stable rules from the authoritative rule inventory, normalize declared kinds, and compare exact sets against manifest/index nodes.
- PD-004 [AC-004] [FR-004] complete: Build an anchor/link graph from parsed headings, explicit anchors, and inline links; require unique targets for fifty chapters, three reading paths, manifest anchors, and mandatory traceability subjects.
- PD-005 [AC-005] [FR-005] [DEC-001] [DEC-002] complete: Replace the M1 line/regex prototype with a dependency-free Markdown block/inline AST whose node kinds enforce the manifest-declared exemptions and inspect eligible prose text only.
- PD-006 [AC-006] [FR-006] complete: Apply four isolated in-memory mutations, require exact detector codes and observed-red results, then re-audit untouched input for restored green.
- PD-007 [AC-007] [FR-007] complete: Compose restore/build/strict docs, focused AST/reconciliation, negative controls, roadmap ledger, and current SDD analysis into one dedicated M6 qualification with JUnit receipts.

## Contract Impact
- PC-001 [PD-002] [PD-003] data contract: `docs/sir-combat-quint-vocabulary.json` advances to schema v2 with aliases and exact inventory reconciliation fields while retaining canonical term/anchor identities.
- PC-002 [PD-004] [PD-005] command contract: `work/375-handbook-m6/audit-handbook-structure.mjs` becomes the focused fail-closed audit used by M6 and documentation qualification.
- PC-003 [PD-007] evidence contract: `work/375-handbook-m6/qualify-handbook-m6.sh` emits dedicated JUnit and aggregate receipts under `readiness/375-handbook-m6/`.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PC-001] semanticTest: Assert 188/188 substantive definitions, canonical alias uniqueness, related-link resolution, and no placeholder/Pending fields.
- VO-002 [PD-003] [PC-001] contractTest: Assert exact model-declaration and sixteen-rule reconciliation, including kind and anchor identity.
- VO-003 [PD-004] [PD-005] [PC-002] contractTest: Assert unique internal destinations and zero eligible unlinked controlled occurrences from parsed Markdown nodes.
- VO-004 [PD-006] [PC-002] mutationTest: Observe detector-specific red for missing fragment, duplicate anchor, absent index entry, and unlinked controlled prose; rerun unchanged input green.
- VO-005 [PD-007] [PC-003] integrationTest: Build strict docs and run focused audits, roadmap state, full existing Q4/runtime qualification, and current lifecycle analysis.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] compatibleExtension: Vocabulary schema v2 retains every existing term/kind/anchor tuple and adds explicit aliases/inventory contracts; the focused audit diagnoses unsupported schemas and malformed aliases before reading handbook content.

## Generated View Impact
- GV-001 [PD-007] workModel: Regenerate analysis, work model, agent guidance, verify, ship, and summary views after task/evidence authoring; qualification rejects stale analysis before evidence can satisfy the milestone.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 375-handbook-m6`.
