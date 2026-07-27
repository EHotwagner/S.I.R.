---
title: Perception Spike — Measured Results
status: accepted
decision-status: evidence
document-type: research
version: "1.1"
last-updated: 2026-07-27
related:
  - docs/performance-budget.md
  - docs/game-vision.md
  - docs/tactical-environment-architecture.md
  - docs/research/wasm-invocation-spike.md
---

# Perception Spike — Measured Results

## Question

What does one tick of perception cost at 100 units per side, on a 512×512 grid
with semantic edge occlusion and multiple levels?

Perception led the provisional allocation at **30%** of the tick budget, on the
reasoning that it is the only cost centre quadratic in unit count before culling
and that verticality had just made its geometry three-dimensional.

## Answer

**About 1%.** The allocation was roughly thirty times too pessimistic.

At the intended upper force target — 200 units in contact, 60-cell sight range,
four exposure sample points per target, two levels, edge-aware line of sight:

```text
mean   0.41 ms      0.8% of the 50 ms tick
p95    0.45 ms      0.9%
p99    0.65 ms      1.3%
max    0.85 ms      1.7%
```

Over 1,200 consecutive ticks — a full minute of match time — **zero allocation
and zero garbage collections.**

## What was measured

A working perception pipeline, not a model of one:

```text
broad-phase bucket cull  →  range  →  attention sector  →  edge-aware LOS
                                                        →  acquisition episodes
```

- 512×512 grid, two levels, cell blockers plus separate vertical, horizontal and
  floor edge layers
- an urban block map: buildings whose walls are edge features with door and
  window openings, interior partitions, scattered solid cells. **0.49% blocking
  cells, 5.35% opaque edges**
- 2×2 footprints, eight-direction facing, hard-edged ~135° attention sector
- integer DDA line of sight testing both cells entered and edges crossed, with
  the diagonal corner rule applied at vertices
- per-observer acquisition episodes in fixed slots, accumulating toward a
  threshold and decaying when not refreshed
- two forces placed in contact, drifting and turning each tick so the candidate
  set and ray outcomes change rather than measuring one frozen configuration

Everything is struct-of-arrays and integer-only. **Nothing in the hot path
allocates**, which the GC counters confirm.

There is no caching. This is the *uncached* cost, and therefore the honest upper
bound — the caching the spatial model calls for could only improve it.

## Where the cost goes

Per tick at the target force: 9,423 candidate pairs survive the broad phase,
2,660 pass the sector test, and 9,858 rays are walked for 402,425 total ray
steps.

**Attention sector is the valuable cull**, worth 1.7×:

| Configuration | mean | rays/tick |
|---|---|---|
| broad phase + sector + 4 samples | 0.464 ms | 12,319 |
| broad phase + sector + 1 sample | 0.288 ms | 3,325 |
| broad phase, **no sector**, 4 samples | 0.794 ms | 28,599 |
| coarse buckets (64 cells) | 0.449 ms | 12,319 |
| **no broad phase at all** | 0.482 ms | 12,319 |

The broad phase is worth almost nothing at this scale — 0.482 ms without it
versus 0.464 ms with. The range check alone is already cheap enough. It will
matter as unit count grows, but at the supported target it is not carrying the
design.

Exposure sample points cost what you would expect: four samples per target is
1.6× one sample.

## Scaling

Superlinear, as predicted, and visible well before it hurts:

| Units | mean | max | % of 50 ms | rays/tick |
|---|---|---|---|---|
| 50 | 0.031 ms | 0.047 ms | 0.1% | 718 |
| 100 | 0.119 ms | 0.214 ms | 0.4% | 2,895 |
| **200** | **0.460 ms** | **0.787 ms** | **1.6%** | 12,319 |
| 400 | 1.522 ms | 2.593 ms | 5.2% | 44,586 |
| 800 | 5.940 ms | 10.622 ms | 21.2% | 193,709 |

Each doubling costs roughly 3.3–3.9×, so the curve is close to quadratic. Even
so, **four times the supported force still fits the working target.**

Sight range matters and then saturates, because occluders stop rays early:

| Range | mean | rays/tick | steps/tick |
|---|---|---|---|
| 20 cells (10 m) | 0.087 ms | 1,140 | 15,804 |
| 60 cells (30 m) | 0.467 ms | 12,319 | 510,972 |
| 100 cells (50 m) | 0.972 ms | 27,573 | 1,717,565 |
| 160 cells (80 m) | 1.151 ms | 32,406 | 2,270,776 |

Force separation behaves as expected — cost appears only once forces are in
contact, rising from 0.059 ms at 150 cells apart to 0.620 ms at 15.

## Verticality is cheaper, not more expensive

This contradicts the performance budget, which warned that verticality "adds a
dimension at exactly the scale where its cost is already unmeasured."

| Levels | mean | max | rays/tick |
|---|---|---|---|
| 1 | 0.633 ms | 1.275 ms | 11,213 |
| 2 | 0.475 ms | 0.838 ms | 12,319 |
| 3 | 0.395 ms | 0.649 ms | 12,689 |
| 4 | 0.362 ms | 0.569 ms | 12,839 |

More levels made perception **faster**. The reason is straightforward once seen:
levels disperse units, so fewer pairs share a level, and a cross-level pair is
rejected cheaply by a floor test before any ray is walked.

