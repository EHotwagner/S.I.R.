---
feedbackSchema: 2
date: 2026-08-21
workspace: S.I.R-item-220-ci-budget-repair-phase
cycle: item-220-ci-budget-repair-phase
lane: sdd
toolVersion: 1.0.1
commit: 7e483cda7faa5ed9209f56d565aacf63ab630d23
---

## §1 Provenance and confidence

This immutable addendum covers repair confirmation round 2 and the round-3 source freeze for issue EHotwagner/S.I.R.#220 and PR #242. The cycle has five checkpoints at `feedback/checkpoints/item-220-ci-budget-repair-phase.jsonl`. The product remains on the existing S.I.R. scaffold and package locks; no package upgrade occurred. Confidence is high for the exact hosted failure, local topology, lifecycle, and focused browser results. Exact-head hosted timing for `7e483cda7faa5ed9209f56d565aacf63ab630d23` is intentionally pending and is not claimed.

## §2 What worked

Typed gate receipts isolated the remaining critical path: the new spatial-mutations and fixed delivery helpers both passed, while the general browser gate remained independently red. Test-granular sharding produced complete balanced 20/20 halves. The SDD lifecycle accepted a third plan revision without losing the prior repair history and returned implementation, evidence, verification, and ship readiness. Focused workflow dependency inversions now fail before hosted execution.

## §3 What did not

Round 2 did not reach acceptance. Hosted run 32438948032 joined at 269,386 ms, 29,386 ms above the 240,000 ms target, and the general browser gate failed one explicit disconnect/reconnect journey on a stale LongPolling `/hub/game?id=...` 404. Five first-pass focused repetitions did not reproduce the hosted diagnostic. Inspection found that `disconnect` stopped the connection and `reconnect` stopped the already stopped connection a second time. The workflow also lacked subject inversions proving that `pr-verdict.needs` named the helper jobs.

## §4 Findings

#### §4.1 SDD reports a coherent round-three endpoint

- **Kind:** positive-pattern
- **Impact:** The round-three source freeze has a current, reviewable lifecycle boundary instead of stale plan or evidence views.
- **Expected:** Source changes follow a regenerated, implementation-ready plan and finish with observed evidence plus verification and ship readiness.
- **Observed:** Tasks report 49 with two in progress, analysis reports 103/103 ready relationships, and evidence/verify/ship report 49 observed non-synthetic declarations with zero blockers.
- **Evidence:** command:dotnet fsgg-sdd analyze --work 220-bounded-pr-ci --text; command:dotnet fsgg-sdd verify --work 220-bounded-pr-ci --text; command:dotnet fsgg-sdd ship --work 220-bounded-pr-ci --text
- **Version:** fsgg-sdd 1.0.1
- **Owner:** FS-GG/FS.GG.SDD lifecycle regeneration and readiness
- **Recurrence:** seen again feedback/2026-08-21-sir-item-220-ci-budget-repair-phase-3.md §4.1; no issue required for this positive pattern
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 Same-runner browser pairs remain a hosted critical-path quality gap

- **Kind:** quality-gap
- **Impact:** Representative PR feedback remained red despite balanced functional shards, blocking merge and dependent performance work.
- **Expected:** The complete growing browser inventory remains isolated, balanced, fail-closed, and completes as part of the representative route within 240,000 ms without oversubscribing a runner.
- **Observed:** Round 2 balanced 40 general tests 20/20 but the two server/browser pairs shared one four-lane runner and the browser gate took 110,987 ms after a 113,993 ms web producer. The aggregate joined at 269,386 ms. Round 3 assigns the complementary global halves to separate hosted runners, one pair each; focused halves completed 20 pass and 19 pass plus one intentional skip, and the cross-fragment merge accounted for 40 complete unique cases. Removal of spatial-mutations, browser-general-helper, or browser-delivery from `pr-verdict.needs` makes the route contract red. Exact-head hosted timing remains pending.
- **Evidence:** review:https://github.com/EHotwagner/S.I.R./pull/242#issuecomment-5364423960; run:https://github.com/EHotwagner/S.I.R./actions/runs/32438948032; issue:EHotwagner/S.I.R.#220; command:SIR_JUNIT_OUTPUT=artifacts/test-results/browser-general-1.junit.xml SIR_BROWSER_SHARDS=2 SIR_BROWSER_SHARD_INDEX=1 SIR_BROWSER_COHORT=general npm run test:browser; command:SIR_JUNIT_OUTPUT=artifacts/test-results/browser-general-2.junit.xml SIR_BROWSER_SHARDS=2 SIR_BROWSER_SHARD_INDEX=2 SIR_BROWSER_COHORT=general npm run test:browser; command:node scripts/test-browser-global-merge.mjs artifacts/test-results/browser-general.junit.xml artifacts/test-results/browser-general-1.junit.xml artifacts/test-results/browser-general-2.junit.xml; command:node scripts/test-ci-route.mjs
- **Version:** hosted workflow Node 26.5.0; local Node 26.7.0; Chromium 151.0.7922.34
- **Owner:** EHotwagner/S.I.R. CI browser qualification
- **Recurrence:** seen again feedback/2026-08-21-sir-item-220-ci-budget-repair-phase-3.md §4.2; existing issue EHotwagner/S.I.R.#220 and PR #242
- **Avoidable cost:** one hosted qualification; no unchanged retry
- **Disposition:** existing issue

