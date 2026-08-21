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
- spec: work/231-svg-pipeline-measurement/spec.md sha256:9fac2ce1b91df41eaa5c0a11c0ea34d8b6d5796244f14cce338538cf1ec34946 schemaVersion:1
- clarifications: work/231-svg-pipeline-measurement/clarifications.md sha256:b801aef28cf0a56a399bb9a24de4ceb99aec6b463e29525c9fd8f4abdb68f499 schemaVersion:1
- checklist: work/231-svg-pipeline-measurement/checklist.md sha256:2d9d59a90a9f6db97c86ff28493c06d67d7513b9983ba871c0fbb9f63cf0db4e schemaVersion:1

## Plan Scope
- Add a focused Node/Playwright Chromium measurement command, versioned fixture definitions, deterministic schema validation/summarization, production-browser journey coverage, focused unit tests, and exact-candidate evidence documentation.
- Keep application source unchanged: fixtures enter through visible production import and all interactions use existing production controls.
- Ship no renderer optimization and no small-PR CI wiring.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: `scripts/measure-svg-pipeline.mjs` builds or consumes the Release publish, starts the production server, launches pinned Chromium through Playwright/CDP, imports generated maps through `#editor-map-import`, and drives the seven named journeys through visible controls.
- PD-002 [AC-001] [FR-002] [DEC-002] complete: CDP trace `EventDispatch`, renderer task, `Paint`, and `DrawFrame` slices plus DOM/layer snapshots emit separate typed observations; no frame callback is injected into the measured renderer and unavailable stages remain explicit.
- PD-003 [AC-001] [FR-003] complete: Schema-v1 artifacts bind git commit/tree, production-build identity, fixture/journey digests, browser/runtime/host capabilities, timestamps, and a manifest of gzip-compressed raw traces named by the SHA-256 of their decompressed bytes. The validator reads every retained byte from clean-archive paths and fails closed.
- PD-004 [AC-002] [FR-004] [DEC-001] complete: `scripts/svg-pipeline-fixtures.v1.json` uses one baseline and six one-factor variants, with a declared controlled pair for every workload axis; every generated unit cell is unique, camera setup uses production controls, viewport intersection reports observed visible glyphs, and production-summary validation refuses a visible-density pair that does not change that count or a global-unit pair that changes it.
- PD-005 [AC-002] [FR-005] complete: The composite pair fixes a 480x320 CSS-pixel viewport, 40 viewport-intersecting units, overlay complexity, and event rate while increasing extent and supporting-list scale; the separate one-factor global-unit pair measures unit-count scaling while holding the observed visible count constant, and summaries retain imported-global, viewport-visible, and live-DOM-by-layer counters separately.
- PD-006 [AC-003] [FR-006] [DEC-003] complete: Pan/zoom/playback runs two warm-up and five stabilization cycles, recording heap/DOM checkpoints and collection capability.
- PD-007 [AC-004] [FR-007] complete: `scripts/lib/svg-pipeline-measurement.mjs` validates and deterministically ranks stage duration/share and structural deltas, preserving ties and unavailable observations.
- PD-008 [AC-004] [FR-008] [DEC-004] complete: Transport/typed-buffer/allocation dispositions use the versioned 20 percent material-share rule and emit the non-ceiling interpretation in every summary.
- PD-009 [AC-005] [FR-009] complete: `node scripts/measure-svg-pipeline.mjs --fixtures ... --journeys ... --out ...` remains opt-in. Changes confined to the five SVG-specific harness files plus documentation take the fixed-function documentation/evidence route; its hosted owner runs the focused retained-evidence validator but never regenerates the expensive Chromium matrix. A routed `work/<id>/hosted-verification.sh` subject is required to be a regular, readable, executable file, its exit status propagates, and exact missing/non-executable mutations fail closed. Broad route, runner, test-policy, and workflow files remain cross-cutting. Both performance and cross-cutting routes enforce the 240-second target and 60-second headroom, and workflow assertions/mutants require the performance route's web/docs producers. The web producer transports the exact lockfile-installed Playwright runtime as receipt- and manifest-bound outputs, so browser consumers reuse those bytes instead of repeating a full dependency install on the critical path; removal of any runtime output or restoration of a consumer install fails the route contract.
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
