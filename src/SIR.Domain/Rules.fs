namespace SIR.Domain

open System
open System.Text

[<Struct>]
type RuleId = private RuleId of value: string

[<RequireQualifiedAccess>]
module RuleId =
    let create value =
        let validCharacter character = (character >= 'A' && character <= 'Z') || (character >= '0' && character <= '9') || character = '-'
        if String.IsNullOrWhiteSpace value || value.Length < 5 || value |> Seq.exists (validCharacter >> not) then Error "Rule IDs use non-empty uppercase ASCII letters, digits, and hyphens."
        elif value.StartsWith("-") || value.EndsWith("-") || value.Contains("--") then Error "Rule IDs cannot begin/end with or repeat a hyphen."
        else Ok(RuleId value)

    let value (RuleId value) = value

type RuleStatus = Proposed | Prototype | Canonical | Deprecated | Superseded
type RuleKind = Fact | Predicate | Formula | Transition | Algorithm | Narrative
type ControlledStatement = { Preconditions: string list; Trigger: string option; System: string; Responses: string list }
type SourceRef = { Symbol: string; RepositoryPath: string; Commit: string }
type RuleMetadata = { Id: RuleId; Title: string; Status: RuleStatus; SemanticKind: RuleKind; Statement: ControlledStatement; Rationale: string; Dependencies: RuleId list; Supersedes: RuleId list; RuleSource: SourceRef option; Examples: string list; Properties: string list; Evidence: string list }
type RuleValueKind = Integer | FixedPoint | Boolean | Text
type TypedValue = { DataKind: RuleValueKind; Unit: string; Value: RuleValue }
and RuleValue = IntegerValue of int32 | FixedPointValue of FixedPoint | BooleanValue of bool | TextValue of string
type FormulaExpr = Constant of TypedValue | Input of name: string * kind: RuleValueKind * unitName: string | Add of FormulaExpr * FormulaExpr | Subtract of FormulaExpr * FormulaExpr | Multiply of FormulaExpr * FormulaExpr | Divide of FormulaExpr * FormulaExpr | MinimumOf of FormulaExpr * FormulaExpr | MaximumOf of FormulaExpr * FormulaExpr | Clamp of minimum: FormulaExpr * maximum: FormulaExpr * value: FormulaExpr | LessThanOrEqual of FormulaExpr * FormulaExpr | IfThenElse of condition: FormulaExpr * whenTrue: FormulaExpr * whenFalse: FormulaExpr
type TransitionContract = { Phase: string; Preconditions: RuleId list; Reads: string list; Effects: string list; Events: string list }
type AlgorithmContract = { ImplementationSymbol: string; Fingerprint: string; Inputs: (string * RuleValueKind * string) list; ResultKind: RuleValueKind; ResultUnit: string; ExplanationFields: string list }
type RuleSemantics = FactSemantics of TypedValue | PredicateSemantics of FormulaExpr | FormulaSemantics of resultKind: RuleValueKind * resultUnit: string * FormulaExpr | TransitionSemantics of TransitionContract | AlgorithmSemantics of AlgorithmContract | NarrativeSemantics
type RuleDefinition = { Metadata: RuleMetadata; Semantics: RuleSemantics }
type RuleApplication = { ApplicationId: string; RuleId: RuleId; Operands: (string * TypedValue) list; Outcome: TypedValue; Children: RuleApplication list; EventId: string; PackageManifestDigest: byte array }
type RegistryError = DuplicateRuleId of string | DanglingRuleReference of owner: string * target: string | IncompleteRuleMetadata of ruleId: string * field: string | IncompatibleRuleKind of ruleId: string
type EvaluationError = MissingInput of string | TypeMismatch of string | UnitMismatch of string * string | DivisionByZero | InvalidExpression of string
type RulePackageIdentity = { SchemaVersion: int32; EngineIdentity: string; CompatibilityProfile: string; PackageVersion: string; SourceCommit: string; ImplementationDigest: byte array; SemanticDigest: byte array; ManifestDigest: byte array }

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

    let validate rules =
        let ids = rules |> List.map (fun rule -> RuleId.value rule.Metadata.Id)
        let idSet = Set.ofList ids
        let duplicates = ids |> List.countBy id |> List.choose (fun (id, count) -> if count > 1 then Some(DuplicateRuleId id) else None)
        let errors =
            rules |> List.collect (fun rule ->
                let id = RuleId.value rule.Metadata.Id
                [ if String.IsNullOrWhiteSpace rule.Metadata.Title then yield IncompleteRuleMetadata(id, "title")
                  if String.IsNullOrWhiteSpace rule.Metadata.Rationale && rule.Metadata.SemanticKind <> Narrative then yield IncompleteRuleMetadata(id, "rationale")
                  if List.isEmpty rule.Metadata.Statement.Responses then yield IncompleteRuleMetadata(id, "statement.responses")
                  if rule.Metadata.SemanticKind <> Narrative && Option.isNone rule.Metadata.RuleSource then yield IncompleteRuleMetadata(id, "source")
                  if rule.Metadata.SemanticKind <> Narrative && List.isEmpty rule.Metadata.Evidence then yield IncompleteRuleMetadata(id, "evidence")
                  for dependency in rule.Metadata.Dependencies @ rule.Metadata.Supersedes do
                      if not (Set.contains (RuleId.value dependency) idSet) then yield DanglingRuleReference(id, RuleId.value dependency)
                  match rule.Metadata.SemanticKind, rule.Semantics with
                  | Fact, FactSemantics _ | Predicate, PredicateSemantics _ | Formula, FormulaSemantics _ | Transition, TransitionSemantics _ | Algorithm, AlgorithmSemantics _ | Narrative, NarrativeSemantics -> ()
                  | _ -> yield IncompatibleRuleKind id ])
        match duplicates @ errors with [] -> Ok (rules |> List.sortBy (fun rule -> RuleId.value rule.Metadata.Id)) | failures -> Error failures

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
        | TransitionSemantics contract -> "{\"type\":\"transition\",\"phase\":" + jsonString contract.Phase + ",\"reads\":" + jsonArray jsonString contract.Reads + ",\"effects\":" + jsonArray jsonString contract.Effects + ",\"events\":" + jsonArray jsonString contract.Events + "}"
        | AlgorithmSemantics contract -> "{\"type\":\"algorithm\",\"symbol\":" + jsonString contract.ImplementationSymbol + ",\"fingerprint\":" + jsonString contract.Fingerprint + ",\"resultKind\":" + jsonString (kindName contract.ResultKind) + ",\"resultUnit\":" + jsonString contract.ResultUnit + "}"
        | NarrativeSemantics -> "{\"type\":\"narrative\"}"

    let manifestJson identity rules =
        let sourceJson = function
            | None -> "null"
            | Some source -> "{\"symbol\":" + jsonString source.Symbol + ",\"path\":" + jsonString source.RepositoryPath + ",\"commit\":" + jsonString source.Commit + "}"
        let ruleJson rule =
            let metadata = rule.Metadata
            "{\"id\":" + jsonString (RuleId.value metadata.Id) + ",\"title\":" + jsonString metadata.Title + ",\"status\":" + jsonString (statusName metadata.Status) + ",\"kind\":" + jsonString (ruleKindName metadata.SemanticKind) + ",\"rationale\":" + jsonString metadata.Rationale + ",\"dependencies\":" + jsonArray (RuleId.value >> jsonString) (metadata.Dependencies |> List.sortBy RuleId.value) + ",\"examples\":" + jsonArray jsonString metadata.Examples + ",\"properties\":" + jsonArray jsonString metadata.Properties + ",\"evidence\":" + jsonArray jsonString metadata.Evidence + ",\"source\":" + sourceJson metadata.RuleSource + ",\"semantics\":" + semanticsProjection rule.Semantics + "}"
        "{\"schemaVersion\":" + string identity.SchemaVersion + ",\"engineIdentity\":" + jsonString identity.EngineIdentity + ",\"compatibilityProfile\":" + jsonString identity.CompatibilityProfile + ",\"packageVersion\":" + jsonString identity.PackageVersion + ",\"sourceCommit\":" + jsonString identity.SourceCommit + ",\"implementationDigest\":" + jsonString (hex identity.ImplementationDigest) + ",\"semanticDigest\":" + jsonString (hex identity.SemanticDigest) + ",\"manifestDigest\":" + jsonString (hex identity.ManifestDigest) + ",\"rules\":" + jsonArray ruleJson (rules |> List.sortBy (fun rule -> RuleId.value rule.Metadata.Id)) + "}"

    let coverageJson identity rules =
        let edge rule target kind = "{\"ruleId\":" + jsonString (RuleId.value rule.Metadata.Id) + ",\"target\":" + jsonString target + ",\"kind\":" + jsonString kind + "}"
        let edges =
            rules |> List.sortBy (fun rule -> RuleId.value rule.Metadata.Id) |> List.collect (fun rule ->
                [ for dependency in rule.Metadata.Dependencies do yield edge rule (RuleId.value dependency) "dependency"
                  for example in rule.Metadata.Examples do yield edge rule example "example"
                  for evidence in rule.Metadata.Evidence do yield edge rule evidence "evidence"
                  match rule.Metadata.RuleSource with Some source -> yield edge rule (source.RepositoryPath + "#" + source.Symbol) "source" | None -> () ])
        "{\"schemaVersion\":1,\"packageManifestDigest\":" + jsonString (hex identity.ManifestDigest) + ",\"authorityBoundary\":{\"migrated\":\"first-combat-vertical-slice\",\"outside\":\"legacy\"},\"edges\":[" + String.concat "," edges + "]}"

    let rec canonicalApplicationBytes application =
        CanonicalEncoding.concatenate
            [ text application.ApplicationId; text (RuleId.value application.RuleId); list (fun (name, value) -> CanonicalEncoding.concatenate [ text name; valueBytes value ]) application.Operands; valueBytes application.Outcome; list canonicalApplicationBytes application.Children; text application.EventId; segment application.PackageManifestDigest ]
