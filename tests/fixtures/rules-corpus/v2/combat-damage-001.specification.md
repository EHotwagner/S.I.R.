<!-- sir-rule-specification/v1 -->
<!-- source-fingerprint: 51eb470c9e98c3fb1255b6647e51c0dd8fb957d1fda6e3ef8e3cf8619e3b96b1 -->
<!-- generated-fingerprint: 4464042404acea5907cdea3b4f50730adccd3b16122dd13faa93742bff7ccd14 -->
# Expected damage

- Model: `COMBAT-DAMAGE-001`
- Schema: `1`
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

`51eb470c9e98c3fb1255b6647e51c0dd8fb957d1fda6e3ef8e3cf8619e3b96b1`
