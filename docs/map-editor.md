---
title: Map Editor Reference
category: Tools & Evidence
categoryindex: 5
index: 2
status: accepted
decision-status: implemented
document-type: reference
version: "1.0"
last-updated: 2026-07-29
description: Map records, terrain, semantic edges, square unit footprints, controller modes, and deterministic execution.
related:
  - docs/interactive-rules-lab.md
  - docs/svg-replay-player.md
  - docs/fable-client-and-documentation.md
---

# Map Editor Reference

The browser editor creates deterministic sandbox maps. It supports terrain,
semantic edges, square units, manual control, repeatable scripts, and a bundled
general controller.

Open the [simulation workspace](interactive-rules-lab.md) to use it.

## Map

| Field | Constraint |
|---|---|
| Width and height | 4–40 cells |
| Coordinates | Zero-based integers |
| Terrain | Open, rough, blocked, or objective |
| Edges | East or south edge of a cell |
| Edge kinds | Wall, door, or window |
| Units | Positive identifier, square footprint, current and maximum HP, controller |

Blocked terrain cannot contain a unit. Unit footprints cannot overlap or extend
outside the map. A selected unit exposes editable side, class identifier,
square size, HP, controller, and script fields.

## Square unit geometry

Every unit base is square. `size` defines both dimensions:

```fsharp
footprintWidth = size
footprintDepth = size
```

The SVG symbol uses the same square bounds. A size-1 unit occupies 1×1 cells; a
size-2 unit occupies 2×2 cells. Movement checks every crossed edge along the
complete leading side of that square.

## Semantic edges

Edges are stored once:

```text
edge <column> <row> <east|south> <wall|door|window> <open|closed>
```

Wall and window edges block movement. Closed doors block movement; open doors
do not. Selecting the same door cycles `closed → open → removed`.

## Controllers

### Manual

A manual unit changes state only after an explicit direction:

```fsharp
nextPosition = currentPosition + directionDelta
```

The move is rejected if the destination is outside the map, blocked terrain,
occupied, or separated by a blocking edge.

### Scripted AI

A script is a comma-separated sequence from:

```text
N, NE, E, SE, S, SW, W, NW
```

On each automatic tick:

```fsharp
direction = script[scriptIndex % scriptLength]
scriptIndex = scriptIndex + 1
```

The index advances even when movement is blocked. This keeps execution
deterministic and prevents an obstruction from changing the subsequent script
phase.

### General AI

The current reference controller is deterministic:

1. Select the nearest hostile by Chebyshev distance between square footprints.
2. Break equal-distance ties by unit identifier.
3. Attack for one damage when adjacent.
4. Otherwise move one step toward the target.
5. Hold if no valid move or hostile exists.

This is a bundled test policy. It is not an external service, a language model,
or the final player-AI contract.

## Tick order

Automatic execution visits living units in ascending identifier order. Each
unit observes changes committed by earlier units in the same tick.

```fsharp
for unitId in livingUnitIds |> List.sort do
    world <- executeController unitId world
```

The editor increments the tick once after all eligible units execute.

## File format

The export format is line-oriented UTF-8:

```text
SIR-MAP 1
size 12 8
terrain 5 3 objective
edge 6 2 south door closed
unit 1 blue rifleman 1 1 2 12 12 manual -
unit 2 blue medic 1 5 2 12 12 scripted E,E,N
unit 3 red goblin 9 1 1 12 12 general -
```

Exports sort terrain, edges, and units. Import validates the version, bounds,
identifiers, health, controller names, scripts, footprints, terrain, and edge
records before replacing the current map.

## Authority

The editor is a browser sandbox. It projects map state into the shared SVG
battlefield but does not host an authoritative match, execute player WASM, or
grant replay verification. Exported maps are design inputs, not accepted match
state.
