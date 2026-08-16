---
schemaVersion: 1
workId: 214-client-feature-loader
title: Client Feature Loader
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Client Feature Loader Contract Specification

Prose status: specified; Tier 1 public/build contract.

## User Value
Users receive a small, stable shell first and deliberately load Rules Explorer,
Docs, or Tactical Environment through visible production controls. Maintainers
can review and gate exactly which source and chunk belongs to each loading phase.

## Scope
- SB-001: Versioned client feature registry, Fable-safe import edge, stable public loader state/messages, deterministic Vite identities, per-route/chunk budgets, eager-reachability gate, production browser qualification, mutations, content-addressed bundle-graph receipt, and loader documentation.
- SB-002: Migrate Rules Explorer, Docs, and Tactical Environment; future modes must register before becoming reachable.

## Non-Goals
- SB-003: Do not alter gameplay/domain semantics, server APIs, retained engine protocol, or unrelated presentation structure.
- SB-004: Do not enable property-name mangling or relax minifier compatibility as a size remedy.
- SB-005: Do not use timing assertions for delivery correctness; deterministic structure and bytes own the gate.

## User Stories
- US-001 (P1): As a user, I see the production shell before optional feature code is fetched and can open each feature through a real control.
- US-002 (P1): As a maintainer, I can inspect one versioned registry and a deterministic receipt to know feature phase, route, chunk identity, and budget.
- US-003 (P1): As a maintainer, I receive an actionable red result when feature code leaks eager, a chunk is missing, an identity is stale, or a route/chunk budget grows.

## Acceptance Scenarios
- AC-001 [US-002] [FR-001] [FR-006]: Given the source registry, when it is validated, then schema/version, unique stable feature IDs, bootstrap/eager/deferred phase, route/control identity, module identity, and byte budgets are complete for Rules Explorer, Docs, and Tactical Environment.
- AC-002 [US-001] [FR-002] [FR-006] [FR-008]: Given the built production shell, when a browser activates each real feature control, then stable public loader messages drive idle/loading/loaded/failed state, shell content appears before deferred requests, and the named feature becomes visible.
- AC-003 [US-001] [US-002] [FR-003] [FR-010]: Given identical source and locked inputs, when production assets are built, then CSP-compatible relative import targets, offline behavior, stable logical chunk identities, and the normalized bundle-graph receipt are deterministic; volatile timings and absolute paths are absent.
- AC-004 [US-002] [US-003] [FR-004] [FR-005] [FR-007]: Given a production bundle, when the contract gate runs, then initial-route and per-feature raw/gzip/Brotli budgets are enforced, no undeclared feature source is eagerly reachable, and property mangling is rejected.
- AC-005 [US-003] [FR-009]: Given protected eager-import, missing-chunk, and stale-identity mutations, when the owning build/browser gate runs, then each mutation fails red for its named subject.
- AC-006 [US-002] [FR-011]: Given implementation sources are frozen, when acceptance evidence is assembled, then one aggregate production build/browser run produces immutable content-addressed receipts; feedback-only metadata changes do not request another build.

## Functional Requirements
- FR-001: The client MUST define one schema-versioned declarative registry with unique stable feature IDs, bootstrap/eager/deferred phases, route/control identities, module/chunk identities, and explicit route/chunk budgets. (Stories: US-002; Acceptance: AC-001)
- FR-002: The Fable client MUST expose stable public loader state and messages, keep dynamic import at the JavaScript effect edge, and reject stale completions by requested feature/chunk identity. (Stories: US-001; Acceptance: AC-002)
- FR-003: Deferred imports MUST use deterministic relative CSP-compatible targets, provide explicit offline/missing-chunk failure state, and preserve stable logical identity independently of content hashes. (Stories: US-001, US-002; Acceptance: AC-003)
- FR-004: The build gate MUST derive and enforce versioned raw/gzip/Brotli budgets for the bootstrap route, each initial route, and every deferred feature chunk from the #154 loading SLO. (Stories: US-002, US-003; Acceptance: AC-004)
- FR-005: Vite/minifier configuration MUST prohibit property-name mangling as a delivery-growth remedy and the gate MUST reject it if introduced. (Stories: US-002, US-003; Acceptance: AC-004)
- FR-006: Rules Explorer, Docs, and Tactical Environment MUST be registered and qualified through the same loader contract, and unregistered future feature imports MUST fail validation. (Stories: US-001, US-002; Acceptance: AC-001, AC-002)
- FR-007: The build gate MUST fail when registered deferred feature code becomes eagerly reachable outside the registry-declared import boundary and name the leaked feature/module path. (Stories: US-003; Acceptance: AC-004)
- FR-008: A real production browser MUST prove the shell boots first and each registered feature loads through its actual user-facing control rather than direct message injection or a test-only seam. (Stories: US-001; Acceptance: AC-002)
- FR-009: Automated mutations MUST prove eager import, missing chunk, and stale chunk identity each turn its owning gate red with a subject-specific diagnostic. (Stories: US-003; Acceptance: AC-005)
- FR-010: The build MUST emit a deterministic normalized bundle-graph receipt bound to registry digest, input/source digest, emitted chunk identities, route membership, budgets, and content digests, excluding timestamps and machine-local paths. (Stories: US-002; Acceptance: AC-003)
- FR-011: Acceptance MUST use focused edit-time tests followed by one source-frozen aggregate that creates immutable/content-addressed receipts; changing feedback-only metadata MUST not rebuild product artifacts. (Stories: US-002; Acceptance: AC-006)

## Ambiguities
- AMB-001: Which features are bootstrap, eager, and deferred in registry version 1, including whether Docs is an internal bundle or an external generated-site navigation.
- AMB-002: Which identity binds a load request and prevents a late/stale chunk result from changing current state.
- AMB-003: Which existing #154 byte ceilings are retained and how per-feature ceilings are derived without rebaselining silently.
- AMB-004: Which production control and visible terminal condition qualifies each feature without test-only dispatch.
- AMB-005: How the graph receipt remains reproducible while Vite content hashes change with content.

## Public Or Tool-Facing Impact
- Adds a versioned registry and receipt schema, stable F# loader state/messages, build diagnostics, browser controls/labels, and loader documentation.
- Existing default shell behavior and retained-engine/publication contracts remain compatible.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 214-client-feature-loader`.
