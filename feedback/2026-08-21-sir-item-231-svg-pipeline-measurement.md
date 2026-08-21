---
feedbackSchema: 2
date: 2026-08-21
workspace: S.I.R
cycle: item-231-svg-pipeline-measurement
lane: sdd
toolVersion: 1.0.1
commit: 4249b0baf12d352a3a6aa696cf5e0bc9fb7950f6
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 1
- **zero-event reason:** n/a

This recovery cycle resumed issue #231 and PR #238 after blocker #239 landed. It used SDD CLI 1.0.1, Node v26.7.0, Fable 5.13.0, and Google Chrome for Testing 151.0.7922.34. The coordination engine was explicitly overridden during recovery, but its exact version is not independently durable in this report. The cycle checkpoint is `feedback/checkpoints/item-231-svg-pipeline-measurement.jsonl`. The retained production authority binds the rebased candidate commit/tree, Release client manifest, server assembly, fixture definition, 35 ordered trace identities, and raw-summary identity. Report commit `4249b0b…` is an evidence-only descendant of measured candidate `8ed4606…`. Confidence is limited to the production routes and Chromium trace capabilities represented by the versioned matrix; the critic did not independently inspect the ignored raw traces or summary, unavailable worker-transfer, projection-allocation, and source-isolated Elmish/React stages remain unresolved, and no live GPU counter beyond Chromium trace events is claimed.

## §2 What worked

The existing measurement contract survived a clean rebase onto the durable rules-corpus fix. Its 13 focused mutation gates still rejected malformed workload axes and coordinated receipt reseals. One Release build and one 35-route Chromium matrix renewed the exact candidate authority without running the repository aggregate locally. SDD verify and ship remained current with 25 observed evidence obligations, 25 observed test obligations, zero invalid evidence, `verificationReady`, and `shipReady`.

## §3 What did not

The recovered In-review item carried a valid legacy v1 delivery-route judgement but no current structured v2 record. Its durable comment chronology shows that recovery recorded an explicit v2 restatement of the same SDD route before a non-force claim marker converged. This is a residual surface of the missing-current-route cause documented in FS-GG/.github#2698, reached through an older in-flight item rather than a newly boarded Ready row.

## §4 Findings

#### §4.1 Exact-candidate performance authority made base-update requalification bounded

- **Kind:** positive-pattern
- **Impact:** Maintainers could requalify a changed base with one production matrix and inspect exactly which candidate, build, fixture, traces, and raw summary support the renderer decision.
- **Expected:** A performance-sensitive open PR whose base changes retains renewed production evidence without treating prior measurements as current or imposing a permanent project-size ceiling.
- **Observed:** The rebased candidate completed 35 of 35 production Chromium routes. The compact receipt and separate authority bind candidate `8ed4606aab4aa9623b7e94e49b7e46dfbcc6020e`, five fixtures, seven journeys, Release build hashes, raw summary `945a354e…`, and ordered trace digest `0ca114e2…`. Generic main-thread script remained the largest available measured stage at 94.02 percent; unavailable stages stayed unresolved.
- **Evidence:** file:work/231-svg-pipeline-measurement/production-chromium-authority.json; file:work/231-svg-pipeline-measurement/production-chromium-evidence.json; command:npm run test:svg-pipeline-measurement
- **Version:** measurement schemas v1; Chrome for Testing 151.0.7922.34; candidate 8ed4606aab4aa9623b7e94e49b7e46dfbcc6020e
- **Owner:** EHotwagner/S.I.R. SVG pipeline measurement harness
- **Recurrence:** seen again after `feedback/2026-08-20-sir-item-231-svg-pipeline-measurement.md §4.1`; renewed evidence after the #239 base update
- **Avoidable cost:** none; one matrix was required because the measured server build and base changed
- **Disposition:** accepted

#### §4.2 In-flight v1 route receipts have no current-ledger recovery projection

