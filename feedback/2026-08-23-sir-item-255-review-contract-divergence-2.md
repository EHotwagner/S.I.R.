---
feedbackSchema: 2
date: 2026-08-23
workspace: S.I.R
cycle: item-255-review-contract-divergence
lane: none
toolVersion: n/a
commit: 78e8fa2c9c53d698ddc07b9c843326f10f105c1d
---

# S.I.R. item 255, repair phase — what makes a documentation gate vacuous

The ordinary three-round review chain on PR #259 exhausted and the item entered the one bounded
repair phase. This report covers that phase only.

**On the earlier report in this cycle.** `feedback/2026-08-23-sir-item-255-review-contract-divergence.md`
is the record of rounds 1-3, bound to commit `3639355`. It is immutable and is not edited here. Two
things about it are worth stating plainly rather than leaving for a reader to trip over, because the
item's own critic raised both:

- Its sentence *"All seven are repaired; the gate now runs twenty-one document mutations over thirty-eight
  checks"* was **true at `3639355` and is not true at this head**, which ships 68 mutations over 46
  named checks (39 `doc:*`, 6 `engine:*`, 1 `known-blind:*`) — and the repair phase found instances 8-13 after that sentence was written. A cycle report
  is a snapshot bound to a commit, not a live description of the tree; this report carries the current
  numbers and supersedes those figures without amending that file.
- Its audit's sealed `checkedEvidence` digests for `docs/coordination-engine-contracts.md` and
  `.claude/skills/pnext-item/references/independent-review.md` **no longer match head**, because this
  phase changed both files. That is what sealing evidence to a commit *means* and is not a defect in
  that audit. That `feedback-tool validate` still returns PASS over stale digests is a separate matter
  and belongs to `S.I.R.#258`/`#270`, not here.

The phase was dispatched to answer a specific question: **why does the `Derived` bucket of
`scripts/test-review-contract-coherence.sh` keep producing vacuous assertions?** Four consecutive
rounds had each found one there and each repaired the instance in front of it. The dispatching host
framed the bucket as the generator and asked for that framing to be tested rather than adopted.
It did not survive the test, and the replacement is the main result below.

## §1 Provenance and confidence

- **Workspace:** `S.I.R`, branch `item/255-review-contract-repair-phase`, commit
  `78e8fa2c9c53d698ddc07b9c843326f10f105c1d` — the round-3 repair head. Round 0's head was
  `6e1ac3fad570c5802ebd2c9b03b07b7ecb4e965e`; §4.8 is the defect round 0 shipped and round 1 repaired.
- **Cycle boundary:** the repair phase of `S.I.R.#255` — a fresh chain numbered from round 1 against
  `repair-phase-max-rounds: 10`, entered automatically after the validated three-round exhaustion of
  PR #259. The branch carries PR #259's ten commits rebased onto `origin/main` so that round 3's
  verified repairs are preserved rather than redone. The work reported here is the commits on top of
  them — `6e1ac3fa` (round 0), `b6b1d36` (round 1) and the round-2 repair this report is bound to.
  §4.8 is the defect round 0 shipped and round 1 repaired; §4.9 is the defect round 1 shipped and
  round 2 repaired. The distinction is load-bearing rather than bookkeeping: each round's repair was
  incomplete in a way only the next round's inversion found.
- **Engine under test:** `fs.gg.coord.cli` `0.71.0`, read from `.config/dotnet-tools.json` by the
  gate itself rather than hardcoded. All engine facts were established by loading
  `FS.GG.Coord.Core.dll` from that pinned package and calling it.
- **Checkpoints:** `feedback/checkpoints/item-255-review-contract-divergence.jsonl`, 21 events, of
  which 6 were appended by this phase. Validated with `validate-checkpoint-state`.
- **Activation envelope.** Capture was active across `implementation-test-evidence` and
  `verify-ship-pr`. `scaffold-onboarding` was not re-exercised: this phase inherited a live
  workspace and an existing cycle rather than onboarding into one. Material events were recorded, so
  no zero-event activation receipt applies to this cycle.
- **The observation commit is `cb38462de5045cabe238fd17f296940e9cb946ea` (`HEAD~1`), not the commit in
  this frontmatter.** Four findings below describe defects in the *pre-repair* gate. Because the repair
  is committed at `6e1ac3f`, a `file:` locator there shows the fixed artifact and not the observation.
  Each such finding therefore carries a `command:git show cb38462:…` locator that reproduces the defect
  from committed history. This was pointed out by the actionability critic, and it is a real property
  of the `file:`-locator rule rather than a slip: the rule requires evidence tracked at the described
  commit, and a fix-in-the-same-commit strips the evidence for what was fixed.
- **Confidence limits.** Every claim about engine behaviour below was obtained by execution against
  the pinned assembly, never by reading IL or inferring from a refusal message — the `timestamp`
  finding in §4.3 is precisely a case where the refusal message was wrong, so message-reading is
  treated as unreliable throughout. Claims about the CLI's production-only behaviour
  (`authorizeReviewRecordWait`, comment-URL equality) are **not** re-verified here; they sit behind a
  live GitHub transport and remain labelled Transcribed in the document, from an earlier round's
  decompilation. I did not re-derive them.
- **Not measured.** Whether earlier rounds' authors actually relied on the mis-scored inversion
  evidence described in §4.1 is unknown and unknowable from the artifacts. What is measured is that
  the harness could not distinguish a firing check from a parse abort, so the evidence was incapable
  of supporting the conclusion drawn from it. The distinction matters and is kept.

## §2 What worked

- **Executing the engine instead of reading it.** Every contested question in this phase — the
  same-critic rule, the `timestamp` extension, whether `claimGeneration` is a string — was settled in
  minutes by `dotnet fsi` against `FS.GG.Coord.Core.dll`. The answers landed differently in each case,
  which is the point: `timestamp` contradicted **the document** *and* the engine's own refusal message;
  the same-critic rule contradicted **the packed skill** while the document was right; and
  `claimGeneration` confirmed the document — whose reader, on a board-host report I did not verify,
  had been misled by **the CLI help** instead.
  Three artifacts describe this engine, and a different one was wrong each time. This is the reusable pattern:
  the validator is cheap enough to be the oracle for every claim, and no description of it — page,
  skill, or help text — is trustworthy without being checked against it.
- **Attributing a red to a check id.** A four-line instrumentation of the existing mutation loop —
  scraping which `FAILED  <id>` lines each mutant produced — turned an opaque pass/fail sweep into a
  map, and that map is what located the defect that three review rounds could not. The technique is
  general: any mutation harness that scores on exit status alone can be given this in an afternoon.
- **Testing the rows believed sound.** The host's instruction was to test the hypothesis rather than
  adopt it. The two vacuous checks found this phase were both inside repairs a previous critic had
  verified; sweeping only the suspect row would have found neither.
- **`git worktree` plus a rebase of the closed PR's head.** Preserving ten commits of verified repair
  while opening a separately scoped PR was a single `worktree add` and `rebase`, with no conflicts.

## §3 What did not

- **The first attempt to reproduce the inherited finding produced a false red.** Mutating the
  document and running the whole gate reported non-zero, which looks like the check catching the
  mutation. It was the fixture aborting: the widening removed a byte-exact mutation anchor,
  `sys.exit` fired, and `set -e` took the run down. The finding was only reproducible against the
  `--inner` route. This cost one wasted measurement and produced §4.1.
- **My first auto-derived widening mutation was wrong and the gate caught it.** Adding a refused
  literal to the accepted column while leaving it in the refused column makes the row
  self-contradictory, which the new parser refuses; all six widenings reported `NO DETECTION`. The
  correct mutation moves the literal. Notable because the gate's own new attribution rule is what
  reported the mistake, in the first run that could have.
- **Two further sites had to be reclassified or widened rather than repaired in place.**
  `doc:wait-window-must-be-positive` has no document term at all and was renamed `engine:`; the
  generation-token parser hardcoded the one correct shape and had to be widened before its check could
  ever fire. Neither was on anyone's list, and the generation-token parser is one of the four
  presence-shaped sites enumerated in §4.2 — the count there includes it.

- **I committed the defect class myself, in the fix for it.** The corrected same-critic paragraph in
  `independent-review.md` ended with: *"each is probed against the validator, so this paragraph cannot
  drift back out of agreement without `scripts/test-review-contract-coherence.sh` reddening."* That is
  false. `grep -c 'skills/' scripts/test-review-contract-coherence.sh` returns **0** — the gate never
  opens the skills tree, so rewriting that paragraph reds nothing. It is a presence-free *and*
  extension-free claim: a binding asserted where no binding exists, written into the repair for a
  finding about exactly that. Caught by self-check before review, and corrected to say plainly that
  the document's table is bound, this paragraph is not, and it is only a pointer. Worth recording
  because it shows the failure mode is not carelessness — I had the whole analysis in front of me and
  still wrote it.

