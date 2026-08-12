
import { Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { enum_type, list_type, uint32_type, record_type, array_type, uint8_type, int32_type, bool_type, uint16_type, union_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { equalsWith, copyTo, item, setItem } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { Result_Map, FSharpResult$2 } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { tryFind, empty, cons, reverse, fold, tail as tail_1, head, isEmpty, sortBy, length as length_1 } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { equals, disposeSafe, getEnumerator, comparePrimitives } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { toUInt32_unchecked, toInt32_unchecked, compare, fromInt32, fromInt64, op_Addition, toInt64_unchecked } from "../fable_modules/fable-library-js.5.13.0/BigInt.js";
import { exists } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { toArray } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { get_UTF8 } from "../fable_modules/fable-library-js.5.13.0/Encoding.js";

/**
 * Whether a canonical envelope is host input or module output.
 */
export class MessageKind extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Input", "Output"];
    }
    static Input = new MessageKind(0, []);
    static Output = new MessageKind(1, []);
}

export function MessageKind_$reflection() {
    return union_type("SIR.ControlAbi.MessageKind", [], MessageKind, () => [[], []]);
}

/**
 * One independently bounded ABI section.
 */
export class Section extends Record {
    constructor(Tag, Required, ElementCount, Payload) {
        super();
        this.Tag = Tag;
        this.Required = Required;
        this.ElementCount = (ElementCount | 0);
        this.Payload = Payload;
    }
}

export function Section_$reflection() {
    return record_type("SIR.ControlAbi.Section", [], Section, () => [["Tag", uint16_type], ["Required", bool_type], ["ElementCount", int32_type], ["Payload", array_type(uint8_type)]]);
}

/**
 * Runtime-neutral representation of a Control ABI v1 invocation envelope.
 */
export class Envelope extends Record {
    constructor(Kind, MinorVersion, Tick, UnitId, Flags, Budget, Sections) {
        super();
        this.Kind = Kind;
        this.MinorVersion = MinorVersion;
        this.Tick = (Tick | 0);
        this.UnitId = (UnitId | 0);
        this.Flags = Flags;
        this.Budget = Budget;
        this.Sections = Sections;
    }
}

export function Envelope_$reflection() {
    return record_type("SIR.ControlAbi.Envelope", [], Envelope, () => [["Kind", MessageKind_$reflection()], ["MinorVersion", uint8_type], ["Tick", int32_type], ["UnitId", int32_type], ["Flags", uint32_type], ["Budget", uint32_type], ["Sections", list_type(Section_$reflection())]]);
}

/**
 * One output request. The request-specific payload is interpreted by the host.
 */
export class Request extends Record {
    constructor(Kind, ModuleRequestId, Payload) {
        super();
        this.Kind = Kind;
        this.ModuleRequestId = ModuleRequestId;
        this.Payload = Payload;
    }
}

export function Request_$reflection() {
    return record_type("SIR.ControlAbi.Request", [], Request, () => [["Kind", enum_type("SIR.ControlAbi.RequestKind", int32_type, [["SetMovementIntent", 1], ["SetFacing", 2], ["SetAttention", 3], ["SetStance", 4], ["SetEngagement", 5], ["StartCapability", 6], ["CancelAction", 7], ["SendMessage", 8], ["RequestService", 9], ["SetEmissionPolicy", 10], ["SetFormationIntent", 11], ["Sleep", 12]])], ["ModuleRequestId", uint32_type], ["Payload", array_type(uint8_type)]]);
}

/**
 * A fully decoded module output and its atomically validated requests.
 */
export class OutputMessage extends Record {
    constructor(Envelope, Requests) {
        super();
        this.Envelope = Envelope;
        this.Requests = Requests;
    }
}

export function OutputMessage_$reflection() {
    return record_type("SIR.ControlAbi.OutputMessage", [], OutputMessage, () => [["Envelope", Envelope_$reflection()], ["Requests", list_type(Request_$reflection())]]);
}

