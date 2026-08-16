---
schemaVersion: 1
workId: 184-scenario-catalog
title: Scenario Catalog
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
performanceIntent:
  id: scenario-catalog-v1
  disposition: active
  targetFps: 20
  workloadIds:
    - scenario-catalog-representative-v1
  workloadDefinitionDigests:
    - scenario-catalog-representative-v1=sha256:9fdc78516912b2e440a6d80d3d6795032edc55876f010fcb96c9f6b03e1d67ac
  maximumExpectedScale: "80x80 map; 200 units; seven scenario packages"
  maxP95Ms: 20
  maxP99Ms: 50
  maxCatchUpFrames: 0
  structuralCostBudgets:
    - "combat-resolutions<=256"
    - "los-samples<=256"
    - "path-expansions<=4096"
    - "scene-nodes<=8000"
  requiredCapability: headless-browser
  liveCompositorRequired: false
  evidenceRefs:
    - docs/performance-budget.md
  rationale: "The catalog is an interactive workload input; deterministic structural limits and the existing 20 Hz/50 ms simulation posture gate it, while headless evidence does not claim compositor coverage."
---

# Scenario Catalog Specification

Prose status: specified

## User Value
Players and designers can open, understand, fork, run, scrub, and compare substantial deterministic tactical scenarios through the production Samples, simulation, replay, and browser routes. The same catalog gives maintainers durable conformance, regression, visual-review, and performance workloads.

## Scope
- SB-001: Add a versioned scenario package and catalog over the existing `ExperienceSamples`, map-editor handoff, simulator, replay, and persistent tactical workscreen routes.
- SB-002: Include a fast-start teaching scenario and at least six deliberately composed families: open-field movement/fire, cover-dense assault/flank, door breach/interior clear, support-by-fire/suppression, armored target/anti-armor response, and multi-objective withdrawal/reinforcement.
- SB-003: Qualify package identity, canonical checkpoints, retained replays, production browser behavior, .NET/Fable equality, gate mutations, and representative/stress cost.

## Non-Goals
- SB-004: Random map generation, campaign persistence, matchmaking, comprehensive balance, and replacement spatial/combat/awareness algorithms are not part of this work.

## User Stories
- US-001 (P1): As a new player, I can open a compact teaching scenario from File → Samples, read its lesson, and run or scrub its checkpoints through ordinary controls.
- US-002 (P1): As a tactical player or designer, I can explore six materially different, editable scenario families with larger maps, varied rosters, terrain, edges, objectives, facing/attention, and multiple plans.
- US-003 (P1): As a maintainer, I can round-trip versioned packages and reject stale engine/ruleset/content/replay bindings without partial admission.
- US-004 (P1): As a maintainer, I can reproduce identical canonical event/checkpoint streams in .NET and Fable and see protected-subject mutations fail.
- US-005 (P1): As an operator, I can run representative and stress catalogs through production update/projection/view routes with deterministic cost counters and explicit timing/capability limits.

## Acceptance Scenarios
- AC-001 [US-001] [US-002] [FR-001] [FR-002] [FR-003]: Given the production browser, when a player opens File → Samples, then every scenario appears with its family and lesson, loads through the real editor/simulator route, remains editable/forkable under policy, and can run or scrub to its visible checkpoints.
- AC-002 [US-002] [FR-002] [FR-004]: Given the catalog, when its scenarios are inspected, then it contains a fast-start lesson plus all six required families on larger maps with materially larger rosters, authored terrain/semantic edges/zones/objectives, varied capabilities/loadouts, initial facing/attention/knowledge, plans, and at least two documented tactical approaches where applicable.
- AC-003 [US-003] [FR-001] [FR-005]: Given an exported package, when it is imported unchanged, then canonical content round-trips; when schema, engine, ruleset, content, map revision, or retained-replay binding is stale or malformed, admission fails closed with a specific diagnostic and no partial scenario.
- AC-004 [US-004] [FR-006] [FR-007]: Given every catalog scenario and its addressed seed, when .NET and Fable run the canonical route, then ordered events and checkpoints are identical and retained replay identity remains bound to the scenario package.
- AC-005 [US-004] [FR-008]: Given one mutation at a time for a missing unit, altered geometry, changed event order, stale replay binding, or unreadable evidence, when its owning gate runs, then the gate reports red and returns green only after the protected subject is restored.
- AC-006 [US-005] [FR-009]: Given the representative full catalog and a stress catalog after warm-up, when Release qualification traverses load, simulation update, spatial/path/LOS/combat, timeline projection, and browser view, then declared deterministic counters and timing budgets pass with host/browser/compositor capability reported honestly.
- AC-007 [US-001] [US-004] [FR-010]: Given the shipped production entry, when a bot uses player-emittable controls from boot, then it opens a named sample, reaches the simulator, advances and scrubs to a declared checkpoint, and the independent acceptance matrix maps every required AC to exact production-route evidence.

