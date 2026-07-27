---
title: S.I.R. WebAssembly Control Architecture
status: proposed
document-type: living-design
version: "0.14"
last-updated: 2026-07-27
related:
  - docs/game-vision.md
  - docs/combat-resolution.md
  - docs/technology-stack.md
  - docs/research/wasm-runtime-selection.md
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
3. **gameplay capability definitions and the event catalog**, which are fixed,
   machine-readable, versioned ruleset data.

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
- setting body facing, attention direction, or stance;
- setting a formation station or requesting a formation change;
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
preserves deterministic causality.

Because an instance is invoked every tick, an event observable at a tick
boundary is delivered to its module on that boundary. Reaction timing is then
governed by the declared reaction delay of the action the module requests, not
by when the module happens to run.

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

## Instance configuration

An artifact is immutable and shared by every unit assigned to it. Configuration
therefore cannot live inside it: if it did, every player running the standard
module would have identical behaviour and no player could give two squads
different intent.

**Instance configuration** is per-unit data supplied alongside the artifact
assignment. It is what makes one artifact serve many differently-directed units.

- it is **opaque to the server**, exactly as message payloads are. The server
  validates size and structural bounds and does not interpret meaning;
- it is **part of the force snapshot**, so it enters the commitment record, the
  input journal, and the replay;
- it is **locked when the match begins**, as the artifact and its version are;
  and
- it is bounded by the host-class profile like any other instance resource.

This is what the mission lifecycle already calls *initial policies*, validated
at deployment and captured in the commitment snapshot. Until now the term had no
definition.

For the standard module, instance configuration is the squad's posture and its
overrides. For a player-written module it is whatever that author wants, and the
server's ignorance of its meaning is the same ignorance it has about every other
player-defined byte.

### Changing configuration during a match

Instance configuration is fixed at lock. Changing behaviour afterwards is not a
configuration edit; it is an **order**, delivered as an ordinary message through
the simulated communications topology, subject to range, jamming, delay, and
loss.

One consequence deserves stating: **the client knows what it sent, not what the
module currently holds.** A player who orders a squad to screen and whose order
does not arrive believes something false about their own force. That is the
intended behaviour, it is the same fog the design applies everywhere else, and
the canonical client must not paper over it by displaying an intended posture as
though it were a confirmed one.

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

## Invocation cadence

Every unit's instance is invoked **on every simulation tick** by default.

This was measured rather than assumed. At the intended upper force target the
full cost of marshalling, invoking, and reading results for 200 instances is
under 2% of a 50 ms tick — see
[WASM Invocation Spike](research/wasm-invocation-spike.md). Per-tick invocation
is affordable, so the architecture does not need a server-executed declarative
layer to stand in for the module between invocations.

An instance may request to sleep until a future tick when it has nothing to do.
Sleeping is a courtesy that saves work for idle units; it is not load-bearing,
and authoritative events wake a sleeping instance regardless.

Because the module is present every tick, it holds its own state, computes its
own predicates, and decides for itself. The server does not evaluate conditions
on a unit's behalf and there is no published condition vocabulary.

### What this replaces

An earlier revision specified standing doctrine as ordered condition-to-action
rule lists evaluated server-side, with wake subscriptions and a delegation
policy for player-issued doctrine commands. That design existed to avoid a
per-tick invocation cost that measurement showed does not exist.

It is removed. Rule lists, the condition vocabulary, ordering guards,
unreachability validation, wake subscriptions, and delegation policy are all
gone. What remains is a plain request-response ABI, which is both simpler and
strictly more expressive.

The requirement it was serving — that a player who writes no module can still
author tactical behaviour — is met by the **standard module** being
configurable, not by the engine. That configuration is standard-module content
carried over the ordinary message channel, so it evolves without versioning the
ABI.

## The control surface

Two vocabularies cross the boundary, and only two: the **events** a unit is told
about, and the **actions** it may request. Both are versioned ruleset data.

Their contents are maintained in [Control ABI Surface](control-abi.md).

### Actions are capability invocations

The ABI does not grow a function per content item. It carries a small set of
request kinds, and the ruleset supplies versioned capability descriptors that
give them meaning, as described under capability-driven actions above.

Weapons, medical actions, breaching, resource transfers, magic, formation
changes, and later faction mechanics are therefore descriptors rather than ABI
surface. Adding content extends the descriptor set; it does not change the
contract.

### Events are authoritative facts

An event tells a unit something happened that it is entitled to know. Events are
generated by the simulation under the ordinary observation and communication
rules — a module cannot subscribe to, suppress, or invent them.