/**
 * Stable decoder failures; diagnostics may add text without changing these cases.
 */
export class DecodeError extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["TooShort", "BadMagic", "WrongMessageKind", "UnsupportedVersion", "InvalidTotalLength", "LimitExceeded", "ReservedBitsSet", "InvalidSectionFlags", "InvalidSectionLength", "InvalidElementCount", "NonCanonicalSectionOrder", "DuplicateSectionTag", "UnknownRequiredSection", "MissingRequiredSection", "InvalidUtf8", "UnknownRequestKind", "NonCanonicalRequestOrder", "DuplicateRequestId", "TrailingRequestBytes"];
    }
    static TooShort = new DecodeError(0, []);
    static BadMagic = new DecodeError(1, []);
    static WrongMessageKind = new DecodeError(2, []);
    static UnsupportedVersion = new DecodeError(3, []);
    static InvalidTotalLength = new DecodeError(4, []);
    static LimitExceeded = new DecodeError(5, []);
    static ReservedBitsSet = new DecodeError(6, []);
    static InvalidSectionFlags = new DecodeError(7, []);
    static InvalidSectionLength = new DecodeError(8, []);
    static InvalidElementCount = new DecodeError(9, []);
    static NonCanonicalSectionOrder = new DecodeError(10, []);
    static DuplicateSectionTag = new DecodeError(11, []);
    static UnknownRequiredSection = new DecodeError(12, []);
    static MissingRequiredSection = new DecodeError(13, []);
    static InvalidUtf8 = new DecodeError(14, []);
    static UnknownRequestKind = new DecodeError(15, []);
    static NonCanonicalRequestOrder = new DecodeError(16, []);
    static DuplicateRequestId = new DecodeError(17, []);
    static TrailingRequestBytes = new DecodeError(18, []);
}

export function DecodeError_$reflection() {
    return union_type("SIR.ControlAbi.DecodeError", [], DecodeError, () => [[], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], [], []]);
}

const V1Codec_inputMagic = new Uint8Array([83, 73, 82, 73]);

const V1Codec_outputMagic = new Uint8Array([83, 73, 82, 79]);

function V1Codec_maximumBytes(kind) {
    if (kind.tag === 1) {
        return 16384;
    }
    else {
        return 65536;
    }
}

function V1Codec_magic(kind) {
    if (kind.tag === 1) {
        return V1Codec_outputMagic;
    }
    else {
        return V1Codec_inputMagic;
    }
}

function V1Codec_knownTag(kind, tag) {
    if (kind.tag === 1) {
        return tag === 4097;
    }
    else if (tag >= 1) {
        return tag <= 9;
    }
    else {
        return false;
    }
}

function V1Codec_writeU16(bytes, offset, value) {
    setItem(bytes, offset, value & 0xFF);
    setItem(bytes, offset + 1, (value >> 8) & 0xFF);
}

function V1Codec_writeU32(bytes, offset, value) {
    setItem(bytes, offset, value & 0xFF);
    setItem(bytes, offset + 1, (value >>> 8) & 0xFF);
    setItem(bytes, offset + 2, (value >>> 16) & 0xFF);
    setItem(bytes, offset + 3, (value >>> 24) & 0xFF);
}

function V1Codec_readU16(bytes, offset) {
    return item(offset, bytes) | (item(offset + 1, bytes) << 8);
}

function V1Codec_readU32(bytes, offset) {
    return (((((item(offset, bytes) | ((item(offset + 1, bytes) << 8) >>> 0)) >>> 0) | ((item(offset + 2, bytes) << 16) >>> 0)) >>> 0) | ((item(offset + 3, bytes) << 24) >>> 0)) >>> 0;
}

function V1Codec_checkedLength(value) {
    if (value > (2147483647 >>> 0)) {
        return undefined;
    }
    else {
        return ~~value;
    }
}

