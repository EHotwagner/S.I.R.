
import { Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { array_type, uint8_type, tuple_type, bool_type, int32_type, record_type, option_type, list_type, union_type, string_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { isNullOrWhiteSpace } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { exists } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { FSharpResult$2 } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { FixedPoint_$reflection } from "./FixedPoint.js";

export class RuleId extends Union {
    constructor(value) {
        super();
        this.tag = 0;
        this.fields = [value];
    }
    cases() {
        return ["RuleId"];
    }
}

export function RuleId_$reflection() {
    return union_type("SIR.Domain.RuleId", [], RuleId, () => [[["value", string_type]]]);
}

export function RuleIdModule_create(value) {
    if ((isNullOrWhiteSpace(value) ? true : (value.length < 5)) ? true : exists((arg) => {
        let character;
        return !((character = arg, (((character >= "A") && (character <= "Z")) ? true : ((character >= "0") && (character <= "9"))) ? true : (character === "-")));
    }, value.split(""))) {
        return new FSharpResult$2(/* Error */ 1, ["Rule IDs use non-empty uppercase ASCII letters, digits, and hyphens."]);
    }
    else if ((value.startsWith("-") ? true : value.endsWith("-")) ? true : (value.indexOf("--") >= 0)) {
        return new FSharpResult$2(/* Error */ 1, ["Rule IDs cannot begin/end with or repeat a hyphen."]);
    }
    else {
        return new FSharpResult$2(/* Ok */ 0, [new RuleId(value)]);
    }
}

export function RuleIdModule_value(_arg) {
    return _arg.fields[0];
}

export class RuleStatus extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Proposed", "Prototype", "Canonical", "Deprecated", "Superseded"];
    }
    static Proposed = new RuleStatus(0, []);
    static Prototype = new RuleStatus(1, []);
    static Canonical = new RuleStatus(2, []);
    static Deprecated = new RuleStatus(3, []);
    static Superseded = new RuleStatus(4, []);
}

export function RuleStatus_$reflection() {
    return union_type("SIR.Domain.RuleStatus", [], RuleStatus, () => [[], [], [], [], []]);
}

export class RuleKind extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Fact", "Predicate", "Formula", "Transition", "Algorithm", "Narrative"];
    }
    static Fact = new RuleKind(0, []);
    static Predicate = new RuleKind(1, []);
    static Formula = new RuleKind(2, []);
    static Transition = new RuleKind(3, []);
    static Algorithm = new RuleKind(4, []);
    static Narrative = new RuleKind(5, []);
}

export function RuleKind_$reflection() {
    return union_type("SIR.Domain.RuleKind", [], RuleKind, () => [[], [], [], [], [], []]);
}

export class ControlledStatement extends Record {
    constructor(Preconditions, Trigger, System, Responses) {
        super();
        this.Preconditions = Preconditions;
        this.Trigger = Trigger;
        this.System = System;
        this.Responses = Responses;
    }
}

export function ControlledStatement_$reflection() {
    return record_type("SIR.Domain.ControlledStatement", [], ControlledStatement, () => [["Preconditions", list_type(string_type)], ["Trigger", option_type(string_type)], ["System", string_type], ["Responses", list_type(string_type)]]);
}

export class SourceRef extends Record {
    constructor(Symbol$, RepositoryPath, Commit) {
        super();
        this.Symbol = Symbol$;
        this.RepositoryPath = RepositoryPath;
        this.Commit = Commit;
    }
}

export function SourceRef_$reflection() {
    return record_type("SIR.Domain.SourceRef", [], SourceRef, () => [["Symbol", string_type], ["RepositoryPath", string_type], ["Commit", string_type]]);
}

export class RuleMetadata extends Record {
    constructor(Id, Title, Status, SemanticKind, Statement, Rationale, Dependencies, Supersedes, RuleSource, Examples, Properties, Evidence) {
        super();
        this.Id = Id;
        this.Title = Title;
        this.Status = Status;
        this.SemanticKind = SemanticKind;
        this.Statement = Statement;
        this.Rationale = Rationale;
        this.Dependencies = Dependencies;
        this.Supersedes = Supersedes;
        this.RuleSource = RuleSource;
        this.Examples = Examples;
        this.Properties = Properties;
        this.Evidence = Evidence;
    }
}

