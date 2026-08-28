---
title: Combat in Quint handbook roadmap
category: Battlefield Systems
categoryindex: 4
index: 47
description: Milestone ledger and authority inventory for the S.I.R. Combat in Quint handbook.
date: 2026-08-27
status: active
document-type: roadmap
---

# Combat in Quint handbook roadmap

This is the S.I.R.-owned milestone ledger imported from
`EHotwagner/.github` commit `b72c96d41d84d1468422b5c616a16d5f3c501c2a`,
`docs/2026-08-27-sir-combat-quint-learning-handbook-design-and-roadmap.md`.
Milestone wording is preserved below; completion evidence is appended to the milestone that lands it.

The finished publication target is `docs/sir-combat-quint-handbook.md`. It is intentionally not created
until M1.

## Roadmap

The work proceeds from authority inventory to a runnable learning spine, then broadens into complete
reference coverage. This keeps the handbook useful and testable before every reference entry is written.

```text
M0 authority inventory
  -> M1 linked handbook skeleton
  -> M2 representative attack learning spine
  -> M3 complete combat-rule walkthroughs
  -> M4 formal reasoning and mutation laboratory
  -> M5 runtime correspondence and evidence
  -> M6 definition index and link enforcement
  -> M6V authoritative mechanics and theory diagrams
  -> M7 review, publication, and maintenance handoff
```

## Milestones

### - [x] M0 — Authority and vocabulary inventory

**Outcome:** a checked source map and controlled-vocabulary inventory.

Deliverables:

- inventory of the S.I.R. ADR, combat architecture, Q4 decisions, model, runtime, and evidence;
- inventory of sixteen rule IDs and their dependencies;
- inventory of Quint declarations and properties;
- initial stat, unit, combat-concept, and formal-method vocabulary;
- explicit list of scope exclusions and unresolved source disagreements.

Exit criteria:

- every planned handbook claim has an identified authority class;
- every model declaration has a planned index kind; and
- no unresolved disagreement changes the proposed state shape or action granularity.

Completion evidence (2026-08-27):

