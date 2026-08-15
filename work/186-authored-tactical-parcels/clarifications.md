---
schemaVersion: 1
workId: 186-authored-tactical-parcels
title: Authored Tactical Parcels
stage: clarify
changeTier: tier1
status: needsAnswers
sourceSpec: work/186-authored-tactical-parcels/spec.md
publicOrToolFacingImpact: true
---

# Authored Tactical Parcels Clarifications

## Source Specification
- work/186-authored-tactical-parcels/spec.md

## Clarification Questions
- CQ-001 [AMB:AMB-001]: Which transform, ordering, seed-consumption, and hash rules define one canonical assembly?
- CQ-002 [AMB:AMB-002]: Which feature states and modality defaults are legal?
- CQ-003 [AMB:AMB-003]: How do bounded destruction and local invalidation compose?
- CQ-004 [AMB:AMB-004]: Which production editor journey proves accessible author-to-play behavior?
- CQ-005 [AMB:AMB-005]: Which workloads, budgets, equality proofs, and mutations qualify release?

## Answers
- CQ-001 → sort slots and compatible variants by stable ids; derive one addressed SHA-256 draw from the seed and stable slot id per non-empty compatible choice; permit identity and quarter-turn transforms only; hash schema-versioned canonical UTF-8 bytes containing the plot, selected variant ids/transforms, and assembled environment.
- CQ-002 → each feature type owns a closed legal-state set; each state carries seven independent modality decisions, with interaction capabilities named separately rather than inferred from movement.
- CQ-003 → an action validates target/capability/cost, changes one declared feature at most, increments revision once only when changed, returns stable dependency keys, and invalidates cache entries whose declared dependency set intersects those keys; no neighbour propagation occurs.
- CQ-004 → boot the production editor, select parcel mode, load a fixture, change feature/state/permeability with keyboard and pointer controls, undo/redo, export/import, preview/assemble, enter play, interact/breach, and replay the captured event sequence to identical canonical bytes.
- CQ-005 → qualify 64-slot/32-variant assembly, full catalog validation, preview projection, one-key local invalidation amid at least 256 cached dependencies, and 100-unit representative combat queries; enforce declared structural caps and environment-qualified Release observations, plus subject mutations for state, hash, locality, and destruction bounds.

## Decisions
- DEC-001 [CQ-001] [AMB:AMB-001] [FR-001] [FR-002] [AC-001]: Canonical assembly uses stable ordinal ids, identity/quarter-turn transforms, one product-owned addressed SHA-256 draw from seed plus slot id per compatible slot choice, and SHA-256 over canonical schema-v1 bytes; content identity mismatch is a typed refusal. This avoids the published Game.Core Fable profile's explicitly DotNetOnly sequential RNG while preserving one stable draw per slot.
- DEC-002 [CQ-002] [AMB:AMB-002] [FR-004] [FR-005] [AC-003]: Door states are Closed/Open/Damaged/Breached/Destroyed, window states Closed/Open/Damaged/Breached/Destroyed, wall states Intact/Damaged/Breached/Destroyed, and cover states Intact/Damaged/Destroyed; legal transitions and seven modality values are explicit data.
- DEC-003 [CQ-003] [AMB:AMB-003] [FR-006] [FR-007] [AC-004]: Destruction changes only the targeted feature, uses non-negative bounded integer damage/cost, advances revision once on an effective transition, and emits sorted dependency keys used by intersection-based cache invalidation.
- DEC-004 [CQ-004] [AMB:AMB-004] [FR-008] [FR-009] [AC-005]: The acceptance journey uses the real Web composition and MapEditor reducer with both pointer and keyboard actions; history snapshots canonical editor state, migration is explicit, and replay compares canonical imported/assembled/play state.
- DEC-005 [CQ-005] [AMB:AMB-005] [FR-010] [FR-012] [AC-006] [AC-008]: Exterior and interior/breach fixtures are the representative content; Release workloads cap slots at 64, variants at 32 per role, findings at 512, actions at one target/no propagation, and dependency inspection at declared entries only; timing observations are host-qualified and never compositor claims.
- DEC-006 [FR-011] [AC-007]: Capability descriptors expose schema, feature id/type, known state, available actions, costs, and observation revision only after requester-knowledge filtering; hidden values and absent capabilities serialize identically for indistinguishable knowledge.

## Accepted Deferrals
No accepted deferrals recorded.

## Remaining Ambiguity
- None. AMB-001 through AMB-005 are resolved by DEC-001 through DEC-005.

## Lifecycle Notes
- No accepted deferrals; DEC-001 through DEC-006 are implementation obligations.
- Next lifecycle action: `fsgg-sdd checklist --work 186-authored-tactical-parcels`.
