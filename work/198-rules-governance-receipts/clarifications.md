---
schemaVersion: 1
workId: 198-rules-governance-receipts
title: Rules Governance Receipts
stage: clarify
changeTier: tier1
status: clarified
sourceSpec: work/198-rules-governance-receipts/spec.md
publicOrToolFacingImpact: true
---

# Rules Governance Receipts Clarifications

## Source Specification
- work/198-rules-governance-receipts/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001] blocking open: Resolve source ambiguity AMB-001 before checklist.
- CQ-002 [AMB:AMB-002] blocking open: Resolve source ambiguity AMB-002 before checklist.
- CQ-003 [AMB:AMB-003] blocking open: Resolve source ambiguity AMB-003 before checklist.
- CQ-004 [AMB:AMB-004] blocking open: Resolve source ambiguity AMB-004 before checklist.
- CQ-005 [AMB:AMB-005] blocking open: Resolve source ambiguity AMB-005 before checklist.

## Answers
- CQ-001 -> `FS.GG.Governance.Adapters.Spi` 0.1.1 is the supported package-only adapter surface and brings the compatible Kernel 0.1.1 transitively; S.I.R. pins both exact identities through Central Package Management and does not consume tool-bundled assemblies.
- CQ-002 -> The current v2 rule manifest, coverage graph, implementation-source receipt, canonical application/replay fixtures, executable conformance reports, browser journey report, public `.fsi`/XML outputs, and SDD ship handoff are authoritative inputs. The governance receipt is a freshness-checked projection over them, never a substitute.
- CQ-003 -> Evidence uses the closed states `current-pass`, `current-fail`, `missing`, `malformed`, `stale`, `synthetic`, and `unavailable`; check maturity is `warn`, `block-on-pr`, or `block-on-ship`. Any unrecognized serialized value is malformed and evaluates to unknown/non-passing.
- CQ-004 -> A standalone S.I.R. rules-governance library and command project own receipt generation, adapter evaluation, verdict serialization, and protected-boundary joining. They reference `SIR.Domain`/`SIR.Simulation`; neither gameplay project references them, so update/replay execution remains Governance-free.
- CQ-005 -> The initial migrated set is exactly the 16 rules in `CombatRules.registry` and the v2 corpus fixtures. Mechanics outside the coverage graph retain its explicit `legacy` authority classification and produce advisory migration findings rather than being silently treated as migrated.

## Decisions
- DEC-001 [CQ-001] [AMB:AMB-001] [FR-005] [FR-012]: Consume `FS.GG.Governance.Adapters.Spi` and `FS.GG.Governance.Kernel` at exact version 0.1.1 only in the standalone governance tool boundary.
- DEC-002 [CQ-002] [AMB:AMB-002] [FR-001] [FR-004] [FR-011]: Sense only the named #194 corpus/evidence artifacts and the SDD handoff; bind every input by schema, path, and digest in the receipt.
- DEC-003 [CQ-003] [AMB:AMB-003] [FR-003] [FR-007] [FR-008]: Use the closed evidence-state and maturity vocabularies above, preserving unknown/malformed states and the underlying verdict when a profile changes effective enforcement.
- DEC-004 [CQ-004] [AMB:AMB-004] [FR-006] [FR-011] [FR-012]: Keep all Governance package references in standalone library/command/test projects and join the separately generated SDD and Governance artifacts only in a protected-boundary result.
- DEC-005 [CQ-005] [AMB:AMB-005] [FR-004] [FR-008]: Govern all 16 current executable registry rules; retain a visible advisory legacy classification for everything outside the v2 coverage boundary.

## Accepted Deferrals
- None.

## Remaining Ambiguity
- None.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 198-rules-governance-receipts`.
