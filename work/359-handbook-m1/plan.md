---
schemaVersion: 1
workId: 359-handbook-m1
title: Handbook M1 linked skeleton
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/359-handbook-m1/spec.md
sourceClarifications: work/359-handbook-m1/clarifications.md
sourceChecklist: work/359-handbook-m1/checklist.md
publicOrToolFacingImpact: true
---

# Handbook M1 linked skeleton Plan

Prose status: planned

## Source Snapshot
- spec: work/359-handbook-m1/spec.md sha256:9484dd23e3dcf19f5099a97ea75cebdfabc76fb3f7e03b8c4e975922e9a367a7 schemaVersion:1
- clarifications: work/359-handbook-m1/clarifications.md sha256:6b0c28ce8ee0b19215515bca3dde65b2062baae0c5e97228541b0af5fc2b159a schemaVersion:1
- checklist: work/359-handbook-m1/checklist.md sha256:8a5b196af2fa2e4b265422adbe1bb5b2882c86729670bb522d4ff58cf292d649 schemaVersion:1

## Plan Scope
- Work item 359-handbook-m1 is planned from the current specification, clarification, and checklist facts.
- Requirement count: 6.
- Clarification decision count: 3.
- Checklist result count: 6.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Author one fsdocs Markdown publication with explicit anchors for Parts I-VIII and all fifty chapter headings; use those anchors in the table of contents and three reading-path maps.
- PD-002 [AC-002] [FR-002] complete: Seed one alphabetical index from the complete M0 inventory and bind each entry to a unique semantic anchor recorded in the checked JSON manifest.
- PD-003 [AC-003] [FR-003] complete: Reserve matrix rows for sixteen rule identities, seven Q4 decisions, and every named source-design behavior and replay obligation; label later mappings pending.
- PD-004 [AC-004] [FR-004] complete: Implement a dependency-free Markdown structural audit that excludes front matter, fences, headings, and the canonical index while checking links, anchors, inventory membership, and controlled prose occurrences; exercise four in-memory negative mutations.
- PD-005 [AC-005] [FR-005] complete: Render the publication with the repository's strict fsdocs path and add its published route to the README start list.
- PD-006 [AC-006] [FR-006] complete: Mark only M1 checked after focused and rendered evidence passes and append immutable artifact and cycle paths without rewriting earlier roadmap history.

## Contract Impact
- PC-001 [PD-001] documentation contract: `docs/sir-combat-quint-handbook.md`, `docs/sir-combat-quint-vocabulary.json`, and semantic fragment identifiers are public navigation surfaces; later milestones extend definitions without renaming anchors.

## Verification Obligations
- VO-001 [PD-001] [PC-001] semanticTest: Run `node work/359-handbook-m1/audit-handbook-links.mjs`, confirm all four deliberate mutations are detected, render through `./scripts/build-docs.sh --prepare-site-only`, and assert the handbook HTML exists.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additive: M1 adds the publication and stable anchors without redirecting or removing any current documentation route; a later page split must preserve these fragments.

## Generated View Impact
- GV-001 [PD-001] workModel: Refresh the SDD work model and generated summary after evidence; generated site output remains ephemeral and is rebuilt by the docs pipeline.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 359-handbook-m1`.
