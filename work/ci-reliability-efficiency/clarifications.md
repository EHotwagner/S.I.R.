---
schemaVersion: 1
workId: ci-reliability-efficiency
title: Reliable and Efficient CI Qualification
stage: clarify
changeTier: tier1
status: clarified
sourceSpec: work/ci-reliability-efficiency/spec.md
publicOrToolFacingImpact: true
---

# Reliable and Efficient CI Qualification Clarifications

## Source Specification
- work/ci-reliability-efficiency/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001] [FR-003] [FR-004]: Which protected subjects may share preparation or runners without weakening isolation or attribution?
- CQ-002 [AMB:AMB-002] [FR-012]: Does Pages rebuild, share the protected workflow, or consume its artifact across workflows?
- CQ-003 [AMB:AMB-003] [FR-007] [FR-008] [FR-009]: How are native/server artifacts reduced without breaking Wasmtime probing or cross-runtime coverage?
- CQ-004 [AMB:AMB-004] [FR-010]: Which jobs are setup-dominated enough to consolidate without lengthening the critical path?
- CQ-005 [AMB:AMB-005] [FR-011]: Which checks may use conservative impact classification?
- CQ-006 [AMB:AMB-006] [FR-013] [FR-014]: Where does authoritative end-to-end cost telemetry run?
- CQ-007 [AMB:AMB-007] [FR-005] [FR-006] [FR-007] [FR-015]: What rollout order and rollback threshold preserve causal evidence?
- CQ-008 [AMB:AMB-008] [FR-015]: How are action runtime upgrades selected and pinned?

## Answers
- CQ-001: Build clean-room native, Fable, web, server, and docs prerequisites once under exact receipts, then fan out isolated mutation/browser/performance consumers. Low-cost read-only domain subjects may share one runner only if each still emits its own receipt and no consumer mutates the shared staged copy. Source mutations remain separate workspaces/jobs.
- CQ-002: Protected main qualification owns the one site build and uploads a qualified-site receipt. A minimal `workflow_run` Pages workflow with only read plus Pages deployment permissions accepts successful `main` push runs, verifies triggering workflow/run/head/receipt/bytes, and deploys without compilation. PR-originated, failed, missing, or SHA-mismatched artifacts are rejected.
- CQ-003: PR artifacts are Linux-runner acceleration, not distributable packages. Produce a Linux-x64 runtime closure, omit other RID native assets, and optionally use a content-addressed file store plus reconstruction manifest for repeated managed files. Preserve all RID/package tests in protected cross-runtime subjects. Prove runtime startup and exact file/mode/hash restoration before accepting size evidence.
- CQ-004: Consolidate only after measuring setup:test ratio. The first candidate is a `domain-conformance` runner consuming native/Fable/web artifacts and emitting separate rules, spatial, cancellation, and cross-runtime receipts; its subjects may execute in deterministic order because their combined measured test time is below duplicated setup saved. Browser shards, producers, source mutations, and docs stay independently scheduled.
- CQ-005: The always-on integrity floor remains small and unconditional. CI-contract mutations, npm audit controls, governance/package-boundary controls, dependency-surface checks, historical byte-stability, and feedback-audit controls receive separate conservative classifiers tied to their real subjects. Unknown, malformed, rename, deletion, topology, classifier, or workflow changes run the check. Production behavior tests and scheduled full-route subjects are never omitted by these classifiers.
- CQ-006: Product gate receipts remain verdict authority. A read-only observer queries GitHub Jobs and Artifacts after `pr-verdict` through a same-run reusable call for pull-request bootstrap and after completion through `workflow_run` on the default branch. The bootstrap excludes exactly its own named active job, rejects every other incomplete timing, binds workflow id/run attempt/head/tree, and emits the complete product-topology cost report. It cannot change the completed verdict or use elevated write permissions; missing observer data blocks optimization evidence, not product correctness.
- CQ-007: Land independently reversible milestones: M0 freshness and green closure; M1 protected decomposition/checkpointing; M2 complete telemetry; M3 Linux/deduplicated payloads; M4 integrity classification and measured job consolidation; M5 exact-site Pages handoff and action modernization. Each milestone must pass focused inversions plus one exact-head hosted control and must roll back if selected subjects/counts shrink, PR time exceeds 240 seconds, first protected failure exceeds 300 seconds, or its targeted cost metric regresses by more than five percent.
- CQ-008: Use supported Node-24-compatible official action releases, pin their full commit SHA with a version comment, retain explicit Node 26.5.0 for product commands, and let dependency automation propose reviewed SHA updates. Preserve least permissions and add explicit job timeouts before changing action versions.

## Decisions
- DEC-001 [CQ-001] [AMB:AMB-001] [FR-003] [FR-004]: Decompose protected/scheduled qualification into one exact clean-room producer layer, isolated mutation/browser/performance consumers, separately receipted read-only subjects, and an unconditional deterministic join; never share a mutable working copy across subjects.
- DEC-002 [CQ-002] [AMB:AMB-002] [FR-012]: Build the documentation site once in successful protected main qualification and deploy that exact verified receipt from a restricted `workflow_run` Pages workflow; Pages performs no build.
- DEC-003 [CQ-003] [AMB:AMB-003] [FR-007] [FR-008] [FR-009]: Make PR transport target-specific: retain Linux-x64 runtime bytes and one content-addressed copy of duplicate closure files, reconstruct expected trees from a signed-by-digest manifest, and keep portable package/cross-runtime proof outside that acceleration boundary.
- DEC-004 [CQ-004] [AMB:AMB-004] [FR-010]: Use measured setup:test ratios to consolidate rules, spatial, cancellation, and cross-runtime into a candidate `domain-conformance` runner with four independent receipts; retain separate producers, mutation jobs, browser shards, documentation, and evidence gates.
- DEC-005 [CQ-005] [AMB:AMB-005] [FR-011]: Split integrity into an unconditional floor plus stable-id conditional subreceipts. Every classifier defaults to run, models both sides of renames, and is mutation-tested against a false omission.
- DEC-006 [CQ-006] [AMB:AMB-006] [FR-013] [FR-014]: Add one read-only reusable/post-run cost observer for complete GitHub job/artifact facts. Pull requests call it after the in-run verdict and exclude only its own active job so new observer logic is qualifiable before merge; `workflow_run` observes terminal default-branch runs. Gate/join receipts remain authoritative for correctness and clearly labeled as partial for post-step cost.
- DEC-007 [CQ-007] [AMB:AMB-007] [FR-005] [FR-006] [FR-007] [FR-015]: Implement M0 through M5 in order with separate commits, inversions, exact-head hosted receipts, five-percent local rollback guards, and unchanged global evidence/workload budgets.
- DEC-008 [CQ-008] [AMB:AMB-008] [FR-015]: Pin supported official actions by immutable SHA, retain explicit product runtime versions and least permissions, add timeouts, and reject required-context or permission drift statically.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
- None. AMB-001 through AMB-008 are resolved by DEC-001 through DEC-008.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work ci-reliability-efficiency`.
