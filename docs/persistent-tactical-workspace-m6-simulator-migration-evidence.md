---
title: Persistent Tactical Workspace M6 Simulator Migration Evidence
category: Architecture
categoryindex: 1
index: 18
---

# Persistent Tactical Workspace M6 Simulator Migration Evidence

Milestone 6 moves the disposable map simulator into the persistent tactical
workspace without changing its authority boundary. The simulator continues to
own a runtime copy of an immutable map revision. The shared scene owns only a
presentation projection, and the unified timeline cursor remains projection
state rather than runtime state.

## Projection and panel mapping

| Simulator capability | Persistent owner | Qualification |
|---|---|---|
| Runtime units and sub-cell movement | shared `units` layer through `PresentationColumn` and `PresentationRow` | .NET projection qualification mutates presentation coordinates independently of the immutable revision; browser smoke observes runtime/controller status on the retained unit primitive |
| Route preview and queued route | shared `routes` layer | browser smoke creates, resets, and cancels a route while asserting the legacy simulator stage is absent |
| Controller and movement state | shared unit status plus typed `simulator-state` annotations | .NET and browser checks cover manual/scripted state, moving state, movement intent, and planned-route state |
| Runtime events and disclosure | shared `annotations` layer with `SandboxDisclosure` | browser smoke requires the explicit scene disclosure; unavailable perspective and visibility remain stated rather than inferred |
| Runtime roster | registered `roster` panel | singleton accessible-name assertion |
| Run, pause, step, reset, and camera alternatives | registered `tools` panel | browser run/step/pause/reset/camera trace |
| Controller and route configuration | registered `selection` panel | native select/text input and registry-routed command trace |
| Runtime diagnostics and disclosure notices | registered `validation`/`diagnostics` panel body | singleton accessible-name assertion and latest-tick trace |
| Immutable revision and stale-draft state | registered `document` panel | immutable digest, runtime tick, and stale-editor wording |
| Curated simulation inputs | registered `samples` panel | the existing sample command boundary is retained |

The old simulator desktop chrome, command dock, owner-controls wrapper,
revision-status strip, map-stage styles, and tool-panel ownership state were
deleted after the production browser trace passed through the registered
panels. No simulator-specific SVG or battlefield root remains.

## Authority and time evidence

`MapEditorSimulator.tryHandoff` still copies `MapRevision.Document` into a
disposable runtime. Editor edits after handoff do not mutate that runtime; the
document panel reports when its immutable digest is behind the editor draft.
Run and step continue to update only the simulator owner. Reset reconstructs
the runtime solely from the existing handoff's pinned `Revision` and
`Revision.Document`; it never reads the mutable Editor. It restores tick zero,
paused state, original unit positions/controllers/scripts, and empty runtime
events, combat, checkpoints, recovery, movement, routes, and preview state. A
cancelled confirmation dispatches no reset and leaves the exact projected
simulator snapshot unchanged. The scene tick is read from
`SimulatorHandoff.Tick`.

The browser trace advances the runtime, invokes timeline Home, and proves that
the tactical cursor becomes zero while the scene/runtime tick remains
unchanged. The timeline declares
`projection-only-runtime-tick-unchanged`. Run subsequently continues from the
runtime tick, not from the scrubbed cursor.

The browser reset trace mutates runtime controller configuration, route state,
movement, events, and tick; cancels once and compares the complete observable
shared-scene snapshot; then edits the Editor behind the existing handoff. An
accepted reset restores the original pinned scene revision, map projection,
tick, controller defaults, positions, empty routes, and disclosures while the
document panel still reports that the separate Editor draft is newer. The
singleton SVG, camera, and semantic selection survive both reset paths. A pure
.NET regression additionally requires the reset record to equal the original
handoff baseline exactly.

## Worker correlation evidence

Two intentionally distinct boundaries remain visible:

- the interactive map sandbox executes the shared deterministic map kernel in
  `MapEditorSimulator`; and
- planning/session messages cross the retained browser worker through the
  public simulator-session protocol.

The UI does not claim that local map ticks were produced by the worker. The M6
qualification runs the actual compiled retained worker asset and preserves the
protocol guard before UI dispatch. It checks kind, protocol version, pending
operation, session, map revision, plan revision, and correlation tick. .NET
adversarial cases reject a foreign session, operation, map revision, plan
revision, correlation tick, kind, version, superseded operation, and completed
operation. The real-worker round trip additionally rejects stale session
requests and foreign cancellation, exercises step/run/reset, and bounds 6,000
ticks to 24 projection messages.

The browser planning double remains useful only for deterministic UI response
ordering tests in `smoke-client.mjs`; it is not cited as real-worker evidence.

## Commands

```text
dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj --no-restore
./scripts/build-client.sh
node scripts/test-simulator-workspace-m6-qualification.mjs
./fake.sh build -t Dev
./fake.sh build -t Test
./fake.sh build -t Verify
```

`test-simulator-workspace-m6-qualification.mjs` is fail closed. It inspects
production source for shared projection and registered-panel ownership,
rejects legacy simulator renderer/CSS tokens, imports the production browser
smoke, and then executes `smoke-worker-roundtrip.mjs` against the compiled
retained worker.
