---
title: S.I.R. Game Vision
status: proposed
document-type: living-vision
version: "0.76"
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
- The 20 Hz simulation rate does not require every unit-control instance to run
  substantial decision logic on every tick. Scheduled wake-ups and
  authoritative event delivery allow inactive instances to remain dormant while
  movement and combat resolution continue.
- The fixed-step model provides a shared temporal basis for server authority,
  control-module execution, replays, debugging, and multiplayer synchronization.
- At the target duration and tick rate, a normal match spans approximately
  24,000 authoritative simulation ticks. Replay, storage, module metering, and
  server-capacity designs should use that order of magnitude while supporting
  shorter and longer scenarios.
- Simulation detail should be included only where it creates meaningful tactical
  choices or supports comprehensible outcomes.
- Performance tests must distinguish unique artifact compilation, per-unit
  instance execution, observation transfer, instruction use, and host-service
  cost rather than assigning one assumed duration to a generic “WASM call.”
- The authoritative simulation kernel is isolated from networking, databases,
  rendering, wall-clock queries, operating-system randomness, and other
  operational state.
- Authoritative gameplay uses integer ticks and grid coordinates, fixed-point
  values where fractions are required, explicit rounding, and deterministic
  counter-addressed randomness.
- Caches, spatial indexes, client projections, rendering state, and compilation
  caches are derived state. They can be rebuilt and cannot alter authoritative
  outcomes.
- External inputs receive authoritative target ticks and stable ordering before
  entering the match journal; clients cannot assign processing priority or
  insert inputs into committed ticks.
- Tick phases calculate keyed candidates or deltas from stable state and commit
  resolved batches. Hash-map order, thread completion, and storage index cannot
  determine gameplay.

## Action timing and tick resolution

### Established action lifecycle

Every time-consuming action uses an explicit integer-tick lifecycle:

```text
start
  → preparation
  → commitment
  → resolution
  → recovery
```

Movement transitions, attacks, spellcasting, reloads, stance changes,
interactions, medical actions, and other activities use this common lifecycle
while defining their own durations, commitment points, costs, interruption
rules, and recovery periods.

Actions that complete on different ticks resolve chronologically. Actions that
complete on the same tick use simultaneous completion:

- outcomes are evaluated from a stable common pre-resolution state;
- results are applied as a batch after those outcomes have been calculated;
- unit iteration order, player identity, module scheduling, and server thread
  order cannot decide which same-tick action happened first;
- mutual hits, mutual incapacitations, mutual kills, and other reciprocal
  outcomes are valid; and
- an effect applied on a tick cannot retroactively cancel an opposing action
  that also completed on that tick.

To interrupt an action before it resolves, the interrupting effect must resolve
on an earlier tick. If both the action and the would-be interruption resolve on
the same tick, the original action still produces its calculated outcome before
the batched consequences are applied.

### Canonical tick pipeline

The authoritative simulation processes each tick through the following logical
stages:

1. Deliver events, messages, reports, and observations available at the tick
   boundary.
2. Wake scheduled or event-triggered WASM instances.
3. Accept and validate new action requests.
4. Advance active actions and deterministic movement credit.
5. Collect movement transitions and other actions completing on this tick.
6. Resolve spatial conflicts and action outcomes from stable snapshots.
7. Apply damage, suppression, healing, resource expenditure, and other
   simultaneous results.
8. Resolve resulting incapacitation, death, cancellation of future actions,
   leadership succession, magical breaches, and other consequence chains.
9. Generate perception, communication, and report events made available by the
   resulting state.
10. Commit the completed tick for replay, audit, persistence, and transmission.

These are authoritative logical phases, not permission to expose hidden
intermediate state to clients or modules. Implementations may parallelize work
inside a phase only when doing so produces exactly the same deterministic
result.

Movement and attacks that complete on the same tick both resolve. The server
uses the movement source, destination, and swept transition envelope when
evaluating the interaction. The exact weapon-specific targeting rule remains to
be defined, but damage is applied only after the movement and attack outcomes
have been calculated. A mover incapacitated by that batch normally falls at its
committed destination because its movement transition also completed.

### Reactions

A reaction is an ordinary timed action created in response to an authoritative
trigger:

```text
trigger becomes locally observable
  → reaction opportunity
  → local WASM instance wakes
  → reaction request
  → reaction delay
  → reaction resolves
```

No reaction resolves in zero simulation time. Prepared states such as covering a
sector, guarding a doorway, or overwatch can reduce reaction delay, but cannot
eliminate the action lifecycle or retroactively affect an earlier tick.

This permits:

- a fast reaction to interrupt an action that would resolve on a later tick;
- equally timed combatants to affect one another simultaneously;
- awareness, surprise, suppression, stance, facing, preparation, injuries, and
  leadership to modify reaction timing;
- an ambusher to exploit readiness and information advantage without receiving
  arbitrary processing priority; and
- precise player-authored reaction policies without requiring continuous human
  micro-control.

If a spell and incoming damage complete on the same tick, the spell can produce
its calculated result before damage consequences are applied. That damage can
then lower current HP below accumulated strain and trigger an immediate breach
during the consequence stage.

## Space, geometry, and distance

### Established vision

- The world is grid-based.
- One grid cell represents 0.5 metres in each horizontal dimension.
- Units have square footprints.
- Every unit footprint is an axis-aligned `N×N` square in grid cells. Units do
  not use elongated or rectangular authoritative bases.
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
- A unit's occupied cells are invariant under its eight possible facing
  directions. Turning changes orientation-dependent rules but does not rotate,
  expand, shrink, or otherwise replace the authoritative square footprint.
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
- The discrete spatial model makes many line-of-sight, visibility, reachability,
  and pathfinding inputs enumerable and therefore suitable for caching,
  precomputation, or incremental reuse.
- Square, facing-invariant bases avoid the ambiguous or oversized diagonal
  occupancy produced by rotating elongated units on a square grid. Pathfinding
  clearance and collision therefore depend on base size without requiring a
  separate rotated footprint for each facing.

Facing may still affect firing arcs, observation, directional armor, access
points, animations, and action timing. Those systems reference orientation
without changing which cells the unit occupies. A visual model that appears
elongated must still fit within or be an abstraction of its authoritative square
base.

### Discrete movement representation

Authoritative unit positions remain cell-aligned. At every simulation tick, a
unit occupies one complete `N×N` footprint anchored to a grid cell. A unit never
has an arbitrary sub-cell authoritative position.

Movement is a transition between adjacent anchor cells:

```text
tick t:     footprint anchored at (x, y)
tick t + 1: footprint anchored at (x + dx, y + dy)

where dx and dy are each -1, 0, or 1
```

Orthogonal and diagonal transitions consume the same movement distance under
the Chebyshev metric. Speed is represented through deterministic fixed-point
movement credit. A unit crosses a cell boundary only when it has sufficient
credit; speeds that do not divide evenly into 20 simulation ticks distribute
their transitions deterministically over time rather than introducing
floating-point positions.

Before committing a transition, the server validates the full area swept by the
square base between its source and destination. This transition envelope:

- prevents diagonal corner cutting;
- respects terrain and unit collision;
- supplies the space-time claim used by movement reservations; and
- remains distinct from the `N×N` footprint occupied at either tick state.

Client rendering interpolates between committed tick positions for visual
smoothness. Interpolation does not affect collision, line of sight, cover,
range, targeting, or any other authoritative result.

Unit combat and perception use committed tick states. The exact within-tick
ordering of movement, perception changes, reactions, and action resolution must
be fixed and public. Fast projectiles and other effects that can cross several
cells in one tick use swept authoritative tests; this does not introduce
continuous unit positions.

### Spatial-query caching direction

The grid is not only a presentation or movement constraint; it is a performance
architecture advantage. Cells, square footprints, eight possible facings,
discrete terrain states, and a finite set of movement and sensor profiles
produce stable query keys.

Candidate cached structures include:

- static line traces and visibility relationships between cells;
- facing-dependent visibility or firing sectors;
- clearance and traversability by square footprint size and movement profile;
- connected regions, choke points, gateways, and hierarchical navigation data;
- reusable local paths or path segments;
- distance and influence fields;
- cover relationships and exposure directions; and
- sensor, communication, and effect-area masks where their rules use the same
  discrete geometry.

