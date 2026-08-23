---
feedbackSchema: 2
date: 2026-08-23
workspace: S.I.R
cycle: item-277-agent-env-dotnet-root
lane: none
toolVersion: n/a
commit: 4ec2925921965d33851986eee66b853610e70463
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** implementation-test-evidence, verify-ship-pr
- **material events:** 22
- **zero-event reason:** n/a

Cycle boundary: board item `EHotwagner/S.I.R.#277`, claimed by worker `crake-610f`, branch
`item/277-agent-env-dotnet-root`, described commit
`4ec2925921965d33851986eee66b853610e70463`, **base `origin/main` at
`f69f1e6cdc203121bd908a3a1bd025545e0aff56`**. The base/head distinction is load-bearing for the
figures below: the pre-repair suite has **36** checks at `f69f1e6` and the repaired suite has **49**
at the described commit, so any "36 checks, 0 unexpected" figure in this report is a statement about
the base commit and reproduces only there.

Checkpoint path `feedback/checkpoints/item-277-agent-env-dotnet-root.jsonl`, 22 events, validated
with `validate-checkpoint-state`. Three are `implementation-test-evidence` and 19 are
`verify-ship-pr`. **The ledger is frozen and no further checkpoint is
appended after this report is written** (22 events as of `be39619`, unchanged by the two commits
that follow it, neither of which touches `feedback/checkpoints/`) — a count in a document about a file that document keeps
appending to is a figure that stales itself, which is exactly how this cycle produced its fourth
staleness instance.

Lifecycle: `none`. The item's typed delivery-route receipt is `route: lightweight`, `kind: current`,
`revision: 1`, digest `b2f62e9a9314130cbaf739a01a4e5a6d71d2617d3b89d47f955a96ef51b16de9`
(**`claim-only`**: `scripts/fsgg-coord delivery EHotwagner/S.I.R.#277` refuses for any non-holder —
*"live claim belongs to worker `crake-610f`"* — so this is a FAILED READ, not an empty answer, and
that refusal independently corroborates the worker id and live claim asserted here and in §12. The
digest is kept rather than dropped: `route`, `kind`, `revision` and `sddPackageReady` are equally
unverifiable by a non-holder, so removing only the most checkable of the five would improve
nothing),
`sddPackageReady: true`, so no `fsgg-sdd` front half was owed and none was authored. `toolVersion`
is `n/a` for the same reason. Coordination engine `fs.gg.coord.cli` 0.71.0, resolved at
`scripts/fsgg-coord` **tier 4** — this repository is a coord *receiver*, a fact that turns out to
matter for §4.4 and §4.5.

Toolchain under measurement: `global.json` pins SDK `10.0.302` with `rollForward: disable`. The
installed roots on the measured machine are `/usr/share/dotnet` (SDKs `6.0.428`, `10.0.400`;
`Microsoft.NETCore.App` `6.0.36`, `9.0.19`, `10.0.11`) and `$HOME/.dotnet` (SDK `10.0.302`;
`Microsoft.NETCore.App` `10.0.10`).

Onboarding, lifecycle authoring, and scaffolding were not phases of this cycle: it is a two-file
hardening change on an existing harness in an existing checkout. §12 records them as
`not-exercised` rather than borrowing a prior cycle's result.

Audit boundary, stated because it is a real limit on this report's own evidence. The independent
actionability critic verified `scripts/test-agent-env.sh` at revision `f591a22f…`. Acting on that
critic's own contradiction — the retraction now in §4.7(a) — the file was then corrected, so the
bytes it verified are not the bytes this report names: at `4ec2925` the digest is `929cdb2b…`. The
suite was re-run green at the new bytes and the inversion inventory re-derived there, but **that
re-verification is the author's, not the critic's**, and the audit records it as such rather than
extending a critic's `verified` across a change the critic never saw.

Confidence limits. Every host-behaviour claim here was measured on one Linux machine with the two
roots listed above, and was not re-verified on another operating system. The divergence the new
check detects requires the machine to carry two installs with different `Microsoft.NETCore.App`
versions, which is why a non-vacuity control ships beside it rather than being assumed; the
single-install path is therefore reasoned about but **not exercised**. §4.5 records a claim in this
cycle's own repair that was wrong and was caught only at review, which is the sharpest confidence
limit here: the new check asserts the *mechanism*, and nothing in this cycle checks the *inventory
of callers* that exercise it.

## §2 What worked

The committed assertion suite made the finding reproducible by a worker who did not measure it.
`S.I.R.#277` carried a table produced by critic `crake-2427` during PR #260 review round 1. Because
`scripts/test-agent-env.sh` is committed rather than living in a scratchpad, mutation S8 was
re-derived at the base commit in one command instead of being taken on trust — the row said 36
checks and 0 unexpected, and 36/0 is what re-running it at `f69f1e6` produced. That is the property
the suite's own header argues for, exercised for the first time by a second worker.

Distinct exit codes inside a check separate a setup failure from a real red. Section I's probe uses
`3`, `4`, and `5` for build failure, an unresolvable muxer, and probe failure, leaving `1` to mean
only the assertion. When the subject was inverted, the observed failure was `rc=1`, which is direct
evidence that the assertion fired rather than the fixture breaking.

The independent actionability critic paid for itself on the artifact, not just the report. It
rejected an example in the shipped comment that no reviewer of the diff alone would likely have
questioned, because refuting it required knowing that this repository is a coord *receiver*. See
§4.5.

## §3 What did not

The dispatch brief's most promising suggested thread was a dead end, for a structural reason it
could not have known — see §4.4.

The repair's own replacement text then made a smaller version of the same mistake, twice: it
asserted an apphost hot path that does not exist in this repository, and then, correcting that,
asserted a second one that no committed script actually exercises — see §4.5. Both are recorded as
findings rather than smoothed over, because together they are evidence about how far the new
check's protection actually reaches: it proves the mechanism and polices none of the prose.

## §4 Findings

#### §4.1 A 36-check suite could not falsify its subject because it exercised only one consumer class

- **Kind:** quality-gap
- **Impact:** `scripts/agent-env.sh` is sourced by every non-interactive bash in an agent session in
  this workspace, so it is the artifact here with the widest blast radius. One line of it —
  `export DOTNET_ROOT="$candidate"` — carried a stated purpose that no check could contradict, and
  the gap survived a full independent review round before becoming its own board row.
- **Expected:** A committed assertion suite for a shim whose entire job is environment repair should
  be able to falsify each repair the shim performs.
- **Observed:** At base commit `f69f1e6`, deleting `export DOTNET_ROOT="$candidate"` left the suite
  at 36 checks and 0 unexpected outcomes. The root cause is not thin coverage: every probe in the
  suite reaches the SDK through the **muxer** (`dotnet …`), and the muxer resolves SDKs relative to
  its own location and does not consult `DOTNET_ROOT`. The only consumer class that reads
  `DOTNET_ROOT` is the framework-dependent **apphost**, and the suite contained none. A suite can be
  thorough along one axis and structurally blind along another.
