---
title: Codebase Map
category: Engineering
categoryindex: 6
index: 3
status: accepted
decision-status: canonical
document-type: living-architecture
version: "0.5"
last-updated: 2026-07-28
related:
  - docs/game-vision.md
  - docs/technology-stack.md
  - docs/simulation-core-architecture.md
  - docs/skirmish-development-plan.md
  - docs/wasm-control-architecture.md
  - docs/public-protocol-architecture.md
  - docs/fable-client-and-documentation.md
---

# F# Codebase Architecture

## Decision status

This is the canonical initial F# solution and dependency architecture.

The proposal deliberately starts with a small number of cohesive assemblies.
Gameplay subsystems begin as F# modules and namespaces within those assemblies.
They become separate projects only when an actual dependency, deployment,
packaging, or build-time boundary justifies the split.

## Canonical solution layout

```text
S.I.R.slnx
Directory.Build.props
Directory.Packages.props
Directory.Packages.local.props
global.json

src/
├── SIR.Domain/
├── SIR.Simulation/
├── SIR.Wasm/
├── SIR.Match/
├── SIR.Protocol.Generated/
├── SIR.Protocol/
├── SIR.Server/
├── SIR.Client/
├── SIR.Replay.Web/
└── SIR.Tools/

tests/
├── SIR.Domain.Tests/
├── SIR.Simulation.Tests/
├── SIR.Wasm.Tests/
├── SIR.Protocol.Tests/
├── SIR.Integration.Tests/
├── SIR.Conformance.Tests/
└── SIR.Performance/

scenarios/
├── canonical/
├── fixtures/
└── benchmarks/

modules/
├── standard/
├── examples/
└── test-fixtures/

schemas/
├── protocol/
├── replay/
└── content/
```

Names use the conventional .NET namespace form `SIR` because punctuation in
identifiers and package names adds friction. The product and repository retain
the display name `S.I.R.`.

## Project responsibilities

### `SIR.Domain`

Owns stable game vocabulary and rules-facing values:

- typed identifiers;
- grid coordinates, canonical cell edges and vertices, square footprints,
  facing, and fixed-point value types;
- edge feature types and their per-modality permeability contracts;
- ruleset and content identifiers;
- observations, reports, orders, intentions, and authoritative events;
- action and effect descriptions;
- resources, equipment, and objective definitions;
- public error and validation types; and
- deterministic keys, including random-sample purposes; and
- runtime-neutral Control ABI framing, bounds, integer registries, and
  canonical codecs.

It contains no sockets, database access, rendering, process clock, concrete
WASM runtime, or server framework. It should be inexpensive to reference from
tests and tools.

Public surfaces use `.fsi` signature files where they improve contract clarity.
Construction functions enforce invariants so invalid states are difficult to
represent.

### `SIR.Simulation`

Owns the authoritative deterministic state transition:

- 20 Hz integer tick state and phase pipeline;
- component and entity storage;
- cell occupancy, edge feature state, reservations, and cooperative
  square-footprint movement;
- Chebyshev path-cost adapters and edge-aware eight-way traversal;
- LOS, FOV, perception, acquisition, and actor knowledge;
- actions, reactions, combat resolution, and consequence batching;
- communications and report delivery;
- logistics and objective state;
- counter-addressed randomness;
- deterministic snapshot and state hashing; and
- cache interfaces and invalidation rules.

It depends on `SIR.Domain` and selected compatible primitives from
`FS.GG.Game.Core`. It does not depend on the concrete WASM runtime. Module
invocation requests and validated module outputs cross typed ports.

### `SIR.Wasm`

Owns the concrete control-module execution adapter:

- module validation and artifact identity;
- compilation and compiled-artifact reuse;
- isolated per-unit instance state;
- ABI memory transfer, semantic translation, and codec integration;
- fuel, memory, and host-call limits;
- deterministic host imports;
- wake scheduling;
- trap and fault normalization;
- asynchronous deterministic service requests; and
- execution metrics that cannot affect outcomes.

