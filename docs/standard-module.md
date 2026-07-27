---
title: S.I.R. Standard Module and Commanding Without Code
status: proposed
document-type: living-design
version: "0.2"
last-updated: 2026-07-27
related:
  - docs/game-vision.md
  - docs/wasm-control-architecture.md
  - docs/control-abi.md
  - docs/electronic-warfare.md
  - docs/formations-and-referents.md
---

# S.I.R. Standard Module and Commanding Without Code

## Purpose

Writing a control module is not required to play. When standing doctrine was
removed from the engine, that requirement moved onto the **standard module being
configurable**, and it has since accumulated three further roles: the balance
baseline, competence while disconnected, and continuous emission discipline.

It is referenced in five documents and specified in none. This document defines
how a player who writes no code commands a force.

## The constraint

The accepted guardrail is explicit: an unrestricted visual-programming language
in the canonical client "could turn pre-match preparation into programming
homework," and what is wanted instead is "strong defaults, concise doctrine
controls, and progressive disclosure."

So the target is not a friendlier way to write rules. It is a way to command
that never looks like authoring rules at all.

## The unit of intent is a posture, assigned to a squad

A player does not configure units, and does not write conditions. They assign
each squad a **posture**: a named, coherent bundle of behaviour that reads as an
order rather than a configuration.

```text
2nd Squad     ADVANCE TO CONTACT      emission restricted
3rd Squad     SCREEN                  emission silent
Support Elt   SUPPORT BY FIRE         covering the eastern approach
```

This is the whole top-level interface, and it is deliberately small. Ten squads
is ten decisions, not a hundred units times a dozen rules.

It also matches how command actually works. A commander does not enumerate
conditions; they say what a subordinate element is *for* right now, and trust it
to work out the rest. A posture is that statement, and the standard module is
the subordinate working it out.

## What a posture bundles

Selecting one posture sets coherent values across every system at once:

| System | What the posture decides |
|---|---|
| Engagement | What to engage, at what confidence, and whether to hold fire |
| Movement | Speed against readiness, and willingness to break contact |
| Perception | Where attention is directed, and whether to observe deliberately |
| Formation | Which template, and how strictly to hold it |
| Emission | Free, restricted, or silent by default |
| Logistics | Ammunition reserve and resupply threshold |
| Casualties | Whether to recover under fire or mark and continue |
| Isolation | What to do when contact with headquarters is lost |

The bundling is the point. These settings are not independent in practice — a
screening element that transmits freely is not screening — and letting a player
combine them arbitrarily invites incoherent combinations that are nobody's
intent.

## Candidate posture catalog

Names and exact behaviour are content, and this is a starting set rather than a
fixed roster:

- **Assault** — press the objective, engage on sight, accept casualties,
  emission free, do not stop to recover casualties.
- **Advance to contact** — move deliberately and ready, report on contact, do
  not commit, emission restricted.
- **Hold** — occupy and defend an area, engage within an assigned sector, do not
  pursue.
- **Support by fire** — hold an area engagement covering a named approach,
  shift or lift on order.
- **Screen** — observe and report, avoid contact, emission silent, withdraw if
  pressed.
- **Withdraw** — break contact and move to a referent, engage only to disengage.
- **Reserve** — hold position quietly, minimal emission, ready to move.

Each is a legitimate answer to "what is this squad for," and none requires the
player to know what a condition is.

## Progressive disclosure: the overrides

Beneath the posture sits a short list of named overrides. They are the settings
a commander genuinely varies, and there are deliberately few:

- **engagement release** — hold fire until a stated trigger, such as a target
  count, a range, or an order;
- **withdrawal condition** — casualties, ammunition, suppression, or isolation
  crossing a threshold;
- **emission policy** — overriding the posture default;
- **casualty policy** — recover, recover if it can be done under cover, or mark
  and continue;
- **ammunition reserve** — the level below which a unit stops offensive fire;
  and
- **isolation behaviour** — see below.

These are **named fields, not an ordered rule list.** That matters: the
ordered-list starvation failure recorded in the reference research, where a
high-priority always-true rule silently strands everything beneath it, cannot
occur in a structure that has no ordering.

## Isolation behaviour is a player decision

Every posture declares what its squad does when contact with headquarters is
lost, and this is exposed as an override because it is a genuine command
decision rather than a default anyone should inherit silently:

- **continue** — pursue the current intent to completion;
- **consolidate** — secure what has been gained and hold; or
- **withdraw** — fall back to a designated referent.

