---
title: Performance Budget
category: Tools & Evidence
categoryindex: 5
index: 5
status: proposed
document-type: living-design
version: "0.9"
last-updated: 2026-08-16
related:
  - docs/simulation-core-architecture.md
  - docs/skirmish-development-plan.md
  - docs/wasm-control-architecture.md
  - docs/combat-resolution.md
  - docs/tactical-environment-architecture.md
  - docs/simulator-worker-protocol.md
---

# Performance Budget

## Tactical visual-system budget

The shared production SVG reports its unit count, estimated node count, active
effect count, effect ceiling, density tier, motion route, and exact layer order
as reviewable metadata. `TacticalSceneProjection` computes those costs beside
the presentation projection so Editor, Plan, Simulate, and Review cannot hide
growth behind component-local rendering estimates.

| Production workload | Estimated SVG nodes | Active effects | Release projection p95 | Browser callback/main-thread inspection |
|---|---:|---:|---:|---:|
| Representative 100-unit replay | ≤ 5,000 | ≤ 128 | < 4 ms | < 16.67 ms |
| Stress 200-unit replay | ≤ 9,000 | ≤ 256 | < 8 ms | < 16.67 ms |

The fixtures include overlapping routes, statuses, attacks, and review
annotations. They preserve the inherited 200/400-unit `Battlefield` structural
guards as a second scale check. The browser number measures one animation-frame
callback plus tactical DOM inspection on the production route; it is explicitly
not a compositor, paint, GPU, or swapchain claim. Layout is bounded by one
retained SVG root, effects are capped at 256 and pointer-inert, and reduced
motion substitutes short opacity emphasis for spatial animation. Any budget
change requires an explicit rebaseline with exact-candidate evidence.

## Physical-combat v1 budget

The v1 schema caps a trace at 256 cells, area delivery at 256 cells, recipients
at 256, ordered facts at 4,096, and canonical explanation at 64 KiB. Release
qualification measures the complete representative scenario matrix and a
deterministic 100-unit/50-area-attack firefight after warm-up. The observed
candidate was 4 ms for the representative matrix and 17 ms for the stress
workload, against explicit 20 ms and 50 ms gates respectively.

The measured published route is also retained: under Slow-3G and 4× CPU the
initial response set is 1,119,915 bytes, the on-demand Rules explorer chunk is
51,702 bytes, and one authenticated physical-combat authority projection is
1,243 bytes. The route invokes that authority exactly once. Physical evaluator
code is absent from presentation chunks and retained only in the immutable
replay worker and Server/Match authority.

## Spatial-query budget

The schema-v1 spatial service caps one query at 4,096 expansions, 64 result
cells, 4,096 crossed cells/edges, 256 footprint samples, and a 64 KiB canonical
explanation. `scripts/verify-spatial-query.sh` measures an 80×80 maximum map,
selected-unit LOS, a bounded route preview, local invalidation, and deterministic
100/200-unit demand after runtime warm-up. Qualification targets are 20 ms LOS,
50 ms route preview, 10 ms invalidation, and 250/500 ms demand batches.

The first list-frontier implementation failed honestly at 193 ms for one route
and 1.6/3.1 seconds for the demand batches. The accepted implementation uses the
published package `Pathfinding.astar EightWay` result as a fast candidate and
then validates every returned step against S.I.R.'s stricter complete-footprint
transition envelopes. The measured Release candidate recorded 0 ms LOS, 0 ms
route, 0 ms invalidation, and 27/43 ms for 100/200-unit demand, with 41 route
cells, 41 expansions, and 2,156 explanation bytes.

## Authored tactical environment v1 budget

The tactical-environment route is qualified as bounded assembly, validation,
editor preview, local dependency invalidation, and representative environment
interaction. Every workload traverses the production Domain/Simulation
functions consumed by the editor, Simulator, and spatial/combat adapters.
Elapsed time is measured in Release with a monotonic clock and host/runtime
facts beside the result; deterministic fingerprints exclude elapsed time.

| Workload | Maximum expected scale | Structural budget | Initial timing gate |
|---|---:|---:|---:|
| Exterior assembly | 64 slots, up to 32 compatible variants per role | 64 selections, 2,048 compatibility inspections, 4,096 placed cells/features | 25 ms representative assembly |
| Catalog validation | 64 slots and 512 authored features | 512 ordered findings, 16,384 route expansions | 50 ms maximum validation/preview |
| Editor preview | One assembled 80×80 map | One assembly, one validation, 6,400 projected cells, 2,048 projected features | included in the 50 ms gate |
| Local invalidation | 256 cached dependency receipts | inspect at most 256 receipts; invalidate only intersecting entries; change one target | 10 ms |
| Combat queries | 100 units and 50 environment interactions | at most one target transition per action; zero propagated changes | 50 ms batch |

Cost stays observable through deterministic counters: slots visited, compatible
variants inspected, selections, placed cells/features, ordered findings,
dependency receipts inspected/invalidated, and query/action counts. The shape
of the work is the primary regression guard; noisy wall-clock assertions do not
replace it. In particular, a local feature transition returns exact dependency
keys and may invalidate only intersecting cache entries, while schema-v1
destruction cannot fan out into neighbour collapse.

