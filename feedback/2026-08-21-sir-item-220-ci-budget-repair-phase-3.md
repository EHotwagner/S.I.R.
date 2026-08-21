---
feedbackSchema: 2
date: 2026-08-21
workspace: S.I.R
cycle: item-220-ci-budget-repair-phase
lane: sdd
toolVersion: 1.0.1
commit: 230f7ce973a628fd2636d304f3c239bca4e14cb2
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 4
- **zero-event reason:** n/a

This third immutable report supersedes the earlier same-cycle reports without rewriting them. It covers repair round two after hosted run 32436729777 passed every substantive gate but missed the 240-second representative target at 279,159 ms. The source boundary is commit `230f7ce973a628fd2636d304f3c239bca4e14cb2`; tools remain Node 26.7.0, npm 12.0.2, .NET SDK 10.0.302, Playwright-managed Chromium 151.0.7922.34, and SDD CLI 1.0.1. Four checkpoints are retained. Round-two evidence is focused and does not claim hosted acceptance, critic confirmation, merge, or delivery completion.

## §2 What worked

Typed per-subject receipts exposed that the repaired browser/spatial work still occupied a second serial DAG stage after roughly 108 seconds of preparation. The next repair therefore moved fixed proofs rather than raising budgets or adding unsafe per-runner processes. The nine fixed spatial mutations now form a required source-bound helper launched directly after routing. Final native/Fable conformance remains in `spatial`. The fixed Slow-3G/4x-CPU delivery proof now forms a required browser helper over the same verified web/server artifacts, concurrent with the growing general browser inventory.

The capacity model returned to exact server-plus-browser pair accounting: `max(1, floor(availableParallelism / 2))`. Locally, the general cohort completed 40 tests as 20/20 in about 20 seconds, with 39 passed and one intentional skip; the tagged fixed delivery cohort passed its one throttled test in about 15 seconds; and the isolated nine-mutant helper passed in about 30 seconds including its clean base build. Missing or failed helper receipts make the deterministic join red.

## §3 What did not

Round one changed pair accounting to `availableParallelism - 1`. Although its three hosted browser shards passed, the same critic correctly rejected that as acceptance drift: on a four-lane runner it schedules three browsers plus three independent servers. The hosted result also improved only 4,394 ms, from 283,553 to 279,159, because spatial mutations remained on the post-preparation critical stage. Round two restores the reviewed capacity contract and uses independently attributable functional fan-out instead.

## §4 Findings

#### §4.1 Lifecycle regeneration continued to preserve a coherent repair package

- **Kind:** positive-pattern
- **Impact:** Each measured repair could update decisions and evidence without stale lifecycle views crossing the next PR boundary.
- **Expected:** Current plan, tasks, evidence, and generated readiness views should agree before another candidate is pushed.
- **Observed:** The package reports 49 tasks, 103 ready relationships, `implementationReady`, `evidenceReady`, `verificationReady`, and `shipReady`, with zero stale, invalid, synthetic, self-attested, missing, or blocking evidence.
- **Evidence:** command:dotnet fsgg-sdd analyze --work 220-bounded-pr-ci --text; command:dotnet fsgg-sdd verify --work 220-bounded-pr-ci --text; command:dotnet fsgg-sdd ship --work 220-bounded-pr-ci --text
- **Version:** FS.GG SDD CLI 1.0.1; commit `230f7ce973a628fd2636d304f3c239bca4e14cb2`
- **Owner:** FS-GG.SpecDrivenDevelopment lifecycle regeneration
- **Recurrence:** seen again feedback/2026-08-21-sir-item-220-ci-budget-repair-phase-2.md §4.1
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 Fixed functional fan-out preserves safe capacity while shortening the serial graph