The event catalog is the half of the contract that determines whether a module
can react competently, and it is specified in
[Control ABI Surface](control-abi.md).

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
- every instance is invoked at the same rate, so no module gains advantage by
  running more often than another; and
- module instructions cannot hide unbounded server work behind cheap host
  calls.

### Separately limited resources

Host-class profiles independently limit:

- linear memory;
- instance configuration size;
- match-local state;
- input and output size;
- host-service type and frequency;
- communication payloads;
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

## Command bandwidth

### Why information must cost something

Control modules are player-authored, and any capability that is free will be
maximised. If a module can request every known contact at full fidelity at no
cost, every competent module will do so on every tick, and the server pays for
detail nobody needed.

Invocation *frequency* is no longer priced, because measurement showed it is
affordable for every unit on every tick. Invocation *richness* still is.

### What it meters

Command bandwidth prices **networked** information only:

- **shared picture** — contacts and facts other units observed, fused and
  delivered to this one, at a fidelity and extent that scale with allocation;
  and
- **expensive host services** — pathfinding, influence queries, firing-line
  evaluation, formation planning, and sensor-coverage analysis, together with
  the results returned.

### Bandwidth is downstream

Both priced items flow **toward** a unit. That is deliberate and it is the
governing asymmetry: **a commander allocates what they give, not what they are
told.**

Reporting upward is not drawn from the allocated pool. A commander does not
ration how much a subordinate element may tell them, and doing so would be
strange in the fiction and hostile in play. What bounds upstream reporting is
link capacity, aggregation at each hop, and emission — see
[Observation Reporting Model](reporting-model.md).

This is also the better constraint. Emission is a bound that cannot be purchased
around, and it is tactically interesting because reporting reveals the reporter.
An administrative allowance on how much a unit may say would be neither.

### What it does not price

**A unit's own perception is never gated.** What it sees, hears, and detects for
itself arrives through the local floor, is unaffected by allocation, and
survives total communications denial. A unit alone in the dark still has its own
eyes.

**Module execution is never priced and never emits.** The instance runs on the
authoritative server every tick regardless of connectivity, allocation, or
jamming. Deliberation is free and silent; it is only the *arrival of information
from elsewhere* that costs bandwidth and produces a transmission.

This is the distinction that makes the coupling in
[Communications Network](communications-network.md) coherent. Nothing emits
because a unit is thinking. Emissions occur because a shared picture, a service
request, or a service result crossed a link to reach it.

Per-invocation fuel remains separate and unchanged: it governs how much a module
may compute *within* one invocation. Command bandwidth governs how well informed
it is by others and how much server work it may commission.

### Two layers

Every unit has a **local floor** and may additionally draw on a **pooled
allocation**.

```text
local floor            always available, comms-independent,
                       equal for every instance of a host class

pooled command         allocated by the player, drawn through the
bandwidth              communications topology
```

The floor represents a unit's own onboard sensing and processing. It is enough
to perceive its immediate surroundings, act, and fight. It is never taken away,
and it is identical for every instance of the same host class, which preserves
the baseline fairness the execution profile already guarantees.

The pooled layer represents networked command and processing support: reachback
analysis and the fused picture supplied through the squad and company network.
It is finite, allocated, and depends on connectivity. It does not affect how
often a module runs, which is every tick for every unit.

A unit that loses its allocation is not disabled. It becomes autonomous and
unsupported — it still runs every tick, still fights, still reacts, but it sees
less and cannot commission expensive analysis.

Allocation is not free of consequence in the other direction either. It is
carried as traffic over the communications network, so a heavily supported unit
emits more and is easier to locate. Attention has a physical signature, and
spending it on a concealed unit is correspondingly expensive.

### Scope and the communications tie

Command bandwidth is pooled **at squad level**, with headquarters holding its
own allocation and distributing shares to squads.

Reallocating bandwidth to a squad requires an operational communications path to
that squad. The consequence is deliberate and severe:

> A commander cannot spend attention on a squad they cannot reach.

A disconnected squad continues on its local floors and whatever allocation it
held when contact was lost. It does not gain more, and the player cannot
redirect capacity toward it however urgent its situation becomes. Restoring
communications restores the ability to allocate.

Its modules keep running at full rate throughout. What degrades is what they can
see and what analysis they can commission, not whether they think.

This binds three systems that were previously only loosely related. Command
topology now governs control quality and not only orders and reports; losing a
squad leader's communications device degrades the quality of that squad's
picture, not merely what it is told; and electronic warfare gains a direct,
legitimate attack on the control layer.

