#!/usr/bin/env node
import fs from "node:fs";
import os from "node:os";
import path from "node:path";
import crypto from "node:crypto";
import { spawn } from "node:child_process";

const root = process.cwd();
const selfTest = process.argv.includes("--self-test");
const positional = process.argv.slice(2).filter(argument => argument !== "--self-test");
const outputRoot = path.resolve(root, positional[0] ?? "");
const receiptPath = path.resolve(root, positional[1] ?? "");
if (!selfTest && (!positional[0] || !positional[1])) throw new Error("usage: qualify-render-replay.mjs <output-root> <receipt-path>");

const cpuCount = os.availableParallelism();
const maxCpuUtilization = 0.5;
const maxCpuPressureAvg10 = 10;
const preflightSamples = 5;
const session = crypto.randomUUID();
const sleep = ms => new Promise(resolve => setTimeout(resolve, ms));
const round = value => Math.round(value * 1000) / 1000;

function cpuPressure() {
  const text = fs.readFileSync("/proc/pressure/cpu", "utf8");
  return Number(text.match(/^some\s+avg10=([0-9.]+)/m)?.[1] ?? Number.NaN);
}

function isDescendant(pid, ancestorPid) {
  if (!ancestorPid) return false;
  let current = pid;
  for (let depth = 0; depth < 12 && current > 1; depth += 1) {
    if (current === ancestorPid) return true;
    try {
      current = Number(fs.readFileSync(`/proc/${current}/status`, "utf8").match(/^PPid:\s+(\d+)/m)?.[1] ?? 0);
    } catch {
      return false;
    }
  }
  return false;
}

function competitors(ownedRootPid) {
  const matches = [];
  for (const entry of fs.readdirSync("/proc")) {
    if (!/^\d+$/.test(entry)) continue;
    try {
      const command = fs.readFileSync(`/proc/${entry}/cmdline`).toString().replaceAll("\0", " ");
      if (!/(inspect-rendered-visuals\.mjs|chrome(?:-headless-shell)?|chromium)/i.test(command)) continue;
      const pid = Number(entry);
      const environment = fs.readFileSync(`/proc/${entry}/environ`).toString();
      if (!isDescendant(pid, ownedRootPid) && !environment.includes(`SIR_M7_RENDER_SESSION=${session}`)) matches.push({ pid, command: command.slice(0, 240).trim() });
    } catch {
      // A process may exit between directory enumeration and inspection.
    }
  }
  return matches.sort((left, right) => left.pid - right.pid);
}

let previousCpuCounters;
function readCpuCounters() {
  const values = fs.readFileSync("/proc/stat", "utf8").split("\n")[0].trim().split(/\s+/).slice(1).map(Number);
  return { idle: values[3] + values[4], total: values.reduce((sum, value) => sum + value, 0) };
}

function hostSample(ownedRootPid) {
  const load1 = os.loadavg()[0];
  const counters = readCpuCounters();
  const utilization = previousCpuCounters && counters.total > previousCpuCounters.total
    ? 1 - ((counters.idle - previousCpuCounters.idle) / (counters.total - previousCpuCounters.total))
    : 0;
  previousCpuCounters = counters;
  return {
    cpuCount,
    cpuUtilization: round(utilization),
    load1: round(load1),
    normalizedLoad1: round(load1 / cpuCount),
    cpuPressureSomeAvg10: cpuPressure(),
    competitors: competitors(ownedRootPid)
  };
}

function isQuiescent(sample) {
  return sample.cpuUtilization <= maxCpuUtilization &&
    sample.cpuPressureSomeAvg10 <= maxCpuPressureAvg10 &&
    sample.competitors.length === 0;
}

if (selfTest) {
  const green = { cpuUtilization: 0.1, normalizedLoad1: 0.1, cpuPressureSomeAvg10: 0, competitors: [] };
  const mutations = [
    { ...green, cpuUtilization: maxCpuUtilization + 0.001 },
    { ...green, cpuPressureSomeAvg10: maxCpuPressureAvg10 + 0.001 },
    { ...green, competitors: [{ pid: 1, command: "chromium" }] }
  ];
  if (!isQuiescent(green) || mutations.some(isQuiescent)) throw new Error("render validity predicate mutation did not observe red and restored green");
  console.log("M7 render validity self-test: PASS (3 observed-red/restored-green controls)");
  process.exit(0);
}

async function awaitQuiescence() {
  const accepted = [];
  for (let attempt = 0; attempt < 60; attempt += 1) {
    const sample = hostSample();
    if (isQuiescent(sample)) accepted.push(sample); else accepted.length = 0;
    if (accepted.length === preflightSamples) return accepted;
    await sleep(500);
  }
  throw new Error(`render host did not become quiescent: ${JSON.stringify(hostSample())}`);
}

