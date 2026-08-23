---
feedbackSchema: 2
date: 2026-08-23
workspace: S.I.R
cycle: item-272-evidence-gate-absence
lane: none
toolVersion: n/a
commit: 23fba42a79b91942af8bab1abb8d0b95d1c124dd
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 4
- **zero-event reason:** n/a

Cycle boundary: board item `EHotwagner/S.I.R.#272`, claimed by worker `swift-f7bd` at claim marker
comment `5383572016`, delivered on branch `item/272-evidence-gate-absence` from base
`f69f1e6cdc203121bd908a3a1bd025545e0aff56`. Described commit
`23fba42a79b91942af8bab1abb8d0b95d1c124dd`.

Checkpoint path `feedback/checkpoints/item-272-evidence-gate-absence.jsonl`, 4 events, validated with
`validate-checkpoint-state`.

Actionability critique: `fresh-context-subagent`, prompt version `actionability-v1`, two passes. The
critic's own row set — dispositions, per-locator results, digests, and corrections, in its words — is
its artifact, not a summary of it; the audit beside this report is bound to it. Its first pass, against
the superseded commit `ba9867df6ee50e91b475e31eced8d4625c29ffa4`, returned §4.2 `incomplete` with
`command:./scripts/test-ci-evidence-mutation.sh` recorded `non-reproducing` for that finding, because
the harness at that commit exercised only two of the five coverage outcomes. That gap was closed in
the deliverable rather than in the prose, and the second pass re-resolved every locator at the
described commit before upgrading it.

Lifecycle: `none`. The item's typed delivery-route receipt is revision 1, `route: lightweight`,
`kind: current`, `sddPackageReady: true`, digest
`d908175564d27cf89817db62fc6d5f496aa2ac04fe5a173c47b2b2f5f957d4bc`, so no `fsgg-sdd` front half was
owed and none was authored. That receipt lives as a `fsgg:route-decision/v2` comment on issue #272 and
is readable with `scripts/fsgg-coord delivery-route show "S.I.R.#272" --json`; it is not a file in the
tree, so a full-tree grep for the digest returns nothing. `toolVersion` is `n/a` for that reason.
Coordination engine `fs.gg.coord.cli` 0.71.0. `fsgg-sdd` was invoked only as the gate's subject, never
as an authoring stage; the repository pins 1.0.1 in `.config/dotnet-tools.json` and that is the version
that ran, while 1.1.0 is installed globally on the measured machine and was not exercised.

Declared touch-set: `scripts/qualify-pr.sh`, widened once (verdict `disjoint`) to
`scripts/test-ci-evidence-mutation.sh` and this cycle's three feedback paths.

Confidence limits. Every measurement below was taken on one machine (Linux, bash 5.3.15) against a
detached worktree of `f69f1e6`. The gate mutations in §4.1 and §4.2 run under a stubbed
`dotnet`/`npm` `PATH`, because what they pin is the gate's own classification and coverage record,
not `fsgg-sdd`'s verdict; the unstubbed `fsgg-sdd verify` path is exercised by the first mutation
already in `scripts/test-ci-evidence-mutation.sh`, which was run to completion at the described
commit. No claim below rests on CI-observed behavior: at the time of writing this branch had not yet
completed a CI run.

Three measurements in §3 and §10 are author measurements that are not preserved as artifacts and are
not reproducible from this report alone: the three `fsgg-sdd verify` probe results in §3, and the two
comparative harness runs in §10 (unmodified `origin/main`, and the three single-variable mutants).
The mutant runs required editing a throwaway worktree, so no committed locator can carry them. The
property each was checking is now pinned by a committed assertion in
`scripts/test-ci-evidence-mutation.sh`, which is the reproducible evidence; the elapsed and
red/green observations themselves remain uncited.

## §2 What worked

The repository already contained the correct fail-closed shape for this defect, three lines below the
defect itself. In the same `for work_id` loop, a routed `hosted-verification.sh` that is missing, not
a regular file, unreadable, or non-executable fails the gate with a named diagnostic. That made the
repair a matter of extending an established in-repo convention to the adjacent subject rather than
inventing a policy, and it supplied the diagnostic's wording and voice.

