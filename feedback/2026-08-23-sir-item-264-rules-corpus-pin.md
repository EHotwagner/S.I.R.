---
feedbackSchema: 2
date: 2026-08-23
workspace: agent-a262fbe43be42b495
cycle: item-264-rules-corpus-pin
lane: none
toolVersion: 1.0.1
commit: aa0908144a924242e53f14a69df1acc041e6ff3f
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 11
- **zero-event reason:** not applicable

This report covers one board item worker lane: EHotwagner/S.I.R.#264, claimed by minted worker `swift-10be` through `fsgg-coord take` and carried from claim to review handoff on PR #276. The stable checkpoint file is `feedback/checkpoints/item-264-rules-corpus-pin.jsonl` with eleven events.

Pinned inputs exercised: .NET SDK 10.0.302 (`global.json`, `rollForward: disable`), `fs.gg.coord.cli` 0.71.0, `fsgg-sdd` 1.0.1, bash 5.3.15.

The SDD lifecycle was **not used** for this item: the delivery route receipt records `route: lightweight` with `sddWorkId: null` and `specHome: null`, so no `fsgg-sdd` stage ran. The item is a gate and tooling repair with no product behaviour, domain model, or rule semantics change.

Confidence limits: the `#239` regression window is dated from commit history and the pre-`#239` pin's ancestry, not from a CI record of the intervening period. The window is **two days**: `d76b477` is authored 2026-08-20T23:27:04+02:00 and this cycle ran 2026-08-23. The claim that no pull request changed a pinned source's substance in that window is inferred from the absence of such a change on `main`, which does not exclude a PR that was opened and abandoned.

## §2 What worked

**The committed feedback corpus prevented a wrong fix.** This is the single highest-value event of the cycle. The item was dispatched with a plausible, well-evidenced Phase 1 — narrow the pinned source set to the paths that host rule symbols — which would have unblocked six board rows immediately. `feedback/2026-08-12-sir-item-194-executable-rules-corpus.md` §4.6 records that byte-exact current-versus-pin correspondence was added to this gate *because* a confirmation critic proved a changed `App.fs` could otherwise pass. Narrowing would have re-opened by hand a hole a critic had already closed. Nothing in the architecture doc, the ADR, any work package, or either rules skill carried that fact; only the retained per-cycle report did.

**Deriving coordination documents with the engine's own functions.** `ReviewWait.generationToken`, `ReviewWait.validate` and `ReviewWait.encode` from `FS.GG.Coord.Core.dll`, driven through `dotnet fsi`, produced a wait document accepted on its first structurally valid attempt, and `validate` reported a missing required field locally with no board write.

**Probing a coupling question by measurement rather than reasoning.** Adding a field shaped like the intended change and running *both* candidate dependency cones produced a split answer — identity cone clean, governance cone coupled — that would not have been reached by inspection, because the governance coupling is a whole-file `SHA256.HashData` that no amount of care about field placement would reveal.

## §3 What did not

**One full design cycle was built and reverted.** A two-set membership redesign of `implementation-sources.json` plus `verify-rules-corpus.sh` was written, then abandoned when the intent investigation contradicted its premise. The rework was caused by the issue's Root cause section, which stated the freeze was inherent to the pin design and latent since the pin was created. Both claims are false.

**A second, smaller design correction.** The first structural finding — that narrowing the pinned set forces a re-seal of the implementation digest in `CombatRules.fs` — was correct about the mechanism but wrong about the conclusion, because it assumed narrowing was the goal. Measuring that the sealed digest derives only from blobs at `sourceCommit` showed the working-tree comparison to be separable, which became the basis of the design that shipped.

**A self-inflicted diff-noise defect.** Updating one digest in `evidence-bindings.json` with `jq` reformatted a single-line JSON document into a pretty-printed one, turning a one-value change into a whole-file rewrite in the diffstat. Caught before commit and repaired with a byte-level `sed` replacement, so the final diff is `1 insertion(+), 1 deletion(-)`. *No artifact survives the repair; the only checkable residue is that the committed change is one line.*

