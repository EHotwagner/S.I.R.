import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname } from "node:path";
import { canonicalArtifactBindings, expectedBuildInvocations, expectedProducersFor, gateOrder, gateParts, producerOrder, subjectOrder, gateResult, joinRoute, routePaths, feedbackBudgetMilliseconds, feedbackAcceptanceTargetMilliseconds, feedbackHeadroomMilliseconds, feedbackBudgetFor, feedbackWaveCount, feedbackPipelineOverheadMilliseconds, feedbackWaveBudgetMilliseconds, feedbackHeadroomBasisPoints, subjectWave, routeSchema, gateSchema, joinSchema, timingSchema } from "./ci-route.mjs";
import { browserShardCapacityFor } from "./browser-shard-capacity.mjs";
import { mergeBrowserShardCases, parseBrowserShardJUnit } from "./browser-junit.mjs";

const route = (paths) => routePaths(paths, { commit: "a".repeat(40), tree: "b".repeat(40) });
const buildDocs = readFileSync(new URL("./build-docs.sh", import.meta.url), "utf8");

assert.equal(route(["docs/index.md"]).classification, "documentation");
assert.deepEqual(route(["docs/index.md"]).selectedGates, ["documentation", "evidence"]);
assert.equal(route(["docs/index.md", "work/220-bounded-pr-ci/spec.md"]).classification, "documentation");
assert.equal(route(["src/SIR.Domain/Rules.fs"]).classification, "domain");
assert.equal(route(["src/SIR.Domain/Rules.fs", "readiness/220-bounded-pr-ci/ship-verdict.json"]).classification, "domain");
assert.equal(route(["tests/SIR.Browser.Tests/journey.js"]).classification, "browser");
assert.deepEqual(route(["tests/SIR.Browser.Tests/journey.js"]).selectedGates, ["browser", "evidence"]);
const productionReviewRoute = route(["src/SIR.Client/UnifiedTacticalWorkspace.fs"]);
assert.equal(productionReviewRoute.classification, "browser");
assert.deepEqual(productionReviewRoute.selectedGates, ["browser", "documentation", "evidence"]);
assert.deepEqual(productionReviewRoute.productionReview, {
  required: true,
  rule: "RP-010-production-review-freshness",
  inputs: ["src/SIR.Client/UnifiedTacticalWorkspace.fs"],
});
assert.deepEqual(route(["src/SIR.Server/Program.fs"]).productionReview, {
  required: false,
  rule: "not-applicable",
  inputs: [],
});
for (const reviewOwnerPath of [
  "docs/assets/map-editor-review/manifest.json",
  "scripts/build-client.sh",
  "scripts/generate-map-editor-review.mjs",
]) {
  assert.deepEqual(route([reviewOwnerPath]).productionReview, {
    required: true,
    rule: "RP-010-production-review-freshness",
    inputs: [reviewOwnerPath],
  });
}
for (const broadReviewPath of [".github/workflows/ci.yml", "package-lock.json", "future/feature.xyz"]) {
  const broadReviewRoute = route([broadReviewPath]);
  assert.equal(broadReviewRoute.classification, "cross-cutting");
  assert.deepEqual(broadReviewRoute.productionReview.inputs, [broadReviewPath]);
  assert.equal(broadReviewRoute.productionReview.required, true);
}
assert.deepEqual(route(["src/SIR.Domain/Rules.fs", "docs/index.md"]).productionReview.inputs, ["docs/index.md", "src/SIR.Domain/Rules.fs"]);
assert.match(buildDocs, /if \[\[ -z "\$reuse_conformance_receipt" \]\]; then\n  \.\/scripts\/test-production-review-freshness-mutations\.sh\n  if \[\[ "\$prepared_pr" != true \]\]; then/u);
assert.deepEqual(route(["feedback/checkpoints/220.jsonl"]).selectedGates, ["evidence"]);
const performance = route(["scripts/measure-svg-pipeline.mjs", "scripts/lib/svg-pipeline-measurement.mjs", "docs/performance-budget.md", "work/231-svg-pipeline-measurement/spec.md"]);
assert.equal(performance.classification, "performance");
assert.deepEqual(performance.selectedGates, ["documentation", "evidence"]);
assert.ok(performance.facts.filter(({ classification }) => classification !== "evidence-only").every(({ classification }) => ["performance", "documentation"].includes(classification)));
for (const broadPolicyPath of ["scripts/ci-route.mjs", "scripts/test-ci-route.mjs", "scripts/qualify-pr.sh", ".github/workflows/ci.yml"]) assert.equal(route([broadPolicyPath]).classification, "cross-cutting", `${broadPolicyPath} must remain conservative`);
assert.equal(route(["scripts/audit-binding-exceptions.json", "feedback/audits/audit.json"]).classification, "evidence-only");
assert.equal(route(["scripts/measure-svg-pipeline.mjs", "src/SIR.Domain/Rules.fs"]).classification, "cross-cutting");
assert.equal(route(["scripts/measure-svg-pipeline.mjs", "scripts/ci-route.mjs"]).classification, "cross-cutting");
assert.equal(route(["src/SIR.Domain/Rules.fs", "docs/index.md"]).classification, "cross-cutting");
assert.equal(route(["future/feature.xyz"]).classification, "cross-cutting");
assert.equal(route([".\\docs\\index.md"]).classification, "documentation");
assert.throws(() => route([]), /inventory is empty/u);
assert.throws(() => route(["../secret"]), /malformed changed path/u);

const domain = route(["src/SIR.Domain/Rules.fs"]);
const producerDigests = { native: "c".repeat(64), fable: "d".repeat(64), web: "e".repeat(64), docs: "f".repeat(64) };
const bindingDigest = (bindings) => createHash("sha256").update(`${JSON.stringify(bindings, null, 2)}\n`).digest("hex");
const expectedSubjects = ["integrity", "prepare-native", "prepare-fable", "spatial-mutations", ...domain.selectedGates];
const resultFor = (gate, status = "pass", overrides = {}) => gateResult(gate, status, { setup: 100, restore: 200, build: 300, test: 400, total: 1_000 }, {
  source: domain.source,
  routeDigest: domain.digest,
  artifactBindings: Object.fromEntries((gateParts[gate] ?? []).map((part) => [part, producerDigests[part]])),
  artifactDigest: gate.startsWith("prepare-") ? producerDigests[gate.slice("prepare-".length)] : (gateParts[gate] ?? []).length > 0 ? bindingDigest(canonicalArtifactBindings(Object.fromEntries(gateParts[gate].map((part) => [part, producerDigests[part]])))) : null,
  receiptReused: (gateParts[gate] ?? []).length > 0,
  ...overrides,
});
const passing = expectedSubjects.map((gate, index) => resultFor(gate, "pass", { cacheHit: index === 0, buildInvocations: expectedBuildInvocations[gate] }));
assert.deepEqual(canonicalArtifactBindings({ web: "1", server: "2" }), canonicalArtifactBindings({ server: "2", web: "1" }));
assert.deepEqual(gateResult("browser", "pass", {}, { artifactBindings: { web: "1", server: "2" }, buildInvocations: ["z", "a"] }).buildInvocations, ["a", "z"]);
const joined = joinRoute(domain, passing, { startedAtMilliseconds: 1_000, completedAtMilliseconds: 239_000 });
assert.equal(joined.result, "pass");
assert.equal(joined.timing.subject, "runner-feedback");
assert.equal(joined.timing.receiptReuses, domain.selectedGates.length - 1);
assert.equal(joined.gateResults[0].timingMilliseconds.queue, null);
assert.equal(joined.gateResults[0].timingMilliseconds.transport, 0);
assert.equal(joined.timing.runnerMilliseconds, expectedSubjects.length * 1_000);
// S.I.R.#280: the ceiling is derived from the route's own dependency-wave depth, so the
// classification that selects the most gates is no longer the one held to the shortest
// deadline. `domain` spans two waves, so it earns the two-wave budget and no reserve.
const twoWaveBudget = feedbackPipelineOverheadMilliseconds + 2 * feedbackWaveBudgetMilliseconds;
assert.equal(feedbackWaveCount(expectedSubjects), 2);
assert.equal(feedbackBudgetFor(expectedSubjects), twoWaveBudget);
assert.equal(joined.timing.budgetMilliseconds, twoWaveBudget);
assert.equal(joined.timing.requiredHeadroomMilliseconds, Math.round((twoWaveBudget * feedbackHeadroomBasisPoints) / 10_000));
assert.equal(joined.timing.acceptanceTargetMilliseconds, twoWaveBudget - joined.timing.requiredHeadroomMilliseconds);
// Every fixture receipt totals 1s, so the attributable path is overhead + one wave-1 max
// + one wave-2 max. It is NOT the 238s of wall clock, and it is NOT Math.max over gates.
assert.equal(joined.timing.criticalPathMilliseconds, feedbackPipelineOverheadMilliseconds + 2_000);
assert.deepEqual(joined.timing.waveCriticalPathMilliseconds, [1_000, 1_000]);
assert.equal(joined.timing.totalMilliseconds, 238_000);
assert.equal(joined.timing.queuedMilliseconds, 238_000 - (feedbackPipelineOverheadMilliseconds + 2_000));
assert.equal(joined.timing.actualHeadroomMilliseconds, twoWaveBudget - (feedbackPipelineOverheadMilliseconds + 2_000));
assert.equal(joined.timing.acceptanceTargetMilliseconds, 312_000);
// Drive the attributable path by the gate work itself, which is the quantity now enforced.
const withTotal = (results, gate, total) => results.map((result) => result.gate === gate
  ? { ...result, timingMilliseconds: { ...result.timingMilliseconds, total } }
  : result);
