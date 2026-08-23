---
feedbackSchema: 2
date: 2026-08-23
workspace: S.I.R
cycle: item-280-pr-verdict-timing-ceiling
lane: none
toolVersion: 1.0.1
commit: acd7b2eb21cdcd95b4b74f342e0f06ee75f2362b
---

# S.I.R. item 280 — pr-verdict timing ceiling

## §1 Provenance and confidence

- **Cycle:** `item-280-pr-verdict-timing-ceiling`; board item `S.I.R.#280` (High, defect); PR #281.
- **Worker:** `heron-a9b8`, minted via `scripts/fsgg-coord whoami --mint`, single claim, converged.
- **Commit described:** `acd7b2eb21cdcd95b4b74f342e0f06ee75f2362b`.
- **Lane:** `none`. No SDD package governs this item; the delivery-route receipt (revision 1, digest
  `561aa65109c3a91eced31a87ad442dbc1ee62d0c22a6aebb2d68097a3349a893`) recorded route `lightweight`
  with `sddWorkId: null`, so charter→analyze was correctly not run.
- **Tool pins:** `fsgg-sdd` 1.0.1; `fs.gg.coord.cli` 0.71.0 pinned in `.config/dotnet-tools.json`
  while the resolved engine assembly on disk is 0.72.0.
- **Checkpoints:** `feedback/checkpoints/item-280-pr-verdict-timing-ceiling.jsonl`, 5 events,
  validated by `validate-checkpoint-state`.
- **Activation:** capture was active across implementation-test-evidence and verify-ship-pr.
  Scaffold/onboarding was not a phase of this cycle (existing repository, existing worktree flow).
- **Confidence limits:** per-job durations come from the Actions REST API for run 32607930272
  attempt 2. The `pr-verdict` artifact for that run was not retrievable (`404` on the attempt-scoped
  artifacts endpoint), so per-gate receipt totals were not read directly; the wave maxima used for
  calibration are job wall times, which are an upper bound on the receipt totals they stand in for.
  This makes the derived budget conservative rather than generous, but it is an approximation and is
  named as one.
- **Three distinct durations describe run 32607930272 attempt 2 and must not be conflated:** its true
  dependency critical path is 263 s (each job scheduled the instant its dependencies complete); the
  per-wave-maxima bound this join enforces is 279 s, deliberately looser because it charges the
  slowest subject in each wave even when that subject is not on the path; and the scored wall clock
  was 470 s. An earlier revision of this report and of `docs/ci-qualification.md` used 272 s and
  labelled 279 s a "critical path". Both were corrected in `acd7b2e` after independent critique.
- **Critic:** fresh-context subagent, `actionability-v1`. It contradicted §4.5's headline mechanism
  and owner, corrected §4.2's queue figure, corrected §4.1's recurrence count, reclassified §4.4 as a
  duplicate, and identified an undisclosed tension in §4.3 that is now §4.7. Those corrections are
  incorporated below rather than argued with.

## §2 What worked

`scripts/test-ci-route-mutations.sh` is the single most valuable artifact in this cycle. It copies
the sources into an isolated fixture tree, applies one `sed` mutation, and asserts the route contract
goes red. Adding eleven inversions for a newly-shaped gate cost one function call each and produced
verified gate-inversion evidence in a single command. Because a `sed` that matches nothing leaves the
fixture unmutated and the suite green, the harness also self-verifies that each mutation actually
applied — a property worth copying to other mutation harnesses.

Writing the acceptance boundary as a runnable probe *before* touching the implementation paid for
itself twice. It first reproduced the exact reported failure (`feedback-budget-exceeded actual
469739 budget 300000`) in a unit fixture, which confirmed the fixture was faithful. It then caught
that the first version of the fix was incomplete: the budget had been shaped to the dependency
graph, but the 20% reserve was still applied only to `cross-cutting` and `performance`, so at equal
wave depth a seven-gate route was still held tighter than a two-gate one. A green PR would have
hidden that, because the PR's own run passes either way.

