---
schemaVersion: 1
workId: 183-tactical-overlays
title: Tactical Overlays
stage: plan
changeTier: tier1
status: planned
sourceSpec: work/183-tactical-overlays/spec.md
sourceClarifications: work/183-tactical-overlays/clarifications.md
sourceChecklist: work/183-tactical-overlays/checklist.md
publicOrToolFacingImpact: true
---

# Tactical Overlays Plan

Prose status: planned

## Source Snapshot
- spec: work/183-tactical-overlays/spec.md sha256:bba9f80efc9a44733cea71067c06a03c6c7aabc771af8c364ce92e4ae3c949e1 schemaVersion:1
- clarifications: work/183-tactical-overlays/clarifications.md sha256:e4f263fc9559a4eba5d053ab0674ddaa10bd11935a29b84735ac293a20c20914 schemaVersion:1
- checklist: work/183-tactical-overlays/checklist.md sha256:208ea798575d813797c4d2e7210cb1e3d6811c0f108dfb60df3d23d2881ddf45 schemaVersion:1

## Plan Scope
- Tier-1 additive Client/Web change over existing authoritative spatial, awareness, combat, planning, simulation, and replay projections.
- Declare `TacticalSceneProjection.fsi` first, implement pure registry/preference/disclosure/order/counter projections in Client, then bind shared commands, Web state/persistence/View controls/SVG semantics, tests, docs, and evidence.
- Existing public `SharedSceneProjection` remains the renderer-neutral handoff; additive overlay registry/payload/preferences/counters become explicit fields/surfaces rather than Web-only state.

## Technical Context
- F#/.NET and Fable share Client code; Feliz renders the production SVG/menus and browser localStorage is an edge effect behind pure preference import/export.
- Current scene inputs already carry disclosure-labelled units/routes/annotations and authoritative planning/simulator/replay facts; absent owning facts remain unavailable rather than approximated.
- `UnifiedTacticalWorkspace` owns shared commands/effective bindings; `CommandRegistry` adapts them for the active product; App routes pointer/key events to one command ID.

## Constitution Check
- I/III: spec/checklist precede code and additive public records/unions/functions land in `.fsi` with tests/docs.
- IV/V: pure records/unions and transitions calculate effective state/payload/order/counters; localStorage and DOM event handling stay at Web edges.
- VI/VIII: .NET/Fable/browser/performance evidence, fail-closed disclosure/unreadable inputs, and subject mutations make every new/modified gate observable.

## Plan Decisions
- PD-001 [AC-001] [FR-001] [DEC-001] complete: Declare stable overlay ID/category/mode/default/command/availability/disclosure/order/payload descriptors and the canonical fourteen-entry registry in `TacticalSceneProjection.fsi`; test exact IDs, uniqueness, order, and command linkage.
- PD-002 [AC-001] [FR-002] [DEC-001] [DEC-004] complete: Add pure preference import/export and effective-mode resolution constrained by supported modes with deterministic unknown/stale fallback and precedence held over selection over persisted/default.
- PD-003 [AC-001] [FR-003] [DEC-004] complete: Extend shared workspace commands and active Web registry from overlay descriptors, dispatch pointer/keyboard/hold release through identical IDs, render checked/effective shortcuts, and persist overlay preferences under a versioned key independent of layout.
- PD-004 [AC-002] [FR-004] [DEC-002] complete: Project initial typed payloads only from `SharedSceneProjection` and disclosed owner projections already supplied by Editor/Planning/Simulator/Review; scene adapters translate payload shape but never calculate LOS/cost/cover/armor truth.
- PD-005 [AC-002] [FR-005] [DEC-002] complete: Preserve exact LOS path cells/semantic edge blockers and authoritative route cost/blocker explanation as typed payload fields; focused tests use corner/door fixtures and a deliberate approximate-line mutant.
- PD-006 [AC-003] [FR-006] [DEC-002] complete: Map stable subjects/events/ticks for planned routes, reservations, area engagements, suppression, attacks/impacts, health/wounds, and command state from existing route/annotation/unit status facts without hidden source invention.
- PD-007 [AC-003] [FR-007] [DEC-007] complete: Recompute time-varying payloads from the selected replay frame and canonical IDs/order on each seek; .NET/Fable fingerprints and a browser seek journey prove no mutable DOM history authority.
- PD-008 [AC-004] [FR-008] [DEC-003] complete: Apply disclosure filtering before payload/availability construction, emit one generic fail-closed unavailable shape and constant structural counters for hidden/malformed/unreadable variants, and test two-perspective/forbidden-token/timing-class equivalence.
- PD-009 [AC-005] [FR-009] [DEC-005] complete: Sort payloads/labels by held-selection priority, registry order, subject/event, and primitive ID; deterministic collision buckets keep one winner and enforce the 256-label cap.
- PD-010 [AC-005] [FR-010] [DEC-005] complete: Add three semantic-zoom bands, explicit high-contrast and monochrome/pattern data semantics, non-color-only SVG/CSS affordances, and Playwright checks at 400% zoom with pointer/keyboard operation.
- PD-011 [AC-006] [FR-011] [DEC-006] complete: Extend the production Client performance qualification with deterministic 100/200-unit update/projection/view workloads, predeclared caps/counters, Release timing/allocation/host receipts, SVG-node browser checks, definition digest, and explicit compositor-not-measured capability.
- PD-012 [AC-007] [FR-012] [DEC-007] complete: Add focused/full build, public-surface, Fable, replay, production-browser, accessibility, performance, docs, lifecycle, schema-v2 feedback, and exact-head evidence plus one protected-subject/unreadable mutation per touched gate and independent review handoff.