export function RuleMetadata_$reflection() {
    return record_type("SIR.Domain.RuleMetadata", [], RuleMetadata, () => [["Id", RuleId_$reflection()], ["Title", string_type], ["Status", RuleStatus_$reflection()], ["SemanticKind", RuleKind_$reflection()], ["Statement", ControlledStatement_$reflection()], ["Rationale", string_type], ["Dependencies", list_type(RuleId_$reflection())], ["Supersedes", list_type(RuleId_$reflection())], ["RuleSource", option_type(SourceRef_$reflection())], ["Examples", list_type(string_type)], ["Properties", list_type(string_type)], ["Evidence", list_type(string_type)]]);
}

export class RuleValueKind extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Integer", "FixedPoint", "Boolean", "Text"];
    }
    static Integer = new RuleValueKind(0, []);
    static FixedPoint = new RuleValueKind(1, []);
    static Boolean$ = new RuleValueKind(2, []);
    static Text$ = new RuleValueKind(3, []);
}

export function RuleValueKind_$reflection() {
    return union_type("SIR.Domain.RuleValueKind", [], RuleValueKind, () => [[], [], [], []]);
}

export class TypedValue extends Record {
    constructor(DataKind, Unit, Value) {
        super();
        this.DataKind = DataKind;
        this.Unit = Unit;
        this.Value = Value;
    }
}

export function TypedValue_$reflection() {
    return record_type("SIR.Domain.TypedValue", [], TypedValue, () => [["DataKind", RuleValueKind_$reflection()], ["Unit", string_type], ["Value", RuleValue_$reflection()]]);
}

export class RuleValue extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["IntegerValue", "FixedPointValue", "BooleanValue", "TextValue"];
    }
}

export function RuleValue_$reflection() {
    return union_type("SIR.Domain.RuleValue", [], RuleValue, () => [[["Item", int32_type]], [["Item", FixedPoint_$reflection()]], [["Item", bool_type]], [["Item", string_type]]]);
}

export class FormulaExpr extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Constant", "Input", "Add", "Subtract", "Multiply", "Divide", "MinimumOf", "MaximumOf", "Clamp", "LessThanOrEqual", "IfThenElse"];
    }
}

export function FormulaExpr_$reflection() {
    return union_type("SIR.Domain.FormulaExpr", [], FormulaExpr, () => [[["Item", TypedValue_$reflection()]], [["name", string_type], ["kind", RuleValueKind_$reflection()], ["unitName", string_type]], [["Item1", FormulaExpr_$reflection()], ["Item2", FormulaExpr_$reflection()]], [["Item1", FormulaExpr_$reflection()], ["Item2", FormulaExpr_$reflection()]], [["Item1", FormulaExpr_$reflection()], ["Item2", FormulaExpr_$reflection()]], [["Item1", FormulaExpr_$reflection()], ["Item2", FormulaExpr_$reflection()]], [["Item1", FormulaExpr_$reflection()], ["Item2", FormulaExpr_$reflection()]], [["Item1", FormulaExpr_$reflection()], ["Item2", FormulaExpr_$reflection()]], [["minimum", FormulaExpr_$reflection()], ["maximum", FormulaExpr_$reflection()], ["value", FormulaExpr_$reflection()]], [["Item1", FormulaExpr_$reflection()], ["Item2", FormulaExpr_$reflection()]], [["condition", FormulaExpr_$reflection()], ["whenTrue", FormulaExpr_$reflection()], ["whenFalse", FormulaExpr_$reflection()]]]);
}

export class TransitionContract extends Record {
    constructor(Phase, Preconditions, Reads, Effects, Events) {
        super();
        this.Phase = Phase;
        this.Preconditions = Preconditions;
        this.Reads = Reads;
        this.Effects = Effects;
        this.Events = Events;
    }
}

export function TransitionContract_$reflection() {
    return record_type("SIR.Domain.TransitionContract", [], TransitionContract, () => [["Phase", string_type], ["Preconditions", list_type(RuleId_$reflection())], ["Reads", list_type(string_type)], ["Effects", list_type(string_type)], ["Events", list_type(string_type)]]);
}

export class AlgorithmContract extends Record {
    constructor(ImplementationSymbol, Fingerprint, Inputs, ResultKind, ResultUnit, ExplanationFields) {
        super();
        this.ImplementationSymbol = ImplementationSymbol;
        this.Fingerprint = Fingerprint;
        this.Inputs = Inputs;
        this.ResultKind = ResultKind;
        this.ResultUnit = ResultUnit;
        this.ExplanationFields = ExplanationFields;
    }
}

export function AlgorithmContract_$reflection() {
    return record_type("SIR.Domain.AlgorithmContract", [], AlgorithmContract, () => [["ImplementationSymbol", string_type], ["Fingerprint", string_type], ["Inputs", list_type(tuple_type(string_type, RuleValueKind_$reflection(), string_type))], ["ResultKind", RuleValueKind_$reflection()], ["ResultUnit", string_type], ["ExplanationFields", list_type(string_type)]]);
}

