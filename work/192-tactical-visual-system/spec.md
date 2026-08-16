---
schemaVersion: 1
workId: 192-tactical-visual-system
title: Tactical Visual System
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Tactical Visual System Specification

Prose status: specified

## User Value
A player can read exact tactical state immediately in a battlefield that feels like one authored grounded near-future and arcane-fantasy game across Editor, Plan, Simulate, Review, Rules/Data, and Docs.

## Scope
- SB-001: Shared tactical workscreen rendering, projection-safe motion and effects, renderer tokens, supporting panels, documentation, deterministic fixtures, browser journeys, and measured 100/200-unit production scenes.
- SB-002: Authoritative simulation, disclosure, replay, overlay geometry, event identity, and committed timing remain inputs and are never inferred by the visual layer.
- SB-003: Legacy visual samples may be updated to the production vocabulary; new bitmap assets are not required when deterministic vector/token primitives express the design.

## Non-Goals
- Replacing authoritative box-shaped pieces with character or vehicle models.
- Defining combat, spatial, perception, disclosure, or exact-overlay truth in rendering.
- Cinematic camera motion, ambient particle noise, bloom, or translucency that obscures exact geometry.
- Treating a headless timing sample as live-compositor or swapchain evidence.

## User Stories
- US-001 (P1): As a commander, I can scan unit faction, role, footprint, facing, selection, condition, intent, and decisive effects without opening an inspector.
- US-002 (P1): As a replay reviewer, I can pause, step, scrub, and change speed without stale or detached presentation implying an uncommitted state.
- US-003 (P1): As a player using reduced motion, high contrast, a configurable palette, a narrow viewport, or 400% zoom, I receive the same causal and tactical information through usable channels.
- US-004 (P1): As a player commanding large forces, I can use representative 100-unit and stress 200-unit scenes without unbounded DOM/SVG, effect, layout, or projection work.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001] [FR-002]: Given an ordinary or dense tactical scene at normal zoom, when it renders, then faction, role, exact footprint, facing, selection/focus, health/condition, immediate intent, and decisive effects are identifiable without an inspector and ordinary/dense scenes share one visual grammar.
- AC-002 [US-001] [FR-003]: Given selected exact overlays and active effects, when they compose, then deterministic layer order and bounded collision preserve exact geometry, labels, unit identity, footprint, and interaction affordances.
- AC-003 [US-002] [FR-004] [FR-005]: Given committed frames and replay controls, when movement, attacks, impacts, suppression, state changes, pause, step, scrub, or speed changes occur, then presentation communicates cause and consequence, converges deterministically, and leaves no stale effect or impossible interpolated position.
- AC-004 [US-003] [FR-006] [FR-007]: Given reduced motion, forced/high contrast, configurable palette, narrow width, or an effective 400% zoom, when the same workflows run, then causal feedback and tactical truth remain perceivable and the controls remain operable.
- AC-005 [US-004] [FR-008]: Given deterministic 100-unit representative and 200-unit stress fixtures with routes, statuses, attacks, and selected overlays, when the production update/view route is measured in Release, then it stays within the declared node, active-effect, projection, layout, memory-proxy, and input-response budgets.
- AC-006 [US-001] [FR-009]: Given a hidden, undisclosed, absent, or explicitly unknown fact, when the visual system projects it, then no geometry, label, count, timing, motion, effect, or outcome leaks that fact.
- AC-007 [US-001] [US-004] [FR-010]: Given the production bundle and deterministic review fixtures, when visual-review qualification runs, then before/after artifacts are bundle-bound and regressions in critical contrast, geometry, hierarchy, density, or effect order fail the owning gate.

## Functional Requirements
- FR-001: The renderer MUST centralize production palette, material, elevation, typography, icon, border, state-emphasis, semantic-zoom, motion, and effect tokens instead of scattering new component-local literals. (covers AC-001)
- FR-002: Unit and terrain presentation MUST preserve the authoritative box-piece language and expose faction, role, exact footprint, facing, selection/focus, disclosed health/condition, and immediate intent at normal tactical zoom with additional detail only at semantic zoom. (covers AC-001)
- FR-003: Terrain, edges, zones, routes, annotations, units, labels, exact overlays, and effects MUST use deterministic z-order and bounded label/effect collision so effects never conceal identity, footprint, current state, exact geometry, or interaction affordances. (covers AC-002)
- FR-004: Presentation motion MUST be projection-only, interpolate only safe disclosed continuity, visibly and deterministically converge to committed frames, and never enter authoritative state, replay fingerprints, collision, or disclosure decisions. (covers AC-003)
- FR-005: Effect instances MUST derive only from disclosed authoritative event identities, use bounded lifetimes and stable keys, distinguish preview, predicted, accepted, committed, rejected, and historical states, and reset coherently across pause, step, scrub, and speed changes. (covers AC-003)
- FR-006: Reduced-motion behavior MUST preserve causal and state-change feedback through short non-spatial emphasis and persistent state cues rather than removing feedback, and MUST pass the same functional journeys. (covers AC-004)
- FR-007: Critical state MUST remain perceivable through configurable/high-contrast palettes plus shape, hierarchy, text, or motion channels, and the tactical workspace MUST remain usable at narrow widths and effective 400% browser zoom. (covers AC-004)
- FR-008: The production update/view workload MUST qualify deterministic 100-unit representative and 200-unit stress scenes with overlapping routes, statuses, attacks, selected overlays, and declared fail-closed structural/timing budgets whose fingerprints exclude elapsed time. (covers AC-005)
- FR-009: The visual system MUST preserve disclosure boundaries and MUST NOT create hidden entity geometry, event timing, labels, counts, particles, interpolation, or outcome cues. (covers AC-006)
- FR-010: Deterministic component fixtures and production browser journeys MUST cover the visual vocabulary, motion/effect lifecycle, overlays, replay controls, responsive layouts, reduced motion, accessibility, and performance, with a bundle-bound visual review artifact and subject-mutation proof for every added or modified gate. (covers AC-007)

## Ambiguities
- AMB-001: Which existing visual token surface is the single renderer registry, and which literals may remain as geometry or protocol constants?
- AMB-002: What explicit 100/200-unit structural and timing budgets are both representative and compatible with the repository's existing 200/400-unit projection guards?
- AMB-003: Which visual effects can be derived honestly from current disclosed event kinds without inventing unavailable combat state?
- AMB-004: How should reduced motion preserve causality while preventing spatial interpolation and repeated animation?
- AMB-005: Which artifact route is production-bundle-bound and deterministic enough for before/after visual review without pretending headless evidence is compositor evidence?

## Public Or Tool-Facing Impact
- Extends the typed battlefield scene/view contract consumed by Fable rendering and deterministic evidence export.
- Extends production DOM/SVG data attributes, CSS token contracts, performance receipts, and browser-visible fixtures.
- Updates living visual, workspace, and performance documentation; no simulation or network protocol schema changes.

## Lifecycle Notes
- Performance-first gate: baseline the current deterministic 100/200-unit production projection and rendering route before implementation; declare budgets in the plan and preserve workload-definition identity.
- Verification must use focused tests while editing, one source-frozen aggregate, metadata-only feedback validation without rebuilding, and one final hosted exact-SHA CI acceptance.
