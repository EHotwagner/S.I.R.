---
feedbackSchema: 2
date: 2026-08-27
workspace: S.I.R-352
cycle: item-352-quint-q4-sir-adoption
lane: sdd
toolVersion: 1.0.1
commit: f8cf9b1264a974d9b12f59bbec52d30e21115834
---

# Quint Q4 standalone adoption feedback

## §1 Provenance and confidence

This cycle repaired and landed the standalone, explicitly noncanonical Quint Q4 combat model. The repository pins FS.GG.SDD 1.0.1, Quint 0.32.0, and FS.GG.Game.Core 0.13.0. The cycle boundary is the original PR #355 implementation through commit `f8cf9b1264a974d9b12f59bbec52d30e21115834`. Two checkpoint events are recorded in `feedback/checkpoints/item-352-quint-q4-sir-adoption.jsonl`. Confidence is high for local model, runtime-correspondence, documentation, evidence, verify, and ship behavior. Hosted exact-head CI and merge outcome were pending when this report was drafted.

## §2 What worked

The focused qualification composed literate extraction, Quint typecheck/test/run, observed-red mutations, and real-interpreter replay into one deterministic command. SDD 1.0.1 accepted eight observed evidence obligations plus four explicit upstream deferrals and produced a ship-ready standalone disposition without treating the unpublished canonical backend as complete.

## §3 What did not

The initial draft PR reached hosted CI with completed tasks but no `evidence.yml`, and its otherwise valid documentation used an H1 form prohibited by the repository-wide docs policy. Both faults were caught fail closed, but only after a hosted run.

## §4 Findings

#### §4.1 Completed tasks were proposed without their required evidence declaration

- **Kind:** orchestration
- **Impact:** PR #355 could not satisfy the evidence or joined PR-verdict gates; one hosted CI run failed and a lifecycle authoring pass was required.
- **Expected:** Every completed SDD task has a truthful evidence disposition before a draft is promoted for hosted qualification.
- **Observed:** T001–T008 were `done` while `work/352-quint-q4-sir-adoption/evidence.yml` was absent; the repair added eight observed passes and four explicit deferrals and reached ship-ready.
- **Evidence:** command:test "$(git show 23e2ca00ce5cd1e3f26bff55bb6da99fbca77fb0:work/352-quint-q4-sir-adoption/tasks.yml | grep -c 'status: done')" -eq 8 && test -z "$(git ls-tree -r --name-only 23e2ca00ce5cd1e3f26bff55bb6da99fbca77fb0 -- work/352-quint-q4-sir-adoption/evidence.yml)"; command:gh run view 33046726115 --json conclusion,jobs; file:work/352-quint-q4-sir-adoption/evidence.yml; file:readiness/352-quint-q4-sir-adoption/ship-verdict.json
- **Version:** FS.GG.SDD 1.0.1 at commit f8cf9b1264a974d9b12f59bbec52d30e21115834
- **Owner:** S.I.R. worker/PR orchestration
- **Recurrence:** seen again feedback/2026-08-23-sir-item-272-evidence-gate-absence.md §3/§4.1; EHotwagner/S.I.R#272; current issue state not reverified
- **Avoidable cost:** one failed hosted CI run and one evidence authoring pass; replacement exact-head run pending
- **Disposition:** skill fix

#### §4.2 The focused model gate did not exercise the repository-wide authored-heading policy

