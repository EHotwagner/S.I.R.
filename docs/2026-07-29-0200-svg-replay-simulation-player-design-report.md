---
title: SVG Replay and Simulation Player Design Report
category: Design
categoryindex: 4
index: 44
status: proposed
document-type: timestamped-design-report
version: "1.0"
created-at: 2026-07-29T02:00:41+02:00
last-updated: 2026-07-29
related:
  - docs/visual-direction.md
  - docs/fable-client-and-documentation.md
  - docs/interactive-rules-lab.md
  - docs/simulation-core-architecture.md
  - docs/formations-and-referents.md
  - docs/reporting-model.md
---

# SVG Replay and Simulation Player Design Report

**Report timestamp:** 2026-07-29 02:00:41 CEST (UTC+02:00)

**Scope:** a canonical SVG presentation for verified replays, perspective
playback, and non-authoritative simulations in the browser.

## Executive decision

Build the player as an SVG tactical table driven by bounded inspection
projections from the existing replay worker. The battlefield is an isometric
square grid. Each unit is a shallow square or rectangular prism whose base is
its authoritative footprint and whose upper face is an information surface,
like a deliberately designed die.

The upper face carries the unit's informative glyph and its primary direction:

- the central glyph identifies role or class;
- the top-face border carries faction;
- a forward wedge cut into the top face shows body facing;
- a compact health rail shows condition;
- optional inspection marks show selected secondary state; and
- a distinct centre-out pointer may show a genuine second heading, such as a
  weapon or sensor axis.

Facing and attention remain different facts. Facing belongs to the unit's
upper face. Attention, observation, and line of sight belong on the ground as
exact sectors or polygons and appear primarily for selected units. The same
shape must never mean both.

The player must not derive authoritative state from pixels, interpolate rule
outcomes, reveal data absent from a perspective projection, or use wall-clock
animation as evidence. SVG is a presentation projection over committed replay
ticks.

## Research basis

This design reconciles four existing bodies of work:

1. [Visual direction](visual-direction.md) establishes authoritative square
   footprints, box-shaped units, top faces as information surfaces, isometric
   terrain, exact selected-unit overlays, and no ghost for lost hostile
   contact.
2. [Fable client and documentation architecture](fable-client-and-documentation.md)
   separates verified replay, perspective playback, and sandbox-derived runs;
   keeps complete replay state in a worker; and treats interpolation as
   presentation rather than authority.
3. The sibling `FS.GG.Rendering` symbology project defines a fixed, typed
   channel grammar, deterministic render functions, grammar-aware legibility
   linting, coverage auditing, and a render-review-tune loop.
4. `FS.GG.Rendering` ADR-0102 establishes two independent heading channels:
   body facing and an optional second pointing direction. It also demonstrates
   the important rule that two headings must differ by form and siting even
   when their angles agree.

The sibling symbology package emits `FS.GG.UI.Scene`, not browser SVG. This
proposal therefore adopts its information-design doctrine and channel
semantics without making an SVG host pretend that a `Scene` is an SVG DOM.
S.I.R. owns the game-specific state-to-glyph mapping and the prism grammar.
Reusable channel meanings remain aligned with the sibling project.

## Product goals

The player must:

- display verified full replays, knowledge-filtered perspective playback, and
  derived design simulations through one coherent surface;
- make 50–100 units per side scannable without replacing spatial reasoning
  with a detached dashboard;
- preserve authoritative grid footprints, semantic edges, levels, and
  committed tick state;
- make role, faction, condition, facing, selection, and disclosure mode
  recognizable at normal tactical zoom;
- support precise seeking, stepping, event inspection, and deterministic
  comparison;
- remain usable with keyboard, screen reader, reduced motion, high contrast,
  and custom faction palettes;
- stay responsive while replay execution remains in a worker; and
- provide visual evidence that can be snapshot-tested without claiming that
  rendered pixels are authoritative.

It is not intended to:

- become the live match authority;
- execute player WASM in the browser and call the result authoritative;
- reconstruct undisclosed state from events, interpolation, or previous
  frames;
- show conventional animated character or vehicle models in place of the
  box-based unit language;
