# Spatial diagnostics and performance contract v1
## Player route

The production-browser journey boots the real entry, selects a real simulator unit through player-emittable controls, opens `View` then `Spatial diagnostics`, and inspects renderer-neutral authoritative data. The panel shows query kind, normalized inputs, result/failure, footprint samples, crossed cells/edges, cover/exposure contributors, expansion/truncation facts, map/ruleset/spatial/knowledge identities, and package/profile/source identity. Presentation code performs no spatial calculation.

## Qualification workloads

- Representative map: 32x32; maximum map: 80x80.
- Selected-unit exact LOS and one bounded route preview capped at 64 result cells and 4,096 expansions.
- One disclosed cell/edge revision invalidation proving unrelated entries survive.
- Deterministic demand batches for 100 and 200 units.

Release qualification asserts the structural caps in `spatial-query-v1.md` and records candidate SHA, Release artifact identity, operating system/architecture, processor count, .NET SDK/runtime, Node, Fable, browser, and package/profile identities. Environment-qualified targets are 20 ms selected LOS, 50 ms route preview, 10 ms local invalidation, 250 ms 100-unit demand, and 500 ms 200-unit demand. A structural-cap breach always fails; a latency breach fails on the declared qualification host and remains explicitly reported elsewhere.
