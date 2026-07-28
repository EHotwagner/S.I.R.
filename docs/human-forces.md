---
title: S.I.R. Human Forces
category: Reference
categoryindex: 5
index: 17
status: accepted
decision-status: canonical
document-type: living-content
version: "1.3"
last-updated: 2026-07-27
related:
  - docs/setting-and-factions.md
  - docs/combat-resolution.md
  - docs/communications-network.md
  - docs/electronic-warfare.md
  - docs/casualty-and-medical-architecture.md
---

# S.I.R. Human Forces

## Purpose

The first content document. Twenty-two architecture documents describe how
systems behave and none of them says what is in the game.

This defines the player's own force: what a squad contains, what its people are,
what they carry, and what each of those choices costs. Everything else is
measured against it — the arcane civilization exists as an asymmetric answer to
it, missions are built for it, and the point catalog will price it.

## How to read this

**This is content, not architecture.** It is versioned ruleset data that extends
the capability descriptor set; it does not change any contract. Numbers here are
indicative shape rather than balance, and every one of them is a prototype
parameter until something is running.

What is *not* provisional is the shape: which roles exist, what distinguishes
them, and what stops each from dominating. Those are design decisions and they
are the point of the document.

### What canonical status covers

Settled: that classes are people and loadout is capability, so there are fewer
classes than roles; the six classes and the system each answers for; command
qualification as an attribute rather than a seventh class; weapons
differentiating by the shape of their engagement-time curve and by point versus
area rather than by damage; armour being directional, so that it makes
positioning matter more rather than less; and human capability being information
capability, which emits and drains. Humans counter arcane use of ambient
critters through a choice among detection, exclusion, capture, removal, and
deception rather than receiving automatic knowledge or being required to
eradicate all ambient life. Individual personnel have approximately XCOM-like
development complexity, but numerical growth and qualitative milestones resolve
automatically through bounded, class-compatible, history-weighted randomness.
Players influence development through persistent bulk policies rather than
routine promotion choices. Perks change tactical options, conditions, and
responses rather than primarily stacking percentage bonuses. Equipment grants
the physical capability; proficiency and perks change how effectively and
flexibly a person can employ it. Human equipment progresses horizontally
through different costs, signatures, engagement shapes, and dependencies rather
than through successively stronger item tiers.

Not settled: every number, and the open parameters at the end.

**What would force a revision of the shape**, as opposed to the values: weapons
collapsing into one good option and several worse ones despite distinct curves;
directional armour making frontal assault unviable rather than making flanking
valuable; or loadout failing to be a decision because carrying capacity,
battery, and signature do not bite. Each is a measurable outcome rather than a
matter of taste, and each would mean the shape was wrong rather than the
numbers.

## The squad

A standard infantry squad is **eight to ten persons**, organised as a squad
leader plus two teams. It requires filled squad leader, second-in-command, and
third-in-command assignments to deploy, with the latter two commanding the
teams.

At the intended force target this puts a side at roughly **six to twelve
squads**, which is the number a commander can hold in mind and assign postures
to without the interface becoming a spreadsheet.

## Classes are people; loadout is capability

A class is permanent for the campaign and establishes proficiency, progression,
and what a person is *good at*. Equipment is reversible and establishes what
they can currently *do*.

**This means there are fewer classes than roles.** A rifleman carrying a support
weapon is performing the gunner's role and doing it worse than a gunner would.
The class does not forbid the loadout; it prices it in effectiveness.

| Class | Core competence |
|---|---|
| **Rifleman** | The baseline. Broad competence, no specialisation, the unit every comparison is measured against. |
| **Gunner** | Sustained area fire. Proficiency with support weapons and the fire discipline they require. |
| **Marksman** | Precision at range. Proficiency with slow-building, range-indifferent weapons and with observation. |
| **Engineer** | Breaching, demolition, deployables, and prepared positions. |
| **Medic** | Casualty work. Faster and more reliable aid and stabilisation, and procedures others cannot perform. |
| **Signaller** | Communications, electronic warfare, and drones. Relays, direction finding, jamming, the command-net set, and the aerial platforms that carry all of it. |

