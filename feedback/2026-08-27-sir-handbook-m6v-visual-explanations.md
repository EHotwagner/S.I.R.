---
feedbackSchema: 2
date: 2026-08-27
workspace: S.I.R-roadmap-m6v
cycle: roadmap-sir-combat-quint-handbook-m6v-visual-explanations
lane: sdd
toolVersion: 1.4.0
commit: f507d57e5be9ca57e2bbc1ba413484605b95f832
---

# Development feedback — combat Quint handbook M6V visual explanations

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring, implementation-test-evidence, verify-ship-pr
- **material events:** 4
- **zero-event reason:** n/a

This cycle covers issue #377 on isolated branch `item/377-handbook-m6v` from exact base `c47286a2961cb50720714bad6e17115c05117dc5` through the commit above. Four immutable checkpoints in `feedback/checkpoints/roadmap-sir-combat-quint-handbook-m6v-visual-explanations.jsonl` cover route/claim onboarding, typed performance authoring, rendered implementation evidence, and verify/ship. Exact-head reviews, hosted PR CI, merge, exact-main validation, issue/project completion, and exact-SHA Pages proof were pending at draft time.

The existing scaffold is `FS.GG.Workspace.Template` 0.8.0 with provider `fable-game` contract 1.1.0. The repository pins Fable 5.13.0, fsdocs-tool 22.1.0, Quint 0.32.0, and Playwright 1.62.1; lifecycle views and commands in this report use `fsgg-sdd` 1.4.0. M6V adds documentation visuals and qualification only: it does not change the authoritative F#/Quint combat semantics, packages, or production game renderer. Confidence covers exact source binding, static meaning and fallbacks, structural budgets, the strict FsDocs route, and pinned headless Chromium image readiness; it does not claim live-compositor frame pacing or empirical learner comprehension.

## §2 What worked

The milestone's nine FR/AC pairs and four clarification decisions preserved a single semantic authority. One manifest binds six diagrams to exact source digests, stable rule/declaration identities, and the existing game glyph/palette vocabulary. The concrete attack pipeline reuses the production symbology; the state, dependency, arithmetic, trace, and invariant diagrams are abstract pure SVG. The handbook embeds all six as external images with captions and adjacent text transcripts, so static document meaning is present independently of animation, filters, SVG scripting, or WebGL.

The qualification deliberately separates structural cost from observed readiness. It records exactly six diagrams, 153 SVG elements, 26,274 bytes, and seven animated elements against the typed-spec limits; it then measures 100 warm same-browser no-store navigations from navigation start until all six SVG images load, decode, and have non-zero intrinsic dimensions. The resulting p95 74.1 ms and p99 84.6 ms pass the unchanged 100/200 ms candidate limits. Browser launch (129.54 ms), cold-site readiness (128.5 ms), and caller/template tail are observations outside that gating subject. Six source/accessibility/fallback/structural mutations and one permanent in-subject timing mutation each observed red and restored green. Strict docs, 30 standalone mode checks, 18 handbook-image screenshots, the full client glyph/Battlefield qualifier, and the existing Quint/runtime correspondence suite passed.

The full SDD lifecycle reached `implementationReady`, 35 completed tasks, 35 observed real evidence passes, 35 observed test passes, and `shipReady` with no synthetic/deferred claims or blocking findings. M6V's roadmap evidence accurately names the merge-candidate boundary: the checked box becomes default-branch completion only by merge, while M7 remains pending.

## §3 What did not

The first checkpoint records that a claim was refused because issue #377 had no structured route receipt; that terminal refusal output was not retained. Independently visible issue chronology shows a route revision manually constructed, digested, posted, and re-read before the successful worker claim. The canonical Project 6 and issue were otherwise correct.

The first typed performance intent used noncanonical structural keys; the lifecycle-authoring checkpoint records the corrective re-baseline. During rendered qualification, the initial `<object>` embeds created six nested browsing contexts and the early timing probe measured Playwright caller delay rather than user-visible SVG readiness. Replacing them with ordinary external `<img>` resources reduced browsing-context overhead and enabled a browser-init `MutationObserver`/`decode()` probe tied to the actual six diagrams. Human screenshot passes then found a clipped right-edge caption, weak print contrast, a branch crossing a label, and two internal label/border collisions; all were repaired and rerendered. The standard performance-evidence contract also required canonical alphabetical structural-budget ordering before evidence converged.

