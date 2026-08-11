---
schemaVersion: 1
workId: 142-stable-working-surface
title: Stable Working Surface
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/142-stable-working-surface/spec.md
sourceClarifications: work/142-stable-working-surface/clarifications.md
sourceChecklist: work/142-stable-working-surface/checklist.md
publicOrToolFacingImpact: true
---

# Stable Working Surface Plan

Prose status: planned

## Source Snapshot
- spec: work/142-stable-working-surface/spec.md sha256:c713ae7f96069a328e55c403ffa2529e8e049d5eef44d5abfa3094c13ae3da63 schemaVersion:1
- clarifications: work/142-stable-working-surface/clarifications.md sha256:477ad449e8d8f0b73ec723281f871d3800e0c533cf61c9c4cb4f9445442dc5e8 schemaVersion:1
- checklist: work/142-stable-working-surface/checklist.md sha256:2bb9e3efe9351eccdf07faf590ed2bac072989fc8ae7bd417ac211ac233e3987 schemaVersion:1

## Plan Scope
- Work item 142-stable-working-surface is planned from the current specification, clarification, and checklist facts.
- Requirement count: 3.
- Clarification decision count: 0.
- Checklist result count: 3.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Select the editor projection as the concrete fallback whenever a derived mode has no accepted projection; retain the shared SVG and its editor-owned camera.
- PD-002 [AC-002] [FR-002] complete: Continue selecting a mode's authoritative projection when it exists, so fallback never masks valid simulator or replay output.
- PD-003 [AC-003] [FR-003] complete: Reuse the existing mode-specific selected-unit reconciliation and assert browser-visible scene, camera, and selection values over real mode controls.

## Contract Impact
- PC-001 [PD-001] internal renderer seam: `activeSceneProjection` remains internal; its `SharedSceneProjection option` now has an editor fallback and preserves existing F# signatures.

## Verification Obligations
- VO-001 [PD-001] [PC-001] browserTest: Exercise the production mode buttons without simulator/replay input and verify viewBox, data-scene revision, camera, and valid selected-unit continuity.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] notApplicable: No persisted data or migration is changed because the fallback is a render-time choice.

## Generated View Impact
- GV-001 [PD-001] workModel: Refresh generated readiness after the authored plan and evidence change so lifecycle receipts bind the actual fallback decision.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 142-stable-working-surface`.
