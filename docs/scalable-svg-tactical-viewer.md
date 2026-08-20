---
title: Scalable SVG Tactical Viewer Architecture
category: Architecture
categoryindex: 2
index: 23
status: proposed
decision-status: accepted-direction
document-type: living-architecture
version: "1.0"
last-updated: 2026-08-20
description: Scalable retained-SVG rendering, scheduling, culling, batching, allocation, and performance-evidence design for the tactical workspace.
related:
  - docs/svg-replay-player.md
  - docs/unified-tactical-workspace.md
  - docs/performance-budget.md
  - docs/simulator-worker-protocol.md
---

# Scalable SVG Tactical Viewer Architecture

S.I.R. will retain SVG as the tactical battlefield renderer while the project
grows. The scaling strategy is to do less work, less often: keep simulation and
replay verification off the UI thread, publish immutable disclosed scene facts,
invalidate only changed scene layers, render only the visible spatial working
set with overscan, reduce detail through semantic zoom, and let stable SVG keys
preserve browser nodes. This direction does not introduce Canvas, WebGL, or
WebGPU, and it does not use mutable pools for authoritative or presentation
domain objects.

## Goals

- Preserve the one persistent, inspectable, accessible SVG workscreen across
  Editor, Plan, Simulate, and Review.
- Keep interaction responsive as map dimensions, unit count, overlays, and
  supporting project data grow.
- Decouple the deterministic 20 Hz simulation clock from browser presentation.
- Make projection, reconciliation, layout, paint, and worker-transfer costs
  independently observable in a real Chromium trace.
- Prefer scale-relative policies over fixed project-size ceilings. A fixed
  bound is valid only for a deliberately bounded function, such as the visible
  causal effect window or one import payload.
- Preserve disclosure, replay identity, accessibility, input routing, and
  evidence-export semantics while optimizing presentation.

## Non-goals

- Moving authoritative simulation, pathfinding, collision, or replay
  verification onto the GPU.
- Replacing the shared scene contract with renderer-specific state.
- Reusing mutable F# domain records to avoid allocation.
- Keeping every map primitive in the live SVG DOM regardless of visibility.
- Treating one benchmark fixture as the largest project S.I.R. supports.

## Architectural invariants

1. Authoritative state remains deterministic CPU state. Presentation never
   feeds facts back into simulation.
2. The browser worker owns replay verification and bounded simulator work. The
   UI receives correlated, disclosed projections rather than authoritative
   internals.
3. `TacticalSceneProjection` remains the common semantic boundary for every
   tactical modality.
4. `svg#persistent-tactical-svg`, its camera, and its stable layer groups retain
   DOM identity across modality and layout changes.
5. Culling may remove offscreen geometry from the SVG, but never from the
   authoritative model, roster, selection model, search, or accessibility
   alternatives. A selected or keyboard-focused primitive remains representable
   even when it crosses the viewport boundary.
6. Semantic zoom removes detail, not facts. Full information remains available
   through selection, inspection, and accessible text.
7. Evidence export is deterministic and scene-based. It does not serialize the
   live, culled DOM.

## Current data flow

```text
authoritative simulation / replay package
                  |
          retained browser worker
                  |
     correlated disclosed projection
                  |
       TacticalSceneProjection
                  |
        Elmish model and update
                  |
       React/Feliz reconciliation
                  |
      one retained layered SVG
                  |
       browser layout and paint
```

The worker boundary already prevents replay and longer simulator operations
from blocking pointer input. Projection uses stable semantic primitive IDs and
arrays, while the SVG uses those IDs as React keys. The current presentation
subscription dispatches at 50 ms intervals. That preserves the simulation
cadence but also permits unchanged state to rebuild virtual SVG children, and
the existing performance evidence does not isolate browser layout, paint, or
compositor work.

## Target data flow

