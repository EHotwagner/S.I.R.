
import { toString, Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { array_type, uint8_type, list_type, bool_type, option_type, class_type, tuple_type, record_type, string_type, union_type, int32_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { Direction8Module_tryFromCode, Direction8Module_tryFromDelta, Direction8Module_toCode, Direction8_$reflection } from "../SIR.Domain/Orientation.js";
import { CapabilityInterruptionRule, HumanCapabilities_tryFind, AuthoredUnitLoadout_$reflection } from "../SIR.Domain/HumanCapabilities.js";
import { Request, Request_$reflection } from "../SIR.Domain/ControlAbiV1.js";
import { get_UTF8 } from "../fable_modules/fable-library-js.5.13.0/Encoding.js";
import { int32ToString, compareArrays, equals, stringHash, compare, Exception } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { contains, item, concat } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { int32LittleEndian } from "../SIR.Domain/CanonicalEncoding.js";
import { max, min } from "../fable_modules/fable-library-js.5.13.0/Double.js";
import { toList, add, remove, tryFind } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { Result_Map, FSharpResult$2 } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { truncate, zip, tryFindIndex, length as length_1, choose, map, collect, sortBy, iterate, empty, reverse, append, cons, singleton } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { value as value_5, defaultArg } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { sha256 } from "../SIR.Domain/CanonicalHash.js";
import { join } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { collect as collect_1, singleton as singleton_1, append as append_1, delay, toList as toList_1 } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { rangeDouble } from "../fable_modules/fable-library-js.5.13.0/Range.js";

export class CapabilityTarget extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["PointCapabilityTarget", "AreaCapabilityTarget"];
    }
}

export function CapabilityTarget_$reflection() {
    return union_type("SIR.Simulation.CapabilityTarget", [], CapabilityTarget, () => [[["unitId", int32_type]], [["referentId", int32_type]]]);
}

export class CapabilityEngagement extends Record {
    constructor(CapabilityId, Target, RequiredAttention, TraverseTicksRemaining, PreparationTicksRemaining, PreparedTicks) {
        super();
        this.CapabilityId = CapabilityId;
        this.Target = Target;
        this.RequiredAttention = RequiredAttention;
        this.TraverseTicksRemaining = (TraverseTicksRemaining | 0);
        this.PreparationTicksRemaining = (PreparationTicksRemaining | 0);
        this.PreparedTicks = (PreparedTicks | 0);
    }
}

export function CapabilityEngagement_$reflection() {
    return record_type("SIR.Simulation.CapabilityEngagement", [], CapabilityEngagement, () => [["CapabilityId", string_type], ["Target", CapabilityTarget_$reflection()], ["RequiredAttention", Direction8_$reflection()], ["TraverseTicksRemaining", int32_type], ["PreparationTicksRemaining", int32_type], ["PreparedTicks", int32_type]]);
}

export class CapabilityUnitState extends Record {
    constructor(Loadout, Cell, Attention, Ammunition, PreservedPreparation, Engagement) {
        super();
        this.Loadout = Loadout;
        this.Cell = Cell;
        this.Attention = Attention;
        this.Ammunition = Ammunition;
        this.PreservedPreparation = PreservedPreparation;
        this.Engagement = Engagement;
    }
}

export function CapabilityUnitState_$reflection() {
    return record_type("SIR.Simulation.CapabilityUnitState", [], CapabilityUnitState, () => [["Loadout", AuthoredUnitLoadout_$reflection()], ["Cell", tuple_type(int32_type, int32_type)], ["Attention", Direction8_$reflection()], ["Ammunition", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, int32_type])], ["PreservedPreparation", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, int32_type])], ["Engagement", option_type(CapabilityEngagement_$reflection())]]);
}

export class CapabilityExecutionState extends Record {
    constructor(Tick, Units, Areas) {
        super();
        this.Tick = (Tick | 0);
        this.Units = Units;
        this.Areas = Areas;
    }
}

export function CapabilityExecutionState_$reflection() {
    return record_type("SIR.Simulation.CapabilityExecutionState", [], CapabilityExecutionState, () => [["Tick", int32_type], ["Units", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, CapabilityUnitState_$reflection()])], ["Areas", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, tuple_type(int32_type, int32_type)])]]);
}