`scripts/test-ci-evidence-mutation.sh` was already a clean-checkout mutation harness driven
unconditionally by `qualify-pr.sh integrity`. Adding mutations to it cost no new wiring and inherited
its detached-worktree isolation, its stubbed-`PATH` fixture, and its cleanup trap. It ends this cycle
with seven new mutations; §3 records why it did not end with three.

`scripts/ci-route.mjs route` classifies a lone `work/<id>/evidence.yml` deletion as `evidence-only`
and selects the `evidence` gate. That made the defect's worst case exactly reproducible: the deletion
is the change that selects the gate the deletion then silences.

Writing the mutations before the fix, as `pnext-item` §3 requires, is what exposed the defect's real
signature. The first formulation would have asserted only that the repaired gate reports absence; the
`cmp -s` comparison between a checked run's log and an unchecked run's log is what turned "these
outcomes are indistinguishable" from a description into a test, and it is the assertion that would
catch a future regression to silence whatever vocabulary the gate then uses.

## §3 What did not

The first design considered was to drop the existence guard and let `fsgg-sdd verify` decide every
case. Measurement rejected it: `verify` blocks whenever `evidence.yml` is absent, both for a package
whose tasks are `done` (`doneTaskMissingEvidence`) and for one whose tasks were reverted to `pending`
(`Evidence prerequisite … is missing`), and it also blocks for a work id with no package at all
(`Analysis prerequisite … is missing`). Deferring wholesale to `verify` would therefore have failed
every evidence-free item outright, which the item explicitly rules out. The measurement cost three
probe runs and produced the discriminator the repair actually uses.

The first pass of the regression harness covered only two of the five outcomes the repair introduces.
`failed`, `work-package-removed`, and `not-reached` were exercised by hand against a scratch checkout
and never committed, so the tree asserted a vocabulary it did not test — a smaller instance of exactly
the shape this item repairs. An independent actionability critique of this report's draft caught it
before handoff, and the harness was extended from three mutations to seven. The rework cost roughly
thirty minutes and is the most useful thing that happened in this cycle.

The repaired gate's coverage artifact cannot be collected by CI from inside this item's touch-set;
see §4.3.

## §4 Findings

#### §4.1 The evidence gate's coverage was conditional on the presence of the file it checks, and its two outcomes were byte-identical

- **Kind:** defect
- **Impact:** Any author able to delete `work/<id>/evidence.yml` converted a mandatory SDD gate into a
  no-op, and nothing in the CI result or its artifacts recorded that the item had been skipped rather
  than passed. Severity high: the gate that certifies evidence could be disabled by removing evidence.
- **Expected:** A gate that checks an item's evidence reports whether it checked it.
- **Observed:** `scripts/qualify-pr.sh` guarded the per-item verify with
  `[[ -f "work/$work_id/evidence.yml" ]] || continue`. On a detached worktree of `f69f1e6`, routing
  the single changed path `work/220-bounded-pr-ci/evidence.yml` classified `evidence-only` and
  selected the `evidence` gate; with that declaration deleted the gate exited `0` having written
  **0 bytes**, and the log was **byte-identical** (`cmp -s`) to the same gate run with the declaration
  present. "Checked and passed" and "not checked" were not merely indistinguishable in principle —
  they were the same bytes and the same exit status. The zero-byte and `cmp`-identical observations
  were taken by hand at the base commit and are not preserved as artifacts; the property they measured
  is now pinned by mutation 4 of the cited harness, which runs `cmp -s` over a checked run's log and an
  unchecked run's log and fails if they match. The pre-repair guard is quoted from
  `f69f1e6:scripts/qualify-pr.sh:444`, not from the described commit, where the same file carries the
  repair.
- **Evidence:** file:scripts/qualify-pr.sh; command:./scripts/test-ci-evidence-mutation.sh
- **Version:** `fsgg-sdd` 1.0.1; repository base `f69f1e6cdc203121bd908a3a1bd025545e0aff56`
- **Owner:** S.I.R. / `scripts/qualify-pr.sh`, evidence gate
- **Recurrence:** new for this trigger; same class as `EHotwagner/S.I.R.#252` (path-conditional
  integrity subject) and `EHotwagner/S.I.R.#268` (verdict written as a literal). Filed as
  `EHotwagner/S.I.R.#272`.
