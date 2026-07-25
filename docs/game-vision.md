---
title: S.I.R. Game Vision
status: proposed
document-type: living-vision
version: "0.63"
last-updated: 2026-07-25
---

# S.I.R. Game Vision

## Document purpose

This document is the living, authoritative description of the intended game. It
captures established design facts, organizes them into a coherent vision, and
records unresolved questions without prematurely choosing implementation
details.

Statements under **Established vision** represent explicit design direction.
Statements under **Derived implications** are consequences inferred from that
direction and remain subject to review.

## High concept

S.I.R. is a fast-paced, grid-based, real-time tactical skirmish game for large
forces in a near-future world undergoing an incursion by monsters and magic.
Portals have opened into the modern world, creating a conflict in which
contemporary military organization, sensors, electronic warfare, logistics, and
firearms coexist with supernatural threats and capabilities.

Each side fields approximately 50–100 units. The player operates as a commander,
not an omniscient hand directly manipulating every soldier. Units execute
player-provided control logic on the authoritative server, while the player
directs the wider battle through squads, communications, intelligence, and
logistical decisions.

Positioning is the decisive tactical language. Scouting, concealment, stealth,
flanking, ambushes, communication denial, attacks from behind, and executions
can produce strong advantages. Server-side automated control makes precise
coordination and micro-level reactions possible without requiring impossible
manual input from a human player. This permits tactical rules with sharper,
more consequential outcomes than a conventional RTS can support through direct
human control alone.

## Design identity

### Primary differentiation from conventional RTS games

**Established vision.** S.I.R. focuses on tactical depth and consequences that
are not practical when a human must directly control every moment-to-moment
action. Its automated,
server-side unit control is what makes this deeper tactical design space
playable. Automation does not compensate for shallow tactical rules; it enables
rules whose timing, coordination, reactions, and consequences would otherwise
exceed human control bandwidth.

The player should therefore make tactically meaningful decisions at the level
appropriate to a commander, while control modules faithfully execute detailed
behavior. The resulting game should test tactical judgment and preparation
rather than the player's ability to perform enough rapid interface actions.

**Derived implications:**

- The tactical model should not be simplified merely to keep manual
  micro-control manageable.
- Automation should expose and execute player intent, not replace tactical
  decision-making with an opaque general-purpose AI.
- Consequences may be stronger and more context-sensitive than in a conventional
  RTS because units can reliably handle precise reactions and coordination.
- The value of a control module should come from doctrine and decision quality,
  within a fair execution budget, rather than mechanical action speed.

### Established principles

- The game is tactical, fast-paced, and deliberately less simulation-heavy.
- Positioning is extremely important.
- Stealth, scouting, sensors, flanking, ambushes, and electronic warfare are
  major elements of play.
- Logistics is strategically and tactically important.
- Logistics should create meaningful decisions and disruption opportunities,
  rather than repetitive player drudgery.
- Player-supplied code handles sufficient micro-control to support more decisive
  positional effects than would be practical in a purely manually controlled
  game.
- Examples of decisive effects include attacks from behind and executions.

### General derived implications

- The game should favor clear, discrete tactical consequences over detailed
  physical simulation.
- Automation is part of the core game model, not merely an accessibility
  feature or optional convenience.
- The player's primary skill is likely to combine force organization, planning,
  doctrine design, intelligence interpretation, and intervention at decisive
  moments.
- Strong positional effects require sufficiently predictable rules and clear
  feedback so that victories feel planned rather than arbitrary.

## World and premise

### Established vision

The setting is the modern world a few years in the future undergoing a
system-integration event in the broad portal-fantasy tradition represented by
works such as Solo Leveling. Portals have opened on Earth, monsters are spilling
through them, and magic has entered the world. The reference concerns the
collision between recognizable modern society and portal-connected supernatural
realms; S.I.R. does not inherit Solo Leveling's exact lore, awakened-human model,
or power structure.

The first portals and the System appeared ten years before the game's present.
Earth has had time to create specialized forces, procedures, industries, and
experienced personnel, but integration remains a recent and ongoing
transformation.

Human factions initially have no access to magic. They operate from
recognizably realistic military, technological, medical, logistical, sensor,
communications, and organizational foundations.

The player controls a mercenary company rather than a conventional national
military force. Conventional militaries provide Earth-side security around
portals and respond to large-scale incursions. Mercenary companies bid for
portal access and conduct contracted operations, resource extraction, and other
missions.

At initial release, players command human mercenary companies. Portal-origin
factions are server-controlled PvE forces rather than playable campaign
factions. Making them playable is a later direction, so their authoritative
rules should not depend on inaccessible server-only shortcuts.

The anchor portal-origin PvE faction for the initial release is an organized
arcane civilization. It is an intelligent society with its own military
organization, command structure, logistics, objectives, and coherent magic
system rather than an undifferentiated monster force.

Its magic system is risk-based. Casting can fail and cost the caster HP. A
caster may deliberately spend HP to empower selected aspects of a spell. Casting
also accumulates strain. Whenever strain exceeds the caster's current HP, the
caster must make a breach check; the check result and the excess of strain over
HP determine consequences ranging from harmful backlash to catastrophic
shattering events. Any loss of HP immediately reevaluates the threshold, so
damage can force an already strained caster to breach even when the caster is
not performing a spell. Resolving a breach discharges some accumulated strain
but does not necessarily reset the caster to zero or restore a safe state.

