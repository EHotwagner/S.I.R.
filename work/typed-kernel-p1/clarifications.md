---
schemaVersion: 1
workId: typed-kernel-p1
title: Typed Kernel P1 Specification Pilot
stage: clarify
changeTier: tier1
status: clarified
sourceSpec: work/typed-kernel-p1/spec.md
publicOrToolFacingImpact: true
---

# Typed Kernel P1 Specification Pilot Clarifications

## Source Specification
- work/typed-kernel-p1/spec.md

## Clarification Questions
- Q-001 [AC-001] [AC-004] [FR-003]: Which of direct records, a computation
  expression, or a hybrid becomes the selected P1 authoring surface?
- Q-002 [AC-002] [FR-002] [FR-005]: Which real rule proves the compiler without
  widening P1 into a second family or another semantic authority?
- Q-003 [AC-003] [FR-001] [FR-004]: Which provenance fields contribute to semantic
  identity, and how is a direct projection edit distinguished from stale source?

## Answers
- A-001 [Q-001]: Implement and measure all three against one AST. Select the hybrid
  only if it preserves record-level inspectability, reduces mechanical construction,
  and adds no hidden builder state. The session receipts, not preference, decide.
- A-002 [Q-002]: Migrate `COMBAT-DAMAGE-001`. It exercises dependencies, formula
  input/result types, provenance, projection, execution, replay identity, and parity
  while remaining one member of the existing combat family.
- A-003 [Q-003]: Normalization includes model identity, schema, source revision,
  and the complete S.I.R. rule AST. Agent, session, timestamp, and intent notes stay
  inspectable in provenance and authoring receipts but do not change semantic bytes.
  The projection embeds both the normalized source fingerprint and a fingerprint over
  its generated body; source drift and direct-edit drift therefore have distinct codes.

## Decisions
- DEC-001 [AC-002] [FR-002]: `RuleDefinition`, `Rules.validate`, and existing
  canonical encoders remain the sole executable authority. `RuleSpecification.compile`
  is an authoring adapter and introduces no evaluator.
- DEC-002 [AC-001] [AC-004] [FR-003]: Direct records are the semantic reference,
  the computation expression is an evaluated candidate, and a hybrid of explicit
  records plus narrow constructors is the recommended candidate pending three sessions.
- DEC-003 [AC-002] [FR-002] [FR-005]: Only `COMBAT-DAMAGE-001` is migrated. Every
  other rule and all registered-algorithm contracts remain unchanged.
- DEC-004 [AC-003] [FR-001] [FR-004]: The normalized specification uses one
  versioned canonical binary encoding and SHA-256 fingerprint. Diagnostics carry a
  stable code, field path, and actionable message in deterministic order.
- DEC-005 [AC-003] [FR-004]: The existing rule-corpus generation tool owns the
  projection write. Check mode regenerates to a temporary location and compares bytes;
  malformed, missing, stale-source, and direct-edit cases are separate controls.
- DEC-006 [AC-004] [FR-003]: Three authoring-session receipts live under the SDD
  package's artifacts. They are evidence only and cannot override F# or corpus authority.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
No blocking ambiguity remains.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work typed-kernel-p1`.
