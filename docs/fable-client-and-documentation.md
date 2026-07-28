---
title: S.I.R. Fable Client and Interactive Documentation Architecture
status: accepted
decision-status: canonical
document-type: living-architecture
category: Design
categoryindex: 4
index: 13
version: "2.0"
last-updated: 2026-07-28
description: Shared .NET/Fable simulation, upstream FS.GG.Game compatibility, deterministic numerics, Elmish MVU browser tooling, replay verification, and delivery roadmap.
related:
  - docs/simulation-core-architecture.md
  - docs/codebase-architecture.md
  - docs/technology-stack.md
  - docs/public-protocol-architecture.md
  - docs/game-vision.md
---

# S.I.R. Fable Client and Interactive Documentation Architecture

## Executive summary

S.I.R. will use one deterministic F# gameplay kernel on the authoritative .NET
server, in headless design and balance tools, and in a Fable browser
application. The server is developed with published `FS.GG.Game` packages.
Fable compatibility for the reusable game substrate is therefore established
upstream in `FS.GG.Game`, not through copied algorithms or a permanent
S.I.R.-specific fork.

The generated FSharp.Formatting site is the explanatory and publication shell.
A separately built Elmish MVU application provides interactive rules-lab and
replay behavior inside that static site. Verified replay and editable sandbox
runs are visibly different modes: modifying a parameter creates a fork and
ends any claim that the run reproduces the authoritative match.

Exact cross-runtime behavior applies only to authoritative state, inputs,
events, and hashes. Rendering, interpolation, browser timing, charts, and
floating-point presentation remain outside that contract. The implementation
cannot begin until the upstream compatibility profile and the canonical
numeric rules have executable conformance tests.

## Decision

The authoritative gameplay kernel is one shared body of F# source compiled for
both .NET and JavaScript through Fable. Given the same validated state and
ordered gameplay inputs, both builds must produce exactly the same
authoritative state, events, and hashes at every tick.

An authorized full replay is therefore a deterministic re-simulation, not a
video and not a separately implemented JavaScript approximation. A browser can
download a versioned replay package, load the matching Fable engine bundle, and
run or inspect the match locally.

This decision covers the shared gameplay kernel. The .NET match host remains
authoritative, and presentation interpolation is not part of the equivalence
contract.

## Accepted decisions

| Decision | Status | Consequence |
|---|---|---|
| One shared F# gameplay kernel targets .NET and Fable | 🟩 **Canonical** | JavaScript cannot be a separate rewrite |
| The authoritative server is developed with `FS.GG.Game` | 🟩 **Canonical** | Framework integration is part of the server architecture |
| Fable support for reusable `FS.GG.Game` surfaces is implemented upstream | 🟩 **Canonical** | S.I.R. consumes a published, versioned artifact |
| Authoritative values use bounded integers and explicit fixed-point arithmetic | 🟩 **Canonical** | Floats cannot affect authoritative outcomes or hashes |
| The browser application uses Elmish MVU from its first implementation | 🟩 **Canonical** | Replay and laboratory state do not begin as ad-hoc DOM state |
| FSharp.Formatting builds the documentation corpus | 🟩 **Canonical** | Literate build-time evaluation and browser execution remain separate |
| Verified replay and editable sandbox are distinct modes | 🟩 **Canonical** | Parameter editing produces a derived run, never a verified replay |

Exact encodings, scales, limits, hash algorithms, package versions, and visual
design remain implementation parameters until their milestones accept them.

## Goals

- Make every gameplay formula inspectable beside the behavior it produces.
- Execute the same authoritative rule source on .NET and in the browser.
- Reproduce authorized completed matches from versioned replay packages.
- Detect and localize runtime divergence at the first tick and logical phase.
- Support interactive parameter sweeps without confusing experiments with
  canonical results.
- Reuse appropriate `FS.GG.Game` capabilities through published upstream
  contracts.
- Preserve a static GitHub Pages deployment with no privileged server
  requirement.
- Keep the browser responsive while replaying or sweeping large scenarios.
- Make old retained replays load the engine version that originally defined
  them.

## Non-goals

- The browser is not an authoritative match host.
- FSharp.Formatting is not the browser application framework.
- Build-time `.fsx` evaluation is not browser execution.
- The first browser host does not re-execute player WASM.
- The browser application is not the canonical live game client.
- The feature does not require every floating-point `FS.GG.Game` facility to
  become lockstep deterministic.
- The design does not choose final balance values before interactive testing.
- The feature does not expose hidden match state merely because replay code
  runs locally.

## Architectural principles

### One rule implementation

Gameplay rules are ordinary shared F# functions. A .NET adapter and a Fable
adapter may provide storage, transport, scheduling, and presentation, but they
cannot independently implement a combat, movement, communication, magic, or
random rule.

### Pure kernel, effectful hosts

The kernel accepts validated values and produces values. File access, HTTP,
browser APIs, clocks, worker messaging, Wasmtime, logging, rendering, and
storage live in hosts or adapters.

### Published dependency contracts

Sibling repositories are available for inspection and coordinated
development. S.I.R. release and CI builds consume a published, pinned
`FS.GG.Game` artifact. A sibling project reference may be used temporarily for
an explicitly recorded upstream integration test, but cannot become the
release dependency.

### Determinism is executable evidence

Claims such as “integer,” “pure,” or “same F#” are insufficient. A surface is
lockstep-compatible only after .NET and Fable execute the same boundary
fixtures and produce the same canonical bytes and digests.

### Rich presentation is a projection

The UI may derive charts, interpolation, tooltips, summaries, animations, and
colors from canonical values. None of those derived values may flow back into
the authoritative transition.

## Component architecture

```text
                           authored docs and literate .fsx
                                       │
                                       ▼
                              FSharp.Formatting
                           static HTML, API pages, search
                                       │
                        mount point + versioned asset manifest
                                       │
                                       ▼
                             Elmish MVU browser host
                       ┌───────────────┴────────────────┐
                       │                                │
                replay/lab worker                presentation views
                       │                         timeline, board, charts
                       ▼
              shared S.I.R. F# kernel
                  Fable compilation
                       ▲
                       │ same source and fixtures
                       ▼
              shared S.I.R. F# kernel
                   .NET compilation
                       ▲
           ┌───────────┴────────────┐
           │                        │
 authoritative match host    headless design tools
 Wasmtime + FS.GG.Game        batches and conformance
```