export class CapabilityEvent extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["CapabilityRejected", "CapabilityTraversing", "CapabilityPrepared", "PointEngagementResolved", "AreaEngagementResolved", "CapabilityInterrupted"];
    }
}

export function CapabilityEvent_$reflection() {
    return union_type("SIR.Simulation.CapabilityEvent", [], CapabilityEvent, () => [[["unitId", int32_type], ["capabilityId", string_type], ["reason", string_type]], [["unitId", int32_type], ["capabilityId", string_type], ["ticks", int32_type]], [["unitId", int32_type], ["capabilityId", string_type]], [["unitId", int32_type], ["targetUnitId", int32_type], ["capabilityId", string_type], ["ammunitionRemaining", int32_type]], [["unitId", int32_type], ["referentId", int32_type], ["capabilityId", string_type], ["ammunitionRemaining", int32_type]], [["unitId", int32_type], ["capabilityId", string_type], ["preparationPreserved", bool_type]]]);
}

export class CapabilityTickResult extends Record {
    constructor(State, Events) {
        super();
        this.State = State;
        this.Events = Events;
    }
}

export function CapabilityTickResult_$reflection() {
    return record_type("SIR.Simulation.CapabilityTickResult", [], CapabilityTickResult, () => [["State", CapabilityExecutionState_$reflection()], ["Events", list_type(CapabilityEvent_$reflection())]]);
}

export class CapabilityReplayRequest extends Record {
    constructor(Tick, UnitId, Request) {
        super();
        this.Tick = (Tick | 0);
        this.UnitId = (UnitId | 0);
        this.Request = Request;
    }
}

export function CapabilityReplayRequest_$reflection() {
    return record_type("SIR.Simulation.CapabilityReplayRequest", [], CapabilityReplayRequest, () => [["Tick", int32_type], ["UnitId", int32_type], ["Request", Request_$reflection()]]);
}

export class CapabilityReplayFrame extends Record {
    constructor(Tick, StateDigest, EventsDigest) {
        super();
        this.Tick = (Tick | 0);
        this.StateDigest = StateDigest;
        this.EventsDigest = EventsDigest;
    }
}

export function CapabilityReplayFrame_$reflection() {
    return record_type("SIR.Simulation.CapabilityReplayFrame", [], CapabilityReplayFrame, () => [["Tick", int32_type], ["StateDigest", array_type(uint8_type)], ["EventsDigest", array_type(uint8_type)]]);
}

const CapabilityExecution_utf8 = get_UTF8();

export function CapabilityExecution_engagementRequest(requestId, target, capabilityId) {
    const patternInput = (target.tag === 1) ? [5 & 0xFF, target.fields[0]] : [1 & 0xFF, target.fields[0]];
    const capability = CapabilityExecution_utf8.getBytes(capabilityId);
    if (capability.length > 255) {
        throw new Exception("Capability identifier exceeds the ABI string bound. (Parameter \'capabilityId\')");
    }
    return new Request(5, requestId, concat([new Uint8Array([1 & 0xFF, patternInput[0]]), int32LittleEndian(patternInput[1]), new Uint8Array([capability.length & 0xFF]), capability], Uint8Array));
}

export function CapabilityExecution_cancelRequest(requestId) {
    return new Request(7, requestId, new Uint8Array([]));
}

function CapabilityExecution_readI32(bytes, offset) {
    return (((~~item(offset, bytes) | (~~item(offset + 1, bytes) << 8)) | (~~item(offset + 2, bytes) << 16)) | (~~item(offset + 3, bytes) << 24)) | 0;
}

function CapabilityExecution_directionIndex(direction) {
    return ~~Direction8Module_toCode(direction) | 0;
}

function CapabilityExecution_directionDistance(left, right) {
    const difference = Math.abs(CapabilityExecution_directionIndex(left) - CapabilityExecution_directionIndex(right)) | 0;
    return min(difference, 8 - difference) | 0;
}

