---
schemaVersion: 1
workId: 377-handbook-m6v
title: Handbook M6v
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/377-handbook-m6v/spec.md
sourceClarifications: work/377-handbook-m6v/clarifications.md
sourceChecklist: work/377-handbook-m6v/checklist.md
publicOrToolFacingImpact: true
---

# Handbook M6v Plan

Prose status: planned

## Source Snapshot
- spec: work/377-handbook-m6v/spec.md sha256:3c503cbcc7ee4e4440f3d0c76c3debe1477f935ab82352ea0773288fe4d1a61d schemaVersion:1
- clarifications: work/377-handbook-m6v/clarifications.md sha256:7051c8f60eb2ad7443fc73d5b7b54773c81e1ac722b356950ed4a0a0996e838f schemaVersion:1
- checklist: work/377-handbook-m6v/checklist.md sha256:3273a2335bc8ff1f7cdf9d5caa66818b7448b88e98682d974fd7cb89aabcb6ee schemaVersion:1

## Plan Scope
- Add six standalone, embedded SVG figures and one schema-v1 binding manifest to the existing handbook without changing combat/model/runtime authorities.
- Extend docs qualification with source/glyph binding, SVG semantics/fallback, canonical regression, real Chromium render, structural performance, and mutation/restoration gates.
- Publish M6V evidence at the roadmap boundary; preserve M7 pending.

## Technical Context
- FsDocs renders Markdown through the production documentation pipeline. Standalone SVGs remain pure document assets; the generated handbook embeds each through an accessible figure/image plus adjacent transcript.
- `UnitGlyphCatalog.fs` provides normalized 24×24 paths and accessible palette facts; `Battlefield.fs` supplies the production symbolic meanings. The manifest stores exact expected tokens/digests and the audit recomputes current values.
- `docs/rules/sir-combat.md` and the runtime stable rule registry supply declarations/rule ids. The audit derives current inventories just as M6 does, and rejects dangling or unbound diagram subjects.
- Playwright is already restored by the repository and will render the generated handbook from a local static server. Semantic DOM fingerprints gate regression; screenshots are retained for inspection but not compared byte-for-byte across hosts.

## Constitution Check
- I/II/III: SDD analysis precedes assets; the JSON manifest is the typed visual contract and SVG/prose remain explanatory projections.
- IV/V: dependency-free Node audits and pure SVG avoid a new rendering framework or state/I/O authority.
- VI/VIII: real strict-docs/Chromium routes plus isolated mutations prove each detector can fail; missing capability is reported rather than guessed.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: Author one mechanics SVG that uses the rifleman primitives verbatim, palette tokens and battlefield symbology, and manifest bindings for the aggregate attack plus every depicted stable rule.
- PD-002 [AC-002] [FR-002] [DEC-001] complete: Author five abstract SVGs for state/action, dependency, arithmetic, trace, and invariant reasoning using geometry/text only and manifest bindings to exact Quint declarations/rule ids.
- PD-003 [AC-003] [FR-003] complete: Require standalone `title`/`desc`/ARIA/group labels and handbook figure captions/transcripts for every diagram; audit nonempty unique label references and embed parity.
- PD-004 [AC-004] [FR-004] [DEC-002] complete: Carry semantics in static geometry/text; restrict motion to CSS classes and shader-like enhancement to SVG filters; add media-query print/reduced-motion and explicit `data-effects="off"` styles.
- PD-005 [AC-005] [FR-005] [DEC-001] complete: Add `docs/sir-combat-quint-diagrams.json` schema v1 and an audit that re-derives rule/declaration/glyph/source facts, validates vocabulary anchors, and canonicalizes SVG fingerprints.
- PD-006 [AC-006] [FR-006] [DEC-004] complete: Build strict FsDocs, serve the exact site output, and inspect the generated handbook in Chromium across normal/reduced-motion/print/effects-off routes for visibility, layout, labels, console/page errors, and screenshots.
- PD-007 [AC-007] [FR-007] [DEC-003] complete: Implement the predeclared workload digest and structural/timing/capability receipt over the six rendered diagrams; p95/p99 cover load-to-all-visible only and do not claim live-compositor frame pacing.
- PD-008 [AC-008] [FR-008] complete: Generate isolated temporary fixtures for authority, glyph, accessibility, fallback, fingerprint, and budget mutations; assert each named detector/red message, then run untouched input green.
- PD-009 [AC-009] [FR-009] complete: Aggregate focused receipts, strict docs, lifecycle/feedback/review, ledger state, hosted CI, merge, and Pages evidence while preserving M7 pending.

