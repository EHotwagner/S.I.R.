---
title: S.I.R. Communications Network Architecture
status: proposed
document-type: living-design
version: "0.2"
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

## Open parameters

- Device ranges, powers, and capacities by class.
- Attenuation per material and per edge type, and whether anything blocks
  outright.
- How elevation modifies path attenuation.
- Base delivery latency and the added latency per relay hop.
- Queue depth and message expiry by device class.
- Throughput accounting: whether payload bytes, message count, or both.
- Relay chaining limits and deployment time.
- Whether lateral leader-to-leader traffic uses the command net or a third net.
- Whether a unit can join a net it is not organisationally part of, and what
  capturing an enemy device permits.