The arcane civilization also fields nonmagical personnel and creatures,
including goblins, orcs, and trolls. These units provide conventional,
biological, armored, or regenerative force options rather than every faction
capability depending on spellcasters. Substantial healing and armor for some of
these unit families is a provisional direction.

Other factions can possess distinct magic systems and include monsters, undead,
and other portal-origin forces. Their supernatural capabilities are intended to
enable fundamentally different tactical and logistical play rather than
reskinned versions of human units.

In the initial setting, organized portal-origin factions are encountered
primarily inside portals or their connected mission spaces. They do not yet
maintain permanent territorial holdings or settlements on Earth. Temporary
spillover and incursions can reach Earth, and later campaign eras may change
this boundary.

The System is a literal phenomenon inside the setting. Game mechanics such as
classes, attributes, skills, progression, abilities, missions, objectives,
status effects, and applicable rules are diegetic System facts rather than
concepts that exist only in the user interface.

Direct System access is limited to recognized participants. Recognition is
earned by killing a required number or category of monsters; merely living in
the integrated world does not make a person a participant. The exact threshold
and kill-credit rules remain to be designed.

Several portal types exist. Most player missions concern temporary incursions
that appear for a limited opportunity window. Players bid for access because
successful completion can yield valuable resources.

### Design direction to preserve

The supernatural conflict does not replace modern tactical concerns. It exists
alongside communications, sensors, electronic warfare, logistics, squad
organization, and modern weapons. Magic and monsters must therefore interact
with the tactical system rather than forming a disconnected alternate ruleset.

Human limitations are part of their faction identity. Human factions should not
receive spell-equivalent abilities merely to make faction capability lists
symmetrical. Advanced nanomachinery or similarly bounded near-future technology
is a provisional option, particularly for making stabilization, healing, and
campaign recovery practical. Such technology should still consume resources,
obey logistics, and preserve injury and casualty consequences.

Every supernatural system requires explicit costs, constraints, information
rules, and counterplay. “Magic” is not permission to bypass authoritative
knowledge, grid occupancy, communications, logistics, or the public capability
contract without a defined rule.

See [Risk-Based Magic System](magic-system.md) for the casting, empowerment,
strain, breach, and shattering architecture.

The System's existence does not make participants omniscient. A person, control
module, or client receives only System facts and battlefield information that
the authoritative rules make available to that actor. Alternative clients can
present those facts differently but cannot obtain additional System knowledge.

See [Setting and Faction Architecture](setting-and-factions.md) for the faction
design framework and campaign implications.

## Scale and force organization

### Established vision

- A side should field approximately 50–100 units.
- Every unit is a persistent individual rather than an anonymous,
  replaceable rank-and-file element.
- Personnel are recruited individually. Complete squads are not the atomic
  recruitment unit.
- Recruit candidates are procedurally generated rather than drawn primarily
  from a roster of authored named mercenaries.
- Once generated, a recruit is a stable individual whose identity and history
  persist for the remainder of that campaign.
- Units can be organized into squads.
- A squad has a squad leader.
- A squad has an explicit, player-configured command succession order. The
  intended standard infantry structure includes a squad leader, a
  second-in-command, and a third-in-command; the latter two may simultaneously
  serve as subordinate team leaders.
- A standard squad cannot deploy unless the squad leader,
  second-in-command, and third-in-command assignments are all filled by eligible
  personnel.
- Squad members communicate with their squad leader only while within an
  applicable communication distance.
- Squad leaders communicate with headquarters—the player—only while within an
  applicable communication distance.
- Communication with headquarters is enabled by a physical communications
  device normally carried by the squad leader, not by the leader role alone.
- Player-provided WASM control logic executes and responds to succession using
  the squad's declared command order and the currently eligible personnel. It
  does not need to improvise an opaque succession order after a loss.
- Succession selects the first eligible person in the declared order. A
  WebAssembly policy cannot skip an eligible successor.
- A second-in-command may carry an additional headquarters communications
  device. Without such redundancy, a successor must recover or loot the fallen
  leader's device to restore headquarters communication.
- Squads may be sent on missions outside communication range.
- Personnel inspection, development, and other large-roster handling can be
  supported by scripts and AI. The amount of individual information does not
  need to be reduced solely to accommodate unaided human management.

### Derived implications

- Direct manual control of every individual cannot be the normal interaction
  model at this scale.
- Individual identity, attributes, skills, history, injuries, and progression
  require authoritative per-person records even when managed in batches.
- Procedural generation creates candidates; it does not make recruited
  personnel interchangeable or permit their identities to be regenerated.
- Each generated candidate requires a stable authoritative identifier and
  reproducible provenance sufficient for validation, auditing, and replay.
- Squad composition references individually recruited personnel; it does not
  replace their individual ownership or history with a single squad record.
- “NCO” is a personnel qualification or status, not the name of a single squad
  role. Squad leader, second-in-command, third-in-command, and team leader are
  explicit functional assignments that may require appropriate leadership
  qualification.
- Squad hierarchy is both an organizational structure and part of the game's
  information and command topology.
