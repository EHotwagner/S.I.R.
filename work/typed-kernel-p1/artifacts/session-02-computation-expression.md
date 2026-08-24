# Session 02 — computation expression

- Scenario: express the same rule with `definition`, `reads`, and `writes` custom operations.
- Elapsed authoring/debug interval: approximately 5 minutes.
- Corrections: 2. The draft field needed a distinct name to avoid record inference ambiguity, and the first separate signature for `Yield` did not match the compiler-generated member shape; the builder remains public from its implementation file rather than carrying a brittle duplicate signature.
- Hesitation: the sequential appearance suggests order semantics even though the operations only fill a draft. This is pleasant for larger declarations but adds a bespoke mini-language to a three-field pilot AST.
- Outcome: normalized and compiled output equals the selected form, but the extra builder mechanics are not justified as the P1 default.
- Evidence: the computation-expression model in `RulesCorpusFixtures.evaluate` normalizes byte-identically to the selected model under both .NET and Fable.
