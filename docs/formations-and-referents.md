---
title: Formations and Referents
category: Battlefield Systems
categoryindex: 4
index: 3
status: proposed
document-type: living-design
version: "0.3"
last-updated: 2026-07-29
related:
  - docs/game-vision.md
  - docs/control-abi.md
  - docs/wasm-control-architecture.md
  - docs/research/squad-command-and-succession.md
---

# Formations and Positional Referents

## Purpose

Orders, control modules, and standard-module postures must be able to talk
about space without naming absolute cells. An instruction that says *withdraw
to grid 214,88* is tied to one map, does not survive that cell being destroyed
or overrun, and expresses a coordinate rather than an intent.

This document defines the two mechanisms that let control behavior refer to space:
**positional referents**, which name a place by its role, and **formations**,
which describe how a squad arranges itself relative to something. It also
resolves what a unit may know about mission objectives, because an objective is
a referent and its disclosure follows the same rules.

Both become capability descriptors and events in the
[Control ABI Surface](control-abi.md).

## Positional referents

### What a referent is

A referent binds a **role** to a **place**:

```text
referent = role tag + location + scope + validity
```

The location may be a cell or a bounded area. The role tag is what makes it
portable: a module or posture withdraws to *the rally point*, not to a
coordinate, so the same control behavior works on every map and survives the
designated place changing.

Candidate roles:

- rally point;
- fallback or withdrawal position;
- assembly area;
- casualty collection point;
- supply point or cache;
- support-by-fire position;
- entry point or breach point; and
- objective location.

### Designation and propagation

A referent is designated by the player through headquarters, or by a module for
its own squad where its disclosed capabilities and ordinary request validation
permit it.

**Designation travels the communications topology like any other command, and
each unit holds its own belief about it.** The server holds the authoritative
designation; a unit acts on what has actually reached it.

This is the important rule and it follows directly from the established
knowledge model. A squad out of contact keeps the rally point it was last told
about. A new designation issued while that squad is jammed does not reach it.

### Stale referents

A referent can be overrun, destroyed, cut off, or rendered unreachable, and the
unit acting on it may not know.

A squad that withdraws to a rally point that has since been taken is not
suffering a bug. It is suffering the consequence the knowledge architecture
exists to produce, and it is the same consequence a real force suffers when a
report does not arrive. A module that withdraws blindly is worse than one that
considers recent reports about the destination, which gives module authors
something real to be good at.

The server does not silently redirect a unit away from a referent that has
become dangerous, for the same reason it does not silently redirect a dangerous
shot.

### Validity

A referent carries authoritative validity state — designated, superseded,
overrun, unreachable, or expired. Validity changes generate reports under the
ordinary observation and reporting rules; they do not update every unit's belief
directly.

A unit may hold a belief that the authoritative state contradicts. That is
legitimate and must be representable in replays and in the API without exposing
the authoritative value to an actor not entitled to it.

## Formations

### Design position

A formation is **an intent layer over the existing cooperative movement rules,
not a new movement system.**

Cooperative movement already solves the hard problems: footprint reservations,
friendly passage, dependency resolution, deadlock detection, and deterministic
yielding. A formation does not re-solve any of that. It supplies goals that
those rules then satisfy as well as terrain allows.

Formation is therefore a **preference that degrades**, never a constraint. A
wedge cannot fit through a doorway; the correct behavior is for it to become a
file and reform on the far side, not for the squad to stall trying to hold a
shape.

### Stations

A formation assigns each member a **station**:

```text
station = relative offset + attention sector + role slot
```

Both halves matter. The offset is where the member should be; the attention
sector is what that member is responsible for watching. A formation that only
described positions would miss the tactically decisive part, since mutual
coverage — and the gaps in it — is what makes formation choice consequential
under the acquisition model.

Assigning sectors through formation is what makes all-round security an
expressible property rather than something each module must invent.

### Reference frame

Offsets are relative to a declared reference:

- the acting squad leader;
- a movement axis, so the shape rotates with direction of travel;
- a fixed bearing, so the shape holds orientation regardless of travel; or
- a referent, such as a position being approached.

