---
schemaVersion: 1
workId: 215-single-pass-qualification
title: Single-Pass Production Qualification
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Single-Pass Production Qualification Specification

Prose status: specified; Tier 1 qualification command and receipt contract.

## User Value
Maintainers run production qualification once, reuse exactly the outputs that run verified across documentation, delivery, browser, and feedback evidence, and receive a named refusal instead of accidentally trusting stale artifacts.

## Scope
- SB-001: Build/qualification scripts, package commands and locks, documentation build integration, feedback evidence validation, tests, immutable receipts, and comparable timing evidence.
- SB-002: Reuse the main Fable compile, Rules Explorer compile, production bundle, documentation site, delivery checks, and browser results produced by the source-frozen aggregate.

## Non-Goals
- SB-003: Do not change production simulation, gameplay, client feature behavior, browser journeys, documentation content semantics, or publication layout.
- SB-004: Do not weaken or remove existing documentation, delivery, browser, client-loader, or mutation subjects.
- SB-005: Do not treat a mutable cache, timestamp, absolute path, or feedback prose as build-output identity.

## User Stories
- US-001 (P1): As a maintainer, I can qualify a frozen source revision with one build of each production target and reuse those verified outputs in every downstream gate.
- US-002 (P1): As a reviewer, I can inspect an immutable receipt to know the exact source, configuration, lock, tool, command, and output identities that were accepted.
- US-003 (P1): As an evidence author, I can cite focused immutable command receipts after the aggregate without creating a feedback-audit/full-aggregate circularity.

## Acceptance Scenarios
- AC-001 [US-001] [US-002] [FR-001] [FR-002] [FR-003]: Given a clean source-frozen checkout, when the aggregate builds production, then it compiles the main Fable client and Rules Explorer once, produces the client/site outputs once, and records their complete immutable identities in one content-addressed receipt.
- AC-002 [US-001] [FR-004] [FR-005]: Given that current receipt, when documentation, delivery, and browser qualification consume it, then they verify and reuse the same outputs while preserving their existing normal behavior and mutation coverage.
- AC-003 [US-002] [FR-006] [FR-007]: Given any drift in revision, source/configuration input, dependency lock, tool version, owning command, expected output path, or output digest, when reuse is attempted, then it fails closed with a subject-specific diagnostic and a self-restoring mutation proves the red result.
- AC-004 [US-003] [FR-008]: Given a passing source-frozen aggregate and later feedback-only metadata changes, when feedback evidence is validated, then focused immutable command receipts satisfy the cited command evidence without rebuilding product artifacts.
- AC-005 [US-001] [US-002] [FR-009]: Given comparable clean baseline and candidate runs on the same host, when wall time is measured, then the candidate demonstrates a material reduction attributable to eliminating duplicate Fable and aggregate work without reducing accepted subjects.
- AC-006 [US-001] [FR-010]: Given the implementation workflow, when edits and acceptance proceed, then only focused tests run during edits, exactly one clean aggregate runs after source freeze, feedback finalization is metadata-only, and exactly one hosted final CI run qualifies the reviewed head.

## Functional Requirements
- FR-001: The aggregate MUST emit a schema-versioned, immutable, content-addressed build-output receipt whose filename digest matches canonical receipt bytes. (Stories: US-001, US-002; Acceptance: AC-001)
- FR-002: The receipt MUST bind the exact source revision/tree and clean-worktree state, all declared source/configuration/dependency-lock inputs, owning command identity, and relevant tool names/versions. (Stories: US-002; Acceptance: AC-001)
- FR-003: The receipt MUST bind the main Fable output, Rules Explorer output, production client bundle, documentation site, and their expected paths/content identities without timestamps or machine-local paths. (Stories: US-001, US-002; Acceptance: AC-001)
- FR-004: Documentation qualification MUST accept only a verified receipt and reuse its existing client/Fable outputs instead of invoking the nested Fable/client build again. (Stories: US-001; Acceptance: AC-002)
- FR-005: Delivery and browser qualification MUST consume the same verified production outputs and retain every existing normal behavior assertion and protected mutation. (Stories: US-001; Acceptance: AC-002)
- FR-006: Receipt verification MUST reject revision/tree, dirty-state, source/configuration input, package-lock, tool-version, owning-command, expected-path, missing-output, and output-content drift with actionable subject names. (Stories: US-002; Acceptance: AC-003)
- FR-007: A protected self-restoring mutation MUST alter one bound reuse subject, prove verification fails red with its expected diagnostic, and restore the subject byte-for-byte even on failure. (Stories: US-002; Acceptance: AC-003)
- FR-008: Feedback validation MUST accept focused immutable command receipts bound to their exact audit head and declared evidence subject so feedback-only report/audit commits do not require a second full aggregate. (Stories: US-003; Acceptance: AC-004)
- FR-009: Committed or reproducible timing evidence MUST record comparable before/after wall time, host facts, command identities, and subject inventory and MUST demonstrate a material single-pass reduction. (Stories: US-001, US-002; Acceptance: AC-005)
- FR-010: The implementation MUST preserve the lean workflow of focused edit-time tests, one post-freeze aggregate, metadata-only feedback finalization, and one hosted final CI execution. (Stories: US-001; Acceptance: AC-006)

## Ambiguities
- AMB-001: Which command owns producing and verifying the canonical receipt, and how downstream commands request reuse without creating parallel cache semantics.
- AMB-002: Which exact input and tool inventory is sufficient to make main Fable, Rules Explorer, client bundle, and site outputs stale whenever their real build contract changes.
- AMB-003: Which focused receipt fields let feedback validate command evidence at an audit head without treating feedback metadata as a product-build input.
- AMB-004: What comparable baseline/candidate commands and materiality threshold establish the required wall-time reduction.
- AMB-005: Which stale-reuse mutation exercises the real verifier while guaranteeing byte-for-byte restoration.

## Public Or Tool-Facing Impact
- Adds a versioned build-output receipt and verify/reuse command contract, a receipt-aware documentation invocation, and focused feedback command-receipt validation.
- Existing production outputs, routes, public client behavior, browser journeys, documentation checks, delivery checks, and mutations remain compatible.

## Lifecycle Notes
- Required route: implementation-ready analyze before source edits, then evidence, verify, and ship.
- Next lifecycle action: `fsgg-sdd clarify --work 215-single-pass-qualification`.
