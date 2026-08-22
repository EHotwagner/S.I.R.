---
schemaVersion: 1
workId: 232-viewport-chunking-semantic-zoom
title: Viewport Chunking, Isolated SVG, Frame Coalescing
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
performanceIntent:
  id: viewport-visible-work-v1
  disposition: active
  targetFps: 60
  workloadIds:
    - viewport-visible-v1
  workloadDefinitionDigests:
    - viewport-visible-v1=sha256:7335f5a84cded141d3a04a0d395b6d5ec8f78d34178d2196c02d4935384dff24
  maximumExpectedScale: "160x160 map; 2,000 units/routes/annotations; 256 active effects; fixed 480x320 viewport"
  maxP95Ms: 16
  maxP99Ms: 32
  maxCatchUpFrames: 0
  structuralCostBudgets:
    - "queried-chunks<=24"
    - "emitted-spatial-primitives<=1600"
    - "semantic-duplicates=0"
    - "offscreen-focusable-svg=0"
  requiredCapability: production-chromium-trace
  liveCompositorRequired: true
  evidenceRefs:
    - work/231-svg-pipeline-measurement/production-chromium-evidence.json
    - work/231-svg-pipeline-measurement/production-chromium-summary.json
---

# Viewport Chunking, Isolated SVG, Frame Coalescing Specification

Prose status: specified; Tier 1 retained-SVG projection and accessibility contract.

## User Value
Large tactical projects remain fluid and readable because live SVG work follows the visible viewport while complete disclosed facts stay available through non-spatial alternatives.

## Scope
- SB-001: Deterministically index and query terrain, edges, units, routes, annotations, effects, and tactical overlays against the current viewport plus a declared overscan margin.
- SB-002: Choose overview, tactical, or detail presentation from projected screen size and interaction promotion while preserving one semantic identity and hit target per emitted primitive.
- SB-003: Preserve complete disclosed facts through existing inspector/roster surfaces and a scene accessibility alternative even when spatial geometry is culled.
- SB-004: Qualify bounded visible work with focused pure tests and a boot-to-visible production Chromium journey using a large-project/small-viewport fixture.
- SB-008: Give the tactical SVG a stable component owner whose accepted scene revision and explicit per-layer tokens determine reconciliation, so unrelated shell/model messages do not rebuild unchanged retained layers.
- SB-009: Give high-frequency camera/pointer presentation one requestAnimationFrame owner that accepts the latest pending state once per visible frame and cancels safely while the page is hidden or disposed.
- SB-010: Repair the retained exact-candidate measurement setup directly invalidated by viewport culling by resizing before Fit and separating emitted-visible equality from candidate/global growth, without changing any fixture identity, workload rate, trace window, or p95/p99/frame threshold.

## Non-Goals
- SB-005: Do not partition or cull simulation, validation, pathfinding, disclosure, replay/evidence export, or any other authoritative computation.
- SB-006: Do not migrate away from retained SVG, change worker transport, introduce a renderer cache, or declare a maximum supported project size.
- SB-007: Do not make offscreen geometry keyboard-focusable inside the SVG; represent it through the roster, inspector, selection state, and accessible scene text until it re-enters the viewport.
- SB-011: Do not absorb #236 or claim completion of the full integrated qualification matrix; this item owns only the culling-compatibility repair and focused exact-candidate trace.

## User Stories
- US-001 (P1): As a player or author, I can pan and zoom a large battlefield without boundary flicker, duplicate geometry, or missed visible hit targets.
- US-002 (P1): As a keyboard, screen-reader, or inspector user, I can retain selected/focused context and read complete disclosed facts even when its geometry is offscreen or simplified.
- US-003 (P1): As a maintainer, I can prove live DOM and presentation work are bounded by the visible working set rather than total project extent.

## Acceptance Scenarios
- AC-001 [US-001] [US-003] [FR-001]: Given equal small viewports over projects with materially different global extents, when the same camera region is rendered, then queried chunks and emitted spatial primitives remain within the same viewport-plus-overscan structural bound.
- AC-002 [US-001] [FR-002]: Given a primitive whose bounds intersect a chunk or viewport boundary, when the camera pans across that boundary, then the primitive keeps one stable semantic id, appears at most once per frame, and remains hittable whenever its geometry intersects the overscan query.
- AC-003 [US-001] [US-002] [FR-003]: Given overview, tactical, and detail projected-size thresholds, when zoom or interaction state changes, then essential faction/class, footprint, selection, and alert meaning remains while selected, hovered, or keyboard-focused units promote predictably when space permits.
- AC-004 [US-002] [FR-004]: Given a selected or focused disclosed entity moves outside the rendered spatial set, when the user reads selection, roster, inspector, or scene accessibility text, then its complete disclosed identity and facts remain available without duplicate focusable SVG geometry.
- AC-005 [US-003] [FR-005]: Given presentation culling is active, when simulation, validation, route search, disclosure projection, or deterministic evidence export executes, then it observes the complete authoritative state and produces the same result as an uncropped presentation.
- AC-006 [US-001] [US-003] [FR-006]: Given the large-project/small-viewport workload, when focused qualification and the built production Chromium journey run, then deterministic counters prove bounded visible work, boundary continuity, semantic tiers, and accessible alternatives without declaring a project-size ceiling.
- AC-007 [US-001] [US-003] [FR-007]: Given unrelated application-model updates or an unchanged tactical layer, when React reconciliation runs, then the stable tactical owner accepts only a changed scene revision and unchanged layer tokens retain their existing SVG subtree identity without rebuilding the whole scene.
- AC-008 [US-001] [US-003] [FR-008]: Given a burst of camera/pointer samples, when the page is visible, then at most one presentation acceptance runs per animation frame using the latest sample; when hidden, disposed, or restarted, pending work is cancelled and the next visible sample resumes without duplicate frame ownership.
- AC-009 [US-003] [FR-009]: Given any retained SVG measurement fixture, when the harness establishes its fixed viewport and camera, then resize completes before Fit, emitted production units equal the declared visible density, candidates and global primitives are reported separately and may grow, and every pre-existing fixture identity, event cadence, trace window, and p95/p99/frame budget remains byte-for-byte unchanged.

