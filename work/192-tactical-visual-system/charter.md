---
schemaVersion: 1
workId: 192-tactical-visual-system
title: Tactical visual system
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

# Tactical visual system Charter

## Identity
Issue 192 establishes the production tactical visual system for the shared workscreen. It turns the existing exact scene projection into a coherent grounded near-future and arcane-fantasy game presentation without moving simulation, disclosure, or replay authority into rendering.

## Principles
- Tactical truth, disclosure, and replay identity remain authoritative inputs; presentation is a deterministic projection.
- Information clarity and density outrank spectacle. Unit footprint, faction, role, facing, health, selection, intent, and decisive effects remain scannable.
- Box-shaped unit pieces remain the game language; this work does not replace them with character models.
- Motion and effects explain causality, converge to committed frames, and retain equivalent state-change feedback under reduced motion.
- Performance is designed before implementation around representative 100-unit and stress 200-unit production routes, structural counters, and frame-work budgets.
- Accessibility is multi-channel: palette, shape, hierarchy, text, and reduced motion compose at narrow widths and 400% browser zoom.

## Scope Boundaries
- In: renderer tokens, battlefield projection metadata, motion/effect grammar, tactical workspace presentation, deterministic fixtures, browser journeys, production documentation, and focused performance qualification.
- In: authorized updates to legacy visual samples when their prior appearance conflicts with the production system.
- Out: combat, spatial, perception, overlay, or disclosure truth; new authoritative simulation events; cinematic camera behavior; character/vehicle model replacement; Governance enforcement.
- Exact analytical overlays remain owned by issue 183 and are consumed compositionally rather than redefined.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- The product constitution requires specify-before-implement, declared public surfaces, pure update boundaries, and fail-before/pass-after evidence.
- `docs/visual-direction.md`, `docs/unified-tactical-workspace.md`, and `docs/performance-budget.md` are the producer-owned design and performance intent.
- The canonical board is the user-owned EHotwagner / S.I.R. project 6.

## Lifecycle Notes
- Complete charter through implementation-ready analysis before editing declared visual source paths.
- Use focused tests while authoring, one source-frozen aggregate after implementation, metadata-only feedback validation, and one final hosted exact-SHA CI run.
