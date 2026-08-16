import { createHash } from "node:crypto";
import { readFile, writeFile, mkdir } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { pathToFileURL } from "node:url";

export const routeSchema = "sir.ci-route/v1";
export const gateSchema = "sir.ci-gate-result/v1";
export const joinSchema = "sir.ci-join/v1";
export const timingSchema = "sir.ci-timing/v1";
export const policyVersion = "1";
export const feedbackBudgetMilliseconds = 300_000;
export const gateOrder = ["rules", "spatial", "cancellation", "cross-runtime", "browser", "documentation", "evidence"];
export const producerOrder = ["prepare-native", "prepare-fable", "prepare-web", "prepare-server", "prepare-docs"];
export const subjectOrder = ["integrity", ...producerOrder, ...gateOrder];
export const gateParts = {
  rules: ["native"],
  spatial: ["native", "fable"],
  cancellation: ["native", "web"],
  "cross-runtime": ["native", "fable"],
  browser: ["web", "server"],
  documentation: ["web", "docs"],
  evidence: [],
};
export const expectedBuildInvocations = {
  integrity: [],
  "prepare-native": ["build:SIR.slnx", "build:src/SIR.Replay.Core/SIR.Replay.Core.fsproj", "build:tests/SIR.Client.Tests/SIR.Client.Tests.fsproj", "build:tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj", "producer:native"],
  "prepare-fable": ["fable:tests/SIR.Client.Tests/ScenarioCatalogRuntime.fsproj", "fable:tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj", "fable:tests/SIR.ModalInput.Fable.Tests/SIR.ModalInput.Fable.Tests.fsproj", "producer:fable"],
  "prepare-web": ["fable:src/SIR.Client.Web/SIR.RulesExplorer.Web.fsproj", "fable:src/SIR.Replay.Web/SIR.Replay.Web.fsproj", "producer:web"],
  "prepare-server": ["producer:server", "publish:src/SIR.Server/SIR.Server.fsproj"],
  "prepare-docs": ["build:src/SIR.Client/SIR.Client.fsproj", "build:src/SIR.Match/SIR.Match.fsproj", "producer:docs"],
  rules: [],
  spatial: [
    "dependency-receipt", "footprint-envelope", "semantic-edge", "knowledge-cache-key", "spatial-revision-key",
    "deterministic-ordering", "package-adapter", "profile-cache-key", "trace-work-bound",
  ].map((name) => `build:src/SIR.Simulation/SIR.Simulation.fsproj:exception:spatial-${name}:artifacts-path:isolated`),
  cancellation: ["cancellation-mutant"].flatMap((name) => [
    `fable:src/SIR.Client.Web/SIR.RulesExplorer.Web.fsproj:exception:${name}`,
    `fable:src/SIR.Replay.Web/SIR.Replay.Web.fsproj:exception:${name}`,
  ]),
  "cross-runtime": ["build:spikes/browser-wasm-verification/BrowserWasmVerificationSpike.fsproj"],
  browser: [],
  documentation: [],
  evidence: [],
};

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
export const canonicalArtifactBindings = (bindings = {}) => Object.fromEntries(
  Object.entries(bindings).sort(([left], [right]) => left.localeCompare(right)),
);
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
  const phases = { queue: null, setup: 0, restore: 0, build: 0, transport: 0, test: 0, total: 0, ...timing };
  for (const [name, value] of Object.entries(phases)) {
    if (name === "queue" && value === null) continue;
    if (!Number.isSafeInteger(value) || value < 0) throw new Error(`ci-route: invalid ${gate} ${name} duration`);
  }
  const artifactBindings = canonicalArtifactBindings(details.artifactBindings);
  const buildInvocations = [...(details.buildInvocations ?? [])].sort();
  return {
    schema: gateSchema,
    gate,
    status,
    source: { commit: "unknown", tree: "unknown" },
    routeDigest: "unknown",
    artifactDigest: null,
    ownerCommand: producerOrder.includes(gate) ? "scripts/qualify-pr.sh" : "scripts/run-ci-gate.sh",
    gateCommand: producerOrder.includes(gate) ? `scripts/qualify-pr.sh prepare-part ${gate.slice("prepare-".length)}` : gate === "integrity" ? "scripts/qualify-pr.sh integrity" : `scripts/qualify-pr.sh gate ${gate}`,
    timingMilliseconds: phases,
    cacheHit: false,
    receiptReused: false,
    buildInvocations,
    retryCount: 0,
    failureStage: status === "pass" ? null : "test",
    ...details,
    artifactBindings,
    buildInvocations,
  };
}

