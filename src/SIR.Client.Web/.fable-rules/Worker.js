
import { EngineCatalog_Current } from "./SIR.Client/EngineCatalog.js";
import { isNullOrWhiteSpace, format, join } from "./fable_modules/fable-library-js.5.13.0/String.js";
import { item, pairwise, tryFind, take, map } from "./fable_modules/fable-library-js.5.13.0/Array.js";
import { tryLast, length, mapIndexed, filter, sortByDescending, tryHead, choose, append, sortBy, map as map_1, empty } from "./fable_modules/fable-library-js.5.13.0/List.js";
import { OperationIdModule_create, RunnerClaim, WorkerTransport_metadataToTransport, WorkerTransport_inspectionToTransport, WorkerProtocol_batchEnds, InspectionProjectionTransport, WorkerResponseEnvelope, OperationIdModule_value, InspectionProjectionTransport_$reflection, RunnerResponse, CheckpointProjection, EdgeProjection, EventProjection, UnitProjection, ReplayMetadata, ReplayKind, InspectionProjection } from "./SIR.Client/Shell.js";
import { numberHash, comparePrimitives, int32ToString } from "./fable_modules/fable-library-js.5.13.0/Util.js";
import { Simulation_runTick, Simulation_unitIdValue } from "./SIR.Simulation/Simulation.js";
import { min, max } from "./fable_modules/fable-library-js.5.13.0/Double.js";
import { defaultArg } from "./fable_modules/fable-library-js.5.13.0/Option.js";
import { BoundedInt32Module_maximum, BoundedInt32Module_value } from "./SIR.Domain/BoundedInt32.js";
import { Direction8, Direction8Module_toCode } from "./SIR.Domain/Orientation.js";
import { FSharpMap__get_IsEmpty, tryFind as tryFind_1, ofArray, empty as empty_2, toList } from "./fable_modules/fable-library-js.5.13.0/Map.js";
import { sha256 } from "./SIR.Domain/CanonicalHash.js";
import { Replay_runKernelReplay, Replay_defaultLimits, Replay_decode, Replay_encode } from "./SIR.Simulation/Replay.js";
import { add, contains, empty as empty_1 } from "./fable_modules/fable-library-js.5.13.0/Set.js";
import { Record } from "./fable_modules/fable-library-js.5.13.0/Types.js";
import { record_type, class_type, option_type, int32_type, int64_type, string_type } from "./fable_modules/fable-library-js.5.13.0/Reflection.js";
import { SimulatorProtocol_batchEnds, SimulatorProtocol_diagnostics, SimulatorResponse, SimulatorProjectionUpdateTransport, SimulatorResponseEnvelope, SimulatorPlanTransport_$reflection } from "./SIR.Client/SimulatorWorkerProtocol.js";
import { equals, compare } from "./fable_modules/fable-library-js.5.13.0/BigInt.js";
import { Array_distinctBy } from "./fable_modules/fable-library-js.5.13.0/Seq2.js";
import { singleton } from "./fable_modules/fable-library-js.5.13.0/AsyncBuilder.js";
import { startImmediate, sleep } from "./fable_modules/fable-library-js.5.13.0/Async.js";
import { Lab_parametersFromTransport, Lab_reportToTransport, Lab_scenarioToTransport, Lab_run, Lab_tryScenario } from "./SIR.Client/Lab.js";

const scope = globalThis;

const supportedEngine = EngineCatalog_Current.EngineHash;

function shortIdentity(bytes) {
    return join("", map((value) => format('{0:' + "x2" + '}', value), take(6, bytes, Uint8Array)));
}

function emptyProjection(tick) {
    return new InspectionProjection(tick, 0, 0, 0, 0, empty(), empty(), empty(), empty(), undefined);
}

function scenarioMetadata(scenario, report) {
    return new ReplayMetadata(scenario.Identity + ".sir-scenario", report.Comparison.Baseline.ResultIdentity, scenario.EngineIdentity, 1, ReplayKind.DesignScenario);
}

function inputSummary(input) {
    switch (input.tag) {
        case 1:
            return (("unit " + int32ToString(Simulation_unitIdValue(input.fields[0]))) + " observes unit ") + int32ToString(Simulation_unitIdValue(input.fields[1]));
        case 2:
            return (("unit " + int32ToString(Simulation_unitIdValue(input.fields[0]))) + " attacks unit ") + int32ToString(Simulation_unitIdValue(input.fields[1]));
        default: {
            const destination = input.fields[1];
            return (((("unit " + int32ToString(Simulation_unitIdValue(input.fields[0]))) + " moves to ") + int32ToString(destination.Col)) + ",") + int32ToString(destination.Row);
        }
    }
}

function inputUnits(input) {
    let matchResult, observerId, targetId;
    switch (input.tag) {
        case 1: {
            matchResult = 1;
            observerId = input.fields[0];
            targetId = input.fields[1];
            break;
        }
        case 2: {
            matchResult = 1;
            observerId = input.fields[0];
            targetId = input.fields[1];
            break;
        }
        default:
            matchResult = 0;
    }
    switch (matchResult) {
        case 0:
            return [Simulation_unitIdValue(input.fields[0]), undefined];
        default:
            return [Simulation_unitIdValue(observerId), Simulation_unitIdValue(targetId)];
    }
}

