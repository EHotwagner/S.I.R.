# Physical combat schema v1

The public F# contract declares separate `WeaponProfile`, `DamageType`, `ArmorState`, `Wound`, `SuppressionState`, `CoverObject`, `CombatEntity`, `CombatWorld`, `CombatRequest`, `CombatFact`, `CombatExplanation`, `CombatResult`, and `CombatLimits` types. `Combat.resolve` is a pure bounded transition over one request/world and `Combat.recover` is a pure commit-phase recovery transition.

Canonical phase order is eligibility → preparation/commitment → delivery → collision → cover → armor → HP → wound/incapacity → suppression → environment → emitted facts. Direct delivery consumes one `SpatialQuery` projectile/cover result; area delivery consumes one validated impact plus a distance/row/column ordered bounded cell set. Recipients are ordered by trace index/cell/entity-kind/id.

HP, armor integrity, wound list, incapacity, and suppression are independent fields. Cover is derived from the current spatial evidence for each recipient and is never stored on a unit. All values are bounded integers or `FixedPoint`; no floating authority or ambient randomness exists.

Limits: at most 256 trace cells, 256 area cells, 256 recipients, 4,096 facts, and 65,536 canonical explanation bytes. Invalid or exhausted spatial/limit evidence returns a typed rejection without partial state mutation.
