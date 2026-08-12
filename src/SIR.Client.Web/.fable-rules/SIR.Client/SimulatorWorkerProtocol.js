
import { Union, Record } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { class_type, bool_type, option_type, array_type, uint8_type, union_type, record_type, int64_type, string_type, int32_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { InspectionProjectionTransport_$reflection } from "./Shell.js";
import { AuthoritativeProjectionFrame_$reflection } from "../SIR.Domain/AuthoritativeProjection.js";
import { exists, empty, append, singleton, enumerateWhile, delay, toArray } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { min } from "../fable_modules/fable-library-js.5.13.0/Double.js";
import { item, equalsWith } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { contains, remove, add, singleton as singleton_1 } from "../fable_modules/fable-library-js.5.13.0/Set.js";
import { comparePrimitives } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { equals } from "../fable_modules/fable-library-js.5.13.0/BigInt.js";
import { toArray as toArray_1 } from "../fable_modules/fable-library-js.5.13.0/Option.js";

/**
 * Identity carried by every simulator request and response. A response is
 * applicable only when all five values still describe the active workspace.
 */
export class SimulatorCorrelation extends Record {
    constructor(Operation, Session, MapRevision, PlanRevision, Tick) {
        super();
        this.Operation = (Operation | 0);
        this.Session = Session;
        this.MapRevision = MapRevision;
        this.PlanRevision = PlanRevision;
        this.Tick = (Tick | 0);
    }
}

export function SimulatorCorrelation_$reflection() {
    return record_type("SIR.Client.SimulatorCorrelation", [], SimulatorCorrelation, () => [["Operation", int32_type], ["Session", string_type], ["MapRevision", string_type], ["PlanRevision", int64_type], ["Tick", int32_type]]);
}

export class SimulatorPreviewLabel extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["DeterministicPreview", "AssumptionBasedPreview", "IntentOnlyPreview"];
    }
    static DeterministicPreview = new SimulatorPreviewLabel(0, []);
    static AssumptionBasedPreview = new SimulatorPreviewLabel(1, []);
    static IntentOnlyPreview = new SimulatorPreviewLabel(2, []);
}

export function SimulatorPreviewLabel_$reflection() {
    return union_type("SIR.Client.SimulatorPreviewLabel", [], SimulatorPreviewLabel, () => [[], [], []]);
}

export class SimulatorInitializationTransport extends Record {
    constructor(InitialProjection, MaximumHorizonTicks) {
        super();
        this.InitialProjection = InitialProjection;
        this.MaximumHorizonTicks = (MaximumHorizonTicks | 0);
    }
}

export function SimulatorInitializationTransport_$reflection() {
    return record_type("SIR.Client.SimulatorInitializationTransport", [], SimulatorInitializationTransport, () => [["InitialProjection", InspectionProjectionTransport_$reflection()], ["MaximumHorizonTicks", int32_type]]);
}

export class SimulatorPlanTransport extends Record {
    constructor(EncodedDocument, HorizonTicks, PreviewLabel, Assumptions, Intents) {
        super();
        this.EncodedDocument = EncodedDocument;
        this.HorizonTicks = (HorizonTicks | 0);
        this.PreviewLabel = PreviewLabel;
        this.Assumptions = Assumptions;
        this.Intents = Intents;
    }
}

export function SimulatorPlanTransport_$reflection() {
    return record_type("SIR.Client.SimulatorPlanTransport", [], SimulatorPlanTransport, () => [["EncodedDocument", array_type(uint8_type)], ["HorizonTicks", int32_type], ["PreviewLabel", SimulatorPreviewLabel_$reflection()], ["Assumptions", array_type(string_type)], ["Intents", array_type(string_type)]]);
}

export class SimulatorDiagnosticTransport extends Record {
    constructor(Code, Field, CommandId, Fields, Detail) {
        super();
        this.Code = Code;
        this.Field = Field;
        this.CommandId = CommandId;
        this.Fields = Fields;
        this.Detail = Detail;
    }
}

export function SimulatorDiagnosticTransport_$reflection() {
    return record_type("SIR.Client.SimulatorDiagnosticTransport", [], SimulatorDiagnosticTransport, () => [["Code", string_type], ["Field", option_type(string_type)], ["CommandId", option_type(string_type)], ["Fields", array_type(StringEntry_$reflection())], ["Detail", string_type]]);
}

export class StringEntry extends Record {
    constructor(Key, Value) {
        super();
        this.Key = Key;
        this.Value = Value;
    }
}

export function StringEntry_$reflection() {
    return record_type("SIR.Client.StringEntry", [], StringEntry, () => [["Key", string_type], ["Value", string_type]]);
}

/**
 * Snapshot/delta transport reuses the bounded inspection projection schema.
 * Deltas contain only fields changed at their tick; empty arrays mean no
 * disclosed change, never "copy the hidden state".
 */
