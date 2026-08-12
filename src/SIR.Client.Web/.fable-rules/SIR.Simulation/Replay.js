
import { FSharpException, Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { class_type, string_type, option_type, bool_type, array_type, uint8_type, record_type, int32_type, union_type, list_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { Simulation_runTick, SimulationState, Board, UnitState, SemanticEdge, KernelInput, Simulation_unitId, Side, Simulation_eventsBytes, Simulation_unitIdValue, SimulationState_$reflection, KernelInput_$reflection } from "./Simulation.js";
import { RulePackageIdentity, RulePackageIdentity_$reflection } from "../SIR.Domain/RuleTypes.js";
import { FSharpResult$2 } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { direction8, boundedInt32, int32LittleEndian, concatenate } from "../SIR.Domain/CanonicalEncoding.js";
import { tail, head, isEmpty, filter, empty as empty_1, tryFind, choose, cons, tryPick, find, sort, exists as exists_1, map, singleton as singleton_1, length, append as append_1, sortBy, ofArray, collect } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { collect as collect_1, exists, empty, append, toList, singleton, delay, toArray } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { comparePrimitives, equals, compare, compareArrays } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { ofList as ofList_1, FSharpMap__get_Count, toList as toList_1 } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { ofList, count as count_1, FSharpSet__get_Count, toList as toList_2 } from "../fable_modules/fable-library-js.5.13.0/Set.js";
import { sha256 } from "../SIR.Domain/CanonicalHash.js";
import { get_UTF8 } from "../fable_modules/fable-library-js.5.13.0/Encoding.js";
import { equalsWith, item } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { Cell } from "../fable_modules/FS.GG.Game.Core.0.13.0/Primitives.fs.js";
import { isNullOrWhiteSpace, printf, toText } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { rangeDouble } from "../fable_modules/fable-library-js.5.13.0/Range.js";
import { BoundedInt32Module_create } from "../SIR.Domain/BoundedInt32.js";
import { Direction8, Direction8Module_tryFromCode } from "../SIR.Domain/Orientation.js";
import { Edges_edgeBetween } from "../fable_modules/FS.GG.Game.Core.0.13.0/Edges.fs.js";
import { value as value_2 } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { min } from "../fable_modules/fable-library-js.5.13.0/Double.js";

/**
 * Disclosure scope is encoded in the replay type, not inferred by the UI.
 */
export class ReplayContent extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["AuthorizedFullReplay", "PerspectivePlayback"];
    }
}

export function ReplayContent_$reflection() {
    return union_type("SIR.Simulation.ReplayContent", [], ReplayContent, () => [[["Item", FullReplay_$reflection()]], [["Item", list_type(PerspectiveFrame_$reflection())]]]);
}

/**
 * One accepted external input after authoritative ordering.
 */
export class ReplayInput extends Record {
    constructor(Tick, Sequence, Input) {
        super();
        this.Tick = (Tick | 0);
        this.Sequence = (Sequence | 0);
        this.Input = Input;
    }
}

export function ReplayInput_$reflection() {
    return record_type("SIR.Simulation.ReplayInput", [], ReplayInput, () => [["Tick", int32_type], ["Sequence", int32_type], ["Input", KernelInput_$reflection()]]);
}

/**
 * One accepted player-WASM output at the kernel boundary.
 */
export class AcceptedWasmOutput extends Record {
    constructor(Tick, Sequence, Input) {
        super();
        this.Tick = (Tick | 0);
        this.Sequence = (Sequence | 0);
        this.Input = Input;
    }
}

export function AcceptedWasmOutput_$reflection() {
    return record_type("SIR.Simulation.AcceptedWasmOutput", [], AcceptedWasmOutput, () => [["Tick", int32_type], ["Sequence", int32_type], ["Input", KernelInput_$reflection()]]);
}

/**
 * A retained, independently verifiable seek point.
 */
export class ReplayCheckpoint extends Record {
    constructor(Tick, State, StateHash, EventHash) {
        super();
        this.Tick = (Tick | 0);
        this.State = State;
        this.StateHash = StateHash;
        this.EventHash = EventHash;
    }
}

export function ReplayCheckpoint_$reflection() {
    return record_type("SIR.Simulation.ReplayCheckpoint", [], ReplayCheckpoint, () => [["Tick", int32_type], ["State", SimulationState_$reflection()], ["StateHash", array_type(uint8_type)], ["EventHash", array_type(uint8_type)]]);
}

/**
 * The canonical terminal claim made by an authorized full replay.
 */
export class ReplayFinalResult extends Record {
    constructor(Tick, OutcomeCode, StateHash, EventHash) {
        super();
        this.Tick = (Tick | 0);
        this.OutcomeCode = (OutcomeCode | 0);
        this.StateHash = StateHash;
        this.EventHash = EventHash;
    }
}

export function ReplayFinalResult_$reflection() {
    return record_type("SIR.Simulation.ReplayFinalResult", [], ReplayFinalResult, () => [["Tick", int32_type], ["OutcomeCode", int32_type], ["StateHash", array_type(uint8_type)], ["EventHash", array_type(uint8_type)]]);
}

/**
 * Kernel material that is deliberately absent from perspective playback.
 */
export class FullReplay extends Record {
    constructor(InitialSnapshot, OrderedInputs, AcceptedWasmOutputs, Checkpoints, FinalResult) {
        super();
        this.InitialSnapshot = InitialSnapshot;
        this.OrderedInputs = OrderedInputs;
        this.AcceptedWasmOutputs = AcceptedWasmOutputs;
        this.Checkpoints = Checkpoints;
        this.FinalResult = FinalResult;
    }
}

export function FullReplay_$reflection() {
    return record_type("SIR.Simulation.FullReplay", [], FullReplay, () => [["InitialSnapshot", SimulationState_$reflection()], ["OrderedInputs", list_type(ReplayInput_$reflection())], ["AcceptedWasmOutputs", list_type(AcceptedWasmOutput_$reflection())], ["Checkpoints", list_type(ReplayCheckpoint_$reflection())], ["FinalResult", ReplayFinalResult_$reflection()]]);
}

/**
 * Knowledge-filtered playback material with no reconstructable kernel state.
 */
export class PerspectiveFrame extends Record {
    constructor(Tick, ProjectionHash) {
        super();
        this.Tick = (Tick | 0);
        this.ProjectionHash = ProjectionHash;
    }
}

export function PerspectiveFrame_$reflection() {
    return record_type("SIR.Simulation.PerspectiveFrame", [], PerspectiveFrame, () => [["Tick", int32_type], ["ProjectionHash", array_type(uint8_type)]]);
}