- **Avoidable cost:** none in this cycle; the repair is this cycle's deliverable.
- **Disposition:** product fix

#### §4.2 The same loop carried a second silent skip that the filed item does not name

- **Kind:** defect
- **Impact:** When one routed work item failed, a bare `break` abandoned every later routed work item.
  Those items were never verified and never mentioned in the gate's output, so a multi-item PR could
  report a single failure while an unknown number of siblings went unchecked. Severity moderate: it
  hides coverage rather than a verdict, and the run is already red.
- **Expected:** A gate that stops early says which subjects it did not reach.
- **Observed:** With two routed work items and the first one's `evidence.yml` absent, the pre-repair
  loop `break`s after the first (`f69f1e6:scripts/qualify-pr.sh:447`) and produces no record of the
  second. After the repair the second is recorded as `not-reached` in both the gate's output and
  `artifacts/ci/results/evidence-coverage.json`. Mutation 5 of the cited harness routes exactly that
  two-item case and asserts both; restoring the bare `break` reds it with
  `a work item abandoned behind a failure was not recorded as not-reached`.
- **Evidence:** file:scripts/qualify-pr.sh; command:./scripts/test-ci-evidence-mutation.sh
- **Version:** repository base `f69f1e6cdc203121bd908a3a1bd025545e0aff56`
- **Owner:** S.I.R. / `scripts/qualify-pr.sh`, evidence gate
- **Recurrence:** new; found while auditing every `|| continue`-on-missing-input subject in the file,
  which `EHotwagner/S.I.R.#272` required.
- **Avoidable cost:** none; repaired inside the declared touch-set in the same change.
- **Disposition:** product fix

#### §4.3 CI collects gate results by an exact-path allow-list, so a new gate result is produced on the runner and discarded

- **Kind:** capability-gap
- **Impact:** The repair's acceptance criterion requires "not checked" to be distinguishable from
  "checked and passed" **in the gate's artifact**. The gate now writes
  `artifacts/ci/results/evidence-coverage.json`, but the workflow's `gate-integrity` upload names its
  three result files literally, so on CI that record is written and thrown away. The distinction is
  fully available locally and in the job log, and absent from the downloadable artifact.
- **Expected:** A gate result written to the conventional results directory is collected, or its
  omission is detected.
- **Observed:** `.github/workflows/ci.yml` enumerates `artifacts/ci/results/integrity.json`,
  `artifacts/ci/results/integrity-plan.json`, and `artifacts/ci/results/evidence.json`. Nothing
  compares the producers' outputs against that list. This item could not repair it: the workflow file
  was inside a concurrently held touch-set (`EHotwagner/S.I.R.#280`), so widening into it would have
  collided.
- **Evidence:** file:.github/workflows/ci.yml; issue:EHotwagner/S.I.R.#282
- **Version:** repository base `f69f1e6cdc203121bd908a3a1bd025545e0aff56`
- **Owner:** S.I.R. / `.github/workflows/ci.yml`, gate result collection
- **Recurrence:** new
- **Avoidable cost:** one filed follow-up and one incomplete acceptance criterion instead of a
  one-line edit in this PR.
- **Disposition:** issue

#### §4.4 A rejected coordination-engine argument prints the entire command reference without naming the accepted spelling

- **Kind:** friction
- **Impact:** Every wrong guess at a subcommand or flag costs a full engine help dump and a second
  read to find the right spelling. Small per occurrence, paid by every worker at claim time.
- **Expected:** A parse refusal names the accepted spelling, or at least the accepted flags of the
  command it rejected.
- **Observed:** `scripts/fsgg-coord show S.I.R.#272 --json` answers `unknown command: show` and then
  prints the complete DECISION/IO command reference. `widen … --path <p>` answers
  `unknown argument: --path` and prints the same reference; the accepted spelling is `--paths T...`,
  visible only by searching that dump.