- Losing, isolating, jamming, or repositioning a squad leader can have
  significant tactical effects.
- Local command succession and headquarters connectivity are independent state:
  a squad can establish a new local leader while remaining disconnected from
  the player.
- The succession order, current acting leader, eligibility state, and reason for
  any skipped successor must be authoritative and visible through the API.
- A leadership handover causes only limited coordination disruption. Succession
  should preserve squad function rather than impose a severe capability
  collapse; the exact short-lived handover effect remains subject to testing.
- Member abilities remain personal, while the acting leader can give the squad
  distinctive doctrine, reaction, formation, coordination, communications, and
  logistics characteristics. These should be conditional behavioral effects
  rather than broad, stackable stat auras.
- Command-qualified individuals can define primary and secondary leadership
  effects. The acting Squad Leader contributes their primary effect; the 2IC and
  3IC contribute narrower or weaker secondary effects while remaining
  subordinate.
- On succession, the new acting leader's primary effect becomes active and the
  command team's secondary effects are recalculated from the surviving role
  assignments. Effects cannot remain active merely because their former source
  was killed, incapacitated, disconnected, or reassigned.
- Leader succession can change or temporarily degrade leader-dependent
  characteristics.
- A squad has an identity record from creation, but its cohesion, traditions,
  and distinctive squad-level traits should provisionally emerge through shared
  training, missions, and history rather than arrive fully formed.
- A usable doctrine can still be assigned immediately so a new squad is
  functional. Emergent identity modifies or specializes that foundation; it
  must not be required for baseline competence.
- Emergent squad identity is a bounded prototype hypothesis because it carries
  substantial readability, balance, content, and implementation risk. The core
  command and combat architecture must remain viable if this layer is reduced
  or removed after testing.
- Communications devices are authoritative equipment with location, ownership,
  operational state, range, and transfer rules.
- Carrying a redundant device trades logistics or equipment capacity for command
  resilience.
- Recovering a fallen leader's device creates a physical tactical objective and
  exposes the recovery unit to positional risk.
- Units outside contact may continue operating according to previously supplied
  orders and control doctrine, but cannot necessarily receive new information
  or direction.
- Communication equipment, range, relays, terrain effects, jamming, and magical
  interference are potential sources of tactical differentiation.
- Personnel-management APIs need deterministic policies, batch operations,
  exception handling, and explanations so automation remains inspectable and
  overridable by the player.

See [Squad Command, Identity, and Succession](research/squad-command-and-succession.md)
for the reference structures and proposed S.I.R. model.

## Personnel progression

### Established vision

- Every individual can develop persistent attributes, skills, history, and
  consequences during a campaign.
- Personnel development can be managed or assisted through scripts and AI.
- Every individual has a fixed class that establishes their core role. The
  assigned class is permanent for the campaign.
- A recruit's class is predetermined before recruitment and visible to the
  player when evaluating that recruit. Recruiting the individual does not
  trigger a class-selection decision.
- Progression includes semi-random opportunities. Fully predetermined
  advancement would become a solved optimization problem too quickly.
- Randomness determines which eligible opportunities are offered; the human
  player or authorized automation chooses among them.
- Numerical attribute growth is primarily automatic rather than manually
  allocated point by point.

### Provisional direction

Recruitment should provide an XCOM- or Jagged Alliance-style personnel dossier.
Before committing, the player can inspect the candidate's class, current
attributes, proficiencies, learned abilities, traits, relevant history, known
injuries or conditions, and recruitment terms. The exact presentation and
number of fields remain to be designed.

This disclosure applies to the recruit's current known state, not their complete
future development. Later semi-random progression offers remain unrevealed
until generated. Recruitment should be an informed force-building decision
without turning each candidate into a perfectly forecastable final build.

Major active abilities should be unlockable through personal progression.
Progression should therefore change what an individual can do, not merely
increase numerical effectiveness or improve use of equipment.

Equipment, magic, implants, and software may still grant, enable, constrain, or
modify active capabilities. The exact division between learned abilities and
equipment-dependent actions remains to be designed.

Automatic attribute development may be influenced by experience, training,
assignment, and other in-world causes. Direct manual allocation can remain
available for exceptional systems if later testing demonstrates a need, but it
is not the default personnel-development interaction.

Specialized advanced classes may be introduced later as an evolution of a
unit's permanent base class. This is a provisional extension, not permission for
general class changes or unrestricted multiclassing.

The leading direction is that a semi-random offer set is final once generated:
the player or authorized automation must choose from that set without rerolling
it. This has not yet been accepted as an established rule. A later retraining or
respec system, if introduced, would modify an already selected outcome rather
than regenerate the historical offer.

### Derived implications

- Active abilities require stable machine-readable definitions available to
  canonical clients and WASM control modules.
- Progression automation must be able to reason about newly unlocked actions,
  prerequisites, conflicts, and the behaviors that can use them.
- Automatic growth rules must be authoritative, machine-readable, and protected
  against farming through repeatable WASM-controlled actions.
- Offer generation should account for relevant facts such as role, history,
  existing development, and campaign events so that randomness produces
  coherent individuals rather than arbitrary builds.
- A class must have a stable public identifier and a machine-readable
  capability and progression contract.
