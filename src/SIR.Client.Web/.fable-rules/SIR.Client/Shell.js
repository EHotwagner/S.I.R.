
import { toString, Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { uint8_type, class_type, array_type, list_type, option_type, bool_type, record_type, string_type, union_type, int32_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { Lab_validate, Lab_parametersToTransport, Lab_reportFromTransport, Lab_scenarioFromTransport, LabReportTransport_$reflection, DesignScenarioTransport_$reflection, Int32Entry_$reflection, LabReport_$reflection, DesignScenario_$reflection } from "./Lab.js";
import { exists, singleton, enumerateWhile, delay, toList } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { max, min } from "../fable_modules/fable-library-js.5.13.0/Double.js";
import { compareArrays, equals, comparePrimitives, int32ToString, Exception } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { tryFind, filter, tryLast, sortBy, empty as empty_1, singleton as singleton_1, append, map, ofArray, toArray } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { RenderFrame, RenderEventVisual, EdgeVisual, UnitVisual, SecondaryHeadingVisual, SecondaryHeadingSource, Disclosure$1, FactionVisual, UnitClassIdModule_placeholder, BoardVisual, DisclosureLabel, HeadingRadiansModule_ofDirection8, HealthVisualModule_tryCreate, CellExtentModule_tryCreate } from "./ReplayPresentation.js";
import { Direction8Module_tryFromCode } from "../SIR.Domain/Orientation.js";
import { toArray as toArray_1, defaultArg } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { add, toList as toList_1, empty } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { contains, ofList } from "../fable_modules/fable-library-js.5.13.0/Set.js";
import { join } from "../fable_modules/fable-library-js.5.13.0/String.js";

export class OperationId extends Union {
    constructor(Item) {
        super();
        this.tag = 0;
        this.fields = [Item];
    }
    cases() {
        return ["OperationId"];
    }
}

export function OperationId_$reflection() {
    return union_type("SIR.Client.OperationId", [], OperationId, () => [[["Item", int32_type]]]);
}

export function OperationIdModule_create(value) {
    return new OperationId(value);
}

export function OperationIdModule_value(_arg) {
    return _arg.fields[0] | 0;
}

export class ReplayKind extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["FullReplay", "PerspectiveReplay", "DesignScenario"];
    }
    static FullReplay = new ReplayKind(0, []);
    static PerspectiveReplay = new ReplayKind(1, []);
    static DesignScenario = new ReplayKind(2, []);
}

export function ReplayKind_$reflection() {
    return union_type("SIR.Client.ReplayKind", [], ReplayKind, () => [[], [], []]);
}

export class ReplayMetadata extends Record {
    constructor(SourceName, SourceIdentity, EngineIdentity, FinalTick, Kind) {
        super();
        this.SourceName = SourceName;
        this.SourceIdentity = SourceIdentity;
        this.EngineIdentity = EngineIdentity;
        this.FinalTick = (FinalTick | 0);
        this.Kind = Kind;
    }
}

export function ReplayMetadata_$reflection() {
    return record_type("SIR.Client.ReplayMetadata", [], ReplayMetadata, () => [["SourceName", string_type], ["SourceIdentity", string_type], ["EngineIdentity", string_type], ["FinalTick", int32_type], ["Kind", ReplayKind_$reflection()]]);
}

export class ReplayMetadataTransport extends Record {
    constructor(SourceName, SourceIdentity, EngineIdentity, FinalTick, Kind) {
        super();
        this.SourceName = SourceName;
        this.SourceIdentity = SourceIdentity;
        this.EngineIdentity = EngineIdentity;
        this.FinalTick = (FinalTick | 0);
        this.Kind = (Kind | 0);
    }
}

export function ReplayMetadataTransport_$reflection() {
    return record_type("SIR.Client.ReplayMetadataTransport", [], ReplayMetadataTransport, () => [["SourceName", string_type], ["SourceIdentity", string_type], ["EngineIdentity", string_type], ["FinalTick", int32_type], ["Kind", int32_type]]);
}

export class PlaybackSpeed extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Half", "Normal", "Double", "Maximum"];
    }
    static Half = new PlaybackSpeed(0, []);
    static Normal = new PlaybackSpeed(1, []);
    static Double = new PlaybackSpeed(2, []);
    static Maximum = new PlaybackSpeed(3, []);
}

export function PlaybackSpeed_$reflection() {
    return union_type("SIR.Client.PlaybackSpeed", [], PlaybackSpeed, () => [[], [], [], []]);
}

export class PlaybackState extends Record {
    constructor(CurrentTick, FinalTick, IsPlaying, Speed) {
        super();
        this.CurrentTick = (CurrentTick | 0);
        this.FinalTick = (FinalTick | 0);
        this.IsPlaying = IsPlaying;
        this.Speed = Speed;
    }
}

export function PlaybackState_$reflection() {
    return record_type("SIR.Client.PlaybackState", [], PlaybackState, () => [["CurrentTick", int32_type], ["FinalTick", int32_type], ["IsPlaying", bool_type], ["Speed", PlaybackSpeed_$reflection()]]);
}

export class RunMode extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["NoRun", "VerifiedReplay", "PerspectivePlayback", "SandboxFork", "ScenarioSandbox"];
    }
    static NoRun = new RunMode(0, []);
    static VerifiedReplay = new RunMode(1, []);
    static PerspectivePlayback = new RunMode(2, []);
}

export function RunMode_$reflection() {
    return union_type("SIR.Client.RunMode", [], RunMode, () => [[], [], [], [["derivedIdentity", string_type]], [["scenarioIdentity", string_type]]]);
}

export class Verification extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["NotLoaded", "Loading", "BrowserKernelVerified", "PerspectiveReady", "SandboxDerived", "Unsupported", "Diverged", "Failed"];
    }
    static NotLoaded = new Verification(0, []);
    static Loading = new Verification(1, []);
    static BrowserKernelVerified = new Verification(2, []);
    static PerspectiveReady = new Verification(3, []);
}

export function Verification_$reflection() {
    return union_type("SIR.Client.Verification", [], Verification, () => [[], [], [], [], [["derivedIdentity", string_type]], [["reason", string_type]], [["tick", int32_type], ["phase", string_type], ["detail", string_type]], [["reason", string_type]]]);
}

export class SourceState extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["NoSource", "Reading", "Loaded", "Rejected"];
    }
    static NoSource = new SourceState(0, []);
}

