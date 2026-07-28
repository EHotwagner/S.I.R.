---
title: S.I.R. Documentation
category: Overview
categoryindex: 1
index: 1
description: Architecture, deterministic simulation reference, and interactive rules tooling for S.I.R.
---

# S.I.R.

S.I.R. is a fast-paced, grid-based tactical skirmish game built around a
deterministic F# simulation. The same authoritative gameplay source compiles
for .NET and JavaScript through Fable; the browser remains an inspection and
design host, never the match authority.

## Start here

- [Game vision](game-vision.md) defines the intended game.
- [Simulation core architecture](simulation-core-architecture.md) defines the
  fixed-step deterministic kernel.
- [Fable client and documentation architecture](fable-client-and-documentation.md)
  defines cross-runtime replay, the rules laboratory, and this site.
- [Deterministic simulation example](deterministic-simulation.fsx) evaluates
  fixed .NET evidence while explaining the shared F# boundary.
- [Interactive replay and rules laboratory](interactive-rules-lab.md) mounts
  the Fable browser application and labels its verification limits.

## Documentation modes

| Surface | Runtime | Authority |
|---|---|---|
| Literate examples | .NET during the strict site build | Fixed explanatory evidence |
| Interactive replay | Fable/JavaScript in the browser | Browser-kernel verification |
| Rules laboratory | Fable/JavaScript in a Web Worker | Exploratory sandbox evidence |
| Match host | .NET with exact player-WASM execution | Authoritative |

The sidebar is intentionally limited to the primary vision, architecture,
interactive, and reference entry points. The complete explanatory corpus,
including retained research evidence, remains indexed by site search. Generated
API pages include links back to the corresponding source on the `main` branch.

## Build locally

```bash
npm ci
./scripts/build-docs.sh
```

The locked build evaluates literate scripts, generates API reference pages,
builds the Fable client, and verifies the versioned asset integrity manifest.
Generated files live under `artifacts/site/` and are not committed.

## License

S.I.R. is licensed under the
[GNU Affero General Public License v3.0](https://github.com/EHotwagner/S.I.R./blob/main/LICENSE).
