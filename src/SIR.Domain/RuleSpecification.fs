namespace SIR.Domain

open System
open System.Text

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
        { DraftDefinition = None; Reads = []; Writes = [] }

    [<CustomOperation("definition")>]
    member _.Definition(state: RuleSpecificationDraft, definition: RuleDefinition) =
        { state with DraftDefinition = Some definition }

    [<CustomOperation("reads")>]
    member _.Reads(state: RuleSpecificationDraft, reads: string list) =
        { state with Reads = reads }

    [<CustomOperation("writes")>]
    member _.Writes(state: RuleSpecificationDraft, writes: string list) =
        { state with Writes = writes }

    member _.Run(state: RuleSpecificationDraft) =
        let definition =
            state.DraftDefinition
            |> Option.defaultWith (fun () -> invalidArg "definition" "A rule specification computation requires a definition operation.")

        { Identity = identity
          SchemaVersion = 1
          Provenance = provenance
          Intent = intent
          Ast = { Definition = definition; Reads = state.Reads; Writes = state.Writes } }

[<RequireQualifiedAccess>]
module RuleSpecification =
    let private diagnostic code path message =
        { Code = code; Path = path; Message = message }

    let hybrid identity provenance intent definition reads writes =
        { Identity = identity
          SchemaVersion = 1
          Provenance = provenance
          Intent = intent
          Ast = { Definition = definition; Reads = reads; Writes = writes } }

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
                Some(diagnostic "RULE-SPEC-SUBJECT-REQUIRED" (path + "/" + string index) "Operational subjects must be non-empty.")
            else
                None)

    let private semanticsKind = function
        | FactSemantics _ -> RuleKind.Fact
        | PredicateSemantics _ -> RuleKind.Predicate
        | FormulaSemantics _ -> RuleKind.Formula
        | TransitionSemantics _ -> RuleKind.Transition
        | AlgorithmSemantics _ -> RuleKind.Algorithm
        | NarrativeSemantics -> RuleKind.Narrative

    let validate model =
        let definition = model.Ast.Definition
        let metadata = definition.Metadata
        let source = metadata.RuleSource

        [ yield! SpecificationModel.validateEnvelope model

          if SpecificationIdentity.value model.Identity <> RuleId.value metadata.Id then
              yield diagnostic "RULE-SPEC-IDENTITY-MISMATCH" "/identity" "Specification identity must equal the compiled rule ID."

          if metadata.SemanticKind <> semanticsKind definition.Semantics then
              yield diagnostic "RULE-SPEC-KIND-MISMATCH" "/ast/definition/semantics" "Metadata semantic kind does not match the rule semantics case."

          match source with
          | None ->
              yield diagnostic "RULE-SPEC-SOURCE-REQUIRED" "/ast/definition/metadata/ruleSource" "A compiled rule specification requires an authoritative source binding."
          | Some binding ->
              if binding.RepositoryPath <> model.Provenance.SourcePath then
                  yield diagnostic "RULE-SPEC-SOURCE-PATH" "/provenance/sourcePath" "Provenance source path must equal the rule source path."
              if binding.Commit <> model.Provenance.SourceRevision then
                  yield diagnostic "RULE-SPEC-SOURCE-REVISION" "/provenance/sourceRevision" "Provenance source revision must equal the rule source revision."

          yield! blankSubjects "/ast/reads" model.Ast.Reads
          yield! blankSubjects "/ast/writes" model.Ast.Writes
          yield! duplicates "/ast/reads" model.Ast.Reads
          yield! duplicates "/ast/writes" model.Ast.Writes

          if List.isEmpty metadata.Evidence then
              yield diagnostic "RULE-SPEC-EVIDENCE-REQUIRED" "/ast/definition/metadata/evidence" "A rule specification requires at least one evidence binding."

          match definition.Semantics with
          | AlgorithmSemantics algorithm ->
              if String.IsNullOrWhiteSpace algorithm.ImplementationSymbol then
                  yield diagnostic "RULE-SPEC-ALGORITHM-SYMBOL" "/ast/definition/semantics/implementationSymbol" "Registered algorithms require an implementation symbol."
              if String.IsNullOrWhiteSpace algorithm.Fingerprint then
                  yield diagnostic "RULE-SPEC-ALGORITHM-FINGERPRINT" "/ast/definition/semantics/fingerprint" "Registered algorithms require an implementation fingerprint."
              if List.isEmpty algorithm.Inputs then
                  yield diagnostic "RULE-SPEC-ALGORITHM-INPUTS" "/ast/definition/semantics/inputs" "Registered algorithms require typed inputs."
              if String.IsNullOrWhiteSpace algorithm.ResultUnit then
                  yield diagnostic "RULE-SPEC-ALGORITHM-RESULT" "/ast/definition/semantics/resultUnit" "Registered algorithms require a result unit."
              if List.isEmpty algorithm.ExplanationFields then
                  yield diagnostic "RULE-SPEC-ALGORITHM-EXPLANATION" "/ast/definition/semantics/explanationFields" "Registered algorithms require explanation fields."
              if List.isEmpty model.Ast.Reads then
                  yield diagnostic "RULE-SPEC-ALGORITHM-READS" "/ast/reads" "Registered algorithms require explicit reads."
              if List.isEmpty model.Ast.Writes then
                  yield diagnostic "RULE-SPEC-ALGORITHM-WRITES" "/ast/writes" "Registered algorithms require explicit writes; use a named no-write token when the algorithm is pure."
          | _ -> () ]
        |> List.distinct
        |> List.sortBy (fun item -> item.Path, item.Code)

    let compile model =
        match validate model with
        | [] -> Ok model.Ast.Definition
        | diagnostics -> Error diagnostics

    let private text (value: string) =
        let bytes = Encoding.UTF8.GetBytes value
        CanonicalEncoding.concatenate [ CanonicalEncoding.int32LittleEndian bytes.Length; bytes ]

    let private list (values: string list) =
        CanonicalEncoding.concatenate
            (CanonicalEncoding.int32LittleEndian values.Length
             :: (values |> List.sort |> List.map text))

    let private encodeAst (ast: RuleSpecificationAst) =
        CanonicalEncoding.concatenate
            [ Rules.canonicalRuleBytes ast.Definition
              list ast.Reads
              list ast.Writes ]

    let normalizedBytes model =
        match validate model with
        | [] -> SpecificationModel.normalize encodeAst model
        | diagnostics -> Error diagnostics

    let fingerprint model =
        normalizedBytes model
        |> Result.map CanonicalHash.sha256
        |> Result.map (Array.map (fun (value: byte) -> value.ToString("x2")) >> String.concat "")

    let semanticDiff before after =
        let diagnostics = validate before @ validate after
        if List.isEmpty diagnostics then SpecificationModel.semanticDiff encodeAst before after
        else Error(diagnostics |> List.distinct |> List.sortBy (fun item -> item.Path, item.Code))

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

    let private hex bytes =
        bytes
        |> Array.map (fun (value: byte) -> value.ToString("x2"))
        |> String.concat ""

    let private projectionParts model =
        fingerprint model
        |> Result.map (fun sourceFingerprint ->
            let definition = model.Ast.Definition
            let metadata = definition.Metadata
            let dependencies =
                metadata.Dependencies
                |> List.map RuleId.value
                |> List.sort
                |> function
                    | [] -> "none"
                    | values -> String.concat ", " values

            let body =
                String.concat "\n"
                    [ "# " + metadata.Title
                      ""
                      "- Model: `" + SpecificationIdentity.value model.Identity + "`"
                      "- Schema: `" + string model.SchemaVersion + "`"
                      "- Rule: `" + RuleId.value metadata.Id + "` (`" + string metadata.SemanticKind + "`)"
                      "- Status: `" + string metadata.Status + "`"
                      "- Source: `" + model.Provenance.SourcePath + "@" + model.Provenance.SourceRevision + "`"
                      "- Dependencies: " + dependencies
                      "- Reads: " + (if List.isEmpty model.Ast.Reads then "none" else String.concat ", " model.Ast.Reads)
                      "- Writes: " + (if List.isEmpty model.Ast.Writes then "none" else String.concat ", " model.Ast.Writes)
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

            let generatedFingerprint = Encoding.UTF8.GetBytes body |> CanonicalHash.sha256 |> hex
            sourceFingerprint, generatedFingerprint, body)

    let markdownProjection model =
        projectionParts model
        |> Result.map (fun (sourceFingerprint, generatedFingerprint, body) ->
            String.concat "\n"
                [ "<!-- sir-rule-specification/v1 -->"
                  "<!-- source-fingerprint: " + sourceFingerprint + " -->"
                  "<!-- generated-fingerprint: " + generatedFingerprint + " -->"
                  body ])

    let projectionReceiptJson selectedSurface projectionPath model =
        projectionParts model
        |> Result.map (fun (sourceFingerprint, generatedFingerprint, _) ->
            "{\"schema\":\"sir-rule-specification-projection/v1\",\"identity\":\""
            + escapeJson (SpecificationIdentity.value model.Identity)
            + "\",\"schemaVersion\":" + string model.SchemaVersion
            + ",\"sourcePath\":\"" + escapeJson model.Provenance.SourcePath
            + "\",\"sourceRevision\":\"" + escapeJson model.Provenance.SourceRevision
            + "\",\"sourceFingerprint\":\"" + sourceFingerprint
            + "\",\"generatedFingerprint\":\"" + generatedFingerprint
            + "\",\"selectedSurface\":\"" + escapeJson selectedSurface
            + "\",\"projectionPath\":\"" + escapeJson projectionPath + "\"}")
