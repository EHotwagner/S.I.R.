---
feedbackSchema: 2
date: 2026-08-21
workspace: S.I.R-item-231-svg-measurement-repair-2
cycle: item-231-svg-pipeline-measurement-repair-phase
lane: sdd
toolVersion: 1.0.1
commit: b384f27f11c01715719a9c932a5d34423fec017b
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** independent-review-repair-round-2
- **material events:** 2
- **zero-event reason:** not applicable

This round-2 addendum covers two of the four total events in the stable checkpoint stream: stale SDD source snapshots and hosted critical-path headroom erosion. The stable checkpoint is `feedback/checkpoints/item-231-svg-pipeline-measurement-repair-phase.jsonl`. Confidence is high for the exact committed prepared-part transport, Playwright runtime import, route mutations, and no-change coherent SDD views. Replacement exact-head hosted timing and implementation-review confirmation remain pending and are not claimed.

## §2 What worked

The independent critic distinguished the already-closed hosted-hook finding from two new failures. The SDD CLI exposed exact stale source digests, while the immutable hosted verdict separated 19 passing substantive receipts from the timing-only failure. The existing producer/consumer receipt contract supported a bounded dependency-reuse repair without weakening artifact provenance.

## §3 What did not

The prior repair edited the plan and then ran analyze directly, skipping the plan and evidence authoring commands that renew downstream source snapshots. Separately, every browser consumer repeated a full `npm ci` even though the web producer had already installed the same lockfile-pinned Playwright runtime; the resulting run took 244047 ms and left only 55953 ms headroom.

## §4 Findings

#### §4.1 Manual plan editing without the ordered lifecycle left evidence snapshots stale

- **Kind:** quality-gap
- **Impact:** SDD verify reported `coherent:false` and `evidence.staleEvidenceSource`, so the feedback claim that readiness was resealed was unsupported.
- **Expected:** After a plan changes, the canonical plan → tasks → analyze → evidence → verify → ship order refreshes every source snapshot, and a second exact-head pass is no-change and coherent.
- **Observed:** Reviewed head `aa9851a0ff755e3a08befa1483d6888da819177f` retained old analysis and plan digests in `evidence.yml` because plan and evidence commands had been skipped during reseal.
- **Evidence:** review:https://github.com/EHotwagner/S.I.R./pull/246#issuecomment-5372745179; command:dotnet fsgg-sdd verify --work 231-svg-pipeline-measurement --root . --json
- **Version:** affected head aa9851a0ff755e3a08befa1483d6888da819177f; repaired source b384f27f11c01715719a9c932a5d34423fec017b; SDD CLI 1.0.1
- **Owner:** EHotwagner/S.I.R. SDD lifecycle ordering and committed readiness projections
- **Recurrence:** seen again after PR #203 review https://github.com/EHotwagner/S.I.R./pull/203#issuecomment-5277361440 F6; no earlier durable feedback §4 finding promoted that occurrence; current occurrence is tracked under EHotwagner/S.I.R.#231 and PR #246
- **Avoidable cost:** one confirmation-round finding and one ordered lifecycle reseal
- **Disposition:** product fix

#### §4.2 Browser consumers repeated dependency installation on the hosted critical path

- **Kind:** quality-gap
- **Impact:** All substantive gates passed, but the exact-head verdict was terminal red at 244047 ms, exceeding the 240000 ms target and missing the required 60000 ms headroom by 4047 ms.
- **Expected:** A producer-installed, lockfile-pinned browser runtime is transported once under the existing production receipt and artifact manifest, then reused by consumers without repeating dependency installation.
- **Observed:** Run 32504440703 spent 117425 ms in prepare-web and then up to 79760 ms in a browser consumer whose workflow repeated `npm ci`; the overall critical path ended at 244047 ms with 55953 ms headroom. The failed run did not separately measure npm-ci duration, so repeated installation is an independently verified avoidable critical-path step, not a uniquely established cause of the 4047-ms target miss.
- **Evidence:** review:https://github.com/EHotwagner/S.I.R./pull/246#issuecomment-5372745179; run:https://github.com/EHotwagner/S.I.R./actions/runs/32504440703; command:node scripts/test-ci-route.mjs; command:bash scripts/test-ci-route-mutations.sh
- **Version:** affected head aa9851a0ff755e3a08befa1483d6888da819177f; local repair candidate b384f27f11c01715719a9c932a5d34423fec017b; Playwright 1.62.1
- **Owner:** EHotwagner/S.I.R. PR qualification web-producer/browser-consumer DAG
- **Recurrence:** new repeated-install observation; adjacent hosted timing recurrence is recorded under closed issue #220 and the item-220 feedback series, but those records did not identify repeated browser dependency installation; current repair remains under #231/PR #246
- **Avoidable cost:** one terminal hosted run; no unchanged retry
- **Disposition:** product fix