- make every stat permanently visible; or
- invent new symbology geometry whenever a new gameplay field appears.

## Experience model

### One shell, three explicit modes

The shell uses the same layout in every mode, but the verification banner,
available tools, and export language change.

| Mode | Source | Claim | Editing |
|---|---|---|---|
| Verified replay | accepted full replay plus matching engine | browser-kernel verified; player WASM not re-executed | disabled until fork |
| Perspective playback | knowledge-filtered frames | projection-only playback | disabled |
| Simulation sandbox | scenario or forked parameters | exploratory, non-authoritative result | enabled |

The mode label remains visible beside the clock. It is never relegated to a
temporary toast or color alone.

### Desktop composition

```text
┌──────────────────────────────── verification / source identity ───────────────┐
├───────────────┬──────────────── tactical SVG viewport ────────────┬───────────┤
│ source        │                                                   │ inspector │
│ scenario      │       battlefield, units, overlays               │ unit      │
│ layers        │                                                   │ event     │
│ legend        │                                                   │ formula   │
├───────────────┴───────────────────────────────────────────────────┴───────────┤
│ play  step  speed     tick/time     scrubber + events + checkpoints          │
├──────────────────────────────── comparison / metrics drawer (optional) ───────┤
└───────────────────────────────────────────────────────────────────────────────┘
```

The battlefield is the dominant surface. The source/layer rail and inspector
may collapse. The transport remains visible while the player is in replay or
simulation mode.

On a narrow viewport, the SVG stays above a tabbed `Layers / Inspect / Events`
drawer. Transport controls become two rows, with play, step, and current tick
remaining first.

### Loading and failure

Loading is progressive but claims are atomic:

1. validate package envelope and resource limits;
2. resolve the exact engine bundle;
3. produce the first bounded inspection projection;
4. show the battlefield;
5. grant the applicable verification claim only after verification completes.

Malformed, unsupported, unauthorized, diverged, and failed packages keep their
distinct current states. A divergence view freezes at the first divergent
tick, highlights the phase and hashes, disables ordinary playback, and offers
safe metadata export. It never shows the last successful frame as though it
were the requested result.

## Battlefield and camera

### Coordinate spaces

The renderer uses four explicit coordinate spaces:

1. **simulation space** — integer cells, levels, edge identities, absolute
   headings, and committed ticks;
2. **world presentation space** — floating-point cell corners and prism
   heights derived from simulation values;
3. **SVG view space** — isometric projected coordinates inside the `viewBox`;
4. **screen space** — CSS pixels used only for input tolerances, focus rings,
   and readable minimum sizes.

For a cell corner `(column, row, level)`, the default projection is:

```text
screenX = originX + (column - row) × halfCellWidth
screenY = originY + (column + row) × halfCellHeight - level × levelHeight
```

The inverse mapping used for pointer hit tests resolves the projected ground
plane, then validates the candidate against actual footprint polygons. It does
not round a pointer position and assume that the nearest cell was clicked.

The default camera is a fixed isometric projection with pan and bounded zoom.
Optional 90-degree rotation changes presentation only. North and the current
camera rotation are always shown. The player never rotates authoritative
headings in stored data; it rotates only their projected drawing.

### Terrain layers

Back-to-front SVG paint order is deterministic:

1. background and out-of-board mask;
2. terrain fills and level faces;
3. square grid;
4. ground decals and objective areas;
5. exact perception, command, route, and area overlays;
6. semantic edge features;
7. unit shadows and footprint halos;
8. unit prism side faces;
9. unit top faces and glyphs;
10. transient actions, traces, and effects;
11. selection, hover, focus, and inspection annotations;
12. screen-aligned labels.

Within a layer, stable ordering uses projected depth, level, footprint anchor,
and stable entity identity as a final presentation tie-break. That tie-break
must not be reused for gameplay priority.

Thin authoritative features sit on cell boundaries. Doors, windows, fences,
walls, rails, and low walls use distinct state geometry rather than a color
swap alone. Open doors visibly break or rotate away from the blocking edge.

### Zoom levels

