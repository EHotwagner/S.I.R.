---
schemaVersion: 1
workId: 366-main-ci-routing
title: Route main-push CI by relevant changes
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

# Route main-push CI by relevant changes Charter

## Identity
- Work id: `366-main-ci-routing`
- Lifecycle stage: charter
- Status: chartered

## Principles
- Route from the exact landed source identity and refuse incomplete evidence; a skip is valid only when the canonical policy declares it non-applicable.
- Reuse the established PR path taxonomy, producer derivation, gate receipts, and deterministic join rather than create a second CI authority.
- Keep complete clean-room qualification on schedule and manual dispatch as the periodic cross-surface safety net.
- A successful CI run must not imply a deployable documentation site unless that run produced and verified an exact-source site handoff.

## Scope Boundaries
- In: GitHub Actions event topology, canonical routing, exact-source typed receipts, protected verdict, qualified-site handoff, Pages filtering, tests, lifecycle evidence, and operator documentation.
- Out: changing product behavior, weakening PR routing, reducing periodic full-system subjects, changing product performance budgets, or adopting the interactive-runtime performance gate.
- Keep SDD lifecycle ownership separate from optional Governance enforcement.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Governance files are optional compatibility pointers and are not evaluated by this command.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd specify --work 366-main-ci-routing`.
