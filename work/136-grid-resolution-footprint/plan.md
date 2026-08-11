---
schemaVersion: 1
workId: 136-grid-resolution-footprint
title: Grid Resolution Footprint
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/136-grid-resolution-footprint/spec.md
sourceClarifications: work/136-grid-resolution-footprint/clarifications.md
sourceChecklist: work/136-grid-resolution-footprint/checklist.md
publicOrToolFacingImpact: true
---

# Grid Resolution Footprint Plan

Prose status: planned

## Source Snapshot
- spec: work/136-grid-resolution-footprint/spec.md sha256:1c4879d5b6221ffbb2b8ef594f675854655afdab761ab474645696caea089192 schemaVersion:1
- clarifications: work/136-grid-resolution-footprint/clarifications.md sha256:cce9d068f0bc2ddd6166fbf046bf59996e4ff8fc8b4b2869631287eace240d3b schemaVersion:1
- checklist: work/136-grid-resolution-footprint/checklist.md sha256:37b660f98d201bcb1ed1110ef7bf47b849b012fe16f170a0a9d55988bab2eacf schemaVersion:1

## Plan Scope
Introduce a single doubled-resolution conversion at the domain/serialization boundary,
then route canonical 4×4 human dimensions through simulation, editor, and renderer.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Replace the canonical human footprint preset and visual projection with 4×4 high-resolution cells while preserving world-space rendering dimensions.
- PD-002 [AC-002] [FR-002] [DEC-001] [DEC-003] complete: Centralize doubled grid units in MapScale and apply them to occupancy, movement, distance, LOS, range, clearance, and explicit semantic-edge checks.
- PD-003 [AC-003] [FR-003] complete: Route the authoritative footprint into editor preview, selection, bounds, terrain, occupancy, and blocking-edge placement validation.
- PD-004 [AC-004] [FR-004] [DEC-002] complete: Version or normalize interchange/replay readers so recognized legacy coordinates migrate deterministically and unknown versions fail with an actionable error.
- PD-005 [AC-005] [FR-005] complete: Add focused .NET, Fable, and browser production-route fixtures for 4×4 occupancy, narrow/diagonal/edge cases, render scale, and legacy compatibility.

## Contract Impact
No standalone contract-impact identifier is required; PD-004 owns the persisted scale/version interpretation.

## Verification Obligations
No standalone verification-obligation identifier is required; PD-005 owns the production-route and mutation evidence.

## Performance Intent
Use the existing producer-owned battlefield workload and declared target; inspect it
before implementation and do not invent a timing budget.

## Migration Posture
No standalone migration-posture identifier is required; PD-004 owns deterministic legacy scaling and rejection behavior.

## Generated View Impact
No standalone generated-view identifier is required; the lifecycle generators derive views from the current authored sources.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 136-grid-resolution-footprint`.
