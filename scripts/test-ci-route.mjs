import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname } from "node:path";
import { canonicalArtifactBindings, expectedBuildInvocations, gateOrder, gateParts, producerOrder, subjectOrder, gateResult, joinRoute, routePaths, feedbackBudgetMilliseconds, feedbackAcceptanceTargetMilliseconds, feedbackHeadroomMilliseconds, routeSchema, gateSchema, joinSchema, timingSchema } from "./ci-route.mjs";

const route = (paths) => routePaths(paths, { commit: "a".repeat(40), tree: "b".repeat(40) });

assert.equal(route(["docs/index.md"]).classification, "documentation");
assert.deepEqual(route(["docs/index.md"]).selectedGates, ["documentation", "evidence"]);
assert.equal(route(["docs/index.md", "work/220-bounded-pr-ci/spec.md"]).classification, "documentation");
assert.equal(route(["src/SIR.Domain/Rules.fs"]).classification, "domain");
assert.equal(route(["src/SIR.Domain/Rules.fs", "readiness/220-bounded-pr-ci/ship-verdict.json"]).classification, "domain");
assert.equal(route(["tests/SIR.Browser.Tests/journey.js"]).classification, "browser");
assert.deepEqual(route(["feedback/checkpoints/220.jsonl"]).selectedGates, ["evidence"]);
assert.equal(route(["src/SIR.Domain/Rules.fs", "docs/index.md"]).classification, "cross-cutting");
assert.equal(route(["future/feature.xyz"]).classification, "cross-cutting");
assert.equal(route([".\\docs\\index.md"]).classification, "documentation");
assert.throws(() => route([]), /inventory is empty/u);
assert.throws(() => route(["../secret"]), /malformed changed path/u);