Six classes, deliberately. Each maps to a system that would otherwise have no
one who is good at it, and none is a variation on another.

Command qualification is an attribute of a person rather than a seventh class.
Any class may hold a leadership assignment if qualified, which keeps succession
from being blocked by the death of the only person of the right type.

### Individual depth without promotion administration

Each person can accumulate attributes, proficiencies, abilities, traits,
history, injuries, and a small number of meaningful qualitative milestones.
The target is roughly the amount of identity and tactical differentiation an
XCOM soldier carries, applied to a substantially larger roster.

Advancement is automatic:

```text
authoritative participation, training, and significant events
        ↓
automatic bounded attribute growth
        ↓
automatic class-compatible qualitative outcome
        ↓
inspectable development report
```

The player is not asked to resolve routine promotions. Persistent development
policies instead influence eligibility or weighting through training focus,
assignment, squad, mentoring, and company direction. Policies can be applied in
bulk and never guarantee a specific perk.

Random development cannot remove or gate the competence promised by a person's
class. It differentiates people within that role. Every resolved change is
permanent, server-authoritative, protected against action farming, and recorded
with enough eligibility and weighting information to explain why it occurred.

## Perks change decisions

A perk should introduce a new condition, response, preparation, or
coordination option. It should not simply make a veteran universally better at
the same action. Four to six milestones over a career are enough to combine
several such changes into a memorable individual without making their state
unreadable.

The following are the initial class-compatible perk families. Their named
effects define intended tactical identities; exact timing, costs, eligibility,
and interaction values remain prototype parameters.

### Rifleman

- **Point Man** improves the constrained first response to a threat encountered
  while advancing, especially at short range.
- **Bounding Partner** restores readiness more effectively after movement made
  under confirmed friendly covering fire.
- **Quiet Advance** provides a slower movement mode with reduced visual and
  acoustic signature.
- **Cross-Trained** reduces, but does not remove, the proficiency penalty for
  carrying specialist equipment outside the unit's class.
- **Local Initiative** supports more capable execution of the last received
  intent while communications are unavailable.
- **Rear Guard** supports observation and readiness while the squad disengages.

### Gunner

- **Traverse Discipline** redirects an established area engagement into an
  adjacent sector while preserving more preparation.
- **Beaten Zone** selects between narrower, deeper and broader, shallower area
  engagement shapes.
- **Walking Fire** shifts suppression along a declared path to support
  movement.
- **Fire Control** avoids spending ammunition on unsuitable or already
  sufficiently suppressed parts of an area.
- **Final Protective Fire** prepares an ammunition-expensive reaction against
  enemies crossing a declared close defensive line.
- **Crew Drill** gains additional handling and sustainment benefit from a
  cooperating assistant carrying ammunition or servicing the weapon.

### Marksman

- **Patient Solution** preserves limited targeting progress through a very
  brief obstruction, but not through meaningful target relocation.
- **Spotter Pair** uses observations explicitly relayed by a nearby spotter to
  establish an initial firing solution.
- **Counter-Observer** improves recognition of optics, silhouettes, attuned
  observers, and other evidence of surveillance.
- **Target Discrimination** improves identification of observable equipment
  and behavior before the marksman commits to a shot.
- **Cold Position** reduces the movement evidence produced while observing from
  a prepared position.
- **Displacement Drill** leaves a firing position more efficiently after
  firing, at the cost of abandoning the current solution.

### Engineer

- **Hasty Breach** prepares a faster but louder and less controlled entry.
- **Surgical Breach** spends additional time and tools to constrain collateral
  damage and unwanted openings.
- **Remote Initiation** connects a prepared charge to a physical remote trigger.
- **Field Fortification** places cover and obstacles more efficiently where
  terrain permits.
- **Trap Sense** improves recognition of disturbed terrain, prepared ritual
  sites, mines, and crude hazards.
