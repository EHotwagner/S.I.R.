---
feedbackSchema: 2
date: 2026-08-13
workspace: S.I.R
cycle: item-183-tactical-overlays
lane: sdd
toolVersion: 1.0.1
commit: development-head
---

## §1 Provenance and confidence

- **activation:** active
- **phases:** onboarding-first-build, lifecycle-authoring-or-not-used, implementation-test-evidence, verify-ship-pr
- **material events:** 7
- **zero-event reason:** n/a
- **checkpoint:** `feedback/checkpoints/item-183-tactical-overlays.jsonl` (7 records)
- **confidence limits:** Native Release qualification, production Fable/Vite, published Chromium interaction/accessibility, protected-subject inversions, clean aggregate, and SDD readiness are observed on the repair candidate. Confirmation round 1 by the same independent reviewer and hosted exact-head CI remain pending.

## §2 What worked

The public registry is reused for overlay identity, label, order, defaults, and supported modes, reducing drift for those fields. A renderer-neutral discriminated union now carries disclosed footprint extents, headings/arcs, paths with cost and blockers, areas, traces/impacts, and status values/tokens into payload-kind-aware SVG rendering. Availability, command modality/shortcut mapping, payload classification, and disclosure-policy enforcement remain explicit code paths rather than registry-owned behavior and are tested separately.

## §3 What did not

The initial exact-head review found that generic point/string payloads could not express the advertised layer set, selected annotations lacked subjects, the production hold/contrast/node gates were decorative, and all 54 SDD obligations reused one narrow browser receipt. Repair required a typed geometry contract, renderer extraction, substantive browser and 100/200-unit gates, an atomic evidence generator, and obligation-specific receipts. The first repaired aggregate then correctly stopped on stale hash-bound review assets; regenerating those assets from the repaired production bundle restored the aggregate.

## §4 Findings

#### §4.1 Typed renderer-neutral geometry makes the advertised layer set falsifiable

- **Kind:** positive-pattern
- **Impact:** Initial layers can express and render their disclosed tactical values without browser-side reconstruction from strings.
- **Expected:** Authority adapters provide structured geometry/state and the renderer consumes the corresponding union case exactly.
- **Observed:** `TacticalOverlayGeometry` carries footprint, direction, path/cost/blockers, area, trace/impact, and status shapes. The Release 100-unit workload emits every one of the fourteen registry IDs, the 200-unit stress workload emits more payloads, and assertions reject zero-radius areas or missing blocker-bearing paths. The published Fable SVG renders union-specific rectangles, lines, polylines, blocker marks, circles, impacts, and status marks.
- **Evidence:** source:src/SIR.Client/TacticalSceneProjection.fsi; source:src/SIR.Client/TacticalSceneProjection.fs; source:src/SIR.Client.Web/TacticalOverlayView.fs; source:tests/SIR.Client.Tests/TacticalSceneProjectionQualification.fs; readiness/183-tactical-overlays/tactical-overlays-native.junit.xml; readiness/183-tactical-overlays/tactical-overlays-fable-production.junit.xml
- **Version:** repair-round-1 development head
- **Owner:** EHotwagner/S.I.R. tactical overlays
- **Recurrence:** repair of initial-review F1
- **Avoidable cost:** the first candidate's generic payload contract deferred this distinction until independent review
- **Disposition:** repaired; confirmation pending

The adapters preserve only facts available in the existing disclosed scene vocabulary. Exact-head confirmation must still check that no layer claim exceeds those source facts.

#### §4.2 Production accessibility and node budgets now depend on emitted behavior

- **Kind:** positive-pattern
- **Impact:** Hold-to-inspect, forced-colors styling, 400% geometry, and the 5,000-node cap can no longer pass from static attributes or undercounted estimates.
- **Expected:** Tests interact with built DOM controls and compare computed/emitted results with declared budgets.
- **Observed:** The production journey dispatches pointer down/up and observes `InspectHeld` enter/exit, checks computed stroke width before and after forced-colors emulation, verifies non-scaling footprint geometry at 400% zoom, and compares every emitted overlay descendant with the renderer-neutral count. Separate subject mutations for hold handlers, forced-colors styling, and node accounting each produced a red JUnit. Release qualification exercises all modes over 100 and 200 units, enforces the declared 20 ms p95 cap, and records the measured p95 in its native JUnit.
- **Evidence:** source:tests/SIR.Browser.Tests/visible-workflows.spec.js; source:src/SIR.Client.Web/styles.css; readiness/183-tactical-overlays/tactical-overlays-browser.junit.xml; readiness/183-tactical-overlays/tactical-overlays-hold-inversion.junit.xml; readiness/183-tactical-overlays/tactical-overlays-contrast-inversion.junit.xml; readiness/183-tactical-overlays/tactical-overlays-node-inversion.junit.xml; readiness/183-tactical-overlays/repair-round-1-inversions.json
- **Version:** repair-round-1 development head
- **Owner:** EHotwagner/S.I.R. browser and projection qualification
- **Recurrence:** repair of initial-review F2
- **Avoidable cost:** three initial gates asserted metadata rather than their behavioral subject
- **Disposition:** repaired; confirmation pending

The Release timing is host evidence and the browser node check is structural; neither is a live-compositor frame-rate claim.

#### §4.3 Atomic evidence generation exposed stale dependencies and enabled honest obligation mapping

