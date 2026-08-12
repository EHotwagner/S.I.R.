
import { toString, Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { list_type, int64_type, option_type, record_type, string_type, array_type, tuple_type, int32_type, union_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { Direction8Module_toCode, Direction8_$reflection } from "../SIR.Domain/Orientation.js";
import { SimulatorPlanTransport, SimulatorPreviewLabel, SimulatorCorrelation, SimulatorCorrelation_$reflection, SimulatorPreviewLabel_$reflection } from "./SimulatorWorkerProtocol.js";
import { isNullOrEmpty, substring, format, join } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { sortBy as sortBy_2, item, contains, tryFind as tryFind_1, append as append_1, tryHead as tryHead_1, map } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { int64ToString, stringHash, comparePrimitives, equals, compareArrays, int32ToString } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { ofArray, toArray as toArray_2, tail, head, tryFindBack, tryFind, find, exists as exists_1, singleton, append, tryHead, filter, empty, cons, isEmpty, sortBy, map as map_1 } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { sha256 } from "../SIR.Domain/CanonicalHash.js";
import { get_UTF8 } from "../fable_modules/fable-library-js.5.13.0/Encoding.js";
import { equals as equals_1, op_Addition, toInt64_unchecked } from "../fable_modules/fable-library-js.5.13.0/BigInt.js";
import { sortBy as sortBy_1, map as map_2, toArray as toArray_1, exists } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { orElse, defaultArg, toArray } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { HumanCapabilities_defaultLoadout } from "../SIR.Domain/HumanCapabilities.js";
import { max, min } from "../fable_modules/fable-library-js.5.13.0/Double.js";

export class PlanningTool extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["RouteTool", "FacingTool", "AttentionTool", "StanceTool", "HoldTool", "EngagementTool", "SynchronizationTool"];
    }
    static RouteTool = new PlanningTool(0, []);
    static FacingTool = new PlanningTool(1, []);
    static AttentionTool = new PlanningTool(2, []);
    static StanceTool = new PlanningTool(3, []);
    static HoldTool = new PlanningTool(4, []);
    static EngagementTool = new PlanningTool(5, []);
    static SynchronizationTool = new PlanningTool(6, []);
}

export function PlanningTool_$reflection() {
    return union_type("SIR.Client.PlanningTool", [], PlanningTool, () => [[], [], [], [], [], [], []]);
}

export class PlanningCommandKind extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["PlannedRoute", "PlannedFacing", "PlannedAttention", "PlannedStance", "PlannedHold", "PlannedEngagement", "PlannedSynchronization"];
    }
    static PlannedHold = new PlanningCommandKind(4, []);
}

export function PlanningCommandKind_$reflection() {
    return union_type("SIR.Client.PlanningCommandKind", [], PlanningCommandKind, () => [[["Item", array_type(tuple_type(int32_type, int32_type))]], [["Item", Direction8_$reflection()]], [["Item", Direction8_$reflection()]], [["Item", string_type]], [], [["targetUnitId", int32_type], ["capabilityId", string_type]], [["marker", string_type], ["deadlineTick", int32_type]]]);
}

export class PlanningCommand extends Record {
    constructor(Id, UnitId, EarliestTick, Kind) {
        super();
        this.Id = Id;
        this.UnitId = (UnitId | 0);
        this.EarliestTick = (EarliestTick | 0);
        this.Kind = Kind;
    }
}

export function PlanningCommand_$reflection() {
    return record_type("SIR.Client.PlanningCommand", [], PlanningCommand, () => [["Id", string_type], ["UnitId", int32_type], ["EarliestTick", int32_type], ["Kind", PlanningCommandKind_$reflection()]]);
}

export class PlanningRosterMember extends Record {
    constructor(UnitId, Name, Side, Role, Equipment, CapabilityIds, Column, Row) {
        super();
        this.UnitId = (UnitId | 0);
        this.Name = Name;
        this.Side = Side;
        this.Role = Role;
        this.Equipment = Equipment;
        this.CapabilityIds = CapabilityIds;
        this.Column = (Column | 0);
        this.Row = (Row | 0);
    }
}

export function PlanningRosterMember_$reflection() {
    return record_type("SIR.Client.PlanningRosterMember", [], PlanningRosterMember, () => [["UnitId", int32_type], ["Name", string_type], ["Side", string_type], ["Role", string_type], ["Equipment", array_type(string_type)], ["CapabilityIds", array_type(string_type)], ["Column", int32_type], ["Row", int32_type]]);
}

export class PlanningIssue extends Record {
    constructor(Code, CommandId, UnitId, Detail) {
        super();
        this.Code = Code;
        this.CommandId = CommandId;
        this.UnitId = UnitId;
        this.Detail = Detail;
    }
}

export function PlanningIssue_$reflection() {
    return record_type("SIR.Client.PlanningIssue", [], PlanningIssue, () => [["Code", string_type], ["CommandId", option_type(string_type)], ["UnitId", option_type(int32_type)], ["Detail", string_type]]);
}

export class PlanningPreview extends Record {
    constructor(Revision, Label, Disclosures) {
        super();
        this.Revision = Revision;
        this.Label = Label;
        this.Disclosures = Disclosures;
    }
}

