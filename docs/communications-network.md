---
title: S.I.R. Communications Network Architecture
status: accepted
decision-status: canonical
document-type: living-design
version: "1.1"
last-updated: 2026-07-27
related:
  - docs/game-vision.md
  - docs/electronic-warfare.md
  - docs/wasm-control-architecture.md
  - docs/research/squad-command-and-succession.md
---

# S.I.R. Communications Network Architecture

## Purpose

The vision establishes that communication range constrains members to their
leader and leaders to headquarters, that modules cannot use an out-of-world
backchannel, and that connectivity depends on a physical device. It does not
define what a link *is*.

[Electronic Warfare](electronic-warfare.md) was written against that gap: it
attacks links, degrades their range, delays their traffic, and reduces the
command bandwidth flowing along them, without the underlying model existing.
This document supplies it.

## Nets and topology are different things

The single most useful distinction here, and one the vision does not yet draw:

- a **net** is a set of participants sharing a channel. It describes who *can*
  hear whom.
- a **topology** is the organisational structure of command. It describes who
  reports to whom.

Radio is not naturally hierarchical. Everyone on a net hears everyone on that
net who is within reach. The hierarchy is a property of the organisation using
the net, not of the medium.

The canonical arrangement is two nets:

```text
command net    headquarters + squad leaders
squad net      one per squad: its members and its leader
```

A squad leader participates in both, which is what makes leadership a
communications position and not merely a title. Losing the leader severs the
squad from the command net even though the squad net survives — the members can
still hear one another, and cannot reach headquarters.

This preserves every established rule while allowing lateral traffic within a
squad, which the star topology implied but never granted.

## When a link exists

A link exists between two participants on the same net when a **signal path**
connects them. The path is evaluated with the same spatial model everything else
uses: cells, semantic edges, and levels.

```text
effective range = device range
                - attenuation from obstructions along the path
                - degradation from jamming and environment
```

Obstruction is computed with the same edge-aware trace as line of sight, and
this is deliberate. Rather than introducing a parallel propagation model, a
signal path crosses the same cells and edges a shot would, and each contributes
attenuation according to its material. A thin partition costs little; masonry
costs a great deal.

Three consequences follow, and all three are wanted:

- **positioning affects communications as it affects everything else.** A squad
  that moves into a basement loses contact for reasons the player can see on the
  map.
- **elevation is a communications asset.** A unit on a roof has clear paths to
  more of the battlefield, which gives high ground a second kind of value beyond
  observation and fire.
- **a relay is a solution the player can reason about**, because the obstruction
  that broke the link is visible terrain rather than an invisible roll.

Attenuation values, and whether any material blocks outright rather than
attenuating, are prototype parameters.

## Range and signature are one property

A device's transmitting power determines both how far it reaches and how easily
it is found. These are not separate statistics to be balanced against each
other; they are the same property seen from two sides.

**A long-range set is a loud set.** Reaching headquarters from deep in a
building means transmitting hard enough that direction finding will resolve
your position. This is the tension that makes emission control a decision rather
than a preference, and it needs no additional rule — it follows from
[emission being a stimulus](electronic-warfare.md).

Directional equipment is the partial escape: it concentrates power along a
bearing, extending reach without broadcasting in every direction, at the cost of
needing to be pointed at something.

## Capacity, and where command bandwidth flows

A link has finite **throughput**. Command bandwidth is drawn through the
communications topology, so a squad's usable bandwidth is bounded by the
weakest link on the path from headquarters to it.

This produces the intended structure without further mechanism:

- a squad at the end of a long relay chain is worse informed than one beside
  headquarters, even with identical allocation;
- degrading any link in a chain degrades everything behind it; and
- restoring a better path improves a squad's picture without moving it.

Player-defined message payloads and authoritative report traffic share the same
capacity. A module that chatters heavily is spending the same resource its
reports need, which makes protocol design a real consideration rather than a
free-form channel.

### Bandwidth is carried as traffic, and traffic emits

Command bandwidth is not an abstract allowance. It is networked support
delivered over links, so an allocation is *carried* — as traffic, on the path
between headquarters and the unit, by devices that transmit.

**A heavily supported unit is therefore a conspicuous one.** Both ends of a
busy link emit in proportion to what is flowing over it, and every relay in a
chain emits as it forwards. Extending reach with relays extends the electronic
footprint at every hop.

