---
title: Performance Budget
category: Tools & Evidence
categoryindex: 5
index: 5
status: proposed
document-type: living-design
version: "0.7"
last-updated: 2026-07-29
related:
  - docs/simulation-core-architecture.md
  - docs/skirmish-development-plan.md
  - docs/wasm-control-architecture.md
  - docs/combat-resolution.md
  - docs/tactical-environment-architecture.md
---

# Performance Budget

## Purpose

The design set specifies seventeen architectures and no cost model. Every rule
so far has been chosen on tactical merit, which is correct, but it means the
project has accumulated obligations against a 50-millisecond tick without ever
accounting for them.

This document names the cost centres, records a provisional allocation, states
what must be measured before which gate, and — most importantly — records the
**fallback position** for each centre, so that discovering a budget overrun does
not become an open-ended redesign.

Most numbers here are starting hypotheses to be replaced by evidence. Their
purpose is to force the argument about which subsystem deserves which share, not
to predict an outcome.

**Three centres are now measured, and all three were badly overestimated.**

WASM invocation and observation marshalling came in at **~2%** against a
provisional 15% — see
[WASM Invocation Spike](research/wasm-invocation-spike.md).

Perception came in at **~1%** against a provisional 30% — see
[Perception Spike](research/perception-spike.md). It was the largest allocation
in this document and the one most feared, on the reasoning that it is quadratic
in unit count and had just gained a third dimension. It is quadratic, and it is
still about one percent of the tick at the supported force target.

Movement came in at effectively **zero** for conflict resolution against a
provisional 30% — see [Movement Spike](research/movement-spike.md). The
allocation was aimed at the wrong thing: reservation and dependency resolution
are free, and **path search is the entire cost of movement**, expressed as a
replan cadence rather than a per-tick share.

Together these three held **75%** of the tick between them at the moment each
was measured — perception 30%, movement 30%, WASM 15% — and they account for
roughly **3% of it**. Every measurement so far has removed an assumption this
document was built on, which is exactly what the gates exist to do.

## The tick budget

The authoritative simulation runs at 20 Hz, so a tick has **50 ms** of wall
clock. That is a hard ceiling, not a target.

A live service cannot run at full utilisation. Garbage collection, operating
system scheduling, network handling, and match-to-match variance all need room,
and a tick that occasionally exceeds its budget is a tick that desynchronises
real-time play. The working target should therefore be a fraction of the
ceiling, with the remainder reserved as headroom.

```text
50 ms   hard ceiling — exceeding this fails real-time play
~20 ms  working target for the authoritative tick at 100v100
~30 ms  headroom for GC, scheduling, variance, and other matches
```

Match density follows directly. A match consuming half a core sustains roughly
two matches per core; one consuming a full core makes the canonical service
substantially more expensive to operate. Density is an operating cost decision,
not only an engineering one.

## Cost centres

| Centre | Scales with | Continuous? |
|---|---|---|
| Movement and reservation | units currently moving | per tick |
| Perception and acquisition | observer × contact episodes | per tick |
| Line of sight and visibility | new candidates, invalidations | on change |
| WASM invocation | units, every tick | **measured** |
| Observation construction | units × what each is entitled to see | **measured** |
| Engagement maintenance | active engagements | per tick |
| Combat resolution | traces on resolution ticks | bursty |
| Objective and logistics state | events | on change |
| Knowledge projection | participants × disclosed deltas | per tick or batched |
| Serialization and network | projection volume | per tick or batched |
| Journal, hashing, and replay | authoritative state size | see below |

### Provisional allocation

Of the ~20 ms working target, a starting hypothesis:

```text
perception and acquisition       5%  (measured: ~1%)
movement: conflict resolution    1%  (measured: <0.05%)
movement: path search           10%  (a replan budget, see below)
WASM invocation and marshalling  5%  (measured: ~2%)
combat resolution               10%
projection and serialization     10%
journal and hashing              5%
```

Path search now leads, and unlike the others it is not a fixed cost. It is a
**replan budget**: a number of searches the tick can afford. At roughly 0.35 ms
for a typical medium-range search, a 10% share buys about 14 searches per tick.

