---
schemaVersion: 1
workId: 183-tactical-overlays
title: Tactical Overlays
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/183-tactical-overlays/spec.md
publicOrToolFacingImpact: true
---

# Tactical Overlays Clarifications

## Source Specification
- work/183-tactical-overlays/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: Which stable overlay descriptors and supported visibility modes form v1?
- CQ-002 [AMB:AMB-002]: Which authoritative inputs and exact LOS/path semantics may each overlay consume?
- CQ-003 [AMB:AMB-003]: Where is disclosure applied and what unavailable/malformed result is observable?
- CQ-004 [AMB:AMB-004]: How do View commands, held/selection/persistent state, and separate preference persistence compose?
- CQ-005 [AMB:AMB-005]: Which order, suppression, zoom, contrast, and pattern rules define legible v1 output?
- CQ-006 [AMB:AMB-006]: Which workload, counters, structural caps, and timing posture qualify 100/200-unit scenes?
- CQ-007 [AMB:AMB-007]: Which production routes and replay states make the feature genuinely player reachable and scrub-able?

## Answers
- CQ-001 → v1 registers fourteen IDs: `unit.footprints`, `unit.body-facing`, `awareness.attention-vision`, `spatial.exact-los`, `movement.reachable-path-cost`, `cover.exposure`, `combat.armor-coverage`, `movement.planned-routes`, `movement.reservations`, `combat.area-engagements`, `combat.suppression`, `combat.attack-traces`, `combat.hp-wounds`, and `command.state`. Every descriptor declares category, order, default, supported subset of held/selection/persistent modes, `view.overlay.<id>` command, availability/disclosure policy, and payload kind; `Off` is universally valid.
- CQ-002 → registry payloads reuse the current `SharedSceneProjection` terrain/edges/units/routes/annotations plus disclosed awareness/physical-combat/replay fields already supplied by owning projections. Exact LOS is represented only from authoritative exact LOS evidence with supercover/corner and semantic-edge door identity preserved; path/cost/blocker payloads retain authoritative values. Missing authority yields unavailable/empty—not a Client or Web approximation.
- CQ-003 → a single Client projection boundary receives disclosure-bearing source values, filters subjects/events first, and only then creates payload arrays and availability. An undisclosed, malformed, or unreadable source produces the same closed shape: no payload/geometry/count/label/diagnostic detail and generic unavailable metadata whose construction path/counters do not distinguish the hidden cause.
- CQ-004 → one `OverlayPreferences` value stores persistent modes by stable ID and is imported/exported under its own schema/storage key, never the layout profile. Effective state is deterministic precedence `held > selection > persisted/default > off`, constrained by descriptor support. Pointer and keyboard dispatch the same registry command; effective shortcut resolution stays in `UnifiedTacticalWorkspace`/`CommandRegistry`; key-up ends a hold.
- CQ-005 → descriptor order is the only z-order source: footprint/facing, movement, awareness/LOS, protection, planning/reservations, engagements/suppression, attacks/health/command. Label candidates sort by selected/held priority, descriptor order, subject/event identity, then primitive ID; accept at most one per collision bucket and cap 256 labels. Zoom bands hide fine labels below 0.75, show selected explanation at 0.75-1.49, and show full bounded detail from 1.5; high contrast and monochrome use explicit stroke/pattern tokens rather than color-only meaning.
- CQ-006 → the production update/projection/view workload uses deterministic representative 100-unit and stress 200-unit disclosed scenes after warm-up. Caps are 4,096 overlay payload primitives, 256 labels, 5,000 total SVG nodes, one registry traversal and one disclosure pass per frame; Release p95 targets are 20 ms representative and 50 ms stress on the qualification host. The receipt records definition digest, candidate/host/runtime, counters, allocation/GC, timings, and explicitly says no live-compositor evidence was measured.
- CQ-007 → Playwright boots the built production entry, opens View, toggles one selection overlay by pointer, invokes the same command by keyboard, holds/releases exact LOS, reloads to prove independent preference restore, selects a unit, and scrubs a replay attack/suppression tick. .NET/Fable tests exact-compare registry/payload/preference/order fingerprints; browser evidence observes the built renderer rather than injecting messages.

## Decisions
- **DEC-001** [CQ-001] [AMB:AMB-001] [FR-001] [FR-002]: Adopt the fourteen stable IDs and descriptor-declared supported modes; `Off` is universal and unknown IDs/modes fail to deterministic defaults without becoming ad hoc layers.
- **DEC-002** [CQ-002] [AMB:AMB-002] [FR-004] [FR-005] [FR-006]: Extend `TacticalSceneProjection` as the renderer-neutral aggregation boundary, consuming only existing disclosed projection values and preserving exact LOS/path/door evidence; absence never licenses recomputation.
- **DEC-003** [CQ-003] [AMB:AMB-003] [FR-008]: Filter disclosure before registry availability and payload construction, returning one indistinguishable fail-closed unavailable shape for hidden/malformed/unreadable authority.
- **DEC-004** [CQ-004] [AMB:AMB-004] [FR-002] [FR-003]: Keep overlay preferences schema/storage independent of layout, compute held/selection/persisted precedence purely, and route pointer/keyboard through the same shared command descriptor and effective binding.
- **DEC-005** [CQ-005] [AMB:AMB-005] [FR-009] [FR-010]: Registry order owns z-order; deterministic priority/collision buckets cap labels at 256; three semantic-zoom bands and explicit contrast/pattern tokens preserve non-color-only meaning.
- **DEC-006** [CQ-006] [AMB:AMB-006] [FR-011]: Qualify 100/200-unit production projection/view with caps 4,096 payloads, 256 labels, 5,000 SVG nodes, one traversal/filter pass, and Release 20/50 ms p95 posture while labeling compositor capability absent.
- **DEC-007** [CQ-007] [AMB:AMB-007] [FR-007] [FR-012]: Require exact .NET/Fable fingerprints plus a built-entry Playwright pointer/keyboard/hold/restore/selection/replay-scrub journey; no direct message injection or mutable DOM history counts.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
- None. AMB-001 through AMB-007 are resolved by DEC-001 through DEC-007.

## Lifecycle Notes
- No accepted clarification deferrals; each issue requirement has a concrete v1 decision.
- Next lifecycle action: `fsgg-sdd checklist --work 183-tactical-overlays`.
