# Executable rule shape

Use the current signatures in `src/SIR.Domain/RuleTypes.fsi` as authority.

- `RuleMetadata`: stable ID, title, status, semantic kind, controlled statement, rationale, dependencies/supersession, source binding, examples, properties, evidence.
- `Fact`/`Predicate`/`Formula`: use typed values and explicit units; controlled English explains but never executes.
- `Transition`: declare phase, preconditions, authoritative reads/effects/events, ordering/conflict facts, and failure outcomes.
- `Algorithm`: ordinary registered F# is valid when an AST would obscure it; bind implementation symbol/fingerprint, typed inputs/result, explanation fields, and enough trusted footprint/contract data for coherence. Missing summaries remain `unknown`.
- `Narrative`: never satisfy executable coverage with narrative prose.

Canonical rules require proportional rationale, real examples/properties, a resolvable source, structured explanations exposing decisive operands/applied rules, and immutable package/implementation identity for retained replay fixtures.

Prefer domain-generated boundaries: thresholds ± one representable unit, zero/exhaustion/capacity, phase boundaries, same-tick order permutations, spatial borders/corners/edges, observation loss before resolution, and stable random-purpose identities.
