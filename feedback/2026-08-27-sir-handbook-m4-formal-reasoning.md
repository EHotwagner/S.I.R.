---
feedbackSchema: 2
date: 2026-08-27
workspace: S.I.R-roadmap-m4
cycle: roadmap-sir-combat-quint-handbook-m4-formal-reasoning
lane: sdd
toolVersion: 1.4.0
commit: dc82658b9b7bf57df705cd994074cc46e4545e78
---

# Development feedback — combat Quint handbook M4 formal reasoning

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring, implementation-test-evidence, verify-ship-pr
- **material events:** 4
- **zero-event reason:** n/a

This cycle covers issue #365 on isolated branch `item/365-handbook-m4` from base `4a92687` through the commit named above. Four immutable checkpoint events are recorded in `feedback/checkpoints/roadmap-sir-combat-quint-handbook-m4-formal-reasoning.jsonl`. Evidence covers reuse of the prior receipt layout, lifecycle-authoring documentation friction, the six-pair mutation loop, and the optional refresh failure. Independent exact-head acceptance, hosted PR CI, merge, issue/project completion, and protected post-merge CI were pending at draft time.

The existing product provenance and package pins did not change. Repository tools continue to pin Fable 5.13.0, fsdocs-tool 22.1.0, and `FS.GG.SDD.Cli` 1.0.1; lifecycle commands resolved `fsgg-sdd` 1.4.0 and the Q4 qualification resolved Quint 0.32.0. The critic identified 1.5.0 as the latest available SDD release, but this cycle did not install or exercise it. The focused audit executed the extracted authority with the model's default `init` and `step`; the handbook's illustrative `quint verify` command was reviewed against the Quint CLI contract but Apalache was not executed in this documentation milestone.

## §2 What worked

The existing authority-extraction pattern scaled cleanly from representative and complete-rule checks to six isolated formal-reasoning mutations. A single focused command now binds handbook teaching claims to named detectors, state-referencing properties, and reachable major actions without committing mutated models. Separating the focused M4 receipt from the full Q4/runtime receipt also keeps model-education evidence distinct from production correspondence.

## §3 What did not

One rendered-text assertion was initially brittle because source whitespace became an HTML line break; the content was present, the marker was narrowed to a stable phrase, and the entire qualification was rerun green. Several SDD stage skills require consumer-local worked examples that this repository does not contain; critique found this is an established duplicate. The refresh checkpoint records a refusal on authored-skill drift followed by cleanup, but no command transcript, before/after status, or exact seven-path inventory was retained, so this report does not promote the incident to an actionable finding. The mandatory invalidation comparison remains red on the same seventeen historical audit-exception findings at both `origin/main..HEAD` and `origin/main..origin/main`; it is not an M4-specific regression.

## §4 Findings

#### §4.1 Authority-derived ephemeral mutations provide precise observed-red/restored-green teaching evidence

- **Kind:** positive-pattern
- **Impact:** Maintainers can prove that each handbook mutation is detected for the intended reason without editing the canonical literate model or retaining defective fixtures.
- **Expected:** Threshold, bounds, suppression, cover, collateral, and catalogue-integrity defects each fail through a named detector, the three major-action witnesses bind identity and state delta, every claimed property field occurs inside its named predicate, and the unchanged extraction passes through the same detector.
- **Observed:** The focused receipt reports 142 passing checks: six executable semantic mutation pairs, six action identity/delta mutation pairs, fifteen predicate-local binding mutation pairs, three reachable major-action witnesses, and seven authoritative properties.
- **Evidence:** file:work/365-handbook-m4/audit-formal-reasoning.mjs; file:readiness/365-handbook-m4/formal-reasoning.junit.xml; command:node work/365-handbook-m4/audit-formal-reasoning.mjs --require-rendered
- **Version:** S.I.R. commit `dc82658`; Quint 0.32.0; authority digest `f121c201…`.
- **Owner:** EHotwagner/S.I.R. handbook qualification
- **Recurrence:** extends the authority-derived detector patterns in the M2 feedback report §4.1 and M3 feedback report §4.2.
- **Avoidable cost:** none
- **Disposition:** accepted

#### §4.2 SDD stage guidance still points at worked examples absent from the consumer repository

- **Kind:** documentation
- **Impact:** A roadmap worker following the stage skills cannot read the required example artifacts at their documented paths and must infer format from an earlier work item.
- **Expected:** Every required referenced example is packaged with the skill or addressed through a stable skill-relative location.
- **Observed:** The documented consumer-relative `docs/examples/lifecycle-artifacts/spec.md` path is absent; M3's committed lifecycle corpus supplied the fallback grammar.
- **Evidence:** command:test ! -e docs/examples/lifecycle-artifacts/spec.md; file:feedback/checkpoints/roadmap-sir-combat-quint-handbook-m4-formal-reasoning.jsonl; issue:FS-GG/FS.GG.SDD#539
- **Version:** FS.GG.SDD skill guidance and `fsgg-sdd` 1.4.0 at S.I.R. commit `dc82658`.
- **Owner:** FS-GG.SDD lifecycle skill packaging/documentation
- **Recurrence:** seen again in `feedback/2026-08-13-sir-item-182-awareness-reaction-windows.md` §4.6, `feedback/item-185-in-application-docs.md` §4.1, and `feedback/2026-08-15-SIR-186.md` §4.1; existing upstream FS-GG/FS.GG.SDD#539 is closed.
- **Avoidable cost:** one failed lookup and a fallback comparison against the M3 lifecycle corpus.
- **Disposition:** existing issue

