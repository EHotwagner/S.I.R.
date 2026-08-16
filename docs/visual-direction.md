---
title: Visual Language
category: Forces & Equipment
categoryindex: 3
index: 13
status: proposed
document-type: living-vision
version: 0.11
last-updated: 2026-08-16
related:
  - docs/game-vision.md
reference-assets:
  - docs/assets/concept-art/unit-footprints-and-information-faces.png
---

# Visual Direction

## Purpose

This document captures the intended graphical language of S.I.R. It describes
the direction communicated by concept art without treating every depicted label,
symbol, proportion, or interface element as a finalized gameplay rule.

## Primary concept

![Concept art showing authoritative square unit footprints and information faces](assets/concept-art/unit-footprints-and-information-faces.png)

**Source:** Concept art supplied by the project owner on 2026-07-25.

## Established direction

The concept presents the battlefield as a readable, isometric tactical space
with a grounded near-future aesthetic. Units are represented as physical square
or rectangular prisms whose bases correspond directly to their authoritative
grid footprints. The unit representation is therefore simultaneously a game
piece, a spatial occupancy indicator, and an information surface.

The box-shaped unit body is the intended final visual abstraction. It is not a
placeholder for a conventional animated character or vehicle model. Different
unit categories can vary in footprint, height, proportions, surface treatment,
and information design while remaining within this shared box-based language.

The top face of a unit communicates important identity and state at a glance.
At normal gameplay zoom, the unit's identification glyph and current hit points
must remain readable. The concept demonstrates a wider visual hierarchy that
can include:

- role or class glyph;
- faction color and frame;
- unit name or type;
- hit points or condition;
- stance through the height and proportions of the piece; and
- compact status markers.

Whether these additional elements must remain visible or readable at normal
zoom will be determined through visual prototypes and playtesting.

Facing and attention are separate concepts. Facing is shown with a small
directional indicator close to the base, while attention or observation is
visualized as a cone or sector projected onto the ground.

Thin structures are authoritative edge features occupying cell boundaries rather
than whole cells. Walls, windows, doors, fences, handrails, and low walls are
therefore drawn on the grid lines between cells, and their state must be
readable: an open door and a closed door look different because they are
tactically different while occupying no floor area.

## Battlefield presentation

The environment uses a three-dimensional isometric presentation with a visible
square grid integrated into the terrain. Architecture and props remain
recognizable and materially grounded, while unit pieces and tactical overlays
use stronger color, shape, and contrast.

The current concept combines:

- worn stone, masonry, water, barriers, handrails, and modern equipment;
- stylized but materially readable lighting and surfaces;
- strong blue, red, and green faction or ownership accents;
- luminous footprint outlines;
- translucent ground overlays for attention or sensor coverage; and
- compact iconography designed to remain readable at tactical camera distance.

This contrast allows the battlefield to feel like a place while the active
tactical state remains legible.

## Production tactical visual system

The shared workscreen implements `tactical-visual-system-v1`. Its renderer-level
registry is the source for terrain, edge, unit, intent, effect, stroke, radius,
motion, density, and layer tokens; component-local color choices are not a
second vocabulary. The default production palette uses canvas `#10161d`, text
`#f7f9fa`, grid `#71808b`, human `#53b7ff`, arcane `#d792ff`, neutral
`#ffd166`, active health/impact `#ff6b6b`, and focus `#ffffff`. High-contrast
and monochrome-pattern routes retain the same semantic roles with their own
tokens, shapes, and patterns.

Materials are deliberately flat and inspectable in the SVG: open, rough,
blocked, and objective terrain use distinct restrained fills; walls, doors, and
windows use semantic edge strokes; unit bodies keep the box-piece silhouette
with a strong faction frame, top glyph, health segments, facing mark, and stance
label. Lighting is expressed by local contrast and ordered faces rather than
glow or translucency accumulation. Typography stays system-sans for prose and
compact labels, with weight and outline carrying hierarchy over size alone.

The exact production layer order is:

`terrain → edges → routes → units → effects → selection → tactical overlays → annotations`

Effects therefore explain causality around a piece without covering its
selection, exact analytical geometry, or final annotation. Every effect has a
stable primitive/event/tick identity and is derived only from disclosed facts.
Its source or target endpoint is omitted when that unit is not in the disclosed
projection. Preview, accepted, rejected, committed, and historical states use
named kinds and structure as well as color; the renderer never invents an
event, endpoint, count, or outcome.

### Motion and effect grammar

| Category | Production treatment | Full motion | Reduced motion |
|---|---|---:|---:|
| Unit movement/facing | projection-only transform/opacity transition | 160 ms | 1 ms |
| Attack or signal | non-scaling trace between disclosed endpoints | 420 ms emphasis | 120 ms opacity emphasis |
| Impact/suppression/recovery | bounded ring/mark at disclosed consequence | one scale-and-fade | no spatial scale |
| Selection/focus | persistent high-contrast outline | state transition only | immediate outline |
| Replay seek/step | reconstruct effects from the target committed frame | reversible | identical causal mark |

Effects are one-shot, pointer-inert SVG primitives capped at 256 active
instances. Stress density strengthens traces instead of adding particles.
Animation never changes the committed coordinate stored in the projection and
never delays the exact current state.

### Semantic zoom and density