- **Render Safe** dismantles eligible human deployables and discovered ritual
  traps rather than merely destroying them.

### Medic

- **Triage** assesses several casualties quickly and exposes their urgency and
  likely treatment requirements.
- **Under Fire** permits limited stabilization while exposed, with reduced
  reliability or greater supply consumption.
- **Damage Control** treats a defined class of complication beyond ordinary
  first aid.
- **Conservative Medicine** uses fewer supplies when adequate time and safety
  are available.
- **Casualty Movement** coordinates carrying or dragging with less disruption
  to the assisting unit.
- **Return to Duty** improves limited in-mission recovery after successful
  stabilization without removing lasting wounds.

### Signaller

- **Burst Discipline** compresses queued reports into shorter transmissions,
  trading immediacy for reduced direction-finding exposure.
- **Frequency Agility** reconfigures communications more efficiently after
  interference is recognized; it does not create immunity to jamming.
- **Cross-Cueing** correlates acoustic, thermal, radio, and magical-detector
  observations into a better contact estimate without revealing hidden truth.
- **False Traffic** makes a decoy emitter reproduce plausible network behavior.
- **Drone Shepherd** supplies drones with better contingency instructions
  before communications are lost.
- **Relay Architect** improves prediction of coverage and weak links while
  placing network equipment.
- **Borrowed-Eye Hunter** improves recognition of evidence left by active
  critter attunement without identifying every ordinary animal as hostile.

### Leadership

Command-qualified personnel of any class may receive leadership outcomes.
Leadership outcomes compete with other milestones rather than forming a second
full progression tree.

- **Clear Intent** supplies subordinates with a richer fallback plan before
  disconnection.
- **Fire Coordinator** establishes a squad-level confirmed-target or
  covering-fire instruction.
- **Controlled Succession** reduces disruption when command passes to the next
  qualified person.
- **Emission Discipline** establishes a squad transmission posture such as
  silent, scheduled, emergency-only, or continuous.
- **Steady Withdrawal** preserves formation and reporting discipline during
  disengagement.

Leadership perks affect orders, information, reactions, and contingency
behavior. They are not abstract squad-wide statistical auras and do not create
connectivity without physical communications equipment.

## Weapons

Weapons differentiate primarily by the **shape of their engagement-time curve**
and by whether they hold a point or an area, not by damage.

| Weapon | Curve | Target | Role |
|---|---|---|---|
| **Carbine** | short base, moderate rise | point | Close and mid work; the assault default |
| **Rifle** | moderate base, shallow rise | point | The general-purpose baseline |
| **Shotgun** | very short base, steep decay | point | Interior and doorway dominance; useless across ground |
| **Marksman rifle** | long base, nearly flat | point | Punishes exposure at any range; slow to bring to bear |
| **Support weapon** | — | **area** | Denies ground, suppresses, covers movement |
| **Grenade launcher** | — | area, slow projectile | Reaches over and behind cover |
| **Anti-armour launcher** | long base | point, slow projectile | Vehicles, hardened positions, and heavy creatures |

Slow projectiles occupy authoritative state across ticks and use swept paths, so
a launcher's round is a visible, dodgeable, interruptible object rather than an
instant result.

### Why none of these dominates

- the **marksman rifle** is the strongest weapon at range and the worst against
  anything that peeks, because a long build time gives a target every chance to
  break the solution under the sustained-targeting rule;
- the **shotgun** is close to unbeatable inside a building and worthless outside
  one, which makes it a decision about where you expect to fight;
- the **support weapon** denies ground better than anything else and kills any
  individual worse, because volume spread over a zone is less lethal than a
  maintained solution; and
- the **carbine and rifle** trade against each other across the ordinary
  engagement band rather than one being an upgrade.

## Armour

Human armour is **strongly directional**: hard plates front and rear, soft
coverage at the sides and limbs.

