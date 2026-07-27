---
title: Combat, Environment, and Command Reference Models
status: proposed
document-type: research
version: "0.4"
last-updated: 2026-07-27
related:
  - docs/game-vision.md
  - docs/tactical-environment-architecture.md
  - docs/combat-resolution.md
reference-models:
  - XCOM 2
  - Xenonauts 2
  - Door Kickers 2
  - Combat Mission
  - Full Spectrum Warrior
  - Frozen Synapse
  - Silent Storm
  - Gladiabots
  - Echoes of the Architects
  - Screeps
  - NEBULOUS: Fleet Command
---

# Combat, Environment, and Command Reference Models

## Purpose

This living document records the games that inform S.I.R.'s design and, for
each, the exact boundary between what S.I.R. adopts and what it rejects.

A reference identifies useful relationships. It does not imply that S.I.R.
copies the referenced game's interface, time control, spatial model, resolution
mathematics, or scale.

S.I.R. deliberately has **no single primary reference**. Its combination of
continuous real time, 50–100 persistent units per side, server-hosted
player-authored control, and simulated command topology does not exist in one
prior game, and different subsystems therefore take direction from different
sources.

## Reference position by system

| System | Primary reference | Secondary |
|---|---|---|
| Map construction and variety | XCOM 2 | — |
| Environmental composition and verticality | XCOM 2 | Xenonauts 2 |
| Destructibility | Xenonauts 2 | Silent Storm *(as caution)* |
| Ballistics and physical shot delivery | Xenonauts 2 | Silent Storm |
| Suppression | Xenonauts 2 | Close Combat |
| Reaction, facing, and awareness | S.I.R.-owned | Xenonauts 2, Door Kickers 2 |
| Engagement timing and precedence | Frozen Synapse | — |
| Doctrine and order vocabulary | Frozen Synapse | Gladiabots |
| Breaching and room clearing | Door Kickers 2 | — |
| Command topology and delegated execution | Combat Mission | Full Spectrum Warrior |
| Local knowledge and relative spotting | Combat Mission | — |
| Simultaneous resolution | Frozen Synapse | Combat Mission |
| Ballistics realism ceiling | Silent Storm | — |
| Player-authored behavior interface | Gladiabots | Echoes of the Architects |
| Server-hosted player code as a service | Screeps | BattleCode |
| Electronic warfare as primary play | NEBULOUS: Fleet Command | — |
| Personnel progression | *see* [progression-systems](progression-systems.md) | — |

## Environment and combat

### XCOM 2

**Adopted.** Map construction through assembly of hand-authored parcels onto
authored plots, producing combinatorial variety without surrendering deliberate
tactical composition. Cover-dense, destructible, multi-level environments in
which position is the scarce resource. Environmental hazards and interactables
as tactical objects.

**Rejected.** XCOM 2's combat resolution is incompatible with established S.I.R.
rules and is not adopted at any level:

- cover is a defence modifier attached to the target, resolved through a single
  to-hit roll against the intended target. S.I.R. resolves physical traces
  against world geometry, so misses, penetration, friendly fire, and crossfire
  are real consequences;
- player units have no facing and perceive in all directions. S.I.R. treats body
  facing and attention direction as distinct authoritative state;
- concealment is a squad-wide binary with a discrete activation moment. S.I.R.
  uses continuous per-unit acquisition with decay, which is strictly more
  expressive and already canonical; and
- squad size of four to six does not inform any scale decision.

**Translation warning.** An XCOM tile is human-sized; a S.I.R. cell is 0.5 m and
a human occupies 2×2. Reference layouts do not transfer at face value. See
[Tactical Environment Architecture](../tactical-environment-architecture.md).

### Xenonauts 2

**Adopted, with a precise boundary.** Xenonauts 2 does roll dice, but the roll
determines **trajectory rather than outcome**. A missed shot does not vanish: it
deviates high, low, or into a neighbouring square, and the deviated round still
exists and still travels. Line of fire is consequently a real geometric
concept — friendly units in the path are highlighted in the shot preview, and
reaction fire is withheld when a friendly stands in the direct line of fire.

Cover is objects along that path rather than a defence number on the target.
Each has a percentile chance to intercept a shot passing over it, community
reported as roughly 40% for low cover and 100% for tall cover. There is no "in
cover" state on a unit; cover is whatever happens to be in the way.

