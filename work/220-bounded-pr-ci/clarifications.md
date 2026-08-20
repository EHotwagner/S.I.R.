---
schemaVersion: 1
workId: 220-bounded-pr-ci
title: Bounded PR CI
stage: clarify
sourceSpec: work/220-bounded-pr-ci/spec.md
changeTier: tier1
status: clarified
---

# Bounded PR CI Clarifications

## Source Specification
- `work/220-bounded-pr-ci/spec.md`

## Clarification Questions
- **CQ-001** (AMB-001): Which path taxonomy defines the smallest sufficient representative routes?
- **CQ-002** (AMB-002): Which integrity subjects run on every PR?
- **CQ-003** (AMB-003): Where is the reusable artifact boundary?
- **CQ-004** (AMB-004): How are skipped and failed DAG nodes joined deterministically?
- **CQ-005** (AMB-005): How is the five-minute feedback budget measured?
- **CQ-006** (AMB-006): Which events own focused and full qualification?

## Answers
- CQ-001 → `docs/**` plus the docs owner script is documentation-only; domain/source/project changes are domain; browser specs/config plus presentation web assets are browser; workflow, locks, build/receipt scripts, lifecycle policy, unknown paths, or mixed classes are cross-cutting. Policy order is explicit and unknown is conservative.
- CQ-002 → routing/schema self-tests, changed-path normalization, workflow/config/lock integrity, receipt-schema parsing, exact build inventory, generated-view currency, npm audit policy, and forbidden authority-boundary checks form the always-on floor.
- CQ-003 → one prepare node restores and builds the candidate prerequisites, creates/verifies the existing content-addressed production receipt plus a route artifact manifest, uploads immutable relative outputs, and consumers verify after download. Explicit clean-room full qualification remains isolated and does not reuse a PR cache as proof.
- CQ-004 → every potential gate has a stable id and emits a typed result or typed skip reason. An `always()` join reads the declared route and all gate result states in stable policy order, fails on missing/unexpected/cancelled/failed required nodes, and accepts only declared non-applicable skips.
- CQ-005 → a route receipt records runner start and deterministic per-phase durations; the join enforces 300,000 ms while representative cross-cutting acceptance targets at most 240,000 ms, preserving 60,000 ms of measured runner-variance and bounded inventory-growth headroom. Queue time is recorded separately when GitHub exposes it, product performance remains outside this budget, and a lucky retry cannot replace headroom.
- CQ-006 → pull requests run the focused route; protected `main` pushes and a scheduled event run the complete clean-room qualification. Superseded PR runs share one concurrency group and cancel without changing the latest candidate's obligations.

## Decisions
- **DEC-001** [AMB:AMB-001] [FR-001] [FR-002]: Commit a closed, ordered route-policy schema with documentation, domain, browser, and cross-cutting classifications; mixed or unknown inputs select cross-cutting and every gate records its matching rule.
- **DEC-002** [AMB:AMB-002] [FR-004] [FR-012]: Keep a cheap always-on integrity job for route/config/lock/receipt/inventory/generated-view/audit/authority checks, and require it before any expensive route job.
- **DEC-003** [AMB:AMB-003] [FR-005] [FR-006] [FR-010]: Prepare candidate prerequisites once, bind them through the current production receipt plus an immutable route manifest, and verify identities after artifact download; clean-room full qualification is a separate named exception.
- **DEC-004** [AMB:AMB-004] [FR-007] [FR-012]: Model each gate as a stable DAG result and use an unconditional deterministic join that rejects missing or unexpected states and accepts skips only when policy says non-applicable.
- **DEC-005** [AMB:AMB-005] [FR-003] [FR-011]: Budget the latest candidate from focused runner start to join verdict at 300,000 ms, require the representative cross-cutting acceptance run to finish within 240,000 ms, record phase/gate/critical-path/runner-minute facts, and never mix this runner contract with gameplay performance.
- **DEC-006** [AMB:AMB-006] [FR-008] [FR-009]: PR events own focused qualification; protected pushes and schedules own the complete clean-room route, with concurrency cancellation limited to superseded PR candidates.

## Accepted Deferrals
- None.

## Remaining Ambiguity
- None. AMB-001 through AMB-006 are resolved by DEC-001 through DEC-006.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 220-bounded-pr-ci`.
