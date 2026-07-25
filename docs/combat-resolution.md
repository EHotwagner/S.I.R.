---
title: S.I.R. Combat Resolution Architecture
status: proposed
document-type: living-design
version: "0.1"
last-updated: 2026-07-25
related:
  - docs/game-vision.md
  - docs/research/combat-awareness-models.md
  - docs/magic-system.md
---

# S.I.R. Combat Resolution Architecture

## Purpose

This document defines the canonical relationships between attacks, physical
delivery, cover, armor, HP, wounds, incapacitation, death, suppression,
friendly fire, and executions. Exact numerical formulas and content remain
prototype data.

## Design boundary

Combat should make position, preparation, information, facing, equipment, and
timing decisive without becoming a detailed physical or medical simulation.

The canonical pipeline is:

```text
attack action
  → projectile or effect path
  → contact with cover or unit
  → armor or resistance
  → HP and wound consequences
  → suppression and secondary effects
```

All stages execute under the simultaneous-completion and batched-consequence
rules defined by the game vision.

## Attack resolution

### Physical shot traces

At its resolution tick, a firearm produces one or more authoritative shot
traces. Each trace is derived from:

- weapon and ammunition;
- aim and preparation;
- range;
- attacker movement and stance;
- suppression, wounds, and other conditions;
- the acquired observation used to attack;
- target exposure;
- applicable abilities and equipment; and
- a deterministic random sample when the rule calls for variation.

The resulting path traverses grid space and contacts the first applicable
obstacle or unit. A selected target is an intention, not a guarantee that the
trace ignores intervening or nearby entities.

This permits:

- partial exposure;
- cover interception and penetration;
- friendly fire;
- misses striking another object or entity;
- crossfires and dangerous firing lanes; and
- suppression from nearby fire and impacts.

Fast projectiles such as ordinary bullets resolve as traces on the attack's
resolution tick. Slow and tactically observable projectiles such as rockets,
grenades, arrows, or applicable magical effects may occupy authoritative state
across multiple ticks and use swept paths.

The simulation does not model aerodynamic drag, detailed organs, millimetre-
scale materials, or similar detail unless a future rule demonstrates a
meaningful tactical need.

### Order-independent randomness

Random combat samples must be deterministic for the authoritative replay and
independent of execution order. A sample is addressed by stable facts such as:

```text
match random context
+ tick
+ action identifier
+ projectile or effect index
+ sample purpose
```

Parallel evaluation, unrelated random events, module scheduling, and unit
iteration order must not shift another action's samples. Distributions and
mechanical modifiers are public ruleset data. Future samples, secret random
context, and hidden world state are not exposed during a match.

## Cover and exposure

Cover is physical world geometry, not a percentage modifier stored on a target.
A shot or effect can:

- pass through an opening;
- strike and stop in cover;
- penetrate with reduced or changed effect;
- damage or destroy the cover; or
- continue into an entity behind it.

Target exposure is derived from the visible portions of its square footprint,
observation and target sample points, stance, attack direction, and intervening
geometry. Cover evaluation and line of sight use the same authoritative spatial
model so they cannot disagree about which geometry exists.

Cover destruction changes the spatial revision used by visibility, pathfinding,
and subsequent attacks. It invalidates only affected cached data rather than
changing earlier same-tick outcomes retroactively.

## Armor and resistance

Armor resolves after a trace or effect contacts a unit and before HP damage.
Inputs can include:

- impact direction;
- coverage or armor arc;
- attack damage type;
- penetration;
- armor protection and remaining integrity;
- stance or shield state; and
- explicit technological, biological, or magical modifiers.

The canonical qualitative outcomes are:

- **stopped**;
- **partially mitigated**;
- **penetrated**; and
- **overmatched**.

Directional armor can make facing important for vehicles, shields, trolls, and
other units without altering their square authoritative footprint. Exact armor
degradation, penetration variation, damage types, and repair rules remain
prototype parameters.

## HP, wounds, incapacitation, and death

### HP

Current HP represents a unit's immediate ability to remain functional. It is
also an explicit input to systems such as arcane strain and breach risk.

### Wounds

Wounds are discrete, lasting conditions produced by sufficiently consequential
damage or other effects. They can alter meaningful capabilities such as:

- movement;
- perception or acquisition;
- weapon handling;
- action and reaction timing;
- bleeding;
- communication;
- maximum or recoverable HP; and
- eligibility for particular actions.