The report does not claim an independently committed pre-implementation numeric declaration: PERF-PLAN, PERF-SMOKE, the typed spec, and the first implementation were committed together. What repository history does prove is that the first candidate `6204e70` already carried the six-diagram ceilings and 100/200 ms budgets. Exact review strengthened the workload policy from digest `9845c702…` to `158170af…` by adding CSS-disabled rendering and increasing the release probe from 30 to 100 samples after a tail outlier; it did not raise those ceilings or timing budgets. The permanent mutation now adds a 350 ms response delay inside the same browser readiness subject and records p95 385 ms/p99 387.2 ms against 100/200 before untouched input restores green. Cold startup, garbage-collection/caller tail, and unrelated template CDN load remain separately reported rather than relabeled as diagram readiness. The earlier `<object>`/caller-delay investigation survives only in the immutable checkpoint narrative; no raw failing receipt was retained for it.

## §4 Findings

#### §4.1 Structured route prerequisite recurred on a newly authored roadmap issue

- **Kind:** friction
- **Impact:** A worker assigned a correctly scoped SDD issue cannot claim it until someone authors and posts a digest-valid v2 route record, even when issue body, paths, dependencies, and canonical project are already present.
- **Expected:** Roadmap issue creation or dispatch produces the structured route receipt needed by the claim gate, or names an explicit authoring command that derives it from the issue's declared route and Paths.
- **Observed:** Issue chronology independently verifies issue creation followed by a worker-authored v2 route comment and then a successful `kite-97e4` claim. The checkpoint records an earlier refused claim, but no terminal refusal receipt was retained, so that part remains claim-only.
- **Evidence:** file:feedback/checkpoints/roadmap-sir-combat-quint-handbook-m6v-visual-explanations.jsonl; issue:EHotwagner/S.I.R.#377; command:scripts/fsgg-coord delivery-route show 377 --json
- **Version:** FS.GG.Coord.Cli 0.72.0; route schema `fsgg.coord.route-decision/v2`.
- **Owner:** FS-GG/.github coordination delivery-route creation and dispatch
- **Recurrence:** duplicate of closed FS-GG/.github#2698's missing-route/unschedulable-work cause. That issue added refusal at Ready-transition seams; it does not own automatic route authoring. Open FS-GG/.github#2728 owns claim/lease extraction, not this residual authoring request.
- **Avoidable cost:** one refused claim and one manual route digest/post/re-read cycle.
- **Disposition:** accepted

#### §4.2 Typed ceilings remained stable while the release-probe policy strengthened

- **Kind:** positive-pattern
- **Impact:** Structural complexity, capability limits, the six-diagram workload, and warm readiness have one machine-readable contract across spec, plan, smoke, manifest, and exact-candidate evidence instead of separate literals that can drift during repair.
- **Expected:** Spec and plan name the workload, scale, budgets, browser capability, warm/cold boundary, and no-live-compositor limitation; candidate evidence uses the same digest and limits, while the report distinguishes final consistency from independently proven pre-implementation ownership.
- **Observed:** Commit `6204e70` establishes the six-diagram ceilings and 100/200 ms limits. The final policy digest is intentionally different: `158170af…` adds CSS-disabled inspection and 100 samples after the original `9845c702…` policy's 30-sample tail proved too luck-sensitive. Final candidate p95/p99 are 74.1/84.6 ms. Structural output is 153 elements, 26,274 bytes, and seven animated elements. No separate earlier commit proves when the numeric limits were first chosen.
- **Evidence:** commit:6204e70c823ac9706568067d4d951b344de6cbb6; commit:99b0d9eaf98e766170c197b9a1e016d82277d0e8; file:work/377-handbook-m6v/spec.md; file:work/377-handbook-m6v/plan.md; file:readiness/377-handbook-m6v/performance-smoke.json; file:readiness/377-handbook-m6v/performance-evidence.json; file:readiness/377-handbook-m6v/visual-qualification.json
- **Version:** FS.GG.SDD.Artifacts and CLI 1.4.0; Playwright 1.62.1; Chromium 151.0.7922.34; FsDocs 22.1.0.
- **Owner:** FS-GG SDD performance-intent contract and EHotwagner/S.I.R. documentation qualification
- **Recurrence:** extends the typed-intent positive pattern in `feedback/2026-08-15-sir-item-184-elaborate-tactical-sample.md` §4.1 to a documentation render workload.
- **Avoidable cost:** one lifecycle correction from noncanonical to canonical structural-budget keys; no independently committed PERF-PLAN boundary.
- **Disposition:** accepted

