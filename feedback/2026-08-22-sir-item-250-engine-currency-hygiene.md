---
feedbackSchema: 2
date: 2026-08-22
workspace: S.I.R
cycle: item-250-engine-currency-hygiene
lane: none
toolVersion: n/a
commit: 355e8e8b477bf11ea030e7b260965f30ef011d4a
---

# S.I.R. item 250 — coordination engine currency hygiene

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 8
- **zero-event reason:** n/a

The four phase names are the fixed set the workspace feedback contract requires in this envelope and
that `validate-feedback-state.py --phases` is invoked with; they name where capture was active, not
where events qualified. Qualifying events came from three: `onboarding-first-build` (1),
`implementation-test-evidence` (1), `verify-ship-pr` (6). `lifecycle-authoring-or-not-used` is the
explicit not-used case, recorded in §5.

Cycle boundary: claim of `S.I.R.#250` by worker `dunlin-2b03` on 2026-08-22 through PR #257. Checkpoint
path `feedback/checkpoints/item-250-engine-currency-hygiene.jsonl`, count 8.

Pins at the described commit: `fs.gg.coord.cli` `0.72.0` (this cycle's change, from `0.71.0`),
`fs.gg.sdd.cli` `1.0.1`, `fs.gg.governance.cli` `1.12.1`,
`fs.gg.governance.fsharpsurfacecommand` `1.12.1`, `fable` `5.13.0`, `fsdocs-tool` `22.1.0`. Engine
version read from the running artifact rather than inferred: `scripts/fsgg-coord --version` → `0.72.0.0`.

SDD lane is `none`: the delivery-route receipt resolved `route: lightweight` with reason codes
`single-manifest-version-bump`, `no-product-behavior-change`, `verifiable-by-one-command`, so no
`fsgg-sdd` front-half was owed and none was authored.

Confidence limits, stated plainly:

- The product change is one version string. This cycle exercised the coordination engine's read surface
  broadly and its write surface narrowly, and carries **no** evidence about `0.72.0` behaviour outside
  `fsgg-coord` itself.
- The item is **not merged at this commit**. No merge, post-merge-obligation, or done-stamp outcome is
  asserted anywhere in this report.
- Wall-clock elapsed time was not instrumented. §8 reports counts, never durations.
- §4.5's central event is **not reproducible** and is recorded as an observation on that basis.
- §4.6's blocking conclusion rests on the `0.73.1` help text, its release notes, and its Core XML. The
  refusal itself was **not** executed, because `done` requires a merged PR and this one is not merged.
  That boundary is stated in the finding rather than papered over.
- **The checkpoint file cited as evidence by §4.4, §4.5 and §4.6 contains a claim this report refutes.**
  Events 4 and 6 name three packed skill mirrors including `.codex`; there are two (§4.3). Checkpoints
  are append-only, so those events cannot be edited: event 8 records the correction instead. A reader
  following a checkpoint locator will meet the refuted wording, and this note is the disclosure rather
  than a silent divergence between the report and its own evidence.

## §2 What worked

The `scripts/fsgg-coord` shim documents its four-tier engine resolution in the file itself, in priority
order, with the measurement history that fixed the order. That turned an otherwise-unfalsifiable claim —
"the bump is what changed the engine" — into a cheap controlled experiment (§4.1).

A stale premise reached a durable artifact before anything caught it. Commit `f7f2a49`'s message
states *"0.73.1 is published but deliberately not taken: its coherent-set release is still a Draft"* —
false, and now permanent in this branch's history; the correction was posted to issue #250 ten minutes
later. Re-reading the brief would not have produced it, because the brief is self-consistent and yields
the same wrong answer on a second read; re-deriving the fact from the release channel did. **No finding
is raised on this and no owner is assigned**: the only durable records are a commit and an issue comment
both authored by this worker, so which reviewer prompted the re-derivation is not established by
anything citable. `Verification: unverified` for that attribution. The propagation itself is
`command:git log -1 --format=%B f7f2a49`.

The engine's **fail-closed refusals were the most useful documentation in the cycle**. Every refusal
encountered while building the review-wait entry named the field and the expected value, including the
exact `reviewGeneration` string. That is what made an undocumented protocol recoverable (§4.4), and what
contained a cross-lane file collision without a bad write (§4.5).

`widen` returned `{"verdict":"disjoint","collisions":[]}` in the same object as the resulting
declaration, so each scope growth was decided and proved in one call.

`verify-paths --pr 257` returned `OK`, not `SKIP`, on first run and after every body edit.
Running it immediately after opening the PR is the cheap placement, because `.github#2107` makes the
`Refs`-instead-of-`Closes` form unrepairable after merge.

Sequencing this item behind #252/#254 rather than repairing a file in another lane's touch-set worked
exactly as the contract intends: #254 merged, the rebase cleared `integrity`, and no duplicate row or
competing fix was ever created.

## §3 What did not

Nothing in the product required rework. The implementation is one line and never changed.

Three things outside the diff cost real effort: a stale-and-misattributed version premise that took a
board round-trip and a host reversal to settle (§4.3), an undocumented review-wait protocol recovered
from refusals (§4.4), and a `0.73.1` adoption that passed every structural check and still had to be
refused (§4.6).

`integrity` was red for a pre-existing stale audit-binding ledger owned by #252. This item's diff touches
`feedback/`, which schedules the `feedback-audit` subject and unhides a red the path-conditional subject
otherwise hides — corroborating evidence transplanted onto #252 rather than filed as a new row. Cleared
by rebasing onto #254.

## §4 Findings

#### §4.1 Documented tier ordering turned an engine bump into a falsifiable measurement

- **Kind:** positive-pattern
- **Impact:** The worker could prove, not assert, that the manifest edit is what moved the engine, and
  that the mid-wave bump was invisible to concurrent lanes until merge. This is the confound
  `independent-review`'s gate-inversion step 6 exists to catch.
- **Expected:** A manifest bump is normally verified by reading the new version back, which is equally
  consistent with "the manifest selected it" and "something else on the box answered".
- **Observed:** Because the shim documents its resolution order, each of the three tiers that could
  preempt the manifest was checkable in one command and all three were absent: no
  `FSGG_COORD_ENGINE_BIN`, no `src/FS.GG.Coord.Cli` source build, no global tool on `PATH`. The manifest
  at `origin/main` reads `0.71.0`; the same tree in the same shell reported `0.71.0.0` before the edit
  and `0.72.0.0` after. Each worktree carries its own checked-out manifest, so blast radius was
  demonstrable rather than argued.
- **Evidence:** command:scripts/fsgg-coord --version; command:printenv FSGG_COORD_ENGINE_BIN; command:command -v fsgg-coord-engine; command:ls -d src/FS.GG.Coord.Cli; file:.config/dotnet-tools.json
- **Version:** `fs.gg.coord.cli` reproduced at `0.71.0`, `0.72.0` and `0.73.1`.
- **Locator caveat:** the `0.71.0.0` **before**-state is deliberately **not** cited as a locator. It is
  not re-derivable in-tree once the manifest has moved, and the only place it is written down is commit
  `f7f2a49`'s own message — this worker's own prose, which is an assertion rather than evidence. That
  same message additionally asserts the `0.73.1` Draft claim §4.3 refutes, so citing it would point an
  auditor at refuted text. The locators above establish the tier-absence controls and the current
  state; the before-state is carried by §2's narrative, marked as such.
- **Owner:** FS-GG/.github — `scripts/fsgg-coord` resolver and its in-file resolution-order commentary.
- **Recurrence:** new
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 The worktree-isolation guard refuses non-git commands and names git as the cause

- **Kind:** friction
- **Impact:** Implementing worker only. No incorrect work resulted — the guard fails closed rather than
  running a mis-scoped command.
- **Expected:** A diagnostic names the constraint that actually fired, so the reader's next action is
  the correct one.
- **Observed:** Commands performing no git operation are refused. The message names two causes: first
  that the command "is too complex to verify that it stays inside the worktree", then that "a
  worktree-isolated agent's git operations must target its own worktree". The first half is accurate;
  **the git half is the misleading one**, because the cited locator contains no git invocation, no path
  and no repository reference and is still refused verbatim. A reader who acts on the git clause
  inspects paths and git state, which is where the problem is not. The precise remedy — split compound
  shell constructs — is recoverable only from the first clause, and the second clause actively points
  away from it.
- **Evidence:** command:bash -c $'for f in a b c\ndo\necho "$f"\ndone'
- **Version:** n/a — agent harness behaviour, not a versioned FS-GG package.
- **Owner:** Agent harness worktree-isolation guard. Outside FS-GG repositories, so no FS-GG component
  can fix the root cause; recorded so the wrong-cause pattern stays legible.
- **Recurrence:** new for this guard. Same wrong-cause diagnostic class as `EHotwagner/S.I.R.#256`
  (pinned .NET SDK unreachable, failure names the wrong cause), a different subject. A full open-and-
  closed issue search for this guard returned nothing, so it is unfiled. It is new as a *finding*, not
  as a first sighting: `feedback/2026-08-22-sir-item-252-audit-binding-ledger-currency.md` already
  describes the same guard in prose without raising it.
- **Avoidable cost:** four refused commands re-issued in split form, as recorded in the cycle checkpoint.
- **Disposition:** accepted

#### §4.3 A version premise was stale AND misattributed — read from the wrong release

- **Kind:** orchestration
- **Impact:** The item would have shipped the second-newest promoted stable release while calling itself
  currency hygiene. It cost a board round-trip, a host re-scope, and a host reversal.
- **Expected:** A release-channel fact quoted as the merge-blocking reason for a version choice is
  current at claim time, and names the release it was read from.
- **Observed:** The disposition said `0.73.1` "is still a Draft, so `0.72.0` remains the promoted stable
  channel." `coherent-set/v0.73.1` is `isDraft: false`, `isPrerelease: false`, published
  `2026-08-22T20:07:33Z`, and its `stable-channel.json` reads `version: 0.73.1`, `promotedAt:
  2026-08-22T20:07:30Z` against `0.72.0`'s `11:17:06Z`. **The deeper error is misattribution, not
  staleness.** A Draft titled "(publishing)" does exist in that repo — it is `coherent-set/v0.70.0`. The
  observation read the right field on the wrong release. That changes the remedy: a premise that merely
  aged out is repaired by re-reading later, and re-reading `v0.70.0` later would still return Draft.
  Separately, **`0.73.0` was never published at all** — the delta from the pin is two releases, not
  three, so a gate instruction to read both `0.73.0` and `0.73.1` notes named a version that does not
  exist.
- **Evidence:** command:gh release list --repo FS-GG/.github --limit 12; command:gh release view coherent-set/v0.73.1 --repo FS-GG/.github --json name,isDraft,isPrerelease,publishedAt,tagName; command:gh release download coherent-set/v0.73.1 --repo FS-GG/.github --pattern stable-channel.json --output -; command:gh release download coherent-set/v0.72.0 --repo FS-GG/.github --pattern stable-channel.json --output -; issue:EHotwagner/S.I.R.#250; issue:EHotwagner/S.I.R.#255
- **Version:** `0.72.0` pinned by this cycle; `0.73.1` is the current promoted stable channel.
- **Owner:** `work-board` host, disposition step. The change surface is that a channel claim names the
  exact tag it was read from, and is re-read at claim time.
- **Recurrence:** new as a filed finding, but it **recurred inside this very cycle, with this worker as
  the offender.** The dispatch brief stated that the packed contract has three mirrors
  (`.claude/`, `.agents/`, `.codex/`). It has two: `.codex/skills/` holds ten `fs-gg-*` product skills
  and no `pnext-item`. `#255`'s body carried that correction, measured by `rook-e7e8`, from 21:42Z —
  about six minutes *after* this item was claimed at 21:36Z, and many hours before this report was
  written. So it was available the whole time the report was being authored. This report asserted
  "three mirrors" three times anyway, and an independent pass caught it. Same failure mode as the host's: a brief fact used as the basis for a handoff, never re-derived
  from the surface it describes.
- **Avoidable cost:** one board round-trip, one re-scope and one reversal; no rework to the diff. The
  in-cycle recurrence cost three wrong owner lines, caught before handoff.
- **Disposition:** accepted — the version decision is an acceptance-boundary change and was surfaced to
  the host rather than taken by this worker. The host amended the criteria twice and settled on `0.72.0`.

#### §4.4 The review-wait entry that gates critic dispatch appears in no packed skill

- **Kind:** capability-gap
- **Impact:** A worker following the packed contract literally cannot reach critic dispatch. The engine
  refuses with `verdict: noVerdict` and exit 4, documented as never retryable — a hard stop.
- **Expected:** The commands required to advance the review protocol are documented where the protocol is.
- **Observed:** `review <ref> --pr N` returns `noVerdict` and exit 4. At first contact the reason was
  `dispatchCritic requires a durable review-wait entry before dispatch`; the ledger has since advanced,
  so the same locator now reports `dispatchCritic requires a new durable review-wait entry for this
  generation` with `waitStatus: cancelled`. The refusal class is what reproduces, not that exact
  string. Neither `review wait` nor `review record` appears in any packed
  skill. Four field rules are non-obvious: `item` must be owner-qualified (`EHotwagner/S.I.R.#250`, not
  the board shorthand the same command's positional accepts); `claimGeneration` is the server-assigned
  claim-marker comment id, as a string; `reviewGeneration` must come from
  `ReviewWait.generationToken`; and the initial round is **0**, not 1. The round error fails *late* —
  the write is accepted and only the next `review` read reports it, requiring a cancel and re-enter.
- **Evidence:** command:scripts/fsgg-coord review S.I.R.#250 --pr 257 --json; issue:EHotwagner/S.I.R.#255; file:feedback/checkpoints/item-250-engine-currency-hygiene.jsonl
- **Version:** reproduced against `0.71.0`, `0.72.0` and `0.73.1`.
- **Owner:** `EHotwagner/S.I.R.` — packed `independent-review` contract in the **two** real mirrors,
  `.claude/skills/` and `.agents/skills/`. **Not `.codex/skills/`**: it holds ten `fs-gg-*` product
  skills and no `pnext-item`, so it is a different distribution surface, not a third contract mirror.
- **Recurrence:** seen again — `EHotwagner/S.I.R.#255` already carries this cause, and
  `feedback/2026-08-22-sir-item-252-audit-binding-ledger-currency.md` §4.4 already records the
  0-based-round half. **Not re-filed.** Three of the four field rules were contributed to #255 as new
  evidence; the `reviewGeneration` derivation was already in that issue's body.
- **Avoidable cost:** three refused attempts, plus one wrong-round entry cancelled and re-entered.
- **Disposition:** existing issue

#### §4.5 Concurrent lanes shared one scratchpad and overwrote each other's helper scripts

- **Kind:** friction
- **Impact:** Potentially severe, actually contained. Two lanes were reverse-engineering the same
  undocumented protocol (§4.4), independently chose the same filenames, and one lane's payload file was
  the same path the other passed to a board-writing command.
- **Expected:** Lanes isolated at the git-worktree and board-claim levels are also isolated in scratch
  space, or are given a naming convention that cannot collide.
- **Observed:** Three of this lane's scratchpad files were replaced in place by another lane's versions —
  pinned to `0.71.0` and targeting `EHotwagner/S.I.R.#252` and PR #254 — while still in use. That lane's
  helper writes to the same `wait-enter.json` filename this lane passed to `review wait`. **No incorrect
  write occurred**, and the reason is the finding: the engine validates `item`, `claimGeneration` and
  head before appending, and refused both mismatched probes. **Fail-closed validation contained this;
  filesystem isolation did not.**
- **Evidence:** command:scripts/fsgg-coord review S.I.R.#250 --pr 257 --json; file:feedback/checkpoints/item-250-engine-currency-hygiene.jsonl
- **Version:** n/a — orchestration environment.
- **Owner:** `work-board-best` host dispatch brief, which pointed every lane at one directory without a
  namespacing instruction. The host accepted this ownership explicitly and is now instructing lanes to
  namespace by worker id.
- **Recurrence:** **seen again, and this report initially got that wrong.**
  `feedback/2026-08-22-sir-item-252-audit-binding-ledger-currency.md` §3 already records the identical
  observation — five overwrites, the same `wait-enter.json` filename, the same owner — and its §11.6
  already proposes the same remedy this report's §11.4 restates. Recorded here as a second occurrence
  with an independent lane's evidence, not as a new cause.
- **Avoidable cost:** no rework; every helper script had to be re-verified before reuse.
- **Disposition:** existing observation — the prior report owns the cause; the host has accepted
  ownership and is instructing lanes to namespace by worker id.
- **Known unknown:** the overwrite events are not reproducible and the files are ephemeral and outside
  the workspace. The review locator establishes only that this lane's own payload was the one
  submitted; the checkpoint records the three filenames at the time they were observed. The specific
  `#252`/PR #254 attribution is now **non-reproducing**: by the time this report was audited, the same
  scratchpad path had been overwritten again by a *third* lane (`#249`/PR #262) — which is further
  evidence of the condition and simultaneously destroys the original observation's reproducibility.

#### §4.6 `0.73.1` redefines `done` in a way no structural compatibility check can see

- **Kind:** defect
- **Impact:** Adopting `0.73.1` would ship an engine this workspace's own packed contract cannot complete
  an item against — including this item's done stamp and those of every other live lane. This is the
  finding that reversed the host's re-scope.
- **Expected:** A compatibility check over commands, flags and output surfaces distinguishes a safe minor
  from an unsafe one.
- **Observed:** `done`'s help text changes from `stamp the item done` in **both** `0.71.0` and `0.72.0`
  to `replay a matching typed completion receipt; it cannot mint authority` in `0.73.1`, with
  `delivery --apply` becoming the sole emitter of completion authority. Meanwhile `done`'s flag set is
  **identical across all three** engines in `command-contract --json`, **zero** commands and **zero**
  flags are removed, and `review`, `delivery-route show` and `who` are byte-identical across `0.71.0`
  and `0.73.1`. Every *machine-readable* structural signal says compatible. One rendered signal does
  differ and is not sufficient: the help usage line shows `[--evidence E]` at `0.71.0`/`0.72.0` and
  `[--pr N]` at `0.73.1`, while the parsed flag set is identical in all three — so the usage line hints
  at a change it does not describe, and the contract that tooling would check says nothing at all. Corroborated in Core XML: `0.73.1` adds `Chore.ChoreKind.PrematureCompletion`,
  *"Receipt-free issue closure must be restored to a safe nonterminal projection"*, absent from `0.72.0`.
  The packed `pnext-item` §7 in this workspace ends with `done <ref> --flip --pr <pr>` and calls that
  completion.
- **Evidence:** command:scripts/compare-coord-engine-versions.sh --old 0.71.0 --new 0.73.1 --ref "S.I.R.#250" --pr 257 --repo "S.I.R."; command:grep -c PrematureCompletion "$HOME/.nuget/packages/fs.gg.coord.cli/0.72.0/tools/net10.0/any/FS.GG.Coord.Core.xml"; file:.claude/skills/pnext-item/SKILL.md; file:feedback/checkpoints/item-250-engine-currency-hygiene.jsonl
- **Version:** reproduced across `0.71.0`, `0.72.0` and `0.73.1`; `0.73.1` is the current promoted stable
  channel and is deliberately not adopted.
- **Owner:** `EHotwagner/S.I.R.` — packed `pnext-item` §7 terminal route in the two real skill mirrors,
  `.claude/skills/` and `.agents/skills/`, which is `#255`'s declared touch-set. The engine change is intended upstream behaviour, not a defect in
  `FS-GG/.github`.
- **Recurrence:** new — no prior report or issue covers it. The host stated it would file `0.73.1`
  adoption as a follow-up row with a `Blocked by` edge on `#255`; **that row did not yet exist when
  this report was written and is not asserted here as done.** Not filed by this worker, and this item's
  touch-set was not widened toward it.
- **Avoidable cost:** none to the diff — the finding is what prevented the cost.
- **Disposition:** existing issue — the packed-route repair belongs to `#255`.
- **Known unknown, and what each locator does not reach:** the refusal was **not executed** — `done`
  requires a merged PR and this one is not merged, so the conclusion rests on help text, release notes
  and Core XML rather than an observed refusal. The harness locator reproduces the byte-identical
  `review`/`delivery-route`/`who` comparison and the additive-only contract result; the `grep` locator
  reaches only the **absence** of `PrematureCompletion` at `0.72.0`, and its **presence** at `0.73.1`
  is carried by the checkpoint file rather than by a command that runs from the repo root, because the
  `0.73.1` engine is not installed here. The three-engine `done` help comparison is likewise recorded
  in the checkpoint (event 6) rather than re-runnable in place. Stated rather than smoothed over.

## §5 Did not exercise

Lifecycle authoring: **not used**, the recorded case for the `lifecycle-authoring-or-not-used` phase —
the delivery-route receipt resolved `route: lightweight` with `sddPackageReady: true`, so no `fsgg-sdd`
package was owed.

Also not exercised: the product build, simulation and client code, every test suite, and packaging. All
are outside a single-manifest version bump. The engine's write surface was exercised for `claim`,
`widen`, `heartbeat`, and `review wait` (enter and cancel); `flush`, `reap`, `release`, `done`,
`review record` and `delivery --apply` were not reached, because the item is not merged.

## §6 Doc-versus-behavior contradictions

1. **The item's original acceptance criterion 2** required `driver --json` to stop returning
   `RepairEngineCurrency` and named the pinned version as the cause. `swift-b572` measured both versions
   with negative controls and established that a receipt-less `driver` fails closed by design; that
   measurement is narrated in issue #250 and was **not re-derived by this cycle**, so it is carried here
   as context rather than as this report's own evidence. The host removed the criterion rather than the
   measurement.
2. **The disposition's `0.73.1`-is-a-Draft claim** versus the live release channel — §4.3. Corrected in
   the PR body and on the item.
3. **`0.73.1`'s release notes say "Packed skills prescribe the new terminal route"** while the skills
   packed in this workspace prescribe the old one — §4.6. Owned by `#255`.

## §7 Workarounds still in the tree

None. The manifest change is one version string and introduces no shim, pin override, or suppression.
`scripts/compare-coord-engine-versions.sh` is a deliverable, not a workaround: it measures, changes no
product behaviour, and never edits the manifest it tests.

## §8 Friction and avoidable cost

Counted from this session's transcript, as counts and not durations: four commands refused by the
worktree guard and re-issued in split form (§4.2, matching the checkpoint); three refused `review wait` attempts plus one
wrong-round entry cancelled and re-entered (§4.4); every scratchpad helper re-verified before reuse
(§4.5); one board round-trip plus a host re-scope and reversal on the version premise (§4.3). Four PR
body edits, as recorded by GitHub's `userContentEdits`, each followed by re-running `verify-paths` to
confirm the closing keyword still parsed.
One rebase onto `#254`. No manual YAML or code edits beyond the single intended line, no lifecycle
reruns, no generated files replaced, no worker restarts, no reverts.

One measurement was **taken wrongly and corrected**: a `who` diff read as an engine regression was two
other lanes widening between captures. The instrument was fixed, not just the conclusion (§11.3).

## §9 Skill value and gaps

Inventory from the packed skill manifest under `.claude/skills/`, mirrored in `.agents/skills/`.

Invoked: `pnext-item` — the binding worker state machine, followed from mint through PR, including its
`independent-review`, `findings-and-filing`, `merge-and-release` and `control-plane-provenance`
references. `fs-gg-feedback-report` — this report and its seven checkpoints, including its independent
actionability-critic step. The critic's own transcript is not a durable workspace artifact, so no claim
is made here about what it specifically caught; its per-finding dispositions are carried by the audit
bound to this report.

Relevant packed skills not invoked, with reason: the `fs-gg-sdd-*` stage skills — route `lightweight`,
no SDD package owed; `cross-repo-coordination` — no cross-repo request arose, and the cross-repo *facts*
needed (release channel, nuspec notes) were reads, not requests; `check-board` and `work-board` —
host-level, not a worker's.

Naming correction: the performance-first planning gate is **not** a packed skill despite reading like
one in worker briefs. It is `references/performance-first.md` inside `pnext-item`, and it did not apply.

Gap: §4.4, already `EHotwagner/S.I.R.#255`, not re-filed.

## §10 Outcome markers

Time to first build: n/a — no product build in scope.

First green verification: `dotnet tool restore` reporting `Restore was successful.` with
`fs.gg.coord.cli 0.72.0`, then `scripts/fsgg-coord --version` → `0.72.0.0`, immediately after the
one-line edit.

Acceptance criterion 3 re-verified fresh at this commit, each exit 0, **as captured at the moment of
that run**: `who` (five live claims listed, including this one), `reconcile --json` (`[]`),
`lint --json` (`[]`), `ready` (19 rows), `batch --explain`
(`wave occupancy: {"activeItems":5,"waveCapacity":6,"openSlots":1}`). These five read the **live**
board and therefore drift: a re-run minutes later returned 21 ready rows, one `reconcile` and one
`lint` row for a newly filed #271, and `activeItems 6` as a sixth lane went live. Every divergence
traces to a specific new or changed row, so the criterion is "each command succeeds against a fresh
read", not "these counts are stable". Extended to the
review-ledger surface at the same head: `delivery-route show --json` (digest `a6cc3741…`, unchanged
across every engine and every head this cycle) and `review --json`.

PR #257 opened; `verify-paths` `OK` on first run and after each body rewrite; `integrity` cleared by
rebasing onto `#254`, verified locally by
`./scripts/test-feedback-audit-binding-exceptions.sh` reporting its full mutant sweep passed.

Not reached at this commit, and not asserted: merge, post-merge obligations, done stamp.

## §11 Falsifiable improvements

1. **Name the exact tag a release-channel claim was read from, and re-read it at claim time.** Prevents
   §4.3, including the misattribution half that re-reading alone would not have caught. Owner:
   `work-board` host disposition step. Acceptance: an item whose criteria name a package version carries
   a channel read that names its release tag and is timestamped at or after the claim.
2. **Document `review wait` and `review record` in the packed contract**, including the 0-based initial
   round, the owner-qualified `item`, the string `claimGeneration`, and that a wrong round fails at the
   next read rather than at the write. Prevents §4.4. Owner: `EHotwagner/S.I.R.`, the two real mirrors
   (`.claude/skills/`, `.agents/skills/`) — `#255`. Acceptance: a worker reaches `dispatchCritic` from the packed contract alone, without
   reflecting on the engine assembly or reading a refusal.
3. **Compare engine versions by measurement, and require the comparison to prove it can fail.** Owner:
   `EHotwagner/S.I.R.`, `scripts/compare-coord-engine-versions.sh` — delivered by this cycle.
   Acceptance, widened in repair round 1 because the original set graded only two of the script's four
   decision points: the harness reds on a contract mutant with a genuinely removed command, refuses a
   malformed contract instead of grading it compatible, aborts when the shim resolved an engine other
   than the one requested, **refuses rather than grades a surface that did not evaluate** (`--ref` or
   `--repo` naming something that does not exist), and **still reds on a real engine difference** rather
   than swallowing it into that refusal. Every row is recorded in the script's own header block, which
   is where the evidence belongs — travelling with the artifact rather than only in a commit message.

   The lesson generalises past this script: **the gate that goes uninverted is the one its author is
   most confident about.** The two inversions originally recorded covered the checks added defensively;
   the check the header called *"the decisive one"* was the one nobody broke.
4. **Give each lane its own scratchpad, or qualify scratch filenames by worker id.** Prevents §4.5.
   **This restates `feedback/2026-08-22-sir-item-252-audit-binding-ledger-currency.md` §11.6 rather
   than proposing something new**, and is repeated here only because the condition recurred in a second
   lane after that report was written. Owner: `work-board` host dispatch. Acceptance: two concurrent
   lanes cannot write the same scratch path, demonstrated by dispatching two lanes that use the same
   helper filename and observing both files survive.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | No scaffold or template step in a single-manifest version bump. |
| onboarding-guidance | partial | The dispatch brief's three environment facts (SDK not on `PATH`, trailing-dot repo name, non-persisting `whoami --mint`) were all load-bearing and correct; its `0.73.1` fact was stale and misattributed (§4.3), its shared-scratchpad instruction collided lanes (§4.5), and the harness guard (§4.2) was covered by no guidance. |
| skills | exercised | `pnext-item` and `fs-gg-feedback-report` invoked and followed; §9 records the packed skills not invoked and why, and corrects one mis-naming. |
| sdd-authoring | not-exercised | Route `lightweight`, `sddPackageReady: true`; no SDD package owed. The recorded not-used case for the lifecycle-authoring phase. |
| implementation-apis | not-exercised | No product API touched. |
| dependencies-build | exercised | `dotnet tool restore` restored all six pinned tools including `fs.gg.coord.cli 0.72.0`; feed availability confirmed against `nuget.org`, the sole source after `NuGet.Config`'s `<clear />`. |
| testing | exercised | The diff adds one gate — the harness. It shipped with three subject-mutation inversions, and an independent critic found that all three exercised only its *contract* check and its version guard, while `compare_surface` — the decision point its own header calls decisive — carried no recorded inversion at all. That gate was then measured to grade two mutual refusals as agreement and to announce a durable write it never performed. Repair round 1 fixed both and the file now records the full set: positive control PASS at exit 0; a genuinely different engine pair (0.58.0 vs 0.72.0) DIFFERS at exit 1; `--ref S.I.R.#999999` and `--repo NoSuchRepoAtAll` each REFUSED at exit 1; the contract mutant REMOVED naming both cuts at exit 1; an unreadable contract REFUSED at exit 1; a mislabelled engine aborting at exit 2; and the removed `--write-probe` at exit 2. The manifest change itself adds no gate. |
| evidence | exercised | Each acceptance criterion carries a reproduction locator; the three tier-absence checks in §4.1 are the negative controls that make criterion 2 a measurement rather than a reading; §4.6 records the one conclusion whose refusal could not be executed and why. |
| runtime-playtest | not-exercised | No reachable game functionality in scope. |
| performance | not-exercised | No performance claim made or required; the performance-first gate does not apply to a manifest bump (§9). |
| documentation | partial | The shim's in-file resolution-order commentary proved accurate and load-bearing (§4.1); the packed review contract proved incomplete (§4.4) and its terminal route proved incompatible with the newest engine (§4.6). The harness documents its own limits in its output; no other documentation was authored. |
| packaging-upgrade | exercised | A pinned package was upgraded; feed availability, channel promotion, and the nuspec `releaseNotes` for `0.72.0` and `0.73.1` were read from the immutable coherent-set assets; `0.73.1` adoption was refused on measured grounds (§4.6) and `0.73.0` was found never to have been published. |
| worker-git-pr | exercised | Mint, claim (converged, no collisions), three widens (all `disjoint`), heartbeats, branch, four commits, rebase onto `#254`, PR #257, `verify-paths` `OK` throughout, review-wait enter and cancel twice; §4.2, §4.4 and §4.5 record friction on this surface. |