### Shared domain and kernel

The shared source owns:

- identifiers and canonical value types;
- ruleset and content values;
- immutable or controlled authoritative state;
- ordered inputs and authoritative events;
- fixed 20 Hz tick transitions;
- movement, perception, combat, communications, logistics, magic, and mission
  rules;
- counter-addressed random sampling;
- checkpoint import and export;
- canonical serialization inputs; and
- state and event digest material.

It cannot reference browser, React, Elmish, networking, filesystem, process
clock, Wasmtime, rendering, or server framework types.

### Authoritative .NET host

The .NET host owns:

- match admission and configuration;
- exact player-WASM execution through Wasmtime;
- input validation, target-tick assignment, and stable ordering;
- invoking the shared kernel;
- recovery snapshots and authoritative replay production;
- participant knowledge projections;
- result signing, storage, and delivery; and
- operational monitoring that cannot affect outcomes.

The server uses the published upstream `FS.GG.Game` compatibility set. S.I.R.
adapters retain ownership of semantic edges, equal-cost Chebyshev movement,
multi-cell cooperative movement, counter-addressed randomness, and any other
rule whose meaning differs from a generic framework operation.

### Headless .NET tools

Headless runners execute fixed scenarios, large batches, parameter sweeps,
replay verification, and conformance fixtures without rendering or real-time
waiting. Build-time literate scripts may call these tools to embed fixed
evidence, but interactive browser behavior uses the Fable build instead.

### Browser replay and rules-lab host

The browser host owns:

- loading and validating replay or scenario packages;
- selecting an exact retained engine bundle;
- starting and supervising replay/lab execution;
- Elmish model, messages, commands, subscriptions, and views;
- board, timeline, event, formula, comparison, and chart presentation;
- explicit verification state;
- sandbox parameter editing and derived-run identity; and
- export of non-authoritative experiment inputs and results.

It cannot silently upgrade an old replay, invent missing content, access live
hidden state, or present a sandbox fork as server-verified.

## Upstream `FS.GG.Game` contract

### Ownership and route

The versioned compatibility change is owned by the `FS.GG.Game` repository.
The eventual receiving issue must be filed there, added to the FS.GG
Coordination board, and identify:

```text
Owner: FS.GG.Game
Consumer: S.I.R.
Paths:
  src/Game.Core/
  tests/Game.Core.Tests/
  package/build configuration for Fable source delivery
  CI workflows for .NET/Fable conformance
Dependencies:
  accepted Fable compatibility profile
  pinned coherent Fable/FSharp.Core toolchain
Blocked by:
  none until the upstream issue identifies a real sequencing dependency
```

The org compatibility/dependency registry and a durable FS.GG ADR are updated
if this establishes a new cross-repository product promise.

### Old and new semantics

**Current contract:** `FS.GG.Game.Core` is a deterministic .NET library.
Individual public signatures describe their determinism scope, but the package
does not promise Fable compilation or .NET/Fable equality.

**Required contract:** the package exposes a documented Fable-consumable source
surface and a versioned compatibility profile. Every profiled operation states
whether it is lockstep exact, portable but non-lockstep, or unavailable on a
target. A lockstep claim is backed by common fixtures executed in both
runtimes.

### Compatibility grades

Classification occurs at the smallest stable public surface that has one
honest numeric contract. A module containing both integer grid functions and
floating presentation conversions cannot receive one blanket grade.

| Grade | Meaning | Permitted in S.I.R. shared authority |
|---|---|---|
| `LockstepExact` | Same canonical inputs produce byte-identical canonical outputs on supported runtimes | Yes, behind accepted S.I.R. semantics |
| `Portable` | Compiles and behaves within a documented tolerance or semantic range, but bytes may differ | No |
| `DotNetOnly` | Not provided or not supported through Fable | No |

Likely qualification candidates include integer cell addressing, canonical
edge and vertex relations, grid LOS, grid FOV, bounded path searches, and
selected map analysis. Sequential RNG, pixel conversion, continuous geometry,
continuous visibility, AI aim variation, effects using floats, ballistics,
physics, and float-returning analysis require individual decisions rather than
assumed inclusion.

### Upstream acceptance criteria

The upstream producer is accepted when:

- a documented project/package path compiles supported source through the
  pinned Fable toolchain;
- published NuGet artifacts contain the required Fable source and metadata;
- normal .NET consumers remain source- and binary-compatible within the
  declared version policy;
- every `LockstepExact` operation has shared fixtures;
- the fixtures run under .NET and Fable/Node in upstream CI;
- fixture outputs use canonical bytes rather than formatted diagnostic text;
- overflow, rounding, boundary coordinates, collection order, degenerate
  inputs, and restore behavior are covered;
- float-dependent or unsupported surfaces are accurately graded;
- the package records its Fable compiler, FSharp.Core, target, and compatibility
  profile versions; and
- a packed artifact, not only a sibling checkout, passes a consumer smoke test.

### Consumer acceptance criteria

S.I.R. adopts the upstream version only when:

- central package management pins the published version;
- the S.I.R. Fable build consumes the package through its supported package
  path;
- S.I.R. adapters pass semantic tests for Chebyshev movement, edge
  permeability, footprints, and random addressing;
- the canonical S.I.R. cross-runtime scenarios pass in both runtimes; and
- removing the sibling repository does not break restore, build, test, or
  documentation generation.

### Version and rollout policy

Producer publication precedes consumer adoption:

1. `FS.GG.Game` accepts the compatibility profile.
2. Upstream implementation and conformance become green.
3. A versioned package is published.
4. The compatibility registry records the producer version and consumer range.
5. S.I.R. updates its pin and runs its own acceptance suite.
6. S.I.R. removes any temporary sibling reference used by the integration
   branch.

