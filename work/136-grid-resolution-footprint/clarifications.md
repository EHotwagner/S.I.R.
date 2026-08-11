---
schemaVersion: 1
workId: 136-grid-resolution-footprint
title: Grid Resolution Footprint
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/136-grid-resolution-footprint/spec.md
publicOrToolFacingImpact: true
---

# Grid Resolution Footprint Clarifications

## Source Specification
- work/136-grid-resolution-footprint/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001] resolved: What scale and compatibility boundary govern persisted coordinates?
- CQ-002 [AMB:AMB-002] resolved: Does finer resolution remove semantic edges?

## Answers
- CQ-001: Multiply grid coordinates, dimensions, distances, speeds, and ranges by two at the compatibility boundary; write the current format and deterministically migrate supported legacy formats.
- CQ-002: No. Semantic edges remain explicit topology constraints; resolution alone does not replace authored blocking-edge semantics.

## Decisions
- DEC-001 [AMB:AMB-001] resolved: The canonical human footprint is 4×4 high-resolution cells; all former cell-unit geometry uses a factor-of-two migration rule.
- DEC-002 [AMB:AMB-001] resolved: Readers migrate recognized legacy data deterministically and reject unknown versions with an actionable compatibility diagnostic.
- DEC-003 [AMB:AMB-002] resolved: Preserve semantic edges and test them at the new resolution rather than deriving their meaning from grid size.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
No remaining blocking ambiguity.

## Lifecycle Notes
The implementation must locate the existing producer-owned performance intent and
representative workload before changing interactive routes.
