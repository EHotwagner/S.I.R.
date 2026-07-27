---
title: S.I.R. Human Forces
status: proposed
document-type: living-content
version: "0.2"
last-updated: 2026-07-27
related:
  - docs/setting-and-factions.md
  - docs/combat-resolution.md
  - docs/communications-network.md
  - docs/electronic-warfare.md
  - docs/casualty-and-medical-architecture.md
---

# S.I.R. Human Forces

## Purpose

The first content document. Twenty-two architecture documents describe how
systems behave and none of them says what is in the game.

This defines the player's own force: what a squad contains, what its people are,
what they carry, and what each of those choices costs. Everything else is
measured against it — the arcane civilization exists as an asymmetric answer to
it, missions are built for it, and the point catalog will price it.

## How to read this

**This is content, not architecture.** It is versioned ruleset data that extends
the capability descriptor set; it does not change any contract. Numbers here are
indicative shape rather than balance, and every one of them is a prototype
parameter until something is running.

What is *not* provisional is the shape: which roles exist, what distinguishes
them, and what stops each from dominating. Those are design decisions and they
are the point of the document.

## The squad

A standard infantry squad is **eight to ten persons**, organised as a squad
leader plus two teams. It requires filled squad leader, second-in-command, and
third-in-command assignments to deploy, with the latter two commanding the
teams.

At the intended force target this puts a side at roughly **six to twelve
squads**, which is the number a commander can hold in mind and assign postures
to without the interface becoming a spreadsheet.

## Classes are people; loadout is capability

A class is permanent for the campaign and establishes proficiency, progression,
and what a person is *good at*. Equipment is reversible and establishes what
they can currently *do*.

**This means there are fewer classes than roles.** A rifleman carrying a support
weapon is performing the gunner's role and doing it worse than a gunner would.
The class does not forbid the loadout; it prices it in effectiveness.

| Class | Core competence |
|---|---|
| **Rifleman** | The baseline. Broad competence, no specialisation, the unit every comparison is measured against. |
| **Gunner** | Sustained area fire. Proficiency with support weapons and the fire discipline they require. |
| **Marksman** | Precision at range. Proficiency with slow-building, range-indifferent weapons and with observation. |
| **Engineer** | Breaching, demolition, deployables, and prepared positions. |
| **Medic** | Casualty work. Faster and more reliable aid and stabilisation, and procedures others cannot perform. |
| **Signaller** | Communications, electronic warfare, and drones. Relays, direction finding, jamming, the command-net set, and the aerial platforms that carry all of it. |

Six classes, deliberately. Each maps to a system that would otherwise have no
one who is good at it, and none is a variation on another.

Command qualification is an attribute of a person rather than a seventh class.
Any class may hold a leadership assignment if qualified, which keeps succession
from being blocked by the death of the only person of the right type.

## Weapons

Weapons differentiate primarily by the **shape of their engagement-time curve**
and by whether they hold a point or an area, not by damage.

| Weapon | Curve | Target | Role |
|---|---|---|---|
| **Carbine** | short base, moderate rise | point | Close and mid work; the assault default |
| **Rifle** | moderate base, shallow rise | point | The general-purpose baseline |
| **Shotgun** | very short base, steep decay | point | Interior and doorway dominance; useless across ground |
| **Marksman rifle** | long base, nearly flat | point | Punishes exposure at any range; slow to bring to bear |
| **Support weapon** | — | **area** | Denies ground, suppresses, covers movement |
| **Grenade launcher** | — | area, slow projectile | Reaches over and behind cover |
| **Anti-armour launcher** | long base | point, slow projectile | Vehicles, hardened positions, and heavy creatures |

Slow projectiles occupy authoritative state across ticks and use swept paths, so
a launcher's round is a visible, dodgeable, interruptible object rather than an
instant result.

### Why none of these dominates

- the **marksman rifle** is the strongest weapon at range and the worst against
  anything that peeks, because a long build time gives a target every chance to
  break the solution under the sustained-targeting rule;
- the **shotgun** is close to unbeatable inside a building and worthless outside
  one, which makes it a decision about where you expect to fight;
- the **support weapon** denies ground better than anything else and kills any
  individual worse, because volume spread over a zone is less lethal than a
  maintained solution; and
- the **carbine and rifle** trade against each other across the ordinary
  engagement band rather than one being an upgrade.

## Armour

Human armour is **strongly directional**: hard plates front and rear, soft
coverage at the sides and limbs.

| Armour | Coverage | Cost |
|---|---|---|
| **None or soft** | Fragmentation only | Nothing; the scout's choice |
| **Carrier** | Front and rear plates, soft flanks | Moderate weight and carrying capacity |
| **Heavy** | Plates plus limb and neck protection | Substantial weight, capacity, and readiness |

Directionality is the point. Frontal protection is genuinely good, and it does
nothing at all against a flank or rear engagement. **Armour therefore makes
positioning more important rather than less**, which is the opposite of what
armour usually does in a tactical game.

