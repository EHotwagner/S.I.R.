---
title: WASM Simulation and Tactical Planning Design
status: proposed
decision-status: proposed
document-type: architecture-and-roadmap
category: Engineering
categoryindex: 6
index: 15
version: "0.4"
created: 2026-07-29T19:27:00+02:00
last-updated: 2026-07-29
description: Design and milestone roadmap for converging the simulator, planning interface, control ABI, and authoritative deterministic kernel.
related:
  - docs/wasm-control-architecture.md
  - docs/control-abi.md
  - docs/simulation-core-architecture.md
  - docs/map-editor.md
  - docs/public-protocol-architecture.md
  - docs/svg-replay-player.md
---

# WASM Simulation and Tactical Planning Design

## Summary

S.I.R. should add tactical planning by converging the existing editor simulator,
shared deterministic kernel, Wasmtime host, and browser worker into one
execution path. A plan is a revisioned, editable source document compiled into
configuration for the standard per-unit WASM controller; it is not a privileged
second way to alter simulation state. Units expose three independent
directions—movement, body facing, and attention—while ordinary weapons follow
attention and engagements remain targets or commitments rather than a fourth
heading. The roadmap delivers this as a sequence of independently testable
vertical slices, ending with the same plan executing through native Wasmtime,
browser projection, and replay verification.

## Status and audience

This is a proposed subsystem design and implementation roadmap for contributors
working on the simulation, WASM host, map-editor simulator, replay system, and
browser client. It records the direction agreed on 2026-07-29, including the
initial ABI, timing, planning, and live-execution decisions. These proposed
decisions become canonical only after their milestone exit gates pass.

## Current baseline

The repository already proves several important pieces:

- `SIR.Simulation` is shared by .NET and Fable and produces conformant
  deterministic state and event bytes.
- `SIR.Match` can execute a bounded Wasmtime artifact and retain accepted WASM
  outputs in an authoritative replay.
- the replay worker provides bounded, correlated operations across a browser
  worker boundary;
- the map editor hands an immutable revision to a disposable simulator
  sandbox;
- the simulator provides routes, fixed-timestep movement, controllers, combat,
  stepping, and deterministic visual projections; and
- the renderer distinguishes body-facing presentation from other disclosed
  headings.

These are not yet one system:

```text
MapEditorSimulator ── owns editor-sandbox movement and combat

SIR.Simulation ────── owns the small cross-runtime authoritative kernel

MatchReplay ───────── owns the qualification-only decide(i32) WASM host

Replay worker ─────── owns replay and rules-experiment browser operations
```

Adding a substantial planning interface directly on top of
`MapEditorSimulator` would make its data model and behavior an accidental
second engine. The first architectural task is therefore convergence.

## Goals

The system must:

1. let a player plan coordinated actions for several units without writing
   code;
2. run those plans through the same per-unit control boundary intended for
   player-authored WASM;
3. keep movement, facing, and attention distinct and readable;
4. provide deterministic, revision-correlated validation and preview;
5. distinguish authored intent, accepted intent, predicted state, and committed
   authoritative state;
6. preserve knowledge filtering and simulated communications where the chosen
   mode requires them;
7. produce replay-driving accepted outputs and useful rejection diagnostics;
8. avoid client-side rules duplication; and
9. retain bounded performance at the intended 100-unit-per-side target.

## Non-goals

The initial system will not:

- introduce a separately commanded weapon direction;
- let plans mutate authoritative state directly;
- make a speculative preview a promise about hidden opposition;
- provide arbitrary user-defined server-side conditions outside WASM;
- expose unrestricted synchronous host calls;
- solve deployment, matchmaking, artifact upload, or live-session transport;
- require an explicit WEGO turn structure for the live game; or
- implement every capability, weapon, spell, formation, or logistics action in
  the first vertical slice.

## Three-direction model

### Shared direction type

The eight canonical compass directions should move from the editor-specific
`MapDirection` type into a shared domain type:

```fsharp
type Direction8 =
    | North
    | NorthEast
    | East
    | SouthEast
    | South
    | SouthWest
    | West
    | NorthWest
```

The wire representation is a closed integer code `0..7` in clockwise order
starting at north. Invalid values are rejected at decoding boundaries.

### Resolved orientation

The authoritative unit state exposes three distinct facts:

```fsharp
type ResolvedOrientation =
    { MovementDirection: Direction8 option
      BodyFacing: Direction8
      AttentionDirection: Direction8 }
```

`MovementDirection` is `None` while stationary and otherwise derives from the
currently resolving route segment. It is not an independently stored fourth
movement order that can disagree with the route.

Body facing and attention are explicit state. Their transitions consume time
through ordinary action rules. A unit can therefore:

- advance north while facing and attending north;
- advance north while facing north and attending west;
- withdraw north while facing and attending south; or
- strafe north while facing and attending east.

The initial ordinary-human descriptor turns body facing at 90 degrees per
second and attention at 180 degrees per second. At the authoritative 20 Hz rate,
a 45-degree change therefore takes ten body-facing ticks or five attention
ticks. These are versioned prototype content values rather than ABI constants.

The angle between movement, body, and attention affects movement rate,
readiness, acquisition, reaction, and capability legality through versioned
content descriptors. Exact modifiers remain balance parameters rather than ABI
constants.

All eight attention directions are legal for ordinary humans. Side and rear
attention incur descriptor-defined acquisition and reaction penalties, and
capabilities may impose stricter body-to-attention limits. Ordinary weapon
legality is therefore capability-specific rather than a universal attention
restriction.

### Orientation intent

Plans and control modules should express durable intent rather than recomputing
absolute headings in the client:

```fsharp
type FacingIntent =
    | KeepFacing
    | FaceFixed of Direction8
    | FaceAlongMovement
    | FaceKnownUnit of UnitId

type AttentionIntent =
    | KeepAttention
    | AttendFixed of Direction8
    | AttendRelativeToBody of Direction8
    | AttendAlongMovement
    | AttendKnownUnit of UnitId
    | AttendKnownArea of AreaReferent
```

`AttendRelativeToBody` uses the same eight-value type as a relative octant:
north means forward, west means left, east means right, and south means rear.
The plan compiler and control module resolve the relative octant against current
body facing each tick. The standard controller emits an absolute
`SetAttention` request; relative attention is not a separate Control ABI v1
request.

Known-unit and known-area intents never grant knowledge. If their referent is
lost, stale, invalid, or unavailable, the standard controller applies its
declared fallback behavior and the host reveals no replacement target.

### Weapon and engagement rule

Ordinary weapons follow `AttentionDirection`:

```text
ordinary weapon direction = attention direction
```

There is no independently commanded weapon heading. An engagement instead
declares a point or area target. Resolution checks:

- whether attention is aligned sufficiently with the target;
- whether body-to-attention offset is legal for the weapon and unit;
- preparation, traverse, stance, range, line, ammunition, and recovery;
- continued knowledge and target validity; and
- capability-specific interruption rules.

A unit may see something to its left without already being able to fire a
shouldered weapon at it. The required body turn or preparation remains an
ordinary timed transition. Turrets, panoramic sensors, unusual anatomy, and
magic are expressed as capability modifiers to the three-direction rules, not
as a universal fourth heading.

The presentation channel previously called `SecondaryHeading` should not become
authoritative orientation state. During migration it may visualize the resolved
attention direction when disclosure permits; its eventual name should reflect
that meaning.

## Plan document

### Boundary

`SIR-PLAN 1` is an editable source artifact. It belongs to authoring and
rehearsal, not to authoritative simulation state.

```fsharp
type PlanDocument =
    { FormatVersion: int32
      PlanId: Guid
      Revision: int64
      ParentDigest: byte array option
      MapRevisionDigest: byte array
      RulesetIdentity: string
      StartTick: int32
      HorizonTicks: int32
      UnitPlans: UnitPlan array }

type UnitPlan =
    { UnitId: int32
      ControllerArtifact: byte array
      Commands: PlannedCommand array
      Fallback: PlanFallback }
```

Canonical encoding sorts unit plans by unit ID and commands by stable command
ID. The plan digest covers all semantic fields but excludes presentation state
such as selection, expanded panels, timeline zoom, and camera position.
Player-authored annotations are also excluded from the semantic digest and
compiled controller configuration. A separate source-document digest includes
semantic fields and annotations so annotation-only edits participate in save
and conflict detection without changing execution identity.

The portable `SIR-PLAN 1` interchange is a strict, bounded, canonical UTF-8 line
grammar. Compiled per-unit controller configuration remains opaque binary.

### Commands

The initial command vocabulary is:

