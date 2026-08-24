---
schemaVersion: 1
workId: typed-kernel-p1
title: Typed Kernel P1 Specification Pilot
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/typed-kernel-p1/spec.md
sourceClarifications: work/typed-kernel-p1/clarifications.md
sourceChecklist: work/typed-kernel-p1/checklist.md
publicOrToolFacingImpact: true
---

# Typed Kernel P1 Specification Pilot Plan

Prose status: planned

## Source Snapshot
- spec: work/typed-kernel-p1/spec.md sha256:77f6869ce0c5fc68ba245ab9f248385d6937c2533cdf2a74cc1f9c7bccc13a99 schemaVersion:1
- clarifications: work/typed-kernel-p1/clarifications.md sha256:6b1dcb2b4f557e981803585a5836972848d8e52a4ad1b4dd9c2875586ae342fc schemaVersion:1
- checklist: work/typed-kernel-p1/checklist.md sha256:241e1fb204ca6de89e463d2e83c91d6bc7e820eb264ec56e5b10f65a5c199d03 schemaVersion:1

## Plan Scope
- Work item typed-kernel-p1 is planned from the current specification, clarification, and checklist facts.
- Requirement count: 5.
- Clarification decision count: 6.
- Checklist result count: 5.

## Plan Decisions
- PD-001 [AC-001] [AC-003] [FR-001] [DEC-004] complete: Add `SpecificationIdentity`, versioned `SpecificationModel<'a>`, explicit provenance, stable diagnostics, canonical normalization/fingerprint, and semantic diff in `SIR.Domain`; use existing canonical encoding/hash primitives and keep Fable-compatible code free of reflection and `obj`.
- PD-002 [AC-001] [AC-002] [FR-002] [FR-003] [DEC-001] [DEC-002] complete: Add the S.I.R.-owned rule specification AST/compiler plus direct record, computation-expression, and narrow hybrid constructors; every form produces one model consumed by one validator/compiler into `RuleDefinition`.
- PD-003 [AC-002] [FR-002] [FR-005] [DEC-003] complete: Replace only the hand-constructed `COMBAT-DAMAGE-001` definition with the compiled pilot model, retain a reference definition in conformance evidence, and assert identical canonical rule bytes, execution outcome, replay/package identity, manifest, coverage, and full cross-runtime vector.
- PD-004 [AC-003] [AC-004] [FR-004] [DEC-005] complete: Extend the existing rule-governance generator and `generate-rules-corpus.sh`/`verify-rules-corpus.sh` boundary to emit/check one deterministic Markdown projection and one JSON authoring receipt in `tests/fixtures/rules-corpus/v2`; embed normalized-source and generated-body fingerprints and refuse missing, malformed, stale-source, or directly edited output distinctly.
- PD-005 [AC-001] [AC-002] [AC-003] [FR-001] [FR-002] [FR-005] complete: Extend shared conformance fixtures with normalization/compile/projection assertions and focused subject mutations for identity, provenance, AST validation, equivalent syntax, stale source, direct edit, unreadable input, and opaque registered-algorithm visibility; preserve the pre-P1 authoritative output bytes.
- PD-006 [AC-004] [FR-003] [FR-004] [DEC-006] complete: Extend `sir-author-rule` through inspect → intent → one material question → typed proposal and human projection → edit → validate → semantic diff → evidence/coherence → revise; commit three bounded authoring-session receipts under the SDD artifact directory and select the lowest-friction surface from their measured fields.
- PD-007 [AC-003] [FR-004] complete: Emit all five rule-corpus artifacts from one native conformance process so projection governance retains its existing refusal coverage without eroding the repository's protected feedback-headroom budget.

## Contract Impact
- PC-001 [PD-001] F# API: additive repository-local schema-v1 `SpecificationModel` identity/provenance/diagnostic/normalization/diff surface in `SIR.Domain`; this is a pilot source, not a published shared package.
- PC-002 [PD-002] F# API: additive S.I.R.-owned `RuleSpecification` authoring and compile surface; compiled `RuleDefinition` remains the sole runtime authority.
- PC-003 [PD-004] generated projection: schema-v1 Markdown plus JSON receipt under `tests/fixtures/rules-corpus/v2`; the generator is authoritative and check mode rejects direct edits.
- PC-004 [PD-004] command: existing `scripts/generate-rules-corpus.sh [--write|--check]` and `scripts/verify-rules-corpus.sh` gain typed-specification projection verification without a new parallel command.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PC-001] [PC-002] semanticTest: Native and Fable shared conformance prove equivalent forms normalize/fingerprint/compile identically and invalid identity/provenance/AST inputs yield stable field diagnostics.
- VO-002 [PD-003] [PC-002] compatibilityTest: The migrated `COMBAT-DAMAGE-001` canonical bytes equal the retained direct reference; complete domain conformance remains byte-identical across .NET/Fable and existing replay/manifest/coverage assertions pass.
- VO-003 [PD-004] [PC-003] [PC-004] generatedViewTest: Corpus generation write/check is deterministic; missing, malformed, stale-source, and direct-edit mutations each red with a distinct actionable diagnostic and restored output is byte-identical.
- VO-004 [PD-005] mutationTest: Every added or modified refusal gate is inverted by breaking its subject, including unreadable JSON and a production-shaped fixture, and the observed red is recorded before restored green.
- VO-005 [PD-006] documentationTest: Three authoring-session receipts have complete measurement envelopes and the skill/report selection is derived from their question/revision/diagnostic/elapsed/diff fields.
- VO-006 [PD-007] performanceTest: The complete corpus and refusal-mutation verifier passes with bundled artifact emission and the protected CI route re-establishes its existing feedback-headroom contract.

## Performance Intent
- Normalization and projection are authoring-time operations. Record elapsed time for all
  three sessions and corpus generation; do not add a latency threshold without a P1 baseline.
- Runtime combat execution gains no alternate evaluator and no additional work after the
  compiled registry value is constructed.

## Migration Posture
- PM-001 [PC-001] additive: Pilot schema version 1 has no durable external consumers; an incompatible future form requires an explicit new schema/version and stable unsupported-version diagnostic.
- PM-002 [PC-002] compile-in-place: Only `COMBAT-DAMAGE-001` changes authoring source, not rule identity or meaning; canonical equality is the rollback boundary and reverting to the retained direct record restores the prior construction without data migration.
- PM-003 [PC-003] regenerate: Generated projection/receipt are disposable derived artifacts; direct edits are refused and recovery is regeneration from canonical F# source.

## Generated View Impact
- GV-001 [PD-001] workModel: Standard SDD exclusively generates `readiness/typed-kernel-p1/work-model.json` from the authored lifecycle sources; implementation code never edits it, and analyze/verify must report stale source digests before acceptance.
- GV-002 [PD-004] ruleProjection: `tests/fixtures/rules-corpus/v2/combat-damage-001.specification.md` and its JSON receipt are generated only by the existing corpus generator and bind source/normalized/body fingerprints.
- GV-003 [PD-001] fsharpPublicSurface: the repository governance verifier regenerates the aggregate and per-project public-surface receipts after the additive `SIR.Domain` and `CombatRules.fsi` API changes.
- GV-004 [PD-001] productionReview: the public kernel changes the production Fable bundle even though gameplay output is unchanged, so `docs/assets/map-editor-review/manifest.json` must be regenerated from that exact bundle to retain the existing production-review freshness contract.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work typed-kernel-p1`.
