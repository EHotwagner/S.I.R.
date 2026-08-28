---
schemaVersion: 1
workId: 380-handbook-m7
title: S.I.R. combat Quint handbook M7 publication handoff
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

# S.I.R. combat Quint handbook M7 publication handoff Charter

## Identity
- Work id: `380-handbook-m7` for `EHotwagner/S.I.R.#380`.
- Complete roadmap M7 by publishing the handbook with explicit, evidence-backed review and an owned maintenance trigger.

## Principles
- Domain meaning remains owned by the combat architecture and runtime registry; formal meaning remains owned by the literate Quint model. Review evidence may approve those boundaries but never merge or replace them.
- M6 and M6V gates are consumed and rebound at the M7 head. M7 does not redefine the six diagrams, their accessible fallbacks, or their typed 100/200 ms decoded-readiness budgets.
- Every new publication gate ships with an isolated observed-red/restored-green control.
- Exact toolchain and source identities are generated from pinned and Git-resolved facts, never copied from memory.

## Scope Boundaries
- In: independent domain, Quint/modeling, beginner, and rendered-document approvals; exact identities; maintenance checklist and owner trigger beside the model; strict docs and live publication proof.
- Out: combat semantic changes, new diagrams or effects, new performance intent, package upgrades, application runtime changes, and the parent-owned final cross-cycle roadmap roll-up.
- Dependency M6V is complete at `318f07a`; its six SVGs, fallbacks, render corpus, and performance evidence must remain linked and reviewable.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml`, `.fsgg/agents.yml`, and `.fsgg/constitution.md`.
- Publication content/ledger: `docs/sir-combat-quint-handbook.md` and `docs/sir-combat-quint-handbook-roadmap.md`.
- Semantic authorities: `docs/rules/sir-combat.md`, `docs/combat-resolution.md`, and `src/SIR.Simulation/CombatRules.fs`.
- Existing visual/performance authority and evidence: `docs/sir-combat-quint-diagrams.json`, `work/377-handbook-m6v/`, and `readiness/377-handbook-m6v/`.

## Lifecycle Notes
- Stable feedback cycle: `roadmap-sir-combat-quint-handbook-m7-publication-handoff`.
- Four independent subject approvals plus separate exact-head implementation and feedback critics are mandatory; no PR may open before both critics accept.
- This publication/review work does not trigger a new PERF-PLAN. Qualification replays and binds the existing M6V typed evidence and accurately preserves its capability limits.
- Next lifecycle action: `fsgg-sdd specify --work 380-handbook-m7`.
