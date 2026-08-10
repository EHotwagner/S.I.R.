---
schemaVersion: 1
workId: 148-live-session-authentication
title: Live Session Authentication
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/148-live-session-authentication/spec.md
sourceClarifications: work/148-live-session-authentication/clarifications.md
sourceChecklist: work/148-live-session-authentication/checklist.md
publicOrToolFacingImpact: true
---

# Live Session Authentication Plan

Prose status: planned

## Source Snapshot
- spec: work/148-live-session-authentication/spec.md sha256:b136f96004c5dad7982dd7130ddf1d87c85d5cc941888dd4d80c20a92be9450f schemaVersion:1
- clarifications: work/148-live-session-authentication/clarifications.md sha256:4c66db51dac206c49ad024e4b2fbd97a72a436ab9596a5895548e37b230e3f20 schemaVersion:1
- checklist: work/148-live-session-authentication/checklist.md sha256:4dab4d82d0dc13a893e3b4de7308531b8f0185e00d28b0902cd1cd3e55a26fc0 schemaVersion:1

## Plan Scope
- Add a server-owned live-admission component and wire it through bootstrap, SignalR, the shared HTTP protocol, and the Fable SignalR binding.
- Keep the game simulation and snapshot protocol unchanged; only admission metadata and authorization boundaries change.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Configure ASP.NET Core authentication and an actor-claim authorization check before bootstrap; make `LiveAuthority.bootstrap` accept a verified principal/actor and issue an opaque, short-lived admission credential.
- PD-002 [AC-002] [FR-002] complete: Extend `BootstrapV1.Response` with `accessToken`, change the Fable SignalR binding to accept an access-token factory, and construct the hub without session or actor query parameters.
- PD-003 [AC-003] [FR-003] complete: Parse only supported bearer metadata, validate signature/expiry/session/actor/principal binding in `LiveAuthority`, cache the validated binding in `Context.Items`, and revalidate it before every hub method state transition.
- PD-004 [AC-004] [FR-004] complete: Track a nonce and connection generation in the in-memory session record; connection takeover replaces the current generation and all superseded contexts fail authorization while the current connection may resync.
- PD-005 [AC-005] [FR-005] complete: Add focused server and browser tests for rejected bootstrap and hub routes plus the allowed browser connect/advance/reconnect route; invert each added authorization assertion to record red evidence before final green.
- PD-006 [AC-006] [FR-006] complete: Keep tokens opaque, omit them from status attributes and error text, and ensure server error paths emit stable error codes rather than request credentials.
- PD-007 [AC-007] [FR-007] complete: Implement a development authentication handler that activates only under `Development` and explicit `SIR_ALLOW_ANONYMOUS_LIVE_SESSIONS=true`; all other environments challenge unauthenticated bootstrap and hub requests.

## Contract Impact
- PC-001 [PD-001] [PD-002] protocol: `BootstrapV1.Response.accessToken` is an opaque bearer value; `sessionId` and `actorId` remain response identifiers but are never authentication material, and SignalR receives the bearer through `accessTokenFactory`.

## Verification Obligations
- VO-001 [PD-001] [PD-003] [PD-004] [PD-005] semanticTest: Run server and browser production-route coverage for allowed admission, all rejection modes, takeover, and reconnect; retain a fail-before/pass-after inversion for each new security gate.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] compatibility: Decode the new response field in server and Fable code together; old clients fail bootstrap decode rather than silently falling back to query credentials.

## Generated View Impact
- GV-001 [PD-001] generatedView: Refresh the SDD work model, summary, and Codex/Claude guidance after authored artifacts change; server/client build outputs remain derived and are not hand-edited.

## Accepted Deferrals
None.

## Planning Findings
None.

## Advisory Notes
- The in-memory token/session store is bounded to the current single-process live slice; durable distributed revocation is deliberately out of scope and must not be inferred as production scaling support.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 148-live-session-authentication`.