Suppression is a first-class mechanic that degrades effectiveness rather than
dealing damage, triggered by near misses and volume of fire. Overlapping fields
of fire, spacing, and incremental advance are the intended tactical vocabulary.

**Awareness is directional**, which is the part of this reference most easily
overlooked. Vision is roughly a 90-degree cone out to about 18 tiles, blocked by
walls, tall objects, and doors; facing matters, and units rotate to face an
attacker. Reaction fire chance scales with Reflexes, weapon, and **unspent time
units**, so banking TU buys reaction capability. That last mechanic is the
turn-based expression of S.I.R.'s reaction reserve and is shipped evidence that
it works.

**S.I.R. goes further than this reference, deliberately.** S.I.R. resolves
traces geometrically in path order with no probabilistic interception layer:
cover either lies in the path or does not. The ordering of the three models is
XCOM 2, then Xenonauts 2, then S.I.R., and S.I.R. sits at the far end alone.
The cost is real — full tracing with burst weapons at 100 units per side is more
per-shot work than either reference performs, and probabilistic interception
remains the fallback if the performance gate fails, at the price of weakening
the semantic edge model.

**Rejected.** Line of sight in Xenonauts is **mutual**: seeing implies being
seen. S.I.R. must reject this, because asymmetric detection is the entire point
of an acquisition model and symmetric spotting would strip most of the value
from stealth, ambush, flanking, and rear approach.

**Instructive failure.** Destroying a fence with the first round of a burst
still leaves the following rounds striking "an invisible wall where the fence
used to be" — cover evaluated against a stale snapshot for the whole burst.
S.I.R.'s spatial-revision and localized-invalidation rule exists to prevent
exactly this, and it is worth a canonical scenario.

**Not inherited.** Time-unit economy, turn structure, and the geoscape strategic
layer.

### Silent Storm

**Adopted as evidence, not as a target.** Demonstrates real projectile physics
with trajectory, velocity, and penetration, and full structural destructibility
including load-bearing collapse.

**Rejected.** Its degree of destructibility is a documented failure mode as much
as an achievement: when demolition is cheap, tactics collapse into demolition.
S.I.R. bounds destruction by logistics cost, committed time, material class, and
objective consequence, and explicitly defers collapse propagation.

### Door Kickers 2

**Status changed.** Previously the primary qualitative reference for combat,
reaction, facing, and awareness. It is now a **narrow reference for breaching,
room clearing, and close-quarters entry**.

**Retained.** Stack order and assigned entry sectors, door handling, the fatal
funnel, muzzle and body clearance, threshold evaluation, and the relationship
between preparation and survival in short lethal engagements.

**No longer directing.** The awareness and reaction architecture it originally
motivated — geometry, stimulus, accumulated acquisition, timed reaction, and
separate body facing and attention direction — is now S.I.R.-owned canonical
design and stands on its own. It is not revisited when this reference changes.

**Not inherited.** Pause-at-will planning, freeform movement, small-team scale,
and manual per-operator path and facing control.

## Command and execution

This cluster is the closest structural analogue to S.I.R.'s command model and
was historically under-weighted in this document.

### Combat Mission

**Most relevant existing analogue.** Orders are issued during planning, then
executed over a fixed real-time window during which the player cannot intervene.
Units run on their own tactical AI, performing their own spotting, morale
resolution, and return fire. The player watches.

**Adopted as validation.** Two S.I.R. positions are proven in production by this
series:

- delegated execution is playable and satisfying rather than a loss of agency;
  and
- **relative spotting** — each unit maintaining its own knowledge, so a player
  genuinely does not know what an unspotted unit knows — sustains a full game.
  This is S.I.R.'s world-truth / local-knowledge / player-knowledge model.

**Not inherited.** Turn structure, the fixed execution window, and the absence
of player-authored unit behavior.

### Full Spectrum Warrior

**Adopted.** The commander-not-puppeteer thesis. The player never controls a
soldier directly, issuing positional and covering-fire orders to fireteams that
execute bounding overwatch and suppression autonomously. Developed with military
consultation specifically to preserve those behaviors under indirect command.

**Relevant lesson.** Order vocabulary is a design surface in its own right. The
cursor that snaps to cover and communicates the resulting posture is an
information-design solution to the problem S.I.R. faces in exposing doctrine to
a human commander.

### Frozen Synapse

The closest shipped analogue to S.I.R.'s engagement timing, and the source of
several accepted directions.

#### Kill-Time

