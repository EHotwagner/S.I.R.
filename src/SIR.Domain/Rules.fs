namespace SIR.Domain

open System
open System.Text

[<RequireQualifiedAccess>]
module Rules =
    let private bytes (value: string) = Encoding.UTF8.GetBytes value
    let private segment (value: byte array) = CanonicalEncoding.concatenate [ CanonicalEncoding.int32LittleEndian value.Length; value ]
    let private text value = value |> bytes |> segment
    let private list encode values = CanonicalEncoding.concatenate ([ CanonicalEncoding.int32LittleEndian (List.length values) ] @ (values |> List.map encode))
    let private boolByte value = CanonicalEncoding.byteValue (if value then 1uy else 0uy)
    let private kindCode = function Integer -> 0uy | FixedPoint -> 1uy | Boolean -> 2uy | Text -> 3uy
    let private statusCode = function Proposed -> 0uy | Prototype -> 1uy | Canonical -> 2uy | Deprecated -> 3uy | Superseded -> 4uy
    let private ruleKindCode = function Fact -> 0uy | Predicate -> 1uy | Formula -> 2uy | Transition -> 3uy | Algorithm -> 4uy | Narrative -> 5uy

    let private valueBytes value =
        let payload =
            match value.Value with
            | IntegerValue number -> CanonicalEncoding.int32LittleEndian number
            | FixedPointValue number -> CanonicalEncoding.fixedPoint number
            | BooleanValue flag -> boolByte flag
            | TextValue content -> text content
        CanonicalEncoding.concatenate
            [ CanonicalEncoding.byteValue (kindCode value.DataKind); text value.Unit; payload ]

    let private sameShape left right = left.DataKind = right.DataKind && left.Unit = right.Unit
    let private fixedBinary operation left right =
        if not (sameShape left right) then Error(UnitMismatch(left.Unit, right.Unit)) else
        match left.Value, right.Value with
        | FixedPointValue a, FixedPointValue b -> Ok { left with Value = FixedPointValue(operation a b) }
        | _ -> Error(TypeMismatch "The arithmetic operator requires FixedPoint values.")

    let private fixedMultiply left right =
        match left.Value, right.Value with
        | FixedPointValue a, FixedPointValue b ->
            let resultUnit =
                if left.Unit = "ratio" then right.Unit
                elif right.Unit = "ratio" then left.Unit
                elif left.Unit = right.Unit then left.Unit
                else ""
            if resultUnit = "" then Error(UnitMismatch(left.Unit, right.Unit))
            else Ok { DataKind = RuleValueKind.FixedPoint; Unit = resultUnit; Value = FixedPointValue(FixedPoint.multiplySaturating a b) }
        | _ -> Error(TypeMismatch "Multiplication requires FixedPoint values.")

    let rec evaluate inputs expression =
        let pair left right continuation =
            match evaluate inputs left, evaluate inputs right with Ok a, Ok b -> continuation a b | Error error, _ | _, Error error -> Error error
        match expression with
        | Constant value -> Ok value
        | Input(name, kind, unitName) ->
            match Map.tryFind name inputs with
            | None -> Error(MissingInput name)
            | Some value when value.DataKind <> kind -> Error(TypeMismatch name)
            | Some value when value.Unit <> unitName -> Error(UnitMismatch(unitName, value.Unit))
            | Some value -> Ok value
        | Add(left, right) -> pair left right (fixedBinary FixedPoint.addSaturating)
        | Subtract(left, right) -> pair left right (fixedBinary FixedPoint.subtractSaturating)
        | Multiply(left, right) -> pair left right fixedMultiply
        | Divide(left, right) ->
            pair left right (fun a b ->
                if not (sameShape a b) then Error(UnitMismatch(a.Unit, b.Unit)) else
                match a.Value, b.Value with
                | FixedPointValue numerator, FixedPointValue denominator when FixedPoint.raw denominator = 0 -> Error DivisionByZero
                | FixedPointValue numerator, FixedPointValue denominator ->
                    FixedPoint.fromRatio (FixedPoint.raw numerator) (FixedPoint.raw denominator)
                    |> Result.map (fun quotient -> { DataKind = RuleValueKind.FixedPoint; Unit = "ratio"; Value = FixedPointValue quotient })
                    |> Result.mapError (fun _ -> DivisionByZero)
                | _ -> Error(TypeMismatch "Division requires FixedPoint values."))
        | MinimumOf(left, right) | MaximumOf(left, right) as expression ->
            pair left right (fun a b ->
                if not (sameShape a b) then Error(UnitMismatch(a.Unit, b.Unit)) else
                match a.Value, b.Value with
                | FixedPointValue x, FixedPointValue y ->
                    let comparison = FixedPoint.compareByRaw x y
                    let chosen =
                        match expression with
                        | MinimumOf _ -> if comparison <= 0 then x else y
                        | _ -> if comparison >= 0 then x else y
                    Ok { a with Value = FixedPointValue chosen }
                | _ -> Error(TypeMismatch "Minimum/maximum require FixedPoint values."))
        | Clamp(minimum, maximum, value) ->
            evaluate inputs (MaximumOf(minimum, MinimumOf(maximum, value)))
        | LessThanOrEqual(left, right) ->
            pair left right (fun a b ->
                if not (sameShape a b) then Error(UnitMismatch(a.Unit, b.Unit)) else
                match a.Value, b.Value with
                | FixedPointValue x, FixedPointValue y -> Ok { DataKind = RuleValueKind.Boolean; Unit = "boolean"; Value = BooleanValue(FixedPoint.compareByRaw x y <= 0) }
                | IntegerValue x, IntegerValue y -> Ok { DataKind = RuleValueKind.Boolean; Unit = "boolean"; Value = BooleanValue(x <= y) }
                | _ -> Error(TypeMismatch "Comparison requires like numeric values."))
        | IfThenElse(condition, whenTrue, whenFalse) ->
            match evaluate inputs condition with
            | Ok { Value = BooleanValue true } -> evaluate inputs whenTrue
            | Ok { Value = BooleanValue false } -> evaluate inputs whenFalse
            | Ok _ -> Error(TypeMismatch "Conditional requires a Boolean condition.")
            | Error error -> Error error

    let rec private expressionBytes = function
        | Constant value -> CanonicalEncoding.concatenate [ [| 0uy |]; valueBytes value ]
        | Input(name, kind, unitName) -> CanonicalEncoding.concatenate [ [| 1uy; kindCode kind |]; text name; text unitName ]
        | Add(a,b) -> binary 2uy a b | Subtract(a,b) -> binary 3uy a b | Multiply(a,b) -> binary 4uy a b | Divide(a,b) -> binary 5uy a b
        | MinimumOf(a,b) -> binary 6uy a b | MaximumOf(a,b) -> binary 7uy a b | LessThanOrEqual(a,b) -> binary 9uy a b
        | Clamp(a,b,c) | IfThenElse(a,b,c) as expression ->
            let tag =
                match expression with
                | Clamp _ -> 8uy
                | _ -> 10uy
            CanonicalEncoding.concatenate [ [| tag |]; expressionBytes a; expressionBytes b; expressionBytes c ]
    and private binary tag left right = CanonicalEncoding.concatenate [ [| tag |]; expressionBytes left; expressionBytes right ]

    let private semanticsBytes = function
        | FactSemantics value -> CanonicalEncoding.concatenate [ [| 0uy |]; valueBytes value ]
        | PredicateSemantics expression -> CanonicalEncoding.concatenate [ [| 1uy |]; expressionBytes expression ]
        | FormulaSemantics(kind, unitName, expression) -> CanonicalEncoding.concatenate [ [| 2uy; kindCode kind |]; text unitName; expressionBytes expression ]
        | TransitionSemantics transition -> CanonicalEncoding.concatenate [ [| 3uy |]; text transition.Phase; list (RuleId.value >> text) transition.Preconditions; list text transition.Reads; list text transition.Effects; list text transition.Events ]
        | AlgorithmSemantics algorithm -> CanonicalEncoding.concatenate [ [| 4uy; kindCode algorithm.ResultKind |]; text algorithm.ImplementationSymbol; text algorithm.Fingerprint; text algorithm.ResultUnit; list text algorithm.ExplanationFields ]
        | NarrativeSemantics -> [| 5uy |]

    let canonicalRuleBytes rule =
        let metadata = rule.Metadata
        let trigger =
            match metadata.Statement.Trigger with
            | None -> [| 0uy |]
            | Some value -> CanonicalEncoding.concatenate [ [| 1uy |]; text value ]
        let source =
            match metadata.RuleSource with
            | None -> [| 0uy |]
            | Some value -> CanonicalEncoding.concatenate [ [| 1uy |]; text value.Symbol; text value.RepositoryPath; text value.Commit ]
        CanonicalEncoding.concatenate
            [ text (RuleId.value metadata.Id); text metadata.Title; [| statusCode metadata.Status; ruleKindCode metadata.SemanticKind |]
              list text metadata.Statement.Preconditions; trigger; text metadata.Statement.System; list text metadata.Statement.Responses; text metadata.Rationale
              metadata.Dependencies |> List.map RuleId.value |> List.sort |> list text
              metadata.Supersedes |> List.map RuleId.value |> List.sort |> list text
              source
              list text metadata.Examples; list text metadata.Properties; list text metadata.Evidence; semanticsBytes rule.Semantics ]

