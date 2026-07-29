---
title: Casualties and Medical
category: Battlefield Systems
categoryindex: 4
index: 7
status: proposed
document-type: living-design
version: "0.2"
last-updated: 2026-07-27
related:
  - docs/combat-resolution.md
  - docs/logistics-architecture.md
  - docs/mission-lifecycle.md
  - docs/control-abi.md
  - docs/formations-and-referents.md
---

# S.I.R. Casualty and Medical Architecture

## Purpose

Combat resolution defines incapacitation, bleeding, stabilization, evacuation,
and recovery as **outcomes and states**. Mission lifecycle defines their
end-of-mission dispositions. Logistics defines the supply classes they consume.

None of them defines the **actions**. This document specifies the medical
capability set under the ordinary action lifecycle, so that it can be expressed
as capability descriptors and events in the
[Control ABI Surface](control-abi.md).

## The governing decision

**Battlefield treatment does not return a unit to the fight.**

This follows the established direction that treatment "primarily restores
limited function, arrests deterioration, or creates an opportunity for
evacuation," and that keeping a casualty alive during a match need not return
that person to combat effectiveness during the same match.

It is the single most consequential rule here, and it is deliberate:

- it forecloses the healer loop, in which supplies convert into restored
  fighting units and lethality quietly stops mattering;
- it makes a casualty a **logistics and mission problem** rather than a combat
  resource to be recycled; and
- it makes the tactical question *is recovering this person worth the risk*
  rather than *how fast can I put them back in the line*, which is the question
  the casualty-recovery feature was accepted to create.

A stabilized casualty is alive and recoverable. They are not a soldier again
until the campaign layer has spent time and resources on them.

## Casualty states

```text
functional (possibly wounded)
      │ HP reaches zero
      ▼
incapacitated + deteriorating ──── untreated ────► dead
      │ stabilize
      ▼
incapacitated + stable
      │
      ├── taken up and carried
      ├── evacuated
      ├── abandoned
      ├── captured
      └── executed
```

A **functional** unit may carry wounds that impair it without incapacitating it.
An **incapacitated** unit is at zero HP, cannot act, and deteriorates until
stabilized or dead. A **stable** unit has had deterioration arrested and remains
incapacitated.

Deterioration rate, and whether some damage bypasses incapacitation into
immediate death, remain prototype parameters under combat resolution.

## Medical actions

All four are ordinary timed actions under the canonical lifecycle, and all are
interruptible.

### Aid

Applied to a **functional** unit. Arrests bleeding and temporarily mitigates a
wound's impairment. It does not meaningfully restore HP.

Aid is what keeps a wounded but standing soldier effective. It is the only
medical action whose subject continues fighting.

### Stabilize

Applied to an **incapacitated, deteriorating** unit. Arrests deterioration and
moves the casualty to stable. It does not restore consciousness, function, or
HP.

Stabilization is the action that converts a probable death into a recoverable
casualty, and it is therefore the one most worth contesting.

### Take up and carry

Carrying is a **continuous state**, not a discrete action. Taking up and setting
down are the timed actions; the carrying itself persists.

A carried casualty consumes carrier capacity under the logistics load model. A
carrying unit is slowed, cannot hold an area engagement, and engages point
targets poorly or not at all.

Two carriers move a casualty faster than one dragging them, at the cost of a
second unit removed from the fight. That trade is the intended decision rather
than an optimization to be solved once.

### Evacuate

Handing a casualty off at a casualty collection point, an evacuation asset, or
an extraction zone. This uses the referent model, so doctrine expresses
evacuation as movement toward a role-tagged place rather than a coordinate.

Evacuation frees the carrier and fixes the casualty's disposition for
mission-end resolution.

## Eligibility and skill

Any unit may attempt stabilization and basic aid. Trained medical personnel do
so faster, more reliably, with better outcomes, and may access procedures others
cannot.

This preserves buddy aid as a real option — a squad is not helpless because its
medic is the casualty — while keeping the medical class meaningfully
specialized. Self-aid is available to a functional unit and unavailable to an
incapacitated one, which is what makes incapacitation dependent on someone else
arriving.

Eligibility inputs include the subject's state, the actor's proficiency and
class, available supplies, adjacency, and the actor's own condition.

## Costs and interruption

Medical actions consume:

- **medical supplies** from the classes logistics already defines, drawn as
  treatment charges or procedure packages;
- **time** under the action lifecycle, with stabilization notably slow; and
- **exposure**, because the actor is stationary, adjacent to a casualty, and
  frequently wherever that casualty fell.

An action is interrupted by damage to the actor, by displacement, by the
subject's death, and by the actor's own incapacitation. Interruption may consume
part of the supply cost according to the capability's declared commitment rules.

The exposure cost is the point. Treatment happens where the casualty is, which
is by definition a place the enemy has recently been able to shoot.

## Tactical consequences

A casualty is a **magnet**. Recovering one costs time, exposure, and at least
one unit removed from the fight, so a single incapacitation can pull a squad's
tempo apart.

That makes casualty denial a legitimate tactic. Suppressing the ground around a
casualty, or holding an area engagement across it, forces the choice between
abandoning a person and feeding more units into a beaten zone. This is the
sharpest expression of the area-engagement model and it needs no special rule.

Executions interact directly: an incapacitated unit is eligible for execution,
so reaching a casualty first matters to both sides. Stabilization does not
protect against execution — it protects against bleeding out.

## Faction variation

The action architecture is shared; the capabilities differ.

**Human forces** are bounded by the provisional nanomedical direction: trained
personnel, consumable stocks, treatment time, limits by injury type, and
possible complications. Their medicine buys survival and evacuation, not
resurrection.

**Arcane forces** may restore HP directly, which interacts with the strain and
breach threshold — healing a strained caster restores breach margin, making
medicine part of the magical economy even when the healer is nonmagical. The
constraint against healing becoming a loop that converts supplies into unlimited
safe casting remains an open question in the magic system.

**Other portal-origin factions** may replace casualty recovery entirely.
Undead conversion, in particular, is a different relationship with casualties
rather than a variant of this one.

A faction's medical capabilities are declared in its capability contract like
any other.

## Campaign boundary

Within a mission, medicine arrests deterioration and enables evacuation.

Between missions, the campaign layer performs actual recovery, consuming time,
facilities, and resources, and resolving lasting wounds. A casualty's
mission-end disposition — extracted, secured, abandoned, captured, dead — is
what the campaign receives, and it is already specified in mission lifecycle.

Skirmish modes resolve dispositions without campaign write-back.

## What this unblocks

Medicine becomes expressible in the control ABI: aid, stabilization, taking up,
setting down, and evacuating to a referent become capability descriptors, and
casualty state changes become events a module is told about.

## Open parameters

- Deterioration rate, and whether it varies by the damage that caused
  incapacitation.
- Aid and stabilization durations, and how far proficiency changes them.
- Supply cost per action, and partial consumption on interruption.
- Wound mitigation from aid: magnitude, duration, and whether it can be
  reapplied.
- Carry and drag speeds, capacity cost, and what a carrier may still do.
- Whether a stabilized casualty can deteriorate again.
- Whether treatment is possible while under suppression, or merely slower.
- Whether some incapacitations are untreatable in the field.
- Whether captured casualties can be treated by their captors.