function CapabilityExecution_targetCell(state, target) {
    if (target.tag === 1) {
        return tryFind(target.fields[0], state.Areas);
    }
    else {
        const option_1 = tryFind(target.fields[0], state.Units);
        if (option_1 != null) {
            return option_1.Cell;
        }
        else {
            return undefined;
        }
    }
}

function CapabilityExecution_directionTo(column, row, targetColumn, targetRow) {
    return Direction8Module_tryFromDelta(compare(targetColumn, column), compare(targetRow, row));
}

function CapabilityExecution_distance(column, row, targetColumn, targetRow) {
    return max(Math.abs(targetColumn - column), Math.abs(targetRow - row)) | 0;
}

function CapabilityExecution_decodeEngagement(payload) {
    if ((payload.length < 7) ? true : (item(0, payload) !== (1 & 0xFF))) {
        return new FSharpResult$2(/* Error */ 1, ["Malformed SetEngagement payload."]);
    }
    else {
        let target;
        const matchValue = item(1, payload);
        target = ((matchValue === (1 & 0xFF)) ? (new FSharpResult$2(/* Ok */ 0, [new CapabilityTarget(/* PointCapabilityTarget */ 0, [CapabilityExecution_readI32(payload, 2)])])) : ((matchValue === (5 & 0xFF)) ? (new FSharpResult$2(/* Ok */ 0, [new CapabilityTarget(/* AreaCapabilityTarget */ 1, [CapabilityExecution_readI32(payload, 2)])])) : (new FSharpResult$2(/* Error */ 1, ["Unsupported engagement target kind."]))));
        const length = ~~item(6, payload) | 0;
        if (payload.length !== (7 + length)) {
            return new FSharpResult$2(/* Error */ 1, ["Malformed capability identifier."]);
        }
        else {
            return Result_Map((value_4) => [value_4, CapabilityExecution_utf8.getString(payload, 7, length)], target);
        }
    }
}

function CapabilityExecution_validateTarget(descriptor, target) {
    let matchResult;
    if (descriptor.TargetContract.tag === 1) {
        if (target.tag === 1) {
            matchResult = 0;
        }
        else {
            matchResult = 1;
        }
    }
    else if (target.tag === 0) {
        matchResult = 0;
    }
    else {
        matchResult = 1;
    }
    switch (matchResult) {
        case 0:
            return new FSharpResult$2(/* Ok */ 0, [undefined]);
        default:
            return new FSharpResult$2(/* Error */ 1, ["Target shape is not permitted by the capability descriptor."]);
    }
}

/**
 * Host-side structural validation before ruleset/loadout validation reaches state.
 */
export function CapabilityExecution_validateRequest(request) {
    const matchValue = request.Kind;
    switch (matchValue) {
        case 5: {
            const matchValue_1 = CapabilityExecution_decodeEngagement(request.Payload);
            if (matchValue_1.tag === 0) {
                return new FSharpResult$2(/* Ok */ 0, [undefined]);
            }
            else {
                return new FSharpResult$2(/* Error */ 1, [matchValue_1.fields[0]]);
            }
        }
        case 7:
            if (request.Payload.length === 0) {
                return new FSharpResult$2(/* Ok */ 0, [undefined]);
            }
            else {
                return new FSharpResult$2(/* Error */ 1, ["CancelAction payload must be empty."]);
            }
        default:
            return new FSharpResult$2(/* Ok */ 0, [undefined]);
    }
}