- **Kind:** positive-pattern
- **Impact:** A failed prerequisite cannot partially replace canonical receipts, and unrelated obligations no longer inherit one narrow browser result.
- **Expected:** One reproducible command runs declared prerequisites and installs only completed native, Fable, browser, and aggregate receipts; SDD entries point to the route that observes their subject.
- **Observed:** `generate-item-183-evidence.sh` stages four receipts in a temporary directory and installs them only after Release native qualification, production Fable/Vite, focused published Chromium, and the self-contained `Test` aggregate pass. `test-conformance.sh` now builds the Release Domain.Tests prerequisite before the no-build worker smoke. Evidence entries are split among native projection, production browser/accessibility, Fable production build, and complete aggregate sources; verify records 54 observed evidence and 54 observed test dispositions with zero findings.
- **Evidence:** source:scripts/generate-item-183-evidence.sh; source:scripts/test-conformance.sh; work/183-tactical-overlays/evidence.yml; readiness/183-tactical-overlays/tactical-overlays-native.junit.xml; readiness/183-tactical-overlays/tactical-overlays-fable-production.junit.xml; readiness/183-tactical-overlays/tactical-overlays-browser.junit.xml; readiness/183-tactical-overlays/tactical-overlays-aggregate.junit.xml; readiness/183-tactical-overlays/verify.json
- **Version:** repair-round-1 development head
- **Owner:** EHotwagner/S.I.R. evidence and lifecycle
- **Recurrence:** repair of initial-review F3
- **Avoidable cost:** the first repaired aggregate required one review-asset regeneration after the stale hash gate fired
- **Disposition:** repaired; confirmation pending

The aggregate receipt records command success as one test case; detailed subsystem outcomes remain in the command output and owning focused receipts rather than being invented as aggregate testcase counts.

## §5 Did not exercise

No server protocol, simulation-authority algorithm, save format, upstream package release, or live-compositor frame-rate changed or is claimed by this repair.

## §6 Doc-versus-behavior contradictions

The tactical overlay documentation previously described availability, shortcut modality, payload classification, and disclosure policy as registry-owned. It now limits registry reuse to the fields actually consumed and names the remaining explicit code paths.

## §7 Workarounds still in the tree

Existing replay/simulator overlay vocabularies still enter through adapter classification because their retained contracts predate typed tactical overlay IDs. The classification occurs before renderer-neutral projection; browser code does not infer tactical truth. Future authority work should publish typed IDs/cost/blocker/event fields directly.

## §8 Friction and avoidable cost

The initial candidate's narrow evidence and decorative assertions deferred material defects to independent review. During repair, the hash-bound review-asset gate added one necessary regeneration cycle; no product or test defect remained after regeneration.

## §9 Skill value and gaps

The SDD, project, grids, line-drawing, visibility, playtest, and parallel-work contracts kept the authority and exactness boundaries visible. The repair showed that structural attributes alone are insufficient playtest evidence: interaction, computed style, geometry, and emitted-node subjects need direct assertions and inversions.

## §10 Outcome markers

Native Release qualification is green with every advertised family present in 100 units and the 200-unit all-layer projection below the 20 ms p95 budget on this host. Production Fable/Vite, focused published Chromium, full isolated aggregate, replay/conformance, and documentation gates are green. Verify contains zero findings, 54 observed test dispositions, and 54 observed evidence dispositions; ship has zero findings and diagnostics. Refresh is coherent and `noChange`.

## §11 Falsifiable improvements

- Keep the typed geometry union and require a red subject mutation whenever a new case or renderer branch is added.
- Keep actual emitted descendant counting equal to the renderer-neutral node record at 100/200-unit scale.
- Add typed overlay IDs and authoritative route/event metadata to retained producer contracts before removing the current adapter classification.
- Retain obligation-specific receipts and the atomic generator instead of relabeling one journey as broad lifecycle evidence.

## §12 Development-surface coverage

| Surface | Status | Evidence and result |
|---|---|---|
| sdd-authoring | exercised | Analyze/evidence/verify/ship are current; refresh reached `noChange`. |
| scaffolding | not-used | Existing repository and SDD package were repaired in place; no scaffold changed. |
| onboarding-guidance | exercised | AGENTS and generated lifecycle guidance were read and refreshed. |
| implementation-apis | exercised | Public typed geometry, routes, annotations, and projections compile in .NET and Fable. |
| dependencies-build | exercised | Native Release and production Fable/Vite builds pass. |
| skills | exercised | SDD, project, grids, line-drawing, visibility, playtest, and parallel-work contracts guided the repair. |
| packaging-upgrade | not-used | No package version or dependency contract changed. |
| testing | exercised | Native, focused published Chromium, protected-subject inversions, and full aggregate pass in restored state. |
| evidence | exercised | 54 observed obligations use native, Fable, browser, or aggregate receipts according to subject. |
| runtime-playtest | exercised | Built DOM View, pointer hold/release, shortcut, reload, computed contrast, node count, and 400% geometry pass. |
| performance | exercised | Representative 100 and stress 200 all-layer projection plus actual SVG descendant caps pass. |
| documentation | exercised | Tactical overlay and performance contracts describe typed geometry and measurement limits. |
| worker-git-pr | partial | Repair commit/push, exact-head CI, same-critic confirmation, and landing remain pending. |
