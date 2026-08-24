# Complete `sir-author-rule` pilot session

## Inspected authority

The session inspected `RuleTypes.fsi`, the combat registry, `COMBAT-DAMAGE-001` dependencies, its source binding, conformance examples, replay/package identity, corpus generation, and the rule-shape and typed-specification skill references.

## Player-facing and typed provisional forms

Human outcome: expected damage is base weapon damage multiplied exactly once by trace probability and retained armor effect. Scope is the existing deterministic combat slice. This pilot does not change rounding, runtime evaluation, replay interpretation, dependency authority, or package identity rules. Boundary observables remain clamped retention and the explained `baseDamage`, `trace`, and `retention` operands.

Typed form: stable ID `COMBAT-DAMAGE-001`; `FormulaSemantics(FixedPoint, "damage", ...)`; dependencies `CONTENT-WEAPON-RIFLE-001`, `COMBAT-TRACE-002`, and `COMBAT-ARMOR-004`; source `src/SIR.Simulation/CombatRules.fs` at the existing durable revision; explicit reads `baseDamage`, `trace`, `retention`; explicit write `expectedDamage`; existing examples, properties, evidence, and explanation behavior preserved.

Material choice: select the hybrid surface. It keeps the current F# definition as the AST value and places the specification envelope at the registry authority boundary without creating a second runtime interpretation.

## Implementation and evidence loop

- Hypothesis/change: compile the existing damage definition from a validated specification model.
- Evidence: focused domain/simulation builds and the shared conformance executable.
- Result: compiled canonical bytes equal `damageReferenceCanonicalBytes`; the complete conformance output is byte-identical to detached P0 baseline `5a94bdc` and across .NET/Fable (`sha256:0f16eec78564daa64d0e6d4aa5b5ed123502d32c76b75f8a4ac68f27f5236286`).
- Protected failures: malformed provenance reports `SPEC-PROVENANCE-REVISION` at `/provenance/sourceRevision`; an AST change reports `/ast`; missing/malformed/stale/directly-edited generated projections fail the corpus gate.
- Registered algorithm check: `COMBAT-TRACE-002` exposes implementation symbol/fingerprint, typed inputs, explicit reads/writes, evidence, and explanation fields.
- Coherence: cone mode analyzed `COMBAT-DAMAGE-001` plus 14 dependencies/dependants, terminated complete after 139 work units, and reported `canonicalizationReady: true`.
- Remaining issue: none in P1 scope. Extraction to a separate package and migration of additional domains remain P2/P3 work.
