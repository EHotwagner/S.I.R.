
import { toString, Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { bool_type, array_type, option_type, float64_type, record_type, int32_type, string_type, union_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { UnitClassIdModule_resolve, SecondaryHeadingVisual, SecondaryHeadingSource, HeadingRadiansModule_ofDirection8, CellExtentModule_tryCreate, FactionVisual, Disclosure$1, DisclosureLabel, UnitVisual, EdgeVisual, BoardVisual, RenderFrame_$reflection, EdgeVisual_$reflection, BoardVisual_$reflection, DisclosureLabel_$reflection, Disclosure$1_$reflection, UnitVisual_$reflection } from "./ReplayPresentation.js";
import { MapTerrain, MapDefinition_$reflection, MapEditorState_$reflection } from "./MapEditorTypes.js";
import { EditorWorkspaceState_$reflection } from "./MapEditorWorkspace.js";
import { PlanningCommandKind, PlanningWorkspaceState_$reflection } from "./PlanningWorkspace.js";
import { BattlefieldCamera_$reflection } from "./Battlefield.js";
import { MapEditorSimulator_frame, SimulatorHandoff_$reflection } from "./MapEditorSimulator.js";
import { min, isInfinity, max } from "../fable_modules/fable-library-js.5.13.0/Double.js";
import { partition, concat, tryFind as tryFind_1, append as append_1, sortBy, mapIndexed, tryHead, collect, copy, map as map_1, initialize } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { value as value_7, orElse, toArray as toArray_2, defaultArg } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { FSharpMap__get_Item, ofArray as ofArray_2, containsKey, toArray, tryFind } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { FSharpSet__get_Count, contains, ofArray } from "../fable_modules/fable-library-js.5.13.0/Set.js";
import { int64ToString, Exception, compare, equals, stringHash, numberHash, comparePrimitives } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { empty, singleton, append, delay, filter, sort, toArray as toArray_1 } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { Array_distinctBy, distinct } from "../fable_modules/fable-library-js.5.13.0/Seq2.js";
import { frame as frame_2 } from "./MapEditor.js";
import { isEmpty, filter as filter_1, map as map_2, toArray as toArray_3, tryFind as tryFind_2, exists, choose as choose_1, tryLast, ofArray as ofArray_1 } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { join, contains as contains_1 } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { ReplayKind, Shell_renderFrame } from "./Shell.js";

export class SceneProjectionOwner extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["EditorScene", "PlanningScene", "SimulatorScene", "ReviewScene"];
    }
    static EditorScene = new SceneProjectionOwner(0, []);
    static PlanningScene = new SceneProjectionOwner(1, []);
    static SimulatorScene = new SceneProjectionOwner(2, []);
    static ReviewScene = new SceneProjectionOwner(3, []);
}

export function SceneProjectionOwner_$reflection() {
    return union_type("SIR.Client.SceneProjectionOwner", [], SceneProjectionOwner, () => [[], [], [], []]);
}

export class ScenePrimitiveId extends Union {
    constructor(Item) {
        super();
        this.tag = 0;
        this.fields = [Item];
    }
    cases() {
        return ["ScenePrimitiveId"];
    }
}

export function ScenePrimitiveId_$reflection() {
    return union_type("SIR.Client.ScenePrimitiveId", [], ScenePrimitiveId, () => [[["Item", string_type]]]);
}

export function ScenePrimitiveIdModule_value(_arg) {
    return _arg.fields[0];
}

export class SceneTerrainProjection extends Record {
    constructor(PrimitiveId, Column, Row, Kind) {
        super();
        this.PrimitiveId = PrimitiveId;
        this.Column = (Column | 0);
        this.Row = (Row | 0);
        this.Kind = Kind;
    }
}

export function SceneTerrainProjection_$reflection() {
    return record_type("SIR.Client.SceneTerrainProjection", [], SceneTerrainProjection, () => [["PrimitiveId", ScenePrimitiveId_$reflection()], ["Column", int32_type], ["Row", int32_type], ["Kind", string_type]]);
}

export class SceneUnitProjection extends Record {
    constructor(PrimitiveId, Visual, PresentationColumn, PresentationRow) {
        super();
        this.PrimitiveId = PrimitiveId;
        this.Visual = Visual;
        this.PresentationColumn = PresentationColumn;
        this.PresentationRow = PresentationRow;
    }
}

export function SceneUnitProjection_$reflection() {
    return record_type("SIR.Client.SceneUnitProjection", [], SceneUnitProjection, () => [["PrimitiveId", ScenePrimitiveId_$reflection()], ["Visual", UnitVisual_$reflection()], ["PresentationColumn", float64_type], ["PresentationRow", float64_type]]);
}

export class SceneRouteProjection extends Record {
    constructor(PrimitiveId, OwnerUnitId, Kind, Points, Label) {
        super();
        this.PrimitiveId = PrimitiveId;
        this.OwnerUnitId = OwnerUnitId;
        this.Kind = Kind;
        this.Points = Points;
        this.Label = Label;
    }
}

export function SceneRouteProjection_$reflection() {
    return record_type("SIR.Client.SceneRouteProjection", [], SceneRouteProjection, () => [["PrimitiveId", ScenePrimitiveId_$reflection()], ["OwnerUnitId", option_type(int32_type)], ["Kind", string_type], ["Points", array_type(float64_type)], ["Label", Disclosure$1_$reflection(string_type)]]);
}

export class SceneAnnotationProjection extends Record {
    constructor(PrimitiveId, Kind, Column, Row, Text$) {
        super();
        this.PrimitiveId = PrimitiveId;
        this.Kind = Kind;
        this.Column = Column;
        this.Row = Row;
        this.Text = Text$;
    }
}

