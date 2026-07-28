---
title: S.I.R. Game Vision
category: Overview
categoryindex: 1
index: 3
status: proposed
document-type: living-vision
version: "1.17"
last-updated: 2026-07-28
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

The game translates turn-based tactical micro-decisions into continuous
real-time doctrine and execution. The human commander chooses purpose,
priority, acceptable risk, commitment, and doctrine. HQ, leader, and unit
modules execute precise reactions, timing, formation behavior, fire discipline,
and routine contingencies. The authoritative server owns information
boundaries, legality, simultaneous resolution, and consequences.

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

Every senior caster normally leads a cell of two or three persistent magical
assistants. Assistants possess lesser spells and ritual abilities and contribute
preparation, maintenance, stability, interruption tolerance, and controlled
shutdown. Minor workings may use one complete caster cell, but major rituals
and portals still require multiple senior casters. Assistants cannot substitute
without limit for senior casters or erase HP, Strain, component, anchor-load, or
breach costs.

Arcane anchored information flow is asymmetric. Legitimate observations and
status from an anchored unit, including observations borrowed from an attuned
critter, reach the controlling caster on the next authoritative tick without
distance or relay delay. A deliberate caster command reaches an anchored
subordinate after a flat 20 ticks or one second. Distance within one anchor
influence does not change that delay, no subordinate relay chain exists, and a
unit outside valid influence receives no new command.

Arcane rituals are distinct from individual spells. They are site-bound,
observable actions requiring multiple casters to maintain a quorum. They commit
their target location or prepared trigger before completion and may produce
delayed geographical effects, finite ritual traps, or portals. Transit portals
move precommitted arcane personnel and supply and retain their ownership. Goblin
portals deliberately release unaffiliated goblins that are hostile to every side
and count toward no arcane supply, command, stake, or anchor capacity. Daemon
portals are uncontrolled catastrophic anchor failures. A ritual cannot track an
unknown target or use privileged server knowledge.

Harmless ambient critters are neutral ordinary actors with natural movement and
species-appropriate perception. Arcane casters can attune one and receive only
the observations it actually earns, without controlling it or improving its
senses. Those facts may establish knowledge needed to place a ritual or portal.
Active attunement leaves evidence, so humans can detect, exclude, capture,
drive off, kill, or mislead suspected critters.

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
- Communication uses nets rather than a bare hierarchy. A net is a set of
  participants sharing a channel; command topology describes who reports to
  whom. A squad shares a squad net, and leaders share the command net with
  headquarters, so a squad leader participates in both.
- A net's throughput is shared among everyone on it, so putting more of a force
  onto one net degrades it for all of them. This is what keeps the command
  topology from being decorative.
- Saturation collapses rather than sagging. Past the capacity of a force's
  command structure, contention causes throughput to fall off a cliff, which
  costs the whole force its coordination rather than costing the marginal units
  their effectiveness. The cause is deterministic and learnable even though the
  specifics are not, so a collapse is explicable rather than arbitrary.
- Command capacity is structural — qualified leaders, command-net sets, and
  relays — so it can be bought, at the direct expense of fighting power. And
  load can be shed, which makes withdrawing units a tactical action rather than
  a concession.
- Arcane command capacity is structural for the same reason but is carried by
  anchors rather than communications nets. Every anchored force imposes load;
  supporting more units requires additional or stronger anchors, at the expense
  of components, casters, defended ground, and magical concealment.
- An overloaded anchor becomes supernaturally unstable rather than merely
  serving its marginal units less well. Instability is observable before
  catastrophe, disrupts the coordination of the force relying on the anchor,
  and can resolve as indiscriminate lightning discharge or, at its severe
  extreme, an uncontrolled daemon portal hostile to every side.
- Arcane anchoring load can be shed by detaching or withdrawing units. Its
  failure is centred on the overloaded anchor and must endanger its own force
  strongly enough that deliberate overload is not reliable magical artillery.
- Signature is aggregate. A force that transmits everywhere can be mapped, and
  what an opponent recovers is the shape of the network rather than a list of
  positions.
- No network shape is unconditionally optimal. A hierarchy is efficient and
  quiet but presents a hub worth killing; a flat force has no decapitation
  target but a contended net and a footprint that can be read.
- Topology is not configured. It follows from who carries which device, which is
  an ordinary loadout decision under logistics.
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
See [Formations and Positional Referents](formations-and-referents.md) for how
squads arrange themselves and how doctrine refers to places by role.

## Personnel progression

### Established vision

- Every individual can develop persistent attributes, skills, history, and
  consequences during a campaign.
- Development policies, inspection, and reporting can be managed or assisted
  through scripts and AI; milestone outcomes themselves resolve automatically.
- Every individual has a fixed class that establishes their core role. The
  assigned class is permanent for the campaign.
- A recruit's class is predetermined before recruitment and visible to the
  player when evaluating that recruit. Recruiting the individual does not
  trigger a class-selection decision.
- Individual personnel target approximately XCOM-like character complexity, but
  advancement management must remain practical across 50–100 units.
- Numerical attribute growth and qualitative milestone upgrades resolve
  automatically. Routine advancement does not present a per-person promotion
  choice to the player or require automation to impersonate one.
- Automatic qualitative outcomes use bounded, server-authoritative randomness
  drawn from class-compatible, history-aware eligibility pools. Randomness may
  differentiate how a person develops but cannot withhold baseline class
  competence, required communications, essential counterplay, or standard
  control-module compatibility.
