---
schemaVersion: 1
workId: 148-live-session-authentication
title: Live Session Authentication
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/148-live-session-authentication/spec.md
publicOrToolFacingImpact: true
---

# Live Session Authentication Clarifications

## Source Specification
- work/148-live-session-authentication/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: Which authenticated identity source can this small host rely on without selecting an external provider?
- CQ-002 [AMB:AMB-002]: What revocation and takeover representation is correct for the single-process live slice?
- CQ-003 [AMB:AMB-003]: How can browser integration obtain a principal without weakening production admission?

## Answers
- CQ-001 → Bootstrap uses ASP.NET Core's authenticated `HttpContext.User`; the requested actor must match the principal's actor claim. Provider selection remains host configuration, not this feature.
- CQ-002 → A server-held session record owns a random token nonce, expiry, revocation flag, and current connection id/generation. New bootstrap revokes the prior actor/session admission; a new connection with a valid current token takes over and invalidates the old connection.
- CQ-003 → A development authentication handler may mint a test principal only when both `Development` and an explicit configuration opt-in are true. It is selected before bootstrap and hub authorization and is rejected in every other environment.

## Decisions
- DEC-001 [CQ-001] [AMB:AMB-001] [FR-001] [FR-003]: Use the host authenticated principal and an actor claim for bootstrap and hub authorization; do not choose or embed an identity provider.
- DEC-002 [CQ-002] [AMB:AMB-002] [FR-003] [FR-004]: Issue an opaque signed bearer token containing no actor identifier; store its nonce, expiry, revocation state, and connection generation server-side for this live slice.
- DEC-003 [CQ-003] [AMB:AMB-003] [FR-005] [FR-007]: Permit the development test principal only behind a `Development` plus explicit opt-in guard; production has no anonymous fallback.

## Accepted Deferrals
- None.

## Remaining Ambiguity
- None.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 148-live-session-authentication`.