/**
 * Content-addressed rule identity and canonical applications retained by replay v3.
 */
export class ReplayRulesArchive extends Record {
    constructor(SchemaVersion, Identity, Applications, ContentDigest) {
        super();
        this.SchemaVersion = (SchemaVersion | 0);
        this.Identity = Identity;
        this.Applications = Applications;
        this.ContentDigest = ContentDigest;
    }
}

export function ReplayRulesArchive_$reflection() {
    return record_type("SIR.Simulation.ReplayRulesArchive", [], ReplayRulesArchive, () => [["SchemaVersion", int32_type], ["Identity", RulePackageIdentity_$reflection()], ["Applications", list_type(array_type(uint8_type))], ["ContentDigest", array_type(uint8_type)]]);
}

/**
 * Versioned replay package header and disclosure-specific payload.
 */
export class ReplayPackage extends Record {
    constructor(FormatVersion, EngineHash, RulesetHash, FullReplayAuthorized, RulesArchive, Content) {
        super();
        this.FormatVersion = (FormatVersion | 0);
        this.EngineHash = EngineHash;
        this.RulesetHash = RulesetHash;
        this.FullReplayAuthorized = FullReplayAuthorized;
        this.RulesArchive = RulesArchive;
        this.Content = Content;
    }
}

export function ReplayPackage_$reflection() {
    return record_type("SIR.Simulation.ReplayPackage", [], ReplayPackage, () => [["FormatVersion", int32_type], ["EngineHash", array_type(uint8_type)], ["RulesetHash", array_type(uint8_type)], ["FullReplayAuthorized", bool_type], ["RulesArchive", option_type(ReplayRulesArchive_$reflection())], ["Content", ReplayContent_$reflection()]]);
}

/**
 * Resource limits applied before and during package decoding.
 */
export class ReplayLimits extends Record {
    constructor(MaxPackageBytes, MaxInputs, MaxWasmOutputs, MaxCheckpoints, MaxPerspectiveFrames, MaxUnits, MaxEdges, MaxObservations) {
        super();
        this.MaxPackageBytes = (MaxPackageBytes | 0);
        this.MaxInputs = (MaxInputs | 0);
        this.MaxWasmOutputs = (MaxWasmOutputs | 0);
        this.MaxCheckpoints = (MaxCheckpoints | 0);
        this.MaxPerspectiveFrames = (MaxPerspectiveFrames | 0);
        this.MaxUnits = (MaxUnits | 0);
        this.MaxEdges = (MaxEdges | 0);
        this.MaxObservations = (MaxObservations | 0);
    }
}

export function ReplayLimits_$reflection() {
    return record_type("SIR.Simulation.ReplayLimits", [], ReplayLimits, () => [["MaxPackageBytes", int32_type], ["MaxInputs", int32_type], ["MaxWasmOutputs", int32_type], ["MaxCheckpoints", int32_type], ["MaxPerspectiveFrames", int32_type], ["MaxUnits", int32_type], ["MaxEdges", int32_type], ["MaxObservations", int32_type]]);
}

/**
 * Why an untrusted replay package was rejected.
 */
export class ReplayError extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["PackageTooLarge", "MalformedPackage", "UnsupportedFormat", "InvalidHashLength", "EngineMismatch", "UnauthorizedFullReplay", "ResourceLimitExceeded", "InvalidOrdering", "InvalidCheckpoint", "ReplayDivergence", "PerspectiveHasNoKernel", "WasmExecutionNotVerified", "WasmOutputDivergence"];
    }
    static UnauthorizedFullReplay = new ReplayError(5, []);
    static PerspectiveHasNoKernel = new ReplayError(10, []);
    static WasmExecutionNotVerified = new ReplayError(11, []);
}

export function ReplayError_$reflection() {
    return union_type("SIR.Simulation.ReplayError", [], ReplayError, () => [[["actual", int32_type], ["maximum", int32_type]], [["detail", string_type]], [["actual", int32_type], ["supported", int32_type]], [["field", string_type], ["actual", int32_type]], [["expected", array_type(uint8_type)], ["actual", array_type(uint8_type)]], [], [["field", string_type], ["actual", int32_type], ["maximum", int32_type]], [["field", string_type]], [["tick", int32_type], ["detail", string_type]], [["tick", int32_type], ["field", string_type]], [], [], [["tick", int32_type], ["sequence", int32_type]]]);
}

/**
 * The verification claim is explicit in the returned value.
 */
export class ReplayVerification extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["BrowserKernelVerified", "AuthoritativeVerified", "PerspectiveReady"];
    }
}

export function ReplayVerification_$reflection() {
    return union_type("SIR.Simulation.ReplayVerification", [], ReplayVerification, () => [[["Item", ReplayFinalResult_$reflection()]], [["Item", ReplayFinalResult_$reflection()]], [["Item", list_type(PerspectiveFrame_$reflection())]]]);
}

class ReplayDecodeFailure extends FSharpException {
    constructor(Data0) {
        super();
        this.Data0 = Data0;
    }
}

function ReplayDecodeFailure_$reflection() {
    return class_type("SIR.Simulation.ReplayDecodeFailure", undefined, ReplayDecodeFailure, class_type("System.Exception"));
}

class ReplayReader extends Record {
    constructor(Bytes, Offset) {
        super();
        this.Bytes = Bytes;
        this.Offset = (Offset | 0);
    }
}

function ReplayReader_$reflection() {
    return record_type("SIR.Simulation.ReplayReader", [], ReplayReader, () => [["Bytes", array_type(uint8_type)], ["Offset", int32_type]]);
}

export const Replay_defaultLimits = new ReplayLimits(1048576, 16384, 16384, 4096, 65536, 4096, 16384, 65536);

const Replay_magic = new Uint8Array([83, 73, 82, 82]);

const Replay_archiveSchemaVersion = 1;

function Replay_requireHash(field, hash) {
    if (hash.length === 32) {
        return new FSharpResult$2(/* Ok */ 0, [undefined]);
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* InvalidHashLength */ 3, [field, hash.length])]);
    }
}

function Replay_cellBytes(cell) {
    return concatenate([int32LittleEndian(cell.Col), int32LittleEndian(cell.Row)]);
}

function Replay_unitIdBytes(id) {
    return int32LittleEndian(Simulation_unitIdValue(id));
}

function Replay_sideByte(side) {
    if (side.tag === 1) {
        return 1;
    }
    else {
        return 0;
    }
}

