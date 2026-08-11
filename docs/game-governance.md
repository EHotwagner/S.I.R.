# Fable game governance parity

S.I.R. consumes `FS.GG.Game.Core` only through the centrally pinned published package.
The shared authoritative boundary is deliberately limited to profile
`fs-gg-game-core-fable-lockstep-v1`: `Cell` ordering, `Edges.edgeBetween`,
`Los.lineOfSightBy`, and `Pathfinding.astar`. S.I.R. owns protocol, replay identity,
simulation rules, UI, and knowledge policy; compilation alone never promotes another
Game.Core surface to authority.

`scripts/test-conformance.sh` is the clean local route. It verifies the package/configuration
boundary before the existing .NET/Fable canonical comparisons and their first-divergence
inversions. CI invokes that same boundary gate independently.

## Responsibility matrix

| Native/Skia responsibility | Fable/SVG disposition | S.I.R. evidence |
| --- | --- | --- |
| Package/profile identity and exact spatial primitives | Direct | pinned package, lock files, canonical .NET/Fable fixture bytes |
| Controlled imports and public/package boundaries | Direct | `verify-fable-game-governance.sh`, CI, governance route |
| Game development skills | Direct | byte-identical materialization in `.agents`, `.claude`, and `.codex` |
| Frame/update performance workloads | Adapted | Fable/SVG production route uses structural scene/search counters; browser compositor evidence remains separately qualified |
| Raster/Skia drawing implementation | Intentionally inapplicable | browser UI renders SVG/Feliz; it does not claim Skia renderer parity |
| Native-only and floating Game.Core surfaces | Intentionally inapplicable to authority | profile classification keeps them out of shared lockstep behavior |

The performance baseline is producer-owned: this repository does not create a second timing
contract. Workloads declare expected scale and deterministic counters before changes, and release
evidence is tied to the exact candidate.
