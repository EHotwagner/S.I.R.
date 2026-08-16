---
schemaVersion: 1
workId: 193-rule-authoring-coherence
title: Conversational rule authoring and corpus coherence
stage: charter
changeTier: tier1
status: chartered
policyPointers:
  - .fsgg/sdd.yml
  - .fsgg/agents.yml
  - .fsgg/policy.yml
  - .fsgg/capabilities.yml
  - .fsgg/tooling.yml
---

# Conversational rule authoring and corpus coherence Charter

## Identity
Deliver repository-owned workflows for collaboratively authoring executable F# rules and independently checking the bounded coherence of a changed rule, its dependency cone, or the complete corpus.

## Principles
- Keep the shared typed F# corpus authoritative; skills orchestrate deterministic tooling and never duplicate semantics in prompts, JavaScript, or prose.
- Report a matrix of scoped claim strengths and explicit unknowns; bounded evidence is never upgraded into an unbounded proof.
- Make state-space cost observable and bounded through typed interaction indexes, deterministic sharding, cache identities, cancellation, and minimized witnesses.
- Keep conversational design choices with the developer while mechanical checks remain reproducible and read-only by default.

## Scope Boundaries
- In: two repository skills, a public analyzer contract and CLI route, structural/interaction/invalidation analysis, deterministic reports, fixtures, mutations, forward tests, and documentation.
- In: the first production slice over the landed #194 combat corpus plus a synthetic scale harness that proves bounded context and work.
- Out: proving fun or balance, silently repairing audited rules, solving arbitrary F#, broad rule migration, or coupling correctness to #192 presentation aesthetics.

## Policy Pointers
- Constitution II, III, VI, VII, and VIII govern the versioned analyzer/report contract, declared F# surface, real fail-before/pass-after evidence, shared human/agent contract, and actionable bounded failure.
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`; optional Governance remains outside this implementation.

## Lifecycle Notes
- Tier 1: adds tool-facing F# signatures, a stable report schema, CLI behavior, and two agent skill contracts.
- The executable corpus delivered by #194 is the authority; #192 is an independent presentation enhancement.
- Next lifecycle action: `fsgg-sdd specify --work 193-rule-authoring-coherence`.
