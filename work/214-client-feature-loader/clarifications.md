---
schemaVersion: 1
workId: 214-client-feature-loader
title: Client Feature Loader
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/214-client-feature-loader/spec.md
publicOrToolFacingImpact: true
---

# Client Feature Loader Clarifications

## Source Specification
- work/214-client-feature-loader/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: Which phase and delivery posture owns shell, Rules Explorer, Docs, and Tactical Environment in registry v1?
- CQ-002 [AMB:AMB-002]: Which public value identifies an in-flight load and makes stale completion observable but harmless?
- CQ-003 [AMB:AMB-003]: Which byte ceilings apply without silently replacing the #154 loading SLO?
- CQ-004 [AMB:AMB-004]: Which real controls and terminal DOM regions qualify each feature?
- CQ-005 [AMB:AMB-005]: Which canonical facts enter the bundle-graph receipt and its identity?

## Answers
- CQ-001 → register `shell` as bootstrap, `tactical-environment` as eager, and `rules-explorer` plus `docs` as deferred. Docs is an in-shell deferred help feature whose links may navigate to generated documentation; it is not an unobserved external redirect.
- CQ-002 → bind every request to registry version, stable feature id, and stable logical chunk id. Loader state accepts success/failure only when all three match the current pending request.
- CQ-003 → retain the #154 initial ceiling (1,250,000 raw / 320,000 gzip / 280,000 Brotli) and the existing Rules Explorer ceiling (65,536 / 20,000 / 16,000). New feature ceilings are authored in registry v1 from an observed source-frozen build and can change only through a registry version/rebaseline.
- CQ-004 → qualify Rules Explorer through the existing Data/spatial-diagnostics control and named Rules region, Docs through the production Docs control and named help region, and Tactical Environment through Editor → Environment and the named authoring region. Page readiness must precede deferred network requests.
- CQ-005 → normalize registry/source digests, logical route/chunk ids, sorted emitted filenames, content SHA-256, byte/compression measurements, and import edges. Exclude timestamps, elapsed duration, absolute paths, and host facts; SHA-256 of canonical JSON is the receipt identity.

## Decisions
- DEC-001 [CQ-001] [AMB:AMB-001] [FR-001] [FR-006]: Registry v1 declares shell/bootstrap, tactical-environment/eager, rules-explorer/deferred, and docs/deferred; only deferred entries own dynamic import targets.
- DEC-002 [CQ-002] [AMB:AMB-002] [FR-002] [FR-003]: Public state is Idle, Loading request, Loaded identity, or Failed identity/reason; request identity is `(registryVersion, featureId, logicalChunkId)` and mismatched completions are ignored with a stable stale-identity diagnostic.
- DEC-003 [CQ-003] [AMB:AMB-003] [FR-004]: Keep #154 version-2 initial and Rules Explorer budgets. Author Docs from the first source-frozen observation with explicit headroom, and require a registry version/rebaseline plus red budget mutation for any increase.
- DEC-004 [CQ-004] [AMB:AMB-004] [FR-008]: Browser acceptance uses only production controls and visible named regions: shell readiness, Data/Rules Explorer, Docs/help, and Editor/Environment. Direct reducer dispatch is not acceptance evidence.
- DEC-005 [CQ-005] [AMB:AMB-005] [FR-007] [FR-010] [FR-011]: One post-build validator owns eager-edge analysis, budgets, minifier policy, and canonical graph receipt generation; one source-frozen aggregate creates receipts, while feedback-only paths are excluded from build workflow triggers.

## Accepted Deferrals
- None.

## Remaining Ambiguity
- None. AMB-001 through AMB-005 are resolved by DEC-001 through DEC-005.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd checklist --work 214-client-feature-loader`.
