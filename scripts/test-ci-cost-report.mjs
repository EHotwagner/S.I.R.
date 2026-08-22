import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { summarize } from "./ci-cost-report.mjs";

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
  { path: "join.json", sha256: "d".repeat(64), value: { schema: "sir.ci-join/v1", result: "pass", source: { commit: head, tree: "c".repeat(40) }, timing: { criticalPath: 8_000 } } },
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

const observer = readFileSync(new URL("../.github/workflows/ci-cost-observer.yml", import.meta.url), "utf8");
assert.match(observer, /workflow_run:\n    workflows: \[CI\]\n    types: \[completed\]/u);
assert.match(observer, /workflow_call:\n    inputs:/u);
assert.match(observer, /permissions:\n  actions: read\n  contents: read/u);
assert.doesNotMatch(observer, /pull-requests: write|contents: write|issues: write/u);
assert.match(observer, /OBSERVED_HEAD_SHA: \$\{\{ inputs\.observed_head_sha \|\| github\.event\.workflow_run\.head_sha \}\}/u);
assert.match(observer, /--active-observer-job-name '\$\{\{ inputs\.active_observer_job_name \}\}'/u);
console.log("CI cost report reconciles API wall/runner/action/artifact facts with exact-head typed receipts and fails closed on incomplete or invalid inventories.");
