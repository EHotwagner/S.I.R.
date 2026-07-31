---
title: Persistent Tactical Workspace Milestone 2 Evidence
category: Engineering
categoryindex: 6
index: 14
status: accepted
decision-status: implementation-evidence
document-type: test-evidence
version: "1.0"
last-updated: 2026-07-31
description: Shared presentation projection contracts, modality adapters, disclosure boundaries, semantic identity, and performance evidence for corrective Milestone 2.
related:
  - docs/2026-07-31-0840-vscode-style-persistent-tactical-workspace-design-report.md
  - docs/persistent-tactical-workspace-m1-shell-layout-evidence.md
---

# Persistent Tactical Workspace Milestone 2 Evidence

Milestone 2 introduces the presentation-only data boundary that the persistent
SVG renderer will consume in Milestone 3. It does not mount or replace a
renderer. Existing modality render paths therefore remain intact while the
shared contract is qualified independently.

## Public scene contract

`TacticalSceneProjection.fsi` publishes projections for terrain, units, routes,
annotations, disclosure, camera, selection, and layers, plus retained edge and
board visuals required by the next renderer milestone. `SharedSceneProjection`
contains only presentation values and a `SceneProjectionOwner`; it contains no
domain command, update function, worker handle, replay package, editor history,
or simulator authority.

`ScenePrimitiveId` is private outside its defining module. Consumers can read
its stable value but cannot construct an arbitrary identity through the public
API. IDs use semantic namespaces:

| Primitive | Stable identity |
| --- | --- |
| terrain cell | `terrain:{column}:{row}` |
| unit | `unit:{unitId}` |
| edge | `edge:{sourceEdgeId}` |
| planned route | `route:{commandId}` |
| simulator route | `route:simulator:{ownerUnitId}:{preview-or-planned}` |
| review overlay | `route:{acceptedOverlayId}` |
| region or event annotation | owner-qualified region/event occurrence identity |
| layer | `layer:{semanticLayerKind}` |

Camera, focus, selection, and disclosure changes do not alter drawable
primitive IDs. Simulator route identity deliberately models preview and planned
as separate semantic slots owned by a unit: changing the route geometry retains
the slot, switching the owning unit does not reuse it. Simulator event IDs use
the frame's tick-qualified occurrence identity, so identical summaries at
different ticks cannot collide. Focused qualification rejects duplicate IDs
within representative scenes for all four modes.

## Pure modality adapters

| Adapter | Owned source | Projection behavior |
| --- | --- | --- |
| Editor | `MapEditorState` and `EditorWorkspaceState` | Projects authored terrain, edges, units, regions/events, authoring layers, camera, and visible selection with sandbox disclosure. |
| Plan | map revision plus `PlanningWorkspaceState` | Projects battlefield terrain/edges and roster positions; route commands become routes, remaining commands become annotations, and runtime-only health/headings remain `NotPresent`. |
| Simulate | immutable `SimulatorHandoff` | Projects the runtime map/frame, route preview or queued route, simulator events, camera, and visible selection. |
| Review | opaque `AcceptedReviewProjection` created from `Shell.Model` | Accepts only a verified full-replay owner or the bounded perspective owner, copies its disclosed frame, and drops selected/focused IDs absent from that frame. |

The public Review input cannot be constructed from a raw `RenderFrame`.
`acceptReview` brands a `Shell.Model` only when source kind, run mode,
verification, and inspection provenance agree. Perspective acceptance also
requires a perspective hash and the worker-owned bounded shape: no units,
edges, events, or checkpoints, and only the worker's zero-extent placeholder
board. An adversarial model that merely relabels a full entity projection as
perspective is rejected. Qualification proves both
entity-level exclusion and field-level non-expansion: accepted full replay
units retain unavailable level and stance fields as `NotPresent`.

Selection is semantic and mode-complete. Editor projects all visible selected
units plus a visible selected region; Plan projects a visible selected unit
and resolves the selected command to either its route or annotation primitive;
Simulate projects its visible selected unit; Review projects visible accepted
unit and event selections. Every adapter rejects stale selection IDs, and
`SelectedPrimitiveIds` contains only the corresponding visible semantic
primitives.

## Mutation and ownership evidence

Focused tests retain adapter inputs before projection and prove structural
equality afterward. They also mutate projected route coordinates, proving
output arrays do not alias planning commands or simulator state. A Review test
mutates the returned unit array, repeats the projection, and proves both the
accepted owner and the later projection remain unchanged.

Planning qualification proves runtime-only health and headings remain absent.
Review qualification rejects a forged perspective owner containing a unit and
event, accepts an entity-free bounded perspective owner, and accepts a verified
full replay without synthesizing absent level or stance fields. Invisible unit,
event, command, and region selections are removed.

Primitive identity is qualified independently for Editor, Plan, Simulate, and
Review across camera, cursor/tick, focus, and selection changes where the
semantic object is unchanged. Simulator qualification additionally proves
distinct route owners and repeated event summaries at distinct ticks cannot
collide. IDs remain unique within every representative scene. Simulator revision identity is the
accepted map revision digest rather than a frame tick; Review revision identity
is the accepted source plus engine identity, stable across frames from one
source and distinct across different sources.

No adapter invokes an Elmish update, domain command, worker request, replay
decode, or persistence write. The functions return one new projection and no
effect value.

## Performance and allocation budget

`TacticalSceneProjectionQualification` uses representative dense inputs: a
40 × 40 map with 1,600 terrain cells, 200 units, 200 regions, and 200 planned
routes for Editor and Plan; a 200-unit simulator handoff; and an accepted
200-unit/200-event Review projection. After warm-up it measures 80 projections
for each adapter.

The accepted guardrails are:

- each of Editor, Plan, Simulate, and Review p95 below 50 ms; and
- one projection in each mode below 3,000,000 allocated bytes.

The focused qualification recorded Editor/Plan/Simulate/Review p95 of
4.753/2.452/0.968/0.420 ms and allocations of
1,913,312/1,834,992/617,208/226,376 bytes. These figures are synthetic local
qualification evidence, not production telemetry; the executable guardrails
remain authoritative across environments.

## Validation record

- focused `.NET` scene-projection and existing client qualifications: passed;
- public `.fsi`/`.fs` contract compilation: passed;
- stable/unique identity and array-isolation qualification: passed;
- dense projection timing/allocation qualification: passed;
- production Fable and browser qualification: passed;
- documentation build, API generation, accessibility, and browser
  qualification: passed;
- `git diff --check`: passed;
- sequential `./fake.sh build -t Dev`, `Test`, and `Verify`: passed.
