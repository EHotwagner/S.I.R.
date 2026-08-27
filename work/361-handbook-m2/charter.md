---
schemaVersion: 1
workId: 361-handbook-m2
title: S.I.R. combat Quint handbook M2 representative attack
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

# S.I.R. combat Quint handbook M2 representative attack Charter

## Identity
- Work id: `361-handbook-m2`
- Lifecycle stage: charter
- Status: chartered

## Principles
- Teach one representative attack from authoritative design facts through executable Quint and bounded runtime correspondence.
- Treat `docs/rules/sir-combat.md` as literate Quint authority; shown executable excerpts must be extracted or mechanically checked against it.
- Distinguish Quint execution evidence from runtime-equivalence evidence and explain Q4 raw scale 10,000 plus the explicit int32-wrap rounding boundary honestly.

## Scope Boundaries
- In: M2's attack pipeline, facts, arithmetic, records, representative action/run, prediction/trace workflow, one observed-red mutation, runtime correspondence, vocabulary/link audits, and roadmap evidence.
- Out: M3's broad rule walkthroughs, M4's mutation laboratory, M5's full correspondence/replay reference, and any change to combat semantics or production code.
- Preserve all roadmap wording and history; only M2 may move from unchecked to checked.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Governance files are optional compatibility pointers and are not evaluated by this command.

## Lifecycle Notes
- Issue: `EHotwagner/S.I.R.#361`.
- Stable feedback cycle: `roadmap-sir-combat-quint-handbook-m2-representative-attack`.
- Current authority dependency landed in PR #355 at `f0abd0353729712255f21c0d19d40f1ce0798907`.
- Next lifecycle action: `fsgg-sdd specify --work 361-handbook-m2`.
