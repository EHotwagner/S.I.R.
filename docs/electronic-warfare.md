---
title: Electronic Warfare
category: Battlefield Systems
categoryindex: 4
index: 15
status: accepted
decision-status: canonical
document-type: living-design
version: "1.2"
last-updated: 2026-07-28
related:
  - docs/game-vision.md
  - docs/communications-network.md
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
what, and what the counterplay is. The links it attacks are defined in
[Communications Network Architecture](communications-network.md).

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

## Three layers

Conflating these is the usual design failure. They are attacked differently,
defended differently, and detected differently.

```text
emission   what a transmitter radiates, whether or not anyone reads it
link       whether a message arrives, when, and intact
bandwidth  how much command capacity reaches a unit
```

There is deliberately no content layer.

The third is specific to S.I.R. Because command bandwidth is drawn through the
communications topology, degrading a link degrades what a unit is told and what
analysis it can commission — see
[WebAssembly Control Architecture](wasm-control-architecture.md).

## Why there is no content layer

Reading and forging enemy traffic are not modelled. Both were considered and
both are rejected, for two independent reasons.

**They are structurally impossible.** Module-to-module payloads are
player-defined opaque bytes. An intercepted message is bytes in a protocol the
opponent does not know, encrypted or not, and forging one means writing valid
traffic in that same unknown protocol. Neither is a puzzle a player can solve
inside a twenty-minute match, and neither should be.

**They would collapse into drudgery even if they worked.** A capability whose
counter is cheap and universally available is not a decision. If interception
threatened and encryption were purchasable, every competent player would encrypt
every time, the mechanic would never fire, and the only lasting effect would be
a box everyone remembers to tick. The same argument applies to authentication
against forgery.

In the setting, traffic is encrypted and authenticated as a matter of course.
This is a near-future military; protecting a radio link is not a tactical
decision anyone makes. It is assumed, and therefore not modelled.

What this leaves is the interesting half. Everything electronic warfare can
still do operates on emission and link, and none of it can be bought off.

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

## There is one protection, and it is tactical

**Emission control is the only defence, and it cannot be purchased.**

The one thing an opponent can always learn is that you transmitted, from where,
and how much. No equipment prevents it. The only way not to be found by these
means is not to transmit, and that costs contact.

This is what makes electronic warfare a live tactical question rather than an
equipment checklist. A player cannot spend their way out of it before the match.
They decide, continuously and under pressure, whether this report is worth being
located for.

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

### Traffic analysis

The primary intelligence capability, and the one that always works.

An observer learns the **envelope** of every transmission it detects: origin,
approximate strength, duration, and timing. From a pattern of envelopes it
learns considerably more:

- a squad going from silence to heavy traffic **has made contact**;
- traffic converging on one node **reveals command topology**, and therefore
  which unit is worth killing;
- a sudden force-wide transmission **is an order going out**, a warning that
  something is about to change;
- a burst far larger than a unit's ordinary traffic **is a reconnection**, and
  announces a squad that has just regained contact; and
- silence where there was traffic **is a casualty, a displacement, or a
  deliberate choice**, and telling which is the analyst's problem.

None of this requires reading a single byte, and none of it can be encrypted
away. Traffic analysis is a first-class capability rather than a consolation
prize for failing to break a cipher.

### Deception at the emission layer

Deception survives, but it operates on emissions rather than on content. It
needs no knowledge of an opponent's protocol and cannot be defeated by
protecting one.

- a **decoy emitter** is a deployable object that transmits, producing a false
  electronic contact where nothing important is;
- a **displaced transmission** is made from a position the unit is leaving, so
  the opponent's fix is real and already worthless; and
- a **replayed emission** retransmits bytes captured earlier. The opponent's
  units will discard it, but to anyone watching the spectrum it is
  indistinguishable from genuine traffic.

All three attack the opponent's *analysis* rather than their message handling.
A decoy is an authoritative object with a position, so the counterplay is the
ordinary one: find it, observe that nothing else is there, and learn that this
opponent uses decoys.

## Command bandwidth degradation

Jamming a squad reduces the command bandwidth that reaches it. Under the control
architecture this means its units are **less well informed and can commission
less analysis**, while continuing to run their control logic every tick at full
rate.

The distinction matters and is worth stating plainly: **a jammed force does not
think less often, it thinks with worse information.** Its modules keep running,
its reactions keep their timing, and its units keep fighting. What degrades is
the quality of the picture they and their commander are working from.

### The reverse direction

Bandwidth also flows the other way as a source of intelligence. Because an
allocation is carried as traffic over links, **a supported unit emits in
proportion to the support it receives** — see
[Communications Network](communications-network.md).

An opponent watching the spectrum therefore learns not only where a force is,
but **where its commander is looking**. A squad whose traffic rises has just
been given attention, and that is a leading indicator of intent rather than a
report of something already done.

