---
schemaVersion: 1
workId: 366-main-ci-routing
title: Main Ci Routing
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/366-main-ci-routing/spec.md
sourceClarifications: work/366-main-ci-routing/clarifications.md
sourceChecklist: work/366-main-ci-routing/checklist.md
publicOrToolFacingImpact: true
---

# Main Ci Routing Plan

Prose status: planned

## Source Snapshot
- spec: work/366-main-ci-routing/spec.md sha256:4e06af2ee12524a10f0649d30e32f1e5c9f86f575573f32c1ae18c055270d131 schemaVersion:1
- clarifications: work/366-main-ci-routing/clarifications.md sha256:8ecef7cbfc257e0930843aa4c8636e17339a4fd78da13c0d58695bce057f5121 schemaVersion:1
- checklist: work/366-main-ci-routing/checklist.md sha256:550ad2b58ff0ff5e251c8283df973f5e92d4a5bc9cf5830006ff59dd45d7785d schemaVersion:1

## Plan Scope
- Work item 366-main-ci-routing is planned from the current specification, clarification, and checklist facts.
- Requirement count: 8.
- Clarification decision count: 3.
- Checklist result count: 8.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Generalize the current route job to pull requests and pushes; checkout the event's exact head and generate the path inventory from PR base/head or push `HEAD^..HEAD` without a fallback that can disguise an empty or missing parent diff.
- PD-002 [AC-001] [FR-002] complete: Reuse the existing route outputs, producer jobs, helper jobs, routed gate jobs, and deterministic result-directory join for both focused event types; retain the integrity floor unconditionally.
- PD-003 [AC-002] [FR-003] complete: Keep `.github`, `.fsgg`, route/receipt/workflow scripts, mixed classifications, and unknown paths on `cross-cutting`, whose selected gate set is the complete routed surface.
- PD-004 [AC-002] [FR-004] complete: Extend `protected-stage-receipt.mjs` to a versioned manifest-driven join with `focused` and `complete` modes. Focused mode verifies the routed join receipt; complete mode verifies preflight/core. Both bind commit/tree/event and reject all non-pass or malformed inputs.
- PD-005 [AC-003] [FR-005] complete: Restrict protected preflight/core jobs to schedule and workflow dispatch. Preserve their commands, subjects, artifact retention, and clean-room semantics unchanged.
- PD-006 [AC-004] [FR-006] complete: On a routed main push with documentation selected, package the already-built documentation site plus its immutable build receipts, route receipt, and documentation gate receipt; publish no site artifact for unrelated routes.
- PD-007 [AC-004] [FR-007] complete: Keep Pages deploy-only and bind it to the triggering run/head. Its job condition uses an explicit CI-produced deployability artifact/manifest rather than success alone, and verification rejects route, source, or receipt drift.
- PD-008 [AC-005] [FR-008] complete: Expand pure route/workflow contract tests, protected receipt mutation tests, Pages handoff tests, and isolated shell mutation controls; preserve current PR fixtures and run the existing full CI contract suite.

## Contract Impact
- PC-001 [PD-001] [PD-004] [PD-006] workflow contract: `.github/workflows/ci.yml`, `.github/workflows/pages.yml`, `scripts/ci-route.mjs`, and `scripts/protected-stage-receipt.mjs` jointly own event routing, required receipts, protected verdict, and site handoff. Existing PR route and gate schemas remain compatible; protected join is versioned when its expected-stage semantics change.

## Verification Obligations
- VO-001 [PD-001] [PD-008] [PC-001] semanticTest: Prove documentation, domain, browser, evidence-only, unknown, mixed, and CI-contract route fixtures for PR and main; mutate exact diff/source, required receipt presence/status/schema/digest/binding, event mode, site eligibility, and Pages source binding; restore every fixture; run route/protected/Pages tests plus strict docs and lifecycle validation.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] versionedContract: Existing PR `sir.ci-route/v2`, `sir.ci-gate-result/v1`, and `sir.ci-join/v1` inputs remain readable. The protected manifest/join change receives a new schema version, and malformed or older incompatible protected inputs fail with an actionable code rather than being inferred.

## Generated View Impact
- GV-001 [PD-008] workModel: SDD refresh regenerates `readiness/366-main-ci-routing/work-model.json` and agent guidance from the authored lifecycle sources; tests and receipts are implementation evidence, not alternate generated authorities.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 366-main-ci-routing`.
