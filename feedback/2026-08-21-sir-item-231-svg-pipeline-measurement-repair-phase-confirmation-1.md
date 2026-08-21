---
feedbackSchema: 2
date: 2026-08-21
workspace: S.I.R-item-231-svg-measurement-repair-2
cycle: item-231-svg-pipeline-measurement-repair-phase
lane: sdd
toolVersion: 1.0.1
commit: debda1e8ee251f88cc8e7c50458e59469c19627c
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** independent-review-repair-round-1
- **material events:** 1
- **zero-event reason:** not applicable

This confirmation-round addendum covers one of the two total events in the stable checkpoint stream, specifically the first independently routed repair finding on PR #246. The stable checkpoint is `feedback/checkpoints/item-231-svg-pipeline-measurement-repair-phase.jsonl`. Confidence is high for the exact local exit-propagation, missing-subject, and non-executable-subject mutations and for the resealed SDD state. The replacement exact-head hosted route and implementation-review confirmation remain pending and are not claimed.

## §2 What worked

The independent critic supplied a minimal reproduction that distinguished propagation from applicability. The focused clean-checkout fixture isolated hosted dispatch with fake non-subject tools, proving exit 37 propagates and exact missing/non-executable subjects fail closed without rerunning the Chromium matrix.

## §3 What did not

The original dispatch used `[[ -x work/<id>/hosted-verification.sh ]]` as both the applicability test and validity check. Removing the executable bit therefore made the validation disappear instead of fail.

## §4 Findings

#### §4.1 Routed hosted verification could disappear when its executable bit was removed

- **Kind:** quality-gap
- **Impact:** A routed hosted-verification owner could be made non-executable and the evidence gate would return green without running its SVG validation.
- **Expected:** An explicitly routed hosted-verification subject is required to exist as a regular, readable, executable file; an invalid subject fails closed, while a valid subject's nonzero exit propagates.
- **Observed:** At reviewed PR head `81b91d07ba9aefb23f27ce6cd9e422b2a1334532`, a hook body `exit 37` propagated red, but changing only its executable bit made `gate evidence` return zero and skip the hook.
- **Evidence:** review:https://github.com/EHotwagner/S.I.R./pull/246#issuecomment-5372458244; command:bash scripts/test-ci-evidence-mutation.sh
- **Version:** affected head 81b91d07ba9aefb23f27ce6cd9e422b2a1334532; repaired source debda1e8ee251f88cc8e7c50458e59469c19627c
- **Owner:** EHotwagner/S.I.R. hosted evidence dispatch in scripts/qualify-pr.sh
- **Recurrence:** new; no matching prior feedback report or open/closed GitHub issue/PR found; PR #221's executable-mode transport failure and PR #225's invalid command-locator failure are analogous but distinct; tracked under EHotwagner/S.I.R.#231 and PR #246
- **Avoidable cost:** one repair-phase confirmation round; no unchanged retry
- **Disposition:** product fix

## §5 Did not exercise

The replacement exact-head hosted qualification, implementation-review confirmation, host acceptance, merge, and done stamp remain unexercised at this boundary.

## §6 Doc-versus-behavior contradictions

None observed.

## §7 Workarounds still in the tree

None. The fake `dotnet` and `npm` binaries exist only inside the mutation test's temporary directory and isolate dispatch behavior.

## §8 Friction and avoidable cost

The repair consumed one focused mutation expansion and one SDD reseal. No unchanged hosted retry or broad local aggregate ran.

## §9 Skill value and gaps

`pnext-item`, `intra-repo-parallel-work`, the performance-first gate, SDD lifecycle skills, `fs-gg-feedback-report`, and the independent-review repair contract governed the round. The critic's exact mutation exposed a contract gap that the prior positive propagation check could not detect. No gameplay or package-publication skill applied.

## §10 Outcome markers

The focused mutation gate passes with exact exit-37 propagation and missing/non-executable routed subjects rejected. CI route baseline and mutation gates pass. SDD analyze is `implementationReady`, verify has 25 observed evidence and 25 observed test obligations with zero invalid or blocking entries, and ship is `shipReady`. Hosted replacement-head timing and review confirmation remain pending.

## §11 Falsifiable improvements

For §4.1, owner `EHotwagner/S.I.R.` should preserve separate applicability and validity checks in `scripts/qualify-pr.sh`. Acceptance is that a routed hook returning 37 makes the evidence gate return 37, and changing only that hook to absent or non-executable makes the evidence gate nonzero with the exact subject diagnostic.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing S.I.R. scaffold retained. |
| onboarding-guidance | partial | Existing board, claim, and review-routing guidance governed entry. |
| skills | exercised | Coordination, performance-first, SDD, feedback, and repair-review contracts were followed. |
| sdd-authoring | exercised | Plan and evidence lineage were updated; analyze, verify, and ship returned ready. |
| implementation-apis | partial | Hosted evidence dispatch was repaired; product runtime APIs were unchanged. |
| dependencies-build | not-exercised | No dependency or product build change was needed. |
| testing | exercised | Exact propagation, absent-subject, and non-executable-subject mutations passed. |
| evidence | exercised | Checkpoint, SDD evidence, route, and audit-binding checks passed. |
| runtime-playtest | not-exercised | No gameplay change occurred. |
| performance | partial | Performance route predicates were preserved; replacement exact-head hosted timing is pending. |
| documentation | partial | SDD plan and this feedback addendum record the strengthened contract. |
| packaging-upgrade | not-exercised | No package version or publication changed. |
| worker-git-pr | partial | Live claim and bounded repair discipline were exercised; push and confirmation remain pending. |