- **Evidence:** file:scripts/agent-env.sh; file:scripts/test-agent-env.sh; command:git worktree add --detach ../sir-277-base f69f1e6cdc203121bd908a3a1bd025545e0aff56 && sed -i -e '\|export DOTNET_ROOT="$candidate"|d' ../sir-277-base/scripts/agent-env.sh && ( cd ../sir-277-base && ./scripts/test-agent-env.sh ) && git worktree remove --force ../sir-277-base; issue:EHotwagner/S.I.R.#277
- **Version:** `fs.gg.coord.cli` 0.71.0; .NET SDK 10.0.302 pinned by `global.json`. The 36/0 result
  is a property of **base commit `f69f1e6`** and does not reproduce at the described commit, where
  the same mutation yields 49 checks and 1 unexpected — that difference is the repair. The cited
  command **never touches the working tree**: it checks the base commit out into a throwaway
  worktree, mutates and runs the suite *inside* it, and removes it. An earlier revision restored
  in-tree via `… || git checkout …`, which does not restore on the success path — and the success
  path is the entire finding — and `git checkout <commit> -- <paths>` *stages* what it writes, so it
  left a partially staged tree for the critic who ran it. Running the suite from inside the
  throwaway worktree is also required rather than stylistic: see §4.9.
- **Owner:** S.I.R. / `scripts/test-agent-env.sh`
- **Recurrence:** first seen EHotwagner/S.I.R.#277. Related prior context in
  `feedback/2026-08-22-sir-item-256-agent-toolchain-wiring.md`, which records the 36-check baseline
  and the fact that the muxer resolves SDKs relative to its own location — but not the apphost
  consumer class, and not that the export was therefore unfalsifiable. Not a duplicate.
- **Avoidable cost:** none in this cycle. Historically: one critic mutation round in PR #260 that
  could not be converted into a repair, plus one board row to carry the surviving point.
- **Disposition:** product fix — `scripts/test-agent-env.sh` section I, in this cycle.

#### §4.2 The export is load-bearing, and its effect is observable — the suite simply never looked

- **Kind:** positive-pattern
- **Impact:** Determined the item's disposition. The issue offered two legitimate outcomes — remove
  the export as dead, or evidence it. Measurement selected the second, so a correct line was not
  deleted on the strength of a green suite that could not see it.
- **Expected:** Per the shim's own comment, `DOTNET_ROOT` is exported "so the muxer that PATH now
  resolves and the root that apphosts consult agree."
- **Observed:** True, and observable. A framework-dependent apphost reads `DOTNET_ROOT` to locate
  `hostfxr`, falling back to the global install location only when it names no directory. On the
  reference workspace a session arrives with `DOTNET_ROOT=/usr/share/dotnet`, whose
  `Microsoft.NETCore.App` differs from the `$HOME/.dotnet` that step 2 selects, so without the
  export the muxer and every apphost load from two different installs. Step 2 puts
  `$HOME/.dotnet/tools` on PATH, so a bare `fable` or `fsgg-sdd` in an agent session is such an
  apphost; no committed script in this repository invokes one that way, which is the missing half of
  why the old suite could not observe the export at all (see §4.5). Measured with `COREHOST_TRACE=1`
  against one of those shims:
  `Chose FX version [/usr/share/dotnet/shared/Microsoft.NETCore.App/10.0.11]` under the arriving
  root, against `Chose FX version [$HOME/.dotnet/shared/Microsoft.NETCore.App/10.0.10]`
  under the exported one. The delta is visible only on a machine carrying two installs with
  differing `Microsoft.NETCore.App`, as this one does.
- **Evidence:** file:scripts/agent-env.sh; command:SYS=<the system root named in §1> ; for R in "$SYS" "$HOME/.dotnet"; do env -i HOME="$HOME" PATH="$PATH" COREHOST_TRACE=1 DOTNET_ROOT="$R" "$HOME/.dotnet/tools/fsgg-sdd" --version 2>&1 | grep 'Chose FX version'; done
- **Version:** .NET SDK 10.0.302; hosts 10.0.11 and 10.0.10 as installed on the measured machine.
- **Owner:** S.I.R. / `scripts/agent-env.sh`
- **Recurrence:** new
- **Avoidable cost:** none
- **Disposition:** accepted — recorded as the reason the export was retained rather than removed.

#### §4.3 An environment variable is testable only through the consumer class that reads it

- **Kind:** positive-pattern
- **Impact:** Turned an untestable variable into a falsifiable assertion, and is reusable by any
  cycle that has to prove an environment repair rather than assert it.
- **Expected:** n/a — this records a technique, not a defect.
- **Observed:** Building a real apphost with the pinned SDK **inside the wired session under test**
  makes the invisible visible: the location of `System.Private.CoreLib` in that process is the
  install the host resolved the runtime from, so the process reports the root it consulted. Building
  it inside the session also doubles as the direct `dotnet` call that retires the shim's `dotnet`
  function and re-heals `PATH`, so `command -v dotnet` afterwards answers a real path. A second
  check requires the probe to actually follow `DOTNET_ROOT` on the machine at hand, so the first
  cannot pass vacuously on a single-install host. Result at the described commit: 49 checks and 0
  unexpected; with the export deleted, exactly one `WRONG` at `rc=1` — the section I assertion — and
  no other check moves, including the non-vacuity control.
- **Evidence:** file:scripts/test-agent-env.sh; command:./scripts/test-agent-env.sh
- **Version:** .NET SDK 10.0.302.
- **Owner:** S.I.R. / `scripts/test-agent-env.sh`
- **Recurrence:** new
- **Avoidable cost:** none. At the described commit the suite measures 11-13s wall across three runs
  on this machine; the figure moves with machine load and with every check added, so it is stated as
  a range bound to that commit rather than as a property. The base suite was not timed, so no delta
  is claimed.
- **Disposition:** accepted — technique recorded for reuse.

#### §4.4 A resolver's tier decides whether a symptom can discriminate, and the tier is a property of the checkout

- **Kind:** orchestration
- **Impact:** A dispatching host's most specific and most plausible diagnostic hint pointed at a path
  that is structurally incapable of showing the effect. Following it without checking which tier
  answers would have produced a check that passes in both directions.
- **Expected:** `scripts/fsgg-coord --version` answering under a mismatched root was expected to be
  the thread on which an observable difference could be found.
- **Observed:** In this repository `scripts/fsgg-coord` resolves at **tier 4** — `dotnet tool run
  fsgg-coord-engine` against `.config/dotnet-tools.json` — which routes through the muxer and
  therefore cannot depend on `DOTNET_ROOT`. Tier 2, the source build, is not merely unbuilt here but
  **structurally unreachable**: `src/FS.GG.Coord.Cli` exists nowhere in this repository (`src/`
  carries only `SIR.*` projects), and `scripts/fsgg-coord` states that only the repo owning coord's
  source can resolve there while "a receiver resolves at tier 1/3/4". Confirmed by elimination on
  this machine: `FSGG_COORD_ENGINE_BIN` unset (tier 1 miss), no `fsgg-coord-engine` on `PATH` (tier
  3 miss), manifest declares `fs.gg.coord.cli` 0.71.0 → command `fsgg-coord-engine` (tier 4 hit).
- **Evidence:** file:scripts/fsgg-coord; file:.config/dotnet-tools.json; command:git ls-tree --name-only HEAD src/
- **Version:** `fs.gg.coord.cli` 0.71.0 declared in `.config/dotnet-tools.json`.
- **Owner:** S.I.R. / `scripts/fsgg-coord` resolution-order documentation
- **Recurrence:** new
- **Avoidable cost:** one measurement round redirected; no rework, no retries, no lifecycle reruns.
- **Disposition:** accepted — the shim's tier comments already state the order; this records that a
  reader diagnosing behaviour must first establish which tier answers in the checkout at hand.

#### §4.5 The repair for an unevidenced claim shipped two unevidenced claims of its own

