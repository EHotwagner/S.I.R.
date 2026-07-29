---
title: SVG Replay and Simulation Player Design Report
category: Design
categoryindex: 4
index: 44
status: proposed
document-type: timestamped-design-report
version: "1.4"
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

**Scope:** a top-down, documentation-only SVG presentation for verified
replays, perspective playback, and non-authoritative simulations in the
browser; the Babylon.js production client remains separate.

## Executive decision

Build the player as a documentation-only SVG tactical table driven by bounded
inspection projections from the existing replay worker. The battlefield is a
flat, top-down square grid. This deliberately avoids introducing simulated 3D
into explanations and lets the same symbol catalog serve interactive playback,
static diagrams, and exported documentation evidence. The production game
client is a separate Babylon.js surface and is not constrained to reproduce
this renderer.

Every unit uses a square symbol even when its authoritative footprint covers a
different shape or area. The footprint remains a separate ground outline; the
symbol never stretches to impersonate occupancy. The square carries a fixed
information grammar:

- the central glyph identifies the exact class;
- an outer outline identifies faction;
- twelve inset perimeter dashes show normalized remaining health;
- a wedge or notch on the square perimeter shows body facing;
- a small corner elevation stack gives non-color height information;
- a close-zoom mark may show stance; and
- a distinct centre-out pointer may show a genuine second heading, such as a
  weapon or sensor axis.

Facing and attention remain different facts. Facing belongs to the square's
perimeter. Attention, observation, and line of sight belong on the ground as
exact sectors or polygons and appear primarily for selected units. The same
shape must never mean both.

The player must not derive authoritative state from pixels, interpolate rule
outcomes, reveal data absent from a perspective projection, or use wall-clock
animation as evidence. SVG is a presentation projection over committed replay
ticks.

## Research basis

This design reconciles four existing bodies of work:

1. [Visual direction](visual-direction.md) establishes authoritative square
   footprints, box-shaped units, information-bearing unit surfaces, exact
   selected-unit overlays, and no ghost for lost hostile contact. This report
   narrows the documentation renderer to top-down 2D; its square glyphs are an
   explanatory projection rather than the production client's 3D geometry.
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
S.I.R. owns the game-specific state-to-glyph mapping and square-symbol grammar.
Reusable channel meanings remain aligned with the sibling project.

## Product goals

The player must:

- display verified full replays, knowledge-filtered perspective playback, and
  derived design simulations through one coherent surface;
- embed the same flat glyphs in narrative documentation and interactive
  explanations;
- make 50–100 units per side scannable without replacing spatial reasoning
  with a detached dashboard;
- preserve authoritative grid footprints, semantic edges, levels, and
  committed tick state;
- make exact class, faction, condition, facing, selection, and disclosure mode
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
- replace or visually constrain the Babylon.js production client;
- execute player WASM in the browser and call the result authoritative;
- reconstruct undisclosed state from events, interpolation, or previous
  frames;
- simulate 3D camera elevation, model height, lighting, or occlusion;
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
2. **world presentation space** — floating-point top-down cell corners and
   footprint polygons derived from simulation values;
3. **SVG view space** — orthographic coordinates inside the `viewBox`;
4. **screen space** — CSS pixels used only for input tolerances, focus rings,
   and readable minimum sizes.

For a cell corner `(column, row, level)`, the default projection is:

```text
screenX = originX + column × cellSize
screenY = originY + row × cellSize
```

`level` does not offset the cell or unit geometry. It is presented through
explicit elevation marks and optional overlays. The inverse mapping used for
pointer hit tests resolves the top-down grid cell, then validates the candidate
against actual footprint polygons and interactive features.

The default camera is a fixed top-down orthographic projection with pan and
bounded zoom. Optional 90-degree rotation changes presentation only. North and
the current camera rotation are always shown. The player never rotates
authoritative headings in stored data; it rotates only their projected
drawing. There is no camera elevation control.

### Terrain layers

Back-to-front SVG paint order is deterministic:

1. background and out-of-board mask;
2. terrain fills and level-area contours;
3. square grid;
4. ground decals and objective areas;
5. exact perception, command, route, and area overlays;
6. semantic edge features;
7. authoritative unit footprint outlines and selection halos;
8. square unit symbol backgrounds and faction outlines;
9. health dashes, class glyphs, facing, elevation, stance, and status marks;
10. transient actions, traces, and effects;
11. selection, hover, focus, and inspection annotations;
12. screen-aligned labels.

Within a layer, stable ordering uses row, column, level, footprint anchor, and
stable entity identity as a final presentation tie-break. That tie-break must
not be reused for gameplay priority. Elevation does not create painter's-order
occlusion in this documentation projection.

Thin authoritative features sit on cell boundaries. Doors, windows, fences,
walls, rails, and low walls use distinct state geometry rather than a color
swap alone. Open doors visibly break or rotate away from the blocking edge.

### Zoom levels

The player uses semantic zoom, not uniform shrinking. Thresholds refer to the
projected width of a unit square:

| Scale | Kept | Suppressed or aggregated |
|---|---|---|
| Overview, below 24 px | footprint, faction outline, class glyph, facing notch, selection | health detail, stance, text, minor status, fine edge material |
| Standard, 24–48 px | all overview channels, 12-segment health track, elevation stack, significant status | long identity text and stance when illegible |
| Detailed, above 48 px | full approved symbol detail, stance, labels, exact height text, exact overlays, edge state | nothing approved for disclosure |

Transitions use approximately 10% hysteresis, so glyph detail does not flicker
when the user rests on a boundary. Selection may retain one additional detail
tier but may not reveal a field absent from the projection.

## Square unit-symbol grammar

### Geometry

Every unit visual combines two independent geometries:

- an authoritative ground outline whose width and depth come only from
  occupancy; and
- one square information symbol centred on the footprint's canonical anchor.

The information symbol remains square for one-cell, rectangular, and large
units. A two-by-two monster therefore has one square class symbol over one
unambiguous four-cell footprint, not four repeated glyphs or a stretched
symbol. A selected unit receives an outside footprint halo, never a footprint
or symbol size change.

### Square information hierarchy

The square is read from outside inward:

1. **faction outline** — a high-salience outer frame;
2. **facing wedge** — a filled wedge or notch interrupting the appropriate
   outer edge;
3. **health track** — twelve inset perimeter dashes;
4. **class glyph** — one central vector mark for the exact class;
5. **elevation stack** — a small screen-aligned stepped mark in one reserved
   corner;
6. **stance and significant status marks** — close-zoom secondary marks in
   declared mounts; and
7. **identity text** — optional detailed-zoom content.

The class glyph, health dashes, elevation stack, and text remain screen-aligned
while the facing wedge moves around the perimeter. At very small sizes, the
wedge becomes a high-contrast edge notch. It must remain distinguishable for
all eight gameplay directions and for continuous heading values if those are
later exposed.

### Fixed channel map

S.I.R. should define one pure `UnitVisualState -> UnitGlyph` mapping. SVG
grammars consume the result; they do not read raw simulation records.

| Game fact | SVG square channel | Symbology analogue | Capacity guidance |
|---|---|---|---|
| faction/affiliation | outer outline and footprint halo hue | `Faction` stroke hue | no more than 7 simultaneous categories |
| exact class | central vector glyph | `Klass` silhouette + `Sigil` | one catalog glyph per exact class |
| stable identity | optional short label or secondary mark | `Sigil` / `Label` | detailed view; never replaces class glyph |
| confirmed/suspected | only if S.I.R. later adopts certainty | stroke treatment | not currently part of canonical presentation |
| shield/armor state | corner mount | `Shield` | binary or ternary inspection channel |
| speed/readiness tier | small pips only if tactically required | `Speed` beads | at most 4 ranked levels |
| magnitude/footprint | separate authoritative ground outline | `R` | never inferred from symbol size |
| threat | do not expose by default | `Threat` stroke width | only from legitimate projection data |
| charge/progress | inset fill or arc when selected | `Charge` gradient | at most 4 ranked levels |
| health/condition | 12 inset perimeter dashes | `Health` arc | fixed normalized segments |
| body facing | outer-edge wedge or notch | `Heading` | continuous or 8-way |
| weapon/sensor heading | centre-out pointer with tip | `SecondaryHeading` | opt-in, visually distinct |
| elevation/height | reserved-corner stepped stack | none | 1–3 bars, then capped stack plus `+N` |
| stance | close-zoom interior mark | none | disclosed values only; inspector fallback |
| activity | one board-wide animation rhythm at a time | `Motion` | budget 1 active rhythm per view |

