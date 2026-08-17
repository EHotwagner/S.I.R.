---
schemaVersion: 1
workId: 198-rules-governance-receipts
title: Rules Governance Receipts
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/198-rules-governance-receipts/spec.md
sourceClarifications: work/198-rules-governance-receipts/clarifications.md
sourceChecklist: work/198-rules-governance-receipts/checklist.md
publicOrToolFacingImpact: true
---

# Rules Governance Receipts Plan

Prose status: planned

## Source Snapshot
- spec: work/198-rules-governance-receipts/spec.md sha256:e64e6aaf1b7c556703ccde8d8c7de75a7975f50f8daf6b2e13af166e5094a3a8 schemaVersion:1
- clarifications: work/198-rules-governance-receipts/clarifications.md sha256:7ab97d155eed4dbe6720622abe4ea6839d66681ea712e999d69f8ba4dcf7c213 schemaVersion:1
- checklist: work/198-rules-governance-receipts/checklist.md sha256:eca78c08cdea8fcbe636c115d7661b784dbb3a98f6bbfab25dfc935ae12d69f5 schemaVersion:1

## Plan Scope
- Work item 198-rules-governance-receipts is planned from the current specification, clarification, and checklist facts.
- Requirement count: 13.
- Clarification decision count: 5.
- Checklist result count: 13.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-002] complete: Declare `RuleGovernance.fsi` receipt records for every required corpus, surface, evidence, parity, package, replay, view, and journey fact, and a JSON envelope tagged `sir-rules-governance/v1`.
- PD-002 [AC-001] [FR-002] complete: Canonically encode ordinal-sorted payload fields with `Utf8JsonWriter`, hash only those payload bytes with SHA-256, and store that lowercase digest beside the payload so decoding can recompute it.
- PD-003 [AC-002] [FR-003] [DEC-003] complete: Parse evidence and maturity through closed discriminated unions; malformed JSON or unknown tokens return typed errors, while missing/unavailable inputs become facts whose probes return `Unknown`.
- PD-004 [AC-002] [FR-004] [DEC-002] [DEC-005] complete: Generate the production receipt from all 16 registry rules plus v2 manifest/coverage/source fixtures and named semantic, parity, replay, public-surface, generated-view, and browser-journey evidence paths.
- PD-005 [AC-003] [FR-005] [DEC-001] complete: Implement a standalone `SIR.Rules.Governance` project whose closed `GovernanceFact` union and total `Adapter<GovernanceFact,GovernedArtifact,GovernedChange>` use only exact package APIs and never interpret `FormulaExpr`.
- PD-006 [AC-003] [FR-006] complete: Build each finding from one `Check` value and persist its `Check.render`, `Check.hash`, `Check.reads`, `Check.eval`, and `Check.explain` result plus receipt provenance in deterministic rule-id order.
- PD-007 [AC-004] [FR-007] [DEC-003] complete: Catalog warn checks for signature/XML/metadata/legacy classification and PR checks for identity, type/unit, surface/source, generated-view, receipt-shape, and non-F# semantics; route PR blockers only at the declared fenced Gate boundary.
- PD-008 [AC-004] [FR-008] [DEC-003] complete: Catalog ship checks for semantic evidence, migrated coverage, parity, package/digest, historical resolution, and production journeys; profiles may demote effective enforcement but the serialized underlying check verdict and declared maturity never change.
- PD-009 [AC-005] [FR-009] complete: Require .NET, Fable/Node, and browser evidence to carry the same package manifest/semantic identities and emit a ship-blocking mismatch finding.
- PD-010 [AC-005] [FR-010] complete: Model historical replay availability separately from current receipt identity and require exact recorded manifest resolution; unavailable recorded packages remain unknown/non-passing with no current fallback.
- PD-011 [AC-006] [FR-011] [DEC-004] complete: Emit a `sir-rules-protected-boundary/v1` result that records independent SDD ship and governance verdict artifact paths/digests plus a joined decision without changing either input artifact.
- PD-012 [AC-007] [FR-012] [DEC-001] [DEC-004] complete: Reference Governance packages only from the standalone adapter/tool/test projects, add an architecture scan rejecting Governance references from gameplay project files, and lock public NuGet resolution.
- PD-013 [AC-008] [FR-013] complete: Add focused positive, permutation, malformed/unreadable, and one-protected-subject mutation per blocking check family; the mutation harness must observe red and verify restoration.

## Contract Impact
- PC-001 [PD-001] [PD-002] schema: `sir-rules-governance/v1` is a S.I.R.-owned canonical JSON receipt envelope and `sir-rules-governance-verdict/v1` is its explainable evaluation projection.
- PC-002 [PD-005] publicSurface: `src/SIR.Domain/Governance/RuleGovernance.fsi` declares the receipt, adapter, evaluation, encoding, and protected-boundary APIs before implementation.
- PC-003 [PD-005] framework: FS.GG.Governance.Adapters.Spi@0.1.1#Adapter`3
- PC-004 [PD-005] framework: FS.GG.Governance.Kernel@0.1.1#CheckModule.probe
- PC-005 [PD-011] command: `scripts/generate-rules-governance.sh --check|--write` owns deterministic production generation and refuses stale generated artifacts.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-003] receiptTests: Focused F# tests prove byte stability, order independence, content-address verification, closed-state round trips, and malformed/unreadable fail-closed behavior.
- VO-002 [PD-005] [PD-006] adapterTests: Package-only tests instantiate the total adapter, evaluate the same reified checks used for render/hash/reads/explain, and verify finding provenance.
- VO-003 [PD-007] [PD-008] [PD-009] [PD-010] policyTests: Positive fixtures and subject mutations prove every PR/ship blocker family red, profile demotion preserves underlying verdicts, runtime divergence blocks, and historical substitution is impossible.
- VO-004 [PD-011] [PD-012] boundaryTests: The generation script checks deterministic fixtures and distinct SDD/Governance inputs; architecture tests reject Governance package references in `SIR.Domain.fsproj` and `SIR.Simulation.fsproj`.
- VO-005 [PD-004] productionEvidence: One source-frozen aggregate produces real semantic, .NET/Fable/Node/browser, replay, package, journey, and public-surface evidence; metadata-only lifecycle sealing reuses its bound receipt.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additiveVersioned: Existing corpus/application/replay schemas remain authoritative; the v1 governance receipt references them by identity and can add only versioned successor schemas.
- PM-002 [PD-007] graduated: Descriptive/public-surface gaps and the bounded legacy classification begin at `warn`; deterministic integrity failures block PR and absent behavioral/release evidence blocks ship.

## Generated View Impact
- GV-001 [PD-004] receipt: `readiness/198-rules-governance-receipts/rules-governance.json` is regenerated from authoritative artifacts and byte-compared in check mode.
- GV-002 [PD-006] verdict: `readiness/198-rules-governance-receipts/rules-governance-verdict.json` is derived only from the verified receipt and exact check catalog.
- GV-003 [PD-011] handoff: `readiness/198-rules-governance-receipts/protected-boundary.json` names distinct current SDD ship and Governance verdict identities.
- GV-004 [PC-001] workModel: SDD work model, verify, ship, and agent guidance refresh after authored lifecycle sources change.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 198-rules-governance-receipts`.
