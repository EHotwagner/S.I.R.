---
title: Turn-Based Tactical Depth in Real-Time S.I.R.
status: accepted
decision-status: canonical-direction
document-type: research-and-options
version: "0.2"
last-updated: 2026-07-25
related:
  - docs/game-vision.md
  - docs/combat-resolution.md
  - docs/wasm-control-architecture.md
  - docs/simulation-core-architecture.md
---

# Turn-Based Tactical Depth in Real-Time S.I.R.

## Purpose

This catalog identifies tactical mechanics normally limited to turn-based or
pause-and-plan games that become practical in always-real-time S.I.R. because
per-unit WASM modules execute detailed decisions at 20 simulation ticks per
second.

The responsibility split and identity package in this document are accepted
design direction. Individual examples remain candidates until their owning
system architecture defines exact rules, timing, costs, and presentation.

## Translation principle

Turn-based games give the human enough time to choose every stance, facing,
reaction, target, movement tile, and synchronized action. Doing that directly
for 50–100 units in real time is impossible.

S.I.R. can retain that depth by dividing responsibility:

```text
human commander
  chooses purpose, priority, acceptable risk, doctrine, and commitment
                         ↓
HQ, leaders, and unit modules
  execute timing, reactions, coordination, and routine contingencies
                         ↓
authoritative simulation
  resolves simultaneous actions, information, and consequences
```

Automation should increase the tactical vocabulary available to the player. It
must not reduce the human to watching an AI independently choose strategy.

## Highest-value feature set

### 1. Continuous reaction and interruption

Turn-based reaction fire becomes a continuous readiness system rather than an
on/off overwatch button.

A reaction depends on:

- body facing and attention direction;
- whether the weapon is ready, aimed, lowered, reloading, or obstructed;
- acquisition progress and target confidence;
- surprise and prior warning;
- stance, suppression, wounds, and movement;
- declared reaction policy;
- friendly-fire and ammunition rules; and
- the time required to turn, shoulder, aim, and act.

Reactions can interrupt door opening, spell preparation, movement across a
gap, an execution, a reload, medical work, or another committed action.

This creates turn-based-style questions—who was ready, who was watching, and
who committed first—without stopping time.

### 2. Progressive awareness and acquisition

Visibility is not instant target ownership. A unit can:

1. receive a stimulus;
2. orient attention;
3. detect a possible contact;
4. classify it;
5. identify it;
6. acquire a firing or action solution; and
7. communicate a report.

Facing, concealment, motion, lighting, sound, sensors, prior reports, and
attention determine progress. A target can move through a peripheral sector
without being immediately engaged.

This makes stealth, distraction, flanking, ambush, rear attacks, and executions
mechanically decisive. Unit modules can react at precise tick boundaries while
the player reasons at squad and battlefield scale.

### 3. Conditional orders and standing operating procedures

Instead of issuing only immediate commands, the player can define bounded
policies:

- hold fire until a confidence, range, target-value, or ambush condition;
- return fire, break contact, or remain concealed;
- use grenades only above a target-density threshold;
- preserve a minimum ammunition reserve;
- avoid crossing an unobserved opening without security;
- withdraw when casualties, suppression, isolation, or supply cross a limit;
- prioritize leaders, casters, sensors, communications, or logistics;
- continue, rally, hide, or return when communication is lost; and
- wake or request help on declared events.

The official modules provide usable doctrines. Player-provided modules can
implement more sophisticated policies without gaining extra information or
capabilities.

### 4. Synchronized multi-unit actions

Turn-based games let a player coordinate several units perfectly because
nothing advances during planning. S.I.R. can support coordination through
explicit action dependencies and local communication:

- stack and breach;
- simultaneous entry through several doors;
- grenade or spell followed by entry;
- suppression followed by movement;
- bounding overwatch;
- crossfire establishment;
- synchronized shots;
- casualty pickup under covering fire;
- vehicle dismount and security;
- staged withdrawal; and
- timed demolition or distraction.

Participants can declare `ready`, `blocked`, `abort`, or `committed` through
their modules. The action begins when its local conditions and communication
policy permit, not because the human clicked every unit within a fraction of a
second.

### 5. Detailed facing, stance, exposure, and weapon posture

Units can continuously manage:

- eight-way body facing;
- independent attention sector;
- standing, crouched, and prone posture;
- weapon-ready versus movement posture;
- which edge or corner of the square footprint is used;
- lean or peek exposure;
- muzzle clearance;
- shield or armored-side orientation; and
- turn-before-action requirements.

