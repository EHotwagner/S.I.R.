---
schemaVersion: 1
workId: 361-handbook-m2
title: Handbook M2
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Handbook M2 representative attack learning spine Specification

Prose status: specified

## User Value
A Quint beginner can predict, execute, observe, and explain one representative S.I.R. rifle attack end to end.

## Scope
- SB-001: Complete only roadmap M2 in the existing handbook; no broad M3 rule walkthroughs or production semantic changes.

## Non-Goals
- SB-002: Do not implement later lifecycle commands or Governance enforcement in this specification.

## User Stories
- US-001 (P1): As a Quint beginner, I can follow the representative rifle attack from domain facts through raw arithmetic to a state transition and observation.
- US-002 (P1): As a learner, I can predict the trace, run the authoritative model, read each changed field, and use a deliberate negative mutation to understand the witness.
- US-003 (P1): As a runtime reviewer, I can distinguish model execution from scoped production correspondence and see the exact authority and claim boundary.
- US-004 (P1): As a maintainer, I can mechanically prove that the handbook's executable excerpts match the literate Quint authority and that its links, vocabulary, and rendered page remain valid.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given the representative attack, when a beginner follows the pipeline overview, facts, trace, retention, expected damage, and rounding layers, then they can explain `25 x 1.0 x 0.8 = 20` and each scale-10,000 raw value.
- AC-002 [US-001] [FR-002]: Given `CombatState`, `AttackInput`, and `Observation`, when the beginner follows the action, then they can identify stable inputs, changed state, explanatory output, guard, and atomicity boundary.
- AC-003 [US-002] [FR-003]: Given the predict-run-observe-explain workflow, when the named run executes, then the learner can read the initial and successor trace states and reconcile every representative-damage assertion.
- AC-004 [US-002] [FR-004]: Given a one-line armor-retention mutation, when the focused run is executed, then it is observed red for the named assertion and the untouched authority is subsequently restored green.
- AC-005 [US-003] [FR-005]: Given the runtime correspondence section, when it is reviewed, then the model helper/action/observation subjects map to current F# subjects and evidence without claiming that Quint simulation alone proves equivalence.
- AC-006 [US-004] [FR-006]: Given the handbook excerpts and links, when the focused audit and pinned qualification run, then excerpts match `docs/rules/sir-combat.md`, Quint 0.32.0 executes them, vocabulary links resolve, and the strict docs build renders the page.
- AC-007 [US-004] [FR-007]: Given the roadmap, when M2 lands, then only M2 changes to checked and its evidence names implementation, observed-red/restored-green, lifecycle, feedback, review, PR, and merge gates.

## Functional Requirements
- FR-001: The handbook MUST teach the representative attack pipeline, weapon fact, full trace, armor retention, expected-damage multiplication, and final rounding, including `250000 x 10000 / 10000 x 8000 / 10000 = 200000` and `200000 / 10000 = 20`. (covers AC-001)
- FR-002: The handbook MUST explain `CombatState`, `AttackInput`, `Observation`, `initialCombat`, `representativeAttack`, `validAttack`, `resolveConsequences`, and the atomic completed-consequence boundary without inventing runtime-visible intermediate states. (covers AC-002)
- FR-003: The handbook MUST provide a prediction prompt, an authoritative command, a two-state trace-reading workflow, and an observe/explain reconciliation for `representativeDamageIsTwenty`. (covers AC-003)
- FR-004: The verification MUST change only the representative `armorRetentionRaw` from `8000` to `7000` in a disposable extracted model, observe the named run fail from `20` to `18`, and then re-run the untouched extraction green. (covers AC-004)
- FR-005: Runtime correspondence MUST cite `SIR.Domain.FixedPoint`, `CombatRules`, and Q4 replay evidence while stating that Quint execution proves model behavior and real-interpreter correspondence is separate, scoped evidence rather than exhaustive implementation equivalence. (covers AC-005)
- FR-006: Executable handbook excerpts MUST be extracted from or mechanically matched against `docs/rules/sir-combat.md`; focused audits MUST cover code provenance, expected text/arithmetic, links/vocabulary, pinned Quint execution, observed-red/restored-green, and rendered documentation. (covers AC-006)
- FR-007: The roadmap MUST preserve all prior text/history, mark only M2 complete after the gates pass, and append concise evidence paths for work, readiness, feedback, review, PR, and merge. (covers AC-007)

## Ambiguities
- AMB-001: The learner-facing ordinary path uses bounded values, but `roundedDamage` intentionally wraps the signed int32 pre-division addition; the handbook must state this exact boundary without generalizing wrap behavior to helpers that saturate.
- AMB-002: M2 needs one representative runtime map but must not preempt M5's complete correspondence and replay walkthrough.
- AMB-003: Handbook code may be excerpted for readability, but only when its exact declaration is mechanically found in the current authoritative extraction.

## Public Or Tool-Facing Impact
- This specification is an SDD lifecycle artifact and command-report contract input.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 361-handbook-m2`.
