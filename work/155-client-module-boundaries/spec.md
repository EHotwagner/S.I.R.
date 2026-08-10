---
schemaVersion: 1
workId: 155-client-module-boundaries
title: Client Module Boundaries
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Client Module Boundaries Specification

Prose status: specified

## User Value
Client changes can be made across explicit application, mode, browser, editor-domain, and test boundaries without changing the production player workflows.

## Scope
- SB-001: Decompose only src/SIR.Client, src/SIR.Client.Web, and tests/SIR.Client.Tests; preserve one root Elmish model/program and current browser/player behavior.
- SB-002: Keep browser persistence, file/download, worker/live effects at the browser edge; keep MapEditor serialization compatible with MapEditorInterchange.

## Non-Goals
- SB-003: Do not change player-facing command semantics, persistence formats, or the existing renderer contract.

## User Stories
- US-001 (P1): As a player, I can use the editor, planning, simulator, and replay workspaces with unchanged behavior while their responsibilities are independently maintained.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given the production Web client is built, when a player exercises each workspace, then one root Elmish program delegates shell, per-mode, scene, command, panel, and browser-edge responsibilities to explicit modules without changing routes.
- AC-002 [US-001] [FR-002]: Given a map is edited, imported/exported, validated, revised, or projected, when the deterministic editor qualification runs, then public types, history/revision, validation, and interchange/projection ownership remain separated and compatible.
- AC-003 [US-001] [FR-003]: Given future code changes, when qualification runs, then ownership-size gates and focused contract tests fail on subject regrowth or malformed machine-readable test-report invocation.

## Functional Requirements
- FR-001: The client MUST retain one root Elmish model/program while App delegates shell state, per-mode adaptation, shared scene commands, command registry, review panels, and browser file/persistence/worker/live effects to explicit compilation-ordered modules. (covers AC-001)
- FR-002: MapEditor MUST separate public types, history/revision and validation ownership while preserving MapEditorInterchange serialization and deterministic projection/update behavior. (covers AC-002)
- FR-003: Client qualification MUST expose a deterministic JUnit report mode that rejects a missing/blank path before running qualifications, split report-contract ownership from the broad test executable, and enforce App/MapEditor anti-regrowth ceilings by inspecting production subjects. (covers AC-003)

## Ambiguities
No material ambiguities recorded.

## Public Or Tool-Facing Impact
- This specification is an SDD lifecycle artifact and command-report contract input.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 155-client-module-boundaries`.