function Replay_inputBytes(input) {
    switch (input.tag) {
        case 1:
            return concatenate([new Uint8Array([1]), Replay_unitIdBytes(input.fields[0]), Replay_unitIdBytes(input.fields[1])]);
        case 2:
            return concatenate([new Uint8Array([2]), Replay_unitIdBytes(input.fields[0]), Replay_unitIdBytes(input.fields[1])]);
        default:
            return concatenate([new Uint8Array([0]), Replay_unitIdBytes(input.fields[0]), Replay_cellBytes(input.fields[1])]);
    }
}

function Replay_snapshotBytesForVersion(formatVersion, state) {
    const edgeSegments = collect((edge_1) => ofArray([Replay_cellBytes(edge_1.Edge.Lo), Replay_cellBytes(edge_1.Edge.Hi), toArray(delay(() => (edge_1.BlocksMovement ? singleton(1) : singleton(0))))]), sortBy((edge) => [edge.Edge.Lo.Col, edge.Edge.Lo.Row, edge.Edge.Hi.Col, edge.Edge.Hi.Row, edge.BlocksMovement], state.Board.Edges, {
        Compare: (x, y) => (compareArrays(x, y) | 0),
    }));
    const unitSegments = collect((tupledArg) => {
        const unit = tupledArg[1];
        return toList(delay(() => append(singleton(Replay_unitIdBytes(tupledArg[0])), delay(() => append(singleton(new Uint8Array([Replay_sideByte(unit.Side)])), delay(() => append(singleton(Replay_cellBytes(unit.Cell)), delay(() => append(singleton(boundedInt32(unit.Health)), delay(() => append((formatVersion >= 2) ? singleton(direction8(unit.BodyFacing)) : empty(), delay(() => ((formatVersion >= 2) ? singleton(direction8(unit.AttentionDirection)) : empty())))))))))))));
    }, toList_1(state.Units));
    const observationSegments = collect((tupledArg_1) => ofArray([Replay_unitIdBytes(tupledArg_1[0]), Replay_unitIdBytes(tupledArg_1[1])]), toList_2(state.Observations));
    return concatenate(append_1(ofArray([int32LittleEndian(state.Tick), Replay_cellBytes(state.Board.Minimum), Replay_cellBytes(state.Board.Maximum), int32LittleEndian(length(state.Board.Edges))]), append_1(edgeSegments, append_1(singleton_1(int32LittleEndian(FSharpMap__get_Count(state.Units))), append_1(unitSegments, append_1(singleton_1(int32LittleEndian(FSharpSet__get_Count(state.Observations))), observationSegments))))));
}

/**
 * Complete current-version snapshot encoding, including orientation.
 */
export function Replay_snapshotBytes(state) {
    return Replay_snapshotBytesForVersion(3, state);
}

function Replay_stateHashForVersion(formatVersion, state) {
    return sha256(Replay_snapshotBytesForVersion(formatVersion, state));
}

export function Replay_stateHash(state) {
    return Replay_stateHashForVersion(3, state);
}

export function Replay_eventHash(events) {
    return sha256(Simulation_eventsBytes(events));
}

function Replay_lengthPrefixed(bytes) {
    return concatenate([int32LittleEndian(bytes.length), bytes]);
}

function Replay_replayInputBytes(input) {
    return concatenate([int32LittleEndian(input.Tick), int32LittleEndian(input.Sequence), Replay_inputBytes(input.Input)]);
}

function Replay_wasmOutputBytes(output) {
    return concatenate([int32LittleEndian(output.Tick), int32LittleEndian(output.Sequence), Replay_inputBytes(output.Input)]);
}

function Replay_checkpointBytes(formatVersion, checkpoint) {
    return concatenate([int32LittleEndian(checkpoint.Tick), Replay_lengthPrefixed(Replay_snapshotBytesForVersion(formatVersion, checkpoint.State)), checkpoint.StateHash, checkpoint.EventHash]);
}

function Replay_fullReplayBytes(formatVersion, full) {
    const inputSegments = map(Replay_replayInputBytes, full.OrderedInputs);
    const wasmSegments = map(Replay_wasmOutputBytes, full.AcceptedWasmOutputs);
    const checkpointSegments = map((checkpoint) => Replay_checkpointBytes(formatVersion, checkpoint), full.Checkpoints);
    return concatenate(append_1(ofArray([Replay_lengthPrefixed(Replay_snapshotBytesForVersion(formatVersion, full.InitialSnapshot)), int32LittleEndian(length(full.OrderedInputs))]), append_1(inputSegments, append_1(singleton_1(int32LittleEndian(length(full.AcceptedWasmOutputs))), append_1(wasmSegments, append_1(singleton_1(int32LittleEndian(length(full.Checkpoints))), append_1(checkpointSegments, ofArray([int32LittleEndian(full.FinalResult.Tick), int32LittleEndian(full.FinalResult.OutcomeCode), full.FinalResult.StateHash, full.FinalResult.EventHash]))))))));
}

function Replay_perspectiveBytes(frames) {
    return concatenate(append_1(singleton_1(int32LittleEndian(length(frames))), collect((frame) => ofArray([int32LittleEndian(frame.Tick), frame.ProjectionHash]), frames)));
}

function Replay_textBytes(value) {
    return get_UTF8().getBytes(value);
}

function Replay_archiveIdentityBytes(identity) {
    return concatenate([int32LittleEndian(identity.SchemaVersion), Replay_lengthPrefixed(Replay_textBytes(identity.EngineIdentity)), Replay_lengthPrefixed(Replay_textBytes(identity.CompatibilityProfile)), Replay_lengthPrefixed(Replay_textBytes(identity.PackageVersion)), Replay_lengthPrefixed(Replay_textBytes(identity.SourceCommit)), identity.ImplementationDigest, identity.SemanticDigest, identity.ManifestDigest]);
}

function Replay_archiveContentBytes(archive) {
    return concatenate(append_1(ofArray([int32LittleEndian(archive.SchemaVersion), Replay_archiveIdentityBytes(archive.Identity), int32LittleEndian(length(archive.Applications))]), map(Replay_lengthPrefixed, archive.Applications)));
}

export function Replay_createRulesArchive(identity, applications) {
    const partial = new ReplayRulesArchive(Replay_archiveSchemaVersion, identity, applications, new Uint8Array([]));
    return new ReplayRulesArchive(partial.SchemaVersion, partial.Identity, partial.Applications, sha256(Replay_archiveContentBytes(partial)));
}

function Replay_rulesArchiveBytes(archive) {
    return concatenate([Replay_archiveContentBytes(archive), archive.ContentDigest]);
}

/**
 * Encodes a package in the stable version-1 binary format.
 */
