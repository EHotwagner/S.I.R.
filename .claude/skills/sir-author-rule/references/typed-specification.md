# Published typed specification boundary

S.I.R. consumes the shared specification envelope and compiler primitives from the exact
`FS.GG.SDD.Artifacts` package pin in `Directory.Packages.props` (the P3 adoption is
`1.3.0-preview.3`). The public F# contract is in
`FS.GG.SDD.Artifacts.TypedSpecifications`; there is deliberately no local
`SpecificationModel.fs` or `SpecificationModel.fsi` to inspect or extend.

The package-owned `SpecificationModel<'Extension>` carries stable identity, schema version,
provenance, authoring intent, and evidence obligations. S.I.R. owns
`RuleSpecificationAst`, its gameplay validation/compiler adapter, `RuleDefinition`, registries,
interpreters, and projections. Never copy package types into S.I.R. or treat a producer source
checkout as a package boundary.

Normalization deliberately excludes session prose and timestamps while retaining the source
path and revision. `RuleSpecification.fs` registers S.I.R.'s extension codec, diagnostics,
normalization, semantic diff, and projection with the package compiler.

Use the hybrid surface unless evidence favors another form:

```fsharp
RuleSpecification.hybrid identity provenance intent definition reads writes
```

The `definition` remains ordinary shared F# data. `reads` and `writes` name operational subjects explicitly; a pure registered algorithm uses a named no-write token rather than an empty implicit effect. Compile with `RuleSpecification.compile`, report every returned diagnostic with its code and path, and place only the compiled rule in the registry.

Before accepting a rule:

1. Confirm the exact public package restores from the configured feed and the model identity equals the rule ID while provenance equals its `RuleSource` path/revision.
2. Compare compiled canonical bytes with the pre-migration/reference bytes when migrating authority.
3. Exercise malformed provenance, an extension-contract mismatch, a package-identity mismatch, an AST semantic change, and registered-algorithm bindings when applicable.
4. Regenerate and check the Markdown projection and JSON receipt. A missing, malformed, stale-source, or directly edited artifact must fail closed.
5. Run the existing corpus, replay, and .NET/Fable gates; the specification wrapper may not create a second runtime authority.