It depends on `SIR.Domain` and the selected .NET WASM runtime. Runtime-specific
types do not escape its public boundary.

### `SIR.Match`

Owns authoritative orchestration around one running match:

- mode-manifest validation;
- force and module locking;
- input journal and target-tick assignment;
- ordered coordination of simulation and WASM execution;
- snapshots, replays, resynchronization, and first-divergence diagnostics;
- participant-to-knowledge projection;
- mission state transitions and result records; and
- headless match lifecycle.

It is the composition point for `SIR.Simulation` and `SIR.Wasm`. This avoids
making the pure simulation project depend directly on a native runtime while
still treating module state and outputs as authoritative match data.

### `SIR.Protocol`

Owns versioned external representations:

- connection and capability negotiation;
- public match and mode descriptions;
- player input envelopes;
- knowledge-filtered projections;
- module upload and selection messages;
- snapshot, replay, and result envelopes;
- schema-version and compatibility rules; and
- conversions between wire records and validated domain values.

Wire types do not become authoritative domain types merely for convenience.
Deserialization produces untrusted data that must pass explicit conversion and
validation.

The project uses the selected FS.GG.Net serialization components but does not
depend on a concrete server host.

The subordinate `SIR.Protocol.Generated` project contains only committed code
generated reproducibly from the canonical `.proto` schemas. `SIR.Protocol`
depends on it and owns validation, limits, compatibility, and domain mapping.
No other project treats generated wire records as authoritative domain state.

### `SIR.Server`

Owns the canonical service and operational composition root:

- selected FS.GG.Net server transports;
- authentication and authorization;
- account and artifact services;
- match admission and process coordination;
- connection-specific projection delivery;
- persistence adapters;
- rate, size, and abuse limits;
- health, logging, and metrics endpoints; and
- configuration and deployment wiring.

It delegates all game outcomes to `SIR.Match`. It cannot mutate simulation
state through an internal path unavailable to the public protocol.

### `SIR.Client`

Owns the canonical client:

- FS.GG.Rendering/MVU application state;
- FS.GG.Net client transport;
- projection decoding and local presentation state;
- camera, selection, overlays, hotkeys, and accessibility;
- communication with the player's HQ WASM instance through the public
  protocol; and
- animation and interpolation between committed server projections.

The client has no authoritative simulation dependency. It may reference
`SIR.Protocol` and presentation-safe value types, but not use server-only state
to reconstruct hidden information.

### `SIR.Replay.Web`

Owns the static Fable browser replay and interactive simulation surface:

- compiles the shared deterministic `SIR.Domain` and `SIR.Simulation` source;
- uses Elmish MVU from the first browser slice, with React rendering and a
  typed F# view DSL;
- decodes and validates version-matched replay packages;
- injects recorded accepted WASM control outputs without claiming to reproduce
  Wasmtime execution;
- restores checkpoints, seeks, and reports first-divergence diagnostics;
- plays knowledge-filtered projection packages when complete state is not
  authorized; and
- provides presentation and parameter controls that cannot alter the canonical
  replay record.

This project is an explicit replay host, not the live canonical client and not
an authority. It does not reference `SIR.Wasm`, server internals, native
transports, or hidden live-match state. Wire decoding maps untrusted replay data
to validated shared-domain values before the kernel consumes it. Large replay
and sweep execution crosses a typed browser-worker boundary; the Elmish model
keeps bounded presentation projections rather than copying the full world into
React state on every tick.

### `SIR.Tools`

Owns developer and operator commands:

- headless scenario execution;
- replay inspection and verification;
- state-hash comparison;
- content and manifest validation;
- module inspection and conformance;
- deterministic simulation batching; and
- benchmark and profiling entry points.

Commands compose library entry points. Test logic should remain in test
projects rather than becoming production command behavior.

## Dependency graph

