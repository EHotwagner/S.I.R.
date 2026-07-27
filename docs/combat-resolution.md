---
title: S.I.R. Combat Resolution Architecture
status: proposed
document-type: living-design
version: "0.6"
last-updated: 2026-07-27
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

The tactical environment follows XCOM 2, but combat resolution does not.
Resolution follows the physical model represented by Xenonauts 2 and defined
here. A to-hit roll against a selected target, with cover as a defence modifier
stored on that target, is explicitly rejected: it would remove friendly fire,
crossfire, penetration, and misses striking other entities, and would reduce
semantic cell edges to a movement and sight concern with no ballistic role.

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

## Sustained targeting

An engagement is not a countdown that fires regardless of what happens during
it. From the moment an attack begins preparing until its resolution tick, the
attacker must **maintain the targeting solution it started with**.

```text
acquisition sufficient to engage
  → engagement begins preparing
  → targeting solution maintained each tick
  → resolution
```

If the solution is lost before resolution — the target breaks line of sight,
leaves effective range, is obscured, or the attacker's own acquisition decays
below the engaging threshold — the engagement does not resolve. Its declared
capability determines whether it cancels, suspends, degrades, or resolves
against the last valid solution at reduced effect.

This makes several behaviors mechanically real rather than cosmetic:

- exposing briefly and withdrawing defeats a slow engagement, so peeking is a
  genuine tactic with a genuine cost;
- breaking contact is an active defence rather than only a repositioning
  decision;
- smoke, obscurants, and concealment matter after first contact, not only
  before it;
- a closing door, a raised shutter, or a rebuilt obstruction can interrupt an
  engagement already in progress, giving semantic edges a role in combat timing
  and not only in initial line of sight; and
- a unit that suppresses an attacker enough to break its acquisition has
  prevented the shot without inflicting damage.

Maintenance is evaluated on simulation ticks against the same authoritative
geometry, edge state, and acquisition rules used everywhere else. It cannot
consult information the attacker is not entitled to.

Weapons and abilities declare their own sensitivity. A committed heavy weapon
may resolve against a lost solution at reduced effect, while a precise shot may
simply fail. The exact per-capability behavior remains prototype data.

## Engagement time profiles

Weapon classes differentiate primarily by the **shape of their engagement-time
curve over range**, not by damage alone. Two weapons with comparable lethality
can occupy entirely different tactical roles because one builds a solution
quickly at close range and the other builds slowly but indifferently to
distance.

```text
engagement time
      │
      │  precision ────────────────────  flat, long
      │
      │  general      ╱────────────      moderate, rising
      │            ╱
      │  close  ╱                        short, steeply rising
      │      ╱
      └──────────────────────────────── range
```

Candidate profiles:

- **precision weapons** build slowly but the cost barely rises with distance, so
  they punish standing exposed at any range and reward prepared positions with
  long observation;
- **general-purpose weapons** build moderately and degrade steadily with
  distance, remaining usable across the ordinary engagement band; and
- **close-range weapons** build almost immediately but degrade sharply, so they
  dominate interior space and doorways and are nearly useless across open
  ground.

This makes weapon selection a positional decision rather than a damage
comparison, and it interacts directly with sustained targeting: a slow-building
weapon gives its target more opportunity to break the solution, so the same
weapon is strong against a committed advance and weak against a target that
peeks.

Dispersion, penetration, and damage remain separate axes. A hard range wall at
which a weapon simply cannot fire is deliberately **not** adopted: S.I.R.
resolves physical traces, so effective range should emerge from dispersion
geometry rather than be declared twice.

Exact curve shapes and per-class values remain prototype data.

## Engagement targets and concurrency

A unit maintains **at most one engagement at a time**. That invariant is
universal. What varies is the *kind of target* an engagement can hold.

An engagement targets either:

- a **point target** — one specific unit; or
- an **area target** — a declared zone, sector, lane, or beaten zone.

### Point engagement

A point engagement holds a targeting solution against one unit. Beginning an
engagement against a second unit requires full preparation against that unit; a
unit does not accumulate simultaneous solutions against everything it can see.

This is a deliberate throttle on precision fire. Without it, a single
well-positioned marksman answers an entire squad and defensive positions become
disproportionately strong at the intended force scale. With it, approaching a
covered position from several directions at once is a legitimate response, and
local numerical advantage means something.

### Area engagement

Support weapons do not engage individuals. A machine gun, automatic grenade
launcher, or comparable weapon commits to an **area**, and everything inside
that area is subject to its traces and suppression for as long as the engagement
persists.

An area engagement is still one engagement. The gunner is not tracking `N`
targets; the gunner is holding one zone. Occupants entering or leaving it do not
require re-preparation, which is precisely what a support weapon is for.

This makes the support role mechanically distinct rather than a
higher-rate-of-fire rifle:

- covering a sector, guarding a doorway, and holding a lane become the same
  authoritative construct rather than separate special cases, and they are the
  prepared states the game vision already credits with reduced reaction delay;
- suppression gains its natural home, since a beaten zone suppresses whatever is
  in it without needing per-target acquisition;
