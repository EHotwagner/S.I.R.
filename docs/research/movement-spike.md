---
title: Movement Spike — Measured Results
status: accepted
decision-status: evidence
document-type: research
version: "1.0"
last-updated: 2026-07-27
related:
  - docs/performance-budget.md
  - docs/game-vision.md
  - docs/formations-and-referents.md
  - docs/research/perception-spike.md
---

# Movement Spike — Measured Results

## Question

What do cooperative footprint reservation, conflict resolution, and path search
cost at 100 units per side?

Movement held the largest remaining allocation at **30%** of the tick — by
default rather than by evidence, since it is algorithmically the most involved
work in the tick and the last major centre unmeasured.

## Answer

The allocation was aimed at the wrong thing.

**Reservation and conflict resolution are effectively free** — 0.003 ms at the
target force, under 0.05% of the tick even at 800 units.

**Path search is the entire cost of movement**, and it is not a per-tick cost at
all. It is a *cadence* decision, and the cadence that fits is roughly **one
replan per unit per second**.

## Reservation and conflict resolution

| Units | mean | max | % of 50 ms | committed/tick |
|---|---|---|---|---|
| 50 | 0.001 ms | 0.004 ms | 0.01% | 2 |
| 100 | 0.001 ms | 0.008 ms | 0.02% | 5 |
| **200** | **0.003 ms** | **0.010 ms** | **0.02%** | 10 |
| 400 | 0.005 ms | 0.015 ms | 0.03% | 14 |
| 800 | 0.010 ms | 0.026 ms | 0.05% | 16 |

This covers credit accrual, intent collection, footprint claiming, multi-pass
dependency resolution, hostile symmetric blocking, commit, and persistent-wait
detection. Nothing in it allocates.

**Contention raises blocked transitions, not cost.** Two cases were built to
stress it:

- **Head-on convergence** — two forces advancing into one another's start areas
  gridlock, committing only 8.6% of attempted transitions. Resolve cost stays at
  0.021 ms.
- **A single 5-cell gap** in a barrier, with 200 units funnelling through it.
  Commit rate falls to 26%, 6,355 transitions are blocked by a friendly, and
  resolve cost is **0.003 ms**.

The multi-pass dependency resolution behaves as the design intends: 550 chain
advances in the congestion case are followers entering space a leader vacated on
the same tick, which is what lets a column move coherently instead of shuffling
one unit per tick. It converges in 1.2 passes on average and never approached
the 8-pass cap.

## Path search

Eight-way A* with **equal orthogonal and diagonal cost**, edge-aware, with
footprint clearance. Preallocated, no allocation per search.

| Distance | mean | max | nodes expanded | path length |
|---|---|---|---|---|
| 30 cells (15 m) | 0.126 ms | 0.430 ms | 2,158 | 79 |
| 120 cells (60 m) | 0.346 ms | 0.952 ms | 6,179 | 161 |
| 300 cells (150 m) | 1.047 ms | 2.811 ms | 17,981 | 330 |
| 450 cells (225 m) | 0.942 ms | 3.603 ms | 16,287 | 467 |

A single cross-map search costs more than **three hundred times** an entire tick
of conflict resolution.

## The binding constraint is replan cadence

This is the result that matters. Holding 200 units in motion and varying only
how often each one recomputes its route:

| Cadence | searches/tick | mean | max | % of 50 ms |
|---|---|---|---|---|
| every unit, every tick | 200 | 149.96 ms | 212.85 ms | **426%** |
| every unit every 4 ticks | 50 | 36.59 ms | 62.29 ms | **125%** |
| every unit every 20 ticks (1 s) | 10 | 7.12 ms | 23.98 ms | 48% |
| every unit every 100 ticks (5 s) | 2 | 1.41 ms | 7.82 ms | 16% |

Replanning every unit every tick is **four times over the entire tick budget**
on its own. Even a four-tick cadence does not fit.

The design already states that "route generation, local avoidance, reservation
extension, and replanning are bounded, staggered, or event-driven" and that
traffic handling must not "scale as a new whole-map cooperative search every 50
milliseconds." That is now quantified: **the sustainable cadence is on the order
of one replan per unit per second**, with event-driven replans on top for units
that actually need one.

## A failed search is expensive

When a goal is unreachable, A* exhausts the reachable region before failing. An
early version of the congestion scenario walled off its objective, and each
search cost **3.853 ms and expanded 66,830 nodes** before reporting failure.

That is roughly 8% of a tick spent discovering that a route does not exist, and
it is a plausible live occurrence: destruction closes a route, a referent is
overrun, a squad is cut off. A bounded node-expansion cap is not an
optimisation but a requirement, and the failure must be reported as an event so
control logic can respond rather than silently retry.

## Footprint size

| Footprint | resolve mean | resolve max | standable anchors |
|---|---|---|---|
| 1×1 | 0.002 ms | 0.014 ms | 99.5% |
| 2×2 | 0.003 ms | 0.013 ms | 97.7% |
| 3×3 | 0.005 ms | 0.014 ms | 95.0% |
| 4×4 | 0.008 ms | 0.021 ms | 91.5% |

Larger footprints cost slightly more to resolve and lose usable ground faster
than their area suggests, because a large base needs a clear square rather than
a clear path. Neither effect is a performance concern at these sizes.

## What this changes in the budget

Movement's 30% allocation was aimed at conflict resolution, which turns out to
be nearly free. The share should instead be expressed as a **replan budget**:
a number of path searches per tick, since that is the only part of movement
whose cost is significant and the only part the implementation controls.

At roughly 0.35 ms for a typical medium-range search, a 10% share of the tick
buys about **14 searches per tick** — comfortably more than the one-per-unit-per
second cadence needs at 200 units, with headroom for event-driven replans.

## Caveats

- **No hierarchical pathfinding.** The spatial model calls for connected
  regions, choke points, gateways, and hierarchical navigation data. None is
  implemented here, and all of it would reduce long-path cost substantially.
  These numbers are therefore a pessimistic upper bound on search.
- **No path reuse or caching.** Given the perception spike's finding that
  cell-pair caches do not stay warm under motion, route caching deserves its own
  measurement rather than an assumption.
- Single level. Inter-level connections and cross-level routing are not
  measured.
- Local avoidance and reservation extension over a longer horizon are not
  implemented; only single-step reservation with same-tick dependency
  resolution.
- The reservation horizon is one tick. A longer space-time horizon, which the
  design contemplates, would cost more and behave differently under congestion.

## Reproducing

Source in [`spikes/movement`](../../spikes/movement). Requires the .NET 10 SDK.

```sh
cd spikes/movement
dotnet run -c Release
```
