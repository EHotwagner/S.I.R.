---
schemaVersion: 1
workId: 184-scenario-catalog
title: Elaborate deterministic tactical sample and scenario catalog
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

# Elaborate deterministic tactical sample and scenario catalog Charter

## Identity
A Tier-1 catalog of substantial, deterministic tactical scenarios that is simultaneously a player-facing Samples experience, an onboarding/design reference, a replay/conformance fixture, and a performance workload.

## Principles
- Every scenario is authored data with explicit engine, ruleset, content, map, seed, checkpoint, and retained-replay identity.
- Scenarios boot and advance through production editor/simulator/replay/browser routes; helper-only state injection cannot satisfy player-facing acceptance.
- Semantic state and canonical event/checkpoint streams are authoritative. Selected visual evidence complements them but never replaces them.
- Representative and stress catalogs exercise production update plus projection/view with deterministic structural counters before elapsed-time qualification.

## Scope Boundaries
- In: a versioned package schema; a fast teaching scenario plus six composed tactical families; authored terrain, edges, zones, forces, facing/attention, plans and objectives; identity-safe import/export; retained replays; design notes; .NET/Fable/browser, mutation, performance, and production-journey evidence.
- Out: random map generation, campaign persistence, matchmaking, comprehensive balance, and new framework combat/spatial algorithms.

## Policy Pointers
- Honor constitution I/III/VI/VIII: specify before code, declare public package surfaces, use fail-capable real-route tests, and reject stale or malformed identity visibly.
- Follow the producer-owned `docs/performance-budget.md` intent and focused `fs-gg-game-core`, `fs-gg-mapcraft`, `fs-gg-grids`, `fs-gg-line-drawing`, `fs-gg-visibility`, `fs-gg-ballistics`, `fs-gg-effects`, `fs-gg-ai`, and `fs-gg-playtest` contracts.

## Lifecycle Notes
- The typed delivery route requires analyze/implementationReady before any implementation-path edit, followed by evidence, verify, and ship.
- The item touch set and coordination claim remain the scope authority; lifecycle/feedback paths were widened through the typed coordination engine.
