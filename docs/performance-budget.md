---
title: S.I.R. Performance Budget
status: proposed
document-type: living-design
version: "0.1"
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

Nothing here is measured. Every number is a starting hypothesis to be replaced
by evidence, and its purpose is to force the argument about which subsystem
deserves which share, not to predict an outcome.

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
| Doctrine evaluation | units × rules × conditions | see below |
| Wake subscription evaluation | units × subscriptions | see below |
| WASM invocation | wakes purchased from bandwidth | bounded |
| Observation construction | wakes × what the unit can see | bounded |
| Engagement maintenance | active engagements | per tick |
| Combat resolution | traces on resolution ticks | bursty |
| Objective and logistics state | events | on change |
| Knowledge projection | participants × disclosed deltas | per tick or batched |
| Serialization and network | projection volume | per tick or batched |
| Journal, hashing, and replay | authoritative state size | see below |

### Provisional allocation

Of the ~20 ms working target, a starting hypothesis:

```text
perception and acquisition      30%
doctrine and subscriptions      15%
movement and reservation        15%
WASM invocation and marshalling 15%
combat resolution               10%
projection and serialization     10%
journal and hashing              5%
```

Perception leads because it scales worst — it is the only centre whose cost is
quadratic in unit count before culling, and verticality has just made its
geometry three-dimensional.

## What this session made more expensive

Recorded honestly, because these were chosen for tactical merit without cost
accounting:

- **Verticality** turns line of sight into a three-dimensional query and adds a
  dimension to pathfinding, at exactly the scale where neither was measured.
- **Semantic edges** add an ordered edge test to every trace and every LOS ray,
  and make cached results depend on edge state as well as geometry.
- **Sustained targeting** adds a per-engagement maintenance check every tick for
  the duration of every engagement. Cheap individually; continuous and
  proportional to how much fighting is happening.
- **Per-shot physical tracing** with burst weapons is more per-shot work than
  either reference game performs.
- **Doctrine evaluation** is a new *continuous* server-side cost for every unit,
  introduced specifically to reduce a different cost.
- **Area engagements** evaluate occupancy of a zone rather than a target.

The pattern is worth noting: several of these were adopted because they made the
game better, and each was correct on its own terms. Their aggregate cost has
never been considered together, which is what this document exists to fix.

## Structural requirements

Three properties protect the budget and should be treated as design
constraints rather than optimisations.

### Doctrine evaluation must be change-driven

Doctrine exists to avoid waking a module every tick. If the server instead
evaluates every unit's full rule list every tick, it has replaced one polling
cost with a cheaper polling cost rather than removing polling.

At 200 units with ten rules of three conditions, naive evaluation is roughly
6,000 condition evaluations per tick, or 120,000 per second, purely to discover
that almost nothing changed.

Doctrine must therefore re-evaluate only when an input a rule depends on
actually changes. The same applies to wake subscriptions. This is the single
most important implementation constraint arising from the control model, and
getting it wrong would silently undermine the reason doctrine exists.

### Perception must cull before it evaluates

Observer-to-target evaluation is quadratic before culling. Range, sector,
level, and spatial partitioning must reduce the candidate set before any
geometric work, and acquisition state must be maintained per contact episode
rather than recomputed per pair per tick.

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
| Perception | coarser acquisition update rate; fewer visibility sample points per footprint; reduced contact-episode count per observer |
| Line of sight | reduce supported level count; coarser vertical LOS; larger cached region granularity |
| Combat tracing | probabilistic cover interception in place of full geometric tracing, at the cost of weakening the semantic edge model |
| Doctrine | reduce rule-count and condition-count bounds per host class |
| WASM | reduce total command bandwidth, raising the value of doctrine quality |
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