export function SceneAnnotationProjection_$reflection() {
    return record_type("SIR.Client.SceneAnnotationProjection", [], SceneAnnotationProjection, () => [["PrimitiveId", ScenePrimitiveId_$reflection()], ["Kind", string_type], ["Column", option_type(int32_type)], ["Row", option_type(int32_type)], ["Text", Disclosure$1_$reflection(string_type)]]);
}

export class SceneDisclosureProjection extends Record {
    constructor(Source, PerspectiveFiltered, PreservesFieldDisclosures) {
        super();
        this.Source = Source;
        this.PerspectiveFiltered = PerspectiveFiltered;
        this.PreservesFieldDisclosures = PreservesFieldDisclosures;
    }
}

export function SceneDisclosureProjection_$reflection() {
    return record_type("SIR.Client.SceneDisclosureProjection", [], SceneDisclosureProjection, () => [["Source", DisclosureLabel_$reflection()], ["PerspectiveFiltered", bool_type], ["PreservesFieldDisclosures", bool_type]]);
}

export class SceneCameraProjection extends Record {
    constructor(PanX, PanY, Zoom) {
        super();
        this.PanX = PanX;
        this.PanY = PanY;
        this.Zoom = Zoom;
    }
}

export function SceneCameraProjection_$reflection() {
    return record_type("SIR.Client.SceneCameraProjection", [], SceneCameraProjection, () => [["PanX", float64_type], ["PanY", float64_type], ["Zoom", float64_type]]);
}

export class SceneSelectionProjection extends Record {
    constructor(SelectedUnits, FocusedUnit, SelectedRegion, SelectedCommand, SelectedEvent, SelectedPrimitiveIds) {
        super();
        this.SelectedUnits = SelectedUnits;
        this.FocusedUnit = FocusedUnit;
        this.SelectedRegion = SelectedRegion;
        this.SelectedCommand = SelectedCommand;
        this.SelectedEvent = SelectedEvent;
        this.SelectedPrimitiveIds = SelectedPrimitiveIds;
    }
}

export function SceneSelectionProjection_$reflection() {
    return record_type("SIR.Client.SceneSelectionProjection", [], SceneSelectionProjection, () => [["SelectedUnits", array_type(int32_type)], ["FocusedUnit", option_type(int32_type)], ["SelectedRegion", option_type(int32_type)], ["SelectedCommand", option_type(string_type)], ["SelectedEvent", option_type(int32_type)], ["SelectedPrimitiveIds", array_type(ScenePrimitiveId_$reflection())]]);
}

export class SceneLayerProjection extends Record {
    constructor(PrimitiveId, Kind, Order, Visible, Locked) {
        super();
        this.PrimitiveId = PrimitiveId;
        this.Kind = Kind;
        this.Order = (Order | 0);
        this.Visible = Visible;
        this.Locked = Locked;
    }
}

export function SceneLayerProjection_$reflection() {
    return record_type("SIR.Client.SceneLayerProjection", [], SceneLayerProjection, () => [["PrimitiveId", ScenePrimitiveId_$reflection()], ["Kind", string_type], ["Order", int32_type], ["Visible", bool_type], ["Locked", bool_type]]);
}

export class SharedSceneProjection extends Record {
    constructor(Owner, RevisionIdentity, Tick, Board, Terrain, Edges, Units, Routes, Annotations, Disclosure, Camera, Selection$, Layers) {
        super();
        this.Owner = Owner;
        this.RevisionIdentity = RevisionIdentity;
        this.Tick = (Tick | 0);
        this.Board = Board;
        this.Terrain = Terrain;
        this.Edges = Edges;
        this.Units = Units;
        this.Routes = Routes;
        this.Annotations = Annotations;
        this.Disclosure = Disclosure;
        this.Camera = Camera;
        this.Selection = Selection$;
        this.Layers = Layers;
    }
}

export function SharedSceneProjection_$reflection() {
    return record_type("SIR.Client.SharedSceneProjection", [], SharedSceneProjection, () => [["Owner", SceneProjectionOwner_$reflection()], ["RevisionIdentity", string_type], ["Tick", int32_type], ["Board", BoardVisual_$reflection()], ["Terrain", array_type(SceneTerrainProjection_$reflection())], ["Edges", array_type(EdgeVisual_$reflection())], ["Units", array_type(SceneUnitProjection_$reflection())], ["Routes", array_type(SceneRouteProjection_$reflection())], ["Annotations", array_type(SceneAnnotationProjection_$reflection())], ["Disclosure", SceneDisclosureProjection_$reflection()], ["Camera", SceneCameraProjection_$reflection()], ["Selection", SceneSelectionProjection_$reflection()], ["Layers", array_type(SceneLayerProjection_$reflection())]]);
}

export class EditorProjectionInput extends Record {
    constructor(EditorState, EditorWorkspace, EditorFocusedUnit) {
        super();
        this.EditorState = EditorState;
        this.EditorWorkspace = EditorWorkspace;
        this.EditorFocusedUnit = EditorFocusedUnit;
    }
}

export function EditorProjectionInput_$reflection() {
    return record_type("SIR.Client.EditorProjectionInput", [], EditorProjectionInput, () => [["EditorState", MapEditorState_$reflection()], ["EditorWorkspace", EditorWorkspaceState_$reflection()], ["EditorFocusedUnit", option_type(int32_type)]]);
}

export class PlanningProjectionInput extends Record {
    constructor(PlanningMap, PlanningState, PlanningCamera, PlanningFocusedUnit) {
        super();
        this.PlanningMap = PlanningMap;
        this.PlanningState = PlanningState;
        this.PlanningCamera = PlanningCamera;
        this.PlanningFocusedUnit = PlanningFocusedUnit;
    }
}