### Bucket semantics

Allocation behaves as a bucket with both a capacity and a maximum flow rate.

Capacity permits deliberate reserve — holding capacity back for an anticipated
decisive moment is a legitimate and interesting choice. The flow rate prevents
that reserve from becoming instantaneous omniscience, so a saved pool cannot be
discharged into a single tick to inspect the whole battlefield at maximum
fidelity.

Unspent capacity above the cap is lost rather than accumulating without bound.

### Allocation is policy, not micromanagement

The human commander sets allocation policy. They do not hand-assign attention
per unit per moment, which would recreate exactly the interface drudgery this
architecture exists to remove.

Allocation policy should be expressible as priority classes, conditional
triggers, and reserves, with automation applying it and surfacing exceptions —
the same discipline already required of logistics automation. The player decides
what matters; the system distributes accordingly and explains what it did.

### Fairness

- Every player receives the same total command bandwidth for the same mode and
  force conditions.
- Pooling changes distribution *within* a player's force. It never changes the
  total available *between* players.
- A server may not sell, grant, or otherwise allocate greater competitive
  bandwidth to particular accounts.
- The per-invocation fuel allowance remains uniform across instances of a host
  class, and every unit is invoked at the same rate. Units may be differently
  informed; they may not be given different amounts of computation.

### Force size

Because the pool is finite and per-player, fielding more units means less
attention available to each. Fifty units are individually better supported than
a hundred.

This is intended. It gives mass a genuine cost beyond point value, makes force
composition a real decision rather than an exercise in maximisation, and applies
a self-balancing pressure across the intended 50–100 band. Point catalogs should
not price this effect a second time.

### Determinism

Allocation is authoritative state, not an operational convenience.

Allocation changes are external inputs. They receive assigned target ticks and
stable ordering through the ordinary input journal, they appear in snapshots and
replays, and they cannot be applied retroactively into a committed tick. A
replay reproduces the same allocations and therefore the same observations.

### Open parameters

- The bandwidth unit, and the exchange rate between observation richness and
  host-service quota.
- Local floor values by host class.
- Total pool size, and whether it derives from force composition, mode, company
  progression, or equipment.
- Bucket capacity and maximum flow rate.
- Whether headquarters retains a reserve distinct from squad shares.
- Allocation policy vocabulary.
- Behavior for unattached units and units temporarily detached from a squad.
- Whether observation richness is a small discrete ladder or continuous.
- The degree to which electronic warfare may degrade bandwidth, and whether it
  can reduce a unit below its local floor.

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
- adversarial rehearsal against scripted opposition;
- the standard control implementation as readable reference code;
- scenario fixtures with known inputs and expected requests;
- fuel, memory, message, and host-service profiling;
- pre-match compatibility and validation tools; and
- replay inspection of permitted inputs, outputs, validation failures, faults,
  and consumed budgets.

Debugging and replay data remain knowledge-scoped during competitive play.
Post-match disclosure policy must not undermine information that should remain
secret across an ongoing campaign.

### Adversarial rehearsal

The offline harness is not only a correctness tool. Because player-authored
control is intended to be a primary form of mastery, a player must be able to
**rehearse a module adversarially** before committing it to a match: run it
against scripted opposition, vary that opposition's behavior, and observe where
the module fails.

Rehearsal runs the same deterministic kernel, ABI, execution profile, and
knowledge filtering as a live match. It is not a simplified simulator, because
doctrine validated against a simplified model would be worthless.

Rehearsal must not become a covert intelligence channel. It uses scripted or
player-authored opposition and published content only. It cannot replay another
player's module, reconstruct an opponent's doctrine from a competitive replay,
or consult campaign state the player is not entitled to see.

## Open parameters

The architecture does not yet fix:

- the contents of the condition and action vocabularies;
- rule-count, condition-count, and doctrine-size bounds by host class;
- subscription count and complexity bounds;
- how far static unreachability analysis is taken at validation;
- the delegation-policy category set;
- whether a starved unit's missed wakes are counted, summarized, or merely
  indicated;
- whether doctrine persists across an amendment or is always replaced whole;
- exact validated version of the
  [canonical Wasmtime runtime](research/wasm-runtime-selection.md);
- binary encoding and schema-evolution mechanism;
- concrete fuel and memory values;
- exact synchronous and asynchronous service boundaries;
- service latency and quota values;
- deterministic module-random interface;
- repeated-fault escalation;
- whether any module state persists between matches; or
- post-match disclosure policy for hidden observations and opponent code.