#### §4.3 Browser-native decoded-image readiness now has a real timing overflow control

- **Kind:** quality-gap
- **Impact:** Nested SVG browsing contexts and an automation-caller timestamp can make a healthy user-visible route appear slow, invite budget laundering, and obscure whether diagrams themselves or unrelated site startup dominate.
- **Expected:** The real rendered handbook route identifies the six external SVG resources, observes navigation-to-loaded-and-decoded readiness in the browser clock, reports cold/browser/caller observations separately, and makes a permanent delay inside that exact subject fail.
- **Observed:** The repaired route uses six external `<img>` resources and a browser-init observer plus `HTMLImageElement.decode()` with non-zero intrinsic dimensions. It reports 100-sample warm p95/p99 74.1/84.6 ms separately from 129.54 ms browser launch and 128.5 ms cold-site readiness. A retained 350 ms in-subject response-delay mutation records p95/p99 385/387.2 ms against the unchanged 100/200 ms limits; the full owner command then restores green. Thirty standalone diagram-by-mode records and 18 handbook-image screenshots passed after clipping, print-contrast, edge/text, and label/border repairs.
- **Evidence:** file:work/377-handbook-m6v/inspect-rendered-visuals.mjs; file:readiness/377-handbook-m6v/rendered/inspection.json; file:readiness/377-handbook-m6v/performance-evidence.json; file:readiness/377-handbook-m6v/timing-overflow-mutation.json; command:bash work/377-handbook-m6v/qualify-handbook-m6v.sh
- **Version:** S.I.R. lifecycle evidence commit `f507d57`; Playwright 1.62.1; Chromium 151.0.7922.34; FsDocs 22.1.0.
- **Owner:** EHotwagner/S.I.R. documentation render qualification
- **Recurrence:** no matching open or closed S.I.R. issue or prior feedback finding was found for this exact nested-SVG/timing-subject failure.
- **Avoidable cost:** three measurement redesign runs, replacement of six embeds, two screenshot-driven repair rounds covering clipping, print contrast, edge/text, and label/border collisions, and one critic-rejected false-green structural mutation before the timing control was added.
- **Disposition:** accepted

#### §4.4 One aggregate observed receipt plus the standard performance artifact kept lifecycle claims coherent

- **Kind:** positive-pattern
- **Impact:** Every lifecycle task is bound to an actually executed qualification run, while performance keeps its typed sample-set semantics instead of being represented by prose or a generic passing test.
- **Expected:** Evidence import marks obligations observed only from a real aggregate JUnit; performance evidence additionally carries the standard workload/digest/sample/budget/capability contract; verify and ship fail if either is absent or stale.
- **Observed:** The final evidence graph has 35 supported observed evidence dispositions and 35 satisfied observed test dispositions, no synthetic or deferred claims, and `shipReady`. Adding the standard performance artifact and canonicalizing structural-budget ordering closed the initial evidence fixpoint without weakening any obligation.
- **Evidence:** file:work/377-handbook-m6v/evidence.yml; file:readiness/377-handbook-m6v/qualification.junit.xml; file:readiness/377-handbook-m6v/verify.json; file:readiness/377-handbook-m6v/ship-verdict.json; command:fsgg-sdd verify --work 377-handbook-m6v; command:fsgg-sdd ship --work 377-handbook-m6v
- **Version:** FS.GG.SDD.Artifacts and CLI 1.4.0; evidence schema 1; `performance-evidence-v1`.
- **Owner:** FS-GG/FS.GG.SDD evidence and performance contracts
- **Recurrence:** extends `feedback/2026-08-27-sir-handbook-m2-representative-attack.md` §4.4, where exact-head review corrected over-attributed aggregate receipts; M5 discusses observed-report import in §3 and §11 rather than §4.2.
- **Avoidable cost:** one evidence fixpoint correction.
- **Disposition:** accepted

