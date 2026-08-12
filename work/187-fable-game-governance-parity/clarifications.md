---
schemaVersion: 1
workId: 187-fable-game-governance-parity
title: Fable Game Governance Parity
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/187-fable-game-governance-parity/spec.md
publicOrToolFacingImpact: true
---

# Fable Game Governance Parity Clarifications

## Source Specification
- work/187-fable-game-governance-parity/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: What happens when the provisional work-178 package is unavailable in the current checkout?

## Answers
- CQ-001: Record migration as unavailable-source provenance, create the canonical 187 package from the issue contract, and verify no committed artifact remains bound to 178.

## Decisions
- DEC-001: [CQ-001] [AMB:AMB-001] [FR-008] [AC-008] resolved: The absent provisional package is not reconstructed from unverified local state; the issue body is the authoritative migration input and repository search is the negative provenance evidence.

## Accepted Deferrals
- None.

## Remaining Ambiguity
- None.

## Lifecycle Notes
- The package-only classification and cross-runtime evidence rules remain binding during implementation.
