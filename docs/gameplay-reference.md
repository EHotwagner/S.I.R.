---
title: S.I.R. Gameplay Reference
status: proposed
document-type: reference-index
category: Reference
categoryindex: 5
index: 1
version: "0.3"
last-updated: 2026-07-28
related:
  - docs/game-vision.md
  - docs/human-forces.md
  - docs/arcane-forces.md
  - docs/combat-resolution.md
---

# S.I.R. Gameplay Reference

## Purpose

This is the main index for the self-contained S.I.R. gameplay corpus. The
linked pages collect the current units, classes, perks, weapons, equipment,
spells, command rules, gameplay formulas, test settings, and measured results
into one readable reference. A reader should not need to reconstruct the game
from the architecture and research history before understanding how its systems
fit together.

The corpus distinguishes accepted rule shape from provisional balance values.
That distinction is part of the information, not a footnote.

## Color legend

Color is always paired with a text label so the corpus remains understandable
without color vision or emoji rendering.

| Marker | Meaning |
|---|---|
| 🟦 **Human** | Human faction content or capability |
| 🟪 **Arcane** | Organized arcane-faction content or capability |
| ⬜ **Shared** | Rules common to every faction |
| 🟩 **Canonical** | Accepted gameplay rule or measured evidence |
| 🟨 **Prototype** | Executable but non-canonical formula, value, or body profile |
| 🟧 **Proposal** | Designed direction awaiting acceptance |
| 🟥 **Risk** | Known failure boundary, exploit, or result requiring more testing |

## Corpus index

| Page | What it contains |
|---|---|
| [Units, Classes, and Progression](gameplay-units.md) | Human squad structure, six classes, perks, leadership, arcane force shape, goblins, orcs, trolls, casters, drones, and progression |
| [Weapons and Equipment](gameplay-weapons-equipment.md) | Weapon roles, prototype weapon statistics, armor packages, sensors, communications, EW, engineering, medical, sustainment, and proposed arcane equipment |
| [Combat and Gameplay Formulas](gameplay-formulas.md) | Authoritative rule order, HP, Armor, Cover, Suppression, engagement, damage, regeneration, and every executable rules-lab equation |
| [Magic, Anchors, Rituals, and Portals](gameplay-magic.md) | HP empowerment, Strain, meditation, breaches, anchor capacity, overload, spells, rituals, portals, and borrowed critter observations |
| [Command, Information, and Mission Rules](gameplay-command-information.md) | Tick lifecycle, perception, acquisition, communications, electronic warfare, logistics, casualties, reinforcement, and knowledge limits |
| [Testing and Balance Evidence](gameplay-testing.md) | Testing setting, work completed before the rules lab, complete exposition of the current run, results, invariant checks, limitations, and reproduction |

## Fast reading paths

### “What can I field?”

Start with [Units, Classes, and Progression](gameplay-units.md), then use
[Weapons and Equipment](gameplay-weapons-equipment.md) for loadouts.

### “How is an attack resolved?”

