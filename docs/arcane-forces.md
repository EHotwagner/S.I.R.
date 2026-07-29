---
title: Arcane Forces
category: Forces & Equipment
categoryindex: 3
index: 7
status: accepted
decision-status: canonical
document-type: living-content
version: "1.5"
last-updated: 2026-07-29
related:
  - docs/setting-and-factions.md
  - docs/human-forces.md
  - docs/magic-system.md
  - docs/electronic-warfare.md
  - docs/communications-network.md
  - docs/research/arcane-horde-progression-and-equipment.md
---

# Arcane Forces

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
| Signature | emits continuously | none while stable; magical when overloaded |
| Attacked by | jamming, direction finding, traffic analysis | reaching the anchor and destroying it |
| Shape | topology | **geography** |
| Extended by | relays and drones | placing and holding ground |
| Timing | one second per command-net leg in either direction | observations rise on the next tick; caster commands arrive after one second |
| Failure | isolation, still fights | isolation; overloaded anchors become hazardous |

The last row matters: neither faction collapses when cut off. Both continue on
local judgement, which is the same rule applied to two different mechanisms.

**Anchors are authoritative objects with positions.** They can be observed,
approached, destroyed, and in some cases captured. Extending arcane coordination
means physically occupying more ground, which is why this faction contests
terrain that a human force would bypass.

### Information rises; intent descends

Anchored coordination is deliberately asymmetric:

```text
anchored unit observation or status → controlling caster = next tick
controlling caster command → anchored subordinate       = 20 ticks
```

The upward path adds no artificial transport delay beyond delivery on the next
authoritative tick. This applies to legitimate observations and status from
anchored units and to observations borrowed from an actively attuned critter.
It does not improve what the observer perceived, identify unknown equipment, or
expose hidden server truth.

A deliberate instruction from a controlling senior caster reaches an anchored
subordinate after a flat 20 ticks, or one second. Distance within the same
anchor influence does not change that delay, and there are no caster, assistant,
or unit relay chains. A subordinate outside valid anchor influence receives
nothing new and continues on prior intent and local judgement.

This is not the human report-and-order loop with different visual effects.
Humans receive richer but delayed remote reports and accumulate delay over
network legs. Arcane casters receive poorer observations immediately on the next
tick but impose a flat one-second delay when projecting intent downward. The
balance cost remains geographical anchor dependence, finite Anchor Capacity,
caster concentration, overload, breach risk, and the lower precision of arcane
sensing.

### Anchoring has finite capacity

Anchored influence is not an unlimited benefit conferred on everything inside
an area. Every coordinated formation imposes load on the anchor supporting it.
Committing more force therefore requires additional or stronger anchors, paid
for in components, scarce casters, defended ground, and a larger magical
footprint.

When load exceeds capacity, the anchor becomes **unstable**. This is the arcane
counterpart to a human command net saturating, but it fails according to the
faction's own rules:

```text
more anchored force
        ↓
anchor overload
        ↓
visible and detectable instability
        ↓
coordination disruption
        ↓
indiscriminate discharge or uncontrolled breach
```

Instability must be telegraphed early enough for its controller to respond.
Which failure will occur is uncertain during play, but the rising risk is not
hidden: the anchor becomes magically conspicuous and its coordination becomes
unreliable before a catastrophic outcome.

An unstable anchor can discharge damaging lightning into nearby units,
equipment, or terrain without respecting allegiance. At the severe extreme it
can open an **uncontrolled daemon portal**. Anything entering through that
failure is outside the arcane force's command and hostile to every side.

These failures are centred on the anchor and endanger the force depending on it
at least as directly as its opponent. Overload may be accepted as a desperate
risk, but it must not become reliable magical artillery deployed beside an
enemy. Detaching or withdrawing units sheds load and permits recovery, making
retreat from an overcommitted anchor a tactical action rather than a concession.

Anchor capacity is structural, not another replenishing resource and not caster
strain under a second name. Strain prices spell use; anchor capacity prices the
amount of force held in supernatural coordination. Their exact interaction, if
any, is a prototype question rather than an assumed rule.

## Two economies, and neither is ammunition

Humans are limited by **supply**: ammunition, batteries, spare parts, all of it
replenishable mid-mission through a physical supply network.

The arcane are limited by **accumulation**. A caster spends health to empower
and gathers strain, becoming progressively more dangerous to themselves — see
[Risk-Based Magic System](magic-system.md).

Strain persistence and recovery are unresolved. No faction-tempo claim may
depend on meditation, residual Strain, or an automatically rising match-long
floor until that rule is accepted and tested.

The confirmed asymmetry is:

