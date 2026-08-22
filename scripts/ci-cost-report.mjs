import { createHash } from "node:crypto";
import { mkdir, readFile, readdir, stat, writeFile } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { pathToFileURL } from "node:url";

export const schema = "sir.ci-cost-report/v1";
export const baselines = Object.freeze({ runnerMilliseconds: 1_518_000, artifactBytes: 351_337_554, verdictMilliseconds: 240_000 });
const targets = Object.freeze({ runnerMilliseconds: Math.floor(baselines.runnerMilliseconds * 0.8), artifactBytes: Math.floor(baselines.artifactBytes * 0.8), verdictMilliseconds: baselines.verdictMilliseconds });
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
  const pullRequest = run.event === "pull_request";
  const expectedReceiptShape = pullRequest
    ? gateReceipts.length > 0 && joins.some(({ schema: receiptSchema }) => receiptSchema === "sir.ci-join/v1")
    : protectedStages.length === 2 && joins.some(({ schema: receiptSchema }) => receiptSchema === "sir.protected-join/v1");
  const apiRunnerMilliseconds = jobs.reduce((sum, job) => sum + job.durationMilliseconds, 0);
  const receiptRunnerMilliseconds = [...gateReceipts, ...protectedStages].reduce((sum, receipt) => sum + (Number.isSafeInteger(receipt.durationMilliseconds) ? receipt.durationMilliseconds : 0), 0);
  const verdictMilliseconds = joins.reduce((maximum, receipt) => Number.isSafeInteger(receipt.durationMilliseconds) ? Math.max(maximum, receipt.durationMilliseconds) : maximum, 0);
  const artifactBytes = artifacts.reduce((sum, artifact) => sum + artifact.sizeInBytes, 0);
  const started = Date.parse(run.created_at ?? "");
  const completed = Date.parse(run.updated_at ?? "");
  const reportBody = {
    schema,
    result: expectedReceiptShape && mismatchedReceipts.length === 0 ? "complete" : "incomplete",
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
      reductionsPercent: {
        runner: Number((((baselines.runnerMilliseconds - apiRunnerMilliseconds) / baselines.runnerMilliseconds) * 100).toFixed(3)),
        artifacts: Number((((baselines.artifactBytes - artifactBytes) / baselines.artifactBytes) * 100).toFixed(3)),
      },
      result: apiRunnerMilliseconds <= targets.runnerMilliseconds && artifactBytes <= targets.artifactBytes && verdictMilliseconds > 0 && verdictMilliseconds <= targets.verdictMilliseconds ? "pass" : "fail",
    },
    reconciliation: { expectedReceiptShape, mismatchedReceiptCount: mismatchedReceipts.length, status: expectedReceiptShape && mismatchedReceipts.length === 0 ? "complete" : "incomplete" },
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
