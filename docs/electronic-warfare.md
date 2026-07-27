---
title: S.I.R. Electronic Warfare Architecture
status: proposed
document-type: living-design
version: "0.1"
last-updated: 2026-07-27
related:
  - docs/game-vision.md
  - docs/wasm-control-architecture.md
  - docs/control-abi.md
  - docs/setting-and-factions.md
---

# S.I.R. Electronic Warfare Architecture

## Purpose

Electronic warfare is established as an important part of play, and as something
that must "affect gameplay at the command and information layers, not exist only
as a numeric combat modifier." Until now it had no model.

This document defines what emits, what can be attacked, what protects against
what, and what the counterplay is.

## Design position

**Electronic warfare attacks the command loop, not the units.**

Every other combat system in S.I.R. reduces a unit's ability to fight.
Electronic warfare reduces a commander's ability to *know* and to *direct*,
leaving the units entirely intact. A jammed squad is not weakened; it is
uninformed and unreachable, and it fights on under whatever intent it already
had.

That is the whole appeal, and it is also why EW must never be expressed as a
percentage penalty. A modifier on accuracy is not electronic warfare; it is a
debuff wearing its clothes.

## Four layers

Conflating these is the usual design failure. They are attacked differently,
defended differently, and detected differently.

```text
emission   what a transmitter radiates, whether or not anyone reads it
link       whether a message arrives, when, and intact
content    what the message says
bandwidth  how much command capacity reaches a unit
```

The fourth is specific to S.I.R. Because command bandwidth is drawn through the
communications topology, degrading a link degrades what a unit is told and what
analysis it can commission — see
[WebAssembly Control Architecture](wasm-control-architecture.md).

## Emission is a stimulus

**Transmitting is an observable act, and it uses the ordinary perception
pipeline.** A radio transmission is a stimulus exactly as a muzzle flash is a
stimulus: it has an origin, a strength, a duration, and a modality, and it
enters `geometry → stimulus → acquisition → reaction` with no parallel
machinery.

This unification matters. It means emissions obey the same knowledge rules, the
same acquisition timing, the same decay, and the same reporting path as
everything else, and that an electronic contact is an ordinary contact whose
sensor happened to be a receiver.

Consequences that follow without further rules:

- **listening is free, transmitting is not.** A passive receiver emits nothing
  and cannot be found by these means;
- an emission gives **bearing** cheaply and **position** only through
  triangulation from several observers, or through proximity;
- emission detection accumulates like any other acquisition, so brief
  transmissions are harder to localise than sustained ones; and
- a jammer, which by definition radiates continuously and powerfully, is the
  most conspicuous object on the battlefield.

That last point is the natural counterplay to jamming and requires no special
case.

## Three separable protections

The cleanest organising principle here, and the one that keeps the system
legible:

| You cannot hide | You can hide | You can prove |
|---|---|---|
| **that** you transmitted | **what** you said | **who** you are |
| countered by emission control | countered by encryption | countered by authentication |

Encryption does not reduce detectability. Emission control does not protect
content. Authentication does neither, and prevents something else entirely.
Players who conflate them will be punished in a way they can understand
afterwards, which is the standard the design holds every decisive mechanic to.

## Attacks

### Jamming

An emitter that degrades links within an area or along a bearing. Effects are
drawn from a declared subset — the established rule is that not every capability
produces every effect:

- reduced effective range;
- increased message delay;
- probabilistic loss;
- severed link; and
- reduced command bandwidth throughput to the affected units.

Jamming is loud, continuous, and locatable. It also degrades traffic
indiscriminately within its footprint unless the capability declares otherwise,
so a jammer sited badly jams its own side.

### Direction finding

Passive and cumulative. Requires the target to transmit, produces a bearing, and
resolves to a position through multiple observers or sustained emission. It
feeds the ordinary acquisition and reporting model, so a direction-finding
contact is reported, ages, and goes stale like any other.

This is the mechanism that makes emission control a real decision rather than
flavour.

### Interception

Passive. Yields the **envelope** whenever the transmission is detected, and the
**payload** only under declared conditions — unencrypted traffic, broken
protection, or a captured key.

The envelope alone is valuable: sender, recipient or route, timing, volume, and
position. That is traffic analysis, and it works on encrypted traffic.

### Traffic analysis

Because module-to-module payloads are **opaque player-defined bytes**, content
interception hands an opponent bytes they cannot necessarily interpret. This is
not a gap in the design; it is the interesting case.

What survives encryption and opacity is the *pattern*: who transmits, to whom,
how often, how much, and when it changes. A squad that goes from silence to
heavy traffic has made contact. A network whose traffic converges on one node
has revealed its command topology. A sudden force-wide transmission is an order
going out.

Traffic analysis should therefore be a first-class capability rather than a
consolation prize, and it is the one form of interception that always works.

### Injection

Inserting a message that appears legitimate. It must satisfy the established
constraint that "false information must enter through the same observation and
report model as true information," and that the server never falsifies a
player's picture outside that model.

