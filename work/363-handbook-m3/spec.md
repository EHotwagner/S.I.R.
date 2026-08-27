---
schemaVersion: 1
workId: 363-handbook-m3
title: Handbook M3
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Handbook M3 complete combat-rule walkthroughs Specification

Prose status: specified

## User Value
A Quint learner can understand and execute every stable S.I.R. combat rule at its appropriate modeling granularity.

## Scope
- SB-001: Complete only roadmap M3 in the existing handbook: catalogue/dependency documentation, sixteen complete rule references and traceability rows, appropriately granular executable walkthroughs, the external line-of-sight contract, three exercise levels, scoped qualification, and merge-boundary ledger evidence.

## Non-Goals
- SB-002: Do not change combat semantics, the authoritative Quint model, production F# code, or package pins.
- SB-003: Do not add M4's counterexample and mutation laboratory, M5's broad runtime correspondence and replay material, M6's complete link enforcement, or M7's publication review and maintenance handoff.

## User Stories
- US-001 (P1): As a Quint beginner, I can locate each stable combat rule, understand its dependency position, and see the appropriate model construct that makes it executable.
- US-002 (P1): As a learner, I can follow focused wound/incapacity, suppression/recovery, cover/collision/destruction, penetration, collateral, and aggregate-resolution transitions without mistaking explanatory order for runtime-visible intermediate state.
- US-003 (P1): As a reviewer, I can distinguish the external line-of-sight algorithm contract from the bounded trace-ratio model and verify all handbook excerpts against the literate authority.
- US-004 (P1): As a maintainer, I can prove sixteen-of-sixteen reference and traceability coverage and preserve the roadmap, lifecycle, and feedback history at the merge boundary.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given the stable registry, when the catalogue and dependency chapters are inspected, then exactly sixteen IDs appear with authoritative kind, direct dependencies, model subjects, and explanation-order guidance.
- AC-002 [US-001] [FR-002]: Given facts, formulas, algorithms, and transitions use different Quint constructs, when a learner follows the modeling map, then each rule is connected to a pure value/helper, contract entry, action, observation, or property at its appropriate granularity.
- AC-003 [US-002] [FR-003]: Given focused consequence and cover scenarios, when their named Quint runs execute, then wound/incapacity, suppression/recovery, collision/cover/destruction, penetration, collateral, and aggregate-resolution outcomes are visible through authoritative helpers, actions, observations, or properties.
- AC-004 [US-003] [FR-004]: Given `COMBAT-TRACE-002`, when the line-of-sight chapter is read, then the learner can explain the external `FS.GG.Game.Core.Los.lineOfSightBy` boundary, its fingerprint and inputs, and why the handbook does not reimplement supercover.
- AC-005 [US-004] [FR-005]: Given the reference section and traceability matrix, when the focused coverage audit runs, then all sixteen stable rule IDs have one complete reference entry and one complete traceability row with no duplicates or placeholders.
- AC-006 [US-001] [FR-006]: Given the exercise section, when readers choose a level, then beginner prediction, intermediate trace/dependency interpretation, and advanced bounded-authority exercises are present without preempting M4 mutation work.
- AC-007 [US-003] [FR-007]: Given the edited handbook, when scoped and full qualification run, then authoritative subjects/excerpts, named runs, sixteen-rule coverage, structural links, strict docs rendering, and the full Q4/runtime suite pass under the pinned toolchain.
- AC-008 [US-004] [FR-008]: Given M3 is ready to merge, when the roadmap and lifecycle receipts are checked, then only M3 changes to checked and its evidence names implementation, qualification, feedback, independent review, CI, PR, merge, and post-merge obligations.

## Functional Requirements
- FR-001: The handbook MUST document exactly sixteen stable rule IDs with their kinds, direct dependencies, relevant Quint subjects, and a dependency graph that distinguishes dependency order from explanation order. (covers AC-001)
- FR-002: The handbook MUST map facts, formulas, the external algorithm contract, and transitions to the existing authoritative pure values/helpers, catalogue entries, actions, observations, runs, and properties without inventing new model state. (covers AC-002)
- FR-003: The handbook MUST provide executable, appropriately granular walkthroughs for wound/incapacity, suppression/recovery, current collision and cover/destruction, penetration, collateral, and atomic aggregate resolution, naming how each focused transition becomes visible. (covers AC-003)
- FR-004: The handbook MUST explain `COMBAT-TRACE-002` as an external line-of-sight contract bound to `FS.GG.Game.Core.Los.lineOfSightBy`, including fingerprint, integer-sample inputs, bounded ratio result, and the authority reason not to duplicate supercover. (covers AC-004)
- FR-005: Every one of the sixteen stable rules MUST have one complete reference entry and one complete traceability row covering kind, dependencies, reads/effects/events, model subjects, verification route, and granularity, with mechanically audited uniqueness and completeness. (covers AC-005)
- FR-006: The handbook MUST include beginner, intermediate, and advanced exercises with prediction or interpretation prompts and answer guidance, while deferring deliberate defect/counterexample laboratories to M4. (covers AC-006)
- FR-007: Qualification MUST use dedicated docs, link, focused-authority, full Q4/runtime, roadmap-ledger, and lifecycle receipts whose locators prove their exact claims; executable snippets and named subjects MUST be extracted from or mechanically checked against `docs/rules/sir-combat.md`. (covers AC-007)
- FR-008: The roadmap MUST preserve prior milestone wording/history, mark only M3 complete at the truthful merge-candidate boundary, and append concise work/readiness/feedback/review/PR/merge/post-merge evidence. (covers AC-008)

## Ambiguities
- AMB-001: Several catalogue rules share one aggregate action; the handbook must show each rule's visible helper/observation/property without presenting those explanations as independently observable runtime transitions.
- AMB-002: `COMBAT-PENETRATION-001` is represented through retained-effect and attack observations rather than a standalone state-changing action; the walkthrough must state that granularity honestly.
- AMB-003: M3 requires exercises and executable coverage but must not become M4's mutation/counterexample laboratory or overstate sampled runs as exhaustive proof.

## Public Or Tool-Facing Impact
- This specification is an SDD lifecycle artifact and command-report contract input.

## Lifecycle Notes
- Issue: `EHotwagner/S.I.R.#363`.
- Stable feedback cycle: `roadmap-sir-combat-quint-handbook-m3-complete-rules`.
- Next lifecycle action: `fsgg-sdd clarify --work 363-handbook-m3`.