## §5 Did not exercise

No combat rule, Quint action, runtime state transition, package, dependency version, application renderer, or game shader changed. The full existing Q4/runtime and production glyph/Battlefield routes ran as regression evidence, not as new semantic implementation. Live-compositor frame pacing, GPU shader throughput, mobile/browser matrix behavior, empirical accessibility-tool audits beyond document/SVG semantics, and learner usability studies were not measured. M7 domain/model/beginner editorial review, publication review, and maintenance handoff remain explicitly pending.

## §6 Doc-versus-behavior contradictions

No authority contradiction remains. The concrete visual binds exact production glyph, neutral overlay-layer styling, six overlay IDs, palette tokens, and the representative authoritative rule path. Each abstract visual binds exact Quint declaration/rule identities and source digests. The manifest expressly denies semantic authority, and the audit makes any bound authoritative-source change invalidate the diagrams. Human renders revealed presentation contradictions that root clipping alone could not see: one label clipped, print contrast was weak, one edge crossed text, and two internal labels touched borders. Rendered inspection, not prose, drove the repairs.

## §7 Workarounds still in the tree

`work/377-handbook-m6v/inspect-rendered-visuals.mjs` starts a pinned no-store loopback host and injects the readiness observer before site code; this is deliberate measurement control, not production logic. `work/377-handbook-m6v/qualify-handbook-m6v.sh` resolves and propagates the pinned .NET host to child FsDocs processes because ambient resolver variables are unreliable in agent environments. Optional SVG animation/filter effects remain CSS-only and are disabled by reduced motion, print, and the explicit effects-off mode. The external `<img>` embedding is the production optimization; it preserves static semantics but does not expose internal SVG nodes to the surrounding HTML accessibility tree, so complete alt text, caption, and adjacent transcript are retained in the document.

## §8 Friction and avoidable cost

One route refusal required manual structured-route authoring. Performance intent needed one canonical-key re-baseline. Three rendered timing runs were discarded while isolating the correct clock and replacing nested contexts; repeated screenshot passes required clipping, print-contrast, edge/text, and label/border repairs. Evidence required one canonical ordering/sample-artifact fixpoint. These costs did not change the fixed performance budgets or semantic scope. The release-probe policy did intentionally strengthen from four modes/30 samples to five modes/100 samples while retaining the original ceilings.

## §9 Skill value and gaps

`work-roadmap` enforced the exact milestone boundary, isolated worktree, roadmap ledger, feedback checkpoints, exact review, guarded merge, and Pages obligations. `pnext-item` and its performance-first contract required PERF-PLAN/PERF-SMOKE sequencing and a single typed declaration; the cycle artifacts record that sequence, while commit history does not independently split planning from implementation. The SDD lifecycle carried charter through ship and failed closed on noncanonical performance/evidence forms. `fs-gg-feedback-report` requires the four immutable checkpoints and independent actionability critique. The existing game SVG/rule corpus supplied the vocabulary and authority; no rule-authoring or Quint-modeling skill was invoked because changing semantics was out of scope. The main remaining gap is reusable documentation-render performance guidance for external SVG readiness and nested browsing-context avoidance.

## §10 Outcome markers

- Claim chronology: the checkpoint records a refusal without a retained terminal receipt; issue history verifies route comment followed by successful `kite-97e4` claim on Project 6.
- Lifecycle readiness: nine FR/AC pairs, four decisions, 82 ready analysis findings, zero blockers/warnings.
- Visual structure: six diagrams, 153 elements, 26,274 bytes, seven animated elements; six source/accessibility/fallback/structural red-restored mutations plus one retained timing red-restored control.
- Rendered route: strict FsDocs handbook, 30 standalone observations and 18 handbook-image screenshot receipts across normal, reduced-motion, print, effects-off, and CSS-disabled modes; zero console/page errors.
- Performance: 100 warm samples after three warmups; p95 74.1 ms, p99 84.6 ms against unchanged first-candidate 100/200 ms limits; launch and cold-site observations separate; retained 350 ms in-subject mutation p95/p99 385/387.2 red before restored green.
- Runtime regression: full production glyph/Battlefield qualifier and Quint 0.32.0 Q4/runtime correspondence passed.
- Lifecycle evidence: 35 tasks, 35 observed evidence passes, 35 observed test passes, no synthetic/deferred claims, `shipReady`.
- Delivery: exact-head implementation review, independent final feedback audit, hosted PR CI, merge, Project 6 Done, branch deletion, and exact-main/Pages proof pending at report draft time.