- **humans spend replenishable physical supply** through a supply network;
- **arcane casters accumulate Strain and may spend HP** for immediate effects.

The recovery decision will determine whether arcane tempo is sustained,
cyclical, or primarily front-loaded.

The faction's logistical economy — components, catalysts, prepared sites — is
real but serves preparation rather than sustainment. It is spent to *establish*
rather than to *continue*.

## Rituals are cooperative magic

A spell is an individual caster action. A **ritual** is a site-bound magical
action that requires multiple casters to maintain a quorum. One caster cannot
perform the same ritual merely by taking longer; cooperation is the defining
requirement rather than a speed bonus.

The caster caste has two functional levels. Every senior caster normally leads
a cell of **two or three magical assistants**. Assistants are persistent junior
characters rather than anonymous equipment or nonmagical attendants. They know
lesser spells and ritual abilities, prepare components and circles, and
contribute to cooperative magical work.

Assistants expand a ritual cell without erasing senior-caster scarcity:

- minor workings may require one senior caster and some or all of that caster's
  assistants;
- standard rituals may use a complete caster cell as their magical quorum; and
- major rituals and portals still require multiple senior casters, supported by
  their assistants.

An assistant cannot replace a required senior caster merely by taking longer,
and additional assistants cannot accelerate completion without bound. Their
contribution may improve preparation, stability, interruption tolerance,
maintenance, or controlled shutdown according to the ritual. It cannot erase
Strain, HP expenditure, components, geographical commitment, anchor load, or
breach risk.

Rituals commit scarce casters for a substantial and observable preparation
period. Participants cannot use their ordinary spell capability while
contributing. The ritual also consumes prepared components and temporarily
occupies anchor capacity, so conducting one reduces the force that can be kept
stably coordinated around it.

```text
prepare a site
        ↓
assemble the caster quorum
        ↓
lock a place or trigger from current knowledge
        ↓
maintain cooperative progress
        ↓
resolve the ritual
```

The target is geographical. A ritual locks a location or prepared trigger early
and cannot secretly follow a unit until completion. Its prediction must use
information the arcane force legitimately possesses — direct observation,
reports, scrying, objectives, or inferred movement — rather than privileged
server knowledge.

Preparation produces a growing magical signature and a physical concentration
of valuable casters. Breaking the quorum by killing, suppressing, or displacing
participants interrupts progress. Interruption does not automatically erase all
work; whether progress stalls, decays, or can be resumed is an open parameter.
Additional casters beyond the quorum cannot accelerate completion without
bound, though they may eventually provide resilience or empowerment.

### Initial ritual shapes

The initial system supports three shapes without yet defining a full ritual
catalog:

- A **culmination** resolves a delayed effect against a location chosen during
  preparation. Its value depends on predicting where the enemy will be when it
  completes. Moving, deceiving the observers, or interrupting the quorum are
  ordinary counterplay.
- A **prepared trap** binds a finite magical trigger to a place. It leaves
  evidence, can be found through appropriate reconnaissance, and must be
  removable, consumable, or avoidable rather than permanently denying terrain.
- A **portal ritual** establishes a connection at an exposed, prepared
  destination. Different portal contracts determine what may cross and who, if
  anyone, controls it.

### Portals are different contracts

A portal is not one mechanic with cosmetic destinations.

| Portal | Purpose | Ownership after crossing |
|---|---|---|
| **Transit portal** | Moves precommitted arcane personnel, equipment, or supply, potentially in successive waves | Remains arcane; counts normally against supply, command, and anchor load |
| **Goblin portal** | Opens an incursion from an unaffiliated goblin territory at a predicted location | Neutral in ownership and hostile to every side, including the arcane force |
| **Daemon breach** | Catastrophic anchor-overload failure rather than a chosen ritual | Uncontrolled and hostile to every side |

A transit portal is the arcane civilization's physical reinforcement method.
It transports owned assets and creates no people. A human force can attack the
ritual, prepare the exit, withdraw from the predicted area, or allow an early
wave through and break quorum before the rest arrives.

A goblin portal deliberately introduces an independent battlefield problem.
The goblins receive no arcane orders, reports, targeting priorities, alliance,
or privileged reason to prefer human targets. They do not belong to the arcane
roster and consume no arcane supply, command capacity, stake, or anchor capacity
after emergence. The ritual itself still consumes casters, components, time,
exposure, and temporary anchor capacity, and its output must be bounded.

The organized arcane civilization includes goblin citizens and soldiers, but
that does not make every goblin politically aligned with it. Goblin-portal
arrivals are explicitly unaffiliated communities or warbands. Their hostility
is a relationship and behavior contract, not an assertion that goblins are
inherently feral.

## Force shape: scarce decisive casters, durable mass