**Two concurrency mistakes against my own worktree.** A background gate run and a foreground `git checkout` of the same file overlapped, producing a spurious `blocked=true` governance failure; and a writer probe was run while an end-to-end demonstration held the tree mutated. Neither reached a commit, and both were re-measured cleanly afterwards. *Self-reported: these happened in transient background jobs and left no committed artifact, so they are not independently checkable.*

## §4 Findings

#### §4.1 A gate can be made unsatisfiable as a side effect of hardening it

- **Kind:** quality-gap
- **Impact:** Six board rows blocked — `#249` and `#232` directly, `#230`/`#233`/`#234`/`#235`/`#236` behind `#232`. The `rules` sub-gate of `domain-conformance` refused any change to any of 19 implementation sources, in either direction, with no path by which such a change could pass.
- **Expected:** A gate that adds a precondition remains satisfiable by some legal input.
- **Observed:** `d76b477` ("Make rules corpus source identity durable", work item `#239`) added `require_durable_source_commit`, correctly fixing source-link durability. The same field was also the baseline the working tree was compared against, and that duty requires naming text not yet on the default branch. Ancestry made it unsatisfiable. Work item `#239` shipped four inversions for its new refusals — three in `d76b477`, the fourth in `d1f6ea7`, both on PR #240 — and all four still pass. Every one proves a bad input is refused. None demonstrates that the operation the precondition constrains, rebinding the pin so a changed source can pass, has any legal execution. (The production call at `scripts/verify-rules-corpus.sh:74` does exercise a valid input for that one function, but only because nothing had changed; it says nothing about whether a rebind is reachable.) `require_durable_source_commit` and all four of its inversions are retained unchanged by this cycle's repair. The defect went undetected for two days (`d76b477` authored 2026-08-20T23:27:04+02:00; this cycle 2026-08-23) because exercising it requires changing a pinned source, and no pull request on `origin/main` had — established by walking `git log d76b477..origin/main` over the 19 declared paths, which is a `main`-only bound and does not exclude an abandoned branch.
- **Evidence:** command:git log --oneline -S require_durable_source_commit -- scripts/verify-rules-corpus.sh; command:git merge-base --is-ancestor 902ac733 origin/main; file:scripts/verify-rules-corpus.sh; file:work/239-durable-rules-identity/spec.md
- **Version:** `fs.gg.coord.cli` 0.71.0; defect is in product scripts, not the engine
- **Owner:** S.I.R. / `scripts/verify-rules-corpus.sh`, and generally any gate that adds a precondition
- **Recurrence:** new as a named class; same family as `#258` and `#252` (a check whose passing condition is unsatisfiable by construction, invisible because nothing selected it); EHotwagner/S.I.R.#264
- **Avoidable cost:** Six blocked rows and two parked workers.
- **Disposition:** product fix

The generalisation worth carrying: **a gate can be made unsatisfiable as a side effect of hardening it, and the hardening's own inversions will not catch it, because they prove the refusals fire — never that the satisfying path is still reachable.** The rule that follows: *a gate that adds a precondition must carry an inversion showing a legal input can still satisfy it.* This item's own suite carries one.

#### §4.2 The issue's root-cause section was wrong, and the wrong diagnosis was actionable

- **Kind:** defect
- **Impact:** One worker built and reverted a complete design. The stated cause pointed at the pin's structure; the real cause was a commit two days old.
- **Expected:** A board row's Root cause section names a cause that survives reading the authoring history.
- **Observed:** The row stated the circularity was inherent and "latent since the pin was created". The ancestry precondition was introduced two days earlier by `d76b477`, and the pin immediately preceding it (`902ac733`) was a feature-branch head — proving the intended flow rebound to the pull request's own candidate, exactly as `work/181-physical-combat-slice/plan.md:42` instructs.
- **Evidence:** issue:EHotwagner/S.I.R.#264; command:git cat-file -e 902ac733^{commit}; command:git merge-base --is-ancestor 902ac733 origin/main; file:work/181-physical-combat-slice/plan.md
- **Version:** n/a
- **Owner:** S.I.R. / board triage
- **Recurrence:** new; corrected in place by the host with a `ROW RE-CUT` comment
- **Avoidable cost:** One design cycle written and reverted.
- **Disposition:** existing issue

