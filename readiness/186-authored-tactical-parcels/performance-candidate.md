# Tactical environment candidate performance

- Date: 2026-08-15
- Configuration: Release
- Host: Linux 7.1.4-arch1-1 x86_64
- .NET SDK: 10.0.302
- Node: 26.7.0
- Fable: 5.13.0
- Game.Core package/profile: FS.GG.Game.Core 0.13.0 / `fs-gg-game-core-fable-lockstep-v1`
- Base commit: `3dc50b5839b51b605aee7d9d5f0b1274e0f0f60d`
- Candidate commit: the commit containing this receipt

The normal `SIR.Match.Tests` executable measured the production `validate`,
`assemble`, spatial projection, action, and dependency-invalidation functions.
The representative exterior workload completed in 0.599 ms. The declared
maximum assembly (64 connected slots, 32 compatible variants per role) made
64 selections after exactly 2,048 variant inspections and completed in
11.173 ms after one explicit warm-up, below the 25 ms contract. A door state transition inspected and
changed one target, propagated zero neighbours, and invalidated exactly one
intersecting cached query entry.

The complete client qualification retained its dense 40x40 map, 3,120 edges,
200 units, 200 regions, and 7,136 interactive-node scale. Its latest Release
observations were p95 2.865 ms preview, 2.275 ms command, 2.821 ms document,
9.069 ms undo/redo, 16.373 ms import, and 1.308 ms export. The normal match
journey completed 40 live ticks in 26.060 ms; preview was 0.248 ms,
serialization 0.107 ms, transfer 0.026 ms, and rendering projection 0.077 ms.

These are headless host observations and make no compositor or portable
wall-clock claim. Structural counters and canonical bytes are deterministic;
elapsed values are deliberately excluded from content identity and replay.