A change that can alter a `LockstepExact` output is a replay-compatibility
change even if its .NET API signature is unchanged. It requires a new
compatibility identity and cannot silently replace a retained replay engine.

## Canonical numeric profile

### Permitted authoritative values

| Kind | Canonical use |
|---|---|
| `bool` and discriminated unions | Logical state and closed alternatives |
| bounded signed integers | Counts, coordinates, HP, Armor, Strain, capacities, durations |
| `int64`/`uint64` | Wide intermediates, ticks or identifiers where required, random and hash material |
| fixed-point integers | Fractions, rates, probabilities, modifiers, accumulated partial values |
| ordered records and collections | State, events, inputs, tables, and canonical serialization |

Floating-point values are allowed only in derived analysis or presentation.
They cannot be stored in authoritative state, accepted as authoritative input,
used by a branch that changes an outcome, or included in authoritative hashes.

### Bounded integer discipline

Every authoritative integer type declares:

- unit and meaning;
- inclusive valid range;
- construction and validation behavior;
- permitted operations;
- overflow policy; and
- wire representation.

An `int` is safe only because its range and intermediates are proven safe, not
because both targets expose a type named `int`. Multiplication, accumulation,
absolute difference, shifts, and coordinate subtraction use wide intermediates
when their mathematical result can exceed the stored range.

### Fixed-point discipline

Each fixed-point type declares its scale in one location. Code cannot combine
raw values from different scales without an explicit conversion.

```text
real value = raw integer / scale
```

Multiplication conceptually follows:

```text
wide = int64 left.raw * int64 right.raw
rounded = declaredRound(wide, scale)
result = declaredClamp(rounded)
```

Division checks zero before entering the kernel transition. Rules select a
named rounding operation—toward zero, floor, ceiling, or nearest with an
explicit tie rule. There is no implicit project-wide “close enough” rounding
and no conversion through `float`.

### Overflow and saturation

Domain constructors reject invalid external values. Internal arithmetic uses
one of three declared policies:

1. **Proven non-overflowing:** a range proof and boundary tests show the result
   fits.
2. **Saturating:** the result clamps to declared minimum or maximum because
   saturation is part of the gameplay rule.
3. **Rejected transition:** an impossible result returns a stable validation
   error before state commit.

Wraparound is never an accidental gameplay rule.

### Canonical ordering

- Entity and event processing use explicit stable keys.
- Dictionary or hash-set enumeration cannot define outcome order.
- Equal-priority conflicts use a documented total tie-break.
- Serialized maps and sets are emitted in canonical key order.
- Parallel computation may produce candidates in any schedule, but commit
  ordering is canonical.

### Random samples

Authoritative randomness is addressed by stable context:

```text
sample(
    secret match context,
    tick,
    event or action id,
    purpose,
    ordinal
)
```

Adding an unrelated random decision cannot shift an existing result. A
sequential `FS.GG.Game.Rng` may support offline map production, tests, or a
local derivation primitive only where the enclosing S.I.R. contract prevents
stream-position coupling. It is not the global combat random stream.

### Canonical bytes and hashes

State hashing operates on a canonical logical encoding with:

- explicit field order;
- explicit integer width and signedness;
- explicit byte order;
- canonical collection order;
- stable union-case and schema identifiers;
- length prefixes and absence markers; and
- no runtime object identity, default `GetHashCode`, locale, reflection order,
  or platform `BitConverter` assumption.

The encoding and hash algorithms remain open until their milestone, but once a
replay format version adopts them they are immutable for that version.

### Numeric conformance corpus

Fixtures include:

- minimum, maximum, zero, and adjacent boundary values;
- positive and negative division with every supported rounding rule;
- scale conversion and lost remainder;
- saturating addition, subtraction, multiplication, and accumulation;
- coordinate differences at domain edges;
- 32-bit operations whose mathematical intermediate exceeds 32 bits;
- 64-bit shifts, multiplication, and conversion;
- random-address avalanche and independence cases;
- canonical ordering with adversarial insertion order; and
- canonical byte vectors with expected hexadecimal output.

## Elmish MVU browser architecture

### Why Elmish is foundational

Replay loading, timeline control, board inspection, selection, comparisons,
parameter editing, verification, worker communication, errors, and routing form
one stateful application. Introducing Elmish after those behaviors exist would
require migrating both state ownership and effect handling. Elmish MVU is
therefore present from the first browser slice.

The intended stack is:

- `Fable.Elmish` for model, message, update, commands, subscriptions, and
  program lifecycle;
- `Fable.Elmish.React` for mounting and batched React rendering; and
- a typed F# React view DSL such as Feliz, pinned as part of the coherent web
  dependency set.

Exact package versions are selected during implementation and locked in both
NuGet and npm manifests.

### Application model

The conceptual model separates source data, execution, verification, and
presentation:

```fsharp
type RunMode =
    | VerifiedReplay
    | PerspectivePlayback
    | SandboxFork

type Verification =
    | NotApplicable
    | Pending
    | Verified of finalHash: string
    | Diverged of tick: int64 * phase: string
    | Unsupported of reason: string

type Model =
    { Mode: RunMode
      Source: ReplaySourceState
      Engine: EngineState
      Playback: PlaybackState
      Selection: SelectionState
      Experiment: ExperimentState
      Verification: Verification
      Presentation: PresentationState
      Diagnostics: DiagnosticState }
```

These names describe responsibilities, not a commitment to one large record.
Implementation may compose feature modules with child models and tagged
messages.

### Message families

Messages represent facts presented to the update function:

- replay or scenario selected;
- package bytes loaded or rejected;
- compatible engine located or unavailable;
- runner started, paused, advanced, sought, completed, or failed;
- checkpoint, event batch, projection, or divergence received;
- unit, cell, event, formula, or comparison selected;
- playback speed changed;
- sandbox parameter edited, accepted, or rejected;
- derived run started, completed, compared, or exported; and
- route or accessibility preference changed.

Messages do not contain callbacks into the kernel and do not perform effects
while being constructed.