The player uses semantic zoom, not uniform shrinking:

| Scale | Kept | Suppressed or aggregated |
|---|---|---|
| Strategic | footprint, faction border, role glyph, facing wedge, selection | text, minor status, fine edge material |
| Tactical | all strategic channels, health, stance height, significant status | long identity text |
| Inspection | full approved top-face detail, labels, exact overlays, edge state | nothing approved for disclosure |

Transitions occur at declared zoom thresholds with hysteresis, so glyph detail
does not flicker when the user rests on a boundary. Selection may retain one
additional detail tier but may not reveal a field absent from the projection.

## Unit prism grammar

### Geometry

Every unit visual begins with its authoritative square or rectangular
footprint. The prism is extruded upward by a presentation height:

- footprint width and depth come only from authoritative occupancy;
- height may encode stance or profile when that mapping is accepted;
- the top face is inset enough to preserve a continuous faction frame;
- side faces use restrained shading derived from the faction-neutral material
  palette; and
- a selected unit receives an outside footprint halo, never a footprint size
  change.

Large units use the same grammar over a larger authoritative base. A two-by-two
monster is not four repeated one-cell glyphs; it is one prism with one
information face and an unambiguous four-cell base.

### Upper-face information hierarchy

The upper face is read in this order:

1. **faction frame** — high-salience saturated outline;
2. **role/class glyph** — central vector mark;
3. **facing wedge** — a triangular forward cut or chevron touching the
   appropriate top-face edge;
4. **condition rail** — a compact screen-aligned health segment along the
   rear top edge;
5. **significant status mounts** — at most two approved corner marks;
6. **identity text** — optional inspection detail at sufficient zoom.

The facing wedge is part of the upper face, satisfying the dice-like visual
direction: the piece itself points. It does not rotate the class glyph or
health rail. Keeping those screen-aligned preserves scan speed while the
asymmetric wedge makes heading readable.

At very small sizes, the wedge becomes a high-contrast edge notch. It must
remain distinguishable for all eight gameplay directions and for continuous
heading values if those are later exposed.

### Fixed channel map

S.I.R. should define one pure `UnitVisualState -> UnitGlyph` mapping. SVG
grammars consume the result; they do not read raw simulation records.

| Game fact | SVG prism channel | Symbology analogue | Capacity guidance |
|---|---|---|---|
| faction/affiliation | top frame and base halo hue | `Faction` stroke hue | no more than 7 simultaneous categories |
| role/class | central vector glyph | `Klass` silhouette + `Sigil` | prefer no more than 6 primary classes per view |
| stable identity | optional short label or secondary mark | `Sigil` / `Label` | inspection detail; never replaces role glyph |
| confirmed/suspected | only if S.I.R. later adopts certainty | stroke treatment | not currently part of canonical presentation |
| shield/armor state | corner mount | `Shield` | binary or ternary inspection channel |
| speed/readiness tier | small pips only if tactically required | `Speed` beads | at most 4 ranked levels |
| magnitude/footprint | authoritative base size | `R` | not a cosmetic rank |
| threat | do not expose by default | `Threat` stroke width | only from legitimate projection data |
| charge/progress | inset fill or arc when selected | `Charge` gradient | at most 4 ranked levels |
| health/condition | rear top-face rail | `Health` arc | continuous, but show meaningful thresholds |
| body facing | top-face forward wedge | `Heading` | continuous or 8-way |
| weapon/sensor heading | centre-out pointer with tip | `SecondaryHeading` | opt-in, visually distinct |
| activity | one board-wide animation rhythm at a time | `Motion` | budget 1 active rhythm per view |

This is a mapping policy, not a promise that every channel is always populated.
Hidden information stays absent. A stat does not earn a glyph merely because
it exists.

### Role glyphs

Role glyphs are simple filled or stroked SVG paths designed on a normalized
24-by-24 grid. They avoid letters as the primary mark. A first prototype set
should cover the accepted human classes and representative arcane, drone,
vehicle, support, objective, and large-creature categories without assuming
that the current proposed catalogs are final.

Each glyph requires:

