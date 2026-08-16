---
schemaVersion: 1
workId: 193-rule-authoring-coherence
title: Rule Authoring Coherence
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Rule Authoring Coherence Specification

Prose status: specified

## User Value
A developer can refine an intended gameplay rule conversationally, implement it once in typed F#, and obtain an independent, bounded coherence report whose claims, unknowns, costs, and witnesses are precise enough to act on.

## Scope
- SB-001: Two repository-owned skills: `sir-author-rule` for iterative authoring and `sir-check-rule-coherence` for read-only-by-default judgment.
- SB-002: A deterministic F# analyzer and stable JSON report over the #194 registry/normalized rule model, with `changed`, `cone`, and `corpus` modes.
- SB-003: Structural, relational interaction, bounded invalidation/cache, cancellation, witness, mutation, scale, .NET/Fable parity, documentation, and forward-test evidence for the first combat-corpus slice.

## Non-Goals
- SB-004: Do not claim that compilation proves coherence, that bounded search proves an unbounded property, or that the tool proves fun or balance.
- SB-005: Do not silently edit rules during coherence analysis, encode arbitrary F# in a solver, enumerate the full game state, compare every rule pair by default, or introduce parallel JavaScript/TypeScript semantics.
- SB-006: Do not migrate the complete future corpus or make #192's richer presentation a correctness dependency.

## User Stories
- US-001 (P1): As a rule author, I can move from player-facing intent to a typed rule, rationale, examples, implementation link, focused evidence, and a coherence-checked dependency cone through one iterative workflow.
- US-002 (P1): As an independent reviewer, I can run a read-only check at changed, cone, or corpus scope and inspect bounded summaries or one minimized witness without loading the corpus into model context.
- US-003 (P1): As a maintainer, I can trust deterministic invalidation, caching, sharding, cancellation, and claim-strength labels because fail-closed mutations and scale fixtures prove their boundaries.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001] [FR-002]: Given a natural-language rule idea or revision, when `sir-author-rule` runs, then it inspects related corpus facts, resolves only material choices one at a time, presents human and typed forms, implements shared F#, executes focused evidence, invokes cone coherence, and reports each iteration honestly.
- AC-002 [US-002] [FR-003] [FR-004]: Given the combat corpus and each analysis mode, when the coherence workflow runs, then a deterministic F# command emits a schema-versioned summary with stable ordering, exact scope, dimensions, strengths, unknowns, costs, and selectable witnesses while the skill remains read-only by default.
- AC-003 [US-002] [US-003] [FR-005] [FR-006]: Given disjoint, dependent, conflicting, opaque, and metadata-only rule deltas, when impact analysis runs, then typed indexes prune non-interactions, semantic deltas invalidate the relevant cone, rationale-only changes skip execution work, and a warm unchanged run performs no expensive work.
- AC-004 [US-003] [FR-007] [FR-008]: Given budgets, cancellation, unsupported constructs, and seeded defects, when analysis cannot complete or finds a violation, then it returns a valid deterministic partial report or minimized actionable witness and never hangs, retries identically, discards completed claims, or converts unknown into pass.
- AC-005 [US-003] [FR-009] [FR-010]: Given generated bounded corpora, alternate shard completion orders, clean/warm runs, and .NET/Fable execution, when qualification runs, then report semantics/digests agree, affected-slice cost is observable, and model-facing output stays bounded independently of corpus size.
- AC-006 [US-001] [US-002] [FR-011] [FR-012]: Given successful authoring and adversarial forward-test prompts, when the two skill packages are validated, then they preserve independent responsibilities, detect the declared contradiction/reachability/unit/dependency/status/fingerprint/history mutations, and expose reproducible usage/documentation.

