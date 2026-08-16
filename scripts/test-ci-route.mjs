import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname } from "node:path";
import { gateOrder, gateParts, producerOrder, subjectOrder, gateResult, joinRoute, routePaths, feedbackBudgetMilliseconds, routeSchema, gateSchema, joinSchema, timingSchema } from "./ci-route.mjs";

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
  artifactDigest: gate.startsWith("prepare-") ? producerDigests[gate.slice("prepare-".length)] : (gateParts[gate] ?? []).length > 0 ? bindingDigest(Object.fromEntries(gateParts[gate].map((part) => [part, producerDigests[part]]))) : null,
  receiptReused: (gateParts[gate] ?? []).length > 0,
  ...overrides,
});
const passing = expectedSubjects.map((gate, index) => resultFor(gate, "pass", { cacheHit: index === 0, buildInvocations: gate.startsWith("prepare-") ? [gate] : [] }));
const joined = joinRoute(domain, passing, { startedAtMilliseconds: 1_000, completedAtMilliseconds: 299_000 });
assert.equal(joined.result, "pass");
assert.equal(joined.timing.subject, "runner-feedback");
assert.equal(joined.timing.receiptReuses, domain.selectedGates.length - 1);
assert.equal(joined.gateResults[0].timingMilliseconds.queue, null);
assert.equal(joined.gateResults[0].timingMilliseconds.transport, 0);
assert.equal(joined.timing.runnerMilliseconds, expectedSubjects.length * 1_000);
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
assert.throws(() => gateResult("unknown", "pass"), /unknown gate result/u);

const workflow = readFileSync(new URL("../.github/workflows/ci.yml", import.meta.url), "utf8");
const jobBody = (name) => new RegExp(`^  ${name}:\\n([\\s\\S]*?)(?=^  [a-z][a-z-]*:\\n)`, "mu").exec(workflow)?.[1] ?? "";
const contracts = JSON.parse(readFileSync(new URL("../tests/fixtures/ci-qualification/v1/contracts.json", import.meta.url), "utf8"));
assert.equal(contracts.feedbackBudgetMilliseconds, feedbackBudgetMilliseconds);
assert.deepEqual(contracts.gateOrder, gateOrder);
assert.deepEqual(contracts.subjectOrder, subjectOrder);
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
assert.match(workflow, /browser:\n[\s\S]*?needs: \[route, integrity, prepare-web, prepare-server\]/u);
assert.match(workflow, /documentation:\n[\s\S]*?needs: \[route, integrity, prepare-web, prepare-docs\]/u);
assert.match(jobBody("browser"), /actions\/setup-dotnet@v4/u);
assert.doesNotMatch(jobBody("rules"), /prepared-part-(?:fable|web|server|docs)/u);
assert.doesNotMatch(jobBody("spatial"), /prepared-part-(?:web|server|docs)/u);
assert.doesNotMatch(jobBody("browser"), /prepared-part-(?:native|fable|docs)/u);
assert.doesNotMatch(jobBody("documentation"), /prepared-part-(?:native|fable|server)/u);
assert.doesNotMatch(jobBody("cancellation"), /prepared-part-/u);
const gateRunner = readFileSync(new URL("./run-ci-gate.sh", import.meta.url), "utf8");
assert.match(gateRunner, /spatial\|cross-runtime\) preflight_parts=\(native fable\)/u);
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
assert.ok(focusedQualification.indexOf('wait "$release_pid"') < focusedQualification.indexOf("part_paths=("));
assert.doesNotMatch(focusedQualification, /find src tests -type d.*-name obj/u);
assert.doesNotMatch(focusedQualification, /--output .*obj/u);
assert.match(focusedQualification, /dotnet restore SIR\.slnx --locked-mode/u);
assert.match(focusedQualification, /trap write_part_timing EXIT/u);
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
