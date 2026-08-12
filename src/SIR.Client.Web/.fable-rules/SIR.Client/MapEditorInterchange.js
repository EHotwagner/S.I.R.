
import { toString, Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { int32_type, bool_type, float64_type, class_type, array_type, option_type, record_type, string_type, union_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { MapEdgeKind, MapDefinition, MapDefinition_$reflection } from "./MapEditorTypes.js";
import { isDigit, isWhiteSpace } from "../fable_modules/fable-library-js.5.13.0/Char.js";
import { Exception, arrayHash, equalArrays, sign as sign_1, compareArrays, defaultOf, equals, round, comparePrimitives, stringHash, int32ToString } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { Result_Map, FSharpResult$2 } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { join, startsWith, substring } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { StringBuilder__Append_244C7CD6, StringBuilder_$ctor } from "../fable_modules/fable-library-js.5.13.0/System.Text.js";
import { append as append_1, exists, singleton as singleton_1, empty as empty_1, collect as collect_1, delay, tryFind, map, toArray } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { addRangeInPlace, indexed, map as map_2, item as item_1, equalsWith, mapIndexed, fold } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { defaultArg, value as value_11 } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { isInfinity } from "../fable_modules/fable-library-js.5.13.0/Double.js";
import { groupBy, distinct, countBy } from "../fable_modules/fable-library-js.5.13.0/Seq2.js";
import { ofArray as ofArray_1, empty, toList, FSharpMap__get_IsEmpty, tryFind as tryFind_1, ofSeq } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { ofSeq as ofSeq_1, iterate, empty as empty_2, append, toArray as toArray_1, isEmpty, filter, map as map_1, ofArray, concat, collect, singleton, fold as fold_1 } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { tryNormalizeEdge } from "./MapEditor.js";
import { rangeDouble } from "../fable_modules/fable-library-js.5.13.0/Range.js";
import { get_UTF8 } from "../fable_modules/fable-library-js.5.13.0/Encoding.js";

export class InterchangeFormat extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["UniversalVtt", "FoundryScene", "FantasyGroundsImage"];
    }
    static UniversalVtt = new InterchangeFormat(0, []);
    static FoundryScene = new InterchangeFormat(1, []);
    static FantasyGroundsImage = new InterchangeFormat(2, []);
}

export function InterchangeFormat_$reflection() {
    return union_type("SIR.Client.InterchangeFormat", [], InterchangeFormat, () => [[], [], []]);
}

export class InterchangeDisposition extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Mapped", "Ignored", "Lossy", "RejectedField"];
    }
    static Mapped = new InterchangeDisposition(0, []);
    static Ignored = new InterchangeDisposition(1, []);
    static Lossy = new InterchangeDisposition(2, []);
    static RejectedField = new InterchangeDisposition(3, []);
}

export function InterchangeDisposition_$reflection() {
    return union_type("SIR.Client.InterchangeDisposition", [], InterchangeDisposition, () => [[], [], [], []]);
}

export class InterchangeFieldReport extends Record {
    constructor(Path, Disposition, Meaning) {
        super();
        this.Path = Path;
        this.Disposition = Disposition;
        this.Meaning = Meaning;
    }
}

export function InterchangeFieldReport_$reflection() {
    return record_type("SIR.Client.InterchangeFieldReport", [], InterchangeFieldReport, () => [["Path", string_type], ["Disposition", InterchangeDisposition_$reflection()], ["Meaning", string_type]]);
}

export class InterchangeReview extends Record {
    constructor(Format, SourceName, Candidate, Fields, Errors) {
        super();
        this.Format = Format;
        this.SourceName = SourceName;
        this.Candidate = Candidate;
        this.Fields = Fields;
        this.Errors = Errors;
    }
}

export function InterchangeReview_$reflection() {
    return record_type("SIR.Client.InterchangeReview", [], InterchangeReview, () => [["Format", InterchangeFormat_$reflection()], ["SourceName", string_type], ["Candidate", option_type(MapDefinition_$reflection())], ["Fields", array_type(InterchangeFieldReport_$reflection())], ["Errors", array_type(string_type)]]);
}

class MapEditorInterchange_JsonValue extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["JObject", "JArray", "JString", "JNumber", "JBool", "JNull"];
    }
    static JNull = new MapEditorInterchange_JsonValue(5, []);
}

function MapEditorInterchange_JsonValue_$reflection() {
    return union_type("SIR.Client.MapEditorInterchange.JsonValue", [], MapEditorInterchange_JsonValue, () => [[["Item", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, MapEditorInterchange_JsonValue_$reflection()])]], [["Item", array_type(MapEditorInterchange_JsonValue_$reflection())]], [["Item", string_type]], [["Item", float64_type]], [["Item", bool_type]], []]);
}

class MapEditorInterchange_Parser extends Record {
    constructor(Text$, Offset) {
        super();
        this.Text = Text$;
        this.Offset = (Offset | 0);
    }
}

function MapEditorInterchange_Parser_$reflection() {
    return record_type("SIR.Client.MapEditorInterchange.Parser", [], MapEditorInterchange_Parser, () => [["Text", string_type], ["Offset", int32_type]]);
}

function MapEditorInterchange_skipWhitespace(parser) {
    while ((parser.Offset < parser.Text.length) && isWhiteSpace(parser.Text[parser.Offset])) {
        parser.Offset = ((parser.Offset + 1) | 0);
    }
}

function MapEditorInterchange_fail(parser, message) {
    return new FSharpResult$2(/* Error */ 1, [((message + " at character ") + int32ToString(parser.Offset)) + "."]);
}