Complete all-pairs path or visibility tables are not automatically appropriate:
a 512×512-cell battlefield has too many cell pairs for naive universal caching.
The implementation should combine static precomputation, bounded or
on-demand caches, hierarchical representations, and incremental recomputation
according to the query type.

Static and dynamic state must be separated. Permanent map geometry can use
long-lived data keyed by map and ruleset version. Doors, destruction, smoke,
temporary magical effects, deployable cover, and moving units require explicit
spatial revision identifiers and localized invalidation. A cached result is
valid only for the geometry, occupancy, stance, height, facing, movement
profile, sensor profile, and ruleset inputs declared by that query.

Caching is an implementation optimization rather than a change to authoritative
rules. Cached and uncached evaluation must return the same deterministic result.
Servers, canonical clients, alternative clients, and WASM-facing services may
use different internal cache strategies, but they may not derive different
visibility or movement truth from them.

Authoritative caches may contain complete world geometry, but query responses
to clients and unit-control modules remain constrained by the requester's
knowledge. A pathfinding or visibility service must not reveal an unseen door,
unit, destroyed wall, magical obstruction, or other hidden state through its
result, failure reason, timing, invalidation event, or cache metadata.

### Cooperative friendly movement

Friendly units use hard authoritative collision with cooperative planning.
Their occupied footprints cannot overlap at a committed simulation state, but
the movement system treats friendly units as moving reservations rather than
permanent obstacles.

The canonical rules are:

- a moving unit reserves its complete footprint over a bounded future movement
  horizon;
- movement and collision validation run on authoritative simulation ticks;
- friendly reservation chains may allow a following unit to enter space that a
  leading unit is guaranteed to vacate, provided the time-ordered swept
  envelopes do not conflict;
- a unit can route around, slow, wait, replan, or move into a legal holding
  position to resolve friendly traffic;
- player orders and WASM policies can express movement priority and willingness
  to yield, while the server remains the final arbiter;
- equal-priority conflicts use a deterministic, replay-stable tie-breaker that
  does not permanently privilege one unit;
- movement across or through another unit is prohibited when the units' swept
  footprints intersect, including an otherwise instantaneous same-tick
  position swap;
- a corridor only wide enough for one unit's authoritative footprint does not
  permit two such units to pass; one must wait or retreat to a legal wider
  position;
- the server detects persistent waits and reservation cycles, then selects a
  legal unit to yield, move to a holding position, or replan;
- if no legal resolution exists, affected units stop and report the blockage;
  and
- an explicit coordinated displacement can ask a friendly unit to step aside or
  exchange positions, but it consumes movement time, requires valid space, and
  never permits overlap.

Friendly units do not ghost through one another, involuntarily push one another,
or temporarily shrink their authoritative footprints to pass. These shortcuts
would undermine formations, doorways, corridors, congestion, cover, and
positioning.

Cooperative resolution is a server movement service, not an implicit
communication channel between control modules. It may arbitrate already
submitted movement intents and return locally legitimate results, but it cannot
share observations, orders, destinations, or hidden state that the units could
not exchange through the simulated information network.

Friendly transitions are resolved as a reservation dependency set rather than
by iterating units in identifier order. Valid convoy movement can therefore
advance coherently, while cycles, swaps, and intersecting swept envelopes remain
blocked or are broken through deterministic yielding.

Full multi-agent pathfinding does not run for every unit on every 20 Hz tick.
Units consume short-horizon reservations at simulation frequency, while route
generation, local avoidance, reservation extension, and replanning are bounded,
staggered, or event-driven. The implementation should combine cached
hierarchical routes with local space-time reservations so traffic handling does
not scale as a new whole-map cooperative search every 50 milliseconds.

### Hostile collision and simultaneous contact

Enemy units never participate in cooperative reservations with one another.
Known hostile footprints can inform locally legitimate planning, but hostile
future movement intentions are neither shared nor reserved across sides. Hidden
hostiles remain absent from an opponent's planning state until revealed through
authoritative observation or physical contact.

The server collects movement transitions due on a tick and resolves their
footprints and swept envelopes together. Hostile conflicts use symmetric hard
resolution:

- a moving unit cannot enter a footprint occupied by a stationary hostile;
- direct hostile position swaps are prohibited;
- if two hostile transitions attempt to claim overlapping destination
  footprints or intersecting swept envelopes, neither transition receives
  arbitrary unit-identifier priority; both remain at their last legal anchors;
- a unit cannot follow into a hostile's source footprint on the same tick that
  the hostile vacates it; it can attempt the transition on a later tick; and
- hostile units cannot push, displace, ghost through, or negotiate an automatic
  yield with one another.

A movement failure caused by an unknown hostile generates only the contact or
obstruction information authorized by the observation rules. It must not reveal
the hostile through an advance path query, reservation response, timing
difference, cache event, or detailed failure reason before contact legitimately
occurs.

Physical hostile contact can trigger perception, reaction, close-combat, or
engagement events according to the combat rules, but collision alone does not
silently resolve an attack. If opposing units block one another in terrain too
narrow to pass, they must stop, withdraw, attack, suppress, displace, or use
another legal route. This is intended tactical congestion rather than a
pathfinding error.

## Command model and programmable control

### Established vision

- The authoritative server controls unit simulation.
- Unit behavior on the server is controlled by WebAssembly modules supplied by
  players.
- Each individual unit is controlled by its own WebAssembly module instance.
  There is no squad-wide or force-wide module instance with direct control over
  multiple units.
- A single uploaded module artifact can be assigned to any number of eligible
  units. The server compiles that immutable artifact once for a compatible
  runtime and host-class interface, then reuses the compiled code across all of
  its per-unit instances.
- Every unit retains isolated runtime state, memory, observations, inputs, and
  outputs even when many units execute the same compiled module artifact.
- Internal scheduling may group evaluations of units using the same artifact
  for locality and throughput, but this batching is not visible to player code.
  A module invocation receives only one unit's permitted context and cannot use
  batching to inspect or coordinate other instances.
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
- Instruction execution and host-service use are metered separately so an
  inexpensive decision cannot conceal unbounded pathfinding, visibility,
  allocation, messaging, or other server work behind host calls.
- WASM instruction execution uses a deterministic fuel allowance per
  invocation. Instances of the same host class receive the same public
  allowance, unused fuel does not accumulate, and shared compiled code does not
  create a shared fuel pool.
- Fuel exhaustion, trapping, or malformed output makes an invocation fail
  atomically without applying partial requests. Previously accepted
  authoritative actions are not retroactively removed.
- A module does not need to perform substantial work on every simulation tick.
  It can request a future wake-up tick, while authoritative events can wake it
  earlier when its local situation requires a decision.
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

- “Module” must be disambiguated in APIs and documentation: an uploaded
  **artifact** is immutable shared code, while an **instance** is one unit's
  isolated execution state.
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
- Validation, compilation, compatible-code caching, and instance preparation
  occur before live match execution. The authoritative simulation does not
  repeatedly compile the same artifact for each assigned unit or tick.
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
- Observation and command exchange should use stable, bounded buffers and avoid
  per-tick serialization and allocation where the selected ABI permits.
- Expensive shared spatial work, including authoritative pathfinding and
  visibility evaluation, belongs in deterministic cached server services rather
  than being independently recomputed by every module instance. Access remains
  knowledge-filtered, capability-limited, and separately metered.
- The scheduler should process instances that share compiled code in locality-
  friendly groups while preserving per-unit knowledge, state, budget, and
  output isolation.
- Scheduled sleeping is an optimization, not immunity from the simulation.
  Damage, observations, messages, order arrival, obstruction, action completion,
  communication changes, and other subscribed authoritative events can make an
  instance runnable before its requested wake-up tick.
- Host-class resource profiles must be authoritative, versioned ruleset data.
  Distinct profiles must correspond to actual game roles or capabilities rather
  than account privileges or server purchasing power.
- Module validation must reject binaries that target an incompatible host-class
  interface.
- The standard module is a major part of the game experience and balance
  baseline, not placeholder sample code.
- Custom control logic creates a player-authored doctrine layer that may become
  one of the game's defining forms of mastery.

