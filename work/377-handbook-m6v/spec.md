---
schemaVersion: 1
workId: 377-handbook-m6v
title: Handbook M6V visual explanations
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
performanceIntent:
  id: handbook-m6v-visuals-v1
  disposition: active
  targetFps: 60
  workloadIds:
    - handbook-m6v-six-diagram-render-v1
  workloadDefinitionDigests:
    - handbook-m6v-six-diagram-render-v1=sha256:9845c702798b8655fdfab2cc7db749a4efa897a3e1c82cfc83acf23acbfef3df
  maximumExpectedScale: "six diagrams; 180 SVG elements; 120 KiB; 24 animated elements"
  maxP95Ms: 100
  maxP99Ms: 200
  maxCatchUpFrames: 0
  structuralCostBudgets:
    - "aggregate-bytes<=122880"
    - "aggregate-elements<=180"
    - "animated-elements<=24"
    - "diagram-bytes<=20480"
    - "diagram-elements<=30"
  requiredCapability: headless-browser
  liveCompositorRequired: false
  evidenceRefs:
    - docs/performance-budget.md
  rationale: "The real strict-FsDocs/headless-browser route measures warm navigation until all six diagrams are visible plus deterministic structural counters; cold browser/context startup is separately observed, 60 Hz is progressive-animation intent only, and the host lacks a live compositor and cannot claim frame pacing."
---

# Handbook M6V visual explanations Specification

Prose status: specified

## User Value
A Quint learner can see concrete combat mechanics and abstract formal reasoning in the handbook, while a maintainer can trust that every visual is source-bound, accessible, fallback-complete, and cheap enough for the real documentation route.

## Scope
- SB-001: Complete roadmap M6V only: authoritative mechanics/theory SVGs, progressive effects, fallbacks, accessibility, mechanical authority checking, render regression/inspection, structural performance qualification, lifecycle, feedback, roadmap, PR, merge, and Pages evidence.

## Non-Goals
- SB-002: Do not change combat rules, Quint declarations, runtime behavior, production unit glyphs, palette semantics, or evidence claim boundaries.
- SB-003: Do not make Canvas, WebGL, WebGPU, CSS animation, filters, screenshots, or a diagram manifest an authority; do not claim live-compositor timing on the headless host.
- SB-004: Do not complete M7 domain/model/beginner editorial review or maintenance handoff.

## User Stories
- US-001 (P1): As a combat learner, I can understand one attack from rifleman through trace, collision, cover, armor, damage, wound, suppression, and collateral through the game visual vocabulary.
- US-002 (P1): As a Quint learner, I can see state/action, rule dependency, Q4 arithmetic, nondeterministic trace, and invariant explanations as pure abstract SVGs.
- US-003 (P1): As a learner using assistive technology or constrained rendering, I receive the same meaning through titles, descriptions, labels, static geometry, reduced motion, print, and unsupported-effect fallbacks.
- US-004 (P1): As a maintainer, I can mechanically detect authority drift, visual regression, accessibility loss, fallback loss, and structural performance overflow before publication.
- US-005 (P1): As a roadmap owner, I can verify only M6V completed and M7 remains pending.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given the mechanics visual, when its source bindings and SVG are inspected, then it reuses exact production rifleman glyph path data, palette tokens, footprint/contact/cover/health/status symbols, and all cited stable rule IDs.
- AC-002 [US-002] [FR-002]: Given the formal visuals, when their markup is inspected, then state/action, dependency, Q4 arithmetic, trace, and invariant subjects are represented by pure SVG geometry/text bound to current Quint declarations or rule metadata.
- AC-003 [US-003] [FR-003]: Given any meaningful diagram, when read as a standalone SVG or embedded figure, then one title, one description, labeled groups/text, role semantics, and a prose transcript expose its full meaning.
- AC-004 [US-003] [FR-004]: Given reduced-motion, static, print, CSS-disabled, filter-unsupported, or non-WebGL conditions, when the visual is rendered, then no semantic label or state disappears and no effect is required to understand it.
- AC-005 [US-004] [FR-005]: Given a rule/model/glyph/manifest change, when the source-binding audit runs, then an affected visual is invalidated or mechanically rechecked without manually duplicating semantics.
- AC-006 [US-004] [FR-006]: Given the real strict FsDocs output, when headless Chromium renders the handbook at normal, reduced-motion, print-emulation, and effects-disabled routes, then every figure is visible, sized, labeled, free of console/page errors, and captured for deterministic inspection.
- AC-007 [US-004] [FR-007]: Given six diagrams, when the typed performance route runs, then each stays at or below 30 SVG elements and 20 KiB, the aggregate stays at or below 180 elements, 120 KiB, and 24 animated elements, and capability facts state that compositor FPS was not measured.
- AC-008 [US-004] [FR-008]: Given isolated authority, glyph, accessibility, fallback, render-snapshot, and budget mutations, when qualification runs, then each named detector observes red and untouched inputs restore green.
- AC-009 [US-005] [FR-009]: Given the roadmap and lifecycle receipts, when delivery is inspected, then M6V alone is checked with evidence and M7 remains pending unchanged.

