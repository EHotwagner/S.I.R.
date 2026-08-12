namespace SIR.Domain

open System

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
type RegistryError = DuplicateRuleId of string | DanglingRuleReference of owner: string * target: string | IncompleteRuleMetadata of ruleId: string * field: string | IncompatibleRuleKind of ruleId: string | InvalidTypedValue of ruleId: string * field: string | InvalidFormulaResult of ruleId: string * detail: string | InvalidAlgorithmContract of ruleId: string * field: string | IncompatibleRuleStatus of ruleId: string
type EvaluationError = MissingInput of string | TypeMismatch of string | UnitMismatch of string * string | DivisionByZero | InvalidExpression of string
type RulePackageIdentity = { SchemaVersion: int32; EngineIdentity: string; CompatibilityProfile: string; PackageVersion: string; SourceCommit: string; ImplementationDigest: byte array; SemanticDigest: byte array; ManifestDigest: byte array }
