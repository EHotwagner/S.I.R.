---
schemaVersion: 1
workId: 382-handbook-roadmap-rollup
title: Handbook Roadmap Rollup
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/382-handbook-roadmap-rollup/spec.md
sourceClarifications: work/382-handbook-roadmap-rollup/clarifications.md
sourceChecklist: work/382-handbook-roadmap-rollup/checklist.md
publicOrToolFacingImpact: true
---

# Handbook Roadmap Rollup Plan

Prose status: planned

## Source Snapshot
- spec: work/382-handbook-roadmap-rollup/spec.md sha256:43bce9d48150ca86e4af21fb6f58eab2a2aa248521f82507c1d4a23904f906a3 schemaVersion:1
- clarifications: work/382-handbook-roadmap-rollup/clarifications.md sha256:7de53340b13604d5c8fec01805c18e516f8ee67dcc6b1dc8c5de6fbc8a84c3a0 schemaVersion:1
- checklist: work/382-handbook-roadmap-rollup/checklist.md sha256:ad0ffdd244f3e881c0c16da6bdff54084c07931c0af471204bc48f02a810a86c schemaVersion:1

## Plan Scope
- Add a dependency-free Node audit and shell qualifier under `work/382-handbook-roadmap-rollup/`, one public Markdown report under `docs/`, and a roadmap link; leave M7's exact-source maintenance page unchanged.
- Parse prior feedback as immutable inputs. Do not edit the twelve cycle artifacts or add a thirteenth terminal cycle.
- Retain one aggregate JUnit receipt for SDD evidence plus a machine-readable qualification summary.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: Enumerate checkpoint filenames and independently parse report front matter, then compare both sets and report rows exactly before accepting the observed twelve-cycle count.
- PD-002 [AC-002] [FR-002] complete: The qualifier invokes the existing three fail-closed validators for each discovered cycle using its uniquely bound report, audit, checkpoint stream, and report-declared phases.
- PD-003 [AC-003] [FR-003] [DEC-003] complete: Render one checkpoint table row per JSONL line, keyed by cycle plus one-based sequence, and compare phase, kind, exact summary, exact evidence, disposition vocabulary, and total cardinality.
- PD-004 [AC-004] [FR-004] [DEC-002] complete: Build a one-row-per-cycle matrix with exact artifact links and evidence cells that either name retained identifiers or say `not retained`; never infer successful delivery.
- PD-005 [AC-005] [FR-005] complete: Implement self-test fixtures as temporary in-memory/source-tree variants and require detector-specific red for each of the six named defects before the pristine tree restores green.
- PD-006 [AC-006] [FR-006] complete: Parse roadmap milestone headings structurally, require the ten exact ids once each, and require `[x]` rather than relying on a count of arbitrary checked boxes.
- PD-007 [AC-007] [FR-007] complete: Link the final report from the roadmap, preserve M7's sealed maintenance blob, run focused audit plus strict docs, and retain hosted PR/main/Pages/live identities at the delivery boundary.
- PD-008 [AC-008] [FR-008] [DEC-004] complete: Reject any terminal-cycle declaration or new matching feedback artifact; the final report describes prior cycles only and has no activation envelope.

## Contract Impact
- PC-001 [PD-001] [PD-003] report contract: `docs/2026-08-28-sir-combat-quint-handbook-roadmap-final-report.md` has exact summary totals, cycle matrix, and one keyed disposition row per checkpoint.
- PC-002 [PD-002] [PD-005] qualification contract: `work/382-handbook-roadmap-rollup/audit-roadmap-rollup.mjs` validates a supplied root and `--self-test` proves all six negative routes; the shell owner emits JUnit only after audit, feedback validators, and strict docs pass.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PD-003] [PD-004] [PD-006] [PD-008] semanticTest: On untouched input, prove exactly twelve derived cycles, forty-eight records, ten checked milestone headings, one unique report/audit binding per cycle, and one exact disposition per checkpoint.
- VO-002 [PD-005] [PC-002] mutationTest: Observe named red for omitted cycle, omitted checkpoint, wrong report/audit binding, count mismatch, invalid disposition, and unchecked milestone, then restore untouched green.
- VO-003 [PD-007] documentationTest: Run the complete strict documentation build and verify both new navigation links resolve in source and rendered output.
- VO-004 [PD-007] deliveryTest: Obtain independent content/feedback-coverage and exact-head implementation acceptance, relevant-only hosted PR CI, guarded merge, exact-main CI, and exact-SHA Pages/live proof.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additive: The report and audit are additive schema-free documentation/test surfaces; prior feedback schemas and handbook semantics remain byte-for-byte inputs, with no migration or compatibility shim.

## Generated View Impact
- GV-001 [PD-002] [PD-005] workModel: lifecycle analysis, aggregate JUnit import, verify, and ship generate only item-scoped views under `readiness/382-handbook-roadmap-rollup/`; stale source digests must fail closed.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 382-handbook-roadmap-rollup`.