export function PlanningProjectionInput_$reflection() {
    return record_type("SIR.Client.PlanningProjectionInput", [], PlanningProjectionInput, () => [["PlanningMap", MapDefinition_$reflection()], ["PlanningState", PlanningWorkspaceState_$reflection()], ["PlanningCamera", BattlefieldCamera_$reflection()], ["PlanningFocusedUnit", option_type(int32_type)]]);
}

export class SimulatorProjectionInput extends Record {
    constructor(SimulatorHandoff, SimulatorSelectedUnit, SimulatorCamera, SimulatorFocusedUnit) {
        super();
        this.SimulatorHandoff = SimulatorHandoff;
        this.SimulatorSelectedUnit = SimulatorSelectedUnit;
        this.SimulatorCamera = SimulatorCamera;
        this.SimulatorFocusedUnit = SimulatorFocusedUnit;
    }
}

export function SimulatorProjectionInput_$reflection() {
    return record_type("SIR.Client.SimulatorProjectionInput", [], SimulatorProjectionInput, () => [["SimulatorHandoff", SimulatorHandoff_$reflection()], ["SimulatorSelectedUnit", option_type(int32_type)], ["SimulatorCamera", BattlefieldCamera_$reflection()], ["SimulatorFocusedUnit", option_type(int32_type)]]);
}

export class AcceptedReviewProjection extends Record {
    constructor(AcceptedFrame, AcceptedRevisionIdentity, AcceptedVerificationIdentity, AcceptedVerificationKind, AcceptedSelectedUnit, AcceptedSelectedEvent) {
        super();
        this.AcceptedFrame = AcceptedFrame;
        this.AcceptedRevisionIdentity = AcceptedRevisionIdentity;
        this.AcceptedVerificationIdentity = AcceptedVerificationIdentity;
        this.AcceptedVerificationKind = AcceptedVerificationKind;
        this.AcceptedSelectedUnit = AcceptedSelectedUnit;
        this.AcceptedSelectedEvent = AcceptedSelectedEvent;
    }
}

export function AcceptedReviewProjection_$reflection() {
    return record_type("SIR.Client.AcceptedReviewProjection", [], AcceptedReviewProjection, () => [["AcceptedFrame", RenderFrame_$reflection()], ["AcceptedRevisionIdentity", string_type], ["AcceptedVerificationIdentity", string_type], ["AcceptedVerificationKind", string_type], ["AcceptedSelectedUnit", option_type(int32_type)], ["AcceptedSelectedEvent", option_type(int32_type)]]);
}

export class ReviewProjectionInput extends Record {
    constructor(AcceptedReview, ReviewCamera, ReviewFocusedUnit) {
        super();
        this.AcceptedReview = AcceptedReview;
        this.ReviewCamera = ReviewCamera;
        this.ReviewFocusedUnit = ReviewFocusedUnit;
    }
}

export function ReviewProjectionInput_$reflection() {
    return record_type("SIR.Client.ReviewProjectionInput", [], ReviewProjectionInput, () => [["AcceptedReview", AcceptedReviewProjection_$reflection()], ["ReviewCamera", BattlefieldCamera_$reflection()], ["ReviewFocusedUnit", option_type(int32_type)]]);
}

function TacticalSceneProjection_invariant(value) {
    return String(value);
}

function TacticalSceneProjection_primitive(kind, identity) {
    return new ScenePrimitiveId((kind + ":") + identity);
}

function TacticalSceneProjection_boardOfMap(map) {
    return new BoardVisual(0, 0, map.Width - 1, map.Height - 1);
}

function TacticalSceneProjection_terrainName(_arg) {
    switch (_arg.tag) {
        case 1:
            return "rough";
        case 2:
            return "blocked";
        case 3:
            return "objective";
        default:
            return "open";
    }
}

function TacticalSceneProjection_terrainOfMap(map) {
    const width = max(0, map.Width) | 0;
    return initialize(width * max(0, map.Height), (index) => {
        const column = (index % width) | 0;
        const row = ~~(index / width) | 0;
        const terrain = defaultArg(tryFind([column, row], map.Terrain), MapTerrain.Open);
        return new SceneTerrainProjection(TacticalSceneProjection_primitive("terrain", (TacticalSceneProjection_invariant(column) + ":") + TacticalSceneProjection_invariant(row)), column, row, TacticalSceneProjection_terrainName(terrain));
    });
}

function TacticalSceneProjection_copyEdge(edge) {
    return new EdgeVisual(edge.Id, edge.Kind, edge.State, edge.StartColumn, edge.StartRow, edge.EndColumn, edge.EndRow);
}

function TacticalSceneProjection_edgesOfMap(map) {
    return map_1((tupledArg) => {
        const _arg = tupledArg[0];
        const _arg_1 = tupledArg[1];
        const row = _arg[1] | 0;
        const direction = _arg[2];
        const column = _arg[0] | 0;
        const kind = _arg_1[0];
        const patternInput = (direction.tag === 1) ? [column, row + 1, column + 1, row + 1] : [column + 1, row, column + 1, row + 1];
        return new EdgeVisual((((("editor-edge-" + TacticalSceneProjection_invariant(column)) + "-") + TacticalSceneProjection_invariant(row)) + "-") + toString(direction), (kind.tag === 1) ? "door" : ((kind.tag === 2) ? "window" : "wall"), _arg_1[1] ? "open" : "solid", patternInput[0], patternInput[1], patternInput[2], patternInput[3]);
    }, toArray(map.Edges));
}

function TacticalSceneProjection_copyVisual(unit) {
    return new UnitVisual(unit.Id, unit.AnchorColumn, unit.AnchorRow, unit.FootprintWidth, unit.FootprintDepth, unit.ClassId, unit.Faction, unit.Health, unit.Level, unit.StanceId, unit.BodyHeading, unit.SecondaryHeading, unit.ShortLabel, copy(unit.StatusIds));
}