Human forces distribute capability across many similar people. The arcane
concentrate it.

- **Senior casters** are few, individually decisive, and fragile in a specific
  way:
  damaging one does not merely reduce its output but can push accumulated strain
  past its remaining health and trigger a breach. A caster is a liability that
  grows more valuable and more dangerous at the same time.
- **Magical assistants** normally occur in cells of two or three per senior
  caster. They provide lesser spells, ritual work, magical preparation, and
  continuity, but do not independently exercise senior command, major anchor
  authority, or decisive spell capability.
- **Nonmagical peoples** — goblins, orcs, trolls, and others — are the mass that
  makes scarcity survivable. They are not failed casters. They screen, hold
  ground, absorb the ranged fire humans are best at delivering, and carry the
  components and infrastructure the casters depend on.

Substantial armour and regeneration are the candidate characteristics for the
heavier of these, which directly answers human strengths: armour against small
arms, and recovery against a faction whose in-match healing is deliberately
limited.

Assistant losses create an intermediate failure state. A damaged cell can
retain its senior caster and command authority while losing ritual resilience,
lesser magic, and the ability to maintain several magical tasks. Killing the
senior caster remains the more decisive blow.

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

Ordinary arcane perception does not emit in any way a human sensor detects,
which means a human force cannot initially tell it is being observed.
Attunement is the declared exception: borrowing a remote critter's senses leaves
magical evidence while the link is active. Undetectable local perception remains
a legitimate advantage under the guardrail requiring evidence, provided its
*effects* are observable — a force that reacts to something it should not have
seen has revealed a capability, and human research is the eventual answer.

### Ambient critters are potential observers

Maps can contain harmless ambient creatures such as rats, birds, insects, or
setting-appropriate equivalents. A critter is an ordinary neutral actor with a
physical position, species-appropriate senses, natural autonomous movement, and
no combat role. It perceives only what its own geometry, attention, and
acquisition permit.

An arcane caster can **attune** a critter and receive the observations it
actually earns. Attunement does not steer the animal, improve its senses, make
it an arcane unit, or expose authoritative state the critter did not perceive.
Its facts enter the ordinary knowledge and reporting model and may satisfy a
ritual's observation or known-location requirement. The ritual still locks its
geographical target when preparation commits; later critter movement does not
make the target track.

```text
critter observes through ordinary perception
        ↓
attunement carries the earned facts
        ↓
arcane knowledge gains an unreliable remote observation
        ↓
ritual or portal may use that knowledge lawfully
```

Attunement is not perfectly deniable. An actively used critter produces magical
evidence or anomalous behavior that appropriate human reconnaissance can
acquire. Humans may kill suspected critters, but can also trap, capture, drive
off, exclude, detect, or deliberately mislead them. Eradication is therefore a
possible tactical response with costs in time, attention, ammunition, and
signature rather than a mandatory map-cleaning step.

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

### Physical countermeasure

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
through a wall.

Such an instrument has several narrow, high-value purposes. An overloaded
anchor becomes magically conspicuous before it discharges or breaches, giving
reconnaissance evidence of its position and unstable load. Ritual preparation
and active critter attunement provide further evidence. Detecting Strain
recovery remains conditional on the unresolved recovery rule.

Fielding such a detector is therefore a natural research objective, and the
eventual answer to the guardrail requiring every capability to leave evidence.

## The faction capability contract

The setting document requires every faction to answer ten questions.

| | Answer |
|---|---|
| Acquires information | Magical perception through obstruction, vague and short-ranged; scouts, screening mass, scrying, and observations borrowed from attuned ambient critters |
| Issues commands | Finite anchored influence, not transmission; outside it or after anchor failure, prior intent |
| Consumes | Health and strain personally; components, catalysts, and prepared sites logistically |
| Moves forces and supply | On foot, with mass carrying infrastructure; constrained translocation for tactical movement; transit portals for prepared reinforcement insertion |
| Replaces casualties | Not within a match. A transit ritual may insert precommitted reinforcements but creates no personnel; goblin portals introduce independent hostiles |
| Recovers from injury | Regeneration in the heavier nonmagical peoples; magical healing at cost to the healer |
| Control failure | Anchor destruction or overload, caster loss, and breach consequences that damage their own side |
| Does better | Fighting where geometry favours the obscured, absorbing ranged fire, operating without a network |
| Vulnerable to | Reaching and destroying anchors, focusing casters, forcing strain, and open ground |
| Reveals itself by | Anchors as physical objects, anchor-instability and ritual signatures, evidence from active critter attunement, visible magical effects, breach events, and reactions to things it should not have seen |

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

## What canonical status covers

