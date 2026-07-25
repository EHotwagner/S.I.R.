---
title: Attribute, Skill, and Progression Systems
status: proposed
document-type: research
version: "0.19"
last-updated: 2026-07-25
related:
  - docs/game-vision.md
research-scope:
  - XCOM
  - adjacent tactical games
  - roguelike progression
  - large passive webs
---

# Attribute, Skill, and Progression Systems

## Research question

How should S.I.R. structure attributes, skills, and progression at roughly the
same conceptual complexity as XCOM while supporting:

- 50–100 units per side;
- real-time fixed-step play;
- server-authoritative simulation;
- player-supplied WebAssembly control modules;
- squad and communication hierarchies;
- decisive positioning, stealth, logistics, and electronic warfare; and
- multiplayer through a public API and alternative clients?

This is comparative research, not an accepted design. It identifies viable
models and recommends a direction to prototype.

## Executive conclusion

Every S.I.R. unit is a persistent individual. The progression architecture does
not need to flatten rank-and-file development merely because a side contains
50–100 units: scripts and AI can manage routine personnel information and apply
player-defined development policies.

The best fit is therefore a **layered individual-and-command progression
system**:

1. Every unit has a readable set of attributes, proficiencies, history, and
   persistent conditions.
2. Personnel are recruited individually and then organized into squads.
3. Recruit candidates are procedurally generated. Once recruited, each is a
   stable persistent individual rather than a replaceable generated template.
4. Every unit has a fixed class that establishes a stable core role and remains
   permanent for the campaign. The class is predetermined and visible before
   the player recruits the individual.
5. Every unit can receive XCOM-scale qualitative progression, provisionally
   four to six meaningful choices over a campaign.
6. Scripts, AI, templates, and batch policies can propose or apply routine
   development while the human reviews exceptions and important individuals.
7. Squad leaders use the same personnel foundation but have access to
   leadership, communications, coordination, and logistics specializations.
8. Squads have a configurable doctrine layer that affects control behavior and
   tactical capabilities.
9. Equipment, magic, implants, and support assets provide reversible active
   capabilities.
10. Research and organizational progression unlock options rather than applying
   large universal stat increases.

The current leading direction is that personal progression can directly unlock
major active abilities. Equipment and magic remain additional capability
sources, prerequisites, or modifiers rather than the exclusive source of active
actions.

This progression model should operate under mode-specific policies. S.I.R.'s
main mode retains account-owned personnel and their progression across matches
within a fixed-duration campaign. Campaign state is wiped when the campaign
ends. Dedicated PvP duels and skirmishes should use bounded point-based force
construction. PvE and cooperative play can use persistent or scenario-defined
rosters without forcing competitive skirmishes to inherit persistent power
differences.

This retains XCOM-like depth for persistent individuals while moving the
repetitive interaction cost into inspectable automation. The player can define
force-wide policies, specialize selected personnel manually, and understand why
an automated decision was made.

