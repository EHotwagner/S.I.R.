
import { toString, Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { uint8_type, tuple_type, class_type, list_type, string_type, record_type, option_type, int32_type, bool_type, union_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { Cell as Cell_1, Cell_$reflection } from "../fable_modules/FS.GG.Game.Core.0.13.0/Primitives.fs.js";
import { Direction8_$reflection } from "../SIR.Domain/Orientation.js";
import { tryHead, empty as empty_2, append as append_1, toList, singleton as singleton_1, enumerateWhile, filter, sortBy, tryPick, exists, map, collect, delay, toArray } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { rangeDouble } from "../fable_modules/fable-library-js.5.13.0/Range.js";
import { numberHash, safeHash, compareArrays, comparePrimitives, compare, equals } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { FSharpMap__get_Count, map as map_2, containsKey, remove, fold as fold_1, toList as toList_1, add, find, ofList, empty, toSeq, tryFind } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { some, defaultArg, toArray as toArray_1 } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { equalsWith, choose, sort, tryHead as tryHead_1, item, tryFind as tryFind_1, append } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { toList as toList_2, intersect, isEmpty as isEmpty_1, contains, ofArray } from "../fable_modules/fable-library-js.5.13.0/Set.js";
import { max, min } from "../fable_modules/fable-library-js.5.13.0/Double.js";
import { zip, tryPick as tryPick_1, sumBy, tail, append as append_2, sort as sort_1, tryHead as tryHead_2, tryFind as tryFind_2, ofArray as ofArray_1, collect as collect_1, reverse, map as map_1, sortBy as sortBy_1, head, length, item as item_1, choose as choose_1, exists as exists_1, fold, cons, empty as empty_1, filter as filter_1, min as min_1, isEmpty, singleton } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { List_groupBy, List_countBy } from "../fable_modules/fable-library-js.5.13.0/Seq2.js";
import { concatenate, int32LittleEndian } from "../SIR.Domain/CanonicalEncoding.js";

/**
 * Runtime-neutral terrain semantics for the map-scale kernel.
 */
export class MapScaleTerrain extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["OpenTerrain", "RoughTerrain", "BlockedTerrain", "ObjectiveTerrain"];
    }
    static OpenTerrain = new MapScaleTerrain(0, []);
    static RoughTerrain = new MapScaleTerrain(1, []);
    static BlockedTerrain = new MapScaleTerrain(2, []);
    static ObjectiveTerrain = new MapScaleTerrain(3, []);
}

export function MapScaleTerrain_$reflection() {
    return union_type("SIR.Simulation.MapScaleTerrain", [], MapScaleTerrain, () => [[], [], [], []]);
}

export class MapScaleEdgeKind extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["WallEdge", "DoorEdge", "WindowEdge"];
    }
    static WallEdge = new MapScaleEdgeKind(0, []);
    static WindowEdge = new MapScaleEdgeKind(2, []);
}

export function MapScaleEdgeKind_$reflection() {
    return union_type("SIR.Simulation.MapScaleEdgeKind", [], MapScaleEdgeKind, () => [[], [["isOpen", bool_type]], []]);
}

export class MapScaleEdgeDirection extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["EastEdge", "SouthEdge"];
    }
    static EastEdge = new MapScaleEdgeDirection(0, []);
    static SouthEdge = new MapScaleEdgeDirection(1, []);
}

export function MapScaleEdgeDirection_$reflection() {
    return union_type("SIR.Simulation.MapScaleEdgeDirection", [], MapScaleEdgeDirection, () => [[], []]);
}

export class MapScaleController extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["ManualController", "ScriptedController", "GeneralController"];
    }
    static ManualController = new MapScaleController(0, []);
    static ScriptedController = new MapScaleController(1, []);
    static GeneralController = new MapScaleController(2, []);
}

export function MapScaleController_$reflection() {
    return union_type("SIR.Simulation.MapScaleController", [], MapScaleController, () => [[], [], []]);
}

export class CombatDelivery extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["MeleeDelivery", "ProjectileDelivery", "LobbedAreaDelivery", "SpellAreaDelivery"];
    }
    static MeleeDelivery = new CombatDelivery(0, []);
    static ProjectileDelivery = new CombatDelivery(1, []);
    static LobbedAreaDelivery = new CombatDelivery(2, []);
    static SpellAreaDelivery = new CombatDelivery(3, []);
}

export function CombatDelivery_$reflection() {
    return union_type("SIR.Simulation.CombatDelivery", [], CombatDelivery, () => [[], [], [], []]);
}

export class AreaShape extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["BurstArea", "ConeArea", "RayArea", "RectangleArea"];
    }
}

export function AreaShape_$reflection() {
    return union_type("SIR.Simulation.AreaShape", [], AreaShape, () => [[["radius", int32_type]], [["range", int32_type], ["angleDegrees", int32_type]], [["length", int32_type], ["width", int32_type]], [["width", int32_type], ["depth", int32_type]]]);
}

export class CombatTarget extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["UnitTarget", "AreaTarget"];
    }
}

export function CombatTarget_$reflection() {
    return union_type("SIR.Simulation.CombatTarget", [], CombatTarget, () => [[["unitId", int32_type]], [["origin", Cell_$reflection()], ["shape", AreaShape_$reflection()]]]);
}

export class CombatProfile extends Record {
    constructor(Delivery, Range$, Damage, RecoveryTicks, AreaShape) {
        super();
        this.Delivery = Delivery;
        this.Range = (Range$ | 0);
        this.Damage = (Damage | 0);
        this.RecoveryTicks = (RecoveryTicks | 0);
        this.AreaShape = AreaShape;
    }
}

export function CombatProfile_$reflection() {
    return record_type("SIR.Simulation.CombatProfile", [], CombatProfile, () => [["Delivery", CombatDelivery_$reflection()], ["Range", int32_type], ["Damage", int32_type], ["RecoveryTicks", int32_type], ["AreaShape", option_type(AreaShape_$reflection())]]);
}

export class MovementProfile extends Record {
    constructor(SpeedMillimetersPerSecond, CellMillimeters) {
        super();
        this.SpeedMillimetersPerSecond = (SpeedMillimetersPerSecond | 0);
        this.CellMillimeters = (CellMillimeters | 0);
    }
}

export function MovementProfile_$reflection() {
    return record_type("SIR.Simulation.MovementProfile", [], MovementProfile, () => [["SpeedMillimetersPerSecond", int32_type], ["CellMillimeters", int32_type]]);
}

export class EngagementState extends Record {
    constructor(Target, Profile, RecoveryTicksRemaining) {
        super();
        this.Target = Target;
        this.Profile = Profile;
        this.RecoveryTicksRemaining = (RecoveryTicksRemaining | 0);
    }
}

export function EngagementState_$reflection() {
    return record_type("SIR.Simulation.EngagementState", [], EngagementState, () => [["Target", CombatTarget_$reflection()], ["Profile", CombatProfile_$reflection()], ["RecoveryTicksRemaining", int32_type]]);
}

