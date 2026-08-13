---
schemaVersion: 1
workId: 183-tactical-overlays
title: Configurable exact tactical overlays and View-menu analysis controls
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

# Configurable exact tactical overlays and View-menu analysis controls Charter

## Identity
Deliver a deterministic, renderer-neutral tactical-overlay registry and VS Code-style View controls that let players inspect exact authoritative battlefield explanations without obscuring play or disclosing unavailable information.

## Principles
- Spatial, combat, awareness, replay, and command-state truth stays in its owning authoritative projection; Client aggregates disclosed values and Web renders them without re-derivation.
- Knowledge/disclosure filtering happens before any payload, geometry, count, diagnostic, or cost-observable projection is constructed.
- Overlay identity, capability, visibility modes, shortcut command identity, ordering, collision suppression, and preference persistence are one deterministic registry contract shared by pointer, keyboard, and restore routes.
- Exact selected-unit LOS consumes inherited exact LOS evidence and preserves corner/door semantics; no approximate Web ray or per-cell predicate loop becomes tactical truth.
- Renderer-neutral payloads remain replay-identical, scrub-able, accessibility-aware, semantically zoomed, and structurally bounded for 100-unit and 200-unit scenes.
- Public signatures, real-entry player journeys, .NET/Fable/browser/performance evidence, protected-subject mutations, schema-v2 feedback, and SDD readiness ship together.

## Scope Boundaries
- In: stable overlay metadata and payloads for footprints, body facing, attention/vision, exact LOS, reachability/path/cost/blockers, directional cover/exposure/armor, planned routes/reservations, area engagements, suppression, attack trace/impact, HP/wounds, and command state.
- In: off, hold-to-inspect, selection-scoped, and persistent modes where supported; checked View entries; effective configurable shortcuts from the shared command registry; independent preference persistence; deterministic z-order and label collision suppression.
- In: disclosure-first projection, semantic zoom, high-contrast and monochrome/pattern affordances, 400% browser zoom, 100/200-unit cost and SVG-node budgets, replay/timeline scrubbing, exact runtime/browser evidence, and failure-capable gates.
- Out: new combat, spatial, LOS, awareness, cover, armor, suppression, or attack rules; renderer-owned tactical inference; global omniscience; panel-layout redesign; and a new compositor/FPS promise not owned by the product performance contract.

## Policy Pointers
- Honor constitution I-III through specification-first work, declared `.fsi` surface changes, versioned contracts, and synchronized tests/docs.
- Honor constitution IV-V through plain F# records/unions and pure projection/update boundaries with browser persistence at the host edge.
- Honor constitution VI-VIII through real package/runtime/player evidence, protected-subject and unreadable-input mutations, deterministic structural counters, and typed absence for undisclosed/unavailable facts.
- Apply `.fsgg/sdd.yml`, `.fsgg/agents.yml`, `docs/performance-budget.md`, `docs/game-governance.md`, and the published FS.GG exact-grid/LOS contracts; Governance remains optional compatibility metadata.

## Lifecycle Notes
- Tier 1: this changes the public Client projection, shared command registry, production browser controls/persistence, rendered accessibility behavior, and executable performance/evidence contracts.
- The producer-owned performance baseline is the existing 20 Hz authoritative posture plus browser delivery and scene-structure budgets; this item will declare overlay projection/node caps and will not invent a live-compositor result.
- SDD and feedback artifacts were added to the live claim touch-set before authoring; no issue requirement is silently deferred.
- Next lifecycle action: `fsgg-sdd specify --work 183-tactical-overlays`.
