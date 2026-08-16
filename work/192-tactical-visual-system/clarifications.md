---
schemaVersion: 1
workId: 192-tactical-visual-system
title: Tactical Visual System
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/192-tactical-visual-system/spec.md
publicOrToolFacingImpact: true
---

# Tactical Visual System Clarifications

## Source Specification
- work/192-tactical-visual-system/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: Which token surface owns production visual semantics?
- CQ-002 [AMB:AMB-002]: Which 100/200-unit budgets govern this cut?
- CQ-003 [AMB:AMB-003]: Which effects can current disclosed events support honestly?
- CQ-004 [AMB:AMB-004]: What is the reduced-motion equivalence contract?
- CQ-005 [AMB:AMB-005]: Which review artifact and measurement route establish acceptance?

## Answers
- CQ-001 → extend the existing `Battlefield`/`ReplayPalettes` projection contract with typed visual, motion, and effect tokens and emit CSS custom properties/data attributes at the root; geometry and protocol constants remain named module constants.
- CQ-002 → preserve the stronger inherited 200/400 pure-projection guards and add issue-specific production-shaped 100/200 workloads. At Standard zoom the 100-unit representative scene is capped at 5,000 estimated scene nodes and 128 active effects; the 200-unit stress scene is capped at 9,000 nodes and 256 active effects. Release p95 pure projection remains below 4/8 ms respectively, and a browser frame-sampling route must remain within a 16.67 ms frame-work budget without claiming compositor timing.
- CQ-003 → derive only movement, attack, impact/damage, suppression, heal/recovery, communication/sensor, objective, accepted/rejected, and historical emphasis from disclosed `RenderEventVisual` kind, source/target identity, summary, and tick. Unknown or undisclosed facts yield a generic bounded disclosed-event emphasis or no geometry, never invented recipients or outcomes.
- CQ-004 → reduced motion disables spatial interpolation and looping travel/pulse animation but retains a short opacity/outline emphasis plus persistent final state and accessible text. Exact-tick and replay seek continue to snap to committed frames.
- CQ-005 → use deterministic SVG/review metadata generated from the production client projection and a Playwright journey against the built Release bundle. Component timing is Release/headless with structural counters; the browser route samples requestAnimationFrame/main-thread work and explicitly makes no live-compositor or swapchain claim.

## Decisions
- DEC-001 [CQ-001] [AMB:AMB-001] [FR-001] [FR-002]: Extend the existing typed battlefield/palette contract as the single renderer-level registry and export it through root CSS variables and stable data attributes; component geometry constants stay named, not tokenized indiscriminately.
- DEC-002 [CQ-002] [AMB:AMB-002] [FR-008]: Qualify deterministic 100-unit representative and 200-unit stress production-shaped workloads at 5,000/9,000 estimated scene nodes, 128/256 active effects, 4/8 ms Release p95 pure projection, and 16.67 ms browser frame work, while retaining stronger inherited 200/400 guards.
- DEC-003 [CQ-003] [AMB:AMB-003] [FR-003] [FR-005] [FR-009]: Effects are stable, bounded projections of disclosed events only; unknown or incomplete events never synthesize hidden geometry, counts, timing, recipients, or outcomes.
- DEC-004 [CQ-004] [AMB:AMB-004] [FR-004] [FR-006]: Reduced motion snaps spatial presentation to authoritative frames and replaces travel/loop motion with short non-spatial emphasis and persistent state cues; it does not remove causal feedback.
- DEC-005 [CQ-005] [AMB:AMB-005] [FR-010]: Acceptance combines deterministic production-projection artifacts, focused component/browser journeys, one source-frozen aggregate receipt, and final hosted exact-SHA CI; headless timing is disclosed as non-compositor evidence.

## Accepted Deferrals
- None. The current item has no accepted requirement deferrals.

## Remaining Ambiguity
- None. AMB-001 through AMB-005 are resolved by DEC-001 through DEC-005.

## Lifecycle Notes
- The pre-implementation performance smoke on clean `origin/main` restored once and passed the Release client executable: dense projection p95 1.959/3.836/0.368/0.160 ms with overlay 8.857 ms; static 200-unit scene projection p95 0.101 ms at 6,942 estimated nodes; 400-unit stress p95 0.197 ms; deterministic SVG export p95 1.381/2.265 ms.
- Next lifecycle action: `fsgg-sdd checklist --work 192-tactical-visual-system`.