## §5 Did not exercise

Apalache-backed `quint verify`, M5's complete runtime correspondence map, M6's complete definition/index enforcement, M6V's authoritative SVG diagrams and animation/shader/accessibility/render/performance qualification, M7 publication review, browser gameplay, runtime playtest, package publication, and upgrades were outside M4. The full Q4/runtime suite was run only as a regression boundary. PR, merge, issue/project completion, and post-merge protected CI remained pending at draft time.

## §6 Doc-versus-behavior contradictions

The SDD skills' required worked-example path contradicts the consumer checkout layout described in §4.2. No combat semantic contradiction was found. The handbook's low-sample simulation is explicitly labelled learning/search evidence, while bounded verification is explicitly scoped to represented paths and depth; neither is described as unbounded proof or runtime equivalence.

## §7 Workarounds still in the tree

The qualification continues the inherited explicit SDK-resolver environment cleanup around strict documentation rendering. All semantic, action-witness, and predicate-binding mutants exist only in temporary directories. No duplicate Quint authority, mutated fixture, generated site, or refresh-created unrelated skill remains in the tree. The absence of retained refresh diagnostics prevents a stronger claim about why the transient files appeared.

## §8 Friction and avoidable cost

The rendered-content marker caused one full qualification retry. Missing skill examples caused one failed lookup and reuse of M3 artifacts. The optional refresh caused one refusal plus targeted cleanup of seven untracked paths. Historical feedback invalidation required baseline comparison and attribution. No pnext performance gate was run because this is a documentation/formal-model milestone and the host explicitly excluded it.

## §9 Skill value and gaps

`work-roadmap` governed the milestone ledger, four checkpoint phases, scoped receipts, independent review, and delivery boundary. The SDD lifecycle and stage skills produced charter-through-ship artifacts with converged analysis/evidence/verify/ship state. `fs-gg-feedback-report` required immutable checkpoints, cold critique, schema-v2 reporting, audit binding, and exact feedback-state validation. `quint-lang` validated sampled-versus-bounded language, nondeterministic trace interpretation, counterexample handling, and CLI examples. The established missing-example gap persists; the refresh observation lacks enough durable evidence for actionable attribution. Rule-authoring/model-building, visual, playtest, performance, package, and cross-repository skills were outside scope.

## §10 Outcome markers

- Final focused qualification: 142/142 checks passed, including six semantic, six action identity/delta, and fifteen predicate-binding red/restored-green pairs, three action witnesses, and seven properties.
- First rendered state: strict fsdocs rendered the M4 chapters; one brittle whitespace assertion was repaired and the complete aggregate reran green.
- Full semantic regression: the authority digest remained `f121c201…`; seven broader witnesses, eight broader mutations, 64 sampled model traces, and 16 runtime replay traces passed under Quint 0.32.0.
- First green lifecycle verification: 20/20 obligations were observed and non-synthetic; analyze reported 53 ready relationships and zero blockers.
- Ship readiness: `shipReady`; two-pass lifecycle convergence passed.
- Delivery: exact-head review, PR/CI, merge, project Done, and post-merge protected CI were pending at report draft time.

## §11 Falsifiable improvements

- Preserve §4.1 by retaining one single-defect temporary fixture per named mutation and requiring red then unchanged-green through the same detector. Acceptance: the focused command reports six semantic, six action identity/delta, and fifteen predicate-binding mutation pairs, three reachable major actions, seven property bindings, and zero failures without leaving a mutated file.
- No new action for duplicate §4.2; reopen or supersede FS-GG/FS.GG.SDD#539 only with a current producer-side reproduction and release scope.
- Maintain honest claim language. Acceptance: sampled commands state sample count, step bound, and seed; bounded verification says exhaustive only within the represented finite bound; neither claims production equivalence.
- No new M4 action is proposed for the duplicate historical invalidation baseline; repair its established audit exceptions before treating a product-head comparison as milestone-specific.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing S.I.R. product; no scaffold generated or parameter changed. |
| onboarding-guidance | partial | AGENTS, roadmap, issue, and the prior receipt pattern were applied; no new-project onboarding occurred. |
| skills | exercised | Work-roadmap, SDD stages, feedback-report, and Quint language guidance shaped and checked the cycle; one duplicate skill gap is a finding and one refresh recollection remains confidence-limited. |
| sdd-authoring | exercised | Charter through ship completed with eight requirements, three decisions, twenty tasks, and converged views. |
| implementation-apis | not-exercised | No runtime or public API changed. |
| dependencies-build | exercised | Locked restore, Release build, strict fsdocs, and pinned Quint completed. |
| testing | exercised | Link audit, 142-check focused audit, 27 mutation pairs, three witnesses, full Q4/runtime regression, and ledger audit passed. |
| evidence | exercised | Twenty obligations are observed and non-synthetic; focused, aggregate, lifecycle, verify, and ship receipts are committed. |
| runtime-playtest | not-exercised | Runtime replay remained a regression suite; no user-facing play journey was run. |
| performance | not-exercised | Reserved for M6V; the explicitly excluded pnext performance gate was not run. |
| documentation | exercised | M4 chapters teach claim kinds, trace reading, counterexample minimization, mutations, reachability, state binding, and bounded claim limits. |
| packaging-upgrade | not-exercised | No package or lock change. |
| worker-git-pr | partial | Isolated branch and implementation commit exist; PR, hosted CI, merge, board Done, and post-merge checks were pending at draft time. |