These details are usually exhausting in real time. Modules can execute them
from doctrine while the human chooses the watched approach, assault direction,
formation, or risk posture.

### 6. Action commitment instead of abstract turns

Actions have preparation, commitment, resolution, and recovery phases.
Consequences include:

- a prepared action can be cancelled cheaply;
- a committed action is difficult or impossible to cancel;
- interruption timing changes the result;
- heavy attacks create exploitable recovery;
- reloading at the wrong moment is dangerous;
- a unit caught turning or changing stance is disadvantaged; and
- attacks completing on the same tick resolve simultaneously.

This preserves the meaningful commitment normally created by spending action
points, while remaining native to continuous time.

### 7. Reaction reserve as a continuous resource

A unit can preserve readiness rather than spending every moment moving or
acting at maximum tempo.

Examples:

- moving cautiously preserves more reaction capability than sprinting;
- aiming at a sector trades mobility for faster engagement;
- suppressing an area consumes ammunition but protects movement;
- keeping a weapon shouldered increases fatigue or reduces movement speed; and
- aggressive pursuit sacrifices security against flank contact.

This is the real-time equivalent of saving action points for overwatch or an
interrupt, without exposing literal turns.

### 8. Precise fire control and lines of fire

Modules make detailed fire discipline feasible:

- do not fire through a friendly's predicted footprint;
- wait for a clear lane;
- choose a burst length from suppression need and ammunition policy;
- select exposed, dangerous, or high-value targets;
- shift fire as friendly assault units approach;
- stop suppressing before a friendly crosses the beaten zone;
- account for penetration and units behind the intended target;
- choose whether collateral risk is acceptable; and
- coordinate crossfires without all units selecting the same target.

Misses, penetration, cover interception, and friendly fire remain physical
consequences rather than accuracy percentages that affect only the intended
target.

### 9. Ambush construction and counter-ambush drills

An ambush can require:

- a defined kill zone;
- concealed firing positions;
- target-value or target-count thresholds;
- initiator authority;
- synchronized first fire;
- blocked escape directions;
- ammunition and withdrawal plans; and
- an abort condition.

The player designs the ambush and delegates the exact release timing. Opposing
modules can execute contact drills: face the threat, seek cover, return
suppression, move out of the kill zone, report, and reorganize.

This makes ambush a system rather than a bonus attached to the first attack.

### 10. Breaching and room-clearing behavior

A breach can model:

- stack order and assigned roles;
- rear and flank security;
- door inspection, opening, kicking, explosives, or magical breach;
- muzzle and body clearance;
- fatal funnel exposure;
- assigned entry sectors;
- threshold evaluation;
- limited penetration versus deep entry;
- crossfire avoidance;
- casualty contingencies; and
- abort or alternate-entry decisions.

The human selects the objective, entry points, timing, tools, and aggression.
Modules handle sub-second spacing and sector allocation.

### 11. Suppression, pinning, and maneuver

Suppression should alter what is tactically safe, not simply reduce accuracy.
It can:

- slow acquisition and turning;
- make exposed movement difficult;
- interrupt preparation;
- force posture changes;
- delay communication or leadership;
- consume reaction reserve;
- enable another element to move; and
- persist briefly after direct fire stops.

Modules can coordinate the timing needed to lift, shift, or renew suppression
as friendly units maneuver.

### 12. Casualty states and recovery under fire

Persistent individuals make consequences beyond zero HP meaningful:

- incapacitated but recoverable;
- bleeding or deteriorating;
- conscious but unable to move normally;
- stabilized;
- carried or dragged;
- abandoned, captured, or extracted; and
- equipment or communications recovered from a casualty.

Modules handle triage, safe approach, covering fire, pickup position, and route
selection according to player doctrine. The player decides how much mission
risk a recovery is worth.

## Information and command features

### Local knowledge rather than player omniscience

Every unit reasons from its own observations, reports, and memory. This permits:

- contradictory reports;
- stale contact positions;
- squads fighting while disconnected from HQ;
- a leader knowing something the player does not yet know;
- reports arriving after their tactical value has changed;
- local initiative based on doctrine; and
- recovery of information when communication returns.

The system produces much of the uncertainty found in tabletop and turn-based
tactics without artificial global fog rules.

### Command succession with behavioral consequences