```text
worker snapshot ──> accepted scene revision ──> layer revision index
                                                |       |
camera input ──> viewport/chunk query ───────────┘       |
                                                        v
20 Hz simulation clock                         visible layer snapshots
                                                        |
requestAnimationFrame <── dirty presentation flag <─────┘
          |
          v
stable keyed SVG layers + semantic HTML accessibility/inspection
```

### Accepted scene revision

Every accepted scene has one revision identity and per-layer revision tokens.
The adapter computes a new layer token only when the facts consumed by that
layer change. Camera motion changes the visible chunk set without manufacturing
a new simulation revision.

```fsharp
type SceneLayerRevision =
    { Layer: string
      FactsRevision: string
      VisibleChunkRevision: string
      DetailTier: string }
```

This sketch is a design shape, not yet a public serialization contract. Cache
keys must be derived from accepted identity and typed presentation inputs; they
must not depend on object reference identity, time, or DOM state.

### Presentation clock

Simulation remains at 20 Hz. A single `requestAnimationFrame` owner renders at
most once per browser frame and only while presentation is dirty or an accepted
transition is active. Multiple worker, pointer, layout, and simulation messages
arriving before the next frame coalesce into one presentation pass.

Interpolation may change presentation coordinates between two compatible
accepted snapshots. It may not interpolate identity, disclosure, health,
status, stance, event truth, revision, or committed tick. Hidden tabs stop
requesting frames and converge directly to the latest accepted scene when they
become visible.

### Spatial working set

The viewer partitions spatial primitives into deterministic map chunks. The
live SVG contains chunks intersecting the camera viewport plus an overscan
margin large enough to avoid edge flicker during ordinary pan and zoom.

- Static terrain and edges use chunk membership computed from authored bounds.
- Units, effects, routes, annotations, and overlays use their current spatial
  bounds and layer revision.
- Large primitives intersect every chunk they touch; ownership is still a
  single semantic primitive ID.
- Offscreen selected or focused primitives retain a minimal focus/selection
  representative and remain available through the roster and inspector.
- Culling is presentation-only and therefore cannot affect simulation,
  validation, route search, disclosure, or evidence export.

Chunk dimensions and overscan are tuning parameters backed by traces, not
project-size limits.

### Semantic zoom

Detail tier derives from projected on-screen size and interaction state rather
than total global unit count alone.

| Tier | Battlefield presentation |
|---|---|
| Overview | footprint, faction/class silhouette, selection, essential alerts |
| Tactical | facing and attention pips, compact health, essential status marker |
| Detail | segmented health, stance, elevation, status labels, full interaction channels |

Selected, hovered, or keyboard-focused units may promote one tier when space
allows. The inspector always exposes the complete disclosed facts independently
of battlefield tier.

### SVG batching

Batching reduces DOM nodes without sacrificing semantic interaction:

- Adjacent static terrain of the same presentation class may share deterministic
  SVG path data inside one chunk.
- Pointer hit testing continues through the existing coordinate-to-cell path;
  it does not require one interactive DOM node per cell.
- Non-interactive grid, hatch, and background geometry may be shared through
  SVG definitions or chunk paths.
- Units remain keyed semantic groups because they own focus, accessible names,
  selection, and individual information channels.
- Effects and overlays may batch only when their individual semantic identity
  is retained in the projection and inspection surface.

Array work should be single-pass where practical. Repeated `Array.append`,
`Array.copy`, full sorting, and intermediate tuple allocation are candidates
only after profiles identify them as material. Typed arrays or packed worker
transport are an evidence-gated optimization, not a prerequisite for SVG.

### Reuse and pooling policy

Stable keys are the primary SVG element-reuse mechanism: React and the browser
retain the actual DOM nodes. S.I.R. does not pool mutable `MapEditorState`,
`SharedSceneProjection`, unit, event, or disclosure records.

Bounded renderer-local reuse is permitted for:

- scratch arrays used during one projection pass;
- chunk lookup tables keyed by accepted revision;
- ring buffers holding the two compatible snapshots used for interpolation;
- bounded effect-window slots; and
- cached deterministic SVG path strings for unchanged chunks.

Every reusable resource has an explicit owner, reset rule, and invalidation
key. No pool may let facts, disclosure, focus, or event identity survive their
accepted revision.

## Performance evidence

The first deliverable is measurement, not optimization. A production Chromium
journey records, for each representative scale and interaction:

- worker compute and worker-to-main transfer;
- scene projection and allocation;
- Elmish update and React reconciliation;
- SVG style recalculation and layout;
- paint/compositor duration;
- DOM and visible primitive counts by layer;
- long tasks, dropped frames, and input latency; and
- memory after warm-up and after repeated pan/zoom/playback cycles.

Fixtures grow along independent axes: map area, visible density, global unit
count, route/overlay complexity, event rate, and supporting-list size. Gates
attach to versioned fixture identities and user journeys. They are regression
budgets, not statements that larger projects are unsupported.

## Delivery sequence

1. Establish real-browser tracing and scalable fixture definitions.
2. Introduce the invalidation-driven `requestAnimationFrame` presentation
   clock without changing SVG output or simulation semantics.
3. Add deterministic spatial chunking, viewport culling, and semantic zoom.
4. Add per-layer revision caches and isolate SVG layer components so unchanged
   layers do not rebuild.
5. Batch static SVG geometry and remove evidenced allocation hot spots.
6. Re-run the full browser evidence matrix and set evidence-backed regression
   budgets for the implemented architecture.

Packed transport and more specialized buffers are follow-up work only when the
trace shows structured cloning or allocation is a material share of latency.

Delivery is tracked by the
[retained SVG scaling workstream](https://github.com/EHotwagner/S.I.R./issues/230)
and its dependency-ordered sub-issues:

- [#231 — Chromium pipeline measurement](https://github.com/EHotwagner/S.I.R./issues/231)
- [#234 — frame-scheduled presentation](https://github.com/EHotwagner/S.I.R./issues/234)
- [#232 — viewport chunking and semantic zoom](https://github.com/EHotwagner/S.I.R./issues/232)
- [#233 — revision-keyed layer caching](https://github.com/EHotwagner/S.I.R./issues/233)
- [#235 — SVG batching and measured allocation work](https://github.com/EHotwagner/S.I.R./issues/235)
- [#236 — integrated browser qualification](https://github.com/EHotwagner/S.I.R./issues/236)

## Acceptance

The workstream is complete when:

- one retained SVG and stable layer identities survive all four modalities;
- simulation still advances deterministically at 20 Hz while rendering is
  frame-scheduled and coalesced;
- unchanged accepted scenes cause no battlefield reconstruction;
- pan and zoom render only the visible chunk working set plus overscan;
- semantic zoom and culling preserve selection, focus, disclosure, inspection,
  and deterministic export;
- the browser evidence separates CPU projection, reconciliation, layout, paint,
  and transfer costs; and
- increasing project extent without increasing the visible working set does
  not proportionally increase per-frame DOM or render work.

## Alternatives considered

### Canvas, WebGL, or WebGPU now

Rejected for this phase. They could reduce draw-call and DOM costs, but would
replace mature SVG accessibility, inspectability, evidence, and event behavior
before measurements demonstrate that SVG cannot meet the intended workload.

### Mutable object and shape pools throughout the client

Rejected. They complicate F#/Elmish ownership and can retain stale disclosure
or identity. Stable keyed DOM reuse and narrowly owned bounded buffers provide
the useful reuse without weakening the semantic model.

### Keep the complete map in the SVG and rely on browser GPU acceleration

Rejected. SVG rasterization may use the GPU, but DOM construction, React
reconciliation, style, layout, and path processing remain proportional to live
geometry. Culling and invalidation remove that work before it reaches the
rasterizer.
