namespace SIR.Domain

open System
open System.Text
#if !FABLE_COMPILER
open System.Text.Json
#endif
open FS.GG.SDD.Artifacts.TypedSpecifications

type RuleSpecificationAst =
    { Definition: RuleDefinition
      Reads: string list
      Writes: string list }

type RegisteredAlgorithmSpecification =
    { ImplementationSymbol: string
      ImplementationFingerprint: string
      Inputs: (string * RuleValueKind * string) list
      ResultKind: RuleValueKind
      ResultUnit: string
      Reads: string list
      Writes: string list
      Evidence: string list
      ExplanationFields: string list }

type RuleSpecificationDraft =
    private
        { DraftDefinition: RuleDefinition option
          Reads: string list
          Writes: string list }

[<Sealed>]
type RuleSpecificationBuilder(identity, provenance, intent) =
    member _.Yield(value: unit) : RuleSpecificationDraft =
        { DraftDefinition = None
          Reads = []
          Writes = [] }

    [<CustomOperation("definition")>]
    member _.Definition(state: RuleSpecificationDraft, definition: RuleDefinition) =
        { state with
            DraftDefinition = Some definition }

    [<CustomOperation("reads")>]
    member _.Reads(state: RuleSpecificationDraft, reads: string list) = { state with Reads = reads }

    [<CustomOperation("writes")>]
    member _.Writes(state: RuleSpecificationDraft, writes: string list) = { state with Writes = writes }

    member _.Run(state: RuleSpecificationDraft) : SpecificationModel<RuleSpecificationAst> =
        let definition =
            state.DraftDefinition
            |> Option.defaultWith (fun () ->
                invalidArg "definition" "A rule specification computation requires a definition operation.")

        { Identity = identity
          SchemaVersion = 1
          Provenance = provenance
          Intent = intent
          EvidenceObligations = []
          Extension =
            { Definition = definition
              Reads = state.Reads
              Writes = state.Writes } }