## Functional Requirements
- FR-001: The system MUST declare a versioned scenario package with stable catalog/scenario/family IDs; schema version; engine, ruleset, content, map and revision identities; forces, capabilities/loadouts, initial knowledge, seed/addressed randomness, plans, objectives, expected checkpoints, retained replay binding, and design notes. (Stories: US-001, US-003; Acceptance: AC-001, AC-003)
- FR-002: The catalog MUST contain a compact teaching scenario and at least six required tactical families with larger maps and materially larger rosters than the current minimal samples. (Stories: US-001, US-002; Acceptance: AC-001, AC-002)
- FR-003: File → Samples MUST list the versioned catalog, expose lesson/design notes, load through ordinary editor and simulator handoff, preserve edit/fork policy, and run/scrub via normal simulator/replay controls without private state injection. (Stories: US-001, US-002; Acceptance: AC-001)
- FR-004: Each composed scenario MUST include meaningful authored terrain, semantic edges, zones/objectives, unit variety and loadouts, initial facing/attention/knowledge, plans, and documented alternative tactical solutions while exercising the applicable spatial, combat, awareness, overlay, and environment capabilities. (Stories: US-002; Acceptance: AC-002)
- FR-005: Package encode/decode MUST round-trip canonically and validate all identity/binding fields atomically, rejecting unsupported schema and stale engine/ruleset/content/map/replay identity with stable diagnostics and no partially admitted package. (Stories: US-003; Acceptance: AC-003)
- FR-006: Every scenario MUST carry deterministic semantic checkpoints and a retained replay whose ordered event/checkpoint stream is derived from the production simulator route and bound to scenario identity. (Stories: US-003, US-004; Acceptance: AC-004)
- FR-007: Headless .NET and built Fable execution MUST produce byte-identical canonical catalog/event/checkpoint fingerprints for the same package and addressed seed. (Stories: US-004; Acceptance: AC-004)
- FR-008: Added or modified gates MUST ship fail-capable mutation controls for missing units, geometry change, event reordering, stale replay binding, and unreadable/stale evidence; visual snapshots alone MUST NOT satisfy semantic acceptance. (Stories: US-004; Acceptance: AC-005)
- FR-009: A versioned representative full-catalog workload and larger stress workload MUST traverse production load, update, spatial/path/LOS/combat, timeline projection, and view; declare structural and timing budgets before implementation; expose deterministic counts for scenarios/maps/units/edges/zones/ticks/events/checkpoints/path expansions/LOS samples/combat resolutions/projection frames/scene nodes; preserve the established 8,000-node production cap; and distinguish headless, browser, and compositor capability. (Stories: US-005; Acceptance: AC-006)
- FR-010: Evidence MUST include a bot-driven real-entry production player journey plus browser matrix, .NET/Fable conformance, focused/full tests, performance, docs, SDD, feedback, exact-head CI/path/claim, gate-inversion, and independent-review receipts. (Stories: US-001, US-004, US-005; Acceptance: AC-007)

## Ambiguities
- AMB-001: What exact package shape and identity constants form scenario catalog v1, and which fields participate in its canonical digest?
- AMB-002: Which authored scenario IDs, map sizes, roster scales, checkpoints, and tactical lessons satisfy the six-family matrix without pretending the current controller implements unshipped mechanics?
- AMB-003: How are facing, attention, initial knowledge, loadout/capability, plans, objectives, and alternative solutions represented while preserving compatibility with the existing map text and simulator handoff?
- AMB-004: What representative and stress workload sizes, structural counters, and timing budgets extend the producer-owned performance intent?
- AMB-005: Which browser controls and real composition-root path prove every scenario is reachable and one scenario can advance/scrub from player input?
- AMB-006: What exact mutations and cross-runtime fingerprints prove the catalog, identity, geometry, ordering, replay, and evidence gates are checking their declared subjects?

## Public Or Tool-Facing Impact
- Additive public F# package/catalog/validation/canonicalization/cost surfaces in `SIR.Client`, consumed by the existing Web Samples route.
- The versioned fixture/catalog format, retained replay binding, browser labels/controls, performance receipt, documentation, and evidence are tool-facing contracts.

## Lifecycle Notes
- Tier 1 contracted change: package schema, public F# surface, production controls, fixtures, tests, docs, migrations, performance, and evidence change together.
- Existing deterministic spatial/combat/awareness packages are consumed as authority; this item composes scenarios rather than redefining their algorithms.
- Next lifecycle action: `fsgg-sdd clarify --work 184-scenario-catalog`.
