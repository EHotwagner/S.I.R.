---
title: Simulator Worker Protocol
category: Engineering
categoryindex: 6
index: 13
status: accepted
decision-status: implemented
document-type: living-architecture
version: "1.0"
last-updated: 2026-07-29
related:
  - docs/2026-07-29-1927-wasm-simulation-planning-design.md
  - docs/wasm-control-architecture.md
  - docs/performance-budget.md
---

# Simulator Worker Protocol

The browser simulator uses the `sir-simulator-session` protocol. It is distinct
from replay inspection even though both protocols share the same retained
engine worker. The simulator protocol version starts at `1`.

The browser boundary and session state machine are qualified. Workspace-only
validation covers the canonical plan envelope, bounds, horizon, and preview
classification. Authoritative validation is owned by the native `SirPlan`
map/ruleset/controller compiler. It supplies pinned projections through
`LoadAuthoritativeRun`; step and run-to then select those exact shared-kernel
results instead of interpreting plan commands in the browser.

Every request and response carries a correlation tuple:

```text
operation · session · map revision · plan revision · expected tick
```

Responses additionally carry the worker's current committed tick. The worker
rejects a stale session, map, plan revision, or tick. The browser runner keeps
the active tuple and pending operation set, and drops a response before
dispatch if any identity is stale. Worker rejection and browser filtering are
both required: rejection explains invalid work, while filtering prevents a
late response from changing a replacement workspace.

## Operations

The bounded operation set is:

- initialize a session with its first disclosed projection;
- validate a canonical plan document;
- preview at most 1,200 ticks;
- commit an accepted plan revision;
- load a match-lock- and replay-pinned authoritative projection journal;
- step 1–256 ticks;
- run to a tick within the committed 6,000-tick planning horizon;
- reset to the initialized map projection; and
- cancel an operation by its operation identity.

Run-to yields cooperatively at 256-tick boundaries. A full 6,000-tick planning
horizon therefore produces 23 progress deltas and one completion delta: 24
projection messages, rather than one message per tick.

Empty deltas remain available only to the intent-only workspace rehearsal.
They are not authoritative simulation. A loaded authoritative run is strict:
missing requested ticks are rejected instead of synthesized. The worker also
rejects non-monotonic sequence/projection revisions, non-32-byte identities,
duplicate or invalid unit IDs, out-of-board positions, negative health, and
projection frames above the 256-unit disclosure bound.

## Projection and disclosure

Snapshots and deltas reuse `InspectionProjectionTransport`, the existing
bounded worker projection. A delta contains only disclosed changed fields.
Empty arrays mean "no disclosed change"; they do not authorize the UI to copy
data from an authoritative or hidden world.

Preview provenance is mandatory:

- deterministic rehearsal has no assumptions;
- assumption-based rehearsal lists every named assumption; and
- intent-only projection lists authored intentions and returns no unit, edge,
  event, or checkpoint state.

The preview channel never upgrades disclosure. A perspective or intent-only
request cannot receive hidden state merely because another session or retained
worker state contains it.

## Evidence

`scripts/smoke-worker-roundtrip.mjs` sends every request and observes every
response variant through the built browser worker and Node's structured-clone
boundary, including a match-lock/replay-pinned authoritative run. It checks
cancellation, stale correlation, preview disclosure, and the
projection-message and elapsed-time budgets. `SIR.Client.Tests` checks
diagnostic classification and the browser workspace guard independently.
