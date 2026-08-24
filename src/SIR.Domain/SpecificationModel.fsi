namespace SIR.Domain

[<Struct>]
type SpecificationIdentity = private SpecificationIdentity of value: string

[<RequireQualifiedAccess>]
module SpecificationIdentity =
    val create: string -> Result<SpecificationIdentity, string>
    val value: SpecificationIdentity -> string

type SpecificationProvenance =
    { Agent: string
      Session: string
      SourcePath: string
      SourceRevision: string
      AuthoredAtUtc: string }

type SpecificationDiagnostic =
    { Code: string
      Path: string
      Message: string }

type SpecificationModel<'ast> =
    { Identity: SpecificationIdentity
      SchemaVersion: int32
      Provenance: SpecificationProvenance
      Intent: string
      Ast: 'ast }

type SpecificationChange =
    { Path: string
      Summary: string
      BeforeFingerprint: string
      AfterFingerprint: string }

type SpecificationSemanticDiff =
    | Equivalent
    | Changed of SpecificationChange list

[<RequireQualifiedAccess>]
module SpecificationModel =
    val validateEnvelope: SpecificationModel<'ast> -> SpecificationDiagnostic list
    val normalize: encodeAst: ('ast -> byte array) -> SpecificationModel<'ast> -> Result<byte array, SpecificationDiagnostic list>
    val fingerprint: encodeAst: ('ast -> byte array) -> SpecificationModel<'ast> -> Result<string, SpecificationDiagnostic list>
    val semanticDiff: encodeAst: ('ast -> byte array) -> SpecificationModel<'ast> -> SpecificationModel<'ast> -> Result<SpecificationSemanticDiff, SpecificationDiagnostic list>