## §11 Falsifiable improvements

- Treat §4.1 as the known FS-GG/.github#2698 cause. A residual improvement would auto-author a digest-valid route when a roadmap issue declares route, work id, dependencies, and Paths; acceptance is a newly created equivalent SDD issue reaching a successful claim without manual route-comment construction while ambiguous scope still fails closed.
- Preserve §4.2 by keeping workload `handbook-m6v-six-diagram-render-v1`, final digest `158170af…`, six-diagram scale, structural limits, five modes, 100 samples, 100/200 ms thresholds, and headless/no-compositor capability consistent from spec through standard evidence. Acceptance: final spec, plan, smoke, manifest, and evidence contain one matching declaration; history honestly records the policy strengthening from `9845c702…` rather than claiming the digest never changed.
- Preserve §4.3 by running the browser-init decoded-image probe on the actual strict FsDocs route. Acceptance: p95/p99 derive only from the 100 warm in-browser readiness samples; browser launch and cold-site remain separately labeled; a permanent in-subject delay over 200 ms fails without changing budgets.
- Preserve semantic non-authority. Acceptance: changing a bound rule, declaration, vocabulary manifest, production glyph/palette token, transcript, or SVG semantic fingerprint makes `node work/377-handbook-m6v/audit-visual-explanations.mjs` fail until the visual is intentionally reconciled.
- Preserve fallbacks and human rendering. Acceptance: normal, reduced-motion, print, effects-off, and CSS-disabled each retain identical labeled mechanics for all six diagrams; non-WebGL/static document text remains complete; 30 standalone plus 18 handbook-image screenshots show no clipping, contrast loss, edge/text crossing, or internal border collision.
- Preserve §4.4 by syncing the real aggregate JUnit with `fsgg-sdd` 1.4.0 and retaining the standard performance artifact. Acceptance: verify reports 35 supported/observed evidence dispositions and 35 satisfied/observed test dispositions, and ship reports `shipReady`, with no synthetic, deferred, stale, or invalid evidence.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing fable-game scaffold; no parameter or generated-product ownership changed. |
| onboarding-guidance | exercised | AGENTS, Project 6, roadmap, pnext route/claim, and performance-first contracts governed delivery; manual route authoring was friction. |
| skills | exercised | Work-roadmap, pnext-item/performance-first, feedback-report, and full SDD lifecycle shaped the work and evidence. |
| sdd-authoring | exercised | Nine requirements, four decisions, 35 tasks, typed performance intent, 82 ready analysis findings, and current verify/ship views. |
| implementation-apis | partial | Existing SVG glyph/palette/rule/model inputs were consumed and mechanically bound; production APIs were not changed. |
| dependencies-build | exercised | Locked restore, Release build, strict FsDocs 22.1.0, pinned Chromium/Playwright, and Quint 0.32.0 passed. |
| testing | exercised | Source binding, accessibility, fallbacks, semantic fingerprints, 30 standalone mode checks, 18 handbook-image screenshots, seven red/restored mutations, strict docs, glyph/Battlefield, and Q4/runtime passed. |
| evidence | exercised | 35 observed real evidence and test dispositions plus standard performance evidence reached `shipReady`. |
| runtime-playtest | not-exercised | Existing headless runtime correspondence is regression proof, not a new user gameplay journey. |
| performance | exercised | Typed workload/digest/scale/budgets, PERF-SMOKE, structural counters, 100 warm decoded-image samples, cold separation, and a retained in-subject overflow mutation passed; independent pre-implementation numeric ownership is not claimed. |
| documentation | exercised | Six authoritative/non-authoritative SVG explanations, captions, transcripts, fallbacks, strict rendered site, inspection, and roadmap evidence. |
| packaging-upgrade | not-exercised | No package or lock version changed. |
| worker-git-pr | partial | Isolated branch, issue/project claim, candidate commit, lifecycle ship readiness, and draft feedback exist; reviews/PR/merge/post-merge proof were pending. |
