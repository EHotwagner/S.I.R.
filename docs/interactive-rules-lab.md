---
title: Simulation workspace
category: Tools & Evidence
categoryindex: 5
index: 1
description: Edit maps, assign controllers, run deterministic simulations, inspect replays, and test rule parameters.
---

# Simulation workspace

<div class="sir-status sir-status-browser">
  <strong>Runtime: Fable/JavaScript in your browser.</strong>
  Replay verification re-executes accepted kernel inputs; it does not re-run
  player WASM and cannot establish authoritative match verification. That
  stronger claim is produced only by the .NET verifier after exact-artifact
  Wasmtime re-execution reproduces the accepted-output journal.
</div>

The application has three workspaces:

| Workspace | Function |
|---|---|
| **Map and simulation** | Edit terrain, semantic edges, and square unit footprints; assign controllers; execute ticks |
| **Replay** | Load, seek, and inspect a versioned replay without exposing hidden state |
| **Rules and data** | Run bounded combat scenarios and inspect canonical, proposed, and experimental content |

## Map editing

Choose a terrain, edge, or unit tool and activate a grid cell. Units occupy
square footprints. A size-2 unit therefore uses a 2×2 base and a square symbol
that fits that base. Select a unit to edit its side, class identifier, square
size, HP, controller, and script. Walls and windows block movement. A door
cycles through closed, open, and removed states.

Maps import and export as deterministic `.sir-map` text. Records are sorted, so
the same map produces the same document:

```text
SIR-MAP 1
size 12 8
terrain 5 3 objective
edge 6 2 south door closed
unit 1 blue rifleman 1 1 2 12 12 manual -
unit 2 blue medic 1 5 2 12 12 scripted E,E,N
unit 3 red goblin 9 1 1 12 12 general -
```

The importer rejects unsupported versions, invalid coordinates, duplicate unit
identifiers, overlapping footprints, blocked placement, and malformed
controller scripts.

## Controllers

| Controller | Execution |
|---|---|
| **Manual** | Acts only when the user issues a direction |
| **Scripted AI** | Repeats a comma-separated sequence of eight-direction moves |
| **General AI** | Uses the bundled deterministic heuristic: approach the nearest hostile and attack when adjacent |

Automatic ticks execute units in ascending identifier order. The general
controller is a local reference policy, not an external or language-model
service. Select **Step** for one tick or **Run** for repeated ticks.

## SVG inspection

The simulation view projects the current map into the same SVG battlefield used
by replay inspection. Square symbols scale to square bases. Semantic edges,
health, controller status, event tooltips, focus states, and explanatory motion
remain presentation data; the tick result is authoritative within the sandbox.

## Replay and rules

Replay verification re-executes accepted kernel inputs. Editing a replay or a
rules parameter creates a sandbox fork and removes the verification claim.
Rules scenarios execute in a Web Worker and use bounded integer inputs. Exports
include enough data to reproduce the result.

<noscript>
  <div class="sir-status sir-status-warning">
    <strong>JavaScript is disabled.</strong>
    The simulation workspace cannot run, but the explanatory
    corpus, deterministic .NET example, architecture pages, and API reference
    remain available.
  </div>
</noscript>

<div id="sir-replay-app" aria-label="S.I.R. simulation workspace">
  <p>Loading the Fable browser application…</p>
</div>

## Trust boundary

The browser receives only replay or scenario data deliberately supplied to it.
It is not a live match client, does not receive hidden authoritative state, and
does not silently upgrade old replay engines. Read the
[Fable client and documentation architecture](fable-client-and-documentation.md)
for the complete verification and disclosure contract.