function TacticalSceneProjection_unitsOfFrame(frame) {
    return map_1((unit) => (new SceneUnitProjection(TacticalSceneProjection_primitive("unit", TacticalSceneProjection_invariant(unit.Id)), TacticalSceneProjection_copyVisual(unit), unit.AnchorColumn, unit.AnchorRow)), frame.Units);
}

function TacticalSceneProjection_camera(value) {
    return new SceneCameraProjection(value.PanX, value.PanY, value.Zoom);
}

function TacticalSceneProjection_selectedUnits(candidates, focused, units) {
    let option_1;
    const visibleIds = ofArray(map_1((unit) => (unit.Visual.Id | 0), units, Int32Array), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    return [toArray_1(sort(distinct(filter((id) => contains(id, visibleIds), candidates), {
        Equals: (x_1, y_1) => (x_1 === y_1),
        GetHashCode: (x_1) => (numberHash(x_1) | 0),
    }), {
        Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
    })), (option_1 = focused, (option_1 != null) ? (contains(option_1, visibleIds) ? option_1 : undefined) : undefined)];
}

function TacticalSceneProjection_selection(selected, focused, region, command, event, extraPrimitiveIds) {
    return new SceneSelectionProjection(selected, focused, region, command, event, Array_distinctBy(ScenePrimitiveIdModule_value, toArray_1(delay(() => append(map_1((id) => TacticalSceneProjection_primitive("unit", TacticalSceneProjection_invariant(id)), selected), delay(() => extraPrimitiveIds)))), {
        Equals: (x, y) => (x === y),
        GetHashCode: (x) => (stringHash(x) | 0),
    }));
}

function TacticalSceneProjection_disclosure(source) {
    return new SceneDisclosureProjection(source, equals(source, DisclosureLabel.PerspectiveDisclosure), true);
}

function TacticalSceneProjection_layer(kind, order, visible, locked) {
    return new SceneLayerProjection(TacticalSceneProjection_primitive("layer", kind), kind, order, visible, locked);
}

const TacticalSceneProjection_standardLayers = [TacticalSceneProjection_layer("terrain", 0, true, false), TacticalSceneProjection_layer("edges", 1, true, false), TacticalSceneProjection_layer("units", 2, true, false), TacticalSceneProjection_layer("routes", 3, true, false), TacticalSceneProjection_layer("annotations", 4, true, false)];

function TacticalSceneProjection_editorLayer(domain, state, order) {
    const kind = (domain.tag === 1) ? "edges" : ((domain.tag === 2) ? "units" : ((domain.tag === 3) ? "annotations" : ((domain.tag === 4) ? "document" : "terrain")));
    switch (state.tag) {
        case 1:
            return TacticalSceneProjection_layer(kind, order, true, false);
        case 2:
            return TacticalSceneProjection_layer(kind, order, false, false);
        case 3:
            return TacticalSceneProjection_layer(kind, order, true, true);
        default:
            return TacticalSceneProjection_layer(kind, order, true, false);
    }
}

function TacticalSceneProjection_routePoints(cells) {
    return collect((tupledArg) => (new Float64Array([tupledArg[0] + 0.5, tupledArg[1] + 0.5])), cells, Float64Array);
}

function TacticalSceneProjection_eventAnnotations(prefix, events) {
    return map_1((event) => (new SceneAnnotationProjection(TacticalSceneProjection_primitive(prefix, TacticalSceneProjection_invariant(event.Id)), event.Kind, undefined, undefined, event.Summary)), events);
}

function TacticalSceneProjection_regionAnnotations(regions) {
    return map_1((tupledArg) => {
        let option_1, value, matchValue_1;
        const id = tupledArg[0] | 0;
        const region = tupledArg[1];
        let patternInput;
        const matchValue = region.Geometry;
        patternInput = ((matchValue.tag === 1) ? defaultArg((option_1 = tryHead(matchValue.fields[0]), (option_1 != null) ? ((value = option_1, [value.CellColumn, value.CellRow])) : undefined), [undefined, undefined]) : [matchValue.fields[0], matchValue.fields[1]]);
        return new SceneAnnotationProjection(TacticalSceneProjection_primitive("region", TacticalSceneProjection_invariant(id)), (matchValue_1 = region.Purpose, (matchValue_1.tag === 1) ? ((matchValue_1.fields[0].tag === 1) ? "red-deployment" : ((matchValue_1.fields[0].tag === 2) ? "neutral-deployment" : "blue-deployment")) : "objective-region"), patternInput[0], patternInput[1], new Disclosure$1(/* Disclosed */ 3, ["Region " + TacticalSceneProjection_invariant(id)]));
    }, toArray(regions));
}

export function TacticalSceneProjection_editor(input) {
    let option_4;
    const frame = frame_2(input.EditorState);
    const units = TacticalSceneProjection_unitsOfFrame(frame);
    const patternInput = TacticalSceneProjection_selectedUnits(delay(() => append(input.EditorState.SelectedUnits, delay(() => ofArray_1(toArray_2(input.EditorState.SelectedUnit))))), input.EditorFocusedUnit, units);
    let selectedRegion;
    const option_2 = input.EditorState.SelectedRegion;
    selectedRegion = ((option_2 != null) ? (containsKey(option_2, input.EditorState.Map.Regions) ? option_2 : undefined) : undefined);
    const layers = mapIndexed((order, tupledArg) => TacticalSceneProjection_editorLayer(tupledArg[0], tupledArg[1], order), sortBy((tuple) => tuple[0], toArray(input.EditorState.Layers), {
        Compare: (x, y) => (compare(x, y) | 0),
    }));
    return new SharedSceneProjection(SceneProjectionOwner.EditorScene, input.EditorState.Revision.Digest, input.EditorState.Tick, frame.Board, TacticalSceneProjection_terrainOfMap(input.EditorState.Map), map_1(TacticalSceneProjection_copyEdge, frame.Edges), units, [], append_1(TacticalSceneProjection_regionAnnotations(input.EditorState.Map.Regions), TacticalSceneProjection_eventAnnotations("editor-event", frame.Events)), TacticalSceneProjection_disclosure(DisclosureLabel.SandboxDisclosure), TacticalSceneProjection_camera(input.EditorWorkspace.Camera), TacticalSceneProjection_selection(patternInput[0], patternInput[1], selectedRegion, undefined, undefined, defaultArg((option_4 = selectedRegion, (option_4 != null) ? [TacticalSceneProjection_primitive("region", TacticalSceneProjection_invariant(option_4))] : undefined), [])), layers);
}

function TacticalSceneProjection_planningUnit(map, commands, member$0027) {
    let option_11, option_14, option_17, option_20;
    const authored = tryFind(member$0027.UnitId, map.Units);
    let faction;
    let matchValue;
    const option_1 = authored;
    matchValue = ((option_1 != null) ? option_1.Side : undefined);
    if (matchValue == null) {
        const matchValue_1 = member$0027.Side.toLowerCase();
        switch (matchValue_1) {
            case "blue": {
                faction = FactionVisual.Human;
                break;
            }
            case "red": {
                faction = FactionVisual.Arcane;
                break;
            }
            case "neutralside":
            case "neutral": {
                faction = FactionVisual.Neutral;
                break;
            }
            default:
                faction = (new FactionVisual(/* OtherFaction */ 3, [matchValue_1]));
        }
    }
    else {
        faction = ((matchValue.tag === 1) ? FactionVisual.Arcane : ((matchValue.tag === 2) ? FactionVisual.Neutral : FactionVisual.Human));
    }
    let footprint;
    let option_9;
    let option_5;
    const option_3 = authored;
    option_5 = ((option_3 != null) ? option_3.Size : undefined);
    option_9 = ((option_5 != null) ? CellExtentModule_tryCreate(option_5) : undefined);
    if (option_9 != null) {
        footprint = option_9;
    }
    else {
        const option_7 = CellExtentModule_tryCreate(1);
        if (option_7 != null) {
            footprint = option_7;
        }
        else {
            throw new Exception("One-cell planning footprint was invalid.");
        }
    }
    const latest = (choose) => tryLast(choose_1((command) => {
        if (command.UnitId === member$0027.UnitId) {
            return choose(command.Kind);
        }
        else {
            return undefined;
        }
    }, commands));
    const bodyHeading = defaultArg((option_11 = latest((_arg_2) => ((_arg_2.tag === 1) ? _arg_2.fields[0] : undefined)), (option_11 != null) ? (new Disclosure$1(/* Disclosed */ 3, [HeadingRadiansModule_ofDirection8(option_11)])) : undefined), Disclosure$1.NotPresent);
    const attentionHeading = defaultArg((option_14 = latest((_arg_3) => ((_arg_3.tag === 2) ? _arg_3.fields[0] : undefined)), (option_14 != null) ? (new Disclosure$1(/* Disclosed */ 3, [new SecondaryHeadingVisual(HeadingRadiansModule_ofDirection8(option_14), SecondaryHeadingSource.AttentionHeading)])) : undefined), Disclosure$1.NotPresent);
    const stance = defaultArg((option_17 = latest((_arg_4) => ((_arg_4.tag === 3) ? _arg_4.fields[0] : undefined)), (option_17 != null) ? (new Disclosure$1(/* Disclosed */ 3, [option_17])) : undefined), Disclosure$1.NotPresent);
    const statusIds = toArray_1(delay(() => append(singleton("planning"), delay(() => append(exists((command_1) => {
        if (command_1.UnitId === member$0027.UnitId) {
            return equals(command_1.Kind, PlanningCommandKind.PlannedHold);
        }
        else {
            return false;
        }
    }, commands) ? singleton("hold") : empty(), delay(() => append(exists((command_2) => {
        if (command_2.UnitId === member$0027.UnitId) {
            if (command_2.Kind.tag === 5) {
                return true;
            }
            else {
                return false;
            }
        }
        else {
            return false;
        }
    }, commands) ? singleton("engagement") : empty(), delay(() => (exists((command_3) => {
        if (command_3.UnitId === member$0027.UnitId) {
            if (command_3.Kind.tag === 6) {
                return true;
            }
            else {
                return false;
            }
        }
        else {
            return false;
        }
    }, commands) ? singleton("synchronization") : empty())))))))));
    return new SceneUnitProjection(TacticalSceneProjection_primitive("unit", TacticalSceneProjection_invariant(member$0027.UnitId)), new UnitVisual(member$0027.UnitId, member$0027.Column, member$0027.Row, footprint, footprint, UnitClassIdModule_resolve(defaultArg((option_20 = authored, (option_20 != null) ? option_20.ClassId : undefined), member$0027.Role)), faction, Disclosure$1.NotPresent, Disclosure$1.NotPresent, stance, bodyHeading, attentionHeading, new Disclosure$1(/* Disclosed */ 3, [member$0027.Name]), statusIds), member$0027.Column, member$0027.Row);
}

function TacticalSceneProjection_planningAnnotation(roster, command) {
    let option_1, option_3;
    let patternInput;
    const matchValue = command.Kind;
    patternInput = ((matchValue.tag === 1) ? ["facing", "Facing " + toString(matchValue.fields[0])] : ((matchValue.tag === 2) ? ["attention", "Attention " + toString(matchValue.fields[0])] : ((matchValue.tag === 3) ? ["stance", "Stance " + matchValue.fields[0]] : ((matchValue.tag === 4) ? ["hold", "Hold"] : ((matchValue.tag === 5) ? ["engagement", (("Engage " + TacticalSceneProjection_invariant(matchValue.fields[0])) + " with ") + matchValue.fields[1]] : ((matchValue.tag === 6) ? ["synchronization", (matchValue.fields[0] + " by ") + TacticalSceneProjection_invariant(matchValue.fields[1])] : ["route", "Route"]))))));
    const owner = tryFind_1((unit) => (unit.UnitId === command.UnitId), roster);
    return new SceneAnnotationProjection(TacticalSceneProjection_primitive("plan-command", command.Id), patternInput[0], (option_1 = owner, (option_1 != null) ? option_1.Column : undefined), (option_3 = owner, (option_3 != null) ? option_3.Row : undefined), new Disclosure$1(/* Disclosed */ 3, [patternInput[1]]));
}

function TacticalSceneProjection_planningIssueAnnotation(state, index, issue) {
    let option_3, option_1, commandId, option_8, option_10;
    let owner;
    const option_6 = orElse(issue.UnitId, (option_3 = ((option_1 = issue.CommandId, (option_1 != null) ? ((commandId = option_1, tryFind_2((command) => (command.Id === commandId), state.Commands))) : undefined)), (option_3 != null) ? option_3.UnitId : undefined));
    if (option_6 != null) {
        const id = option_6 | 0;
        owner = tryFind_1((unit) => (unit.UnitId === id), state.Roster);
    }
    else {
        owner = undefined;
    }
    return new SceneAnnotationProjection(TacticalSceneProjection_primitive("planning-issue", TacticalSceneProjection_invariant(index)), "validation", (option_8 = owner, (option_8 != null) ? option_8.Column : undefined), (option_10 = owner, (option_10 != null) ? option_10.Row : undefined), new Disclosure$1(/* Disclosed */ 3, [(issue.Code + " · ") + issue.Detail]));
}

export function TacticalSceneProjection_planning(input) {
    let option_6, prediction;
    const units = map_1((member$0027) => TacticalSceneProjection_planningUnit(input.PlanningMap, input.PlanningState.Commands, member$0027), sortBy((_arg) => (_arg.UnitId | 0), input.PlanningState.Roster, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }));
    const authoredRoutes = toArray_3(choose_1((command) => {
        const matchValue = command.Kind;
        if (matchValue.tag === 0) {
            return new SceneRouteProjection(TacticalSceneProjection_primitive("route", command.Id), command.UnitId, "planned", TacticalSceneProjection_routePoints(matchValue.fields[0]), new Disclosure$1(/* Disclosed */ 3, ["Planned route for unit " + TacticalSceneProjection_invariant(command.UnitId)]));
        }
        else {
            return undefined;
        }
    }, input.PlanningState.Commands));
    const patternInput = TacticalSceneProjection_selectedUnits(ofArray_1(toArray_2(input.PlanningState.SelectedUnit)), input.PlanningFocusedUnit, units);
    let patternInput_1;
    let _arg_1;
    const option_4 = input.PlanningState.SelectedCommand;
    if (option_4 != null) {
        const selectedId = option_4;
        const option_2 = tryFind_2((command_1) => (command_1.Id === selectedId), input.PlanningState.Commands);
        _arg_1 = ((option_2 != null) ? [selectedId, TacticalSceneProjection_primitive((option_2.Kind.tag === 0) ? "route" : "plan-command", selectedId)] : undefined);
    }
    else {
        _arg_1 = undefined;
    }
    patternInput_1 = ((_arg_1 == null) ? [undefined, []] : [_arg_1[0], [_arg_1[1]]]);
    return new SharedSceneProjection(SceneProjectionOwner.PlanningScene, (input.PlanningState.MapRevision + ":") + input.PlanningState.Digest, input.PlanningState.AuthoringTick, TacticalSceneProjection_boardOfMap(input.PlanningMap), TacticalSceneProjection_terrainOfMap(input.PlanningMap), TacticalSceneProjection_edgesOfMap(input.PlanningMap), units, authoredRoutes, concat([toArray_3(map_2((command_5) => TacticalSceneProjection_planningAnnotation(input.PlanningState.Roster, command_5), filter_1((command_4) => {
        if (command_4.Kind.tag === 0) {
            return false;
        }
        else {
            return true;
        }
    }, input.PlanningState.Commands))), mapIndexed((index, issue) => TacticalSceneProjection_planningIssueAnnotation(input.PlanningState, index, issue), input.PlanningState.Issues), defaultArg((option_6 = input.PlanningState.Predicted, (option_6 != null) ? ((prediction = option_6, mapIndexed((index_1, disclosure) => (new SceneAnnotationProjection(TacticalSceneProjection_primitive("prediction", (int64ToString(prediction.Revision) + ":") + TacticalSceneProjection_invariant(index_1)), "prediction", undefined, undefined, new Disclosure$1(/* Disclosed */ 3, [disclosure]))), prediction.Disclosures))) : undefined), [])]), TacticalSceneProjection_disclosure(DisclosureLabel.SandboxDisclosure), TacticalSceneProjection_camera(input.PlanningCamera), TacticalSceneProjection_selection(patternInput[0], patternInput[1], undefined, patternInput_1[0], undefined, patternInput_1[1]), copy(TacticalSceneProjection_standardLayers));
}