## Contract Impact
- PC-001 [PD-001] [PD-002] [PD-005] diagramManifest: `docs/sir-combat-quint-diagrams.json` schema v1 names six diagram ids, kinds, asset paths, source bindings, accessible ids, fallback/effect contracts, fingerprints, and budgets; it describes but never executes combat semantics.
- PC-002 [PD-003] [PD-004] svgPublication: Each asset is standalone pure SVG with static semantic geometry/text, exact ARIA label relationships, optional CSS motion/filter enhancement, and explicit fallback selectors.
- PC-003 [PD-006] renderReceipt: Browser evidence binds generated handbook route, modes, viewport, diagram ids, DOM fingerprints, visible sizes, accessibility labels, screenshots, timings, and capability facts.
- PC-004 [PD-007] performanceReceipt: Typed evidence binds workload id/digest, candidate source, six-diagram scale, structural counters, p95/p99 load timings, headless capability, and no-live-compositor limitation.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-005] [PC-001] authorityTest: Re-derive exact production glyph primitives/palette tokens, current Quint declarations, stable rule ids, vocabulary anchors, asset fingerprints, and reject any mismatch.
- VO-002 [PD-003] [PD-004] [PC-002] accessibilityFallbackTest: Parse all six assets and generated embeds; require complete ARIA/title/description/labels/transcripts plus reduced-motion/print/effects-off/static-semantic selectors.
- VO-003 [PD-005] [PC-001] regressionTest: Canonicalize meaningful SVG DOM and compare the manifest fingerprint; ignore no semantic node/attribute/text.
- VO-004 [PD-006] [PC-003] renderedInspection: Build strict FsDocs and run real Chromium normal/reduced-motion/print/effects-off inspection with screenshots, console/page-error refusal, and all-six-visible assertions.
- VO-005 [PD-007] [PC-004] performanceTest: Run PERF-SMOKE before assets and PERF-RELEASE on exact candidate; verify workload digest, scale, counters, p95/p99, headless capability, and honest compositor limitation.
- VO-006 [PD-008] mutationTest: Observe six named red mutations and untouched restored green using disposable fixtures.
- VO-007 [PD-009] lifecycleTest: Run focused/full docs and relevant product qualification, SDD evidence/verify/ship, feedback validators, roadmap audit, exact-head review/CI/path/delivery, merge, and exact-main Pages proof.

## Performance Intent
- id: handbook-m6v-visuals-v1
- disposition: active
- targetFps: 60
- workloadIds: [handbook-m6v-six-diagram-render-v1]
- workloadDefinitionDigests: [handbook-m6v-six-diagram-render-v1=sha256:9845c702798b8655fdfab2cc7db749a4efa897a3e1c82cfc83acf23acbfef3df]
- maximumExpectedScale: six diagrams; 180 SVG elements; 120 KiB; 24 animated elements
- maxP95Ms: 100
- maxP99Ms: 200
- maxCatchUpFrames: 0
- structuralCostBudgets: [aggregate-bytes<=122880, aggregate-elements<=180, animated-elements<=24, diagram-bytes<=20480, diagram-elements<=30]
- requiredCapability: headless-browser
- liveCompositorRequired: false

## Migration Posture
- PM-001 [PC-001] additive-v1: New diagram manifest/assets are additive; schema other than 1 is rejected and no diagram semantics are inferred from unknown fields.
- PM-002 [PC-002] fallback-first: Existing prose remains complete. If SVG embedding, CSS motion, filters, or browser capabilities are absent, prose/transcripts and static SVG retain the learning path.

## Generated View Impact
- GV-001 [PD-005] [PD-006] diagramViews: Source-binding, canonical-regression, browser-render, screenshot, and performance receipts regenerate from current assets/site or report stale/missing evidence.
- GV-002 [PD-009] lifecycleViews: Analysis, work model, verify, ship, feedback report/audit, roadmap evidence, and review/delivery receipts refresh from exact current sources/head.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- The host supports headless Chromium but exposes no protected live compositor; render/layout/accessibility/timing evidence is valid for the declared route, while animation smoothness and GPU frame pacing are explicitly unclaimed.
- No focused `fs-gg-scene`/Skia skill applies to standalone FsDocs SVG assets. The repository's production SVG/glyph/performance documentation supplies the subsystem contract.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 377-handbook-m6v`.