Frozen Synapse resolves shooting without a to-hit roll. When a unit begins
firing, the target must remain in sight and in range for a required duration —
the Kill-Time — before the shot lands. Combat is a race between two countdowns.

Kill-Time is modified by unit class, range, cover, recent movement, and active
aiming. Snipers have a long base time that barely scales with distance;
shotguns are near-instant but cut off entirely past a short range; machine guns
sit in the middle on both axes. A target in cover takes **longer** to kill, so
cover buys time rather than granting a defence number.

**Adopted.** The duration model itself matches S.I.R.'s reaction and action
lifecycle. More importantly, Frozen Synapse requires the target to remain
observable **for the whole duration**, which S.I.R.'s specification previously
omitted. That requirement is now canonical as sustained targeting in
[Combat Resolution Architecture](../combat-resolution.md), and it is what makes
peeking, breaking contact, obscurants, and door state matter after first
contact rather than only before it.

#### Precedence over stacking

Competing advantages resolve through a strict priority ladder — reported as
`Focus > Cover > Still > Aiming` — rather than by summing modifiers. One
advantage dominates.

**Adopted** as the canonical direction for S.I.R.'s competing reaction
advantages. It is explainable after the fact, cheap at 100 units per side, and
testable by a control module, which an accumulated total is not. It directly
serves the established requirement that strong causal states beat stacks of
small opaque bonuses.

#### Hard-edged focus zones

The Focus Diamond is a declared watch zone whose advantage applies only when the
enemy is **literally inside it** at the moment of spotting. A contact close to
but outside the zone receives only the ordinary aiming bonus. Distance and angle
both bound it, and it outranks every other advantage.

**Adopted** as the leading candidate for S.I.R.'s attended sector: a hard
boundary with a step change rather than a smooth falloff, so that choosing what
to watch is a real commitment.

#### One engagement at a time

Units engage one enemy at a time, and a second target costs the full duration
again. **Adopted** as a throttle. Without it a single prepared unit reacts to an
entire squad simultaneously, which would make defensive positions
disproportionately strong at S.I.R.'s force scale.

#### Movement and stance costs

Kill-Time scales with how fast a unit was moving before firing, and when both
units are moving the one closer to its destination — decelerating — fires
first. Standing up from a duck costs roughly 0.35 seconds, which is about seven
ticks at S.I.R.'s 20 Hz. The deliberate consequence is that Frozen Synapse does
**not** reward ducking behind half cover, because standing up loses the
engagement against someone already upright.

**Adopted.** Reaction readiness scales with recent movement, and stance and
posture transitions cost ticks. A cover posture free to enter and leave would
trivialise exposure.

#### Order vocabulary

Frozen Synapse separates persistent state orders from discrete action orders:

| Kind | Orders |
|---|---|
| State (persistent policy) | `Aim`, `Focus`, `Engage on Sight`, `Continue on Sight`, `Ignore`, `Duck` |
| Action (discrete) | `Move`, `Wait`, `Check`, `Fire` |

This is the working answer to the open question of the smallest doctrine
vocabulary that makes a standard module competent without configuration. The
split maps directly onto S.I.R.: state orders are doctrine and reaction intent,
action orders are action requests.

Three primitives are worth carrying over specifically:

- **Continue on Sight** — move at speed and decline to engage. A legible
  expression of spending reaction readiness to buy movement.
- **Engage and Stop on Sight** — does not improve engagement timing at all. It
  exists only to stop a unit walking away from a target it would otherwise have
  killed. Precisely the class of primitive a doctrine layer should express, and
  one unlikely to be derived from first principles.
- **Ignore** — move from A to B without stopping to engage. The doctrine-level
  counterpart to the commander deciding acceptable risk.

**Rejected.** Frozen Synapse's dark mode retains a foe's last known position and
time since last seen. S.I.R.'s canonical client shows nothing for a hostile it
can no longer observe. The divergence is deliberate, but it is a divergence from
a game that found the marker valuable, and it should be revisited if playtesting
shows the canonical client is too information-poor.

**Not inherited.** Five-second resolution windows, plan re-simulation against
hypothetical enemy plans before commitment, instant death without HP or wound
states, and abstract geometry.

## Player-authored control

### Gladiabots

**Adopted.** A visual node-based editor of conditions and actions through which
a player builds squad behavior and then observes it execute, requiring no
programming skill. This is the model for S.I.R.'s doctrine layer, which
`realtime-turnbased-tactical-features.md` requires not become programming
homework.

