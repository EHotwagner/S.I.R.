---
title: Simulation Core
category: Engineering
categoryindex: 6
index: 1
status: proposed
document-type: living-design
version: "0.8"
last-updated: 2026-07-28
related:
  - docs/game-vision.md
  - docs/skirmish-development-plan.md
  - docs/technology-stack.md
  - docs/fable-client-and-documentation.md
  - docs/wasm-control-architecture.md
  - docs/combat-resolution.md
---

# Deterministic Simulation Core

## Purpose

This document defines the technology-independent behavior of the authoritative
simulation. Its implementation is F# on .NET 10 using selected FS.GG
components, but networking transport, database, deployment, and client choices
must still serve this contract rather than redefine it.

## Core transition

The conceptual simulation interface is:

```text
step(
    immutable ruleset,
    authoritative state,
    ordered tick inputs
)
→ next authoritative state
+ authoritative events
```

An implementation may use controlled internal mutation for performance, but its
observable result must be equivalent to this deterministic transition.

## System boundary

```text
content and rules
        ↓
deterministic match kernel ←→ metered WASM host
        ↓
authoritative state and events
        ↓
knowledge-filtered projections
        ↓
network API
        ↓
canonical or custom client
```

Inside the deterministic boundary:

- grid and entity state;
- movement, occupancy, and reservations;
- actions and reactions;
- perception and actor knowledge;
- combat and consequences;
- communications;
- logistics;
- mission objectives;
- deterministic randomness; and
- per-unit WASM execution state and results.

Outside it:

- authentication;
- connection objects;
- wall-clock scheduling;
- databases and campaign storage;
- match discovery;
- metrics and logging destinations;
- rendering and audio; and
- client input devices.

The kernel does not read a clock, socket, database, filesystem, locale,
environment variable, process-global random generator, or other uncontrolled
operational input.

## State categories

### Authoritative state

Authoritative state is included in deterministic snapshots and hashes. It
includes:

- entity identities and components;
- grid occupancy, edge feature state, and spatial revisions;
- movement credit and reservations;
- action state and timing;
- HP, wounds, armor, suppression, and other conditions;
- per-actor perception and knowledge;
- communication queues and delivery state;
- logistics ownership, reservations, and transactions;
- objective and mission state;
- deterministic random contexts; and
- WASM artifact identity, instance memory, pending services, wake schedule, and
  rule-relevant fault state; and
- command-bandwidth pools, current allocations, and allocation policy state,
  since they determine what each unit is told and therefore what it decides.

### Derived state

Derived state is rebuildable and is normally excluded from authoritative
snapshots and hashes:

- LOS and visibility caches;
- pathfinding caches;
- spatial indexes;
- connected-region and influence data;
- client knowledge projections;
- render interpolation;
- compiled WASM code caches; and
- performance statistics.

Cached and uncached evaluation must agree. Cache population, eviction, memory
address, and iteration order cannot affect authoritative results.

### External operational state

External state never decides game rules:

- live connections;
- CPU timing;
- thread scheduling;
- server load;
- database sessions;
- log destinations; and
- metrics exporters.

## Authoritative numeric model

Gameplay uses:

- integer grid coordinates;
- integer simulation ticks;
- fixed-point quantities where fractions are required;
- explicit overflow and rounding rules;
- bounded integer resources; and
- counter-addressed deterministic random samples.

Authoritative floating-point calculations are avoided unless a future case
demonstrates a necessary tactical benefit and defines a reproducible
implementation.

Clients can freely use floating point for interpolation, animation, lighting,
audio, and other presentation.

## Stable typed identifiers

Stateful concepts use stable typed identifiers, including:

- entities;
- actions;
- effects;
- observations;
- messages;
- reservations;
- logistics transactions;
- objectives;
- WASM instances; and
- random sample purposes.

Entity references use a generation or equivalent mechanism so a destroyed
entity's storage index cannot accidentally identify a later entity.

Identifiers do not grant gameplay priority. Conflict resolution uses declared
symmetric or deterministic arbitration, never the lowest entity identifier or
first storage entry unless a specific public rule explicitly says otherwise.

## External input journal

Clients and external services do not assign authoritative timestamps or
priority.

