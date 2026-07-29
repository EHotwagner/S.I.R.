---
title: Logistics
category: Battlefield Systems
categoryindex: 4
index: 9
status: proposed
document-type: living-design
version: "0.1"
last-updated: 2026-07-25
related:
  - docs/game-vision.md
  - docs/wasm-control-architecture.md
  - docs/combat-resolution.md
  - docs/setting-and-factions.md
---

# Logistics Architecture

## Purpose

This document defines how resources move from persistent campaign ownership to
mission commitment, physical battlefield use, disruption, recovery, and
campaign write-back.

The governing principle is:

> Physical last mile, abstract upstream.

Routine accounting and transfer execution should be automatable. Physical
location, capacity, access, timing, communications, loss, and interdiction
remain tactically consequential.

## Three logistics layers

```text
campaign stockpile
  → mission manifest
  → battlefield inventories and supply network
```

### Campaign stockpile

Between missions, the mercenary company can own aggregated stocks such as:

- ammunition classes;
- fuel;
- energy;
- medical supplies;
- food and general sustainment;
- spare parts;
- specialized equipment;
- recovered portal resources; and
- faction-specific magical resources where applicable.

The campaign layer supports procurement, allocation, reserves, maintenance, and
routine automated restocking. It does not require every interchangeable package
to have an individual simulated warehouse position.

Replacement personnel are not fungible stock. They remain persistent
individuals with their own identity and progression.

### Mission manifest

Before deployment, the player or authorized automation commits resources to a
mission and assigns them to authoritative holders or containers such as:

- individual units;
- squad carriers;
- vehicles;
- supply crates;
- medical elements;
- forward caches; and
- headquarters or support assets.

When the live match starts, that manifest becomes battlefield state. Resources
cannot return to the campaign stockpile, move between holders, or appear at a
new location without an applicable authoritative event or action.

### Battlefield logistics

Every tactically relevant resource has sufficient state to determine:

- type and compatibility;
- quantity;
- holder or grid location;
- load or capacity cost;
- ownership;
- operational condition;
- reservation or transfer state;
- locally available knowledge and provenance; and
- recovery or extraction status.

## Dynamic supply network

Battlefield logistics form a dynamic physical network:

```text
source
  → carrier or vehicle
  → forward cache
  → squad
  → individual unit
```

A link exists because a valid carrier, route, access relationship, and transfer
action can connect its endpoints. Proximity to an abstract aura is not
sufficient.

A transfer requires:

- legitimate knowledge of the source;
- compatible source, resource, and recipient;
- physical access or an explicitly defined transfer range;
- available quantity;
- recipient capacity;
- an authoritative reservation;
- a timed transfer action; and
- validation when the action resolves.

The general transaction lifecycle is:

```text
available
  → reserved
  → in transfer
  → delivered
  → consumed
```

Interruption follows the transfer capability's declared commitment rules. It
can release the reservation, leave stock at the source, drop a physical
container, lose part of the resource, or produce another explicit result.

Reservations prevent several units from independently planning around the same
scarce package. They do not create knowledge or communication between units
that could not otherwise coordinate.

## Resource granularity

Resources are consumed at the precision required by gameplay and transferred in
meaningful logistical packages.

### Ammunition

- Weapons consume individual shots or declared burst amounts.
- Reloads use compatible magazines, belts, batteries, rockets, charges, or
  other weapon-specific packages.
- Inventories can group interchangeable packages.
- Individual loose cartridges are not a required player-management unit.

This supports reload timing, compatibility, partial depletion, recovery, and
scavenging without turning routine play into manual cartridge handling.

### Other resources

- **Fuel and energy:** measured quantities or standardized compatible cells.
- **Medical supplies:** treatment charges, kits, or procedure-specific
  packages.
- **Spare parts:** compatible repair packages.
- **Food and sustainment:** primarily campaign or extended-operation resources,
  not continuous hunger simulation during a normal 20-minute match.
- **Magical resources:** faction-specific components, catalysts, charges, or
  infrastructure with explicit transfer and consumption rules.

Exact package sizes and compatibility taxonomies remain prototype data.