| Armour | Coverage | Cost |
|---|---|---|
| **None or soft** | Fragmentation only | Nothing; the scout's choice |
| **Carrier** | Front and rear plates, soft flanks | Moderate weight and carrying capacity |
| **Heavy** | Plates plus limb and neck protection | Substantial weight, capacity, and readiness |

Directionality is the point. Frontal protection is genuinely good, and it does
nothing at all against a flank or rear engagement. **Armour therefore makes
positioning more important rather than less**, which is the opposite of what
armour usually does in a tactical game.

Weight is not free: it competes with ammunition and equipment for carrying
capacity under the logistics model, and it degrades the movement-to-readiness
relationship, so an armoured unit is slower to become ready after moving. Heavy
armour is a commitment to being shot at from the front.

## Equipment is where human identity lives

The equipment list is longer than the weapon list, and most of it is not lethal:

| Category | Items |
|---|---|
| **Weapon packages** | Suppressor; compact optic; magnified optic; thermal sight; bipod or support mount; under-barrel launcher; specialist ammunition |
| **Communications** | Personal set; command-net set; directional antenna; burst-transmission unit; deployable relay; physical data package |
| **Sensors** | Compact optics; magnified observation optic; thermal imager; acoustic direction finder; magical-signature detector; trip sensor |
| **Electronic warfare** | Configurable jammer; radio direction finder; decoy emitter |
| **Drones** | Observation drone; relay drone |
| **Medical** | Individual aid kit; stabilisation kit; nanomedical stock; diagnostic sensor; casualty harness or folding litter |
| **Engineering** | Breaching charge; cutting tool; demolition charge; remote initiator and wire; deployable cover; lightweight obstacle; mine or directional defensive charge; sensor stake; critter trap, cage, repellent, and exclusion equipment; ritual-site disruption tools |
| **Protection** | Soft armour; plate carrier; heavy directional armour; helmet and optional sensor mount |
| **Sustainment** | Ammunition by class; batteries; drone parts; engineering consumables; medical stock; relay components |

Notice what that list mostly is. **Human capability is information capability**,
and the setting document's claim that humans are "disciplined, information-rich,
logistics-dependent combined arms" is not an assertion to be honoured elsewhere
— it is this table.

Equipment grants capabilities; it does not belong exclusively to a class. Class
proficiency, progression, weight, preparation, and supply determine whether
assigning an item to a particular person is sensible. A rifleman can carry a
jammer or stabilization kit, but will not exploit it as well as the relevant
specialist.

Equipment development is horizontal. A new item should change engagement curve,
coverage, signature, power use, carrying burden, preparation, or dependency.
Successively numbered weapons that are simply more damaging, and electronics
that remove emission or jamming counterplay, do not fit the human force.

### Drones are elevation you can move

Drones belong to the signaller because they are information capability with
rotors. Both of their uses — carrying a sensor and carrying a relay — are the
signaller's existing job performed from somewhere a person cannot stand.

A drone is a **unit, not a piece of equipment.** It has a footprint, a position,
its own perception, and its own control module instance, because the
architecture admits no other way for something to act. Several consequences
follow without needing rules of their own:

- it **counts against force size**, so fielding drones divides the same command
  bandwidth pool across more units and dilutes attention exactly as any other
  addition does;
- it **occupies height**, which the spatial model already makes valuable twice
  over — clear observation and clear signal paths. A drone is the one asset that
  can put both wherever they are needed;
- it is **the most network-dependent thing a human force fields**, since a relay
  drone transmits continuously and is therefore a beacon, while an observation
  drone is only useful if what it sees reaches somebody; and
- when jammed it does not fall out of the sky. Its module runs every tick like
  any other unit's, so it **becomes autonomous and unsupported** — still flying,
  still seeing, and no longer telling anyone.

That last property is the sharpest expression of the faction's central tension.
The drone is the clearest thing humans own, and it is the thing an opponent has
most reason to cut off rather than shoot down.

### Countering borrowed eyes

Harmless ambient critters can become arcane observers without becoming arcane
units. A human force cannot assume that every rat, bird, or insect is hostile,
but it also cannot assume that an animal inside a sensitive position is
irrelevant.