### Commands and subscriptions

Elmish commands own:

- HTTP fetch and local-file reads;
- integrity verification;
- decompression;
- dynamic engine-bundle loading;
- browser-worker creation and requests;
- downloads and clipboard operations; and
- persisted UI preferences.

Subscriptions own long-lived browser event sources such as worker responses,
playback animation cadence, keyboard shortcuts, and route changes. Wall-clock
values control presentation only; advancing canonical simulation always uses
integer tick counts.

### Runner and worker boundary

Kernel execution sits behind a typed replay-runner protocol. The production
browser host runs large replays and sweeps in a Web Worker so React rendering
cannot stall the simulation and simulation cannot directly touch the DOM.

```text
UI → runner:
  Load(package, engine identity)
  Advance(max ticks or until event)
  Seek(target tick)
  Fork(parameter patch)
  Cancel(operation id)

runner → UI:
  Loaded(metadata)
  Progress(tick, summary)
  Checkpoint(tick, digest, projection)
  Events(tick range, visible event batch)
  Diverged(details)
  Completed(result)
  Failed(stable error)
```

Every request carries an operation identity. Results from a cancelled or
superseded operation are ignored deterministically by the UI model.

The worker owns the full kernel state. The Elmish model retains only the
current presentation projection, selected detail, metadata, and bounded
diagnostics. It does not copy the complete world into React state every tick.

### Batching and playback

A normal match contains approximately 24,000 authoritative ticks. Fast replay
or parameter sweeps must not dispatch one Elmish message or perform one React
render per simulated tick.

The runner advances by bounded work batches and reports at:

- requested visible ticks;
- event or breakpoint boundaries;
- checkpoint boundaries;
- progress intervals; and
- completion or divergence.

Real-time playback may display at 20 Hz or interpolate presentation between
received committed states. Accelerated replay advances as fast as the worker
budget allows while keeping cancellation and UI input responsive.

### View composition

The first complete application provides:

- package/source panel;
- verification and disclosure banner;
- board view with faction-appropriate knowledge;
- timeline and playback controls;
- selected unit, event, and formula inspector;
- input and event stream;
- state/hash checkpoint inspector;
- sandbox parameter editor;
- baseline/fork comparison; and
- diagnostics and first-divergence view.

Color always has a redundant text, shape, icon, or pattern channel.
Keyboard navigation, reduced motion, scalable text, screen-reader labels, and
non-color verification status are part of the first production-ready
definition rather than a later cosmetic pass.

### Update-function testability

Elmish `init` and `update` are tested on .NET where their shared types permit
and through Fable tests for browser-specific behavior. Tests assert:

- legal state transitions;
- effect requests rather than executed browser effects;
- stale worker-message rejection;
- cancellation;
- verified-to-sandbox transition on first parameter edit;
- inability to return from a modified fork to `Verified` without reloading the
  original package;
- bounded diagnostic retention; and
- accessibility state independent of kernel outcomes.

## FSharp.Formatting integration

### Responsibility boundary

FSharp.Formatting owns:

- corpus navigation and frontmatter;
- Markdown and literate `.fsx` rendering;
- API reference, source links, and F# tooltips;
- site search;
- build-time evaluation of fixed .NET examples;
- static images, diagrams, CSS, and explanatory tables; and
- the page containing the browser-app mount element.

The Fable/Elmish build owns the interactive JavaScript, worker, CSS, asset
manifest, and runtime behavior. `fsdocs --eval` never substitutes for the
browser conformance run.

### Page integration

The default fsdocs template is retained initially. Theme CSS and the supported
head/body injection points add only:

- the application mount container on designated pages;
- the versioned asset-manifest loader;
- feature-specific CSS; and
- accessible fallback content when JavaScript is unavailable.

A fully custom fsdocs template is deferred until an evidenced limitation
requires it because custom templates have a wider upgrade surface.

### Literate page pattern

A behavior page can contain four coordinated layers:

1. narrative and formulas;
2. visible shared F# source excerpts;
3. fixed .NET-evaluated examples embedded at documentation-build time; and
4. an Elmish widget or deep link that executes the Fable kernel interactively.

The page states the ruleset, content, engine, input, and result identities for
every fixed result. An interactive result states whether it is verified,
perspective-only, or a sandbox fork.

### Static-site assembly pipeline

```text
restore pinned .NET and npm dependencies
  → build .NET projects and XML docs
  → run .NET tests and canonical fixtures
  → compile Fable and run JavaScript conformance
  → bundle Elmish application and worker with Vite
  → build fsdocs with strict/evaluated examples
  → copy versioned web assets into the fsdocs output
  → validate links, asset manifest, root paths, and browser smoke tests
  → upload one immutable GitHub Pages artifact
```

Generated `output/`, Fable output, and Vite distribution directories are build
artifacts and are not committed. Tool manifests and both dependency lock files
are committed.

### Development loop

Documentation-only editing uses `fsdocs watch`. Browser-host editing uses Fable
watch and Vite. Integrated work runs both watchers through one repository
command and uses the same asset paths as the production assembly step.

The development server must not rely on an unrecorded sibling package build.
When coordinated upstream work is being tested, the active dependency mode and
exact upstream commit are visible in build output and evidence.

## Browser run modes

### Verified replay

- Inputs, engine, rules, content, random context, and checkpoints are immutable.
- The runner compares canonical state and event hashes.
- Successful completion displays `Verified`.
- Any mismatch displays the first divergent tick and phase.
- Editing a gameplay parameter creates a sandbox fork; it never mutates the
  loaded verified run.

### Perspective playback

- The package contains only projections and messages legitimately disclosed to
  that viewer.
- No full hidden-world kernel reconstruction is claimed.
- Timeline and inspection are restricted to delivered knowledge.
- The UI displays `Perspective playback`, not `Verified simulation`.

### Sandbox fork

- The fork records its parent engine, source package, fork tick, and parameter
  patch.
- It receives a derived identity.
- It can be compared with its baseline.
- It cannot display the authoritative verification badge.
- Export contains the patch and reproducible inputs, not a claim of server
  authority.

