---
feedbackSchema: 2
date: 2026-08-21
workspace: S.I.R
cycle: item-220-ci-budget-repair-phase
lane: sdd
toolVersion: 1.0.1
commit: ee0eb0c791a7cfb10079def8e88aad889cfb129c
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 3
- **zero-event reason:** n/a

This superseding cycle report covers the same single automatic repair phase as `feedback/2026-08-21-sir-item-220-ci-budget-repair-phase.md` and adds the first hosted exact-head result plus repair round one. The source boundary is commit `ee0eb0c791a7cfb10079def8e88aad889cfb129c`; tools remain Node 26.7.0, npm 12.0.2, .NET SDK 10.0.302, Playwright-managed Chromium 151.0.7922.34, and SDD CLI 1.0.1. Three immutable checkpoints are recorded. The first hosted head passed all substantive gates but missed the 240-second target; no unchanged aggregate retry ran. Round-one evidence is focused and does not claim hosted acceptance, independent confirmation, merge, or delivery completion.

## §2 What worked

SDD regeneration kept the revised 49-task package current and implementation-ready. Exact hosted receipts made the critical-path shift visible: browser balance repaired the functional timeout and empty shard, then spatial became the longest gate instead of being hidden behind a browser failure. Round one responded to that measured graph rather than retrying: browser capacity now reserves one fixed lane for the server cohort and scales the remaining lanes, prepared spatial mutations overlap independent final conformance, and shard JUnit fragments fail closed on malformed, empty, count-drifted, or duplicate content.

The focused browser inventory balances `14/14/13` across three isolated server/browser pairs and completed 41 tests in 27 seconds with no failures and one intentional skip. The reproducible prepared spatial command ran all nine named mutants in 22 seconds. The route contract passed with direct negative parser cases and exact source-topology assertions for the capacity reserve and spatial overlap.

## §3 What did not

The first repair head's two-process-per-shard model was functionally safe but under-used the declared runner: hosted browser remained 123,440 ms and spatial's serial mutation-then-conformance composition became the 132,219 ms critical gate. The aggregate therefore joined at 283,553 ms instead of the required 240,000 ms. The fresh PR critic also demonstrated that an existing malformed shard fragment was silently interpreted as an empty shard, allowing a false one-test passing aggregate. Both causes were repaired in round one; hosted proof remains pending.

## §4 Findings

#### §4.1 Current lifecycle coherence remains cheap to re-establish after measured repair

- **Kind:** positive-pattern
- **Impact:** Each review repair can update the technical decision without carrying stale lifecycle views into the next hosted boundary.
- **Expected:** The current authored plan, tasks, evidence, and generated views should agree before a repaired source head is pushed.
- **Observed:** The package reports 49 tasks, 103 ready relationships, `implementationReady`, `evidenceReady`, `verificationReady`, and `shipReady`, with zero stale, invalid, missing, self-attested, or blocking evidence.
- **Evidence:** command:dotnet fsgg-sdd analyze --work 220-bounded-pr-ci --text; command:dotnet fsgg-sdd verify --work 220-bounded-pr-ci --text; command:dotnet fsgg-sdd ship --work 220-bounded-pr-ci --text
- **Version:** FS.GG SDD CLI 1.0.1; commit `ee0eb0c791a7cfb10079def8e88aad889cfb129c`
- **Owner:** FS-GG.SpecDrivenDevelopment lifecycle regeneration
- **Recurrence:** seen again feedback/2026-08-21-sir-item-220-ci-budget-repair-phase.md §4.1
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 Typed hosted timing exposed the next critical gate without an aggregate retry

