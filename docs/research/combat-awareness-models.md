---
title: Combat, Reaction, and Awareness Reference Models
status: proposed
document-type: research
version: "0.1"
last-updated: 2026-07-25
related:
  - docs/game-vision.md
reference-models:
  - Door Kickers 2
---

# Combat, Reaction, and Awareness Reference Models

## Purpose

This living research document records games and systems that inform S.I.R.'s
combat, reaction, facing, perception, and awareness architecture. A reference
identifies useful tactical relationships; it does not imply that S.I.R. copies
the referenced game's interface, time controls, spatial model, or complete
ruleset.

## Door Kickers 2

### Status

Door Kickers 2 is an explicit primary reference for the qualitative behavior of
combat, reactions, and awareness in S.I.R.

### Why it is relevant

Door Kickers 2 makes the following concerns legible and tactically decisive:

- the direction an operator faces and aims;
- lines of sight and exposure around corners and openings;
- the time required to notice, aim at, and engage a threat;
- autonomous engagement once a valid threat enters an operator's effective
  awareness;
- coordinated movement and coverage between multiple operators;
- the relationship between weapon, equipment, distance, posture, and reaction;
- surprise and the direction from which contact begins; and
- short, lethal engagements in which preparation creates a large advantage.

KillHouse Games describes Door Kickers 2 as top-down real-time tactics focused
on modern close-quarters combat, intelligence assets, special-operations units,
suppressive fire, non-linear approaches, and destructible environments. Its
controls distinguish looking briefly along movement, strafing while maintaining
an aim direction, and turning to track a chosen point while moving. The
developer has also described line-of-sight-based automated coordination as a
design goal.

### Lessons to adapt

S.I.R. should prototype:

1. **Facing as active tactical state.** Movement direction and attention
   direction are related but not necessarily identical.
2. **Explicit awareness geometry.** A unit should have strong forward
   perception and weaker side and rear awareness, modified by sensors,
   conditions, stance, movement, and class.
3. **Reaction as a process.** Detection, recognition, turning, aiming, deciding,
   and acting need not occur simultaneously, even though the game should avoid
   unnecessary simulation detail.
4. **Prepared contact advantage.** A unit already watching the correct approach
   should respond more effectively than one surprised from an uncovered
   direction.
5. **Autonomous reactions.** A unit's control module should execute authorized
   reactions without waiting for a network round trip to the human player.
6. **Coverage and coordination.** Mutually supporting fields of attention
   should make formations, entry order, overwatch, crossfires, and sector
   responsibility meaningful.
7. **Strong but explainable consequences.** Reaction and awareness advantages
   can be decisive, but the API and client must expose the facts needed to
   understand why one unit acted first.

### Boundaries: what S.I.R. does not inherit

- Door Kickers 2 permits pause-at-will planning; S.I.R. matches always advance
  continuously in real time.
- Door Kickers 2 uses freeform movement; S.I.R. uses square multi-cell
  footprints and Chebyshev distance.
- Door Kickers 2 centers on small close-quarters teams; S.I.R. targets
  approximately 50–100 units per side and broader battlefields.
- Door Kickers 2 exposes detailed manual path and facing control; S.I.R. places
  detailed execution in per-unit player-provided WebAssembly modules.
- Door Kickers 2 is a reference for tactical relationships and feel, not a
  specification for S.I.R.'s exact numerical mechanics.

### Questions for a focused prototype

- How wide should strong forward awareness be on an eight-direction grid?
- Should side and rear sectors impose detection delay, recognition delay,
  reaction delay, or different combinations?
- Which stages of reaction are explicit authoritative state, and which can be
  collapsed into a small number of readable timers?
- How should turning, movement, stance, suppression, wounds, weapons, sensors,
  and magic modify reaction?
- When does rear position grant an advantage independently of unawareness?
- When does the combination of rear position and unawareness permit an
  execution?
- What information must a unit module receive to make a reaction policy
  possible without exposing hidden world truth?

## Sources

- [Door Kickers 2 — official game page](https://inthekillhouse.com/doorkickers2/)
- [KillHouse Games — line-of-sight-based coordination development note](https://inthekillhouse.com/door-kickers-2-delayed/)
- [Door Kickers 2 community explanation of aimpoint, strafe, and slice-the-pie controls](https://steamcommunity.com/app/1239080/discussions/0/3003303575730378491/)
- [Door Kickers 2 developer discussion of reaction context](https://steamcommunity.com/app/1239080/discussions/0/594011786520332581/)