function V1Codec_validateSection(section) {
    if ((section.ElementCount < 0) ? true : (section.ElementCount > 256)) {
        return new FSharpResult$2(/* Error */ 1, [DecodeError.InvalidElementCount]);
    }
    else if (section.Payload.length > 65536) {
        return new FSharpResult$2(/* Error */ 1, [DecodeError.InvalidSectionLength]);
    }
    else {
        return new FSharpResult$2(/* Ok */ 0, [undefined]);
    }
}

/**
 * Encodes an envelope, sorting sections by ascending tag.
 */
export function V1Codec_encode(envelope) {
    if ((((envelope.MinorVersion > 0) ? true : (envelope.Tick < 0)) ? true : (envelope.UnitId < 0)) ? true : (length_1(envelope.Sections) > 32)) {
        return new FSharpResult$2(/* Error */ 1, [DecodeError.LimitExceeded]);
    }
    else {
        const sections = sortBy((_arg) => _arg.Tag, envelope.Sections, {
            Compare: (x, y) => (comparePrimitives(x, y) | 0),
        });
        const validate = (previous_mut, remaining_mut) => {
            validate:
            while (true) {
                const previous = previous_mut, remaining = remaining_mut;
                if (!isEmpty(remaining)) {
                    const section = head(remaining);
                    const matchValue = V1Codec_validateSection(section);
                    let matchResult, tag_1, error;
                    if (previous != null) {
                        if (previous === section.Tag) {
                            matchResult = 0;
                            tag_1 = previous;
                        }
                        else {
                            const copyOfStruct = matchValue;
                            if (copyOfStruct.tag === 0) {
                                if (section.Required && !V1Codec_knownTag(envelope.Kind, section.Tag)) {
                                    matchResult = 2;
                                }
                                else {
                                    matchResult = 3;
                                }
                            }
                            else {
                                matchResult = 1;
                                error = copyOfStruct.fields[0];
                            }
                        }
                    }
                    else {
                        const copyOfStruct_1 = matchValue;
                        if (copyOfStruct_1.tag === 0) {
                            if (section.Required && !V1Codec_knownTag(envelope.Kind, section.Tag)) {
                                matchResult = 2;
                            }
                            else {
                                matchResult = 3;
                            }
                        }
                        else {
                            matchResult = 1;
                            error = copyOfStruct_1.fields[0];
                        }
                    }
                    switch (matchResult) {
                        case 0:
                            return new FSharpResult$2(/* Error */ 1, [DecodeError.DuplicateSectionTag]);
                        case 1:
                            return new FSharpResult$2(/* Error */ 1, [error]);
                        case 2:
                            return new FSharpResult$2(/* Error */ 1, [DecodeError.UnknownRequiredSection]);
                        default: {
                            previous_mut = section.Tag;
                            remaining_mut = tail_1(remaining);
                            continue validate;
                        }
                    }
                }
                else {
                    return new FSharpResult$2(/* Ok */ 0, [undefined]);
                }
                break;
            }
        };
        const matchValue_2 = validate(undefined, sections);
        if (matchValue_2.tag === 0) {
            const total = fold((size, section_1) => toInt64_unchecked(op_Addition(toInt64_unchecked(op_Addition(toInt64_unchecked(fromInt64(size)), toInt64_unchecked(fromInt32(12)))), toInt64_unchecked(fromInt32(section_1.Payload.length)))), toInt64_unchecked(fromInt32(32)), sections);
            if (compare(total, toInt64_unchecked(fromInt32(V1Codec_maximumBytes(envelope.Kind)))) > 0) {
                return new FSharpResult$2(/* Error */ 1, [DecodeError.LimitExceeded]);
            }
            else {
                const bytes = new Uint8Array(~~toInt32_unchecked(total));
                copyTo(V1Codec_magic(envelope.Kind), 0, bytes, 0, 4);
                setItem(bytes, 4, 1);
                setItem(bytes, 5, envelope.MinorVersion);
                V1Codec_writeU16(bytes, 6, 32 & 0xFFFF);
                V1Codec_writeU32(bytes, 8, toUInt32_unchecked(total) >>> 0);
                V1Codec_writeU32(bytes, 12, envelope.Tick >>> 0);
                V1Codec_writeU32(bytes, 16, envelope.UnitId >>> 0);
                V1Codec_writeU32(bytes, 20, envelope.Flags);
                V1Codec_writeU32(bytes, 24, envelope.Budget);
                V1Codec_writeU16(bytes, 28, length_1(envelope.Sections) & 0xFFFF);
                V1Codec_writeU16(bytes, 30, 0);
                let offset = 32;
                const enumerator = getEnumerator(sections);
                try {
                    while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                        const section_2 = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
                        V1Codec_writeU16(bytes, offset, section_2.Tag);
                        V1Codec_writeU16(bytes, offset + 2, section_2.Required ? 1 : 0);
                        V1Codec_writeU32(bytes, offset + 4, section_2.Payload.length >>> 0);
                        V1Codec_writeU16(bytes, offset + 8, section_2.ElementCount & 0xFFFF);
                        V1Codec_writeU16(bytes, offset + 10, 0);
                        copyTo(section_2.Payload, 0, bytes, offset + 12, section_2.Payload.length);
                        offset = (((offset + 12) + section_2.Payload.length) | 0);
                    }
                }
                finally {
                    disposeSafe(enumerator);
                }
                return new FSharpResult$2(/* Ok */ 0, [bytes]);
            }
        }
        else {
            return new FSharpResult$2(/* Error */ 1, [matchValue_2.fields[0]]);
        }
    }
}

