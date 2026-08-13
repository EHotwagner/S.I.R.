---
schemaVersion: 1
workId: 183-tactical-overlays
title: Tactical Overlays
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Tactical Overlays Specification

Prose status: specified

## User Value
Players can turn exact tactical explanations on only when useful, understand LOS, movement, protection, attacks, suppression, wounds, and command state from the same authoritative facts the game uses, and configure those views without hidden-information leaks or permanent battlefield clutter.

## Scope
- SB-001: Add a public renderer-neutral registry that declares stable overlay IDs, labels, categories, supported visibility modes, defaults, command IDs, availability/disclosure policy, deterministic order, and typed payloads.
- SB-002: Cover footprints, body facing, attention/vision, exact LOS, reachable/path/movement cost and blockers, directional cover/exposure, armor coverage, routes/reservations, area engagements, suppression, attack trace/impact, HP/wounds, and command state from existing authoritative projections.
- SB-003: Add checked View-menu controls and effective configurable shortcuts from the shared command registry for hold-to-inspect, selection-scoped, and persistent modes; persist overlay preferences separately from workspace-panel layout.
- SB-004: Apply disclosure before projection and render deterministic z-order, semantic zoom, collision/label suppression, high contrast, monochrome/pattern affordances, dense boards, and 400% zoom.
- SB-005: Ship exact .NET/Fable/replay/browser behavior, a real-entry player journey, 100/200-unit Release projection/node budgets, protected-subject mutations, schema-v2 feedback, and lifecycle evidence.

## Non-Goals
- SB-006: Do not define or approximate new spatial, LOS, cover, armor, combat, awareness, suppression, attack, reservation, wound, or command-state rules.
- SB-007: Do not expose authority-only world state, derive tactical truth in Web/DOM/CSS, couple overlay preferences to panel layout, or reinterpret retained replay through current state.
- SB-008: Do not claim live-compositor frame-rate qualification from bounded headless/browser structural measurements.

## User Stories
- US-001 (P1): As a tactical player, I can use View controls or shortcuts to inspect only the layers relevant to my selected unit and persist the layers I want always visible.
- US-002 (P1): As a player, I can trust exact LOS, path costs/blockers, cover/exposure, armor, attacks, suppression, wounds, and commands because the overlays project authoritative disclosed facts rather than renderer estimates.
- US-003 (P1): As a replay user, I can scrub attack, impact, suppression, reservation, engagement, wound, and command overlays deterministically.
- US-004 (P1): As a low-vision or zoomed user, I can distinguish enabled layers through semantic zoom, contrast, patterns, bounded labels, and keyboard/pointer parity.
- US-005 (P1): As an operator, I can qualify deterministic 100/200-unit overlay projection and SVG structure with explicit budgets and fail-capable evidence.

## Acceptance Scenarios
- AC-001 [US-001] [FR-001] [FR-002] [FR-003]: Given the production View menu, when a player uses pointer, keyboard, hold, selection, and persistent controls, then checked state and effective shortcuts come from one registry, both routes agree, and restored preferences reproduce the same modes independently of panel layout.
- AC-002 [US-002] [FR-004] [FR-005]: Given selected-unit corners, closed/open doors, reachable terrain, blockers, directional cover/exposure, and armor arcs, when overlays project, then exact inherited LOS and authoritative spatial/combat facts determine renderer-neutral geometry, costs, blockers, and directions without Web recomputation.
- AC-003 [US-002] [US-003] [FR-006] [FR-007]: Given authoritative engagements, reservations, suppression, attacks/impacts, HP/wounds, and command state across replay ticks, when the timeline is scrubbed, then typed payloads reconstruct the same ordered state and transient traces remain inspectable.
- AC-004 [US-002] [FR-008]: Given two observers over one authoritative world with different knowledge, when projections are built, then unavailable facts yield no payload, geometry, count, label, diagnostic, timing-class, or registry-availability leak.
- AC-005 [US-004] [FR-009] [FR-010]: Given multiple enabled overlays on a dense board at ordinary and 400% browser zoom under default, high-contrast, and monochrome/pattern modes, when rendered, then deterministic z-order and bounded collision suppression keep state legible and controls remain operable by pointer and keyboard.
- AC-006 [US-005] [FR-011]: Given representative 100-unit and stress 200-unit scenes after warm-up, when the production projection/view route runs in Release, then workload-bound deterministic counters, SVG-node caps, allocation/host facts, and declared timing limits pass without claiming compositor evidence.
- AC-007 [US-005] [FR-012]: Given mutations that approximate exact LOS, bypass disclosure, detach menu state from the registry, alter z-order/collision bounds, exceed performance caps, diverge runtimes, or make evidence unreadable, when owning gates run, then each gate rejects its protected-subject mutation.

