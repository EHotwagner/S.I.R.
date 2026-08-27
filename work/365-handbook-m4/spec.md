---
schemaVersion: 1
workId: 365-handbook-m4
title: Handbook M4
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Handbook M4 Specification

Prose status: specified

## User Value
A Quint learner can distinguish sampled examples, reachable witnesses, invariants, and counterexamples, and can use deliberate defects to understand combat-model verification.

## Scope
- SB-001: Complete only roadmap M4 in the existing handbook: formal claim taxonomy, trace and
  counterexample reading, six mutation families with observed-red/restored-green evidence, action
  reachability, invariant/state binding, scoped qualification, and merge-boundary ledger evidence.

## Non-Goals
- SB-002: Do not change combat semantics, the authoritative Quint model, production F# code, or package pins.
- SB-003: Do not implement M5 runtime correspondence/replay, M6 global definition/link enforcement,
  M6V mechanics/theory SVG diagrams and effects, or M7 publication/maintenance handoff.

## User Stories
- US-001 (P1): As a Quint learner, I can distinguish one example from an existential witness, a
  state invariant, an exhaustive check, and a concrete counterexample.
- US-002 (P1): As a learner, I can read nondeterministic traces and counterexamples without assuming
  that one execution is canonical or that intermediate explanatory order is runtime state.
- US-003 (P1): As a maintainer, I can introduce each scheduled deliberate defect, observe its named
  detector fail, restore the untouched authority, and observe the same route return green.
- US-004 (P1): As a reviewer, I can verify that major actions are reachable, invariants bind to actual
  model-state fields, and every formal claim uses honest sampled-versus-exhaustive language.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given a handbook claim, when its evidence form is inspected, then the text identifies whether it is an example, reachable witness, invariant, sampled run, exhaustive bounded verification, or counterexample and states what that form does and does not establish.
- AC-002 [US-002] [FR-002]: Given nondeterministic `step` behavior, when two valid traces differ, then the learner can identify the chosen action and state delta without treating trace order as a unique prescribed execution.
- AC-003 [US-002] [FR-003]: Given a Quint counterexample, when the learner follows the workflow, then they can identify the violated property, earliest relevant transition, changed state fields, input/guard facts, and smallest authority seam to inspect.
- AC-004 [US-003] [FR-004]: Given threshold, bounds, suppression, cover, collateral, and catalogue-integrity mutations, when the focused laboratory runs, then every defect produces observed red through its named detector and the restored authority produces green through that same route.
- AC-005 [US-004] [FR-005]: Given the model's major actions and named runs, when reachability qualification executes, then `resolveConsequences`, `resolveCoverImpact`, and `resolveRecovery` each have a concrete successful witness whose observation identifies the action.
- AC-006 [US-004] [FR-006]: Given every required invariant, when state-binding qualification runs, then each invariant names and mechanically references the authoritative `CombatState` fields or state-derived observation fields it constrains.
- AC-007 [US-004] [FR-007]: Given executable handbook material, when focused and full qualification run, then authoritative excerpts, mutation fixtures, restored authority, strict docs/links, major-action witnesses, invariants, and full Q4/runtime regression pass under the pinned toolchain.
- AC-008 [US-004] [FR-008]: Given M4 is ready to merge, when roadmap and lifecycle receipts are checked, then only M4 changes to checked and its evidence names implementation, mutation restoration, feedback, independent review, CI, PR, merge, and post-merge obligations while M5/M6/M6V/M7 remain pending.

## Functional Requirements
- FR-001: The handbook MUST define examples, reachable witnesses, invariants, sampled executions, exhaustive bounded verification, and counterexamples with explicit strength and limitation language. (covers AC-001)
- FR-002: The handbook MUST teach nondeterministic trace interpretation from the authoritative `step` action, including action choice, state delta, observation fields, valid alternative ordering, and the non-canonical status of a sampled trace. (covers AC-002)
- FR-003: The handbook MUST give a repeatable counterexample-reading workflow grounded in Quint output and the authoritative state/action/property vocabulary, without inventing runtime-visible intermediate state. (covers AC-003)
- FR-004: Qualification MUST execute named threshold, bounds, suppression, cover, collateral, and catalogue-integrity mutations; each MUST fail through a documented detector before repair and MUST rerun the untouched authority green through the same route. (covers AC-004)
- FR-005: Every major state-changing action (`resolveConsequences`, `resolveCoverImpact`, and `resolveRecovery`) MUST have reachable execution evidence that identifies its action observation and resulting state delta. (covers AC-005)
- FR-006: Every required invariant MUST name the authoritative state or state-derived observation fields it constrains, and qualification MUST reject an invariant entry that lacks a resolvable state binding. (covers AC-006)
- FR-007: Qualification MUST own separate strict-docs, structural-link, focused formal-reasoning/mutation, full Q4/runtime, roadmap-ledger, and lifecycle receipts; snippets and mutation inputs MUST be extracted from or mechanically checked against `docs/rules/sir-combat.md`. (covers AC-007)
- FR-008: The roadmap MUST preserve prior history and later milestones, mark only M4 complete at the merge-candidate boundary, and append concise work/readiness/feedback/review/PR/merge/post-merge evidence. (covers AC-008)

## Ambiguities
- AMB-001: A mutation laboratory needs executable defects but must not edit or duplicate the authoritative model.
- AMB-002: `run` samples nondeterministic behavior while `verify` checks a bounded state space; the handbook needs precise claims for both without teaching tool output as unbounded proof.
- AMB-003: Catalogue integrity is largely a constant-data property, while reachability and state invariants exercise transitions; their detectors must remain distinct and honestly scoped.

## Public Or Tool-Facing Impact
- This specification is an SDD lifecycle artifact and command-report contract input.

## Lifecycle Notes
- Issue: `EHotwagner/S.I.R.#365`.
- Stable feedback cycle: `roadmap-sir-combat-quint-handbook-m4-formal-reasoning`.
- M3's explicit mutation/counterexample deferral is accepted by this work and must be discharged before ship.
- Next lifecycle action: `fsgg-sdd clarify --work 365-handbook-m4`.
