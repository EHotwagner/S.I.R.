---
schemaVersion: 1
workId: 353-quint-q1-sir-replay
title: Quint Q1 Sir Replay
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Quint Q1 Sir Replay Specification

Prose status: specified

## User Value
exact cross-repository runtime/model correspondence evidence

## Scope
- SB-001: test-only S.I.R. conformance fixtures, committed Q1 witness corpus, scripts, work, and readiness; no production authority, API, package, provider, or historical evidence changes

## Non-Goals
- SB-002: Do not implement later lifecycle commands or Governance enforcement in this specification.

## User Stories
- US-001 (P1): As a user, I can exact cross-repository runtime/model correspondence evidence.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given the exact producer witness, when the focused replay runs, then the real combat interpreter matches every declared observable state and each independent mapping or implementation mutation fails at its first divergence.
- AC-002 [US-001] [FR-002]: Given the selected conformance CI route, when qualification runs, then pinned Quint and evaluator binaries generate at least 64 deterministic model-derived ITF traces and the production adapter replays every state.
- AC-003 [US-001] [FR-003]: Given any injected divergence, when replay stops, then the diagnostic names the transition, stable action, fixture path, JSON pointer, adapter source, and implementation source.
- AC-004 [US-001] [FR-004]: Given a passing qualification, when its receipt is inspected, then it binds the executed .NET muxer, SDK entry point, hostfxr, runtime tree, package locks, model tools, adapter, implementation, corpus, seed, and bounds by exact path and digest.

## Functional Requirements
- FR-001: replay Initialize then ApplyDamage(3) then ApplyDamage(20) through CombatRules.resolveConsequences and compare hitPoints, lastAction, and lastAmount after every transition with exact producer, adapter, implementation, trace, seed, and bound fingerprints; require five injected mapping/implementation defects to fail at their first divergent transition (Stories: US-001; Acceptance: AC-001)
- FR-002: the selected conformance CI route shall use the committed producer-derived Quint model plus pinned Quint 0.32.0 and evaluator 0.6.0 binaries to generate at least 64 deterministic ITF traces and replay their complete normalized state corpus through the production adapter; an unrelated route shall skip this work (Stories: US-001; Acceptance: AC-002)
- FR-003: every first-divergence failure shall report transition index, stable action identity, fixture path, JSON pointer, adapter source path, and production implementation source path, including the independent combat-boundary mutation at the real interpreter seam (Stories: US-001; Acceptance: AC-003)
- FR-004: qualification receipts shall bind exact bytes and resolved paths for the executed .NET muxer, SDK dotnet entry point, hostfxr, runtime tree, combined package locks, pinned Quint and evaluator tools, model, adapter, implementation, corpus, seed, bounds, and JUnit output (Stories: US-001; Acceptance: AC-004)

## Ambiguities
No material ambiguities recorded.

## Public Or Tool-Facing Impact
- This specification is an SDD lifecycle artifact and command-report contract input.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 353-quint-q1-sir-replay`.