```text
                    FS.GG.Game.Core
                           │
SIR.Domain ─────────► SIR.Simulation
     │                      │
     ├──────────────► SIR.Wasm
     │                      │
     └──────────────► SIR.Protocol ◄── SIR.Protocol.Generated

SIR.Simulation ──┐
                 ├──► SIR.Match ───────┐
SIR.Wasm ────────┘                     │
                                       ├──► SIR.Server ◄── FS.GG.Net server components
SIR.Protocol ──────────────────────────┘

SIR.Protocol ───────────► SIR.Client ◄── FS.GG.Net client components
                               ▲
                               └──────── FS.GG.Rendering

SIR.Domain + SIR.Simulation ───► SIR.Replay.Web ───► Fable + Elmish/browser

Domain + Simulation + Wasm + Match + Protocol ───► SIR.Tools
```

The graph must remain acyclic. In particular:

- `Domain` does not reference `Simulation`;
- `Simulation` does not reference `Wasm`, `Protocol`, or `Server`;
- `Protocol` does not reference `Server` or `Client`;
- `Match` does not reference `Protocol` or a transport;
- `Server` does not become a gameplay-rules assembly; and
- the live `Client` does not reference `Simulation`, `Wasm`, or server
  internals; and
- `Replay.Web` may reference the shared simulation but not `Wasm`, `Match`,
  `Server`, or privileged live services.

## Internal F# organization

Within an assembly, files follow dependency order rather than feature discovery
order:

```text
AssemblyInfo.fs
Prelude.fs
Identifiers.fsi / Identifiers.fs
Values.fsi / Values.fs
Commands.fsi / Commands.fs
Events.fsi / Events.fs
State.fsi / State.fs
...
PublicApi.fsi / PublicApi.fs
```

Large gameplay areas use feature directories or filename prefixes without
immediately becoming assemblies:

```text
Spatial/
Perception/
Movement/
Combat/
Communications/
Logistics/
Objectives/
```

Each area should expose a narrow public facade and keep storage or algorithm
details internal. Cross-feature operations should pass typed facts,
intentions, candidates, deltas, and events through the declared tick pipeline
rather than directly reaching into another module's mutable storage.

## Package and sibling-reference policy

The solution uses:

- `global.json` to pin the .NET SDK feature band;
- central package management for all external versions;
- the FS.GG shared build baseline where applicable;
- a S.I.R. local package file for S.I.R.-specific dependencies; and
- locked restore in CI and release builds.

Normal builds consume pinned FS.GG packages. A deliberate local-development
property may replace a package with an explicit sibling project reference, but:

- the mode is opt-in and visible in build output;
- every expected sibling path is validated before restore or build;
- mixed package/project references for the same component fail fast;
- CI does not rely on the sibling directory layout; and
- a dependency tested from a sibling is pinned to an immutable commit in the
  resulting evidence.

The current local FS.GG repositories do not yet constitute a chosen coherent
S.I.R. version set. The initial implementation task must resolve and record that
set rather than copying whatever versions happen to be checked out.

## Test architecture

### Unit and property tests

- Domain constructor and invariant tests.
- Chebyshev, footprint, facing, and fixed-point properties.
- Edge canonicality: one representative per boundary, symmetric permeability,
  and consistent cell/edge/vertex relationships.
- Edge-crossing agreement between movement, line of sight, and shot traces.
- Deterministic random-address stability.
- Phase-local resolution and permutation invariance.
- Cache equivalence and invalidation.
- Protocol conversion and hostile-input rejection.
- WASM budget, memory, trap, and isolation behavior.

### Canonical scenario tests

Scenario manifests in `scenarios/canonical/` are data rather than test source.
They execute through the same `SIR.Match` entry point as a hosted game and pin:

- initial content, rules, state, and modules;
- input journal;
- significant event sequence;
- periodic state hashes; and
- final result.

### Conformance tests

The public server and module surfaces have implementation-independent
conformance suites. They can test:

- the canonical server;
- a custom or third-party server;
- standard and player-provided WASM modules; and
- custom clients' protocol behavior where a headless client is sufficient.

