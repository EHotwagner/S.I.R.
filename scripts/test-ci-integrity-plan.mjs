import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { spawnSync } from "node:child_process";
import { costBoundedSubjects, planFor, subjectOrder, sweepEnvironmentVariable, sweepRequested } from "./ci-integrity-plan.mjs";
import { routePaths } from "./ci-route.mjs";

const route = (paths) => routePaths(paths, { commit: "a".repeat(40), tree: "b".repeat(40) });
const byId = (plan) => new Map(plan.subjects.map((subject) => [subject.id, subject]));

// S.I.R.#265. Two omission reasons now exist, and they mean different things: "the predicates did
// not match" vs "the predicates did not match AND this subject does not take the conservative
// fallbacks". Every assertion below that ranges over ALL subjects goes through this, so a subject
// silently changing which kind of omission it reports cannot pass unnoticed.
const omissionReason = (id) => (costBoundedSubjects.has(id) ? "cost-bounded-omission" : "measured-omission");
const conservativeSubjects = subjectOrder.filter((id) => !costBoundedSubjects.has(id));
assert.ok(costBoundedSubjects.size > 0 && conservativeSubjects.length > 0, "both partitions must be non-empty or the assertions below range over nothing");

const browser = byId(planFor(route(["src/SIR.Client/App.fs"])));
assert.ok(subjectOrder.every((id) => browser.has(id)));
assert.ok([...browser.values()].every(({ id, run, reason }) => run === false && reason === omissionReason(id)));

const packagePlan = byId(planFor(route(["package-lock.json"])));
assert.equal(packagePlan.get("npm-audit").run, true);
assert.ok([...packagePlan.values()].filter(({ id }) => id !== "npm-audit").every(({ run }) => run === false));
const topology = byId(planFor(route([".github/workflows/ci.yml"])));
assert.ok(conservativeSubjects.every((id) => topology.get(id).run && topology.get(id).reason === "topology-change"));
const feedback = byId(planFor(route(["feedback/checkpoints/current.jsonl"])));
assert.equal(feedback.get("feedback-audit").run, true);
assert.equal(feedback.get("feedback-audit").reason, "relevant-path");
assert.ok([...feedback.values()].filter(({ id }) => id !== "feedback-audit").every(({ run }) => run === false));
const project = byId(planFor(route(["src/SIR.Domain/SIR.Domain.fsproj"])));
assert.equal(project.get("dependency-surface").run, true);
assert.equal(project.get("npm-audit").run, false);
const self = byId(planFor(route(["scripts/ci-integrity-plan.mjs"])));
assert.ok(conservativeSubjects.every((id) => self.get(id).run && self.get(id).reason === "classifier-self-change"));
const unknown = byId(planFor(route(["unknown/new-topology.file"])));
assert.ok(conservativeSubjects.every((id) => unknown.get(id).run && unknown.get(id).reason === "unknown-conservative"));
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
  conditional.subjects.every(({ id, run, reason }) => run === false && reason === omissionReason(id)),
  "the sweep fixture must be paths that select no subject, or it proves nothing",
);

// #252 is a defect about a gate that silently did not run, so pin the subject inventory ABSOLUTELY.
// Every other assertion here is relative to `subjectOrder`; without this line, deleting a subject
// shrinks the sweep and the whole suite still passes — the same class of silence being repaired.
const declaredSubjects = ["npm-audit", "governance", "dependency-surface", "sdd-byte-stability", "feedback-audit", "review-contract"];
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

// AC4 — the sweep must not widen per-PR selection. Sweeping is opt-in and off by default.
//
// Note precisely what is and is not claimed. Per-PR SELECTION is unchanged, and that is what AC4
// requires and what these pins assert. The plan DIGEST is deliberately NOT claimed to be unchanged:
// `mode` lives inside the digested body, so every plan digest differs from its pre-sweep value. No
// consumer compares plan digests across versions, so that is a versioning fact, not a regression.
// Pinning selection absolutely — literal expected tuples, not a comparison against another call of
// the same function — is what makes these assertions capable of failing.
const conditionalSelection = (paths) => planFor(route(paths)).subjects.map(({ id, run, reason }) => [id, run, reason]);
const allOmitted = (ids) => ids.map((id) => [id, false, omissionReason(id)]);