async function runBatch(id) {
  const preflight = await awaitQuiescence();
  const output = path.join(outputRoot, id);
  fs.mkdirSync(output, { recursive: true });
  const seenCompetitors = new Map();
  const hostSamples = [];
  let outputText = "";
  const child = spawn(process.execPath, ["work/377-handbook-m6v/inspect-rendered-visuals.mjs"], {
    cwd: root,
    env: {
      ...process.env,
      SIR_M7_RENDER_SESSION: session,
      SIR_M6V_BROWSER_PREFLIGHT_RECEIPT: "readiness/380-handbook-m7/browser-preflight.json",
      SIR_M6V_RENDER_OUTPUT: output
    },
    stdio: ["ignore", "pipe", "pipe"]
  });
  for (const stream of [child.stdout, child.stderr]) stream.on("data", chunk => {
    const text = chunk.toString();
    outputText += text;
    process.stderr.write(text);
  });
  const monitor = setInterval(() => {
    for (const process of competitors(child.pid)) seenCompetitors.set(process.pid, process);
    hostSamples.push(hostSample(child.pid));
  }, 250);
  const exitCode = await new Promise(resolve => child.on("exit", code => resolve(code ?? 1)));
  clearInterval(monitor);
  hostSamples.push(hostSample(child.pid));
  const invalidHostSamples = hostSamples.filter(sample => !isQuiescent(sample));
  const environment = {
    sampleCount: hostSamples.length,
    maxCpuUtilization: round(Math.max(...hostSamples.map(sample => sample.cpuUtilization))),
    maxNormalizedLoad1: round(Math.max(...hostSamples.map(sample => sample.normalizedLoad1))),
    maxCpuPressureSomeAvg10: round(Math.max(...hostSamples.map(sample => sample.cpuPressureSomeAvg10))),
    competitors: [...seenCompetitors.values()],
    invalidSampleCount: invalidHostSamples.length,
    firstInvalidSamples: invalidHostSamples.slice(0, 5)
  };
  const timingMatch = outputText.match(/render timing budget exceeded: (\{[^\n]+\})/);
  const failedTiming = timingMatch ? JSON.parse(timingMatch[1]) : null;
  if (invalidHostSamples.length > 0 || seenCompetitors.size > 0) {
    const error = new Error(`render batch ${id} invalidated by concurrent host load: ${JSON.stringify(environment)}`);
    error.batch = { id, preflight, exitCode, failedTiming, environment };
    throw error;
  }
  if (exitCode !== 0) {
    const error = new Error(`render batch ${id} failed the unchanged M6V gate in a valid quiescent environment (exit ${exitCode})`);
    error.batch = { id, preflight, exitCode, failedTiming, environment };
    throw error;
  }
  const inspection = JSON.parse(fs.readFileSync(path.join(output, "inspection.json"), "utf8"));
  return {
    id,
    preflight,
    candidateSourceInputsSha256: inspection.provenance.candidateSourceInputsSha256,
    sampleCount: inspection.timings.sampleCount,
    warmupNavigations: inspection.timings.warmupNavigations,
    p95LoadMs: inspection.timings.p95LoadMs,
    p99LoadMs: inspection.timings.p99LoadMs,
    maxP95Ms: inspection.timings.maxP95Ms,
    maxP99Ms: inspection.timings.maxP99Ms,
    samples: inspection.timings.samples.map(sample => sample.readinessMs),
    environment
  };
}

const receipt = {
  schema: "sir.handbook.render-measurement-validity/v1",
  status: "pass",
  policy: {
    exactNode: "v26.5.0",
    batches: 2,
    preflightSamples,
    preflightIntervalMs: 500,
    maxCpuUtilization,
    normalizedLoad1RecordedButNotGating: true,
    maxCpuPressureSomeAvg10: maxCpuPressureAvg10,
    competingBrowserOrRenderProcessesAllowed: false,
    performanceIntent: "unchanged M6V warm same-browser SVG load/decode gate",
    maxP95Ms: 100,
    maxP99Ms: 200
  },
  batches: []
};

try {
  receipt.batches.push(await runBatch("batch-a"));
  receipt.batches.push(await runBatch("batch-b"));
  if (receipt.batches.some(batch => batch.sampleCount !== 100 || batch.warmupNavigations !== 3 || batch.p95LoadMs > 100 || batch.p99LoadMs > 200)) {
    throw new Error("render replay did not preserve the inherited sample count, warmup count, or 100/200 ms budgets");
  }
  if (new Set(receipt.batches.map(batch => batch.candidateSourceInputsSha256)).size !== 1) throw new Error("render batches used different candidate source inputs");
  fs.mkdirSync(path.dirname(receiptPath), { recursive: true });
  fs.writeFileSync(receiptPath, JSON.stringify(receipt, null, 2) + "\n");
  console.log(`M7 render validity: PASS (${receipt.batches.map(batch => `${batch.id} p95=${batch.p95LoadMs}ms p99=${batch.p99LoadMs}ms`).join("; ")})`);
} catch (error) {
  if (error.batch) receipt.batches.push(error.batch);
  receipt.status = error.message.includes("invalidated by concurrent host load") || error.message.includes("did not become quiescent")
    ? "invalid-environment"
    : "fail";
  receipt.error = error.message;
  fs.mkdirSync(path.dirname(receiptPath), { recursive: true });
  fs.writeFileSync(receiptPath, JSON.stringify(receipt, null, 2) + "\n");
  throw error;
}
