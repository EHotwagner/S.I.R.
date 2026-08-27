---
schemaVersion: 1
workId: 375-handbook-m6
title: S.I.R. combat Quint handbook M6 index and link enforcement
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

# S.I.R. combat Quint handbook M6 index and link enforcement Charter

## Identity
- Work id: `375-handbook-m6` for issue `EHotwagner/S.I.R.#375`.
- Complete roadmap M6 so every controlled term, Quint declaration, stable rule, chapter, and index target is reconciled and mechanically linkable.

## Principles
- The handbook remains the explanatory publication; `docs/rules/sir-combat.md` remains the literate model authority and the sixteen stable rules remain registry authority.
- `docs/sir-combat-quint-vocabulary.json` is the structured inventory for canonical terms, aliases, and structural exemptions; it may describe but never redefine combat semantics.
- Link enforcement parses Markdown structure and fails closed on missing fragments, duplicate anchors, absent index entries, unresolved inventory entries, and unlinked controlled prose outside declared exemptions.

## Scope Boundaries
- In: complete all 185 planned definition entries; canonical aliases and related links; vocabulary/declaration/rule/chapter/index reconciliation; structural Markdown link audit integrated into docs qualification; observed-red/restored-green negative controls; lifecycle, feedback, review, roadmap, PR, and hosted evidence.
- Out: combat or Quint semantic changes; production F# changes; M6V mechanics/theory SVGs, animation/shaders, accessibility and fallback implementations, render regression, and performance qualification; M7 final editorial/publication handoff.
- Preserve all prior milestone history and later milestone wording/pending state; only M6 may become checked at the truthful merge-candidate boundary.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml`, `.fsgg/agents.yml`, and `.fsgg/constitution.md`.
- Publication and ledger: `docs/sir-combat-quint-handbook.md` and `docs/sir-combat-quint-handbook-roadmap.md`; structured inventory: `docs/sir-combat-quint-vocabulary.json`; model authority: `docs/rules/sir-combat.md`.

## Lifecycle Notes
- Stable feedback cycle: `roadmap-sir-combat-quint-handbook-m6-index-link-enforcement`.
- M0/M1 established the address inventory and prototype audit; M2–M5 filled handbook content while explicitly deferring complete definitions, aliases, and enforcement to M6. This charter accepts and must discharge those handoffs.
- Separate receipts cover strict docs, AST/structural audit, three named negative controls with restored green, complete inventory reconciliation, roadmap state, and lifecycle currency.
- No typed performance intent exists; the pnext performance-first gate is not invoked for M6. M6V retains the visual/performance scope.
- Next lifecycle action: `fsgg-sdd specify --work 375-handbook-m6`.
