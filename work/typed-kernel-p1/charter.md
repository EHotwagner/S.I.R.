---
schemaVersion: 1
workId: typed-kernel-p1
title: Typed protocol kernel P1 specification pilot
stage: charter
changeTier: tier1
status: chartered
policyPointers:
  - .fsgg/sdd.yml
  - .fsgg/agents.yml
  - .fsgg/policy.yml
  - .fsgg/capabilities.yml
  - .fsgg/tooling.yml
---

# Typed protocol kernel P1 specification pilot Charter

## Identity
- Work id: `typed-kernel-p1`
- Lifecycle stage: charter
- Status: chartered

## Principles
- Keep `RuleDefinition` and the executable corpus as the sole gameplay authority.
- Make authored intent inspectable, versioned, deterministic, and reviewable before compilation.
- Preserve current execution, replay, manifest, coverage, and .NET/Fable bytes for the migrated rule.
- Prefer the smallest authoring surface selected by measured sessions, not abstraction appetite.
- Fail closed on malformed source, stale projections, direct edits, and provenance/fingerprint drift.

## Scope Boundaries
- In scope: a repository-local `SpecificationModel` envelope, a S.I.R. rule extension,
  direct-record/computation-expression/hybrid pilots, canonical normalization and semantic
  diff, typed validation/diagnostics, one migrated real rule, a generated freshness-bound
  Markdown projection, three iterative authoring-session receipts, and the governing skill loop.
- Out of scope: a second rule family, general mutation algebra, platform packaging,
  coordination adoption, provider/profile expansion, or changing gameplay behavior.
- `EHotwagner/S.I.R.#347` is the governing issue. Its typed delivery route is
  `sdd-required` and binds this package at `work/typed-kernel-p1/spec.md`.

## Acceptance Boundary
- Direct, computation-expression, and hybrid forms for the same meaning normalize to
  byte-identical specification bytes and compile to the existing authoritative rule bytes.
- `COMBAT-DAMAGE-001` is authored through the pilot without changing execution, replay,
  manifest/coverage semantics, or the complete native/Fable conformance vector.
- The human projection carries model identity, schema, source fingerprint, and generated
  fingerprint; check mode rejects stale or directly edited content with actionable diagnostics.
- Registered algorithms remain explicit opaque contracts with inputs, outputs, reads/writes,
  evidence, and implementation fingerprint; no reflection, `obj`, or untyped escape hatch enters.
- Three bounded authoring sessions record questions, revisions, diagnostics, elapsed time,
  and select one surface without creating another semantic authority.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Governance files are optional compatibility pointers and are not evaluated by this command.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd specify --work typed-kernel-p1`.