export function Replay_encode(package$) {
    let patternInput;
    const matchValue = package$.Content;
    patternInput = ((matchValue.tag === 1) ? [1, Replay_perspectiveBytes(matchValue.fields[0])] : [0, Replay_fullReplayBytes(package$.FormatVersion, matchValue.fields[0])]);
    return concatenate(toList(delay(() => append(singleton(Replay_magic), delay(() => append(singleton(int32LittleEndian(package$.FormatVersion)), delay(() => append(singleton(new Uint8Array([patternInput[0]])), delay(() => append(singleton(package$.EngineHash), delay(() => append(singleton(package$.RulesetHash), delay(() => append(singleton(toArray(delay(() => (package$.FullReplayAuthorized ? singleton(1) : singleton(0))))), delay(() => {
        let matchValue_1;
        return append((package$.FormatVersion >= 3) ? ((matchValue_1 = package$.RulesArchive, (matchValue_1 != null) ? singleton(concatenate([new Uint8Array([1]), Replay_lengthPrefixed(Replay_rulesArchiveBytes(matchValue_1))])) : singleton(new Uint8Array([0])))) : empty(), delay(() => singleton(patternInput[1])));
    })))))))))))))));
}

function Replay_failDecode(detail) {
    throw new ReplayDecodeFailure(detail);
}

function Replay_readBytes(count, reader) {
    if ((count < 0) ? true : (reader.Offset > (reader.Bytes.length - count))) {
        Replay_failDecode("Unexpected end of package.");
    }
    const value = reader.Bytes.slice(reader.Offset, ((reader.Offset + count) - 1) + 1);
    reader.Offset = ((reader.Offset + count) | 0);
    return value;
}

function Replay_readByte(reader) {
    return item(0, Replay_readBytes(1, reader));
}

function Replay_readInt32(reader) {
    const bytes = Replay_readBytes(4, reader);
    return (((~~item(0, bytes) | (~~item(1, bytes) << 8)) | (~~item(2, bytes) << 16)) | (~~item(3, bytes) << 24)) | 0;
}

function Replay_readCell(reader) {
    return new Cell(Replay_readInt32(reader), Replay_readInt32(reader));
}

function Replay_readCount(field, maximum, reader) {
    const count = Replay_readInt32(reader) | 0;
    if ((count < 0) ? true : (count > maximum)) {
        Replay_failDecode(toText(printf("Resource limit exceeded for %s: %d is outside 0..%d."))(field)(count)(maximum));
    }
    return count | 0;
}

function Replay_readBool(reader) {
    const matchValue = Replay_readByte(reader);
    switch (matchValue) {
        case 0:
            return false;
        case 1:
            return true;
        default:
            return Replay_failDecode(toText(printf("Invalid Boolean byte %d."))(matchValue));
    }
}

function Replay_readLengthPrefixed(field, maximum, reader) {
    return Replay_readBytes(Replay_readCount(field, maximum, reader), reader);
}

function Replay_readText(field, reader) {
    let value;
    const bytes = Replay_readLengthPrefixed(field, 256, reader);
    const objectArg = get_UTF8();
    value = objectArg.getString(bytes);
    if (isNullOrWhiteSpace(value)) {
        Replay_failDecode(field + " is empty.");
    }
    return value;
}

function Replay_readRulesArchive(bytes) {
    const archiveReader = new ReplayReader(bytes, 0);
    const schemaVersion = Replay_readInt32(archiveReader) | 0;
    if (schemaVersion !== Replay_archiveSchemaVersion) {
        Replay_failDecode(toText(printf("Unsupported rules archive schema %d."))(schemaVersion));
    }
    const identitySchemaVersion = Replay_readInt32(archiveReader) | 0;
    if (identitySchemaVersion !== 1) {
        Replay_failDecode(toText(printf("Unsupported rule identity schema %d."))(identitySchemaVersion));
    }
    const identity = new RulePackageIdentity(identitySchemaVersion, Replay_readText("rules archive engine identity", archiveReader), Replay_readText("rules archive compatibility profile", archiveReader), Replay_readText("rules archive package version", archiveReader), Replay_readText("rules archive source commit", archiveReader), Replay_readBytes(32, archiveReader), Replay_readBytes(32, archiveReader), Replay_readBytes(32, archiveReader));
    if ((identity.SourceCommit.length !== 40) ? true : exists((value) => !(((value >= "0") && (value <= "9")) ? true : ((value >= "a") && (value <= "f"))), identity.SourceCommit.split(""))) {
        Replay_failDecode("Rules archive source commit is not a lowercase 40-character SHA.");
    }
    const applicationCount = Replay_readCount("rules archive applications", 1024, archiveReader) | 0;
    const applications = toList(delay(() => collect_1((matchValue) => singleton(Replay_readLengthPrefixed("rules archive application", 65536, archiveReader)), rangeDouble(1, 1, applicationCount))));
    const contentBoundary = archiveReader.Offset | 0;
    const contentDigest = Replay_readBytes(32, archiveReader);
    if (archiveReader.Offset !== bytes.length) {
        Replay_failDecode("Rules archive has trailing bytes.");
    }
    if (!equalsWith((x, y) => (x === y), sha256(bytes.slice(0, (contentBoundary - 1) + 1)), contentDigest)) {
        Replay_failDecode("Rules archive content digest does not match.");
    }
    if (exists_1((application) => {
        if (application.length < 32) {
            return true;
        }
        else {
            return !equalsWith((x_1, y_1) => (x_1 === y_1), application.slice(application.length - 32, application.length), identity.ManifestDigest);
        }
    }, applications)) {
        Replay_failDecode("Rules archive application is not bound to its manifest identity.");
    }
    return new ReplayRulesArchive(schemaVersion, identity, applications, contentDigest);
}

function Replay_readSide(reader) {
    const matchValue = Replay_readByte(reader);
    switch (matchValue) {
        case 0:
            return Side.Red;
        case 1:
            return Side.Blue;
        default:
            return Replay_failDecode(toText(printf("Invalid side byte %d."))(matchValue));
    }
}

function Replay_readHealth(reader) {
    const matchValue = BoundedInt32Module_create(0, 100, Replay_readInt32(reader));
    if (matchValue.tag === 1) {
        return Replay_failDecode("Unit health is outside 0..100.");
    }
    else {
        return matchValue.fields[0];
    }
}

function Replay_readDirection(field, reader) {
    const matchValue = Direction8Module_tryFromCode(Replay_readByte(reader));
    if (matchValue == null) {
        return Replay_failDecode(("Invalid " + field) + " direction code.");
    }
    else {
        return matchValue;
    }
}