Conformance does not promise support for modified servers. It makes the
canonical contracts executable and auditable.

The cross-runtime suite also compiles the shared kernel for .NET and Fable,
runs identical canonical fixtures through both, and requires exact checkpoint,
event, and final-state digests. It validates the browser-published bundle as
well as the development JavaScript build. A mismatch reports the first
divergent tick and phase.

### Performance tests

Performance evidence is separate from correctness tests and cannot weaken
deterministic assertions. Benchmarks grow through declared scale gates:

1. tiny deterministic scenarios;
2. 10 units per side;
3. 50 units per side;
4. 100 units per side; and
5. stress cases above the supported target.

Measurements report simulation, WASM, projection, serialization, and client
rendering costs separately.

## First executable vertical slice

The first slice should be small in content but cross every foundational
boundary:

1. Load a versioned scenario containing a bounded grid, a cell-occupying
   blocker, a wall edge, a window edge, a door edge, and two opposing 2×2
   units.
2. Run a headless authoritative match at 20 ticks per second with eight-way
   facing and equal-cost Chebyshev movement.
3. Instantiate one isolated WASM instance per unit from reusable compiled
   artifacts.
4. Deliver a local observation, accept a movement or attention intention, and
   apply fuel and memory limits.
5. Resolve footprint movement, LOS, acquisition, and one minimal attack through
   deterministic phases, with the movement blocked by the wall edge and the
   attack resolved through the window edge.
6. Emit knowledge-filtered projections through one public transport.
7. Render the square units, footprint, facing, identification glyph, and HP in
   the canonical client.
8. Save the input journal, module identities, events, periodic state hashes,
   and final result.
9. Replay the match headlessly and prove identical hashes.

The slice is successful when the server, a headless client, and the canonical
client all use the same public protocol, and when changing presentation cannot
change the replay.

This intentionally includes WASM, networking, projection filtering, rendering,
and replay early. Omitting any one of them would postpone a central
architectural risk until after the simulation had already grown around an
untested assumption.

## Implementation order

1. Pin SDK, central packages, FS.GG compatibility set, formatting, analyzers,
   and deterministic build metadata.
2. Create `Domain`, `Simulation`, and their invariant/property test projects.
3. Implement the minimal grid, edge layer, footprint, tick, phase, hash, and
   scenario loader.
4. Select the WASM runtime and implement the smallest metered ABI through
   `SIR.Wasm`.
5. Add `SIR.Match`, input journal, module scheduling, replay, and the first
   canonical scenario.
6. Define `SIR.Protocol` and implement one FS.GG.Net transport end to end.
7. Add the minimal FS.GG.Rendering client.
8. Add determinism, protocol, WASM, and .NET/Fable parity conformance gates,
   then publish the first version-bound browser replay bundle.
9. Pass the 10-unit and then 50-unit-per-side performance gates before
   expanding combat content.
10. Grow the slice system by system according to the skirmish development plan.

## Alternatives considered

### One project per gameplay subsystem

Rejected initially. It gives strong physical boundaries but produces many
assemblies, conversion surfaces, and F# compile-order constraints before the
real dependency graph is understood. A subsystem should split out later only
when evidence justifies it.

### One monolithic application project

Rejected. It makes headless testing, custom-client parity, deterministic
boundaries, and runtime isolation harder to enforce.

### Client sharing the authoritative simulation

Rejected. It encourages hidden-state reconstruction and privileged client
behavior. The client shares protocol and presentation-safe values instead.

### Server directly invoking simulation and WASM throughout request handlers

Rejected. A transport-independent `SIR.Match` boundary is necessary for
single-player parity, headless scenarios, replays, and alternate server hosts.

## Decisions still required

- Pin the validated version of the
  [canonical Wasmtime runtime](research/wasm-runtime-selection.md).
- Choose the first persistence technology; it remains outside the match kernel.
- Define the exact local sibling-reference switch and coherent FS.GG baseline.