- **Kind:** defect
- **Impact:** Directly undercuts this cycle's own thesis, so it is recorded rather than quietly
  fixed. The comment that replaced the unfalsifiable sentence made a claim about *which callers in
  this repository are apphosts*, and got it wrong twice in a row, in the file whose entire subject
  is claims nobody checked.
- **Expected:** A repair whose acceptance criterion is "the shim contains no purpose claim that its
  suite cannot demonstrate" should not introduce caller claims the suite cannot demonstrate either.
- **Observed:** Two successive errors in the same sentence.
  **(a)** It named "the built `fsgg-coord-engine` that `scripts/fsgg-coord` execs at its tier 2" as a
  hot path. False here: `src/FS.GG.Coord.Cli` exists nowhere in this repository and only the repo
  owning coord's source resolves at tier 2 (§4.4). Found by the independent actionability critic,
  which resolved the path against the tree.
  **(b)** The correction for (a) then named the `$HOME/.dotnet/tools` shims as "this workspace's hot
  path". Also unevidenced: **no committed script in this repository invokes a tool as a bare
  command.** Every call goes through the muxer — `dotnet fable`, `dotnet fsgg-sdd`,
  `dotnet tool run` — confirmed by scanning every occurrence of each global tool name across the
  shell and `.mjs` sources. **The characterisation of the residue in an earlier revision was wrong
  and is corrected:** that grep returns 89 lines, and the non-`dotnet` remainder is not only XML
  testcase labels and a route-name array — it also includes shell `case`/part labels, a verb
  comparison in `dotnet-invocation-trace.sh`, tool-id strings in receipt builders, a loop variable,
  `--outDir` path names, JS fixtures and an error message. The load-bearing claim survives every one
  of them: none is a bare tool invocation. Found by self-audit after the critic had found (a).
  The final text states the evidenced position: step 2 puts that directory on PATH, so a bare
  `fable`/`fsgg-sdd` in an agent session *is* an apphost and the export governs its runtime, but the
  protected path is ad-hoc and interactive rather than scripted — which is the missing half of why
  deleting the export broke nothing the old suite could observe, and why section I must **build** an
  apphost rather than reuse a caller.
- **Observed bound on the new check:** it did not and could not catch either error. It asserts the
  *mechanism* — an apphost's runtime root follows `DOTNET_ROOT` and must agree with the muxer's —
  and reds correctly when the export is deleted. It says nothing about which callers are apphosts,
  so a wrong caller inventory in the surrounding prose is outside its reach. A gate that proves a
  mechanism does not thereby police the prose around it.