See [WebAssembly Control Architecture](wasm-control-architecture.md) for the
canonical invocation, ABI, capability, sandbox, fuel, host-service, messaging,
and development-tooling contracts.

## Perception, acquisition, and awareness

### Canonical perception pipeline

Perception is resolved as four distinct processes:

```text
geometry
  → stimulus
  → acquisition
  → reaction
```

These are authoritative simulation processes, not mandatory player-facing
certainty categories.

#### Geometry

The geometry layer deterministically determines whether and how a sensing path
exists. Relevant inputs include:

- observer and target footprints;
- observation origins and target exposure points;
- range and directional sensor arcs;
- facing and attention direction;
- terrain, doors, cover, smoke, and other occluders;
- unit stance and height;
- intervening units and effects; and
- the sensor modality being evaluated.

For multi-cell units, line of sight operates between defined observation origins
and target exposure points rather than treating each unit as a dimensionless
point. Seeing an exposed part of a footprint can begin acquisition; the amount
and direction of exposure can subsequently affect acquisition, targeting, and
cover.

Geometry establishes physical opportunity. It does not by itself mean that the
observer noticed, localized, identified, or reacted to the target.

#### Stimulus

Sensors produce factual stimuli appropriate to their modality. Stimuli can
include:

- optical shape or movement;
- muzzle flash;
- sound and approximate direction;
- thermal signature;
- radar return;
- electronic emission;
- magical signature; and
- physical contact.

A stimulus reveals only the facts supported by its source and the applicable
rules. Hearing a weapon does not automatically identify its user or reveal exact
coordinates.

#### Acquisition

Acquisition represents the time required to turn available stimuli into
actionable local observations. It accumulates over simulation ticks according
to authoritative inputs such as:

```text
sensor effectiveness
× target signature
× exposed amount
× attention and facing
× environmental and status modifiers
```

The actual function need not be a literal multiplication, but its inputs and
result must be deterministic, versioned, and inspectable. Acquisition progress
belongs to a specific observer, target or stimulus, sensor, and contact episode.

When exposure or stimulus ends, progress decays according to explicit rules
rather than resetting instantly. This prevents one-tick boundary crossing from
erasing all accumulated awareness and supports coherent reacquisition.

Crossing an acquisition threshold emits the factual observation fields earned
by that interaction. It does not automatically reveal the target's complete
authoritative state.

#### Reaction

An actionable local observation creates a reaction opportunity under the
canonical timed-action model:

```text
acquisition completes
  → local observation is delivered
  → unit WASM instance wakes
  → reaction is requested
  → reaction delay elapses
  → reaction resolves
```

Acquisition time and reaction time are separate. A unit can notice a threat
before it has turned, aimed, changed stance, or otherwise become ready to act.

### Facing and attention

The simulation distinguishes:

- **body facing**, the unit's physical orientation; and
- **attention direction**, where the unit is actively looking, aiming, sensing,
  or concentrating.

Both use the eight canonical compass directions. They can coincide or differ,
allowing behavior such as strafing while watching an opening or withdrawing
while maintaining rearward attention.

An attended forward sector provides the strongest ordinary acquisition and
reaction performance. Side and rear relationships impose progressively
different limitations unless equipment, sensors, anatomy, magic, or another
explicit capability changes them. Exact sector shapes and modifiers remain
prototype parameters.

### Observation facts

The server exposes factual observations rather than requiring universal labels
such as “suspected,” “confirmed,” or “uncertain.” Depending on what was actually
observed, a local observation or delivered report can contain:

- observation time;
- observer, sensor, and provenance;
- exact or bounded location;
- visible footprint or silhouette;
- facing and current action when perceptible;
- identification glyph and HP when the applicable rules reveal them;
- observable equipment, effects, and status; and
- sound, emission, or stimulus direction.

The human player, client, and player-provided code decide how to store,
interpret, prioritize, and present those facts. A module can maintain its own
memory, but cannot obtain later world truth merely because it remembers an
earlier observation.

### Derived tactical relationships

- Geometric line of sight does not guarantee immediate awareness.
- Watching the correct direction reduces acquisition and reaction delay.
- Stealth can alter time to acquisition rather than relying only on binary
  invisibility.
- Flanking and rear approaches reduce the target's opportunity to acquire and
  react.