#### §4.3 No document states a membership criterion for the pinned source list

- **Kind:** capability-gap
- **Impact:** The absence of a stated rule is what made narrowing the set look defensible. Membership had to be reconstructed from commit archaeology and feedback prose.
- **Expected:** A durable-identity artifact states why a path is on its list.
- **Observed:** *As of the pre-change commit `11cfc87adbe410b561e4ecb8592c9b09241edcea`, the base of this cycle's branch* — silent in the architecture doc, the single ADR, the rules-manifest contract, every `work/` package, both rules skills, and every CI workflow. At this report's `commit:` the architecture doc and `implementation-sources.json` DO state a criterion, because **this cycle added it** — so those two files are deliberately NOT cited below; citing them would be citing the fix as evidence of the defect. The two locators below are the artifacts this cycle did not touch, and both are silent in the reviewed tree as well as the pre-change one. The pre-change state of the changed fixture is cited directly below at `11cfc87`, and the skills / CI / `work/` classes are routed by a repository-wide grep whose only hits are `work/239-durable-rules-identity/{plan.md,evidence.yml}`, neither of which states a criterion. `work/239-durable-rules-identity/spec.md` calls them "the intended normalized implementation sources" without saying what makes them intended. The list is hand-maintained; nothing in the repository writes it.
- **Evidence:** file:docs/adr-0001-executable-rules-corpus.md; file:work/239-durable-rules-identity/spec.md; command:grep -rl implementation-sources .claude/skills/ .github/ work/; command:git show 11cfc87adbe410b561e4ecb8592c9b09241edcea:tests/fixtures/rules-corpus/v2/implementation-sources.json
- **Version:** n/a
- **Owner:** S.I.R. / `docs/executable-rules-corpus-architecture.md`
- **Recurrence:** new
- **Avoidable cost:** An intent investigation that had to be commissioned before any code could be written.
- **Disposition:** doc fix — addressed in this cycle. *The independent critic first graded this finding `incomplete` because its original evidence line was self-refuting: it cited the very files that state a membership rule as proof that none does, because this cycle added that rule. The line has been rebuilt from artifacts this cycle did not touch, plus the pre-change fixture at `11cfc87` and a repository-wide grep, all of which the critic checked.*

#### §4.4 A durable-identity artifact had no writer

- **Kind:** capability-gap
- **Impact:** The rebind workflow the design depends on was entirely manual, which is how the pinned list drifted into ad-hoc curation with no stated rule and an internally inconsistent treatment of `.fsi` companions.
- **Expected:** An artifact a gate requires to be exact has a tool that writes it exactly.
- **Observed:** Nothing generated `implementation-sources.json`; the sealed implementation digest is a hand-maintained 64-hex literal in `CombatRules.fs` that only the verifier ever recomputes, and it only asserts.
- **Evidence:** file:scripts/verify-rules-corpus.sh; file:src/SIR.Simulation/CombatRules.fs; file:scripts/generate-rules-corpus.sh; file:tests/fixtures/rules-corpus/v2/implementation-sources.json; file:feedback/2026-08-15-SIR-186-6.md; command:find src -name '*.fsi'
- **Version:** n/a
- **Owner:** S.I.R. / `scripts`
- **Recurrence:** new
- **Avoidable cost:** Every prior rebind was hand-edited; one such rebind is recorded as friction in `feedback/2026-08-15-SIR-186-6.md`.
- **Disposition:** issue — **partially addressed, and the remaining half is open.** This cycle added `scripts/rebind-rules-corpus-sources.sh`, which writes the *mutable* half (`source-correspondence.json`). `implementation-sources.json` and the sealed 64-hex digest in `CombatRules.fs` still have no writer, and the new tool deliberately refuses to add or remove a path from that list. The capability gap this finding names is therefore narrowed, not closed.

