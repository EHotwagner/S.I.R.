---
title: S.I.R. Performance Budget
status: proposed
document-type: living-design
version: "0.4"
last-updated: 2026-07-27
related:
  - docs/simulation-core-architecture.md
  - docs/skirmish-development-plan.md
  - docs/wasm-control-architecture.md
  - docs/combat-resolution.md
  - docs/tactical-environment-architecture.md
---

# S.I.R. Performance Budget

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

**Two centres are now measured, and both were badly overestimated.**

WASM invocation and observation marshalling came in at **~2%** against a
provisional 15% — see
[WASM Invocation Spike](research/wasm-invocation-spike.md).

Perception came in at **~1%** against a provisional 30% — see
[Perception Spike](research/perception-spike.md). It was the largest allocation
in this document and the one most feared, on the reasoning that it is quadratic
in unit count and had just gained a third dimension. It is quadratic, and it is
still about one percent of the tick at the supported force target.

Together the two centres the design most feared account for roughly **3% of the
tick**. Both measurements removed assumptions this document was built on, which
is exactly what the gates exist to do.

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
movement and reservation        30%
WASM invocation and marshalling  5%  (measured: ~2%)
combat resolution               10%
projection and serialization     10%
journal and hashing              5%
```

Movement now leads by default rather than by evidence. It is the largest centre
that has not been measured, and its cooperative reservation, dependency
resolution, and deadlock handling are algorithmically the most involved work in
the tick. That is a hypothesis, and the last two hypotheses in this document
were wrong by more than an order of magnitude each.

## What this session made more expensive

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

## Open parameters

- Benchmark hardware and configuration.
- The working target and required headroom as fractions of the ceiling.
- Target match density per core, which is an operating cost decision.
- Whether the 20 Hz rate is itself negotiable if the aggregate does not fit.
- Provisional allocation shares, which exist to be replaced by measurement.
- Whether any centre justifies a native or unmanaged implementation.