export function PlanningPreview_$reflection() {
    return record_type("SIR.Client.PlanningPreview", [], PlanningPreview, () => [["Revision", int64_type], ["Label", SimulatorPreviewLabel_$reflection()], ["Disclosures", array_type(string_type)]]);
}

export class PlanningRequestKind extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["InitializePlanningRequest", "PreviewPlanningRequest", "ValidatePlanningRequest", "CommitPlanningRequest"];
    }
    static InitializePlanningRequest = new PlanningRequestKind(0, []);
    static PreviewPlanningRequest = new PlanningRequestKind(1, []);
    static ValidatePlanningRequest = new PlanningRequestKind(2, []);
    static CommitPlanningRequest = new PlanningRequestKind(3, []);
}

export function PlanningRequestKind_$reflection() {
    return union_type("SIR.Client.PlanningRequestKind", [], PlanningRequestKind, () => [[], [], [], []]);
}

export class PendingPlanningRequest extends Record {
    constructor(Kind, Correlation) {
        super();
        this.Kind = Kind;
        this.Correlation = Correlation;
    }
}

export function PendingPlanningRequest_$reflection() {
    return record_type("SIR.Client.PendingPlanningRequest", [], PendingPlanningRequest, () => [["Kind", PlanningRequestKind_$reflection()], ["Correlation", SimulatorCorrelation_$reflection()]]);
}

export class PlanningSnapshot extends Record {
    constructor(Commands, Revision, Digest) {
        super();
        this.Commands = Commands;
        this.Revision = Revision;
        this.Digest = Digest;
    }
}

export function PlanningSnapshot_$reflection() {
    return record_type("SIR.Client.PlanningSnapshot", [], PlanningSnapshot, () => [["Commands", list_type(PlanningCommand_$reflection())], ["Revision", int64_type], ["Digest", string_type]]);
}

export class PlanningWorkspaceState extends Record {
    constructor(SessionId, MapRevision, Roster, SelectedUnit, SelectedCommand, AuthoringTick, Tool, Commands, Revision, NextRevision, Digest, Past, Future, NextCommand, NextOperation, PendingRequest, Issues, FocusedIssue, Predicted, AcceptedRevision, CommittedRevision, CommittedTick, WorkerStatus) {
        super();
        this.SessionId = SessionId;
        this.MapRevision = MapRevision;
        this.Roster = Roster;
        this.SelectedUnit = SelectedUnit;
        this.SelectedCommand = SelectedCommand;
        this.AuthoringTick = (AuthoringTick | 0);
        this.Tool = Tool;
        this.Commands = Commands;
        this.Revision = Revision;
        this.NextRevision = NextRevision;
        this.Digest = Digest;
        this.Past = Past;
        this.Future = Future;
        this.NextCommand = (NextCommand | 0);
        this.NextOperation = (NextOperation | 0);
        this.PendingRequest = PendingRequest;
        this.Issues = Issues;
        this.FocusedIssue = FocusedIssue;
        this.Predicted = Predicted;
        this.AcceptedRevision = AcceptedRevision;
        this.CommittedRevision = CommittedRevision;
        this.CommittedTick = CommittedTick;
        this.WorkerStatus = WorkerStatus;
    }
}

export function PlanningWorkspaceState_$reflection() {
    return record_type("SIR.Client.PlanningWorkspaceState", [], PlanningWorkspaceState, () => [["SessionId", string_type], ["MapRevision", string_type], ["Roster", array_type(PlanningRosterMember_$reflection())], ["SelectedUnit", option_type(int32_type)], ["SelectedCommand", option_type(string_type)], ["AuthoringTick", int32_type], ["Tool", PlanningTool_$reflection()], ["Commands", list_type(PlanningCommand_$reflection())], ["Revision", int64_type], ["NextRevision", int64_type], ["Digest", string_type], ["Past", list_type(PlanningSnapshot_$reflection())], ["Future", list_type(PlanningSnapshot_$reflection())], ["NextCommand", int32_type], ["NextOperation", int32_type], ["PendingRequest", option_type(PendingPlanningRequest_$reflection())], ["Issues", array_type(PlanningIssue_$reflection())], ["FocusedIssue", option_type(int32_type)], ["Predicted", option_type(PlanningPreview_$reflection())], ["AcceptedRevision", option_type(int64_type)], ["CommittedRevision", option_type(int64_type)], ["CommittedTick", option_type(int32_type)], ["WorkerStatus", string_type]]);
}

export class PlanningAction extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["SelectPlanningUnit", "SelectPlanningCommand", "SetPlanningAuthoringTick", "MoveSelectedPlanningCommandTo", "ChoosePlanningTool", "AddRouteWaypoint", "SetPlanningFacing", "SetPlanningAttention", "SetPlanningStance", "AddPlanningHold", "AddPlanningEngagement", "AddPlanningSynchronization", "RemoveSelectedPlanningCommand", "UndoPlanning", "RedoPlanning", "FocusPlanningIssue"];
    }
    static AddPlanningHold = new PlanningAction(9, []);
    static RemoveSelectedPlanningCommand = new PlanningAction(12, []);
    static UndoPlanning = new PlanningAction(13, []);
    static RedoPlanning = new PlanningAction(14, []);
}