```fsharp
type PlannedCommandKind =
    | MovePath of Cell array * MovementPosture
    | SetFacingIntent of FacingIntent
    | SetAttentionIntent of AttentionIntent
    | SetStance of stanceId: string
    | EngageUnit of unitId: int32 * capabilityId: string
    | EngageArea of AreaReferent * capabilityId: string
    | Hold
    | Synchronize of markerId: string
```

Every command also carries:

- a stable command ID;
- an earliest-start constraint;
- optional predecessor command IDs;
- an interruption policy;
- a fallback transition; and
- player-authored annotation excluded from simulation semantics.

The plan does not author an authoritative duration. The compiler queries
versioned movement and capability descriptors and produces the earliest legal
schedule. A later ruleset may therefore change timing without permitting the
client to invent faster actions.

### Synchronization

A synchronization marker is a standard-controller coordination primitive, not
instantaneous global knowledge. Two forms must remain visibly distinct:

- **preloaded clock synchronization** waits until a shared tick or until all
  locally preloaded predecessor commands complete; and
- **acknowledged synchronization** waits for messages that have traversed the
  simulated communications system.

Every synchronization marker has a bounded absolute deadline. Its timeout
transition is one of `Continue`, `Hold`, `JumpTo` a stable command ID, or
`AbortUnitPlan`. Fallback jumps participate in dependency and cycle validation.

The first is suitable for a plan locked before execution. The second models
coordination issued or revised during a match. The UI must never present a sent
message as a confirmed state change.

### Compilation target

The plan compiler produces bounded opaque instance configuration for the
standard WASM controller:

```text
SIR-PLAN source
    → validate against map and ruleset
    → normalize paths, dependencies, and referents
    → compile one bounded configuration blob per unit
    → assign the standard controller artifact
```

This makes the no-code planner and player-authored controllers peers at the
same host boundary. The simulation does not receive a privileged `ExecutePlan`
kernel input.

## Control ABI v1

### Module surface

The first production ABI should use one bulk input copy and one bulk output
read:

```text
exports:
  memory
  sir_abi_version() -> i32
  sir_input_ptr() -> i32
  sir_input_capacity() -> i32
  sir_output_ptr() -> i32
  sir_output_capacity() -> i32
  sir_decide(input_length: i32) -> i32
```

`sir_decide` returns a non-negative output length or a negative stable module
status code. A trap, fuel exhaustion, invalid length, malformed output, or
forbidden request rejects the complete invocation atomically.

ABI v1 should have no gameplay host imports. Cheap queries can be answered from
the immutable input. Expensive work is requested in output and delivered as a
later `ServiceResult` event. This avoids re-entrancy and makes fuel and replay
accounting straightforward.

### Binary envelope

Both input and output use canonical little-endian byte layouts:

```text
fixed header
  magic
  ABI major/minor
  total byte length
  tick
  unit ID
  flags and budget summary

tagged bounded sections
  own state
  resolved orientation
  action and recovery
  observations and stimuli
  events
  messages and reports
  capability descriptors
  service results
  request status
```

Each section has a tag, byte length, element count, and canonical element
layout. Unknown optional tags can be skipped within the same ABI major version;
unknown required tags reject the invocation. All offsets, counts, strings, and
payloads are independently bounded before allocation.

ABI v1 uses one 16-bit section-tag registry. Required versus optional is a
separate section flag:

| Tag | Section |
|---|---|
| `0x0001` | own state |
| `0x0002` | resolved orientation |
| `0x0003` | action and recovery |
| `0x0004` | observations and stimuli |
| `0x0005` | events |
| `0x0006` | messages and reports |
| `0x0007` | capability descriptors |
| `0x0008` | service results |
| `0x0009` | request status |
| `0x1001` | output requests |

Minor versions may append bounded optional sections. A required semantic change
requires a new ABI major version.

The initial execution profile limits each invocation to 64 KiB of input, 16 KiB
of output, 32 sections, 256 elements per section, 255 UTF-8 bytes per string,
and 4 KiB per opaque payload. Increasing a limit requires a new qualified
execution profile.

Output contains an ordered list of requests from the existing control
vocabulary: movement intent, facing, attention, stance, engagement,
capability, cancellation, messages, services, emission, formation, and sleep.
Every request has a module-local ID so acceptance and later outcome events can
be correlated.