/**
 * Decodes and fully bounds a canonical input or output envelope.
 */
export function V1Codec_decode(kind, bytes) {
    if (bytes.length < 32) {
        return new FSharpResult$2(/* Error */ 1, [DecodeError.TooShort]);
    }
    else if (bytes.length > V1Codec_maximumBytes(kind)) {
        return new FSharpResult$2(/* Error */ 1, [DecodeError.LimitExceeded]);
    }
    else if (!equalsWith((x, y) => (x === y), bytes.slice(0, 3 + 1), V1Codec_magic(kind))) {
        return new FSharpResult$2(/* Error */ 1, [DecodeError.BadMagic]);
    }
    else if ((item(4, bytes) !== 1) ? true : (item(5, bytes) > 0)) {
        return new FSharpResult$2(/* Error */ 1, [DecodeError.UnsupportedVersion]);
    }
    else if (V1Codec_readU16(bytes, 6) !== (32 & 0xFFFF)) {
        return new FSharpResult$2(/* Error */ 1, [DecodeError.InvalidTotalLength]);
    }
    else if (V1Codec_readU32(bytes, 8) !== (bytes.length >>> 0)) {
        return new FSharpResult$2(/* Error */ 1, [DecodeError.InvalidTotalLength]);
    }
    else if ((V1Codec_readU32(bytes, 12) > (2147483647 >>> 0)) ? true : (V1Codec_readU32(bytes, 16) > (2147483647 >>> 0))) {
        return new FSharpResult$2(/* Error */ 1, [DecodeError.LimitExceeded]);
    }
    else if (V1Codec_readU16(bytes, 30) !== 0) {
        return new FSharpResult$2(/* Error */ 1, [DecodeError.ReservedBitsSet]);
    }
    else {
        const sectionCount = ~~V1Codec_readU16(bytes, 28) | 0;
        if (sectionCount > 32) {
            return new FSharpResult$2(/* Error */ 1, [DecodeError.LimitExceeded]);
        }
        else {
            const readSections = (index_mut, offset_mut, previousTag_mut, sections_mut) => {
                readSections:
                while (true) {
                    const index = index_mut, offset = offset_mut, previousTag = previousTag_mut, sections = sections_mut;
                    if (index === sectionCount) {
                        if (offset === bytes.length) {
                            return new FSharpResult$2(/* Ok */ 0, [reverse(sections)]);
                        }
                        else {
                            return new FSharpResult$2(/* Error */ 1, [DecodeError.InvalidTotalLength]);
                        }
                    }
                    else if (offset > (bytes.length - 12)) {
                        return new FSharpResult$2(/* Error */ 1, [DecodeError.InvalidSectionLength]);
                    }
                    else {
                        const tag = V1Codec_readU16(bytes, offset);
                        const flags = V1Codec_readU16(bytes, offset + 2);
                        const payloadLengthValue = V1Codec_readU32(bytes, offset + 4);
                        const elementCount = ~~V1Codec_readU16(bytes, offset + 8) | 0;
                        const reserved = V1Codec_readU16(bytes, offset + 10);
                        const matchValue = V1Codec_checkedLength(payloadLengthValue);
                        if (matchValue != null) {
                            const payloadLength = matchValue | 0;
                            const payloadOffset = (offset + 12) | 0;
                            if ((flags & ~1) !== 0) {
                                return new FSharpResult$2(/* Error */ 1, [DecodeError.InvalidSectionFlags]);
                            }
                            else if (reserved !== 0) {
                                return new FSharpResult$2(/* Error */ 1, [DecodeError.ReservedBitsSet]);
                            }
                            else if (elementCount > 256) {
                                return new FSharpResult$2(/* Error */ 1, [DecodeError.InvalidElementCount]);
                            }
                            else if (payloadLength > (bytes.length - payloadOffset)) {
                                return new FSharpResult$2(/* Error */ 1, [DecodeError.InvalidSectionLength]);
                            }
                            else if (equals(previousTag, tag)) {
                                return new FSharpResult$2(/* Error */ 1, [DecodeError.DuplicateSectionTag]);
                            }
                            else if (exists((previous) => (previous > tag), toArray(previousTag))) {
                                return new FSharpResult$2(/* Error */ 1, [DecodeError.NonCanonicalSectionOrder]);
                            }
                            else if ((flags === 1) && !V1Codec_knownTag(kind, tag)) {
                                return new FSharpResult$2(/* Error */ 1, [DecodeError.UnknownRequiredSection]);
                            }
                            else {
                                index_mut = (index + 1);
                                offset_mut = (payloadOffset + payloadLength);
                                previousTag_mut = tag;
                                sections_mut = cons(new Section(tag, flags === 1, elementCount, bytes.slice(payloadOffset, ((payloadOffset + payloadLength) - 1) + 1)), sections);
                                continue readSections;
                            }
                        }
                        else {
                            return new FSharpResult$2(/* Error */ 1, [DecodeError.InvalidSectionLength]);
                        }
                    }
                    break;
                }
            };
            return Result_Map((sections_1) => (new Envelope(kind, item(5, bytes), ~~V1Codec_readU32(bytes, 12), ~~V1Codec_readU32(bytes, 16), V1Codec_readU32(bytes, 20), V1Codec_readU32(bytes, 24), sections_1)), readSections(0, 32, undefined, empty()));
        }
    }
}

