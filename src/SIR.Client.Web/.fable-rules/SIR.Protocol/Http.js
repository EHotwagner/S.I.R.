
import { Record } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { list_type, record_type, string_type, int32_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { list as list_1, object, toString } from "../fable_modules/Thoth.Json.10.5.1/Encode.fs.js";
import { list as list_2, fromString, string, int, object as object_1 } from "../fable_modules/Thoth.Json.10.5.1/Decode.fs.js";
import { uncurry2 } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { map } from "../fable_modules/fable-library-js.5.13.0/List.js";

export class Request extends Record {
    constructor(Version, ActorName) {
        super();
        this.Version = (Version | 0);
        this.ActorName = ActorName;
    }
}

export function Request_$reflection() {
    return record_type("SIR.Protocol.Http.BootstrapV1.Request", [], Request, () => [["Version", int32_type], ["ActorName", string_type]]);
}

export class VisibleUnit extends Record {
    constructor(UnitId, Column, Row, Health) {
        super();
        this.UnitId = (UnitId | 0);
        this.Column = (Column | 0);
        this.Row = (Row | 0);
        this.Health = (Health | 0);
    }
}

export function VisibleUnit_$reflection() {
    return record_type("SIR.Protocol.Http.BootstrapV1.VisibleUnit", [], VisibleUnit, () => [["UnitId", int32_type], ["Column", int32_type], ["Row", int32_type], ["Health", int32_type]]);
}

export class Snapshot extends Record {
    constructor(Version, Tick, ServerSequence, ProjectionRevision, VisibleUnits, StateIdentity) {
        super();
        this.Version = (Version | 0);
        this.Tick = (Tick | 0);
        this.ServerSequence = (ServerSequence | 0);
        this.ProjectionRevision = (ProjectionRevision | 0);
        this.VisibleUnits = VisibleUnits;
        this.StateIdentity = StateIdentity;
    }
}

export function Snapshot_$reflection() {
    return record_type("SIR.Protocol.Http.BootstrapV1.Snapshot", [], Snapshot, () => [["Version", int32_type], ["Tick", int32_type], ["ServerSequence", int32_type], ["ProjectionRevision", int32_type], ["VisibleUnits", list_type(VisibleUnit_$reflection())], ["StateIdentity", string_type]]);
}

export class Response extends Record {
    constructor(Version, SessionId, ActorId, AccessToken, MatchLock, Snapshot) {
        super();
        this.Version = (Version | 0);
        this.SessionId = SessionId;
        this.ActorId = ActorId;
        this.AccessToken = AccessToken;
        this.MatchLock = MatchLock;
        this.Snapshot = Snapshot;
    }
}

export function Response_$reflection() {
    return record_type("SIR.Protocol.Http.BootstrapV1.Response", [], Response, () => [["Version", int32_type], ["SessionId", string_type], ["ActorId", string_type], ["AccessToken", string_type], ["MatchLock", string_type], ["Snapshot", Snapshot_$reflection()]]);
}

export function encodeRequest(value) {
    return toString(0, object([["version", value.Version], ["actorName", value.ActorName]]));
}

export const decodeRequest = (path_1) => ((v) => object_1((get$) => {
    let objectArg, objectArg_1;
    return new Request((objectArg = get$.Required, objectArg.Field("version", uncurry2(int))), (objectArg_1 = get$.Required, objectArg_1.Field("actorName", string)));
}, path_1, v));

export function requestFromJson(json) {
    return fromString(uncurry2(decodeRequest), json);
}

export function encodeVisibleUnit(value) {
    return object([["unitId", value.UnitId], ["column", value.Column], ["row", value.Row], ["health", value.Health]]);
}

export const decodeVisibleUnit = (path) => ((v) => object_1((get$) => {
    let objectArg, objectArg_1, objectArg_2, objectArg_3;
    return new VisibleUnit((objectArg = get$.Required, objectArg.Field("unitId", uncurry2(int))), (objectArg_1 = get$.Required, objectArg_1.Field("column", uncurry2(int))), (objectArg_2 = get$.Required, objectArg_2.Field("row", uncurry2(int))), (objectArg_3 = get$.Required, objectArg_3.Field("health", uncurry2(int))));
}, path, v));

export function encodeSnapshot(value) {
    return object([["version", value.Version], ["tick", value.Tick], ["serverSequence", value.ServerSequence], ["projectionRevision", value.ProjectionRevision], ["visibleUnits", list_1(map(encodeVisibleUnit, value.VisibleUnits))], ["stateIdentity", value.StateIdentity]]);
}

export const decodeSnapshot = (path_2) => ((v) => object_1((get$) => {
    let objectArg, objectArg_1, objectArg_2, objectArg_3, objectArg_4, objectArg_5;
    return new Snapshot((objectArg = get$.Required, objectArg.Field("version", uncurry2(int))), (objectArg_1 = get$.Required, objectArg_1.Field("tick", uncurry2(int))), (objectArg_2 = get$.Required, objectArg_2.Field("serverSequence", uncurry2(int))), (objectArg_3 = get$.Required, objectArg_3.Field("projectionRevision", uncurry2(int))), (objectArg_4 = get$.Required, objectArg_4.Field("visibleUnits", (path, value) => list_2(uncurry2(decodeVisibleUnit), path, value))), (objectArg_5 = get$.Required, objectArg_5.Field("stateIdentity", string)));
}, path_2, v));

export function encodeResponse(value) {
    return toString(0, object([["version", value.Version], ["sessionId", value.SessionId], ["actorId", value.ActorId], ["accessToken", value.AccessToken], ["matchLock", value.MatchLock], ["snapshot", encodeSnapshot(value.Snapshot)]]));
}

export const decodeResponse = (path_4) => ((v) => object_1((get$) => {
    let objectArg, objectArg_1, objectArg_2, objectArg_3, objectArg_4, objectArg_5;
    return new Response((objectArg = get$.Required, objectArg.Field("version", uncurry2(int))), (objectArg_1 = get$.Required, objectArg_1.Field("sessionId", string)), (objectArg_2 = get$.Required, objectArg_2.Field("actorId", string)), (objectArg_3 = get$.Required, objectArg_3.Field("accessToken", string)), (objectArg_4 = get$.Required, objectArg_4.Field("matchLock", string)), (objectArg_5 = get$.Required, objectArg_5.Field("snapshot", uncurry2(decodeSnapshot))));
}, path_4, v));

export function responseFromJson(json) {
    return fromString(uncurry2(decodeResponse), json);
}

