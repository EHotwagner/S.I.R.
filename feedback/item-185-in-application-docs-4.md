---
feedbackSchema: 2
date: 2026-08-16
workspace: S.I.R
cycle: item-185-in-application-docs
lane: sdd
toolVersion: 1.0.0
commit: 287c950e9c32f637469f542929aaa7ef702325a5
---

## §1 Provenance and confidence

- **activation:** active; this is the hosted-acceptance repair addendum after `feedback/item-185-in-application-docs-3.md`.
- **phases:** implementation-test-evidence, verify-ship-pr.
- **material events:** 10 checkpoint records at the described commit; this addendum owns the failed hosted run, its bounded repair, and the repaired-head historical-evidence boundary.
- **checkpoint:** `feedback/checkpoints/item-185-in-application-docs.jsonl`.
- **confidence limits:** The first hosted recovery run, focused Release/Fable build, seven repaired browser subjects, local protected-subject probes, actual headless-browser performance, and SDD 1.0.0 verify/ship were exercised. The replacement PR, its sole hosted run, fresh exact-head product review, merge, and post-merge checks remain outside this report. No live compositor was available.

## §2 What worked

Freezing product source before the evidence seal kept the repair lean. The seven hosted failures were reduced to five owned contracts, fixed once, and verified with a seven-test browser receipt plus subject-specific inversions. The later commit changes only feedback, readiness, and work metadata, so SDD returned to 54/54 observed obligations and `shipReady` without rebuilding the product or rerunning the broad loader aggregate.

## §3 What did not

Hosted run 31961720372 failed seven browser tests even though every non-browser producer was green. The failures combined a lost `isBehindDraft` guard, two contextual Docs controls absent from the command-registry disclosure, a test-owned Rules source SHA, a second hard-coded initial-route ceiling, and a one-pixel zoom assumption that differed across Chromium executables. The shared symptom was not product size: acceptance contracts had drifted away from their owning implementation or registry.

## §4 Findings

#### §4.1 Browser acceptance subjects need one owning contract and a protected inversion

- **Kind:** quality-gap
- **Impact:** The recovery spent one hosted run discovering seven failures across Docs, Rules, delivery, and simulator journeys; retrying or optimizing bundle source would not have repaired the contract drift.
- **Expected:** Each acceptance threshold or identity should be derived from its owning product contract, and each portability allowance should retain a mutant that proves material regressions still fail.
- **Observed:** The bounded repair restored the simulator's `isBehindDraft` guard; marked two contextual Docs controls explicitly unassigned with accessible disclosure; derived the Rules SHA from `implementation-sources.json`; moved initial and Rules-activation response ceilings into registry v2; and bounded 400%-zoom rounding while retaining a material-overflow mutation subject. The committed JUnit records seven green focused browser subjects, and the retained performance receipt passed with 242 DOM nodes, 1.004 ms representative-search p95, and 11.742 ms construction p95. No unrelated size optimization was performed. Local inversion probes were observed during repair but are not claimed as separately retained exact-head receipts here; the replacement hosted run remains the acceptance boundary.
- **Evidence:** run:https://github.com/EHotwagner/S.I.R./actions/runs/31961720372; command:git diff 78800cf9d8ead197ddbfcb2eb10109ee5ab58222..35882dc4768cac0184f3bdd7d99b7501adc74690 -- src/SIR.Client.Web scripts/test-client-feature-loader.mjs tests/SIR.Browser.Tests docs/client-feature-loader.md work/185-in-app-docs-modality; file:readiness/185-in-app-docs-modality/in-app-docs-browser.junit.xml; file:readiness/185-in-app-docs-modality/performance-after.json; file:src/SIR.Client.Web/feature-registry.v2.json; command:git diff --name-only 35882dc4768cac0184f3bdd7d99b7501adc74690..287c950e9c32f637469f542929aaa7ef702325a5
- **Version:** hosted run 31961720372 at 78800cf9d8ead197ddbfcb2eb10109ee5ab58222; repaired source commit 35882dc4768cac0184f3bdd7d99b7501adc74690; checkpoint commit 287c950e9c32f637469f542929aaa7ef702325a5.
- **Owner:** EHotwagner/S.I.R. browser acceptance, WorkspaceTransitions, and client feature-registry surfaces
- **Recurrence:** new hosted evidence under existing issue #185, refining `feedback/item-185-in-application-docs-3.md §4.1` and closed issues #214/#154 for route-budget ownership; source binding recurs from `feedback/2026-08-15-SIR-186-6.md §4.3`, inversion policy from its §4.6, command disclosure from `feedback/2026-08-11-sir-item-143-shortcut-command-registry.md §4.1`, and transition reconciliation from `feedback/item-179-continuous-simulation-state.md §4.2/§4.4/§4.6`. No separate issue is warranted.
- **Avoidable cost:** one failed hosted run plus focused local mutation probes.
- **Disposition:** product fix; keep route/source identities under their owning receipts and require a protected inversion for every portability tolerance.

#### §4.2 The necessary browser-gate repairs invalidate four additional historical bindings

