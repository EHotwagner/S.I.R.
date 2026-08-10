---
schemaVersion: 1
workId: 147-live-session-resource-lifecycle
title: Live Session Resource Lifecycle
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Live Session Resource Lifecycle Specification

Prose status: specified

## User Value
Operators can run the live server without unauthenticated clients exhausting process memory or serializing unrelated sessions.

## Scope
- SB-001: Bootstrap admission, session lifecycle and per-session concurrency in src/SIR.Server, browser integration coverage, and live integration qualification documentation; no durable or multi-process session implementation.

## Non-Goals
- SB-002: Do not implement later lifecycle commands or Governance enforcement in this specification.

## User Stories
- US-001 (P1): As a user, I can operators can run the live server without unauthenticated clients exhausting process memory or serializing unrelated sessions.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given Live Session Resource Lifecycle is available, when the user exercises it, then they can operators can run the live server without unauthenticated clients exhausting process memory or serializing unrelated sessions.

## Functional Requirements
- FR-001: The live server rejects oversized bootstrap requests and admissions exceeding configured rate or capacity limits, expires disconnected sessions, releases their resources, and permits independent session mutations without a process-global lock. (Stories: US-001; Acceptance: AC-001)

## Ambiguities
No material ambiguities recorded.

## Public Or Tool-Facing Impact
- This specification is an SDD lifecycle artifact and command-report contract input.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 147-live-session-resource-lifecycle`.
