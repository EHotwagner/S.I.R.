---
schemaVersion: 1
workId: 232-viewport-chunking-semantic-zoom
title: Deterministic viewport chunking, isolated SVG reconciliation, and frame-coalesced presentation
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

# Deterministic viewport chunking, isolated SVG reconciliation, and frame-coalesced presentation Charter

## Identity
- Make retained SVG construction and per-frame presentation work proportional to the viewport's deterministic spatial working set while keeping semantic primitive identity stable.
- Add screen-size and interaction-aware semantic detail tiers without hiding disclosed facts from inspector, roster, or accessibility alternatives.
- Isolate the tactical SVG behind explicit scene and per-layer revision tokens, and coalesce high-frequency camera/pointer invalidation into one visibility-aware animation-frame presentation update.

## Principles
- Chunking and culling are pure presentation projections; simulation, validation, route search, disclosure, and evidence export continue to consume complete authoritative state.
- A semantic primitive has one durable identity even when its bounds cross chunk boundaries, and deterministic ordering must not depend on hash iteration or browser timing.
- Overscan and tier thresholds are trace-backed structural budgets, not supported project-size ceilings.
- Selected, hovered, and keyboard-focused objects retain predictable interaction and accessible alternatives when their geometry is outside the rendered viewport.

## Scope Boundaries
- In: deterministic chunk addressing/query, viewport plus overscan culling for shared scene layers and overlays, semantic detail tiers, revision-keyed tactical SVG ownership, animation-frame camera/pointer coalescing, hidden-page cancellation/restart, production SVG integration, accessibility fallbacks, focused F# and Chromium evidence, performance-intent updates, and the three-file retained measurement compatibility repair required by culling.
- Out: simulation/world partitioning, validation or pathfinding culling, disclosure changes, Canvas/WebGL migration, worker transport changes, and any permanent maximum map or project size.
- Work is limited to the issue's ten declared source, focused-test, browser-test, stylesheet, performance-document, and measurement-harness paths plus lifecycle-owned work/readiness artifacts; `BrowserInfrastructure.fs` is admitted specifically for the presentation frame owner and visibility lifecycle, while the three measurement files are admitted only for resize-before-Fit and culling-aware structural assertions.

## Policy Pointers
- SDD policy comes from `.fsgg/sdd.yml` and `.fsgg/agents.yml`.
- Constitution I requires analyzed specification before implementation; V keeps presentation filtering outside authoritative update state; VI requires fail-before/pass-after evidence; VIII requires invalid viewport/capability input to degrade deterministically and visibly.
- `docs/performance-budget.md` is the producer-owned performance intent and the retained #231 Chromium evidence is the baseline for this optimization.
- Governance files are optional compatibility pointers; protected-host browser evidence remains separately identified when available.

## Lifecycle Notes
- Tier 1: this changes the production scene projection/render contract and its observable DOM/accessibility metadata.
- The pnext performance-first gate requires a pre-edit baseline and exact-candidate Release production-browser evidence.
- Next lifecycle action: `fsgg-sdd specify --work 232-viewport-chunking-semantic-zoom`.