Leader, second-in-command, and NCO succession can affect:

- which module coordinates the squad;
- what doctrine survives;
- communication equipment access;
- formation and objective memory;
- rally and reaction behavior; and
- what must be reconstructed after an unexpected casualty.

Leadership loss therefore changes behavior instead of merely applying an aura
penalty.

### Electronic warfare and deception

Detailed automated control makes several effects manageable:

- delayed, corrupted, intercepted, or suppressed reports;
- false contacts with provenance;
- direction finding against transmitters;
- emission-control doctrine;
- relay placement and destruction;
- spoofed identifiers;
- decoy sensors or magical signatures; and
- local behavior triggered by loss of trusted communication.

False information must enter through the same observation/report model as true
information. The server never directly falsifies the player's UI outside that
model.

## Logistics features with tactical timing

Automation makes granular supply tactically useful without requiring repetitive
human inventory work:

- magazine and ammunition-type selection;
- reload timing and partial-magazine handling;
- redistribution within a squad;
- casualty equipment recovery;
- medical and magical consumable allocation;
- battery, drone, sensor, and communications power;
- vehicle fuel and ammunition;
- cached supplies and emergency reserves;
- resupply routes and handoff points; and
- automatic requests constrained by communications.

The human sets priorities and reserves. Modules perform routine transfers and
choose safe opportunities.

## Faction-asymmetric possibilities

### Modern human forces

- superior communications and sensor fusion;
- disciplined fire control;
- drones and deployable relays;
- coordinated breaching and casualty evacuation;
- ammunition and maintenance dependence; and
- advanced but bounded nanomedical recovery.

### Arcane civilization

- risk-managed spell preparation;
- casters choosing health expenditure versus strain;
- coordinated protective or amplifying magic;
- magical detection and counter-detection;
- durable nonhuman assault units;
- summons or undead with unusual command requirements; and
- catastrophic breach consequences that opponents can deliberately pressure.

The same timing, knowledge, and commitment architecture supports both without
making the factions mechanically symmetrical.

## Features to treat cautiously

### Detailed anatomical simulation

Fine hit locations, individual organs, and many wound subtypes can create
processing and UI cost without proportionate tactical value. Use a small number
of consequential wound states unless tests prove that finer anatomy changes
decisions.

### Constant morale micromanagement

Suppression, shock, leadership, and isolation are tactically valuable. A dense
personality simulation that frequently overrides orders risks making outcomes
feel arbitrary. Prefer legible state transitions and doctrine-dependent
responses.

### Excessive conditional scripting

An unrestricted visual-programming language in the canonical client could turn
pre-match preparation into programming homework. The standard modules need
strong defaults, concise doctrine controls, and progressive disclosure.
Advanced players can supply code directly.

### Perfect automated optimization

If modules always calculate the globally optimal target, route, formation, and
resource use from all friendly information, the human loses meaningful
command. Preserve:

- local knowledge;
- communication cost;
- bounded services and fuel;
- uncertain enemy intent;
- action commitment;
- competing objectives; and
- player-selected doctrine and risk.

### Too many invisible modifiers

Automation can calculate hundreds of modifiers, but players still need to
understand why a decisive event occurred. Prefer strong causal states—rear
exposure, suppression, no communication, weapon not ready, breached cover—over
large stacks of small opaque bonuses.

## Canonical identity package

The strongest initial combination is:

1. continuous reaction and interruption;
2. progressive awareness and acquisition;
3. conditional doctrine and communication-loss behavior;
4. synchronized suppression, movement, breach, and withdrawal;
5. facing, attention, stance, and action commitment;
6. physical firing lanes and friendly-fire risk;
7. meaningful suppression;
8. local knowledge and delayed reports; and
9. casualty recovery with persistent consequences.

Together these features create tactical depth that conventional real-time
control cannot comfortably support and that is more distinctive than simply
running an XCOM-like ruleset without turns.

## Evaluation questions

For every candidate mechanic:

1. What strategic or tactical decision does the human make?
2. What precise execution does the module automate?
3. What information is locally available to that module?
4. What communication is required?
5. What commitment, cost, or risk prevents free optimization?
6. Can an opponent observe, infer, disrupt, or exploit it?
7. Can the result be explained clearly after it happens?
8. Does it remain computationally bounded at 100 units per side?
9. Does the standard module use it competently without configuration?
10. Does it create distinct faction, class, equipment, or terrain choices?