- **Evidence:** command:scripts/fsgg-coord show S.I.R.#272; command:scripts/fsgg-coord widen S.I.R.#272 --path scripts/qualify-pr.sh
- **Version:** `fs.gg.coord.cli` 0.71.0
- **Owner:** FS-GG coordination engine (`fs.gg.coord.cli`) / argument parser diagnostics
- **Recurrence:** new for these two commands; same root cause as
  `feedback/2026-08-22-sir-item-256-agent-toolchain-wiring.md` §11, which asks that
  `fsgg-coord landable`'s refusal name its producing command. Engine refusals not naming the accepted
  next step is a recurring class; this is a second instance, not a duplicate.
- **Avoidable cost:** two full help dumps (274 lines each) before the first successful `widen`.
- **Disposition:** accepted

## §5 Did not exercise

No `fsgg-sdd` authoring stage was exercised: the delivery route is `lightweight` and no SDD package
was owed. No runtime, rendering, or playtest surface was touched — the change is a CI shell gate.
Packaging and upgrade were not exercised. The repaired gate has not yet been observed running on CI
hardware at the time of writing; every measurement is local.

## §6 Doc-versus-behavior contradictions

None observed.

## §7 Workarounds still in the tree

The seven mutations added to `scripts/test-ci-evidence-mutation.sh` run under a stubbed
`dotnet`/`npm` `PATH`, following the fixture the file already used for its hosted-verification
mutations. That is deliberate scoping, not a workaround: they assert the gate's classification and
its coverage record, while the file's first mutation exercises the unstubbed `fsgg-sdd verify` path.

Mutation 7 adds a **second stub layer** and should be read as a deliberate exception rather than
drift. It writes a `dotnet` shim that exits 9 when its second argument is `verify`, and prepends that
shim ahead of the pass-through stub (`PATH="$verify_fail_bin:$fake_bin:$PATH"`), so the gate observes
a failing verify without any real lifecycle state being wrong. That is the only way to reach the
`failed` outcome from a tree where every work package is genuinely healthy, and reaching it is the
point: it is what pins `checked: true` as a statement about coverage rather than a synonym for
success. Two stub layers is more fixture than any other mutation in the file needs, so it is the
thing to look at first if this harness ever starts passing for the wrong reason.

The removal condition for the whole stubbed block is unchanged: if a coverage assertion ever needs a
real lifecycle verdict rather than a controlled exit code, it belongs with the file's unstubbed first
mutation, not here.

## §8 Friction and avoidable cost

Two full engine help dumps at claim time (§4.4). Three probe runs of `fsgg-sdd verify` to establish
that it blocks on an absent `evidence.yml` in every package state, which rejected the first design
(§3) — necessary measurement, not waste. No worker restarts, no lifecycle reruns, no generated files
replaced, no reverts. One follow-up issue filed because a needed one-line edit lay in another live
claim's touch-set (§4.3).

## §9 Skill value and gaps

`pnext-item` was invoked and followed; its §3 requirement that a modified gate ship with evidence it
can fail is what produced the seven mutations and the three single-variable mutant runs, and doing it
before the fix rather than after is what made the defect's byte-identical signature visible at all.
`intra-repo-parallel-work` supplied the mint/claim/widen sequence; the `widen` overlap check is what
prevented a collision with `EHotwagner/S.I.R.#280` over `.github/workflows/ci.yml` and turned it into
§4.3 instead of a conflict. `fs-gg-feedback-report` was invoked for this report.
`fs-gg-sdd-evidence` and the other lifecycle skills were correctly not invoked: the route is
`lightweight`. No skill was missing, and no skill guidance misled this cycle.

## §10 Outcome markers

Claim to converged claim receipt: one command. Claim to confirmed red reproduction of the defect:
approximately 40 minutes, dominated by reading the gate and measuring `fsgg-sdd verify`'s behavior in
three package states. Red reproduction to green repair: approximately 25 minutes. A further
approximately 30 minutes extended the harness from three mutations to seven after an independent
actionability critique established that three of the five outcomes were exercised by hand and never
committed; that rework is recorded here because the first pass would have shipped an asserted
vocabulary rather than a tested one.