- Recruitment interfaces and APIs must expose a candidate's class before the
  player commits to recruiting them.
- The canonical client and public API should expose the same authoritative
  recruitment-dossier data; a third-party client must not gain access to hidden
  future progression rolls.
- Advanced-class definitions, if introduced, must declare their required base
  class and preserve an inspectable class lineage.
- Semi-random opportunities should create distinct builds within a class
  without making its core tactical role unreadable.
- The server must generate offers authoritatively and expose enough information
  to audit eligibility and selection without allowing reroll manipulation.
- A unit's available action set is derived from both persistent personnel state
  and current mission loadout.

## Time and simulation

### Established vision

- The game uses a fixed time step.
- The authoritative simulation runs at 20 ticks per second, making each fixed
  simulation step 50 milliseconds.
- Play is real-time.
- Once a live match begins, the authoritative simulation advances continuously
  at real-time speed. Players cannot pause, slow, accelerate, or otherwise
  change simulation speed.
- A normal match should last approximately 20 minutes from deployment to
  resolution.
- The game should prioritize tactical pace over heavy simulation.

### Derived implications

- Gameplay results should be produced by the authoritative fixed-step
  simulation rather than rendering frame rate.
- Client rendering may run at a different frame rate and interpolate between
  authoritative simulation states without changing gameplay outcomes.
- Unit-control modules should observe and act at defined simulation boundaries.
- The fixed-step model provides a shared temporal basis for server authority,
  control-module execution, replays, debugging, and multiplayer synchronization.
- At the target duration and tick rate, a normal match spans approximately
  24,000 authoritative simulation ticks. Replay, storage, module metering, and
  server-capacity designs should use that order of magnitude while supporting
  shorter and longer scenarios.
- Simulation detail should be included only where it creates meaningful tactical
  choices or supports comprehensible outcomes.

## Space, geometry, and distance

### Established vision

- The world is grid-based.
- One grid cell represents 0.5 metres in each horizontal dimension.
- Units have square footprints.
- A typical human unit occupies a contiguous 2×2-cell footprint.
- A unit can face any of the eight compass directions: north, northeast, east,
  southeast, south, southwest, west, or northwest.
- A structure with thickness `1`, including a wall or handrail, occupies a full
  grid cell rather than an edge between cells.
- Distance uses the Chebyshev metric.
- A typical battlefield should be approximately 512×512 cells, representing
  256×256 metres. Individual scenarios may use smaller or larger maps.

For two grid positions, Chebyshev distance is:

```text
distance = max(abs(x2 - x1), abs(y2 - y1))
```

This makes orthogonal and diagonal displacement equivalent when their maximum
axis displacement is the same.

### Derived implications

- Footprint-aware placement, movement, pathfinding, line of sight, cover, and
  adjacency are foundational systems.
- Tactical rules cannot assume that an entity occupies only one cell.
- A typical human's 2×2-cell footprint represents 1×1 metre of occupied tactical
  space, including body, equipment, stance, and clearance rather than literal
  body dimensions.
- A one-cell terrain footprint reserves a 0.5-metre spatial band for collision
  and tactical interaction; it does not imply that every depicted wall or
  handrail is literally 0.5 metres thick.
- A common spatial-query model should be used across movement, targeting,
  sensing, communications, and AI to prevent inconsistent interpretations of
  range.
- Thin terrain features need explicit rules for traversal, collision, sight,
  projectiles, cover, and destruction.

## Command model and programmable control

### Established vision

- The authoritative server controls unit simulation.
- Unit behavior on the server is controlled by WebAssembly modules supplied by
  players.
- Each individual unit is controlled by its own WebAssembly module instance.
  There is no squad-wide or force-wide module instance with direct control over
  multiple units.
- The player's client communicates with the WebAssembly instance at
  headquarters. Commands propagate outward from headquarters through the
  simulated communications hierarchy, and reports propagate back through that
  hierarchy to headquarters and the client.
- The game does not prescribe the semantic command-and-report protocol used
  between a player's client and modules. Message formats, meanings, and
  higher-level behavior are chosen by the player and client.
- A standard control module is provided so that writing a custom module is not
  required to play.
- The project provides an official example client/module communication protocol,
  but custom clients and modules are not required to use it.
- Each player account has an account-scoped library of uploaded WebAssembly
  modules. Players may upload modules between matches, not during a live match.
- Before a match begins, the player selects uploaded modules for the units they
  will control.
- The module binary and version assigned to a unit are locked when the live
  match begins and cannot be replaced or updated during that match.
- WebAssembly modules are specialized for a declared host class. Headquarters,
  vehicles, and different unit classes may use different module types,
  capability interfaces, and resource profiles.
- Execution, memory, storage, and API usage are strictly limited. Every module
  instance in the same host class receives the same limits.
- Player code can handle micro-control and reactions that would be impractical
  for direct human control.
- Moving this control close to the authoritative simulation makes network
  latency less important for moment-to-moment unit behavior.

### Role of control modules

Control modules are part of the player's command capability. They allow units
to carry out doctrine, react to local conditions, coordinate detailed actions,
and exploit short-lived tactical opportunities without waiting for a
round-trip command from a remote client. This capability is also a design
prerequisite for the intended tactical depth: rules can demand reactions,
precision, and coordination beyond ordinary human micro-control because their
detailed execution is delegated to player-chosen code.

