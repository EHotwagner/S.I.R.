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
- AC-001 [US-001] [FR-001]: Given Quint Q1 Sir Replay is available, when the user exercises it, then they can exact cross-repository runtime/model correspondence evidence.

## Functional Requirements
- FR-001: replay Initialize then ApplyDamage(3) then ApplyDamage(20) through CombatRules.resolveConsequences and compare hitPoints, lastAction, and lastAmount after every transition with exact producer, adapter, implementation, trace, seed, and bound fingerprints; require five injected mapping/implementation defects to fail at their first divergent transition (Stories: US-001; Acceptance: AC-001)

## Ambiguities
No material ambiguities recorded.

## Public Or Tool-Facing Impact
- This specification is an SDD lifecycle artifact and command-report contract input.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 353-quint-q1-sir-replay`.
