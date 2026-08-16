---
schemaVersion: 1
workId: 193-rule-authoring-coherence
title: Rule Authoring Coherence
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/193-rule-authoring-coherence/spec.md
sourceClarifications: work/193-rule-authoring-coherence/clarifications.md
sourceChecklist: work/193-rule-authoring-coherence/checklist.md
publicOrToolFacingImpact: true
---

# Rule Authoring Coherence Plan

Prose status: planned

## Source Snapshot
- spec: work/193-rule-authoring-coherence/spec.md sha256:fafdc4eb0ef9951b199bbd569a4bee1fe644db7d04b1212084159cb229267919 schemaVersion:1
- clarifications: work/193-rule-authoring-coherence/clarifications.md sha256:a8004844b6a1d58cd736f4df17d4f5548ef355ba33dae42c6fe6e2e8bfc7ebc2 schemaVersion:1
- checklist: work/193-rule-authoring-coherence/checklist.md sha256:e37d779b4774e48799a2a8f725d00b6b7845009a60f678eea7b402e6f2bd5d3d schemaVersion:1

## Plan Scope
- Declare additive public coherence types and a pure `RuleCoherence.analyze` API before implementation.
- Build stable indexes over rule identity, dependencies, status, phases, reads/effects/events, formula units, source/package identity, and coverage; select `changed`, `cone`, or `corpus` slices without all-pairs comparison.
- Add deterministic report encoding, cache snapshots, work budgets, partial cancellation, candidate/pruning reasons, findings, witnesses, and cost counters.
- Expose `sir-rules check` through `SIR.Tools`, with explicit mode/rule/budget/cache/report arguments and fail-closed exit codes.
- Add shared .NET/Fable fixtures, adversarial mutations, bounded synthetic scale, clean/warm and pruned/unpruned equivalence, alternate-order determinism, skill structure/forward tests, docs, lifecycle evidence, and feedback.

## Technical Context
F#/.NET 10 and Fable 5.13 over the existing `SIR.Domain` rule registry and canonical encoders; `System.Text.Json` at the command edge; stable UTF-8/LF JSON and SHA-256 identities; shell ownership scripts for focused qualification.

## Constitution Check
- II/III: schema-versioned public records and `.fsi` declarations precede implementation; canonical JSON is a machine contract.
- IV: use immutable maps/sets/lists and explicit folds; no solver/framework dependency is justified for the first structural/relational slice.
- V: the analyzer core is pure; cache/report filesystem I/O stays in `SIR.Tools`.
- VI: every finding and pruning/cache gate receives a red mutation before a final green aggregate.
- VII/VIII: skills invoke the same command as humans and report malformed input, exhaustion, unknowns, and failed claims distinctly.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-005] complete: Author `sir-author-rule` as the conversational owner of intent, typed proposal, implementation/evidence iterations, and automatic cone-coherence handoff; forward-test behavior through repository fixtures rather than prose assertions.
- PD-002 [AC-001] [FR-002] complete: Make stable identity/supersession, rationale, examples/properties, source/package binding, shared F#, explanations, parity, and honest iteration summaries explicit stop conditions in the author skill and its tests.
- PD-003 [AC-002] [FR-003] [DEC-002] complete: Author a separate read-only-default coherence skill that only selects mode/bounds, invokes `sir-rules check`, reads the bounded summary, and retrieves one slice/witness on demand.
- PD-004 [AC-002] [FR-004] [DEC-001] complete: Add public mode/strength/dimension/termination/request/report/finding/witness/cost/cache types and canonical encoding in `SIR.Domain`; keep the v1 schema additive to existing rule package formats.
- PD-005 [AC-003] [FR-005] [DEC-001] complete: Build candidate relations from dependencies and shared transition phase/read-write/event facts, sort all keys, record selection/pruning reasons, and return unknown for missing trusted footprints instead of widening silently or pruning unsafely.
- PD-006 [AC-003] [FR-006] [DEC-003] complete: Compute semantic slice/cache keys from analyzer version, package/source/implementation identity, normalized request/bounds, and relevant rule bytes; rationale-only deltas reuse execution claims while algorithm/package/global changes invalidate their users.
- PD-007 [AC-004] [FR-007] [DEC-003] complete: Spend explicit deterministic work units per structural rule/candidate/shard, stop before exceeding limits, retain sorted completed claims, and return pending shard ids plus `budget-exhausted` termination; wall-clock cancellation remains an edge concern.
- PD-008 [AC-004] [FR-008] [DEC-004] complete: Emit stable fingerprints and small witnesses for structural/reference/unit/status/conflict failures and bounded unknowns; canonicalization is ready only when no failed or policy-required unknown claim remains.
- PD-009 [AC-005] [FR-009] [DEC-005] complete: Add fixture mutations for every implemented finding/invalidation/pruning family and compare optimized reports with an unpruned bounded oracle, cache hits with clean recomputation, and reversed shard inputs with canonical output.
- PD-010 [AC-005] [FR-010] complete: Reuse the portable shared conformance fixture in native and Fable runners; generate synthetic dependency/transition corpora at several sizes and assert bounded summary/witness sizes plus affected-slice rather than total-corpus work for leaf changes.
- PD-011 [AC-006] [FR-011] complete: Package each skill with concise trigger metadata, `agents/openai.yaml`, focused references/scripts, validator coverage, and realistic command-backed forward cases with no expected diagnosis embedded in skill text.
- PD-012 [AC-006] [FR-012] complete: Document command/report/cache/canonicalization contracts and retain successful authoring plus contradiction, reachability, unit, dependency, status, fingerprint, history, cancellation, ordering, and bounded-context tests in one focused owner route.