assert.deepEqual(conditionalSelection(omittedPaths), allOmitted(declaredSubjects), "an inert route must select nothing");
assert.deepEqual(
  conditionalSelection(["package-lock.json"]),
  [["npm-audit", true, "relevant-path"], ["governance", false, "measured-omission"], ["dependency-surface", false, "measured-omission"], ["sdd-byte-stability", false, "measured-omission"], ["feedback-audit", false, "measured-omission"], ["review-contract", false, "cost-bounded-omission"]],
  "per-PR selection must stay path-conditional",
);
assert.deepEqual(
  conditionalSelection(["scripts/audit-binding-exceptions.json"]),
  [["npm-audit", false, "measured-omission"], ["governance", false, "measured-omission"], ["dependency-surface", false, "measured-omission"], ["sdd-byte-stability", false, "measured-omission"], ["feedback-audit", true, "relevant-path"], ["review-contract", false, "cost-bounded-omission"]],
  "the subject this item repairs must still be selected by its own paths on a pull request",
);

// ---------------------------------------------------------------------------
// S.I.R.#265 — the review-contract subject, from both sides.
//
// Absolute tuples, not a comparison against another call of the same function: the defect this
// row repairs is a gate that existed and that no route selected, and a relative assertion cannot
// tell "selected" from "the predicate returned the same thing twice".
// ---------------------------------------------------------------------------

// SELECTED, one path at a time. Each of these five is a path the gate script actually opens, so
// each must be able to select it ON ITS OWN — a set-union assertion would pass while four of the
// five were dead.
for (const path of [
  "docs/coordination-engine-contracts.md",
  ".config/dotnet-tools.json",
  "global.json",
  "scripts/fsgg-coord",
  "scripts/test-review-contract-coherence.sh",
]) {
  const selected = byId(planFor(route([path]))).get("review-contract");
  assert.equal(selected.run, true, `${path} must select review-contract on its own`);
  assert.equal(selected.reason, "relevant-path", `${path} must select review-contract by PATH, not by a conservative fallback`);
  assert.deepEqual(selected.matchingPaths, [path], `${path} must be recorded as the reason it was selected`);
}

// NOT SELECTED. #309's defect was a job graph that could not be satisfied because a gate was
// declared where nothing could run it; the mirror of that is a subject selected where it cannot
// find anything. #265's Scope names the packed skill mirrors as selectors; they are not, and both
// halves of the reason are asserted here rather than argued in prose.
//
// The gate never opens them (falsify a load-bearing claim in one and it still exits 0), and
// `.claude/`/`.agents/` are outside every classifier prefix, so the router files them under
// RP-005-unknown-conservative — the fallback this subject is exempt from. A mirror-only change
// therefore runs the other five subjects and not this one.
const mirrors = [".claude/skills/pnext-item/references/independent-review.md", ".agents/skills/pnext-item/references/independent-review.md"];
const mirrorPlan = byId(planFor(route(mirrors)));
assert.deepEqual(
  mirrorPlan.get("review-contract").matchingPaths,
  [],
  "no review-contract predicate may match a packed skill mirror: the gate never opens those files",
);
assert.deepEqual(
  [mirrorPlan.get("review-contract").run, mirrorPlan.get("review-contract").reason],
  [false, "cost-bounded-omission"],
  "a mirror-only change must not run review-contract, and must say WHY it was omitted",
);
assert.ok(
  conservativeSubjects.every((id) => mirrorPlan.get(id).run && mirrorPlan.get(id).reason === "unknown-conservative"),
  "the exemption is per-subject: the other five must still take the conservative fallback on the same route",
);

// ---------------------------------------------------------------------------
// S.I.R.#265 — the conservative exemption, from both sides.
//
// The exemption is the one change here that touches a decision the other five subjects share, so
// it is pinned twice: the exempt subject must be omitted on every fallback route, and the
// non-exempt subjects must be BIT-FOR-BIT what the pre-#265 expression produced. The second is
// asserted against an independent re-implementation of that expression rather than against another
// call of `planFor`, which would only prove the function agrees with itself.
// ---------------------------------------------------------------------------
const fallbackRoutes = {
  "classifier-self-change": ["scripts/qualify-pr.sh"],
  "topology-change": [".github/workflows/ci.yml"],
  "unknown-conservative": ["unknown/new-topology.file"],
};
for (const [expected, paths] of Object.entries(fallbackRoutes)) {
  const plan = byId(planFor(route(paths)));
  assert.deepEqual(
    [plan.get("review-contract").run, plan.get("review-contract").reason],
    [false, "cost-bounded-omission"],
    `${expected}: a cost-bounded subject must not be pulled in by a conservative fallback`,
  );
  for (const id of conservativeSubjects) {
    assert.deepEqual(
      [plan.get(id).run, plan.get(id).reason],
      [true, expected],
      `${expected}: ${id} must be unaffected by the exemption`,
    );
  }
}