The match coordinator:

1. receives an input;
2. authenticates and validates its envelope;
3. assigns an eligible target tick;
4. assigns stable ordering within the applicable boundary; and
5. appends it to the match input journal.

Late input follows a public next-eligible-tick or rejection policy. An input is
never inserted retroactively into a committed tick.

Ordinary player control enters as messages delivered to the HQ module through
the applicable transport. It is not a direct mutation of a field unit.

## Phase-local computation and batch commit

Systems do not opportunistically mutate shared state while iterating entities.
Each logical phase:

1. reads stable phase input;
2. produces keyed candidates, intentions, or deltas;
3. groups or orders them through explicit rules;
4. resolves conflicts; and
5. commits a batch result.

Examples:

```text
movement intentions
  → reservation dependency graph
  → conflict resolution
  → committed transitions
```

```text
completed attacks
  → physical traces
  → contact and mitigation
  → batched consequences
```

Hash-map iteration, memory layout, task scheduling, and thread completion cannot
change a result.

## Deterministic parallelism

The initial implementation prioritizes a correct logical ordering. Read-only
work can run in parallel when it returns keyed results to a deterministic merge
stage.

Candidate parallel work includes:

- LOS geometry;
- independent acquisition updates;
- shot trace calculation;
- path requests;
- per-unit WASM invocation; and
- influence fields.

Parallel workers cannot directly mutate shared authoritative state. The
canonical merge stage applies public conflict and ordering rules.

## Data organization

The simulation uses a data-oriented component model without requiring an early
commitment to a specific third-party ECS framework.

Representative stores include:

```text
Position[entity]
FootprintSize[entity]
Facing[entity]
Attention[entity]
CurrentAction[entity]
Health[entity]
Armor[entity]
Knowledge[observer]
Inventory[holder]
```

Dense grid occupancy, the edge feature layer, revisions, and other spatial
structures use specialized data layouts rather than depending exclusively on
generic component queries. Edges are stored once per canonical boundary so a
feature cannot disagree with itself depending on which side reads it.

At the intended scale, clarity, determinism, and direct profiling take priority
over adopting infrastructure designed primarily for millions of entities.

## WASM integration

The WASM host is part of the deterministic tick pipeline:

```text
knowledge-filtered control input
  → exact artifact and execution profile
  → isolated metered invocation
  → atomic output or declared failure
```

Snapshots contain enough module state to resume and replay an instance:

- immutable artifact identity;
- instance memory and declared globals;
- pending host-service handles;
- wake schedule; and
- fuel or fault state required by the rules.

Compiled native code and compilation caches are derived operational state.

## Randomness

Random samples are addressed by stable context rather than consumed from one
global sequence. Inputs include a secret match random context and stable facts
such as tick, action, effect index, and sample purpose.

Unrelated random activity cannot shift another action's result. Public rules
define distributions, while future samples and secret context remain hidden
during the match.

## Snapshots, events, and replay

A match retains:

- initial authoritative snapshot;
- ruleset and content hashes;
- exact WASM artifacts and execution profile;
- ordered external input journal;
- periodic recovery snapshots;
- authoritative event records or event hashes;
- per-tick state hashes, whose implementation must be incremental or
  event-stream based rather than hashing full state at tick rate; and
- final outcome record.

Replay reconstructs from an initial or periodic snapshot and the ordered journal.
Generated events and hashes verify reconstruction.

Accepted, validated, and ordered WASM outputs are stored as replay-driving
inputs. A non-Wasmtime replay host, including the Fable browser host, injects
those outputs into the shared kernel rather than claiming to reproduce module
execution. Authoritative verification re-executes the exact artifacts under the
pinned profile and compares their outputs before reconstructing the kernel
state.

The authoritative F# kernel compiles from shared source for .NET and Fable.
Both builds must produce exactly equal state and event digests from the same
decoded snapshot and ordered kernel inputs. Periodic checkpoints permit seeking
by restoring the nearest checkpoint and simulating forward.

A full authorized replay can re-simulate the complete world. A
player-perspective replay instead plays back recorded knowledge-filtered
projections; it cannot reconstruct hidden world state. The canonical
[Fable Client and Interactive Documentation](fable-client-and-documentation.md)
decision defines the package, version binding, disclosure modes, WASM boundary,
and conformance gate.