export function PlanningAction_$reflection() {
    return union_type("SIR.Client.PlanningAction", [], PlanningAction, () => [[["Item", int32_type]], [["Item", string_type]], [["Item", int32_type]], [["Item", int32_type]], [["Item", PlanningTool_$reflection()]], [["column", int32_type], ["row", int32_type]], [["Item", Direction8_$reflection()]], [["Item", Direction8_$reflection()]], [["Item", string_type]], [], [["targetUnitId", int32_type], ["capabilityId", string_type]], [["marker", string_type], ["deadlineTick", int32_type]], [], [], [], [["Item", int32_type]]]);
}

function PlanningWorkspace_hex(bytes) {
    return join("", map((value) => format('{0:' + "x2" + '}', value), bytes));
}

function PlanningWorkspace_direction(direction) {
    return Direction8Module_toCode(direction).toString();
}

function PlanningWorkspace_commandText(command) {
    let kind;
    const matchValue = command.Kind;
    kind = ((matchValue.tag === 1) ? ("facing:" + PlanningWorkspace_direction(matchValue.fields[0])) : ((matchValue.tag === 2) ? ("attention:" + PlanningWorkspace_direction(matchValue.fields[0])) : ((matchValue.tag === 3) ? ("stance:" + matchValue.fields[0]) : ((matchValue.tag === 4) ? "hold" : ((matchValue.tag === 5) ? ((("engage:" + int32ToString(matchValue.fields[0])) + ":") + matchValue.fields[1]) : ((matchValue.tag === 6) ? ((("sync:" + matchValue.fields[0]) + ":") + int32ToString(matchValue.fields[1])) : ("route:" + join(";", map((tupledArg) => ((int32ToString(tupledArg[0]) + ",") + int32ToString(tupledArg[1])), matchValue.fields[0])))))))));
    return join("|", [command.Id, int32ToString(command.UnitId), int32ToString(command.EarliestTick), kind]);
}

export function PlanningWorkspace_canonicalText(commands) {
    const lines = map_1(PlanningWorkspace_commandText, sortBy((command) => [command.UnitId, command.EarliestTick, command.Id], commands, {
        Compare: (x, y) => (compareArrays(x, y) | 0),
    }));
    return join("\n", lines) + (isEmpty(lines) ? "" : "\n");
}

export function PlanningWorkspace_digest(commands) {
    let chars, objectArg;
    return PlanningWorkspace_hex(sha256((chars = PlanningWorkspace_canonicalText(commands), (objectArg = get_UTF8(), objectArg.getBytes(chars)))));
}

function PlanningWorkspace_snapshot(state) {
    return new PlanningSnapshot(state.Commands, state.Revision, state.Digest);
}

function PlanningWorkspace_edit(command, state) {
    const commands = command(state.Commands);
    if (equals(commands, state.Commands)) {
        return state;
    }
    else {
        return new PlanningWorkspaceState(state.SessionId, state.MapRevision, state.Roster, state.SelectedUnit, state.SelectedCommand, state.AuthoringTick, state.Tool, commands, state.NextRevision, toInt64_unchecked(op_Addition(state.NextRevision, 1n)), PlanningWorkspace_digest(commands), cons(PlanningWorkspace_snapshot(state), state.Past), empty(), state.NextCommand, state.NextOperation, undefined, [], undefined, undefined, undefined, state.CommittedRevision, state.CommittedTick, state.WorkerStatus);
    }
}

function PlanningWorkspace_selected(state) {
    return state.SelectedUnit;
}

function PlanningWorkspace_committedCommands(boundary, commands) {
    return filter((command) => (command.EarliestTick <= boundary), commands);
}

function PlanningWorkspace_preservesCommittedHistory(state, candidateSnapshot) {
    const matchValue = state.CommittedTick;
    if (matchValue != null) {
        const boundary = matchValue | 0;
        return equals(PlanningWorkspace_committedCommands(boundary, candidateSnapshot.Commands), PlanningWorkspace_committedCommands(boundary, state.Commands));
    }
    else {
        return true;
    }
}

export function PlanningWorkspace_canUndo(state) {
    return exists((candidateSnapshot) => PlanningWorkspace_preservesCommittedHistory(state, candidateSnapshot), toArray(tryHead(state.Past)));
}

export function PlanningWorkspace_canRedo(state) {
    return exists((candidateSnapshot) => PlanningWorkspace_preservesCommittedHistory(state, candidateSnapshot), toArray(tryHead(state.Future)));
}

function PlanningWorkspace_append(kind, state) {
    const matchValue = PlanningWorkspace_selected(state);
    if (matchValue != null) {
        if (exists((tick) => (state.AuthoringTick <= tick), toArray(state.CommittedTick))) {
            return state;
        }
        else {
            const unitId = matchValue | 0;
            const id = "command-" + format('{0:' + "D4" + '}', state.NextCommand);
            const bind$0040 = PlanningWorkspace_edit((commands) => append(commands, singleton(new PlanningCommand(id, unitId, state.AuthoringTick, kind))), state);
            return new PlanningWorkspaceState(bind$0040.SessionId, bind$0040.MapRevision, bind$0040.Roster, bind$0040.SelectedUnit, id, bind$0040.AuthoringTick, bind$0040.Tool, bind$0040.Commands, bind$0040.Revision, bind$0040.NextRevision, bind$0040.Digest, bind$0040.Past, bind$0040.Future, state.NextCommand + 1, bind$0040.NextOperation, bind$0040.PendingRequest, bind$0040.Issues, bind$0040.FocusedIssue, bind$0040.Predicted, bind$0040.AcceptedRevision, bind$0040.CommittedRevision, bind$0040.CommittedTick, bind$0040.WorkerStatus);
        }
    }
    else {
        return state;
    }
}

