# Tactical environment performance contract v1

All workloads traverse product assembly/validation plus the same projection and transition functions consumed by MapEditor/Simulator and spatial/combat adapters. Timing runs in Release on a monotonic clock and records host/runtime facts separately; deterministic fingerprints exclude elapsed time. Headless evidence makes no compositor claim.

| Workload | Expected scale | Structural budget |
|---|---:|---:|
| exterior assembly | 64 slots, up to 32 compatible variants per role | at most 64 selections, 2,048 compatibility inspections, 4,096 placed cells/features |
| catalog validation | 64 slots, 512 authored features | at most 512 ordered findings and 16,384 route expansions |
| editor preview | one assembled 80x80 map | one assembly, one validation, at most 6,400 projected cells and 2,048 projected features |
| local invalidation | 256 cached dependency receipts | inspect at most 256 receipts; invalidate only intersecting entries; one changed target |
| representative combat queries | 100 units and 50 environment interactions | at most one target transition per action; no propagated changes |

Initial iteration timing thresholds are 25 ms for representative assembly, 50 ms for maximum validation/preview, 10 ms for local invalidation, and 50 ms for the representative combat-query batch on the recorded host. A slower observation blocks release; it cannot be waived by relabelling the workload. The Release receipt records workload-definition digest, counters, elapsed observation, runtime, OS/architecture, and candidate commit.

Protected-subject mutations must independently prove red when selection ignores seed/state, identity verification accepts a stale hash, invalidation clears non-intersecting entries, or one action changes more than its target/declared budget.
