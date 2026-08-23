# S.I.R.#249 — per-finding working-set measurement

Recorded here rather than in `docs/performance-budget.md`, which #249 declares off-limits because
it is contended by #232, #235 and #236.

## How to reproduce

```sh
scripts/measure-working-set.sh origin/main 3
```

That is the whole measurement, committed: it builds both sides, swaps only `SIR.Simulation.dll`
between runs so each side executes an identical harness binary, runs one workload per process, and
interleaves the two sides so machine load falls on both equally. A single workload can be run alone
with `--working-set <f2f5|f4|f3|f1f6>` against the performance project directly.

The gates this evidence supports have their own committed inversion harness:

```sh
scripts/test-working-set-gate-mutations.sh
```

It mutates one source line per gate, requires the suite to fail WITH THE EXPECTED MESSAGE, and
refuses a mutation that did not modify its subject — a pattern that has drifted away from the source
reports itself invalid rather than passing quietly.

## ONE WORKLOAD PER PROCESS — this is not a detail

The four workloads originally ran one after another in a single process, and in that shape the
harness **manufactured two reproducible ~2x regressions in code the affected workloads never
executed**. The trace and path workloads allocate a boundary index per evaluation; the heap state
they left behind slowed the cache and tick workloads that ran after them. `GC.Collect` between
workloads did not settle it — only process isolation did. Four interleaved rounds agreed with each
other and were still wrong. Do not re-merge these into one process.

## Results — 3 interleaved rounds, one workload per process, medians

**Re-measured after rebasing onto current `main`.** The figures below replace an earlier set taken
against a `main` that was 62 commits older; they are not the same numbers, and F4's ratio moved
materially (3.5x -> 2.9x), so the earlier table is not carried forward. `before` is `origin/main` at
the rebase base.

| Finding | Workload | Before | After | Change |
|---|---|---|---|---|
| F2 + F5 | exact LOS, 4x4 footprint (256 pairs), 1344 boundaries | 14.97 ms | 5.58 ms | **2.7x faster** |
| F4 | bounded path, fallback loop, 184 boundaries | 37.21 ms | 12.70 ms | **2.9x faster** |
| F3 | cache lookup against 512 dynamic entries | 13.11 us | 13.84 us | **no win — 5% slower** |
| F1 + F6 | authoritative tick, 331 board edges, 20 observations | 12.18 ms | 8.96 ms | **1.36x faster** |

Per-round figures, ascending (ms, or us for F3). These are medians of three; the spread is visible
and the timings are NOT stable to better than a few percent, which is why F3's 5% is read as noise
rather than as a regression:

| Finding | Before | After |
|---|---|---|
| F2 + F5 | 13.12, 14.97, 15.73 | 5.43, 5.58, 5.59 |
| F4      | 36.87, 37.21, 37.68 | 12.35, 12.70, 12.74 |
| F3      | 12.83, 13.11, 13.65 | 13.76, 13.84, 14.19 |
| F1 + F6 | 12.16, 12.18, 13.40 | 8.71, 8.96, 9.24 |

## F3's predicted win did not materialise, and that is reported rather than assumed

Removing `DynamicEntries @ StaticEntries` from every lookup is a real allocation removal, but it does
not show in this workload, whose static tier is empty — and F# `List.append` already returns the
first list unchanged when the second is empty, so the concatenation this item removed was **already
free in exactly the shape the workload exercises**. Re-measured against current `main` the after side
is 5% SLOWER, which is inside this harness's round-to-round spread (the before side itself varies by
6% across rounds); it is reported as no win, and specifically NOT as a win. The change still matters
for a cache with a populated static tier, which this workload does not construct.

The other half of F3 as filed — "re-evaluating an existing key appends an unreachable duplicate" — is
not reachable at all: `evaluateCached` returns before insertion on a hit, and
`tests/SIR.Conformance.Shared/SpatialQueryFixtures.fs` has been pinning full cache identity across a
hit for as long as it has existed. The reachable half was the unbounded dynamic tier, now bounded.

## Structural counters — identical on both sides

These do not move with machine load, and they are the evidence that behaviour is unchanged. Each was
identical on the before and after side in ALL THREE rounds — they are stable, unlike the timings:

- `expansions=545`, `path-cells=35`, `cost=34` for the bounded path
- `crossed-cells=86`, `crossed-edges=86`, `visible=true` for the exact LOS
- `observed=20`, `events=276` for the authoritative tick
- `dynamic-entries=512` for the cache

Canonical conformance output is byte-identical to `origin/main` across the full 211,029-byte corpus.

## What is continuously gated, and what is not

Three of this item's properties are enforced by fixtures that run in CI, and each ships a committed
inversion proving the fixture can fail (`scripts/test-working-set-gate-mutations.sh`):

| AC | Property | Fixture | Inversion case |
|---|---|---|---|
| 1 (F1) | world built at most once per observation phase | `SimulationFixtures` | `world-construction-count` |
| 2 (F2/F6) | boundary index answers first-declaration-wins | `SpatialQueryFixtures` | `boundary-index-first-wins` |
| 3 (F3) | dynamic cache tier is bounded | `SIR.Match.Tests` | `dynamic-cache-bound` |

That harness previously was referenced by nothing — unlike `scripts/test-spatial-subject-mutations.sh`,
which has three call sites — so the proof existed but never ran. It is now invoked by
`scripts/verify-spatial-query.sh` on the direct route. It is deliberately NOT on the prepared-PR/CI
route: it mutates `Simulation.fs` and `SpatialQuery.fs` **in the working tree**, and that route runs
its domain gates concurrently against those same sources, so it would be unsafe there without the
isolated-mutant-directory treatment `test-spatial-subject-mutations.sh` uses. Wiring the CI route
additionally requires `scripts/qualify-pr.sh`, which is owned by S.I.R.#272; its holder has been
notified rather than raced.

**F4 and F5 have no continuous gate, and this is a property of the findings, not an omission.**
"`neighbours` is evaluated once per expansion" and "`lineCells` is evaluated once per origin/target
pair" are pure-function call-count properties: both functions are deterministic and side-effect free,
so evaluating either twice produces byte-identical output, identical `expansions`, identical
`crossed-cells`/`crossed-edges`, and identical canonical bytes. There is no behavioural signature to
assert, and no injectable seam — `resolveBoundary` and the observers are constructed inside
`evaluate`. The available evidence for F4/F5 is therefore:

- the measured before/after above (F4 2.9x; F5 is folded into the F2+F5 workload), and
- the structural counters being identical on both sides, which proves the rewritten frontier
  selection explores the *same* search rather than a cheaper different one.

A source-text assertion that the call appears once was considered and rejected: it would restate the
diff rather than test behaviour, and would pass vacuously the moment the expression were reshaped.
Recorded as measured-but-ungated rather than claimed as gated.
