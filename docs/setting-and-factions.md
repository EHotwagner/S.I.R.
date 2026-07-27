---
title: S.I.R. Setting and Faction Architecture
status: proposed
document-type: living-design
version: "1.3"
last-updated: 2026-07-27
related:
  - docs/game-vision.md
  - docs/research/squad-command-and-succession.md
---

# S.I.R. Setting and Faction Architecture

## Purpose

This document defines how setting and faction identity constrain S.I.R.'s
combat, logistics, progression, command, information, and campaign systems. It
separates established direction from candidate faction design spaces so that
premature lore does not silently become architecture.

## Setting premise

### Established direction

S.I.R. takes place on Earth a few years in the future during an ongoing
system-integration event. Portals connect Earth to supernatural domains.
Monsters, undead, magic, foreign ecologies, materials, and organized forces can
cross through them.

The first portals and the System appeared ten years before the game's present.
The setting therefore represents the first mature response to integration, not
the first hours of an apocalypse and not a world in which portals have been
normal for generations.

Several portal types exist. The dominant operational event is a temporary
incursion: a portal appears, creates a limited mission opportunity, and exposes
valuable resources or rewards. Players bid for access to these missions.

Organized portal-origin factions are initially encountered primarily beyond
portals or inside portal-connected mission spaces. They do not maintain
permanent recognized settlements or territorial holdings on Earth in the
initial setting. Monsters or hostile forces may spill across temporary
incursions, but this does not yet amount to durable occupation.

This boundary may change in later campaign eras. A faction establishing an
Earth foothold, embassy, enclave, colony, occupation zone, or persistent portal
anchor can be a major world-state development rather than baseline history.

The broad genre reference is the portal-fantasy structure seen in works such as
Solo Leveling: mysterious gates connect a recognizable modern world to dangerous
other realms, creating new combat, institutions, and resource activity. S.I.R.
uses that collision as a premise but does not inherit the reference's exact
cosmology, awakened humans, hunter economy, ranks, or protagonist power model.

The official Solo Leveling synopsis describes gates as passages between the
present world and another dimension inhabited by monsters. That is the relevant
structural comparison, not a lore dependency.

### Meaning of integration

Integration is not merely a sequence of isolated dungeon raids. Portal contact
changes:

- security and military organization;
- access to territory and resources;
- ecology and contamination;
- technology and research priorities;
- logistics and infrastructure;
- political authority and faction formation;
- the meaning of casualties and remains;
- intelligence and communications; and
- the tactical assumptions under which modern forces operate.

After ten years:

- governments and militaries can maintain dedicated integration-response
  organizations;
- experienced participant veterans and institutional doctrine can exist;
- portal-derived materials can influence research and industry;
- civilians know that portals, monsters, and the System are real;
- infrastructure and law have begun adapting without becoming wholly alien to
  the present day; and
- major political, scientific, military, and religious questions remain
  unsettled.

The System is a literal in-world phenomenon. Its mechanics are the game's
mechanics: classes, attributes, skills, progression, abilities, statuses,
missions, objectives, and applicable rules exist as facts in the setting rather
than interface-only abstractions.

## The diegetic System

### Authoritative role

The authoritative server resolves the System's rules. This is both a software
architecture boundary and an in-world truth:

- the server determines valid state, eligibility, costs, and outcomes;
- clients render the System information available to their user;
- WebAssembly modules receive machine-readable System capabilities and events
  available to their host;
- alternative clients may reorganize or interpret presentation but receive no
  privileged mechanics; and
- replays and audits can identify which System rule and ruleset version produced
  an outcome.

The System should therefore provide stable identifiers for every diegetic
mechanical concept, including:

- classes and advanced-class lineage;
- attributes and proficiencies;
- abilities, actions, reactions, and prerequisites;
- progression offers and selections;
- injuries, conditions, effects, and durations;
- items, resources, magic, technology, and equipment;
- missions, objectives, rewards, and consequences;
- factions, relationships, and eligibility;
- command roles, succession, and squad effects; and
- campaign, mode, and ruleset scope.

### Knowledge boundary