Ordinary (up to 40 units), dense (41–100), and stress (over 100) scenes use the
same grammar. Density removes decoration before tactical truth: footprint,
faction, glyph/role, facing, selection, health, immediate intent, and decisive
effects remain; supporting labels yield to semantic zoom and focused
inspection. The existing 24/48-pixel semantic thresholds and ten-percent
hysteresis remain authoritative for overview/standard/detailed transitions.
At narrow widths and 400% browser zoom the retained workscreen and native
controls reflow without changing disclosure or layer ownership.

Deterministic ordinary/dense/stress prototypes and an exact production-bundle
before/after capture are recorded in
[the tactical visual review](assets/tactical-visual-system-review/README.md).

## Unit-as-interface principle

The unit representation should communicate as much essential information as
possible directly on or immediately around the unit. This reduces dependence on
a detached HUD and makes large-force state easier to scan spatially.

The concept separates information by surface:

| Surface | Intended information |
|---|---|
| Top face | Identity, role, faction, condition, and compact status |
| Body height | Stance or vertical profile |
| Base outline | Authoritative footprint and allegiance |
| Base indicator | Facing |
| Ground overlay | Attention, observation, targeting, or other spatial influence |

This mapping is a design direction, not yet a final UI contract.

## Scale expression

Different entities may use different square footprint sizes while retaining the
same overall graphical language. The concept depicts individual units, support
or cargo objects, a large monster, and a vehicle as related game pieces of
different footprints and heights.

This supports the vision that:

- the base is authoritative for spatial occupation;
- stance can change a unit's visible height without changing its identity;
- large entities remain immediately distinguishable;
- role and state remain readable without inspecting a separate panel; and
- the player can scan a battlefield containing 50–100 units per side.

## Tactical overlays

Overlays should explain transient or selected tactical state without permanently
covering the environment. The concept suggests overlays for:

- unit footprint;
- facing;
- attention or observation sector;
- faction or control;
- suppression;
- command state;
- normal or active state; and
- special state.

These examples establish a visual vocabulary but do not yet establish the final
set of statuses or their exact colors.

Overlay visibility must be customizable by the human player and client. Players
should be able to show or hide distinct tactical information according to their
current task rather than having line of sight, sensors, communications,
electronic warfare, and other fields permanently displayed together.

Line-of-sight overlays for **selected** units should show exact visible
geometry rather than an approximate glow or radius. Because positioning is the
decisive tactical language, and edges, corners, doors, and levels determine
what a unit can see, an imprecise overlay would misrepresent the single most
important fact on the battlefield. A commander deciding where to place a
support weapon needs to know precisely what that position covers.

Exactness applies to selection, not to the whole force. Rendering exact geometry
for 100 units at once would be unreadable, so the unselected force uses compact
indicators and the exact projection is a property of attention.

Hotkeys for quickly toggling individual overlays are a candidate interaction,
but the exact controls, defaults, combinations, and persistence of overlay
preferences require prototyping and testing.

It is not yet decided whether selection or gameplay context may cause relevant
overlays to appear automatically. Both fully explicit control and contextual
display should remain available for evaluation in client prototypes.

## Color and accessibility

Color is a valid standalone category for communicating faction, status, or
tactical information. The visual language does not require every color-coded
distinction to be repeated through a glyph, pattern, label, or other redundant
channel.

Accessibility should instead be supported through customizable color schemes.
The canonical client may provide alternative palettes and user-defined color
configuration, with the exact scope and presets determined through testing.

## Information certainty

The canonical presentation does not currently classify information as certain,
uncertain, stale, or indirectly reported. Information received by the player is
presented as information; deciding how reliable or conclusive it is belongs to
the human player or client.

Future research, intelligence, or data-analysis capabilities may provide
additional information from which a player or client can draw stronger
conclusions. This does not presently require a universal visual certainty
category.

## Lost hostile contact

When a previously observed hostile unit is no longer observable through the
information currently available to the player, the canonical client shows
nothing for that unit. It does not leave a last-known-position marker, ghost,
faded unit, or uncertainty indicator.

## Relationship to gameplay

The graphical approach reinforces several core design goals:

- **Consequential positioning:** footprints, facing, and observation are visible
  spatial facts.
- **Non-omniscient command:** attention and sensor information can be displayed
  only when it is known to the observing player.
- **Automated micro-control:** unit state and intent can be monitored at scale
  without following every action individually.
- **Large forces:** glyphs, silhouettes, colors, and top-face information support
  rapid recognition across many units.
- **Modern fantasy integration:** contemporary military roles, vehicles, supply,
  monsters, and magic share one coherent tactical presentation.

## Provisional elements

The following details appear in the concept but are not yet confirmed as
authoritative design facts:

- the exact role list, including rifleman, medic, mage, scout, and vehicle;
- the precise meaning of displayed numbers and tier markers;
- which information beyond identification glyph and hit points remains readable
  at normal gameplay zoom;
- exact faction colors;
- exact status icon colors and meanings;
- whether crouched and prone stances change only height or also footprint;
- whether attention is always a cone rather than another field shape;
- whether top-face information is always visible;
- the final camera angle, zoom range, and environmental rendering style.

## Questions for later visual design

1. When should tactical overlays appear automatically, on selection, or only on
   request?
