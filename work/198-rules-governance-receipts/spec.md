---
schemaVersion: 1
workId: 198-rules-governance-receipts
title: Rules Governance Receipts
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Rules Governance Receipts Specification

Prose status: specified; Tier 1 receipt, adapter, verdict, and protected-boundary contract.

## User Value
Maintainers receive one stable, explainable verdict over the executable F# rules corpus without moving gameplay authority into Governance.

## Scope
- SB-001: A S.I.R.-owned `sir-rules-governance/v1` receipt over the rule manifest, public surface, semantic evidence, parity, generated views, package identities, replay history, and production journeys.
- SB-002: A product-owned closed fact union and reified-check adapter, deterministic generation/evaluation, graduated maturity, CI integration, negative mutations, documentation, and SDD protected-boundary handoff.
- SB-003: Exact released package dependencies only; a missing general-purpose Governance capability is producer work and a typed blocker rather than a local DLL or sibling-project workaround.

## Non-Goals
- SB-004: Do not replace `FormulaExpr`, transitions, `RuleApplication`, the rule registry, or replay interpretation with Governance checks.
- SB-005: Do not make `SIR.Domain` or gameplay execution depend on Governance runtime semantics.
- SB-006: Do not migrate every legacy mechanic or extract a cross-product rule algebra in this item.

## User Stories
- US-001 (P1): As a maintainer, I can generate and compare one deterministic receipt whose identities trace every governed rule fact to authoritative corpus and evidence artifacts.
- US-002 (P1): As a reviewer, I can inspect the exact reified checks, stable text/hash/explanation, finding provenance, and effective maturity that produced the verdict.
- US-003 (P1): As a release owner, I receive a fail-closed ship decision for missing semantic, parity, package, replay, coverage, or production-journey evidence without conflating it with SDD readiness.
- US-004 (P1): As a gameplay engineer, I retain the existing F# evaluator and replay semantics with no Governance dependency on the runtime path.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001] [FR-002]: Given identical authoritative corpus and evidence inputs in different enumeration orders, when the receipt is generated twice, then its canonical bytes, digest, package binding, and ordered rule facts are identical.
- AC-002 [US-001] [US-003] [FR-003] [FR-004]: Given complete, missing, malformed, stale, synthetic, or unavailable evidence, when sensing runs, then each state is represented explicitly and only real current satisfying evidence can pass a required check.
- AC-003 [US-002] [FR-005] [FR-006]: Given a valid receipt, when the product adapter evaluates it, then a closed fact union, reified checks, stable rendered text, structural hashes, explanations, and per-finding provenance all derive from the same check values.
- AC-004 [US-002] [US-003] [FR-007] [FR-008]: Given advisory, PR-blocking, and ship-blocking findings under different profiles, when routing runs, then effective enforcement changes only as declared and never hides the underlying verdict or maturity.
- AC-005 [US-003] [FR-009] [FR-010]: Given .NET, Fable/Node, and browser evidence or a historical replay, when identities disagree or the recorded package is absent, then ship blocks and current rules are never substituted.
- AC-006 [US-003] [FR-011]: Given SDD ship readiness and a rules-governance verdict, when the protected handoff runs, then both remain distinct versioned artifacts and the protected-boundary result names both exact inputs.
- AC-007 [US-004] [FR-012]: Given the completed dependency graph and production runtime build, when boundaries are inspected and mutation-tested, then Governance is present only at package-only tooling/test edges and never in `SIR.Domain` or gameplay evaluation.
- AC-008 [US-001] [US-002] [US-003] [FR-013]: Given each blocking rule and each protected receipt subject, when a targeted negative mutation is applied, then the named check turns red and fixture restoration is verified.

