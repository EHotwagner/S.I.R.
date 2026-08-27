---
feedbackSchema: 2
date: 2026-08-27
workspace: S.I.R-roadmap-m5
cycle: roadmap-sir-combat-quint-handbook-m5-runtime-correspondence
lane: sdd
toolVersion: 1.4.0
commit: 95b7545dd372c89fe24e1d5b9601d05de84274fc
---

# Development feedback — combat Quint handbook M5 runtime correspondence

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring, implementation-test-evidence, verify-ship-pr
- **material events:** 4
- **zero-event reason:** n/a

This cycle covers issue #373 on isolated branch `item/373-handbook-m5` from base `858368e` through the commit above. Four immutable checkpoints in `feedback/checkpoints/roadmap-sir-combat-quint-handbook-m5-runtime-correspondence.jsonl` record the inherited qualifier boundary, lifecycle translation, composed runtime evidence, and the observed-receipt import correction. Exact-head review, hosted PR CI, merge, issue/project completion, and post-merge Pages were pending at draft time.

The existing scaffold is `FS.GG.Workspace.Template` 0.8.0 with provider `fable-game`, product `S.I.R.`, and root namespace `SIR`; no scaffold parameter changed. The repository pins Fable 5.13.0, fsdocs-tool 22.1.0, `FS.GG.SDD.Cli` 1.0.1, and Quint 0.32.0. Bare lifecycle commands resolved a separately installed global `fsgg-sdd` 1.4.0; lifecycle receipts were finalized with the pinned `dotnet fsgg-sdd` 1.0.1, so those versions are not conflated. The runtime evidence uses production `CombatRules` entry points through the shared Q4 replay adapter. Exact and sampled replay establish bounded correspondence only; they do not establish general implementation equivalence or replay supercover geometry.

## §2 What worked

The existing Q4 real-interpreter harness accepted both the committed exact trace and deterministic sampled traces without creating a second combat interpreter. The handbook-specific audit enforces the controlled status vocabulary, five structural red/restored controls, and honest claim limits; the composed Q4 qualifier separately executes three runtime inversions and restores exact/sample replay afterward. The roadmap exits translated directly into eight FR/AC pairs and twenty-one observed obligations.

## §3 What did not

The prior M4 aggregate reaches an expected milestone-local ledger failure on a descendant branch, so it is not a clean M5 baseline command. One rendered-content marker needed narrowing after fsdocs inserted markup. The lifecycle procedure also used the maintenance-only `evidence --sync-observed-run` form before bootstrapping the new receipt; `verify` correctly blocked until the JUnit was imported with the documented `evidence --from-test-report` bootstrap. The checkpoint preserves the pre-correction observation, while final receipts prove only the corrected state.

## §4 Findings

#### §4.1 Controlled correspondence plus first-divergence controls provide bounded, falsifiable runtime evidence

- **Kind:** positive-pattern
- **Impact:** Maintainers can see exactly which model/runtime subjects are compared, which are aggregate or external contracts, and where comparison is missing, while detector inversions prove that the adapter reports the earliest actionable mismatch.
- **Expected:** All sixteen stable rules have a controlled status; supercover geometry is explicitly missing; exact and sampled traces traverse real production entry points; three independent mutations report transition, action, pointer, expected, actual, adapter, and implementation before untouched replay returns green.
- **Observed:** The focused audit passed 47 checks, five structural red/restored-green controls, and sixteen mappings. The real Q4 qualifier accepted one exact trace/nine states and seed-352 sampling with sixteen traces/144 states; all three runtime divergence controls failed at their intended seam, after which untouched exact and sampled replay were rerun and restored green.
- **Evidence:** file:docs/sir-combat-quint-handbook.md; file:work/373-handbook-m5/audit-runtime-correspondence.mjs; file:readiness/373-handbook-m5/runtime-correspondence.junit.xml; file:readiness/373-handbook-m5/sir-combat-q4.junit.xml; command:bash work/373-handbook-m5/qualify-handbook-m5.sh
- **Version:** S.I.R. commit `95b7545`; Quint 0.32.0; authority digest `f121c201a6f77d0cfc4c86fe72455e8d821b3d941bf012ab0e3482db103e43e7`.
- **Owner:** EHotwagner/S.I.R. handbook qualification
- **Recurrence:** seen again `feedback/2026-08-27-sir-handbook-m2-representative-attack.md` §4.1 and `feedback/2026-08-27-sir-handbook-m4-formal-reasoning.md` §4.1, now extended to complete runtime correspondence.
- **Avoidable cost:** one rendered-marker correction.
- **Disposition:** accepted