export class MapScaleUnit extends Record {
    constructor(Id, Side, ClassId, Cell, Size, Health, Controller, Script, ScriptIndex, BodyFacing, AttentionDirection) {
        super();
        this.Id = (Id | 0);
        this.Side = (Side | 0);
        this.ClassId = ClassId;
        this.Cell = Cell;
        this.Size = (Size | 0);
        this.Health = (Health | 0);
        this.Controller = Controller;
        this.Script = Script;
        this.ScriptIndex = (ScriptIndex | 0);
        this.BodyFacing = BodyFacing;
        this.AttentionDirection = AttentionDirection;
    }
}

export function MapScaleUnit_$reflection() {
    return record_type("SIR.Simulation.MapScaleUnit", [], MapScaleUnit, () => [["Id", int32_type], ["Side", int32_type], ["ClassId", string_type], ["Cell", Cell_$reflection()], ["Size", int32_type], ["Health", int32_type], ["Controller", MapScaleController_$reflection()], ["Script", list_type(Direction8_$reflection())], ["ScriptIndex", int32_type], ["BodyFacing", Direction8_$reflection()], ["AttentionDirection", Direction8_$reflection()]]);
}

export class MapScaleBoard extends Record {
    constructor(Width, Height, Terrain, Edges) {
        super();
        this.Width = (Width | 0);
        this.Height = (Height | 0);
        this.Terrain = Terrain;
        this.Edges = Edges;
    }
}

export function MapScaleBoard_$reflection() {
    return record_type("SIR.Simulation.MapScaleBoard", [], MapScaleBoard, () => [["Width", int32_type], ["Height", int32_type], ["Terrain", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [Cell_$reflection(), MapScaleTerrain_$reflection()])], ["Edges", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [tuple_type(int32_type, int32_type, MapScaleEdgeDirection_$reflection()), MapScaleEdgeKind_$reflection()])]]);
}

export class MovementProgress extends Record {
    constructor(Origin, Destination, ProgressMillimeters, CostMillimeters) {
        super();
        this.Origin = Origin;
        this.Destination = Destination;
        this.ProgressMillimeters = (ProgressMillimeters | 0);
        this.CostMillimeters = (CostMillimeters | 0);
    }
}

export function MovementProgress_$reflection() {
    return record_type("SIR.Simulation.MovementProgress", [], MovementProgress, () => [["Origin", Cell_$reflection()], ["Destination", Cell_$reflection()], ["ProgressMillimeters", int32_type], ["CostMillimeters", int32_type]]);
}

export class MapScaleState extends Record {
    constructor(Tick, Board, Units, MovementCreditsMillimeters, MovementProgress, MovementIntents, PlannedRoutes, Engagements) {
        super();
        this.Tick = (Tick | 0);
        this.Board = Board;
        this.Units = Units;
        this.MovementCreditsMillimeters = MovementCreditsMillimeters;
        this.MovementProgress = MovementProgress;
        this.MovementIntents = MovementIntents;
        this.PlannedRoutes = PlannedRoutes;
        this.Engagements = Engagements;
    }
}

export function MapScaleState_$reflection() {
    return record_type("SIR.Simulation.MapScaleState", [], MapScaleState, () => [["Tick", int32_type], ["Board", MapScaleBoard_$reflection()], ["Units", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, MapScaleUnit_$reflection()])], ["MovementCreditsMillimeters", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, int32_type])], ["MovementProgress", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, MovementProgress_$reflection()])], ["MovementIntents", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, Direction8_$reflection()])], ["PlannedRoutes", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, list_type(Cell_$reflection())])], ["Engagements", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, EngagementState_$reflection()])]]);
}

export class MovementBlocker extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["OutsideBoard", "BlockedTerrainCollision", "BlockingEdge", "OccupiedCell", "DestinationConflict", "CrossingConflict"];
    }
}

export function MovementBlocker_$reflection() {
    return union_type("SIR.Simulation.MovementBlocker", [], MovementBlocker, () => [[["Item", Cell_$reflection()]], [["Item", Cell_$reflection()]], [["Item1", Cell_$reflection()], ["Item2", Cell_$reflection()]], [["Item1", Cell_$reflection()], ["unitId", int32_type]], [["Item", Cell_$reflection()]], [["otherUnitId", int32_type]]]);
}

export class MapScaleEvent extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["MovementPrepared", "UnitMoved", "MovementRejected", "UnitHeld", "AttackRecovering", "AttackResolved"];
    }
}

export function MapScaleEvent_$reflection() {
    return union_type("SIR.Simulation.MapScaleEvent", [], MapScaleEvent, () => [[["unitId", int32_type], ["origin", Cell_$reflection()], ["destination", Cell_$reflection()], ["progress", int32_type], ["cost", int32_type]], [["unitId", int32_type], ["origin", Cell_$reflection()], ["destination", Cell_$reflection()], ["distance", int32_type], ["cost", int32_type]], [["unitId", int32_type], ["destination", Cell_$reflection()], ["blocker", MovementBlocker_$reflection()]], [["unitId", int32_type], ["reason", string_type]], [["unitId", int32_type], ["ticksRemaining", int32_type]], [["sourceUnitId", int32_type], ["target", CombatTarget_$reflection()], ["delivery", CombatDelivery_$reflection()], ["damage", int32_type]]]);
}

export class MapScalePhase extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["CollectPhase", "ValidatePhase", "ResolvePhase", "CommitPhase"];
    }
    static CollectPhase = new MapScalePhase(0, []);
    static ValidatePhase = new MapScalePhase(1, []);
    static ResolvePhase = new MapScalePhase(2, []);
    static CommitPhase = new MapScalePhase(3, []);
}

export function MapScalePhase_$reflection() {
    return union_type("SIR.Simulation.MapScalePhase", [], MapScalePhase, () => [[], [], [], []]);
}

export class MapScaleCheckpoint extends Record {
    constructor(Tick, Phase, State, Events) {
        super();
        this.Tick = (Tick | 0);
        this.Phase = Phase;
        this.State = State;
        this.Events = Events;
    }
}

export function MapScaleCheckpoint_$reflection() {
    return record_type("SIR.Simulation.MapScaleCheckpoint", [], MapScaleCheckpoint, () => [["Tick", int32_type], ["Phase", MapScalePhase_$reflection()], ["State", MapScaleState_$reflection()], ["Events", list_type(MapScaleEvent_$reflection())]]);
}

export class MapScaleTickResult extends Record {
    constructor(State, Events, Checkpoints) {
        super();
        this.State = State;
        this.Events = Events;
        this.Checkpoints = Checkpoints;
    }
}

export function MapScaleTickResult_$reflection() {
    return record_type("SIR.Simulation.MapScaleTickResult", [], MapScaleTickResult, () => [["State", MapScaleState_$reflection()], ["Events", list_type(MapScaleEvent_$reflection())], ["Checkpoints", list_type(MapScaleCheckpoint_$reflection())]]);
}