## Functional Requirements
- FR-001: The concrete mechanics diagram MUST reuse the exact production rifleman glyph path and the established battlefield palette, footprint, trace/contact, cover, armor, health/wound, suppression, and collateral vocabulary while naming every bound stable rule ID. (covers AC-001)
- FR-002: Five abstract diagrams MUST remain pure SVG and explain state/action, rule dependency, scale-10,000 arithmetic, nondeterministic trace/counterexample reading, and invariant reasoning from current authoritative declarations and rule metadata. (covers AC-002)
- FR-003: Every meaningful SVG MUST provide one nonempty `title`, one nonempty `desc`, `role="img"`, `aria-labelledby`, labeled semantic groups/text, a figure caption, and an adjacent prose transcript. (covers AC-003)
- FR-004: Animation and SVG-filter effects MUST be progressive-only; base geometry/text MUST carry all meaning and explicit reduced-motion, static, print, CSS-disabled, filter-unsupported, and non-WebGL routes MUST preserve it. (covers AC-004)
- FR-005: A schema-versioned diagram manifest and audit MUST bind diagram ids/subjects to current rule IDs, Quint declarations, vocabulary anchors, exact production glyph primitives, and source digests or derived values without becoming an executable authority. (covers AC-005)
- FR-006: Qualification MUST build the real strict FsDocs site and inspect the generated handbook in headless Chromium for normal, reduced-motion, print, and disabled-effects routes, retaining deterministic render fingerprints and human-inspectable screenshots. (covers AC-006)
- FR-007: Typed performance evidence MUST declare the six-diagram production docs workload, structural byte/node/animation budgets, measured counters, and host/capability facts; it MUST refuse stale workload definitions or exceeded budgets and MUST not invent wall-clock/FPS verdicts. (covers AC-007)
- FR-008: Qualification MUST isolate at least six mutations covering authority drift, production glyph drift, accessibility loss, fallback loss, visual fingerprint drift, and performance overflow; every mutation MUST observe its named red detector before untouched inputs restore green. (covers AC-008)
- FR-009: Dedicated qualification MUST own source-binding, SVG/accessibility/fallback, rendered inspection, visual-regression, performance, strict-docs, roadmap, and lifecycle receipts; only M6V may become checked while M7 remains pending. (covers AC-009)

## Ambiguities
- AMB-001: Source-bound SVG generation could either commit generated SVGs or commit authored SVGs plus exact audits; the authority and reviewability tradeoff must be explicit.
- AMB-002: Shader language could imply WebGL, but the handbook is an accessible SVG publication and non-WebGL fallback is mandatory.
- AMB-003: The producer performance budget governs the tactical game route, while the new documentation diagrams need bounded costs without inventing a second FPS contract.
- AMB-004: Render regression needs deterministic evidence despite browser raster differences and a host with no live compositor.

## Public Or Tool-Facing Impact
- Adds published SVG assets and a checked diagram manifest to the handbook, plus a documentation qualification gate and machine-readable render/performance receipts.

## Lifecycle Notes
- Issue: `EHotwagner/S.I.R.#377`; stable feedback cycle: `roadmap-sir-combat-quint-handbook-m6v-visual-explanations`.
- M3 through M6 explicitly handed this scope to M6V; all those deferrals must be discharged before ship.
- Next lifecycle action: `fsgg-sdd clarify --work 377-handbook-m6v`.