**Boundary.** S.I.R.'s standard module and doctrine controls must be competent
by default. Authoring is an option for depth, not an entry requirement.

### Echoes of the Architects

**Closest commercial precedent to S.I.R.'s premise.** From the Gladiabots
developer, released July 2025: a real-time strategy game with modular
programmable units and visual behavior logic, explicitly designed to move the
genre away from actions per minute and toward unit design and adaptation.

Worth continuous observation as competitive research rather than inspiration.
Its reception is direct evidence about whether a delegated-control strategy game
finds an audience.

### Screeps

**Adopted as operational precedent.** Players upload real code that executes
server-side in a persistent shared world, with WASM transpilation supported. It
is the existing proof that S.I.R.'s central operational risk — untrusted
player code as a live service — is survivable, and a source of evidence on
metering, abuse, and fairness.

### BattleCode

**Relevant.** An RTS for which contestants write an autonomous player,
explicitly concerned with pathfinding, distributed algorithms, and
bandwidth-limited communication between a player's own robots. S.I.R.'s
communication-constrained module messaging is that constraint expressed as a
game mechanic rather than a competition rule.

## Information and electronic warfare

### NEBULOUS: Fleet Command

**Adopted as proof of concept.** Radar and passive detection, occlusion and
sensor shadows, signature management, and jamming that creates degraded volumes
in an opponent's sensor picture. Ships can run cool, trading capability for
concealment.

This is the shipped demonstration that electronic warfare can be primary
gameplay rather than a damage modifier, which is what
[the game vision](../game-vision.md) requires of S.I.R.'s EW model.

**Not inherited.** Three-dimensional continuous space, fleet scale, and its
detection mathematics.

## Open questions for prototyping

- How wide should strong forward awareness be on an eight-direction grid?
- Should side and rear sectors impose detection delay, recognition delay,
  reaction delay, or different combinations?
- When does rear position grant an advantage independently of unawareness, and
  when does the combination permit an execution?
- What cover density at 0.5-metre cell scale reproduces the intended
  cover-anchored grammar without becoming visual noise at 100 units per side?
- How many discrete levels can be presented legibly at full force scale?
- What is the smallest doctrine vocabulary that makes the standard module
  competent without configuration?

## Sources

- [XCOM 2 — cover mechanics discussion](https://steamcommunity.com/app/268500/discussions/0/412446292773670210/)
- [A Deep Dive Into XCOM and XCOM 2 — Game Developer](https://www.gamedeveloper.com/design/a-deep-dive-into-xcom-and-xcom-2)
- [Xenonauts 2 — official beginner's guide](https://wiki.hoodedhorse.com/Xenonauts_2/Beginner's_Guide)
- [Xenonauts 2 review — NGOHQ](https://www.ngohq.com/2026/04/03/xenonauts-2-review/)
- [Silent Storm review — GameSpot](https://www.gamespot.com/reviews/silent-storm-review/1900-6087043/)
- [Door Kickers 2 — official game page](https://inthekillhouse.com/doorkickers2/)
- [KillHouse Games — line-of-sight-based coordination note](https://inthekillhouse.com/door-kickers-2-delayed/)
- [Combat Mission — Wikipedia](https://en.wikipedia.org/wiki/Combat_Mission)
- [Combat Mission series guide — Wargamer](https://www.wargamer.com/combat-mission/games)
- [Full Spectrum Warrior — Wikipedia](https://en.wikipedia.org/wiki/Full_Spectrum_Warrior)
- [Full Spectrum Warrior review — GameSpot](https://www.gamespot.com/reviews/full-spectrum-warrior-review/1900-6121041/)
- [Frozen Synapse — Wikipedia](https://en.wikipedia.org/wiki/Frozen_Synapse)
- [GLADIABOTS — AI Combat Arena on Steam](https://store.steampowered.com/app/871930/GLADIABOTS__AI_Combat_Arena/)
- [Echoes of the Architects on Steam](https://store.steampowered.com/app/3136490/Echoes_of_the_Architects/)
- [Screeps](https://store.screeps.com/)
- [Battlecode](https://battlecode.org/)
- [NEBULOUS: Fleet Command — Electronic Warfare wiki](https://wiki.hoodedhorse.com/NEBULOUS_Fleet_Command/Electronic_Warfare)
- [Mars Tactics on Steam](https://store.steampowered.com/app/1727760/Mars_Tactics/)
