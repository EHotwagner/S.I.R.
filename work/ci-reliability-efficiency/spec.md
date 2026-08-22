---
schemaVersion: 1
workId: ci-reliability-efficiency
title: Reliable and Efficient CI Qualification
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Reliable and Efficient CI Qualification Specification

Prose status: specified

## User Value
Contributors receive a pre-merge verdict that predicts default-branch health,
maintainers see the first actionable protected failure early, and the repository
spends materially fewer runner-minutes and artifact bytes for unchanged evidence.

## Scope
- SB-001: Focused PR routing, protected and scheduled qualification, Pages delivery, prepared-artifact topology, setup/restore ownership, timing and cost receipts, action/runtime hygiene, and fail-closed optimization tests.
- SB-002: Apply the new FS-GG `.github` conservative subject-classification and original-result parsing patterns only to equivalent S.I.R. gates, while preserving S.I.R.'s user-owned board and repository-local authority.

## Non-Goals
- SB-003: Do not change product behavior, production workloads, browser or mutation populations, accessibility assertions, dependency-surface checks, or product-performance budgets.
- SB-004: Do not treat a cache, uploaded archive, skipped job, retry, or generated dashboard as evidence without exact source/tree/tool/lock/command/output verification.
- SB-005: Do not introduce self-hosted runners, paid runner classes, custom base images, permission expansion, or producer-repository workflow changes.
- SB-006: Do not preserve every current job boundary merely for familiarity; preserve stable subject ids and typed attribution even when measured low-cost subjects share a runner.

## User Stories
- US-001 (P1): As a contributor, I receive a merge-blocking verdict that catches stale generated review artifacts and other predictable protected-route defects before merge.
- US-002 (P1): As a maintainer, I see a named protected/scheduled subject failure and downloadable partial receipts without waiting for an opaque fourteen-minute aggregate to finish.
- US-003 (P1): As a repository owner, I retain the complete evidence surface while reducing cross-cutting PR runner-minutes, redundant setup, and prepared-artifact bytes.
- US-004 (P2): As a release owner, I deploy Pages only from the exact successfully qualified main SHA and do not rebuild the same production/documentation site independently.
- US-005 (P2): As a CI maintainer, I can explain every executed or omitted expensive self-test, compare end-to-end hosted costs, and roll back each optimization independently.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001] [FR-002]: Given a candidate whose production bundle disagrees with tracked map-editor or other generated review artifacts, when PR routing runs, then the merge verdict fails on a named freshness subject before protected or Pages execution; the regenerated candidate passes the same subject.
- AC-002 [US-002] [FR-003] [FR-004]: Given a protected or scheduled run with an early, middle, or late subject mutation, when qualification runs, then a typed failure plus partial diagnostic artifacts appears within 300 seconds and the deterministic final join rejects missing, stale, failed, cancelled, or unexpected subject receipts.
- AC-003 [US-003] [FR-005] [FR-006] [FR-007]: Given the representative cross-cutting workload from run 32523274261, when the optimized PR DAG runs on `ubuntu-latest`, then every retained subject passes within 240 seconds, actual GitHub job time is at least 20 percent below 1,518 seconds, and prepared uploads are at least 20 percent below 351,337,554 bytes.
- AC-004 [US-003] [FR-008] [FR-009]: Given Linux-hosted native/server consumers, when prepared artifacts are produced and reconstructed, then only required Linux runtime assets cross the boundary, duplicate closure bytes are content-addressed or eliminated, exact modes/hashes remain verified, and mutation/removal of any required file fails closed.
- AC-005 [US-003] [US-005] [FR-010] [FR-011]: Given low-test/setup-dominated subjects and expensive gate self-tests, when ownership is optimized, then stable typed subject attribution remains, conservative classifiers run on relevant/unknown/rename/delete inputs, omissions carry reasons, and each wrong omission or zero-test result is killed by a focused inversion.
- AC-006 [US-004] [FR-012]: Given a successful protected main SHA, when Pages delivery runs, then it verifies and deploys the exact qualified site receipt without recompiling; a failed, mismatched, missing, or unqualified SHA never deploys.
- AC-007 [US-005] [FR-013] [FR-014]: Given any PR, protected, scheduled, or Pages run, when telemetry is finalized, then full hosted job/action/post-step durations, artifact bytes, cache facts, retries, failure stage, critical path, runner-minutes, and candidate identity are available under a versioned schema and reconcile to the GitHub Jobs/Artifacts APIs within a declared tolerance.
- AC-008 [US-005] [FR-015]: Given an action/runtime upgrade or any optimization milestone, when focused workflow tests and a final hosted run execute, then deprecated action-runtime warnings are absent, permissions and required contexts are unchanged, rollback is one bounded commit, and no unchanged subject is lost.

