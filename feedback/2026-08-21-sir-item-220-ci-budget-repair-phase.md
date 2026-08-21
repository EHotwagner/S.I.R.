---
feedbackSchema: 2
date: 2026-08-21
workspace: S.I.R
cycle: item-220-ci-budget-repair-phase
lane: sdd
toolVersion: 1.0.1
commit: a0ba2dbd4da44dd13881c6f2b3b2648e4c5c0565
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 2
- **zero-event reason:** n/a

This report covers the single automatic repair phase for issue 220 after ordinary PR 241 exhausted three review rounds and closed unmerged at `e4f3ae2`. The repair began from that exact head on a fresh claim, branch, and worktree. It used Node 26.7.0, npm 12.0.2, .NET SDK 10.0.302, Playwright's managed Chromium 151.0.7922.34, and SDD CLI 1.0.1. Two checkpoint events are recorded in `feedback/checkpoints/item-220-ci-budget-repair-phase.jsonl`. Focused evidence covers SDD regeneration, the route contract, shard distribution, both governing inversions, and one complete local browser run. Hosted exact-head timing, independent PR review, merge, and post-merge obligations remain outside this report's evidence boundary.

## §2 What worked

The revised SDD package is currently coherent under the same `220-bounded-pr-ci` work identity: analysis reports `implementationReady` with 103 source relationships and no blocking, stale, or missing-disposition findings. Test-granular Playwright sharding converted the measured four-shard `18/3/20/0` file-level distribution to `11/10/10/10`; applying the isolation-safe two-process capacity on the representative four-way runner yields two shards with `21/20`. A focused run completed the full 41-test browser inventory in 34 seconds with no failures while preserving one server, one browser worker, one port, and one deterministic JUnit fragment per shard.

## §3 What did not

The exhausted implementation treated reported CPU count as shard count and let Playwright shard whole files. Four shards therefore scheduled four servers plus four browsers, one shard received no tests, and the production-delivery assertion timed out under contention. The previous shared-server experiment was faster but caused deterministic live-session collisions, so shared-server concurrency was not reused. No aggregate local CI retry was run.

## §4 Findings

#### §4.1 The revised SDD package is coherent and implementation-ready

- **Kind:** positive-pattern
- **Impact:** The repair can proceed against a current machine-readable lifecycle package instead of relying on stale views or prose status.
- **Expected:** The current authored plan, tasks, and generated analysis should agree under one stable work identity before source handoff.
- **Observed:** `tasks` reports a coherent 49-task package and `analyze` reports `implementationReady` with 103 relationships and zero blocking, stale, or missing-disposition findings.
- **Evidence:** command:dotnet fsgg-sdd tasks --work 220-bounded-pr-ci --text; command:dotnet fsgg-sdd analyze --work 220-bounded-pr-ci --text
- **Version:** FS.GG SDD CLI 1.0.1; cycle commit `a0ba2dbd4da44dd13881c6f2b3b2648e4c5c0565`
- **Owner:** FS-GG.SpecDrivenDevelopment lifecycle regeneration
- **Recurrence:** new
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 Test-granular isolation capacity removed both imbalance and server starvation

- **Kind:** positive-pattern
- **Impact:** The growing production browser inventory can use available runner capacity without empty shards, shared-session collisions, or CPU starvation of its production servers.
- **Expected:** Capacity should model the independently runnable processes a shard owns, and distribution should balance tests rather than depend on file sizes.
- **Observed:** The repair maps the 41-test inventory `21/20` across two isolation-safe shards and completed the focused full browser run in 34 seconds with zero failures and one intentional skip. Inverting either `fullyParallel` or the two-process shard cost makes the route contract test fail.
- **Evidence:** command:SIR_BROWSER_SHARDS=2 npm run test:browser; command:node scripts/test-ci-route.mjs
- **Version:** Playwright managed Chromium 151.0.7922.34; cycle commit `a0ba2dbd4da44dd13881c6f2b3b2648e4c5c0565`
- **Owner:** EHotwagner/S.I.R. browser qualification
- **Recurrence:** seen again issue:EHotwagner/S.I.R.#220
- **Avoidable cost:** one focused web/server preparation; no aggregate local CI
- **Disposition:** product fix

## §5 Did not exercise

Hosted runner timing, full protected qualification, scheduled qualification, package publication, gameplay behavior changes, and merge/post-merge delivery were not exercised before this report boundary.

## §6 Doc-versus-behavior contradictions

The exhausted documentation described raw reported parallel capacity as safe browser shard capacity, while each shard actually schedules both Chromium and a production server. The repair changes the documentation and implementation to the same two-process capacity model.

## §7 Workarounds still in the tree

None. Explicit `SIR_BROWSER_SHARDS` remains a documented capacity-bounded local control, not a bypass; values above the isolation-safe capacity fail closed.

## §8 Friction and avoidable cost

The repair used one dependency installation and one focused web/server preparation to execute the full browser inventory. Receipt production refused the dirty tracked candidate during that preparation, as designed, so the already-built artifacts were composed only for the local focused browser measurement. The aggregate local route was not run or retried. The ordinary chain's three prior hosted repair rounds belong to the exhausted cycle and are not re-counted here.

## §9 Skill value and gaps

The next-item and intra-repository coordination contracts preserved fresh identity, claim, branch, and repair-phase provenance. The SDD lifecycle and authoring-contract skills kept the revised decision current before implementation. The feedback skill captured both reusable patterns and requires this independently audited schema-v2 report. No gameplay, persistence, API documentation, or packaging skill was relevant.

## §10 Outcome markers

SDD reached `implementationReady`, `evidenceReady`, `verificationReady`, and `shipReady` with 49/49 observed evidence declarations and zero self-attested, invalid, stale, missing, or blocking evidence. The focused route contract passed; the capacity and balance inversions each failed as intended. The complete local browser inventory passed 41 tests in 34 seconds across `21/20` isolated shards. Hosted exact-head acceptance, review, merge, and done remain pending.

## §11 Falsifiable improvements

- Preserve §4.1: the revised PD-003 package must remain current with zero missing dispositions under the same `220-bounded-pr-ci` work identity.
- Preserve §4.2: a four-way capacity input must produce two isolated shards, Playwright list mode must distribute the 41-test inventory `21/20`, and changing either `fullyParallel: true` or `browserProcessesPerShard = 2` must make the focused route contract red.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing product scaffold retained. |
| onboarding-guidance | partial | Existing generated guidance was consumed; no new-user onboarding. |
| skills | exercised | Coordination, SDD, and feedback skills drove the repair. |
| sdd-authoring | exercised | Revised plan regenerated through ship-ready views. |
| implementation-apis | exercised | Browser capacity module and Playwright configuration changed. |
| dependencies-build | exercised | Locked npm and .NET restore plus focused web/server preparation completed. |
| testing | exercised | Route contract, two inversions, distribution listing, and 41 browser tests ran. |
| evidence | exercised | SDD reached 49/49 observed and feedback checkpoints validate. |
| runtime-playtest | partial | Production browser journeys ran; no manual gameplay playtest. |
| performance | exercised | Shard balance and focused elapsed time were measured; hosted timing is pending. |
| documentation | exercised | CI qualification documentation now names the isolation capacity. |
| packaging-upgrade | not-exercised | No dependency or package publication change. |
| worker-git-pr | partial | Fresh repair worker/claim/branch established; PR review and merge pending. |
