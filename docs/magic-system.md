---
title: S.I.R. Risk-Based Magic System
status: proposed
document-type: living-design
version: "0.6"
last-updated: 2026-07-27
related:
  - docs/game-vision.md
  - docs/setting-and-factions.md
references:
  - Frostgrave
  - Warcrow
---

# S.I.R. Risk-Based Magic System

## Purpose

This document defines the architectural foundation for spellcasting, health
empowerment, strain, breaches, and shattering. Exact formulas and content remain
open, but the risk relationship is established.

## Design intent

Magic should provide effects strong enough to create faction-level asymmetry
without becoming a predictable cooldown rotation or a separate mana-based
weapon system.

The caster decides how much immediate power and future instability to accept.
Enemy action can exploit that decision by damaging, suppressing, isolating, or
forcing additional action from a strained caster.

The design takes qualitative inspiration from:

- **Frostgrave**, where a caster can spend Health to improve a casting result
  and sufficiently failed casts cause damage; and
- **Warcrow**, where spell properties can be altered and magical use interacts
  with accumulating adverse state.

S.I.R.'s HP-versus-strain breach threshold and shattering outcomes are its own
system rather than a reproduction of either reference's dice rules.

## Established core rules

### Health

Spellcasters have current HP under the normal authoritative damage model.
Current HP serves three purposes in magic:

1. survivability;
2. a resource that can be deliberately spent to empower spells; and
3. the threshold against which accumulated strain is compared.

Using current rather than maximum HP ensures that wounds and empowerment alter
magical safety immediately.

### Casting failure

Spells are not automatically successful. Failed casting can cost the caster HP.
The exact casting check and failure-damage function remain open.

### Empowerment

Before resolution, a caster may spend HP to empower one or more declared spell
aspects. Candidate aspects include:

- effect strength;
- range;
- area or number of targets;
- duration;
- penetration or resistance difficulty;
- casting speed;
- precision;
- reliability;
- concealment or signature; and
- resistance to interruption or counter-magic.

The set is illustrative. Each spell declares which aspects can be empowered,
their costs, limits, and incompatibilities.

### Strain

Casting accumulates strain on the caster. Strain represents mounting magical
instability and persists beyond a single casting action.

The exact strain gain, reduction, persistence, and recovery rules remain open.
Spell definition, selected empowerment, casting result, environmental
conditions, injuries, equipment, and status effects are possible inputs.

### Strain recovery: meditation

Strain has **two components**, and only one of them comes back.

```text
accumulated strain = settling strain  +  residual strain
                     recovers by       persists for the
                     meditating        rest of the match
```

**Settling strain** is the immediate turbulence of recent casting. A caster who
stops, stills themselves, and concentrates sheds it over time.

**Residual strain** is what does not settle. Every cast leaves some, so a
caster's total floor rises across a match no matter how carefully they pace
themselves.

Both count toward the breach threshold. A caster carrying residual strain is
permanently closer to the edge than a fresh one, which is why a veteran of a
long engagement is more fragile than their health alone suggests.

#### What meditating costs

Meditation is an ordinary timed action and it is expensive in the currency that
matters most in a firefight:

- the caster **cannot cast**;
- the caster is **stationary**;
- the caster is **inattentive**, with degraded acquisition and reaction under
  the ordinary awareness rules; and
- damage, suppression, or displacement **interrupts it**, so meditating within
  reach of a fight is meditating badly.

#### Why this is better than no recovery at all

Without recovery, a caster is a countdown rather than a decision. Once spent
they are dead weight, and the only question was how fast to spend them.

With it, the faction acquires a **rhythm**. Strained casters are pulled back,
fresh ones step forward, and the screening mass exists partly to protect
whoever is currently stationary and unaware. Casters cycle rather than simply
depleting, and a commander who never rotates them is doing it wrong.

It also creates a specific, valuable target. **A meditating caster is the most
vulnerable high-value unit on the battlefield** — stationary, inattentive, and
irreplaceable. Finding one is a windfall, which gives a human force's
reconnaissance apparatus a concrete objective against this faction beyond
locating anchors.

#### Releasing strain is detectable

Shedding strain is not a quiet act. It produces a **magical signature**, which
is already a stimulus modality in the perception model, so a caster releasing
strain announces themselves to anything equipped to perceive it.

