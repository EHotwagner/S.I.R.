---
schemaVersion: 1
workId: 181-physical-combat-slice
title: Physical Combat Slice
stage: specify
changeTier: tier1
status: specified
publicOrToolFacingImpact: true
---

# Physical Combat Slice Specification

Prose status: specified

## User Value
Players can use position, weapon delivery, cover, armor, wounds, and suppression in one visible, deterministic, explainable physical combat loop where collateral effects and battlefield damage matter.

## Scope
- SB-001: Add a versioned ordered attack lifecycle covering eligibility, commitment, physical delivery, intervening collision, cover, armor, HP damage, wounds/incapacitation, suppression/recovery, environment changes, and emitted facts.
- SB-002: Ship four representative typed profiles: rifle point fire, support-weapon area engagement, anti-armor direct fire, and lobbed area delivery.
- SB-003: Integrate the inherited authoritative spatial-query layer and executable rules corpus with canonical replay, client projections, production-browser controls, and documentation.
- SB-004: Qualify representative firefights and 100-unit stress through bounded native/Fable/browser workloads and protected subject mutations.

## Non-Goals
- SB-005: Do not implement complete weapon/equipment balance, campaign casualty recovery, magic, structural-collapse physics, or final lethality tuning.
- SB-006: Do not add a stored `inCover` unit Boolean, client-side geometry/combat authority, copied Game.Core algorithms, floating authoritative arithmetic, faction immunity, or an unbounded trace/area search.
- SB-007: Do not broaden the upstream `FS.GG.Game.Core` compatibility profile or reinterpret historical replay through the current rules package.

## User Stories
- US-001 (P1): As a player, I can choose representative weapons and see physical traces interact with intervening units, civilians, friendlies, cover, armor, and the environment without implicit immunity.
- US-002 (P1): As a tactical player, I can use direction and destructible cover to reduce or redirect harm, while suppression changes timing/effectiveness without pretending to be HP damage.
- US-003 (P1): As a player, I can inspect the trace, cover, armor, damage, wound/incapacity, suppression, and environment decisions that produced an outcome.
- US-004 (P1): As a replay consumer, I can seek and reconstruct a fight using its exact spatial/rules identities and receive identical traces, state, and events.
- US-005 (P1): As an operator, I can run representative and 100-unit combat workloads within declared structural and latency budgets at deterministic 20 Hz ordering.

## Acceptance Scenarios
- AC-001 [US-001] [US-002] [FR-001] [FR-002]: Given open, partial-cover, full-cover, intervening-object/unit, and friendly-fire lanes, when rifle or support fire resolves, then the authoritative spatial trace orders every collision and derives cover per trace without stored unit cover state or faction immunity.
- AC-002 [US-001] [US-002] [FR-003] [FR-004]: Given front/rear armor, intact/degraded integrity, penetration/no-penetration, and anti-armor fire, when contact resolves, then armor runs after contact and cover but before HP with typed inspectable effective parameters.
- AC-003 [US-002] [FR-005] [FR-006]: Given damaging and suppressive effects, when thresholds and recovery ticks resolve, then HP, armor integrity, wounds, incapacitation, and suppression remain distinct and yield deterministic capability/timing changes.
- AC-004 [US-001] [FR-007] [FR-008]: Given point, support-area, anti-armor, and lobbed-area profiles around units and destructible cover, when delivery resolves, then bounded ordered recipients include valid enemies, friendlies, civilians, cover, and environment and cover destruction changes later spatial outcomes.
- AC-005 [US-003] [FR-009]: Given any accepted or rejected attack, when its result is projected, then versioned ordered facts record effective inputs, spatial evidence, each decision, applied consequences, source symbols, and rules/package identity sufficient to explain why.
- AC-006 [US-004] [FR-010]: Given a canonical fight package and timeline seeks, when native and Fable replay reconstruct it, then exact retained spatial/rules identities reproduce identical state, trace, fact, and event bytes; unavailable historical identity fails explicitly.
- AC-007 [US-003] [FR-011]: Given the production browser entry and player-emittable controls, when a fight is booted and an attack is issued, then visible trace, cover/armor outcome, HP/wound/suppression changes, cover damage, and ordered explanation are reachable without test-only messages.
- AC-008 [US-005] [FR-012]: Given representative firefights and deterministic 100-unit stress, when the Release workload runs, then declared structural caps and environment-qualified latency budgets pass without inventing an unmeasured target.
- AC-009 [US-005] [FR-013]: Given subject mutations bypassing trace collision, cover, armor, suppression, consequence ordering, rules identity, replay identity, or runtime equality, when their owning gates run, then each gate reds on its mutated subject and rejects unreadable evidence.
- AC-010 [US-004] [US-005] [FR-014]: Given the exact candidate, when lifecycle and release qualification run, then public signatures, manifests/fixtures, docs, feedback, SDD analysis/evidence/verify/ship, full native/Fable/browser/docs conformance, and hosted CI are current and passing.