- **Kind:** positive-pattern
- **Impact:** Repair effort moved from the now-functional browser route to the actual 132,219 ms spatial critical gate, while the malformed-JUnit review finding was repaired at its fail-closed boundary.
- **Expected:** A failed target run should preserve per-gate attribution sufficient to choose the next bounded repair, and focused gates should exercise the repaired behavior before another hosted qualification.
- **Observed:** Run 32434802616 passed every substantive job but failed only `feedback-headroom-eroded` at 283,553 ms; round-one focused evidence then passed 41 browser tests in 27 seconds over `14/14/13`, ran all nine prepared spatial mutants in 22 seconds, and passed the route contract's strict JUnit negative cases and exact topology assertions.
- **Evidence:** issue:EHotwagner/S.I.R.#220; command:SIR_BROWSER_SHARDS=3 npm run test:browser; command:./scripts/test-spatial-subject-mutations.sh --prepared-pr; command:node scripts/test-ci-route.mjs
- **Version:** `sir.ci-join/v1`; run 32434802616; commit `ee0eb0c791a7cfb10079def8e88aad889cfb129c`
- **Owner:** EHotwagner/S.I.R. PR qualification topology
- **Recurrence:** seen again issue:EHotwagner/S.I.R.#220
- **Avoidable cost:** one hosted exact-head qualification; no unchanged aggregate retry
- **Disposition:** product fix

## §5 Did not exercise

Round-one hosted timing, protected/scheduled full qualification, package publication, merge, and post-merge delivery were not exercised at this report boundary.

## §6 Doc-versus-behavior contradictions

The earlier cycle report describes the exact two-process-per-shard first attempt at its own immutable commit. Current documentation now reflects the measured server-cohort reserve, strict fragment validation, and overlapping spatial work. No current doc-versus-source contradiction remains.

## §7 Workarounds still in the tree

None. The explicit shard override remains capacity-bounded; the fixed one-lane server reserve represents fixed helper functionality while browser lanes continue to scale with runner capacity.

## §8 Friction and avoidable cost

One hosted qualification was required to reveal the new critical path. It was not retried unchanged. Round one used one focused 41-test browser run, one prepared nine-mutant spatial run, focused route/parser contracts, and lifecycle regeneration. The critic's malformed-fragment probe prevented a false-green receipt from advancing.

## §9 Skill value and gaps

The repair-phase review protocol preserved the exhausted ordinary chain and routed two material findings to the same fresh critic. SDD kept decisions and evidence current. Feedback checkpoints preserved the critical-path transition. No gameplay, API-doc, persistence, or package-publication skill applied.

## §10 Outcome markers

First hosted head: all substantive gates green, aggregate 283,553 ms and red. Round one: route contract green; `14/14/13` browser list; 41 browser tests green in 27 seconds; nine spatial mutants green in 22 seconds; strict parser negative cases and exact source-topology assertions green. SDD is ship-ready with 49/49 observed evidence. Round-one hosted acceptance and same-critic confirmation remain pending.

## §11 Falsifiable improvements

- Preserve §4.1: every repaired authored source must regenerate to zero stale/missing/blocking lifecycle findings before push.
- Preserve §4.2: a four-way capacity must yield three browser shards, malformed/empty/count-drifted/duplicate shard receipts must fail, prepared spatial mutation and final conformance must overlap, and the focused route contract must assert each property.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing scaffold retained. |
| onboarding-guidance | partial | Existing generated guidance consumed. |
| skills | exercised | Coordination, SDD, review, and feedback skills drove repair. |
| sdd-authoring | exercised | Revised plan regenerated through ship-ready views. |
| implementation-apis | exercised | Capacity, JUnit, shard, and spatial topology modules changed. |
| dependencies-build | exercised | Focused native and browser prerequisites built. |
| testing | exercised | Browser, spatial, route, parser, and inversion checks ran. |
| evidence | exercised | 49/49 SDD evidence observed; three feedback checkpoints validate. |
| runtime-playtest | partial | Production browser journeys ran; no manual playtest. |
| performance | exercised | Exact hosted and focused critical-path measurements recorded. |
| documentation | exercised | CI qualification topology documentation updated. |
| packaging-upgrade | not-exercised | No package publication or upgrade. |
| worker-git-pr | exercised | Fresh repair PR, critic initial decision, and round-one repair active. |