This is what gives a human magical-signature detector a purpose worth the
research: it is not a general magic-finder, it is an instrument that locates
casters at **precisely the moment they are stationary, inattentive, and
irreplaceable**.

It also makes the recovery decision genuinely risky rather than a safe pause. A
caster who withdraws to meditate has traded one exposure for another: they are
out of the fight, but they are lit up while they are.

**Concealment works differently from the human equivalent.** Radio emission is
attenuated by terrain, so a human hides a transmission by putting mass between
themselves and the listener. A strain signature is not, so a caster can only
hide it with **distance**. There is no wall to meditate behind.

#### Both factions are loudest when they recover

The symmetry is worth stating because neither faction was designed to produce
it.

A human squad regaining contact transmits a conspicuous reconnection burst — the
most legible event available to traffic analysis. An arcane caster recovering
from strain emits a signature that a suitably equipped opponent can locate.

**The moment either force restores itself is the moment it is most visible.**
That is a good property for a game about contested information, and it arrived
from two unrelated mechanics rather than being imposed on them.

Meditating below the breach threshold is a legitimate emergency measure, but a
slow one. It does not rescue a caster from a breach already due; it prevents the
next one.

### Breach threshold

Whenever accumulated strain exceeds current HP, the caster must make a breach
check. This condition is evaluated whenever either value changes, not only when
the caster performs a magical action.

```text
breach_excess = strain - current_hp

if breach_excess > 0:
    resolve breach check
```

Both the breach-check result and `breach_excess` contribute to severity. A
caster barely over the threshold faces less expected danger than one whose
strain greatly exceeds remaining HP.

Any authoritative reduction in current HP immediately reevaluates the
threshold. Enemy damage, self-inflicted costs, environmental damage, ongoing
conditions, or other HP loss can therefore force an immediate breach check
against strain the caster had already accumulated. The caster does not need to
be casting when this occurs.

This makes a strained caster a volatile battlefield asset. Protecting and
healing that caster preserves magical safety, while identifying and wounding
them is direct counterplay that may cause consequences beyond ordinary damage.

### Breach and shattering

Breach consequences range from bad outcomes to catastrophic shattering events.
The final table is not designed, but its outcome contract may affect:

- the caster;
- nearby units;
- terrain and cover;
- active spells;
- communications and sensors;
- portal stability;
- summoned or controlled entities;
- persistent injuries or transformations;
- the mission environment; and
- campaign state.

Catastrophic outcomes must be rare enough to preserve decision-making but
credible enough that high strain cannot be treated as a routine efficiency
calculation.

A resolved breach discharges some, but not necessarily all, of the caster's
accumulated strain. A breach is therefore a partial release rather than an
automatic reset to a safe state. The discharge amount or rule remains to be
designed and may depend on the breach result, severity, spell, caster, or
shattering outcome.

## The core feedback loop

```text
cast or empower
      ↓
gain effect now
      ↓
lose HP and/or gain strain
      ↓
lower safety margin
      ↓
enemy pressure or further casting
      ↓
breach risk
      ↓
backlash or shattering
```

Spending HP is especially consequential because it can simultaneously:

- improve the current spell;
- move the caster closer to incapacitation;
- increase `strain - current_hp`; and
- trigger or worsen a breach check.

## Resolution architecture

A spell request must be an atomic authoritative command containing at least:

- caster identifier;
- spell and version identifier;
- targets or target area;
- declared aspect modifications;
- HP committed to each aspect;
- any consumed items or external resources; and
- the local knowledge used to form the request.

The server validates:

- that the caster knows and can currently use the spell;
- target legality under local information;
- aspect and HP-spend limits;
- current HP and strain;
- range, line of sight, facing, and other spatial requirements;
- cast time and interruption state;
- resource and equipment prerequisites; and
- host-class and ruleset compatibility.

The exact ordering of HP payment, casting resolution, failure damage, strain
gain, ordinary incapacitation, and breach resolution must be specified before
implementation. Different orderings can produce materially different outcomes
near zero HP and the breach threshold.

## Information and control-module contract

A spellcaster's WASM instance requires machine-readable access to:

- current HP and strain;
- known spells and empowerable aspects;
- legal aspect ranges and costs;
- breach trigger and public severity rules;
- locally known targets and environmental modifiers;
- current cast, interruption, and cooldown state; and
- authoritative results and consequences after resolution.

