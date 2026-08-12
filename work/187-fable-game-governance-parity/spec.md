---
schemaVersion: 1
workId: 187-fable-game-governance-parity
title: Fable Game Governance Parity
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Fable Game Governance Parity Specification

Prose status: specified

## User Value
Browser players receive the same governed and executable lockstep game foundation as native consumers.

## Scope
- SB-001: Pin released FS.GG.Game.Core, FS.GG.Governance.Cli, and FS.GG.ReferenceGateSet packages; prohibit source-copy and local-binary substitutions.
- SB-002: Materialize the applicable AI, ballistics, effects, game-core, grids, line-drawing, mapcraft, persistence, playtest, and visibility skills for all repository agent targets.
- SB-003: Declare capabilities, controlled imports, tooling/policy identities, performance workloads, package/runtime boundaries, and the Fable/SVG-to-native/Skia applicability matrix.
- SB-004: Migrate authored and generated work/readiness provenance from provisional id 178 to canonical id 187 without retaining an epic-178 binding.

## Non-Goals
- SB-005: No cover, armor, awareness, tactical overlays, or new tactical scenarios are introduced.

## User Stories
- US-001 (P1): As a browser player, I receive the same governed lockstep primitives as native consumers.
- US-002 (P1): As a maintainer, I can prove package-only and controlled-import policy from a clean checkout.
- US-003 (P1): As a game developer, I can identify exactly which Fable/SVG responsibilities match, adapt, or do not apply to native/Skia governance.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given a clean consumer, when it restores the pinned package, then it uses no project reference, copied core source, or local binary substitution.
- AC-002 [US-001] [FR-002]: Given canonical fixtures, when .NET and Fable execute Cell ordering, edge queries, LOS, and bounded pathfinding, then their full canonical byte streams match the oracle.
- AC-003 [US-001] [FR-003]: Given a subject mutation in each adopted primitive, when comparison runs, then the gate reports first divergence and fails.
- AC-004 [US-002] [FR-004]: Given a forbidden import or boundary mutation, when local or CI conformance runs, then the intended governance gate fails.
- AC-005 [US-002] [FR-005]: Given the repository agent targets, when guidance is refreshed, then all named game skills are materialized equivalently.
- AC-006 [US-002] [FR-006]: Given a clean checkout, when CI and local conformance run, then governance, reference, public-surface, boundary, and performance gates pass.
- AC-007 [US-003] [FR-007]: Given the parity documentation, when a maintainer reviews it, then each native/Skia responsibility is marked direct, adapted for SVG/Fable, or intentionally inapplicable.
- AC-008 [US-002] [FR-008]: Given the canonical SDD work id, when readiness is generated, then analyze, evidence, verify, ship, refresh, and agents are current and no artifact references work item 178.

## Functional Requirements
- FR-001: The product MUST consume only exact pinned released FS.GG.Game.Core, FS.GG.Governance.Cli, and FS.GG.ReferenceGateSet packages. (covers AC-001)
- FR-002: The product MUST compare package-derived .NET and Fable outputs for Cell ordering, Edges.edgeBetween, Los.lineOfSightBy, and Pathfinding.astar against canonical fixtures. (covers AC-002)
- FR-003: The comparison harness MUST fail each adopted primitive mutation and emit first-byte divergence diagnostics. (covers AC-003)
- FR-004: Controlled-import and package-only boundary gates MUST reject their intended mutations. (covers AC-004)
- FR-005: The repository MUST materialize applicable game skills into Claude, Codex, and neutral agent targets with equivalent behavior. (covers AC-005)
- FR-006: CI and the documented local route MUST execute governance, reference, public-surface, boundary, and performance conformance. (covers AC-006)
- FR-007: Documentation MUST provide a direct/adapted/inapplicable native-Skia versus Fable-SVG governance matrix. (covers AC-007)
- FR-008: The provisional 178 package and readiness provenance MUST migrate to 187 and all lifecycle generated views MUST be current. (covers AC-008)

## Ambiguities
- AMB-001: The original provisional work package is not present in this checkout; its recoverable provenance and migration disposition must be recorded.

## Public Or Tool-Facing Impact
- Package pins, controlled-import policies, CI/local conformance commands, agent skill projections, and generated SDD readiness are tool-facing contracts.

## Lifecycle Notes
- Preserve replay identity outside the package boundary; only profile-v1 LockstepExact surfaces are authoritative shared logic.