function Replay_readInput(reader) {
    const matchValue = Replay_readByte(reader);
    switch (matchValue) {
        case 0:
            return new KernelInput(/* Move */ 0, [Simulation_unitId(Replay_readInt32(reader)), Replay_readCell(reader)]);
        case 1:
            return new KernelInput(/* Observe */ 1, [Simulation_unitId(Replay_readInt32(reader)), Simulation_unitId(Replay_readInt32(reader))]);
        case 2:
            return new KernelInput(/* Attack */ 2, [Simulation_unitId(Replay_readInt32(reader)), Simulation_unitId(Replay_readInt32(reader))]);
        default:
            return Replay_failDecode(toText(printf("Invalid kernel-input tag %d."))(matchValue));
    }
}

function Replay_readSnapshot(formatVersion, limits, reader) {
    const declaredLength = Replay_readInt32(reader) | 0;
    if ((declaredLength < 0) ? true : (declaredLength > (reader.Bytes.length - reader.Offset))) {
        Replay_failDecode("Invalid snapshot length.");
    }
    const boundary = (reader.Offset + declaredLength) | 0;
    const tick = Replay_readInt32(reader) | 0;
    const minimum = Replay_readCell(reader);
    const maximum = Replay_readCell(reader);
    const edgeCount = Replay_readCount("edges", limits.MaxEdges, reader) | 0;
    const edges = toList(delay(() => collect_1((matchValue) => {
        let option_1;
        return singleton(new SemanticEdge((option_1 = Edges_edgeBetween(Replay_readCell(reader), Replay_readCell(reader)), (option_1 != null) ? option_1 : Replay_failDecode("A semantic edge is not orthogonal.")), Replay_readBool(reader)));
    }, rangeDouble(1, 1, edgeCount))));
    const unitCount = Replay_readCount("units", limits.MaxUnits, reader) | 0;
    const units = toList(delay(() => collect_1((matchValue_1) => {
        const unitId = Simulation_unitId(Replay_readInt32(reader));
        return singleton([unitId, new UnitState(unitId, Replay_readSide(reader), Replay_readCell(reader), Replay_readHealth(reader), (formatVersion >= 2) ? Replay_readDirection("body-facing", reader) : Direction8.North, (formatVersion >= 2) ? Replay_readDirection("attention", reader) : Direction8.North)]);
    }, rangeDouble(1, 1, unitCount))));
    if (count_1(ofList(map((tuple) => tuple[0], units), {
        Compare: (x, y) => (compare(x, y) | 0),
    })) !== unitCount) {
        Replay_failDecode("Snapshot contains duplicate unit identifiers.");
    }
    const observationCount = Replay_readCount("observations", limits.MaxObservations, reader) | 0;
    const observations = ofList(toList(delay(() => collect_1((matchValue_2) => singleton([Simulation_unitId(Replay_readInt32(reader)), Simulation_unitId(Replay_readInt32(reader))]), rangeDouble(1, 1, observationCount)))), {
        Compare: (x_1, y_1) => (compareArrays(x_1, y_1) | 0),
    });
    if (FSharpSet__get_Count(observations) !== observationCount) {
        Replay_failDecode("Snapshot contains duplicate observations.");
    }
    if (reader.Offset !== boundary) {
        Replay_failDecode("Snapshot length does not match its canonical fields.");
    }
    return new SimulationState(tick, new Board(minimum, maximum, edges), ofList_1(units, {
        Compare: (x_2, y_2) => (compare(x_2, y_2) | 0),
    }), observations);
}

function Replay_readReplayInput(reader) {
    return new ReplayInput(Replay_readInt32(reader), Replay_readInt32(reader), Replay_readInput(reader));
}

function Replay_readWasmOutput(reader) {
    return new AcceptedWasmOutput(Replay_readInt32(reader), Replay_readInt32(reader), Replay_readInput(reader));
}

function Replay_readCheckpoint(formatVersion, limits, reader) {
    return new ReplayCheckpoint(Replay_readInt32(reader), Replay_readSnapshot(formatVersion, limits, reader), Replay_readBytes(32, reader), Replay_readBytes(32, reader));
}

function Replay_readFull(formatVersion, limits, reader) {
    const initial = Replay_readSnapshot(formatVersion, limits, reader);
    const inputCount = Replay_readCount("inputs", limits.MaxInputs, reader) | 0;
    const inputs = toList(delay(() => collect_1((matchValue) => singleton(Replay_readReplayInput(reader)), rangeDouble(1, 1, inputCount))));
    const wasmCount = Replay_readCount("accepted WASM outputs", limits.MaxWasmOutputs, reader) | 0;
    const wasm = toList(delay(() => collect_1((matchValue_1) => singleton(Replay_readWasmOutput(reader)), rangeDouble(1, 1, wasmCount))));
    const checkpointCount = Replay_readCount("checkpoints", limits.MaxCheckpoints, reader) | 0;
    return new FullReplay(initial, inputs, wasm, toList(delay(() => collect_1((matchValue_2) => singleton(Replay_readCheckpoint(formatVersion, limits, reader)), rangeDouble(1, 1, checkpointCount)))), new ReplayFinalResult(Replay_readInt32(reader), Replay_readInt32(reader), Replay_readBytes(32, reader), Replay_readBytes(32, reader)));
}

function Replay_readPerspective(limits, reader) {
    const count = Replay_readCount("perspective frames", limits.MaxPerspectiveFrames, reader) | 0;
    return toList(delay(() => collect_1((matchValue) => singleton(new PerspectiveFrame(Replay_readInt32(reader), Replay_readBytes(32, reader))), rangeDouble(1, 1, count))));
}

/**
 * Decodes untrusted bytes with strict bounds and no partial acceptance.
 */
