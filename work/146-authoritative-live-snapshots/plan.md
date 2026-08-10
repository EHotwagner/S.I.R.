---
schemaVersion: 1
workId: 146-authoritative-live-snapshots
title: Authoritative Live Snapshots
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/146-authoritative-live-snapshots/spec.md
sourceClarifications: work/146-authoritative-live-snapshots/clarifications.md
sourceChecklist: work/146-authoritative-live-snapshots/checklist.md
publicOrToolFacingImpact: true
---

# Authoritative Live Snapshots Plan

Prose status: planned

## Source Snapshot
- spec: work/146-authoritative-live-snapshots/spec.md sha256:56b3a112e86af462d03eca5fe7487296cb2282ab5f1d63b32a76dc658ba53614 schemaVersion:1
- clarifications: work/146-authoritative-live-snapshots/clarifications.md sha256:e327637ace906ea66e39f17815a7e6a33cf022fb28d5f78480b255984798bd7f schemaVersion:1
- checklist: work/146-authoritative-live-snapshots/checklist.md sha256:e2ca0a4a8923c0e979674e8d8249459cff2e1b15b185d3e0535a894a89678582 schemaVersion:1

## Plan Scope
- Move the existing SignalR slice behind Elmish effects and project accepted authority through the shared persistent tactical renderer.
- Preserve server sequence/resync semantics while removing the parallel DOM/session implementation.
- Extend browser qualification only through player-emittable visible controls and refresh client-review artifacts if a bundle changes.

## Plan Decisions
- PD-001 [FR-001] [DEC-002] complete: Define live-session Model/Msg/effect ownership in `src/SIR.Client.Web`; decode events at the effect edge and reduce accepted authority into the shared tactical scene input.
- PD-002 [FR-002] [DEC-001] complete: Bind player-visible live controls to Elmish messages and prove advance/disconnect/reconnect/resync from the real browser entry point against the running server.
- PD-003 [FR-003] [DEC-002] complete: Surface connection, decode, and resync outcomes in the normal diagnostic UI using knowledge-scoped deterministic text.
- PD-004 [FR-004] [DEC-003] complete: Delete the separate fixed-position DOM mount, mutable module lifecycle, and window test hooks after consumers use the Elmish route.
- PD-005 [AC-001] [AC-002] [AC-003] complete: Regenerate and validate both map-editor and persistent-workspace-M9 review bindings when the Fable bundle changes.

## Contract Impact
- PC-001 [PD-001] public client MVU surface: Live connection state, snapshot event, command intent, and diagnostic state are owned by the application Model/Msg/effect boundary rather than globals.

## Verification Obligations
- VO-001 [PD-002] [PC-001] playerJourney: Run a bot-driven headless browser journey through product entry and visible controls; direct dispatch and globals are invalid evidence.
- VO-002 [PD-005] structuralReview: Run exact map-editor and persistent-workspace-M9 regeneration/validation gates for any changed client bundle.
- VO-003 [PD-001] [PD-003] regression: Add focused client/server/browser coverage and demonstrate the new regression gate reds when its live subject is mutated.

## Performance Intent
No producer-owned typed performance intent or target FPS is declared for this work item. The implementation will retain existing structural counters and report the absence rather than inventing a timing target; the required player journey remains production-route evidence, not a performance substitute.

## Migration Posture
- PM-001 [PC-001] complete: The former global live slice is removed in the same change; no dual routing or silent fallback remains.

## Generated View Impact
- GV-001 [PD-005] bundleReview: Client bundle changes require regenerated map-editor and persistent-workspace-M9 review bindings and their exact validation commands.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 146-authoritative-live-snapshots`.
