
import { Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { array_type, uint8_type, tuple_type, class_type, int32_type, list_type, bool_type, record_type, union_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { Direction8, ResolvedOrientation, Direction8Module_tryFromDelta, UnitIdModule_value, UnitIdModule_create, Direction8_$reflection, UnitId_$reflection } from "../SIR.Domain/Orientation.js";
import { Cell, Cell_$reflection } from "../fable_modules/FS.GG.Game.Core.0.13.0/Primitives.fs.js";
import { BoundedInt32Module_subtractSaturating, BoundedInt32Module_value, BoundedInt32Module_create, BoundedInt32_$reflection } from "../SIR.Domain/BoundedInt32.js";
import { Edges_edgeBetween, Edge_$reflection } from "../fable_modules/FS.GG.Game.Core.0.13.0/Edges.fs.js";
import { RuleApplication_$reflection } from "../SIR.Domain/RuleTypes.js";
import { printf, toFail } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { safeHash, equals, compareArrays, compare, Exception } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { sortWith, map, length, append, collect, reverse, empty as empty_1, cons, fold, choose, tryPick, tryFind, ofArray, singleton } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { FSharpMap__get_Count, toList, find, exists, add, tryFind as tryFind_1, ofList } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { FSharpSet__get_Count, toList as toList_1, contains, add as add_1, empty } from "../fable_modules/fable-library-js.5.13.0/Set.js";
import { compare as compare_1, toInt32_unchecked, equals as equals_1, fromInt32, op_Subtraction, toInt64_unchecked, abs, max } from "../fable_modules/fable-library-js.5.13.0/BigInt.js";
import { List_distinct, List_countBy } from "../fable_modules/fable-library-js.5.13.0/Seq2.js";
import { LineMode, Los_lineOfSightBy } from "../fable_modules/FS.GG.Game.Core.0.13.0/Los.fs.js";
import { CombatAttackInput, CombatRules_resolveAttack } from "./CombatRules.js";
import { FixedPointModule_fromRatio, FixedPointModule_zero } from "../SIR.Domain/FixedPoint.js";
import { digest32, direction8, boundedInt32, byteValue, int32LittleEndian, concatenate } from "../SIR.Domain/CanonicalEncoding.js";

/**
 * The two sides used by the minimal simulation slice.
 */
export class Side extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Red", "Blue"];
    }
    static Red = new Side(0, []);
    static Blue = new Side(1, []);
}

export function Side_$reflection() {
    return union_type("SIR.Simulation.Side", [], Side, () => [[], []]);
}

/**
 * Authoritative state for one unit.
 */
export class UnitState extends Record {
    constructor(Id, Side, Cell, Health, BodyFacing, AttentionDirection) {
        super();
        this.Id = Id;
        this.Side = Side;
        this.Cell = Cell;
        this.Health = Health;
        this.BodyFacing = BodyFacing;
        this.AttentionDirection = AttentionDirection;
    }
}

export function UnitState_$reflection() {
    return record_type("SIR.Simulation.UnitState", [], UnitState, () => [["Id", UnitId_$reflection()], ["Side", Side_$reflection()], ["Cell", Cell_$reflection()], ["Health", BoundedInt32_$reflection()], ["BodyFacing", Direction8_$reflection()], ["AttentionDirection", Direction8_$reflection()]]);
}

/**
 * A canonical boundary with semantics owned by S.I.R.
 */
export class SemanticEdge extends Record {
    constructor(Edge, BlocksMovement) {
        super();
        this.Edge = Edge;
        this.BlocksMovement = BlocksMovement;
    }
}

export function SemanticEdge_$reflection() {
    return record_type("SIR.Simulation.SemanticEdge", [], SemanticEdge, () => [["Edge", Edge_$reflection()], ["BlocksMovement", bool_type]]);
}

/**
 * The fixed board and its semantic boundaries.
 */