const pathOf = (waveTwoTotal) => feedbackPipelineOverheadMilliseconds + 1_000 + waveTwoTotal;
const representativeTarget = twoWaveBudget - Math.round((twoWaveBudget * feedbackHeadroomBasisPoints) / 10_000);
// Every classification now reserves the same fraction of its own budget, so no route can be
// held to a tighter deadline than another that selects fewer gates at the same wave depth.
// This is the invariant that replaced the `["cross-cutting", "performance"]` special case.
const erodesAtTarget = joinRoute(domain, withTotal(passing, "spatial", representativeTarget - feedbackPipelineOverheadMilliseconds - 1_000 + 1), { startedAtMilliseconds: 0, completedAtMilliseconds: 1 });
assert.equal(erodesAtTarget.result, "fail");
assert.ok(erodesAtTarget.failures.some(({ code }) => code === "feedback-headroom-eroded"));
assert.ok(!erodesAtTarget.failures.some(({ code }) => code === "feedback-budget-exceeded"));
const performanceResultFor = (gate) => gateResult(gate, "pass", { setup: 100, restore: 200, build: 300, test: 400, total: 1_000 }, {
  source: performance.source,
  routeDigest: performance.digest,
  artifactBindings: Object.fromEntries((gateParts[gate] ?? []).map((part) => [part, producerDigests[part]])),
  artifactDigest: gate.startsWith("prepare-") ? producerDigests[gate.slice("prepare-".length)] : (gateParts[gate] ?? []).length > 0 ? bindingDigest(canonicalArtifactBindings(Object.fromEntries(gateParts[gate].map((part) => [part, producerDigests[part]])))) : null,
  receiptReused: (gateParts[gate] ?? []).length > 0,
  buildInvocations: expectedBuildInvocations[gate],
});
const performancePassing = ["integrity", "prepare-web", "prepare-docs", "documentation", "evidence"].map(performanceResultFor);
const performanceReserve = Math.round((twoWaveBudget * feedbackHeadroomBasisPoints) / 10_000);
const performanceAtTarget = withTotal(performancePassing, "documentation", representativeTarget - feedbackPipelineOverheadMilliseconds - 1_000);
assert.equal(joinRoute(performance, performanceAtTarget, { startedAtMilliseconds: 0, completedAtMilliseconds: 1 }).result, "pass");
const performanceHeadroomEroded = joinRoute(performance, withTotal(performancePassing, "documentation", representativeTarget - feedbackPipelineOverheadMilliseconds - 1_000 + 1), { startedAtMilliseconds: 0, completedAtMilliseconds: 1 });
assert.equal(performanceHeadroomEroded.result, "fail");
assert.ok(performanceHeadroomEroded.failures.some(({ code, target, requiredHeadroom }) => code === "feedback-headroom-eroded" && target === representativeTarget && requiredHeadroom === performanceReserve));
const crossCutting = route(["src/SIR.Domain/Rules.fs", "docs/index.md"]);
const crossSubjects = ["integrity", ...producerOrder, "spatial-mutations", "cancellation-mutations", "browser-general-helper", "browser-delivery", ...crossCutting.selectedGates];
const crossResultFor = (gate) => gateResult(gate, "pass", { setup: 100, restore: 200, build: 300, test: 400, total: 1_000 }, {
  source: crossCutting.source,
  routeDigest: crossCutting.digest,
  artifactBindings: Object.fromEntries((gateParts[gate] ?? []).map((part) => [part, producerDigests[part] ?? "e".repeat(64)])),
  artifactDigest: gate.startsWith("prepare-") ? (producerDigests[gate.slice("prepare-".length)] ?? "e".repeat(64)) : (gateParts[gate] ?? []).length > 0 ? bindingDigest(canonicalArtifactBindings(Object.fromEntries(gateParts[gate].map((part) => [part, producerDigests[part] ?? "e".repeat(64)])))) : null,
  receiptReused: (gateParts[gate] ?? []).length > 0,
  buildInvocations: expectedBuildInvocations[gate],
});
const crossPassing = crossSubjects.map(crossResultFor);
for (const helper of ["spatial-mutations", "cancellation-mutations", "browser-general-helper", "browser-delivery"]) {
  const missingHelper = joinRoute(crossCutting, crossPassing.filter((result) => result.gate !== helper), { completedAtMilliseconds: 1 });
  assert.ok(missingHelper.failures.some(({ code, subject }) => code === "missing-gate-result" && subject === helper));
  const failedHelper = joinRoute(crossCutting, crossPassing.map((result) => result.gate === helper ? { ...result, status: "fail", failureStage: "test" } : result), { completedAtMilliseconds: 1 });
  assert.ok(failedHelper.failures.some(({ code, subject }) => code === "required-gate-fail" && subject === helper));
}
const erodedHeadroom = joinRoute(crossCutting, withTotal(crossPassing, "browser", representativeTarget - feedbackPipelineOverheadMilliseconds - 1_000 + 1), { startedAtMilliseconds: 0, completedAtMilliseconds: 1 });
assert.equal(erodedHeadroom.result, "fail");
assert.ok(erodedHeadroom.failures.some(({ code, target, requiredHeadroom }) => code === "feedback-headroom-eroded" && target === representativeTarget && requiredHeadroom === Math.round((twoWaveBudget * feedbackHeadroomBasisPoints) / 10_000)));
assert.ok(!erodedHeadroom.failures.some(({ code }) => code === "feedback-budget-exceeded"));
const overBudget = joinRoute(domain, withTotal(passing, "spatial", twoWaveBudget - feedbackPipelineOverheadMilliseconds - 1_000 + 1), { startedAtMilliseconds: 0, completedAtMilliseconds: 1 });
assert.equal(overBudget.result, "fail");
assert.ok(overBudget.failures.some(({ code, actual, budget }) => code === "feedback-budget-exceeded" && actual === twoWaveBudget + 1 && budget === twoWaveBudget));
// The enforced quantity must be the gate work, never the wall clock: a run whose gates all
// finished quickly is admitted no matter how long the fleet made it wait, and a run whose
// gates genuinely overran is refused even when the wall clock is small.
assert.equal(joinRoute(domain, passing, { startedAtMilliseconds: 0, completedAtMilliseconds: 86_400_000 }).result, "pass");
assert.equal(pathOf(twoWaveBudget), twoWaveBudget + feedbackPipelineOverheadMilliseconds + 1_000);
// A receipt that carries no usable duration cannot silently neutralise the timing gate.
const blindTiming = joinRoute(domain, withTotal(passing, "spatial", 0), { startedAtMilliseconds: 0, completedAtMilliseconds: 1 });
assert.ok(blindTiming.failures.some(({ code, subject }) => code === "missing-feedback-timing" && subject === "spatial"));
const aggregate = joinRoute(domain, [passing[0], passing[1], resultFor("spatial", "fail", { routeDigest: "e".repeat(64), artifactBindings: { native: "f".repeat(64), fable: producerDigests.fable } }), resultFor("browser")], { completedAtMilliseconds: 1 });
assert.equal(aggregate.result, "fail");
for (const code of ["missing-gate-result", "required-gate-fail", "route-binding-mismatch", "unexpected-gate-result", "artifact-binding-mismatch"]) assert.ok(aggregate.failures.some((failure) => failure.code === code), code);
const staleCandidate = joinRoute(domain, passing.map((result) => result.gate === "rules" ? { ...result, source: { ...result.source, commit: "f".repeat(40) } } : result), { completedAtMilliseconds: 1 });
assert.ok(staleCandidate.failures.some(({ code }) => code === "candidate-binding-mismatch"));
const tamperedRoute = { ...domain, paths: ["src/SIR.Domain/Tampered.fs"] };
const staleRoute = joinRoute(tamperedRoute, passing, { completedAtMilliseconds: 1 });
assert.ok(staleRoute.failures.some(({ code }) => code === "route-digest-mismatch"));
const duplicateUnknownBuild = joinRoute(domain, passing.map((result) => result.gate === "rules" ? {
  ...result,
  buildInvocations: ["build:SIR.slnx", "run-build:tests/Unknown.fsproj"],
} : result), { completedAtMilliseconds: 1 });
assert.ok(duplicateUnknownBuild.failures.some(({ code, invocation }) => code === "duplicate-build-invocation" && invocation === "build:SIR.slnx"));
assert.ok(duplicateUnknownBuild.failures.some(({ code, invocation }) => code === "unknown-build-invocation" && invocation === "run-build:tests/Unknown.fsproj"));
const missingBuildDuration = joinRoute(domain, passing.map((result) => result.gate === "prepare-native" ? {
  ...result,
  timingMilliseconds: { ...result.timingMilliseconds, build: 0 },
} : result), { completedAtMilliseconds: 1 });
assert.ok(missingBuildDuration.failures.some(({ code, subject }) => code === "missing-build-duration" && subject === "prepare-native"));
const spatialBuilds = expectedBuildInvocations["spatial-mutations"];
const withSpatialBuilds = (buildInvocations) => passing.map((result) => result.gate === "spatial-mutations" ? { ...result, buildInvocations } : result);
const anonymousSpatial = "build:src/SIR.Simulation/SIR.Simulation.fsproj";
const anonymousSpatialTrace = joinRoute(domain, withSpatialBuilds(Array(spatialBuilds.length).fill(anonymousSpatial)), { completedAtMilliseconds: 1 });
assert.ok(anonymousSpatialTrace.failures.some(({ code, invocation }) => code === "unknown-build-invocation" && invocation === anonymousSpatial));
assert.ok(anonymousSpatialTrace.failures.some(({ code, invocation, expected, actual }) => code === "duplicate-build-invocation" && invocation === anonymousSpatial && expected === 0 && actual === spatialBuilds.length));
assert.equal(anonymousSpatialTrace.failures.filter(({ code, subject }) => code === "missing-build-invocation" && subject === "spatial-mutations").length, spatialBuilds.length);
const missingSpatialTrace = joinRoute(domain, withSpatialBuilds(spatialBuilds.slice(0, -1)), { completedAtMilliseconds: 1 });
assert.ok(missingSpatialTrace.failures.some(({ code, invocation }) => code === "missing-build-invocation" && invocation === spatialBuilds.at(-1)));
const duplicatedSpatialBuilds = [...spatialBuilds];
duplicatedSpatialBuilds[duplicatedSpatialBuilds.length - 1] = duplicatedSpatialBuilds[0];
const duplicatedSpatialTrace = joinRoute(domain, withSpatialBuilds(duplicatedSpatialBuilds), { completedAtMilliseconds: 1 });
assert.ok(duplicatedSpatialTrace.failures.some(({ code, invocation, expected, actual }) => code === "duplicate-build-invocation" && invocation === spatialBuilds[0] && expected === 1 && actual === 2));
assert.ok(duplicatedSpatialTrace.failures.some(({ code, invocation }) => code === "missing-build-invocation" && invocation === spatialBuilds.at(-1)));
assert.throws(() => gateResult("unknown", "pass"), /unknown gate result/u);

const workflow = readFileSync(new URL("../.github/workflows/ci.yml", import.meta.url), "utf8");
const routeSource = readFileSync(new URL("./ci-route.mjs", import.meta.url), "utf8");
// ONE parse of the workflow's job graph, shared by every assertion below -- the body, the job's
// own `if:` (inline or folded), and its `needs:`. A lazy body match needs a real terminator: `$`
// under /m/ matches the end of EVERY line, so an alternation with it truncates each body at its
// first newline while still looking like it read the job. The sentinel job is that terminator,
// and it is also why the LAST job in the file now has a body at all.
const workflowJobs = (() => {
  const section = `${workflow.slice(workflow.indexOf("\njobs:\n"))}\n  zzsentinel:\n`;
  const parsed = {};
  for (const [, name, body] of section.matchAll(/^  ([a-z][a-z0-9-]*):\n([\s\S]*?)(?=^  [a-z][a-z0-9-]*:\n)/gmu)) {
    if (name === "zzsentinel") continue;
    const head = /^    if: (.*)$/mu.exec(body);
    let condition = head ? head[1].trim() : null;
    if (condition !== null && /^[>|][-+]?$/u.test(condition)) {
      // A folded block scalar joins its continuation lines with spaces: one expression, written
      // over several lines because a guard with one clause per producer does not fit on one.
      const lines = [];
      for (const line of body.slice(head.index + head[0].length + 1).split("\n")) {
        if (line.trim() === "") continue;
        if (line.length - line.trimStart().length <= 4) break;
        lines.push(line.trim());
      }
      condition = lines.join(" ");
    }
    const needsRaw = /^    needs: (.+)$/mu.exec(body)?.[1]?.trim() ?? null;
    parsed[name] = {
      name,
      body,
      if: condition,
      needs: needsRaw === null ? [] : needsRaw.startsWith("[")
        ? needsRaw.slice(1, -1).split(",").map((entry) => entry.trim()).filter(Boolean)
        : [needsRaw],
    };
  }
  return parsed;
})();
const jobBody = (name) => workflowJobs[name]?.body ?? "";
const contracts = JSON.parse(readFileSync(new URL("../tests/fixtures/ci-qualification/v1/contracts.json", import.meta.url), "utf8"));
assert.equal(contracts.feedbackBudgetMilliseconds, feedbackBudgetMilliseconds);
assert.equal(contracts.feedbackAcceptanceTargetMilliseconds, feedbackAcceptanceTargetMilliseconds);
assert.equal(contracts.feedbackHeadroomMilliseconds, feedbackHeadroomMilliseconds);
assert.equal(contracts.feedbackPipelineOverheadMilliseconds, feedbackPipelineOverheadMilliseconds);
assert.equal(contracts.feedbackWaveBudgetMilliseconds, feedbackWaveBudgetMilliseconds);
assert.equal(contracts.feedbackHeadroomBasisPoints, feedbackHeadroomBasisPoints);
assert.equal(contracts.feedbackWaveCount, feedbackWaveCount(subjectOrder));
// The flat constants remain the single-wave reference the shaped budget floors at.
assert.equal(feedbackAcceptanceTargetMilliseconds, feedbackBudgetMilliseconds - feedbackHeadroomMilliseconds);
assert.equal(Math.round((feedbackBudgetMilliseconds * feedbackHeadroomBasisPoints) / 10_000), feedbackHeadroomMilliseconds);
assert.deepEqual(contracts.gateOrder, gateOrder);
assert.deepEqual(contracts.subjectOrder, subjectOrder);
assert.deepEqual(expectedBuildInvocations["cancellation-mutations"], [
  "build:tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj:exception:cancellation-fixture:artifacts-path:isolated",
  "fable:src/SIR.Client.Web/SIR.RulesExplorer.Web.fsproj:exception:cancellation-mutant",
  "fable:src/SIR.Replay.Web/SIR.Replay.Web.fsproj:exception:cancellation-mutant",
]);
assert.deepEqual(spatialBuilds, [
  "build:tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj:exception:spatial-mutation-base",
  ...[
    "dependency-receipt", "footprint-envelope", "semantic-edge", "knowledge-cache-key", "spatial-revision-key",
    "deterministic-ordering", "package-adapter", "profile-cache-key", "trace-work-bound",
  ].map((name) => `build:src/SIR.Simulation/SIR.Simulation.fsproj:exception:spatial-${name}:artifacts-path:isolated`),
]);
assert.deepEqual(expectedBuildInvocations.spatial, []);
assert.deepEqual(expectedBuildInvocations["prepare-native"], [
  "build:SIR.slnx",
  "producer:native",
]);
const duplicatedSolutionOwner = joinRoute(domain, passing.map((result) => result.gate === "prepare-native" ? {
  ...result,
  buildInvocations: [...result.buildInvocations, "build:SIR.slnx"],
} : result), { completedAtMilliseconds: 1 });
assert.ok(duplicatedSolutionOwner.failures.some(({ code, invocation }) =>
  code === "duplicate-build-invocation" && invocation === "build:SIR.slnx"));