They should support the intended distinction between:

- strategic or tactical intent supplied by the human player;
- local information available to a unit or squad;
- autonomous execution performed by player-selected code; and
- authoritative validation and resolution performed by the game server.

### Provisional direction

The effect of communication loss on previously received orders is not yet
determined. A leading option is for the server to preserve still-valid order
state and let the disconnected unit's WebAssembly module decide whether to
continue, suspend, reinterpret, or abandon that order according to
player-authored doctrine. The server would continue to enforce action validity
and world rules.

### Derived implications

- Runtime state is isolated per unit even when many units use the same module
  binary and configuration.
- A module can directly issue actions only for its own unit.
- Multi-unit coordination must use player orders, common doctrine, or
  communication paths permitted by the simulation; it cannot rely on shared
  process memory or a privileged squad-level controller.
- The HQ instance is a communications endpoint and participant, not a
  privileged force-wide controller that can bypass the simulated network.
- Module-to-module messages may use player-defined payloads, but the server
  controls their routing, delivery eligibility, timing, and resource limits.
- The public API must provide a stable transport envelope even though the game
  does not define the application-level semantics of player messages.
- Each accepted upload should become an immutable, content-addressed module
  artifact so match configuration, validation, replays, and audits identify the
  exact code that ran.
- Upload validation and deployment selection are separate operations: a
  validated artifact can be reused across units and later matches without
  sharing runtime state between its instances.
- Locking deployed code does not freeze runtime state: modules may continue to
  process permitted messages and change their internal state during the match.
- The server must treat every player-provided module as untrusted.
- Modules require deterministic or otherwise tightly specified execution
  semantics, resource limits, capability restrictions, versioning, and a stable
  API.
- The module API must expose only information the controlling player, squad, or
  unit is entitled to know.
- The server, not a control module, must validate actions and determine their
  results.
- Compute budgets are part of competitive fairness. A module cannot gain an
  advantage merely by consuming more server resources, and two instances in the
  same host class cannot receive different budgets.
- Host-class resource profiles must be authoritative, versioned ruleset data.
  Distinct profiles must correspond to actual game roles or capabilities rather
  than account privileges or server purchasing power.
- Module validation must reject binaries that target an incompatible host-class
  interface.
- The standard module is a major part of the game experience and balance
  baseline, not placeholder sample code.
- Custom control logic creates a player-authored doctrine layer that may become
  one of the game's defining forms of mastery.

## Player knowledge and fog of war

### Established vision

- The player is not omniscient.
- Units may exist without the player having current knowledge of them.
- In PvP, a player receives no privileged pre-match inspection of opposing
  personnel, progression, perks, abilities, doctrine, or loadouts.
- Information about an opposing build is available only when it can be observed
  or inferred from the battlefield information available to the player.
- A unit or squad can operate beyond communication range.
- Information must travel through the command structure: squad members connect
  to their squad leader, and squad leaders connect to headquarters/the player,
  subject to range.
- Scouting and sensors are important.

### Coherent information model

The game distinguishes at least three concepts:

1. **World truth** — the authoritative state known to the server.
2. **Local knowledge** — information currently available to a unit or squad
   through its own perception, sensors, and communications.
3. **Player knowledge** — information that has successfully reached
   headquarters/the player and has not necessarily remained current.

The player may therefore have no information, delayed information, or stale
information about a location or unit. A squad outside communication range can
continue to encounter and respond to the world without immediately updating the
player.

Information is not currently divided into system-defined certainty categories
for presentation. The server provides the information available through the
game's observation and communication rules; the human player or client decides
how certain, uncertain, or significant that information is. Future research or
data-gathering capabilities may provide additional evidence without requiring a
universal certainty classification.

When a previously observed hostile unit is no longer observable through the
player's currently available information, the canonical client removes it
entirely. It does not retain a last-known marker or ghost representation.

### Provisional report model

Observation reporting should follow fixed, authoritative game rules rather than
letting a unit's WebAssembly module decide which authoritative observations
exist. These rules determine which local observations generate reports and
whether those reports are immediate, summarized, delayed, lost, or retained for
later delivery.

Fixed observation reports are distinct from the player-defined command and
message protocol. Modules may use custom messages for doctrine and coordination,
but they cannot suppress, invent, or modify authoritative report facts. The
client remains responsible for presenting and interpreting delivered reports;
fixed report rules do not imply fixed certainty categories in the interface.

### Derived implications

- Fog of war is a knowledge system rather than a simple visibility mask.
- Reports may need observation times and provenance as factual metadata, without
  the server converting them into certainty labels.
- Reconnection may transmit accumulated reports, subject to the intended
  communication rules.
- Control modules must make decisions from their permitted local knowledge, not
  from server-wide state.
- Opponent build data must not be exposed through the public API unless the
  active player's battlefield knowledge currently reveals that data.
- Using an active ability, carrying visible equipment, or producing an
  observable effect may reveal information without exposing the unit's complete
  build.
- Electronic warfare can attack the flow, quality, reliability, or timeliness
  of information and command.
- Intelligence provenance may matter: the game may need to remember which unit
  or sensor produced a report and how it reached the player.