function TacticalSceneProjection_overlayRoute(overlay) {
    let matchValue;
    return new SceneRouteProjection(TacticalSceneProjection_primitive("route", overlay.Id), (matchValue = overlay.Scope, (matchValue.tag === 1) ? undefined : matchValue.fields[0]), overlay.Kind, copy(overlay.Points), overlay.Label);
}

function TacticalSceneProjection_simulatorOverlayRoute(overlay) {
    let matchValue, matchValue_1;
    return new SceneRouteProjection(TacticalSceneProjection_primitive("route", (("simulator:" + ((matchValue = overlay.Scope, (matchValue.tag === 1) ? "force" : TacticalSceneProjection_invariant(matchValue.fields[0])))) + ":") + (contains_1(overlay.Kind, "preview", 5) ? "preview" : "planned")), (matchValue_1 = overlay.Scope, (matchValue_1.tag === 1) ? undefined : matchValue_1.fields[0]), overlay.Kind, copy(overlay.Points), overlay.Label);
}

export function TacticalSceneProjection_simulator(input) {
    const frame = MapEditorSimulator_frame(input.SimulatorSelectedUnit, input.SimulatorHandoff);
    const units = map_1((projected) => {
        const visual = projected.Visual;
        const patternInput = defaultArg(tryFind(visual.Id, input.SimulatorHandoff.PresentationPositions), [visual.AnchorColumn, visual.AnchorRow]);
        return new SceneUnitProjection(projected.PrimitiveId, new UnitVisual(visual.Id, visual.AnchorColumn, visual.AnchorRow, visual.FootprintWidth, visual.FootprintDepth, visual.ClassId, visual.Faction, visual.Health, visual.Level, visual.StanceId, visual.BodyHeading, visual.SecondaryHeading, visual.ShortLabel, toArray_1(delay(() => append(visual.StatusIds, delay(() => append(containsKey(visual.Id, input.SimulatorHandoff.MovementProgress) ? singleton("moving") : empty(), delay(() => append(containsKey(visual.Id, input.SimulatorHandoff.MovementIntents) ? singleton("movement-intent") : empty(), delay(() => (containsKey(visual.Id, input.SimulatorHandoff.PlannedRoutes) ? singleton("route-planned") : empty())))))))))), patternInput[0], patternInput[1]);
    }, TacticalSceneProjection_unitsOfFrame(frame));
    const patternInput_1 = TacticalSceneProjection_selectedUnits(ofArray_1(toArray_2(input.SimulatorSelectedUnit)), input.SimulatorFocusedUnit, units);
    return new SharedSceneProjection(SceneProjectionOwner.SimulatorScene, input.SimulatorHandoff.Revision.Digest, input.SimulatorHandoff.Tick, frame.Board, TacticalSceneProjection_terrainOfMap(input.SimulatorHandoff.RuntimeMap), map_1(TacticalSceneProjection_copyEdge, frame.Edges), units, map_1(TacticalSceneProjection_simulatorOverlayRoute, frame.Overlays), append_1(map_1((unit) => {
        const visual_1 = unit.Visual;
        return new SceneAnnotationProjection(TacticalSceneProjection_primitive("simulator-state", TacticalSceneProjection_invariant(visual_1.Id)), "simulator-state", visual_1.AnchorColumn, visual_1.AnchorRow, new Disclosure$1(/* Disclosed */ 3, [(("Unit " + TacticalSceneProjection_invariant(visual_1.Id)) + " · ") + join(" · ", visual_1.StatusIds)]));
    }, units), TacticalSceneProjection_eventAnnotations("simulator-event", frame.Events)), TacticalSceneProjection_disclosure(DisclosureLabel.SandboxDisclosure), TacticalSceneProjection_camera(input.SimulatorCamera), TacticalSceneProjection_selection(patternInput_1[0], patternInput_1[1], undefined, undefined, undefined, []), copy(TacticalSceneProjection_standardLayers));
}

