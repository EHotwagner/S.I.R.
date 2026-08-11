---
schemaVersion: 1
workId: 136-grid-resolution-footprint
title: Grid Resolution Footprint
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Grid Resolution Footprint Specification

Prose status: specified

## User Value
Players can position human units naturally on a finer battlefield grid without changing their apparent physical size.

## Scope
- SB-001: Battlefield domain, simulation, editor/client rendering, serialization, migrated content, documentation, and .NET/Fable/browser tests.

## Non-Goals
- SB-002: Redesign non-human unit archetypes or unrelated map systems.

## User Stories
- US-001 (P1): As a player, I can position human units naturally on a finer grid without changing their physical world size.
- US-002 (P1): As an editor user, I can see and validate a complete human footprint before placing it.
- US-003 (P1): As an operator, I receive deterministic migration or actionable rejection when loading legacy battlefield data.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given a human unit, when it occupies the battlefield, then it occupies exactly 4×4 cells and renders at its prior apparent world size.
- AC-002 [US-001] [FR-002]: Given collision, occupancy, pathfinding, clearance, line-of-sight, range, or movement cost evaluation, when it uses grid geometry, then it applies the doubled-resolution scale consistently.
- AC-003 [US-002] [FR-003]: Given editor placement or selection, when a human is previewed, then the complete 4×4 footprint is visible and invalid placement is rejected.
- AC-004 [US-003] [FR-004]: Given legacy map, save, replay, or import data, when it is read, then it is deterministically migrated under the documented scale rule or rejected with an actionable compatibility message.
- AC-005 [US-001] [FR-005]: Given .NET, Fable, and browser test routes, when focused fixtures cover edge placement, narrow passages, diagonal movement, rendering scale, and legacy data, then each route reports the same intended behavior.

## Functional Requirements
- FR-001: A human unit occupies exactly 4×4 grid cells and retains its prior apparent world size when rendered. (Stories: US-001; Acceptance: AC-001)
- FR-002: Domain and simulation geometry for collision, occupancy, pathfinding, clearance, line-of-sight, range, and movement cost uses one doubled-resolution rule. (Stories: US-001; Acceptance: AC-002)
- FR-003: Editor placement and selection previews the full 4×4 footprint and rejects placements that violate occupancy or map bounds. (Stories: US-002; Acceptance: AC-003)
- FR-004: Persisted map, save, replay, and import data has one documented deterministic migration rule, or an actionable compatibility rejection where migration is unsupported. (Stories: US-003; Acceptance: AC-004)
- FR-005: Automated .NET, Fable, and browser tests prove footprint occupancy, narrow passages, diagonal movement, edge placement, rendering scale, and legacy-data behavior. (Stories: US-001; Acceptance: AC-005)

## Ambiguities
- AMB-001: The canonical multiplier and compatibility version boundary for persisted coordinates require an explicit decision.
- AMB-002: Whether semantic edges remain needed after the resolution change requires investigation and an explicit disposition.

## Public Or Tool-Facing Impact
- This specification is an SDD lifecycle artifact and command-report contract input.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 136-grid-resolution-footprint`.