function MapEditorInterchange_parseValue(parser) {
    let character;
    MapEditorInterchange_skipWhitespace(parser);
    if (parser.Offset >= parser.Text.length) {
        return MapEditorInterchange_fail(parser, "Unexpected end of JSON");
    }
    else {
        const matchValue = parser.Text[parser.Offset];
        switch (matchValue) {
            case "\"":
                return Result_Map((Item) => (new MapEditorInterchange_JsonValue(/* JString */ 2, [Item])), MapEditorInterchange_parseString(parser));
            case "[":
                return MapEditorInterchange_parseArray(parser);
            case "f":
                return MapEditorInterchange_parseLiteral(parser, "false", new MapEditorInterchange_JsonValue(/* JBool */ 4, [false]));
            case "n":
                return MapEditorInterchange_parseLiteral(parser, "null", MapEditorInterchange_JsonValue.JNull);
            case "t":
                return MapEditorInterchange_parseLiteral(parser, "true", new MapEditorInterchange_JsonValue(/* JBool */ 4, [true]));
            case "{":
                return MapEditorInterchange_parseObject(parser);
            default:
                if ((character = matchValue, (character === "-") ? true : isDigit(character))) {
                    return MapEditorInterchange_parseNumber(parser);
                }
                else {
                    return MapEditorInterchange_fail(parser, "Invalid JSON token");
                }
        }
    }
}

function MapEditorInterchange_parseLiteral(parser, literal, value) {
    if (((parser.Offset + literal.length) <= parser.Text.length) && (substring(parser.Text, parser.Offset, literal.length) === literal)) {
        parser.Offset = ((parser.Offset + literal.length) | 0);
        return new FSharpResult$2(/* Ok */ 0, [value]);
    }
    else {
        return MapEditorInterchange_fail(parser, "Expected " + literal);
    }
}

function MapEditorInterchange_parseString(parser) {
    if (parser.Text[parser.Offset] !== "\"") {
        return MapEditorInterchange_fail(parser, "Expected string");
    }
    else {
        parser.Offset = ((parser.Offset + 1) | 0);
        const buffer = StringBuilder_$ctor();
        let complete = false;
        let error = undefined;
        while ((!complete && (error == null)) && (parser.Offset < parser.Text.length)) {
            const character = parser.Text[parser.Offset];
            parser.Offset = ((parser.Offset + 1) | 0);
            let matchResult, character_4, character_5;
            switch (character) {
                case "\"": {
                    matchResult = 0;
                    break;
                }
                case "\\": {
                    if (parser.Offset < parser.Text.length) {
                        matchResult = 1;
                    }
                    else if (~~character.charCodeAt(0) < 32) {
                        matchResult = 2;
                        character_4 = character;
                    }
                    else {
                        matchResult = 3;
                        character_5 = character;
                    }
                    break;
                }
                default:
                    if (~~character.charCodeAt(0) < 32) {
                        matchResult = 2;
                        character_4 = character;
                    }
                    else {
                        matchResult = 3;
                        character_5 = character;
                    }
            }
            switch (matchResult) {
                case 0: {
                    complete = true;
                    break;
                }
                case 1: {
                    const escaped = parser.Text[parser.Offset];
                    parser.Offset = ((parser.Offset + 1) | 0);
                    let matchResult_1;
                    switch (escaped) {
                        case "\"": {
                            matchResult_1 = 0;
                            break;
                        }
                        case "/": {
                            matchResult_1 = 2;
                            break;
                        }
                        case "\\": {
                            matchResult_1 = 1;
                            break;
                        }
                        case "b": {
                            matchResult_1 = 3;
                            break;
                        }
                        case "f": {
                            matchResult_1 = 4;
                            break;
                        }
                        case "n": {
                            matchResult_1 = 5;
                            break;
                        }
                        case "r": {
                            matchResult_1 = 6;
                            break;
                        }
                        case "t": {
                            matchResult_1 = 7;
                            break;
                        }
                        case "u": {
                            if ((parser.Offset + 4) <= parser.Text.length) {
                                matchResult_1 = 8;
                            }
                            else {
                                matchResult_1 = 9;
                            }
                            break;
                        }
                        default:
                            matchResult_1 = 9;
                    }
                    switch (matchResult_1) {
                        case 0: {
                            StringBuilder__Append_244C7CD6(buffer, "\"");
                            break;
                        }
                        case 1: {
                            StringBuilder__Append_244C7CD6(buffer, "\\");
                            break;
                        }
                        case 2: {
                            StringBuilder__Append_244C7CD6(buffer, "/");
                            break;
                        }
                        case 3: {
                            StringBuilder__Append_244C7CD6(buffer, "\b");
                            break;
                        }
                        case 4: {
                            StringBuilder__Append_244C7CD6(buffer, "\f");
                            break;
                        }
                        case 5: {
                            StringBuilder__Append_244C7CD6(buffer, "\n");
                            break;
                        }
                        case 6: {
                            StringBuilder__Append_244C7CD6(buffer, "\r");
                            break;
                        }
                        case 7: {
                            StringBuilder__Append_244C7CD6(buffer, "\t");
                            break;
                        }
                        case 8: {
                            const digits = toArray(map((character_3) => {
                                if ((character_3 >= "0") && (character_3 <= "9")) {
                                    return ~~character_3.charCodeAt(0) - ~~"0".charCodeAt(0);
                                }
                                else if ((character_3 >= "a") && (character_3 <= "f")) {
                                    return (10 + ~~character_3.charCodeAt(0)) - ~~"a".charCodeAt(0);
                                }
                                else if ((character_3 >= "A") && (character_3 <= "F")) {
                                    return (10 + ~~character_3.charCodeAt(0)) - ~~"A".charCodeAt(0);
                                }
                                else {
                                    return undefined;
                                }
                            }, substring(parser.Text, parser.Offset, 4).split("")));
                            if (digits.every((option) => (option != null))) {
                                const value_8 = fold((total, digit) => (((total * 16) + value_11(digit)) | 0), 0, digits) | 0;
                                StringBuilder__Append_244C7CD6(buffer, String.fromCharCode(value_8 & 0xFFFF));
                                parser.Offset = ((parser.Offset + 4) | 0);
                            }
                            else {
                                error = "Invalid JSON unicode escape";
                            }
                            break;
                        }
                        case 9: {
                            error = "Invalid JSON escape";
                            break;
                        }
                    }
                    break;
                }
                case 2: {
                    error = "Control character in JSON string";
                    break;
                }
                case 3: {
                    StringBuilder__Append_244C7CD6(buffer, character_5);
                    break;
                }
            }
        }
        if (error == null) {
            if (!complete) {
                return MapEditorInterchange_fail(parser, "Unterminated JSON string");
            }
            else {
                return new FSharpResult$2(/* Ok */ 0, [toString(buffer)]);
            }
        }
        else {
            return MapEditorInterchange_fail(parser, error);
        }
    }
}

