
import { toString, Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { MapEditorState, RevisionState as RevisionState_1, MapDefinition, EditorUnit, EditorCellAddress, MapController_$reflection, MapDefinition_$reflection, MapRevision_$reflection, EditorCellAddress_$reflection } from "./MapEditorTypes.js";
import { tuple_type, float64_type, list_type, bool_type, class_type, option_type, string_type, record_type, array_type, union_type, int32_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { RenderFrame, RenderEventVisual, OverlayVisual, Disclosure$1, OverlayScope, DisclosureLabel, RenderFrame_$reflection } from "./ReplayPresentation.js";
import { MapScale_tick, MapScale_movementCollision, MapScale_tryFindPath, MapScale_movementProfileFor, MapScale_combatProfileFor, MapScaleState, MapScaleBoard, MapScaleUnit, MapScaleEdgeDirection, MapScaleEdgeKind, MapScaleTerrain, MapScaleController, MapScale_cell, MapScaleCheckpoint_$reflection, MapScaleState_$reflection } from "../SIR.Simulation/MapScale.js";
import { Direction8_$reflection } from "../SIR.Domain/Orientation.js";
import { remove, toSeq, count, add, fold, isEmpty, containsKey, filter, forAll, find, tryFind, empty, map as map_2, toList, ofList } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { mapIndexed, ofArray, singleton, fold as fold_1, filter as filter_1, append, choose, toArray, length as length_1, empty as empty_1, map as map_1 } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { Exception, equals, int32ToString, comparePrimitives, compareArrays, compare } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { orElse, toArray as toArray_1, defaultArg } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { initial, frame as frame_2, parseScript, validationIssues } from "./MapEditor.js";
import { join } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { collect, append as append_1, map as map_3 } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { FSharpResult$2 } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { toList as toList_1, tryPick } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { min, max } from "../fable_modules/fable-library-js.5.13.0/Double.js";
import { rangeDouble } from "../fable_modules/fable-library-js.5.13.0/Range.js";
import { contains, ofList as ofList_1, empty as empty_2, singleton as singleton_1 } from "../fable_modules/fable-library-js.5.13.0/Set.js";
import { fromInt32, op_Modulus, toInt64_unchecked, toInt32_unchecked } from "../fable_modules/fable-library-js.5.13.0/BigInt.js";

export class SimulatorCollision extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["RouteClear", "OutsideMap", "BlockedTerrainAt", "BlockingEdgeAt", "OccupiedAt", "NoPathTo"];
    }
    static RouteClear = new SimulatorCollision(0, []);
}

export function SimulatorCollision_$reflection() {
    return union_type("SIR.Client.SimulatorCollision", [], SimulatorCollision, () => [[], [["Item", EditorCellAddress_$reflection()]], [["Item", EditorCellAddress_$reflection()]], [["Item1", EditorCellAddress_$reflection()], ["Item2", EditorCellAddress_$reflection()]], [["Item1", EditorCellAddress_$reflection()], ["unitId", int32_type]], [["Item", EditorCellAddress_$reflection()]]]);
}

export class SimulatorRoutePreview extends Record {
    constructor(UnitId, Origin, Destination, Distance, DistanceMillimeters, MovementCostMillimeters, Route, Collision) {
        super();
        this.UnitId = (UnitId | 0);
        this.Origin = Origin;
        this.Destination = Destination;
        this.Distance = (Distance | 0);
        this.DistanceMillimeters = (DistanceMillimeters | 0);
        this.MovementCostMillimeters = (MovementCostMillimeters | 0);
        this.Route = Route;
        this.Collision = Collision;
    }
}

export function SimulatorRoutePreview_$reflection() {
    return record_type("SIR.Client.SimulatorRoutePreview", [], SimulatorRoutePreview, () => [["UnitId", int32_type], ["Origin", EditorCellAddress_$reflection()], ["Destination", EditorCellAddress_$reflection()], ["Distance", int32_type], ["DistanceMillimeters", int32_type], ["MovementCostMillimeters", int32_type], ["Route", array_type(EditorCellAddress_$reflection())], ["Collision", SimulatorCollision_$reflection()]]);
}

export class PerspectivePreviewAvailability extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["PerspectivePreviewUnavailable", "AcceptedPerspectiveProjection"];
    }
}

export function PerspectivePreviewAvailability_$reflection() {
    return union_type("SIR.Client.PerspectivePreviewAvailability", [], PerspectivePreviewAvailability, () => [[["reason", string_type]], [["Item", RenderFrame_$reflection()]]]);
}

export class VisibilityOverlayAvailability extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["VisibilityOverlaysUnavailable", "SharedKernelVisibilityAvailable"];
    }
    static SharedKernelVisibilityAvailable = new VisibilityOverlayAvailability(1, []);
}

export function VisibilityOverlayAvailability_$reflection() {
    return union_type("SIR.Client.VisibilityOverlayAvailability", [], VisibilityOverlayAvailability, () => [[["reason", string_type]], []]);
}

export class SimulatorCombatDelivery extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["MeleeDelivery", "ProjectileDelivery", "LobbedAreaDelivery", "SpellAreaDelivery"];
    }
    static MeleeDelivery = new SimulatorCombatDelivery(0, []);
    static ProjectileDelivery = new SimulatorCombatDelivery(1, []);
    static LobbedAreaDelivery = new SimulatorCombatDelivery(2, []);
    static SpellAreaDelivery = new SimulatorCombatDelivery(3, []);
}

export function SimulatorCombatDelivery_$reflection() {
    return union_type("SIR.Client.SimulatorCombatDelivery", [], SimulatorCombatDelivery, () => [[], [], [], []]);
}

export class SimulatorAreaShape extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["BurstArea", "ConeArea", "RayArea", "RectangleArea"];
    }
}

export function SimulatorAreaShape_$reflection() {
    return union_type("SIR.Client.SimulatorAreaShape", [], SimulatorAreaShape, () => [[["radius", int32_type]], [["range", int32_type], ["angleDegrees", int32_type]], [["length", int32_type], ["width", int32_type]], [["width", int32_type], ["depth", int32_type]]]);
}

export class SimulatorCombatTarget extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["UnitCombatTarget", "AreaCombatTarget"];
    }
}

export function SimulatorCombatTarget_$reflection() {
    return union_type("SIR.Client.SimulatorCombatTarget", [], SimulatorCombatTarget, () => [[["unitId", int32_type]], [["origin", EditorCellAddress_$reflection()], ["shape", SimulatorAreaShape_$reflection()]]]);
}

export class SimulatorAttackProfile extends Record {
    constructor(Delivery, Range$, Damage, RecoveryTicks, AreaShape) {
        super();
        this.Delivery = Delivery;
        this.Range = (Range$ | 0);
        this.Damage = (Damage | 0);
        this.RecoveryTicks = (RecoveryTicks | 0);
        this.AreaShape = AreaShape;
    }
}

export function SimulatorAttackProfile_$reflection() {
    return record_type("SIR.Client.SimulatorAttackProfile", [], SimulatorAttackProfile, () => [["Delivery", SimulatorCombatDelivery_$reflection()], ["Range", int32_type], ["Damage", int32_type], ["RecoveryTicks", int32_type], ["AreaShape", option_type(SimulatorAreaShape_$reflection())]]);
}

