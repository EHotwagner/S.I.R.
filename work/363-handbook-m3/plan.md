---
schemaVersion: 1
workId: 363-handbook-m3
title: Handbook M3
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/363-handbook-m3/spec.md
sourceClarifications: work/363-handbook-m3/clarifications.md
sourceChecklist: work/363-handbook-m3/checklist.md
publicOrToolFacingImpact: true
---

# Handbook M3 Plan

Prose status: planned

## Source Snapshot
- spec: work/363-handbook-m3/spec.md sha256:1a363542b212eb073682849f60b6dbfd386edbe49ea7983a4190dbde1b822325 schemaVersion:1
- clarifications: work/363-handbook-m3/clarifications.md sha256:a1a75bbc33ab2f8d50949ea5c5666df32cb977bd79ccdb9018f2dce6c0c729eb schemaVersion:1
- checklist: work/363-handbook-m3/checklist.md sha256:2c2dbce4a830ff2bee9791f0b626e419ffe6fb040efc24c5faa24f069f5f102c schemaVersion:1

## Plan Scope
- Edit the existing handbook in place, preserving M2 content and its stable anchors.
- Add a catalogue table and dependency graph sourced from the authoritative `ruleCatalogue`, then expand the existing rule chapters and reference/traceability sections around exact model subjects.
- Add dedicated Node audits and shell qualification that extract/check the literate Quint authority, run positive named examples under Quint 0.32.0, build strict docs, run the full Q4/runtime qualification, and emit scoped JUnit receipts.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Derive one sixteen-row catalogue and a direct-dependency graph from `ruleCatalogue`, with explanation order shown separately and labelled non-causal.
- PD-002 [AC-002] [FR-002] complete: Give each fact/formula/algorithm/transition an existing Quint visibility subject and mechanically reject missing or invented subject names.
- PD-003 [AC-003] [FR-003] [DEC-001] [DEC-002] complete: Build focused walkthroughs around current helpers, `resolveConsequences`, `resolveCoverImpact`, `recoveredSuppression`, `resolveRecovery`, observations, runs, and invariants; preserve aggregate atomicity and formula-level penetration.
- PD-004 [AC-004] [FR-004] complete: Teach line of sight from `traceAlgorithm` and `traceRaw`, citing the external implementation/fingerprint and explicitly excluding a duplicate supercover implementation.
- PD-005 [AC-005] [FR-005] complete: Replace every rule-reference and traceability placeholder with exactly one complete row/entry, then audit sixteen-of-sixteen identity, fields, uniqueness, dependencies, and locators.
- PD-006 [AC-006] [FR-006] [DEC-003] complete: Add tiered prediction/interpretation/design exercises plus answer guidance using positive authority behavior only; reserve semantic mutations for M4.
- PD-007 [AC-007] [FR-007] complete: Own six scoped receipts—docs, links, focused M3 model, full Q4/runtime, roadmap ledger, and lifecycle—plus an aggregate qualification receipt whose six named test cases and owning component receipts preserve claim granularity.
- PD-008 [AC-008] [FR-008] complete: Update only M3's checkbox/evidence at merge-candidate time and validate that prior history and later unchecked milestones remain unchanged.

## Contract Impact
- PC-001 [PD-001] [PD-005] [PD-007] documentationContract: `docs/sir-combat-quint-handbook.md`, `docs/sir-combat-quint-handbook-roadmap.md`, `work/363-handbook-m3/*.mjs`, qualification commands, and scoped JUnit receipts are new public documentation/evidence surfaces; no code API or schema changes.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-003] [PD-004] [PD-005] [PD-006] [PD-007] [PD-008] [PC-001] semanticTest: Run structural negative controls, authoritative subject/excerpt checks, positive focused Quint runs, strict docs/link qualification, full Q4/runtime qualification, roadmap ledger audit, lifecycle currency, and an independent exact-head review before merge.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] compatibility: Existing handbook anchors and M2 content remain stable; the milestone only fills scheduled M3 sections and adds additive qualification/lifecycle artifacts.

## Generated View Impact
- GV-001 [PD-007] [PD-008] workModel: `readiness/363-handbook-m3/` is regenerated from current lifecycle sources and qualification runs; stale work-model, analysis, verify, ship, or scoped receipts block handoff.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- No framework API is introduced and no dependency-surface capture is needed.
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Implement only after `analyze` reports implementation-ready.
- Next lifecycle action: `fsgg-sdd tasks --work 363-handbook-m3`.
