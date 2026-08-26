---
schemaVersion: 1
workId: 353-quint-q1-sir-replay
title: Quint Q1 Sir Replay
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/353-quint-q1-sir-replay/spec.md
sourceClarifications: work/353-quint-q1-sir-replay/clarifications.md
sourceChecklist: work/353-quint-q1-sir-replay/checklist.md
publicOrToolFacingImpact: true
---

# Quint Q1 Sir Replay Plan

Prose status: planned

## Source Snapshot
- spec: work/353-quint-q1-sir-replay/spec.md sha256:010e40ea1422b7bad084330b62d2fd89875100c4b31b3b16a3b5e9bf5c026547 schemaVersion:1
- clarifications: work/353-quint-q1-sir-replay/clarifications.md sha256:5231406d3d817d66871cda6b3811f1cd09f80971061131f4ac862c58205b2a81 schemaVersion:1
- checklist: work/353-quint-q1-sir-replay/checklist.md sha256:60880ce36036d6144bcb4e3604996ee83e8cedd8db806ea4fbd8bf35d80f0774 schemaVersion:1

## Plan Scope
- Work item 353-quint-q1-sir-replay is planned from the current specification, clarification, and checklist facts.
- Requirement count: 4.
- Clarification decision count: 0.
- Checklist result count: 4.

## Plan Decisions
- PD-001 [AC-001] [FR-001] complete: Add a native-only shared conformance fixture that parses the exact committed Q1 replay envelope, maps `Initialize` and `ApplyDamage(amount)` to a thin adapter over `CombatRules.resolveConsequences`, and compares only `hitPoints`, `lastAction`, and `lastAmount` after each transition.
- PD-002 [AC-002] [FR-002] complete: Commit the exact producer-derived Quint module and make the selected conformance route download only pinned Quint/evaluator artifacts, generate 64 seeded traces with eight steps, normalize their ITF states, and replay them through the same production adapter; retain an unrelated-path skip control.
- PD-003 [AC-003] [FR-003] complete: Keep mutations outside the adapter's model expectation logic, inject the combat-boundary defect through a resolver seam around the real interpreter, and emit fixture/JSON-pointer/adapter/implementation locations for the first divergence.
- PD-004 [AC-004] [FR-004] complete: Resolve and hash the executed .NET muxer, SDK `dotnet.dll`, hostfxr, runtime tree, combined package locks, pinned model tools, model, adapter, implementation, corpus, seed, bounds, and JUnit output into compact receipts.

## Contract Impact
- PC-001 [PD-001] fixture contract: The committed producer receipt, replay envelope, normalized ITF, adapter/implementation source digests, seed, and bounds form one fail-closed correspondence receipt; no production API or package surface changes.
- PC-002 [PD-002] [PD-004] sampled qualification contract: The selected CI route, pinned tool downloads, generated corpus, runtime closure, and compact receipts are one fail-closed contract; missing files, moving versions, digest drift, incomplete replay, or an unexpected route selection fail qualification.

## Verification Obligations
- VO-001 [PD-001] [PC-001] semanticTest: Run the exact witness successfully, then require wrong action mapping, omitted action, wrong observable field, stale expected state, and bypassed combat-boundary mutations to fail with the first transition index and stable action name.
- VO-002 [PD-002] [PC-002] semanticTest: Assert the live CI route reads the route artifact, selects a bound combat path, skips an unrelated path, generates at least 64 pinned model traces, and replays every normalized state.
- VO-003 [PD-003] [PC-001] negativeControl: Require all five independent mutations to fail for the intended assertion and require each divergence to carry transition, action, fixture, JSON pointer, adapter source, and implementation source.
- VO-004 [PD-004] [PC-002] integrityTest: Recompute every runtime, model-tool, source, corpus, and result digest and reject omission or substitution of any closure member.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] testOnly: Keep the adapter and corpus non-authoritative and test-only; Q4 alone may migrate canonical combat authority or add native/Fable/package/rollback obligations.

## Generated View Impact
- GV-001 [PD-001] [PD-002] [PD-003] [PD-004] evidenceViews: Refresh only the SDD-owned analysis, evidence, verify, and ship views after exact and sampled replay evidence is observed.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 353-quint-q1-sir-replay`.
