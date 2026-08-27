---
feedbackSchema: 2
date: 2026-08-27
workspace: S.I.R-roadmap-m3
cycle: roadmap-sir-combat-quint-handbook-m3-complete-rules
lane: sdd
toolVersion: 1.4.0
commit: 12f2a99010cbfb95347e5f8be822dec2c9f2896a
---

# Development feedback — combat Quint handbook M3 complete rules

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring, implementation-test-evidence, verify-ship-pr
- **material events:** 6
- **zero-event reason:** n/a

This cycle covers issue #363 and PR #364 from base `b109fd2` through the full commit named above. Six checkpoint events are recorded in `feedback/checkpoints/roadmap-sir-combat-quint-handbook-m3-complete-rules.jsonl`. Evidence covers worker onboarding, lifecycle decisions, focused and full qualification, verify/ship, PR creation, invalidation inspection, and the first exact-head review repair. The onboarding checkpoint preserves the claim friction but is not a first-build receipt, and the potentially mutating claim command was not repeated during cold critique. Replacement exact-head acceptance, final hosted CI, merge, and board completion were pending at draft time.

The existing product came from the `fable-game` provider recorded by `config/scaffold-provenance.json` and `.fsgg/scaffold-provenance.json`; no scaffold parameter changed in M3. Repository tools pin Fable 5.13.0, fsdocs-tool 22.1.0, and `FS.GG.SDD.Cli` 1.0.1 in `.config/dotnet-tools.json`. Lifecycle commands resolved global `fsgg-sdd` 1.4.0. Quint remained pinned at 0.32.0 by the existing Q4 gate.

## §2 What worked

Explicit action-granularity decisions kept the sixteen-rule teaching model faithful: pure helpers and observations expose focused effects while aggregate attack resolution remains one atomic action. Exact excerpt extraction and a dedicated audit made all sixteen catalogue, reference, traceability, and definition entries mechanically accountable. Independent review caught three documentation/evidence defects before merge, and the user-added visual requirement fit cleanly as pending M6V without expanding M3.

## §3 What did not

The issue lacked the structured delivery-route receipt required by the coordination claim command, although the host assignment explicitly excluded the pnext gate. Initial exact-head review found a nonexistent recovery subject, stale forward-looking M3 wording, and ambiguous aggregate-receipt language. The required feedback invalidation command also remains red because of seventeen historical audit-exception findings that reproduce independently of M3.

## §4 Findings

#### §4.1 Clarification decisions made shared-action granularity teachable without inventing runtime state

- **Kind:** positive-pattern
- **Impact:** Readers can inspect wound, suppression, penetration, collateral, and cover effects separately without being told those explanations are runtime-visible transitions.
- **Expected:** Every stable rule is visible through an existing value, helper, action, observation, run, or property, while aggregate resolution remains atomic.
- **Observed:** DEC-001 through DEC-003 resolved aggregate atomicity, penetration visibility, and the M3/M4 exercise boundary; analyze reported 50 ready relationships and zero blockers.
- **Evidence:** file:work/363-handbook-m3/clarifications.md; file:readiness/363-handbook-m3/analysis.json
- **Version:** fsgg-sdd 1.4.0 at S.I.R. commit `0a22a77`.
- **Owner:** FS-GG/FS.GG.SDD authoring workflow and EHotwagner/S.I.R. handbook model
- **Recurrence:** extends the clarification pattern in `feedback/2026-08-27-sir-handbook-m2-representative-attack.md` §4.2 to shared actions and non-action rule subjects.
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 Exact extraction plus focused auditing scaled complete coverage to all sixteen stable rules

- **Kind:** positive-pattern
- **Impact:** Handbook prose cannot silently omit a stable rule, leave a placeholder traceability row, or drift from the executable literate authority.
- **Expected:** Sixteen catalogue rows, complete references, stable traceability rows, and indexed definitions are checked against `docs/rules/sir-combat.md`, with executable examples at honest granularity.
- **Observed:** The focused receipt reports 16/16 rules, sixteen exact excerpts, 341 checks, six named Quint runs, and two structural negative controls. The full Q4 receipt separately reports seven witnesses and eight mutations.
- **Evidence:** file:work/363-handbook-m3/audit-complete-rules.mjs; file:readiness/363-handbook-m3/handbook-m3-rules.junit.xml; file:readiness/363-handbook-m3/sir-combat-q4.junit.xml; file:docs/rules/sir-combat.md
- **Version:** S.I.R. commit `0a22a77`; Quint 0.32.0.
- **Owner:** EHotwagner/S.I.R. handbook qualification
- **Recurrence:** extends the canonical Q4 teaching-source pattern in M2 §4.1 and the focused-receipt pattern from `feedback/2026-08-16-sir-item-215-single-pass-qualification.md`.
- **Avoidable cost:** none established by the cited durable evidence.
- **Disposition:** accepted