#### §4.5 The review-wait `claimGeneration` field is the claim marker's comment id, and no reachable documentation says so

- **Kind:** friction
- **Impact:** One refused board round trip, plus a reflection pass over the whole engine assembly that found no producer for the value.
- **Expected:** A refusal names the field's source, or the value is derivable from a published engine function.
- **Observed:** The claim marker comment carries both a comment id and a `renewed=` numeral; the receipt wants the comment id. Supplying the `renewed=` numeral carried in that same marker produced `refused: receipt claimGeneration is not current`; supplying the comment id `5383131140` was accepted. The `renewed=` value is rotated by lease renewal and the specific numeral is not recoverable, so it is deliberately not quoted here. The refusal names neither, and no function in `FS.GG.Coord.Core.dll` produces it — it appears only as a `WaitReceipt` field, a `project` parameter, and serialization plumbing. The assembly does document the *sibling* concept on `OpKey.compose` ("`generation` is the comment id of the winning `fsgg:claim` marker"), so the fact is not wholly unwritten — but it is attached to a different type and reachable only by decompiling.
- **Evidence:** command:scripts/fsgg-coord review wait S.I.R.#264 event.json --pr 276; issue:EHotwagner/S.I.R.#255
- **Version:** `fs.gg.coord.cli` 0.71.0; 0.72.0 present in the local package cache and not re-verified against it
- **Owner:** FS-GG Coordination Kit / `review wait` refusal text
- **Recurrence:** new evidence on an existing issue; EHotwagner/S.I.R.#255
- **Avoidable cost:** One refused round trip and roughly one reflection cycle.
- **Disposition:** existing issue

#### §4.6 An unauthorized review-wait round is durably appended before it is refused

- **Kind:** friction
- **Impact:** An invalid entry was written to a digest-sealed append-only ledger, then had to be cancelled — three comments where one was needed.
- **Expected:** An append that the protocol will not authorize is refused at append time.
- **Observed:** An initial review wait entered at round 1 was accepted by `review wait` (commentId 5383246998) and only afterwards refused by `review`, which requires round 0: `expected generation ...:initial-review:0, got ...:initial-review:1`.
- **Evidence:** command:scripts/fsgg-coord review S.I.R.#264 --pr 276; command:scripts/fsgg-coord review wait S.I.R.#264 enter-round-1.json --pr 276
- **Version:** `fs.gg.coord.cli` 0.71.0
- **Owner:** FS-GG Coordination Kit / `review wait` round validation
- **Recurrence:** new evidence on an existing issue; EHotwagner/S.I.R.#255
- **Avoidable cost:** One wasted ledger entry plus a cancel event.
- **Disposition:** existing issue

#### §4.7 Retained per-cycle feedback reports carried decision-critical intent no other artifact held

- **Kind:** positive-pattern
- **Impact:** Prevented a change that would have re-opened a defect an independent critic had closed, on an item whose dispatch, route receipt, and issue body all pointed the other way.
- **Expected:** n/a
- **Observed:** The only record that byte-exact correspondence was a *deliberate fix* rather than an over-broad accident lives in `feedback/2026-08-12-sir-item-194-executable-rules-corpus.md` §4.6, with corroboration in `feedback/2026-08-15-SIR-186-6.md` describing the gate's refusal as "correctly rejected".
- **Evidence:** file:feedback/2026-08-12-sir-item-194-executable-rules-corpus.md; file:feedback/2026-08-15-SIR-186-6.md
- **Version:** n/a
- **Owner:** FS-GG / `fs-gg-feedback-report`
- **Recurrence:** new
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.8 Engine-derived coordination documents were accepted first time; hand-written ones were not attempted

