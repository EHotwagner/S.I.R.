<!-- fsgg-typed-specification/v1 -->
<!-- extension: sir-rule-specification/1 -->
<!-- source-fingerprint: 6c09fe9c43a2f3c658b3ab1a0f80459ec6c7206e63408a65d8a0c6d14b078af5 -->
<!-- generated-fingerprint: 209b7d2ac3ed1a174a436a130973a47daf78cdc278935b3f063181299bf9a2f7 -->
# Expected damage

- Model: `COMBAT-DAMAGE-001`
- Schema: `1`
- Extension: `sir-rule-specification/1`
- Rule: `COMBAT-DAMAGE-001` (`Formula`)
- Status: `Canonical`
- Source: `src/SIR.Simulation/CombatRules.fs@eb0b2c29a80f0bf3b400ce4415bf8587b4645083`
- Dependencies: COMBAT-ARMOR-004, COMBAT-TRACE-002, CONTENT-WEAPON-RIFLE-001
- Reads: baseDamage, trace, retention
- Writes: expectedDamage

## Controlled statement

S.I.R. combat simulation Expected damage

## Rationale

Expected damage is the weapon effect multiplied once by trace probability and retained armor effect.

## Semantic fingerprint

`6c09fe9c43a2f3c658b3ab1a0f80459ec6c7206e63408a65d8a0c6d14b078af5`
