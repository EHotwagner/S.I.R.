---
feedbackSchema: 2
date: 2026-08-16
workspace: S.I.R
cycle: item-215-single-pass-qualification
lane: sdd
toolVersion: 1.0.1
commit: 7ac67e0ec0616a1b53a7ba234884037e018c4c18
---

## §1 Provenance and confidence

This is the immutable repair-round report following the first cycle report. It describes commit `7ac67e0ec0616a1b53a7ba234884037e018c4c18`, SDD CLI 1.0.1, Fable 5.13.0, .NET SDK 10.0.302, Node v26.7.0, and npm 12.0.2. The cycle checkpoint stream contains six events. The final same-host timing receipt binds clean baseline commit/tree `dab492e…`/`724970b…`, clean candidate commit/tree `032fbea…`/`a688c18…`, and host digest `162c45a7…`. Local acceptance is complete; exact repaired-head hosted CI, critic confirmation, merge, and post-merge obligations remain pending.

## §2 What worked

Cold review found four concrete boundary gaps before merge. Focused fixtures repaired path ownership, site-receipt stale/missing restoration, process-boundary observation, and timing provenance without another baseline run. A deterministic fixture proved concurrent scheduling, parity of its isolated target outputs, and joined failure handling. The final real aggregate retained all conformance, delivery, browser, documentation, and accessibility subjects and passed at 284,842 ms versus the 376,975 ms baseline.

## §3 What did not

The first review candidate had only build and conformance receipts; the post-doc site output was not immutably bound. Its cooperative build-script counter did not observe direct `dotnet fable` processes, and timing evidence omitted exact clean commit/tree and shared-host provenance. A repaired process allowlist initially modeled only the two production projects and failed closed when it observed the two legitimate Fable conformance projects. A later candidate passed functional gates but missed the performance threshold. No threshold or retained subject was weakened.

## §4 Findings

#### §4.1 A three-receipt chain closes the post-documentation output boundary

- **Kind:** positive-pattern
- **Impact:** Reviewers can verify build, conformance, and final site outputs independently, and stale or missing site bytes cannot be silently reused.
- **Expected:** Every reused output, including the site produced after documentation, is content-addressed and linked to the receipts it consumes.
- **Observed:** The final site receipt binds `artifacts/site` plus the exact build and conformance receipts. The production mutation uses the real verifier for stale and missing site subjects and restores the original bytes in both cases.
- **Evidence:** file:docs/evidence/production-build-receipt-v1/55f473f678cb02d829b93c069413155f9ad73aef4c5d7ce8d9b1e0540cdeb55d.json; file:readiness/215-single-pass-qualification/single-pass-timing.json; command:npm run test:production-build-receipt
- **Version:** receipt schema `sir.production-build-receipt/v1`; timing schema `sir.production-qualification-timing/v2`
- **Owner:** EHotwagner/S.I.R. production qualification
- **Recurrence:** new repair pattern; the missing post-documentation receipt was first identified by the independent review on EHotwagner/S.I.R.#217 and was not a finding in the earlier cycle report.
- **Avoidable cost:** one review repair round
- **Disposition:** product fix

#### §4.2 Process-boundary inventory plus parallel targets preserved exactness and recovered material speed

- **Kind:** positive-pattern
- **Impact:** Qualification detects every Fable process, rejects duplicate or unknown projects, and still exceeds the required 20 percent wall-time reduction.
- **Expected:** The exact declared Fable project set runs once each regardless of scheduling order, with Vite starting only after both production targets succeed.
- **Observed:** A PATH-boundary shim recorded the two conformance and two production projects. The verifier canonicalizes the set but preserves multiplicity, so real duplicate and unknown-project mutations are red. A deterministic fake-process fixture proves concurrent scheduling, isolated-output parity, and fixed-order joined failures. Separately, the real final qualification measured 284,842 ms against 376,975 ms, a 2,444-basis-point reduction; the evidence does not isolate how much of that aggregate gain came from concurrency.
- **Evidence:** file:scripts/test-build-client-fable-parallel.sh; file:scripts/test-dotnet-invocation-trace.mjs; file:readiness/215-single-pass-qualification/single-pass-timing.json; command:npm run test:production-build-receipt
- **Version:** Fable 5.13.0; Node v26.7.0
- **Owner:** EHotwagner/S.I.R. production qualification
- **Recurrence:** new
- **Avoidable cost:** one review repair round
- **Disposition:** product fix