const redundantProjectOwner = joinRoute(domain, passing.map((result) => result.gate === "prepare-native" ? {
  ...result,
  buildInvocations: [...result.buildInvocations, "build:tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"],
} : result), { completedAtMilliseconds: 1 });
assert.ok(redundantProjectOwner.failures.some(({ code, invocation }) =>
  code === "unknown-build-invocation" && invocation === "build:tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj"));
assert.deepEqual(expectedBuildInvocations["prepare-fable"], [
  "fable:tests/SIR.Client.Tests/ScenarioCatalogRuntime.fsproj",
  "fable:tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj",
  "fable:tests/SIR.ModalInput.Fable.Tests/SIR.ModalInput.Fable.Tests.fsproj",
  "producer:fable",
]);
assert.deepEqual(expectedBuildInvocations["prepare-docs"], [
  "build:src/SIR.Match/SIR.Match.fsproj",
  "build:src/SIR.Client/SIR.Client.fsproj",
  "producer:docs",
]);
const duplicatedScenarioOwner = joinRoute(domain, passing.map((result) => result.gate === "prepare-fable" ? {
  ...result,
  buildInvocations: [...result.buildInvocations, "fable:tests/SIR.Client.Tests/ScenarioCatalogRuntime.fsproj"],
} : result), { completedAtMilliseconds: 1 });
assert.ok(duplicatedScenarioOwner.failures.some(({ code, invocation }) =>
  code === "duplicate-build-invocation" && invocation === "fable:tests/SIR.Client.Tests/ScenarioCatalogRuntime.fsproj"));
const unknownScenarioOwner = joinRoute(domain, passing.map((result) => result.gate === "prepare-fable" ? {
  ...result,
  buildInvocations: [...result.buildInvocations, "fable:tests/SIR.Client.Tests/UnknownRuntime.fsproj"],
} : result), { completedAtMilliseconds: 1 });
assert.ok(unknownScenarioOwner.failures.some(({ code, invocation }) =>
  code === "unknown-build-invocation" && invocation === "fable:tests/SIR.Client.Tests/UnknownRuntime.fsproj"));
assert.deepEqual(contracts.timingPhases, ["queue", "setup", "restore", "build", "transport", "test", "total"]);
assert.deepEqual(contracts.schemas, { route: routeSchema, artifactManifest: "sir.ci-artifact-manifest/v2", gateResult: gateSchema, timing: timingSchema, join: joinSchema });
for (const job of ["route:", "integrity:", "spatial-mutations:", "cancellation-mutations:", "prepare-native:", "prepare-fable:", "prepare-web:", "prepare-docs:", "domain-conformance:", "cross-runtime:", "browser:", "browser-general-helper:", "browser-delivery:", "documentation:", "collection-strategies:", "pr-verdict:", "cost-observer:", "protected-preflight:", "full-qualification:", "protected-verdict:", "integrity-sweep:"]) assert.match(workflow, new RegExp(`^  ${job}$`, "mu"));
for (const removedJob of ["browser-general-helper-2", "browser-general-helper-3"]) assert.doesNotMatch(workflow, new RegExp(`^  ${removedJob}:$`, "mu"));
assert.match(workflow, /if: always\(\)/u);
assert.match(workflow, /if: always\(\) && github\.event_name == 'pull_request'/u);
assert.match(workflow, /schedule:/u);
assert.match(workflow, /cancel-in-progress: \$\{\{ github\.event_name == 'pull_request' \}\}/u);
assert.match(workflow, /hashFiles\('\*\*\/packages\.lock\.json', 'Directory\.Packages\.props', '\.config\/dotnet-tools\.json'\)/u);
// S.I.R.#263 added the `collection-strategies` job, so 19 became 20. An absolute count, updated
// deliberately: it is what goes red if a job is added or removed without a look.
assert.equal(workflow.match(/name: Capture runner start/gu)?.length, 20);
for (const part of ["native", "fable", "web", "docs"]) assert.match(workflow, new RegExp(`qualify-pr\\.sh prepare-part ${part}`, "u"));
assert.doesNotMatch(workflow, /qualify-pr\.sh prepare-part server/u);
// S.I.R.#304. This line used to read `/classification != 'evidence-only'/` -- it PINNED the
// defect. `prepare-native` ran on that negation while the join's expected set, derived from the
// selected gates' `gateParts`, never listed prepare-native for `documentation` or `performance`.
// Both classifications therefore ran a producer the join refused, and `pr-verdict` reported
// `unexpected-gate-result: prepare-native`: two of six classifications could not pass at all.
// The suite could not catch it because its join fixtures were built from the EXPECTED set, so
// they never modelled what ci.yml actually runs. The agreement block further down closes that.
assert.match(jobBody("prepare-native"), /if: needs\.route\.outputs\.prepare_native == 'true'/u);
assert.doesNotMatch(workflow, /^  prepare:$/mu);
assert.doesNotMatch(workflow, /prepared-candidate/u);
assert.match(workflow, /domain-conformance:\n[\s\S]*?needs: \[route, prepare-native, prepare-fable, prepare-web\]/u);
// S.I.R.#309 made the extracted part set DERIVED rather than the literal `native fable web`.
// The literal was the consumption half of the same defect: a `domain` route prepares no `web`
// part, so an unconditional `extract-parts ... web` turns the skip this job stopped being into
// a red download step instead. The gate list itself is unchanged and still pinned in order.
assert.match(jobBody("domain-conformance"), /parts\+=\(native\)[\s\S]*parts\+=\(fable\)[\s\S]*parts\+=\(web\)[\s\S]*extract-parts "\$\{parts\[@\]\}"[\s\S]*gates\+=\(rules\)[\s\S]*gates\+=\(spatial\)[\s\S]*gates\+=\(cancellation\)[\s\S]*for gate in[\s\S]*run-ci-gate\.sh "\$gate"[\s\S]*for pid in[^\n]*wait[^\n]*done[\s\S]*run-ci-gate\.sh browser-delivery[\s\S]*gate-domain-conformance/u);
assert.doesNotMatch(jobBody("domain-conformance"), /extract-parts native fable web/u,
  "the extracted part set must be derived from the route's producer flags, never a fixed literal");
assert.doesNotMatch(jobBody("domain-conformance"), /gates\+=\(browser-delivery\)/u);
assert.doesNotMatch(jobBody("domain-conformance"), /actions\/cache@|npm ci|dotnet tool restore|cache: npm/u);
assert.doesNotMatch(jobBody("domain-conformance"), /gates\+=\(cross-runtime\)/u);
assert.doesNotMatch(jobBody("domain-conformance"), /run-ci-gate\.sh cross-runtime/u);
assert.match(jobBody("cross-runtime"), /needs: \[route, prepare-native, prepare-fable\][\s\S]*prepared-part-native[\s\S]*prepared-part-fable[\s\S]*run-ci-gate\.sh cross-runtime/u);
assert.doesNotMatch(jobBody("cross-runtime"), /actions\/cache@|nuget-cache|dotnet tool restore/u);
assert.match(jobBody("cross-runtime"), /SIR_CI_CACHE_HIT: "false"/u);
assert.doesNotMatch(workflow, /^  spatial:$/mu);
assert.match(jobBody("prepare-web"), /if: needs\.route\.outputs\.prepare_web == 'true'/u);
for (const browserJob of ["browser", "browser-general-helper", "browser-delivery"]) assert.match(jobBody(browserJob), /needs: \[route, prepare-web, prepare-native\]/u);
assert.match(jobBody("browser-delivery"), /if: needs\.route\.outputs\.browser == 'true' && needs\.route\.outputs\.rules != 'true' && needs\.route\.outputs\.spatial != 'true' && needs\.route\.outputs\.cancellation != 'true'/u);
assert.match(workflow, /documentation:\n[\s\S]*?needs: \[route, prepare-web, prepare-docs\]/u);
for (const browserJob of ["browser", "browser-general-helper", "browser-delivery"]) {
  assert.doesNotMatch(jobBody(browserJob), /actions\/setup-dotnet@/u);
  assert.match(jobBody(browserJob), /dotnet --list-runtimes \| grep -F 'Microsoft\.NETCore\.App 10\.0\.'/u);
  assert.match(jobBody(browserJob), /prepared-part-web/u);
  assert.match(jobBody(browserJob), /prepared-part-native/u);
  assert.doesNotMatch(jobBody(browserJob), /prepared-part-(?:fable|server|docs)/u);
}
// The documentation gate and its producer key off the route's `documentation` OUTPUT, never the
// classification. A `browser` or `cross-cutting` route that requires production-review freshness
// sets that output without being classified `documentation` (RP-010), so gating the jobs on the
// classification would silently drop the documentation gate for exactly those routes.
assert.match(jobBody("documentation"), /if: needs\.route\.outputs\.documentation == 'true'/u);
// S.I.R.#304 moved `prepare-docs` onto its own derived flag, so the RP-010 property is now stated
// as the equivalence it always meant: the docs PRODUCER runs exactly when the documentation GATE
// is selected -- including the production-review-freshness route, which selects that gate without
// being classified `documentation`. `docs` appears in no other gate's parts, so the derived flag
// carries the guarantee the literal `documentation == 'true'` used to carry here.
assert.match(jobBody("prepare-docs"), /if: needs\.route\.outputs\.prepare_docs == 'true'/u);
for (const documentationJob of ["documentation", "prepare-docs"]) {
  assert.doesNotMatch(jobBody(documentationJob), /classification == 'documentation'/u);
}
for (const sample of [["docs/index.md"], ["scripts/measure-svg-pipeline.mjs"], ["src/SIR.Domain/Rules.fs"],
  ["tests/SIR.Browser.Tests/journey.js"], [".github/workflows/ci.yml"], ["work/220-bounded-pr-ci/spec.md"],
  ["src/SIR.Client/UnifiedTacticalWorkspace.fs"]]) {
  const candidate = route(sample);
  assert.equal(expectedProducersFor(candidate.selectedGates).includes("prepare-docs"), candidate.selectedGates.includes("documentation"),
    `${sample}: the docs producer must run exactly when the documentation gate is selected`);
}
assert.ok(route(["src/SIR.Client/UnifiedTacticalWorkspace.fs"]).selectedGates.includes("documentation"));
assert.notEqual(route(["src/SIR.Client/UnifiedTacticalWorkspace.fs"]).classification, "documentation");
assert.match(jobBody("documentation"), /prepared-part-docs/u);
assert.match(jobBody("documentation"), /prepared-part-web/u);
assert.doesNotMatch(jobBody("documentation"), /prepared-part-native/u);
assert.match(jobBody("documentation"), /actions\/setup-dotnet@[0-9a-f]{40} # v6\.0\.0/u);
assert.match(jobBody("spatial-mutations"), /needs: route/u);
assert.match(jobBody("spatial-mutations"), /run-ci-gate\.sh spatial-mutations/u);
assert.doesNotMatch(jobBody("spatial-mutations"), /prepared-part-|needs: \[[^\]]*prepare-/u);
assert.doesNotMatch(jobBody("spatial-mutations"), /npm ci|dotnet tool restore/u);
assert.match(jobBody("cancellation-mutations"), /npm ci --ignore-scripts[\s\S]*dotnet tool restore/u);
assert.match(jobBody("cancellation-mutations"), /needs: route/u);
assert.match(jobBody("cancellation-mutations"), /run-ci-gate\.sh cancellation-mutations/u);
assert.doesNotMatch(jobBody("cancellation-mutations"), /prepared-part-|needs: \[[^\]]*prepare-/u);
const gateRunner = readFileSync(new URL("./run-ci-gate.sh", import.meta.url), "utf8");
assert.match(gateRunner, /spatial\|cross-runtime\) preflight_parts=\(native fable\)/u);
assert.match(gateRunner, /cancellation\) preflight_parts=\(native web\)/u);
assert.match(gateRunner, /browser\|browser-general-helper\|browser-delivery\) preflight_parts=\(web native\)/u);
assert.match(gateRunner, /documentation\) preflight_parts=\(web docs\)/u);
assert.match(gateRunner, /SIR_CI_PREFLIGHT_REUSED/u);
// The consumer honouring the flag is only half the contract. domain-conformance extracts the
// prepared parts ONCE and then runs several gates concurrently in that same job, so it must also
// SET the flag; without it each of those gates re-extracts the same artifacts.
// S.I.R.#309 made the part LIST derived, so this states the "once" property directly instead of
// through a literal that no longer appears: exactly one extraction, and it precedes the gate loop.
assert.equal(jobBody("domain-conformance").match(/qualify-pr\.sh extract-parts/gu)?.length, 1,
  "domain-conformance must extract the prepared parts exactly once, before its concurrent gates");
