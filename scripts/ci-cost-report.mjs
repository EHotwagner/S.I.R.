import { createHash } from "node:crypto";
import { mkdir, readFile, readdir, stat, writeFile } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { pathToFileURL } from "node:url";

export const schema = "sir.ci-cost-report/v1";
// S.I.R.#295. There is NO verdict constant in this file, and its absence is the repair. The PR
// feedback ceiling is graph-sized: `ci-route.mjs` derives it per classification as
// max(feedbackBudget, overhead + waves * waveBudget) less a headroom fraction, so any figure
// restated here is a second source of truth that goes stale the moment the graph moves. It did.
// This file graded a `cross-cutting` route against a flat 240000 while `pr-verdict` applied 312000,
// and on 2026-08-23 four of five sampled cross-cutting runs recorded `baselineComparison.result`
// "fail" for runs the gate admitted (runs 32629941705, 32627284163, 32626778195, 32624951107 at
// heads 9ef7bd7e, 06d4cdc9, 582e8d0e, 2de75573).
//
// Re-deriving the ceiling here would not fix that. The derivation is per-classification and this
// file does not know the route's expected subject set, so it would have to reconstruct one -- and a
// second derivation is a second source of truth wearing a different hat. Instead the report reads
// the basis the gate ACTUALLY APPLIED to the run being described, out of the same join receipt it
// already reads the measurement from. One artifact carries numerator and denominator, so there is
// no second number left to disagree with.
export const verdictBasisSchema = "sir.ci-join/v1";
export const verdictObservedField = "timing.criticalPathMilliseconds";
export const verdictBasisField = "timing.acceptanceTargetMilliseconds";
export const baselines = Object.freeze({ runnerMilliseconds: 1_518_000, artifactBytes: 351_337_554 });
const targets = Object.freeze({ runnerMilliseconds: Math.floor(baselines.runnerMilliseconds * 0.8), artifactBytes: Math.floor(baselines.artifactBytes * 0.8) });
// A measurement or a basis is USABLE only when it is a positive safe integer. Missing key, null, a
// string, a float, zero, a negative and NaN all reach the same answer -- this cannot be evaluated --
// and not one of them is enumerated, so a shape nobody anticipated refuses rather than being graded.
// The predicate rests on a positive fact rather than on the absence of a diagnostic, because
// `null > ceiling` is false and would otherwise have let a non-measurement read as conforming.
const usable = (value) => (Number.isSafeInteger(value) && value > 0 ? value : null);
const canonical = (value) => `${JSON.stringify(value, null, 2)}\n`;
const sha256 = (value) => createHash("sha256").update(value).digest("hex");
const milliseconds = (started, completed) => {
  const left = Date.parse(started ?? "");
  const right = Date.parse(completed ?? "");
  return Number.isFinite(left) && Number.isFinite(right) && right >= left ? right - left : null;
};

function argumentsFor(argv) {
  const [mode, ...tail] = argv;
  const values = new Map();
  for (let index = 0; index < tail.length; index += 2) {
    const name = tail[index];
    if (!name?.startsWith("--") || index + 1 >= tail.length) throw new Error(`ci-cost-report: malformed option ${name ?? "<missing>"}`);
    values.set(name.slice(2), tail[index + 1]);
  }
  return { mode, one: (key, fallback) => values.get(key) ?? fallback };
}

async function json(path) { return JSON.parse(await readFile(resolve(path), "utf8")); }

async function fetchJson(url, token) {
  const response = await fetch(url, { headers: { accept: "application/vnd.github+json", authorization: `Bearer ${token}`, "x-github-api-version": "2022-11-28" } });
  if (!response.ok) throw new Error(`ci-cost-report: GitHub API ${response.status} for ${url}`);
  return response.json();
}

async function paged(url, field, token) {
  const output = [];
  for (let page = 1; ; page += 1) {
    const separator = url.includes("?") ? "&" : "?";
    const value = await fetchJson(`${url}${separator}per_page=100&page=${page}`, token);
    const items = value[field] ?? [];
    output.push(...items);
    if (items.length < 100) return { totalCount: value.total_count ?? output.length, items: output };
  }
}

