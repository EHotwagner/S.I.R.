
import { toString, Union, Record } from "./fable_modules/fable-library-js.5.13.0/Types.js";
import { union_type, record_type, string_type, int32_type, option_type, class_type } from "./fable_modules/fable-library-js.5.13.0/Reflection.js";
import { Request, Snapshot_$reflection, Response_$reflection } from "./SIR.Protocol/Http.js";
import { AdvanceInput, Message, encodeMessage, ResyncRequest, messageFromJson, Message_$reflection } from "./SIR.Protocol/Realtime.js";
import { startImmediate } from "./fable_modules/fable-library-js.5.13.0/Async.js";
import { singleton } from "./fable_modules/fable-library-js.5.13.0/AsyncBuilder.js";
import { bootstrap } from "./LiveApi.js";
import { build } from "./SignalR.js";
import { defaultArg } from "./fable_modules/fable-library-js.5.13.0/Option.js";

export class State extends Record {
    constructor(Connection, Bootstrap, Snapshot, NextSequence, ResyncCount, Status) {
        super();
        this.Connection = Connection;
        this.Bootstrap = Bootstrap;
        this.Snapshot = Snapshot;
        this.NextSequence = (NextSequence | 0);
        this.ResyncCount = (ResyncCount | 0);
        this.Status = Status;
    }
}

export function State_$reflection() {
    return record_type("SIR.Client.Web.LiveSession.State", [], State, () => [["Connection", option_type(class_type("SIR.Client.Web.SignalR.HubConnection"))], ["Bootstrap", option_type(Response_$reflection())], ["Snapshot", option_type(Snapshot_$reflection())], ["NextSequence", int32_type], ["ResyncCount", int32_type], ["Status", string_type]]);
}

export class Action extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Bootstrapped", "BootstrapFailed", "Connected", "ConnectionOpened", "ConnectionClosed", "ConnectionFailed", "Received", "DecodeFailed"];
    }
    static ConnectionOpened = new Action(3, []);
    static ConnectionClosed = new Action(4, []);
}

export function Action_$reflection() {
    return union_type("SIR.Client.Web.LiveSession.Action", [], Action, () => [[["Item", Response_$reflection()]], [["Item", string_type]], [["Item", class_type("SIR.Client.Web.SignalR.HubConnection")]], [], [], [["Item", string_type]], [["Item", Message_$reflection()]], [["Item", string_type]]]);
}

export const initial = new State(undefined, undefined, undefined, 1, 0, "bootstrapping");

export function start(dispatch) {
    const request = new Request(1, "browser-commander");
    startImmediate(singleton.Delay(() => singleton.TryWith(singleton.Delay(() => singleton.Bind(bootstrap(request), (_arg) => {
        dispatch(new Action(/* Bootstrapped */ 0, [_arg]));
        return singleton.Zero();
    })), (_arg_1) => {
        dispatch(new Action(/* BootstrapFailed */ 1, [_arg_1.message]));
        return singleton.Zero();
    })));
}

export function connect(dispatch, response) {
    const active = build("/hub/game", response.AccessToken);
    active.on("Message", (json) => {
        const matchValue = messageFromJson(json);
        if (matchValue.tag === 1) {
            dispatch(new Action(/* DecodeFailed */ 7, [matchValue.fields[0]]));
        }
        else {
            dispatch(new Action(/* Received */ 6, [matchValue.fields[0]]));
        }
    });
    active.onreconnected((_arg) => {
        dispatch(Action.ConnectionOpened);
    });
    active.onclose((_arg_1) => {
        dispatch(Action.ConnectionClosed);
    });
    active.start().then((() => {
        dispatch(Action.ConnectionOpened);
    }), ((error_1) => {
        dispatch(new Action(/* ConnectionFailed */ 5, [toString(error_1)]));
    }));
    return active;
}

export function requestResync(dispatch, state) {
    let option_1, option_4;
    const matchValue = state.Connection;
    if (matchValue != null) {
        const active = matchValue;
        const snapshot = state.Snapshot;
        const request = new ResyncRequest(1, defaultArg((option_1 = snapshot, (option_1 != null) ? option_1.ServerSequence : undefined), 0), defaultArg((option_4 = snapshot, (option_4 != null) ? option_4.ProjectionRevision : undefined), 0));
        active.invoke("SendMessage", encodeMessage(new Message(/* ResyncRequestMessage */ 2, [request]))).then(((value_2) => {
        }), ((error) => {
            dispatch(new Action(/* ConnectionFailed */ 5, [toString(error)]));
        }));
    }
}

export function advance(dispatch, state) {
    const matchValue = state.Connection;
    if (matchValue != null) {
        const active = matchValue;
        const input = new AdvanceInput(1, state.NextSequence);
        active.invoke("SendMessage", encodeMessage(new Message(/* AdvanceInputMessage */ 0, [input]))).then(((value) => {
        }), ((error) => {
            dispatch(new Action(/* ConnectionFailed */ 5, [toString(error)]));
        }));
    }
}

export function disconnect(dispatch, state) {
    const matchValue = state.Connection;
    if (matchValue != null) {
        const active = matchValue;
        active.stop().then((() => {
            dispatch(Action.ConnectionClosed);
        }), ((error) => {
            dispatch(new Action(/* ConnectionFailed */ 5, [toString(error)]));
        }));
    }
}

export function reconnect(dispatch, state) {
    const matchValue = state.Connection;
    if (matchValue != null) {
        const active = matchValue;
        active.stop().then((() => {
            active.start().then((() => {
                requestResync(dispatch, state);
            }), ((error) => {
                dispatch(new Action(/* ConnectionFailed */ 5, [toString(error)]));
            }));
        }), ((error_1) => {
            dispatch(new Action(/* ConnectionFailed */ 5, [toString(error_1)]));
        }));
    }
}

