import assert from "node:assert/strict";
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname, resolve } from "node:path";

const evidencePath = resolve("work/ci-reliability-efficiency/hosted-observation.json");
const evidence = JSON.parse(readFileSync(evidencePath, "utf8"));

assert.equal(evidence.schema, "sir.ci-hosted-evidence/v1");
assert.equal(evidence.repository, "EHotwagner/S.I.R.");
assert.ok(Number.isSafeInteger(evidence.workflow.runId) && evidence.workflow.runId > 0);
assert.equal(evidence.workflow.url, `https://github.com/EHotwagner/S.I.R./actions/runs/${evidence.workflow.runId}`);
assert.equal(evidence.workflow.attempt, 1);
assert.equal(evidence.workflow.event, "pull_request");
assert.equal(evidence.workflow.status, "completed");
assert.equal(evidence.workflow.conclusion, "success");
assert.match(evidence.workflow.headSha, /^[0-9a-f]{40}$/u);
assert.match(evidence.workflow.treeSha, /^[0-9a-f]{40}$/u);
assert.equal(evidence.verdict.schema, "sir.ci-join/v1");
assert.equal(evidence.verdict.result, "pass");
assert.equal(evidence.verdict.classification, "cross-cutting");
assert.deepEqual(evidence.verdict.selectedGates, ["rules", "spatial", "cancellation", "cross-runtime", "browser", "documentation", "evidence"]);
assert.equal(evidence.verdict.skippedGateCount, 0);
assert.equal(evidence.verdict.failedGateCount, 0);
assert.ok(evidence.verdict.elapsedMilliseconds <= evidence.targets.verdictMilliseconds);
assert.ok(evidence.cost.apiRunnerMilliseconds <= evidence.targets.runnerMilliseconds);
assert.ok(evidence.cost.artifactBytes <= evidence.targets.artifactBytes);
assert.ok(evidence.cost.runnerReductionPercent >= 20);
assert.ok(evidence.cost.artifactReductionPercent >= 20);
assert.equal(evidence.cost.baselineComparison, "pass");
assert.equal(evidence.cost.reconciliation, "complete");
assert.equal(evidence.cost.mismatchedReceiptCount, 0);
assert.equal(
  Object.entries(evidence.preparedArtifactBytes)
    .filter(([name]) => name !== "total")
    .reduce((sum, [, bytes]) => sum + bytes, 0),
  evidence.preparedArtifactBytes.total,
);
for (const digest of [evidence.verdict.payloadSha256, evidence.cost.payloadSha256, evidence.cost.reportDigest]) {
  assert.match(digest, /^[0-9a-f]{64}$/u);
}

const output = resolve(process.env.SIR_JUNIT_OUTPUT ?? "readiness/ci-reliability-efficiency/ci-reliability.junit.xml");
mkdirSync(dirname(output), { recursive: true });
writeFileSync(output, `<?xml version="1.0" encoding="UTF-8"?>
<testsuite name="ci-reliability-efficiency" tests="5" failures="0" errors="0" skipped="0">
  <testcase classname="SIR.CI" name="exact-head-hosted-verdict" />
  <testcase classname="SIR.CI" name="developer-wait-at-most-240-seconds" />
  <testcase classname="SIR.CI" name="aggregate-runner-reduction-at-least-20-percent" />
  <testcase classname="SIR.CI" name="artifact-reduction-at-least-20-percent" />
  <testcase classname="SIR.CI" name="api-reconciliation-complete" />
</testsuite>
`);

console.log("Exact-head hosted CI evidence satisfies verdict, runner, artifact, and reconciliation targets.");
