---
title: S.I.R. Arcane Spells
status: accepted
decision-status: canonical
document-type: living-content
version: "1.3"
last-updated: 2026-07-28
related:
  - docs/magic-system.md
  - docs/arcane-forces.md
  - docs/human-forces.md
  - docs/game-vision.md
---

# S.I.R. Arcane Spells

## Purpose

[Risk-Based Magic System](magic-system.md) defines the economy — health spent to
empower, strain accumulated, breach when strain exceeds health. It does not name
a single spell.

This is the content: what casters actually do, and why each thing is worth its
cost.

Rituals are a separate cooperative surface rather than entries in this spell
list. A spell is performed by one caster; a ritual requires a caster quorum at
a prepared site, commits to geography, and competes for anchor capacity. The
canonical ritual shapes are defined in
[Arcane Civilization Forces](arcane-forces.md).

This catalog describes senior-caster capability. Every senior caster normally
has two or three persistent magical assistants with a bounded lesser repertoire
and ritual-support abilities. Assistants do not automatically inherit this
complete catalog, decisive senior spells, major ritual authority, or
unrestricted anchor control. The exact assistant-eligible spell list remains
content data.

## Utility first, and why

Magic should not do what guns already do. Humans are better at delivering damage
at range than anything the arcane can field, and a caster who is artillery is
both a worse gun and a less interesting unit.

The faction design principle asks for capabilities that create **different
operational problems** rather than different numbers. That points the spell set
at movement, information, denial, and terrain — the things a human force cannot
buy at any price.

**But casters must be dangerous.** A purely supportive caster is not worth
focusing fire on, and focusing casters is what the strain and breach mechanics
exist to reward. So the set is utility-dominant with a small number of
expensive, decisive options that make an unattended caster a mistake.

## The strain career

Strain accumulates as a caster works, and meditation sheds only part of it. What
remains is residual, and it rises with every cast.

So a caster has a **career** rather than a rotation. They can recover enough to
work again, repeatedly, but never back to where they started, and every cast
spends a little of the match's remaining safety permanently.

That shapes the whole set. The interesting decision is rarely *which* spell —
the situation usually decides that — but **when to spend, and how hard**.
Empowerment sharpens it further, since each cast is also a question of how much
health to pay for a better version of it now.

The cost gradient below is therefore the most important design axis in the
document.

## Denying the human picture

The sharpest lever, because human capability is information capability.

### Dampening field

An area in which **electronic** sensing and communication fail. Thermal optics
go dark, links inside it drop, the fused picture stops arriving, and command
bandwidth cannot reach anything within it.

It does **not** blind anyone. Human eyes work normally, as do arcane senses.
What it removes is precisely the apparatus humans built and nothing else.

- **Cost:** high, and it is stationary once placed.
- **Counterplay:** it is a *place*, so it can be walked around, and everything
  inside it can still see and shoot.
- **The catch worth designing for:** casting one announces that something here
  matters. A dampening field is a flag planted over whatever it conceals, and a
  competent opponent reads it as one.

That last property is what keeps it from being an unconditional answer to the
human faction. It buys concealment at the price of attention.

### Obscuration

Fog, dust, or darkness that blocks **optical** line of sight, affecting everyone
including the caster's own side. Cheap, common, and the ordinary tool for
crossing open ground against a force that shoots better than you.

It interacts with the existing spatial model rather than overriding it: it
changes what a trace crosses, and the spatial revision updates as it would for
smoke.

## Shaping the ground

The arcane fight in geography. Two spells make that literal, and both work
through machinery the spatial model already has.

### Barrier

A created obstruction — **a semantic edge that did not exist a moment ago.**

Because edges declare permeability per modality, a barrier is not one thing:

| Form | Movement | Sight | Traces |
|---|---|---|---|
| **Wall** | blocked | blocked | blocked |
| **Screen** | passable | blocked | passable |
| **Ward** | passable | clear | **blocked** |

The third is the distinctive one. A ward that stops projectiles but not people
or vision produces tactical shapes nothing else in the game can make: infantry
walk through it, bullets do not, and both sides can see each other the whole
time.

Barriers are destructible under the ordinary rules, so breaching one is the same
action as breaching a wall, and they advance the spatial revision like any other
terrain change.

- **Cost:** moderate, scaling with extent and duration.
- **Counterplay:** break it, go around it, or wait — duration is finite and
  empowering duration is expensive.

### Rupture

The offensive spell, and it is aimed at **terrain rather than people**.

Against a force that fights from prepared positions and cover, removing the
cover is worth more than direct damage — and it is something no human weapon
does as decisively. Rupture collapses structures, destroys cover, and opens
routes, with casualties as a consequence of the collapse rather than the point
of it.

- **Cost:** high in both health and strain. This is a caster spending part of
  their career.
- **Counterplay:** fight in the open where there is nothing to collapse, or
  reach the caster during the cast, which is slow and interruptible.

This is the spell that makes an unattended caster a catastrophe, which is what
justifies the risk of closing with one.

## Moving

### Translocation

Constrained short-range teleportation, and the guardrail names it explicitly as
requiring definition.

