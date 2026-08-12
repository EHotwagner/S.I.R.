
import { get_UTF8 } from "../fable_modules/fable-library-js.5.13.0/Encoding.js";
import { fixedPoint, byteValue, int32LittleEndian, concatenate } from "./CanonicalEncoding.js";
import { ofArray, sortBy, isEmpty, collect, choose, sort, map, singleton, length, append } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { int32ToString, stringHash, equalArrays, comparePrimitives, equals } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { RulePackageIdentity, RuleStatus, RuleKind, RegistryError, RuleIdModule_value, FormulaExpr, RuleValueKind, TypedValue, RuleValue, EvaluationError } from "./RuleTypes.js";
import { Result_Map, Result_MapError, FSharpResult$2 } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { FixedPointModule_compareByRaw, FixedPointModule_raw, FixedPointModule_fromRatio, FixedPointModule_subtractSaturating, FixedPointModule_addSaturating, FixedPointModule_multiplySaturating } from "./FixedPoint.js";
import { tryFind } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { contains, ofList } from "../fable_modules/fable-library-js.5.13.0/Set.js";
import { format, join, replace, printf, toText, isNullOrWhiteSpace } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { List_distinct, List_countBy } from "../fable_modules/fable-library-js.5.13.0/Seq2.js";
import { map as map_2, collect as collect_1, empty, singleton as singleton_1, append as append_1, delay, toList } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { sha256 } from "./CanonicalHash.js";
import { map as map_1 } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { defaultArg } from "../fable_modules/fable-library-js.5.13.0/Option.js";

function bytes(value) {
    return get_UTF8().getBytes(value);
}

function segment(value) {
    return concatenate([int32LittleEndian(value.length), value]);
}

function text(value) {
    return segment(bytes(value));
}

function list(encode, values) {
    return concatenate(append(singleton(int32LittleEndian(length(values))), map(encode, values)));
}

function boolByte(value) {
    return byteValue(value ? 1 : 0);
}

function kindCode(_arg) {
    switch (_arg.tag) {
        case 1:
            return 1;
        case 2:
            return 2;
        case 3:
            return 3;
        default:
            return 0;
    }
}

function statusCode(_arg) {
    switch (_arg.tag) {
        case 1:
            return 1;
        case 2:
            return 2;
        case 3:
            return 3;
        case 4:
            return 4;
        default:
            return 0;
    }
}

function ruleKindCode(_arg) {
    switch (_arg.tag) {
        case 1:
            return 1;
        case 2:
            return 2;
        case 3:
            return 3;
        case 4:
            return 4;
        case 5:
            return 5;
        default:
            return 0;
    }
}

function valueBytes(value) {
    let payload;
    const matchValue = value.Value;
    payload = ((matchValue.tag === 1) ? fixedPoint(matchValue.fields[0]) : ((matchValue.tag === 2) ? boolByte(matchValue.fields[0]) : ((matchValue.tag === 3) ? text(matchValue.fields[0]) : int32LittleEndian(matchValue.fields[0]))));
    return concatenate([byteValue(kindCode(value.DataKind)), text(value.Unit), payload]);
}

function sameShape(left, right) {
    if (equals(left.DataKind, right.DataKind)) {
        return left.Unit === right.Unit;
    }
    else {
        return false;
    }
}

function fixedBinary(operation, left, right) {
    if (!sameShape(left, right)) {
        return new FSharpResult$2(/* Error */ 1, [new EvaluationError(/* UnitMismatch */ 2, [left.Unit, right.Unit])]);
    }
    else {
        const matchValue = left.Value;
        const matchValue_1 = right.Value;
        let matchResult, a, b;
        if (matchValue.tag === 1) {
            if (matchValue_1.tag === 1) {
                matchResult = 0;
                a = matchValue.fields[0];
                b = matchValue_1.fields[0];
            }
            else {
                matchResult = 1;
            }
        }
        else {
            matchResult = 1;
        }
        switch (matchResult) {
            case 0:
                return new FSharpResult$2(/* Ok */ 0, [new TypedValue(left.DataKind, left.Unit, new RuleValue(/* FixedPointValue */ 1, [operation(a, b)]))]);
            default:
                return new FSharpResult$2(/* Error */ 1, [new EvaluationError(/* TypeMismatch */ 1, ["The arithmetic operator requires FixedPoint values."])]);
        }
    }
}

function fixedMultiply(left, right) {
    const matchValue = left.Value;
    const matchValue_1 = right.Value;
    let matchResult, a, b;
    if (matchValue.tag === 1) {
        if (matchValue_1.tag === 1) {
            matchResult = 0;
            a = matchValue.fields[0];
            b = matchValue_1.fields[0];
        }
        else {
            matchResult = 1;
        }
    }
    else {
        matchResult = 1;
    }
    switch (matchResult) {
        case 0: {
            const resultUnit = (left.Unit === "ratio") ? right.Unit : ((right.Unit === "ratio") ? left.Unit : ((left.Unit === right.Unit) ? left.Unit : ""));
            if (resultUnit === "") {
                return new FSharpResult$2(/* Error */ 1, [new EvaluationError(/* UnitMismatch */ 2, [left.Unit, right.Unit])]);
            }
            else {
                return new FSharpResult$2(/* Ok */ 0, [new TypedValue(RuleValueKind.FixedPoint, resultUnit, new RuleValue(/* FixedPointValue */ 1, [FixedPointModule_multiplySaturating(a, b)]))]);
            }
        }
        default:
            return new FSharpResult$2(/* Error */ 1, [new EvaluationError(/* TypeMismatch */ 1, ["Multiplication requires FixedPoint values."])]);
    }
}

