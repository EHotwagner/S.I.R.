---
name: sir-author-rule
description: Collaboratively add, change, tune, formalize, explain, or repair a S.I.R. gameplay rule in the embedded F# corpus. Use for natural-language rule ideas and revisions that must become typed shared semantics, rationale, examples/properties, source-bound explanations, executable evidence, and a coherence-checked dependency cone.
---

# Author a S.I.R. rule

Develop the rule with the user; do not treat the first draft or a successful compile as completion. Keep execution in shared F# and use the independent coherence skill as a gate, not as an editor.

## Workflow

1. Inspect `src/SIR.Domain/RuleTypes.fsi`, `src/SIR.Domain/SpecificationModel.fsi`, `src/SIR.Domain/RuleSpecification.fs`, the current registry, related rules and dependencies, source/history bindings, examples, simulation paths, generated docs, and tests. Read [references/rule-shape.md](references/rule-shape.md) and [references/typed-specification.md](references/typed-specification.md) before proposing the typed shape.
2. Restate the player-facing outcome, scope, non-goals, observables, and boundary cases from repository evidence. Record any material design choice that cannot be answered locally, but do not ask it before showing the provisional human and typed forms in step 4.
3. Select the narrowest honest semantic kind: fact, predicate, formula, transition, registered algorithm, or narrative. Preserve the stable ID when meaning remains compatible; use explicit supersession for an incompatible replacement.
4. Present both provisional forms before settling semantics:
   - human: controlled statement, rationale, examples, boundaries;
   - typed: `RuleMetadata`, `RuleSemantics`, dependencies, source/package binding, properties, evidence, and explanation operands.
   Then ask one focused question for the first unresolved material choice, with a recommendation and consequences.
5. Declare public signatures first where the surface changes. For pilot rules, author a `SpecificationModel<RuleSpecificationAst>`, compile it through `RuleSpecification.compile`, and keep the compiled `RuleDefinition` as the only registry/runtime input. Use `RuleSpecification.hybrid` by default; use the direct record for low-level transformations and the computation expression only when its sequence materially improves readability. Never add parallel TypeScript or JavaScript game semantics.
6. Run the smallest focused examples, envelope/AST diagnostics, semantic-diff checks, boundary/property tests, deterministic scenario/replay fixtures, explanation checks, generated projection freshness, and .NET/Fable comparison that exercise the change. A new or modified gate must have an observed red mutation before its restored green result. Do not edit generated specification Markdown or its receipt directly; regenerate them with `scripts/generate-rules-corpus.sh --write`.
7. Invoke `$sir-check-rule-coherence` in `cone` mode for every changed rule. Inspect one failing witness at a time.
8. Classify each failure as an implementation defect, rule defect, example defect, or unresolved design choice. Revise the appropriate source with the user and repeat from step 4.

## Stop conditions

Stop only when applicable focused gates and cone analysis pass at their declared strengths, the user explicitly accepts a documented prototype/open question, or an external blocker remains. Never weaken, delete, relabel, or bypass a failing check solely to finish.

After every iteration, report: hypothesis/change, evidence command, result, remaining issue, and next design choice. Leave the corpus passing or name every red obligation exactly.