- a support weapon can deny space it cannot see clearly, which precision fire
  cannot; and
- a squad's support element and its assault element are doing genuinely
  different things at the same time.

### What bounds an area engagement

Area engagement must not dominate point engagement. Its costs are:

- **ammunition**, consumed continuously and drawn from belt or box packages
  under the logistics model rather than per aimed shot;
- **diluted effect**, since volume spread across a zone is less lethal against
  any one occupant than a maintained precision solution;
- **no discrimination**, because friendly units, civilians, and protected
  entities in the zone receive no immunity from traces passing through it;
- **inertia**, as shifting, lifting, or traversing the zone costs time under the
  action lifecycle, so an area engagement is slow to redirect;
- **signature**, because sustained fire is a loud, bright, persistent stimulus
  that makes the firing position easy to locate; and
- **emplacement**, where weapons that require deployment pay setup and teardown
  time and are correspondingly hard to reposition.

### Maintenance

The sustained-targeting rule applies to both kinds, against their own target.

A point engagement is maintained against the unit. An area engagement is
maintained against the **area** — its observation, orientation, and firing
geometry — and is not broken merely because occupants move through it. Losing
observation of the zone itself, being displaced, or having the firing geometry
obstructed degrades or ends it under the same rules.

### Open

Whether a unit may hold a prepared area engagement while taking opportunistic
point shots, or whether the two are strictly exclusive, is unresolved. Strict
exclusivity is the simpler contract and the current assumption.

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
obstacle or unit, evaluating cell contents and crossed edge features in path
order. A selected target is an intention, not a guarantee that the trace ignores
intervening or nearby entities, walls, or closed openings.

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
It exists in both spatial layers: cell-occupying volumes and semantic edge
features on cell boundaries. A shot or effect can:

- pass through an opening, an open door, or a window edge;
- strike and stop in cover;
- penetrate with reduced or changed effect;
- damage or destroy the cover; or
- continue into an entity behind it.

A trace therefore contacts obstacles in path order across both layers. It tests
the ordered sequence of edges it crosses as well as the cells it enters, and an
edge feature can stop a trace between two otherwise open cells.

Edge features make several ordinary situations resolvable that a cell-only model
cannot express:

- firing through a window while the surrounding wall protects the shooter;
- fire passing freely over a handrail or low wall that still blocks movement;
- a closed door stopping a trace that an open door would not;
- partial cover granted by the specific boundary an attack crosses rather than
  by the target's cell; and
- a breach converting a blocking edge into a firing line.

Target exposure is derived from the visible portions of its square footprint,
observation and target sample points, stance, attack direction, and intervening
geometry in both layers. Cover evaluation and line of sight use the same
authoritative spatial model so they cannot disagree about which geometry exists
or which edges a path crosses.

### Cover extends engagement time

Cover acts twice, at two different stages, and the two roles must not be
confused with one another.

1. **Before resolution**, a partially covered target is harder to build a
   solution against. Less of its footprint is exposed, so the engagement takes
   longer to prepare.
2. **At resolution**, cover is geometry that a trace physically contacts,
   stopping it, mitigating it, or being penetrated.

The first role is new and is what makes managing partial exposure worthwhile.
Combined with sustained targeting, it produces the intended relationship:
a covered target buys time, and time is the resource it needs to break contact,
withdraw, or be reinforced.

**These are not two applications of the same benefit.** The pre-resolution
effect derives from *exposed footprint area* — how much of the target an
attacker can work with. The resolution effect derives from *what the trace
strikes on its path*. A target with a small exposed area behind a flimsy screen
is slow to engage but poorly protected once engaged; a target fully behind thick
masonry with a wide firing slot is quick to engage through the slot but well
protected against anything striking the wall. Implementations must derive each
from its own input rather than applying one cover value to both stages.

Cover destruction—including edge breaching, door destruction, and window
breaking—changes the spatial revision used by visibility, pathfinding, and
subsequent attacks. It invalidates only affected cached data rather than
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

See [Casualty and Medical Architecture](casualty-and-medical-architecture.md)
for the medical capability set, carrying and evacuation, and the rule that
battlefield treatment does not return a unit to the fight.

## Suppression

Suppression is an accumulating tactical state distinct from HP damage. Sources
can include:

- fire passing near a unit;
- occupying an area held by an active area engagement;
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
- contacted geometry or entity, identifying the specific cell or canonical edge;
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
- edge feature permeability values by modality, stance, and movement profile;
- edge integrity, breaching costs, and post-breach state;
- per-capability behavior when a targeting solution is lost mid-engagement;
- whether losing and regaining a solution resumes or restarts an engagement;
- engagement-time curve shapes and values by weapon class;
- how exposed footprint area converts into engagement-time cost, kept
  independent of the trace-interception effect;
- area-engagement zone shapes, minimum and maximum size, and how a zone is
  declared through the module ABI;
- trace density and distribution within an engaged area;
- the time cost of shifting, lifting, or traversing an engaged area;
- emplacement setup and teardown times by weapon class;
- whether a prepared area engagement permits opportunistic point shots;
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