`gateParts` already encoded the exact producer-consumption relation the wave model needed, so the
model is a projection of existing structure rather than a new parallel description of the workflow.

## §3 What did not

The issue body's quantitative reconstruction was wrong and was already load-bearing: its 353s figure
had been copied into the route-decision receipt's `rationale`. Building the fix on it would have
raised the 300s outer budget, which the measured data does not support.

The first fix attempt was incomplete in the way described in §2. It was caught by the cycle's own
test rather than by review, which is the intended order but was not guaranteed by anything except
having written the invariant as an assertion.

The report as first drafted argued that the 300 s budget was satisfiable and must not be raised, and
did not disclose that the delivered fix raises it to 390 s anyway. That omission was the critic's
principal finding and is now §4.7. The two positions are reconcilable but only because the measured
quantity changed underneath the constant, and a report that states one without the other is
self-serving.

Bringing `scripts/audit-binding-exceptions.json` current required two attempts: the first rewrote the
whole file because `json.dumps` defaults to `ensure_ascii=True`, escaping `§` and `—` throughout and
producing a 44-line diff for a 4-line change. Verifying the round-trip was byte-identical before
editing would have caught it immediately.

## §4 Findings

#### §4.1 The pr-verdict feedback ceiling was unsatisfiable for every cross-cutting PR, and has been repaired at least six times by making gates faster

- **Kind:** defect
- **Impact:** Every `cross-cutting` PR — the broadest and most review-sensitive class of change — was
  structurally unmergeable. At the time of this cycle S.I.R.#264 and S.I.R.#250 were both parked
  behind it, the latter with a passed review.