What emits is the **transfer**, not the thinking. A unit's control logic runs on
the authoritative server every tick whether or not anything reaches it, and that
costs nothing and radiates nothing. A unit's own perception is likewise free and
silent. Only information arriving from elsewhere — a fused picture, a
commissioned analysis, an order, a report going out — crosses a link, and only
that produces an emission.

Three consequences follow, and the implementation needs all three:

- **attention has a signature.** The units a commander is watching most closely
  are the easiest to find, and a squad that has been cut loose goes quiet and
  blind together;
- **emission control costs allocation, not just reports.** A unit ordered to
  silence gives up the support flowing to it, so going dark is expensive in a
  way that is felt immediately rather than only when something is missed; and
- **a change in allocation is observable.** A sudden rise in a squad's traffic
  means a commander just began paying attention to it, which under traffic
  analysis is a leading indicator of intent — visible before the action it
  precedes.

This is self-balancing in a way worth stating. Attention is cheap to spend on a
unit already in contact and already located by other means, and expensive to
spend on a concealed one. Supporting a hidden reconnaissance element is exactly
the case where the cost bites hardest, which is correct.

## A net's capacity is shared

A link has finite throughput. **A net has finite throughput shared among
everyone on it.**

This is the mechanism that stops the command topology from being decorative.
Without it, nothing prevents a player from equipping every unit with a
command-net set, putting the whole force on one channel, and dissolving the
hierarchy the design assumes. With it, doing so degrades the net for everyone on
it, including the commander.

It is also how radio actually works, and it is why real militaries use nets
hierarchically rather than putting a battalion on one channel. The structure
exists because the medium is contended, not because someone preferred an
organisation chart.

## Signature is aggregate

A force is not only a set of individually locatable transmitters. **A force that
transmits everywhere can be mapped**, and what an opponent recovers is not a
list of positions but the shape of the network: how many participants, how
traffic is distributed, and where it concentrates.

Adding emitters therefore costs twice — each is individually findable, and
together they describe the force.

## Why neither network shape dominates

The two previous sections would, alone, make hierarchy unconditionally correct.
It is not, and the reason matters.

Traffic analysis finds the node a network converges on. In a hierarchical net
that node is a command element, and killing it forces succession and severs a
squad from headquarters — the chain recorded in
[Electronic Warfare](electronic-warfare.md). **A flat network has no convergence
node**, and therefore presents no decapitation target.

So the shapes trade genuinely:

| | Hierarchical | Flat |
|---|---|---|
| Net capacity | efficient, few participants per net | contended, degrades for everyone |
| Emissions | fewer, concentrated | many, force-wide |
| Legibility | topology is readable, hubs identifiable | structure visible, no obvious hub |
| Decapitation | one death isolates a squad | no single target worth the effort |

Neither is correct in general. Which is better depends on the mission, the map,
the size of the force, and whether the opponent is equipped to exploit
concentration or volume. That is the condition a real choice has to meet.

### Topology is not configured, it is equipped

A player does not set a network shape. They decide **who carries what**, and the
shape follows. A device that reaches the command net is heavier, louder, and
competes for carrying capacity under the ordinary logistics model, so the number
of units on that net is limited by things a commander already has to weigh.

This keeps the decision inside a system that exists rather than adding a
separate configuration surface, and it means the answer can differ per squad
within one force.

## Upstream and downstream are not symmetric

Traffic in the two directions is bounded differently, and the difference is a
design position rather than an accident.

**Downstream** — the fused picture and commissioned analysis flowing to a unit —
is drawn from allocated command bandwidth. It is discretionary support a
commander chooses to spend, and spending it somewhere means not spending it
elsewhere.

**Upstream** — reports flowing to a commander — is not allocated. It is bounded
by link capacity, by aggregation at each hop, and by emission.

The principle is that **a commander allocates what they give, not what they are
told.** Rationing how much a subordinate element may report would be strange in
the fiction and hostile in play, and it would replace an interesting constraint
with an administrative one.

### Why the asymmetry is correct rather than merely kind

A unit that receives less still fights. Its own perception is its floor, it
runs its control logic every tick, and its reactions are unaffected. Degrading
downstream support degrades its *judgement*, not its existence.