This is a mapping policy, not a promise that every channel is always populated.
Hidden information stays absent. A stat does not earn a glyph merely because
it exists.

### Class glyphs

Class glyphs are simple filled or stroked SVG paths designed on a normalized
24-by-24 grid. They avoid letters as the primary mark. Every exact class
receives its own primary glyph; related classes may share construction rules
but not a catalog identifier. A first prototype set should cover the accepted
human classes and representative arcane, drone, vehicle, support, objective,
and large-creature categories without assuming that the current proposed
catalogs are final.

Each glyph requires:

- a stable semantic identifier independent of path data;
- a human-readable name and description;
- a monochrome path that works in any permitted faction palette;
- recognition at the overview minimum size;
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
| body facing | square perimeter | filled wedge/notch touching the forward edge |
| optional weapon or sensor heading | square centre | thin centre-out pointer ending in a dot |
| attention/observation | ground | translucent exact sector or polygon with boundary |

When body and secondary headings align, the centre-out pointer stops before
the facing wedge so both remain visible. When they oppose, neither crosses the
central class glyph; the pointer begins outside the glyph's clear zone.

Attention is not automatically mapped to `SecondaryHeading`. Attention may be
a sector, multi-lobed field, or exact visible polygon rather than a ray.

### Health, damage, and death

Health uses twelve fixed dashes distributed around the inside of the square
perimeter. The outer faction outline remains visually separate. The track:

- lights zero dashes at zero health and otherwise
  `ceil(12 × Health01)` dashes in a stable clockwise order;
- uses a health token that defaults to red, with contrast and pattern
  equivalents in accessible palettes;
- distinguishes active and depleted dashes by form or value, not hue alone;
- exposes exact numbers only in the inspector or at detailed zoom; and
- updates only on committed ticks.

Damage may produce a short presentation flash keyed to a committed damage
event. The flash is derived from replay time, is disabled under reduced motion,
and is never needed to discover that health changed.

A defeated unit follows the authoritative event and persistence rules. The
renderer does not decide whether a body, marker, wreck, or nothing remains.

### Elevation and stance

Elevation is useful context but not a dominant demonstration channel. Ground
level has no mark. Levels one through three use one through three short,
screen-aligned stepped bars in a reserved corner inside the square. Higher
levels use the capped three-bar stack and add an exact `+N` label at detailed
zoom. Exact elevation is always available in the inspector.

The elevation stack uses geometry and line count rather than color. It does not
offset the symbol, imply a 3D camera, or change painter's order. Stance is
separate: it may appear as a disclosed interior symbol at detailed zoom and
otherwise remains in the inspector or an explanatory label.

### Selection and status

Selection is drawn outside the authoritative footprint with a double-corner
bracket and a screen-reader announcement. Hover uses a lighter single bracket.
Keyboard focus uses the platform focus color and remains visible independently
of hover.

Status mounts are a scarce resource. One corner is reserved for elevation; the
remaining declared mounts display no more than two status marks. Surplus
statuses move to the inspector and an optional selected-unit callout. A
generic “more” dot may announce additional disclosed statuses, but it may not
merge statuses into an ambiguous color.

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
- exact class, faction, footprint, cell, level, stance, facing, and condition;
- current disclosed action and target;
- disclosed communication, referent, formation, and objective facts;
- the event history available in this replay mode; and
- raw diagnostic IDs behind an advanced disclosure.

An unavailable field says “not present in this projection,” not zero, unknown,
or stale. The inspector must distinguish absent, not applicable, and explicitly
unknown values in its view model.

Simulation comparison supports a baseline and one fork:

- a linked, persistently labeled split view by default;
- linked camera, selection, tick, and overlay state;
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
      StanceId: string option
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
- level and optional stance;
- exact-class visual identifier;
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