#### §4.3 Reconnect stopped an already stopped LongPolling connection

- **Kind:** defect
- **Impact:** One authorized player journey emitted a user-visible network and console 404 under hosted timing, making the general browser receipt fail closed.
- **Expected:** Explicit disconnect followed by reconnect starts the completed connection lifecycle and requests an authoritative resync without a stale connection-id request or ignored diagnostic.
- **Observed:** The hosted journey captured `/hub/game?id=...` as 404. `disconnect` had already awaited `active.stop()`, while `reconnect` called `active.stop()` again before `active.start()`. Round 3 starts directly from the completed disconnected state, retains the resync request, and the rebuilt production client passed five focused repetitions. Restoring the second stop makes the route ownership assertion red.
- **Evidence:** review:https://github.com/EHotwagner/S.I.R./pull/242#issuecomment-5364423960; command:npm run build:client && SIR_PLAYWRIGHT_PORT=5181 SIR_JUNIT_OUTPUT=artifacts/test-results/browser-reconnect.junit.xml npx playwright test tests/SIR.Browser.Tests/live-session.spec.js --config tests/SIR.Browser.Tests/playwright.config.js --grep 'authorized player journey' --repeat-each=5; command:node scripts/test-ci-route.mjs
- **Version:** hosted workflow Node 26.5.0; local Node 26.7.0; Chromium 151.0.7922.34; Microsoft SignalR LongPolling transport
- **Owner:** EHotwagner/S.I.R. live-session lifecycle
- **Recurrence:** new; related closed live-session issues #146, #148, and #150 did not record this exact double-stop 404
- **Avoidable cost:** one hosted functional failure and one focused diagnosis
- **Disposition:** product fix

## §5 Did not exercise

Exact-head hosted acceptance, merge, post-merge obligations, scheduled full qualification, packaging, and upgrade paths remain unexercised at this source freeze.

## §6 Doc-versus-behavior contradictions

None observed. The CI documentation now distinguishes same-runner pair capacity from the two-runner global shard allocation and explicitly states that inventory grows without a test-count ceiling.

## §7 Workarounds still in the tree

None. The two hosted general-browser jobs are an explicit concurrency allocation with complete test-granular coverage, not a temporary test exclusion. The fixed delivery helper remains fixed because it owns one fixed throttled functionality proof.

## §8 Friction and avoidable cost

One hosted round-2 qualification was required and correctly not retried unchanged. Diagnosis used one log extraction, five focused reconnect repetitions before the source change, two isolated 20-test halves, one client rebuild, and five focused repetitions after the change. Three dependency-edge inversions and the reconnect inversion were restored before the source freeze. No full local aggregate was run.

## §9 Skill value and gaps

`work-board-best` and `pnext-item` preserved the repair-phase claim/review ledger. The SDD lifecycle skills kept the revised plan, tasks, evidence, and readiness coherent. `fs-gg-feedback-report` preserved the exact hosted failure and required cold review. No gameplay, map, ballistics, persistence, package, or documentation-authoring skill applied to this CI topology repair.

## §10 Outcome markers

Round-2 hosted run: browser 110,987 ms, aggregate 269,386 ms, red. Round-3 focused global halves: 20/20 inventory, 39 passed plus one intentional skip, approximately 19 seconds each locally. Rebuilt reconnect journey: five of five passed. Route, audit-binding, SDD analysis, evidence, verification, and ship gates are green. Hosted acceptance, merge, and done remain pending.

## §11 Falsifiable improvements

Preserve §4.1 by requiring every repair-round source freeze to reproduce zero-blocker analyze, verify, and ship outputs after task regeneration. Resolve §4.2 only when an exact-head hosted representative run is fully green at or below 240,000 ms, both general-browser receipts account for the complete disjoint inventory, the throttled delivery receipt remains green, and removing any helper dependency makes the focused route contract fail. Preserve §4.3 by making any reconnect implementation containing a second `active.stop()` fail the focused route ownership assertion while the production journey remains diagnostic-clean.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | partial | Existing scaffold retained; no new scaffold generated. |
| onboarding-guidance | partial | Existing repository guidance governed board ownership and scope. |
| skills | exercised | Board, SDD, and feedback skills drove the repair lifecycle. |
| sdd-authoring | exercised | 49 tasks and 103/103 relationships remained ready after revision. |
| implementation-apis | exercised | Typed helper receipts, global shard indexing, and reconnect lifecycle changed. |
| dependencies-build | exercised | Client rebuilt successfully; hosted prepared artifacts were independently green in round 2. |
| testing | exercised | Two 20-test halves, strict merge, reconnect repetitions, route gate, and four focused inversions passed. |
| evidence | exercised | 49 observed declarations; verification and ship readiness green. |
| runtime-playtest | partial | Real Chromium reconnect and tactical browser journeys ran; no gameplay playtest was required. |
| performance | exercised | Exact hosted gate/aggregate timings and focused shard durations informed the design. |
| documentation | exercised | CI qualification documentation was updated and route-audit binding passed. |
| packaging-upgrade | not-exercised | No package or upgrade work occurred. |
| worker-git-pr | exercised | Repair-phase confirmation round 2 was mechanically recorded; round 3 remains pre-push. |
