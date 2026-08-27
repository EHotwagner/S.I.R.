---
schemaVersion: 1
workId: 352-quint-q4-sir-adoption
title: Adopt Quint authority for the complete S.I.R. combat rule corpus
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

# Adopt Quint authority for the complete S.I.R. combat rule corpus

## Identity
Migrate all sixteen rules in `CombatRules.registry` from F# typed-kernel authoring to the published Quint-backed Typed SDD v2 backend while preserving executable behavior and frozen compatibility evidence.

## Principles
- Literate Quint is the sole new behavioral authority; extracted `.qnt`, compiled contracts, bindings, and projections are deterministic generated artifacts.
- Exercise Quint at deliberately different granularities: immutable content facts, pure fixed-point formulae, a registered external-algorithm contract, focused state transitions, and an aggregate attack/consequence transition.
- The real F# interpreter remains the implementation under correspondence testing, not a coequal authored specification.
- The registered `FS.GG.Game.Core.Los.lineOfSightBy` supercover algorithm remains external; Quint specifies its bounded input/output contract and observables without copying its implementation.
- Stable rule identities, canonical compatibility bytes, explanation order/shape, native/Fable parity, package-only consumption, and authenticated rollback remain fail-closed obligations.
- Pin and execute the exact published coherent set that implements FS.GG.SDD#932, with 1.4.0 as the compatibility floor; do not reference producer source projects.

## Scope Boundaries
- In: all sixteen registered combat rules, their dependency catalogue, facts, formulae, algorithm boundary, attack/consequence/cover/recovery behavior, generated products, frozen corpora, native/Fable/runtime/ITF correspondence, rollback evidence, and the exact published Q3-plus-profile package pins.
- Out: changing gameplay arithmetic or public gameplay APIs, reimplementing supercover in Quint, changing unrelated client/rendering behavior, changing the FS.GG.SDD producer contract, or beginning the Q5 coordination model.
- Retain authenticated v1 rollback evidence, but remove F# as coequal authoring authority for the migrated corpus.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Constitution principles I, II, III, VI, VII, and VIII govern specification-first work, structured contracts, public surfaces, evidence, shared agent/human authority, and fail-closed diagnostics.
- ADR-0077, the Quint-first migration design, issue #352, and route decision revision 2 govern the authoring boundary and broadened consumer scope.

## Lifecycle Notes
- Tier 1: canonical authoring and package-consumer contracts change, although gameplay behavior and public S.I.R. APIs must remain compatible.
- FS.GG.SDD#924 is discharged by the dual-feed-verified 1.4.0 release.
- The user approved the explicit Quint type/state/atomicity sketch on 2026-08-27.
- FS.GG.SDD#932 owns the newly discovered producer requirement for a versioned general consumer-model profile; standalone model authoring and runtime correspondence can proceed while canonical backend integration waits for its published coherent set.