async function receiptFiles(root) {
  const information = await stat(root).catch(() => undefined);
  if (!information) return [];
  if (information.isFile()) return root.endsWith(".json") ? [root] : [];
  const files = [];
  for (const entry of await readdir(root, { withFileTypes: true })) {
    const child = resolve(root, entry.name);
    if (entry.isDirectory()) files.push(...await receiptFiles(child));
    else if (entry.isFile() && entry.name.endsWith(".json")) files.push(child);
  }
  return files.sort();
}

async function receiptsUnder(root) {
  const receipts = [];
  for (const path of await receiptFiles(resolve(root))) {
    const bytes = await readFile(path);
    let value;
    try { value = JSON.parse(bytes); } catch { continue; }
    if (typeof value?.schema !== "string" || !value.schema.startsWith("sir.")) continue;
    receipts.push({ path: path.replace(`${resolve(root)}/`, ""), sha256: sha256(bytes), value });
  }
  return receipts;
}

export function summarize(run, jobsPayload, artifactsPayload, receiptInventory = [], options = {}) {
  if (!run?.id || !run?.head_sha || !run?.run_attempt) throw new Error("ci-cost-report: incomplete workflow run identity");
  const allJobs = (jobsPayload.jobs ?? jobsPayload.items ?? []).map((job) => ({
    id: job.id,
    name: job.name,
    status: job.status,
    conclusion: job.conclusion,
    startedAt: job.started_at,
    completedAt: job.completed_at,
    durationMilliseconds: milliseconds(job.started_at, job.completed_at),
    steps: (job.steps ?? []).map((step) => ({ name: step.name, conclusion: step.conclusion, durationMilliseconds: milliseconds(step.started_at, step.completed_at) })),
  })).sort((left, right) => String(left.name).localeCompare(String(right.name)) || Number(left.id) - Number(right.id));
  const activeObserverJobName = options.activeObserverJobName ?? null;
  const invalidSkippedJobs = allJobs.filter(({ conclusion, status }) => conclusion === "skipped" && status !== "completed");
  if (invalidSkippedJobs.length > 0) throw new Error("ci-cost-report: incomplete skipped job inventory");
  const skippedJobs = allJobs.filter(({ conclusion, status }) => conclusion === "skipped" && status === "completed");
  const excludedActiveObservers = activeObserverJobName === null
    ? []
    : allJobs.filter(({ name, status, durationMilliseconds }) => name === activeObserverJobName && status !== "completed" && durationMilliseconds === null);
  if (activeObserverJobName !== null && excludedActiveObservers.length !== 1) throw new Error("ci-cost-report: active observer identity mismatch");
  const jobs = allJobs.filter((job) => !skippedJobs.includes(job) && !excludedActiveObservers.includes(job));
  if (jobs.length === 0 || jobs.some(({ durationMilliseconds }) => durationMilliseconds === null)) throw new Error("ci-cost-report: incomplete job timing inventory");
  const artifacts = (artifactsPayload.artifacts ?? artifactsPayload.items ?? []).filter(({ expired }) => expired !== true).map((artifact) => ({
    id: artifact.id,
    name: artifact.name,
    sizeInBytes: artifact.size_in_bytes,
    createdAt: artifact.created_at,
    expiresAt: artifact.expires_at,
  })).sort((left, right) => String(left.name).localeCompare(String(right.name)) || Number(left.id) - Number(right.id));
  if (artifacts.some(({ sizeInBytes }) => !Number.isSafeInteger(sizeInBytes) || sizeInBytes < 0)) throw new Error("ci-cost-report: invalid artifact byte inventory");
  const candidateReceipts = receiptInventory.map(({ path, sha256: digest, value }) => ({
    path,
    sha256: digest,
    schema: value.schema,
    status: value.status ?? value.result ?? null,
    source: value.source ?? null,
    durationMilliseconds: value.timingMilliseconds?.total ?? value.timing?.criticalPathMilliseconds ?? value.timing?.criticalPath ?? null,
  }));
  const mismatchedReceipts = candidateReceipts.filter(({ source }) => source?.commit && source.commit !== run.head_sha);
  const gateReceipts = candidateReceipts.filter(({ schema: receiptSchema }) => receiptSchema === "sir.ci-gate-result/v1");
  const protectedStages = candidateReceipts.filter(({ schema: receiptSchema }) => receiptSchema === "sir.protected-stage/v1");
  const joins = candidateReceipts.filter(({ schema: receiptSchema }) => ["sir.ci-join/v1", "sir.protected-join/v1"].includes(receiptSchema));
  // The verdict comparison is defined over the PR feedback join ALONE, and it takes both of its
  // numbers from that one receipt: the attributable critical path the gate measured, and the
  // acceptance target the gate applied to it. The fields are read by name rather than through the
  // inventory's `??` chain above, so a join that later grows a `timingMilliseconds.total` cannot
  // silently change which quantity is graded.
  const verdicts = receiptInventory
    .filter(({ value }) => value.schema === verdictBasisSchema)
    .map(({ path, value }) => {
      const observedMilliseconds = usable(value.timing?.criticalPathMilliseconds);
      const appliedTargetMilliseconds = usable(value.timing?.acceptanceTargetMilliseconds);
      const appliedBudgetMilliseconds = usable(value.timing?.budgetMilliseconds);
      return {
        path,
        observedMilliseconds,
        appliedTargetMilliseconds,
        appliedBudgetMilliseconds,
        // Only a receipt supplying BOTH positive integers is graded. `within` stays null otherwise,
        // because an ungraded receipt must never read as a passing one.
        within: observedMilliseconds !== null && appliedTargetMilliseconds !== null ? observedMilliseconds <= appliedTargetMilliseconds : null,
      };
    })
    .sort((left, right) => String(left.path).localeCompare(String(right.path)));
  const unevaluableVerdicts = verdicts.filter(({ within }) => within === null);
  const verdictResult = verdicts.length === 0
    // No PR feedback join at all. On a pull_request run `expectedReceiptShape` already refuses that
    // below, so this cannot become a silent pass; on a protected run there is no PR feedback verdict
    // to grade, which is a property of the run rather than a measurement that went missing.
    ? "not-applicable"
    : unevaluableVerdicts.length > 0
      ? "unevaluable"
      : verdicts.every(({ within }) => within) ? "pass" : "fail";
  const pullRequest = run.event === "pull_request";
  const expectedReceiptShape = pullRequest
    ? gateReceipts.length > 0 && joins.some(({ schema: receiptSchema }) => receiptSchema === "sir.ci-join/v1")
    : protectedStages.length === 2 && joins.some(({ schema: receiptSchema }) => receiptSchema === "sir.protected-join/v1");
  const apiRunnerMilliseconds = jobs.reduce((sum, job) => sum + job.durationMilliseconds, 0);
  const receiptRunnerMilliseconds = [...gateReceipts, ...protectedStages].reduce((sum, receipt) => sum + (Number.isSafeInteger(receipt.durationMilliseconds) ? receipt.durationMilliseconds : 0), 0);
  const verdictMilliseconds = verdicts.reduce((maximum, { observedMilliseconds }) => observedMilliseconds === null ? maximum : Math.max(maximum, observedMilliseconds), 0);
  const artifactBytes = artifacts.reduce((sum, artifact) => sum + artifact.sizeInBytes, 0);
  const started = Date.parse(run.created_at ?? "");
  const completed = Date.parse(run.updated_at ?? "");
  // An unevaluable basis is a REFUSAL, and it is routed to the report's only exit-code path
  // (`report.result !== "complete"` in `main`) so that it surfaces as a failing observer job rather
  // than as a quietly-worded field nobody reads. Grading against a basis this code cannot evaluate
  // is the defect being repaired; declining to grade is the repair.
  const complete = expectedReceiptShape && mismatchedReceipts.length === 0 && verdictResult !== "unevaluable";
  const resourcesWithinTarget = apiRunnerMilliseconds <= targets.runnerMilliseconds && artifactBytes <= targets.artifactBytes;
  const reportBody = {
    schema,
    result: complete ? "complete" : "incomplete",
    source: { repository: run.repository?.full_name ?? null, workflowRunId: run.id, attempt: run.run_attempt, event: run.event, headSha: run.head_sha, headBranch: run.head_branch, conclusion: run.conclusion },
    workflowWallMilliseconds: Number.isFinite(started) && Number.isFinite(completed) && completed >= started ? completed - started : null,
    jobs,
    exclusions: [
      ...skippedJobs.map(({ id, name, status }) => ({ id, name, status, reason: "completed skipped job allocated no runner" })),
      ...excludedActiveObservers.map(({ id, name, status }) => ({ id, name, status, reason: "current reusable observer cannot complete before observing its own run" })),
    ],
    artifacts,
    receipts: candidateReceipts,
    totals: {
      jobs: jobs.length,
      skippedJobs: skippedJobs.length,
      apiRunnerMilliseconds,
      receiptRunnerMilliseconds,
      unreceiptedOrActionMilliseconds: apiRunnerMilliseconds - receiptRunnerMilliseconds,
      artifacts: artifacts.length,
      artifactBytes,
    },
    baselineComparison: {
      baselines,
      targets,
      observed: { runnerMilliseconds: apiRunnerMilliseconds, artifactBytes, verdictMilliseconds },
      // Where the verdict figure came from, stated in the artifact itself, so a reader of the
      // 90-day record can see WHICH ceiling this run was graded against without trusting that some
      // constant elsewhere happened to be current on the day.
      verdict: {
        basis: "applied",
        schema: verdictBasisSchema,
        observedField: verdictObservedField,
        basisField: verdictBasisField,
        receipts: verdicts,
        unevaluableCount: unevaluableVerdicts.length,
        result: verdictResult,
      },
      reductionsPercent: {
        runner: Number((((baselines.runnerMilliseconds - apiRunnerMilliseconds) / baselines.runnerMilliseconds) * 100).toFixed(3)),
        artifacts: Number((((baselines.artifactBytes - artifactBytes) / baselines.artifactBytes) * 100).toFixed(3)),
      },
      // A definite overrun outranks a refusal, and a refusal outranks a pass: a run that really did
      // exceed a resource target still reports `fail` even when its verdict basis is unreadable.
      result: !resourcesWithinTarget || verdictResult === "fail" ? "fail" : verdictResult === "unevaluable" ? "unevaluable" : "pass",
    },
    reconciliation: { expectedReceiptShape, mismatchedReceiptCount: mismatchedReceipts.length, unevaluableVerdictCount: unevaluableVerdicts.length, status: complete ? "complete" : "incomplete" },
  };
  return { ...reportBody, digest: sha256(canonical(reportBody)) };
}