## §5 Did not exercise

No scaffold creation, gameplay/client behavior change, package publishing, or dependency upgrade was exercised. Hosted repaired-head CI, merge, and post-merge done/release were not yet exercised.

## §6 Doc-versus-behavior contradictions

The pre-repair process documentation described only the production Fable pair while the real qualification graph also contained two Fable conformance projects. The product documentation and verifier now declare the complete four-project exact-once set.

## §7 Workarounds still in the tree

None. The process shim, receipt chain, and parallel target helper are permanent fail-closed qualification boundaries rather than bypasses.

## §8 Friction and avoidable cost

Cold review required one repair round. One repaired candidate failed because the process allowlist omitted two legitimate conformance projects; a later candidate passed every functional gate but missed timing. The structural repair then required focused concurrency/parity work and one final aggregate. The sealed baseline was reused rather than rerun. Exact intermediate timing and real parity measurements were not committed, so this report does not treat them as durable evidence.

## §9 Skill value and gaps

The next-item, intra-repository coordination, SDD lifecycle/stage, performance, and feedback-report skills were exercised. The feedback skill preserved the earlier report and required this repair delta as a new immutable report. The performance workflow’s fixed threshold forced a structural improvement instead of accepting a noisy slow run. No gameplay, playtest, docs-authoring, or packaging skill was invoked because those product surfaces were unchanged.

## §10 Outcome markers

Focused receipt, process-inventory, synthetic output-parity, and deterministic parallel-failure fixtures passed. Final real qualification passed at 284,842 ms versus 376,975 ms, a 24.44 percent reduction. All three receipts verified; stale build, stale site, and missing site mutations failed closed and restored. SDD evidence remains 39/39 observed and ship-ready. Hosted CI and merge remain pending.

## §11 Falsifiable improvements

- Preserve §4.1 as a merge invariant: deleting or changing any site file, build receipt, or conformance receipt must make site-receipt verification red before reuse, while the stale and missing mutations must restore exact bytes.
- Preserve §4.2 as a process invariant: the declared four-project set must pass in any scheduling order, while a duplicate or unknown direct `dotnet fable` invocation must fail the unchanged verifier. The deterministic fixture must retain isolated-output parity and fixed-order joined failures.
- For future timing comparisons, require both baseline and candidate receipts to bind exact clean commit/tree plus byte-identical host fingerprint before the clocked command begins; acceptance is refusal before work on any mismatch.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing scaffold retained. |
| onboarding-guidance | partial | Existing generated guidance refreshed; no onboarding run. |
| skills | exercised | Coordination, SDD, performance, and feedback skills drove the repair. |
| sdd-authoring | exercised | 39/39 observed obligations and ship readiness refreshed metadata-only. |
| implementation-apis | exercised | Receipt, process shim, and target-build scripts were changed and tested. |
| dependencies-build | exercised | Full Fable 5.13 qualification passed; synthetic target parity fixture passed. |
| testing | exercised | Focused mutations, process inventory, parallel failure, and retained aggregate subjects passed. |
| evidence | exercised | Three immutable receipts and timing v2 were committed. |
| runtime-playtest | not-exercised | No gameplay behavior changed. |
| performance | exercised | Same-host paired aggregate wall-time was measured; focused concurrency fixture passed. |
| documentation | exercised | Site build, experience, smoke, accessibility, and site receipt passed. |
| packaging-upgrade | not-exercised | No package publish or upgrade. |
| worker-git-pr | partial | Review repair and local seal completed; hosted CI/merge pending. |
