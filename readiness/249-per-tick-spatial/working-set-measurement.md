# S.I.R.#249 — per-finding working-set measurement

Recorded here rather than in `docs/performance-budget.md`, which #249 declares off-limits because
it is contended by #232, #235 and #236.

## How to reproduce

```sh
scripts/measure-working-set.sh e1185714734d5a4942c60e84ca31e371b7c50f80 3
```

`e118571` is this branch's rebase base — the commit the `before` side is built from. **The base is a
required argument.** It used to default to `origin/main`, resolved at read time, which was a latent
falsehood with a fuse on it: the moment this branch merges, `origin/main` *contains* the change,
`before` becomes the same code as `after`, every workload reports ~1.00x, and the harness exits 0. A
harness that compares something to itself does not fail — it silently succeeds, and the absence of a
difference reads as a result.

The harness now refuses that, and records its own provenance so the figures below are re-derivable:

```
working-set-measurement base-ref=e118571… base-sha=e118571… head-sha=… rounds=3
working-set-measurement swapped-source=src/SIR.Simulation/SpatialQuery.fs base-blob=772d34c5… tree-blob=f01db540…
working-set-measurement swapped-source=src/SIR.Simulation/Simulation.fs   base-blob=01d2a118… tree-blob=c29f4269…
working-set-measurement before-assembly=<sha256> after-assembly=<sha256> assemblies-differ=yes
```

`e118571` remains the recorded base after a later rebase onto a newer `main`, and that is checked
rather than assumed: across the 12 intervening commits both swapped files have identical blob ids
(`SpatialQuery.fs` `772d34c5…`, `Simulation.fs` `01d2a118…`), so the `before` side is the same code
and the figures below still describe this change. This is the same check that retired the false
"the rebase moved F4" story below — applied before restating a number rather than after.

Three guards, and each is stated at the strength it actually has:

- **On source blobs, not on commits.** Two different commits routinely carry identical text for these
  two files, so "the refs differ" does not imply "the sides differ". Refuses when every swapped
  source is byte-identical between the base and the working tree. A real detector.
- **On the built assemblies — in the equality direction only.** Two identical assemblies built from
  two *different* sources means a build was skipped, and that is refused (exit 4). Digest
  **inequality** proves nothing, for the reason below, so this guard cannot certify that two sides
  differ; it can only catch the case where they provably do not.
- **On the runtime.** Each side records the framework it actually loaded, and the harness refuses
  when they disagree (exit 9). A framework version, unlike a build digest, *is* a meaningful
  identity.

## The harness produced ~1.00x on repeated runs, and that was a real defect

Running the harness three times in succession gave F4 = 3.56x, then **0.99x, then 1.01x** — with a
perfectly valid base and genuinely differing sources. Cause: `restore()` used `cp -p`, so the working
tree was restored with its *original* mtimes, older than the build outputs already on disk. The next
incremental build therefore considered the sources up to date and handed back the previous side's
assembly. Both sides then measured the same code.

**What closes it is the `touch` pair, and that is a mitigation rather than a detector.** Touching the
swapped sources on restore and before each side's build makes them newer than any existing output, so
the incremental build cannot skip. Nothing detects the escape after the fact.

An earlier revision of this file claimed the assembly-digest comparison was the thing that caught it.
**That was wrong, and the reason is a property of the ground rather than of the implementation: this
toolchain does not guarantee reproducible output.** The evidence is in this file's own recorded
provenance — three different `before-assembly` digests across the three clean runs below, for the one
immutable base blob at `e118571`. (Repeated rebuilds of an unchanged source often *do* agree — five
in a row did — which is precisely why inequality cannot be relied upon: it is not stable enough to
mean "different", and not unstable enough to notice.)

So digest inequality carries no information, and no detector can be built on it. Digest **equality**
does carry information: two identical assemblies from two different sources means a build was
skipped. That case is refused (exit 4), verified by stubbing `dotnet build` to a no-op so both sides
copy the same assembly. The claim is narrow because the mechanism is narrow.

## Results — 3 independent runs, 3 interleaved rounds each, one workload per process

