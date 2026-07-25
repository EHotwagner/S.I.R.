---
title: S.I.R. Game Vision
status: proposed
document-type: living-vision
version: 0.9
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

The setting is the modern world a few years in the future. Portals have opened,
monsters are spilling through them, and magic exists. The setting integrates
these supernatural elements into an otherwise recognizable contemporary world.

### Design direction to preserve

The supernatural conflict does not replace modern tactical concerns. It exists
alongside communications, sensors, electronic warfare, logistics, squad
organization, and modern weapons. Magic and monsters must therefore interact
with the tactical system rather than forming a disconnected alternate ruleset.

## Scale and force organization

### Established vision

- A side should field approximately 50–100 units.
- Units can be organized into squads.
- A squad has a squad leader.
- Squad members communicate with their squad leader only while within an
  applicable communication distance.
- Squad leaders communicate with headquarters—the player—only while within an
  applicable communication distance.
- Squads may be sent on missions outside communication range.

### Derived implications

- Direct manual control of every individual cannot be the normal interaction
  model at this scale.
- Squad hierarchy is both an organizational structure and part of the game's
  information and command topology.
- Losing, isolating, jamming, or repositioning a squad leader can have
  significant tactical effects.
- Units outside contact may continue operating according to previously supplied
  orders and control doctrine, but cannot necessarily receive new information
  or direction.
- Communication equipment, range, relays, terrain effects, jamming, and magical
  interference are potential sources of tactical differentiation.

## Time and simulation

### Established vision

- The game uses a fixed time step.
- Play is real-time.
- The game should prioritize tactical pace over heavy simulation.

### Derived implications

- Gameplay results should be produced by the authoritative fixed-step
  simulation rather than rendering frame rate.
- Unit-control modules should observe and act at defined simulation boundaries.
- The fixed-step model provides a shared temporal basis for server authority,
  control-module execution, replays, debugging, and multiplayer synchronization.
- Simulation detail should be included only where it creates meaningful tactical
  choices or supports comprehensible outcomes.

## Space, geometry, and distance

### Established vision

- The world is grid-based.
- Units have square footprints.
- A typical human unit occupies a four-square base.
- A thickness of one grid unit can represent narrow structures such as walls
  and handrails.
- Distance uses the Chebyshev metric.

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
- A standard control module is provided so that writing a custom module is not
  required to play.
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

### Derived implications

- The server must treat every player-provided module as untrusted.
- Modules require deterministic or otherwise tightly specified execution
  semantics, resource limits, capability restrictions, versioning, and a stable
  API.
- The module API must expose only information the controlling player, squad, or
  unit is entitled to know.
- The server, not a control module, must validate actions and determine their
  results.
- Compute budgets are part of competitive fairness. A module cannot gain an
  advantage merely by consuming more server resources.
- The standard module is a major part of the game experience and balance
  baseline, not placeholder sample code.
- Custom control logic creates a player-authored doctrine layer that may become
  one of the game's defining forms of mastery.

## Player knowledge and fog of war

### Established vision

- The player is not omniscient.
- Units may exist without the player having current knowledge of them.
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

### Derived implications

- Fog of war is a knowledge system rather than a simple visibility mask.
- Reports may need observation times and provenance as factual metadata, without
  the server converting them into certainty labels.
- Reconnection may transmit accumulated reports, subject to the intended
  communication rules.
- Control modules must make decisions from their permitted local knowledge, not
  from server-wide state.
- Electronic warfare can attack the flow, quality, reliability, or timeliness
  of information and command.
- Intelligence provenance may matter: the game may need to remember which unit
  or sensor produced a report and how it reached the player.

## Communications and electronic warfare

### Established vision

- Communication range constrains contact between squad members and their leader.
- Communication range constrains contact between squad leaders and the player or
  headquarters.
- Electronic warfare is an important part of play.

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

## Tactical combat

### Established vision

- Combat should be fast-paced and tactically focused rather than highly
  simulation-heavy.
- Positioning should have very high importance.
- Stealth, flanking, and ambushes should provide meaningful advantages.
- Player-controlled automation permits strong, decisive contextual effects.
- Attacks from behind and executions are examples of such effects.

### Derived implications

- Facing, awareness, exposure, and relative position may be as important as raw
  weapon statistics.
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

1. Does “a typical human has a four-square base” mean a 2×2-cell footprint?
2. Does “one thickness” mean exactly one grid cell, and are walls and handrails
   represented as occupied cells or as edges between cells?
3. What is the simulation tick duration?
4. Is real-time play continuous, or can the player pause, slow time, or plan
   while the simulation advances?
5. At what scope does a WebAssembly module operate: individual unit, squad,
   player force, or a combination?
6. How are modules submitted, selected, updated, and versioned for a match?
7. What execution time, memory, storage, and API limits apply to modules?
8. Can modules communicate directly with one another, or only through simulated
   in-world communications?
9. What happens to existing orders when communication is lost?
10. Which observations are transmitted immediately, summarized, delayed, lost,
    or stored for later delivery?
11. Can communications be relayed through units, vehicles, infrastructure,
    drones, magic, or deployable equipment?
12. Does electronic warfare cause binary disconnection, degraded range, delay,
    false information, interception, or several of these effects?
13. How are unit facing, awareness, surprise, attacks from behind, and
    executions defined?
14. What resources constitute logistics: ammunition, energy, fuel, medical
    supplies, food, magical resources, replacements, or others?
15. What is the expected match length and physical battlefield scale?
16. Is multiplayer exclusively competitive, or does it also include cooperative
    and player-versus-environment modes?
17. Is server implementation part of the AGPL-distributed project, and are
    third-party servers intended to be supported?

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