So an injected message is a real message with false content and forged
provenance, travelling the ordinary path. It can be:

- refused by authentication where that exists;
- detected by a module or a human noticing inconsistency; and
- attributed afterwards in replay, so a decisive deception is explainable.

Forging requires knowing enough to forge — the sender identity, the route, the
protocol. That is itself an intelligence requirement, which keeps injection at
the end of a chain of work rather than being a first-contact ability.

## Intercepted intelligence inherits belief, not truth

**What you intercept is what the opponent said, which is not necessarily true.**

An intercepted contact report carries the observation time, provenance, and
error of the original. Intercepting a stale report gives stale information.
Intercepting a mistaken report gives a mistake. Intercepting a deception aimed
at someone else gives the deception.

This falls directly out of the three-tier knowledge model and needs no
additional rule, but it deserves stating because it is what stops interception
from becoming an oracle. Electronic warfare moves *beliefs* between actors. It
never reads world truth.

## Command bandwidth degradation

Jamming a squad reduces the command bandwidth that reaches it. Under the control
architecture this means its units are **less well informed and can commission
less analysis**, while continuing to run their control logic every tick at full
rate.

The distinction matters and is worth stating plainly: **a jammed force does not
think less often, it thinks with worse information.** Its modules keep running,
its reactions keep their timing, and its units keep fighting. What degrades is
the quality of the picture they and their commander are working from.

## Counterplay

Every major capability needs recognisable evidence and at least one practical
response. For each attack:

| Attack | Responses |
|---|---|
| Jamming | Locate and destroy the emitter; route around it; relay past it; accept isolation and operate on intent |
| Direction finding | Emission control; brief transmissions; directional equipment; transmit from positions you are leaving |
| Interception | Encryption; brevity; pre-arranged codes that carry meaning in few bytes |
| Traffic analysis | Constant-rate traffic that hides real volume, at a bandwidth cost; decoy emitters |
| Injection | Authentication; corroboration before acting; doctrine that treats unexpected orders with suspicion |

Two responses deserve emphasis because they are structural rather than
equipment.

**Relays.** A relay extends or restores a path at the cost of a physical asset
placed somewhere exposed. This answers the vision's open question about whether
communications can be relayed through units, vehicles, infrastructure, drones,
or deployable equipment: **yes, and a relay is an authoritative object with a
position that can be found, jammed, and destroyed.** A relay chain is a supply
line for information, with the same vulnerabilities.

**Physical courier.** A unit carrying a message emits nothing and cannot be
jammed or intercepted, at the cost of travel time and the risk to the carrier.
It is the fallback that always exists, and it is why total communications denial
degrades rather than eliminates command.

## The connection to player-authored control

This is where electronic warfare earns its place in *this* game specifically.

A player whose units depend on continuous direction is devastated by jamming. A
player whose control logic can pursue intent without supervision is
inconvenienced by it.

**Electronic warfare is therefore the mechanic that most directly rewards good
module authorship**, and it does so without giving better code better
information, better budgets, or better timing. It rewards the specific quality
of operating well when unsupervised, which is exactly the capability the
control architecture exists to enable and which nothing else in the design puts
under pressure.

It also gives the standard module a clear design target: it must be competent
while disconnected, because a player who writes no module will be jammed too.

## Faction variation

Portal-origin factions may communicate by means human electronic warfare cannot
touch. That is legitimate asymmetry, subject to the standing guardrail that
every major capability needs evidence and a response.

A magical command network still needs declared range, capacity, latency, an
observable signature, a disruption mechanism, and an anchor or dependency that
can be attacked. It does not need to be attacked by *the same* equipment.
Whether human EW can degrade magical links at all, and what the human answer to
an unjammable network is, remain open.

The reverse also holds: an arcane force facing a human network needs some way to
contest it, or human communications become an unassailable advantage.

## Failure modes to avoid

- Electronic warfare expressed as an accuracy or damage modifier.
- Jamming that cannot be located, which removes the counterplay entirely.
- Interception that reveals authoritative world state rather than what an
  opponent believed.
- Injected information the receiving player has no possible way to doubt.
- Server-side falsification outside the observation and report model, which is
  indistinguishable from a bug.
- Encryption that also prevents direction finding, collapsing three separable
  protections into one purchase.
- Total denial with no fallback, which produces a helpless force rather than an
  isolated one.

## Open parameters

- Emission strength, band, and directionality as content values.
- Detection, bearing accuracy, and triangulation rules for emissions.
- Jamming footprint shapes, and whether jamming is omnidirectional or sectored.
- Whether jamming affects its own side, and how strongly.
- Encryption strength as a discrete tier or a time-to-break.
- Whether keys can be captured from equipment or casualties.
- Authentication cost and whether it is per-message or per-link.
- Relay capacity, chaining limits, and setup time.
- How much command bandwidth a given degradation removes.
- Whether traffic analysis is a passive capability, an active analysis
  commissioned through host services, or both.
- The threshold at which a module can detect that it is being jammed rather than
  merely receiving nothing.
