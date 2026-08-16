import { createHash } from "node:crypto";
import { readFile, writeFile, mkdir } from "node:fs/promises";
import { basename, dirname, resolve } from "node:path";
import { pathToFileURL } from "node:url";

export const routeSchema = "sir.ci-route/v1";
export const gateSchema = "sir.ci-gate-result/v1";
export const joinSchema = "sir.ci-join/v1";
export const timingSchema = "sir.ci-timing/v1";
export const policyVersion = "1";
export const feedbackBudgetMilliseconds = 300_000;
export const gateOrder = ["rules", "spatial", "cancellation", "cross-runtime", "browser", "documentation", "evidence"];
export const subjectOrder = ["integrity", "prepare", ...gateOrder];

const classifications = {
  documentation: ["documentation", "evidence"],
  domain: ["rules", "spatial", "cross-runtime", "evidence"],
  browser: ["browser", "evidence"],
  "evidence-only": ["evidence"],
  "cross-cutting": [...gateOrder],
};

const normalize = (path) => path.trim().replaceAll("\\", "/").replace(/^\.\//u, "");
const under = (path, prefix) => path === prefix || path.startsWith(`${prefix}/`);
const isDocumentation = (path) => under(path, "docs") || path === "scripts/build-docs.sh";
const isDomain = (path) => ["src/SIR.Domain", "src/SIR.Simulation", "src/SIR.Match", "tests/SIR.Domain.Tests", "tests/SIR.Domain.Fable.Tests", "tests/SIR.Match.Tests", "tests/SIR.Conformance.Shared"].some((prefix) => under(path, prefix));
const isBrowser = (path) => ["src/SIR.Client", "src/SIR.Client.Web", "src/SIR.Server", "tests/SIR.Browser.Tests", "tests/SIR.Client.Tests", "tests/SIR.Server.Tests"].some((prefix) => under(path, prefix));
const isEvidence = (path) => ["feedback", "readiness", "work"].some((prefix) => under(path, prefix));
const isCrossCutting = (path) => [".github", ".config", ".fsgg"].some((prefix) => under(path, prefix))
  || ["package.json", "package-lock.json", "global.json", "Directory.Build.props", "Directory.Packages.props", "SIR.slnx"].includes(path)
  || (under(path, "scripts") && !isDocumentation(path));

const canonical = (value) => `${JSON.stringify(value, null, 2)}\n`;
const digest = (value) => createHash("sha256").update(canonical(value)).digest("hex");
const routeBody = (route) => Object.fromEntries(Object.entries(route ?? {}).filter(([key]) => key !== "digest"));
export const routeDigest = (route) => digest(routeBody(route));

function classifyOne(path) {
  if (isEvidence(path)) return { classification: "evidence-only", rule: "RP-000-evidence-metadata" };
  if (isCrossCutting(path)) return { classification: "cross-cutting", rule: "RP-004-cross-cutting" };
  if (isDocumentation(path)) return { classification: "documentation", rule: "RP-001-documentation" };
  if (isDomain(path)) return { classification: "domain", rule: "RP-002-domain" };
  if (isBrowser(path)) return { classification: "browser", rule: "RP-003-browser" };
  return { classification: "cross-cutting", rule: "RP-005-unknown-conservative" };
}

export function routePaths(rawPaths, source = {}) {
  if (!Array.isArray(rawPaths)) throw new Error("ci-route: changed paths must be an array");
  const paths = [...new Set(rawPaths.map(normalize).filter(Boolean))].sort();
  if (paths.length === 0) throw new Error("ci-route: changed-path inventory is empty");
  if (paths.some((path) => path.startsWith("/") || path.includes("\0") || path.split("/").includes(".."))) throw new Error("ci-route: malformed changed path");
  const facts = paths.map((path) => ({ path, ...classifyOne(path) }));
  const kinds = [...new Set(facts.map((fact) => fact.classification).filter((kind) => kind !== "evidence-only"))];
  const classification = kinds.length === 0 ? "evidence-only" : kinds.length === 1 ? kinds[0] : "cross-cutting";
  const selectedGates = classifications[classification];
  const classificationRule = classification === "cross-cutting" && kinds.length > 1
    ? "RP-006-mixed-conservative"
    : facts.find((fact) => fact.classification === classification)?.rule ?? "RP-000-evidence-metadata";
  const skippedGates = gateOrder
    .filter((gate) => !selectedGates.includes(gate))
    .map((gate) => ({ gate, reason: `not-applicable:${classification}`, rule: classificationRule }));
  const route = {
    schema: routeSchema,
    policyVersion,
    source: { commit: source.commit ?? "unknown", tree: source.tree ?? "unknown" },
    classification,
    paths,
    facts,
    alwaysOn: ["integrity"],
    selectedGates,
    skippedGates,
  };
  return { ...route, digest: digest(route) };
}

export function gateResult(gate, status, timing = {}, details = {}) {
  if (!subjectOrder.includes(gate)) throw new Error(`ci-route: unknown gate result:${gate}`);
  if (!["pass", "fail", "cancelled"].includes(status)) throw new Error(`ci-route: invalid gate status:${status}`);
  const phases = { queue: null, setup: 0, restore: 0, build: 0, test: 0, total: 0, ...timing };
  for (const [name, value] of Object.entries(phases)) {
    if (name === "queue" && value === null) continue;
    if (!Number.isSafeInteger(value) || value < 0) throw new Error(`ci-route: invalid ${gate} ${name} duration`);
  }
  return {
    schema: gateSchema,
    gate,
    status,
    source: { commit: "unknown", tree: "unknown" },
    routeDigest: "unknown",
    artifactDigest: null,
    ownerCommand: "scripts/run-ci-gate.sh",
    gateCommand: gate === "prepare" ? "scripts/qualify-pr.sh prepare" : gate === "integrity" ? "scripts/qualify-pr.sh integrity" : `scripts/qualify-pr.sh gate ${gate}`,
    timingMilliseconds: phases,
    cacheHit: false,
    receiptReused: false,
    buildInvocations: [],
    retryCount: 0,
    failureStage: status === "pass" ? null : "test",
    ...details,
  };
}

export function joinRoute(route, results, { startedAtMilliseconds = 0, completedAtMilliseconds = 0, enforceBudget = true, expectedArtifactDigest = null, artifactManifestFailure = null } = {}) {
  const failures = [];
  if (route?.schema !== routeSchema || route.policyVersion !== policyVersion) failures.push({ code: "stale-or-malformed-route-receipt", subject: "route" });
  const computedRouteDigest = routeDigest(route);
  if (route?.digest !== computedRouteDigest) failures.push({ code: "route-digest-mismatch", subject: "route", expected: computedRouteDigest, actual: route?.digest ?? null });
  const selectedGates = Array.isArray(route?.selectedGates) ? route.selectedGates : [];
  const needsPrepared = selectedGates.some((gate) => gate !== "evidence");
  const expectedSubjects = ["integrity", ...(needsPrepared ? ["prepare"] : []), ...selectedGates];
  const byGate = new Map();
  for (const result of results) {
    const subject = typeof result?.gate === "string" ? result.gate : "unknown";
    if (result?.schema !== gateSchema) failures.push({ code: "malformed-gate-result", subject });
    if (byGate.has(subject)) {
      failures.push({ code: "duplicate-gate-result", subject });
      continue;
    }
    if (!subjectOrder.includes(subject)) failures.push({ code: "unknown-gate-result", subject });
    byGate.set(subject, result);
  }
  const expected = new Set(expectedSubjects);
  for (const subject of expectedSubjects) {
    const result = byGate.get(subject);
    if (!result) {
      failures.push({ code: "missing-gate-result", subject });
      continue;
    }
    if (result.status !== "pass") failures.push({ code: `required-gate-${result.status ?? "malformed"}`, subject });
    if (result.source?.commit !== route?.source?.commit || result.source?.tree !== route?.source?.tree) failures.push({ code: "candidate-binding-mismatch", subject });
    if (result.routeDigest !== route?.digest) failures.push({ code: "route-binding-mismatch", subject });
    if (result.ownerCommand !== "scripts/run-ci-gate.sh") failures.push({ code: "owner-command-mismatch", subject });
    if (typeof result.gateCommand !== "string" || !result.gateCommand.endsWith(subject)) failures.push({ code: "gate-command-mismatch", subject });
  }
  for (const gate of byGate.keys()) if (!expected.has(gate)) failures.push({ code: "unexpected-gate-result", subject: gate });

  const preparedDigest = byGate.get("prepare")?.artifactDigest ?? null;
  if (artifactManifestFailure) failures.push({ code: artifactManifestFailure, subject: "prepared-artifact" });
  if (needsPrepared && expectedArtifactDigest === null) failures.push({ code: "missing-artifact-manifest", subject: "prepared-artifact" });
  if (needsPrepared && expectedArtifactDigest !== null && preparedDigest !== expectedArtifactDigest) failures.push({ code: "artifact-manifest-binding-mismatch", subject: "prepare" });
  if (needsPrepared && (typeof preparedDigest !== "string" || preparedDigest.length !== 64)) failures.push({ code: "missing-prepared-artifact-binding", subject: "prepare" });
  for (const subject of expectedSubjects) {
    const result = byGate.get(subject);
    if (!result) continue;
    const needsArtifact = subject === "prepare" || (gateOrder.includes(subject) && subject !== "evidence");
    if (needsArtifact && result.artifactDigest !== preparedDigest) failures.push({ code: "artifact-binding-mismatch", subject });
    if (!needsArtifact && result.artifactDigest !== null) failures.push({ code: "unexpected-artifact-binding", subject });
  }
  const elapsed = completedAtMilliseconds - startedAtMilliseconds;
  if (!Number.isSafeInteger(elapsed) || elapsed < 0) failures.push({ code: "invalid-feedback-duration", subject: "timing" });
  if (enforceBudget && Number.isSafeInteger(elapsed) && elapsed > feedbackBudgetMilliseconds) failures.push({ code: "feedback-budget-exceeded", subject: "timing", actual: elapsed, budget: feedbackBudgetMilliseconds });
  const ordered = subjectOrder.filter((gate) => byGate.has(gate)).map((gate) => byGate.get(gate));
  const validTotals = ordered.map((result) => result?.timingMilliseconds?.total).filter((value) => Number.isSafeInteger(value) && value >= 0);
  const criticalPath = Math.max(0, ...validTotals);
  const runnerMilliseconds = validTotals.reduce((sum, value) => sum + value, 0);
  const timing = {
    schema: timingSchema,
    subject: "runner-feedback",
    startedAtMilliseconds,
    completedAtMilliseconds,
    totalMilliseconds: Number.isSafeInteger(elapsed) && elapsed >= 0 ? elapsed : null,
    budgetMilliseconds: feedbackBudgetMilliseconds,
    criticalPathMilliseconds: criticalPath,
    runnerMilliseconds,
    cacheHits: ordered.filter((result) => result.cacheHit).length,
    receiptReuses: ordered.filter((result) => result.receiptReused).length,
    buildInvocations: ordered.flatMap((result) => result.buildInvocations),
    retryCount: ordered.reduce((sum, result) => sum + (Number.isSafeInteger(result.retryCount) ? result.retryCount : 0), 0),
  };
  return { schema: joinSchema, result: failures.length === 0 ? "pass" : "fail", routeDigest: route?.digest ?? null, classification: route?.classification ?? null, selectedGates, skippedGates: route?.skippedGates ?? [], gateResults: ordered, failures, timing };
}

async function writeJson(path, value) {
  await mkdir(dirname(resolve(path)), { recursive: true });
  await writeFile(path, canonical(value));
}

function parseOptions(argv) {
  const [mode, ...tail] = argv;
  const options = new Map();
  for (let index = 0; index < tail.length; index += 2) {
    const name = tail[index];
    if (!name?.startsWith("--") || tail[index + 1] === undefined) throw new Error(`ci-route: malformed option:${name ?? "missing"}`);
    const key = name.slice(2);
    options.set(key, [...(options.get(key) ?? []), tail[index + 1]]);
  }
  return { mode, one: (name, fallback) => options.get(name)?.at(-1) ?? fallback, many: (name) => options.get(name) ?? [] };
}

async function main(argv) {
  const { mode, one, many } = parseOptions(argv);
  if (mode === "route") {
    const pathFile = one("paths-file", undefined);
    const paths = pathFile ? (await readFile(pathFile, "utf8")).split(/\r?\n/u) : many("path");
    const route = routePaths(paths, { commit: one("commit", "unknown"), tree: one("tree", "unknown") });
    const output = one("output", undefined);
    if (output) await writeJson(output, route);
    const githubOutput = one("github-output", undefined);
    if (githubOutput) {
      const lines = [`classification=${route.classification}`, `prepare=${route.selectedGates.some((gate) => gate !== "evidence")}`, ...gateOrder.map((gate) => `${gate.replace("-", "_")}=${route.selectedGates.includes(gate)}`), `digest=${route.digest}`];
      await writeFile(githubOutput, `${lines.join("\n")}\n`, { flag: "a" });
    }
    process.stdout.write(canonical(route));
    return;
  }
  if (mode === "gate") {
    const gate = one("gate", "");
    const status = one("status", "pass");
    const result = gateResult(gate, status, {
      queue: one("queue-ms", "unknown") === "unknown" ? null : Number(one("queue-ms", "0")),
      setup: Number(one("setup-ms", "0")),
      restore: Number(one("restore-ms", "0")),
      build: Number(one("build-ms", "0")),
      test: Number(one("test-ms", "0")),
      total: Number(one("total-ms", "0")),
    }, {
      source: { commit: one("commit", "unknown"), tree: one("tree", "unknown") },
      routeDigest: one("route-digest", "unknown"),
      artifactDigest: one("artifact-digest", "none") === "none" ? null : one("artifact-digest", ""),
      cacheHit: one("cache-hit", "false") === "true",
      receiptReused: one("receipt-reused", "false") === "true",
      buildInvocations: many("build"),
      retryCount: Number(one("retry-count", "0")),
      failureStage: status === "pass" ? null : one("failure-stage", "test"),
    });
    const output = one("output", undefined);
    if (output) await writeJson(output, result);
    process.stdout.write(canonical(result));
    return;
  }
  if (mode === "join") {
    const route = JSON.parse(await readFile(one("route", ""), "utf8"));
    const results = [];
    for (const declaration of many("result")) {
      const separator = declaration.indexOf("=");
      if (separator <= 0) throw new Error(`ci-route: malformed result declaration:${declaration}`);
      const result = JSON.parse(await readFile(declaration.slice(separator + 1), "utf8"));
      if (result.gate !== declaration.slice(0, separator)) throw new Error(`ci-route: gate-result-name-drift:${declaration.slice(0, separator)}`);
      results.push(result);
    }
    let expectedArtifactDigest = null;
    let artifactManifestFailure = null;
    const artifactManifestPath = one("artifact-manifest", undefined);
    if (artifactManifestPath) {
      try {
        const bytes = await readFile(artifactManifestPath);
        expectedArtifactDigest = createHash("sha256").update(bytes).digest("hex");
        const manifest = JSON.parse(bytes);
        if (basename(artifactManifestPath) !== `${expectedArtifactDigest}.json` || manifest.schema !== "sir.ci-artifact-manifest/v1" || manifest.result !== "pass") artifactManifestFailure = "malformed-artifact-manifest";
        else if (manifest.candidate?.commit !== route.source?.commit || manifest.candidate?.tree !== route.source?.tree || manifest.route?.digest !== route.digest) artifactManifestFailure = "artifact-manifest-binding-mismatch";
      } catch {
        artifactManifestFailure = "unreadable-artifact-manifest";
      }
    }
    const joined = joinRoute(route, results, { startedAtMilliseconds: Number(one("started-ms", "0")), completedAtMilliseconds: Number(one("completed-ms", "0")), enforceBudget: one("enforce-budget", "true") === "true", expectedArtifactDigest, artifactManifestFailure });
    const output = one("output", undefined);
    if (output) await writeJson(output, joined);
    process.stdout.write(canonical(joined));
    if (joined.result !== "pass") process.exitCode = 1;
    return;
  }
  throw new Error("ci-route: usage route|gate|join [options]");
}

if (import.meta.url === pathToFileURL(process.argv[1]).href) main(process.argv.slice(2)).catch((error) => { console.error(error.message); process.exitCode = 1; });
