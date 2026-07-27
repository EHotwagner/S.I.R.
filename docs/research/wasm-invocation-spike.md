---
title: WASM Invocation Spike — Measured Results
status: accepted
decision-status: evidence
document-type: research
version: "1.0"
last-updated: 2026-07-27
related:
  - docs/performance-budget.md
  - docs/wasm-control-architecture.md
  - docs/research/wasm-runtime-selection.md
---

# WASM Invocation Spike — Measured Results

## Question

Can the authoritative server invoke one WebAssembly instance per unit, every
tick, at 100 units per side, inside a 50 ms budget?

The design had assumed not. Standing doctrine, command bandwidth, and the
wake-scheduling model were all justified partly on the belief that 200
invocations per tick was expensive. **That belief was never measured, and it is
wrong.**

## Answer

**Yes, by roughly two orders of magnitude.**

At the intended upper force target — 200 units, 30 known contacts each,
representative decision logic — a full tick of marshal, invoke, and read costs:

```text
mean   0.79 ms      1.6% of the 50 ms tick
p99    1.15 ms      2.3%
max    1.64 ms      3.3%
```

Over 1,200 consecutive ticks, a full 60 seconds of match time: **zero ticks
exceeded the 50 ms ceiling, and zero exceeded the 20 ms working target.**

## Environment

Measurements are machine-specific and should be re-run on target hardware.

| | |
|---|---|
| Runtime | .NET 10.0.10, server GC, concurrent |
| Wasmtime | 44.0.0 (`Wasmtime` NuGet package) |
| Host | Linux x64, 24 cores |
| Guest | WAT compiled at startup, core modules only |
| Profile | fuel enabled; SIMD, relaxed SIMD, bulk memory, reference types, multi-value, multi-memory, threads, tail calls and component model all disabled |

The guest scans its known contacts, scores each by threat, Chebyshev distance
and staleness, selects a target, and writes a decision. A `work` multiplier
repeats the scan to model heavier logic without changing its shape.

## Cost breakdown

Phases measured separately, 200 units, 30 contacts, `work=4`:

| Phase | mean | max | share |
|---|---|---|---|
| Observation marshalling | 1.015 ms | 1.570 ms | **87%** |
| Guest invocation | 0.123 ms | 4.127 ms | 11% |
| Output read | 0.021 ms | 0.040 ms | 2% |

The architecture's claim that marshalling dominates instruction execution is
**confirmed** — but at a magnitude that makes the distinction academic. Guest
execution for 200 units is roughly 0.12 ms.

## Marshalling is mostly an implementation choice

The 1.0 ms above uses one interop call per field: 30 contacts × 8 fields × 200
units is 48,000 boundary crossings per tick. Building the observation in a
host-side buffer and copying it once as a span:

| Contacts | Per-field | Bulk copy | Speedup |
|---|---|---|---|
| 15 | 0.358 ms | 0.051 ms | 7.0× |
| 30 | 0.682 ms | 0.091 ms | 7.5× |
| 60 | 1.354 ms | 0.092 ms | 14.7× |
| 120 | 2.661 ms | 0.160 ms | 16.6× |

Done properly, a full tick at the target force is closer to **0.23 ms**, or
0.5% of budget. The existing guidance to "avoid per-tick serialization and
allocation where the selected ABI permits" is worth roughly an order of
magnitude, and is the difference between marshalling mattering and not.

## Scaling

**Unit count** — linear, as expected, with no cliff:

| Units | mean | max | % of 50 ms |
|---|---|---|---|
| 100 | 0.394 ms | 0.844 ms | 1.7% |
| 200 | 0.801 ms | 1.161 ms | 2.3% |
| 400 | 1.600 ms | 3.190 ms | 6.4% |
| 800 | 3.245 ms | 6.850 ms | 13.7% |

**Fuel** — cost is invocations × allowance, and the allowance is the real knob:

| Work | Fuel consumed | mean | % of 50 ms |
|---|---|---|---|
| 1 | 2,701 | 0.767 ms | 1.9% |
| 16 | 42,526 | 0.913 ms | 2.2% |
| 64 | 169,966 | 1.391 ms | 4.1% |
| 256 | 679,726 | 3.302 ms | 8.4% |

A module burning 680,000 fuel per invocation — far beyond anything a doctrine
evaluation would need — still costs under 9% of the tick across 200 units.