export function TacticalSceneProjection_acceptReview(model) {
    let inspection, inspection_1;
    const matchValue = model.Source;
    const matchValue_1 = model.Mode;
    const matchValue_2 = model.Verification;
    const matchValue_3 = model.Inspection;
    let matchResult, inspection_2, metadata_2, inspection_3, metadata_3;
    if (matchValue.tag === 2) {
        switch (matchValue_1.tag) {
            case 1: {
                if (matchValue_2.tag === 2) {
                    if (matchValue_3 != null) {
                        if ((inspection = matchValue_3, equals(matchValue.fields[0].Kind, ReplayKind.FullReplay) && (inspection.PerspectiveHash == null))) {
                            matchResult = 0;
                            inspection_2 = matchValue_3;
                            metadata_2 = matchValue.fields[0];
                        }
                        else {
                            matchResult = 2;
                        }
                    }
                    else {
                        matchResult = 2;
                    }
                }
                else {
                    matchResult = 2;
                }
                break;
            }
            case 2: {
                if (matchValue_2.tag === 3) {
                    if (matchValue_3 != null) {
                        if ((inspection_1 = matchValue_3, ((((((((equals(matchValue.fields[0].Kind, ReplayKind.PerspectiveReplay) && (inspection_1.PerspectiveHash != null)) && isEmpty(inspection_1.Units)) && isEmpty(inspection_1.Edges)) && isEmpty(inspection_1.Events)) && isEmpty(inspection_1.Checkpoints)) && (inspection_1.BoardMinimumColumn === 0)) && (inspection_1.BoardMinimumRow === 0)) && (inspection_1.BoardMaximumColumn === 0)) && (inspection_1.BoardMaximumRow === 0))) {
                            matchResult = 1;
                            inspection_3 = matchValue_3;
                            metadata_3 = matchValue.fields[0];
                        }
                        else {
                            matchResult = 2;
                        }
                    }
                    else {
                        matchResult = 2;
                    }
                }
                else {
                    matchResult = 2;
                }
                break;
            }
            default:
                matchResult = 2;
        }
    }
    else {
        matchResult = 2;
    }
    switch (matchResult) {
        case 0: {
            const option_1 = Shell_renderFrame(model);
            if (option_1 != null) {
                return new AcceptedReviewProjection(option_1, (("replay:" + metadata_2.SourceIdentity) + ":") + metadata_2.EngineIdentity, (metadata_2.SourceIdentity + " · ") + metadata_2.EngineIdentity, "browser-kernel-verified", model.Selection.Unit, model.Selection.Event);
            }
            else {
                return undefined;
            }
        }
        case 1: {
            const option_3 = Shell_renderFrame(model);
            if (option_3 != null) {
                return new AcceptedReviewProjection(option_3, (("replay:" + metadata_3.SourceIdentity) + ":") + metadata_3.EngineIdentity, (((metadata_3.SourceIdentity + " · ") + metadata_3.EngineIdentity) + " · ") + value_7(inspection_3.PerspectiveHash), "perspective-projection", model.Selection.Unit, model.Selection.Event);
            }
            else {
                return undefined;
            }
        }
        default:
            return undefined;
    }
}

