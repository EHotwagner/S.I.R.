---
feedbackSchema: 2
date: 2026-08-21
workspace: S.I.R-item-220-ci-budget-repair-phase
cycle: item-220-ci-budget-repair-phase
lane: sdd
toolVersion: 1.0.1
commit: f8bad809bb2c83d0163c967912be903fb205eae7
---

## §1 Provenance and confidence

This immutable addendum covers repair confirmation round 4 and the round-5 source freeze for issue EHotwagner/S.I.R.#220 and PR #242. The source commit has six checkpoints at `feedback/checkpoints/item-220-ci-budget-repair-phase.jsonl`; the seventh checkpoint accompanies this report metadata. Scaffold and package locks remain unchanged. Confidence is high for the exact hosted round-4 timing/result, its isolated cancellation-helper failure, the focused repaired mutation proof, current route contract, and SDD readiness. Exact-head hosted acceptance for `f8bad809bb2c83d0163c967912be903fb205eae7` remains intentionally pending.

## §2 What worked

The four-runner topology met the unchanged feedback target: the representative hosted aggregate completed in 231,069 ms, leaving 68,931 ms under the unchanged 300,000 ms boundary, while all four browser quarters, throttled delivery, and ordinary gates passed. Typed receipts then failed closed on the sole cancellation helper defect instead of accepting a timing-only result. The helper’s real probe exposed the exact missing native fixture boundary. A named minimal `SIR.Domain.Tests` build now supplies that fixture before mutation without moving the helper behind prepared-native work.

## §3 What did not

Round 4 was not acceptable despite meeting timing. The isolated `cancellation-mutations` runner built the mutant client but lacked the `SIR.Domain.Tests` Release executable required by `smoke-worker-roundtrip.mjs --print-replay-package`; its proof therefore stopped before cancellation semantics. No unchanged hosted retry was attempted.

## §4 Findings

#### §4.1 SDD retained a coherent repair-round boundary

- **Kind:** positive-pattern
- **Impact:** Review can distinguish the narrow fixture repair from a timing or gate-policy change.
- **Expected:** A repair-round source change follows a revised plan and ends with current observed evidence, verification, and ship readiness.
- **Observed:** Tasks retain 49 entries with two in progress, analysis reports 103/103 ready relationships, evidence reports 49 observed non-synthetic declarations, and verify/ship report zero blockers.
- **Evidence:** command:dotnet fsgg-sdd analyze --work 220-bounded-pr-ci --text; command:dotnet fsgg-sdd verify --work 220-bounded-pr-ci --text; command:dotnet fsgg-sdd ship --work 220-bounded-pr-ci --text
- **Version:** fsgg-sdd 1.0.1
- **Owner:** FS-GG/FS.GG.SDD lifecycle regeneration and readiness
- **Recurrence:** seen again feedback/2026-08-21-sir-item-220-ci-budget-repair-phase-5.md §4.1; no issue required for this positive pattern
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 Four isolated browser runners met the hosted feedback target without reducing coverage

- **Kind:** positive-pattern
- **Impact:** The representative route now has sufficient measured headroom while retaining every browser and mutation subject.
- **Expected:** All 41 browser subjects and every typed substantive receipt remain isolated and fail-closed while the representative aggregate completes within 240,000 ms under the unchanged 300,000 ms boundary.
- **Observed:** Round 4 completed at 231,069 ms with 68,931 ms outer headroom. Four general-browser receipts ran 10/10/10/10 cases, accounting for 39 passes and one intentional skip; strict merge accounted for 40 unique general cases, and the independent throttled delivery subject passed as the 41st. The verdict had no headroom failure and failed only the cancellation helper receipt.
- **Evidence:** run:https://github.com/EHotwagner/S.I.R./actions/runs/32442912623; review:https://github.com/EHotwagner/S.I.R./pull/242#issuecomment-5364827469; issue:EHotwagner/S.I.R.#220
- **Version:** hosted workflow Node 26.5.0 and Chromium 151.0.7922.0; local managed Chromium 151.0.7922.34
- **Owner:** EHotwagner/S.I.R. CI qualification topology
- **Recurrence:** seen again feedback/2026-08-21-sir-item-220-ci-budget-repair-phase-5.md §4.2 with new exact hosted evidence; existing issue EHotwagner/S.I.R.#220 and PR #242
- **Avoidable cost:** none beyond the required one hosted qualification
- **Disposition:** existing issue

#### §4.3 The early cancellation helper omitted the native fixture generator its real probe requires

