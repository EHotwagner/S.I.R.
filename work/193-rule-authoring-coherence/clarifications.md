---
schemaVersion: 1
workId: 193-rule-authoring-coherence
title: Rule Authoring Coherence
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/193-rule-authoring-coherence/spec.md
publicOrToolFacingImpact: true
---

# Rule Authoring Coherence Clarifications

## Source Specification
- work/193-rule-authoring-coherence/spec.md

## Clarification Questions
- **CQ-001** (AMB-001): Which honest first analyzer slice fits the metadata #194 already exposes?
- **CQ-002** (AMB-002): Where should the executable command live?
- **CQ-003** (AMB-003): How should cache persistence and cancellation remain deterministic and portable?
- **CQ-004** (AMB-004): Which first-slice findings block canonicalization?
- **CQ-005** (AMB-005): How can forward tests judge skills independently rather than trust their prose?

## Answers
- CQ-001 → implement complete structural registry/reference/status/unit/source/history checks plus dependency-cone and transition interaction checks derived from existing `Dependencies`, `Phase`, `Reads`, `Effects`, and `Events`; report opaque algorithm interactions without sufficient summaries as `unknown`.
- CQ-002 → expose the analyzer in `SIR.Domain` and a `sir-rules check` command in `SIR.Tools`; widen the declared touch-set to `src/SIR.Tools` before editing rather than hiding executable code in a test harness.
- CQ-003 → make one invocation pure over an explicit request plus rules/package identity, return a deterministic report and caller-owned cache snapshot, and model cancellation as an explicit maximum-work budget. Disk persistence and process cancellation stay at the command edge.
- CQ-004 → malformed identity/reference/unit/status/source/history facts and reproducible transition conflicts block. Unsupported opaque interaction analysis remains explicit `unknown`; canonical rules block on unknown only when their declared policy requires a strength the first slice cannot establish.
- CQ-005 → validate package structure mechanically, then run deterministic fixture journeys through the real command: one successful authoring transcript contract and adversarial changed/cone/corpus mutations whose expected finding is kept in the evaluator fixture rather than the skill prompt.

## Decisions
- **DEC-001** [AMB:AMB-001] [FR-004] [FR-005] [FR-008]: The first slice uses current typed registry, formula, transition, package, and source facts for complete structural claims and relational transition candidates; insufficient opaque algorithm summaries produce bounded `unknown` findings.
- **DEC-002** [AMB:AMB-002] [FR-003] [FR-004]: Public analysis contracts and pure evaluation live in `SIR.Domain`; `SIR.Tools` owns `sir-rules check`, JSON rendering, cache-file I/O, and exit status. The worker widened the live claim before this path was touched.
- **DEC-003** [AMB:AMB-003] [FR-006] [FR-007]: Analysis accepts explicit bounds and optional prior cache entries, keys reuse to complete semantic/package/analyzer inputs, spends deterministic work units rather than wall-clock time in the pure core, and returns a valid partial report when work is exhausted.
- **DEC-004** [AMB:AMB-004] [FR-004] [FR-008]: `failed` claims block; required-strength `unknown` claims block; other honest unknowns remain visible and non-successful without being mislabeled as failures or passes.
- **DEC-005** [AMB:AMB-005] [FR-009] [FR-011] [FR-012]: Structural skill validation is paired with command-level forward fixtures, pre-fix red mutations, pruned/unpruned and clean/warm equivalence, alternate ordering, bounded scale, and .NET/Fable parity.

## Accepted Deferrals
- None.

## Remaining Ambiguity
- None. AMB-001 through AMB-005 are resolved by DEC-001 through DEC-005.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 193-rule-authoring-coherence`.
