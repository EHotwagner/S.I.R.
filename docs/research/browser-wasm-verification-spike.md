---
title: Browser WASM Verification Spike — M14 Result
status: accepted
decision-status: evidence
document-type: research
version: "1.0"
last-updated: 2026-07-29
related:
  - docs/fable-client-and-documentation.md
  - docs/wasm-control-architecture.md
  - docs/research/wasm-runtime-selection.md
---

# Browser WASM Verification Spike — M14 Result

## Question

Can a native browser WebAssembly host reproduce the pinned Wasmtime 44.0.0
execution profile closely enough for the browser to grant the same
authoritative replay-verification claim as the .NET match host?

## Answer

**No.** The common WebAssembly behavior matches, but native browser
WebAssembly has no deterministic fuel contract corresponding to the
authoritative Wasmtime store. The browser cannot set an instruction allowance,
read consumed fuel, or receive the same deterministic out-of-fuel trap from an
unbounded guest.

Terminating a Web Worker after an elapsed-time deadline is not equivalent.
Elapsed time depends on the device, browser, compilation tier, scheduling, and
contention. It cannot reproduce the authoritative instruction boundary.

The canonical boundary therefore remains unchanged: the browser replays the
recorded accepted-output journal through the shared kernel and may claim
browser-kernel verification. Only the .NET host, re-executing the exact module
under the pinned Wasmtime profile, may claim authoritative verification.

## Executable experiment

The spike executes one 125-byte core WebAssembly artifact in Wasmtime and in
the JavaScript `WebAssembly` API used by browsers. The artifact has:

- one integer host import;
- persistent mutable instance state;
- a deterministic `decide` export;
- an explicit trap export;
- an unbounded loop; and
- a state-inspection export used only by the spike.

Both hosts execute ticks `3, 3, -2` in the same order. They must agree on the
artifact SHA-256 identity, outputs `[8, 10, 2]`, host-call arguments
`[4, 5, 1]`, final counter `3`, fresh-instance counter `0`, and explicit
WebAssembly trap classification.

Wasmtime additionally receives a 1,000-unit fuel allowance for the unbounded
loop and must produce an out-of-fuel trap. The browser host feature-detects the
native API surface and must report that no store/fuel contract exists. If a
future browser API provides one, the gate intentionally fails and instructs
maintainers to reopen M14 rather than silently preserving the negative result.

Run the complete comparison from the repository root:

```bash
./scripts/test-browser-wasm-verification.sh
```

## Contract result

| Contract surface | Result | Consequence |
|---|---|---|
| Exact artifact bytes and core integer semantics | PASS | The same bounded module executes in both hosts |
| Persistent instance state and fresh-instance isolation | PASS | Ordinary mutable globals reproduce |
| Deterministic invocation order | PASS | The host can schedule recorded ticks explicitly |
| Integer host service calls | PASS | Inputs and outputs match exactly |
| Explicit guest trap | PASS | Both hosts expose a semantic WebAssembly runtime trap |
| Pinned proposal restrictions | NOT QUALIFIED | Browser engines do not expose Wasmtime's feature configuration |
| Deterministic fuel allowance and consumption | FAIL | No native browser store/fuel API exists |
| Deterministic unbounded-execution trap | FAIL | Only non-deterministic external worker termination is available |
| Complete authoritative execution profile | **FAIL** | Browser authoritative verification is rejected |

## Decision

M14 ends as a negative research result; it does not create a browser-WASM
implementation milestone. A future proposal may reopen the question only with
an executable deterministic instruction-metering contract. Shipping a custom
instrumenter or a full Wasmtime-class runtime into the documentation site would
be a new architecture decision with its own artifact identity, security,
retention, and conformance obligations.
