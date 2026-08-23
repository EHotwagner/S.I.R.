import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { baselines, summarize, verdictBasisField, verdictBasisSchema, verdictObservedField } from "./ci-cost-report.mjs";

const head = "a".repeat(40);
const run = {
  id: 42,
  run_attempt: 2,
  event: "pull_request",
  head_sha: head,
  head_branch: "candidate",
  conclusion: "success",
  created_at: "2026-08-22T00:00:00Z",
  updated_at: "2026-08-22T00:00:10Z",
  repository: { full_name: "EHotwagner/S.I.R." },
};
const jobs = { jobs: [{ id: 1, name: "gate", status: "completed", conclusion: "success", started_at: "2026-08-22T00:00:01Z", completed_at: "2026-08-22T00:00:06Z", steps: [{ name: "Set up job", conclusion: "success", started_at: "2026-08-22T00:00:01Z", completed_at: "2026-08-22T00:00:02Z" }] }, { id: 2, name: "join", status: "completed", conclusion: "success", started_at: "2026-08-22T00:00:06Z", completed_at: "2026-08-22T00:00:09Z", steps: [] }] };
const artifacts = { artifacts: [{ id: 7, name: "gate", size_in_bytes: 200, created_at: "2026-08-22T00:00:06Z", expires_at: "2026-08-23T00:00:00Z", expired: false }, { id: 8, name: "expired", size_in_bytes: 900, expired: true }] };
const receipts = [
  { path: "gate.json", sha256: "b".repeat(64), value: { schema: "sir.ci-gate-result/v1", gate: "integrity", status: "pass", source: { commit: head, tree: "c".repeat(40) }, timingMilliseconds: { total: 4_000 } } },
  { path: "join.json", sha256: "d".repeat(64), value: { schema: "sir.ci-join/v1", result: "pass", source: { commit: head, tree: "c".repeat(40) }, timing: { criticalPathMilliseconds: 8_000, acceptanceTargetMilliseconds: 240_000, budgetMilliseconds: 300_000 } } },
];

const report = summarize(run, jobs, artifacts, receipts);
assert.equal(report.schema, "sir.ci-cost-report/v1");
assert.equal(report.result, "complete");
assert.equal(report.workflowWallMilliseconds, 10_000);
assert.deepEqual(report.totals, { jobs: 2, skippedJobs: 0, apiRunnerMilliseconds: 8_000, receiptRunnerMilliseconds: 4_000, unreceiptedOrActionMilliseconds: 4_000, artifacts: 1, artifactBytes: 200 });
assert.equal(report.jobs[0].steps[0].durationMilliseconds, 1_000);
assert.deepEqual(report.exclusions, []);
assert.equal(report.baselineComparison.result, "pass");
assert.deepEqual(report.baselineComparison.observed, { runnerMilliseconds: 8_000, artifactBytes: 200, verdictMilliseconds: 8_000 });

const activeObserver = { id: 3, name: "cost-observer / observe", status: "in_progress", conclusion: null, started_at: "2026-08-22T00:00:09Z", completed_at: null, steps: [] };
const sameRunReport = summarize(run, { jobs: [...jobs.jobs, activeObserver] }, artifacts, receipts, { activeObserverJobName: "cost-observer / observe" });
assert.equal(sameRunReport.result, "complete");
assert.deepEqual(sameRunReport.exclusions, [{ id: 3, name: "cost-observer / observe", status: "in_progress", reason: "current reusable observer cannot complete before observing its own run" }]);
assert.equal(sameRunReport.totals.jobs, 2);
assert.throws(() => summarize(run, { jobs: [...jobs.jobs, activeObserver] }, artifacts, receipts), /incomplete job timing inventory/u);
assert.throws(() => summarize(run, jobs, artifacts, receipts, { activeObserverJobName: "cost-observer \/ observe" }), /active observer identity mismatch/u);
assert.throws(() => summarize(run, { jobs: [...jobs.jobs, activeObserver, { ...activeObserver, id: 4 }] }, artifacts, receipts, { activeObserverJobName: "cost-observer / observe" }), /active observer identity mismatch/u);

const skipped = { id: 5, name: "protected-preflight", status: "completed", conclusion: "skipped", started_at: null, completed_at: null, steps: [] };
const skippedReport = summarize(run, { jobs: [...jobs.jobs, skipped] }, artifacts, receipts);
assert.equal(skippedReport.totals.jobs, 2);
assert.equal(skippedReport.totals.skippedJobs, 1);
assert.deepEqual(skippedReport.exclusions, [{ id: 5, name: "protected-preflight", status: "completed", reason: "completed skipped job allocated no runner" }]);
assert.throws(() => summarize(run, { jobs: [...jobs.jobs, { ...skipped, status: "queued" }] }, artifacts, receipts), /incomplete skipped job inventory/u);