- a stable semantic identifier independent of path data;
- a human-readable name and description;
- a monochrome path that works in any permitted faction palette;
- recognition at the strategic minimum size;
- a non-color distinction from every other primary glyph in its review set;
- a text alternative; and
- a mapping-table entry or an explicit reasoned decision not to render.

SVG path data lives in one `<symbol>` catalog under `<defs>` and is instantiated
with `<use>`. Product state maps to catalog identifiers; it never injects
arbitrary replay-supplied path markup.

### Two headings and attention

The three directional concepts have intentionally different forms:

| Fact | Placement | Form |
|---|---|---|
| body facing | top-face perimeter | filled wedge/notch touching the forward edge |
| optional weapon or sensor heading | top-face centre | thin centre-out pointer ending in a dot |
| attention/observation | ground | translucent exact sector or polygon with boundary |

When body and secondary headings align, the centre-out pointer stops before
the facing wedge so both remain visible. When they oppose, neither crosses the
central role glyph; the pointer begins outside the glyph's clear zone.

Attention is not automatically mapped to `SecondaryHeading`. Attention may be
a sector, multi-lobed field, or exact visible polygon rather than a ray.

### Health, damage, and death

Health uses a top-face rail because current hit points are required at normal
gameplay zoom. The rail:

- decreases monotonically in length;
- uses palette tokens rather than raw red/green assumptions;
- adds threshold notches at accepted rule boundaries;
- exposes exact numbers only in the inspector or at inspection zoom; and
- updates only on committed ticks.

Damage may produce a short presentation flash keyed to a committed damage
event. The flash is derived from replay time, is disabled under reduced motion,
and is never needed to discover that health changed.

A defeated unit follows the authoritative event and persistence rules. The
renderer does not decide whether a body, marker, wreck, or nothing remains.

### Selection and status

Selection is drawn outside the authoritative footprint with a double-corner
bracket and a screen-reader announcement. Hover uses a lighter single bracket.
Keyboard focus uses the platform focus color and remains visible independently
of hover.

Status mounts are a scarce resource. The default top face displays no more
than two corner marks. Surplus statuses move to the inspector and an optional
selected-unit callout. A generic “more” dot may announce additional disclosed
statuses, but it may not merge statuses into an ambiguous color.

## Overlays

Layers are individually toggleable and grouped by task:

- movement: route, reservation, destination, formation station;
- perception: exact visible geometry, attention, sensor coverage;
- command: communication links, delivery delay, referents, objective beliefs;
- combat: engagement, target, trace, effect area;
- logistics: ownership, routes, reservations, transfers;
- diagnostics: cell coordinates, entity IDs, checkpoints, projection hashes.

Only selected-unit line of sight defaults to exact geometry. Whole-force exact
visibility is an explicit diagnostic action and may be rejected when it exceeds
the configured complexity budget.

Overlay styling obeys three rules:

1. fills remain translucent enough to preserve terrain and footprints;
2. boundaries use pattern or dash differences where layers may overlap; and
3. an overlay never changes the underlying unit glyph's meaning.

Perspective playback renders only fields delivered by its projection. Lost
hostile contact disappears immediately when the applicable frame no longer
contains it. The client does not retain a ghost, last-known marker, DOM node,
accessible label, or hit target.

## Replay transport and timeline

### Primary controls

The transport provides:

- play/pause;
- one committed-tick step backward and forward;
- previous/next event;
- speed choices `½×`, `1×`, `2×`, `4×`, and maximum;
- current tick and elapsed simulation time;
- a range scrubber;
- checkpoint ticks;
- filtered event markers;
- loop range for design review; and
- a “return to live end” action when inspecting a growing simulation.

Space or `K` toggles play, arrow keys step, `J`/`L` seek between events, and
Home/End seek to boundaries when focus is in the player. Shortcuts do not fire
while a text field, slider, or editable control owns the key.

Maximum speed advances bounded worker batches and paints at a lower
presentation cadence. It never drops committed simulation ticks from
verification; it only coalesces intermediate visual frames.

### Seeking

A seek request identifies a target committed tick. The worker restores the
nearest retained checkpoint at or before the target, deterministically advances
to it, then returns one bounded inspection projection. The UI:

- shows progress for long seeks;
- cancels the previous seek when a newer operation supersedes it;
- correlates every response with its operation identifier;
- keeps the old frame visibly marked as stale while seeking; and
- commits the new frame only when the matching response arrives.

Backward stepping is a seek, not inverse simulation.

### Timeline lanes

The timeline has optional lanes for:

- authoritative or disclosed events;
- checkpoints;
- selections and bookmarks;
- communications and acknowledgements;
- unit-local actions;
- comparison divergence.

Event density aggregates by pixel column. Zooming the timeline expands clusters
without changing event order. Selecting an event seeks to its committed tick,
selects legitimately disclosed participants, and opens the event inspector.

### Replay time and interpolation

At rest or during stepping, the SVG shows the exact committed tick. During
playback, unit translation may interpolate between two disclosed committed
positions to improve motion:

- interpolation never crosses a blocked semantic edge;
- teleport, level change, spawn, destruction, and discontinuity events cut the
  interpolation;
- health, status, facing, visibility, and event occurrence change at the
  declared committed boundary unless a presentation-only transition is
  explicitly safe;
- scrubbing disables interpolation; and
- an “exact ticks” option disables it everywhere.

All animation phases derive from `(current replay tick, fractional presentation
alpha, event identity)`. SVG SMIL, CSS infinite animations, and wall-clock-only
timers are not sources of replay state.

## Inspection and comparison

Selecting a unit opens a structured inspector:

- identity and disclosure source;
- role, faction, footprint, cell, level, facing, and condition;
- current disclosed action and target;
- disclosed communication, referent, formation, and objective facts;
- the event history available in this replay mode; and
- raw diagnostic IDs behind an advanced disclosure.

An unavailable field says “not present in this projection,” not zero, unknown,
or stale. The inspector must distinguish absent, not applicable, and explicitly
unknown values in its view model.

Simulation comparison supports a baseline and one fork:

- linked camera, selection, tick, and overlay state by default;
- split, swipe, and difference-overlay views;
- metric deltas in the lower drawer;
- first divergent event and first differing disclosed field;
- persistent labels identifying baseline and fork; and
- no implication that either sandbox result is authoritative.

Verified replay cannot be edited in place. Editing creates a named fork with
its own identity, returns the shell to sandbox mode, and retains a link to the
source replay.

## SVG component architecture

### Pure render model

Do not render directly from `SimulationState` or the binary replay package.
Introduce a presentation-safe model:

```fsharp
type Heading =
    { BodyRadians: float
      SecondaryRadians: float option }

type UnitGlyph =
    { CatalogId: string
      Faction: string
      Health01: float
      Facing: Heading
      Label: string option
      Statuses: string list }

type UnitVisual =
    { Id: int32
      AnchorColumn: int32
      AnchorRow: int32
      Level: int32
      FootprintWidth: int32
      FootprintDepth: int32
      ProfileHeight: float
      Glyph: UnitGlyph }

type RenderFrame =
    { Tick: int32
      Board: BoardVisual
      Units: UnitVisual list
      Edges: EdgeVisual list
      Overlays: OverlayVisual list
      Events: EventVisual list
      Disclosure: DisclosureLabel }
```

Names are illustrative; final types should use constrained domain values rather
than unconstrained strings and floats where practical.

The mapping pipeline is:

```text
bounded worker projection
    -> disclosure-preserving presentation projection
    -> pure UnitVisual / OverlayVisual mapping
    -> deterministic SVG virtual tree
    -> browser DOM
```

The renderer may receive both endpoints for interpolation, but the committed
`RenderFrame` remains independently drawable.

### Required projection evolution

The current `UnitProjection` exposes only ID, side, column, row, and health.
That is enough for the current rectangular board, not for the proposed glyph.
Add fields only when the underlying replay or perspective schema can disclose
them correctly:

- footprint width and depth;
- level and presentation profile/stance;
- role/class visual identifier;
- body facing;
- optional second heading;
- short identity label;
- approved status identifiers; and
- overlay geometry or references.