export class MapScaleDivergence extends Record {
    constructor(Tick, Phase, ByteOffset, Expected, Actual) {
        super();
        this.Tick = (Tick | 0);
        this.Phase = Phase;
        this.ByteOffset = (ByteOffset | 0);
        this.Expected = Expected;
        this.Actual = Actual;
    }
}

export function MapScaleDivergence_$reflection() {
    return record_type("SIR.Simulation.MapScaleDivergence", [], MapScaleDivergence, () => [["Tick", int32_type], ["Phase", MapScalePhase_$reflection()], ["ByteOffset", int32_type], ["Expected", uint8_type], ["Actual", uint8_type]]);
}

export class RouteResult extends Record {
    constructor(Route, DistanceMillimeters, MovementCostMillimeters) {
        super();
        this.Route = Route;
        this.DistanceMillimeters = (DistanceMillimeters | 0);
        this.MovementCostMillimeters = (MovementCostMillimeters | 0);
    }
}

export function RouteResult_$reflection() {
    return record_type("SIR.Simulation.RouteResult", [], RouteResult, () => [["Route", list_type(Cell_$reflection())], ["DistanceMillimeters", int32_type], ["MovementCostMillimeters", int32_type]]);
}

export function MapScale_cell(column, row) {
    return new Cell_1(column, row);
}

function MapScale_directionDelta(direction) {
    switch (direction.tag) {
        case 1:
            return [1, -1];
        case 2:
            return [1, 0];
        case 3:
            return [1, 1];
        case 4:
            return [0, 1];
        case 5:
            return [-1, 1];
        case 6:
            return [-1, 0];
        case 7:
            return [-1, -1];
        default:
            return [0, -1];
    }
}

function MapScale_sign(value) {
    if (value < 0) {
        return -1;
    }
    else if (value > 0) {
        return 1;
    }
    else {
        return 0;
    }
}

export function MapScale_footprint(size, position) {
    return toArray(delay(() => collect((row) => map((column) => MapScale_cell(column, row), rangeDouble(position.Col, 1, (position.Col + size) - 1)), rangeDouble(position.Row, 1, (position.Row + size) - 1))));
}

export function MapScale_stepDistance(origin, destination) {
    if ((origin.Col !== destination.Col) && (origin.Row !== destination.Row)) {
        return 354;
    }
    else {
        return 250;
    }
}

export function MapScale_movementProfileFor(classId) {
    let matchValue;
    return new MovementProfile((matchValue = classId.trim().toLowerCase(), (matchValue === "goblin") ? 2000 : ((matchValue === "rifleman") ? 1500 : ((matchValue === "orc") ? 1200 : ((matchValue === "troll") ? 1000 : ((matchValue === "observation-drone") ? 3000 : 1500))))), 250);
}

export function MapScale_combatProfileFor(classId) {
    const matchValue = classId.trim().toLowerCase();
    switch (matchValue) {
        case "rifleman":
            return new CombatProfile(CombatDelivery.ProjectileDelivery, 16, 12, 10, undefined);
        case "troll":
            return new CombatProfile(CombatDelivery.MeleeDelivery, 2, 18, 20, undefined);
        case "orc":
            return new CombatProfile(CombatDelivery.MeleeDelivery, 2, 5, 20, undefined);
        case "goblin":
            return new CombatProfile(CombatDelivery.MeleeDelivery, 2, 2, 16, undefined);
        default:
            return new CombatProfile(CombatDelivery.MeleeDelivery, 2, 1, 20, undefined);
    }
}

export function MapScale_movementCost(board, unit, origin, destination) {
    let array;
    const distance = MapScale_stepDistance(origin, destination) | 0;
    if ((array = MapScale_footprint(unit.Size, destination), array.some((address) => equals(tryFind(address, board.Terrain), MapScaleTerrain.RoughTerrain)))) {
        return ~~((distance * 3) / 2) | 0;
    }
    else {
        return distance | 0;
    }
}

function MapScale_edgeBlocks(board, key_, key__1, key__2) {
    return exists((_arg) => {
        let matchResult;
        switch (_arg.tag) {
            case 0:
            case 2: {
                matchResult = 1;
                break;
            }
            default:
                if (_arg.fields[0]) {
                    matchResult = 0;
                }
                else {
                    matchResult = 1;
                }
        }
        switch (matchResult) {
            case 0:
                return false;
            default:
                return true;
        }
    }, toArray_1(tryFind([key_, key__1, key__2], board.Edges)));
}

function MapScale_crossedEdges(unit, destination) {
    const columnDelta = (destination.Col - unit.Cell.Col) | 0;
    const rowDelta = (destination.Row - unit.Cell.Row) | 0;
    return append((columnDelta > 0) ? toArray(delay(() => map((row) => [(unit.Cell.Col + unit.Size) - 1, row, MapScaleEdgeDirection.EastEdge], rangeDouble(unit.Cell.Row, 1, (unit.Cell.Row + unit.Size) - 1)))) : ((columnDelta < 0) ? toArray(delay(() => map((row_1) => [unit.Cell.Col - 1, row_1, MapScaleEdgeDirection.EastEdge], rangeDouble(unit.Cell.Row, 1, (unit.Cell.Row + unit.Size) - 1)))) : []), (rowDelta > 0) ? toArray(delay(() => map((column) => [column, (unit.Cell.Row + unit.Size) - 1, MapScaleEdgeDirection.SouthEdge], rangeDouble(unit.Cell.Col, 1, (unit.Cell.Col + unit.Size) - 1)))) : ((rowDelta < 0) ? toArray(delay(() => map((column_1) => [column_1, unit.Cell.Row - 1, MapScaleEdgeDirection.SouthEdge], rangeDouble(unit.Cell.Col, 1, (unit.Cell.Col + unit.Size) - 1)))) : []));
}

