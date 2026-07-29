---
title: Combat Values and Formulas
status: proposed
document-type: reference
category: Foundations
categoryindex: 2
index: 3
version: "0.1"
last-updated: 2026-07-28
related:
  - docs/gameplay-reference.md
  - docs/combat-resolution.md
  - docs/game-vision.md
  - docs/research/rules-lab-prototype.md
---

# Gameplay Combat and Formula Reference

## Summary

This page defines the shared combat vocabulary, canonical resolution order, and
every equation currently executed by the fixed-state F# rules laboratory.
🟩 rules describe accepted behavior; 🟨 equations and numerical thresholds are
non-canonical balance-lab inputs. Where the final formula is unresolved, this
page states the required inputs and qualitative relationship instead of
inventing a number.

Return to the [Gameplay Reference](gameplay-reference.md).

> **Prefer running code to copied algebra.** The
> [executable combat-formula walkthrough](combat-formulas.fsx) displays
> literate F# for engagement, retained effect, cover, and expected damage. The
> strict documentation build evaluates every example and compares it with the
> rules-laboratory implementation, so drift fails the build.

## Formula status legend

| Marker | Formula status |
|---|---|
| 🟩 **Canonical relationship** | Required behavior; exact values may still be open |
| 🟨 **Prototype equation** | Implemented in `spikes/rules-lab`; safe to sweep, not accepted balance |
| 🟧 **Required inputs** | Inputs are settled but the combining function remains open |
| 🟥 **Known limitation** | Current equation can produce misleading or undesirable behavior |

## Time, ticks, and distance

### 🟩 Authoritative time

```text
simulation frequency = 20 ticks / second
tick duration         = 1 / 20 second = 50 milliseconds
normal match          ≈ 20 minutes ≈ 24,000 ticks
```

Gameplay time is represented by integer ticks. Rendering can interpolate but
cannot change outcomes.

### 🟩 Grid distance

One cell is 0.5 m horizontally. A typical human has a 2×2-cell footprint,
occupying approximately 1×1 m.

For positions `(x1, y1)` and `(x2, y2)`, canonical distance uses the Chebyshev
metric:

```text
distance = max(abs(x2 - x1), abs(y2 - y1))
```

Orthogonal and diagonal movement therefore cost one equal step. Speed is
represented through deterministic fixed-point movement credit rather than
weighted diagonal costs.

The current rules lab uses Euclidean distance between its floating-point fixed
snapshot positions:

```text
lab_distance = sqrt((x2 - x1)² + (y2 - y1)²)
```

🟥 This is a lab simplification and must not silently become the authoritative
grid-distance rule.

## Action lifecycle

🟩 Every time-consuming action uses:

```text
start
  → preparation
  → commitment
  → resolution
  → recovery
```

Actions completing on different ticks resolve chronologically. Actions
completing on the same tick:

1. read one stable pre-resolution state;
2. calculate all outcomes;
3. apply those outcomes as one batch; and
4. then resolve incapacitation, death, breach checks, succession, and other
   consequences.

An interrupt must resolve on an earlier tick. Same-tick damage cannot
retroactively cancel an action that also completed on that tick.

## Attack resolution order

🟩 The complete attack pipeline is:

```text
local observation and acquired facts
        ↓
engagement preparation and maintenance
        ↓
attack resolves on an authoritative tick
        ↓
physical trace or slow projectile path
        ↓
cover or first contacted object
        ↓
armor or resistance
        ↓
HP and wound consequences
        ↓
suppression and secondary effects
        ↓
simultaneous consequence batch
```

Friendly units, civilians, and protected entities receive no implicit immunity
from valid traces or effects.

## Engagement

An **engagement** is a maintained targeting solution. A unit may maintain at
most one:

- **point engagement** against one specific unit; or
- **area engagement** against one geographical zone.

The solution must remain valid until resolution. Losing observation, range,
geometry, or adequate acquisition can cancel, suspend, or degrade it according
to the weapon contract.

### 🟧 Final engagement inputs

The final function must consider:

- weapon profile;
- range;
- exposed footprint;
- observation and [Acquisition](gameplay-command-information.md#acquisition);
- attention and facing;
- attacker movement and readiness;
- target movement;
- stance;
- suppression and wounds;
- prepared state; and
- equipment and abilities.

### 🟨 Rules-lab engagement equation

Definitions:

```text
r = range
e = clamp(exposure_floor, 1, target_exposure)
s = clamp(0, 2, existing_suppression / suppression_threshold)
```

Range curve:

```text
range_time = base_engagement_seconds
           + range_slope × r ^ range_exponent
```

Complete equation:

```text
engagement_seconds =
    range_time
    ────────── × (1 + s × suppression_engagement_penalty)
       √ e
```

Current shared parameters:

| Parameter | Value | Status |
|---|---:|---|
| Exposure floor | 0.10 | 🟨 Prototype |
| Suppression threshold | 50 | 🟨 Prototype |
| Suppression engagement penalty | 0.60 | 🟨 Prototype |

Partial exposure therefore extends preparation independently of whether the
cover later stops a trace.

### 🟨 Resolved shot count in a fixed window

```text
active_seconds = window_seconds - engagement_seconds

if active_seconds < 0:
    shots = 0
else:
    shots = 1 + floor(active_seconds × shots_per_second)
```

The first shot resolves when preparation completes. This does not yet model
reloads, bursts, ammunition, recovery, or loss of solution during the window.

## Cover and exposure

**Cover** is physical geometry, not a defense percentage stored on a unit.
Cover acts in two separate places:

1. exposed footprint affects engagement preparation; and
2. contacted material stops, mitigates, or passes the resolved trace.

A small exposure behind weak material may take a long time to engage yet offer
little protection after the trace resolves. A wide opening in strong masonry
may be easy to engage through but strongly stop misplaced traces.

### 🟨 Rules-lab cover path

For a covered path:

```text
through_cover = retained_effect(penetration, cover_protection)

remaining_penetration =
    penetration × (0.35 + 0.65 × through_cover)

covered_path_factor =
    through_cover
    × retained_effect(remaining_penetration, body_armor)
```

Expected layer factor:

```text
layer_factor =
    exposure × direct_armor_factor
    + (1 - exposure) × covered_path_factor
```

🟥 The final game must trace actual geometry in path order, including cells and
semantic edges. This mixture is only a fixed-state expectation surrogate.

## Armor

**Armor** (also spelled *armour* in the design documents) resolves after a trace
contacts a unit and before [HP](#hp) damage.

🟩 Inputs include:

- impact direction;
- coverage arc;
- damage type and penetration;
- protection and remaining integrity;
- stance or shield state; and
- explicit technological, biological, or magical resistance.

Canonical qualitative outcomes are:

- **stopped**;
- **partially mitigated**;
- **penetrated**; and
- **overmatched**.

### 🟨 Rules-lab outcome bands

For `ratio = penetration / protection`:

| Condition | Named outcome |
|---|---|
| `protection ≤ 0` or `ratio > 1.4` | Overmatched |
| `0.9 < ratio ≤ 1.4` | Penetrated |
| `0.5 < ratio ≤ 0.9` | Partially mitigated |
| `ratio ≤ 0.5` | Stopped |

### 🟨 Continuous retained-effect curve

The lab avoids damage plateaus by assigning a continuous retained fraction
inside the named bands:

```text
if protection <= 0:
    retained = 1
else:
    ratio = max(0, penetration / protection)

    if ratio <= 0.5:
        retained = 0.05 × ratio / 0.5
    elif ratio <= 0.9:
        retained = 0.05 + (ratio - 0.5) / 0.4 × 0.30
    elif ratio <= 1.4:
        retained = 0.35 + (ratio - 0.9) / 0.5 × 0.50
    elif ratio <= 2.0:
        retained = 0.85 + (ratio - 1.4) / 0.6 × 0.15
    else:
        retained = 1
```

🟨 Each sampled shot varies penetration uniformly from 85% to 115% of the
weapon's nominal value.

## Trace probability

🟩 The final game resolves physical shot traces. A selected target is an
intention; geometry determines what the trace contacts.

### 🟨 Rules-lab surrogate

The fixed-state lab has no full grid trace, so it uses:

```text
trace_probability =
    clamp(0, 1, accuracy × exp(-dispersion_per_meter × range))
```

Each sampled shot first tests this probability. If the target has cover, the
sample then chooses the exposed or covered path using target exposure.

🟥 This equation does not model miss direction, another unit being hit,
destructible geometry, firing lanes, projectile travel, or friendly fire.

## Damage

### 🟨 Expected damage per shot

```text
expected_damage_per_shot =
    trace_probability
    × nominal_damage
    × effect_density
    × layer_factor
```

```text
expected_damage_per_second =
    expected_damage_per_shot × shots_per_second
```

`effect_density = 1` for prototype point weapons and `0.12` for the prototype
support weapon. Sampled shot damage varies uniformly from 85% to 115% of
nominal.

🟥 Expected DPS is a diagnostic, not the final action model. It omits
engagement setup, reloads, ammunition, interruptions, wounds, target movement,
and discrete recovery.

## HP

**HP**, or Health Points, represents immediate ability to remain functional. It
is not detailed anatomy and does not erase lasting injury.

For casters, current HP additionally serves as:

1. survivability;
2. currency voluntarily spent on spell empowerment; and
3. the threshold compared against accumulated Strain.

Reaching zero HP normally causes incapacitation, not unconditional immediate
death.

## Wounds

**Wounds** are discrete lasting conditions caused by consequential damage.
They may affect:

- movement;
- perception and acquisition;
- weapon handling;
- action and reaction timing;
- bleeding;
- communication;
- maximum or recoverable HP; and
- eligibility for actions.

🟧 Wound thresholds and exact effects remain open.

## Incapacitation and death

🟩 At zero HP a unit normally becomes incapacitated and may deteriorate,
stabilize, be carried, evacuate, be captured, be executed, or die later. Severe
damage, overkill, explicitly lethal effects, catastrophic magic, bleeding, or
execution may bypass or end that state.

Battlefield treatment arrests deterioration or restores limited function. It
does not normally return an incapacitated human to combat.

## Suppression

**Suppression** is an accumulating state separate from HP damage. It can arise
from nearby traces, held areas, cover impacts, explosions, casualties, and
explicit effects.

It may degrade acquisition, reaction, accuracy, preparation, movement,
communication, willingness to leave cover, and interruption resistance.

### 🟨 Rules-lab suppression equation

```text
exposure_factor = 0.6 + 0.4 × clamp(0, 1, exposure)

suppression =
    suppression_per_second
    × max(0, active_seconds)
    × exposure_factor
    ÷ max(0.1, target_suppression_resistance)

reported_suppression =
    min(suppression, 2 × suppression_threshold)
```

The current threshold is 50 and the reporting cap is 100.

🟥 Suppression currently does not feed back during the sampled firing window.
The engagement equation can read pre-existing suppression, but gained
suppression is applied only as a reported result.

## Regeneration

**Regeneration** restores a regenerating body's effective HP during a combat
window.

### 🟨 Rules-lab equation

```text
active_seconds = max(0, window_seconds - engagement_seconds)
regenerated_hp = regeneration_per_second × active_seconds
final_damage   = max(0, sampled_damage - regenerated_hp)
```

Analytic expected time to incapacitation:

```text
net_dps = expected_damage_per_second - regeneration_per_second

if net_dps <= 0:
    expected_time = infinity
else:
    expected_time =
        engagement_seconds + target_max_hp / net_dps
```

🟥 This continuous subtraction can turn intermittent damage into absolute
immunity and does not preserve the sequence of hits and healing. Discrete
regeneration timing is the next required refinement.

## Deterministic randomness

🟩 Authoritative random samples must be addressed by stable facts:

```text
match random context
+ tick
+ action identifier
+ projectile or effect index
+ sample purpose
```

Execution order, parallel scheduling, hash-map iteration, and unrelated random
events cannot shift another action's sample.

The rules lab uses a seeded SplitMix64 generator. Its current sampling purposes
are trace contact, exposed-versus-covered path, penetration variation, and
damage variation.

## Formula register: accepted shape, values open

| System | Settled relationship | Missing formula or values |
|---|---|---|
| Movement readiness | Faster recent movement reduces readiness; readiness returns approaching destination | Credit-to-speed and readiness curve |
| Acquisition | Sensor, signature, exposure, attention, environment, and status accumulate toward a threshold | Combination, decay, and modality values |
| Reaction | Trigger must be observed; reaction is a timed action | Delay values and precedence states |
| Cover | Exposure delays engagement; physical material separately affects trace | Sample geometry, materials, integrity, destruction |
| Armor | Direction and penetration produce four qualitative outcomes | Final damage, integrity, degradation, repair |
| Wounds | Consequential damage creates few legible persistent conditions | Thresholds, selection, bleeding |
| Suppression | Accumulates separately and decays after pressure | Gain, decay, thresholds, feedback |
| Regeneration | Heavy arcane bodies may recover during a match | Timing, rates, limits, suppression and counters |
| Medical | Aid and stabilization are timed, interruptible, and supplied | Durations, charges, deterioration |
| Command network | Weakest path link bounds delivery; shared net saturation collapses | Throughput, attenuation, saturation curve |
| Anchor overload | More load raises instability; load shedding permits recovery | Capacity, load, warning, failure probabilities |
| Magic | Casting and empowerment change HP and Strain; breach occurs above HP | Casting check, strain gain, discharge, severity |

## Sources and executable definitions

- [Combat Resolution Architecture](combat-resolution.md)
- [Game Vision](game-vision.md)
- [Casualty and Medical Architecture](casualty-and-medical-architecture.md)
- [Fixed-State Rules Laboratory](research/rules-lab-prototype.md)
- [Rules-lab domain](../spikes/rules-lab/Domain.fs)
- [Rules-lab combat formulas](../spikes/rules-lab/Combat.fs)
- [Rules-lab catalog](../spikes/rules-lab/Catalog.fs)