export function evaluate(inputs_mut, expression_mut) {
    evaluate:
    while (true) {
        const inputs = inputs_mut, expression = expression_mut;
        const pair = (left, right, continuation) => {
            const matchValue = evaluate(inputs, left);
            const matchValue_1 = evaluate(inputs, right);
            let matchResult, a, b, error;
            const copyOfStruct = matchValue;
            if (copyOfStruct.tag === 1) {
                matchResult = 1;
                error = copyOfStruct.fields[0];
            }
            else {
                const copyOfStruct_1 = matchValue_1;
                if (copyOfStruct_1.tag === 1) {
                    matchResult = 1;
                    error = copyOfStruct_1.fields[0];
                }
                else {
                    matchResult = 0;
                    a = copyOfStruct.fields[0];
                    b = copyOfStruct_1.fields[0];
                }
            }
            switch (matchResult) {
                case 0:
                    return continuation(a)(b);
                default:
                    return new FSharpResult$2(/* Error */ 1, [error]);
            }
        };
        let matchResult_1, expression_1, left_8, right_8;
        switch (expression.tag) {
            case 1: {
                matchResult_1 = 1;
                break;
            }
            case 2: {
                matchResult_1 = 2;
                break;
            }
            case 3: {
                matchResult_1 = 3;
                break;
            }
            case 4: {
                matchResult_1 = 4;
                break;
            }
            case 5: {
                matchResult_1 = 5;
                break;
            }
            case 6: {
                matchResult_1 = 6;
                expression_1 = expression;
                left_8 = expression.fields[0];
                right_8 = expression.fields[1];
                break;
            }
            case 7: {
                matchResult_1 = 6;
                expression_1 = expression;
                left_8 = expression.fields[0];
                right_8 = expression.fields[1];
                break;
            }
            case 8: {
                matchResult_1 = 7;
                break;
            }
            case 9: {
                matchResult_1 = 8;
                break;
            }
            case 10: {
                matchResult_1 = 9;
                break;
            }
            default:
                matchResult_1 = 0;
        }
        switch (matchResult_1) {
            case 0:
                return new FSharpResult$2(/* Ok */ 0, [expression.fields[0]]);
            case 1: {
                const unitName = expression.fields[2];
                const name = expression.fields[0];
                const matchValue_3 = tryFind(name, inputs);
                if (matchValue_3 != null) {
                    if (!equals(matchValue_3.DataKind, expression.fields[1])) {
                        const value_3 = matchValue_3;
                        return new FSharpResult$2(/* Error */ 1, [new EvaluationError(/* TypeMismatch */ 1, [name])]);
                    }
                    else if (matchValue_3.Unit !== unitName) {
                        const value_4 = matchValue_3;
                        return new FSharpResult$2(/* Error */ 1, [new EvaluationError(/* UnitMismatch */ 2, [unitName, value_4.Unit])]);
                    }
                    else {
                        const value_5 = matchValue_3;
                        return new FSharpResult$2(/* Ok */ 0, [value_5]);
                    }
                }
                else {
                    return new FSharpResult$2(/* Error */ 1, [new EvaluationError(/* MissingInput */ 0, [name])]);
                }
            }
            case 2:
                return pair(expression.fields[0], expression.fields[1], (left_2) => ((right_2) => fixedBinary(FixedPointModule_addSaturating, left_2, right_2)));
            case 3:
                return pair(expression.fields[0], expression.fields[1], (left_4) => ((right_4) => fixedBinary(FixedPointModule_subtractSaturating, left_4, right_4)));
            case 4:
                return pair(expression.fields[0], expression.fields[1], (left_6) => ((right_6) => fixedMultiply(left_6, right_6)));
            case 5:
                return pair(expression.fields[0], expression.fields[1], (a_1) => ((b_1) => {
                    if (!sameShape(a_1, b_1)) {
                        return new FSharpResult$2(/* Error */ 1, [new EvaluationError(/* UnitMismatch */ 2, [a_1.Unit, b_1.Unit])]);
                    }
                    else {
                        const matchValue_4 = a_1.Value;
                        const matchValue_5 = b_1.Value;
                        let matchResult_2, denominator_1, numerator_1, denominator_2, numerator_2;
                        if (matchValue_4.tag === 1) {
                            if (matchValue_5.tag === 1) {
                                if (FixedPointModule_raw(matchValue_5.fields[0]) === 0) {
                                    matchResult_2 = 0;
                                    denominator_1 = matchValue_5.fields[0];
                                    numerator_1 = matchValue_4.fields[0];
                                }
                                else {
                                    matchResult_2 = 1;
                                    denominator_2 = matchValue_5.fields[0];
                                    numerator_2 = matchValue_4.fields[0];
                                }
                            }
                            else {
                                matchResult_2 = 2;
                            }
                        }
                        else {
                            matchResult_2 = 2;
                        }
                        switch (matchResult_2) {
                            case 0:
                                return new FSharpResult$2(/* Error */ 1, [EvaluationError.DivisionByZero]);
                            case 1:
                                return Result_MapError((_arg) => EvaluationError.DivisionByZero, Result_Map((quotient) => (new TypedValue(RuleValueKind.FixedPoint, "ratio", new RuleValue(/* FixedPointValue */ 1, [quotient]))), FixedPointModule_fromRatio(FixedPointModule_raw(numerator_2), FixedPointModule_raw(denominator_2))));
                            default:
                                return new FSharpResult$2(/* Error */ 1, [new EvaluationError(/* TypeMismatch */ 1, ["Division requires FixedPoint values."])]);
                        }
                    }
                }));
            case 6:
                return pair(left_8, right_8, (a_2) => ((b_2) => {
                    if (!sameShape(a_2, b_2)) {
                        return new FSharpResult$2(/* Error */ 1, [new EvaluationError(/* UnitMismatch */ 2, [a_2.Unit, b_2.Unit])]);
                    }
                    else {
                        const matchValue_7 = a_2.Value;
                        const matchValue_8 = b_2.Value;
                        let matchResult_3, x, y;
                        if (matchValue_7.tag === 1) {
                            if (matchValue_8.tag === 1) {
                                matchResult_3 = 0;
                                x = matchValue_7.fields[0];
                                y = matchValue_8.fields[0];
                            }
                            else {
                                matchResult_3 = 1;
                            }
                        }
                        else {
                            matchResult_3 = 1;
                        }
                        switch (matchResult_3) {
                            case 0: {
                                const comparison = FixedPointModule_compareByRaw(x, y) | 0;
                                return new FSharpResult$2(/* Ok */ 0, [new TypedValue(a_2.DataKind, a_2.Unit, new RuleValue(/* FixedPointValue */ 1, [(expression_1.tag === 6) ? ((comparison <= 0) ? x : y) : ((comparison >= 0) ? x : y)]))]);
                            }
                            default:
                                return new FSharpResult$2(/* Error */ 1, [new EvaluationError(/* TypeMismatch */ 1, ["Minimum/maximum require FixedPoint values."])]);
                        }
                    }
                }));
            case 7: {
                inputs_mut = inputs;
                expression_mut = (new FormulaExpr(/* MaximumOf */ 7, [expression.fields[0], new FormulaExpr(/* MinimumOf */ 6, [expression.fields[1], expression.fields[2]])]));
                continue evaluate;
            }
            case 8:
                return pair(expression.fields[0], expression.fields[1], (a_3) => ((b_3) => {
                    if (!sameShape(a_3, b_3)) {
                        return new FSharpResult$2(/* Error */ 1, [new EvaluationError(/* UnitMismatch */ 2, [a_3.Unit, b_3.Unit])]);
                    }
                    else {
                        const matchValue_10 = a_3.Value;
                        const matchValue_11 = b_3.Value;
                        let matchResult_4, x_1, y_1, x_2, y_2;
                        switch (matchValue_10.tag) {
                            case 1: {
                                if (matchValue_11.tag === 1) {
                                    matchResult_4 = 0;
                                    x_1 = matchValue_10.fields[0];
                                    y_1 = matchValue_11.fields[0];
                                }
                                else {
                                    matchResult_4 = 2;
                                }
                                break;
                            }
                            case 0: {
                                if (matchValue_11.tag === 0) {
                                    matchResult_4 = 1;
                                    x_2 = matchValue_10.fields[0];
                                    y_2 = matchValue_11.fields[0];
                                }
                                else {
                                    matchResult_4 = 2;
                                }
                                break;
                            }
                            default:
                                matchResult_4 = 2;
                        }
                        switch (matchResult_4) {
                            case 0:
                                return new FSharpResult$2(/* Ok */ 0, [new TypedValue(RuleValueKind.Boolean$, "boolean", new RuleValue(/* BooleanValue */ 2, [FixedPointModule_compareByRaw(x_1, y_1) <= 0]))]);
                            case 1:
                                return new FSharpResult$2(/* Ok */ 0, [new TypedValue(RuleValueKind.Boolean$, "boolean", new RuleValue(/* BooleanValue */ 2, [x_2 <= y_2]))]);
                            default:
                                return new FSharpResult$2(/* Error */ 1, [new EvaluationError(/* TypeMismatch */ 1, ["Comparison requires like numeric values."])]);
                        }
                    }
                }));
            default: {
                const matchValue_13 = evaluate(inputs, expression.fields[0]);
                if (matchValue_13.tag === 1) {
                    return new FSharpResult$2(/* Error */ 1, [matchValue_13.fields[0]]);
                }
                else if (matchValue_13.fields[0].Value.tag === 2) {
                    if (matchValue_13.fields[0].Value.fields[0]) {
                        inputs_mut = inputs;
                        expression_mut = expression.fields[1];
                        continue evaluate;
                    }
                    else {
                        inputs_mut = inputs;
                        expression_mut = expression.fields[2];
                        continue evaluate;
                    }
                }
                else {
                    return new FSharpResult$2(/* Error */ 1, [new EvaluationError(/* TypeMismatch */ 1, ["Conditional requires a Boolean condition."])]);
                }
            }
        }
        break;
    }
}