- **Kind:** orchestration
- **Impact:** The previously accepted five historical item-186 invalidations remain, and the required edits to the two shared browser acceptance files invalidate four more digest-bound citations. Claiming those historical bindings are still current would be false; rewriting the merged reports would erase what their reviews actually observed.
- **Expected:** The fixed commit-aware checker indexes only audits present in `origin/main`, reports every changed bound path, and the current cycle records intentional invalidations additively without editing historical reports.
- **Observed:** At the repaired metadata head the authoritative checker reports nine base-present bindings. The prior five are unchanged. Four are new and directly caused by the authorized acceptance repairs: item-183 §4.2 binds `visible-workflows.spec.js`; item-186-6 §4.1 binds `production-delivery.spec.js`; item-186-6 §4.3 binds `visible-workflows.spec.js`; and item-192 §4.1 binds `visible-workflows.spec.js`. No historical report or audit was edited.
- **Evidence:** command:dotnet fsi ../FS.GG.Rendering/template/feedback-report/skill/scripts/feedback-tool.fsx -- check-invalidation --base 97478f789480f1adf190c777f26cb619b18d1dd5 --head 287c950e9c32f637469f542929aaa7ef702325a5 --root .; command:git diff --name-only 97478f789480f1adf190c777f26cb619b18d1dd5...287c950e9c32f637469f542929aaa7ef702325a5 -- feedback
- **Version:** authoritative feedback producer commit 219b198dda7dfa9ad4ce234d57dff6e0299aeaec; repaired checkpoint commit 287c950e9c32f637469f542929aaa7ef702325a5.
- **Owner:** EHotwagner/S.I.R. feedback evidence boundary and shared browser acceptance files
- **Recurrence:** continuation of `feedback/item-185-in-application-docs-3.md §4.2`; that report records the original five, while this repair adds four exact browser-gate bindings.
- **Avoidable cost:** one authoritative recheck and additive audit record; no historical rewrite.
- **Disposition:** accepted boundary for #185; preserve all nine exact locators as invalidated and require future consumers not to treat them as current evidence.

## §5 Did not exercise

No package publication, dependency upgrade, persistence format, simulation algorithm, public network protocol, remote Markdown source, or live-compositor frame rate changed or is claimed.

## §6 Doc-versus-behavior contradictions

The earlier production-delivery test owned a 1,250,000-byte initial-route ceiling separately from the feature registry. Registry v2 and `docs/client-feature-loader.md` now agree that the initial and deferred activation ceilings are registry-owned and explicitly versioned.

## §7 Workarounds still in the tree

None added by this repair. The two-pixel zoom tolerance is bounded by control containment and a material-overflow mutant; it is not an unconditional overflow exemption.

## §8 Friction and avoidable cost

One hosted run failed after non-browser gates were green. The repair used one focused compile/build pass, seven focused browser subjects, and local real-subject mutation probes. The source-freeze-to-metadata diff contains only feedback, readiness, and work paths; no second aggregate loader build or opportunistic size reduction was run.

## §9 Skill value and gaps

The SDD evidence synchronization made regenerated JUnit receipts fail closed until both browser and performance digests were restamped, then reported 54/54 observed obligations. The feedback contract preserved the hosted failure as a reusable checkpoint instead of hiding it behind the later green result. The workflow still relies on discipline to keep browser thresholds derived from canonical receipts.

## §10 Outcome markers

Hosted run 31961720372 finished with all named non-browser producers green and browser at 30 passed, 1 skipped, 7 failed. Repaired source commit 35882dc passed the seven retained focused browser subjects. The committed performance receipt records 242 Docs DOM nodes, representative-search p95 1.004 ms, and full-construction p95 11.742 ms. Registry v2 owns 1,310,720-byte initial and 65,536-byte Rules-activation ceilings. SDD reports 54 observed non-synthetic obligations and `shipReady`.

## §11 Falsifiable improvements

- Every browser budget or source identity must be read from one versioned owning receipt or registry; a repository search for a second numeric/SHA literal must return no independent contract.
- Every cross-executable tolerance must pair the bounded green condition with a real-subject mutant that exceeds the tolerance and exits nonzero.
- Recovery automation should stop after the first hosted run, classify failures by owning contract, and permit a second run only on a sealed replacement head; acceptance is one green hosted run, not repeated speculative retries.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing product recovery; no scaffold changes. |
| onboarding-guidance | not-exercised | Covered by earlier item-185 reports. |
| skills | exercised | SDD, feedback, pnext, and intra-repository coordination contracts applied. |
| sdd-authoring | exercised | Clarification and plan ownership updated; 54/54 observed and shipReady. |
| implementation-apis | exercised | Workspace transition guard and explicit contextual-control metadata compiled and passed browser journeys. |
| dependencies-build | exercised | Focused Release/Fable build passed; no dependency changed. |
| testing | exercised | Seven repaired browser subjects are retained green; local protected-subject probes were observed red, with replacement hosted acceptance pending. |
| evidence | exercised | Exact-source-head browser/performance receipts synchronized into SDD evidence. |
| runtime-playtest | exercised | Real production controls covered Docs, Rules, delivery, and simulator state transitions. |
| performance | exercised | Route bytes, actual DOM, search, and construction were measured; no compositor claim. |
| documentation | exercised | Loader documentation now names registry-owned route ceilings. |
| packaging-upgrade | not-exercised | No package or tooling upgrade. |
| worker-git-pr | partial | First hosted run diagnosed; replacement PR/run, exact-head review, merge, and post-merge remain pending. |

