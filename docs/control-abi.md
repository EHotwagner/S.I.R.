---
title: S.I.R. Control ABI Surface
status: proposed
document-type: living-design
version: "0.1"
last-updated: 2026-07-27
related:
  - docs/wasm-control-architecture.md
  - docs/combat-resolution.md
  - docs/formations-and-referents.md
  - docs/casualty-and-medical-architecture.md
  - docs/logistics-architecture.md
---

# S.I.R. Control ABI Surface

## Purpose

Exactly two vocabularies cross the boundary between the authoritative
simulation and a player's control module: the **events** a unit is told about,
and the **actions** it may request. This document holds both.

It replaces an earlier doctrine vocabulary that also published a *condition*
vocabulary for server-side rule evaluation. Measurement removed the reason that
layer existed — see
[WASM Invocation Spike](research/wasm-invocation-spike.md) — and with the module
invoked every tick, it computes its own predicates. Nothing server-side
evaluates conditions, so no condition vocabulary is published.

## Shape of the contract

```text
events in   →   module decides   →   action requests out   →   server validates
```

The ABI itself is small and stable. Content lives in versioned ruleset data:

- **action request kinds** are few and fixed;
- **capability descriptors** give most requests their meaning, so weapons,
  medical actions, breaching, transfers, magic, and later faction mechanics are
  content rather than ABI surface; and
- **event kinds** are a published catalog.

Adding content extends the descriptors and the catalog. It does not change the
contract.

## Action request kinds

The complete set a module may emit. Everything content-specific funnels through
`StartCapability`.

| Kind | Purpose |
|---|---|
| `StartCapability` | Begin any declared capability with parameters and a target. Weapons, medical actions, breaching, transfers, interaction, and magic all use this. |
| `CancelAction` | Cancel the current action where its declared rules permit. |
| `SetMovementIntent` | Destination cell or referent, route preference, movement priority, and the speed-versus-readiness posture. |
| `SetFacing` | Body facing, one of the eight canonical directions. |
| `SetAttention` | Attention direction, independent of body facing. |
| `SetStance` | Stance transition; costs ticks under the action lifecycle. |
| `SetFormationIntent` | Adopt a station, request a template change, or release from formation. |
| `SetEngagement` | Declare a point target or an engaged area, or disengage. |
| `SendMessage` | Opaque player-defined payload to a permitted recipient. |
| `RequestService` | Commission an expensive host service; result arrives later. |
| `Sleep` | Request no invocation until a given tick. A courtesy, not a guarantee. |

Targeting for `StartCapability` and `SetEngagement` accepts a unit the module
legitimately knows about, a cell, an area, an edge, or a referent — the referent
form being what keeps control logic portable across maps.

## Event catalog

Events are authoritative facts delivered under the ordinary observation and
communication rules. A module cannot subscribe to, suppress, or fabricate them.

### Perception

| Event | Carries |
|---|---|
| `ContactGained` | A new local observation crossed its acquisition threshold. |
| `ContactUpdated` | Position, facing, action, or status changed on a known contact. |
| `ContactLost` | An existing contact is no longer observable. |
| `StimulusReceived` | Sound, muzzle flash, emission, thermal or magical signature, with direction and whatever the source supports. |

### Own state

| Event | Carries |
|---|---|
| `DamageTaken` | Amount, direction, damage type, and armour outcome. |
| `WoundSustained` | A discrete lasting condition and its effects. |
| `SuppressionChanged` | Crossing a threshold, in either direction. |
| `HealthStateChanged` | Incapacitation, stabilisation, deterioration, or death. |
| `StrainChanged` | For casters: strain, breach margin, and breach resolution. |

### Actions

| Event | Carries |
|---|---|
| `ActionCompleted` | Result and observable effects. |
| `ActionFailed` | A stable reason code. |
| `ActionInterrupted` | What interrupted it and at which lifecycle phase. |
| `EngagementSolutionLost` | The sustained-targeting requirement was broken, and why. |
| `ServiceResult` | An earlier `RequestService` completed. |

### Command and communication

| Event | Carries |
|---|---|
| `MessageReceived` | Opaque payload, sender, and delivery provenance. |
| `OrderReceived` | An authoritative order that reached this unit. |
| `CommandRoleChanged` | Succession, promotion, or reassignment. |
| `ConnectivityChanged` | Link to leader or headquarters gained, lost, or degraded. |
| `ReportDelivered` | A fixed authoritative report arrived. |

### World

| Event | Carries |
|---|---|
| `ReferentDesignated` | A role-tagged place was designated or superseded. |
| `ReferentInvalidated` | A referent is reported overrun, unreachable, or expired. |
| `ObjectiveStateChanged` | Objective progress this unit is entitled to know. |
| `TerrainChanged` | Nearby edge state or cell destruction the unit can observe. |
| `FormationStationChanged` | This unit's station or the formation's integrity changed. |

### Logistics

| Event | Carries |
|---|---|
| `InventoryChanged` | Consumption, transfer, pickup, or loss. |
| `TransferOutcome` | A transfer completed, failed, or was interrupted. |
| `SupplyObserved` | A supply source, cache, or carrier came into local knowledge. |

## Rules that bind both vocabularies

**Everything is knowledge-filtered.** An event carries only what the unit is
entitled to know, and a request is validated against what the unit could
legitimately have known when it was formed. Neither vocabulary may become a
channel for authoritative world truth.

**Reason codes are stable.** `ActionFailed` and its relatives drive module
behaviour, so their codes are a compatibility contract. Human-readable text is
diagnostic and may change freely.

**Events are facts, not invitations.** An event describes what happened. It does
not imply the server expects a particular response, and ignoring one is
legitimate.

**Both are versioned with the ruleset.** A match pins its event catalog and
descriptor set alongside its execution profile, so a replay reconstructs the
same contract.

## Open parameters

- Binary encoding and layout of the observation snapshot.
- Whether events are delivered as a flat log or grouped by category.
- Bounds on events per tick, and what happens when a unit exceeds them.
- Which events are always delivered and which are gated by observation richness.
- Whether a module may decline event kinds it does not use, and whether
  declining refunds bandwidth.
- The stable reason-code set for failed and rejected requests.
- Whether `SetEngagement` is distinct from `StartCapability` or a special case
  of it.
- Per-faction variation, given that capability contracts are already
  per-faction.
