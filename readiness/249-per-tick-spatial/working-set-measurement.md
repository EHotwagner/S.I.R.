# S.I.R.#249 — per-finding working-set measurement

Recorded here rather than in `docs/performance-budget.md`, which #249 declares off-limits because
it is contended by #232, #235 and #236.

## How to reproduce

```sh
scripts/measure-working-set.sh ccee1d2778ab1da202df8ee0348e0664060e8ae4 3
```

`ccee1d27` is this branch's rebase base — the commit the `before` side is built from. **The base is a
required argument.** It used to default to `origin/main`, resolved at read time, which was a latent
falsehood with a fuse on it: the moment this branch merges, `origin/main` *contains* the change,
`before` becomes the same code as `after`, every workload reports ~1.00x, and the harness exits 0. A
harness that compares something to itself does not fail — it silently succeeds, and the absence of a
difference reads as a result.

The harness now refuses that, and records its own provenance so the figures below are re-derivable:

Recorded verbatim by run 1 of the three below (`…` marks truncation for width; nothing else is
elided, and no field here is a placeholder):

```
working-set-measurement base-ref=ccee1d2778ab… base-sha=ccee1d2778ab… head-sha=a607df031875… rounds=3
working-set-measurement swapped-source=src/SIR.Simulation/SpatialQuery.fs base-blob=772d34c52507… tree-blob=…
working-set-measurement swapped-source=src/SIR.Simulation/Simulation.fs   base-blob=01d2a1185c0e… tree-blob=…
working-set-measurement differing-sources=2/2
working-set-measurement side=after  assembly-sha256=c152ceef3949…
working-set-measurement side=before assembly-sha256=3950b44885f6…
working-set-measurement before-assembly=3950b44885f6… after-assembly=c152ceef3949… assemblies-differ=yes
working-set-measurement runtime=.NET-10.0.10 dotnet-root=/home/developer/.dotnet dotnet-root-x64=/usr/share/dotnet
```

The base has moved three times as this branch rebased, and **the `before` side has not changed once**
— checked at each rebase rather than assumed. Both swapped files carry identical blob ids at the
original `e118571` and at the current `ccee1d27` (`SpatialQuery.fs` `772d34c52507…`, `Simulation.fs`
`01d2a1185c0e…`), so every figure below describes the same comparison the earlier ones did. This is
the check that retired the false "the rebase moved F4" story below, now applied before restating a
number rather than after.

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
toolchain does not guarantee reproducible output.**

Measured across the three runs below, all built from the one immutable base blob at `ccee1d27`, the
`before` assembly hashed **two distinct values**:

| run | `before-assembly` |
|---|---|
| 1 | `3950b44885f6fbcb37f03f434daf797d71a7ac1c706efc76b0f724e93f7f98a6` |
| 2 | `5be3e84a4ecd4ad5635850ee176768de98732a76b8d28d7bbb5f7f08acefa701` |
| 3 | `5be3e84a4ecd4ad5635850ee176768de98732a76b8d28d7bbb5f7f08acefa701` |

Two distinct digests for identical input, and the `after` side moved with it (`c152ceef…` then
`36e092c3…` twice). Meanwhile five consecutive rebuilds of an unchanged source in one worktree all
agreed. So the variation is **not** a reliable signal in either direction: not stable enough for
inequality to mean "different code", and not unstable enough for anyone to notice it is unreliable.
An earlier revision of this file asserted three distinct digests; the measurement says two, and the
measurement is what is recorded here.

So digest inequality carries no information, and no detector can be built on it. Digest **equality**
does carry information: two identical assemblies from two different sources means a build was
skipped. That case is refused (exit 4), verified by stubbing `dotnet build` to a no-op so both sides
copy the same assembly. The claim is narrow because the mechanism is narrow.

## Results — 3 independent runs, 3 interleaved rounds each, one workload per process

Re-measured **after** the rebase onto `ccee1d27`, base `ccee1d27`, all six sides on `.NET-10.0.10`.