For very large static terrain, one SVG path may combine same-style cell fills.
Interactive edges and units remain separate hit targets. If profiling shows
that the unit DOM exceeds budget, simplify semantic detail before replacing
the SVG unit layer with an inaccessible bitmap.

## Legibility and coverage process

The sibling symbology workflow should be mirrored for S.I.R.'s square-symbol
grammar:

1. inventory every documentation-visible unit and tactical element;
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

- 100 versus 100 at overview zoom;
- eight headings for every primary glyph;
- aligned and opposed primary/secondary headings;
- zero through twelve normalized health segments;
- every supported faction palette;
- ground, one-bar, three-bar, and capped-plus-label elevation;
- stance absent at standard zoom and present at detailed zoom;
- selected exact line of sight over dense semantic edges;
- perspective contact disappearing between consecutive frames;
- high status density;
- large footprints and mixed levels; and
- reduced-motion and high-contrast modes.

If the fixed square-symbol grammar cannot express a new essential fact, file a
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
C4, elevation 2, 75 health, facing north-east.” Labels contain only disclosed
data. The SVG root describes mode, tick, selection, and visible-unit count.
Event and tick changes use a throttled live region; playback does not announce
every frame.

Accessibility settings include:

- custom faction and overlay palettes;
- high-contrast outlines and patterns;
- text scaling without clipping the surrounding shell;
- reduced motion that removes flashes and interpolation;
- exact-tick playback;
- keyboard shortcut help; and
- optional direction text in the inspector.

Although current visual direction permits color as a standalone category,
class glyphs, health state, elevation, focus, selection, direction, edge state,
and verification mode remain non-color-readable because they already have
meaningful geometry, value, pattern, or text.

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
| whole-force overlays | at most 8,000 path segments before aggregation or refusal |

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

Exported SVG and PNG files are evidence artifacts, not authoritative replays.
Both carry or accompany source identity, tick, mode, engine identity, ruleset
identity when available, projection hash when applicable, palette identity,
and renderer version. Sanitized SVG excludes undisclosed state, executable
script, and external references. PNG provides the stable preview and fallback
for documentation systems that do not admit inline SVG.

## Testing strategy

### Pure tests

- projection-to-visual mapping is deterministic;
- identical `RenderFrame` plus options yields identical canonical SVG;
- orthographic projection and inverse hit testing agree at cell corners and
  footprint interiors;
- stable paint order does not depend on map iteration;
- every catalog entry has a name, path, text alternative, and mapping;
- every documented element is shown or has a reasoned hidden decision;
- heading wedges wrap angles and reject non-finite input;
- health geometry is monotone and contains exactly twelve normalized segments;
- elevation stacks cap at three bars and exact high-level labels appear only
  at detailed zoom;
- stance marks obey the 48 px threshold and disclosure rules;
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
- accessible default, high-contrast, and monochrome/pattern palettes;
- large-board frame budget;
- whole-force overlay aggregation or refusal above 8,000 segments;
- safe handling of hostile labels and unknown catalog IDs; and
- SVG and PNG export provenance, plus absence of scripts and external
  references from SVG.

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

- [x] Accept this report or extract its durable decisions into living design
  docs.
- [x] Record the boundary between the top-down documentation renderer and the
  Babylon.js production client.
- [x] Define `UnitVisual`, `RenderFrame`, disclosure-safe optional fields, and
  structured-clone transport.
- [x] Inventory unit and overlay elements.
- [x] Author one primary glyph for every exact class, with accessibility
  descriptions and a visible safe placeholder.
- [x] Define the accessible default, high-contrast, and monochrome/pattern
  palette tokens.
- [x] Record whether the sibling symbology package is a semantic reference
  only or a future build-time dependency.

**Exit:** typed contract review, complete initial coverage inventory, and no
ambiguous disclosure defaults; every initial class has a catalog entry.

