---
title: Persistent Tactical Workspace M7 Review Migration Evidence
category: Architecture
categoryindex: 1
index: 19
---

# Persistent Tactical Workspace M7 Review Migration Evidence

Milestone 7 moves Review into the persistent tactical workspace without
changing replay authority. Review renders only the accepted, bounded worker
projection for a committed tick. It cannot mutate replay history, reconstruct
omitted facts, or promote presentation interpolation to verification evidence.

## Projection and panel mapping

| Review capability | Persistent owner | Qualification |
|---|---|---|
| Committed units and edges | shared `units` and `edges` layers | opaque accepted-owner projection and browser singleton trace |
| Disclosed replay events | typed `review-event` annotations | selected events are retained only while visible in the accepted frame |
| Verification identity | typed `review-verification` annotation | full replay and perspective projection carry distinct labels and exact source/engine identity |
| Selection and camera | shared scene selection/camera | browser trace preserves the semantic unit across exact stepping and modality transitions |
| Disclosed roster | registered `roster` panel | only current bounded-projection units are listed |
| Package source and transport | registered `tools` panel | file input, event navigation, exact stepping/seeking, playback speed, checkpoints, and cancellation have one DOM owner |
| Disclosure summary | registered `layers` panel | counts and perspective hash describe only the current inspection projection |
| Event and unit inspection | registered `selection` panel | read-only selected facts and disclosed-event controls |
| Verification status | registered `validation` panel | browser-kernel and perspective claims retain their existing scope |
| Source/engine identity | registered `document` panel | exact replay metadata, kind, and committed extent |
| Worker state | registered `diagnostics` panel | protocol and bounded-batch status |

The replay branch now supplies no companion page content to the tactical
shell. The old `battlefieldView`, its inspector/sidecar renderer, duplicate SVG,
dashboard wrapper, and replay battlefield CSS were deleted after the panel and
shared-scene browser trace passed. Rules retains its separate non-battlefield
laboratory inspector until its supporting-panel milestone.

## Disclosure and verification evidence

`TacticalSceneProjection.acceptReview` is the only entry to Review projection.
A full replay is accepted only with `BrowserKernelVerified`, `VerifiedReplay`,
full-replay metadata, and no perspective hash. A perspective replay is accepted
only with `PerspectiveReady`, `PerspectivePlayback`, perspective metadata, a
perspective hash, zero hidden entities/events/checkpoints, and the bounded zero
board. The qualification constructs twelve otherwise-valid perspective owners
and injects exactly one fault into each: a unit, edge, event, checkpoint, each
of the four board bounds independently, a missing perspective hash,
replay-kind mismatch, mode mismatch, or verification mismatch. Every
individual owner must be rejected, so removing any one production guard fails
the matrix.

The accepted owner is opaque. It captures the bounded frame, stable replay
revision identity, selected visible identities, and verification identity.
Projection creates a disclosed verification annotation without granting the
browser claim authoritative .NET exact-artifact verification. Perspective
projection contains no entity/event state and drops invisible selections.

## Time, cancellation, and worker evidence

Committed frames remain read-only. Previous/next event, step, range seek, and
checkpoint controls issue bounded worker requests. Backward step seeks one
exact committed tick. Cancellation clears the active operation, stops playback,
and sends the worker cancel request. Presentation interpolation now runs
through `TacticalSceneProjection.interpolateReviewPresentation` before the
persistent SVG. It is accepted only for Review projections with a different
tick, equal board and disclosure, finite alpha, unique semantic unit
identities, and the exact same unit-identity set in both frames. It iterates
only current units and changes only their presentation row/column. The current
tick, revision, verification annotation, events, selection, disclosure, and
visual facts stay authoritative. Any guard failure returns the exact current
projection with effective alpha one, so a prior entity or perspective frame
cannot leak into the current scene. The SVG exposes this effective
presentation alpha solely as diagnostic presentation state; it is not
evidence.

Two evidence sources are deliberately separated:

- `smoke-client.mjs` uses a deterministic browser worker double to prove DOM
  ownership, singleton worksurface identity, controls, camera, selection, and
  shared-layer rendering; it is not cited as real-worker verification. Its
  replay worker handles `Advance`: the trace clicks Play, receives the next
  worker frame, observes a strictly intermediate unit position and alpha on
  the persistent SVG, observes exact convergence to the committed position,
  clicks Pause, and proves tick/position/alpha remain stable while the SVG,
  camera, and non-default semantic selection are retained. Exact Step remains
  a separate seek assertion.
- `smoke-worker-roundtrip.mjs` loads verified full and perspective `.sirr`
  fixtures into the actual compiled worker asset. It checks the full bounded
  projection, deterministic exact seeks, projection-only perspective output,
  disclosed perspective hash, and structured-clone correlation.

## Commands

```text
dotnet run --project tests/SIR.Client.Tests/SIR.Client.Tests.fsproj --no-restore
./scripts/build-client.sh
node scripts/test-review-workspace-m7-qualification.mjs
./fake.sh build -t Dev
./fake.sh build -t Test
./fake.sh build -t Verify
```

The M7 qualification fails closed on missing panel/projection/interpolation
guards, any missing member of the twelve-case perspective matrix, absent
worker-driven Play/intermediate/convergence/Pause coverage, any legacy replay
renderer/CSS token, duplicate browser ownership, or a failed real-worker
full/perspective replay round trip. The older detached `Battlefield`
interpolation regression remains compatibility coverage but is no longer the
M7 proof; M7 requires the shared Review projection regression and persistent
SVG browser observation.

Milestone 6 simulator projection and authority evidence is recorded in the
[Simulator migration evidence](persistent-tactical-workspace-m6-simulator-migration-evidence.md).