export class SimulatorCombatEvent extends Record {
    constructor(Tick, SourceUnitId, Target, Delivery, Damage, Summary) {
        super();
        this.Tick = (Tick | 0);
        this.SourceUnitId = (SourceUnitId | 0);
        this.Target = Target;
        this.Delivery = Delivery;
        this.Damage = (Damage | 0);
        this.Summary = Summary;
    }
}

export function SimulatorCombatEvent_$reflection() {
    return record_type("SIR.Client.SimulatorCombatEvent", [], SimulatorCombatEvent, () => [["Tick", int32_type], ["SourceUnitId", int32_type], ["Target", SimulatorCombatTarget_$reflection()], ["Delivery", SimulatorCombatDelivery_$reflection()], ["Damage", int32_type], ["Summary", string_type]]);
}

export class SimulatorMovementProfile extends Record {
    constructor(SpeedMillimetersPerSecond, CellMillimeters) {
        super();
        this.SpeedMillimetersPerSecond = (SpeedMillimetersPerSecond | 0);
        this.CellMillimeters = (CellMillimeters | 0);
    }
}

export function SimulatorMovementProfile_$reflection() {
    return record_type("SIR.Client.SimulatorMovementProfile", [], SimulatorMovementProfile, () => [["SpeedMillimetersPerSecond", int32_type], ["CellMillimeters", int32_type]]);
}

export class SimulatorMovementProgress extends Record {
    constructor(Origin, Destination, ProgressMillimeters, CostMillimeters) {
        super();
        this.Origin = Origin;
        this.Destination = Destination;
        this.ProgressMillimeters = (ProgressMillimeters | 0);
        this.CostMillimeters = (CostMillimeters | 0);
    }
}

export function SimulatorMovementProgress_$reflection() {
    return record_type("SIR.Client.SimulatorMovementProgress", [], SimulatorMovementProgress, () => [["Origin", EditorCellAddress_$reflection()], ["Destination", EditorCellAddress_$reflection()], ["ProgressMillimeters", int32_type], ["CostMillimeters", int32_type]]);
}

export class SimulatorHandoff extends Record {
    constructor(Revision, InitialRevision, ActivationTicks, ReconciliationMessage, RuntimeMap, KernelState, Tick, IsRunning, LastEvents, LastCombatEvents, LastCheckpoints, AttackRecoveryTicks, MovementCreditsMillimeters, MovementProgress, PresentationPositions, MovementIntents, PlannedRoutes, PreviewDestination) {
        super();
        this.Revision = Revision;
        this.InitialRevision = InitialRevision;
        this.ActivationTicks = ActivationTicks;
        this.ReconciliationMessage = ReconciliationMessage;
        this.RuntimeMap = RuntimeMap;
        this.KernelState = KernelState;
        this.Tick = (Tick | 0);
        this.IsRunning = IsRunning;
        this.LastEvents = LastEvents;
        this.LastCombatEvents = LastCombatEvents;
        this.LastCheckpoints = LastCheckpoints;
        this.AttackRecoveryTicks = AttackRecoveryTicks;
        this.MovementCreditsMillimeters = MovementCreditsMillimeters;
        this.MovementProgress = MovementProgress;
        this.PresentationPositions = PresentationPositions;
        this.MovementIntents = MovementIntents;
        this.PlannedRoutes = PlannedRoutes;
        this.PreviewDestination = PreviewDestination;
    }
}

export function SimulatorHandoff_$reflection() {
    return record_type("SIR.Client.SimulatorHandoff", [], SimulatorHandoff, () => [["Revision", MapRevision_$reflection()], ["InitialRevision", MapRevision_$reflection()], ["ActivationTicks", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, int32_type])], ["ReconciliationMessage", option_type(string_type)], ["RuntimeMap", MapDefinition_$reflection()], ["KernelState", MapScaleState_$reflection()], ["Tick", int32_type], ["IsRunning", bool_type], ["LastEvents", list_type(string_type)], ["LastCombatEvents", list_type(SimulatorCombatEvent_$reflection())], ["LastCheckpoints", list_type(MapScaleCheckpoint_$reflection())], ["AttackRecoveryTicks", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, int32_type])], ["MovementCreditsMillimeters", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, int32_type])], ["MovementProgress", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, SimulatorMovementProgress_$reflection()])], ["PresentationPositions", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, tuple_type(float64_type, float64_type)])], ["MovementIntents", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, Direction8_$reflection()])], ["PlannedRoutes", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, list_type(EditorCellAddress_$reflection())])], ["PreviewDestination", option_type(EditorCellAddress_$reflection())]]);
}

export class SimulatorAction extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["ToggleSimulatorRun", "StepSimulator", "AdvanceRunningSimulatorTick", "MoveSimulatorUnit", "SetSimulatorController", "SetSimulatorScript", "MoveSimulatorPreview", "ResetSimulatorPreviewToOrigin", "ResetSimulatorPreview", "CommitSimulatorPreview"];
    }
    static ToggleSimulatorRun = new SimulatorAction(0, []);
    static StepSimulator = new SimulatorAction(1, []);
    static AdvanceRunningSimulatorTick = new SimulatorAction(2, []);
    static ResetSimulatorPreviewToOrigin = new SimulatorAction(7, []);
    static ResetSimulatorPreview = new SimulatorAction(8, []);
    static CommitSimulatorPreview = new SimulatorAction(9, []);
}

export function SimulatorAction_$reflection() {
    return union_type("SIR.Client.SimulatorAction", [], SimulatorAction, () => [[], [], [], [["Item", Direction8_$reflection()]], [["Item", MapController_$reflection()]], [["Item", string_type]], [["columnDelta", int32_type], ["rowDelta", int32_type]], [], [], []]);
}

export const MapEditorSimulator_TicksPerSecond = 20;

export const MapEditorSimulator_CellMillimeters = 250;

export const MapEditorSimulator_DiagonalCellMillimeters = 354;

export const MapEditorSimulator_MaximumMovementCreditMillimeters = 1060;

function MapEditorSimulator_toCell(address) {
    return MapScale_cell(address.CellColumn, address.CellRow);
}

function MapEditorSimulator_fromCell(address) {
    return new EditorCellAddress(address.Col, address.Row);
}

function MapEditorSimulator_sideCode(side) {
    switch (side.tag) {
        case 1:
            return 1;
        case 2:
            return 2;
        default:
            return 0;
    }
}

function MapEditorSimulator_controllerToKernel(controller) {
    switch (controller.tag) {
        case 1:
            return MapScaleController.ScriptedController;
        case 2:
            return MapScaleController.GeneralController;
        default:
            return MapScaleController.ManualController;
    }
}

function MapEditorSimulator_terrainToKernel(terrain) {
    switch (terrain.tag) {
        case 1:
            return MapScaleTerrain.RoughTerrain;
        case 2:
            return MapScaleTerrain.BlockedTerrain;
        case 3:
            return MapScaleTerrain.ObjectiveTerrain;
        default:
            return MapScaleTerrain.OpenTerrain;
    }
}

function MapEditorSimulator_edgeToKernel(kind, isOpen) {
    switch (kind.tag) {
        case 1:
            return new MapScaleEdgeKind(/* DoorEdge */ 1, [isOpen]);
        case 2:
            return MapScaleEdgeKind.WindowEdge;
        default:
            return MapScaleEdgeKind.WallEdge;
    }
}

