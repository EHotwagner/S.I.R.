---
title: Command, Information, and Missions
status: proposed
document-type: reference
category: Battlefield Systems
categoryindex: 4
index: 5
version: "0.3"
last-updated: 2026-07-28
related:
  - docs/gameplay-reference.md
  - docs/communications-network.md
  - docs/electronic-warfare.md
  - docs/casualty-and-medical-architecture.md
  - docs/stakes-and-reinforcement.md
---

# Gameplay Command, Information, and Mission Rules

## Summary

S.I.R. separates local perception, communicated knowledge, command support, and
player orders. Human capability travels through physical electronic networks
that emit and can be jammed. Arcane coordination occupies geographical anchor
influence and must be attacked physically. Units remain locally autonomous when
disconnected. This page also collects the shared action, logistics, casualty,
and reinforcement rules that determine how information becomes consequence.

Return to the [Gameplay Reference](gameplay-reference.md).

## Authoritative tick

⬜ 🟩 The server advances at 20 Hz through this logical pipeline:

1. Deliver events, messages, reports, and observations available at the tick
   boundary.
2. Wake scheduled or event-triggered unit control modules.
3. Accept and validate action requests.
4. Advance actions and movement credit.
5. Collect transitions and actions completing this tick.
6. Resolve spatial conflicts and action outcomes from stable snapshots.
7. Apply damage, [Suppression](gameplay-formulas.md#suppression), healing, and
   resource expenditure simultaneously.
8. Resolve incapacitation, death, cancellation, succession, breaches, and
   consequence chains.
9. Generate perception, communication, and report events from the new state.
10. Commit the tick for replay, audit, persistence, and transmission.

Nothing resolves in zero time. A reaction is an ordinary timed action triggered
after a locally observable event.

## Perception pipeline

**Perception** is not one visibility test:

```text
geometry
  → stimulus
  → acquisition
  → reaction
```

### Geometry

Geometry determines whether and how a sensing path exists from:

- observer and target footprints;
- observation origins and exposure points;
- range and sensor arc;
- body facing and attention;
- terrain cells and semantic edges;
- smoke and other modality-specific occluders;
- stance and height;
- intervening units and effects; and
- sensor modality.

Line of sight alone does not mean the observer noticed or identified anything.

### Stimulus

A sensor produces modality-specific facts:

- optical shape or motion;
- muzzle flash;
- sound and approximate direction;
- thermal signature;
- radar return;
- electronic emission;
- magical signature; or
- physical contact.

A stimulus reveals only what it physically supports. Hearing a shot does not
reveal exact coordinates or a complete unit record.

### Acquisition

**Acquisition** is the time required to turn a stimulus into a usable local
observation.

🟧 The accepted input relationship is:

```text
acquisition contribution depends on:
    sensor effectiveness
  × target signature
  × exposed amount
  × attention and facing
  × environment and status
```

The final combination need not be literal multiplication. Progress belongs to a
specific observer, stimulus or target, sensor, and contact episode. It decays
when the stimulus ends rather than resetting instantly.

Crossing a threshold emits only the observation fields earned by that
interaction.

### Reaction

```text
acquisition completes
  → local observation delivered
  → control module requests a reaction
  → reaction delay
  → resolution
```

Acquisition time and reaction time are separate. Prepared attention can reduce
delay but cannot eliminate the action lifecycle.

## Facing and attention

Body facing and attention direction each use eight compass directions and may
differ. A unit can withdraw while watching behind or strafe while watching a
door.

The attended forward sector provides the strongest ordinary acquisition and
reaction. Side and rear approaches are progressively better for the attacker.
The leading candidate is a hard sector boundary with a readable step change;
exact angles and values remain open.

Turning, changing stance, shouldering a weapon, and deliberately observing a
sector are timed actions.

## Knowledge and reports

Units and commanders know only:

- their own state;
- local observations they acquired;
- reports that physically reached them; and
- rules and static content available to their role.

An observation can contain time, observer, sensor, provenance, bounded
location, silhouette, facing, visible action, identification glyph,
[HP](gameplay-formulas.md#hp) when legitimately revealed, visible equipment,
status, sound, emission, and direction.

Reports preserve observation time and provenance. They can arrive late, become
stale, be aggregated, and be contradicted by newer evidence. Remembering an old
observation does not grant current truth. Older observations arriving over a
slower route cannot overwrite newer knowledge of the same event or contact;
independent observations may still conflict.

## 🟦 Human communications

### Nets and topology

The canonical arrangement is:

```text
command net = headquarters + squad leaders
squad net   = one squad leader + that squad's members
```

A squad leader participates in both. Losing the leader can leave the squad net
intact while separating it from headquarters. A successor still needs physical
command-net equipment or must recover it.

### Signal path

A link exists between participants on the same net when a signal path connects
them.

```text
effective range =
    device range
  - obstruction attenuation along the path
  - jamming and environmental degradation
```

The path uses the same cells, semantic edges, and levels as other spatial
traces. Elevation can improve both observation and communications.

Transmitting power determines range and detectability together. A long-range
set is a loud set.

### Command Bandwidth

**Command Bandwidth** is downstream networked support delivered to a squad.
It prices fused information, commissioned analysis, and other facts arriving
from elsewhere. Local perception and local thinking are free and silent.

Usable delivery is bounded by:

- the weakest link from headquarters to the squad;
- finite link throughput;
- shared net capacity;
- player allocation;
- traffic from orders and reports;
- jamming;
- latency; and
- available physical devices.

Upstream reporting is not allocated from the player's bandwidth pool, but still
consumes link capacity and emits.

Bandwidth allocation is pooled at squad scale. A unit cannot receive support
through a nonexistent path.

### Saturation

🟩 Net load does not degrade linearly forever. Past a deterministic learnable
threshold, retries and contention cause throughput to collapse:

```text
load below threshold → carried traffic
load above threshold → retries → more load → whole-net collapse
```

The cause is deterministic; which particular messages fail can be
unpredictable. Shedding load, splitting nets, ordering silence, or withdrawing
formations can restore service.

### Latency

🟩 Local squad-net delivery takes at least one tick. Every physical command-net
leg takes 20 ticks, or one second at the authoritative simulation rate:

```text
command latency =
    20 ticks × command-net legs
  + queue, contention, and degradation delay
```

Leader-to-HQ and leader-to-leader are each one leg. Leader-to-relay-to-HQ is two
legs. The rule applies equally to orders, reports, observations,
acknowledgements, friendly status, and player-defined messages. Reports do not
receive an instantaneous upstream exception.

Remote closed-loop direction is strictly slower than local reaction:

```text
unit observes
  → report travels up
  → commander decides
  → order travels down
  → unit acts
```

Compression can reduce volume but cannot eliminate round-trip distance.
Headquarters therefore holds a delayed command picture. Reports carry
observation and arrival ticks, source, provenance, identity, and route age. The
interface may show that an order was sent immediately, but receipt or execution
is known only after a delayed acknowledgement returns.

### Store and forward

Disconnected traffic queues within declared size and expiry limits. On
reconnection:

- the information may be stale;
- queued messages form a large burst; and
- the burst is conspicuous to traffic analysis.

## 🟦 Electronic warfare

Electronic warfare attacks three separable layers:

| Layer | Attack | Result |
|---|---|---|
| Link | Jamming | Reduces range, capacity, and delivered command support |
| Emission | Direction finding and traffic analysis | Locates transmitters and infers network shape or attention |
| Content | Deception | Sends false but physically plausible information through legitimate channels |

Jamming does not disable the unit or its control module. It makes the unit
isolated and worse informed. Local perception and prior intent remain.

A jammer is itself a loud continuous emitter and can affect its own side
depending on the final footprint rules. Direction finding cannot be defeated by
an equipment checkbox; the protection is not transmitting, transmitting
briefly, using terrain, using directionality, or leaving the transmit position.

🟪 Arcane anchor coordination emits no radio traffic, so human EW has nothing to
jam, direction-find, or analyze. Human optics, thermal sensors, acoustics, and
physical communications continue to work normally against arcane bodies and
terrain.

## 🟪 Arcane information

Arcane local perception is proposed to detect vague presence and vitality
through obstruction without producing human-quality exact identification.

🟩 Arcane information timing is asymmetric:

```text
anchored observation or status → controlling caster = next tick
controlling caster → anchored subordinate command   = 20 ticks
```

Upward observations accumulate no command-hop delay, including legitimate
observations borrowed from an attuned critter. They retain the quality and
limits of the original observer. Downward commands take one second flat within
valid anchor influence, independent of distance. Arcane units and assistants do
not form relay chains. Outside anchor influence, a unit receives no new command
and operates on local judgement and prior intent.

Arcane forces also use:

- ordinary scouts and reports;
- scrying snapshots;
- observations borrowed from attuned neutral critters; and
- geographical anchor influence for coordination.

Anchor coordination is detailed on
[Magic, Anchors, Rituals, and Portals](gameplay-magic.md#anchor-capacity).

## Logistics

⬜ 🟩 Resources are physical quantities with ownership, location, capacity,
access, timing, and transfer.

Human sustainment includes ammunition, batteries, spare parts, medical supplies,
fuel where applicable, and replacement components. Arcane logistics emphasizes
components, catalysts, prepared sites, and transported infrastructure; caster
[Strain](gameplay-magic.md#strain) and
[Anchor Capacity](gameplay-magic.md#anchor-capacity) remain separate
non-logistical limits.

Load competes for carrying capacity. More equipment can mean less ammunition,
slower movement, worse readiness, and greater signature.

## Casualties and medicine

### State flow

```text
functional, possibly wounded
        │ HP reaches zero
        ▼
incapacitated + deteriorating ── untreated ──► dead
        │ stabilize
        ▼
incapacitated + stable
        │
        ├── carried
        ├── evacuated
        ├── abandoned
        ├── captured
        └── executed
```

### Medical actions

| Action | Subject | Result |
|---|---|---|
| **Aid** | Functional wounded unit | Arrests bleeding and temporarily mitigates impairment; little or no HP restoration |
| **Stabilize** | Incapacitated deteriorating unit | Arrests deterioration; does not restore combat function |
| **Take up / set down** | Incapacitated unit | Begins or ends the continuous carried state |
| **Evacuate** | Carried casualty at a valid destination | Fixes mission disposition and frees the carrier |

Any unit may attempt basic aid and stabilization. Medics are faster, more
reliable, and can access specialist procedures.

All actions consume time, supplies, and exposure and can be interrupted.
Carrying consumes capacity, slows the carrier, prevents area engagement, and
severely limits point engagement.

🟩 Battlefield treatment does not normally return an incapacitated human to the
fight. Actual recovery is a campaign process.

## Reinforcement and stakes

Reinforcement adds force to a live match but cannot buy new people into
existence.

### Humans

Exact physical human reinforcement insertion remains open. Any method must
preserve travel, exposure, roster ownership, and logistics.

### Arcane

A transit portal physically moves precommitted owned personnel and supply in
successive waves. Arrivals immediately consume local anchor load. A goblin
portal creates an independent hostile problem and is not arcane reinforcement.

### Overcommitment

Human overcommitment can collapse shared network throughput. Arcane
overcommitment can destabilize an anchor. In both cases, the cost applies to the
whole coordinated force rather than only the marginal arrivals.

## Open command and mission data

🟥 Still unresolved:

- acquisition rates, decay, thresholds, sector angles, and transition times;
- device range, power, attenuation, capacity, and queue;
- bandwidth pool, flow rate, allocation exchange rates, and saturation curve;
- jamming footprints and degradation;
- report aggregation and traffic volume;
- logistics weights, capacities, transfer timings, and consumption;
- wound deterioration and medical action timings;
- carry speeds and interruption costs;
- human reinforcement insertion;
- stake values, availability windows, and reward division; and
- anchor influence and load values.

## Sources and deeper rationale

- [Game Vision](game-vision.md)
- [Communications Network Architecture](communications-network.md)
- [Electronic Warfare Architecture](electronic-warfare.md)
- [Casualty and Medical Architecture](casualty-and-medical-architecture.md)
- [Logistics Architecture](logistics-architecture.md)
- [Stakes and Reinforcement](stakes-and-reinforcement.md)