## Functional Requirements
- FR-001: The generator MUST emit one versioned canonical receipt covering declared rule IDs/metadata, resolved dependencies/supersession, `.fsi`/XML state, semantic evidence, runtime parity, explanations/generated views, manifest/implementation/semantic/replay identities, historical resolution, and production journeys. (Stories: US-001; Acceptance: AC-001)
- FR-002: Receipt bytes and their SHA-256 identity MUST be deterministic, content-addressed, package-qualified, sorted by stable identity where order is not semantic, and derived only from authoritative corpus/test outputs. (Stories: US-001; Acceptance: AC-001)
- FR-003: Required evidence MUST preserve current, missing, malformed, stale, synthetic, unavailable, passing, and failing states explicitly; absence or unreadability MUST become `Unknown` or a visible non-passing finding. (Stories: US-001, US-003; Acceptance: AC-002)
- FR-004: Every migrated executable rule MUST bind real semantic evidence, coverage, explanation, documentation/source, runtime parity, replay, and package identities; synthetic evidence MUST remain visibly non-satisfying where required. (Stories: US-001, US-003; Acceptance: AC-002)
- FR-005: A S.I.R.-owned adapter MUST map the receipt into a closed fact union and a catalog of reified Governance checks without interpreting gameplay formulas. (Stories: US-002; Acceptance: AC-003)
- FR-006: Evaluation, stable rule text, declared artifact reads, structural hashes, explanations, and finding provenance MUST derive from the same check values and remain deterministic across input/catalog order. (Stories: US-002; Acceptance: AC-003)
- FR-007: Initial migration findings for signatures/XML/descriptive metadata/legacy classification MUST be advisory, while duplicate/dangling IDs, invalid types/units, signature/source mismatch, stale views, malformed receipts, and copied non-F# gameplay semantics MUST block on PR. (Stories: US-002, US-003; Acceptance: AC-004)
- FR-008: Missing semantic evidence, runtime divergence, incomplete migrated-rule coverage, package/digest inconsistency, historical substitution, and missing required production journeys MUST block on ship; profiles MAY alter effective enforcement but MUST preserve the underlying verdict. (Stories: US-002, US-003; Acceptance: AC-004)
- FR-009: Canonical .NET, Fable/Node, and browser evidence MUST bind the same rule package and disagreement MUST be a ship-blocking finding. (Stories: US-003; Acceptance: AC-005)
- FR-010: Historical replay MUST resolve only its recorded package identity and MUST report explicit unavailability rather than falling back to the current corpus. (Stories: US-003; Acceptance: AC-005)
- FR-011: SDD ship readiness and rules-governance verdicts MUST remain distinct artifacts joined only by a protected-boundary handoff bound to their exact schema, source, and digest identities. (Stories: US-003; Acceptance: AC-006)
- FR-012: The product MUST consume Governance through exact released packages at tooling/test boundaries only and MUST NOT introduce Governance references into `SIR.Domain`, gameplay evaluation, or replay interpretation. (Stories: US-004; Acceptance: AC-007)
- FR-013: Every blocking check MUST ship with a positive fixture, a protected-subject negative mutation that proves red, and malformed/unreadable fail-closed coverage with verified restoration. (Stories: US-001, US-002, US-003; Acceptance: AC-008)

## Ambiguities
- AMB-001: Which published Governance package surface supports a package-only external domain adapter with the documented SPI and Kernel reified checks.
- AMB-002: Which existing #194 artifacts are authoritative inputs versus generated projections that must be freshness-checked.
- AMB-003: Which exact per-rule evidence state and maturity vocabulary is canonical in `sir-rules-governance/v1`.
- AMB-004: Which command owns generation, evaluation, and the protected-boundary join without adding work to gameplay update paths.
- AMB-005: Which bounded set of executable rules constitutes the initial migrated corpus and which mechanics remain visibly legacy.

## Public Or Tool-Facing Impact
- Introduces a versioned JSON receipt and verdict schema, declared F# generator/adapter surface, stable command output/exit behavior, generated readiness artifacts, policy maturity, CI gate, and migration documentation.
- Existing rule manifest/application/replay schemas remain authoritative and compatible; the new receipt references rather than replaces them.

## Lifecycle Notes
- Clarification and planning are blocked until FS-GG/FS.GG.Governance#413 supplies a package-consumable `Adapters.Spi` surface; using DLL `HintPath` or a sibling project would violate SB-003 and FR-012.
- Next lifecycle action after the producer dependency lands: `fsgg-sdd clarify --work 198-rules-governance-receipts`.