function MapEditorSimulator_edgeDirectionToKernel(direction) {
    if (direction.tag === 1) {
        return MapScaleEdgeDirection.SouthEdge;
    }
    else {
        return MapScaleEdgeDirection.EastEdge;
    }
}

function MapEditorSimulator_unitToKernel(unit) {
    return new MapScaleUnit(unit.Id, MapEditorSimulator_sideCode(unit.Side), unit.ClassId, MapScale_cell(unit.Column, unit.Row), unit.Size, unit.Health, MapEditorSimulator_controllerToKernel(unit.Controller), unit.Script, unit.ScriptIndex, unit.BodyFacing, unit.AttentionDirection);
}

function MapEditorSimulator_initialKernel(map) {
    return new MapScaleState(0, new MapScaleBoard(map.Width, map.Height, ofList(map_1((tupledArg) => {
        const _arg = tupledArg[0];
        return [MapScale_cell(_arg[0], _arg[1]), MapEditorSimulator_terrainToKernel(tupledArg[1])];
    }, toList(map.Terrain)), {
        Compare: (x, y) => (compare(x, y) | 0),
    }), ofList(map_1((tupledArg_1) => {
        const _arg_1 = tupledArg_1[0];
        const edge = tupledArg_1[1];
        return [[_arg_1[0], _arg_1[1], MapEditorSimulator_edgeDirectionToKernel(_arg_1[2])], MapEditorSimulator_edgeToKernel(edge[0], edge[1])];
    }, toList(map.Edges)), {
        Compare: (x_1, y_1) => (compareArrays(x_1, y_1) | 0),
    })), map_2((_arg_2, unit) => MapEditorSimulator_unitToKernel(unit), map.Units), empty({
        Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
    }), empty({
        Compare: (x_3, y_3) => (comparePrimitives(x_3, y_3) | 0),
    }), empty({
        Compare: (x_4, y_4) => (comparePrimitives(x_4, y_4) | 0),
    }), empty({
        Compare: (x_5, y_5) => (comparePrimitives(x_5, y_5) | 0),
    }), empty({
        Compare: (x_6, y_6) => (comparePrimitives(x_6, y_6) | 0),
    }));
}

function MapEditorSimulator_syncMapFromKernel(map, kernel) {
    return new MapDefinition(map.Width, map.Height, map.Terrain, map.Edges, map_2((id, editor) => {
        const matchValue = tryFind(id, kernel.Units);
        if (matchValue != null) {
            const unit = matchValue;
            return new EditorUnit(editor.Id, editor.Side, editor.ClassId, unit.Cell.Col, unit.Cell.Row, editor.Size, unit.Health, editor.HealthMaximum, editor.Controller, editor.Script, unit.ScriptIndex, unit.BodyFacing, unit.AttentionDirection);
        }
        else {
            return editor;
        }
    }, map.Units), map.NextUnitId, map.Regions, map.NextRegionId);
}

function MapEditorSimulator_syncKernelConfiguration(map, kernel) {
    return new MapScaleState(kernel.Tick, MapEditorSimulator_initialKernel(map).Board, map_2((id, editor) => {
        const configured = MapEditorSimulator_unitToKernel(editor);
        const matchValue = tryFind(id, kernel.Units);
        if (matchValue == null) {
            return configured;
        }
        else {
            const existing = matchValue;
            return new MapScaleUnit(configured.Id, configured.Side, configured.ClassId, existing.Cell, configured.Size, existing.Health, configured.Controller, configured.Script, configured.ScriptIndex, configured.BodyFacing, configured.AttentionDirection);
        }
    }, map.Units), kernel.MovementCreditsMillimeters, kernel.MovementProgress, kernel.MovementIntents, kernel.PlannedRoutes, kernel.Engagements);
}

function MapEditorSimulator_fromProgress(progress) {
    return new SimulatorMovementProgress(MapEditorSimulator_fromCell(progress.Origin), MapEditorSimulator_fromCell(progress.Destination), progress.ProgressMillimeters, progress.CostMillimeters);
}

function MapEditorSimulator_collisionFromKernel(blocker) {
    switch (blocker.tag) {
        case 1:
            return new SimulatorCollision(/* BlockedTerrainAt */ 2, [MapEditorSimulator_fromCell(blocker.fields[0])]);
        case 2:
            return new SimulatorCollision(/* BlockingEdgeAt */ 3, [MapEditorSimulator_fromCell(blocker.fields[0]), MapEditorSimulator_fromCell(blocker.fields[1])]);
        case 3:
            return new SimulatorCollision(/* OccupiedAt */ 4, [MapEditorSimulator_fromCell(blocker.fields[0]), blocker.fields[1]]);
        case 4:
            return new SimulatorCollision(/* OccupiedAt */ 4, [MapEditorSimulator_fromCell(blocker.fields[0]), -1]);
        case 5:
            return new SimulatorCollision(/* OccupiedAt */ 4, [new EditorCellAddress(0, 0), blocker.fields[0]]);
        default:
            return new SimulatorCollision(/* OutsideMap */ 1, [MapEditorSimulator_fromCell(blocker.fields[0])]);
    }
}

function MapEditorSimulator_directionCode(direction) {
    switch (direction.tag) {
        case 1:
            return "NE";
        case 2:
            return "E";
        case 3:
            return "SE";
        case 4:
            return "S";
        case 5:
            return "SW";
        case 6:
            return "W";
        case 7:
            return "NW";
        default:
            return "N";
    }
}

function MapEditorSimulator_deliveryLabel(delivery) {
    switch (delivery.tag) {
        case 1:
            return "ranged";
        case 2:
            return "lobbed area";
        case 3:
            return "spell area";
        default:
            return "melee";
    }
}

function MapEditorSimulator_deliveryFromKernel(delivery) {
    switch (delivery.tag) {
        case 1:
            return SimulatorCombatDelivery.ProjectileDelivery;
        case 2:
            return SimulatorCombatDelivery.LobbedAreaDelivery;
        case 3:
            return SimulatorCombatDelivery.SpellAreaDelivery;
        default:
            return SimulatorCombatDelivery.MeleeDelivery;
    }
}

function MapEditorSimulator_shapeFromKernel(shape) {
    switch (shape.tag) {
        case 1:
            return new SimulatorAreaShape(/* ConeArea */ 1, [shape.fields[0], shape.fields[1]]);
        case 2:
            return new SimulatorAreaShape(/* RayArea */ 2, [shape.fields[0], shape.fields[1]]);
        case 3:
            return new SimulatorAreaShape(/* RectangleArea */ 3, [shape.fields[0], shape.fields[1]]);
        default:
            return new SimulatorAreaShape(/* BurstArea */ 0, [shape.fields[0]]);
    }
}

