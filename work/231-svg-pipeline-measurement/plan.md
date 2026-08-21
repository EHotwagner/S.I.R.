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
- spec: work/231-svg-pipeline-measurement/spec.md sha256:cb0a7ed85f5319f265c742444208c39e0a748d142b0e71aa2d6e0a3b1ce3121a schemaVersion:1
- clarifications: work/231-svg-pipeline-measurement/clarifications.md sha256:db02df55139fb63fac252b663ef9ddb1d62714871dd470fea8766890e01341ba schemaVersion:1
- checklist: work/231-svg-pipeline-measurement/checklist.md sha256:5656091f6c375563a52a26597511aca5e3c4d4efc34713d318830bb16d054c0b schemaVersion:1

## Plan Scope
- Add a focused Node/Playwright Chromium measurement command, versioned fixture definitions, deterministic schema validation/summarization, production-browser journey coverage, focused unit tests, and exact-candidate evidence documentation.
- Keep application source unchanged: fixtures enter through visible production import and all interactions use existing production controls.
- Ship no renderer optimization and no small-PR CI wiring.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: `scripts/measure-svg-pipeline.mjs` builds or consumes the Release publish, starts the production server, launches pinned Chromium through Playwright/CDP, imports generated maps through `#editor-map-import`, and drives the seven named journeys through visible controls.
- PD-002 [AC-001] [FR-002] [DEC-002] complete: CDP trace `EventDispatch`, renderer task, `Paint`, and `DrawFrame` slices plus DOM/layer snapshots emit separate typed observations; no frame callback is injected into the measured renderer and unavailable stages remain explicit.
- PD-003 [AC-001] [FR-003] complete: Schema-v1 artifacts bind git commit/tree, production-build identity, fixture/journey digests, browser/runtime/host capabilities, timestamps, and a manifest of gzip-compressed raw traces named by the SHA-256 of their decompressed bytes. The validator reads every retained byte from clean-archive paths and fails closed.
- PD-004 [AC-002] [FR-004] [DEC-001] complete: `scripts/svg-pipeline-fixtures.v1.json` uses one baseline and six one-factor variants, with a declared controlled pair for every workload axis; a separate composite global-scale variant serves the large-project/small-viewport comparison without weakening axis independence.
- PD-005 [AC-002] [FR-005] complete: The composite pair fixes a 480x320 CSS-pixel viewport, 40 visible/global units, overlay complexity, and event rate while increasing extent and supporting-list scale; the separate one-factor global-unit pair measures unit-count scaling, and summaries retain observed global and live-DOM-by-layer counters separately.
- PD-006 [AC-003] [FR-006] [DEC-003] complete: Pan/zoom/playback runs two warm-up and five stabilization cycles, recording heap/DOM checkpoints and collection capability.
- PD-007 [AC-004] [FR-007] complete: `scripts/lib/svg-pipeline-measurement.mjs` validates and deterministically ranks stage duration/share and structural deltas, preserving ties and unavailable observations.
- PD-008 [AC-004] [FR-008] [DEC-004] complete: Transport/typed-buffer/allocation dispositions use the versioned 20 percent material-share rule and emit the non-ceiling interpretation in every summary.
- PD-009 [AC-005] [FR-009] complete: `npm run measure:svg-pipeline -- --fixtures ... --journeys ... --out ...` is opt-in and absent from `scripts/ci-route.mjs` and ordinary PR workflow routing.
- PD-010 [AC-005] [FR-010] complete: Focused Node tests cover schema, six one-factor controls, trace-derived timing and sampler absence, receipt-to-authority binding, every retained raw byte, unavailable capability, ranking/disposition, and malformed/stale rejection; each repaired escape receives a subject-mutation demonstration.

## Contract Impact
- PC-001 [PD-001] [PD-003] commandArtifact: Add one opt-in npm command and schema-v1 JSON fixture/summary artifacts; schema changes require a new version and old artifacts remain readable or fail with an explicit unsupported-schema diagnostic.

## Verification Obligations
- VO-001 [PD-001] [PD-004] [PD-010] semanticTest: Focused tests validate fixture generation, inventory, schema, and fail-closed bindings, including subject mutations that turn each new gate red.
- VO-002 [PD-001] [PD-002] [PD-003] productionJourney: Run the exact Release candidate in production Chromium and commit the content-addressed compressed raw traces, manifest, and summary bindings; source-only, missing-byte, or headless DOM evidence does not satisfy.
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
