---
feedbackSchema: 2
date: 2026-08-28
workspace: S.I.R-roadmap-m7
cycle: roadmap-sir-combat-quint-handbook-m7-publication-handoff
lane: sdd
toolVersion: 1.4.0
commit: e6cb1971e1c8d01d1d8112853e3cb94aaa37a3f9
---

# Development feedback — combat Quint handbook M7 publication handoff

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring, implementation-test-evidence, verify-ship-pr
- **material events:** 4
- **zero-event reason:** n/a

This cycle covers issue #380 on isolated branch `item/380-handbook-m7` from exact base
`318f07a7ae30e98d86a5d5780126cab1e105b7f2` through lifecycle commit
`e6cb1971e1c8d01d1d8112853e3cb94aaa37a3f9`. Four immutable checkpoints at
`feedback/checkpoints/roadmap-sir-combat-quint-handbook-m7-publication-handoff.jsonl` record route onboarding,
SDD authoring, touch-set repair, and exact-source review convergence. The report covers the maintained
handbook, adjacent owner trigger, exact identity record, independent subject approvals, strict FsDocs,
and current headless SVG load/decode replay. Checkpoint chronology is worker testimony unless a retained
terminal receipt, current file, command, issue, or commit independently verifies the stated event. It
does not claim live compositor, FPS, physical-printer,
empirical learner-study, hosted CI, merge, or Pages proof; those delivery facts were pending at draft.

## §2 What worked

The milestone kept semantic authorities separate: combat design, the literate Quint model, production
F#, controlled vocabulary, diagram manifest, and handbook pedagogy retain distinct ownership. The
authoritative model stayed byte-identical while an adjacent maintenance page names the S.I.R.
repository maintainer, triggering inputs, ordered gates, review roles, and publication obligations.

Four independent reviews bind the same exact handbook/model/diagram identities. They rejected stale
approvals after every repair. The final beginner route names pinned prerequisites and gives copyable
extraction, witness, sampled simulation, and committed-ITF observation commands. The M6 AST/link audit
passes 188 definitions, 74 declarations, 16 rules, and 50 chapters. Strict FsDocs renders the handbook
and maintenance page. Fresh inspection verifies six SVGs in five standalone modes plus three handbook
modes, 48 screenshots total, with p95 71.4 ms and p99 76.8 ms under the unchanged M6V 100/200 ms
load/decode budgets. Eight isolated M7 mutations each observed red before untouched green.

## §3 What did not

The onboarding checkpoint records that the initial route record was refused because the caller had not
precomputed the framed SHA-256 digest; no raw refusal receipt was retained. The command help names a
receipt file but does not provide an authoring command. The implementation checkpoint records that the
adjacent maintenance page was first added just outside the declared route; current issue/route facts
independently verify the subsequent route widening, not the order of the first write or claim-touch-set widening.

Checkpoint testimony records that independent review retired three handbook candidates. Reviewer
messages during the cycle found an external geometry overclaim,
a registry body-retention fact conflated with the representative 0.8 replay input, missing beginner
host prerequisites, commands targeting a nonexistent root `sir-combat.qnt`, an incorrect `missedAttack`
glossary description, and wording that appeared to pin npm to Node's version. Each reported repair changed
the handbook identity, invalidated earlier approvals, and required a fresh render replay; discarded
candidate counts are not retained as standalone receipts. This was costly,
but the exact-source boundary prevented stale evidence from becoming publication approval.

One direct `scripts/build-docs.sh --prepare-site-only` invocation failed because ambient
`/usr/share/dotnet` could not resolve the pinned 10.0.302 SDK. Propagating the same `DOTNET_ROOT`,
`DOTNET_ROOT_X64`, and `DOTNET_HOST_PATH` used by the owner qualifier restored the strict build. No
package or tool pin was changed.

## §4 Findings

#### §4.1 Delivery-route recording exposes validation but not digest authoring

- **Kind:** friction
- **Impact:** A correctly scoped issue cannot reach Ready/claim until the caller manually reconstructs the route receipt's framed digest.
- **Expected:** The recorder derives the canonical digest or exposes a command that authors a valid receipt from route facts.
- **Observed:** The checkpoint records the digest-free refusal without a retained terminal receipt. Current route revision 3 and issue chronology independently verify that a digest-valid receipt was later recorded and re-read before the successful claim.
- **Evidence:** file:feedback/checkpoints/roadmap-sir-combat-quint-handbook-m7-publication-handoff.jsonl; issue:EHotwagner/S.I.R.#380; command:scripts/fsgg-coord delivery-route show 380 --json
- **Version:** FS.GG.Coord.Cli 0.72.0; route schema `fsgg.coord.route-decision/v2`.
- **Owner:** FS-GG/.github coordination delivery-route authoring surface
- **Recurrence:** duplicate of the residual missing-route authoring cause recorded against closed FS-GG/.github#2698 and M6V feedback §4.1.
- **Avoidable cost:** checkpoint testimony reports one refused Ready write and two route-record attempts; exact attempt output was not retained.
- **Disposition:** accepted