function MapEditorSimulator_eventSummary(event) {
    switch (event.tag) {
        case 1:
            return ((((("Unit " + int32ToString(event.fields[0])) + " moves by ") + int32ToString(event.fields[3])) + " mm, spending ") + int32ToString(event.fields[4])) + " mm of movement credit.";
        case 2:
            return ((("Unit " + int32ToString(event.fields[0])) + " cannot move: ") + toString(event.fields[2])) + ".";
        case 3:
            return ((("Unit " + int32ToString(event.fields[0])) + " ") + event.fields[1]) + ".";
        case 4:
            return ((("Unit " + int32ToString(event.fields[0])) + " recovers from its attack for ") + int32ToString(event.fields[1])) + " more ticks.";
        case 5:
            if (event.fields[1].tag === 1) {
                return ((((("Unit " + int32ToString(event.fields[0])) + " makes a ") + MapEditorSimulator_deliveryLabel(MapEditorSimulator_deliveryFromKernel(event.fields[2]))) + " area attack for ") + int32ToString(event.fields[3])) + " damage.";
            }
            else {
                return ((((((("Unit " + int32ToString(event.fields[0])) + " makes a ") + MapEditorSimulator_deliveryLabel(MapEditorSimulator_deliveryFromKernel(event.fields[2]))) + " attack against unit ") + int32ToString(event.fields[1].fields[0])) + " for ") + int32ToString(event.fields[3])) + " damage.";
            }
        default:
            return ((((("Unit " + int32ToString(event.fields[0])) + " prepares to move (") + int32ToString(event.fields[3])) + "/") + int32ToString(event.fields[4])) + " mm).";
    }
}

export function MapEditorSimulator_attackProfileFor(unit) {
    let option_1;
    const profile = MapScale_combatProfileFor(unit.ClassId);
    return new SimulatorAttackProfile(MapEditorSimulator_deliveryFromKernel(profile.Delivery), profile.Range, profile.Damage, profile.RecoveryTicks, (option_1 = profile.AreaShape, (option_1 != null) ? MapEditorSimulator_shapeFromKernel(option_1) : undefined));
}

export function MapEditorSimulator_movementProfileFor(unit) {
    const profile = MapScale_movementProfileFor(unit.ClassId);
    return new SimulatorMovementProfile(profile.SpeedMillimetersPerSecond, profile.CellMillimeters);
}

export function MapEditorSimulator_pathfind(destination, unit, map) {
    const kernel = MapEditorSimulator_initialKernel(map);
    const option_1 = MapScale_tryFindPath(kernel.Board, kernel.Units, find(unit.Id, kernel.Units), MapEditorSimulator_toCell(destination));
    if (option_1 != null) {
        return map_1(MapEditorSimulator_fromCell, option_1.Route);
    }
    else {
        return undefined;
    }
}

export function MapEditorSimulator_preview(selectedUnitId, destination, handoff) {
    let unit, result, collision, option_1, route, option_4, option_7, option_10;
    const kernel = MapEditorSimulator_syncKernelConfiguration(handoff.RuntimeMap, handoff.KernelState);
    const option_15 = selectedUnitId;
    if (option_15 != null) {
        const id = option_15 | 0;
        const option_13 = tryFind(id, kernel.Units);
        if (option_13 != null) {
            return (unit = option_13, (result = MapScale_tryFindPath(kernel.Board, kernel.Units, unit, MapEditorSimulator_toCell(destination)), (collision = ((result == null) ? defaultArg((option_1 = MapScale_movementCollision(kernel.Board, kernel.Units, unit, MapEditorSimulator_toCell(destination)), (option_1 != null) ? MapEditorSimulator_collisionFromKernel(option_1) : undefined), new SimulatorCollision(/* NoPathTo */ 5, [destination])) : SimulatorCollision.RouteClear), (route = defaultArg((option_4 = result, (option_4 != null) ? option_4.Route : undefined), empty_1()), new SimulatorRoutePreview(id, MapEditorSimulator_fromCell(unit.Cell), destination, length_1(route), defaultArg((option_7 = result, (option_7 != null) ? option_7.DistanceMillimeters : undefined), 0), defaultArg((option_10 = result, (option_10 != null) ? option_10.MovementCostMillimeters : undefined), 0), toArray(map_1(MapEditorSimulator_fromCell, route)), collision)))));
        }
        else {
            return undefined;
        }
    }
    else {
        return undefined;
    }
}

function MapEditorSimulator_fromRevision(revision) {
    const map = revision.Document;
    const kernel = MapEditorSimulator_initialKernel(map);
    return new SimulatorHandoff(revision, revision, empty({
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }), undefined, map, kernel, 0, false, empty_1(), empty_1(), empty_1(), empty({
        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
    }), empty({
        Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
    }), empty({
        Compare: (x_3, y_3) => (comparePrimitives(x_3, y_3) | 0),
    }), map_2((_arg, unit) => [unit.Column, unit.Row], map.Units), empty({
        Compare: (x_4, y_4) => (comparePrimitives(x_4, y_4) | 0),
    }), empty({
        Compare: (x_5, y_5) => (comparePrimitives(x_5, y_5) | 0),
    }), undefined);
}

export function MapEditorSimulator_tryHandoff(state) {
    let issues;
    const array = validationIssues(state.Revision.Document);
    issues = array.filter((issue) => (issue.Code !== "EDGE-GAP"));
    if (!(issues.length === 0)) {
        return new FSharpResult$2(/* Error */ 1, [join(" ", map_3((issue_1) => ((issue_1.Code + ": ") + issue_1.Message), issues))]);
    }
    else {
        return new FSharpResult$2(/* Ok */ 0, [MapEditorSimulator_fromRevision(state.Revision)]);
    }
}

/**
 * Restores the disposable runtime from the revision pinned by its
 * existing handoff. The mutable editor draft is deliberately absent from
 * this boundary.
 */
export function MapEditorSimulator_reset(handoff) {
    return MapEditorSimulator_fromRevision(handoff.Revision);
}

export function MapEditorSimulator_isBehindDraft(state, handoff) {
    return handoff.Revision.Digest !== state.Revision.Digest;
}

/**
 * Reconciles a valid authored revision with live simulation. Additions retain the live kernel;
 * all geometry or existing-unit changes restart from the deterministic initial revision.
 */
