import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname } from "node:path";
import { canonicalArtifactBindings, expectedBuildInvocations, gateOrder, gateParts, producerOrder, subjectOrder, gateResult, joinRoute, routePaths, feedbackBudgetMilliseconds, feedbackAcceptanceTargetMilliseconds, feedbackHeadroomMilliseconds, routeSchema, gateSchema, joinSchema, timingSchema } from "./ci-route.mjs";
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
assert.equal(joined.timing.acceptanceTargetMilliseconds, feedbackBudgetMilliseconds);
assert.equal(joined.timing.requiredHeadroomMilliseconds, 0);
assert.equal(joined.timing.actualHeadroomMilliseconds, 62_000);
const nonCrossCuttingReserve = joinRoute(domain, passing, { startedAtMilliseconds: 0, completedAtMilliseconds: feedbackAcceptanceTargetMilliseconds + 1 });
assert.equal(nonCrossCuttingReserve.result, "pass");
assert.ok(!nonCrossCuttingReserve.failures.some(({ code }) => code === "feedback-headroom-eroded"));
const performanceResultFor = (gate) => gateResult(gate, "pass", { setup: 100, restore: 200, build: 300, test: 400, total: 1_000 }, {
  source: performance.source,
  routeDigest: performance.digest,
  artifactBindings: Object.fromEntries((gateParts[gate] ?? []).map((part) => [part, producerDigests[part]])),
  artifactDigest: gate.startsWith("prepare-") ? producerDigests[gate.slice("prepare-".length)] : (gateParts[gate] ?? []).length > 0 ? bindingDigest(canonicalArtifactBindings(Object.fromEntries(gateParts[gate].map((part) => [part, producerDigests[part]])))) : null,
  receiptReused: (gateParts[gate] ?? []).length > 0,
  buildInvocations: expectedBuildInvocations[gate],
});
const performancePassing = ["integrity", "prepare-web", "prepare-docs", "documentation", "evidence"].map(performanceResultFor);
assert.equal(joinRoute(performance, performancePassing, { startedAtMilliseconds: 0, completedAtMilliseconds: feedbackAcceptanceTargetMilliseconds }).result, "pass");
const performanceHeadroomEroded = joinRoute(performance, performancePassing, { startedAtMilliseconds: 0, completedAtMilliseconds: feedbackAcceptanceTargetMilliseconds + 1 });
assert.equal(performanceHeadroomEroded.result, "fail");
assert.ok(performanceHeadroomEroded.failures.some(({ code, target, requiredHeadroom }) => code === "feedback-headroom-eroded" && target === feedbackAcceptanceTargetMilliseconds && requiredHeadroom === feedbackHeadroomMilliseconds));
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
const erodedHeadroom = joinRoute(crossCutting, crossPassing, { startedAtMilliseconds: 0, completedAtMilliseconds: feedbackAcceptanceTargetMilliseconds + 1 });
assert.equal(erodedHeadroom.result, "fail");
assert.ok(erodedHeadroom.failures.some(({ code, target, requiredHeadroom }) => code === "feedback-headroom-eroded" && target === feedbackAcceptanceTargetMilliseconds && requiredHeadroom === feedbackHeadroomMilliseconds));
assert.ok(!erodedHeadroom.failures.some(({ code }) => code === "feedback-budget-exceeded"));
const overBudget = joinRoute(domain, passing, { startedAtMilliseconds: 0, completedAtMilliseconds: feedbackBudgetMilliseconds + 1 });
assert.equal(overBudget.result, "fail");
assert.ok(overBudget.failures.some(({ code }) => code === "feedback-budget-exceeded"));
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
const jobBody = (name) => new RegExp(`^  ${name}:\\n([\\s\\S]*?)(?=^  [a-z][a-z-]*:\\n)`, "mu").exec(workflow)?.[1] ?? "";
const contracts = JSON.parse(readFileSync(new URL("../tests/fixtures/ci-qualification/v1/contracts.json", import.meta.url), "utf8"));
assert.equal(contracts.feedbackBudgetMilliseconds, feedbackBudgetMilliseconds);
assert.equal(contracts.feedbackAcceptanceTargetMilliseconds, feedbackAcceptanceTargetMilliseconds);
assert.equal(contracts.feedbackHeadroomMilliseconds, feedbackHeadroomMilliseconds);
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
for (const job of ["route:", "integrity:", "spatial-mutations:", "cancellation-mutations:", "prepare-native:", "prepare-fable:", "prepare-web:", "prepare-docs:", "domain-conformance:", "cross-runtime:", "browser:", "browser-general-helper:", "browser-delivery:", "documentation:", "pr-verdict:", "cost-observer:", "protected-preflight:", "full-qualification:", "protected-verdict:"]) assert.match(workflow, new RegExp(`^  ${job}$`, "mu"));
for (const removedJob of ["browser-general-helper-2", "browser-general-helper-3"]) assert.doesNotMatch(workflow, new RegExp(`^  ${removedJob}:$`, "mu"));
assert.match(workflow, /if: always\(\)/u);
assert.match(workflow, /if: always\(\) && github\.event_name == 'pull_request'/u);
assert.match(workflow, /schedule:/u);
assert.match(workflow, /cancel-in-progress: \$\{\{ github\.event_name == 'pull_request' \}\}/u);
assert.match(workflow, /hashFiles\('\*\*\/packages\.lock\.json', 'Directory\.Packages\.props', '\.config\/dotnet-tools\.json'\)/u);
assert.equal(workflow.match(/name: Capture runner start/gu)?.length, 18);
for (const part of ["native", "fable", "web", "docs"]) assert.match(workflow, new RegExp(`qualify-pr\\.sh prepare-part ${part}`, "u"));
assert.doesNotMatch(workflow, /qualify-pr\.sh prepare-part server/u);
assert.match(jobBody("prepare-native"), /classification != 'evidence-only'/u);
assert.doesNotMatch(workflow, /^  prepare:$/mu);
assert.doesNotMatch(workflow, /prepared-candidate/u);
assert.match(workflow, /domain-conformance:\n[\s\S]*?needs: \[route, prepare-native, prepare-fable, prepare-web\]/u);
assert.match(jobBody("domain-conformance"), /extract-parts native fable web[\s\S]*run-ci-gate\.sh rules[\s\S]*run-ci-gate\.sh browser-delivery[\s\S]*gates\+=\(spatial\)[\s\S]*gates\+=\(cancellation\)[\s\S]*for gate in[\s\S]*run-ci-gate\.sh "\$gate"[\s\S]*for pid in[^\n]*wait[^\n]*done[\s\S]*gate-domain-conformance/u);
assert.doesNotMatch(jobBody("domain-conformance"), /gates\+=\(rules\)/u);
assert.doesNotMatch(jobBody("domain-conformance"), /gates\+=\(browser-delivery\)/u);
assert.doesNotMatch(jobBody("domain-conformance"), /actions\/cache@|npm ci|dotnet tool restore|cache: npm/u);
assert.doesNotMatch(jobBody("domain-conformance"), /gates\+=\(cross-runtime\)/u);
assert.doesNotMatch(jobBody("domain-conformance"), /run-ci-gate\.sh cross-runtime/u);
assert.match(jobBody("cross-runtime"), /needs: \[route, prepare-native, prepare-fable\][\s\S]*prepared-part-native[\s\S]*prepared-part-fable[\s\S]*run-ci-gate\.sh cross-runtime/u);
assert.doesNotMatch(workflow, /^  spatial:$/mu);
assert.match(jobBody("prepare-web"), /classification == 'performance'/u);
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
assert.match(gateRunner, /ci-gate-artifact-bindings\.sh" "\$repo_root" "\$\{preflight_parts\[@\]\}"/u);
assert.match(workflow, /pr-verdict:\n[\s\S]*?needs: \[[^\]]*prepare-docs[^\]]*spatial-mutations[^\]]*cancellation-mutations[^\]]*browser-general-helper[^\]]*browser-delivery[^\]]*domain-conformance[^\]]*cross-runtime[^\]]*\]/u);
assert.match(jobBody("browser"), /SIR_JUNIT_OUTPUT: artifacts\/ci\/results\/browser-general-1\.junit\.xml[\s\S]*SIR_JUNIT_OUTPUT_2: artifacts\/ci\/results\/browser-general-2\.junit\.xml/u);
assert.match(jobBody("browser-general-helper"), /SIR_JUNIT_OUTPUT: artifacts\/ci\/results\/browser-general-3\.junit\.xml[\s\S]*SIR_JUNIT_OUTPUT_2: artifacts\/ci\/results\/browser-general-4\.junit\.xml/u);
assert.match(jobBody("pr-verdict"), /test-browser-global-merge\.mjs[\s\S]*browser-general-1\.junit\.xml[\s\S]*browser-general-2\.junit\.xml[\s\S]*browser-general-3\.junit\.xml[\s\S]*browser-general-4\.junit\.xml/u);
assert.match(jobBody("pr-verdict"), /browser_merge_status=\$\?[\s\S]*rm -f artifacts\/ci\/results\/browser-general-helper\.json/u);
assert.match(workflow, /for gate in integrity prepare-native prepare-fable prepare-web prepare-docs spatial-mutations cancellation-mutations browser-general-helper browser-delivery rules spatial cancellation cross-runtime browser documentation evidence/u);
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
for (const subject of ["rules", "spatial", "cancellation", "cross-runtime", "historical-compatibility", "governance", "production-browser", "documentation", "performance", "sdd-verify"]) assert.match(fullQualification, new RegExp(`"${subject}"`, "u"));
assert.equal(gateOrder.length, 7);

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
