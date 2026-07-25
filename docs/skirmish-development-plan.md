---
title: S.I.R. Robust Skirmish Development Plan
status: proposed
document-type: development-plan
version: "0.5"
last-updated: 2026-07-25
related:
  - docs/game-vision.md
  - docs/mission-lifecycle.md
  - docs/simulation-core-architecture.md
  - docs/technology-stack.md
  - docs/wasm-control-architecture.md
  - docs/combat-resolution.md
  - docs/logistics-architecture.md
---

# S.I.R. Robust Skirmish Development Plan

## Objective

Build a robust, nonpersistent skirmish foundation that supports single-player
and multiplayer missions through the same authoritative server, simulation,
public API, WASM runtime, objective system, outcome record, and replay format.

Skirmish is a maintained production mode and engineering benchmark. It is not a
disposable prototype for the later campaign.

## Governing boundary

Single-player and multiplayer use the same authoritative mission
implementation.

Single-player has one human participant connected to a server that also hosts
opposing control modules. Multiplayer changes participant assignment,
authentication, and knowledge projections; it does not introduce another
simulation path.

Persistent accounts, portal bidding, hidden co-allocation, scheduled major
missions, and campaign write-back are outside the first implementation
milestone.

## Implementation foundation

The implementation uses F# on .NET 10 and the FS.GG framework family:

- selected FS.GG.Game.Core primitives support the headless deterministic
  simulation behind S.I.R.-owned semantic adapters;
- FS.GG.Net supplies transport and serialization infrastructure for the public
  protocol, whose first canonical profile is native gRPC with contract-first
  Protobuf schemas;
- FS.GG.Rendering supplies the canonical client's F#/MVU rendering foundation;
  and
- FS.GG.Audio is available for later client audio integration, whose exact use
  is not yet canonical.

The sibling FS.GG repositories support local inspection and coordinated
development. CI and releases use an explicit coherent dependency set; a
neighboring checkout is never consumed implicitly.

The Chebyshev path-cost, cooperative footprint movement, counter-addressed
randomness, knowledge filtering, combat phase order, and WASM host ABI remain
S.I.R. contracts. See
[Technology Stack and FS.GG Integration](technology-stack.md).

## Versioned mode manifest

Every skirmish resolves from a machine-readable manifest containing:

- mode identifier and version;
- ruleset, content, and execution-profile versions;
- map and environment;
- participant and team structure;
- force source;
- point or deployment budget;
- deployment rules;
- objective set;
- time limit;
- victory, defeat, and draw rules;
- knowledge policy;
- WASM execution profile;
- result and replay policy; and
- persistence policy, initially `none`.

The public API, canonical client, custom clients, headless runner, and third-party
servers consume the same contract.

## Force sources

### Scenario-provided forces

Development begins with exact scenario forces so tests can pin:

- units and classes;
- positions and facing;
- equipment and resources;
- squads and command roles;
- module artifacts and state;
- initial orders;
- ruleset and random context; and
- expected events or outcomes.

### Standardized point catalog

Player-facing force construction follows after foundational combat
relationships are stable. Catalog entries resolve to complete immutable unit,
progression, equipment, supply, doctrine, and point-cost definitions.

Technical tests begin with mirrored human forces to isolate core mechanics from
faction asymmetry. Early PvE content then introduces a bounded arcane force to
verify genuinely asymmetric mechanics.

## Match state machine

The authoritative skirmish lifecycle is:

```text
Created
  → ForceSelection
  → Deployment
  → Ready
  → Live
  → Resolving
  → Completed
  → Archived
```

Every transition is validated, timestamped, replayable, and available through
the public API. Force and module assignments become immutable at the applicable
lock transition. No required transition depends on privileged canonical-client
logic.

## Objective framework

Objectives are versioned authoritative state machines consuming simulation
events. The first framework supports at least:

- elimination;
- area control;
- timed survival; and
- extraction.

A mission can combine objectives and mark them as primary, secondary, shared,
or opposed. Outcome can include partial success and does not require eliminating
every hostile.

## Canonical deterministic scenarios

The project maintains small focused scenarios before relying on large showcase
maps.

### Spatial scenarios

- Two friendly units pass in open terrain.
- A friendly column advances through a doorway.
- A friendly reservation deadlock is resolved.
- Two hostile units attempt the same destination.
- A large square unit moves diagonally near an obstacle.
- Hidden hostile contact blocks movement without advance disclosure.

### Perception scenarios

- Two units meet while rounding a corner.
- A watched doorway produces rapid acquisition.
- A rear approach remains unnoticed long enough to attack.
- Sound creates a directional stimulus without identification.
- Smoke changes geometry state and invalidates affected visibility caches.
- A valid local report fails to reach disconnected headquarters.

### Combat scenarios