- **Kind:** quality-gap
- **Impact:** Exact hosted qualification remained red after meeting the timing target because the mutation proof failed before reaching its intended cancellation behavior.
- **Expected:** The isolated early helper must generate the authoritative replay fixture, build the mutant client, and fail for the expected cancellation diagnostic; removing either subject must make focused qualification red.
- **Observed:** Hosted round 4 failed because `SIR.Domain.Tests` had not been built when the smoke requested `--print-replay-package`. Round 5 adds one traced Release build under `cancellation-fixture` before source mutation. The real mutation-only probe now fails for the intended cancellation semantic and restores the source; removing the named fixture ownership makes the route contract red, while the restored baseline passes.
- **Evidence:** run:https://github.com/EHotwagner/S.I.R./actions/runs/32442912623; review:https://github.com/EHotwagner/S.I.R./pull/242#issuecomment-5364827469; command:npm ci && ./scripts/test-worker-cancellation-subject-mutation.sh --mutation-only; command:node scripts/test-ci-route.mjs
- **Version:** hosted workflow .NET 10.0.302 and Node 26.5.0; local .NET 10.0.302 and Node 26.7.0
- **Owner:** EHotwagner/S.I.R. `cancellation-mutations` helper and traced build inventory
- **Recurrence:** new in round 4; existing issue EHotwagner/S.I.R.#220 and PR #242; no separate owner found
- **Avoidable cost:** one hosted qualification; no unchanged retry
- **Disposition:** product fix

## §5 Did not exercise

Round-5 exact-head hosted acceptance, merge, post-merge obligations, scheduled full qualification, packaging, and upgrade paths remain unexercised at this source freeze.

## §6 Doc-versus-behavior contradictions

None observed. CI documentation now names the minimal `cancellation-fixture` build and distinguishes it from ordinary prepared-native ownership.

## §7 Workarounds still in the tree

None. The helper’s fixture build is fixed functionality required by the real replay probe, not a project-size-dependent duplicate solution build; its named exception and route inventory make it reviewable.

## §8 Friction and avoidable cost

One required hosted round-4 qualification exposed the isolated-runner defect and was not retried unchanged. Round 5 used one real mutation-only proof, one route baseline, one build-marker removal inversion, and lifecycle regeneration. No full local aggregate was run.

## §9 Skill value and gaps

`work-board-best` and `pnext-item` preserved the fresh repair-phase claim and same-critic ledger. The SDD lifecycle skills forced the new native fixture exception into the plan before implementation and kept tasks/evidence/readiness coherent. `fs-gg-feedback-report` captured the exact hosted defect and requires this fresh-context audit. No gameplay, map, ballistics, persistence, package, or documentation-authoring skill applied.

## §10 Outcome markers

Round-4 exact hosted result: aggregate 231,069 ms and timing target met; all substantive receipts except `cancellation-mutations` passed. Round-5 focused result: real cancellation mutation proof passed and restored source; route baseline passed; removal of named fixture ownership failed closed. SDD analysis, evidence, verification, and ship are ready. Exact-head hosted acceptance, merge, and done remain pending.

## §11 Falsifiable improvements

Preserve §4.1 by requiring zero-blocker analyze, verify, and ship output after every repair revision. Preserve §4.2 only if the final exact-head hosted route stays at or below 240,000 ms with the unchanged 300,000 ms boundary and complete 41-test browser accounting. Resolve §4.3 only when the exact isolated hosted helper passes and its receipt proves the named fixture build, mutant builds, and intended cancellation diagnostic; removing the fixture ownership must keep the focused route contract red.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | partial | Existing scaffold retained; no new scaffold generated. |
| onboarding-guidance | partial | Existing repository guidance governed board ownership and scope. |
| skills | exercised | Board, SDD, and feedback skills drove the repair lifecycle. |
| sdd-authoring | exercised | 49 tasks and 103/103 relationships remained ready after revision. |
| implementation-apis | exercised | Typed helper/build receipts preserved fail-closed ownership. |
| dependencies-build | exercised | A named minimal native fixture build now precedes the isolated mutation probe. |
| testing | exercised | Real mutation proof, route baseline, build-marker removal inversion, and audit-binding mutants passed. |
| evidence | exercised | 49 observed declarations; verification and ship readiness green. |
| runtime-playtest | partial | Real Chromium journeys passed on the hosted timing head; no gameplay playtest was required. |
| performance | exercised | Exact hosted aggregate and helper timings proved the target and isolated the functional defect. |
| documentation | exercised | CI qualification documentation records the fixture exception. |
| packaging-upgrade | not-exercised | No package or upgrade work occurred. |
| worker-git-pr | exercised | Repair-phase round 4 was recorded; round 5 remains pre-push. |
