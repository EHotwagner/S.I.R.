---
schemaVersion: 1
workId: 146-authoritative-live-snapshots
title: Authoritative Live Snapshots
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Authoritative Live Snapshots Specification

Prose status: specified

## User Value
Players see the server-authoritative live session in the persistent tactical workspace and can recover a dropped connection without leaving the visible product route.

## Scope
- SB-001: Integrate the existing HTTP/SignalR live slice with Elmish state, shared tactical rendering, visible command controls, diagnostics, server reconnect/resync behavior, browser tests, and qualification documentation.

## Non-Goals
- SB-002: Do not redesign tactical gameplay, map-editor authoring, or unrelated authentication and transport policy.

## User Stories
- US-001 (P1): As a player, I see the server-authoritative live projection in the same persistent battlefield I use for tactical work.
- US-002 (P1): As a player, I can use visible controls to advance, recover a disconnected session, and see the resynchronized authority.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given the product entry point, when an accepted live snapshot arrives, then `#persistent-tactical-svg` renders its shared tactical projection and the normal status UI reports the live connection state.
- AC-002 [US-002] [FR-002]: Given the product entry point, when a player uses visible advance, disconnect, reconnect, and resync commands, then the visible battlefield returns to the authoritative projection without window test hooks.
- AC-003 [US-001] [FR-003]: Given connection, decode, or resync failure, when it occurs, then normal diagnostics present a deterministic, knowledge-scoped failure state without exposing undisclosed authority.

## Functional Requirements
- FR-001: Live connection/session/snapshot state is represented by Elmish Model, Msg, and effects; accepted snapshots feed the shared tactical scene rendered by `#persistent-tactical-svg`. (Stories: US-001; Acceptance: AC-001)
- FR-002: Visible product commands drive advance, disconnect, reconnect, and resync; a browser journey starts at product entry and proves the server-authoritative visible battlefield changes and survives reconnect without window hooks. (Stories: US-002; Acceptance: AC-002)
- FR-003: Connection, decode, and resync failures are rendered through normal status/diagnostic UI with deterministic knowledge-scoped disclosure. (Stories: US-001; Acceptance: AC-003)
- FR-004: The fixed out-of-tree live aside, module-level mutable connection/session state, and test-only global functions are removed. (Stories: US-001; Acceptance: AC-001, AC-002)

## Ambiguities
No material ambiguities recorded.

## Public Or Tool-Facing Impact
- The Elmish client model/message/effect surface and visible live-session command controls change; browser qualification and client review bindings are regenerated when the bundle changes.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 146-authoritative-live-snapshots`.
