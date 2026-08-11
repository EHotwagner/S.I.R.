---
schemaVersion: 1
workId: 143-shortcut-command-registry
title: Shortcut Command Registry
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/143-shortcut-command-registry/spec.md
sourceClarifications: work/143-shortcut-command-registry/clarifications.md
sourceChecklist: work/143-shortcut-command-registry/checklist.md
publicOrToolFacingImpact: true
---

# Shortcut Command Registry Plan

Prose status: planned

## Source Snapshot
- spec: work/143-shortcut-command-registry/spec.md sha256:b9ac51abfbedb77327b428e2922a749a997b97bf846e8d9b4f8b39dd707b542e schemaVersion:1
- clarifications: work/143-shortcut-command-registry/clarifications.md sha256:8a1ab3f2b56fe1cd8467a9aeb5eae35ff4174cf49480e1af8293268ed2b6477a schemaVersion:1
- checklist: work/143-shortcut-command-registry/checklist.md sha256:b146f076aad0ba21c40fa39d04c53981274e4c824918b63929feb7764bfd78dc schemaVersion:1

## Plan Scope
- Work item 143-shortcut-command-registry is planned from the current specification, clarification, and checklist facts.
- Requirement count: 5.
- Clarification decision count: 2.
- Checklist result count: 5.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: Extend `UnifiedTacticalWorkspace` with registry-owned effective display and ARIA shortcut formatting, retaining `None` as the explicit unassigned representation.
- PD-002 [AC-002] [FR-002] [DEC-001] [DEC-002] complete: Add a shared browser command-control renderer that preserves the command label, adds a visible `kbd` label when space permits, and exposes a registry-derived accessible label, title, and `aria-keyshortcuts`.
- PD-003 [AC-003] [FR-003] [DEC-001] complete: Route representative tactical command buttons through their command id so the dispatched action and displayed effective binding share one registry lookup.
- PD-004 [AC-004] [FR-004] [DEC-002] complete: Render customized bindings and unassigned state exclusively through `effectiveGesture`; do not cache labels in component state or duplicate modifier conversion.
- PD-005 [AC-005] [FR-005] [DEC-001] complete: Add focused F# regression tests for registry formatting and browser tests that inspect production controls, custom bindings, ARIA metadata, and keyboard activation.

## Contract Impact
- PC-001 [PD-001] [PD-002] publicSurface: Declare the formatter and command-presentation surface in the existing `UnifiedTacticalWorkspace` signature before implementation; browser DOM attributes are the externally observable UI contract.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-003] [PD-004] [PD-005] [PC-001] semanticTest: Run focused client tests and Playwright browser tests; invert each added shortcut expectation first and record its observed red result before restoring the implementation.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] compatible: Existing persisted `TacticalBindingProfile` schema remains version 1; missing overrides retain default registry bindings and explicit `None` remains unassigned.

## Generated View Impact
- GV-001 [PD-001] [PD-005] workModel: Refresh SDD readiness views after authored lifecycle changes; browser test output and evidence declarations bind the final implementation head.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 143-shortcut-command-registry`.
