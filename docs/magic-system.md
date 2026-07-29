---
title: Risk-Based Magic
category: Battlefield Systems
categoryindex: 4
index: 17
status: accepted
decision-status: canonical
document-type: living-design
version: "1.5"
last-updated: 2026-07-29
related:
  - docs/game-vision.md
  - docs/setting-and-factions.md
  - docs/arcane-forces.md
references:
  - Frostgrave
  - Warcrow
---

# Risk-Based Magic System

## Purpose

This document defines the architectural foundation for spellcasting, health
empowerment, strain, breaches, and shattering. Exact formulas and content remain
open, but the risk relationship is established.

## Design intent

Magic should provide effects strong enough to create faction-level asymmetry
without becoming a predictable cooldown rotation or a separate mana-based
weapon system.

The caster decides how much immediate power and future instability to accept.
Enemy action can exploit that decision by damaging, suppressing, isolating, or
forcing additional action from a strained caster.

The design takes qualitative inspiration from:

- **Frostgrave**, where a caster can spend Health to improve a casting result
  and sufficiently failed casts cause damage; and
- **Warcrow**, where spell properties can be altered and magical use interacts
  with accumulating adverse state.

S.I.R.'s HP-versus-strain breach threshold and shattering outcomes are its own
system rather than a reproduction of either reference's dice rules.

## Established core rules

### Health

Spellcasters have current HP under the normal authoritative damage model.
Current HP serves three purposes in magic:

1. survivability;
2. a resource that can be deliberately spent to empower spells; and
3. the threshold against which accumulated strain is compared.

Using current rather than maximum HP ensures that wounds and empowerment alter
magical safety immediately.

### Casting failure

Spells are not automatically successful. Failed casting can cost the caster HP.
The exact casting check and failure-damage function remain open.

### Empowerment

Before resolution, a caster may spend HP to empower one or more declared spell
aspects. Candidate aspects include:

- effect strength;
- range;
- area or number of targets;
- duration;
- penetration or resistance difficulty;
- casting speed;
- precision;
- reliability;
- concealment or signature; and
- resistance to interruption or counter-magic.

The set is illustrative. Each spell declares which aspects can be empowered,
their costs, limits, and incompatibilities.

### Strain

Casting accumulates strain on the caster. Strain represents mounting magical
instability and persists beyond a single casting action.

The exact strain gain, reduction, persistence, and recovery rules remain open.
Spell definition, selected empowerment, casting result, environmental
conditions, injuries, equipment, and status effects are possible inputs.

### Strain recovery is unresolved

The recovery model has not been selected. No canonical rule currently divides
Strain into settling and residual components, requires meditation, or makes
part of every cast permanent.

A recovery proposal must specify:

- whether Strain decreases during a match;
- the action, rate, and conditions for any decrease;
- interruption and detection rules;
- interaction with HP healing and regeneration; and
- whether a breach can discharge Strain before another threshold check.

Until that decision is accepted, implementations and content must treat
recovery as ruleset data and must not infer a permanent match-long cost from
casting.

### Breach threshold

Whenever accumulated strain exceeds current HP, the caster must make a breach
check. This condition is evaluated whenever either value changes, not only when
the caster performs a magical action.

```text
breach_excess = strain - current_hp

if breach_excess > 0:
    resolve breach check
```

Both the breach-check result and `breach_excess` contribute to severity. A
caster barely over the threshold faces less expected danger than one whose
strain greatly exceeds remaining HP.

Any authoritative reduction in current HP immediately reevaluates the
threshold. Enemy damage, self-inflicted costs, environmental damage, ongoing
conditions, or other HP loss can therefore force an immediate breach check
against strain the caster had already accumulated. The caster does not need to
be casting when this occurs.

This makes a strained caster a volatile battlefield asset. Protecting and
healing that caster preserves magical safety, while identifying and wounding
them is direct counterplay that may cause consequences beyond ordinary damage.

### Breach and shattering

Breach consequences range from bad outcomes to catastrophic shattering events.
The final table is not designed, but its outcome contract may affect:

- the caster;
- nearby units;
- terrain and cover;
- active spells;
- communications and sensors;
- portal stability;
- summoned or controlled entities;
- persistent injuries or transformations;
- the mission environment; and
- campaign state.

Catastrophic outcomes must be rare enough to preserve decision-making but
credible enough that high strain cannot be treated as a routine efficiency
calculation.

A resolved breach discharges some, but not necessarily all, of the caster's
accumulated strain. A breach is therefore a partial release rather than an
automatic reset to a safe state. The discharge amount or rule remains to be
designed and may depend on the breach result, severity, spell, caster, or
shattering outcome.

## The core feedback loop