const mismatched = structuredClone(receipts);
mismatched[0].value.source.commit = "e".repeat(40);
assert.equal(summarize(run, jobs, artifacts, mismatched).result, "incomplete");
assert.equal(summarize(run, jobs, artifacts, receipts.slice(0, 1)).reconciliation.expectedReceiptShape, false);
assert.throws(() => summarize(run, { jobs: [] }, artifacts, receipts), /incomplete job timing inventory/u);
assert.throws(() => summarize(run, jobs, { artifacts: [{ id: 1, name: "bad", size_in_bytes: -1 }] }, receipts), /invalid artifact byte inventory/u);
const overRunnerBudget = structuredClone(jobs);
overRunnerBudget.jobs[0].completed_at = "2026-08-22T01:00:00Z";
assert.equal(summarize(run, overRunnerBudget, artifacts, receipts).baselineComparison.result, "fail");

// --- S.I.R.#295: the verdict basis is READ from the run, never restated here --------------------

// No verdict constant survives in the emitted contract. Reintroducing one reds this immediately.
assert.deepEqual(Object.keys(baselines).sort(), ["artifactBytes", "runnerMilliseconds"]);
assert.deepEqual(Object.keys(report.baselineComparison.baselines).sort(), ["artifactBytes", "runnerMilliseconds"]);
assert.deepEqual(Object.keys(report.baselineComparison.targets).sort(), ["artifactBytes", "runnerMilliseconds"]);

// The report declares WHERE it read the basis, and that declaration is pinned by the qualification
// contract -- the same fixture `test-ci-route.mjs` pins the enforced constants against.
const contracts = JSON.parse(readFileSync(new URL("../tests/fixtures/ci-qualification/v1/contracts.json", import.meta.url), "utf8"));
assert.equal(verdictBasisSchema, contracts.costReportVerdictBasisSchema);
assert.equal(verdictObservedField, contracts.costReportVerdictObservedField);
assert.equal(verdictBasisField, contracts.costReportVerdictBasisField);
assert.deepEqual(Object.keys(baselines).sort(), [...contracts.costReportVerdictBaselineKeys].sort());
assert.equal(report.baselineComparison.verdict.schema, verdictBasisSchema);
assert.equal(report.baselineComparison.verdict.basisField, verdictBasisField);
assert.equal(report.baselineComparison.verdict.result, "pass");
assert.deepEqual(report.baselineComparison.verdict.receipts, [
  { path: "join.json", observedMilliseconds: 8_000, appliedTargetMilliseconds: 240_000, appliedBudgetMilliseconds: 300_000, within: true },
]);

const withVerdict = (criticalPathMilliseconds, timing = {}) => [receipts[0], {
  ...receipts[1],
  value: { ...receipts[1].value, timing: { criticalPathMilliseconds, acceptanceTargetMilliseconds: 240_000, budgetMilliseconds: 300_000, ...timing } },
}];
const graded = (criticalPathMilliseconds, timing) => summarize(run, jobs, artifacts, withVerdict(criticalPathMilliseconds, timing));

// THE INVERSION-PROOF CHECK. One identical measurement, two different APPLIED bases, opposite
// grades. No hardcoded basis can satisfy both halves: whichever constant is restated, one of these
// two assertions fails. This is what makes "the two cannot diverge" falsifiable rather than a claim.
assert.equal(graded(259_393, { acceptanceTargetMilliseconds: 312_000, budgetMilliseconds: 390_000 }).baselineComparison.verdict.result, "pass");
assert.equal(graded(259_393, { acceptanceTargetMilliseconds: 240_000, budgetMilliseconds: 300_000 }).baselineComparison.verdict.result, "fail");

// The exact false `fail` measured on the 90-day record: run 32629941705 at head 9ef7bd7e, a
// `cross-cutting` route whose attributable critical path was 259393 ms against an APPLIED
// acceptance target of 312000 ms. `pr-verdict` recorded `pass`; the cost report recorded `fail`.
const measured = graded(259_393, { acceptanceTargetMilliseconds: 312_000, budgetMilliseconds: 390_000 });
assert.equal(measured.baselineComparison.result, "pass");
assert.equal(measured.baselineComparison.observed.verdictMilliseconds, 259_393);
assert.equal(measured.result, "complete");

// Each join is graded against ITS OWN applied basis, never against one global figure.
const mixed = summarize(run, jobs, artifacts, [receipts[0],
  { path: "join-a.json", sha256: "d".repeat(64), value: { schema: "sir.ci-join/v1", result: "pass", source: { commit: head }, timing: { criticalPathMilliseconds: 259_393, acceptanceTargetMilliseconds: 312_000, budgetMilliseconds: 390_000 } } },
  { path: "join-b.json", sha256: "e".repeat(64), value: { schema: "sir.ci-join/v1", result: "pass", source: { commit: head }, timing: { criticalPathMilliseconds: 259_393, acceptanceTargetMilliseconds: 240_000, budgetMilliseconds: 300_000 } } },
]);
assert.deepEqual(mixed.baselineComparison.verdict.receipts.map(({ within }) => within), [true, false]);
assert.equal(mixed.baselineComparison.verdict.result, "fail");

