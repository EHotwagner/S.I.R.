---
schemaVersion: 1
workId: 220-bounded-pr-ci
title: Bounded PR CI
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/220-bounded-pr-ci/spec.md
sourceClarifications: work/220-bounded-pr-ci/clarifications.md
sourceChecklist: work/220-bounded-pr-ci/checklist.md
publicOrToolFacingImpact: true
---

# Bounded PR CI Plan

Prose status: planned

## Source Snapshot
- spec: work/220-bounded-pr-ci/spec.md sha256:126808eaa7785b9d2ce0a5e958d06def32ffb98358c1a42ab57f53caa231dd96 schemaVersion:1
- clarifications: work/220-bounded-pr-ci/clarifications.md sha256:b3525c9c3afa13a5f28599546e0455f5dc1fcf9157277f68e733970d5ce680d2 schemaVersion:1
- checklist: work/220-bounded-pr-ci/checklist.md sha256:0b89c050b3209b15e15fd44c3b1aeb3fb913a561a3e09cb42a4ef516ad3678c3 schemaVersion:1

## Plan Scope
- Add a small Node route/receipt/join module with committed policy and timing schemas; keep routing pure and independently testable.
- Add a focused PR qualification driver and split workflow topology into route, integrity, narrow producer jobs, named parallel gates, and an unconditional deterministic join.
- Reuse the existing production build receipt for prepared outputs and keep `qualify-production.sh` as the complete clean-room protected/scheduled route.
- Add path, receipt, inventory, cache, join, and scheduled-hidden-defect mutations, plus route-matrix and workflow-contract tests.
- Document PR, protected ship, scheduled, and local routes and record comparable hosted timing without changing gameplay performance assertions.

## Technical Context
GitHub Actions `ubuntu-latest`, Node 26 ESM, strict Bash, .NET 10/Fable 5.13, npm/Vite/Playwright, fsdocs, canonical JSON, SHA-256 production receipts, TRX/JUnit results, and the existing single-pass qualification driver.

## Constitution Check
- II Structured Artifacts: route policy, gate results, artifact manifests, timing, and join verdicts are versioned machine contracts.
- IV Simplicity: pure Node classification/join code plus explicit workflow jobs; no service, daemon, or hidden cache authority.
- VI Test Evidence: route matrix, fail-closed receipt/inventory/join mutations, workflow assertions, and one source-frozen aggregate.
- VIII Safe Failure: unknown paths and unreadable/missing/stale evidence select conservative failure or cross-cutting work with named diagnostics.