## §4 Findings

#### §4.1 The inversion harness scored a parser abort as a detection, so three mutations certified checks that had never fired

- **Kind:** defect
- **Impact:** The gate's central guarantee — "every DERIVED claim reds when the document is
  falsified" — was partly unfounded, and so was the review evidence for three rounds of repairs to it.
- **Expected:** A mutation that reds the run does so because the check bound to that claim fired.
- **Observed:** Attributing each of the 21 mutations to the check ids it reddened shows 18 red via a
  named check and **3 red with no failing check at all** — `S1`, `S14`, `S15` trip STEP 1's parser,
  which exits before any check runs. `doc:record-subject-form`, `doc:same-critic-exception` and
  `doc:accepted-exceptions-owner` therefore carried inversion evidence while never having executed.
  These 3 are the checks that had inversion evidence but never fired. They are **not** the same set as
  the 11 checks reported in §4.5 as having no inversion at all; the two counts are different views of
  one coverage hole — 3 checks had *misattributed* evidence, 11 had *none*, and one check
  (`doc:record-subject-form`) is in both because its only mutation was the parse-aborting one.

  **A second, distinct mechanism with the same effect**, folded here because the repair is shared: a
  widening that changes a row's text removes the byte-exact anchor of the mutation meant to catch it,
  `sys.exit` fires in the anchor guard, and `set -e` takes the run down — an abort scored as a
  detection. I hit this on my first measurement (§3) rather than deriving it; it is reproduced there
  and not by a separate locator here.
- **Evidence:** command:git show cb38462:scripts/test-review-contract-coherence.sh | grep -c 'mutate_and_expect_red "'; file:scripts/test-review-contract-coherence.sh
- **On that locator:** it counts *call sites* and prints exactly the **21** this finding claims. The
  looser `grep -c mutate_and_expect_red` prints 22, because it also matches the function definition at
  line 614 — a locator whose number does not equal the number in the text is a small instance of this
  report's own subject, so it is pinned rather than explained.
- **Version:** `fs.gg.coord.cli` 0.71.0 (pinned); n/a for the harness, which is repo-local.
- **Observation commit:** `cb38462` (`HEAD~1`). The `file:` locator is the **repaired** harness at
  `6e1ac3f` and shows the fix, not the defect; the `command:` locator reproduces the defect from
  committed history. Running the gate at `6e1ac3f` passes and does **not** exhibit this.
- **Owner:** EHotwagner/S.I.R. — `scripts/test-review-contract-coherence.sh`, STEP 3.
- **Recurrence:** new. Related in kind to the round-1 finding F1 recorded in
  `feedback/2026-08-23-sir-item-255-review-contract-divergence.md`, which found the gate comparing the
  engine against hardcoded expectations; this is that class relocated into the harness that was
  supposed to prove F1 fixed.
- **Avoidable cost:** three review rounds whose inversion evidence was partly an artifact of `set -e`,
  and one wasted measurement in this phase before the cause was identified.
- **Disposition:** product fix — landed in this PR. Every mutation now declares the check id it must
  red; a mutant that reds no check is reported `NO DETECTION` and fails the step.

#### §4.2 A presence-shaped expectation is structurally incapable of detecting a widening, which is why four rounds each found another vacuous assertion

- **Kind:** defect
- **Impact:** The recurrence the repair phase was dispatched to end. Three ordinary rounds each
  repaired one instance and the next round found another, because the property that produced them was
  never addressed.
- **Expected:** A claim documented as `Derived` is compared against the engine, so falsifying it reds.
- **Observed:** The discriminator is not the `Derived` bucket. STEP 1 at `cb38462` parses **18**
  expectation keys. Of those, **three are presence-shaped outright** —
  `acceptedExceptionsOwnedByCritic` and `sameCriticExceptionDocumented` (each `"<phrase>" in doc`) and
  `generationTokenShape` (a regex that spells out the one correct answer, so the only document that
  parses is the one already right). A **fourth presence-shaped site** sits *inside* `vocabularies`: the
  free-form branch, taken by any row whose cell is prose, which reduced to `cell.Contains tok`. Two
  further keys — `waitEnterKeys` and `waitTerminalKeys` — carry a **partial** extension: key names with
  every value discarded, which is §4.7. Every remaining key carries a full extension and held up under
  the same widening sweep. The failing sites are exactly those whose parsed expectation carries only
  the claim's **presence**
  (`"<phrase>" in doc`, `cell.Contains tok`) rather than its **extension**. STEP 1 aborts when such a
  phrase is absent, so by the time STEP 2 compares it the value is a constant and the check reduces to
  an engine self-test with a flag stapled to the front. Widening — keeping the text and adding a
  permitted-but-wrong alternative — survived every one: `acceptedExceptions` ownership widened to
  permit both, and the same-critic exception widened to cover `pass`, both stayed green, and both sit
  inside round 3's critic-verified repairs. The enumerated rows, key lists, cardinalities and subject
  forms all reddened correctly under the same sweep.
- **Evidence:** command:git show cb38462:scripts/test-review-contract-coherence.sh | grep -nE 'in doc\)|in inv\)|cell.Contains tok|<headSha>:<kind>:<round>'; file:scripts/test-review-contract-coherence.sh
- **Observation commit:** `cb38462` (`HEAD~1`); the `file:` locator is the repaired harness.
- **Version:** `fs.gg.coord.cli` 0.71.0 (pinned).
- **Owner:** EHotwagner/S.I.R. — `scripts/test-review-contract-coherence.sh` and
  `docs/coordination-engine-contracts.md`.
- **Recurrence:** seen again. The prior cycle report
  `feedback/2026-08-23-sir-item-255-review-contract-divergence.md` catalogues **seven**, and its §4.7
  names the `timestamp` row as that seventh — *"Round three then found a seventh … the `timestamp`
  vocabulary row was `Derived`, parsed, and checked by nothing."* The dispatch brief for this phase
  called the `timestamp` row an **eighth**; on the committed record it is the seventh, and I do not
  assert the higher total. (A case could be made that round 3 produced a *distinct successor* defect —
  it replaced "checked by nothing" with a vacuous `cell.Contains "ISO-8601"`, which is §4.3 — but
  that argument is not made here and the count is left at the committed one.) What this phase adds is
  measured and enumerable: the two prose invariants (§4.2), the generation-token parser, the wait-example
  value types (§4.7), and one check with no document input at all (§4.5). The contribution is not the
  count — it is the property common to all of them.
- **Avoidable cost:** four review rounds and one exhausted three-round chain, each repairing an
  instance rather than the generator.
- **Disposition:** product fix — landed in this PR. No expectation may be presence-shaped: vocabulary
  rows state both columns of their extension and the free-form branch is deleted rather than patched;
  the two prose invariants become a probed outcome table; every derived check must be reddened by a
  named mutation or the gate fails naming the uncovered ids; and widening mutations are derived from
  each row's own refused column so a row added later gets its inversion for free.

#### §4.3 `timestamp` accepts values that are not ISO-8601 instants, and the validator's own refusal message names a grammar it does not enforce

- **Kind:** defect
- **Impact:** The document told authors the field takes "any ISO-8601 instant" for four review rounds.
  More seriously, the check written to defend that row was derived **from the refusal message**, so a
  wrong oracle certified a wrong claim with full confidence.
- **Expected:** `timestamp must be an ISO-8601 instant`, per `validateReviewLedger`'s own refusal.
- **Observed:** Probed against the pinned engine, `2026-08-23 00:00:00`, `2026-08-23` and
  `August 23, 2026` are all **accepted**, and so is the RFC-1123 form
  `Sun, 23 Aug 2026 00:00:00 GMT` — a different standard entirely, found by the actionability critic
  and added to the row after I re-probed it. `1787434100552`, `banana`, `now`, `23/08/2026` and
  `2026-13-45T99:99:99Z` are refused with that message. `""` is refused with **two** errors,
  `timestamp is required` *and* `timestamp must be an ISO-8601 instant` — worth stating exactly in a
  finding whose subject is refusal messages that mislead. The field is parsed with .NET date parsing, not an ISO-8601 grammar. The previous check
  extracted the distinctive token of the refusal and asked whether the row's prose contained it,
  which reduced to `cell.Contains "ISO-8601"` — satisfied by the false claim, because the false claim
  was copied from the message the token came from.
- **Evidence:** command:bash scripts/test-review-contract-coherence.sh; file:docs/coordination-engine-contracts.md
- **On the locator:** an earlier draft cited "`dotnet fsi` against `FS.GG.Coord.Core.dll` over twelve
  probes", which is prose describing what I ran rather than something a reader can re-run. The
  committed, runnable form is the gate itself: `doc:vocabulary:timestamp` now probes every literal in
  both columns of that row against the validator, so the row's extension is re-verified on every run.