#### §4.3 Scoped lifecycle convergence and an early PR made the delivery boundary inspectable

- **Kind:** positive-pattern
- **Impact:** Evidence, verify, and ship claims are reviewable before the final feedback-only head and exact-head acceptance.
- **Expected:** Component receipts compose only after their owning commands pass, and lifecycle views converge without stale or synthetic evidence before review.
- **Observed:** The six-case aggregate covers docs, links, focused model, full Q4/runtime, roadmap ledger, and SDD analysis. Seventeen obligations are observed; the two-pass lifecycle receipt reports zero blocking, stale, or synthetic evidence and `shipReady`. PR #364 was opened before final report binding.
- **Evidence:** file:readiness/363-handbook-m3/qualification.junit.xml; file:readiness/363-handbook-m3/lifecycle.junit.xml; file:readiness/363-handbook-m3/ship.json; command:gh pr view 364 --repo EHotwagner/S.I.R. --json headRefOid,state,url
- **Version:** S.I.R. commit `0a22a77`; fsgg-sdd 1.4.0.
- **Owner:** EHotwagner/S.I.R. qualification and pull-request workflow
- **Recurrence:** preserves the corrected aggregate-receipt pattern from M2 §4.4.
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.4 Exact-head review caught three documentation/evidence defects before merge

- **Kind:** quality-gap
- **Impact:** A learner could read completed cover/recovery walkthroughs as still pending, while maintainers saw one nonexistent model subject and a receipt policy that appeared to contradict the actual aggregate evidence binding.
- **Expected:** The plan names authoritative Quint subjects exactly, completed M3 prose is current, and aggregate evidence wording matches its six scoped component cases.
- **Observed:** Review found `recoverSuppression` instead of `recoveredSuppression`/`resolveRecovery`, a sentence saying completed walkthroughs remained M3 work, and “never cited as narrower proof” beside aggregate-bound evidence. Commit `12f2a99` repaired all three and resealed qualification and lifecycle evidence.
- **Evidence:** file:feedback/checkpoints/roadmap-sir-combat-quint-handbook-m3-complete-rules.jsonl; file:work/363-handbook-m3/plan.md; file:docs/sir-combat-quint-handbook.md; command:git diff 1a060c8..12f2a99 -- work/363-handbook-m3/plan.md docs/sir-combat-quint-handbook.md
- **Version:** S.I.R. repaired commit `12f2a99`; authoritative Quint model digest `f121c201…`.
- **Owner:** EHotwagner/S.I.R. handbook and SDD plan
- **Recurrence:** new in M3.
- **Avoidable cost:** one focused documentation repair, two qualification attempts after lifecycle refresh, and one replacement exact-head review.
- **Disposition:** doc fix

#### §4.5 Feedback invalidation is still red on historical audit exceptions

- **Kind:** defect
- **Impact:** The mandatory check cannot establish an M3-specific invalidation verdict from the repository baseline.
- **Expected:** An unchanged default-branch comparison passes while touched evidence with stale bindings fails.
- **Observed:** The M3 invocation exits 1 with seventeen `overbroad or mismatched exception` errors from historical audits, matching the established baseline failure class.
- **Evidence:** file:feedback/checkpoints/roadmap-sir-combat-quint-handbook-m3-complete-rules.jsonl; command:dotnet fsi .agents/skills/fs-gg-feedback-report/scripts/feedback-tool.fsx -- check-invalidation --base origin/main --head HEAD --root .
- **Version:** feedback tool bundled at S.I.R. commit `0a22a77`.
- **Owner:** FS.GG feedback invalidation semantics and S.I.R. historical audit maintenance
- **Recurrence:** duplicate of `feedback/2026-08-27-sir-handbook-m2-representative-attack.md` §4.5 and earlier item 255 evidence.
- **Avoidable cost:** one failed invalidation run and explicit baseline attribution.
- **Disposition:** accepted

## §5 Did not exercise

M4 counterexample/mutation teaching, M5 broad runtime correspondence, M6 exhaustive link enforcement, pending M6V visual mechanics/theory diagrams, M7 publication review, browser gameplay, visual rendering, performance qualification, and package publication/upgrades were outside M3. The newly requested M6V is deliberately pending and M7 now depends on it. Merge and board completion remained pending at draft time.

## §6 Doc-versus-behavior contradictions

No combat semantic contradiction was found. The clarification artifact retains generated status `needsAnswers` even though all three questions are answered, it says no ambiguity remains, and analysis is implementation-ready; downstream readiness treats the answers as resolved. The other process contradiction was the host's valid bounded assignment versus the checkpoint-recorded coordination refusal without a route receipt. Neither changed source scope or rule semantics.