- **Kind:** orchestration
- **Impact:** Recovery of an open PR in `In review` required manually restating its already-decided SDD route in the v2 schema before the claim chronology could proceed.
- **Expected:** Recovery guidance or a migration projection identifies an older valid route judgement and produces or requests the current structured receipt before the claim boundary.
- **Observed:** Issue #231 carried legacy comment 5360341475. Structured comment 5362965431 then recorded the same SDD judgement, and non-force claim comment 5362966481 followed. The original terminal diagnostic and exact engine version are not promoted as independently durable facts.
- **Evidence:** issue:EHotwagner/S.I.R.#231; issue-comment:5360341475; issue-comment:5362965431; issue-comment:5362966481; issue-comment:5363148379
- **Version:** legacy receipt schema `fsgg:delivery-route/v1`; current schema `fsgg.coord.route-decision/v2`; exact recovery-engine version unverified in durable evidence
- **Owner:** FS-GG/.github coordination recovery and route-ledger migration guidance
- **Recurrence:** seen again after FS-GG/.github#2698; increment https://github.com/FS-GG/.github/issues/2698#issuecomment-5363148379 records that the delivered Ready-seam remedy does not cover an older In-review item
- **Avoidable cost:** one manual receipt migration before claim
- **Disposition:** existing issue

## §5 Did not exercise

No scaffold creation, package publication, dependency upgrade, gameplay rule change, live GPU tooling, or compatibility migration was exercised. Hosted exact-head CI, host acceptance, merge, and post-merge done remained pending at this report boundary.

## §6 Doc-versus-behavior contradictions

The pnext recovery contract requires a current structured route before claim. The guidance did not state how to migrate a valid legacy v1 decision on an already In-review PR; §4.2 records the resulting recovery gap without asserting an independently unpreserved terminal transcript.

## §7 Workarounds still in the tree

None. The route migration is an append-only board receipt, not a product-tree bypass. Production trace artifacts remain ignored and the compact tracked authority retains their identities.

## §8 Friction and avoidable cost

One manual v2 restatement was required before the claim marker. The feature branch rebased without conflict. The 13 focused gates ran during iteration; the one 35-route production matrix ran only at the completed-feature boundary. No full local aggregate CI was run.

## §9 Skill value and gaps

`work-board-best`, `pnext-item`, the performance-first gate, SDD verify/ship, `fs-gg-feedback-report`, and cross-repository coordination were exercised. Performance-first guidance kept the expensive matrix at the candidate boundary. The recovery guidance gap is §4.2: it requires the current receipt but does not distinguish migration of an already-authored legacy judgement from creation of a new judgement.

## §10 Outcome markers

Fresh identity, explicit v2 restatement, and non-force claim are preserved in the issue chronology. Focused gates passed 13 mutation cases. The rebased production matrix passed 35/35 routes. SDD verify reported 25 observed evidence and 25 observed test obligations with zero invalid entries; ship reported `shipReady`. PR #238 update, hosted checks, review, acceptance, merge, and done remained pending.

## §11 Falsifiable improvements

- For §4.1, retain the exact candidate/build/fixture/raw-summary/ordered-trace equality checks and coordinated reseal mutations. Acceptance remains a nonzero focused-test exit when any authority identity is changed and consistently resealed.
- For §4.2, extend recovery guidance or tooling with a fixture containing only a valid v1 receipt plus an open In-review PR. Acceptance is either a named migration action before claim or an explicit statement that the route must be re-authored, with no bare `take`, forced claim, or inferred route.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing S.I.R. scaffold retained. |
| onboarding-guidance | exercised | Current route, identity, unclaimed state, explicit claim, and isolated recovery worktree were verified. |
| skills | exercised | Board, item, performance-first, SDD, feedback, and cross-repository contracts drove recovery. |
| sdd-authoring | partial | Existing authored package was not rewritten; verify and ship were refreshed and remained current. |
| implementation-apis | partial | Production measurement consumed existing UI and trace interfaces; runtime APIs were unchanged. |
| dependencies-build | exercised | Locked Node dependencies and Release client/server build completed once. |
| testing | exercised | 13 focused mutations and 35 production Chromium routes passed. |
| evidence | exercised | Compact receipt, separate authority, SDD verify, and ship bindings were renewed. |
| runtime-playtest | exercised | Production controls drove idle, playback, pan, zoom, selection, modality, and overlay journeys. |
| performance | exercised | Five fixtures by seven journeys renewed the measured pipeline evidence. |
| documentation | exercised | Existing performance contract and a recovery feedback report were reviewed and updated. |
| packaging-upgrade | not-exercised | No package version or publication changed. |
| worker-git-pr | exercised | Fresh identity, route migration, explicit claim, rebase, heartbeat, and PR recovery were exercised. |