### Design scenario

Design-time fixed board states use the same sandbox machinery without requiring
a parent match replay. They identify their scenario, ruleset, content, seed,
parameters, engine, and result digest so a useful experiment can become a
permanent conformance or balance fixture later.

## Runtime boundary

```text
player inputs + unit WASM artifacts
                 │
                 ▼
       authoritative .NET match host
        ├── Wasmtime execution
        └── validation and stable ordering
                 │
                 ▼
        shared F# simulation kernel
         (.NET and Fable builds)
                 │
       ┌─────────┴──────────┐
       ▼                    ▼
server result and      versioned replay package
authoritative hashes          │
                              ▼
                    Fable browser replay host
                    + recorded accepted inputs
```

The browser replay host is not a second authority. Its result is evidence that
the recorded inputs reproduce the server's kernel hashes.

## Exact equivalence contract

The cross-runtime contract includes:

- authoritative state after every committed tick;
- authoritative event identity, contents, and order;
- action, effect, observation, communication, and resource outcomes;
- deterministic random samples addressed by their stable keys;
- kernel checkpoints, state hashes, event hashes, and final outcome; and
- validation and rejection results for replay-driving gameplay inputs.

The contract excludes rendering, animation, audio, wall-clock scheduling,
network timing, browser UI state, telemetry, and other presentation or
operational values.

Authoritative code uses integer ticks, integer grid coordinates, and declared
fixed-point representations. Every conversion, division, rounding rule,
overflow behavior, comparison, collection traversal, and tie-breaker that can
affect an outcome is explicit. Platform-dependent floating-point behavior,
transcendental functions, locale-sensitive conversion, object hash codes,
unordered map iteration, process clocks, and ambient entropy cannot influence
the kernel.

Randomness is counter-addressed by stable gameplay facts rather than consumed
from a mutable global stream. Serialized collections have canonical order.
Inputs receive authoritative tick and sequence keys before the kernel consumes
them.

## Shared source, not equivalent rewrites

`SIR.Domain` and the authoritative parts of `SIR.Simulation` must compile from
the same F# source for .NET and Fable. Platform adapters sit outside that
source. Conditional compilation is permitted only at declared adapter
boundaries and cannot provide two different implementations of a gameplay
rule.

Any dependency used by the shared kernel must either:

1. have demonstrated equivalent .NET and Fable behavior under the canonical
   conformance suite; or
2. remain behind an adapter whose shared deterministic output is completely
   specified and tested.

## WASM execution and recorded control outputs

The authoritative .NET match invokes the exact per-unit WASM artifacts through
the pinned Wasmtime execution profile. The match host validates and orders the
resulting unit intentions, service requests, and other control outputs before
passing them to the shared simulation kernel.

The browser replay does **not** implicitly claim to reproduce Wasmtime
execution, fuel accounting, instance memory, traps, or host-call scheduling.
Instead, a full replay package contains the accepted, validated, and ordered
WASM control outputs that crossed into the shared kernel. The Fable replay host
injects those recorded outputs at the same ticks.

This creates two distinct verification levels:

| Verification | Execution | What it proves |
|---|---|---|
| Authoritative verification | .NET re-executes the exact WASM artifacts under the pinned profile, then runs the shared kernel | Module execution and complete match reconstruction agree |
| Browser kernel verification | Fable injects the recorded accepted control outputs into the shared kernel | Cross-runtime gameplay state, events, and results agree |

A future browser WASM host may add full module verification only after it passes
the same artifact, ABI, fuel, memory, trap, host-service, and scheduling
conformance requirements. Until then, the browser UI must accurately label the
verification level.

## Full replay package

A full replay package contains or unambiguously identifies:

| Field | Purpose |
|---|---|
| Replay format version | Selects decoding and validation rules |
| Engine and ruleset hash | Selects the exact compatible kernel bundle |
| Content and map hashes | Identifies all authoritative data |
| Initial snapshot | Establishes the complete starting state |
| Match random context | Reproduces addressed random samples; disclosed only when policy permits |
| Ordered external inputs | Reconstructs player, server, and mode decisions |
| Accepted WASM control outputs | Drives browser kernel replay without re-running Wasmtime |
| WASM artifact and execution-profile identities | Supports authoritative verification and audit |
| Periodic checkpoints | Permits efficient seeking and bounded reconstruction |
| State and event hashes | Detects the first divergent tick |
| Final outcome and hash | Confirms the reconstructed result |

The serialization format uses canonical field and collection ordering. Packages
are versioned, size-limited, validated before execution, and may be compressed
without changing their canonical decoded meaning.

The replay host seeks by loading the nearest preceding checkpoint and
re-simulating forward. On mismatch it reports at least the first divergent tick,
logical phase, affected identifiers, expected and actual hashes, and relevant
inputs and events.

## Engine bundle retention

Replays bind to an exact engine/rules compatibility identity. The documentation
site publishes immutable Fable replay bundles under versioned or content-hashed
paths, conceptually:

```text
replay/engines/<engine-rules-hash>/engine.js
```

Opening an old replay loads its matching retained bundle. The current engine
must never silently reinterpret an incompatible old package. If the required
bundle or content is unavailable, the replay is explicitly unsupported rather
than presented with altered results.

Release and retention policy must preserve every bundle required by retained
replays. A bundle's build manifest records compiler, package, source, rules,
content-schema, and replay-format identities.

## Disclosure modes

Replay delivery follows the same knowledge and authorization policy as the
public protocol.

### Authorized full replay

A full replay contains enough authoritative state and inputs to re-simulate the
complete match. It is normally suitable after match completion or for
authorized administration, testing, and balance research. It must not be
delivered while its hidden state, random context, opponent artifacts, or
observations would grant an illegitimate advantage.

### Player-perspective replay

A player-perspective replay contains the knowledge-filtered projections and
messages that the participant legitimately received. It is projection playback,
not complete world re-simulation: hidden state cannot be reconstructed from a
properly filtered package. The UI must not describe a perspective replay as
full deterministic verification.