function CapabilityExecution_beginEngagement(state, unitId, target, capabilityId, unit) {
    let cell, tupledArg;
    const matchValue = HumanCapabilities_tryFind(capabilityId);
    if (matchValue != null) {
        if (!contains(capabilityId, unit.Loadout.CapabilityIds, {
            Equals: (x, y) => (x === y),
            GetHashCode: (x) => (stringHash(x) | 0),
        })) {
            const descriptor_1 = matchValue;
            return [unit, singleton(new CapabilityEvent(/* CapabilityRejected */ 0, [unitId, capabilityId, "not present in authored loadout"]))];
        }
        else {
            const descriptor_2 = matchValue;
            const matchValue_1 = CapabilityExecution_validateTarget(descriptor_2, target);
            const matchValue_2 = CapabilityExecution_targetCell(state, target);
            const copyOfStruct = matchValue_1;
            if (copyOfStruct.tag === 0) {
                if (matchValue_2 != null) {
                    if ((cell = matchValue_2, ((tupledArg = unit.Cell, CapabilityExecution_distance(tupledArg[0], tupledArg[1], cell[0], cell[1]))) > descriptor_2.MaximumRangeCells)) {
                        const cell_1 = matchValue_2;
                        return [unit, singleton(new CapabilityEvent(/* CapabilityRejected */ 0, [unitId, capabilityId, "target is out of range"]))];
                    }
                    else {
                        const cell_2 = matchValue_2;
                        let targetDirection;
                        const tupledArg_1 = unit.Cell;
                        targetDirection = CapabilityExecution_directionTo(tupledArg_1[0], tupledArg_1[1], cell_2[0], cell_2[1]);
                        if (targetDirection != null) {
                            const direction = targetDirection;
                            const traverse = (CapabilityExecution_directionDistance(unit.Attention, direction) * descriptor_2.TraverseTicksPerDirection) | 0;
                            const preserved = defaultArg(tryFind(capabilityId, unit.PreservedPreparation), 0) | 0;
                            return [new CapabilityUnitState(unit.Loadout, unit.Cell, unit.Attention, unit.Ammunition, remove(capabilityId, unit.PreservedPreparation), new CapabilityEngagement(capabilityId, target, direction, traverse, descriptor_2.PreparationTicks - preserved, preserved)), singleton(new CapabilityEvent(/* CapabilityTraversing */ 1, [unitId, capabilityId, traverse]))];
                        }
                        else {
                            return [unit, singleton(new CapabilityEvent(/* CapabilityRejected */ 0, [unitId, capabilityId, "target has no direction"]))];
                        }
                    }
                }
                else {
                    return [unit, singleton(new CapabilityEvent(/* CapabilityRejected */ 0, [unitId, capabilityId, "target is unavailable"]))];
                }
            }
            else {
                return [unit, singleton(new CapabilityEvent(/* CapabilityRejected */ 0, [unitId, capabilityId, copyOfStruct.fields[0]]))];
            }
        }
    }
    else {
        return [unit, singleton(new CapabilityEvent(/* CapabilityRejected */ 0, [unitId, capabilityId, "unknown descriptor"]))];
    }
}

function CapabilityExecution_applyRequest(state, unitId, request, unit, events) {
    const matchValue = request.Kind;
    let matchResult;
    switch (matchValue) {
        case 3: {
            if (request.Payload.length === 1) {
                matchResult = 0;
            }
            else {
                matchResult = 3;
            }
            break;
        }
        case 5: {
            matchResult = 1;
            break;
        }
        case 7: {
            matchResult = 2;
            break;
        }
        default:
            matchResult = 3;
    }
    switch (matchResult) {
        case 0: {
            const matchValue_1 = Direction8Module_tryFromCode(item(0, request.Payload));
            if (matchValue_1 == null) {
                return [unit, cons(new CapabilityEvent(/* CapabilityRejected */ 0, [unitId, "", "invalid attention direction"]), events)];
            }
            else {
                return [new CapabilityUnitState(unit.Loadout, unit.Cell, matchValue_1, unit.Ammunition, unit.PreservedPreparation, unit.Engagement), events];
            }
        }
        case 1: {
            const matchValue_2 = CapabilityExecution_decodeEngagement(request.Payload);
            if (matchValue_2.tag === 1) {
                return [unit, cons(new CapabilityEvent(/* CapabilityRejected */ 0, [unitId, "", matchValue_2.fields[0]]), events)];
            }
            else {
                const patternInput = CapabilityExecution_beginEngagement(state, unitId, matchValue_2.fields[0][0], matchValue_2.fields[0][1], unit);
                return [patternInput[0], append(reverse(patternInput[1]), events)];
            }
        }
        case 2: {
            const matchValue_3 = unit.Engagement;
            if (matchValue_3 != null) {
                const engagement = matchValue_3;
                const preserve = equals(value_5(HumanCapabilities_tryFind(engagement.CapabilityId)).InterruptionRule, CapabilityInterruptionRule.PreservePreparation);
                return [new CapabilityUnitState(unit.Loadout, unit.Cell, unit.Attention, unit.Ammunition, preserve ? add(engagement.CapabilityId, engagement.PreparedTicks, unit.PreservedPreparation) : remove(engagement.CapabilityId, unit.PreservedPreparation), undefined), cons(new CapabilityEvent(/* CapabilityInterrupted */ 5, [unitId, engagement.CapabilityId, preserve]), events)];
            }
            else {
                return [unit, events];
            }
        }
        default:
            return [unit, events];
    }
}

