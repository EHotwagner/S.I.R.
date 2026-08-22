---
schemaVersion: 1
workId: ci-reliability-efficiency
title: Ci Reliability Efficiency
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/ci-reliability-efficiency/spec.md
sourceClarifications: work/ci-reliability-efficiency/clarifications.md
sourceChecklist: work/ci-reliability-efficiency/checklist.md
publicOrToolFacingImpact: true
---

# Ci Reliability Efficiency Plan

Prose status: planned

## Source Snapshot
- spec: work/ci-reliability-efficiency/spec.md sha256:6e86d77d74b132b17f056d858cba792116d11aa10eaa39f386d23daa4fc7ebfe schemaVersion:1
- clarifications: work/ci-reliability-efficiency/clarifications.md sha256:a7c9b9a52d84264ba6476effc77038392863edc55b2996b1c6c2b810b94843bc schemaVersion:1
- checklist: work/ci-reliability-efficiency/checklist.md sha256:e34051a66e4dc6a14c56d5520bccecfff3eebaec6cee77c96e9748eb3684ae17 schemaVersion:1

## Plan Scope
- Redesign the S.I.R. pull-request, protected-main, scheduled, and Pages qualification topology without weakening any production workload, mutation, browser, accessibility, documentation, or cross-runtime assertion.
- Treat the current successful PR run `32523274261` as the comparison baseline: 1,518 seconds of GitHub job time, 351,337,554 transported artifact bytes, a 125.604-second observed critical path, and a 238.449-second verdict.
- Treat protected run `32525850376`, scheduled run `32548223694`, and Pages run `32525850336` as the P0 failure baseline: all eventually reject stale map-editor production-review evidence.
- Apply the conservative impact-classification lessons from FS-GG/.github commit `bcadfdd1` without importing producer-owned registry or coordination machinery into S.I.R.
- Deliver the design as six independently reversible milestones (M0-M5); implementation, evidence, verification, and shipping happen after this design reaches `implementationReady`.

## Architecture

The target topology has five explicit layers:

1. **Routing and integrity floor.** One fail-closed route classifies the exact diff. A small unconditional integrity floor always runs; optional integrity subjects may be omitted only by their own conservative classifiers.
2. **Clean producers.** Exact native, Fable, web, server, and documentation producers build once from the accepted head and emit content-addressed manifests. Producer artifacts are immutable inputs, never mutable shared workspaces.
3. **Isolated consumers.** Mutation, browser, performance, and other stateful subjects remain isolated. Measured read-only domains may share one runner while retaining separate named receipts.
4. **Deterministic joins.** PR and protected joins consume typed receipts and reject missing, mismatched, duplicate, stale, or unverified outputs. Partial receipts survive early or late failure for diagnosis.
5. **Read-only observation and deployment.** A post-run observer reconciles complete GitHub job/artifact cost. Pages consumes the exact successful protected-site artifact and performs no compilation.

