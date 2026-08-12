# Spatial cache and requester-knowledge contract v1
## Knowledge boundary

The caller supplies a stable requester-knowledge identity and revision plus the world facts disclosed to that requester. Projection occurs before request validation, traversal, dependency capture, diagnostics, and encoding. Unknown doors, units, breaches, obstructions, and their absence map to the same projected cell/edge vocabulary.

Public observation consists only of canonical result/explanation bytes and a fixed workload class. It never contains raw cache keys, hit/miss timing, bucket sizes, secret dependencies, internal invalidation counts, privileged labels, or different diagnostic text for knowledge-indistinguishable worlds.

## Cache partitions

- Static entries bind map identity, ruleset identity, spatial revision, disclosed static geometry, modality/profile, normalized footprint, and endpoints.
- Dynamic entries additionally bind only disclosed occupancy/edge revision tokens actually depended on by evaluation.
- Pure invalidation removes a dynamic entry exactly when its dependency receipt intersects a changed disclosed token, or when its immutable revision identity changes.
- A cache hit returns the same public canonical payload as uncached evaluation. Cache provenance is retained only for privileged test/telemetry receipts outside public bytes.

Paired fixtures whose authoritative worlds differ only in undisclosed facts must emit identical public bytes, outcome vocabulary, diagnostics, and workload class for all schema-v1 query kinds.