function expressionBytes(_arg) {
    let matchResult, a_7, b_7, c, expression;
    switch (_arg.tag) {
        case 1: {
            matchResult = 1;
            break;
        }
        case 2: {
            matchResult = 2;
            break;
        }
        case 3: {
            matchResult = 3;
            break;
        }
        case 4: {
            matchResult = 4;
            break;
        }
        case 5: {
            matchResult = 5;
            break;
        }
        case 6: {
            matchResult = 6;
            break;
        }
        case 7: {
            matchResult = 7;
            break;
        }
        case 9: {
            matchResult = 8;
            break;
        }
        case 8: {
            matchResult = 9;
            a_7 = _arg.fields[0];
            b_7 = _arg.fields[1];
            c = _arg.fields[2];
            expression = _arg;
            break;
        }
        case 10: {
            matchResult = 9;
            a_7 = _arg.fields[0];
            b_7 = _arg.fields[1];
            c = _arg.fields[2];
            expression = _arg;
            break;
        }
        default:
            matchResult = 0;
    }
    switch (matchResult) {
        case 0:
            return concatenate([new Uint8Array([0]), valueBytes(_arg.fields[0])]);
        case 1:
            return concatenate([new Uint8Array([1, kindCode(_arg.fields[1])]), text(_arg.fields[0]), text(_arg.fields[2])]);
        case 2:
            return binary(2, _arg.fields[0], _arg.fields[1]);
        case 3:
            return binary(3, _arg.fields[0], _arg.fields[1]);
        case 4:
            return binary(4, _arg.fields[0], _arg.fields[1]);
        case 5:
            return binary(5, _arg.fields[0], _arg.fields[1]);
        case 6:
            return binary(6, _arg.fields[0], _arg.fields[1]);
        case 7:
            return binary(7, _arg.fields[0], _arg.fields[1]);
        case 8:
            return binary(9, _arg.fields[0], _arg.fields[1]);
        default:
            return concatenate([new Uint8Array([(expression.tag === 8) ? 8 : 10]), expressionBytes(a_7), expressionBytes(b_7), expressionBytes(c)]);
    }
}

function binary(tag, left, right) {
    return concatenate([new Uint8Array([tag]), expressionBytes(left), expressionBytes(right)]);
}

function semanticsBytes(_arg) {
    switch (_arg.tag) {
        case 1:
            return concatenate([new Uint8Array([1]), expressionBytes(_arg.fields[0])]);
        case 2:
            return concatenate([new Uint8Array([2, kindCode(_arg.fields[0])]), text(_arg.fields[1]), expressionBytes(_arg.fields[2])]);
        case 3: {
            const transition = _arg.fields[0];
            return concatenate([new Uint8Array([3]), text(transition.Phase), list((arg) => text(RuleIdModule_value(arg)), transition.Preconditions), list(text, transition.Reads), list(text, transition.Effects), list(text, transition.Events)]);
        }
        case 4: {
            const algorithm = _arg.fields[0];
            return concatenate([new Uint8Array([4, kindCode(algorithm.ResultKind)]), text(algorithm.ImplementationSymbol), text(algorithm.Fingerprint), list((tupledArg) => concatenate([text(tupledArg[0]), new Uint8Array([kindCode(tupledArg[1])]), text(tupledArg[2])]), algorithm.Inputs), text(algorithm.ResultUnit), list(text, algorithm.ExplanationFields)]);
        }
        case 5:
            return new Uint8Array([5]);
        default:
            return concatenate([new Uint8Array([0]), valueBytes(_arg.fields[0])]);
    }
}

export function canonicalRuleBytes(rule) {
    const metadata = rule.Metadata;
    let trigger;
    const matchValue = metadata.Statement.Trigger;
    trigger = ((matchValue != null) ? concatenate([new Uint8Array([1]), text(matchValue)]) : (new Uint8Array([0])));
    let source;
    const matchValue_1 = metadata.RuleSource;
    if (matchValue_1 != null) {
        const value_1 = matchValue_1;
        source = concatenate([new Uint8Array([1]), text(value_1.Symbol), text(value_1.RepositoryPath), text(value_1.Commit)]);
    }
    else {
        source = (new Uint8Array([0]));
    }
    return concatenate([text(RuleIdModule_value(metadata.Id)), text(metadata.Title), new Uint8Array([statusCode(metadata.Status), ruleKindCode(metadata.SemanticKind)]), list(text, metadata.Statement.Preconditions), trigger, text(metadata.Statement.System), list(text, metadata.Statement.Responses), text(metadata.Rationale), list(text, sort(map(RuleIdModule_value, metadata.Dependencies), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    })), list(text, sort(map(RuleIdModule_value, metadata.Supersedes), {
        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
    })), source, list(text, metadata.Examples), list(text, metadata.Properties), list(text, metadata.Evidence), semanticsBytes(rule.Semantics)]);
}

