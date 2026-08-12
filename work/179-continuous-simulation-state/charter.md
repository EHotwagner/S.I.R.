---
schemaVersion: 1
workId: 179-continuous-simulation-state
title: Continuous simulation state
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

# Continuous simulation state Charter

## Identity
- Work id: `179-continuous-simulation-state`
- Lifecycle stage: charter
- Status: chartered

## Principles
- Treat the simulator as a continuously reconciled projection, not a manually invoked handoff.
- Preserve deterministic simulation truth across all tactical modalities and timeline operations.
- Keep effects at the UI edge; reconciliation and seeking remain pure, testable transitions.

## Scope Boundaries
- In: compatible-edit reconciliation, activation history, deterministic reconstruction, modality parity,
  visible fallback reasons, focused .NET and production-browser evidence.
- Out: LOS, cover, armor, weapon rules, and scenario content.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Governance files are optional compatibility pointers and are not evaluated by this command.

## Lifecycle Notes
- Tier 1 interactive behavior. No producer-owned performance intent is declared, so the performance
  gate records that absence rather than inventing a timing budget.