## Functional Requirements
- FR-001: The system MUST declare a versioned typed attack request/result and canonical phase order for eligibility, preparation/commitment, physical delivery, intervening collision, cover, armor, HP damage, wound/incapacitation, suppression, recovery, environment changes, and emitted facts. (Stories: US-001, US-003; Acceptance: AC-001, AC-005)
- FR-002: Every direct trace MUST consume `SIR.Simulation.SpatialQuery` projectile/cover authority over cell volumes and semantic edges, order intervening collisions canonically, derive cover per trace/direction, and apply no stored cover Boolean or faction immunity. (Stories: US-001, US-002; Acceptance: AC-001)
- FR-003: Armor MUST resolve only after physical contact and cover, using typed coverage arc, facing, integrity, damage type, and penetration to produce an inspectable retained effect before HP changes. (Stories: US-001, US-002; Acceptance: AC-002)
- FR-004: All combat parameters and arithmetic MUST use bounded integer/fixed-point values with explicit units, versions, saturation/rounding rules, canonical ordering, and no authoritative floating-point or ambient randomness. (Stories: US-002, US-005; Acceptance: AC-002, AC-008)
- FR-005: Unit state MUST keep HP, armor integrity, wounds, incapacity, and suppression distinct; wounds/incapacity follow declared HP/consequence thresholds while suppression never directly masquerades as HP damage. (Stories: US-002; Acceptance: AC-003)
- FR-006: Suppression MUST accumulate from declared physical/area effects, change capability timing/effectiveness through a bounded typed function, and recover deterministically in tick order without erasing wounds or HP. (Stories: US-002; Acceptance: AC-003)
- FR-007: The initial registry MUST execute rifle point fire, support-weapon area engagement, anti-armor direct fire, and lobbed/area delivery through one bounded lifecycle with profile-specific range, area, penetration, damage, suppression, and delivery semantics. (Stories: US-001; Acceptance: AC-004)
- FR-008: Valid bounded deliveries MUST consider intervening units, civilians, friendlies, cover, and environment in canonical distance/position/id order; destructible cover integrity reaching zero MUST update later spatial permeability through the inherited authority. (Stories: US-001; Acceptance: AC-004)
- FR-009: Every result and rejection MUST expose renderer-neutral versioned effective parameters and ordered decisions for trace, collision, cover, armor, damage, wounds/incapacity, suppression, and environment effects, bound to source symbols and the executable rules package identity. (Stories: US-003; Acceptance: AC-005)
- FR-010: Replay and timeline seeking MUST bind the exact combat schema, spatial identity/profile/package, rules engine/profile/package/source/implementation/semantic/manifest identities, ordered inputs, facts, and effects; historical packages resolve exactly or return a typed unavailable result. (Stories: US-004; Acceptance: AC-006)
- FR-011: Client and browser code MUST project authoritative combat results without recomputing geometry or outcomes, and the real product entry plus player-emittable controls MUST visibly demonstrate all four profiles and required state/explanation changes. (Stories: US-003; Acceptance: AC-007)
- FR-012: Before implementation acceptance, the performance contract MUST declare and Release-measure representative firefights plus a deterministic 100-unit stress route, with trace/area/recipient/explanation structural caps and environment-qualified latency observations against the 50 ms tick ceiling. (Stories: US-005; Acceptance: AC-008)
- FR-013: Each added or modified combat gate MUST retain a subject mutation proving red for ignored collision, cover, armor, suppression, consequence ordering, identity binding, replay divergence, and native/Fable/browser divergence, and MUST fail closed for malformed/unreadable inputs. (Stories: US-005; Acceptance: AC-009)
- FR-014: The change MUST ship declared public `.fsi` surfaces, canonically rebound `CombatRules` metadata/package/current-source correspondence, fixtures/manifests, docs, schema-v2 feedback, SDD readiness, clean package-only native/Fable/browser evidence, production delivery, full conformance, and exact-head CI. (Stories: US-004, US-005; Acceptance: AC-010)

## Ambiguities
- AMB-001: What exact typed combat state/request/result/fact surface extends the minimal simulation without conflating armor, HP, wounds, incapacity, and suppression?
- AMB-002: How do direct and area deliveries consume inherited `SpatialQuery` evidence and order collisions/cover/environment effects without creating another geometry authority?
- AMB-003: Which deterministic profile values, directional armor/penetration rules, consequence thresholds, suppression/recovery functions, and collateral ordering define combat v1?
- AMB-004: How are the expanded executable rules registry, current source identity, canonical replay identity, and historical package behavior rebound after implementation?
- AMB-005: Which real browser journey and renderer-neutral projection make every required combat outcome player-visible without test-only injection?
- AMB-006: Which representative/100-unit workload definitions, structural caps, latency observations, and protected subject mutations qualify the exact Release candidate?

## Public Or Tool-Facing Impact
- Tier 1: expanded public F# combat state/request/result/event/fact APIs, canonical state/event/replay bytes, executable rule registry/package identity, fixtures, client projection, production browser workflow, docs, and CI gates.
- `SpatialQuery` and Game.Core remain inherited authorities. This item consumes their published surfaces and records exact identity; it does not broaden either contract.

## Lifecycle Notes
- Resolve all six ambiguities before plan; no issue-level acceptance obligation is an implicit deferral.
- The performance-first gate binds the plan before product implementation and must name the real production route, scale, caps, counters, host facts, and fallback behavior.
- Next lifecycle action: `fsgg-sdd clarify --work 181-physical-combat-slice`.