export function PlanningWorkspace_initial(mapRevision, units) {
    let option_1;
    const roster = toArray_1(map_2((unit) => {
        const loadout = HumanCapabilities_defaultLoadout(unit.Id, unit.ClassId);
        return new PlanningRosterMember(unit.Id, (unit.ClassId + " ") + int32ToString(unit.Id), toString(unit.Side), loadout.Role, loadout.Equipment, loadout.CapabilityIds, unit.Column, unit.Row);
    }, sortBy_1((_arg) => (_arg.Id | 0), units, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    })));
    return new PlanningWorkspaceState("planning-" + substring(mapRevision, 0, min(12, mapRevision.length)), mapRevision, roster, (option_1 = tryHead_1(roster), (option_1 != null) ? option_1.UnitId : undefined), undefined, 0, PlanningTool.RouteTool, empty(), 0n, 1n, PlanningWorkspace_digest(empty()), empty(), empty(), 1, 1, undefined, [], undefined, undefined, undefined, undefined, undefined, "Not connected");
}

export function PlanningWorkspace_update(action, state) {
    let command_3, route, id_1, option_7, command_9, option_5, index;
    switch (action.tag) {
        case 1:
            if (exists_1((command) => (command.Id === action.fields[0]), state.Commands)) {
                return new PlanningWorkspaceState(state.SessionId, state.MapRevision, state.Roster, find((value) => (value.Id === action.fields[0]), state.Commands).UnitId, action.fields[0], state.AuthoringTick, state.Tool, state.Commands, state.Revision, state.NextRevision, state.Digest, state.Past, state.Future, state.NextCommand, state.NextOperation, state.PendingRequest, state.Issues, state.FocusedIssue, state.Predicted, state.AcceptedRevision, state.CommittedRevision, state.CommittedTick, state.WorkerStatus);
            }
            else {
                return state;
            }
        case 2:
            return new PlanningWorkspaceState(state.SessionId, state.MapRevision, state.Roster, state.SelectedUnit, state.SelectedCommand, max(0, action.fields[0]), state.Tool, state.Commands, state.Revision, state.NextRevision, state.Digest, state.Past, state.Future, state.NextCommand, state.NextOperation, state.PendingRequest, state.Issues, state.FocusedIssue, state.Predicted, state.AcceptedRevision, state.CommittedRevision, state.CommittedTick, state.WorkerStatus);
        case 3: {
            const matchValue = state.SelectedCommand;
            if (matchValue == null) {
                return state;
            }
            else {
                const id = matchValue;
                const matchValue_1 = tryFind((command_2) => (command_2.Id === id), state.Commands);
                if (matchValue_1 == null) {
                    return state;
                }
                else if ((command_3 = matchValue_1, exists((boundary) => {
                    if (command_3.EarliestTick <= boundary) {
                        return true;
                    }
                    else {
                        return action.fields[0] <= boundary;
                    }
                }, toArray(state.CommittedTick)))) {
                    const command_4 = matchValue_1;
                    return state;
                }
                else {
                    return PlanningWorkspace_edit((list_3) => map_1((command_5) => {
                        if (command_5.Id === id) {
                            return new PlanningCommand(command_5.Id, command_5.UnitId, max(0, action.fields[0]), command_5.Kind);
                        }
                        else {
                            return command_5;
                        }
                    }, list_3), state);
                }
            }
        }
        case 4:
            return new PlanningWorkspaceState(state.SessionId, state.MapRevision, state.Roster, state.SelectedUnit, state.SelectedCommand, state.AuthoringTick, action.fields[0], state.Commands, state.Revision, state.NextRevision, state.Digest, state.Past, state.Future, state.NextCommand, state.NextOperation, state.PendingRequest, state.Issues, state.FocusedIssue, state.Predicted, state.AcceptedRevision, state.CommittedRevision, state.CommittedTick, state.WorkerStatus);
        case 5: {
            const matchValue_2 = state.SelectedUnit;
            if (matchValue_2 != null) {
                const unitId_2 = matchValue_2 | 0;
                const existing = tryFindBack((command_6) => {
                    if (command_6.UnitId === unitId_2) {
                        if (command_6.Kind.tag === 0) {
                            return true;
                        }
                        else {
                            return false;
                        }
                    }
                    else {
                        return false;
                    }
                }, state.Commands);
                if (existing == null) {
                    return PlanningWorkspace_append(new PlanningCommandKind(/* PlannedRoute */ 0, [[[action.fields[0], action.fields[1]]]]), state);
                }
                else if ((route = existing, exists((tick_2) => (route.EarliestTick <= tick_2), toArray(state.CommittedTick)))) {
                    const route_1 = existing;
                    return state;
                }
                else {
                    const route_2 = existing;
                    return PlanningWorkspace_edit((list_5) => map_1((command_7) => {
                        if (command_7.Id === route_2.Id) {
                            const matchValue_4 = command_7.Kind;
                            if (matchValue_4.tag === 0) {
                                return new PlanningCommand(command_7.Id, command_7.UnitId, command_7.EarliestTick, new PlanningCommandKind(/* PlannedRoute */ 0, [append_1(matchValue_4.fields[0], [[action.fields[0], action.fields[1]]])]));
                            }
                            else {
                                return command_7;
                            }
                        }
                        else {
                            return command_7;
                        }
                    }, list_5), state);
                }
            }
            else {
                return state;
            }
        }
        case 6:
            return PlanningWorkspace_append(new PlanningCommandKind(/* PlannedFacing */ 1, [action.fields[0]]), state);
        case 7:
            return PlanningWorkspace_append(new PlanningCommandKind(/* PlannedAttention */ 2, [action.fields[0]]), state);
        case 8:
            return PlanningWorkspace_append(new PlanningCommandKind(/* PlannedStance */ 3, [action.fields[0]]), state);
        case 9:
            return PlanningWorkspace_append(PlanningCommandKind.PlannedHold, state);
        case 10: {
            const updated = PlanningWorkspace_append(new PlanningCommandKind(/* PlannedEngagement */ 5, [action.fields[0], action.fields[1]]), state);
            let matchValue_5;
            const option_3 = state.SelectedUnit;
            if (option_3 != null) {
                const unitId_3 = option_3 | 0;
                matchValue_5 = tryFind_1((unit_1) => (unit_1.UnitId === unitId_3), state.Roster);
            }
            else {
                matchValue_5 = undefined;
            }
            let matchResult, unit_3;
            if (matchValue_5 != null) {
                if (!contains(action.fields[1], matchValue_5.CapabilityIds, {
                    Equals: (x, y) => (x === y),
                    GetHashCode: (x) => (stringHash(x) | 0),
                })) {
                    matchResult = 0;
                    unit_3 = matchValue_5;
                }
                else {
                    matchResult = 1;
                }
            }
            else {
                matchResult = 1;
            }
            switch (matchResult) {
                case 0:
                    return new PlanningWorkspaceState(updated.SessionId, updated.MapRevision, updated.Roster, updated.SelectedUnit, updated.SelectedCommand, updated.AuthoringTick, updated.Tool, updated.Commands, updated.Revision, updated.NextRevision, updated.Digest, updated.Past, updated.Future, updated.NextCommand, updated.NextOperation, updated.PendingRequest, [new PlanningIssue("SIR.PLAN.CAPABILITY.NOT_IN_LOADOUT", updated.SelectedCommand, unit_3.UnitId, ((action.fields[1] + " is not present in ") + unit_3.Name) + "\'s explicit loadout.")], 0, updated.Predicted, updated.AcceptedRevision, updated.CommittedRevision, updated.CommittedTick, updated.WorkerStatus);
                default:
                    return updated;
            }
        }
        case 11:
            return PlanningWorkspace_append(new PlanningCommandKind(/* PlannedSynchronization */ 6, [action.fields[0], action.fields[1]]), state);
        case 12: {
            const matchValue_6 = state.SelectedCommand;
            if (matchValue_6 == null) {
                return state;
            }
            else if ((id_1 = matchValue_6, defaultArg((option_7 = tryFind((command_8) => (command_8.Id === id_1), state.Commands), (option_7 != null) ? ((command_9 = option_7, (option_5 = state.CommittedTick, (option_5 != null) ? (command_9.EarliestTick <= option_5) : undefined))) : undefined), false))) {
                const id_2 = matchValue_6;
                return state;
            }
            else {
                const id_3 = matchValue_6;
                const bind$0040 = PlanningWorkspace_edit((list_7) => filter((command_10) => (command_10.Id !== id_3), list_7), state);
                return new PlanningWorkspaceState(bind$0040.SessionId, bind$0040.MapRevision, bind$0040.Roster, bind$0040.SelectedUnit, undefined, bind$0040.AuthoringTick, bind$0040.Tool, bind$0040.Commands, bind$0040.Revision, bind$0040.NextRevision, bind$0040.Digest, bind$0040.Past, bind$0040.Future, bind$0040.NextCommand, bind$0040.NextOperation, bind$0040.PendingRequest, bind$0040.Issues, bind$0040.FocusedIssue, bind$0040.Predicted, bind$0040.AcceptedRevision, bind$0040.CommittedRevision, bind$0040.CommittedTick, bind$0040.WorkerStatus);
            }
        }
        case 13: {
            const matchValue_7 = state.Past;
            let matchResult_1, previous_1, remaining_1;
            if (!isEmpty(matchValue_7)) {
                if (PlanningWorkspace_preservesCommittedHistory(state, head(matchValue_7))) {
                    matchResult_1 = 0;
                    previous_1 = head(matchValue_7);
                    remaining_1 = tail(matchValue_7);
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
                    return new PlanningWorkspaceState(state.SessionId, state.MapRevision, state.Roster, state.SelectedUnit, undefined, state.AuthoringTick, state.Tool, previous_1.Commands, previous_1.Revision, state.NextRevision, previous_1.Digest, remaining_1, cons(PlanningWorkspace_snapshot(state), state.Future), state.NextCommand, state.NextOperation, undefined, state.Issues, state.FocusedIssue, undefined, undefined, state.CommittedRevision, state.CommittedTick, state.WorkerStatus);
                default:
                    return state;
            }
        }
        case 14: {
            const matchValue_8 = state.Future;
            let matchResult_2, next_1, remaining_3;
            if (!isEmpty(matchValue_8)) {
                if (PlanningWorkspace_preservesCommittedHistory(state, head(matchValue_8))) {
                    matchResult_2 = 0;
                    next_1 = head(matchValue_8);
                    remaining_3 = tail(matchValue_8);
                }
                else {
                    matchResult_2 = 1;
                }
            }
            else {
                matchResult_2 = 1;
            }
            switch (matchResult_2) {
                case 0:
                    return new PlanningWorkspaceState(state.SessionId, state.MapRevision, state.Roster, state.SelectedUnit, undefined, state.AuthoringTick, state.Tool, next_1.Commands, next_1.Revision, state.NextRevision, next_1.Digest, cons(PlanningWorkspace_snapshot(state), state.Past), remaining_3, state.NextCommand, state.NextOperation, undefined, state.Issues, state.FocusedIssue, undefined, undefined, state.CommittedRevision, state.CommittedTick, state.WorkerStatus);
                default:
                    return state;
            }
        }
        case 15:
            if ((index = (action.fields[0] | 0), (index >= 0) && (index < state.Issues.length))) {
                const issue = item(action.fields[0], state.Issues);
                return new PlanningWorkspaceState(state.SessionId, state.MapRevision, state.Roster, orElse(issue.UnitId, state.SelectedUnit), orElse(issue.CommandId, state.SelectedCommand), state.AuthoringTick, state.Tool, state.Commands, state.Revision, state.NextRevision, state.Digest, state.Past, state.Future, state.NextCommand, state.NextOperation, state.PendingRequest, state.Issues, action.fields[0], state.Predicted, state.AcceptedRevision, state.CommittedRevision, state.CommittedTick, state.WorkerStatus);
            }
            else {
                return state;
            }
        default:
            if (state.Roster.some((unit) => (unit.UnitId === action.fields[0]))) {
                return new PlanningWorkspaceState(state.SessionId, state.MapRevision, state.Roster, action.fields[0], undefined, state.AuthoringTick, state.Tool, state.Commands, state.Revision, state.NextRevision, state.Digest, state.Past, state.Future, state.NextCommand, state.NextOperation, state.PendingRequest, state.Issues, state.FocusedIssue, state.Predicted, state.AcceptedRevision, state.CommittedRevision, state.CommittedTick, state.WorkerStatus);
            }
            else {
                return state;
            }
    }
}

export function PlanningWorkspace_correlation(tick, state) {
    return new SimulatorCorrelation(state.NextOperation, state.SessionId, state.MapRevision, state.Revision, tick);
}

export function PlanningWorkspace_beginRequest(kind, tick, state) {
    const expected = PlanningWorkspace_correlation(tick, state);
    return [expected, new PlanningWorkspaceState(state.SessionId, state.MapRevision, state.Roster, state.SelectedUnit, state.SelectedCommand, state.AuthoringTick, state.Tool, state.Commands, state.Revision, state.NextRevision, state.Digest, state.Past, state.Future, state.NextCommand, state.NextOperation + 1, new PendingPlanningRequest(kind, expected), state.Issues, state.FocusedIssue, state.Predicted, state.AcceptedRevision, state.CommittedRevision, state.CommittedTick, state.WorkerStatus)];
}

export function PlanningWorkspace_planTransport(state) {
    const loadouts = join("\n", map((unit) => ((((((("loadout|" + int32ToString(unit.UnitId)) + "|") + unit.Role) + "|") + join(",", unit.Equipment)) + "|") + join(",", unit.CapabilityIds)), sortBy_2((_arg) => (_arg.UnitId | 0), state.Roster, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    })));
    const document$ = (((((("SIR-PLAN 1\nworkspace|" + state.Digest) + "|") + int64ToString(state.Revision)) + "\n") + loadouts) + (isNullOrEmpty(loadouts) ? "" : "\n")) + PlanningWorkspace_canonicalText(state.Commands);
    return new SimulatorPlanTransport(get_UTF8().getBytes(document$), 6000, SimulatorPreviewLabel.IntentOnlyPreview, [], toArray_2(map_1(PlanningWorkspace_commandText, state.Commands)));
}