#if !SIR_WEB_CLIENT
    let validate rules =
        let ids = rules |> List.map (fun rule -> RuleId.value rule.Metadata.Id)
        let idSet = Set.ofList ids
        let duplicates = ids |> List.countBy id |> List.choose (fun (id, count) -> if count > 1 then Some(DuplicateRuleId id) else None)
        let valueMatches value =
            match value.DataKind, value.Value with
            | Integer, IntegerValue _ | FixedPoint, FixedPointValue _ | Boolean, BooleanValue _ | Text, TextValue _ -> true
            | _ -> false
        let rec expressionShape = function
            | Constant value when valueMatches value && not (String.IsNullOrWhiteSpace value.Unit) -> Ok(value.DataKind, value.Unit)
            | Constant _ -> Error "constant kind/unit"
            | Input(name, kind, unitName) when not (String.IsNullOrWhiteSpace name) && not (String.IsNullOrWhiteSpace unitName) -> Ok(kind, unitName)
            | Input _ -> Error "input name/unit"
            | Add(a,b) | Subtract(a,b) | MinimumOf(a,b) | MaximumOf(a,b) ->
                match expressionShape a, expressionShape b with Ok left, Ok right when left = right && fst left = FixedPoint -> Ok left | _ -> Error "like fixed-point operands"
            | Multiply(a,b) ->
                match expressionShape a, expressionShape b with
                | Ok(FixedPoint, "ratio"), Ok(FixedPoint, unitName) | Ok(FixedPoint, unitName), Ok(FixedPoint, "ratio") -> Ok(FixedPoint, unitName)
                | Ok(FixedPoint, left), Ok(FixedPoint, right) when left = right -> Ok(FixedPoint, left)
                | _ -> Error "compatible fixed-point multiply operands"
            | Divide(a,b) -> match expressionShape a, expressionShape b with Ok left, Ok right when left = right && fst left = FixedPoint -> Ok(FixedPoint, "ratio") | _ -> Error "like fixed-point divide operands"
            | Clamp(a,b,c) -> match expressionShape a, expressionShape b, expressionShape c with Ok x, Ok y, Ok z when x = y && y = z && fst x = FixedPoint -> Ok x | _ -> Error "like fixed-point clamp operands"
            | LessThanOrEqual(a,b) -> match expressionShape a, expressionShape b with Ok left, Ok right when left = right && (fst left = FixedPoint || fst left = RuleValueKind.Integer) -> Ok(RuleValueKind.Boolean, "boolean") | _ -> Error "like numeric comparison operands"
            | IfThenElse(c,t,f) -> match expressionShape c, expressionShape t, expressionShape f with Ok(RuleValueKind.Boolean,"boolean"), Ok left, Ok right when left = right -> Ok left | _ -> Error "Boolean condition and like branches"
        let errors =
            rules |> List.collect (fun rule ->
                let id = RuleId.value rule.Metadata.Id
                [ if String.IsNullOrWhiteSpace rule.Metadata.Title then yield IncompleteRuleMetadata(id, "title")
                  if String.IsNullOrWhiteSpace rule.Metadata.Rationale && rule.Metadata.SemanticKind <> Narrative then yield IncompleteRuleMetadata(id, "rationale")
                  if List.isEmpty rule.Metadata.Statement.Responses then yield IncompleteRuleMetadata(id, "statement.responses")
                  if rule.Metadata.SemanticKind <> Narrative && Option.isNone rule.Metadata.RuleSource then yield IncompleteRuleMetadata(id, "source")
                  if rule.Metadata.SemanticKind <> Narrative && List.isEmpty rule.Metadata.Evidence then yield IncompleteRuleMetadata(id, "evidence")
                  if rule.Metadata.SemanticKind <> Narrative && (List.isEmpty rule.Metadata.Examples || List.isEmpty rule.Metadata.Properties) then yield IncompleteRuleMetadata(id, "examples/properties")
                  if rule.Metadata.Status = Superseded && List.isEmpty rule.Metadata.Supersedes then yield IncompatibleRuleStatus id
                  match rule.Metadata.RuleSource with
                  | Some source when String.IsNullOrWhiteSpace source.Symbol || String.IsNullOrWhiteSpace source.RepositoryPath || source.RepositoryPath.StartsWith("/") || source.RepositoryPath.Contains("..") || source.Commit.Length <> 40 -> yield IncompleteRuleMetadata(id, "source.identity")
                  | _ -> ()
                  for dependency in rule.Metadata.Dependencies @ rule.Metadata.Supersedes do
                      if not (Set.contains (RuleId.value dependency) idSet) then yield DanglingRuleReference(id, RuleId.value dependency)
                  match rule.Metadata.SemanticKind, rule.Semantics with
                  | Fact, FactSemantics _ | Predicate, PredicateSemantics _ | Formula, FormulaSemantics _ | Transition, TransitionSemantics _ | Algorithm, AlgorithmSemantics _ | Narrative, NarrativeSemantics -> ()
                  | _ -> yield IncompatibleRuleKind id
                  match rule.Semantics with
                  | FactSemantics value when not (valueMatches value) || String.IsNullOrWhiteSpace value.Unit -> yield InvalidTypedValue(id, "fact")
                  | PredicateSemantics expression -> match expressionShape expression with Ok(Boolean, "boolean") -> () | verdict -> yield InvalidFormulaResult(id, sprintf "%A" verdict)
                  | FormulaSemantics(kind, unitName, expression) -> match expressionShape expression with Ok(actualKind, actualUnit) when actualKind = kind && actualUnit = unitName -> () | verdict -> yield InvalidFormulaResult(id, sprintf "%A" verdict)
                  | TransitionSemantics contract when String.IsNullOrWhiteSpace contract.Phase || List.isEmpty contract.Reads || List.isEmpty contract.Effects || List.isEmpty contract.Events -> yield IncompleteRuleMetadata(id, "transition.contract")
                  | AlgorithmSemantics contract ->
                      if String.IsNullOrWhiteSpace contract.ImplementationSymbol then yield InvalidAlgorithmContract(id, "implementationSymbol")
                      if String.IsNullOrWhiteSpace contract.Fingerprint then yield InvalidAlgorithmContract(id, "fingerprint")
                      if String.IsNullOrWhiteSpace contract.ResultUnit then yield InvalidAlgorithmContract(id, "resultUnit")
                      if List.isEmpty contract.Inputs then yield InvalidAlgorithmContract(id, "inputs")
                      if List.isEmpty contract.ExplanationFields then yield InvalidAlgorithmContract(id, "explanationFields")
                  | _ -> () ])
        match duplicates @ errors with [] -> Ok (rules |> List.sortBy (fun rule -> RuleId.value rule.Metadata.Id)) | failures -> Error failures