- **Kind:** positive-pattern
- **Impact:** The review-wait document was structurally correct on its first attempt, and the one missing required field was caught locally with no board write.
- **Expected:** n/a
- **Observed:** `ReviewWait.validate` reported `claimGeneration is required` before any network call; `ReviewWait.generationToken` produced the generation token; `ReviewWait.encode` produced the document body, which needed only the posting marker stripped to become the bare JSON the command reads.
- **Evidence:** command:dotnet fsi with `#r` on ~/.nuget/packages/fs.gg.coord.cli/0.71.0/tools/net10.0/any/FS.GG.Coord.Core.dll, calling ReviewWait.generationToken head kind round, then ReviewWait.validate transition, then ReviewWait.encode transition; issue:EHotwagner/S.I.R.#255
- **Version:** `fs.gg.coord.cli` 0.71.0
- **Owner:** FS-GG Coordination Kit / worth documenting as the supported authoring route
- **Recurrence:** new evidence on an existing issue; EHotwagner/S.I.R.#255
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.9 Acceptance evidence that confirms the thesis is not adversarial evidence

- **Kind:** quality-gap
- **Impact:** Two material findings on one pull request (M1, M2), both reached by mutations the author had not tried, costing one repair round.
- **Expected:** Acceptance evidence for a new mechanism uses a mutation the *sibling* checks cannot already catch, so that it measures the mechanism under test rather than its neighbour.
- **Observed:** Three instances in this lane, all the author's own and all checkable.
  **(1)** The acceptance demonstration for the rebindable pin mutated a declared rule *fact*. A declared fact is part of the canonical manifest payload, so `generate-rules-corpus.sh --check` already refused it — the demonstration measured the sibling check. An independent critic mutated an algorithm *body* instead (`CombatRules.resolveCoverImpact`) and the `rules` gate passed while registered rule behaviour had moved (M1). The reason the class is invisible to that gate is narrow and checkable: 10 of the 16 corpus rules are `transition` kind, whose recorded `semantics` is invariant under an arithmetic change, so nothing *the `rules` gate consults* observes it. The conformance fixtures did observe it, which is what the repair now runs.
  **(2)** A CI failure was diagnosed as a `--failed` re-run measuring across attempts — a story that fitted this item's own defect class — without checking `run_attempt`, the single fact that falsifies it. `route` had re-run.
  **(3)** The critic's verdict marker was acted on without opening the critic's review. The sealed decision comment carries the verdict; the findings are in a separate comment posted three minutes later containing **two** findings. M1 was reproduced and repaired and the round reported as complete while M2 was neither read nor repaired — and the first M1 repair replaced one fixed build project with another, so it would not have fixed M2 either.
  **(4)** After writing this finding, a non-vacuity guard was added *to prove the new conformance check could fail* — and it could not fail, because it asserted only an exit code from an arm that aborts whether or not a divergence is found. It sat twelve lines above a correct sibling that asserts the diagnostic. **(5)** The build guard from instance (3)'s repair was itself fixed by adopting a second naming convention rather than by reading the build graph, and was wrong for three of the four paths probed. Instances (4) and (5) are the strongest evidence for this finding precisely because they postdate it: stating the rule did not prevent the author from breaking it twice more.
  *Not counted:* the host self-reported a similar inference from green gate receipts to a sound diff. It appears nowhere on #264 or PR #276 and has no artifact, so it is excluded rather than relabelled — counting it would have been counting hearsay to reach a number.
- **Evidence:** issue:EHotwagner/S.I.R.#264; command:gh api repos/EHotwagner/S.I.R./issues/276/comments --jq '.[] | select(.id==5383452179) | .body'; command:gh run view 32607930272 --repo EHotwagner/S.I.R. --json attempt --jq .attempt; command:git show --stat bf2b93afed5291a664baec0c36d60cb335660abf
- **Version:** n/a
- **Owner:** S.I.R. / gate-authoring convention (author-side); FS-GG / `independent-review` (critic-side)
- **Recurrence:** new
- **Avoidable cost:** One repair round, one refuted diagnosis relayed to the host, and one review round in which half the verdict went unread.
- **Disposition:** skill fix — acceptance conditions in §11.5 and §11.6