Humans may kill suspected critters, spending time or ammunition and potentially
revealing the position they intended to conceal. They can also trap or capture
them, drive them away, seal likely routes, use magical-signature detection to
find evidence of active attunement, or deliberately let one observe a false
deployment.

The correct response depends on what the position is worth and what evidence
exists. Eradicating every ambient creature must not be the automatic opening;
it is one costly form of information denial among several.

### Strength and vulnerability are the same property

The through-line worth stating explicitly:

```text
human capability is information capability
        ↓
information capability is electronic
        ↓
electronic capability emits, and consumes power
```

Every advantage humans have — the fused picture, direction finding, relayed
command, thermal observation, drones, coordinated fire — is carried by a device
that
**announces its position** and **runs out of battery**. The faction's strength
and its vulnerability are not two lists that must be balanced against each
other. They are one property seen from two sides.

That is why a human force is formidable while its network holds and awkward when
it does not, and it is what an opponent designed against them should attack.

## Checking against invariant 13

No category should have an unconditionally correct answer.

**Weapons** trade across range and against the sustained-targeting rule, so the
best weapon depends on where you expect to fight and whether the enemy will
stand still.

**Armour** trades protection against carrying capacity and readiness, and its
protection is directional, so it is a bet on being engaged from the front.

**Equipment** trades capability against weight, battery, and signature. A
maximally equipped squad is heavy, slow, and loud.

**Classes** trade specialisation against squad flexibility, since a squad of six
specialists has no depth when one dies and a squad of riflemen is good at
nothing in particular.

The category most at risk is **equipment**, because carrying more is locally
always better and the costs are diffuse. Carrying capacity, battery consumption,
and aggregate signature all need to bite hard enough to make loadout a real
decision, and that is the first thing to check once anything runs.

## What this tests in the architecture

Writing content is also how architecture gets validated, and this set is
deliberately chosen to exercise claims that are currently only asserted:

- whether **engagement-time curves** produce genuinely distinct weapon roles, or
  collapse into one good weapon and several worse ones;
- whether **area engagement** makes a support weapon feel different in kind
  rather than merely faster;
- whether **directional armour** makes flanking matter more, as intended, or
  simply makes frontal assaults unviable;
- whether the **two-layer cover model** produces readable outcomes when a real
  weapon meets a real wall;
- whether **emission and battery costs** are enough to stop every squad carrying
  every device; and
- whether **attuned critters** produce enough evidence and enough useful
  counterplay that humans make local security decisions rather than clearing
  every map of ambient life.

## Open parameters

- Every numeric value: engagement times, curve shapes, armour values,
  penetration, weights, capacities, battery life.
- Squad size, and whether specialists are organic to a squad or attached.
- Whether the six classes are the right six, and whether advanced classes
  branch from them.
- Attribute and proficiency catalogs, growth bounds, milestone count and
  cadence, class-compatible outcome pools, history tags, weighting formulas,
  and the exact influence allowed to development policies.
- Exact perk effects, prerequisites, exclusions, history tags, policy weights,
  activation rules, resource costs, timings, and whether every named perk
  survives prototyping.
- Whether retraining or respec exists, what it costs, and which existing
  outcomes it may alter without rerolling historical advancement.
- Equipment weights, slots or packing rules, supply prices, power consumption,
  signatures, compatibility, and which listed items belong in the initial
  playable catalog.
- Ammunition package sizes and compatibility across weapon classes.
- Battery as a single resource or per-device-class stocks.
- Which sensors reveal which observation facts.
- Which tools detect, exclude, capture, repel, or quietly kill attuned and
  unattuned critters, and which of those belong in initial human content.
- Vehicle content, deliberately excluded here.
- Drone endurance, altitude bands, control range, and whether an observation
  drone can operate usefully while silent.
- Whether a drone occupies a declared altitude band or the topmost level.
- Nanomedical limits, which remain provisional in the setting document.
