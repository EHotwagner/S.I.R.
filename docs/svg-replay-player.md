---
title: SVG Replay Player Contract and Visual Catalog
status: accepted
decision-status: canonical
document-type: living-architecture
category: Design
categoryindex: 4
index: 45
version: "1.0"
last-updated: 2026-07-29
description: Durable boundaries, disclosure-safe render contract, initial element inventory, glyph coverage, and palettes for the documentation SVG replay player.
related:
  - docs/2026-07-29-0200-svg-replay-simulation-player-design-report.md
  - docs/fable-client-and-documentation.md
  - docs/visual-direction.md
  - docs/gameplay-units.md
---

# SVG Replay Player Contract and Visual Catalog

## Accepted decision

S.I.R. has accepted the durable decisions from the timestamped
[SVG replay and simulation player design report](2026-07-29-0200-svg-replay-simulation-player-design-report.md).
The documentation player is a flat, top-down SVG tactical table. It draws only
a committed, bounded, disclosure-preserving `RenderFrame`; it does not read
authoritative simulation records, infer state from pixels, retain a lost
contact, or treat interpolation as evidence.

An authoritative footprint and a square information symbol remain separate
geometries. The square's fixed channels are faction outline, exact-class
glyph, health track, body-facing perimeter mark, optional centre-out secondary
heading, elevation stack, and disclosed close-zoom details. Attention and
line-of-sight geometry remain ground overlays and never reuse a heading mark.

## Renderer boundary

The SVG renderer belongs only to generated documentation, interactive
explanations, replay inspection, and non-authoritative simulation evidence.
The production game client remains a separate Babylon.js renderer. It shares
game facts and disclosure rules, but it is not required to use SVG, a top-down
camera, the square-symbol geometry, DOM structure, semantic zoom thresholds,
or documentation palette tokens. Conversely, the documentation renderer may
not import Babylon scene state or become a compatibility test for production
pixels.

The shared contract stops at typed, legitimately disclosed presentation facts:
entity identity, footprint, exact class, faction, condition, headings, level,
stance, status, edges, events, and exact overlay geometry. Each renderer owns
its own safe mapping from those facts to its presentation technology.

## Typed contract and disclosure

The executable contract lives in `SIR.Client`:

- `UnitVisual` separates footprint from the fixed square glyph facts.
- `RenderFrame` is independently drawable at one committed tick.
- `Disclosure<'T>` distinguishes `NotPresent`, `NotApplicable`,
  `ExplicitlyUnknown`, and `Disclosed value`. No optional presentation field
  obtains a zero, empty string, ground level, default heading, or stale value
  merely because the worker omitted it.
- constrained `CellExtent`, `HeadingRadians`, and `UnitClassId` values reject
  invalid extents, non-finite headings, and replay-supplied catalog geometry.
- `RenderFrameTransport` converts the contract to records containing only
  scalar values, options, nested scalar records, and arrays suitable for the
  browser structured-clone boundary. Every disclosure value travels as a
  validated tag/value pair.

Unknown class text resolves to `unknown-unit`. It never becomes SVG markup,
an element identifier, a URL, or a CSS value.

## Initial unit coverage

This inventory covers every documentation-visible unit class currently named
by the accepted or proposed gameplay corpus. A vehicle has no accepted exact
class yet; if a replay exposes one before its class is cataloged, it visibly
uses the safe placeholder rather than guessing a generic vehicle identity.

| Gameplay element | Catalog ID | Decision |
|---|---|---|
| Rifleman | `rifleman` | primary glyph |
| Gunner | `gunner` | primary glyph |
| Marksman | `marksman` | primary glyph |
| Engineer | `engineer` | primary glyph |
| Medic | `medic` | primary glyph |
| Signaller | `signaller` | primary glyph |
| Observation drone | `observation-drone` | primary glyph; distinct lens centre |
| Relay drone | `relay-drone` | primary glyph; distinct mast centre |
| Goblin | `goblin` | primary glyph |
| Orc | `orc` | primary glyph |
| Troll / current large creature | `troll` | primary glyph over its separate footprint |
| Senior caster | `senior-caster` | primary glyph |
| Magical assistant | `magical-assistant` | primary glyph |
| Ambient critter | `ambient-critter` | primary glyph when legitimately disclosed |
| Unsupported or future exact class | `unknown-unit` | visible diamond-and-hook placeholder |
| Defeated unit, wreck, or remains | none yet | render only when authoritative persistence rules supply an exact class/state |
| Vehicle | none yet | provisional concept; placeholder until an exact gameplay class is accepted |

