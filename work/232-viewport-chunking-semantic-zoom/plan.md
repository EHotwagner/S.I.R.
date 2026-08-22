---
schemaVersion: 1
workId: 232-viewport-chunking-semantic-zoom
title: Viewport Chunking, Isolated SVG, Frame Coalescing
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/232-viewport-chunking-semantic-zoom/spec.md
sourceClarifications: work/232-viewport-chunking-semantic-zoom/clarifications.md
sourceChecklist: work/232-viewport-chunking-semantic-zoom/checklist.md
publicOrToolFacingImpact: true
---

# Viewport Chunking, Isolated SVG, Frame Coalescing Plan

Prose status: planned

## Source Snapshot
- spec: work/232-viewport-chunking-semantic-zoom/spec.md sha256:bfc67ffc12b32699e737f2abb3c557e39b284fb650412c8b0d9bbad8efe5e73b schemaVersion:1
- clarifications: work/232-viewport-chunking-semantic-zoom/clarifications.md sha256:592e34b35834beee3a5dfe48c0e9b4cde23675442ffa31f769c2c10285d08632 schemaVersion:1
- checklist: work/232-viewport-chunking-semantic-zoom/checklist.md sha256:6e1d9e94ef72adbddabb9ea932b798b67b4473f8fc967fb51d8414ba1b1550b9 schemaVersion:1

## Plan Scope
- Add a pure viewport/chunk filter to the shared tactical projection implementation without changing its published FSI surface, and apply it after each complete authoritative modality projection.
- Render only viewport-projected arrays and overlays in the retained SVG while publishing deterministic structural counters, semantic-tier metadata, and a non-spatial accessible selection summary derived from complete model state.
- Add focused F# boundary/scale/authority qualification, a production-browser large-project/small-viewport journey, tier styling, and producer-owned performance intent/evidence documentation.
- Isolate the tactical SVG in a stable React owner with deterministic scene/per-layer revision tokens, then add one visibility-aware latest-value frame scheduler in `BrowserInfrastructure.fs` so high-frequency camera/pointer presentation cannot force whole-application synchronous reconciliation per sample.
- Update only the retained SVG measurement runner, its measurement library, and its dedicated self-test so viewport sizing precedes Fit and structural qualification distinguishes emitted-visible equality from candidate/global growth; keep fixture/workload/budget definitions unchanged and leave #236 blocked.
- Evaluate the pinned Elmish.React batched root renderer without adding a custom transition state machine, and retain the synchronous renderer when fail-capable controlled-input or authoritative-presentation gates reject the prototype.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: `TacticalSceneProjection.fs` uses the canonical `Battlefield.CellSize` pitch and 8×8-cell integer chunk coordinates, derives the board-space query from finite camera and viewport facts, expands by two cells, and filters each complete modality projection immediately before returning presentation arrays; `App.fs` exposes the same constants plus queried/candidate/emitted/global counters on the SVG root.
- PD-002 [AC-002] [FR-002] [DEC-002] complete: Terrain cells, edge endpoints, unit footprints, route polylines, annotation geometry/anchors, effect endpoints, and overlay geometry contribute conservative bounds. Inclusive chunk membership feeds a stable `ScenePrimitiveId`-ordered deduplication pass so a multi-chunk primitive emits once and keeps its existing key/hit target.
- PD-003 [AC-003] [FR-003] [DEC-003] complete: `App.fs` derives overview/tactical/detail from `Battlefield.CellSize * camera.Zoom` at 20/48 CSS-pixel thresholds, writes `data-semantic-tier`, and renders bounded visible units with essential footprint/faction/class/selection/alert marks in all tiers. Detail glyph/channels/labels are CSS-promoted for tactical-tier selected, focus-visible, or hovered units and remain suppressed when overview geometry cannot carry them.
- PD-004 [AC-004] [FR-004] [DEC-004] complete: The model-owned roster/inspector and a new `aria-live` non-spatial scene-selection summary retain complete disclosed selected/focused facts. Offscreen geometry is absent rather than hidden/focusable, and selection ids survive viewport projection for deterministic re-entry.
- PD-005 [AC-005] [FR-005] [DEC-004] complete: Viewport filtering consumes a completed `SharedSceneProjection` only and returns a presentation projection without mutating editor/simulator/replay state; regression tests compare authoritative revision, tick, disclosure, selection, and complete evidence-export inputs before and after camera-only changes.
- PD-006 [AC-006] [FR-006] complete: `TacticalSceneProjectionQualification.fs` compares equal viewports over small and 160×160 projects, crosses exact chunk boundaries, checks every category and subject mutation, and proves authoritative invariants. `visible-workflows.spec.js` boots the built client, loads the large fixture through the visible import route, pans/zooms/selects through production controls, and verifies DOM bounds, unique ids, semantic tiers, hit targets, and accessible offscreen facts.
- PD-007 [AC-007] [FR-007] [DEC-005] complete: Refactor `App.fs` so a stable tactical-scene React component accepts the authoritative scene revision and deterministic per-layer tokens. Terrain, edges, units, routes, annotations, effects, overlays, counters, and accessibility metadata receive explicit invalidation dependencies; unrelated shell/model updates preserve unchanged retained layer owners.
- PD-008 [AC-008] [FR-008] [DEC-006] complete: Add a single latest-value requestAnimationFrame scheduler to `BrowserInfrastructure.fs`, including visibilitychange and disposal cancellation. Wire high-frequency camera/pointer presentation through the isolated tactical owner, while pointer completion still dispatches the final authoritative command synchronously.
- PD-009 [AC-009] [FR-009] [DEC-007] complete: In `measure-svg-pipeline.mjs`, resize and await presentation convergence before Fit, then require emitted units and actual viewport-intersecting glyphs to equal visible density. Extend the measurement projection with separately named candidate/global growth facts and add dedicated self-test mutations that reverse setup order or restore the obsolete global-equality assertion and observe red.
- PD-010 [AC-007] [AC-008] [FR-007] [FR-008] [DEC-008] complete: The one-line pinned `Program.withReactBatched` prototype compiled and passed corrected focused controlled-input, View/keyboard, and Plan acceptance gates, but the unchanged seven-journey trace regressed playback to 37 ms/58.049 ms script and retained two dropped modality frames with 80.050 ms script. Restore `Program.withReactSynchronous` and the exact synchronous harness assumptions; preserve only the independently proven right-sidebar `EditorView`/`ImportAnnouncement` ownership correction.