**Completed 2026-07-29:** durable decisions, the renderer boundary, the
coverage inventory, and the semantic-reference-only decision now live in
[SVG Replay Player Contract and Visual Catalog](svg-replay-player.md).
`SIR.Client` contains the constrained presentation contract, validated
structured-clone transport, fourteen-class glyph catalog, safe unknown-class
placeholder, and three palette token sets. Client tests cover contract
round-tripping, invalid disclosure rejection, catalog completeness,
accessibility metadata, placeholder behavior, and palette coverage. The full
.NET/Fable conformance, production browser build, documentation verification,
browser smoke, and accessibility gates passed.

### Phase 1 — static SVG battlefield

- [x] Implement the orthographic top-down projection, square grid, terrain,
  semantic edges, and authoritative footprint outlines.
- [x] Implement the fixed square symbol with faction outline, exact-class
  glyph, 12-segment health track, and perimeter facing wedge.
- [x] Implement the corner elevation stack, capped `+N` detailed label, and
  detailed-zoom stance mark.
- [x] Apply the 24 px and 48 px semantic-zoom thresholds with approximately
  10% hysteresis.
- [x] Add pan, zoom, selection, focus, inspector, legend, and palettes.
- [x] Render exact committed frames without interpolation.

**Exit:** representative static frames pass mapping tests, accessibility tests,
legibility review, all three palette reviews, and the 200-unit performance
target.

**Completed 2026-07-29:** the Fable documentation client renders a committed
six-by-six tick-24 frame through the pure `Battlefield` projection and an
accessible Feliz SVG view. Pure and browser-DOM tests cover board-relative
projection, semantic edges, independent footprints, all symbol channels,
disclosure omission, semantic-zoom hysteresis, pointer/keyboard parity,
inspector equivalence, and palette-stable geometry. The 200-unit fixture
projects 6,933 estimated interactive nodes; the parent verification run
measured pure scene projection p95 at 0.173 ms over 240 runs. The three
deterministic 768 × 768 SVG/PNG palette boards and their hashes are recorded
in [Static SVG battlefield review evidence](assets/svg-player-review/README.md);
native-size review found the footprint, glyph, health, facing, elevation,
stance, edge-state, and monochrome-pattern channels legible. Full
cross-runtime conformance, production client, browser smoke, documentation,
publication, and accessibility gates passed.

### Phase 2 — replay transport

- [x] Connect current worker projections to the SVG.
- [x] Add seek, step backward/forward, event navigation, checkpoint markers,
  cancellation, and progress.
- [x] Preserve all verification and divergence states.

**Exit:** full and perspective fixtures seek deterministically; disclosure
tests prove lost contact leaves no visual or accessible residue.

**Completed 2026-07-29:** worker protocol 3 carries bounded replay/render
projections into the SVG while leaving absent class, heading, elevation,
stance, overlay, and perspective-unit facts undisclosed. Loaded replay
transport includes exact projection ticks, bounded health, semantic edges,
event endpoints, and checkpoints. The shell supports reverse/forward step,
range seek, previous/next event navigation, correlated progress,
cancellation, and checkpoint jumps while preserving all verification and
failure states. Production-worker smoke fixtures prove repeatable full and
perspective seeks; the projection tick is the sole committed display source,
including duplicate-response mismatch regressions. Browser disclosure tests
prove lost contact removes unit geometry, footprints, hit targets, accessible
names, event controls and links, inspector selection, SVG selection, roving
focus, and detached DOM references. Full conformance, production browser,
documentation, publication, and accessibility gates passed.

### Phase 3 — overlays and playback polish

- [ ] Add exact selected overlays, semantic timeline lanes, deterministic
  interpolation, action traces, and reduced motion.
- [ ] Aggregate or decline whole-force overlays above the 8,000-path-segment
  limit while preserving precise selected-unit overlays.
- [ ] Add the optional second heading only with an accepted gameplay source.

**Exit:** exact-tick and interpolated modes converge on every committed frame;
two-heading and exact-overlay visual reviews pass.

### Phase 4 — simulation comparison and export

- [ ] Add fork workflow, linked split-view baseline/fork comparison,
  divergence inspection, and bookmarks.
- [ ] Export sanitized SVG and PNG evidence with source, replay, projection,
  palette, and renderer provenance.
- [ ] Capture end-to-end performance and visual-review provenance.