The API should expose enough rule information for human and automated risk
decisions without exposing future random results or hidden world state.

The standard module needs explicit risk policies, such as:

- conservative strain ceiling;
- emergency HP-spend allowance;
- protected-caster behavior;
- willingness to risk a breach for mission-critical outcomes;
- friendly-proximity constraints; and
- evacuation or recovery behavior after high strain.

## Counterplay

Opponents can interact with magic through:

- damaging the caster to reduce the strain threshold and potentially trigger
  an immediate breach;
- forcing repeated casting;
- interrupting cast time;
- suppressing or displacing the caster;
- separating the caster from healing or protection;
- threatening objectives that demand empowerment;
- counter-magic or resistance where available;
- exploiting visible spell signatures; and
- disengaging until accumulated strain becomes tactically costly.

Counterplay should not require human factions to receive magic. Technology,
positioning, reconnaissance, timing, ranged fire, electronic effects,
specialized materials, or portal research can provide bounded responses.

## Healing interaction

Healing a caster can increase current HP and therefore restore breach margin.
That makes healing part of the magical economy even when the healer is
nonmagical.

The design must prevent a trivial infinite loop in which healing converts
ordinary supplies into unlimited safe spellcasting. Possible constraints
include persistent strain, healing limits, injury state, reduced maximum HP,
scarce treatment resources, treatment time, and diminishing recovery. No
specific constraint is selected yet.

## Nonmagical arcane-civilization units

The organized arcane civilization includes nonmagical goblins, orcs, trolls,
and potentially other peoples or creatures.

Their purposes include:

- protecting and screening strained casters;
- holding terrain without accumulating magical risk;
- applying conventional pressure that forces human expenditure;
- carrying armor, supplies, components, or magical infrastructure;
- providing resilient shock or line units; and
- ensuring the faction remains functional after caster loss.

Substantial armor and healing for some of these units is a provisional
direction. Their recovery must have clear rates, limits, damage interactions,
and counters so regeneration does not become indistinguishable from excessive
HP.

## Balance and failure modes

Avoid:

- a mathematically obvious safe strain ceiling used in every situation;
- HP empowerment so efficient that casters always spend to the same breakpoint;
- catastrophic results frequent enough to make planning irrelevant;
- catastrophes so rare that rational play ignores them;
- healing that erases both health and strain costs;
- hidden severity rules unavailable to player modules;
- effects that bypass fog of war or grid rules without explicit contracts;
- AI casters receiving better risk information than future playable versions;
  and
- nonmagical faction units existing only as disposable HP screens.

## The spell set

The spells themselves are content rather than architecture and live in
[Arcane Spells](arcane-spells.md). The set is deliberately utility-dominant,
because a caster who is artillery is a worse gun than a gun, with a small number
of expensive decisive options so that an unattended caster remains a mistake.

## Prototype questions

1. What base casting check creates useful uncertainty at 20 simulation ticks per
   second?
2. When is HP paid relative to casting success?
3. How much strain does a cast generate, and what modifies it?
4. What are the settling and residual proportions, how fast does meditation
   shed settling strain, and does the residual fraction rise as total strain
   does? *The recovery mechanism itself is settled: meditation, above.*
15. How far does a strain signature carry, does its strength scale with the
    amount being shed, and can a caster meditate slowly to stay quiet?
5. After a breach partially discharges strain, when is another breach check
   required if the caster remains above the threshold?
6. What inputs determine breach severity?
7. What is the smallest harmful breach outcome?
8. What qualifies as a shattering catastrophe?
9. Can a caster voluntarily vent, transfer, contain, or weaponize strain?
10. Can allies share or absorb breach consequences?
11. How visible are strain, empowerment, and breach risk to enemies?
12. How do armor, healing, regeneration, and temporary HP interact with the
    threshold?
13. Which spell aspects are universal, and which are spell-specific?
14. How is the amount of strain discharged by a breach determined?

## Sources

- [Frostgrave: Second Edition official quick-reference sheet](https://www.ospreypublishing.com/media/ep5c2oi2/frostgrave_qrs-1.pdf)
- [Warcrow official how-to and stress overview](https://warcrow.com/en/games/warcrow/how-to-play)
- [Warcrow official magic rules update](https://downloads.corvusbelli.com/warcrow/wargame/rules/rules-update-1.5.1-en.pdf)