Wounds should be few, legible, and tactically meaningful. The architecture does
not require detailed organs or a separate injury entry for every minor hit.

### Incapacitation and death

Reaching zero HP normally causes incapacitation, not unconditional immediate
death. An incapacitated unit can be subject to:

- bleeding or other deterioration;
- stabilization;
- limited battlefield recovery;
- evacuation;
- capture;
- execution; or
- death from later consequences.

Severe damage, overkill, explicitly lethal effects, bleeding, catastrophic
magic, or execution can cause death under their declared rules.

Battlefield treatment primarily restores limited function, arrests
deterioration, or creates an opportunity for evacuation. Full recovery from
lasting injuries primarily occurs between missions and consumes the applicable
campaign resources and time.

## Suppression

Suppression is an accumulating tactical state distinct from HP damage. Sources
can include:

- fire passing near a unit;
- impacts against nearby cover;
- explosions;
- observable casualties;
- heavy or sustained weapon effects; and
- explicit technological, biological, or magical effects.

Suppression can affect:

- acquisition and reaction time;
- accuracy and action preparation;
- movement behavior or speed;
- willingness or ability to leave cover;
- communication and coordination; and
- vulnerability to interruption.

Suppression decays under explicit rules when pressure ends. Training,
leadership, abilities, wounds, stance, equipment, environment, and faction
traits may modify its gain, effects, or recovery.

A shot can therefore have tactical value without causing HP damage. Exact
suppression radii, rates, thresholds, and behaviors remain prototype data.

## Friendly fire

Friendly units, civilians, allied participants, and protected entities receive
no implicit immunity from otherwise valid traces, projectiles, explosions, or
areas of effect.

The server does not silently redirect a dangerous attack. Clients and control
modules receive the locally legitimate geometry and fire-line information
needed to evaluate risk, but remain responsible for their decisions. Hidden
entities are not revealed merely to make a fire-line prediction safe.

## Rear attacks and executions

Rear position and target awareness are independent, cumulative factors.

- Rear position can exploit directional armor, cover orientation, weapon
  access, and turning time.
- An unaware target cannot begin a reaction to an attacker or attack stimulus
  before acquiring the relevant information.
- Combining rear position with lack of awareness creates the strongest ordinary
  attack opportunity.

An execution is a deliberate, highly lethal action with explicit eligibility.
Its requirements can include:

- suitable range;
- a vulnerable, unaware, restrained, or incapacitated target;
- compatible weapon and relative facing;
- a committed preparation period; and
- continued eligibility until its resolution tick.

Execution is not an automatic rear-attack multiplier. It can be prevented by an
earlier-resolving interruption, loss of eligibility, displacement, or another
declared counter. If the execution and an opposing action complete on the same
tick, both use the normal simultaneous-completion rule.

## Same-tick results

All attacks and effects completing on the same tick calculate their outcomes
before their consequences are applied. A shooter incapacitated by the batch can
still produce a shot that completed on that tick.

The consequence phase then applies HP changes, wounds, suppression, armor and
cover damage, incapacitation, death, magical breach triggers, and related
effects in a fixed public order. The exact handling of simultaneous healing,
damage-triggered thresholds, and recursive consequence chains still requires a
specific ordering contract.

## API and replay requirements

The authoritative record must be able to identify:

- action and trace identifiers;
- ruleset and content versions;
- source observations and locally known targeting inputs;
- public mechanical modifiers;
- contacted geometry or entity;
- armor and resistance outcome;
- HP, wound, suppression, and secondary consequences;
- random sample purpose and replay provenance without exposing future samples;
  and
- the tick and consequence stage in which each result occurred.

Clients and modules receive only the subset permitted by their knowledge.
Explainability must not become a side channel for hidden armor, concealed units,
unseen attacks, or secret random context.

## Prototype parameters

The following remain open:

- dispersion functions and aim progression;
- projectile count and burst abstraction;
- visibility and exposure sample points;
- cover protection, penetration, damage, and destruction;
- armor values, coverage, integrity, and degradation;
- damage types and resistance relationships;
- HP scales and wound thresholds;
- incapacitation, bleeding, stabilization, and death timing;
- suppression gain, effects, and decay;
- friendly-fire safety estimates exposed to modules;
- rear-position effects;
- execution eligibility and duration; and
- the precise ordering of simultaneous healing and damage-triggered
  consequences.