## Communications and electronic warfare

### Established vision

- Communication range constrains contact between squad members and their leader.
- Communication range constrains contact between squad leaders and the player or
  headquarters.
- Control modules cannot communicate through a privileged out-of-world
  backchannel. Their messages follow the simulated communications topology:
  client to HQ, HQ to squad leaders, squad leaders to members, and the reverse
  path for reports.
- The payload protocol used across those links is player-defined rather than a
  fixed gameplay protocol.
- Headquarters connectivity depends on an operational communications device
  carried by the acting leader or another appropriate unit.
- Electronic warfare is an important part of play.
- The communications architecture must support electronic-warfare effects that
  cause disconnection, degrade range, delay messages or reports, intercept
  traffic, and inject false information.

### Derived implications

Communications form a dynamic tactical network. Its state influences:

- which observations reach the player;
- which new orders reach squads;
- whether squads can coordinate with one another;
- how quickly information becomes stale;
- whether local control modules must operate independently; and
- how reconnaissance becomes actionable intelligence.

Electronic warfare should consequently affect gameplay at the command and
information layers, not exist only as a numeric combat modifier.

Not every electronic-warfare capability must produce every effect. Individual
equipment, abilities, environmental conditions, or magical effects may use
subsets of the supported disruption model. Interception and false-information
effects require explicit attribution, authorization, and knowledge-state rules
so they cannot expose authoritative world truth or become indistinguishable from
server corruption.

If a leader is lost, player-provided WASM logic determines local succession.
Succession does not create a communications device. A prepared second-in-command
may already carry a redundant device; otherwise, a unit must physically recover
or loot the original device before the squad can re-establish its equipment path
to headquarters.

## Tactical combat

### Established vision

- Combat should be fast-paced and tactically focused rather than highly
  simulation-heavy.
- Positioning should have very high importance.
- Stealth, flanking, and ambushes should provide meaningful advantages.
- Awareness is strongly directional. A unit perceives most effectively in its
  forward direction, with reduced awareness toward its sides and rear.
- Player-controlled automation permits strong, decisive contextual effects.
- Attacks from behind and executions are examples of such effects.
- Door Kickers 2 is a primary qualitative reference for the combat, reaction,
  facing, and awareness model.

### Reference boundary

The intended reference is Door Kickers 2's emphasis on readable sightlines,
deliberate facing and aim direction, autonomous reactions, reaction timing,
coordinated movement, surprise, and rapidly lethal close engagements. These
features make preparation and the direction from which contact occurs matter as
much as raw weapon power.

S.I.R. does not inherit every surrounding Door Kickers 2 rule. In particular,
S.I.R. remains grid-based, runs continuously without pause, operates at a much
larger force scale, and delegates detailed execution to per-unit WebAssembly
modules. The reference describes the desired tactical relationships and combat
feel, not the control interface or spatial simulation.

See [Combat, Reaction, and Awareness Reference Models](research/combat-awareness-models.md)
for the maintained comparison and adaptation notes.

### Provisional direction

Relative position and awareness should be distinct, cumulative tactical factors.
Attacking through a target's rear arc can grant an increased effect, and
attacking while the target is unaware can grant an additional increased effect.
Combining rear position with lack of awareness should produce the strongest
outcome and may create an execution opportunity. Exact effects, eligibility,
counterplay, and lethality require prototyping.

### Derived implications

- Facing, awareness, exposure, and relative position can be as important as raw
  weapon statistics.
- The exact forward cone, peripheral zones, rear coverage, and effects of
  sensors, stance, movement, suppression, and unit class require tactical
  prototyping.
- An unaware or poorly positioned force can be defeated quickly.
- Scouting and communication failures can directly create lethal openings.
- Tactical effects must be discoverable by control modules through the official
  API and understandable to players through the canonical client.
- The combat model should avoid detail that does not materially change a
  player's decisions, control doctrine, or interpretation of the battle.

## Logistics

### Established vision

- Logistics plays an important role.
- Logistics disruption should create interesting gameplay.
- The core logistics model includes ammunition, fuel, energy, medical supplies,
  food, magical resources, replacement personnel, and spare parts.
- Routine logistical control should be handled substantially by AI or code so
  that it does not become drudgery for the player.

### Intended role

Logistics creates dependencies, routes, vulnerable assets, preparation choices,
and reasons to operate beyond direct combat. Automation handles routine
execution while the player decides priorities, acceptable risks, force
allocation, and responses to disruption.

### Derived implications

- Logistics should be represented through actionable tactical state rather than
  excessive manual inventory handling.
- Supply availability can shape operational capability and make reconnaissance,
  interdiction, escort, raiding, and area control meaningful.
- These logistics categories do not all need identical storage, transport, or
  consumption rules. Replacement personnel in particular remain persistent
  individuals rather than a fungible numeric commodity.
- The exact subtypes, unit scales, containers, transfer rates, consumption
  rules, and abstraction level require a dedicated logistics model.
- Control modules may need logistics-related observations and intentions, while
  the authoritative server retains ownership of resources and transfers.
- Logistics and communications together can create battles about networks and
  access, not only elimination of enemy units.

## Multiplayer and platform openness

### Established vision

