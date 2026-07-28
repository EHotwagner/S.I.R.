---
title: S.I.R. Technology Stack and FS.GG Integration
status: proposed
document-type: living-architecture
version: "0.13"
last-updated: 2026-07-28
related:
  - docs/game-vision.md
  - docs/simulation-core-architecture.md
  - docs/skirmish-development-plan.md
  - docs/wasm-control-architecture.md
  - docs/codebase-architecture.md
  - docs/research/wasm-runtime-selection.md
  - docs/research/public-transport-selection.md
  - docs/public-protocol-architecture.md
  - docs/cross-runtime-replay.md
---

# S.I.R. Technology Stack and FS.GG Integration

## Canonical decision

S.I.R. is implemented in F# on .NET 10 and uses the FS.GG framework family.
This is a selected architectural foundation, not an open technology candidate.

The primary FS.GG repositories are cloned as sibling repositories of S.I.R.
during local development:

```text
projects/
├── S.I.R/
├── FS.GG.Game/
├── FS.GG.Net/
├── FS.GG.Rendering/
├── FS.GG.Audio/
├── FS.GG.SDD/
├── FS.GG.Governance/
└── FS.GG.Templates/
```

Sibling availability makes source inspection, debugging, coordinated
development, and temporary project-reference testing practical. It does not
make the current contents of a sibling checkout an implicit build dependency.
Reproducible S.I.R. builds must select an explicit, coherent set of package
versions or immutable commits.

The deterministic domain and simulation kernel also target JavaScript through
Fable. FSharp.Formatting supplies the literate documentation and generated
site, and GitHub Pages hosts the static documentation, interactive experiments,
and versioned browser replay bundles. This web surface is a design, testing,
and replay host; it does not replace the authoritative .NET match host or
select a live-client framework.

The browser replay and rules-lab application uses Elmish MVU from its first
implementation, with React rendering and a typed F# view DSL. Fable
compatibility needed from `FS.GG.Game` is developed and released by the
upstream repository as a versioned package contract. S.I.R. does not copy the
framework algorithms or retain a permanent sibling source reference.

## Architectural composition

```text
S.I.R. rules, domain types, and deterministic match kernel
                │
                ├── selected FS.GG.Game.Core primitives
                ├── metered per-unit WASM host
                └── S.I.R.-owned compatibility adapters
                              │
              authoritative events and projections
                              │
                  server and public protocol
                              │
                     selected FS.GG.Net transports
                              │
               canonical client or custom client
                              │
           FS.GG.Rendering and, when required, FS.GG.Audio
```

S.I.R. owns its gameplay domain. FS.GG supplies reusable platform and game
building blocks; it does not define S.I.R.'s rules merely because a similarly
named generic operation exists in the framework.

## Dependency direction

The intended project boundaries are:

- a pure or tightly controlled S.I.R. domain and simulation core;
- a WASM host adapter around the selected runtime;
- server coordination and S.I.R. protocol projects;
- persistence and operational adapters outside the match kernel;
- a canonical client consuming the same public projections available to custom
  clients; and
- test, scenario, replay, and benchmark projects exercising the headless core;
  and
- a Fable browser replay host compiling the same authoritative domain and
  simulation source without the native Wasmtime adapter.

The domain and match kernel must not depend on rendering, audio, sockets,
databases, process clocks, or client UI types. Network and client projects
depend inward on stable domain contracts and outward on FS.GG infrastructure.

Public cross-project surfaces should follow the FS.GG convention of explicit
F# signature files where practical. Domain modeling should prefer
discriminated unions, records, units of measure, and typed identifiers over
loosely related primitive values.

## FS.GG.Game integration

FS.GG.Game provides useful render-independent primitives for fixed-step games,
grids, pathfinding, line of sight, fields of view, geometry, ballistics, and
effects. S.I.R. consumes them selectively and verifies each authoritative use
against the game's deterministic contract.