#endif

    let canonicalManifestPayload schemaVersion sourceCommit rules =
        let canonical = rules |> List.sortBy (fun rule -> RuleId.value rule.Metadata.Id) |> List.map canonicalRuleBytes
        CanonicalEncoding.concatenate ([ CanonicalEncoding.int32LittleEndian schemaVersion; text sourceCommit; CanonicalEncoding.int32LittleEndian canonical.Length ] @ canonical)

    let private canonicalSemanticPayload rules =
        rules
        |> List.sortBy (fun rule -> RuleId.value rule.Metadata.Id)
        |> List.map (fun rule ->
            CanonicalEncoding.concatenate
                [ text (RuleId.value rule.Metadata.Id)
                  CanonicalEncoding.byteValue (ruleKindCode rule.Metadata.SemanticKind)
                  rule.Metadata.Dependencies |> List.map RuleId.value |> List.sort |> list text
                  semanticsBytes rule.Semantics ])
        |> fun encoded -> CanonicalEncoding.concatenate ([ CanonicalEncoding.int32LittleEndian encoded.Length ] @ encoded)

    let private artifactBytes (name, digest) = CanonicalEncoding.concatenate [ text name; segment digest ]
    let packageIdentity engineIdentity compatibilityProfile packageVersion sourceCommit implementationArtifacts rules =
        let implementationDigest = implementationArtifacts |> List.sortBy fst |> List.map artifactBytes |> fun segments -> CanonicalEncoding.concatenate ([ text compatibilityProfile; text packageVersion ] @ segments) |> CanonicalHash.sha256
        let semanticPayload = CanonicalEncoding.concatenate [ segment implementationDigest; canonicalSemanticPayload rules ]
        let semanticDigest = CanonicalHash.sha256 semanticPayload
        let manifestPayload = CanonicalEncoding.concatenate [ text engineIdentity; text compatibilityProfile; text packageVersion; text sourceCommit; segment implementationDigest; segment semanticDigest; canonicalManifestPayload 1 sourceCommit rules ]
        { SchemaVersion = 1; EngineIdentity = engineIdentity; CompatibilityProfile = compatibilityProfile; PackageVersion = packageVersion; SourceCommit = sourceCommit; ImplementationDigest = implementationDigest; SemanticDigest = semanticDigest; ManifestDigest = CanonicalHash.sha256 manifestPayload }

