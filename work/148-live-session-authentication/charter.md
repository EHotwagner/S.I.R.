---
schemaVersion: 1
workId: 148-live-session-authentication
title: Authenticate live sessions and remove bearer credentials from query strings
stage: charter
changeTier: tier1
status: chartered
policyPointers:
  - .fsgg/sdd.yml
  - .fsgg/agents.yml
  - .fsgg/policy.yml
  - .fsgg/capabilities.yml
  - .fsgg/tooling.yml
---

# Authenticate live sessions and remove bearer credentials from query strings Charter

## Identity
- Work id: `148-live-session-authentication`
- Lifecycle stage: charter
- Status: chartered

## Principles
- Authenticate and authorize at the server boundary; the browser is never an authority.
- Bind every realtime connection and intent to a verified principal, session, and actor scope.
- Keep credentials out of URLs, rendered DOM, logs, diagnostics, and telemetry.
- Make expiry, revocation, replay, reconnect, and takeover behavior explicit and testable.
- Preserve the live game's responsive production route and make authentication-path cost observable.

## Scope Boundaries
- In: authenticated bootstrap admission, actor/match authorization, scoped expiring and revocable session credentials, SignalR credential transport, per-connection and per-intent authorization, redaction, and integration coverage.
- In: an explicit development-only anonymous mode that is unavailable by default in production.
- Out: a general account-management UI, third-party identity-provider selection, durable identity administration, and unrelated game simulation changes.

## Policy Pointers
- Honors constitution principles I (specify before implementation), III (declared public protocol), V (state/I/O boundary), VI (test evidence), and VIII (safe failure and observability).
- Applies the live-runtime performance-first gate before implementation; representative workloads must use the production update/view route and preserve live-compositor evidence requirements.
- Security-sensitive transport and token behavior require Tier 1 protocol, browser, server, and integration evidence.

## Lifecycle Notes
- Delivery route is `sdd-required`; the required work id is `148-live-session-authentication`.
- The front half must reach `implementationReady` before any declared product path changes.
- Next lifecycle action: `fsgg-sdd specify --work 148-live-session-authentication`.