// A basis this code cannot evaluate is REFUSED, and the refusal reaches the report's only
// exit-code path. The implementation enumerates none of these shapes -- it requires a positive safe
// integer -- so a shape nobody listed here refuses on the same rule rather than being graded.
for (const acceptanceTargetMilliseconds of [undefined, null, 0, -1, 312_000.5, "312000", Number.NaN, Number.POSITIVE_INFINITY, Number.MAX_SAFE_INTEGER + 2, {}, [], true]) {
  const refused = graded(259_393, { acceptanceTargetMilliseconds });
  const shape = `${typeof acceptanceTargetMilliseconds}:${String(acceptanceTargetMilliseconds)}`;
  assert.equal(refused.baselineComparison.verdict.result, "unevaluable", `basis ${shape} must refuse`);
  assert.equal(refused.baselineComparison.verdict.receipts[0].within, null, `basis ${shape} must not read as within`);
  assert.equal(refused.baselineComparison.result, "unevaluable", `basis ${shape} must not produce a grade`);
  assert.equal(refused.reconciliation.unevaluableVerdictCount, 1, `basis ${shape} must be counted`);
  assert.equal(refused.result, "incomplete", `basis ${shape} must reach the exit path`);
}

// The measurement side refuses on the same rule, so a missing critical path cannot pass by being
// compared as `null <= target`.
for (const criticalPathMilliseconds of [undefined, null, 0, -1, 235_638.5, "235638", Number.NaN]) {
  const refused = graded(criticalPathMilliseconds);
  const shape = `${typeof criticalPathMilliseconds}:${String(criticalPathMilliseconds)}`;
  assert.equal(refused.baselineComparison.verdict.result, "unevaluable", `measurement ${shape} must refuse`);
  assert.equal(refused.result, "incomplete", `measurement ${shape} must reach the exit path`);
}

// A definite resource overrun outranks a refusal: a real overrun still reports `fail` even when the
// verdict basis is unreadable, and the run is still incomplete.
const overAndRefused = summarize(run, overRunnerBudget, artifacts, withVerdict(259_393, { acceptanceTargetMilliseconds: null }));
assert.equal(overAndRefused.baselineComparison.result, "fail");
assert.equal(overAndRefused.result, "incomplete");

// A protected run has no PR feedback verdict to grade: `sir.protected-join/v1` carries stage
// results, not feedback timing. The comparison is not applicable rather than failed. The previous
// verdict term was `verdictMilliseconds > 0`, which made every protected run record `fail` against
// a ceiling that was never applied to it.
const protectedReceipts = [
  { path: "core.json", sha256: "1".repeat(64), value: { schema: "sir.protected-stage/v1", stage: "core", status: "pass", source: { commit: head }, timingMilliseconds: { total: 4_000 } } },
  { path: "join.json", sha256: "2".repeat(64), value: { schema: "sir.protected-join/v1", result: "pass", source: { commit: head } } },
  { path: "preflight.json", sha256: "3".repeat(64), value: { schema: "sir.protected-stage/v1", stage: "preflight", status: "pass", source: { commit: head }, timingMilliseconds: { total: 3_000 } } },
];
const protectedReport = summarize({ ...run, event: "push" }, jobs, artifacts, protectedReceipts);
assert.equal(protectedReport.reconciliation.expectedReceiptShape, true);
assert.equal(protectedReport.baselineComparison.verdict.result, "not-applicable");
assert.deepEqual(protectedReport.baselineComparison.verdict.receipts, []);
assert.equal(protectedReport.baselineComparison.result, "pass");
assert.equal(protectedReport.result, "complete");

// `not-applicable` cannot launder a pull_request run into a pass: with no PR feedback join the
// expected receipt shape already refuses, so the run is incomplete whatever the grade says.
const withoutJoin = summarize(run, jobs, artifacts, [receipts[0]]);
assert.equal(withoutJoin.baselineComparison.verdict.result, "not-applicable");
assert.equal(withoutJoin.reconciliation.expectedReceiptShape, false);
assert.equal(withoutJoin.result, "incomplete");

const observer = readFileSync(new URL("../.github/workflows/ci-cost-observer.yml", import.meta.url), "utf8");
assert.match(observer, /workflow_run:\n    workflows: \[CI\]\n    types: \[completed\]/u);
assert.match(observer, /workflow_call:\n    inputs:/u);
assert.match(observer, /permissions:\n  actions: read\n  contents: read/u);
assert.doesNotMatch(observer, /pull-requests: write|contents: write|issues: write/u);
assert.match(observer, /OBSERVED_HEAD_SHA: \$\{\{ inputs\.observed_head_sha \|\| github\.event\.workflow_run\.head_sha \}\}/u);
assert.match(observer, /--active-observer-job-name '\$\{\{ inputs\.active_observer_job_name \}\}'/u);
console.log("CI cost report reconciles API wall/runner/action/artifact facts with exact-head typed receipts, grades the PR feedback verdict against the acceptance target the gate ACTUALLY APPLIED to that run rather than any constant restated here, refuses a basis or measurement that is not a positive safe integer, and fails closed on incomplete or invalid inventories.");