export function TacticalSceneProjection_review(input) {
    let option_4;
    const frame = input.AcceptedReview.AcceptedFrame;
    const units = TacticalSceneProjection_unitsOfFrame(frame);
    const patternInput = TacticalSceneProjection_selectedUnits(ofArray_1(toArray_2(input.AcceptedReview.AcceptedSelectedUnit)), input.ReviewFocusedUnit, units);
    const patternInput_1 = partition((overlay) => contains_1(overlay.Kind, "route", 5), frame.Overlays);
    const overlayAnnotations = map_1((overlay_1) => (new SceneAnnotationProjection(TacticalSceneProjection_primitive("overlay", overlay_1.Id), overlay_1.Kind, undefined, undefined, overlay_1.Label)), patternInput_1[1]);
    const eventAnnotations = TacticalSceneProjection_eventAnnotations("review-event", frame.Events);
    const verificationAnnotation = new SceneAnnotationProjection(TacticalSceneProjection_primitive("review-verification", "accepted"), input.AcceptedReview.AcceptedVerificationKind, undefined, undefined, new Disclosure$1(/* Disclosed */ 3, ["Verification · " + input.AcceptedReview.AcceptedVerificationIdentity]));
    const visibleEvents = ofArray(map_1((_arg) => (_arg.Id | 0), frame.Events, Int32Array), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    let selectedEvent;
    const option_2 = input.AcceptedReview.AcceptedSelectedEvent;
    selectedEvent = ((option_2 != null) ? (contains(option_2, visibleEvents) ? option_2 : undefined) : undefined);
    return new SharedSceneProjection(SceneProjectionOwner.ReviewScene, input.AcceptedReview.AcceptedRevisionIdentity, frame.Tick, frame.Board, [], map_1(TacticalSceneProjection_copyEdge, frame.Edges), units, map_1(TacticalSceneProjection_overlayRoute, patternInput_1[0]), concat([overlayAnnotations, eventAnnotations, [verificationAnnotation]]), TacticalSceneProjection_disclosure(frame.Disclosure), TacticalSceneProjection_camera(input.ReviewCamera), TacticalSceneProjection_selection(patternInput[0], patternInput[1], undefined, undefined, selectedEvent, defaultArg((option_4 = selectedEvent, (option_4 != null) ? [TacticalSceneProjection_primitive("review-event", TacticalSceneProjection_invariant(option_4))] : undefined), [])), copy(TacticalSceneProjection_standardLayers));
}

/**
 * Interpolates only presentation coordinates for semantic units present
 * in both accepted Review frames. Current committed identity, tick,
 * disclosure, visual facts, events, annotations, and selection remain the
 * authoritative projection. A failed guard returns the exact current
 * projection with an effective alpha of one.
 */
export function TacticalSceneProjection_interpolateReviewPresentation(previousFrame, alpha, current) {
    const currentIds = ofArray(map_1((_arg) => (_arg.Visual.Id | 0), current.Units, Int32Array), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    const previousIds = ofArray(map_1((_arg_1) => (_arg_1.Id | 0), previousFrame.Units, Int32Array), {
        Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
    });
    if (!(((((((equals(current.Owner, SceneProjectionOwner.ReviewScene) && (previousFrame.Tick !== current.Tick)) && equals(previousFrame.Board, current.Board)) && equals(previousFrame.Disclosure, current.Disclosure.Source)) && currentIds.Equals(previousIds)) && (FSharpSet__get_Count(currentIds) === current.Units.length)) && (FSharpSet__get_Count(previousIds) === previousFrame.Units.length)) && !(Number.isNaN(alpha) ? true : isInfinity(alpha)))) {
        return [current, 1];
    }
    else {
        const effectiveAlpha = max(0, min(1, alpha));
        const previousById = ofArray_2(map_1((unit) => [unit.Id, unit], previousFrame.Units), {
            Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
        });
        return [new SharedSceneProjection(current.Owner, current.RevisionIdentity, current.Tick, current.Board, current.Terrain, current.Edges, map_1((unit_1) => {
            const previous = FSharpMap__get_Item(previousById, unit_1.Visual.Id);
            return new SceneUnitProjection(unit_1.PrimitiveId, unit_1.Visual, previous.AnchorColumn + ((unit_1.PresentationColumn - previous.AnchorColumn) * effectiveAlpha), previous.AnchorRow + ((unit_1.PresentationRow - previous.AnchorRow) * effectiveAlpha));
        }, current.Units), current.Routes, current.Annotations, current.Disclosure, current.Camera, current.Selection, current.Layers), effectiveAlpha];
    }
}

export function TacticalSceneProjection_primitiveIds(projection) {
    return toArray_1(delay(() => append(map_1((_arg) => _arg.PrimitiveId, projection.Terrain), delay(() => append(map_1((edge) => TacticalSceneProjection_primitive("edge", edge.Id), projection.Edges), delay(() => append(map_1((_arg_1) => _arg_1.PrimitiveId, projection.Units), delay(() => append(map_1((_arg_2) => _arg_2.PrimitiveId, projection.Routes), delay(() => append(map_1((_arg_3) => _arg_3.PrimitiveId, projection.Annotations), delay(() => map_1((_arg_4) => _arg_4.PrimitiveId, projection.Layers)))))))))))));
}