function CapabilityExecution_advance(unitId, unit, events) {
    let matchValue_1;
    const matchValue = unit.Engagement;
    if (matchValue != null) {
        const engagement = matchValue;
        const descriptor = value_5(HumanCapabilities_tryFind(engagement.CapabilityId));
        if (engagement.TraverseTicksRemaining > 0) {
            const next = new CapabilityEngagement(engagement.CapabilityId, engagement.Target, engagement.RequiredAttention, engagement.TraverseTicksRemaining - 1, engagement.PreparationTicksRemaining, engagement.PreparedTicks);
            return [new CapabilityUnitState(unit.Loadout, unit.Cell, (next.TraverseTicksRemaining === 0) ? engagement.RequiredAttention : unit.Attention, unit.Ammunition, unit.PreservedPreparation, next), events];
        }
        else if (engagement.PreparationTicksRemaining > 0) {
            const nextRemaining = (engagement.PreparationTicksRemaining - 1) | 0;
            return [new CapabilityUnitState(unit.Loadout, unit.Cell, unit.Attention, unit.Ammunition, unit.PreservedPreparation, new CapabilityEngagement(engagement.CapabilityId, engagement.Target, engagement.RequiredAttention, engagement.TraverseTicksRemaining, nextRemaining, min(descriptor.PreparationTicks, engagement.PreparedTicks + 1))), (nextRemaining === 0) ? cons(new CapabilityEvent(/* CapabilityPrepared */ 2, [unitId, engagement.CapabilityId]), events) : events];
        }
        else {
            const available = defaultArg(tryFind(engagement.CapabilityId, unit.Ammunition), 0) | 0;
            if (available < descriptor.AmmunitionPerResolution) {
                return [new CapabilityUnitState(unit.Loadout, unit.Cell, unit.Attention, unit.Ammunition, unit.PreservedPreparation, undefined), cons(new CapabilityEvent(/* CapabilityRejected */ 0, [unitId, engagement.CapabilityId, "ammunition unavailable"]), events)];
            }
            else {
                const remaining = (available - descriptor.AmmunitionPerResolution) | 0;
                return [new CapabilityUnitState(unit.Loadout, unit.Cell, unit.Attention, add(engagement.CapabilityId, remaining, unit.Ammunition), remove(engagement.CapabilityId, unit.PreservedPreparation), undefined), cons((matchValue_1 = engagement.Target, (matchValue_1.tag === 1) ? (new CapabilityEvent(/* AreaEngagementResolved */ 4, [unitId, matchValue_1.fields[0], engagement.CapabilityId, remaining])) : (new CapabilityEvent(/* PointEngagementResolved */ 3, [unitId, matchValue_1.fields[0], engagement.CapabilityId, remaining]))), events)];
            }
        }
    }
    else {
        return [unit, events];
    }
}

export function CapabilityExecution_runTick(state, requests) {
    let units = state.Units;
    let events = empty();
    iterate((tupledArg_1) => {
        const unitId_1 = tupledArg_1[0] | 0;
        const matchValue = tryFind(unitId_1, units);
        if (matchValue != null) {
            const patternInput = CapabilityExecution_applyRequest(state, unitId_1, tupledArg_1[1], matchValue, empty());
            units = add(unitId_1, patternInput[0], units);
            events = append(reverse(patternInput[1]), events);
        }
        else {
            events = cons(new CapabilityEvent(/* CapabilityRejected */ 0, [unitId_1, "", "unit unavailable"]), events);
        }
    }, sortBy((tupledArg) => [tupledArg[0], tupledArg[1].ModuleRequestId], requests, {
        Compare: (x, y) => (compareArrays(x, y) | 0),
    }));
    iterate((tupledArg_2) => {
        const unitId_2 = tupledArg_2[0] | 0;
        const patternInput_1 = CapabilityExecution_advance(unitId_2, tupledArg_2[1], empty());
        units = add(unitId_2, patternInput_1[0], units);
        events = append(reverse(patternInput_1[1]), events);
    }, toList(units));
    return new CapabilityTickResult(new CapabilityExecutionState(state.Tick + 1, units, state.Areas), reverse(events));
}

