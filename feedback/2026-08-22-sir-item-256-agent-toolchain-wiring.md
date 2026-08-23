---
feedbackSchema: 2
date: 2026-08-22
workspace: S.I.R
cycle: item-256-toolchain-wiring
lane: none
toolVersion: n/a
commit: 442d28189f4a9b7eb5812b6db2fdd0f288276e82
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 6
- **zero-event reason:** n/a

Cycle boundary: board item `EHotwagner/S.I.R.#256`, claimed by worker `tern-fc83` at claim marker
comment `5382729642`, delivered as PR #260 on branch `item/256-agent-toolchain-wiring`. The branch was rebased onto
`origin/main` at `11cfc87adbe410b561e4ecb8592c9b09241edcea` after PR #254 landed, so the base named
in an earlier draft (`1d8c93d…`) is no longer this branch's base. Described commit
`442d28189f4a9b7eb5812b6db2fdd0f288276e82`. Independent review of the pre-rebase head returned
`changes-required` with two material findings; the repair is in this described commit's ancestry and
the completed round is **round 0** (`reviewGeneration …:initial-review:0`). A round-1 review at the
final head had not occurred when this report was written, and this report does not claim one.

Checkpoint path `feedback/checkpoints/item-256-toolchain-wiring.jsonl`, 6 events, validated with
`validate-checkpoint-state`.

Lifecycle: `none`. The item's typed delivery-route receipt is `route: lightweight`, `kind: current`,
`sddPackageReady: true`, so no `fsgg-sdd` front half was owed and none was authored. `toolVersion` is
`n/a` for the same reason. Coordination engine `fs.gg.coord.cli` 0.71.0.

Pinned toolchain under test: `global.json` pins SDK `10.0.302` with `rollForward: disable`; the
installed roots on the measured machine are `/usr/share/dotnet` (`6.0.428`, `10.0.400`) and
`$HOME/.dotnet` (`10.0.302`).

Confidence limits. Every host-behaviour claim below was measured on one machine (Linux, Claude Code
2.1.240, Codex CLI, `/bin/sh` symlinked to bash). The two host-side findings (§4.2, §4.3) are
reproducible but not established as version-independent; §4.3 in particular describes an
implementation detail of a generated file that its owner is free to change. §4.6's root cause is
attributed to an already-filed issue rather than diagnosed independently: this cycle established only
that the failing subject lies outside this branch's diff.

The limit that mattered most was absent from the first draft and had to be found by the
actionability audit: single-machine and version-dependence are weaker limits than *inspectability*.
§4.2 and §4.3 originally cited only files this branch authored, which restate a claim rather than
witness it. The reproduction routes now cited sat in this cycle's own checkpoint file the whole time
and were dropped when the report was written.

## §2 What worked

- **The typed delivery-route receipt answered a scope question before any work started.** Reading
  `delivery-route show` returned `route: lightweight` with `sddPackageReady: true`, which settled
  whether an SDD front half was owed without inspecting `work/` or guessing from the item's class.
- **`widen` with a `disjoint` verdict made growing the touch-set a decision rather than a risk.**
  Four widens ran during this cycle — the shim, the `feedback/` artifacts, the probe fixture, and the
  committed assertion suite — each returning `verdict: disjoint` with an empty `collisions` array.
  (The board state that made those verdicts meaningful is not recoverable from any artifact this
  report cites, so the verdicts are reported and the concurrency they were checked against is not.)
- **`verify-paths` immediately after opening the PR caught nothing, and that is the point.** Run at
  PR-open rather than at `done`, it confirmed `FSGG-PATHS OK` while a `Refs`-instead-of-`Closes` body
  would still have been free to fix.
- **Reflecting the coordination engine's own assemblies produced a valid protocol document on the
  first submission.** `FS.GG.Coord.ReviewWait.validate` named the one missing field
  (`evidenceRef is required`) and `encode` produced the exact wire form, so nothing was guessed into a
  digest-sealed append-only ledger.

## §3 What did not

- The mechanism the item body prescribed — recording `${HOME}/.dotnet` in the host `env` blocks —
  cannot work, and a full design pass was spent establishing that rather than assuming it (§4.2).
- The replacement mechanism then passed every synthesized-environment check while still failing in a
  real host session, because a host reverts PATH after the mechanism runs (§4.3). The rework was one
  design change plus one correctness fix to the shim's step 0.