**No causal claim is made about mastery.** An earlier draft of this finding asserted that a defect class just mastered becomes the hypothesis that stops being tested. Instance (1) does not support it: nothing had been characterised when that mutation was chosen, and ordinary insufficient adversariality explains it. That framing was supplied by the host, adopted without the scrutiny a peer's claim would have received, and removed after an independent actionability critic judged the finding to convert one critic-caught defect into a general insight authored by the person who caused it. **The generalisation was accepted because it was flattering** — which is the property that makes a framing go unaudited, whoever offers it.

The repair for M1 was validated by *attacking* it: the correspondence file was hand-edited to bypass the new writer guard, and the gate refused anyway. That distinction — **defence in depth versus defence in theatre** — is whether the protection survives the author's cooperation being withdrawn, and the writer's check is documented in-file as explicitly *not* the guard so a later reader does not stop at it. *The hand-edit bypass is self-reported: the guard's design is checkable in the committed script, but no artifact records that particular run.*

A second rule fell out of the same repair: **a gate that owns an identity must be able to fail on its own.** Coverage that depends on a sibling gate being selected into the same CI route is coverage the gate does not control — the route classifier does. The conformance run was kept in the `rules` gate even though `cross-runtime` already performs it, for that reason.


## §5 Did not exercise

- The SDD lifecycle: the route receipt records `sddWorkId: null`, so no `fsgg-sdd` stage ran.
- Packaging and upgrade surfaces: no package version moved.
- Runtime playtest: no product behaviour changed, so no gameplay path was driven.
- The `--rebind` writer's `dotnet build` refusal path was reasoned about and implemented but not driven to a red result, because producing a non-building simulation tree solely to observe the refusal was judged disproportionate. This is a known gap in an otherwise inversion-covered change.

## §6 Doc-versus-behavior contradictions

`docs/executable-rules-corpus-architecture.md` documents `SourceCommit` only as the target of published source links — *"a repository source link pinned to `SourceCommit`"* — and says nothing about a working-tree comparison. `scripts/verify-rules-corpus.sh` additionally required every declared source's current text to be byte-identical to that commit. The document described one duty; the implementation carried two, and the second was the one that became unsatisfiable. Owning documentation: `docs/executable-rules-corpus-architecture.md`, corrected in this cycle.

## §7 Workarounds still in the tree

`scripts/rebind-rules-corpus-sources.sh` carries its own copy of `normalize_implementation_source`, duplicated from `scripts/verify-rules-corpus.sh`, because the verifier is not sourceable as a library. Removal condition: extract the normalizer into a shared shell library both can source. Risk if permanent: the two copies diverge and the writer records digests the gate cannot reproduce. Mitigated, not removed — inversion 8 in the gate asserts the writer finds nothing to rebind on a tree the gate considers current, so **a divergence observable on the current tree** fails CI rather than landing silently. A divergence in a branch the current tree does not exercise — a new special case added to one copy for a path whose text is identical either way — would still pass inversion 8. That is the residual risk, and it is why the removal condition is extraction rather than vigilance.

## §8 Friction and avoidable cost

- One complete design cycle written and reverted (`implementation-sources.json` plus `verify-rules-corpus.sh`), caused by §4.2.
- One commissioned intent investigation before any code could be written, caused by §4.3.
- Two refused coordination round trips plus one cancel event on the review ledger, caused by §4.5 and §4.6.
- One diff-noise repair: a `jq` rewrite reformatted a single-line JSON receipt into a pretty-printed whole-file change; repaired before commit to a one-line diff. Not independently checkable — the repair preceded the commit.
- Two self-inflicted concurrency errors against the worktree, producing one spurious governance failure and one spurious writer drift report. Both were re-measured rather than believed. Self-reported; no committed artifact.
- **Four** `widen` invocations beyond the originally declared touch-set, each returning `disjoint` with no collisions: 3 declared paths → 4 → 10 → 11 → 13. *Self-reported*: `widen` updates the claim's path list rather than writing a distinct board artifact per call, so the board records two `fsgg:route-decision/v2` revisions (3 paths, then 10), not four marks. **The two tens are different sets of ten and should not be read as agreement**: the receipt's `touchSet` includes `src/SIR.Simulation/CombatRules.fs` and excludes the checkpoint file, while PR #276's ten changed files include the checkpoint file and exclude `CombatRules.fs`, which is declared-and-untouched. The final 13-path claim adds the two feedback artifacts on top.

