---
schemaVersion: 1
workId: 192-tactical-visual-system
title: Tactical Visual System
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/192-tactical-visual-system/spec.md
sourceClarifications: work/192-tactical-visual-system/clarifications.md
sourceChecklist: work/192-tactical-visual-system/checklist.md
publicOrToolFacingImpact: true
---

# Tactical Visual System Plan

Prose status: planned

## Source Snapshot
- spec: work/192-tactical-visual-system/spec.md sha256:10a108eb2cd03cd2f66b808e86b45d36d8b4ef463c7dfa93931c03d9dd335b87 schemaVersion:1
- clarifications: work/192-tactical-visual-system/clarifications.md sha256:5bb02dfe1cbe869ec499a5051b245f89931b2697cb8031796b7f086bb8482db7 schemaVersion:1
- checklist: work/192-tactical-visual-system/checklist.md sha256:42b014fe31bcaf01b6bb95cbd054cabc9c8586e69c0f853ad981f6279ddede23 schemaVersion:1

## Plan Scope
- Extend the existing renderer-neutral `BattlefieldScene` projection with one coherent token registry and bounded disclosed-event effect projections, then consume those values in the shared Fable/SVG workscreen.
- Keep simulation frames, replay fingerprints, exact overlays, disclosure filtering, and tactical geometry authoritative; Web rendering receives already-projected data and stable identities.
- Qualify with focused Client and browser routes while editing, one final source-frozen production aggregate, deterministic review metadata, and exact-SHA hosted acceptance.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: Add typed material, hierarchy, motion, and effect token records beside `BattlefieldScene`; resolve them centrally from the current palette and expose stable renderer metadata rather than adding component-local semantic literals.
- PD-002 [AC-001] [FR-002] [DEC-001] complete: Preserve exact unit footprint/facing/health projection and enhance the SVG piece hierarchy with material faces, role/faction/state data, status channels, and semantic-zoom detail using the existing glyph catalog and palette authority.
- PD-003 [AC-002] [FR-003] [DEC-003] complete: Render deterministic terrain/edge/unit/overlay/effect layers in a named order; cap effect instances and keep all transient primitives pointer-inert, label-safe, and outside exact-overlay geometry.
- PD-004 [AC-003] [FR-004] [DEC-004] complete: Keep `interpolatedScene` as presentation-only safe continuity; exact ticks, reduced motion, replay seek, discontinuities, spawns, removals, footprint changes, and non-adjacent moves snap to committed projection.
- PD-005 [AC-003] [FR-005] [DEC-003] complete: Project disclosed committed events into stable `ProjectedEffect` values keyed by event id/tick/kind, classify only known presentation semantics, bound instance count, and derive geometry solely from visible projected source/target centers.
- PD-006 [AC-004] [FR-006] [DEC-004] complete: Emit reduced-motion state at the scene root and replace travel/loop animation with brief outline/opacity emphasis plus persistent text/state markers; browser qualification exercises the same route under `prefers-reduced-motion`.
- PD-007 [AC-004] [FR-007] complete: Extend token-driven CSS with high-contrast/forced-color affordances, non-color state shapes, focus hierarchy, narrow-width reflow, and effective 400% zoom checks while retaining configurable `ReplayPalettes`.
- PD-008 [AC-005] [FR-008] [DEC-002] complete: Add deterministic representative/stress event fixtures and counters for 100/200 units, 5,000/9,000 nodes, 128/256 effects, 4/8 ms Release p95 pure projection, and a 16.67 ms browser main-thread frame-work ceiling; retain inherited 200/400 guards and exclude elapsed time from fingerprints.
- PD-009 [AC-006] [FR-009] [DEC-003] complete: Require disclosed summaries and visible endpoints before effect geometry exists; incomplete source/target disclosure can produce only non-geometric disclosed emphasis and cannot reveal a missing identity, count, direction, or outcome.
- PD-010 [AC-007] [FR-010] [DEC-005] complete: Extend deterministic evidence with token/effect/layer identities, generate ordinary/dense/stress review artifacts from the production bundle, exercise real-entry browser controls, and subject-mutate every new or modified gate once.

