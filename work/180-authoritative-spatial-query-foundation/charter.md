---
schemaVersion: 1
workId: 180-authoritative-spatial-query-foundation
title: Authoritative footprint-aware spatial-query foundation
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

# Authoritative footprint-aware spatial-query foundation Charter

## Identity
Create the S.I.R.-owned authoritative spatial-query foundation that gives simulation, control, and clients one deterministic interpretation of footprints, terrain, semantic edges, direction, visibility, bounded paths, reachability, exposure, and cover inputs.

## Principles
- Spatial truth is evaluated once in portable typed F# and projected to clients; JavaScript never becomes geometry authority.
- Every adopted `FS.GG.Game.Core` primitive is consumed package-only and only under its declared `LockstepExact` profile.
- Hidden information is a contract boundary: values, explanations, diagnostics, timing classes, and cache metadata reveal no fact outside requester knowledge.
- Cached and uncached evaluation are byte-equivalent, with immutable identity keys and bounded local invalidation.
- Public contracts, fixtures, performance budgets, mutation evidence, player-reachable diagnostics, and lifecycle evidence ship together.

## Scope Boundaries
- In: versioned footprint-aware line traces, exact LOS, bounded paths, reachability, movement cost, crossed cells/edges, cover contributors, exposure directions, query keys, caches, explanations, control-facing services, and browser diagnostics.
- In: square footprints and transition envelopes; diagonal corner rules; terrain and modality-specific semantic-edge permeability; stance, height, facing, sensor/movement profiles, occupancy, immutable map/ruleset identity, spatial revision, and requester knowledge.
- In: .NET/Fable/Node/browser canonical comparison, negative mutations, representative/maximum-map workloads, and 100/200-unit demand evidence.
- Out: physical damage resolution, overlay styling, arbitrary 3D levels, full multi-agent route scheduling, and broadening the upstream package profile.

## Policy Pointers
- Honor constitution I–III by specifying first, declaring the public F# surface, and treating structured schemas/fixtures as the machine contract.
- Honor constitution IV–V with plain records/unions and pure spatial evaluation/cache transitions behind explicit I/O edges.
- Honor constitution VI–VIII with real package/runtime fixtures, fail-capable gates, actionable bounded diagnostics, and no silent fallback.
- Apply `.fsgg/sdd.yml`, `.fsgg/agents.yml`, and the `fs-gg-game-core-fable-lockstep-v1` package contract; Governance remains optional compatibility metadata.

## Lifecycle Notes
- Tier 1: this creates public F# APIs, canonical result/explanation contracts, cache identity/invalidation behavior, browser-visible diagnostics, and multi-runtime evidence.
- No issue-level requirement may be deferred silently; any genuine downstream boundary must name its receiving work item and remain out of this item’s acceptance claim.
- Next lifecycle action: `fsgg-sdd specify --work 180-authoritative-spatial-query-foundation`.
