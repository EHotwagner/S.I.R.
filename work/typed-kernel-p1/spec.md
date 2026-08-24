---
schemaVersion: 1
workId: typed-kernel-p1
title: Typed Kernel P1 Specification Pilot
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Typed Kernel P1 Specification Pilot Specification

Prose status: specified

## User Value
A S.I.R. rule author can inspect, compare, validate, and review typed intent before it becomes the existing executable RuleDefinition.

## Scope
- SB-001: Add a repository-local specification envelope and S.I.R. rule extension, evaluate three authoring forms, migrate COMBAT-DAMAGE-001 through the compiler, generate a freshness-bound projection, record three authoring sessions, and preserve all authoritative behavior.

## Non-Goals
- SB-002: Do not extract a package, add another rule family, introduce a general mutation algebra, use reflection or obj, or change gameplay semantics.

## User Stories
- US-001 (P1): As a rule author, I can author equivalent direct, computation-expression, and hybrid forms and see one canonical normal form and semantic diff.
- US-002 (P1): As a maintainer, I can compile one real rule without changing its canonical bytes and reject stale projections or malformed provenance.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001] [FR-003]: Given direct-record, computation-expression,
  and hybrid sources with the same meaning, when they validate and normalize, then
  normalized bytes, fingerprints, compiled definitions, and semantic diffs are identical.
- AC-002 [US-002] [FR-002] [FR-005]: Given the current `COMBAT-DAMAGE-001`, when
  its typed specification compiles into the registry, then its canonical rule bytes,
  execution, replay binding, generated manifest/coverage meaning, and complete native/Fable
  conformance bytes remain identical to the P0 baseline.
- AC-003 [US-002] [FR-001] [FR-004]: Given malformed identity/provenance, an invalid
  rule AST, missing/malformed generated content, a changed source fingerprint, or a direct
  projection edit, when check mode runs, then it refuses with the failed field and repair named.
- AC-004 [US-001] [US-002] [FR-003] [FR-004]: Given three complete iterative
  authoring sessions and Standard SDD artifacts, when the slice is reviewed, then the
  session receipts select one surface from measured friction and SDD reports
  `implementationReady` before implementation and `shipReady` only after observed evidence.

## Functional Requirements
- FR-001: Canonical `SpecificationModel` identity, schema version, provenance, normalized bytes, stable fingerprint, typed diagnostics, and deterministic semantic diff MUST be explicit and inspectable; invalid or unreadable values MUST not become a specification verdict. (covers AC-001, AC-003)
- FR-002: A S.I.R.-owned rule specification compiler MUST validate and compile one migrated real rule to the current `RuleDefinition` authority without a second execution path. (covers AC-002)
- FR-003: Direct-record, computation-expression, and hybrid forms with equivalent meaning MUST normalize byte-identically; three iterative sessions MUST record questions, revisions, diagnostics, semantic-diff result, elapsed time, and select the least-friction surface. (covers AC-001, AC-004)
- FR-004: Generated Markdown and manifest projection MUST carry source and generated fingerprints; check mode MUST reject stale, missing, malformed, or directly edited output, and generation MUST be the only normal write path. (covers AC-003, AC-004)
- FR-005: Registered algorithms MUST remain explicit opaque typed contracts whose symbol, fingerprint, inputs, result, reads/writes, evidence, and explanation fields are visible; the complete .NET/Fable corpus, execution, replay, manifest, and coverage behavior MUST remain compatible. (covers AC-002)

## Ambiguities
No material ambiguities recorded.

## Public Or Tool-Facing Impact
- Adds repository-local public F# types and functions for the pilot, extends the existing
  rule-corpus generation/check command, and generates one schema-v1 human projection.
  It does not publish a package or change a runtime/wire contract.

## Compatibility
- `RuleDefinition`, `Rules.canonicalRuleBytes`, combat execution, replay resolution,
  retained corpus v1/v2, manifest schema, and coverage schema remain authoritative.
- The compiler is an authoring adapter into that authority, not a parallel evaluator.
- The generated pilot projection starts at schema version 1 and is never read as gameplay input.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work typed-kernel-p1`.
