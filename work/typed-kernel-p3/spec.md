---
schemaVersion: 1
workId: typed-kernel-p3
title: Published Typed Specification Kernel Adoption
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Published Typed Specification Kernel Adoption Specification

Prose status: specified

## User Value
S.I.R. consumes the published typed specification kernel without changing authoritative game behavior.

## Scope
- SB-001: Adopt FS.GG.SDD.Artifacts 1.3.0-preview.3 in SIR.Domain and retain only the S.I.R.-owned rule extension contract.
- SB-002: Delete SpecificationModel.fs and SpecificationModel.fsi and update the frozen v2 corpus projections and correspondence.

## Non-Goals
- SB-003: Move gameplay semantics or S.I.R. runtime dependencies into FS.GG.SDD.
- SB-004: Change combat rules, outcomes, or registered algorithm behavior.

## User Stories
- US-001 (P1): As a user, I can S.I.R. consumes the published typed specification kernel without changing authoritative game behavior.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given Published Typed Specification Kernel Adoption is available, when the user exercises it, then they can S.I.R. consumes the published typed specification kernel without changing authoritative game behavior.

## Functional Requirements
- FR-001: The dependency graph resolves exactly FS.GG.SDD.Artifacts 1.3.0-preview.3 and FS.GG.Contracts 7.5.2 with no S.I.R. runtime dependency in the producer package. (Stories: US-001; Acceptance: AC-001)
- FR-002: COMBAT-DAMAGE-001 compilation, canonical rule bytes, execution outcome, replay identity, coherence, and native/Fable conformance remain unchanged. (Stories: US-001; Acceptance: AC-001)
- FR-003: Package identity, source provenance, semantic identity, malformed model, stale source, and edited projection mismatches produce stable diagnostics before execution or projection. (Stories: US-001; Acceptance: AC-001)
- FR-004: Generated Markdown and JSON views are derived from the package-owned normalized model and verify byte-for-byte. (Stories: US-001; Acceptance: AC-001)
- FR-005: Public API and package lock receipts prove there is one shared kernel authority and no vendored or forked substrate. (Stories: US-001; Acceptance: AC-001)

## Ambiguities
No material ambiguities recorded.

## Public Or Tool-Facing Impact
- This specification is an SDD lifecycle artifact and command-report contract input.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work typed-kernel-p3`.
