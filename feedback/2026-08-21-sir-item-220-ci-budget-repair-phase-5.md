---
feedbackSchema: 2
date: 2026-08-21
workspace: S.I.R-item-220-ci-budget-repair-phase
cycle: item-220-ci-budget-repair-phase
lane: sdd
toolVersion: 1.0.1
commit: 5957277c6f34f05fc8ba64a9ac5868cc80d50fd9
---

## §1 Provenance and confidence

This immutable addendum covers repair confirmation round 3 and the round-4 source freeze for issue EHotwagner/S.I.R.#220 and PR #242. The cycle has six checkpoints at `feedback/checkpoints/item-220-ci-budget-repair-phase.jsonl`. The existing scaffold and package locks remain unchanged. Confidence is high for the exact hosted round-3 result, current lifecycle state, four-way browser coverage, strict merge, cancellation mutation proof, and workflow inversions. Exact-head hosted timing for `5957277c6f34f05fc8ba64a9ac5868cc80d50fd9` is intentionally pending and is not claimed.

## §2 What worked

Typed gate timing made the remaining serial work measurable: round 3 passed every substantive receipt while identifying browser and cancellation as two long gates. Global Playwright indexing distributed the complete general inventory 10/10/10/10 without a test-count ceiling, and the existing strict merger accounted for 40 complete unique cases. The cancellation mutation script could separate its fixed failing proof from the restored passing smoke without changing either subject. SDD regeneration preserved 49 tasks and returned all 103 relationships ready.

## §3 What did not

Round 3 did not reach acceptance. The exact hosted aggregate was 276,119 ms against the 240,000 ms target even though all 16 substantive receipts passed. The two general browser halves were balanced by count but consumed 123,741 ms and 79,912 ms total, while cancellation consumed 118,384 ms and placed its 56,417 ms fixed mutation build after preparation. No unchanged hosted retry was attempted.

## §4 Findings

#### §4.1 SDD retained a coherent repair-round boundary

- **Kind:** positive-pattern
- **Impact:** Review can distinguish a measured topology revision from an undocumented timing experiment.
- **Expected:** A repair-round source change follows a revised plan and ends with current observed evidence, verification, and ship readiness.
- **Observed:** Tasks report 49 with two in progress, analysis reports 103/103 ready relationships, evidence reports 49 observed non-synthetic declarations, and verify/ship report zero blockers.
- **Evidence:** command:dotnet fsgg-sdd analyze --work 220-bounded-pr-ci --text; command:dotnet fsgg-sdd verify --work 220-bounded-pr-ci --text; command:dotnet fsgg-sdd ship --work 220-bounded-pr-ci --text
- **Version:** fsgg-sdd 1.0.1
- **Owner:** FS-GG/FS.GG.SDD lifecycle regeneration and readiness
- **Recurrence:** seen again feedback/2026-08-21-sir-item-220-ci-budget-repair-phase-4.md §4.1; no issue required for this positive pattern
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 Two-runner browser fan-out and serial cancellation mutation left the hosted target unmet

- **Kind:** quality-gap
- **Impact:** Representative PR feedback remained red despite complete functional qualification, blocking merge and dependent work.
- **Expected:** Every browser and cancellation subject remains isolated and fail-closed while the representative cross-cutting route completes within 240,000 ms without raising the 300,000 ms boundary.
- **Observed:** Round 3 passed all 16 substantive receipts but joined at 276,119 ms. The two browser runners took 123,741 ms and 79,912 ms, and cancellation took 118,384 ms with a 56,417 ms fixed mutation build after preparation. Round 4 keeps one server/browser pair per runner but globally indexes the complete general inventory over four independent runners; focused runs assigned exactly 10 cases to each runner, produced 39 passes plus one intentional skip, and merged to 40 complete unique cases. The fixed cancellation mutant now runs in a source-bound helper directly after routing, while the ordinary gate retains the restored worker smoke over prepared artifacts. Removing either new browser helper or the cancellation helper from `pr-verdict.needs`, or removing `--mutation-only` ownership, makes the route contract red.
- **Evidence:** review:https://github.com/EHotwagner/S.I.R./pull/242#issuecomment-5364657647; run:https://github.com/EHotwagner/S.I.R./actions/runs/32441258743; issue:EHotwagner/S.I.R.#220; command:SIR_JUNIT_OUTPUT=artifacts/test-results/browser-general-1.junit.xml SIR_BROWSER_SHARDS=4 SIR_BROWSER_SHARD_INDEX=1 SIR_BROWSER_COHORT=general npm run test:browser; command:SIR_JUNIT_OUTPUT=artifacts/test-results/browser-general-2.junit.xml SIR_BROWSER_SHARDS=4 SIR_BROWSER_SHARD_INDEX=2 SIR_BROWSER_COHORT=general npm run test:browser; command:SIR_JUNIT_OUTPUT=artifacts/test-results/browser-general-3.junit.xml SIR_BROWSER_SHARDS=4 SIR_BROWSER_SHARD_INDEX=3 SIR_BROWSER_COHORT=general npm run test:browser; command:SIR_JUNIT_OUTPUT=artifacts/test-results/browser-general-4.junit.xml SIR_BROWSER_SHARDS=4 SIR_BROWSER_SHARD_INDEX=4 SIR_BROWSER_COHORT=general npm run test:browser; command:node scripts/test-browser-global-merge.mjs artifacts/test-results/browser-general.junit.xml artifacts/test-results/browser-general-1.junit.xml artifacts/test-results/browser-general-2.junit.xml artifacts/test-results/browser-general-3.junit.xml artifacts/test-results/browser-general-4.junit.xml; command:./scripts/test-worker-cancellation-subject-mutation.sh --mutation-only; command:node scripts/test-ci-route.mjs
- **Version:** hosted workflow Node 26.5.0; local Node 26.7.0; Chromium 151.0.7922.34
- **Owner:** EHotwagner/S.I.R. CI qualification topology
- **Recurrence:** seen again feedback/2026-08-21-sir-item-220-ci-budget-repair-phase-4.md §4.2; existing issue EHotwagner/S.I.R.#220 and PR #242
- **Avoidable cost:** one hosted qualification; no unchanged retry
- **Disposition:** existing issue

