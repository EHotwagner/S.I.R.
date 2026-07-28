---
title: Gameplay Testing and Balance Evidence
status: proposed
document-type: evidence-reference
category: Reference
categoryindex: 5
index: 13
version: "0.1"
last-updated: 2026-07-28
related:
  - docs/gameplay-reference.md
  - docs/research/wasm-invocation-spike.md
  - docs/research/perception-spike.md
  - docs/research/movement-spike.md
  - docs/research/rules-lab-prototype.md
---

# Gameplay Testing and Balance Evidence

## Summary

The project has completed three performance spikes and one fixed-state gameplay
rules laboratory. WebAssembly invocation, perception, and movement conflict
resolution are inexpensive at the target force size; pathfinding cadence is the
measured performance constraint. The current rules-lab run executes 25,000
samples per stochastic scenario and passes all twelve qualitative combat
invariants, while exposing sharp 🟥 risk boundaries around troll regeneration
and aggressive area suppression.

Return to the [Gameplay Reference](gameplay-reference.md).

## Evidence status

| Marker | Meaning on this page |
|---|---|
| 🟩 **Measured** | Output produced by executable code in the repository |
| 🟨 **Prototype input** | Formula or value being evaluated, not accepted balance |
| 🟥 **Risk** | Result that may violate intended play despite passing current invariants |

Passing an invariant means the expected ordering exists. It does not mean the
magnitude is balanced or fun.

## What was completed before the current run

### 1. WebAssembly invocation spike

**Question:** Can the server invoke one control-module instance per unit every
50 ms tick at 100 units per side?

🟩 **Result:** Yes, with wide margin.

| Measure | Result |
|---|---:|
| Mean complete tick at 200 units and 30 contacts | 0.79 ms |
| p99 | 1.15 ms |
| Maximum | 1.64 ms |
| Mean share of 50 ms tick | 1.6% |
| Ticks above 50 ms in a 1,200-tick run | 0 |
| Ticks above 20 ms working target | 0 |

Bulk observation copy reduces the target-force result to roughly 0.23 ms.
Marshalling dominates guest execution, but both are small. Serial invocation is
sufficient; parallel invocation is unnecessary at this scale.

The spike overturned the assumption that standing doctrine or command bandwidth
was technically necessary to reduce invocation frequency. Those systems must be
justified as gameplay, accessibility, and resilience rules.

### 2. Perception spike

**Question:** What does edge-aware, multi-level perception cost at 200 units?

🟩 **Result:** About 1% of the tick.

| Measure | Result |
|---|---:|
| Mean | 0.41 ms |
| p95 | 0.45 ms |
| p99 | 0.65 ms |
| Maximum | 0.85 ms |
| Allocation over 1,200 ticks | 0 |
| Garbage collections | 0 |

The test used a 512×512 grid, two levels, 2×2 footprints, hard-edged attention,
four exposure samples, edge-aware line of sight, and acquisition episodes.

Important findings:

- attention-sector culling is valuable;
- broad-phase culling is nearly irrelevant at the target scale;
- vertical levels can reduce work by dispersing units;
- cell-pair caches help stationary forces but can lose during motion;
- symmetric pair evaluation removes rays without depending on cache hits; and
- precomputed field of view is the wrong optimization at current density.

### 3. Movement spike

**Question:** What do cooperative movement reservation, conflict resolution,
and path search cost?

🟩 **Result:** Reservation is effectively free; pathfinding cadence is the
constraint.

| Target-force result | Value |
|---|---:|
| Reservation/conflict mean at 200 units | 0.003 ms |
| Maximum | 0.010 ms |
| Every unit replans every tick | 149.96 ms mean |
| Every unit replans every 20 ticks | 7.12 ms mean |
| Recommended order of cadence | About one replan per unit per second |

Failed searches are expensive: one unreachable-goal case cost 3.853 ms and
expanded 66,830 nodes. Node-expansion caps and event-driven failure reporting
are therefore required.

### 4. Rules-lab implementation

Before the current run, the project added a standalone .NET 10 F# executable
under [`spikes/rules-lab`](../spikes/rules-lab/). It:

- uses immutable fixed tactical board states;
- contains no framework, rendering, networking, AI, WASM, or pathfinding
  dependency;