export function MapEditorSimulator_reconcile(state, handoff) {
    const next = state.Revision;
    const before = handoff.Revision.Document;
    const after = next.Document;
    const unchangedGeometry = (before.Width === after.Width) && (before.Height === after.Height);
    const unchangedTerrain = before.Terrain.Equals(after.Terrain);
    const unchangedTopology = before.Edges.Equals(after.Edges);
    const retained = forAll((id, unit) => equals(tryFind(id, after.Units), unit), before.Units);
    if (((unchangedGeometry && unchangedTerrain) && unchangedTopology) && retained) {
        const introduced = filter((id_1, _arg) => !containsKey(id_1, before.Units), after.Units);
        if (isEmpty(introduced)) {
            return new SimulatorHandoff(next, handoff.InitialRevision, handoff.ActivationTicks, undefined, handoff.RuntimeMap, handoff.KernelState, handoff.Tick, handoff.IsRunning, handoff.LastEvents, handoff.LastCombatEvents, handoff.LastCheckpoints, handoff.AttackRecoveryTicks, handoff.MovementCreditsMillimeters, handoff.MovementProgress, handoff.PresentationPositions, handoff.MovementIntents, handoff.PlannedRoutes, handoff.PreviewDestination);
        }
        else {
            let runtime;
            const bind$0040 = handoff.RuntimeMap;
            runtime = (new MapDefinition(bind$0040.Width, bind$0040.Height, bind$0040.Terrain, bind$0040.Edges, fold((units, id_2, unit_1) => add(id_2, unit_1, units), handoff.RuntimeMap.Units, introduced), bind$0040.NextUnitId, bind$0040.Regions, bind$0040.NextRegionId));
            let kernel;
            const bind$0040_1 = handoff.KernelState;
            kernel = (new MapScaleState(bind$0040_1.Tick, bind$0040_1.Board, fold((units_1, id_3, unit_2) => add(id_3, MapEditorSimulator_unitToKernel(unit_2), units_1), handoff.KernelState.Units, introduced), bind$0040_1.MovementCreditsMillimeters, bind$0040_1.MovementProgress, bind$0040_1.MovementIntents, bind$0040_1.PlannedRoutes, bind$0040_1.Engagements));
            const PresentationPositions = fold((positions, id_4, unit_3) => add(id_4, [unit_3.Column, unit_3.Row], positions), handoff.PresentationPositions, introduced);
            return new SimulatorHandoff(next, handoff.InitialRevision, fold((ticks, id_5, _arg_1) => add(id_5, handoff.Tick, ticks), handoff.ActivationTicks, introduced), ((("Added " + int32ToString(count(introduced))) + " unit(s) at tick ") + int32ToString(handoff.Tick)) + ".", runtime, kernel, handoff.Tick, handoff.IsRunning, handoff.LastEvents, handoff.LastCombatEvents, handoff.LastCheckpoints, handoff.AttackRecoveryTicks, handoff.MovementCreditsMillimeters, handoff.MovementProgress, PresentationPositions, handoff.MovementIntents, handoff.PlannedRoutes, handoff.PreviewDestination);
        }
    }
    else {
        const reason = !unchangedGeometry ? "map geometry changed" : (!unchangedTerrain ? "terrain changed" : (!unchangedTopology ? "edge topology changed" : defaultArg(tryPick((tupledArg) => {
            const id_6 = tupledArg[0] | 0;
            const matchValue = tryFind(id_6, after.Units);
            if (matchValue != null) {
                if (!equals(matchValue, tupledArg[1])) {
                    const updated_1 = matchValue;
                    return ("existing unit " + int32ToString(id_6)) + " changed";
                }
                else {
                    return undefined;
                }
            }
            else {
                return ("existing unit " + int32ToString(id_6)) + " was removed";
            }
        }, toSeq(before.Units)), "an incompatible authored value changed")));
        const bind$0040_2 = MapEditorSimulator_fromRevision(next);
        return new SimulatorHandoff(bind$0040_2.Revision, next, bind$0040_2.ActivationTicks, ("Simulation restarted at tick 0 because " + reason) + ".", bind$0040_2.RuntimeMap, bind$0040_2.KernelState, bind$0040_2.Tick, bind$0040_2.IsRunning, bind$0040_2.LastEvents, bind$0040_2.LastCombatEvents, bind$0040_2.LastCheckpoints, bind$0040_2.AttackRecoveryTicks, bind$0040_2.MovementCreditsMillimeters, bind$0040_2.MovementProgress, bind$0040_2.PresentationPositions, bind$0040_2.MovementIntents, bind$0040_2.PlannedRoutes, bind$0040_2.PreviewDestination);
    }
}