Weight is not free: it competes with ammunition and equipment for carrying
capacity under the logistics model, and it degrades the movement-to-readiness
relationship, so an armoured unit is slower to become ready after moving. Heavy
armour is a commitment to being shot at from the front.

## Equipment is where human identity lives

The equipment list is longer than the weapon list, and most of it is not lethal:

| Category | Items |
|---|---|
| **Communications** | Personal set; command-net set; deployable relay |
| **Sensors** | Optics; thermal; acoustic direction finder |
| **Electronic warfare** | Jammer; direction finder; decoy emitter |
| **Drones** | Observation drone; relay drone |
| **Medical** | Aid kit; stabilisation kit; nanomedical stock |
| **Engineering** | Breaching charges; cutting tools; deployable cover |
| **Sustainment** | Ammunition by class; batteries; spare parts |

Notice what that list mostly is. **Human capability is information capability**,
and the setting document's claim that humans are "disciplined, information-rich,
logistics-dependent combined arms" is not an assertion to be honoured elsewhere
— it is this table.

### Drones are elevation you can move

Drones belong to the signaller because they are information capability with
rotors. Both of their uses — carrying a sensor and carrying a relay — are the
signaller's existing job performed from somewhere a person cannot stand.

A drone is a **unit, not a piece of equipment.** It has a footprint, a position,
its own perception, and its own control module instance, because the
architecture admits no other way for something to act. Several consequences
follow without needing rules of their own:

- it **counts against force size**, so fielding drones divides the same command
  bandwidth pool across more units and dilutes attention exactly as any other
  addition does;
- it **occupies height**, which the spatial model already makes valuable twice
  over — clear observation and clear signal paths. A drone is the one asset that
  can put both wherever they are needed;
- it is **the most network-dependent thing a human force fields**, since a relay
  drone transmits continuously and is therefore a beacon, while an observation
  drone is only useful if what it sees reaches somebody; and
- when jammed it does not fall out of the sky. Its module runs every tick like
  any other unit's, so it **becomes autonomous and unsupported** — still flying,
  still seeing, and no longer telling anyone.

That last property is the sharpest expression of the faction's central tension.
The drone is the clearest thing humans own, and it is the thing an opponent has
most reason to cut off rather than shoot down.

### Strength and vulnerability are the same property

The through-line worth stating explicitly:

```text
human capability is information capability
        ↓
information capability is electronic
        ↓
electronic capability emits, and consumes power
```

Every advantage humans have — the fused picture, direction finding, relayed
command, thermal observation, drones, coordinated fire — is carried by a device
that
**announces its position** and **runs out of battery**. The faction's strength
and its vulnerability are not two lists that must be balanced against each
other. They are one property seen from two sides.

That is why a human force is formidable while its network holds and awkward when
it does not, and it is what an opponent designed against them should attack.

## Checking against invariant 13

No category should have an unconditionally correct answer.

**Weapons** trade across range and against the sustained-targeting rule, so the
best weapon depends on where you expect to fight and whether the enemy will
stand still.

**Armour** trades protection against carrying capacity and readiness, and its
protection is directional, so it is a bet on being engaged from the front.

**Equipment** trades capability against weight, battery, and signature. A
maximally equipped squad is heavy, slow, and loud.

**Classes** trade specialisation against squad flexibility, since a squad of six
specialists has no depth when one dies and a squad of riflemen is good at
nothing in particular.

The category most at risk is **equipment**, because carrying more is locally
always better and the costs are diffuse. Carrying capacity, battery consumption,
and aggregate signature all need to bite hard enough to make loadout a real
decision, and that is the first thing to check once anything runs.

## What this tests in the architecture

Writing content is also how architecture gets validated, and this set is
deliberately chosen to exercise claims that are currently only asserted:

- whether **engagement-time curves** produce genuinely distinct weapon roles, or
  collapse into one good weapon and several worse ones;
- whether **area engagement** makes a support weapon feel different in kind
  rather than merely faster;
- whether **directional armour** makes flanking matter more, as intended, or
  simply makes frontal assaults unviable;
- whether the **two-layer cover model** produces readable outcomes when a real
  weapon meets a real wall; and
- whether **emission and battery costs** are enough to stop every squad carrying
  every device.

## Open parameters

- Every numeric value: engagement times, curve shapes, armour values,
  penetration, weights, capacities, battery life.
- Squad size, and whether specialists are organic to a squad or attached.
- Whether the six classes are the right six, and whether advanced classes
  branch from them.
- Ammunition package sizes and compatibility across weapon classes.
- Battery as a single resource or per-device-class stocks.
- Which sensors reveal which observation facts.
- Vehicle content, deliberately excluded here.
- Drone endurance, altitude bands, control range, and whether an observation
  drone can operate usefully while silent.
- Whether a drone occupies a declared altitude band or the topmost level.
- Nanomedical limits, which remain provisional in the setting document.