- **Version:** reproduced on `fs.gg.coord.cli` 0.71.0, the version pinned in
  `.config/dotnet-tools.json`. Not re-checked against a later release: 0.72.0 is the currency bump
  tracked by `S.I.R.#250` and 0.73.1's coherent-set release is still Draft, so the pinned version is
  the one this workspace runs.
- **Owner:** FS-GG Kit — `FS.GG.Coord.Core`, `StructuredDecision` refusal messages. The document half
  is EHotwagner/S.I.R.
- **Recurrence:** new for `timestamp`. Same class as the route-evidence overstatement already
  recorded in `docs/coordination-engine-contracts.md` and already filed against the Kit, where
  `meaningful route evidence must contain built artifact, command, comparison, and result` describes a
  structure the validator only counts.
- **Avoidable cost:** four rounds of a documented falsehood, and one vacuous check derived from it.
- **Disposition:** doc fix landed here — the row now states its extension in both directions and the
  page records the discrepancy explicitly. The engine-side half (a refusal naming a grammar it does
  not enforce) belongs to the Kit and is **not** filed by this report; it is surfaced to the host for
  a `cross-repo-coordination` request, consistent with this item's existing engine-side boundary.

#### §4.4 The packed review contract forbids a changed critic; the engine accepts a successor after `changes-required`

- **Kind:** defect
- **Impact:** A contract **stricter than the mechanism** is the dangerous direction. It is obeyed
  silently, produces no error at the point of the decision, and surfaces at acceptance after a full
  critic cycle in a round that can no longer close — charging the reader who followed the
  documentation most carefully. It also forbids the fresh-successor handoff the repair phase itself
  depends on.
- **Expected:** per `.claude/skills/pnext-item/references/independent-review.md`, "duplicate round
  numbers, skipped rounds, competing markers, **a changed critic**, or a fourth automated repair fail
  closed."
- **Observed:** `validateReviewLedger` refuses a differently-bound critic only within a settled
  generation. A `confirmation` bound to a different critic is **accepted** after a
  `changes-required` verdict and **refused** after a `pass`, with `every record in one review
  generation must bind the same critic`. A chain of an initial `changes-required` plus three
  confirmations by three further critics validates cleanly — the observed shape of PR #259's own
  exhausted chain. The board host separately reports that the review oracle returns
  `dispatchSuccessor` on a repaired head; **I did not verify that** — I observed only
  `awaitingInitialReview — dispatchCritic` on my own PR, and the live successor route was never
  exercised here.
