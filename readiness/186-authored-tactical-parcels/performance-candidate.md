# Tactical environment candidate performance

- Date: 2026-08-15
- Configuration: Release
- Host: Linux 7.1.4-arch1-1 x86_64
- .NET SDK: 10.0.302
- Node: 26.7.0
- Fable: 5.13.0
- Game.Core package/profile: FS.GG.Game.Core 0.13.0 / `fs-gg-game-core-fable-lockstep-v1`
- Base commit: `3dc50b5839b51b605aee7d9d5f0b1274e0f0f60d`
- Measured source commit: `bf4091af8dce2b13ca7f3a2b648592f874b13e2a`

The repair-phase authoritative aggregate ran at candidate `bf4091a`, beginning
with locked .NET restore and `npm ci`. The normal
`SIR.Match.Tests` executable measured production validation, assembly, spatial
projection, combat resolution, environment actions, and dependency-local cache
invalidation. The representative exterior workload completed in 1.074 ms. The
declared maximum assembly (64 connected slots, 32 compatible variants per role)
validated in 2.039 ms, made 64 selections after exactly 2,048 variant
inspections, and completed in 8.425 ms. The 80x80 preview with 2,048 features
validated separately in 7.653 ms and completed assembly in 20.305 ms. Sixteen
simultaneous independent Release processes retained the exact workload and
counters with preview assembly observations from 24.715 ms through 36.538 ms.
Every independently enforced validation/preview limit remained below 50 ms,
including the host-contention proof.

The repair removes two allocation multipliers from the real production route:
schema-v1 canonical bytes now stream into one ordered buffer instead of
concatenating one array per scalar, and valid bounded authored parcels validate
reachability over their dense declared footprint instead of hashing records and
allocating four neighbour records per visited cell. Malformed or oversized
inputs retain the general hash-set fallback. The canonical byte grammar,
1,094-byte native/Fable stream, content identity, and structural counters are
unchanged.

The production-adapter batch used 100 real combatants and resolved 50 rifle
attacks through `Combat.resolve`, production line traces, and 50 targeted
environment actions in 11.875 ms. It emitted 50 spatial query receipts, crossed
at least one spatial cell per query, and produced exactly one `HealthChanged`
target per attack without propagating unrelated environment changes. A door
state transition inspected and changed one target, propagated zero neighbours,
and invalidated exactly one intersecting cached query entry in 0.018 ms.

The complete client qualification retained its dense 40x40 map, 3,120 edges,
200 units, 200 regions, and 7,136 interactive-node scale. The normal match
journey completed 40 live ticks in 25.480 ms; preview was 0.258 ms,
serialization 0.120 ms, transfer 0.028 ms, and rendering projection 0.079 ms.

These are headless host observations and make no compositor or portable
wall-clock claim. Structural counters and canonical bytes are deterministic;
elapsed values are deliberately excluded from content identity and replay.