export function validate(rules) {
    const ids = map((rule) => RuleIdModule_value(rule.Metadata.Id), rules);
    const idSet = ofList(ids, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    const valueMatches = (value) => {
        const matchValue = value.DataKind;
        const matchValue_1 = value.Value;
        let matchResult;
        switch (matchValue.tag) {
            case 1: {
                if (matchValue_1.tag === 1) {
                    matchResult = 0;
                }
                else {
                    matchResult = 1;
                }
                break;
            }
            case 2: {
                if (matchValue_1.tag === 2) {
                    matchResult = 0;
                }
                else {
                    matchResult = 1;
                }
                break;
            }
            case 3: {
                if (matchValue_1.tag === 3) {
                    matchResult = 0;
                }
                else {
                    matchResult = 1;
                }
                break;
            }
            default:
                if (matchValue_1.tag === 0) {
                    matchResult = 0;
                }
                else {
                    matchResult = 1;
                }
        }
        switch (matchResult) {
            case 0:
                return true;
            default:
                return false;
        }
    };
    const expressionShape = (_arg) => {
        let left, left_4, y_2, x_3, left_6, value_1;
        let matchResult_1, value_2, kind_1, name_1, unitName_1, a, b, a_1, b_1, a_2, b_2, a_3, b_3, c, a_4, b_4, c_1, f, t;
        switch (_arg.tag) {
            case 1: {
                if (!isNullOrWhiteSpace(_arg.fields[0]) && !isNullOrWhiteSpace(_arg.fields[2])) {
                    matchResult_1 = 2;
                    kind_1 = _arg.fields[1];
                    name_1 = _arg.fields[0];
                    unitName_1 = _arg.fields[2];
                }
                else {
                    matchResult_1 = 3;
                }
                break;
            }
            case 2: {
                matchResult_1 = 4;
                a = _arg.fields[0];
                b = _arg.fields[1];
                break;
            }
            case 3: {
                matchResult_1 = 4;
                a = _arg.fields[0];
                b = _arg.fields[1];
                break;
            }
            case 6: {
                matchResult_1 = 4;
                a = _arg.fields[0];
                b = _arg.fields[1];
                break;
            }
            case 7: {
                matchResult_1 = 4;
                a = _arg.fields[0];
                b = _arg.fields[1];
                break;
            }
            case 4: {
                matchResult_1 = 5;
                a_1 = _arg.fields[0];
                b_1 = _arg.fields[1];
                break;
            }
            case 5: {
                matchResult_1 = 6;
                a_2 = _arg.fields[0];
                b_2 = _arg.fields[1];
                break;
            }
            case 8: {
                matchResult_1 = 7;
                a_3 = _arg.fields[0];
                b_3 = _arg.fields[1];
                c = _arg.fields[2];
                break;
            }
            case 9: {
                matchResult_1 = 8;
                a_4 = _arg.fields[0];
                b_4 = _arg.fields[1];
                break;
            }
            case 10: {
                matchResult_1 = 9;
                c_1 = _arg.fields[0];
                f = _arg.fields[2];
                t = _arg.fields[1];
                break;
            }
            default:
                if ((value_1 = _arg.fields[0], valueMatches(value_1) && !isNullOrWhiteSpace(value_1.Unit))) {
                    matchResult_1 = 0;
                    value_2 = _arg.fields[0];
                }
                else {
                    matchResult_1 = 1;
                }
        }
        switch (matchResult_1) {
            case 0:
                return new FSharpResult$2(/* Ok */ 0, [[value_2.DataKind, value_2.Unit]]);
            case 1:
                return new FSharpResult$2(/* Error */ 1, ["constant kind/unit"]);
            case 2:
                return new FSharpResult$2(/* Ok */ 0, [[kind_1, unitName_1]]);
            case 3:
                return new FSharpResult$2(/* Error */ 1, ["input name/unit"]);
            case 4: {
                const matchValue_3 = expressionShape(a);
                const matchValue_4 = expressionShape(b);
                let matchResult_2, left_1, right_1;
                const copyOfStruct = matchValue_3;
                if (copyOfStruct.tag === 0) {
                    const copyOfStruct_1 = matchValue_4;
                    if (copyOfStruct_1.tag === 0) {
                        if ((left = copyOfStruct.fields[0], equalArrays(left, copyOfStruct_1.fields[0]) && equals(left[0], RuleValueKind.FixedPoint))) {
                            matchResult_2 = 0;
                            left_1 = copyOfStruct.fields[0];
                            right_1 = copyOfStruct_1.fields[0];
                        }
                        else {
                            matchResult_2 = 1;
                        }
                    }
                    else {
                        matchResult_2 = 1;
                    }
                }
                else {
                    matchResult_2 = 1;
                }
                switch (matchResult_2) {
                    case 0:
                        return new FSharpResult$2(/* Ok */ 0, [left_1]);
                    default:
                        return new FSharpResult$2(/* Error */ 1, ["like fixed-point operands"]);
                }
            }
            case 5: {
                const matchValue_6 = expressionShape(a_1);
                const matchValue_7 = expressionShape(b_1);
                let matchResult_3, unitName_2, left_3, right_3;
                const copyOfStruct_2 = matchValue_6;
                if (copyOfStruct_2.tag === 0) {
                    if (copyOfStruct_2.fields[0][0].tag === 1) {
                        if (copyOfStruct_2.fields[0][1] === "ratio") {
                            const copyOfStruct_3 = matchValue_7;
                            if (copyOfStruct_3.tag === 0) {
                                if (copyOfStruct_3.fields[0][0].tag === 1) {
                                    matchResult_3 = 0;
                                    unitName_2 = copyOfStruct_3.fields[0][1];
                                }
                                else {
                                    matchResult_3 = 2;
                                }
                            }
                            else {
                                matchResult_3 = 2;
                            }
                        }
                        else {
                            const copyOfStruct_4 = matchValue_7;
                            if (copyOfStruct_4.tag === 0) {
                                if (copyOfStruct_4.fields[0][0].tag === 1) {
                                    if (copyOfStruct_4.fields[0][1] === "ratio") {
                                        matchResult_3 = 0;
                                        unitName_2 = copyOfStruct_2.fields[0][1];
                                    }
                                    else if (copyOfStruct_2.fields[0][1] === copyOfStruct_4.fields[0][1]) {
                                        matchResult_3 = 1;
                                        left_3 = copyOfStruct_2.fields[0][1];
                                        right_3 = copyOfStruct_4.fields[0][1];
                                    }
                                    else {
                                        matchResult_3 = 2;
                                    }
                                }
                                else {
                                    matchResult_3 = 2;
                                }
                            }
                            else {
                                matchResult_3 = 2;
                            }
                        }
                    }
                    else {
                        matchResult_3 = 2;
                    }
                }
                else {
                    matchResult_3 = 2;
                }
                switch (matchResult_3) {
                    case 0:
                        return new FSharpResult$2(/* Ok */ 0, [[RuleValueKind.FixedPoint, unitName_2]]);
                    case 1:
                        return new FSharpResult$2(/* Ok */ 0, [[RuleValueKind.FixedPoint, left_3]]);
                    default:
                        return new FSharpResult$2(/* Error */ 1, ["compatible fixed-point multiply operands"]);
                }
            }
            case 6: {
                const matchValue_9 = expressionShape(a_2);
                const matchValue_10 = expressionShape(b_2);
                let matchResult_4, left_5, right_5;
                const copyOfStruct_5 = matchValue_9;
                if (copyOfStruct_5.tag === 0) {
                    const copyOfStruct_6 = matchValue_10;
                    if (copyOfStruct_6.tag === 0) {
                        if ((left_4 = copyOfStruct_5.fields[0], equalArrays(left_4, copyOfStruct_6.fields[0]) && equals(left_4[0], RuleValueKind.FixedPoint))) {
                            matchResult_4 = 0;
                            left_5 = copyOfStruct_5.fields[0];
                            right_5 = copyOfStruct_6.fields[0];
                        }
                        else {
                            matchResult_4 = 1;
                        }
                    }
                    else {
                        matchResult_4 = 1;
                    }
                }
                else {
                    matchResult_4 = 1;
                }
                switch (matchResult_4) {
                    case 0:
                        return new FSharpResult$2(/* Ok */ 0, [[RuleValueKind.FixedPoint, "ratio"]]);
                    default:
                        return new FSharpResult$2(/* Error */ 1, ["like fixed-point divide operands"]);
                }
            }
            case 7: {
                const matchValue_12 = expressionShape(a_3);
                const matchValue_13 = expressionShape(b_3);
                const matchValue_14 = expressionShape(c);
                let matchResult_5, x_4, y_3, z_1;
                const copyOfStruct_7 = matchValue_12;
                if (copyOfStruct_7.tag === 0) {
                    const copyOfStruct_8 = matchValue_13;
                    if (copyOfStruct_8.tag === 0) {
                        const copyOfStruct_9 = matchValue_14;
                        if (copyOfStruct_9.tag === 0) {
                            if ((y_2 = copyOfStruct_8.fields[0], (x_3 = copyOfStruct_7.fields[0], (equalArrays(x_3, y_2) && equalArrays(y_2, copyOfStruct_9.fields[0])) && equals(x_3[0], RuleValueKind.FixedPoint)))) {
                                matchResult_5 = 0;
                                x_4 = copyOfStruct_7.fields[0];
                                y_3 = copyOfStruct_8.fields[0];
                                z_1 = copyOfStruct_9.fields[0];
                            }
                            else {
                                matchResult_5 = 1;
                            }
                        }
                        else {
                            matchResult_5 = 1;
                        }
                    }
                    else {
                        matchResult_5 = 1;
                    }
                }
                else {
                    matchResult_5 = 1;
                }
                switch (matchResult_5) {
                    case 0:
                        return new FSharpResult$2(/* Ok */ 0, [x_4]);
                    default:
                        return new FSharpResult$2(/* Error */ 1, ["like fixed-point clamp operands"]);
                }
            }
            case 8: {
                const matchValue_16 = expressionShape(a_4);
                const matchValue_17 = expressionShape(b_4);
                let matchResult_6, left_7, right_7;
                const copyOfStruct_10 = matchValue_16;
                if (copyOfStruct_10.tag === 0) {
                    const copyOfStruct_11 = matchValue_17;
                    if (copyOfStruct_11.tag === 0) {
                        if ((left_6 = copyOfStruct_10.fields[0], equalArrays(left_6, copyOfStruct_11.fields[0]) && (equals(left_6[0], RuleValueKind.FixedPoint) ? true : equals(left_6[0], RuleValueKind.Integer)))) {
                            matchResult_6 = 0;
                            left_7 = copyOfStruct_10.fields[0];
                            right_7 = copyOfStruct_11.fields[0];
                        }
                        else {
                            matchResult_6 = 1;
                        }
                    }
                    else {
                        matchResult_6 = 1;
                    }
                }
                else {
                    matchResult_6 = 1;
                }
                switch (matchResult_6) {
                    case 0:
                        return new FSharpResult$2(/* Ok */ 0, [[RuleValueKind.Boolean$, "boolean"]]);
                    default:
                        return new FSharpResult$2(/* Error */ 1, ["like numeric comparison operands"]);
                }
            }
            default: {
                const matchValue_19 = expressionShape(c_1);
                const matchValue_20 = expressionShape(t);
                const matchValue_21 = expressionShape(f);
                let matchResult_7, left_9, right_9;
                const copyOfStruct_12 = matchValue_19;
                if (copyOfStruct_12.tag === 0) {
                    if (copyOfStruct_12.fields[0][0].tag === 2) {
                        if (copyOfStruct_12.fields[0][1] === "boolean") {
                            const copyOfStruct_13 = matchValue_20;
                            if (copyOfStruct_13.tag === 0) {
                                const copyOfStruct_14 = matchValue_21;
                                if (copyOfStruct_14.tag === 0) {
                                    if (equalArrays(copyOfStruct_13.fields[0], copyOfStruct_14.fields[0])) {
                                        matchResult_7 = 0;
                                        left_9 = copyOfStruct_13.fields[0];
                                        right_9 = copyOfStruct_14.fields[0];
                                    }
                                    else {
                                        matchResult_7 = 1;
                                    }
                                }
                                else {
                                    matchResult_7 = 1;
                                }
                            }
                            else {
                                matchResult_7 = 1;
                            }
                        }
                        else {
                            matchResult_7 = 1;
                        }
                    }
                    else {
                        matchResult_7 = 1;
                    }
                }
                else {
                    matchResult_7 = 1;
                }
                switch (matchResult_7) {
                    case 0:
                        return new FSharpResult$2(/* Ok */ 0, [left_9]);
                    default:
                        return new FSharpResult$2(/* Error */ 1, ["Boolean condition and like branches"]);
                }
            }
        }
    };
    const matchValue_30 = append(choose((tupledArg) => {
        if (tupledArg[1] > 1) {
            return new RegistryError(/* DuplicateRuleId */ 0, [tupledArg[0]]);
        }
        else {
            return undefined;
        }
    }, List_countBy((x_1) => x_1, ids, {
        Equals: (x_2, y_1) => (x_2 === y_1),
        GetHashCode: (x_2) => (stringHash(x_2) | 0),
    })), collect((rule_1) => {
        const id_1 = RuleIdModule_value(rule_1.Metadata.Id);
        return toList(delay(() => append_1(isNullOrWhiteSpace(rule_1.Metadata.Title) ? singleton_1(new RegistryError(/* IncompleteRuleMetadata */ 2, [id_1, "title"])) : empty(), delay(() => append_1((isNullOrWhiteSpace(rule_1.Metadata.Rationale) && !equals(rule_1.Metadata.SemanticKind, RuleKind.Narrative)) ? singleton_1(new RegistryError(/* IncompleteRuleMetadata */ 2, [id_1, "rationale"])) : empty(), delay(() => append_1(isEmpty(rule_1.Metadata.Statement.Responses) ? singleton_1(new RegistryError(/* IncompleteRuleMetadata */ 2, [id_1, "statement.responses"])) : empty(), delay(() => append_1((!equals(rule_1.Metadata.SemanticKind, RuleKind.Narrative) && (rule_1.Metadata.RuleSource == null)) ? singleton_1(new RegistryError(/* IncompleteRuleMetadata */ 2, [id_1, "source"])) : empty(), delay(() => append_1((!equals(rule_1.Metadata.SemanticKind, RuleKind.Narrative) && isEmpty(rule_1.Metadata.Evidence)) ? singleton_1(new RegistryError(/* IncompleteRuleMetadata */ 2, [id_1, "evidence"])) : empty(), delay(() => append_1((!equals(rule_1.Metadata.SemanticKind, RuleKind.Narrative) && (isEmpty(rule_1.Metadata.Examples) ? true : isEmpty(rule_1.Metadata.Properties))) ? singleton_1(new RegistryError(/* IncompleteRuleMetadata */ 2, [id_1, "examples/properties"])) : empty(), delay(() => append_1((equals(rule_1.Metadata.Status, RuleStatus.Superseded) && isEmpty(rule_1.Metadata.Supersedes)) ? singleton_1(new RegistryError(/* IncompatibleRuleStatus */ 7, [id_1])) : empty(), delay(() => {
            let matchValue_23, source, source_1;
            return append_1((matchValue_23 = rule_1.Metadata.RuleSource, (matchValue_23 != null) ? (((source = matchValue_23, (((isNullOrWhiteSpace(source.Symbol) ? true : isNullOrWhiteSpace(source.RepositoryPath)) ? true : source.RepositoryPath.startsWith("/")) ? true : (source.RepositoryPath.indexOf("..") >= 0)) ? true : (source.Commit.length !== 40))) ? ((source_1 = matchValue_23, singleton_1(new RegistryError(/* IncompleteRuleMetadata */ 2, [id_1, "source.identity"])))) : (empty())) : (empty())), delay(() => append_1(collect_1((dependency) => (!contains(RuleIdModule_value(dependency), idSet) ? singleton_1(new RegistryError(/* DanglingRuleReference */ 1, [id_1, RuleIdModule_value(dependency)])) : empty()), append(rule_1.Metadata.Dependencies, rule_1.Metadata.Supersedes)), delay(() => {
                let matchValue_24, matchValue_25;
                return append_1((matchValue_24 = rule_1.Metadata.SemanticKind, (matchValue_25 = rule_1.Semantics, (matchValue_24.tag === 1) ? ((matchValue_25.tag === 1) ? (empty()) : singleton_1(new RegistryError(/* IncompatibleRuleKind */ 3, [id_1]))) : ((matchValue_24.tag === 2) ? ((matchValue_25.tag === 2) ? (empty()) : singleton_1(new RegistryError(/* IncompatibleRuleKind */ 3, [id_1]))) : ((matchValue_24.tag === 3) ? ((matchValue_25.tag === 3) ? (empty()) : singleton_1(new RegistryError(/* IncompatibleRuleKind */ 3, [id_1]))) : ((matchValue_24.tag === 4) ? ((matchValue_25.tag === 4) ? (empty()) : singleton_1(new RegistryError(/* IncompatibleRuleKind */ 3, [id_1]))) : ((matchValue_24.tag === 5) ? ((matchValue_25.tag === 5) ? (empty()) : singleton_1(new RegistryError(/* IncompatibleRuleKind */ 3, [id_1]))) : ((matchValue_25.tag === 0) ? (empty()) : singleton_1(new RegistryError(/* IncompatibleRuleKind */ 3, [id_1]))))))))), delay(() => {
                    let value_3, contract;
                    const matchValue_27 = rule_1.Semantics;
                    let matchResult_8, value_4, expression, expression_1, kind_2, unitName_3, contract_1, contract_2;
                    switch (matchValue_27.tag) {
                        case 0: {
                            if ((value_3 = matchValue_27.fields[0], !valueMatches(value_3) ? true : isNullOrWhiteSpace(value_3.Unit))) {
                                matchResult_8 = 0;
                                value_4 = matchValue_27.fields[0];
                            }
                            else {
                                matchResult_8 = 5;
                            }
                            break;
                        }
                        case 1: {
                            matchResult_8 = 1;
                            expression = matchValue_27.fields[0];
                            break;
                        }
                        case 2: {
                            matchResult_8 = 2;
                            expression_1 = matchValue_27.fields[2];
                            kind_2 = matchValue_27.fields[0];
                            unitName_3 = matchValue_27.fields[1];
                            break;
                        }
                        case 3: {
                            if ((contract = matchValue_27.fields[0], ((isNullOrWhiteSpace(contract.Phase) ? true : isEmpty(contract.Reads)) ? true : isEmpty(contract.Effects)) ? true : isEmpty(contract.Events))) {
                                matchResult_8 = 3;
                                contract_1 = matchValue_27.fields[0];
                            }
                            else {
                                matchResult_8 = 5;
                            }
                            break;
                        }
                        case 4: {
                            matchResult_8 = 4;
                            contract_2 = matchValue_27.fields[0];
                            break;
                        }
                        default:
                            matchResult_8 = 5;
                    }
                    switch (matchResult_8) {
                        case 0:
                            return singleton_1(new RegistryError(/* InvalidTypedValue */ 4, [id_1, "fact"]));
                        case 1: {
                            const matchValue_28 = expressionShape(expression);
                            let matchResult_9, verdict;
                            if (matchValue_28.tag === 0) {
                                if (matchValue_28.fields[0][0].tag === 2) {
                                    if (matchValue_28.fields[0][1] === "boolean") {
                                        matchResult_9 = 0;
                                    }
                                    else {
                                        matchResult_9 = 1;
                                        verdict = matchValue_28;
                                    }
                                }
                                else {
                                    matchResult_9 = 1;
                                    verdict = matchValue_28;
                                }
                            }
                            else {
                                matchResult_9 = 1;
                                verdict = matchValue_28;
                            }
                            switch (matchResult_9) {
                                case 0: {
                                    return empty();
                                }
                                default:
                                    return singleton_1(new RegistryError(/* InvalidFormulaResult */ 5, [id_1, toText(printf("%A"))(verdict)]));
                            }
                        }
                        case 2: {
                            const matchValue_29 = expressionShape(expression_1);
                            let matchResult_10, verdict_1;
                            if (matchValue_29.tag === 0) {
                                if (equals(matchValue_29.fields[0][0], kind_2) && (matchValue_29.fields[0][1] === unitName_3)) {
                                    matchResult_10 = 0;
                                }
                                else {
                                    matchResult_10 = 1;
                                    verdict_1 = matchValue_29;
                                }
                            }
                            else {
                                matchResult_10 = 1;
                                verdict_1 = matchValue_29;
                            }
                            switch (matchResult_10) {
                                case 0: {
                                    return empty();
                                }
                                default:
                                    return singleton_1(new RegistryError(/* InvalidFormulaResult */ 5, [id_1, toText(printf("%A"))(verdict_1)]));
                            }
                        }
                        case 3:
                            return singleton_1(new RegistryError(/* IncompleteRuleMetadata */ 2, [id_1, "transition.contract"]));
                        case 4:
                            return append_1(isNullOrWhiteSpace(contract_2.ImplementationSymbol) ? singleton_1(new RegistryError(/* InvalidAlgorithmContract */ 6, [id_1, "implementationSymbol"])) : empty(), delay(() => append_1(isNullOrWhiteSpace(contract_2.Fingerprint) ? singleton_1(new RegistryError(/* InvalidAlgorithmContract */ 6, [id_1, "fingerprint"])) : empty(), delay(() => append_1(isNullOrWhiteSpace(contract_2.ResultUnit) ? singleton_1(new RegistryError(/* InvalidAlgorithmContract */ 6, [id_1, "resultUnit"])) : empty(), delay(() => append_1(isEmpty(contract_2.Inputs) ? singleton_1(new RegistryError(/* InvalidAlgorithmContract */ 6, [id_1, "inputs"])) : empty(), delay(() => (isEmpty(contract_2.ExplanationFields) ? singleton_1(new RegistryError(/* InvalidAlgorithmContract */ 6, [id_1, "explanationFields"])) : empty())))))))));
                        default: {
                            return empty();
                        }
                    }
                }));
            }))));
        }))))))))))))))));
    }, rules));
    if (isEmpty(matchValue_30)) {
        return new FSharpResult$2(/* Ok */ 0, [sortBy((rule_2) => RuleIdModule_value(rule_2.Metadata.Id), rules, {
            Compare: (x_5, y_4) => (comparePrimitives(x_5, y_4) | 0),
        })]);
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [matchValue_30]);
    }
}