- **Evidence:** file:scripts/agent-env.sh; file:scripts/test-agent-env.sh; command:git ls-tree --name-only HEAD src/; command:git grep -InE "(^|[^-a-zA-Z0-9_./])(fable|fsdocs|fantomas|paket|fake|fsgg-sdd)([^-a-zA-Z0-9_.]|$)" -- '*.sh' 'build.sh' '*.mjs'
- **Version:** fixed at commit `2e4b07ee2c53b83ecbb1b91ba15da2c1ae078907` (a *fix* commit, not the
  frontmatter's described commit); error (a) existed in
  `9c53f83`/`f13fcc4` and was fixed in `af6ae10`, error (b) existed in `af6ae10` and was fixed in
  `2e4b07e`. Neither left the branch.
- **Owner:** S.I.R. / `scripts/agent-env.sh` and `scripts/test-agent-env.sh` comments
- **Recurrence:** new. Same family as §4.1 — an unfalsifiable claim — at one and then two further
  removes.
- **Avoidable cost:** two artifact corrections and three full green/inversion/restore loops. Both
  caught before the PR was opened, so no review round or head move was spent on either.
- **Disposition:** product fix — corrected in this cycle, in `af6ae10` and `2e4b07e`.

#### §4.6 `check-invalidation` is red on a clean default branch, and its live home is three closure-hops away

- **Kind:** friction
- **Impact:** A worker following the feedback contract runs this command before their commit lands
  and gets errors that have nothing to do with their branch — 17 against a clean default branch, 15
  on this one (§4.10); the count is `entries − entries this diff touches`, not a constant. Read
  literally it blocks the commit;
  read correctly it is noise. Worse for dedupe: the row named nearest the symptom is closed, the row
  carrying the root cause is *also* closed, and only the third hop is open — so a worker who stops
  at either of the first two files a duplicate.
- **Expected:** `check-invalidation --base origin/main --head HEAD` reports citations that *this
  branch* invalidated, and exits 0 on a clean default branch.
- **Observed:** 17 errors of the form `invalidation: overbroad or mismatched exception
  feedback/audits/<audit>.json §4.n file:<path>`, naming files this branch does not touch. Running
  it with `--base origin/main --head origin/main` produces the identical 17, so the default branch
  is red on its own — measured after an explicit `git fetch origin`, against `origin/main` at
  `f62d6e2`. Root cause, read from the tool rather than inferred:
  `FeedbackReportTool.fs` requires each entry in `scripts/audit-binding-exceptions.json` to match
  **exactly one** binding invalidated by the *current* change set, and errors otherwise. The ledger
  is append-only and permanent while `invalidated` covers only what this diff touches, so
  `errors = entries − entries invalidated by this diff`, and an empty diff therefore yields the
  maximum. Confirmed directly: with all 13 ledgered paths supplied via `--changed`, the command
  stops reporting overbroad entries and instead reports genuinely invalidated bindings (computed
  against the **merge-base `f69f1e6`**, which is what "this commit changes" means: **4** bindings.
  Against `git diff origin/main HEAD` the same command reports 6, because that diff also carries the
  reverse of commits on `main` this branch does not have. The base is load-bearing and is named
  wherever the figure appears.).
  `scripts/test-feedback-audit-binding-exceptions.sh` stays green (rc=0) because it never uses this
  form — it passes a synthetic `--changed` inventory against a fixture root.
- **Evidence:** command:dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- check-invalidation --base origin/main --head origin/main; command:./scripts/test-feedback-audit-binding-exceptions.sh; issue:FS-GG/.github#2856
- **Version:** the clean-branch measurement (17) was taken against `origin/main` **at `f62d6e2`**.
  That ref is not a fixed point and has moved repeatedly during this cycle; **no successor SHA is
  named here, because naming one is how §4.15 happens.** Each number in this finding is bound to the
  ref SHA it was measured against, and the formula — `entries − entries this diff touches` — is what
  makes the finding survive any of those moves. The
  vendored tool carries no independent version string.
- **Owner:** FS-GG `fs-gg-feedback-report` skill, `FeedbackReportTool.fs` invalidation checker —
  **not S.I.R.** It is a distributed skill artifact, so repairing the vendored copy here re-diverges
  on the next kit sync.
- **Recurrence:** seen again, and the closure chain is the point. `EHotwagner/S.I.R.#252` is
  **CLOSED** and covers a different half of the symptom (stale digests and gate selection);
  `#254` is a **MERGED pull request**, not an issue. The root cause was filed as
  `EHotwagner/S.I.R.#258`, which is **CLOSED as NOT_PLANNED** — re-routed rather than resolved,
  because no S.I.R. worker can land a durable fix in a distributed artifact. Its live home is
  `FS-GG/.github#2856`, **OPEN**. The prior cycle report
  `feedback/2026-08-22-sir-item-252-audit-binding-ledger-currency.md` §4.2 already records this exact
  root cause, and notes the symptom alone appears in at least four earlier reports that never
  reached it.
- **Avoidable cost:** one dedupe investigation. Nothing filed — the cause is carried upstream. An
  earlier revision of this report wrongly deduped to the closed `#252` and asserted its repair was
  "still in flight" on the strength of `git merge-base --is-ancestor 8972d37 origin/main` returning
  false; that proxy is invalid under squash-merge, since `#254` merged under a different SHA. The
  behavioural measurement above replaces it.
- **Disposition:** existing issue — `FS-GG/.github#2856`.

#### §4.7 The suite reds for a cause outside the diff, and destroys the file under test when interrupted

*Critic-originated: raised as `§C1` in the pass-3 actionability audit, reported against the critic's
own interest after its run produced a red that was not real. **Part (a) as originally written was
overstated; it is corrected below against measurement, and the correction is the finding's most
useful content.***

- **Kind:** defect
- **Impact:** Two failures in the file that is supposed to be this item's guard. Found because an
  independent reviewer's run collided with an edit to `scripts/agent-env.sh` and produced a red that
  was not real — costing that reviewer a wasted run and an investigation.
- **Expected:** A suite's red should identify one condition. An interrupted suite should not damage
  the artifact it tests.
- **Observed:** `scripts/test-agent-env.sh` sets `SHIM` to the **real tracked file** and section H
  `mv`s it aside, so anything reading that file during the window sees it missing or truncated.
  **(a)** A concurrent run therefore reds section I for a cause that has nothing to do with
  `DOTNET_ROOT`.
  **An earlier revision of this finding said that red was "byte-identical" to a genuine inversion and
  that the two are "indistinguishable in the output". That is false. I never measured it — I carried
  it from the reviewer's report — and a later audit was unable to reproduce it.** Measured directly at
  `2e4b07e` (pre-lock), across three collision shapes (shim moved aside, truncated, rewritten) and two
  timings:

  | condition | section I assertion | its non-vacuity control | overall |
  |---|---|---|---|
  | genuine `export` deletion | red at **`rc=1`** | **green** | 1 unexpected |
  | concurrent collision | red at **`rc=3`** | **also red at `rc=3`** | 1-9 unexpected, timing-dependent |

  The conditions are **distinguishable on two independent signals**: the exit code (`3` is the
  deliberate build-failure code — the collision breaks the probe's `dotnet build`, it does not make
  the assertion fail — while `1` means the assertion fired) and the control (green under a genuine
  inversion, red under a collision, because a collision breaks both probes and a real inversion breaks
  only the asserted one). **The 3/4/5 exit-code discipline that §2 praises had already done its job;
  this finding claimed it had not.** What survives is narrower and still sufficient: a concurrent run
  produces an untrustworthy result and burns a reviewer's run, so the suite should refuse rather than
  emit one. Only the *count* is timing-dependent and unreproducible — one narrowly-timed collision
  gives `RESULT: 1 unexpected`, matching the reviewer's summary line, but with the **control** red at
  `rc=3` rather than the assertion at `rc=1`.
  **(b)** The only `trap` was `rm -rf "$TMP"`, and the backup lives in `$TMP`, so a run killed
  between the two `mv`s removed the working tree's `scripts/agent-env.sh` **and** its sole backup in
  one command. Verified by reconstructing the old trap and sending `SIGTERM` inside section H: the
  file was gone. Fixed in `439bede` with a per-worktree lock under the git dir that refuses rather
  than emitting an untrustworthy result, and a trap that restores the shim before removing `$TMP`,
  covering `INT` and `TERM` as well as `EXIT`. Refusal exits **99**, not 1, because this script's
  contract is "exit code is the number of unexpected outcomes" and refusing with 1 would be
  indistinguishable from one failed check. That reason stands on its own and does not depend on
  (a)'s retracted claim.
- **Evidence:** file:scripts/test-agent-env.sh; command:./scripts/test-agent-env.sh
- **Version:** fixed at commit `439bedeb35cec56f6d1f5256158e4e46bc66d4d2` (a *fix* commit, not the
  frontmatter's described commit); the defect predates this
  cycle and is present at base `f69f1e6`.
- **Owner:** S.I.R. / `scripts/test-agent-env.sh`
- **Recurrence:** new for (b). For (a), the recurrence claim itself was the error: it was filed as a
  third instance of "a signal that cannot distinguish two conditions" and is not one — the signal
  distinguished them. Recorded because a finding that *pattern-matches* an item's thesis is the one
  least likely to be measured, by the author or by the reviewer who raised it.
- **Avoidable cost:** one wasted measurement and a re-run for the reviewer; one artifact fix here.
- **Disposition:** product fix — corrected in this cycle, in `439bede`.

#### §4.8 The guard written for §4.7 could not fail, and its inversions were claimed rather than committed

- **Kind:** defect
- **Impact:** Two failures in one guard, and the second is the sharper. The fix for a
  could-not-fail signal was itself a could-not-fail signal; and once fixed, its evidence was
  asserted in a commit message while the repository contained none of it.
- **Expected:** A guard reds when its subject is broken, and the suite can demonstrate that at rest.
- **Observed:** **(a)** The first lock did `mkdir "$LOCKDIR"` and, on failure, tested whether the
  directory *existed*. A losing `mkdir` leaves the **holder's** directory in place, so that test
  always succeeded: run against a live holder, the suite executed in full and returned `0`. It
  passed its own concurrency test. Fixed by tracking acquisition explicitly. **(b)** The commit
  message then stated the guard "ships with evidence it can fail" and listed four inversions. **The
  repository contained none of them.** All 38 checks were environment checks and
  `grep -cE '^run (pass|fail).*(lock|REFUSED|99|reclaim)'` returned `0`; the inversions existed in a
  worker transcript and an independent reviewer's transcript and nowhere that outlives either
  session. The substance was true and the words "are committed" were false — an independent audit
  called it `incomplete` on exactly that ground. This is §4.1's own condition, unfalsifiable at
  rest, occurring inside the finding about a guard that could not fail.
- **Resolved:** section J commits eleven checks at the described commit (J1-J11); the four that
  close this finding are (J1 live holder refuses at 99 naming the lock; J2 a
  refused run leaves the holder's lock in place; J3 a dead holder is reclaimed, proved by contrast
  via the differing refusal message; J4 an absent shim refuses rather than being mistaken for the
  section H mutation), each with a committed inversion that reds it: lock-tests-existence → J1+J2;
  refusal-deletes-lock → J2; reclaim-removed → J3; shim-guard-removed → J3+J4.
- **A hazard the fix introduced, found at authoring time:** the fixture is a copy of the suite, so
  an unguarded section J builds its own fixture without bound. It surfaced only while **inverting**
  J1 — disabling the lock is precisely what lets a nested run reach its own section J, so the
  mutation that proves the guard works is also the one that removes the brake. Fixture invocations
  now carry `FSGG_AGENT_ENV_SUITE_FIXTURE=1` and the section is skipped under it.
- **Evidence:** file:scripts/test-agent-env.sh; command:./scripts/test-agent-env.sh
- **Version:** described commit.
- **Owner:** S.I.R. / `scripts/test-agent-env.sh`
- **Recurrence:** new
- **Avoidable cost:** (a) none — caught at authoring time by `pnext-item`'s inversion rule. (b) four
  commits during which the guard was unfalsifiable at rest; caught by independent audit, not by the
  author.
- **Disposition:** product fix — both corrected in this cycle.

#### §4.9 The suite's documented `[repo-root]` argument tested the caller's shim, not that root's

- **Kind:** defect
- **Impact:** `scripts/test-agent-env.sh [repo-root]` is documented usage. Given any root other than
  the caller's own, every wired check **tested the wrong artifact — which both misses the real defect
  and can manufacture a false one.** The suite was not silent about it: it printed
  `WRONG (expected fail got rc=0) wired, but the shim is deleted`. It was loud and misattributed,
  which is worse than silence, because the operator is handed a specific and wrong conclusion about
  a file that was never under test.
- **Expected:** Given a root, the suite exercises the shim at that root.
- **Observed:** `BASH_ENV_VALUE` is `$(git rev-parse --show-toplevel)/scripts/agent-env.sh` — the
  exact form both host config files set, and correctly so. Bash performs that command substitution
  **at shell startup**, before it runs `-c`. `fresh()` did `bash -c "cd '$ROOT' && …"`, so the `cd`
  happened *after* the substitution and it resolved against the **caller's** cwd. Measured with the
  target root's shim moved aside: the wired probe still answered `10.0.302` and section H's "wired,
  but the shim is deleted" **passed**, because the caller's intact shim had been sourced instead.
  Fixed in `1020717` by cd-ing in a subshell before `env -i`; the `BASH_ENV` value is deliberately
  unchanged, since the host-configured form is what is under test.
- **Evidence:** file:scripts/test-agent-env.sh; command:git worktree add --detach ../sir-277-base f69f1e6cdc203121bd908a3a1bd025545e0aff56 && sed -i -e '\|export DOTNET_ROOT="$candidate"|d' ../sir-277-base/scripts/agent-env.sh && ( cd ../sir-277-base && ./scripts/test-agent-env.sh ) && git worktree remove --force ../sir-277-base
- **Version:** fixed at commit `1020717a4f9aec5df7d79aac57bce474f621c870` (a *fix* commit, not the
  frontmatter's described commit); the defect predates this
  cycle and is present at base `f69f1e6`.
- **Owner:** S.I.R. / `scripts/test-agent-env.sh`
- **Recurrence:** new. Fourth instance in this cycle of a signal that does not mean what it says —
  after §4.1, §4.5 and §4.7.
- **Avoidable cost:** one wrong measurement. Found only because a base-commit reproduction built for
  §4.1 disagreed with the figure it existed to confirm, and the disagreement turned out to be this
  bug rather than the figure.
- **Disposition:** product fix — corrected in this cycle, in `1020717`.

#### §4.10 `check-invalidation`'s failure mode suppresses true positives, and one was mine

- **Kind:** defect
- **Impact:** A worker reading the documented pre-commit command concludes "these errors are
  pre-existing and none are mine" — which is what this cycle concluded on its first pass, and it was
  wrong. A real merge obligation was hidden behind that verdict.
- **Expected:** The command reports the merged-audit citations this commit invalidates.
- **Observed:** The invalidation listing prints **only on the non-FAIL path**. Ledger errors are
  present for any realistic diff (§4.6), so the listing effectively never prints. Same tree, same
  file, same digest change:
  the **documented** form `--base origin/main --head HEAD` → `FAIL: validation failed (15 error(s))`
  and the output **never names `scripts/test-agent-env.sh`** (grep count 0), even though this
  commit changes it. Cited in the documented form deliberately: it removes any "you misused
  `--changed`" objection. The same run with the 13 ledgered paths added, which clears the overbroad errors →
  `4 merged feedback-audit binding(s) invalidated`, including
  `feedback/audits/2026-08-22-sir-item-256-agent-toolchain-wiring.audit.json` §4.1 **and** §4.4,
  both `file:scripts/test-agent-env.sh`. This cycle's change moves that file from
  `988b7252…` to whatever it is at the described commit — deliberately not quoted here, because it
  moves with every repair round and quoting it is how §4.15 happened twice — so two sealed citations
  in a **merged** audit are genuinely invalidated and the exception-ledger entry is genuinely owed. The defect is therefore not only
  that the gate cannot pass; its failure mode **suppresses true positives**, and the louder the
  ledger grows the more it suppresses.
- **Evidence:** command:dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- check-invalidation --base origin/main --head HEAD; issue:FS-GG/.github#2856
- **Version:** measured at described commit; the vendored tool carries no independent version string.
- **Owner:** FS-GG `fs-gg-feedback-report` skill, `FeedbackReportTool.fs` — same owner as §4.6.
- **Recurrence:** new facet of an existing cause. Transplanted onto `FS-GG/.github#2856` rather than
  filed, per the dedupe rule; not present in that row's body or in
  `feedback/2026-08-22-sir-item-252-audit-binding-ledger-currency.md` §4.2, both of which
  characterise the defect as "cannot pass" rather than "hides what it exists to report".
- **Avoidable cost:** the owed exception-ledger entry was nearly missed entirely.
- **Disposition:** existing issue — `FS-GG/.github#2856`, evidence transplanted.

#### §4.11 The ledger's trigger is touch-based while its remedy is movement-based, and closing the gap requires a false record

- **Kind:** capability-gap
- **Impact:** This cycle owes an exception-ledger entry and **did not write one**. The obligation is
  real; the compliant route requires asserting something untrue; the refusal is therefore a
  truthfulness decision, not a technical dead end.
- **Expected:** A change that invalidates a merged audit's cited evidence records a replacement
  digest, replacement evidence and a reason, and the gate goes green.
- **Observed:** the tool is behaving as documented throughout, which is why this is a gap in the
  contract's shape rather than a bug in its implementation.
  `SKILL.md:376-378` states that `check-invalidation` fails "for each **touched** citation" and
  "does not run the full historical validator or read cited files". A tool that never reads the file
  cannot compare digests, so a touch-based trigger is the documented and correct behaviour. The gap
  is between that trigger and the **remedy's shape**: every ledger entry carries both
  `previousSha256` *and* `replacementSha256` — the live `ci-route.mjs` entry runs
  `10da76f4…` → `aec19c9ac8fe…` — so the remedy describes a **movement**, while the output that
  demands it reports a **touch**. Measured on the cleanest possible input: `--changed` over only the
  13 ledgered paths, a change set in which **not one digest differs from its recorded citation**,
  reports `2 merged feedback-audit binding(s) invalidated`. Nothing had moved. Of the citations this
  cycle actually touches, only `scripts/test-agent-env.sh` genuinely moved
  (`988b7252…` → its current digest).
- **Consequence for this PR.** Adding the two owed entries reddens the wrapper gate, because it
  copies the whole real ledger into a curated fixture and any entry whose path the fixture lacks is
  `overbroad or mismatched`. Extending the fixture with the 256 audit leaves it red on a **third**
  citation, §4.6 `file:scripts/ci-route.mjs`, whose recorded digest `aec19c9ac8fe…` **equals the
  current file**. Green therefore requires an entry with `previousSha256 == replacementSha256`: a
  record of a replacement that never happened, plus a reason attesting that a third party's finding
  survives bytes that never changed.
- **The blockage is dishonesty, not impossibility.** Nothing indicates the tool rejects such an
  entry; it would very likely be accepted. The entry was refused because it would be false, and the
  host's ruling upholds that. This is a truthfulness objection to a route that is technically open,
  which is a stronger claim than being unable to comply.
- **Observed consequence:** the repository's actual gate
  `scripts/test-feedback-audit-binding-exceptions.sh` is **green (rc=0)** on this change, because its
  fixture does not cover the 256 audit. The obligation is simultaneously real and structurally
  unenforced.
- **Evidence:** file:scripts/test-feedback-audit-binding-exceptions.sh; command:./scripts/test-feedback-audit-binding-exceptions.sh; issue:FS-GG/.github#2856
- **Version:** measured at described commit.
- **Owner:** FS-GG `fs-gg-feedback-report` exception ledger and its wrapper fixture — same owner as
  §4.6 and §4.10. Framed as a trigger/remedy mismatch rather than as the tool misbehaving, because
  the touch predicate is documented and the "misbehaviour" framing is what would get the row closed.
- **Recurrence:** new facet of the same cause; transplanted onto `FS-GG/.github#2856`.
- **Avoidable cost:** one attempted discharge, reverted. Nothing shipped.
- **Disposition:** accepted as a disclosed, undischarged obligation, with an explicit host ruling.
  Two paths were widened for the attempt with `disjoint` verdicts and neither appears in the final
  diff; a widen is a permission, not a commitment.

#### §4.12 The concurrency guard decided liveness from a pid file it could not read

- **Kind:** defect
- **Impact:** The guard added to stop an untrustworthy red was itself untrustworthy on a whole input
  class. It deleted a **live** holder's lock and ran — reintroducing the exact collision it exists to
  prevent — and separately could never reclaim an abandoned one.
- **Expected:** A lock whose holder cannot be identified is neither taken nor treated as free.
- **Observed:** `kill -0 "$(cat "$LOCKDIR/pid")"` on an empty or non-numeric pid expands to
  `kill -0 ""`, which fails, which the guard read as *the holder is dead*. Measured on the round-0
  head, live holder in every row: valid pid → `rc=99`, lock kept; **EMPTY pid → `rc=0`, lock
  DELETED**; **NON-NUMERIC → `rc=0`, lock DELETED**; no pid file → `rc=99` forever, which falsified
  the file's own comment that a crashed run cannot wedge the suite. One root: no distinction between
  *read and decided* and *could not read*. `lock_holder_state` now returns
  `live | dead | unreadable`, and repairing it at the predicate rather than special-casing two inputs
  immediately surfaced a **third** shape neither the critic nor the author had tested — pid `0`,
  whose `kill(2)` target is the whole process group. Unreadable is bounded by age, so it can neither
  delete a live lock nor wedge the suite.
- **Evidence:** file:scripts/test-agent-env.sh; command:./scripts/test-agent-env.sh
- **Version:** described commit; defect introduced by this cycle's own round-0 diff.
- **Owner:** S.I.R. / `scripts/test-agent-env.sh`
- **Recurrence:** new. The `.github#2223` unreadable-input shape: a predicate returning a confident
  answer about input it could not evaluate.
- **Avoidable cost:** one critic round.
- **Disposition:** product fix — round 1.

#### §4.13 The interrupt trap ran cleanup and then let the suite carry on through the signal

- **Kind:** defect
- **Impact:** A regression this cycle introduced. At base a signal kills the suite; at the round-0
  head it **survived**, released its lock mid-run while section H still had the real tracked shim
  moved aside, and kept emitting check failures. (An earlier revision called those failures
  *indistinguishable* from real ones; that is the §4.7(a) claim and it is retracted there — the
  exit-code discipline separates them. What survives is that the run continued at all.) — the
  one-signal-two-meanings confusion `exit 99` was chosen to prevent, reintroduced by the handler
  meant to make interruption safe.
- **Expected:** `SIGINT`/`SIGTERM` terminate the suite.
- **Observed:** A bash trap handler that does not exit **returns to the interrupted line**.
  Measured at the round-0 head `3a38243`, where the suite carried 42 checks: TERM → the suite ran to
  completion, exit `2`, all 42 check lines emitted after the signal.
  `on_signal` now restores the default disposition and re-raises: TERM → `143`, INT → `130`, zero
  checks after the signal. **The spurious-`WRONG` count is a sample, not a property** — this run
  measured 2, an independent run measured 3, and the difference is which checks were in flight when
  the signal landed. Recorded as a range rather than a figure.
- **A residual race, documented rather than denied:** `mkdir` and the assignment after it are
  separate commands and bash dispatches traps between commands, so a signal can land with the lock
  created but unclaimed — reproduced 5 times in 5 by polling on the directory. Shell cannot close it.
  The failure mode is bounded instead: such a lock has no pid file, is classified `unreadable`, the
  next run **refuses** rather than deleting it, and the staleness window reclaims it. Demonstrated
  end to end.
- **Evidence:** file:scripts/test-agent-env.sh; command:./scripts/test-agent-env.sh
- **Version:** described commit.
- **Owner:** S.I.R. / `scripts/test-agent-env.sh`
- **Recurrence:** new
- **Avoidable cost:** one critic round.
- **Disposition:** product fix — round 1.

#### §4.14 Three defects that only inverting the checks revealed, one of them in a check written this cycle

- **Kind:** positive-pattern
- **Impact:** Running the new checks proved nothing about three of them. Each was found by breaking
  the subject and watching which check moved.
- **Expected:** n/a — this records a practice and the defects it exposed, not a single defect.
- **Observed:** **(a) J10 could not fail.** It polled for the shim to vanish then asserted it was
  back, so when it missed the window it signalled a finished run, found the shim restored by section
  H's own `mv`, and passed — green even with the restore deleted from `cleanup` — and flaked
  intermittently. **The shipped configuration reds 6 runs in 6, and that is the figure that counts;
  the flake rates measured on the pre-fix and doubly-mutated fixtures were samples of a
  timing-dependent quantity and are not quoted.** Repaired twice: it now records that it *observed* its precondition and reds when it did not, and
  the fixture's window is widened **in the copy** so the check tests the trap rather than the
  scheduler. **(b) SIGINT was unasserted.** Reverting only the INT handler left every check green, so
  a TERM-only assertion would have let the interactive Ctrl-C path regress silently; each trapped
  signal now has its own witness. `set -m` is load-bearing there — a shell starts an async job with
  SIGINT ignored and POSIX makes an ignored signal untrappable, so without job control the fixture
  cannot install the handler under test. **(c) The generator rewrite dropped the shim-missing guard,
  and J3/J4/J8 went red immediately** — the first time on this item the suite caught the author
  rather than a reviewer catching the suite.
- **Evidence:** file:scripts/test-agent-env.sh; command:./scripts/test-agent-env.sh
- **Version:** described commit.
- **Owner:** S.I.R. / `scripts/test-agent-env.sh`
- **Recurrence:** new; same family as §4.8.
- **Avoidable cost:** none — all three caught at authoring time.
- **Disposition:** accepted; every check J1-J11 now has a committed mutation that reds it.

#### §4.15 The hash-verification practice this report proposes cannot catch the hash error it shipped

- **Kind:** documentation
- **Impact:** Third hash error on this item, in an item whose subject is unevidenced claims.
- **Expected:** A hash written into a report or PR body is verifiable by the practice that report
  proposes for verifying hashes.
- **Observed:** The PR body gave the new digest of `scripts/test-agent-env.sh` as `748495da…` — the
  value at `d849243f`, stale by one commit when written, and the value a later worker would carry as
  `replacementSha256` to `FS-GG/.github#2856`. §11's proposed practice, resolving every hash through
  `git cat-file -t`, **structurally cannot catch it**: `git cat-file -t 748495da` correctly fails
  because it is a *content* digest, not a git object. It was caught only by recomputation. The
  generalisable lesson is narrower than "check your hashes": **a reference is verifiable only against
  the thing it names**, and a check covering one class of hash gives zero coverage of another that
  looks identical.
- **Evidence:** issue:EHotwagner/S.I.R.#298; command:git show d849243:scripts/test-agent-env.sh > tae-at-d849243.tmp && dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- digest tae-at-d849243.tmp && rm tae-at-d849243.tmp; command:git cat-file -t 748495da
- **Version:** the stale value was the digest at `d849243`; the correct value is recomputed from the
  file at whatever commit a reader needs it for, and is deliberately not quoted here.
- **Owner:** S.I.R. / feedback report and PR authoring practice
- **Recurrence:** third instance when found — after a transcribed commit SHA and a sweep reporting
  three false failures — and a **fourth** was found in this report while checking for the third:
  §4.10 still carried the stale value as though current. The repair is not a better value but a rule:
  **a digest that moves with the head is not quoted in prose at all**, it is named relative to the
  commit and recomputed by whoever needs it.
- **Avoidable cost:** none shipped; corrected in the body as a stated correction.
- **Disposition:** accepted — §11 proposal 4 amended to say what it does and does not cover.

## §5 Did not exercise

- Scaffolding, onboarding, and the SDD lifecycle: not owed on a `lightweight` route, and not touched.
- The change was not exercised on a non-Linux host, nor on a machine carrying a single .NET install.
  The non-vacuity control in section I is what would report that second case; it was not itself
  exercised in that configuration.
- `scripts/agent-env.sh` behaviour under Codex CLI was not re-measured this cycle; only the
  `BASH_ENV` contract the suite already pins was exercised.
- The tier-2 apphost path was not exercised and cannot be from this repository (§4.4).

## §6 Doc-versus-behavior contradictions

The subject of this item was one, and it is resolved rather than merely observed.

`scripts/agent-env.sh` stated: "`DOTNET_ROOT` is exported only in case 2, only to the root that was
positively confirmed to carry the pinned SDK, so the muxer that PATH now resolves and the root that
apphosts consult agree." The behaviour matched the sentence, but nothing in the repository could
demonstrate the second clause, and deleting the line it justified changed no observable outcome in
the committed suite. The sentence was true and unfalsifiable at the same time. It is now bound to
`scripts/test-agent-env.sh` section I, which reds when the export is removed.

Two further contradictions were introduced by this cycle and corrected within it, both in the same
replacement sentence. **(a)** It cited a tier-2 `fsgg-coord-engine` apphost that this repository
cannot resolve, contradicting `scripts/fsgg-coord`'s own statement that "a receiver resolves at tier
1/3/4". **(b)** The correction for (a) then named the `$HOME/.dotnet/tools` shims as a hot path,
contradicting the tree, in which no committed script invokes a tool as a bare command. Both are
recorded in §4.5.

A third, in the harness rather than the shim: `scripts/test-agent-env.sh` section I stated the
deletion left "all 36 checks above" green "reproduced at this head", which read as though the 36/0
figure reproduces at head while §1 denies exactly that. Corrected in `8279c89` to state both
commits explicitly.

Owning documentation for both: `scripts/agent-env.sh` header comment and `scripts/test-agent-env.sh`
section I comment. No other contradiction observed.

## §7 Workarounds still in the tree

None introduced by this cycle. `scripts/agent-env.sh` is itself a workaround for two agent hosts that
do not expand variables in their `env` blocks, but it is a documented and asserted one, its removal
condition is stated in its own header, and this cycle narrowed rather than widened its unproven
surface.

## §8 Friction and avoidable cost

- Retries: none.
- Manual edits reverted: one, self-inflicted and instructive. Restoring the mutated shim with
  `git checkout -- scripts/agent-env.sh` during an inversion run also discarded an uncommitted
  correction in the same file; the re-run used a file copy instead. This is the same asymmetric
  restore hazard the critic flagged in §4.1's locator.
- Lifecycle reruns: none.
- Generated files replaced: none.
- Worker restarts: none.
- One redirected measurement round (§4.4), between reproducing S8 and locating a consumer that can
  discriminate `DOTNET_ROOT`.
- Two artifact corrections plus three full re-runs of the green/inversion/restore loop (§4.5).
- One artifact fix for the suite's own concurrency and interrupt hazards, plus four inversions for
  its guard, one of which found the guard could not fail (§4.7, §4.8).
- One dedupe investigation against a red default branch (§4.6), and one against a closure chain
  three hops deep (§4.6 recurrence).
- One attempted audit-binding discharge, reverted without shipping (§4.11).
- One hand-transcribed commit SHA in this report was wrong and was caught by resolving every hash in
  the document through `git cat-file -t`, not by re-reading it. Nothing shipped with it; the audit
  had not been written. Recorded because the error class is this cycle's own subject: an unverified
  claim in a document that exists to carry verified ones.
- Command duration, kept separate from wall clock: at the described commit the suite measures 11-13s, including one
  warm `dotnet build` of a single-file probe with no `PackageReference`, built cold in a per-run
  `mktemp -d`. The base suite was not timed.

## §9 Skill value and gaps

- `pnext-item` — invoked, and followed literally. Its §3 rule that every added gate ships with
  evidence it can fail is what produced the inversion measurement rather than a green suite and a
  claim. Evidence: `scripts/test-agent-env.sh` section I and the `rc=1` inversion result.
- `fs-gg-feedback-report` — invoked in checkpoint mode at the implementation-test-evidence and
  verify-ship-pr boundaries and in finalize mode for this report. Its independent actionability
  critic returned a finding that changed a shipped artifact, not merely the report (§4.5); that is
  the strongest evidence for the two-pass design in this cycle. Evidence:
  `feedback/checkpoints/item-277-agent-env-dotnet-root.jsonl`, 22 events.
- `intra-repo-parallel-work` — exercised through `whoami --mint`, `take`, `widen`, and the worktree
  isolation rule. `widen` was required because the feedback contract writes under `feedback/`, which
  is outside the item's declared `Paths:`; the widen returned `disjoint`.
- Not invoked, with reason: the `fs-gg-sdd-*` family (route is `lightweight`, no SDD package owed);
  the `fs-gg-*` gameplay/simulation skills (no runtime or simulation surface touched); the
  `performance-first` planning gate (no interactive or per-tick work — the only performance-relevant
  fact is the suite's own runtime, recorded in §8).
- Wanted and absent: none identified. No misleading skill guidance observed this cycle.
- Overlap: none newly observed.

## §10 Outcome markers

- Time to first build: n/a — no product build in this cycle's scope.
- First meaningful test: the baseline suite re-run at base commit `f69f1e6`, 36 checks / 0
  unexpected.
- First render or playable state: n/a.
- First green verification: 38 checks / 0 unexpected at `439bede`, when section I was added.
- Inversion verified: exactly 1 unexpected outcome with `export DOTNET_ROOT="$candidate"` deleted,
  at `rc=1`, in the intended check, with all 48 others green at the described commit. Re-verified
  after each subsequent repair.
- Artifact defects found during this cycle: **10**, enumerated so the count and the list agree —
  two caller-inventory claims (§4.5), the suite's out-of-diff red and its destructive trap (§4.7,
  two), a guard that could not fail *and* inversions claimed rather than committed (§4.8, two), a
  documented argument that tested the caller's artifact (§4.9), a predicate deciding liveness from
  unreadable input (§4.12), a trap that did not terminate (§4.13), and a check that certified a
  property it never observed (§4.14). **4 before the PR opened** (§4.5 ×2, §4.8(a), §4.9);
  **6 after, by independent review** (§4.7 ×2, §4.8(b), §4.12, §4.13, §4.14).
- Overstated claims retracted against measurement: **1** (§4.7(a)) — counted separately from the 10,
  because a retraction is not a defect in the artifact but a defect in the report about it.
- Ship readiness and merge: recorded in the PR, not estimated here.
- Test count delta: 36 to 49. Command duration is reported in §8 and is not offered as a substitute
  for elapsed time, which was not instrumented this cycle.

## §11 Falsifiable improvements

1. **Assert the tier-2 engine's apphost root in the repository that owns coord's source.** Observed
   finding: §4.4 and §4.5 — the highest-value apphost consumer of the export is `fsgg-coord-engine`,
   and it is structurally unreachable from S.I.R., which is why an attempt to cite it here was
   wrong. Owner: **the FS-GG repository that owns `src/FS.GG.Coord.Cli`, not S.I.R.** — routing it
   here would repeat §4.5's mistake. Acceptance condition: in that repository, a check asserts the
   built `fsgg-coord-engine` loads its runtime from the same install the resolved muxer lives in,
   and reds when `export DOTNET_ROOT="$candidate"` is deleted from the shim it ships. Falsifiable:
   if that engine is published self-contained rather than framework-dependent, it consults no
   `DOTNET_ROOT` and the proposal should be withdrawn.
2. **Name the consumer classes a harness covers, so coverage is distinguishable from blindness.**
   Observed finding: §4.1 — 36 checks exercised one consumer class and were structurally unable to
   see the second; §4.5 — the surrounding prose then got the caller inventory wrong and no check
   noticed. Owner: S.I.R. / `scripts/test-agent-env.sh` header. Acceptance condition: the header
   names the consumer classes the suite covers (muxer, apphost) and states that caller inventories
   in comments are unasserted, so a future reader can tell which claims the suite backs without
   re-deriving it.
3. **Extend the independent-review gate to comments, not only to code.** Observed finding: §4.5 —
   the claim was refutable only by resolving `src/FS.GG.Coord.Cli` against the tree and by scanning
   for bare tool invocations. Neither fact is visible in the diff: a reviewer reading the patch sees
   a plausible sentence about tiers and callers, and the refutation lives entirely outside the
   changed lines. Owner: FS-GG `independent-review` contract (a contract observation, not S.I.R.
   work). Acceptance condition: the contract directs the critic to resolve caller, path, and tier
   claims made in prose against the tree, and treats an unresolvable one as material — such that a
   claim like §4.5(a) is a review finding rather than a merged sentence. Falsifiable: if critics
   already do this by default, the two errors in this cycle would have been caught by the diff read
   alone, and they were not.
4. **Resolve every hash in a report before sealing its audit.** Observed finding: §8 — a commit SHA
   hand-transcribed into this report was wrong, in a document whose whole purpose is verifiable
   evidence, and re-reading it did not catch it. Owner: S.I.R. / feedback report authoring practice,
   or the `fs-gg-feedback-report` validator if it is worth mechanising. Acceptance condition: every
   40-character and short hash in a report resolves via `git cat-file -t` in the repository the
   report describes, checked before the audit is written. **Scope, added after it failed to catch a
   fourth error (§4.15):** this covers git object references ONLY. A content digest is not a git
   object, `git cat-file -t` correctly fails on it, and the check has nothing to say — those must be
   recomputed from the file they name. Falsifiable: if no report has ever carried an unresolvable
   hash, the check is unnecessary — this one did, before the check was run.
5. **Make `check-invalidation` usable as a branch signal.** Observed finding: §4.6 — it is red on a
   clean default branch, so its output cannot be read as a verdict on a worker's own changes. Owner:
   S.I.R. / `feedback-tool check-invalidation`. Acceptance condition:
   `check-invalidation --base origin/main --head origin/main` exits 0 on a clean default branch, so
   any non-zero result on a branch is attributable to that branch. Route to `FS-GG/.github#2856`, which is
   open and carries this cause; `S.I.R.#252` and `S.I.R.#258` are both closed, the latter
   NOT_PLANNED and explicitly re-routed upstream.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | No scaffold or template provider involved; a two-file change in an existing checkout. |
| onboarding-guidance | partial | The dispatch brief's toolchain facts (`DOTNET_ROOT`/`PATH` export, repo name with trailing dot, non-persisting `whoami --mint`) were applied and all three held. `docs/workspace-onboarding.md` was read for drift and states no check count, so this change caused none. |
| skills | exercised | `pnext-item`, `fs-gg-feedback-report`, `intra-repo-parallel-work` invoked; §9 records each with evidence and records the non-invoked families with reasons. The feedback skill's independent critic changed a shipped artifact (§4.5). |
| sdd-authoring | not-exercised | Route receipt is `lightweight` with `sddPackageReady: true`; no SDD package was owed and none was authored. |
| implementation-apis | not-exercised | No product API touched. The .NET host's `DOTNET_ROOT`/`hostfxr` contract was measured, not consumed as an API. |
| dependencies-build | partial | `dotnet build` exercised only as a single-file probe built by the new check with the pinned SDK 10.0.302; no product build, no restore of the tool manifest. |
| testing | exercised | The testing surface was itself the subject of seven of this report's findings (a finding count, not the defect count of 10 in §10, which splits §4.7 and §4.8 into two each): §4.1 (a 36-check suite that could not falsify its subject), §4.7 (a red for a cause outside the diff — NOT, as an earlier revision said, one indistinguishable from a real inversion; that is retracted in §4.7 — plus a trap that destroyed the file under test), §4.8 (a guard that could not fail, and whose inversions were claimed as committed while absent from the tree), §4.9 (a documented argument that tested the caller's artifact), §4.12 (a predicate deciding liveness from a pid file it could not read), §4.13 (a trap that ran cleanup and let the suite continue through the signal), and §4.14 (a check that certified a property it never observed). Suite extended 36 to 49 checks; every added gate ships with a committed inversion, and each inversion was run and the redding check identified. |
| evidence | exercised | S8 reproduced independently at the base commit before repair rather than taken from the issue table; `COREHOST_TRACE=1` used to observe the resolved framework directory under each root; 22 checkpoints recorded and validated with `validate-checkpoint-state`; §4.6 records a red default branch that makes `check-invalidation` unusable as a branch signal. |
| runtime-playtest | not-exercised | No runtime or simulation surface in scope. |
| performance | partial | Only the suite's own cost was measured: 11-13s wall at the described commit, including one warm `dotnet build` and two deliberately-interrupted fixture runs; the base suite was not timed, so no delta is claimed. `pnext-item`'s performance-first planning gate was not owed — no interactive or per-tick work. |
| documentation | exercised | The shim's header comment was the defect's subject; it now states the mechanism, the `COREHOST_TRACE=1` measurement, and the check that holds it, and the export line carries a one-line pointer to section I. Two wrong caller inventories in that same replacement text were found and corrected (§4.5), and an ambiguous commit binding in the harness header was corrected after review (§6). `docs/workspace-onboarding.md` checked for count drift; none found and it is outside the declared paths. |
| packaging-upgrade | not-exercised | No package, pin, or manifest changed. |
| worker-git-pr | exercised | Identity minted (`crake-610f`), item claimed through the scheduler (converged), isolated worktree from `origin/main`, touch-set widened to the feedback paths with verdict `disjoint`, PR opened against `main` with a bare `Closes #277`. |
