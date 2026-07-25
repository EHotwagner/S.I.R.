---
title: S.I.R. .NET WebAssembly Runtime Selection
status: accepted
decision-status: canonical
document-type: research-and-options
version: "0.2"
last-updated: 2026-07-25
related:
  - docs/wasm-control-architecture.md
  - docs/technology-stack.md
  - docs/codebase-architecture.md
  - docs/simulation-core-architecture.md
---

# S.I.R. .NET WebAssembly Runtime Selection

## Decision status

Direct Wasmtime embedding through the official `Wasmtime` NuGet package is the
canonical runtime family. The exact version remains unpinned until a small
validation spike proves that binding satisfies the required limits, state
handling, and concurrency behavior.

This assessment reflects the runtime ecosystem on 2026-07-25. Runtime and
package versions must be rechecked when the implementation pin is created.

## Selection requirements

The S.I.R. control host needs:

- maintained F#/.NET-compatible embedding;
- safe execution of untrusted player code;
- deterministic instruction metering;
- explicit host functions without ambient authority;
- strict linear-memory, stack, input, output, and host-call limits;
- reusable compiled artifacts with isolated per-unit state;
- efficient invocation of approximately 100–200 instances per match;
- trap and exhaustion containment;
- Linux and Windows development support;
- observable profiling and consumed-budget data;
- a license compatible with the project's AGPL distribution; and
- a narrow adapter that prevents runtime-specific types from entering the game
  domain.

Ease of running arbitrary WASI applications is not a selection advantage.
S.I.R. needs a deliberately small game ABI rather than an operating-system
compatibility layer.

## Options

| Option | Strengths | Concerns | Assessment |
|---|---|---|---|
| Direct Wasmtime .NET embedding | Official Bytecode Alliance .NET package; deterministic fuel; host functions can consume fuel; reusable serialized compiled modules; configurable stack and feature set; mature untrusted-code security focus | Native dependency and large package; rapid major-version cadence; .NET surface may lag the Rust API; complete resource limiting and checkpoint behavior require a spike | **Selected** |
| Extism .NET SDK | Explicit untrusted plug-in model; F# support; convenient host functions, persistent variables, limiters, and timers | Adds its own PDK, memory, and plug-in conventions; hides runtime details S.I.R. needs to make canonical; timer-oriented limits do not replace deterministic fuel; unnecessary HTTP/configuration features | Useful inspiration or tooling, not the authoritative host |
| Wasmer through WasmerSharp | Embeddable runtime; core runtime advertises metering, caching, and several compiler backends | Current high-level documentation emphasizes the broader Wasmer/WASIX ecosystem; the .NET path is less central and would require more interop and metering validation | Viable fallback, weaker initial fit |
| WebAssembly Micro Runtime through native interop | Small footprint; interpreter and AOT modes; suitable for embedded environments | No first-class S.I.R.-ready .NET API; custom P/Invoke and lifecycle ownership; greater maintenance and security-update burden | Reject for the initial server |
| Higher isolation through one OS process per unit | Strong process boundary | Hundreds of processes, expensive communication and lifecycle, and no replacement for deterministic instruction accounting | Reject as the normal model |

Process or container isolation can still provide defense in depth around a
match worker. It complements rather than replaces the in-process WASM sandbox.

## Why direct Wasmtime fits

Wasmtime's fuel mechanism is deterministic for the same module and execution
configuration. The .NET API can:

- enable fuel consumption;
- add a per-invocation allowance;
- report consumed fuel;
- synthetically charge fuel from host functions;
- configure maximum WASM stack size;
- validate and compile modules; and
- serialize compiled modules for compatible reuse.

This maps closely to S.I.R.'s existing canonical rules:

- every instance has an independent public budget;
- unused fuel does not roll forward;
- host calls cannot hide unlimited server work;
- one artifact can be compiled once and instantiated many times; and
- one unit's state cannot become another unit's state merely because both use
  the same artifact.

Epoch interruption is intentionally not the gameplay budget. Its interrupt
point depends on external scheduling. It may exist as a wall-clock emergency
backstop, but triggering it indicates an operational failure or invalid match
execution rather than a normal deterministic module fault.