Every field added to the full replay projection needs an explicit answer for
perspective playback. “Omit when not disclosed” is safer than constructing a
default. Transport records should use structured-clone-safe arrays and scalar
values as the current worker boundary does.

### DOM structure

```xml
<svg role="application" viewBox="…">
  <title>Replay battlefield at tick …</title>
  <desc>…mode, selected unit, and visible-unit summary…</desc>
  <defs>
    <symbol id="glyph-rifleman">…</symbol>
    <pattern id="overlay-command">…</pattern>
    <clipPath id="board-clip">…</clipPath>
  </defs>
  <g data-layer="terrain">…</g>
  <g data-layer="overlays">…</g>
  <g data-layer="edges">…</g>
  <g data-layer="units">
    <g data-unit-id="…" tabindex="0" aria-label="…">…</g>
  </g>
  <g data-layer="effects">…</g>
  <g data-layer="selection">…</g>
</svg>
```

Use `<path>`, `<polygon>`, `<line>`, `<circle>`, `<text>`, `<use>`, and grouped
transforms. Avoid `foreignObject` for battlefield content. UI controls remain
semantic HTML around the SVG.

Replay data never becomes raw SVG markup, CSS, element IDs, URLs, or event
handler source. Generated identifiers are internal, prefixed, and escaped.

### Update strategy

Retain stable unit groups by entity ID. On a frame:

- update transforms for moving groups;
- update only attributes whose presentation values changed;
- add and remove units based on the current disclosed projection;
- keep `<defs>` stable;
- rebuild exact overlays only when their geometry revision changes; and
- batch DOM work into one animation frame.

Do not leave hidden hostile groups in the DOM with `display:none`; remove them
from the rendered and accessible trees.

For very large static terrain, one SVG path may combine same-style cell faces.
Interactive edges and units remain separate hit targets. If profiling shows
that the unit DOM exceeds budget, simplify semantic detail before replacing
the SVG unit layer with an inaccessible bitmap.

## Legibility and coverage process

The sibling symbology workflow should be mirrored for S.I.R.'s prism grammar:

1. inventory every production-visible unit and tactical element;
2. map each element to a shown glyph or a reasoned hidden decision;
3. render representative rosters in all semantic zoom tiers;
4. lint channel use against declared capacities;
5. inspect SVG and reference PNGs at actual target sizes;
6. tune the product mapping, not ad hoc geometry;
7. repeat until mechanical findings are clean and human review passes;
8. pin the approved catalog, rationale, and golden boards.

Each review iteration records a timestamped SVG/PNG board and the exact mapping
snapshot that produced it. Deterministic scene identity excludes the workflow
timestamp.

At minimum, review boards include:

- 100 versus 100 at strategic zoom;
- eight headings for every primary glyph;
- aligned and opposed primary/secondary headings;
- minimum, threshold, and maximum health;
- every supported faction palette;
- selected exact line of sight over dense semantic edges;
- perspective contact disappearing between consecutive frames;
- high status density;
- large footprints and mixed levels; and
- reduced-motion and high-contrast modes.

If the fixed prism grammar cannot express a new essential fact, file a
cross-repository request against the owning symbology contract or record a
S.I.R. design decision. Do not smuggle a second meaning into an existing
channel merely because it fits the linter's numeric capacity.

## Accessibility

The battlefield supports a roving focus model:

- Tab enters or leaves the battlefield;
- arrow keys move focus to the nearest disclosed unit in the requested screen
  direction;
- Enter selects;
- Escape clears selection or closes the current overlay;
- the inspector provides the same information without requiring SVG pointer
  interaction.

Every focusable unit has a concise label such as “Blue rifleman Bravo 6, cell
C4, 75 health, facing north-east.” Labels contain only disclosed data. The SVG
root describes mode, tick, selection, and visible-unit count. Event and tick
changes use a throttled live region; playback does not announce every frame.

Accessibility settings include:

- custom faction and overlay palettes;
- high-contrast outlines and patterns;
- text scaling without clipping the surrounding shell;
- reduced motion that removes flashes and interpolation;
- exact-tick playback;
- keyboard shortcut help; and
- optional direction text in the inspector.