Control ABI v1 exposes all path searches through metered asynchronous
`RequestService` operations whose results arrive on a later tick. Immutable
input contains the cheap local occupancy and simple geometry needed for local
decisions; there is no synchronous pathfinding host import.

### Instance state

Each controlled unit owns an isolated instance with:

- immutable artifact identity;
- immutable initial configuration;
- linear memory and declared mutable globals;
- pending service handles;
- wake state;
- fault state; and
- execution-profile identity.

Snapshots and authoritative replay verification retain sufficient state to
resume the exact instance. Compiled native code remains derived cache state.

## Authoritative host pipeline

One tick follows:

```text
commit previous tick
  → derive knowledge-filtered unit input
  → copy canonical ABI input
  → invoke eligible unit instance with fresh fuel
  → decode complete output
  → validate requests against knowledge and capabilities
  → order accepted intentions canonically
  → execute shared simulation phases
  → commit state, events, hashes, and accepted-output journal
```

The host owns validation and ordering; WASM owns decisions. Neither the planner
nor the browser worker can modify state.

The current editor simulator's unit-by-unit mutation order is not the final
authority model. Its route and combat behavior must migrate behind the shared
kernel's explicit phases before the planner claims authoritative parity.

## Simulator session protocol

The simulator needs a worker protocol separate from replay inspection:

```fsharp
type SimulatorRequest =
    | Initialize of SimulatorInitialization
    | ValidatePlan of PlanDocument
    | PreviewPlan of planRevision: int64 * fromTick: int32 * toTick: int32
    | CommitPlan of planRevision: int64
    | Step of expectedTick: int32 * tickCount: int32
    | RunTo of expectedTick: int32 * targetTick: int32
    | Reset of mapRevisionDigest: byte array
    | Cancel of operationId: int32
```

Responses carry:

- operation, session, map, and plan revision identities;
- current committed simulation tick;
- validation issues with stable codes and affected command IDs;
- accepted or rejected plan revision;
- a bounded knowledge-scoped projection snapshot or delta;
- authoritative events and request outcomes;
- preview provenance and assumptions; and
- progress or cancellation state.

`expectedTick`, plan revision, and map digest make stale operations explicit.
The worker never silently applies a response to a different plan revision.

### Preview classifications

Every preview is labeled as one of:

- **deterministic rehearsal** — all participating controller artifacts and
  initial state are known;
- **assumption-based rehearsal** — unknown opposition uses an explicitly named
  scripted assumption; or
- **intent-only projection** — shows routes and scheduled commitments without
  predicting opposition outcomes.

The UI must not render assumption-based or intent-only results as committed
future state.

## Planning interface

### Layout

The initial desktop workspace should use:

```text
┌─────────────┬───────────────────────────────┬──────────────────┐
│ unit roster │ battlefield and plan overlays │ command inspector│
├─────────────┴───────────────────────────────┴──────────────────┤
│ time ruler │ unit lanes │ command blocks │ events/conflicts   │
└────────────────────────────────────────────────────────────────┘
```

- **Unit roster:** controller assignment, health, role, current command,
  validation state, and visibility/filter controls.
- **Battlefield:** direct route drawing, waypoint editing, body-facing pip,
  attention indicator, engagement targets, sync markers, and conflict overlays.
- **Inspector:** exact command constraints, fallback, posture, capability, and
  diagnostic reason codes.
- **Timeline:** one lane per selected unit, dependency lines, synchronization
  markers, predicted timing ranges, and committed event markers.

### Three-direction interaction

The planner presents:

1. a route arrow for movement;
2. the large perimeter pip for body facing; and
3. a distinct attention arrow or sector.

Facing and attention controls offer durable modes such as fixed, follow
movement, relative to body, or track known target. Selecting an engagement
normally proposes the required attention intent, but it does not create another
direction handle.

The plan remains usable from the keyboard. Unit lanes and battlefield commands
share selection, all handles have labeled inspector equivalents, and animation
obeys reduced-motion preferences. Persistent pulsing is not used for ordinary
orientation; temporary conflict or changed-state emphasis may pulse only when
motion is permitted.

### State language

The interface visually distinguishes:

- **authored** — editable plan source;
- **valid** or **invalid** — compiler result;
- **accepted** — configuration or order accepted by the simulator host;
- **predicted** — derived rehearsal projection;
- **committed** — authoritative simulation state; and
- **reported** — knowledge delivered through simulated communication.

