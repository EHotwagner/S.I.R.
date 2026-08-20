---
schemaVersion: 1
workId: 231-svg-pipeline-measurement
title: Svg Pipeline Measurement
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/231-svg-pipeline-measurement/spec.md
sourceClarifications: work/231-svg-pipeline-measurement/clarifications.md
sourceChecklist: work/231-svg-pipeline-measurement/checklist.md
publicOrToolFacingImpact: true
---

# SVG Pipeline Measurement Plan

Prose status: planned

## Source Snapshot
- spec: work/231-svg-pipeline-measurement/spec.md sha256:1a7593c4705d8e90f805e0f1edfc2475e7f9ef7e84e2cdcfbda63822798157c8 schemaVersion:1
- clarifications: work/231-svg-pipeline-measurement/clarifications.md sha256:9cecc8c95ec0b9d6e70f44ea798657b0a1616246883b85e81a5076750807a427 schemaVersion:1
- checklist: work/231-svg-pipeline-measurement/checklist.md sha256:dc48ebb81e12b9ea7cc06a81da91f2a0d5bf982b56e4c27fc2ede596ab81de93 schemaVersion:1

## Plan Scope
- Add a focused Node/Playwright Chromium measurement command, versioned fixture definitions, deterministic schema validation/summarization, production-browser journey coverage, focused unit tests, and exact-candidate evidence documentation.
- Keep application source unchanged: fixtures enter through visible production import and all interactions use existing production controls.
- Ship no renderer optimization and no small-PR CI wiring.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: `scripts/measure-svg-pipeline.mjs` builds or consumes the Release publish, starts the production server, launches pinned Chromium through Playwright/CDP, imports generated maps through `#editor-map-import`, and drives the seven named journeys through visible controls.
- PD-002 [AC-001] [FR-002] [DEC-002] complete: CDP tracing plus in-page observers and DOM/layer snapshots emit separate stage observations; every observation is a typed available/unavailable value with no aggregate fallback masquerading as separation.
- PD-003 [AC-001] [FR-003] complete: Schema-v1 artifacts bind git commit/tree, production-build receipt when present, fixture/journey digests, browser/runtime/host capabilities, raw trace digest, timestamps, and result.
- PD-004 [AC-002] [FR-004] [DEC-001] complete: `scripts/svg-pipeline-fixtures.v1.json` declares all six axes and generates canonical maps; the baseline includes representative, dense-overlay, and global-small/global-large paired fixtures.
- PD-005 [AC-002] [FR-005] complete: The pair fixes a 480x320 CSS-pixel viewport and visible density while increasing global extent from 20x20 to the production-valid 79x79 maximum, units, and supporting-list scale; summaries retain observed global and live-DOM-by-layer counters separately.
- PD-006 [AC-003] [FR-006] [DEC-003] complete: Pan/zoom/playback runs two warm-up and five stabilization cycles, recording heap/DOM checkpoints and collection capability.
- PD-007 [AC-004] [FR-007] complete: `scripts/lib/svg-pipeline-measurement.mjs` validates and deterministically ranks stage duration/share and structural deltas, preserving ties and unavailable observations.
- PD-008 [AC-004] [FR-008] [DEC-004] complete: Transport/typed-buffer/allocation dispositions use the versioned 20 percent material-share rule and emit the non-ceiling interpretation in every summary.
- PD-009 [AC-005] [FR-009] complete: `npm run measure:svg-pipeline -- --fixtures ... --journeys ... --out ...` is opt-in and absent from `scripts/ci-route.mjs` and ordinary PR workflow routing.
- PD-010 [AC-005] [FR-010] complete: Focused Node tests cover schema, axis independence, route inventory, digest/candidate binding, unavailable capability, ranking/disposition, and malformed/stale rejection; each new gate receives a subject-mutation demonstration.

## Contract Impact
- PC-001 [PD-001] [PD-003] commandArtifact: Add one opt-in npm command and schema-v1 JSON fixture/summary artifacts; schema changes require a new version and old artifacts remain readable or fail with an explicit unsupported-schema diagnostic.

## Verification Obligations
- VO-001 [PD-001] [PD-004] [PD-010] semanticTest: Focused tests validate fixture generation, inventory, schema, and fail-closed bindings, including subject mutations that turn each new gate red.
- VO-002 [PD-001] [PD-002] [PD-003] productionJourney: Run the exact Release candidate in production Chromium and retain its raw trace plus summary; source-only or headless DOM emulation does not satisfy.
- VO-003 [PD-005] [PD-006] performanceEvidence: Compare the paired global-scale fixtures and capture warm/stable memory for repeated pan/zoom/playback with capability facts.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additive: The new schema-v1 fixture and summary contracts are additive; no existing artifact or command is migrated.

## Generated View Impact
- GV-001 [PD-003] evidenceArtifact: Measurement outputs are generated under the caller-selected artifact directory and never hand-edited; committed SDD readiness views refresh from authored sources and exact observed runs.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 231-svg-pipeline-measurement`.
