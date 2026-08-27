---
schemaVersion: 1
workId: 365-handbook-m4
title: S.I.R. combat Quint handbook M4 formal reasoning
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

# S.I.R. combat Quint handbook M4 formal reasoning Charter

## Identity
- Work id: `365-handbook-m4` for issue `EHotwagner/S.I.R.#365`.
- Complete roadmap milestone M4 so readers can distinguish examples, reachable witnesses, invariants,
  sampled executions, and exhaustive verification, then diagnose counterexamples and deliberate defects.

## Principles
- Treat `docs/rules/sir-combat.md` as the literate Quint authority; handbook snippets and mutation
  fixtures are derived from it and never become a second semantic authority.
- Every deliberate defect must produce observed-red evidence through its named detection route and an
  untouched-authority rerun must produce restored-green evidence.
- State exactly what each Quint command establishes: a sampled run is not exhaustive proof, an
  existential witness is not a universal invariant, and a counterexample is a concrete refutation.

## Scope Boundaries
- In: examples versus witnesses versus invariants; nondeterministic trace reading; counterexample
  workflow; threshold, bounds, suppression, cover, collateral, and catalogue-integrity mutations;
  action reachability; invariant-to-state binding; sampled-versus-exhaustive language; dedicated
  qualification, feedback, lifecycle, and roadmap evidence.
- Out: combat semantic changes, production F# changes, broad runtime correspondence/replay work (M5),
  complete definition/link enforcement (M6), mechanics/theory SVG diagrams and effects (M6V), and
  publication/maintenance handoff (M7).
- Preserve all later milestone wording and pending state. Only M4 may become checked, at the truthful
  merge-candidate boundary with explicit independent-review, CI, merge, and post-merge obligations.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Current semantic authority is `docs/rules/sir-combat.md`; the learning surface is
  `docs/sir-combat-quint-handbook.md`; the milestone ledger is
  `docs/sir-combat-quint-handbook-roadmap.md`.
- Governance files are optional compatibility pointers and are not evaluated by this command.

## Lifecycle Notes
- Stable feedback cycle: `roadmap-sir-combat-quint-handbook-m4-formal-reasoning`.
- M3 landed in PR #364 and deliberately handed learner-facing mutation/counterexample work to M4;
  this charter accepts and must discharge that deferral.
- Build separate receipts for docs, links, formal-reasoning/mutations, full Q4/runtime regression,
  roadmap ledger, and lifecycle currency.
- Next lifecycle action: `fsgg-sdd specify --work 365-handbook-m4`.