function MapEditorInterchange_parseNumber(parser) {
    let character;
    const start = parser.Offset | 0;
    while ((parser.Offset < parser.Text.length) && ((character = parser.Text[parser.Offset], ((((isDigit(character) ? true : (character === "-")) ? true : (character === "+")) ? true : (character === ".")) ? true : (character === "e")) ? true : (character === "E")))) {
        parser.Offset = ((parser.Offset + 1) | 0);
    }
    const token = substring(parser.Text, start, parser.Offset - start);
    let index = 0;
    let sign = 1;
    if ((index < token.length) && (token[index] === "-")) {
        sign = -1;
        index = ((index + 1) | 0);
    }
    const integerStart = index | 0;
    if ((index < token.length) && (token[index] === "0")) {
        index = ((index + 1) | 0);
    }
    else {
        while ((index < token.length) && isDigit(token[index])) {
            index = ((index + 1) | 0);
        }
    }
    let value = 0;
    for (let digitIndex = integerStart; digitIndex <= (index - 1); digitIndex++) {
        value = ((value * 10) + (~~token[digitIndex].charCodeAt(0) - ~~"0".charCodeAt(0)));
    }
    let valid = index > integerStart;
    if ((index < token.length) && (token[index] === ".")) {
        index = ((index + 1) | 0);
        const fractionStart = index | 0;
        let place = 0.1;
        while ((index < token.length) && isDigit(token[index])) {
            value = (value + ((~~token[index].charCodeAt(0) - ~~"0".charCodeAt(0)) * place));
            place = (place * 0.1);
            index = ((index + 1) | 0);
        }
        valid = (valid && (index > fractionStart));
    }
    if ((index < token.length) && ((token[index] === "e") ? true : (token[index] === "E"))) {
        index = ((index + 1) | 0);
        let exponentSign = 1;
        if ((index < token.length) && ((token[index] === "+") ? true : (token[index] === "-"))) {
            if (token[index] === "-") {
                exponentSign = -1;
            }
            index = ((index + 1) | 0);
        }
        const exponentStart = index | 0;
        let exponent = 0;
        while ((index < token.length) && isDigit(token[index])) {
            exponent = ((((exponent * 10) + ~~token[index].charCodeAt(0)) - ~~"0".charCodeAt(0)) | 0);
            index = ((index + 1) | 0);
        }
        valid = (valid && (index > exponentStart));
        value = (value * Math.pow(10, exponentSign * exponent));
    }
    value = (value * sign);
    if ((valid && (index === token.length)) && !(Number.isNaN(value) ? true : isInfinity(value))) {
        return new FSharpResult$2(/* Ok */ 0, [new MapEditorInterchange_JsonValue(/* JNumber */ 3, [value])]);
    }
    else {
        return MapEditorInterchange_fail(parser, "Invalid JSON number");
    }
}

function MapEditorInterchange_parseArray(parser) {
    parser.Offset = ((parser.Offset + 1) | 0);
    MapEditorInterchange_skipWhitespace(parser);
    const values = [];
    let complete = false;
    let error = undefined;
    if ((parser.Offset < parser.Text.length) && (parser.Text[parser.Offset] === "]")) {
        parser.Offset = ((parser.Offset + 1) | 0);
        complete = true;
    }
    while (!complete && (error == null)) {
        const matchValue = MapEditorInterchange_parseValue(parser);
        if (matchValue.tag === 0) {
            void (values.push(matchValue.fields[0]));
            MapEditorInterchange_skipWhitespace(parser);
            if (parser.Offset >= parser.Text.length) {
                error = "Unterminated JSON array.";
            }
            else if (parser.Text[parser.Offset] === "]") {
                parser.Offset = ((parser.Offset + 1) | 0);
                complete = true;
            }
            else if (parser.Text[parser.Offset] === ",") {
                parser.Offset = ((parser.Offset + 1) | 0);
            }
            else {
                error = (("Expected comma in JSON array at character " + int32ToString(parser.Offset)) + ".");
            }
        }
        else {
            error = matchValue.fields[0];
        }
    }
    if (error == null) {
        return new FSharpResult$2(/* Ok */ 0, [new MapEditorInterchange_JsonValue(/* JArray */ 1, [values.slice()])]);
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [error]);
    }
}

