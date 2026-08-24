---
schemaVersion: 1
workId: typed-kernel-p3
title: Published Typed Specification Kernel Adoption
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/typed-kernel-p3/spec.md
sourceClarifications: work/typed-kernel-p3/clarifications.md
sourceChecklist: work/typed-kernel-p3/checklist.md
publicOrToolFacingImpact: true
---

# Published Typed Specification Kernel Adoption Plan

Prose status: planned

## Source Snapshot
- spec: work/typed-kernel-p3/spec.md sha256:dae9883a04c69bbe6281a274430f085b5c8a180560c70e12f2d866fa919d3bdb schemaVersion:1
- clarifications: work/typed-kernel-p3/clarifications.md sha256:1040c2d89bd83d2eba564f580e1dcaf7a119fca8b49aeeab239eb1dac5e77c0b schemaVersion:1
- checklist: work/typed-kernel-p3/checklist.md sha256:fa31b779574bc208d4b092b867962803637e0595d1cb93858743846bc8ce89a3 schemaVersion:1

## Plan Scope
- Work item typed-kernel-p3 is planned from the current specification, clarification, and checklist facts.
- Requirement count: 5.
- Clarification decision count: 0.
- Checklist result count: 5.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] [DEC-004] complete: Add an exact central
  package version and direct `SIR.Domain` package reference, restore the locked graph,
  and verify the producer package metadata contains only `FS.GG.Contracts 7.5.2` plus
  its existing generic artifact dependencies and no S.I.R. assembly or type.
- PD-002 [AC-001] [FR-002] [DEC-002] complete: Adapt `RuleSpecificationAst` as an
  explicit `ExtensionContract` while retaining `Rules.canonicalRuleBytes` and the
  existing evaluator as the only rule semantics. Compare frozen native and Fable
  corpus results, canonical rule bytes, replay identity, and coherence.
- PD-003 [AC-001] [FR-003] [DEC-003] complete: Validate the package assembly version,
  model/rule identity, provenance/source binding, extension invariants, codec input,
  and projection observations before returning a compiled rule or accepted view; add
  negative conformance cases for every mismatch class.
- PD-004 [AC-001] [FR-004] [DEC-002] complete: Generate the rule Markdown and JSON
  receipt from `SpecificationProjection.generate`, preserve the S.I.R.-specific
  controlled statement in extension Markdown, and make check mode compare regenerated
  bytes plus the package-owned source/generated fingerprints.
- PD-005 [AC-001] [FR-005] [DEC-001] [DEC-004] complete: Remove
  `SpecificationModel.fs/.fsi` and their compile entries, update public-surface and
  source-correspondence baselines, and record lock-file, assembly, package-content,
  and isolated-consumer receipts proving the shared kernel has one authority.

## Contract Impact
- PC-001 [PD-001] [PD-002] F# API: `SIR.Domain` replaces its pilot generic envelope
  types with public types from `FS.GG.SDD.Artifacts.TypedSpecifications`; the
  S.I.R.-specific `RuleSpecificationAst` and authoring helpers remain consumer-owned.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-003] [PD-004] [PD-005] [PC-001] semanticTest:
  Restore locked dependencies; run Domain and shared conformance tests on .NET and
  Fable; verify the v2 corpus, projections, coherence, replay/package identity, public
  surface, exact package graph, producer nuspec/content, and an isolated consumer.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additiveThenRemove: First compile S.I.R. against the published
  contract, then remove the pilot files in the same atomic change. Regenerate the
  package-owned wrapper fingerprints and projections, but require the historical
  canonical rule bytes and runtime observations to remain identical.

## Generated View Impact
- GV-001 [PD-004] generatedViews: Refresh the v2 specification Markdown/JSON,
  implementation-source correspondence, public API baselines, SDD work model, and P3
  verification receipts; all check modes must report stale or edited output distinctly.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work typed-kernel-p3`.