These states must differ by labels and shape or pattern, not color alone.

## Continuous execution and WEGO

The first implementation is a paused planning and rehearsal tool for continuous
per-tick execution. It does not introduce mandatory global planning windows.

The plan format uses a bounded horizon and simultaneous controller evaluation,
so it does not prevent a later game mode from adding:

```text
planning window → lock plans → execute N ticks → disclose results → repeat
```

Choosing that loop for live play is a separate game-design decision. It would
change interruption, communications, input deadlines, and multiplayer pacing
and should not be smuggled in as a simulator UI implementation detail.
Continuous per-tick execution is canonical for the planned live integration.
Any WEGO variant is a separate optional mode requiring its own design and
qualification evidence.

## Validation and diagnostics

Validation is layered:

1. **structural:** format, bounds, IDs, ordering, dependency cycles;
2. **map:** unit ownership, cells, edges, footprints, referents;
3. **ruleset:** capability, target shape, stance, inventory, timing;
4. **controller:** artifact compatibility and configuration size;
5. **schedule:** impossible dependencies, horizon overflow, synchronization;
6. **runtime:** knowledge, interruption, changed world, communication, and
   resource availability.

Every failure has a stable code and structured fields. Human-readable prose can
change without breaking modules or tests.

## Determinism, replay, and disclosure

- Plan encoding and compilation are deterministic.
- The plan digest, compiler version, standard-controller artifact, ABI,
  ruleset, content, and execution profile are pinned.
- Accepted WASM requests—not plan commands—enter the authoritative journal.
- Authoritative verification re-executes the exact module instances.
- Browser playback can inject recorded accepted requests into the Fable kernel
  without claiming native WASM fuel verification.
- Full replay and perspective replay remain separate disclosure products.
- A preview never receives hidden state merely because the server possesses it.

## Security and resource bounds

The existing WASM sandbox constraints remain. The planning additions also
bound:

- units and commands per plan;
- path points and dependencies per command;
- plan horizon;
- configuration bytes per unit;
- preview ticks and concurrent preview operations;
- projection bytes and messages;
- validation issue count; and
- retained plan revisions.

An editable plan is limited to 6,000 ticks, or five minutes at 20 Hz. One
preview request is limited to 1,200 ticks, or 60 seconds. Longer rehearsals use
bounded committed or chunked continuation rather than an oversized request.

Plan annotations are untrusted text. They never enter WASM configuration, and
they are escaped in HTML/SVG exports.

## Roadmap

Milestone status uses `[x] 🟩` for implemented work with evidence and `[ ] ⬜`
for planned work whose exit gate has not yet passed.

### [x] 🟩 Milestone 0 — presentation and roster baseline

Status: implemented with this design report.

- enlarge and increase contrast of body-facing pips;
- expose all six existing human personnel glyphs and both drone roles in the
  editor unit palette;
- complete the seven-role prototype weapon-profile table; and
- regenerate deterministic visual and map-editor review evidence.

Exit gate:

- native/Fable conformance, browser smoke, accessibility, map-editor
  qualification, replay, and WASM qualification all pass.

### [x] 🟩 Milestone 1 — shared orientation domain

Status: implemented on 2026-07-29.

- introduce `Direction8`, `ResolvedOrientation`, `FacingIntent`, and
  `AttentionIntent` in the shared domain;
- migrate `MapDirection` serialization through an explicit compatibility
  adapter;
- add body facing and attention to authoritative unit state;
- derive movement direction from the active movement segment;
- update canonical encoding, state hashes, fixtures, replay versioning, and
  projection transport; and
- render attention as the third disclosed direction.

Exit gate:

- all 64 body/attention combinations and all movement-relative combinations
  round-trip identically in .NET and Fable;
- old map documents import deterministically with documented defaults; and
- no weapon-heading state is introduced.

Evidence:

- the shared .NET/Fable orientation fixture covers every body/attention pair,
  every body-relative octant pair, all eight movement octants, and rejects
  invalid direction codes;
- `SIR-MAP 1` and `SIR-MAP 2` imports default body facing and attention to
  north before canonical `SIR-MAP 3` export;
- replay format 2 includes authoritative body facing and attention while
  replay format 1 remains readable with deterministic north defaults;
- the projection transport and map renderer disclose attention independently
  from body facing; legacy weapon and sensor headings remain presentation-only
  compatibility cases; and