Correctness receipts remain authoritative. Neither cache hits, artifact reuse, classifiers, nor telemetry may synthesize a passing product result.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-007] complete: M0 makes every source or tool input that affects map-editor production-review output part of the protected, scheduled, and Pages freshness decision. A route mutation that removes any such path must fail.
- PD-002 [AC-001] [FR-002] [DEC-007] complete: M0 regenerates production-review evidence from the accepted production bundle, validates the result with the existing mutation suite, and prevents a PR from reporting green when its merge would deterministically fail the protected freshness gate.
- PD-003 [AC-002] [FR-003] [DEC-001] complete: M1 replaces the opaque protected monolith with clean-room producer jobs and named consumer stages. Every stage binds repository, workflow, run attempt, head SHA, tree digest, workload identity, and producer-manifest digests.
- PD-004 [AC-002] [FR-004] [DEC-001] complete: M1 uploads each completed stage receipt with `always()` semantics and uses an unconditional join that reports the first failed stage plus all completed stage timings. Early, middle, and late fault injections prove partial diagnosis survives.
- PD-005 [AC-003] [FR-005] [DEC-007] complete: The existing four-minute PR verdict is a hard ceiling, not a best-effort target. Route and verdict clocks stay source-frozen and exact-head bound; any milestone that exceeds 240,000 ms is rolled back.
- PD-006 [AC-003] [FR-006] [DEC-006] complete: M2 establishes actual GitHub runner cost as the optimization denominator. The accepted final design must use at most 1,214 seconds (20 percent below 1,518 seconds) on a representative exact workload and must not move work outside the measured topology.
- PD-007 [AC-003] [FR-007] [DEC-003] complete: M2 inventories every upload/download by logical artifact, producer, consumer, compressed bytes, expanded bytes, and transfer time. M3 must transport at most 281,070,043 bytes (20 percent below 351,337,554) for the representative route.
- PD-008 [AC-004] [FR-008] [DEC-003] complete: M3 produces Linux-x64 acceleration closures for Linux-hosted consumers and stores duplicate files once by digest. The manifest retains relative path, mode, size, hash, runtime identifier, and logical owner; reconstruction occurs in a fresh directory and rejects missing, extra, corrupt, or non-executable files.
- PD-009 [AC-004] [FR-009] [DEC-001] [DEC-003] complete: Artifact consumers verify manifest and payload digests before use and have no source-build fallback. Portable RID/package coverage remains in protected cross-runtime proof, while hosted Linux jobs exercise reconstructed Wasmtime/runtime startup.
- PD-010 [AC-005] [FR-010] [DEC-004] complete: M4 may consolidate rules, spatial, cancellation, and cross-runtime read-only subjects into one `domain-conformance` job only after setup-to-test measurements show a benefit. It emits four independently addressable receipts and stops with the failing subject named; browser, mutation, producer, and documentation jobs remain separate.
- PD-011 [AC-005] [FR-011] [DEC-005] complete: M4 splits integrity into an unconditional floor and stable-id optional subjects. Unknown changes, malformed diffs, renames, deletions, workflow/topology/classifier changes, and classifier self-changes conservatively run the affected subject. Existing authoritative test executions are counted from TRX/JSON receipts rather than rerun for counting.
- PD-012 [AC-006] [FR-012] [DEC-002] complete: M5 builds and verifies the site once in the protected main workflow. A read-only `workflow_run` deployment accepts only a successful main-push run, downloads the exact qualified-site artifact, verifies trigger/run/attempt/head/tree and byte digests, then deploys with the minimum Pages permissions and no build tools.
- PD-013 [AC-007] [FR-013] [DEC-006] complete: M2 instruments checkout, action setup, restore, build, test, browser, mutation, packaging, upload, download, and post-job phases. In-run partial timing is diagnostic; it must never omit action or transport time from the final cost claim.
- PD-014 [AC-007] [FR-014] [DEC-006] complete: A read-only reusable/post-run observer queries GitHub Jobs and Artifacts APIs, binds workflow id/run id/attempt/head/tree, reconciles API duration and bytes with typed receipts, records explicit residual overhead, and publishes a versioned cost report. Pull requests bootstrap it after `pr-verdict`, excluding exactly the observer's own active job while rejecting every other incomplete timing; terminal default-branch runs use `workflow_run`. Missing or inconsistent observation blocks optimization acceptance, not the already-completed product verdict.
- PD-015 [AC-008] [FR-015] [DEC-007] [DEC-008] complete: M0-M5 land as separate reversible commits with focused inversions and exact-head hosted controls. Official actions move to supported Node-24-compatible releases pinned by full SHA plus version comment; jobs gain explicit timeouts and least permissions. A milestone rolls back on any workload/count/target loss, correctness failure, target metric regression over five percent, or ceiling breach.