function MapScale_staticCollision(board, units, unit, destination, ignoreOccupancy) {
    const cells = MapScale_footprint(unit.Size, destination);
    const matchValue = tryFind_1((p) => {
        if (((p.Col < 0) ? true : (p.Row < 0)) ? true : (p.Col >= board.Width)) {
            return true;
        }
        else {
            return p.Row >= board.Height;
        }
    }, cells);
    if (matchValue == null) {
        const matchValue_1 = tryFind_1((p_1) => equals(tryFind(p_1, board.Terrain), MapScaleTerrain.BlockedTerrain), cells);
        if (matchValue_1 == null) {
            const matchValue_2 = tryFind_1((tupledArg) => MapScale_edgeBlocks(board, tupledArg[0], tupledArg[1], tupledArg[2]), MapScale_crossedEdges(unit, destination));
            if (matchValue_2 == null) {
                if (!ignoreOccupancy) {
                    return tryPick((tupledArg_2) => {
                        const other = tupledArg_2[1];
                        const occupied = ofArray(MapScale_footprint(other.Size, other.Cell), {
                            Compare: (x_1, y_1) => (compare(x_1, y_1) | 0),
                        });
                        const option_1 = tryFind_1((p_2) => contains(p_2, occupied), cells);
                        if (option_1 != null) {
                            return new MovementBlocker(/* OccupiedCell */ 3, [option_1, tupledArg_2[0]]);
                        }
                        else {
                            return undefined;
                        }
                    }, sortBy((tuple) => (tuple[0] | 0), filter((tupledArg_1) => (tupledArg_1[0] !== unit.Id), toSeq(units)), {
                        Compare: (x, y) => (comparePrimitives(x, y) | 0),
                    }));
                }
                else {
                    return undefined;
                }
            }
            else {
                const row = matchValue_2[1] | 0;
                const direction = matchValue_2[2];
                const column = matchValue_2[0] | 0;
                const patternInput = (direction.tag === 1) ? [MapScale_cell(column, row), MapScale_cell(column, row + 1)] : [MapScale_cell(column, row), MapScale_cell(column + 1, row)];
                return new MovementBlocker(/* BlockingEdge */ 2, [patternInput[0], patternInput[1]]);
            }
        }
        else {
            return new MovementBlocker(/* BlockedTerrainCollision */ 1, [matchValue_1]);
        }
    }
    else {
        return new MovementBlocker(/* OutsideBoard */ 0, [matchValue]);
    }
}

export function MapScale_movementCollision(board, units, unit, destination) {
    const direct = MapScale_staticCollision(board, units, unit, destination, false);
    const dc = (destination.Col - unit.Cell.Col) | 0;
    const dr = (destination.Row - unit.Cell.Row) | 0;
    if (((direct == null) && (dc !== 0)) && (dr !== 0)) {
        const horizontal = MapScale_cell(destination.Col, unit.Cell.Row);
        const vertical = MapScale_cell(unit.Cell.Col, destination.Row);
        const matchValue = MapScale_staticCollision(board, units, unit, horizontal, false);
        const matchValue_1 = MapScale_staticCollision(board, units, unit, vertical, false);
        let matchResult, collision, first;
        if (matchValue != null) {
            if (matchValue_1 != null) {
                matchResult = 2;
                first = matchValue;
            }
            else {
                matchResult = 1;
                collision = matchValue;
            }
        }
        else if (matchValue_1 != null) {
            matchResult = 1;
            collision = matchValue_1;
        }
        else {
            matchResult = 0;
        }
        switch (matchResult) {
            case 0:
                return undefined;
            case 1:
                return collision;
            default:
                return first;
        }
    }
    else {
        return direct;
    }
}

function MapScale_staticMovementCollision(board, unit, destination) {
    const direct = MapScale_staticCollision(board, empty({
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }), unit, destination, true);
    const dc = (destination.Col - unit.Cell.Col) | 0;
    const dr = (destination.Row - unit.Cell.Row) | 0;
    if (((direct == null) && (dc !== 0)) && (dr !== 0)) {
        const horizontal = MapScale_cell(destination.Col, unit.Cell.Row);
        const vertical = MapScale_cell(unit.Cell.Col, destination.Row);
        const matchValue = MapScale_staticCollision(board, empty({
            Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
        }), unit, horizontal, true);
        const matchValue_1 = MapScale_staticCollision(board, empty({
            Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
        }), unit, vertical, true);
        let matchResult, collision, first;
        if (matchValue != null) {
            if (matchValue_1 != null) {
                matchResult = 2;
                first = matchValue;
            }
            else {
                matchResult = 1;
                collision = matchValue;
            }
        }
        else if (matchValue_1 != null) {
            matchResult = 1;
            collision = matchValue_1;
        }
        else {
            matchResult = 0;
        }
        switch (matchResult) {
            case 0:
                return undefined;
            case 1:
                return collision;
            default:
                return first;
        }
    }
    else {
        return direct;
    }
}

const MapScale_pathNeighbors = [[0, -1], [1, 0], [0, 1], [-1, 0], [1, -1], [1, 1], [-1, 1], [-1, -1]];

function MapScale_heuristic(origin, destination) {
    const column = Math.abs(destination.Col - origin.Col) | 0;
    const row = Math.abs(destination.Row - origin.Row) | 0;
    const diagonal = min(column, row) | 0;
    return ((diagonal * 354) + ((max(column, row) - diagonal) * 250)) | 0;
}

export function MapScale_tryFindPath(board, units, unit, destination) {
    const origin = unit.Cell;
    let frontier = singleton([0, 0, origin.Row, origin.Col, origin]);
    let costs = ofList(singleton([origin, 0]), {
        Compare: (x, y) => (compare(x, y) | 0),
    });
    let previous = empty({
        Compare: (x_1, y_1) => (compare(x_1, y_1) | 0),
    });
    let result = undefined;
    while ((result == null) && !isEmpty(frontier)) {
        const patternInput = min_1(frontier, {
            Compare: (x_2, y_2) => (compareArrays(x_2, y_2) | 0),
        });
        const current = patternInput[4];
        frontier = filter_1((tupledArg) => !equals(tupledArg[4], current), frontier);
        if (equals(current, destination)) {
            let cursor = current;
            let route = empty_1();
            while (!equals(cursor, origin)) {
                route = cons(cursor, route);
                cursor = find(cursor, previous);
            }
            let patternInput_1;
            const tupledArg_2 = fold((tupledArg_1, next) => {
                const prior = tupledArg_1[2];
                const probe = new MapScaleUnit(unit.Id, unit.Side, unit.ClassId, prior, unit.Size, unit.Health, unit.Controller, unit.Script, unit.ScriptIndex, unit.BodyFacing, unit.AttentionDirection);
                return [tupledArg_1[0] + MapScale_stepDistance(prior, next), tupledArg_1[1] + MapScale_movementCost(board, probe, prior, next), next];
            }, [0, 0, origin], route);
            patternInput_1 = [tupledArg_2[0], tupledArg_2[1]];
            result = (new RouteResult(route, patternInput_1[0], patternInput_1[1]));
        }
        else {
            const probe_1 = new MapScaleUnit(unit.Id, unit.Side, unit.ClassId, current, unit.Size, unit.Health, unit.Controller, unit.Script, unit.ScriptIndex, unit.BodyFacing, unit.AttentionDirection);
            for (let idx = 0; idx <= (MapScale_pathNeighbors.length - 1); idx++) {
                const forLoopVar = item(idx, MapScale_pathNeighbors);
                const next_1 = MapScale_cell(current.Col + forLoopVar[0], current.Row + forLoopVar[1]);
                if (MapScale_movementCollision(board, units, probe_1, next_1) == null) {
                    const nextCost = (patternInput[1] + MapScale_movementCost(board, probe_1, current, next_1)) | 0;
                    if (nextCost < defaultArg(tryFind(next_1, costs), 2147483647)) {
                        costs = add(next_1, nextCost, costs);
                        previous = add(next_1, current, previous);
                        frontier = cons([nextCost + MapScale_heuristic(next_1, destination), nextCost, next_1.Row, next_1.Col, next_1], frontier);
                    }
                }
            }
        }
    }
    return result;
}