The pre-implementation smoke on base `3dc50b5` establishes inherited-route
headroom, not acceptance evidence for the new parcel workload. On Linux x64
with .NET SDK 10.0.302, the Release Match suite passed in 10.145 s and its live
integration observed 40 ticks in 27.944 ms, preview in 0.213 ms, serialization
in 0.161 ms, worker transfer in 0.028 ms, and projection in 0.086 ms. The exact
replay digest was
`84c086053d423768c51f2dc7be23d495904a70fef4de6957f2c8b36ab31d4137`.

The Release Client suite passed in 14.649 s. Its dense 40×40 editor document
(1,600 terrain records, 3,120 edges, 200 units, and 200 regions) observed p95
preview 2.628 ms, command 2.471 ms, document validation 3.239 ms, undo/redo
10.969 ms, import 9.369 ms, export 1.348 ms, and 7,136 estimated interactive
nodes. The isolated worktree first required an ordinary restore because no
`obj/project.assets.json` existed; that setup event is not a performance result.

The dense maximum-map pointer-preview budget is versioned at 12 ms p95. This
retains roughly 29% headroom above the 9.271 ms hosted observation from repair
run 31917304397 while preserving the same 40×40 workload, structural gates,
and all other editor budgets. Further workload or implementation growth must
remain below this bound or explicitly rebaseline it with hosted evidence.

These observations are headless and make no compositor or swapchain claim.
Release acceptance must measure the exact candidate's new environment
workloads, record the workload-definition digest and host facts, and remain
blocked when a timing or structural budget is exceeded. Protected mutations
must also turn the owning gate red when selection ignores seed/state, stale
content identity is accepted, invalidation becomes global, or one action
changes more than its declared target/budget.

## Production delivery budget

The production client has an executable delivery contract in
`scripts/test-production-delivery-budget.mjs`. It records the versioned entry
asset's raw, gzip, and Brotli bytes and confirms the on-demand support chunk is
not folded into the initial entry. Fixed byte ceilings are opt-in environment
policy for deliberately bounded deployments; the growing application and its
feature routes have no default size cap. Browser coverage records a Slow-3G/4×
CPU request graph against the published Release server: mutable entries
revalidate; identity-qualified retained engines may be cached immutably.
ASP.NET Core owns these headers. A proxy or CDN may provide compression/caching
too, but must keep `Vary: Accept-Encoding` and must not weaken the documented
cache class.

## Browser simulator session budget

The bounded planning worker uses 256-tick cooperative batches. A normal
6,000-tick planning horizon has a hard budget of 24 projection messages and
5,000 milliseconds in the browser-worker qualification harness. The harness
measures the built retained worker rather than a DTO-only loop, and also proves
that cancellation can enter between batches.

The deterministic evidence command is:

```shell
npm run build:client
node scripts/smoke-worker-roundtrip.mjs
```

The output records the observed message count and elapsed time. Projection
deltas remain bounded by the existing inspection projection schema; no
per-tick render or full-world transfer is permitted.

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

## Tactical overlay projection and view

The item-183 qualification builds the renderer-neutral tactical overlay view for
two declared scenes: 100 disclosed units as the representative workload and 200
as stress. Every registered overlay mode is enabled (including held modes), so
the measurement includes footprints, directions, paths/cost/blockers, areas,
traces/impacts, and status payloads rather than only the default footprint
layer. Release projection must remain below 20 ms p95 for the 200-unit scene.

Projection admits at most 4,096 payloads and 256 labels. Its 5,000-node view cap
counts the actual overlay descendants emitted by the production SVG: each
payload group, every shape/blocker mark, and every label text node. The built
Chromium journey compares that declared count with `querySelectorAll("*")` on
the overlay layer and fails on disagreement or overflow. This is a structural
projection/view budget; compositor frame rate is not measured or claimed.

## Scenario-catalog production workload

The item-184 qualification imports the serialized maximum-scale sample as a
literal current-format 80×80 map with 200 units, previews a route through the
production map-editor simulator, advances eight production simulation ticks,
and projects every resulting frame through `Battlefield.scene`. Its receipt
records authoritative counters from those routes: A* expanded nodes,
simulation LOS evaluations, resolved attacks, and the scene's interactive node
estimate. The budgets are 4,096 path expansions, 256 LOS evaluations per tick,
256 combat resolutions per tick, and 8,000 scene nodes. Release elapsed-time
budgets remain 20 ms p95 and 50 ms p99. Headless browser evidence proves the
real boot-to-visible-outcome journey; it does not claim compositor frame rate.

## Open parameters

- Benchmark hardware and configuration.
- The working target and required headroom as fractions of the ceiling.
- Target match density per core, which is an operating cost decision.
- Whether the 20 Hz rate is itself negotiable if the aggregate does not fit.
- Provisional allocation shares, which exist to be replaced by measurement.
- Whether any centre justifies a native or unmanaged implementation.
