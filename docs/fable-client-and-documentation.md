---
title: S.I.R. Fable Client and Interactive Documentation Architecture
status: accepted
decision-status: canonical
document-type: living-architecture
category: Design
categoryindex: 4
index: 13
version: "3.0"
last-updated: 2026-07-29
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
floating-point presentation remain outside that contract. Implementation began
only after the upstream compatibility profile and canonical numeric rules had
executable conformance tests; the versioned replay format and shared runner now
extend that evidence through deterministic checkpoint seeking. A standalone
Elmish/React shell now exposes the replay state machine, typed runner boundary,
mode disclosures, accessible controls, and irreversible sandbox transition
without moving authority into the browser. Replay execution now runs in a
dedicated versioned Web Worker, yields between bounded batches, and returns
compact inspection projections rather than copying the complete world into
Elmish state. The same worker now runs a fixed, versioned rules-laboratory
catalog with bounded typed parameters, immutable baseline/fork comparisons,
deterministic integer sweeps, explicit exploratory-evidence labeling, and
reproducible experiment exports.
The explanatory corpus, evaluated .NET evidence, generated API reference, and
Fable application now build together through a pinned, strict
FSharp.Formatting pipeline. The default fsdocs template supplies navigation
and search while supported extension points mount the versioned browser bundle
with an integrity manifest and an explicit no-JavaScript fallback.
The combined static artifact now publishes retained replay workers beneath
immutable engine-identity paths, verifies retention and SHA-384 integrity
through a publication manifest, rejects sensitive runtime material, and
deploys from the locked build through GitHub Pages.
A bounded authoritative match host now runs an immutable binary player module
through the pinned Wasmtime profile, records its accepted outputs, emits full
and knowledge-filtered replay packages, and re-executes the exact artifact
before granting the stronger authoritative verification claim. Browser replay
continues to claim kernel verification only.
The optional browser-WASM spike reproduced common core semantics, instance
state, host calls, scheduling, and explicit traps, but rejected authoritative
browser verification because native browser WebAssembly has no deterministic
equivalent of Wasmtime fuel or its out-of-fuel boundary.
The completed v1 implementation parameters are now recorded in one
machine-readable compatibility baseline and checked against executable source,
locked toolchains, browser protocol, publication policy, and test harnesses.
Only production policy that depends on future game scale remains outside the
v1 contract.

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
- the request was scheduled on the FS-GG Coordination board as
  `fs-gg-game-fable-lockstep`, phase `P6 Game`, and completed through M4.

Producer publication was sequenced before registry activation and S.I.R.
adoption. The receiving issue stayed open through M3/M4 implementation;
completing the proposal milestone did not itself qualify a function.

### [x] 🟩 M3 — Upstream Fable packaging and compilation spike

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

**Outcome:** the existing `FS.GG.Game.Core` package now carries a bounded Fable
source view built with `Fable.Package.SDK`. The view packages the same canonical
`Primitives`, `Pathfinding`, `Edges`, and `Los` implementation files used by the
.NET assembly, together with a versioned spike profile, fixture schema, and
toolchain manifest. It does not copy algorithms or introduce a second public
package.

An initial whole-project package compile failed on unsupported float-heavy BCL
calls including `System.Double.IsFinite`, `System.Math.ScaleB`, and
`System.Math.ILogB`. The accepted view therefore exposes only the selected
integer/grid source files and records every other surface as unclassified.
This is the intended bounded-spike result, not an assembly-wide compatibility
claim.

The isolated consumer packs a synthetic exact-version artifact, restores it
outside the producer checkout into an empty global-packages directory, compiles
the package-derived source with Fable 5.13.0, and executes it under Node 26.
Smoke fixtures cover `Cell` ordering, `Edges.edgeBetween`, symmetric integer
LOS, and bounded four-way `Pathfinding.astar`. Normal .NET public surface gates,
Linux and Windows builds, and all 872 .NET tests remain green.

**Evidence:**

