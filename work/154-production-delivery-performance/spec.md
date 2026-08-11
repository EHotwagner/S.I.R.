---
schemaVersion: 1
workId: 154-production-delivery-performance
title: Production Delivery Performance
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Production Delivery Performance Specification

Prose status: specified

## User Value
Production users receive a responsive client whose delivery assets have explicit compression, caching, and size guarantees.

## Scope
- SB-001: ASP.NET Core static-asset delivery, Vite client build output, automated production-route measurement, and deployment documentation.
- SB-002: No CDN deployment, engine protocol changes, replay format changes, or changes to deterministic engine selection and integrity manifests.

## Non-Goals
- Do not claim that application middleware can override a proxy/CDN which removes or replaces headers; document the owning deployment layer instead.

## User Stories
- US-001 (P1): As a production user, I receive a compressed, cache-safe initial application route without downloading every supporting mode before I use it.
- US-002 (P1): As a maintainer, I get a deterministic CI failure when production-delivery bytes or HTTP delivery contracts regress.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001] [FR-002] [FR-003]: Given the Release server hosts a built client, when a browser requests an immutable engine asset and mutable entry assets with Brotli/gzip accepted, then it receives content encoding, `Vary: Accept-Encoding`, immutable caching only for content-addressed engine assets, and safe revalidation for mutable entry assets.
- AC-002 [US-001] [FR-004] [FR-005]: Given a throttled first visit, when the user reaches the default route and then activates a deferred mode or supporting panel, then the recorded initial-route bytes exclude that deferred code and the mode activation fetches it successfully.
- AC-003 [US-002] [FR-006] [FR-007]: Given a production client build, when an asset, compression result, initial-route request graph, or manifest/integrity invariant regresses beyond its declared contract, then the delivery-budget gate fails with the responsible artifact and measurement.

## Functional Requirements
- FR-001: The production server MUST negotiate Brotli and gzip for compressible client assets and emit `Vary: Accept-Encoding`. (Stories: US-001; Acceptance: AC-001)
- FR-002: The production server MUST give content-addressed engine assets an immutable long-lived cache policy and mutable entry assets a safe revalidation policy. (Stories: US-001; Acceptance: AC-001)
- FR-003: Cache and compression configuration MUST leave engine selection, integrity manifests, offline/error behavior, and source-map delivery policy intact. (Stories: US-001; Acceptance: AC-001)
- FR-004: The client build MUST defer a non-default mode or supporting-panel code path until activation without changing the first interactive route. (Stories: US-001; Acceptance: AC-002)
- FR-005: The delivery measurement MUST exercise the Release artifact under declared network and CPU throttling and report first-load and deferred-mode activation byte totals. (Stories: US-001; Acceptance: AC-002)
- FR-006: CI MUST reject a production delivery artifact when any defined raw, compressed, or initial-route byte budget is exceeded. (Stories: US-002; Acceptance: AC-003)
- FR-007: The delivery-budget gate MUST fail if its HTTP/cache/compression expectations or integrity-manifest protections are violated, and name the responsible artifact or request. (Stories: US-002; Acceptance: AC-003)

## Ambiguities
No material ambiguities recorded.

## Public Or Tool-Facing Impact
- This specification is an SDD lifecycle artifact and command-report contract input.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 154-production-delivery-performance`.