// The pre-#265 selection expression, transcribed. If the refactor that introduced the exemption
// changed anything for a non-exempt subject, these disagree.
// `matchingPaths` is read back from the plan on purpose: the exemption changes only how a
// fallback is APPLIED, never what a predicate matches, so re-deriving the predicates here would
// duplicate the thing that did not change and miss the thing that did.
const legacySelection = (paths) => {
  const routed = route(paths);
  const plan = byId(planFor(routed));
  const selfChange = routed.paths.some((path) => ["scripts/ci-integrity-plan.mjs", "scripts/test-ci-integrity-plan.mjs", "scripts/qualify-pr.sh"].includes(path));
  const topologyChange = routed.paths.some((path) => path === ".github/workflows" || path.startsWith(".github/workflows/"));
  const unknown = routed.facts?.some(({ rule }) => rule === "RP-005-unknown-conservative") ?? true;
  return conservativeSubjects.map((id) => {
    const run = selfChange || topologyChange || unknown || plan.get(id).matchingPaths.length > 0;
    return [id, run, selfChange ? "classifier-self-change"
      : topologyChange ? "topology-change"
      : unknown ? "unknown-conservative"
      : run ? "relevant-path" : "measured-omission"];
  });
};
for (const paths of [
  ["scripts/qualify-pr.sh"],
  [".github/workflows/ci.yml"],
  ["unknown/new-topology.file"],
  ["package-lock.json"],
  ["scripts/audit-binding-exceptions.json"],
  ["docs/coordination-engine-contracts.md"],
  [".config/dotnet-tools.json"],
  ["src/SIR.Client/App.fs", "docs/architecture.md"],
  mirrors,
]) {
  const actual = byId(planFor(route(paths)));
  assert.deepEqual(
    conservativeSubjects.map((id) => [id, actual.get(id).run, actual.get(id).reason]),
    legacySelection(paths),
    `the exemption changed a non-exempt subject's selection on ${paths.join(", ")}`,
  );
}

// Self-test: the transcription above must be capable of disagreeing, or the loop proves nothing.
assert.notDeepEqual(
  legacySelection(["package-lock.json"]),
  legacySelection(["unknown/new-topology.file"]),
  "legacy-selection self-test: the transcribed expression must distinguish two routes",
);

// The document is a `docs/` path, and `docs/` is the classification the sweep fixture uses for a
// route that selects NOTHING. Pin that these two do not collapse into each other, or the
// selection above and the omission above are the same assertion written twice.
assert.deepEqual(
  conditionalSelection(["docs/architecture.md"]),
  allOmitted(declaredSubjects),
  "an unrelated docs/ path must not select review-contract",
);

// The plan is a sealed artifact that `qualify-pr.sh` reads with jq and CI archives for 30 days, so
// pin its SHAPE too. Selection pins alone cannot see a field appearing in or vanishing from the
// digested body, and that is a schema change to a consumed artifact, not an internal detail.
for (const [label, plan] of [["pull-request", conditional], ["sweep", swept]]) {
  assert.deepEqual(
    Object.keys(plan).sort(),
    ["alwaysOn", "digest", "mode", "routeDigest", "schema", "source", "subjects"],
    `${label} plan shape drifted`,
  );
  for (const subject of plan.subjects) {
    assert.deepEqual(Object.keys(subject).sort(), ["id", "matchingPaths", "reason", "run"], `${label} subject shape drifted`);
  }
}

// Activation is explicit: only the exact string "true" sweeps.
assert.equal(sweepRequested({ [sweepEnvironmentVariable]: "true" }), true);
assert.equal(sweepRequested({ [sweepEnvironmentVariable]: "false" }), false);
assert.equal(sweepRequested({ [sweepEnvironmentVariable]: "1" }), false);
assert.equal(sweepRequested({}), false);