- Loud, bright, electronic, thermal, or magical actions can reveal bounded
  information without visual contact.
- Different sensor modalities can reveal different factual properties without
  granting omniscience.
- Local control code can respond to local observations without waiting for a
  round trip through headquarters.
- Strong awareness and reaction advantages may be decisive, but the causal
  observations and timings must remain explainable through the API and replay.

### Prototype parameters

The following are intentionally not yet canonical:

- exact forward, side, and rear sector shapes;
- acquisition rates and thresholds;
- the decay and reacquisition curve;
- the visibility sample points used by each footprint and stance;
- whether a contact episode receives a small replay-stable hidden variation in
  its acquisition threshold; and
- the exact facts revealed by each sensor and identification capability.

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
- Ranged weapons use authoritative physical shot traces rather than resolving
  only as an abstract hit roll against a selected target.
- Cover is external grid geometry that can block, mitigate, be penetrated by, or
  be damaged by an attack.
- Armor resolves after physical contact and before HP damage. It can depend on
  impact direction, coverage, damage type, penetration, and integrity.
- HP represents immediate ability to remain functional, while discrete wounds
  represent meaningful lasting consequences.
- Reaching zero HP normally causes incapacitation rather than unconditional
  immediate death.
- Suppression is an accumulating tactical state distinct from HP damage.
- Friendly units, civilians, and protected entities receive no immunity from
  otherwise valid traces or areas of effect.
- Executions are deliberate, timed, interruptible actions with strict
  eligibility rather than automatic rear-attack damage multipliers.

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

See [Combat Resolution Architecture](combat-resolution.md) for the canonical
attack, cover, armor, HP, wound, suppression, friendly-fire, and execution
pipeline.

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
- Logistics uses three layers: aggregated campaign stockpiles, a committed
  mission manifest, and physical battlefield inventories and transfers.
- Once deployed, tactically relevant resources exist at authoritative holders
  or locations and cannot move through abstract resupply auras or teleport
  between inventories without an explicit capability.
- Battlefield transfers are timed, interruptible, server-validated actions using
  authoritative reservation and ownership state.
- Combat resources are consumed at the granularity required by their rules but
  transferred and managed in meaningful packages rather than unnecessary
  individual components.
- Logistics information obeys observation, reporting, and communications
  constraints; headquarters can hold stale beliefs about battlefield stock.
- Mission-end resources have explicit consumed, extracted, secured, abandoned,
  destroyed, captured, or recovered outcomes before campaign write-back.

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

See [Logistics Architecture](logistics-architecture.md) for the canonical
stockpile, manifest, battlefield supply, transfer, automation, disruption, and
campaign write-back model.

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

### Development sequence

Persistent campaign infrastructure is not the first implementation milestone.
Development begins with a robust skirmish and mission foundation that supports
both single-player and multiplayer play without campaign write-back.

The intended sequence is:

1. establish the complete authoritative skirmish lifecycle with standardized or
   scenario-provided forces;
2. support single-player missions against server-controlled opposition;
3. support direct multiplayer missions using the same simulation, networking,
   API, WASM, deployment, objective, resolution, and replay contracts;
4. make skirmish mode a robust test and competitive environment rather than a
   disposable prototype;
5. generalize missions for PvE, PvP, and cooperative objective structures; and
6. only then implement persistent campaign reservation, bidding, hidden
   co-allocation, extraction write-back, scheduled major missions, and campaign
   transaction recovery.

Skirmish results remain isolated from campaign state. The early implementation
should nevertheless use stable match identifiers, immutable force snapshots,
objective results, outcome records, deterministic replay, and mode manifests so
the later campaign layer can consume proven match results instead of replacing
the match architecture.

See [Mission Lifecycle and Delivery Sequence](mission-lifecycle.md) for the
canonical target lifecycle and the skirmish-first implementation boundary.
See [Robust Skirmish Development Plan](skirmish-development-plan.md) for the
first playable milestones, deterministic scenario suite, scale gates, and
implementation order.
See [Deterministic Simulation Core](simulation-core-architecture.md) for the
authoritative state transition, data, input, parallelism, snapshot, replay, and
headless-execution contracts.

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