assert.ok(jobBody("domain-conformance").indexOf("extract-parts") < jobBody("domain-conformance").indexOf("for gate in"),
  "the single extraction must precede the gate loop that reuses it");
assert.match(jobBody("domain-conformance"), /SIR_CI_PREFLIGHT_REUSED: "true"/u);
assert.match(gateRunner, /ci-gate-artifact-bindings\.sh" "\$repo_root" "\$\{preflight_parts\[@\]\}"/u);
assert.match(workflow, /pr-verdict:\n[\s\S]*?needs: \[[^\]]*prepare-docs[^\]]*spatial-mutations[^\]]*cancellation-mutations[^\]]*browser-general-helper[^\]]*browser-delivery[^\]]*domain-conformance[^\]]*cross-runtime[^\]]*\]/u);
assert.match(jobBody("browser"), /SIR_JUNIT_OUTPUT: artifacts\/ci\/results\/browser-general-1\.junit\.xml[\s\S]*SIR_JUNIT_OUTPUT_2: artifacts\/ci\/results\/browser-general-2\.junit\.xml/u);
assert.match(jobBody("browser-general-helper"), /SIR_JUNIT_OUTPUT: artifacts\/ci\/results\/browser-general-3\.junit\.xml[\s\S]*SIR_JUNIT_OUTPUT_2: artifacts\/ci\/results\/browser-general-4\.junit\.xml/u);
assert.match(jobBody("pr-verdict"), /test-browser-global-merge\.mjs[\s\S]*browser-general-1\.junit\.xml[\s\S]*browser-general-2\.junit\.xml[\s\S]*browser-general-3\.junit\.xml[\s\S]*browser-general-4\.junit\.xml/u);
assert.match(jobBody("pr-verdict"), /browser_merge_status=\$\?[\s\S]*rm -f artifacts\/ci\/results\/browser-general-helper\.json/u);
// S.I.R.#263/F3. The literal restatement of pr-verdict's subject loop that stood here is DELETED.
// It was the #304 duplicate this change exists to remove -- a second declaration of `subjectOrder`
// maintained by hand -- and it had already gone stale: it omitted `collection-strategies` and passed
// anyway, because a substring match against a longer list is a prefix match. It also fired BEFORE the
// derived check below, masking that check's refusal message on every unreadable-input case. The
// equality asserted further down reads the loop out of this same workflow and needs no literal twin.
assert.match(workflow, /\.\/scripts\/qualify-production\.sh --protected/u);
const fullQualification = readFileSync(new URL("./qualify-production.sh", import.meta.url), "utf8");
assert.ok(fullQualification.indexOf("dotnet restore SIR.slnx --locked-mode") < fullQualification.indexOf("test-conformance.sh"));
assert.ok(fullQualification.indexOf("dotnet-invocation-trace.sh") < fullQualification.indexOf("test-worker-cancellation-subject-mutation.sh"));
assert.match(fullQualification, /verify-spatial-query\.sh --static-only/u);
assert.match(fullQualification, /fable_target_builds=.*\.total/u);
const focusedQualification = readFileSync(new URL("./qualify-pr.sh", import.meta.url), "utf8");
for (const browserJob of ["browser", "browser-general-helper", "browser-delivery"]) {
  assert.doesNotMatch(jobBody(browserJob), /npm ci/u);
  assert.doesNotMatch(jobBody(browserJob), /cache: npm/u);
}
assert.match(focusedQualification, /web\)[\s\S]*node_modules\/@playwright\/test[\s\S]*node_modules\/playwright[\s\S]*node_modules\/playwright-core[\s\S]*;;/u);
const conformanceQualification = readFileSync(new URL("./test-conformance.sh", import.meta.url), "utf8");
const spatialMutationQualification = readFileSync(new URL("./test-spatial-subject-mutations.sh", import.meta.url), "utf8");
const cancellationMutationQualification = readFileSync(new URL("./test-worker-cancellation-subject-mutation.sh", import.meta.url), "utf8");
const spatialQualification = readFileSync(new URL("./verify-spatial-query.sh", import.meta.url), "utf8");
const simulationProject = readFileSync(new URL("../src/SIR.Simulation/SIR.Simulation.fsproj", import.meta.url), "utf8");
const browserConfiguration = readFileSync(new URL("../tests/SIR.Browser.Tests/playwright.config.js", import.meta.url), "utf8");
const browserShards = readFileSync(new URL("./test-browser-shards.mjs", import.meta.url), "utf8");
const browserJunit = readFileSync(new URL("./browser-junit.mjs", import.meta.url), "utf8");
const browserGlobalMerge = readFileSync(new URL("./test-browser-global-merge.mjs", import.meta.url), "utf8");
const liveSession = readFileSync(new URL("../src/SIR.Client.Web/LiveSession.fs", import.meta.url), "utf8");
const reconnectLifecycle = liveSession.slice(liveSession.indexOf("    let reconnect"));
const productionDelivery = readFileSync(new URL("../tests/SIR.Browser.Tests/production-delivery.spec.js", import.meta.url), "utf8");
const packageManifest = JSON.parse(readFileSync(new URL("../package.json", import.meta.url), "utf8"));
const matchQualification = readFileSync(new URL("../tests/SIR.Match.Tests/Program.fs", import.meta.url), "utf8");
const focusedNativeProducer = /      native\)\n(?<body>[\s\S]*?)\n        ;;/u.exec(focusedQualification);
assert.ok(focusedNativeProducer?.groups?.body, "focused native producer block is missing");
assert.equal(focusedNativeProducer.groups.body.match(/dotnet build SIR\.slnx -c Release --no-restore/gu)?.length, 1);
assert.doesNotMatch(focusedNativeProducer.groups.body, /build-docs\.sh|artifacts\/site/u);
const focusedDocsProducer = [...focusedQualification.matchAll(/      docs\)\n(?<body>[\s\S]*?)\n        ;;/gu)]
  .find((match) => match.groups?.body.includes("dotnet build src/SIR.Match/SIR.Match.fsproj"));