**Caveat, and it matters.** The cross-level visibility rule here is deliberately
conservative — it permits sight between levels only through open floor in the
shared column. A permissive rule allowing oblique sight across levels would walk
real rays and cost more. The honest finding is that **verticality does not
inherently make perception expensive, and disperses units in a way that helps**;
it is not a licence to adopt any vertical visibility rule for free.

## Occlusion increases ray count

Open terrain with no occluders costs 0.590 ms and walks 5,437 rays; the urban
map costs 0.467 ms and walks 12,319.

The dense map walks **more than twice as many rays** because a ray that fails
forces the next exposure sample to be tried, while an unobstructed first sample
short-circuits the rest. Occlusion buys shorter rays and pays in more of them.

Both are cheap. The point is that neither open ground nor dense interior is the
worst case in the way one might assume.

## Does caching help?

The measurements above are uncached, so the obvious question is how much of that
work a cache would remove. The answer depends entirely on whether the force is
moving, and it is not the answer one would assume.

A line-of-sight memo keyed on both endpoints' cells and levels, with a validity
stamp:

| | mean | rays/tick | hit rate |
|---|---|---|---|
| **Force in motion** | | | |
| uncached | 0.431 ms | 10,060 | — |
| memo, per-direction key | **0.496 ms** | 9,642 | 4.2% |
| memo, symmetric key | 0.425 ms | 7,865 | 22.3% |
| **Force stationary** | | | |
| uncached | 0.392 ms | 10,790 | — |
| memo, per-direction key | **0.137 ms** | 152 | 98.6% |
| memo, symmetric key | 0.150 ms | 95 | 99.1% |

Both blocks start from the same diffused configuration, so they are directly
comparable.

### Caching helps when it is least needed

A stationary force gets a **2.9× speedup** at a 98.6% hit rate. A moving force
gets **nothing**, at 4.2%.

The reason is the one the spatial model already warns about: the memo is keyed
on cell pairs, and a 512×512 grid has far too many cell pairs for that key space
to stay warm while units keep moving into new ones. Every step a unit takes
changes its key against every contact it holds. The existing caution that
"complete all-pairs path or visibility tables are not automatically appropriate"
is confirmed empirically rather than by argument.

### Per-direction caching is a net loss in motion

At 0.496 ms against an uncached 0.431 ms, the memo costs more than the 4.2% of
rays it saves. **A cache that misses is not free**, and this one loses by 15%
during exactly the conditions perception is being asked to handle.

### Symmetry is the lever that always works

Geometric line of sight is symmetric, so an unordered pair need only be traced
once even though acquisition is evaluated separately in each direction. Keying
the memo canonically cuts rays from 10,060 to 7,865 — **22% fewer** — while
moving, and does so unconditionally rather than depending on hit rate.

Interestingly it is *slightly slower* than the per-direction key when the force
is stationary, because canonicalising the key costs a little and both are
hitting above 98% anyway. Symmetry is worth having for its ray reduction, not
for its cache behaviour.

### Precomputed field of view is the wrong optimisation

The instinctive answer to "cache the visibility work" is to shadowcast a field
of view per observer and reduce every target test to a lookup. At these ratios
that is roughly five times worse.

An observer tests **50.3 targets per tick**. A radius-60 shadowcast must visit
about **11,310 cells**. Field of view only becomes cheaper once a single
observer is testing more than roughly **273 targets per tick**, which is more
than the entire opposing force.

Ray casting wins because the ratio of targets to visible area is low. It would
stop winning at much higher unit densities or much shorter sight ranges.

### Recommendation

**Do not cache line of sight yet.** The uncached path already fits in about 1%
of the tick, a cell-pair memo does not pay during movement and can lose, and
field of view is the wrong shape entirely.

Adopt symmetric pair evaluation, because it removes a fifth of the rays for free
and is not a cache. Revisit memoisation only if unit counts rise far beyond the
supported target, and measure it against motion rather than at rest.

## What this changes in the budget

Perception drops from a provisional 30% to a measured **~1%** at the supported
force target. Together with the WASM invocation spike's ~2%, the two cost
centres the design most feared account for roughly **3% of the tick between
them**.

That leaves the remaining unmeasured centres — movement and reservation, combat
resolution, projection and serialization, journal and hashing — far more room
than the allocation assumed, and it removes perception as a reason to constrain
sight ranges, sample points, or level counts.

## Caveats

- One implementation of one model on one machine. The real perception system
  will do more.
- Acquisition is integer accumulation toward a threshold with linear decay. A
  richer model with per-modality stimulus, signature, and stance inputs would
  cost more, though the geometry — the expensive part — would not change.
- Stimulus modalities other than sight are not modelled. Sound, thermal, and
  emission propagation are separate work.
- No report generation, knowledge-state projection, or event emission. Those are
  downstream of perception and belong to other centres.
- The caching measurements use a single open-addressed memo. A hierarchical or
  region-based cache, which the spatial model also contemplates, is untested and
  would have different characteristics.
- Occluder density is generated, not authored. A real map may differ, though the
  sweeps bound the effect in both directions.
- The conservative cross-level rule caveat above.

## Reproducing

Source in [`spikes/perception`](../../spikes/perception). Requires the .NET 10
SDK and no other dependency.

```sh
cd spikes/perception
dotnet run -c Release
```