A commander who receives less has nothing at all. They have no perception of
their own; their entire faculty is derived from what reaches them. Fog of war is
supposed to make information contested, uncertain, and late — **not to remove
the player's ability to decide anything.** A player with no information has no
decisions, and that is idleness rather than tension.

The volume argument points the same way. Aggregation compresses upstream traffic
at every hop, so a squad's reporting arrives as one summary rather than a dozen
feeds. A fused downstream picture has no equivalent compression, and is the
larger of the two.

### The constraint that remains

Upstream is not free. **Reporting emits**, so a talkative element is a findable
one, and that bound cannot be purchased around. Invariant 13 therefore still
holds: reporting everything is not dominant, because it maps your force for the
opponent.

The asymmetry moves the upstream constraint from an allowance to a signature,
which is a better place for it.

## Delivery and latency

Delivery is never instantaneous. A message sent on one tick arrives no earlier
than the next tick boundary, consistent with the rule that nothing in the
simulation resolves in zero time.

Degradation adds ticks rather than probability wherever possible, because a
delayed message is more interesting than a lost one and far easier to explain
afterwards. Loss remains available to capabilities that declare it.

Ordering within a link is preserved. Ordering across links is not, so a message
relayed by a longer path can arrive after a later message that took a shorter
one — which is a legitimate source of confusion and must be representable rather
than smoothed away.

## The network is not a puppet string

A client is unregulated compute. Nothing prevents a player from running an
external optimiser and issuing per-unit orders down the communications chain,
which would route around command bandwidth, the execution budget, and the
premise that units act on local judgement.

**Restricting what may be sent does not work.** A preset of message types is a
channel: N types carry log2(N) bits, and a player will encode arbitrary
instructions in whatever vocabulary exists. Even an acknowledgement is one bit.
Restricting *meaning* is unenforceable when meaning is player-defined, and
attempting it produces a rule that looks like a limit and is not one.

What does work is charging for **volume rather than vocabulary**. Bytes and
messages are counted regardless of what they encode, so a cleverly compressed
order still pays for its bytes and a stream of them still pays per message.
Compact protocol design is rewarded, which is legitimate skill, and no amount of
it buys unlimited direction.

Four independent costs bound remote control:

- **capacity** — orders share the net's finite shared throughput with the
  reports the commander needs;
- **emission** — heavy traffic is loud, and a force under continuous direction
  is a force that can be mapped;
- **bandwidth** — an order is drawn from the same allocation as everything else
  reaching that squad; and
- **latency** — and this one cannot be encoded away.

### Latency is the durable bound

Directing a unit remotely requires a round trip: it observes, the report travels
up the chain, the commander decides, the order travels back down. A local
reaction requires none of that — the module is present on the tick the trigger
becomes observable and the only delay is the declared reaction delay.

**The remote loop is strictly longer than the local one, and no protocol
compression shortens it.** Bandwidth can be bought, traffic can be compressed,
emissions can be timed. Distance cannot be argued with.

That produces the correct relationship rather than a prohibition: **global
coordination is possible but slow, local reaction is fast.** Which is what
command actually looks like, and why real orders describe intent rather than
keystrokes.

### The point is that it is priced, not that it is banned

Micromanagement is not the failure. *Unlimited* micromanagement is. A commander
who spends attention directing the decisive element and leaves the rest on their
own judgement is playing well, and the costs above bound that to a few elements
at a time, which is the right number.

A player who tries to direct everything pays all four costs at once: a saturated
net, a mapped force, exhausted allocation, and units reacting a round trip late.
They will lose to a player whose force does not need telling.

### Relays trade reach against tightness of control

Every hop adds latency. A squad at the end of a relay chain is one a commander
can **direct but not micro**, because the round trip is too long for anything
closed-loop.

Extending a network therefore buys contact with distant elements at the price of
their responsiveness to instruction. The further out a force reaches, the more
it must act on intent — which is a cost worth paying and worth understanding
before paying it.

## Store and forward

When a link is unavailable, traffic queues at the sender rather than vanishing.
Queues are bounded by size and by message expiry, both declared per device
class.

On reconnection, queued traffic flows. Three properties matter:

- **it is stale.** A report queued for thirty seconds describes the battlefield
  of thirty seconds ago, and carries its original observation time so the
  recipient can tell;