#if !SIR_WEB_CLIENT
    let private jsonString (value: string) =
        "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n") + "\""

    let private jsonArray encode values = "[" + (values |> List.map encode |> String.concat ",") + "]"
    let private hex (bytes: byte array) = bytes |> Array.map (fun (value: byte) -> value.ToString("x2")) |> String.concat ""
    let private kindName = function Integer -> "integer" | FixedPoint -> "fixedPoint" | Boolean -> "boolean" | Text -> "text"
    let private statusName = function Proposed -> "proposed" | Prototype -> "prototype" | Canonical -> "canonical" | Deprecated -> "deprecated" | Superseded -> "superseded"
    let private ruleKindName = function Fact -> "fact" | Predicate -> "predicate" | Formula -> "formula" | Transition -> "transition" | Algorithm -> "algorithm" | Narrative -> "narrative"

    let private valueNotation value =
        match value.Value with
        | IntegerValue number -> string number
        | FixedPointValue number -> string (FixedPoint.raw number) + "/" + string FixedPoint.Scale
        | BooleanValue flag -> if flag then "true" else "false"
        | TextValue content -> jsonString content

    let rec formulaNotation = function
        | Constant value -> valueNotation value + " " + value.Unit
        | Input(name, _, unitName) -> name + ":" + unitName
        | Add(a, b) -> "(" + formulaNotation a + " + " + formulaNotation b + ")"
        | Subtract(a, b) -> "(" + formulaNotation a + " - " + formulaNotation b + ")"
        | Multiply(a, b) -> "(" + formulaNotation a + " × " + formulaNotation b + ")"
        | Divide(a, b) -> "(" + formulaNotation a + " / " + formulaNotation b + ")"
        | MinimumOf(a, b) -> "min(" + formulaNotation a + ", " + formulaNotation b + ")"
        | MaximumOf(a, b) -> "max(" + formulaNotation a + ", " + formulaNotation b + ")"
        | Clamp(a, b, c) -> "clamp(" + formulaNotation a + ", " + formulaNotation b + ", " + formulaNotation c + ")"
        | LessThanOrEqual(a, b) -> "(" + formulaNotation a + " <= " + formulaNotation b + ")"
        | IfThenElse(a, b, c) -> "if " + formulaNotation a + " then " + formulaNotation b + " else " + formulaNotation c

    let private semanticsProjection = function
        | FactSemantics value -> "{\"type\":\"fact\",\"value\":" + jsonString (valueNotation value) + ",\"kind\":" + jsonString (kindName value.DataKind) + ",\"unit\":" + jsonString value.Unit + "}"
        | PredicateSemantics expression -> "{\"type\":\"predicate\",\"notation\":" + jsonString (formulaNotation expression) + "}"
        | FormulaSemantics(kind, unitName, expression) -> "{\"type\":\"formula\",\"kind\":" + jsonString (kindName kind) + ",\"unit\":" + jsonString unitName + ",\"notation\":" + jsonString (formulaNotation expression) + "}"
        | TransitionSemantics contract -> "{\"type\":\"transition\",\"phase\":" + jsonString contract.Phase + ",\"preconditions\":" + jsonArray (RuleId.value >> jsonString) contract.Preconditions + ",\"reads\":" + jsonArray jsonString contract.Reads + ",\"effects\":" + jsonArray jsonString contract.Effects + ",\"events\":" + jsonArray jsonString contract.Events + "}"
        | AlgorithmSemantics contract ->
            let inputJson (name, kind, unitName) = "{\"name\":" + jsonString name + ",\"kind\":" + jsonString (kindName kind) + ",\"unit\":" + jsonString unitName + "}"
            "{\"type\":\"algorithm\",\"symbol\":" + jsonString contract.ImplementationSymbol + ",\"fingerprint\":" + jsonString contract.Fingerprint + ",\"inputs\":" + jsonArray inputJson contract.Inputs + ",\"resultKind\":" + jsonString (kindName contract.ResultKind) + ",\"resultUnit\":" + jsonString contract.ResultUnit + ",\"explanationFields\":" + jsonArray jsonString contract.ExplanationFields + "}"
        | NarrativeSemantics -> "{\"type\":\"narrative\"}"

    let manifestJson identity rules =
        let sourceJson = function
            | None -> "null"
            | Some source -> "{\"symbol\":" + jsonString source.Symbol + ",\"path\":" + jsonString source.RepositoryPath + ",\"commit\":" + jsonString source.Commit + "}"
        let ruleJson rule =
            let metadata = rule.Metadata
            let statement = "{\"preconditions\":" + jsonArray jsonString metadata.Statement.Preconditions + ",\"trigger\":" + (metadata.Statement.Trigger |> Option.map jsonString |> Option.defaultValue "null") + ",\"system\":" + jsonString metadata.Statement.System + ",\"responses\":" + jsonArray jsonString metadata.Statement.Responses + "}"
            "{\"id\":" + jsonString (RuleId.value metadata.Id) + ",\"title\":" + jsonString metadata.Title + ",\"status\":" + jsonString (statusName metadata.Status) + ",\"kind\":" + jsonString (ruleKindName metadata.SemanticKind) + ",\"statement\":" + statement + ",\"rationale\":" + jsonString metadata.Rationale + ",\"dependencies\":" + jsonArray (RuleId.value >> jsonString) (metadata.Dependencies |> List.sortBy RuleId.value) + ",\"supersedes\":" + jsonArray (RuleId.value >> jsonString) (metadata.Supersedes |> List.sortBy RuleId.value) + ",\"examples\":" + jsonArray jsonString metadata.Examples + ",\"properties\":" + jsonArray jsonString metadata.Properties + ",\"evidence\":" + jsonArray jsonString metadata.Evidence + ",\"source\":" + sourceJson metadata.RuleSource + ",\"explanationVocabulary\":[\"operands\",\"outcome\",\"children\",\"eventId\"],\"semantics\":" + semanticsProjection rule.Semantics + "}"
        "{\"schemaVersion\":" + string identity.SchemaVersion + ",\"engineIdentity\":" + jsonString identity.EngineIdentity + ",\"compatibilityProfile\":" + jsonString identity.CompatibilityProfile + ",\"packageVersion\":" + jsonString identity.PackageVersion + ",\"sourceCommit\":" + jsonString identity.SourceCommit + ",\"implementationDigest\":" + jsonString (hex identity.ImplementationDigest) + ",\"semanticDigest\":" + jsonString (hex identity.SemanticDigest) + ",\"manifestDigest\":" + jsonString (hex identity.ManifestDigest) + ",\"rules\":" + jsonArray ruleJson (rules |> List.sortBy (fun rule -> RuleId.value rule.Metadata.Id)) + "}"

    let coverageJson identity rules =
        let node kind identity authority = "{\"kind\":" + jsonString kind + ",\"identity\":" + jsonString identity + ",\"authority\":" + jsonString authority + "}"
        let edge rule target kind = "{\"from\":" + jsonString ("rule:" + RuleId.value rule.Metadata.Id) + ",\"to\":" + jsonString target + ",\"kind\":" + jsonString kind + "}"
        let sortedRules = rules |> List.sortBy (fun rule -> RuleId.value rule.Metadata.Id)
        let nodes =
            sortedRules
            |> List.collect (fun rule ->
                let id = RuleId.value rule.Metadata.Id
                let sourceIdentity = rule.Metadata.RuleSource |> Option.map (fun source -> source.RepositoryPath + "#" + source.Symbol) |> Option.defaultValue "unresolved"
                [ node "rule" id "Corpus"
                  node "implementation" sourceIdentity "Corpus"
                  node "event" (id + ":application") "Corpus"
                  node "explanation" (id + ":derivation") "Corpus"
                  for example in rule.Metadata.Examples do node "example/property" example "Corpus"
                  for property in rule.Metadata.Properties do node "example/property" property "Corpus"
                  node "documentation" ("rules/" + id) "Corpus"
                  node "source" sourceIdentity "Corpus"
                  node "replay" "tests/fixtures/rules-corpus/v1" "Corpus" ])
            |> List.distinct
            |> List.sort
        let edges =
            sortedRules |> List.collect (fun rule ->
                let id = RuleId.value rule.Metadata.Id
                let sourceIdentity = rule.Metadata.RuleSource |> Option.map (fun source -> source.RepositoryPath + "#" + source.Symbol) |> Option.defaultValue "unresolved"
                [ for dependency in rule.Metadata.Dependencies do yield edge rule (RuleId.value dependency) "dependency"
                  yield edge rule sourceIdentity "implementation"
                  yield edge rule (id + ":application") "event/application"
                  yield edge rule (id + ":derivation") "explanation"
                  for example in rule.Metadata.Examples do yield edge rule example "example"
                  for property in rule.Metadata.Properties do yield edge rule property "property"
                  yield edge rule ("rules/" + id) "documentation"
                  yield edge rule sourceIdentity "source"
                  yield edge rule "tests/fixtures/rules-corpus/v1" "replay" ])
            |> List.sort
        "{\"schemaVersion\":1,\"packageManifestDigest\":" + jsonString (hex identity.ManifestDigest) + ",\"authorityBoundary\":{\"migrated\":\"first-combat-vertical-slice\",\"outside\":\"legacy\"},\"nodes\":[" + String.concat "," nodes + "],\"edges\":[" + String.concat "," edges + "]}"
#endif

    let rec canonicalApplicationBytes application =
        CanonicalEncoding.concatenate
            [ text application.ApplicationId; text (RuleId.value application.RuleId); list (fun (name, value) -> CanonicalEncoding.concatenate [ text name; valueBytes value ]) application.Operands; valueBytes application.Outcome; list canonicalApplicationBytes application.Children; text application.EventId; segment application.PackageManifestDigest ]
