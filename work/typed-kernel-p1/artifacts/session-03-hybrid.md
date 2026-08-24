# Session 03 — hybrid helper

- Scenario: migrate the authoritative construction of `COMBAT-DAMAGE-001` while leaving its ordinary F# `RuleDefinition` visible as the typed AST value.
- Elapsed authoring/debug interval: approximately 3 minutes from the migration edit through a clean simulation build.
- Corrections: 0 in the migration call site; it compiled on the first focused build after the kernel itself was green.
- Hesitation: choosing explicit operational write vocabulary for a formula. Decision: `expectedDamage` is the observable result subject; registered pure algorithms use a named `no-write:*` token instead of an implicit empty effect.
- Outcome: least ceremony, keeps ordinary F# data legible, and makes identity/provenance/intent explicit at the authority boundary.
- Evidence: compiled canonical rule bytes equal the pre-migration reference bytes; native and Fable projection and full conformance output compare byte-for-byte.
