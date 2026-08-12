---
schemaVersion: 1
workId: 181-physical-combat-slice
title: First physical combat, cover, armor, wound, and suppression slice
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

# First physical combat, cover, armor, wound, and suppression slice Charter

## Identity
Deliver S.I.R.'s first bounded end-to-end physical combat slice: an authoritative, deterministic attack lifecycle whose position, trace, cover, armor, health, wounds, incapacitation, suppression, collateral effects, and explanations agree across native, Fable, replay, and the production browser.

## Principles
- Combat consumes the inherited `SpatialQuery` authority and the executable rules corpus; it does not recreate geometry or rule meaning in clients, JavaScript, tests, or docs.
- Cover is derived per physical trace from cell volumes and semantic edges. It is never a persistent unit flag.
- Armor, HP, wounds/incapacitation, and suppression remain distinct typed state and ordered resolution phases.
- All authoritative arithmetic is bounded integer/fixed-point, all iteration is canonically ordered and capped, and the 20 Hz tick remains deterministic.
- Valid traces may affect every intervening entity and destructible cover without faction immunity; every decision is replay-safe and explainable.
- Public contracts, player-reachable browser behavior, package-only cross-runtime evidence, performance evidence, subject mutations, docs, and lifecycle evidence ship together.

## Scope Boundaries
- In: rifle point fire, support-weapon area engagement, anti-armor fire, one lobbed/area delivery, trace/collision, cover, directional armor/integrity/penetration, HP, wounds/incapacitation, suppression/recovery, destructible cover, friendly/civilian/intervening effects, and ordered emitted facts.
- In: integration with `SpatialQuery`, `CombatRules` identity/source correspondence, replay/timeline reconstruction, client projections, a player-reachable browser journey, representative-firefight and 100-unit performance workloads, and fail-capable gates.
- Out: complete equipment balance, campaign casualty recovery, magic, structural-collapse physics, final lethality tuning, new upstream Game.Core semantics, and client-side authority.

## Policy Pointers
- Honor constitution I-III by specifying first, declaring `.fsi` surfaces before implementations, and treating versioned contracts/fixtures as the machine truth.
- Honor constitution IV-V with idiomatic records/unions and pure Model-Update-Effect combat transitions behind explicit host/client edges.
- Honor constitution VI-VIII with real package/runtime/player evidence, subject mutations, explicit caps, and typed failure instead of fallback.
- Apply `.fsgg/sdd.yml`, `.fsgg/agents.yml`, `fs-gg-game-core-fable-lockstep-v1`, and inherited work items `180-authoritative-spatial-query-foundation` and `194-executable-rules-corpus`; Governance remains optional compatibility metadata.

## Lifecycle Notes
- Tier 1: this changes public F# combat state/events, rules/package identity, canonical replay bytes, production browser behavior, and performance contracts.
- No issue requirement is silently deferred. Any genuine cross-repository dependency remains on its producer board and is referenced through a typed blocker rather than copied locally.
- Next lifecycle action: `fsgg-sdd specify --work 181-physical-combat-slice`.
