---
schemaVersion: 1
workId: typed-kernel-p0
title: Typed Kernel P0
stage: clarify
changeTier: tier1
status: clarified
sourceSpec: work/typed-kernel-p0/spec.md
publicOrToolFacingImpact: true
---

# Typed Kernel P0 Clarifications

## Source Specification
- work/typed-kernel-p0/spec.md

## Clarification Questions
- Q-001 [AC-001] [FR-001]: How are missing live predicate and supersession nodes
  represented without inventing corpus authority?

## Answers
- A-001 [Q-001]: Classify them as `validation-fixture` surfaces, bind the exact
  fixture source and protected check, and keep them out of the canonical registry
  selection. The manifest distinguishes authority, projection, and fixture.

## Decisions
- DEC-001 [AC-001] [FR-001]: Generate the committed schema-v1 P0 manifest from a
  small shared F# projection; the repository check script binds committed artifact
  files with SHA-256 and rejects drift.
- DEC-002 [AC-002] [FR-002] [FR-004]: Reuse the complete existing native/Fable
  conformance route as the parity promise. P0 does not create a weaker subset.
- DEC-003 [AC-001] [FR-003]: Record historical measurements absent from SDD and
  authoring receipts as `unknown`, with the inspected source named; never estimate.
- DEC-004 [AC-004] [FR-005]: Candidate reuse is an evidence table. A concept with
  fewer than two concrete checked uses is rejected before P1.
- DEC-005 [AC-004] [FR-005]: Gameplay typed values, formula semantics, registered
  algorithm implementations, attack/recovery interpreters, and replay execution stay
  S.I.R.-owned even when their identity/provenance substrate motivates shared concepts.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
No blocking ambiguity remains.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work typed-kernel-p0`.
