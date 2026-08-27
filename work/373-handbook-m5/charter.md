---
schemaVersion: 1
workId: 373-handbook-m5
title: S.I.R. combat Quint handbook M5 runtime correspondence
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

# S.I.R. combat Quint handbook M5 runtime correspondence Charter

## Identity
- Work id: `373-handbook-m5` for issue `EHotwagner/S.I.R.#373`.
- Complete roadmap M5 so readers can connect model claims to current production F# through scoped, reproducible evidence without merging their authorities.

## Principles
- `docs/rules/sir-combat.md` remains the literate model authority; extracted `.qnt` and ITF are generated projections, never authoring sources.
- Production claims require named F# subjects plus interpreter comparison evidence; Quint execution alone never establishes implementation equivalence.
- Exact fixtures establish only those fixtures; deterministic sampled replay establishes only its declared sample/seed/state boundary; missing mappings stay explicit.

## Scope Boundaries
- In: complete Quint-to-F# correspondence map; literate/generated pipeline; exact and sampled ITF replay walkthroughs; first-divergence reporting; observed-red/restored-green correspondence controls; safe rule-change workflow; dedicated qualification, lifecycle, feedback, review, and roadmap evidence.
- Out: combat semantic or production-code changes, M6 complete link/index enforcement, M6V SVG mechanics/theory visuals and effects, and M7 publication/maintenance handoff.
- Preserve all prior milestone history and later milestone wording/pending state; only M5 may become checked at the truthful merge-candidate boundary.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Model authority is `docs/rules/sir-combat.md`; runtime subjects are under `src/SIR.Domain/` and `src/SIR.Simulation/`; the publication and ledger are `docs/sir-combat-quint-handbook.md` and `docs/sir-combat-quint-handbook-roadmap.md`.

## Lifecycle Notes
- Stable feedback cycle: `roadmap-sir-combat-quint-handbook-m5-runtime-correspondence`.
- M2 and M3 deliberately deferred complete runtime correspondence to M5; this charter accepts and must discharge that handoff.
- Build separate focused correspondence, exact/sample replay, negative-control, full Q4/runtime, docs/link, roadmap, and lifecycle receipts.
- No typed performance intent exists; the pnext performance-first gate is not invoked for M5.
- Next lifecycle action: `fsgg-sdd specify --work 373-handbook-m5`.
