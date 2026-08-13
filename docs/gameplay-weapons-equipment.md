---
title: Weapons and Equipment
status: proposed
document-type: reference
category: Forces & Equipment
categoryindex: 3
index: 1
version: "0.1"
last-updated: 2026-08-13
related:
  - docs/gameplay-reference.md
  - docs/human-forces.md
  - docs/arcane-spells.md
  - docs/research/arcane-horde-progression-and-equipment.md
---

# Gameplay Weapons and Equipment

## Summary

🟦 Human equipment is a horizontal capability layer: weapons differ by
engagement shape, and nonlethal equipment trades capability against weight,
power, signature, preparation, and supply. 🟪 Arcane equipment is culturally
and magically integrated rather than industrially modular; its detailed catalog
is still proposed. Numerical weapon profiles on this page are the current
🟨 fixed-state rules-lab inputs.

Return to the [Gameplay Reference](gameplay-reference.md).

## Equipment rule

Equipment grants a physical capability. Class proficiency and perks change how
well and how flexibly a unit employs it. Equipment is not locked exclusively to
a class.

No item should be a numbered vertical upgrade that is simply stronger. A new
item should instead change one or more of:

- [Engagement](gameplay-formulas.md#engagement) curve;
- point or area target;
- range and dispersion;
- penetration and [Armor](gameplay-formulas.md#armor) interaction;
- [Suppression](gameplay-formulas.md#suppression);
- weight and readiness;
- ammunition or power demand;
- preparation and recovery;
- electronic or magical signature; or
- dependency on another unit, device, or resource.

## 🟦 Human weapons

### 🟩 Canonical weapon roles

| Weapon | Engagement shape | Target | Tactical role |
|---|---|---|---|
| **Carbine** | Short base, moderate rise | Point | Close and mid-range assault default |
| **Rifle** | Moderate base, shallow rise | Point | General-purpose baseline |
| **Shotgun** | Very short base, steep degradation | Point | Interior and doorway dominance; poor across open ground |
| **Marksman rifle** | Long base, nearly flat | Point | Punishes sustained exposure at any range; weak against peeking |
| **Support weapon** | Prepared and slow to redirect | Area | Denies ground, suppresses, and covers movement |
| **Grenade launcher** | Slow projectile | Area | Reaches behind or over cover |
| **Anti-armor launcher** | Long base, slow projectile | Point | Vehicles, hardened positions, and heavy creatures |

A unit may maintain at most one engagement. Point weapons maintain a solution
against one unit. Area weapons maintain one zone; occupants entering that zone
do not require new per-target preparation.

Fast projectiles resolve as physical traces on the resolution tick. Slow
rockets, grenades, arrows, and comparable effects occupy authoritative state
across ticks and use swept paths.

The executable v1 profiles deliberately cover one point rifle, an area support
weapon, a penetrating anti-armor point attack, and a lobbed area attack. Their
parameters are typed integers exposed in every result. They establish delivery
semantics and evidence, not final balance; campaign inventory and complete
equipment tuning remain outside this slice.

### Weapon trade-offs

- A carbine prepares faster than a rifle at close range but degrades much more
  sharply.
- A rifle is the general-purpose midpoint rather than a prerequisite for a
  better weapon.
- A marksman rifle is excellent against committed exposure and loses to brief
  peeks that break the targeting solution.
- A shotgun's positional specialization makes its value depend on the expected
  terrain.
- A support weapon suppresses and denies an area but dilutes individual
  lethality, consumes ammunition continuously, cannot discriminate, and is slow
  to redirect.
- A launcher answers heavy protection but pays in preparation, projectile
  travel, ammunition, and vulnerability to interruption.

### Prototype weapon profiles

These values are mirrored in the browser laboratory's structured
[`RulesCatalog.fs`](https://github.com/EHotwagner/S.I.R./blob/main/src/SIR.Client/RulesCatalog.fs)
catalog. They are not canonical stats.

| Weapon | Kind | Base engage (s) | Range slope | Exponent | Accuracy | Dispersion/m | Damage | Penetration | Shots/s | Effect density | Suppression/s |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| Carbine | Point | 0.32 | 0.018 | 1.15 | 0.86 | 0.005 | 30 | 24 | 2.60 | 1.00 | 7 |
| Rifle | Point | 0.55 | 0.012 | 1.10 | 0.88 | 0.004 | 35 | 28 | 2.20 | 1.00 | 8 |
| Shotgun | Point | 0.18 | 0.040 | 1.25 | 0.82 | 0.018 | 52 | 16 | 1.25 | 1.00 | 12 |
| Marksman rifle | Point | 1.25 | 0.003 | 1.00 | 0.94 | 0.001 | 55 | 36 | 0.65 | 1.00 | 5 |
| Support weapon | Area | 0.72 | 0.006 | 1.00 | 0.78 | 0.006 | 24 | 24 | 7.00 | 0.12 | 42 |
| Grenade launcher | Area | 1.10 | 0.010 | 1.00 | 0.72 | 0.008 | 70 | 25 | 0.25 | 0.35 | 30 |
| Anti-armor launcher | Point | 1.50 | 0.006 | 1.00 | 0.76 | 0.003 | 120 | 85 | 0.30 | 1.00 | 18 |

`Effect density` is the current balance surrogate that dilutes area-fire damage
per occupant. It is not a final physical trace-density rule.

The current engagement-time results are:

| Weapon | 8 m | 20 m | 35 m | 50 m | 65 m |
|---|---:|---:|---:|---:|---:|
| Carbine | 0.52 s | 0.88 s | 1.39 s | 1.94 s | 2.51 s |
| Rifle | 0.67 s | 0.87 s | 1.15 s | 1.44 s | 1.73 s |
| Marksman rifle | 1.27 s | 1.31 s | 1.35 s | 1.40 s | 1.45 s |

🟨 The current carbine/rifle crossover is near 20 m. This is a useful candidate
relationship, not an accepted distance.

## 🟦 Human armor

[Armor](gameplay-formulas.md#armor) is directional. Human hard protection is
strongest across front and rear plates and weaker at the sides and limbs.

| Package | Coverage | Cost |
|---|---|---|
| **None or soft** | Fragmentation protection | Low weight; scout choice |
| **Plate carrier** | Front and rear plates with soft flanks | Moderate weight and capacity cost |
| **Heavy armor** | Plates plus limb and neck protection | High weight, capacity, and readiness cost |

Heavy armor is a commitment to receiving fire from the protected direction. It
does not erase flanking and competes with ammunition and devices for carrying
capacity.

## 🟦 Human equipment catalog

### Weapon packages

- suppressor;
- compact optic;
- magnified optic;
- thermal weapon sight;
- bipod or support mount;
- under-barrel launcher; and
- specialist ammunition.

### Communications

- personal set;
- command-net set;
- directional antenna;
- burst-transmission unit;
- deployable relay;
- relay drone; and
- physical data package for a courier.

Radio power determines both range and detectability. Directional equipment
concentrates that power along a bearing but must be pointed.

### Sensors

- compact optics;
- magnified observation optic;
- thermal imager;
- acoustic direction finder;
- magical-signature detector;
- trip sensor; and
- observation drone.

The magical-signature detector is narrow rather than universal. Its canonical
uses are evidence from unstable anchors, ritual preparation, and active critter
attunement. Detecting Strain recovery depends on the unresolved recovery rule.

### Electronic warfare

- configurable jammer;
- radio direction finder; and
- decoy emitter.

Electronic warfare attacks emissions and electronic links. It does nothing to
the arcane faction's non-transmitting anchor coordination.

### Medical

- individual aid kit;
- stabilization kit;
- nanomedical stock;
- diagnostic sensor; and
- casualty harness or folding litter.

These items support aid, stabilization, carrying, and evacuation. Battlefield
treatment does not normally return an incapacitated unit to combat.

### Engineering

- breaching charge;
- cutting tool;
- demolition charge;
- remote initiator and wire;
- deployable cover;
- lightweight obstacle;
- mine or directional defensive charge;
- sensor stake;
- critter trap, cage, repellent, and exclusion equipment; and
- ritual-site disruption tools.

### Sustainment

- ammunition by weapon class;
- batteries;
- drone parts;
- engineering consumables;
- medical stock; and
- relay components.

The human faction's capability is information-rich and logistics-dependent.
Carrying every device should make a squad heavy, slow, power-hungry, and
conspicuous rather than universally superior.

## 🟪 Arcane spells as capabilities

Spells are fully described on
[Magic, Anchors, Rituals, and Portals](gameplay-magic.md#spell-catalog). Their
equipment relationship is:

- a focus may change which spell aspects can be empowered;
- components and prepared sites enable particular magical actions;
- a scrying vessel, ritual kit, portal frame, bound standard, or anchor stone
  supplies physical prerequisites; and
- no item may erase [Strain](gameplay-magic.md#strain), breach risk, ritual
  quorum, or legitimate observation requirements.

## 🟪 🟧 Proposed arcane equipment

### Caster equipment

- staff, wand, blade, or another casting focus;
- grimoire, tablets, knots, or inscribed memory aids;
- component satchel;
- ward tokens and boundary stakes;
- scrying vessel;
- ritual-circle kit;
- prepared portal frame;
- bound standard or anchor stone;
- ceremonial armor; and
- healing or strain-management implements.

### Goblin equipment

- shortbows, slings, knives, and light spears;
- nets, hooks, climbing gear, and ropes;
- smoke, irritant, fire, or obscuring pots;
- traps and local alarms;
- critter cages and feed;
- component panniers; and
- scavenged human equipment with proficiency and supply limitations.

### Orc equipment

- shields, spears, polearms, axes, cleavers, and heavy bows;
- lamellar, mail, hides, and reinforced helmets;
- pavises and portable barriers;
- breaching tools;
- standards; and
- ritual-site defense equipment.

### Troll equipment

- slab or harness armor;
- massive tools and impact weapons;
- stone or incendiary throwing loads;
- cargo and casualty harnesses;
- portable anchor or portal components; and
- protective fittings for a bonded handler or caster.

These lists explain intended capability space but remain 🟧 proposals. Mundane
arrows, food, and armor may still be supplies; they should not replace caster
Strain, magical components, and
[Anchor Capacity](gameplay-magic.md#anchor-capacity) as the faction's defining
match clocks.

## Open equipment data

🟥 The following data does not yet exist as accepted values:

- weights, carrying slots, and readiness penalties;
- ammunition package sizes and reload timings;
- battery capacities and drain;
- emission power and signal attenuation;
- equipment point or supply costs;
- armor integrity and degradation;
- final projectile dispersion, penetration, and damage distributions;
- support-weapon zone dimensions and trace density;
- slow-projectile velocities and swept-path interactions; and
- initial versus research-unlocked catalog membership.

## Sources and deeper rationale

- [Human Forces](human-forces.md)
- [Combat Resolution Architecture](combat-resolution.md)
- [Electronic Warfare Architecture](electronic-warfare.md)
- [Arcane Spells](arcane-spells.md)
- [Arcane Horde Progression and Equipment Proposal](research/arcane-horde-progression-and-equipment.md)
- [Rules-lab catalog](../spikes/rules-lab/Catalog.fs)