function MapScale_axisGap(firstStart, firstSize, secondStart, secondSize) {
    const firstEnd = ((firstStart + firstSize) - 1) | 0;
    const secondEnd = ((secondStart + secondSize) - 1) | 0;
    if (firstEnd < secondStart) {
        return (secondStart - firstEnd) | 0;
    }
    else if (secondEnd < firstStart) {
        return (firstStart - secondEnd) | 0;
    }
    else {
        return 0;
    }
}

export function MapScale_footprintDistance(first, second) {
    return max(MapScale_axisGap(first.Cell.Col, first.Size, second.Cell.Col, second.Size), MapScale_axisGap(first.Cell.Row, first.Size, second.Cell.Row, second.Size)) | 0;
}

function MapScale_directRoute(origin, destination) {
    let column = origin.Col;
    let row = origin.Row;
    return toArray(delay(() => enumerateWhile(() => ((column !== destination.Col) ? true : (row !== destination.Row)), delay(() => {
        column = ((column + MapScale_sign(destination.Col - column)) | 0);
        row = ((row + MapScale_sign(destination.Row - row)) | 0);
        return singleton_1(MapScale_cell(column, row));
    }))));
}

function MapScale_projectilePathClear(attacker, target, board) {
    const origin = MapScale_cell(attacker.Cell.Col + ~~(attacker.Size / 2), attacker.Cell.Row + ~~(attacker.Size / 2));
    let previous = origin;
    let clear = true;
    const arr = MapScale_directRoute(origin, MapScale_cell(target.Cell.Col + ~~(target.Size / 2), target.Cell.Row + ~~(target.Size / 2)));
    for (let idx = 0; idx <= (arr.length - 1); idx++) {
        const current = item(idx, arr);
        if (clear) {
            const matchValue = (current.Col - previous.Col) | 0;
            const dr = (current.Row - previous.Row) | 0;
            const dc = matchValue | 0;
            const edges = toList(delay(() => append_1((dc > 0) ? singleton_1([previous.Col, previous.Row, MapScaleEdgeDirection.EastEdge]) : ((dc < 0) ? singleton_1([current.Col, previous.Row, MapScaleEdgeDirection.EastEdge]) : empty_2()), delay(() => ((dr > 0) ? singleton_1([previous.Col, previous.Row, MapScaleEdgeDirection.SouthEdge]) : ((dr < 0) ? singleton_1([previous.Col, current.Row, MapScaleEdgeDirection.SouthEdge]) : empty_2()))))));
            clear = (!exists_1((tupledArg) => MapScale_edgeBlocks(board, tupledArg[0], tupledArg[1], tupledArg[2]), edges) && !equals(tryFind(current, board.Terrain), MapScaleTerrain.BlockedTerrain));
            previous = current;
        }
    }
    return clear;
}

export function MapScale_canAttack(attacker, target, profile, board) {
    if (MapScale_footprintDistance(attacker, target) <= profile.Range) {
        if (!equals(profile.Delivery, CombatDelivery.ProjectileDelivery)) {
            return true;
        }
        else {
            return MapScale_projectilePathClear(attacker, target, board);
        }
    }
    else {
        return false;
    }
}

class MapScale_CollectedIntent extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["MoveIntent", "AttackIntent", "HoldIntent"];
    }
}

function MapScale_CollectedIntent_$reflection() {
    return union_type("SIR.Simulation.MapScale.CollectedIntent", [], MapScale_CollectedIntent, () => [[["Item1", MapScaleUnit_$reflection()], ["Item2", Cell_$reflection()], ["Item3", string_type]], [["Item1", MapScaleUnit_$reflection()], ["Item2", MapScaleUnit_$reflection()], ["Item3", CombatProfile_$reflection()]], [["Item1", MapScaleUnit_$reflection()], ["Item2", string_type]]]);
}

class MapScale_ValidatedMove extends Record {
    constructor(Unit, Destination, Verb, Available, Required) {
        super();
        this.Unit = Unit;
        this.Destination = Destination;
        this.Verb = Verb;
        this.Available = (Available | 0);
        this.Required = (Required | 0);
    }
}

function MapScale_ValidatedMove_$reflection() {
    return record_type("SIR.Simulation.MapScale.ValidatedMove", [], MapScale_ValidatedMove, () => [["Unit", MapScaleUnit_$reflection()], ["Destination", Cell_$reflection()], ["Verb", string_type], ["Available", int32_type], ["Required", int32_type]]);
}

function MapScale_nearestHostile(unit, units) {
    return tryHead(sortBy((other_1) => [MapScale_footprintDistance(unit, other_1), other_1.Id], filter((other) => {
        if ((other.Id !== unit.Id) && (other.Side !== unit.Side)) {
            return other.Health > 0;
        }
        else {
            return false;
        }
    }, map((tuple) => tuple[1], toSeq(units))), {
        Compare: (x, y) => (compareArrays(x, y) | 0),
    }));
}

function MapScale_nextApproachStep(state, unit, target) {
    MapScale_combatProfileFor(unit.ClassId);
    const option_4 = tryHead_1(sort(choose((tupledArg) => {
        let option_1;
        const destination = MapScale_cell(unit.Cell.Col + tupledArg[0], unit.Cell.Row + tupledArg[1]);
        return defaultArg((option_1 = MapScale_movementCollision(state.Board, state.Units, unit, destination), (option_1 != null) ? some(undefined) : undefined), [MapScale_footprintDistance(new MapScaleUnit(unit.Id, unit.Side, unit.ClassId, destination, unit.Size, unit.Health, unit.Controller, unit.Script, unit.ScriptIndex, unit.BodyFacing, unit.AttentionDirection), target), MapScale_movementCost(state.Board, unit, unit.Cell, destination), destination]);
    }, MapScale_pathNeighbors), {
        Compare: (x, y) => (compareArrays(x, y) | 0),
    }));
    if (option_4 != null) {
        return option_4[2];
    }
    else {
        return undefined;
    }
}

