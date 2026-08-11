---
schemaVersion: 1
workId: 153-separated-project-graph
title: Separated Project Graph
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Separated Project Graph Specification

Prose status: specified

## User Value
Maintainers can evolve live play, replay/web editing, generated protocol, and runtime adapters through explicit project boundaries.

## Scope
- SB-001: Restore the documented separated project graph while preserving the working vertical slice.
- SB-002: Establish separate Wasm, generated-protocol, replay-web, and tools ownership without introducing a server/framework dependency into the deterministic kernel.
- SB-003: Make the canonical architecture, protocol document, integration report, solution, and machine guards state the same graph.

## Non-Goals
- SB-004: Do not replace the released HTTP/Thoth and SignalR transport with Fable.Remoting or another transport.
- SB-005: Do not change game/replay semantics merely to make extraction convenient.

## User Stories
- US-001 (P1): As a maintainer, I can evolve live play, replay/web editing, generated protocol, and runtime adapters through explicit project boundaries.
- US-002 (P1): As a reviewer, I can verify the declared graph from the solution, project references, canonical documents, and a machine-readable guard.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given the solution is restored, when a maintainer inspects it, then it contains `SIR.Wasm`, `SIR.Protocol.Generated`, `SIR.Replay.Web`, and `SIR.Tools` with the documented responsibility boundaries.
- AC-002 [US-001] [FR-002]: Given live-client and replay/editor code, when dependencies are evaluated, then `SIR.Client` has no `SIR.Simulation` reference and replay/editor simulation ownership is confined to `SIR.Replay.Web`.
- AC-003 [US-002] [FR-003]: Given the restored solution and canonical documents, when the architecture guard runs, then it rejects each load-bearing forbidden edge and a documentation/project mismatch.
- AC-004 [US-002] [FR-004]: Given the existing live vertical slice, when the normal conformance route runs, then the server, client bundle, and browser workflow remain operational.

## Functional Requirements
- FR-001: The solution MUST include and build separated `SIR.Wasm`, `SIR.Protocol.Generated`, `SIR.Replay.Web`, and `SIR.Tools` projects with their canonical responsibilities. (covers AC-001)
- FR-002: `SIR.Client` MUST not reference `SIR.Simulation`; replay/editor Fable code that requires simulation MUST be owned by `SIR.Replay.Web`. (covers AC-002)
- FR-003: A deterministic architecture guard MUST compare the solution/project references with the canonical graph and reject every load-bearing forbidden edge or canonical-document mismatch. (covers AC-003)
- FR-004: The existing HTTP/Thoth plus SignalR vertical slice and production browser workflow MUST remain compatible after the project-graph migration. (covers AC-004)

## Ambiguities
- AMB-001: The original `SIR.Protocol.Generated` document says generated gRPC contracts, while the released scaffold uses Thoth HTTP and SignalR codecs; the delivery decision preserves the released transport but requires a generated-project boundary.

## Public Or Tool-Facing Impact
- The solution graph and project names are build- and tool-facing surfaces.
- The architecture guard is a tool-facing executable contract.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 153-separated-project-graph`.
