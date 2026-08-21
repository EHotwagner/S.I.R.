---
schemaVersion: 1
workId: 231-svg-pipeline-measurement
title: Svg Pipeline Measurement
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/231-svg-pipeline-measurement/spec.md
publicOrToolFacingImpact: true
---

# Svg Pipeline Measurement Clarifications

## Source Specification
- work/231-svg-pipeline-measurement/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: Which production seam varies all fixture axes without creating a test-only renderer?
- CQ-002 [AMB:AMB-002]: Which Chromium sources separate application and browser-owned stages, and how are capability gaps represented?
- CQ-003 [AMB:AMB-003]: What bounded sequence defines memory warm-up and stabilization?
- CQ-004 [AMB:AMB-004]: How are optimization dispositions derived without turning a fixture into a project ceiling?

## Answers
- CQ-001 → Generate canonical `SIR-MAP 2` texts from versioned fixture definitions, import them through the visible production map picker, and drive the real workspace controls. Fixture metadata declares the independent axes; DOM attributes and the imported map report observed global/visible counters.
- CQ-002 → Use Chromium DevTools trace slices for renderer tasks, `EventDispatch`, `Paint`, and `DrawFrame` timing, plus DOM/layer and heap snapshots. Do not inject a frame sampler into the measured page. Every metric carries `available`, `value`, and a bounded reason when unavailable; compressed raw traces are retained by content digest.
- CQ-003 → Run one unrecorded setup, two warm-up pan/zoom/playback cycles, capture the warm checkpoint, then five identical stabilization cycles and capture the stabilized checkpoint. Attempt collection only through an explicitly available Chromium capability and disclose whether it ran.
- CQ-004 → Rank observed stage duration/share and structural deltas. Mark an optimization required only when its owning stage is the measured top bottleneck or exceeds the declared material-share threshold; otherwise defer it. Always state that workload ids are regression fixtures, not support ceilings.

## Decisions
- **DEC-001** [CQ-001] [AMB:AMB-001] [FR-001] [FR-004] [FR-005]: Versioned JSON fixture definitions generate maps that enter through the production import control; journeys use only production pointer, keyboard, menu, timeline, and workspace controls.
- **DEC-002** [CQ-002] [AMB:AMB-002] [FR-002] [FR-003]: A schema-v1 summary plus clean-archive-durable content-addressed raw Chromium traces is authoritative; trace-derived frame/input timing introduces no renderer sampling callback, and unavailable browser capabilities remain explicit unavailable observations rather than zero-valued successes.
- **DEC-003** [CQ-003] [AMB:AMB-003] [FR-006]: Memory uses two warm-up and five stabilization cycles, with identical pan/zoom/playback actions and separately labelled heap/DOM checkpoints.
- **DEC-004** [CQ-004] [AMB:AMB-004] [FR-007] [FR-008]: The summary ranks measured stages and applies a versioned 20 percent material-share threshold for transport/allocation disposition; ties and missing data remain inconclusive/deferred, and no size ceiling is emitted.

## Accepted Deferrals
- None. Renderer optimization and protected-host governance are outside this item's scope, not unresolved requirements of this harness.

## Remaining Ambiguity
- None. All four specification ambiguities are resolved by DEC-001 through DEC-004.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 231-svg-pipeline-measurement`.