The 2026 game *MENACE* offers a useful contrasting pattern: it organizes
infantry into squads of 3–9 and concentrates promotion trees on squad leaders.
S.I.R. does not adopt that abstraction because every member is persistent, but
MENACE's squad equipment and supply-cost model remains relevant for preserving
loadout choices instead of allowing every unit to use the best item
([official description](https://store.steampowered.com/app/2432860/MENACE/),
[supply-system developer diary](https://steamcommunity.com/ogg/2432860/announcements/detail/535466136352850185)).

## What “XCOM complexity” actually means

The useful part of XCOM's complexity is not the raw node count. Its strength
comes from a particular division of responsibility:

- a class establishes an immediate tactical identity;
- rank provides a clear progression cadence;
- each promotion asks for one understandable decision;
- most attribute growth happens automatically;
- equipment adds a reversible customization layer;
- injuries, death, fatigue, and bonds make soldiers consequential; and
- strategic research expands the available build space.

In base *XCOM 2*, a class defines a battlefield role and divides its abilities
between two specializations. Abilities unlock with rank, while loadouts and
weapon upgrades are managed separately
([XCOM 2 manual](https://www.feralinteractive.com/en/manuals/xcom2/latest/steam/)).
*War of the Chosen* adds ability points, cross-class abilities, bonds, negative
traits, and exhaustion. Firaxis explicitly described the original system as a
binary promotion choice and the expansion as a way to make tactical actions
such as flanking and taking high ground feed a more flexible ability economy
([official War of the Chosen article](https://xcom.com/news/xcom-2-war-of-the-chosen-soldier-bonds-and-new-skills/amp/)).

This results in substantial tactical differentiation without requiring the
player to allocate every attribute point. That balance—not the exact XCOM class
tree—is the appropriate complexity target.

## Comparative models

| Model | Representative games | Main decision | Strength | Main risk for S.I.R. |
|---|---|---|---|---|
| Binary class promotions | XCOM, XCOM 2 | One of two abilities per rank | Clear, paced, legible | Too many decisions across 100 units |
| Point-buy class trees | War of the Chosen, Gears Tactics, Phoenix Point | Spend points among unlocked nodes | Flexible builds and hybrids | Optimization burden and dominant combinations |
| Classless perks plus attributes | Battle Brothers, Jagged Alliance 3 | Improve stats and select open perks | Emergent roles and individuality | Spreadsheet play, thresholds, roster micromanagement |
| Bounded random draft | Wildermyth, many roguelikes | Choose one of several random offers | Adaptation and replayability | Build unreliability and multiplayer variance |
| Ability loadout or deck | Midnight Suns | Equip a subset from a larger pool | Reversible specialization | Random availability can weaken tactical predictability |
| Large passive web | Path of Exile | Path through a shared graph | Theorycrafting and exotic builds | Grossly excessive per individual unit |
| Leader and squad progression | MENACE | Develop leaders; equip whole squads | Scales to larger formations | Rank-and-file identity can become too thin |

## Detailed findings

### XCOM: constrained choices with strong role anchors

#### Structure

- Soldiers have a class and rank.
- Classes establish role, equipment access, and ability choices.
- Promotions add abilities and automatic stat growth.
- XCOM 2 divides each class into two recognizable specializations.
- War of the Chosen adds individual and shared ability points, random
  cross-class potential, bonds, exhaustion, and negative traits.

#### What works

- The player understands a soldier's purpose before reading every perk.
- A single decision at a promotion makes advancement feel important without
  opening an enormous planning screen.
- Binary choices create opportunity cost and recognizable builds.
- Equipment is separate from permanent advancement, allowing mission-specific
  adaptation.
- Bonds and injuries create consequences orthogonal to raw power.
- War of the Chosen rewards tactically desirable actions with ability points,
  connecting play quality to progression.

#### Limitations

- A two-choice row can contain a false choice if one option is generally
  superior.
- Fixed classes can make soldiers within a class converge.
- Kill-based experience encourages feeding kills to particular soldiers.
- Extra ability points can erode specialization if late-game soldiers purchase
  most desirable abilities.
- Repeating the promotion interaction for 50–100 active units would create
  administrative overload.

#### S.I.R. lesson

Use XCOM's **decision cadence and role clarity**, not necessarily its
one-tree-per-unit implementation. Progression rewards should not be granted for
repeatable micro-actions that a WASM controller can farm. Mission contribution,
objectives, survival, training, commendations, or capped tactical achievements
are safer inputs.

### Phoenix Point: shared currency, multiclassing, and stat competition

Phoenix Point uses class skill trees, training, research-gated knowledge, and
multiclassing. Progression points can also improve base statistics such as speed
and strength
([official feature overview](https://phoenixpoint.info/game),
[official development article](https://phoenixpoint.info/blog/2019/3/26/backer-build-four)).

#### What works

- A common currency lets the player compare a new ability against raw physical
  improvement.
- Multiclassing produces strong tactical combinations.
- Faction research expands the possible class and equipment space.
- Personal and organization-wide progression resources can help lagging or
  newly recruited soldiers catch up.

#### Limitations

- When stats, perks, and multiclass access use one currency, optimal conversions
  may dominate expressive choices.
- Cross-class combinations dramatically increase balance complexity.
- Flexible point-buy systems can let mature soldiers acquire too much of both
  classes, weakening identity.
- Shared points invite concentration into a few favored units.

#### S.I.R. lesson

If S.I.R. uses shared progression resources, they should probably purchase
**training opportunities, certifications, or doctrine access**, not be freely
convertible into every attribute and perk. Cross-role access should have an
explicit cost such as a doctrine slot, equipment dependency, or mutually
exclusive specialization.

### Gears Tactics: readable trees with deeper subclasses

Gears Tactics has six classes, each with four named subclass directions. Players
earn skill points and tailor soldiers through these trees
([official class guide](https://www.gearsofwar.com/en-us/game-guide/classes/)).
The game also provides limited respec opportunities during the campaign and free
respec in its veteran mode
([Xbox developer interview](https://news.xbox.com/en-us/podcast/721-gears-tactics-on-xbox-series-xs/)).

#### What works

- Named branches communicate intended builds.
- A spatial tree makes prerequisites and deeper investment visible.
- Limited points preserve opportunity cost.
- Respec allows experimentation without making every campaign decision
  meaningless.

#### Limitations

- A tree with more than 30 skills per class is a large planning surface.
- Four branches per class create more content and balance work than XCOM.
- Deep trees encourage external build planning and can punish early ignorance.
- This scale is suitable for a small squad, not 100 separately managed units.

#### S.I.R. lesson

Named branches are useful for squad leaders or doctrine packages. If used,
trees should be shallow enough that every node changes behavior rather than
serving as a small numerical travel tax.

### Battle Brothers: emergent roles from random potential

Battle Brothers is classless. Random starting stats and talents create
individual potential; level-ups improve several attributes and award perks.
The developers' stated goals are particularly relevant: every stat should
matter, perks should support a strategy without invalidating core mechanics, and
good perks should require skill to exploit
([character-stat design](https://battlebrothersgame.com/character-stats/),
[perk-system design](https://battlebrothersgame.com/dev-blog-80-progress-update-new-perk-system/)).

#### What works

- Recruits suggest roles through strengths rather than arriving as empty shells.
- Random potential makes campaigns and personnel markets replayable.
- Classless perk rows allow unusual builds.
- Perks specialize characters while equipment completes the role.
- Numerical growth and qualitative perks are separate decisions.

#### Limitations

- Random rolls can make a recruit's long-term value difficult to assess without
  tools.
- Repeating several stat choices and a perk choice on every level is
  management-heavy.
- Open perk pools tend to generate solved builds and benchmark-driven hiring.
- Bad random potential can make a character feel invalid before they have acted.

#### S.I.R. lesson

Random starting variation is valuable, but it should be **bounded and
legible**. The player or control module should be able to understand a unit's
aptitude without consulting hidden growth tables. Ordinary unit development
should be template-driven or automated at scale.

### Jagged Alliance 3: classless identity with attribute gates

Jagged Alliance 3 explicitly uses a classless system in which mercenaries can be
developed in different directions and specialized with perks
([official game site](https://jaggedalliance3.thqnordic.com/jaggedalliance3/US/win)).
Its perks are grouped by core attributes and gated by attribute thresholds,
while skills can also improve through relevant activity and training
([attribute reference](https://jaggedalliance.fandom.com/wiki/Attributes_in_Jagged_Alliance_3)).

#### What works

- Existing characters arrive with meaningful strengths, weaknesses, traits, and
  personality.
- Attribute thresholds make stats unlock qualitative possibilities rather than
  only adding percentages.
- Classless perks allow a mercenary's role to emerge from their complete
  profile.
- Learn-by-doing makes experience feel grounded in actual service.

#### Limitations

- Hard thresholds encourage exact breakpoint optimization.
- Learn-by-doing can reward repetitive or artificial behavior.
- Ten attributes plus perks, talents, equipment, and personality are more
  complex than XCOM.
- A large roster magnifies training and inspection overhead.

#### S.I.R. lesson

Requirements can be useful, but prefer **broad eligibility bands** or tags over
precise breakpoints. Learn-by-doing should use capped mission-level evidence,
not count unlimited individual actions that automated controllers can repeat.

### Wildermyth and roguelikes: bounded random drafting

Wildermyth gives a promoted hero a selection from random abilities. Its current
rules typically offer several choices with guarantees: a class option and, when
applicable, an upgrade to something the hero already knows
([official overview](https://www.wildermyth.com/wiki/Main_Page),
[official ability reference](https://www.wildermyth.com/wiki/Abilities)).

#### What works

- Players adapt rather than executing the same precomputed build every campaign.
- Bounded offers create distinct individuals with low decision-page complexity.
- Guaranteed relevant options prevent pure randomness from destroying build
  continuity.
- Upgrades create a breadth-versus-depth decision.
- Random drafting turns progression into an event rather than routine point
  allocation.

#### Limitations

- A desired build may never appear.
- Competitive multiplayer can make different offers feel unfair.
- Players may hoard rerolls or restart to manipulate results.
- Large content pools make synergies harder to communicate and balance.

#### S.I.R. lesson

Semi-random choices are viable if randomness selects the **opportunity set**, not
the result of the player's choice. Every offer should include:

- at least one role-consistent option;
- at least one option that builds on an existing capability;
- protection against duplicates or nonfunctional combinations;
- deterministic server seeding;
- a documented offer pool and weighting policy; and
- limited replacement, training, or reroll mechanisms.

For competitive modes, offers should be symmetric, pre-match, or tied to a
campaign ruleset accepted by all participants.

### Midnight Suns: loadouts and controlled tactical randomness

Midnight Suns represents abilities as cards. Each hero builds an eight-card
deck; a team's cards are shuffled together, and the player draws a limited hand
with redraws. Firaxis described this as the desired amount of randomness while
making the abilities themselves deterministic—they do exactly what they say and
do not miss
([official combat explanation](https://blog.playstation.com/2022/10/26/marvels-midnight-suns-super-heroic-turn-based-combat-and-card-tactics-explained/),
[official tactical guide](https://midnightsuns.2k.com/en-US/game-guide/gameplay/tactical-missions/)).

#### What works

- Unlocking an ability and equipping it are separate decisions.
- A fixed-size loadout prevents mature characters from using every unlocked
  tool simultaneously.
- Controlled randomness forces tactical adaptation.
- Redraws give the player agency over bad offers.
- Deterministic ability outcomes allow randomness to live in availability
  instead of resolution.

#### Limitations

- Random action availability conflicts with precise operational planning.
- Deck construction and card improvement add collection-management overhead.
- The system is designed for three heroes, not a large real-time force.

#### S.I.R. lesson

The useful idea is the **limited capability loadout**, not card draw. A unit,
squad, leader, or WASM doctrine can know many unlocked behaviors but equip only
a small number for a mission. This preserves builds, balance, API simplicity,
and pre-mission planning without randomly disabling expected actions during a
real-time engagement.

### Path of Exile: depth through topology and rule-changing nodes

Path of Exile's shared passive tree contains more than a thousand nodes. Classes
start in different positions but can travel into other regions. Notables
summarize clusters, while keystones change fundamental rules and usually include
a tradeoff. Large respecs are intentionally costly
([official passive tree](https://www.pathofexile.com/passive-skill-tree)).
Later Masteries allow players who reach a thematic cluster to select one of
several advanced effects, improving local freedom and readability
([official Mastery explanation](https://www.pathofexile.com/scourge)).

#### What works

- A shared topology makes classes soft starting positions rather than prisons.
- Notables and keystones create recognizable destinations.
- Rule-changing nodes generate build-defining identities.
- Pathing creates an opportunity cost between specialization and breadth.
- The tree itself supports long-term theorycrafting as a metagame.

#### Limitations

- Much of the tree consists of incremental pathing nodes.
- The system assumes the player is developing one primary avatar.
- Meaningful planning often moves outside the game into build tools.
- Balance changes can invalidate substantial prior planning.
- Replicating it for every S.I.R. unit would be untenable.

#### S.I.R. lesson

A Path of Exile-style web is plausible only for a **force-wide doctrine,
research, faction, or control-program architecture**. Useful elements to borrow
are:

- themed clusters;
- visible major destinations;
- a small number of rule-changing keystones with explicit tradeoffs;
- different starting positions for factions or doctrines; and
- mastery choices inside reached clusters.

Avoid hundreds of small numerical travel nodes. A S.I.R. organizational web
should likely have tens of nodes, not thousands.

### MENACE: progression concentrated at squad scale

MENACE is especially relevant because it is a contemporary tactical game with
larger formations. Infantry squads contain multiple soldiers, but named squad
leaders carry individual promotion trees, attributes, personality, and
consequences. Equipment is chosen for the squad and constrained by a
mission-level supply budget
([official description](https://store.steampowered.com/app/2432860/MENACE/)).
Its promotion design deliberately awards a shared resource for mission
performance and lets the commander decide whom to promote, avoiding kill
funneling while making repeated investment in one leader progressively more
expensive
([promotion-system developer diary](https://steamcommunity.com/games/2432860/announcements/detail/512950040394727468)).

As of July 2026 it remains in Early Access, so its details are not a settled
standard. Nevertheless, it demonstrates a current direction:

- concentrate character progression on the people the player can remember;
- make equipment and squad composition the main reversible build layer;
- make stronger gear consume more deployment capacity;
- let losses and morale create character consequences; and
- give a leader's perks effects across their squad.

This has been received well enough to merit attention: the Steam page reports
strongly positive current user reviews, and contemporary criticism specifically
praised multiple viable builds per leader and equipment with distinct niches
([Steam](https://store.steampowered.com/app/2432860/MENACE/),
[PC Gamer review](https://www.pcgamer.com/games/strategy/after-a-decade-of-stale-turn-based-tactics-menace-is-a-breath-of-fresh-air/)).
That reception is evidence for the overall package, not proof that every
individual progression mechanic is correct.

## Current progression trends

### 1. Controlled randomness rather than unrestricted randomness

The prevailing useful pattern is not “receive a random perk.” It is “choose from
a small, curated random offer.” Guarantees, tags, rerolls, and upgrade slots
protect build coherence. Randomness creates adaptation and replayability while
the player retains ownership of the decision.

S.I.R. adopts this principle as an established direction. Personnel progression
must include semi-random opportunities so that development does not collapse
into a solved deterministic build order. The player or authorized automation
chooses from the generated offer; randomness does not automatically impose the
result.

### 2. Unlock pool separated from equipped loadout

More games distinguish:

- what a character or organization has unlocked; and
- what is currently equipped, prepared, memorized, or doctrinally enabled.

This allows experimentation and horizontal growth without putting every earned
ability on the tactical interface. It is highly compatible with S.I.R.'s
equipment, magic, and control-module model.

### 3. Qualitative nodes over incremental stat tax

The strongest perks change decisions, timing, targeting, positioning, resource
use, or rule interactions. Battle Brothers' developers explicitly argued that a
perk should support a strategy, not invalidate core mechanics, and should
require skill to exploit. Path of Exile uses Notables and Keystones to make the
important destinations legible.

Small percentage bonuses still have a role in attributes and equipment, but
they are weak promotion rewards.

### 4. More reversible experimentation

Modern systems increasingly provide respecs, loadouts, or post-campaign freedom.
Gears Tactics, for example, grants reset opportunities and later allows free
respec. The trend is not necessarily consequence-free choice; it is protection
against permanently ruining a long campaign before the player understands the
system.

For S.I.R., permanent history can remain in injuries, experience, and
relationships while doctrine and loadouts remain more reversible.

### 5. Progression attached to teams and relationships

War of the Chosen's bonds, Midnight Suns' friendship progression, and MENACE's
squad-leader model all move some advancement away from isolated individuals.
This produces tactical identity through relationships and formation context.

S.I.R.'s squad leadership and communications make this direction especially
natural.

### 6. Horizontal options protected by budgets

Loadout slots, card-deck sizes, supply budgets, memory slots, and doctrine slots
let the game unlock more content without unlimited power accumulation. MENACE's
supply system explicitly exists to keep lower-cost equipment viable rather than
allowing best-in-slot gear everywhere.

### 7. Progression as build expression, not only power

The best systems let two equally advanced units behave differently. Pure
vertical progression tends to erase early content, create mandatory grinding,
and complicate multiplayer fairness.

## What consistently works well

### Clear role before detailed choices

A player should understand “scout,” “squad leader,” “medic,” or “electronic
warfare specialist” before inspecting individual nodes. Role labels reduce the
cost of managing a roster.

### Few high-impact promotion decisions

Four to seven qualitative decisions usually create enough combinations for
identity. Beyond that, the burden shifts from tactics to build administration.

### Separate permanent identity from mission adaptation

A useful division is:

- **persistent:** aptitude, history, injuries, experience, signature traits;
- **retrainable:** learned specialization and doctrine;
- **swappable:** equipment, prepared magic, software behavior packages;
- **temporary:** mission effects, morale, suppression, supply, intelligence.

### Perks that alter behavior

Good perks create new conditions or responses:

- remain concealed after a constrained action;
- relay sensor data through a particular network;
- execute a different reaction when ambushed;
- trade speed for reduced detection;
- coordinate fire when a leader confirms a target; or
- operate under communications loss for longer.

Weak perks merely add another small percentage to something the unit already
does.

### Bounded asymmetry

Units should differ, but no recruit should be unusable because of opaque random
growth. Randomness should produce a tactical prompt, not a trap.

### Build commitments with escape valves

Choices should matter over a meaningful period, while retraining exists at an
appropriate cost in time, logistics, morale, or opportunity. The player should
not need to discard a veteran because an early tooltip was misunderstood.

## Failure modes to avoid

### Promotion inbox overload

At 100 units, even four manual promotions per unit create 400 interruptions.
S.I.R. accepts the underlying individual depth but must not require 400 manual
modal interactions. Batch policies, auto-development templates, AI
recommendations, scripted allocation, exception queues, and audit logs are
necessary. Automation should apply the player's doctrine consistently while
allowing inspection and override.

### Automation farming

WASM modules can execute repeatable behaviors more consistently than humans.
Progression based on shots fired, meters moved, healing performed, or other raw
action counts will be optimized and exploited. Use mission-capped evidence,
objectives, risk, outcomes, or training allocation.

### Best-in-slot convergence

If progression only increases power and carries no deployment, supply, or
opportunity cost, mature forces converge on the same top-tier choices.

### Perk invalidates core system

A perk that simply removes suppression, communications, logistics, facing, or
stealth from consideration destroys the system that makes S.I.R. distinctive.
Perks should bend rules under conditions, not erase them universally.

### Hidden breakpoint optimization

Exact attribute thresholds invite spreadsheets and make a one-point difference
disproportionately important. If thresholds exist, show them clearly and use
few, broad tiers.

### Randomness without recovery

Pure random perks can determine competitive strength before tactics begin.
Offer choice, guarantees, server-visible seeds, and limited correction.

### Individual progression eclipses force design

If veteran units become overwhelmingly powerful, logistics, replacement,
formation, and doctrine become secondary to protecting a small set of
super-soldiers. That would work against the intended 50–100-unit scale.

## Design choices for S.I.R.

### Choice A: Direct XCOM model

Every unit receives a role/class, ranks up six or seven times, and selects one of
two abilities at most ranks. Attributes grow automatically.

**Advantages**

- Proven and immediately understandable.
- Clear class identity.
- Strong emotional attachment to veterans.
- Moderate implementation and balancing scope.

**Disadvantages**

- Hundreds of promotion decisions.
- Repeated builds across a large roster.
- Difficult to inspect during force preparation.
- Encourages the player to value individuals over squad and logistics systems.

**Fit:** Strong if progression is designed as an automation-first public
contract with templates, scripts, batch application, and human override.

### Choice B: Semi-random milestone draft

At three to five career milestones, a unit chooses one of three perks drawn from
role-, history-, and behavior-tagged pools.

**Advantages**

- High campaign variation.
- Units develop distinctive histories.
- A small choice surface at each milestone.
- Supports unusual combinations without a huge visible tree.

**Disadvantages**

- Fairness concerns in competitive play.
- Desired doctrines cannot be guaranteed.
- Requires careful offer-generation rules.
- Still produces many decisions at 100 units.

**Fit:** Good as a bounded source of variation for any persistent individual,
provided policies can evaluate offers and core role competence is guaranteed;
weak as the only progression system.

### Choice C: Classless compact web

Each unit spends approximately 8–10 points in a shared web of 25–40 nodes.
Starting aptitude or role determines the initial region.

**Advantages**

- High build freedom.
- Supports hybrids and magic/technology combinations.
- One system can represent many roles.
- Attractive to theorycrafting players.

**Disadvantages**

- Too much per-unit planning.
- High balance and API-description cost.
- Tends toward solved paths.
- Easy to create ineffective builds.

**Fit:** Possible per individual through automation, but difficult for humans to
understand and audit at force scale; stronger for squad doctrine or force-wide
research.

### Choice D: Equipment-led horizontal progression

Units gain modest attributes, while weapons, armor, sensors, magic implements,
software packages, and support assets provide most active capabilities.

**Advantages**

- Builds are reversible.
- Logistics becomes progression.
- Mission preparation matters.
- The public API can expose a clean capability set based on loadout.
- Replacement units remain useful.

**Disadvantages**

- Individual veterans may feel interchangeable.
- Equipment economy can dominate all other rewards.
- Loadout management can itself become drudgery.

**Fit:** Strong as a major layer, but insufficient by itself.

### Choice E: Leader-centered progression

Ordinary units develop mostly automatically. Squad leaders receive XCOM-sized
trees, and squads select a compact doctrine/loadout that defines behaviors,
communication, reactions, and tactical specialties.

**Advantages**

- Scales naturally to 50–100 units.
- Makes the command hierarchy mechanically meaningful.
- Concentrates memorable choices on memorable actors.
- Connects progression directly to WASM control behavior.

**Disadvantages**

- Conflicts with the established requirement that every unit is a persistent
  individual with meaningful handling.
- Ordinary members may lack sufficient development and identity.
- Leader loss can create a severe capability cliff.
- Requires rules for succession and doctrine retention.
- A leader tree can become mandatory if its bonuses affect too many members.

**Fit:** Useful as a supplementary command layer, but not sufficient as the
complete personnel progression model.

### Choice F: Layered hybrid

Combine:

- a fixed class establishing each unit's core role;
- bounded individual attributes and proficiencies for every unit;
- four to six qualitative progression choices for every unit;
- automation policies for routine development;
- leader-specific command specializations;
- squad doctrine loadouts;
- equipment-provided active capabilities; and
- a compact force-wide research/doctrine web.

**Advantages**

- Preserves persistent individual consequence and development.
- Uses scripts and AI to prevent individual depth from becoming administrative
  overload.
- Supports tactical, logistical, technological, magical, and organizational
  progression in distinct layers.
- Different progression layers can use different degrees of permanence.
- Matches S.I.R.'s command topology.

**Disadvantages**

- Requires strict boundaries to avoid additive complexity.
- UI must show why a unit has a capability.
- Balance must account for interactions across several layers.

**Fit:** Best overall, provided the total number of simultaneous decisions is
budgeted and every progression operation is exposed through an automation-ready
API.

## Recommended prototype

Prototype Choice F with the following provisional budget.

### Recruitment dossier

Use an informed-hiring model broadly comparable to XCOM and Jagged Alliance.
Before recruitment, expose the candidate's permanent class, current attributes,
proficiencies, learned abilities, traits, relevant history, known injuries or
conditions, and recruitment terms.

Candidates are procedurally generated rather than selected from a primarily
authored roster. Generation creates a stable candidate record; recruitment then
preserves that individual's identity, development, and history throughout the
campaign.

The dossier describes the candidate as they currently exist. It does not reveal
future semi-random progression offers or a deterministic final build. All
clients receive the same authoritative dossier fields through the public API;
hidden future rolls are not available to either the canonical client or a
third-party client.

### Individual unit

- One permanent class with a stable role and public machine-readable
  identifier.
- The class is assigned before recruitment, disclosed while the candidate is
  evaluated, and not selected after recruitment.
- 5–7 universal attributes.
- 3–6 relevant proficiencies selected from a broader catalog.
- One origin or aptitude trait at creation.
- 4–6 qualitative progression milestones over a campaign.
- One meaningful selection at each milestone, usually from two fixed options or
  two fixed options plus one eligible semi-random opportunity.
- Semi-random offers are class-compatible but allow meaningful variation within
  the class.
- No unrestricted class switching or multiclassing.
- A later advanced-class specialization may evolve from the permanent base
  class while preserving its class lineage.
- Primarily automatic numerical growth influenced by authoritative experience,
  training, assignment, or other in-world rules.
- Manual point-by-point attribute allocation is not the default interaction.
- Persistent personal history, development, injuries, and other consequences.
- Injuries and temporary conditions recorded separately from progression.

Attributes should answer broad questions such as:

- How durable and physically capable is the unit?
- How quickly and precisely can it act?
- How well does it perceive and interpret the environment?
- How well does it function under pressure and communications loss?
- Which technical, medical, leadership, or magical tasks can it perform?

The exact attribute list should follow the combat, perception, communications,
and magic rules rather than precede them.

### Squad leader

- Uses the same individual progression depth and rules as other personnel.
- Gains access to leader-specific choices emphasizing command, information,
  coordination, logistics, and reactions.
- Leadership choices compete with or replace some general choices rather than
  automatically doubling the number of perks.
- Player-provided WASM logic determines local succession after leader loss.
- Headquarters communication remains tied to physical communications equipment:
  a successor must already carry a redundant device or recover the fallen
  leader's device.
- Leadership progression should affect command behavior or equipment use without
  making the abstract leader role itself a source of connectivity.

### Personnel-management automation

- Every progression operation is available through a stable, versioned API.
- Policies can filter units by role, attributes, existing perks, squad,
  doctrine, equipment, history, and eligible offer tags.
- Scripts can preview a change, validate it, and apply it in batches.
- The standard client provides official templates and AI recommendations.
- The server returns reasons for eligibility, recommendation, rejection, and
  application.
- Players can lock individuals against automation and override any uncommitted
  recommendation.
- Applied decisions produce an audit history identifying the policy, script, or
  human action responsible.
- Hidden information is never exposed to personnel-management automation.

### Squad doctrine

- A doctrine family establishes the broad role.
- 3–5 doctrine slots equip unlocked behaviors or tactical rules.
- Doctrine packages define capabilities visible to the WASM module.
- Changing doctrine requires preparation, training time, or logistics, but does
  not destroy prior unlocks.
- The standard control module ships with behavior profiles for every supported
  doctrine.

### Equipment and magic

- Personal progression can directly unlock major active abilities.
- Equipment, magic implements, implants, consumables, software, and support
  assets can grant additional actions or enable and modify learned abilities.
- Slots and supply cost limit simultaneous capability.
- More powerful equipment is not automatically optimal for every mission.
- Capabilities are exposed through stable API tags and action contracts.

### Organization

- A compact web or clustered tree of approximately 30–60 meaningful nodes.
- Nodes unlock training, equipment, doctrine, logistics, sensor, electronic
  warfare, communications, and magical options.
- A few keystone-style nodes change organizational rules with explicit
  tradeoffs.
- Avoid universal percentage stacking wherever an option unlock is possible.

## Semi-random choice recommendation

Semi-random progression opportunities are part of the intended system. Use them
where they create adaptation and individual identity, while keeping baseline
role competence deterministic.

### Good uses

- leader personality or history opportunities;
- rare veteran specializations;
- magical mutations or portal exposure;
- field commendations based on mission events;
- recruit aptitude;
- research opportunities; and
- temporary campaign adaptations.

### Poor uses

- basic role competence;
- required communication functions;
- access to core weapons;
- standard control-module compatibility;
- essential counterplay in competitive matches; and
- fundamental attributes needed for a chosen squad role.

### Suggested offer rule

When a leader reaches a draft milestone, offer three options:

1. one guaranteed option from the leader's current role;
2. one option that extends an existing perk, doctrine, or recent experience;
3. one wildcard from an eligible cross-role, history, magic, or technology
   pool.

The player chooses one. A costly retraining process can later replace a perk, but
cannot reroll repeatedly until the perfect result appears.

## Multiplayer requirements

### Established mode requirements

S.I.R. requires more than one progression context:

1. **Persistent main mode** — personnel, history, and progression carry across
   matches within a fixed-duration campaign. Personnel belong to the player
   account, and campaign state is wiped at campaign end. This mode can combine
   PvE, PvP, and cooperative play.
2. **PvP duel and skirmish modes** — bounded force construction uses a point
   budget and a standardized unit catalog. Persistent main-mode personnel are
   not used.
3. **PvE and cooperative scenarios** — may use persistent main-mode personnel or
   a scenario-provided roster, depending on the ruleset.

Persistent main-mode PvP does not have to make all opposing forces identical.
Fairness can instead use opponent selection, matchmaking, deployment costs,
mission asymmetry, objectives, risk, and rewards. The game should make these
differences legible rather than implying that persistent forces are equal.

Point-based duels and skirmishes provide the controlled competitive context.
Players construct forces from a standardized competitive catalog at defined
progression levels rather than bringing persistent personnel. Units, attributes,
perks, equipment, magic, doctrines, eligibility rules, and point costs must be
versioned and available through the public API.

This is easier to balance and reproduce than pricing arbitrary persistent
personnel. It also allows two players to recreate the same force independently,
supports tournament rules, and prevents accumulated campaign power from
determining catalog-based competitive matches.

Duel and skirmish results are completely isolated from the persistent main
campaign. They do not grant campaign experience, resources, personnel, or
equipment, and they do not write injuries, deaths, losses, consumption, or other
consequences back to campaign state.

### Progression scopes

The canonical ownership and lifecycle are:

- the player account owns its personnel;
- personnel and progression persist between matches in the active campaign;
- each campaign has a fixed duration; and
- campaign personnel, progression, and other campaign state are wiped when that
  campaign ends.

A provisional canonical cadence would run each campaign for two weeks and start
a new campaign every week, leaving two campaign cohorts active at a time. This
is a hypothesis for testing rather than an accepted duration. Whether one
account can participate in both overlapping campaigns is unresolved.

Separate single-player campaigns may use independent state. Open-source servers
and derivatives may provide different lifecycles, so the engine and API should
implement persistence as a ruleset policy rather than a single hard-coded
global behavior.

The architecture must therefore distinguish:

1. **Ownership scope** — the account that owns a record.
2. **Campaign namespace** — the campaign within which that record exists and
   progresses.
3. **Roster source** — persistent personnel, standardized catalog, draft, or
   scenario assignment.
4. **Match effects** — temporary progression or conditions that exist only
   during one match.
5. **Write-back policy** — which experience, injuries, deaths, equipment
   changes, and rewards return to persistent state.

Every mode should provide an authoritative ruleset manifest containing at least:

- mode identifier and ruleset version;
- roster source;
- point or deployment budget;
- permitted content and progression range;
- persistence and write-back policy;
- campaign start, end, and reset policy;
- random-offer policy and seed ownership; and
- victory, reward, and consequence rules.

Custom clients and WASM modules create additional requirements:

- Every attribute, proficiency, perk, and doctrine effect needs a stable
  machine-readable identifier.
- The server remains authoritative over eligibility and outcomes.
- Modules receive only capabilities and information their unit or squad is
  entitled to use.
- Random offers use server-owned seeds and auditable generation rules.
- Execution budgets cannot vary with progression in a way that rewards more
  compute.
- A perk must never grant hidden API access to world truth.
- Standard and custom controllers must see the same capability contract.
- Point costs and roster-eligibility calculations must be public,
  machine-readable rules rather than privileged canonical-client logic.
- Catalog entries must resolve to complete authoritative unit definitions so
  canonical and alternative clients can validate and reproduce a force.
- The server does not disclose an opponent's selected personnel build, perks,
  abilities, doctrine, or loadout before a match.
- During PvP, opponent build information is available only through battlefield
  observation and the player's resulting knowledge state.
- A public catalog defines possible content but does not reveal which content an
  opponent selected.

## Evaluation criteria

Each candidate prototype should be evaluated by measuring:

- promotion decisions per hour and per campaign;
- time spent managing progression versus planning missions;
- number of visibly distinct viable builds;
- percentage of choices dominated by one option;
- ability to identify a unit or squad's role at a glance;
- replacement-unit viability;
- power gap between a new and veteran force;
- frequency of respec or restart regret;
- WASM API complexity;
- ease of explaining why an action is available;
- exploitability of progression triggers;
- competitive variance introduced by random offers; and
- whether progression strengthens positioning, communications, intelligence,
  and logistics rather than bypassing them.

## Recommended next decisions

Before accepting a progression architecture, determine:

1. How opponents and force disparities are handled in persistent main-mode PvP.
2. Whether the proposed two-week duration and weekly launch cadence produce the
   intended progression arc, population level, and recovery from a poor start.
3. Whether one account may participate in both overlapping campaign cohorts.
4. The exact boundary between progression-unlocked active abilities and actions
   granted or enabled by equipment, doctrine, magic, and control software.
5. Whether semi-random progression offers are final or can be replaced through a
   limited reroll, training, or resource mechanism.

## Sources

Primary and official sources were preferred for system structure. Community
references were used where official mechanical detail was unavailable.

- [XCOM 2 manual](https://www.feralinteractive.com/en/manuals/xcom2/latest/steam/)
- [XCOM 2: War of the Chosen — bonds and ability points](https://xcom.com/news/xcom-2-war-of-the-chosen-soldier-bonds-and-new-skills/amp/)
- [Phoenix Point game features](https://phoenixpoint.info/game)
- [Phoenix Point character progression development article](https://phoenixpoint.info/blog/2019/3/26/backer-build-four)
- [Gears Tactics class guide](https://www.gearsofwar.com/en-us/game-guide/classes/)
- [Gears Tactics developer interview on respec](https://news.xbox.com/en-us/podcast/721-gears-tactics-on-xbox-series-xs/)
- [Battle Brothers character-stat design](https://battlebrothersgame.com/character-stats/)
- [Battle Brothers perk-system design](https://battlebrothersgame.com/dev-blog-80-progress-update-new-perk-system/)
- [Jagged Alliance 3 official site](https://jaggedalliance3.thqnordic.com/jaggedalliance3/US/win)
- [Wildermyth official wiki](https://www.wildermyth.com/wiki/Main_Page)
- [Wildermyth ability rules](https://www.wildermyth.com/wiki/Abilities)
- [Midnight Suns combat and deck explanation](https://blog.playstation.com/2022/10/26/marvels-midnight-suns-super-heroic-turn-based-combat-and-card-tactics-explained/)
- [Midnight Suns tactical guide](https://midnightsuns.2k.com/en-US/game-guide/gameplay/tactical-missions/)
- [Path of Exile passive skill tree](https://www.pathofexile.com/passive-skill-tree)
- [Path of Exile Masteries](https://www.pathofexile.com/scourge)
- [MENACE official Steam page](https://store.steampowered.com/app/2432860/MENACE/)
- [MENACE promotions developer diary](https://steamcommunity.com/games/2432860/announcements/detail/512950040394727468)
- [MENACE supply and equipment developer diary](https://steamcommunity.com/ogg/2432860/announcements/detail/535466136352850185)
- [Tactical Breach Wizards official Steam page](https://store.steampowered.com/app/1043810/Tactical_Breach_Wizards/)
