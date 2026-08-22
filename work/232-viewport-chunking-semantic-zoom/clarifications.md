---
schemaVersion: 1
workId: 232-viewport-chunking-semantic-zoom
title: Viewport Chunking, Isolated SVG, Frame Coalescing
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/232-viewport-chunking-semantic-zoom/spec.md
publicOrToolFacingImpact: true
---

# Viewport Chunking, Isolated SVG, Frame Coalescing Clarifications

## Source Specification
- work/232-viewport-chunking-semantic-zoom/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: What chunk and overscan constants define the production query?
- CQ-002 [AMB:AMB-002]: How are multi-cell and non-cell-aligned primitive bounds chunked and deduplicated?
- CQ-003 [AMB:AMB-003]: What projected-size thresholds and interaction rule define the three semantic tiers?
- CQ-004 [AMB:AMB-004]: Where do complete disclosed facts remain available after spatial culling?
- CQ-005 [AMB:AMB-005]: What deterministic revision boundary isolates unchanged tactical SVG layers from unrelated application updates?
- CQ-006 [AMB:AMB-006]: Who owns frame coalescing, and how does it behave across visibility and disposal transitions?
- CQ-007 [AMB:AMB-007]: What exact measurement setup and structural assertions change for culling, and what remains immutable?
- CQ-008: Can the pinned Elmish.React root integration coalesce superseded render effects without delaying authoritative Elmish updates or violating controlled-input semantics?

## Answers
- CQ-001 → Use deterministic 8×8-cell chunks and a two-cell overscan around the camera-derived viewport, expressed through the canonical `Battlefield.CellSize` (currently 24 board units, so 48 board units of overscan at standard zoom). The #231 fixed 480×320 viewport and repeated pan/zoom trace is the comparison baseline; the margin covers two complete retained cells and is declared as a structural input, not inferred from elapsed time.
- CQ-002 → Compute conservative axis-aligned board-space bounds for every primitive category, enumerate all inclusive chunk coordinates touched by those bounds, union candidates from queried chunks, then sort and deduplicate by existing `ScenePrimitiveId`. Exact boundary coordinates belong to both neighboring chunks during indexing, but emit once after identity deduplication.
- CQ-003 → Derive projected cell size as `Battlefield.CellSize * finite camera zoom` (currently `24 * zoom`): overview below 20 CSS pixels, tactical from 20 through 47.999, detail at 48 or above. Selection, SVG focus, or hover promotes a visible tactical-tier unit to detail; overview retains footprint/faction/class/selection/alert marks but refuses labels/channels that cannot fit.
- CQ-004 → Keep the complete authoritative `SharedSceneProjection` long enough to form selection/roster/inspector and one non-interactive accessible scene summary, then render a separate viewport projection. Existing model-owned inspector/roster state remains authoritative; the culled SVG contains only visible interactive geometry and therefore no duplicate focus target.
- CQ-005 → A stable tactical React owner accepts an explicit scene revision plus deterministic terrain/edge/unit/route/annotation/effect/overlay/accessibility tokens derived only from authoritative revision, viewport chunk query, semantic tier, and interaction promotion inputs. A token change invalidates its named layer; object reference, wall clock, and hash iteration order do not participate.
- CQ-006 → `BrowserInfrastructure.fs` owns exactly one cancellable requestAnimationFrame handle for presentation invalidation. Enqueue replaces the pending camera/pointer presentation with the latest value; the visible callback accepts once and clears ownership before invoking. `document.visibilitychange`, explicit cancellation, and disposal clear the handle and pending value; a later visible enqueue schedules a fresh handle. Authoritative pointer completion remains an ordinary synchronous command.
- CQ-007 → Set the fixture viewport and await its resize delivery before invoking the existing Fit command. Compare the SVG's emitted unit count and viewport-intersecting production glyph count with `visibleDensity`; retain candidate and global counts as separately observable growth facts rather than expecting projected units to equal the global count. Fixture JSON, workload recipe, event rate, journey action, trace clock markers, p95/p99 thresholds, and frame-health rules do not change. #236 remains blocked for the complete matrix.
- CQ-008 → Prototype only the pinned `Program.withReactBatched` integration at the final application root. Elmish update, simulation, worker acceptance, and effects remain synchronous; the package-owned renderer cancels a superseded animation-frame request and presents only the latest model. The focused controlled-input, View/keyboard, and Plan acceptance gates passed after repairing an independent right-sidebar ownership omission and waiting for the RAF-presented state. The unchanged seven-journey gate nevertheless regressed playback from 33 to 37 ms and main-thread script from 52.912 to 58.049 ms, retained two modality dropped frames, and left modality script at 80.050 ms; therefore the synchronous integration is retained and the prototype plus batched-only harness accommodations are reverted.

## Decisions
- **DEC-001** [CQ-001] [AMB:AMB-001] [FR-001] [AC-001]: Index in 8×8-cell chunks and query the finite camera viewport with a deterministic two-cell overscan; publish both constants and structural query counters in the DOM/performance intent.
- **DEC-002** [CQ-002] [AMB:AMB-002] [FR-002] [AC-002]: Use conservative category bounds, inclusive multi-chunk membership, stable-id deduplication, and stable lexical/id ordering; chunk identity never becomes semantic identity.
- **DEC-003** [CQ-003] [AMB:AMB-003] [FR-003] [AC-003]: Use projected-cell thresholds 20/48 CSS pixels; interaction promotion is allowed only from tactical to detail, while overview remains compact and still encodes essential meaning.
- **DEC-004** [CQ-004] [AMB:AMB-004] [FR-004] [FR-005] [AC-004] [AC-005]: Preserve a complete projection for non-spatial/accessibility consumers and derive a presentation-only viewport projection for SVG construction; no culling result is stored in or returned to authoritative simulation paths.
- **DEC-005** [CQ-005] [AMB:AMB-005] [FR-007] [AC-007]: Isolate tactical SVG ownership behind explicit accepted-scene and per-layer revision tokens computed from deterministic semantic inputs; unchanged layers retain ownership across unrelated shell updates and no renderer cache becomes authoritative state.
- **DEC-006** [CQ-006] [AMB:AMB-006] [FR-008] [AC-008]: Put one latest-value requestAnimationFrame scheduler at the browser presentation boundary, cancel it on hidden/dispose, resume by fresh visible enqueue, and keep final authoritative interaction commands outside the coalesced presentation channel.
- **DEC-007** [CQ-007] [AMB:AMB-007] [FR-009] [AC-009]: Make the retained harness culling-aware only by resize-before-Fit and emitted-visible equality, publish candidate/global growth independently, and freeze all fixture, cadence, tracing, and budget identities; keep the full #236 matrix outside this composite.
- **DEC-008** [CQ-008] [FR-007] [FR-008] [AC-007] [AC-008]: Reject the pinned Elmish.React batched root integration after the bounded exact production trace regressed playback and failed the unchanged dropped-frame/p95 contract despite passing focused semantics. Retain `Program.withReactSynchronous`; package-level render-request cancellation does not make the genuine accepted React commits fit the frame budget.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
- None.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 232-viewport-chunking-semantic-zoom`.
