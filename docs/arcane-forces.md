---
title: S.I.R. Arcane Civilization Forces
status: proposed
document-type: living-content
version: "0.3"
last-updated: 2026-07-27
related:
  - docs/setting-and-factions.md
  - docs/human-forces.md
  - docs/magic-system.md
  - docs/electronic-warfare.md
  - docs/communications-network.md
---

# S.I.R. Arcane Civilization Forces

## Purpose

The second content document, and the anchor portal-origin opponent for the
initial release. It is written after
[Human Forces](human-forces.md) deliberately, because a faction defined in the
abstract produces a list of powers, while a faction defined against a concrete
opponent produces a different game.

## The design position

Humans are a network. Their capability is information capability, it is carried
electronically, and it therefore emits, drains, and can be attacked without ever
being shot at.

**The arcane civilization is the answer to that.** Not a force with different
damage values, but one whose coordination cannot be jammed, direction-found, or
intercepted — because none of it is transmitted.

That does not make it free. The faction capability contract requires every
command network to declare a dependency that can be attacked. The arcane
dependency is simply not electronic:

```text
humans          coordination is a network        attacked electronically
arcane          coordination is a geography      attacked physically
```

Against humans you jam, locate, and cut. Against the arcane you must **reach
something and destroy it**, or manoeuvre outside its influence. Those are
different operational problems, which is what faction asymmetry is supposed to
produce.

## Coordination is anchored, not transmitted

Arcane coordination flows through **anchors**: ritual sites, bound standards,
inscribed objects, and the casters who sustain them.

Within an anchor's influence, units coordinate without transmitting anything.
Outside it, they operate on their own judgement and whatever they were told
before they left.

The properties this produces are the exact inverse of the human network:

| | Human network | Arcane anchoring |
|---|---|---|
| Medium | transmission | influence |
| Signature | emits continuously | none, to human sensors |
| Attacked by | jamming, direction finding, traffic analysis | reaching the anchor and destroying it |
| Shape | topology | **geography** |
| Extended by | relays and drones | placing and holding ground |
| Failure | isolation, still fights | isolation, still fights |

The last row matters: neither faction collapses when cut off. Both continue on
local judgement, which is the same rule applied to two different mechanisms.

**Anchors are authoritative objects with positions.** They can be observed,
approached, destroyed, and in some cases captured. Extending arcane coordination
means physically occupying more ground, which is why this faction contests
terrain that a human force would bypass.

## Two economies, and neither is ammunition

Humans are limited by **supply**: ammunition, batteries, spare parts, all of it
replenishable mid-mission through a physical supply network.

The arcane are limited by **accumulation**. A caster spends health to empower,
gathers strain that persists, and becomes progressively more dangerous to
themselves — see [Risk-Based Magic System](magic-system.md). Strain does not
resupply.

The consequence is a completely different tempo:

- **humans sustain and degrade slowly**, provided the supply line holds;
- **the arcane front-load and degrade steadily**, and cannot resupply their way
  out of it.

An arcane force is therefore most dangerous early and becomes volatile as a
match progresses, while a human force is most dangerous while its network and
supply hold and becomes ordinary when they do not. Two clocks running in
opposite directions is a better basis for a match than two health bars.

The faction's logistical economy — components, catalysts, prepared sites — is
real but serves preparation rather than sustainment. It is spent to *establish*
rather than to *continue*.

## Force shape: scarce decisive casters, durable mass

Human forces distribute capability across many similar people. The arcane
concentrate it.

- **Casters** are few, individually decisive, and fragile in a specific way:
  damaging one does not merely reduce its output but can push accumulated strain
  past its remaining health and trigger a breach. A caster is a liability that
  grows more valuable and more dangerous at the same time.
- **Nonmagical peoples** — goblins, orcs, trolls, and others — are the mass that
  makes scarcity survivable. They are not failed casters. They screen, hold
  ground, absorb the ranged fire humans are best at delivering, and carry the
  components and infrastructure the casters depend on.

Substantial armour and regeneration are the candidate characteristics for the
heavier of these, which directly answers human strengths: armour against small
arms, and recovery against a faction whose in-match healing is deliberately
limited.

**This inverts the human answer to attrition.** Humans stabilise casualties and
evacuate them for recovery between missions. The arcane recover during the
match and lose people permanently when they lose them.

## Information: different facts, not more of them

Arcane sensing does not produce a better version of the human picture. It
produces a different one.

The candidate direction is that magical perception detects **presence and
vitality through obstruction**, without resolving identity, equipment, or exact
position. A caster knows something living is behind that wall. It does not know
what it is carrying or which way it faces.

Set against human sensors — precise, identifying, ranged, and blocked by
geometry — this produces a genuine trade rather than a hierarchy:

- **humans see further and more exactly, and only what they have line to**;
- **the arcane see through, and vaguely**.

Neither is dominant, and the correct approach differs by terrain. In open ground
the human picture is enormously better. Inside a structure it is the arcane who
know where everyone is.

Arcane sensing does not emit in any way a human sensor detects, which means a
human force cannot initially tell it is being observed. That is a legitimate
advantage under the guardrail requiring evidence, provided the *effects* are
observable — a force that reacts to something it should not have seen has
revealed a capability, and human research is the eventual answer.

## Meeting the human sensory and electronic apparatus

Humans field a great deal of machinery. It divides cleanly, and the division is
the point.

