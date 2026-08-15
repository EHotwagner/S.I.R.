# Tactical environment candidate performance

- Date: 2026-08-15
- Configuration: Release
- Host: Linux 7.1.4-arch1-1 x86_64
- .NET SDK: 10.0.302
- Node: 26.7.0
- Fable: 5.13.0
- Game.Core package/profile: FS.GG.Game.Core 0.13.0 / `fs-gg-game-core-fable-lockstep-v1`
- Base commit: `3dc50b5839b51b605aee7d9d5f0b1274e0f0f60d`
- Measured source commit: `15aabb06a403f9a2fde158ed4e6c2dfc2533078f`

The authoritative aggregate ran from a fresh detached checkout at candidate
`15aabb06`, beginning with locked .NET restore and `npm ci`. The normal
`SIR.Match.Tests` executable measured production validation, assembly, spatial
projection, combat resolution, environment actions, and dependency-local cache
invalidation. The representative exterior workload completed in 1.272 ms. The
declared maximum assembly (64 connected slots, 32 compatible variants per role)
validated in 4.134 ms, made 64 selections after exactly 2,048 variant
inspections, and completed in 7.719 ms. The 80x80 preview with 2,048 features
validated separately in 13.757 ms and completed assembly in 26.431 ms. Every
independently enforced validation/preview limit remained below 50 ms.

The production-adapter batch used 100 real combatants and resolved 50 rifle
attacks through `Combat.resolve`, production line traces, and 50 targeted
environment actions in 30.486 ms. It emitted 50 spatial query receipts, crossed
at least one spatial cell per query, and produced exactly one `HealthChanged`
target per attack without propagating unrelated environment changes. A door
state transition inspected and changed one target, propagated zero neighbours,
and invalidated exactly one intersecting cached query entry in 0.023 ms.

The complete client qualification retained its dense 40x40 map, 3,120 edges,
200 units, 200 regions, and 7,136 interactive-node scale. Its latest Release
observations were p95 2.865 ms preview, 2.275 ms command, 2.821 ms document,
9.069 ms undo/redo, 16.373 ms import, and 1.308 ms export. Its explicit tactical
100-unit preview completed in 154.828 ms against the documented 250 ms demand
batch budget, and 50 validated authoring interactions completed in 10.485 ms
against the 100 ms interaction budget. The normal match
journey completed 40 live ticks in 26.060 ms; preview was 0.248 ms,
serialization 0.107 ms, transfer 0.026 ms, and rendering projection 0.077 ms.

These are headless host observations and make no compositor or portable
wall-clock claim. Structural counters and canonical bytes are deterministic;
elapsed values are deliberately excluded from content identity and replay.
