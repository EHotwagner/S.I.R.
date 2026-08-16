---
schemaVersion: 1
workId: 215-single-pass-qualification
title: Single Pass Qualification
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/215-single-pass-qualification/spec.md
sourceClarifications: work/215-single-pass-qualification/clarifications.md
sourceChecklist: work/215-single-pass-qualification/checklist.md
publicOrToolFacingImpact: true
---

# Single-Pass Production Qualification Plan

Prose status: planned

## Source Snapshot
- spec: work/215-single-pass-qualification/spec.md sha256:3c8737b6ff275cf7e3319963c9f1e71b72d176c7e9a89e29ea1635946f0f801a schemaVersion:1
- clarifications: work/215-single-pass-qualification/clarifications.md sha256:3ce348c6cec5931d9065aacaed8ad6524620a09852b0ba98759cbd8289df61e6 schemaVersion:1
- checklist: work/215-single-pass-qualification/checklist.md sha256:39a468ec1b1d1e144a739be1c2bcbeb1e29768227099279803a3c219fca451e2 schemaVersion:1

## Plan Scope
- Add one Node receipt producer/verifier shared by build, documentation, delivery, browser, mutation, timing, and feedback validation commands.
- Teach the existing client build/conformance and documentation scripts to produce or verify/reuse outputs while preserving standalone documentation behavior.
- Add a single production qualification driver that measures stages, builds once, runs the existing subjects, emits focused command receipts, and proves protected drift refusal.
- Extend feedback audit validation with a narrow focused-receipt evidence locator; do not make feedback report/audit content a product build input.
- Add script/Node/FSI regression tests and documentation for the schemas, reuse contract, standalone fallback, and lean execution sequence.

## Technical Context
Node 26 ESM plus strict Bash orchestrate Fable 5.13, Vite 8, .NET 10/fsdocs, Playwright, and existing JUnit/TRX outputs. Canonical JSON and SHA-256 already underpin client-loader and feedback audit receipts.

## Constitution Check
- II Structured Artifacts: receipt schemas, canonical ordering, and explicit producer/consumer commands are the machine contract.
- IV Simplicity: one small receipt module/CLI and explicit shell flags; no cache service or daemon.
- VI Test Evidence: focused schema/drift/reuse tests, real downstream subjects, and a self-restoring bound-output mutation.
- VIII Safe Failure: verification never rebuilds or refreshes a stale subject and names the mismatched revision/input/tool/output.

## Design
- `scripts/production-build-receipt.mjs` has `create`, `verify`, and `mutate-stale-reuse` modes. It derives a sorted manifest from Git identity/clean state, explicit tracked inputs, tool versions, owning command, and recursive expected output identities; creation writes canonical bytes under a digest-named receipt directory.
- `scripts/build-client.sh` remains the owner of the two Fable compilations and Vite bundle. The production qualification driver invokes it once, creates the receipt after outputs exist, and passes the receipt to downstream commands.
- `scripts/build-docs.sh --reuse-build-receipt <path>` verifies the receipt and skips only the solution/Fable/client rebuild already proven by it; standalone invocation retains the current full build path. It still runs every qualification and fsdocs/site verification step and adds the site identity to the final receipt chain.
- Delivery and browser commands read the same artifacts after receipt verification. Existing loader, budget, publication, diagnostics, and browser mutations remain unchanged; new tests assert their invocation/inventory.
- Focused command receipts are canonical JSON bound to exact revision/tree, declared command/inputs/tools/result/output identities. Feedback validation recognizes only a typed `receipt:` locator and verifies its committed bytes/digest/head/command/result instead of accepting an unexecuted command assertion.
- Timing evidence records the comparable baseline/candidate commands, host/tool facts, stage durations, output/subject inventory, duplicate target counts, and percentage reduction; timing itself is evidence, never a correctness threshold in normal CI.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: Canonically serialize receipt content, hash the exact LF-terminated bytes, and store it at a path containing that digest; creation refuses an existing non-identical path.
- PD-002 [AC-001] [FR-002] [DEC-002] complete: Derive source/tree/clean state, explicit build inputs including package/project/tool/lock configuration, and exact Git/.NET/Fable/Node/npm/Vite/fsdocs versions rather than trusting caller strings.
- PD-003 [AC-001] [FR-003] [DEC-002] complete: Recursively bind the two Fable output roots, client bundle, publication/feature manifests, and later site output using sorted relative paths, byte sizes, and SHA-256 digests.
- PD-004 [AC-002] [FR-004] [DEC-001] complete: Add an explicit docs reuse flag that verifies before skipping the nested solution/Fable/client build; no receipt means today's standalone build behavior.
- PD-005 [AC-002] [FR-005] complete: Drive existing delivery/browser checks unchanged against verified output and assert every retained normal/mutation subject appears in the focused/aggregate results.
- PD-006 [AC-003] [FR-006] [DEC-002] complete: Verification re-derives every identity and fails before consumption on revision, tree, dirty state, input/lock/tool/command/path/missing/output drift; verify mode never writes.
- PD-007 [AC-003] [FR-007] [DEC-005] complete: Mutate a temporary copy of a real bound output, require the production verifier's named output-drift red, and compare original bytes before/after in an unconditional restoration boundary.
- PD-008 [AC-004] [FR-008] [DEC-003] complete: Add a typed focused-receipt feedback locator and validator that resolves immutable committed receipt bytes at the report head and checks schema, content address, commit/tree, command, result, and evidence subject.
- PD-009 [AC-005] [FR-009] [DEC-004] complete: Measure old build-plus-docs and candidate single-pass routes on one host, preserve equivalent subject inventory, record raw stages and at least 20 percent reduction, and keep timing outside functional pass/fail gates.
- PD-010 [AC-006] [FR-010] [DEC-003] complete: Provide focused edit commands, one aggregate producer, metadata-only feedback validation, and unchanged final hosted CI; documentation and tests state the sequence explicitly.