## Contract Impact
- PC-001 [PD-001] [PD-002] publicSurface: `src/SIR.Client/TacticalSceneProjection.fsi` declares overlay registry, modes, preferences, availability/disclosure, payloads, ordering, labels, and cost-counter surfaces before implementation.
- PC-002 [PD-003] commandRegistry: Overlay command IDs are derived from descriptors and resolve through `UnifiedTacticalWorkspace` effective bindings; App contains no parallel shortcut truth.
- PC-003 [PD-004] [PD-005] authorityBoundary: Client accepts disclosed authoritative scene facts, exact LOS/path evidence, and typed absence; SceneAdapters/Web only transform coordinates/style.
- PC-004 [PD-006] [PD-007] replayProjection: Overlay subject/event/tick identities and order derive from selected authoritative frame and reconstruct identically across seeks/runtimes.
- PC-005 [PD-008] disclosureContract: Disclosure precedes payload/availability, with an indistinguishable generic unavailable result for hidden/malformed/unreadable sources.
- PC-006 [PD-009] [PD-010] presentationContract: Registry order, bounded collision suppression, zoom bands, contrast/pattern tokens, and SVG data semantics are deterministic renderer inputs.
- PC-007 [PD-011] performanceContract: `docs/performance-budget.md` owns the overlay 100/200-unit definition, 4,096/256/5,000/one-pass caps, 20/50 ms posture, receipt fields, and compositor limitation.

## Verification Obligations
- VO-001 [PD-001] [PD-002] [PC-001] semanticTest: Assert all fourteen descriptors, unique stable IDs/commands/order, supported modes/defaults, deterministic preference round-trip/stale fallback, precedence, and exact .NET/Fable fingerprint.
- VO-002 [PD-003] [PC-002] commandRouteTest: Assert View checks/effective shortcuts come from registry, pointer/key/hold-release dispatch agree, rebinding is honored, and overlay storage round-trips independently from layout.
- VO-003 [PD-004] [PD-005] [PD-006] [PC-003] authorityTest: Real corner/door/path/combat/awareness/planning/replay fixtures assert exact payloads/costs/blockers/directions/identities and source scans reject Web-side tactical inference.
- VO-004 [PD-007] [PC-004] replayRuntimeTest: Native/Fable fingerprints match for multiple seeks and browser scrubbing reconstructs attack/suppression/reservation state without DOM-history dependence.
- VO-005 [PD-008] [PC-005] disclosureTest: Two observers plus hidden/malformed/unreadable fixtures prove no forbidden geometry/count/label/diagnostic/timing-class leak and identical fail-closed counters.
- VO-006 [PD-009] [PD-010] [PC-006] accessibilityTest: Dense multi-overlay fixtures assert stable z-order/collision winners/label cap/zoom bands/pattern tokens; built Playwright verifies View operability, checks, accessible shortcuts, and 400% zoom.
- VO-007 [PD-011] [PC-007] performanceTest: Release 100/200 production routes enforce 4,096 payload, 256 label, 5,000 SVG node, single-pass, 20/50 ms budgets and record deterministic counters, allocation/GC, digest, host/candidate, and compositor absence.
- VO-008 [PD-012] gateMutationTest: Break approximate LOS, disclosure ordering, registry-menu linkage, collision/order caps, runtime equality, performance caps, and unreadable receipts one subject at a time; each owning gate must red and recover green.
- VO-009 [PD-012] lifecycleTest: Run Dev/Test/Verify, focused .NET/TRX, Fable/Node, production browser/journey/accessibility, docs, schema-v2 feedback/audit/checkpoints, SDD evidence/verify/ship/refresh/agents, exact-head CI/path/claim, and independent review validators.

## Performance Intent
No performance intent is declared for this work item.

## Migration Posture
- PM-001 [PC-001] additive-versioned: Existing scene fields remain valid; overlay fields/preferences use explicit schema/version and unknown IDs/modes fall back without mutating stored layout.
- PM-002 [PC-002] registry-authority: Existing shared shortcut/custom-binding resolution is extended; no duplicate App key map is retained.
- PM-003 [PC-003] authority-preserving: Existing projection inputs are reused; unavailable owning facts produce no overlay rather than inferred substitutes.
- PM-004 [PC-004] replay-derived: Retained frames preserve their recorded facts and never synthesize current overlay truth; projection is regenerated from the selected frame only.
- PM-005 [PC-005] disclosure-first: Any previous presentation-only layer derived before disclosure is replaced by the typed filtered route rather than kept as fallback.

## Generated View Impact
- GV-001 [PD-011] evidenceViews: Overlay performance, Fable/runtime, browser/accessibility, journey, mutation, and documentation receipts bind their workload/source digests and exact candidate or fail unreadable/stale.
- GV-002 [PD-012] lifecycleViews: Analysis, work model, summary, equivalent Claude/Codex guidance, verify, ship, optional Governance handoff, and committed ship verdict refresh from current authored sources/evidence.

## Accepted Deferrals
No accepted plan deferrals recorded.

## Planning Findings
No blocking planning findings recorded.

## Advisory Notes
- No new upstream Game.Core surface is required: exact LOS/grid results are consumed through existing S.I.R. authoritative projections, following the grids/line-drawing/visibility boundary.
- The browser workload measures the built production route and SVG structure; absence of a live compositor is reported as a capability limit, not silently upgraded.

## Lifecycle Notes
- Next lifecycle action: `fsgg-sdd tasks --work 183-tactical-overlays`.
