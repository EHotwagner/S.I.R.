---
schemaVersion: 1
workId: 146-authoritative-live-snapshots
title: Integrate authoritative live snapshots into the Elmish tactical workspace
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

# Integrate authoritative live snapshots into the Elmish tactical workspace Charter

## Identity
- Work id: `146-authoritative-live-snapshots`
- Lifecycle stage: charter
- Status: chartered

## Principles
- Keep SignalR lifecycle and authoritative snapshot data inside Elmish Model/Msg/effects.
- Render accepted authority through the shared tactical scene, preserving deterministic disclosure boundaries.
- Exercise the real player-visible command route and retain reconnect/resync diagnostics.

## Scope Boundaries
- In: client MVU integration, server live-session behavior required for reconnect/resync, browser evidence, and qualification documentation.
- Out: redesigning the tactical editor, changing gameplay rules, or broad transport/authentication work unrelated to the live projection.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Governance files are optional compatibility pointers and are not evaluated by this command.

## Lifecycle Notes
- Tier 1 cross-layer change. Complete the performance-first plan, player-journey evidence, and both map-editor and persistent-workspace-M9 review bindings if the client bundle changes.
