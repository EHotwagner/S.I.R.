---
title: Persistent Tactical Workspace M5 Planner Migration Evidence
category: Tools & Evidence
categoryindex: 5
index: 17
status: accepted
decision-status: implemented
last-updated: 2026-07-31
---

# Persistent Tactical Workspace M5 Planner Migration Evidence

Milestone 5 moves Plan presentation onto the retained
`svg#persistent-tactical-svg` and the registered Field Focus panels. The
`PlanningWorkspaceState` remains the authored authority and the simulator worker
remains the validation, prediction, and commit boundary.

## Projection and control mapping

| Plan capability | Persistent owner |
|---|---|
| Authored routes | `#persistent-layer-routes [data-route-kind=planned]` |
| Intent-only prediction disclosure | `#persistent-layer-annotations [data-annotation-kind=prediction]`; no entity geometry |
| Facing and attention | shared unit heading primitives plus typed annotations |
| Stance, hold, engagement, synchronization | shared unit state plus typed annotations |
| Validation diagnostics and prediction disclosure | `#persistent-layer-annotations` |
| Roster | registered `roster` panel |
| Plan tools and Preview → Validate → Commit | registered `tools` panel |
| Selected unit and authored-command inspector | registered `selection` panel |
| Validation navigation | registered `validation` panel |
| Authored, predicted, accepted, committed revision state and review export | registered `document` panel |
| Authored/predicted/accepted/committed intervals | shared bottom timeline |

All surface, panel, and timeline actions resolve through the tactical command
registry and authoritative availability checks. The shared SVG cell intent is
the only battlefield route-authoring pointer path.

## Live parity and removal proof

`scripts/test-planning-workspace-m5-qualification.mjs` runs the production
browser smoke and fails closed unless it proves:

- strict SVG and stable layer reference identity throughout every modality,
  panel, playback, and planning update;
- singleton registered roster, tools, inspector, validation, and revision
  owners with no legacy planner page or alternate battlefield;
- live authored-route, facing, attention, stance, hold, engagement,
  synchronization, annotation-only intent prediction, and validation projection
  through shared layers;
- exact authored revision and route geometry restoration across undo/redo;
- rejection of wrong operation, correlation tick, response class, envelope
  kind/version, no-pending responses, and a superseded same-revision preview;
- enforced Preview → Validate → Commit ordering, non-acceptance while a live
  validation annotation exists, authored-document change and re-preview before
  revalidation, and worker-correlated acceptance/commit;
- immutable committed intervals across shared-cell, inspector, undo, redo, and
  removal attempts;
- shared camera, semantic selection, timeline, keyboard help, workers,
  accessibility, Editor parity, Rules, and Samples continuity; and
- source and CSS absence of the planner battlefield, grid, page layout,
  duplicate timeline lanes, and their responsive exceptions.

The current real worker's `IntentOnlyPreview` contract returns the canonical
authored intent disclosures and an empty entity/event delta. Plan therefore
projects prediction text only and deliberately creates no synthetic movement
route. The real worker round-trip gate proves empty Units, Edges, Events, and
Checkpoints while matching every disclosed line to an authored intent. Every
response must match the one pending request's operation, session, map revision,
plan revision, and correlation tick, plus envelope kind/version and the expected
response class, before it can update Plan.

The production-DOM browser test uses a deterministic worker double to force a
diagnostic on one exact authored revision, then changes that authored document
before re-previewing and accepting the next revision. This synthetic diagnostic
is interaction evidence only; it is not claimed as a real rules diagnostic.
The separate real-worker round-trip gate is the authority for protocol,
structured-clone, disclosure, and empty intent-only entity-delta behavior.

## Qualification commands

```text
npm run build:client
npm run review:map-editor
npm run test:planning-m5-qualification
npm run test:persistent-workspace-m0
node scripts/smoke-worker-roundtrip.mjs
./fake.sh build -t Dev
./fake.sh build -t Test
./fake.sh build -t Verify
```
