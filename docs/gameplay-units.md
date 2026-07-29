---
title: Units, Classes, and Progression
status: proposed
document-type: reference
category: Forces & Equipment
categoryindex: 3
index: 3
version: "0.3"
last-updated: 2026-07-29
related:
  - docs/gameplay-reference.md
  - docs/human-forces.md
  - docs/arcane-forces.md
  - docs/research/arcane-horde-progression-and-equipment.md
---

# Gameplay Units, Classes, and Progression

## Summary

🟦 **Human forces** distribute capability across persistent specialists,
equipment, communications, and combined-arms squads. 🟪 **Arcane forces**
concentrate decisive capability in a few casters supported by nonmagical
goblins, orcs, trolls, and other peoples. Human classes are canonical; detailed
arcane species roles and caster perk families remain proposals. Candidate
species statistics from the fixed-state rules laboratory are prototype values.

Return to the [Gameplay Reference](gameplay-reference.md).

## Shared unit facts

⬜ 🟩 Every acting object is an authoritative unit with:

- identity and ownership;
- a square grid footprint and physical position;
- body facing and attention direction;
- current action, stance, and readiness;
- locally acquired observations and received reports;
- [HP](gameplay-formulas.md#hp), [Armor](gameplay-formulas.md#armor),
  [Wounds](gameplay-formulas.md#wounds), and
  [Suppression](gameplay-formulas.md#suppression) where applicable;
- equipment, capabilities, progression, and resources; and
- its own WebAssembly control-module instance.

A drone, caster, goblin, or troll is a unit under these same rules. No faction
receives a privileged squad-wide controller or hidden world truth.

### Canonical base and symbol rule

🟩 Every unit occupies one axis-aligned `N×N` square base and is represented by
one square information symbol fitted to that base with the standard inset. The
symbol and its class glyph scale uniformly with the base. Large units do not
use a one-cell symbol, a rectangular or stretched symbol, or repeated symbols.
The footprint remains authoritative for occupancy; the fitted symbol is its
visual expression.

## 🟦 Human force

### Squad

🟩 A standard human infantry squad contains **eight to ten people**:

- one squad leader;
- two subordinate teams;
- a designated second-in-command; and
- a designated third-in-command.

A side fields roughly **six to twelve squads** at the intended 50–100-person
force size. Leadership is an assignment backed by personal qualification, not a
seventh class. Any qualified class can hold command.

### Classes

Class is permanent and says what a person is good at. Equipment is reversible
and says what the person can currently do.

| Class | Canonical competence |
|---|---|
| 🟦 **Rifleman** | Broad baseline competence and flexible substitution |
| 🟦 **Gunner** | Sustained area fire, support weapons, and fire discipline |
| 🟦 **Marksman** | Slow-building precision at range and observation |
| 🟦 **Engineer** | Breaching, demolition, deployables, and prepared positions |
| 🟦 **Medic** | Aid, stabilization, and specialist casualty procedures |
| 🟦 **Signaller** | Communications, EW, relays, direction finding, and drones |

A rifleman may carry a support weapon and perform the gunner role, but the
gunner's proficiency and progression make the gunner better at it. Baseline
class competence cannot be withheld by random progression.

### Progression

🟩 Human personnel target approximately XCOM-like individual complexity across
a much larger roster:

```text
authoritative participation, training, and significant events
        ↓
automatic bounded attribute growth
        ↓
automatic class-compatible qualitative outcome
        ↓
inspectable development report
```

Every person can have attributes, proficiencies, traits, abilities, history,
injuries, and approximately four to six qualitative milestones. Numerical and
qualitative advancement resolves automatically through deterministic
server-owned randomness.

Players influence future eligibility and weighting through persistent training,
assignment, squad, mentoring, and company policies. Policies can be applied in
bulk but cannot guarantee a perk. Outcomes are permanent, cannot be rerolled,
and record their inputs and random purpose.

### Perk families

Perks change decisions, conditions, responses, preparation, and cooperation.
They are not primarily stacks of percentage bonuses.

#### Rifleman

| Perk | Tactical change |
|---|---|
| **Point Man** | Improves the constrained first response while advancing |
| **Bounding Partner** | Restores readiness better after movement under confirmed covering fire |
| **Quiet Advance** | Trades speed for reduced visual and acoustic signature |
| **Cross-Trained** | Reduces, but does not erase, off-class equipment penalties |
| **Local Initiative** | Executes the last received intent better while disconnected |
| **Rear Guard** | Maintains observation and readiness during disengagement |

#### Gunner

| Perk | Tactical change |
|---|---|
| **Traverse Discipline** | Redirects an area engagement while preserving preparation |
| **Beaten Zone** | Chooses narrow/deep or broad/shallow engagement shapes |
| **Walking Fire** | Shifts suppression along a declared movement path |
| **Fire Control** | Avoids wasting ammunition on unsuitable parts of an area |
| **Final Protective Fire** | Prepares an ammunition-expensive close defensive line |
| **Crew Drill** | Benefits from a cooperating ammunition or weapon assistant |

#### Marksman

| Perk | Tactical change |
|---|---|
| **Patient Solution** | Preserves limited targeting progress through a very brief obstruction |
| **Spotter Pair** | Uses explicitly relayed spotter observations for an initial solution |
| **Counter-Observer** | Recognizes optics and evidence of surveillance |
| **Target Discrimination** | Identifies observable equipment and behavior before firing |
| **Cold Position** | Produces less movement evidence in a prepared position |
| **Displacement Drill** | Leaves after firing more efficiently but abandons the solution |

#### Engineer

| Perk | Tactical change |
|---|---|
| **Hasty Breach** | Faster, louder, less-controlled entry |
| **Surgical Breach** | Slower entry with constrained collateral damage |
| **Remote Initiation** | Connects a prepared charge to a physical trigger |
| **Field Fortification** | Places cover and obstacles where terrain permits |
| **Trap Sense** | Recognizes disturbed terrain, mines, and ritual sites |
| **Render Safe** | Dismantles eligible deployables and discovered ritual traps |

#### Medic

| Perk | Tactical change |
|---|---|
| **Triage** | Rapidly assesses several casualties |
| **Under Fire** | Permits limited exposed stabilization at added cost or reduced reliability |
| **Damage Control** | Treats a defined complication beyond ordinary aid |
| **Conservative Medicine** | Saves supplies when time and safety permit |
| **Casualty Movement** | Coordinates carrying or dragging with less disruption |
| **Return to Duty** | Improves limited function after stabilization without removing wounds |

#### Signaller

| Perk | Tactical change |
|---|---|
| **Burst Discipline** | Trades immediacy for shorter, less exposed transmissions |
| **Frequency Agility** | Reconfigures faster after interference without granting immunity |
| **Cross-Cueing** | Correlates legitimate acoustic, thermal, radio, and magical observations |
| **False Traffic** | Makes decoy emissions resemble plausible network behavior |
| **Drone Shepherd** | Gives drones better pre-disconnection contingencies |
| **Relay Architect** | Predicts relay coverage and weak links |
| **Borrowed-Eye Hunter** | Recognizes evidence of active critter attunement |

#### Leadership

Command-qualified personnel of any class may receive leadership outcomes.
These compete with ordinary milestones rather than forming a second full tree.

| Perk | Tactical change |
|---|---|
| **Clear Intent** | Supplies a richer fallback plan before disconnection |
| **Fire Coordinator** | Establishes confirmed-target or covering-fire instructions |
| **Controlled Succession** | Reduces disruption when command transfers |
| **Emission Discipline** | Sets silent, scheduled, emergency-only, or continuous transmission posture |
| **Steady Withdrawal** | Preserves formation and reporting during disengagement |

### Drones

🟩 Observation and relay drones are units rather than carried actions. They:

- count against force size;
- occupy physical position and elevation;
- carry their own perception and control module;
- depend heavily on the network; and
- remain locally autonomous when jammed, but cannot report through the lost
  link.

## Organized arcane force

### Canonical force shape

🟩 The arcane faction contains **scarce decisive casters and durable nonmagical
mass**.

- Senior casters spend HP, accumulate Strain, maintain or use magical
  infrastructure, command major formations, and participate in cooperative
  rituals.
- Every senior caster normally leads two or three persistent magical
  assistants. Assistants use lesser spells and abilities, prepare and maintain
  magical work, and contribute to rituals without replacing senior-caster
  requirements for major workings.
- Goblins, orcs, trolls, and other non-casters screen casters, hold terrain,
  carry components and anchors, force human expenditure, and keep the faction
  functional after caster loss.
- These peoples are not failed casters or disposable summons.
- Arcane coordination depends on geographical
  [Anchor Capacity](gameplay-magic.md#anchor-capacity), not radio transmission.

### 🟧 Proposed hierarchy

The following organization is a proposal:

```text
small caster circle
        ↓ intent, magic, rituals, and anchors
senior caster cells with 2–3 magical assistants each
        ↓ lesser magic, ritual work, and magical continuity
orc captains and goblin bosses
        ↓ bounded local interpretation
goblin screens, orc formations, and trolls
```

A representative 50–100-unit force may contain only three to six senior
casters, supported by six to eighteen assistants. One senior caster is the
commander; others lead major warbands, work magic, maintain anchors, and
assemble for rituals. Gathering a major ritual quorum therefore concentrates
several scarce leaders and their supporting cells at one exposed site.

Mundane officers execute prior intent and make bounded local decisions. Caster
loss removes magical capability and strategic replanning, but does not
mind-control the surviving horde into passivity.

### Species roles

| People | Status | Proposed role |
|---|---|---|
| 🟪 **Goblins** | 🟧 Proposal | Numerous scouts, skirmishers, sappers, trap layers, handlers, attendants, carriers, and crews |
| 🟪 **Orcs** | 🟧 Proposal | Disciplined shield and spear formations, assault troops, archers, bodyguards, captains, and anchor defenders |
| 🟪 **Trolls** | 🟧 Proposal | Living heavy assets providing assault, mobile cover, obstacle destruction, transport, recovery, and short-ranged siege action |
| 🟪 **Senior casters** | 🟩 Canonical shape | Few leaders and decisive magical specialists whose strain career and ritual commitment shape the force |
| 🟪 **Magical assistants** | 🟩 Canonical shape | Two or three persistent juniors per senior caster, providing lesser spells, ritual contribution, preparation, maintenance, and continuity |

Organized goblin soldiers are distinct from unaffiliated goblins emerging from a
goblin portal. Portal goblins belong to neither side and are hostile to
everyone, including the arcane force.

### Prototype body profiles

These are current fixed-state rules-lab inputs, not accepted unit statistics.
Armor values are directional protection inputs; HP is immediate functional
capacity.

| Body | HP | Front Armor | Flank Armor | Rear Armor | Suppression resistance | Regeneration/second |
|---|---:|---:|---:|---:|---:|---:|
| Goblin skirmisher | 35 | 8 | 4 | 2 | 0.75 | 0 |
| Shielded orc | 100 | 38 | 16 | 10 | 1.25 | 0 |
| Armored troll | 240 | 55 | 38 | 24 | 1.80 | 6 |

🟥 The troll values currently make one frontal rifle unable to outpace
[Regeneration](gameplay-formulas.md#regeneration). This is a measured boundary,
not evidence that the values are fun.

### 🟧 Proposed caster development

| Family | Example outcomes | Purpose |
|---|---|---|
| Authority | Imprinted Intent, Delegated Voice, Many as One, Last Injunction, Cruel Priority | Standing intent, local delegation, and controlled load shedding |
| Anchoring | Standard Bearer, Twin Allegiance, Deep Foundation, Burden Reader, Ordered Collapse | Mobile versus fixed anchors, transfer, diagnosis, and shutdown |
| Strain | Quiet Meditation, Violent Meditation, Blood Geometry, Scarred Channel, Last Reserve | Different recovery, empowerment, and catastrophe decisions |
| Ritual | Ritual Conductor, Redundant Circle, Patient Omen, Bound Trigger, Gatewright | Quorum resilience and ritual-shape choices |
| Perception | Borrowed Eyes, Familiar Territory, Vital Reading, Lingering Impression, Watcher's Patience | Different uses of legitimate arcane observations |

These outcomes must not grant hidden knowledge, remove ritual quorum, erase
Strain risk, or convert anchor capacity into an unconditional bonus.

### 🟧 Proposed progression depth

- Casters receive full character depth.
- Magical assistants receive medium character depth, bounded lesser-spell
  development, injuries, and persistent history.
- Captains, specialists, handlers, and trolls receive medium depth.
- Rank-and-file goblins and orcs retain lighter individual traits,
  proficiencies, scars, and history.
- Warbands may hold persistent collective traits.
- Advancement remains automatic and policy-influenced to avoid managing a
  larger-than-human roster one promotion at a time.

## Sources and deeper rationale

- [Human Forces](human-forces.md)
- [Arcane Civilization Forces](arcane-forces.md)
- [Arcane Horde Progression and Equipment Proposal](research/arcane-horde-progression-and-equipment.md)
- [Attribute, Skill, and Progression Systems](research/progression-systems.md)
- [Fixed-State Rules Laboratory](research/rules-lab-prototype.md)