- The game is multiplayer.
- The game exposes an API.
- The project provides a canonical client.
- Anyone may develop an alternative client.
- The game is licensed under the GNU Affero General Public License (AGPL).
- The authoritative server implementation is part of the AGPL-licensed project,
  not a closed-source service separate from the released game.
- Independent third-party servers are encouraged, but they are not a supported
  compatibility target that obligates the canonical project to spend
  development or operational resources on them.
- The game supports multiple modes involving player-versus-environment,
  player-versus-player, and cooperative play.
- The main mode has persistent personnel whose progression carries across
  matches.
- Persistent personnel belong to the player account.
- In the setting, the player account's persistent force represents its
  mercenary company.
- Main-mode campaigns run for a fixed duration. At the end of a campaign, its
  personnel, progression, and other campaign state are wiped.

### Campaign lifecycle

Persistence is seasonal rather than permanent. A player develops account-owned
personnel across the matches of one active campaign, accepting losses and
building progression during that campaign. The campaign has a defined end, after
which its accumulated state is reset and a new progression cycle can begin.

### Campaign mission rhythm

The main multiplayer campaign has two mission tiers:

1. **Resource missions** are simpler single-player missions used to gather
   resources and prepare the persistent force.
2. **Major missions** become available on a scheduled half-hour cadence and can
   place more than one player into the same mission.

Portal-mission access is allocated through bidding. A player can learn the
mission information and outcome of their own bid that the rules disclose, but
the bidding interface must not reveal how many other players bid, who they are,
or whether any were allocated to the same incursion.

The scheduled gameplay term **major mission** describes a consequential company
operation. It does not necessarily mean a civilization-scale **major
incursion**, which conventional militaries primarily contain.

A player entering a major mission is not told whether any other player has been
placed in that mission. The server, public API, and canonical client must not
reveal participant count, identity, faction, roster, or spawn information
through pre-match lobby or session metadata. Another player's presence becomes
known only after their forces meet through information legitimately acquired on
the battlefield.

Major missions therefore create uncertain PvPvE encounters. A player must be
prepared for a purely PvE operation, an encounter with another player, or
several possible relationships after contact. The exact rules for cooperation,
hostility, negotiation, identification, and rewards are not yet determined.

At the normal 20-minute match target, a half-hour major-mission cadence leaves
approximately ten minutes for consequences, force preparation, module and
loadout selection, and entry into the next scheduled opportunity. Queue,
lock-in, late-entry, and missed-window rules require dedicated design.

### Provisional canonical cadence

The current candidate is a two-week campaign duration with a new campaign
starting every week. This would normally keep two overlapping campaign cohorts
active. The cadence is not yet accepted and requires testing; whether one player
account may participate in both overlapping campaigns is also unresolved.

Separate single-player campaigns may maintain their own campaign state and
lifecycle. Because the project is open source, third-party servers and
derivatives may define additional modes, campaign structures, persistence
policies, and reset schedules beyond the canonical offering.

Third-party operators may reuse, modify, and deploy the released server under
the project's license. They are responsible for their own deployment,
operations, modifications, compatibility, and migration work. Public
documentation should make independent use practical, but no compatibility,
support, uptime, federation, or migration guarantee is implied.

### Additional competitive modes

Dedicated PvP duels and skirmishes are an intended direction. These modes should
use point-based force construction from a standardized unit catalog. Persistent
main-mode personnel are not used to construct these forces. The catalog provides
defined unit, progression, equipment, magic, and doctrine options with public
point costs, allowing bounded and reproducible competitive forces. Match results
are completely isolated from the persistent main campaign: they do not
grant campaign rewards or progression and do not write injuries, losses,
equipment changes, or other consequences back to campaign personnel or state.

### Derived implications

- The server API is a first-class public product contract.
- The canonical client must not depend on secret or privileged gameplay
  information unavailable to alternative clients, except for explicitly
  administrative capabilities.
- API versioning, capability discovery, compatibility policy, and protocol
  documentation are necessary parts of the architecture.
- The server must enforce knowledge boundaries independently of client behavior.
- Spectator, replay, observer, and administrative access—if provided—need
  identities and permissions distinct from those of an active player.
- Licensing of dependencies and linked or distributed components must remain
  compatible with the intended AGPL release.
- Server build, configuration, deployment, migration, and API documentation must
  be sufficient to make the released server source usable and auditable.
- Canonical APIs and rulesets should be clearly versioned for the canonical
  service's own evolution; this does not require preserving compatibility with
  independently modified servers.
- Persistence, roster eligibility, progression write-back, and balance policy
  must be explicit properties of each game mode.
- The public API must expose the active mode and ruleset so canonical and custom
  clients can construct and validate eligible forces.
- Persistent main-mode play and bounded point-based competition can share unit,
  equipment, and capability definitions while applying different roster and
  progression policies.
- Duel and skirmish catalog content, eligibility rules, and point calculations
  must be versioned, public, and machine-readable.
- Public catalog definitions do not imply disclosure of which catalog entries an
  opponent selected; match knowledge remains constrained by battlefield
  observation.
- Persistent records must be scoped by player account and campaign identifier so
  that one campaign's state cannot leak into another.