## Contract Impact
- PC-001 [PD-004] publicSurface: `RuleCoherence.fsi` declares schema-v1 analysis modes, strengths, dimensions, bounds, requests, findings, witnesses, costs, cache entries, reports, `analyze`, and canonical report bytes.
- PC-002 [PD-003] commandSurface: `SIR.Tools sir-rules check --mode changed|cone|corpus [--rule ID] [--max-work N] [--cache PATH] [--out PATH]` emits only schema-v1 report JSON on stdout and uses distinct invalid-input/failed/unknown exit states.
- PC-003 [PD-001] [PD-003] skillSurface: `.agents/skills/sir-author-rule` and `.agents/skills/sir-check-rule-coherence` are independently triggerable repository contracts; the author skill invokes the checker, while the checker never repairs.
- PC-004 [PD-006] cacheSurface: cache schema v1 is caller-owned, content-addressed, source/package/analyzer/request-bound, corruption-rejecting, and safe to omit; no migration from an earlier cache exists.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PC-003] semanticTest: Validate both skill packages and run successful/adversarial forward fixtures through their real command contracts.
- VO-002 [PD-004] [PD-005] [PD-008] [PC-001] semanticTest: Run shared native/Fable fixtures for structural, dependency, unit, status, interaction, unknown, witness, canonical-order, and report-byte behavior.
- VO-003 [PD-006] [PD-007] [PD-009] [PD-010] [PC-004] performanceTest: Run deterministic clean/warm, invalidation, pruning-oracle, cancellation, alternate-order, and synthetic-scale assertions; no wall-clock target is invented for this non-interactive core.
- VO-004 [PD-003] [PC-002] commandTest: Exercise changed/cone/corpus CLI modes, malformed arguments/cache, failed/unknown exit states, output selection, and stable JSON.
- VO-005 [PD-009] [PD-012] mutationTest: Invert every added/modified gate, observe focused red with the responsible diagnostic, restore, then run one source-frozen aggregate.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additive: Existing rule manifest/application/package schemas remain byte-compatible; analysis schema v1 is a separate additive contract.
- PM-002 [PC-004] diagnoseOnly: Missing cache means clean analysis; malformed, stale, or identity-mismatched cache is rejected or reported unusable and never becomes a hit.

## Generated View Impact
- GV-001 [PD-004] report: Analyzer JSON is generated deterministically from explicit rules/package/request/cache inputs and includes its complete identity/cost/termination facts.
- GV-002 [PD-001] [PD-003] skillGuidance: Repository skill metadata and any derived listings remain equivalent to their authored contracts; generated SDD guidance is refreshed after authored lifecycle changes.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- Optional Governance pointers remain compatibility facts only.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 193-rule-authoring-coherence`.
