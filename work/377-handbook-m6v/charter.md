---
schemaVersion: 1
workId: 377-handbook-m6v
title: S.I.R. combat Quint handbook M6V visual explanations
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

# S.I.R. combat Quint handbook M6V visual explanations Charter

## Identity
- Work id: `377-handbook-m6v` for issue `EHotwagner/S.I.R.#377`.
- Complete roadmap M6V by adding mechanically trustworthy visual explanations to the combat Quint handbook without making diagrams an independent semantic authority.

## Principles
- Concrete mechanics visuals reuse the production `UnitGlyphCatalog`, palette, battlefield footprint, contact, cover, health, and status vocabulary; abstract formal visuals are pure SVG.
- `docs/rules/sir-combat.md`, `src/SIR.Simulation/CombatRules.fs`, `src/SIR.Client/UnitGlyphCatalog.fs`, and the checked handbook manifests remain authority. A diagram manifest describes bindings and budgets but does not redefine rules.
- SVG animation and filter effects are progressive presentation only. Static SVG geometry, text, and accessible descriptions carry the full meaning under reduced motion, print, and unsupported-effect routes.
- Every new gate ships with an isolated observed-red/restored-green mutation, including authority drift, glyph drift, accessibility loss, fallback loss, render drift, and performance overflow.

## Scope Boundaries
- In: concrete mechanics diagrams; pure-SVG state, dependency, arithmetic, trace, and invariant diagrams; source bindings; progressive CSS animation and SVG-filter effects; reduced-motion/static/print/non-WebGL fallbacks; accessible title/description/labels; rendered-browser inspection, visual regression, and typed performance evidence; handbook and roadmap integration.
- Out: combat/model/runtime semantic changes, new unit glyphs, Canvas/WebGL authority, simulation GPU work, package upgrades, M7 final domain/editorial publication review, and any claim of live-compositor evidence from this headless host.
- M6V accepts and must discharge the visual/performance handoff recorded by M3, M4, M5, and M6. M7 stays pending.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml`, `.fsgg/agents.yml`, and `.fsgg/constitution.md`.
- Performance authority is `docs/performance-budget.md` and `scripts/lib/performance-budget.mjs`; established SVG/glyph authority is `src/SIR.Client/UnitGlyphCatalog.fs` and `src/SIR.Client/Battlefield.fs`.
- Publication ledger and content are `docs/sir-combat-quint-handbook-roadmap.md` and `docs/sir-combat-quint-handbook.md`; formal/rule authorities remain `docs/rules/sir-combat.md` and `src/SIR.Simulation/CombatRules.fs`.

## Lifecycle Notes
- Stable feedback cycle: `roadmap-sir-combat-quint-handbook-m6v-visual-explanations`.
- PERF-PLAN was invoked before implementation. The real route is strict FsDocs build to the generated handbook HTML, followed by headless Chromium inspection of embedded SVGs. Representative scale is six diagrams and the declared per-diagram/aggregate SVG budgets; the product's existing 100/200-unit tactical budgets are not redefined or falsely applied to documentation diagrams.
- PERF-SMOKE must capture baseline strict-docs render, SVG element/byte/effect counters, browser capability facts, reduced-motion/print inspection, and absence of a live compositor before implementation.
- Full SDD, four feedback checkpoints, independent exact-head implementation review, independent feedback critique, relevant hosted CI, merge, and exact-main Pages proof are mandatory.
- Next lifecycle action: `fsgg-sdd specify --work 377-handbook-m6v`.