- merged upstream implementation
  [FS.GG.Game#528](https://github.com/FS-GG/FS.GG.Game/pull/528);
- the producer's
  [M3 result](https://github.com/FS-GG/FS.GG.Game/blob/main/docs/reports/2026-07-28-fable-lockstep-compatibility-proposal.md#m3-result)
  records the successful package shape and the rejected whole-project shape;
- `scripts/test-fable-package-consumer.sh` and the
  `Packed Fable consumer (Fable/Node)` pull-request gate prove artifact-only
  restore, compilation, and Node execution; and
- issue
  [FS.GG.Game#526](https://github.com/FS-GG/FS.GG.Game/issues/526) carried the
  M4 expansion, publication, and registry activation through completion.

At M3, the four exercised functions were not yet promoted to `LockstepExact`;
canonical binary cross-runtime vectors, expanded classification, publication,
and registry activation remained M4 work.

### [x] 🟩 M4 — Upstream lockstep compatibility profile

**Unblocked by:** completed M3 packaging and compilation spike.

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

**Outcome:** `FS.GG.Game.Core` 0.13.0 now publishes the bounded
`fs-gg-game-core-fable-lockstep-v1` profile. Cell ordering,
`Edges.edgeBetween`, `Los.lineOfSightBy`, and `Pathfinding.astar` are certified
`LockstepExact`; packaged float value types are `Portable`; explicitly excluded
modules remain `DotNetOnly`. One generated fixture vector is compiled by both
the .NET package consumer and Fable/Node, and both must reproduce the packaged
442-byte oracle with SHA-256
`9c0d7128f7b9558ef7e985618b1165199f83a0bb4af7533b175cc95b340c4a24`.
Boundary coverage includes full-width ordering, coordinate extremes, reversed
and corner LOS, equal-cost paths, exhaustion, unreachable paths, and
adversarial blocked-cell insertion order.

**Evidence:**

- [FS.GG.Game#531](https://github.com/FS-GG/FS.GG.Game/pull/531) merged the
  compatibility profile, canonical vectors, first-byte diagnostics, separate
  .NET/Fable conformance legs, and isolated packaged-consumer runner;
- tag `v0.13.0` and release run
  [30395301449](https://github.com/FS-GG/FS.GG.Game/actions/runs/30395301449)
  published the coherent Core/Render/Harness set;
- Core, Render, and Harness were independently downloaded from GitHub Packages
  and nuget.org, with all 43/9/19 payload entries matching byte for byte after
  excluding nuget.org signatures;
- [FS-GG/.github#1852](https://github.com/FS-GG/.github/pull/1852) activated
  `game-sim-core@0.13.0` and the profile in the org compatibility registry; and
- [FS.GG.Game#532](https://github.com/FS-GG/FS.GG.Game/pull/532) finalized the
  upstream compatibility report and closed the receiving request.

### [x] 🟩 M5 — S.I.R. solution and shared numeric foundation

**Unblocked by:** completed M4 compatibility profile and published package.

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

**Outcome:** the root `SIR.slnx` now contains `SIR.Domain`, the minimal
`SIR.Simulation` package seam, and separate .NET and Fable/Node conformance
hosts linked to the same fixture source. Dependencies and tools are centrally
locked to .NET SDK 10.0.302, Fable 5.13.0, Node 26.5.0,
`@fable-org/fable-library-js` 2.5.1, `FSharp.Core` 10.1.302, and the exact
published `FS.GG.Game.Core` 0.13.0 package. NuGet and npm lock files are
committed; the simulation consumes no sibling checkout or project reference
outside this repository.

`BoundedInt32` carries an inclusive range and rejects invalid construction or
mixed-bound operations. Its add and subtract operations saturate through
64-bit intermediates. `FixedPoint` stores four base-ten places in a signed
32-bit raw value, uses 64-bit intermediates, saturates overflow, and rounds
nearest with ties away from zero. Canonical primitives define signed-value
ordering and little-endian encoding without platform-dependent conversion.
Authoritative source contains no floating-point type or operation.

**Evidence:**

- [S.I.R.#53](https://github.com/EHotwagner/S.I.R./pull/53) introduced the
  foundation, shared fixtures, lock files, clean-checkout gate, and roadmap
  transition;
- `scripts/test-conformance.sh` restores in locked mode from nuget.org into an
  isolated package cache, builds both hosts, compiles the package-derived Fable
  source, and runs it under Node;
- ten shared boundary fixtures produce the same 60 canonical bytes in .NET
  and Fable/Node, with SHA-256
  `99a578d8dbb135b9bb33cdaebed9e10a347398abd394474668b1ca75f3689eb4`;
- the vector covers bounded overflow and underflow, positive and negative
  half-away rounding, fixed-point saturation and multiplication, signed
  ordering, little-endian encoding, and an `FS.GG.Game.Core.Cell` restored
  from the published package;
- a deliberate mutation of `bounded-add-overflow-saturates` is rejected at
  that first changed fixture and byte zero by both runtime hosts; and
- the pull-request CI runs the same conformance, lock, published-package, and
  floating-source gates from a clean checkout.

### [x] 🟩 M6 — Minimal shared simulation slice

**Unblocked by:** completed M5 shared numeric foundation.

**Scope:** fixed board state, two units, one semantic edge, deterministic
movement, one observation, one attack, one event stream, and one state digest.

**Acceptance:**

- .NET and Fable produce identical events and final bytes;
- S.I.R. Chebyshev and semantic-edge adapters are exercised;
- repeated execution is order-independent where promised;
- a first-divergence report identifies tick and phase; and
- no rendering or network dependency enters the kernel.

**Outcome:** `SIR.Simulation` now owns a headless one-tick kernel slice with a
fixed 3×2 board, two units, one movement-blocking semantic edge, canonicalized
kernel inputs, and stable movement, observation, attack, and commit phases. The
red unit completes one equal-cost diagonal Chebyshev step. The blue unit's
orthogonal step is rejected by the canonical `FS.GG.Game.Core.Edge` between
the cells. Symmetric integer supercover LOS then creates one observation, and
the observed adjacent target receives one fixed 25-point attack.

The movement adapter applies S.I.R.'s strict diagonal rule: all four
origin/side/destination semantic boundaries around a diagonal step must be
passable. Movement candidates read one stable pre-phase state, destination
conflicts are rejected symmetrically, and accepted moves commit as one batch.
The input list is deduplicated and canonically ordered before execution;
reversing the complete M6 journal must reproduce the same state, events,
checkpoints, and digest.

The M6 phase-oracle encoding remains a local conformance diagnostic; M7
supersedes it for replay snapshots and cryptographic identities. It produces
561 canonical simulation bytes across four phase checkpoints, including 55
final-state bytes, 93 event bytes, and the four diagnostic digest bytes
`9baa05cc`. Combined with the M5 numeric vector, the shared .NET and Fable/Node
hosts agree on a 621-byte pre-replay conformance oracle with SHA-256
`6e822e333a46ce1f3e6716841a5a47a69780a428407d426b26ddf98984210d99`.

**Evidence:**

- [S.I.R.#54](https://github.com/EHotwagner/S.I.R./pull/54) introduced the
  shared slice, phase oracles, divergence gate, and roadmap transition;
- `src/SIR.Simulation/Simulation.fs` contains the shared state, inputs, phase
  transitions, semantic-edge adapter, Chebyshev rule, LOS observation, attack,
  event encoding, and state encoding without rendering or network references;
- `tests/SIR.Conformance.Shared/SimulationFixtures.fs` executes the same source
  in both runtimes, holds independent phase oracles, and proves that reversing
  the journal does not change the result;
- `scripts/test-conformance.sh` restores published dependencies into an
  isolated cache, builds .NET, compiles Fable, executes Node, compares the
  complete oracle, and checks the floating-source gate; and
- deliberate mutation of the movement checkpoint is rejected at tick 1,
  phase `movement`, byte zero by both runtime hosts.

### [x] 🟩 M7 — Replay format and runner

**Unblocked by:** completed M6 minimal shared simulation slice.

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

**Outcome:** replay format version 1 is a bounded canonical binary envelope
with `SIRR` magic, explicit format and disclosure discriminators, 32-byte
engine and ruleset identities, and a full-replay authorization gate. Authorized
full payloads contain a complete board and unit snapshot, separately ordered
external inputs and accepted player-WASM outputs, retained state checkpoints,
per-tick state and event SHA-256 hashes, and a terminal result. Perspective
payloads contain only tick-indexed projection hashes; their union case has no
kernel snapshot, input journal, checkpoint, or final authoritative result to
expose.

`SIR.Domain.CanonicalHash` implements the selected SHA-256 identity in source
shared by .NET and Fable and passes the published `abc` vector. The replay
decoder applies byte, count, snapshot, ordering, and hash-length bounds before
execution. It rejects malformed, truncated, oversized, incompatible,
wrong-engine, unordered, unauthorized, and divergent packages with typed
errors.

`Replay.runKernelReplay` re-simulates from the initial snapshot and every
retained checkpoint and returns only `BrowserKernelVerified`.
`Replay.verifyAuthoritative` can return `AuthoritativeVerified` only when its
host supplies the exact player-WASM re-execution journal and every accepted
output matches the package by tick, sequence, and input. A perspective package
returns `PerspectiveReady`, and an attempt to require its kernel fails with
`PerspectiveHasNoKernel`.

**Evidence:**

- [S.I.R.#55](https://github.com/EHotwagner/S.I.R./pull/55) introduced replay
  format v1, the shared runners and SHA-256 implementation, safety fixtures,
  and this roadmap transition;
- `src/SIR.Simulation/Replay.fs` owns the versioned schema, canonical
  encoding/decoding, resource limits, disclosure boundary, checkpoint runner,
  and the distinct browser and authoritative verification claims;
- `src/SIR.Domain/CanonicalHash.fs` provides one portable SHA-256
  implementation for both runtimes;
- `tests/SIR.Conformance.Shared/ReplayFixtures.fs` round-trips the canonical
  package, seeks from retained ticks 0 and 1 to final tick 2, exercises both
  verification levels and perspective playback, and rejects every required
  safety case;
- the canonical full package is 639 bytes with SHA-256
  `7a883614ae2ef19457fa13f32939dbe6e28504e9339b2373cf726010d6516baf`;
  its final state hash is
  `843663a69c87b4e8792b1cd2800df83b70bf9a03eb4cfa766b66d3b995cd0c05`;
  and its perspective-package counterpart is
  `6526545ed82530de5065847beb9c8ca3cf50e0fc80bb68d33146bf5d6ddbf075`;
  and
- `scripts/test-conformance.sh` restores from the published package, builds
  both hosts, runs the same replay fixture in .NET and Fable/Node, and compares
  a combined 717-byte canonical oracle while preserving the earlier numeric
  and phase-divergence gates.

### [x] 🟩 M8 — Elmish replay shell

**Unblocked by:** completed M7 replay format and runner.

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

**Outcome:** `SIR.Client` now owns a platform-neutral replay-shell model,
messages, deterministic update function, typed runner requests and responses,
operation identities, and value-shaped effects. `SIR.Client.Web` maps those
effects into Elmish commands, uses subscriptions for keyboard input and
presentation-only playback cadence, and mounts the React view into the
standalone development page. Browser APIs, React, replay decoding, and the
in-process M8 runner adapter remain outside the pure shell.

Loading a package supersedes and cancels any active operation. Every runner
response carries its operation identity, and the update function ignores stale
responses without changing model or effects. Play, pause, one-tick step,
bounded seek, speed, unit/event/formula selection, and cancellation are
represented as messages rather than view callbacks into the kernel. The M8
runner validates the bounded replay envelope and the retained M7 fixture engine
through the shared replay runner. A package requiring another engine becomes
explicitly unsupported; versioned engine-manifest selection remains M12 work.

Verified browser-kernel replay, perspective-only playback, sandbox fork,
unsupported engine, divergence, and failure each have distinct text and
redundant color treatment. The first permitted parameter edit creates a stable
derived identity and removes the verification claim; reloading the source is
the only route back to verified mode. Primary controls have accessible names,
the current mode and operation announcement use live regions, keyboard
operation covers play/pause, step, and cancel, focus is visible, and reduced
motion is respected.

**Evidence:**

- [S.I.R.#56](https://github.com/EHotwagner/S.I.R./pull/56) introduced the
  shared shell, standalone browser host, locked build, accessibility smoke gate,
  and this roadmap transition;
- `src/SIR.Client/Shell.fs` contains the platform-neutral model, transition
  function, operation protocol, mode state, selection state, sandbox identity,
  and effect values;
- `src/SIR.Client.Web/App.fs` supplies Elmish commands and subscriptions, the
  standalone React mount, file loading, controls, mode disclosures, live
  announcements, and keyboard interaction;
- `src/SIR.Client.Web/Runner.fs` adapts the M7 decoder and browser-kernel runner
  to the typed M8 boundary without making an authoritative-WASM claim;
- `tests/SIR.Client.Tests/Program.fs` proves deterministic state and effects,
  distinct verified/perspective/unsupported/divergent states, irreversible
  sandbox transition, supersession, stale-response rejection, and cancellation;
- `scripts/build-client.sh` compiles the web host with Fable 5.13.0 and creates
  a production Vite 8.1.5 bundle from locked NuGet and npm dependencies; and
- `scripts/smoke-client.mjs` executes the production React bundle in a browser
  DOM, verifies the mount, initial live verification status, accessible file
  input, and six labelled primary controls. The complete conformance script
  retains the 717-byte .NET/Fable replay oracle and adds the shell tests,
  production bundle, and browser-mount gates.

### [x] 🟩 M9 — Worker execution and responsive inspection

**Unblocked by:** completed M8 Elmish replay shell.

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

**Outcome:** the in-process M8 adapter has been replaced by a production Web
Worker emitted as its own Vite asset. Requests and responses use protocol
version 1 envelopes with operation identities. Advance work is planned in
256-tick batches and yields between batches so cancellation and newer
operations can be received. Progress responses keep the operation active;
completion, cancellation, failure, or supersession closes it. The Elmish
update continues to reject every response whose operation is no longer active.

The worker retains the decoded replay and reconstructable simulation state.
Elmish receives only the currently requested presentation projection: board
bounds, compact unit summaries, bounded accepted-input/event summaries,
checkpoint digest prefixes, or a perspective-frame digest. At normal
24,000-tick match length the accepted batch plan has 94 projection boundaries,
not 24,000 per-tick renders. Cooperative yielding is measured with a queued
heartbeat, and measured durations are reported only by the tooling; they do
not enter replay packages, kernel inputs, state, events, or canonical hashes.

Board, timeline/event, formula, checkpoint, perspective, and existing
first-divergence inspection are exposed with accessible labels and textual
status. A worker error or protocol mismatch transitions the shell to `Failed`,
stops playback, clears the active operation, and explicitly revokes any
browser-verification display.

**Evidence:**

- [S.I.R.#57](https://github.com/EHotwagner/S.I.R./pull/57) introduced the
  versioned worker, bounded progress protocol, compact inspectors,
  responsiveness measurement, and this roadmap transition;
- `src/SIR.Client.Web/Worker.fs` owns replay decoding, verification, retained
  worker state, cooperative batch execution, cancellation, seeking, and compact
  projections;
- `src/SIR.Client/Shell.fs` defines protocol version 1, the 256-tick batch
  planner, projection types, streaming progress, worker lifecycle, and
  verification-revocation transitions;
- `src/SIR.Client.Web/App.fs` and `styles.css` provide worker supervision plus
  board, timeline/event, formula, checkpoint, perspective, and worker-status
  views without placing canonical state in React;
- `tests/SIR.Client.Tests/Program.fs` proves the 94-boundary normal-match plan,
  streaming operation retention, compact projection replacement, stale
  response rejection, cancellation, and worker-failure revocation; and
- `scripts/measure-worker.mjs` requires a separately emitted production worker
  asset, runs the 24,000-tick cooperative schedule, proves queued input can run,
  and reports non-canonical timing observations. The browser smoke gate checks
  the inspector and protocol disclosures in the production bundle.

### [x] 🟩 M10 — Interactive rules laboratory

**Unblocked by:** completed M9 worker execution and responsive inspection.

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

**Outcome:** `SIR.Client` now owns a platform-neutral rules-laboratory model
with six fixed, revisioned attack-resolution scenarios, exact engine and ruleset
identities, typed parameter definitions, bounded validation, deterministic
integer evaluation through `Simulation.runTickWithRules`, baseline/fork
comparison, and sweep results. The laboratory varies bounded rule inputs but
does not reimplement attack resolution. The baseline is always recomputed from
catalog defaults and is never mutated by a patch.
Every result carries its complete effective parameter map plus scenario,
revision, engine, ruleset, and content-derived result identities.

Scenario selection and experiment execution cross the existing versioned
worker boundary. Editing a parameter creates a derived sandbox identity and
cannot restore a verification claim. Sweep values follow the parameter's
declared integer minimum, maximum, and step; tables and accessible meter charts
are presentation projections of those integer results. Reports are always
labeled `Exploratory balance evidence — not accepted balance`.

The versioned `sir-lab-experiment-v1` text export records the immutable
baseline, derived fork, exact compatibility identities, effective inputs,
integer metrics, and optional sweep identities. The checked-in
`attack-power-30.sir-lab` export demonstrates that a useful browser experiment
can be promoted directly into a permanent regression fixture.

**Evidence:**

- [S.I.R.#58](https://github.com/EHotwagner/S.I.R./pull/58) introduced the
  shared-kernel laboratory, worker operations, comparison UI, promoted
  fixture, conformance gates, and this roadmap transition;
- `src/SIR.Client/Lab.fs` owns the fixed catalog, typed validation,
  shared-kernel experiment runner, comparison and sweep model, result
  identities, and reproducible export format;
- `src/SIR.Simulation/Simulation.fs` exposes bounded
  `SimulationRules`/`runTickWithRules` while retaining `runTick` with the
  canonical default rules for replay compatibility;
- `src/SIR.Client/Shell.fs` and `src/SIR.Client.Web/Worker.fs` keep scenario
  loading, sandbox transitions, experiments, and sweeps behind the typed
  operation protocol with stale-response and cancellation protection;
- `src/SIR.Client.Web/App.fs` provides the catalog, parameter editor,
  baseline/fork/delta tables, integer sweep chart, compatibility disclosures,
  evidence label, and experiment download;
- `tests/SIR.Client.Tests/fixtures/attack-power-30.sir-lab` is a promoted
  permanent fixture whose complete export is checked by the client tests;
- `tests/SIR.Client.Tests/Program.fs` proves baseline immutability, complete
  inputs and identities, range rejection, deterministic repeated execution,
  the 100-value integer sweep, fixture stability, and the worker-driven
  scenario/fork state transitions; and
- `scripts/test-conformance.sh` retains the 717-byte .NET/Fable replay oracle,
  builds the laboratory source with Fable, creates the separately emitted
  production worker, and runs the browser smoke gate for the scenario catalog
  and comparison surface.

### [x] 🟩 M11 — Literate corpus integration

**Unblocked by:** completed M8 mount and completed M10 rules laboratory.

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

**Outcome:** `fsdocs-tool` 22.1.0 is pinned in the repository-local tool
manifest. A strict, locked build now compiles the solution in Release mode,
evaluates fixed literate F# examples on .NET, generates the complete
explanatory corpus and public API reference, builds the Fable client, combines
the outputs under `artifacts/site`, and rejects broken Pages-root links,
missing source links, absent search entries, incomplete runtime disclosures,
or asset-integrity drift.

The default FSharp.Formatting template remains intact. Supported `_head.html`,
`_body.html`, and `fsdocs-theme.css` extension points add the scoped client
styles, mount the Fable application only when its dedicated element exists,
and reuse the established verified, browser, sandbox, and warning color
vocabulary. The interactive page identifies Fable/JavaScript browser
execution and its verification boundary; the evaluated tutorial identifies
.NET build-time execution. A visible `<noscript>` explanation preserves routes
to the corpus, fixed evidence, architecture, and API pages.

Vite emits the application, worker, and scoped stylesheet beneath
`content/sir-client/v1`. The combined build creates
`sir-docs-assets-v1`, records byte lengths and SHA-384 integrity for every
browser asset, and verifies the manifest before acceptance. Generated HTML,
Markdown, search indexes, API pages, client bundles, and manifests remain
ignored build outputs.

**Evidence:**

- [S.I.R.#59](https://github.com/EHotwagner/S.I.R./pull/59) introduced the
  strict literate corpus, evaluated .NET evidence, embedded Fable application,
  integrity gates, generated-page browser smoke, and this roadmap transition;
- `docs/index.md`, `docs/deterministic-simulation.fsx`, and
  `docs/interactive-rules-lab.md` provide navigation, evaluated fixed
  evidence, the Fable mount, runtime/trust disclosures, and the no-JavaScript
  path;
- `Directory.Build.props` supplies the Pages project root, repository/source
  identities, navigation metadata, AGPL link, and default-template styling
  parameters for all documented libraries;
- `scripts/build-docs.sh` performs the locked Release build, strict evaluated
  fsdocs generation, Fable/Vite build, and uncommitted artifact assembly;
- `scripts/generate-docs-manifest.mjs` and `scripts/verify-docs.mjs` prove
  versioned SHA-384 asset integrity, project-root links, main-branch source
  links, API output, search membership, evaluated results, runtime labels, and
  fallback content;
- `scripts/smoke-docs.mjs` loads the generated fsdocs page and proves that the
  production Fable application mounts there with live verification status and
  the fixed scenario catalog; and
- the pull-request workflow runs this complete documentation gate separately
  from the retained cross-runtime conformance gate.

### [x] 🟩 M12 — Versioned engine publication and GitHub Pages

**Unblocked by:** completed M7 replay format and completed M11 literate corpus
integration.

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

**Outcome:** Replay loading now decodes the bounded package header before
worker activation and selects only an exact engine identity and replay-format
pair from the retained catalog. The current format-v1 worker is emitted below
its complete 32-byte engine identity instead of the mutable application path;
an unlisted engine produces an explicit unsupported-replay result without
silently invoking the current worker.

The combined fsdocs/Vite build copies the retained engine tree into the static
site and creates `sir-pages-publication-v1`. That manifest records the static
hosting contract, application assets, supported replay formats, retention
policy, immutable worker paths, byte lengths, and SHA-384 integrity. The build
fails when the source catalog, compiled selector, Vite path, manifest, or
artifact differs; when a supported replay format has no retained engine; or
when the public output contains source maps, replay packages, WASM modules,
symbols, package lockfiles, or environment files.

The dedicated Pages workflow performs the same pinned .NET, Node, Fable, Vite,
and FSharp.Formatting build, uploads only `artifacts/site`, and deploys it
through the `github-pages` environment. Browser smoke and accessibility gates
prove the generated application mount, live verification announcement,
control labels, named regions, fallback, and static operation without a
privileged service.

**Evidence:**

- [S.I.R.#60](https://github.com/EHotwagner/S.I.R./pull/60) introduced exact
  retained-engine selection, immutable publication paths, the combined
  publication manifest, security/retention/accessibility gates, the Pages
  deployment workflow, and this roadmap transition;
- `src/SIR.Client/EngineCatalog.fs`, `src/SIR.Client.Web/Runner.fs`, and
  `src/SIR.Client.Web/Worker.fs` select the engine before worker activation,
  preserve the format-v1 worker contract, and explicitly reject unavailable
  identities;
- `config/engine-publication.json`,
  `scripts/generate-publication-manifest.mjs`, and
  `scripts/verify-docs.mjs` keep the runtime catalog, output paths, replay
  policy, artifact existence, integrity, and public-artifact exclusions under
  one failing build gate;
- `tests/SIR.Client.Tests/Program.fs` round-trips the retained format-v1
  package fixture, proves its exact bundle selection, and proves a missing
  engine cannot fall forward;
- `scripts/smoke-docs.mjs` and `scripts/test-docs-accessibility.mjs` exercise
  the generated Pages document and mounted production application; and
- `.github/workflows/pages.yml` builds the locked combined artifact and
  deploys it through GitHub's static Pages artifact contract.

### [x] 🟩 M13 — Full match replay qualification

**Unblocked by:** completed M12 publication and the bounded authoritative
match/WASM host delivered in this milestone.

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

**Outcome:** `SIR.Match` now owns a completed four-tick authoritative
qualification match. It validates the SHA-256 identity of an immutable binary
WebAssembly artifact, compiles it once with Wasmtime 44.0.0 under the pinned
core-only, no-WASI, fuel-metered profile, resets the fuel allowance for every
invocation, and records four accepted attack outputs in stable tick/sequence
order. The shared .NET kernel produces retained checkpoints and terminal state
and event hashes from those outputs.

The host emits a 933-byte authorized full package and a 258-byte
knowledge-filtered perspective package. The full package passes the same shared
kernel runner used by the Fable worker. Authoritative verification then starts
a fresh Wasmtime instance for the exact artifact and profile and compares every
re-executed output with the recorded journal before returning
`AuthoritativeVerified`; a Boolean assertion is no longer sufficient.

The perspective package contains only five tick-indexed projection hashes. Its
type cannot expose a kernel snapshot or input journal, and the qualification
gate additionally proves its canonical bytes contain neither the hidden final
state hash nor the player artifact. The browser UI labels its own result as
browser-kernel verification and states that exact-artifact authoritative
verification is a separate .NET operation.

**Evidence:**

- [S.I.R.#61](https://github.com/EHotwagner/S.I.R./pull/61) introduced the
  bounded authoritative match host, exact-artifact re-execution, full and
  perspective qualification packages, verification/disclosure gates, and this
  roadmap transition;
- `src/SIR.Match/MatchReplay.fs` owns the pinned execution profile, immutable
  binary artifact, deterministic invocation, accepted-output journal, complete
  match run, exact-artifact re-execution, and both disclosure packages;
- `src/SIR.Simulation/Replay.fs` compares the complete re-executed WASM journal
  and reports the first changed tick and sequence before it can grant
  `AuthoritativeVerified`;
- `tests/SIR.Match.Tests/Program.fs` proves browser/authoritative terminal hash
  equality, rejects a changed WASM output at tick/sequence 2, localizes an
  intentionally corrupted checkpoint to tick 2, and audits the perspective
  package for hidden state and artifact bytes;
- `src/SIR.Client.Web/App.fs` and the browser smoke gate preserve the visible
  distinction between browser-kernel and authoritative verification; and
- `scripts/test-conformance.sh` runs the new match gate alongside the existing
  .NET/Fable canonical replay, first-divergence, browser, and worker gates.

### [x] 🟩 M14 — Optional browser WASM verification research

**Unblocked by:** completed M13. This is not required for the canonical browser
replay.

**Question:** can a browser WASM host reproduce the pinned Wasmtime ABI,
instance state, fuel, traps, scheduling, and host services closely enough for
full authoritative verification?

**Exit:** accept a separate implementation milestone only if the spike passes
the complete module-execution conformance contract. Otherwise retain recorded
accepted outputs as the canonical browser boundary.

**Outcome:** the spike rejected a separate browser-WASM verification
implementation milestone. One shared 125-byte core module produced identical
SHA-256 identity, integer decisions, persistent and fresh-instance state,
ordered host calls, and explicit guest-trap behavior under Wasmtime 44.0.0 and
the JavaScript `WebAssembly` API. The shared vector is decisions `[8, 10, 2]`,
host-call arguments `[4, 5, 1]`, final counter `3`, and fresh-instance counter
`0`.

The complete profile does not qualify. Wasmtime deterministically traps the
module's unbounded loop at a 1,000-unit fuel allowance. Native browser
WebAssembly exposes no store fuel allowance, consumed-fuel counter, or
out-of-fuel trap. Wall-clock Web Worker termination is device- and
scheduler-dependent and cannot reproduce the authoritative instruction
boundary. Browser replay therefore continues to consume recorded accepted
outputs and may claim only browser-kernel verification.

**Evidence:**

- `spikes/browser-wasm-verification/` holds the exact shared artifact,
  Wasmtime oracle, JavaScript WebAssembly host, and contract evaluator;
- `scripts/test-browser-wasm-verification.sh` proves all common vectors match,
  proves Wasmtime fuel enforcement, and deliberately fails if the browser API
  later exposes a fuel surface without renewed qualification;
- [Browser WASM Verification Spike](research/browser-wasm-verification-spike.md)
  records the passed and failed contract surfaces and the negative
  implementation decision; and
- `scripts/test-conformance.sh` runs the M14 decision gate with the canonical
  replay and match qualification suite.

### [x] 🟩 M15 — V1 compatibility baseline and maintenance handoff

**Unblocked by:** completed M14 research and the accepted M7–M13 runtime,
publication, and verification contracts.

**Purpose:** close the feature roadmap without leaving implemented v1 choices
misclassified as open design questions, and make later compatibility changes
deliberate.

**Acceptance:**

- every implemented replay, numeric, worker, publication, toolchain, export,
  smoke, and accessibility parameter has one machine-readable baseline;
- the baseline is checked against executable source and locked configuration;
- incompatible changes fail before the conformance and documentation builds;
- production-scale policy that cannot yet be selected is named and owned by a
  future profile rather than implied to be part of v1; and
- the architecture document distinguishes compatibility contracts from tuning
  policy.

**Outcome:** `sir-fable-client-baseline-v1` records replay format 1, canonical
uncompressed little-endian binary encoding, flat SHA-256 state/event hashes,
decoder limits, four-place fixed point, worker protocol 1, 256-tick batches,
the experiment-export schema, SHA-384 publication integrity, engine-retention
policy, browser test harness, and the complete locked .NET/Fable/Elmish/React/
Vite/fsdocs toolchain. The baseline does not create a second runtime
configuration source: a verification script compares its declarations with
the existing F#, JSON, XML, and npm owners and fails on drift.

Three choices remain intentionally outside v1 because the current bounded
qualification fixtures cannot select them honestly: production checkpoint
cadence, time-based public replay/engine retention, and subsystem-specific
numeric scales and ranges. Each now requires a future compatibility or
production profile. Changing a frozen v1 encoding, hash, protocol, worker
budget, export schema, or retained-engine contract requires a new versioned
identity and migration evidence.

**Evidence:**

- [S.I.R.#63](https://github.com/EHotwagner/S.I.R./pull/63) introduced the
  compatibility inventory, executable drift gate, future-profile handoff, and
  this final roadmap transition;
- `config/fable-client-baseline.json` is the machine-readable inventory of
  frozen v1 contracts and explicitly deferred production profiles;
- `scripts/verify-fable-client-baseline.mjs` checks replay and numeric
  constants, worker protocol and batching, export schema, retention policy,
  browser harness, and every locked toolchain version against their executable
  owners;
- `scripts/test-conformance.sh` and `scripts/build-docs.sh` run the drift gate
  before their existing cross-runtime and publication work; and
- the v1 implementation-baseline table below replaces the stale undifferentiated
  list of open parameters.

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

## V1 implementation baseline

The compatibility inventory is
`config/fable-client-baseline.json`. The values below are frozen for v1 and
verified against their executable owners by
`scripts/verify-fable-client-baseline.mjs`.

| Surface | V1 contract | Change rule |
|---|---|---|
| Replay | Format 1; canonical uncompressed little-endian binary | New replay-format identity and migration evidence |
| Digests | Flat SHA-256 state and event hashes; no hash tree | New replay/engine compatibility identity |
| Replay limits | 1 MiB package; 16,384 inputs; 16,384 WASM outputs; 4,096 checkpoints; 65,536 perspective frames; 4,096 units; 16,384 edges; 65,536 observations | New validated resource profile |
| Numerics | Signed int32 storage, saturating overflow, four-place base-ten fixed point, nearest ties away from zero | New ruleset/engine identity when authoritative output can change |
| Browser execution | Worker protocol 1; one dedicated versioned worker for the active engine; 256-tick batches; one compact projection per completed batch | New protocol/engine identity and responsiveness evidence |
| Laboratory export | `sir-lab-experiment-v1` with exact engine, ruleset, scenario, inputs, and integer results | New export schema and fixture migration |
| Publication | SHA-384 asset integrity; retain each engine while its replay format is supported | New publication schema and retention audit |
| Upstream package | `FS.GG.Game.Core` 0.13.0, profile `fs-gg-game-core-fable-lockstep-v1`, source delivered through the existing package | New published profile and consumer conformance |
| Toolchain | .NET SDK 10.0.302; Fable 5.13.0; FSharp.Core 10.1.302; Elmish 5.0.2; Elmish.React 5.6.0; React 19.2.8; Vite 8.1.5; Node 26.5.0; fsdocs 22.1.0 | Coherent lock update plus complete conformance and documentation gates |
| Browser quality gates | Production-bundle mount, generated-site mount, and structural accessibility checks under happy-dom | Replacement requires equivalent or stronger checked evidence |

## Future production profiles

The following are not silently open v1 parameters. They belong to later work
whose scale and retention inputs do not yet exist:

- production checkpoint cadence and retained diagnostic detail;
- time-based public replay and engine-bundle retention duration; and
- subsystem-specific fixed-point scales and bounded numeric ranges.

Those profiles may tune future content without weakening the shared-source,
exact-equivalence, upstream-publication, numeric, Elmish-MVU, version-binding,
disclosure, or verification-level decisions above. Any output-affecting change
must also advance the appropriate ruleset, engine, replay, protocol, or export
identity.

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