- The branch cannot reach green CI on its own account (§4.6).
- Independent review returned `changes-required` with two material findings, and repairing them
  produced the cycle's sharpest self-correction: see §4.4. The evidence suite itself was the largest
  omission — it existed only in a scratchpad directory, so every green result reported during
  implementation was unreproducible at the reviewed head until it was committed as
  `scripts/test-agent-env.sh`.

No product code was changed and no scope was added beyond the four declared widens.

## §4 Findings

#### §4.1 A fresh agent session cannot reach the pinned SDK, and the failure names the pin instead of the lookup root

- **Kind:** friction
- **Impact:** Every `dotnet`-backed entry point in the workspace — `scripts/fsgg-coord` (so the
  identity mint, the claim, and every board read and write), `./build.sh`, `dotnet fsi`, and
  `dotnet fsgg-sdd` — fails on a fresh session until an agent repairs `PATH` by hand.
- **Expected:** A workspace that records its coordination board identity in host wiring also records
  the toolchain location that identity is useless without.
- **Observed:** `DOTNET_ROOT` and `PATH` point at `/usr/share/dotnet`, which carries `6.0.428` and
  `10.0.400`. The pinned `10.0.302` is installed at `$HOME/.dotnet/sdk/10.0.302`. The resulting error
  names the requested version and the `global.json` path, which reads as "the pin is wrong" or "an
  SDK is missing" — steering the reader toward editing `rollForward`, the one change that would cost
  every consumer a reproducibility guarantee.
- **Evidence:** file:global.json;file:scripts/test-agent-env.sh;issue:EHotwagner/S.I.R.#256
- **Version:** n/a
- **Owner:** EHotwagner/S.I.R., workspace host wiring (`.claude/settings.json`, `.codex/config.toml`)
- **Recurrence:** new; EHotwagner/S.I.R.#256
- **Avoidable cost:** As recorded in the issue's own Impact section: on the 2026-08-22 board pass a
  worker hit this and wrote its remediation note into a comment on an unrelated item, where only a
  worker claiming *that* item would ever find it; on the same day the board host hit it on its first
  board read and carried the workaround into every dispatch brief by hand. An earlier draft of this
  report asserted a session count the issue does not state.
- **Disposition:** product fix

#### §4.2 Neither agent host expands variables in its environment block, so home-relative wiring cannot be expressed there

- **Kind:** capability-gap
- **Impact:** A workspace can record a literal string in host wiring but not a path relative to the
  user's home, which is where per-user toolchains are installed. The obvious fix for §4.1 is
  unavailable, and the failure is silent — the literal string is delivered as a path that does not
  exist.
- **Expected:** An environment block that can express `${HOME}/.dotnet`, or that documents that it
  cannot.
- **Observed:** A probe project whose `.claude/settings.json` set an `env` value of `${HOME}/.dotnet`
  delivered the literal string `${HOME}/.dotnet` to the process; `$HOME/.dotnet` likewise. Codex's
  `[shell_environment_policy.set]` behaved identically. `execvp` does not expand `PATH` entries
  either, so a literal there is a directory that does not exist.
- **Evidence:** file:feedback/fixtures/item-256-host-environment-probes.md
- **Version:** Claude Code 2.1.240; Codex CLI 0.149.0 (`codex --version`). Not re-verified against
  later releases of either.
- **Owner:** Anthropic Claude Code (`settings.json` `env`); OpenAI Codex
  (`shell_environment_policy.set`)
- **Recurrence:** new
- **Avoidable cost:** One design pass discarded and replaced with `BASH_ENV`, which bash expands
  itself.
- **Disposition:** accepted

#### §4.3 Claude Code reverts PATH for every shell call after BASH_ENV has run

- **Kind:** defect
- **Impact:** Any session-level `PATH` repair applied at shell start is silently undone, while other
  exported variables survive — which is worse than being undone entirely, because the surviving
  variables make the environment look repaired. A fix can pass every reproduction of the environment
  and still fail in the environment.
- **Expected:** A shell call either honours the shell's startup environment or documents that it
  replaces part of it.
- **Observed:** Each shell call runs `source <shell-snapshot>; <command>`. The generated snapshot's
  final line is an `export PATH=` carrying the host process's own `PATH`, and it runs after
  `BASH_ENV`. `DOTNET_ROOT` set by the shim survived; `PATH` did not, and `DOTNET_ROOT` alone fixes
  nothing because the muxer resolves SDKs relative to its own location. The snapshot contains no
  `unset -f`, which is what makes a shell function survive it and is the basis of the workaround now
  in `scripts/agent-env.sh`.