Every catalog entry has a stable semantic ID, normalized 24-by-24 monochrome
geometry, a human-readable description, and a text alternative. Geometry is
built into the client and replay input can select only a known ID.

## Initial tactical element coverage

| Group | Elements | Presentation decision |
|---|---|---|
| Board | background, out-of-board mask, terrain fills, level contours, grid | dedicated deterministic layers |
| Objectives | objective area, location, disclosed state/progress | ground decal/area and inspector; absent when not communicated |
| Semantic edges | wall, low wall, door, window, fence, rail | edge geometry with state-specific form; no color-only state |
| Unit ground facts | occupied footprint, selection, hover, focus | exact outline and outside brackets/halo |
| Unit symbol | faction, class, health, body facing, secondary heading, elevation, stance, status, identity | fixed square channels; only disclosed fields |
| Movement | route, reservation, destination, formation station | toggleable ground overlays |
| Perception | exact visible polygon, attention sector/polygon, sensor coverage | selected-unit default; exact supplied geometry only |
| Command | communication link, delivery delay, referent, objective belief | toggleable links/areas; knowledge-filtered |
| Combat | engagement, target, trace, effect area | toggleable paths/areas or committed transient event |
| Logistics | ownership, route, reservation, transfer | toggleable pattern/dash overlays |
| Diagnostics | coordinates, entity IDs, checkpoints, projection hashes | explicit diagnostic layer; never default disclosure |
| Timeline | disclosed events, checkpoints, bookmarks, communications, actions, divergence | semantic HTML/SVG timeline outside battlefield authority |

Not-yet-specified terrain materials, edge states, statuses, stances, vehicles,
and remains are coverage gaps in the gameplay schema, not permission to invent
visual meaning. Each new exact value requires a catalog/mapping decision.

## Palette tokens

`ReplayPalettes` defines three named token sets:

- `accessible-default` uses a dark field, light text and grid, distinct
  faction hues, and pattern-capable overlays.
- `high-contrast` uses black, white, cyan, magenta, yellow, and a distinct
  focus token with stronger overlay patterns.
- `monochrome-pattern` makes faction strokes chromatically identical and
  carries overlapping distinctions through horizontal, diagonal, vertical,
  and crosshatch patterns.

All three provide explicit canvas, terrain, grid, text, faction, active and
depleted health, focus, and overlay-pattern tokens. Glyph geometry never
depends on faction color, depleted health differs in value as well as hue, and
focus remains independent of hover.

## Sibling symbology decision

`FS.GG.Rendering` is a semantic reference only for this implementation. S.I.R.
adopts its fixed-channel discipline, deterministic mapping, coverage audit,
legibility review, and distinct primary/secondary heading meanings.

It is not a runtime or build-time dependency: the sibling package emits
`FS.GG.UI.Scene`, while this host emits browser SVG and must not pretend those
trees are interchangeable. A future dependency requires a separately reviewed
cross-repository contract that exposes technology-neutral catalog or channel
data and demonstrably removes duplication without coupling SVG to `Scene`.

## Replay transport

Worker protocol 3 carries one bounded inspection projection at an exact
committed tick. Full replay projections contain the minimal kernel slice's
board, one-cell unit occupancy, side, bounded health, semantic edges, disclosed
input events and checkpoint hashes. The client adapts those facts to
`RenderFrame` without inventing class, heading, elevation, stance, overlay, or
event-endpoint data that the projection did not supply.

Perspective replay format 1 supplies only committed ticks and projection
hashes. Its player therefore seeks and labels those committed frames
deterministically but draws no units or edges. Rendering a prior full-replay
unit from that hash-only source would be a disclosure violation.

Every load, advance, seek, event jump, and cancellation carries an operation
identity. A replacement operation cancels its predecessor; responses with any
other identity are ignored. Progress preserves the same identity until the
terminal response, and the shell commits the projection's tick rather than a
requested or estimated tick.

