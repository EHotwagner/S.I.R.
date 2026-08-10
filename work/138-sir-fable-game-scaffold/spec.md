---
schemaVersion: 1
workId: 138-sir-fable-game-scaffold
title: Incorporate S.I.R. into the fs-gg-fable-game scaffold
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Incorporate S.I.R. into the fs-gg-fable-game scaffold Specification

Prose status: specified

## User Value
Adopt FS.GG.Workspace.Template 0.8.0 fs-gg-fable-game boundaries in place without resetting S.I.R. features.

## Scope
- SB-001: SIR.slnx, package.json, package-lock.json, src/SIR.Domain, src/SIR.Client, src/SIR.Client.Web, src/SIR.Protocol, src/SIR.Server, tests, scripts, .fsgg, work, .github/workflows, docs.

## Non-Goals
- SB-002: Do not implement later lifecycle commands or Governance enforcement in this specification.

## User Stories
- US-001 (P1): As a user, I can adopt FS.GG.Workspace.Template 0.8.0 fs-gg-fable-game boundaries in place without resetting S.I.R. features.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given Incorporate S.I.R. into the fs-gg-fable-game scaffold is available, when the user exercises it, then they can adopt FS.GG.Workspace.Template 0.8.0 fs-gg-fable-game boundaries in place without resetting S.I.R. features.

## Functional Requirements
- FR-001: Preserve all existing domain, simulation, match, client, and .NET/Fable conformance tests and record an explicit migration mapping. (Stories: US-001; Acceptance: AC-001)
- FR-002: Add a buildable F# ASP.NET Core server and a bounded shared F# wire-protocol lane. (Stories: US-001; Acceptance: AC-001)
- FR-003: A browser smoke test must observe a real S.I.R. live projection through typed HTTP bootstrap and SignalR authoritative traffic, disconnect/reconnect, and bounded full resync. (Stories: US-001; Acceptance: AC-001)
- FR-004: FS.GG.Game.Core 0.13.0 remains package-only and canonical vectors pass on .NET and Fable under fs-gg-game-core-fable-lockstep-v1. (Stories: US-001; Acceptance: AC-001)
- FR-005: Provenance records FS.GG.Workspace.Template 0.8.0 and FS.GG.Game.Skills 0.7.0 digest 443a82d24a0b4bbd21f4499b06f6e3d12b95a36a858f3880b414b74cae1a5c50. (Stories: US-001; Acceptance: AC-001)
- FR-006: Clean restore, build, conformance, production browser build, browser smoke, SDD evidence import, doctor, provenance, and integration report pass. (Stories: US-001; Acceptance: AC-001)

## Ambiguities
No material ambiguities recorded.

## Public Or Tool-Facing Impact
- This specification is an SDD lifecycle artifact and command-report contract input.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 138-sir-fable-game-scaffold`.
