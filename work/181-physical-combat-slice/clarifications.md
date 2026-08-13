---
schemaVersion: 1
workId: 181-physical-combat-slice
title: Physical Combat Slice
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/181-physical-combat-slice/spec.md
publicOrToolFacingImpact: true
---

# Physical Combat Slice Clarifications

## Source Specification
- work/181-physical-combat-slice/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001] blocking answered: Which public combat types and state transitions keep armor, HP, wounds/incapacity, and suppression separate?
- CQ-002 [AMB:AMB-002] blocking answered: How do delivery profiles consume the inherited spatial authority and order collision, cover, and environment effects?
- CQ-003 [AMB:AMB-003] blocking answered: Which first-cut integer/fixed-point profile values, armor arcs, consequence thresholds, and suppression rules are canonical?
- CQ-004 [AMB:AMB-004] blocking answered: How are rules metadata/package identity and replay identity rebound to the implementation?
- CQ-005 [AMB:AMB-005] blocking answered: Which real browser route makes the combat state and explanation player-visible?
- CQ-006 [AMB:AMB-006] blocking answered: What performance workload, caps, and mutation matrix qualify combat v1?

## Answers
- CQ-001: `SIR.Simulation` owns public discriminated unions/records for weapon profile, damage type, armor, wound, suppression, cover object, attack request, ordered combat facts, and result. `UnitState` stores HP, armor, wounds, incapacity, and suppression as separate fields; pure phase functions return new state plus facts. Client layers receive projections only.
- CQ-002: Direct delivery submits `ProjectileTrace` and `Cover` requests against a projected `SpatialQuery` world. Its crossed cells/edges and cover contributors are the only geometry evidence. Occupants and cover volumes are joined onto that ordered evidence, then sorted by trace index, stable entity kind, and id. Area delivery uses a bounded canonical cell enumeration around its spatially validated impact cell; lobbed delivery does not claim a clear direct-fire trace. Cover remains a per-recipient directional calculation, and destroyed cover changes the next projected world revision/permeability.
- CQ-003: Schema v1 profiles are Rifle (damage 25, penetration 20, suppression 10, point), Support (damage 15, penetration 10, suppression 25, radius 1), AntiArmor (damage 50, penetration 70, suppression 12, point), and Lobbed (damage 30, penetration 25, suppression 30, radius 2). Values are bounded integers converted to fixed point only in rules evaluation. Front armor rating is 50, rear/flank 20, integrity scales rating; penetration at or above effective rating retains full damage, otherwise the bounded penetration/rating ratio retains damage. A hit of 25+ adds a wound; HP zero incapacitates. Suppression saturates at 100, applies effectiveness/timing bands at 25/50/75, and recovers five points per committed tick after attack consequences.
- CQ-004: Extend the canonical registry with trace/collision, cover, penetration, HP consequence, wound/incapacity, suppression, recovery, collateral ordering, and cover-destruction rules. `RuleSource.Commit`, package `SourceCommit`, and implementation artifacts bind the exact candidate source commit; semantic changes update semantic/manifest identities while documentation-only metadata preserves the semantic digest. Replay carries the complete rules identity plus combat/spatial schema/profile identities and either resolves retained exact packages or returns a typed unavailable result.
- CQ-005: Boot the production client at its real entry, start the canonical combat scenario using ordinary UI controls, select each profile, issue attacks, and open the combat explanation panel. The scene and panel display trace cells, first/intervening contacts, cover/armor decisions, HP, wounds/incapacity, suppression, cover integrity, ordered facts, and pinned source/package identity. No direct Elmish message injection or test-only endpoint counts.
- CQ-006: The Release workload runs a deterministic representative matrix containing all canonical scenarios and a 100-unit/50-attack stress tick after warm-up. Caps are 256 trace cells, 256 area cells, 256 recipients, 4,096 ordered facts, and 64 KiB per canonical explanation. The environment-qualified observation target is 20 ms for the representative matrix and 50 ms for the 100-unit stress tick, matching the authoritative tick ceiling rather than inventing a live-compositor claim. Mutations individually bypass collision, cover, armor, suppression, consequence ordering, identity, and native/Fable equality, plus malformed-evidence refusal.

## Decisions
- DEC-001: [AMB:AMB-001] [FR-001] [FR-005] [FR-006] complete. Use separate typed combat state plus pure ordered phase transitions in `SIR.Simulation`; clients consume projections and never own outcomes.
- DEC-002: [AMB:AMB-002] [FR-002] [FR-008] complete. Consume `SpatialQuery` projectile/cover evidence, join occupants/destructibles in canonical trace order, and enumerate bounded area cells canonically; do not recreate LOS/edge algorithms or store cover state.
- DEC-003: [AMB:AMB-003] [FR-003] [FR-004] [FR-007] complete. Adopt the four fixed schema-v1 profiles, directional integrity-scaled armor/penetration, 25-damage wound and zero-HP incapacity thresholds, suppression bands, and five-point tick recovery described above.
- DEC-004: [AMB:AMB-004] [FR-009] [FR-010] [FR-014] complete. Expand and canonically rebind the executable rules registry/current-source package, and bind replay to complete combat, spatial, and rules identities with typed historical unavailability.
- DEC-005: [AMB:AMB-005] [FR-011] complete. Qualify the real entry → start scenario → choose profile → issue attack → open explanation player journey and show every required renderer-neutral outcome.
- DEC-006: [AMB:AMB-006] [FR-012] [FR-013] complete. Bind the representative matrix, 100-unit/50-attack stress tick, structural/latency caps, exact Release artifact, and subject-mutation/unreadable-input matrix above.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
None. AMB-001 through AMB-006 are resolved by DEC-001 through DEC-006.

## Lifecycle Notes
- No accepted deferrals; every issue acceptance obligation remains owned by this work item.
- Next lifecycle action: `fsgg-sdd checklist --work 181-physical-combat-slice`.