## §9 Skill value and gaps

- `pnext-item` — invoked; supplied the worker state machine, the widen-before-touch rule, and the gate-inversion evidence requirement that shaped §4.1's suite.
- `fs-gg-feedback-report` — invoked; eight checkpoints and this report.
- `sir-check-rule-coherence` and `sir-author-rule` — read as part of the intent investigation, not invoked. Neither mentions `implementation-sources.json`, the pinned set, or a membership criterion; they operate on the rule registry and were silent on the question this item turned on. That silence is itself recorded in §4.3.
- Wanted and absent: a skill covering the rules-corpus **identity** surface as distinct from the rule registry — what the pin is, when to rebind it, and what a rebind propagates to. The two rules skills cover authoring and coherence; nothing covered identity, which is where this defect lived.

## §10 Outcome markers

- Time to first green build: single `dotnet build` of the domain test project, first attempt, no repair.
- First meaningful test: baseline `verify-rules-corpus.sh` on clean `main`, exit 0, first attempt.
- First reproduction of the reported defect: a comment appended to `Simulation.fs` producing `exit=1` with the pin diagnostic while `generate-rules-corpus.sh --check` exited 0 — the two mechanisms disagreeing, which is the whole item.
- First green verification of the repair: full `rules` sub-gate run locally, all four steps green (`verify-rules-corpus.sh`, `SIR.Rules.Governance.Tests`, `test-rules-governance-tool-mutations.sh`, `generate-rules-governance.sh --check`). The hosted equivalent is checkable: `domain-conformance` is SUCCESS on PR #276 at `91e650b`.
- Ship readiness: PR #276, all checks green including `domain-conformance` and `pr-verdict`; `verify-paths` OK.
- Merge: not reached at time of writing; awaiting independent review.

## §11 Falsifiable improvements

1. **Require a satisfying-input inversion whenever a gate adds a precondition.** Prevents §4.1. Owner: S.I.R. / gate authoring convention, enforceable in `independent-review`. Acceptance: a gate change that adds a refusal without a companion inversion demonstrating a legal input still passes is a material review finding.
2. **Make `review wait` validate the round against live protocol state at append time.** Prevents §4.6. Owner: FS-GG Coordination Kit. Acceptance: an entry the `review` oracle would refuse is refused by `review wait` instead of being appended.
3. **Name the field source in the `claimGeneration` refusal.** Prevents §4.5. Owner: FS-GG Coordination Kit. Acceptance: the refusal text identifies which observable value on the claim marker is expected.
4. **Extract the rules-corpus source normalizer into a shared shell library.** Removes §7. Owner: S.I.R. / `scripts`. Acceptance: `verify-rules-corpus.sh` and `rebind-rules-corpus-sources.sh` reference one definition, and inversion 8 becomes redundant rather than load-bearing.
5. **State a membership rule on every hand-maintained durable-identity artifact.** Prevents §4.3. Owner: S.I.R. Acceptance: each such artifact carries, in-file, the predicate deciding membership, and a reader can classify a candidate path without reading commit history.

5. **An inversion must use a mutation the sibling checks cannot already catch.** Prevents §4.9 instance (1). Owner: S.I.R. / gate-authoring convention. **Acceptance:** for each gate a change adds or modifies, the inversion's mutation is shown to leave every *other* selected gate green — if a sibling also refuses it, the inversion has not exercised the gate under test and does not count as its evidence.
6. **A critic asks what the author did not mutate.** Prevents §4.9 instance (1) from the other side. Owner: FS-GG / `independent-review`. **Acceptance:** the review records at least one mutation class the author's evidence did not cover, or states that the author's inversions exhaust the gate's observable surface and why. A review that only re-runs the author's inversions does not satisfy it.
7. **Act on a review, not on its verdict marker.** Prevents §4.9 instance (3). Owner: FS-GG / `independent-review`. **Acceptance:** the repair round enumerates every finding id in the review body and dispositions each; a round that references the sealed decision without enumerating its findings is incomplete by construction.

