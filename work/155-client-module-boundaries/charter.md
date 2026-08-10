---
schemaVersion: 1
workId: 155-client-module-boundaries
title: Decompose oversized client modules into mode and infrastructure boundaries
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

# Decompose oversized client modules into mode and infrastructure boundaries Charter

## Identity
Refactor the S.I.R. client into explicit, compilation-order-safe boundaries while
retaining the single root Elmish program and existing deterministic behaviour.

## Principles
- Preserve public client behaviour, canonical evidence, browser routes, and Fable compilation.
- Keep runtime-neutral domain logic free of browser interop; browser effects stay at the Web edge.
- Make ownership visible through small typed module surfaces and focused tests.

## Scope Boundaries
- In: boundary extraction in `src/SIR.Client`, `src/SIR.Client.Web`, and their client tests.
- Out: changing protocol, simulation semantics, server live-session ownership, or user-visible workflows.

## Policy Pointers
- Constitution principles I, V, VI, and VIII; `.fsgg/sdd.yml`; and `.fsgg/agents.yml`.
- Existing performance evidence is retained through the production client qualification route.

## Lifecycle Notes
- Tier 1 architectural refactor: retain one root Elmish composition while extracting a focused
  typed boundary, prove it through existing deterministic client qualifications, and leave a
  documented dependency/size guard against root-module regrowth.
