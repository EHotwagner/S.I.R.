---
schemaVersion: 1
workId: 148-live-session-authentication
title: Live Session Authentication
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Live Session Authentication Specification

## User Value
Players can enter and continue an authorized live session without exposing bearer credentials.

## Scope
- SB-001: Server bootstrap, SignalR admission and intent authorization, scoped token lifecycle, browser transport, redaction, and browser integration tests.
- SB-002: The server remains the sole authority for principal, actor, match, session, and input authorization.

## Non-Goals
- SB-003: Account-management UX, identity-provider choice, durable user administration, and unrelated game-simulation changes are out of scope.

## User Stories
- US-001 (P1): As an authenticated player, I can enter and reconnect to my authorized live session without exposing a bearer credential in a URL.
- US-002 (P1): As a service owner, I can reject attempts to use another actor's, stale, revoked, or replayed session credential.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given an authenticated principal authorized for the requested actor and match, when bootstrap succeeds, then it returns a scoped expiring admission credential and an initial snapshot.
- AC-002 [US-001] [FR-002]: Given a live browser session, when it connects or reconnects, then it sends its credential through SignalR's supported access-token metadata and no credential appears in the hub URL or rendered DOM.
- AC-003 [US-002] [FR-003]: Given a hub connection or intent, when its bound principal, session, actor, token expiry, or token revocation is invalid, then the server aborts or rejects it without mutating the live state.
- AC-004 [US-002] [FR-004]: Given a valid credential is replayed after a newer connection takes over, when the older connection sends an intent, then the server rejects it while the current connection may reconnect and resync.
- AC-005 [US-002] [FR-005]: Given an unauthorized bootstrap, cross-actor request, query-string credential, expired/revoked token, replay attempt, and reconnect, when integration tests exercise them, then each outcome is asserted.
- AC-006 [US-002] [FR-006]: Given an operator inspects application logs, diagnostics, or the browser status element, when a credential is present on the request path, then its value is absent or redacted.
- AC-007 [US-001] [FR-007]: Given development anonymous admission is enabled, when the host is not Development or the explicit opt-in is absent, then bootstrap remains unavailable anonymously.

## Functional Requirements
- FR-001: The server MUST require and authorize a principal for the requested actor and match before creating a live session and issuing a scoped expiring admission credential. (Stories: US-001; Acceptance: AC-001)
- FR-002: The browser MUST supply the admission credential via SignalR's supported access-token metadata and MUST NOT put credentials in query strings, the DOM, logs, or diagnostics. (Stories: US-001; Acceptance: AC-002)
- FR-003: The hub MUST validate the credential and its principal/session/actor binding on every connection and every intent, and MUST reject invalid, expired, revoked, or cross-actor use without state mutation. (Stories: US-002; Acceptance: AC-003)
- FR-004: The server MUST enforce one current connection generation per session, reject superseded-token replay, and allow the current holder to reconnect and resync. (Stories: US-002; Acceptance: AC-004)
- FR-005: Integration coverage MUST exercise unauthenticated admission, authorization denial, query-string rejection, cross-actor access, expiry, revocation, replay/takeover, and successful reconnect. (Stories: US-002; Acceptance: AC-005)
- FR-006: The server and client MUST redact admission credentials from logs and diagnostics and MUST never render credentials into the DOM. (Stories: US-002; Acceptance: AC-006)
- FR-007: Any anonymous development admission MUST require an explicit development-only configuration opt-in and MUST fail closed in every other environment. (Stories: US-001; Acceptance: AC-007)

## Ambiguities
- AMB-001: The credentials and claims available from the host identity provider have not yet been selected.
- AMB-002: Whether revocation is represented by an in-memory session version for this live slice or a durable cross-node store must be resolved.
- AMB-003: The authenticated browser test identity and its development-host configuration need an explicit test-only boundary.

## Public Or Tool-Facing Impact
- The bootstrap response gains a credential field and the SignalR construction binding gains an access-token factory; this is a version-1 live transport contract change.
- Server admission, hub authorization, and browser status diagnostics are security-sensitive public behavior.

## Lifecycle Notes
- Tier 1: publish contract and integration behavior together. Next lifecycle action: `fsgg-sdd clarify --work 148-live-session-authentication`.