function MapScale_collect(state) {
    return choose_1((unit) => {
        if (unit.Health <= 0) {
            return undefined;
        }
        else {
            const matchValue = tryFind(unit.Id, state.MovementProgress);
            let matchResult, progress_1;
            if (matchValue != null) {
                if (equals(matchValue.Origin, unit.Cell)) {
                    matchResult = 0;
                    progress_1 = matchValue;
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
                    return new MapScale_CollectedIntent(/* MoveIntent */ 0, [unit, progress_1.Destination, "along its locked approach segment"]);
                default: {
                    const matchValue_1 = unit.Controller;
                    switch (matchValue_1.tag) {
                        case 1:
                            if (isEmpty(unit.Script)) {
                                return new MapScale_CollectedIntent(/* HoldIntent */ 2, [unit, "has no script"]);
                            }
                            else {
                                const direction_1 = item_1(unit.ScriptIndex % length(unit.Script), unit.Script);
                                const patternInput_1 = MapScale_directionDelta(direction_1);
                                return new MapScale_CollectedIntent(/* MoveIntent */ 0, [unit, MapScale_cell(unit.Cell.Col + patternInput_1[0], unit.Cell.Row + patternInput_1[1]), "script " + toString(direction_1)]);
                            }
                        case 2: {
                            const matchValue_5 = MapScale_nearestHostile(unit, state.Units);
                            if (matchValue_5 != null) {
                                const target = matchValue_5;
                                const profile = MapScale_combatProfileFor(unit.ClassId);
                                if (MapScale_canAttack(unit, target, profile, state.Board)) {
                                    return new MapScale_CollectedIntent(/* AttackIntent */ 1, [unit, target, profile]);
                                }
                                else {
                                    const matchValue_6 = MapScale_nextApproachStep(state, unit, target);
                                    if (matchValue_6 == null) {
                                        return new MapScale_CollectedIntent(/* HoldIntent */ 2, [unit, "holds; no collision-free approach exists"]);
                                    }
                                    else {
                                        return new MapScale_CollectedIntent(/* MoveIntent */ 0, [unit, matchValue_6, "toward its attack position"]);
                                    }
                                }
                            }
                            else {
                                return new MapScale_CollectedIntent(/* HoldIntent */ 2, [unit, "holds; no hostile is present"]);
                            }
                        }
                        default: {
                            const matchValue_2 = tryFind(unit.Id, state.PlannedRoutes);
                            const matchValue_3 = tryFind(unit.Id, state.MovementIntents);
                            let matchResult_1, next, direction;
                            if (matchValue_2 != null) {
                                if (!isEmpty(matchValue_2)) {
                                    matchResult_1 = 0;
                                    next = head(matchValue_2);
                                }
                                else if (matchValue_3 != null) {
                                    matchResult_1 = 1;
                                    direction = matchValue_3;
                                }
                                else {
                                    matchResult_1 = 2;
                                }
                            }
                            else if (matchValue_3 != null) {
                                matchResult_1 = 1;
                                direction = matchValue_3;
                            }
                            else {
                                matchResult_1 = 2;
                            }
                            switch (matchResult_1) {
                                case 0:
                                    return new MapScale_CollectedIntent(/* MoveIntent */ 0, [unit, next, "along its planned route"]);
                                case 1: {
                                    const patternInput = MapScale_directionDelta(direction);
                                    return new MapScale_CollectedIntent(/* MoveIntent */ 0, [unit, MapScale_cell(unit.Cell.Col + patternInput[0], unit.Cell.Row + patternInput[1]), toString(direction)]);
                                }
                                default:
                                    return new MapScale_CollectedIntent(/* HoldIntent */ 2, [unit, "awaits manual input"]);
                            }
                        }
                    }
                }
            }
        }
    }, sortBy_1((_arg) => (_arg.Id | 0), map_1((tuple) => tuple[1], toList_1(state.Units)), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }));
}

function MapScale_checkpoint(phase, state, events) {
    return new MapScaleCheckpoint(state.Tick + 1, phase, state, events);
}

/**
 * Executes explicit collect, validate, resolve, and atomic commit phases.
 */