export function PlanningWorkspace_receive(envelope, state) {
    let terminal;
    let matchValue;
    const option_1 = state.PendingRequest;
    matchValue = ((option_1 != null) ? option_1.Kind : undefined);
    const matchValue_1 = envelope.Response;
    let matchResult;
    if (matchValue != null) {
        switch (matchValue.tag) {
            case 1: {
                switch (matchValue_1.tag) {
                    case 2:
                    case 1:
                    case 9: {
                        matchResult = 1;
                        break;
                    }
                    default:
                        matchResult = 4;
                }
                break;
            }
            case 2: {
                switch (matchValue_1.tag) {
                    case 1:
                    case 9: {
                        matchResult = 2;
                        break;
                    }
                    default:
                        matchResult = 4;
                }
                break;
            }
            case 3: {
                switch (matchValue_1.tag) {
                    case 3:
                    case 9: {
                        matchResult = 3;
                        break;
                    }
                    default:
                        matchResult = 4;
                }
                break;
            }
            default:
                switch (matchValue_1.tag) {
                    case 0:
                    case 9: {
                        matchResult = 0;
                        break;
                    }
                    default:
                        matchResult = 4;
                }
        }
    }
    else {
        matchResult = 4;
    }
    switch (matchResult) {
        case 0: {
            terminal = true;
            break;
        }
        case 1: {
            terminal = true;
            break;
        }
        case 2: {
            terminal = true;
            break;
        }
        case 3: {
            terminal = true;
            break;
        }
        default:
            terminal = false;
    }
    const advanced = new PlanningWorkspaceState(state.SessionId, state.MapRevision, state.Roster, state.SelectedUnit, state.SelectedCommand, state.AuthoringTick, state.Tool, state.Commands, state.Revision, state.NextRevision, state.Digest, state.Past, state.Future, state.NextCommand, max(state.NextOperation, envelope.Correlation.Operation + 1), terminal ? undefined : state.PendingRequest, state.Issues, state.FocusedIssue, state.Predicted, state.AcceptedRevision, state.CommittedRevision, state.CommittedTick, "Worker responded at tick " + int32ToString(envelope.CurrentTick));
    const matchValue_3 = envelope.Response;
    switch (matchValue_3.tag) {
        case 1: {
            const diagnostics = matchValue_3.fields[1];
            return new PlanningWorkspaceState(advanced.SessionId, advanced.MapRevision, advanced.Roster, advanced.SelectedUnit, advanced.SelectedCommand, advanced.AuthoringTick, advanced.Tool, advanced.Commands, advanced.Revision, advanced.NextRevision, advanced.Digest, advanced.Past, advanced.Future, advanced.NextCommand, advanced.NextOperation, advanced.PendingRequest, map((issue) => (new PlanningIssue(issue.Code, issue.CommandId, undefined, issue.Detail)), diagnostics), undefined, advanced.Predicted, matchValue_3.fields[0], advanced.CommittedRevision, advanced.CommittedTick, (diagnostics.length === 0) ? "Revision accepted by worker validation" : (int32ToString(diagnostics.length) + " validation issues"));
        }
        case 2:
            return new PlanningWorkspaceState(advanced.SessionId, advanced.MapRevision, advanced.Roster, advanced.SelectedUnit, advanced.SelectedCommand, advanced.AuthoringTick, advanced.Tool, advanced.Commands, advanced.Revision, advanced.NextRevision, advanced.Digest, advanced.Past, advanced.Future, advanced.NextCommand, advanced.NextOperation, advanced.PendingRequest, advanced.Issues, advanced.FocusedIssue, new PlanningPreview(envelope.Correlation.PlanRevision, matchValue_3.fields[0], matchValue_3.fields[1]), advanced.AcceptedRevision, advanced.CommittedRevision, advanced.CommittedTick, "Intent-only prediction ready");
        case 3:
            return new PlanningWorkspaceState(advanced.SessionId, advanced.MapRevision, advanced.Roster, advanced.SelectedUnit, advanced.SelectedCommand, max(advanced.AuthoringTick, envelope.CurrentTick + 1), advanced.Tool, advanced.Commands, advanced.Revision, advanced.NextRevision, advanced.Digest, advanced.Past, advanced.Future, advanced.NextCommand, advanced.NextOperation, advanced.PendingRequest, advanced.Issues, advanced.FocusedIssue, advanced.Predicted, advanced.AcceptedRevision, matchValue_3.fields[0], envelope.CurrentTick, "Plan committed to simulator session");
        case 10:
            return new PlanningWorkspaceState(advanced.SessionId, advanced.MapRevision, advanced.Roster, advanced.SelectedUnit, advanced.SelectedCommand, advanced.AuthoringTick, advanced.Tool, advanced.Commands, advanced.Revision, advanced.NextRevision, advanced.Digest, advanced.Past, advanced.Future, advanced.NextCommand, advanced.NextOperation, advanced.PendingRequest, advanced.Issues, advanced.FocusedIssue, advanced.Predicted, advanced.AcceptedRevision, advanced.CommittedRevision, advanced.CommittedTick, "Qualified authoritative playback ready through tick " + int32ToString(matchValue_3.fields[2]));
        case 4:
        case 6:
        case 7:
            return new PlanningWorkspaceState(advanced.SessionId, advanced.MapRevision, advanced.Roster, advanced.SelectedUnit, advanced.SelectedCommand, advanced.AuthoringTick, advanced.Tool, advanced.Commands, advanced.Revision, advanced.NextRevision, advanced.Digest, advanced.Past, advanced.Future, advanced.NextCommand, advanced.NextOperation, advanced.PendingRequest, advanced.Issues, advanced.FocusedIssue, advanced.Predicted, advanced.AcceptedRevision, advanced.CommittedRevision, envelope.CurrentTick, advanced.WorkerStatus);
        case 5:
            return new PlanningWorkspaceState(advanced.SessionId, advanced.MapRevision, advanced.Roster, advanced.SelectedUnit, advanced.SelectedCommand, advanced.AuthoringTick, advanced.Tool, advanced.Commands, advanced.Revision, advanced.NextRevision, advanced.Digest, advanced.Past, advanced.Future, advanced.NextCommand, advanced.NextOperation, advanced.PendingRequest, advanced.Issues, advanced.FocusedIssue, advanced.Predicted, advanced.AcceptedRevision, advanced.CommittedRevision, envelope.CurrentTick, "Committed execution progressing");
        case 8:
            return new PlanningWorkspaceState(advanced.SessionId, advanced.MapRevision, advanced.Roster, advanced.SelectedUnit, advanced.SelectedCommand, advanced.AuthoringTick, advanced.Tool, advanced.Commands, advanced.Revision, advanced.NextRevision, advanced.Digest, advanced.Past, advanced.Future, advanced.NextCommand, advanced.NextOperation, advanced.PendingRequest, advanced.Issues, advanced.FocusedIssue, advanced.Predicted, advanced.AcceptedRevision, advanced.CommittedRevision, advanced.CommittedTick, "Worker operation cancelled");
        case 9:
            return new PlanningWorkspaceState(advanced.SessionId, advanced.MapRevision, advanced.Roster, advanced.SelectedUnit, advanced.SelectedCommand, advanced.AuthoringTick, advanced.Tool, advanced.Commands, advanced.Revision, advanced.NextRevision, advanced.Digest, advanced.Past, advanced.Future, advanced.NextCommand, advanced.NextOperation, advanced.PendingRequest, [new PlanningIssue(matchValue_3.fields[0], undefined, undefined, matchValue_3.fields[1])], 0, advanced.Predicted, advanced.AcceptedRevision, advanced.CommittedRevision, advanced.CommittedTick, "Worker rejected request");
        default:
            return new PlanningWorkspaceState(advanced.SessionId, advanced.MapRevision, advanced.Roster, advanced.SelectedUnit, advanced.SelectedCommand, advanced.AuthoringTick, advanced.Tool, advanced.Commands, advanced.Revision, advanced.NextRevision, advanced.Digest, advanced.Past, advanced.Future, advanced.NextCommand, advanced.NextOperation, advanced.PendingRequest, advanced.Issues, advanced.FocusedIssue, advanced.Predicted, advanced.AcceptedRevision, advanced.CommittedRevision, advanced.CommittedTick, "Planning worker ready");
    }
}

