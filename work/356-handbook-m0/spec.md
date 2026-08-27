---
schemaVersion: 1
workId: 356-handbook-m0
title: Handbook M0
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Handbook M0 Specification

Prose status: specified

## User Value
Maintainers can inspect a checked inventory of every authority, rule, model declaration, property, stat, unit, combat concept, evidence term, exclusion, and disagreement needed by the S.I.R. combat Quint handbook before prose is built on unstable assumptions.

## Scope
- SB-001: Import the roadmap ledger and complete only M0 inventory content; do not author the handbook skeleton or M1 content.

## Non-Goals
- SB-002: Do not implement later lifecycle commands or Governance enforcement in this specification.

## User Stories
- US-001 (P1): As a handbook author, I can identify the authority class and current status for every planned claim.
- US-002 (P1): As a reviewer, I can reconcile all sixteen stable combat rules with their dependency graph and candidate Quint representation.
- US-003 (P1): As a Quint learner, I can see every declaration, property, stat, unit, combat concept, and evidence concept that later chapters must define.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001]: Given the imported design, repository main, Q4 candidate, runtime, and evidence sources, when the source map is inspected, then each source has an authority class, status, question answered, and handbook use.
- AC-002 [US-002] [FR-002]: Given the runtime registry and candidate Quint catalogue, when the rule inventory is checked, then exactly sixteen unique stable IDs appear with kind and exact direct dependencies.
- AC-003 [US-003] [FR-003]: Given the candidate literate model, when its inventory is inspected, then every top-level type, constant/value, pure function, state variable, action, property, and run has a planned index kind.
- AC-004 [US-003] [FR-004]: Given the bounded combat model and evidence workflow, when vocabulary is inspected, then initial stats, units/encodings, combat concepts, and formal/evidence concepts are named and categorized.
- AC-005 [US-001] [FR-005]: Given source boundaries and current repository state, when exclusions and disagreements are inspected, then no unresolved disagreement changes the proposed state shape or action granularity and every non-authoritative candidate is labeled.
- AC-006 [US-001] [FR-006]: Given the source roadmap, when the repository ledger is inspected, then M0 is marked complete with concise evidence, M1-M7 remain unchecked with wording preserved, and the future handbook target is `docs/sir-combat-quint-handbook.md`.

## Functional Requirements
- FR-001: The M0 ledger MUST inventory the ADR, combat architecture, Q4 decisions, literate and generated model roles, runtime subjects, and evidence with authority class and current status. (covers AC-001)
- FR-002: The M0 ledger MUST list exactly sixteen unique stable rule IDs with kind and direct dependencies matching the Q4 candidate and runtime registry. (covers AC-002)
- FR-003: The M0 ledger MUST classify every top-level candidate Quint type, constant/value, pure function, state variable, action, property, and run by planned definition-index kind. (covers AC-003)
- FR-004: The M0 ledger MUST provide initial categorized vocabularies for stats, units/encodings, combat concepts, and formal/evidence concepts. (covers AC-004)
- FR-005: The M0 ledger MUST explicitly record scope exclusions, source disagreements, authority status, and whether any unresolved item changes proposed state shape or action granularity. (covers AC-005)
- FR-006: The repository MUST contain the imported M0-M7 roadmap ledger, identify `docs/sir-combat-quint-handbook.md` as the publication target, preserve later milestone wording, and mark only M0 complete with evidence. (covers AC-006)

## Ambiguities
No material ambiguities recorded.

## Public Or Tool-Facing Impact
- Adds a public documentation roadmap ledger; no runtime, package, API, or generated Quint surface changes.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd clarify --work 356-handbook-m0`.