Settled: coordination that is anchored rather than transmitted, so an arcane
geography is attacked physically where a human network is attacked
electronically; the two economies, with humans limited by supply and the arcane
by accumulation; and a force shape of scarce decisive senior casters, two or
three persistent lesser-magical assistants per senior caster, and durable mass
in which the nonmagical peoples are not failed casters. Information differs
rather than ranks, seeing through obstruction and vaguely where humans see far
and exactly. Anchored observations rise to the controlling caster on the next
tick, while caster commands descend after a flat 20 ticks without relay chains.
Human sensors work completely against this faction and human electronic warfare
does nothing at all. Anchoring has finite structural capacity; overload becomes
observably unstable, disrupts the whole force relying on the anchor, and can
produce indiscriminate lightning or a severe uncontrolled daemon portal hostile
to every side. Rituals are cooperative, site-bound magic requiring a caster
quorum, observable preparation, and a geographical commitment; minor workings
may use one caster cell, while major rituals and portals still require multiple
senior casters. Their initial shapes are delayed culminations, prepared traps,
and portal rituals. Transit portals move owned assets, goblin portals release
unaffiliated hostiles that count toward no arcane resource, and daemon portals
are uncontrolled overload failures. Arcane casters may borrow only the ordinary
observations of attuned ambient critters without controlling them.

Not settled: every number, and the open parameters at the end.

**What would force a revision of the shape:** anchored coordination proving
inexpressible through the ordinary knowledge and command rules and requiring
privileged server behaviour; regeneration proving indistinguishable from simply
having more health; or the asymmetry collapsing so that one side is correct
regardless of terrain and mission.

## Checking against invariant 13

Neither faction's approach is unconditionally correct, and neither faction has
an internally dominant configuration.

Casters versus mass trades decisiveness against durability, and the correct
ratio depends on terrain and on how long the engagement will last. Empowerment
trades immediate effect against strain, which is a decision every cast rather
than a build chosen once. Anchoring trades coordination against the obligation
to hold ground, and committing more mass against the capacity and supernatural
stability required to coordinate it.

The risk sits with **anchors**. If placing one is always correct, they become a
setup step rather than a decision. They need to cost enough — in time, in
components, and in the units required to hold them — that operating unanchored
is sometimes right. Stronger or additional anchors must likewise create enough
expense and observable battlefield exposure that capacity is a force-design
choice rather than an automatic upgrade.

## Architectural tests

- whether **strain and breach** produce interesting decisions at 20 ticks per
  second, or resolve too fast to reason about;
- whether a faction with **no communications network** can be represented
  without special-casing the comms model;
- whether **regeneration** stays distinguishable from simply having more health;
- whether **anchored coordination** is expressible through the ordinary
  knowledge and command rules rather than requiring privileged server behaviour;
- whether anchor overload is legible soon enough to permit load shedding,
  dangerous enough to deter unconditional reinforcement, and too self-destructive
  to serve as reliable artillery;
- whether rituals reward prediction and create interruptible objectives rather
  than resolving as unavoidable delayed attacks or static area denial;
- whether critter attunement creates useful but unreliable reconnaissance
  without making exterminating every ambient creature the automatic human
  opening; and
- whether the asymmetry survives contact, or collapses into one side being
  correct.

## Open parameters

- Every numeric value, as with human forces.
- Anchor influence shape, radius, placement cost, and whether it is blocked by
  terrain as signal paths are.
- How anchor capacity is derived, how formation load is measured, and whether
  stronger anchors or multiple anchors are the more efficient response.
- Instability thresholds, warning progression, recovery after load shedding,
  and the exact coordination failures produced before catastrophe.
- The lightning-discharge and daemon-breach outcome tables, including area,
  severity, timing, portal duration, and uncontrolled daemon behaviour.
- Ritual quorum sizes, preparation times, participant positioning, contribution
  limits, signature progression, and how interruption changes stored progress.
- The first culmination effects and trap triggers, including their evidence,
  lifetime, discovery, removal, and consumption rules.
- Transit-portal preparation, wave cadence, gate duration, insertion capacity,
  exit constraints, and the disposition of committed assets that have not yet
  crossed when the ritual ends.
- Goblin-portal duration and output bounds, goblin objectives and behavior,
  neutral reward treatment, and safeguards against farming or reliably using
  the incursion as artillery.
- Ambient-critter species, populations, movement, perception, attunement range
  and duration, report detail, magical evidence, countermeasures, and
  treatment of critters that leave or re-enter the playable area.
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

The non-canonical
[Arcane Horde Progression and Equipment Proposal](research/arcane-horde-progression-and-equipment.md)
explores caster-led force structure, species roles, progression depth, perks,
and equipment without settling these parameters.