export function Replay_decode(limits, bytes) {
    if (bytes.length > limits.MaxPackageBytes) {
        return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* PackageTooLarge */ 0, [bytes.length, limits.MaxPackageBytes])]);
    }
    else {
        try {
            const reader = new ReplayReader(bytes, 0);
            if (!equalsWith((x, y) => (x === y), Replay_readBytes(Replay_magic.length, reader), Replay_magic)) {
                Replay_failDecode("Replay magic is invalid.");
            }
            const version = Replay_readInt32(reader) | 0;
            if (((version !== 1) && (version !== 2)) && (version !== 3)) {
                Replay_failDecode(toText(printf("Unsupported replay format %d."))(version));
            }
            const disclosure = Replay_readByte(reader);
            const engineHash = Replay_readBytes(32, reader);
            const rulesetHash = Replay_readBytes(32, reader);
            const fullReplayAuthorized = Replay_readBool(reader);
            let rulesArchive;
            if (version >= 3) {
                const matchValue = Replay_readByte(reader);
                rulesArchive = ((matchValue === 0) ? undefined : ((matchValue === 1) ? Replay_readRulesArchive(Replay_readLengthPrefixed("rules archive", limits.MaxPackageBytes, reader)) : Replay_failDecode(toText(printf("Invalid rules archive byte %d."))(matchValue))));
            }
            else {
                rulesArchive = undefined;
            }
            const content = (disclosure === 0) ? (new ReplayContent(/* AuthorizedFullReplay */ 0, [Replay_readFull(version, limits, reader)])) : ((disclosure === 1) ? (new ReplayContent(/* PerspectivePlayback */ 1, [Replay_readPerspective(limits, reader)])) : Replay_failDecode(toText(printf("Invalid disclosure byte %d."))(disclosure)));
            if (reader.Offset !== bytes.length) {
                Replay_failDecode("Trailing bytes are not permitted.");
            }
            return new FSharpResult$2(/* Ok */ 0, [new ReplayPackage(version, engineHash, rulesetHash, fullReplayAuthorized, rulesArchive, content)]);
        }
        catch (matchValue_1) {
            if (matchValue_1 instanceof ReplayDecodeFailure) {
                return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* MalformedPackage */ 1, [matchValue_1.Data0])]);
            }
            else {
                throw matchValue_1;
            }
        }
    }
}

function Replay_ordered(field, entries) {
    const keys = map((tupledArg) => [tupledArg[0], tupledArg[1]], entries);
    if (equals(keys, sort(keys, {
        Compare: (x, y) => (compareArrays(x, y) | 0),
    })) && (length(keys) === count_1(ofList(keys, {
        Compare: (x_1, y_1) => (compareArrays(x_1, y_1) | 0),
    })))) {
        return new FSharpResult$2(/* Ok */ 0, [undefined]);
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* InvalidOrdering */ 7, [field])]);
    }
}

function Replay_validateHeader(expectedEngine, package$) {
    if (((package$.FormatVersion !== 1) && (package$.FormatVersion !== 2)) && (package$.FormatVersion !== 3)) {
        return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* UnsupportedFormat */ 2, [package$.FormatVersion, 3])]);
    }
    else if (!equalsWith((x, y) => (x === y), package$.EngineHash, expectedEngine)) {
        return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* EngineMismatch */ 4, [expectedEngine, package$.EngineHash])]);
    }
    else if (((package$.FormatVersion >= 3) && (package$.RulesArchive == null)) && (!(package$.Content.tag === 1))) {
        return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* MalformedPackage */ 1, ["Replay v3 requires a rules archive."])]);
    }
    else if (((package$.FormatVersion >= 3) && (package$.RulesArchive != null)) && !equalsWith((x_1, y_1) => (x_1 === y_1), value_2(package$.RulesArchive).Identity.ManifestDigest, package$.RulesetHash)) {
        return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* MalformedPackage */ 1, ["Replay rules archive manifest identity does not match the package ruleset hash."])]);
    }
    else {
        const matchValue_1 = Replay_requireHash("engine", package$.EngineHash);
        if (matchValue_1.tag === 0) {
            return Replay_requireHash("ruleset", package$.RulesetHash);
        }
        else {
            return new FSharpResult$2(/* Error */ 1, [matchValue_1.fields[0]]);
        }
    }
}

