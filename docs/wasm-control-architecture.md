---
title: S.I.R. WebAssembly Control Architecture
status: proposed
document-type: living-design
version: "0.8"
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
3. **gameplay capability definitions and the doctrine vocabularies**, which are
   fixed, machine-readable, versioned ruleset data.

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
- replacing or amending the unit's standing doctrine, which the server then
  executes without further invocation;
- declaring wake subscriptions and the delegation policy for player doctrine
  commands;
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

This describes what happens when an invocation occurs. It is not the path by
which a unit reacts. Standing doctrine executes on every tick without an
invocation, so reaction timing is governed by the declared reaction delay rather
than by when the module next runs.

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

## Standing doctrine

Doctrine is the declarative half of the control interface. It is authoritative
per-unit state that the **server executes continuously without invoking the
module**, and it is what a unit runs on during the overwhelming majority of
ticks.

Without it, a module must be present for every decision, which forces frequent
wakes and makes command bandwidth unspendable on anything else. Doctrine is
therefore not a convenience layer over the imperative ABI; it is the mechanism
that makes bounded control possible at all.

### Two published vocabularies

Doctrine is expressed over two versioned ruleset vocabularies:

- a **condition vocabulary** — predicates over the unit's own state and local
  knowledge; and
- an **action vocabulary** — the standing behaviors and capability invocations a
  unit may adopt.

Both are machine-readable ruleset data under the same contract as capability
descriptors. New content extends the vocabularies; it does not change the ABI.

Their contents are maintained in [Doctrine Vocabulary](doctrine-vocabulary.md),
which currently records the readiness audit that must precede naming them.

### Rule lists

A doctrine is an **ordered list of rules**. Each rule pairs one or more
conditions with an action:

```text
rule 1:  hostile in assigned sector AND ammunition above reserve  → engage sector
rule 2:  ammunition at or below reserve                           → request resupply
rule 3:  squad casualties above threshold                         → withdraw to rally
rule 4:  (unconditional)                                          → hold position
```

The server evaluates the list in author order and adopts the first rule whose
conditions currently hold. Author order is preserved deliberately: automatic
reordering by inferred specificity would be less predictable than the order the
author wrote, and predictability is worth more than convenience in a system
whose failures are only visible after a match.

### Guarding the ordering failure

An ordered rule list has a well-documented failure mode: a high-priority rule
whose conditions are always satisfied starves every rule beneath it, and the
author sees a doctrine that silently ignores most of what they wrote.

Two structural guards apply:

1. **The final rule must be unconditional.** Every doctrine has a guaranteed
   default, so a unit is never without a governing rule.
2. **Unreachable rules are reported at validation time.** Because the condition
   vocabulary is fixed and finite, the obvious cases are statically
   detectable — a rule preceded by an unconditional rule, or by one whose
   conditions are a superset of its own. Validation reports these before a match
   rather than leaving the author to infer them from a replay.

Full unreachability analysis is not decidable in general. Validation detects
what it can and reports the remainder as unverified rather than claiming a
doctrine is sound.

### Knowledge filtering

**Conditions evaluate against the unit's local knowledge, never world truth.**

A condition counting hostiles in a sector counts *known* hostiles. A condition
testing a route counts *known* obstructions. If conditions could read
authoritative state, doctrine would become an oracle that grants exactly the
omniscience the entire knowledge architecture exists to prevent — and it would
do so continuously, server-side, at no cost to the player.

This is the single most important constraint in this section.

### Cost model

Doctrine evaluation is a **static cost**, bounded when doctrine is assigned and
paid continuously by the server. Command bandwidth is a **dynamic cost**, spent
on wakes. They are separate budgets:

```text
unit server cost = doctrine evaluation   (bounded, continuous, always running)
                 + wakes                 (allocated, variable, player-directed)
```

Rule count, condition count per rule, and total doctrine size are bounded per
host class, because evaluation cost scales with them. A module cannot buy more
doctrine with unspent bandwidth, and it cannot evade a wake budget by encoding
arbitrarily elaborate behavior into doctrine.

### Determinism

Rule evaluation uses fixed order, fixed condition semantics, integer and
fixed-point comparison only, and no dependence on storage or iteration order.
Identical doctrine over identical knowledge produces identical adoption.

## Wake triggers

A module declares **parameterized subscriptions** describing when it wants to be
invoked. Subscriptions use the same condition vocabulary as doctrine rules, so
there is one vocabulary with two uses.

```text
OnContact(sector: assigned, minCount: 2)
OnAmmunitionBelow(reserve)
OnCommandRoleChanged
OnActionBlocked
```

When a subscription's conditions become true the instance is woken, and the wake
spends command bandwidth.

### Why this is bounded

Subscription count and complexity are bounded per host class and metered like
host services. Without a hard bound, server-evaluated subscriptions become a
computation-offload channel: a module could declare a large set and infer world
state from which one fired, performing inference outside its fuel budget.

Telling a woken module *which* subscription fired is nonetheless safe, because
subscription conditions are knowledge-filtered like everything else. The module
learns only what its own knowledge could already establish. The exploit is
volume, not disclosure, and volume is what the bound addresses.