- implements the 🟨 formulas documented in
  [Combat and Gameplay Formulas](gameplay-formulas.md);
- uses explicit deterministic seeds;
- performs weapon, armor, and regeneration sweeps; and
- exits unsuccessfully if any qualitative invariant fails.

## Current-run testing setting

This section records the exact setting for the run explained below.

| Property | Current run |
|---|---|
| Date | 2026-07-28 |
| Repository commit | `91691e4` |
| Command | `dotnet run -c Release --no-build` |
| Project | `spikes/rules-lab/RulesLab.fsproj` |
| Runtime reported by executable | .NET 10.0.10 |
| SDK | 10.0.302 |
| Operating system | Linux 7.1.4-arch1-1 x86_64 |
| Logical processors | 24 |
| Stochastic samples per scenario | 25,000 |
| Random generator | Seeded SplitMix64 |
| Build mode | Release |
| External packages | None |
| Result | 🟩 12 of 12 invariants passed |

### What a fixed board state contains

Each scenario supplies:

- attacker and target positions;
- target body profile;
- impact bearing: front, flank, or rear;
- exposed footprint fraction;
- cover protection;
- existing [Suppression](gameplay-formulas.md#suppression); and
- a bounded action window where applicable.

The run calculates relationships without pretending to be the final physical
simulation. It uses the prototype formulas and exact values copied into
[Weapons and Equipment](gameplay-weapons-equipment.md#prototype-weapon-profiles)
and [Units](gameplay-units.md#prototype-body-profiles).

## Current run: complete exposition

### Scenario 1 — engagement-time curves

**Purpose:** Verify that the carbine wins at close range, the rifle becomes
better over distance, and the marksman rifle stays slow but flat.

**Setting:** Fully exposed target, zero cover, zero existing Suppression.

| Weapon | 8 m | 20 m | 35 m | 50 m | 65 m |
|---|---:|---:|---:|---:|---:|
| Carbine | 0.52 s | 0.88 s | 1.39 s | 1.94 s | 2.51 s |
| Rifle | 0.67 s | 0.87 s | 1.15 s | 1.44 s | 1.73 s |
| Marksman rifle | 1.27 s | 1.31 s | 1.35 s | 1.40 s | 1.45 s |

🟩 **Interpretation:** The intended curve shapes exist. The carbine is faster at
8 m; the rifle and carbine are almost equal at 20 m; the rifle is clearly
better beyond that; and marksman preparation changes by only 0.18 s across the
entire range.

🟨 **Balance question:** The near-20 m crossover is a candidate, not a target.
The marksman becomes faster than the rifle around the long end, but damage,
peeking, target concurrency, and movement still determine its actual value.

### Scenario 2 — rifle against directional orc armor

**Purpose:** Test whether facing materially changes the result without making
the target a different unit.

**Setting:** Rifle, 25 m, fully exposed shielded orc, 100
[HP](gameplay-formulas.md#hp).

| Bearing | Armor | Nominal result | Expected damage/shot | Expected time to 0 HP |
|---|---:|---|---:|---:|
| Front | 38 | Partially mitigated | 6.34 | 8.13 s |
| Flank | 16 | Overmatched | 26.13 | 2.70 s |
| Rear | 10 | Overmatched | 27.87 | 2.59 s |

🟩 **Interpretation:** Flanking is decisive. The same rifle does about four
times the damage per shot from the flank, and expected incapacitation time falls
to roughly one third.

🟥 **Risk:** A threefold time difference may make frontal engagement
uninteresting rather than merely unfavorable. Multi-attacker concentration,
shield state, movement, and armor degradation are not yet represented.

### Scenario 3 — exposure, hard cover, and short peeks

**Purpose:** Verify that Cover acts independently during engagement preparation
and trace resolution, and that a brief peek defeats a slow precision solution.

**Setting:** Marksman rifle at 45 m against the same frontal orc.

| State | Engagement time | Expected damage/shot | Shots in 0.75 s |
|---|---:|---:|---:|
| Fully exposed | 1.39 s | 19.64 | 0 |
| 30% exposed behind protection 45 | 2.53 s | 6.37 | 0 |

A fully exposed commitment of 2.5 seconds permits one shot.

🟩 **Interpretation:** The test confirms both intended Cover roles:

1. less exposed footprint increases preparation from 1.39 to 2.53 seconds; and
2. hard material independently reduces expected damage from 19.64 to 6.37.

A 0.75-second peek ends before marksman preparation completes.

### Scenario 4 — three goblins crossing a held area

**Purpose:** Verify that a support weapon suppresses every occupant while
remaining less lethal per person than a point-engaged rifle.

**Setting:** Three fully exposed goblins, 35 m, three-second window, 25,000
samples per goblin.

| Measure | Result |
|---|---:|
| Mean damage per goblin | 27.3 HP |
| p10 | 20.2 HP |
| p50 | 27.4 HP |
| p90 | 34.4 HP |
| Incapacitation probability per goblin | 7.7% |
| Suppression per goblin | 100 |
| Suppression threshold | 50 |
| Expected damage across all three | 82.0 HP |
| Point-rifle damage rate against one goblin | 58.9 HP/s |

🟩 **Interpretation:** The area weapon creates force-wide tactical effect without
behaving like three simultaneous point engagements. Most goblins survive the
three-second crossing but all are heavily suppressed.

🟥 **Risk:** Suppression reaches the reporting cap of twice threshold. The
current rate may erase useful differences between “suppressed” and “far beyond
suppressed,” and gained Suppression does not yet feed back during the window.

### Scenario 5 — frontal fire against an armored troll

**Purpose:** Test whether heavy Armor plus Regeneration resists ordinary frontal
fire while retaining a dedicated anti-armor answer.

**Setting:** Armored troll, 240 HP, frontal Armor 55, Regeneration 6 HP/s, 30 m,
eight-second window, 25,000 samples per weapon.

| Weapon | Mean damage | p10 | p90 | Incapacitation |
|---|---:|---:|---:|---:|
| Rifle | 0.0 | 0.0 | 0.0 | 0% |
| Support weapon | 0.0 | 0.0 | 0.0 | 0% |
| Marksman rifle | 4.1 | 0.0 | 11.4 | 0% |
| Anti-armor launcher | 112.5 | 47.2 | 185.8 | 0% |

🟩 **Interpretation:** The intended counter hierarchy exists. Ordinary frontal
small arms cannot make net progress; anti-armor fire produces substantial
damage.

🟥 **Risk:** The troll may be an immunity puzzle or damage sponge. Even the
dedicated launcher does not normally incapacitate it within eight seconds.
Multiple shooters and discrete recovery timing are required before accepting
any troll value.

### Scenario 6 — orc frontal-armor sweep

**Purpose:** Show parameter sensitivity rather than selecting one Armor number.

**Setting:** Rifle at 25 m, 100-HP orc, fully exposed; frontal protection swept
from 20 to 50.

| Armor | Nominal outcome | Retained effect | Expected time to 0 HP |
|---:|---|---:|---:|
| 20 | Penetrated | 85.0% | 2.88 s |
| 22 | Penetrated | 72.3% | 3.22 s |
| 24 | Penetrated | 61.7% | 3.61 s |
| 26 | Penetrated | 52.7% | 4.06 s |
| 28 | Penetrated | 45.0% | 4.59 s |
| 30 | Penetrated | 38.3% | 5.22 s |
| 32 | Partially mitigated | 33.1% | 5.89 s |
| 34 | Partially mitigated | 29.3% | 6.54 s |
| 36 | Partially mitigated | 25.8% | 7.28 s |
| 38 | Partially mitigated | 22.8% | 8.13 s |
| 40 | Partially mitigated | 20.0% | 9.12 s |
| 42 | Partially mitigated | 17.5% | 10.28 s |
| 44 | Partially mitigated | 15.2% | 11.68 s |
| 46 | Partially mitigated | 13.2% | 13.36 s |
| 48 | Partially mitigated | 11.3% | 15.46 s |
| 50 | Partially mitigated | 9.5% | 18.13 s |

🟩 **Interpretation:** The continuous retained-effect curve prevents a hard
damage plateau at the named outcome boundary. Protection 32 changes the label
from penetrated to partially mitigated, but the numerical result remains
continuous.

🟥 **Risk:** Above roughly 40 protection, one frontal rifle takes nine seconds
or more in this analytic model. Whether this is desirable depends on
concentration, suppression, armor degradation, and positional counterplay.

### Scenario 7 — troll-regeneration sweep

**Purpose:** Locate the Regeneration rates where individual weapon profiles stop
making net progress.

**Setting:** Frontal armored troll at 30 m; Regeneration swept from 0 to 12 HP/s.

| Regeneration/s | Rifle | Marksman rifle | Anti-armor launcher |
|---:|---:|---:|---:|
| 0 | 71.34 s | 45.70 s | 12.51 s |
| 2 | 170.71 s | 71.71 s | 13.58 s |
| 4 | No net progress | 171.48 s | 14.89 s |
| 6 | No net progress | No net progress | 16.53 s |
| 8 | No net progress | No net progress | 18.62 s |
| 10 | No net progress | No net progress | 21.41 s |
| 12 | No net progress | No net progress | 25.29 s |

🟩 **Interpretation:** The test precisely identifies the current counter
boundaries. One rifle loses net progress around 4 HP/s; one marksman rifle loses
it around 6 HP/s; anti-armor remains effective throughout the sweep.

🟥 **Risk:** “No net progress” is produced by continuous expected-DPS
subtraction. Discrete large hits may interact with Regeneration very differently.
This table is diagnostic evidence for replacing or refining that timing model.

### Scenario 8 — invariant suite

The current run passed all twelve executable guards:

| Result | Invariant |
|---|---|
| 🟩 PASS | Flanking beats frontal Armor |
| 🟩 PASS | Rear is no safer than flank |
| 🟩 PASS | A 0.75-second peek defeats marksman preparation at 45 m |
| 🟩 PASS | A 2.5-second committed exposure permits precision fire |
| 🟩 PASS | Cover increases engagement time |
| 🟩 PASS | Cover independently reduces resolved damage |
| 🟩 PASS | Carbine prepares faster than rifle at 8 m |
| 🟩 PASS | Rifle prepares faster than carbine at 65 m |
| 🟩 PASS | A held area suppresses a crossing goblin |
| 🟩 PASS | Support fire is less lethal per individual than point rifle fire |
| 🟩 PASS | Troll Regeneration resists one frontal rifle |
| 🟩 PASS | Anti-armor fire overcomes troll Regeneration |

## Current-run conclusion

The present equations are useful enough to continue because they preserve every
qualitative relationship they were designed to test and expose sensitive ranges
through sweeps.

They are not ready to become canonical balance because:

- single-attacker expected DPS exaggerates binary immunity;
- continuous Regeneration erases event timing;
- Suppression does not feed back during a window;
- Armor does not degrade;
- ammunition and reloads do not exist;
- fixed exposure does not model moving between cover states;
- wounds and casualty consequences are absent; and
- full physical traces and friendly-fire geometry are not represented.

The correct next step is to add multiple attackers, discrete damage and
Regeneration, and moving exposure schedules before adding magic or anchor
formulas.

## Reproducing the current run

```sh
cd spikes/rules-lab
dotnet build -c Release
dotnet run -c Release --no-build
```

Quick development run:

```sh
dotnet run -c Release -- --quick
```

The quick run uses 2,000 samples. The full run uses 25,000. Both use explicit
deterministic seeds and should return a nonzero exit code if an invariant fails.

## Evidence limitations

- Measurements are machine-specific.
- The three performance spikes are deliberately disposable implementations, not
  product architecture.
- The rules lab evaluates formulas and fixed snapshots, not complete matches.
- Prototype values are not faction point costs.
- Passing orderings do not establish player comprehension, counterplay quality,
  mission balance, or fun.
- Every result must be rerun after a formula or catalog change.

## Source evidence

- [WASM Invocation Spike](research/wasm-invocation-spike.md)
- [Perception Spike](research/perception-spike.md)
- [Movement Spike](research/movement-spike.md)
- [Fixed-State Rules Laboratory](research/rules-lab-prototype.md)
- [Rules-lab scenarios](../spikes/rules-lab/Scenarios.fs)
