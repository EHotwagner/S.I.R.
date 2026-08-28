---
schemaVersion: 1
workId: 382-handbook-roadmap-rollup
title: Combat in Quint handbook roadmap terminal roll-up
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

# Combat in Quint handbook roadmap terminal roll-up Charter

## Identity
- Work id: `382-handbook-roadmap-rollup` for `EHotwagner/S.I.R.#382`.
- Complete the parent-owned terminal `$work-roadmap` cross-cycle reporting obligation after M7 without creating another roadmap milestone or feedback cycle.

## Principles
- Repository artifacts, not a hard-coded cycle list, determine the complete roadmap-cycle set.
- Every checkpoint is an individual reporting obligation; cycle-level aggregation cannot hide an omitted record.
- Existing schema-v2 reports, audits, and checkpoint streams remain immutable process evidence. The roll-up validates and cites them but does not rewrite their conclusions.
- Every new machine-reporting gate must ship with an isolated observed-red/restored-green control.

## Scope Boundaries
- In: one public final report, a fail-closed audit/qualifier, exact totals and coverage matrix, a roadmap link, independent content and implementation review, and exact delivery proof.
- Out: handbook semantic changes, combat/model/runtime changes, edits to prior feedback artifacts, a new milestone, a self-referential feedback cycle, and unrelated CI/product qualification.
- The dirty `/home/developer/projects/S.I.R-roadmap-m2` worktree is outside this item's ownership and must remain untouched.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml`, `.fsgg/agents.yml`, and `.fsgg/constitution.md`.
- Roadmap completion and feedback gates come from `.agents/skills/work-roadmap/SKILL.md` plus its host-loop, ledger, and feedback-contract references.
- Project state is projected through the typed coordination engine onto the user-owned EHotwagner / S.I.R. Project 6.

## Lifecycle Notes
- This terminal report deliberately has no `roadmap-sir-combat-quint-handbook-*` feedback cycle of its own; creating one would make completion recursive.
- Independent feedback-coverage/content review and a separate exact-head implementation review are mandatory before guarded merge.
- Next lifecycle action: `fsgg-sdd specify --work 382-handbook-roadmap-rollup`.