- **Evidence:** file:feedback/fixtures/item-256-host-environment-probes.md
- **Version:** Claude Code 2.1.240. Not re-verified against later releases.
- **Owner:** Anthropic Claude Code, shell snapshot generation
- **Recurrence:** new
- **Avoidable cost:** One correctness fix to the shim's step 0 and one added mechanism (a
  self-erasing `dotnet` function), found only because the end-to-end check was run through the actual
  runtime rather than a reproduction of it.
- **Disposition:** accepted

#### §4.4 For an environment fix, only a stripped shell plus a negative control and a mutation distinguishes a real fix from a vacuous one

- **Kind:** positive-pattern
- **Impact:** This cycle's fix passed every check in its first harness and was still wrong. The
  practice that caught it is cheap and transfers to any change whose subject is the environment the
  tests themselves run in.
- **Expected:** n/a
- **Observed:** Three properties did the work. Running each check inside `env -i` with
  `bash --noprofile --norc` — this workspace's own `~/.bashrc` and `~/.bash_profile` both export the
  repair, so any check that lets a profile load passes for the wrong reason. A negative control
  asserting the same commands fail without the wiring. A mutation that deletes the mechanism and
  requires the wired case to go red, then restores it and requires green, which establishes that the
  wiring rather than the ambient environment is doing the work. A fourth property was added after
  §4.3: run the final check through the real host, not a reproduction of it.

  A fifth was learned the hard way during review repair, and it is the general lesson of this cycle:
  **a test cannot go red against a wrong sentence.** The checks written to close review finding M1
  were inverted against the pre-review shim expecting red, and they passed — the same vacuity they
  were meant to repair. The cause was that M1 was a defect in the documentation, not in the
  behaviour: the code always did what the corrected sentence now says. The checks were therefore
  relabelled in the suite as characterization checks rather than regression checks, with the reason
  written beside them, because green checks would otherwise imply a fix that was never made. The second audit went further
  and verified this by swapping in the pre-repair shim and re-running: **every one of the 36 checks
  passes against it**, so the suite as a whole does not discriminate the review repair at all. Only
  group H, the delete-the-mechanism mutation, discriminates anything — the existence of the wiring
  itself. A reader is entitled to know that before reading 36 green lines as proof of the repair.
- **Evidence:** file:scripts/test-agent-env.sh;file:docs/workspace-onboarding.md
- **Version:** n/a
- **Owner:** EHotwagner/S.I.R. worker evidence practice; FS-GG `independent-review` gate-inversion
  guidance
- **Recurrence:** new
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.5 The review gate landable enforces is documented in no packed skill, down to the required fields of its own wait event

- **Kind:** orchestration
- **Impact:** A worker following the packed contract writes prose HTML-comment markers and is then
  refused by `landable`, with no packed text naming the commands that produce what it actually wants.
- **Expected:** The packed `independent-review` contract describes the gate the engine enforces.
- **Observed:** `landable` requires a `fsgg.coord.review-decision/v2` record written through
  `fsgg-coord review record`, itself gated behind a durable entry written through
  `fsgg-coord review wait`; neither command appears in any packed skill. `review` returned
  `verdict: noVerdict` with `dispatchCritic requires a durable review-wait entry before dispatch`. The
  wait event's own required fields are stated nowhere: `ReviewWait.validate` returned
  `Error ["evidenceRef is required"]` for an otherwise complete receipt.
- **Evidence:** issue:EHotwagner/S.I.R.#255
- **Version:** `fs.gg.coord.cli` 0.71.0
- **Owner:** FS-GG `.github` coordination engine, and the packed `independent-review` skill mirrored
  into `.claude/skills/`, `.agents/skills/` and `.codex/skills/`
- **Recurrence:** seen again EHotwagner/S.I.R.#255, which is open and owns this cause; also carried
  by a prior cycle report, `feedback/2026-08-22-sir-item-252-audit-binding-ledger-currency.md` §6 and
  §9, which attribute the same contract/engine disagreement to the same issue
- **Avoidable cost:** One reflection pass over `FS.GG.Coord.Core.dll` to recover the `WaitReceipt`
  shape rather than guess a value in a digest-sealed append-only ledger.
- **Disposition:** existing issue