Although current visual direction permits color as a standalone category, role
glyphs, focus, selection, direction, edge state, and verification mode remain
non-color-readable because they already have meaningful geometry or text.

## Performance budgets

Initial browser targets, to be replaced by measurement:

| Measure | Target |
|---|---|
| disclosed units | 200 normal, 400 stress |
| interactive SVG nodes | under 8,000 normal view |
| steady playback paint | 60 Hz when available; never below 30 Hz at 200 units on reference hardware |
| maximum-speed paint | coalesced to at most 15 Hz while worker verification continues |
| main-thread frame work | p95 under 8 ms during ordinary playback |
| seek response feedback | visible within 100 ms |
| pointer/keyboard response | visible within 100 ms |
| exact selected overlay | under 2,000 path segments before simplification warning |

Measure worker execution, projection transfer, projection mapping, SVG
reconciliation, layout/style, and paint separately. A fast worker does not
prove a responsive SVG, and a fast SVG does not prove replay verification.

Prefer:

- `<symbol>/<use>` for repeated glyph geometry;
- class and CSS custom-property styling for palette changes;
- precomputed normalized path catalogs;
- geometry revision keys;
- batched attribute changes;
- view culling for off-board annotations; and
- coalesced presentation frames at high replay speed.

Avoid per-unit filters, large blurred shadows, animated gradients, layout reads
inside the update loop, and a unique clip path for every unchanged unit.

## Security and trust boundaries

Replay packages are untrusted input. Before SVG rendering:

- enforce existing package, unit, edge, observation, and frame limits;
- validate all numeric values as finite and in declared ranges;
- bound labels, status counts, path segment counts, and overlay vertices;
- reject unknown catalog identifiers or map them to a visible safe
  placeholder;
- never load external images, fonts, stylesheets, links, or paint servers from
  replay data;
- never accept arbitrary SVG path data from a replay;
- sanitize exported annotations separately from replay content; and
- preserve Content Security Policy compatibility.

Exported SVG is an evidence artifact, not an authoritative replay. It includes
source identity, tick, mode, engine identity, ruleset identity when available,
projection hash when applicable, palette identity, and renderer version in
metadata. It excludes undisclosed state and executable script.

## Testing strategy

### Pure tests

- projection-to-visual mapping is deterministic;
- identical `RenderFrame` plus options yields identical canonical SVG;
- isometric projection and inverse hit testing agree at cell corners and
  footprint interiors;
- stable depth order does not depend on map iteration;
- every catalog entry has a name, path, text alternative, and mapping;
- every production element is shown or has a reasoned hidden decision;
- heading wedges wrap angles and reject non-finite input;
- health geometry is monotone;
- contact removal removes the unit from render and accessibility models;
- semantic zoom thresholds have hysteresis; and
- perspective mappings cannot populate full-replay-only fields.

### Browser tests

- play, pause, step, seek, speed, cancellation, and operation correlation;
- pointer and keyboard selection after pan, zoom, and camera rotation;
- focus order and inspector parity;
- no stale DOM node after lost contact;
- reduced-motion behavior;
- responsive panel layouts;
- custom palettes and high contrast;
- large-board frame budget;
- safe handling of hostile labels and unknown catalog IDs; and
- SVG export metadata and absence of scripts/external references.

### Visual evidence

Pin reference renders for the review boards listed above. Visual diffs are
review evidence, never self-approving tests. Any changed golden requires a
mapping or design rationale.

Run replay conformance tests independently. Pixel equality cannot replace
state/event hash verification, and a correct state hash cannot replace visual
inspection.

## Roadmap

Roadmap items remain unchecked until their implementation and phase exit
evidence are complete.

### Phase 0 — contract and catalog

- [ ] Accept this report or extract its durable decisions into living design
  docs.
- [ ] Define `UnitVisual`, `RenderFrame`, disclosure-safe optional fields, and
  structured-clone transport.
- [ ] Inventory unit and overlay elements.
- [ ] Author the first role-glyph catalog and accessibility descriptions.
- [ ] Record whether the sibling symbology package is a semantic reference
  only or a future build-time dependency.