| Finding | Workload | Before | After | Change |
|---|---|---|---|---|
| F2 + F5 | exact LOS, 4x4 footprint (256 pairs), 1344 boundaries | 12.03 ms | 4.36 ms | **2.73x faster** |
| F4 | bounded path, fallback loop, 184 boundaries | 36.94 ms | 10.28 ms | **3.59x faster** |
| F3 | cache lookup against 512 dynamic entries | 11.13 us | 12.02 us | **no win — 8% slower** |
| F1 + F6 | authoritative tick, 331 board edges, 20 observations | 10.29 ms | 7.61 ms | **1.36x faster** |

Per-run ratios, and the spread that matters — **across** runs, not within one:

| Finding | run 1 | run 2 | run 3 | across-run swing |
|---|---|---|---|---|
| F2 + F5 | 2.78x | 2.73x | 2.64x | 5% |
| F4 | 3.53x | 3.59x | 3.60x | 2% |
| F3 | 0.84x | 0.93x | 0.94x | 11% |
| F1 + F6 | 1.36x | 1.38x | 1.34x | 3% |

F3's 11% swing is the widest here and sits on the smallest absolute quantity (microseconds); it
straddles 1.00x in neither direction far enough to be a win, and is reported as no win.

### A correction, and the wrong lesson it nearly sealed

An earlier revision of this file reported F4 as **2.9x** and attributed the drop from 3.5x to the
rebase onto current `main`. **That causal story was false and is retracted.** The rebase cannot have
moved this number: `measure-working-set.sh` swaps exactly two files, and both have identical blob
ids at the old and new bases — they were last touched 190 commits earlier. The baseline did not move.

The 2.9x was a **load artifact**. That measurement was run in the background while other work
occupied the machine; its *before* side matched, only its *after* side was inflated. Re-run serially
on a quiet machine, the figure is 3.59x with a 2% across-run swing, and the original 3.5x was
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
| 1 (F1) | world built once per phase **on the production route** | `--print-spatial-performance` | `production-route-world-construction` |

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
assertion would turn into a flake. The gate also asserts it actually reached the fallback loop —
`outcome = Found` **and** strictly `expansions > path-cells` — so the budget cannot pass vacuously on
a query that took one of the three non-loop exits. (`expansions > 0` appeared in an earlier revision
of this gate and is retracted; see below for why it separated nothing.)

This gate lives in `--print-spatial-performance`, which `scripts/verify-spatial-query.sh` invokes
unconditionally — on both the direct route and the `--prepared-pr` CI route. **It therefore needed no
change to `scripts/qualify-pr.sh`**, which is owned by S.I.R.#272.

**F5 remains measured but ungated.** Its signal is small (+5–16%) and it is folded into the F2+F5
workload; a budget that narrow would be a flake rather than a gate. F4 is gated; F5 is the one
remaining ungated acceptance criterion, and it is stated rather than hidden.

### The budget's anti-vacuity assertion took three attempts, and the first two were wrong

`boundedPath` has **three** non-loop exits, and each attempt covered only a prefix of them:

| exit | returns | defeats |
|---|---|---|
| `Unreachable` | `expansions = 0`, `path = []` | — |
| `Found` (fast path) | `expansions = int32 path.Length` — **equal by construction** | `expansions > 0` |
| `Exhausted` | `expansions = MaximumExpansions`, `path = []` — so `4096 > 0` | `expansions > path-cells` |

`expansions > 0` was decorative: positive on every exit. Replacing it with `expansions > path-cells`
closed the fast path but left `Exhausted` open — 4096 expansions against 0 path cells is strictly
greater on a query that never entered the loop at all.

The shipped assertion is `outcome = Found` **and** `expansions > path-cells`. `Found` excludes
`Unreachable` and `Exhausted`; strict inequality excludes the fast path, which forces equality. That
leaves only the loop, and it is a true impossibility statement rather than a nearly-true one.

Three escapes, all inverted and observed red:

| escape | observed | caught by |
|---|---|---|
| emptied world → fast path taken | `expansions=21 path-cells=21`, 504,896 B | strict `expansions > path-cells` |
| open world, target beyond `MaximumResultCells` → `Exhausted` | `expansions=4096 path-cells=0`, 2,907,472 B | `outcome = Found` |
| thinned walls → fallback still entered, still under ceiling | `boundaries=79 expansions=521 path-cells=27`, 16,343,104 B | the calibrated pin `184/545/35/34/Found` |

The third matters because it fits *under* the 20,000,000 ceiling: without the pin it would have
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