Literal mechanics do not imply omniscience. The System may know authoritative
world truth, but an actor sees only the facts the rules disclose to that actor.

For example:

- a person can know their own class, attributes, available abilities, and
  conditions;
- a commander can know subordinate information that has legitimately reached
  headquarters;
- an opponent's class, condition, or ability remains unknown until an
  observation or System rule reveals it;
- hidden participant allocation in a major mission is not disclosed; and
- a custom client or WASM module cannot query the System for inaccessible world
  state.

This preserves the established fog-of-war and communications model while making
the mechanical vocabulary part of the fiction.

### Recognition and initiation

Only recognized participants can directly perceive and interact with the
System. Recognition is earned by killing a required number or category of
monsters. Exposure to portals, knowledge that the System exists, military
service, or proximity to another participant is not sufficient by itself.

Recognition is an authoritative state transition:

```text
unrecognized entity
        ↓ eligible monster-kill credit
recognition threshold satisfied
        ↓
recognized System participant
```

The System must determine eligible kills and attribution. The detailed rules
remain open, including:

- the required number of kills;
- whether monster type, threat, or origin matters;
- whether assisted or squad kills grant credit;
- whether incapacitation, capture, or indirect kills count;
- whether recognition progress is visible before completion;
- whether different factions use the same recognition condition; and
- whether summoned, controlled, farmed, or otherwise artificial targets qualify.

Recognition cannot rely on client claims or WASM-reported outcomes. The server
must derive it from authoritative combat events and prevent safe target farming,
collusive boosting, and repeated credit for the same entity.

Recognition creates a meaningful boundary between ordinary people and
System-active personnel. It can influence recruitment, military organization,
social status, risk-taking, training, and access to progression without implying
that recognized humans gain magic.

### Human access is not human magic

Human interaction with the System does not by itself grant spellcasting.
Initially, human factions remain nonmagical and use realistic or bounded
near-future capabilities. The System can classify, measure, unlock, constrain,
or communicate human training and technological capabilities without converting
them into magic.

Unrecognized humans cannot directly perceive or interact with the System.
Recognized human participants can access the System, but recognition does not
itself grant magic. Whether portal-origin factions begin recognized or satisfy
different conditions, and whether different participants experience the same
presentation, remain open.

## Technology and magic boundary

### Human baseline

Human factions initially have no access to magic. They approach the integration
from recognizably realistic modern or near-future standards:

- firearms, explosives, vehicles, armor, drones, and engineering;
- optical, acoustic, thermal, radar, electronic, and networked sensors;
- radio communications, relays, encryption, signals intelligence, and
  electronic warfare;
- conventional leadership, squads, command succession, and mission command;
- ammunition, fuel, energy, medicine, food, personnel, and spare-parts
  logistics; and
- trauma care, evacuation, repair, training, research, and industrial
  production.

This constraint is a source of identity, not a temporary content deficiency.
Humans solve problems through preparation, combined arms, information,
engineering, production, and doctrine.

### Human organizations

The player commands a mercenary company. The company owns or employs the
player's persistent personnel, organizes squads, maintains equipment and
supplies, uploads control modules, develops doctrine, bids for portal access,
and accepts the consequences of its operations.

Conventional national militaries have a different role. They:

- secure Earth-side portal sites and their surrounding territory;
- establish perimeters, screening, evacuation, and containment;
- respond to civilization-scale or otherwise major incursions;
- protect critical infrastructure and civilian populations; and
- provide the large institutional force that mercenary gameplay does not
  attempt to simulate directly.

Mercenary companies occupy the operational space where opportunity, scarcity,
specialization, and acceptable risk make contracted forces useful. They conduct
portal expeditions, resource extraction, reconnaissance, recovery, targeted
elimination, escort, sabotage, research support, and other bounded missions.

This creates three separate concepts:

1. **Human capability family:** the common nonmagical technological and
   doctrinal foundation.
2. **Player organization:** an individually developed mercenary company with
   its own personnel, leadership, equipment, doctrine, reputation, resources,
   and history.
3. **Institutional faction:** a state, military, regulator, corporation,
   research body, or other organization that can issue contracts, control
   access, provide equipment, or affect campaign relationships.