export class SimulatorProjectionUpdateTransport extends Record {
    constructor(IsSnapshot, Projection) {
        super();
        this.IsSnapshot = IsSnapshot;
        this.Projection = Projection;
    }
}

export function SimulatorProjectionUpdateTransport_$reflection() {
    return record_type("SIR.Client.SimulatorProjectionUpdateTransport", [], SimulatorProjectionUpdateTransport, () => [["IsSnapshot", bool_type], ["Projection", InspectionProjectionTransport_$reflection()]]);
}

export class SimulatorRequest extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["InitializeSession", "ValidatePlan", "PreviewPlan", "CommitPlan", "Step", "RunTo", "Reset", "CancelOperation", "LoadAuthoritativeRun"];
    }
    static Reset = new SimulatorRequest(6, []);
}

export function SimulatorRequest_$reflection() {
    return union_type("SIR.Client.SimulatorRequest", [], SimulatorRequest, () => [[["Item", SimulatorInitializationTransport_$reflection()]], [["Item", SimulatorPlanTransport_$reflection()]], [["plan", SimulatorPlanTransport_$reflection()], ["fromTick", int32_type], ["toTick", int32_type]], [["Item", SimulatorPlanTransport_$reflection()]], [["tickCount", int32_type]], [["targetTick", int32_type]], [], [["targetOperation", int32_type]], [["matchLock", string_type], ["replayIdentity", string_type], ["projections", array_type(AuthoritativeProjectionFrame_$reflection())]]]);
}

export class SimulatorResponse extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["SessionInitialized", "PlanValidated", "PlanPreviewed", "PlanCommitted", "SimulatorStepped", "SimulatorProgress", "SimulatorRunCompleted", "SimulatorReset", "SimulatorOperationCancelled", "SimulatorRequestRejected", "AuthoritativeRunLoaded"];
    }
}

export function SimulatorResponse_$reflection() {
    return union_type("SIR.Client.SimulatorResponse", [], SimulatorResponse, () => [[["Item", SimulatorProjectionUpdateTransport_$reflection()]], [["acceptedRevision", option_type(int64_type)], ["Item2", array_type(SimulatorDiagnosticTransport_$reflection())]], [["label", SimulatorPreviewLabel_$reflection()], ["disclosures", array_type(string_type)], ["Item3", array_type(SimulatorProjectionUpdateTransport_$reflection())]], [["acceptedRevision", int64_type]], [["Item", SimulatorProjectionUpdateTransport_$reflection()]], [["completedBatches", int32_type], ["Item2", SimulatorProjectionUpdateTransport_$reflection()]], [["Item", SimulatorProjectionUpdateTransport_$reflection()]], [["Item", SimulatorProjectionUpdateTransport_$reflection()]], [["targetOperation", int32_type]], [["code", string_type], ["detail", string_type]], [["matchLock", string_type], ["replayIdentity", string_type], ["finalTick", int32_type]]]);
}

export class SimulatorRequestEnvelope extends Record {
    constructor(Kind, ProtocolVersion, Correlation, Request) {
        super();
        this.Kind = Kind;
        this.ProtocolVersion = (ProtocolVersion | 0);
        this.Correlation = Correlation;
        this.Request = Request;
    }
}

export function SimulatorRequestEnvelope_$reflection() {
    return record_type("SIR.Client.SimulatorRequestEnvelope", [], SimulatorRequestEnvelope, () => [["Kind", string_type], ["ProtocolVersion", int32_type], ["Correlation", SimulatorCorrelation_$reflection()], ["Request", SimulatorRequest_$reflection()]]);
}

export class SimulatorResponseEnvelope extends Record {
    constructor(Kind, ProtocolVersion, Correlation, CurrentTick, Response) {
        super();
        this.Kind = Kind;
        this.ProtocolVersion = (ProtocolVersion | 0);
        this.Correlation = Correlation;
        this.CurrentTick = (CurrentTick | 0);
        this.Response = Response;
    }
}

export function SimulatorResponseEnvelope_$reflection() {
    return record_type("SIR.Client.SimulatorResponseEnvelope", [], SimulatorResponseEnvelope, () => [["Kind", string_type], ["ProtocolVersion", int32_type], ["Correlation", SimulatorCorrelation_$reflection()], ["CurrentTick", int32_type], ["Response", SimulatorResponse_$reflection()]]);
}

export class SimulatorWorkspaceGuard extends Record {
    constructor(Active, PendingOperations) {
        super();
        this.Active = Active;
        this.PendingOperations = PendingOperations;
    }
}

export function SimulatorWorkspaceGuard_$reflection() {
    return record_type("SIR.Client.SimulatorWorkspaceGuard", [], SimulatorWorkspaceGuard, () => [["Active", option_type(SimulatorCorrelation_$reflection())], ["PendingOperations", class_type("Microsoft.FSharp.Collections.FSharpSet`1", [int32_type])]]);
}