Reverse/forward step, range seek, previous/next disclosed event, and checkpoint
markers all use the same correlated seek path. When a projection omits a
previously disclosed unit or event, the shell removes its inspection selection
and the battlefield removes its SVG selection and roving focus before render.
Consequently no unit geometry, accessible name, event control, selection,
focus, or pointer hit target survives lost contact.

## Overlay and playback policy

`RenderFrame` carries only typed coordinate arrays; replay input never supplies
SVG path syntax. The battlefield validates finite, bounded coordinate pairs
before rendering. Selected-unit geometry is shown only while its disclosed
owner is selected. It stays exact through 1,999 path segments; geometry above
the 2,000-segment review threshold is deterministically simplified and labeled
as such. Whole-force geometry has an independent combined budget: at more than
8,000 path segments it is replaced by bounded aggregate contours or declined.
Whole-force aggregation never consumes or degrades the selected-unit budget.

The current worker projection does not contain authoritative overlay geometry,
so production full and perspective replay adaptation emits none. The Phase 3
visual board uses an explicit sandbox fixture to review exact geometry without
weakening that boundary.

Playback translation may interpolate between adjacent disclosed positions
only. Spawn, disappearance, level/footprint change, and moves longer than one
cell are discontinuities. Health, status, facing, overlays, and events remain
on the earlier committed frame until the next boundary. Alpha derives from the
replay presentation pulse, not CSS/SMIL or an untracked animation clock.
Exact-tick and reduced-motion settings bypass interpolation. At alpha one the
interpolated path returns the same pure scene as direct rendering of the
committed frame.

Timeline events are grouped into disclosed-event, unit-action, and
communication lanes. Action traces require both participants to be disclosed
and present; otherwise no trace or hidden endpoint remains in SVG or accessible
HTML.

`SecondaryHeadingVisual` accepts only the typed `WeaponHeading` or
`SensorHeading` sources. Transport requires the source tag and finite angle
together. Attention and body facing cannot inhabit this channel. The current
worker projection exposes no accepted second-heading source, so production
replay adaptation omits it; the sandbox visual review deliberately supplies
one weapon and one sensor example.

## Simulation comparison and evidence export

The scenario laboratory keeps the default result as an immutable baseline.
The first typed edit creates a separately identified derived fork, stops
playback, and changes the verification banner to sandbox mode. The comparison
surface persistently labels both sides as exploratory simulation and the fork
as “not verified replay.” Split view is the default; swipe and difference
views retain the same linked camera, selected unit, committed tick, and overlay
state. Bookmarks capture that linked tick. The lower inspection area reports
metric deltas, the first differing disclosed event, and the first differing
disclosed field.

Evidence export does not serialize the live DOM. `EvidenceExport` builds a
fresh SVG from the bounded `RenderFrame` with a closed element and attribute
vocabulary. It emits no script, event handler, `foreignObject`, external
reference, URL, replay-provided SVG path, style, or identifier. Untrusted
annotation and provenance text is length-bounded and reduced to inert
characters. PNG export rasterizes this exact sanitized SVG snapshot in a
canvas, so the fallback cannot silently contain a richer state than the SVG.

Every artifact identifies source, replay, projection hash, engine, available
ruleset, exact tick, evidence mode, approved palette, and renderer version.
Derived exports carry the visible legend “DERIVED SIMULATION — NOT VERIFIED
REPLAY.” These are presentation evidence, never a substitute for replay
packages, accepted hashes, or exact-artifact verification.

Projection identity uses a versioned canonical binary encoding: explicit union
tags and array counts, little-endian integers, raw IEEE-754 float bits, and
UTF-8 strings prefixed by byte length. It does not concatenate fields with
text delimiters. Tests pin distinct identities for adversarial edge fields
whose colons, pipes, and commas would collide under delimiter joining.

Laboratory results adapt only the canonical minimal-slice cells, edge, units,
events, and bounded health disclosed by the deterministic report. A sandbox
without that result uses an empty frame instead of the on-screen static
demonstration. This avoids attaching unrelated review-fixture geometry to a
scenario identity.

Pure tests pin canonical bytes and SHA-256 output and exercise hostile strings
containing script, handler, `foreignObject`, URL, style, path, and identifier
payloads. Browser smoke tests cover persistent labels, linked-state markers,
view switching, bookmarks, export controls, provenance disclosure, and
keyboard-readable comparison summaries.