export class RuleSemantics extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["FactSemantics", "PredicateSemantics", "FormulaSemantics", "TransitionSemantics", "AlgorithmSemantics", "NarrativeSemantics"];
    }
    static NarrativeSemantics = new RuleSemantics(5, []);
}

export function RuleSemantics_$reflection() {
    return union_type("SIR.Domain.RuleSemantics", [], RuleSemantics, () => [[["Item", TypedValue_$reflection()]], [["Item", FormulaExpr_$reflection()]], [["resultKind", RuleValueKind_$reflection()], ["resultUnit", string_type], ["Item3", FormulaExpr_$reflection()]], [["Item", TransitionContract_$reflection()]], [["Item", AlgorithmContract_$reflection()]], []]);
}

export class RuleDefinition extends Record {
    constructor(Metadata, Semantics) {
        super();
        this.Metadata = Metadata;
        this.Semantics = Semantics;
    }
}

export function RuleDefinition_$reflection() {
    return record_type("SIR.Domain.RuleDefinition", [], RuleDefinition, () => [["Metadata", RuleMetadata_$reflection()], ["Semantics", RuleSemantics_$reflection()]]);
}

export class RuleApplication extends Record {
    constructor(ApplicationId, RuleId, Operands, Outcome, Children, EventId, PackageManifestDigest) {
        super();
        this.ApplicationId = ApplicationId;
        this.RuleId = RuleId;
        this.Operands = Operands;
        this.Outcome = Outcome;
        this.Children = Children;
        this.EventId = EventId;
        this.PackageManifestDigest = PackageManifestDigest;
    }
}

export function RuleApplication_$reflection() {
    return record_type("SIR.Domain.RuleApplication", [], RuleApplication, () => [["ApplicationId", string_type], ["RuleId", RuleId_$reflection()], ["Operands", list_type(tuple_type(string_type, TypedValue_$reflection()))], ["Outcome", TypedValue_$reflection()], ["Children", list_type(RuleApplication_$reflection())], ["EventId", string_type], ["PackageManifestDigest", array_type(uint8_type)]]);
}

export class RegistryError extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["DuplicateRuleId", "DanglingRuleReference", "IncompleteRuleMetadata", "IncompatibleRuleKind", "InvalidTypedValue", "InvalidFormulaResult", "InvalidAlgorithmContract", "IncompatibleRuleStatus"];
    }
}

export function RegistryError_$reflection() {
    return union_type("SIR.Domain.RegistryError", [], RegistryError, () => [[["Item", string_type]], [["owner", string_type], ["target", string_type]], [["ruleId", string_type], ["field", string_type]], [["ruleId", string_type]], [["ruleId", string_type], ["field", string_type]], [["ruleId", string_type], ["detail", string_type]], [["ruleId", string_type], ["field", string_type]], [["ruleId", string_type]]]);
}

export class EvaluationError extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["MissingInput", "TypeMismatch", "UnitMismatch", "DivisionByZero", "InvalidExpression"];
    }
    static DivisionByZero = new EvaluationError(3, []);
}

export function EvaluationError_$reflection() {
    return union_type("SIR.Domain.EvaluationError", [], EvaluationError, () => [[["Item", string_type]], [["Item", string_type]], [["Item1", string_type], ["Item2", string_type]], [], [["Item", string_type]]]);
}

export class RulePackageIdentity extends Record {
    constructor(SchemaVersion, EngineIdentity, CompatibilityProfile, PackageVersion, SourceCommit, ImplementationDigest, SemanticDigest, ManifestDigest) {
        super();
        this.SchemaVersion = (SchemaVersion | 0);
        this.EngineIdentity = EngineIdentity;
        this.CompatibilityProfile = CompatibilityProfile;
        this.PackageVersion = PackageVersion;
        this.SourceCommit = SourceCommit;
        this.ImplementationDigest = ImplementationDigest;
        this.SemanticDigest = SemanticDigest;
        this.ManifestDigest = ManifestDigest;
    }
}

export function RulePackageIdentity_$reflection() {
    return record_type("SIR.Domain.RulePackageIdentity", [], RulePackageIdentity, () => [["SchemaVersion", int32_type], ["EngineIdentity", string_type], ["CompatibilityProfile", string_type], ["PackageVersion", string_type], ["SourceCommit", string_type], ["ImplementationDigest", array_type(uint8_type)], ["SemanticDigest", array_type(uint8_type)], ["ManifestDigest", array_type(uint8_type)]]);
}