| Concern | Integration rule |
|---|---|
| Grid cells and bounded grids | Reuse compatible integer primitives and storage where their semantics match S.I.R. |
| Cell edges and vertices | Reuse `FS.GG.Game.Core.Edges` canonical `Edge`/`Vertex` addressing, dedupe, and cell/edge/vertex relationships. S.I.R. owns semantic edge features and their per-modality permeability; the module's boolean `Set<Edge>` wall model is an input to that, not a substitute. Its `bfs`/`astar` are four-way with a Manhattan heuristic and are not canonical for S.I.R.'s eight-way equal-cost movement. Diagonal-through-corner rules are S.I.R.-owned because `Edges` addresses only orthogonal boundaries. |
| Fixed-step host loop | May translate wall-clock time into due simulation ticks. Floating-point accumulator time and render interpolation stay outside authoritative state. |
| Tick state | S.I.R. owns the integer 20 Hz tick index, phase pipeline, journal, snapshots, and hashes. |
| LOS and FOV | Reuse compatible discrete algorithms behind S.I.R. interfaces and golden tests. S.I.R. owns stance, cell blockers, edge-feature occlusion, square footprints, perception, and knowledge semantics. A reused trace must report the edges it crosses, not only the cells it enters. |
| Spatial indexes and caches | Reuse where useful, but treat all caches as derived and require cached and uncached results to agree. |
| Map generation | `MapGen`'s BSP, cellular-automata, and room-graph families are procedural generators. S.I.R.'s canonical map model is deterministic assembly of hand-authored parcels, so they apply to terrain fill, natural and portal space, and clutter rather than to authored tactical composition. `Grid<'T>`, `Rect`, and `Region` are reusable regardless. |
| Map validation | Reuse the `MapAnalysis` structure directly, including `coverMap`, `exposureMap`, `killzones`, `fairness`, `spacing`, `articulationPoints`, `isConnected`, and `validate`/`Rule`/`Report`. S.I.R. supplies its own cover and line-of-sight definitions because both depend on the semantic edge layer and square footprints. Validation is an offline content gate, not a runtime cost. |
| Elevation | Not provided. `Cell` is two-dimensional, so multi-level terrain, horizontal edges, and inter-level movement are S.I.R.-owned. Reused 2D algorithms apply per level and must not be assumed to compose across levels. |
| Pathfinding | Use an S.I.R. adapter or implementation that gives orthogonal and diagonal moves equal Chebyshev step cost. The generic weighted eight-way A* uses a longer diagonal cost and is not canonical as-is. |
| Cooperative movement | S.I.R. owns footprint reservations, friendly passage, dependency resolution, hostile collision, and deterministic batch commit. Generic paths may only provide candidates. |
| Randomness | S.I.R. owns counter-addressed samples keyed by match, tick, event, purpose, and ordinal. A sequential or splittable framework generator may support isolated generation, but must not become one global authoritative combat stream. |
| Geometry, ballistics, visibility, and physics | Adopt module by module. Floating-point-heavy behavior is not authoritative unless its reproducibility and gameplay semantics are explicitly accepted. |
| Effects | Reusable effect primitives may support implementation; S.I.R. still owns action completion, contact, mitigation, consequence batching, and event order. |
| AI knowledge views | S.I.R. owns the knowledge and communication model. Framework conveniences must not retain or disclose hostile state contrary to the canonical observation and report rules. |

An adapter is a stable S.I.R. boundary, not a temporary workaround. It should
state the semantic difference, have deterministic conformance tests, and make
an eventual compatible upstream capability replaceable without changing the
game rules.

## FS.GG.Net integration

FS.GG.Net supplies transport and serialization infrastructure, including
Protobuf-oriented WebSocket and gRPC components. S.I.R. owns:

- protocol messages and schema evolution;
- authentication and account authorization;
- match admission and input cutoffs;
- knowledge-filtered player projections;
- tick, command, snapshot, replay, and resynchronization semantics;
- module upload and selection workflows; and
- operational deployment, TLS, rate limits, and abuse controls.

Transport is not the game protocol. The authoritative kernel accepts validated,
ordered inputs and emits domain events; an FS.GG.Net adapter moves their public
representations across a selected transport.

Single-player, multiplayer, the canonical client, custom clients, headless
tests, and third-party servers should use the same versioned public contracts.
Native gRPC is the
[canonical first public transport](research/public-transport-selection.md).
Contract-first `.proto` schemas define the language-neutral public surface. A
bidirectional stream carries the live session; separate methods carry
discovery, setup, artifacts, catalogs, and replays.

The canonical
[Public gRPC Protocol Architecture](public-protocol-architecture.md) defines
the service split, session envelopes, sequencing, resume, projections,
backpressure, and compatibility policy.