export class Board extends Record {
    constructor(Minimum, Maximum, Edges) {
        super();
        this.Minimum = Minimum;
        this.Maximum = Maximum;
        this.Edges = Edges;
    }
}

export function Board_$reflection() {
    return record_type("SIR.Simulation.Board", [], Board, () => [["Minimum", Cell_$reflection()], ["Maximum", Cell_$reflection()], ["Edges", list_type(SemanticEdge_$reflection())]]);
}

/**
 * Complete authoritative state for the minimal slice.
 */
export class SimulationState extends Record {
    constructor(Tick, Board, Units, Observations) {
        super();
        this.Tick = (Tick | 0);
        this.Board = Board;
        this.Units = Units;
        this.Observations = Observations;
    }
}

export function SimulationState_$reflection() {
    return record_type("SIR.Simulation.SimulationState", [], SimulationState, () => [["Tick", int32_type], ["Board", Board_$reflection()], ["Units", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [UnitId_$reflection(), UnitState_$reflection()])], ["Observations", class_type("Microsoft.FSharp.Collections.FSharpSet`1", [tuple_type(UnitId_$reflection(), UnitId_$reflection())])]]);
}

/**
 * Validated replay-driving inputs consumed by the shared kernel.
 */
export class KernelInput extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Move", "Observe", "Attack"];
    }
}

export function KernelInput_$reflection() {
    return union_type("SIR.Simulation.KernelInput", [], KernelInput, () => [[["unitId", UnitId_$reflection()], ["destination", Cell_$reflection()]], [["observerId", UnitId_$reflection()], ["targetId", UnitId_$reflection()]], [["attackerId", UnitId_$reflection()], ["targetId", UnitId_$reflection()]]]);
}

/**
 * Stable logical phases used by conformance diagnostics.
 */
export class SimulationPhase extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["MovementPhase", "ObservationPhase", "AttackPhase", "CommitPhase"];
    }
    static MovementPhase = new SimulationPhase(0, []);
    static ObservationPhase = new SimulationPhase(1, []);
    static AttackPhase = new SimulationPhase(2, []);
    static CommitPhase = new SimulationPhase(3, []);
}

export function SimulationPhase_$reflection() {
    return union_type("SIR.Simulation.SimulationPhase", [], SimulationPhase, () => [[], [], [], []]);
}

/**
 * The authoritative event stream emitted by the minimal slice.
 */
export class SimulationEvent extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["UnitMoved", "MovementBlockedByEdge", "UnitObserved", "AttackResolved"];
    }
}

export function SimulationEvent_$reflection() {
    return union_type("SIR.Simulation.SimulationEvent", [], SimulationEvent, () => [[["unitId", UnitId_$reflection()], ["origin", Cell_$reflection()], ["destination", Cell_$reflection()]], [["unitId", UnitId_$reflection()], ["origin", Cell_$reflection()], ["destination", Cell_$reflection()], ["edge", Edge_$reflection()]], [["observerId", UnitId_$reflection()], ["targetId", UnitId_$reflection()], ["distance", int32_type]], [["attackerId", UnitId_$reflection()], ["targetId", UnitId_$reflection()], ["damage", int32_type], ["remainingHealth", int32_type], ["explanation", RuleApplication_$reflection()]]]);
}

/**
 * One logical-phase checkpoint for first-divergence diagnosis.
 */
export class PhaseCheckpoint extends Record {
    constructor(Tick, Phase, State, Events) {
        super();
        this.Tick = (Tick | 0);
        this.Phase = Phase;
        this.State = State;
        this.Events = Events;
    }
}

export function PhaseCheckpoint_$reflection() {
    return record_type("SIR.Simulation.PhaseCheckpoint", [], PhaseCheckpoint, () => [["Tick", int32_type], ["Phase", SimulationPhase_$reflection()], ["State", SimulationState_$reflection()], ["Events", list_type(SimulationEvent_$reflection())]]);
}