export function SourceState_$reflection() {
    return union_type("SIR.Client.SourceState", [], SourceState, () => [[], [["sourceName", string_type]], [["Item", ReplayMetadata_$reflection()]], [["sourceName", string_type], ["reason", string_type]]]);
}

export class SelectionState extends Record {
    constructor(Unit, Event$, Formula) {
        super();
        this.Unit = Unit;
        this.Event = Event$;
        this.Formula = Formula;
    }
}

export function SelectionState_$reflection() {
    return record_type("SIR.Client.SelectionState", [], SelectionState, () => [["Unit", option_type(int32_type)], ["Event", option_type(int32_type)], ["Formula", option_type(string_type)]]);
}

export class UnitProjection extends Record {
    constructor(Id, Side, Column, Row, Health, HealthMaximum, MovementDirection, BodyFacing, AttentionDirection) {
        super();
        this.Id = (Id | 0);
        this.Side = Side;
        this.Column = (Column | 0);
        this.Row = (Row | 0);
        this.Health = (Health | 0);
        this.HealthMaximum = (HealthMaximum | 0);
        this.MovementDirection = MovementDirection;
        this.BodyFacing = (BodyFacing | 0);
        this.AttentionDirection = (AttentionDirection | 0);
    }
}

export function UnitProjection_$reflection() {
    return record_type("SIR.Client.UnitProjection", [], UnitProjection, () => [["Id", int32_type], ["Side", string_type], ["Column", int32_type], ["Row", int32_type], ["Health", int32_type], ["HealthMaximum", int32_type], ["MovementDirection", option_type(int32_type)], ["BodyFacing", int32_type], ["AttentionDirection", int32_type]]);
}

export class EventProjection extends Record {
    constructor(Id, Tick, Source, Summary, SourceUnitId, TargetUnitId) {
        super();
        this.Id = (Id | 0);
        this.Tick = (Tick | 0);
        this.Source = Source;
        this.Summary = Summary;
        this.SourceUnitId = SourceUnitId;
        this.TargetUnitId = TargetUnitId;
    }
}

export function EventProjection_$reflection() {
    return record_type("SIR.Client.EventProjection", [], EventProjection, () => [["Id", int32_type], ["Tick", int32_type], ["Source", string_type], ["Summary", string_type], ["SourceUnitId", option_type(int32_type)], ["TargetUnitId", option_type(int32_type)]]);
}

export class EdgeProjection extends Record {
    constructor(Id, Kind, State, StartColumn, StartRow, EndColumn, EndRow) {
        super();
        this.Id = Id;
        this.Kind = Kind;
        this.State = State;
        this.StartColumn = (StartColumn | 0);
        this.StartRow = (StartRow | 0);
        this.EndColumn = (EndColumn | 0);
        this.EndRow = (EndRow | 0);
    }
}

export function EdgeProjection_$reflection() {
    return record_type("SIR.Client.EdgeProjection", [], EdgeProjection, () => [["Id", string_type], ["Kind", string_type], ["State", string_type], ["StartColumn", int32_type], ["StartRow", int32_type], ["EndColumn", int32_type], ["EndRow", int32_type]]);
}

export class CheckpointProjection extends Record {
    constructor(Tick, StateHash, EventHash) {
        super();
        this.Tick = (Tick | 0);
        this.StateHash = StateHash;
        this.EventHash = EventHash;
    }
}

export function CheckpointProjection_$reflection() {
    return record_type("SIR.Client.CheckpointProjection", [], CheckpointProjection, () => [["Tick", int32_type], ["StateHash", string_type], ["EventHash", string_type]]);
}

/**
 * Bounded presentation data; the complete replay and world remain in the worker.
 */
export class InspectionProjection extends Record {
    constructor(Tick, BoardMinimumColumn, BoardMinimumRow, BoardMaximumColumn, BoardMaximumRow, Units, Edges, Events, Checkpoints, PerspectiveHash) {
        super();
        this.Tick = (Tick | 0);
        this.BoardMinimumColumn = (BoardMinimumColumn | 0);
        this.BoardMinimumRow = (BoardMinimumRow | 0);
        this.BoardMaximumColumn = (BoardMaximumColumn | 0);
        this.BoardMaximumRow = (BoardMaximumRow | 0);
        this.Units = Units;
        this.Edges = Edges;
        this.Events = Events;
        this.Checkpoints = Checkpoints;
        this.PerspectiveHash = PerspectiveHash;
    }
}

export function InspectionProjection_$reflection() {
    return record_type("SIR.Client.InspectionProjection", [], InspectionProjection, () => [["Tick", int32_type], ["BoardMinimumColumn", int32_type], ["BoardMinimumRow", int32_type], ["BoardMaximumColumn", int32_type], ["BoardMaximumRow", int32_type], ["Units", list_type(UnitProjection_$reflection())], ["Edges", list_type(EdgeProjection_$reflection())], ["Events", list_type(EventProjection_$reflection())], ["Checkpoints", list_type(CheckpointProjection_$reflection())], ["PerspectiveHash", option_type(string_type)]]);
}

export class InspectionProjectionTransport extends Record {
    constructor(Tick, BoardMinimumColumn, BoardMinimumRow, BoardMaximumColumn, BoardMaximumRow, Units, Edges, Events, Checkpoints, PerspectiveHash) {
        super();
        this.Tick = (Tick | 0);
        this.BoardMinimumColumn = (BoardMinimumColumn | 0);
        this.BoardMinimumRow = (BoardMinimumRow | 0);
        this.BoardMaximumColumn = (BoardMaximumColumn | 0);
        this.BoardMaximumRow = (BoardMaximumRow | 0);
        this.Units = Units;
        this.Edges = Edges;
        this.Events = Events;
        this.Checkpoints = Checkpoints;
        this.PerspectiveHash = PerspectiveHash;
    }
}

export function InspectionProjectionTransport_$reflection() {
    return record_type("SIR.Client.InspectionProjectionTransport", [], InspectionProjectionTransport, () => [["Tick", int32_type], ["BoardMinimumColumn", int32_type], ["BoardMinimumRow", int32_type], ["BoardMaximumColumn", int32_type], ["BoardMaximumRow", int32_type], ["Units", array_type(UnitProjection_$reflection())], ["Edges", array_type(EdgeProjection_$reflection())], ["Events", array_type(EventProjection_$reflection())], ["Checkpoints", array_type(CheckpointProjection_$reflection())], ["PerspectiveHash", option_type(string_type)]]);
}

