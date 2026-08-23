import assert from "node:assert/strict";
import { chmodSync, existsSync, mkdirSync, mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { spawnSync } from "node:child_process";
import { tmpdir } from "node:os";
import { dirname, join } from "node:path";
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
//
// The guard's full TEXT is captured too, not only its id — the assertions further down execute it,
// and a scan that kept only the id could not have seen what round 1's F1 was about.
const guards = [];
{
  const lines = integrityBody.split("\n");
  for (let index = 0; index < lines.length; index += 1) {
    const opener = /^(?<indent> +)if integrity_runs (?<id>[a-z0-9-]+); then(?<tail>.*)$/u.exec(lines[index]);
    if (!opener) continue;
    const { indent, id, tail } = opener.groups;
    if (/\bfi\s*$/u.test(tail)) { guards.push({ id, block: lines[index] }); continue; }
    const close = lines.findIndex((line, at) => at > index && line === `${indent}fi`);
    assert.notEqual(close, -1, `the guard for ${id} has no closing \`fi\` at its own indent; refusing rather than probing a truncated block`);
    guards.push({ id, block: lines.slice(index, close + 1).join("\n") });
  }
}
const dispatched = guards.map(({ id }) => id);
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
// S.I.R.#265 round 1, F1 — a guard that RUNS is not a guard whose VERDICT COUNTS.
//
// Everything above pins that a call site exists and carries the right id. It pins nothing about
// what happens to that call's exit status, and those are different properties. Measured through
// the production route by smew-2162: changing one dispatch to `… coherence.sh || true` — one line
// added, one removed — left this suite green, left every other gate green, and made
// `run-ci-gate.sh integrity` exit 0 with a `pass` receipt, while its own 482-line log still
// contained `FAILED  doc:scalar:wait-window-max-hours`. The gate ran. It caught the falsification.
// It printed it. The run was green. `then :; fi` and pointing the guard at another subject's
// script survived identically.
//
// That is this row's own defect one layer in. #265 exists because a coherence gate was wired so
// that nothing ran it, making it indistinguishable from a gate that passes; a guard whose verdict
// is discardable is indistinguishable in precisely the same way, and costs 106s to be so.
//
// SO THE PROPERTY IS ASSERTED BEHAVIOURALLY, NOT TEXTUALLY. A blocklist was considered and
// rejected: `|| true` is one spelling of a class that also holds `|| :`, `; true`, `&& true`,
// `set +e`, a trailing `&`, and a subshell, and pinning the spellings someone has thought of is
// the shape of check this whole row exists to delete. Each committed guard is instead EXECUTED in
// a sandbox where every command it invokes fails, and required to fail. That measures the property
// itself, so the spellings nobody has thought of are covered too.
// ---------------------------------------------------------------------------

// Runs one guard block with `integrity_runs` forced true and EVERY command it names replaced by a
// stub that records its own argv and exits 1. Returns the guard's status and what it invoked.
//
// `mkdtempSync` rather than a fixed path, deliberately: a probe that writes stubs to a shared
// filename is a differential another process can silently overwrite, which is the failure this
// very item is about. Each call gets a directory no other process names.
const probeGuard = (block, failing = "") => {
  const root = mkdtempSync(join(tmpdir(), "sir-integrity-guard-probe-"));
  const log = join(root, "invocations.log");
  const shim = join(root, ".shim");
  mkdirSync(shim, { recursive: true });
  // The stub records its own argv and then decides its status from `SIR_PROBE_FAIL`: empty means
  // EVERY named command fails, a token means only the command whose `$0 $*` contains that token
  // fails and the rest succeed. The second mode is what makes each command in a MULTI-command
  // guard individually observable — see the loop below for why that is not optional.
  const stub = (path) => {
    mkdirSync(dirname(path), { recursive: true });
    writeFileSync(path, `#!/usr/bin/env bash\nprintf '%s %s\\n' "$0" "$*" >> ${JSON.stringify(log)}\n`
      + `if [[ -z "\${SIR_PROBE_FAIL:-}" || "$0 $*" == *"\${SIR_PROBE_FAIL}"* ]]; then exit 1; fi\nexit 0\n`);
    chmodSync(path, 0o755);
  };
  // Every in-tree script the guard names, created at the path the guard names it by.
  for (const [, relative] of block.matchAll(/(?:^|[\s(])(?:\.\/)?((?:\.github\/)?scripts\/[A-Za-z0-9._-]+)/gu)) stub(join(root, relative));
  // Every PATH command it starts a line with (`node …`, `dotnet …`). Shell KEYWORDS and BUILTINS
  // are excluded — and `true`/`:` would be harmless anyway, since bash resolves a builtin before
  // PATH and never consults a stub named for one. That is load-bearing: a stubbed `true` that
  // exited 1 would make `|| true` fail and the mutant this whole block exists to catch would pass.
  const shellWords = new Set(["if", "fi", "then", "else", "elif", "do", "done", "true", "false", "set", "echo", "printf", "return", "exit", "local"]);
  for (const [, name] of block.matchAll(/^\s*([a-z][a-z0-9-]*)\s/gmu)) if (!shellWords.has(name)) stub(join(shim, name));
  // THE PROBE OWNS ITS ENVIRONMENT, and this is measured rather than tidy. `--noprofile --norc`
  // does NOT stop a non-interactive bash from sourcing `$BASH_ENV`, and in this workspace
  // `BASH_ENV=scripts/agent-env.sh`, which PREPENDS `$HOME/.dotnet` to PATH. Passing the shim in
  // `env` alone therefore lost the race: the real `dotnet` ran, no stub was recorded, and the
  // guard exited non-zero for a reason that had nothing to do with the guard. Assertion 2 below
  // refused that as proof, which is the only reason it was noticed. So the file is removed from
  // the child AND the shim is prepended INSIDE the script, after anything BASH_ENV would have done.
  const env = { ...process.env, PATH: `${shim}:${process.env.PATH}`, SIR_PROBE_FAIL: failing };
  delete env.BASH_ENV;
  delete env.ENV;
  const probe = spawnSync("bash", ["--noprofile", "--norc", "-c",
    `export PATH=${JSON.stringify(shim)}:"$PATH"\nset -euo pipefail\nintegrity_runs() { return 0; }\n${block}\n`], {
    cwd: root,
    env,
  });
  const invoked = existsSync(log) ? readFileSync(log, "utf8") : "";
  rmSync(root, { recursive: true, force: true });
  return { status: probe.status, invoked };
};

// Self-test FIRST. A probe that cannot tell a neutered guard from a live one would report every
// committed guard sound, which is the same silence being repaired. The `|| true` row is round 1's
// finding stated as an executable expectation: it is the exact mutant that must red.
//
// EVERY ROW BELOW IS A MEASUREMENT, NOT AN EXPECTATION. Two spellings that read like escapes are
// not escapes here, and asserting them would have been asserting something false: under
// `set -euo pipefail` a failing command aborts the block at once, so `cmd; true` never reaches its
// `true` and `set +e; cmd` still ends on `cmd`'s own status. They are kept as rows precisely
// because they mark where the class stops — a later reader tempted to "also block `; true`" can
// see that it was run rather than reasoned about. `cmd &` needs its own line: `then cmd & fi` is a
// bash syntax error (status 2), which would have been a probe erroring rather than detecting.
const probeSubjectCommand = "./scripts/probe-subject-thing.sh";
for (const [label, body, mustFail] of [
  ["a failing command fails the guard", probeSubjectCommand, true],
  ["`|| true` is detected", `${probeSubjectCommand} || true`, false],
  ["`|| :` is detected", `${probeSubjectCommand} || :`, false],
  ["`|| <cmd>` is detected", `${probeSubjectCommand} || echo skipped`, false],
  ["an empty body is detected", ":", false],
  ["a swallowing `if` wrapper is detected", `if ${probeSubjectCommand}; then :; fi`, false],
  ["a subshell does not hide the status", `( ${probeSubjectCommand} )`, true],
  ["`; true` does NOT neuter under set -e — measured, and the block still fails", `${probeSubjectCommand}; true`, true],
  ["`set +e` before the last command does NOT neuter it — measured", `set +e; ${probeSubjectCommand}`, true],
]) {
  const { status } = probeGuard(`    if integrity_runs probe-subject; then ${body}; fi`);
  assert.equal(
    Number.isInteger(status) && status !== 0,
    mustFail,
    `guard-probe self-test: ${label} — the probe does not behave as measured, so the assertions below would not mean what they say`,
  );
}
// Backgrounding, which genuinely does discard the status, on its own line because the one-liner
// form does not parse.
assert.equal(
  probeGuard(`    if integrity_runs probe-subject; then\n      ${probeSubjectCommand} &\n    fi`).status,
  0,
  "guard-probe self-test: a backgrounded subject discards its status and must be detectable",
);

// SELF-TEST FOR THE SELECTIVE MODE (round 2, F2). The per-command assertions below are only worth
// anything if failing exactly one command of a MULTI-command guard is distinguishable from failing
// another. Round 1's probe could not do this — it stubbed everything to fail, so the first command
// aborted the block and the second was unobservable in status and in existence alike. These rows
// prove the distinction exists BEFORE it is relied on, and they prove it in BOTH positions, since
// a mechanism that only sees the first command is exactly the defect being repaired.
{
  const first = "./scripts/probe-first.sh";
  const second = "./scripts/probe-second.sh";
  const twoCommand = (firstTail, secondTail) =>
    `    if integrity_runs probe-subject; then\n      ${first}${firstTail}\n      ${second}${secondTail}\n    fi`;
  const healthy = twoCommand("", "");
  assert.notEqual(probeGuard(healthy, "probe-first.sh").status, 0, "selective self-test: a failing FIRST command must fail a healthy guard");
  assert.notEqual(probeGuard(healthy, "probe-second.sh").status, 0, "selective self-test: a failing SECOND command must fail a healthy guard");
  assert.ok(probeGuard(healthy, "probe-second.sh").invoked.includes("probe-first.sh"),
    "selective self-test: the earlier command must still run, or the later one was never reached");

  const secondSwallowed = twoCommand("", " || true");
  assert.notEqual(probeGuard(secondSwallowed, "probe-first.sh").status, 0, "selective self-test: swallowing the SECOND must not mask the FIRST");
  assert.equal(probeGuard(secondSwallowed, "probe-second.sh").status, 0,
    "selective self-test: a swallowed SECOND command must be detectable — this is the case round 1 could not see at all");

  const firstSwallowed = twoCommand(" || true", "");
  assert.equal(probeGuard(firstSwallowed, "probe-first.sh").status, 0, "selective self-test: a swallowed FIRST command must be detectable");
  assert.notEqual(probeGuard(firstSwallowed, "probe-second.sh").status, 0, "selective self-test: swallowing the FIRST must not mask the SECOND");
}

// ---------------------------------------------------------------------------
// S.I.R.#265 round 2, F2 — the round-1 probe could only observe a guard's FIRST command.
//
// Every command was stubbed to exit 1 and the block ran under `set -euo pipefail`, so the first
// stub aborted it and everything after was unobservable in status AND in existence. `npm-audit` is
// the only multi-command guard and its two commands are not interchangeable:
// `check-npm-audit.mjs` is the policy runner and is clean in CI, so it proves nothing on its own;
// `test-npm-audit.mjs` is the SOLE provider of the proof that the runner can tell a high-severity
// advisory from a clean report. Measured through the production route, arms differing by one line
// of `qualify-pr.sh` with an identical break in the checker: control exit 1 / receipt `fail`;
// delete `node .github/scripts/test-npm-audit.mjs` (0 added, 1 removed) → exit 0, receipt `pass`,
// suite green. At suite level `|| true` on EITHER command alone also escaped.
//
// This is the same shape as the two layers before it: the repair claimed "the spellings nobody has
// thought of are covered too" while the evidence covered single-command guards only. The claim
// outran the measurement. Two things are needed, and neither substitutes for the other.
// ---------------------------------------------------------------------------

// (I) THE COMMAND INVENTORY IS DECLARED, NOT DERIVED — and that is the whole answer to DELETION.
//
// No probe that reads its expectations out of the block can see a command removed FROM that block:
// the mutant simply shrinks the expected set and the probe agrees with itself. That is this repo's
// own `declaredSubjects` lesson one level down, so the remedy is the same one — an absolute pin
// that an author must update deliberately, stated here where a reviewer reads it rather than
// recovered from the shell being checked.
//
// Each entry is a distinctive substring of the command's `$0 $*` as the sandbox observes it.
const declaredDispatch = {
  "npm-audit": [".github/scripts/test-npm-audit.mjs", ".github/scripts/check-npm-audit.mjs"],
  governance: ["scripts/verify-fable-game-governance.sh"],
  "dependency-surface": ["fsgg-sdd dependency-surface"],
  "sdd-byte-stability": ["scripts/test-item-184-sdd-byte-stability.sh"],
  "feedback-audit": ["scripts/test-feedback-audit-binding-exceptions.sh"],
  "review-contract": ["scripts/test-review-contract-coherence.sh"],
};
assert.deepEqual(
  Object.keys(declaredDispatch).sort(),
  [...subjectOrder].sort(),
  "every dispatched subject needs a declared command inventory, and only dispatched subjects may have one",
);

// The statements a guard actually contains. Split on `;` and newline, which is exactly the shape
// this case block is written in; a command carrying a literal `;` inside quotes would be
// miscounted, and the count assertion below would then fire and demand a deliberate look rather
// than silently miscounting.
const guardStatements = (block) => block
  .replace(/^\s*if integrity_runs [a-z0-9-]+; then/u, "")
  .replace(/\bfi\s*$/u, "")
  .split(/[;\n]/u)
  .map((statement) => statement.trim())
  .filter((statement) => statement.length > 0 && !statement.startsWith("#"));

for (const { id, block } of guards) {
  const declared = declaredDispatch[id];
  const statements = guardStatements(block);

  // (I.a) One statement per declared command, and no others. Deletion shortens `statements`;
  //       an added command lengthens it. Both are refused here rather than absorbed.
  assert.equal(
    statements.length,
    declared.length,
    `the \`${id}\` guard runs ${statements.length} command(s) but ${declared.length} are declared for it.\n`
      + `  declared: ${declared.join(", ")}\n  found:    ${statements.join(" | ")}\n`
      + "  A command DELETED from a guard cannot be seen by probing the guard — the probe would just expect less.\n"
      + "  If this change is intended, update `declaredDispatch` deliberately and say why in the commit.",
  );
  // (I.b) …and they are the declared ones, matched one-to-one in both directions.
  for (const command of declared) {
    assert.equal(
      statements.filter((statement) => statement.includes(command)).length,
      1,
      `the \`${id}\` guard does not run exactly one statement containing \`${command}\`:\n  ${statements.join("\n  ")}`,
    );
  }
  for (const statement of statements) {
    assert.equal(
      declared.filter((command) => statement.includes(command)).length,
      1,
      `the \`${id}\` guard runs \`${statement}\`, which matches no single declared command for it`,
    );
  }

  // (II) EACH DECLARED COMMAND IS INDIVIDUALLY LOAD-BEARING. Only this command fails; every other
  //      succeeds, so the block reaches it and its status is the only thing that can fail the
  //      guard. Round 1 asserted this for the first command and, by aborting there, for no other.
  for (const command of declared) {
    const { status, invoked } = probeGuard(block, command);
    assert.ok(
      Number.isInteger(status) && status !== 0,
      `qualify-pr.sh discards the exit status of \`${command}\` in the \`${id}\` guard:`
        + ` with only that command failing and every other succeeding, the guard still succeeded (status ${status}).`
        + " That command's failure cannot fail the integrity gate, so a real defect it detects would be printed"
        + " into a green run's log. Remove whatever swallows the status (`|| true`, `|| :`, a trailing `&`, a subshell).",
    );
    assert.ok(
      invoked.includes(command),
      `the \`${id}\` guard never reached \`${command}\`, so its non-zero status is unexplained:\n${invoked.trim() || "(nothing invoked)"}`,
    );
  }

  // (III) The whole-guard properties round 1 established, unchanged: with EVERYTHING failing the
  //       guard fails, it invoked something, and what it invoked names its own subject.
  const { status, invoked } = probeGuard(block);
  assert.ok(
    Number.isInteger(status) && status !== 0,
    `qualify-pr.sh discards the \`${id}\` subject's exit status: with every command it invokes failing, the guard still succeeded (status ${status}).`
      + " A subject whose failure does not fail the integrity gate is indistinguishable from one that passed —"
      + " the run goes green with the failure printed in its own log.",
  );
  assert.ok(
    invoked.trim().length > 0,
    `the \`${id}\` guard invoked none of the stubs, so its non-zero status is unexplained; refusing rather than counting it as proof`,
  );
  // What it invoked names the subject it is guarding — what catches a guard pointed at ANOTHER
  // subject's script, which the assertions above cannot see: that mutant fails and invokes
  // something, it just runs the wrong thing.
  assert.ok(
    invoked.includes(id),
    `the \`${id}\` guard runs a command that does not name \`${id}\`:\n${invoked.trim()}`,
  );
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