function MapEditorInterchange_parseObject(parser) {
    parser.Offset = ((parser.Offset + 1) | 0);
    MapEditorInterchange_skipWhitespace(parser);
    const values = [];
    let complete = false;
    let error = undefined;
    if ((parser.Offset < parser.Text.length) && (parser.Text[parser.Offset] === "}")) {
        parser.Offset = ((parser.Offset + 1) | 0);
        complete = true;
    }
    while (!complete && (error == null)) {
        MapEditorInterchange_skipWhitespace(parser);
        const matchValue = MapEditorInterchange_parseString(parser);
        if (matchValue.tag === 0) {
            MapEditorInterchange_skipWhitespace(parser);
            if ((parser.Offset >= parser.Text.length) ? true : (parser.Text[parser.Offset] !== ":")) {
                error = (("Expected colon in JSON object at character " + int32ToString(parser.Offset)) + ".");
            }
            else {
                parser.Offset = ((parser.Offset + 1) | 0);
                const matchValue_1 = MapEditorInterchange_parseValue(parser);
                if (matchValue_1.tag === 0) {
                    void (values.push([matchValue.fields[0], matchValue_1.fields[0]]));
                    MapEditorInterchange_skipWhitespace(parser);
                    if (parser.Offset >= parser.Text.length) {
                        error = "Unterminated JSON object.";
                    }
                    else if (parser.Text[parser.Offset] === "}") {
                        parser.Offset = ((parser.Offset + 1) | 0);
                        complete = true;
                    }
                    else if (parser.Text[parser.Offset] === ",") {
                        parser.Offset = ((parser.Offset + 1) | 0);
                    }
                    else {
                        error = (("Expected comma in JSON object at character " + int32ToString(parser.Offset)) + ".");
                    }
                }
                else {
                    error = matchValue_1.fields[0];
                }
            }
        }
        else {
            error = matchValue.fields[0];
        }
    }
    if (error == null) {
        const duplicate = tryFind((tupledArg) => (tupledArg[1] > 1), countBy((tuple) => tuple[0], values, {
            Equals: (x, y) => (x === y),
            GetHashCode: (x) => (stringHash(x) | 0),
        }));
        if (duplicate == null) {
            return new FSharpResult$2(/* Ok */ 0, [new MapEditorInterchange_JsonValue(/* JObject */ 0, [ofSeq(values, {
                Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
            })])]);
        }
        else {
            return new FSharpResult$2(/* Error */ 1, [("Duplicate JSON field \'" + duplicate[0]) + "\' is ambiguous."]);
        }
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [error]);
    }
}

function MapEditorInterchange_parseJson(text) {
    const parser = new MapEditorInterchange_Parser(text, 0);
    const matchValue = MapEditorInterchange_parseValue(parser);
    if (matchValue.tag === 0) {
        MapEditorInterchange_skipWhitespace(parser);
        if (parser.Offset !== text.length) {
            return MapEditorInterchange_fail(parser, "Trailing JSON content");
        }
        else {
            return new FSharpResult$2(/* Ok */ 0, [matchValue.fields[0]]);
        }
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [matchValue.fields[0]]);
    }
}

function MapEditorInterchange_property(name, _arg) {
    if (_arg.tag === 0) {
        return tryFind_1(name, _arg.fields[0]);
    }
    else {
        return undefined;
    }
}

function MapEditorInterchange_numberAt(path, value) {
    const option_3 = fold_1((current, name) => {
        const option_1 = current;
        if (option_1 != null) {
            return MapEditorInterchange_property(name, option_1);
        }
        else {
            return undefined;
        }
    }, value, path);
    if (option_3 != null) {
        const _arg_1 = option_3;
        if (_arg_1.tag === 3) {
            return _arg_1.fields[0];
        }
        else {
            return undefined;
        }
    }
    else {
        return undefined;
    }
}

function MapEditorInterchange_arrayAt(path, value) {
    const option_3 = fold_1((current, name) => {
        const option_1 = current;
        if (option_1 != null) {
            return MapEditorInterchange_property(name, option_1);
        }
        else {
            return undefined;
        }
    }, value, path);
    if (option_3 != null) {
        const _arg_1 = option_3;
        if (_arg_1.tag === 1) {
            return _arg_1.fields[0];
        }
        else {
            return undefined;
        }
    }
    else {
        return undefined;
    }
}

function MapEditorInterchange_integer(value) {
    const rounded = round(value);
    if (((Math.abs(value - rounded) < 1E-06) && (rounded >= -2147483648)) && (rounded <= 2147483647)) {
        return ~~rounded;
    }
    else {
        return undefined;
    }
}

function MapEditorInterchange_leafPaths(prefix, value) {
    switch (value.tag) {
        case 0:
            if (FSharpMap__get_IsEmpty(value.fields[0])) {
                return singleton(prefix);
            }
            else {
                return collect((tupledArg) => {
                    const name = tupledArg[0];
                    return MapEditorInterchange_leafPaths((prefix === "") ? name : ((prefix + ".") + name), tupledArg[1]);
                }, toList(value.fields[0]));
            }
        case 1:
            if (value.fields[0].length === 0) {
                return singleton(prefix);
            }
            else {
                return concat(ofArray(mapIndexed((index, child_1) => MapEditorInterchange_leafPaths(((prefix + "[") + int32ToString(index)) + "]", child_1), value.fields[0])));
            }
        default:
            return singleton(prefix);
    }
}

function MapEditorInterchange_point(_arg) {
    let testExpr;
    let matchResult, fields, x_2, y_2;
    switch (_arg.tag) {
        case 0: {
            matchResult = 0;
            fields = _arg.fields[0];
            break;
        }
        case 1: {
            if ((testExpr = _arg.fields[0], !equalsWith(equals, testExpr, defaultOf()) && (testExpr.length === 2))) {
                if (item_1(0, _arg.fields[0]).tag === 3) {
                    if (item_1(1, _arg.fields[0]).tag === 3) {
                        matchResult = 1;
                        x_2 = item_1(0, _arg.fields[0]).fields[0];
                        y_2 = item_1(1, _arg.fields[0]).fields[0];
                    }
                    else {
                        matchResult = 2;
                    }
                }
                else {
                    matchResult = 2;
                }
            }
            else {
                matchResult = 2;
            }
            break;
        }
        default:
            matchResult = 2;
    }
    switch (matchResult) {
        case 0: {
            const matchValue = tryFind_1("x", fields);
            const matchValue_1 = tryFind_1("y", fields);
            let matchResult_1, x_1, y_1;
            if (matchValue != null) {
                if (matchValue.tag === 3) {
                    if (matchValue_1 != null) {
                        if (matchValue_1.tag === 3) {
                            matchResult_1 = 0;
                            x_1 = matchValue.fields[0];
                            y_1 = matchValue_1.fields[0];
                        }
                        else {
                            matchResult_1 = 1;
                        }
                    }
                    else {
                        matchResult_1 = 1;
                    }
                }
                else {
                    matchResult_1 = 1;
                }
            }
            else {
                matchResult_1 = 1;
            }
            switch (matchResult_1) {
                case 0:
                    return [x_1, y_1];
                default:
                    return undefined;
            }
        }
        case 1:
            return [x_2, y_2];
        default:
            return undefined;
    }
}