- Players influence future development through persistent training,
  assignment, squad, mentoring, and company policies applied individually or in
  bulk. Policies change eligibility or weighting; they do not guarantee a
  selected perk.
- Every automatic outcome records the eligibility, weighting inputs, random
  purpose, and resulting change so that development is inspectable and
  auditable.
- Advancement is permanent once resolved and cannot be rerolled. Any later
  retraining or respec changes an existing result through its own explicit cost
  and rule rather than regenerating history until a preferred result appears.
- Advancement credit comes from authoritative participation, training, and
  significant events rather than repeatable low-value actions that a control
  module could farm.
- Qualitative perks primarily change tactical options, conditions, responses,
  preparation, and cooperation rather than stacking universal percentage
  bonuses.
- Initial perk families follow the six permanent human classes, with an
  additional leadership pool available to command-qualified personnel of any
  class. Leadership outcomes compete with other milestones rather than adding a
  second complete progression tree.
- Equipment supplies physical capabilities and remains assignable outside its
  associated specialist class. Class proficiency and perks determine how well
  and flexibly the person uses it.
- Human equipment progression is horizontal: items trade engagement shape,
  coverage, signature, power, weight, preparation, supply, and dependency
  rather than forming successively stronger tiers.

### Provisional direction

Recruitment should provide an XCOM- or Jagged Alliance-style personnel dossier.
Before committing, the player can inspect the candidate's class, current
attributes, proficiencies, learned abilities, traits, relevant history, known
injuries or conditions, and recruitment terms. The exact presentation and
number of fields remain to be designed.

This disclosure applies to the recruit's current known state, not their complete
future development. Hidden future random outcomes remain unrevealed until
resolved. Recruitment should be an informed force-building decision without
turning each candidate into a perfectly forecastable final build.

Major active abilities should be unlockable through automatic personal
progression. Progression should therefore change what an individual can do, not
merely increase numerical effectiveness or improve use of equipment.

Equipment, magic, implants, and software may still grant, enable, constrain, or
modify active capabilities. As a baseline, equipment grants the physical action
while learned abilities alter its conditions, responses, preparation, or
coordination. Exceptions and compound prerequisites remain to be designed.

Automatic attribute and qualitative development may be influenced by
experience, training, assignment, mentoring, squad role, injuries, and other
in-world causes. Persistent policies let a player express direction at useful
organizational scales without turning that influence into a guaranteed build.
Direct manual point allocation or routine perk selection is not a personnel
development interaction.

Specialized advanced classes may be introduced later as an evolution of a
unit's permanent base class. This is a provisional extension, not permission for
general class changes or unrestricted multiclassing.

### Derived implications

- Active abilities require stable machine-readable definitions available to
  canonical clients and WASM control modules.
- Personnel automation must be able to reason about newly unlocked actions,
  prerequisites, conflicts, training policies, and the behaviors that can use
  them, but it does not make routine promotion selections.
- Automatic growth rules must be authoritative, machine-readable, and protected
  against farming through repeatable WASM-controlled actions.
- Outcome generation should account for relevant facts such as class, role,
  history, existing development, training policy, assignment, and campaign
  events so that randomness produces coherent individuals rather than arbitrary
  builds.
- A class must have a stable public identifier and a machine-readable
  capability and progression contract.
- Recruitment interfaces and APIs must expose a candidate's class before the
  player commits to recruiting them.
- The canonical client and public API should expose the same authoritative
  recruitment-dossier and advancement-audit data; a third-party client must not
  gain access to hidden future progression rolls.
- Advanced-class definitions, if introduced, must declare their required base
  class and preserve an inspectable class lineage.
- Automatic random outcomes should create distinct builds within a class
  without making its core tactical role unreadable.
- The server must resolve progression authoritatively and expose enough
  information to audit eligibility, weighting, policy influence, and the
  resolved outcome without revealing future rolls.
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
- The authoritative F# gameplay kernel is shared between .NET and Fable builds.
  Given the same validated snapshot and ordered kernel inputs, they must produce
  exactly equal authoritative state, events, and hashes. An authorized browser
  replay re-simulates that kernel from a version-bound package; a
  player-perspective replay plays recorded knowledge-filtered projections and
  cannot reconstruct hidden state.
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
  → the unit's module selects the reaction on this tick boundary
  → reaction delay
  → reaction resolves
```

Because every instance is invoked on every tick, a trigger observable at a tick
boundary reaches its module on that boundary. Reaction speed is therefore
determined by the declared reaction delay of the action requested, not by when
the module next happens to run, and there is no scheduling phase that makes two
otherwise identical units react differently.

No reaction resolves in zero simulation time. Prepared states such as covering a
sector, guarding a doorway, or overwatch can reduce reaction delay, but cannot
eliminate the action lifecycle or retroactively affect an earlier tick.

An engagement also requires its targeting solution to be maintained until it
resolves. Losing observation, range, or acquisition partway through can defeat
it. See [Combat Resolution Architecture](combat-resolution.md).

#### Competing advantages resolve by precedence

When two units engage one another, their relative advantage is decided by an
ordered ladder of engagement states rather than by summing modifiers. The
highest applicable state on each side is compared; lower states do not
accumulate into the result.

The candidate ordering, strongest first, is:

```text
attending the sector the contact appears in
  > holding a prepared covered position
    > stationary and ready
      > aiming while moving
        > moving without readiness
