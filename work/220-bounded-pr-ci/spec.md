---
schemaVersion: 1
workId: 220-bounded-pr-ci
title: Bounded PR CI
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Bounded PR CI Specification

Prose status: specified; Tier 1 CI topology, routing, and receipt contract extension.

## User Value
Maintainers receive a correct, explainable PR verdict within five minutes for representative changes, while protected and scheduled qualification retain every existing evidence subject.

## Scope
- SB-001: Path-aware PR routing, an always-on integrity floor, explicit parallel gate topology, deterministic joining, immutable receipt reuse, timing evidence, tests, and local/process documentation.
- SB-002: Full protected and scheduled qualification continues to cover rules, spatial, cancellation, cross-runtime, browser, documentation, performance, Governance, SDD, mutation, and historical compatibility subjects.

## Non-Goals
- SB-003: Do not change production gameplay, visual, simulation, browser-journey, or documentation-content behavior.
- SB-004: Do not remove evidence, turn absent/unreadable evidence into success, lower product budgets, or replace the production-build receipt without a versioned migration.
- SB-005: Do not make a single forever-growing monolithic size or timing assertion the extension mechanism; route policy and per-gate budgets are independently versioned.

## User Stories
- US-001 (P1): As a contributor, I receive the smallest sufficient required gate set for the paths I changed and can see why each other gate was skipped.
- US-002 (P1): As a reviewer, I receive deterministic, separately attributed results from independent gates without waiting for their serial sum.
- US-003 (P1): As a release owner, I retain the complete clean-room ship qualification and a scheduled drift detector even when PR routing omits unaffected surfaces.
- US-004 (P1): As an evidence consumer, I can reuse only immutable artifacts whose exact inputs, tools, command, candidate, and outputs are current.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001] [FR-002]: Given documentation-only, F# domain-only, browser-only, and cross-cutting path sets, when routing runs, then each receives its declared smallest sufficient gates and every skip carries a policy reason.
- AC-002 [US-001] [US-002] [FR-003] [FR-004]: Given any PR path set, when required qualification runs on `ubuntu-latest`, then the deterministic join reports an actionable required verdict within five minutes and preserves gate-specific attribution; the representative cross-cutting acceptance run completes within 240 seconds so the enforced boundary retains at least 60 seconds of runner-variance headroom.
- AC-003 [US-004] [FR-005] [FR-006]: Given a prepared candidate artifact, when downstream gates reuse it, then each prerequisite is produced at most once and reuse rejects any candidate/input/configuration/lock/tool/command/output drift.
- AC-004 [US-002] [FR-007]: Given independent rules, spatial, cancellation, browser, documentation, and evidence gates, when the DAG runs or one outcome is mutated, then independence is concurrent and the join deterministically reports the named failure.
- AC-005 [US-003] [FR-008] [FR-009]: Given a protected push or schedule, when full qualification runs, then all pre-change subjects remain present and a deliberately hidden cross-surface defect is caught by the scheduled route.
- AC-006 [US-004] [FR-010] [FR-011]: Given a candidate and route, when receipts and timing are emitted, then exact identities, queue/setup/restore/build/test/total durations, cache/reuse/build counts, routes/skips, retries, and failure stage are recorded under committed schemas.
- AC-007 [US-001] [US-004] [FR-012]: Given malformed/unknown paths, missing/stale/malformed receipts, poisoned cache identity, duplicate/unknown builds, or an authority-boundary mutation, when qualification runs, then it fails closed with an actionable subject.

## Functional Requirements
- FR-001: The PR router MUST classify documentation-only, F# domain-only, browser-only, and cross-cutting changes from normalized changed paths using a committed versioned policy. (Stories: US-001; Acceptance: AC-001)
- FR-002: Every routed and skipped gate MUST record the matching policy fact, and unknown paths MUST select the conservative cross-cutting route. (Stories: US-001; Acceptance: AC-001)
- FR-003: Required PR feedback MUST complete within 300 seconds from runner start to actionable deterministic join verdict on the declared `ubuntu-latest` runner for the representative route matrix. The representative cross-cutting acceptance run MUST complete within 240 seconds, retaining at least 60 seconds of headroom for observed runner variance and bounded producer-inventory growth; retrying an over-budget head does not satisfy this requirement. (Stories: US-001, US-002; Acceptance: AC-002)
- FR-004: Formatting, compilation, signature, generated-view, and deterministic contract failures MUST be ordered before expensive browser/performance work, while independent gates remain explicit DAG nodes with fixed attribution. (Stories: US-002; Acceptance: AC-002)
- FR-005: Native, Fable, client, server, and documentation prerequisites MUST be produced at most once per candidate unless a named clean-room subject requires isolation, then published as immutable content-addressed artifacts for consumers. (Stories: US-004; Acceptance: AC-003)
- FR-006: Reuse MUST reject source, configuration, dependency lock, toolchain, owning command, candidate revision/tree, expected path, or output-identity drift and retain exact-once Fable inventory checks. (Stories: US-004; Acceptance: AC-003)
- FR-007: Rules, spatial, cancellation, browser, documentation, and evidence checks MUST expose concurrent DAG nodes and a deterministic mutation-tested join rather than one opaque serial failure. (Stories: US-002; Acceptance: AC-004)
- FR-008: Protected ship qualification MUST retain complete clean-cache, mutation, production browser/docs, performance, Governance, SDD verify/ship, cross-runtime, and historical compatibility subjects. (Stories: US-003; Acceptance: AC-005)
- FR-009: A scheduled full route MUST exercise the same retained full surface and prove a cross-surface defect omitted by a valid focused route is still caught. (Stories: US-003; Acceptance: AC-005)
- FR-010: Route, artifact, gate, and aggregate receipts MUST bind exact commit/tree, locks, tools, commands, inputs, outputs, ownership, and result under committed versioned schemas. (Stories: US-004; Acceptance: AC-006)
- FR-011: Timing evidence MUST separate runner feedback from product performance and record per-gate queue/setup/restore/build/test/total durations, cache/reuse facts, invocation counts, routes/skips, retries, failure stage, critical path, and runner-minutes. (Stories: US-002, US-004; Acceptance: AC-006)
- FR-012: Changed-path, unknown-path, missing/stale/malformed-receipt, cache-poisoning, duplicate-build, unknown-build, deterministic-join, and authority-boundary mutations MUST fail closed and restore every changed fixture. (Stories: US-001, US-004; Acceptance: AC-007)

## Ambiguities
- AMB-001: Which committed path taxonomy is the authoritative smallest-sufficient route for each representative change class.
- AMB-002: Which cheap integrity subjects must run for every PR before route-specific work.
- AMB-003: Which artifact boundary permits downstream reuse without weakening clean-room subjects or repeating builds.
- AMB-004: How the DAG records skipped gates and deterministically joins success, failure, cancellation, and superseded-run states.
- AMB-005: How the five-minute budget is measured without confusing queue variance with product performance.
- AMB-006: Which event owns full qualification for protected pushes and scheduled drift detection.

## Public Or Tool-Facing Impact
- Replaces the serial PR workflow with versioned path-routing, explicit gate/join jobs, immutable artifact metadata, timing receipts, and distinct PR/protected/scheduled/local commands.
- Existing production receipt schema and every protected qualification subject remain compatible and authoritative.

## Lifecycle Notes
- Required route: implementation-ready analyze before workflow/script edits, then evidence, verify, and ship.
- Next lifecycle action: `fsgg-sdd clarify --work 220-bounded-pr-ci`.