/**
 * Result of one committed simulation tick.
 */
export class TickResult extends Record {
    constructor(State, Events, StateBytes, EventBytes, StateDigest, Checkpoints) {
        super();
        this.State = State;
        this.Events = Events;
        this.StateBytes = StateBytes;
        this.EventBytes = EventBytes;
        this.StateDigest = StateDigest;
        this.Checkpoints = Checkpoints;
    }
}

export function TickResult_$reflection() {
    return record_type("SIR.Simulation.TickResult", [], TickResult, () => [["State", SimulationState_$reflection()], ["Events", list_type(SimulationEvent_$reflection())], ["StateBytes", array_type(uint8_type)], ["EventBytes", array_type(uint8_type)], ["StateDigest", array_type(uint8_type)], ["Checkpoints", list_type(PhaseCheckpoint_$reflection())]]);
}

/**
 * Bounded authoritative rules that may be varied by a derived design scenario.
 */
export class SimulationRules extends Record {
    constructor(AttackPower) {
        super();
        this.AttackPower = AttackPower;
    }
}

export function SimulationRules_$reflection() {
    return record_type("SIR.Simulation.SimulationRules", [], SimulationRules, () => [["AttackPower", BoundedInt32_$reflection()]]);
}

export function Simulation_unitId(value) {
    return UnitIdModule_create(value);
}

export function Simulation_unitIdValue(id) {
    return UnitIdModule_value(id) | 0;
}

/**
 * Resolves orientation from authoritative body/attention state and an
 * optional active route segment. Movement direction is never stored.
 */
export function Simulation_resolvedOrientation(origin, destination, unit) {
    let option_1, target;
    return new ResolvedOrientation((option_1 = destination, (option_1 != null) ? ((target = option_1, Direction8Module_tryFromDelta(target.Col - origin.Col, target.Row - origin.Row))) : undefined), unit.BodyFacing, unit.AttentionDirection);
}

function Simulation_required(result) {
    if (result.tag === 1) {
        return toFail(printf("Invalid minimal-slice state: %A"))(result.fields[0]);
    }
    else {
        return result.fields[0];
    }
}

function Simulation_health(value) {
    return Simulation_required(BoundedInt32Module_create(0, 100, value));
}

export const Simulation_defaultRules = new SimulationRules(Simulation_health(25));

function Simulation_cell(col, row) {
    return new Cell(col, row);
}

function Simulation_requiredEdge(left, right) {
    const option_1 = Edges_edgeBetween(left, right);
    if (option_1 != null) {
        return option_1;
    }
    else {
        throw new Exception("The minimal-slice semantic edge must be orthogonal.");
    }
}

export const Simulation_initialState = (() => {
    const red = new UnitState(Simulation_unitId(10), Side.Red, Simulation_cell(0, 0), Simulation_health(100), Direction8.North, Direction8.North);
    const blue = new UnitState(Simulation_unitId(20), Side.Blue, Simulation_cell(2, 0), Simulation_health(100), Direction8.North, Direction8.North);
    const edge = new SemanticEdge(Simulation_requiredEdge(Simulation_cell(1, 0), Simulation_cell(2, 0)), true);
    return new SimulationState(0, new Board(Simulation_cell(0, 0), Simulation_cell(2, 1), singleton(edge)), ofList(ofArray([[red.Id, red], [blue.Id, blue]]), {
        Compare: (x, y) => (compare(x, y) | 0),
    }), empty({
        Compare: (x_1, y_1) => (compareArrays(x_1, y_1) | 0),
    }));
})();

export const Simulation_inputs = ofArray([new KernelInput(/* Attack */ 2, [Simulation_unitId(10), Simulation_unitId(20)]), new KernelInput(/* Move */ 0, [Simulation_unitId(20), Simulation_cell(1, 0)]), new KernelInput(/* Observe */ 1, [Simulation_unitId(10), Simulation_unitId(20)]), new KernelInput(/* Move */ 0, [Simulation_unitId(10), Simulation_cell(1, 1)])]);