```text
cast or empower
      ↓
gain effect now
      ↓
lose HP and/or gain strain
      ↓
lower safety margin
      ↓
enemy pressure or further casting
      ↓
breach risk
      ↓
backlash or shattering
```

Spending HP is especially consequential because it can simultaneously:

- improve the current spell;
- move the caster closer to incapacitation;
- increase `strain - current_hp`; and
- trigger or worsen a breach check.

## Individual spells and cooperative rituals

The health-and-strain rules govern an individual caster's spells. The arcane
civilization also uses **rituals**, which are not long spells: they require
multiple casters to maintain a quorum at a prepared site and cannot be completed
by one caster given more time.

Every senior caster normally operates with two or three persistent magical
assistants. Assistants can cast only a bounded lesser repertoire and contribute
ritual preparation, maintenance, stability, interruption tolerance, or
controlled shutdown. Exact lesser spells and contribution values are content
data, but their supporting role is canonical.

A minor working may use one senior caster and assistants, and a standard ritual
may use a complete caster cell. A major ritual or portal still requires
multiple senior casters as well as any assistant contribution it declares.
Assistants never count as unrestricted replacements for required senior
casters, cannot provide unlimited acceleration, and are not batteries that
remove HP, Strain, breach, component, or anchor-capacity costs.

Rituals are paid primarily through caster commitment, preparation time,
components, exposure, and temporary anchor capacity. Whether participating also
causes health expenditure or strain is deliberately open; those costs must not
erase the distinction by turning a ritual into several ordinary casts executed
together.

Rituals lock a geographical target or trigger from information available when
they are prepared. Their cooperative progress, interruption, delayed
culmination, trap, and portal contracts are canonical in
[Arcane Civilization Forces](arcane-forces.md).

## Resolution architecture

A spell request must be an atomic authoritative command containing at least:

- caster identifier;
- spell and version identifier;
- targets or target area;
- declared aspect modifications;
- HP committed to each aspect;
- any consumed items or external resources; and
- the local knowledge used to form the request.

The server validates:

- that the caster knows and can currently use the spell;
- target legality under local information;
- aspect and HP-spend limits;
- current HP and strain;
- range, line of sight, facing, and other spatial requirements;
- cast time and interruption state;
- resource and equipment prerequisites; and
- host-class and ruleset compatibility.

The exact ordering of HP payment, casting resolution, failure damage, strain
gain, ordinary incapacitation, and breach resolution must be specified before
implementation. Different orderings can produce materially different outcomes
near zero HP and the breach threshold.

**This is the one open decision rather than an open value**, and it is the
single thing canonical status below does not cover. It has the same shape as the
consequence-ordering contract combat resolution still owes, and the two should
probably be settled together, since a spell resolving on the same tick as
incoming damage is exactly where they meet.

## Information and control-module contract

A spellcaster's WASM instance requires machine-readable access to:

- current HP and strain;
- known spells and empowerable aspects;
- legal aspect ranges and costs;
- breach trigger and public severity rules;
- locally known targets and environmental modifiers;
- current cast, interruption, and cooldown state; and
- authoritative results and consequences after resolution.

The API should expose enough rule information for human and automated risk
decisions without exposing future random results or hidden world state.

The standard module needs explicit risk policies, such as:

- conservative strain ceiling;
- emergency HP-spend allowance;
- protected-caster behavior;
- willingness to risk a breach for mission-critical outcomes;
- friendly-proximity constraints; and
- evacuation or recovery behavior after high strain.

## Counterplay

Opponents can interact with magic through:

- damaging the caster to reduce the strain threshold and potentially trigger
  an immediate breach;
- forcing repeated casting;
- interrupting cast time;
- suppressing or displacing the caster;
- separating the caster from healing or protection;
- threatening objectives that demand empowerment;
- counter-magic or resistance where available;
- exploiting visible spell signatures; and
- disengaging until accumulated strain becomes tactically costly.

Counterplay should not require human factions to receive magic. Technology,
positioning, reconnaissance, timing, ranged fire, electronic effects,
specialized materials, or portal research can provide bounded responses.

## Healing interaction

Healing a caster can increase current HP and therefore restore breach margin.
That makes healing part of the magical economy even when the healer is
nonmagical.

The design must prevent a trivial infinite loop in which healing converts
ordinary supplies into unlimited safe spellcasting. Possible constraints
include persistent strain, healing limits, injury state, reduced maximum HP,
scarce treatment resources, treatment time, and diminishing recovery. No
specific constraint is selected yet.

## Nonmagical arcane-civilization units

The organized arcane civilization includes nonmagical goblins, orcs, trolls,
and potentially other peoples or creatures.

Their purposes include:

- protecting and screening strained casters;
- holding terrain without accumulating magical risk;
- applying conventional pressure that forces human expenditure;
- carrying armor, supplies, components, or magical infrastructure;
- providing resilient shock or line units; and
- ensuring the faction remains functional after caster loss.

Substantial armor and healing for some of these units is a provisional
direction. Their recovery must have clear rates, limits, damage interactions,
and counters so regeneration does not become indistinguishable from excessive
HP.

## What canonical status covers

Settled, and later work should build against it: health as survivability,
empowerment currency, and breach threshold simultaneously; casting that can fail
and cost health; Strain that accumulates from casting and remains in state until
an explicit rule changes it; the breach
check whenever strain exceeds current health, re-evaluated on any health change;
severity depending on both the check and the excess; breaches discharging some
strain rather than necessarily resetting it. Rituals are distinct from
individual spells: they require a caster quorum at a prepared site, commit to a
geographical target or trigger, and use the culmination, prepared-trap, and
portal shapes defined by the arcane faction contract. Controlled transit,
deliberate goblin incursion, and catastrophic daemon breach are distinct portal
contracts. Every senior caster normally has two or three persistent magical
assistants with lesser spells and ritual-support abilities. Minor workings may
use one caster cell, but major rituals and portals still require multiple senior
casters; assistants cannot erase the system's ordinary costs or risks.

Not settled: every numeric value, Strain persistence and recovery, the
resolution ordering noted above, and the prototype questions below.

## Balance and failure modes

Avoid:

- a mathematically obvious safe strain ceiling used in every situation;
- HP empowerment so efficient that casters always spend to the same breakpoint;
- catastrophic results frequent enough to make planning irrelevant;
- catastrophes so rare that rational play ignores them;
- healing that erases both health and strain costs;
- hidden severity rules unavailable to player modules;
- effects that bypass fog of war or grid rules without explicit contracts;
- AI casters receiving better risk information than future playable versions;
- rituals using privileged server knowledge to track targets after preparation;
- ritual interruption erasing so much work that incidental damage makes the
  system unusable;
- unlimited acceleration from adding every available caster to one ritual; and
- nonmagical faction units existing only as disposable HP screens.

## The spell and ritual set

The spells themselves are content rather than architecture and live in
[Arcane Spells](arcane-spells.md). The set is deliberately utility-dominant,
because a caster who is artillery is a worse gun than a gun, with a small number
of expensive decisive options so that an unattended caster remains a mistake.

The ritual catalog is separate and initially contains the three shapes defined
by the faction contract: delayed culmination, prepared trap, and portal ritual.
Portal rituals distinguish controlled transit from uncontrolled goblin
incursion; daemon breaches are catastrophic anchor failures rather than chosen
rituals. Their exact effects and values remain prototype questions.

## Prototype questions

1. What base casting check creates useful uncertainty at 20 simulation ticks per
   second?
2. When is HP paid relative to casting success?
3. How much strain does a cast generate, and what modifies it?
4. Can Strain decrease during a match? If so, what action, rate, interruption,
   and detection rules govern recovery?
5. After a breach partially discharges strain, when is another breach check
   required if the caster remains above the threshold?
6. What inputs determine breach severity?
7. What is the smallest harmful breach outcome?
8. What qualifies as a shattering catastrophe?
9. Can a caster voluntarily vent, transfer, contain, or weaponize strain?
10. Can allies share or absorb breach consequences?
11. How visible are strain, empowerment, and breach risk to enemies?
12. How do armor, healing, regeneration, and temporary HP interact with the
    threshold?
13. Which spell aspects are universal, and which are spell-specific?
14. How is the amount of strain discharged by a breach determined?
15. If recovery emits a magical signature, how far does it carry and how does
    signal strength scale with the amount recovered?
16. What caster quorum, preparation time, positioning, interruption, and
    contribution rules make rituals cooperative without making one casualty
    erase excessive progress?
17. Do ritual participants gain strain or spend health, or are caster
    commitment, components, exposure, and anchor capacity sufficient costs?
18. Which culmination effects and trap triggers reward prediction without
    becoming unavoidable delayed attacks or permanent area denial?
19. What bounds goblin-portal output and prevents farming, safe deployment, or
    reliable use as artillery while preserving its value as predicted chaos?

## Sources

- [Frostgrave: Second Edition official quick-reference sheet](https://www.ospreypublishing.com/media/ep5c2oi2/frostgrave_qrs-1.pdf)
- [Warcrow official how-to and stress overview](https://warcrow.com/en/games/warcrow/how-to-play)
- [Warcrow official magic rules update](https://downloads.corvusbelli.com/warcrow/wargame/rules/rules-update-1.5.1-en.pdf)