#### §4.2 A milestone-local ledger makes the prior aggregate unsuitable as a descendant baseline

- **Kind:** orchestration
- **Impact:** A later roadmap worker cannot use the preceding milestone's complete aggregate as a clean semantic baseline even when every inherited semantic gate remains green.
- **Expected:** Descendant baseline qualification should be able to exercise inherited build/docs/model/runtime gates without failing solely because the predecessor is no longer the newly checked milestone.
- **Observed:** `qualify-handbook-m4.sh` passed inherited build, docs, links, formal mutations, and Q4/runtime checks before its final ledger assertion rejected the already-checked M4 state with `expected only M4 newly checked; got []`.
- **Evidence:** file:feedback/checkpoints/roadmap-sir-combat-quint-handbook-m5-runtime-correspondence.jsonl; file:work/365-handbook-m4/qualify-handbook-m4.sh; file:work/365-handbook-m4/audit-roadmap-ledger.mjs
- **Version:** S.I.R. report commit `95b7545`; inherited M4 qualification harness from base `858368e`.
- **Owner:** EHotwagner/S.I.R. handbook milestone qualification harness
- **Recurrence:** new
- **Avoidable cost:** one approximately two-minute baseline run reached the expected final ledger failure.
- **Disposition:** product fix

## §5 Did not exercise

General semantic equivalence, supercover geometry replay, M6 index/link enforcement, M6V SVG mechanics/theory diagrams and their animation/shader/accessibility/fallback/visual-regression/performance gates, M7 publication review, gameplay journeys, package publication, and dependency upgrades were outside M5. No pnext performance gate ran because this documentation/correspondence milestone has no typed performance intent.

## §6 Doc-versus-behavior contradictions

No combat semantic contradiction was found. The handbook explicitly distinguishes `exact`, `aggregate`, `external-contract`, `presentation-only`, and `missing`; exact/sample evidence is scoped to named fixtures and deterministic bounds. The sequencing error in §3 was procedural misuse of guidance that correctly distinguishes receipt bootstrap from maintenance, not a documentation contradiction.

## §7 Workarounds still in the tree

The qualifier retains the inherited SDK-resolver environment cleanup around strict documentation rendering. Mutated mapping, observable, and runtime results exist only in temporary directories. The explicit `--from-test-report` import remains in the lifecycle procedure because it is the command form proven to produce observed receipts.

## §8 Friction and avoidable cost

The inherited M4 baseline consumed approximately two minutes before its final, expected ledger failure. One rendered marker required a complete aggregate retry. Using receipt maintenance before bootstrap caused one blocked verify and corrective import. Adding a real post-mutation restored-green pass required lifecycle snapshot refresh. No production code, package, or generated ITF authority was duplicated.

## §9 Skill value and gaps

The committed scaffold provenance inventories `work-roadmap`; it enforced isolated milestone identity, ledger advancement, checkpoints, review, and delivery. `fs-gg-feedback-report` required immutable checkpoints, fresh-context critique, schema-v2 audit binding, and feedback-state validation. The invoked SDD skills were `fs-gg-sdd-lifecycle`, `fs-gg-sdd-charter`, `fs-gg-sdd-specify`, `fs-gg-sdd-clarify`, `fs-gg-sdd-checklist`, `fs-gg-sdd-plan`, `fs-gg-sdd-tasks`, `fs-gg-sdd-analyze`, `fs-gg-sdd-evidence`, `fs-gg-sdd-verify`, `fs-gg-sdd-ship`, and `fs-gg-sdd-refresh-agents`; generated guidance confirmed task skill visibility for `fsharp`, `implementation`, `automated-tests`, `readiness-evidence`, `schema-versioning`, and `deterministic-json`. `quint-lang` kept ITF and bounded-evidence language precise. Gameplay-rule authoring, model creation, playtest, visual, performance, packaging, and cross-repository skills were not invoked because M5 changes documentation/evidence around an existing rule/model/runtime. The procedure error was using documented receipt maintenance before documented bootstrap, not a missing capability.