function journalAt(tick, full) {
    return map_1((tuple_1) => tuple_1[1], sortBy((tuple) => (tuple[0] | 0), append(choose((input) => {
        if (input.Tick === tick) {
            return [input.Sequence, input.Input];
        }
        else {
            return undefined;
        }
    }, full.OrderedInputs), choose((output) => {
        if (output.Tick === tick) {
            return [output.Sequence, output.Input];
        }
        else {
            return undefined;
        }
    }, full.AcceptedWasmOutputs)), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }));
}

function stateAt(tick, full) {
    let option_1;
    const target = max(full.InitialSnapshot.Tick, min(full.FinalResult.Tick, tick)) | 0;
    const start = defaultArg((option_1 = tryHead(sortByDescending((checkpoint_1) => (checkpoint_1.Tick | 0), filter((checkpoint) => (checkpoint.Tick <= target), full.Checkpoints), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    })), (option_1 != null) ? option_1.State : undefined), full.InitialSnapshot);
    let state = start;
    for (let current = start.Tick + 1; current <= target; current++) {
        state = Simulation_runTick(state, journalAt(current, full)).State;
    }
    return state;
}

function fullProjection(tick, full) {
    const state = stateAt(tick, full);
    const units = map_1((tupledArg) => {
        const unit = tupledArg[1];
        return new UnitProjection(Simulation_unitIdValue(unit.Id), (unit.Side.tag === 1) ? "Blue" : "Red", unit.Cell.Col, unit.Cell.Row, BoundedInt32Module_value(unit.Health), BoundedInt32Module_maximum(unit.Health), undefined, ~~Direction8Module_toCode(unit.BodyFacing), ~~Direction8Module_toCode(unit.AttentionDirection));
    }, toList(state.Units));
    const externalEvents = mapIndexed((index, input) => {
        const patternInput = inputUnits(input.Input);
        return new EventProjection(index, input.Tick, "External input", inputSummary(input.Input), patternInput[0], patternInput[1]);
    }, full.OrderedInputs);
    const wasmEvents = mapIndexed((index_1, output) => {
        const patternInput_1 = inputUnits(output.Input);
        return new EventProjection(length(externalEvents) + index_1, output.Tick, "Accepted WASM output", inputSummary(output.Input), patternInput_1[0], patternInput_1[1]);
    }, full.AcceptedWasmOutputs);
    const edges = mapIndexed((index_2, edge) => (new EdgeProjection("edge-" + int32ToString(index_2), "wall", edge.BlocksMovement ? "solid" : "open", edge.Edge.Lo.Col, edge.Edge.Lo.Row, edge.Edge.Hi.Col, edge.Edge.Hi.Row)), state.Board.Edges);
    const checkpoints = map_1((checkpoint) => (new CheckpointProjection(checkpoint.Tick, shortIdentity(checkpoint.StateHash), shortIdentity(checkpoint.EventHash))), full.Checkpoints);
    return new InspectionProjection(state.Tick, state.Board.Minimum.Col, state.Board.Minimum.Row, state.Board.Maximum.Col, state.Board.Maximum.Row, units, edges, append(externalEvents, wasmEvents), checkpoints, undefined);
}

function perspectiveProjection(tick, frames) {
    let frame;
    const option_1 = tryLast(filter((candidate) => (candidate.Tick <= tick), frames));
    frame = ((option_1 != null) ? option_1 : tryHead(frames));
    if (frame == null) {
        return emptyProjection(0);
    }
    else {
        const selected = frame;
        const bind$0040 = emptyProjection(selected.Tick);
        return new InspectionProjection(bind$0040.Tick, bind$0040.BoardMinimumColumn, bind$0040.BoardMinimumRow, bind$0040.BoardMaximumColumn, bind$0040.BoardMaximumRow, bind$0040.Units, bind$0040.Edges, bind$0040.Events, bind$0040.Checkpoints, shortIdentity(selected.ProjectionHash));
    }
}

function projectionAt(tick, package$) {
    const matchValue = package$.Content;
    if (matchValue.tag === 1) {
        return perspectiveProjection(tick, matchValue.fields[0]);
    }
    else {
        return fullProjection(tick, matchValue.fields[0]);
    }
}

