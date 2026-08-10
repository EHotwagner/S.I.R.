---
schemaVersion: 1
workId: 155-client-module-boundaries
title: Client Module Boundaries
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/155-client-module-boundaries/spec.md
sourceClarifications: work/155-client-module-boundaries/clarifications.md
sourceChecklist: work/155-client-module-boundaries/checklist.md
publicOrToolFacingImpact: true
---

# Client Module Boundaries Plan

Prose status: planned

## Source Snapshot
- spec: work/155-client-module-boundaries/spec.md sha256:67ec4bd04a4972a95014780647c7f0f1eab90bfa6bf7dbc79e078e11cde9f501 schemaVersion:1
- clarifications: work/155-client-module-boundaries/clarifications.md sha256:be3e529b1a41224909daff2f533a46085e658991b59fdf319c949c4effb26e2b schemaVersion:1
- checklist: work/155-client-module-boundaries/checklist.md sha256:18011c6aeaf35f7f461c6b9ade7cfa4d516e91146294dd86447cb08092e8bcf8 schemaVersion:1

## Plan Scope
- Extract a small browser-independent `AppBoundary` module from the Web root and retain the Web
  composition module as the only browser/bootstrap owner.
- Preserve the existing client public API and use the current deterministic client test executable as
  the regression route; this cut establishes a repeatable seam rather than attempting a risky
  all-at-once decomposition of App.fs and MapEditor.fs.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Introduce a typed runtime-neutral boundary for root model
  helpers and test it directly; App.fs delegates to that boundary and remains responsible only for
  browser-specific composition and interop.

## Contract Impact
- PC-001 [PD-001] F# module contract: `AppBoundary` exposes only pure values/functions needed by
  the root program and is compiled before `App.fs`; it cannot reference Browser, Fable DOM, or Web
  interop modules.

## Verification Obligations
- VO-001 [PD-001] [PC-001] semanticTest: Build the Web client and run the focused client test
  executable, including a regression that detects an App.fs bypass of the boundary; mutate the
  extracted subject once and record the expected red result.

## Performance Intent
Existing producer-owned performance intent applies: the `TacticalSceneProjectionQualification`
production `update` + `view` route uses 1,600 terrain cells, 200 units, 200 regions, and 200 routes
with p95 below 50 ms. This extraction changes no hot scene path; capture its existing qualification
as a baseline and do not invent a new timing target.

## Migration Posture
- PM-001 [PC-001] compatibility: Preserve compilation order and all existing module names; migrate
  only the selected pure helper through `AppBoundary` so downstream callers require no source change.

## Generated View Impact
- GV-001 [PD-001] generated guidance: Refresh the SDD work model, summary, and Codex/Claude views
  after evidence/verification so the committed lifecycle receipt remains source-current.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 155-client-module-boundaries`.
