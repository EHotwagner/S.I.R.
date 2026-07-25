---
title: S.I.R. WebAssembly Control Architecture
status: proposed
document-type: living-design
version: "0.1"
last-updated: 2026-07-25
related:
  - docs/game-vision.md
  - docs/combat-resolution.md
---

# S.I.R. WebAssembly Control Architecture

## Purpose

This document defines the canonical contract between the authoritative
simulation and player-provided WebAssembly control modules. It covers
invocation, observations, requests, isolation, host services, fuel, messaging,
fault containment, and development support.

## Three distinct contracts

S.I.R. separates:

1. the **server-to-WASM ABI**, which is fixed, public, and versioned;
2. **client-to-HQ and module-to-module payload semantics**, which are defined by
   the player; and
3. **gameplay capability definitions**, which are fixed, machine-readable,
   versioned ruleset data.

Players can invent their own command vocabulary, reports, compression,
acknowledgements, doctrine, and higher-level protocol. They cannot redefine how
a module receives authoritative observations, consumes server resources, or
requests game actions.

## Observe–decide–request

Each invocation operates on one unit and follows:

```text
immutable knowledge-filtered input
  → isolated module decision
  → bounded declarative output
  → server validation
  → accepted authoritative intentions
```

A module never directly changes position, HP, ammunition, perception,
leadership, inventory, communications, or other game state.

### Input

A control input can contain:

- authoritative tick and ruleset identifiers;
- unit and host-class identifiers;
- the unit's locally known state;
- current action, commitment, and recovery state;
- locally available observations and stimuli;
- messages and fixed reports delivered on this tick;
- authoritative events relevant to the unit;
- currently available capability descriptors;
- completed host-service results;
- outstanding request status; and
- applicable execution and output budget information.

The snapshot is immutable for the invocation. It contains only information the
unit is entitled to know at that tick.

### Output

A control output can request:

- starting an action;
- cancelling a cancellable action;
- setting a movement objective, route preference, or movement priority;
- setting body facing or attention direction;
- configuring reaction intent;
- invoking an allowed host service;
- sending an opaque communication payload; and
- scheduling the instance's next requested wake-up tick.

Outputs are intentions. The server validates capability, timing, knowledge,
targeting, cost, equipment, communication, spatial, and other requirements
before accepting them.

## Tick relationship

An invocation can respond only to information available at its tick boundary:

```text
previous tick commits
  → newly available events and observations are delivered
  → eligible instance wakes
  → module returns requests
  → server validates requests
  → accepted actions can begin
```

Information generated later in that tick cannot influence the invocation
retroactively. It becomes available at the next applicable boundary. This
preserves deterministic causality and the canonical nonzero reaction time.

## Artifact reuse and instance isolation

An immutable uploaded artifact can be assigned to many eligible units. The
server validates and compiles it once for a compatible execution profile, then
reuses that compiled code.

Every controlled unit still has its own:

- module instance;
- memory and match-local persistent state;
- input and output buffers;
- observation and message stream;
- execution budget;
- host-service quota; and
- wake schedule.

The server may evaluate instances using the same compiled artifact together for
cache locality. Player code never receives the batch or another instance's
context. Shared code is not shared state, shared knowledge, or a communication
channel.

## Capability-driven actions

The ABI uses stable generic request structures while the ruleset supplies
versioned capability descriptors. A descriptor can define:

- capability and version identifiers;
- parameter schema;
- targeting contract;
- preparation, commitment, resolution, and recovery timing;
- costs and prerequisites;
- cancellation and interruption rules;
- observable effects; and
- host-class compatibility.

Weapons, medical actions, movement, leadership, equipment, magic, and later
faction mechanics can therefore use one control architecture without requiring
a new privileged ABI function for every content item.

A unit instance can inspect only capabilities currently available to that unit.
Capability presence does not bypass server validation.

## Host services

### Synchronous queries

Cheap, bounded queries against the immutable invocation snapshot can be
synchronous. Candidate examples include:

- capability inspection;
- current action legality;
- known local occupancy;
- simple range and direction calculations; and
- bounded message-buffer operations.

### Expensive services

Potentially expensive operations are independently metered and may be
asynchronous:

- pathfinding;
- large-area influence queries;
- complex firing-line evaluation;
- formation planning; and
- sensor-coverage analysis.

An asynchronous request returns a handle. Its result is delivered during a
later invocation. A module cannot extend the current simulation tick by issuing
unbounded expensive queries.