Mercenary companies can become mechanically distinct through their accumulated
people, doctrine, technology, logistics, reputation, and System progression
without requiring every company to be a separate hard-coded species-level
faction.

### Initial playability

The initial canonical player faction is the human mercenary company. Organized
arcane forces, monsters, undead, and other portal-origin factions begin as
server-controlled PvE forces.

The initial release is anchored by one organized arcane civilization as its
primary portal-origin opponent. Monsters, undead, hazards, or other creatures
may still appear, but they do not need to constitute equally complete launch
factions.

Portal-origin playability is a later direction. Their initial implementations
should therefore:

- use authoritative actions, resources, knowledge, and command rules rather
  than scripted omniscience;
- avoid mechanics that only work because an AI receives hidden world truth;
- define versioned capability and observation contracts;
- separate faction rules from the server controller that operates them; and
- remain compatible with later standard and player-provided WebAssembly control
  modules if the faction becomes playable.

Later playability does not require competitive symmetry. It requires that the
faction's asymmetry be expressed through legitimate mechanics that a human or
module can eventually understand and control.

### Provisional human nanotechnology

Advanced nanomachinery or related medical technology may exist to make
stabilization, healing, prosthetics, and between-mission recovery compatible
with the campaign's pace.

It should not function as consequence-free technological magic. A viable model
would require some combination of:

- trained medical personnel;
- consumable nanomedical stocks;
- power and specialized equipment;
- treatment time and evacuation;
- limits based on injury type and severity;
- side effects, complications, or reduced readiness; and
- strategic production or research capacity.

The first prototype should distinguish battlefield stabilization from recovery.
Keeping a casualty alive during a match does not need to return that person to
full combat effectiveness during the same match.

### Supernatural factions

Portal-origin factions may use magic, monsters, undead, or other supernatural
systems. Each faction needs an internally coherent ruleset with:

- a power or casting resource;
- acquisition and replenishment rules;
- capability limits and prerequisites;
- observable effects and information boundaries;
- logistical dependencies;
- command and control structure;
- recovery, replacement, and casualty rules;
- counters available to other factions; and
- stable public API contracts for actions and observations.

Magic cannot be implemented as an unrestricted exception mechanism. A spell
that sees through terrain, creates a unit, revives a corpse, teleports, controls
a mind, or disrupts communications must define its range, timing, cost,
knowledge requirements, failure conditions, and counterplay.

## Faction design principle

Faction asymmetry should produce different operational problems, not merely
different damage values.

A complete faction should answer:

1. How does it acquire actionable information?
2. How does it issue commands and coordinate?
3. What does it consume to operate?
4. How does it move forces and supplies?
5. How does it replace casualties?
6. How does it recover from injury or damage?
7. What creates morale, cohesion, or control failure?
8. What does it do better than other factions?
9. Which disruption strategies are effective against it?
10. What evidence reveals its capabilities to an opponent?

## Portal ecology and mission access

### Portal categories

The setting supports several portal categories rather than one universal gate.
Most have not yet been mechanically defined. Candidate distinctions include:

- temporary incursions;
- persistent or recurring gates;
- unstable breaches;
- controllable or anchor-bound portals;
- one-way spill events;
- portals large enough for organized forces or logistics; and
- small anomalies that transmit effects, resources, or creatures.

Only the plurality of types and the predominance of temporary incursions are
established. The remaining categories are design space.

### Temporary incursions

A temporary incursion provides:

- a limited discovery, bidding, preparation, and entry window;
- a mission area containing threats, objectives, and valuable resources;
- an uncertain lifetime or closure condition;
- reasons for multiple actors to seek access; and
- consequences for success, failure, incomplete extraction, and portal closure.

The portal lifecycle must be authoritative and visible to the degree permitted
by the System:

```text
incursion detected
      ↓
mission information disclosed
      ↓
bidding and preparation window
      ↓
participant allocation
      ↓
entry and battlefield discovery
      ↓
completion, extraction, collapse, or failure
```

### Bidding