#### §4.6 The integrity gate fails this PR for a cause outside its diff, and only a conservative route makes that visible

- **Kind:** defect
- **Impact:** A branch that changes four workspace-wiring files cannot reach green CI, and the
  failing subject is one it cannot repair. The same failure is invisible on branches whose route does
  not select the integrity subject.
- **Expected:** A gate fails for a property of the change under test.
- **Observed:** `integrity` failed with
  `feedback-tool: invalidation: exception replacement evidence is stale for file:.github/workflows/ci.yml`
  and the same for `file:scripts/test-ci-route.mjs`. This branch touches neither file, nor
  `scripts/audit-binding-exceptions.json`: its complete changed-path set is the four declared paths.
  The gate runs here because `scripts/ci-route.mjs` selects the full gate set for this change. The
  per-path rules are `RP-004-cross-cutting` for `scripts/agent-env.sh` and
  `RP-005-unknown-conservative` for `.claude/settings.json` and `.codex/config.toml`, but the
  four-path SET resolves through **`RP-006-mixed-conservative`**, because two kinds are present
  (cross-cutting plus documentation). An earlier draft named only the per-path rules.
- **Evidence:** file:scripts/ci-route.mjs;issue:EHotwagner/S.I.R.#252
- **Version:** `fs.gg.coord.cli` 0.71.0; observed at `a7f05776d1a80f39a018ee8b4056bc475de445dc`,
  before the repair landed
- **Owner:** EHotwagner/S.I.R., feedback-audit exception binding and the path-conditional integrity
  subject
- **Recurrence:** seen again EHotwagner/S.I.R.#252, which is now CLOSED; repair merged as PR #254
- **Avoidable cost:** Blocked this PR's merge until #254 landed and this branch rebased.
- **Resolved:** PR #254 merged as `11cfc87adbe410b561e4ecb8592c9b09241edcea`, re-sealing the two
  stale digests. Re-verified here after rebasing onto it: `./scripts/test-feedback-audit-binding-exceptions.sh`
  exits 0, where at the pre-rebase head it emitted both `invalidation` lines.
- **Disposition:** existing issue

#### §4.7 Board policy reserves slots for "independent critics" without defining whether a feedback auditor is one

- **Kind:** orchestration
- **Impact:** A lane cannot tell from inside its own worktree whether dispatching a feedback-report
  actionability auditor consumes one of the wave's reserved critic slots. Two agents were live in
  this lane at once — a PR review critic and an actionability auditor — and whether that was one
  reservation or two was unresolvable from any written source.
- **Expected:** The wave policy that reserves slots for "independent critics" says whether the
  feedback contract's actionability auditor is one.
- **Observed:** Neither the packed contract nor the board policy addresses it either way. The absence
  is the finding; the host ruled for this pass that an actionability auditor does **not** consume a
  reserved review slot, and ruled it with no documentary basis in either direction.
- **Evidence:** issue:EHotwagner/S.I.R.#261
- **Version:** `fs.gg.coord.cli` 0.71.0
- **Owner:** FS-GG board wave policy and the packed `pnext-item` / `independent-review` contracts
- **Recurrence:** seen again EHotwagner/S.I.R.#261. An earlier draft called this `new` and described
  #261 as carrying only an adjacent cause; the second audit read #261's Scope section and found it
  owns this exact question in its own words, so `new` was not sustainable.
- **Avoidable cost:** none this pass, because the host ruled promptly; the cost is latent and lands
  on whichever lane guesses differently.
- **Disposition:** issue — recorded as the host's ruling **for this pass**, one data point rather
  than a settled rule

#### §4.8 whoami --mint does not persist, so the documented idiom silently loses a worker's identity

- **Kind:** defect
- **Impact:** A worker following the documented `eval "$(scripts/fsgg-coord whoami --mint)"` idiom in
  a harness that gives each command a fresh shell loses the minted id after that one command. Re-
  minting yields a *different* id, and a worker that re-mints abandons its own claim.
- **Expected:** The documented identity idiom establishes an identity that survives for the item.
- **Observed:** `whoami --mint` only prints an `export` line for the current shell. This cycle
  avoided the failure only because the dispatch brief warned about it in prose; nothing in the packed
  contract does.