export function MapScale_tick(state) {
    const earnedCredits = fold_1((credits, id, unit) => {
        if (unit.Health <= 0) {
            return credits;
        }
        else {
            const earned = ~~(MapScale_movementProfileFor(unit.ClassId).SpeedMillimetersPerSecond / 20) | 0;
            return add(id, min(1060, defaultArg(tryFind(id, credits), 0) + earned), credits);
        }
    }, state.MovementCreditsMillimeters, state.Units);
    const collected = MapScale_collect(state);
    const collectState = new MapScaleState(state.Tick, state.Board, state.Units, earnedCredits, state.MovementProgress, state.MovementIntents, state.PlannedRoutes, state.Engagements);
    const collectCheckpoint = MapScale_checkpoint(MapScalePhase.CollectPhase, collectState, empty_1());
    const patternInput = fold((tupledArg, intent) => {
        const moves = tupledArg[0];
        const events = tupledArg[1];
        switch (intent.tag) {
            case 1:
                return [moves, events];
            case 0: {
                const unit_2 = intent.fields[0];
                const destination = intent.fields[1];
                const available = find(unit_2.Id, earnedCredits) | 0;
                const required = MapScale_movementCost(state.Board, unit_2, unit_2.Cell, destination) | 0;
                const matchValue = MapScale_staticMovementCollision(state.Board, unit_2, destination);
                if (matchValue == null) {
                    if (available < required) {
                        return [moves, cons(new MapScaleEvent(/* MovementPrepared */ 0, [unit_2.Id, unit_2.Cell, destination, available, required]), events)];
                    }
                    else {
                        return [cons(new MapScale_ValidatedMove(unit_2, destination, intent.fields[2], available, required), moves), events];
                    }
                }
                else {
                    return [moves, cons(new MapScaleEvent(/* MovementRejected */ 2, [unit_2.Id, destination, matchValue]), events)];
                }
            }
            default:
                return [moves, cons(new MapScaleEvent(/* UnitHeld */ 3, [intent.fields[0].Id, intent.fields[1]]), events)];
        }
    }, [empty_1(), empty_1()], collected);
    const validatedMoves_1 = reverse(patternInput[0]);
    const validationEvents_1 = reverse(patternInput[1]);
    const rejectedIds = choose_1((_arg) => {
        if (_arg.tag === 2) {
            return _arg.fields[0];
        }
        else {
            return undefined;
        }
    }, validationEvents_1);
    const validatedState = new MapScaleState(collectState.Tick, collectState.Board, collectState.Units, fold((credits_1, id_2) => add(id_2, 0, credits_1), collectState.MovementCreditsMillimeters, rejectedIds), fold((progress, id_3) => remove(id_3, progress), collectState.MovementProgress, rejectedIds), collectState.MovementIntents, collectState.PlannedRoutes, collectState.Engagements);
    const validateCheckpoint = MapScale_checkpoint(MapScalePhase.ValidatePhase, validatedState, validationEvents_1);
    const destinationCounts = ofList(List_countBy((x) => x, collect_1((move) => ofArray_1(MapScale_footprint(move.Unit.Size, move.Destination)), validatedMoves_1), {
        Equals: equals,
        GetHashCode: (x_1) => (safeHash(x_1) | 0),
    }), {
        Compare: (x_2, y_1) => (compare(x_2, y_1) | 0),
    });
    const conflicts = ofList(choose_1((move_1) => {
        const destinationConflict = tryFind_1((p) => (find(p, destinationCounts) > 1), MapScale_footprint(move_1.Unit.Size, move_1.Destination));
        if (destinationConflict == null) {
            const crossing = tryFind_2((other) => {
                let set$_1, set$_4;
                if ((other.Unit.Id !== move_1.Unit.Id) && !isEmpty_1((set$_1 = ofArray(MapScale_footprint(move_1.Unit.Size, move_1.Destination), {
                    Compare: (x_3, y_2) => (compare(x_3, y_2) | 0),
                }), intersect(ofArray(MapScale_footprint(other.Unit.Size, other.Unit.Cell), {
                    Compare: (x_4, y_3) => (compare(x_4, y_3) | 0),
                }), set$_1)))) {
                    return !isEmpty_1((set$_4 = ofArray(MapScale_footprint(other.Unit.Size, other.Destination), {
                        Compare: (x_5, y_4) => (compare(x_5, y_4) | 0),
                    }), intersect(ofArray(MapScale_footprint(move_1.Unit.Size, move_1.Unit.Cell), {
                        Compare: (x_6, y_5) => (compare(x_6, y_5) | 0),
                    }), set$_4)));
                }
                else {
                    return false;
                }
            }, validatedMoves_1);
            if (crossing == null) {
                const destinationCells = ofArray(MapScale_footprint(move_1.Unit.Size, move_1.Destination), {
                    Compare: (x_7, y_6) => (compare(x_7, y_6) | 0),
                });
                return tryPick((tupledArg_2) => {
                    const id_5 = tupledArg_2[0] | 0;
                    const other_2 = tupledArg_2[1];
                    const occupied = ofArray(MapScale_footprint(other_2.Size, other_2.Cell), {
                        Compare: (x_8, y_7) => (compare(x_8, y_7) | 0),
                    });
                    if (isEmpty_1(intersect(destinationCells, occupied))) {
                        return undefined;
                    }
                    else if (exists_1((candidate) => {
                        if (candidate.Unit.Id === id_5) {
                            return isEmpty_1(intersect(ofArray(MapScale_footprint(candidate.Unit.Size, candidate.Destination), {
                                Compare: (x_9, y_8) => (compare(x_9, y_8) | 0),
                            }), destinationCells));
                        }
                        else {
                            return false;
                        }
                    }, validatedMoves_1)) {
                        return undefined;
                    }
                    else {
                        const option_2 = tryHead_2(sort_1(toList_2(intersect(destinationCells, occupied)), {
                            Compare: (x_10, y_9) => (compare(x_10, y_9) | 0),
                        }));
                        if (option_2 != null) {
                            return [move_1.Unit.Id, new MovementBlocker(/* OccupiedCell */ 3, [option_2, id_5])];
                        }
                        else {
                            return undefined;
                        }
                    }
                }, filter((tupledArg_1) => (tupledArg_1[0] !== move_1.Unit.Id), toSeq(state.Units)));
            }
            else {
                return [move_1.Unit.Id, new MovementBlocker(/* CrossingConflict */ 5, [crossing.Unit.Id])];
            }
        }
        else {
            return [move_1.Unit.Id, new MovementBlocker(/* DestinationConflict */ 4, [destinationConflict])];
        }
    }, validatedMoves_1), {
        Compare: (x_11, y_10) => (comparePrimitives(x_11, y_10) | 0),
    });
    const resolvedMoves = filter_1((move_2) => !containsKey(move_2.Unit.Id, conflicts), validatedMoves_1);
    const conflictEvents = choose_1((move_3) => {
        const option_4 = tryFind(move_3.Unit.Id, conflicts);
        if (option_4 != null) {
            return new MapScaleEvent(/* MovementRejected */ 2, [move_3.Unit.Id, move_3.Destination, option_4]);
        }
        else {
            return undefined;
        }
    }, validatedMoves_1);
    const resolveEvents = append_2(validationEvents_1, conflictEvents);
    const resolvedState = new MapScaleState(validatedState.Tick, validatedState.Board, validatedState.Units, fold((credits_2, event) => {
        if (event.tag === 2) {
            return add(event.fields[0], 0, credits_2);
        }
        else {
            return credits_2;
        }
    }, validatedState.MovementCreditsMillimeters, conflictEvents), validatedState.MovementProgress, validatedState.MovementIntents, validatedState.PlannedRoutes, validatedState.Engagements);
    const resolveCheckpoint = MapScale_checkpoint(MapScalePhase.ResolvePhase, resolvedState, resolveEvents);
    const patternInput_1 = fold((tupledArg_3, move_4) => {
        const current_1 = tupledArg_3[0];
        let moved;
        const bind$0040 = move_4.Unit;
        moved = (new MapScaleUnit(bind$0040.Id, bind$0040.Side, bind$0040.ClassId, move_4.Destination, bind$0040.Size, bind$0040.Health, bind$0040.Controller, bind$0040.Script, equals(move_4.Unit.Controller, MapScaleController.ScriptedController) ? (move_4.Unit.ScriptIndex + 1) : move_4.Unit.ScriptIndex, bind$0040.BodyFacing, bind$0040.AttentionDirection));
        let routes;
        const matchValue_1 = tryFind(move_4.Unit.Id, current_1.PlannedRoutes);
        let matchResult, remaining_1;
        if (matchValue_1 == null) {
            matchResult = 2;
        }
        else if (!isEmpty(matchValue_1)) {
            if (!isEmpty(tail(matchValue_1))) {
                matchResult = 0;
                remaining_1 = tail(matchValue_1);
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
                routes = add(move_4.Unit.Id, remaining_1, current_1.PlannedRoutes);
                break;
            }
            case 1: {
                routes = remove(move_4.Unit.Id, current_1.PlannedRoutes);
                break;
            }
            default:
                routes = current_1.PlannedRoutes;
        }
        return [new MapScaleState(current_1.Tick, current_1.Board, add(moved.Id, moved, current_1.Units), add(moved.Id, move_4.Available - move_4.Required, current_1.MovementCreditsMillimeters), remove(moved.Id, current_1.MovementProgress), remove(moved.Id, current_1.MovementIntents), routes, current_1.Engagements), cons(new MapScaleEvent(/* UnitMoved */ 1, [moved.Id, move_4.Unit.Cell, move_4.Destination, MapScale_stepDistance(move_4.Unit.Cell, move_4.Destination), move_4.Required]), tupledArg_3[1])];
    }, [resolvedState, empty_1()], resolvedMoves);
    const movedState = patternInput_1[0];
    const afterProgress = new MapScaleState(movedState.Tick, movedState.Board, movedState.Units, movedState.MovementCreditsMillimeters, fold((progress_2, tupledArg_4) => add(tupledArg_4[0], tupledArg_4[1], progress_2), movedState.MovementProgress, choose_1((_arg_2) => {
        if (_arg_2.tag === 0) {
            return [_arg_2.fields[0], new MovementProgress(_arg_2.fields[1], _arg_2.fields[2], _arg_2.fields[3], _arg_2.fields[4])];
        }
        else {
            return undefined;
        }
    }, validationEvents_1)), movedState.MovementIntents, movedState.PlannedRoutes, movedState.Engagements);
    const attackIntents = choose_1((_arg_3) => {
        if (_arg_3.tag === 1) {
            return [_arg_3.fields[0], _arg_3.fields[1], _arg_3.fields[2]];
        }
        else {
            return undefined;
        }
    }, collected);
    const recovery = map_2((_arg_4, engagement) => (new EngagementState(engagement.Target, engagement.Profile, max(0, engagement.RecoveryTicksRemaining - 1))), state.Engagements);
    const patternInput_2 = fold((tupledArg_5, tupledArg_6) => {
        let option_6;
        const accepted = tupledArg_5[0];
        const events_2 = tupledArg_5[1];
        const attacker = tupledArg_6[0];
        const target = tupledArg_6[1];
        const profile_1 = tupledArg_6[2];
        const remaining_2 = defaultArg((option_6 = tryFind(attacker.Id, recovery), (option_6 != null) ? option_6.RecoveryTicksRemaining : undefined), 0) | 0;
        if (remaining_2 > 0) {
            return [accepted, cons(new MapScaleEvent(/* AttackRecovering */ 4, [attacker.Id, remaining_2]), events_2)];
        }
        else {
            return [add(attacker.Id, [target, profile_1], accepted), cons(new MapScaleEvent(/* AttackResolved */ 5, [attacker.Id, new CombatTarget(/* UnitTarget */ 0, [target.Id]), profile_1.Delivery, profile_1.Damage]), events_2)];
        }
    }, [empty({
        Compare: (x_12, y_11) => (comparePrimitives(x_12, y_11) | 0),
    }), empty_1()], attackIntents);
    const attacks = patternInput_2[0];
    const damageByTarget = ofList(map_1((tupledArg_8) => [tupledArg_8[0], sumBy((tuple_1) => (tuple_1[1] | 0), tupledArg_8[1], {
        GetZero: () => 0,
        Add: (x_14, y_13) => ((x_14 + y_13) | 0),
    })], List_groupBy((tuple) => (tuple[0] | 0), map_1((tupledArg_7) => {
        const _arg_7 = tupledArg_7[1];
        return [_arg_7[0].Id, _arg_7[1].Damage];
    }, toList_1(attacks)), {
        Equals: (x_13, y_12) => (x_13 === y_12),
        GetHashCode: (x_13) => (numberHash(x_13) | 0),
    })), {
        Compare: (x_15, y_14) => (comparePrimitives(x_15, y_14) | 0),
    });
    const committed = new MapScaleState(state.Tick + 1, afterProgress.Board, map_2((id_10, unit_3) => {
        let option_9;
        return defaultArg((option_9 = tryFind(id_10, damageByTarget), (option_9 != null) ? (new MapScaleUnit(unit_3.Id, unit_3.Side, unit_3.ClassId, unit_3.Cell, unit_3.Size, max(0, unit_3.Health - option_9), unit_3.Controller, unit_3.Script, unit_3.ScriptIndex, unit_3.BodyFacing, unit_3.AttentionDirection)) : undefined), unit_3);
    }, afterProgress.Units), afterProgress.MovementCreditsMillimeters, afterProgress.MovementProgress, afterProgress.MovementIntents, afterProgress.PlannedRoutes, fold_1((current_2, attackerId, tupledArg_9) => {
        const profile_3 = tupledArg_9[1];
        return add(attackerId, new EngagementState(new CombatTarget(/* UnitTarget */ 0, [tupledArg_9[0].Id]), profile_3, profile_3.RecoveryTicks), current_2);
    }, recovery, attacks));
    const allEvents = append_2(resolveEvents, append_2(reverse(patternInput_1[1]), reverse(patternInput_2[1])));
    return new MapScaleTickResult(committed, allEvents, ofArray_1([collectCheckpoint, validateCheckpoint, resolveCheckpoint, new MapScaleCheckpoint(committed.Tick, MapScalePhase.CommitPhase, committed, allEvents)]));
}