## §5 Did not exercise

Replacement exact-head hosted qualification, same-critic confirmation round 2, host acceptance, merge, and done remain unexercised.

## §6 Doc-versus-behavior contradictions

None observed.

## §7 Workarounds still in the tree

None. The three transported Playwright package directories are declared web-producer outputs whose bytes are bound by the existing production receipt and artifact manifest.

## §8 Friction and avoidable cost

One stale lifecycle reseal required ordered regeneration. One hosted run was terminal red only on timing after all substantive receipts passed. No unchanged hosted retry or broad local aggregate ran.

## §9 Skill value and gaps

The generated work-model inventory lists automated tests, deterministic JSON, F#, implementation, readiness evidence, and schema versioning. This round exercised automated tests, deterministic receipt/manifest verification, readiness evidence, and schema-versioned feedback. `pnext-item`, `intra-repo-parallel-work`, the performance-first gate, SDD lifecycle skills, `fs-gg-feedback-report`, and the independent-review repair contract governed the round. No gameplay or package-publication skill applied.

## §10 Outcome markers

The committed 35,153,920-byte web archive passed transport and staged-output verification, and its extracted Playwright 1.62.1 runtime loaded without a consumer install. Route baseline and exact missing-runtime/restored-install mutations pass. Analyze, verify, and ship each return `noChange` with `coherent:true`; verify reports 25 observed evidence and 25 observed tests with zero stale or invalid entries, and ship is `shipReady`. Replacement hosted timing remains pending.

## §11 Falsifiable improvements

For §4.1, owner `EHotwagner/S.I.R.` should require the ordered SDD authoring path after a plan edit. Acceptance is a second exact-commit analyze, verify, and ship pass returning `outcome:noChange`, `coherent:true`, current generated views, and zero stale evidence.

For §4.2, owner `EHotwagner/S.I.R.` should preserve Playwright runtime reuse in the web-producer/browser-consumer boundary. Acceptance is that removing any of the three runtime outputs or restoring `npm ci` to any browser consumer fails the route contract, while a changed exact-head hosted verdict passes at no more than 240000 ms with at least 60000 ms headroom and zero retries.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing S.I.R. scaffold retained. |
| onboarding-guidance | partial | Existing claim and repair-review guidance governed the round. |
| skills | exercised | Coordination, performance-first, SDD, feedback, and repair-review contracts were followed. |
| sdd-authoring | exercised | Canonical ordered lifecycle and exact-commit no-change proof passed. |
| implementation-apis | partial | CI producer/consumer transport changed; runtime product APIs did not. |
| dependencies-build | exercised | Lockfile-installed Playwright runtime became receipt-bound prepared output. |
| testing | exercised | Route baseline, missing-runtime/restored-install mutations, F1 mutations, SVG, and audit-binding gates passed. |
| evidence | exercised | Production receipt, artifact manifest, SDD, checkpoint, report, and invalidation checks passed. |
| runtime-playtest | not-exercised | No gameplay change occurred. |
| performance | exercised | Failed immutable timing was diagnosed and critical-path dependency work was removed; replacement hosted proof is pending. |
| documentation | partial | SDD plan and this addendum record the strengthened contracts. |
| packaging-upgrade | not-exercised | No package version or publication changed. |
| worker-git-pr | partial | Live claim and bounded repair discipline were exercised; push and confirmation remain pending. |