## Contract Impact
- PC-001 [PD-001] [PD-005] additive client contract: `BattlefieldScene` gains typed visual-system and projected-effect values; `Battlefield.deterministicEvidence` gains stable visual/effect metadata without changing authoritative `RenderFrame` or replay transport.
- PC-002 [PD-002] [PD-003] additive DOM contract: the persistent tactical SVG exposes visual-system, layer-order, motion, density, and effect attributes/classes plus CSS custom properties consumed only as presentation metadata.
- PC-003 [PD-008] performance contract: `docs/performance-budget.md` and focused Client/browser fixtures declare the 100/200 workload definitions, structural caps, host/timing evidence, and non-compositor limitation.
- PC-004 [PD-010] review contract: a deterministic generator emits bundle-bound ordinary/dense/stress visual metadata and before/after inspection artifacts suitable for the independent critic.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PC-001] semanticTest: Focused Client assertions prove centralized complete tokens, unit hierarchy, semantic zoom, palette invariance of exact geometry, and deterministic evidence.
- VO-002 [PD-003] [PD-005] [PD-009] semanticTest: Focused Client fixtures prove stable effect classification/order/keys, instance caps, visible-endpoint geometry, unknown/incomplete fail-closed behavior, pointer-inert layer order, and replay reconstruction.
- VO-003 [PD-004] [PD-006] semanticTest: Focused Client and browser journeys prove safe interpolation convergence, discontinuity snapping, scrub reset, exact ticks, reduced-motion causal feedback, and no stale/detached effect.
- VO-004 [PD-007] [PC-002] browserJourney: A real-entry Playwright journey exercises default/high-contrast/reduced-motion, narrow width, effective 400% zoom, keyboard focus, and readable state without direct reducer injection.
- VO-005 [PD-008] [PC-003] performance: Release 100/200 workloads record deterministic counters, allocation proxies, workload identity, host facts, p95 observations, and explicit headless/browser limitations; budget mutations must red.
- VO-006 [PD-010] [PC-004] visualReview: Generate and inspect bundle-bound ordinary/dense/stress review artifacts and verify their exact source/bundle identity and critical layer/contrast metadata.
- VO-007 [PD-010] gateInversion: For each test/checker added or modified, mutate the protected production subject once and record the exact red outcome before restoring it.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additive: Existing `BattlefieldScene` consumers are updated atomically; `RenderFrame`, replay transport, persisted workspace state, network protocol, and simulation schemas remain unchanged.
- PM-002 [PC-002] visual samples: Authorized legacy screenshots/goldens are regenerated only when production token/layer changes invalidate them, retaining deterministic provenance and avoiding hand-edited output.

## Generated View Impact
- GV-001 [PD-010] lifecycle: analysis, work-model, evidence, verify, ship, summary, and agent guidance refresh from current authored sources; stale lifecycle views are never accepted as readiness.
- GV-002 [PD-010] review artifacts: deterministic visual/performance review output is generated from the exact built candidate and source identity; changed source or bundle identity invalidates reuse.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- The CLI-generated Performance Intent section is empty because this repository carries producer-owned performance authority in `docs/performance-budget.md`; DEC-002, FR-008, PC-003, and VO-005 bind that authority without creating a competing contract.
- The gameplay `Playable` harness is not the proof boundary for this presentation-only item. Production user reachability is proved through the existing real-entry browser journey surface; pure scene/component fixtures remain supporting evidence only.

## Lifecycle Notes
- Implementation order: typed projection/tokens → focused Client tests → SVG/CSS consumption → focused browser journey → performance/review generator → docs → one source-frozen aggregate → lifecycle evidence and feedback metadata.
- The clean pre-implementation Release smoke observed the inherited 200-unit scene at 6,942 nodes and 0.101 ms p95, 400-unit stress at 0.197 ms p95, dense overlay projection at 8.857 ms, and SVG export at 1.381/2.265 ms. These are baseline observations, not acceptance for the new workload.