/**
 * Encodes a length-prefixed bounded UTF-8 field.
 */
export function V1Codec_encodeString(value) {
    const bytes = get_UTF8().getBytes(value);
    if (bytes.length > 255) {
        return new FSharpResult$2(/* Error */ 1, [DecodeError.LimitExceeded]);
    }
    else {
        const result = new Uint8Array(2 + bytes.length);
        V1Codec_writeU16(result, 0, bytes.length & 0xFFFF);
        copyTo(bytes, 0, result, 2, bytes.length);
        return new FSharpResult$2(/* Ok */ 0, [result]);
    }
}

/**
 * Decodes a complete length-prefixed bounded UTF-8 field.
 */
export function V1Codec_decodeString(bytes) {
    if (bytes.length < 2) {
        return new FSharpResult$2(/* Error */ 1, [DecodeError.InvalidUtf8]);
    }
    else {
        const length = ~~V1Codec_readU16(bytes, 0) | 0;
        if ((length > 255) ? true : (length !== (bytes.length - 2))) {
            return new FSharpResult$2(/* Error */ 1, [DecodeError.LimitExceeded]);
        }
        else {
            const payload = bytes.slice(2, bytes.length);
            const decoded = get_UTF8().getString(payload);
            if (!equalsWith((x, y) => (x === y), get_UTF8().getBytes(decoded), payload)) {
                return new FSharpResult$2(/* Error */ 1, [DecodeError.InvalidUtf8]);
            }
            else {
                return new FSharpResult$2(/* Ok */ 0, [decoded]);
            }
        }
    }
}