export function joinRoute(route, results, { startedAtMilliseconds = 0, completedAtMilliseconds = 0, enforceBudget = true } = {}) {
  const failures = [];
  if (route?.schema !== routeSchema || route.policyVersion !== policyVersion) failures.push({ code: "stale-or-malformed-route-receipt", subject: "route" });
  const computedRouteDigest = routeDigest(route);
  if (route?.digest !== computedRouteDigest) failures.push({ code: "route-digest-mismatch", subject: "route", expected: computedRouteDigest, actual: route?.digest ?? null });
  const selectedGates = Array.isArray(route?.selectedGates) ? route.selectedGates : [];
  const requiredParts = [...new Set(selectedGates.flatMap((gate) => gateParts[gate] ?? []))];
  const expectedProducers = requiredParts.map((part) => `prepare-${part}`);
  const expectedSubjects = ["integrity", ...expectedProducers, ...selectedGates];
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
    const expectedOwner = producerOrder.includes(subject) ? "scripts/qualify-pr.sh" : "scripts/run-ci-gate.sh";
    if (result.ownerCommand !== expectedOwner) failures.push({ code: "owner-command-mismatch", subject });
    const expectedCommand = producerOrder.includes(subject)
      ? `scripts/qualify-pr.sh prepare-part ${subject.slice("prepare-".length)}`
      : subject === "integrity" ? "scripts/qualify-pr.sh integrity" : `scripts/qualify-pr.sh gate ${subject}`;
    if (result.gateCommand !== expectedCommand) failures.push({ code: "gate-command-mismatch", subject });
    const actualBuilds = Array.isArray(result.buildInvocations) ? result.buildInvocations : [];
    const allowedBuilds = expectedBuildInvocations[subject] ?? [];
    if (actualBuilds.some((invocation) => !invocation.startsWith("producer:")) && !(Number.isSafeInteger(result.timingMilliseconds?.build) && result.timingMilliseconds.build > 0)) {
      failures.push({ code: "missing-build-duration", subject, ownerCommand: result.ownerCommand });
    }
    const allowedCounts = new Map(allowedBuilds.map((invocation) => [invocation, (allowedBuilds.filter((value) => value === invocation).length)]));
    const actualCounts = new Map(actualBuilds.map((invocation) => [invocation, (actualBuilds.filter((value) => value === invocation).length)]));
    for (const [invocation, count] of actualCounts) {
      const allowed = allowedCounts.get(invocation) ?? 0;
      if (allowed === 0) failures.push({ code: "unknown-build-invocation", subject, ownerCommand: result.ownerCommand, invocation });
      if (count > allowed) failures.push({ code: "duplicate-build-invocation", subject, ownerCommand: result.ownerCommand, invocation, expected: allowed, actual: count });
    }
    for (const [invocation, count] of allowedCounts) {
      const actual = actualCounts.get(invocation) ?? 0;
      if (actual < count) failures.push({ code: "missing-build-invocation", subject, ownerCommand: result.ownerCommand, invocation, expected: count, actual });
    }
  }
  for (const gate of byGate.keys()) if (!expected.has(gate)) failures.push({ code: "unexpected-gate-result", subject: gate });

  const globalBuildOwners = new Map();
  for (const subject of expectedSubjects) {
    for (const invocation of byGate.get(subject)?.buildInvocations ?? []) {
      if (invocation.includes(":exception:")) continue;
      const previous = globalBuildOwners.get(invocation);
      if (previous) failures.push({ code: "duplicate-build-invocation", subject, ownerCommand: byGate.get(subject)?.ownerCommand, invocation, previousOwner: previous });
      else globalBuildOwners.set(invocation, subject);
    }
  }

  const producerDigests = Object.fromEntries(expectedProducers.map((subject) => [subject.slice("prepare-".length), byGate.get(subject)?.artifactDigest ?? null]));
  for (const subject of expectedProducers) {
    const producerDigest = byGate.get(subject)?.artifactDigest;
    if (typeof producerDigest !== "string" || !/^[0-9a-f]{64}$/u.test(producerDigest)) failures.push({ code: "missing-prepared-artifact-binding", subject });
  }
  for (const subject of expectedSubjects) {
    const result = byGate.get(subject);
    if (!result) continue;
    const parts = gateParts[subject] ?? [];
    if (parts.length > 0) {
      const expectedBindings = canonicalArtifactBindings(Object.fromEntries(parts.map((part) => [part, producerDigests[part]])));
      if (canonical(canonicalArtifactBindings(result.artifactBindings)) !== canonical(expectedBindings)) failures.push({ code: "artifact-binding-mismatch", subject });
      if (result.artifactDigest !== digest(expectedBindings)) failures.push({ code: "artifact-set-digest-mismatch", subject });
    } else if (!producerOrder.includes(subject) && (result.artifactDigest !== null || Object.keys(result.artifactBindings ?? {}).length > 0)) failures.push({ code: "unexpected-artifact-binding", subject });
  }
  const elapsed = completedAtMilliseconds - startedAtMilliseconds;
  if (!Number.isSafeInteger(elapsed) || elapsed < 0) failures.push({ code: "invalid-feedback-duration", subject: "timing" });
  if (enforceBudget && Number.isSafeInteger(elapsed) && elapsed > feedbackBudgetMilliseconds) failures.push({ code: "feedback-budget-exceeded", subject: "timing", actual: elapsed, budget: feedbackBudgetMilliseconds });
  const ordered = subjectOrder.filter((gate) => byGate.has(gate)).map((gate) => byGate.get(gate));
  const validTotals = ordered.map((result) => result?.timingMilliseconds?.total).filter((value) => Number.isSafeInteger(value) && value >= 0);
  const observedGateCriticalPath = Math.max(0, ...validTotals);
  const criticalPath = Number.isSafeInteger(elapsed) && elapsed >= 0 ? elapsed : observedGateCriticalPath;
  const runnerMilliseconds = validTotals.reduce((sum, value) => sum + value, 0);
  const timing = {
    schema: timingSchema,
    subject: "runner-feedback",
    startedAtMilliseconds,
    completedAtMilliseconds,
    totalMilliseconds: Number.isSafeInteger(elapsed) && elapsed >= 0 ? elapsed : null,
    budgetMilliseconds: feedbackBudgetMilliseconds,
    criticalPathMilliseconds: criticalPath,
    observedGateCriticalPathMilliseconds: observedGateCriticalPath,
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
      transport: Number(one("transport-ms", "0")),
      test: Number(one("test-ms", "0")),
      total: Number(one("total-ms", "0")),
    }, {
      source: { commit: one("commit", "unknown"), tree: one("tree", "unknown") },
      routeDigest: one("route-digest", "unknown"),
      artifactBindings: Object.fromEntries(many("artifact-binding").map((declaration) => {
        const separator = declaration.indexOf("=");
        if (separator <= 0) throw new Error(`ci-route: malformed artifact binding:${declaration}`);
        return [declaration.slice(0, separator), declaration.slice(separator + 1)];
      }).sort(([left], [right]) => left.localeCompare(right))),
      artifactDigest: one("artifact-digest", "none") === "auto"
        ? digest(Object.fromEntries(many("artifact-binding").map((declaration) => declaration.split(/=(.*)/su).slice(0, 2)).sort(([left], [right]) => left.localeCompare(right))))
        : one("artifact-digest", "none") === "none" ? null : one("artifact-digest", ""),
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
    const joined = joinRoute(route, results, { startedAtMilliseconds: Number(one("started-ms", "0")), completedAtMilliseconds: Number(one("completed-ms", "0")), enforceBudget: one("enforce-budget", "true") === "true" });
    const output = one("output", undefined);
    if (output) await writeJson(output, joined);
    process.stdout.write(canonical(joined));
    if (joined.result !== "pass") process.exitCode = 1;
    return;
  }
  throw new Error("ci-route: usage route|gate|join [options]");
}

if (import.meta.url === pathToFileURL(process.argv[1]).href) main(process.argv.slice(2)).catch((error) => { console.error(error.message); process.exitCode = 1; });