assert.ok(focusedDocsProducer?.groups?.body, "focused docs producer block is missing");
assert.match(focusedDocsProducer.groups.body, /dotnet build src\/SIR\.Match\/SIR\.Match\.fsproj -c Release --no-restore[\s\S]*dotnet build src\/SIR\.Client\/SIR\.Client\.fsproj -c Release --no-restore[\s\S]*build-docs\.sh --prepare-site-only[\s\S]*artifacts\/site/u);
assert.match(focusedQualification, /documentation\)[\s\S]*build-docs\.sh[^\n]*--prepared-pr --reuse-site-build/u);
assert.match(buildDocs, /--prepare-site-only[\s\S]*build_site_projection[\s\S]*--reuse-site-build[\s\S]*prepared site projection is missing index\.html/u);
assert.doesNotMatch(focusedQualification, /find src tests -type d.*-name obj/u);
assert.doesNotMatch(focusedQualification, /--output .*obj/u);
const producerRestoreBlock = /    # Restore only the graph owned by this producer\.[\s\S]*?    restore_completed=/u.exec(focusedQualification)?.[0] ?? "";
assert.match(focusedQualification, /case "\$part" in\n      fable\|web\|docs\) dotnet tool restore ;;/u);
assert.match(producerRestoreBlock, /native\) dotnet restore SIR\.slnx --locked-mode/u);
for (const project of [
  "tests/SIR.Domain.Fable.Tests/SIR.Domain.Fable.Tests.fsproj",
  "tests/SIR.ModalInput.Fable.Tests/SIR.ModalInput.Fable.Tests.fsproj",
  "tests/SIR.Client.Tests/ScenarioCatalogRuntime.fsproj",
  "src/SIR.Replay.Web/SIR.Replay.Web.fsproj",
  "src/SIR.Client.Web/SIR.RulesExplorer.Web.fsproj",
  "src/SIR.Server/SIR.Server.fsproj",
  "src/SIR.Match/SIR.Match.fsproj",
  "src/SIR.Client/SIR.Client.fsproj",
]) assert.match(producerRestoreBlock, new RegExp(`dotnet restore ${project.replaceAll(".", "\\.")} --locked-mode`, "u"));
assert.equal(producerRestoreBlock.match(/dotnet restore SIR\.slnx --locked-mode/gu)?.length, 1);
// The published server payload is the composition root: the native producer publishes into it,
// the client is copied into its wwwroot, and BOTH the compose and verify steps must read that
// same client/publish pair. Losing the publish root leaves the composition checking an empty tree.
assert.match(focusedQualification, /dotnet publish src\/SIR\.Server\/SIR\.Server\.fsproj -c Release -o artifacts\/publish/u);
assert.match(focusedQualification, /cp -a artifacts\/client\/\. artifacts\/publish\/wwwroot\//u);
assert.equal(focusedQualification.match(/--client artifacts\/client --publish artifacts\/publish/gu)?.length, 2);
assert.match(focusedQualification, /trap write_part_timing EXIT/u);
assert.match(focusedQualification, /verify-staged[\s\S]*cp -a "\$stage\/\$output_path" "\$target"/u);
assert.match(focusedQualification, /run_browser_shard_pair[\s\S]*SIR_BROWSER_SHARD_INDEX="\$first_index"[\s\S]*SIR_BROWSER_SHARD_INDEX="\$second_index"[\s\S]*wait "\$first_pid"[\s\S]*wait "\$second_pid"/u);
assert.match(focusedQualification, /browser\)[\s\S]*compose-browser[\s\S]*run_browser_shard_pair 1 2/u);
assert.match(focusedQualification, /browser-general-helper\)[\s\S]*compose-browser[\s\S]*run_browser_shard_pair 3 4/u);
assert.match(focusedQualification, /browser-delivery\)[\s\S]*compose-browser[\s\S]*SIR_BROWSER_COHORT=production-delivery npm run test:browser/u);
assert.match(focusedQualification, /SIR_BROWSER_SHARDS=1 SIR_BROWSER_COHORT=production-delivery/u);
assert.match(focusedQualification, /verify-browser-composition/u);
assert.doesNotMatch(focusedNativeProducer.groups.body, /dotnet build (?:tests|src)\/[^\n]+\.fsproj/u);
assert.match(focusedQualification, /tests\/SIR\.Rules\.Governance\.Tests\/bin\/Release\/net10\.0/u);
assert.match(focusedQualification, /src\/SIR\.Simulation\/Governance\.Tool\/bin\/Release\/net10\.0/u);
const focusedRulesGate = /      rules\)\n(?<body>[\s\S]*?)\n        ;;/u.exec(focusedQualification);
assert.ok(focusedRulesGate?.groups?.body, "focused rules gate block is missing");
for (const command of [
  "SIR_RULES_PREPARED_PR=1 ./scripts/verify-rules-corpus.sh",
  "SIR_RULES_PREPARED_PR=1 dotnet run --project tests/SIR.Rules.Governance.Tests/SIR.Rules.Governance.Tests.fsproj -c Release --no-build --no-restore",
  "SIR_RULES_PREPARED_PR=1 ./scripts/test-rules-governance-tool-mutations.sh",
  "SIR_RULES_PREPARED_PR=1 ./scripts/generate-rules-governance.sh --check",
]) assert.match(focusedRulesGate.groups.body, new RegExp(command.replaceAll(".", "\\."), "u"));
assert.match(focusedQualification, /spatial-mutations\)[\s\S]*test-spatial-subject-mutations\.sh --prepared-pr/u);
assert.match(focusedQualification, /spatial\).*--prepared-pr --prepared-parts-verified --external-mutation-proof/u);
assert.match(focusedQualification, /cancellation-mutations\).*test-worker-cancellation-subject-mutation\.sh --mutation-only/u);
assert.match(focusedQualification, /cancellation\).*smoke-worker-roundtrip\.mjs/u);
assert.match(cancellationMutationQualification, /dotnet restore tests\/SIR\.Domain\.Tests\/SIR\.Domain\.Tests\.fsproj --locked-mode[\s\S]*--artifacts-path "\$native_artifacts"[\s\S]*SIR_BUILD_EXCEPTION=cancellation-fixture[\s\S]*dotnet build tests\/SIR\.Domain\.Tests\/SIR\.Domain\.Tests\.fsproj -c Release --no-restore[\s\S]*native_pid=\$![\s\S]*build-client\.sh[\s\S]*wait "\$native_pid"/u);
assert.match(cancellationMutationQualification, /SIR_DOMAIN_TESTS_DLL="\$native_artifacts\/bin\/SIR\.Domain\.Tests\/release\/SIR\.Domain\.Tests\.dll"/u);
assert.match(readFileSync(new URL("./smoke-worker-roundtrip.mjs", import.meta.url), "utf8"), /SIR_DOMAIN_TESTS_DLL[\s\S]*domainTestsCommand/u);
assert.match(focusedQualification, /cross-runtime\)[\s\S]*--domain-only[\s\S]*--ordinary-pr-functional/u);
assert.doesNotMatch(fullQualification, /--ordinary-pr-functional/u);
assert.match(conformanceQualification, /--ordinary-pr-functional requires --domain-only/u);
assert.match(conformanceQualification, /match_arguments=\(-- --functional-cross-runtime\)/u);
assert.match(conformanceQualification, /dotnet build SIR\.slnx -c Release --no-restore/u);
assert.doesNotMatch(conformanceQualification, /bin\/ScenarioCatalogRuntime\/Debug/u);
assert.match(simulationProject, /Compile Include="SpatialQuery\.fs" Condition="'\$\(SpatialQueryImplementation\)' == ''"/u);
assert.match(simulationProject, /ChangeExtension\('\$\(SpatialQueryImplementation\)', '\.fsi'\)/u);
assert.match(simulationProject, /Compile Include="\$\(SpatialQueryImplementation\)" Link="SpatialQuery\.fs" Condition="'\$\(SpatialQueryImplementation\)' != ''"/u);
assert.match(spatialMutationQualification, /SIR_SPATIAL_MUTATION_CONCURRENCY:-3/u);
assert.match(spatialMutationQualification, /run_prepared_mutation "\$name" &/u);
assert.match(spatialMutationQualification, /--artifacts-path "\$artifacts" -p:SpatialQueryImplementation="\$mutant"/u);
assert.match(spatialQualification, /external_mutation_proof/u);
assert.match(spatialQualification, /test-spatial-subject-mutations\.sh" --prepared-pr &/u);
assert.match(spatialQualification, /wait "\$mutation_pid"/u);
assert.match(spatialQualification, /local fixture="\$task_tmp\/unreadable-client-scan"/u);
assert.match(spatialQualification, /client_has_authority_calls "\$fixture"/u);
assert.doesNotMatch(spatialQualification, /chmod 000 "src\/SIR\.Client\.Web/u);
assert.match(focusedQualification, /spatial\) \.\/scripts\/verify-spatial-query\.sh[^\n]*--prepared-parts-verified/u);
assert.match(spatialQualification, /--prepared-parts-verified[\s\S]*prepared_parts_verified=true/u);
assert.match(spatialQualification, /if \[\[ "\$prepared_parts_verified" == false \]\]; then[\s\S]*production-build-receipt\.mjs verify/u);
assert.match(browserConfiguration, /workers: 1/u);
assert.match(browserConfiguration, /fullyParallel: true/u);
assert.match(browserConfiguration, /process\.env\.SIR_BROWSER_PORT/u);
assert.equal(browserShardCapacityFor(1), 1);
assert.equal(browserShardCapacityFor(2), 1);
assert.equal(browserShardCapacityFor(4), 2);
assert.equal(browserShardCapacityFor(8), 4);
assert.throws(() => browserShardCapacityFor(0), /positive safe integer/u);
assert.match(browserShards, /browserShardCapacityFor\(availableParallelism\(\)\)/u);
assert.match(browserShards, /process\.env\.CI \? Math\.min\(browserShardCapacity, browserPortCapacity\) : 1/u);
assert.match(browserShards, /--shard=\$\{index\}\/\$\{browserShards\}/u);
assert.match(browserShards, /SIR_BROWSER_SHARD_INDEX/u);
assert.match(browserShards, /SIR_BROWSER_COHORT/u);
assert.match(browserShards, /--grep-invert/u);
assert.match(browserShards, /--grep/u);
assert.match(browserGlobalMerge, /parseBrowserShardJUnit/u);
assert.match(browserGlobalMerge, /mergeBrowserShardCases/u);
assert.doesNotMatch(reconnectLifecycle, /active\.stop\(\)/u);
assert.match(reconnectLifecycle, /active\.start\(\)/u);
assert.match(productionDelivery, /@production-delivery/u);
assert.match(browserShards, /SIR_BROWSER_PORT: String\(browserPortBase \+ index - 1\)/u);
assert.doesNotMatch(browserShards, /browserShards > 2/u);
assert.match(browserShards, /mergeShardReports/u);
assert.match(browserJunit, /left\.localeCompare\(right\)/u);
const browserCase = '  <testcase classname="browser" name="one"></testcase>';
const browserFragment = `<?xml version="1.0" encoding="UTF-8"?>\n<testsuites tests="1" failures="0" skipped="0">\n <testsuite name="sir-browser" tests="1" failures="0" skipped="0">\n${browserCase}\n </testsuite>\n</testsuites>\n`;
assert.deepEqual(parseBrowserShardJUnit(browserFragment), [browserCase]);
assert.throws(() => parseBrowserShardJUnit("{"), /malformed deterministic JUnit/u);
assert.throws(() => parseBrowserShardJUnit(browserFragment.replaceAll('tests="1"', 'tests="2"')), /count-drifted deterministic JUnit/u);
assert.throws(() => mergeBrowserShardCases([[browserCase], [browserCase]]), /duplicate deterministic JUnit cases/u);
assert.equal(mergeBrowserShardCases([['  <testcase classname="z" name="z"></testcase>'], [browserCase]]), mergeBrowserShardCases([[browserCase], ['  <testcase classname="z" name="z"></testcase>']]));
assert.equal(packageManifest.scripts["test:browser"], "node scripts/test-browser-shards.mjs");
assert.match(matchQualification, /not enforceProductPerformanceBudgets \|\| interactionBest < 50\.0/u);
assert.doesNotMatch(focusedQualification, /verify --work 138-sir-fable-game-scaffold/u);
assert.match(focusedQualification, /route\.paths\.map/u);
assert.match(fullQualification, /--paired-optimization/u);
assert.match(fullQualification, /if \[\[ "\$paired_mode" == true \]\]; then\n  reduction_basis_points=/u);
assert.match(fullQualification, /protected_main_fable_builds=3/u);
assert.match(fullQualification, /if \[\[ -n \$\{SIR_PROTECTED_PREFLIGHT_RECEIPT:-\} \]\]; then[\s\S]*protected_main_fable_builds=1/u);
assert.match(fullQualification, /src\/SIR\.Replay\.Web\/SIR\.Replay\.Web\.fsproj="\$protected_main_fable_builds"/u);
assert.match(fullQualification, /src\/SIR\.Client\.Web\/SIR\.RulesExplorer\.Web\.fsproj="\$protected_main_fable_builds"/u);
for (const subject of ["rules", "spatial", "cancellation", "cross-runtime", "historical-compatibility", "governance", "production-browser", "documentation", "performance", "sdd-verify"]) assert.match(fullQualification, new RegExp(`"${subject}"`, "u"));
// S.I.R.#263 added `collection-strategies`, so 7 became 8. Absolute by design: this is the line
// that goes red when a gate is added or removed without a deliberate look at the pins below it.
assert.equal(gateOrder.length, 8);


// ---------------------------------------------------------------------------------------
// S.I.R.#280: the feedback-timing ceiling, against the run the defect was measured on.
// ---------------------------------------------------------------------------------------

// The wave model must be a fact about ci.yml, not an assertion about it. A subject is
// second-wave exactly when the job that emits its receipt consumes a prepared producer.
const emittingJob = {
  integrity: "integrity", evidence: "integrity",
  "spatial-mutations": "spatial-mutations", "cancellation-mutations": "cancellation-mutations",
  "prepare-native": "prepare-native", "prepare-fable": "prepare-fable",
  "prepare-web": "prepare-web", "prepare-docs": "prepare-docs",
  rules: "domain-conformance", spatial: "domain-conformance", cancellation: "domain-conformance",
  "browser-delivery": "browser-delivery", "cross-runtime": "cross-runtime",
  browser: "browser", "browser-general-helper": "browser-general-helper",
  documentation: "documentation",
  // S.I.R.#263. Its own job, which is the whole placement decision: as an `integrity` subject it
  // would have emitted from the job that also emits `evidence`, and that job's receipt `total` is
  // anchored at the JOB's first step, so its cost would have been inside the wave-1 maximum.
  "collection-strategies": "collection-strategies",
};
for (const subject of subjectOrder) {
  const needsLine = /^\s*needs: (.+)$/mu.exec(jobBody(emittingJob[subject]))?.[1] ?? "";
  assert.ok(needsLine.length > 0, `${subject}: no needs: line for job ${emittingJob[subject]}`);
  const consumesProducer = needsLine.includes("prepare-");
  assert.equal(subjectWave(subject) === 2, consumesProducer,
    `${subject}: wave model disagrees with ci.yml needs: ${needsLine}`);
}
// pr-verdict closes the graph, so the pipeline really is overhead + two waves.
assert.equal(feedbackWaveCount(subjectOrder), 2);
assert.match(jobBody("pr-verdict"), /needs: \[route,/u);
assert.match(jobBody("route"), /Capture runner start/u);

// The inversion itself: the widest classification must never carry the shortest deadline.
const ceilingFor = (paths) => {
  const candidate = route(paths);
  const subjects = ["integrity", ...candidate.selectedGates];
  const budget = feedbackBudgetFor(subjects);
  const reserve = Math.round((budget * feedbackHeadroomBasisPoints) / 10_000);
  return { classification: candidate.classification, gates: candidate.selectedGates.length, target: budget - reserve };
};
const crossCeiling = ceilingFor(["scripts/ci-route.mjs"]);
assert.equal(crossCeiling.classification, "cross-cutting");
for (const paths of [["docs/index.md"], ["src/SIR.Domain/Rules.fs"], ["tests/SIR.Browser.Tests/journey.js"], ["work/x/spec.md"]]) {
  const other = ceilingFor(paths);
  assert.ok(other.gates <= crossCeiling.gates, `${other.classification} selects more gates than cross-cutting`);
  assert.equal(crossCeiling.target >= other.target, true,
    `cross-cutting (${crossCeiling.gates} gates, ${crossCeiling.target}ms) is held tighter than ${other.classification} (${other.gates} gates, ${other.target}ms)`);
}

// Measured per-job durations of run 32607930272 attempt 2 -- the run the issue cites, whose
// sixteen gate receipts all passed and which pr-verdict refused anyway. Queue is excluded
// because run-ci-gate.sh anchors total_ms at the job's own first step.
const measuredTotals = {
  integrity: 108_000, evidence: 118_000, "spatial-mutations": 129_000, "cancellation-mutations": 111_000,
  "prepare-native": 104_000, "prepare-fable": 98_000, "prepare-web": 120_000, "prepare-docs": 102_000,
  rules: 120_000, spatial: 120_000, cancellation: 120_000, "browser-delivery": 120_000,
  "cross-runtime": 70_000, browser: 89_000, "browser-general-helper": 78_000, documentation: 49_000,
  // S.I.R.#263. NOT from run 32607930272, which predates this gate -- stated rather than smuggled
  // in among numbers that do come from it. 23_010ms is this gate's own measured marginal cost on
  // run 32654620076 (restore+build 15_530 + measurement 7_480), where it still ran as an
  // `integrity` subject; the wave-2 job wrapper differs, so treat it as the right MAGNITUDE rather
  // than a reading of the wave-2 job. That is all this fixture needs it to be: the two wave maxima
  // asserted below are unchanged by its presence, which is the placement claim itself.
  "collection-strategies": 23_010,
};
const measuredReceipt = (gate, override) => {
  const total = override ?? measuredTotals[gate];
  const setup = Math.round(total * 0.25);
  const build = Math.round(total * 0.35);
  const parts = gateParts[gate] ?? [];
  return gateResult(gate, "pass", { setup, restore: 0, build, transport: 0, test: total - setup - build, total }, {
    source: crossCutting.source,
    routeDigest: crossCutting.digest,
    artifactBindings: Object.fromEntries(parts.map((part) => [part, producerDigests[part] ?? "e".repeat(64)])),
    artifactDigest: gate.startsWith("prepare-") ? (producerDigests[gate.slice("prepare-".length)] ?? "e".repeat(64))
      : parts.length > 0 ? bindingDigest(canonicalArtifactBindings(Object.fromEntries(parts.map((part) => [part, producerDigests[part] ?? "e".repeat(64)])))) : null,
    receiptReused: parts.length > 0,
    buildInvocations: expectedBuildInvocations[gate],
  });
};
const measuredRun = (overrides = {}) => crossSubjects.map((gate) => measuredReceipt(gate, overrides[gate]));
// 00:33:06 -> 00:41:00. ~191s of that was one job (domain-conformance) in the runner queue.
const measuredWallClock = { startedAtMilliseconds: 0, completedAtMilliseconds: 469_739 };

const realistic = joinRoute(crossCutting, measuredRun(), measuredWallClock);
// The wave maxima are 129s (spatial-mutations) and 120s (domain-conformance), NOT the 129s
// Math.max over every gate, and NOT the 469.7s of wall clock.
assert.deepEqual(realistic.timing.waveCriticalPathMilliseconds, [129_000, 120_000]);
assert.equal(realistic.timing.criticalPathMilliseconds, feedbackPipelineOverheadMilliseconds + 249_000);
assert.equal(realistic.timing.totalMilliseconds, 469_739);
assert.equal(realistic.timing.queuedMilliseconds, 469_739 - (feedbackPipelineOverheadMilliseconds + 249_000));
assert.deepEqual(realistic.failures, [], "a fully green cross-cutting run within a realistic critical path must be admitted");
assert.equal(realistic.result, "pass");

// ...and the gate is still live. A run that GENUINELY overruns is still refused, on its own
// work, at the same wall clock. This is the assertion that separates a repair from a
// disablement: widening or deleting the ceiling turns these green.
const genuineErosion = joinRoute(crossCutting, measuredRun({ browser: 250_000 }), measuredWallClock);
assert.equal(genuineErosion.result, "fail");
assert.ok(genuineErosion.failures.some(({ code, actual }) => code === "feedback-headroom-eroded" && actual === feedbackPipelineOverheadMilliseconds + 129_000 + 250_000));
const genuineOverrun = joinRoute(crossCutting, measuredRun({ browser: 330_000 }), measuredWallClock);
assert.equal(genuineOverrun.result, "fail");
assert.ok(genuineOverrun.failures.some(({ code }) => code === "feedback-budget-exceeded"));
// A wave-1 regression is caught the same way, so the ceiling is not blind to either wave.
assert.ok(joinRoute(crossCutting, measuredRun({ "prepare-native": 400_000 }), measuredWallClock)
  .failures.some(({ code }) => code === "feedback-budget-exceeded"));


// --- S.I.R.#304: the run set and the expected set are ONE declaration, for ALL SIX routes ----
// The defect this closes was not a wrong condition, it was TWO sources of truth. `ci.yml` decided
// which producers to run from a hand-written classification test; `joinRoute` decided which
// producers to expect from the selected gates' `gateParts`. Nothing compared them, so they drifted
// and `pr-verdict` became unsatisfiable for `documentation` and `performance`. Both sides now read
// `expectedProducersFor`, and this block is what holds them there.
//
// SELF-TEST FIRST. A comparison that has never been red is equally consistent with "the two agree"
// and "this cannot detect disagreement". Prove both directions fire before trusting the verdict --
// the same discipline pr-verdict's own collection-coverage check applies to itself.
{
  const agree = (ran, expected) => ({
    ranNotExpected: ran.filter((s) => !expected.includes(s)),
    expectedNotRun: expected.filter((s) => !ran.includes(s)),
  });
  const clean = agree(["prepare-web"], ["prepare-web"]);
  assert.deepEqual([clean.ranNotExpected, clean.expectedNotRun], [[], []], "agreement self-test: identical sets must compare equal");
  // Direction 1 -- a producer the workflow runs that no classification expects. This is the exact
  // shape of the defect: `unexpected-gate-result: prepare-native`.
  assert.deepEqual(agree(["prepare-web", "prepare-native"], ["prepare-web"]).ranNotExpected, ["prepare-native"],
    "agreement self-test: a producer that ran but was not expected must be detected");
  // Direction 2 -- a producer expected that the workflow never runs: `missing-gate-result`.
  assert.deepEqual(agree(["prepare-web"], ["prepare-web", "prepare-docs"]).expectedNotRun, ["prepare-docs"],
    "agreement self-test: a producer that was expected but never ran must be detected");
}

// The route job must PUBLISH every producer flag. An unpublished output reads as the empty string
// in `needs.route.outputs.*`, so the job would silently never run -- a missing-gate-result dressed
// as a skip.
for (const producer of producerOrder) {
  const key = producer.replace("-", "_");
  assert.match(jobBody("route"), new RegExp(`^      ${key}: \\$\\{\\{ steps\\.route\\.outputs\\.${key} \\}\\}$`, "mu"),
    `the route job must publish the ${key} output`);
}
// The emitted VALUE must be the derived membership, not a constant. A flag hard-wired `true`
// reintroduces exactly the defect -- every route running every producer -- while still looking
// like a per-producer output.
assert.match(routeSource, /\.\.\.producerOrder\.map\(\(producer\) => `\$\{producer\.replace\("-", "_"\)\}=\$\{routeProducers\.includes\(producer\)\}`\)/u,
  "scripts/ci-route.mjs must emit each producer flag as its derived membership in expectedProducersFor");
assert.match(routeSource, /const routeProducers = expectedProducersFor\(route\.selectedGates\);/u,
  "the route CLI must derive its producer flags from expectedProducersFor");

// Every producer job keys on its own derived flag: enumerated, never negated, and never
// re-deriving the classification by hand. `classification != 'evidence-only'` is the shape that
// broke two routes; `classification == 'domain' || ...` is the shape that silently goes stale when
// a classification is added.
for (const producer of producerOrder) {
  const condition = /^    if: (.+)$/mu.exec(jobBody(producer))?.[1];
  assert.equal(condition, `needs.route.outputs.${producer.replace("-", "_")} == 'true'`,
    `${producer} must run exactly when the route says its part is needed`);
  assert.doesNotMatch(condition, /classification/u,
    `${producer} must not re-derive the classification by hand -- that is the second source of truth this closed`);
}

// And the agreement itself, over all six classifications -- not just the two that were broken.
// `ran` is evaluated from the workflow's OWN condition text against the route's OWN emitted
// outputs; `expected` is what pr-verdict's join will require. They must be equal for every route.
// Evaluate ci.yml's own condition TEXT, so this models the workflow whatever shape the condition
// takes -- a derived flag, a classification test, or a negation. Modelling only the shape we just
// wrote would make the check agree with itself; this one disagrees when the workflow is wrong.
// Split on a top-level operator only. S.I.R.#309 put parenthesised clauses into a job `if:`
// (a producer-readiness guard is `(A || B)` per producer), and a naive `.split("||")` cuts
// straight through them -- it would read the guard as a disjunction of half-terms and quietly
// answer about an expression the workflow does not contain.
const splitTopLevel = (expression, operator) => {
  const parts = [];
  let depth = 0;
  let current = "";
  for (let index = 0; index < expression.length; index += 1) {
    const character = expression[index];
    if (character === "(") depth += 1;
    if (character === ")") depth -= 1;
    if (depth === 0 && expression.startsWith(operator, index)) {
      parts.push(current);
      current = "";
      index += operator.length - 1;
      continue;
    }
    current += character;
  }
  parts.push(current);
  return parts;
};
const evalWorkflowTerm = (term, context) => {
  let text = term.trim();
  let negate = false;
  while (text.startsWith("!") && !text.startsWith("!=")) { negate = !negate; text = text.slice(1).trim(); }
  // A wholly parenthesised term is a sub-expression, not an operand.
  if (text.startsWith("(") && text.endsWith(")")) {
    let depth = 0;
    let whole = true;
    for (let index = 0; index < text.length; index += 1) {
      if (text[index] === "(") depth += 1;
      else if (text[index] === ")") { depth -= 1; if (depth === 0 && index !== text.length - 1) { whole = false; break; } }
    }
    if (whole) {
      const inner = evalWorkflowCondition(text.slice(1, -1).trim(), context);
      return negate ? !inner : inner;
    }
  }
  let value;
  // Status-check functions. `always()` is true and `cancelled()` is false for the runs modelled
  // here (a PR run nobody cancelled); their PRESENCE is separately load-bearing, because it is
  // what lifts GitHub's implicit "every need succeeded" requirement -- see runsUnder below.
  if (text === "always()") value = true;
  else if (text === "cancelled()") value = false;
  else {
    const parsed = /^(.+?)\s*(==|!=)\s*'([^']*)'$/u.exec(text);
    assert.ok(parsed, `unparsed ci.yml condition term: ${term}`);
    const [, lhsRaw, operator, literal] = parsed;
    const lhs = lhsRaw.trim();
    let actual;
    if (lhs === "github.event_name") actual = "pull_request";
    else {
      const output = /^needs\.route\.outputs\.([a-z_]+)$/u.exec(lhs);
      // `needs.<job>.result`, written either way round. Index syntax is what ci.yml uses for a
      // hyphenated job id; both spellings are read here so the check does not depend on which.
      const jobResult = /^needs(?:\.([a-z][a-z0-9-]*)|\['([a-z][a-z0-9-]*)'\])\.result$/u.exec(lhs);
      if (output) {
        assert.ok(output[1] in context.outputs, `ci.yml reads needs.route.outputs.${output[1]}, which the route job never emits`);
        actual = context.outputs[output[1]];
      } else {
        assert.ok(jobResult, `unparsed ci.yml condition operand: ${lhs}`);
        const jobName = jobResult[1] ?? jobResult[2];
        assert.ok(jobName in context.results, `ci.yml reads needs.${jobName}.result, which that job does not declare in its own needs:`);
        actual = context.results[jobName];
      }
    }
    value = operator === "==" ? actual === literal : actual !== literal;
  }
  return negate ? !value : value;
};
function evalWorkflowCondition(expression, context) {
  return splitTopLevel(expression, "||")
    .some((clause) => splitTopLevel(clause, "&&").every((term) => evalWorkflowTerm(term, context)));
}
// Exactly what `route --github-output` writes, derived the same way the CLI derives it.
const emittedRouteOutputs = (candidate) => {
  const outputs = { classification: candidate.classification, prepare: String(candidate.selectedGates.some((gate) => gate !== "evidence")) };
  for (const gate of gateOrder) outputs[gate.replace("-", "_")] = String(candidate.selectedGates.includes(gate));
  const producers = expectedProducersFor(candidate.selectedGates);
  for (const producer of producerOrder) outputs[producer.replace("-", "_")] = String(producers.includes(producer));
  return outputs;
};
const producersWorkflowRuns = (candidate) => producerOrder.filter((producer) =>
  evalWorkflowCondition(workflowJobs[producer].if ?? "always()",
    { outputs: emittedRouteOutputs(candidate), results: { route: "success" } }));