### Starvation

When a squad's command bandwidth is exhausted, subscriptions do not wake their
modules. The affected units continue on doctrine.

This is the intended degradation and it is what makes scarcity bite: a
bandwidth-starved force does not stop fighting, it stops *adapting*. A bounded
indication that wakes were missed should be available on the next funded
invocation, so doctrine can respond to having been starved rather than silently
losing information.

## Doctrine authorship

Doctrine has two authors.

1. **The unit's module**, which sets doctrine as invocation output.
2. **The player**, whose doctrine commands travel from the client to HQ and
   outward through the simulated communications topology exactly as any other
   command does.

A player who never writes a module still authors doctrine, through the canonical
client and the standard module. This preserves the established rule that writing
a custom module is not required to play, and makes the client's doctrine editor
a first-class feature rather than a wrapper around presets.

### Delegation policy

Requiring an invocation to adopt every player doctrine command would make
reconfiguring a force cost bandwidth per unit, which is prohibitive at scale.

A module therefore declares a **delegation policy**: which categories of
doctrine change headquarters may apply directly, without waking the module.
Commands inside that envelope are applied by the server. Commands outside it
require a wake, or are refused according to the policy.

This puts the module author in control of how much autonomy they cede to the
human commander, which is the correct place for that decision and a genuine
expression of doctrine design. A module written for a tightly directed force
delegates broadly; one written to operate independently delegates narrowly.

Delegation does not bypass the simulation. A doctrine command still travels the
communications path, still obeys range, jamming, interception, and equipment
rules, and still cannot reach a squad that is out of contact.

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
- wake frequency is not merely constrained but purchased from an allocated
  command-bandwidth budget, so a module cannot evade the per-invocation limit
  through wake spam and cannot treat frequent thinking as free; and
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

## Command bandwidth

### Why attention must cost something

Control modules are player-authored. Any capability that is free will be
maximised, so a design that merely rate-limits invocation produces a rate limit
that every competent player pins permanently. The limit becomes a floor rather
than a ceiling, worst-case server load becomes the continuous case rather than
the firefight case, and no interesting decision exists because every player
behaves identically.

Declarative standing behavior is only attractive when *not* using it costs
something. Attention is therefore a priced, allocated resource.

### What it meters

Command bandwidth prices the two things previously free:

- **wakes** — how often an instance is invoked, which is the dominant real cost
  because it is mostly knowledge-filtered observation construction and
  marshalling rather than WASM instruction execution; and
- **observation richness** — how much detail an instance receives when it does
  wake.

Per-invocation fuel and host-service quotas remain separate and unchanged. Fuel
governs how much a module may compute *within* one invocation. Command bandwidth
governs how often it is invoked and how well informed it is when it is.

### Two layers

Every unit has a **local floor** and may additionally draw on a **pooled
allocation**.

```text
local floor            always available, comms-independent,
                       equal for every instance of a host class

pooled command         allocated by the player, drawn through the
bandwidth              communications topology
```

The floor represents a unit's own onboard autonomy. It is enough to observe,
run standing doctrine, react under declared policy, and act. It is never taken
away, and it is identical for every instance of the same host class, which
preserves the baseline fairness the execution profile already guarantees.

The pooled layer represents networked command and processing support:
reachback analysis, richer sensor resolution, and more frequent deliberation
supplied through the squad and company network. It is finite, allocated, and
depends on connectivity.

A unit that loses its allocation is not disabled. It becomes autonomous and
unsupported — it still fights, still reacts, still follows doctrine, but it
thinks less often and sees in less detail.

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

This binds three systems that were previously only loosely related. Command
topology now governs control quality and not only orders and reports; losing a
squad leader's communications device degrades how well that squad thinks, not
merely what it is told; and electronic warfare gains a direct, legitimate attack
on the control layer.

### Bucket semantics

Allocation behaves as a bucket with both a capacity and a maximum flow rate.

Capacity permits deliberate reserve — holding attention back for an anticipated
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
  class. Two units may be invoked at different frequencies; they may not be
  given different amounts of computation per invocation.

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
replay reproduces the same allocations and therefore the same wake schedule.

### Open parameters

- The bandwidth unit, and the exchange rate between wakes and observation
  richness.
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
- adversarial doctrine rehearsal against scripted opposition;
- the standard control implementation as readable reference code;
- scenario fixtures with known inputs and expected requests;
- fuel, memory, message, and host-service profiling;
- pre-match compatibility and validation tools; and
- replay inspection of permitted inputs, outputs, validation failures, faults,
  and consumed budgets.

Debugging and replay data remain knowledge-scoped during competitive play.
Post-match disclosure policy must not undermine information that should remain
secret across an ongoing campaign.

### Doctrine rehearsal

The offline harness is not only a correctness tool. Because doctrine is intended
to be a primary form of mastery, a player must be able to **rehearse doctrine
adversarially** before committing it to a match: run a module against scripted
opposition, vary that opposition's behavior, and observe where the doctrine
fails.

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