async function main(argv) {
  const { mode, one } = argumentsFor(argv);
  let run;
  let jobs;
  let artifacts;
  if (mode === "fixture") {
    run = await json(one("run", ""));
    jobs = await json(one("jobs", ""));
    artifacts = await json(one("artifacts", ""));
  } else if (mode === "observe") {
    const repository = one("repository", process.env.GITHUB_REPOSITORY ?? "");
    const runId = one("run-id", "");
    const token = process.env.GITHUB_TOKEN ?? "";
    if (!/^[^/]+\/[^/]+$/u.test(repository) || !/^\d+$/u.test(runId) || token.length < 20) throw new Error("ci-cost-report: incomplete observation authority");
    const base = `https://api.github.com/repos/${repository}/actions/runs/${runId}`;
    run = await fetchJson(base, token);
    const jobsResult = await paged(`${base}/jobs`, "jobs", token);
    const artifactsResult = await paged(`${base}/artifacts`, "artifacts", token);
    jobs = { total_count: jobsResult.totalCount, jobs: jobsResult.items };
    artifacts = { total_count: artifactsResult.totalCount, artifacts: artifactsResult.items };
  } else throw new Error("ci-cost-report: usage fixture|observe");
  const expectedHead = one("expected-head", run.head_sha);
  const expectedAttempt = Number(one("expected-attempt", String(run.run_attempt)));
  if (run.head_sha !== expectedHead || run.run_attempt !== expectedAttempt) throw new Error("ci-cost-report: workflow identity mismatch");
  const activeObserverJobName = one("active-observer-job-name", "");
  const report = summarize(run, jobs, artifacts, await receiptsUnder(one("receipts", "artifacts/observed")), {
    activeObserverJobName: activeObserverJobName.length > 0 ? activeObserverJobName : null,
  });
  await mkdir(dirname(resolve(one("output", ""))), { recursive: true });
  await writeFile(resolve(one("output", "")), canonical(report));
  console.log(canonical(report).trim());
  if (report.result !== "complete") process.exitCode = 1;
}

if (import.meta.url === pathToFileURL(process.argv[1]).href) main(process.argv.slice(2)).catch((error) => { console.error(error.message); process.exitCode = 1; });
