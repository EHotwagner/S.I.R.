---
title: Fixed-State Rules Laboratory
status: proposed
decision-status: non-canonical
document-type: research
version: "0.1"
last-updated: 2026-07-28
related:
  - docs/combat-resolution.md
  - docs/human-forces.md
  - docs/arcane-forces.md
---

# Fixed-State Rules Laboratory

## Summary

The F# rules laboratory evaluates provisional combat formulas against fixed
tactical snapshots without depending on the future game framework. Its first
increment covers engagement-time curves, exposure, cover, directional armour,
suppression, regeneration, and candidate goblin, orc, and troll bodies. All
formulas and values are non-canonical balance inputs; the useful result is the
relationships and sensitivity they expose, not the current numbers.

The executable is in [`spikes/rules-lab`](../../spikes/rules-lab/).

## Boundary

This is not a reduced implementation of the game simulation. It deliberately
omits rendering, networking, WebAssembly, AI, pathfinding, continuous movement,
and the final physical projectile trace. A fixed board state supplies:

- attacker and target positions;
- target body profile;
- impact bearing;
- exposed footprint fraction;
- cover protection; and
- existing suppression.

The lab then resolves one interaction or bounded time window. F# records remain
the scenario representation until repeated use demonstrates that an external
scenario format is valuable.

## Provisional formulas

### Engagement preparation

For range `r`, exposure `e`, and suppression ratio `s`:

```text
range time = base + slope × r ^ exponent

engagement time =
    range time
    ────────── × (1 + s × suppression penalty)
      √ max(e, exposure floor)
```

This makes partial exposure buy time before resolution without using cover
protection as the same modifier. Carbine, rifle, and marksman profiles differ
primarily in the shape of `range time`.

### Trace and layers

The current trace-contact surrogate is:

```text
trace probability = accuracy × exp(-dispersion × range)
```

After contact, penetration produces a continuous retained-effect value across
the canonical stopped, partially mitigated, penetrated, and overmatched bands.
An exposed trace tests body armour directly. A covered trace tests cover first,
reduces remaining penetration, and then tests body armour. Expected damage mixes
the exposed and covered paths according to exposed footprint.

This is intentionally a balance surrogate. A future authoritative
implementation still requires physical shot traces through real geometry.

### Area fire and suppression

Area weapons use an effect-density factor below one so that their individual
lethality is diluted while every occupant of the held area accumulates
suppression. Suppression is divided by the target body's resistance and capped
for reporting. It remains separate from HP damage.

### Regeneration

The first model subtracts regeneration as a continuous rate during the active
fire window. This is useful for locating rate boundaries but is not yet a
candidate damage-timing rule. It can overstate the immunity of a regenerating
target to intermittent fire because it does not preserve the sequence of
individual damage and healing events.

## First results

The full deterministic run uses 25,000 samples for stochastic windows and
passes twelve qualitative invariants.

### Weapon curves

At the provisional values:

- the carbine prepares in `0.52 s` at 8 m and `2.51 s` at 65 m;
- the rifle prepares in `0.67 s` at 8 m and `1.73 s` at 65 m; and
- the marksman rifle remains nearly flat, from `1.27 s` to `1.45 s`.

The carbine and rifle cross near 20 m. A marksman at 45 m cannot resolve against
a `0.75 s` exposure but can resolve one shot during a committed `2.5 s`
exposure.

### Directional orc armour

A rifle against the candidate shielded orc at 25 m produces:

| Bearing | Expected damage per shot | Expected time to incapacitation |
|---|---:|---:|
| Front | 6.34 | 8.13 s |
| Flank | 26.13 | 2.70 s |
| Rear | 27.87 | 2.59 s |

The intended ordering is present and strongly rewards a flank. Whether the
roughly threefold time difference is too decisive remains a balance question.
Sweeping frontal protection from 20 to 50 moves expected incapacitation time
from `2.88 s` to `18.13 s`, giving a useful range for later comparisons.

### Support weapon against goblins

Three fully exposed goblins crossing a held area at 35 m for three seconds each
receive:

- mean damage `27.3 HP`;
- `7.7%` incapacitation probability;
- suppression capped at `100` against a threshold of `50`; and
- much lower individual damage rate than a point-engaged rifle.

This has the intended initial shape: the weapon reliably disrupts the entire
crossing without behaving like three simultaneous precision rifles. Suppression
currently reaches twice its threshold, so its gain rate or the reporting cap may
be too aggressive.

### Troll armour and regeneration

With 55 frontal protection and 6 HP/s regeneration, one frontal rifle produces
no net damage over the eight-second sample. One marksman rifle produces only
`4.1` mean damage. An anti-armour launcher produces `112.5` mean damage but does
not normally incapacitate the 240 HP troll within that window.

The regeneration sweep exposes sharp boundaries:

- around 4 HP/s, one frontal rifle no longer makes net progress;
- around 6 HP/s, one frontal marksman rifle no longer makes net progress; and
- the anti-armour launcher continues to make progress through 12 HP/s, although
  its expected time rises from `12.51 s` at zero regeneration to `25.29 s`.

This confirms that armour, regeneration, and dedicated penetration can produce
the intended counter relationship. It does not yet establish that the troll is
fun to fight: multi-attacker concentration and discrete regeneration timing
must be tested before accepting any values.

## Enforced qualitative invariants

The executable exits unsuccessfully if any current invariant fails:

1. A rifle flank is better than a frontal attack against an orc.
2. The rear is no safer than the flank.
3. A short peek defeats marksman preparation.
4. Committed exposure permits precision fire.
5. Cover increases preparation time.
6. Cover independently reduces resolved damage.
7. A carbine prepares faster than a rifle at close range.
8. A rifle prepares faster than a carbine at long range.
9. A held area suppresses a crossing goblin.
10. Support fire is less lethal per individual than point rifle fire.
11. Troll regeneration resists one frontal rifle.
12. Anti-armour fire overcomes troll regeneration.

These are design guards, not assertions that the present balance is correct.

## Running

```sh
cd spikes/rules-lab
dotnet run -c Release
dotnet run -c Release -- --quick
```

The quick run uses 2,000 samples; the full run uses 25,000. Both use explicit
deterministic seeds.

## Next useful increments

The next physical-baseline work should add:

1. multiple attackers concentrating and dividing fire;
2. discrete damage and regeneration timing;
3. suppression feeding back into acquisition and movement windows;
4. ammunition expenditure and area-engagement duration;
5. armour integrity and repeated impacts;
6. a small number of moving exposure schedules rather than static windows; and
7. wound and incapacitation consequences after damage.

Arcane strain, rituals, and anchor capacity should follow only after this
physical baseline can distinguish durable mass from a damage sponge.
