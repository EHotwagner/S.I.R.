# S.I.R.

A fast-paced, grid-based, **real-time tactical skirmish game** for large forces
in a near-future world undergoing an incursion by monsters and magic.

Each side fields roughly 50–100 units. The player commands rather than
puppeteers: units execute player-supplied WebAssembly control logic on the
authoritative server, while the human directs the wider battle through squads,
doctrine, intelligence, communications, and logistics.

That division is the point. Delegating precise execution to code makes tactical
rules practical that a human could not micro-manage in real time — directional
awareness, reaction timing, fire discipline, flanking, ambush, and execution —
so the game tests judgment and preparation rather than actions per minute.

## Status

**Design phase. There is no implementation yet.** This repository currently
contains the design set only. Nothing here has been built, measured, or
validated in code, and numeric values throughout are explicitly marked as
prototype parameters.

Documents carry a `status` of `proposed` or `accepted` in their front matter.
`accepted` with `decision-status: canonical` marks a decision that later work
should treat as settled.

## The design set

[`docs/game-vision.md`](docs/game-vision.md) is the authoritative living
description of the intended game. Everything else derives from it.

### Architecture

| Document | Covers |
|---|---|
| [game-vision](docs/game-vision.md) | The authoritative vision. Start here. |
| [simulation-core-architecture](docs/simulation-core-architecture.md) | Deterministic 20 Hz kernel, state, replay, parallelism |
| [combat-resolution](docs/combat-resolution.md) | Traces, cover, armor, wounds, suppression, engagement |
| [tactical-environment-architecture](docs/tactical-environment-architecture.md) | Map construction, cover composition, verticality, destructibility |
| [wasm-control-architecture](docs/wasm-control-architecture.md) | Module ABI, fuel, command bandwidth, standing doctrine |
| [doctrine-vocabulary](docs/doctrine-vocabulary.md) | Doctrine conditions and actions; currently a readiness audit |
| [formations-and-referents](docs/formations-and-referents.md) | Named positional referents, formations, objective knowledge |
| [casualty-and-medical-architecture](docs/casualty-and-medical-architecture.md) | Casualty states, medical actions, evacuation |
| [public-protocol-architecture](docs/public-protocol-architecture.md) | Canonical gRPC service split, sessions, projections |
| [codebase-architecture](docs/codebase-architecture.md) | F# solution layout and dependency graph |
| [technology-stack](docs/technology-stack.md) | .NET 10, FS.GG integration boundaries, adapters |
| [logistics-architecture](docs/logistics-architecture.md) | Stockpiles, manifests, battlefield supply, write-back |
| [magic-system](docs/magic-system.md) | Risk-based casting, strain, breach, shattering |
| [setting-and-factions](docs/setting-and-factions.md) | Setting, the diegetic System, faction contracts |
| [mission-lifecycle](docs/mission-lifecycle.md) | Missions, bidding, extraction, campaign write-back |
| [skirmish-development-plan](docs/skirmish-development-plan.md) | Milestones, scenarios, scale gates |
| [visual-direction](docs/visual-direction.md) | Graphical language and tactical overlays |

### Research

Comparative research backing the above lives in [`docs/research/`](docs/research/),
covering reference models, progression systems, squad command and succession,
and the WASM runtime and public transport selections.

## Foundational decisions

- **F# on .NET 10**, using the [FS.GG](https://github.com/FS-GG) framework family.
- **20 Hz fixed-step** authoritative simulation; matches target ~20 minutes.
- **Grid-based**, 0.5 m cells, square `N×N` footprints, Chebyshev distance, with
  thin structures modelled as semantic cell edges and multi-level terrain.
- **Per-unit WebAssembly** control through direct Wasmtime embedding, with
  metered fuel and a player-allocated command-bandwidth budget.
- **Native gRPC** as the first public transport, contract-first from `.proto`.
- **Server-authoritative knowledge filtering.** The canonical client holds no
  gameplay privilege unavailable to a third-party client.

## Contributing

The design set is intended to drive implementation, not be replaced by it. A
change that alters a canonical rule should say so explicitly and update every
document that depends on it — the value of this repository is that its documents
agree with one another.

## License

Licensed under the **GNU Affero General Public License v3.0**. See
[LICENSE](LICENSE).

The authoritative server is part of the AGPL-licensed project rather than a
closed service alongside it. Independent third-party servers are encouraged but
are not a supported compatibility target.

FS.GG dependencies are MIT licensed, which is compatible with AGPL
redistribution.
