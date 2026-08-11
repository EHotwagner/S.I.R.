---
schemaVersion: 1
workId: 153-separated-project-graph
title: Restore the separated S.I.R. project graph
stage: charter
changeTier: tier1
status: chartered
policyPointers:
  - .fsgg/sdd.yml
  - .fsgg/agents.yml
  - .fsgg/policy.yml
  - .fsgg/capabilities.yml
  - .fsgg/tooling.yml
---

# Restore the separated S.I.R. project graph Charter

## Identity
- Work id: `153-separated-project-graph`
- Lifecycle stage: charter
- Status: chartered

## Principles
- Restore boundaries by moving ownership rather than weakening the canonical graph.
- Preserve the existing live vertical slice while introducing compatibility adapters only at explicit seams.
- Keep generated protocol and runtime-specific dependencies out of domain, simulation, live client, and replay-web boundaries.

## Scope Boundaries
- In: the separated `SIR.Wasm`, `SIR.Protocol.Generated`, `SIR.Replay.Web`, and `SIR.Tools` projects; solution wiring; canonical documentation; and executable dependency/documentation guards.
- In: removing the live `SIR.Client` dependency on `SIR.Simulation` by relocating editor/replay-only ownership to the replay-web host.
- Out: replacing the working HTTP/Thoth plus SignalR vertical slice with a new transport.
- Out: changing match, replay, simulation, or protocol semantics except where project extraction requires a compatibility-preserving move.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Governance files are optional compatibility pointers and are not evaluated by this command.

## Lifecycle Notes
- Tier 1 architecture migration. The recorded board decision selects the original separated graph; scaffold-era combined projects are transitional, not canonical.