- **Evidence:** command:git show cb38462:.claude/skills/pnext-item/references/independent-review.md | sed -n '388,392p'; command:git show cb38462:.claude/skills/work-board/references/host-loop.md | sed -n '71,75p'; command:bash scripts/test-review-contract-coherence.sh
- **Observation commit:** `cb38462` (`HEAD~1`'s parent line). The `Expected:` sentence quoted above
  **does not exist at this report's frontmatter commit** — it was removed by the repair, so a `file:`
  locator at `b6b1d36` shows the corrected paragraph and not the defect. The two `git show` locators
  reproduce the original wording from committed history; the `bash` locator verifies the engine half
  at head, where all four `successor-critic` outcome rows are probed.
- **Version:** `fs.gg.coord.cli` 0.71.0 (pinned).
- **Owner:** EHotwagner/S.I.R. — `.claude/` and `.agents/` mirrors of
  `skills/pnext-item/references/independent-review.md` and `skills/work-board/references/host-loop.md`.
- **Recurrence:** **seen again**, corrected from an earlier draft's "new". The engine measurement is
  already recorded in the prior cycle report's §4.14 —
  `feedback/2026-08-23-sir-item-255-review-contract-divergence.md` — in the same terms (accepted after
  `changes-required`, refused after `pass`). What is new here is the **contradiction with the packed
  contract**: that the text in `independent-review.md` and `host-loop.md` states the opposite of the
  measurement the prior report already carried, in both mirrors, and that nothing checked it. The
  board host independently ruled for the engine from precedent before this execution existed.
- **Avoidable cost:** none realised in this phase, because the host had already ruled for the engine.
  The unrealised cost is a wasted critic cycle for any worker that followed the text.
- **Disposition:** skill fix — landed in this PR in both mirrors, and bound: the four measured
  outcomes are now `successor-critic` rows in the document's outcome table, each probed against the
  validator, so the corrected sentence cannot drift back out of agreement silently.

#### §4.5 Attributed inversions plus a derived coverage requirement convert an unbound claim from a silent pass into a failure

- **Kind:** positive-pattern
- **Impact:** This is the part of the repair intended to outlive the item. The recurrence persisted
  because the **default outcome** for a newly documented claim was "no inversion, and nothing
  notices". Inverting that default is what the previous three repairs did not do.
- **Expected:** n/a — a pattern, not a defect.
- **Observed:** Three properties together, each of which had failed at least once here. (1) Every
  mutation names the check id it must red, and that exact id must appear as `FAILED`, so a parse abort
  or a fixture abort cannot be counted as evidence. (2) Every `doc:*` check the clean run emits must
  be reddened by some named mutation, or the step fails listing the uncovered ids — on its first run
  this listed 11 checks that had never had an inversion, including one with no document input at all.
  *(That 11 is an intermediate-iteration figure: it was reported by a build that no longer exists, so
  it is not recoverable from anything committed. It is stated as history, not as evidence.)* After
  round 2 the sweep is **78 mutations — 39 widening, 38 narrowing and 1 `widen-attempt`, 22.7s** —
  with every derived check carrying both directions, or an exemption that is itself proven by a
  mutation (§4.9). The requirement grew twice under review: set-coverage (round 0), then direction
  (round 1), then proof-of-exemption (round 2). (2b) **The coverage requirement covers `doc:*` only.** The 6 `engine:*` checks and
  the 1 `known-blind:*` check have no attributed inversion, because their predicates take no term from
  the document and no document mutation can red them — that is why `doc:wait-window-must-be-positive`
  was renamed rather than given a mutation. The gate's own summary says "every derived check named by
  the clean run"; any broader claim would be the over-claim this report is about.
  (3) The widening mutations are constructed by the gate from each row's own documented-refused
  column, so a row added later gets its inversion without anyone remembering. The sweep grew from 21
  went 21 -> 47 at round 0 and 47 -> 68 at round 1, and stayed at the cost of the sweep it replaced:
  **22.9s for 68 mutations against 22.5s for 21**, by running the independent mutants at
  `SIR_COHERENCE_JOBS` (default 6) instead of serially.
- **Evidence:** file:scripts/test-review-contract-coherence.sh; command:bash scripts/test-review-contract-coherence.sh
- **Version:** n/a — repo-local harness.
- **Owner:** EHotwagner/S.I.R. — `scripts/test-review-contract-coherence.sh`; the pattern generalises
  to any mutation harness in the org that scores on exit status.
- **Recurrence:** new.
- **Avoidable cost:** none.
- **Disposition:** accepted — landed in this PR, and described in the document's
  `How the gate keeps itself honest` section so the next author meets it before adding a claim.

#### §4.6 `check-invalidation` reports the same errors for an empty diff, which is the per-diff-assertion defect already filed

- **Kind:** defect
- **Impact:** Every worker running the commit-time invalidation check sees a failure and must
  establish for itself that it is not their own diff. I spent a measurement doing exactly that.
- **Expected:** `check-invalidation --base origin/main --head origin/main` compares a commit with
  itself and should report nothing.
- **Observed:** It reports **17 errors**, all `invalidation: overbroad or mismatched exception <audit>
  <finding> <locator>`, across audits for items 186, 220 and 231 — and the output is **byte-identical**
  to `--base origin/main --head HEAD`. That identity is the sharp part: an empty change set produces
  the maximum error count, so the result cannot be a function of the diff at all.
- **Root cause (established, on the existing row):** `S.I.R.#258` — the exceptions ledger is
  append-only and permanent, accumulating one entry per historical drift repair forever, while
  `invalidated` is derived from `git diff --name-status base head` and contains only citations the
  current change set touches. The ledger is therefore validated as a per-diff assertion, which no diff
  can satisfy.
- **Evidence:** command:dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- check-invalidation --base origin/main --head origin/main; issue:EHotwagner/S.I.R.#258
- **Version:** repo-local tooling at `origin/main`.
- **Owner:** EHotwagner/S.I.R. — `feedback-tool`'s invalidation index, per `S.I.R.#258`.
- **Recurrence:** **duplicate of `S.I.R.#258`** (OPEN). Explicitly **not** `S.I.R.#252`: that row is
  CLOSED, its fix is on `main` at `11cfc87`, and #258's own body records that this set is unrelated to
  the stale-digest defect #252 repaired. An earlier draft of this report attributed it to #252, which
  was wrong on the artifacts.
- **Avoidable cost:** one measurement per worker who meets it, until #258 lands.
- **Disposition:** **existing issue** — deduped to `S.I.R.#258`, with the empty-diff identity
  transplanted there (it is absent from that row's body) rather than filed as a new row. No new row
  created.
- **A claim I made and retracted.** I first transplanted a second "new fact" onto #258: that #258
  recorded 15 errors and I measure 17 *because the append-only ledger had since gained two entries*.
  **That is false.** `scripts/audit-binding-exceptions.json` holds **57 exceptions at `HEAD`, at
  `1d8c93d`, and at `8972d37`** — it gained nothing. #258's body already carries the correct
  explanation: `errors = entries − entries invalidated by this diff`, so the count varies with how many
  ledgered files a diff happens to touch (15 at `1c34852`, which touched three; 17 at `8972d37`, which
  touched one). I inferred a mechanism from two numbers without checking the quantity the mechanism
  was about. Retracted on the row at
  https://github.com/EHotwagner/S.I.R./issues/258#issuecomment-5384313982 before this report was
  committed. Recorded here rather than quietly dropped, because "a number that fits the story" is the
  same failure this report is about.

#### §4.7 The `review wait` enter example was parsed for key names only, so `claimGeneration`'s type could drift undetected

- **Kind:** defect
- **Impact:** `claimGeneration` is a `String` on the engine while `claim --json` emits a **numeric**
  `markerId`, so the obvious idiom is refused. The document's example was the only artifact that could
  have said so, and nothing held it to the engine.
- **Expected:** A documented example that the gate compares against the encoder is held to it.
- **Observed:** STEP 1 parsed the example with `sorted(json.loads(...).keys())` — key names only,
  every value discarded. Rewriting `"claimGeneration": "5382700300"` to `5382700300` reddened nothing.
  This is a **partial extension** rather than a presence test: the key *set* really is an extension and
  really was compared, but the claim the example makes about value *types* had none. It is the pair of
  keys §4.2 counts separately from the four presence-shaped sites, for that reason.
- **Evidence:** command:git show cb38462:scripts/test-review-contract-coherence.sh | grep -n 'sorted(json.loads'; command:bash scripts/test-review-contract-coherence.sh
- **Observation commit:** `cb38462` (`HEAD~1`). At `6e1ac3f` the `doc:wait-enter-value-types` check
  exists and mutation `S17` reds it.
- **Version:** `fs.gg.coord.cli` 0.71.0 (pinned).
- **Owner:** EHotwagner/S.I.R. — `scripts/test-review-contract-coherence.sh` STEP 1.
- **Recurrence:** new here. Related but distinct: a worker on `S.I.R.#232` (an unrelated item —
  *"Add viewport chunking, isolated SVG reconciliation, and frame-coalesced presentation"*) had to
  author a `review wait` entry, found no packed skill documenting the command, and recovered the
  schema by reflecting the assembly; it was misled by the **CLI help text**, which describes `kind` as
  the *event* kind while the field takes the *review* kind. `docs/coordination-engine-contracts.md`
  already documented both that vocabulary and `claimGeneration`'s string form **correctly**, so the
  defect surface for that half is the CLI help, not this page. I did not verify that worker's session;
  it is reported by the board host.
- **Avoidable cost:** none in this phase. Another lane paid an unmeasured number of refused attempts
  against the CLI help.
- **Disposition:** product fix — landed in this PR. The example's value **types** are now parsed and
  compared against `ReviewWait.encode`'s emitted JSON value kinds.

#### §4.8 Coverage without DIRECTION is the same defect one level up — my own repair shipped it, and review caught it

- **Kind:** defect
- **Impact:** The repair-phase round-0 head shipped a gate that certified checks as inverted while
  they were green on falsified pages. Six widenings survived, in rows a previous critic had already
  verified. This is the finding the whole phase exists to prevent, committed by the phase.
- **Expected:** "Every derived check is reddened by a named mutation" establishes that the check works.
- **Observed:** It does not. Round 0 required every `doc:*` check to be reddened by **some** attributed
  mutation and said nothing about the mutation's **direction**. Auto-derived widenings existed for
  three families; **22 of the 39** `doc:*` checks round 0 emits were covered by hand-written
  replacements alone — the
  narrowing direction, which is the one three ordinary rounds had already caught. Critic `tern-af70`
  measured six survivors (`W1`,`W2`,`W3`,`W4`,`W6`,`W7`); I reproduced all of them. The clearest
  exhibit, from one run on the widened `revision` row:

  ```
    ok      doc:overwrite-description:revision
    reds doc:overwrite-description:revision   S8 the revision substitution loses its off-by-one
  review-contract coherence passed:
  ```

- **Root cause, and it is not the mutation list.** Three checks' expectations were parsed with
  `re.search(...)` or `[0]` — the **first** literal, the rest discarded. **A first-match parse cannot
  represent a widening**: a widening *adds* an alternative, so the parsed expectation is unchanged and
  the comparison stays green. No number of added widening mutations would have fixed those checks,
  because the escape was in the parse. Underneath it, `doc:overwrite-description:*` was literally
  `text.Contains needle` — five of the thirty-nine, a substring test in the Derived bucket, under a
  banner I had written reading *"No expectation is presence-shaped"*. And the outcome table's `rule`
  column, in the table I added in the same commit, was parsed and read by nothing — the
  parsed-but-discarded defect I had repaired for the authorization table twenty lines earlier.
- **Evidence:** command:bash scripts/test-review-contract-coherence.sh; file:scripts/test-review-contract-coherence.sh
- **Version:** `fs.gg.coord.cli` 0.71.0 (pinned).
- **Owner:** EHotwagner/S.I.R. — `scripts/test-review-contract-coherence.sh`.
- **Recurrence:** the same generator as §4.2, one level up. Coverage-by-any-mutation is itself a
  **presence**-shaped claim: it establishes that *a* mutation reds, not that the *dangerous* one does.
- **Avoidable cost:** one full review round.
- **Disposition:** product fix — repaired in round 1, and **the round-1 repair was itself incomplete;
  see §4.9.** (1) The six cells the round-0 critic measured were made total, which is what makes
  `W1`–`W7` red. The sentence that stood here — *"Every cell parse is now total: a set of literals or
  an exact string, never a first match"* — was **false when it was sealed**: nine first-match parses
  survived, and round 1's critic found three more widenings through them.
  (2) Every mutation declares `widen` or `narrow`, and every derived check must carry **both**, or
  appear in a printed `NO_WIDENING`/`NO_NARROWING` registry with its reason. A check added later
  inherits neither, so the gate reds until someone supplies or declares one. Per `independent-review`
  step 7 the repaired gate was re-run against the critic's **exact** six escapes, plus `W5`: all seven
  now red, each attributed to the right check.

#### §4.9 The repair was applied to the cells that were measured, not to the property — and the exemption registry became an unchecked escape hatch

- **Kind:** defect
- **Impact:** Round 1 shipped a gate asserting *"EVERY PARSE BELOW IS TOTAL OVER ITS CELL. NONE TAKES
  THE FIRST MATCH"* while nine first-match parses survived beneath that banner, and a `NO_WIDENING`
  registry of ten hand-written reasons that nothing checked. **Three reasons were false and three
  widenings survived at exit 0.**
- **Expected:** a check excused from the widening requirement genuinely cannot be widened.
- **Observed:** critic `merlin-2693` constructed four widenings against cells the registry declared
  un-widenable. `doc:initial-round` (+ *"except that in a repair-phase chain an `initial` record must
  carry `"round": 1`"*), `doc:confirmation-round-contiguity` (+ *"the first must be `"round": 0`,
  the second `5`"*) and `doc:wait-enter-value-types` (a **second** `enter` example typing
  `claimGeneration` as the number `claim --json` emits) all **survive at exit 0**. Each states
  something the engine refuses, and each is the exact false claim a repair-phase author would
  plausibly write on that page. A fourth, `doc:not-meaningful-evidence-cardinality`, reds only as a
  fixture abort — fail-closed, but not a detection.

  **The sharpest observation, and it is the round-0 finding with `NO_WIDENING` substituted for
  coverage.** On the first document the gate prints, in one run:

  ```
    ok      doc:initial-round
    reds narrow doc:initial-round     S19 the initial record's round is falsified
              doc:initial-round       no widening: the claim is a single integer in one sentence; an
                                      added alternative is not a permitted-but-wrong member of a set
                                      but a second sentence, which the parser refuses rather than
                                      mis-parses
  review-contract coherence passed:
  ```

  **It printed the reason the escape is impossible, in the run in which that escape succeeded.**
- **Root cause:** the round-1 totalisation was applied to **the six cells round 0 measured, not to the
  property**. The checks left un-totalised were then excused on the ground that their parser *refuses*
  a second alternative. It does not — `re.search` cannot refuse and silently takes the first. The
  file's own `sole()` helper does exactly the refusing the registry claimed, and was used **once**.
  So the exemption was written for one mechanism (`doc:generation-token-shape`, where it is true) and
  extended to checks that lack it, with nothing to tell which was which. **An exemption whose reason
  is authored prose, inside a gate whose entire thesis is that authored prose rots, is that thesis
  applied to everything except itself.**
- **Evidence:** command:bash scripts/test-review-contract-coherence.sh; file:scripts/test-review-contract-coherence.sh
- **Version:** `fs.gg.coord.cli` 0.71.0 (pinned).
- **Owner:** EHotwagner/S.I.R. — `scripts/test-review-contract-coherence.sh`.
- **Recurrence:** the same generator as §4.2 and §4.8, at a third level. §4.2: a claim's expectation
  carried presence, not extension. §4.8: coverage established that *a* mutation reds, not that the
  dangerous one does. §4.9: an *exemption* from that requirement was itself an unchecked assertion.
  Each repair moved the unchecked claim up one layer rather than removing the layer.
- **Avoidable cost:** one further review round.
- **Disposition:** product fix — repaired in round 2, **and round 2's own totalisation was still
  phrasing-specific; see §4.10.** The four checks are totalised and now red by
  name (`X1`→`doc:initial-round`, `X2`→`doc:confirmation-round-contiguity`,
  `X3`→`doc:not-meaningful-evidence-cardinality`, `X4`→`doc:wait-enter-value-types`); nine of the ten
  exemptions are deleted and replaced by real widenings. **The one that remains is proven rather than
  asserted**: the gate emits a `widen-attempt` mutation for it and requires that mutation to red the
  run *without naming any check*, which demonstrates the parse-refusal the reason claims. An exemption
  may no longer cover both directions — a check excused one way must demonstrably carry the other, and
  the gate reds if it does not. The banner, the success footer and this report's own §4.8 are corrected
  to the property that actually holds, enumerated clause by clause, with the widening mutations rather
  than the sentence as the proof. Per step 7, re-run against **both** critics' exact escapes: all of
  `X1`–`X4` and all of `W1`–`W7` red by name.

#### §4.10 A prose claim's parse is phrasing-specific, so every patch was defeated by the next rephrasing

- **Kind:** defect
- **Impact:** Three consecutive rounds of widenings escaped through the same five claims, and each
  round's repair was defeated by the *next* critic simply rewording the counterexample.
- **Expected:** a documented value is bound to the engine.
- **Observed:** critic `avocet-dac3` widened three of them at exit 0 — the generation-token claim with
  *"in a repair-phase chain it returns `<kind>:<headSha>:<round>` instead"*, the wait window with
  *"a repair-phase wait may run for up to 72 hours"*, and the initial round with *"must carry round
  one instead"*. The last defeats round 2's own repair by **spelling the number as a word in a
  different position**. And the generation-token page was thereby made false *about this PR's own live
  wait receipt*, while the run printed that check's exemption reason.
- **Root cause — one sentence, and it is the reason the sequence never terminated.** *A claim stated
  in prose has no enumerable cell, so its parse is a phrasing-specific pattern, which is
  presence-shaped for every phrasing it was not written to see.* Round 2 loosened
  `must be at most (\d+) hours` to `at most (\d+) hours` because `W4` used that wording; added
  `` `"round": (\d+)` `` because `X1` did; added `\b(one|…|five)\b` because `X3` did. **Every patch
  matched the counterexample the previous critic happened to supply**, which is why each round's fix
  validated cleanly and the next round found another.
- **Evidence:** command:bash scripts/test-review-contract-coherence.sh; file:docs/coordination-engine-contracts.md
- **Version:** `fs.gg.coord.cli` 0.71.0 (pinned).
- **Owner:** EHotwagner/S.I.R. — `docs/coordination-engine-contracts.md`, `scripts/test-review-contract-coherence.sh`.
- **Recurrence:** the fourth level of the same generator. §4.2 presence-not-extension; §4.8
  coverage-without-direction; §4.9 exemption-without-proof; §4.10 **structure-not-prose**. The
  evidence across all four is one-sided and was in this repository's own guidance the whole time:
  *"When a gate's false positive comes from prose, the fix is not to enumerate the shapes prose takes;
  prose has more shapes than anyone enumerates. The fix is to match the structure of the thing being
  asserted."* **Every claim converted to structure has survived every attack since** — the free-form
  vocabulary cell, the two prose invariants. **Every claim left as a sentence has escaped in three
  consecutive rounds.**
- **Avoidable cost:** three review rounds, each spent patching a pattern.
- **Disposition:** product fix — repaired in round 3, by conversion rather than by another pattern.
  The five became a **Scalar invariants table** with accepted and refused columns, probed exactly like
  the vocabulary table. Their prose is wrapped in `<!-- scalar-governed:<id> -->` regions in which no
  digit, number word or shape token may appear — **a rule about where a claim may live, not how a
  sentence may be phrased**, so it needs no foresight about the next author's wording. Token rules
  alone were still insufficient (a Roman numeral, and *"increments it by a single unit"*, both
  survived), and enumerating more token shapes would be the trap itself — so each governed region's
  exact text is **transcribed and compared for equality**, the same repair that fixed the overwrite
  descriptions. Accepted cost, stated rather than hidden: improving that wording now requires updating
  the transcription, which is what *"this region carries no claims"* must mean to be enforceable.
  Per step 7, re-run against **every** round's escapes and against phrasings no round supplied:
  `Y1`–`Y3`, `N1` (Roman numeral), `N3` (spelled-out caps), `N5`, `N6` (no numeral at all) and a
  purely cosmetic reword all red; `W1`–`W7` unregressed.

#### §4.11 A check whose expectation is parsed out of the document it governs can never be more total than that document's own markup — so the fourth escape was the third one relocated, and the fix was to stop claiming the total

- **Observed.** Round 3's repair left two escapes, both executed on both routes at head `f61e23af`,
  each false against pinned `fs.gg.coord.cli` 0.71.0, each leaving `86 mutations, zero FAILED,
  exit 0`. **F6:** the purity rule constrains text *between* `scalar-governed` markers, and its own
  inversions inject only inside `_gov[0]`, so prose outside every marked region had never been
  exercised — three widenings survived there, two of them (`P3`, `P4`) round 2's `Y1`/`Y2`
  **relocated rather than rephrased**. **F7:** the transcription was looked up with a per-id
  `re.search` — the FIRST match — so a *second* region bearing an id already governed was compared
  against nothing and reached only the token rules, which the file's own comment concedes are
  insufficient; `P5` (a Roman numeral), `P6` (*"increments it by a single unit"*) and `P7` (*"may
  triple that ceiling"*) all survived.
- **Root cause, one cause for both.** The check derives its expectation by parsing the document it
  governs. `GOVERNED_TEXT` is a transcription of the page's own prose, and the *set of governed
  places* is read from markers the page itself authors. **A document that authors the expectation can
  always author around it**, and no amount of tightening the parse changes that — which is precisely
  why each round's repair reappeared one layer up: presence → direction → exemption → phrasing →
  position. F7 is the sharper instance: it is **round 1's `§4.1` first-match defect reintroduced by
  the mechanism that replaced round 2's escape**, in the same file, under the STEP 1 banner at `:119`
  that forbids it (*"NO PARSE BELOW PRODUCES A CHECK'S EXPECTATION FROM A FIRST MATCH"*) and clause 3
  at `:134` permitting `re.search` *"only as a LOCATOR"*.
- **Impact.** The gate printed *"NO claim is stated in prose"* on every green run while
  `docs/coordination-engine-contracts.md:542` stated *"use `not-meaningful` with exactly one entry"*
  in unclassified prose. **The page was already in the escaped state, unmutated.** The footer's own
  next clause recorded that two earlier versions of itself had overclaimed; this was the third.
- **Avoidable cost:** four review rounds, three of which repaired an instance and re-created the
  generator.
- **Disposition:** product fix — repaired in round 4, in **two different ways, deliberately**, because
  the two findings are not the same kind of problem.
  1. **F7 is a real hole, so it was closed by mechanism.** The region parse is now total: every
     region's body is collected in the one `finditer` pass under its marker label, an id governed by
     more than one region is itself a violation (`sole()`'s discipline, which this file already had
     and had not applied to governance), and the transcription equality is compared against **every**
     region bearing the label rather than the first. Both rules fire independently — `P5`–`P7` each
     red on `doc:scalar-region-purity` naming *"governed by 2 regions"* **and** *"region 2 of 2"* — so
     neither depends on the other having run. A duplicate-id widening was added to STEP 3's own
     catalogue, carrying no digit, no listed number word and no shape token, so it is invisible to the
     token rules and only the new totality reds it (86 → 87 attributed mutations).
  2. **F6 is an overclaim, so the CLAIM was weakened to what the mechanism delivers.** Building a
     total partition of the page was considered and rejected: it requires either freezing the whole
     document or marking every existing passage exempt, and unproven exemptions are `§4.9` — a defect
     class this chain already fought. The binding contract answers this directly, and had answered it
     before the escape was written: *"When a gate's false positive comes from prose, the fix is not to
     enumerate the shapes prose takes; prose has more shapes than anyone enumerates. The fix is to
     match the structure of the thing being asserted."* The structure asserted by *"no claim is
     stated in prose"* is a partition of the page, and **this page has no partition**. So the footer
     and STEP 1's clause 2 now state the marked-region property and print the boundary explicitly:
     prose outside a marked region is not examined, the marking is authored, and the gate does not
     stand behind what is outside it. `:542` — the live instance — was fixed by moving the value into
     the table rather than by marking the sentence around it.
  **`P1`, `P3` and `P4` still survive, and that is the reported result, not an oversight.** They are
  the disclosed boundary; the footer now says so in the same run that passes. A narrower true claim
  was chosen over a broader one that would fail again in round 5 — which is the choice this file has
  made correctly twice before (`known-blind:`, the transcription labels).
- **The rule was available and was not applied.** `independent-review.md` contains the answer to both
  findings, in a worked example, and it is the contract governing the very reviews that missed them.
  That is evidence about how these escapes survive: not a missing rule, an unapplied one.

#### §4.12 A protocol oracle answered `noVerdict` and a confident directive in the same breath — a failed read presented as a decision, on the item about checks that report what they did not verify

- **Observed.** Round 4's implementer took the item over from a dead holder with the engine's own
  documented recovery verb (`claim --force`; `reap` and `adopt` both refuse a live-lease holder with
  an open `item/<n>-*` PR, permanently, so neither was ever reachable here). Oracle snapshots were
  taken immediately before and after. The claim moved — and so did the protocol state:
  `verdict: next → noVerdict`, `state: awaitingImplementerRepair → null`, `stateRound: 4 → null`,
  `waitStatus: completed → ordinaryExhaustion`.
- **Root cause.** The durable round-3 wait receipt records `claimGeneration` as **the claim marker's
  GitHub comment id** — the exact object `--force` deletes. With that binding dangling, the oracle
  re-read a **repair-phase** round-3 confirmation as an **ordinary-chain** round-3 exhaustion. The
  board's own provenance contradicted it throughout: PR #285 carries
  `fsgg:independent-review-repair-phase:v1`, and PR #259 is closed with two escalation markers.
  Not self-healing: the deleted comment id can never be reissued.
- **Impact.** The oracle emitted `noVerdict` **and** a confident instruction — *"record exactly one
  structured escalation for repair-phase entry — never dispatch or resume ordinary round four"*.
  Acting on it would have recorded a second repair-phase entry for an item already in its repair
  phase, which `independent-review` forbids outright, or parked #255 on a false exhaustion. This is
  `#266`'s shape at the protocol layer: *"I could not evaluate this"* rendered as *"I evaluated it
  and it passed"* — and the engine already refuses exactly this elsewhere, calling it *"a FAILED
  READ, not an empty answer"*. The implementer refused to author a wait entry against it and stopped.
- **Avoidable cost:** one halted round, two host decisions, and a permanently dangling ledger binding.
- **Disposition:** filed by the host as `FS-GG/.github#2860` (engine-owned; not repairable here). The
  first fix named there is the non-answer, not the binding: an oracle that cannot decide must refuse,
  not direct. Recorded here because it happened **to this item, while repairing this item's own
  defect class** — a check reporting a state it had not verified.

#### §4.13 A marker the gate could not read was reported as a marker it had read and approved — the fourth overclaim, closed by refusing the input rather than by recognising the sentence

- **Observed.** Round 4's narrowed footer says a value *"cannot be restated inside a marked region in
  any phrasing, nor smuggled in beside one."* It could. Region ids are parsed by splitting the marker
  label on commas and dropping empties, and the label pattern `[a-z,-]+` admits **separators alone** —
  `<!-- scalar-governed:, -->` — which yields an **empty id list**. Every rule was keyed on an id or
  on a `GOVERNED_TEXT` label, so all of them iterated over nothing: the unknown-id check saw no ids,
  the uniqueness rule gained no entry, and the raw label `","` is not a transcription key, so the
  equality never compared it. Only the token rules remained — the ones this file concedes are
  insufficient. `Q1` (label `,` carrying `P6` **verbatim**) and `Q5` (label `,,`) both left the gate
  printing `ok doc:scalar-region-purity`, 87 mutations, zero `FAILED`, **exit 0**, on both routes.
- **Root cause.** Not a missing rule and not a phrasing gap: a **failed read reported as a pass**.
  `Q4` — the same region with the numeral spelled — reds *and prints an empty label before the colon*,
  which is the proof that the gate parsed the thing as a region and then issued a confident verdict
  about a marker whose ids resolved to nothing. This is `#266`'s shape, and it is the same shape as
  §4.12 one layer down: an authority that cannot interpret its input must refuse, not decide.
- **Impact.** Fourth overclaim in this file, printed two lines above the footer's own record of the
  other three.
- **Why this one terminates, where rounds 1–3 did not.** Round 4's critic measured the closure instead
  of arguing it. Of the four reachable ways to introduce a marked region, three already refuse: `R1`
  (regrouping ids across regions) reds via *"governed region is missing entirely"*, `R2` (a new row
  plus its region) reds via `doc:scalar-coverage`, and `Q4` reds on its content. The fourth needed
  **one fail-closed predicate**. Every previous round either widened a claim or moved an escape; this
  round makes **a sentence already printed become true**, which is a different act.
- **Avoidable cost:** one review round.
- **Disposition:** product fix — repaired in round 5. A marker whose id list resolves to nothing is
  refused and the region is not interpreted further. `,` and `,,` are **not** enumerated: what is
  refused is unreadability. Ablating that single predicate restores `Q1`/`Q5` to exit 0 on `--inner`,
  which is the step-7 proof that the predicate is what catches them; ablating it also reds the **clean**
  run with `NO DETECTION`, because the unreadable-marker widening added to STEP 3's catalogue then
  reddens no check. The gate now enforces its own coverage of the case it could not previously see.
  Sweep `87 → 88`.
- **Four copies of the retired total claim, not one.** The comment above the check
  (*"no value may be restated in prose"*) and the comment above the parse (*"a widening cannot smuggle
  a value back into prose regardless of wording"*) both still asserted the property the footer retired
  in round 4. Neither is printed, so neither is a step-8 finding — but a retired claim left standing in
  a comment directly above the mechanism that no longer implements it is how the next reader reinstates
  it. Both now state the marked-region scope and say plainly not to restore the wider one. Zero copies
  of the retired wording remain.

## §5 Did not exercise

- **`scaffold-onboarding`.** This phase inherited a live workspace, an existing branch and an existing
  cycle; nothing was scaffolded and no first build occurred.
- **The SDD lifecycle.** `delivery-route show` reports this item `route: lightweight` with
  `sddPackageReady: true`; no `fsgg-sdd` stage was required or run.
- **The production CLI review route.** `review wait` / `review record` were not driven in this phase.
  The gate deliberately cannot drive them — they sit behind a live GitHub transport — and the rules
  they enforce are labelled `production-only` in the document (6 occurrences) and asserted by the gate
  as a `known-blind:` check, rather than being silently absent. The document itself carries no
  `KNOWN-BLIND` label — that prefix is the gate's check-id namespace, and an earlier draft conflated
  the two.
- **Runtime/playtest and packaging.** No product runtime is in this item's scope.

## §6 Doc-versus-behavior contradictions

1. **`timestamp`.** `docs/coordination-engine-contracts.md` said "any ISO-8601 instant"; the engine
   accepts `2026-08-23 00:00:00`, `2026-08-23` and `August 23, 2026`. The engine's own refusal —
   `timestamp must be an ISO-8601 instant` — is itself the contradicting party, and is wrong.
   Owner: the page (fixed here) and `FS.GG.Coord.Core` (Kit).
2. **A changed critic.** `independent-review.md` said "duplicate round numbers, skipped rounds,
   competing markers, **a changed critic**, or a fourth automated repair fail closed"; the validator
   **accepts** a differently-bound confirming critic after a `changes-required` verdict. Owner: the
   packed skill, both mirrors. Fixed here.
3. **"require the same critic's confirmation after each repair"** in
   `work-board/references/host-loop.md`, same contradiction on the host side. Fixed here.
4. **The gate's own summary line.** It printed that every `Derived` claim "reds when the document is
   falsified" while three of its checks had never fired. Owner: the gate. Fixed here, and the summary
   now enumerates only what is enforced.

## §7 Workarounds still in the tree

- **The `Transcribed` category** in `docs/coordination-engine-contracts.md` — the authorization
  table's wait-state column and the engine-overwrites table are compared against a transcription of
  decompiled CLI code, not against the engine. Removal condition: an engine-side accessor that reports
  `authorizeReviewRecordWait`'s mapping without a live transport. Risk if permanent: falsifying the
  **engine** there would not be noticed; the page says so explicitly, which is the mitigation.
- **`known-blind:validator-cannot-check-the-subject`** — the in-process validator accepts any subject
  string, including `not-even-a-ref`, while the production route derives it. **Asserted by the gate**
  as a known blind spot rather than left as a silent gap; the document carries the `not-even-a-ref`
  worked example but no `KNOWN-BLIND` category. Removal condition: as above.
- **The gate is invoked by nothing, and is deliberately not half-wired.** No workflow references
  `scripts/test-review-contract-coherence.sh`. Wiring it so it can report a **pass** needs
  `scripts/ci-route.mjs`, `scripts/qualify-pr.sh`, `scripts/test-ci-route.mjs` and
  `.github/workflows/ci.yml` to change together: `subjectOrder` in `ci-route.mjs` does not contain
  `review-contract`, and `gateResult("review-contract", "pass")` throws `ci-route: unknown gate
  result:review-contract`, so a `run-ci-gate.sh` case alone exits non-zero on a correct and a falsified
  document alike. Round 1 of this item shipped that half-wire (`2006a76`) and reverted it (`c3b10be`).

  **This is `S.I.R.#265`, already filed, and it is already published** — the prior cycle report's §4.13
  records the same measurement, and #265's first comment records it again from round 1's critic. It is
  therefore listed here as a standing workaround rather than as a finding of this phase; an earlier
  draft carried it as §4.6 and that was a duplicate.

  Two corrections to that earlier draft's reasoning, both refuted by the workspace: the failure is
  **not** `RP-005-unknown-conservative` (that is *path* classification;
  `scripts/test-review-contract-coherence.sh` classifies `RP-004-cross-cutting` and
  `docs/coordination-engine-contracts.md` classifies `RP-001-documentation`) — it is `subjectOrder` /
  `gateResult`. And the touch-set attribution was wrong: `#280`'s declared `Paths:` are
  `scripts/ci-route.mjs`, `scripts/test-ci-route.mjs`, `scripts/test-ci-route-mutations.sh`, fixtures,
  `docs/ci-qualification.md`, `scripts/audit-binding-exceptions.json` and feedback files — **no
  `ci.yml`**; `#265`'s are `.github/workflows/ci.yml`, `scripts/run-ci-gate.sh`,
  `scripts/ci-integrity-plan.mjs`; `scripts/qualify-pr.sh` is in neither, and
  `overlap S.I.R.#265 S.I.R.#280` returns **DISJOINT**.

  One reason that attribution went wrong is worth keeping, because it is not a reading error: **#280's
  touch-set moved.** When the board host relayed it to me it *did* include `.github/workflows/ci.yml`;
  its worker later found the fix needed no workflow change and narrowed the declaration with
  `set-paths`, releasing `ci.yml` and unblocking #265. Both readings were correct when taken. A
  touch-set is live state, and quoting one as a standing fact — which is what I did — is the same error
  as quoting a refusal message as a grammar.

  **Blocker chain, from the board:** `#265` is `Blocked by` **`#255`** (this item), and `#255` is
  `Blocked by` `#280`. So #265 waits on #255 directly and on #280 only transitively. An earlier draft
  said #265 was blocked by #280 "by touch-set", which is wrong twice over.

  Removal condition: `S.I.R.#265`. Risk if permanent: the whole gate decays to documentation, which is
  the strongest form of the vacuity this item exists to end.

## §8 Friction and avoidable cost

- **One wasted measurement** reproducing the inherited finding, caused by the fixture-abort scoring in
  §4.1: the first widening sweep reported red and looked like a detection.
- **One self-inflicted iteration** on the auto-derived widening mutation, caught by the new
  attribution rule on its first run (§3).
- **Two additional gate iterations** to close the 11 uncovered checks and then the single remaining
  one (`doc:generation-token-shape`), both surfaced by the new coverage requirement rather than by
  reading.
- **Command duration, kept separate from wall-clock.** Stated explicitly because "we did not wire it
  into CI" (§7) invites the assumption that cost was the reason, and it was not:

  | version | mutations | wall clock |
  |---|---|---|
  | pre-repair sweep, `cb38462` | 21 | 22.5s |
  | round 0, `6e1ac3fa` | 47 | 13.5s |
  | round 1, `b6b1d36` | 68 — 30 widening, 38 narrowing | 22.9s |
  | round 2 | 78 — 39 widening, 38 narrowing, 1 widen-attempt | 22.7s |
  | round 3 | 86 — 45 widening, 41 narrowing | 28.6s |
  | round 4 | 87 — 46 widening, 41 narrowing | 26.3s |
  | round 5 | 88 — 47 widening, 41 narrowing | 25.2s |

  **3.2x the mutations for the same wall clock as the sweep it replaces** — but that is parallelism,
  not efficiency, and the honest counterpart belongs in the same paragraph: **CPU went from ~26s user
  to ~2m10s user, roughly 5x.** The wall clock is flat because the independent mutants run at
  `SIR_COHERENCE_JOBS` (default 6). On a serial runner the gate takes **1m24s**, which I measured
  rather than assumed; it passes identically there, so correctness does not depend on parallelism, only
  speed does. A section written to pre-empt "cost was the reason it is not wired into CI" (§7) should
  not omit the multiplier that a busy CI runner would actually feel.
- **Not a cost of this diff:** `feedback-tool check-invalidation` reports 17 errors, and reports the
  **same 17** for `--base origin/main --head origin/main` — byte-identical output. The condition is on
  the default branch, not on this branch. See §4.6: this is `S.I.R.#258`, not `S.I.R.#252` — #252 is
  closed and its fix is on `main` at `11cfc87`, and #258's body says explicitly that this error set is
  unrelated to the stale-digest defect #252 repaired.
- **`pr-verdict` passed, contradicting an expectation I was given.** I was briefed that it would fail
  for `S.I.R.#280` "no matter what you do". The PR classified `cross-cutting` as predicted, and the
  check **passed**: `budgetMilliseconds 300000`, `acceptanceTargetMilliseconds 240000`,
  `requiredHeadroomMilliseconds 60000`, `actualHeadroomMilliseconds 70297` — elapsed 229,703ms, 10.3s
  under the target. All 20 checks: 15 pass, 5 skipping, 0 fail. #280 is real and the margin is thin
  (a prior clean run on another item measured 259,611ms, 19.6s *over*), so it remains a live re-run
  risk; it was not blocking this head. I corrected the PR body, which had asserted the red.

## §9 Skill value and gaps

- **`pnext-item` — invoked, load-bearing.** Its `independent-review` reference supplied the
  gate-inversion discipline this whole phase applies, including the rule that a surviving inversion is
  material by definition and that predicate inversion is strictly weaker than subject inversion. The
  finding in §4.1 is that reference's own doctrine applied to the harness enforcing it.
- **`pnext-item/references/independent-review.md` — misleading guidance, fixed.** §4.4.
- **`work-board/references/host-loop.md` — misleading guidance, fixed.** §6.3. Note this file is
  host-facing and was corrected by a worker; a worker reading only `pnext-item` would not have found it.
- **`fs-gg-feedback-report` — invoked, and its critic step earned its cost.** The fresh-context
  actionability critic re-derived the central claim with its own mutations rather than trusting mine,
  and separately found: a duplicate finding (the old §4.6, already published twice), a wrong recurrence
  disposition on §4.4, three inconsistent instance counts, a stale check-count in §10, and a
  fabricated touch-set attribution. Five of those are corrections I could not have reached by
  re-reading my own draft.
- **`fs-gg-feedback-report` — a real trap in the `file:` locator rule.** A `file:` locator is valid
  only when tracked in the commit named by frontmatter. For a report whose findings are defects **that
  this same commit repairs**, that rule points every locator at the fixed artifact and strips the
  evidence for the observation — four of this report's findings were affected. The rule is right and
  the fix is to cite the pre-repair commit with a `command:git show <sha>:<path>` locator alongside;
  an earlier draft praised the constraint without noticing it had done this. Worth documenting in the
  skill, because "commit the implementation, then draft" is the correct order and still leaves the
  trap.
- **`intra-repo-parallel-work` — invoked.** `overlap --active` returning `DISJOINT` after the widen is
  what made the report-file widen safe; the same mechanism is what would have refused a widen into the
  CI-wiring files (**§7**).
- **Not invoked, correctly:** every `fs-gg-*` product skill, and the whole SDD lifecycle — see §5.
- **Wanted and absent:** nothing this phase needed was missing. The one capability gap is CI wiring
  (**§7**, and `S.I.R.#265`), which is a repo-configuration gap rather than a skill gap.

## §10 Outcome markers

| Marker | Value | Basis |
|---|---|---|
| Identity minted to claim converged | under 5 minutes | commands in sequence; estimate |
| Claim to root cause established | ~1 hour | estimate, wall clock |
| First green run of the rebuilt gate | after 3 iterations | measured (uncovered-check reports) |
| Full gate at round 0, 47 mutations | 13.5s | `time ./scripts/test-review-contract-coherence.sh` |
| Baseline it replaces, 21 mutations | 22.5s | same command at the branch point |
| Checks emitted on a clean document | 39 `doc:*`, 6 `engine:*`, 1 `known-blind:*` | gate output at head |
| Mutations after round 3 | 86 — 45 widening, 41 narrowing, 28.6s | gate output at that head |
| Mutations after round 4 | 87 — 46 widening, 41 narrowing, 26.3s | gate output at that head |
| Mutations after round 5 | 88 — 47 widening, 41 narrowing, 25.2s | gate output at head |
| Round 4's escapes `Q1`, `Q5` (unreadable marker) | were green, now red by name | §4.13, both routes |
| `Q4`, `R1`, `R2` (the three that already refused) | unregressed, still red | §4.13 |
| Ablating the unreadability predicate | `Q1`/`Q5` return to exit 0; clean run reds `NO DETECTION` | §4.13 |
| Round 3's five escapes (`Y1`,`Y2`,`N1`,`N6`,`C1`), re-run per step 7 | all still red, each `doc:scalar-region-purity` | §4.11 |
| Round 3's escapes `P5`–`P7` (duplicate-id regions) | were green, now red by name | §4.11, both routes |
| Round 3's escapes `P1`,`P3`,`P4` (unmarked prose) | still green — the DISCLOSED boundary | §4.11; the footer now states it |
| `doc:*` checks with a NARROWING inversion | all, or a printed reason (2 rows) | enforced by the gate itself |
| `doc:*` checks with a WIDENING inversion | all, or a printed reason (10 rows) | enforced by the gate itself |
| Round 0's seven escapes, re-run per step 7 | all red, each attributed | §4.8 |
| Round 1's four escapes, re-run per step 7 | all red, each attributed | §4.9 |
| `engine:*` / `known-blind:*` with an inversion | 0 of 7, by design | their predicates take no document term |
| CI on the candidate | 15 pass, 5 skipping, 0 fail | `gh pr checks 285` |
| Merge | not reached in this report | review in progress; `Blocked by: S.I.R.#280` is a live risk, not a current block |

## §11 Falsifiable improvements

1. **Give every mutation harness in the org the attribution rule.** Would have prevented §4.1 and the
   three rounds of misattributed evidence. Owner: EHotwagner/S.I.R. and any repo with a mutation
   sweep. Acceptance: a mutant that reddens no named check fails the step rather than passing it.
2. **Refuse presence-shaped expectations by construction, not by review.** Would have prevented §4.2
   and the four presence-shaped sites it enumerates — not a larger historical total, which §4.2
   declines to assert and this line must not smuggle back in. Owner: `scripts/test-review-contract-coherence.sh`. Acceptance: a table row
   that cannot state what the engine refuses is a parse failure — verifiable by deleting a
   `must be refused` column and observing the gate red.
3. **Make refusal messages in `FS.GG.Coord.Core` state what the code enforces.** Would have prevented
   §4.3 outright, and prevented the check derived from that message. Owner: FS-GG Kit. Acceptance:
   `timestamp must be an ISO-8601 instant` either becomes accurate or the parser becomes strict;
   either is testable by the probe set in §4.3.
4. **Rank contract-versus-engine divergences by direction when reconciling.** A contract stricter than
   the mechanism (§4.4) is silent, late and expensive, and selects for conscientious readers; a looser
   one is found out immediately. Owner: `pnext-item`. Acceptance: the reconciliation checklist orders
   over-strict claims first — measurable as: for each divergence, does following the document produce
   a state the engine refuses?
5. **Land `S.I.R.#255` (this item) so `S.I.R.#265` can land, and wire the gate there.** Per the board,
   `#265` is `Blocked by` `#255` directly; `#255` is `Blocked by` `#280`, so #280 gates #265 only
   transitively. Would make every guarantee above actually run instead of decaying to documentation.
   Owner: EHotwagner/S.I.R. — but **not `#265` as currently declared.** The change needs
   `scripts/ci-route.mjs`, `scripts/qualify-pr.sh`, `scripts/test-ci-route.mjs` and
   `.github/workflows/ci.yml` together, and #265 declares only the last of those four plus
   `run-ci-gate.sh` and `ci-integrity-plan.mjs`; two of the others sit in **#280**'s declared paths and
   `qualify-pr.sh` in **#272**'s. So #265 cannot make the change its own title describes until its
   touch-set is rescoped — which is host judgement, not something this report should decide. That is
   §7's own "a touch-set is live state" lesson applied to the recommendation §7 feeds; an earlier draft
   of this line assigned #265 files it does not hold, which is the same error one step further on. Acceptance:
   falsifying one row of `docs/coordination-engine-contracts.md` reds a PR with no one running a
   script by hand — and the same PR passes when the row is restored, which is the half the reverted
   half-wire could not do.
6. **Give the `file:` locator rule a documented companion for self-repairing reports.** Would have
   prevented four stale locators here (§9). Owner: `fs-gg-feedback-report`. Acceptance: a report whose
   findings are repaired by its own commit cites the pre-repair commit and validates, without the
   author having to discover the trap.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Inherited a live worktree and an existing branch; nothing scaffolded. |
| onboarding-guidance | partial | The dispatch brief's environment facts (`DOTNET_ROOT`, the trailing dot in `S.I.R.`, non-persistent `whoami --mint`) were each load-bearing and each correct; no onboarding path was newly exercised. |
| skills | exercised | `pnext-item`, `intra-repo-parallel-work`, `fs-gg-feedback-report` invoked; two skill files corrected (§4.4, §6.3). |
| sdd-authoring | not-exercised | `delivery-route show` reports `route: lightweight`; no lifecycle stage required. |
| implementation-apis | exercised | `StructuredDecision.validateReviewLedger`, `Driver.decodeStructuredReview`, `ReviewWait.encode`, `ReviewWait.generationToken`, `ReviewWait.validate` driven directly from `dotnet fsi`. |
| dependencies-build | partial | `dotnet tool restore` against the pinned `fs.gg.coord.cli` 0.71.0; no product build in scope. |
| testing | exercised | The gate rebuilt three times and re-measured each time: 78 attributed mutations at round 2 (39 widening, 38 narrowing, 1 proof-of-exemption), `22.7s`. Two vacuous checks and eleven uncovered ones found by measurement at round 0; six surviving widenings found by round 0's critic (§4.8); three more, through an unchecked exemption registry, found by round 1's critic (§4.9). Every repair to this gate has been incomplete in a way only the next inversion found — which is the report's own subject happening to the report's own instrument. |
| evidence | exercised | Every engine claim established by execution; §4.3 is a worked case of why message-reading is not evidence. |
| runtime-playtest | not-exercised | No product runtime in this item's scope. |
| performance | partial | Command duration measured at each round and held flat while the work tripled (21 mutations/22.5s → 68/22.9s), verified at `SIR_COHERENCE_JOBS=1` as well so correctness does not depend on parallelism; no typed performance intent applies — this is not interactive or simulation work, so the `performance-first` gate does not bind. |
| documentation | exercised | `docs/coordination-engine-contracts.md` substantially rewritten: vocabulary extensions in both directions, the outcome table, the `timestamp` correction, and the section explaining what makes a check vacuous. Two packed skill files corrected in both mirrors. |
| packaging-upgrade | not-exercised | No package version change; `S.I.R.#250` owns the currency bump. |
| worker-git-pr | exercised | Forced claim recovery of a dead holder, a separately scoped repair-phase PR preserving a closed PR's verified commits, a `widen` that `overlap --active` confirmed disjoint, a `review wait` entry accepted first try from the documented schema alone, and one wiring change correctly left to `S.I.R.#265` (§7). |