function replayError(error) {
    switch (error.tag) {
        case 4:
            return new RunnerResponse(/* RunnerUnsupported */ 6, ["the required retained engine bundle is unavailable"]);
        case 9:
            return new RunnerResponse(/* RunnerDiverged */ 7, [error.fields[0], "kernel", error.fields[1]]);
        case 0:
            return new RunnerResponse(/* RunnerFailed */ 8, [(("package size " + int32ToString(error.fields[0])) + " exceeds ") + int32ToString(error.fields[1])]);
        case 1:
            return new RunnerResponse(/* RunnerFailed */ 8, [error.fields[0]]);
        case 5:
            return new RunnerResponse(/* RunnerFailed */ 8, ["the full replay is not authorized"]);
        case 3:
            return new RunnerResponse(/* RunnerFailed */ 8, ["invalid hash length for " + error.fields[0]]);
        case 6:
            return new RunnerResponse(/* RunnerFailed */ 8, ["resource limit exceeded for " + error.fields[0]]);
        case 7:
            return new RunnerResponse(/* RunnerFailed */ 8, ["invalid canonical ordering for " + error.fields[0]]);
        case 8:
            return new RunnerResponse(/* RunnerFailed */ 8, [(("invalid checkpoint at tick " + int32ToString(error.fields[0])) + ": ") + error.fields[1]]);
        case 10:
            return new RunnerResponse(/* RunnerFailed */ 8, ["perspective playback has no reconstructable kernel"]);
        case 11:
            return new RunnerResponse(/* RunnerFailed */ 8, ["browser verification does not include WASM execution"]);
        case 12:
            return new RunnerResponse(/* RunnerDiverged */ 7, [error.fields[0], "WASM re-execution", "accepted output sequence " + int32ToString(error.fields[1])]);
        default:
            return new RunnerResponse(/* RunnerUnsupported */ 6, [(("format " + int32ToString(error.fields[0])) + " is not supported; expected ") + int32ToString(error.fields[1])]);
    }
}

function metadata(sourceName, package$) {
    let option_1;
    let patternInput;
    const matchValue = package$.Content;
    patternInput = ((matchValue.tag === 1) ? [ReplayKind.PerspectiveReplay, defaultArg((option_1 = tryLast(matchValue.fields[0]), (option_1 != null) ? option_1.Tick : undefined), 0)] : [ReplayKind.FullReplay, matchValue.fields[0].FinalResult.Tick]);
    return new ReplayMetadata(sourceName, shortIdentity(sha256(Replay_encode(package$))), shortIdentity(package$.EngineHash), patternInput[1], patternInput[0]);
}

let loadedPackage = undefined;

let cancelled = empty_1({
    Compare: (x, y) => (comparePrimitives(x, y) | 0),
});

class SimulatorSessionState extends Record {
    constructor(Session, MapRevision, PlanRevision, InitialProjection, CurrentProjection, MaximumHorizonTicks, CommittedPlan, AuthoritativeRun) {
        super();
        this.Session = Session;
        this.MapRevision = MapRevision;
        this.PlanRevision = PlanRevision;
        this.InitialProjection = InitialProjection;
        this.CurrentProjection = CurrentProjection;
        this.MaximumHorizonTicks = (MaximumHorizonTicks | 0);
        this.CommittedPlan = CommittedPlan;
        this.AuthoritativeRun = AuthoritativeRun;
    }
}

function SimulatorSessionState_$reflection() {
    return record_type("SIR.Client.Web.Worker.SimulatorSessionState", [], SimulatorSessionState, () => [["Session", string_type], ["MapRevision", string_type], ["PlanRevision", int64_type], ["InitialProjection", InspectionProjectionTransport_$reflection()], ["CurrentProjection", InspectionProjectionTransport_$reflection()], ["MaximumHorizonTicks", int32_type], ["CommittedPlan", option_type(SimulatorPlanTransport_$reflection())], ["AuthoritativeRun", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, InspectionProjectionTransport_$reflection()])]]);
}

let simulatorSession = undefined;

let cancelledSimulatorOperations = empty_1({
    Compare: (x, y) => (comparePrimitives(x, y) | 0),
});

function post(operation, response) {
    const envelope = new WorkerResponseEnvelope(3, OperationIdModule_value(operation), response);
    return scope.postMessage(envelope);
}

function isCancelled(operation) {
    const set$ = cancelled;
    return contains(OperationIdModule_value(operation), set$);
}

function postSimulator(correlation, response) {
    let option_1;
    const envelope = new SimulatorResponseEnvelope("sir-simulator-session", 1, correlation, defaultArg((option_1 = simulatorSession, (option_1 != null) ? option_1.CurrentProjection.Tick : undefined), correlation.Tick), response);
    return scope.postMessage(envelope);
}

function simulatorUpdate(isSnapshot, projection) {
    return new SimulatorProjectionUpdateTransport(isSnapshot, projection);
}

function simulatorDisclosure(plan) {
    const matchValue = plan.PreviewLabel;
    switch (matchValue.tag) {
        case 1:
            return map((assumption) => ("Assumption: " + assumption), plan.Assumptions);
        case 2:
            return map((intent) => ("Intent only: " + intent), plan.Intents);
        default:
            return ["Deterministic from committed, disclosed session inputs."];
    }
}

function validateSimulatorCorrelation(correlation, session) {
    if ((correlation.Session === session.Session) && (correlation.MapRevision === session.MapRevision)) {
        return correlation.Tick === session.CurrentProjection.Tick;
    }
    else {
        return false;
    }
}

function simulatorDelta(tick, projection) {
    return new InspectionProjectionTransport(tick, projection.BoardMinimumColumn, projection.BoardMinimumRow, projection.BoardMaximumColumn, projection.BoardMaximumRow, [], [], [], [], projection.PerspectiveHash);
}