## Contract Impact
- PC-001 [PD-001] [PD-002] [PD-003] buildReceipt: Document `sir.production-build-receipt/v1`, canonical identity fields, content-addressed path, producer ownership, and fail-closed verification.
- PC-002 [PD-004] docsReuse: Document `build-docs.sh --reuse-build-receipt <path>` as verified reuse only; no flag preserves standalone full build behavior.
- PC-003 [PD-008] focusedCommandReceipt: Document the focused receipt schema and `receipt:` feedback evidence locator, including exact-head/content-address validation and feedback-only exclusions.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-003] [PD-006] [PC-001] receiptTest: Exercise create/verify determinism and each stale dimension using temporary fixtures, proving verify-only operation writes nothing.
- VO-002 [PD-004] [PC-002] docsTest: Prove receipt-aware docs skips both Fable compiles while running every existing documentation/site subject; prove no-receipt standalone still owns the full build.
- VO-003 [PD-005] deliveryBrowserTest: Run existing client-loader, production-delivery, browser-diagnostics, production browser, and protected mutations against the verified client output.
- VO-004 [PD-007] mutationTest: Run the real self-restoring output-drift mutation, require its subject-specific red, and compare original receipt/output/tracked bytes after restoration.
- VO-005 [PD-008] [PC-003] feedbackTest: Validate a correct focused receipt at its report head and reject stale head, digest, command, result, subject, untracked, and malformed variants.
- VO-006 [PD-009] performanceTest: Capture comparable baseline/candidate wall times and subject inventories on the same host and compute the declared reduction.
- VO-007 [PD-010] aggregateTest: From a source-frozen clean tree run one aggregate, inspect its build count/receipt/result inventory, then make feedback-only metadata changes and prove no product build is requested.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additive: Receipt schema v1 and digest path are additive; absent/unsupported/malformed receipts fail reuse without affecting standalone builds.
- PM-002 [PC-002] compatible: Documentation's default invocation remains a complete standalone build; only explicit verified reuse skips duplicate work.
- PM-003 [PC-003] compatible: Existing `command:` and `file:` feedback locators retain their current semantics; `receipt:` adds a stronger focused proof path.

## Generated View Impact
- GV-001 [PD-001] [PD-003] buildReceipt: Immutable receipt files are generated from exact inputs/outputs and never refreshed in place; stale identity produces refusal and a new valid build produces a new digest path.
- GV-002 [PD-008] feedbackAudit: Feedback audit validation resolves receipt bytes from the report commit and reports missing/stale/contradictory evidence instead of rerunning the aggregate.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Hosted CI documentation remains independently buildable because jobs do not share a filesystem; single-pass reuse governs the local/review aggregate and any same-workspace qualification route.

## Tests
- Focused during edits: receipt unit fixture, docs reuse command fixture, feedback validator unit/FSI suite, and existing affected Node checks.
- Source freeze: one clean production aggregate producing the immutable receipt, docs/site, delivery/browser results, protected mutation result, timing receipt, and machine-readable aggregate report.
- After aggregate: feedback report/audit/checkpoint metadata and SDD views only; no source/build input edit and no second aggregate.
- Hosted: one final CI execution on the reviewed exact SHA.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 215-single-pass-qualification`.
