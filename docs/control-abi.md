---
title: Control ABI
category: Engineering
categoryindex: 6
index: 13
status: proposed
document-type: living-design
version: "0.4"
last-updated: 2026-07-29
related:
  - docs/wasm-control-architecture.md
  - docs/combat-resolution.md
  - docs/formations-and-referents.md
  - docs/casualty-and-medical-architecture.md
  - docs/logistics-architecture.md
---

# Control ABI Surface

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
instance configuration (fixed at lock, opaque to the server)
        │
        ▼
events in   →   module decides   →   action requests out   →   server validates
```

An instance reads its own configuration, which is per-unit data supplied with
the artifact assignment and is what lets one shared artifact serve many
differently-directed units. See
[WebAssembly Control Architecture](wasm-control-architecture.md).

The ABI itself is small and stable. Content lives in versioned ruleset data:

- **action request kinds** are few and fixed;
- **capability descriptors** give most requests their meaning, so weapons,
  medical actions, breaching, transfers, magic, and later faction mechanics are
  content rather than ABI surface; and
- **event kinds** are a published catalog.

Adding content extends the descriptors and the catalog. It does not change the
contract.

## Control ABI v1 encoding

Control ABI v1 is frozen as a bulk-memory, canonical little-endian contract.
The host performs one copy to module input memory and one read from module
output memory. Modules export:

```text
memory
sir_abi_version() -> i32       // 0x0001_0000 for v1.0
sir_input_ptr() -> i32
sir_input_capacity() -> i32
sir_output_ptr() -> i32
sir_output_capacity() -> i32
sir_decide(input_length: i32) -> i32
```

There are no gameplay imports. `sir_decide` returns a non-negative output byte
length or a negative stable module status. Input and output memory ranges must
not overlap during an invocation.

A module that consumes immutable per-instance configuration exports both of
these additional buffer functions:

```text
sir_configuration_ptr() -> i32
sir_configuration_capacity() -> i32
```

The host copies the bounded opaque configuration once during instantiation.
The configuration range may not overlap either invocation buffer, and its
capacity remains subject to the execution profile's 4 KiB opaque-payload
limit. Modules that do not consume configuration may omit both exports; they
must never export only one. This is an explicit lifecycle surface, not a
gameplay host import, and it grants no ambient capability.

The generated-code source of truth is
[`control-abi-v1.json`](control-abi-v1.json). Running
`node scripts/generate-control-abi.mjs` publishes the F# constants in
`SIR.ControlAbi.V1Constants` and the standalone ES-module binding in
`generated/control-abi-v1.mjs`.

### Envelope and bounds

Input magic is `SIRI`; output magic is `SIRO`. Both use this 32-byte header:

| Offset | Width | Field |
|---:|---:|---|
| 0 | 4 | ASCII magic |
| 4 | 1 | ABI major (`1`) |
| 5 | 1 | ABI minor (`0`) |
| 6 | 2 | header length (`32`) |
| 8 | 4 | total byte length, including header |
| 12 | 4 | non-negative tick |
| 16 | 4 | non-negative unit ID |
| 20 | 4 | invocation flags |
| 24 | 4 | budget summary |
| 28 | 2 | section count |
| 30 | 2 | reserved zero |

Every section begins with a 12-byte header:

| Offset | Width | Field |
|---:|---:|---|
| 0 | 2 | section tag |
| 2 | 2 | flags; bit 0 means required, all other bits are zero |
| 4 | 4 | payload byte length |
| 8 | 2 | element count |
| 10 | 2 | reserved zero |

Sections are unique and strictly ascending by tag. An encoder sorts them; a
decoder rejects a non-canonical order or duplicate. Unknown optional sections
are retained or skipped, while unknown required sections reject the complete
invocation. The assigned tags are:

| Tag | Section |
|---|---|
| `0x0001` | own state |
| `0x0002` | resolved orientation |
| `0x0003` | action and recovery |
| `0x0004` | observations and stimuli |
| `0x0005` | events |
| `0x0006` | messages and reports |
| `0x0007` | capability descriptors |
| `0x0008` | service results |
| `0x0009` | request status |
| `0x1001` | output requests |

The v1.0 execution-profile limits are 65,536 input bytes, 16,384 output bytes,
32 sections, 256 elements per section, 255 bytes per canonical UTF-8 string,
and 4,096 bytes per opaque payload. Lengths and counts are checked before
allocation. Integers are unsigned unless the field explicitly says otherwise;
ticks and unit IDs must fit a non-negative signed 32-bit value.

Each output request has a 12-byte header followed by its request-specific
payload: request kind `u16`, reserved zero `u16`, module-local request ID `u32`,
and payload length `u32`. Requests are unique and strictly ascending by
module-local ID. Their kind codes are `SetMovementIntent=1`, `SetFacing=2`,
`SetAttention=3`, `SetStance=4`, `SetEngagement=5`,
`StartCapability=6`, `CancelAction=7`, `SendMessage=8`,
`RequestService=9`, `SetEmissionPolicy=10`, `SetFormationIntent=11`, and
`Sleep=12`.

The registry also freezes direction codes `0..7` clockwise from north, event
codes `1..28` in the catalog order below, target kinds, movement postures,
emission policies, formation and engagement operations, and the v1
`PathSearch` asynchronous service. Consumers must use the generated binding
rather than reproduce these integers.

### Stable failures

Negative module statuses are `MalformedInput=-1`, `UnsupportedVersion=-2`,
`InputTooLarge=-3`, `OutputCapacityInsufficient=-4`,
`ConfigurationInvalid=-5`, and `InternalFault=-6`.

Host invocation failures use positive codes: module rejection `1`, trap `2`,
fuel exhaustion `3`, invalid output length `4`, malformed output `5`, forbidden
request `6`, memory limit `7`, and host-service limit `8`. Request rejection
and action-lifecycle failure codes are separately assigned in the generated
registry. A host records only the integer compatibility code in authoritative
state; accompanying diagnostic text is not stable.

Any malformed header, section, request, string, length, count, reserved field,
or unknown required value rejects the entire invocation atomically.

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
| `SetEmissionPolicy` | How freely the unit may transmit: free, restricted, or silent. The only defence against being located, and therefore something a module must be able to exercise continuously. |
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
| `InterferenceDetected` | The unit's receiver is being jammed, as distinct from nothing being sent. Whether and how reliably this is distinguishable is an open parameter of the electronic-warfare model. |
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

- Which events are always delivered and which are gated by observation richness.
- Whether a module may decline event kinds it does not use, and whether
  declining refunds bandwidth.
- Whether `SetEngagement` is distinct from `StartCapability` or a special case
  of it.
- Whether emission policy is a unit setting, a per-message decision, or both.
- Per-faction variation, given that capability contracts are already
  per-faction.