export class WorkerState extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["WorkerStarting", "WorkerReady", "WorkerBusy", "WorkerStopped"];
    }
    static WorkerStarting = new WorkerState(0, []);
    static WorkerReady = new WorkerState(1, []);
}

export function WorkerState_$reflection() {
    return union_type("SIR.Client.WorkerState", [], WorkerState, () => [[], [], [["completedBatches", int32_type]], [["reason", string_type]]]);
}

export class LabState extends Record {
    constructor(Scenario, Report, ValidationError) {
        super();
        this.Scenario = Scenario;
        this.Report = Report;
        this.ValidationError = ValidationError;
    }
}

export function LabState_$reflection() {
    return record_type("SIR.Client.LabState", [], LabState, () => [["Scenario", option_type(DesignScenario_$reflection())], ["Report", option_type(LabReport_$reflection())], ["ValidationError", option_type(string_type)]]);
}

export class Model extends Record {
    constructor(Source, Mode, Verification, Playback, Selection$, Inspection, Patch, Lab, Worker$, ActiveOperation, NextOperation, Announcement) {
        super();
        this.Source = Source;
        this.Mode = Mode;
        this.Verification = Verification;
        this.Playback = Playback;
        this.Selection = Selection$;
        this.Inspection = Inspection;
        this.Patch = Patch;
        this.Lab = Lab;
        this.Worker = Worker$;
        this.ActiveOperation = ActiveOperation;
        this.NextOperation = (NextOperation | 0);
        this.Announcement = Announcement;
    }
}

export function Model_$reflection() {
    return record_type("SIR.Client.Model", [], Model, () => [["Source", SourceState_$reflection()], ["Mode", RunMode_$reflection()], ["Verification", Verification_$reflection()], ["Playback", PlaybackState_$reflection()], ["Selection", SelectionState_$reflection()], ["Inspection", option_type(InspectionProjection_$reflection())], ["Patch", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, int32_type])], ["Lab", LabState_$reflection()], ["Worker", WorkerState_$reflection()], ["ActiveOperation", option_type(OperationId_$reflection())], ["NextOperation", int32_type], ["Announcement", string_type]]);
}

export class RunnerRequest extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["LoadPackage", "Advance", "Seek", "Fork", "LoadScenario", "RunExperiment", "Cancel"];
    }
    static Cancel = new RunnerRequest(6, []);
}

export function RunnerRequest_$reflection() {
    return union_type("SIR.Client.RunnerRequest", [], RunnerRequest, () => [[["sourceName", string_type], ["bytes", array_type(uint8_type)]], [["currentTick", int32_type], ["tickCount", int32_type], ["finalTick", int32_type]], [["targetTick", int32_type], ["finalTick", int32_type]], [["derivedIdentity", string_type], ["patch", array_type(Int32Entry_$reflection())]], [["scenarioIdentity", string_type]], [["scenarioIdentity", string_type], ["patch", array_type(Int32Entry_$reflection())], ["sweepParameter", option_type(string_type)]], []]);
}

export class RunnerClaim extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["KernelVerified", "ProjectionOnly", "ScenarioReady"];
    }
    static KernelVerified = new RunnerClaim(0, []);
    static ProjectionOnly = new RunnerClaim(1, []);
    static ScenarioReady = new RunnerClaim(2, []);
}

export function RunnerClaim_$reflection() {
    return union_type("SIR.Client.RunnerClaim", [], RunnerClaim, () => [[], [], []]);
}

export class RunnerResponse extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["LoadedPackage", "RunnerProgress", "Progressed", "Forked", "LoadedScenario", "ExperimentCompleted", "RunnerUnsupported", "RunnerDiverged", "RunnerFailed"];
    }
}

export function RunnerResponse_$reflection() {
    return union_type("SIR.Client.RunnerResponse", [], RunnerResponse, () => [[["Item1", ReplayMetadataTransport_$reflection()], ["Item2", RunnerClaim_$reflection()], ["Item3", InspectionProjectionTransport_$reflection()]], [["tick", int32_type], ["completedBatches", int32_type], ["Item3", InspectionProjectionTransport_$reflection()]], [["tick", int32_type], ["Item2", InspectionProjectionTransport_$reflection()]], [["derivedIdentity", string_type]], [["Item1", ReplayMetadataTransport_$reflection()], ["Item2", DesignScenarioTransport_$reflection()], ["Item3", LabReportTransport_$reflection()], ["Item4", InspectionProjectionTransport_$reflection()]], [["derivedIdentity", string_type], ["Item2", LabReportTransport_$reflection()]], [["reason", string_type]], [["tick", int32_type], ["phase", string_type], ["detail", string_type]], [["reason", string_type]]]);
}

export class Effect extends Union {
    constructor(Item1, Item2) {
        super();
        this.tag = 0;
        this.fields = [Item1, Item2];
    }
    cases() {
        return ["Run"];
    }
}

export function Effect_$reflection() {
    return union_type("SIR.Client.Effect", [], Effect, () => [[["Item1", OperationId_$reflection()], ["Item2", RunnerRequest_$reflection()]]]);
}

export class Msg extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["ReplayBytesSelected", "RunnerResponded", "TogglePlayback", "StepBackward", "StepForward", "PreviousEvent", "NextEvent", "SeekRequested", "SpeedChanged", "UnitSelected", "EventSelected", "FormulaSelected", "ParameterEdited", "ScenarioSelected", "SweepRequested", "CancelRequested", "WorkerStarted", "WorkerTerminated"];
    }
    static TogglePlayback = new Msg(2, []);
    static StepBackward = new Msg(3, []);
    static StepForward = new Msg(4, []);
    static PreviousEvent = new Msg(5, []);
    static NextEvent = new Msg(6, []);
    static CancelRequested = new Msg(15, []);
    static WorkerStarted = new Msg(16, []);
}

export function Msg_$reflection() {
    return union_type("SIR.Client.Msg", [], Msg, () => [[["sourceName", string_type], ["bytes", array_type(uint8_type)]], [["Item1", OperationId_$reflection()], ["Item2", RunnerResponse_$reflection()]], [], [], [], [], [], [["Item", int32_type]], [["Item", PlaybackSpeed_$reflection()]], [["Item", option_type(int32_type)]], [["Item", option_type(int32_type)]], [["Item", option_type(string_type)]], [["name", string_type], ["value", int32_type]], [["scenarioIdentity", string_type]], [["parameter", string_type]], [], [], [["reason", string_type]]]);
}

