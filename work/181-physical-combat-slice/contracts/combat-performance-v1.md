# Combat performance workload v1

The representative workload executes open, partial/full-cover, front/rear armor, penetration/no-penetration, intervening unit/object, friendly fire, support area suppression, lobbed area, incapacity, and cover-destruction scenarios through the production combat resolver after warm-up. The stress workload builds 100 canonically ordered units and resolves 50 attacks in one authoritative tick through the same resolver/update route.

Structural ceilings are 256 trace cells, 256 area cells, 256 recipients, 4,096 facts, and 65,536 bytes per explanation. Environment-qualified Release observation targets are 20 ms for the representative matrix and 50 ms for the stress tick. These are headless authoritative-route observations, not live-compositor evidence.

The receipt records workload-definition digest, exact commit/artifact/package/runtime identities, OS/CPU/runtime capability, elapsed observations, attack/trace/area/recipient/fact/explanation counters, warm-up, and pass/fail. Missing/stale identity, a cap breach, unreadable output, or a failed run blocks qualification.