- **Kind:** documentation
- **Impact:** The documentation gate failed after the focused Quint model qualification had passed.
- **Expected:** Authored docs entering a PR satisfy the repository-wide ban on redundant `# S.I.R.` headings.
- **Observed:** `docs/rules/sir-combat.md` began with the prohibited heading form; the repair changed it to `# Combat rules in Quint` without changing extracted Quint bytes.
- **Evidence:** file:scripts/verify-docs.mjs; file:docs/rules/sir-combat.md; file:scripts/qualify-quint-q4-sir-combat.sh; command:test "$(git show 23e2ca00ce5cd1e3f26bff55bb6da99fbca77fb0:docs/rules/sir-combat.md | sed -n '1p')" = '# S.I.R. combat rules in Quint' && ! rg -q 'verify-docs|# S[.]I[.]R[.]' scripts/qualify-quint-q4-sir-combat.sh
- **Version:** S.I.R. commit f8cf9b1264a974d9b12f59bbec52d30e21115834
- **Owner:** S.I.R. documentation policy and focused qualification
- **Recurrence:** seen again feedback/2026-08-27-sir-handbook-m1-linked-skeleton.md §4.5; no issue state reverified
- **Avoidable cost:** one heading edit and one failed hosted CI run; replacement exact-head run pending
- **Disposition:** accepted

## §5 Did not exercise

The unpublished general consumer-defined Quint profile, canonical manifest-v2 migration, generated F#/Fable bindings, authenticated migration rollback, and complete package-only Q4 gate were not exercised. They remain explicit deferrals owned after FS-GG/FS.GG.SDD#932.

## §6 Doc-versus-behavior contradictions

None observed. The docs rule and its failure message agreed; the missing step was routing that policy into the focused model gate.

## §7 Workarounds still in the tree

The standalone model and receipt remain intentionally noncanonical until FS-GG/FS.GG.SDD#932 publishes the consumer profile. Their disclosure and deferred evidence prevent this boundary from being mistaken for completed canonical adoption.

## §8 Friction and avoidable cost

One failed hosted CI run, one lifecycle evidence authoring pass, and one documentation heading edit were avoidable; the replacement exact-head run was pending. Rebase and model requalification required no semantic rework.

## §9 Skill value and gaps

`quint-lang` made the pinned CLI/type/action/witness contract explicit during model verification. `fs-gg-sdd-evidence`, `fs-gg-sdd-verify`, and `fs-gg-sdd-ship` provided the correct observed-run and deferral grammar. `fs-gg-feedback-report` preserved the two repair findings. No additional skill gap was observed.

## §10 Outcome markers

The repaired focused gate passed six witnesses, seven invariants over 64 samples × 8 steps, six observed-red mutations, and 16 interpreter traces covering 144 states. SDD reached verification-ready and ship-ready with eight observed passes, four deferrals, and zero synthetic evidence. Hosted exact-head green and merge were pending at draft time.

## §11 Falsifiable improvements

- For §4.1, S.I.R. worker/PR orchestration should run the evidence gate before first push whenever `tasks.yml` contains `status: done`; acceptance is a clean-checkout control that refuses PR promotion before `evidence.yml` exists.
- §4.2 is duplicate recurrence evidence for the already-recorded handbook M1 publication-policy gap; no second improvement is proposed here.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing generated product; no scaffold activity. |
| onboarding-guidance | partial | Repository AGENTS guidance and current generated lifecycle state were read. |
| skills | exercised | Quint language, SDD evidence/verify/ship, and feedback guidance were applied. |
| sdd-authoring | exercised | Evidence was authored and analyze/evidence/verify/ship completed. |
| implementation-apis | exercised | Production CombatRules interpreter replay accepted 16 traces and 144 states. |
| dependencies-build | exercised | Locked restore and native/Fable compatibility passed in the focused gate. |
| testing | exercised | Six witnesses, seven invariants, six mutations, and runtime replay passed. |
| evidence | exercised | Eight observed passes and four explicit deferrals reached ship-ready. |
| runtime-playtest | not-exercised | No interactive or production journey was in scope. |
| performance | not-exercised | No performance claim was made. |
| documentation | exercised | Repository docs policy was reproduced and repaired. |
| packaging-upgrade | not-exercised | The producer profile remains unpublished and deferred. |
| worker-git-pr | exercised | Rebase, hosted failure diagnosis, and PR repair were exercised. |