Read [Combat and Gameplay Formulas](gameplay-formulas.md#attack-resolution-order),
then [Engagement](gameplay-formulas.md#engagement), [Cover and Exposure](gameplay-formulas.md#cover-and-exposure),
[Armor](gameplay-formulas.md#armor), and [HP](gameplay-formulas.md#hp).

### “How does the arcane faction work?”

Read the arcane section of [Units](gameplay-units.md#organized-arcane-force),
then [Magic, Anchors, Rituals, and Portals](gameplay-magic.md).

### “Which numbers are real?”

Read [Rule and value status](#rule-and-value-status), then
[Testing and Balance Evidence](gameplay-testing.md). Current rules-lab numbers
are 🟨 **Prototype**, while its executed results are 🟩 **Measured evidence**
about those prototype inputs.

## Linked keyword index

The first meaningful use of these terms on every corpus page links back to its
main definition.

| Keyword | Main definition |
|---|---|
| **Acquisition** | [Acquisition](gameplay-command-information.md#acquisition) |
| **Anchor Capacity** | [Anchor Capacity](gameplay-magic.md#anchor-capacity) |
| **Armor / Armour** | [Armor](gameplay-formulas.md#armor) |
| **Breach** | [Breach](gameplay-magic.md#breach-threshold-and-resolution) |
| **Command Bandwidth** | [Command Bandwidth](gameplay-command-information.md#command-bandwidth) |
| **Cover** | [Cover and Exposure](gameplay-formulas.md#cover-and-exposure) |
| **Engagement** | [Engagement](gameplay-formulas.md#engagement) |
| **HP / Health Points** | [HP](gameplay-formulas.md#hp) |
| **Incapacitation** | [Incapacitation and Death](gameplay-formulas.md#incapacitation-and-death) |
| **Perception** | [Perception Pipeline](gameplay-command-information.md#perception-pipeline) |
| **Regeneration** | [Regeneration](gameplay-formulas.md#regeneration) |
| **Ritual** | [Rituals](gameplay-magic.md#rituals) |
| **Strain** | [Strain](gameplay-magic.md#strain) |
| **Suppression** | [Suppression](gameplay-formulas.md#suppression) |
| **Wound** | [Wounds](gameplay-formulas.md#wounds) |

## Rule and value status

### 🟩 Canonical rule shape

Canonical rules include:

- 20 authoritative ticks per second and 50 ms per tick;
- timed actions with preparation, commitment, resolution, and recovery;
- simultaneous resolution for actions completing on the same tick;
- sustained point or area engagements, at most one per unit;
- physical traces, physical cover, directional armor, HP, wounds,
  incapacitation, suppression, and friendly fire;
- six permanent human classes with automatic bounded-random progression;
- scarce senior arcane casters, two or three persistent magical assistants per
  senior caster, durable non-caster mass, finite anchor capacity, cooperative
  rituals, and risk-based magic;
- faction-limited knowledge, physical communications, and electronic emissions;
- local squad messages taking at least one tick, and every command-net leg
  taking 20 ticks symmetrically for orders, reports, observations,
  acknowledgements, status, and player-defined traffic;
- anchored arcane observations and status reaching their controlling caster on
  the next tick, while caster commands take a flat 20 ticks to reach anchored
  subordinates without relay chains; and
- battlefield treatment preserving casualties without returning them to combat.

### 🟨 Prototype values and equations

The fixed-state rules laboratory currently supplies candidate:

- weapon curves and weapon statistics;
- goblin, orc, and troll HP, armor, suppression resistance, and regeneration;
- exposure, trace, penetration, retained-effect, suppression, and expected-time
  formulas; and
- deterministic samples for selected fixed board states.

These values are useful because they can be run and swept. They are not yet
accepted game balance.

### 🟧 Proposals

The detailed caster-led horde hierarchy, arcane perk families, mundane arcane
officers, warband progression, and species-specific equipment remain proposals.
They are included because they explain the intended design space, and are
visibly marked wherever used.

### 🟥 Open or dangerous boundaries

Important unresolved areas include:

- troll regeneration becoming effective immunity rather than recoverable
  durability;
- support suppression reaching its reporting cap too quickly;
- exact wound, bleeding, stabilization, and death timing;
- final physical projectile and cover-destruction formulas;
- multi-attacker concentration;
- exact strain gain, casting checks, breach tables, and ritual timings;
- command-net range, capacity, queue, and saturation numbers; and
- movement, readiness, acquisition, and suppression feedback curves.

## Source-of-truth policy

This corpus is the readable gameplay reference. The existing canonical design
documents remain the decision history and architecture source. When a conflict
exists:

1. a canonical accepted document overrides a 🟨 or 🟧 corpus entry;
2. executable rules-lab source overrides a copied prototype value;
3. measured output must identify the exact inputs and run; and
4. the corpus must be updated rather than silently allowing both descriptions
   to persist.

Each page ends with source links so maintainers can trace a summarized rule back
to its full rationale.