**Human sensors work completely.** Arcane units are physical, warm, audible
bodies. Optics see them, thermal sees them, acoustics hear them. They are not
invisible and nothing here makes them so.

**Human electronic warfare does nothing at all.** There is no emission to
direction-find, no link to jam, and no traffic to analyse. An entire branch of
human capability has no purchase on this enemy.

That split is deliberate: humans keep their observation advantage in full and
lose their disruption advantage entirely.

### The human network becomes more reliable, not less

The consequence is easy to state backwards. Against another human force,
communications are contested continuously — jammed, located, degraded. Against
the arcane, **nothing contests them electronically**, so a human commander
enjoys their full information apparatus uninterrupted.

That is the human edge in this matchup and it is why they are the side capable
of finding anchors. It also means a human player must fight differently in PvE
than in PvP, with a toolkit that is decisive in one and inert in the other.

This is a setting statement rather than an imbalance. Humans built their
apparatus to fight humans, and *difficulty understanding new supernatural rules*
is already a listed human vulnerability.

### The arcane answer is physical

The arcane cannot attack a human network electronically, so they attack it the
way they attack everything else: by reaching it.

- kill the **signaller**, and a squad loses its command-net set and its drones;
- destroy the **relay**, and everything behind it is isolated;
- reach the **drone**, or fight where a drone cannot see;
- **obscure**, and fight at ranges where seeing-through beats seeing-far.

So both factions attack each other's coordination physically — anchors on one
side, carriers on the other. Humans simply also carry an electronic toolkit that
does not apply.

### What humans can eventually detect

**Magical signature is already a stimulus modality** in the perception model, so
detecting magic requires an instrument rather than new machinery.

Until humans build one they observe *effects* and not *sensing*: they see a ward
take a shot and a caster gesture, and they cannot tell they are being perceived
through a wall. Fielding a magical-signature detector is therefore a natural
research objective and the eventual answer to the guardrail requiring every
capability to leave evidence.

## The faction capability contract

The setting document requires every faction to answer ten questions.

| | Answer |
|---|---|
| Acquires information | Magical perception through obstruction, vague and short-ranged; scouts and screening mass otherwise |
| Issues commands | Anchored influence, not transmission; outside it, prior intent |
| Consumes | Health and strain personally; components, catalysts, and prepared sites logistically |
| Moves forces and supply | On foot, with mass carrying infrastructure; constrained translocation as a candidate specialist capability |
| Replaces casualties | Not within a match. Reinforcement is a campaign matter |
| Recovers from injury | Regeneration in the heavier nonmagical peoples; magical healing at cost to the healer |
| Control failure | Anchor destruction, caster loss, and breach consequences that damage their own side |
| Does better | Fighting where geometry favours the obscured, absorbing ranged fire, operating without a network |
| Vulnerable to | Reaching and destroying anchors, focusing casters, forcing strain, and open ground |
| Reveals itself by | Anchors as physical objects, visible magical effects, breach events, and reactions to things it should not have seen |

## The matchup

**What humans do to the arcane:** locate anchors with superior observation,
engage casters at range where their sensing is weakest, force strain by
threatening objectives that demand empowerment, and fight in open ground.

**What the arcane do to humans:** close distance under durable screens, fight
inside structures where seeing-through beats seeing-far, and make the entire
electronic apparatus irrelevant by having nothing for it to attack.

The sharpest expression is what each side does about drones. A human relay drone
is elevation you can move and the clearest asset humans own. The arcane cannot
jam it, cannot direction-find it, and have no counter to it at all except
reaching it or making it pointless — which means an arcane force that fights
where drones cannot see is fighting on its own terms.

## Checking against invariant 13

Neither faction's approach is unconditionally correct, and neither faction has
an internally dominant configuration.

Casters versus mass trades decisiveness against durability, and the correct
ratio depends on terrain and on how long the engagement will last. Empowerment
trades immediate effect against strain, which is a decision every cast rather
than a build chosen once. Anchoring trades coordination against the obligation
to hold ground.

The risk sits with **anchors**. If placing one is always correct, they become a
setup step rather than a decision. They need to cost enough — in time, in
components, and in the units required to hold them — that operating unanchored
is sometimes right.

## What this tests in the architecture

- whether **strain and breach** produce interesting decisions at 20 ticks per
  second, or resolve too fast to reason about;
- whether a faction with **no communications network** can be represented
  without special-casing the comms model;
- whether **regeneration** stays distinguishable from simply having more health;
- whether **anchored coordination** is expressible through the ordinary
  knowledge and command rules rather than requiring privileged server behaviour;
  and
- whether the asymmetry survives contact, or collapses into one side being
  correct.

## Open parameters

- Every numeric value, as with human forces.
- Anchor influence shape, radius, placement cost, and whether it is blocked by
  terrain as signal paths are.
- Whether anchors can be captured and used, or only destroyed.
- Which aspects each spell permits empowering, and at what rates. The spell set
  itself is in [Arcane Spells](arcane-spells.md).
- Strain recovery rate, and whether anything recovers it within a match.
- Regeneration rates, and what suppresses them.
- The distribution of roles across goblins, orcs, and trolls, which the setting
  document leaves open.
- Whether constrained translocation exists at all, and what bounds it.
- What a human magical-signature detector reveals, its range, and where it sits
  on the research path.
