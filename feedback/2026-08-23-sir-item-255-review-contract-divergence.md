---
feedbackSchema: 2
date: 2026-08-23
workspace: S.I.R
cycle: item-255-review-contract-divergence
lane: none
toolVersion: n/a
commit: 3639355882f53565b902c96dffab08c221a9b482
---

# S.I.R. item 255 — the packed review contract and the engine's review ledger

## §1 Provenance and confidence

**Activation envelope.**

- `activation`: the capture loop was active for this cycle from claim through review round 1 and the
  post-rebase close-out.
- `phases`: `scaffold-onboarding`, `implementation-test-evidence`, `verify-ship-pr`.
- `material events`: 15 checkpoints in `feedback/checkpoints/item-255-review-contract-divergence.jsonl`.
- `zero-event reason`: not applicable — events qualified.

**Cycle boundary.** Claim of `S.I.R.#255` by worker `rook-e7e8` through `fsgg-coord take` to the
verified done stamp for that item. One item, one worker, one PR (#259), one critic round.

**Versions, read from artifacts rather than re-derived.** `fs.gg.coord.cli` **0.71.0**, pinned in
`.config/dotnet-tools.json`. SDK **10.0.302**, pinned in `global.json` with `rollForward: disable`.
Engine assemblies at `~/.nuget/packages/fs.gg.coord.cli/0.71.0/tools/net10.0/any/`.

**Lifecycle.** SDD was **not used**, and that was correct: the item's delivery-route receipt
(revision 2, digest `e8e1c33b8d2bdb2e8e78a24befbd6eaf84a5d93de28fe5ebd745a0dbe5bb7497`) records
`route: lightweight`, `sddWorkId: null`, `requiredGates: []`.

**Confidence limits.**

- Engine behaviour was established from the shipped 0.71.0 assemblies by calling public functions and
  by decompiling `FS.GG.Coord.Cli.Client` with `ilspycmd`. Source was never available. Claims about
  *what* the engine does are measurement; claims about *why* are inference.
- This worker drove `wait enter`, the critic's `record`, and `wait complete`. The `acceptance` record
  and a green `landable` are the host's acts. Statements about the acceptance gate are derived from the
  decompiled gate and the pure validator, and were independently corroborated on PR #254's chain.
- §4.7 and §4.8 are defects in this cycle's own first deliverable, found by review. They are reported
  as measured, not as self-assessment.

## §2 What worked

**The engine's own encoders and validator are a drift-proof contract oracle.** `ReviewWait.encode`
emits the exact wire document; `Driver.encodeStructuredReview` and `Driver.decodeStructuredReview` do
the same for records; `StructuredDecision.validateReviewLedger` names every invariant it enforces in
its own words. Reusable rule: **call the producer instead of guessing the product.**

**`ilspycmd` answers what the pure API cannot**, and is already on `PATH` at `~/.dotnet/tools`. One
invocation over `FS.GG.Coord.Cli.Client` settled which draft fields the engine overwrites, what each
command gate compares, and the derived-subject rule.

**The typed oracle is genuinely authoritative.** At every transition `fsgg-coord review <ref> --pr <n>`
named the exact next action and, where one was required, the exact generation token — matching an
independently derived token character for character. Twice it contradicted prose guidance and was right
both times, which is now this board's standing rule.

**The independent-review gate worked against the author of the contract it was gating.** The critic's
`changes-required` found a `subject` defect the author could not have found by the author's method. A
second read-only reading found two more the first missed.

**The recovered path ran end to end on the first attempt.** `wait enter` was accepted with no
trial-and-error field discovery, and the same sequence carried PR #254 to a green `landable` the same
night with no wasted round.

## §3 What did not

The first deliverable shipped a regression gate that could not fail against its own declared subject
and asserted in three files that it could (§4.7). Documentation of the `review wait` entry rules was
wrong in both directions (§4.8). Both share the `subject` error's cause: the contract was recovered
from `FS.GG.Coord.Core`, which cannot see what the CLI command gates enforce.

A critic checked out the reviewed SHA inside the implementer's live worktree, leaving the branch
detached for hours and costing re-applied edits (§4.10), and one review was duplicated by a dispatch
collision (§4.11).

## §4 Findings

#### §4.1 The packed review contract and the engine's enforced contract are different mechanisms

- **Kind:** defect
- **Impact:** No PR in the workspace could reach a `landable` green verdict by following the packed
  skills. Three PRs stalled simultaneously; one worker escalated the item as unresolvable locally.
- **Expected:** An agent following the packed `independent-review` contract produces a review chain the
  engine accepts.
- **Observed:** The skill documented prose HTML-comment markers; `landable` requires a
  `fsgg.coord.review-decision/v2` ledger written through `review record`, gated behind a durable entry
  from `review wait`. Neither command appeared in any packed skill **before this change**. Note the
  tense: at the commit this report names, the cited file is already REPAIRED and names both commands.
  The pre-change state is on `origin/main`, where the same file contains zero occurrences of either;
  the cited file is evidence of the repair, and the issue is evidence of the defect.
- **Evidence:** issue:EHotwagner/S.I.R.#255; file:.claude/skills/pnext-item/references/independent-review.md
- **Version:** fs.gg.coord.cli 0.71.0, the current pin.
- **Owner:** EHotwagner/S.I.R. — the packed `pnext-item` and `work-board` skills.
- **Recurrence:** new; this item is the row.
- **Avoidable cost:** three stalled PRs; one abandoned item; this cycle.
- **Disposition:** product fix

#### §4.2 The blocking premise was false: no invented value can reach the ledger

- **Kind:** documentation
- **Impact:** A worker stopped rather than write guessed values into an append-only ledger. The
  instinct was right and the premise wrong, so an item was escalated when it was completable.
- **Expected:** Documentation states which draft fields are authored and which are derived.
- **Observed:** `review record` rebuilds the record with `existing.Length + 1`, the preceding digest,
  the live claim comment id, the live PR base tip, and `StructuredDecision.reviewDigest`, discarding
  the draft's values for all five. The draft must contain the keys; the values are never read.
- **Evidence:** file:docs/coordination-engine-contracts.md; issue:EHotwagner/S.I.R.#255
- **Version:** 0.71.0
- **Owner:** EHotwagner/S.I.R. — docs/coordination-engine-contracts.md
- **Recurrence:** new
- **Avoidable cost:** one item stopped mid-flight and re-dispatched.
- **Disposition:** doc fix

#### §4.3 `landable`'s review-acceptance refusal names a schema but not its producing command

- **Kind:** defect
- **Impact:** The refusal an agent hits gives no actionable next step; recovering the contract required
  decompilation.
- **Expected:** A refusal names the command that produces what it wants, as `Protocol.landableExitCodes`
  does for other codes.
- **Observed:** The refusal names `fsgg:review-decision/v2` and never `review record` or `review wait`.
- **Evidence:** issue:FS-GG/.github#2834
- **Version:** 0.71.0
- **Owner:** FS-GG/.github — src/FS.GG.Coord.Core/Protocol.fs
- **Recurrence:** same class as FS-GG/.github#1667 (closed, different refusal path); follow-on to #2360.
- **Avoidable cost:** the recovery effort in this cycle.
- **Disposition:** existing issue

#### §4.4 `validateReviewLedger` refuses with a message asserting a check it does not perform

- **Kind:** quality-gap
- **Impact:** A reader who believes the message records vacuous route evidence in good faith, and the
  check that would have caught it reads as satisfied.
- **Expected:** A refusal describes what the function checks.
- **Observed:** The message names four required parts; the check is pure cardinality. Any four strings
  pass; three or five fail however well they describe those parts.
- **Evidence:** issue:FS-GG/.github#2834; command:bash scripts/test-review-contract-coherence.sh
- **Version:** 0.71.0
- **Owner:** FS-GG/.github — src/FS.GG.Coord.Core/StructuredDecision.fs
- **Recurrence:** new; not covered by #2148 (closed), which required the evidence rather than defining its check.
- **Avoidable cost:** none — caught before relying on it.
- **Disposition:** existing issue

#### §4.5 `intake apply` is documented as atomic, is not atomic, and poisons its own retry

- **Kind:** defect
- **Impact:** A failed filing left an unlabelled, off-board issue upstream with no indication it
  existed, and the corrected draft could never be applied.
- **Expected:** "validate or atomically project one receipt-bound filing draft".
- **Observed:** `intake validate` passed a draft using the lowercase `severity` shown by the
  `fsgg.coord.intake/v1` template at `~/.claude/skills/cross-repo-coordination/references/deep-detail.md`
  line 107 — the Kit-distributed skill actually loaded at runtime, **not** the repository's
  `.claude/skills/cross-repo-coordination/` mirror, which carries no `intake` section at all (see
  §4.12). `intake apply` then failed HTTP 422 after creating the issue and before labelling and
  projection. Correcting the rejected value produced `receipt content digest does not match this draft`.
- **Evidence:** issue:FS-GG/.github#2835
- **Version:** 0.71.0
- **Owner:** FS-GG/.github — src/FS.GG.Coord.Cli/IntakeApplication.fs, src/FS.GG.Coord.Core/IntakeReceipt.fs
- **Recurrence:** new; same family as #2301 and #2541 (closed) but neither covers this write path.
- **Avoidable cost:** one partially created upstream issue completed by hand outside the typed path.
- **Disposition:** existing issue

#### §4.6 `whoami --mint` does not persist, and nothing says so

- **Kind:** defect
- **Impact:** A harness giving each tool call a fresh shell silently reverts the worker to the shared
  session identity — the exact claim collision the mint exists to prevent — with no failure signal.
- **Expected:** The identity established at step 0 of `pnext-item` holds for the item.
- **Observed:** `whoami --mint` prints an `export` line for one shell. No packed skill states the
  consequence for a per-call-shell harness. That re-minting — the obvious recovery — would destroy the
  existing claim by issuing a different id is an **inference from the observed mint behaviour, not a
  measurement**; this worker did not test it, and neither #266 nor the packed
  `intra-repo-parallel-work` states it.
- **Evidence:** issue:EHotwagner/S.I.R.#266
- **Version:** 0.71.0
- **Owner:** EHotwagner/S.I.R. — .claude/skills/intra-repo-parallel-work; Kit-mirrored.
- **Recurrence:** new in this workspace; the host hit it independently.
- **Avoidable cost:** none here — mitigated by an explicit export in every invocation.
- **Disposition:** existing issue

#### §4.7 The cycle's own regression gate could not fail against its declared subject

- **Kind:** defect
- **Impact:** The gate shipped asserting in three files that it "fails when any load-bearing claim is
  inverted". Seven falsifications of the document's tables and key lists all exited 0.
- **Expected:** A gate whose subject is a document reds when that document is falsified.
- **Observed:** The engine-conformance half compared the engine against constants transcribed into the
  script; the doc-binding half grepped for twelve literal substrings. The two halves never met, so no
  engine claim was bound to the prose stating it.
  **The repair then reproduced the defect six times in miniature across two review rounds**, none
  found by the repair's own sweep. Round one: the gate iterated only four of the authorization table's
  five rows, leaving the `acceptance` row falsifiable undetected; it compared the wait-state column
  against `if kindName = "initial" then "Waiting" else "Waiting"`, a constant compared with itself; and
  it "proved" the literal-only claims red-when-deleted with a loop whose mutation and assertion were
  the same `grep`. Round two: two vocabulary rows were parsed into the expectation file and never
  looped over — parsed-but-unchecked, worse than unparsed because the expectation file makes it look
  covered; the overwrites table was compared against a string literal inside the gate, three lines
  below the comment introducing the category it belonged in; and the authorization table's receipt-kind
  column was captured by the parser and discarded, in no category and checked by nothing.
  Round three then found a **seventh**, and it is the one that settles the pattern: the `timestamp`
  vocabulary row was `Derived`, parsed, and checked by nothing — four independent falsifications left
  the gate green, including deleting the row outright. The mechanism was a parser dropping any row
  with no backticked value (`if vals:`) feeding a loop over a hardcoded list of five names.
  **That list had been widened from three to five in the previous round, in the repair for round one's
  identical finding, and the prose asserting it read "every row of the vocabulary table."** The critic's
  verdict on it — *eloquence outran correctness* — is the most accurate sentence written about this
  cycle. **The repair for an over-claim over-claimed.**

  All seven are repaired; the gate now runs twenty-one document mutations over thirty-eight checks.
  The round-three repair treats the mechanism rather than the instance: the checked set is now
  **derived from the parsed table**, a row the gate does not probe is a failure rather than a silent
  skip, and free-form rows are checked against the engine's own refusal tokens. Adding `timestamp` to
  the hardcoded five would have made six and left the next reader one row from an eighth.

  **Every one of the seven was filed under `Derived`** — the category promising the most — while
  `Transcribed` held up under two subsequent reviews and the literal-only claims behaved exactly as
  documented. The transferable lesson is in the document: **a gate's inversion evidence is itself a
  claim that can be vacuous, and over-claiming lands on whichever bucket promises the most.**
- **Evidence:** file:scripts/test-review-contract-coherence.sh; command:bash scripts/test-review-contract-coherence.sh
- **Version:** n/a — this cycle's own artifact.
- **Owner:** EHotwagner/S.I.R. — scripts/test-review-contract-coherence.sh
- **Recurrence:** new, and then recurring once inside its own repair.
- **Avoidable cost:** one full review round, plus one report-critique round.
- **Disposition:** product fix

#### §4.8 The review-wait entry is validated in two layers with disjoint rules

- **Kind:** documentation
- **Impact:** A contract recovered from either layer alone states some real rules, omits others, and
  reads as complete. The first write-up asserted a rule the validator does not have and omitted a hard
  ceiling that would refuse a generously sized window.
- **Expected:** The documented `enter` constraints are the enforced constraints.
- **Observed:** `ReviewWait.validate` enforces a 24-hour window ceiling and `expiresAt > enteredAt`, and
  does not bound the window relative to now. The CLI gate enforces item identity, claim currency,
  `enteredAt <= now + 5min`, and `expiresAt > now`. Neither layer enforces the other's rules.
- **Evidence:** file:docs/coordination-engine-contracts.md; command:bash scripts/test-review-contract-coherence.sh; command:ilspycmd -r ~/.nuget/packages/fs.gg.coord.cli/0.71.0/tools/net10.0/any ~/.nuget/packages/fs.gg.coord.cli/0.71.0/tools/net10.0/any/fsgg-coord-engine.dll -t FS.GG.Coord.Cli.Client
- **Version:** 0.71.0
- **Owner:** EHotwagner/S.I.R. — docs/coordination-engine-contracts.md
- **Recurrence:** new; same root cause as the `subject` and oracle-claim errors in the same document.
- **Avoidable cost:** part of one review round.
- **Disposition:** doc fix

#### §4.9 A pure validator parameterised on the fact you need cannot reveal it

- **Kind:** positive-pattern
- **Impact:** Naming this limit is what prevents the next agent repeating three errors made in this
  cycle. It generalises well beyond this engine.
- **Expected:** n/a — a method result, not a defect.
- **Observed:** `validateReviewLedger` takes `expectedSubject` as a parameter, so both sides of its
  subject comparison come from the caller. It accepts the canonical item ref, the production
  `<canonical>/pr/<n>` form, and the literal string `not-even-a-ref` on identical terms. Converging
  against it cannot discover the subject rule the CLI derives.
- **Evidence:** file:docs/coordination-engine-contracts.md; command:bash scripts/test-review-contract-coherence.sh
- **Version:** 0.71.0
- **Owner:** FS-GG — generalisable guidance for contracts recovered in-process.
- **Recurrence:** new
- **Avoidable cost:** none — this is the repair for §4.7 and §4.8.
- **Disposition:** doc fix

#### §4.10 A critic checking out the reviewed SHA inside the implementer's worktree destroys uncommitted work

- **Kind:** friction
- **Impact:** The implementer's branch pointer was silently relocated and stayed detached for hours. A
  push from that state reports success while pushing nothing, so the failure mode is silent loss rather
  than a visible error — worse than reverted files, which at least show in `git status`.
- **Expected:** A critic reviewing an exact head SHA works in its own checkout.
- **Observed:** The worktree's HEAD reflog records
  `checkout: moving from item/255-review-contract-divergence to 72d40c0` — a detached checkout of the
  exact reviewed head SHA, inside the implementer's live worktree, seconds before the critic's review
  record was posted, with no re-attach until the rebase hours later. That is the whole claim, and the
  reflog carries it. The implementer separately observed applied edits reverted to HEAD on two
  occasions; only one HEAD-moving checkout appears in the reflog, so the second reversion is noted as
  an observation and forms no part of this finding.
- **Evidence:** command:git -C . reflog show HEAD --date=iso
- **Version:** n/a — orchestration.
- **Owner:** EHotwagner/S.I.R. — critic dispatch guidance.
- **Recurrence:** new
- **Avoidable cost:** a branch left detached and discovered only at rebase; edit passes re-applied.
- **Disposition:** skill fix

#### §4.11 Two critics were dispatched to one review generation

- **Kind:** orchestration
- **Impact:** Two critic identities in one generation are refused outright by the same-critic
  continuity rule. The ledger survived only because the second critic was redirected to read-only
  before it wrote.
- **Expected:** One role owns critic dispatch.
- **Observed:** Two critic identities were live against one PR head; a second identity distinct from
  the recording critic reported the oracle's action on the item while stating it had not acted on it.
  That two separate dispatchers each acted without knowing of the other is the implementer's own
  account and carries no artifact beyond it; the collision itself does.
- **Evidence:** file:feedback/checkpoints/item-255-review-contract-divergence.jsonl
- **Version:** n/a — orchestration.
- **Owner:** EHotwagner/S.I.R. — work-board host-loop and pnext-item review-handoff.
- **Recurrence:** new
- **Avoidable cost:** one duplicated review; no ledger damage.
- **Disposition:** skill fix

#### §4.12 The repository's `cross-repo-coordination` mirror has silently diverged from the Kit skill

- **Kind:** defect
- **Impact:** The skill an agent loads and the skill committed in the repository are different
  documents. Guidance verified against one is not thereby verified against the other, and a reviewer
  checking the repository copy will conclude a template does not exist when the loaded skill shows it.
  That happened during this cycle's own report review.
- **Expected:** The repository mirror of a Kit-distributed skill matches the distributed skill, as the
  `pnext-item` and `work-board` mirrors do.
- **Observed:** `.claude/skills/cross-repo-coordination/references/deep-detail.md` is 409 lines and
  contains **zero** occurrences of `intake`. The loaded
  `~/.claude/skills/cross-repo-coordination/references/deep-detail.md` is 429 lines and carries the
  entire `fsgg.coord.intake/v1` draft template, including the lowercase `severity` vocabulary of §4.5.
- **Evidence:** command:wc -l .claude/skills/cross-repo-coordination/references/deep-detail.md ~/.claude/skills/cross-repo-coordination/references/deep-detail.md; command:grep -c intake .claude/skills/cross-repo-coordination/references/deep-detail.md
- **Version:** measured 2026-08-23 against the workspace as checked out.
- **Owner:** EHotwagner/S.I.R. — the skill mirroring that keeps `.claude/skills/` current with the Kit.
- **Recurrence:** new; same class as §4.1 — a packed projection drifting from its source — and the
  reason this cycle verified `pnext-item`/`work-board` mirror parity explicitly after every edit.
- **Avoidable cost:** one incorrect `unsupported` disposition during report review, corrected here.
- **Disposition:** issue

#### §4.13 A CI dispatch route was added that could never report a pass, and two mirrors asserted it worked

- **Kind:** defect
- **Impact:** The gate this cycle exists to deliver was described in both packed skill mirrors as
  dispatchable through CI. It was not: the route exits 1 on a correct document and 1 on a falsified
  one, so it cannot report a pass and cannot distinguish pass from fail. A worker trusting the mirrors
  would have believed a check was available that could never go green.
- **Expected:** A documented dispatch route either works, or is not documented as working.
- **Observed:** `bash scripts/run-ci-gate.sh review-contract <out>` exits 1 with no receipt, failing
  `ci-route: unknown gate result:review-contract`. The subject was added to `run-ci-gate.sh`'s case
  statement without being added to `ci-route.mjs`'s `gateOrder`, `gateParts`, `expectedBuildInvocations`
  or the join's `expectedCommand` branch — none of which is in this item's declared `Paths:`.
- **Evidence:** command:bash scripts/run-ci-gate.sh review-contract artifacts/ci/results/review-contract.json; file:scripts/run-ci-gate.sh
- **Version:** n/a — this cycle's own artifact.
- **Owner:** EHotwagner/S.I.R. — the wiring belongs to EHotwagner/S.I.R.#265, which owns the four files
  that must change together.
- **Recurrence:** new, and the sharpest instance of this item's own subject: a change whose purpose is
  contracts that assert things which are not true, shipping a route that could not be green with
  mirrors saying it was.
- **Avoidable cost:** part of one review round; the half-wiring was removed rather than completed,
  because completing it needs `ci-route.mjs`, `qualify-pr.sh`, `test-ci-route.mjs` and `ci.yml` to
  change together and three of those are outside this item's touch-set.
- **Disposition:** product fix

#### §4.14 The contract page presented a hand-picked subset as complete, and inverted one field

- **Kind:** defect
- **Impact:** A host authoring its first acceptance record from this page would have been refused. The
  page told it `acceptedExceptions` "records exceptions the host is knowingly accepting"; the engine
  refuses exactly that with `accepted exceptions belong to critic review records, not host acceptance`.
  Separately, a successor critic could not obtain its `round` from the page at all — which is this
  item's own AC1 ("an agent following only the packed skills can drive a reviewed PR to a landable
  green verdict") failing on the artifact written to satisfy it.
- **Expected:** A page that says the validator "names every invariant it enforces" carries them, or
  does not say so.
- **Observed:** The page listed **8 of roughly 40** invariants while asserting completeness, and both
  skill mirrors repeated the claim. Two of the omissions are load-bearing: `confirmation round must be
  contiguous within its generation` (first confirmation must be 1, second 2 — measured), and the
  same-critic rule's exception permitting a *different* critic to confirm after a `changes-required`
  verdict (measured: accepted after `changes-required`, refused after `pass`). **This PR's own review
  chain used that exception twice.**
- **Evidence:** file:docs/coordination-engine-contracts.md; command:bash scripts/test-review-contract-coherence.sh
- **Version:** 0.71.0
- **Owner:** EHotwagner/S.I.R. — docs/coordination-engine-contracts.md and both `pnext-item` mirrors.
- **Recurrence:** new as a finding; the same over-claiming shape as §4.7, in the contract rather than
  in the gate. A list presented as complete is what stopped the inverted field being noticed for two
  rounds — completeness claims suppress the checking that would catch their own contents.
- **Avoidable cost:** part of one review round; the acceptance draft it would have refused was the
  host's next action.
- **Disposition:** doc fix

## §5 Did not exercise

- Host acceptance (`review record` of kind `acceptance`) and a green `landable` — the host's acts.
- The repair-phase path, critic succession, and the diff-audit receipt path — none was reached.
- Any SDD lifecycle stage — correctly; the route was `lightweight` with `sddWorkId: null`.
- Any product runtime surface — this change ships no runtime behaviour.

## §6 Doc-versus-behavior contradictions

1. **Packed skill vs engine.** `independent-review.md` presented the durable prose markers as the
   review evidence; `landable` gates on `fsgg.coord.review-decision/v2`. Owner: the packed `pnext-item`
   skill. Repaired.
2. **Refusal message vs check.** `meaningful route evidence must contain built artifact, command,
   comparison, and result` vs a pure cardinality test. Owner: FS-GG/.github. Filed as #2834.
3. **Command help vs behaviour.** `intake apply` — "atomically project one receipt-bound filing draft"
   vs a partial write leaving an issue created but unlabelled and off-board. Owner: FS-GG/.github.
   Filed as #2835.
4. **Oracle prose vs engine.** The document and both skill mirrors said the review oracle needs a claim
   *you hold*; the engine requires only that a live claim marker exists. Repaired.
5. **Feedback skill prose vs its own validator.** `fs-gg-feedback-report` states that "a required
   finding left `incomplete` or `unsupported` may remain as an observation". Its validator disagrees:
   `FeedbackReportTool.fs:704-710` raises a hard error for **any** finding whose audit status is
   `incomplete` or `unsupported`, with no disposition or wording that permits it. So a report carrying
   an honestly reduced disposition cannot validate, and the only ways forward are to strengthen the
   evidence or to drop the finding — never to keep it as the observation the prose invites. Encountered
   in this cycle on §4.10, which was resolved by narrowing the claim to what its evidence carries.
   Owner: FS-GG — the feedback skill and its validator, whose two mirrors are already contested in
   EHotwagner/S.I.R.#267.

## §7 Workarounds still in the tree

- `scripts/test-review-contract-coherence.sh` resolves `DOTNET_ROOT` itself rather than trusting the
  inherited value, because the ambient environment points at an install lacking the SDK `global.json`
  pins. Removal condition: the workspace exports a `DOTNET_ROOT` containing the pinned SDK. Risk if
  permanent: low; the resolution is explicit and fails closed naming the pin.
- The gate runs only when a human runs it: `bash scripts/test-review-contract-coherence.sh`. Nothing
  invokes it automatically. An earlier revision of this change added a `review-contract` case to
  `scripts/run-ci-gate.sh` and described the gate as "dispatchable", but that route could never report
  a pass — `ci-route.mjs` refuses an unknown subject, so it exited 1 on a correct document and on a
  falsified one alike. The half-wiring was removed rather than left in place asserting more than it
  delivered. Removal condition: EHotwagner/S.I.R.#265, which owns the four files that must change
  together (`ci-route.mjs`, `qualify-pr.sh`, `test-ci-route.mjs`, `ci.yml`). Risk if permanent: the
  gate silently stops being run, which is this item's own defect class.

## §8 Friction and avoidable cost

- Three review rounds consumed by §4.7, §4.8, §4.13 and §4.14, plus one report-critique round: the
  gate's own repair carried seven vacuous assertions across the three rounds, a dispatch route was
  shipped that could never report a pass, and the contract page presented a subset as complete while
  one field was documented backwards.
- A detached checkout inside the live worktree left the branch pointer relocated until the rebase, and
  cost re-applied edits (§4.10).
- One duplicated critic review (§4.11).
- One partially created upstream issue completed by hand (§4.5).
- Two feedback checkpoints rejected for an invalid `surface` value — `orchestration` is a valid `kind`
  but not a valid `surface`; re-recorded under `worker-git-pr`.
- One F# probe silently rewritten by an unquoted heredoc executing backtick-quoted words from F#
  comments as shell commands. The run still exited 0, so the corruption was visible only as wrong
  expected values.

## §9 Skill value and gaps

**Invoked with evidence.** `pnext-item` (the worker state machine, followed start to finish);
`fs-gg-feedback-report` (15 checkpoints, this report); `cross-repo-coordination` (two Kit rows filed
through `intake validate`/`apply`).

**Relevant, not invoked, and why.** All `fs-gg-sdd-*` stages — the delivery route was `lightweight`
with `sddWorkId: null`, so no stage was owed. All `fs-gg-*` product skills and both `sir-*` rule skills
— this change touches no game or rules surface.

**Wanted and absent.** A packed skill or reference documenting the coordination engine's own document
schemas. `facts --json` publishes `reviewPolicy` and the complete `fsgg.coord.planning-receipt/3`
schema in `ledgerPolicy`, but no skill projects them, so each is rediscovered by decompilation.
`docs/coordination-engine-contracts.md` is a partial local answer.

**Misleading skill guidance.** `independent-review.md`'s review-evidence section before this change
(§4.1); the Kit-distributed `cross-repo-coordination` skill's `fsgg.coord.intake/v1` draft template,
whose lowercase `severity` vocabulary the board field rejects (§4.5) — and which the repository's own
mirror of that skill does not contain at all (§4.12).

## §10 Outcome markers

- Claim to converged claim receipt: under one minute (estimate — the claim marker on #255 is stamped
  21:30:31Z and no separately timestamped receipt artifact exists to difference against).
- Claim to the complete recovered contract published to the host: approximately 25 minutes (estimate).
- Wait-open to critic record: **11 minutes 35 seconds** (21:51:44Z to 22:03:19Z) — measured, and the
  basis for the board's default wait-window sizing.
- First review verdict: `changes-required`, five material findings, plus two more from a second
  read-only reading.
- Regression gate at the final head: all derived claims green, nine document mutations red, thirteen
  literal-only claims present. Command duration approximately 2 minutes, dominated by `dotnet fsi`.
- Merge and done stamp: pending host acceptance at the post-rebase head.

## §11 Falsifiable improvements

1. **Project the engine's `facts --json` schemas into the packed skills.** Would have prevented §4.1 and
   the `planning-receipt/3` rediscovery. Owner: FS-GG/.github, the projection generator. Acceptance:
   grepping the packed skills for `fsgg.coord.review-decision/v2` returns a hit describing how to
   produce one.
2. **Make every engine refusal name its producing command.** Would have prevented §4.3. Owner:
   FS-GG/.github. Acceptance: the review-acceptance refusal names `review wait` and `review record` in
   order.
3. **Make `intake apply` atomic or resumable.** Would have prevented §4.5. Owner: FS-GG/.github.
   Acceptance: a value-rejecting apply leaves no visible upstream write, or reports what it created,
   and the corrected draft re-applies under the same id.
4. **Require a doc-defending gate to derive its expectations from the document.** Would have prevented
   §4.7. Owner: EHotwagner/S.I.R., the `pnext-item` gate-inversion rule. Acceptance: the rule states
   that mutating the gate's own expectation is predicate inversion, and only mutating the subject counts
   as subject inversion.
5. **Dispatch a critic into its own worktree, never the implementer's.** Would have prevented §4.10.
   Owner: EHotwagner/S.I.R., critic dispatch guidance. Acceptance: the critic contract states it, and a
   critic's first action is creating its own checkout, verified by the absence of any HEAD-moving
   checkout in an implementer worktree's reflog during review.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | No scaffold or template was generated; the workspace pre-existed. |
| onboarding-guidance | partial | The pinned SDK is not on `PATH` and the ambient `DOTNET_ROOT` lacks it; the dispatch brief carried the workaround, no packed guidance did. |
| skills | exercised | `pnext-item` followed start to finish; `cross-repo-coordination` filed two rows; the review-evidence section was found materially wrong (§4.1). |
| sdd-authoring | not-exercised | Route `lightweight`, `sddWorkId: null`; no stage owed. |
| implementation-apis | exercised | `FS.GG.Coord.Core` public API driven directly — encoders, validator, digest, generation token. |
| dependencies-build | partial | No product build; the pinned engine was resolved from the tool manifest and loaded by `dotnet fsi`. |
| testing | exercised | One gate added, found decorative in review, rebuilt to derive from its subject; nine document mutations required to red. |
| evidence | exercised | 15 checkpoints; gate-inversion evidence committed in-tree and runnable from the repository. |
| runtime-playtest | not-exercised | No runtime surface in this change. |
| performance | not-exercised | No performance-sensitive surface. |
| documentation | exercised | `docs/coordination-engine-contracts.md` authored, reviewed, found wrong in three places, repaired. |
| packaging-upgrade | not-exercised | No package version changed. |
| worker-git-pr | exercised | Claim, widen (four times), branch, PR, review ledger, rebase, two Kit filings; one dispatch collision (§4.11) and one detached-checkout event in the live worktree (§4.10), both reflog- or ledger-corroborated. |