## Recommended restricted runtime profile

The initial execution profile should use:

- core WebAssembly modules;
- the official Wasmtime .NET embedding;
- Cranelift's normal optimizing compilation;
- fuel consumption enabled;
- NaN canonicalization enabled if floating-point instructions are permitted;
- one declared bounded linear memory per module;
- no WASI;
- no filesystem, sockets, environment, clocks, or host entropy;
- no threads, atomics, or shared memory;
- no memory64;
- no multiple memories;
- no component model for the first ABI;
- no guest-controlled dynamic module loading; and
- an explicit allowlist of S.I.R. imports and required exports.

SIMD, reference types, tail calls, exceptions, garbage-collected references, and
other optional proposals should begin disabled unless required by a supported
guest toolchain. Enabling a proposal changes the versioned execution profile
and requires determinism, resource, and compatibility tests.

Floating point may be useful inside player decision logic. If allowed, it
cannot enter authoritative game values directly: the ABI carries bounded
integer, fixed-point, identifier, enum, and byte-buffer representations.
Modules with identical inputs must still produce identical output under the
pinned profile.

## Host object model

The recommended ownership model is:

```text
one Engine per compatible execution profile and match-worker process
  └── one compiled Module per unique accepted artifact
        ├── unit A Store + Instance + private memory/state
        ├── unit B Store + Instance + private memory/state
        └── unit N Store + Instance + private memory/state
```

A Wasmtime store can move between threads but must not be used concurrently.
S.I.R. can execute different unit stores in parallel, return keyed outputs, and
merge those outputs deterministically. It never invokes the same instance from
two threads at once.

Compiled artifacts are cache entries, not authoritative state. Cache keys must
include:

- original module content hash;
- Wasmtime package and native-runtime version;
- target architecture;
- compilation configuration;
- enabled WebAssembly features; and
- S.I.R. execution-profile version.

The original validated module remains the portable artifact of record.
Serialized native code is an optional rebuildable optimization and must never
be accepted from an untrusted player.

## Fuel lifecycle

For every invocation:

1. Ensure the instance starts with zero carry-over fuel.
2. Add the host-class allowance from the pinned execution profile.
3. Charge declared fixed costs before executing each synchronous host function.
4. Run exactly one ABI entry point.
5. Accept output only after successful completion and bounded validation.
6. Remove any unused fuel so it cannot accumulate.
7. Record consumed fuel as diagnostic and replay-audit data.

If the pinned .NET API cannot directly reset fuel, the adapter can
synthetically consume the known remainder before adding the next allowance.
This behavior requires a focused test across normal return, guest trap,
host-function trap, and out-of-fuel cases.

Expensive pathfinding, influence, formation, and sensor queries are not
performed as unbounded synchronous host calls. The module emits bounded service
requests whose deterministic results arrive in a later invocation under a
separate quota.

## Memory and resource enforcement

The host should reject a module unless:

- it declares exactly the allowed memory topology;
- every memory has a maximum;
- the declared maximum does not exceed the host-class profile;
- initial memory, tables, globals, imports, exports, functions, and code size
  fit independent validation limits;
- imported functions exactly match the ABI allowlist; and
- input and output buffers obey fixed bounds.

WebAssembly's declared memory maximum prevents `memory.grow` beyond that
maximum. The validation spike must confirm whether the selected .NET package
also exposes an adequate store-level resource limiter. If it does not, module
validation, stack limits, host-side allocation quotas, and match-worker
process/container limits form the initial layered enforcement.

Runtime limits do not account for every allocation made by the F# host.
Decoded requests, logs, service queues, compiled-code caches, and operational
metadata require independent bounded structures.

## Determinism and versioning

The match execution profile records:

- Wasmtime package and native-runtime version;
- target architecture policy;
- engine configuration and feature flags;
- ABI version;
- module-validation profile;
- fuel allowance by host class;
- synchronous host-call fuel schedule;
- memory and structural limits;
- trap classification rules; and
- module-state checkpoint format.