- **it is bulky.** A reconnecting squad transmits a burst far larger than its
  ordinary traffic; and
- **that burst is conspicuous.** Under traffic analysis, a reconnection is one
  of the most legible events on the battlefield — a squad that has just regained
  contact announces both its existence and its position at the worst possible
  moment.

The third point is an interaction rather than a rule, and it is the kind the
design wants: re-establishing contact is not free, and a commander who
understands that will stagger reconnections or accept the cost knowingly.

## Devices

Communications equipment is authoritative equipment with position, ownership,
operational state, and transfer rules, as already established. Device classes
differ along declared axes:

| Axis | Effect |
|---|---|
| Power | Range and signature together |
| Directionality | Reach along a bearing versus omnidirectional |
| Capacity | Throughput available to bandwidth and traffic |
| Nets | Which nets the device can participate in |
| Queue | Store-and-forward depth and expiry |

Traffic is encrypted and authenticated as a matter of course and this is not a
device option, for the reasons recorded in
[Electronic Warfare](electronic-warfare.md).

A personal set reaches within a squad. A leader's set reaches the command net. A
relay is a device optimised for capacity and reach rather than portability. A
vehicle can carry a larger set than a person.

The established rules stand: succession does not create a device, a
second-in-command may carry a redundant one, and a successor without one must
recover the fallen leader's set or restore contact some other way.

## Relays

A relay extends or restores a path. It is an authoritative object with a
position, and therefore something that can be found, jammed, captured, or
destroyed.

Relays chain, subject to declared limits, and each hop costs latency and is
bounded by the weakest link in the chain. A relay network is a supply line for
information, with the same properties: it must be established, it must be
protected, and cutting it isolates everything behind it.

Relays may be carried and deployed, mounted on vehicles, or found as
infrastructure on the map. The last of these makes existing structures worth
seizing for reasons other than cover.

## What survives total denial

Communications can be denied completely. When they are, command does not stop:

- units continue under their existing intent and their own control logic;
- the squad net may survive even when the command net does not, so a squad
  retains internal coordination while isolated;
- a physical courier carries information at walking pace and cannot be jammed;
  and
- pre-arranged plans need no transmission at all.

This is the property that keeps denial from producing a helpless force. The
established requirement that units outside contact continue operating on
previously supplied orders is what makes it work, and it is another place where
control logic that can pursue intent unsupervised is worth more than control
logic that cannot.

## Faction variation

Portal-origin factions may communicate by other means. Any such network still
declares range, capacity, latency, an observable signature, a disruption
mechanism, and a dependency that can be attacked — the same contract, not the
same equipment.

Whether human electronic warfare can touch a magical network at all, and what
the human answer is to one it cannot, remains open and is recorded in
[Electronic Warfare](electronic-warfare.md).

## Failure modes to avoid

- Range as a bare radius, which makes terrain irrelevant to communications and
  wastes the spatial model already built.
- Instantaneous delivery, which removes the delay that makes stale information
  interesting.
- Unbounded queues, which turn a reconnection into a free information dump with
  no cost.
- Silent loss, where a message vanishes with no observable consequence and no
  explanation available in replay.
- Range and signature balanced as independent statistics, which lets a player
  buy reach without accepting detectability.
- A network so reliable that the disconnection rules never fire, which would
  make the entire knowledge architecture decorative.
- Restricting which messages may be sent, which is unenforceable against
  player-defined meaning and produces a rule that looks like a limit and is not.
- Any network shape that is unconditionally optimal. A player will find it and
  every player will then use it, at which point the topology is a tax rather
  than a decision.

## Open parameters

- Device ranges, powers, and capacities by class.
- Attenuation per material and per edge type, and whether anything blocks
  outright.
- How elevation modifies path attenuation.
- Base delivery latency and the added latency per relay hop.
- Queue depth and message expiry by device class.
- Throughput accounting: whether payload bytes, message count, or both.
- Relay chaining limits and deployment time.
- Net capacity by class, and how sharply it degrades with participant count.
- What an opponent recovers from aggregate signature, and how much observation
  it takes.
- Whether lateral leader-to-leader traffic uses the command net or a third net.
- Whether a unit can join a net it is not organisationally part of, and what
  capturing an enemy device permits.
