---
title: Combat in Quint handbook maintenance trigger
category: Battlefield Systems
categoryindex: 4
index: 49
description: Owner and update trigger adjacent to the authoritative S.I.R. combat Quint model.
date: 2026-08-28
status: maintained
document-type: maintenance
---

# Combat in Quint handbook maintenance trigger

This file sits beside the authoritative literate model at `docs/rules/sir-combat.md` so a model-only
change encounters the handbook obligation without changing the model bytes or making this notice a
semantic authority.

**Owner:** the S.I.R. repository maintainer responsible for the literate model and
`docs/sir-combat-quint-handbook.md`.

Any change to the literate model, `docs/combat-resolution.md`,
`src/SIR.Simulation/CombatRules.fs`, `docs/rules/sir-visibility.md`, the handbook vocabulary/diagram manifests,
the six base SVG assets, seven visibility SVG assets, Q4/runtime or visibility qualification, pinned tools, or documentation
publication route requires the dependency-ordered checklist in handbook chapter
48. Re-extract and typecheck the model first; then reconcile rules,
declarations, definitions, runtime correspondence, diagram bindings and fallbacks,
the existing M6V render/performance evidence, four independent base-publication
reviews, any explicitly qualified later extension, strict docs, relevant-only CI, and exact-SHA Pages. Never imply
that the inherited six-diagram measurement or base reviews cover later diagrams or substantive chapters.