## Functional Requirements
- FR-001: The presentation projection MUST use the canonical `Battlefield.CellSize` coordinate pitch, deterministic spatial chunk coordinates, and query only chunks intersecting the finite viewport plus a trace-backed overscan margin for terrain, edges, units, routes, annotations, effects, and overlays; live cost counters MUST report queried chunks, candidates, emitted primitives, and global primitives separately. (Stories: US-001, US-003; Acceptance: AC-001)
- FR-002: Each semantic primitive MUST be assigned to every chunk its bounds intersect while query emission deduplicates by its existing stable `ScenePrimitiveId`, orders results deterministically, and uses inclusive overscan intersection so boundary pans produce neither flicker nor duplicate hit targets. (Stories: US-001; Acceptance: AC-002)
- FR-003: The renderer MUST choose overview, tactical, and detail tiers from finite projected cell size with deterministic thresholds, retain faction/class, footprint, selection, and alert meaning in every tier, and promote selected, hovered, or keyboard-focused units to detail only when the projected geometry can carry it. (Stories: US-001, US-002; Acceptance: AC-003)
- FR-004: Culling MUST preserve selected/focused ids and complete disclosed facts in non-spatial selection, roster, inspector, and accessible scene text; culled offscreen geometry MUST not create a second focusable SVG target. (Stories: US-002; Acceptance: AC-004)
- FR-005: The chunk query MUST remain a presentation-only transformation over an already-authoritative `SharedSceneProjection`; it MUST NOT feed simulation, validation, route search, disclosure, replay, or deterministic evidence export. (Stories: US-003; Acceptance: AC-005)
- FR-006: Committed focused tests and a built-product Chromium journey MUST compare fixed-small-viewport workloads across project scales, exercise pans and zooms across chunk boundaries, assert the tier/accessibility contract, and include subject mutations that make every added gate fail. (Stories: US-001, US-003; Acceptance: AC-006)
- FR-007: The tactical SVG MUST be owned by a stable isolated React component with an explicit accepted scene revision and deterministic per-layer revision tokens; unchanged tokens MUST preserve retained layer ownership across unrelated model updates, while changed tokens MUST invalidate only their dependent layer and accessibility/counter metadata. (Stories: US-001, US-003; Acceptance: AC-007)
- FR-008: High-frequency camera/pointer presentation MUST use one BrowserInfrastructure-owned requestAnimationFrame scheduler that retains only the latest pending presentation, invokes at most once per visible frame, cancels pending work on hidden/dispose, resumes deterministically when visible, and leaves authoritative command completion synchronous and unchanged. (Stories: US-001, US-003; Acceptance: AC-008)
- FR-009: The retained exact-candidate measurement harness MUST establish the requested browser viewport before invoking Fit, MUST compare emitted production units with the declared visible density, MUST report candidate and global growth independently, and MUST preserve exact fixture definitions, workload recipes, event cadence, trace boundaries, p95/p99 limits, and frame-health gates; dedicated mutations MUST fail when ordering or emitted-visible equality is regressed. (Stories: US-003; Acceptance: AC-009)

## Ambiguities
- AMB-001: Which chunk size and overscan margin are justified by the existing #231 production trace and remain stable across zoom levels.
- AMB-002: Which geometric bounds each scene category contributes, especially polylines, edge-aligned geometry, effects, and overlays crossing multiple chunks.
- AMB-003: Which projected-size thresholds define overview/tactical/detail and how interaction promotion behaves when the geometry is too small to carry detailed channels.
- AMB-004: Which existing production surface supplies complete accessible disclosed facts when selected/focused geometry is culled, without duplicating interactive SVG nodes.
- AMB-005: Which scene revision and per-layer tokens provide a deterministic invalidation boundary without introducing object-identity or wall-clock cache semantics.
- AMB-006: Which component owns the single animation-frame handle, and what hidden-page/disposal contract avoids stale or duplicate presentation acceptance.
- AMB-007: Which retained harness assumptions are presentation-invalid after culling, and which fixture/workload/budget facts must remain immutable during compatibility repair.

## Public Or Tool-Facing Impact
- Extends the shared scene presentation contract with deterministic viewport/chunk metadata, semantic tier metadata, and observable structural counters.
- Adds explicit tactical-scene/layer revision metadata and visibility-aware frame-coalescing behavior at the browser presentation boundary.
- Changes the live retained-SVG DOM and accessibility description while preserving existing semantic primitive ids and authoritative scene inputs.
- Updates `docs/performance-budget.md` with the workload identity, structural budgets, overscan/tier definitions, and bounded-headless versus production-browser capability posture.

## Lifecycle Notes
- Required route: implementation-ready analysis before edits under declared implementation paths, then observed evidence, verify, ship, independent exact-head review, and hosted CI acceptance.
- Pre-implementation smoke records current full-project DOM work for the fixed viewport; it is intentionally expected to show the scaling debt this item addresses.
- Next lifecycle action: `fsgg-sdd clarify --work 232-viewport-chunking-semantic-zoom`.