export function PlanningWorkspace_acceptsResponse(envelope, state) {
    if ((envelope.Kind === "sir-simulator-session") && (envelope.ProtocolVersion === 1)) {
        return exists((expected) => {
            if (((equals(envelope.Correlation, expected.Correlation) && (envelope.Correlation.Session === state.SessionId)) && (envelope.Correlation.MapRevision === state.MapRevision)) && equals_1(envelope.Correlation.PlanRevision, state.Revision)) {
                const matchValue = expected.Kind;
                const matchValue_1 = envelope.Response;
                let matchResult;
                switch (matchValue.tag) {
                    case 1: {
                        switch (matchValue_1.tag) {
                            case 2:
                            case 1:
                            case 9: {
                                matchResult = 0;
                                break;
                            }
                            default:
                                matchResult = 1;
                        }
                        break;
                    }
                    case 2: {
                        switch (matchValue_1.tag) {
                            case 1:
                            case 9: {
                                matchResult = 0;
                                break;
                            }
                            default:
                                matchResult = 1;
                        }
                        break;
                    }
                    case 3: {
                        switch (matchValue_1.tag) {
                            case 3:
                            case 5:
                            case 9: {
                                matchResult = 0;
                                break;
                            }
                            default:
                                matchResult = 1;
                        }
                        break;
                    }
                    default:
                        switch (matchValue_1.tag) {
                            case 0:
                            case 9: {
                                matchResult = 0;
                                break;
                            }
                            default:
                                matchResult = 1;
                        }
                }
                switch (matchResult) {
                    case 0:
                        return true;
                    default:
                        return false;
                }
            }
            else {
                return false;
            }
        }, toArray(state.PendingRequest));
    }
    else {
        return false;
    }
}

