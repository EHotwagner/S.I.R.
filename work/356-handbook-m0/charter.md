---
schemaVersion: 1
workId: 356-handbook-m0
title: S.I.R. combat Quint handbook M0 authority inventory
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

# S.I.R. combat Quint handbook M0 authority inventory Charter

## Identity
- Work id: `356-handbook-m0`
- Lifecycle stage: charter
- Status: chartered

## Principles
- Preserve the imported roadmap wording and use it as the milestone ledger.
- Distinguish current `origin/main` authority from candidate authority in PR #355.
- Inventory sources without silently resolving disagreements or implementing M1.

## Scope Boundaries
- In: `docs/sir-combat-quint-handbook-roadmap.md`, M0 authority/source inventory, rule dependencies, Quint declaration/property inventory, vocabulary, exclusions, disagreements, lifecycle and feedback evidence.
- Out: `docs/sir-combat-quint-handbook.md` content, M1 skeleton/anchors/link audit, changes to runtime combat behavior, and changes to Q4 PR #355.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Governance files are optional compatibility pointers and are not evaluated by this command.

## Lifecycle Notes
- Tier 1 documentation contract because this establishes the durable roadmap ledger and publication target.
- Next lifecycle action: `fsgg-sdd specify --work 356-handbook-m0`.