## Contract Impact
- PC-001 [PD-001] [PD-002] route policy: version the CI route receipt so production-review freshness inputs and protected merge-equivalence are explicit, stable identifiers. Older receipts diagnose as unsupported rather than silently pass.
- PC-002 [PD-003] [PD-004] qualification receipts: define producer, stage, partial-diagnostic, and join schemas with exact run/head/tree/workload bindings and deterministic missing/duplicate/mismatch errors.
- PC-003 [PD-007] [PD-008] [PD-009] artifact manifest: define a versioned target-specific content manifest and content-addressed payload contract with path/mode/size/hash/RID/owner fields and clean-room reconstruction rules.
- PC-004 [PD-010] [PD-011] subject receipts: retain stable rules, spatial, cancellation, cross-runtime, and integrity subject identities even when their process topology changes; record executed/omitted and a typed reason.
- PC-005 [PD-012] qualified-site handoff: define the protected-to-Pages receipt, permitted triggering event, successful-run requirement, exact site digest, and deploy-only permissions boundary.
- PC-006 [PD-013] [PD-014] cost report: define partial and complete cost-report schemas covering product phases, action/setup/post phases, transfers, GitHub API observations, residuals, and reconciliation status.
- PC-007 [PD-015] workflow contract: retain required-check names or provide an explicit branch-protection migration mapping; pin every third-party action by full SHA and record its human-readable version.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PC-001] routeTest: mutate every production-review input, remove one route edge, and stale one accepted artifact; the route/freshness tests must turn red, while an unchanged exact bundle remains green.
- VO-002 [PD-003] [PD-004] [PC-002] faultInjection: fail one early producer, one middle consumer, and one late performance stage; each run must retain completed receipts, name the first failure, and make the deterministic join red.
- VO-003 [PD-005] [PD-006] hostedTiming: run the representative exact-head PR route with unchanged selected targets/counts and prove verdict at most 240,000 ms and summed GitHub job time at most 1,214 seconds.
- VO-004 [PD-007] [PD-008] [PD-009] [PC-003] artifactTest: inventory baseline and candidate bytes; mutate manifest hash/path/mode/RID, remove payload data, add an unexpected file, and start the reconstructed runtime. Accept only at most 281,070,043 transported bytes with portable proof unchanged.
- VO-005 [PD-010] [PC-004] topologyTest: compare pre/post subject names, target lists, assertion counts, and results; force each consolidated subject to fail independently and prove attribution plus downstream join failure.
- VO-006 [PD-011] [PC-004] classifierTest: cover relevant, unrelated, unknown, malformed, rename, deletion, topology, workflow, and classifier-self changes; remove a TRX result and falsify its count to prove non-vacuity fails closed.
- VO-007 [PD-012] [PC-005] deploymentTest: prove exact successful protected-main artifact deploys with no build invocation; wrong SHA/tree/run/attempt, failed run, PR run, missing artifact, corrupt bytes, and excessive permissions all fail.
- VO-008 [PD-013] [PD-014] [PC-006] reconciliationTest: compare receipts with GitHub Jobs/Artifacts APIs for a complete and a failed run; require action, post, upload, and download time/bytes plus explicit residual, and reject missing or inconsistent observations.
- VO-009 [PD-015] [PC-007] workflowStaticTest: validate YAML, stable required contexts, SHA-pinned supported actions, version comments, explicit timeouts, least permissions, concurrency, and dependency graph; mutate each contract to prove the checker turns red.
- VO-010 [PD-001] [PD-015] protectedControl: after every milestone, run focused local/unit/mutation proof and one exact-head hosted control; after M5, require green pull-request, protected-main, scheduled, Pages, documentation, browser, mutation, and cross-runtime receipts before implementation is accepted.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] expandAndCutOver: M0 expands freshness routing while preserving existing required contexts, then cuts over only after an exact-head merge-equivalence control is green.
- PM-002 [PC-002] parallelReceipts: M1 emits old and new protected verdict data during one observation window; the new join becomes authoritative only after equality and fault-injection proof.
- PM-003 [PC-003] producerFirst: M3 introduces manifests and reconstruction behind an opt-in producer version, compares reconstructed trees/runtime behavior, then removes duplicated transport only after parity.
- PM-004 [PC-004] stableIdentity: M4 changes process placement but preserves subject receipt identities and branch-protection meaning; reverting the consolidation restores jobs without changing evidence consumers.
- PM-005 [PC-005] deployCutOver: M5 publishes a qualified site artifact before enabling `workflow_run` deployment, verifies one shadow consumption, then removes the Pages rebuild path.
- PM-006 [PC-006] observeOnly: Cost observation is read-only and never gates product correctness; it gates only optimization claims until its reconciliation is trusted.
- PM-007 [PC-007] stagedModernization: Action/runtime/timeouts/permissions changes land independently of topology changes where possible, with SHA update rollback isolated from product scripts.

## Generated View Impact
- GV-001 [PD-001] workModel: `readiness/ci-reliability-efficiency/work-model.json` and generated agent guidance refresh from the authored SDD sources and must report currency.
- GV-002 [PD-014] costReports: versioned CI cost summaries and human-readable reports are generated from immutable receipt/API inputs; generators must be deterministic and never treated as authored evidence.
- GV-003 [PD-015] workflowDocumentation: CI qualification, production qualification, required-context, and Pages handoff documentation regenerate or update in lockstep with the executable workflow contracts.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- The current PR verdict has only 1.551 seconds of headroom under its 240-second ceiling; M0 correctness work must not wait for later optimization.
- Existing receipts undercount actual GitHub job time by about 117 seconds (7.7 percent), so no optimization claim may use typed script duration alone.
- Current prepared artifacts total 351,337,554 bytes and local output analysis finds roughly 324 MB of duplicate file instances; these identify M3 opportunity but do not authorize package or portability changes.
- Full qualification currently takes about fourteen minutes and fails stale review evidence only at the end; M1 must improve time-to-first-diagnosis before attempting broad consolidation.
- Shared compiled artifact reuse beyond immutable producer/consumer manifests remains rejected until trust boundaries, reconstruction, and measured benefit are proven.

## Milestone Exit Criteria

- **M0 — Green closure:** protected, scheduled, and Pages freshness paths are consistent; current production-review evidence passes; route and stale-artifact inversions pass.
- **M1 — Observable protected topology:** clean producers, named stages, partial receipts, deterministic join, and early/middle/late fault injection are green; first protected failure is reported within 300 seconds.
- **M2 — Complete accounting:** in-run phase data and post-run Jobs/Artifacts reconciliation agree within explicit residual accounting; the 1,518-second and 351,337,554-byte baselines are reproducible.
- **M3 — Payload reduction:** clean reconstruction and runtime/package parity are green; transported artifact bytes are at least 20 percent lower.
- **M4 — Scheduling reduction:** stable receipt identities and selected workloads/counts are unchanged; actual runner time is at least 20 percent lower and PR verdict remains at most 240 seconds.
- **M5 — Deployment/runtime modernization:** Pages deploys the protected artifact without build, actions are supported and SHA-pinned, permissions/timeouts are explicit, and PR/protected/scheduled/Pages controls are all green.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work ci-reliability-efficiency`.