export class WorkerRequestEnvelope extends Record {
    constructor(ProtocolVersion, Operation, Request) {
        super();
        this.ProtocolVersion = (ProtocolVersion | 0);
        this.Operation = (Operation | 0);
        this.Request = Request;
    }
}

export function WorkerRequestEnvelope_$reflection() {
    return record_type("SIR.Client.WorkerRequestEnvelope", [], WorkerRequestEnvelope, () => [["ProtocolVersion", int32_type], ["Operation", int32_type], ["Request", RunnerRequest_$reflection()]]);
}

export class WorkerResponseEnvelope extends Record {
    constructor(ProtocolVersion, Operation, Response) {
        super();
        this.ProtocolVersion = (ProtocolVersion | 0);
        this.Operation = (Operation | 0);
        this.Response = Response;
    }
}

export function WorkerResponseEnvelope_$reflection() {
    return record_type("SIR.Client.WorkerResponseEnvelope", [], WorkerResponseEnvelope, () => [["ProtocolVersion", int32_type], ["Operation", int32_type], ["Response", RunnerResponse_$reflection()]]);
}

export function WorkerProtocol_batchEnds(startTick, targetTick) {
    return toList(delay(() => {
        let tick = startTick;
        return enumerateWhile(() => (tick < targetTick), delay(() => {
            tick = (min(targetTick, tick + 256) | 0);
            return singleton(tick);
        }));
    }));
}

export function WorkerTransport_metadataToTransport(metadata) {
    let matchValue;
    return new ReplayMetadataTransport(metadata.SourceName, metadata.SourceIdentity, metadata.EngineIdentity, metadata.FinalTick, (matchValue = metadata.Kind, (matchValue.tag === 1) ? 1 : ((matchValue.tag === 2) ? 2 : 0)));
}

export function WorkerTransport_metadataFromTransport(metadata) {
    let matchValue;
    return new ReplayMetadata(metadata.SourceName, metadata.SourceIdentity, metadata.EngineIdentity, metadata.FinalTick, (matchValue = (metadata.Kind | 0), (matchValue === 0) ? ReplayKind.FullReplay : ((matchValue === 1) ? ReplayKind.PerspectiveReplay : ((matchValue === 2) ? ReplayKind.DesignScenario : (() => {
        throw new Exception("Unknown replay kind from worker transport: " + int32ToString(matchValue));
    })()))));
}

export function WorkerTransport_inspectionToTransport(inspection) {
    return new InspectionProjectionTransport(inspection.Tick, inspection.BoardMinimumColumn, inspection.BoardMinimumRow, inspection.BoardMaximumColumn, inspection.BoardMaximumRow, toArray(inspection.Units), toArray(inspection.Edges), toArray(inspection.Events), toArray(inspection.Checkpoints), inspection.PerspectiveHash);
}

export function WorkerTransport_inspectionFromTransport(inspection) {
    return new InspectionProjection(inspection.Tick, inspection.BoardMinimumColumn, inspection.BoardMinimumRow, inspection.BoardMaximumColumn, inspection.BoardMaximumRow, ofArray(inspection.Units), ofArray(inspection.Edges), ofArray(inspection.Events), ofArray(inspection.Checkpoints), inspection.PerspectiveHash);
}

const Shell_visualExtent = (() => {
    const option_1 = CellExtentModule_tryCreate(1);
    if (option_1 != null) {
        return option_1;
    }
    else {
        throw new Exception("One-cell visual extent is invalid.");
    }
})();

function Shell_projectedHealth(remaining, maximum) {
    const option_1 = HealthVisualModule_tryCreate(remaining, maximum);
    if (option_1 != null) {
        return option_1;
    }
    else {
        throw new Exception("Projected health bounds are invalid.");
    }
}

function Shell_projectedDirection(field, code) {
    const matchValue = Direction8Module_tryFromCode(code & 0xFF);
    if (matchValue == null) {
        throw new Exception("Projection direction is outside 0..7." + ((" (Parameter \'" + field) + "\')"));
    }
    else {
        return HeadingRadiansModule_ofDirection8(matchValue);
    }
}

/**
 * Adapts only the worker's currently disclosed bounded projection.
 * Missing class, footprint-detail, heading, elevation, and stance facts stay absent.
 */
export function Shell_renderFrame(model) {
    const matchValue = model.Mode;
    const matchValue_1 = model.Inspection;
    let matchResult, inspection;
    switch (matchValue.tag) {
        case 1: {
            if (matchValue_1 != null) {
                matchResult = 0;
                inspection = matchValue_1;
            }
            else {
                matchResult = 1;
            }
            break;
        }
        case 2: {
            if (matchValue_1 != null) {
                matchResult = 0;
                inspection = matchValue_1;
            }
            else {
                matchResult = 1;
            }
            break;
        }
        case 3: {
            if (matchValue_1 != null) {
                matchResult = 0;
                inspection = matchValue_1;
            }
            else {
                matchResult = 1;
            }
            break;
        }
        case 4: {
            if (matchValue_1 != null) {
                matchResult = 0;
                inspection = matchValue_1;
            }
            else {
                matchResult = 1;
            }
            break;
        }
        default:
            matchResult = 1;
    }
    switch (matchResult) {
        case 0: {
            let disclosure;
            const matchValue_3 = model.Mode;
            switch (matchValue_3.tag) {
                case 1: {
                    disclosure = DisclosureLabel.FullReplayDisclosure;
                    break;
                }
                case 2: {
                    disclosure = DisclosureLabel.PerspectiveDisclosure;
                    break;
                }
                case 3:
                case 4: {
                    disclosure = DisclosureLabel.SandboxDisclosure;
                    break;
                }
                default:
                    throw new Exception("Unreachable replay disclosure.");
            }
            return new RenderFrame(inspection.Tick, new BoardVisual(inspection.BoardMinimumColumn, inspection.BoardMinimumRow, inspection.BoardMaximumColumn, inspection.BoardMaximumRow), toArray(map((unit) => {
                let matchValue_4;
                return new UnitVisual(unit.Id, unit.Column, unit.Row, Shell_visualExtent, Shell_visualExtent, UnitClassIdModule_placeholder, (matchValue_4 = unit.Side, (matchValue_4 === "Blue") ? FactionVisual.Human : ((matchValue_4 === "Red") ? FactionVisual.Arcane : (new FactionVisual(/* OtherFaction */ 3, [matchValue_4])))), new Disclosure$1(/* Disclosed */ 3, [Shell_projectedHealth(unit.Health, unit.HealthMaximum)]), Disclosure$1.NotPresent, Disclosure$1.NotPresent, new Disclosure$1(/* Disclosed */ 3, [Shell_projectedDirection("BodyFacing", unit.BodyFacing)]), new Disclosure$1(/* Disclosed */ 3, [new SecondaryHeadingVisual(Shell_projectedDirection("AttentionDirection", unit.AttentionDirection), SecondaryHeadingSource.AttentionHeading)]), new Disclosure$1(/* Disclosed */ 3, [int32ToString(unit.Id)]), []);
            }, inspection.Units)), toArray(map((edge) => (new EdgeVisual(edge.Id, edge.Kind, edge.State, edge.StartColumn, edge.StartRow, edge.EndColumn, edge.EndRow)), inspection.Edges)), [], toArray(map((event) => {
                let option_1, option_4;
                return new RenderEventVisual(event.Id, event.Tick, event.Source, defaultArg((option_1 = event.SourceUnitId, (option_1 != null) ? (new Disclosure$1(/* Disclosed */ 3, [option_1])) : undefined), Disclosure$1.NotPresent), defaultArg((option_4 = event.TargetUnitId, (option_4 != null) ? (new Disclosure$1(/* Disclosed */ 3, [option_4])) : undefined), Disclosure$1.NotPresent), new Disclosure$1(/* Disclosed */ 3, [event.Summary]));
            }, inspection.Events)), disclosure);
        }
        default:
            return undefined;
    }
}

