import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { planFor, subjectOrder, sweepEnvironmentVariable, sweepRequested } from "./ci-integrity-plan.mjs";
import { routePaths } from "./ci-route.mjs";

const route = (paths) => routePaths(paths, { commit: "a".repeat(40), tree: "b".repeat(40) });
const byId = (plan) => new Map(plan.subjects.map((subject) => [subject.id, subject]));

const browser = byId(planFor(route(["src/SIR.Client/App.fs"])));
assert.ok(subjectOrder.every((id) => browser.has(id)));
assert.ok([...browser.values()].every(({ run, reason }) => run === false && reason === "measured-omission"));

const packagePlan = byId(planFor(route(["package-lock.json"])));
assert.equal(packagePlan.get("npm-audit").run, true);
assert.ok([...packagePlan.values()].filter(({ id }) => id !== "npm-audit").every(({ run }) => run === false));
const topology = planFor(route([".github/workflows/ci.yml"]));
assert.ok(topology.subjects.every(({ run, reason }) => run && reason === "topology-change"));
const feedback = byId(planFor(route(["feedback/checkpoints/current.jsonl"])));
assert.equal(feedback.get("feedback-audit").run, true);
assert.equal(feedback.get("feedback-audit").reason, "relevant-path");
assert.ok([...feedback.values()].filter(({ id }) => id !== "feedback-audit").every(({ run }) => run === false));
const project = byId(planFor(route(["src/SIR.Domain/SIR.Domain.fsproj"])));
assert.equal(project.get("dependency-surface").run, true);
assert.equal(project.get("npm-audit").run, false);
const self = planFor(route(["scripts/ci-integrity-plan.mjs"]));
assert.ok(self.subjects.every(({ run, reason }) => run && reason === "classifier-self-change"));
const unknown = planFor(route(["unknown/new-topology.file"]));
assert.ok(unknown.subjects.every(({ run, reason }) => run && reason === "unknown-conservative"));
assert.throws(() => planFor({ schema: "sir.ci-route/v2", paths: [] }), /malformed route/u);

// ---------------------------------------------------------------------------
// #252: a path-conditional subject must not be able to stay red on the default
// branch unobserved. The sweep is the unconditional counterpart to per-PR
// selection, and these assertions are what go red if it is removed or widened.
// ---------------------------------------------------------------------------

// The precondition the defect needed: a realistic default-branch commit whose paths select nothing.
// Every #252 acceptance claim below rests on this staying true, so assert it rather than assume it.
const omittedPaths = ["src/SIR.Client/App.fs", "docs/architecture.md"];
const conditional = planFor(route(omittedPaths));
assert.equal(conditional.mode, "pull-request");
assert.ok(
  conditional.subjects.every(({ run, reason }) => run === false && reason === "measured-omission"),
  "the sweep fixture must be paths that select no subject, or it proves nothing",
);

// #252 is a defect about a gate that silently did not run, so pin the subject inventory ABSOLUTELY.
// Every other assertion here is relative to `subjectOrder`; without this line, deleting a subject
// shrinks the sweep and the whole suite still passes — the same class of silence being repaired.
const declaredSubjects = ["npm-audit", "governance", "dependency-surface", "sdd-byte-stability", "feedback-audit"];
assert.deepEqual(subjectOrder, declaredSubjects, "an integrity subject was added or removed; update this pin deliberately");

// The sweep runs every subject over exactly those paths.
const swept = planFor(route(omittedPaths), { sweep: true });
assert.equal(swept.mode, "sweep");
assert.deepEqual(swept.subjects.map(({ id }) => id), declaredSubjects, "the sweep must cover every declared subject");
assert.ok(subjectOrder.every((id) => byId(swept).has(id)), "the sweep must cover every declared subject");
assert.ok(
  swept.subjects.every(({ run, reason }) => run === true && reason === "scheduled-sweep"),
  "every swept subject runs, and says why it ran",
);
// A future subject added to subjectOrder is swept automatically; nothing here enumerates subjects.
assert.equal(swept.subjects.length, subjectOrder.length);

// The sweep stays honest about the selection it overrode: it records that the predicates chose
// nothing. That is what makes an archived sweep plan readable as evidence of this defect class.
assert.ok(swept.subjects.every(({ matchingPaths }) => matchingPaths.length === 0));

// The two modes are distinguishable in the sealed artifact, not merely in behaviour.
assert.notEqual(swept.digest, conditional.digest);

// AC4 — the sweep must not widen per-PR selection. Sweeping is opt-in and off by default, and the
// conditional branch is byte-identical to its pre-sweep behaviour for the same route.
assert.equal(planFor(route(omittedPaths)).digest, conditional.digest);
assert.deepEqual(
  planFor(route(["package-lock.json"])).subjects.map(({ id, run }) => [id, run]),
  [["npm-audit", true], ["governance", false], ["dependency-surface", false], ["sdd-byte-stability", false], ["feedback-audit", false]],
  "per-PR selection must stay path-conditional",
);

// Activation is explicit: only the exact string "true" sweeps.
assert.equal(sweepRequested({ [sweepEnvironmentVariable]: "true" }), true);
assert.equal(sweepRequested({ [sweepEnvironmentVariable]: "false" }), false);
assert.equal(sweepRequested({ [sweepEnvironmentVariable]: "1" }), false);
assert.equal(sweepRequested({}), false);

// ---------------------------------------------------------------------------
// A correct planner that nothing invokes is exactly the #252 failure with a
// better-looking mechanism, so assert the wiring in ci.yml, not just the module.
// ---------------------------------------------------------------------------
const ci = readFileSync(new URL("../.github/workflows/ci.yml", import.meta.url), "utf8");
const jobs = ci.slice(ci.indexOf("\njobs:\n") + 7);
const headers = [...jobs.matchAll(/^ {2}([a-z0-9-]+):$/gmu)];
const jobBody = (name) => {
  const index = headers.findIndex(([, id]) => id === name);
  assert.notEqual(index, -1, `ci.yml has no ${name} job — the unconditional integrity signal is missing`);
  return jobs.slice(headers[index].index, headers[index + 1]?.index ?? jobs.length);
};

const sweepJob = jobBody("integrity-sweep");
assert.match(sweepJob, /^ {4}if: github\.event_name != 'pull_request'$/mu, "the sweep must never run on a pull request");
assert.match(sweepJob, new RegExp(`^ {10}${sweepEnvironmentVariable}: "true"$`, "mu"), "the sweep job must activate sweep mode");
assert.match(sweepJob, /run-ci-gate\.sh integrity /u, "the sweep must run the integrity gate, not a private copy of it");
assert.match(ci, /^ {2}schedule:\n {4}- cron: "[^"]+"$/mu, "the sweep needs a schedule to be a scheduled signal");

// AC4 again, from the other side: the per-PR integrity job must not have acquired sweep mode.
const prIntegrity = jobBody("integrity");
assert.match(prIntegrity, /^ {4}if: github\.event_name == 'pull_request'$/mu);
assert.doesNotMatch(prIntegrity, new RegExp(sweepEnvironmentVariable, "u"), "sweeping a pull request would undo #248's cost work");

console.log("Integrity planning preserves an unconditional floor, fails conservative for unknown, topology, workflow, and classifier changes, records explicit measured omissions, and carries an unconditional off-PR sweep so no subject can stay red on the default branch unobserved.");
