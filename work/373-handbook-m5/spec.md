---
schemaVersion: 1
workId: 373-handbook-m5
title: Handbook M5
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Handbook M5 Specification

Prose status: specified

## User Value
A Quint learner can trace each modeled combat subject to current F# behavior, inspect exact and sampled replay evidence, diagnose the first divergence, and change a rule safely without conflating model and implementation authority.

## Scope
- SB-001: Complete only roadmap M5 in the existing handbook: full correspondence map, literate/generated explanation, exact and sampled ITF replay, first-divergence reporting, negative controls, safe rule-change workflow, scoped qualification, and merge-boundary evidence.

## Non-Goals
- SB-002: Do not change combat semantics, authoritative Quint, production F#, package pins, or existing replay contracts.
- SB-003: Do not implement M6 index/link enforcement, M6V SVG mechanics/theory visuals, or M7 publication/maintenance handoff.

## User Stories
- US-001 (P1): As a learner, I can map model declarations and stable rules to named F# subjects and honest evidence scopes.
- US-002 (P1): As a learner, I can distinguish the literate authority from generated Quint/ITF projections and reproduce both exact and sampled comparisons.
- US-003 (P1): As a maintainer, I can locate the first model/runtime divergence, observe a deliberate mismatch red, restore green, and follow a safe rule-change sequence.
- US-004 (P1): As a reviewer, I can mechanically reject missing correspondence and equivalence overclaims while preserving later roadmap scope.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given any production-behavior claim in the handbook, when its correspondence row is inspected, then it names the stable rule/model subjects, exact current F# subject, evidence route, evidence scope, and correspondence status.
- AC-002 [US-002] [FR-002]: Given the literate model, when qualification runs, then the generated `.qnt` projection is mechanically extracted and identified as disposable output rather than authority.
- AC-003 [US-002] [FR-003]: Given named exact fixtures and the pinned runtime interpreter, when replay runs, then normalized model and runtime observations compare field by field with exact-fixture claim language.
- AC-004 [US-002] [FR-004]: Given the deterministic sampled corpus, when replay runs with its declared seed/sample/step bounds, then all compared observations match and the handbook labels the result sampled rather than exhaustive.
- AC-005 [US-003] [FR-005]: Given an intentionally mutated model-side expected observation, when comparison runs, then it reports the earliest trace index, event/action identity, field path, expected value, and runtime value before untouched inputs restore green.
- AC-006 [US-003] [FR-006]: Given a proposed combat-rule change, when the documented workflow is followed, then authority, projections, correspondence, exact fixtures, sampled replay, observed-red control, full regression, evidence, and documentation are updated in dependency order.
- AC-007 [US-004] [FR-007]: Given incomplete or overstated correspondence prose, when focused qualification runs, then missing runtime/evidence/status classifications and forbidden implementation-equivalence language fail through named controls.
- AC-008 [US-004] [FR-008]: Given M5 is merge-ready, when roadmap and lifecycle receipts are inspected, then only M5 changes to checked and its evidence names implementation, controls, feedback, independent reviews, CI, PR, merge, and post-merge obligations while M6/M6V/M7 remain pending.

## Functional Requirements
- FR-001: The handbook MUST provide a complete Quint-to-F# correspondence map in which every production claim names model/rule subjects, exact F# subjects, evidence, scoped status, and explicit missing correspondence where applicable. (covers AC-001)
- FR-002: The handbook and qualification MUST explain and mechanically check literate-authority extraction into generated `.qnt` and ITF projections, which MUST never be described as authoring sources. (covers AC-002)
- FR-003: Qualification MUST execute exact ITF replay fixtures through the real F# interpreter and compare normalized observations field by field; handbook claims MUST remain limited to the named fixtures. (covers AC-003)
- FR-004: Qualification MUST execute a deterministic sampled ITF corpus with declared seed, sample count, and step bound against the real interpreter; handbook claims MUST call the evidence sampled and non-exhaustive. (covers AC-004)
- FR-005: Qualification MUST introduce one correspondence-only mismatch in an ephemeral fixture, observe a named first-divergence detector report index/event/action/field/expected/actual, and rerun untouched inputs green. (covers AC-005)
- FR-006: The handbook MUST publish a safe rule-change workflow that updates authorities, generated projections, exact and sampled replay, negative controls, full regression, evidence, and documentation in dependency order. (covers AC-006)
- FR-007: Focused qualification MUST reject absent runtime subjects, evidence routes, correspondence classifications, claim bounds, and language asserting that Quint simulation alone proves implementation equivalence. (covers AC-007)
- FR-008: Qualification MUST own separate docs/link, correspondence, exact/sample replay, negative-control/restoration, full Q4/runtime, roadmap, and lifecycle receipts; the roadmap MUST preserve history and later milestones while marking only M5 complete at merge-candidate scope. (covers AC-008)

## Ambiguities
- AMB-001: Existing Q4 replay already compares exact and sampled data, but M5 must teach and audit it without copying a second interpreter or overstating its breadth.
- AMB-002: Several rules correspond through aggregate runtime functions rather than one-to-one F# declarations, so the status vocabulary must distinguish exact, aggregate, external, presentation-only, and absent mappings.
- AMB-003: A useful first-divergence example needs real red evidence while leaving both model and runtime authorities untouched.

## Public Or Tool-Facing Impact
- This specification is an SDD lifecycle artifact and command-report contract input.

## Lifecycle Notes
- Issue: `EHotwagner/S.I.R.#373`.
- Stable feedback cycle: `roadmap-sir-combat-quint-handbook-m5-runtime-correspondence`.
- This work accepts M2/M3's explicit complete-correspondence deferrals and must close them before ship.
- Next lifecycle action: `fsgg-sdd clarify --work 373-handbook-m5`.