## Design
- `scripts/ci-route.mjs` normalizes changed paths, evaluates a versioned ordered policy, emits JSON outputs for stable gate ids, writes canonical route/timing/join receipts, and exposes self-restoring mutation fixtures. A mixed or unmatched path set is cross-cutting.
- `scripts/qualify-pr.sh` is the local focused-lane owner. It captures changed paths, starts one timing receipt, runs the integrity floor, prepares only the selected prerequisites, runs selected gates in stable concurrent groups, then invokes the same deterministic join as hosted CI.
- `.github/workflows/ci.yml` separates PR and full routes. PR jobs are `route`, `integrity`, independent native/Fable/web/server/docs producers, named rules/spatial/cancellation/cross-runtime/browser/documentation/evidence gates, and `pr-verdict`; each gate depends only on its relevant producers. Main pushes and schedules run `qualify-production.sh` as a clean-room job.
- Each producer uses lock-keyed caches only as transport acceleration, builds its selected prerequisite once, creates/verifies a content-addressed production receipt and route artifact manifest, and uploads only its immutable declared outputs. Consumers download and verify only their required producer artifacts; mutable `obj` intermediates and unrelated `bin` trees are never sealed, and missing or drifted artifacts never trigger an implicit rebuild.
- Every job emits a small gate-result JSON with queue/setup/restore/build/test/total durations, cache/reuse and invocation counts, route fact, retry/failure stage, candidate commit/tree, command, and output identities. The join orders results by policy, validates all required/skip states, calculates critical path and runner-minutes, rejects representative cross-cutting routes above the 240,000 ms acceptance target, and retains the unchanged 300,000 ms outer budget diagnostic.
- Protected and scheduled full qualification keep the current clean-cache receipt/mutation/browser/docs/performance/SDD/Governance/historical subjects. A scheduled mutation proves a cross-surface defect omitted from an otherwise correct focused route is caught at the full boundary.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: Define one ordered route-policy object and pure changed-path classifier for documentation, domain, browser, and cross-cutting classes; normalize separators and reject empty/malformed path facts.
- PD-002 [AC-001] [FR-002] [DEC-001] complete: Emit selected gates and `skipped` entries with exact matching rule ids; mixed or unknown paths carry a conservative rule id and select every required cross-cutting gate.
- PD-003 [AC-002] [FR-003] [DEC-005] in-progress: Keep the 300,000 ms enforced PR boundary, require the final representative cross-cutting acceptance run to complete within 240,000 ms, consolidate native preparation into one Release solution graph whose source-bound outputs all downstream gates consume without configuration-duplicate builds, overlap the independent prepared spatial mutation matrix with final native/Fable conformance after their common verified inputs are available, and balance the production browser inventory at test granularity across isolated single-worker Playwright shards. Browser capacity reserves one fixed runner lane for the server cohort and scales browser shards over the remaining lanes as `max(1, availableParallelism - 1)`; this avoids both raw-CPU overcommit and the measured under-utilization of charging a full CPU to every mostly-waiting server. Each shard still owns an independent server, port, browser worker, and JUnit fragment. Every fragment must be well-formed, non-empty, internally count-consistent JUnit before deterministic merge; unreadable, malformed, empty, or count-drifted fragments fail closed. Local runs preserve a one-shard default and an explicit capacity-bounded override. Round-one focused implementation and inversions are complete; exact-head hosted acceptance remains outstanding.
- PD-004 [AC-002] [FR-004] [DEC-002] complete: Place integrity before prepare/expensive nodes and express every independently attributable gate as a named DAG node rather than serial workflow steps.
- PD-005 [AC-003] [FR-005] [DEC-003] complete: Build selected prerequisites in independent producer nodes, publish narrow content-addressed manifests and immutable outputs, and make each downstream node consume only its required producers without fallback rebuilds or a global join dependency.
- PD-006 [AC-003] [FR-006] [DEC-003] complete: Reuse the production receipt verifier and exact-once Fable inventory; add route artifact verification for candidate/config/locks/tools/command/paths/outputs and cache-key identity.
- PD-007 [AC-004] [FR-007] [DEC-004] complete: Emit stable per-gate result files and let an `always()` join accept only declared skips and passing required gates in policy order; mutation-test missing, cancelled, failed, duplicate, and unknown results.
- PD-008 [AC-005] [FR-008] [DEC-006] complete: Preserve `qualify-production.sh` as the complete clean-room protected-main route and inventory every existing subject in its retained-subject receipt.
- PD-009 [AC-005] [FR-009] [DEC-006] complete: Run the same full command on schedule and add a self-restoring hidden-cross-surface mutation whose focused route legitimately skips the affected gate but full qualification rejects it.
- PD-010 [AC-006] [FR-010] [DEC-003] complete: Version canonical route, artifact, gate, timing, and join receipts, bind candidate source/tree and current production receipt identities, and reject non-canonical or stale bytes.
- PD-011 [AC-006] [FR-011] [DEC-005] complete: Record phase/gate timing, cache/reuse/invocation/route/skip/retry/failure facts, compute critical path and runner-minutes, and label all values as runner feedback rather than product performance.
- PD-012 [AC-007] [FR-012] [DEC-002] [DEC-004] complete: Provide deterministic self-restoring mutations for route, receipt, cache, inventory, join, and authority facts and require each to fail for its intended named diagnostic.

## Contract Impact
- PC-001 [PD-001] [PD-002] routePolicy: Add `sir.ci-route/v1` with normalized paths, class, selected gates, skipped gates, rule ids, and conservative fallback.
- PC-002 [PD-005] [PD-006] artifactManifest: Add `sir.ci-artifact-manifest/v1` chained to `sir.production-build-receipt/v1`, exact candidate/locks/tools/command/outputs, and explicit clean-room exceptions.
- PC-003 [PD-003] [PD-007] [PD-011] verdictReceipts: Add `sir.ci-gate-result/v1`, `sir.ci-timing/v1`, and `sir.ci-join/v1` with stable gate order and fail-closed state vocabulary.
- PC-004 [PD-008] [PD-009] workflowRoutes: Document `pull_request` focused, protected `push` full, `schedule` full, and local focused/full commands with no ambiguous default.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PC-001] routeTest: Exercise documentation-only, domain-only, browser-only, mixed/cross-cutting, normalized, empty, malformed, and unknown path fixtures and inspect exact explanations.
- VO-002 [PD-003] [PD-011] [PC-003] budgetTest: Test the 300,000 ms enforced boundary and 240,000 ms representative acceptance target, phase sums, critical path, runner-minutes, cache/reuse/invocation/retry/failure fields, and separation from product performance.
- VO-003 [PD-004] [PD-007] [PC-003] dagTest: Parse workflow topology and mutation-test deterministic join success, skip, failure, cancellation, missing, duplicate, and unknown states.
- VO-004 [PD-005] [PD-006] [PC-002] reuseTest: Create and verify one prepared artifact chain, prove no downstream fallback rebuild, retain exact-once Fable inventory, and reject candidate/input/lock/tool/command/output/cache drift.
- VO-005 [PD-008] [PD-009] [PC-004] fullRouteTest: Compare retained full-subject inventory before/after and prove the scheduled hidden-cross-surface mutation is omitted by valid focused routing but rejected by full qualification.
- VO-006 [PD-012] mutationTest: Run every self-restoring mutation through production validators, require its named diagnostic, and compare changed fixture bytes after restoration.
- VO-007 [PD-003] [PD-004] hostedTest: Capture one final hosted run on the final head and verify the representative cross-cutting route is fully green within 240,000 ms, leaving at least 60,000 ms below the enforced boundary, with separately attributed results.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additive: Route schema v1 is new; unknown schema versions fail closed and unknown paths conservatively run cross-cutting gates.
- PM-002 [PC-002] compatible: Production build receipt v1 remains authoritative; the route manifest chains to it instead of replacing or weakening it.
- PM-003 [PC-003] additive: Gate/timing/join receipts are new generated outputs and do not reinterpret existing test, TRX, JUnit, performance, or SDD evidence.
- PM-004 [PC-004] topology: Required check names migrate to the stable join plus separately visible gate nodes; protected and scheduled full qualification remain complete.

