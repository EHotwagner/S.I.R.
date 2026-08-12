
import { Union, Record } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { union_type, record_type, int32_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { decodeSnapshot, encodeSnapshot, Snapshot_$reflection } from "./Http.js";
import { toString, object } from "../fable_modules/Thoth.Json.10.5.1/Encode.fs.js";
import { fromString, string, fail, field, map, andThen, int, object as object_1 } from "../fable_modules/Thoth.Json.10.5.1/Decode.fs.js";
import { uncurry3, uncurry2 } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { printf, toText } from "../fable_modules/fable-library-js.5.13.0/String.js";

export class AdvanceInput extends Record {
    constructor(Version, Sequence) {
        super();
        this.Version = (Version | 0);
        this.Sequence = (Sequence | 0);
    }
}

export function AdvanceInput_$reflection() {
    return record_type("SIR.Protocol.Realtime.RealtimeV1.AdvanceInput", [], AdvanceInput, () => [["Version", int32_type], ["Sequence", int32_type]]);
}

export class ResyncRequest extends Record {
    constructor(Version, LastServerSequence, LastProjectionRevision) {
        super();
        this.Version = (Version | 0);
        this.LastServerSequence = (LastServerSequence | 0);
        this.LastProjectionRevision = (LastProjectionRevision | 0);
    }
}

export function ResyncRequest_$reflection() {
    return record_type("SIR.Protocol.Realtime.RealtimeV1.ResyncRequest", [], ResyncRequest, () => [["Version", int32_type], ["LastServerSequence", int32_type], ["LastProjectionRevision", int32_type]]);
}

export class Message extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["AdvanceInputMessage", "SnapshotMessage", "ResyncRequestMessage", "ResyncSnapshotMessage"];
    }
}

export function Message_$reflection() {
    return union_type("SIR.Protocol.Realtime.RealtimeV1.Message", [], Message, () => [[["Item", AdvanceInput_$reflection()]], [["Item", Snapshot_$reflection()]], [["Item", ResyncRequest_$reflection()]], [["Item", Snapshot_$reflection()]]]);
}

function encodeAdvance(value) {
    return object([["version", value.Version], ["sequence", value.Sequence]]);
}

const decodeAdvance = (path) => ((v) => object_1((get$) => {
    let objectArg, objectArg_1;
    return new AdvanceInput((objectArg = get$.Required, objectArg.Field("version", uncurry2(int))), (objectArg_1 = get$.Required, objectArg_1.Field("sequence", uncurry2(int))));
}, path, v));

function encodeResync(value) {
    return object([["version", value.Version], ["lastServerSequence", value.LastServerSequence], ["lastProjectionRevision", value.LastProjectionRevision]]);
}

const decodeResync = (path) => ((v) => object_1((get$) => {
    let objectArg, objectArg_1, objectArg_2;
    return new ResyncRequest((objectArg = get$.Required, objectArg.Field("version", uncurry2(int))), (objectArg_1 = get$.Required, objectArg_1.Field("lastServerSequence", uncurry2(int))), (objectArg_2 = get$.Required, objectArg_2.Field("lastProjectionRevision", uncurry2(int))));
}, path, v));

export function encodeMessage(value) {
    const patternInput = (value.tag === 1) ? ["snapshot", encodeSnapshot(value.fields[0])] : ((value.tag === 2) ? ["resyncRequest", encodeResync(value.fields[0])] : ((value.tag === 3) ? ["resyncSnapshot", encodeSnapshot(value.fields[0])] : ["advance", encodeAdvance(value.fields[0])]));
    return toString(0, object([["kind", patternInput[0]], ["payload", patternInput[1]]]));
}

export const decodeMessage = (path_11) => ((value_10) => andThen(uncurry3((kind) => {
    switch (kind) {
        case "advance":
            return (path_3) => ((value_3) => map((Item) => (new Message(/* AdvanceInputMessage */ 0, [Item])), (path_2, value_2) => field("payload", uncurry2(decodeAdvance), path_2, value_2), path_3, value_3));
        case "snapshot":
            return (path_5) => ((value_5) => map((Item_1) => (new Message(/* SnapshotMessage */ 1, [Item_1])), (path_4, value_4) => field("payload", uncurry2(decodeSnapshot), path_4, value_4), path_5, value_5));
        case "resyncRequest":
            return (path_7) => ((value_7) => map((Item_2) => (new Message(/* ResyncRequestMessage */ 2, [Item_2])), (path_6, value_6) => field("payload", uncurry2(decodeResync), path_6, value_6), path_7, value_7));
        case "resyncSnapshot":
            return (path_9) => ((value_9) => map((Item_3) => (new Message(/* ResyncSnapshotMessage */ 3, [Item_3])), (path_8, value_8) => field("payload", uncurry2(decodeSnapshot), path_8, value_8), path_9, value_9));
        default: {
            const msg = toText(printf("unknown realtime message kind \'%s\'"))(kind);
            return (path_10) => ((arg20$0040) => fail(msg, path_10, arg20$0040));
        }
    }
}), (path_1, value_1) => field("kind", string, path_1, value_1), path_11, value_10));

export function messageFromJson(json) {
    return fromString(uncurry2(decodeMessage), json);
}