function CapabilityExecution_text(value) {
    return CapabilityExecution_utf8.getBytes(value + "\n");
}

export function CapabilityExecution_stateDigest(state) {
    return sha256(CapabilityExecution_text(join("|", append(collect((tupledArg) => {
        const unit = tupledArg[1];
        return toList_1(delay(() => append_1(singleton_1(int32ToString(tupledArg[0])), delay(() => append_1(singleton_1((int32ToString(unit.Cell[0]) + ",") + int32ToString(unit.Cell[1])), delay(() => append_1(singleton_1(Direction8Module_toCode(unit.Attention).toString()), delay(() => append_1(singleton_1(join(",", unit.Loadout.CapabilityIds)), delay(() => append_1(singleton_1(join(",", map((tupledArg_1) => ((tupledArg_1[0] + "=") + int32ToString(tupledArg_1[1])), toList(unit.Ammunition)))), delay(() => append_1(singleton_1(join(",", map((tupledArg_2) => ((tupledArg_2[0] + "=") + int32ToString(tupledArg_2[1])), toList(unit.PreservedPreparation)))), delay(() => {
            let target;
            const matchValue = unit.Engagement;
            if (matchValue != null) {
                const engagement = matchValue;
                return singleton_1(join(":", [engagement.CapabilityId, (target = engagement.Target, (target.tag === 1) ? ("area:" + int32ToString(target.fields[0])) : ("point:" + int32ToString(target.fields[0]))), Direction8Module_toCode(engagement.RequiredAttention).toString(), int32ToString(engagement.TraverseTicksRemaining), int32ToString(engagement.PreparationTicksRemaining), int32ToString(engagement.PreparedTicks)]));
            }
            else {
                return singleton_1("-");
            }
        }))))))))))))));
    }, toList(state.Units)), map((tupledArg_3) => {
        const _arg = tupledArg_3[1];
        return (((("area=" + int32ToString(tupledArg_3[0])) + ":") + int32ToString(_arg[0])) + ",") + int32ToString(_arg[1]);
    }, toList(state.Areas))))));
}

export function CapabilityExecution_eventsDigest(events) {
    return sha256(CapabilityExecution_text(join("|", map(toString, events))));
}

export function CapabilityExecution_replay(initial, finalTick, journal) {
    let state = initial;
    return toList_1(delay(() => collect_1((tick) => {
        const requests = choose((entry) => {
            if (entry.Tick === tick) {
                return [entry.UnitId, entry.Request];
            }
            else {
                return undefined;
            }
        }, journal);
        const result = CapabilityExecution_runTick(state, requests);
        state = result.State;
        return singleton_1(new CapabilityReplayFrame(state.Tick, CapabilityExecution_stateDigest(state), CapabilityExecution_eventsDigest(result.Events)));
    }, rangeDouble(initial.Tick, 1, finalTick - 1))));
}

export function CapabilityExecution_verifyReplay(initial, finalTick, journal, expected) {
    let value, option_1;
    const actual = CapabilityExecution_replay(initial, finalTick, journal);
    if (equals(actual, expected)) {
        return new FSharpResult$2(/* Ok */ 0, [actual]);
    }
    else {
        const sharedLength = min(length_1(actual), length_1(expected)) | 0;
        return new FSharpResult$2(/* Error */ 1, [(value = (((initial.Tick + sharedLength) + 1) | 0), defaultArg((option_1 = tryFindIndex((tupledArg) => !equals(tupledArg[0], tupledArg[1]), zip(truncate(sharedLength, actual), truncate(sharedLength, expected))), (option_1 != null) ? ((initial.Tick + option_1) + 1) : undefined), value))]);
    }
}