function Replay_validateFull(formatVersion, limits, full) {
    const limit = (field, maximum, values) => {
        if (length(values) > maximum) {
            return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* ResourceLimitExceeded */ 6, [field, length(values), maximum])]);
        }
        else {
            return new FSharpResult$2(/* Ok */ 0, [undefined]);
        }
    };
    const matchValue = limit("inputs", limits.MaxInputs, full.OrderedInputs);
    if (matchValue.tag === 0) {
        const matchValue_1 = limit("accepted WASM outputs", limits.MaxWasmOutputs, full.AcceptedWasmOutputs);
        if (matchValue_1.tag === 0) {
            const matchValue_2 = limit("checkpoints", limits.MaxCheckpoints, full.Checkpoints);
            if (matchValue_2.tag === 0) {
                const matchValue_3 = Replay_ordered("inputs", map((input) => [input.Tick, input.Sequence], full.OrderedInputs));
                if (matchValue_3.tag === 0) {
                    const matchValue_4 = Replay_ordered("accepted WASM outputs", map((output) => [output.Tick, output.Sequence], full.AcceptedWasmOutputs));
                    if (matchValue_4.tag === 0) {
                        const combinedJournalKeys = append_1(map((input_1) => [input_1.Tick, input_1.Sequence], full.OrderedInputs), map((output_1) => [output_1.Tick, output_1.Sequence], full.AcceptedWasmOutputs));
                        const checkpointTicks = map((checkpoint) => (checkpoint.Tick | 0), full.Checkpoints);
                        if (length(combinedJournalKeys) !== count_1(ofList(combinedJournalKeys, {
                            Compare: (x, y) => (compareArrays(x, y) | 0),
                        }))) {
                            return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* InvalidOrdering */ 7, ["combined input journal"])]);
                        }
                        else if (exists_1((tupledArg) => {
                            const tick = tupledArg[0] | 0;
                            if (tick <= full.InitialSnapshot.Tick) {
                                return true;
                            }
                            else {
                                return tick > full.FinalResult.Tick;
                            }
                        }, combinedJournalKeys)) {
                            return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* InvalidOrdering */ 7, ["input ticks"])]);
                        }
                        else if (!equals(checkpointTicks, sort(checkpointTicks, {
                            Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
                        })) ? true : (length(checkpointTicks) !== count_1(ofList(checkpointTicks, {
                            Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
                        })))) {
                            return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* InvalidOrdering */ 7, ["checkpoints"])]);
                        }
                        else if (exists_1((tick_1) => {
                            if (tick_1 < full.InitialSnapshot.Tick) {
                                return true;
                            }
                            else {
                                return tick_1 > full.FinalResult.Tick;
                            }
                        }, checkpointTicks)) {
                            return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* InvalidOrdering */ 7, ["checkpoint ticks"])]);
                        }
                        else if (full.FinalResult.Tick < full.InitialSnapshot.Tick) {
                            return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* InvalidCheckpoint */ 8, [full.FinalResult.Tick, "Final tick precedes the initial snapshot."])]);
                        }
                        else if (exists_1((checkpoint_1) => (checkpoint_1.Tick !== checkpoint_1.State.Tick), full.Checkpoints)) {
                            return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* InvalidCheckpoint */ 8, [find((checkpoint_2) => (checkpoint_2.Tick !== checkpoint_2.State.Tick), full.Checkpoints).Tick, "Checkpoint tick does not match its snapshot."])]);
                        }
                        else if (exists_1((checkpoint_4) => !equalsWith((x_3, y_3) => (x_3 === y_3), checkpoint_4.StateHash, Replay_stateHashForVersion(formatVersion, checkpoint_4.State)), full.Checkpoints)) {
                            return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* InvalidCheckpoint */ 8, [find((checkpoint_5) => !equalsWith((x_4, y_4) => (x_4 === y_4), checkpoint_5.StateHash, Replay_stateHashForVersion(formatVersion, checkpoint_5.State)), full.Checkpoints).Tick, "Checkpoint state hash does not match its snapshot."])]);
                        }
                        else {
                            const snapshotLimitError = tryPick((state) => {
                                if (FSharpMap__get_Count(state.Units) > limits.MaxUnits) {
                                    return new ReplayError(/* ResourceLimitExceeded */ 6, ["units", FSharpMap__get_Count(state.Units), limits.MaxUnits]);
                                }
                                else if (length(state.Board.Edges) > limits.MaxEdges) {
                                    return new ReplayError(/* ResourceLimitExceeded */ 6, ["edges", length(state.Board.Edges), limits.MaxEdges]);
                                }
                                else if (FSharpSet__get_Count(state.Observations) > limits.MaxObservations) {
                                    return new ReplayError(/* ResourceLimitExceeded */ 6, ["observations", FSharpSet__get_Count(state.Observations), limits.MaxObservations]);
                                }
                                else {
                                    return undefined;
                                }
                            }, cons(full.InitialSnapshot, map((checkpoint_7) => checkpoint_7.State, full.Checkpoints)));
                            if (snapshotLimitError == null) {
                                const _arg_1 = tryPick((tupledArg_1) => {
                                    const matchValue_5 = Replay_requireHash(tupledArg_1[0], tupledArg_1[1]);
                                    if (matchValue_5.tag === 1) {
                                        return matchValue_5.fields[0];
                                    }
                                    else {
                                        return undefined;
                                    }
                                }, append_1(ofArray([["final state", full.FinalResult.StateHash], ["final events", full.FinalResult.EventHash]]), collect((checkpoint_8) => ofArray([["checkpoint state", checkpoint_8.StateHash], ["checkpoint events", checkpoint_8.EventHash]]), full.Checkpoints)));
                                if (_arg_1 == null) {
                                    return new FSharpResult$2(/* Ok */ 0, [undefined]);
                                }
                                else {
                                    return new FSharpResult$2(/* Error */ 1, [_arg_1]);
                                }
                            }
                            else {
                                return new FSharpResult$2(/* Error */ 1, [snapshotLimitError]);
                            }
                        }
                    }
                    else {
                        return new FSharpResult$2(/* Error */ 1, [matchValue_4.fields[0]]);
                    }
                }
                else {
                    return new FSharpResult$2(/* Error */ 1, [matchValue_3.fields[0]]);
                }
            }
            else {
                return new FSharpResult$2(/* Error */ 1, [matchValue_2.fields[0]]);
            }
        }
        else {
            return new FSharpResult$2(/* Error */ 1, [matchValue_1.fields[0]]);
        }
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [matchValue.fields[0]]);
    }
}

function Replay_validatePerspective(limits, frames) {
    if (length(frames) > limits.MaxPerspectiveFrames) {
        return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* ResourceLimitExceeded */ 6, ["perspective frames", length(frames), limits.MaxPerspectiveFrames])]);
    }
    else if (!equals(map((frame) => (frame.Tick | 0), frames), sort(map((frame_1) => (frame_1.Tick | 0), frames), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }))) {
        return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* InvalidOrdering */ 7, ["perspective frames"])]);
    }
    else {
        const _arg = tryPick((frame_2) => {
            const matchValue = Replay_requireHash("projection", frame_2.ProjectionHash);
            if (matchValue.tag === 1) {
                return matchValue.fields[0];
            }
            else {
                return undefined;
            }
        }, frames);
        if (_arg == null) {
            return new FSharpResult$2(/* Ok */ 0, [undefined]);
        }
        else {
            return new FSharpResult$2(/* Error */ 1, [_arg]);
        }
    }
}

function Replay_journalAt(tick, full) {
    return map((tuple_1) => tuple_1[1], sortBy((tuple) => (tuple[0] | 0), append_1(choose((input) => {
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

function Replay_checkpointAt(tick, full) {
    return tryFind((checkpoint) => (checkpoint.Tick === tick), full.Checkpoints);
}

function Replay_replayFrom(formatVersion, full, start) {
    let state = start;
    let lastEvents = empty_1();
    let failure = undefined;
    for (let tick = start.Tick + 1; tick <= full.FinalResult.Tick; tick++) {
        if (failure == null) {
            const result = Simulation_runTick(state, Replay_journalAt(tick, full));
            const actualStateHash = Replay_stateHashForVersion(formatVersion, result.State);
            const actualEventHash = Replay_eventHash(result.Events);
            const matchValue = Replay_checkpointAt(tick, full);
            let matchResult, checkpoint_3, checkpoint_4, checkpoint_5;
            if (matchValue != null) {
                if (!equalsWith((x, y) => (x === y), matchValue.StateHash, actualStateHash)) {
                    matchResult = 0;
                    checkpoint_3 = matchValue;
                }
                else if (!equalsWith((x_1, y_1) => (x_1 === y_1), matchValue.EventHash, actualEventHash)) {
                    matchResult = 1;
                    checkpoint_4 = matchValue;
                }
                else if (!equalsWith((x_2, y_2) => (x_2 === y_2), Replay_snapshotBytesForVersion(formatVersion, matchValue.State), Replay_snapshotBytesForVersion(formatVersion, result.State))) {
                    matchResult = 2;
                    checkpoint_5 = matchValue;
                }
                else {
                    matchResult = 3;
                }
            }
            else {
                matchResult = 3;
            }
            switch (matchResult) {
                case 0: {
                    failure = (new ReplayError(/* ReplayDivergence */ 9, [tick, "checkpoint state hash"]));
                    break;
                }
                case 1: {
                    failure = (new ReplayError(/* ReplayDivergence */ 9, [tick, "checkpoint event hash"]));
                    break;
                }
                case 2: {
                    failure = (new ReplayError(/* ReplayDivergence */ 9, [tick, "checkpoint snapshot"]));
                    break;
                }
                case 3: {
                    state = result.State;
                    lastEvents = result.Events;
                    break;
                }
            }
        }
    }
    if (failure == null) {
        const actualStateHash_1 = Replay_stateHashForVersion(formatVersion, state);
        const actualEventHash_1 = Replay_eventHash(lastEvents);
        if (!equalsWith((x_3, y_3) => (x_3 === y_3), actualStateHash_1, full.FinalResult.StateHash)) {
            return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* ReplayDivergence */ 9, [full.FinalResult.Tick, "final state hash"])]);
        }
        else if (!equalsWith((x_4, y_4) => (x_4 === y_4), actualEventHash_1, full.FinalResult.EventHash)) {
            return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* ReplayDivergence */ 9, [full.FinalResult.Tick, "final event hash"])]);
        }
        else {
            return new FSharpResult$2(/* Ok */ 0, [full.FinalResult]);
        }
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [failure]);
    }
}