**Observation size** — linear in payload, per-field path:

| Contacts | Bytes | Marshal | Invoke |
|---|---|---|---|
| 15 | 512 | 0.359 ms | 0.063 ms |
| 30 | 992 | 0.690 ms | 0.087 ms |
| 120 | 3,872 | 2.659 ms | 0.218 ms |

## Stress

Deliberate overload past the supported target:

| Units | Contacts | Work | mean | max | Verdict |
|---|---|---|---|---|---|
| 200 | 30 | 4 | 0.78 ms | 1.05 ms | fits target |
| 200 | 30 | 32 | 1.07 ms | 1.33 ms | fits target |
| 200 | 60 | 32 | 2.05 ms | 2.23 ms | fits target |
| 400 | 60 | 32 | 4.08 ms | 4.47 ms | fits target |
| 800 | 60 | 64 | 11.01 ms | 14.89 ms | fits target |

**Four times the force, twice the contacts, sixteen times the logic still fits
the 20 ms working target.**

## Allocation and GC

17.1 KB allocated per tick, one gen-0 and one gen-1 collection across 1,200
ticks, no gen-2. The allocation comes from boxing the invocation argument and
result through the untyped `Function.Invoke` path and would largely disappear
with typed function wrappers. GC is not a threat at this scale.

## Setup cost

Compiling the artifact once takes 4.3 ms. Instantiating 200 stores takes 8.0 ms,
about 0.04 ms each. Total per-match setup is roughly 12 ms, paid before live
play. Artifact reuse works as the architecture assumes.

## Parallelism is unnecessary

Parallel invocation across stores yields only 1.8–2.8× and degrades past four
threads, because per-instance work is too small to amortise scheduling. Keyed
outputs were identical after parallel runs, so determinism holds — but there is
no reason to spend the complexity. **Serial invocation is sufficient.**

## Architectural guarantees verified

| Property | Result |
|---|---|
| Instance isolation — shared compiled code, separate state | PASS |
| Fuel exhaustion traps, no partial output | PASS |
| No fuel carry-over between invocations | PASS |
| Determinism — identical inputs reproduce identical outputs | PASS |
| Restricted profile accepted by the runtime | PASS |

These cover items 1–5, 7 and 9–12 of the validation spike required by
[wasm-runtime-selection](wasm-runtime-selection.md). Items 6 and 8 — host-side
memory limiting and confirming no ambient WASI capability — and item 11's live
instance checkpointing remain untested and are the substantive gaps.

## What this overturns

**The performance justification for standing doctrine does not survive.** The
architecture can afford to invoke every unit's module every tick, which removes
the cost argument for a server-executed declarative layer entirely.

The remaining arguments for doctrine are unaffected by this measurement and must
be judged on their own merits:

- it lets a player express intent without writing code, through the canonical
  client and the standard module;
- it gives the server something to run when a module faults or is being
  replaced; and
- it makes behaviour inspectable and explainable in replay.

Whether those justify the specification cost of two vocabularies is now a design
question rather than an engineering constraint.

**Command bandwidth loses its primary rationale too.** If wake frequency is
affordable at 20 Hz for every unit, pricing it is a game-design choice — a
deliberate scarcity that creates an allocation decision and gives electronic
warfare a handle on the control layer — rather than a technical necessity. That
may still be worth keeping, but it should be argued as a mechanic, not as a
budget.

## Caveats

- One machine, one configuration. Re-run on target hardware.
- The guest is representative in *shape*, not in complexity. A real module doing
  pathfinding through host services would cost more, though host services are
  metered separately by design.
- Observation construction is modelled as writing a payload, not as querying the
  knowledge system to decide what the payload should contain. **That query is
  game logic and is not measured here** — it belongs to the perception budget,
  which remains the largest unmeasured cost centre.
- Nothing here measures the rest of the tick: perception, movement, combat
  resolution, projection, or hashing.

The last two caveats matter. This spike removes WASM invocation from the list of
concerns. It says nothing about whether the *simulation* fits.

## Reproducing

Source in [`spikes/wasm-invocation`](../../spikes/wasm-invocation). Requires the
.NET 10 SDK; no WebAssembly toolchain is needed because the test modules are WAT
compiled by Wasmtime at startup.