## Generated View Impact
- GV-001 [PD-001] [PD-010] routeReceipt: Canonical route receipts are regenerated from changed paths and policy; stale candidate/policy identity is rejected.
- GV-002 [PD-005] [PD-010] artifactReceipt: Immutable artifact manifests are content-addressed and never refreshed in place; drift requires a new preparation.
- GV-003 [PD-007] [PD-011] joinReceipt: The join view is derived from exact route/gate receipts and reports missing/stale/malformed inputs instead of inferring success.
- GV-004 [PD-008] [PD-009] fullQualification: Existing production timing/build/site/conformance receipts and retained-subject inventory stay authoritative at protected/scheduled boundaries.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- GitHub queue delay availability varies; record it when available and keep the enforced 300-second interval rooted at runner start as specified.
- Cross-job transfer acceleration is not evidence authority: only content-addressed verified outputs count.
- The first hosted candidate completed every functional gate but joined at 299,640 ms. Native preparation improved to 101,454 ms total with one 69,538 ms solution build; the critical path moved to the 103,800 ms web producer followed by the 150,811 ms browser consumer, whose 118,756 ms test phase still serialized 41 independent tests on a two-CPU runner. These are architecture measurements, not substitutes for VO-007 hosted acceptance.
- A focused two-worker run completed 38 of 41 tests in 33.2 seconds but exposed two live-session collisions because both workers shared one server. The accepted repair therefore isolates each single-worker shard behind its own server instead of permitting concurrent contexts against shared live-session state.
- Independent review found that the first head applied the 240,000 ms reserve target to every route classification. The join must apply that stricter target only to representative cross-cutting work; documentation, domain, and browser routes retain the unchanged 300,000 ms outer contract with zero additional reserve requirement.
- The repaired focused browser route passed all 41 tests across two isolated shards in 33.361 seconds and produced one 41-test, zero-failure deterministic JUnit receipt.
- The repaired hosted browser gate passed functionally at 108,306 ms total / 81,587 ms test, but the representative route still joined at 276,614 ms. The arbitrary two-shard ceiling left 36,614 ms of acceptance work unresolved even though the runner reports more parallel capacity; shard count must therefore be capability-derived rather than a fixed project-size limit.
- The exhausted capacity-derived attempt mapped four test files to four raw-CPU shards as 18/3/20/0 tests, starved the production-delivery server until its 30-second assertion timed out, and reached 286,171 ms when the browser gate did pass. The remaining cause is twofold: Playwright's default file-level sharding cannot balance the growing inventory, and raw CPU count is not a safe shard count when every shard schedules both a browser and a server. The repair phase therefore enables test-level sharding while retaining `workers: 1` per isolated server, and derives shard capacity from two independently scheduled processes per shard.
- The repair-phase focused run assigned the complete inventory 21/20 across two isolated shards and completed in 34 seconds with 41 tests, zero failures, one intentional skip, and the production-delivery throttling assertion passing. Setting `fullyParallel` back to false and changing the per-shard process cost from two to one each independently made the focused route contract test fail, so both balancing and isolation capacity are gate-owned properties.
- The first repair-phase hosted head passed every substantive gate but joined at 283,553 ms. The two-shard browser remained 123,440 ms total / 97,654 ms test, while spatial became the exact critical gate at 132,219 ms because its isolated mutation matrix and final conformance still execute serially. The measured next repair therefore reserves one runner lane for the server cohort and uses the other three lanes for balanced browser shards on the four-way runner, while spatial overlaps its independent mutation and conformance subjects after shared inputs are verified. The same critic also proved a malformed existing shard fragment is silently interpreted as an empty shard; strict fragment structure/count validation is blocking, not optional hardening.
- Round-one focused evidence distributed the full browser inventory 14/14/13 and passed all 41 tests in 27 seconds with the throttled production-delivery assertion intact. The isolated nine-mutant spatial matrix passed in 22 seconds. Capacity-reserve removal, malformed-JUnit acceptance, and spatial-overlap removal each made the focused route contract red; the restored head is green.

## Tests
- Focused while editing: route/join Node tests, workflow topology test, receipt/inventory mutation tests, and shell syntax checks.
- Source freeze: one local aggregate exercising the complete production route and its immutable receipt chain.
- Metadata seal: feedback and lifecycle views only, with focused receipt validation and no product rebuild.
- Hosted: exactly one final workflow run on the final head; diagnose any failure before a retry.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 220-bounded-pr-ci`.
