import assert from "node:assert/strict";
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { dirname } from "node:path";
import { gateOrder, subjectOrder, gateResult, joinRoute, routePaths, feedbackBudgetMilliseconds, routeSchema, gateSchema, joinSchema, timingSchema } from "./ci-route.mjs";

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
const artifactDigest = "c".repeat(64);
const expectedSubjects = ["integrity", "prepare", ...domain.selectedGates];
const resultFor = (gate, status = "pass", overrides = {}) => gateResult(gate, status, { setup: 100, restore: 200, build: 300, test: 400, total: 1_000 }, {
  source: domain.source,
  routeDigest: domain.digest,
  artifactDigest: gate === "prepare" || (gateOrder.includes(gate) && gate !== "evidence") ? artifactDigest : null,
  receiptReused: gateOrder.includes(gate) && gate !== "evidence",
  ...overrides,
});
const passing = expectedSubjects.map((gate, index) => resultFor(gate, "pass", { cacheHit: index === 0, buildInvocations: gate === "prepare" ? ["domain"] : [] }));
const joined = joinRoute(domain, passing, { startedAtMilliseconds: 1_000, completedAtMilliseconds: 299_000, expectedArtifactDigest: artifactDigest });
assert.equal(joined.result, "pass");
assert.equal(joined.timing.subject, "runner-feedback");
assert.equal(joined.timing.receiptReuses, domain.selectedGates.length - 1);
assert.equal(joined.gateResults[0].timingMilliseconds.queue, null);
const overBudget = joinRoute(domain, passing, { startedAtMilliseconds: 0, completedAtMilliseconds: feedbackBudgetMilliseconds + 1, expectedArtifactDigest: artifactDigest });
assert.equal(overBudget.result, "fail");
assert.ok(overBudget.failures.some(({ code }) => code === "feedback-budget-exceeded"));
const aggregate = joinRoute(domain, [passing[0], passing[2], resultFor("spatial", "fail", { routeDigest: "d".repeat(64) }), resultFor("browser")], { completedAtMilliseconds: 1, expectedArtifactDigest: "e".repeat(64) });
assert.equal(aggregate.result, "fail");
for (const code of ["missing-gate-result", "required-gate-fail", "route-binding-mismatch", "unexpected-gate-result", "artifact-manifest-binding-mismatch"]) assert.ok(aggregate.failures.some((failure) => failure.code === code), code);
const staleCandidate = joinRoute(domain, passing.map((result) => result.gate === "rules" ? { ...result, source: { ...result.source, commit: "f".repeat(40) } } : result), { completedAtMilliseconds: 1, expectedArtifactDigest: artifactDigest });
assert.ok(staleCandidate.failures.some(({ code }) => code === "candidate-binding-mismatch"));
const tamperedRoute = { ...domain, paths: ["src/SIR.Domain/Tampered.fs"] };
const staleRoute = joinRoute(tamperedRoute, passing, { completedAtMilliseconds: 1, expectedArtifactDigest: artifactDigest });
assert.ok(staleRoute.failures.some(({ code }) => code === "route-digest-mismatch"));
assert.throws(() => gateResult("unknown", "pass"), /unknown gate result/u);

const workflow = readFileSync(new URL("../.github/workflows/ci.yml", import.meta.url), "utf8");
const contracts = JSON.parse(readFileSync(new URL("../tests/fixtures/ci-qualification/v1/contracts.json", import.meta.url), "utf8"));
assert.equal(contracts.feedbackBudgetMilliseconds, feedbackBudgetMilliseconds);
assert.deepEqual(contracts.gateOrder, gateOrder);
assert.deepEqual(contracts.subjectOrder, subjectOrder);
assert.deepEqual(contracts.schemas, { route: routeSchema, artifactManifest: "sir.ci-artifact-manifest/v1", gateResult: gateSchema, timing: timingSchema, join: joinSchema });
for (const job of ["route:", "integrity:", "prepare:", "rules:", "spatial:", "cancellation:", "cross-runtime:", "browser:", "documentation:", "evidence:", "pr-verdict:", "full-qualification:"]) assert.match(workflow, new RegExp(`^  ${job}$`, "mu"));
assert.match(workflow, /if: always\(\)/u);
assert.match(workflow, /if: always\(\) && github\.event_name == 'pull_request'/u);
assert.match(workflow, /schedule:/u);
assert.match(workflow, /cancel-in-progress: \$\{\{ github\.event_name == 'pull_request' \}\}/u);
assert.match(workflow, /hashFiles\('\*\*\/packages\.lock\.json', 'Directory\.Packages\.props', '\.config\/dotnet-tools\.json'\)/u);
assert.equal(workflow.match(/name: Capture runner start/gu)?.length, 12);
assert.match(workflow, /prepare:\n    if:[\s\S]*?needs: route/u);
assert.match(workflow, /prepared-candidate\.tar/u);
assert.match(workflow, /qualify-pr\.sh extract-prepared/u);
assert.match(workflow, /for gate in integrity prepare rules spatial cancellation cross-runtime browser documentation evidence/u);
assert.match(workflow, /--artifact-manifest/u);
assert.match(workflow, /\.\/scripts\/qualify-production\.sh --protected/u);
const fullQualification = readFileSync(new URL("./qualify-production.sh", import.meta.url), "utf8");
assert.ok(fullQualification.indexOf("dotnet restore SIR.slnx --locked-mode") < fullQualification.indexOf("test-conformance.sh"));
assert.ok(fullQualification.indexOf("dotnet-invocation-trace.sh") < fullQualification.indexOf("test-worker-cancellation-subject-mutation.sh"));
assert.match(fullQualification, /verify-spatial-query\.sh --static-only/u);
assert.match(fullQualification, /fable_target_builds=.*\.total/u);
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