- **Evidence:** issue:EHotwagner/S.I.R.#266
- **Version:** `fs.gg.coord.cli` 0.71.0
- **Owner:** FS-GG `.github` coordination engine, and the packed `pnext-item` identity step
- **Recurrence:** seen again EHotwagner/S.I.R.#266; that issue is open and owns this cause
- **Avoidable cost:** none here, because the brief pre-empted it; unbounded for a worker without that
  brief, which is the condition the issue describes.
- **Disposition:** existing issue

## §5 Did not exercise

- No product runtime, simulation, rendering, or playtest surface was touched; the change is host
  wiring and documentation only.
- `./build.sh` was exercised only as far as its first step resolves. `dotnet tool restore` under the
  wiring is asserted by `scripts/test-agent-env.sh` (groups B and D); the full build was not run,
  because the item's acceptance criterion is that the entry point resolves and the branch changes
  nothing the build compiles.
- Packaging and upgrade surfaces were not exercised.

## §6 Doc-versus-behavior contradictions

The packed `independent-review` contract says the review gate is satisfied by prose HTML-comment
markers (`fsgg:independent-review:v1`, `fsgg:review-accepted:v1`). The engine's `landable` requires a
`fsgg.coord.review-decision/v2` record produced by `fsgg-coord review record` behind a
`fsgg-coord review wait` entry. Both statements are current and they disagree. Owner: the packed
`independent-review` skill in all three mirrors, tracked as `EHotwagner/S.I.R.#255`.

## §7 Workarounds still in the tree

- `scripts/agent-env.sh` carries a self-erasing `dotnet` shell function whose only reason to exist is
  §4.3. Removal condition: the host stops re-exporting a captured `PATH` after `BASH_ENV`, at which
  point the plain `PATH` prepend suffices and the function and the step-0 re-heal can both go. Risk of
  permanence: a shell function that deletes itself is hard to debug from the outside, which is why the
  file names the behaviour it absorbs and `docs/workspace-onboarding.md` documents the one place it
  does not reach.
- No other workaround was introduced. `global.json` is unchanged.

## §8 Friction and avoidable cost

- One discarded design pass (§4.2) and one design correction plus a shim fix (§4.3).
- Zero worker restarts, zero lifecycle reruns, zero generated files replaced.
- One avoidable checkpoint retry: `feedback-tool checkpoint` rejected a call missing `--surface`,
  which is correct behaviour and cost one command.
- One review round (round 0, the initial review) spent on two material findings, plus one discarded
  repair theory when the inversion of the new checks passed instead of failing.
- The check that exposed the uncommitted-harness gap, `grep -rn "unexpected outcome" scripts/`, was
  available for some hours before an auditor ran it, and the reported number was accepted meanwhile.
  The elapsed figure and the two sibling lanes below are **reported by the board host, not measured
  by this cycle**, and no locator is offered for them. The generalisable half is that a count reported in prose is not evidence
  and costs nothing to verify — two other lanes found the same gap in their own work once asked, and
  one correctly declined to commit its automation because its gate was self-sufficient in-tree. The
  distinguishing question is whether the harness *is* the gate; here it was.
- Command duration is dominated by end-to-end host probes: each real-host verification round and each
  full harness run costs minutes, not seconds. That cost is the price of §4.4 and was worth it.

## §9 Skill value and gaps

- `pnext-item` — invoked, and drove the whole cycle: identity, claim, isolate, implement, route
  findings, independent critique, merge and stamp.
- `update-config` — invoked before touching `.claude/settings.json`. It supplied the settings schema,
  in which `env` is typed as a plain string map with no expansion documented. That is an absence
  rather than a statement, so it could not settle whether expansion happens at runtime; that had to
  be measured (§4.2). No locator is offered for the skill text itself, which is loaded into a session
  rather than committed here.
- `fs-gg-feedback-report` — invoked; produced this report and the checkpoint file.
- `fs-gg-sdd-*` — not invoked, correctly: the delivery-route receipt is `lightweight`.
- `intra-repo-parallel-work` — followed through `pnext-item` (mint, claim, worktree, `widen`,
  heartbeat) rather than read directly.
- The `performance-first` planning gate was not run: this item is workspace wiring, not interactive or
  simulation work. The one performance-relevant property, per-shell cost, is bounded in the shim and
  asserted in the harness.
- Gap: no packed skill documents the review gate the engine enforces (§4.5).

## §10 Outcome markers