function authoritativeProjection(initial, frame) {
    return new InspectionProjectionTransport(frame.Tick, initial.BoardMinimumColumn, initial.BoardMinimumRow, initial.BoardMaximumColumn, initial.BoardMaximumRow, map((visible) => {
        const matchValue = tryFind((unit) => (unit.Id === visible.UnitId), initial.Units);
        if (matchValue == null) {
            return new UnitProjection(visible.UnitId, "disclosed", visible.DisplayColumn, visible.DisplayRow, visible.Health, max(1, visible.Health), undefined, ~~Direction8Module_toCode(Direction8.North), ~~Direction8Module_toCode(Direction8.North));
        }
        else {
            const unit_1 = matchValue;
            return new UnitProjection(unit_1.Id, unit_1.Side, visible.DisplayColumn, visible.DisplayRow, visible.Health, unit_1.HealthMaximum, unit_1.MovementDirection, unit_1.BodyFacing, unit_1.AttentionDirection);
        }
    }, frame.VisibleUnits), initial.Edges, [], [new CheckpointProjection(frame.Tick, shortIdentity(frame.StateIdentity), shortIdentity(frame.EventIdentity))], initial.PerspectiveHash);
}

function validAuthoritativeFrame(initial, frame) {
    let array_1;
    if (((((((frame.Tick > initial.Tick) && (compare(frame.ServerSequence, 0n) > 0)) && (compare(frame.ProjectionRevision, 0n) > 0)) && (frame.StateIdentity.length === 32)) && (frame.EventIdentity.length === 32)) && (frame.VisibleUnits.length <= 256)) && (((array_1 = Array_distinctBy((_arg) => (_arg.UnitId | 0), frame.VisibleUnits, {
        Equals: (x, y) => (x === y),
        GetHashCode: (x) => (numberHash(x) | 0),
    }), array_1.length)) === frame.VisibleUnits.length)) {
        return frame.VisibleUnits.every((unit) => {
            if (((((unit.UnitId > 0) && (unit.DisplayColumn >= initial.BoardMinimumColumn)) && (unit.DisplayColumn <= initial.BoardMaximumColumn)) && (unit.DisplayRow >= initial.BoardMinimumRow)) && (unit.DisplayRow <= initial.BoardMaximumRow)) {
                return unit.Health >= 0;
            }
            else {
                return false;
            }
        });
    }
    else {
        return false;
    }
}