export function SimulatorProtocol_batchEnds(startTick, targetTick) {
    return toArray(delay(() => {
        let tick = startTick;
        return enumerateWhile(() => (tick < targetTick), delay(() => {
            tick = (min(targetTick, tick + 256) | 0);
            return singleton(tick);
        }));
    }));
}

export function SimulatorProtocol_diagnostics(maximumHorizon, plan) {
    const header = new Uint8Array([83, 73, 82, 45, 80, 76, 65, 78, 32, 49]);
    return toArray(delay(() => append((plan.EncodedDocument.length === 0) ? singleton(new SimulatorDiagnosticTransport("SIR.SIMULATOR.PLAN.EMPTY", "EncodedDocument", undefined, [], "The canonical SIR-PLAN document is empty.")) : (((((plan.EncodedDocument.length <= header.length) ? true : !equalsWith((x, y) => (x === y), plan.EncodedDocument.slice(0, (header.length - 1) + 1), header)) ? true : (item(header.length, plan.EncodedDocument) !== 10)) ? true : (item(plan.EncodedDocument.length - 1, plan.EncodedDocument) !== 10)) ? singleton(new SimulatorDiagnosticTransport("SIR.PLAN.STRUCTURAL.BAD_HEADER", "EncodedDocument", undefined, [], "The plan is not a canonical SIR-PLAN 1 line document.")) : empty()), delay(() => append((plan.EncodedDocument.length > 262144) ? singleton(new SimulatorDiagnosticTransport("SIR.SIMULATOR.PLAN.SIZE", "EncodedDocument", undefined, [], "The canonical SIR-PLAN document exceeds the worker limit.")) : empty(), delay(() => append(((plan.HorizonTicks <= 0) ? true : (plan.HorizonTicks > maximumHorizon)) ? singleton(new SimulatorDiagnosticTransport("SIR.SIMULATOR.PLAN.HORIZON", "HorizonTicks", undefined, [], "The planning horizon is outside the initialized session limit.")) : empty(), delay(() => {
        const matchValue = plan.PreviewLabel;
        let matchResult;
        switch (matchValue.tag) {
            case 1: {
                if (plan.Assumptions.length === 0) {
                    matchResult = 1;
                }
                else {
                    matchResult = 3;
                }
                break;
            }
            case 2: {
                if (plan.Intents.length === 0) {
                    matchResult = 2;
                }
                else {
                    matchResult = 3;
                }
                break;
            }
            default:
                if (plan.Assumptions.length !== 0) {
                    matchResult = 0;
                }
                else {
                    matchResult = 3;
                }
        }
        switch (matchResult) {
            case 0:
                return singleton(new SimulatorDiagnosticTransport("SIR.SIMULATOR.PREVIEW.DETERMINISTIC_ASSUMPTIONS", "Assumptions", undefined, [], "A deterministic preview cannot carry assumptions."));
            case 1:
                return singleton(new SimulatorDiagnosticTransport("SIR.SIMULATOR.PREVIEW.ASSUMPTIONS_REQUIRED", "Assumptions", undefined, [], "An assumption-based preview must disclose its assumptions."));
            case 2:
                return singleton(new SimulatorDiagnosticTransport("SIR.SIMULATOR.PREVIEW.INTENTS_REQUIRED", "Intents", undefined, [], "An intent-only preview must disclose at least one intent."));
            default: {
                return empty();
            }
        }
    }))))))));
}

export function SimulatorProtocol_activate(correlation) {
    return new SimulatorWorkspaceGuard(correlation, singleton_1(correlation.Operation, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }));
}

export function SimulatorProtocol_beginOperation(correlation, guard) {
    return new SimulatorWorkspaceGuard(correlation, add(correlation.Operation, guard.PendingOperations));
}

export function SimulatorProtocol_completeOperation(operation, guard) {
    return new SimulatorWorkspaceGuard(guard.Active, remove(operation, guard.PendingOperations));
}

/**
 * The browser applies this before dispatching into workspace state.
 */
export function SimulatorProtocol_accepts(envelope, guard) {
    if (((envelope.Kind === "sir-simulator-session") && (envelope.ProtocolVersion === 1)) && contains(envelope.Correlation.Operation, guard.PendingOperations)) {
        return exists((active) => {
            if ((((active.Operation === envelope.Correlation.Operation) && (active.Session === envelope.Correlation.Session)) && (active.MapRevision === envelope.Correlation.MapRevision)) && equals(active.PlanRevision, envelope.Correlation.PlanRevision)) {
                return active.Tick === envelope.Correlation.Tick;
            }
            else {
                return false;
            }
        }, toArray_1(guard.Active));
    }
    else {
        return false;
    }
}

