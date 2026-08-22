# S.I.R.#249 — per-finding working-set measurement

Recorded here rather than in `docs/performance-budget.md`, which #249 declares off-limits because
it is contended by #232, #235 and #236.

## How to reproduce

```sh
dotnet build tests/SIR.PhysicalCombat.Performance/SIR.PhysicalCombat.Performance.fsproj -c Release
dotnet run  --project tests/SIR.PhysicalCombat.Performance/SIR.PhysicalCombat.Performance.fsproj \
            -c Release --no-build -- --working-set <f2f5|f4|f3|f1f6>
```

The "before" side is the same binary with `src/SIR.Simulation/{SpatialQuery,Simulation}.fs` taken
from `origin/main`; only `SIR.Simulation.dll` is swapped between runs, so both sides execute an
identical harness.

## ONE WORKLOAD PER PROCESS — this is not a detail

The four workloads originally ran one after another in a single process, and in that shape the
harness **manufactured two reproducible ~2x regressions in code the affected workloads never
executed**. The trace and path workloads allocate a boundary index per evaluation; the heap state
they left behind slowed the cache and tick workloads that ran after them. `GC.Collect` between
workloads did not settle it — only process isolation did. Four interleaved rounds agreed with each
other and were still wrong. Do not re-merge these into one process.

## Results — 3 interleaved rounds, one workload per process, medians

| Finding | Workload | Before | After | Change |
|---|---|---|---|---|
| F2 + F5 | exact LOS, 4x4 footprint (256 pairs), 1344 boundaries | 12.23 ms | 4.48 ms | **2.7x faster** |
| F4 | bounded path, fallback loop, 184 boundaries | 37.94 ms | 10.78 ms | **3.5x faster** |
| F3 | cache lookup against 512 dynamic entries | 11.52 us | 11.74 us | **no change** |
| F1 + F6 | authoritative tick, 331 board edges, 20 observations | 10.21 ms | 7.44 ms | **1.38x faster** |

Per-round figures (ms, or us for F3):

| Finding | Before | After |
|---|---|---|
| F2 + F5 | 10.18, 12.56, 12.23 | 4.45, 4.58, 4.48 |
| F4      | 39.77, 37.26, 37.94 | 15.10, 10.68, 10.78 |
| F3      | 11.52, 11.48, 11.60 | 11.62, 12.08, 11.74 |
| F1 + F6 | 10.26, 10.13, 10.21 | 7.33, 7.72, 7.44 |

## F3's predicted win did not materialise, and that is reported rather than assumed

Removing `DynamicEntries @ StaticEntries` from every lookup is a real allocation removal, but it does
not show in this workload, whose static tier is empty — and F# `List.append` already returns the
first list unchanged when the second is empty, so the concatenation this item removed was **already
free in exactly the shape the workload exercises**. The change still matters for a cache with a
populated static tier, which this workload does not construct. Recorded as neutral, not as a win.

The other half of F3 as filed — "re-evaluating an existing key appends an unreachable duplicate" — is
not reachable at all: `evaluateCached` returns before insertion on a hit, and
`tests/SIR.Conformance.Shared/SpatialQueryFixtures.fs` has been pinning full cache identity across a
hit for as long as it has existed. The reachable half was the unbounded dynamic tier, now bounded.

## Structural counters — identical on both sides

These do not move with machine load, and they are the evidence that behaviour is unchanged:

- `expansions=545`, `path-cells=35`, `cost=34` for the bounded path
- `crossed-cells=86`, `crossed-edges=86`, `visible=true` for the exact LOS
- `observed=20`, `events=276` for the authoritative tick
- `dynamic-entries=512` for the cache

Canonical conformance output is byte-identical to `origin/main` across the full 211,029-byte corpus.
