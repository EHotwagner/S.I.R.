---
schemaVersion: 1
workId: 187-fable-game-governance-parity
title: Fable Game Governance Parity
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/187-fable-game-governance-parity/spec.md
sourceClarifications: work/187-fable-game-governance-parity/clarifications.md
sourceChecklist: work/187-fable-game-governance-parity/checklist.md
publicOrToolFacingImpact: true
---

# Fable Game Governance Parity Plan

Prose status: planned

## Source Snapshot
- spec: work/187-fable-game-governance-parity/spec.md sha256:ea7832d29c41a4126e82dbf7a4b27cfa1542b73dd74ac58fe88181c890595284 schemaVersion:1
- clarifications: work/187-fable-game-governance-parity/clarifications.md sha256:3a24d65ac85a98b5167c26c374d9b92be9b63ee2ca88933572dd042141b803ff schemaVersion:1
- checklist: work/187-fable-game-governance-parity/checklist.md sha256:26117ababcf78b16cf447fe80a3a0208e52ccd336f141a781fdfe9477fc76eb0 schemaVersion:1

## Plan Scope
- Pin package-only dependencies, add governed Fable policy and skills, implement clean-consumer cross-runtime conformance, document parity, and migrate the work identity.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Pin the three published packages centrally and assert lock files and project files contain no project-reference or local-binary escape hatch.
- PD-002 [AC-002] [FR-002] complete: Build a clean package consumer that executes only profile-v1 LockstepExact surfaces and compares complete canonical bytes from .NET, Node/Fable, and browser Fable to the package oracle.
- PD-003 [AC-003] [FR-003] complete: Add per-primitive subject mutations and assert the comparison command returns red with primitive identity and first-byte divergence.
- PD-004 [AC-004] [FR-004] complete: Declare controlled imports and boundary allowlists in .fsgg, then run mutation fixtures through the same local and CI policy commands.
- PD-005 [AC-005] [FR-005] complete: Materialize the ten named game skills under neutral, Claude, and Codex roots and verify byte/equivalence projections.
- PD-006 [AC-006] [FR-006] complete: Wire governance, reference, surface, boundary, and performance checks into CI and a single clean-checkout conformance entry point.
- PD-007 [AC-007] [FR-007] complete: Publish a responsibility matrix separating direct native parity, SVG/Fable adaptations, and intentionally inapplicable Skia features.
- PD-008 [AC-008] [FR-008] complete: Move 178 provenance to canonical 187 artifacts, search for stale bindings, and regenerate analyze/evidence/verify/ship/refresh/agents.

## Contract Impact
- PC-001 [PD-001] package boundary: Directory.Packages.props and project lock files declare released package identities; no source tree becomes a dependency contract.

## Verification Obligations
- VO-001 [PD-002] [PD-003] [PC-001] verification: Run clean .NET/Node/browser fixture comparison and every primitive inversion; retain package, profile, fixture, toolchain, and first-divergence evidence.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PD-008] provenance: The missing local 178 package is recorded as unavailable-source provenance; canonical authored and generated artifacts use 187 exclusively.

## Generated View Impact
- GV-001 [PD-005] [PD-008] workModel: Refresh work model, summary, and equivalent Claude/Codex guidance whenever authored lifecycle or skill material changes.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 187-fable-game-governance-parity`.