Spectator and campaign policies may define additional projection scopes, but
each scope is explicit and authenticated.

## Cross-runtime conformance

CI runs canonical scenario and replay fixtures through:

1. the .NET shared-kernel runner;
2. the Fable build under a JavaScript test host; and
3. the browser-published engine artifact where packaging could alter behavior.

Each runner consumes the same decoded fixtures and must produce byte-identical
canonical event and state digests at declared checkpoints and at completion.
The gate fails on the first mismatch. Diagnostics preserve the first divergent
tick and phase rather than reporting only the final hash.

Fixtures cover boundary arithmetic, overflow and rounding, collection ordering,
random addressing, movement conflicts, LOS, combat, communications, magic,
accepted and rejected inputs, checkpoint restore, and replay seeking. Every
gameplay bug caused by runtime disagreement receives a permanent reduced
fixture.

## Security and trust

- The server result remains authoritative even when a browser produces a
  different result.
- Replay data is untrusted input and receives strict schema, size, count,
  recursion, decompression, and resource limits.
- A browser engine has no live-match privileged channel and receives no hidden
  information beyond the replay's authorized disclosure scope.
- Published packages and engine bundles carry server-issued identities and
  integrity hashes.
- Replaying recorded accepted WASM outputs does not attest that arbitrary WASM
  would have produced them; only authoritative verification provides that
  assurance.

## Consequences

The shared kernel can power interactive literate examples, balance experiments,
and post-match replay inspection in a static browser site while the same rules
remain authoritative on .NET. Formula changes become directly testable against
fixed scenarios in both runtimes.

The cost is a stricter implementation subset, an exact conformance gate, and
retention of historical web bundles. These costs are accepted because silent
runtime drift would make browser behavior misleading.

The upstream choice adds coordination and release sequencing. It avoids a
larger long-term cost: duplicated algorithms, hidden source references, and an
S.I.R.-only framework fork.

Elmish and the worker boundary add structure to the first browser slice. That
structure is accepted because replay loading, cancellation, verification,
timeline control, and sandbox forking are already stateful requirements rather
than speculative UI complexity.

## Rejected alternatives

- **A separately rewritten JavaScript simulator:** rejected because behavioral
  drift would be inevitable and difficult to diagnose.
- **Replaying only video or server snapshots:** rejected as the only replay
  form because it cannot validate formulas or support deterministic inspection.
- **Running every old replay with the current engine:** rejected because rule
  evolution would silently rewrite history.
- **Claiming full browser verification without Wasmtime conformance:** rejected
  because recorded kernel inputs and module execution are different trust
  boundaries.
- **Using a full replay package for player-perspective playback:** rejected
  because client-side filtering cannot protect secrets already delivered.
- **Copying `FS.GG.Game` algorithms into S.I.R.:** rejected because it creates
  a second implementation and defeats upstream reuse.
- **Permanent sibling source references:** rejected because builds would depend
  on unversioned external checkout state.
- **Ad-hoc DOM state before Elmish:** rejected because the known replay and lab
  state machine would later require a disruptive migration.
- **Treating `fsdocs --eval` as browser execution:** rejected because it runs
  examples on .NET during site generation and cannot establish Fable behavior.
- **One mode that permits both verification and parameter editing:** rejected
  because a modified run is no longer the recorded match.

## Risks and mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| Upstream scope expands to “make every float exact” | Fable enablement stalls | Grade public operations; require exactness only for certified lockstep surfaces |
| Current package layout cannot cleanly expose a Fable subset | Consumer cannot use published artifacts | Upstream packaging spike before promising a package shape |
| JavaScript integer behavior diverges from .NET | Silent gameplay mismatch | Bounded values, wide intermediates, canonical byte vectors, dual-runtime gates |
| BigInt is overused in hot loops | Browser replay becomes slow | Use bounded 32-bit storage where proven; benchmark wide operations |
| Mutable collection insertion order leaks into results | Replay differs after harmless refactor | Canonical total ordering and adversarial-order fixtures |
| Historical bundle is deleted | Retained replay becomes unreadable | Couple replay retention to immutable engine-bundle retention |
| Player-perspective package leaks full inputs | Hidden state can be reconstructed | Produce filtered packages server-side; never filter a full package in browser |
| Worker protocol floods Elmish | Browser UI stalls | Bounded batches, summaries, cancellation, and backpressure |
| Parameter editor appears authoritative | Users misread experimental results | Mandatory mode banner, derived identity, irreversible fork transition |
| Custom fsdocs template breaks on upgrade | Documentation deployment fails | Begin with supported CSS/head/body extension points |
| Two dependency ecosystems drift | Reproducibility degrades | Pin .NET tool/package and npm versions; commit lock files and build manifest |
| Upstream API changes without output signature change | Old replay silently changes | Treat lockstep output changes as replay compatibility changes |

## Roadmap

### Roadmap legend

Color is paired with text and checkbox state:

| Marker | Meaning |
|---|---|
| 🟩 **Complete** | Accepted design or finished milestone with evidence |
| 🟦 **Ready** | Defined and unblocked; may begin when its predecessor is accepted |
| 🟨 **Blocked** | Defined but waiting on a real dependency |
| 🟧 **Exploratory** | Requires a bounded spike before the implementation shape is accepted |
| 🟥 **Gate failed** | Evidence contradicts the required contract; dependent work stops |

Checkboxes record completion only. Color records present scheduling state.

### [x] 🟩 M0 — Capability research and architectural choice

**Outcome:** official Fable and FSharp.Formatting capabilities and the current
`FS.GG.Game` repository were inspected. The project accepted upstream Fable
enablement, integer/fixed-point authority, and Elmish MVU from the first browser
slice.

**Evidence:**

- documented current runtime and documentation-tool boundaries;
- inventoried `FS.GG.Game.Core`, Harness, Playtest, and product skills;
- identified numeric, packaging, path-cost, edge, RNG, and float boundaries;
  and
- recorded the accepted choices in this document.

### [x] 🟩 M1 — Canonical feature architecture