- Two attacks complete on the same tick.
- Cover intercepts and is penetrated.
- A missed trace contacts a unit behind the intended target.
- A dangerous firing line produces friendly fire.
- Suppression affects a unit without HP damage.
- An incapacitated unit is stabilized.
- An execution is interrupted on an earlier tick.
- A strained caster is wounded and breaches.

### Control-module scenarios

- Sixty units share one compiled artifact while retaining isolated state.
- A module exhausts fuel and produces no partial output.
- An expensive host-service request returns asynchronously.
- A sleeping module wakes for damage or contact.
- A disconnected squad continues its permitted doctrine.
- Identical inputs reproduce identical module outputs and final simulation
  state.

## Single-player milestone

Single-player skirmish is complete when:

- the client connects through the public API;
- the authoritative server hosts friendly and hostile control modules;
- force validation, deployment, live play, objectives, results, and replay work
  end to end;
- client disconnection does not pause the simulation;
- standard modules can complete representative missions without continuous
  human micro-control;
- the mission can run headlessly in automated tests; and
- no gameplay subsystem requires persistent campaign state.

## Multiplayer milestone

Direct multiplayer skirmish adds:

- two or more independently authenticated participants;
- per-player knowledge projections;
- hidden opposing force composition and deployment;
- simultaneous input and action handling;
- reconnection;
- surrender, timeout, draw, and abandonment policies;
- consistent result and replay permissions; and
- no host-player authority advantage.

Participant count can be public in direct skirmish. Hidden co-allocation belongs
to the later major-mission stage.

## Scale gates

Correctness and performance expand through:

```text
4 versus 4       focused correctness and debugging
20 versus 20     squad, perception, and coordination behavior
50 versus 50     normal lower force target
100 versus 100   intended upper force target
stress case      deliberate overload and containment behavior
```

Robust skirmish is not complete until the representative 100-versus-100 case
sustains the authoritative 20 Hz target with:

- standard WASM control;
- representative movement and collision;
- perception and acquisition;
- combat, cover, armor, wounds, and suppression;
- communications;
- representative logistics;
- objective processing;
- networking; and
- replay recording.

The benchmark environment and required headroom remain to be selected.
Measurements report simulation, WASM execution, host services, networking,
serialization, and replay costs separately.

## Determinism gate

Every canonical scenario can be represented as:

```text
initial snapshot
+ ordered external inputs
+ ruleset and content
+ WASM execution profile
+ random context
= final state and event hashes
```

Repeated execution must produce identical results. A mismatch must identify the
first divergent tick and enough subsystem information to diagnose it.

Persistent campaign write-back does not begin until authoritative match outcomes
are reproducible under this contract.

## Implementation order

1. Headless fixed-step simulation, snapshots, events, and state hashing.
2. Grid occupancy, discrete movement, square-footprint collision, and spatial
   scenarios.
3. Action lifecycle and simultaneous resolution.
4. Perception, stimuli, acquisition, and reactions.
5. Physical traces, cover, armor, HP, wounds, and suppression.
6. WASM runtime, fuel, standard module, and bounded host services.
7. Objective framework and match state machine.
8. Single-player client/server mission.
9. Direct multiplayer, knowledge projections, and reconnection.
10. Battlefield logistics and extraction.
11. Standardized point-catalog force construction.
12. Scale and optimize through 100 versus 100.
13. Begin persistent campaign transactions only after the preceding gates pass.

## Required engineering properties

- Headless execution is first-class.
- The canonical client has no privileged gameplay path.
- All externally meaningful identifiers and schemas are versioned.
- Hidden information is filtered by the authoritative server.
- Module traps and fuel exhaustion cannot stall a match.
- Replay and state hashing are implemented alongside simulation systems rather
  than retrofitted later.
- Single-player does not bypass networking or server validation.
- Optimization cannot change authoritative outcomes.
- Skirmish remains isolated from campaign progression and resource state.

The kernel boundary and replay contract are defined in
[Deterministic Simulation Core](simulation-core-architecture.md).

## Deferred from the skirmish milestone

- Persistent personnel and campaign inventory.
- Portal bidding.
- Hidden player co-allocation.
- Half-hour major-mission scheduling.
- Transactional campaign write-back.
- Campaign wipes and overlapping campaign cadence.
- Long-term faction or company relationships.

The foundational data contracts should permit these later systems without
requiring their implementation during skirmish development.

## Open implementation parameters

- Authoritative server deployment topology.
- Exact Protobuf schema layout and compatibility window for the canonical gRPC
  profile.
- Validated version of the
  [canonical Wasmtime runtime](research/wasm-runtime-selection.md).
- Initial coherent FS.GG dependency set.
- Initial benchmark hardware and performance headroom.
- First maps and point-catalog contents.
- Exact surrender, timeout, and abandonment policies.
- Replay visibility and retention.
- The smallest arcane PvE content slice.