- Time from claim receipt to PR open: **22 minutes 32 seconds**, measured, not estimated — claim
  marker comment `5382729642` created `2026-08-22T21:37:27Z`, PR #260 created `2026-08-22T21:59:59Z`.
  An earlier draft said "approximately 75 minutes (estimate)", wrong by 3.3x against two artifacts
  §1 already named; the hedge did not cover an error that size.
- First green fresh-session check: after the first design, and it was misleading — see §4.3.
- First green real-host check: after the design correction; both hosts then resolved `10.0.302` end to
  end.
- Checks: **36** in `scripts/test-agent-env.sh`, `0 unexpected outcome(s)`. An earlier draft of this
  report said 33; the suite has 36 `run` invocations and prints 36 `OK` lines. An earlier draft reported 28
  checks from a harness that existed only in a scratchpad directory and was therefore unrunnable at
  the reviewed head — a number in a message, not evidence.
- Ship readiness: the CI blocker cleared when PR #254 landed. At the pre-rebase head **two** checks
  were red, `integrity` and `pr-verdict`, the second downstream of the first, which consumes its
  receipt — one cause and two symptoms (§4.6). After rebasing onto `11cfc87…` the local gate passes.
- Merge: not reached within this report's boundary.

## §11 Falsifiable improvements

- **Classify `.claude/` and `.codex/` explicitly in `scripts/ci-route.mjs`.** Both currently reach
  cross-cutting through `RP-005-unknown-conservative`, the catch-all — the conservative answer, but
  arrived at by not recognising the path rather than by deciding about it. Owner: EHotwagner/S.I.R.,
  `scripts/ci-route.mjs`. Acceptance: a changed-path set consisting only of host wiring classifies
  under a named rule, and `scripts/test-ci-route.mjs` asserts the rule id. Prevents: a future reader
  cannot tell whether the full gate set on a settings-only change was intended or accidental.
- **Make `scripts/fsgg-coord landable`'s refusal name its producing command.** Owner: FS-GG `.github`
  coordination engine. Acceptance: the refusal text for a missing review decision contains
  `fsgg-coord review record`, and for a missing wait entry contains `fsgg-coord review wait`. Prevents
  §4.5's recovery cost. This half is not in this repository and belongs in a cross-repo request
  against the Kit; §4.5 is already carried by `EHotwagner/S.I.R.#255`.
- **Have `feedback-tool checkpoint` report every missing required argument at once.** Owner: FS-GG
  `fs-gg-feedback-report`. Acceptance: a call missing two required arguments names both. Prevents the
  retry recorded in §8.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | No scaffold or template provider was involved; the item modifies an existing workspace. |
| onboarding-guidance | exercised | The item's entire subject. A fresh session could not reach the pinned SDK; the resolution order is now documented in `docs/workspace-onboarding.md` and delivered by both host files. §4.1, §4.2. |
| skills | exercised | `pnext-item`, `update-config`, `fs-gg-feedback-report` invoked; the SDD skills correctly not invoked on a `lightweight` route; one documented gap at §4.5. |
| sdd-authoring | not-exercised | Delivery-route receipt is `lightweight` with `sddPackageReady: true`, so no lifecycle stage was owed or authored. |
| implementation-apis | not-exercised | No product API was called or changed. |
| dependencies-build | partial | `dotnet tool restore` and the `./build.sh` entry point were exercised as resolution targets; the full build was not run. |
| testing | exercised | `scripts/test-agent-env.sh`, committed: 36 checks with a negative control, degradation cases, a host-clobber case, invocation-shape and `command -v` characterization, idempotence, and a delete-the-mechanism mutation. §4.4. |
| evidence | exercised | The harness is the item's evidence; §4.6 is a defect in the evidence-gate binding itself, deduped to #252. |
| runtime-playtest | not-exercised | No runtime behaviour was changed. |
| performance | partial | Only the wiring's own per-shell cost: the `git rev-parse` is bounded by rewriting `BASH_ENV` to a literal, asserted in the harness. No product performance work. |
| documentation | exercised | `docs/workspace-onboarding.md` authored, deliberately without front matter so `scripts/generate-in-app-docs.mjs` excludes it from the published bundle and its page budget. |
| packaging-upgrade | not-exercised | No package version, lock, or manifest was touched. |
| worker-git-pr | exercised | Mint, claim, four `widen`s returning `disjoint`, isolated worktree, PR #260 with a bare `Closes #256`, `verify-paths` OK, `review wait` entry, fresh critic dispatched. §4.5, §4.6. |