This is the standard module's answer to the vision's open question about what
authority a unit's control logic has over existing orders after communication is
lost. At the engine level the question remains open. At the player level it is
answered by putting the choice in the commander's hands before it matters, which
is where it belongs, because the decision cannot be communicated at the moment
it becomes relevant.

## Emission discipline

Because emission control is the only defence against being located, and because
it must be exercised continuously rather than purchased once, every posture
carries an emission default and the standard module honours it without being
told again.

A screening element defaults to silent and therefore reports only when its
findings justify being found. An assaulting element defaults to free, because
it has already been located by other means and the marginal cost is low.

This is the clearest case of a posture encoding tactical judgement a
non-programming player should not have to reconstruct.

## How a posture reaches the module

Postures are not compiled into the standard module's artifact. They cannot be:
the artifact is immutable and shared by every unit assigned to it, so a posture
baked into it would be the same posture for every player and every squad.

A posture is **instance configuration** — per-unit data supplied with the
artifact assignment, opaque to the server, locked when the match begins, and
carried in the force snapshot and the replay. The mission lifecycle already
required this under the name *initial policies*; it now has a definition.

The consequence for the client is worth stating. Because the server does not
interpret configuration, **the client knows what it ordered rather than what a
squad currently holds.** A player whose re-tasking order was jammed believes
something false about their own force, which is the same fog applied everywhere
else and must not be smoothed over by displaying an intended posture as a
confirmed one.

## Pre-match preparation is free; in-match change is not

Postures and overrides set **before deployment** cost nothing. They are part of
force preparation, like loadout.

Changing them **during a match** is an order. It travels the communications
topology, it can be delayed, degraded, or prevented, and issuing it produces
traffic that can be seen.

This distinction is worth stating plainly because it makes preparation
genuinely valuable rather than merely convenient. A well-configured force is
resilient to command disruption; a force that needs constant re-tasking is
brittle against an opponent who jams it. That is the same thesis the control
architecture rests on, expressed for players who write no code.

## This is not the doctrine vocabulary returning

The distinction matters and will be asked about.

Standing doctrine was **engine machinery**: a published condition vocabulary the
authoritative server evaluated on every unit's behalf, with rule ordering,
unreachability validation, and a delegation contract, all of it versioned as
part of the ABI.

Postures are **content inside one module**. The engine does not know they exist.
The standard module reads its own configuration and issues ordinary ABI requests
exactly as any player-written module does, and it holds no privilege of any
kind. The configuration schema is published and versioned as standard-module
content, so it can change without touching the contract.

The practical test: a third party could write a module with an entirely
different configuration model, ship it, and the engine would neither know nor
care.

## The balance baseline

Because most units will run the standard module most of the time, its behaviour
*is* the game's balance for most players. Two consequences:

- posture behaviour is tuned content and must be versioned with the ruleset, so
  a match can state which behaviour it was played under; and
- a change to a posture is a balance change, and should be treated with the same
  seriousness as changing a weapon.

The standard module is competitive baseline, not sample code. A custom module
should be able to beat it through better judgement, and should not be able to
beat it merely because the standard module is careless.

## Relationship to custom modules

Module artifacts are assigned per unit, so the two coexist without special
handling. A player may run the standard module across a whole force, replace it
on a single squad, or mix freely.

A custom module ignores postures entirely. It receives the same events, issues
the same requests, and gets no additional information and no additional budget.

## Failure modes to avoid

- A configuration surface that grows until it is a rule editor, at which point
  the guardrail against programming homework has been lost by degrees.
- Ordered or prioritised settings anywhere, which reintroduces the starvation
  trap that ordering always brings.
- Postures that are strictly ranked, so that one is simply the best and the
  choice is not a choice.
- A standard module weak enough that custom code wins by default, which would
  make authoring mandatory in practice while the documents claim otherwise.
- Overrides that can express incoherent combinations the posture existed to
  prevent.
- Silent defaults for isolation behaviour, which decides a squad's fate at the
  moment the player can no longer influence it.

## Open parameters

- The final posture roster and the exact behaviour each encodes.
- Whether postures are per-squad only, or can be assigned to a whole force or an
  individual unit.
- Whether the player may author a custom posture from overrides, and whether
  that is still meaningfully not-programming.
- How a posture change in flight interacts with an action already committed.
- Whether posture is visible to an opponent through observed behaviour, and how
  quickly.
- The configuration schema's versioning and compatibility policy.
- Whether the canonical client presents postures per squad, per element, or on
  the map itself.