**Outcome:** responsibilities, trust boundaries, run modes, numeric rules,
upstream ownership, Elmish structure, fsdocs integration, security posture, and
rollout order are canonical.

**Acceptance:**

- no browser authority is implied;
- verified replay and sandbox fork are distinct;
- WASM replay and verification levels are explicit;
- the upstream producer and downstream consumer criteria are separate; and
- unresolved implementation parameters remain visible.

### [x] 🟩 M2 — Upstream compatibility proposal

**Owner:** `FS.GG.Game`

**Outcome:** `FS.GG.Game` accepted a bounded, falsifiable compatibility
proposal. The existing `FS.GG.Game.Core` package remains the single producer
artifact and gains a supported Fable source view through `Fable.Package.SDK`.
Compatibility is graded per stable function as `LockstepExact`, `Portable`, or
`DotNetOnly`; exact claims require shared canonical-byte fixtures under pinned
.NET and Fable/Node toolchains.

The selected spike covers `Cell`, `Edges.edgeBetween`, one integer LOS
operation, one bounded pathfinding operation, and a clean consumer that
restores only the packed artifact. Permanent sibling references, copied
algorithms, and a second public package without a new decision are rejected.

**Evidence:**

- receiving request
  [FS.GG.Game#526](https://github.com/FS-GG/FS.GG.Game/issues/526) records
  owner, consumer, paths, dependencies, old/new semantics, producer and
  consumer acceptance, version policy, and rollout;
- the accepted
  [upstream proposal](https://github.com/FS-GG/FS.GG.Game/blob/main/docs/reports/2026-07-28-fable-lockstep-compatibility-proposal.md)
  records the function profile, package alternatives, canonical fixture format,
  clean-consumer test, and CI stages;
- org
  [ADR-0069](https://github.com/FS-GG/.github/blob/main/docs/adr/0069-fable-lockstep-is-a-profiled-game-core-package-contract.md)
  makes the cross-repository package/profile decision durable; and
- the request is scheduled on the FS-GG Coordination board as
  `fs-gg-game-fable-lockstep`, phase `P6 Game`, status `Ready`.

Producer publication remains sequenced before registry activation and S.I.R.
adoption. The receiving issue stays open for M3/M4 implementation; completing
this proposal milestone does not claim that a function is already qualified.

### [ ] 🟦 M3 — Upstream Fable packaging and compilation spike

**Purpose:** retire packaging and compiler uncertainty before broad conversion.

**Minimum surface:**

- `Cell` and stable identifiers;
- one canonical edge relation;
- one integer LOS fixture;
- one bounded pathfinding fixture; and
- one package-consumer smoke test.

**Acceptance:**

- the selected upstream source compiles under pinned .NET and Fable toolchains;
- the packed NuGet artifact is consumable without the sibling checkout;
- .NET public compatibility remains within the accepted policy;
- Node executes the package-derived JavaScript; and
- unsupported APIs or packaging faults are recorded with reduced reproductions.

**Failure transition:** mark 🟥 and return to M2 if the selected package shape
cannot support the minimum surface without unacceptable API or source
duplication.

### [ ] 🟨 M4 — Upstream lockstep compatibility profile

**Blocked by:** M3.

**Deliverables:**

- versioned `LockstepExact`/`Portable`/`DotNetOnly` declarations;
- shared canonical input and output vectors;
- .NET and Fable/Node CI jobs;
- numeric and collection-order boundary suite;
- packaged-artifact consumer test; and
- published upstream version.

**Acceptance:**

- every exact claim has executable cross-runtime evidence;
- float-dependent surfaces are not accidentally certified;
- output-changing fixes update compatibility identity; and
- the package version and profile are recorded in the FS.GG compatibility
  registry.

### [ ] 🟨 M5 — S.I.R. solution and shared numeric foundation

**Blocked by:** M4.

**Deliverables:**

- root solution and centrally pinned dependency set;
- `SIR.Domain` and minimal `SIR.Simulation`;
- canonical bounded integer and fixed-point types;
- explicit rounding, saturation, ordering, and encoding primitives;
- .NET and Fable test projects sharing fixtures; and
- published `FS.GG.Game` package consumption.

**Acceptance:**

- no sibling checkout is required;
- numeric boundary vectors match exactly;
- package/tool lock files are committed;
- authoritative projects contain no floating state; and
- a deliberately introduced divergence fails at its first fixture.

### [ ] 🟨 M6 — Minimal shared simulation slice

**Blocked by:** M5.

**Scope:** fixed board state, two units, one semantic edge, deterministic
movement, one observation, one attack, one event stream, and one state digest.

**Acceptance:**

- .NET and Fable produce identical events and final bytes;
- S.I.R. Chebyshev and semantic-edge adapters are exercised;
- repeated execution is order-independent where promised;
- a first-divergence report identifies tick and phase; and
- no rendering or network dependency enters the kernel.

### [ ] 🟨 M7 — Replay format and runner

**Blocked by:** M6.

**Deliverables:**

- versioned replay schema;
- canonical encoding and selected hashes;
- initial snapshot, ordered inputs, accepted WASM-output journal, checkpoints,
  event/state hashes, and final result;
- .NET authoritative verifier; and
- Fable/Node kernel replay runner.

**Acceptance:**

- seek from every retained checkpoint reaches the same final digest;
- corrupt, oversized, incompatible, and unauthorized packages fail safely;
- old/current engine mismatch is explicit;
- browser-level verification does not claim Wasmtime verification; and
- perspective packages cannot reconstruct hidden state.

### [ ] 🟨 M8 — Elmish replay shell

**Blocked by:** M7.

**Deliverables:**

- Elmish model, message, update, commands, and subscriptions;
- React mount inside a standalone development page;
- replay loading and validation states;
- play, pause, step, seek, speed, selection, and verification status;
- typed runner boundary; and
- stale-operation and cancellation tests.

**Acceptance:**

- browser effects are requested through commands/subscriptions;
- update remains deterministic for equal messages and model;
- verified, perspective, sandbox, unsupported, and divergent states are
  visually and textually distinct;
- parameter editing transitions to a derived sandbox identity; and
- keyboard and screen-reader operation cover primary controls.

### [ ] 🟨 M9 — Worker execution and responsive inspection

**Blocked by:** M8.

**Deliverables:**

- versioned worker protocol;
- bounded execution batches and progress;
- cancellation and supersession;
- board, timeline, event, formula, checkpoint, and divergence inspectors; and
- performance measurements at normal match length.

**Acceptance:**

- a 24,000-tick replay does not require 24,000 React renders;
- UI input remains responsive during accelerated replay;
- worker termination cannot leave the app in a false verified state;
- full world state is not copied into Elmish on every tick; and
- performance observations never enter canonical hashes.

### [ ] 🟨 M10 — Interactive rules laboratory

**Blocked by:** M9.

**Deliverables:**

- fixed design-scenario catalog;
- typed parameter editor with validation;
- sandbox fork provenance;
- baseline/fork comparison;
- deterministic sweep runner;
- tables and charts; and
- reproducible experiment export.

**Acceptance:**

- exact inputs and engine identity accompany every result;
- changing one parameter never mutates the baseline;
- a useful experiment can be promoted into a permanent fixture;
- charts remain derived from canonical integer results; and
- balance evidence is labeled separately from accepted balance.

### [ ] 🟨 M11 — Literate corpus integration

**Blocked by:** M8; full lab pages also depend on M10.

**Deliverables:**

- pinned local `fsdocs-tool`;
- frontmatter/navigation integration;
- strict build with evaluated fixed examples;
- API reference and source links;
- Elmish mount page and asset manifest;
- CSS using the existing color/status vocabulary; and
- no-JavaScript fallback explanation.

**Acceptance:**

- build-time .NET output and browser Fable output identify their runtime;
- generated links work at the GitHub Pages project root;
- default-template upgrades remain viable;
- site search includes the explanatory corpus;
- interactive assets are versioned and integrity-checked; and
- generated outputs remain uncommitted.

### [ ] 🟨 M12 — Versioned engine publication and GitHub Pages

**Blocked by:** M7 and M11.

**Deliverables:**

- immutable engine/rules bundle paths;
- build manifest;
- combined fsdocs/Vite Pages artifact;
- retention checks;
- browser smoke and accessibility tests; and
- deployment workflow.

**Acceptance:**

- an old fixture selects its matching engine bundle;
- missing engines fail explicitly;
- Pages deploys from a reproducible locked build;
- ordinary replay browsing requires no privileged server;
- no source map or package exposes live-match secrets; and
- retained replay policy cannot outlive required engine artifacts.

### [ ] 🟨 M13 — Full match replay qualification

**Blocked by:** authoritative match/WASM implementation and M12.

**Deliverables:**

- completed .NET match replay package;
- accepted WASM-output journal;
- full browser kernel replay;
- authoritative .NET WASM re-execution comparison;
- perspective playback package; and
- disclosure/security review.

**Acceptance:**

- browser kernel hashes equal authoritative hashes;
- .NET verification re-executes exact WASM artifacts and outputs;
- the UI labels both verification levels accurately;
- the perspective package reveals no unauthorized state; and
- first-divergence diagnostics survive an intentionally corrupted fixture.

### [ ] 🟧 M14 — Optional browser WASM verification research

**Blocked by:** M13. This is not required for the canonical browser replay.

**Question:** can a browser WASM host reproduce the pinned Wasmtime ABI,
instance state, fuel, traps, scheduling, and host services closely enough for
full authoritative verification?

**Exit:** accept a separate implementation milestone only if the spike passes
the complete module-execution conformance contract. Otherwise retain recorded
accepted outputs as the canonical browser boundary.

## Definition of feature-ready

The feature is ready for ordinary design use after M10 when:

- the browser and .NET kernel pass exact shared fixtures;
- the lab can run, fork, compare, and export fixed scenarios;
- experimental results are unmistakably non-canonical; and
- failures identify actionable divergence rather than only a final mismatch.

It is ready for public documented use after M12 when the literate corpus and
versioned application deploy reproducibly to GitHub Pages.

It is ready for authoritative match replay after M13 when a real server package
passes kernel replay, WASM re-execution, disclosure, and retention gates.

## Open implementation parameters

- Canonical replay encoding and compression.
- State and event hash algorithms and hash-tree layout.
- Checkpoint cadence and retained diagnostic detail.
- Engine-bundle and replay retention duration.
- Exact browser resource limits and worker topology.
- Fixed-point scales by subsystem.
- Exact bounded ranges for authoritative numeric types.
- Upstream `FS.GG.Game` source/package partition.
- Pinned Fable, Elmish, React, view DSL, Vite, Node, and fsdocs versions.
- Worker batch budgets and projection cadence.
- Scenario and experiment export schema.
- Browser smoke-test and accessibility-test tooling.

These choices may change without weakening the shared-source, exact-equivalence,
upstream-publication, numeric, Elmish-MVU, version-binding, disclosure, or
verification-level decisions above.

## References

- [Fable JavaScript compatibility](https://fable.io/docs/javascript/compatibility.html)
- [Fable CLI](https://fable.io/docs/getting-started/cli.html)
- [Fable JavaScript build and Vite workflow](https://fable.io/docs/javascript/build-and-run.html)
- [Authoring Fable-compatible packages](https://fable.io/docs/your-fable-project/author-a-fable-library.html)
- [Elmish architecture](https://elmish.github.io/elmish/)
- [Elmish React integration](https://elmish.github.io/react/)
- [FSharp.Formatting content](https://fsprojects.github.io/FSharp.Formatting/content.html)
- [FSharp.Formatting literate scripts](https://fsprojects.github.io/FSharp.Formatting/literate.html)
- [FSharp.Formatting output evaluation](https://fsprojects.github.io/FSharp.Formatting/evaluation.html)
- [FSharp.Formatting styling](https://fsprojects.github.io/FSharp.Formatting/styling.html)
- [FSharp.Formatting GitHub Pages guide](https://fsprojects.github.io/FSharp.Formatting/zero-to-hero.html)