```

This is a canonical direction, not a final table. The exact states, their
ordering, and the timing each confers remain prototype parameters.

#### Movement and readiness

Readiness and movement trade against one another continuously rather than
through a binary moving/stationary flag.

- Reaction and engagement timing degrade with a unit's **recent movement rate**,
  not merely with whether it is currently moving. A unit that has been sprinting
  is less ready than one that has been walking.
- Readiness returns **progressively as a unit approaches its destination**. A
  unit arriving and settling is readier than one still crossing open ground, so
  arrival is a meaningful moment and bounding movement behaves correctly: the
  element that has reached its position covers the element still moving.
- A unit may **maintain readiness while moving at the cost of movement speed**.
  Holding a weapon up and an attention direction fixed while advancing is
  slower than moving without that readiness.

Together these make speed and readiness a continuous doctrinal choice. Moving
fast without readiness, moving slowly while ready, and holding a prepared
position are three points on one curve rather than separate modes, and a control
module can choose among them according to the risk its commander has accepted.

Recent movement rate and distance remaining to a declared destination are both
derivable from the existing deterministic fixed-point movement credit, so
neither requires new authoritative state.

Precedence is preferred over a modifier stack because:

- a decisive outcome stays explainable. "She was watching that doorway and you
  were still moving" is a complete account of why one unit acted first;
- a comparison is cheaper than a modifier chain at 100 units per side;
- a control module can test which state it currently occupies and whether it is
  dominated, which an opaque accumulated total does not permit; and
- it directly serves the established requirement that strong causal states
  matter more than large stacks of small opaque bonuses.

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
- The grid addresses cell boundaries as well as cells. The boundary between two
  orthogonally adjacent cells is an addressable **edge**, and the lattice point
  where cells meet is an addressable **vertex**.
- Thin obstructions are semantic edge features rather than cell-filling terrain.
  Interior walls, windows, doors, fences, handrails, low walls, embrasures, and
  comparable structures occupy the boundary between two cells.
- Thick terrain remains cell-occupying. Masonry mass, rubble, machinery, and
  similar volumes fill cells. The two representations coexist; edges complement
  the cell terrain layer rather than replacing it.
- Distance uses the Chebyshev metric.
- A typical battlefield should be approximately 512×512 cells, representing
  256×256 metres. Individual scenarios may use smaller or larger maps.
- Terrain is multi-level. Elevation uses a small, bounded number of discrete
  levels rather than continuous height, and each level carries its own cell grid
  and edge layer.
- The boundary between vertically adjacent cells is a horizontal edge using the
  same semantic permeability contract as a vertical one. A floor, grating,
  hatch, or hole in a floor is an edge feature.
- Movement between levels uses declared connection features such as stairs,
  ladders, ramps, and drops, each with its own timing, cost, and interruption
  rules. Vertical adjacency alone does not permit movement.
- Battlefields are assembled from hand-authored parcels placed onto authored
  plots. Assembly is deterministic, occurs before the match, and resolves to an
  immutable content-hashed map instance.

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
- The world model has two spatial layers: cell terrain for volumes and edge
  features for thin structures. Every spatial rule must state which layer it
  reads.
- Multi-level terrain extends line of sight, pathfinding, reservation, and
  visibility caching into a third dimension at exactly the scale where their
  cost is already unmeasured. The supported level count must stay bounded and
  declared until that cost is measured.
- Deterministic map assembly means a replay reconstructs the exact map instance
  rather than re-running assembly against a parcel library that may have
  changed.

See [Tactical Environment Architecture](tactical-environment-architecture.md)
for parcel assembly, cover composition at S.I.R. cell scale, verticality,
destructibility bounds, and the map validation gate.
- A common spatial-query model should be used across movement, targeting,
  sensing, communications, and AI to prevent inconsistent interpretations of
  range and of what a boundary blocks.
- Thin terrain features need explicit rules for traversal, collision, sight,
  projectiles, cover, and destruction. The semantic edge model supplies them.
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

### Semantic cell edges

Representing a thin structure as a fully occupied cell is rejected. At 0.5
metres per cell it consumes tactical floor space the structure does not
physically occupy, shrinks every room by a cell for each wall, turns a doorway
into lost floor area, and prevents a unit from standing against a wall. More
importantly, a cell is a single occupancy fact and cannot express a feature that
blocks one interaction while permitting another. A window is a property of a
wall face, not a place a unit can stand; a handrail stops movement but not
sight; a closed door and an open door are the same masonry.

Cell edges are therefore authoritative spatial state.

#### Addressing

An edge is the boundary between two orthogonally adjacent cells and has one
canonical representative regardless of which side names it. A feature stored on
that edge applies symmetrically to both directions of crossing. Vertices—the
lattice points where up to four cells and four edges meet—are addressable
because diagonal movement and corner geometry depend on them.

#### Semantic permeability

An edge feature is not one boolean blocker. It declares its behavior separately
for each interaction that crosses it:

- movement, by footprint size and movement profile;
- line of sight and observation, which may depend on stance and height;
- projectile traces, including penetration and deflection;
- area, blast, and other effects;
- sound and other stimuli;
- cover value and the direction from which it applies; and
- interactions declared by a specific capability, such as sensor or magical
  effects.

This makes the necessary distinctions expressible:

- a **full wall** blocks movement, sight, and fire, and can be penetrated,
  damaged, or breached under its declared rules;
- a **low wall or handrail** blocks or slows movement while permitting sight and
  fire over it, and provides directional cover;
- a **window** blocks movement, permits sight and fire, and gives partial cover
  to a unit engaging through it;
- a **door** behaves according to its current state; and
- a **fence, railing, or hedge** may permit sight and fire while imposing a
  movement cost or requiring a climb or vault action.

#### Edge state

Doors, windows, shutters, hatches, and destructible structures carry
authoritative state such as open, closed, locked, barricaded, damaged,
breached, or destroyed. A state change advances the spatial revision and
invalidates only the cached queries that depend on that edge. Breaching is
therefore an explicit transition of an edge from blocking to passable, which
creates a new route and a new firing line.

#### Footprints and transitions

An `N×N` footprint crossing a boundary crosses `N` edges, not one. The movement
transition envelope enumerates every cell entered and every edge crossed. A
transition is legal only when every crossed edge permits it for that unit's
movement profile.

A diagonal transition passes the vertex shared by four cells and interacts with
the flanking edges on both sides of it. The existing prohibition on diagonal
corner cutting extends to edge features: a unit cannot pass diagonally through a
corner closed by wall edges even though both diagonal cells are unoccupied.
Whether a single flanking wall edge also blocks the diagonal, or only both
together, is a prototype parameter.

#### Derived implications

- Edge state belongs to the authoritative spatial revision. Visibility,
  pathfinding, cover, and trace caches must key on it and invalidate locally
  when it changes.
- Knowledge rules apply to edges exactly as they apply to units. An unobserved
  door state, broken window, or breached wall must not be revealed through a
  path query, reservation response, failure reason, timing difference, or cache
  event.
- Content authoring needs an edge layer distinct from the cell terrain layer,
  with stable identifiers for edge feature types and their permeability
  contracts.
- Cover evaluation reads both layers. A unit can be covered by a cell-occupying
  volume, by an edge feature, or by both from different directions.
- Control modules require machine-readable access to the locally known
  permeability of nearby edges so doctrine can reason about doors, windows,
  firing lines, and breach opportunities.
- The canonical client must render edge features and their state legibly at
  normal gameplay zoom, because a closed door and an open door are a tactically
  decisive difference occupying no floor area.

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
- respects terrain, edge features, and unit collision;
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
architecture advantage. Cells, canonical edges and vertices, square footprints,
eight possible facings, discrete terrain and edge states, and a finite set of
movement and sensor profiles produce stable query keys.

Candidate cached structures include:

- static line traces and visibility relationships between cells, including the
  ordered set of edges each trace crosses;
- facing-dependent visibility or firing sectors;
- clearance and traversability by square footprint size and movement profile,
  including edge permeability for that profile;
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
long-lived data keyed by map and ruleset version. Door and window state, edge
destruction and breaching, cell destruction, smoke, temporary magical effects,
deployable cover, and moving units require explicit spatial revision identifiers
and localized invalidation. A cached result is valid only for the geometry, edge
state, occupancy, stance, height, facing, movement profile, sensor profile, and
ruleset inputs declared by that query.

Caching is an implementation optimization rather than a change to authoritative
rules. Cached and uncached evaluation must return the same deterministic result.
Servers, canonical clients, alternative clients, and WASM-facing services may
use different internal cache strategies, but they may not derive different
visibility or movement truth from them.

Authoritative caches may contain complete world geometry, but query responses
to clients and unit-control modules remain constrained by the requester's
knowledge. A pathfinding or visibility service must not reveal an unseen door
state, unit, breached wall, broken window, magical obstruction, or other hidden
state through its result, failure reason, timing, invalidation event, or cache
metadata.

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
- Each assignment carries its own instance configuration: per-unit data supplied
  with the assignment, opaque to the server, and locked when the match begins.
  This is what lets one shared artifact serve many differently-directed units,
  and it is what the mission lifecycle calls initial policies.
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
- A player who writes no code commands by assigning each squad a **posture**: a
  named, coherent bundle of behaviour that reads as an order rather than a
  configuration. Postures are standard-module content, not engine contract.
- A posture declares what its squad does when contact with headquarters is lost.
  That choice is exposed to the player, because it cannot be communicated at the
  moment it becomes relevant.
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
  instance in the same host class receives the same per-invocation limits and
  the same guaranteed local floor.
- How richly an instance is informed, and how much expensive server analysis it
  may commission, are purchased from a finite command-bandwidth pool the player
  allocates. Every player receives the same total; they decide where it goes.
- Command bandwidth is downstream only. A commander allocates what they give a
  unit, not what a unit tells them. Reporting upward is bounded by link
  capacity, by aggregation at each hop, and by emission, not by allocation.
- Fog of war exists to make information contested, uncertain, and late. It does
  not exist to remove the player's ability to decide anything. A unit that
  receives less still fights; a commander who receives nothing has no faculty at
  all, because they have no perception of their own.
- Command bandwidth is pooled at squad level and drawn through the
  communications topology. A commander cannot spend attention on a squad they
  cannot reach.
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
- Every unit's module is invoked on every simulation tick. Measurement showed
  this costs under 2% of the tick budget at the intended force target, so the
  server does not execute behavior on a module's behalf.
- Exactly two vocabularies cross the control boundary: the events a unit is told
  about and the actions it may request. Both are versioned ruleset data.
- A module computes its own conditions from its own knowledge. The server
  publishes no condition vocabulary and evaluates no rules for a unit.
- A player who writes no module authors behavior by configuring the standard
  module, which is standard-module content rather than engine contract.
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
  advantage merely by consuming more server resources, and every player receives
  the same total command bandwidth under the same mode and force conditions.
- Two instances in the same host class receive the same computation per
  invocation and are invoked at the same rate. They can legitimately be
  differently informed, because the player has chosen to spend attention on one
  and not the other.
- Anything free will be maximised by player-authored code. If observation detail
  costs nothing, every competent module requests maximum fidelity on every tick.
  Pricing information is what makes what a unit knows a commander's decision
  rather than a default.
- Fielding more units divides the same pool further, so mass carries a real
  coordination cost that is not expressed in point value.
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
  baseline, not placeholder sample code. Because most units run it most of the
  time, its behaviour is the game's balance for most players, and a change to a
  posture is a balance change.
- The standard module is also the vehicle through which a non-programming player
  authors behavior, so its configuration surface and its interpretation of
  player commands are first-class design concerns.
- Configuration set before deployment is free. Changing it during a match is an
  order: it travels the communications topology, can be delayed or prevented,
  and produces traffic that can be seen. Preparation is therefore resilience.
- Because the standard module's configuration is content rather than engine
  contract, it can evolve without versioning the ABI.
- The event catalog determines whether a module can react competently. A missing
  event is a capability a player cannot write, however good their code.
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
- terrain volumes, edge features such as walls, windows, and doors, cover,
  smoke, and other occluders;
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

Ambient critters use this same pipeline. Attunement transports their earned
observation facts into arcane knowledge; it does not turn the critter into a
privileged sensor, reveal facts it failed to acquire, or let an AI use hidden
server state. An observation borrowed from a critter ages and becomes stale
like any other.

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
  → the unit's module requests a reaction
  → reaction delay elapses
  → reaction resolves
```