export function MapEditorSimulator_perspectivePreview(projection) {
    let matchResult, frame_1;
    if (projection != null) {
        if (equals(projection.Disclosure, DisclosureLabel.PerspectiveDisclosure)) {
            matchResult = 0;
            frame_1 = projection;
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
            return new PerspectivePreviewAvailability(/* AcceptedPerspectiveProjection */ 1, [frame_1]);
        default:
            return new PerspectivePreviewAvailability(/* PerspectivePreviewUnavailable */ 0, ["Player perspective is unavailable: no accepted disclosure-filtered projection exists for editor drafts."]);
    }
}

export const MapEditorSimulator_visibilityOverlays = new VisibilityOverlayAvailability(/* VisibilityOverlaysUnavailable */ 0, ["Visibility overlays are unavailable until shared-kernel perception rules are accepted."]);

function MapEditorSimulator_step(handoff) {
    const result = MapScale_tick(MapEditorSimulator_syncKernelConfiguration(handoff.RuntimeMap, handoff.KernelState));
    const map = MapEditorSimulator_syncMapFromKernel(handoff.RuntimeMap, result.State);
    const combat = choose((_arg) => {
        if (_arg.tag === 5) {
            if (_arg.fields[1].tag === 1) {
                return new SimulatorCombatEvent(result.State.Tick, _arg.fields[0], new SimulatorCombatTarget(/* AreaCombatTarget */ 1, [MapEditorSimulator_fromCell(_arg.fields[1].fields[0]), MapEditorSimulator_shapeFromKernel(_arg.fields[1].fields[1])]), MapEditorSimulator_deliveryFromKernel(_arg.fields[2]), _arg.fields[3], MapEditorSimulator_eventSummary(_arg));
            }
            else {
                return new SimulatorCombatEvent(result.State.Tick, _arg.fields[0], new SimulatorCombatTarget(/* UnitCombatTarget */ 0, [_arg.fields[1].fields[0]]), MapEditorSimulator_deliveryFromKernel(_arg.fields[2]), _arg.fields[3], MapEditorSimulator_eventSummary(_arg));
            }
        }
        else {
            return undefined;
        }
    }, result.Events);
    const recent = append(filter_1((event_2) => ((result.State.Tick - event_2.Tick) <= 5), handoff.LastCombatEvents), combat);
    const progress_1 = map_2((_arg_1, progress) => MapEditorSimulator_fromProgress(progress), result.State.MovementProgress);
    const approach = (current, target_1) => (current + max(-0.3, min(0.3, target_1 - current)));
    const presentationPositions = map_2((id, unit) => {
        let option_2, movement, fraction;
        const current_1 = defaultArg(tryFind(id, handoff.PresentationPositions), [unit.Cell.Col, unit.Cell.Row]);
        const offset = defaultArg((option_2 = tryFind(id, progress_1), (option_2 != null) ? ((movement = option_2, (fraction = min(1, movement.ProgressMillimeters / max(1, movement.CostMillimeters)), [(movement.Destination.CellColumn - movement.Origin.CellColumn) * fraction, (movement.Destination.CellRow - movement.Origin.CellRow) * fraction]))) : undefined), [0, 0]);
        const targetRow = unit.Cell.Row + offset[1];
        return [approach(current_1[0], unit.Cell.Col + offset[0]), approach(current_1[1], targetRow)];
    }, result.State.Units);
    return new SimulatorHandoff(handoff.Revision, handoff.InitialRevision, handoff.ActivationTicks, handoff.ReconciliationMessage, map, result.State, result.State.Tick, handoff.IsRunning, map_1(MapEditorSimulator_eventSummary, result.Events), recent, result.Checkpoints, map_2((_arg_2, engagement) => (engagement.RecoveryTicksRemaining | 0), result.State.Engagements), result.State.MovementCreditsMillimeters, progress_1, presentationPositions, result.State.MovementIntents, map_2((_arg_3, route) => map_1(MapEditorSimulator_fromCell, route), result.State.PlannedRoutes), undefined);
}

/**
 * Reconstructs actual simulation state at a timeline tick from its pinned initial revision.
 */
export function MapEditorSimulator_seek(tick, handoff) {
    const target = max(0, tick) | 0;
    const activate = (atTick, current) => fold((state, id, activation) => {
        let bind$0040, bind$0040_1;
        if (activation === atTick) {
            const matchValue = tryFind(id, handoff.Revision.Document.Units);
            if (matchValue == null) {
                return state;
            }
            else {
                const unit = matchValue;
                return new SimulatorHandoff(state.Revision, state.InitialRevision, state.ActivationTicks, state.ReconciliationMessage, (bind$0040 = state.RuntimeMap, new MapDefinition(bind$0040.Width, bind$0040.Height, bind$0040.Terrain, bind$0040.Edges, add(id, unit, state.RuntimeMap.Units), bind$0040.NextUnitId, bind$0040.Regions, bind$0040.NextRegionId)), (bind$0040_1 = state.KernelState, new MapScaleState(bind$0040_1.Tick, bind$0040_1.Board, add(id, MapEditorSimulator_unitToKernel(unit), state.KernelState.Units), bind$0040_1.MovementCreditsMillimeters, bind$0040_1.MovementProgress, bind$0040_1.MovementIntents, bind$0040_1.PlannedRoutes, bind$0040_1.Engagements)), state.Tick, state.IsRunning, state.LastEvents, state.LastCombatEvents, state.LastCheckpoints, state.AttackRecoveryTicks, state.MovementCreditsMillimeters, state.MovementProgress, add(id, [unit.Column, unit.Row], state.PresentationPositions), state.MovementIntents, state.PlannedRoutes, state.PreviewDestination);
            }
        }
        else {
            return state;
        }
    }, current, handoff.ActivationTicks);
    const replayed = fold_1((current_1, nextTick) => activate(nextTick, MapEditorSimulator_step(current_1)), activate(0, MapEditorSimulator_fromRevision(handoff.InitialRevision)), toList_1(rangeDouble(1, 1, target)));
    return new SimulatorHandoff(handoff.Revision, handoff.InitialRevision, handoff.ActivationTicks, handoff.ReconciliationMessage, replayed.RuntimeMap, replayed.KernelState, replayed.Tick, handoff.IsRunning, replayed.LastEvents, replayed.LastCombatEvents, replayed.LastCheckpoints, replayed.AttackRecoveryTicks, replayed.MovementCreditsMillimeters, replayed.MovementProgress, replayed.PresentationPositions, replayed.MovementIntents, replayed.PlannedRoutes, replayed.PreviewDestination);
}

export function MapEditorSimulator_update(action, selectedUnitId, handoff) {
    let option_8, option_6, unit_1, kernel, bind$0040_1, MovementIntents, MovementProgress_1, PlannedRoutes_1, option_17, option_15, option_13, option_11, unit_4, p, option_21, option_19, unit_5, route;
    const updateSelected = (transform) => {
        let option_3, option_1, unit, map, bind$0040;
        return defaultArg((option_3 = ((option_1 = selectedUnitId, (option_1 != null) ? tryFind(option_1, handoff.RuntimeMap.Units) : undefined)), (option_3 != null) ? ((unit = option_3, (map = ((bind$0040 = handoff.RuntimeMap, new MapDefinition(bind$0040.Width, bind$0040.Height, bind$0040.Terrain, bind$0040.Edges, add(unit.Id, transform(unit), handoff.RuntimeMap.Units), bind$0040.NextUnitId, bind$0040.Regions, bind$0040.NextRegionId))), new SimulatorHandoff(handoff.Revision, handoff.InitialRevision, handoff.ActivationTicks, handoff.ReconciliationMessage, map, MapEditorSimulator_syncKernelConfiguration(map, handoff.KernelState), handoff.Tick, handoff.IsRunning, handoff.LastEvents, handoff.LastCombatEvents, handoff.LastCheckpoints, handoff.AttackRecoveryTicks, handoff.MovementCreditsMillimeters, handoff.MovementProgress, handoff.PresentationPositions, handoff.MovementIntents, handoff.PlannedRoutes, undefined)))) : undefined), handoff);
    };
    let matchResult;
    switch (action.tag) {
        case 2: {
            if (handoff.IsRunning) {
                matchResult = 1;
            }
            else {
                matchResult = 2;
            }
            break;
        }
        case 1: {
            if (handoff.IsRunning) {
                matchResult = 3;
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 3: {
            if (handoff.IsRunning) {
                matchResult = 3;
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 4: {
            if (handoff.IsRunning) {
                matchResult = 3;
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 5: {
            if (handoff.IsRunning) {
                matchResult = 3;
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 6: {
            if (handoff.IsRunning) {
                matchResult = 3;
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 7: {
            if (handoff.IsRunning) {
                matchResult = 3;
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 8: {
            if (handoff.IsRunning) {
                matchResult = 3;
            }
            else {
                matchResult = 4;
            }
            break;
        }
        case 9: {
            if (handoff.IsRunning) {
                matchResult = 3;
            }
            else {
                matchResult = 4;
            }
            break;
        }
        default:
            matchResult = 0;
    }
    switch (matchResult) {
        case 0:
            return new SimulatorHandoff(handoff.Revision, handoff.InitialRevision, handoff.ActivationTicks, handoff.ReconciliationMessage, handoff.RuntimeMap, handoff.KernelState, handoff.Tick, !handoff.IsRunning, handoff.LastEvents, handoff.LastCombatEvents, handoff.LastCheckpoints, handoff.AttackRecoveryTicks, handoff.MovementCreditsMillimeters, handoff.MovementProgress, handoff.PresentationPositions, handoff.MovementIntents, handoff.PlannedRoutes, handoff.IsRunning ? handoff.PreviewDestination : undefined);
        case 1:
            return MapEditorSimulator_step(handoff);
        case 2:
            return handoff;
        case 3:
            return handoff;
        default:
            switch (action.tag) {
                case 1:
                    return MapEditorSimulator_step(handoff);
                case 3: {
                    const direction = action.fields[0];
                    return defaultArg((option_8 = ((option_6 = selectedUnitId, (option_6 != null) ? tryFind(option_6, handoff.RuntimeMap.Units) : undefined)), (option_8 != null) ? ((unit_1 = option_8, (kernel = ((bind$0040_1 = handoff.KernelState, (MovementIntents = add(unit_1.Id, direction, handoff.KernelState.MovementIntents), new MapScaleState(bind$0040_1.Tick, bind$0040_1.Board, bind$0040_1.Units, bind$0040_1.MovementCreditsMillimeters, remove(unit_1.Id, handoff.KernelState.MovementProgress), MovementIntents, remove(unit_1.Id, handoff.KernelState.PlannedRoutes), bind$0040_1.Engagements)))), (MovementProgress_1 = remove(unit_1.Id, handoff.MovementProgress), (PlannedRoutes_1 = remove(unit_1.Id, handoff.PlannedRoutes), new SimulatorHandoff(handoff.Revision, handoff.InitialRevision, handoff.ActivationTicks, handoff.ReconciliationMessage, handoff.RuntimeMap, kernel, handoff.Tick, handoff.IsRunning, singleton(((("Unit " + int32ToString(unit_1.Id)) + " receives movement intent ") + MapEditorSimulator_directionCode(direction)) + "; advance simulation time to resolve it."), empty_1(), handoff.LastCheckpoints, handoff.AttackRecoveryTicks, handoff.MovementCreditsMillimeters, MovementProgress_1, handoff.PresentationPositions, kernel.MovementIntents, PlannedRoutes_1, undefined)))))) : undefined), handoff);
                }
                case 4:
                    return updateSelected((unit_2) => (new EditorUnit(unit_2.Id, unit_2.Side, unit_2.ClassId, unit_2.Column, unit_2.Row, unit_2.Size, unit_2.Health, unit_2.HealthMaximum, action.fields[0], unit_2.Script, unit_2.ScriptIndex, unit_2.BodyFacing, unit_2.AttentionDirection)));
                case 5: {
                    const matchValue = parseScript(action.fields[0]);
                    if (matchValue.tag === 1) {
                        return new SimulatorHandoff(handoff.Revision, handoff.InitialRevision, handoff.ActivationTicks, handoff.ReconciliationMessage, handoff.RuntimeMap, handoff.KernelState, handoff.Tick, handoff.IsRunning, singleton(matchValue.fields[0]), empty_1(), handoff.LastCheckpoints, handoff.AttackRecoveryTicks, handoff.MovementCreditsMillimeters, handoff.MovementProgress, handoff.PresentationPositions, handoff.MovementIntents, handoff.PlannedRoutes, handoff.PreviewDestination);
                    }
                    else {
                        return updateSelected((unit_3) => (new EditorUnit(unit_3.Id, unit_3.Side, unit_3.ClassId, unit_3.Column, unit_3.Row, unit_3.Size, unit_3.Health, unit_3.HealthMaximum, unit_3.Controller, matchValue.fields[0], 0, unit_3.BodyFacing, unit_3.AttentionDirection)));
                    }
                }
                case 6:
                    return new SimulatorHandoff(handoff.Revision, handoff.InitialRevision, handoff.ActivationTicks, handoff.ReconciliationMessage, handoff.RuntimeMap, handoff.KernelState, handoff.Tick, handoff.IsRunning, handoff.LastEvents, handoff.LastCombatEvents, handoff.LastCheckpoints, handoff.AttackRecoveryTicks, handoff.MovementCreditsMillimeters, handoff.MovementProgress, handoff.PresentationPositions, handoff.MovementIntents, handoff.PlannedRoutes, (option_17 = ((option_15 = handoff.PreviewDestination, (option_15 != null) ? option_15 : ((option_13 = ((option_11 = selectedUnitId, (option_11 != null) ? tryFind(option_11, handoff.RuntimeMap.Units) : undefined)), (option_13 != null) ? ((unit_4 = option_13, new EditorCellAddress(unit_4.Column, unit_4.Row))) : undefined)))), (option_17 != null) ? ((p = option_17, new EditorCellAddress(p.CellColumn + action.fields[0], p.CellRow + action.fields[1]))) : undefined));
                case 7:
                    return new SimulatorHandoff(handoff.Revision, handoff.InitialRevision, handoff.ActivationTicks, handoff.ReconciliationMessage, handoff.RuntimeMap, handoff.KernelState, handoff.Tick, handoff.IsRunning, handoff.LastEvents, handoff.LastCombatEvents, handoff.LastCheckpoints, handoff.AttackRecoveryTicks, handoff.MovementCreditsMillimeters, handoff.MovementProgress, handoff.PresentationPositions, handoff.MovementIntents, handoff.PlannedRoutes, (option_21 = ((option_19 = selectedUnitId, (option_19 != null) ? tryFind(option_19, handoff.RuntimeMap.Units) : undefined)), (option_21 != null) ? ((unit_5 = option_21, new EditorCellAddress(unit_5.Column, unit_5.Row))) : undefined));
                case 8:
                    return new SimulatorHandoff(handoff.Revision, handoff.InitialRevision, handoff.ActivationTicks, handoff.ReconciliationMessage, handoff.RuntimeMap, handoff.KernelState, handoff.Tick, handoff.IsRunning, handoff.LastEvents, handoff.LastCombatEvents, handoff.LastCheckpoints, handoff.AttackRecoveryTicks, handoff.MovementCreditsMillimeters, handoff.MovementProgress, handoff.PresentationPositions, handoff.MovementIntents, handoff.PlannedRoutes, undefined);
                case 9: {
                    let matchValue_1;
                    const option_23 = handoff.PreviewDestination;
                    matchValue_1 = ((option_23 != null) ? MapEditorSimulator_preview(selectedUnitId, option_23, handoff) : undefined);
                    if (matchValue_1 == null) {
                        return handoff;
                    }
                    else if ((route = matchValue_1, equals(route.Collision, SimulatorCollision.RouteClear) && (route.Route.length > 0))) {
                        const route_1 = matchValue_1;
                        let kernel_1;
                        const bind$0040_2 = handoff.KernelState;
                        const PlannedRoutes_2 = add(route_1.UnitId, ofArray(map_3(MapEditorSimulator_toCell, route_1.Route)), handoff.KernelState.PlannedRoutes);
                        const MovementIntents_2 = remove(route_1.UnitId, handoff.KernelState.MovementIntents);
                        kernel_1 = (new MapScaleState(bind$0040_2.Tick, bind$0040_2.Board, bind$0040_2.Units, bind$0040_2.MovementCreditsMillimeters, remove(route_1.UnitId, handoff.KernelState.MovementProgress), MovementIntents_2, PlannedRoutes_2, bind$0040_2.Engagements));
                        const PlannedRoutes_3 = add(route_1.UnitId, ofArray(route_1.Route), handoff.PlannedRoutes);
                        const MovementIntents_3 = remove(route_1.UnitId, handoff.MovementIntents);
                        const MovementProgress_3 = remove(route_1.UnitId, handoff.MovementProgress);
                        return new SimulatorHandoff(handoff.Revision, handoff.InitialRevision, handoff.ActivationTicks, handoff.ReconciliationMessage, handoff.RuntimeMap, kernel_1, handoff.Tick, handoff.IsRunning, singleton(((((((("Unit " + int32ToString(route_1.UnitId)) + " accepts a ") + int32ToString(route_1.Distance)) + "-step, ") + int32ToString(route_1.DistanceMillimeters)) + " mm path costing ") + int32ToString(route_1.MovementCostMillimeters)) + " mm of movement credit; advance simulation time to move."), empty_1(), handoff.LastCheckpoints, handoff.AttackRecoveryTicks, handoff.MovementCreditsMillimeters, MovementProgress_3, handoff.PresentationPositions, MovementIntents_3, PlannedRoutes_3, undefined);
                    }
                    else {
                        const route_2 = matchValue_1;
                        return new SimulatorHandoff(handoff.Revision, handoff.InitialRevision, handoff.ActivationTicks, handoff.ReconciliationMessage, handoff.RuntimeMap, handoff.KernelState, handoff.Tick, handoff.IsRunning, singleton(("Preview route rejected: " + toString(route_2.Collision)) + "."), empty_1(), handoff.LastCheckpoints, handoff.AttackRecoveryTicks, handoff.MovementCreditsMillimeters, handoff.MovementProgress, handoff.PresentationPositions, handoff.MovementIntents, handoff.PlannedRoutes, handoff.PreviewDestination);
                    }
                }
                default:
                    throw new Exception("Match failure: SIR.Client.SimulatorAction");
            }
    }
}

export function MapEditorSimulator_presentationOffsets(handoff) {
    return fold((offsets, id, tupledArg) => {
        let matchValue_2, progress, fraction;
        const matchValue = tryFind(id, handoff.RuntimeMap.Units);
        if (matchValue == null) {
            return offsets;
        }
        else {
            const unit = matchValue;
            return add(id, (matchValue_2 = tryFind(id, handoff.MovementProgress), (unit.Controller.tag === 0) ? ((matchValue_2 != null) ? ((progress = matchValue_2, (fraction = min(1, progress.ProgressMillimeters / max(1, progress.CostMillimeters)), [(progress.Destination.CellColumn - progress.Origin.CellColumn) * fraction, (progress.Destination.CellRow - progress.Origin.CellRow) * fraction]))) : [tupledArg[0] - unit.Column, tupledArg[1] - unit.Row]) : [tupledArg[0] - unit.Column, tupledArg[1] - unit.Row]), offsets);
        }
    }, empty({
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }), handoff.PresentationPositions);
}

export function MapEditorSimulator_frame(selectedUnitId, handoff) {
    let option_1, route_1, unit;
    const baseFrame = frame_2(new MapEditorState(handoff.RuntimeMap, initial.Tool, initial.TerrainSelection, initial.BrushSize, initial.TerrainCursor, initial.KeyboardCursor, initial.KeyboardObject, initial.LastTerrainPaintTool, initial.TerrainAnnouncement, initial.EdgeCursor, initial.EdgeAnnouncement, initial.UnitPaletteSearch, initial.UnitPaletteCursor, initial.UnitPlacementCursor, initial.UnitAnnouncement, initial.RegionAnnouncement, initial.RegionKeyboardMode, selectedUnitId, defaultArg((option_1 = selectedUnitId, (option_1 != null) ? singleton_1(option_1, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }) : undefined), empty_2({
        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
    })), initial.SelectedRegion, initial.Gesture, handoff.Revision, RevisionState_1.SimulatedRevision, initial.SavedDigest, handoff.Revision.Digest, initial.RecoveredFromDigest, initial.UndoHistory, initial.RedoHistory, initial.HistoryBytes, initial.Clipboard, handoff.Tick, handoff.IsRunning, handoff.LastEvents, initial.Validation, initial.Layers, initial.Issues, initial.ActiveIssue, initial.PendingDestructiveChange, initial.PendingRecovery, initial.Authoring));
    const routeOverlay = (kind, unitId, origin, route, label) => (new OverlayVisual("simulator-route-preview", kind, new OverlayScope(/* SelectedUnitOverlay */ 0, [unitId]), ~~toInt32_unchecked(toInt64_unchecked(op_Modulus(handoff.Revision.Number, toInt64_unchecked(fromInt32(2147483647))))), append_1(new Float64Array([origin.CellColumn + 0.5, origin.CellRow + 0.5]), collect((p) => (new Float64Array([p.CellColumn + 0.5, p.CellRow + 0.5])), route, Float64Array), Float64Array), new Disclosure$1(/* Disclosed */ 3, [label])));
    let previewOverlay;
    let option_6;
    const option_4 = handoff.PreviewDestination;
    option_6 = ((option_4 != null) ? MapEditorSimulator_preview(selectedUnitId, option_4, handoff) : undefined);
    previewOverlay = ((option_6 != null) ? ((route_1 = option_6, routeOverlay(equals(route_1.Collision, SimulatorCollision.RouteClear) ? "route-preview-clear" : "route-preview-collision", route_1.UnitId, route_1.Origin, route_1.Route, (("Route " + int32ToString(route_1.Distance)) + " steps; ") + (equals(route_1.Collision, SimulatorCollision.RouteClear) ? "clear" : ("collision: " + toString(route_1.Collision)))))) : undefined);
    let plannedOverlay;
    const option_12 = selectedUnitId;
    if (option_12 != null) {
        const unitId_1 = option_12 | 0;
        const option_10 = tryFind(unitId_1, handoff.PlannedRoutes);
        if (option_10 != null) {
            const route_2 = option_10;
            const option_8 = tryFind(unitId_1, handoff.RuntimeMap.Units);
            plannedOverlay = ((option_8 != null) ? ((unit = option_8, routeOverlay("route-planned", unitId_1, new EditorCellAddress(unit.Column, unit.Row), toArray(route_2), ("Queued path with " + int32ToString(length_1(route_2))) + " remaining steps."))) : undefined);
        }
        else {
            plannedOverlay = undefined;
        }
    }
    else {
        plannedOverlay = undefined;
    }
    const combatEvents = toArray(mapIndexed((index, combat) => {
        let matchValue, matchValue_1;
        return new RenderEventVisual(((combat.Tick * 1000) + 500) + index, combat.Tick, (matchValue = combat.Delivery, (matchValue.tag === 1) ? "combat-projectile" : ((matchValue.tag === 2) ? "combat-lobbed-area" : ((matchValue.tag === 3) ? "combat-spell-area" : "combat-melee"))), new Disclosure$1(/* Disclosed */ 3, [combat.SourceUnitId]), (matchValue_1 = combat.Target, (matchValue_1.tag === 1) ? Disclosure$1.NotApplicable : (new Disclosure$1(/* Disclosed */ 3, [matchValue_1.fields[0]]))), new Disclosure$1(/* Disclosed */ 3, [combat.Summary]));
    }, handoff.LastCombatEvents));
    const combatSummaries = ofList_1(map_1((_arg) => _arg.Summary, handoff.LastCombatEvents), {
        Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
    });
    return new RenderFrame(baseFrame.Tick, baseFrame.Board, baseFrame.Units, baseFrame.Edges, toArray_1(orElse(previewOverlay, plannedOverlay)), append_1(baseFrame.Events.filter((event) => {
        const matchValue_2 = event.Summary;
        if (matchValue_2.tag === 3) {
            return !contains(matchValue_2.fields[0], combatSummaries);
        }
        else {
            return true;
        }
    }), combatEvents), baseFrame.Disclosure);
}

export function MapEditorSimulator_viewState(selectedUnitId, handoff) {
    let option_1;
    return new MapEditorState(handoff.RuntimeMap, initial.Tool, initial.TerrainSelection, initial.BrushSize, initial.TerrainCursor, initial.KeyboardCursor, initial.KeyboardObject, initial.LastTerrainPaintTool, initial.TerrainAnnouncement, initial.EdgeCursor, initial.EdgeAnnouncement, initial.UnitPaletteSearch, initial.UnitPaletteCursor, initial.UnitPlacementCursor, initial.UnitAnnouncement, initial.RegionAnnouncement, initial.RegionKeyboardMode, selectedUnitId, defaultArg((option_1 = selectedUnitId, (option_1 != null) ? singleton_1(option_1, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }) : undefined), empty_2({
        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
    })), initial.SelectedRegion, initial.Gesture, handoff.Revision, RevisionState_1.SimulatedRevision, initial.SavedDigest, handoff.Revision.Digest, initial.RecoveredFromDigest, initial.UndoHistory, initial.RedoHistory, initial.HistoryBytes, initial.Clipboard, handoff.Tick, handoff.IsRunning, handoff.LastEvents, initial.Validation, initial.Layers, initial.Issues, initial.ActiveIssue, initial.PendingDestructiveChange, initial.PendingRecovery, initial.Authoring);
}