A divergence report identifies at least:

```text
first divergent tick
logical phase
affected identifiers
expected hash
actual hash
relevant inputs and events
```

## Knowledge-filtered projections

An ordinary client never receives raw world state. For each player, the server
derives a projection from:

- information legitimately present at HQ;
- delivered reports and player messages;
- public mission state; and
- mode permissions.

Network snapshots and deltas contain only that projection. Reconnection restores
the current HQ projection rather than the complete battlefield.

Administrative, spectator, and debugging projections use separate authenticated
permissions and cannot share an ordinary player path.

## Headless execution

The kernel runs without rendering, audio, input devices, or real-time waiting.

A live match advances at 20 ticks per wall-clock second. A test, batch, or replay
runner advances the same simulation ticks as quickly as compute allows without
changing simulation time or outcomes.

Headless execution supports:

- deterministic scenario tests;
- large simulation batches;
- WASM evaluation;
- balance experiments;
- performance profiling;
- first-divergence diagnosis; and
- replay verification.

## Continuous invariants

Development and test builds continuously verify invariants such as:

- committed unit footprints do not overlap;
- no committed transition crosses an edge that forbids it for that movement
  profile;
- movement, line of sight, and shot traces agree on which edges a path crosses;
- resources are conserved except through declared creation and destruction;
- actions follow valid lifecycles;
- observations have legitimate provenance;
- modules receive only permitted knowledge;
- communications use allowed routes;
- command bandwidth is conserved, never exceeds its pool, and is never allocated
  to a squad across an unavailable communications path;
- every instance retains its local floor regardless of allocation;
- entities do not act after disqualifying consequences unless simultaneous
  completion permits it;
- nonpersistent skirmish does not access campaign state;
- cached and uncached queries agree; and
- identical inputs reproduce identical hashes.

## Selected platform and integration constraints

The selected implementation platform is F# on .NET 10 with the FS.GG framework
family. The stack must preserve:

- predictable integer and fixed-point behavior;
- explicit memory and allocation control;
- a mature embeddable WASM runtime with fuel or equivalent metering;
- headless server execution;
- deterministic serialization and hashing;
- strong testing and profiling;
- networking without coupling it to the kernel;
- safe concurrency with deterministic merge stages; and
- practical AGPL-compatible distribution.

FS.GG.Game.Core primitives are consumed only where their exact semantics match
the authoritative rules. In particular:

- S.I.R. retains its own integer tick and fixed-point authoritative state even
  if an outer FS.GG loop uses floating point for wall-clock accumulation or
  render interpolation;
- canonical eight-direction movement uses equal orthogonal and diagonal
  Chebyshev steps, so generic weighted eight-way A* behavior requires an
  S.I.R. pathfinding adapter;
- `FS.GG.Game.Core.Edges` supplies canonical edge and vertex addressing, but its
  traversal helpers are four-way with a Manhattan heuristic, so S.I.R. owns
  eight-way edge-aware traversal and the diagonal-through-corner rule;
- counter-addressed authoritative randomness requires an S.I.R. adapter rather
  than a single sequential random stream; and
- floating-point-heavy geometry, visibility, ballistics, or physics modules
  require explicit deterministic acceptance before authoritative use.

FS.GG.Net provides transport infrastructure without owning the S.I.R. protocol
or knowledge policy. FS.GG.Rendering provides the canonical client foundation
without entering the authoritative kernel. Local sibling clones enable
co-development, but reproducible builds use an explicit coherent dependency
set.

See [Technology Stack and FS.GG Integration](technology-stack.md) for component
boundaries, dependency policy, and validation gates.

## Open implementation parameters

- Fixed-point representations by subsystem.
- Snapshot and journal encodings.
- Snapshot frequency and retention.
- State and event hash algorithms.
- Input cutoff and late-input policy.
- Component storage implementation.
- Parallel job system.
- Server process and match-isolation topology.
- Validated version of the
  [canonical Wasmtime runtime](research/wasm-runtime-selection.md).
- Initial coherent FS.GG dependency set.