export function canonicalManifestPayload(schemaVersion, sourceCommit, rules) {
    const canonical = map(canonicalRuleBytes, sortBy((rule) => RuleIdModule_value(rule.Metadata.Id), rules, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }));
    return concatenate(append(ofArray([int32LittleEndian(schemaVersion), text(sourceCommit), int32LittleEndian(length(canonical))]), canonical));
}

function canonicalSemanticPayload(rules) {
    const encoded = map((rule_1) => concatenate([text(RuleIdModule_value(rule_1.Metadata.Id)), byteValue(ruleKindCode(rule_1.Metadata.SemanticKind)), list(text, sort(map(RuleIdModule_value, rule_1.Metadata.Dependencies), {
        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
    })), semanticsBytes(rule_1.Semantics)]), sortBy((rule) => RuleIdModule_value(rule.Metadata.Id), rules, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }));
    return concatenate(append(singleton(int32LittleEndian(length(encoded))), encoded));
}

function artifactBytes(name, digest) {
    return concatenate([text(name), segment(digest)]);
}

export function packageIdentity(engineIdentity, compatibilityProfile, packageVersion, sourceCommit, implementationArtifacts, rules) {
    let implementationDigest;
    const segments = map((tupledArg) => artifactBytes(tupledArg[0], tupledArg[1]), sortBy((tuple) => tuple[0], implementationArtifacts, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }));
    implementationDigest = sha256(concatenate(append(ofArray([text(compatibilityProfile), text(packageVersion)]), segments)));
    const semanticDigest = sha256(concatenate([segment(implementationDigest), canonicalSemanticPayload(rules)]));
    return new RulePackageIdentity(1, engineIdentity, compatibilityProfile, packageVersion, sourceCommit, implementationDigest, semanticDigest, sha256(concatenate([text(engineIdentity), text(compatibilityProfile), text(packageVersion), text(sourceCommit), segment(implementationDigest), segment(semanticDigest), canonicalManifestPayload(1, sourceCommit, rules)])));
}

