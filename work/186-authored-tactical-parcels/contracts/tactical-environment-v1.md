# Tactical environment schema v1

The canonical envelope contains schema version `1`, plot identity, sorted slot definitions, sorted parcel variants, seed, selected variant ids and quarter-turn transforms, sorted placed cells/features, and assembly counters. Strings are UTF-8 length-prefixed, integers are little-endian, collections are count-prefixed, and union cases use stable integer tags. The content identity is lowercase SHA-256 over exactly those canonical bytes.

Assembly sorts slots and compatible variants by ordinal id, rejects duplicates and invalid geometry before selection, and consumes exactly one returned `FS.GG.Game.Core.Rng` draw per slot having more than one compatible variant. Supported transforms are identity and 90/180/270-degree clockwise rotations. A supplied content identity that differs from the recomputed identity is rejected; content is never reinterpreted under the supplied hash.

Each semantic feature declares an id, kind, legal state, edge address, independent movement/sight/projectile/area-effect/sound/cover permeability, directional cover material/integrity/penetration/protected directions, capability descriptors, and dependency keys. Feature actions may change only their target, never trigger neighbour collapse, and increment spatial revision once only when canonical state changes.

Legacy MapEditor format-v4 `(Wall|Door|Window, isOpen)` edges migrate explicitly: walls become `Wall/Intact`, closed/open doors become `Door/Closed|Open`, and windows become `Window/Closed`. Default modality values are feature/state-specific and remain overridable only by valid schema-v1 values. Unsupported schema versions, impossible states, and stale identities are typed failures.