function MapEditorInterchange_blankMap(width, height) {
    return new MapDefinition(width, height, empty({
        Compare: (x, y) => (compareArrays(x, y) | 0),
    }), empty({
        Compare: (x_1, y_1) => (compareArrays(x_1, y_1) | 0),
    }), empty({
        Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
    }), 1, empty({
        Compare: (x_3, y_3) => (comparePrimitives(x_3, y_3) | 0),
    }), 1);
}

function MapEditorInterchange_splitGridSegment(width, height, pixelsPerGrid, originX, originY, kind, isOpen, first_, first__1, second_, second__1) {
    const first = [first_, first__1];
    const second = [second_, second__1];
    const gridCoordinate = (x, origin) => ((x - origin) / pixelsPerGrid);
    const matchValue = gridCoordinate(first[0], originX);
    const matchValue_1 = gridCoordinate(first[1], originY);
    const matchValue_2 = gridCoordinate(second[0], originX);
    const matchValue_3 = gridCoordinate(second[1], originY);
    const matchValue_4 = MapEditorInterchange_integer(matchValue);
    const matchValue_5 = MapEditorInterchange_integer(matchValue_1);
    const matchValue_6 = MapEditorInterchange_integer(matchValue_2);
    const matchValue_7 = MapEditorInterchange_integer(matchValue_3);
    let matchResult, x1_2, x2_2, y1_2, y2_2;
    if (matchValue_4 != null) {
        if (matchValue_5 != null) {
            if (matchValue_6 != null) {
                if (matchValue_7 != null) {
                    if ((matchValue_4 === matchValue_6) !== (matchValue_5 === matchValue_7)) {
                        matchResult = 0;
                        x1_2 = matchValue_4;
                        x2_2 = matchValue_6;
                        y1_2 = matchValue_5;
                        y2_2 = matchValue_7;
                    }
                    else {
                        matchResult = 1;
                    }
                }
                else {
                    matchResult = 1;
                }
            }
            else {
                matchResult = 1;
            }
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
            const length = (Math.abs(x2_2 - x1_2) + Math.abs(y2_2 - y1_2)) | 0;
            const mapped = toArray(delay(() => collect_1((step) => {
                const sx = (x1_2 + (step * sign_1(x2_2 - x1_2))) | 0;
                const sy = (y1_2 + (step * sign_1(y2_2 - y1_2))) | 0;
                const matchValue_9 = tryNormalizeEdge(width, height, sx, sy, sx + sign_1(x2_2 - x1_2), sy + sign_1(y2_2 - y1_2));
                if (matchValue_9 == null) {
                    return empty_1();
                }
                else {
                    return singleton_1([matchValue_9, [kind, isOpen]]);
                }
            }, rangeDouble(0, 1, length - 1))));
            return [mapped, (mapped.length === length) ? undefined : "segment includes map-border geometry that S.I.R.\'s east/south edge records cannot represent"];
        }
        default:
            return [[], "segment is diagonal, curved, off-grid, or not representable on the top/left map border"];
    }
}

function MapEditorInterchange_ignoredLeaves(root, consumed) {
    return map_1((path_1) => (new InterchangeFieldReport(path_1, InterchangeDisposition.Ignored, "No authoritative S.I.R. semantic mapping; retained only in the source file.")), filter((path) => !exists((prefix) => {
        if ((path === prefix) ? true : startsWith(path, prefix + ".", 4)) {
            return true;
        }
        else {
            return startsWith(path, prefix + "[", 4);
        }
    }, consumed), MapEditorInterchange_leafPaths("", root)));
}

function MapEditorInterchange_finish(format, sourceName, root, dimensions, edges, reports, consumed, errors) {
    let bind$0040;
    let candidate;
    let matchResult, height, width;
    if (dimensions != null) {
        if (isEmpty(errors)) {
            matchResult = 0;
            height = dimensions[1];
            width = dimensions[0];
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
            const edgeMap = toArray(map((tupledArg) => [tupledArg[0], toArray(distinct(map((tuple_1) => tuple_1[1], tupledArg[1]), {
                Equals: equalArrays,
                GetHashCode: (x_1) => (arrayHash(x_1) | 0),
            }))], groupBy((tuple) => tuple[0], edges, {
                Equals: equalArrays,
                GetHashCode: (x) => (arrayHash(x) | 0),
            })));
            candidate = (edgeMap.some((tupledArg_1) => (tupledArg_1[1].length !== 1)) ? undefined : ((bind$0040 = MapEditorInterchange_blankMap(width, height), new MapDefinition(bind$0040.Width, bind$0040.Height, bind$0040.Terrain, ofArray_1(map_2((tupledArg_2) => [tupledArg_2[0], item_1(0, tupledArg_2[1])], edgeMap), {
                Compare: (x_2, y_2) => (compareArrays(x_2, y_2) | 0),
            }), bind$0040.Units, bind$0040.NextUnitId, bind$0040.Regions, bind$0040.NextRegionId))));
            break;
        }
        default:
            candidate = undefined;
    }
    return new InterchangeReview(format, sourceName, candidate, toArray_1(append(reports, MapEditorInterchange_ignoredLeaves(root, consumed))), Array.from(delay(() => append_1(errors, delay(() => (((candidate == null) && isEmpty(errors)) ? singleton_1("Conflicting duplicate semantic edges prevent deterministic import.") : empty_1()))))));
}