## §5 Did not exercise

Exact-head hosted acceptance, merge, post-merge obligations, scheduled full qualification, packaging, and upgrade paths remain unexercised at this source freeze.

## §6 Doc-versus-behavior contradictions

None observed. CI documentation now describes four globally indexed general-browser runners, per-runner pair accounting, the fifth fixed delivery receipt, and the separate cancellation mutation helper.

## §7 Workarounds still in the tree

None. Four general-browser runners are a workflow concurrency allocation over an uncapped discovered inventory, not a fixed test inventory. The cancellation helper owns a fixed functionality proof whose size does not grow with the project.

## §8 Friction and avoidable cost

One hosted round-3 qualification was required and correctly not retried unchanged. Round 4 used one 24-second cancellation mutation-only proof, four focused 10-case browser runners, one strict merge, and four restored topology/ownership inversions. No full local aggregate was run.

## §9 Skill value and gaps

`work-board-best` and `pnext-item` preserved the fresh repair-phase claim and same-critic ledger. The SDD lifecycle skills kept plan, tasks, evidence, and readiness coherent across another measured revision. `fs-gg-feedback-report` captured the exact failed head and requires this fresh-context audit. No gameplay, map, ballistics, persistence, package, or documentation-authoring skill applied to this CI topology repair.

## §10 Outcome markers

Round-3 exact hosted result: 16 substantive receipts passed, aggregate 276,119 ms, red. Round-4 focused browser distribution: 10/10/10/10, 39 passed plus one intentional skip, strict 40-case merge digest `c419a6281dc5b36feb36e674d5ced205a2e981886dab70b24efd34a79b7bc723`. Cancellation mutation-only proof passed and restored source in approximately 24 seconds. Route, SDD analysis, evidence, verification, and ship gates are green. Hosted acceptance, merge, and done remain pending.

## §11 Falsifiable improvements

Preserve §4.1 by requiring every repair-round source freeze to reproduce zero-blocker analyze, verify, and ship outputs after task regeneration. Resolve §4.2 only when one exact-head hosted representative run passes all substantive receipts and completes at or below 240,000 ms, all four general fragments account for the complete disjoint inventory, the throttled delivery receipt remains green, the cancellation mutation helper and restored smoke both remain green, and removal of any helper dependency makes the focused route contract fail.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | partial | Existing scaffold retained; no new scaffold generated. |
| onboarding-guidance | partial | Existing repository guidance governed board ownership and scope. |
| skills | exercised | Board, SDD, and feedback skills drove the repair lifecycle. |
| sdd-authoring | exercised | 49 tasks and 103/103 relationships remained ready after revision. |
| implementation-apis | exercised | Typed helper receipts, four-way global browser indexing, and mutation-only cancellation ownership changed. |
| dependencies-build | exercised | The cancellation mutant built and failed closed; prepared build topology stayed explicit. |
| testing | exercised | Four 10-case browser quarters, strict merge, cancellation mutation proof, route gate, and four focused inversions passed. |
| evidence | exercised | 49 observed declarations; verification and ship readiness green. |
| runtime-playtest | partial | Real Chromium tactical/browser journeys ran; no gameplay playtest was required. |
| performance | exercised | Exact hosted gate/aggregate timings and focused shard durations informed the design. |
| documentation | exercised | CI qualification documentation was updated and route/audit binding was checked. |
| packaging-upgrade | not-exercised | No package or upgrade work occurred. |
| worker-git-pr | exercised | Repair-phase confirmation round 3 was mechanically recorded; round 4 remains pre-push. |