function MapScale_phaseCode(phase) {
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

function MapScale_eventCode(event) {
    switch (event.tag) {
        case 1:
            return 1;
        case 2:
            switch (event.fields[2].tag) {
                case 4:
                    return 2;
                case 5:
                    return 3;
                default:
                    return 4;
            }
        case 3:
            return 5;
        case 4:
            return 6;
        case 5:
            return 7;
        default:
            return 0;
    }
}

/**
 * Canonical diagnostic bytes for locating the first divergent map-scale phase.
 */
export function MapScale_checkpointBytes(checkpoint) {
    const int32 = int32LittleEndian;
    const units = collect_1((tupledArg) => {
        const unit = tupledArg[1];
        return ofArray_1([int32(tupledArg[0]), int32(unit.Cell.Col), int32(unit.Cell.Row), int32(unit.Size), int32(unit.Health), int32(unit.ScriptIndex)]);
    }, toList_1(checkpoint.State.Units));
    const credits = collect_1((tupledArg_1) => ofArray_1([int32(tupledArg_1[0]), int32(tupledArg_1[1])]), toList_1(checkpoint.State.MovementCreditsMillimeters));
    const events = collect_1((event) => {
        const identity = ((event.tag === 1) ? event.fields[0] : ((event.tag === 2) ? event.fields[0] : ((event.tag === 3) ? event.fields[0] : ((event.tag === 4) ? event.fields[0] : ((event.tag === 5) ? event.fields[0] : event.fields[0]))))) | 0;
        return ofArray_1([int32(MapScale_eventCode(event)), int32(identity)]);
    }, checkpoint.Events);
    return concatenate(append_2(ofArray_1([int32(checkpoint.Tick), int32(MapScale_phaseCode(checkpoint.Phase)), int32(FSharpMap__get_Count(checkpoint.State.Units))]), append_2(units, append_2(singleton(int32(FSharpMap__get_Count(checkpoint.State.MovementCreditsMillimeters))), append_2(credits, append_2(singleton(int32(length(checkpoint.Events))), events))))));
}

/**
 * Returns the earliest phase/byte mismatch between two map-scale runs.
 */
export function MapScale_firstCheckpointDivergence(expected, actual) {
    return tryPick_1((tupledArg) => {
        const expectedCheckpoint = tupledArg[0];
        const expectedBytes = MapScale_checkpointBytes(expectedCheckpoint);
        const actualBytes = MapScale_checkpointBytes(tupledArg[1]);
        if (equalsWith((x, y) => (x === y), expectedBytes, actualBytes)) {
            return undefined;
        }
        else {
            const limit = min(expectedBytes.length, actualBytes.length) | 0;
            const offset = defaultArg(tryFind_2((index) => (item(index, expectedBytes) !== item(index, actualBytes)), toList(rangeDouble(0, 1, limit - 1))), limit) | 0;
            return new MapScaleDivergence(expectedCheckpoint.Tick, expectedCheckpoint.Phase, offset, (offset < expectedBytes.length) ? item(offset, expectedBytes) : 0, (offset < actualBytes.length) ? item(offset, actualBytes) : 0);
        }
    }, zip(expected, actual));
}

