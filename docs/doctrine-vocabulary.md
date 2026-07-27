---
title: S.I.R. Doctrine Vocabulary
status: proposed
document-type: living-design
version: "0.3"
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
| Position reference | distance to, arrival at, or reported validity of a referent | **Ready** — [formations-and-referents](formations-and-referents.md) |
| Squad and command | acting leader, succession state, casualties, connectivity to leader and to HQ | **Ready** |
| Formation | formation integrity, own station occupancy | **Ready** — [formations-and-referents](formations-and-referents.md) |
| Communications | connected to leader, connected to HQ, degraded or jammed | **Ready** |
| Logistics | own inventory levels, observed supplies, delivered stock reports, local transfer outcomes | **Ready** |
| Mission | known objective location, state, or progress | **Ready** — [formations-and-referents](formations-and-referents.md) |
| Command bandwidth | own allocation or starvation state | **Open** — see question 5 |

## Action readiness

| Area | Candidate actions | Status |
|---|---|---|
| Engagement | engage point target, engage area, hold fire, cover a sector | **Ready** |
| Posture | stance, body facing, attention direction, deliberate observation | **Ready** |
| Movement | move to a cell, hold position, follow, yield | **Ready** |
| Movement to a referent | withdraw to, rally at, assemble at | **Ready** |
| Coordinated movement | bounding advance, overwatch handoff, formation change | **Ready** |
| Logistics | request resupply, transfer, pick up, drop, redistribute | **Ready** |
| Medical | aid, stabilize, take up, set down, evacuate | **Ready** — [casualty-and-medical](casualty-and-medical-architecture.md) |
| Communication | report, relay, send player-defined payload | **Ready**, subject to the provisional report model |
| Magic | cast with declared aspect spend and strain ceiling | **Ready** |

## Resolved

Gaps 1 to 3 — named positional referents, formation, and unit-level objective
knowledge — are addressed in
[Formations and Positional Referents](formations-and-referents.md).

All three turned out to be one problem: how doctrine refers to things beyond the
unit's own state. Referents name a place by role, formations describe
arrangement relative to a reference, and an objective is a referent whose facts
propagate through the ordinary report path. Each is held as a per-unit *belief*
delivered through the communications topology rather than as universally known
truth, so a disconnected squad can act on a stale rally point or a stale
objective picture.

## Gaps closed

All four gaps identified by this audit are now addressed.

Named positional referents, formation, and unit-level objective knowledge are
defined in [Formations and Positional Referents](formations-and-referents.md).
All three were one problem — how doctrine refers to things beyond the unit's own
state — and each resolves as a per-unit belief delivered through the
communications topology rather than as universally known truth.

Medical actions are defined in
[Casualty and Medical Architecture](casualty-and-medical-architecture.md), which
specifies aid, stabilization, carrying, and evacuation as timed capabilities and
establishes that battlefield treatment does not return a unit to the fight.

**The vocabulary is no longer blocked by missing models.** What remains is
naming its entries, which is a specification task rather than a design one, and
resolving the open questions below.

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