Players bid for portal-mission access because completion can grant valuable
resources. The exact bid instrument and allocation algorithm remain open.

In-world, bidding is performed by mercenary companies seeking permission or
allocation to enter an incursion. The authority that licenses companies,
controls portal access, publishes opportunities, and resolves bids remains
unresolved.

The bidding system must preserve hidden participation:

- a player sees the information required to form their own bid;
- the player sees whether and how their own force was admitted;
- no bidder list or bidder count is exposed;
- competing bids are not exposed;
- no clearing price or allocation detail may reveal whether another player
  entered the same instance; and
- public APIs and custom clients receive the same restricted bidding state.

A bid should create a meaningful campaign decision rather than merely reward the
player who checks the schedule first. Candidate commitments include currency,
supplies, reputation, readiness, force exposure, reward share, or acceptance of
greater mission risk. None is selected yet.

## Initial faction design spaces

These are capability spaces for prototyping, not accepted names, lore, or launch
roster commitments.

### Human containment and expeditionary forces

**Identity:** disciplined, information-rich, logistics-dependent combined arms.

Potential strengths:

- firearms and ranged lethality;
- sensors and reconnaissance;
- reliable hierarchical command;
- electronic warfare;
- vehicles, drones, engineering, and prepared defenses;
- industrially interchangeable equipment; and
- precise logistics and medical evacuation.

See [Human Forces](human-forces.md) for the concrete classes, weapons, armour,
and equipment. The identity above is not asserted there but produced: human
capability is information capability, information capability is electronic, and
electronic capability emits and consumes power, so the faction's strength and
its vulnerability are one property seen from two sides.

Potential vulnerabilities:

- ammunition, fuel, batteries, spare parts, and communications dependence;
- limited same-match healing;
- physically vulnerable personnel;
- difficulty understanding new supernatural rules;
- jamming, ambush, infiltration, and infrastructure disruption; and
- no native spellcasting.

### Organized arcane force

**Identity:** rule-bound magical specialists whose capabilities depend on a
distinct magical economy.

This is the selected anchor PvE faction for the initial release. It represents
an intelligent civilization rather than a loose monster category. Its forces
need:

- recognizable political or institutional motives;
- military organization and subordinate formations;
- leadership, succession, and a command topology;
- a magical resource and logistics economy;
- multiple unit roles and combined-arms relationships;
- reasons to defend, exploit, close, or expand portal access;
- objectives beyond killing every human unit; and
- consistent behavior that supports eventual playability.

#### Risk-based magic

The civilization's core magic economy uses health, accumulated strain, and
breach risk rather than an ordinary replenishing mana bar.

- A cast can fail and cause HP loss.
- The caster can voluntarily spend HP to empower chosen spell aspects.
- Casting accumulates strain.
- If strain exceeds current HP, a breach check is mandatory.
- Any HP reduction that leaves strain above current HP immediately forces the
  check, even when the caster is not casting.
- Breach severity depends on both the check result and `strain - current HP`.
- Resolving a breach discharges some accumulated strain, but does not
  necessarily reset the caster to zero strain or a safe state.
- Outcomes range from harmful backlash to catastrophic shattering events.

Spending HP creates a double risk: it pays for immediate power and
simultaneously lowers the threshold against which strain is compared. Damage
from enemy action can likewise turn previously tolerable strain into breach
risk, making focus fire, attrition, disruption, and healing relevant forms of
magical counterplay.

The exact spell aspects, casting test, strain gain and recovery, breach table,
and shattering outcomes remain to be designed. See
[Risk-Based Magic System](magic-system.md).

#### Nonmagical combined arms

The civilization also fields nonmagical goblins, orcs, trolls, and potentially
other peoples or creatures. These are not merely failed or low-level casters.
They provide durable and tactically meaningful nonmagical roles inside the
arcane force.

Substantial armor and healing are candidate characteristics for these units.
This can let them screen fragile casters, hold ground, survive human ranged
fire, recover from attrition, or serve as shock forces. The exact distribution
between goblin, orc, and troll roles remains open.

Additional candidate differentiators:

- wards, rituals, summoned effects, curses, enchantments, or constrained
  translocation;