[<RequireQualifiedAccess>]
module RuleSpecification =
    let private diagnostic code path message : SpecificationDiagnostic =
        { Code = code
          Path = path
          Message = message
          Location = None }

    let hybrid identity provenance intent definition reads writes : SpecificationModel<RuleSpecificationAst> =
        { Identity = identity
          SchemaVersion = 1
          Provenance = provenance
          Intent = intent
          EvidenceObligations = []
          Extension =
            { Definition = definition
              Reads = reads
              Writes = writes } }

    let computation identity provenance intent =
        RuleSpecificationBuilder(identity, provenance, intent)

    let private duplicates path values =
        values
        |> List.countBy id
        |> List.choose (fun (value, count) ->
            if count > 1 then
                Some(diagnostic "RULE-SPEC-DUPLICATE" path ("Duplicate operational subject: " + value))
            else
                None)

    let private blankSubjects path values =
        values
        |> List.mapi (fun index value -> index, value)
        |> List.choose (fun (index, value) ->
            if String.IsNullOrWhiteSpace value then
                Some(
                    diagnostic
                        "RULE-SPEC-SUBJECT-REQUIRED"
                        (path + "/" + string index)
                        "Operational subjects must be non-empty."
                )
            else
                None)

    let private semanticsKind =
        function
        | FactSemantics _ -> RuleKind.Fact
        | PredicateSemantics _ -> RuleKind.Predicate
        | FormulaSemantics _ -> RuleKind.Formula
        | TransitionSemantics _ -> RuleKind.Transition
        | AlgorithmSemantics _ -> RuleKind.Algorithm
        | NarrativeSemantics -> RuleKind.Narrative

    let private extensionDiagnostics _ (ast: RuleSpecificationAst) =
        let definition = ast.Definition
        let metadata = definition.Metadata

        [ if metadata.SemanticKind <> semanticsKind definition.Semantics then
              yield
                  diagnostic
                      "RULE-SPEC-KIND-MISMATCH"
                      "/extension/definition/semantics"
                      "Metadata semantic kind does not match the rule semantics case."

          if Option.isNone metadata.RuleSource then
              yield
                  diagnostic
                      "RULE-SPEC-SOURCE-REQUIRED"
                      "/extension/definition/metadata/ruleSource"
                      "A compiled rule specification requires an authoritative source binding."

          yield! blankSubjects "/extension/reads" ast.Reads
          yield! blankSubjects "/extension/writes" ast.Writes
          yield! duplicates "/extension/reads" ast.Reads
          yield! duplicates "/extension/writes" ast.Writes

          if List.isEmpty metadata.Evidence then
              yield
                  diagnostic
                      "RULE-SPEC-EVIDENCE-REQUIRED"
                      "/extension/definition/metadata/evidence"
                      "A rule specification requires at least one evidence binding."

          match definition.Semantics with
          | AlgorithmSemantics algorithm ->
              if String.IsNullOrWhiteSpace algorithm.ImplementationSymbol then
                  yield
                      diagnostic
                          "RULE-SPEC-ALGORITHM-SYMBOL"
                          "/extension/definition/semantics/implementationSymbol"
                          "Registered algorithms require an implementation symbol."

              if String.IsNullOrWhiteSpace algorithm.Fingerprint then
                  yield
                      diagnostic
                          "RULE-SPEC-ALGORITHM-FINGERPRINT"
                          "/extension/definition/semantics/fingerprint"
                          "Registered algorithms require an implementation fingerprint."

              if List.isEmpty algorithm.Inputs then
                  yield
                      diagnostic
                          "RULE-SPEC-ALGORITHM-INPUTS"
                          "/extension/definition/semantics/inputs"
                          "Registered algorithms require typed inputs."

              if String.IsNullOrWhiteSpace algorithm.ResultUnit then
                  yield
                      diagnostic
                          "RULE-SPEC-ALGORITHM-RESULT"
                          "/extension/definition/semantics/resultUnit"
                          "Registered algorithms require a result unit."

              if List.isEmpty algorithm.ExplanationFields then
                  yield
                      diagnostic
                          "RULE-SPEC-ALGORITHM-EXPLANATION"
                          "/extension/definition/semantics/explanationFields"
                          "Registered algorithms require explanation fields."

              if List.isEmpty ast.Reads then
                  yield
                      diagnostic
                          "RULE-SPEC-ALGORITHM-READS"
                          "/extension/reads"
                          "Registered algorithms require explicit reads."

              if List.isEmpty ast.Writes then
                  yield
                      diagnostic
                          "RULE-SPEC-ALGORITHM-WRITES"
                          "/extension/writes"
                          "Registered algorithms require explicit writes; use a named no-write token when the algorithm is pure."
          | _ -> () ]

    let private text (value: string) =
        let bytes = Encoding.UTF8.GetBytes value
        CanonicalEncoding.concatenate [ CanonicalEncoding.int32LittleEndian bytes.Length; bytes ]

    let private list (values: string list) =
        CanonicalEncoding.concatenate (
            CanonicalEncoding.int32LittleEndian values.Length
            :: (values |> List.sort |> List.map text)
        )

    let private encodeAst (ast: RuleSpecificationAst) =
        CanonicalEncoding.concatenate [ Rules.canonicalRuleBytes ast.Definition; list ast.Reads; list ast.Writes ]

    let private hex bytes =
        bytes
        |> Array.map (fun (value: byte) -> value.ToString("x2"))
        |> String.concat ""