The remaining unmeasured centres — combat resolution, projection and
serialization, journal and hashing — hold the rest. Every measured centre came
in at least seven times under its allocation, perception by thirty times, and
movement's conflict resolution by far more than either because its allocation
was aimed at the wrong half of the problem. These shares should be treated as
placeholders rather than predictions.

## Revised cost estimates

Recorded honestly, because these were chosen for tactical merit without cost
accounting:

- **Verticality** adds a dimension to line of sight and pathfinding. Measurement
  has since shown it makes *perception* cheaper rather than dearer, because
  levels disperse units and cross-level pairs reject early — though that result
  depends on a conservative cross-level visibility rule. Its effect on
  pathfinding remains unmeasured.
- **Semantic edges** add an ordered edge test to every trace and every LOS ray,
  and make cached results depend on edge state as well as geometry.
- **Sustained targeting** adds a per-engagement maintenance check every tick for
  the duration of every engagement. Cheap individually; continuous and
  proportional to how much fighting is happening.
- **Per-shot physical tracing** with burst weapons is more per-shot work than
  either reference game performs.
- **Area engagements** evaluate occupancy of a zone rather than a target.

The pattern is worth noting: several of these were adopted because they made the
game better, and each was correct on its own terms. Their aggregate cost has
never been considered together, which is what this document exists to fix.

## Structural requirements

Three properties protect the budget and should be treated as design
constraints rather than optimisations.

### Staggered replanning cadence

Measured: replanning every unit every tick costs **150 ms**, four times the
entire tick budget. Every four ticks still does not fit at 36 ms. A cadence of
one replan per unit per second fits at 7 ms, and one per five seconds at 1.4 ms.

The design already required route generation to be "bounded, staggered, or
event-driven". The sustainable figure is **on the order of one replan per unit
per second**, with event-driven replans layered on top for units that need one.

### Path search must be expansion-capped

A search for an unreachable goal exhausts the reachable region before failing:
measured at **3.85 ms and 66,830 nodes**, roughly 8% of a tick to discover that
no route exists. Destruction closing a route, an overrun referent, or a cut-off
squad all produce this case in live play.

A node-expansion cap is a requirement rather than an optimisation, and failure
must surface as an event so control logic can respond instead of silently
retrying.

### Cache only what stays warm

Line-of-sight memoisation was measured and **does not pay while units move**: a
cell-pair key achieves 4.2% hit rate in motion against 98.6% at rest, and the
per-direction variant is a net 15% loss during movement because a cache that
misses still costs. Symmetric pair evaluation removes 22% of rays
unconditionally and is not a cache.

This generalises the existing caution about all-pairs tables. Before adding a
cache to any centre, establish its hit rate *under motion and contact*, not at
rest, and confirm that a miss is cheap enough to lose on.

### Observation marshalling must be bulk

Measurement showed that building an observation with one interop call per field
costs seven to sixteen times more than assembling it host-side and copying it
once. At the target force this is the difference between marshalling being 87%
of a cheap total and being nearly free.

This is the single most important implementation constraint on the control path.

### Perception must cull before it evaluates

Observer-to-target evaluation is quadratic before culling, and measurement
confirms the shape: each doubling of force costs 3.3-3.9×.

What measurement also showed is *which* cull earns its place. The **attention
sector** test is worth 1.7×. The broad-phase spatial index is worth almost
nothing at the supported force target — 0.482 ms without it against 0.464 ms
with — because the range check alone is already cheap. Keep it for headroom at
larger forces, but do not treat it as load-bearing.

### Full-state hashing cannot be per tick

`simulation-core-architecture.md` calls for per-tick state hashes. Hashing the
complete authoritative state of 200 units and their components twenty times a
second is likely unaffordable at the target scale.

The determinism contract needs incremental hashing, hashing of a committed event
stream rather than full state, or hashing at a lower frequency with full-state
hashes at checkpoints. The contract's *guarantee* is unchanged; its
implementation cannot be the naive one.

## Fallback positions

Each centre needs a known retreat, decided before measurement rather than under
pressure. Several are already recorded elsewhere and are collected here.