function executeSimulator(correlation, request) {
    return singleton.Delay(() => {
        let session_1, bind$0040, plan_3;
        switch (request.tag) {
            case 0: {
                const initialization = request.fields[0];
                if ((((isNullOrWhiteSpace(correlation.Session) ? true : isNullOrWhiteSpace(correlation.MapRevision)) ? true : (initialization.InitialProjection.Tick !== correlation.Tick)) ? true : (initialization.MaximumHorizonTicks <= 0)) ? true : (initialization.MaximumHorizonTicks > 6000)) {
                    postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.SESSION.INVALID", "Session, map revision, tick, and horizon must form a valid initialization."]));
                    return singleton.Zero();
                }
                else {
                    const session = new SimulatorSessionState(correlation.Session, correlation.MapRevision, correlation.PlanRevision, initialization.InitialProjection, initialization.InitialProjection, initialization.MaximumHorizonTicks, undefined, empty_2({
                        Compare: (x, y) => (comparePrimitives(x, y) | 0),
                    }));
                    simulatorSession = session;
                    cancelledSimulatorOperations = empty_1({
                        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
                    });
                    postSimulator(correlation, new SimulatorResponse(/* SessionInitialized */ 0, [simulatorUpdate(true, initialization.InitialProjection)]));
                    return singleton.Zero();
                }
            }
            case 7: {
                const targetOperation = request.fields[0] | 0;
                let matchResult, session_2;
                if (simulatorSession != null) {
                    if ((session_1 = simulatorSession, validateSimulatorCorrelation(correlation, session_1) && equals(correlation.PlanRevision, session_1.PlanRevision))) {
                        matchResult = 0;
                        session_2 = simulatorSession;
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
                        cancelledSimulatorOperations = add(targetOperation, cancelledSimulatorOperations);
                        postSimulator(correlation, new SimulatorResponse(/* SimulatorOperationCancelled */ 8, [targetOperation]));
                        return singleton.Zero();
                    }
                    default: {
                        postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.CORRELATION.STALE", "Cancellation does not match the active simulator workspace."]));
                        return singleton.Zero();
                    }
                }
            }
            default:
                if (simulatorSession != null) {
                    if (!validateSimulatorCorrelation(correlation, simulatorSession)) {
                        const session_4 = simulatorSession;
                        postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.CORRELATION.STALE", "The session, map revision, or tick is stale."]));
                        return singleton.Zero();
                    }
                    else {
                        const session_5 = simulatorSession;
                        switch (request.tag) {
                            case 2: {
                                const toTick = request.fields[2] | 0;
                                const plan_1 = request.fields[0];
                                const fromTick = request.fields[1] | 0;
                                const diagnostics_1 = SimulatorProtocol_diagnostics(session_5.MaximumHorizonTicks, plan_1);
                                if (compare(correlation.PlanRevision, session_5.PlanRevision) < 0) {
                                    postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.PLAN.STALE", "The plan revision is stale."]));
                                    return singleton.Zero();
                                }
                                else if (((fromTick !== correlation.Tick) ? true : (toTick < fromTick)) ? true : ((toTick - fromTick) > 1200)) {
                                    postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.PREVIEW.HORIZON", "A preview must start at the expected tick and span at most 1,200 ticks."]));
                                    return singleton.Zero();
                                }
                                else if (diagnostics_1.length !== 0) {
                                    postSimulator(correlation, new SimulatorResponse(/* PlanValidated */ 1, [undefined, diagnostics_1]));
                                    return singleton.Zero();
                                }
                                else {
                                    const projection = (plan_1.PreviewLabel.tag === 2) ? simulatorDelta(session_5.CurrentProjection.Tick, session_5.CurrentProjection) : session_5.CurrentProjection;
                                    postSimulator(correlation, new SimulatorResponse(/* PlanPreviewed */ 2, [plan_1.PreviewLabel, simulatorDisclosure(plan_1), [simulatorUpdate(true, projection)]]));
                                    return singleton.Zero();
                                }
                            }
                            case 3: {
                                const plan_2 = request.fields[0];
                                const diagnostics_2 = SimulatorProtocol_diagnostics(session_5.MaximumHorizonTicks, plan_2);
                                if (compare(correlation.PlanRevision, session_5.PlanRevision) < 0) {
                                    postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.PLAN.STALE", "The plan revision is older than the active plan."]));
                                    return singleton.Zero();
                                }
                                else if (diagnostics_2.length !== 0) {
                                    postSimulator(correlation, new SimulatorResponse(/* PlanValidated */ 1, [undefined, diagnostics_2]));
                                    return singleton.Zero();
                                }
                                else {
                                    simulatorSession = (new SimulatorSessionState(session_5.Session, session_5.MapRevision, correlation.PlanRevision, session_5.InitialProjection, session_5.CurrentProjection, session_5.MaximumHorizonTicks, plan_2, session_5.AuthoritativeRun));
                                    postSimulator(correlation, new SimulatorResponse(/* PlanCommitted */ 3, [correlation.PlanRevision]));
                                    return singleton.Zero();
                                }
                            }
                            case 8: {
                                const updates = request.fields[2];
                                const replayIdentity = request.fields[1];
                                const matchLock = request.fields[0];
                                const projections = map((frame) => authoritativeProjection(session_5.InitialProjection, frame), updates);
                                let ordered;
                                const array_2 = pairwise(updates);
                                ordered = array_2.every((tupledArg) => {
                                    const left = tupledArg[0];
                                    const right = tupledArg[1];
                                    if ((left.Tick < right.Tick) && (compare(left.ServerSequence, right.ServerSequence) < 0)) {
                                        return compare(left.ProjectionRevision, right.ProjectionRevision) < 0;
                                    }
                                    else {
                                        return false;
                                    }
                                });
                                if ((((((((session_5.CommittedPlan == null) ? true : isNullOrWhiteSpace(matchLock)) ? true : isNullOrWhiteSpace(replayIdentity)) ? true : (projections.length === 0)) ? true : !ordered) ? true : !updates.every((frame_1) => validAuthoritativeFrame(session_5.InitialProjection, frame_1))) ? true : (item(0, projections).Tick <= session_5.InitialProjection.Tick)) ? true : (item(projections.length - 1, projections).Tick > (session_5.InitialProjection.Tick + session_5.MaximumHorizonTicks))) {
                                    postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.AUTHORITATIVE_RUN.INVALID", "A qualified run requires pinned identities and ordered bounded projections."]));
                                    return singleton.Zero();
                                }
                                else {
                                    const run = ofArray(map((projection_1) => [projection_1.Tick, projection_1], projections), {
                                        Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
                                    });
                                    simulatorSession = (new SimulatorSessionState(session_5.Session, session_5.MapRevision, session_5.PlanRevision, session_5.InitialProjection, session_5.CurrentProjection, session_5.MaximumHorizonTicks, session_5.CommittedPlan, run));
                                    postSimulator(correlation, new SimulatorResponse(/* AuthoritativeRunLoaded */ 10, [matchLock, replayIdentity, item(projections.length - 1, projections).Tick]));
                                    return singleton.Zero();
                                }
                            }
                            case 4: {
                                const tickCount = request.fields[0] | 0;
                                if (!equals(correlation.PlanRevision, session_5.PlanRevision)) {
                                    postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.PLAN.STALE", "Step does not match the committed plan revision."]));
                                    return singleton.Zero();
                                }
                                else if (session_5.CommittedPlan != null) {
                                    if ((tickCount <= 0) ? true : (tickCount > 256)) {
                                        postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.STEP.COUNT", "A step must advance between 1 and 256 ticks."]));
                                        return singleton.Zero();
                                    }
                                    else {
                                        const tick = (session_5.CurrentProjection.Tick + tickCount) | 0;
                                        const matchValue_2 = tryFind_1(tick, session_5.AuthoritativeRun);
                                        if (matchValue_2 == null) {
                                            if (FSharpMap__get_IsEmpty(session_5.AuthoritativeRun)) {
                                                const delta = simulatorDelta(tick, session_5.CurrentProjection);
                                                simulatorSession = (new SimulatorSessionState(session_5.Session, session_5.MapRevision, session_5.PlanRevision, session_5.InitialProjection, (bind$0040 = session_5.CurrentProjection, new InspectionProjectionTransport(tick, bind$0040.BoardMinimumColumn, bind$0040.BoardMinimumRow, bind$0040.BoardMaximumColumn, bind$0040.BoardMaximumRow, bind$0040.Units, bind$0040.Edges, bind$0040.Events, bind$0040.Checkpoints, bind$0040.PerspectiveHash)), session_5.MaximumHorizonTicks, session_5.CommittedPlan, session_5.AuthoritativeRun));
                                                postSimulator(correlation, new SimulatorResponse(/* SimulatorStepped */ 4, [simulatorUpdate(false, delta)]));
                                                return singleton.Zero();
                                            }
                                            else {
                                                postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.AUTHORITATIVE_RUN.MISSING_TICK", "No qualified authoritative projection exists for the requested tick."]));
                                                return singleton.Zero();
                                            }
                                        }
                                        else {
                                            const projection_2 = matchValue_2;
                                            simulatorSession = (new SimulatorSessionState(session_5.Session, session_5.MapRevision, session_5.PlanRevision, session_5.InitialProjection, projection_2, session_5.MaximumHorizonTicks, session_5.CommittedPlan, session_5.AuthoritativeRun));
                                            postSimulator(correlation, new SimulatorResponse(/* SimulatorStepped */ 4, [simulatorUpdate(false, projection_2)]));
                                            return singleton.Zero();
                                        }
                                    }
                                }
                                else {
                                    postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.PLAN.NOT_COMMITTED", "Commit a valid plan before stepping."]));
                                    return singleton.Zero();
                                }
                            }
                            case 5: {
                                const targetTick = request.fields[0] | 0;
                                const matchValue_3 = session_5.CommittedPlan;
                                if (!equals(correlation.PlanRevision, session_5.PlanRevision)) {
                                    postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.PLAN.STALE", "Run-to does not match the committed plan revision."]));
                                    return singleton.Zero();
                                }
                                else if (matchValue_3 != null) {
                                    if ((plan_3 = matchValue_3, (targetTick < session_5.CurrentProjection.Tick) ? true : (targetTick > (session_5.InitialProjection.Tick + plan_3.HorizonTicks)))) {
                                        const plan_4 = matchValue_3;
                                        postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.RUN.TARGET", "The target tick is outside the committed planning horizon."]));
                                        return singleton.Zero();
                                    }
                                    else {
                                        const batchEnds = SimulatorProtocol_batchEnds(session_5.CurrentProjection.Tick, targetTick);
                                        let current = session_5;
                                        let completed = 0;
                                        let stopped = false;
                                        return singleton.For(batchEnds, (_arg) => {
                                            const batchEnd = _arg | 0;
                                            return !stopped ? singleton.Bind(sleep(0), () => {
                                                let CurrentProjection_1, bind$0040_1;
                                                if (contains(correlation.Operation, cancelledSimulatorOperations)) {
                                                    stopped = true;
                                                    postSimulator(correlation, new SimulatorResponse(/* SimulatorOperationCancelled */ 8, [correlation.Operation]));
                                                    return singleton.Zero();
                                                }
                                                else {
                                                    completed = ((completed + 1) | 0);
                                                    const matchValue_4 = tryFind_1(batchEnd, current.AuthoritativeRun);
                                                    if (matchValue_4 != null) {
                                                        const projection_3 = matchValue_4;
                                                        current = (new SimulatorSessionState(current.Session, current.MapRevision, current.PlanRevision, current.InitialProjection, projection_3, current.MaximumHorizonTicks, current.CommittedPlan, current.AuthoritativeRun));
                                                        simulatorSession = current;
                                                        if (batchEnd === targetTick) {
                                                            postSimulator(correlation, new SimulatorResponse(/* SimulatorRunCompleted */ 6, [simulatorUpdate(false, projection_3)]));
                                                            return singleton.Zero();
                                                        }
                                                        else {
                                                            postSimulator(correlation, new SimulatorResponse(/* SimulatorProgress */ 5, [completed, simulatorUpdate(false, projection_3)]));
                                                            return singleton.Zero();
                                                        }
                                                    }
                                                    else if (FSharpMap__get_IsEmpty(current.AuthoritativeRun)) {
                                                        const delta_1 = simulatorDelta(batchEnd, current.CurrentProjection);
                                                        current = ((CurrentProjection_1 = ((bind$0040_1 = current.CurrentProjection, new InspectionProjectionTransport(batchEnd, bind$0040_1.BoardMinimumColumn, bind$0040_1.BoardMinimumRow, bind$0040_1.BoardMaximumColumn, bind$0040_1.BoardMaximumRow, bind$0040_1.Units, bind$0040_1.Edges, bind$0040_1.Events, bind$0040_1.Checkpoints, bind$0040_1.PerspectiveHash))), new SimulatorSessionState(current.Session, current.MapRevision, current.PlanRevision, current.InitialProjection, CurrentProjection_1, current.MaximumHorizonTicks, current.CommittedPlan, current.AuthoritativeRun)));
                                                        simulatorSession = current;
                                                        if (batchEnd === targetTick) {
                                                            postSimulator(correlation, new SimulatorResponse(/* SimulatorRunCompleted */ 6, [simulatorUpdate(false, delta_1)]));
                                                            return singleton.Zero();
                                                        }
                                                        else {
                                                            postSimulator(correlation, new SimulatorResponse(/* SimulatorProgress */ 5, [completed, simulatorUpdate(false, delta_1)]));
                                                            return singleton.Zero();
                                                        }
                                                    }
                                                    else {
                                                        stopped = true;
                                                        postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.AUTHORITATIVE_RUN.MISSING_TICK", "No qualified authoritative projection exists for the requested tick."]));
                                                        return singleton.Zero();
                                                    }
                                                }
                                            }) : singleton.Zero();
                                        });
                                    }
                                }
                                else {
                                    postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.PLAN.NOT_COMMITTED", "Commit a valid plan before running."]));
                                    return singleton.Zero();
                                }
                            }
                            case 6:
                                if (!equals(correlation.PlanRevision, session_5.PlanRevision)) {
                                    postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.PLAN.STALE", "Reset does not match the committed plan revision."]));
                                    return singleton.Zero();
                                }
                                else {
                                    const reset = new SimulatorSessionState(session_5.Session, session_5.MapRevision, session_5.PlanRevision, session_5.InitialProjection, session_5.InitialProjection, session_5.MaximumHorizonTicks, session_5.CommittedPlan, session_5.AuthoritativeRun);
                                    simulatorSession = reset;
                                    postSimulator(correlation, new SimulatorResponse(/* SimulatorReset */ 7, [simulatorUpdate(true, reset.InitialProjection)]));
                                    return singleton.Zero();
                                }
                            case 0:
                            case 7: {
                                return singleton.Zero();
                            }
                            default:
                                if (compare(correlation.PlanRevision, session_5.PlanRevision) < 0) {
                                    postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.PLAN.STALE", "The plan revision is stale."]));
                                    return singleton.Zero();
                                }
                                else {
                                    const diagnostics = SimulatorProtocol_diagnostics(session_5.MaximumHorizonTicks, request.fields[0]);
                                    postSimulator(correlation, new SimulatorResponse(/* PlanValidated */ 1, [(diagnostics.length === 0) ? correlation.PlanRevision : undefined, diagnostics]));
                                    return singleton.Zero();
                                }
                        }
                    }
                }
                else {
                    postSimulator(correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.SESSION.MISSING", "Initialize the simulator session before sending operations."]));
                    return singleton.Zero();
                }
        }
    });
}

function execute(operation, request) {
    return singleton.Delay(() => {
        let set$;
        switch (request.tag) {
            case 1: {
                const currentTick = request.fields[0] | 0;
                const target = min(request.fields[2], currentTick + request.fields[1]) | 0;
                let tick = currentTick;
                let completedBatches = 0;
                return singleton.Combine(singleton.For(WorkerProtocol_batchEnds(currentTick, target), (_arg) => {
                    let package$_2;
                    if (!isCancelled(operation)) {
                        tick = (_arg | 0);
                        completedBatches = ((completedBatches + 1) | 0);
                        return singleton.Combine((loadedPackage != null) ? ((tick < target) ? ((package$_2 = loadedPackage, (post(operation, new RunnerResponse(/* RunnerProgress */ 1, [tick, completedBatches, WorkerTransport_inspectionToTransport(projectionAt(tick, package$_2))])), singleton.Zero()))) : (singleton.Zero())) : (singleton.Zero()), singleton.Delay(() => singleton.Bind(sleep(0), () => singleton.Return(undefined))));
                    }
                    else {
                        return singleton.Zero();
                    }
                }), singleton.Delay(() => {
                    if (!isCancelled(operation)) {
                        if (loadedPackage == null) {
                            post(operation, new RunnerResponse(/* RunnerFailed */ 8, ["no replay is loaded in the worker"]));
                            return singleton.Zero();
                        }
                        else {
                            const package$_3 = loadedPackage;
                            const projection = projectionAt(tick, package$_3);
                            post(operation, new RunnerResponse(/* Progressed */ 2, [projection.Tick, WorkerTransport_inspectionToTransport(projection)]));
                            return singleton.Zero();
                        }
                    }
                    else {
                        return singleton.Zero();
                    }
                }));
            }
            case 2:
                if (loadedPackage == null) {
                    post(operation, new RunnerResponse(/* RunnerFailed */ 8, ["no replay is loaded in the worker"]));
                    return singleton.Zero();
                }
                else {
                    const package$_4 = loadedPackage;
                    const projection_1 = projectionAt(max(0, min(request.fields[1], request.fields[0])), package$_4);
                    post(operation, new RunnerResponse(/* RunnerProgress */ 1, [projection_1.Tick, 1, WorkerTransport_inspectionToTransport(projection_1)]));
                    return singleton.Bind(sleep(0), () => {
                        if (!isCancelled(operation)) {
                            post(operation, new RunnerResponse(/* Progressed */ 2, [projection_1.Tick, WorkerTransport_inspectionToTransport(projection_1)]));
                            return singleton.Zero();
                        }
                        else {
                            return singleton.Zero();
                        }
                    });
                }
            case 3: {
                post(operation, new RunnerResponse(/* Forked */ 3, [request.fields[0]]));
                return singleton.Zero();
            }
            case 4: {
                const scenarioIdentity = request.fields[0];
                const matchValue_2 = Lab_tryScenario(scenarioIdentity);
                if (matchValue_2 != null) {
                    const scenario = matchValue_2;
                    const matchValue_3 = Lab_run(scenario, empty_2({
                        Compare: (x, y) => (comparePrimitives(x, y) | 0),
                    }), undefined);
                    if (matchValue_3.tag === 0) {
                        const report = matchValue_3.fields[0];
                        post(operation, new RunnerResponse(/* LoadedScenario */ 4, [WorkerTransport_metadataToTransport(scenarioMetadata(scenario, report)), Lab_scenarioToTransport(scenario), Lab_reportToTransport(report), WorkerTransport_inspectionToTransport(emptyProjection(0))]));
                        return singleton.Zero();
                    }
                    else {
                        post(operation, new RunnerResponse(/* RunnerFailed */ 8, [matchValue_3.fields[0]]));
                        return singleton.Zero();
                    }
                }
                else {
                    post(operation, new RunnerResponse(/* RunnerFailed */ 8, ["unknown design scenario: " + scenarioIdentity]));
                    return singleton.Zero();
                }
            }
            case 5: {
                const scenarioIdentity_1 = request.fields[0];
                const matchValue_4 = Lab_tryScenario(scenarioIdentity_1);
                if (matchValue_4 != null) {
                    const matchValue_5 = Lab_run(matchValue_4, Lab_parametersFromTransport(request.fields[1]), request.fields[2]);
                    if (matchValue_5.tag === 0) {
                        const report_1 = matchValue_5.fields[0];
                        post(operation, new RunnerResponse(/* ExperimentCompleted */ 5, [report_1.Comparison.Fork.ResultIdentity, Lab_reportToTransport(report_1)]));
                        return singleton.Zero();
                    }
                    else {
                        post(operation, new RunnerResponse(/* RunnerFailed */ 8, [matchValue_5.fields[0]]));
                        return singleton.Zero();
                    }
                }
                else {
                    post(operation, new RunnerResponse(/* RunnerFailed */ 8, ["unknown design scenario: " + scenarioIdentity_1]));
                    return singleton.Zero();
                }
            }
            case 6: {
                cancelled = ((set$ = cancelled, add(OperationIdModule_value(operation), set$)));
                return singleton.Zero();
            }
            default: {
                const sourceName = request.fields[0];
                const matchValue = Replay_decode(Replay_defaultLimits, request.fields[1]);
                if (matchValue.tag === 0) {
                    const package$ = matchValue.fields[0];
                    const matchValue_1 = Replay_runKernelReplay(Replay_defaultLimits, supportedEngine, package$);
                    if (matchValue_1.tag === 1) {
                        post(operation, replayError(matchValue_1.fields[0]));
                        return singleton.Zero();
                    }
                    else {
                        switch (matchValue_1.fields[0].tag) {
                            case 2: {
                                loadedPackage = package$;
                                post(operation, new RunnerResponse(/* LoadedPackage */ 0, [WorkerTransport_metadataToTransport(metadata(sourceName, package$)), RunnerClaim.ProjectionOnly, WorkerTransport_inspectionToTransport(projectionAt(0, package$))]));
                                return singleton.Zero();
                            }
                            case 1: {
                                post(operation, new RunnerResponse(/* RunnerFailed */ 8, ["browser runner made an authoritative verification claim"]));
                                return singleton.Zero();
                            }
                            default: {
                                loadedPackage = package$;
                                post(operation, new RunnerResponse(/* LoadedPackage */ 0, [WorkerTransport_metadataToTransport(metadata(sourceName, package$)), RunnerClaim.KernelVerified, WorkerTransport_inspectionToTransport(projectionAt(0, package$))]));
                                return singleton.Zero();
                            }
                        }
                    }
                }
                else {
                    post(operation, replayError(matchValue.fields[0]));
                    return singleton.Zero();
                }
            }
        }
    });
}

function receive(event) {
    if (event.data.Kind === "sir-simulator-session") {
        const envelope = event.data;
        if (envelope.ProtocolVersion !== 1) {
            postSimulator(envelope.Correlation, new SimulatorResponse(/* SimulatorRequestRejected */ 9, ["SIR.SIMULATOR.PROTOCOL.VERSION", "The simulator worker protocol version is not supported."]));
        }
        else {
            startImmediate(executeSimulator(envelope.Correlation, envelope.Request));
        }
    }
    else {
        const envelope_1 = event.data;
        const operation = OperationIdModule_create(envelope_1.Operation);
        if (envelope_1.ProtocolVersion !== 3) {
            post(operation, new RunnerResponse(/* RunnerFailed */ 8, [("worker protocol " + int32ToString(envelope_1.ProtocolVersion)) + " is not supported"]));
        }
        else {
            startImmediate(execute(operation, envelope_1.Request));
        }
    }
}

scope.onmessage = ((event) => {
    receive(event);
});

