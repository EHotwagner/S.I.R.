---
schemaVersion: 1
workId: 186-authored-tactical-parcels
title: Authored tactical parcels and semantic environment
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

# Authored tactical parcels and semantic environment Charter

## Identity
- Work id: `186-authored-tactical-parcels`
- Lifecycle stage: charter
- Status: chartered

## Principles
- Authored plot and parcel content is versioned data; assembly is deterministic, seeded, bounded, and content-addressed.
- Semantic edge and volume state is one authority shared by movement, sight, projectile, effect, sound, cover, and interaction queries.
- Destruction is targeted and bounded: state transitions may breach or destroy declared features but never propagate emergent structural collapse.
- Editor operations remain pure, replayable, undoable, and accessible through production keyboard and pointer routes.
- Performance is an authored contract with production-route structural counters and Release evidence, not an inferred timing claim.

## Scope Boundaries
- In scope: plot/parcel schema and seeded assembly, semantic environment state, cover and bounded destruction, local spatial invalidation, editor import/export/migration/undo/redo/preview, production UI use, fixtures, documentation, cross-runtime and performance evidence.
- Out of scope: arbitrary procedural generation, unbounded verticality, structural-collapse simulation, physics debris, and final art production.
- Existing `SpatialQuery` and physical-combat authorities are extended through declared contracts; browser code remains projection/input only.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Governance files are optional compatibility pointers and are not evaluated by this command.
- Constitution principles I, II, III, V, VI, VII, and VIII govern specification, typed contracts, public surfaces, pure transitions, evidence, guidance currency, and safe failure.

## Lifecycle Notes
- Tier 1 public schema and editor workflow change; signatures, migration, fixtures, browser journey, mutation proofs, performance evidence, and documentation land together.
- Governing issue: `EHotwagner/S.I.R.#186`; delivery route is `sdd-required`.
- No downstream milestone deferral is accepted for the issue's required environment-content phase.
- Next lifecycle action: `fsgg-sdd specify --work 186-authored-tactical-parcels`.