export function Shell_init() {
    return new Model(SourceState.NoSource, RunMode.NoRun, Verification.NotLoaded, new PlaybackState(0, 0, false, PlaybackSpeed.Normal), new SelectionState(undefined, undefined, undefined), undefined, empty({
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }), new LabState(undefined, undefined, undefined), WorkerState.WorkerStarting, undefined, 1, "Choose a design scenario to run, or load a replay package.");
}

function Shell_beginOperation(request, model) {
    let option_1;
    const operation = new OperationId(model.NextOperation);
    return [new Model(model.Source, model.Mode, model.Verification, model.Playback, model.Selection, model.Inspection, model.Patch, model.Lab, model.Worker, operation, model.NextOperation + 1, model.Announcement), append(ofArray(toArray_1((option_1 = model.ActiveOperation, (option_1 != null) ? (new Effect(option_1, RunnerRequest.Cancel)) : undefined))), singleton_1(new Effect(operation, request)))];
}

function Shell_stopOperation(model) {
    return new Model(model.Source, model.Mode, model.Verification, model.Playback, model.Selection, model.Inspection, model.Patch, model.Lab, model.Worker, undefined, model.NextOperation, model.Announcement);
}

function Shell_rejectSource(reason, source) {
    switch (source.tag) {
        case 0:
        case 2:
        case 3:
            return source;
        default:
            return new SourceState(/* Rejected */ 3, [source.fields[0], reason]);
    }
}

function Shell_clampTick(finalTick, tick) {
    return max(0, min(finalTick, tick)) | 0;
}