The distinction between axis-relative and bearing-relative is tactically real. A
squad crossing open ground wants its shape oriented to its direction of travel;
a squad holding a position wants sectors fixed to the terrain regardless of how
individuals shuffle.

### Templates and assignment

A formation template is versioned ruleset content naming a shape and its
stations — column, file, wedge, line, echelon, staggered column, and so on.
Templates declare their stations, sector assignments, and the footprint sizes
and movement profiles they are valid for.

Assignment maps squad members to stations. It should consider role and weapon,
so a support weapon lands where its area engagement covers the intended
approach, and command roles sit where their communications reach. Assignment is
authoritative state and is recalculated on casualty, succession, or template
change.

### Integrity

Formation integrity is a **derived measure** of how well members currently
occupy their stations. It is not authoritative state that must be maintained;
it is a derived fact a module can inspect and a leader effect can influence.

Integrity degrades through terrain constriction, casualties, suppression,
contact, and communication loss, and it recovers as members return to station.
This gives a module or standard-module posture a legible input — *reform before
advancing* — without requiring a formation to be a hard constraint.

### Terrain, edges, and levels

Formations interact with the two-layer spatial model rather than ignoring it. A
station that falls inside a wall, beyond a closed edge, or on another level is
not occupiable, and assignment must degrade rather than fail.

Doorways, stairs, and ladders are the ordinary cause of formation collapse and
should be handled as a routine transition — file through, reform beyond — not as
an error condition. Multi-level terrain means a formation may legitimately span
levels, and templates must state whether they permit that.

### Casualties and succession

Losing a member vacates a station. Losing the acting leader changes the
reference frame itself when offsets are leader-relative.

Reassignment happens under the established succession rules: the server promotes
the first eligible successor, stations are recalculated from surviving members,
and the squad's modules receive the authoritative change. The brief handover
disturbance already described applies to formation reorganization as well.

### Cost

The selected formation template, station assignments, and resulting movement
goals are authoritative state handled by the cooperative movement system.
Every unit's module is still invoked on every tick, but preserving an existing
formation does not require the server to evaluate module conditions or carry a
new network message. It therefore does not spend command bandwidth.

This is deliberate. Formation keeping is exactly the kind of continuous, precise
coordination that the delegated-execution thesis exists to provide, and it would
be worthless if a squad had to buy attention to stay in a wedge.

`formation planning` remains an expensive metered host service for the distinct
case of a module computing a *new* arrangement, rather than the server
maintaining an existing one.

## Objective knowledge

Objectives are authoritative state machines, and their disclosure to players is
already specified. What an individual unit may know was not.

**An objective is a referent, and its facts reach a unit the same way any other
information does** — through observation and through reports travelling the
communications topology. Nothing about an objective is universally known merely
because it is a mission objective.

A unit may hold:

- an objective's designated location, where that has been communicated to it;
- objective state and progress that has been reported to it;
- what it can directly observe of the objective's conditions; and
- its own assigned relationship to the objective, such as securing it or
  screening it.

A disconnected squad therefore holds a stale objective picture exactly as it
holds a stale rally point, and may continue securing an objective that
headquarters already knows is lost. This is consistent with every other part of
the knowledge model and requires no special case.

## Dependent work

Movement intent and engagement can target a referent rather than a coordinate,
which is what keeps control logic portable across maps. Formation intent becomes
a request kind, and referent designation, referent invalidation, objective state
changes, and station changes become events a module is told about.

## Open parameters

- The referent role catalog, and whether modules may designate all roles or only
  some.
- Whether referents are per-squad, per-force, or both.
- Referent area shapes and whether a referent may be a route rather than a
  place.
- The formation template catalog and their station layouts.
- The station assignment algorithm, and how much of it is player-configurable.
- The integrity measure, and how the standard module's postures interpret a
  broken formation.
- Whether templates may span levels, and how stations project across them.
- Station reoccupation behavior after casualties: hold gaps, or close up.
- Whether a formation constrains movement speed to its slowest member, and
  whether that is a template property.