| Finding | Workload | Before | After | Change |
|---|---|---|---|---|
| F2 + F5 | exact LOS, 4x4 footprint (256 pairs), 1344 boundaries | 12.25 ms | 4.48 ms | **2.73x faster** |
| F4 | bounded path, fallback loop, 184 boundaries | 37.46 ms | 10.39 ms | **3.60x faster** |
| F3 | cache lookup against 512 dynamic entries | 11.34 us | 11.78 us | **no win — 4% slower** |
| F1 + F6 | authoritative tick, 331 board edges, 20 observations | 10.35 ms | 7.57 ms | **1.37x faster** |

Per-run ratios, and the spread that matters — **across** runs, not within one:

| Finding | run 1 | run 2 | run 3 | across-run swing |
|---|---|---|---|---|
| F2 + F5 | 2.72x | 2.73x | 2.73x | 1% |
| F4 | 3.67x | 3.60x | 3.56x | 3% |
| F3 | 0.96x | 0.97x | 0.93x | 4% |
| F1 + F6 | 1.37x | 1.34x | 1.39x | 4% |

### A correction, and the wrong lesson it nearly sealed

An earlier revision of this file reported F4 as **2.9x** and attributed the drop from 3.5x to the
rebase onto current `main`. **That causal story was false and is retracted.** The rebase cannot have
moved this number: `measure-working-set.sh` swaps exactly two files, and both have identical blob
ids at the old and new bases — they were last touched 190 commits earlier. The baseline did not move.

The 2.9x was a **load artifact**. That measurement was run in the background while other work
occupied the machine; its *before* side matched, only its *after* side was inflated. Re-run serially
on a quiet machine, the figure is 3.60x with a 3% across-run swing, and the original 3.5x was
substantially correct.

The earlier "not stable to better than a few percent" bound was also derived incorrectly — from
spread *within* a single run, where all rounds share the same machine load and therefore agree with
each other while being jointly wrong. Within-run agreement is not evidence of reproducibility. The
across-run figures above are the honest bound, and they are only meaningful because the runs were
serialized.

## F3's predicted win did not materialise, and that is reported rather than assumed

Removing `DynamicEntries @ StaticEntries` from every lookup is a real allocation removal, but it does
not show in this workload, whose static tier is empty — and F# `List.append` already returns the
first list unchanged when the second is empty, so the concatenation this item removed was **already
free in exactly the shape the workload exercises**. The after side measures 4% slower, inside the 4%
across-run swing. Reported as no win, and specifically not as a win. The change still matters for a
cache with a populated static tier, which this workload does not construct.

The other half of F3 as filed — "re-evaluating an existing key appends an unreachable duplicate" — is
not reachable at all: `evaluateCached` returns before insertion on a hit, and
`tests/SIR.Conformance.Shared/SpatialQueryFixtures.fs` has been pinning full cache identity across a
hit for as long as it has existed. The reachable half was the unbounded dynamic tier, now bounded.

## Structural counters — identical on both sides

These do not move with machine load, and they are the evidence that behaviour is unchanged. Each was
identical on the before and after side in every round of every run — they are stable, unlike the
timings:

- `expansions=545`, `path-cells=35`, `cost=34` for the bounded path
- `crossed-cells=86`, `crossed-edges=86`, `visible=true` for the exact LOS
- `observed=20`, `events=276` for the authoritative tick
- `dynamic-entries=512` for the cache

Canonical conformance output is byte-identical to the base across the full 211,029-byte corpus.

## What is continuously gated

Four properties, each enforced by a gate that runs in CI, and each with a committed inversion in
`scripts/test-working-set-gate-mutations.sh` proving the gate can fail:

| AC | Property | Gate | Inversion case |
|---|---|---|---|
| 1 (F1) | world built at most once per observation phase | `SimulationFixtures` | `world-construction-count` |
| 2 (F2/F6) | boundary index answers first-declaration-wins | `SpatialQueryFixtures` | `boundary-index-first-wins` |
| 3 (F3) | dynamic cache tier is bounded | `SIR.Match.Tests` | `dynamic-cache-bound` |
| 4 (F4) | `neighbours` evaluated once per expansion | `--print-spatial-performance` | `neighbours-once-per-expansion` |

### F4 is gated on allocation, because that is the only signature it has

`neighbours` is deterministic and side-effect free, so evaluating it twice per expansion leaves the
path, the cost, `expansions`, the crossed sets and the canonical bytes **byte-identical**. No
result-level assertion can see it. Allocation can:

| | allocated bytes | boundaries | expansions | path-cells | cost | outcome |
|---|---|---|---|---|---|---|
| correct | **15,656,832** (byte-exact, 3 of 3 runs) | 184 | 545 | 35 | 34 | Found |
| `nextBest` re-evaluates `neighbours` | **26,831,424** (+71%) | 184 | 545 | 35 | 34 | Found |

Every counter is unmoved; only the budget moves. The ceiling is 20,000,000 bytes — between the
measured correct cost and the measured cost of the regression it exists to catch, rather than at a
round number, and far enough above the true figure to absorb runtime-version drift that a byte-exact
assertion would turn into a flake. The gate also asserts it actually reached the fallback loop
(`outcome=Found`, `expansions > 0`), so the budget cannot pass vacuously on a query that never ran.

This gate lives in `--print-spatial-performance`, which `scripts/verify-spatial-query.sh` invokes
unconditionally — on both the direct route and the `--prepared-pr` CI route. **It therefore needed no
change to `scripts/qualify-pr.sh`**, which is owned by S.I.R.#272.

**F5 remains measured but ungated.** Its signal is small (+5–16%) and it is folded into the F2+F5
workload; a budget that narrow would be a flake rather than a gate. F4 is gated; F5 is the one
remaining ungated acceptance criterion, and it is stated rather than hidden.

### The budget's anti-vacuity assertions, and the one that was decorative

An earlier revision guarded the budget with `expansions > 0`. That was decorative: `boundedPath`'s
**fast path returns `int32 path.Length` as its expansion count**, so the counter is positive on both
routes and separates neither. Emptying the wall-dense world validates the package A* candidate and
never enters the fallback loop — `expansions=21 path-cells=21`, 504,896 bytes — and the old assertion
passed it.

Two assertions replace it, both inverted and observed red:

| escape | observed | caught by |
|---|---|---|
| emptied world → fast path taken | `expansions=21 path-cells=21`, 504,896 B | `expansions > path-cells`, which the fast path cannot produce because it forces equality |
| thinned walls → fallback still entered, still under ceiling | `boundaries=79 expansions=521 path-cells=27`, 16,343,104 B | the calibrated pin `184/545/35/34/Found` |

The second matters because it fits *under* the 20,000,000 ceiling: without the pin it would have
re-baselined the budget silently instead of failing.

## Gate-inversion harness wiring

`scripts/test-working-set-gate-mutations.sh` was previously referenced by nothing, while the
repository's equivalent for the pre-existing spatial subjects has three call sites — so the proof
existed but never ran, and would not have re-run when a later refactor reshaped the literals its
mutations match. It is now invoked by `scripts/verify-spatial-query.sh` on the serial direct route,
which also reaches `qualify-protected-preflight.sh` and `qualify-production.sh`, and through
`ci.yml`'s `protected-preflight` on every push to `main` plus nightly.

It is deliberately NOT on the concurrent prepared-PR route: it mutates `Simulation.fs` and
`SpatialQuery.fs` **in the working tree**, and that route runs its domain gates in parallel against
those same sources. `test-spatial-subject-mutations.sh` may run there because it builds its mutants
in an isolated directory; this one does not.

It also no longer leaves a mutant binary behind. Restoring the sources is not enough — the harness
builds mutants into **two** project output directories, and both survive it, so any later step
running with `--no-build` executes one of them. That is not hypothetical: `verify-spatial-query.sh`
runs `--print-spatial-performance --no-build` after calling this harness, and was safe only because
an unrelated `dotnet build` happened to sit between them.

Measured with the fix reverted, running each consumer with `--no-build` immediately afterwards —
**two** spurious failures, not one:

| consumer | residue | spurious failure |
|---|---|---|
| `SIR.Match.Tests` | `dynamic-cache-bound` mutant | `The dynamic spatial cache tier was not bounded: 1025 distinct keys left 1025 entries` |
| `--print-spatial-performance` | `neighbours-once-per-expansion` mutant | `Spatial bounded-path allocation budget exceeded: 26831424 bytes` |
| conformance corpus | `neighbours-once-per-expansion` mutant | **none — exit 0** |

The third row is worth keeping. The `neighbours` mutant leaves the full canonical corpus green, which
is an independent re-proof of why F4 needs an allocation budget at all: the corpus cannot see it. The
second row is this item's own new gate, which would have gone red for a reason that had nothing to do
with the code under test.

The harness now rebuilds the restored sources before returning, so it is self-contained rather than
ordering-dependent.