function Shell_reconcileSelection(inspection, selection) {
    let option_1, option_3;
    const disclosedUnits = ofList(map((_arg) => (_arg.Id | 0), inspection.Units), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    const disclosedEvents = ofList(map((_arg_1) => (_arg_1.Id | 0), inspection.Events), {
        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
    });
    return new SelectionState((option_1 = selection.Unit, (option_1 != null) ? (contains(option_1, disclosedUnits) ? option_1 : undefined) : undefined), (option_3 = selection.Event, (option_3 != null) ? (contains(option_3, disclosedEvents) ? option_3 : undefined) : undefined), selection.Formula);
}

function Shell_advanceSize(speed) {
    switch (speed.tag) {
        case 2:
            return 2;
        case 3:
            return 2048;
        default:
            return 1;
    }
}

function Shell_sourceIdentity(model) {
    const matchValue = model.Source;
    if (matchValue.tag === 2) {
        return matchValue.fields[0].SourceIdentity;
    }
    else {
        return "unloaded";
    }
}

function Shell_derivedIdentity(model, patch) {
    const suffix = join(";", map((tupledArg) => ((tupledArg[0] + "=") + toString(tupledArg[1])), toList_1(patch)));
    return (Shell_sourceIdentity(model) + ":fork:") + suffix;
}

function Shell_applyRunnerResponse(response, model) {
    let bind$0040_1, bind$0040_2, bind$0040_5, bind$0040_6, bind$0040_7;
    switch (response.tag) {
        case 1: {
            const completedBatches = response.fields[1] | 0;
            const inspection_3 = WorkerTransport_inspectionFromTransport(response.fields[2]);
            const tick_1 = Shell_clampTick(model.Playback.FinalTick, inspection_3.Tick) | 0;
            return new Model(model.Source, model.Mode, model.Verification, (bind$0040_1 = model.Playback, new PlaybackState(tick_1, bind$0040_1.FinalTick, bind$0040_1.IsPlaying, bind$0040_1.Speed)), Shell_reconcileSelection(inspection_3, model.Selection), inspection_3, model.Patch, model.Lab, new WorkerState(/* WorkerBusy */ 2, [completedBatches]), model.ActiveOperation, model.NextOperation, ((("Worker completed batch " + int32ToString(completedBatches)) + " at tick ") + int32ToString(tick_1)) + ".");
        }
        case 2: {
            const inspection_5 = WorkerTransport_inspectionFromTransport(response.fields[1]);
            const tick_3 = Shell_clampTick(model.Playback.FinalTick, inspection_5.Tick) | 0;
            return Shell_stopOperation(new Model(model.Source, model.Mode, model.Verification, (bind$0040_2 = model.Playback, new PlaybackState(tick_3, bind$0040_2.FinalTick, model.Playback.IsPlaying && (tick_3 < model.Playback.FinalTick), bind$0040_2.Speed)), Shell_reconcileSelection(inspection_5, model.Selection), inspection_5, model.Patch, model.Lab, WorkerState.WorkerReady, model.ActiveOperation, model.NextOperation, ("Playback moved to tick " + int32ToString(tick_3)) + "."));
        }
        case 3: {
            const identity = response.fields[0];
            return Shell_stopOperation(new Model(model.Source, new RunMode(/* SandboxFork */ 3, [identity]), new Verification(/* SandboxDerived */ 4, [identity]), model.Playback, model.Selection, model.Inspection, model.Patch, model.Lab, WorkerState.WorkerReady, model.ActiveOperation, model.NextOperation, "Sandbox fork created. Verification no longer applies."));
        }
        case 4: {
            const metadata_3 = WorkerTransport_metadataFromTransport(response.fields[0]);
            const scenario_1 = Lab_scenarioFromTransport(response.fields[1]);
            const report_1 = Lab_reportFromTransport(response.fields[2]);
            return Shell_stopOperation(new Model(new SourceState(/* Loaded */ 2, [metadata_3]), new RunMode(/* ScenarioSandbox */ 4, [metadata_3.SourceIdentity]), new Verification(/* SandboxDerived */ 4, [metadata_3.SourceIdentity]), new PlaybackState(0, metadata_3.FinalTick, false, model.Playback.Speed), model.Selection, WorkerTransport_inspectionFromTransport(response.fields[3]), empty({
                Compare: (x, y) => (comparePrimitives(x, y) | 0),
            }), new LabState(scenario_1, report_1, undefined), WorkerState.WorkerReady, model.ActiveOperation, model.NextOperation, "Design scenario loaded as an editable sandbox."));
        }
        case 5: {
            const identity_1 = response.fields[0];
            return Shell_stopOperation(new Model(model.Source, new RunMode(/* SandboxFork */ 3, [identity_1]), new Verification(/* SandboxDerived */ 4, [identity_1]), model.Playback, model.Selection, model.Inspection, model.Patch, new LabState(model.Lab.Scenario, Lab_reportFromTransport(response.fields[1]), undefined), WorkerState.WorkerReady, model.ActiveOperation, model.NextOperation, "Sandbox comparison completed. Results are exploratory evidence."));
        }
        case 6: {
            const reason = response.fields[0];
            return Shell_stopOperation(new Model(Shell_rejectSource(reason, model.Source), model.Mode, new Verification(/* Unsupported */ 5, [reason]), (bind$0040_5 = model.Playback, new PlaybackState(bind$0040_5.CurrentTick, bind$0040_5.FinalTick, false, bind$0040_5.Speed)), model.Selection, model.Inspection, model.Patch, model.Lab, WorkerState.WorkerReady, model.ActiveOperation, model.NextOperation, "Unsupported replay: " + reason));
        }
        case 7: {
            const tick_4 = response.fields[0] | 0;
            const phase = response.fields[1];
            return Shell_stopOperation(new Model(Shell_rejectSource((("diverged at tick " + int32ToString(tick_4)) + " during ") + phase, model.Source), model.Mode, new Verification(/* Diverged */ 6, [tick_4, phase, response.fields[2]]), (bind$0040_6 = model.Playback, new PlaybackState(Shell_clampTick(model.Playback.FinalTick, tick_4), bind$0040_6.FinalTick, false, bind$0040_6.Speed)), model.Selection, model.Inspection, model.Patch, model.Lab, WorkerState.WorkerReady, model.ActiveOperation, model.NextOperation, ((("Replay diverged at tick " + int32ToString(tick_4)) + " during ") + phase) + "."));
        }
        case 8: {
            const reason_1 = response.fields[0];
            return Shell_stopOperation(new Model(Shell_rejectSource(reason_1, model.Source), model.Mode, new Verification(/* Failed */ 7, [reason_1]), (bind$0040_7 = model.Playback, new PlaybackState(bind$0040_7.CurrentTick, bind$0040_7.FinalTick, false, bind$0040_7.Speed)), model.Selection, model.Inspection, model.Patch, model.Lab, WorkerState.WorkerReady, model.ActiveOperation, model.NextOperation, "Replay failed: " + reason_1));
        }
        default: {
            const claim = response.fields[1];
            const metadata_1 = WorkerTransport_metadataFromTransport(response.fields[0]);
            const inspection_1 = WorkerTransport_inspectionFromTransport(response.fields[2]);
            const patternInput = (claim.tag === 1) ? [RunMode.PerspectivePlayback, Verification.PerspectiveReady, "Perspective playback loaded. Hidden world state is unavailable."] : ((claim.tag === 2) ? [new RunMode(/* ScenarioSandbox */ 4, [metadata_1.SourceIdentity]), new Verification(/* SandboxDerived */ 4, [metadata_1.SourceIdentity]), "Design scenario loaded as a sandbox."] : [RunMode.VerifiedReplay, Verification.BrowserKernelVerified, "Replay loaded and browser-kernel verified."]);
            return Shell_stopOperation(new Model(new SourceState(/* Loaded */ 2, [metadata_1]), patternInput[0], patternInput[1], new PlaybackState(Shell_clampTick(metadata_1.FinalTick, inspection_1.Tick), metadata_1.FinalTick, false, model.Playback.Speed), Shell_reconcileSelection(inspection_1, model.Selection), inspection_1, model.Patch, model.Lab, WorkerState.WorkerReady, model.ActiveOperation, model.NextOperation, patternInput[2]));
        }
    }
}

export function Shell_update(msg, model) {
    let bind$0040_1, bind$0040_2, bind$0040_3, option_1, bind$0040_5, bind$0040_4, bind$0040_6, bind$0040_7, bind$0040_8, bind$0040_9, bind$0040_10, bind$0040_15, bind$0040_13, bind$0040_14, bind$0040_12, bind$0040_16, option_5, bind$0040_17, matchValue;
    let matchResult, bytes, sourceName, operation, response, navigation, tick_1, speed, unitId, eventId, formula, scenarioIdentity, name_1, value_2, parameter, reason;
    switch (msg.tag) {
        case 1: {
            matchResult = 1;
            operation = msg.fields[0];
            response = msg.fields[1];
            break;
        }
        case 2: {
            if (model.Playback.FinalTick > 0) {
                matchResult = 2;
            }
            else {
                matchResult = 17;
            }
            break;
        }
        case 4: {
            if (model.Playback.CurrentTick < model.Playback.FinalTick) {
                matchResult = 3;
            }
            else {
                matchResult = 17;
            }
            break;
        }
        case 3: {
            if (model.Playback.CurrentTick > 0) {
                matchResult = 4;
            }
            else {
                matchResult = 17;
            }
            break;
        }
        case 5: {
            matchResult = 5;
            navigation = msg;
            break;
        }
        case 6: {
            matchResult = 5;
            navigation = msg;
            break;
        }
        case 7: {
            if (model.Playback.FinalTick > 0) {
                matchResult = 6;
                tick_1 = msg.fields[0];
            }
            else {
                matchResult = 17;
            }
            break;
        }
        case 8: {
            matchResult = 7;
            speed = msg.fields[0];
            break;
        }
        case 9: {
            matchResult = 8;
            unitId = msg.fields[0];
            break;
        }
        case 10: {
            matchResult = 9;
            eventId = msg.fields[0];
            break;
        }
        case 11: {
            matchResult = 10;
            formula = msg.fields[0];
            break;
        }
        case 13: {
            matchResult = 11;
            scenarioIdentity = msg.fields[0];
            break;
        }
        case 12: {
            if ((matchValue = model.Mode, (matchValue.tag === 3) ? true : ((matchValue.tag === 4) ? true : ((matchValue.tag === 0) ? false : (!(matchValue.tag === 2)))))) {
                matchResult = 12;
                name_1 = msg.fields[0];
                value_2 = msg.fields[1];
            }
            else {
                matchResult = 17;
            }
            break;
        }
        case 14: {
            matchResult = 13;
            parameter = msg.fields[0];
            break;
        }
        case 15: {
            matchResult = 14;
            break;
        }
        case 16: {
            matchResult = 15;
            break;
        }
        case 17: {
            matchResult = 16;
            reason = msg.fields[0];
            break;
        }
        default: {
            matchResult = 0;
            bytes = msg.fields[1];
            sourceName = msg.fields[0];
        }
    }
    switch (matchResult) {
        case 0:
            return Shell_beginOperation(new RunnerRequest(/* LoadPackage */ 0, [sourceName, bytes]), new Model(new SourceState(/* Reading */ 1, [sourceName]), RunMode.NoRun, Verification.Loading, new PlaybackState(0, 0, false, model.Playback.Speed), new SelectionState(undefined, undefined, undefined), undefined, empty({
                Compare: (x, y) => (comparePrimitives(x, y) | 0),
            }), new LabState(undefined, undefined, undefined), new WorkerState(/* WorkerBusy */ 2, [0]), model.ActiveOperation, model.NextOperation, ("Loading " + sourceName) + "."));
        case 1:
            if (equals(model.ActiveOperation, operation)) {
                return [Shell_applyRunnerResponse(response, model), empty_1()];
            }
            else {
                return [model, empty_1()];
            }
        case 2:
            return [new Model(model.Source, model.Mode, model.Verification, (bind$0040_1 = model.Playback, new PlaybackState(bind$0040_1.CurrentTick, bind$0040_1.FinalTick, !model.Playback.IsPlaying, bind$0040_1.Speed)), model.Selection, model.Inspection, model.Patch, model.Lab, model.Worker, model.ActiveOperation, model.NextOperation, model.Playback.IsPlaying ? "Playback paused." : "Playback started."), empty_1()];
        case 3:
            return Shell_beginOperation(new RunnerRequest(/* Advance */ 1, [model.Playback.CurrentTick, 1, model.Playback.FinalTick]), new Model(model.Source, model.Mode, model.Verification, (bind$0040_2 = model.Playback, new PlaybackState(bind$0040_2.CurrentTick, bind$0040_2.FinalTick, false, bind$0040_2.Speed)), model.Selection, model.Inspection, model.Patch, model.Lab, model.Worker, model.ActiveOperation, model.NextOperation, model.Announcement));
        case 4:
            return Shell_beginOperation(new RunnerRequest(/* Seek */ 2, [model.Playback.CurrentTick - 1, model.Playback.FinalTick]), new Model(model.Source, model.Mode, model.Verification, (bind$0040_3 = model.Playback, new PlaybackState(bind$0040_3.CurrentTick, bind$0040_3.FinalTick, false, bind$0040_3.Speed)), model.Selection, model.Inspection, model.Patch, model.Lab, model.Worker, model.ActiveOperation, model.NextOperation, "Stepping backward one committed tick."));
        case 5: {
            const events = sortBy((event) => [event.Tick, event.Id], defaultArg((option_1 = model.Inspection, (option_1 != null) ? option_1.Events : undefined), empty_1()), {
                Compare: (x_1, y_1) => (compareArrays(x_1, y_1) | 0),
            });
            const candidate = (navigation.tag === 5) ? tryLast(filter((event_1) => {
                if (event_1.Tick < model.Playback.CurrentTick) {
                    return true;
                }
                else if (event_1.Tick === model.Playback.CurrentTick) {
                    return exists((selected) => (event_1.Id < selected), toArray_1(model.Selection.Event));
                }
                else {
                    return false;
                }
            }, events)) : ((navigation.tag === 6) ? tryFind((event_2) => {
                if (event_2.Tick > model.Playback.CurrentTick) {
                    return true;
                }
                else if (event_2.Tick === model.Playback.CurrentTick) {
                    const matchValue_1 = model.Selection.Event;
                    if (matchValue_1 == null) {
                        return true;
                    }
                    else {
                        return event_2.Id > matchValue_1;
                    }
                }
                else {
                    return false;
                }
            }, events) : undefined);
            if (candidate == null) {
                return [model, empty_1()];
            }
            else {
                const event_3 = candidate;
                return Shell_beginOperation(new RunnerRequest(/* Seek */ 2, [event_3.Tick, model.Playback.FinalTick]), new Model(model.Source, model.Mode, model.Verification, (bind$0040_5 = model.Playback, new PlaybackState(bind$0040_5.CurrentTick, bind$0040_5.FinalTick, false, bind$0040_5.Speed)), (bind$0040_4 = model.Selection, new SelectionState(bind$0040_4.Unit, event_3.Id, bind$0040_4.Formula)), model.Inspection, model.Patch, model.Lab, model.Worker, model.ActiveOperation, model.NextOperation, ((("Navigating to event " + int32ToString(event_3.Id)) + " at tick ") + int32ToString(event_3.Tick)) + "."));
            }
        }
        case 6:
            return Shell_beginOperation(new RunnerRequest(/* Seek */ 2, [tick_1, model.Playback.FinalTick]), new Model(model.Source, model.Mode, model.Verification, (bind$0040_6 = model.Playback, new PlaybackState(bind$0040_6.CurrentTick, bind$0040_6.FinalTick, false, bind$0040_6.Speed)), model.Selection, model.Inspection, model.Patch, model.Lab, model.Worker, model.ActiveOperation, model.NextOperation, "Seeking replay."));
        case 7:
            return [new Model(model.Source, model.Mode, model.Verification, (bind$0040_7 = model.Playback, new PlaybackState(bind$0040_7.CurrentTick, bind$0040_7.FinalTick, bind$0040_7.IsPlaying, speed)), model.Selection, model.Inspection, model.Patch, model.Lab, model.Worker, model.ActiveOperation, model.NextOperation, "Playback speed changed."), empty_1()];
        case 8:
            return [new Model(model.Source, model.Mode, model.Verification, model.Playback, (bind$0040_8 = model.Selection, new SelectionState(unitId, bind$0040_8.Event, bind$0040_8.Formula)), model.Inspection, model.Patch, model.Lab, model.Worker, model.ActiveOperation, model.NextOperation, model.Announcement), empty_1()];
        case 9:
            return [new Model(model.Source, model.Mode, model.Verification, model.Playback, (bind$0040_9 = model.Selection, new SelectionState(bind$0040_9.Unit, eventId, bind$0040_9.Formula)), model.Inspection, model.Patch, model.Lab, model.Worker, model.ActiveOperation, model.NextOperation, model.Announcement), empty_1()];
        case 10:
            return [new Model(model.Source, model.Mode, model.Verification, model.Playback, (bind$0040_10 = model.Selection, new SelectionState(bind$0040_10.Unit, bind$0040_10.Event, formula)), model.Inspection, model.Patch, model.Lab, model.Worker, model.ActiveOperation, model.NextOperation, model.Announcement), empty_1()];
        case 11:
            return Shell_beginOperation(new RunnerRequest(/* LoadScenario */ 4, [scenarioIdentity]), new Model(new SourceState(/* Reading */ 1, [scenarioIdentity + ".sir-scenario"]), RunMode.NoRun, Verification.Loading, new PlaybackState(0, 0, false, model.Playback.Speed), new SelectionState(undefined, undefined, undefined), undefined, empty({
                Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
            }), new LabState(undefined, undefined, undefined), new WorkerState(/* WorkerBusy */ 2, [0]), model.ActiveOperation, model.NextOperation, ("Loading design scenario " + scenarioIdentity) + "."));
        case 12: {
            const patch = add(name_1, value_2, model.Patch);
            const identity = Shell_derivedIdentity(model, patch);
            const matchValue_2 = model.Lab.Scenario;
            if (matchValue_2 == null) {
                const forked_1 = new Model(model.Source, new RunMode(/* SandboxFork */ 3, [identity]), new Verification(/* SandboxDerived */ 4, [identity]), (bind$0040_15 = model.Playback, new PlaybackState(bind$0040_15.CurrentTick, bind$0040_15.FinalTick, false, bind$0040_15.Speed)), model.Selection, model.Inspection, patch, model.Lab, model.Worker, model.ActiveOperation, model.NextOperation, "Parameter edited. This run is now a sandbox fork.");
                return Shell_beginOperation(new RunnerRequest(/* Fork */ 3, [identity, Lab_parametersToTransport(patch)]), forked_1);
            }
            else {
                const scenario = matchValue_2;
                const matchValue_3 = Lab_validate(scenario, patch);
                if (matchValue_3.tag === 0) {
                    const forked = new Model(model.Source, new RunMode(/* SandboxFork */ 3, [identity]), new Verification(/* SandboxDerived */ 4, [identity]), (bind$0040_13 = model.Playback, new PlaybackState(bind$0040_13.CurrentTick, bind$0040_13.FinalTick, false, bind$0040_13.Speed)), model.Selection, model.Inspection, patch, (bind$0040_14 = model.Lab, new LabState(bind$0040_14.Scenario, bind$0040_14.Report, undefined)), model.Worker, model.ActiveOperation, model.NextOperation, "Parameter edited. This run is now a sandbox fork.");
                    return Shell_beginOperation(new RunnerRequest(/* RunExperiment */ 5, [scenario.Identity, Lab_parametersToTransport(patch), undefined]), forked);
                }
                else {
                    const error = matchValue_3.fields[0];
                    return [new Model(model.Source, model.Mode, model.Verification, model.Playback, model.Selection, model.Inspection, patch, (bind$0040_12 = model.Lab, new LabState(bind$0040_12.Scenario, bind$0040_12.Report, error)), model.Worker, model.ActiveOperation, model.NextOperation, "Parameter validation failed: " + error), empty_1()];
                }
            }
        }
        case 13: {
            const matchValue_4 = model.Lab.Scenario;
            let matchResult_1, scenario_1;
            if (matchValue_4 != null) {
                if (model.Lab.ValidationError == null) {
                    matchResult_1 = 0;
                    scenario_1 = matchValue_4;
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
                    return Shell_beginOperation(new RunnerRequest(/* RunExperiment */ 5, [scenario_1.Identity, Lab_parametersToTransport(model.Patch), parameter]), new Model(model.Source, model.Mode, model.Verification, model.Playback, model.Selection, model.Inspection, model.Patch, model.Lab, new WorkerState(/* WorkerBusy */ 2, [0]), model.ActiveOperation, model.NextOperation, "Running deterministic parameter sweep."));
                default:
                    return [model, empty_1()];
            }
        }
        case 14:
            return [new Model(model.Source, model.Mode, model.Verification, (bind$0040_16 = model.Playback, new PlaybackState(bind$0040_16.CurrentTick, bind$0040_16.FinalTick, false, bind$0040_16.Speed)), model.Selection, model.Inspection, model.Patch, model.Lab, WorkerState.WorkerReady, undefined, model.NextOperation, "Operation cancelled."), ofArray(toArray_1((option_5 = model.ActiveOperation, (option_5 != null) ? (new Effect(option_5, RunnerRequest.Cancel)) : undefined)))];
        case 15:
            return [new Model(model.Source, model.Mode, model.Verification, model.Playback, model.Selection, model.Inspection, model.Patch, model.Lab, WorkerState.WorkerReady, model.ActiveOperation, model.NextOperation, "Replay worker ready."), empty_1()];
        case 16:
            return [new Model(model.Source, model.Mode, new Verification(/* Failed */ 7, ["worker stopped: " + reason]), (bind$0040_17 = model.Playback, new PlaybackState(bind$0040_17.CurrentTick, bind$0040_17.FinalTick, false, bind$0040_17.Speed)), model.Selection, model.Inspection, model.Patch, model.Lab, new WorkerState(/* WorkerStopped */ 3, [reason]), undefined, model.NextOperation, "Replay worker stopped. Verification has been revoked."), empty_1()];
        default:
            return [model, empty_1()];
    }
}

export function Shell_playbackTick(model) {
    if (model.Playback.IsPlaying && (model.ActiveOperation == null)) {
        return Shell_beginOperation(new RunnerRequest(/* Advance */ 1, [model.Playback.CurrentTick, Shell_advanceSize(model.Playback.Speed), model.Playback.FinalTick]), model);
    }
    else {
        return [model, empty_1()];
    }
}