## Capacity and access

The load model is bounded and tactically meaningful rather than a detailed
packing simulation. Holders can define:

- equipment slots;
- cargo capacity;
- item bulk or load cost;
- compatibility restrictions; and
- accessible versus packed storage.

Additional ammunition therefore competes with medicine, sensors, explosives,
communications equipment, or other supplies. Vehicles and specialist carriers
provide greater capacity while creating valuable, vulnerable assets.

Access state can affect transfer and use time. An immediately accessible
magazine is different from a sealed package inside a vehicle, without requiring
continuous three-dimensional packing geometry.

## Automation and intent

The human player primarily sets logistical intent and policy. Examples include:

- maintain a declared ammunition reserve;
- prioritize anti-armor ammunition for suitable targets;
- reserve medical resources for stabilization;
- limit how much risk a carrier accepts;
- establish or abandon a forward cache;
- recover valuable equipment only below a risk threshold; and
- prioritize evacuation over ordinary supply recovery.

WASM control modules and the standard implementation handle routine
reservations, routing, carrying, transfer, redistribution, and resupply actions.
The server validates all resource state and transactions.

Automation must remain inspectable. A client should be able to explain which
policy, demand, reservation, route, or failure caused a logistical decision
without exposing hidden state.

## Communications and knowledge

Logistical truth and logistical knowledge are distinct.

A unit can know:

- its own inventory;
- supplies it currently observes;
- reservations, requests, and stock reports legitimately delivered to it; and
- locally completed or failed transfers.

Headquarters can believe a cache or carrier still holds supplies after it has
been depleted, destroyed, captured, or displaced if no valid report has
returned. A disconnected squad can redistribute locally known supplies through
its own control modules but cannot request remote support over an unavailable
communication path.

Pathfinding, reservation, stock-query, and diagnostic APIs cannot reveal hidden
supplies, attackers, routes, destruction, or ownership changes through results,
failure details, timing, or cache metadata.

## Tactical disruption

The physical network enables:

- destruction or capture of supply vehicles;
- suppression of transfer points;
- interdiction of carriers and routes;
- isolation of forward forces;
- recovery of supplies dropped by casualties;
- loss of demand and stock reports through jamming;
- deception entering through legitimate communication mechanics;
- terrain destruction or portal effects invalidating safe routes; and
- withdrawal choices between speed, casualties, equipment, and supply
  recovery.

Loss of communication does not destroy physical supply. Loss of physical supply
does not automatically inform headquarters.

## Mission resolution and write-back

At mission end, every committed or acquired resource receives an explicit
outcome such as:

- consumed;
- extracted;
- secured in a qualifying controlled area;
- abandoned;
- destroyed;
- captured; or
- recovered from another faction.

Only qualifying results return value to campaign state. Survival or mission
completion does not automatically recover every deployed item or container.

Mission types can define different securing, extraction, salvage, ownership,
and capture rules. Those rules are public mode data and use authoritative
battlefield state.

## Deliberate abstractions

The canonical system does not require:

- manual handling of individual cartridges;
- continuous hunger and thirst during ordinary matches;
- detailed maintenance procedures;
- per-item three-dimensional packing;
- human approval for every routine transfer;
- supply auras that ignore location; or
- automatic recovery of everything deployed.

## API and replay requirements

Authoritative logistics records require stable identifiers for:

- resources and compatibility classes;
- holders, containers, and locations;
- quantities and capacity use;
- reservations and requesting policies;
- transfer actions and commitment state;
- consumption, loss, capture, and recovery events;
- observation and report provenance; and
- mission-to-campaign write-back.

Clients and modules receive only the knowledge permitted to their actor.
Replays and audits must reconstruct resource movement and explain conservation
without exposing protected campaign or opponent information prematurely.

## Prototype parameters

The following remain open:

- package sizes and compatibility granularity;
- unit and vehicle capacity values;
- accessible and packed-storage timing;
- transfer ranges and durations;
- interruption and partial-transfer rules;
- supply ownership and looting restrictions;
- securing, extraction, and salvage rules;
- automation policy vocabulary; and
- mode-specific write-back policies.
