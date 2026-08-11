---
schemaVersion: 1
workId: 136-grid-resolution-footprint
title: "Double grid resolution and use a 4×4 human-unit footprint"
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

# Double grid resolution and use a 4×4 human-unit footprint Charter

## Identity
Double the battlefield grid resolution while retaining the physical scale of human
units through a canonical 4×4-cell footprint and a deterministic content/save
migration posture.

## Principles
- Preserve deterministic domain and simulation behavior across .NET and Fable.
- Keep placement, occupancy, movement, rendering, and serialized content governed
  by one grid-scale rule.
- Prove behavior through production routes and cross-runtime regression tests.

## Scope Boundaries
In: grid units, 4×4 human footprint, collision/path/range/LOS/clearance semantics,
editor preview/validation, rendering scale, content migration, and documented
compatibility behavior. Out: unrelated unit archetype redesign and changes to
non-battlefield map systems.

## Policy Pointers
Honors constitution principles I, III, V, VI, and VIII: specify before code,
declare contracts, keep transitions deterministic, provide failing regression
evidence, and reject malformed legacy data with actionable diagnostics.

## Lifecycle Notes
Tier 1 migration spanning domain, simulation, client/editor, serialization, and
cross-runtime evidence. Performance planning must use the producer-owned workload
and target; no timing target may be invented.