const sixRoutes = {
  documentation: ["docs/index.md"],
  domain: ["src/SIR.Domain/Rules.fs"],
  browser: ["tests/SIR.Browser.Tests/journey.js"],
  performance: ["scripts/measure-svg-pipeline.mjs"],
  "evidence-only": ["work/220-bounded-pr-ci/spec.md"],
  "cross-cutting": [".github/workflows/ci.yml"],
};
assert.deepEqual(Object.keys(sixRoutes).sort(), ["browser", "cross-cutting", "documentation", "domain", "evidence-only", "performance"],
  "all six classifications must be covered; a repair proven on two leaves four unproven");
for (const [classification, paths] of Object.entries(sixRoutes)) {
  const candidate = route(paths);
  assert.equal(candidate.classification, classification, `${classification} sample must classify as ${classification}`);
  const expected = expectedProducersFor(candidate.selectedGates);
  const ran = producersWorkflowRuns(candidate);
  // Set equality, not order: `expectedProducersFor` preserves the selected gates' part order (which
  // is what joinRoute's expectedSubjects has always used), while `ran` reads `producerOrder`. The
  // contract is which producers run, not the sequence the two lists happen to name them in.
  assert.deepEqual([...ran].sort(), [...expected].sort(),
    `${classification}: ci.yml runs [${ran}] but pr-verdict expects [${expected}] -- these must be one declaration`);
}
// --- S.I.R.#263: the JOIN's subject list and `subjectOrder` are the same set -----------------
//
// `pr-verdict` builds its `--result` arguments from a hand-written `for gate in ...` list in
// ci.yml. Nothing joined that list to `subjectOrder`, and a subject missing from it is never
// passed to the join at all -- so the join reports `missing-gate-result` for a gate whose job ran
// and PASSED. Measured, not hypothesised: on run 32656572583 the `collection-strategies` job
// passed in 51s and `pr-verdict` reported its receipt missing, because adding a gate to the router
// and to ci.yml's job graph left this third list untouched.
//
// This is #304's defect one layer out. #304 made producer SELECTION one declaration; the join's
// own INPUT list stayed hand-written. Both directions are asserted, because they fail differently:
// a subject missing from the loop is a gate whose verdict is silently discarded, and a name in the
// loop that is not a subject is a `--result` for a file that can never exist.
const joinSubjectLoop = (() => {
  const body = jobBody("pr-verdict");
  // EXACTLY ONE, and it must be THE loop that feeds the join. The first version of this used `exec`,
  // which takes the FIRST match and asserts nothing about how many exist -- so a second
  // `for gate in <correct list>; do` anywhere earlier in the job satisfied it while the real loop went
  // unchecked, and the suite stayed green with `collection-strategies` dropped from the join. A check
  // that reads the first thing shaped like its subject is not reading its subject.
  const loops = [...body.matchAll(/^\s*for gate in ([a-z][a-z0-9 -]*); do$([\s\S]*?)^\s*done$/gmu)];
  assert.equal(
    loops.length, 1,
    `pr-verdict contains ${loops.length} \`for gate in ...; do\` loops; exactly one must exist so that "the loop" is unambiguous`,
  );
  const [, names, loopBody] = loops[0];
  // PROVENANCE: the loop must be the one that actually builds the join's arguments. Shape alone is not
  // identity -- a decorative loop with the right words in it would otherwise pass for the real one.
  assert.match(
    loopBody, /args\+=\(--result/u,
    "the matched `for gate in ...` loop does not build `args+=(--result ...)`, so it is not the loop that feeds the join",
  );
  return names.trim().split(/\s+/u);
})();
assert.equal(new Set(joinSubjectLoop).size, joinSubjectLoop.length,
  `pr-verdict's join loop names a subject twice: ${joinSubjectLoop.join(" ")}`);
// ONE comparison, used by the assertion AND by its self-test. The first version self-tested a
// REPLICA (`disagree`) while the assertion was a separate `deepEqual`, so making the real predicate
// tautological left the self-test green and the suite passing -- a vacuity guard that guarded nothing.
const joinLoopDisagreement = (loop, subjects) =>
  subjects.filter((id) => !loop.includes(id)).concat(loop.filter((id) => !subjects.includes(id)));
assert.equal(joinLoopDisagreement(["a", "b"], ["a", "b"]).length, 0, "join-loop self-test: agreement must read as agreement");
assert.deepEqual(joinLoopDisagreement(["a"], ["a", "b"]), ["b"], "join-loop self-test: a subject missing from the loop must be detectable");
assert.deepEqual(joinLoopDisagreement(["a", "b"], ["a"]), ["b"], "join-loop self-test: a loop name that is not a subject must be detectable");
assert.deepEqual(
  joinLoopDisagreement(joinSubjectLoop, subjectOrder),
  [],
  "pr-verdict's join loop and subjectOrder disagree.\n"
    + `  a subject the join never receives (its gate's verdict is DISCARDED): ${subjectOrder.filter((id) => !joinSubjectLoop.includes(id)).join(", ") || "(none)"}\n`
    + `  a name in the loop that is not a subject (a --result that can never exist): ${joinSubjectLoop.filter((id) => !subjectOrder.includes(id)).join(", ") || "(none)"}`,
);

// --- S.I.R.#309: every classification's job graph must be SATISFIABLE -----------------------
// S.I.R.#304 made producer SELECTION one declaration. It did not, and could not, decide whether
// the workflow's `needs:` GRAPH can be satisfied once that selection is correct -- and it is a
// separate question, because `needs:` is static YAML that no run-time declaration can rewrite.
//
// One correction to how #308's PR body put it, established by its own critic and accepted by its
// author: correct producer derivation does NOT cause `prepare-web` to be skipped for `domain`.
// The pre-#304 hand-written condition (`classification == 'documentation' || 'browser' ||
// 'performance' || 'cross-cutting'`) already excluded `domain`, so the skip -- and this single
// invariant violation -- is present identically at `630d656` and at `d500145`. #304 leaves
// `domain`'s skip unchanged in BOTH directions; it neither causes nor repairs it. The conclusion
// that the repair belongs here rather than in producer selection is unaffected.
//
// `domain-conformance` hosts three gates whose parts differ, so its `needs:` must name the UNION
// while any one route selects a subset. GitHub skips a job whose `needs:` were skipped, so the
// job was skipped with its own `if:` true and `rules`/`spatial` were never written.
//
// The generalisation, which is what this block enforces: A JOB WHOSE STATIC `needs:` IS A STRICT
// SUPERSET OF WHAT SOME ROUTE SELECTS IS UNSATISFIABLE ON THAT ROUTE. That is a property of the
// graph, not of `domain-conformance`, so it is checked for every job on every classification.

// Which jobs actually run, applying GitHub's own rule: a job runs when its own `if:` is true AND
// -- unless that `if:` contains a status-check function -- every job it needs SUCCEEDED.
const runsUnder = (outputs, { failed = [] } = {}) => {
  const results = {};
  const names = Object.keys(workflowJobs);
  let progressed = true;
  while (progressed) {
    progressed = false;
    for (const name of names) {
      if (name in results) continue;
      const job = workflowJobs[name];
      if (job.needs.some((need) => !(need in results))) continue;
      // Only this job's OWN needs are readable from its `if:`; anything else is a workflow error
      // the evaluator must report rather than quietly answer.
      const visible = Object.fromEntries(job.needs.map((need) => [need, results[need]]));
      const condition = job.if ?? "always()";
      const own = evalWorkflowCondition(condition, { outputs, results: visible });
      const statusFunction = /\b(?:always|success|failure|cancelled)\s*\(\s*\)/u.test(condition);
      const needsMet = statusFunction || job.needs.every((need) => results[need] === "success");
      results[name] = !own || !needsMet ? "skipped" : failed.includes(name) ? "failure" : "success";
      progressed = true;
    }
  }
  assert.equal(Object.keys(results).length, names.length, "every ci.yml job must be schedulable -- an unresolved job means a needs: cycle");
  return results;
};

// A job's own `if:` read as though every job it needs had SUCCEEDED. This is what separates "this
// route does not want this job" from "this route wants it and cannot have it": once the readiness
// guard is inside the `if:`, a plain reading can no longer tell those two apart.
const ownConditionHolds = (job, outputs) => evalWorkflowCondition(job.if ?? "always()",
  { outputs, results: Object.fromEntries(job.needs.map((need) => [need, "success"])) });

// Which gate receipts a job WRITES, read off ci.yml's own steps -- the YAML step `if:` and the
// bash guard its `run:` block puts around each invocation. Nothing here is a hand-kept list, so a
// gate that moves between jobs moves here too.
const gatesWrittenBy = (body, outputs) => {
  const context = { outputs, results: {} };
  const written = [];
  let stepCondition = null;
  let bashGuards = [];
  let inStep = false;
  for (const line of body.split("\n")) {
    if (/^      - /u.test(line)) { inStep = true; stepCondition = null; bashGuards = []; }
    if (!inStep) continue;
    const stepIf = /^      (?:- )?if: (.+)$/u.exec(line);
    if (stepIf) { stepCondition = stepIf[1].trim(); continue; }
    if (/^\s*if \[\[ /u.test(line)) {
      const guard = /\$\{\{\s*(.+?)\s*\}\}'?\s*(==|!=)\s*'([^']*)'/u.exec(line);
      bashGuards.push(guard ? `${guard[1]} ${guard[2]} '${guard[3]}'` : "always()");
      continue;
    }
    if (/^\s*fi\s*$/u.test(line)) { bashGuards.pop(); continue; }
    const inlineGuarded = /\[\[\s*'\$\{\{\s*(.+?)\s*\}\}'\s*(==|!=)\s*'([^']*)'\s*\]\]\s*&&\s*gates\+=\(([a-z-]+)\)/u.exec(line);
    const literal = /run-ci-gate\.sh ([a-z][a-z-]*) /u.exec(line);
    const preparePart = /qualify-pr\.sh prepare-part ([a-z]+)\s*$/u.exec(line);
    const found = inlineGuarded ? { gate: inlineGuarded[4], extra: `${inlineGuarded[1]} ${inlineGuarded[2]} '${inlineGuarded[3]}'` }
      : literal ? { gate: literal[1], extra: null }
        : preparePart ? { gate: `prepare-${preparePart[1]}`, extra: null } : null;
    if (!found) continue;
    const guards = [...bashGuards, ...(found.extra ? [found.extra] : []), ...(stepCondition ? [stepCondition] : [])];
    if (!guards.every((guard) => evalWorkflowCondition(guard, context))) continue;
    if (!written.includes(found.gate)) written.push(found.gate);
  }
  return written;
};

// SELF-TEST FIRST, on the same principle as #304's block above: a check that has never been red
// is equally consistent with "the graph is satisfiable" and "this cannot detect anything". Each
// of the three moving parts is shown to fire before any verdict below is trusted.
{
  const outputs = emittedRouteOutputs(route(sixRoutes.domain));
  // (1) skip propagation is modelled at all. A probe job whose own `if:` this route satisfies,
  // but which needs a producer this route does not select, must come out SKIPPED -- and its twin
  // with a status-check function must come out run. Same route, same producer, one difference.
  const probe = { name: "probe", body: "", if: "needs.route.outputs.rules == 'true'", needs: ["route", "prepare-web"] };
  const scheduleWith = (extra) => {
    const real = runsUnder(outputs);
    const visible = Object.fromEntries(extra.needs.map((need) => [need, real[need]]));
    const own = evalWorkflowCondition(extra.if, { outputs, results: visible });
    const statusFunction = /\b(?:always|success|failure|cancelled)\s*\(\s*\)/u.test(extra.if);
    return !own || !(statusFunction || extra.needs.every((need) => real[need] === "success")) ? "skipped" : "success";
  };
  assert.equal(runsUnder(outputs)["prepare-web"], "skipped", "self-test premise: a domain route does not select prepare-web");
  assert.equal(scheduleWith(probe), "skipped",
    "self-test: a job needing an unselected producer must be modelled as skipped -- this is the defect");
  assert.equal(scheduleWith({ ...probe, if: `!cancelled() && ${probe.if}` }), "success",
    "self-test: and a status-check function must be modelled as lifting that implicit requirement");
  // (2) the optimistic reading really is optimistic -- it ignores a skipped need.
  assert.equal(ownConditionHolds(probe, outputs), true,
    "self-test: the optimistic reading must hold even when a need was skipped");
  assert.equal(ownConditionHolds({ ...probe, if: "needs.route.outputs.browser == 'true'" }, outputs), false,
    "self-test: the optimistic reading must still be false when the route does not want the job");
  // (3) parenthesised guards are evaluated as guards, not cut through by the || split.
  assert.equal(evalWorkflowCondition("(needs.route.outputs.rules == 'true' || needs.route.outputs.browser == 'true') && needs.route.outputs.browser == 'true'",
    { outputs, results: {} }), false, "self-test: a top-level && after a parenthesised group must still bind");
  assert.equal(evalWorkflowCondition("!cancelled() && (needs.route.outputs.prepare_web != 'true' || needs['prepare-web'].result == 'success')",
    { outputs, results: { "prepare-web": "skipped" } }), true,
    "self-test: an unselected producer must satisfy its readiness clause");
  assert.equal(evalWorkflowCondition("!cancelled() && (needs.route.outputs.prepare_native != 'true' || needs['prepare-native'].result == 'success')",
    { outputs, results: { "prepare-native": "failure" } }), false,
    "self-test: a producer this route DID select and that failed must NOT satisfy its readiness clause");
  // (4) the receipt reader can tell a written gate from an unwritten one.
  assert.deepEqual(gatesWrittenBy(jobBody("cross-runtime"), outputs), ["cross-runtime"],
    "self-test: the receipt reader must read a job's gate off its own steps");
  assert.deepEqual(gatesWrittenBy(jobBody("domain-conformance"), outputs).sort(), ["rules", "spatial"],
    "self-test: a domain route's domain-conformance writes rules and spatial, and not cancellation");
}

// AC-4. THE INVARIANT, for every job on every classification: a job's own `if:` -- read as though
// every job it needs had succeeded -- must imply that the job actually runs. A job that is true
// under that reading and skipped in the real schedule was skipped by `needs:` propagation alone.
// This is the assertion the reintroduced static edge in AC-5 goes red against.
const unsatisfiableJobs = (candidate) => {
  const outputs = emittedRouteOutputs(candidate);
  const results = runsUnder(outputs);
  return Object.values(workflowJobs)
    .filter((job) => results[job.name] === "skipped" && ownConditionHolds(job, outputs))
    .map((job) => ({ job: job.name, skippedNeeds: job.needs.filter((need) => results[need] !== "success") }));
};
for (const [classification, paths] of Object.entries(sixRoutes)) {
  assert.deepEqual(unsatisfiableJobs(route(paths)), [],
    `${classification}: a job whose own if: is satisfied was skipped because its needs: were -- this route's job graph is unsatisfiable`);
}

// The structural half, and the reason this class does not come back one job at a time. `needs:`
// cannot be GENERATED -- GitHub reads it as static YAML before anything of ours runs -- so it is
// CHECKED against the same `gateParts` declaration #304 made the single source of producer truth.
// Which gates a job hosts is itself read off ci.yml under the widest route, so neither side of
// this equality is hand-kept and there is no second place to update.
const widestOutputs = emittedRouteOutputs(route(sixRoutes["cross-cutting"]));
const derivedProducerNeeds = (job) => [...new Set(gatesWrittenBy(job.body, widestOutputs).flatMap((gate) => gateParts[gate] ?? []))]
  .map((part) => `prepare-${part}`).sort();
for (const job of Object.values(workflowJobs)) {
  // pr-verdict is the JOIN, not a consumer: it needs every producer by construction and carries
  // `always()`, so the equality below does not describe it. It is pinned separately above.
  if (job.name === "pr-verdict") continue;
  const declared = job.needs.filter((need) => producerOrder.includes(need)).sort();
  assert.deepEqual(declared, derivedProducerNeeds(job),
    `${job.name}: needs: names producers [${declared}] but the gates it hosts consume [${derivedProducerNeeds(job)}]`
    + " -- a producer edge no hosted gate consumes makes this job unsatisfiable on any route that omits it");
}

// The CONSUMPTION half of the same invariant, and the half a `needs:`-only reading misses. Fixing
// participation alone would have turned the skip into a RED download step: `domain-conformance`
// asked for `prepared-part-web` unconditionally, and on a `domain` route that artifact does not
// exist because its producer correctly never ran. So: no job may download a part this route did
// not prepare, on any classification it runs on.
const preparedPartDownloads = (body) => {
  const found = [];
  let stepCondition = null;
  for (const line of body.split("\n")) {
    if (/^      - /u.test(line)) stepCondition = null;
    const stepIf = /^      (?:- )?if: (.+)$/u.exec(line);
    if (stepIf) { stepCondition = stepIf[1].trim(); continue; }
    const part = /name: prepared-part-([a-z]+)/u.exec(line);
    if (part) found.push({ part: part[1], condition: stepCondition });
  }
  return found;
};
// Self-test: the reader must find the downloads at all, and must read their guards.
{
  const found = preparedPartDownloads(jobBody("domain-conformance"));
  assert.deepEqual(found.map(({ part }) => part), ["native", "fable", "web"],
    "self-test: the download reader must see every prepared-part download in the job");
  assert.ok(found.every(({ condition }) => condition !== null),
    "self-test: each of those downloads must carry a guard for this check to be about anything");
}
for (const [classification, paths] of Object.entries(sixRoutes)) {
  const outputs = emittedRouteOutputs(route(paths));
  const results = runsUnder(outputs);
  for (const job of Object.values(workflowJobs)) {
    if (results[job.name] !== "success") continue;
    for (const { part, condition } of preparedPartDownloads(job.body)) {
      if (condition !== null && !evalWorkflowCondition(condition, { outputs, results: {} })) continue;
      assert.equal(outputs[`prepare_${part}`], "true",
        `${classification}: ${job.name} downloads prepared-part-${part}, which this route never prepared`
        + " -- that artifact does not exist, so the job fails on the download rather than being repaired");
    }
  }
}

// AC-3. Lifting GitHub's implicit success requirement must not lift the real one. A producer this
// route DID select and that FAILED still blocks every job that consumes it -- otherwise the repair
// would have traded a skipped gate for a gate that runs against inputs that were never built.
for (const producer of ["prepare-native", "prepare-fable", "prepare-web"]) {
  const outputs = emittedRouteOutputs(route(sixRoutes["cross-cutting"]));
  const results = runsUnder(outputs, { failed: [producer] });
  assert.equal(results[producer], "failure", `${producer} must be modelled as failing for this case to mean anything`);
  assert.equal(results["domain-conformance"], "skipped",
    `domain-conformance must not run when ${producer}, which this route selected, failed`);
}
// ...and the control: with nothing failing, it does run. Without this the assertion above is also
// satisfied by a job that never runs at all.
assert.equal(runsUnder(emittedRouteOutputs(route(sixRoutes["cross-cutting"])))["domain-conformance"], "success",
  "control: domain-conformance must run when every producer this route selected succeeded");

// AC-1 and AC-2. END TO END, through the real join, for all six: feed `joinRoute` exactly the
// receipts the surviving jobs write and require `pass`.
//
// The subject set is built from the jobs that SURVIVE skip propagation, never from the selected
// gate list. That distinction is this item: the block #304 shipped here built its subjects from
// `...selected`, so for `domain` it supplied `rules` and `spatial` receipts that production never
// writes, and asserted `pass` for the one classification where production `pr-verdict` failed. It
// was green and production was red, and both were correct about what they measured.
for (const [classification, paths] of Object.entries(sixRoutes)) {
  const candidate = route(paths);
  const outputs = emittedRouteOutputs(candidate);
  const results = runsUnder(outputs);
  const subjects = [];
  for (const name of Object.keys(workflowJobs)) {
    if (results[name] !== "success") continue;
    for (const gate of gatesWrittenBy(workflowJobs[name].body, outputs)) if (!subjects.includes(gate)) subjects.push(gate);
  }
  const digestsFor = (part) => ({ native: "c", fable: "d", web: "e", docs: "f" }[part] ?? "e").repeat(64);
  const receipt = (gate) => gateResult(gate, "pass", { setup: 100, restore: 200, build: 300, test: 400, total: 1_000 }, {
    source: candidate.source,
    routeDigest: candidate.digest,
    artifactBindings: Object.fromEntries((gateParts[gate] ?? []).map((part) => [part, digestsFor(part)])),
    artifactDigest: gate.startsWith("prepare-")
      ? digestsFor(gate.slice("prepare-".length))
      : (gateParts[gate] ?? []).length > 0
        ? bindingDigest(canonicalArtifactBindings(Object.fromEntries(gateParts[gate].map((part) => [part, digestsFor(part)]))))
        : null,
    receiptReused: (gateParts[gate] ?? []).length > 0,
    buildInvocations: expectedBuildInvocations[gate],
  });
  const verdict = joinRoute(candidate, subjects.map(receipt), { startedAtMilliseconds: 0, completedAtMilliseconds: 1 });
  const producerComplaints = verdict.failures.filter(({ code, subject }) =>
    ["unexpected-gate-result", "missing-gate-result", "missing-prepared-artifact-binding"].includes(code)
    && (subject ?? "").startsWith("prepare-"));
  assert.deepEqual(producerComplaints, [],
    `${classification}: pr-verdict must not refuse the producer set ci.yml runs, got ${JSON.stringify(producerComplaints)}`);
  assert.equal(verdict.result, "pass",
    `${classification}: pr-verdict must pass when every gate ci.yml RUNS reports pass, got ${JSON.stringify(verdict.failures)}`);
}

// The two routes the defect made unsatisfiable, stated explicitly so a regression names itself.
for (const unsatisfiable of ["documentation", "performance"]) {
  assert.ok(!expectedProducersFor(route(sixRoutes[unsatisfiable]).selectedGates).includes("prepare-native"),
    `${unsatisfiable} must not expect prepare-native`);
  assert.doesNotMatch(jobBody("prepare-native"), /classification/u,
    `prepare-native must not run on a classification negation -- that is what made ${unsatisfiable} unsatisfiable`);
}

console.log("CI route matrix, conservative fallback, deterministic join mutations, budget boundary, and workflow DAG contract passed.");

const junitOutput = process.env.SIR_JUNIT_OUTPUT;
if (junitOutput) {
  mkdirSync(dirname(junitOutput), { recursive: true });
  writeFileSync(junitOutput, `<?xml version="1.0" encoding="UTF-8"?>
<testsuite name="ci-route" tests="1" failures="0" errors="0" skipped="0" time="0">
  <testcase classname="SIR.CI" name="route-matrix-dag-and-budget-contract" time="0" />
</testsuite>
`, "utf8");
}
