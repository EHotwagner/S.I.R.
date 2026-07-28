---
title: S.I.R. Tactical Environment Architecture
category: Design
categoryindex: 4
index: 17
status: proposed
document-type: living-design
version: "0.1"
last-updated: 2026-07-27
related:
  - docs/game-vision.md
  - docs/combat-resolution.md
  - docs/technology-stack.md
  - docs/skirmish-development-plan.md
  - docs/research/combat-awareness-models.md
---

# S.I.R. Tactical Environment Architecture

## Purpose

This document defines what a S.I.R. battlefield contains, how one is
constructed, and which environmental properties are authoritative. It is the
architecture behind the accepted shift toward an XCOM 2-style tactical
environment.

It does not change combat resolution. Physical shot traces, cover as geometry,
semantic cell edges, body facing and attention direction, and the progressive
acquisition pipeline remain canonical.

## Reference position

The environment and spatial grammar follow XCOM 2 and Xenonauts 2. Combat
resolution follows S.I.R.'s established physical model, for which Xenonauts is
the closer reference.

Door Kickers 2 is no longer the primary combat reference. It remains a narrow
reference for breaching, room clearing, and close-quarters entry behavior.

The full per-system reference position is maintained in
[Combat, Environment, and Command Reference Models](research/combat-awareness-models.md).

## Map construction

### Assembly, not generation

XCOM 2's environmental strength is not procedural generation. It is
**assembly of hand-authored pieces**: authored parcels are placed onto authored
plots with defined connectivity, producing combinatorial variety while every
individual piece retains deliberate tactical composition.

This distinction is the canonical direction. Purely procedural generators
produce connectivity, not tactical intent. A cellular-automata cave or a BSP
room graph can guarantee that a map is traversable and that no region is
stranded; it cannot guarantee that cover is placed where an approach needs to be
contested, that a flank exists but costs time, or that a defender has a
defensible interior without an unbeatable one.

The canonical model is therefore:

```text
authored plot
  → parcel slots with declared connectivity and role
      → authored parcels selected per slot
          → seeded variation within a parcel
              → deterministic assembly
                  → validation gate
                      → immutable map instance
```

Procedural generation remains useful for terrain fill, natural and portal-space
regions, rubble distribution, clutter, and other space where authored tactical
composition is not required.

### Determinism

Map assembly is authoritative content, not runtime decoration. An assembled map
resolves to an immutable instance identified by content hash, and that hash
enters the match record alongside the ruleset and execution profile. Two servers
given the same plot, parcel set, and seed must produce byte-identical maps, and
a replay must reconstruct the exact map rather than re-running assembly against
a possibly changed parcel library.

Assembly runs before the match. It is not part of the deterministic tick
pipeline.

## Cell-scale translation

XCOM 2's grammar does not transfer at a one-to-one tile ratio, and this is the
most consequential authoring consideration.

An XCOM 2 tile is roughly human-sized: one soldier occupies one tile, and cover
is a property of that tile's edges. S.I.R. uses 0.5-metre cells with a 2×2
footprint for a typical human. One XCOM cover position therefore corresponds to
roughly a 3×3 region of S.I.R. cells, and an XCOM parcel translated at face
value would be drastically cover-sparse at S.I.R. scale.

Consequences:

- cover density must be authored against S.I.R.'s cell size, not inherited from
  a reference layout;
- several units can occupy what a reference game would treat as one cover slot,
  so cover is a contested spatial resource rather than a per-unit attachment
  point;
- a footprint can be partially exposed, because a 2×2 unit can have some cells
  behind a wall edge and some in the open — an expressiveness the reference
  model does not have and should not be flattened away; and
- "in cover" is not an authoritative unit state. It is the derived result of
  geometry between an attacker and a target at the moment a trace is evaluated.

## Cover-anchored spatial grammar

What transfers from XCOM 2 is not its cover *mechanic* but its cover
*composition*: battlefields built so that position is the scarce resource.

The canonical intent is that maps make lines of fire the contested asset.
Authored space should produce:

- hard points worth holding and worth taking;
- approaches that are covered, costly, or both;
- flanking routes that exist but trade time or exposure;
- lanes whose control denies movement without requiring occupation;
- interior space where engagement ranges collapse and edges dominate; and
- enough destructible material that a static stalemate can be broken by
  expenditure rather than only by maneuver.

Because S.I.R. resolves shots as physical traces, a lane is a real geometric
fact rather than a designer annotation. Cover placement defines which lanes
exist; moving between hard points means crossing them.

## Verticality

Multi-level terrain is an accepted requirement of the XCOM 2-style environment.
S.I.R. currently has no elevation model, so this is a genuine architectural
addition rather than a content decision.

### Canonical direction

Elevation uses **discrete levels**, not continuous height. Each level carries
its own cell grid and its own semantic edge layer, using the established
addressing.

The boundary between vertically adjacent cells is a **horizontal edge** and uses
the same per-modality permeability contract as a vertical one. This keeps one
spatial model:

- a solid floor is a horizontal edge blocking movement, sight, and traces;
- an open stairwell, hatch, or hole is a horizontal edge permitting some subset;
- a catwalk grating may block movement while permitting sight and fire; and
- destroying a floor is breaching a horizontal edge, producing the same route
  and firing-line consequences as breaching a wall.

Inter-level movement is an explicit capability using declared connection
features — stairs, ladders, ramps, and drops — each with its own timing, cost,
exposure, and interruption rules. It is not free adjacency.

### Consequences to resolve

Verticality touches nearly every spatial system and its cost should not be
understated:

- line of sight becomes a three-dimensional query, and the existing 2D
  visibility caching strategy must be re-evaluated against level count;
- height advantage, downward and upward fire, and sight over intervening cover
  need explicit rules;
- footprint reservation and cooperative movement extend across level
  transitions;
- falling, forced displacement, and structural collapse need consequence rules;
- the canonical client must present multiple levels legibly at 100 units per
  side, which is a harder information-design problem than the terrain itself;
  and
- pathfinding gains a dimension at exactly the scale where its cost is already
  unmeasured.

The number of supported levels should be bounded and declared per map. Starting
with two levels plus roof access is sufficient to prove the architecture and
avoids committing to arbitrary depth before the cost is measured.

## Destructibility

Destruction spans both spatial layers:

- **cell terrain** can be damaged, destroyed, or reduced to rubble that may
  itself become cover or difficult ground; and
- **edge features** can be breached, broken, blown open, or removed, converting
  a blocking boundary into a route and a firing line.

Every destruction event advances the spatial revision and invalidates only the
dependent cached queries.

### Bounding the Silent Storm failure

Fully destructible environments have a known failure mode: when demolition is
cheap, tactics collapse into demolition. Silent Storm is the standing example —
technically remarkable, and routinely reduced to levelling the building.

Destruction must therefore be bounded by cost rather than by arbitrary
indestructibility:

- demolition consumes ammunition, explosives, charges, or magical resources that
  compete with other logistics under the established capacity model;
- heavy breaching takes committed time under the action lifecycle and is
  interruptible;
- structural rules make some material genuinely resistant rather than merely
  expensive;
- destruction is observable and creates its own signature and stimulus; and
- objectives can penalize collateral destruction, making demolition a decision
  with a cost outside the firefight.

Structural collapse propagation is attractive and dangerous. It is deferred: the
first implementation destroys what is targeted and its declared dependents, not
an emergent physics cascade.

## Environmental interaction

The environment may contain interactable features — doors, windows, shutters,
ladders, hazards, volatile containers, deployable and movable cover, and
objective fixtures.

### The machine-readability filter

An environmental affordance is admissible only if a control module can discover
and evaluate it. Concretely, every interactable must:

1. expose a versioned, machine-readable capability descriptor like any other
   action;
2. be discoverable through the knowledge-filtered observation model, so a module
   learns about it the same way it learns about anything else;
3. be evaluable within an ordinary fuel and host-service budget; and
4. produce an observable, explainable result.

A feature that only a human studying a paused board can notice and exploit is
not admissible. It would either be ignored by every module or force the human
back into per-unit micro-control, and both outcomes contradict the command
model.

This filter is the main reason the environment reference cannot be adopted
wholesale. Several classic tactical-game affordances are human-facing puzzles
rather than delegable decisions, and they must be rebuilt as declared
capabilities or left out.

## Map validation

Procedural assembly, destructibility, and asymmetric forces make automated map
quality assurance mandatory rather than optional. A parcel or assembled map is
accepted only when it passes declared checks.

Candidate gates:

- full connectivity for every supported footprint size and movement profile;
- no stranded regions and no unintended one-way spaces;
- articulation points and choke points identified and within declared bounds;
- cover distribution within a target density band at S.I.R. cell scale;
- exposure analysis identifying unintentionally dominant positions;
- kill-zone length distribution within declared limits;
- deployment-zone fairness and spacing for symmetric modes;
- objective reachability under the mission's time limit; and
- level connectivity and inter-level route redundancy.

`FS.GG.Game.Core.MapAnalysis` already provides much of this shape — including
`coverMap`, `exposureMap`, `killzones`, `fairness`, `spacing`,
`articulationPoints`, `isConnected`, and a `validate`/`Rule`/`Report` structure.
S.I.R. supplies its own definitions of cover and line of sight, since those
depend on the semantic edge layer and square footprints, but the analysis and
reporting structure is directly reusable.

Validation runs offline as a content gate and in tests. It is not a runtime
cost.

## Readability at scale

The reference games author environments for four to eight units, with a camera
the player can move at leisure while time is stopped. S.I.R. presents 50 to 100
units per side in continuous real time.

Every environmental feature must therefore pass a readability test as well as a
tactical one: can the commander understand the battlefield's relevant state
while it is moving, and can they reconstruct afterwards why a decisive event
occurred? This is the same constraint the design already applies to combat
modifiers, extended to terrain.

Practical consequences:

- environmental state that changes tactical truth — door state, breached walls,
  destroyed cover, level connectivity — must be legible at normal gameplay zoom;
- cover and exposure are candidates for player-toggled tactical overlays rather
  than permanent display;
- visual density must not grow with unit count; and
- a feature that is only comprehensible when the player zooms to one squad is a
  feature the commander will not use.

## Deferred

- structural collapse propagation and load-bearing simulation;
- continuous elevation or slope;
- more than a small bounded number of levels;
- deformable terrain outside declared destruction rules;
- weather, lighting, and time-of-day as authoritative tactical state; and
- runtime map generation during a live match.

## Open parameters

- Plot and parcel dimensions, and the parcel slot connectivity contract.
- Target cover density band at 0.5-metre cell scale.
- Supported level count and the inter-level LOS rule.
- Inter-level movement timings, costs, and interruption rules.
- Destruction cost schedule, material classes, and rubble behavior.
- Fall damage, displacement, and collapse consequences.
- The interactable capability catalog and its observation contract.
- Map validation thresholds and which gates are blocking.
- Whether portal-space environments use a different construction model from
  Earth-side ones.
- Authoring toolchain and parcel content format.