| Centre | Fallback if the budget is exceeded |
|---|---|
| Perception | measured at ~1% of budget; no fallback currently required. If one is ever needed: symmetric pair evaluation first (22% fewer rays, free), then fewer exposure sample points, then coarser acquisition rate. Line-of-sight memoisation is *not* a useful fallback: it does not pay while units are moving |
| Line of sight | reduce sight range, which dominates ray cost; larger cached region granularity. Reducing level count would not help and might hurt |
| Combat tracing | probabilistic cover interception in place of full geometric tracing, at the cost of weakening the semantic edge model |
| WASM | measured at ~0.5-2% of budget; no fallback currently required |
| Observation richness | reduce disclosed detail per unit, which is already the bandwidth mechanic |
| Engagement maintenance | evaluate maintenance every *n* ticks rather than every tick, accepting coarser interruption granularity |
| Projection | coalesce deltas more aggressively; lower projection cadence below tick rate |
| Hashing | checkpoint hashing with event-stream hashing between checkpoints |
| Everything | reduce the supported force target below 100 per side |

The last row is the honest one. If the aggregate does not fit, the 100-per-side
target is a design parameter like any other, and reducing it is preferable to
degrading the rules that make the game distinctive.

## Measurement requirements

The skirmish plan already requires that measurements report simulation, WASM,
host services, networking, serialization, and replay separately. This document
adds that measurement must:

- attribute cost to the centres named above rather than to "simulation";
- report **worst-tick** and high-percentile figures, not averages, because a
  real-time simulation is bounded by its worst ticks;
- measure under *contact*, not at rest, since perception, engagement
  maintenance, and wakes all peak together during a firefight and that
  correlation is the actual risk;
- report allocation and garbage-collection behaviour, since a managed runtime's
  pauses land inside the tick budget; and
- record the hardware and configuration, so figures remain comparable.

## Gates

Attach budget verification to the scale gates the skirmish plan already defines:

```text
4v4      instrument the cost centres; establish the measurement harness
20v20    first real allocation review; correct the provisional shares
50v50    worst-tick figures must fit the working target
100v100  worst-tick figures must fit with declared headroom, under contact
stress   demonstrate graceful degradation rather than desynchronisation
```

A gate that a subsystem fails is a decision point about that subsystem's
fallback, taken deliberately, rather than an occasion for unplanned
optimisation.

## Documentation replay-player measurement

The SVG documentation player measures its pipeline separately from the
authoritative 20 Hz server tick. The reproducible Phase 4 validation reports:

- worker execution and bounded projection transfer from
  `scripts/measure-worker.mjs`;
- pure projection mapping for 200 normal and 400 stress units;
- production Fable build and DOM reconciliation through
  `scripts/smoke-client.mjs`;
- the normal-view interactive SVG-node estimate against the 8,000-node limit;
  and
- canonical safe-SVG construction for the same 200/400-unit fixtures.

The test output records p95 rather than only averages. The 200-unit projection
has a hard p95 guardrail of 8 ms. Safe export has review guardrails of 100 ms
normal and 250 ms stress. The 400-unit figure is a stress observation, not a
promise that 400 interactive units meet the normal 60 Hz paint target. Browser
paint, style, and layout remain environment-dependent; the browser smoke test
proves reconciliation and interaction, while the committed review manifest
pins the production bundle and rasterizer used for human visual evidence.

On the Phase 4 validation host, 240 measured normal projections produced a
0.166 ms p95; 120 stress projections produced a 0.357 ms p95. Canonical SVG
construction measured 8.926 ms p95 for 200 units and 10.918 ms for 400 units.
The 200-unit scene estimated 6,942 interactive nodes. The worker advanced
24,000 ticks in 94 bounded batches in 99.625 ms elapsed, observed its heartbeat,
and emitted no more than 94 projection messages; its slowest measured batch was
0.003 ms. These numbers are dated review evidence, not portable hardware
guarantees.

## Open parameters

- Benchmark hardware and configuration.
- The working target and required headroom as fractions of the ceiling.
- Target match density per core, which is an operating cost decision.
- Whether the 20 Hz rate is itself negotiable if the aggregate does not fit.
- Provisional allocation shares, which exist to be replaced by measurement.
- Whether any centre justifies a native or unmanaged implementation.
