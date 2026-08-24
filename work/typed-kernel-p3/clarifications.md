---
schemaVersion: 1
workId: typed-kernel-p3
title: Published Typed Specification Kernel Adoption
stage: clarify
changeTier: tier1
status: clarified
sourceSpec: work/typed-kernel-p3/spec.md
publicOrToolFacingImpact: true
---

# Published Typed Specification Kernel Adoption Clarifications

## Source Specification
- work/typed-kernel-p3/spec.md

## Clarification Questions
- Q-001 [FR-001] [FR-005]: Which repository owns the shared model and which owns
  rule semantics after adoption?
- Q-002 [FR-002] [FR-004]: How is compatibility judged when the package-owned
  wrapper encoding intentionally differs from the P1 pilot encoding?
- Q-003 [FR-003]: Which mismatches must stop before compilation or projection?

## Answers
- A-001 [Q-001]: `FS.GG.SDD.Artifacts` owns `SpecificationId`, provenance,
  diagnostics, envelope validation, normalization, codec, semantic diff, evidence,
  and generic projection mechanics. S.I.R. owns `RuleDefinition`, its canonical
  encoding and evaluator, and the `RuleSpecificationAst` extension contract.
- A-002 [Q-002]: Existing canonical `RuleDefinition` bytes, execution results,
  replay/package identity, coherence, and .NET/Fable parity are the frozen behavior
  boundary. Package-owned specification fingerprints and projections are expected to
  migrate once and are then frozen by generated-view receipts.
- A-003 [Q-003]: Wrong package versions, model/rule identity disagreement,
  provenance/source disagreement, malformed JSON, stale generated source, and edited
  generated bodies must return stable diagnostics without compiling the rule or
  producing an accepted projection.

## Decisions
- DEC-001 [FR-001] [FR-005]: Consume exactly `FS.GG.SDD.Artifacts
  1.3.0-preview.3` through NuGet. Preview.3 is the fix-forward package whose packed
  portable source projection passes the real Fable consumer; preview.2 is retained as
  the red control. Never link producer source or copy its kernel into
  S.I.R.; delete the P1 `SpecificationModel.fs/.fsi` substrate.
- DEC-002 [FR-002] [FR-004]: Treat a package-schema-only wrapper fingerprint
  change as the planned migration, but block any canonical `RuleDefinition` byte,
  execution, replay, coherence, or native/Fable parity change.
- DEC-003 [FR-003]: Bind package identity, rule identity, source path/revision,
  extension semantics, and generated-view freshness before execution or projection.
- DEC-004 [FR-001] [FR-005]: The consumer extension may depend on the generic
  package contract, but the producer package must contain no S.I.R. reference,
  gameplay type, or runtime dependency.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
No blocking ambiguity remains.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work typed-kernel-p3`.
