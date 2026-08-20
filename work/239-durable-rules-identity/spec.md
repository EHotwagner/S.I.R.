---
schemaVersion: 1
workId: 239-durable-rules-identity
title: Durable rules corpus source identity
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Durable Rules Corpus Source Identity Specification

Prose status: specified

## User Value
Fresh and hosted checkouts can regenerate and verify the retained rules corpus from a canonical reachable source identity.

## Scope
- SB-001: Rebind the source commit and implementation digest, retain coherent fixtures and governance evidence, add fail-closed verifier mutations, and prove a no-alternates network clone.

## Non-Goals
- SB-002: Do not change gameplay-rule semantics, add rules, redesign broad qualification, or relax protected evidence gates.

## User Stories
- US-001 (P1): As a maintainer, I can regenerate and verify the retained rules corpus from a fresh checkout without depending on deleted branch objects.
- US-002 (P1): As a protected-boundary operator, I receive an early actionable refusal for a source identity that is not reproducible from canonical `origin/main`.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given the intended normalized implementation sources, when the corpus is regenerated, then its source commit is durably reachable from `origin/main` and its implementation digest, manifest identity, and affected retained evidence agree.
- AC-002 [US-002] [FR-002]: Given a malformed or missing declared source commit, when verification begins, then it fails before generation with an actionable diagnostic.
- AC-003 [US-002] [FR-002]: Given a commit that exists only in an implementer object store or deleted feature branch, or given a checkout without the canonical remote ref, when verification begins, then it fails before generation with an actionable diagnostic.
- AC-004 [US-001] [FR-003]: Given an exact repair head cloned over the network with `--no-local` and no object alternates, when `scripts/verify-rules-corpus.sh` runs, then it passes.
- AC-005 [US-001] [FR-004]: Given the changed retained corpus identity and this work item's ship artifact, when Governance receipts are generated, then the evidence bindings and protected boundary consistently bind current artifacts without blocking findings.
- AC-006 [US-001] [FR-004]: Given the exact repair head in hosted qualification, when protected checks complete, then `rules` and the aggregate verdict are green.

## Functional Requirements
- FR-001: The retained v2 corpus MUST bind the intended normalized implementation sources to a commit reachable from canonical `origin/main`, with a consistently regenerated implementation digest, manifest identity, and affected retained evidence. (covers AC-001)
- FR-002: The verifier MUST fail before generation with an actionable diagnostic for malformed, missing, checkout-local, missing-canonical-ref, or non-`origin/main`-ancestor source commits, including executable subject mutations for each fail-closed class. (covers AC-002, AC-003)
- FR-003: The exact repair head MUST pass `scripts/verify-rules-corpus.sh` in a fresh full network clone created with `--no-local` and without local object alternates. (covers AC-004)
- FR-004: The current SDD ship artifact and affected Governance evidence bindings MUST form a coherent protected boundary, and hosted `rules` plus the aggregate verdict MUST pass on the exact repair head. (covers AC-005, AC-006)

## Ambiguities
- AMB-001 open: which Git ref is the canonical reachability boundary when hosted checkout does not create `refs/remotes/origin/HEAD`?
- AMB-002 open: which SDD ship artifact must the resealed protected-boundary receipt bind after the corpus identity changes?

## Public Or Tool-Facing Impact
- The rules verifier gains an explicit source-durability contract and new failure diagnostics.
- Retained corpus and Governance JSON identities change together; gameplay semantics and the public simulation API do not change.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 239-durable-rules-identity`.