- **Expected:** A fully green run whose gates all pass within a realistic critical path is admitted.
- **Observed:** `cross-cutting` selected `[...gateOrder]`, the maximal gate set, and was
  simultaneously one of the two classifications whose 300s budget was replaced by a 240s acceptance
  target. The classification selecting the most gates carried the shortest deadline. Three
  independent runs failed only this check, at elapsed values of 243,224 ms (PR 257 head `0981c169`,
  recorded by critic `curlew-cac2` in a comment on S.I.R.#280), 283,553 ms (run 32434802616, recorded
  in `…repair-phase-2.md`), and 469,739 ms (run 32607930272 attempt 2, in the S.I.R.#280 body), all
  against the same 240,000 ms target — a 226,515 ms spread of elapsed values, not of margins, on a
  metric supposed to measure the same pipeline. A fourth, run 32609825054 at 259,611 ms, fires
  `feedback-headroom-eroded` alone while `feedback-budget-exceeded` does not, which is the signature
  of the target being the unsatisfiable half.
- **Evidence:** issue:EHotwagner/S.I.R.#280; file:feedback/2026-08-21-sir-item-220-ci-budget-repair-phase-2.md
- **Version:** Reproduced at `f69f1e6cdc203121bd908a3a1bd025545e0aff56`; repaired at `33628aacc7bd0a0bf18fec3341167f614e740c29`.
- **Owner:** S.I.R./scripts/ci-route.mjs `joinRoute` timing block
- **Recurrence:** seen again; first seen feedback/2026-08-21-sir-item-220-ci-budget-repair-phase-2.md, then four further `item-220-ci-budget-repair-phase-*` reports (six exist in total; `-phase.md` precedes `-phase-2` but records no 240,000 ms overrun, so `-phase-2` is the first record of this symptom); S.I.R.#280
- **Avoidable cost:** Five prior repair-phase cycles that observed this symptom optimized gate work to
  fit a ceiling that could not be met, rather than correcting the ceiling. That is the dominant
  avoidable cost in this record.
- **Disposition:** product fix

#### §4.2 The scored quantity charged each diff for runner queue time it does not control

- **Kind:** defect
- **Impact:** The same tree passed or failed on unrelated fleet load, so the check was
  non-deterministic and a retry was indistinguishable from a repair.
- **Expected:** A timing gate on a pull request measures work attributable to that pull request.
- **Observed:** `elapsed` was wall clock from the `route` job's first step to the join. On run
  32607930272 attempt 2, `domain-conformance`'s last dependency (`prepare-web`) completed at 00:35:20
  and the job started at 00:38:47: a **207 s** wait for a hosted runner, inside a scored 469,739 ms.
  (An earlier revision of this finding said 191 s. That figure is wall clock minus the enforced
  per-wave bound, which is not a queue measurement; 207 s is the dependency-satisfied-to-start
  interval and is the number meant here.) Critic `curlew-cac2` independently measured the
  complementary case on PR 257 (head `0981c169`): gate work
  identical to within 0.14% (`observedGateCriticalPathMilliseconds` 127,606 → 127,787), total runner
  milliseconds *lower* (1,260,210 → 1,256,145), and the scored number still crossed the threshold
  (236,135 → 243,224). This cycle's own run 32609923345 closes the loop from the other side: the same
  pipeline queued 3,232 ms on an idle fleet against ~207,000 ms on a saturated one.
- **Evidence:** command:gh api repos/EHotwagner/S.I.R./actions/runs/32607930272/attempts/2/jobs; issue:EHotwagner/S.I.R.#280
- **Version:** Reproduced at `f69f1e6cdc203121bd908a3a1bd025545e0aff56`.
- **Owner:** S.I.R./scripts/ci-route.mjs `joinRoute` timing block
- **Recurrence:** new; folded into S.I.R.#280 rather than filed separately, because it shares a root
  cause with §4.1 and neither fix is sufficient alone
- **Avoidable cost:** none in this cycle beyond the analysis itself
- **Disposition:** product fix

#### §4.3 The issue body's queue-free reconstruction serialized two structurally parallel jobs

- **Kind:** quality-gap
- **Impact:** The stated conclusion "353s > 300s budget" would have justified raising the outer
  budget, weakening a check that the measured data shows was working. The figure had already been
  copied into the item's route-decision receipt rationale, so it was propagating.
- **Expected:** A critical-path reconstruction follows the workflow's `needs:` graph.
- **Observed:** The body added `domain-conformance` after the parallel gate block. Both
  `domain-conformance` and `browser` need only `[route, prepare-native, prepare-fable, prepare-web]`
  and are structurally concurrent. Scheduling every job the instant its dependencies complete gives
  route 12 s → `prepare-web` completing at t=132 → `domain-conformance` 120 s to t=252 → `pr-verdict`
  11 s to **t=263**. The queue-free critical path is 263 s, not 353 s, so the 300 s budget was
  satisfiable and only the 240 s target was not. The body also gave the queue wait as 117 s where the
  dependency-satisfied-to-start interval is 207 s. (An earlier revision of this finding said 272 s.
  That is the sum of per-wave maxima, which charges `spatial-mutations` at 129 s even though it is not
  on the path; it is the bound the fix enforces, not the critical path, and the two are now
  distinguished throughout — see §1.)
- **Evidence:** command:gh api repos/EHotwagner/S.I.R./actions/runs/32607930272/attempts/2/jobs; file:.github/workflows/ci.yml
- **Version:** n/a
- **Owner:** S.I.R./issue triage practice
- **Recurrence:** new
- **Avoidable cost:** none — caught before implementation — but only because the reconstruction was
  re-derived from the API rather than trusted
- **Disposition:** accepted

#### §4.4 The durable review-wait entry gating critic dispatch has no documented event schema

- **Kind:** capability-gap
- **Impact:** A worker cannot reach `dispatchCritic` from any packed documentation. The review gate is
  unreachable without reverse-engineering the engine.
- **Expected:** The command that the oracle names as the required next action is documented, or its
  refusals name the producing value.
- **Observed:** `scripts/fsgg-coord review <ref> --pr N` returned `noVerdict` with
  "dispatchCritic requires a durable review-wait entry before dispatch". No packed skill documents
  `review wait` or its event shape. Recovering it required loading `FS.GG.Coord.Core.dll` and
  reflecting `ReviewWait.encode`/`generationToken`/`WaitReceipt`. Two field values still had to be
  guessed after that: `item` must be the fully qualified `EHotwagner/S.I.R.#280` rather than the
  board's own `S.I.R.#280` ref, and `claimGeneration` is the claim marker's GitHub comment id. Each
  refusal named the mismatch but never the producing value or command. The round index also had to be
  `0`, discovered only because the oracle echoed the expected generation string on failure — which is
  the one refusal in this family that did name what it wanted.
- **Evidence:** issue:EHotwagner/S.I.R.#255; file:feedback/2026-08-22-sir-item-252-audit-binding-ledger-currency.md
- **Version:** `fs.gg.coord.cli` pinned 0.71.0; resolved engine assembly on disk 0.72.0.
- **Owner:** FS-GG Kit/FS.GG.Coord.Cli `review wait`, and the packed `independent-review` contract in
  the three skill mirrors
- **Recurrence:** seen again; S.I.R.#255 carries the contract divergence and S.I.R.#279 carries a
  closely related `review wait` defect (a durable entry appended and then reported malformed, so a
  retry double-appends). The identical mechanism is also already a filed finding in
  feedback/2026-08-22-sir-item-252-audit-binding-ledger-currency.md §4.4, which this report did not
  find on its first pass — an instance of the gap S.I.R.#278 exists to describe, that the contract
  dedupes against issues but not against prior cycle reports.
- **Avoidable cost:** six refused `review wait` invocations plus one assembly-reflection detour
- **Disposition:** existing issue

#### §4.5 The drift-ledger refusal names the stale binding but not the remedy, and is emitted by a repo that the report first mis-attributed

- **Kind:** friction
- **Impact:** A change to a digest-attested file fails the integrity gate with a message that states
  the fact and not the fix, costing an investigation to establish what is owed and to whom.
- **Expected:** The refusal names the remedy, or the obligation is visible when the attested file is
  edited rather than at CI.
- **Observed:** Editing `scripts/ci-route.mjs` and `scripts/test-ci-route.mjs` invalidated their
  `replacementSha256` entries in `scripts/audit-binding-exceptions.json`, and the integrity gate
  refused with "exception replacement evidence is stale for file:scripts/ci-route.mjs". The remedy —
  record the new digest and extend the entry's `reason` — is not stated. The audit passes on a clean
  `origin/main` and failed only for this change, so the obligation is real and correctly this
  branch's, and is not S.I.R.#252.
  Two corrections to this finding's first revision, both from independent critique. First, its
  headline claimed the subject fires "only for routes broad enough to select `feedback-audit`". That
  is wrong: `scripts/ci-integrity-plan.mjs:19` selects `feedback-audit` by **path relevance**, and its
  list names `scripts/ci-route.mjs` and `scripts/test-ci-route.mjs` explicitly. The obligation fired
  because those files were touched, not because the route was broad — which is also what this
  finding's own Observed line always said. Second, the owner was wrong: the message is emitted by the
  FS-GG Kit at `.agents/skills/fs-gg-feedback-report/scripts/FeedbackReportTool.fs:558`, and
  `scripts/test-feedback-audit-binding-exceptions.sh` does not contain the string at all. Routing the
  remedy to S.I.R. would have sent it to a repo that cannot fix it.
- **Evidence:** file:scripts/ci-integrity-plan.mjs; file:.agents/skills/fs-gg-feedback-report/scripts/FeedbackReportTool.fs
- **Version:** Observed at `b98ab8badee2d8e683f8a3a9cf98ad00b5b255d8`; resolved at `33628aacc7bd0a0bf18fec3341167f614e740c29`.
- **Owner:** FS-GG Kit/fs-gg-feedback-report `FeedbackReportTool.fs` invalidation message text
- **Recurrence:** new
- **Avoidable cost:** one failed CI run plus one investigation to separate it from S.I.R.#252
- **Disposition:** doc fix

#### §4.6 scripts/test-ci-product-performance-route.sh fails silently in a fresh worktree

- **Kind:** friction
- **Impact:** A worker validating in a fresh worktree sees exit 1 with no output and must determine
  whether it is a regression from their own change.
- **Expected:** A harness with a build-state precondition either builds, skips, or says what is missing.
- **Observed:** The script runs `dotnet run --project ... --no-build --no-restore` and redirects both
  streams to a temporary log that `trap ... EXIT` deletes, so the failure surfaces as a bare exit 1.
  It passes in the primary checkout, which has build output, and fails in a fresh worktree, which does
  not.
- **Evidence:** command:bash scripts/test-ci-product-performance-route.sh; file:scripts/test-ci-product-performance-route.sh
- **Version:** Observed at `33628aacc7bd0a0bf18fec3341167f614e740c29`.
- **Owner:** S.I.R./scripts/test-ci-product-performance-route.sh
- **Recurrence:** new
- **Avoidable cost:** one investigation cycle
- **Disposition:** issue

#### §4.7 This cycle raises the outer budget it also argues was never the defect

- **Kind:** quality-gap
- **Impact:** Read alone, §4.3 says the issue's arithmetic "would have justified raising the outer
  budget, weakening a check that the measured data shows was working", while the delivered change
  raises that budget from 300 s to 390 s for every multi-wave route. A reviewer who noticed the second
  fact and not the reconciliation would be right to read the report as self-serving.
- **Expected:** A report that argues against raising a constant discloses that it raised it.
- **Observed:** `scripts/ci-route.mjs` now derives `feedbackBudgetFor = max(300_000, 30_000 +
  waveCount * 180_000)`, so a two-wave route carries 390 s and, with the uniform 2000 bp reserve, a
  312 s acceptance target. The reconciliation is that the two constants do not measure the same thing.
  The 300 s budget was applied to wall clock, which includes runner queue; against that quantity it
  was working, and raising it would have absorbed queue time into the budget and made the check
  vacuous — which is what the issue's arithmetic would have produced. The 390 s budget is applied to
  the attributable per-wave-maxima path, which excludes queue and which no prior constant was
  calibrated against. On the measured worst case those are 470 s and 279 s respectively; the new
  ceiling is 111 s above the quantity it judges, where the old one was 170 s below the quantity it
  judged. Both facts are true and neither is a substitute for the other, so both belong in the record.
- **Evidence:** file:scripts/ci-route.mjs; file:scripts/test-ci-route.mjs
- **Version:** Introduced at `33628aacc7bd0a0bf18fec3341167f614e740c29`.
- **Owner:** S.I.R./scripts/ci-route.mjs, and this report
- **Recurrence:** new
- **Avoidable cost:** none; caught at critique before the report was preserved
- **Disposition:** accepted

## §5 Did not exercise

- Scaffolding and onboarding: this is an existing repository; no scaffold was generated.
- SDD authoring: correctly not applicable, `sddWorkId: null` on a `lightweight` route.
- Runtime/playtest and packaging/upgrade: this change is confined to the CI harness and its contract
  documentation; no product runtime or package surface was touched.
- `docs/performance-budget.md`: in scope by subject but held by a live claim on S.I.R.#232, so it was
  deliberately not edited. Its statement that the fixed-function route "retains the shared 240-second
  target and 60-second headroom" is now stale; the holder was notified via `fsgg-coord say`.

## §6 Doc-versus-behavior contradictions

`docs/ci-qualification.md` stated: "Representative cross-cutting acceptance is stricter: it must
finish within 240 seconds, retaining at least 60 seconds of headroom for observed runner variance and
bounded producer-inventory growth." The measured queue-free critical path of the widest route this
pipeline can select is approximately 272 s, so the documented contract described a state the pipeline
could not reach. This is not a documentation error about an implementation — the documentation and the
implementation agreed, and both were wrong. Corrected in this cycle. Owner:
S.I.R./docs/ci-qualification.md.

`docs/performance-budget.md` still asserts the shared 240-second target for the fixed-function route.
Not corrected here; see §5.

This cycle also briefly introduced a contradiction of its own. The corrected `docs/ci-qualification.md`
first stated the run's "queue-free 279-second critical path" and a "191"-second queue. 279 s is the
per-wave-maxima bound the join enforces, not a critical path (263 s), and 191 s is wall clock minus
that bound, not a queue measurement (207 s). Three durations were in circulation across the report and
the document it shipped. Corrected in `acd7b2e`, which now names all three and says which is enforced.
Owner: S.I.R./docs/ci-qualification.md.

## §7 Workarounds still in the tree

None introduced by this cycle.

Pre-existing and still present: `scripts/ci-cost-report.mjs` carries `verdictMilliseconds: 240_000` as
a flat baseline target, the same constant and the same flat-constant shape this cycle removed from the
enforced path. It is report-only — the job exits non-zero on reconciliation status, not on
`baselineComparison.result` — so it blocks nothing, but it will now disagree with the enforced
contract. Removal condition: rederive it from the same wave model, or state explicitly that it is an
unshaped observational baseline.

## §8 Friction and avoidable cost

- Six refused `review wait` invocations plus one assembly-reflection detour (§4.4).
- One failed CI run and one investigation to attribute the `feedback-audit` failure correctly (§4.5).
- One investigation to attribute the `test-ci-product-performance-route.sh` failure correctly (§4.6).
- One extra `widen` call. `fsgg-coord widen` is atomic across the path batch: widening five paths
  where only `docs/performance-budget.md` collided with a live claim on S.I.R.#232 refused all five
  and applied none, with the returned `paths` array still showing the original touch-set. The refusal
  named the colliding path and notified its holder, so the retry was mechanical. Recorded here as an
  observation rather than as a finding: the only locator is a board-mutating command a reader cannot
  safely re-run, and no transcript was captured, so it does not meet the evidence bar for §4.
- One discarded ledger edit due to `ensure_ascii` (§3), self-inflicted, roughly one edit cycle.
- Two probe iterations to give fixture receipts phase splits the `gateResult` validator accepts —
  `test` is derived as `total - setup - restore - build - transport` and must not go negative.

The dominant cost is not in this cycle at all: six prior `item-220-ci-budget-repair-phase-*` cycles
spent on making gates faster to fit an unsatisfiable ceiling (§4.1).

## §9 Skill value and gaps

- `pnext-item`: invoked, followed literally. Its §3 instruction to ship gate-inversion evidence at
  authoring time rather than review time is what produced the §2 outcome, and it is the reason the
  incomplete first fix was caught by a test rather than by a critic.
- `intra-repo-parallel-work`: exercised through `widen`, `overlap`, and `say`. The "stop on overlap"
  rule worked exactly as written: it prevented an edit to a file held by another live claim, and the
  `say` channel let the correction reach its owner instead of being dropped.
- `fs-gg-feedback-report`: invoked; this report.
- `independent-review`: read; its documented prose-marker contract does not match the engine, which is
  §4.4 and S.I.R.#255.
- Not invoked and correctly so: the `fs-gg-sdd-*` lifecycle skills (`lane: none`), and
  `performance-first` — this is CI harness timing policy, not interactive or simulation work.
- Gap: no skill documents the v2 review ledger path that `landable` actually requires.

## §10 Outcome markers

- Time to first reproduction of the defect in a unit fixture: one probe iteration after the structural
  analysis; the probe reproduced the exact reported failure codes and `actual: 469739`.
- First green on the full route contract after the fix: same working session.
- Gate-inversion evidence: eleven mutations, all red, each confirmed to fail on an assertion rather
  than a syntax error (three spot-checked explicitly).
- Ship readiness: PR #281 opened, `verify-paths` OK, delivery obligations declared, review-wait entered,
  oracle at `dispatchCritic` / `awaitingInitialReview`.
- Estimates: elapsed developer time is not recorded precisely and is not estimated here.

## §11 Falsifiable improvements

1. **Refusals in the review-wait family should name the producing value or command.** Owner: FS-GG
   Kit/FS.GG.Coord.Cli. Would have prevented the six refused invocations in §4.4. Acceptance: a worker
   with no prior knowledge reaches `dispatchCritic` state from the oracle's own output in
   at most two invocations, with no assembly reflection. The oracle's generation-mismatch message,
   which does echo the expected string, is the model to copy.

2. **Derive the cost report's `verdictMilliseconds` target from the same wave model, or label it
   observational.** Owner: S.I.R./scripts/ci-cost-report.mjs. Would prevent the enforced and reported
   contracts drifting apart (§7). Acceptance: the two constants cannot disagree without a test failing.

3. **Make the drift-ledger refusal state its remedy.** Owner: FS-GG Kit/fs-gg-feedback-report,
   `FeedbackReportTool.fs:558`. Would have removed the §4.5 investigation. Acceptance: the message
   names the ledger file to edit and the field to update, not only the stale locator.

4. **Give `test-ci-product-performance-route.sh` a build-state precondition.** Owner: S.I.R. Would have
   removed the §4.6 investigation. Acceptance: run in a checkout with no build output, the script
   either builds or exits with a message naming the missing prerequisite; never a bare exit 1.

5. **When an issue body states a reconstructed measurement, cite the derivation.** Owner: S.I.R. issue
   triage. Would have prevented §4.3 propagating into the route-decision receipt. Acceptance: a
   reconstructed critical path names the dependency edges it assumes.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing repository; no scaffold generated this cycle. |
| onboarding-guidance | partial | The dispatch brief's three environment facts (DOTNET_ROOT export, the trailing dot in `S.I.R.`, non-persisting `whoami --mint`) were each load-bearing and each correct; without them the first `fsgg-coord` call fails with an unrelated-looking SDK resolution error. |
| skills | exercised | `pnext-item`, `intra-repo-parallel-work`, `fs-gg-feedback-report` invoked; see §9. One documented gap (§4.4). |
| sdd-authoring | not-exercised | `lane: none`; route receipt records `sddWorkId: null`, correctly. |
| implementation-apis | exercised | `joinRoute`'s existing `gateParts` structure carried the dependency relation the wave model needed; no new parallel description of the workflow was required. |
| dependencies-build | partial | `npm`/`node` paths exercised; the .NET build was not, and `--no-build` harnesses fail in a fresh worktree (§4.6). |
| testing | exercised | `scripts/test-ci-route.mjs` extended; eleven inversions added to `scripts/test-ci-route-mutations.sh`, all verified red on assertions. |
| evidence | exercised | Actions REST API used to re-derive per-job timings and refute the issue body's reconstruction (§4.3); the run's `pr-verdict` artifact was not retrievable (§1). |
| runtime-playtest | not-exercised | CI harness change; no product runtime surface. |
| performance | partial | The subject is CI feedback latency, measured and re-derived from real runs; no product-performance budget was touched, and `performance-first` was correctly not invoked. |
| documentation | exercised | `docs/ci-qualification.md` corrected; `docs/performance-budget.md` left stale by overlap and its holder notified (§5, §6). |
| packaging-upgrade | not-exercised | No package or version surface touched. |
| worker-git-pr | exercised | Mint, take, widen×3 (one refused on overlap), say, verify-paths, delivery declaration, review-wait ledger, PR #281. Friction in §4.4 and §4.7. |