// ---------------------------------------------------------------------------
// S.I.R.#265 — the DECLARED subject set and the GATED subject set are the same
// set, and this is the assertion that says so.
//
// This is the hole #265 fell through, one level up. `subjectOrder` decides what the plan
// selects and what the sweep runs; `qualify-pr.sh` decides what actually EXECUTES. Nothing
// joined them, so a subject could be planned, selected, recorded in an archived sweep plan as
// `run: true` — and dispatched by nothing. That is indistinguishable from a subject that ran
// and passed, which is the same shape as the decorative gate this row repairs and as the six
// differentials that measured nothing on this board.
//
// Asserted in BOTH directions on purpose. A subject with no dispatch is a gate that cannot
// fire. A dispatch whose id is in no plan is worse than dead code: `integrity_runs` asks jq for
// a subject the plan does not contain, jq exits non-zero, and the `if` takes the else branch
// forever — a dispatch that is silently skipped on every run rather than one that errors.
//
// The set equality is derived from the SUBJECT (the committed shell), not from a second list
// maintained here; a hand-copied expectation would be one edit away from agreeing with itself.
// ---------------------------------------------------------------------------
const qualify = readFileSync(new URL("../scripts/qualify-pr.sh", import.meta.url), "utf8");
const integrityCase = qualify.slice(qualify.indexOf("\n  integrity)\n"));
const caseEnd = integrityCase.indexOf("\n    ;;\n");
assert.ok(
  integrityCase.startsWith("\n  integrity)\n") && caseEnd > 0,
  "could not locate qualify-pr.sh's `integrity)` case block — refusing rather than deciding over a slice that may be the whole file",
);
const integrityBody = integrityCase.slice(0, caseEnd);

// The guard SHAPE is pinned, not merely the id: a looser scan would also match the
// `integrity_runs()` definition itself, an `integrity_runs x || true`, or a mention inside a
// comment, and would then report a dispatch where none exists. Both committed layouts are
// admitted — the one-liner (`; then <cmd>; fi`) and the multi-line block (`; then` at end of
// line) — because the anchor is the guard, and which side of it the body sits on is style.
const dispatched = [...integrityBody.matchAll(/^ +if integrity_runs ([a-z0-9-]+); then(?: .*)?$/gmu)].map(([, id]) => id);
assert.ok(dispatched.length > 0, "found no `if integrity_runs …; then` guard at all — the scan below would pass vacuously");
assert.equal(
  new Set(dispatched).size,
  dispatched.length,
  `qualify-pr.sh dispatches a subject twice (${dispatched.join(", ")}); the second guard is unreachable work`,
);
assert.deepEqual(
  [...dispatched].sort(),
  [...subjectOrder].sort(),
  `the planned subject set and the dispatched subject set disagree.\n`
    + `  planned but never dispatched (a subject nothing runs): ${subjectOrder.filter((id) => !dispatched.includes(id)).join(", ") || "(none)"}\n`
    + `  dispatched but never planned (a guard that is skipped on every run): ${dispatched.filter((id) => !subjectOrder.includes(id)).join(", ") || "(none)"}`,
);

// Self-test, in the shape this repo already applies to pr-verdict's collection-coverage check: a
// comparison that has never been red is equally consistent with "nothing was wrong" and "it
// cannot fire". Both directions, because the message above claims both.
{
  const disagree = (planned, gated) => planned.filter((id) => !gated.includes(id)).concat(gated.filter((id) => !planned.includes(id)));
  assert.equal(disagree(["a", "b"], ["a", "b"]).length, 0, "plan/dispatch self-test: agreement must read as agreement");
  assert.deepEqual(disagree(["a", "b"], ["a"]), ["b"], "plan/dispatch self-test: a planned-but-undispatched subject must be detectable");
  assert.deepEqual(disagree(["a"], ["a", "b"]), ["b"], "plan/dispatch self-test: a dispatched-but-unplanned guard must be detectable");
}

// ---------------------------------------------------------------------------
// A correct planner that nothing invokes is exactly the #252 failure with a
// better-looking mechanism, so assert the wiring in ci.yml, not just the module.
// ---------------------------------------------------------------------------
const ci = readFileSync(new URL("../.github/workflows/ci.yml", import.meta.url), "utf8");
const jobsIndex = ci.indexOf("\njobs:\n");
assert.notEqual(jobsIndex, -1, "ci.yml has no jobs: block — the workflow parse below would silently degrade");
const jobs = ci.slice(jobsIndex + 7);
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
assert.match(sweepJob, /^ {10}test -s artifacts\/ci\/changed-paths\.txt$/mu, "the sweep must refuse an empty path inventory rather than hand it to the router");

// Presence assertions alone cannot protect a signal: they say what must exist, never what must not,
// so a one-line ADDITION can neutralise the job while every "is it wired?" assertion stays green.
// Each of the three below was demonstrated to destroy the signal with the whole suite passing.

// 1. `continue-on-error: true` anywhere in the sweep makes the job report success while its gate fails.
assert.doesNotMatch(
  sweepJob,
  /continue-on-error/u,
  "the sweep must not tolerate a failing step: continue-on-error turns a red gate into a green job",
);