## §10 Outcome markers

- Focused qualification: 47/47 checks, five red/restored controls, and sixteen rule mappings passed.
- Runtime replay: exact 1 trace/9 states; sampled seed 352, 16 traces/144 states, maximum 8 steps.
- Full Q4 boundary: seven witnesses, eight broader mutations, and 64 model simulations passed under Quint 0.32.0.
- Lifecycle: 54 analysis relationships ready; 21/21 evidence and test obligations observed, non-synthetic, and current; `shipReady`.
- Roadmap: only M5 newly checked; M6, M6V, and M7 remain pending.
- Time to first build: not measured and not reconstructable from retained evidence.
- Time to first meaningful focused test: not measured and not reconstructable.
- Time to first rendered handbook state: not measured and not reconstructable.
- Time to first green lifecycle verification: not measured and not reconstructable.
- Delivery: exact-head review, PR/CI, merge, board Done, and post-merge Pages were pending at report draft time.

## §11 Falsifiable improvements

- Preserve §4.1 by requiring every stable rule row to use the controlled vocabulary and by running all three first-divergence mutations followed by untouched exact/sample green. Acceptance: the focused receipt reports 47 checks, five structural restored controls, sixteen mappings, and no geometry overclaim; the Q4 receipt reports 20 cases including restored exact and restored sampled runtime correspondence.
- Separate §4.2's semantic regression command from milestone-ledger qualification in the S.I.R. handbook harness. Acceptance: an M5 descendant can run inherited M4 semantic/build/runtime gates successfully without satisfying M4's branch-local “newly checked” assertion, while the standalone M4 ledger command still rejects incorrect advancement.
- Preserve the documented evidence bootstrap/maintenance order in the local lifecycle procedure. Acceptance: new JUnit evidence uses `--from-test-report` before any `--sync-observed-run`, and verify reports zero self-attested obligations on the first pass.
- Maintain claim limits. Acceptance: exact evidence names its fixture/state count; sampled evidence names version, seed, trace/state counts, and step bound; supercover geometry remains `missing` until a real comparison lands.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing product; no scaffold or template parameter changed. |
| onboarding-guidance | partial | AGENTS, roadmap, issue, and prior milestone receipts were used; no new-project onboarding. |
| skills | exercised | Work-roadmap, SDD stages, feedback-report, and Quint/runtime guidance shaped the cycle. |
| sdd-authoring | exercised | Charter through ship completed with eight requirements, three decisions, twenty-one tasks, and current views. |
| implementation-apis | exercised | Existing `CombatRules` production entry points and the shared replay adapter were invoked and mutation-tested; no API changed. |
| dependencies-build | exercised | Release build, strict fsdocs, and pinned Quint completed. |
| testing | exercised | Link, 47-check focused, full Q4/runtime, analysis, and ledger gates passed. |
| evidence | exercised | Final `verify.json` records twenty-one observed, non-synthetic obligations; §3 preserves the confidence-limited bootstrap sequencing correction. |
| runtime-playtest | not-exercised | Headless runtime replay was evidence, not a user journey. |
| performance | not-exercised | Reserved for M6V; no typed M5 performance intent. |
| documentation | exercised | Chapters 38–43, traceability, glossary, roadmap, and safe rule-change guidance landed. |
| packaging-upgrade | not-exercised | No package or lock change. |
| worker-git-pr | partial | Isolated branch and evidence commit exist; hosted delivery was pending at draft time. |