Every service is knowledge-filtered. A path or firing-line request cannot reveal
an unseen enemy, door, destroyed wall, hazard, magical obstruction, or other
hidden state. Authoritative action can still fail later when the unit encounters
something it did not know.

## Deterministic sandbox

Player modules have no unrestricted access to:

- wall-clock time;
- operating-system randomness;
- files;
- network sockets;
- environment variables;
- threads;
- process creation;
- memory shared with another unit instance; or
- unrestricted WASI facilities.

Permitted inputs are authoritative and replayable. If control logic needs
randomness for doctrine, the ABI can expose a separately scoped deterministic
module stream. That stream cannot reveal or consume the authoritative combat
random context.

## Fuel and resource accounting

### Per-invocation fuel

Fuel is the primary deterministic execution limit. At each invocation:

```text
assign host-class fuel allowance
  → execute
  → accept complete bounded output
     or fail atomically on exhaustion or trap
```

Canonical rules:

- every instance of the same host class receives the same public fuel allowance
  under the same execution profile;
- unused fuel does not accumulate;
- one unit cannot transfer, lend, or pool fuel with another;
- assigning one compiled artifact to many units does not combine their budgets;
- artifact validation, compilation, and compatible-code caching occur before
  live play and are not charged repeatedly to instances;
- wake frequency and any rolling execution allowance are constrained separately
  so a module cannot evade the per-invocation limit through wake spam; and
- module instructions cannot hide unbounded server work behind cheap host
  calls.

### Separately limited resources

Host-class profiles independently limit:

- linear memory;
- match-local state;
- input and output size;
- host-service type and frequency;
- communication payloads;
- wake frequency;
- logging and diagnostics; and
- other capability-specific resources.

Expensive host-service quotas and cost schedules are separate from WASM fuel.

### Versioned execution profile

Fuel accounting is runtime-specific. Every match pins a versioned execution
profile containing at least:

- WASM runtime and version;
- compilation configuration;
- host ABI version;
- fuel schedule;
- host-service cost schedule; and
- host-class budget profiles.

Servers implementing the same competitive ruleset must preserve the declared
execution semantics and budgets. A server cannot sell or allocate larger
competitive budgets to particular accounts.

### Wall-clock backstop

Wall-clock duration is unsuitable as the ordinary competitive budget because it
depends on hardware, load, and scheduling. A server may use a wall-clock
deadline as an emergency infrastructure safeguard. It must not silently replace
the public fuel model or create hardware-dependent gameplay advantage.

## Atomic output and faults

The server accepts a module output only after the invocation completes and the
bounded output is structurally valid.

If an invocation:

- exhausts fuel;
- traps;
- emits malformed data;
- exceeds an output limit; or
- invokes a forbidden capability,

then none of that invocation's requests are applied. Previously accepted
authoritative actions remain in effect unless interrupted by an independent
game rule.

A faulty module cannot stall the simulation. The precise escalation policy for
repeated faults—retry, backoff, quarantine, last-order behavior, or a declared
fallback—remains to be designed and must be public and non-exploitable.

## Communications transport

Module messages use a fixed authoritative transport envelope with an opaque
player-defined payload. The envelope can include:

- sender;
- intended recipient or simulated route;
- payload bytes;
- priority;
- expiry; and
- size or resource cost.

The server determines whether, when, and through which simulated communication
path the envelope arrives. Player code defines payload semantics but cannot
bypass range, equipment, leadership topology, electronic warfare, interception,
delay, loss, or other communication rules.

## Development and audit support

Because custom control is a core game feature, the project provides:

- a versioned ABI specification;
- generated bindings for supported languages;
- a deterministic offline test harness;
- the standard control implementation as readable reference code;
- scenario fixtures with known inputs and expected requests;
- fuel, memory, message, and host-service profiling;
- pre-match compatibility and validation tools; and
- replay inspection of permitted inputs, outputs, validation failures, faults,
  and consumed budgets.

Debugging and replay data remain knowledge-scoped during competitive play.
Post-match disclosure policy must not undermine information that should remain
secret across an ongoing campaign.

## Open parameters

The architecture does not yet fix:

- binary encoding and schema-evolution mechanism;
- concrete fuel and memory values;
- exact synchronous and asynchronous service boundaries;
- service latency and quota values;
- deterministic module-random interface;
- repeated-fault escalation;
- whether any module state persists between matches; or
- post-match disclosure policy for hidden observations and opponent code.