## Contract Impact
- PC-001 [PD-001] [PD-003] [PD-007] [PD-008] additiveDom: Extend `#persistent-tactical-svg` with `data-viewport-*`, structural-cost, semantic-tier, accepted-revision/layer-token, and frame-acceptance metadata while preserving `shared-scene-projection-v1`, semantic ids, authoritative commands, and existing FSI surface.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-005] semanticTest: Focused F# qualification proves deterministic inclusive chunks, category bounds, stable-id deduplication, equal visible cost across global scales, and unchanged authoritative identities; invert chunk intersection/deduplication and observe red.
- VO-002 [PD-003] [PD-004] accessibilityTest: Production browser coverage proves tier transitions, interaction promotion, one hit target, no offscreen focusable SVG node, and retained accessible selected/focused facts; remove promotion/summary and observe red.
- VO-003 [PD-001] [PD-006] performanceEvidence: Run the exact Release candidate against `viewport-visible-v1`, retain structural and Chromium trace facts, and require queried chunks ≤24, emitted primitives ≤1600, zero semantic duplicates, zero offscreen focusable nodes, plus declared p95/p99 capability results.
- VO-004 [PD-007] semanticTest: Focused browser instrumentation proves unrelated model updates and unchanged layer tokens do not rebuild the tactical SVG owner, while a changed layer token invalidates the expected layer; remove token equality/isolation and observe red.
- VO-005 [PD-008] performanceEvidence: A deterministic scheduler qualification and exact-candidate Chromium trace prove latest-value acceptance, at most one callback per visible animation frame, hidden/disposal cancellation, clean resume, and the unchanged `viewport-visible-v1` p95/p99 budget; bypass coalescing or retain multiple handles and observe red.
- VO-006 [PD-009] performanceEvidence: Dedicated measurement self-tests reject Fit-before-resize and projected-equals-global mutations, then the exact Release candidate executes the unchanged focused production trace with emitted-visible equality, candidate/global growth facts, and the original p95/p99/frame verdicts.
- VO-007 [PD-010] performanceEvidence: Focused production-browser comparison proves invalid/valid file input, View/keyboard, and Plan worker acceptance under both roots after the independent right-sidebar correction. The batched candidate's retained seven-journey artifact records playback 37 ms/drop1/script58.049 and modality 30 ms/drop2/script80.050 against unchanged budgets; after surgical restoration, the same focused gates and synchronous baseline must pass with no batched integration or batched-only runner assumptions remaining in source.

## Performance Intent
- id: viewport-visible-work-v1
- disposition: active
- targetFps: 60
- workloadIds: [viewport-visible-v1]
- workloadDefinitionDigests: [viewport-visible-v1=sha256:7335f5a84cded141d3a04a0d395b6d5ec8f78d34178d2196c02d4935384dff24]
- maximumExpectedScale: 160x160 map; 2,000 units/routes/annotations; 256 active effects; fixed 480x320 viewport
- maxP95Ms: 16
- maxP99Ms: 32
- maxCatchUpFrames: 0
- structuralCostBudgets: [queried-chunks<=24, emitted-spatial-primitives<=1600, semantic-duplicates=0, offscreen-focusable-svg=0]
- requiredCapability: production-chromium-trace
- liveCompositorRequired: true

## Migration Posture
- PM-001 [PC-001] additive: Existing DOM selectors and semantic ids remain valid; consumers may adopt the new viewport/tier counters incrementally and unknown metadata is ignored.

## Generated View Impact
- GV-001 [PD-006] evidenceArtifacts: Lifecycle analysis/verification/ship views and focused test/browser receipts regenerate from exact sources and observed runs; stale workload digests or candidate bindings fail closed.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Production-Chromium compositor evidence is mandatory for release; bounded F# structural evidence remains separately labelled and cannot satisfy that capability.
- The 160×160/2,000-object workload is a regression identity, not a project-size ceiling.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 232-viewport-chunking-semantic-zoom`.
