---
schemaVersion: 1
workId: 146-authoritative-live-snapshots
title: Authoritative Live Snapshots
stage: clarify
changeTier: tier1
status: clarified
sourceSpec: work/146-authoritative-live-snapshots/spec.md
publicOrToolFacingImpact: true
---

# Authoritative Live Snapshots Clarifications

## Source Specification
- work/146-authoritative-live-snapshots/spec.md

## Clarification Questions
- CQ-001 [AMB-001]: Which route is authoritative for reconnect proof?
- CQ-002 [AMB-002]: How is disclosure bounded when a projection is unavailable or malformed?

## Answers
- CQ-001: Start from the product entry point and use player-emittable visible controls; direct message injection and `window` hooks are excluded.
- CQ-002: Reuse the shared tactical projection/disclosure boundary and render a deterministic diagnostic rather than raw transport or snapshot content.

## Decisions
- DEC-001 [CQ-001] [FR-002] accepted: Browser evidence follows the visible product command route through live connection and server authority.
- DEC-002 [CQ-002] [FR-001] [FR-003] accepted: Live snapshots enter Elmish as decoded messages and project through the existing shared tactical scene/disclosure seam.
- DEC-003 [FR-004] accepted: Remove the out-of-tree DOM mount, mutable module session, and test-only global hooks instead of retaining a parallel compatibility path.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
No blocking ambiguity remains.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 146-authoritative-live-snapshots`.
