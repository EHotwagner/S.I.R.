---
feedbackSchema: 2
date: 2026-08-27
workspace: S.I.R-352
cycle: item-352-quint-q4-sir-adoption
lane: sdd
toolVersion: 1.0.1
commit: 60eff02cc49fdbf7e0fcd6d1e4fea8ad9664726d
---

# Quint Q4 review-correction feedback

## §1 Provenance and confidence

This follow-up report preserves the exact-head review repair after the earlier cycle report was finalized. It covers commit `60eff02cc49fdbf7e0fcd6d1e4fea8ad9664726d`, the third checkpoint event, focused qualification, and regenerated SDD views. Confidence is high for the corrected standalone model and focused receipt. Hosted CI and merge were pending when drafted.

## §2 What worked

Independent review supplied a concrete `INT32_MAX` counterexample and distinguished repository-wide cross-runtime CI from evidence actually observed by the focused JUnit receipt. The repaired gate now checks seven Quint witnesses, exact and sampled interpreter replay, and eight observed-red controls. SDD remains ship-ready while truthfully deferring compatibility evidence that is outside this receipt.

## §3 What did not

The previously green focused qualification sampled only ordinary traces, exposed no exact replay command, treated a deliberately unequal file as a freshness rejection, and attributed frozen-corpus/Fable results to a JUnit report that did not execute them. As a result, the first exact-head review rejected the evidence despite hosted CI being green.

## §4 Findings

#### §4.1 A passing focused receipt did not cover its signed boundary, exact-replay, freshness, and evidence-attribution claims

- **Kind:** quality-gap
- **Impact:** The focused receipt could pass while one semantic mismatch and three evidence gaps remained.
- **Expected:** The focused receipt preserves production signed-int rounding, executes both exact and sampled replay, observes generation/identity/contract rejection through the real checks, and claims only results present in its committed report.
- **Observed:** The earlier head used unbounded Quint addition before damage rounding, ran sampled replay only, tested stale output with bare inequality, and marked frozen-corpus/Fable compatibility observed. Commit `60eff02c` adds int32 wrapping plus a boundary witness, a committed 9-state exact trace, real projection/identity/contract controls, and changes EV007 to an explicit deferral.
- **Evidence:** command:test "$(git show 3fe6f279e9cdb12b3e7ca3514920e98061bb9fa5:scripts/qualify-quint-q4-sir-combat.sh | grep -c -- '--quint-q4-exact')" -eq 0 && test "$(git show 3fe6f279e9cdb12b3e7ca3514920e98061bb9fa5:docs/rules/sir-combat.md | grep -c 'wrapInt32')" -eq 0; command:git show 3fe6f279e9cdb12b3e7ca3514920e98061bb9fa5:scripts/qualify-quint-q4-sir-combat.sh | grep -Fq 'if cmp -s' && git show 3fe6f279e9cdb12b3e7ca3514920e98061bb9fa5:scripts/qualify-quint-q4-sir-combat.sh | grep -Fq 'stale-generated.qnt'; command:git show 3fe6f279e9cdb12b3e7ca3514920e98061bb9fa5:work/352-quint-q4-sir-adoption/evidence.yml | grep -A30 -F '  - id: EV007' | grep -Fq 'kind: verification' && git show 3fe6f279e9cdb12b3e7ca3514920e98061bb9fa5:work/352-quint-q4-sir-adoption/evidence.yml | grep -A30 -F '  - id: EV007' | grep -Fq 'result: pass'; command:gh run view 33068728390 --json conclusion,headSha; file:docs/rules/sir-combat.md; file:scripts/qualify-quint-q4-sir-combat.sh; file:tests/fixtures/rules-corpus/quint-q4/trace_0.itf.json; file:work/352-quint-q4-sir-adoption/evidence.yml; file:readiness/352-quint-q4-sir-adoption/ship-verdict.json
- **Version:** Quint 0.32.0 and FS.GG.SDD 1.0.1 at commit 60eff02cc49fdbf7e0fcd6d1e4fea8ad9664726d
- **Owner:** S.I.R. Quint qualification and evidence binding
- **Recurrence:** seen again after feedback/2026-08-12-sir-item-194-executable-rules-corpus.md §4.1 for receipt over-attribution; new for signed-width rounding, exact replay, and real freshness/identity controls
- **Avoidable cost:** one independent-review repair cycle; elapsed developer time not measured
- **Disposition:** skill fix

## §5 Did not exercise

The canonical manifest-v2 consumer profile, generated F#/Fable bindings, authenticated migration rollback, package-only adoption, and focused receipt-bound frozen-corpus/Fable compatibility remain unexercised. Five evidence obligations defer these surfaces, principally until FS-GG/FS.GG.SDD#932 publishes the coherent producer set.

## §6 Doc-versus-behavior contradictions

The earlier receipt contradicted its behavior by describing frozen-corpus and Fable compatibility as observed even though its JUnit cases did not execute those checks. The corrected receipt labels both deferred.

## §7 Workarounds still in the tree

The committed standalone receipt and native replay adapter remain temporary, explicitly noncanonical qualification surfaces until the producer profile is available.

## §8 Friction and avoidable cost

The semantic boundary and evidence-attribution problems survived both local qualification and hosted CI. The exact-head review added one repair cycle; all four gaps were reproducible without product-runtime changes.

## §9 Skill value and gaps

`quint-lang` guided the explicit signed wrapping and executable witness. The SDD evidence skill prevented laundering hosted cross-runtime success into a focused receipt and accepted EV007 as a visible deferral. The feedback skill preserved the review gap separately from the already-finalized first report. A reusable Quint/F# signed-arithmetic boundary checklist would reduce recurrence.

## §10 Outcome markers

The repaired focused gate passes seven witnesses, seven invariants over 64 samples × 8 steps, eight observed-red mutations, one exact trace with nine states, and 16 sampled traces with 144 states. SDD is ship-ready with seven observed passes, five deferrals, and zero synthetic, missing, stale, or invalid evidence.

## §11 Falsifiable improvements

- For §4.1, the Quint qualification guidance should require at least one signed-width boundary witness whenever production uses fixed-width arithmetic, and its receipt schema should reject compatibility claims without named JUnit cases or a separately bound observed report. Acceptance is a clean-room worker producing a Q4 receipt that includes exact replay, a signed-overflow witness, real freshness/identity/contract rejection, and no unbound compatibility claim before review.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing product; no scaffold activity. |
| onboarding-guidance | not-exercised | No new onboarding path. |
| skills | exercised | Quint, SDD evidence, and feedback guidance shaped the corrections. |
| sdd-authoring | exercised | EV007 was reclassified and all generated readiness views were resealed. |
| implementation-apis | exercised | Exact and sampled traces replayed through the production CombatRules interpreter. |
| dependencies-build | partial | Native focused build passed; focused cross-runtime compatibility stayed deferred. |
| testing | exercised | Seven witnesses, eight mutations, exact replay, and sampled replay passed. |
| evidence | exercised | Seven observed obligations and five explicit deferrals reached ship-ready. |
| runtime-playtest | not-exercised | No interactive journey was in scope. |
| performance | not-exercised | No performance claim was made. |
| documentation | exercised | The literate Quint authority gained the signed-overflow witness. |
| packaging-upgrade | not-exercised | FS-GG/FS.GG.SDD#932 remains unpublished. |
| worker-git-pr | exercised | Independent exact-head review rejected the old head and drove the repair. |
