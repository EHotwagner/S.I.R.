---
schemaVersion: 1
workId: 198-rules-governance-receipts
title: Rules Governance Receipts
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

# Rules Governance Receipts Charter

## Identity
Define S.I.R.'s deterministic `sir-rules-governance/v1` receipt and a product-owned adapter that turns the executable rules corpus into explainable protected-boundary policy inputs.

## Principles
- The executable F# corpus remains the sole gameplay authority; Governance inspects receipts and never enters runtime evaluation.
- Every receipt is deterministic, content-addressed, package-qualified, and fail-closed for malformed, missing, stale, synthetic, or unavailable evidence.
- SDD readiness and Governance verdicts remain separate artifacts joined only at the protected boundary.
- Governance dependencies are exact released packages only; producer capability gaps belong on the producer's coordination board.

## Scope Boundaries
- In: a versioned receipt schema, canonical encoder/decoder, closed S.I.R. fact union, reified product checks, maturity policy, deterministic generator, negative mutations, cross-runtime and historical-package evidence, CI integration, documentation, and lifecycle evidence.
- Out: replacing `FormulaExpr`, transitions, or `RuleApplication`; interpreting gameplay formulas in Governance; adding Governance runtime semantics to `SIR.Domain`; migrating every legacy mechanic; or inventing a shared cross-product rules algebra.

## Policy Pointers
- Constitution II requires the receipt and verdict projections to be versioned machine contracts; III requires declared `.fsi` surfaces; VI requires positive and protected-subject negative evidence; VIII requires explicit unavailable/unknown outcomes.
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`; Governance owns protected-boundary enforcement through its package-only tooling/configuration edge.
- Issue #194's rule manifest, coverage graph, application/replay contracts, and package identities are authoritative inputs rather than representations to duplicate.

## Lifecycle Notes
- Tier 1: this introduces a schema, declared F# adapter/generator surface, generated receipt/verdict views, CI gate, documentation, and migration maturity.
- The current published Governance CLI exposes built-in adapters but no package-referenceable custom-adapter SPI; the plan must keep the S.I.R. adapter product-owned and use supported declarative/reified-check inputs without linking gameplay runtime code to Governance assemblies.
- Next lifecycle action: `fsgg-sdd specify --work 198-rules-governance-receipts`.