8. **Break the guard's own subject and watch the guard go red.** Prevents the round-1 confirmation finding, in which a non-vacuity guard added *to prove the new check could fail* could not itself fail: `--inject-combat-divergence` computes its offset with `Array.findIndex`, which throws when the two evaluations agree, and then `failwith`s regardless, so the process aborts either way and an exit-code-only assertion passes in both worlds. Owner: S.I.R. / gate-authoring convention. **Acceptance:** for every inversion, the author records the guard observed **failing** with its subject deliberately broken — not merely passing with the subject intact. An inversion never seen red is a hypothesis, not evidence.
9. **Resolve ownership by reading the build graph, not by naming convention.** Prevents the round-1 confirmation finding that a build guard resolved `src/<Project>/<Project>.fsproj` for every declared path, which exists and builds for all of them but *compiles* only some: `RulesExplorer.fs` belongs to `SIR.RulesExplorer.Web.fsproj`, and `Lab.fs` and `EngineCatalog.fs` are compiled by `src/SIR.Replay.Core/SIR.Replay.Core.fsproj` from a different directory. Owner: S.I.R. / `scripts`. **Acceptance:** the guard resolves each path through `Compile Include` entries and refuses any path no project compiles; verified by appending invalid syntax to one file per owning project and observing a refusal naming that project.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | No scaffold or template operation ran; the workspace was an existing checkout. |
| onboarding-guidance | partial | The dispatch brief's environment facts (`DOTNET_ROOT`, repo name with trailing dot, non-persisting `whoami --mint`) all held; its review-contract section did not cover the two facts in §4.5 and §4.6. |
| skills | exercised | `pnext-item` and `fs-gg-feedback-report` invoked; `sir-check-rule-coherence` and `sir-author-rule` read and found silent on the item's subject (§4.3, §9). |
| sdd-authoring | not-exercised | Route receipt records `sddWorkId: null`; no `fsgg-sdd` stage ran. |
| implementation-apis | partial | `FS.GG.Coord.Core.dll` `ReviewWait` API driven directly through `dotnet fsi` (§4.8); no product API changed. |
| dependencies-build | exercised | `dotnet build -c Release` for the domain tests and the simulation project; pinned SDK 10.0.302 resolved via `DOTNET_ROOT`; no version moved. |
| testing | exercised | Eight inversions added inline to `verify-rules-corpus.sh` and checkable in the committed file; each production function was additionally mutated in a scratch copy and the suite observed red. The mutation runs were transient, so the durable evidence is the committed suite; the mutation results are recorded in the PR #276 body (§4.1). |
| evidence | exercised | Both acceptance directions demonstrated end to end and recorded in the PR #276 body. **The first version of the second direction was insufficiently adversarial** — it mutated a declared rule fact the regenerated-artifact check could already see (§4.9); an independent critic returned `changes-required` with **two** material findings — M1, the laundering escape, and M2, a build guard that covered 9 of the 19 declared paths while both the tool comment and the architecture doc claimed it covered a tree — and the repair is verified by hand-editing the correspondence file to bypass the new writer guard and confirming the gate still refuses, and by re-running the critic's own M2 probe. The demonstration is a transient working-tree mutation, so the durable residue is the committed inversion suite plus the PR body. |
| runtime-playtest | not-exercised | No product behaviour changed; no gameplay path driven. |
| performance | not-exercised | No performance-sensitive path touched; the `performance-first` planning gate did not apply to a gate and tooling repair. |
| documentation | exercised | `docs/executable-rules-corpus-architecture.md` gained `sourceCommit` semantics, both membership rules, and the rebind procedure, none of which it previously carried (§6). |
| packaging-upgrade | not-exercised | No package version moved. |
| worker-git-pr | exercised | Mint, take, four `widen` calls all `disjoint` (3 → 4 → 10 → 11 → 13 declared paths), PR #276, `verify-paths` OK, delivery-obligations declaration, review-wait entry — with the two frictions in §4.5 and §4.6. |