- **Range:** short, on the order of a tactical bound rather than a redeployment.
- **Knowledge requirement:** the caster must have **observed the destination**.
  It cannot be used to arrive somewhere unseen.
- **Timing:** slow to prepare, and the preparation is observable.
- **Failure:** a blocked or occupied destination fails the cast, with the
  failure damage the magic system already defines.
- **Counterplay:** the preparation is the window. Interrupting a translocation
  is the same as interrupting any committed action.

It exists to answer human ranged dominance — crossing a beaten zone that cannot
be crossed on foot — and not to make casters unpinnable. A caster who
translocates has spent time, health, strain, and their position is now known
because somebody watched them arrive.

## Seeing

### Scrying

Remote observation of a place the caster has knowledge of.

Consistent with arcane sensing generally, it returns **presence and disposition
rather than identification**: how many, roughly where, roughly what kind. It
does not read equipment, faces, or intent.

- **Timing:** it is a **snapshot, not a feed.** The caster concentrates, and
  receives one picture.
- **Cost:** moderate, and the caster is stationary and exposed throughout.
- **Counterplay:** it is aged the moment it arrives, and a force that moves
  after being scryed has already invalidated it.

This is the arcane answer to the drone, and it is deliberately worse: cheaper to
field, far less current, and it cannot be left watching.

## Sustaining the mass

The faction is scarce casters and durable mass, so spells that serve the mass
are natural. They should follow the discipline already applied to leader
effects: **conditional behavioural effects rather than broad stackable stat
auras.**

### Mending

Accelerates the regeneration the heavier nonmagical peoples already possess. It
does not create healing where none exists; it makes an existing recovery faster.

This is the faction's answer to attrition, and it is why arcane forces recover
during a match while humans stabilise and evacuate.

- **Cost:** low per application, but it competes for the caster's career with
  everything else.
- **Counterplay:** damage faster than it mends, or kill the caster, or use
  effects regeneration does not answer.

## The cost gradient

| Spell | Health | Strain | Cast time | Frequency in a match |
|---|---|---|---|---|
| Obscuration | low | low | short | often |
| Mending | low | low | short | often |
| Barrier | moderate | moderate | moderate | several |
| Scrying | moderate | moderate | long | a few |
| Translocation | moderate | high | long | rarely |
| Dampening field | high | high | long | once or twice |
| Rupture | high | very high | long | once, decisively |

The shape matters more than the values: a caster can sustain the top of this
table indefinitely and the bottom of it almost not at all, so the decisive
spells are decisions about the whole match rather than the current moment.

## What is deliberately excluded

- **Mind control.** The guardrail names it, and it is incoherent with the
  control architecture: every unit is driven by its owner's module instance, and
  a spell that overrides that would require a privileged path the design does
  not have.
- **Resurrection and reanimation.** That is an undead faction's identity, not
  this one's.
- **Summoning.** Creating new personnel remains a candidate rather than an
  initial spell. Neither canonical portal ritual is summoning: a transit portal
  moves precommitted owned assets, while a goblin portal opens a route for
  unaffiliated goblins who remain hostile to every side. Neither creates
  personnel under arcane control.
- **Counter-magic.** Deferred until there is a second magical faction for it to
  matter against.

## What canonical status covers

Settled: a utility-dominant set with a small number of expensive decisive
options, and the reasoning that a caster who is artillery is a worse gun than a
gun while a purely supportive caster is not worth focusing; the strain career as
the axis the set is built on, so that the cost gradient's shape matters more
than its values; barrier as a created semantic edge expressed through
per-modality permeability; rupture aimed at terrain rather than people; the
dampening field attacking electronics only and announcing its own importance;
translocation requiring an observed destination; scrying as a snapshot returning
presence rather than identification; and the exclusions, mind control as
incoherent with the control architecture and true summoning as a candidate
rather than a rejection. Rituals are a distinct cooperative surface governed by
the arcane faction contract rather than additions to this individual spell list.

Not settled: every number, which aspects each spell permits empowering, and the
open parameters at the end.

**What would force a revision of the shape:** the decisive spells proving
correct as opening moves rather than as commitments, which would mean the cost
gradient is not steep enough to make a career meaningful; or the dampening field
proving an unconditional answer to the human faction despite announcing itself.

## Checking against invariant 13

The risk concentrates in two places.

**The dampening field** could be an unconditional answer to the human faction if
it were cheap or mobile. It is neither, and it announces its own importance,
which is what keeps casting one a decision rather than an opening move.

**Rupture** could dominate if cover removal were always correct. It is bounded
by being a large fraction of a caster's career: using it early means not having
it later, and a caster who has spent it is a much less frightening object for
the rest of the match.

Everything above the middle of the cost gradient is cheap enough to be
situational rather than decisive, which is the intended shape.

## Open parameters

- All values, as ever.
- Which aspects each spell permits empowering, and at what rates.
- Dampening field radius, duration, and whether it degrades or hard-cuts.
- Whether a ward is one-directional, which is powerful and may be too much.
- Translocation range, and whether a squad can be moved or only a caster.
- Scrying range, and whether it requires an anchor at the destination.
- Whether mending works on casters. Meditation is now the intended route back
  to safety, which makes healing-as-margin-restoration less necessary and the
  laundering loop easier to close.