const domain = route(["src/SIR.Domain/Rules.fs"]);
const producerDigests = { native: "c".repeat(64), fable: "d".repeat(64) };
const bindingDigest = (bindings) => createHash("sha256").update(`${JSON.stringify(bindings, null, 2)}\n`).digest("hex");
const expectedSubjects = ["integrity", "prepare-native", "prepare-fable", ...domain.selectedGates];
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
const crossCutting = route(["src/SIR.Domain/Rules.fs", "docs/index.md"]);
const crossSubjects = ["integrity", ...producerOrder, ...crossCutting.selectedGates];
const crossResultFor = (gate) => gateResult(gate, "pass", { setup: 100, restore: 200, build: 300, test: 400, total: 1_000 }, {
  source: crossCutting.source,
  routeDigest: crossCutting.digest,
  artifactBindings: Object.fromEntries((gateParts[gate] ?? []).map((part) => [part, producerDigests[part] ?? "e".repeat(64)])),
  artifactDigest: gate.startsWith("prepare-") ? (producerDigests[gate.slice("prepare-".length)] ?? "e".repeat(64)) : (gateParts[gate] ?? []).length > 0 ? bindingDigest(canonicalArtifactBindings(Object.fromEntries(gateParts[gate].map((part) => [part, producerDigests[part] ?? "e".repeat(64)])))) : null,
  receiptReused: (gateParts[gate] ?? []).length > 0,
  buildInvocations: expectedBuildInvocations[gate],
});
const crossPassing = crossSubjects.map(crossResultFor);
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
const spatialBuilds = expectedBuildInvocations.spatial;
const withSpatialBuilds = (buildInvocations) => passing.map((result) => result.gate === "spatial" ? { ...result, buildInvocations } : result);
const anonymousSpatial = "build:src/SIR.Simulation/SIR.Simulation.fsproj";
const anonymousSpatialTrace = joinRoute(domain, withSpatialBuilds(Array(spatialBuilds.length).fill(anonymousSpatial)), { completedAtMilliseconds: 1 });
assert.ok(anonymousSpatialTrace.failures.some(({ code, invocation }) => code === "unknown-build-invocation" && invocation === anonymousSpatial));
assert.ok(anonymousSpatialTrace.failures.some(({ code, invocation, expected, actual }) => code === "duplicate-build-invocation" && invocation === anonymousSpatial && expected === 0 && actual === spatialBuilds.length));
assert.equal(anonymousSpatialTrace.failures.filter(({ code, subject }) => code === "missing-build-invocation" && subject === "spatial").length, spatialBuilds.length);
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
assert.deepEqual(expectedBuildInvocations.cancellation, [
  "fable:src/SIR.Client.Web/SIR.RulesExplorer.Web.fsproj:exception:cancellation-mutant",
  "fable:src/SIR.Replay.Web/SIR.Replay.Web.fsproj:exception:cancellation-mutant",
]);
assert.deepEqual(spatialBuilds, [
  "dependency-receipt", "footprint-envelope", "semantic-edge", "knowledge-cache-key", "spatial-revision-key",
  "deterministic-ordering", "package-adapter", "profile-cache-key", "trace-work-bound",
].map((name) => `build:src/SIR.Simulation/SIR.Simulation.fsproj:exception:spatial-${name}:artifacts-path:isolated`));
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
assert.deepEqual(contracts.schemas, { route: routeSchema, artifactManifest: "sir.ci-artifact-manifest/v1", gateResult: gateSchema, timing: timingSchema, join: joinSchema });
for (const job of ["route:", "integrity:", "prepare-native:", "prepare-fable:", "prepare-web:", "prepare-server:", "prepare-docs:", "rules:", "spatial:", "cancellation:", "cross-runtime:", "browser:", "documentation:", "evidence:", "pr-verdict:", "full-qualification:"]) assert.match(workflow, new RegExp(`^  ${job}$`, "mu"));
assert.match(workflow, /if: always\(\)/u);
assert.match(workflow, /if: always\(\) && github\.event_name == 'pull_request'/u);
assert.match(workflow, /schedule:/u);
assert.match(workflow, /cancel-in-progress: \$\{\{ github\.event_name == 'pull_request' \}\}/u);
assert.match(workflow, /hashFiles\('\*\*\/packages\.lock\.json', 'Directory\.Packages\.props', '\.config\/dotnet-tools\.json'\)/u);
assert.equal(workflow.match(/name: Capture runner start/gu)?.length, 16);
for (const part of ["native", "fable", "web", "server", "docs"]) assert.match(workflow, new RegExp(`qualify-pr\\.sh prepare-part ${part}`, "u"));
assert.doesNotMatch(workflow, /^  prepare:$/mu);
assert.doesNotMatch(workflow, /prepared-candidate/u);
assert.match(workflow, /rules:\n[\s\S]*?needs: \[route, integrity, prepare-native\]/u);
assert.match(workflow, /cancellation:\n[\s\S]*?needs: \[route, integrity, prepare-native, prepare-web\]/u);
assert.match(workflow, /browser:\n[\s\S]*?needs: \[route, integrity, prepare-web, prepare-server\]/u);
assert.match(workflow, /documentation:\n[\s\S]*?needs: \[route, integrity, prepare-web, prepare-docs\]/u);
assert.match(jobBody("browser"), /actions\/setup-dotnet@v4/u);
assert.doesNotMatch(jobBody("rules"), /prepared-part-(?:fable|web|server|docs)/u);
assert.doesNotMatch(jobBody("spatial"), /prepared-part-(?:web|server|docs)/u);
assert.doesNotMatch(jobBody("browser"), /prepared-part-(?:native|fable|docs)/u);
assert.doesNotMatch(jobBody("documentation"), /prepared-part-(?:native|fable|server)/u);
assert.match(jobBody("cancellation"), /prepared-part-native/u);
assert.match(jobBody("cancellation"), /prepared-part-web/u);
assert.match(jobBody("prepare-server"), /needs: route/u);
assert.doesNotMatch(jobBody("prepare-server"), /prepared-part-web|qualify-pr\.sh extract-parts web/u);
const gateRunner = readFileSync(new URL("./run-ci-gate.sh", import.meta.url), "utf8");
assert.match(gateRunner, /spatial\|cross-runtime\) preflight_parts=\(native fable\)/u);
assert.match(gateRunner, /cancellation\) preflight_parts=\(native web\)/u);
assert.match(gateRunner, /browser\) preflight_parts=\(web server\)/u);
assert.match(gateRunner, /documentation\) preflight_parts=\(web docs\)/u);
assert.match(workflow, /for gate in integrity prepare-native prepare-fable prepare-web prepare-server prepare-docs rules spatial cancellation cross-runtime browser documentation evidence/u);
assert.match(workflow, /\.\/scripts\/qualify-production\.sh --protected/u);
const fullQualification = readFileSync(new URL("./qualify-production.sh", import.meta.url), "utf8");
assert.ok(fullQualification.indexOf("dotnet restore SIR.slnx --locked-mode") < fullQualification.indexOf("test-conformance.sh"));
assert.ok(fullQualification.indexOf("dotnet-invocation-trace.sh") < fullQualification.indexOf("test-worker-cancellation-subject-mutation.sh"));
assert.match(fullQualification, /verify-spatial-query\.sh --static-only/u);
assert.match(fullQualification, /fable_target_builds=.*\.total/u);
const focusedQualification = readFileSync(new URL("./qualify-pr.sh", import.meta.url), "utf8");
const conformanceQualification = readFileSync(new URL("./test-conformance.sh", import.meta.url), "utf8");
const spatialMutationQualification = readFileSync(new URL("./test-spatial-subject-mutations.sh", import.meta.url), "utf8");
const simulationProject = readFileSync(new URL("../src/SIR.Simulation/SIR.Simulation.fsproj", import.meta.url), "utf8");
const browserConfiguration = readFileSync(new URL("../tests/SIR.Browser.Tests/playwright.config.js", import.meta.url), "utf8");
const browserShards = readFileSync(new URL("./test-browser-shards.mjs", import.meta.url), "utf8");
const packageManifest = JSON.parse(readFileSync(new URL("../package.json", import.meta.url), "utf8"));
const matchQualification = readFileSync(new URL("../tests/SIR.Match.Tests/Program.fs", import.meta.url), "utf8");
const focusedNativeProducer = /      native\)\n(?<body>[\s\S]*?)\n        ;;/u.exec(focusedQualification);
assert.ok(focusedNativeProducer?.groups?.body, "focused native producer block is missing");
assert.equal(focusedNativeProducer.groups.body.match(/dotnet build SIR\.slnx -c Release --no-restore/gu)?.length, 1);
assert.doesNotMatch(focusedQualification, /find src tests -type d.*-name obj/u);
assert.doesNotMatch(focusedQualification, /--output .*obj/u);
assert.match(focusedQualification, /dotnet restore SIR\.slnx --locked-mode/u);
assert.match(focusedQualification, /trap write_part_timing EXIT/u);
assert.match(focusedQualification, /verify-staged[\s\S]*cp -a "\$stage\/\$output_path" "\$target"/u);
assert.match(focusedQualification, /browser\)[\s\S]*compose-browser[\s\S]*npm run test:browser/u);
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
assert.match(focusedQualification, /spatial\).*--prepared-pr/u);
assert.match(focusedQualification, /cancellation\).*--prepared-pr/u);
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
assert.match(browserConfiguration, /workers: 1/u);
assert.match(browserConfiguration, /process\.env\.SIR_BROWSER_PORT/u);
assert.match(browserShards, /process\.env\.CI \? Math\.min\(2, browserShardCapacity\) : 1/u);
assert.match(browserShards, /--shard=\$\{index\}\/\$\{browserShards\}/u);
assert.match(browserShards, /SIR_BROWSER_PORT: String\(5100 \+ index - 1\)/u);
assert.match(browserShards, /mergeShardReports/u);
assert.match(browserShards, /left\.localeCompare\(right\)/u);
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
