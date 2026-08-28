---
schemaVersion: 1
workId: 380-handbook-m7
title: Handbook M7
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/380-handbook-m7/spec.md
sourceClarifications: work/380-handbook-m7/clarifications.md
sourceChecklist: work/380-handbook-m7/checklist.md
publicOrToolFacingImpact: true
---

# Handbook M7 Plan

Prose status: planned

## Source Snapshot
- spec: work/380-handbook-m7/spec.md sha256:94375a7afca44a81a3c8748df5bb9c223e3a0e3fd4820bf7f98703a3d58119fb schemaVersion:1
- clarifications: work/380-handbook-m7/clarifications.md sha256:0513af5234f94e9427e1efdd6fdd5ea61b4857cd6de608d33b9449cee17f2ded schemaVersion:1
- checklist: work/380-handbook-m7/checklist.md sha256:d9c5b24ded14e14dbec8392daa8b9109178f6b87db2c8aab31fb63acbe098efb schemaVersion:1

## Plan Scope
- Work item 380-handbook-m7 is planned from the current specification, clarification, and checklist facts.
- Requirement count: 8.
- Clarification decision count: 3.
- Checklist result count: 8.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: Commit exact-source structured domain and Quint/modeling approvals, then mechanically validate their scope, verdict, reviewer independence, and boundaries.
- PD-002 [AC-002] [FR-002] [DEC-001] complete: Commit a beginner approval produced by executing the rendered setup-to-explanation path and checking command/output/link expectations.
- PD-003 [AC-003] [FR-003] [DEC-001] complete: Commit rendered approval over the six assets and retained modes; rerun the existing browser inspection and accessibility/fallback checks.
- PD-004 [AC-004] [FR-004] [DEC-002] complete: Generate a schema-v1 publication record from exact Git blobs/trees and pinned tool manifests; compare actual tool versions when qualification runs.
- PD-005 [AC-005] [FR-005] [DEC-002] complete: Add a model-adjacent maintenance trigger naming S.I.R. documentation ownership, triggering paths, ordered gates, review roles, and publication obligations; mirror the actionable checklist in handbook chapter 49.
- PD-006 [AC-006] [FR-006] [DEC-003] complete: Invoke M6 structure/link audit and M6V visual qualification under their existing contracts, checking exact evidence source binding and unchanged claim limits.
- PD-007 [AC-007] [FR-007] complete: Build one M7 audit with isolated temporary mutations for missing/rejected reviews, stale identities, missing owner trigger, broken M6/M6V links, and weakened claim limits; require named red then untouched green.
- PD-008 [AC-008] [FR-008] complete: Aggregate strict docs, reviews, existing M6/M6V gates, lifecycle, feedback, ledger, exact-head CI, merge, exact-main CI, Pages/live content, and Done evidence without performing the cross-cycle roll-up.

## Contract Impact
- PC-001 [PD-001] [PD-002] [PD-003] reviewManifest: `work/380-handbook-m7/publication-reviews.json` schema v1 records four scoped approvals; it carries review evidence only.
- PC-002 [PD-004] [PD-005] publicationRecord: `work/380-handbook-m7/publication-record.json` schema v1 records exact source/tool identity, owner, triggers, ordered gates, and evidence bindings; semantic sources remain authoritative.
- PC-003 [PD-006] [PD-007] qualification: `work/380-handbook-m7/qualify-handbook-m7.sh` and `audit-publication-handoff.mjs` fail closed on drift and write focused JUnit/JSON receipts.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-003] [PC-001] reviewTest: Validate four exact-source, independently produced approvals and rerun their cited domain/model/beginner/render routes.
- VO-002 [PD-004] [PD-005] [PC-002] identityTest: Recompute every source/tool/owner/trigger field and reject staleness or missing ordered maintenance obligations.
- VO-003 [PD-006] [PC-003] regressionTest: Run strict docs, M6 structure/link enforcement, and the existing M6V SVG/accessibility/fallback/render/performance qualification with original workload and limits.
- VO-004 [PD-007] [PC-003] mutationTest: Observe red for each new gate by changing its claimed subject, then restore untouched green.
- VO-005 [PD-008] [PC-003] lifecycleTest: Run SDD analyze/evidence/verify/ship, feedback validators, roadmap audit, relevant hosted CI, merge, exact-main CI, exact-SHA Pages/live content, and Done verification.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additive: Existing handbook anchors, model declarations, M6 definition/link enforcement, and M6V diagram contracts remain byte-compatible; M7 adds review/publication evidence and owner maintenance guidance only.

## Generated View Impact
- GV-001 [PD-008] lifecycleViews: `readiness/380-handbook-m7/` contains current analysis, qualification, verify, ship, and compact ship verdict bound to exact authored sources.
- GV-002 [PD-001] [PD-002] [PD-003] reviewReceipt: focused approval/audit receipts are generated only from committed review inputs and exact-source replay.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 380-handbook-m7`.