## §7 Workarounds still in the tree

The bounded qualification clears inherited SDK resolver variables around strict docs rendering, continuing the explicit workaround documented by M2. No generated site output or duplicate combat model is committed. The missing claim receipt was not fabricated.

## §8 Friction and avoidable cost

One coordination claim failed before write. One proposed non-contiguous excerpt was repaired into exact source excerpts. Independent review required a three-part documentation correction; the first requalification refreshed analysis and stopped before aggregate emission, and the converged rerun passed. The invalidation check required one baseline attribution. The user-added visual milestone required one scoped roadmap and ledger-audit update, without an M3 implementation reroute.

## §9 Skill value and gaps

`work-roadmap` governed milestone history, scoped receipts, feedback phases, and merge-boundary evidence. The SDD lifecycle and stage references produced the charter-through-ship package. `fs-gg-feedback-report` required five durable checkpoints, cold critique, schema-v2 report/audit validation, and exact feedback-state validation. The existing authoritative Quint model and Q4 gate were consumed rather than changed, so Quint modeling and S.I.R. rule-authoring skills were not invoked. Gameplay, visual, playtest, performance, package, and cross-repo skills were outside M3; the newly added M6V explicitly reserves the visual/performance surfaces for later work. The principal gap is orchestration: a dispatched roadmap worker needs a compatible current delivery-route receipt.

## §10 Outcome markers

- First meaningful test: the focused audit passed 16/16 rules, sixteen exact excerpts, 341 checks, and six named Quint runs.
- First rendered state: strict fsdocs rendered the complete-rule handbook.
- Full semantic gate: seven witnesses and eight mutations passed under Quint 0.32.0; the focused receipt separately passed six named runs.
- First green verification: 17/17 evidence obligations observed, with zero missing, stale, synthetic, or invalid evidence.
- Ship readiness: `shipReady`; two-pass lifecycle convergence passed.
- PR: #364 opened against `main`; first exact-head review supplied the repaired §4.4 findings, while replacement acceptance, replacement hosted CI, merge, and board completion remain pending.

## §11 Falsifiable improvements

- Preserve §4.1 by requiring explicit decisions whenever multiple rule IDs share one action or a rule is visible only through helpers/observations. Acceptance: analyze is blocker-free and the focused audit maps all sixteen rules without adding state variables or actions.
- Preserve §4.2 by retaining exact authority extraction, complete 16-row audits, named runs, and structural negative controls. Acceptance: deleting one reference row, making one trace row pending, or changing an exact excerpt makes the focused command fail.
- Preserve §4.3 by emitting the aggregate only after all six scoped receipts pass and requiring two-pass lifecycle convergence. Acceptance: verify reports 17 observed obligations with zero stale/synthetic evidence and ship reports `shipReady`.
- Preserve §4.4 by checking every named Quint subject and temporal milestone statement at exact head. Acceptance: no plan identifier is absent from authority, completed walkthrough prose has no pending-M3 claim, and aggregate wording names its six component cases.
- No new action for duplicate §4.5; resolve the established historical-audit baseline before treating product-head invalidation errors as milestone-specific.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing S.I.R. product; no scaffold generated. |
| onboarding-guidance | partial | AGENTS, roadmap, and issue assignment were applied; missing delivery route was recorded, but the checkpoint is not a first-build receipt. |
| skills | partial | Work-roadmap, SDD lifecycle/stages, and feedback-report guidance shaped the cycle; the scaffold manifest was not independently re-inventoried. |
| sdd-authoring | exercised | Charter through ship completed with eight requirements, three decisions, and seventeen tasks. |
| implementation-apis | not-exercised | No runtime or public API changed. |
| dependencies-build | exercised | Locked restore, Release build, strict fsdocs, and pinned Quint completed. |
| testing | exercised | Link audit, 341-check focused audit, six runs, and full Q4/runtime qualification passed. |
| evidence | exercised | Six scoped receipts compose into seventeen observed obligations; invalidation baseline defect is disclosed. |
| runtime-playtest | not-exercised | Headless correspondence stayed under testing; no user-facing play journey was run. |
| performance | not-exercised | Reserved for pending M6V; no performance claim changed. |
| documentation | exercised | Complete catalogue, walkthroughs, references, traceability, definitions, and exercises rendered; exact-head review corrected three documentation/evidence defects. |
| packaging-upgrade | not-exercised | No package or lock change. |
| worker-git-pr | partial | Isolated branch and PR #364 exist; exact-head review, CI, merge, and board completion were pending at draft time. |