## Functional Requirements
- FR-001: PR routing MUST select a production-review freshness gate for every production-bundle input, review generator, review artifact, build script, lock, workflow, unknown path, or cross-cutting change. (Stories: US-001; Acceptance: AC-001)
- FR-002: The freshness gate MUST compare tracked review evidence with a production-equivalent generated bundle, fail with exact stale paths/digests, and include self-restoring stale/regenerated controls. (Stories: US-001; Acceptance: AC-001)
- FR-003: Protected and scheduled qualification MUST expose independently attributable prepare, mutation, functional, browser/performance, documentation/site, SDD/Governance, and final-join receipts instead of one opaque serial job. (Stories: US-002; Acceptance: AC-002)
- FR-004: Every protected/scheduled subject MUST checkpoint a typed result and diagnostic artifact under `always()` semantics; the first named failure MUST be visible within 300 seconds and no failed dependency may be rendered as an acceptable skip. (Stories: US-002; Acceptance: AC-002)
- FR-005: The representative cross-cutting PR verdict MUST remain at most 240,000 milliseconds and accepted optimization evidence MUST use a source-frozen exact-head hosted run rather than an unchanged retry. (Stories: US-003; Acceptance: AC-003)
- FR-006: Against run 32523274261, actual summed GitHub job duration MUST fall by at least 20 percent from 1,518 seconds while every selected subject, mutation population, test count, and product budget remains equal or larger. (Stories: US-003; Acceptance: AC-003)
- FR-007: Prepared artifact upload bytes MUST fall by at least 20 percent from 351,337,554 bytes, with per-artifact and aggregate compressed/uncompressed sizes recorded before and after. (Stories: US-003; Acceptance: AC-003)
- FR-008: Linux PR producers MUST exclude non-Linux runtime assets and redundant closure copies that consumers cannot load, without changing publish/package portability or protected cross-runtime evidence. (Stories: US-003; Acceptance: AC-004)
- FR-009: Prepared artifacts MUST remain immutable, content-addressed, mode-preserving, exact-candidate-bound, and reconstructed only after transport and staged-byte verification; no consumer may rebuild on verification failure. (Stories: US-003; Acceptance: AC-004)
- FR-010: Setup-dominated subjects MAY share a hosted job only when their stable ids, commands, timings, independent failures, and join obligations remain separate typed receipts and measured critical path does not regress. (Stories: US-003, US-005; Acceptance: AC-005)
- FR-011: Expensive synthetic/mutation self-tests MAY be conditionally omitted only through a committed conservative impact classifier that runs for relevant, topology, rename, deletion, malformed, or unknown inputs and emits a stable measured omission reason otherwise. Original test results MUST be parsed for non-vacuity rather than rerun. (Stories: US-005; Acceptance: AC-005)
- FR-012: Pages MUST consume an exact-SHA qualified site receipt from successful protected main qualification, verify every byte before deployment, and perform no independent restore/build/Fable/Vite/fsdocs work. (Stories: US-004; Acceptance: AC-006)
- FR-013: Timing authority MUST include GitHub job start/end, setup actions, restores, builds, artifact upload/download/extraction, tests, post steps, finalization, queue when available, retry count, and failure stage; script-only receipts MUST be labeled partial. (Stories: US-005; Acceptance: AC-007)
- FR-014: A versioned cost report MUST reconcile workflow critical path, total runner-minutes, per-job setup/test ratios, cache hits, artifact bytes, and omitted reasons against the GitHub Jobs and Artifacts APIs, rejecting missing or materially inconsistent facts. (Stories: US-005; Acceptance: AC-007)
- FR-015: Workflow actions MUST use supported Node-runtime-compatible releases with least permissions, explicit timeouts, stable required contexts, and fail-closed YAML/topology/subject mutation tests; each milestone MUST document an independent rollback boundary. (Stories: US-005; Acceptance: AC-008)

## Ambiguities
- AMB-001: Which protected qualification subjects can safely share prepared outputs or runners while retaining independent attribution and isolation.
- AMB-002: Whether Pages should consume a cross-workflow artifact or move build/deploy behind the protected qualification workflow.
- AMB-003: Which bytes in the 169 MB native and 132 MB server artifacts are required by Linux consumers, and how reconstruction preserves runtime probing.
- AMB-004: Which current setup-dominated gates should be consolidated, and which parallelism is still critical-path positive.
- AMB-005: Which S.I.R. self-tests match the producer `.github` conservative-classifier pattern and which must remain unconditional.
- AMB-006: How end-to-end cost telemetry is finalized after post steps without making the product verdict depend on a privileged observer.
- AMB-007: What staged rollout and quantitative rollback thresholds prevent a broad CI rewrite from obscuring causality.
- AMB-008: Which action major upgrades are supported on the pinned Node/.NET runner contract and how immutable action identity is recorded.

## Public Or Tool-Facing Impact
- Extends CI route, gate, timing, artifact, cost, protected-join, and Pages handoff contracts; changes workflow topology and required implementation commands while preserving stable subject identities.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work ci-reliability-efficiency`.
