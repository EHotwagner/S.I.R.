# Session 01 — direct record

- Scenario: express the `COMBAT-DAMAGE-001` model by constructing `SpecificationModel<RuleSpecificationAst>` directly.
- Elapsed authoring/debug interval: approximately 6 minutes (kernel record introduction through the first compiler diagnostic pass).
- Corrections: 2. The generic `SchemaVersion` field made an existing `RulePackageIdentity` parameter ambiguous, and the AST record initially collided with the draft record's `Definition` field.
- Hesitation: whether author/session/time belong in semantic normalization. Decision: retain them as auditable envelope data but exclude them from semantic bytes; retain source path and revision.
- Outcome: expressive and useful for transformations, but too much envelope ceremony for routine rule authoring.
- Evidence: `RulesCorpusFixtures.evaluate` constructs the direct form and proves its normalized bytes equal the selected hybrid form.

Focused verification: `dotnet run --project tests/SIR.Domain.Tests/SIR.Domain.Tests.fsproj -c Release --no-build -- --print-rule-specification` completed in 0.550 seconds on the recorded run.
