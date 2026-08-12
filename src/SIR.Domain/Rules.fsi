namespace SIR.Domain

[<RequireQualifiedAccess>]
module Rules =
#if !SIR_WEB_CLIENT
    val validate: RuleDefinition list -> Result<RuleDefinition list, RegistryError list>
#endif
    val evaluate: Map<string, TypedValue> -> FormulaExpr -> Result<TypedValue, EvaluationError>
    val canonicalRuleBytes: RuleDefinition -> byte array
    val canonicalManifestPayload: schemaVersion: int32 -> sourceCommit: string -> RuleDefinition list -> byte array
    val decodeCanonicalManifestPayload: payload: byte array -> Result<int32 * string * RuleDefinition list, string>
    val manifestDigestForPayload: identity: RulePackageIdentity -> canonicalPayload: byte array -> byte array
    val packageIdentity: engineIdentity: string -> compatibilityProfile: string -> packageVersion: string -> sourceCommit: string -> implementationArtifacts: (string * byte array) list -> RuleDefinition list -> RulePackageIdentity
#if !SIR_WEB_CLIENT
    val formulaNotation: FormulaExpr -> string
    val manifestJson: identity: RulePackageIdentity -> RuleDefinition list -> string
    val coverageJson: identity: RulePackageIdentity -> RuleDefinition list -> string
#endif
    val canonicalApplicationBytes: RuleApplication -> byte array