/**
 * Encodes output request records in ascending module-request-ID order.
 */
export function V1Codec_encodeRequests(requests) {
    const ordered = sortBy((_arg) => _arg.ModuleRequestId, requests, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    if (length_1(ordered) > 256) {
        return new FSharpResult$2(/* Error */ 1, [DecodeError.LimitExceeded]);
    }
    else {
        let previous = undefined;
        let error = undefined;
        let total = 0;
        const enumerator = getEnumerator(ordered);
        try {
            while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
                const request = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]();
                if (equals(previous, request.ModuleRequestId)) {
                    error = DecodeError.DuplicateRequestId;
                }
                else if (request.Payload.length > 4096) {
                    error = DecodeError.LimitExceeded;
                }
                else {
                    previous = request.ModuleRequestId;
                    total = (((total + 12) + request.Payload.length) | 0);
                }
            }
        }
        finally {
            disposeSafe(enumerator);
        }
        if (error == null) {
            const bytes = new Uint8Array(total);
            let offset = 0;
            const enumerator_1 = getEnumerator(ordered);
            try {
                while (enumerator_1["System.Collections.IEnumerator.MoveNext"]()) {
                    const request_1 = enumerator_1["System.Collections.Generic.IEnumerator`1.get_Current"]();
                    V1Codec_writeU16(bytes, offset, request_1.Kind & 0xFFFF);
                    V1Codec_writeU16(bytes, offset + 2, 0);
                    V1Codec_writeU32(bytes, offset + 4, request_1.ModuleRequestId);
                    V1Codec_writeU32(bytes, offset + 8, request_1.Payload.length >>> 0);
                    copyTo(request_1.Payload, 0, bytes, offset + 12, request_1.Payload.length);
                    offset = (((offset + 12) + request_1.Payload.length) | 0);
                }
            }
            finally {
                disposeSafe(enumerator_1);
            }
            return new FSharpResult$2(/* Ok */ 0, [bytes]);
        }
        else {
            return new FSharpResult$2(/* Error */ 1, [error]);
        }
    }
}

/**
 * Decodes exactly elementCount output request records.
 */