function Replay_verifyAllSeekPoints(formatVersion, full) {
    const _arg = tryPick((state) => {
        const matchValue = Replay_replayFrom(formatVersion, full, state);
        if (matchValue.tag === 1) {
            return matchValue.fields[0];
        }
        else {
            return undefined;
        }
    }, cons(full.InitialSnapshot, map((checkpoint_1) => checkpoint_1.State, filter((checkpoint) => (checkpoint.Tick < full.FinalResult.Tick), full.Checkpoints))));
    if (_arg == null) {
        return new FSharpResult$2(/* Ok */ 0, [full.FinalResult]);
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [_arg]);
    }
}

/**
 * Runs the shared kernel from the initial snapshot and every retained checkpoint.
 */
export function Replay_runKernelReplay(limits, expectedEngine, package$) {
    const matchValue = Replay_validateHeader(expectedEngine, package$);
    if (matchValue.tag === 0) {
        const matchValue_1 = package$.Content;
        if (matchValue_1.tag === 0) {
            if (!package$.FullReplayAuthorized) {
                return new FSharpResult$2(/* Error */ 1, [ReplayError.UnauthorizedFullReplay]);
            }
            else {
                const matchValue_3 = Replay_validateFull(package$.FormatVersion, limits, matchValue_1.fields[0]);
                if (matchValue_3.tag === 0) {
                    const matchValue_4 = Replay_verifyAllSeekPoints(package$.FormatVersion, matchValue_1.fields[0]);
                    if (matchValue_4.tag === 1) {
                        return new FSharpResult$2(/* Error */ 1, [matchValue_4.fields[0]]);
                    }
                    else {
                        return new FSharpResult$2(/* Ok */ 0, [new ReplayVerification(/* BrowserKernelVerified */ 0, [matchValue_4.fields[0]])]);
                    }
                }
                else {
                    return new FSharpResult$2(/* Error */ 1, [matchValue_3.fields[0]]);
                }
            }
        }
        else {
            const matchValue_2 = Replay_validatePerspective(limits, matchValue_1.fields[0]);
            if (matchValue_2.tag === 1) {
                return new FSharpResult$2(/* Error */ 1, [matchValue_2.fields[0]]);
            }
            else {
                return new FSharpResult$2(/* Ok */ 0, [new ReplayVerification(/* PerspectiveReady */ 2, [matchValue_1.fields[0]])]);
            }
        }
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [matchValue.fields[0]]);
    }
}

function Replay_firstWasmDifference(expected, actual) {
    const compareEntries = (expectedEntries_mut, actualEntries_mut) => {
        compareEntries:
        while (true) {
            const expectedEntries = expectedEntries_mut, actualEntries = actualEntries_mut;
            if (!isEmpty(expectedEntries)) {
                if (isEmpty(actualEntries)) {
                    return [head(expectedEntries).Tick, head(expectedEntries).Sequence];
                }
                else if (equals(head(expectedEntries), head(actualEntries))) {
                    expectedEntries_mut = tail(expectedEntries);
                    actualEntries_mut = tail(actualEntries);
                    continue compareEntries;
                }
                else {
                    return [min(head(expectedEntries).Tick, head(actualEntries).Tick), min(head(expectedEntries).Sequence, head(actualEntries).Sequence)];
                }
            }
            else if (!isEmpty(actualEntries)) {
                return [head(actualEntries).Tick, head(actualEntries).Sequence];
            }
            else {
                return undefined;
            }
            break;
        }
    };
    return compareEntries(expected, actual);
}

/**
 * Adds the stronger authoritative claim only when exact WASM re-execution
 * reproduces the complete accepted-output journal byte for byte.
 */
export function Replay_verifyAuthoritative(limits, expectedEngine, reexecutedWasmOutputs, package$) {
    const matchValue = package$.Content;
    if (matchValue.tag === 0) {
        if (reexecutedWasmOutputs != null) {
            const matchValue_1 = Replay_firstWasmDifference(matchValue.fields[0].AcceptedWasmOutputs, reexecutedWasmOutputs);
            if (matchValue_1 == null) {
                const matchValue_2 = Replay_runKernelReplay(limits, expectedEngine, package$);
                if (matchValue_2.tag === 1) {
                    return new FSharpResult$2(/* Error */ 1, [matchValue_2.fields[0]]);
                }
                else if (matchValue_2.fields[0].tag === 0) {
                    return new FSharpResult$2(/* Ok */ 0, [new ReplayVerification(/* AuthoritativeVerified */ 1, [matchValue_2.fields[0].fields[0]])]);
                }
                else {
                    return new FSharpResult$2(/* Error */ 1, [ReplayError.PerspectiveHasNoKernel]);
                }
            }
            else {
                return new FSharpResult$2(/* Error */ 1, [new ReplayError(/* WasmOutputDivergence */ 12, [matchValue_1[0], matchValue_1[1]])]);
            }
        }
        else {
            return new FSharpResult$2(/* Error */ 1, [ReplayError.WasmExecutionNotVerified]);
        }
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [ReplayError.PerspectiveHasNoKernel]);
    }
}

/**
 * Explicitly rejects attempts to obtain kernel verification from perspective data.
 */
export function Replay_requireKernel(package$) {
    const matchValue = package$.Content;
    if (matchValue.tag === 0) {
        if (!package$.FullReplayAuthorized) {
            return new FSharpResult$2(/* Error */ 1, [ReplayError.UnauthorizedFullReplay]);
        }
        else {
            return new FSharpResult$2(/* Ok */ 0, [matchValue.fields[0]]);
        }
    }
    else {
        return new FSharpResult$2(/* Error */ 1, [ReplayError.PerspectiveHasNoKernel]);
    }
}

export const Replay_emptyEventHash = Replay_eventHash(empty_1());

