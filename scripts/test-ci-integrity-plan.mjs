import assert from "node:assert/strict";
import { planFor, subjectOrder } from "./ci-integrity-plan.mjs";
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
console.log("Integrity planning preserves an unconditional floor and fails conservative for unknown, topology, workflow, and classifier changes while recording explicit measured omissions.");