- magical sensing that reveals different facts from human sensors;
- limited but high-impact specialists;
- preparation, components, mana, sites, or environmental conditions as
  logistics; and
- counters based on interrupting casters, anchors, rituals, or supply.

### Monster ecology or swarm

**Identity:** biological mass, unusual senses, terrain interaction, and
non-industrial replenishment.

Candidate differentiators:

- large numbers or physically extreme bodies;
- scent, vibration, magical, or collective perception;
- burrowing, climbing, leaping, or obstacle destruction;
- nesting, feeding, growth, mutation, or captured biomass as logistics;
- decentralized local behavior; and
- vulnerability to reconnaissance, fire lanes, habitat destruction, or command
  organisms where applicable.

“Monster” should not imply mindless. Separate species or factions may range from
animal behavior to sophisticated organization.

### Undead or necromantic force

**Identity:** casualty conversion, persistent bodies, magical command anchors,
and a fundamentally different relationship with morale and medicine.

Candidate differentiators:

- corpses, remains, souls, ritual materials, or necromantic energy as resources;
- reanimation or conversion under explicit battlefield rules;
- reduced dependence on food, conventional medicine, or morale;
- control through necromancers, relics, signals, or magical networks;
- unusual persistence after injury; and
- catastrophic local failure when command anchors are isolated or destroyed.

The undead must not receive free exponential growth. Corpse access, conversion
time, control capacity, resource cost, vulnerability, and counterplay must bound
reanimation.

## Faction capability contract

Every playable or server-controlled faction should expose a versioned,
machine-readable definition covering:

- faction identifier and ruleset version;
- unit and host classes;
- legal footprints, movement modes, and facing rules;
- actions and reactions;
- perception and report types;
- command and communications topology;
- WebAssembly host interfaces and budgets;
- resources and logistics;
- recruitment, creation, summoning, and replacement;
- injury, damage, repair, healing, death, and recovery;
- progression and squad-identity systems;
- equipment, magic, traits, and status effects;
- observable signatures and opponent knowledge;
- point-catalog eligibility for isolated skirmish modes; and
- scenario and campaign availability.

All factions operate through the same authoritative simulation principles, but
they do not need identical implementations of communication, morale, medicine,
or logistics.

## Asymmetry guardrails

- No faction receives privileged access to server-wide truth.
- Supernatural perception must produce defined observations, not omniscience.
- Different command networks may exist, but their advantages require cost and
  counterplay.
- Faction-specific WASM interfaces may expose different capabilities while
  preserving comparable execution fairness within each host class.
- A faction's strength should change tactical decisions rather than merely add
  universal percentage bonuses.
- Every major capability needs recognizable evidence and at least one practical
  response.
- Human technology should not become visually renamed magic.
- Magic factions should still face logistics, preparation, and loss, even when
  their resources differ from human supplies.
- Competitive point catalogs may rebalance availability and costs without
  changing the underlying capability semantics.

## Main multiplayer campaign loop

### Mission tiers

The canonical persistent campaign alternates between two mission tiers.

#### Resource missions

- Simpler, single-player missions.
- No hidden human participant is present.
- Used to gather campaign resources, recover options, intelligence, equipment,
  or other preparation value.
- Results write back to the player's persistent campaign state.
- They still require explicit risk, consumption, casualty, and anti-farming
  rules.

#### Major missions

- More consequential missions offered on a half-hour schedule.
- Access is allocated through portal-mission bidding.
- The server may place other players into the same mission.
- A player is not told whether other players are present.
- Participant presence is learned only through legitimate battlefield contact
  and information.
- The mission can therefore become PvE, PvPvE, cooperative, avoidant, or
  adversarial depending on allocation and later interaction rules.
- Results write meaningful consequences back to the campaign.

The gameplay label “major mission” means a high-consequence operation for a
mercenary company. It is distinct from a world-scale “major incursion,” for
which conventional military containment is the primary response.

### Information boundary

Before legitimate discovery, the following must remain hidden:

- whether another player is present;
- participant count;
- player or account identity;
- faction and force composition;
- deployment location and arrival timing;
- matchmaking allocation; and
- metadata from which those facts can be reliably inferred.

The restriction applies equally to:

- the canonical client;
- alternative clients;
- public APIs;
- WebAssembly modules;
- lobby and matchmaking responses;
- connection and session diagnostics exposed to players; and
- replay or spectator capabilities during the live match.

Discovery must be caused by in-world information such as visual detection,
sensors, intercepted communication, reports, observable effects, captured
evidence, or direct encounter. The exact threshold for learning that a force is
player-controlled rather than server-controlled remains open.

### Thirty-minute rhythm

With a normal match target of approximately 20 minutes, a major mission every
30 minutes creates a provisional rhythm:

```text
private resource activity and preparation
                    ↓
major-mission lock-in
                    ↓
approximately 20-minute uncertain shared mission
                    ↓
consequences, recovery, reconfiguration, and next entry decision
```

This creates strategic pressure because time spent in a resource mission,
healing, rearming, reorganizing squads, or updating pre-match WASM assignments
can affect readiness for the next major opportunity.

The architecture still needs:

- a registration and lock-in window;
- behavior when a resource mission overlaps the next major mission;
- late entry and disconnect rules;
- population-aware allocation without participant disclosure;
- treatment of parties or prearranged cooperation;
- rules for declining or missing a major mission;
- safeguards against using external coordination to identify instances;
- expected number of players per major mission; and
- reward and risk scaling when no other player is allocated.

## Design consequences

### Combat

- Humans need credible counterplay against magic through positioning,
  intelligence, specialized equipment, disruption, and preparation.
- Supernatural factions need credible counterplay against human ranged weapons,
  sensors, vehicles, and industrial logistics.
- Healing and reanimation rules directly affect lethality, executions, casualty
  recovery, and objective value.

### Communications and knowledge

- A faction's command topology can be a major source of asymmetry.
- Magical links, hive coordination, or necromantic control still need range,
  capacity, disruption, delay, interception, or anchor rules.
- Unknown player presence reinforces the established rule that clients receive
  battlefield knowledge rather than lobby truth.

### Logistics

- Human supplies and supernatural resources should interact through capture,
  denial, conversion, research, or incompatibility.
- Resource missions need varied objectives tied to actual faction dependencies,
  not one universal currency pickup.
- Major missions should threaten resources, routes, sites, and capabilities in
  addition to personnel.

### Progression

- Human progression should emphasize training, doctrine, leadership,
  technology, equipment, and adaptation without granting unexplained magic.
- Supernatural progression may follow different systems, but must remain
  inspectable and bounded.
- Faction progression should unlock options and transformations more often than
  universal stat escalation.

### WASM control

- Different factions and host classes may require different capability
  interfaces.
- The server must expose only locally available observations regardless of
  supernatural flavor.
- Standard modules must support every canonical playable faction.
- Player-defined protocols remain possible within each faction's permitted
  communications topology.

## Open design questions

1. What exact monster-kill threshold and attribution rules grant System
   recognition?
2. How socially transformed is Earth after ten years of integration?
3. What additional portal categories exist beyond the predominant temporary
   incursions, and how does each category behave?
4. What is the initial arcane civilization's culture, political structure,
   military organization, portal policy, and relationship with humanity?
5. Who licenses mercenary companies, controls portals, issues contracts, and
   resolves bids?
6. Can humans eventually acquire magic, and if so, does that create a new
   faction rather than changing the initial human baseline?
7. What exact nanomedical capabilities exist during and between missions?
8. Can players communicate or negotiate after discovering one another?
9. Is player hostility determined by faction, mission, prior campaign
    relations, or player choice?
10. How many players can share a major mission?
11. Can external parties deliberately enter the same hidden-participant
    instance?
12. Are canonical player-controlled personnel recruited only after recognition,
    or can unrecognized personnel earn recognition during a campaign?
13. What does a bid commit, and how does the server select and co-allocate
    winning bidders without leaking participant information?

## Reference

- [Solo Leveling official anime introduction](https://www.aniplex.co.jp/lineup/sololeveling/)
