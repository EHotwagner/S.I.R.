---
schemaVersion: 1
workId: 352-quint-q4-sir-adoption
title: Quint Q4 complete SIR combat adoption
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Adopt Quint authority for the complete S.I.R. combat rule corpus

Prose status: specified

## User Value
S.I.R. maintainers can read and verify one coherent Quint authority that demonstrates modeling across content, arithmetic, external-algorithm contracts, and stateful combat while players observe unchanged behavior.

## Scope
- SB-001: Migrate all sixteen rules in `CombatRules.registry` to literate Quint through the published Typed SDD v2 backend.
- SB-002: Model the existing kinds at fitting granularities: two facts, three formulae, one registered algorithm contract, nine focused transitions, and one aggregate transition.
- SB-003: Consume the exact stable `FS.GG.SDD.Artifacts` and `FS.GG.SDD.Cli` coherent set that implements FS.GG.SDD#932, with 1.4.0 as its compatibility floor, from configured feeds with locked dependency graphs.
- SB-004: Preserve the frozen rule corpus, stable identities, dependency and explanation contracts, native/Fable equality, real-interpreter correspondence, and authenticated v1 rollback.

## Non-Goals
- SB-005: Do not reproduce `Los.lineOfSightBy` supercover pathfinding in Quint; model the registered algorithm's valid sample/result boundary.
- SB-006: Do not replace the real `CombatRules` interpreter with generated gameplay code; check it against the Quint authority.
- SB-007: Do not modify the FS.GG.SDD producer, change the workspace default backend, publish a producer package, or begin Q5.
- SB-008: Do not change combat arithmetic, public gameplay APIs, event/explanation order, or retained historical-package behavior.

## User Stories
- US-001 (P1): As a rule maintainer learning Quint, I can follow examples of facts, pure functions, constraints, actions, state records, invariants, witnesses, and an external algorithm boundary in one production model.
- US-002 (P1): As a simulation consumer, I observe the same damage, health, wound, suppression, cover, collision, collateral, and recovery behavior after migration.
- US-003 (P1): As a reviewer, I can trace exact 1.4.0 producer identity through generated products into native, Fable, corpus, ITF, and rollback evidence without a producer checkout.

## Acceptance Scenarios
- AC-001 [US-001]: Typed SDD inspection proves manifest v2 selects `quint-specification-v1`, literate Markdown is authoritative, all generated products reproduce deterministically, and no F# rule-definition authoring remains for the sixteen-rule corpus.
- AC-002 [US-001]: The model exposes readable examples at each promised granularity and executable witnesses for representative damage, wound thresholds, health/incapacity, suppression eligibility/recovery, cover destruction/blocking, and registry/dependency integrity.
- AC-003 [US-003]: A clean restore and compile uses the exact published producer version implementing FS.GG.SDD#932 and locked dependencies, with no source-project or checkout-relative producer reference.
- AC-004 [US-002]: Native and Fable consumers preserve canonical bytes, stable IDs/dependencies, ordered explanation children/events, representative damage `25 × 1.0 × 0.8 = 20`, and all focused transition outcomes.
- AC-005 [US-002] [US-003]: Exact and sampled Quint traces replay through the real interpreter, and independent mapping/implementation mutations report the first divergent observation.
- AC-006 [US-003]: Authenticated rollback succeeds and interrupted, stale, missing, substituted, or forged inventory fails closed and recovers transactionally.
- AC-007 [US-001] [US-003]: Semantic edits, stale generated outputs, wrong producer/backend/profile identity, malformed contracts, and correspondence defects are observed red before restored-green evidence is accepted.

## Functional Requirements
- FR-001: The repository MUST author the complete sixteen-rule corpus once in literate Markdown with ordered named Quint blocks; embedded Quint owns behavior and extracted `.qnt` is generated and non-authoritative. (covers AC-001)
- FR-002: The authority MUST retain every current rule ID, kind, dependency, read/effect/event contract, and relevant observable explanation order. (covers AC-001, AC-004)
- FR-003: Facts and formulae MUST model the rifle/body constants, engagement preparation, bounded armor retention, fixed-point trace ratio boundary, and expected-damage arithmetic including current rounding semantics. (covers AC-002, AC-004)
- FR-004: The trace rule MUST remain a registered external algorithm contract with the exact symbol/fingerprint and bounded sample/result observables; Quint MUST NOT claim to implement supercover. (covers AC-002, AC-004)
- FR-005: Stateful actions MUST cover attack/consequences, cover impact/destruction, and suppression recovery while exposing the focused rules through pure helpers, ordered observations, and properties rather than inventing runtime-visible intermediate commits. (covers AC-002, AC-004)
- FR-006: Invariants MUST include bounded health/suppression/cover integrity, incapacity iff health is zero, cover destruction iff integrity is zero, valid trace ratios, and suppression increase only after positive damage. (covers AC-002)
- FR-007: Witnesses/tests MUST include damage 20, wound boundaries 24/25/50, health reaching zero, suppression eligibility and five-point recovery cap, destructive cover impact/current-collision blocking, and faction-neutral collateral consequences. (covers AC-002)
- FR-008: Generated bindings MUST replace F# `RuleDefinition`/`SpecificationModel` authoring without replacing the real interpreter and MUST preserve native/Fable/runtime/corpus behavior. (covers AC-001, AC-004)
- FR-009: S.I.R. MUST consume the exact stable coherent producer set that implements FS.GG.SDD#932 through package references and locked graphs only; 1.4.0 remains the compatibility floor. (covers AC-003)
- FR-010: Q4 MUST productionize Q1 correspondence into the expanded state/observation mapping, replay exact and sampled ITF traces, and fail at the first divergence. (covers AC-005)
- FR-011: Q4 MUST retain authenticated v1 rollback and prove accepted rollback, crash recovery, retry, and tamper refusal. (covers AC-006)
- FR-012: Qualification MUST contain observed-red controls for semantic, generation, identity, contract, and correspondence faults. (covers AC-007)

## Ambiguities
No unresolved ambiguities remain. The user approved the `Fixed`, `Wound`, `CombatState`, `AttackInput`, and `Observation` shape and the atomic aggregate-consequence boundary on 2026-08-27.

## Public Or Tool-Facing Impact
- Upgrades the pinned Typed SDD coherent set from its preview to the exact stable release implementing FS.GG.SDD#932; stable 1.4.0 is the compatibility floor.
- Introduces manifest-v2 Quint authority, generated contracts/bindings, migration/rollback artifacts, and expanded qualification for all combat rules.
- S.I.R. gameplay APIs and runtime outcomes remain compatibility surfaces and MUST NOT change.

## Lifecycle Notes
- FS.GG.SDD#932 is the publish-before-adopt dependency for canonical backend integration; standalone Quint authoring, tests, and interpreter correspondence remain independently executable.
- Next lifecycle action: checklist.