Fable browser replay does not change the live transport decision. It downloads
authorized, completed replay packages over ordinary static or replay-service
delivery and runs the shared kernel locally. Exact behavior, historical engine
bundle retention, and the distinction between kernel replay and authoritative
WASM verification are defined by
[Cross-Runtime Determinism and Browser Replay](cross-runtime-replay.md).

## FS.GG.Rendering and FS.GG.Audio integration

The canonical desktop client uses the F#/MVU-oriented FS.GG rendering stack.
It renders knowledge-filtered projections and may interpolate presentation
between authoritative ticks. It cannot infer hidden state from local simulation
or invoke a privileged gameplay path.

Client customization, overlays, certainty presentation, accessibility color
schemes, hotkeys, camera behavior, animation, and audio are presentation
concerns. They may be changed without changing authoritative outcomes.

FS.GG.Audio is the available framework integration when the client audio design
is implemented, subject to the same separation between presentation and
authoritative sound observations. Its exact use is not yet canonical.

## WASM runtime boundary

The control-module contract uses direct Wasmtime .NET embedding behind the
`SIR.Wasm` adapter. The
[canonical runtime decision](research/wasm-runtime-selection.md) selects the
runtime family; a focused validation spike must pass before the exact package
version is pinned. The runtime must provide:

- deterministic-enough execution under the restricted host contract;
- fuel or an equivalent instruction budget;
- strict memory limits;
- controlled imports with no ambient filesystem, clock, network, or entropy;
- isolated per-unit instances with reusable compiled artifacts;
- trap and timeout containment; and
- practical F#/.NET hosting and profiling.

Runtime-specific values and exceptions remain behind an S.I.R. host adapter.
Saved matches identify the module artifact, ABI, execution profile, and
runtime-compatibility version rather than relying on an unrecorded installed
runtime.

## Local development and dependency policy

The normal dependency modes are:

1. **Pinned packages** for reproducible builds, CI, releases, and ordinary
   development.
2. **Explicit sibling project references** for deliberate coordinated
   development or diagnosis.
3. **Immutable commit pins** when testing an unreleased cross-repository
   capability.

Changing between these modes must be visible in project or central dependency
configuration. A build must never discover and consume a sibling repository
only because it happens to exist.

S.I.R. maintains:

- an explicit FS.GG compatibility set;
- contract tests for every authoritative adapter;
- a deterministic scenario suite that runs after dependency changes;
- a record of package and protocol versions in replay and server build
  metadata; and
- release notes for changes that can alter simulation or wire compatibility.

## Cross-repository change policy

S.I.R.-specific policy and adapters belong in this repository. If S.I.R. needs
a generally reusable capability or a change to a versioned FS.GG contract, the
work starts as an issue in the owning FS.GG repository and follows the FS.GG
coordination, compatibility-registry, and ADR process.

No upstream change is currently required for the Chebyshev path-cost, semantic
edge, or counter-addressed-randomness boundaries; all can be implemented cleanly
in S.I.R. `FS.GG.Game.Core.Edges` already supplies the canonical edge and vertex
addressing S.I.R.'s edge model needs, and it explicitly complements rather than
replaces the tile model. If eight-way edge-aware traversal proves generally
reusable, proposing it upstream is preferable to duplicating the addressing.

If experience shows that a generic FS.GG primitive should own any of these
capabilities, that becomes an explicit cross-repository proposal rather than an
uncoordinated sibling edit.

## Validation gates

A technology integration is acceptable when:

- authoritative tests produce identical hashes across repeated runs;
- identical replay fixtures produce exact checkpoint, event, and final-state
  digests in the .NET kernel, the Fable JavaScript test host, and the published
  browser engine bundle;
- adapters have focused golden and property tests;
- cached and uncached spatial results agree;
- network serialization round-trips every public contract and rejects unknown
  incompatible versions safely;
- the headless kernel runs without rendering or audio dependencies;
- the canonical client has no information or command privilege unavailable
  through the public API;
- WASM failures remain bounded to their declared gameplay consequence; and
- package or sibling-reference changes cannot silently alter a recorded match.

## Open implementation parameters

- Initial coherent FS.GG package-version set.
- Validated Wasmtime package version and execution-profile pin.
- Fixed-point representations by subsystem.
- Exact public Protobuf fields, operational limits, and generator version.
- Server process, persistence, and match-isolation topology.
- Rules for promoting a sibling-tested dependency to a released package.