function jsonString(value) {
    return ("\"" + replace(replace(replace(replace(value, "\\", "\\\\"), "\"", "\\\""), "\r", "\\r"), "\n", "\\n")) + "\"";
}

function jsonArray(encode, values) {
    return ("[" + join(",", map(encode, values))) + "]";
}

function hex(bytes_1) {
    return join("", map_1((value) => format('{0:' + "x2" + '}', value), bytes_1));
}

function kindName(_arg) {
    switch (_arg.tag) {
        case 1:
            return "fixedPoint";
        case 2:
            return "boolean";
        case 3:
            return "text";
        default:
            return "integer";
    }
}

function statusName(_arg) {
    switch (_arg.tag) {
        case 1:
            return "prototype";
        case 2:
            return "canonical";
        case 3:
            return "deprecated";
        case 4:
            return "superseded";
        default:
            return "proposed";
    }
}

function ruleKindName(_arg) {
    switch (_arg.tag) {
        case 1:
            return "predicate";
        case 2:
            return "formula";
        case 3:
            return "transition";
        case 4:
            return "algorithm";
        case 5:
            return "narrative";
        default:
            return "fact";
    }
}

function valueNotation(value) {
    const matchValue = value.Value;
    switch (matchValue.tag) {
        case 1:
            return (int32ToString(FixedPointModule_raw(matchValue.fields[0])) + "/") + int32ToString(10000);
        case 2:
            if (matchValue.fields[0]) {
                return "true";
            }
            else {
                return "false";
            }
        case 3:
            return jsonString(matchValue.fields[0]);
        default:
            return int32ToString(matchValue.fields[0]);
    }
}

