---
title: Magic, Anchors, and Rituals
status: proposed
document-type: reference
category: Battlefield Systems
categoryindex: 4
index: 18
version: "0.2"
last-updated: 2026-07-28
related:
  - docs/gameplay-reference.md
  - docs/magic-system.md
  - docs/arcane-forces.md
  - docs/arcane-spells.md
---

# Gameplay Magic, Anchors, Rituals, and Portals

## Summary

🟪 Arcane magic spends current [HP](gameplay-formulas.md#hp), accumulates
Strain, and risks breaches rather than drawing from a safe replenishing mana
bar. Arcane forces coordinate through finite geographical anchors instead of
radio. Individual spells emphasize information, movement, denial, and terrain;
cooperative rituals require multiple exposed casters and commit to geography.

Return to the [Gameplay Reference](gameplay-reference.md).

## Magical economy

### Health and empowerment

🟩 A caster's current HP simultaneously represents:

1. ability to remain functional;
2. currency that may be spent to empower a spell; and
3. the threshold against which accumulated Strain is tested.

Before resolution, a caster may spend HP on spell-declared aspects such as
strength, range, area, targets, duration, penetration, speed, precision,
reliability, concealment, or interruption resistance. Every spell defines its
legal aspects, limits, costs, and incompatible combinations.

Exact aspect prices remain open.

### Casting failure

🟩 Spells are not automatically successful. A failed cast can cost the caster
HP. The casting check and failure-damage formula remain open.

## Strain

**Strain** is persistent magical instability accumulated through casting.

```text
accumulated strain = settling strain + residual strain
```

- **Settling Strain** can be shed through meditation.
- **Residual Strain** persists for the rest of the match.
- Both count against the breach threshold.
- Every cast leaves some residual component, so a caster's safe floor rises
  across the match.

🟩 This produces a strain career rather than a repeating cooldown. Casters can
surge, withdraw, recover partway, and return, but cannot reset completely.

### Meditation

Meditation is an ordinary timed action:

- the caster cannot cast;
- the caster is stationary;
- the caster is inattentive and has degraded
  [Acquisition](gameplay-command-information.md#acquisition) and reaction;
- damage, displacement, or Suppression can interrupt it; and
- released Strain produces a magical signature that terrain does not attenuate.

The magical-signature detector can therefore locate a caster precisely when
they are stationary and vulnerable. Distance, rather than a wall, is the
caster's protection while meditating.

Meditating below the breach threshold can prevent a future breach. It does not
cancel a breach already due.

## Breach threshold and resolution

🟩 The canonical threshold is:

```text
breach_excess = accumulated_strain - current_hp

if breach_excess > 0:
    resolve breach check
```

The condition is reevaluated whenever either HP or Strain changes. Enemy damage,
self-inflicted empowerment, environment, or ongoing effects can therefore
trigger a breach even when the caster is not currently casting.

Breach severity depends on both:

- the breach-check result; and
- positive `breach_excess`.

Consequences may affect the caster, nearby units, terrain, cover, active spells,
communications, sensors, portal stability, injuries, transformations, and
campaign state.

A breach discharges some Strain but does not necessarily reset the caster to
safety. The check, severity, result table, and discharge formula remain open.

## Magical feedback loop

```text
cast or empower
      ↓
gain effect now
      ↓
lose HP and/or gain Strain
      ↓
reduce remaining safety margin
      ↓
enemy pressure or further casting
      ↓
breach risk
      ↓
backlash or shattering
```

Healing can restore HP and breach margin, but may not erase residual Strain.
The final rules must prevent ordinary healing supplies from becoming unlimited
safe spellcasting.

## Anchor Capacity

**Anchor Capacity** is the finite structural capacity used to hold an arcane
force in supernatural coordination. It is distinct from caster Strain.

Anchors may be ritual sites, bound standards, inscribed objects, or sustained
caster-linked structures. Each has a position and geographical influence.

Within an anchor's influence, formations coordinate without transmitting.
Outside it, they act on local judgment and prior intent.

### Capacity and load

🟩 Every coordinated formation imposes load. Supporting more force requires
additional or stronger anchors, paid through:

- prepared components;
- scarce caster effort;
- defended ground; and
- a larger physical and magical footprint.

The exact load and capacity functions remain open.

### Overload

```text
more anchored force
        ↓
load exceeds capacity
        ↓
visible and magically detectable instability
        ↓
coordination disruption
        ↓
indiscriminate lightning or uncontrolled daemon breach
```

🟩 Instability must be telegraphed early enough to respond. The specific failure
is uncertain, but the increasing risk is observable.

An overloaded anchor may:

- disrupt every formation depending on it;
- discharge damaging lightning without respecting allegiance; or
- open an uncontrolled daemon portal hostile to every side.

Failures center on the anchor and must endanger its own force strongly enough
that deliberate overload is not reliable artillery.

Detaching, silencing, or withdrawing formations sheds load and can permit
recovery.

## Rituals

A **Ritual** is not a long individual spell. It is a site-bound cooperative
action requiring a hard caster quorum.

Every senior caster normally brings two or three persistent magical assistants.
Assistants possess lesser spells and ritual abilities. They can prepare
components and circles and contribute to maintenance, stability, interruption
tolerance, or controlled shutdown.

Minor workings may use one senior caster with assistants, and a standard ritual
may use a complete caster cell. Major rituals and portals still require
multiple senior casters. Assistants cannot replace required senior casters,
remove the hard quorum, accelerate progress without bound, or erase HP, Strain,
component, anchor-load, and breach costs.

```text
prepare a site
        ↓
assemble the caster quorum
        ↓
lock a place or trigger from legitimate knowledge
        ↓
maintain cooperative progress
        ↓
resolve
```

🟩 Rules:

- one caster cannot replace the quorum merely by taking longer;
- participants cannot use ordinary spells while contributing;
- the ritual consumes components, time, exposure, and temporary Anchor
  Capacity;
- preparation produces growing magical evidence;
- the target is geographical and locks before completion;
- the ritual cannot track an unknown unit or use hidden server truth;
- killing, suppressing, or displacing participants can break quorum; and
- adding more casters cannot accelerate completion without bound.

Whether interrupted progress stalls, decays, or resumes remains open.

### Ritual shapes

| Shape | Contract | Counterplay |
|---|---|---|
| **Culmination** | Delayed effect at a committed location | Predict movement, deceive observers, leave, or interrupt quorum |
| **Prepared trap** | Finite magical trigger bound to a place | Detect evidence, remove, consume, deceive, or avoid |
| **Portal ritual** | Connection opened at an exposed prepared destination | Attack preparation, prepare the exit, leave, or break quorum |

## Portals

Different portals have different ownership contracts.

| Portal | Purpose | Ownership and resource treatment |
|---|---|---|
| **Transit portal** | Moves precommitted arcane personnel, equipment, and supply | Remains arcane; counts normally against roster, command, supply, stake, and anchor load |
| **Goblin portal** | Deliberately opens an incursion from unaffiliated goblin territory | Neutral ownership; hostile to every side; no arcane orders, reports, supply, stake, command, or anchor load after emergence |
| **Daemon breach** | Catastrophic anchor-overload failure | Uncontrolled and hostile to everyone |

A transit portal transports; it does not create people. A goblin portal also
does not create personnel: it opens a path for independent goblins whose output
must be bounded. A daemon breach is not a chosen ritual.

## Spell catalog

The initial individual spell set is utility-dominant. Cost labels are
qualitative and canonical in ordering, not numeric.

| Spell | HP cost | Strain | Cast time | Expected frequency | Capability |
|---|---|---|---|---|---|
| **Obscuration** | Low | Low | Short | Often | Creates fog, dust, or darkness blocking optical sight for everyone |
| **Mending** | Low | Low | Short | Often | Accelerates existing non-caster Regeneration; does not create healing |
| **Barrier** | Moderate | Moderate | Moderate | Several | Creates a destructible semantic edge |
| **Scrying** | Moderate | Moderate | Long | A few | Returns one remote snapshot of presence and disposition, not identification |
| **Translocation** | Moderate | High | Long | Rarely | Moves over a short tactical bound to an observed, unoccupied destination |
| **Dampening field** | High | High | Long | Once or twice | Stationary area disabling electronic sensing and communication, not ordinary vision |
| **Rupture** | High | Very high | Long | Once decisively | Destroys cover, collapses structures, and opens routes |

### Barrier forms

| Form | Movement | Sight | Physical traces |
|---|---|---|---|
| **Wall** | Blocked | Blocked | Blocked |
| **Screen** | Passable | Blocked | Passable |
| **Ward** | Passable | Clear | Blocked |

Barriers are finite and destructible through ordinary terrain rules.

### Dampening field

A dampening field attacks electronic capability only. Human eyes and arcane
senses still work inside it. Because the field is stationary and visible
through its effects, placing one also announces that the area matters.

### Obscuration

Obscuration changes optical geometry for both sides. It is the ordinary arcane
answer to crossing open ground against superior human ranged fire.

### Rupture

Rupture targets terrain rather than a person. Casualties can result from the
collapse. Its high HP, Strain, and cast-time cost makes it a match-level
commitment.

### Translocation

The destination must have been observed, must remain valid, and is checked at
resolution. Preparation is slow and observable; failure can injure the caster.

### Scrying

Scrying is a snapshot rather than a feed. It supplies broad presence,
disposition, and rough location, not equipment, exact identity, intent, or
current tracking.

### Mending

Mending accelerates a body's existing Regeneration. It cannot turn a
non-regenerating body into a regenerating one.

## Borrowed critter observations

Harmless ambient animals are neutral units with physical positions,
species-appropriate senses, autonomous movement, and no combat role.

A caster may attune one and receive only the observations that critter
legitimately acquires:

```text
critter observes through ordinary perception
        ↓
attunement transports the earned facts
        ↓
arcane knowledge receives an unreliable remote observation
        ↓
ritual or portal may use that location lawfully
```

The acquired observation reaches the attuned caster on the next authoritative
tick. It pays no distance or relay delay while the attunement and supporting
anchor relationship remain valid.

Attunement does not steer the critter, improve its senses, make it arcane-owned,
or expose hidden state. Facts age normally. Active attunement leaves evidence,
so humans can detect, trap, capture, repel, exclude, kill, or deliberately
deceive the observer.

## Explicit exclusions

The initial arcane spell set excludes:

- **mind control**, because it would override unit ownership and control;
- **resurrection and reanimation**, reserved for a possible undead faction;
- **true summoning**, because portals move or admit existing beings; and
- **counter-magic**, deferred until another magical faction requires it.

## Open magic data

🟥 Still unresolved:

- casting checks and failure damage;
- HP empowerment exchange rates;
- per-spell empowerable aspects;
- Strain gain and settling/residual proportions;
- meditation rate and signature range;
- breach probability, severity, discharge, and outcome tables;
- healing limits;
- anchor capacity, formation load, warning, and failure probabilities;
- ritual quorum, time, contribution, interruption, and recovery;
- culmination effects and trap triggers;
- transit-portal capacity and wave cadence;
- goblin-portal output and behavior; and
- exact spell range, area, duration, and cost.

## Sources and deeper rationale

- [Risk-Based Magic System](magic-system.md)
- [Arcane Civilization Forces](arcane-forces.md)
- [Arcane Spells](arcane-spells.md)
- [Combat and Gameplay Formulas](gameplay-formulas.md)
