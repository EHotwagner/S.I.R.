---
schemaVersion: 1
workId: 363-handbook-m3
title: S.I.R. combat Quint handbook M3 complete rules
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

# S.I.R. combat Quint handbook M3 complete rules Charter

## Identity
- Work id: `363-handbook-m3` for issue `EHotwagner/S.I.R.#363`.
- Complete roadmap milestone M3 so every stable combat rule has an executable, appropriately granular handbook walkthrough.

## Principles
- Treat `docs/rules/sir-combat.md` as the literate Quint authority and mechanically check every executable excerpt or named subject against it.
- Teach catalogue identity, dependency order, focused transitions, and the external line-of-sight contract without inventing runtime-visible intermediate state.
- Preserve M2's representative attack learning spine and keep M4's mutation laboratory and M5's broad runtime correspondence out of scope.

## Scope Boundaries
- In: the sixteen-rule catalogue and dependency map; wound/incapacity, suppression/recovery, cover/current collision/destruction, penetration, collateral, aggregate resolution, line-of-sight contract; a complete reference entry and traceability row per rule; beginner/intermediate/advanced exercises; focused authority and documentation qualification; roadmap evidence.
- Out: new combat semantics, production-code changes, runtime-visible intermediate transitions, M4 counterexample/mutation laboratory, M5 full Quint-to-F# correspondence/replay chapter, M6 global definition-index enforcement, and M7 publication review/handoff.
- Only roadmap M3 may move from unchecked to checked, and only at the merge-candidate boundary with explicit post-merge obligations.

## Policy Pointers
- Follow `.fsgg/constitution.md`, `.fsgg/sdd.yml`, `.fsgg/agents.yml`, and the roadmap ledger contract.
- Current semantic authority is `docs/rules/sir-combat.md`; domain intent is `docs/combat-resolution.md`; the publication target is `docs/sir-combat-quint-handbook.md`.
- Governance files are optional compatibility pointers and are not evaluated by this command.

## Lifecycle Notes
- Stable feedback cycle: `roadmap-sir-combat-quint-handbook-m3-complete-rules`.
- M2 landed at `b109fd2`; M3 must preserve that content and discharge the M3 deferrals named there.
- Build scoped receipts from the start for docs, links, focused authority coverage, full Q4 qualification, roadmap ledger, and lifecycle currency.
- Next lifecycle action: `fsgg-sdd specify --work 363-handbook-m3`.
