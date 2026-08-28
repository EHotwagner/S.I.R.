---
schemaVersion: 1
workId: 382-handbook-roadmap-rollup
title: Handbook Roadmap Rollup
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Handbook Roadmap Rollup Specification

Prose status: specified

## User Value
Maintainers can inspect one terminal report proving every completed Combat in Quint roadmap feedback cycle and every checkpoint was validated, explicitly dispositioned, and tied to delivery evidence without creating an endless reporting cycle.

## Scope
- SB-001: Reporting, machine audit, and roadmap navigation only; no handbook or sealed M7 maintenance-record change and no new feedback cycle.
- SB-002: The repository-derived set currently contains twelve cycles and forty-eight checkpoint records; those counts are observations the audit must recompute, not constants that define membership.

## Non-Goals
- SB-003: Do not edit any prior checkpoint, schema-v2 report, actionability audit, combat rule, Quint model, runtime implementation, diagram, visual evidence, or handbook semantics.
- SB-004: Do not introduce a new roadmap milestone, feedback cycle, broad CI route, or external semantic authority.

## User Stories
- US-001 (P1): As a roadmap maintainer, I can prove that the terminal roll-up covers the complete repository-derived cycle set rather than a remembered list.
- US-002 (P1): As a feedback owner, I can trace every checkpoint record to exactly one explicit disposition and its bound report/audit evidence.
- US-003 (P1): As a reviewer, I can falsify cycle, checkpoint, binding, count, disposition, and milestone completeness claims through isolated controls.
- US-004 (P1): As a publication owner, I can trace each delivery cycle to its issue/PR and hosted main/Pages evidence where repository or GitHub history makes it available.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given the current repository, when the roll-up audit enumerates matching checkpoint streams and report front matter, then exactly the same complete cycle set is present in the final report and no extra matching cycle is omitted.
- AC-002 [US-002] [FR-002]: Given every enumerated cycle, when validation runs, then its checkpoint stream, schema-v2 report, actionability audit, activation phases, event count, and exact bindings pass the existing fail-closed tools.
- AC-003 [US-002] [FR-003]: Given every checkpoint JSONL line, when the final report is parsed, then exactly one row binds its cycle and one-based sequence to a permitted disposition class with evidence-specific rationale.
- AC-004 [US-004] [FR-004]: Given completed delivery history, when a maintainer reads the coverage matrix, then every cycle names its report and available issue, PR, exact-main CI, and Pages evidence without fabricating unavailable proof.
- AC-005 [US-003] [FR-005]: Given isolated mutations for omitted cycle, omitted checkpoint, wrong report/audit binding, count mismatch, invalid disposition, and unchecked milestone, when the audit self-test runs, then every mutation observes red before untouched input restores green.
- AC-006 [US-001] [FR-006]: Given the complete roadmap ledger, when the audit runs, then all ten required checked headings M0, M1, M2, M3, O1, M4, M5, M6, M6V, and M7 are present and checked.
- AC-007 [US-004] [FR-007]: Given the public report, when documentation qualification and delivery complete, then roadmap and maintenance navigation resolve, relevant-only PR CI passes, guarded merge lands, and exact-main CI plus Pages/live content are verified.
- AC-008 [US-001] [FR-008]: Given this terminal roll-up, when cycle enumeration runs, then it excludes itself and the change creates no matching feedback checkpoint/report/audit artifact.

## Functional Requirements
- FR-001: The audit MUST derive the roadmap cycle set from matching repository checkpoint streams, independently reconcile report-front-matter cycles, and fail on an omitted or unexpected matching cycle. (covers AC-001)
- FR-002: Every derived cycle MUST pass `validate-checkpoints`, exact report/audit `validate`, and `validate-feedback-state` with the phases declared by that bound report. (covers AC-002)
- FR-003: The final report MUST disposition every checkpoint individually as exactly one of `structured finding`, `positive pattern`, `accepted observation`, or `deduplicated existing issue`, with cycle, sequence, phase, kind, summary, evidence, and rationale. (covers AC-003)
- FR-004: The report MUST contain exact recomputed cycle/checkpoint/disposition totals and a one-row-per-cycle coverage matrix naming report, audit, checkpoint count, phases, delivery issue/PR, exact-main CI, and Pages evidence where available. (covers AC-004)
- FR-005: The owning audit MUST prove six isolated observed-red/restored-green controls for omitted cycle, omitted checkpoint, wrong report/audit binding, count mismatch, invalid disposition, and unchecked milestone. (covers AC-005)
- FR-006: The audit MUST require checked roadmap headings M0, M1, M2, M3, O1, M4, M5, M6, M6V, and M7 and reject duplicates or unchecked forms. (covers AC-006)
- FR-007: The report MUST be linked from the roadmap while the exact-source M7 maintenance page remains unchanged, and MUST pass strict docs, relevant-only hosted PR CI, guarded merge, exact-main CI, and exact-SHA Pages/live verification. (covers AC-007)
- FR-008: This work MUST create no `roadmap-sir-combat-quint-handbook-*` checkpoint stream, feedback report, or actionability audit for itself. (covers AC-008)

## Ambiguities
- AMB-001: Decide how repository enumeration distinguishes the twelve handbook-roadmap cycles from unrelated S.I.R. feedback.
- AMB-002: Decide whether delivery evidence absent from repository text is omitted, marked unavailable, or queried live and captured.
- AMB-003: Decide how to bind each checkpoint disposition without copying mutable summary prose into a second authority.

## Public Or Tool-Facing Impact
- Adds one maintained public final report, two navigation links, and a repository-local audit/qualification contract; no gameplay, Quint, runtime, or handbook semantic surface changes.

## Lifecycle Notes
- Issue: `EHotwagner/S.I.R.#382`; no feedback cycle by terminal non-recursion rule.
- Next lifecycle action: `fsgg-sdd clarify --work 382-handbook-roadmap-rollup`.
