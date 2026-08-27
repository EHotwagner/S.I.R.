---
schemaVersion: 1
workId: 366-main-ci-routing
title: Route main-push CI by relevant changes
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Route main-push CI by relevant changes Specification

Prose status: specified

## User Value
Ordinary main merges receive a fast, relevant protected verdict while nightly and manually requested runs still prove the complete system.

## Scope
- SB-001: Extend the canonical route and routed gate DAG to exact `HEAD^..HEAD` main-push changes.
- SB-002: Extend protected receipts/verdicts and qualified-site handoff so every accepted result is exact-source and fail-closed.
- SB-003: Preserve current pull-request routing and complete schedule/manual qualification.

## Non-Goals
- SB-004: Do not change game, runtime, rendering, or handbook content other than the roadmap's operational evidence entry.
- SB-005: Do not remove any complete qualification subject or make an absent, stale, malformed, cancelled, failed, or mismatched result count as passing.
- SB-006: Do not make Pages rebuild documentation or infer deployability from workflow success alone.

## User Stories
- US-001 (P1): As a maintainer, I get a protected main verdict from the smallest sufficient route for the exact landed merge.
- US-002 (P1): As a release owner, I retain complete nightly and on-demand qualification independent of focused merge routing.
- US-003 (P1): As a documentation reader, I receive a Pages deployment only from an exact, successfully qualified site artifact.
- US-004 (P1): As an evidence consumer, I can distinguish missing, stale, malformed, cancelled, failed, and source-mismatched protected inputs.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001] [FR-002]: Given an ordinary push to `main`, when routing runs, then it classifies exactly `HEAD^..HEAD`, always runs integrity, and runs only the canonical selected producers, helpers, and gates.
- AC-002 [US-001] [US-004] [FR-003] [FR-004]: Given unknown, mixed, workflow, router, or CI-contract changes, when routing runs, then the conservative complete gate route is selected and the protected verdict rejects any missing, stale, malformed, cancelled, failed, unexpected, or binding-mismatched required receipt.
- AC-003 [US-002] [FR-005]: Given a scheduled or manually dispatched run, when CI runs, then complete preflight and clean-room production qualification remain required regardless of the last diff.
- AC-004 [US-003] [FR-006] [FR-007]: Given a main merge, when Pages considers deployment, then only a routed documentation/browser-delivery run or a complete run can publish an exact-source qualified-site handoff; unrelated successful merges cannot deploy.
- AC-005 [US-001] [US-004] [FR-008]: Given existing PR route fixtures and main/protected/Pages mutations, when focused tests run, then PR semantics remain unchanged and every named safety mutation fails before restoration.

## Functional Requirements
- FR-001: Ordinary main pushes MUST classify the exact landed `HEAD^..HEAD` path set with the existing canonical route policy and exact commit/tree identity. (Stories: US-001; Acceptance: AC-001)
- FR-002: Ordinary main pushes MUST run the integrity floor and exactly the route-selected producers, helpers, and gates; PR events MUST retain their existing classification and join semantics. (Stories: US-001; Acceptance: AC-001)
- FR-003: Unknown, mixed/cross-cutting, workflow, router, protected-receipt, Pages-contract, and otherwise unclassifiable paths MUST select the conservative complete route. (Stories: US-001, US-004; Acceptance: AC-002)
- FR-004: The stable protected verdict MUST join typed exact-source receipts and fail on missing, stale, malformed, cancelled, failed, unexpected, digest-mismatched, route-mismatched, or source-mismatched required results. (Stories: US-004; Acceptance: AC-002)
- FR-005: Scheduled and `workflow_dispatch` runs MUST retain complete protected preflight and clean-room production qualification, including their existing subjects and site handoff. (Stories: US-002; Acceptance: AC-003)
- FR-006: A focused main route MUST create a qualified-site handoff only when selected authoritative inputs require documentation and browser delivery, and that handoff MUST bind the exact source and qualification receipts. (Stories: US-003; Acceptance: AC-004)
- FR-007: Pages MUST deploy only when the triggering successful main-push CI run exposes and verifies the exact qualified-site handoff; unrelated merges and schedule/manual runs MUST not deploy. (Stories: US-003; Acceptance: AC-004)
- FR-008: Documentation plus strong positive and negative tests MUST prove event routing, exact diff/source binding, conservative fallback, full periodic coverage, protected joining, site handoff selection, and PR behavior retention. (Stories: US-001, US-004; Acceptance: AC-005)

## Ambiguities
- AMB-001: Whether focused main qualification should duplicate the existing routed DAG or execute it through the same jobs and receipt join.
- AMB-002: How one stable protected verdict declares different required receipt sets for focused pushes versus complete schedule/manual runs.
- AMB-003: How the routed documentation gate transports a site without allowing Pages to rebuild or deploy unrelated merges.

## Public Or Tool-Facing Impact
- Changes the GitHub Actions event topology and protected verdict/Pages artifact contract; no product API changes.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 366-main-ci-routing`.