export function formulaNotation(_arg) {
    switch (_arg.tag) {
        case 1:
            return (_arg.fields[0] + ":") + _arg.fields[2];
        case 2:
            return ((("(" + formulaNotation(_arg.fields[0])) + " + ") + formulaNotation(_arg.fields[1])) + ")";
        case 3:
            return ((("(" + formulaNotation(_arg.fields[0])) + " - ") + formulaNotation(_arg.fields[1])) + ")";
        case 4:
            return ((("(" + formulaNotation(_arg.fields[0])) + " × ") + formulaNotation(_arg.fields[1])) + ")";
        case 5:
            return ((("(" + formulaNotation(_arg.fields[0])) + " / ") + formulaNotation(_arg.fields[1])) + ")";
        case 6:
            return ((("min(" + formulaNotation(_arg.fields[0])) + ", ") + formulaNotation(_arg.fields[1])) + ")";
        case 7:
            return ((("max(" + formulaNotation(_arg.fields[0])) + ", ") + formulaNotation(_arg.fields[1])) + ")";
        case 8:
            return ((((("clamp(" + formulaNotation(_arg.fields[0])) + ", ") + formulaNotation(_arg.fields[1])) + ", ") + formulaNotation(_arg.fields[2])) + ")";
        case 9:
            return ((("(" + formulaNotation(_arg.fields[0])) + " <= ") + formulaNotation(_arg.fields[1])) + ")";
        case 10:
            return (((("if " + formulaNotation(_arg.fields[0])) + " then ") + formulaNotation(_arg.fields[1])) + " else ") + formulaNotation(_arg.fields[2]);
        default: {
            const value = _arg.fields[0];
            return (valueNotation(value) + " ") + value.Unit;
        }
    }
}

function semanticsProjection(_arg) {
    switch (_arg.tag) {
        case 1:
            return ("{\"type\":\"predicate\",\"notation\":" + jsonString(formulaNotation(_arg.fields[0]))) + "}";
        case 2:
            return ((((("{\"type\":\"formula\",\"kind\":" + jsonString(kindName(_arg.fields[0]))) + ",\"unit\":") + jsonString(_arg.fields[1])) + ",\"notation\":") + jsonString(formulaNotation(_arg.fields[2]))) + "}";
        case 3: {
            const contract = _arg.fields[0];
            return ((((((((("{\"type\":\"transition\",\"phase\":" + jsonString(contract.Phase)) + ",\"preconditions\":") + jsonArray((arg) => jsonString(RuleIdModule_value(arg)), contract.Preconditions)) + ",\"reads\":") + jsonArray(jsonString, contract.Reads)) + ",\"effects\":") + jsonArray(jsonString, contract.Effects)) + ",\"events\":") + jsonArray(jsonString, contract.Events)) + "}";
        }
        case 4: {
            const contract_1 = _arg.fields[0];
            return ((((((((((("{\"type\":\"algorithm\",\"symbol\":" + jsonString(contract_1.ImplementationSymbol)) + ",\"fingerprint\":") + jsonString(contract_1.Fingerprint)) + ",\"inputs\":") + jsonArray((tupledArg) => (((((("{\"name\":" + jsonString(tupledArg[0])) + ",\"kind\":") + jsonString(kindName(tupledArg[1]))) + ",\"unit\":") + jsonString(tupledArg[2])) + "}"), contract_1.Inputs)) + ",\"resultKind\":") + jsonString(kindName(contract_1.ResultKind))) + ",\"resultUnit\":") + jsonString(contract_1.ResultUnit)) + ",\"explanationFields\":") + jsonArray(jsonString, contract_1.ExplanationFields)) + "}";
        }
        case 5:
            return "{\"type\":\"narrative\"}";
        default: {
            const value = _arg.fields[0];
            return ((((("{\"type\":\"fact\",\"value\":" + jsonString(valueNotation(value))) + ",\"kind\":") + jsonString(kindName(value.DataKind))) + ",\"unit\":") + jsonString(value.Unit)) + "}";
        }
    }
}