This closes the loop between the two systems. Electronic warfare degrades an
opponent's command bandwidth, and an opponent's command bandwidth tells you
where to point your electronic warfare.

## Tactics this produces

Three consequences emerge from rules written separately, none of which
anticipated them. They are recorded because they are the evidence that the model
is load-bearing rather than decorative.

### The scout's dilemma

Electronic and visual concealment are independent. A reconnaissance element can
be perfectly hidden and electronically loud, so **finding something and telling
anyone are separate acts with separate costs.** A scout that reports is located;
a scout that stays silent is useless.

This is the sharpest available expression of the information-versus-exposure
tension the design is built around, and it required no mechanic of its own.

### Jamming to force a transmission

Store-and-forward guarantees that a squad which has lost contact will burst-
transmit the moment it regains it. So an attacker can jam, wait for the queue to
fill, then **deliberately lift the jamming** and direction-find the burst.

Jamming becomes a tool for *creating* an emission rather than suppressing one.
Staggering reconnections is the counter, which is why it appears in the
counterplay table.

### Jamming as an attack on the opponent's economy

If mid-mission reinforcement is paid for, as
[Stakes and Reinforcement](stakes-and-reinforcement.md) proposes, then jamming
acquires a third target beyond information and control.

A squad that has been cut off and a squad that has been destroyed both reach
their commander as silence. An attacker who jams an intact element, and waits,
may induce that commander to spend capital and stake replacing a force that is
still fighting.

This is worth recording because nothing else in the design attacks an opponent's
economy directly, and because it raises the stakes on an open parameter: **how
reliably interference can be distinguished from absence** decides whether the
victim is making an inference or guessing.

### Attention as a targeting cue

Traffic analysis identifies the node a network converges on, which is a command
element. Command elements carry the sets that reach the command net, which are
the most powerful and therefore the most detectable sets on the field. A
commander who supports that element heavily makes it louder still.

Locating and killing it forces succession under the established rules, and
succession does not create a communications device. The chain from traffic
analysis to a decapitated and disconnected squad runs entirely through
mechanisms that already existed.

This chain is also what stops hierarchy from being unconditionally correct. A
force spread flat across one net presents no convergence node and therefore no
decapitation target, at the cost of a contended net and a force-wide emission
footprint that can be mapped. Neither shape dominates, which is what makes the
decision a decision — see
[Communications Network](communications-network.md).

## Counterplay

Every major capability needs recognisable evidence and at least one practical
response. For each attack:

| Attack | Responses |
|---|---|
| Jamming | Locate and destroy the emitter; route around it; relay past it; accept isolation and operate on intent |
| Direction finding | Emission control; brief transmissions; directional equipment; transmit from positions you are leaving |
| Traffic analysis | Constant-rate traffic that hides real volume, at a bandwidth cost; decoy emitters; staggering reconnections |
| Deception | Corroborate an electronic contact before acting on it; a decoy emits and does nothing else |

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
The canonical arcane anchor network cannot be degraded by human EW. Whether
other magical factions expose an electronic or magical disruption surface
remains open.

For the canonical arcane anchor network, legitimate unit observations and
status reach the controlling caster on the next tick and caster commands reach
anchored subordinates after a flat 20 ticks. It has no relay-hop latency and
emits no radio traffic. Humans contest that advantage by finding and destroying
anchors and casters, exploiting overload and breach signatures, and using the
greater precision of electronic and optical sensing.

The reverse also holds: an arcane force facing a human network needs some way to
contest it, or human communications become an unassailable advantage.

## Failure modes to avoid

- Electronic warfare expressed as an accuracy or damage modifier.
- Jamming that cannot be located, which removes the counterplay entirely.
- Reintroducing content interception or forgery. Both are structurally
  impossible against opaque player-defined payloads, and both would collapse
  into a universally purchased counter that never fires.
- Any protection that can be bought once before a match and then forgotten.
  Emission control is the only defence precisely because it must be exercised
  continuously.
- Server-side falsification outside the observation and report model, which is
  indistinguishable from a bug.
- Total denial with no fallback, which produces a helpless force rather than an
  isolated one.

## Open parameters

- Emission strength, band, and directionality as content values.
- Detection, bearing accuracy, and triangulation rules for emissions.
- Jamming footprint shapes, and whether jamming is omnidirectional or sectored.
- Whether jamming affects its own side, and how strongly.
- Decoy emitter cost, duration, and how convincingly it mimics a real set.
- Whether a captured device lets its holder emit on an opponent's net.
- Relay capacity, chaining limits, and setup time.
- How much command bandwidth a given degradation removes.
- Whether traffic analysis is a passive capability, an active analysis
  commissioned through host services, or both.
- The threshold at which a module can detect that it is being jammed rather than
  merely receiving nothing. This is load-bearing rather than incidental: it
  determines whether a commander can distinguish an isolated squad from a
  destroyed one.
