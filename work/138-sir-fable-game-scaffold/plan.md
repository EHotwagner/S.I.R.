---
schemaVersion: 1
workId: 138-sir-fable-game-scaffold
title: Incorporate S.I.R. into the fs-gg-fable-game scaffold
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/138-sir-fable-game-scaffold/spec.md
sourceClarifications: work/138-sir-fable-game-scaffold/clarifications.md
sourceChecklist: work/138-sir-fable-game-scaffold/checklist.md
publicOrToolFacingImpact: true
---

# Incorporate S.I.R. into the fs-gg-fable-game scaffold Plan

Prose status: planned

## Source Snapshot
- spec: work/138-sir-fable-game-scaffold/spec.md sha256:4611c424ccb2c68a9986016a944c74348c733a7c90e404a88501d6c3d374fe99 schemaVersion:1
- clarifications: work/138-sir-fable-game-scaffold/clarifications.md sha256:c1b19c71457a578485e01d457186870ded06d4e67f825cbd1ac3c7c33d9be0df schemaVersion:1
- checklist: work/138-sir-fable-game-scaffold/checklist.md sha256:e1b92f850da930f9423e8d5ebe3d5309ad1aa79b2e9e30a13fffe785611b1fbe schemaVersion:1

## Plan Scope
- Work item 138-sir-fable-game-scaffold is planned from the current specification, clarification, and checklist facts.
- Requirement count: 6.
- Clarification decision count: 0.
- Checklist result count: 6.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Preserve the existing `src/SIR.Domain`, `src/SIR.Simulation`, `src/SIR.Match`, `src/SIR.Client`, and `src/SIR.Client.Web` projects in place; add an integration report mapping every published scaffold lane to the retained or new S.I.R. lane.
- PD-002 [AC-001] [FR-002] complete: Add `src/SIR.Protocol` as the bounded DTO/codec lane and `src/SIR.Server` as the ASP.NET Core host; neither project owns S.I.R. gameplay rules, which remain in the existing domain/simulation/match projects.
- PD-003 [AC-001] [FR-003] complete: Use the published scaffold's ADR-0073 transport split: named cross-runtime codecs over plain HTTP for bootstrap and SignalR for authoritative session traffic. The browser proof will force disconnect/reconnect and assert a full S.I.R. projection resync.
- PD-004 [AC-001] [FR-004] complete: Keep exact shared numeric/path logic behind the public `FS.GG.Game.Core` 0.13.0 package, retain the existing .NET/Fable canonical-vector comparison, and reject sibling references or copied producer source in verification.
- PD-005 [AC-001] [FR-005] complete: Commit scaffold provenance naming `FS.GG.Workspace.Template` 0.8.0, provider `fs-gg-fable-game`, `FS.GG.Game.Skills` 0.7.0, and lockstep digest `443a82d24a0b4bbd21f4499b06f6e3d12b95a36a858f3880b414b74cae1a5c50` from SDD#817.
- PD-006 [AC-001] [FR-006] complete: Extend root scripts and CI so one command restores/builds/tests the .NET solution, existing conformance lanes, production Fable/Vite bundle, server publish, Playwright smoke, provenance verification, SDD evidence import, and doctor.

## Contract Impact
- PC-001 [PD-002] [PD-003] public protocol: `SIR.Protocol` defines versioned bootstrap, snapshot, input, and resync DTOs with named codecs compiled for both .NET and Fable; `/api/bootstrap` and `/hub/game` are the new external application boundary.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-003] [PD-004] [PD-005] [PD-006] [PC-001] integrationTest: Run the existing .NET/Fable conformance suite, protocol cross-runtime tests, server tests, production browser build, two-context Playwright reconnect/resync smoke, public-dependency/provenance verifier, clean-clone lifecycle, SDD evidence import, and doctor.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additiveInPlace: Retain all existing S.I.R. features and project identities, map scaffold boundaries onto `src/` and `tests/`, and record the one deliberate deviation: published ADR-0073 plain HTTP codecs supersede issue #138's stale Fable.Remoting wording.

## Generated View Impact
- GV-001 [PD-005] provenanceAndReadiness: SDD refresh owns `readiness/138-sir-fable-game-scaffold/work-model.json`; repository scripts verify the committed scaffold provenance and materialized lockstep-skill identity without copying producer-owned skill bytes.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 138-sir-fable-game-scaffold`.
