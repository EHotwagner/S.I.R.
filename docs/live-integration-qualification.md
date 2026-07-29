---
title: Live Integration Qualification
category: Engineering
categoryindex: 6
index: 14
status: accepted
decision-status: implemented
document-type: evidence
version: "1.0"
last-updated: 2026-07-29
related:
  - docs/wasm-control-architecture.md
  - docs/simulator-worker-protocol.md
  - docs/public-protocol-architecture.md
---

# Live Integration Qualification

## Proven vertical slice

`SIR.Match.LiveIntegration` qualifies one continuous, authoritative path:

```text
canonical SIR-PLAN 1 editor handoff
  → SirPlan validation and compilation
  → embedded standard WASM controller
  → reusable native ControlHost
  → shared MapScale and capability kernels
  → knowledge-scoped projection journal
  → simulator worker authoritative-run transport
  → deterministic replay re-execution
```

The representative plan coordinates two rifle units over 40 consecutive
20 Hz ticks. It includes movement, body facing, attention, a preloaded-clock
synchronization marker, point engagement, and hold. Every tick invokes both
isolated standard-controller instances, validates the complete Control ABI
output before applying it, advances the shared kernels once, and commits one
monotonic projection revision. There is no turn-resolution or WEGO phase.

The native path compiles the canonical bytes received at handoff; the browser
does not interpret plan commands or run a second planner. The native host
produces the runtime-neutral `AuthoritativeProjectionFrame` type in
`SIR.Domain`, and the Fable worker consumes that same type through
`LoadAuthoritativeRun`. Subsequent step/run-to operations select those exact
projections. The older empty-delta path remains only for unqualified
intent-only workspace rehearsal and carries no authoritative claim.

## Pinned evidence

The accepted planning artifact and replay diagnostics carry:

- map revision;
- plan semantic and source identities;
- ruleset identity;
- descriptor set identity and version;
- standard-controller artifact identity;
- engine identity;
- authoritative replay identity;
- match-lock identity; and
- logical session identity at admission.

The match lock hashes the first seven immutable inputs. Admission rejects any
artifact that differs from the accepted plan, including a forged lock.
Reconnect requires the same match lock and equal bounded server/projection
cursors. A short retained gap resumes deltas; a longer valid gap replaces them
with the latest knowledge-filtered snapshot. Disconnect never alters or pauses
continuous kernel ticks.

## Disclosure and replay evidence

The qualification projection includes only the two side-1 units. The opposing
target, controller bytes, capability state, complete map state, and identities
derived from hidden changes never enter the browser payload. Per-frame browser
state hashes cover disclosed units only; because this slice discloses no event
bodies, its browser event hash is constant. Complete state and event identities
remain in the server-side authoritative journal. Tests reject inconsistent
reconnect cursors and verify that neither the opposing unit, the controller
artifact, nor a hidden-state-derived checkpoint identity appears in projection
transport.

Authoritative verification reruns the same compiler, controller host, kernels,
and projection builder, then separately compares the server-only authoritative
journal, the knowledge-scoped projection journal, final authoritative state,
and pinned identities. Browser playback remains projection-only; it does not
claim to re-execute native WASM.

## Measured budget

The focused 2026-07-29 qualification measured:

| Boundary | Measurement | Gate |
|---|---:|---:|
| 40 full ticks / 80 WASM invocations | 30.197 ms | 5,000 ms |
| 20-frame preview mapping | 0.254 ms | 100 ms |
| 40-frame serialization | 0.151 ms | 100 ms |
| structured worker-transfer copy | 0.025 ms | 100 ms |
| rendering projection mapping | 0.092 ms | 100 ms |
| bounded projection payload | 4,800 bytes | bounded |

Wall-clock measurements are evidence, not deterministic state. The automated
gate uses deliberately generous ceilings to avoid treating host timing as game
authority. The production worker smoke separately exercises the actual built
worker and structured-clone boundary.

## Accepted boundary and remaining proposals

Accepted by direct evidence: continuous per-tick standard-controller
execution, native host isolation, shared-kernel application, identity pinning,
knowledge-scoped projection transport, match-lock admission, bounded reconnect,
and replay re-execution for this representative slice.

Still proposed: distributed token persistence, multi-process resume buffers,
real network authentication/takeover, every ruleset capability, and production
telemetry at fleet scale.
