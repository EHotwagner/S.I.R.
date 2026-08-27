---
schemaVersion: 1
workId: 365-handbook-m4
title: Handbook M4
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/365-handbook-m4/spec.md
sourceClarifications: work/365-handbook-m4/clarifications.md
sourceChecklist: work/365-handbook-m4/checklist.md
publicOrToolFacingImpact: true
---

# Handbook M4 Plan

Prose status: planned

## Source Snapshot
- spec: work/365-handbook-m4/spec.md sha256:f5799562481b52433e1f3d399a40a88fa9a522cff2d456908673ef324eb39ef0 schemaVersion:1
- clarifications: work/365-handbook-m4/clarifications.md sha256:65838aa7aae9af2982692444f60a82b1238ed892d355517ca2002bdf90662097 schemaVersion:1
- checklist: work/365-handbook-m4/checklist.md sha256:10f69c1cdbd69b6afbc6428d1d0c482d39d67896495c836dcc732f5e8ac10a9c schemaVersion:1

## Plan Scope
- Edit the existing handbook in place, preserving all M1-M3 anchors and content while filling only the
  M4 formal-reasoning and mutation-laboratory chapters.
- Add a dedicated Node audit and shell qualification that extract the literate Quint module into
  temporary fixtures, run six single-defect mutations through named detectors, confirm observed red,
  and confirm untouched-authority restored green.
- Mechanically audit three major-action witnesses, every required invariant's state/observation fields,
  formal claim labels/limitations, strict docs/links, full Q4/runtime regression, lifecycle currency,
  and the merge-boundary roadmap ledger.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-002] complete: Add a claim-strength ladder and comparison table whose mechanically required scope phrases distinguish examples, witnesses, invariants, sampled runs, bounded exhaustive checks, and counterexamples.
- PD-002 [AC-002] [FR-002] [DEC-002] complete: Teach nondeterministic `step` traces as valid choices among guarded actions, using two authoritative action paths and state-delta questions while declaring every shown trace non-canonical.
- PD-003 [AC-003] [FR-003] complete: Add a counterexample workflow from property and earliest divergent state to action/input/guard and authority seam, with one bounded worked failure and no invented intermediate state.
- PD-004 [AC-004] [FR-004] [DEC-001] [DEC-003] complete: Generate six ephemeral single-defect fixtures—threshold, bounds, suppression, cover, collateral, and catalogue integrity—and require named observed-red/restored-green result pairs from the same focused detector.
- PD-005 [AC-005] [FR-005] [DEC-003] complete: Use authoritative named runs/observations to prove one reachable witness each for `resolveConsequences`, `resolveCoverImpact`, and `resolveRecovery`, checking action identity and state delta.
- PD-006 [AC-006] [FR-006] [DEC-003] complete: Publish an invariant/state-binding table derived from the authoritative property catalogue and reject missing or unknown state/observation-field bindings.
- PD-007 [AC-007] [FR-007] complete: Own separate docs, links, focused M4, full Q4/runtime, roadmap-ledger, and lifecycle receipts plus one aggregate receipt that names but does not blur the component claims.
- PD-008 [AC-008] [FR-008] complete: Mark only M4 checked at the merge-candidate boundary, append concise evidence with explicit PR/merge/post-merge conditions, and mechanically preserve M5/M6/M6V/M7 unchecked.

## Contract Impact
- PC-001 [PD-001] [PD-004] [PD-007] documentationContract: `docs/sir-combat-quint-handbook.md`, `docs/sir-combat-quint-handbook-roadmap.md`, `work/365-handbook-m4/*.mjs`, qualification commands, and scoped JUnit receipts are additive public documentation/evidence surfaces; no code API or schema changes.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-003] [PD-004] [PD-005] [PD-006] [PD-007] [PD-008] [PC-001] semanticTest: Run six mutation red/green controls, formal-claim and state-binding structural controls, authoritative action witnesses, strict docs/links, full Q4/runtime regression, roadmap ledger, lifecycle currency, and independent exact-head review before merge.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] compatibility: Existing handbook anchors and M1-M3 prose remain stable; M4 fills scheduled chapters and adds additive qualification/lifecycle artifacts only.

## Generated View Impact
- GV-001 [PD-007] [PD-008] workModel: `readiness/365-handbook-m4/` is regenerated from current lifecycle sources and qualification runs; stale work-model, analysis, verify, ship, or scoped receipts block handoff.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.
- No framework API or production performance surface is introduced; M6V owns visual render and performance qualification.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 365-handbook-m4`.
