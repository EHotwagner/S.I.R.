# Spikes

Disposable, information-gathering programs. They answer a specific architectural
question with measurement and are then kept only as evidence for the decision
they informed.

Spikes deliberately live outside the canonical solution layout described in
[codebase-architecture](../docs/codebase-architecture.md). They are not part of
the product, are not held to its dependency rules, and should not be extended
into it. When a spike's answer matters, the answer moves into `docs/` and the
code stays here as provenance.

| Spike | Question | Result |
|---|---|---|
| [wasm-invocation](wasm-invocation/) | Can the server invoke one WASM instance per unit, every tick, at 100 units per side, inside 50 ms? | [Yes, with wide margin](../docs/research/wasm-invocation-spike.md) |
| [perception](perception/) | What does edge-aware, multi-level perception cost at 100 units per side? | [About 1% of the tick](../docs/research/perception-spike.md) |

## Running

```sh
cd <spike>
dotnet run -c Release            # full suite
dotnet run -c Release -- --quick # shorter sampling, skips the long scenarios
```

Requires the .NET 10 SDK. The WebAssembly test modules are WAT source compiled
by Wasmtime at startup, so no external WebAssembly toolchain is needed.