function Simulation_inputCompare(left, right) {
    const key = (input) => {
        switch (input.tag) {
            case 1:
                return [1, Simulation_unitIdValue(input.fields[0]), 0, 0, Simulation_unitIdValue(input.fields[1])];
            case 2:
                return [2, Simulation_unitIdValue(input.fields[0]), 0, 0, Simulation_unitIdValue(input.fields[1])];
            default: {
                const destination = input.fields[1];
                return [0, Simulation_unitIdValue(input.fields[0]), destination.Col, destination.Row, 0];
            }
        }
    };
    return compareArrays(key(left), key(right)) | 0;
}

function Simulation_inBounds(board, position) {
    if (((position.Col >= board.Minimum.Col) && (position.Col <= board.Maximum.Col)) && (position.Row >= board.Minimum.Row)) {
        return position.Row <= board.Maximum.Row;
    }
    else {
        return false;
    }
}

function Simulation_chebyshevDistance(left, right) {
    return max(abs(toInt64_unchecked(op_Subtraction(toInt64_unchecked(fromInt32(right.Col)), toInt64_unchecked(fromInt32(left.Col))))), abs(toInt64_unchecked(op_Subtraction(toInt64_unchecked(fromInt32(right.Row)), toInt64_unchecked(fromInt32(left.Row))))));
}

function Simulation_blockingEdge(board, left, right) {
    const option_3 = Edges_edgeBetween(left, right);
    if (option_3 != null) {
        const crossed = option_3;
        const option_1 = tryFind((semantic) => {
            if (semantic.BlocksMovement) {
                return equals(semantic.Edge, crossed);
            }
            else {
                return false;
            }
        }, board.Edges);
        if (option_1 != null) {
            return option_1.Edge;
        }
        else {
            return undefined;
        }
    }
    else {
        return undefined;
    }
}

function Simulation_diagonalEdges(origin, destination) {
    const horizontal = Simulation_cell(destination.Col, origin.Row);
    const vertical = Simulation_cell(origin.Col, destination.Row);
    return ofArray([[origin, horizontal], [origin, vertical], [horizontal, destination], [vertical, destination]]);
}

function Simulation_movementBlocker(board, origin, destination) {
    if (!Simulation_inBounds(board, destination) ? true : !equals_1(Simulation_chebyshevDistance(origin, destination), 1n)) {
        return undefined;
    }
    else if ((origin.Col === destination.Col) ? true : (origin.Row === destination.Row)) {
        return Simulation_blockingEdge(board, origin, destination);
    }
    else {
        return tryPick((tupledArg) => Simulation_blockingEdge(board, tupledArg[0], tupledArg[1]), Simulation_diagonalEdges(origin, destination));
    }
}

function Simulation_tryUnit(id, state) {
    return tryFind_1(id, state.Units);
}

function Simulation_replaceUnit(unit, state) {
    return new SimulationState(state.Tick, state.Board, add(unit.Id, unit, state.Units), state.Observations);
}