**Exit:** a derived simulation cannot be mistaken for verified replay, exports
carry complete provenance, and all normal/stress budgets have measured results.

## Acceptance criteria

The first documentation-ready player is acceptable when:

1. The battlefield remains flat and top-down at every supported camera
   rotation, without simulated camera elevation or 3D unit geometry.
2. Every unit uses a square information symbol, while a separate ground
   outline matches its authoritative footprint.
3. Exact class, faction, normalized health, and body facing remain readable at
   standard zoom.
4. The perimeter facing wedge is unambiguous for the supported heading model
   while the exact-class glyph remains upright.
5. The health track always has twelve positions, decreases monotonically, and
   remains distinguishable in all three palette presets.
6. Elevation zero has no mark; levels one through three have matching stepped
   bars; higher levels use a capped stack and detailed `+N` label.
7. Stance appears on the symbol only when disclosed and sufficiently close,
   with inspector access at other zoom levels.
8. Attention and optional secondary heading cannot be mistaken for facing.
9. A selected unit's exact visible geometry matches its disclosed projection.
10. Full, perspective, and sandbox modes remain explicitly labeled.
11. A lost hostile contact leaves no visual, DOM, accessibility, event-link,
    or hit-target residue beyond legitimately disclosed history.
12. Paused and stepped frames show exact committed ticks.
13. Interpolation never changes committed state or verification outcomes.
14. Seeking uses checkpoints and operation correlation without stale-response
    races.
15. Keyboard and pointer users can reach the same selection and inspection
    information.
16. The documented element inventory has no unexplained visual gaps.
17. Channel-capacity and target-size reviews are clean for representative
    rosters at the 24 px and 48 px thresholds.
18. Whole-force overlays aggregate or refuse rendering above 8,000 path
    segments without degrading precise selected-unit overlays.
19. Linked comparison opens in a persistently labeled split view.
20. The 200-unit normal view meets measured responsiveness targets.
21. Exported SVG and PNG carry provenance; SVG contains no executable or
    external replay-supplied content.

## Resolved design decisions

The prototype must validate these accepted choices rather than reopen them
without new evidence:

1. The SVG battlefield is flat, top-down, and documentation-only. Babylon.js
   owns the production 3D client.
2. Every exact class has its own primary glyph.
3. Height is secondary information. A corner elevation stack provides a
   compact non-color cue, while exact values remain in detailed labels and the
   inspector.
4. Stance may appear inside the square at detailed zoom; it does not alter
   footprint or simulated height.
5. Health uses twelve normalized inset perimeter dashes. Exact hit points
   appear at detailed zoom or in the inspector.
6. The initial palette set is accessible default, high contrast, and
   monochrome/pattern.
7. Body facing uses a perimeter wedge or notch while the class glyph remains
   upright.
8. Semantic zoom changes at projected symbol widths of 24 px and 48 px, with
   approximately 10% hysteresis.
9. Baseline/fork comparison defaults to a linked split view.
10. Whole-force overlays are capped at 8,000 path segments before aggregation
    or refusal.
11. The first release exports sanitized SVG and PNG evidence with provenance.

Prototype measurements may tune dimensions, stroke weights, dash spacing, and
the exact elevation-stack drawing without changing these channel assignments.

## Recommendation

Proceed with Phase 0 and a small Phase 1 prototype containing:

- one six-by-six top-down board;
- blocking and open semantic edges;
- eight square unit symbols spanning factions, exact classes, health,
  footprints, elevations, stances, and all eight facings;
- one selected exact attention/visibility polygon;
- one optional second heading;
- overview, standard, and detailed zoom tiers at the accepted thresholds;
- all three accepted palette presets; and
- keyboard selection plus the structured inspector.

Render that prototype at 23 px, 24 px, 47 px, 48 px, and a representative
detailed size. Run coverage, channel-capacity, accessibility, SVG-sanitization,
and PNG-fallback reviews, then tune only the S.I.R. state-to-glyph mapping and
declared visual tokens. That will test the decisive visual proposition—a flat,
reusable square symbol that explains authoritative replay state—before replay
transport and comparison features increase the surface area.