#if !FABLE_COMPILER
    let private writeStrings (writer: Utf8JsonWriter) (name: string) (values: string list) =
        writer.WriteStartArray(name)

        values
        |> List.sort
        |> List.iter (fun value -> writer.WriteStringValue(value: string))

        writer.WriteEndArray()

    let private writeExtension (writer: Utf8JsonWriter) (ast: RuleSpecificationAst) =
        writer.WriteStartObject()
        writer.WriteString("ruleId", RuleId.value ast.Definition.Metadata.Id)
        writer.WriteString("canonicalRule", Rules.canonicalRuleBytes ast.Definition |> hex)
        writeStrings writer "reads" ast.Reads
        writeStrings writer "writes" ast.Writes
        writer.WriteEndObject()

    let private decodeExtension (_: JsonElement) =
        Error
            [ diagnostic
                  "RULE-SPEC-CODEC-AUTHORED-SOURCE-REQUIRED"
                  "/extension"
                  "Generated rule projections are receipts; authoritative RuleDefinition values must be authored and compiled from F#." ]
#endif

    let private extensionMarkdown (ast: RuleSpecificationAst) =
        let metadata = ast.Definition.Metadata

        [ "## Rule"
          ""
          "- Rule: `"
          + RuleId.value metadata.Id
          + "` (`"
          + string metadata.SemanticKind
          + "`)"
          "- Reads: "
          + (if List.isEmpty ast.Reads then
                 "none"
             else
                 String.concat ", " ast.Reads)
          "- Writes: "
          + (if List.isEmpty ast.Writes then
                 "none"
             else
                 String.concat ", " ast.Writes)
          ""
          metadata.Statement.System + " " + String.concat " " metadata.Statement.Responses ]

    let contract: ExtensionContract<RuleSpecificationAst> =
        { Kind = "sir-rule-specification"
          SchemaVersion = 1
          Validate = extensionDiagnostics
          EncodeCanonical = encodeAst
#if !FABLE_COMPILER
          WriteJson = writeExtension
          DecodeJson = decodeExtension
#endif
          ProjectMarkdown = extensionMarkdown }

    let validate (model: SpecificationModel<RuleSpecificationAst>) =
        let metadata = model.Extension.Definition.Metadata

        [ yield! SpecificationCompiler.validate contract model

          if SpecificationId.value model.Identity <> RuleId.value metadata.Id then
              yield
                  diagnostic
                      "RULE-SPEC-IDENTITY-MISMATCH"
                      "/identity"
                      "Specification identity must equal the compiled rule ID."

          match metadata.RuleSource with
          | None -> ()
          | Some binding ->
              if binding.RepositoryPath <> model.Provenance.SourcePath then
                  yield
                      diagnostic
                          "RULE-SPEC-SOURCE-PATH"
                          "/provenance/sourcePath"
                          "Provenance source path must equal the rule source path."

              if binding.Commit <> model.Provenance.SourceRevision then
                  yield
                      diagnostic
                          "RULE-SPEC-SOURCE-REVISION"
                          "/provenance/sourceRevision"
                          "Provenance source revision must equal the rule source revision." ]
        |> List.distinct
        |> List.sortBy (fun item -> item.Path, item.Code, item.Message)

    let compile model =
        match validate model with
        | [] -> Ok model.Extension.Definition
        | diagnostics -> Error diagnostics

    let normalizedBytes model =
        match validate model with
        | [] -> SpecificationCompiler.normalize contract model
        | diagnostics -> Error diagnostics

    let fingerprint model =
        match validate model with
        | [] -> SpecificationCompiler.fingerprint contract model
        | diagnostics -> Error diagnostics

    let semanticDiff before after =
        let diagnostics = validate before @ validate after

        if List.isEmpty diagnostics then
            SpecificationCompiler.semanticDiff contract before after
        else
            Error(
                diagnostics
                |> List.distinct
                |> List.sortBy (fun item -> item.Path, item.Code, item.Message)
            )

    let tryRegisteredAlgorithm ast =
        match ast.Definition.Semantics with
        | AlgorithmSemantics algorithm ->
            Some
                { ImplementationSymbol = algorithm.ImplementationSymbol
                  ImplementationFingerprint = algorithm.Fingerprint
                  Inputs = algorithm.Inputs
                  ResultKind = algorithm.ResultKind
                  ResultUnit = algorithm.ResultUnit
                  Reads = ast.Reads
                  Writes = ast.Writes
                  Evidence = ast.Definition.Metadata.Evidence
                  ExplanationFields = algorithm.ExplanationFields }
        | _ -> None

    let private escapeJson (value: string) =
        value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n")

    let private projectionParts model =
        fingerprint model
        |> Result.map (fun sourceFingerprint ->
            let ast = model.Extension
            let metadata = ast.Definition.Metadata

            let dependencies =
                metadata.Dependencies
                |> List.map RuleId.value
                |> List.sort
                |> function
                    | [] -> "none"
                    | values -> String.concat ", " values

            let body =
                String.concat
                    "\n"
                    [ "# " + metadata.Title
                      ""
                      "- Model: `" + SpecificationId.value model.Identity + "`"
                      "- Schema: `" + string model.SchemaVersion + "`"
                      "- Extension: `" + contract.Kind + "/" + string contract.SchemaVersion + "`"
                      "- Rule: `"
                      + RuleId.value metadata.Id
                      + "` (`"
                      + string metadata.SemanticKind
                      + "`)"
                      "- Status: `" + string metadata.Status + "`"
                      "- Source: `"
                      + model.Provenance.SourcePath
                      + "@"
                      + model.Provenance.SourceRevision
                      + "`"
                      "- Dependencies: " + dependencies
                      "- Reads: "
                      + (if List.isEmpty ast.Reads then
                             "none"
                         else
                             String.concat ", " ast.Reads)
                      "- Writes: "
                      + (if List.isEmpty ast.Writes then
                             "none"
                         else
                             String.concat ", " ast.Writes)
                      ""
                      "## Controlled statement"
                      ""
                      metadata.Statement.System + " " + String.concat " " metadata.Statement.Responses
                      ""
                      "## Rationale"
                      ""
                      metadata.Rationale
                      ""
                      "## Semantic fingerprint"
                      ""
                      "`" + sourceFingerprint + "`" ]

            let generatedFingerprint =
                Encoding.UTF8.GetBytes body |> CanonicalHash.sha256 |> hex

            sourceFingerprint, generatedFingerprint, body)

    let markdownProjection model =
        projectionParts model
        |> Result.map (fun (sourceFingerprint, generatedFingerprint, body) ->
            String.concat
                "\n"
                [ "<!-- fsgg-typed-specification/v1 -->"
                  "<!-- extension: "
                  + contract.Kind
                  + "/"
                  + string contract.SchemaVersion
                  + " -->"
                  "<!-- source-fingerprint: " + sourceFingerprint + " -->"
                  "<!-- generated-fingerprint: " + generatedFingerprint + " -->"
                  body ])

    let projectionReceiptJson selectedSurface projectionPath model =
        projectionParts model
        |> Result.map (fun (sourceFingerprint, generatedFingerprint, _) ->
            "{\"schema\":\"sir-rule-specification-projection/v2\",\"kernelPackage\":\"FS.GG.SDD.Artifacts@1.3.0-preview.3\",\"extension\":\""
            + contract.Kind
            + "/"
            + string contract.SchemaVersion
            + "\",\"identity\":\""
            + escapeJson (SpecificationId.value model.Identity)
            + "\",\"schemaVersion\":"
            + string model.SchemaVersion
            + ",\"sourcePath\":\""
            + escapeJson model.Provenance.SourcePath
            + "\",\"sourceRevision\":\""
            + escapeJson model.Provenance.SourceRevision
            + "\",\"sourceFingerprint\":\""
            + sourceFingerprint
            + "\",\"generatedFingerprint\":\""
            + generatedFingerprint
            + "\",\"selectedSurface\":\""
            + escapeJson selectedSurface
            + "\",\"projectionPath\":\""
            + escapeJson projectionPath
            + "\"}")