function MapEditorInterchange_evaluateUniversal(sourceName, root) {
    let w, pixels, h, h_1, pixels_1, w_1;
    let width;
    const option_1 = MapEditorInterchange_numberAt(ofArray(["resolution", "map_size", "x"]), root);
    width = ((option_1 != null) ? MapEditorInterchange_integer(option_1) : undefined);
    let height;
    const option_3 = MapEditorInterchange_numberAt(ofArray(["resolution", "map_size", "y"]), root);
    height = ((option_3 != null) ? MapEditorInterchange_integer(option_3) : undefined);
    const scale = MapEditorInterchange_numberAt(ofArray(["resolution", "pixels_per_grid"]), root);
    const originX = defaultArg(MapEditorInterchange_numberAt(ofArray(["resolution", "map_origin", "x"]), root), 0);
    const originY = defaultArg(MapEditorInterchange_numberAt(ofArray(["resolution", "map_origin", "y"]), root), 0);
    const dimensions = (width != null) ? ((height != null) ? ((scale != null) ? (((w = (width | 0), (pixels = scale, (h = (height | 0), ((((w >= 4) && (h >= 4)) && (w <= 40)) && (h <= 40)) && (pixels > 0))))) ? ((h_1 = (height | 0), (pixels_1 = scale, (w_1 = (width | 0), [w_1, h_1])))) : undefined) : undefined) : undefined) : undefined;
    const errors = (dimensions == null) ? singleton("UVTT resolution must provide integral map_size 4–40 and positive pixels_per_grid.") : empty_2();
    const consumed = [];
    iterate((item) => {
        void (consumed.push(item));
    }, ofArray(["resolution.map_size.x", "resolution.map_size.y", "resolution.pixels_per_grid", "resolution.map_origin.x", "resolution.map_origin.y"]));
    const reports = [];
    void (reports.push(new InterchangeFieldReport("resolution", InterchangeDisposition.Mapped, "Map size and pixel-to-cell transform.")));
    const edges = [];
    const matchValue_1 = MapEditorInterchange_arrayAt(singleton("line_of_sight"), root);
    let matchResult, h_2, pixels_2, polylines, w_2;
    if (dimensions != null) {
        if (scale != null) {
            if (matchValue_1 != null) {
                matchResult = 0;
                h_2 = dimensions[1];
                pixels_2 = scale;
                polylines = matchValue_1;
                w_2 = dimensions[0];
            }
            else {
                matchResult = 1;
            }
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
            const arr = indexed(polylines);
            for (let idx = 0; idx <= (arr.length - 1); idx++) {
                const forLoopVar = item_1(idx, arr);
                const line = forLoopVar[1];
                const path = ("line_of_sight[" + int32ToString(forLoopVar[0])) + "]";
                if (line.tag === 1) {
                    const points = line.fields[0];
                    for (let pointIndex = 0; pointIndex <= (points.length - 1); pointIndex++) {
                        void (consumed.push(((path + "[") + int32ToString(pointIndex)) + "].x"));
                        void (consumed.push(((path + "[") + int32ToString(pointIndex)) + "].y"));
                        void (consumed.push(((path + "[") + int32ToString(pointIndex)) + "][0]"));
                        void (consumed.push(((path + "[") + int32ToString(pointIndex)) + "][1]"));
                    }
                    for (let segmentIndex = 0; segmentIndex <= (points.length - 2); segmentIndex++) {
                        const matchValue_3 = MapEditorInterchange_point(item_1(segmentIndex, points));
                        const matchValue_4 = MapEditorInterchange_point(item_1(segmentIndex + 1, points));
                        let matchResult_1, first, second;
                        if (matchValue_3 != null) {
                            if (matchValue_4 != null) {
                                matchResult_1 = 0;
                                first = matchValue_3;
                                second = matchValue_4;
                            }
                            else {
                                matchResult_1 = 1;
                            }
                        }
                        else {
                            matchResult_1 = 1;
                        }
                        switch (matchResult_1) {
                            case 0: {
                                const patternInput = MapEditorInterchange_splitGridSegment(w_2, h_2, pixels_2, originX, originY, MapEdgeKind.Wall, false, first[0], first[1], second[0], second[1]);
                                const loss = patternInput[1];
                                addRangeInPlace(patternInput[0], edges);
                                void (reports.push(new InterchangeFieldReport(((((path + "[") + int32ToString(segmentIndex)) + "..") + int32ToString(segmentIndex + 1)) + "]", (loss == null) ? InterchangeDisposition.Mapped : InterchangeDisposition.Lossy, defaultArg(loss, "Axis-aligned grid segment mapped to closed S.I.R. wall edges."))));
                                break;
                            }
                            case 1: {
                                void (reports.push(new InterchangeFieldReport(path, InterchangeDisposition.Lossy, "Malformed line-of-sight point was not imported.")));
                                break;
                            }
                        }
                    }
                }
                else {
                    void (reports.push(new InterchangeFieldReport(path, InterchangeDisposition.Lossy, "Line-of-sight entry is not a point array.")));
                }
            }
            break;
        }
    }
    const matchValue_6 = MapEditorInterchange_arrayAt(singleton("portals"), root);
    let matchResult_2, h_3, pixels_3, portals, w_3;
    if (dimensions != null) {
        if (scale != null) {
            if (matchValue_6 != null) {
                matchResult_2 = 0;
                h_3 = dimensions[1];
                pixels_3 = scale;
                portals = matchValue_6;
                w_3 = dimensions[0];
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
        case 0: {
            const arr_1 = indexed(portals);
            for (let idx_1 = 0; idx_1 <= (arr_1.length - 1); idx_1++) {
                let option_10, _arg_1, testExpr;
                const forLoopVar_1 = item_1(idx_1, arr_1);
                const portal = forLoopVar_1[1];
                const path_1 = ("portals[" + int32ToString(forLoopVar_1[0])) + "]";
                void (consumed.push(path_1 + ".closed"));
                let bounds;
                const option_8 = MapEditorInterchange_property("bounds", portal);
                if (option_8 != null) {
                    const _arg = option_8;
                    bounds = ((_arg.tag === 1) ? _arg.fields[0] : undefined);
                }
                else {
                    bounds = undefined;
                }
                const closed = defaultArg((option_10 = MapEditorInterchange_property("closed", portal), (option_10 != null) ? ((_arg_1 = option_10, (_arg_1.tag === 4) ? _arg_1.fields[0] : undefined)) : undefined), true);
                let matchResult_3, firstValue, secondValue;
                if (bounds != null) {
                    if ((testExpr = bounds, !equalsWith(equals, testExpr, defaultOf()) && (testExpr.length === 2))) {
                        matchResult_3 = 0;
                        firstValue = item_1(0, bounds);
                        secondValue = item_1(1, bounds);
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
                        for (let pointIndex_1 = 0; pointIndex_1 <= 1; pointIndex_1++) {
                            void (consumed.push(((path_1 + ".bounds[") + int32ToString(pointIndex_1)) + "].x"));
                            void (consumed.push(((path_1 + ".bounds[") + int32ToString(pointIndex_1)) + "].y"));
                            void (consumed.push(((path_1 + ".bounds[") + int32ToString(pointIndex_1)) + "][0]"));
                            void (consumed.push(((path_1 + ".bounds[") + int32ToString(pointIndex_1)) + "][1]"));
                        }
                        const matchValue_8 = MapEditorInterchange_point(firstValue);
                        const matchValue_9 = MapEditorInterchange_point(secondValue);
                        let matchResult_4, first_1, second_1;
                        if (matchValue_8 != null) {
                            if (matchValue_9 != null) {
                                matchResult_4 = 0;
                                first_1 = matchValue_8;
                                second_1 = matchValue_9;
                            }
                            else {
                                matchResult_4 = 1;
                            }
                        }
                        else {
                            matchResult_4 = 1;
                        }
                        switch (matchResult_4) {
                            case 0: {
                                const patternInput_1 = MapEditorInterchange_splitGridSegment(w_3, h_3, pixels_3, originX, originY, MapEdgeKind.Door, !closed, first_1[0], first_1[1], second_1[0], second_1[1]);
                                const loss_1 = patternInput_1[1];
                                addRangeInPlace(patternInput_1[0], edges);
                                void (reports.push(new InterchangeFieldReport(path_1, (loss_1 == null) ? InterchangeDisposition.Mapped : InterchangeDisposition.Lossy, defaultArg(loss_1, "Portal mapped to semantic door edge(s), preserving open/closed state."))));
                                break;
                            }
                            case 1: {
                                void (reports.push(new InterchangeFieldReport(path_1, InterchangeDisposition.Lossy, "Portal bounds were malformed.")));
                                break;
                            }
                        }
                        break;
                    }
                    case 1: {
                        void (reports.push(new InterchangeFieldReport(path_1, InterchangeDisposition.Lossy, "Portal without exactly two bounds points was ignored.")));
                        break;
                    }
                }
            }
            break;
        }
    }
    return MapEditorInterchange_finish(InterchangeFormat.UniversalVtt, sourceName, root, dimensions, edges, ofSeq_1(reports), consumed, errors);
}

function MapEditorInterchange_evaluateFoundry(sourceName, root) {
    let option_3, w, h, size;
    const pixelWidth = MapEditorInterchange_numberAt(singleton("width"), root);
    const pixelHeight = MapEditorInterchange_numberAt(singleton("height"), root);
    let gridSize;
    const option_1 = MapEditorInterchange_numberAt(ofArray(["grid", "size"]), root);
    gridSize = ((option_1 != null) ? option_1 : MapEditorInterchange_numberAt(singleton("grid"), root));
    const gridType = defaultArg((option_3 = MapEditorInterchange_numberAt(ofArray(["grid", "type"]), root), (option_3 != null) ? option_3 : MapEditorInterchange_numberAt(singleton("gridType"), root)), 1);
    let dimensions;
    let matchResult, ph_1, pw_1, size_1;
    if (pixelWidth != null) {
        if (pixelHeight != null) {
            if (gridSize != null) {
                if ((size = gridSize, (gridType === 1) && (size > 0))) {
                    matchResult = 0;
                    ph_1 = pixelHeight;
                    pw_1 = pixelWidth;
                    size_1 = gridSize;
                }
                else {
                    matchResult = 1;
                }
            }
            else {
                matchResult = 1;
            }
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
            const matchValue_1 = MapEditorInterchange_integer(pw_1 / size_1);
            const matchValue_2 = MapEditorInterchange_integer(ph_1 / size_1);
            let matchResult_1, h_1, w_1;
            if (matchValue_1 != null) {
                if (matchValue_2 != null) {
                    if ((w = (matchValue_1 | 0), (h = (matchValue_2 | 0), (((w >= 4) && (h >= 4)) && (w <= 40)) && (h <= 40)))) {
                        matchResult_1 = 0;
                        h_1 = matchValue_2;
                        w_1 = matchValue_1;
                    }
                    else {
                        matchResult_1 = 1;
                    }
                }
                else {
                    matchResult_1 = 1;
                }
            }
            else {
                matchResult_1 = 1;
            }
            switch (matchResult_1) {
                case 0: {
                    dimensions = [w_1, h_1];
                    break;
                }
                default:
                    dimensions = undefined;
            }
            break;
        }
        default:
            dimensions = undefined;
    }
    const errors = (dimensions == null) ? singleton("Foundry scene must use a square grid and resolve to an integral 4–40-cell board.") : empty_2();
    const consumed = [];
    iterate((item) => {
        void (consumed.push(item));
    }, ofArray(["width", "height", "grid.size", "grid.type", "gridType"]));
    const matchValue_4 = MapEditorInterchange_property("grid", root);
    let matchResult_2;
    if (matchValue_4 != null) {
        if (matchValue_4.tag === 3) {
            matchResult_2 = 0;
        }
        else {
            matchResult_2 = 1;
        }
    }
    else {
        matchResult_2 = 1;
    }
    switch (matchResult_2) {
        case 0: {
            void (consumed.push("grid"));
            break;
        }
    }
    const reports = [];
    void (reports.push(new InterchangeFieldReport("width,height,grid", InterchangeDisposition.Mapped, "Square-grid scene extent mapped to S.I.R. cells.")));
    const edges = [];
    const matchValue_5 = MapEditorInterchange_arrayAt(singleton("walls"), root);
    let matchResult_3, h_2, pixels, w_2, walls;
    if (dimensions != null) {
        if (gridSize != null) {
            if (matchValue_5 != null) {
                matchResult_3 = 0;
                h_2 = dimensions[1];
                pixels = gridSize;
                w_2 = dimensions[0];
                walls = matchValue_5;
            }
            else {
                matchResult_3 = 1;
            }
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
            const arr = indexed(walls);
            for (let idx = 0; idx <= (arr.length - 1); idx++) {
                let option_8, _arg_1, option_11, _arg_2, testExpr;
                const forLoopVar = item_1(idx, arr);
                const wall = forLoopVar[1];
                const path = ("walls[" + int32ToString(forLoopVar[0])) + "]";
                void (consumed.push(path + ".c"));
                void (consumed.push(path + ".door"));
                void (consumed.push(path + ".ds"));
                let coordinates;
                const option_6 = MapEditorInterchange_property("c", wall);
                if (option_6 != null) {
                    const _arg = option_6;
                    coordinates = ((_arg.tag === 1) ? _arg.fields[0] : undefined);
                }
                else {
                    coordinates = undefined;
                }
                const doorCode = defaultArg((option_8 = MapEditorInterchange_property("door", wall), (option_8 != null) ? ((_arg_1 = option_8, (_arg_1.tag === 3) ? MapEditorInterchange_integer(_arg_1.fields[0]) : undefined)) : undefined), 0) | 0;
                const stateCode = defaultArg((option_11 = MapEditorInterchange_property("ds", wall), (option_11 != null) ? ((_arg_2 = option_11, (_arg_2.tag === 3) ? MapEditorInterchange_integer(_arg_2.fields[0]) : undefined)) : undefined), 0) | 0;
                let matchResult_4, x1, x2, y1, y2;
                if (coordinates != null) {
                    if ((testExpr = coordinates, !equalsWith(equals, testExpr, defaultOf()) && (testExpr.length === 4))) {
                        if (item_1(0, coordinates).tag === 3) {
                            if (item_1(1, coordinates).tag === 3) {
                                if (item_1(2, coordinates).tag === 3) {
                                    if (item_1(3, coordinates).tag === 3) {
                                        matchResult_4 = 0;
                                        x1 = item_1(0, coordinates).fields[0];
                                        x2 = item_1(2, coordinates).fields[0];
                                        y1 = item_1(1, coordinates).fields[0];
                                        y2 = item_1(3, coordinates).fields[0];
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
                    case 0: {
                        const patternInput = MapEditorInterchange_splitGridSegment(w_2, h_2, pixels, 0, 0, (doorCode > 0) ? MapEdgeKind.Door : MapEdgeKind.Wall, (doorCode > 0) && (stateCode === 1), x1, y1, x2, y2);
                        const geometryLoss = patternInput[1];
                        addRangeInPlace(patternInput[0], edges);
                        const lockedLoss = (doorCode > 0) && (stateCode > 1);
                        void (reports.push(new InterchangeFieldReport(path, ((geometryLoss != null) ? true : lockedLoss) ? InterchangeDisposition.Lossy : InterchangeDisposition.Mapped, lockedLoss ? "Foundry locked-door state has no S.I.R. equivalent and was mapped to closed." : defaultArg(geometryLoss, "Wall coordinates mapped to semantic wall/door edge(s)."))));
                        break;
                    }
                    case 1: {
                        void (reports.push(new InterchangeFieldReport(path, InterchangeDisposition.Lossy, "Wall without four numeric coordinates was ignored.")));
                        break;
                    }
                }
            }
            break;
        }
    }
    return MapEditorInterchange_finish(InterchangeFormat.FoundryScene, sourceName, root, dimensions, edges, ofSeq_1(reports), consumed, errors);
}

export function MapEditorInterchange_evaluate(format, sourceName, text) {
    if (get_UTF8().getBytes(text).length > 2000000) {
        return new InterchangeReview(format, sourceName, undefined, [], [("Interchange input is missing or exceeds the " + int32ToString(2000000)) + "-byte qualification limit."]);
    }
    else {
        switch (format.tag) {
            case 0:
            case 1: {
                const matchValue = MapEditorInterchange_parseJson(text);
                if (matchValue.tag === 0) {
                    const root = matchValue.fields[0];
                    switch (format.tag) {
                        case 1:
                            return MapEditorInterchange_evaluateFoundry(sourceName, root);
                        case 2:
                            throw new Exception("unreachable");
                        default:
                            return MapEditorInterchange_evaluateUniversal(sourceName, root);
                    }
                }
                else {
                    return new InterchangeReview(format, sourceName, undefined, [], [matchValue.fields[0]]);
                }
            }
            default:
                return new InterchangeReview(format, sourceName, undefined, [new InterchangeFieldReport("image/grid/occluder XML", InterchangeDisposition.RejectedField, "Fantasy Grounds image exports vary by campaign/database schema and encode paint, occluders, assets, and extensions without a stable portable semantic contract.")], ["Fantasy Grounds XML import is evaluation-only: no deterministic, reviewable mapping is accepted. Export through Universal VTT or author semantic edges in S.I.R."]);
        }
    }
}

export function MapEditorInterchange_canAccept(review) {
    if (review.Candidate != null) {
        return review.Errors.length === 0;
    }
    else {
        return false;
    }
}

export function MapEditorInterchange_accept(review) {
    if (MapEditorInterchange_canAccept(review)) {
        const matchValue = review.Candidate;
        if (matchValue == null) {
            return new FSharpResult$2(/* Error */ 1, ["Interchange review has no candidate map."]);
        }
        else {
            return new FSharpResult$2(/* Ok */ 0, [matchValue]);
        }
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [join(" ", review.Errors)]);
    }
}