#### §4.2 Analyze rejected an unauthored migration placeholder before implementation

- **Kind:** positive-pattern
- **Impact:** A scaffold placeholder could otherwise masquerade as an accepted compatibility decision and weaken downstream readiness.
- **Expected:** Cross-artifact analysis refuses placeholder posture until a concrete additive/compatibility decision is authored.
- **Observed:** The lifecycle checkpoint records the initial PM-001 rejection without retaining that red analysis view. The current authored plan plus fresh analysis independently verify the explicit additive compatibility posture and restored 68/68 ready relationships with no blockers.
- **Evidence:** file:work/380-handbook-m7/plan.md; file:readiness/380-handbook-m7/analysis.json; command:artifacts/tools/fsgg-sdd-1.4.0/fsgg-sdd analyze --work 380-handbook-m7 --text
- **Version:** FS.GG.SDD.Cli 1.4.0.
- **Owner:** FS-GG.SDD analyze placeholder gate
- **Recurrence:** no matching prior S.I.R. feedback finding was identified for this exact placeholder rejection.
- **Avoidable cost:** checkpoint testimony reports one plan/tasks regeneration and analyze rerun; the retained evidence proves only the repaired state.
- **Disposition:** accepted

#### §4.3 Touch-set widening occurred after the adjacent file was first added

- **Kind:** orchestration
- **Impact:** A correct model-adjacent maintenance design briefly existed outside its declared delivery route, weakening coordination observability.
- **Expected:** The issue route and held claim include every intended path before the first write.
- **Observed:** The checkpoint records that the first add preceded route/claim widening; no exact pre-widen tree or claim-touch-set receipt was retained. Current issue/route history independently verifies that revision 2 added the exact path and final revision 3 remains `sdd-required`; the `widen` operation itself remains checkpoint testimony.
- **Evidence:** issue:EHotwagner/S.I.R.#380; command:scripts/fsgg-coord delivery-route show 380 --json
- **Version:** FS.GG.Coord.Cli 0.72.0.
- **Owner:** EHotwagner/S.I.R. M7 worker orchestration
- **Recurrence:** self-owned one-cycle process rework; no duplicate external issue was found.
- **Avoidable cost:** checkpoint testimony reports one issue-body edit, route revision, and claim widen after the first add; the final route state is independently verified.
- **Disposition:** accepted

#### §4.4 Exact-source reviews prevented stale publication approval

- **Kind:** positive-pattern
- **Impact:** Semantic, beginner, and toolchain wording defects would have survived if approvals were carried across edited handbook bytes.
- **Expected:** Every approval binds exact source blobs; any handbook repair retires prior verdicts and current render provenance.
- **Observed:** The verify/ship checkpoint reports three retired candidates and four replays; discarded outputs are not retained as independent receipts. Current evidence independently verifies that four final approvals bind handbook `6a2a2c5c…`, model `2347769…`, and manifest `7a84db6…`; fresh render digest `a794b7b8…` records 48 captures and p95/p99 71.4/76.8 ms.
- **Evidence:** file:work/380-handbook-m7/publication-reviews.json; file:work/380-handbook-m7/publication-record.json; file:readiness/380-handbook-m7/rendered/inspection.json; command:node work/380-handbook-m7/audit-publication-handoff.mjs --self-test
- **Version:** Node.js 26.5.0 contract; Playwright 1.62.1; FsDocs 22.1.0; Quint 0.32.0.
- **Owner:** EHotwagner/S.I.R. handbook publication qualification
- **Recurrence:** extends M6V's exact candidate provenance pattern to independent human/agent review attestations.
- **Avoidable cost:** checkpoint testimony reports three retired candidates, repeated subject review, and four fresh 100-sample replays; current retained evidence proves the final replay only.
- **Disposition:** accepted

## §5 Did not exercise

No combat rule, Quint declaration/action, production runtime, SVG asset, package, lockfile, visual budget,
or production game renderer changed. The existing Q4/runtime and M6/M6V evidence is regression input,
not new semantic authority. Java/Apalache verification, live-compositor timing, GPU/shader throughput,
physical printing, assistive-technology user studies, and empirical beginner studies were not exercised.

## §6 Doc-versus-behavior contradictions

Review repaired all observed contradictions: `traceAlgorithm` now names `lineOfSightBy` and leaves its
geometry missing; the human body fact maps to the registry's full-retention value while 0.8 replay
retention belongs to COMBAT-ARMOR-004; `missedAttack` is valid-target with zero visible samples; and the
learner commands recreate the literate projection instead of naming a nonexistent file. The source
precedence section still makes the handbook pedagogical rather than semantic authority.

## §7 Workarounds still in the tree