function Simulation_movementPhase(state, inputs) {
    const candidates = choose((tupledArg) => {
        const unitId_1 = tupledArg[0];
        const destination_1 = tupledArg[1];
        const matchValue = Simulation_tryUnit(unitId_1, state);
        if (matchValue != null) {
            const unit = matchValue;
            const matchValue_1 = Simulation_movementBlocker(state.Board, unit.Cell, destination_1);
            if (matchValue_1 == null) {
                if ((Simulation_inBounds(state.Board, destination_1) && equals_1(Simulation_chebyshevDistance(unit.Cell, destination_1), 1n)) && !exists((otherId, other) => {
                    if (!equals(otherId, unitId_1)) {
                        return equals(other.Cell, destination_1);
                    }
                    else {
                        return false;
                    }
                }, state.Units)) {
                    return [unit, destination_1, undefined];
                }
                else {
                    return undefined;
                }
            }
            else {
                return [unit, destination_1, matchValue_1];
            }
        }
        else {
            return undefined;
        }
    }, choose((_arg) => {
        if (_arg.tag === 0) {
            return [_arg.fields[0], _arg.fields[1]];
        }
        else {
            return undefined;
        }
    }, inputs));
    const destinationCounts = ofList(List_countBy((x) => x, choose((tupledArg_1) => {
        if (tupledArg_1[2] == null) {
            return tupledArg_1[1];
        }
        else {
            return undefined;
        }
    }, candidates), {
        Equals: equals,
        GetHashCode: (x_1) => (safeHash(x_1) | 0),
    }), {
        Compare: (x_2, y_1) => (compare(x_2, y_1) | 0),
    });
    return [fold((current, tupledArg_2) => {
        const unit_1 = tupledArg_2[0];
        const destination_3 = tupledArg_2[1];
        let matchResult;
        if (tupledArg_2[2] == null) {
            if (find(destination_3, destinationCounts) === 1) {
                matchResult = 0;
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
                return Simulation_replaceUnit(new UnitState(unit_1.Id, unit_1.Side, destination_3, unit_1.Health, unit_1.BodyFacing, unit_1.AttentionDirection), current);
            default:
                return current;
        }
    }, state, candidates), choose((tupledArg_3) => {
        const unit_2 = tupledArg_3[0];
        const destination_4 = tupledArg_3[1];
        const blocker_2 = tupledArg_3[2];
        if (blocker_2 == null) {
            if (find(destination_4, destinationCounts) === 1) {
                return new SimulationEvent(/* UnitMoved */ 0, [unit_2.Id, unit_2.Cell, destination_4]);
            }
            else {
                return undefined;
            }
        }
        else {
            return new SimulationEvent(/* MovementBlockedByEdge */ 1, [unit_2.Id, unit_2.Cell, destination_4, blocker_2]);
        }
    }, candidates)];
}

function Simulation_observationPhase(state, inputs) {
    const tupledArg_2 = fold((tupledArg, tupledArg_1) => {
        const current = tupledArg[0];
        const events = tupledArg[1];
        const observerId_1 = tupledArg_1[0];
        const targetId_1 = tupledArg_1[1];
        const matchValue = Simulation_tryUnit(observerId_1, current);
        const matchValue_1 = Simulation_tryUnit(targetId_1, current);
        let matchResult, observer, target;
        if (matchValue != null) {
            if (matchValue_1 != null) {
                matchResult = 0;
                observer = matchValue;
                target = matchValue_1;
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
                if (Los_lineOfSightBy(LineMode.Supercover, (position) => Simulation_inBounds(current.Board, position), observer.Cell, target.Cell)) {
                    const distance = ~~toInt32_unchecked(Simulation_chebyshevDistance(observer.Cell, target.Cell)) | 0;
                    return [new SimulationState(current.Tick, current.Board, current.Units, add_1([observerId_1, targetId_1], current.Observations)), cons(new SimulationEvent(/* UnitObserved */ 2, [observerId_1, targetId_1, distance]), events)];
                }
                else {
                    return [current, events];
                }
            default:
                return [current, events];
        }
    }, [state, empty_1()], choose((_arg) => {
        if (_arg.tag === 1) {
            return [_arg.fields[0], _arg.fields[1]];
        }
        else {
            return undefined;
        }
    }, inputs));
    return [tupledArg_2[0], reverse(tupledArg_2[1])];
}

function Simulation_attackPhase(rules, state, inputs) {
    const tupledArg_2 = fold((tupledArg, tupledArg_1) => {
        let target, attacker;
        const current = tupledArg[0];
        const events = tupledArg[1];
        const attackerId_1 = tupledArg_1[0];
        const targetId_1 = tupledArg_1[1];
        const matchValue = Simulation_tryUnit(attackerId_1, current);
        const matchValue_1 = Simulation_tryUnit(targetId_1, current);
        let matchResult, attacker_1, target_1;
        if (matchValue != null) {
            if (matchValue_1 != null) {
                if ((target = matchValue_1, (attacker = matchValue, contains([attackerId_1, targetId_1], current.Observations) && (compare_1(Simulation_chebyshevDistance(attacker.Cell, target.Cell), 1n) <= 0)))) {
                    matchResult = 0;
                    attacker_1 = matchValue;
                    target_1 = matchValue_1;
                }
                else {
                    matchResult = 1;
                }
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
                const combat = Simulation_required(CombatRules_resolveAttack(new CombatAttackInput(attacker_1.Cell, singleton(target_1.Cell), (position) => Simulation_inBounds(current.Board, position), ~~toInt32_unchecked(Simulation_chebyshevDistance(attacker_1.Cell, target_1.Cell)), FixedPointModule_zero, Simulation_required(FixedPointModule_fromRatio(BoundedInt32Module_value(rules.AttackPower), 1)), Simulation_required(FixedPointModule_fromRatio(1, 1)), `tick-${current.Tick + 1}-attack-${Simulation_unitIdValue(attackerId_1)}-${Simulation_unitIdValue(targetId_1)}`)));
                const damage = Simulation_required(BoundedInt32Module_create(0, 100, combat.ExpectedDamage));
                const remaining = Simulation_required(BoundedInt32Module_subtractSaturating(target_1.Health, damage));
                return [Simulation_replaceUnit(new UnitState(target_1.Id, target_1.Side, target_1.Cell, remaining, target_1.BodyFacing, target_1.AttentionDirection), current), cons(new SimulationEvent(/* AttackResolved */ 3, [attackerId_1, targetId_1, BoundedInt32Module_value(damage), BoundedInt32Module_value(remaining), combat.Explanation]), events)];
            }
            default:
                return [current, events];
        }
    }, [state, empty_1()], choose((_arg) => {
        if (_arg.tag === 2) {
            return [_arg.fields[0], _arg.fields[1]];
        }
        else {
            return undefined;
        }
    }, inputs));
    return [tupledArg_2[0], reverse(tupledArg_2[1])];
}

function Simulation_sideCode(side) {
    if (side.tag === 1) {
        return 1;
    }
    else {
        return 0;
    }
}

function Simulation_phaseCode(phase) {
    switch (phase.tag) {
        case 1:
            return 1;
        case 2:
            return 2;
        case 3:
            return 3;
        default:
            return 0;
    }
}

function Simulation_cellBytes(position) {
    return concatenate([int32LittleEndian(position.Col), int32LittleEndian(position.Row)]);
}

function Simulation_unitIdBytes(id) {
    return int32LittleEndian(Simulation_unitIdValue(id));
}

function Simulation_edgeBytes(edge) {
    return concatenate([Simulation_cellBytes(edge.Lo), Simulation_cellBytes(edge.Hi)]);
}

/**
 * Provisional canonical M6 state encoding. The versioned replay schema is selected in M7.
 */
export function Simulation_stateBytes(state) {
    const unitBytes = collect((tupledArg) => {
        const unit = tupledArg[1];
        return ofArray([Simulation_unitIdBytes(tupledArg[0]), byteValue(Simulation_sideCode(unit.Side)), Simulation_cellBytes(unit.Cell), boundedInt32(unit.Health), direction8(unit.BodyFacing), direction8(unit.AttentionDirection)]);
    }, toList(state.Units));
    const observationBytes = collect((tupledArg_1) => ofArray([Simulation_unitIdBytes(tupledArg_1[0]), Simulation_unitIdBytes(tupledArg_1[1])]), toList_1(state.Observations));
    return concatenate(append(ofArray([byteValue(2), int32LittleEndian(state.Tick), int32LittleEndian(FSharpMap__get_Count(state.Units))]), append(unitBytes, append(singleton(int32LittleEndian(FSharpSet__get_Count(state.Observations))), observationBytes))));
}

function Simulation_eventBytes(event) {
    switch (event.tag) {
        case 1:
            return concatenate([byteValue(1), Simulation_unitIdBytes(event.fields[0]), Simulation_cellBytes(event.fields[1]), Simulation_cellBytes(event.fields[2]), Simulation_edgeBytes(event.fields[3])]);
        case 2:
            return concatenate([byteValue(2), Simulation_unitIdBytes(event.fields[0]), Simulation_unitIdBytes(event.fields[1]), int32LittleEndian(event.fields[2])]);
        case 3:
            return concatenate([byteValue(3), Simulation_unitIdBytes(event.fields[0]), Simulation_unitIdBytes(event.fields[1]), int32LittleEndian(event.fields[2]), int32LittleEndian(event.fields[3])]);
        default:
            return concatenate([byteValue(0), Simulation_unitIdBytes(event.fields[0]), Simulation_cellBytes(event.fields[1]), Simulation_cellBytes(event.fields[2])]);
    }
}

/**
 * Provisional canonical M6 event encoding. Event order is phase order then canonical input order.
 */
export function Simulation_eventsBytes(events) {
    return concatenate(append(ofArray([byteValue(1), int32LittleEndian(length(events))]), map(Simulation_eventBytes, events)));
}

export function Simulation_checkpointBytes(checkpoint) {
    return concatenate([int32LittleEndian(checkpoint.Tick), byteValue(Simulation_phaseCode(checkpoint.Phase)), Simulation_stateBytes(checkpoint.State), Simulation_eventsBytes(checkpoint.Events)]);
}

/**
 * Executes one tick from stable phase inputs and commits each phase as a deterministic batch.
 */
export function Simulation_runTickWithRules(rules, state, journal) {
    const canonicalInputs = sortWith((left, right) => (Simulation_inputCompare(left, right) | 0), List_distinct(journal, {
        Equals: equals,
        GetHashCode: (x) => (safeHash(x) | 0),
    }));
    const nextTick = (state.Tick + 1) | 0;
    const patternInput = Simulation_movementPhase(state, canonicalInputs);
    const movementState = patternInput[0];
    const movementEvents = patternInput[1];
    const movementCheckpoint = new PhaseCheckpoint(nextTick, SimulationPhase.MovementPhase, movementState, movementEvents);
    const patternInput_1 = Simulation_observationPhase(movementState, canonicalInputs);
    const observationState = patternInput_1[0];
    const throughObservation = append(movementEvents, patternInput_1[1]);
    const observationCheckpoint = new PhaseCheckpoint(nextTick, SimulationPhase.ObservationPhase, observationState, throughObservation);
    const patternInput_2 = Simulation_attackPhase(rules, observationState, canonicalInputs);
    const attackState = patternInput_2[0];
    const allEvents = append(throughObservation, patternInput_2[1]);
    const attackCheckpoint = new PhaseCheckpoint(nextTick, SimulationPhase.AttackPhase, attackState, allEvents);
    const committed = new SimulationState(nextTick, attackState.Board, attackState.Units, attackState.Observations);
    const commitCheckpoint = new PhaseCheckpoint(nextTick, SimulationPhase.CommitPhase, committed, allEvents);
    const canonicalState = Simulation_stateBytes(committed);
    return new TickResult(committed, allEvents, canonicalState, Simulation_eventsBytes(allEvents), digest32(canonicalState), ofArray([movementCheckpoint, observationCheckpoint, attackCheckpoint, commitCheckpoint]));
}

/**
 * Executes the canonical rules used by replay and authoritative hosts.
 */
export function Simulation_runTick(state, journal) {
    return Simulation_runTickWithRules(Simulation_defaultRules, state, journal);
}