**Exit:** typed contract review, complete initial coverage inventory, and no
ambiguous disclosure defaults.

### Phase 1 — static SVG battlefield

- [ ] Implement projection, grid, terrain, semantic edges, prism units,
  upper-face glyphs, health, and facing.
- [ ] Add pan, zoom, selection, focus, inspector, legend, and palettes.
- [ ] Render exact committed frames without interpolation.

**Exit:** representative static frames pass mapping tests, accessibility tests,
legibility review, and the 200-unit performance target.

### Phase 2 — replay transport

- [ ] Connect current worker projections to the SVG.
- [ ] Add seek, step backward/forward, event navigation, checkpoint markers,
  cancellation, and progress.
- [ ] Preserve all verification and divergence states.

**Exit:** full and perspective fixtures seek deterministically; disclosure
tests prove lost contact leaves no visual or accessible residue.

### Phase 3 — overlays and playback polish

- [ ] Add exact selected overlays, semantic timeline lanes, deterministic
  interpolation, action traces, and reduced motion.
- [ ] Add the optional second heading only with an accepted gameplay source.

**Exit:** exact-tick and interpolated modes converge on every committed frame;
two-heading and exact-overlay visual reviews pass.

### Phase 4 — simulation comparison and export

- [ ] Add fork workflow, linked baseline/fork comparison, divergence
  inspection, bookmarks, and safe SVG evidence export.
- [ ] Capture end-to-end performance and visual-review provenance.

**Exit:** a derived simulation cannot be mistaken for verified replay, exports
carry complete provenance, and all normal/stress budgets have measured results.

## Acceptance criteria

The first production-ready player is acceptable when:

1. A unit's rendered base matches its authoritative footprint at every camera
   rotation.
2. Role, faction, health, and body facing remain readable at normal tactical
   zoom.
3. The upper-face facing wedge is unambiguous for the supported heading model.
4. Attention and optional secondary heading cannot be mistaken for facing.
5. A selected unit's exact visible geometry matches its disclosed projection.
6. Full, perspective, and sandbox modes remain explicitly labeled.
7. A lost hostile contact leaves no visual, DOM, accessibility, event-link, or
   hit-target residue beyond legitimately disclosed history.
8. Paused and stepped frames show exact committed ticks.
9. Interpolation never changes committed state or verification outcomes.
10. Seeking uses checkpoints and operation correlation without stale-response
    races.
11. Keyboard and pointer users can reach the same selection and inspection
    information.
12. The production element inventory has no unexplained visual gaps.
13. Channel-capacity and target-size reviews are clean for representative
    rosters.
14. The 200-unit normal view meets measured responsiveness targets.
15. Exported SVG contains provenance and no executable or external replay-
    supplied content.

## Decisions still requiring prototypes

- the exact isometric cell ratio and default camera elevation;
- final role/class catalog and which roles share a primary glyph;
- whether stance height is readable enough to serve as a reliable channel;
- the health rail's normal-zoom numeric threshold;
- the accepted faction and overlay palette presets;
- whether a selected unit repeats facing at its base for dense occluded scenes;
- the exact tactical and strategic semantic-zoom thresholds;
- comparison default: split, swipe, or difference overlay;
- permitted whole-force overlay complexity; and
- whether SVG evidence export is needed in the first release or only PNG
  capture plus replay identity.

These are prototype questions, not reasons to leave replay authority,
disclosure, direction semantics, or channel ownership ambiguous.

## Recommendation

Proceed with Phase 0 and a small Phase 1 prototype containing:

- one six-by-six isometric board;
- blocking and open semantic edges;
- eight units spanning factions, roles, health, footprints, and all eight
  facings;
- one selected exact attention/visibility polygon;
- one optional second heading;
- strategic, tactical, and inspection zoom tiers; and
- keyboard selection plus the structured inspector.

Render that prototype at the smallest intended tactical size, run a coverage
and channel-capacity review, and tune only the S.I.R. state-to-glyph mapping.
That will test the decisive visual proposition—the informative upper face of
the unit die—before replay transport and comparison features increase the
surface area.