The M7 qualifier explicitly exports the resolved pinned .NET host because child FsDocs project cracking
does not reliably inherit the agent's SDK resolution. The successor branch in `scripts/build-docs.sh`
selects M7 publication evidence when present; this avoids pretending the immutable M6V candidate digest
covers later handbook bytes. The M6V headless loopback/decode probe is reused unchanged and remains
explicitly limited to external SVG readiness.

## §8 Friction and avoidable cost

Checkpoint testimony records one route refusal, one touch-set repair, one placeholder regeneration, and
three retired review candidates with repeated render capture; retained current-state evidence verifies
the repairs and final replay rather than each discarded attempt. Separately,
one ambient-SDK docs invocation was rerun with pinned host propagation. These costs did not alter combat
semantics, SVG bytes, package pins, render workload, or performance budgets.

## §9 Skill value and gaps

`work-roadmap` bounded M7 and preserved the parent-owned cross-cycle roll-up. `pnext-item` enforced issue,
claim, route, independent review, relevant CI, and post-merge obligations. The full SDD lifecycle made
requirements/decisions/tasks/evidence explicit and rejected placeholder posture. `fs-gg-feedback-report`
required four immutable phase checkpoints and an independent actionability audit. Quint language/modeling
review supplied exact syntax and claim-boundary scrutiny without changing the model. The remaining gap is
a first-class route-receipt authoring command and clearer standalone propagation of the pinned .NET host.

## §10 Outcome markers

- Route: revision 3 digest `c96118cf…`, `sdd-required`, claimed by `swift-49fe` on Project 6 issue #380.
- Sources: handbook `6a2a2c5c…`; model preserved `2347769…`; maintenance `151395f…`; eight source blobs and seven tools checked.
- Reviews: four independent exact-source approvals with no unresolved material finding.
- Structure: 188 definitions, five aliases, 74 declarations, 16 rules, 50 chapters; 13 retained M6 red/restored controls.
- Render: six diagrams, five modes, 48 fresh screenshots, 100 samples after three warmups, p95 71.4 ms and p99 76.8 ms under unchanged 100/200 ms budgets.
- Publication: eight M7 red/restored controls; strict FsDocs and successor build selector green under the pinned host environment.
- Delivery: SDD evidence/verify/ship, exact-head critics, hosted PR CI, merge, exact-main/Pages/live proof, issue Done, and cleanup were pending at report draft.

## §11 Falsifiable improvements

- Preserve §4.1 by adding a route-authoring command. Acceptance: an equivalent issue reaches a digest-valid route without caller-implemented framing while malformed facts still fail closed.
- Preserve §4.2. Acceptance: an unchanged scaffold placeholder keeps `analyze` non-ready, while a concrete additive compatibility decision restores a fully ready relationship graph.
- Prevent §4.3. Acceptance: a worker compares intended paths with the route and claim before the first write; `verify-paths` later reports OK for exactly that set.
- Preserve §4.4. Acceptance: changing the handbook, model, manifest, receipt, metrics, or any review verdict makes the M7 audit fail; four current-source approvals plus a fresh 48-capture replay restore green.
- Preserve capability limits. Acceptance: current and inherited evidence keep 100/200 ms budgets and explicitly report `liveCompositor:false` and `framePacingMeasured:false`.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| scaffolding | not-exercised | Existing fable-game scaffold; no generator parameters or ownership changed. |
| onboarding-guidance | exercised | Roadmap, AGENTS, Project 6, route/claim, and prerequisites governed the cycle; route digest authoring was friction. |
| skills | exercised | Work-roadmap, pnext-item, feedback-report, full SDD lifecycle, and Quint review contracts shaped the work. |
| sdd-authoring | exercised | Eight FR/AC pairs, three decisions, 28 tasks, and 68/68 ready relationships; placeholder posture was rejected. |
| implementation-apis | not-exercised | Runtime and model APIs were reviewed but not changed. |
| dependencies-build | exercised | Exact .NET, Node, Quint, FsDocs, Playwright, SDD, and Coord identities checked; strict FsDocs passed with pinned host propagation. |
| testing | exercised | Four subject reviews, M6 structure/link gate, Q4 regression, 48 render captures, and eight M7 mutations passed. |
| evidence | partial | Exact review/render/identity receipts exist; final SDD verify/ship was pending at draft. |
| runtime-playtest | not-exercised | Runtime correspondence is retained regression evidence, not a new gameplay journey. |
| performance | exercised | Existing M6V workload replayed at p95/p99 71.4/76.8 ms without a new performance intent or budget. |
| documentation | exercised | Maintained handbook, owner trigger, exact identities, accessible diagrams, strict rendered output, and roadmap ledger completed. |
| packaging-upgrade | not-exercised | No package, dependency, or lockfile changed. |
| worker-git-pr | partial | Isolated claim and implementation commit exist; critics/PR/merge/post-merge proof were pending. |
