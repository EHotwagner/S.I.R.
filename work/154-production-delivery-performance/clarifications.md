---
schemaVersion: 1
workId: 154-production-delivery-performance
title: Production Delivery Performance
stage: clarify
changeTier: tier1
status: clarified
sourceSpec: work/154-production-delivery-performance/spec.md
publicOrToolFacingImpact: true
---

# Production Delivery Performance Clarifications

## Source Specification
- work/154-production-delivery-performance/spec.md

## Clarification Questions
No clarification questions recorded.

## Answers
No clarification answers recorded.

## Decisions
- DEC-001 [FR-001] [FR-002]: Let ASP.NET Core own response compression and static-file cache headers. A reverse proxy/CDN may add equivalent behavior, but must not strip `Vary` or weaken the documented cache policy.
- DEC-002 [FR-004] [FR-005]: Treat the normal simulator route as initial; defer the Rules Lab mode because it is not needed to reach the first interactive simulator route and can be loaded explicitly on activation.
- DEC-003 [FR-006] [FR-007]: Check deterministic raw/Brotli/gzip file budgets and a browser-observed Release request graph. Use CDP network throttling and CPU throttling only as measurement conditions, not timing pass/fail thresholds.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
No blocking ambiguity remains.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 154-production-delivery-performance`.