- Campaign duration and reset timing must be public ruleset data.
- Major-mission schedule and entry eligibility are public ruleset data, but
  actual participant allocation is hidden match information.
- Bid rules and a player's own bid state must be available through the public
  API. Competing bids, bidder counts, clearing information that reveals
  participation, and co-allocation remain hidden.
- Custom clients must not infer hidden participants from matchmaking, session,
  connection, API, timing, or resource-allocation metadata.
- Resource missions and major missions write consequences into the same
  campaign state under explicit mode policies.
- The architecture must support canonical modes without hard-coding them as the
  only possible modes or persistence policies.

## Visual direction

### Established vision

The graphical direction is represented by the current
[unit-footprints and information-faces concept](assets/concept-art/unit-footprints-and-information-faces.png).
It presents an isometric, three-dimensional battlefield in which units have
clearly readable authoritative footprints and act as information-bearing game
pieces. Their top faces communicate identity and state, while separate base and
ground overlays communicate spatial properties such as footprint, facing, and
attention.

The depicted box-shaped unit bodies are the intended final visual abstraction,
not placeholders for conventional character or vehicle models.

At normal gameplay zoom, a unit's identification glyph and hit points must
remain readable. Which additional details should remain visible at that distance
will be decided through prototyping and playtesting.

Tactical overlays must be customizable by the player and client. Distinct
information layers—such as line of sight, sensor coverage, communication range,
and electronic-warfare effects—can be shown or hidden as needed to avoid
overwhelming the battlefield. Hotkey toggles are a possible canonical-client
interaction, but the exact controls and any automatic context-sensitive display
remain open for testing.

Color may be used as a standalone information category; color-coded distinctions
do not require redundant glyphs, patterns, or labels. Accessibility should be
supported through customizable color schemes, with the canonical client's exact
palette controls determined through testing.

The concept supports the game's emphasis on readable positioning and command at
scale. Modern military units, vehicles, logistics objects, monsters, and magic
share a consistent visual language.

Individual labels, glyphs, colors, proportions, roles, and status examples in
the concept remain provisional unless established separately. The detailed
interpretation is maintained in [the visual-direction document](visual-direction.md).

## Core experience

The intended battle is not a contest of raw click speed. A player prepares
forces and control doctrine, organizes squads, establishes reconnaissance and
communications, maintains supply, and issues tactical intent. Units then
execute that intent locally through server-hosted control modules.

The player receives only the intelligence that friendly forces can observe and
successfully communicate. Enemy scouting, stealth, jamming, maneuver, and
logistics attacks can undermine both the player's picture of the battle and the
ability to respond. Strong positioning and precise automated reactions allow a
well-prepared smaller or better-informed force to create decisive local
advantages.

## Foundational design invariants

The following invariants are implied strongly enough by the current vision to
guide subsequent design:

1. The server is authoritative over world state and action resolution.
2. Clients and player-provided control modules are not trusted with hidden
   information.
3. Control logic may decide intent or actions only from its permitted knowledge.
4. The human player does not automatically receive all observations made by all
   friendly units.
5. Communication topology affects both command and knowledge.
6. Routine automation must reduce workload without removing consequential
   tactical and logistical decisions.
7. Position and information must be capable of creating decisive advantages.
8. Simulation detail must justify itself through tactical consequence.
9. The public API and canonical client operate against the same documented game
   contracts.
10. Spatial rules must consistently account for multi-cell square footprints
    and Chebyshev distance.
11. Automation must be used to expand tactical depth and consequence, not to
    remove meaningful tactical agency from the player.

## Open questions

These questions are recorded for later development and do not block the current
vision:

1. What precise authority does a unit's WebAssembly policy have over existing
   orders after communication is lost?
2. What are the exact fixed rules determining which observations become
   reports and whether they are immediate, summarized, delayed, lost, or stored
   for later delivery?
3. Can communications be relayed through units, vehicles, infrastructure,
    drones, magic, or deployable equipment?
4. What exact directional-awareness zones, rear-attack effects, and unawareness
   thresholds produce surprise and execution opportunities?
5. Should the provisional rule that semi-random offer sets cannot be rerolled
   become final?
6. What monster kills qualify for System recognition, how many are required,
   and how is credit assigned among cooperating units?
7. What are the exact spell aspects, casting checks, strain recovery rules,
   breach table, and shattering outcomes of the initial arcane civilization?
8. What limits, costs, and recovery times apply to provisional human
   nanomedical technology?
9. After players discover one another in a major mission, what determines
   hostility, cooperation, identification, communication, victory, and
   write-back consequences?
10. What does a portal-access bid commit—currency, resources, reputation,
    forces, risk, reward share, or another scarce value—and how are winning bids
    selected?
11. Who licenses mercenary companies, controls portal access, and administers
    mission bidding?

## Future derived documents

When the vision contains enough detail, this document should drive—not be
replaced by—the following architecture documents:

- simulation and tick model;
- spatial and footprint model;
- knowledge, perception, and report model;
- communications network model;
- command and order model;
- WebAssembly control-module contract;
- multiplayer server and public API contract;
- movement, positioning, and pathfinding model;
- combat and awareness model;
- logistics model;
- electronic-warfare model; and
- canonical client responsibilities;
- canonical client information design; and
- final visual language and tactical-overlay specification.
