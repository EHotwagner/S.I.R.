---
feedbackSchema: 2
date: 2026-08-16
workspace: S.I.R
cycle: 220-bounded-pr-ci
lane: sdd
toolVersion: 1.0.1
commit: 7d07d3b6b0c771931dcbd1d1fa3193e83f01ba2d
---

## §1 Provenance and confidence

This report describes the repaired source boundary at commit `7d07d3b6b0c771931dcbd1d1fa3193e83f01ba2d`, based on `origin/main` commit `ee6a2df`. The cycle used .NET SDK 10.0.302, Fable 5.13.0, Node v26.7.0, npm 12.0.2, and SDD CLI 1.0.1. Four checkpoint events are recorded. One source-frozen local aggregate reached the old paired wall-time assertion and failed there; it exited before writing a durable timing receipt, so this report does not treat its exact duration or complete preceding subject inventory as audited evidence. No aggregate retry was run. The first hosted run exposed six transport, binding, aggregation, timing, and protected-route defects; the canonical independent review required their repair. Focused mutation proofs cover the repair, while replacement exact-head hosted CI, review confirmation, merge, and post-merge obligations remain pending.

The fresh-context feedback critic verified §4.1 under the stated confidence limit. The PR critic independently classified the initial hosted head changes-required; this repair preserves that review provenance rather than treating the original run as acceptance.

## §2 What worked

The versioned route, candidate/route/artifact-bound gate receipts, all-failure deterministic join, mode-preserving immutable prepared-candidate transport, and explicit workflow DAG make the PR acceptance surface inspectable and independently attributable. Lifecycle metadata is neutral when mixed with a product change, so adding SDD or feedback records does not promote a focused product route to cross-cutting; metadata-only changes retain an evidence-only route. Focused route, join, workflow, scheduled-full mutation, archive, mode, and receipt tests found defects quickly without an aggregate loop.

## §3 What did not

The one aggregate reached item #215's frozen-baseline assertion after the qualification surface had changed. The collector observed a negative comparison, but the failed run did not preserve a tracked timing receipt or complete baseline-versus-candidate inventory comparison. The correction kept the threshold unchanged on an explicit paired experiment and made normal local/protected commands emit clean-room timing without comparing an expanding surface to a permanent baseline. The first hosted run then showed that raw artifact upload discarded executable modes, the evidence lane assumed a warm package cache, gate results were under-bound, the join lost later failures, timing began too late, and the protected route consumed no-restore outputs before a cold restore while tracing an incomplete Fable inventory. Round one repaired exactly those findings and added focused fail-closed mutations; replacement hosted evidence remains pending.

## §4 Findings

#### §4.1 Neutral lifecycle metadata preserves focused product routes

- **Kind:** positive-pattern
- **Impact:** Later feature PRs can commit their required `work/`, `readiness/`, and `feedback/` records without being forced from a domain, browser, or documentation route into every expensive gate.
- **Expected:** Required lifecycle metadata should preserve the route selected by the actual product paths, while metadata-only changes still receive integrity and evidence checks and unknown product paths fail closed.
- **Observed:** The versioned classifier removes evidence-only facts before selecting the product classification. Focused tests cover documentation plus work metadata, domain plus readiness metadata, metadata-only, mixed product paths, unknown paths, malformed paths, deterministic join failures, and the workflow DAG contract.
- **Evidence:** file:scripts/ci-route.mjs; file:scripts/test-ci-route.mjs; file:.github/workflows/ci.yml; command:npm run test:ci-route
- **Version:** route schema `sir.ci-route/v1`; gate/join/timing schemas v1
- **Owner:** EHotwagner/S.I.R. PR CI route policy
- **Recurrence:** new
- **Avoidable cost:** none
- **Disposition:** product fix

## §5 Did not exercise

No scaffold creation, gameplay behavior change, package publishing, dependency upgrade, or interactive runtime playtest was exercised. The replacement hosted PR DAG, externally unobservable queue timing, merge, and post-merge done stamp remain pending.

## §6 Doc-versus-behavior contradictions

Before the correction, `docs/production-qualification.md` said timing was acceptance evidence rather than a runtime correctness threshold, while the default command made the historical reduction threshold its final functional exit condition. The document and command now agree: the named paired experiment owns that historical assertion.

## §7 Workarounds still in the tree

None. The route policy, typed receipts, artifact manifest, clean-room timing route, and explicit paired experiment are permanent fail-closed boundaries, not bypasses.

## §8 Friction and avoidable cost

The stale paired comparison consumed the only local aggregate. Diagnosis and correction used static/focused checks and did not restart the aggregate. The failed command did not preserve a tracked terminal timing receipt, so exact elapsed cost is intentionally omitted. SDD evidence required one correction from authored pass declarations to an observed JUnit receipt; no product source changed during that correction.

## §9 Skill value and gaps

The next-item and intra-repository coordination skills established isolated ownership and overlap refusal. The SDD lifecycle made 49 task obligations explicit and refused self-attested evidence until an observed report was attached. The feedback skill preserved the non-comparable timing failure as a durable process finding. No gameplay, runtime-playtest, API-documentation, or packaging skill was relevant to the CI-only scope.

## §10 Outcome markers

Route, join, workflow, scheduled/protected-edge mutation, archive/mode receipt, Fable inventory, shell-syntax, and diff focused checks passed at the repair boundary. The sole local aggregate reached the historical timing assertion and failed there; its complete preceding pass inventory is not claimed as durable evidence, and no retry occurred. SDD has 49/49 observed evidence declarations, verification readiness, and ship readiness. Replacement exact-head hosted CI and confirmation by the same independent reviewer remain pending.

## §11 Falsifiable improvements

- Preserve §4.1: a domain path plus any combination of `work/`, `readiness/`, and `feedback/` paths must select the domain route; metadata-only must select evidence; any unknown product path must select cross-cutting.
- Preserve the §3 timing-contract correction: default local and protected aggregates must never load the frozen paired baseline, while `--paired-optimization` must retain host identity validation and the unchanged 2,000-basis-point threshold. Supplying paired and protected flags together must fail before qualification begins.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing scaffold retained. |
| onboarding-guidance | partial | Generated guidance was refreshed; no onboarding run. |
| skills | exercised | Coordination, SDD, and feedback skills drove the cycle. |
| sdd-authoring | exercised | Full charter-through-ship lifecycle reached 49/49 observed and ship-ready. |
| implementation-apis | exercised | Route, receipt, manifest, gate, and join scripts were implemented and tested. |
| dependencies-build | exercised | Existing .NET/Fable/npm build surface passed in the sole aggregate. |
| testing | exercised | Focused route/mutation/receipt tests and retained aggregate subjects ran. |
| evidence | exercised | JUnit-backed SDD evidence reached 49/49 observed. |
| runtime-playtest | not-exercised | No gameplay behavior changed. |
| performance | partial | Product performance subjects passed; hosted PR feedback timing remains pending. |
| documentation | exercised | CI documentation plus the full docs site, smoke, and accessibility subjects passed. |
| packaging-upgrade | not-exercised | No package publish or upgrade. |
| worker-git-pr | partial | Isolated claim and source seal completed; PR review/merge remain pending. |
