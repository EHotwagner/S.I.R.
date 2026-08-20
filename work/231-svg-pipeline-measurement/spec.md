---
schemaVersion: 1
workId: 231-svg-pipeline-measurement
title: Svg Pipeline Measurement
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# SVG Pipeline Measurement Specification

Prose status: specified; Tier 1 browser-performance harness and evidence contract.

## User Value
Maintainers can identify the next retained-SVG bottleneck from exact-candidate production Chromium evidence instead of optimizing by assumption.

## Scope
- SB-001: Versioned scalable fixtures independently vary map extent, visible density, global unit count, route/overlay complexity, event rate, and supporting-list size.
- SB-002: Focused production-Chromium journeys cover idle, playback, pan, zoom, selection, modality transition, and dense overlays against the built product.
- SB-003: Measurements separate application and browser pipeline stages, structural cost, frame health, interaction latency, and memory after warm-up and stabilization.
- SB-004: Artifacts bind the exact candidate, fixture definition digest, journey identity, browser/runtime/capability facts, trace, and deterministic summary.

## Non-Goals
- SB-005: Do not optimize the renderer, change simulation or presentation semantics, add culling/caching/batching, or introduce packed/typed transport in this item.
- SB-006: Do not declare a permanent supported project-size ceiling from the fixture matrix or noisy wall-clock results.
- SB-007: Do not add the production Chromium matrix to every small-PR CI route; it remains an independently runnable Release qualification.

## User Stories
- US-001 (P1): As a rendering maintainer, I can compare separated pipeline-stage costs and identify the next material bottleneck.
- US-002 (P1): As a performance reviewer, I can reproduce an exact fixture and journey against the exact built candidate and inspect its trace.
- US-003 (P1): As an architecture owner, I can distinguish global project scale from the visible working set and make transport/allocation decisions from evidence.

## Acceptance Scenarios
- AC-001 [US-001] [US-002] [FR-001] [FR-002] [FR-003]: Given a built Release candidate and a versioned fixture, when a named journey runs in production Chromium, then the artifact separates worker compute/transfer, projection/allocation, Elmish/React reconciliation, SVG style/layout/paint/compositor activity, DOM-by-layer counts, long tasks, dropped frames, and input latency rather than reporting one callback aggregate.
- AC-002 [US-003] [FR-004] [FR-005]: Given two fixtures with the same small visible viewport and density but materially different global extent/unit/list scale, when their idle and interaction journeys run, then the summary preserves both global and visible counters and compares their per-frame work without treating the larger fixture as unsupported.
- AC-003 [US-001] [US-002] [FR-006]: Given repeated pan, zoom, and playback cycles, when Chromium warms and then stabilizes, then the artifact records memory checkpoints after warm-up and after stabilization plus the delta and collection capability.
- AC-004 [US-001] [US-003] [FR-007] [FR-008]: Given the complete fixture/journey matrix, when the deterministic summary is produced, then it identifies the next material bottleneck with cited observations and classifies packed transport, typed buffers, and further allocation work as required or deferred without declaring a permanent size ceiling.
- AC-005 [US-002] [FR-009] [FR-010]: Given a maintainer outside normal PR CI, when they invoke the focused command, then it builds or consumes the exact candidate, runs only the requested production Chromium matrix, validates versioned artifacts, and fails visibly on missing capabilities or malformed/stale bindings.

## Functional Requirements
- FR-001: The harness MUST drive idle, playback, pan, zoom, selection, modality transition, and dense-overlay journeys through the built production client in Chromium. (Stories: US-001, US-002; Acceptance: AC-001)
- FR-002: Each journey artifact MUST separately report worker compute and transfer, scene projection and allocation, Elmish update and React reconciliation, SVG style/layout/paint/compositor activity, DOM counts by layer, long tasks, dropped frames, and input latency, and MUST disclose unavailable measurement capabilities rather than merging or inventing values. (Stories: US-001, US-002; Acceptance: AC-001)
- FR-003: Every artifact MUST bind the exact candidate commit/tree and build identity, fixture schema/id/digest, journey id, Chromium and runtime versions, host/capability facts, trace path/digest, start/end timestamps, and result. (Stories: US-002; Acceptance: AC-001)
- FR-004: The versioned fixture matrix MUST independently vary map extent, visible density, global unit count, route/overlay complexity, event rate, and supporting-list size with declared expected scales and structural counters. (Stories: US-002, US-003; Acceptance: AC-002)
- FR-005: At least one paired large-project/small-viewport fixture MUST hold visible density and viewport constant while increasing global project scale, and the summary MUST keep global and visible working-set metrics distinct. (Stories: US-003; Acceptance: AC-002)
- FR-006: Repeated pan, zoom, and playback MUST record memory after warm-up and after a declared stabilization cycle count, including heap/DOM observations, delta, and whether collection control was available. (Stories: US-001, US-002; Acceptance: AC-003)
- FR-007: The deterministic summary MUST rank pipeline stages from observed duration/share and structural counters, identify the next material bottleneck with evidence references, and disclose ties or unavailable data. (Stories: US-001; Acceptance: AC-004)
- FR-008: The summary MUST classify packed worker transport, typed buffers, and further allocation work as required or deferred from the measured worker-transfer/projection/allocation shares, and MUST state that fixtures are regression workloads rather than supported-size ceilings. (Stories: US-001, US-003; Acceptance: AC-004)
- FR-009: The harness MUST be independently runnable and focused, support selecting fixtures/journeys and an artifact directory, and MUST not be wired into the small-PR CI route. (Stories: US-002; Acceptance: AC-005)
- FR-010: Committed tests MUST validate fixture/artifact schemas, route coverage, binding/digest checks, capability-safe failure, and summary classification, and subject mutations MUST make each new gate fail. (Stories: US-002; Acceptance: AC-005)

## Ambiguities
- AMB-001: Which production surface and deterministic fixture-loader seam can vary all six axes without introducing a test-only rendering route.
- AMB-002: Which Chromium trace and in-page observation sources can separate every named stage, and how unavailable paint/compositor or collection controls are represented.
- AMB-003: What declared warm-up and stabilization sequence provides bounded, reproducible memory checkpoints.
- AMB-004: What evidence-backed threshold classifies transport, typed buffers, and allocation work as required rather than deferred without creating a permanent project-size ceiling.

## Public Or Tool-Facing Impact
- Introduces versioned scalable fixture and browser-performance artifact schemas plus a focused maintainer command.
- Extends `docs/performance-budget.md` with the workload identities, structural budgets, production-browser capability posture, and interpretation boundary.
- Does not change public gameplay, simulation, scene, or renderer semantics.

## Lifecycle Notes
- Required route: implementation-ready analyze before edits under the declared implementation paths, then evidence, verify, and ship.
- Pre-implementation performance smoke records current production-route capabilities and baseline structural/browser observations; it is not ship evidence.
- Next lifecycle action: `fsgg-sdd clarify --work 231-svg-pipeline-measurement`.