- the full native/Fable conformance, replay, browser smoke, worker,
  map-editor qualification, accessibility, and deterministic evidence gates
  pass.

### [x] 🟩 Milestone 2 — shared map-scale kernel

Status: implemented on 2026-07-29.

- move fixed-timestep movement, square-footprint collision, terrain cost, and
  semantic-edge traversal from `MapEditorSimulator` into `SIR.Simulation`;
- replace sequential controller mutation with explicit collect, validate,
  resolve, and commit phases;
- migrate typed combat profiles and engagement state;
- retain the editor simulator as orchestration and presentation only; and
- establish phase-checkpoint divergence diagnostics for map-scale scenarios.

Exit gate:

- editor and headless runs consume the same kernel;
- simultaneous destination and crossing conflicts have canonical fixtures; and
- no authoritative numeric or rules logic remains in the web project.

Evidence:

- `SIR.Simulation.MapScale` owns fixed-timestep movement credit,
  square-footprint collision, terrain cost, semantic-edge traversal,
  deterministic pathfinding, typed combat profiles, and engagement recovery;
- every map-scale tick produces collect, validate, resolve, and commit
  checkpoints, while the editor simulator only converts, orchestrates,
  narrates, and interpolates shared-kernel results;
- native/Fable canonical fixtures reject same-destination and crossing/swap
  conflicts symmetrically from the pre-tick snapshot and identify the first
  divergent tick, phase, and byte; and
- the full conformance suite passes with exact .NET/Fable output, browser
  smoke, replay, worker, accessibility, regenerated map-editor evidence, and
  zero build warnings.

### [ ] ⬜ Milestone 3 — Control ABI v1 codec

- implement the assigned ABI tags and bounds and assign the remaining integer
  codes and stable failure codes;
- implement canonical input/output codecs in a runtime-neutral project;
- publish generated constants and bindings;
- add malformed, unknown-tag, maximum-size, and canonical-order vectors; and
- freeze ABI v1 conformance fixtures.

Exit gate:

- F#, a small reference WASM module, and a standalone decoder agree byte for
  byte;
- fuzz/property tests cannot escape declared bounds; and
- the ABI specification contains no editor-only types.

### [ ] ⬜ Milestone 4 — reusable Wasmtime control host

- extract the qualification host into a reusable match subsystem;
- compile artifacts once and instantiate isolated per-unit stores;
- implement bulk copy, output validation, fuel, memory limits, trap handling,
  sleep, and snapshot state;
- feed accepted requests into the map-scale kernel; and
- journal requests, failures, budgets, and module state required for replay.

Exit gate:

- 200 standard-controller instances remain within the declared tick budget;
- fuel exhaustion and malformed output apply nothing atomically;
- snapshot/resume reproduces outputs and hashes; and
- ambient WASI and memory-growth limits are explicitly qualified.

### [ ] ⬜ Milestone 5 — `SIR-PLAN 1` and standard controller

- implement plan domain types, canonical encoding, digest, validation, and
  dependency scheduling;
- compile per-unit plan tracks to bounded standard-controller configuration;
- implement movement, facing, attention, hold, one point engagement, and
  synchronization;
- provide readable reference source for the standard controller; and
- build adversarial rehearsal fixtures.

Exit gate:

- a two-unit coordinated plan produces identical accepted requests across
  repeated native runs;
- invalid and cyclic plans produce stable command-scoped diagnostics; and
- the standard controller uses only the public ABI.

### [ ] ⬜ Milestone 6 — simulator worker protocol

- add session initialization, plan validation, preview, commit, step, run-to,
  reset, progress, and cancellation;
- correlate every operation with session, map, plan revision, and tick;
- provide deterministic, assumption-based, and intent-only preview labels;
- reuse bounded projection snapshots/deltas; and
- prevent stale worker responses from changing the active workspace.

Exit gate:

- structured-clone round trips cover every request and response;
- cancellation and stale revision tests pass;
- a normal planning horizon respects projection-message and elapsed-time
  budgets; and
- preview disclosure tests receive no hidden state.

### [ ] ⬜ Milestone 7 — planning workspace

- add roster, battlefield planning tools, timeline lanes, inspector, and
  validation navigation;