export function PlanningWorkspace_reviewArtifact(state) {
    let option_1, value, option_4, matchValue, matchValue_1, revision, tick;
    const loadouts = map((unit) => ((((((("loadout|" + int32ToString(unit.UnitId)) + "|") + unit.Role) + "|") + join(",", unit.Equipment)) + "|") + join(",", unit.CapabilityIds)), sortBy_2((_arg) => (_arg.UnitId | 0), state.Roster, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }));
    return join("\n", append(ofArray(["SIR-PLANNING-REVIEW 1", "map|" + state.MapRevision, (("authored|" + int64ToString(state.Revision)) + "|") + state.Digest, "predicted|" + defaultArg((option_1 = state.Predicted, (option_1 != null) ? ((value = option_1, (int64ToString(value.Revision) + "|") + toString(value.Label))) : undefined), "-"), "accepted|" + defaultArg((option_4 = state.AcceptedRevision, (option_4 != null) ? int64ToString(option_4) : undefined), "-"), "committed|" + ((matchValue = state.CommittedRevision, (matchValue_1 = state.CommittedTick, (matchValue != null) ? ((matchValue_1 != null) ? ((revision = matchValue, (tick = (matchValue_1 | 0), (int64ToString(revision) + "|") + int32ToString(tick)))) : "-") : "-"))), "conflicts|" + int32ToString(state.Issues.length)]), append(ofArray(loadouts), singleton(PlanningWorkspace_canonicalText(state.Commands))))) + "\n";
}

