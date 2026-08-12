
import { empty } from "./fable_modules/fable-library-js.5.13.0/Set.js";
import { int32ToString, comparePrimitives } from "./fable_modules/fable-library-js.5.13.0/Util.js";
import { SimulatorProtocol_beginOperation, SimulatorProtocol_activate, SimulatorRequestEnvelope, SimulatorProtocol_completeOperation, SimulatorProtocol_accepts, SimulatorWorkspaceGuard } from "./SIR.Client/SimulatorWorkerProtocol.js";
import { RunnerResponse, WorkerRequestEnvelope, OperationIdModule_value, Msg, OperationIdModule_create } from "./SIR.Client/Shell.js";
import { format, join } from "./fable_modules/fable-library-js.5.13.0/String.js";
import { map } from "./fable_modules/fable-library-js.5.13.0/Array.js";
import { Replay_defaultLimits, Replay_decode } from "./SIR.Simulation/Replay.js";
import { EngineCatalog_tryFind, EngineCatalog_Current } from "./SIR.Client/EngineCatalog.js";

let worker = undefined;

let subscriber = undefined;

let simulatorSubscriber = undefined;

let simulatorGuard = new SimulatorWorkspaceGuard(undefined, empty({
    Compare: (x, y) => (comparePrimitives(x, y) | 0),
}));

function dispatch(message) {
    const option_1 = subscriber;
    if (option_1 != null) {
        option_1(message);
    }
}

function bind(active) {
    active.onmessage = ((event) => {
        const message = event;
        if (message.data.Kind === "sir-simulator-session") {
            const envelope = message.data;
            if (SimulatorProtocol_accepts(envelope, simulatorGuard)) {
                const option_1 = simulatorSubscriber;
                if (option_1 != null) {
                    option_1(envelope);
                }
                if (envelope.Response.tag === 5) {
                }
                else {
                    simulatorGuard = SimulatorProtocol_completeOperation(envelope.Correlation.Operation, simulatorGuard);
                }
            }
        }
        else {
            const envelope_1 = message.data;
            if (envelope_1.ProtocolVersion === 3) {
                dispatch(new Msg(/* RunnerResponded */ 1, [OperationIdModule_create(envelope_1.Operation), envelope_1.Response]));
            }
            else {
                dispatch(new Msg(/* WorkerTerminated */ 17, [(("protocol " + int32ToString(envelope_1.ProtocolVersion)) + " is incompatible with protocol ") + int32ToString(3)]));
            }
        }
    });
    active.onerror = ((event_1) => {
        dispatch(new Msg(/* WorkerTerminated */ 17, [event_1.message]));
        return true;
    });
}

function activate(engine) {
    let identity;
    if (worker == null) {
        const active_3 = new Worker(new URL('./Worker.js', import.meta.url), { type: 'module', name: 'sir-engine-v1' });
        bind(active_3);
        worker = [engine.Identity, active_3];
        return active_3;
    }
    else if ((identity = worker[0], (worker[1], identity === engine.Identity))) {
        const active_1 = worker[1];
        const identity_1 = worker[0];
        return active_1;
    }
    else {
        const active_2 = worker[1];
        active_2.terminate();
        const replacement = new Worker(new URL('./Worker.js', import.meta.url), { type: 'module', name: 'sir-engine-v1' });
        bind(replacement);
        worker = [engine.Identity, replacement];
        return replacement;
    }
}

function engineIdentity(bytes) {
    return join("", map((value) => format('{0:' + "x2" + '}', value), bytes));
}

export function post(operation, request) {
    const envelope = new WorkerRequestEnvelope(3, OperationIdModule_value(operation), request);
    if (request.tag === 0) {
        const matchValue = Replay_decode(Replay_defaultLimits, request.fields[1]);
        if (matchValue.tag === 1) {
            activate(EngineCatalog_Current).postMessage(envelope);
        }
        else {
            const package$ = matchValue.fields[0];
            const matchValue_1 = EngineCatalog_tryFind(package$);
            if (matchValue_1 == null) {
                dispatch(new Msg(/* RunnerResponded */ 1, [operation, new RunnerResponse(/* RunnerUnsupported */ 6, [("engine " + engineIdentity(package$.EngineHash)) + " is not retained by this publication"])]));
            }
            else {
                const engine = matchValue_1;
                activate(engine).postMessage(envelope);
            }
        }
    }
    else {
        activate(EngineCatalog_Current).postMessage(envelope);
    }
}

/**
 * Sends a simulator-session operation through the retained browser worker.
 * Responses that no longer match the active workspace correlation are
 * discarded in `bind` before any UI subscriber can observe them.
 */
export function postSimulator(correlation, request) {
    const envelope = new SimulatorRequestEnvelope("sir-simulator-session", 1, correlation, request);
    simulatorGuard = ((request.tag === 0) ? SimulatorProtocol_activate(correlation) : SimulatorProtocol_beginOperation(correlation, simulatorGuard));
    return activate(EngineCatalog_Current).postMessage(envelope);
}

export function subscribeSimulator(receive) {
    simulatorSubscriber = receive;
    return {
        Dispose() {
            simulatorSubscriber = undefined;
            simulatorGuard = (new SimulatorWorkspaceGuard(undefined, empty({
                Compare: (x, y) => (comparePrimitives(x, y) | 0),
            })));
        },
    };
}

export function subscribe(send) {
    subscriber = send;
    send(Msg.WorkerStarted);
    return {
        Dispose() {
            const option_1 = worker;
            if (option_1 != null) {
                const tupledArg = option_1;
                tupledArg[1].terminate();
            }
            worker = undefined;
            subscriber = undefined;
        },
    };
}

