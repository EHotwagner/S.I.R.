# Typed specification pilot

The P1 pilot wraps a rule AST in `SpecificationModel<RuleSpecificationAst>`. The envelope carries a stable identity, schema version, provenance, and authoring intent; normalization deliberately excludes session prose and timestamps while retaining the source path and revision.

Use the hybrid surface unless evidence favors another form:

```fsharp
RuleSpecification.hybrid identity provenance intent definition reads writes
```

The `definition` remains ordinary shared F# data. `reads` and `writes` name operational subjects explicitly; a pure registered algorithm uses a named no-write token rather than an empty implicit effect. Compile with `RuleSpecification.compile`, report every returned diagnostic with its code and path, and place only the compiled rule in the registry.

Before accepting a rule:

1. Confirm model identity equals the rule ID and provenance equals its `RuleSource` path/revision.
2. Compare compiled canonical bytes with the pre-migration/reference bytes when migrating authority.
3. Exercise malformed provenance, an AST semantic change, and registered-algorithm bindings when applicable.
4. Regenerate and check the Markdown projection and JSON receipt. A missing, malformed, stale-source, or directly edited artifact must fail closed.
5. Run the existing corpus, replay, and .NET/Fable gates; the specification wrapper may not create a second runtime authority.