Acquisition time and reaction time are separate. A unit can notice a threat
before it has turned, aimed, changed stance, or otherwise become ready to act.

The module is invoked on the same tick boundary the observation is delivered.
See the reaction lifecycle under
[Action timing and tick resolution](#reactions).

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

The leading candidate is that the attended sector has a **hard boundary with a
step change** rather than a smooth falloff. A contact appearing inside the
attended sector receives the full attention advantage; a contact appearing just
outside it receives none, falling back to ordinary unattended performance.

A hard edge is preferred because it is readable to the player after the fact,
cheaply testable by a control module deciding where to point attention, and
makes the choice of what to watch a real commitment with a real cost. A smooth
gradient makes every attention direction partially correct and therefore makes
the decision weak.

Stance and attention transitions are not free. Changing stance, turning to a new
attention direction, or shouldering a lowered weapon consumes ticks under the
ordinary action lifecycle, and a unit caught mid-transition is at a
disadvantage. A cover posture that can be entered and left at no cost would make
exposure meaningless and crouching universally dominant. The exact transition
durations remain prototype parameters.

### Deliberate observation

Acquisition accumulates passively whenever geometry and stimulus permit, but a
unit can also **deliberately observe**: halt and commit time to building
awareness of a specified sector before proceeding.

Deliberate observation is an ordinary timed action under the canonical
lifecycle. It is not a separate perception mode and grants no information the
ordinary rules would withhold. It commits the unit to stillness and a fixed
attention direction for its duration, accelerating acquisition within that
sector at the cost of tempo and of awareness elsewhere.

Much of this is emergent — a stationary, attending unit already acquires faster
than a moving one. Making it an explicit action matters because it lets doctrine
express a deliberate intent that emergent behavior cannot: *clear this corner
before advancing through it*. Without it, cautious movement is only the absence
of hurry rather than a positive decision a module can commit to and a commander
can order.

The cost is real. A unit that stops to check is not advancing, is not covering
another sector, and is a stationary target for anything already watching it.

Exact acceleration rates, minimum useful durations, and whether checking a
sector degrades awareness of others remain prototype parameters.

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
- Breaking observation defeats an engagement that has not yet resolved, so
  exposing briefly and withdrawing is a real defensive technique.
- A unit holds one engagement at a time, but that engagement targets either a
  single unit or a declared area. Precision fire answers one attacker; support
  weapons deny a zone.
- Local numerical advantage is a legitimate answer to a prepared position held
  by precision fire, and a poor answer to one covered by a support weapon.
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

- exact forward, side, and rear sector shapes, and whether the attended sector's
  boundary is as hard as the leading candidate assumes;
- the engagement-state precedence ladder's exact membership and ordering;
- acquisition rates and thresholds;
- the decay and reacquisition curve;
- stance, turn, and weapon-posture transition durations in ticks;
- the movement-rate-to-readiness curve and how quickly readiness returns on
  approach to a destination;
- the speed penalty for maintaining readiness while moving;
- deliberate-observation acceleration rates and minimum useful durations;
- per-capability sensitivity to a lost targeting solution;
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

### Report model

Observation reporting follows fixed, authoritative game rules rather than
letting a unit's WebAssembly module decide which authoritative observations
exist. Reports are the guaranteed information floor for a player who has written
no protocol; player-defined messaging is the optimisation on top of it.

A report describes a **change, not a condition**, so traffic is bounded by the
rate of change rather than the rate of observation, and a static situation is a
quiet one. Each event class carries an authoritative significance, and a
reporting threshold set by posture decides what clears the bar.

Reports are **aggregated at every hop**. A squad leader forwards a squad-level
picture rather than a dozen individual contacts, which is the information work
the command hierarchy exists to do, and which means losing a leader removes the
thing that made a squad's information legible rather than merely severing a
link.

Reports cost link capacity and emit like any other traffic, so report volume
trades information against signature. They are not drawn from allocated command
bandwidth, which prices only what a commander gives a unit.

Reports, observations, orders, acknowledgements, friendly status, and
player-defined messages all pay the same transport latency. A direct local
squad-net transmission takes at least one tick. Every physical command-net leg,
including leader-to-leader and leader-to-headquarters, takes 20 ticks or one
second. A leader-to-relay-to-headquarters route has two command legs and
therefore takes at least two seconds one way.

The player consequently sees a stale remote command picture rather than live
server truth. Reports preserve observation time, arrival time, source,
provenance, and identity. An older observation arriving over a slower path
cannot overwrite newer knowledge of the same event or contact, while conflicting
independent observations remain separate evidence. Order receipt and execution
are known only when acknowledgements return through the delayed network.

See [Observation Reporting Model](reporting-model.md).

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
- Remote direction is bounded by cost rather than prohibited. Orders pay for net
  capacity, emission, and command bandwidth, and above all they pay latency: a
  round trip through the network is strictly longer than a local reaction, and
  no protocol compression shortens it. Global coordination is therefore possible
  but slow, and local reaction fast.
- Every command-network transmission leg costs 20 simulation ticks. The delay is
  symmetric for orders, reports, observations, acknowledgements, status, and
  player-defined messages; command traffic has no zero-delay report path.
- Portable command sets provide command-net access. Dedicated relays provide
  materially greater range and capacity, with exact ratios left to prototyping.
  Their longer links reduce the number of one-second legs compared with a chain
  of leaders.
- Arcane anchoring does not inherit human command-net timing. Anchored
  observations and status rise to the controlling caster on the next tick;
  caster commands descend to anchored subordinates after a flat 20 ticks.
  Arcane information does not accumulate relay hops.
- Restricting which messages may be sent is not attempted. Any preset vocabulary
  is a channel a player will encode arbitrary instructions into, so volume is
  charged instead of meaning.
- Control modules cannot communicate through a privileged out-of-world
  backchannel. Their messages follow the simulated communications topology:
  client to HQ, HQ to squad leaders, squad leaders to members, and the reverse
  path for reports.
- The payload protocol used across those links is player-defined rather than a
  fixed gameplay protocol.
- Headquarters connectivity depends on an operational communications device
  carried by the acting leader or another appropriate unit.
- A signal path is evaluated with the same cells, edges, and levels as line of
  sight, so terrain and elevation affect communications as they affect
  everything else.
- A device's power determines its range and its detectability together. Reach
  and signature are one property, not two that can be balanced apart.
- Command bandwidth prices networked information only. A unit's own perception
  is never gated by it and survives total communications denial.
- Module execution is never priced and never emits. A unit's control logic runs
  every tick regardless of connectivity; deliberation is free and silent.
- What is carried over the network, and therefore what emits, is information
  arriving from elsewhere: a fused picture, a commissioned analysis, an order,
  or a report going out. A heavily supported unit emits more and is easier to
  locate, so attention has a physical signature.
- Electronic warfare is an important part of play.
- The communications architecture must support electronic-warfare effects that
  cause disconnection, degrade range, delay messages or reports, locate
  transmitters, and support deception through false emissions.
- Reading or forging an opponent's message content is deliberately not modelled.
  Payloads are player-defined opaque bytes, so neither is achievable in a match;
  and a capability whose counter is cheap and universal would collapse into a
  purchase everyone makes and nothing ever fires against.
- Transmitting is an observable act. Emissions are stimuli in the ordinary
  perception pipeline, so listening is free and transmitting is not.
- Emission control is the only defence against being located, and it cannot be
  purchased. Not transmitting is the sole way to avoid being found, and it costs
  contact.
- Because command bandwidth is drawn through the communications topology,
  electronic warfare can also degrade how richly a unit is informed and what
  analysis it can commission. This attacks the quality of an opponent's control
  rather than only the delivery of their orders.
- The coupling runs both ways. Because an allocation is carried as traffic, a
  rise in a unit's traffic reveals that its commander has begun attending to it,
  which is a leading indicator of intent rather than a report of something
  already done.
- Attention is therefore cheap to spend on a unit already in contact and
  located, and expensive to spend on a concealed one.

### Derived implications

Communications form a dynamic tactical network. Its state influences:

- which observations reach the player;
- which new orders reach squads;
- whether squads can coordinate with one another;
- how quickly information becomes stale;
- whether local control modules must operate independently;
- how well informed those modules are while operating independently, since a
  disconnected squad falls back to its local floors and its last allocation; and
- how reconnaissance becomes actionable intelligence.

Electronic warfare should consequently affect gameplay at the command and
information layers, not exist only as a numeric combat modifier.

See [Communications Network Architecture](communications-network.md) for nets,
signal paths, capacity, latency, store-and-forward, devices, and relays.
See [Electronic Warfare Architecture](electronic-warfare.md) for the emission,
link, content, and bandwidth layers, the separable protections, and the
counterplay each attack requires.

Not every electronic-warfare capability must produce every effect. Individual
equipment, abilities, environmental conditions, or magical effects may use
subsets of the supported disruption model. Deception effects require explicit
attribution and knowledge-state rules so they cannot expose authoritative world
truth or become indistinguishable from server corruption.

If a leader is lost, the server promotes the first eligible person in the
squad's declared succession order, and player-provided WASM logic adapts the
squad's behavior to that change. Succession does not create a communications
device. A prepared second-in-command may already carry a redundant device;
otherwise, a unit must physically recover or loot the original device before the
squad can re-establish its equipment path to headquarters.

## Tactical combat

### Established vision

- Continuous reaction and interruption depend on attention, acquisition,
  readiness, posture, declared policy, and action timing rather than a binary
  universal overwatch mode.
- Awareness progresses through stimulus, detection, classification,
  identification, acquisition, and reporting rather than granting immediate
  target ownership.
- Conditional doctrine and standing operating procedures let modules handle
  hold-fire rules, engagement priorities, reserves, withdrawal conditions, and
  communication-loss behavior.
- The action system supports synchronized suppression and movement, breaching,
  crossfires, withdrawal, casualty recovery, and other multi-unit tactics.
- Facing, attention, stance, exposure, weapon posture, preparation, commitment,
  resolution, and recovery can all create exploitable tactical states.
- Fire lanes, penetration, friendly-fire risk, suppression, local knowledge,
  delayed reports, and casualty recovery are central sources of consequence.
- Automation performs sub-second execution; it does not choose the player's
  strategic purpose, acceptable risk, or operational priorities.
- Combat should be fast-paced and tactically focused rather than highly
  simulation-heavy.
- Positioning should have very high importance.
- Stealth, flanking, and ambushes should provide meaningful advantages.
- Awareness is strongly directional. A unit perceives most effectively in its
  forward direction, with reduced awareness toward its sides and rear.
- Player-controlled automation permits strong, decisive contextual effects.
- Attacks from behind and executions are examples of such effects.
- The tactical environment and spatial grammar follow XCOM 2: cover-dense,
  destructible, multi-level battlefields assembled from hand-authored parcels,
  in which position is the scarce resource.
- Combat resolution follows Xenonauts 2 rather than XCOM 2: physical projectile
  delivery, destructible cover, and suppression as a distinct mechanic.
- Door Kickers 2 is a narrow reference for breaching, room clearing, and
  close-quarters entry. It is no longer the primary combat reference.
- The reaction, facing, and awareness architecture is S.I.R.-owned canonical
  design and does not depend on any single external reference.
- Ranged weapons use authoritative physical shot traces rather than resolving
  only as an abstract hit roll against a selected target.
- Cover is external grid geometry—cell-occupying volumes and semantic edge
  features—that can block, mitigate, be penetrated by, or be damaged by an
  attack.
- Armor resolves after physical contact and before HP damage. It can depend on
  impact direction, coverage, damage type, penetration, and integrity.
- HP represents immediate ability to remain functional, while discrete wounds
  represent meaningful lasting consequences.
- Reaching zero HP normally causes incapacitation rather than unconditional
  immediate death.
- Suppression is an accumulating tactical state distinct from HP damage.
- A unit holds one engagement at a time. Its target is either a single unit or a
  declared area, which is what distinguishes precision fire from support weapons
  rather than rate of fire alone.
- Covering a sector, guarding a doorway, holding a lane, and suppressing a
  beaten zone are the same authoritative construct: an area engagement.
- Friendly units, civilians, and protected entities receive no immunity from
  otherwise valid traces or areas of effect.
- Executions are deliberate, timed, interruptible actions with strict
  eligibility rather than automatic rear-attack damage multipliers.

### Reference boundary

S.I.R. has no single primary reference. Its combination of continuous real time,
50–100 persistent units per side, server-hosted player-authored control, and
simulated command topology does not exist in one prior game, so different
subsystems take direction from different sources.

The environment and spatial grammar follow XCOM 2: battlefields assembled from
hand-authored parcels, dense in destructible cover, multi-level, and composed so
that lines of fire are the contested asset. XCOM 2's combat resolution is
explicitly not adopted. Its cover is a defence modifier on the target resolved
through a single to-hit roll, its units have no facing, and its concealment is a
squad-wide binary. Each of those contradicts an established S.I.R. rule.

Combat resolution follows Xenonauts 2, whose projectiles are physically
delivered and can strike intervening objects, whose cover is destructible
geometry, and whose suppression degrades effectiveness without dealing damage.

Door Kickers 2 remains the reference for breaching, room clearing, and
close-quarters entry. The awareness and reaction architecture it originally
motivated is now S.I.R.-owned canonical design and stands independently of it.

Delegated execution and local knowledge take direction from Combat Mission and
Full Spectrum Warrior, where a commander issues intent and autonomous
subordinates execute it under their own perception and morale.

In every case the reference describes desired tactical relationships, not the
control interface, resolution mathematics, spatial model, or force scale.

See [Combat, Environment, and Command Reference Models](research/combat-awareness-models.md)
for the maintained per-system reference position and adopt/reject boundaries.
See [Tactical Environment Architecture](tactical-environment-architecture.md)
for map construction, cover composition, verticality, destructibility, and map
validation.

See [Combat Resolution Architecture](combat-resolution.md) for the canonical
attack, cover, armor, HP, wound, suppression, friendly-fire, and execution
pipeline.
See
[Turn-Based Tactical Depth in Real-Time S.I.R.](research/realtime-turnbased-tactical-features.md)
for the accepted responsibility split, tactical identity package, extended
feature catalog, and design risks.

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

## Technology foundation

### Established vision

- S.I.R. is implemented in F# on .NET 10.
- It uses the FS.GG framework family, whose repositories are also available as
  sibling clones during local development.
- FS.GG.Game supplies selected render-independent game primitives,
  FS.GG.Net supplies network and serialization infrastructure, and
  FS.GG.Rendering supplies the canonical client foundation.
- S.I.R. owns its authoritative domain rules, public game protocol, knowledge
  filtering, and the adapters required where generic framework behavior differs
  from the canonical design.
- Player control modules run through direct Wasmtime .NET embedding behind an
  S.I.R.-owned adapter and restricted, versioned execution profile.
- The first public client transport is native gRPC over HTTP/2, defined by
  contract-first `.proto` schemas. Live play uses a bidirectional session
  stream; browser support may be added later through a gateway or separate
  profile.
- The public protocol separates discovery, account, catalog, artifact, match,
  live-session, and replay services. Live sessions have explicit sequencing,
  acknowledgement, reconnect, snapshot, delta, and backpressure semantics.
- The F# solution separates domain vocabulary, deterministic simulation,
  Wasmtime hosting, match orchestration, generated protocol types, validated
  protocol mapping, server composition, canonical client, and developer tools
  through an acyclic project graph.
- Local sibling checkouts do not silently determine a build. CI, releases,
  replays, and ordinary development use an explicit coherent dependency set.
- Fable compiles the shared deterministic kernel for browser experiments and
  replay. FSharp.Formatting builds the literate documentation site, and GitHub
  Pages hosts its static HTML, JavaScript, and immutable version-bound replay
  engine bundles.
- The browser rules laboratory and replay interface uses Elmish MVU from its
  first implementation. Required Fable compatibility in `FS.GG.Game` is
  developed upstream and consumed through published, pinned packages rather
  than copied source or permanent sibling references.

### Derived implications

- The generic framework's weighted diagonal path cost cannot directly define
  S.I.R.'s equal-cost Chebyshev movement.
- Generic sequential or splittable randomness cannot replace S.I.R.'s
  counter-addressed authoritative samples.
- Rendering, audio, wall-clock accumulation, sockets, and persistence remain
  outside the deterministic match kernel.
- A reusable capability missing from FS.GG should be proposed through its
  owning repository and cross-repository compatibility process rather than
  added through an untracked sibling dependency.

See [Technology Stack and FS.GG Integration](technology-stack.md) for the
canonical component boundaries, dependency policy, adapter inventory, and
validation gates.

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

The following invariants guide subsequent design. The first eleven are implied
by the vision. The remainder are design laws learned from applying it, each of
which has caught a concrete failure in a system that had already been written.

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
10. Spatial rules must consistently account for multi-cell square footprints,
    Chebyshev distance, and the semantic permeability of the cell edges a
    movement, trace, or sensing path crosses.
11. Automation must be used to expand tactical depth and consequence, not to
    remove meaningful tactical agency from the player.
12. **Anything free will be maximised.** Player-authored code takes every
    capability that costs nothing, every time. A capability that should be a
    decision must therefore be priced, and a merely rate-limited one becomes a
    limit every competent player pins permanently.
13. **A single dominant configuration is a tax, not a decision.** Every
    configurable structure must present a trade with no general winner. If one
    answer is unconditionally correct, players will find it, all of them will
    then use it, and the structure will have cost effort without producing a
    choice.
14. **Restrictions on the meaning of player-defined data are unenforceable.**
    Any permitted vocabulary is a channel, since N options carry log2(N) bits
    and players will encode whatever they need into it. Price measurable volume
    instead of interpreting intent.
15. **Prefer bounds that cannot be purchased around.** A limit a player can buy
    off once before a match stops being a limit. Bounds rooted in physics or
    topology — that transmitting is observable, that a round trip is longer than
    a local decision, that a shared medium is contended — hold regardless of
    preparation.

Invariants 12 to 15 were each established by a failure they exposed: free
invocation frequency, a costless flat network, restricted message vocabularies,
and remote direction routed through a hierarchy. They should be applied to any
new configurable surface before it is written rather than after.

## Open questions

These questions are recorded for later development and do not block the current
vision:

1. What precise authority does a unit's WebAssembly policy have over existing
   orders after communication is lost?
2. What is the command-bandwidth unit, how large is the pool, how is it derived,
   and what exchange rate governs observation richness against host-service
   quota?
3. What significance does each event class carry, what aggregation windows
   apply, and is aggregation performed by the unit, the leader, or both? The
   reporting model itself is settled; these are its values.
4. What relay capacity, chaining limits, and setup times make a relay chain
   worth placing? Relaying itself is established: a relay is an authoritative
   object with a position that can be found, jammed, and destroyed.
5. What exact directional-awareness zones, rear-attack effects, and unawareness
   thresholds produce surprise and execution opportunities?
6. Which training, assignment, mentoring, and company-policy inputs influence
   automatic progression, and how strongly may they change eligibility or
   weighting without making outcomes deterministic?
7. What monster kills qualify for System recognition, how many are required,
   and how is credit assigned among cooperating units?
8. What are the exact spell aspects, casting checks, strain recovery rules,
   breach table, and shattering outcomes of the initial arcane civilization?
9. What limits, costs, and recovery times apply to provisional human
   nanomedical technology?
10. After players discover one another in a major mission, what determines
    hostility, cooperation, identification, communication, victory, and
    write-back consequences?
11. What does a portal-access bid commit—currency, resources, reputation,
    forces, risk, reward share, or another scarce value—and how are winning bids
    selected? A candidate answer unifying the bid with a mission stake is
    proposed in [Stakes and Reinforcement](stakes-and-reinforcement.md).
12. Who licenses mercenary companies, controls portal access, and administers
    mission bidding? A candidate motive is proposed in
    [Stakes and Reinforcement](stakes-and-reinforcement.md): the administrator
    takes a share of every contested pot, which is both why the institution
    tolerates the fighting and what stops escalation being free.

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
