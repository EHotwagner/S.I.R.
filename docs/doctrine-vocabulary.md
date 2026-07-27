---
title: S.I.R. Doctrine Vocabulary
status: proposed
document-type: living-design
version: "0.1"
last-updated: 2026-07-27
related:
  - docs/wasm-control-architecture.md
  - docs/game-vision.md
  - docs/combat-resolution.md
  - docs/logistics-architecture.md
---

# S.I.R. Doctrine Vocabulary

## Purpose

[WebAssembly Control Architecture](wasm-control-architecture.md) establishes
that standing doctrine is an ordered list of condition-to-action rules drawn
from two versioned published vocabularies, and that the same condition
vocabulary is reused for wake subscriptions. It specifies the container,
evaluation semantics, guards, cost model, and authorship contract.

It does not name a single condition or action. This document will hold those
vocabularies. It currently holds the readiness audit that must precede them.

## Why an audit comes first

A doctrine condition can only test a fact the simulation authoritatively
defines, and a doctrine action can only invoke a capability that exists. The
vocabulary is therefore not a free design exercise — it is largely determined by
which underlying models are specified.

Auditing first also prevents the vocabulary from silently inventing concepts.
The illustrative rule list in the control architecture already does this: it
uses `withdraw to rally`, and **no document defines a rally point**. That is
exactly the failure this audit exists to catch.

## Condition readiness

| Area | Candidate conditions | Status |
|---|---|---|
| Self state | HP band, wounds, suppression, stance, facing, attention, current action and phase, readiness from recent movement | **Ready** |
| Caster state | current strain, breach margin, spell availability | **Ready** |
| Local knowledge | known hostile count in a sector or range band, classification, time since last contact, known friendly positions, stimulus direction | **Ready** |
| Geometry | exposure, cover relationship, edge adjacency, level | **Ready** |
| Position reference | distance to, or arrival at, a named place | **Blocked** — see gap 1 |
| Squad and command | acting leader, succession state, casualties, connectivity to leader and to HQ | **Ready** |
| Formation | current formation, formation integrity, position within it | **Blocked** — see gap 2 |
| Communications | connected to leader, connected to HQ, degraded or jammed | **Ready** |
| Logistics | own inventory levels, observed supplies, delivered stock reports, local transfer outcomes | **Ready** |
| Mission | objective location, state, or progress | **Blocked** — see gap 3 |
| Command bandwidth | own allocation or starvation state | **Open** — see question 5 |

## Action readiness

| Area | Candidate actions | Status |
|---|---|---|
| Engagement | engage point target, engage area, hold fire, cover a sector | **Ready** |
| Posture | stance, body facing, attention direction, deliberate observation | **Ready** |
| Movement | move to a cell, hold position, follow, yield | **Ready** |
| Movement to a referent | withdraw to, rally at, assemble at | **Blocked** — gap 1 |
| Coordinated movement | bounding advance, overwatch handoff, formation change | **Blocked** — gap 2 |
| Logistics | request resupply, transfer, pick up, drop, redistribute | **Ready** |
| Medical | stabilize, carry, evacuate | **Blocked** — see gap 4 |
| Communication | report, relay, send player-defined payload | **Ready**, subject to the provisional report model |
| Magic | cast with declared aspect spend and strain ceiling | **Ready** |

## Blocking gaps

### 1. Named positional referents

A unit can be told to move to a cell. Nothing lets doctrine refer to a *place*
by role — a rally point, an assembly area, a fallback position, a casualty
collection point, a supply point.

This matters more than it appears. A doctrine that can only name absolute cells
is not portable between maps, cannot survive the map changing through
destruction, and cannot express intent that outlives a specific coordinate.
Almost every useful withdrawal, resupply, or regroup rule needs one.

Required: what a referent is, who assigns it, whether it is per-squad or
per-force, whether it is authoritative state or local belief, how it is
communicated, and what happens when it becomes unreachable or is overrun.

### 2. Formation

Twenty references across the design set. No definition anywhere.

The documents already lean on formations as a leader effect, a doctrine element,
a host service (`formation planning` is listed as an expensive metered service),
and a tactical behavior — bounding overwatch, sector assignment, reorganizing
after disruption. None of it rests on a model.

Required: what a formation is in authoritative terms, whether it is a
constraint on movement or an advisory shape, how it interacts with cooperative
footprint reservation and the semantic edge layer, how sectors of attention are
assigned within it, and what "integrity" means when a formation is disrupted.

The cooperative movement rules already solve the hard part — reservations,
yielding, deadlock breaking. Formation is likely a layer that expresses intent
over those rules rather than a new movement system.

### 3. Unit-level objective knowledge

Objectives are defined as versioned authoritative state machines consuming
simulation events, and their disclosure to *players* is specified. Nothing
states what an individual unit may know about an objective.

Without it, doctrine cannot express the most basic mission-linked behavior:
hold until the objective completes, withdraw once it is secured, prioritize a
threat to it.

Required: which objective facts reach a unit, whether they arrive as ordinary
reports through the communications topology, and whether a disconnected unit
retains a stale objective picture as it does for everything else.

### 4. Medical actions

Combat resolution specifies incapacitation, bleeding, stabilization,
evacuation, and recovery as *outcomes and states*. No document specifies them as
*actions* with timing, eligibility, resource cost, interruption rules, and
observable effects, which is what a capability descriptor requires.

Casualty recovery under fire is named as one of the nine features in the
accepted tactical identity package, so this is not a peripheral omission.

Required: the medical capability set under the ordinary action lifecycle, its
draw on the medical supply classes logistics already defines, and its
eligibility against wound and incapacitation state.

## Open questions

1. Should conditions be a flat set, or grouped into namespaces per area?
2. Should conditions support composition beyond conjunction within a rule —
   disjunction, negation, thresholds with hysteresis?
3. Should an action be able to fail *softly* and fall through to the next
   matching rule, or does rule selection commit for the tick?
4. Do wake subscriptions need conditions the doctrine vocabulary does not, such
   as "any change in this fact" rather than a predicate over its value?
5. **May doctrine test its own command bandwidth or starvation state?** Allowing
   it lets a module degrade gracefully when starved, which is desirable. It also
   makes doctrine reflexive about its own execution budget, which risks
   feedback that is hard to reason about and may leak scheduling detail into
   gameplay. Not resolved.
6. Are the vocabularies shared across factions, or does each faction publish
   its own, given that faction capability contracts are already per-faction?

## Not blocking

An earlier assessment held that the logistics model was too thin to support the
vocabulary. On inspection it is not. `logistics-architecture.md` already states
precisely what a unit may know — its own inventory, supplies it observes,
reservations and stock reports legitimately delivered to it, and locally
completed or failed transfers — and it enumerates the automation intents that
map onto doctrine rules. Its open items are values and taxonomies rather than
missing structure.

Combat, perception, movement, communications, and magic are likewise ready. The
vocabulary is blocked by four specific gaps, not by general immaturity.