Fuel is deterministic within a pinned configuration, but S.I.R. must not assume
that fuel accounting, compilation, trap text, or serialized native artifacts
are stable across runtime upgrades. An upgrade creates a new execution-profile
version and runs the full module conformance and replay suite.

Trap strings are diagnostic only. The adapter maps runtime failures into stable
S.I.R. cases such as:

- `FuelExhausted`;
- `GuestTrap`;
- `ForbiddenImport`;
- `MemoryLimit`;
- `InvalidOutput`;
- `HostFault`; and
- `InfrastructureAbort`.

Only stable S.I.R. cases can affect gameplay or appear in canonical replay
events.

## Module state and checkpointing

Wasmtime module serialization serializes compiled code, not the live state of a
unit instance. S.I.R. therefore needs an explicit checkpoint design for:

- linear memory;
- mutable globals or equivalent compiler runtime state;
- pending host-service handles;
- wake schedule;
- fault state; and
- S.I.R.-owned invocation metadata.

Possible approaches are:

1. constrain modules so all persistent guest state is externally
   snapshot-readable;
2. define bounded `save_state` and `load_state` ABI operations;
3. restart from an earlier journal position and replay module calls; or
4. combine periodic guest checkpoints with journal replay.

The recommended direction is a combination of bounded explicit checkpoints and
journal replay, but this must be prototyped with the intended guest languages.
It is the largest unresolved issue in the runtime selection.

## Required validation spike

Before pinning the Wasmtime version for production, implement a disposable F#
spike that proves:

1. a module is compiled once and instantiated into at least 200 isolated
   stores;
2. instances retain private state with no cross-instance communication;
3. deterministic fuel exhausts at the same instruction for repeated runs;
4. unused fuel cannot carry into another invocation;
5. host functions consume declared synthetic fuel;
6. module-declared and host-enforced memory limits behave as expected;
7. stack overflow, unreachable, division, invalid memory access, host trap, and
   out-of-fuel map into stable S.I.R. fault cases;
8. no ambient WASI capability is available;
9. different stores can execute concurrently and their keyed outputs merge
   deterministically;
10. compiled-artifact reuse materially reduces startup cost;
11. live instance state can be checkpointed and restored or deterministically
    reconstructed; and
12. the 20 Hz scale target retains measured headroom with representative
    standard modules.

The spike reports compilation, instantiation, invocation, fuel, memory,
checkpoint, and parallel throughput separately. It does not assume the earlier
illustrative `0.1 ms` invocation figure.

## Canonical decision

Use direct Wasmtime .NET embedding. Keep all runtime access behind `SIR.Wasm`,
pin the validated version as part of the execution profile, and begin with the
restricted core-module profile above.

Do not adopt Extism as the authoritative layer: its conveniences are valuable
for conventional plug-ins, but S.I.R.'s player-code model needs lower-level
control over deterministic fuel, ABI shape, instance state, and replay
identity.

Do not request an FS.GG upstream change. The runtime is an S.I.R.-specific
application dependency behind an S.I.R. adapter.

## Primary sources

- [Wasmtime project and supported embeddings](https://github.com/bytecodealliance/wasmtime)
- [Wasmtime .NET embedding](https://bytecodealliance.github.io/wasmtime-dotnet/)
- [Wasmtime .NET configuration API](https://bytecodealliance.github.io/wasmtime-dotnet/api/Wasmtime.Config.html)
- [Wasmtime .NET store and fuel API](https://bytecodealliance.github.io/wasmtime-dotnet/api/Wasmtime.Store.html)
- [Wasmtime deterministic fuel and epoch interruption](https://docs.wasmtime.dev/examples-interrupting-wasm.html)
- [Wasmtime .NET module validation and serialization](https://bytecodealliance.github.io/wasmtime-dotnet/api/Wasmtime.Module.html)
- [Wasmtime security model](https://docs.wasmtime.dev/security.html)
- [Current Wasmtime NuGet package](https://www.nuget.org/packages/Wasmtime/)
- [Extism host and PDK model](https://github.com/extism/extism)
- [Wasmer runtime and SDK overview](https://github.com/wasmerio/wasmer)
- [Wasmer runtime metering and caching](https://docs.wasmer.io/runtime/features/)