## Functional Requirements
- FR-001: The system MUST declare one public renderer-neutral overlay registry with stable ID, label, category, default visibility, supported modes, shortcut command ID, availability, disclosure policy, deterministic z-order, and typed payload kind for every initial layer. (Stories: US-001; Acceptance: AC-001)
- FR-002: Overlay visibility MUST support `Off`, temporary hold-to-inspect, selection-scoped, and persistent modes only where each descriptor declares support, with deterministic fallback for stale/unknown preference values. (Stories: US-001; Acceptance: AC-001)
- FR-003: Checked View-menu entries, pointer dispatch, keyboard dispatch, effective shortcut display/configuration, and preference restoration MUST resolve through the shared overlay/command registries, and overlay preferences MUST persist independently of panel layout. (Stories: US-001; Acceptance: AC-001)
- FR-004: Client projection MUST consume disclosed authoritative spatial/combat/awareness state for footprints, facing, attention/vision, exact LOS, reachability/path/cost/blockers, directional cover/exposure, and directional armor; Web and scene adapters MUST NOT recompute tactical truth. (Stories: US-002; Acceptance: AC-002)
- FR-005: Selected-unit LOS MUST preserve exact inherited supercover/corner/semantic-door results and path payloads MUST retain authoritative costs and typed blockers; no approximate line, float ray, or per-cell LOS loop may substitute. (Stories: US-002; Acceptance: AC-002)
- FR-006: Renderer-neutral payloads MUST include planned routes/reservations, bounded area engagements, suppression, attack traces/impact, HP/wounds, and command state with stable subject/event/tick identity. (Stories: US-002, US-003; Acceptance: AC-003)
- FR-007: Time-varying payloads MUST reconstruct deterministically from replay/timeline state, retain canonical ordering, and remain scrub-able without relying on mutable DOM history. (Stories: US-003; Acceptance: AC-003)
- FR-008: Knowledge and disclosure filters MUST run before payload construction so unavailable facts emit no geometry, count, label, diagnostic detail, timing-class distinction, or availability clue; malformed/unreadable disclosure input MUST fail closed. (Stories: US-002; Acceptance: AC-004)
- FR-009: Multiple enabled layers MUST use registry-defined stable z-order plus bounded deterministic label/collision suppression, with selected/held information prioritized over persistent context. (Stories: US-004; Acceptance: AC-005)
- FR-010: Projection and rendering MUST expose semantic zoom plus high-contrast and monochrome/pattern semantics, preserve keyboard/pointer operability and checked-state meaning at 400% browser zoom, and avoid color-only distinctions. (Stories: US-004; Acceptance: AC-005)
- FR-011: A versioned performance workload MUST exercise the production update/projection/view route for representative 100-unit and stress 200-unit scenes, declare projection/payload/label/SVG-node and timing budgets before implementation, report deterministic counters and host capability facts, and label headless/browser evidence without compositor overclaim. (Stories: US-005; Acceptance: AC-006)
- FR-012: The change MUST ship synchronized `.fsi` surfaces, tests/docs, real-entry browser journey, .NET/Fable/replay evidence, schema-v2 feedback/audit/checkpoints, SDD readiness, protected-subject and unreadable-input mutations for every added/modified gate, exact-head CI, independent review, and guarded delivery. (Stories: US-005; Acceptance: AC-007)

## Ambiguities
- AMB-001: What exact stable overlay IDs/categories/payload kinds and per-layer supported visibility modes form v1?
- AMB-002: Which current Client projections are authoritative inputs for each layer, and how is exact LOS/corner/door/path evidence preserved without inventing missing truth?
- AMB-003: What disclosure envelope and fail-closed behavior prevent geometry/count/timing/diagnostic leaks while keeping registry metadata usable?
- AMB-004: How do registry state, the shared command registry, temporary holds, selection state, persisted preferences, and panel-layout persistence compose deterministically?
- AMB-005: What z-order, semantic-zoom, collision/label caps, contrast/pattern vocabulary, and 400% behavior define legible v1 output?
- AMB-006: What representative/stress workload, structural counters, and timing/node budgets extend the producer-owned performance posture?
- AMB-007: Which real-entry production controls and timeline states make the overlays player-reachable and replay-scrub-able across .NET/Fable/browser routes?

## Public Or Tool-Facing Impact
- Additive public F# registry, visibility-mode, availability/disclosure, payload, preference, projection, ordering, and cost-counter surfaces in `SIR.Client`.
- Add shared Web command descriptors/bindings and production View-menu/persistence/rendering behavior without browser authority.
- Documentation and evidence add overlay semantics, accessibility, performance workload/receipt, browser journey, mutation receipts, and lifecycle/feedback artifacts.

## Lifecycle Notes
- Tier 1 contracted change: public signatures, command registry, persisted behavior, production rendering/accessibility, performance contracts, and evidence change together.
- Exact LOS uses inherited package/authoritative spatial results; this item consumes rather than defines spatial/combat/awareness rules.
- Next lifecycle action: `fsgg-sdd clarify --work 183-tactical-overlays`.