// 2. Pin the `if:` INVENTORY, not just the presence of the right one. `if: false` on the gate step
//    leaves every existing assertion true while nothing runs — "a correct planner that nothing
//    invokes", which is precisely the case this block exists to prevent.
assert.deepEqual(
  sweepJob.match(/^\s*-?\s*if:.*$/gmu).map((line) => line.trim()),
  ["if: github.event_name != 'pull_request'", "- if: always()"],
  "unexpected `if:` in the sweep job — a step-level condition can silently stop the gate running",
);

// The sweep's steps declare `shell: bash`, which GitHub runs as `bash --noprofile --norc -eo pipefail`.
// A consumer that stops reading before EOF (`head -n 1`, `grep -m1`, `sed -n '1{p;q}'`,
// `awk 'NR==1{...exit}'`) SIGPIPEs its producer, and under `pipefail` the pipeline status is 141 —
// which would fail this job on every push to `main`, making the only unconditional integrity signal
// the thing that reddens the branch.
//
// Asserting over the workflow TEXT cannot express that: the property is about the step's BEHAVIOUR,
// and a blocklist of one token (`head`) leaves three equivalents passing. So EXECUTE each pipeline's
// consumer instead, against a producer large enough that an early exit always has pending writes.
// Measured over 20 runs each: `sed -n '1p'` and `cat` fail 0/20; `head -n 1`, `grep -m1 ''`,
// `sed -n '1{p;q}'` and `awk 'NR==1{print; exit}'` each fail 20/20. Deterministic, not flaky.
// A real pipe, not the `||` of `… || true`.
const pipeAt = (line) => line.search(/[^|]\|[^|]/u);
// EVERY pipeline in the job, not only those whose producer happens to be one command. Scoping the
// scan to a producer prefix is the same defect this probe exists to catch: the comment and the
// failure message claim the property for the whole step, so the scan must cover the whole step.
const pipelines = sweepJob
  .split("\n")
  .map((line) => line.trim())
  .filter((line) => pipeAt(line) !== -1 && !line.startsWith("#"));
assert.ok(pipelines.length > 0, "expected the sweep to build its path inventory through a pipeline");
// Only a consumer chain is executed, and only against a synthetic producer. But a consumer could
// itself carry side effects, so fail CLOSED on anything not known to be a read-only text filter:
// extending the job's plumbing then stays a deliberate act rather than a silent one.
const safeConsumers = /^(sed|cat|sort|tail|tr|cut|awk|grep|uniq|wc|nl|rev|fold|head|column)\b/u;
for (const pipeline of pipelines) {
  const consumer = pipeline.slice(pipeAt(pipeline) + 2).replace(/>\s*\S+\s*$/u, "").trim();
  assert.match(
    consumer,
    safeConsumers,
    `unrecognised pipeline consumer \`${consumer}\` in the sweep job. This probe executes consumers to prove they read to EOF, so it refuses one it cannot classify as a read-only text filter. Add it to safeConsumers only after confirming it is side-effect free.`,
  );
  const probe = spawnSync("bash", ["--noprofile", "--norc", "-eo", "pipefail", "-c", `seq 1 2000000 | ${consumer} > /dev/null`]);
  assert.equal(
    probe.status,
    0,
    `pipefail hazard: \`${consumer}\` stops reading before EOF, so it SIGPIPEs its producer and fails the step with ${probe.status}. Use a consumer that reads to EOF, e.g. \`sed -n '1p'\`.`,
  );
}
// Both trigger legs, not just the cron. `push: branches: [main]` is the stronger of the two — it puts
// a red X on the main-branch commit itself, fires on every merge, and is immune to the 60-day
// auto-disable that applies to scheduled workflows. Deleting it leaves only the leg GitHub turns off.
assert.match(ci, /^ {2}schedule:\n {4}- cron: "[^"]+"$/mu, "the sweep needs a schedule to be a scheduled signal");
assert.match(
  ci,
  /^ {2}push:\n {4}branches: \[main\]$/mu,
  "the sweep needs the push-to-main leg: it is the stronger signal and the one the 60-day scheduled auto-disable cannot remove",
);

// AC4 again, from the other side: the per-PR integrity job must not have acquired sweep mode.
const prIntegrity = jobBody("integrity");
assert.match(prIntegrity, /^ {4}if: github\.event_name == 'pull_request'$/mu);
assert.doesNotMatch(prIntegrity, new RegExp(sweepEnvironmentVariable, "u"), "sweeping a pull request would undo #248's cost work");

console.log("Integrity planning preserves an unconditional floor, fails conservative for unknown, topology, workflow, and classifier changes, records explicit measured omissions, and carries an unconditional off-PR sweep so no subject can stay red on the default branch unobserved.");