export function V1Codec_decodeRequests(elementCount, bytes) {
    if ((elementCount < 0) ? true : (elementCount > 256)) {
        return new FSharpResult$2(/* Error */ 1, [DecodeError.InvalidElementCount]);
    }
    else {
        const loop = (index_mut, offset_mut, previous_mut, requests_mut) => {
            loop:
            while (true) {
                const index = index_mut, offset = offset_mut, previous = previous_mut, requests = requests_mut;
                if (index === elementCount) {
                    if (offset === bytes.length) {
                        return new FSharpResult$2(/* Ok */ 0, [reverse(requests)]);
                    }
                    else {
                        return new FSharpResult$2(/* Error */ 1, [DecodeError.TrailingRequestBytes]);
                    }
                }
                else if (offset > (bytes.length - 12)) {
                    return new FSharpResult$2(/* Error */ 1, [DecodeError.InvalidSectionLength]);
                }
                else if (V1Codec_readU16(bytes, offset + 2) !== 0) {
                    return new FSharpResult$2(/* Error */ 1, [DecodeError.ReservedBitsSet]);
                }
                else {
                    const requestId = V1Codec_readU32(bytes, offset + 4);
                    let matchValue;
                    const value = ~~V1Codec_readU16(bytes, offset) | 0;
                    matchValue = (((value >= 1) && (value <= 12)) ? value : undefined);
                    const matchValue_1 = V1Codec_checkedLength(V1Codec_readU32(bytes, offset + 8));
                    if (matchValue != null) {
                        if (matchValue_1 != null) {
                            const kind = matchValue;
                            const payloadLength = matchValue_1 | 0;
                            const payloadOffset = (offset + 12) | 0;
                            if (payloadLength > 4096) {
                                return new FSharpResult$2(/* Error */ 1, [DecodeError.LimitExceeded]);
                            }
                            else if (payloadLength > (bytes.length - payloadOffset)) {
                                return new FSharpResult$2(/* Error */ 1, [DecodeError.InvalidSectionLength]);
                            }
                            else if (equals(previous, requestId)) {
                                return new FSharpResult$2(/* Error */ 1, [DecodeError.DuplicateRequestId]);
                            }
                            else if (exists((previousId) => (previousId > requestId), toArray(previous))) {
                                return new FSharpResult$2(/* Error */ 1, [DecodeError.NonCanonicalRequestOrder]);
                            }
                            else {
                                index_mut = (index + 1);
                                offset_mut = (payloadOffset + payloadLength);
                                previous_mut = requestId;
                                requests_mut = cons(new Request(kind, requestId, bytes.slice(payloadOffset, ((payloadOffset + payloadLength) - 1) + 1)), requests);
                                continue loop;
                            }
                        }
                        else {
                            return new FSharpResult$2(/* Error */ 1, [DecodeError.InvalidSectionLength]);
                        }
                    }
                    else {
                        return new FSharpResult$2(/* Error */ 1, [DecodeError.UnknownRequestKind]);
                    }
                }
                break;
            }
        };
        return loop(0, 0, undefined, empty());
    }
}

/**
 * Encodes the one required output-request section and any future optional sections.
 */
export function V1Codec_encodeOutput(tick, unitId, flags, budget, requests, optionalSections) {
    const matchValue = V1Codec_encodeRequests(requests);
    if (matchValue.tag === 0) {
        return V1Codec_encode(new Envelope(MessageKind.Output, 0, tick, unitId, flags, budget, cons(new Section(4097, true, length_1(requests), matchValue.fields[0]), optionalSections)));
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [matchValue.fields[0]]);
    }
}

/**
 * Decodes a complete output atomically, including every request record.
 */
export function V1Codec_decodeOutput(bytes) {
    const matchValue = V1Codec_decode(MessageKind.Output, bytes);
    if (matchValue.tag === 0) {
        const envelope = matchValue.fields[0];
        const matchValue_1 = tryFind((section) => (section.Tag === 4097), envelope.Sections);
        if (matchValue_1 != null) {
            if (!matchValue_1.Required) {
                const section_2 = matchValue_1;
                return new FSharpResult$2(/* Error */ 1, [DecodeError.MissingRequiredSection]);
            }
            else {
                const section_3 = matchValue_1;
                return Result_Map((requests) => (new OutputMessage(envelope, requests)), V1Codec_decodeRequests(section_3.ElementCount, section_3.Payload));
            }
        }
        else {
            return new FSharpResult$2(/* Error */ 1, [DecodeError.MissingRequiredSection]);
        }
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [matchValue.fields[0]]);
    }
}

/**
 * Encodes a host input envelope and rejects a mismatched message kind.
 */
export function V1Codec_encodeInput(envelope) {
    if (!equals(envelope.Kind, MessageKind.Input)) {
        return new FSharpResult$2(/* Error */ 1, [DecodeError.WrongMessageKind]);
    }
    else {
        return V1Codec_encode(envelope);
    }
}

/**
 * Decodes a canonical host input envelope.
 */
export function V1Codec_decodeInput(bytes) {
    return V1Codec_decode(MessageKind.Input, bytes);
}