- [M0 source and authority inventory](#m0-source-and-authority-inventory) distinguishes current,
  candidate, generated, runtime, and evidence sources.
- [Sixteen-rule inventory](#m0-sixteen-rule-inventory) records exactly sixteen stable IDs, kinds, and
  direct dependencies.
- [Quint declaration and property inventory](#m0-quint-declaration-and-property-inventory) assigns an
  index kind to every top-level declaration in PR #355's candidate literate model.
- [Vocabulary inventory](#m0-controlled-vocabulary-inventory), [exclusions](#m0-scope-exclusions), and
  [disagreements](#m0-source-disagreements) close the remaining M0 deliverables. The disagreement audit
  finds no open item that changes the proposed `CombatState`/`AttackInput`/`Observation` state shape or
  the atomic aggregate/focused-action granularity.
- Lifecycle evidence: `work/356-handbook-m0/` and `readiness/356-handbook-m0/`; feedback cycle:
  `roadmap-sir-combat-quint-handbook-m0-authority-inventory`.

### - [x] M1 — Linked handbook skeleton

**Outcome:** the publication file exists with its complete hierarchy and navigation.

Deliverables:

- front matter and title;
- table of contents;
- three reading-path maps;
- stable anchors for all planned definitions;
- empty traceability matrix with all mandatory rows;
- initial alphabetical definition index;
- checked vocabulary manifest and link-audit prototype.

Exit criteria:

- every table-of-contents link resolves;
- every controlled term in the skeleton links; and
- the document builds in the S.I.R. documentation pipeline.

Completion evidence (2026-08-27):

- `docs/sir-combat-quint-handbook.md` fixes the complete fifty-chapter hierarchy, table of contents,
  three reading paths, 185 semantic definition anchors, seed alphabetical index, and every mandatory
  traceability row without adding M2 walkthrough substance.
- `docs/sir-combat-quint-vocabulary.json` and `work/359-handbook-m1/audit-handbook-links.mjs` provide a
  checked manifest and structurally aware positive/negative link audit; the focused audit and rendered
  S.I.R. docs build pass.
- Lifecycle evidence: `work/359-handbook-m1/` and `readiness/359-handbook-m1/`; feedback cycle:
  `roadmap-sir-combat-quint-handbook-m1-linked-skeleton`; delivery: PR #360.

### - [x] M2 — Representative attack learning spine

**Outcome:** a Quint beginner can follow one attack end to end.

Deliverables:

- attack pipeline overview;
- facts, Q4 arithmetic, trace, retention, expected damage, and rounding;
- `CombatState`, `AttackInput`, and `Observation` explanations;
- representative-damage action and run;
- prediction prompts, trace reading, and one negative mutation;
- runtime correspondence for the representative attack.

Exit criteria:

- `25 x 1.0 x 0.8 = 20` is explained at every modeling layer;
- all shown executable code runs under the pinned toolchain; and
- the learner can explain why the model uses raw scale-10,000 integers.

Completion evidence (2026-08-27):

- `docs/sir-combat-quint-handbook.md` teaches one representative attack through the domain
  pipeline, Q4 facts, pure helpers, records, guarded action, named run, two-state trace, and the
  predict-run-observe-explain loop. It derives `25 × 1.0 × 0.8 = 20` at raw scale 10,000 and
  distinguishes saturating helpers from the signed-int32 pre-division wrap boundary.
- `work/361-handbook-m2/qualify-handbook-m2.sh` owns dedicated strict-docs, structural-link,
  focused mutation/restoration, and full Q4/runtime receipts before emitting their aggregate;
  the negative mutation observes 18 and the untouched authority restores 20.
- Lifecycle evidence: `work/361-handbook-m2/` and `readiness/361-handbook-m2/`; feedback:
  `feedback/2026-08-27-sir-handbook-m2-representative-attack.md`; delivery: PR #362.
- Delivery boundary: this checked entry is the merge-candidate ledger update. Independent
  exact-head acceptance and green exact-head hosted CI are required before merge. It becomes
  landed completion history only when PR #362 merges; post-merge validation must confirm main,
  issue #361 closure, and project status Done.

### - [x] M3 — Complete combat-rule walkthroughs

**Outcome:** every stable combat rule is documented and executable at its appropriate granularity.

Deliverables:

- catalogue and dependency documentation;
- wound, incapacity, suppression, recovery, cover, collateral, penetration, and aggregate-resolution chapters;
- external line-of-sight contract chapter;
- a rule-reference entry and traceability row for every rule;
- exercises at beginner, intermediate, and advanced levels.

Exit criteria:

- sixteen of sixteen rules have complete reference coverage;
- every focused transition is visible through a pure helper, action, observation, or property; and
- no chapter invents a runtime-visible intermediate state.

Completion evidence (2026-08-27):

- `docs/sir-combat-quint-handbook.md` provides 16/16 catalogue, dependency, reference, and
  traceability coverage plus executable walkthroughs for wound/incapacity, suppression/recovery,
  current collision and cover/destruction, penetration, collateral, atomic aggregate resolution, and
  the external line-of-sight contract; beginner, intermediate, and advanced exercises preserve the M4
  mutation-laboratory boundary.
- `work/363-handbook-m3/qualify-handbook-m3.sh` owns dedicated strict-docs, structural-link,
  focused sixteen-rule authority, and full Q4/runtime receipts. The focused audit mechanically checks
  exact excerpts, all stable rule/dependency/reference/traceability subjects, structural negative
  controls, and six named Quint runs without inventing model state.
- Lifecycle evidence: `work/363-handbook-m3/` and `readiness/363-handbook-m3/`; feedback cycle:
  `roadmap-sir-combat-quint-handbook-m3-complete-rules`; report:
  `feedback/2026-08-27-sir-handbook-m3-complete-rules.md`.
- User-added roadmap requirement: pending M6V now owns mechanics/theory diagrams and their visual,
  accessibility, fallback, authority-derivation, regression, and performance qualification. This records
  sequencing only and does not expand M3 implementation.
- Delivery boundary: this checked entry is the merge-candidate ledger update. Independent exact-head acceptance
  and green exact-head hosted CI are required before merge. It becomes landed completion
  history only when the M3 PR merges; post-merge validation must confirm main, issue #363 closure, and
  project status Done.

### - [x] O1 — Route ordinary main-push CI by relevance

**Outcome:** handbook roadmap merges no longer repeat unrelated complete product qualification, while
the protected boundary remains exact-source and fail-closed.

Completion evidence (2026-08-27):

- Ordinary `main` pushes and pull requests share the canonical `sir.ci-route/v2` classifier, producer
  derivation, focused gate DAG, and deterministic join. Push routing uses exact `HEAD^..HEAD`; an empty
  inventory or missing parent refuses instead of inventing a path.
- Unknown, mixed, `.github`, router, protected-receipt, and CI-contract changes remain conservative
  cross-cutting routes. The stable `protected-verdict` validates `sir.protected-join/v2` focused receipts,
  while schedule and manual dispatch retain complete preflight/core clean-room qualification.
- A permissionless Pages selector verifies the exact triggering route and creates a deployment only when
  documentation was selected. The final mutated site is sealed separately from its pre-consumption docs
  producer, and `sir.qualified-site-handoff/v1` binds its route, documentation gate, final-site receipt,
  and archive. The focused protected join requires that handoff whenever documentation is selected; the
  deploy job verifies the same identity and cannot rebuild or deploy an unrelated successful merge.
  Its bounded 30-minute timeout covers GitHub Pages service latency without weakening any selector,
  receipt, archive, permission, or no-rebuild contract; issue #370 records the post-merge timeout repair.
- Focused positive and restored-negative evidence is owned by
  `work/366-main-ci-routing/qualify-main-ci-routing.sh`; lifecycle evidence is under
  `work/366-main-ci-routing/` and `readiness/366-main-ci-routing/`; feedback cycle:
  `roadmap-sir-combat-quint-handbook-ci-main-routing`; delivery issue: #366; post-merge handoff repair:
  #368; external Pages timeout repair: #370.
- Delivery boundary: this checked entry records the merge candidate. It becomes landed completion history
  only after independent exact-head review, green PR CI, merge, a first focused post-merge protected verdict,
  issue #366 closure, and project status Done. M4 and the later M6V visual milestone remain unchanged.

### - [x] M4 — Formal reasoning and mutation laboratory

**Outcome:** the handbook teaches how to learn from execution and failure.

Deliverables:

- examples versus witnesses versus invariants;
- nondeterministic trace interpretation;
- counterexample-reading workflow;
- mutation cases for thresholds, bounds, suppression, cover, collateral, and catalogue integrity;
- restored-green results for every deliberate defect;
- clear sampled-versus-exhaustive claim language.

Exit criteria:

- every major action has reachable execution evidence;
- every required invariant references model state; and
- each mutation fails through its named detection route before repair.

Completion evidence (2026-08-27):

- `docs/sir-combat-quint-handbook.md` distinguishes concrete examples, existential witnesses,
  transition/observation invariants, sampled executions, bounded exhaustive verification, and
  counterexamples; chapters 33–37 teach nondeterministic trace reading, counterexample minimization,
  reachability, state bindings, and honest claim limits without inventing intermediate runtime state.
- `work/365-handbook-m4/audit-formal-reasoning.mjs` derives temporary fixtures from the literate
  authority and proves six detector-specific observed-red/restored-green pairs for threshold, bounds,
  suppression, cover, collateral, and catalogue-integrity defects. It also checks three major-action
  witnesses and seven authoritative property bindings; the untouched model and full Q4/runtime suite
  remain green.
- Lifecycle evidence: `work/365-handbook-m4/` and `readiness/365-handbook-m4/`; feedback cycle:
  `roadmap-sir-combat-quint-handbook-m4-formal-reasoning`.
- Delivery boundary: this checked entry is the merge-candidate ledger update. Independent exact-head
  acceptance and green exact-head hosted CI are required before merge. It becomes landed completion
  history only when the M4 PR merges; post-merge validation must confirm main, issue #365 closure, and
  project status Done.
- M5, M6, M6V, and M7 remain pending. M6V retains ownership of authoritative mechanics/theory SVGs,
  progressive animation/shader enhancement, accessible fallbacks, render regression, and performance
  qualification before M7 publication.

### - [x] M5 — Runtime correspondence and evidence

**Outcome:** model claims connect to production behavior without merging their authorities.

Deliverables:

- Quint-to-F# correspondence map;
- literate-source and generated-projection explanation;
- exact and sampled ITF replay walkthroughs;
- first-divergence example;
- evidence and observed-red control reference;
- safe rule-change workflow.

Exit criteria:

- every production claim cites a runtime subject and evidence;
- missing correspondence is explicitly classified; and
- the handbook never describes simulation output as proof of implementation equivalence.

Completion evidence (2026-08-27):

- `docs/sir-combat-quint-handbook.md` now maps all sixteen stable rules through controlled `exact`,
  `aggregate`, `external-contract`, `presentation-only`, or `missing` correspondence states and names
  current F# subjects plus scoped evidence for every production claim. Supercover geometry is explicitly
  missing while the visible/total ratio and pinned external contract remain checked.
- Chapters 39–43 distinguish the literate authority from disposable `.qnt`/ITF projections; teach the
  committed exact trace (one trace, nine states) separately from the deterministic sampled corpus
  (seed 352, sixteen traces, 144 states, at most eight steps); explain structured first-divergence output;
  and give a dependency-ordered safe rule-change workflow without equivalence overclaims.
- `work/373-handbook-m5/audit-runtime-correspondence.mjs` owns five structural observed-red/restored-green
  controls while `scripts/qualify-quint-q4-sir-combat.sh` exercises the real interpreter, exact and sampled
  replay, and three independent runtime-correspondence inversions. Dedicated aggregate and JUnit receipts
  are under `readiness/373-handbook-m5/`.
- Lifecycle evidence: `work/373-handbook-m5/` and `readiness/373-handbook-m5/`; feedback cycle:
  `roadmap-sir-combat-quint-handbook-m5-runtime-correspondence`; delivery issue: #373.
- Delivery boundary: this checked entry is the merge-candidate ledger update. Independent feedback
  critique, independent exact-head acceptance, green hosted PR CI, merge, post-merge protected/Pages
  proof, issue #373 closure, and project status Done are required before it becomes landed history.
- M6, M6V, and M7 remain pending. M6V retains authoritative SVG mechanics/theory visuals, progressive
  animation/shader enhancement, accessibility/fallbacks, visual regression, and performance qualification.

### - [x] M6 — Complete definition index and enforced linkability

**Outcome:** every controlled term is one click from its definition.

Deliverables:

- complete alphabetical definition index;
- aliases and related-term links;
- declaration and rule inventories reconciled with the index;
- Markdown-AST link audit integrated with documentation qualification;
- negative controls for missing links, duplicate anchors, and absent index entries.

Exit criteria:

- zero unresolved internal links;
- zero unindexed controlled terms;
- zero unlinked controlled occurrences outside documented exemptions; and
- all deliberate link defects are detected.

Completion evidence (2026-08-27):

- Chapter 50 now contains 188 canonical definitions with declaration loci, related canonical links,
  scoped runtime-correspondence statements, and no planned/Pending placeholders. The schema-v2 vocabulary
  manifest adds five canonical aliases and reconciles the original 185 terms with the three authoritative
  declarations discovered since M0: `UINT32_RANGE`, `wrapInt32`, and
  `damageRoundingPreservesInt32Wrap`.
- `work/375-handbook-m6/audit-handbook-structure.mjs` parses Markdown block/inline structure and proves
  exact coverage for 74 top-level Quint declarations, all sixteen stable rules, fifty chapters, three
  reading paths, 188 index targets, five canonical aliases, internal fragments, and eligible controlled
  prose. Structural exemptions are explicit in the manifest rather than inferred from text.
- Thirteen isolated controls observe red for missing fragments, duplicate anchors, absent index entries,
  unlinked controlled occurrences and model symbols, wrong canonical targets, insubstantial definitions,
  wrong targets hidden inside extended link labels, missing authoritative declarations, exact declaration-module locus drift,
  rule-ID drift across model/runtime, manifest/index alias drift, and coordinated term/index cardinality
  loss before untouched input restores green.
  `scripts/build-docs.sh` now runs
  the audit before every site projection; dedicated qualification and JUnit receipts live under
  `work/375-handbook-m6/` and `readiness/375-handbook-m6/`.
- Lifecycle evidence: `work/375-handbook-m6/` and `readiness/375-handbook-m6/`; feedback cycle:
  `roadmap-sir-combat-quint-handbook-m6-index-link-enforcement`; delivery issue: #375; delivery PR: #376.
- Delivery boundary: this checked entry is the merge-candidate ledger update. Independent exact-head
  acceptance, green relevant hosted CI, merge, exact-main validation, Pages proof, issue closure, and
  project status Done remain required before it becomes landed completion history.
- M6V and M7 remain pending. M6V retains authoritative mechanics/theory SVG reuse, progressive
  animation/shaders, accessibility, reduced-motion/static/print/non-WebGL fallbacks, mechanical
  derivation, visual regression, rendered inspection, and performance qualification.

### - [x] M6V — Authoritative mechanics and theory diagrams

**Outcome:** concrete combat mechanics and formal explanations gain trustworthy, accessible visuals
without creating a second semantic authority.

Deliverables:

- concrete combat-mechanics diagrams that reuse the existing in-game SVG symbology and glyph vocabulary;
- pure abstract SVG diagrams for formal theory, state, dependency, arithmetic, trace, and invariant explanations;
- animation and shader effects only as progressive enhancement;
- reduced-motion, static, print, and non-WebGL fallbacks;
- accessible labels and descriptions for every meaningful visual;
- derivation or mechanical checking against authoritative rules and the Quint model, avoiding duplicated semantics;
- visual-regression/render-inspection evidence and performance qualification.

Exit criteria:

- every concrete visual uses the established in-game SVG vocabulary and every abstract visual remains pure SVG;
- enhanced effects preserve the same meaning under reduced-motion, static, print, and non-WebGL fallbacks;
- labels and descriptions expose the visual's meaning accessibly;
- authoritative rule/model changes invalidate or mechanically recheck affected diagrams; and
- visual regression, rendered inspection, and performance qualification pass.

Completion evidence:

- The handbook embeds six source-bound figures: one concrete attack pipeline reuses the production
  rifleman paths, battlefield footprint, and palette; five pure abstract SVGs explain state/action,
  direct rule dependency, Q4 arithmetic, a safe/violating trace split, and an invariant boundary.
  Each figure has a caption and adjacent text transcript, while each standalone asset carries a
  linked `title`/`desc`, image role, and labeled semantic groups.
- `docs/sir-combat-quint-diagrams.json` is the checked schema-v1 projection over current glyph,
  battlefield, runtime-rule, Quint-model, and controlled-vocabulary sources. The owning audit
  re-derives those bindings and fingerprints, denies independent authority, and observed red before
  restored green for authority, glyph, accessibility, fallback, fingerprint, structural-budget, and
  decoded-readiness timing mutations.
- Static geometry and text own meaning. CSS dash motion and SVG-filter glow are progressive only;
  the same six diagrams passed actual Chromium inspection in normal, reduced-motion, print, effects-off, css-disabled
  modes with stable semantic fingerprints, zero console/page errors, and
  retained non-WebGL meaning. The retained evidence contains 30 standalone and 18 handbook-image screenshots;
  human inspection repaired clipping, print contrast, edge/text collisions, and internal label/border collisions.
- Typed performance evidence covers the declared six-diagram, 153-element, 26,274-byte,
  7-animated-element workload. 100 warm, no-store navigations reached browser-native decoded
  diagram readiness at p95 74.1 ms and p99 84.6 ms within the typed-spec 100/200 ms budgets.
  The retained 350 ms response-delay control observed red at p95 385 ms and p99 387.2 ms before
  untouched restoration. Browser launch (129.54 ms), cold-site decode (128.5 ms), unrelated template
  completion, and the lack of a live compositor are reported separately; no frame-pacing claim is made.
- Focused receipts, screenshots, and qualification live under `work/377-handbook-m6v/` and
  `readiness/377-handbook-m6v/`; feedback cycle:
  `roadmap-sir-combat-quint-handbook-m6v-visual-explanations`; delivery issue: #377.
- Capture provenance resolves available revisions to their exact commit trees and permits a
  squash/shallow-history fallback only when the capture object is absent and the candidate-input
  and render-baseline digests still match exactly.
- Delivery boundary: this checked entry is the merge-candidate ledger update. Independent exact-head
  implementation and feedback acceptance, green relevant hosted CI, merge, exact-main validation,
  Pages proof, issue closure, and project status Done remain required before it becomes landed
  completion history. M7 remains pending unchanged.

### - [x] M7 — Review, publication, and maintenance handoff

**Outcome:** the handbook is published as maintained S.I.R. documentation.

Deliverables:

- domain review;
- Quint language and modeling review;
- beginner walkthrough review;
- rendered-document inspection;
- last-verified toolchain and source identities;
- update checklist and owner handoff.

Dependency: M6V authoritative mechanics and theory diagrams must be complete before M7 publication.

Exit criteria:

- all acceptance criteria pass;
- reviewers approve the domain and model boundaries;
- the S.I.R. docs build is green; and
- the maintenance trigger is documented beside the authoritative model.

Completion evidence:

- Four independent approvals bind exact handbook blob `6a2a2c5c44da82bef28c459497786f35017d17e4`,
  unchanged literate-model blob `2347769cf18c278b27e4af4322580cd50719fe3f`, and diagram-manifest blob
  `7a84db6af105d3653243828e4828beea451e69ea`: domain, Quint/modeling, beginner walkthrough,
  and rendered-document review. Their structured scopes, evidence routes, and limits are retained in
  `work/380-handbook-m7/publication-reviews.json`.
- The maintained handbook now has no scheduled placeholders. Its setup-to-explanation route names exact
  prerequisites, gives copyable literate extraction, witness, sampled-run, and ITF observation commands,
  and keeps bounded-model, runtime-correspondence, and external-geometry claims separate.
- `work/380-handbook-m7/publication-record.json` binds eight exact source blobs, seven pinned tool versions,
  the M6 structure/link gate, immutable M6V visual evidence, and the current successor render replay.
  `docs/rules/sir-combat-handbook-maintenance.md` names the S.I.R. repository maintainer beside the
  authoritative model and supplies an ordered trigger/checklist without changing the model bytes.
- Fresh strict FsDocs output and all six diagrams passed normal, reduced-motion, print, effects-off, and
  CSS-disabled inspection. The 48 receipt-bound captures retain accessible static meaning; 100 warm
  browser-native load/decode samples measured p95 71.4 ms and p99 76.8 ms under the unchanged M6V
  100/200 ms budgets. Evidence remains headless with `liveCompositor:false` and
  `framePacingMeasured:false`; it makes no FPS or compositor claim.
- `work/380-handbook-m7/audit-publication-handoff.mjs` observed red and restored green for eight isolated
  review, identity, owner, placeholder, M6/M6V linkage, budget, and capability mutations. Lifecycle and
  aggregate qualification receipts live under `work/380-handbook-m7/` and `readiness/380-handbook-m7/`;
  feedback cycle: `roadmap-sir-combat-quint-handbook-m7-publication-handoff`; delivery issue: #380.
- Delivery boundary: this checked entry is the merge-candidate ledger update. Exact-head implementation
  and feedback acceptance, green relevant hosted CI, merge, exact-main CI, exact-SHA Pages/live-content
  proof, issue closure, and Project 6 Done remain required before it becomes landed completion history.
  The parent roadmap host retains the separate final cross-cycle roll-up.

## M0 source and authority inventory

Inventory boundary: current S.I.R. `origin/main` is
`77e56d11867a5e2e7ad99f4d61b0f0c9fff61a5f`; the complete Q4 model is candidate material from PR
#355 at `2d41880356997cd0e265180941ffc094e49dd1f9`. Candidate rows are complete inputs to handbook
planning but do not become current authority merely by appearing here.

| Authority class | Source and identity | Status at inventory | Question answered / planned handbook claims |
|---|---|---|---|
| Cross-project direction | `.github` ADR-0077 and Quint-first Typed SDD migration design, as referenced by source design `b72c96d` | External design authority; not copied into S.I.R. | Why Quint is the target language and how migration boundaries work |
| Handbook design | `.github` design and roadmap at `b72c96d` | Imported design source; this file is the S.I.R.-owned ledger | Handbook shape, reading paths, link contract, milestones, and acceptance |
| Corpus architecture | `docs/adr-0001-executable-rules-corpus.md` | Current S.I.R. authority; says F# is canonical today | Stable identity, executable/explainable corpus, replay, and source layering |
| Combat architecture | `docs/combat-resolution.md` | Current S.I.R. domain authority | Physical combat intent, attack order, cover, armor, HP/wounds, suppression, friendly fire, and replay |
| Q4 scope/decisions | PR #355 `work/352-quint-q4-sir-adoption/`, especially `clarifications.md` DEC-001–DEC-007 | Candidate, commit-bound | Sixteen-rule scope, modeling granularity, compatibility, LoS boundary, atomicity, Q4 records, and dependency on FS.GG.SDD#932 |
| Literate Quint model | PR #355 `docs/rules/sir-combat.md` | Candidate standalone-noncanonical | Exact bounded Quint declarations, actions, runs, and properties |
| Generated Quint model | PR #355 extraction to `sir-combat.qnt` performed by qualification | Generated candidate projection; never an authoring source | Bytes consumed by Quint tools |
| Runtime rule corpus | `src/SIR.Simulation/CombatRules.fs` and `.fsi` | Current production/runtime correspondence subject | What current F# executes and which stable rule metadata it publishes |
| Shared domain semantics | `src/SIR.Domain/RuleTypes.*`, `Rules.*`, and fixed-point subjects | Current runtime support authority | Rule kinds, IDs, fixed-point semantics, canonical serialization |
| Current exact/sampled replay | `work/353-quint-q1-sir-replay/`, `readiness/353-quint-q1-sir-replay/`, `tests/SIR.Conformance.Shared/QuintReplayFixtures.fs` | Current scoped evidence for Q1 damage replay only | What has been observed against the real interpreter; not sixteen-rule proof |
| Q4 model evidence | PR #355 qualification script, conformance fixture, standalone receipt, and PR checks | Candidate scoped evidence; PR currently unmerged | Typecheck, runs, invariants, mutations, sampled replay, and correspondence claimed by Q4 |
| Lifecycle evidence | `work/356-handbook-m0/` and `readiness/356-handbook-m0/` | Current M0 evidence when merged | M0 requirement coverage and merge-boundary readiness |
| Feedback evidence | `feedback/checkpoints/roadmap-sir-combat-quint-handbook-m0-authority-inventory.jsonl` plus bound report/audit | Current M0 process evidence when merged | Development-experience observations; not combat-semantic authority |

Every planned claim therefore has an authority class: language/migration claims use cross-project
direction; handbook structure uses the imported design; domain meaning uses combat architecture;
identity/corpus claims use the current ADR and registry; candidate Quint syntax and behavior use the
commit-bound PR #355 model; runtime claims use production F#; verification claims use their explicitly
scoped receipts.

## M0 sixteen-rule inventory

The direct-dependency column is transcribed from both the current F# registry and PR #355 candidate
`ruleCatalogue`. Explanation order is separately modeled and must not be mistaken for this graph.

| Stable rule ID | Kind | Direct dependencies |
|---|---|---|
| `CONTENT-WEAPON-RIFLE-001` | fact | none |
| `CONTENT-BODY-HUMAN-001` | fact | none |
| `COMBAT-ENGAGEMENT-001` | formula | none |
| `COMBAT-TRACE-002` | algorithm | none |
| `COMBAT-ARMOR-004` | formula | none |
| `COMBAT-DAMAGE-001` | formula | `CONTENT-WEAPON-RIFLE-001`, `COMBAT-TRACE-002`, `COMBAT-ARMOR-004` |
| `COMBAT-COLLISION-001` | transition | `COMBAT-TRACE-002` |
| `COMBAT-COVER-003` | transition | `COMBAT-COLLISION-001` |
| `COMBAT-PENETRATION-001` | transition | `COMBAT-COVER-003`, `COMBAT-ARMOR-004` |
| `COMBAT-HEALTH-001` | transition | `COMBAT-DAMAGE-001` |
| `COMBAT-WOUND-001` | transition | `COMBAT-HEALTH-001` |
| `COMBAT-SUPPRESSION-001` | transition | `COMBAT-COLLISION-001` |
| `COMBAT-SUPPRESSION-RECOVERY-001` | transition | `COMBAT-SUPPRESSION-001` |
| `COMBAT-COLLATERAL-001` | transition | `COMBAT-COLLISION-001` |
| `COMBAT-COVER-DESTRUCTION-001` | transition | `COMBAT-COVER-003` |
| `COMBAT-ATTACK-RESOLUTION-001` | transition | `COMBAT-ENGAGEMENT-001`, `COMBAT-COLLISION-001`, `COMBAT-COVER-003`, `COMBAT-PENETRATION-001`, `COMBAT-DAMAGE-001`, `COMBAT-WOUND-001`, `COMBAT-SUPPRESSION-001`, `COMBAT-COLLATERAL-001` |

Canonical count by kind: 2 facts + 3 formulas + 1 algorithm + 10 transitions = 16. Within those
canonical kinds, `COMBAT-TRACE-002` is the external algorithm contract and
`COMBAT-ATTACK-RESOLUTION-001` is the aggregate transition; those descriptions do not replace the
registry's kind values.

## M0 Quint declaration and property inventory

This is the complete top-level inventory of the two candidate Quint modules in PR #355. Local bindings
inside declarations are intentionally not index entries. Each entry already has its planned M6 index kind.

| Planned index kind | Candidate declarations |
|---|---|
| type | `RuleEntry`, `AlgorithmEntry`, `PropertyEntry`, `Wound`, `CombatState`, `AttackInput`, `Observation` |
| variant | `NoWound`, `MinorWound`, `MajorWound` |
| constant/value | `SCALE`, `INT32_MIN`, `INT32_MAX`, `rifleDamageRaw`, `humanArmorRetentionRaw`, `rangeSlopeRaw`, `ruleCatalogue`, `traceAlgorithm`, `consequenceExplanationOrder`, `propertyCatalogue`, `representativeAttack`, `missedAttack`, `alliedAttack`, `initialCombat` |
| pure function | `saturateInt32`, `absolute`, `minimum`, `maximum`, `divideRoundedAwayFromZero`, `fromRatio`, `addFixed`, `multiplyFixed`, `bounded100`, `retainedEffect`, `preparationRaw`, `validTrace`, `traceRaw`, `expectedDamageRaw`, `roundedDamage`, `woundForDamage`, `validAttack`, `damageForAttack`, `suppressionForDamage`, `nextConsequences`, `consequenceObservation`, `coverDamage`, `nextCoverImpact`, `coverObservation`, `recoveredSuppression`, `nextRecovery`, `recoveryObservation`, `fullDamageAttack` |
| state variable | `combat`, `last` |
| action | `init`, `resolveConsequences`, `resolveCoverImpact`, `resolveRecovery`, `step` |
| invariant/property | `sixteenRulesDeclared`, `boundedCombatState`, `incapacityMatchesHealth`, `destroyedCoverIsPermeable`, `validTraceObservation`, `suppressionRequiresDamage`, `factionNeutralCollateral` |
| run/witness | `representativeDamageIsTwenty`, `woundThresholdsAreExact`, `zeroHealthMeansIncapacitated`, `suppressionNeedsPositiveDamageAndRecoversFive`, `destroyingCoverConsumesCurrentCollision`, `collateralOutcomeIgnoresFaction` |
| catalogue property ID | `SixteenRulesDeclared`, `BoundedCombatState`, `IncapacityMatchesHealth`, `DestroyedCoverIsPermeable`, `ValidTraceObservation`, `SuppressionRequiresDamage`, `FactionNeutralCollateral` |

The future index must also define the modules `SirCombat` and `SirCombatTests` and the imported namespace
relationship, even though modules are containers rather than declarations inside the tables above.

## M0 controlled-vocabulary inventory

| Class | Initial canonical terms |
|---|---|
| Stats and bounded quantities | health/HP, damage, base damage, expected damage, range cells, visible samples, total samples, trace probability, armor retention, suppression, suppression delta, cover integrity, cover damage, wound threshold, preparation time |
| Units and encodings | Q4 raw integer, scale 10,000, fixed-point ratio, hit points, damage points, suppression points, integrity points, cells, samples, seconds, signed 32-bit saturation, round-half-away-from-zero |
| Combat concepts | target footprint, physical shot trace, first collision, projectile contact, cover blocking, destroyed cover, current-collision consumption, penetration, retained effect, wound, incapacitation, suppression eligibility, suppression recovery, collateral consequence, faction-neutral consequence, aggregate attack resolution, explanation order, event identity, registered line-of-sight implementation |
| Quint/model concepts | module, import, type, variant, record, set, list, constant, pure value, pure function, state variable, initialization, guard, action, primed assignment, nondeterminism, run, witness, invariant, state transition, reachable state, stuttering, terminal state |
| Formal/evidence concepts | prediction, execution trace, ITF trace, property, safety property, sampled run, bounded verification, exhaustive check, counterexample, mutation, observed-red control, restored green, first divergence, correspondence, claim boundary, source digest, generated projection, external algorithm contract |

M6 owns canonical anchors, aliases, occurrence linking, and AST-aware enforcement. This M0 list owns only
the initial membership and categorization needed to prevent omissions in later milestones.

## M0 scope exclusions

- The handbook does not replace the combat architecture, corpus ADR, stable rule IDs, or runtime.
- M0 does not create `docs/sir-combat-quint-handbook.md`; navigation, anchors, traceability rows, initial
  index content, vocabulary manifest, and link-audit prototype are M1.
- No supercover traversal is reproduced in Quint; `FS.GG.Game.Core.Los.lineOfSightBy` remains external.
- Generated `.qnt` bytes are a projection, never an independent source or direct edit target.
- The bounded model does not include unrelated application behavior, death/finalization beyond
  zero-health incapacitation, all weapon/body catalogues, spatial traversal internals, UI, persistence,
  networking, or AI.
- Sampled execution and replay are not described as exhaustive proof or automatic implementation
  equivalence.
- M0 does not change the current F#-canonical posture or claim that FS.GG.SDD#932 has landed.

## M0 source disagreements

| Topic | Sources | Status / handbook treatment | Changes proposed state shape or action granularity? |
|---|---|---|---|
| Current authoring authority | ADR-0001 says F# is canonical; cross-project direction and Q4 target Quint authority | Real temporal disagreement, not silently reconciled. State current F# authority and candidate standalone Quint status until the consumer-model profile and migration land. | No |
| Candidate model availability | Source design describes `docs/rules/sir-combat.md` as S.I.R. authority; file exists only on unmerged PR #355 at inventory time | Bind all declaration claims to PR #355 head and downgrade from current authority to candidate. Re-evaluate after #355 changes or merges. | No |
| Canonical Typed SDD integration | Q4 DEC-007 requires FS.GG.SDD#932; PR #355 says standalone-noncanonical | Open external dependency. The handbook may teach the checked candidate but must not claim canonical migration or generated F#/Fable binding. | No |
| Aggregate atomicity | Combat runtime publishes completed consequences; Q4 DEC-005 uses one atomic consequence action plus focused pure helpers and separate cover/recovery actions | Sources agree; retain this boundary and do not invent runtime-visible intermediate states. | No |
| State representation | Q4 DEC-006 selects raw Q4 integers and cohesive `CombatState`, `AttackInput`, `Observation` records | User-approved candidate design and model agree; no contrary current runtime fact requires a different abstract state. | No |
| Cover destruction timing | Combat design and candidate witness require the projectile-blocking cover to consume the current collision even when destroyed | Sources agree; later walkthrough must distinguish current collision from later permeability. | No |
| Evidence breadth | Current Q1 replay proves only a damage slice; Q4 claims sixteen-rule sampled correspondence in unmerged evidence | Scope difference, not semantic disagreement. Label every evidence claim by receipt/commit and never generalize Q1 to the full registry. | No |

There are unresolved authority and sequencing items, but none changes the proposed state shape or action
granularity. Any future Q4 head change invalidates the commit-bound declaration/property inventory and
must trigger re-inventory before M1 exits.
