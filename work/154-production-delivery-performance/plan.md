---
schemaVersion: 1
workId: 154-production-delivery-performance
title: Production Delivery Performance
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/154-production-delivery-performance/spec.md
sourceClarifications: work/154-production-delivery-performance/clarifications.md
sourceChecklist: work/154-production-delivery-performance/checklist.md
publicOrToolFacingImpact: true
---

# Production Delivery Performance Plan

Prose status: planned

## Source Snapshot
- spec: work/154-production-delivery-performance/spec.md sha256:d944db2aa6622035bd82bd61b3ee4e264839a3c0509876f34ea12459d06c5fde schemaVersion:1
- clarifications: work/154-production-delivery-performance/clarifications.md sha256:da0ba318a9b57bbf534b6cdc1240cb62c31b7e1a594500de704014ee1d97eafd schemaVersion:1
- checklist: work/154-production-delivery-performance/checklist.md sha256:8e398b50967a5eb7aba85e0b5877216a319a31d128a402e0857efb7c04189fd2 schemaVersion:1

## Plan Scope
- Add explicit Release static-file compression/cache behavior and verify headers using the published server.
- Partition the Rules Lab mode behind a dynamic client loader while retaining the initial simulator and worker paths.
- Add a deterministic delivery-budget script that inspects build bytes, compression, manifest invariants, and a throttled browser request graph.

## Plan Decisions
- PD-001 [FR-001] [FR-002] [FR-003] [DEC-001] complete: Register ASP.NET Core response compression before routing and use static-file callbacks to classify engine-hash paths as immutable while keeping `index.html`, `app.js`, CSS, and generated chunks revalidatable.
- PD-002 [FR-004] [FR-005] [DEC-002] complete: Move the Rules Lab mode boundary behind a dynamic import and expose a loading/error state so its assets are absent from the initial browser request graph and fetched only after deliberate activation.
- PD-003 [FR-006] [FR-007] [DEC-003] complete: Add a Node delivery-budget gate that builds no artifacts itself; it consumes the already-built client and Release publish output, enforces versioned raw/Brotli/gzip and initial-route budgets, and uses Playwright/CDP throttling to record the request graph.

## Contract Impact
- PC-001 [PD-001] HTTP delivery contract: `Content-Encoding`, `Vary: Accept-Encoding`, and `Cache-Control` are asserted on the published ASP.NET Core static-file route; the policy class is determined only from the request path.
- PC-002 [PD-003] delivery budget contract: `scripts/test-production-delivery-budget.mjs` emits a JSON measurement containing build artifact identities, raw/Brotli/gzip bytes, initial/deferred request bytes, and declared throttle conditions.

## Verification Obligations
- VO-001 [PD-001] [PC-001] integrationTest: Publish the server, request compressible and engine assets with each encoding, and verify header policy plus decompressed response readability.
- VO-002 [PD-002] [PC-002] browserTest: Under CDP network/CPU throttling, prove a first route does not fetch the deferred Rules Lab chunk and activating Rules Lab does.
- VO-003 [PD-003] [PC-002] gateTest: Mutate a budgeted asset/header expectation and show the delivery-budget gate rejects the changed subject.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] compatibility: Existing mutable client URLs remain revalidatable, so deployments can update entry assets without a cache purge; only hash-addressed engine assets are immutable.

## Generated View Impact
- GV-001 [PD-001] [PD-002] [PD-003] workModel: Refresh the SDD work model and agent guidance after authored requirements, the delivery script, and evidence declarations change; stale generated views block readiness.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 154-production-delivery-performance`.