Full `scripts/test-ci-evidence-mutation.sh` at the described commit: green, exit 0. Same harness
against unmodified `origin/main`: red. Same harness against three single-variable mutants of the
described commit: red, each naming its own defect. Ship readiness and merge had not occurred when this
report was written.

All elapsed figures are estimates read from the working session, not instrumented measurements, and
the comparative harness runs in this section are author measurements without committed locators; see
the confidence limits in §1.

## §11 Falsifiable improvements

1. Collect `artifacts/ci/results/` as a directory, or add a check that compares each gate producer's
   declared outputs against the workflow's collected set. Prevents §4.3. Owner: S.I.R.
   `.github/workflows/ci.yml`. Acceptance: removing one existing result file from the collection
   wiring makes that check go red.
2. Have the coordination engine's parse refusals print the rejected token together with the accepted
   spellings for the command actually named, instead of the whole reference. Prevents §4.4. Owner:
   `fs.gg.coord.cli` argument parser. Acceptance: `fsgg-coord widen <ref> --path x` names `--paths`
   in its refusal and prints no more than the `widen` usage line.
3. Treat "exercised" and "asserted by a committed test" as different claims in the feedback §12
   matrix, and require the latter wherever a finding's evidence is a `command:` locator. Prevents the
   §3 rework: three outcomes were exercised by hand and would have shipped as covered. Owner:
   `fs-gg-feedback-report` skill, §12 guidance. Acceptance: a §12 row whose evidence is an
   uncommitted ad-hoc run cannot be marked `exercised` without naming that limitation.
4. Audit the remaining `|| continue`, `|| return 0`, and `if [[ -f … ]]` tolerances in
   `scripts/qualify-pr.sh` for the same shape whenever a new one is introduced, since two of the four
   existing ones were the same defect. Prevents recurrence of §4.1 and §4.2. Owner: S.I.R.
   `scripts/qualify-pr.sh`. Acceptance: a new missing-input tolerance in that file cannot land without
   a recorded outcome for the skipped subject.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | No scaffold or template provider was involved; the item is a repair to an existing CI gate. |
| onboarding-guidance | partial | The dispatch brief's three environment facts (pinned SDK under `$HOME/.dotnet`, the trailing dot in `S.I.R.`, non-persisting `whoami --mint`) were all load-bearing and all correct; nothing else was needed to reach a first command. |
| skills | exercised | `pnext-item`, `intra-repo-parallel-work`, `fs-gg-feedback-report` invoked; see §9. |
| sdd-authoring | not-exercised | Delivery route is `lightweight`, `sddPackageReady: true`; no stage was owed and none was authored. |
| implementation-apis | not-exercised | The change is bash and node-in-heredoc inside one CI script; no product API was touched. |
| dependencies-build | partial | `dotnet restore` and `npm run verify:scaffold` ran only as the evidence gate's own steps during the unstubbed harness block; no dependency or lock changed. |
| testing | exercised | Seven mutations added to `scripts/test-ci-evidence-mutation.sh`; green at the described commit, red against `origin/main`, and red against each of three single-variable mutants (restore the `\|\| continue` guard, restore the bare `break`, make a bare absence fatal), each naming its own defect. The mutant runs are author measurements, not committed artifacts. |
| evidence | exercised | The evidence gate itself is the subject. All five outcomes (`verified`, `failed`, `not-evidenced` fatal and non-fatal, `work-package-removed`, `not-reached`) are asserted by committed mutations in `scripts/test-ci-evidence-mutation.sh`, each checking the gate's output and its `evidence-coverage.json` record. The zero-routed-item case was exercised by hand only and is not committed. |
| runtime-playtest | not-exercised | No runtime or rendering surface is reachable from this change. |
| performance | not-exercised | No interactive or simulation path is touched; the item's route carries no performance gate. |
| documentation | partial | No document changed; the audit of the file's other missing-input tolerances is recorded as comments in `scripts/qualify-pr.sh` and in the PR body rather than in a document. |
| packaging-upgrade | not-exercised | No package, pin, or release surface was touched. |
| worker-git-pr | exercised | Mint, claim, `widen` with a `disjoint` verdict, isolated worktree, branch, and follow-up filing all ran; the `widen` overlap check is what surfaced §4.3. |