- support route, facing, attention, stance, hold, engagement, and sync editing;
- add undo/redo and plan revision identity;
- support full keyboard operation, touch targets, reduced motion, forced
  colors, and 400% responsive layout; and
- export deterministic review artifacts for plan, conflict, and execution
  states.

Exit gate:

- a representative coordinated plan can be authored without editing text;
- every pointer operation has a keyboard/inspector equivalent;
- the interface never conflates authored, predicted, accepted, and committed
  state; and
- accessibility and performance qualification pass at the intended roster
  size.

### [ ] ⬜ Milestone 8 — capability and roster integration

- attach explicit loadouts and capability descriptors to authored units;
- execute the seven prototype human weapon roles through generic capability
  requests;
- add traverse, preparation, attention alignment, area engagement, ammunition,
  and interruption;
- make role and equipment selection visible in planning diagnostics; and
- add arcane capabilities only after their descriptors are accepted.

Exit gate:

- each weapon role changes a planning or positional decision;
- no weapon requires a new ABI request kind or fourth direction; and
- point and area engagements reproduce across replay verification.

### [ ] ⬜ Milestone 9 — end-to-end qualification and live integration

- run one map and plan through editor handoff, plan compiler, standard WASM,
  native host, shared kernel, projection, browser playback, and authoritative
  replay verification;
- measure full tick, preview, serialization, worker, and rendering budgets;
- connect accepted planning artifacts to match lock and session admission; and
- keep WEGO outside this integration unless a separate mode design is accepted.

Exit gate:

- the vertical slice has one authoritative implementation path;
- all pinned identities appear in replay and diagnostics;
- disclosure and reconnect behavior pass adversarial tests; and
- the architecture documents are updated from proposed to accepted only for
  the portions demonstrated by evidence.

## Recommended implementation order

The critical path is Milestones 1–6. The interface should begin only after the
plan compiler and simulator worker can provide real validation and previews.
Visual prototypes may precede that work, but they must use inert fixtures and
must not establish a competing execution model.

The smallest meaningful end-to-end slice is:

```text
two units
  + movement path
  + body facing
  + attention
  + hold
  + one rifle engagement
  + one synchronization marker
  → standard WASM controller
  → shared kernel
  → preview
  → replay
```

This slice exercises every architectural boundary without requiring the full
content catalog.

## Risks and mitigations

| Risk | Mitigation |
|---|---|
| Planner becomes a second engine | Compile plans to the public standard controller and keep rules in the shared kernel |
| Preview promises impossible certainty | Label preview class and preserve knowledge filtering |
| Direction controls become unreadable | Limit authority to movement, body, and attention; derive weapon use |
| ABI grows with every capability | Keep generic request kinds and versioned descriptors |
| Timeline authors impossible durations | Store constraints; let descriptors and the compiler schedule |
| Mid-match edits bypass communications | Treat them as ordinary orders, not configuration mutation |
| Browser claims native WASM verification | Preserve the existing verification-level distinction |
| Migration breaks maps and replays | Version formats and provide explicit deterministic adapters |
| Hundreds of animated indicators create noise | Use static high-contrast orientation and bounded transient emphasis |

## Resolved decisions

The initial decisions are:

- versioned ordinary-human descriptors use 90-degree-per-second body turning
  and 180-degree-per-second attention turning;
- rear attention is legal but penalized, while capability descriptors own
  stricter angular limits;
- the standard controller resolves relative attention to absolute ABI requests;
- `SIR-PLAN 1` uses canonical bounded UTF-8 text and compiles to opaque binary
  unit configuration;
- ABI v1 uses the assigned 16-bit tag registry, compact invocation bounds, and
  append-only optional minor-version evolution;
- pathfinding is an asynchronous metered service;
- synchronization uses an absolute deadline and the four declared timeout
  transitions;
- annotations affect a source-document digest but not the semantic plan digest;
- editable and preview horizons are limited to 6,000 and 1,200 ticks
  respectively; and
- continuous execution is canonical, with WEGO reserved for a separately
  designed optional mode.

## Decision checkpoints

Before Milestone 3 starts, confirm the selected ABI memory and encoding shape
against conformance vectors. Before Milestone 5 starts, confirm the plan command
and fallback vocabulary against adversarial fixtures. Before Milestone 7
starts, review an inert interaction prototype and the real worker responses
together. Before Milestone 9 connects to live sessions, qualify continuous
execution; a separate WEGO mode requires a new design decision.