export function manifestJson(identity, rules) {
    return ((((((((((((((((("{\"schemaVersion\":" + int32ToString(identity.SchemaVersion)) + ",\"engineIdentity\":") + jsonString(identity.EngineIdentity)) + ",\"compatibilityProfile\":") + jsonString(identity.CompatibilityProfile)) + ",\"packageVersion\":") + jsonString(identity.PackageVersion)) + ",\"sourceCommit\":") + jsonString(identity.SourceCommit)) + ",\"implementationDigest\":") + jsonString(hex(identity.ImplementationDigest))) + ",\"semanticDigest\":") + jsonString(hex(identity.SemanticDigest))) + ",\"manifestDigest\":") + jsonString(hex(identity.ManifestDigest))) + ",\"rules\":") + jsonArray((rule) => {
        let option_1, _arg, source;
        const metadata = rule.Metadata;
        const statement = ((((((("{\"preconditions\":" + jsonArray(jsonString, metadata.Statement.Preconditions)) + ",\"trigger\":") + defaultArg((option_1 = metadata.Statement.Trigger, (option_1 != null) ? jsonString(option_1) : undefined), "null")) + ",\"system\":") + jsonString(metadata.Statement.System)) + ",\"responses\":") + jsonArray(jsonString, metadata.Statement.Responses)) + "}";
        return ((((((((((((((((((((((((("{\"id\":" + jsonString(RuleIdModule_value(metadata.Id))) + ",\"title\":") + jsonString(metadata.Title)) + ",\"status\":") + jsonString(statusName(metadata.Status))) + ",\"kind\":") + jsonString(ruleKindName(metadata.SemanticKind))) + ",\"statement\":") + statement) + ",\"rationale\":") + jsonString(metadata.Rationale)) + ",\"dependencies\":") + jsonArray((arg) => jsonString(RuleIdModule_value(arg)), sortBy(RuleIdModule_value, metadata.Dependencies, {
            Compare: (x, y) => (comparePrimitives(x, y) | 0),
        }))) + ",\"supersedes\":") + jsonArray((arg_1) => jsonString(RuleIdModule_value(arg_1)), sortBy(RuleIdModule_value, metadata.Supersedes, {
            Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
        }))) + ",\"examples\":") + jsonArray(jsonString, metadata.Examples)) + ",\"properties\":") + jsonArray(jsonString, metadata.Properties)) + ",\"evidence\":") + jsonArray(jsonString, metadata.Evidence)) + ",\"source\":") + ((_arg = metadata.RuleSource, (_arg != null) ? ((source = _arg, ((((("{\"symbol\":" + jsonString(source.Symbol)) + ",\"path\":") + jsonString(source.RepositoryPath)) + ",\"commit\":") + jsonString(source.Commit)) + "}")) : "null"))) + ",\"explanationVocabulary\":[\"operands\",\"outcome\",\"children\",\"eventId\"],\"semantics\":") + semanticsProjection(rule.Semantics)) + "}";
    }, sortBy((rule_1) => RuleIdModule_value(rule_1.Metadata.Id), rules, {
        Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
    }))) + "}";
}

export function coverageJson(identity, rules) {
    const node = (kind, identity_1, authority) => (((((("{\"kind\":" + jsonString(kind)) + ",\"identity\":") + jsonString(identity_1)) + ",\"authority\":") + jsonString(authority)) + "}");
    const edge = (rule, target, kind_1) => (((((("{\"from\":" + jsonString("rule:" + RuleIdModule_value(rule.Metadata.Id))) + ",\"to\":") + jsonString(target)) + ",\"kind\":") + jsonString(kind_1)) + "}");
    const sortedRules = sortBy((rule_1) => RuleIdModule_value(rule_1.Metadata.Id), rules, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    const nodes = sort(List_distinct(collect((rule_2) => {
        let option_1, source;
        const id = RuleIdModule_value(rule_2.Metadata.Id);
        const sourceIdentity = defaultArg((option_1 = rule_2.Metadata.RuleSource, (option_1 != null) ? ((source = option_1, (source.RepositoryPath + "#") + source.Symbol)) : undefined), "unresolved");
        return toList(delay(() => append_1(singleton_1(node("rule", "rule:" + id, "Corpus")), delay(() => append_1(singleton_1(node("implementation", "implementation:" + sourceIdentity, "Corpus")), delay(() => append_1(singleton_1(node("event", ("event:" + id) + ":application", "Corpus")), delay(() => append_1(singleton_1(node("explanation", ("explanation:" + id) + ":derivation", "Corpus")), delay(() => append_1(map_2((example) => node("example/property", "example:" + example, "Corpus"), rule_2.Metadata.Examples), delay(() => append_1(map_2((property) => node("example/property", "property:" + property, "Corpus"), rule_2.Metadata.Properties), delay(() => append_1(singleton_1(node("documentation", "documentation:rules/" + id, "Corpus")), delay(() => append_1(singleton_1(node("source", "source:" + sourceIdentity, "Corpus")), delay(() => singleton_1(node("replay", "replay:tests/fixtures/rules-corpus/v1", "Corpus"))))))))))))))))))));
    }, sortedRules), {
        Equals: (x_1, y_1) => (x_1 === y_1),
        GetHashCode: (x_1) => (stringHash(x_1) | 0),
    }), {
        Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
    });
    const edges = sort(collect((rule_3) => {
        let option_4, source_1;
        const id_1 = RuleIdModule_value(rule_3.Metadata.Id);
        const sourceIdentity_1 = defaultArg((option_4 = rule_3.Metadata.RuleSource, (option_4 != null) ? ((source_1 = option_4, (source_1.RepositoryPath + "#") + source_1.Symbol)) : undefined), "unresolved");
        return toList(delay(() => append_1(map_2((dependency) => edge(rule_3, "rule:" + RuleIdModule_value(dependency), "dependency"), rule_3.Metadata.Dependencies), delay(() => append_1(singleton_1(edge(rule_3, "implementation:" + sourceIdentity_1, "implementation")), delay(() => append_1(singleton_1(edge(rule_3, ("event:" + id_1) + ":application", "event/application")), delay(() => append_1(singleton_1(edge(rule_3, ("explanation:" + id_1) + ":derivation", "explanation")), delay(() => append_1(map_2((example_1) => edge(rule_3, "example:" + example_1, "example"), rule_3.Metadata.Examples), delay(() => append_1(map_2((property_1) => edge(rule_3, "property:" + property_1, "property"), rule_3.Metadata.Properties), delay(() => append_1(singleton_1(edge(rule_3, "documentation:rules/" + id_1, "documentation")), delay(() => append_1(singleton_1(edge(rule_3, "source:" + sourceIdentity_1, "source")), delay(() => singleton_1(edge(rule_3, "replay:tests/fixtures/rules-corpus/v1", "replay"))))))))))))))))))));
    }, sortedRules), {
        Compare: (x_3, y_3) => (comparePrimitives(x_3, y_3) | 0),
    });
    return ((((("{\"schemaVersion\":1,\"packageManifestDigest\":" + jsonString(hex(identity.ManifestDigest))) + ",\"authorityBoundary\":{\"migrated\":\"first-combat-vertical-slice\",\"outside\":\"legacy\"},\"nodes\":[") + join(",", nodes)) + "],\"edges\":[") + join(",", edges)) + "]}";
}

export function canonicalApplicationBytes(application) {
    return concatenate([text(application.ApplicationId), text(RuleIdModule_value(application.RuleId)), list((tupledArg) => concatenate([text(tupledArg[0]), valueBytes(tupledArg[1])]), application.Operands), valueBytes(application.Outcome), list(canonicalApplicationBytes, application.Children), text(application.EventId), segment(application.PackageManifestDigest)]);
}