## Functional Requirements
- FR-001: `sir-author-rule` MUST implement the conversational inspect-intent-design-implement-test-coherence-revise loop, ask only one repository-unanswerable material question at a time, and distinguish implementation defects, rule defects, example defects, and unresolved design choices. (Stories: US-001; Acceptance: AC-001)
- FR-002: Authoring MUST preserve stable identities or explicit supersession, shared F# authority, rationale, examples/properties, structured explanations, source/package bindings, .NET/Fable evidence, and an iteration summary; it MUST NOT weaken a red gate merely to finish. (Stories: US-001; Acceptance: AC-001)
- FR-003: `sir-check-rule-coherence` MUST be read-only by default, support `changed`, `cone`, and `corpus`, invoke deterministic tooling rather than reason over a pasted corpus, and retrieve bounded summaries, slices, or witnesses on demand. (Stories: US-002; Acceptance: AC-002)
- FR-004: The analyzer MUST emit deterministic schema-versioned human/machine contracts covering identity, references, types/units, dependencies, compatibility, temporal/reachability/invariants/content/intent/coverage/history/presentation with explicit `proved-structural`, `proved-fragment`, `exhaustive-bounded`, `tested`, `heuristic`, `unknown`, and `failed` strengths. (Stories: US-002; Acceptance: AC-002)
- FR-005: Candidate construction MUST use typed dependency, status, read/write/event/phase/resource/disclosure/random/invariant relations and MUST record why candidates were selected or pruned; missing or untrusted footprints MUST become findings or unknowns, never silent all-clear pruning. (Stories: US-002, US-003; Acceptance: AC-003)
- FR-006: Impact analysis and cache identity MUST bind analyzer version, semantic slice, contracts/invariants, bounds/profile, implementation fingerprints, package/source identity, and configuration; unchanged warm work MUST perform zero expensive analyses and deltas MUST invalidate only relevant dependants/conflicts/watchers/tests/history checks. (Stories: US-003; Acceptance: AC-003)
- FR-007: Every expensive analyzer MUST declare deterministic rule/edge/state/transition/case/witness and outer time budgets, shard stably, retain completed results under cancellation, and report pending work, exact termination, cost, and explicit unknowns. (Stories: US-003; Acceptance: AC-004)
- FR-008: Failures MUST carry stable fingerprints, involved rules/invariant, dependency reason, strength/scope, minimized witness, cache/invalidation facts, cost, termination, and the smallest unresolved design question; unsupported or exhausted work MUST NOT report pass. (Stories: US-002, US-003; Acceptance: AC-004)
- FR-009: Qualification MUST compare pruned analysis with an unpruned bounded oracle, incremental with clean recomputation, and alternate shard orders; it MUST seed violations and cache-poisoning mutations for every implemented constraint/invalidation family. (Stories: US-003; Acceptance: AC-005)
- FR-010: The first slice MUST prove deterministic .NET/Fable report agreement and affected-slice scaling on #194's combat corpus plus synthetic corpora, including zero expensive warm work, bounded summary size, deterministic cancellation, and honest opaque-algorithm unknowns. (Stories: US-003; Acceptance: AC-005)
- FR-011: Both skills MUST ship concise `SKILL.md`, `agents/openai.yaml`, only necessary references/scripts, standard structural validation, and independent forward tests that do not leak expected diagnoses. (Stories: US-001, US-002; Acceptance: AC-006)
- FR-012: Documentation and tests MUST cover a successful authoring journey plus contradiction, unreachable transition, unit mismatch, undeclared dependency, prototype-to-canonical leakage, algorithm-fingerprint drift, historical replay mismatch, stable report ordering, cancellation, and context-bounded retrieval. (Stories: US-001, US-002, US-003; Acceptance: AC-006)

## Ambiguities
- AMB-001: Which analyzer slice is both honest for #194's current rule metadata and small enough to ship without inventing future corpus facts.
- AMB-002: Which existing executable boundary should host the `sir-rules check` command without expanding the issue's declared paths.
- AMB-003: Which cache persistence and cancellation interface can be deterministic across .NET/Fable while remaining useful to local and CI workflows.
- AMB-004: Which findings block canonicalization in this first slice and which must remain explicit non-blocking unknowns.
- AMB-005: How forward tests can evaluate the skills independently without allowing prose-only self-certification.

## Public Or Tool-Facing Impact
- Adds public F# coherence-analysis types/functions, a repository command/report schema, two Codex skill contracts, stable fixtures, and documented author/check workflows.
- Existing rule manifest/package/application formats remain compatible; analyzer data is additive and versioned.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 193-rule-authoring-coherence`.