- **Kind:** positive-pattern
- **Impact:** The growing browser inventory and fixed delivery/spatial proofs no longer compete inside one post-preparation critical gate, while every proof remains required and independently attributable.
- **Expected:** CI topology should scale by functional ownership without oversubscribing a runner, omitting tests, weakening receipt checks, or increasing the 300,000 ms budget.
- **Observed:** Pair accounting yields two browser/server pairs at parallelism four. The general browser cohort completes 40 tests as 20/20 with 39 passed and one intentional skip, the one-test throttled delivery helper passes independently, all nine spatial mutants pass their fail-closed proof, and the route contract rejects missing or failed helper receipts. Current-head hosted timing remains pending.
- **Evidence:** command:node scripts/test-ci-route.mjs; command:SIR_BROWSER_SHARDS=2 SIR_BROWSER_COHORT=general npm run test:browser; command:SIR_BROWSER_SHARDS=1 SIR_BROWSER_COHORT=production-delivery npm run test:browser; command:./scripts/test-spatial-subject-mutations.sh --prepared-pr
- **Version:** `sir.ci-join/v1`; commit `230f7ce973a628fd2636d304f3c239bca4e14cb2`
- **Owner:** EHotwagner/S.I.R. PR qualification topology
- **Recurrence:** seen again issue:EHotwagner/S.I.R.#220
- **Avoidable cost:** one hosted exact-head qualification for round one; no unchanged aggregate retry
- **Disposition:** product fix

## §5 Did not exercise

Round-two hosted timing, same-critic confirmation, protected/scheduled full qualification, package publication, merge, and post-merge delivery were not exercised at this boundary.

## §6 Doc-versus-behavior contradictions

The prior report records `availableParallelism - 1` at its own immutable boundary. Current documentation and code both restore pair accounting and describe the two required helper subjects. No current contradiction remains.

## §7 Workarounds still in the tree

None. The one-shard delivery helper is fixed because it owns exactly one fixed throttled proof; the general inventory continues to scale within pair-accounted machine capacity.

## §8 Friction and avoidable cost

One hosted round-one qualification exposed the remaining serial graph and capacity drift; it was not retried. Round two used two focused browser cohorts, one spatial helper run, route/join inversions, and lifecycle regeneration. No local or hosted aggregate was repeated unchanged.

## §9 Skill value and gaps

The repair review protocol prevented a functional pass from silently widening the capacity contract. SDD kept the revised helper topology and evidence current. Feedback checkpoints preserved both hosted failures and the eventual functional fan-out. No gameplay, API-doc, persistence, or package-publication skill applied.

## §10 Outcome markers

Round-one hosted head: all substantive gates green, aggregate 279,159 ms and red. Round two: general browser completed 40 tests as 20/20 in about 20 seconds, with 39 passed and one intentional skip; fixed throttled delivery one test green in about 15 seconds; nine spatial mutants plus base build green in about 30 seconds; route/helper join contract green; capacity and helper-removal inversions red. SDD is ship-ready with 49/49 observed evidence. Hosted acceptance and same-critic confirmation remain pending.

## §11 Falsifiable improvements

- Preserve §4.1: every repaired authored source must regenerate to zero stale, missing, or blocking lifecycle findings before push.
- Preserve §4.2: parallelism four must yield two browser/server pairs; general and delivery cohorts must be disjoint and total 41 tests; spatial-mutations and browser-delivery receipts must be required whenever their parent gates apply; removing either helper or restoring `parallelism - 1` must red the focused route contract.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing scaffold retained. |
| onboarding-guidance | partial | Existing generated guidance consumed. |
| skills | exercised | Coordination, SDD, review, and feedback skills drove repair. |
| sdd-authoring | exercised | Revised plan regenerated through ship-ready views. |
| implementation-apis | exercised | Route, receipt, shard, helper, and workflow topology changed. |
| dependencies-build | exercised | Focused spatial base/mutant and browser prerequisites built. |
| testing | exercised | Both browser cohorts, spatial mutations, route, join, parser, and inversions ran. |
| evidence | exercised | 49/49 SDD evidence observed; four feedback checkpoints validate. |
| runtime-playtest | partial | Production browser journeys ran; no manual playtest. |
| performance | exercised | Exact hosted timing and focused helper measurements recorded. |
| documentation | exercised | CI qualification topology documentation updated. |
| packaging-upgrade | not-exercised | No package publication or upgrade. |
| worker-git-pr | exercised | Repair PR, two critic decisions, and round-two repair active. |
