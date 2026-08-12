
import { EditorClipboard, EditorBox, MapEditorAction, CrashRecoveryDraft, SavedMapView, EditorKeyboardObject, EditorHistoryEntry, ValidatedEditorCommand, MapIssue, MapRegion, RegionBehavior, RegionGeometry, RegionPurpose, EditorCommand, PendingDestructiveChange as PendingDestructiveChange_4, ResizeLossPreview, MapEditorState, MapAuthoringMetadata, EditorDomain, EditorLayerState, RevisionState as RevisionState_9, EditorGesture, RegionKeyboardMode as RegionKeyboardMode_26, UnitPaletteCursor as UnitPaletteCursor_3, TerrainAuthoringTool, EditorKeyboardCursor, EditorCellAddress, MapEditorTool, MapEdgeKind, EditorUnit, MapDefinition, MapEdgeDirection, MapController, MapTerrain, MapUnitFootprintPreset, MapSide } from "./MapEditorTypes.js";
import { mapIndexed as mapIndexed_1, findIndex, tryFindIndex, pairwise, tryPick, fold, toArray as toArray_2, skip, tail, head, iterate, reverse, sort, length as length_1, item as item_1, tryHead, contains as contains_1, collect as collect_1, forAll, isEmpty, append as append_1, choose, map as map_9, empty, cons, singleton, exists, filter, sortBy, tryFind, ofArray } from "../fable_modules/fable-library-js.5.13.0/List.js";
import { replace, format, join, split, isNullOrWhiteSpace, compare } from "../fable_modules/fable-library-js.5.13.0/String.js";
import { stringHash, int64ToString, structuralHash, numberHash, defaultOf, safeHash, disposeSafe, getEnumerator, arrayHash, equalArrays, equals, compare as compare_1, comparePrimitives, int32ToString, Exception, compareArrays } from "../fable_modules/fable-library-js.5.13.0/Util.js";
import { RenderFrame, DisclosureLabel, RenderEventVisual, UnitVisual, SecondaryHeadingVisual, SecondaryHeadingSource, HeadingRadiansModule_ofDirection8, Disclosure$1, FactionVisual, UnitClassIdModule_resolve, BoardVisual, EdgeVisual, HealthVisualModule_tryCreate, CellExtentModule_tryCreate } from "./ReplayPresentation.js";
import { tryHead as tryHead_1, setItem, copy, take, contains as contains_2, tryPick as tryPick_1, maxBy, minBy, sortBy as sortBy_2, tryFind as tryFind_2, fold as fold_1, choose as choose_1, mapIndexed, sum, equalsWith, item as item_2, append as append_2, map as map_8 } from "../fable_modules/fable-library-js.5.13.0/Array.js";
import { max, min } from "../fable_modules/fable-library-js.5.13.0/Double.js";
import { Direction8 } from "../SIR.Domain/Orientation.js";
import { Result_IsOk, Result_Bind, Result_Map, FSharpResult$2 } from "../fable_modules/fable-library-js.5.13.0/Result.js";
import { pairwise as pairwise_1, empty as empty_3, filter as filter_2, sortBy as sortBy_1, toArray as toArray_1, exists as exists_1, map as map_10, collect, singleton as singleton_1, append, delay, toList } from "../fable_modules/fable-library-js.5.13.0/Seq.js";
import { map as map_11, toSeq, ofSeq, FSharpMap__get_Count, containsKey, remove, filter as filter_1, add, tryFind as tryFind_1, ofList, empty as empty_1, toList as toList_1 } from "../fable_modules/fable-library-js.5.13.0/Map.js";
import { sha256 } from "../SIR.Domain/CanonicalHash.js";
import { get_UTF8 } from "../fable_modules/fable-library-js.5.13.0/Encoding.js";
import { create } from "./MapEditorRevision.js";
import { rangeDouble } from "../fable_modules/fable-library-js.5.13.0/Range.js";
import { ofArray as ofArray_1, union, remove as remove_1, FSharpSet__get_IsEmpty, toArray as toArray_3, toList as toList_2, filter as filter_3, add as add_1, empty as empty_2, contains, ofList as ofList_1, singleton as singleton_2 } from "../fable_modules/fable-library-js.5.13.0/Set.js";
import { value as value_18, defaultArg, toArray } from "../fable_modules/fable-library-js.5.13.0/Option.js";
import { Array_groupBy, List_groupBy, List_distinct, Array_distinct, countBy, distinct } from "../fable_modules/fable-library-js.5.13.0/Seq2.js";
import { tryParse, op_UnaryNegation_Int32 } from "../fable_modules/fable-library-js.5.13.0/Int32.js";
import { Queue$1__Dequeue, Queue$1__get_Count, Queue$1__Enqueue_2B595, Queue$1_$ctor } from "../fable_modules/fable-library-js.5.13.0/System.Collections.Generic.js";
import { isWhiteSpace } from "../fable_modules/fable-library-js.5.13.0/Char.js";
import { FSharpRef, toString } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { op_Addition, fromInt32, op_Multiply, op_Subtraction, toInt64_unchecked, equals as equals_1 } from "../fable_modules/fable-library-js.5.13.0/BigInt.js";
import { hasSupportedDimensions } from "./MapEditorValidation.js";
import { size as size_8, withinBounds } from "./MapEditorHistory.js";

export const canonicalFootprintPresets = ofArray([new MapUnitFootprintPreset("goblin", "Goblin skirmisher", "Arcane", "Skirmisher", "goblin", "goblin", MapSide.Red, 2, 35, 35), new MapUnitFootprintPreset("orc", "Orc assault", "Arcane", "Assault", "orc", "orc", MapSide.Red, 4, 100, 100), new MapUnitFootprintPreset("troll", "Armored troll", "Arcane", "Heavy", "troll", "troll", MapSide.Red, 6, 240, 240), new MapUnitFootprintPreset("human", "Human rifleman", "Human", "Line infantry", "rifleman", "rifleman", MapSide.Blue, 4, 12, 12), new MapUnitFootprintPreset("human-gunner", "Human gunner", "Human", "Area fire", "gunner", "gunner", MapSide.Blue, 4, 12, 12), new MapUnitFootprintPreset("human-marksman", "Human marksman", "Human", "Precision fire", "marksman", "marksman", MapSide.Blue, 4, 12, 12), new MapUnitFootprintPreset("human-engineer", "Human engineer", "Human", "Breaching and fieldworks", "engineer", "engineer", MapSide.Blue, 4, 12, 12), new MapUnitFootprintPreset("human-medic", "Human medic", "Human", "Casualty care", "medic", "medic", MapSide.Blue, 4, 12, 12), new MapUnitFootprintPreset("human-signaller", "Human signaller", "Human", "Communications and EW", "signaller", "signaller", MapSide.Blue, 4, 12, 12), new MapUnitFootprintPreset("drone", "Observation drone", "Neutral", "Reconnaissance", "observation-drone", "observation-drone", MapSide.NeutralSide, 2, 8, 8), new MapUnitFootprintPreset("relay-drone", "Relay drone", "Neutral", "Communications relay", "relay-drone", "relay-drone", MapSide.NeutralSide, 2, 8, 8)]);

export function tryCanonicalFootprintPreset(id) {
    return tryFind((preset) => (compare(preset.Id, id, 4) === 0), canonicalFootprintPresets);
}

export function searchCanonicalUnitPresets(query) {
    const needle = isNullOrWhiteSpace(query) ? "" : query.trim().toLowerCase();
    return sortBy((preset_1) => [preset_1.Faction, preset_1.Role, preset_1.Name, preset_1.Id], filter((preset) => {
        if (needle === "") {
            return true;
        }
        else {
            return exists((value) => (value.toLowerCase().indexOf(needle) >= 0), ofArray([preset.Name, preset.Faction, preset.Role, preset.ClassId, preset.GlyphId]));
        }
    }, canonicalFootprintPresets), {
        Compare: (x, y) => (compareArrays(x, y) | 0),
    });
}

function canonicalFootprintSize(id) {
    let option_3;
    const option_1 = tryCanonicalFootprintPreset(id);
    option_3 = ((option_1 != null) ? option_1.FootprintSize : undefined);
    if (option_3 != null) {
        return option_3 | 0;
    }
    else {
        throw new Exception(("Unknown canonical footprint preset: " + id) + " (Parameter \'id\')");
    }
}

function extent(value) {
    const option_1 = CellExtentModule_tryCreate(value);
    if (option_1 != null) {
        return option_1;
    }
    else {
        throw new Exception("Extent must be positive. (Parameter \'value\')");
    }
}

function health(remaining, maximum) {
    const option_1 = HealthVisualModule_tryCreate(remaining, maximum);
    if (option_1 != null) {
        return option_1;
    }
    else {
        throw new Exception("Health is out of bounds. (Parameter \'remaining\')");
    }
}

function terrainName(terrain) {
    switch (terrain.tag) {
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

function terrainFromName(value) {
    switch (value) {
        case "open":
            return MapTerrain.Open;
        case "rough":
            return MapTerrain.Rough;
        case "blocked":
            return MapTerrain.Blocked;
        case "objective":
            return MapTerrain.Objective;
        default:
            return undefined;
    }
}

function sideName(side) {
    switch (side.tag) {
        case 1:
            return "red";
        case 2:
            return "neutral";
        default:
            return "blue";
    }
}

function sideFromName(value) {
    switch (value) {
        case "blue":
            return MapSide.Blue;
        case "red":
            return MapSide.Red;
        case "neutral":
            return MapSide.NeutralSide;
        default:
            return undefined;
    }
}

function controllerName(controller) {
    switch (controller.tag) {
        case 1:
            return "scripted";
        case 2:
            return "general";
        default:
            return "manual";
    }
}

function controllerFromName(value) {
    switch (value) {
        case "manual":
            return MapController.Manual;
        case "scripted":
            return MapController.Scripted;
        case "general":
            return MapController.General;
        default:
            return undefined;
    }
}

function edgeDirectionName(direction) {
    if (direction.tag === 1) {
        return "south";
    }
    else {
        return "east";
    }
}

function edgeKindName(kind) {
    switch (kind.tag) {
        case 1:
            return "door";
        case 2:
            return "window";
        default:
            return "wall";
    }
}

export function regionPurposeLabel(purpose) {
    if (purpose.tag === 1) {
        switch (purpose.fields[0].tag) {
            case 1:
                return "Red deployment";
            case 2:
                return "Neutral deployment";
            default:
                return "Blue deployment";
        }
    }
    else {
        return "Objective";
    }
}

function regionPurposeFields(purpose) {
    if (purpose.tag === 1) {
        return ofArray(["deployment", sideName(purpose.fields[0])]);
    }
    else {
        return singleton("objective");
    }
}

function regionGeometryFields(geometry) {
    if (geometry.tag === 1) {
        return cons("polygon", ofArray(map_8((vertex) => ((int32ToString(vertex.CellColumn) + ",") + int32ToString(vertex.CellRow)), geometry.fields[0])));
    }
    else {
        return ofArray(["rectangle", int32ToString(geometry.fields[0]), int32ToString(geometry.fields[1]), int32ToString(geometry.fields[2]), int32ToString(geometry.fields[3])]);
    }
}

/**
 * Normalizes one unit grid segment to its single authoritative east/south
 * record. North and west border segments have no owning cell and are
 * rejected instead of being silently shifted into the document.
 */
export function tryNormalizeEdge(width, height, x1, y1, x2, y2) {
    if ((x1 === x2) && (Math.abs(y2 - y1) === 1)) {
        const column = (x1 - 1) | 0;
        const row = min(y1, y2) | 0;
        if ((((column >= 0) && (column < width)) && (row >= 0)) && (row < height)) {
            return [column, row, MapEdgeDirection.EastEdge];
        }
        else {
            return undefined;
        }
    }
    else if ((y1 === y2) && (Math.abs(x2 - x1) === 1)) {
        const column_1 = min(x1, x2) | 0;
        const row_1 = (y1 - 1) | 0;
        if ((((column_1 >= 0) && (column_1 < width)) && (row_1 >= 0)) && (row_1 < height)) {
            return [column_1, row_1, MapEdgeDirection.SouthEdge];
        }
        else {
            return undefined;
        }
    }
    else {
        return undefined;
    }
}

function directionCode(direction) {
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

function directionFromCode(value) {
    const matchValue = value.trim().toUpperCase();
    switch (matchValue) {
        case "N":
            return Direction8.North;
        case "NE":
            return Direction8.NorthEast;
        case "E":
            return Direction8.East;
        case "SE":
            return Direction8.SouthEast;
        case "S":
            return Direction8.South;
        case "SW":
            return Direction8.SouthWest;
        case "W":
            return Direction8.West;
        case "NW":
            return Direction8.NorthWest;
        default:
            return undefined;
    }
}

export function parseScript(value) {
    if (isNullOrWhiteSpace(value)) {
        return new FSharpResult$2(/* Ok */ 0, [empty()]);
    }
    else {
        const parsed = map_9(directionFromCode, ofArray(split(value, [","], undefined, 1)));
        if (exists((option) => (option == null), parsed)) {
            return new FSharpResult$2(/* Error */ 1, ["Use comma-separated directions: N, NE, E, SE, S, SW, W, NW."]);
        }
        else {
            return new FSharpResult$2(/* Ok */ 0, [choose((x) => x, parsed)]);
        }
    }
}

export function scriptText(script) {
    return join(",", map_9(directionCode, script));
}

function canonicalMapText(map) {
    return join("\n", toList(delay(() => append(singleton_1("SIR-MAP " + int32ToString(4)), delay(() => append(singleton_1((("size " + int32ToString(map.Width)) + " ") + int32ToString(map.Height)), delay(() => append(map_9((tupledArg) => {
        const _arg = tupledArg[0];
        return (((("terrain " + int32ToString(_arg[0])) + " ") + int32ToString(_arg[1])) + " ") + terrainName(tupledArg[1]);
    }, toList_1(map.Terrain)), delay(() => append(map_9((tupledArg_1) => {
        const _arg_1 = tupledArg_1[0];
        const _arg_2 = tupledArg_1[1];
        return (((((((("edge " + int32ToString(_arg_1[0])) + " ") + int32ToString(_arg_1[1])) + " ") + edgeDirectionName(_arg_1[2])) + " ") + edgeKindName(_arg_2[0])) + " ") + (_arg_2[1] ? "open" : "closed");
    }, toList_1(map.Edges)), delay(() => append(map_9((tupledArg_2) => {
        const region = tupledArg_2[1];
        return join(" ", append_1(ofArray(["zone", int32ToString(region.Id)]), append_1(regionPurposeFields(region.Purpose), regionGeometryFields(region.Geometry))));
    }, toList_1(map.Regions)), delay(() => map_9((tupledArg_3) => {
        const unit = tupledArg_3[1];
        return (((((((((((((((((((((("unit " + int32ToString(unit.Id)) + " ") + sideName(unit.Side)) + " ") + unit.ClassId) + " ") + int32ToString(unit.Column)) + " ") + int32ToString(unit.Row)) + " ") + int32ToString(unit.Size)) + " ") + int32ToString(unit.Health)) + " ") + int32ToString(unit.HealthMaximum)) + " ") + controllerName(unit.Controller)) + " ") + (isEmpty(unit.Script) ? "-" : scriptText(unit.Script))) + " ") + directionCode(unit.BodyFacing)) + " ") + directionCode(unit.AttentionDirection);
    }, toList_1(map.Units))))))))))))))) + "\n";
}

function hex(bytes) {
    return join("", map_8((value) => format('{0:' + "x2" + '}', value), bytes));
}

export function revisionDigest(map) {
    let chars, objectArg;
    return hex(sha256((chars = canonicalMapText(map), (objectArg = get_UTF8(), objectArg.getBytes(chars)))));
}

function revision(number, parent, map) {
    return create(number, parent, map, revisionDigest(map));
}

function serializedBytes(map) {
    let array;
    const chars = canonicalMapText(map);
    const objectArg = get_UTF8();
    array = objectArg.getBytes(chars);
    return array.length | 0;
}

function emptyMap(width, height) {
    return new MapDefinition(width, height, empty_1({
        Compare: (x, y) => (compareArrays(x, y) | 0),
    }), empty_1({
        Compare: (x_1, y_1) => (compareArrays(x_1, y_1) | 0),
    }), empty_1({
        Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
    }), 1, empty_1({
        Compare: (x_3, y_3) => (comparePrimitives(x_3, y_3) | 0),
    }), 1);
}

export const initial = (() => {
    const units = ofList(map_9((unit) => [unit.Id, unit], ofArray([new EditorUnit(1, MapSide.Blue, "rifleman", 2, 2, canonicalFootprintSize("human"), 12, 12, MapController.Manual, empty(), 0, Direction8.North, Direction8.North), new EditorUnit(2, MapSide.Blue, "medic", 2, 10, canonicalFootprintSize("human"), 12, 12, MapController.Scripted, ofArray([Direction8.East, Direction8.East, Direction8.North]), 0, Direction8.North, Direction8.North), new EditorUnit(3, MapSide.Red, "goblin", 16, 2, canonicalFootprintSize("goblin"), 12, 12, MapController.General, empty(), 0, Direction8.North, Direction8.North), new EditorUnit(4, MapSide.Red, "troll", 18, 10, canonicalFootprintSize("troll"), 12, 12, MapController.General, empty(), 0, Direction8.North, Direction8.North)])), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
    let map;
    const bind$0040 = emptyMap(24, 16);
    map = (new MapDefinition(bind$0040.Width, bind$0040.Height, ofList(toList(delay(() => collect((matchValue) => {
        const row = matchValue[1] | 0;
        const column = matchValue[0] | 0;
        return collect((scaledRow) => map_10((scaledColumn) => [[scaledColumn, scaledRow], matchValue[2]], rangeDouble(column * 2, 1, (column * 2) + 1)), rangeDouble(row * 2, 1, (row * 2) + 1));
    }, [[5, 3, MapTerrain.Objective], [5, 4, MapTerrain.Objective], [4, 3, MapTerrain.Rough], [4, 4, MapTerrain.Rough], [6, 3, MapTerrain.Rough], [6, 4, MapTerrain.Rough]]))), {
        Compare: (x_1, y_1) => (compareArrays(x_1, y_1) | 0),
    }), ofList(toList(delay(() => collect((matchValue_1) => {
        const kind = matchValue_1[1];
        const column_1 = matchValue_1[0] | 0;
        return append(singleton_1([[column_1 * 2, 4, MapEdgeDirection.SouthEdge], [kind, false]]), delay(() => singleton_1([[(column_1 * 2) + 1, 4, MapEdgeDirection.SouthEdge], [kind, false]])));
    }, [[5, MapEdgeKind.Wall], [6, MapEdgeKind.Door], [7, MapEdgeKind.Window$]]))), {
        Compare: (x_2, y_2) => (compareArrays(x_2, y_2) | 0),
    }), units, 5, bind$0040.Regions, bind$0040.NextRegionId));
    const initialRevision = revision(0n, undefined, map);
    return new MapEditorState(map, MapEditorTool.Select, MapTerrain.Rough, 1, new EditorCellAddress(0, 0), new EditorKeyboardCursor(new EditorCellAddress(0, 0), 0), undefined, TerrainAuthoringTool.PencilTool, "Terrain authoring ready.", [0, 0, MapEdgeDirection.EastEdge], "Semantic edge authoring ready.", "", new UnitPaletteCursor_3("goblin", 0, 0), new EditorCellAddress(0, 0), "Unit authoring ready.", "Zone authoring ready.", RegionKeyboardMode_26.RegionIdle, 1, singleton_2(1, {
        Compare: (x_3, y_3) => (comparePrimitives(x_3, y_3) | 0),
    }), undefined, EditorGesture.IdleGesture, initialRevision, RevisionState_9.SavedRevision, initialRevision.Digest, undefined, undefined, empty(), empty(), 0, undefined, 0, false, empty(), undefined, ofList(map_9((domain) => [domain, EditorLayerState.VisibleLayer], ofArray([EditorDomain.TerrainDomain, EditorDomain.EdgeDomain, EditorDomain.UnitDomain, EditorDomain.RegionDomain, EditorDomain.DocumentDomain])), {
        Compare: (x_4, y_4) => (compare_1(x_4, y_4) | 0),
    }), [], undefined, undefined, undefined, new MapAuthoringMetadata("Untitled battlefield", empty_1({
        Compare: (x_5, y_5) => (comparePrimitives(x_5, y_5) | 0),
    }), initialRevision.Digest, undefined));
})();

function directionDelta(direction) {
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

function sign(value) {
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

function directionForDelta(x, y) {
    const matchValue = sign(x) | 0;
    const matchValue_1 = sign(y) | 0;
    let matchResult;
    switch (matchValue) {
        case -1: {
            switch (matchValue_1) {
                case -1: {
                    matchResult = 7;
                    break;
                }
                case 0: {
                    matchResult = 6;
                    break;
                }
                case 1: {
                    matchResult = 5;
                    break;
                }
                default:
                    matchResult = 8;
            }
            break;
        }
        case 0: {
            switch (matchValue_1) {
                case -1: {
                    matchResult = 0;
                    break;
                }
                case 1: {
                    matchResult = 4;
                    break;
                }
                default:
                    matchResult = 8;
            }
            break;
        }
        case 1: {
            switch (matchValue_1) {
                case -1: {
                    matchResult = 1;
                    break;
                }
                case 0: {
                    matchResult = 2;
                    break;
                }
                case 1: {
                    matchResult = 3;
                    break;
                }
                default:
                    matchResult = 8;
            }
            break;
        }
        default:
            matchResult = 8;
    }
    switch (matchResult) {
        case 0:
            return Direction8.North;
        case 1:
            return Direction8.NorthEast;
        case 2:
            return Direction8.East;
        case 3:
            return Direction8.SouthEast;
        case 4:
            return Direction8.South;
        case 5:
            return Direction8.SouthWest;
        case 6:
            return Direction8.West;
        case 7:
            return Direction8.NorthWest;
        default:
            return undefined;
    }
}

function cells(unit, column, row) {
    return toList(delay(() => collect((y) => map_10((x) => [x, y], rangeDouble(column, 1, (column + unit.Size) - 1)), rangeDouble(row, 1, (row + unit.Size) - 1))));
}

function edgeIsBlocking(map, key_, key__1, key__2) {
    return exists_1((tupledArg) => {
        const kind = tupledArg[0];
        switch (kind.tag) {
            case 0:
            case 2:
                return true;
            default:
                return !tupledArg[1];
        }
    }, toArray(tryFind_1([key_, key__1, key__2], map.Edges)));
}

function edgeBlocks(map, unit, dx, dy) {
    return exists((tupledArg) => edgeIsBlocking(map, tupledArg[0], tupledArg[1], tupledArg[2]), append_1((dx > 0) ? toList(delay(() => map_10((row) => [(unit.Column + unit.Size) - 1, row, MapEdgeDirection.EastEdge], rangeDouble(unit.Row, 1, (unit.Row + unit.Size) - 1)))) : ((dx < 0) ? toList(delay(() => map_10((row_1) => [unit.Column - 1, row_1, MapEdgeDirection.EastEdge], rangeDouble(unit.Row, 1, (unit.Row + unit.Size) - 1)))) : empty()), (dy > 0) ? toList(delay(() => map_10((column) => [column, (unit.Row + unit.Size) - 1, MapEdgeDirection.SouthEdge], rangeDouble(unit.Column, 1, (unit.Column + unit.Size) - 1)))) : ((dy < 0) ? toList(delay(() => map_10((column_1) => [column_1, unit.Row - 1, MapEdgeDirection.SouthEdge], rangeDouble(unit.Column, 1, (unit.Column + unit.Size) - 1)))) : empty())));
}

function validPlacement(map, excludedUnit, unit, column, row) {
    const targetCells = cells(unit, column, row);
    const inBounds_1 = forAll((tupledArg) => {
        const x = tupledArg[0] | 0;
        const y = tupledArg[1] | 0;
        if ((((x >= 0) && (y >= 0)) && (x < map.Width)) && (y < map.Height)) {
            return !equals(tryFind_1([x, y], map.Terrain), MapTerrain.Blocked);
        }
        else {
            return false;
        }
    }, targetCells);
    const occupied = ofList_1(collect_1((tupledArg_2) => {
        const other = tupledArg_2[1];
        return cells(other, other.Column, other.Row);
    }, filter((tupledArg_1) => !equals(tupledArg_1[0], excludedUnit), toList_1(map.Units))), {
        Compare: (x_1, y_1) => (compareArrays(x_1, y_1) | 0),
    });
    if (inBounds_1) {
        return forAll((cell) => !contains(cell, occupied), targetCells);
    }
    else {
        return false;
    }
}

function moveUnit(direction, unit, map) {
    const patternInput = directionDelta(direction);
    const dy = patternInput[1] | 0;
    const dx = patternInput[0] | 0;
    const column = (unit.Column + dx) | 0;
    const row = (unit.Row + dy) | 0;
    if (!edgeBlocks(map, unit, dx, dy) && validPlacement(map, unit.Id, unit, column, row)) {
        return [new EditorUnit(unit.Id, unit.Side, unit.ClassId, column, row, unit.Size, unit.Health, unit.HealthMaximum, unit.Controller, unit.Script, unit.ScriptIndex, unit.BodyFacing, unit.AttentionDirection), true];
    }
    else {
        return [unit, false];
    }
}

function setUnit(unit, map) {
    return new MapDefinition(map.Width, map.Height, map.Terrain, map.Edges, add(unit.Id, unit, map.Units), map.NextUnitId, map.Regions, map.NextRegionId);
}

function selectedUnit(state) {
    const option_1 = state.SelectedUnit;
    if (option_1 != null) {
        return tryFind_1(option_1, state.Map.Units);
    }
    else {
        return undefined;
    }
}

function unitAtCell(column, row, map) {
    return tryFind((unit) => contains_1([column, row], cells(unit, unit.Column, unit.Row), {
        Equals: equalArrays,
        GetHashCode: (x) => (arrayHash(x) | 0),
    }), map_9((tuple) => tuple[1], toList_1(map.Units)));
}

function placeUnit(side, classId, size, column, row, state) {
    let bind$0040;
    const unit = new EditorUnit(state.Map.NextUnitId, side, classId, column, row, size, 12, 12, MapController.Manual, empty(), 0, Direction8.North, Direction8.North);
    if (validPlacement(state.Map, undefined, unit, column, row)) {
        return new MapEditorState((bind$0040 = state.Map, new MapDefinition(bind$0040.Width, bind$0040.Height, bind$0040.Terrain, bind$0040.Edges, add(unit.Id, unit, state.Map.Units), unit.Id + 1, bind$0040.Regions, bind$0040.NextRegionId)), state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, unit.Id, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
    }
    else {
        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "The unit does not fit on those cells.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
    }
}

function nearestHostile(unit, map) {
    let other_3;
    const axisGap = (start, size, otherStart, otherSize) => (max(0, (max(start, otherStart) - min((start + size) - 1, (otherStart + otherSize) - 1)) - 1) | 0);
    const distance = (other) => (max(axisGap(unit.Column, unit.Size, other.Column, other.Size), axisGap(unit.Row, unit.Size, other.Row, other.Size)) | 0);
    const option_1 = tryHead(sortBy((other_2) => [distance(other_2), other_2.Id], filter((other_1) => {
        if (other_1.Id !== unit.Id) {
            return !equals(other_1.Side, unit.Side);
        }
        else {
            return false;
        }
    }, map_9((tuple) => tuple[1], toList_1(map.Units))), {
        Compare: (x, y) => (compareArrays(x, y) | 0),
    }));
    if (option_1 != null) {
        return (other_3 = option_1, [other_3, distance(other_3)]);
    }
    else {
        return undefined;
    }
}

function actGeneral(unit, map) {
    const matchValue = nearestHostile(unit, map);
    if (matchValue != null) {
        const target = matchValue[0];
        const dx = (target.Column - unit.Column) | 0;
        const dy = (target.Row - unit.Row) | 0;
        if (matchValue[1] === 0) {
            return [setUnit(new EditorUnit(target.Id, target.Side, target.ClassId, target.Column, target.Row, target.Size, max(0, target.Health - 1), target.HealthMaximum, target.Controller, target.Script, target.ScriptIndex, target.BodyFacing, target.AttentionDirection), map), ((("Unit " + int32ToString(unit.Id)) + " attacks unit ") + int32ToString(target.Id)) + " for 1 damage."];
        }
        else {
            const matchValue_1 = directionForDelta(dx, dy);
            if (matchValue_1 != null) {
                const direction = matchValue_1;
                const patternInput = moveUnit(direction, unit, map);
                if (patternInput[1]) {
                    return [setUnit(patternInput[0], map), ((("Unit " + int32ToString(unit.Id)) + " advances ") + directionCode(direction)) + "."];
                }
                else {
                    return [map, ("Unit " + int32ToString(unit.Id)) + " cannot advance."];
                }
            }
            else {
                return [map, ("Unit " + int32ToString(unit.Id)) + " holds."];
            }
        }
    }
    else {
        return [map, ("Unit " + int32ToString(unit.Id)) + " holds; no hostile is present."];
    }
}

function actScripted(unit, map) {
    const matchValue = unit.Script;
    if (isEmpty(matchValue)) {
        return [map, ("Unit " + int32ToString(unit.Id)) + " has no script."];
    }
    else {
        const script = matchValue;
        const direction = item_1(unit.ScriptIndex % length_1(script), script);
        const patternInput = moveUnit(direction, unit, map);
        const moved = patternInput[0];
        return [setUnit(new EditorUnit(moved.Id, moved.Side, moved.ClassId, moved.Column, moved.Row, moved.Size, moved.Health, moved.HealthMaximum, moved.Controller, moved.Script, unit.ScriptIndex + 1, moved.BodyFacing, moved.AttentionDirection), map), ((("Unit " + int32ToString(unit.Id)) + (patternInput[1] ? " follows " : " is blocked moving ")) + directionCode(direction)) + "."];
    }
}

export function step(state) {
    let map = state.Map;
    let events = empty();
    const enumerator = getEnumerator(sort(map_9((tuple) => (tuple[0] | 0), toList_1(state.Map.Units)), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }));
    try {
        while (enumerator["System.Collections.IEnumerator.MoveNext"]()) {
            const id = enumerator["System.Collections.Generic.IEnumerator`1.get_Current"]() | 0;
            const matchValue = tryFind_1(id, map.Units);
            let matchResult, unit_1;
            if (matchValue != null) {
                if (matchValue.Health > 0) {
                    matchResult = 0;
                    unit_1 = matchValue;
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
                    let patternInput;
                    const matchValue_1 = unit_1.Controller;
                    patternInput = ((matchValue_1.tag === 1) ? actScripted(unit_1, map) : ((matchValue_1.tag === 2) ? actGeneral(unit_1, map) : [map, ("Unit " + int32ToString(id)) + " awaits manual input."]));
                    map = patternInput[0];
                    events = cons(patternInput[1], events);
                    break;
                }
            }
        }
    }
    finally {
        disposeSafe(enumerator);
    }
    const Tick = (state.Tick + 1) | 0;
    const LastEvents = reverse(events);
    return new MapEditorState(map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, Tick, state.IsRunning, LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
}

export function resizeLossPreview(width, height, map) {
    const width_1 = max(4, min(40, width)) | 0;
    const height_1 = max(4, min(40, height)) | 0;
    return new ResizeLossPreview(width_1, height_1, length_1(filter((tupledArg) => {
        const _arg = tupledArg[0];
        if (_arg[0] >= width_1) {
            return true;
        }
        else {
            return _arg[1] >= height_1;
        }
    }, toList_1(map.Terrain))), length_1(filter((tupledArg_1) => {
        const _arg_2 = tupledArg_1[0];
        if (_arg_2[0] >= width_1) {
            return true;
        }
        else {
            return _arg_2[1] >= height_1;
        }
    }, toList_1(map.Edges))), length_1(filter((tupledArg_2) => {
        const unit = tupledArg_2[1];
        if ((unit.Column + unit.Size) > width_1) {
            return true;
        }
        else {
            return (unit.Row + unit.Size) > height_1;
        }
    }, toList_1(map.Units))), length_1(filter((tupledArg_3) => {
        let matchValue, row_2, regionWidth, regionHeight, column_2;
        return exists((tupledArg_4) => {
            const column_3 = tupledArg_4[0] | 0;
            const row_3 = tupledArg_4[1] | 0;
            if (((column_3 < 0) ? true : (row_3 < 0)) ? true : (column_3 > width_1)) {
                return true;
            }
            else {
                return row_3 > height_1;
            }
        }, (matchValue = tupledArg_3[1].Geometry, (matchValue.tag === 1) ? ofArray(map_8((point) => [point.CellColumn, point.CellRow], matchValue.fields[0])) : ((row_2 = (matchValue.fields[1] | 0), (regionWidth = (matchValue.fields[2] | 0), (regionHeight = (matchValue.fields[3] | 0), (column_2 = (matchValue.fields[0] | 0), ofArray([[column_2, row_2], [column_2 + regionWidth, row_2], [column_2 + regionWidth, row_2 + regionHeight], [column_2, row_2 + regionHeight]]))))))));
    }, toList_1(map.Regions))));
}

function resizedDocument(preview, map) {
    return new MapDefinition(preview.TargetWidth, preview.TargetHeight, filter_1((tupledArg, _arg) => {
        if (tupledArg[0] < preview.TargetWidth) {
            return tupledArg[1] < preview.TargetHeight;
        }
        else {
            return false;
        }
    }, map.Terrain), filter_1((tupledArg_1, _arg_2) => {
        if (tupledArg_1[0] < preview.TargetWidth) {
            return tupledArg_1[1] < preview.TargetHeight;
        }
        else {
            return false;
        }
    }, map.Edges), filter_1((_arg_3, unit) => {
        if ((unit.Column + unit.Size) <= preview.TargetWidth) {
            return (unit.Row + unit.Size) <= preview.TargetHeight;
        }
        else {
            return false;
        }
    }, map.Units), map.NextUnitId, filter_1((_arg_4, region) => {
        let matchValue, width, row_2, height, column_2;
        return forAll((tupledArg_2) => {
            const column_3 = tupledArg_2[0] | 0;
            const row_3 = tupledArg_2[1] | 0;
            if (((column_3 >= 0) && (row_3 >= 0)) && (column_3 <= preview.TargetWidth)) {
                return row_3 <= preview.TargetHeight;
            }
            else {
                return false;
            }
        }, (matchValue = region.Geometry, (matchValue.tag === 1) ? ofArray(map_8((point) => [point.CellColumn, point.CellRow], matchValue.fields[0])) : ((width = (matchValue.fields[2] | 0), (row_2 = (matchValue.fields[1] | 0), (height = (matchValue.fields[3] | 0), (column_2 = (matchValue.fields[0] | 0), ofArray([[column_2, row_2], [column_2 + width, row_2], [column_2 + width, row_2 + height], [column_2, row_2 + height]]))))))));
    }, map.Regions), map.NextRegionId);
}

function resize(width, height, state) {
    const preview = resizeLossPreview(width, height, state.Map);
    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, ((((preview.LostTerrainCells + preview.LostEdges) + preview.LostUnits) + preview.LostRegions) === 0) ? undefined : (((((((("Resize would remove " + int32ToString(preview.LostTerrainCells)) + " terrain cells, ") + int32ToString(preview.LostEdges)) + " edges, and ") + int32ToString(preview.LostUnits)) + " units, and ") + int32ToString(preview.LostRegions)) + " regions. Confirm to continue."), state.Layers, state.Issues, state.ActiveIssue, new PendingDestructiveChange_4(/* ResizePending */ 0, [preview]), state.PendingRecovery, state.Authoring);
}

function inBounds(map, address) {
    if (((address.CellColumn >= 0) && (address.CellRow >= 0)) && (address.CellColumn < map.Width)) {
        return address.CellRow < map.Height;
    }
    else {
        return false;
    }
}

function normalizeAddresses(addresses) {
    return toArray_1(sortBy_1((address) => [address.CellRow, address.CellColumn], distinct(addresses, {
        Equals: equals,
        GetHashCode: (x) => (safeHash(x) | 0),
    }), {
        Compare: (x_1, y_1) => (compareArrays(x_1, y_1) | 0),
    }));
}

function lineAddresses(first, last) {
    let x = first.CellColumn;
    let y = first.CellRow;
    const dx = Math.abs(last.CellColumn - first.CellColumn) | 0;
    const sx = ((first.CellColumn < last.CellColumn) ? 1 : -1) | 0;
    const dy = op_UnaryNegation_Int32(Math.abs(last.CellRow - first.CellRow)) | 0;
    const sy = ((first.CellRow < last.CellRow) ? 1 : -1) | 0;
    let error = dx + dy;
    const addresses = [];
    let finished = false;
    while (!finished) {
        void (addresses.push(new EditorCellAddress(x, y)));
        if ((x === last.CellColumn) && (y === last.CellRow)) {
            finished = true;
        }
        else {
            const doubled = (2 * error) | 0;
            if (doubled >= dy) {
                error = ((error + dy) | 0);
                x = ((x + sx) | 0);
            }
            if (doubled <= dx) {
                error = ((error + dx) | 0);
                y = ((y + sy) | 0);
            }
        }
    }
    return addresses.slice();
}

function brushAddresses(map, brushSize, centers) {
    const brushSize_1 = max(1, brushSize) | 0;
    const leading = ~~((brushSize_1 - 1) / 2) | 0;
    const trailing = ((brushSize_1 - leading) - 1) | 0;
    return normalizeAddresses(filter_2((address) => inBounds(map, address), collect((center) => delay(() => collect((row) => map_10((column) => (new EditorCellAddress(column, row)), rangeDouble(center.CellColumn - leading, 1, center.CellColumn + trailing)), rangeDouble(center.CellRow - leading, 1, center.CellRow + trailing))), centers)));
}

function rectangleAddresses(map, brushSize, first, last) {
    const minimumColumn = min(first.CellColumn, last.CellColumn) | 0;
    const maximumColumn = max(first.CellColumn, last.CellColumn) | 0;
    const minimumRow = min(first.CellRow, last.CellRow) | 0;
    const maximumRow = max(first.CellRow, last.CellRow) | 0;
    return brushAddresses(map, brushSize, delay(() => collect((row) => map_10((column) => (new EditorCellAddress(column, row)), rangeDouble(minimumColumn, 1, maximumColumn)), rangeDouble(minimumRow, 1, maximumRow))));
}

function floodAddresses(map, start) {
    if (!inBounds(map, start)) {
        return [];
    }
    else {
        const source = defaultArg(tryFind_1([start.CellColumn, start.CellRow], map.Terrain), MapTerrain.Open);
        const pending = Queue$1_$ctor();
        let visited = empty_2({
            Compare: (x, y) => (compareArrays(x, y) | 0),
        });
        Queue$1__Enqueue_2B595(pending, start);
        while (Queue$1__get_Count(pending) > 0) {
            const address = Queue$1__Dequeue(pending);
            const key = [address.CellColumn, address.CellRow];
            const terrain = defaultArg(tryFind_1(key, map.Terrain), MapTerrain.Open);
            if (!contains(key, visited) && equals(terrain, source)) {
                visited = add_1(key, visited);
                iterate((item) => {
                    Queue$1__Enqueue_2B595(pending, item);
                }, filter((address_1) => inBounds(map, address_1), ofArray([new EditorCellAddress(address.CellColumn - 1, address.CellRow), new EditorCellAddress(address.CellColumn + 1, address.CellRow), new EditorCellAddress(address.CellColumn, address.CellRow - 1), new EditorCellAddress(address.CellColumn, address.CellRow + 1)])));
            }
        }
        return normalizeAddresses(map_10((tupledArg) => (new EditorCellAddress(tupledArg[0], tupledArg[1])), visited));
    }
}

function terrainGestureAddresses(map, brushSize, tool, anchor, current, visited) {
    switch (tool.tag) {
        case 1:
            return rectangleAddresses(map, brushSize, anchor, current);
        case 2:
            return brushAddresses(map, brushSize, lineAddresses(anchor, current));
        case 3:
            return floodAddresses(map, anchor);
        case 4: {
            const array_2 = [anchor];
            return array_2.filter((address) => inBounds(map, address));
        }
        case 5:
            return brushAddresses(map, brushSize, append_2(lineAddresses(anchor, current), visited));
        default:
            return brushAddresses(map, brushSize, append_2(lineAddresses(anchor, current), visited));
    }
}

function terrainGestureCommand(state, tool, anchor, current, visited) {
    return new EditorCommand(/* PaintCells */ 0, [(tool.tag === 5) ? MapTerrain.Open : state.TerrainSelection, terrainGestureAddresses(state.Map, state.BrushSize, tool, anchor, current, visited)]);
}

function legacyUpdate(action, state) {
    let option_2, bind$0040, bind$0040_1, bind$0040_2, matchValue_1, existing_5, existing_6, existing_4, bind$0040_3;
    switch (action.tag) {
        case 0:
            return new MapEditorState(state.Map, action.fields[0], state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
        case 1:
        case 2:
        case 3:
        case 4:
        case 5:
        case 6:
        case 7:
        case 8:
        case 9:
        case 10:
        case 11:
        case 12:
        case 13:
        case 14:
        case 15:
        case 16:
        case 17:
        case 18:
        case 19:
        case 20:
        case 21:
        case 22:
        case 23:
        case 24:
        case 25:
        case 26:
        case 27:
        case 28:
        case 29:
        case 30:
        case 31:
        case 32:
        case 33:
        case 34:
        case 35:
        case 36:
        case 37:
        case 38:
        case 39:
        case 40:
        case 41:
        case 42:
        case 43:
        case 44:
        case 45:
        case 46:
        case 47:
        case 56:
        case 57:
        case 58:
        case 59:
        case 60:
            return state;
        case 61: {
            const row = action.fields[1] | 0;
            const column = action.fields[0] | 0;
            const matchValue = state.Tool;
            let matchResult, terrain, classId, side, size, direction, kind;
            switch (matchValue.tag) {
                case 1: {
                    switch (matchValue.fields[0].tag) {
                        case 0: {
                            matchResult = 1;
                            break;
                        }
                        case 2: {
                            if (unitAtCell(column, row, state.Map) != null) {
                                matchResult = 2;
                            }
                            else {
                                matchResult = 3;
                                terrain = matchValue.fields[0];
                            }
                            break;
                        }
                        default: {
                            matchResult = 3;
                            terrain = matchValue.fields[0];
                        }
                    }
                    break;
                }
                case 2: {
                    matchResult = 4;
                    break;
                }
                case 3: {
                    matchResult = 5;
                    break;
                }
                case 4: {
                    matchResult = 6;
                    classId = matchValue.fields[1];
                    side = matchValue.fields[0];
                    size = matchValue.fields[2];
                    break;
                }
                case 5: {
                    matchResult = 7;
                    direction = matchValue.fields[0];
                    kind = matchValue.fields[1];
                    break;
                }
                default:
                    matchResult = 0;
            }
            switch (matchResult) {
                case 0:
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, (option_2 = tryFind((unit) => contains_1([column, row], cells(unit, unit.Column, unit.Row), {
                        Equals: equalArrays,
                        GetHashCode: (x) => (arrayHash(x) | 0),
                    }), map_9((tuple) => tuple[1], toList_1(state.Map.Units))), (option_2 != null) ? option_2.Id : undefined), state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                case 1:
                    return new MapEditorState((bind$0040 = state.Map, new MapDefinition(bind$0040.Width, bind$0040.Height, remove([column, row], state.Map.Terrain), bind$0040.Edges, bind$0040.Units, bind$0040.NextUnitId, bind$0040.Regions, bind$0040.NextRegionId)), state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                case 2:
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Remove the unit before blocking this cell.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                case 3:
                    return new MapEditorState((bind$0040_1 = state.Map, new MapDefinition(bind$0040_1.Width, bind$0040_1.Height, add([column, row], terrain, state.Map.Terrain), bind$0040_1.Edges, bind$0040_1.Units, bind$0040_1.NextUnitId, bind$0040_1.Regions, bind$0040_1.NextRegionId)), state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                case 4:
                    return state;
                case 5:
                    return state;
                case 6:
                    return placeUnit(side, classId, size, column, row, state);
                default: {
                    const key = [column, row, direction];
                    return new MapEditorState((bind$0040_2 = state.Map, new MapDefinition(bind$0040_2.Width, bind$0040_2.Height, bind$0040_2.Terrain, (matchValue_1 = tryFind_1(key, state.Map.Edges), (matchValue_1 != null) ? (matchValue_1[1] ? ((equals(matchValue_1[0], kind) && equals(kind, MapEdgeKind.Door)) ? ((existing_5 = matchValue_1[0], remove(key, state.Map.Edges))) : (equals(matchValue_1[0], kind) ? ((existing_6 = matchValue_1[0], remove(key, state.Map.Edges))) : add(key, [kind, false], state.Map.Edges))) : ((equals(matchValue_1[0], kind) && equals(kind, MapEdgeKind.Door)) ? ((existing_4 = matchValue_1[0], add(key, [kind, true], state.Map.Edges))) : (equals(matchValue_1[0], kind) ? ((existing_6 = matchValue_1[0], remove(key, state.Map.Edges))) : add(key, [kind, false], state.Map.Edges)))) : add(key, [kind, false], state.Map.Edges)), bind$0040_2.Units, bind$0040_2.NextUnitId, bind$0040_2.Regions, bind$0040_2.NextRegionId)), state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
        }
        case 62:
            return resize(action.fields[0], action.fields[1], state);
        case 77:
            return new MapEditorState(state.Map, MapEditorTool.Select, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, action.fields[0], state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
        case 78:
        case 79:
        case 80:
        case 81:
        case 82:
        case 83:
        case 84:
        case 85:
        case 86:
        case 87:
        case 88:
        case 89:
        case 90:
        case 91:
        case 92:
        case 93:
        case 94:
            return state;
        case 95: {
            const matchValue_2 = state.SelectedUnit;
            if (matchValue_2 != null) {
                return new MapEditorState((bind$0040_3 = state.Map, new MapDefinition(bind$0040_3.Width, bind$0040_3.Height, bind$0040_3.Terrain, bind$0040_3.Edges, remove(matchValue_2, state.Map.Units), bind$0040_3.NextUnitId, bind$0040_3.Regions, bind$0040_3.NextRegionId)), state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, undefined, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            }
            else {
                return state;
            }
        }
        case 96: {
            const matchValue_3 = selectedUnit(state);
            if (matchValue_3 != null) {
                const unit_1 = matchValue_3;
                return new MapEditorState(setUnit(new EditorUnit(unit_1.Id, action.fields[0], unit_1.ClassId, unit_1.Column, unit_1.Row, unit_1.Size, unit_1.Health, unit_1.HealthMaximum, unit_1.Controller, unit_1.Script, unit_1.ScriptIndex, unit_1.BodyFacing, unit_1.AttentionDirection), state.Map), state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            }
            else {
                return state;
            }
        }
        case 97: {
            const classId_2 = action.fields[0].trim();
            const matchValue_4 = selectedUnit(state);
            if (matchValue_4 != null) {
                if (isNullOrWhiteSpace(classId_2) ? true : exists_1(isWhiteSpace, classId_2.split(""))) {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Class ID must be one non-empty token.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    const unit_3 = matchValue_4;
                    return new MapEditorState(setUnit(new EditorUnit(unit_3.Id, unit_3.Side, classId_2, unit_3.Column, unit_3.Row, unit_3.Size, unit_3.Health, unit_3.HealthMaximum, unit_3.Controller, unit_3.Script, unit_3.ScriptIndex, unit_3.BodyFacing, unit_3.AttentionDirection), state.Map), state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            else {
                return state;
            }
        }
        case 98: {
            const size_1 = action.fields[0] | 0;
            const matchValue_5 = selectedUnit(state);
            if (matchValue_5 != null) {
                const unit_5 = matchValue_5;
                const resized = new EditorUnit(unit_5.Id, unit_5.Side, unit_5.ClassId, unit_5.Column, unit_5.Row, size_1, unit_5.Health, unit_5.HealthMaximum, unit_5.Controller, unit_5.Script, unit_5.ScriptIndex, unit_5.BodyFacing, unit_5.AttentionDirection);
                if ((size_1 > 0) && validPlacement(state.Map, unit_5.Id, resized, unit_5.Column, unit_5.Row)) {
                    return new MapEditorState(setUnit(resized, state.Map), state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "The resized square does not fit.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            else {
                return state;
            }
        }
        case 99: {
            const remaining = action.fields[0] | 0;
            const maximum = action.fields[1] | 0;
            const matchValue_6 = selectedUnit(state);
            if (matchValue_6 != null) {
                if (((maximum > 0) && (remaining >= 0)) && (remaining <= maximum)) {
                    const unit_7 = matchValue_6;
                    return new MapEditorState(setUnit(new EditorUnit(unit_7.Id, unit_7.Side, unit_7.ClassId, unit_7.Column, unit_7.Row, unit_7.Size, remaining, maximum, unit_7.Controller, unit_7.Script, unit_7.ScriptIndex, unit_7.BodyFacing, unit_7.AttentionDirection), state.Map), state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Health must satisfy 0 ≤ current ≤ maximum.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            else {
                return state;
            }
        }
        case 100: {
            const matchValue_7 = selectedUnit(state);
            if (matchValue_7 != null) {
                const unit_9 = matchValue_7;
                return new MapEditorState(setUnit(new EditorUnit(unit_9.Id, unit_9.Side, unit_9.ClassId, unit_9.Column, unit_9.Row, unit_9.Size, unit_9.Health, unit_9.HealthMaximum, action.fields[0], unit_9.Script, unit_9.ScriptIndex, unit_9.BodyFacing, unit_9.AttentionDirection), state.Map), state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            }
            else {
                return state;
            }
        }
        case 101: {
            const matchValue_8 = selectedUnit(state);
            const matchValue_9 = parseScript(action.fields[0]);
            if (matchValue_8 != null) {
                const copyOfStruct = matchValue_9;
                if (copyOfStruct.tag === 1) {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, copyOfStruct.fields[0], state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    const unit_11 = matchValue_8;
                    return new MapEditorState(setUnit(new EditorUnit(unit_11.Id, unit_11.Side, unit_11.ClassId, unit_11.Column, unit_11.Row, unit_11.Size, unit_11.Health, unit_11.HealthMaximum, unit_11.Controller, copyOfStruct.fields[0], 0, unit_11.BodyFacing, unit_11.AttentionDirection), state.Map), state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            else {
                return state;
            }
        }
        case 102: {
            const direction_1 = action.fields[0];
            const matchValue_11 = selectedUnit(state);
            if (matchValue_11 != null) {
                const unit_13 = matchValue_11;
                const patternInput = moveUnit(direction_1, unit_13, state.Map);
                if (patternInput[1]) {
                    return new MapEditorState(setUnit(patternInput[0], state.Map), state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick + 1, state.IsRunning, singleton(((("Unit " + int32ToString(unit_13.Id)) + " moves ") + directionCode(direction_1)) + "."), undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "That move is blocked.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            else {
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Select a unit first.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            }
        }
        case 103:
            return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, !state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
        case 104:
            return step(state);
        case 105:
            return new MapEditorState(emptyMap(state.Map.Width, state.Map.Height), initial.Tool, initial.TerrainSelection, initial.BrushSize, initial.TerrainCursor, initial.KeyboardCursor, initial.KeyboardObject, initial.LastTerrainPaintTool, initial.TerrainAnnouncement, initial.EdgeCursor, initial.EdgeAnnouncement, initial.UnitPaletteSearch, initial.UnitPaletteCursor, initial.UnitPlacementCursor, initial.UnitAnnouncement, initial.RegionAnnouncement, initial.RegionKeyboardMode, undefined, initial.SelectedUnits, initial.SelectedRegion, initial.Gesture, initial.Revision, initial.RevisionState, initial.SavedDigest, initial.SimulatedDigest, initial.RecoveredFromDigest, initial.UndoHistory, initial.RedoHistory, initial.HistoryBytes, initial.Clipboard, initial.Tick, initial.IsRunning, initial.LastEvents, initial.Validation, initial.Layers, initial.Issues, initial.ActiveIssue, initial.PendingDestructiveChange, initial.PendingRecovery, initial.Authoring);
        case 106: {
            const matchValue_12 = tryImport(action.fields[0]);
            if (matchValue_12.tag === 1) {
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, matchValue_12.fields[0], state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            }
            else {
                return new MapEditorState(matchValue_12.fields[0], state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, undefined, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, 0, false, empty(), undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            }
        }
        default:
            return state;
    }
}

export function export$(state) {
    return canonicalMapText(state.Map);
}

export function tryImport(text) {
    if (get_UTF8().getBytes(text).length > 2000000) {
        return new FSharpResult$2(/* Error */ 1, [("Map input exceeds the " + int32ToString(2000000)) + "-byte qualification limit."]);
    }
    else {
        return tryImportWithinLimit(text);
    }
}

function tryImportWithinLimit(text) {
    const fail = (line, message) => (new FSharpResult$2(/* Error */ 1, [(("Map line " + toString(line)) + ": ") + message]));
    const parseInt$ = (line_1, value) => {
        let matchValue;
        let outArg = 0;
        matchValue = [tryParse(value, 511, false, 32, new FSharpRef(() => (outArg | 0), (v) => {
            outArg = (v | 0);
        })), outArg];
        if (matchValue[0]) {
            return new FSharpResult$2(/* Ok */ 0, [matchValue[1]]);
        }
        else {
            return fail(line_1, ("invalid integer \'" + value) + "\'.");
        }
    };
    const lines = split(replace(text, "\r", ""), ["\n"], undefined, 1);
    const version = (lines.length === 0) ? undefined : ((item_2(0, lines) === ("SIR-MAP " + int32ToString(1))) ? 1 : ((item_2(0, lines) === ("SIR-MAP " + int32ToString(2))) ? 2 : ((item_2(0, lines) === ("SIR-MAP " + int32ToString(3))) ? 3 : ((item_2(0, lines) === ("SIR-MAP " + int32ToString(4))) ? 4 : undefined))));
    if ((lines.length < 2) ? true : (version == null)) {
        return new FSharpResult$2(/* Error */ 1, ["The file is not a supported SIR-MAP 1, SIR-MAP 2, SIR-MAP 3, or SIR-MAP 4 document."]);
    }
    else {
        let result = new FSharpResult$2(/* Ok */ 0, [emptyMap(12, 8)]);
        for (let index_1 = 1; index_1 <= (lines.length - 1); index_1++) {
            let width_3, height_3, option_1, line_2, vertices_4, id_4, remaining_3, maximum_3, id_7, remaining_6, maximum_6, id_10;
            if (result.tag === 0) {
                const map_1 = result.fields[0];
                const line_3 = (index_1 + 1) | 0;
                const matchValue_6 = ofArray(split(item_2(index_1, lines), [" "], undefined, 1));
                let matchResult, height_2, width_2, column_6, row_6, terrain, column_8, direction, kind, row_8, state, id_3, purposeAndGeometry, attention_1, body_1, classId_2, column_13, controller_2, id_6, maximum_2, remaining_2, row_13, script_2, side_3, size_2, classId_3, column_16, controller_5, id_9, maximum_5, remaining_5, row_16, script_5, side_6, size_5;
                if (!isEmpty(matchValue_6)) {
                    switch (head(matchValue_6)) {
                        case "size": {
                            if (!isEmpty(tail(matchValue_6))) {
                                if (!isEmpty(tail(tail(matchValue_6)))) {
                                    if (isEmpty(tail(tail(tail(matchValue_6))))) {
                                        matchResult = 0;
                                        height_2 = head(tail(tail(matchValue_6)));
                                        width_2 = head(tail(matchValue_6));
                                    }
                                    else {
                                        matchResult = 7;
                                    }
                                }
                                else {
                                    matchResult = 7;
                                }
                            }
                            else {
                                matchResult = 7;
                            }
                            break;
                        }
                        case "terrain": {
                            if (!isEmpty(tail(matchValue_6))) {
                                if (!isEmpty(tail(tail(matchValue_6)))) {
                                    if (!isEmpty(tail(tail(tail(matchValue_6))))) {
                                        if (isEmpty(tail(tail(tail(tail(matchValue_6)))))) {
                                            matchResult = 1;
                                            column_6 = head(tail(matchValue_6));
                                            row_6 = head(tail(tail(matchValue_6)));
                                            terrain = head(tail(tail(tail(matchValue_6))));
                                        }
                                        else {
                                            matchResult = 7;
                                        }
                                    }
                                    else {
                                        matchResult = 7;
                                    }
                                }
                                else {
                                    matchResult = 7;
                                }
                            }
                            else {
                                matchResult = 7;
                            }
                            break;
                        }
                        case "edge": {
                            if (!isEmpty(tail(matchValue_6))) {
                                if (!isEmpty(tail(tail(matchValue_6)))) {
                                    if (!isEmpty(tail(tail(tail(matchValue_6))))) {
                                        if (!isEmpty(tail(tail(tail(tail(matchValue_6)))))) {
                                            if (!isEmpty(tail(tail(tail(tail(tail(matchValue_6))))))) {
                                                if (isEmpty(tail(tail(tail(tail(tail(tail(matchValue_6)))))))) {
                                                    matchResult = 2;
                                                    column_8 = head(tail(matchValue_6));
                                                    direction = head(tail(tail(tail(matchValue_6))));
                                                    kind = head(tail(tail(tail(tail(matchValue_6)))));
                                                    row_8 = head(tail(tail(matchValue_6)));
                                                    state = head(tail(tail(tail(tail(tail(matchValue_6))))));
                                                }
                                                else {
                                                    matchResult = 7;
                                                }
                                            }
                                            else {
                                                matchResult = 7;
                                            }
                                        }
                                        else {
                                            matchResult = 7;
                                        }
                                    }
                                    else {
                                        matchResult = 7;
                                    }
                                }
                                else {
                                    matchResult = 7;
                                }
                            }
                            else {
                                matchResult = 7;
                            }
                            break;
                        }
                        case "zone": {
                            if (equals(version, 1)) {
                                matchResult = 3;
                            }
                            else if (!isEmpty(tail(matchValue_6))) {
                                matchResult = 4;
                                id_3 = head(tail(matchValue_6));
                                purposeAndGeometry = tail(tail(matchValue_6));
                            }
                            else {
                                matchResult = 7;
                            }
                            break;
                        }
                        case "unit": {
                            if (!isEmpty(tail(matchValue_6))) {
                                if (!isEmpty(tail(tail(matchValue_6)))) {
                                    if (!isEmpty(tail(tail(tail(matchValue_6))))) {
                                        if (!isEmpty(tail(tail(tail(tail(matchValue_6)))))) {
                                            if (!isEmpty(tail(tail(tail(tail(tail(matchValue_6))))))) {
                                                if (!isEmpty(tail(tail(tail(tail(tail(tail(matchValue_6)))))))) {
                                                    if (!isEmpty(tail(tail(tail(tail(tail(tail(tail(matchValue_6))))))))) {
                                                        if (!isEmpty(tail(tail(tail(tail(tail(tail(tail(tail(matchValue_6)))))))))) {
                                                            if (!isEmpty(tail(tail(tail(tail(tail(tail(tail(tail(tail(matchValue_6))))))))))) {
                                                                if (!isEmpty(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(matchValue_6)))))))))))) {
                                                                    if (isEmpty(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(matchValue_6))))))))))))) {
                                                                        if (!equals(version, 3) && !equals(version, 4)) {
                                                                            matchResult = 6;
                                                                            classId_3 = head(tail(tail(tail(matchValue_6))));
                                                                            column_16 = head(tail(tail(tail(tail(matchValue_6)))));
                                                                            controller_5 = head(tail(tail(tail(tail(tail(tail(tail(tail(tail(matchValue_6))))))))));
                                                                            id_9 = head(tail(matchValue_6));
                                                                            maximum_5 = head(tail(tail(tail(tail(tail(tail(tail(tail(matchValue_6)))))))));
                                                                            remaining_5 = head(tail(tail(tail(tail(tail(tail(tail(matchValue_6))))))));
                                                                            row_16 = head(tail(tail(tail(tail(tail(matchValue_6))))));
                                                                            script_5 = head(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(matchValue_6)))))))))));
                                                                            side_6 = head(tail(tail(matchValue_6)));
                                                                            size_5 = head(tail(tail(tail(tail(tail(tail(matchValue_6)))))));
                                                                        }
                                                                        else {
                                                                            matchResult = 7;
                                                                        }
                                                                    }
                                                                    else if (!isEmpty(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(matchValue_6)))))))))))))) {
                                                                        if (isEmpty(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(matchValue_6))))))))))))))) {
                                                                            if (equals(version, 3) ? true : equals(version, 4)) {
                                                                                matchResult = 5;
                                                                                attention_1 = head(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(matchValue_6)))))))))))));
                                                                                body_1 = head(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(matchValue_6))))))))))));
                                                                                classId_2 = head(tail(tail(tail(matchValue_6))));
                                                                                column_13 = head(tail(tail(tail(tail(matchValue_6)))));
                                                                                controller_2 = head(tail(tail(tail(tail(tail(tail(tail(tail(tail(matchValue_6))))))))));
                                                                                id_6 = head(tail(matchValue_6));
                                                                                maximum_2 = head(tail(tail(tail(tail(tail(tail(tail(tail(matchValue_6)))))))));
                                                                                remaining_2 = head(tail(tail(tail(tail(tail(tail(tail(matchValue_6))))))));
                                                                                row_13 = head(tail(tail(tail(tail(tail(matchValue_6))))));
                                                                                script_2 = head(tail(tail(tail(tail(tail(tail(tail(tail(tail(tail(matchValue_6)))))))))));
                                                                                side_3 = head(tail(tail(matchValue_6)));
                                                                                size_2 = head(tail(tail(tail(tail(tail(tail(matchValue_6)))))));
                                                                            }
                                                                            else {
                                                                                matchResult = 7;
                                                                            }
                                                                        }
                                                                        else {
                                                                            matchResult = 7;
                                                                        }
                                                                    }
                                                                    else {
                                                                        matchResult = 7;
                                                                    }
                                                                }
                                                                else {
                                                                    matchResult = 7;
                                                                }
                                                            }
                                                            else {
                                                                matchResult = 7;
                                                            }
                                                        }
                                                        else {
                                                            matchResult = 7;
                                                        }
                                                    }
                                                    else {
                                                        matchResult = 7;
                                                    }
                                                }
                                                else {
                                                    matchResult = 7;
                                                }
                                            }
                                            else {
                                                matchResult = 7;
                                            }
                                        }
                                        else {
                                            matchResult = 7;
                                        }
                                    }
                                    else {
                                        matchResult = 7;
                                    }
                                }
                                else {
                                    matchResult = 7;
                                }
                            }
                            else {
                                matchResult = 7;
                            }
                            break;
                        }
                        default:
                            matchResult = 7;
                    }
                }
                else {
                    matchResult = 7;
                }
                switch (matchResult) {
                    case 0: {
                        const matchValue_7 = parseInt$(line_3, width_2);
                        const matchValue_8 = parseInt$(line_3, height_2);
                        let matchResult_1, height_4, width_4, error_1;
                        const copyOfStruct_2 = matchValue_7;
                        if (copyOfStruct_2.tag === 1) {
                            matchResult_1 = 2;
                            error_1 = copyOfStruct_2.fields[0];
                        }
                        else {
                            const copyOfStruct_3 = matchValue_8;
                            if (copyOfStruct_3.tag === 1) {
                                matchResult_1 = 2;
                                error_1 = copyOfStruct_3.fields[0];
                            }
                            else if ((width_3 = (copyOfStruct_2.fields[0] | 0), (height_3 = (copyOfStruct_3.fields[0] | 0), (((width_3 >= 4) && (width_3 <= 80)) && (height_3 >= 4)) && (height_3 <= 80)))) {
                                matchResult_1 = 0;
                                height_4 = copyOfStruct_3.fields[0];
                                width_4 = copyOfStruct_2.fields[0];
                            }
                            else {
                                matchResult_1 = 1;
                            }
                        }
                        switch (matchResult_1) {
                            case 0: {
                                result = (new FSharpResult$2(/* Ok */ 0, [new MapDefinition(width_4, height_4, map_1.Terrain, map_1.Edges, map_1.Units, map_1.NextUnitId, map_1.Regions, map_1.NextRegionId)]));
                                break;
                            }
                            case 1: {
                                result = fail(line_3, ("size must be between 4 and " + int32ToString(80)) + ".");
                                break;
                            }
                            case 2: {
                                result = (new FSharpResult$2(/* Error */ 1, [error_1]));
                                break;
                            }
                        }
                        break;
                    }
                    case 1: {
                        const matchValue_10 = parseInt$(line_3, column_6);
                        const matchValue_11 = parseInt$(line_3, row_6);
                        const matchValue_12 = terrainFromName(terrain);
                        let matchResult_2, column_7, row_7, terrain_1, error_2;
                        const copyOfStruct_4 = matchValue_10;
                        if (copyOfStruct_4.tag === 1) {
                            if (matchValue_12 == null) {
                                matchResult_2 = 1;
                            }
                            else {
                                matchResult_2 = 2;
                                error_2 = copyOfStruct_4.fields[0];
                            }
                        }
                        else {
                            const copyOfStruct_5 = matchValue_11;
                            if (copyOfStruct_5.tag === 1) {
                                if (matchValue_12 == null) {
                                    matchResult_2 = 1;
                                }
                                else {
                                    matchResult_2 = 2;
                                    error_2 = copyOfStruct_5.fields[0];
                                }
                            }
                            else if (matchValue_12 == null) {
                                matchResult_2 = 1;
                            }
                            else {
                                matchResult_2 = 0;
                                column_7 = copyOfStruct_4.fields[0];
                                row_7 = copyOfStruct_5.fields[0];
                                terrain_1 = matchValue_12;
                            }
                        }
                        switch (matchResult_2) {
                            case 0: {
                                result = (new FSharpResult$2(/* Ok */ 0, [new MapDefinition(map_1.Width, map_1.Height, add([column_7, row_7], terrain_1, map_1.Terrain), map_1.Edges, map_1.Units, map_1.NextUnitId, map_1.Regions, map_1.NextRegionId)]));
                                break;
                            }
                            case 1: {
                                result = fail(line_3, "unknown terrain.");
                                break;
                            }
                            case 2: {
                                result = (new FSharpResult$2(/* Error */ 1, [error_2]));
                                break;
                            }
                        }
                        break;
                    }
                    case 2: {
                        const edgeDirection = (direction === "east") ? MapEdgeDirection.EastEdge : ((direction === "south") ? MapEdgeDirection.SouthEdge : undefined);
                        const edgeKind = (kind === "wall") ? MapEdgeKind.Wall : ((kind === "door") ? MapEdgeKind.Door : ((kind === "window") ? MapEdgeKind.Window$ : undefined));
                        const matchValue_14 = parseInt$(line_3, column_8);
                        const matchValue_15 = parseInt$(line_3, row_8);
                        let matchResult_3, column_10, direction_2, kind_2, row_10, error_3;
                        const copyOfStruct_6 = matchValue_14;
                        if (copyOfStruct_6.tag === 1) {
                            matchResult_3 = 1;
                            error_3 = copyOfStruct_6.fields[0];
                        }
                        else {
                            const copyOfStruct_7 = matchValue_15;
                            if (copyOfStruct_7.tag === 1) {
                                matchResult_3 = 1;
                                error_3 = copyOfStruct_7.fields[0];
                            }
                            else if (edgeDirection != null) {
                                if (edgeKind != null) {
                                    if ((state === "open") ? true : (state === "closed")) {
                                        matchResult_3 = 0;
                                        column_10 = copyOfStruct_6.fields[0];
                                        direction_2 = edgeDirection;
                                        kind_2 = edgeKind;
                                        row_10 = copyOfStruct_7.fields[0];
                                    }
                                    else {
                                        matchResult_3 = 2;
                                    }
                                }
                                else {
                                    matchResult_3 = 2;
                                }
                            }
                            else {
                                matchResult_3 = 2;
                            }
                        }
                        switch (matchResult_3) {
                            case 0: {
                                const address = [column_10, row_10, direction_2];
                                if (containsKey(address, map_1.Edges)) {
                                    result = fail(line_3, "duplicate or overlapping canonical edge record.");
                                }
                                else {
                                    result = (new FSharpResult$2(/* Ok */ 0, [new MapDefinition(map_1.Width, map_1.Height, map_1.Terrain, add(address, [kind_2, state === "open"], map_1.Edges), map_1.Units, map_1.NextUnitId, map_1.Regions, map_1.NextRegionId)]));
                                }
                                break;
                            }
                            case 1: {
                                result = (new FSharpResult$2(/* Error */ 1, [error_3]));
                                break;
                            }
                            case 2: {
                                result = fail(line_3, "invalid edge direction, kind, or state.");
                                break;
                            }
                        }
                        break;
                    }
                    case 3: {
                        result = fail(line_3, "SIR-MAP 1 cannot contain zone records.");
                        break;
                    }
                    case 4: {
                        const patternInput = !isEmpty(purposeAndGeometry) ? ((head(purposeAndGeometry) === "objective") ? [1, RegionPurpose.ObjectiveRegion] : ((head(purposeAndGeometry) === "deployment") ? (!isEmpty(tail(purposeAndGeometry)) ? [2, (option_1 = sideFromName(head(tail(purposeAndGeometry))), (option_1 != null) ? (new RegionPurpose(/* DeploymentZone */ 1, [option_1])) : undefined)] : [0, undefined]) : [0, undefined])) : [0, undefined];
                        const purpose = patternInput[1];
                        const geometryFields = skip(min(patternInput[0], length_1(purposeAndGeometry)), purposeAndGeometry);
                        let geometry;
                        let matchResult_4, column_11, height_5, row_11, width_5, vertices_5;
                        if (!isEmpty(geometryFields)) {
                            switch (head(geometryFields)) {
                                case "rectangle": {
                                    if (!isEmpty(tail(geometryFields))) {
                                        if (!isEmpty(tail(tail(geometryFields)))) {
                                            if (!isEmpty(tail(tail(tail(geometryFields))))) {
                                                if (!isEmpty(tail(tail(tail(tail(geometryFields)))))) {
                                                    if (isEmpty(tail(tail(tail(tail(tail(geometryFields))))))) {
                                                        matchResult_4 = 0;
                                                        column_11 = head(tail(geometryFields));
                                                        height_5 = head(tail(tail(tail(tail(geometryFields)))));
                                                        row_11 = head(tail(tail(geometryFields)));
                                                        width_5 = head(tail(tail(tail(geometryFields))));
                                                    }
                                                    else {
                                                        matchResult_4 = 2;
                                                    }
                                                }
                                                else {
                                                    matchResult_4 = 2;
                                                }
                                            }
                                            else {
                                                matchResult_4 = 2;
                                            }
                                        }
                                        else {
                                            matchResult_4 = 2;
                                        }
                                    }
                                    else {
                                        matchResult_4 = 2;
                                    }
                                    break;
                                }
                                case "polygon": {
                                    if ((vertices_4 = tail(geometryFields), (length_1(vertices_4) >= 3) && (length_1(vertices_4) <= 256))) {
                                        matchResult_4 = 1;
                                        vertices_5 = tail(geometryFields);
                                    }
                                    else {
                                        matchResult_4 = 2;
                                    }
                                    break;
                                }
                                default:
                                    matchResult_4 = 2;
                            }
                        }
                        else {
                            matchResult_4 = 2;
                        }
                        switch (matchResult_4) {
                            case 0: {
                                const matchValue_17 = parseInt$(line_3, column_11);
                                const matchValue_18 = parseInt$(line_3, row_11);
                                const matchValue_19 = parseInt$(line_3, width_5);
                                const matchValue_20 = parseInt$(line_3, height_5);
                                let matchResult_5, column_12, height_6, row_12, width_6, error_4;
                                const copyOfStruct_8 = matchValue_17;
                                if (copyOfStruct_8.tag === 1) {
                                    matchResult_5 = 1;
                                    error_4 = copyOfStruct_8.fields[0];
                                }
                                else {
                                    const copyOfStruct_9 = matchValue_18;
                                    if (copyOfStruct_9.tag === 1) {
                                        matchResult_5 = 1;
                                        error_4 = copyOfStruct_9.fields[0];
                                    }
                                    else {
                                        const copyOfStruct_10 = matchValue_19;
                                        if (copyOfStruct_10.tag === 1) {
                                            matchResult_5 = 1;
                                            error_4 = copyOfStruct_10.fields[0];
                                        }
                                        else {
                                            const copyOfStruct_11 = matchValue_20;
                                            if (copyOfStruct_11.tag === 1) {
                                                matchResult_5 = 1;
                                                error_4 = copyOfStruct_11.fields[0];
                                            }
                                            else {
                                                matchResult_5 = 0;
                                                column_12 = copyOfStruct_8.fields[0];
                                                height_6 = copyOfStruct_11.fields[0];
                                                row_12 = copyOfStruct_9.fields[0];
                                                width_6 = copyOfStruct_10.fields[0];
                                            }
                                        }
                                    }
                                }
                                switch (matchResult_5) {
                                    case 0: {
                                        geometry = (new FSharpResult$2(/* Ok */ 0, [new RegionGeometry(/* RegionRectangle */ 0, [column_12, row_12, width_6, height_6])]));
                                        break;
                                    }
                                    default:
                                        geometry = (new FSharpResult$2(/* Error */ 1, [error_4]));
                                }
                                break;
                            }
                            case 1: {
                                geometry = Result_Map((arg_1) => (new RegionGeometry(/* RegionPolygon */ 1, [toArray_2(reverse(arg_1))])), fold((current, point_1) => {
                                    let matchResult_6, points, value_2, error_5;
                                    const copyOfStruct_12 = current;
                                    if (copyOfStruct_12.tag === 1) {
                                        matchResult_6 = 1;
                                        error_5 = copyOfStruct_12.fields[0];
                                    }
                                    else {
                                        const copyOfStruct_13 = point_1;
                                        if (copyOfStruct_13.tag === 1) {
                                            matchResult_6 = 1;
                                            error_5 = copyOfStruct_13.fields[0];
                                        }
                                        else {
                                            matchResult_6 = 0;
                                            points = copyOfStruct_12.fields[0];
                                            value_2 = copyOfStruct_13.fields[0];
                                        }
                                    }
                                    switch (matchResult_6) {
                                        case 0:
                                            return new FSharpResult$2(/* Ok */ 0, [cons(value_2, points)]);
                                        default:
                                            return new FSharpResult$2(/* Error */ 1, [error_5]);
                                    }
                                }, new FSharpResult$2(/* Ok */ 0, [empty()]), map_9((line_2 = (line_3 | 0), (value_1) => {
                                    const matchValue_1 = split(value_1, [","], undefined, 0);
                                    if (!equalsWith((x, y) => (x === y), matchValue_1, defaultOf()) && (matchValue_1.length === 2)) {
                                        const row = item_2(1, matchValue_1);
                                        const matchValue_2 = parseInt$(line_2, item_2(0, matchValue_1));
                                        const matchValue_3 = parseInt$(line_2, row);
                                        let matchResult_7, column_1, row_1, error;
                                        const copyOfStruct = matchValue_2;
                                        if (copyOfStruct.tag === 1) {
                                            matchResult_7 = 1;
                                            error = copyOfStruct.fields[0];
                                        }
                                        else {
                                            const copyOfStruct_1 = matchValue_3;
                                            if (copyOfStruct_1.tag === 1) {
                                                matchResult_7 = 1;
                                                error = copyOfStruct_1.fields[0];
                                            }
                                            else {
                                                matchResult_7 = 0;
                                                column_1 = copyOfStruct.fields[0];
                                                row_1 = copyOfStruct_1.fields[0];
                                            }
                                        }
                                        switch (matchResult_7) {
                                            case 0:
                                                return new FSharpResult$2(/* Ok */ 0, [new EditorCellAddress(column_1, row_1)]);
                                            default:
                                                return new FSharpResult$2(/* Error */ 1, [error]);
                                        }
                                    }
                                    else {
                                        return fail(line_2, ("invalid polygon vertex \'" + value_1) + "\'.");
                                    }
                                }), vertices_5)));
                                break;
                            }
                            default:
                                geometry = fail(line_3, "invalid or unsupported zone geometry.");
                        }
                        const matchValue_23 = parseInt$(line_3, id_3);
                        let matchResult_8, geometry_2, id_5, purpose_2, error_6;
                        const copyOfStruct_14 = matchValue_23;
                        if (copyOfStruct_14.tag === 1) {
                            matchResult_8 = 1;
                            error_6 = copyOfStruct_14.fields[0];
                        }
                        else if (purpose == null) {
                            const copyOfStruct_15 = geometry;
                            if (copyOfStruct_15.tag === 1) {
                                matchResult_8 = 1;
                                error_6 = copyOfStruct_15.fields[0];
                            }
                            else {
                                matchResult_8 = 2;
                            }
                        }
                        else {
                            const copyOfStruct_16 = geometry;
                            if (copyOfStruct_16.tag === 1) {
                                matchResult_8 = 1;
                                error_6 = copyOfStruct_16.fields[0];
                            }
                            else if ((id_4 = (copyOfStruct_14.fields[0] | 0), ((id_4 > 0) && (FSharpMap__get_Count(map_1.Regions) < 1600)) && !containsKey(id_4, map_1.Regions))) {
                                matchResult_8 = 0;
                                geometry_2 = copyOfStruct_16.fields[0];
                                id_5 = copyOfStruct_14.fields[0];
                                purpose_2 = purpose;
                            }
                            else {
                                matchResult_8 = 3;
                            }
                        }
                        switch (matchResult_8) {
                            case 0: {
                                const region_1 = new MapRegion(id_5, geometry_2, purpose_2, RegionBehavior.NoRegionBehavior);
                                result = (new FSharpResult$2(/* Ok */ 0, [new MapDefinition(map_1.Width, map_1.Height, map_1.Terrain, map_1.Edges, map_1.Units, map_1.NextUnitId, add(id_5, region_1, map_1.Regions), max(map_1.NextRegionId, id_5 + 1))]));
                                break;
                            }
                            case 1: {
                                result = (new FSharpResult$2(/* Error */ 1, [error_6]));
                                break;
                            }
                            case 2: {
                                result = fail(line_3, "unknown zone purpose.");
                                break;
                            }
                            case 3: {
                                result = fail(line_3, "invalid or duplicate zone identifier.");
                                break;
                            }
                        }
                        break;
                    }
                    case 5: {
                        const matchValue_25 = parseInt$(line_3, id_6);
                        const matchValue_26 = sideFromName(side_3);
                        const matchValue_27 = parseInt$(line_3, column_13);
                        const matchValue_28 = parseInt$(line_3, row_13);
                        const matchValue_29 = parseInt$(line_3, size_2);
                        const matchValue_30 = parseInt$(line_3, remaining_2);
                        const matchValue_31 = parseInt$(line_3, maximum_2);
                        const matchValue_32 = controllerFromName(controller_2);
                        const matchValue_33 = (script_2 === "-") ? (new FSharpResult$2(/* Ok */ 0, [empty()])) : parseScript(script_2);
                        const matchValue_34 = directionFromCode(body_1);
                        const matchValue_35 = directionFromCode(attention_1);
                        let matchResult_9, attentionDirection_1, bodyFacing_1, column_15, controller_4, id_8, maximum_4, remaining_4, row_15, script_4, side_5, size_4, error_7;
                        const copyOfStruct_17 = matchValue_25;
                        if (copyOfStruct_17.tag === 1) {
                            matchResult_9 = 1;
                            error_7 = copyOfStruct_17.fields[0];
                        }
                        else if (matchValue_26 != null) {
                            const copyOfStruct_18 = matchValue_27;
                            if (copyOfStruct_18.tag === 1) {
                                matchResult_9 = 1;
                                error_7 = copyOfStruct_18.fields[0];
                            }
                            else {
                                const copyOfStruct_19 = matchValue_28;
                                if (copyOfStruct_19.tag === 1) {
                                    matchResult_9 = 1;
                                    error_7 = copyOfStruct_19.fields[0];
                                }
                                else {
                                    const copyOfStruct_20 = matchValue_29;
                                    if (copyOfStruct_20.tag === 1) {
                                        matchResult_9 = 1;
                                        error_7 = copyOfStruct_20.fields[0];
                                    }
                                    else {
                                        const copyOfStruct_21 = matchValue_30;
                                        if (copyOfStruct_21.tag === 1) {
                                            matchResult_9 = 1;
                                            error_7 = copyOfStruct_21.fields[0];
                                        }
                                        else {
                                            const copyOfStruct_22 = matchValue_31;
                                            if (copyOfStruct_22.tag === 1) {
                                                matchResult_9 = 1;
                                                error_7 = copyOfStruct_22.fields[0];
                                            }
                                            else if (matchValue_32 != null) {
                                                const copyOfStruct_23 = matchValue_33;
                                                if (copyOfStruct_23.tag === 1) {
                                                    matchResult_9 = 1;
                                                    error_7 = copyOfStruct_23.fields[0];
                                                }
                                                else if (matchValue_34 != null) {
                                                    if (matchValue_35 != null) {
                                                        if ((remaining_3 = (copyOfStruct_21.fields[0] | 0), (maximum_3 = (copyOfStruct_22.fields[0] | 0), (id_7 = (copyOfStruct_17.fields[0] | 0), ((((((id_7 > 0) && !containsKey(id_7, map_1.Units)) && (classId_2.length <= 128)) && (copyOfStruct_20.fields[0] > 0)) && (maximum_3 > 0)) && (remaining_3 >= 0)) && (remaining_3 <= maximum_3))))) {
                                                            matchResult_9 = 0;
                                                            attentionDirection_1 = matchValue_35;
                                                            bodyFacing_1 = matchValue_34;
                                                            column_15 = copyOfStruct_18.fields[0];
                                                            controller_4 = matchValue_32;
                                                            id_8 = copyOfStruct_17.fields[0];
                                                            maximum_4 = copyOfStruct_22.fields[0];
                                                            remaining_4 = copyOfStruct_21.fields[0];
                                                            row_15 = copyOfStruct_19.fields[0];
                                                            script_4 = copyOfStruct_23.fields[0];
                                                            side_5 = matchValue_26;
                                                            size_4 = copyOfStruct_20.fields[0];
                                                        }
                                                        else {
                                                            matchResult_9 = 2;
                                                        }
                                                    }
                                                    else {
                                                        matchResult_9 = 2;
                                                    }
                                                }
                                                else {
                                                    matchResult_9 = 2;
                                                }
                                            }
                                            else {
                                                const copyOfStruct_24 = matchValue_33;
                                                if (copyOfStruct_24.tag === 1) {
                                                    matchResult_9 = 1;
                                                    error_7 = copyOfStruct_24.fields[0];
                                                }
                                                else {
                                                    matchResult_9 = 2;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else {
                            const copyOfStruct_25 = matchValue_27;
                            if (copyOfStruct_25.tag === 1) {
                                matchResult_9 = 1;
                                error_7 = copyOfStruct_25.fields[0];
                            }
                            else {
                                const copyOfStruct_26 = matchValue_28;
                                if (copyOfStruct_26.tag === 1) {
                                    matchResult_9 = 1;
                                    error_7 = copyOfStruct_26.fields[0];
                                }
                                else {
                                    const copyOfStruct_27 = matchValue_29;
                                    if (copyOfStruct_27.tag === 1) {
                                        matchResult_9 = 1;
                                        error_7 = copyOfStruct_27.fields[0];
                                    }
                                    else {
                                        const copyOfStruct_28 = matchValue_30;
                                        if (copyOfStruct_28.tag === 1) {
                                            matchResult_9 = 1;
                                            error_7 = copyOfStruct_28.fields[0];
                                        }
                                        else {
                                            const copyOfStruct_29 = matchValue_31;
                                            if (copyOfStruct_29.tag === 1) {
                                                matchResult_9 = 1;
                                                error_7 = copyOfStruct_29.fields[0];
                                            }
                                            else {
                                                const copyOfStruct_30 = matchValue_33;
                                                if (copyOfStruct_30.tag === 1) {
                                                    matchResult_9 = 1;
                                                    error_7 = copyOfStruct_30.fields[0];
                                                }
                                                else {
                                                    matchResult_9 = 2;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        switch (matchResult_9) {
                            case 0: {
                                const unit = new EditorUnit(id_8, side_5, classId_2, column_15, row_15, size_4, remaining_4, maximum_4, controller_4, script_4, 0, bodyFacing_1, attentionDirection_1);
                                result = (new FSharpResult$2(/* Ok */ 0, [new MapDefinition(map_1.Width, map_1.Height, map_1.Terrain, map_1.Edges, add(id_8, unit, map_1.Units), max(map_1.NextUnitId, id_8 + 1), map_1.Regions, map_1.NextRegionId)]));
                                break;
                            }
                            case 1: {
                                result = (new FSharpResult$2(/* Error */ 1, [error_7]));
                                break;
                            }
                            case 2: {
                                result = fail(line_3, "invalid unit orientation.");
                                break;
                            }
                        }
                        break;
                    }
                    case 6: {
                        const matchValue_37 = parseInt$(line_3, id_9);
                        const matchValue_38 = sideFromName(side_6);
                        const matchValue_39 = parseInt$(line_3, column_16);
                        const matchValue_40 = parseInt$(line_3, row_16);
                        const matchValue_41 = parseInt$(line_3, size_5);
                        const matchValue_42 = parseInt$(line_3, remaining_5);
                        const matchValue_43 = parseInt$(line_3, maximum_5);
                        const matchValue_44 = controllerFromName(controller_5);
                        const matchValue_45 = (script_5 === "-") ? (new FSharpResult$2(/* Ok */ 0, [empty()])) : parseScript(script_5);
                        let matchResult_10, column_18, controller_7, id_11, maximum_7, remaining_7, row_18, script_7, side_8, size_7, error_8;
                        const copyOfStruct_31 = matchValue_37;
                        if (copyOfStruct_31.tag === 1) {
                            matchResult_10 = 1;
                            error_8 = copyOfStruct_31.fields[0];
                        }
                        else if (matchValue_38 != null) {
                            const copyOfStruct_32 = matchValue_39;
                            if (copyOfStruct_32.tag === 1) {
                                matchResult_10 = 1;
                                error_8 = copyOfStruct_32.fields[0];
                            }
                            else {
                                const copyOfStruct_33 = matchValue_40;
                                if (copyOfStruct_33.tag === 1) {
                                    matchResult_10 = 1;
                                    error_8 = copyOfStruct_33.fields[0];
                                }
                                else {
                                    const copyOfStruct_34 = matchValue_41;
                                    if (copyOfStruct_34.tag === 1) {
                                        matchResult_10 = 1;
                                        error_8 = copyOfStruct_34.fields[0];
                                    }
                                    else {
                                        const copyOfStruct_35 = matchValue_42;
                                        if (copyOfStruct_35.tag === 1) {
                                            matchResult_10 = 1;
                                            error_8 = copyOfStruct_35.fields[0];
                                        }
                                        else {
                                            const copyOfStruct_36 = matchValue_43;
                                            if (copyOfStruct_36.tag === 1) {
                                                matchResult_10 = 1;
                                                error_8 = copyOfStruct_36.fields[0];
                                            }
                                            else if (matchValue_44 != null) {
                                                const copyOfStruct_37 = matchValue_45;
                                                if (copyOfStruct_37.tag === 1) {
                                                    matchResult_10 = 1;
                                                    error_8 = copyOfStruct_37.fields[0];
                                                }
                                                else if ((remaining_6 = (copyOfStruct_35.fields[0] | 0), (maximum_6 = (copyOfStruct_36.fields[0] | 0), (id_10 = (copyOfStruct_31.fields[0] | 0), ((((((id_10 > 0) && !containsKey(id_10, map_1.Units)) && (classId_3.length <= 128)) && (copyOfStruct_34.fields[0] > 0)) && (maximum_6 > 0)) && (remaining_6 >= 0)) && (remaining_6 <= maximum_6))))) {
                                                    matchResult_10 = 0;
                                                    column_18 = copyOfStruct_32.fields[0];
                                                    controller_7 = matchValue_44;
                                                    id_11 = copyOfStruct_31.fields[0];
                                                    maximum_7 = copyOfStruct_36.fields[0];
                                                    remaining_7 = copyOfStruct_35.fields[0];
                                                    row_18 = copyOfStruct_33.fields[0];
                                                    script_7 = copyOfStruct_37.fields[0];
                                                    side_8 = matchValue_38;
                                                    size_7 = copyOfStruct_34.fields[0];
                                                }
                                                else {
                                                    matchResult_10 = 2;
                                                }
                                            }
                                            else {
                                                const copyOfStruct_38 = matchValue_45;
                                                if (copyOfStruct_38.tag === 1) {
                                                    matchResult_10 = 1;
                                                    error_8 = copyOfStruct_38.fields[0];
                                                }
                                                else {
                                                    matchResult_10 = 2;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else {
                            const copyOfStruct_39 = matchValue_39;
                            if (copyOfStruct_39.tag === 1) {
                                matchResult_10 = 1;
                                error_8 = copyOfStruct_39.fields[0];
                            }
                            else {
                                const copyOfStruct_40 = matchValue_40;
                                if (copyOfStruct_40.tag === 1) {
                                    matchResult_10 = 1;
                                    error_8 = copyOfStruct_40.fields[0];
                                }
                                else {
                                    const copyOfStruct_41 = matchValue_41;
                                    if (copyOfStruct_41.tag === 1) {
                                        matchResult_10 = 1;
                                        error_8 = copyOfStruct_41.fields[0];
                                    }
                                    else {
                                        const copyOfStruct_42 = matchValue_42;
                                        if (copyOfStruct_42.tag === 1) {
                                            matchResult_10 = 1;
                                            error_8 = copyOfStruct_42.fields[0];
                                        }
                                        else {
                                            const copyOfStruct_43 = matchValue_43;
                                            if (copyOfStruct_43.tag === 1) {
                                                matchResult_10 = 1;
                                                error_8 = copyOfStruct_43.fields[0];
                                            }
                                            else {
                                                const copyOfStruct_44 = matchValue_45;
                                                if (copyOfStruct_44.tag === 1) {
                                                    matchResult_10 = 1;
                                                    error_8 = copyOfStruct_44.fields[0];
                                                }
                                                else {
                                                    matchResult_10 = 2;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        switch (matchResult_10) {
                            case 0: {
                                const unit_1 = new EditorUnit(id_11, side_8, classId_3, column_18, row_18, size_7, remaining_7, maximum_7, controller_7, script_7, 0, Direction8.North, Direction8.North);
                                result = (new FSharpResult$2(/* Ok */ 0, [new MapDefinition(map_1.Width, map_1.Height, map_1.Terrain, map_1.Edges, add(id_11, unit_1, map_1.Units), max(map_1.NextUnitId, id_11 + 1), map_1.Regions, map_1.NextRegionId)]));
                                break;
                            }
                            case 1: {
                                result = (new FSharpResult$2(/* Error */ 1, [error_8]));
                                break;
                            }
                            case 2: {
                                result = fail(line_3, "invalid unit.");
                                break;
                            }
                        }
                        break;
                    }
                    case 7: {
                        result = fail(line_3, "unknown record.");
                        break;
                    }
                }
            }
        }
        return Result_Bind((map_3) => {
            const invalidTerrain = tryFind((tupledArg_3) => {
                const _arg_4 = tupledArg_3[0];
                const row_22 = _arg_4[1] | 0;
                const column_22 = _arg_4[0] | 0;
                if (((column_22 < 0) ? true : (row_22 < 0)) ? true : (column_22 >= map_3.Width)) {
                    return true;
                }
                else {
                    return row_22 >= map_3.Height;
                }
            }, toList_1(map_3.Terrain));
            const invalidEdge = tryFind((tupledArg_4) => {
                const _arg_6 = tupledArg_4[0];
                const row_23 = _arg_6[1] | 0;
                const column_23 = _arg_6[0] | 0;
                if (((column_23 < 0) ? true : (row_23 < 0)) ? true : (column_23 >= map_3.Width)) {
                    return true;
                }
                else {
                    return row_23 >= map_3.Height;
                }
            }, toList_1(map_3.Edges));
            const invalidEdgeState = tryFind((tupledArg_5) => {
                const _arg_9 = tupledArg_5[1];
                if (_arg_9[1]) {
                    return !equals(_arg_9[0], MapEdgeKind.Door);
                }
                else {
                    return false;
                }
            }, toList_1(map_3.Edges));
            const occupiedCellCounts = ofSeq(countBy((x_6) => x_6, collect((tupledArg_6) => {
                const unit_3 = tupledArg_6[1];
                return cells(unit_3, unit_3.Column, unit_3.Row);
            }, toSeq(map_3.Units)), {
                Equals: equalArrays,
                GetHashCode: (x_7) => (arrayHash(x_7) | 0),
            }), {
                Compare: (x_8, y_6) => (compareArrays(x_8, y_6) | 0),
            });
            const invalid = tryFind((unit_4) => exists((tupledArg_7) => {
                const column_24 = tupledArg_7[0] | 0;
                const row_24 = tupledArg_7[1] | 0;
                if (((((column_24 < 0) ? true : (row_24 < 0)) ? true : (column_24 >= map_3.Width)) ? true : (row_24 >= map_3.Height)) ? true : equals(tryFind_1([column_24, row_24], map_3.Terrain), MapTerrain.Blocked)) {
                    return true;
                }
                else {
                    return defaultArg(tryFind_1([column_24, row_24], occupiedCellCounts), 0) > 1;
                }
            }, cells(unit_4, unit_4.Column, unit_4.Row)), map_9((tuple) => tuple[1], toList_1(map_3.Units)));
            if (invalidTerrain == null) {
                if (invalidEdge == null) {
                    if (invalidEdgeState == null) {
                        if (invalid == null) {
                            let matchValue_49;
                            const map = map_3;
                            matchValue_49 = tryPick((tupledArg) => {
                                let vertices_2, array_2, vertices, vertices_1, orientation, onSegment, width, row_2, height, column_2;
                                const id = tupledArg[0] | 0;
                                const region = tupledArg[1];
                                if ((id !== region.Id) ? true : (id <= 0)) {
                                    return "region identity is invalid.";
                                }
                                else if (equals(region.Purpose, new RegionPurpose(/* DeploymentZone */ 1, [MapSide.NeutralSide]))) {
                                    return "deployment zones must belong to blue or red.";
                                }
                                else {
                                    const matchValue_5 = region.Geometry;
                                    let matchResult_11, column_3, height_1, row_3, width_1, vertices_3;
                                    if (matchValue_5.tag === 1) {
                                        if ((vertices_2 = matchValue_5.fields[0], ((((vertices_2.length < 3) ? true : (((array_2 = Array_distinct(vertices_2, {
                                            Equals: equals,
                                            GetHashCode: (x_3) => (safeHash(x_3) | 0),
                                        }), array_2.length)) !== vertices_2.length)) ? true : equals_1((vertices = vertices_2, sum(mapIndexed((index, point) => {
                                            const next = item_2((index + 1) % vertices.length, vertices);
                                            return toInt64_unchecked(op_Subtraction(toInt64_unchecked(op_Multiply(toInt64_unchecked(fromInt32(point.CellColumn)), toInt64_unchecked(fromInt32(next.CellRow)))), toInt64_unchecked(op_Multiply(toInt64_unchecked(fromInt32(next.CellColumn)), toInt64_unchecked(fromInt32(point.CellRow))))));
                                        }, vertices, BigInt64Array), {
                                            GetZero: () => (0n),
                                            Add: (x_1, y_1) => toInt64_unchecked(op_Addition(x_1, y_1)),
                                        })), 0n)) ? true : ((vertices_2.length >= 4) && ((vertices_1 = vertices_2, (orientation = ((a, b, c) => ((((b.CellColumn - a.CellColumn) * (c.CellRow - a.CellRow)) - ((b.CellRow - a.CellRow) * (c.CellColumn - a.CellColumn))) | 0)), (onSegment = ((a_1, b_1, c_1) => {
                                            if (((min(a_1.CellColumn, c_1.CellColumn) <= b_1.CellColumn) && (b_1.CellColumn <= max(a_1.CellColumn, c_1.CellColumn))) && (min(a_1.CellRow, c_1.CellRow) <= b_1.CellRow)) {
                                                return b_1.CellRow <= max(a_1.CellRow, c_1.CellRow);
                                            }
                                            else {
                                                return false;
                                            }
                                        }), exists((x_2) => x_2, toList(delay(() => collect((first_1) => collect((second_1) => {
                                            let a_2, b_2, c_2, d, first, second, third, fourth;
                                            return ((second_1 !== (first_1 + 1)) && !((first_1 === 0) && (second_1 === (vertices_1.length - 1)))) ? singleton_1((a_2 = item_2(first_1, vertices_1), (b_2 = item_2((first_1 + 1) % vertices_1.length, vertices_1), (c_2 = item_2(second_1, vertices_1), (d = item_2((second_1 + 1) % vertices_1.length, vertices_1), (first = (orientation(a_2, b_2, c_2) | 0), (second = (orientation(a_2, b_2, d) | 0), (third = (orientation(c_2, d, a_2) | 0), (fourth = (orientation(c_2, d, b_2) | 0), (((((sign(first) !== sign(second)) && (sign(third) !== sign(fourth))) ? true : ((first === 0) && onSegment(a_2, c_2, b_2))) ? true : ((second === 0) && onSegment(a_2, d, b_2))) ? true : ((third === 0) && onSegment(c_2, a_2, d))) ? true : ((fourth === 0) && onSegment(c_2, b_2, d))))))))))) : empty_3();
                                        }, rangeDouble(first_1 + 1, 1, vertices_1.length - 1)), rangeDouble(0, 1, vertices_1.length - 1))))))))))) ? true : vertices_2.some((vertex) => {
                                            if (((vertex.CellColumn < 0) ? true : (vertex.CellRow < 0)) ? true : (vertex.CellColumn > map.Width)) {
                                                return true;
                                            }
                                            else {
                                                return vertex.CellRow > map.Height;
                                            }
                                        }))) {
                                            matchResult_11 = 1;
                                            vertices_3 = matchValue_5.fields[0];
                                        }
                                        else {
                                            matchResult_11 = 2;
                                        }
                                    }
                                    else if ((width = (matchValue_5.fields[2] | 0), (row_2 = (matchValue_5.fields[1] | 0), (height = (matchValue_5.fields[3] | 0), (column_2 = (matchValue_5.fields[0] | 0), (((((width <= 0) ? true : (height <= 0)) ? true : (column_2 < 0)) ? true : (row_2 < 0)) ? true : ((column_2 + width) > map.Width)) ? true : ((row_2 + height) > map.Height)))))) {
                                        matchResult_11 = 0;
                                        column_3 = matchValue_5.fields[0];
                                        height_1 = matchValue_5.fields[3];
                                        row_3 = matchValue_5.fields[1];
                                        width_1 = matchValue_5.fields[2];
                                    }
                                    else {
                                        matchResult_11 = 2;
                                    }
                                    switch (matchResult_11) {
                                        case 0:
                                            return "region rectangle is invalid or outside the map.";
                                        case 1:
                                            return "region polygon is invalid or outside the map.";
                                        default:
                                            return undefined;
                                    }
                                }
                            }, toList_1(map.Regions));
                            if (matchValue_49 == null) {
                                return new FSharpResult$2(/* Ok */ 0, [map_3]);
                            }
                            else {
                                return new FSharpResult$2(/* Error */ 1, ["Map zone: " + matchValue_49]);
                            }
                        }
                        else {
                            return new FSharpResult$2(/* Error */ 1, [("Unit " + int32ToString(invalid.Id)) + " does not fit the map."]);
                        }
                    }
                    else {
                        return new FSharpResult$2(/* Error */ 1, ["Only doors may carry open edge state."]);
                    }
                }
                else {
                    const row_26 = invalidEdge[0][1] | 0;
                    return new FSharpResult$2(/* Error */ 1, [((("Edge " + int32ToString(invalidEdge[0][0])) + ",") + int32ToString(row_26)) + " is outside the map."]);
                }
            }
            else {
                const row_25 = invalidTerrain[0][1] | 0;
                return new FSharpResult$2(/* Error */ 1, [((("Terrain cell " + int32ToString(invalidTerrain[0][0])) + ",") + int32ToString(row_25)) + " is outside the map."]);
            }
        }, Result_Map((map_2) => {
            if (equals(version, 4)) {
                return map_2;
            }
            else {
                const terrain_2 = ofList(collect_1((tupledArg_1) => {
                    const _arg = tupledArg_1[0];
                    const row_19 = _arg[1] | 0;
                    const column_19 = _arg[0] | 0;
                    return toList(delay(() => collect((scaledColumn) => map_10((scaledRow) => [[scaledColumn, scaledRow], tupledArg_1[1]], rangeDouble(row_19 * 2, 1, ((row_19 * 2) + 2) - 1)), rangeDouble(column_19 * 2, 1, ((column_19 * 2) + 2) - 1))));
                }, toList_1(map_2.Terrain)), {
                    Compare: (x_4, y_3) => (compareArrays(x_4, y_3) | 0),
                });
                const edges = ofList(collect_1((tupledArg_2) => {
                    const _arg_1 = tupledArg_2[0];
                    const value_4 = tupledArg_2[1];
                    const row_20 = _arg_1[1] | 0;
                    const column_20 = _arg_1[0] | 0;
                    if (_arg_1[2].tag === 1) {
                        return toList(delay(() => map_10((scaledColumn_1) => [[scaledColumn_1, ((row_20 * 2) + 2) - 1, MapEdgeDirection.SouthEdge], value_4], rangeDouble(column_20 * 2, 1, ((column_20 * 2) + 2) - 1))));
                    }
                    else {
                        return toList(delay(() => map_10((scaledRow_1) => [[((column_20 * 2) + 2) - 1, scaledRow_1, MapEdgeDirection.EastEdge], value_4], rangeDouble(row_20 * 2, 1, ((row_20 * 2) + 2) - 1))));
                    }
                }, toList_1(map_2.Edges)), {
                    Compare: (x_5, y_4) => (compareArrays(x_5, y_4) | 0),
                });
                const regions = map_11((_arg_2, region_2) => {
                    let matchValue_47;
                    return new MapRegion(region_2.Id, (matchValue_47 = region_2.Geometry, (matchValue_47.tag === 1) ? (new RegionGeometry(/* RegionPolygon */ 1, [map_8((point_2) => (new EditorCellAddress(point_2.CellColumn * 2, point_2.CellRow * 2)), matchValue_47.fields[0])])) : (new RegionGeometry(/* RegionRectangle */ 0, [matchValue_47.fields[0] * 2, matchValue_47.fields[1] * 2, matchValue_47.fields[2] * 2, matchValue_47.fields[3] * 2]))), region_2.Purpose, region_2.Behavior);
                }, map_2.Regions);
                return new MapDefinition(map_2.Width * 2, map_2.Height * 2, terrain_2, edges, map_11((_arg_3, unit_2) => (new EditorUnit(unit_2.Id, unit_2.Side, unit_2.ClassId, unit_2.Column * 2, unit_2.Row * 2, unit_2.Size * 2, unit_2.Health, unit_2.HealthMaximum, unit_2.Controller, unit_2.Script, unit_2.ScriptIndex, unit_2.BodyFacing, unit_2.AttentionDirection)), map_2.Units), map_2.NextUnitId, regions, map_2.NextRegionId);
            }
        }, result));
    }
}

function issue(code, message) {
    return new MapIssue(code, message);
}

/**
 * Returns deterministic, non-destructive semantic-edge diagnostics. Gaps
 * are reported only when two collinear segments enclose missing records;
 * intentional endpoints remain valid.
 */
export function edgeIssues(map) {
    return append_1(collect_1((tupledArg) => {
        const _arg = tupledArg[0];
        const _arg_1 = tupledArg[1];
        const row = _arg[1] | 0;
        const column = _arg[0] | 0;
        return toList(delay(() => append(((((column < 0) ? true : (row < 0)) ? true : (column >= map.Width)) ? true : (row >= map.Height)) ? singleton_1(issue("EDGE-BORDER", ((("Edge " + int32ToString(column)) + ",") + int32ToString(row)) + " has no canonical owning cell.")) : empty_3(), delay(() => ((_arg_1[1] && !equals(_arg_1[0], MapEdgeKind.Door)) ? singleton_1(issue("EDGE-OVERLAP", ((((("Only a door may carry open state at " + int32ToString(column)) + ",") + int32ToString(row)) + " ") + edgeDirectionName(_arg[2])) + ".")) : empty_3())))));
    }, toList_1(map.Edges)), toList(delay(() => collect((direction_1) => collect((matchValue) => {
        const fixedCoordinate = matchValue[0] | 0;
        return collect((matchValue_1) => {
            const last = matchValue_1[1] | 0;
            const first = matchValue_1[0] | 0;
            return ((last - first) > 1) ? collect((missing) => {
                const patternInput = equals(direction_1, MapEdgeDirection.EastEdge) ? [fixedCoordinate, missing] : [missing, fixedCoordinate];
                return singleton_1(issue("EDGE-GAP", ((((("A collinear edge run has a gap at " + int32ToString(patternInput[0])) + ",") + int32ToString(patternInput[1])) + " ") + edgeDirectionName(direction_1)) + "."));
            }, rangeDouble(first + 1, 1, last - 1)) : empty_3();
        }, pairwise(sort(List_distinct(map_9((tuple_1) => (tuple_1[1] | 0), matchValue[1]), {
            Equals: (x_1, y_1) => (x_1 === y_1),
            GetHashCode: (x_1) => (numberHash(x_1) | 0),
        }), {
            Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
        })));
    }, List_groupBy((tuple) => (tuple[0] | 0), choose((tupledArg_1) => {
        const _arg_2 = tupledArg_1[0];
        const row_1 = _arg_2[1] | 0;
        const column_1 = _arg_2[0] | 0;
        if (!equals(_arg_2[2], direction_1)) {
            return undefined;
        }
        else if (equals(direction_1, MapEdgeDirection.EastEdge)) {
            return [column_1, row_1];
        }
        else {
            return [row_1, column_1];
        }
    }, toList_1(map.Edges)), {
        Equals: (x, y) => (x === y),
        GetHashCode: (x) => (numberHash(x) | 0),
    })), [MapEdgeDirection.EastEdge, MapEdgeDirection.SouthEdge]))));
}

/**
 * Lints the complete leading side of every moving square footprint.
 */
export function leadingSideMovementIssues(map, direction, units) {
    const patternInput = directionDelta(direction);
    return ofArray(choose_1((unit) => {
        if (edgeBlocks(map, unit, patternInput[0], patternInput[1])) {
            return issue("EDGE-LEADING-SIDE", ("Unit " + int32ToString(unit.Id)) + " crosses a blocking edge on its complete leading side.");
        }
        else {
            return undefined;
        }
    }, units));
}

function polygonTwiceArea(vertices) {
    return sum(mapIndexed((index, point) => {
        const next = item_2((index + 1) % vertices.length, vertices);
        return toInt64_unchecked(op_Subtraction(toInt64_unchecked(op_Multiply(toInt64_unchecked(fromInt32(point.CellColumn)), toInt64_unchecked(fromInt32(next.CellRow)))), toInt64_unchecked(op_Multiply(toInt64_unchecked(fromInt32(next.CellColumn)), toInt64_unchecked(fromInt32(point.CellRow))))));
    }, vertices, BigInt64Array), {
        GetZero: () => (0n),
        Add: (x, y) => toInt64_unchecked(op_Addition(x, y)),
    });
}

function polygonSelfIntersects(vertices) {
    const orientation = (a, b, c) => ((((b.CellColumn - a.CellColumn) * (c.CellRow - a.CellRow)) - ((b.CellRow - a.CellRow) * (c.CellColumn - a.CellColumn))) | 0);
    const onSegment = (a_1, b_1, c_1) => {
        if (((min(a_1.CellColumn, c_1.CellColumn) <= b_1.CellColumn) && (b_1.CellColumn <= max(a_1.CellColumn, c_1.CellColumn))) && (min(a_1.CellRow, c_1.CellRow) <= b_1.CellRow)) {
            return b_1.CellRow <= max(a_1.CellRow, c_1.CellRow);
        }
        else {
            return false;
        }
    };
    return exists((x) => x, toList(delay(() => collect((first_1) => collect((second_1) => {
        let a_2, b_2, c_2, d, first, second, third, fourth;
        return !((second_1 === (first_1 + 1)) ? true : ((first_1 === 0) && (second_1 === (vertices.length - 1)))) ? singleton_1((a_2 = item_2(first_1, vertices), (b_2 = item_2((first_1 + 1) % vertices.length, vertices), (c_2 = item_2(second_1, vertices), (d = item_2((second_1 + 1) % vertices.length, vertices), (first = (orientation(a_2, b_2, c_2) | 0), (second = (orientation(a_2, b_2, d) | 0), (third = (orientation(c_2, d, a_2) | 0), (fourth = (orientation(c_2, d, b_2) | 0), (((((sign(first) !== sign(second)) && (sign(third) !== sign(fourth))) ? true : ((first === 0) && onSegment(a_2, c_2, b_2))) ? true : ((second === 0) && onSegment(a_2, d, b_2))) ? true : ((third === 0) && onSegment(c_2, a_2, d))) ? true : ((fourth === 0) && onSegment(c_2, b_2, d))))))))))) : empty_3();
    }, rangeDouble(first_1 + 1, 1, vertices.length - 1)), rangeDouble(0, 1, vertices.length - 1)))));
}

/**
 * Validates authoritative geometry independently from its semantic purpose.
 */
export function regionIssues(map) {
    return toList(delay(() => append((FSharpMap__get_Count(map.Regions) > 1600) ? singleton_1(issue("REGION-LIMIT", ("Maps support at most " + int32ToString(1600)) + " authoritative regions.")) : empty_3(), delay(() => collect_1((tupledArg) => {
        const id = tupledArg[0] | 0;
        const region = tupledArg[1];
        return toList(delay(() => append(((id <= 0) ? true : (id !== region.Id)) ? singleton_1(issue("REGION-IDENTITY", "Region keys and positive identifiers must agree.")) : empty_3(), delay(() => {
            let matchValue;
            return append((matchValue = region.Purpose, (matchValue.tag === 1) ? ((matchValue.fields[0].tag === 2) ? singleton_1(issue("REGION-PURPOSE", "Deployment zones must belong to blue or red.")) : (empty_3())) : (empty_3())), delay(() => {
                const matchValue_1 = region.Geometry;
                if (matchValue_1.tag === 1) {
                    const vertices = matchValue_1.fields[0];
                    return append((vertices.length < 3) ? singleton_1(issue("REGION-POLYGON-VERTICES", "Region polygons require at least three vertices.")) : empty_3(), delay(() => append((vertices.length > 256) ? singleton_1(issue("REGION-POLYGON-LIMIT", ("Region polygons support at most " + int32ToString(256)) + " vertices.")) : empty_3(), delay(() => {
                        let array_1;
                        return append((((array_1 = Array_distinct(vertices, {
                            Equals: equals,
                            GetHashCode: (x) => (safeHash(x) | 0),
                        }), array_1.length)) !== vertices.length) ? singleton_1(issue("REGION-POLYGON-DUPLICATE", "Region polygon vertices must be unique.")) : empty_3(), delay(() => append(vertices.some((vertex) => {
                            if (((vertex.CellColumn < 0) ? true : (vertex.CellRow < 0)) ? true : (vertex.CellColumn > map.Width)) {
                                return true;
                            }
                            else {
                                return vertex.CellRow > map.Height;
                            }
                        }) ? singleton_1(issue("REGION-OUTSIDE", ("Region " + int32ToString(id)) + " leaves the map boundary.")) : empty_3(), delay(() => append(((vertices.length >= 3) && equals_1(polygonTwiceArea(vertices), 0n)) ? singleton_1(issue("REGION-POLYGON-AREA", "Region polygons must enclose non-zero area.")) : empty_3(), delay(() => (((vertices.length >= 4) && polygonSelfIntersects(vertices)) ? singleton_1(issue("REGION-POLYGON-SELF-INTERSECTION", "Region polygons cannot self-intersect.")) : empty_3())))))));
                    }))));
                }
                else {
                    const width = matchValue_1.fields[2] | 0;
                    const row = matchValue_1.fields[1] | 0;
                    const height = matchValue_1.fields[3] | 0;
                    const column = matchValue_1.fields[0] | 0;
                    return append(((width <= 0) ? true : (height <= 0)) ? singleton_1(issue("REGION-RECTANGLE-SIZE", "Region rectangles must have positive width and height.")) : empty_3(), delay(() => (((((column < 0) ? true : (row < 0)) ? true : ((column + width) > map.Width)) ? true : ((row + height) > map.Height)) ? singleton_1(issue("REGION-OUTSIDE", ("Region " + int32ToString(id)) + " leaves the map boundary.")) : empty_3())));
                }
            }));
        }))));
    }, toList_1(map.Regions))))));
}

function validateDocument(map) {
    const dimensions = !hasSupportedDimensions(map) ? singleton(issue("MAP-DIMENSIONS", "Map dimensions must be between 4 and 40 cells.")) : empty();
    const terrain = choose((tupledArg) => {
        const _arg = tupledArg[0];
        const row = _arg[1] | 0;
        const column = _arg[0] | 0;
        if ((((column < 0) ? true : (row < 0)) ? true : (column >= map.Width)) ? true : (row >= map.Height)) {
            return issue("TERRAIN-OUTSIDE", "A terrain cell is outside the map.");
        }
        else {
            return undefined;
        }
    }, toList_1(map.Terrain));
    const edges = choose((tupledArg_1) => {
        const _arg_2 = tupledArg_1[0];
        const row_1 = _arg_2[1] | 0;
        const column_1 = _arg_2[0] | 0;
        if ((((column_1 < 0) ? true : (row_1 < 0)) ? true : (column_1 >= map.Width)) ? true : (row_1 >= map.Height)) {
            return issue("EDGE-OUTSIDE", "A semantic edge is outside the map.");
        }
        else {
            return undefined;
        }
    }, toList_1(map.Edges));
    const occupiedCellCounts = ofSeq(countBy((x) => x, collect((tupledArg_2) => {
        const unit = tupledArg_2[1];
        return cells(unit, unit.Column, unit.Row);
    }, toSeq(map.Units)), {
        Equals: equalArrays,
        GetHashCode: (x_1) => (arrayHash(x_1) | 0),
    }), {
        Compare: (x_2, y_1) => (compareArrays(x_2, y_1) | 0),
    });
    return append_1(dimensions, append_1(terrain, append_1(edges, append_1(choose((tupledArg_3) => {
        const id = tupledArg_3[0] | 0;
        const unit_1 = tupledArg_3[1];
        if ((id !== unit_1.Id) ? true : (id <= 0)) {
            return issue("UNIT-IDENTITY", "Unit keys and positive identifiers must agree.");
        }
        else if ((isNullOrWhiteSpace(unit_1.ClassId) ? true : (unit_1.ClassId.length > 128)) ? true : exists_1(isWhiteSpace, unit_1.ClassId.split(""))) {
            return issue("UNIT-CLASS", "A unit class ID must be one non-empty token.");
        }
        else if (((unit_1.HealthMaximum <= 0) ? true : (unit_1.Health < 0)) ? true : (unit_1.Health > unit_1.HealthMaximum)) {
            return issue("UNIT-HEALTH", "Unit health is outside its accepted range.");
        }
        else if ((unit_1.Size < 1) ? true : (unit_1.Size > 8)) {
            return issue("UNIT-FOOTPRINT", ("Unit footprints must be between 1 and " + int32ToString(8)) + " cells square.");
        }
        else if (exists((tupledArg_4) => {
            const column_2 = tupledArg_4[0] | 0;
            const row_2 = tupledArg_4[1] | 0;
            if (((((column_2 < 0) ? true : (row_2 < 0)) ? true : (column_2 >= map.Width)) ? true : (row_2 >= map.Height)) ? true : equals(tryFind_1([column_2, row_2], map.Terrain), MapTerrain.Blocked)) {
                return true;
            }
            else {
                return defaultArg(tryFind_1([column_2, row_2], occupiedCellCounts), 0) > 1;
            }
        }, cells(unit_1, unit_1.Column, unit_1.Row))) {
            return issue("UNIT-PLACEMENT", ("Unit " + int32ToString(id)) + " does not fit.");
        }
        else {
            return undefined;
        }
    }, toList_1(map.Units)), regionIssues(map)))));
}

/**
 * Runs validation against the authoritative document only. Layer
 * visibility and locks are deliberately ignored, so hidden content keeps
 * participating in validation and simulation.
 */
export function validationIssues(map) {
    return toArray_2(sortBy((candidate) => [candidate.Code, candidate.Message], append_1(validateDocument(map), edgeIssues(map)), {
        Compare: (x, y) => (compareArrays(x, y) | 0),
    }));
}

export function applyCommand(_arg, map) {
    const command = _arg.fields[0];
    let matchResult, units, regions;
    switch (command.tag) {
        case 1: {
            matchResult = 1;
            break;
        }
        case 2: {
            matchResult = 2;
            units = command.fields[0];
            break;
        }
        case 3: {
            matchResult = 2;
            units = command.fields[0];
            break;
        }
        case 4: {
            matchResult = 3;
            break;
        }
        case 5: {
            matchResult = 4;
            regions = command.fields[0];
            break;
        }
        case 6: {
            matchResult = 4;
            regions = command.fields[0];
            break;
        }
        case 7: {
            matchResult = 5;
            break;
        }
        case 8: {
            matchResult = 6;
            break;
        }
        case 9: {
            matchResult = 7;
            break;
        }
        default:
            matchResult = 0;
    }
    switch (matchResult) {
        case 0: {
            const terrain = command.fields[0];
            return new MapDefinition(map.Width, map.Height, fold_1((current, address) => {
                if (equals(terrain, MapTerrain.Open)) {
                    return remove([address.CellColumn, address.CellRow], current);
                }
                else {
                    return add([address.CellColumn, address.CellRow], terrain, current);
                }
            }, map.Terrain, command.fields[1]), map.Edges, map.Units, map.NextUnitId, map.Regions, map.NextRegionId);
        }
        case 1:
            return new MapDefinition(map.Width, map.Height, map.Terrain, fold_1((current_1, tupledArg) => {
                const address_1 = tupledArg[0];
                const replacement = tupledArg[1];
                if (replacement == null) {
                    return remove(address_1, current_1);
                }
                else {
                    return add(address_1, replacement, current_1);
                }
            }, map.Edges, command.fields[0]), map.Units, map.NextUnitId, map.Regions, map.NextRegionId);
        case 2:
            return new MapDefinition(map.Width, map.Height, map.Terrain, map.Edges, fold_1((current_2, unit) => add(unit.Id, unit, current_2), map.Units, units), fold_1((next, unit_1) => (max(next, unit_1.Id + 1) | 0), map.NextUnitId, units), map.Regions, map.NextRegionId);
        case 3:
            return new MapDefinition(map.Width, map.Height, map.Terrain, map.Edges, fold_1((current_3, id) => remove(id, current_3), map.Units, command.fields[0]), map.NextUnitId, map.Regions, map.NextRegionId);
        case 4:
            return new MapDefinition(map.Width, map.Height, map.Terrain, map.Edges, map.Units, map.NextUnitId, fold_1((current_4, region) => add(region.Id, region, current_4), map.Regions, regions), fold_1((next_1, region_1) => (max(next_1, region_1.Id + 1) | 0), map.NextRegionId, regions));
        case 5:
            return new MapDefinition(map.Width, map.Height, map.Terrain, map.Edges, map.Units, map.NextUnitId, fold_1((current_5, id_1) => remove(id_1, current_5), map.Regions, command.fields[0]), map.NextRegionId);
        case 6:
            return new MapDefinition(command.fields[0], command.fields[1], map.Terrain, map.Edges, map.Units, map.NextUnitId, map.Regions, map.NextRegionId);
        default:
            return command.fields[1];
    }
}

export function validateCommand(map, command) {
    let width, height;
    const duplicate = (values) => {
        let array_1;
        return ((array_1 = Array_distinct(values, {
            Equals: equals,
            GetHashCode: (x) => (structuralHash(x) | 0),
        }), array_1.length)) !== values.length;
    };
    const duplicateEdgeAddress = (changes) => {
        let array_6;
        const option_1 = tryFind_2((tupledArg) => (tupledArg[1].length > 1), Array_groupBy((tuple) => tuple[0], changes, {
            Equals: equals,
            GetHashCode: (x_1) => (structuralHash(x_1) | 0),
        }));
        if (option_1 != null) {
            return ((array_6 = Array_distinct(map_8((tuple_1) => tuple_1[1], option_1[1]), {
                Equals: equals,
                GetHashCode: (x_2) => (structuralHash(x_2) | 0),
            }), array_6.length)) === 1;
        }
        else {
            return undefined;
        }
    };
    const shapeIssues = (command.tag === 0) ? ((command.fields[1].length === 0) ? singleton(issue("COMMAND-EMPTY", "A paint command must contain at least one cell.")) : empty()) : ((command.tag === 1) ? ((command.fields[0].length === 0) ? singleton(issue("COMMAND-EMPTY", "An edge command must contain at least one change.")) : (equals(duplicateEdgeAddress(command.fields[0]), true) ? singleton(issue("EDGE-DUPLICATE", "An edge command contains the same canonical edge record more than once.")) : (equals(duplicateEdgeAddress(command.fields[0]), false) ? singleton(issue("EDGE-OVERLAP", "An edge command assigns conflicting meanings to one canonical edge record.")) : empty()))) : ((command.tag === 2) ? ((command.fields[0].length === 0) ? singleton(issue("COMMAND-EMPTY", "An add command must contain at least one unit.")) : (duplicate(map_8((_arg_2) => (_arg_2.Id | 0), command.fields[0], Int32Array)) ? singleton(issue("UNIT-DUPLICATE", "An add command contains duplicate unit identifiers.")) : (command.fields[0].some((unit) => containsKey(unit.Id, map.Units)) ? singleton(issue("UNIT-DUPLICATE", "An added unit identifier already exists.")) : (command.fields[0].some((unit_1) => exists((tupledArg_2) => edgeIsBlocking(map, tupledArg_2[0], tupledArg_2[1], tupledArg_2[2]), toList(delay(() => append(collect((y_3) => map_10((x_3) => [x_3, y_3, MapEdgeDirection.EastEdge], rangeDouble(unit_1.Column, 1, (unit_1.Column + unit_1.Size) - 2)), rangeDouble(unit_1.Row, 1, (unit_1.Row + unit_1.Size) - 1)), delay(() => collect((y_4) => map_10((x_4) => [x_4, y_4, MapEdgeDirection.SouthEdge], rangeDouble(unit_1.Column, 1, (unit_1.Column + unit_1.Size) - 1)), rangeDouble(unit_1.Row, 1, (unit_1.Row + unit_1.Size) - 2)))))))) ? singleton(issue("UNIT-EDGE", "A blocking edge crosses an added unit footprint.")) : empty())))) : ((command.tag === 3) ? ((command.fields[0].length === 0) ? singleton(issue("COMMAND-EMPTY", "An update command must contain at least one unit.")) : (duplicate(map_8((_arg_3) => (_arg_3.Id | 0), command.fields[0], Int32Array)) ? singleton(issue("UNIT-DUPLICATE", "An update command contains duplicate unit identifiers.")) : (command.fields[0].some((unit_2) => !containsKey(unit_2.Id, map.Units)) ? singleton(issue("UNIT-MISSING", "An updated unit does not exist.")) : empty()))) : ((command.tag === 4) ? ((command.fields[0].length === 0) ? singleton(issue("COMMAND-EMPTY", "A remove command must contain at least one unit.")) : (duplicate(command.fields[0]) ? singleton(issue("UNIT-DUPLICATE", "A remove command contains duplicate identifiers.")) : (command.fields[0].some((id) => !containsKey(id, map.Units)) ? singleton(issue("UNIT-MISSING", "A removed unit does not exist.")) : empty()))) : ((command.tag === 5) ? ((command.fields[0].length === 0) ? singleton(issue("COMMAND-EMPTY", "An add command must contain at least one region.")) : (duplicate(map_8((_arg_4) => (_arg_4.Id | 0), command.fields[0], Int32Array)) ? singleton(issue("REGION-DUPLICATE", "An add command contains duplicate region identifiers.")) : (command.fields[0].some((region) => containsKey(region.Id, map.Regions)) ? singleton(issue("REGION-DUPLICATE", "An added region identifier already exists.")) : empty()))) : ((command.tag === 6) ? ((command.fields[0].length === 0) ? singleton(issue("COMMAND-EMPTY", "An update command must contain at least one region.")) : (duplicate(map_8((_arg_5) => (_arg_5.Id | 0), command.fields[0], Int32Array)) ? singleton(issue("REGION-DUPLICATE", "An update command contains duplicate region identifiers.")) : (command.fields[0].some((region_1) => !containsKey(region_1.Id, map.Regions)) ? singleton(issue("REGION-MISSING", "An updated region does not exist.")) : empty()))) : ((command.tag === 7) ? ((command.fields[0].length === 0) ? singleton(issue("COMMAND-EMPTY", "A remove command must contain at least one region.")) : (duplicate(command.fields[0]) ? singleton(issue("REGION-DUPLICATE", "A remove command contains duplicate identifiers.")) : (command.fields[0].some((id_1) => !containsKey(id_1, map.Regions)) ? singleton(issue("REGION-MISSING", "A removed region does not exist.")) : empty()))) : ((command.tag === 8) ? (((width = (command.fields[0] | 0), (height = (command.fields[1] | 0), (((width < 4) ? true : (width > 40)) ? true : (height < 4)) ? true : (height > 40)))) ? singleton(issue("MAP-DIMENSIONS", "Map dimensions must be between 4 and 40 cells.")) : empty()) : empty()))))))));
    if (!isEmpty(shapeIssues)) {
        return new FSharpResult$2(/* Error */ 1, [shapeIssues]);
    }
    else {
        const matchValue = validateDocument(applyCommand(new ValidatedEditorCommand(command), map));
        if (isEmpty(matchValue)) {
            return new FSharpResult$2(/* Ok */ 0, [new ValidatedEditorCommand(command)]);
        }
        else {
            return new FSharpResult$2(/* Error */ 1, [matchValue]);
        }
    }
}

function historyWithinBounds(entries) {
    return withinBounds(100, 2000000, entries);
}

function historySize(entries) {
    return size_8(entries) | 0;
}

function selectedAfterMap(map, selected_1) {
    return filter_3((id) => containsKey(id, map.Units), selected_1);
}

function commit(command, state) {
    let option_3, option_1, option_5, bind$0040;
    const matchValue = validateCommand(state.Map, command);
    if (matchValue.tag === 0) {
        const map = applyCommand(matchValue.fields[0], state.Map);
        if (equals(map, state.Map)) {
            return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
        }
        else {
            const before = state.Revision;
            const after = revision(toInt64_unchecked(op_Addition(before.Number, 1n)), before.Digest, map);
            const undo = historyWithinBounds(cons(new EditorHistoryEntry(command, before, after, serializedBytes(before.Document) + serializedBytes(map)), state.UndoHistory));
            const selected_1 = selectedAfterMap(map, state.SelectedUnits);
            return new MapEditorState(map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, (option_3 = ((option_1 = state.SelectedUnit, (option_1 != null) ? (contains(option_1, selected_1) ? option_1 : undefined) : undefined)), (option_3 != null) ? option_3 : tryHead(toList_2(selected_1))), selected_1, (option_5 = state.SelectedRegion, (option_5 != null) ? (containsKey(option_5, map.Regions) ? option_5 : undefined) : undefined), EditorGesture.IdleGesture, after, RevisionState_9.DirtyRevision, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, undo, empty(), historySize(undo), state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, validationIssues(map), (validationIssues(map).length === 0) ? undefined : 0, undefined, state.PendingRecovery, (bind$0040 = state.Authoring, new MapAuthoringMetadata(bind$0040.Name, bind$0040.SavedViews, after.Digest, undefined)));
        }
    }
    else {
        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, join(" ", map_9((_arg) => _arg.Message, matchValue.fields[0])), state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
    }
}

function validEdgeKey(map, column, row, _arg) {
    if (((column >= 0) && (row >= 0)) && (column < map.Width)) {
        return row < map.Height;
    }
    else {
        return false;
    }
}

function edgeEndpoints(column, row, direction) {
    if (direction.tag === 1) {
        return [[column, row + 1], [column + 1, row + 1]];
    }
    else {
        return [[column + 1, row], [column + 1, row + 1]];
    }
}

function connectingEdgePath(map, source_, source__1, source__2, target_, target__1, target__2) {
    let option_2;
    const source = [source_, source__1, source__2];
    const target = [target_, target__1, target__2];
    let sourceEndpoints;
    const patternInput = edgeEndpoints(source[0], source[1], source[2]);
    sourceEndpoints = ofArray([patternInput[0], patternInput[1]]);
    let targetEndpoints;
    const patternInput_1 = edgeEndpoints(target[0], target[1], target[2]);
    targetEndpoints = ofArray([patternInput_1[0], patternInput_1[1]]);
    const value = [target];
    return defaultArg((option_2 = tryHead(sortBy((tupledArg_1) => [tupledArg_1[0], tupledArg_1[1], tupledArg_1[2], tupledArg_1[3]], toList(delay(() => collect((sourcePoint) => collect((targetPoint) => collect((horizontalFirst_1) => {
        let start, finish, vertices, y, x, finishY, finishX, horizontal, vertical;
        const normalized = toArray_1(map_10((tupledArg) => {
            const _arg = tupledArg[0];
            const _arg_1 = tupledArg[1];
            return tryNormalizeEdge(map.Width, map.Height, _arg[0], _arg[1], _arg_1[0], _arg_1[1]);
        }, pairwise_1((start = sourcePoint, (finish = targetPoint, (vertices = [], (y = start[1], (x = start[0], (finishY = (finish[1] | 0), (finishX = (finish[0] | 0), (void (vertices.push([x, y])), (horizontal = (() => {
            while (x !== finishX) {
                x = ((x + ((finishX > x) ? 1 : -1)) | 0);
                void (vertices.push([x, y]));
            }
        }), (vertical = (() => {
            while (y !== finishY) {
                y = ((y + ((finishY > y) ? 1 : -1)) | 0);
                void (vertices.push([x, y]));
            }
        }), (horizontalFirst_1 ? ((horizontal(), vertical())) : ((vertical(), horizontal())), vertices.slice()))))))))))))));
        return normalized.every((option) => (option != null)) ? singleton_1([normalized.length, sourcePoint, targetPoint, horizontalFirst_1, choose_1((x_1) => x_1, normalized)]) : empty_3();
    }, [true, false]), targetEndpoints), sourceEndpoints))), {
        Compare: (x_2, y_1) => (compareArrays(x_2, y_1) | 0),
    })), (option_2 != null) ? toArray_1(distinct(append([target], option_2[4]), {
        Equals: equalArrays,
        GetHashCode: (x_3) => (arrayHash(x_3) | 0),
    })) : undefined), value);
}

function finishEdgePolyline(state) {
    const matchValue = state.Gesture;
    if (matchValue.tag === 6) {
        if (matchValue.fields[1].length === 0) {
            return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, "Empty edge polyline canceled.", state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
        }
        else {
            const next = commit(new EditorCommand(/* ReplaceEdges */ 1, [map_8((address) => [address, [matchValue.fields[0], false]], Array_distinct(matchValue.fields[1], {
                Equals: equalArrays,
                GetHashCode: (x) => (arrayHash(x) | 0),
            }))]), state);
            return new MapEditorState(next.Map, next.Tool, next.TerrainSelection, next.BrushSize, next.TerrainCursor, next.KeyboardCursor, next.KeyboardObject, next.LastTerrainPaintTool, next.TerrainAnnouncement, next.EdgeCursor, (next.Validation != null) ? ("Edge polyline rejected. " + value_18(next.Validation)) : (((((("Committed " + int32ToString(matchValue.fields[1].length)) + "-segment ") + edgeKindName(matchValue.fields[0])) + " polyline in revision ") + int64ToString(next.Revision.Number)) + "."), next.UnitPaletteSearch, next.UnitPaletteCursor, next.UnitPlacementCursor, next.UnitAnnouncement, next.RegionAnnouncement, next.RegionKeyboardMode, next.SelectedUnit, next.SelectedUnits, next.SelectedRegion, next.Gesture, next.Revision, next.RevisionState, next.SavedDigest, next.SimulatedDigest, next.RecoveredFromDigest, next.UndoHistory, next.RedoHistory, next.HistoryBytes, next.Clipboard, next.Tick, next.IsRunning, next.LastEvents, next.Validation, next.Layers, next.Issues, next.ActiveIssue, next.PendingDestructiveChange, next.PendingRecovery, next.Authoring);
        }
    }
    else {
        return state;
    }
}

function replaceOneEdge(address_, address__1, address__2, replacement, announcement, state) {
    const address = [address_, address__1, address__2];
    if (!validEdgeKey(state.Map, address[0], address[1], address[2])) {
        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, "Edge change rejected at the map border.", state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "The edge has no canonical owning cell.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
    }
    else {
        const next = commit(new EditorCommand(/* ReplaceEdges */ 1, [[[address, replacement]]]), state);
        return new MapEditorState(next.Map, next.Tool, next.TerrainSelection, next.BrushSize, next.TerrainCursor, next.KeyboardCursor, next.KeyboardObject, next.LastTerrainPaintTool, next.TerrainAnnouncement, address, announcement, next.UnitPaletteSearch, next.UnitPaletteCursor, next.UnitPlacementCursor, next.UnitAnnouncement, next.RegionAnnouncement, next.RegionKeyboardMode, next.SelectedUnit, next.SelectedUnits, next.SelectedRegion, next.Gesture, next.Revision, next.RevisionState, next.SavedDigest, next.SimulatedDigest, next.RecoveredFromDigest, next.UndoHistory, next.RedoHistory, next.HistoryBytes, next.Clipboard, next.Tick, next.IsRunning, next.LastEvents, next.Validation, next.Layers, next.Issues, next.ActiveIssue, next.PendingDestructiveChange, next.PendingRecovery, next.Authoring);
    }
}

function activeDomain(tool) {
    switch (tool.tag) {
        case 2:
            return EditorDomain.TerrainDomain;
        case 5:
            return EditorDomain.EdgeDomain;
        case 3:
        case 4:
            return EditorDomain.UnitDomain;
        case 0:
            return EditorDomain.UnitDomain;
        default:
            return EditorDomain.TerrainDomain;
    }
}

export function layerState(domain, state) {
    return defaultArg(tryFind_1(domain, state.Layers), EditorLayerState.VisibleLayer);
}

function actionDomain(action, state) {
    switch (action.tag) {
        case 59:
        case 60:
        case 4:
        case 1:
        case 2:
            return EditorDomain.TerrainDomain;
        case 12:
        case 13:
        case 14:
        case 15:
        case 16:
        case 17:
        case 18:
        case 19:
        case 20:
        case 21:
            return EditorDomain.EdgeDomain;
        case 56:
        case 23:
        case 24:
        case 25:
        case 26:
        case 27:
        case 28:
        case 29:
        case 30:
        case 31:
        case 57:
        case 58:
        case 95:
        case 96:
        case 97:
        case 98:
        case 99:
        case 100:
        case 101:
        case 102:
        case 90:
        case 88:
        case 89:
            return EditorDomain.UnitDomain;
        case 48:
        case 49:
        case 32:
        case 33:
        case 34:
        case 35:
        case 36:
        case 37:
        case 38:
        case 39:
        case 40:
        case 41:
        case 42:
        case 43:
        case 44:
        case 45:
        case 46:
        case 47:
        case 51:
        case 52:
        case 53:
        case 54:
        case 55:
            return EditorDomain.RegionDomain;
        case 62:
        case 63:
        case 64:
        case 105:
        case 106:
        case 65:
            return EditorDomain.DocumentDomain;
        case 61:
            return activeDomain(state.Tool);
        case 82: {
            const matchValue = state.Gesture;
            switch (matchValue.tag) {
                case 5:
                    return EditorDomain.TerrainDomain;
                case 6:
                    return EditorDomain.EdgeDomain;
                case 3:
                case 4:
                    return EditorDomain.UnitDomain;
                default:
                    return undefined;
            }
        }
        default:
            return undefined;
    }
}

function isTerrainAuthoringTool(tool) {
    if (tool.tag === 2) {
        return true;
    }
    else {
        return false;
    }
}

function idsInBox(box, map) {
    const minimumColumn = min(box.FirstColumn, box.LastColumn) | 0;
    const maximumColumn = max(box.FirstColumn, box.LastColumn) | 0;
    const minimumRow = min(box.FirstRow, box.LastRow) | 0;
    const maximumRow = max(box.FirstRow, box.LastRow) | 0;
    return ofList_1(choose((tupledArg) => {
        const unit = tupledArg[1];
        const lastColumn = ((unit.Column + unit.Size) - 1) | 0;
        const lastRow = ((unit.Row + unit.Size) - 1) | 0;
        if ((((unit.Column <= maximumColumn) && (lastColumn >= minimumColumn)) && (unit.Row <= maximumRow)) && (lastRow >= minimumRow)) {
            return tupledArg[0];
        }
        else {
            return undefined;
        }
    }, toList_1(map.Units)), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
}

function regionContains(address, region) {
    const column = address.CellColumn | 0;
    const row = address.CellRow | 0;
    const matchValue = region.Geometry;
    if (matchValue.tag === 1) {
        if (matchValue.fields[0].length === 0) {
            return false;
        }
        else {
            const pointX = column + 0.5;
            const pointY = row + 0.5;
            let inside = false;
            let previous = matchValue.fields[0].length - 1;
            for (let current = 0; current <= (matchValue.fields[0].length - 1); current++) {
                const currentX = item_2(current, matchValue.fields[0]).CellColumn;
                const currentY = item_2(current, matchValue.fields[0]).CellRow;
                const previousX = item_2(previous, matchValue.fields[0]).CellColumn;
                const previousY = item_2(previous, matchValue.fields[0]).CellRow;
                if (((currentY > pointY) !== (previousY > pointY)) && (pointX < ((((previousX - currentX) * (pointY - currentY)) / (previousY - currentY)) + currentX))) {
                    inside = !inside;
                }
                previous = (current | 0);
            }
            return inside;
        }
    }
    else if (((column >= matchValue.fields[0]) && (column < (matchValue.fields[0] + matchValue.fields[2]))) && (row >= matchValue.fields[1])) {
        return row < (matchValue.fields[1] + matchValue.fields[3]);
    }
    else {
        return false;
    }
}

/**
 * Deterministic object order used by keyboard traversal and its live
 * description: units, regions, east/south edges, then the terrain cell.
 */
export function keyboardObjectsAtCursor(state) {
    const address = state.KeyboardCursor.Cell;
    return toList(delay(() => append(choose((tupledArg) => {
        const unit = tupledArg[1];
        if (contains_1([address.CellColumn, address.CellRow], cells(unit, unit.Column, unit.Row), {
            Equals: equalArrays,
            GetHashCode: (x_1) => (arrayHash(x_1) | 0),
        })) {
            return new EditorKeyboardObject(/* KeyboardUnit */ 0, [tupledArg[0]]);
        }
        else {
            return undefined;
        }
    }, sortBy((tuple) => (tuple[0] | 0), toList_1(state.Map.Units), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    })), delay(() => append(choose((tupledArg_1) => {
        if (regionContains(address, tupledArg_1[1])) {
            return new EditorKeyboardObject(/* KeyboardRegion */ 1, [tupledArg_1[0]]);
        }
        else {
            return undefined;
        }
    }, sortBy((tuple_1) => (tuple_1[0] | 0), toList_1(state.Map.Regions), {
        Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
    })), delay(() => append(collect((direction) => (containsKey([address.CellColumn, address.CellRow, direction], state.Map.Edges) ? singleton_1(new EditorKeyboardObject(/* KeyboardEdge */ 2, [address.CellColumn, address.CellRow, direction])) : empty_3()), [MapEdgeDirection.EastEdge, MapEdgeDirection.SouthEdge]), delay(() => singleton_1(new EditorKeyboardObject(/* KeyboardTerrain */ 3, [address]))))))))));
}

function objectAtKeyboardCursor(state) {
    const objects = keyboardObjectsAtCursor(state);
    if (isEmpty(objects)) {
        return undefined;
    }
    else {
        return item_1(((state.KeyboardCursor.ObjectCycleIndex % length_1(objects)) + length_1(objects)) % length_1(objects), objects);
    }
}

function translatedUnits(offset, source, nextId) {
    return mapIndexed((index, unit) => (new EditorUnit(nextId + index, unit.Side, unit.ClassId, unit.Column + offset, unit.Row + offset, unit.Size, unit.Health, unit.HealthMaximum, unit.Controller, unit.Script, 0, unit.BodyFacing, unit.AttentionDirection)), sortBy_2((_arg) => (_arg.Id | 0), source, {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    }));
}

function placementUnit(side, classId, size, address, state) {
    let option_1, option_4;
    const preset = tryFind((candidate) => {
        if (equals(candidate.Side, side) && (candidate.ClassId === classId)) {
            return candidate.FootprintSize === size;
        }
        else {
            return false;
        }
    }, canonicalFootprintPresets);
    return new EditorUnit(state.Map.NextUnitId, side, classId, address.CellColumn, address.CellRow, size, defaultArg((option_1 = preset, (option_1 != null) ? option_1.Health : undefined), 12), defaultArg((option_4 = preset, (option_4 != null) ? option_4.HealthMaximum : undefined), 12), MapController.Manual, empty(), 0, Direction8.North, Direction8.North);
}

function placementIssue(map, excludedUnit, unit, column, row) {
    const targetCells = cells(unit, column, row);
    if (exists((tupledArg) => {
        const x = tupledArg[0] | 0;
        const y = tupledArg[1] | 0;
        if (((x < 0) ? true : (y < 0)) ? true : (x >= map.Width)) {
            return true;
        }
        else {
            return y >= map.Height;
        }
    }, targetCells)) {
        return "outside the map";
    }
    else if (exists((cell) => equals(tryFind_1(cell, map.Terrain), MapTerrain.Blocked), targetCells)) {
        return "blocked terrain is inside the footprint";
    }
    else {
        const occupied = ofList_1(collect_1((tupledArg_2) => {
            const other = tupledArg_2[1];
            return cells(other, other.Column, other.Row);
        }, filter((tupledArg_1) => !equals(tupledArg_1[0], excludedUnit), toList_1(map.Units))), {
            Compare: (x_1, y_1) => (compareArrays(x_1, y_1) | 0),
        });
        if (exists((cell_1) => contains(cell_1, occupied), targetCells)) {
            return "another unit occupies the footprint";
        }
        else if (exists((tupledArg_3) => edgeIsBlocking(map, tupledArg_3[0], tupledArg_3[1], tupledArg_3[2]), toList(delay(() => append(collect((y_2) => map_10((x_2) => [x_2, y_2, MapEdgeDirection.EastEdge], rangeDouble(column, 1, (column + unit.Size) - 2)), rangeDouble(row, 1, (row + unit.Size) - 1)), delay(() => collect((y_3) => map_10((x_3) => [x_3, y_3, MapEdgeDirection.SouthEdge], rangeDouble(column, 1, (column + unit.Size) - 1)), rangeDouble(row, 1, (row + unit.Size) - 2)))))))) {
            return "a blocking edge crosses the footprint";
        }
        else {
            return undefined;
        }
    }
}

export function unitPlacementIssue(state) {
    const matchValue = state.Tool;
    if (matchValue.tag === 4) {
        const address = state.UnitPlacementCursor;
        return placementIssue(state.Map, undefined, placementUnit(matchValue.fields[0], matchValue.fields[1], matchValue.fields[2], address, state), address.CellColumn, address.CellRow);
    }
    else {
        return "no unit preset is armed";
    }
}

function visibleUnitPresets(state) {
    return searchCanonicalUnitPresets(state.UnitPaletteSearch);
}

export function selectedUnitPalettePreset(state) {
    const visible = visibleUnitPresets(state);
    let option_3;
    const option_1 = state.UnitPaletteCursor.PresetId;
    if (option_1 != null) {
        const id = option_1;
        option_3 = tryFind((preset) => (preset.Id === id), visible);
    }
    else {
        option_3 = undefined;
    }
    if (option_3 != null) {
        return option_3;
    }
    else {
        return tryHead(visible);
    }
}

function paletteCursorFor(preferredId, preferredIndex, state) {
    let option_1, id;
    const visible = visibleUnitPresets(state);
    if (isEmpty(visible)) {
        return new UnitPaletteCursor_3(undefined, 0, 0);
    }
    else {
        const index = defaultArg((option_1 = preferredId, (option_1 != null) ? ((id = option_1, tryFindIndex((preset) => (preset.Id === id), visible))) : undefined), max(0, min(length_1(visible) - 1, preferredIndex))) | 0;
        const preset_1 = item_1(index, visible);
        return new UnitPaletteCursor_3(preset_1.Id, defaultArg(tryFindIndex((y_1) => (preset_1.Faction === y_1), List_distinct(map_9((_arg) => _arg.Faction, visible), {
            Equals: (x, y) => (x === y),
            GetHashCode: (x) => (stringHash(x) | 0),
        })), 0), index);
    }
}

function selectedUnits(state) {
    return sortBy_2((_arg) => (_arg.Id | 0), choose_1((id) => tryFind_1(id, state.Map.Units), toArray_3(state.SelectedUnits)), {
        Compare: (x, y) => (comparePrimitives(x, y) | 0),
    });
}

function translatedRegionGeometry(columnDelta, rowDelta, geometry) {
    if (geometry.tag === 1) {
        return new RegionGeometry(/* RegionPolygon */ 1, [map_8((vertex) => (new EditorCellAddress(vertex.CellColumn + columnDelta, vertex.CellRow + rowDelta)), geometry.fields[0])]);
    }
    else {
        return new RegionGeometry(/* RegionRectangle */ 0, [geometry.fields[0] + columnDelta, geometry.fields[1] + rowDelta, geometry.fields[2], geometry.fields[3]]);
    }
}

function regionBounds(geometry) {
    if (geometry.tag === 1) {
        const vertices = geometry.fields[0];
        return [minBy((_arg) => (_arg.CellColumn | 0), vertices, {
            Compare: (x, y) => (comparePrimitives(x, y) | 0),
        }).CellColumn, minBy((_arg_2) => (_arg_2.CellRow | 0), vertices, {
            Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
        }).CellRow, maxBy((_arg_4) => (_arg_4.CellColumn | 0), vertices, {
            Compare: (x_2, y_2) => (comparePrimitives(x_2, y_2) | 0),
        }).CellColumn, maxBy((_arg_6) => (_arg_6.CellRow | 0), vertices, {
            Compare: (x_3, y_3) => (comparePrimitives(x_3, y_3) | 0),
        }).CellRow];
    }
    else {
        const row = geometry.fields[1] | 0;
        const column = geometry.fields[0] | 0;
        return [column, row, (column + geometry.fields[2]) - 1, (row + geometry.fields[3]) - 1];
    }
}

function translatedRegionPreview(map, columnDelta, rowDelta, geometry) {
    const patternInput = regionBounds(geometry);
    return translatedRegionGeometry(max(op_UnaryNegation_Int32(patternInput[0]), min((map.Width - 1) - patternInput[2], columnDelta)), max(op_UnaryNegation_Int32(patternInput[1]), min((map.Height - 1) - patternInput[3], rowDelta)), geometry);
}

function selectedRegion(state) {
    const option_1 = state.SelectedRegion;
    if (option_1 != null) {
        return tryFind_1(option_1, state.Map.Regions);
    }
    else {
        return undefined;
    }
}

function regionPurposeName(_arg) {
    if (_arg.tag === 1) {
        switch (_arg.fields[0].tag) {
            case 1:
                return "Red deployment";
            case 2:
                return "Neutral deployment";
            default:
                return "Blue deployment";
        }
    }
    else {
        return "Objective";
    }
}

function translatedSelection(columnDelta, rowDelta, source) {
    return map_8((unit) => (new EditorUnit(unit.Id, unit.Side, unit.ClassId, unit.Column + columnDelta, unit.Row + rowDelta, unit.Size, unit.Health, unit.HealthMaximum, unit.Controller, unit.Script, unit.ScriptIndex, unit.BodyFacing, unit.AttentionDirection)), source);
}

function movementCrossesEdge(map, columnDelta, rowDelta, source) {
    const stepX = sign(columnDelta) | 0;
    const stepY = sign(rowDelta) | 0;
    const steps = max(Math.abs(columnDelta), Math.abs(rowDelta)) | 0;
    if (steps === 0) {
        return false;
    }
    else {
        return exists((step_1) => {
            const offset = step_1 | 0;
            return source.some((unit) => edgeBlocks(map, new EditorUnit(unit.Id, unit.Side, unit.ClassId, unit.Column + (stepX * offset), unit.Row + (stepY * offset), unit.Size, unit.Health, unit.HealthMaximum, unit.Controller, unit.Script, unit.ScriptIndex, unit.BodyFacing, unit.AttentionDirection), stepX, stepY));
        }, toList(rangeDouble(0, 1, steps - 1)));
    }
}

function unitPreviewMessage(prefix, map, command) {
    const matchValue = validateCommand(map, command);
    if (matchValue.tag === 1) {
        return (prefix + " Invalid destination: ") + join(" ", map_9((_arg) => _arg.Message, matchValue.fields[0]));
    }
    else {
        return prefix + " Valid destination.";
    }
}

function tryTranslatedCommand(source, state) {
    return tryPick_1((offset) => {
        const units = translatedUnits(offset, source, state.Map.NextUnitId);
        const command = new EditorCommand(/* AddUnits */ 2, [units]);
        const matchValue = validateCommand(state.Map, command);
        if (matchValue.tag === 1) {
            return undefined;
        }
        else {
            return [command, units];
        }
    }, toArray_1(rangeDouble(1, 1, max(state.Map.Width, state.Map.Height))));
}

function legacyCommand(action, map) {
    return new EditorCommand(/* ReplaceDocument */ 9, [(action.tag === 61) ? "activate" : ((action.tag === 62) ? "resize" : ((action.tag === 95) ? "remove" : ((action.tag === 96) ? "update-unit" : ((action.tag === 97) ? "update-unit" : ((action.tag === 98) ? "update-unit" : ((action.tag === 99) ? "update-unit" : ((action.tag === 100) ? "update-unit" : ((action.tag === 101) ? "update-unit" : ((action.tag === 102) ? "move-unit" : ((action.tag === 104) ? "simulation-step" : ((action.tag === 105) ? "clear" : ((action.tag === 106) ? "import" : "editor")))))))))))), map]);
}

export function update(action, state) {
    const state_1 = (action.tag === 0) ? ((state.Gesture.tag === 6) ? finishEdgePolyline(state) : state) : state;
    const matchValue_2 = actionDomain(action, state_1);
    let matchResult, domain_1;
    if (matchValue_2 != null) {
        if (equals(layerState(matchValue_2, state_1), EditorLayerState.LockedLayer)) {
            matchResult = 0;
            domain_1 = matchValue_2;
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
            return new MapEditorState(state_1.Map, state_1.Tool, state_1.TerrainSelection, state_1.BrushSize, state_1.TerrainCursor, state_1.KeyboardCursor, state_1.KeyboardObject, state_1.LastTerrainPaintTool, state_1.TerrainAnnouncement, state_1.EdgeCursor, state_1.EdgeAnnouncement, state_1.UnitPaletteSearch, state_1.UnitPaletteCursor, state_1.UnitPlacementCursor, state_1.UnitAnnouncement, state_1.RegionAnnouncement, state_1.RegionKeyboardMode, state_1.SelectedUnit, state_1.SelectedUnits, state_1.SelectedRegion, state_1.Gesture, state_1.Revision, state_1.RevisionState, state_1.SavedDigest, state_1.SimulatedDigest, state_1.RecoveredFromDigest, state_1.UndoHistory, state_1.RedoHistory, state_1.HistoryBytes, state_1.Clipboard, state_1.Tick, state_1.IsRunning, state_1.LastEvents, ("The " + toString(domain_1)) + " layer is locked.", state_1.Layers, state_1.Issues, state_1.ActiveIssue, state_1.PendingDestructiveChange, state_1.PendingRecovery, state_1.Authoring);
        default:
            return unlockedUpdate(action, state_1);
    }
}

function unlockedUpdate(action_mut, state_mut) {
    unlockedUpdate:
    while (true) {
        const action = action_mut, state = state_mut;
        let bind$0040, bind$0040_1, bind$0040_2, bind$0040_3, option_3, option_6, matchValue_5, option_9, option_11, vertices_7, option_16, vertices_13, option_27, id_10, id_11, tupledArg_1, option_33, option_31, option_36, option_38, option_44, bind$0040_5, option_46, bind$0040_6, option_48, option_51, option_56, option_54;
        let matchResult, domain, value, name, camera, name_1, name_2, thumbnail, text_1, direction, kind, terrain, tool, terrain_1, query, delta, last, delta_1, columnDelta, rowDelta, delta_2, returnToBrowse, columnDelta_1, rowDelta_1, purpose, shape, columnDelta_2, fromOppositeOrigin, rowDelta_2, delta_3, first, last_1, purpose_12, purpose_13, vertices_12, id_2, purpose_14, geometry_3, columnDelta_3, rowDelta_3, columnDelta_4, index_6, rowDelta_4, address_2, address_3, address_4, size_4, address_5, address_6, columnDelta_6, extendPreview, rowDelta_6, columnDelta_7, rowDelta_7, delta_4, toggle, column_5, direction_1, row_5, columnDelta_8, extendPreview_1, rowDelta_8, column_9, direction_4, kind_5, row_9, column_10, direction_5, row_10, column_11, direction_6, row_11, column_12, direction_7, row_12, column_13, row_13, column_14, row_14, id_16, id_17, box, box_1, address_9, address_10, side_4, classId_4, size_6, maximum_1, remaining_2, controller_1, text_2, sourceDigest, direction_8;
        switch (action.tag) {
            case 67: {
                matchResult = 0;
                domain = action.fields[0];
                value = action.fields[1];
                break;
            }
            case 68: {
                matchResult = 1;
                break;
            }
            case 69: {
                matchResult = 2;
                break;
            }
            case 70: {
                matchResult = 3;
                name = action.fields[0];
                break;
            }
            case 71: {
                matchResult = 4;
                camera = action.fields[1];
                name_1 = action.fields[0];
                break;
            }
            case 72: {
                matchResult = 5;
                name_2 = action.fields[0];
                break;
            }
            case 73: {
                matchResult = 6;
                thumbnail = action.fields[0];
                break;
            }
            case 66: {
                matchResult = 7;
                break;
            }
            case 64: {
                matchResult = 8;
                break;
            }
            case 63: {
                matchResult = 9;
                break;
            }
            case 65: {
                matchResult = 10;
                break;
            }
            case 74: {
                matchResult = 11;
                text_1 = action.fields[0];
                break;
            }
            case 75: {
                matchResult = 12;
                break;
            }
            case 76: {
                matchResult = 13;
                break;
            }
            case 0: {
                switch (action.fields[0].tag) {
                    case 5: {
                        matchResult = 14;
                        direction = action.fields[0].fields[0];
                        kind = action.fields[0].fields[1];
                        break;
                    }
                    case 1: {
                        matchResult = 15;
                        terrain = action.fields[0].fields[0];
                        break;
                    }
                    case 2: {
                        matchResult = 16;
                        tool = action.fields[0].fields[0];
                        break;
                    }
                    case 3: {
                        matchResult = 17;
                        break;
                    }
                    default:
                        matchResult = 109;
                }
                break;
            }
            case 1: {
                matchResult = 18;
                terrain_1 = action.fields[0];
                break;
            }
            case 22: {
                matchResult = 19;
                query = action.fields[0];
                break;
            }
            case 23: {
                matchResult = 20;
                delta = action.fields[0];
                break;
            }
            case 25: {
                matchResult = 21;
                last = action.fields[0];
                break;
            }
            case 24: {
                matchResult = 22;
                delta_1 = action.fields[0];
                break;
            }
            case 26: {
                matchResult = 23;
                break;
            }
            case 27: {
                matchResult = 24;
                break;
            }
            case 28: {
                matchResult = 25;
                columnDelta = action.fields[0];
                rowDelta = action.fields[1];
                break;
            }
            case 29: {
                matchResult = 26;
                delta_2 = action.fields[0];
                break;
            }
            case 30: {
                matchResult = 27;
                returnToBrowse = action.fields[0];
                break;
            }
            case 31: {
                matchResult = 28;
                break;
            }
            case 32: {
                matchResult = 29;
                columnDelta_1 = action.fields[0];
                rowDelta_1 = action.fields[1];
                break;
            }
            case 33: {
                matchResult = 30;
                break;
            }
            case 34: {
                matchResult = 31;
                purpose = action.fields[0];
                break;
            }
            case 35: {
                matchResult = 32;
                shape = action.fields[0];
                break;
            }
            case 36: {
                matchResult = 33;
                break;
            }
            case 37: {
                matchResult = 34;
                break;
            }
            case 38: {
                matchResult = 35;
                break;
            }
            case 39: {
                matchResult = 36;
                break;
            }
            case 40: {
                matchResult = 37;
                break;
            }
            case 41: {
                matchResult = 38;
                break;
            }
            case 42: {
                matchResult = 39;
                break;
            }
            case 43: {
                matchResult = 40;
                columnDelta_2 = action.fields[0];
                fromOppositeOrigin = action.fields[2];
                rowDelta_2 = action.fields[1];
                break;
            }
            case 44: {
                matchResult = 41;
                delta_3 = action.fields[0];
                break;
            }
            case 45: {
                matchResult = 42;
                break;
            }
            case 46: {
                matchResult = 43;
                break;
            }
            case 47: {
                matchResult = 44;
                break;
            }
            case 48: {
                matchResult = 45;
                first = action.fields[1];
                last_1 = action.fields[2];
                purpose_12 = action.fields[0];
                break;
            }
            case 49: {
                matchResult = 46;
                purpose_13 = action.fields[0];
                vertices_12 = action.fields[1];
                break;
            }
            case 50: {
                matchResult = 47;
                id_2 = action.fields[0];
                break;
            }
            case 51: {
                matchResult = 48;
                purpose_14 = action.fields[0];
                break;
            }
            case 52: {
                matchResult = 49;
                geometry_3 = action.fields[0];
                break;
            }
            case 53: {
                matchResult = 50;
                columnDelta_3 = action.fields[0];
                rowDelta_3 = action.fields[1];
                break;
            }
            case 54: {
                matchResult = 51;
                columnDelta_4 = action.fields[1];
                index_6 = action.fields[0];
                rowDelta_4 = action.fields[2];
                break;
            }
            case 55: {
                matchResult = 52;
                break;
            }
            case 56: {
                matchResult = 53;
                address_2 = action.fields[0];
                break;
            }
            case 57: {
                matchResult = 54;
                address_3 = action.fields[0];
                break;
            }
            case 58: {
                matchResult = 55;
                address_4 = action.fields[0];
                break;
            }
            case 2: {
                matchResult = 56;
                size_4 = action.fields[0];
                break;
            }
            case 59: {
                if (inBounds(state.Map, action.fields[0])) {
                    matchResult = 57;
                    address_5 = action.fields[0];
                }
                else {
                    matchResult = 58;
                }
                break;
            }
            case 60: {
                if (inBounds(state.Map, action.fields[0])) {
                    matchResult = 59;
                    address_6 = action.fields[0];
                }
                else {
                    matchResult = 60;
                }
                break;
            }
            case 3: {
                matchResult = 61;
                columnDelta_6 = action.fields[0];
                extendPreview = action.fields[2];
                rowDelta_6 = action.fields[1];
                break;
            }
            case 4: {
                matchResult = 62;
                break;
            }
            case 5: {
                matchResult = 63;
                break;
            }
            case 6: {
                matchResult = 64;
                columnDelta_7 = action.fields[0];
                rowDelta_7 = action.fields[1];
                break;
            }
            case 7: {
                matchResult = 65;
                delta_4 = action.fields[0];
                break;
            }
            case 8: {
                matchResult = 66;
                toggle = action.fields[0];
                break;
            }
            case 9: {
                matchResult = 67;
                break;
            }
            case 10: {
                matchResult = 68;
                break;
            }
            case 12: {
                matchResult = 69;
                column_5 = action.fields[0];
                direction_1 = action.fields[2];
                row_5 = action.fields[1];
                break;
            }
            case 13: {
                matchResult = 70;
                columnDelta_8 = action.fields[0];
                extendPreview_1 = action.fields[2];
                rowDelta_8 = action.fields[1];
                break;
            }
            case 14: {
                matchResult = 71;
                break;
            }
            case 15: {
                matchResult = 72;
                break;
            }
            case 16: {
                matchResult = 73;
                break;
            }
            case 17: {
                matchResult = 74;
                column_9 = action.fields[0];
                direction_4 = action.fields[2];
                kind_5 = action.fields[3];
                row_9 = action.fields[1];
                break;
            }
            case 18: {
                matchResult = 75;
                column_10 = action.fields[0];
                direction_5 = action.fields[2];
                row_10 = action.fields[1];
                break;
            }
            case 19: {
                matchResult = 76;
                column_11 = action.fields[0];
                direction_6 = action.fields[2];
                row_11 = action.fields[1];
                break;
            }
            case 20: {
                matchResult = 76;
                column_11 = action.fields[0];
                direction_6 = action.fields[2];
                row_11 = action.fields[1];
                break;
            }
            case 21: {
                matchResult = 77;
                column_12 = action.fields[0];
                direction_7 = action.fields[2];
                row_12 = action.fields[1];
                break;
            }
            case 61: {
                if (state.Tool.tag === 4) {
                    matchResult = 78;
                    column_13 = action.fields[0];
                    row_13 = action.fields[1];
                }
                else if (isTerrainAuthoringTool(state.Tool)) {
                    matchResult = 79;
                    column_14 = action.fields[0];
                    row_14 = action.fields[1];
                }
                else {
                    matchResult = 109;
                }
                break;
            }
            case 77: {
                matchResult = 80;
                id_16 = action.fields[0];
                break;
            }
            case 78: {
                if (containsKey(action.fields[0], state.Map.Units)) {
                    matchResult = 81;
                    id_17 = action.fields[0];
                }
                else {
                    matchResult = 82;
                }
                break;
            }
            case 79: {
                matchResult = 83;
                box = action.fields[0];
                break;
            }
            case 11: {
                matchResult = 84;
                box_1 = action.fields[0];
                break;
            }
            case 80: {
                matchResult = 85;
                address_9 = action.fields[0];
                break;
            }
            case 81: {
                matchResult = 86;
                address_10 = action.fields[0];
                break;
            }
            case 82: {
                matchResult = 87;
                break;
            }
            case 83: {
                matchResult = 88;
                break;
            }
            case 84: {
                matchResult = 89;
                break;
            }
            case 87: {
                matchResult = 90;
                break;
            }
            case 88: {
                matchResult = 91;
                break;
            }
            case 89: {
                matchResult = 92;
                break;
            }
            case 90: {
                matchResult = 93;
                break;
            }
            case 96: {
                if (!equals(state.RevisionState, RevisionState_9.SimulatedRevision)) {
                    matchResult = 94;
                    side_4 = action.fields[0];
                }
                else {
                    matchResult = 109;
                }
                break;
            }
            case 97: {
                if (!equals(state.RevisionState, RevisionState_9.SimulatedRevision)) {
                    matchResult = 95;
                    classId_4 = action.fields[0];
                }
                else {
                    matchResult = 109;
                }
                break;
            }
            case 98: {
                if (!equals(state.RevisionState, RevisionState_9.SimulatedRevision)) {
                    matchResult = 96;
                    size_6 = action.fields[0];
                }
                else {
                    matchResult = 109;
                }
                break;
            }
            case 99: {
                if (!equals(state.RevisionState, RevisionState_9.SimulatedRevision)) {
                    matchResult = 97;
                    maximum_1 = action.fields[1];
                    remaining_2 = action.fields[0];
                }
                else {
                    matchResult = 109;
                }
                break;
            }
            case 100: {
                if (!equals(state.RevisionState, RevisionState_9.SimulatedRevision)) {
                    matchResult = 98;
                    controller_1 = action.fields[0];
                }
                else {
                    matchResult = 109;
                }
                break;
            }
            case 101: {
                if (!equals(state.RevisionState, RevisionState_9.SimulatedRevision)) {
                    matchResult = 99;
                    text_2 = action.fields[0];
                }
                else {
                    matchResult = 109;
                }
                break;
            }
            case 85: {
                matchResult = 100;
                break;
            }
            case 86: {
                matchResult = 101;
                break;
            }
            case 91: {
                matchResult = 102;
                break;
            }
            case 92: {
                matchResult = 103;
                break;
            }
            case 93: {
                matchResult = 104;
                sourceDigest = action.fields[0];
                break;
            }
            case 94: {
                matchResult = 105;
                break;
            }
            case 95: {
                matchResult = 106;
                break;
            }
            case 102: {
                matchResult = 107;
                direction_8 = action.fields[0];
                break;
            }
            case 104: {
                matchResult = 108;
                break;
            }
            default:
                matchResult = 109;
        }
        switch (matchResult) {
            case 0:
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, add(domain, value, state.Layers), state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            case 1:
                if (state.Issues.length === 0) {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, undefined, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, (defaultArg(state.ActiveIssue, -1) + 1) % state.Issues.length, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            case 2:
                if (state.Issues.length === 0) {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, undefined, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, ((defaultArg(state.ActiveIssue, 0) - 1) + state.Issues.length) % state.Issues.length, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            case 3:
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, (bind$0040 = state.Authoring, new MapAuthoringMetadata(isNullOrWhiteSpace(name) ? "Untitled battlefield" : name.trim(), bind$0040.SavedViews, bind$0040.RevisionIdentity, bind$0040.ThumbnailSvg)));
            case 4: {
                const normalized_1 = name_1.trim();
                if (isNullOrWhiteSpace(normalized_1)) {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Saved view names cannot be empty.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, (bind$0040_1 = state.Authoring, new MapAuthoringMetadata(bind$0040_1.Name, add(normalized_1, new SavedMapView(normalized_1, camera), state.Authoring.SavedViews), bind$0040_1.RevisionIdentity, bind$0040_1.ThumbnailSvg)));
                }
            }
            case 5:
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, (bind$0040_2 = state.Authoring, new MapAuthoringMetadata(bind$0040_2.Name, remove(name_2, state.Authoring.SavedViews), bind$0040_2.RevisionIdentity, bind$0040_2.ThumbnailSvg)));
            case 6:
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, (bind$0040_3 = state.Authoring, new MapAuthoringMetadata(bind$0040_3.Name, bind$0040_3.SavedViews, bind$0040_3.RevisionIdentity, thumbnail)));
            case 7:
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, undefined, state.PendingRecovery, state.Authoring);
            case 8:
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Clearing removes every terrain cell, edge, unit, and region. Confirm to continue.", state.Layers, state.Issues, state.ActiveIssue, PendingDestructiveChange_4.ClearPending, state.PendingRecovery, state.Authoring);
            case 9:
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Creating a new map replaces this draft with an empty 12 by 8 document. Confirm to continue.", state.Layers, state.Issues, state.ActiveIssue, new PendingDestructiveChange_4(/* NewMapPending */ 2, [12, 8, "Untitled battlefield"]), state.PendingRecovery, state.Authoring);
            case 10: {
                const matchValue_1 = state.PendingDestructiveChange;
                if (matchValue_1 == null) {
                    return state;
                }
                else {
                    switch (matchValue_1.tag) {
                        case 1:
                            return commit(new EditorCommand(/* ReplaceDocument */ 9, ["confirmed-clear", emptyMap(state.Map.Width, state.Map.Height)]), state);
                        case 2: {
                            const height = matchValue_1.fields[1] | 0;
                            const name_3 = matchValue_1.fields[2];
                            const width = matchValue_1.fields[0] | 0;
                            const replaced = commit(new EditorCommand(/* ReplaceDocument */ 9, ["confirmed-new-map", emptyMap(width, height)]), state);
                            const Authoring_4 = new MapAuthoringMetadata(name_3, empty_1({
                                Compare: (x, y) => (comparePrimitives(x, y) | 0),
                            }), replaced.Revision.Digest, undefined);
                            return new MapEditorState(replaced.Map, MapEditorTool.Select, replaced.TerrainSelection, replaced.BrushSize, replaced.TerrainCursor, replaced.KeyboardCursor, replaced.KeyboardObject, replaced.LastTerrainPaintTool, replaced.TerrainAnnouncement, replaced.EdgeCursor, replaced.EdgeAnnouncement, replaced.UnitPaletteSearch, replaced.UnitPaletteCursor, replaced.UnitPlacementCursor, replaced.UnitAnnouncement, replaced.RegionAnnouncement, replaced.RegionKeyboardMode, undefined, empty_2({
                                Compare: (x_1, y_1) => (comparePrimitives(x_1, y_1) | 0),
                            }), undefined, replaced.Gesture, replaced.Revision, replaced.RevisionState, replaced.SavedDigest, replaced.SimulatedDigest, replaced.RecoveredFromDigest, replaced.UndoHistory, replaced.RedoHistory, replaced.HistoryBytes, replaced.Clipboard, replaced.Tick, replaced.IsRunning, replaced.LastEvents, replaced.Validation, replaced.Layers, replaced.Issues, replaced.ActiveIssue, replaced.PendingDestructiveChange, replaced.PendingRecovery, Authoring_4);
                        }
                        case 3: {
                            const identifiers = matchValue_1.fields[0];
                            const next = commit(new EditorCommand(/* RemoveUnits */ 4, [identifiers]), state);
                            return new MapEditorState(next.Map, next.Tool, next.TerrainSelection, next.BrushSize, next.TerrainCursor, next.KeyboardCursor, next.KeyboardObject, next.LastTerrainPaintTool, next.TerrainAnnouncement, next.EdgeCursor, next.EdgeAnnouncement, next.UnitPaletteSearch, next.UnitPaletteCursor, next.UnitPlacementCursor, int32ToString(identifiers.length) + ((identifiers.length === 1) ? " unit deleted." : " units deleted."), next.RegionAnnouncement, next.RegionKeyboardMode, next.SelectedUnit, next.SelectedUnits, next.SelectedRegion, next.Gesture, next.Revision, next.RevisionState, next.SavedDigest, next.SimulatedDigest, next.RecoveredFromDigest, next.UndoHistory, next.RedoHistory, next.HistoryBytes, next.Clipboard, next.Tick, next.IsRunning, next.LastEvents, next.Validation, next.Layers, next.Issues, next.ActiveIssue, next.PendingDestructiveChange, next.PendingRecovery, next.Authoring);
                        }
                        default: {
                            const preview = matchValue_1.fields[0];
                            return commit(new EditorCommand(/* ReplaceDocument */ 9, ["confirmed-resize", resizedDocument(preview, state.Map)]), state);
                        }
                    }
                }
            }
            case 11: {
                const matchValue_2 = tryImport(text_1);
                if (matchValue_2.tag === 1) {
                    return state;
                }
                else {
                    const map = matchValue_2.fields[0];
                    const digest = revisionDigest(map);
                    if (digest === state.Revision.Digest) {
                        return state;
                    }
                    else {
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, new CrashRecoveryDraft(digest, map), state.Authoring);
                    }
                }
            }
            case 12: {
                const matchValue_3 = state.PendingRecovery;
                if (matchValue_3 != null) {
                    const draft = matchValue_3;
                    const recovered = commit(new EditorCommand(/* ReplaceDocument */ 9, ["crash-recovery", draft.Map]), new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, undefined, state.Authoring));
                    return new MapEditorState(recovered.Map, recovered.Tool, recovered.TerrainSelection, recovered.BrushSize, recovered.TerrainCursor, recovered.KeyboardCursor, recovered.KeyboardObject, recovered.LastTerrainPaintTool, recovered.TerrainAnnouncement, recovered.EdgeCursor, recovered.EdgeAnnouncement, recovered.UnitPaletteSearch, recovered.UnitPaletteCursor, recovered.UnitPlacementCursor, recovered.UnitAnnouncement, recovered.RegionAnnouncement, recovered.RegionKeyboardMode, recovered.SelectedUnit, recovered.SelectedUnits, recovered.SelectedRegion, recovered.Gesture, recovered.Revision, RevisionState_9.RecoveredRevision, recovered.SavedDigest, recovered.SimulatedDigest, draft.SourceDigest, recovered.UndoHistory, recovered.RedoHistory, recovered.HistoryBytes, recovered.Clipboard, recovered.Tick, recovered.IsRunning, recovered.LastEvents, recovered.Validation, recovered.Layers, recovered.Issues, recovered.ActiveIssue, recovered.PendingDestructiveChange, recovered.PendingRecovery, recovered.Authoring);
                }
                else {
                    return state;
                }
            }
            case 13:
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, undefined, state.Authoring);
            case 14: {
                const patternInput = state.EdgeCursor;
                return new MapEditorState(state.Map, new MapEditorTool(/* Edge */ 5, [direction, kind]), state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, [patternInput[0], patternInput[1], direction], ((edgeKindName(kind) + " ") + edgeDirectionName(direction)) + " edge tool selected.", state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            }
            case 15:
                return new MapEditorState(state.Map, new MapEditorTool(/* Paint */ 1, [terrain]), terrain, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, ("Pencil selected with " + terrainName(terrain)) + " terrain.", state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            case 16:
                return new MapEditorState(state.Map, new MapEditorTool(/* Terrain */ 2, [tool]), state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, (tool.tag === 1) ? tool : ((tool.tag === 2) ? tool : ((tool.tag === 3) ? tool : ((tool.tag === 5) ? tool : ((tool.tag === 4) ? state.LastTerrainPaintTool : tool)))), ((tool.tag === 1) ? "Rectangle" : ((tool.tag === 2) ? "Line" : ((tool.tag === 3) ? "Flood fill" : ((tool.tag === 4) ? "Eyedropper" : ((tool.tag === 5) ? "Erase" : "Pencil"))))) + " terrain tool selected.", state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            case 17: {
                const cursor = paletteCursorFor(state.UnitPaletteCursor.PresetId, state.UnitPaletteCursor.ResultIndex, state);
                return new MapEditorState(state.Map, MapEditorTool.UnitBrowse, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, cursor, state.UnitPlacementCursor, (cursor.PresetId != null) ? "Unit preset browser ready." : "No unit presets match the current filter.", state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            }
            case 18:
                return new MapEditorState(state.Map, state.Tool, terrain_1, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, terrainName(terrain_1) + " terrain selected.", state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            case 19: {
                const filtered = new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, query, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                const cursor_1 = paletteCursorFor(state.UnitPaletteCursor.PresetId, 0, filtered);
                const count = length_1(searchCanonicalUnitPresets(query)) | 0;
                return new MapEditorState(filtered.Map, filtered.Tool, filtered.TerrainSelection, filtered.BrushSize, filtered.TerrainCursor, filtered.KeyboardCursor, filtered.KeyboardObject, filtered.LastTerrainPaintTool, filtered.TerrainAnnouncement, filtered.EdgeCursor, filtered.EdgeAnnouncement, query, cursor_1, filtered.UnitPlacementCursor, ((int32ToString(count) + " canonical unit ") + ((count === 1) ? "preset" : "presets")) + " shown.", filtered.RegionAnnouncement, filtered.RegionKeyboardMode, filtered.SelectedUnit, filtered.SelectedUnits, filtered.SelectedRegion, filtered.Gesture, filtered.Revision, filtered.RevisionState, filtered.SavedDigest, filtered.SimulatedDigest, filtered.RecoveredFromDigest, filtered.UndoHistory, filtered.RedoHistory, filtered.HistoryBytes, filtered.Clipboard, filtered.Tick, filtered.IsRunning, filtered.LastEvents, filtered.Validation, filtered.Layers, filtered.Issues, filtered.ActiveIssue, filtered.PendingDestructiveChange, filtered.PendingRecovery, filtered.Authoring);
            }
            case 20: {
                const visible = visibleUnitPresets(state);
                if (isEmpty(visible)) {
                    return state;
                }
                else {
                    const index = ((((paletteCursorFor(state.UnitPaletteCursor.PresetId, state.UnitPaletteCursor.ResultIndex, state).ResultIndex + delta) % length_1(visible)) + length_1(visible)) % length_1(visible)) | 0;
                    return new MapEditorState(state.Map, MapEditorTool.UnitBrowse, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, paletteCursorFor(undefined, index, state), state.UnitPlacementCursor, ((((item_1(index, visible).Name + ", preset ") + int32ToString(index + 1)) + " of ") + int32ToString(length_1(visible))) + ".", state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 21: {
                const visible_1 = visibleUnitPresets(state);
                if (isEmpty(visible_1)) {
                    return state;
                }
                else {
                    const index_1 = (last ? (length_1(visible_1) - 1) : 0) | 0;
                    return new MapEditorState(state.Map, MapEditorTool.UnitBrowse, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, paletteCursorFor(undefined, index_1, state), state.UnitPlacementCursor, item_1(index_1, visible_1).Name + " selected.", state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 22: {
                const visible_2 = visibleUnitPresets(state);
                const factions = List_distinct(map_9((_arg) => _arg.Faction, visible_2), {
                    Equals: (x_2, y_2) => (x_2 === y_2),
                    GetHashCode: (x_2) => (stringHash(x_2) | 0),
                });
                if (isEmpty(factions)) {
                    return state;
                }
                else {
                    const factionIndex = ((((paletteCursorFor(state.UnitPaletteCursor.PresetId, state.UnitPaletteCursor.ResultIndex, state).FactionIndex + delta_1) % length_1(factions)) + length_1(factions)) % length_1(factions)) | 0;
                    return new MapEditorState(state.Map, MapEditorTool.UnitBrowse, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, paletteCursorFor(undefined, findIndex((preset) => (preset.Faction === item_1(factionIndex, factions)), visible_2), state), state.UnitPlacementCursor, item_1(factionIndex, factions) + " faction presets.", state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 23: {
                const matchValue_4 = selectedUnitPalettePreset(state);
                if (matchValue_4 != null) {
                    const preset_1 = matchValue_4;
                    const armed = new MapEditorState(state.Map, new MapEditorTool(/* Place */ 4, [preset_1.Side, preset_1.ClassId, preset_1.FootprintSize]), state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    return new MapEditorState(armed.Map, armed.Tool, armed.TerrainSelection, armed.BrushSize, armed.TerrainCursor, armed.KeyboardCursor, armed.KeyboardObject, armed.LastTerrainPaintTool, armed.TerrainAnnouncement, armed.EdgeCursor, armed.EdgeAnnouncement, armed.UnitPaletteSearch, armed.UnitPaletteCursor, armed.UnitPlacementCursor, ((preset_1.Name + " armed at the placement cursor — ") + defaultArg((option_3 = unitPlacementIssue(armed), (option_3 != null) ? ("invalid: " + option_3) : undefined), "valid")) + ".", armed.RegionAnnouncement, armed.RegionKeyboardMode, armed.SelectedUnit, armed.SelectedUnits, armed.SelectedRegion, armed.Gesture, armed.Revision, armed.RevisionState, armed.SavedDigest, armed.SimulatedDigest, armed.RecoveredFromDigest, armed.UndoHistory, armed.RedoHistory, armed.HistoryBytes, armed.Clipboard, armed.Tick, armed.IsRunning, armed.LastEvents, armed.Validation, armed.Layers, armed.Issues, armed.ActiveIssue, armed.PendingDestructiveChange, armed.PendingRecovery, armed.Authoring);
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, "No unit preset is available.", state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "No visible unit preset can be armed.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 24: {
                action_mut = (new MapEditorAction(/* ChooseTool */ 0, [MapEditorTool.UnitBrowse]));
                state_mut = state;
                continue unlockedUpdate;
            }
            case 25: {
                const moved = new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, new EditorCellAddress(max(0, min(state.Map.Width - 1, state.UnitPlacementCursor.CellColumn + columnDelta)), max(0, min(state.Map.Height - 1, state.UnitPlacementCursor.CellRow + rowDelta))), state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                return new MapEditorState(moved.Map, moved.Tool, moved.TerrainSelection, moved.BrushSize, moved.TerrainCursor, moved.KeyboardCursor, moved.KeyboardObject, moved.LastTerrainPaintTool, moved.TerrainAnnouncement, moved.EdgeCursor, moved.EdgeAnnouncement, moved.UnitPaletteSearch, moved.UnitPaletteCursor, moved.UnitPlacementCursor, defaultArg((option_6 = unitPlacementIssue(moved), (option_6 != null) ? (("Placement preview invalid: " + option_6) + ".") : undefined), "Placement preview valid."), moved.RegionAnnouncement, moved.RegionKeyboardMode, moved.SelectedUnit, moved.SelectedUnits, moved.SelectedRegion, moved.Gesture, moved.Revision, moved.RevisionState, moved.SavedDigest, moved.SimulatedDigest, moved.RecoveredFromDigest, moved.UndoHistory, moved.RedoHistory, moved.HistoryBytes, moved.Clipboard, moved.Tick, moved.IsRunning, moved.LastEvents, moved.Validation, moved.Layers, moved.Issues, moved.ActiveIssue, moved.PendingDestructiveChange, moved.PendingRecovery, moved.Authoring);
            }
            case 26: {
                const visible_3 = visibleUnitPresets(state);
                if (isEmpty(visible_3)) {
                    return state;
                }
                else {
                    const index_3 = ((((paletteCursorFor((matchValue_5 = state.Tool, (matchValue_5.tag === 4) ? ((option_9 = tryFind((preset_2) => {
                        if (equals(preset_2.Side, matchValue_5.fields[0]) && (preset_2.ClassId === matchValue_5.fields[1])) {
                            return preset_2.FootprintSize === matchValue_5.fields[2];
                        }
                        else {
                            return false;
                        }
                    }, visible_3), (option_9 != null) ? option_9.Id : undefined)) : state.UnitPaletteCursor.PresetId), 0, state).ResultIndex + delta_2) % length_1(visible_3)) + length_1(visible_3)) % length_1(visible_3)) | 0;
                    const preset_3 = item_1(index_3, visible_3);
                    const next_1 = new MapEditorState(state.Map, new MapEditorTool(/* Place */ 4, [preset_3.Side, preset_3.ClassId, preset_3.FootprintSize]), state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, paletteCursorFor(undefined, index_3, state), state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    return new MapEditorState(next_1.Map, next_1.Tool, next_1.TerrainSelection, next_1.BrushSize, next_1.TerrainCursor, next_1.KeyboardCursor, next_1.KeyboardObject, next_1.LastTerrainPaintTool, next_1.TerrainAnnouncement, next_1.EdgeCursor, next_1.EdgeAnnouncement, next_1.UnitPaletteSearch, next_1.UnitPaletteCursor, next_1.UnitPlacementCursor, ((preset_3.Name + " armed — ") + defaultArg((option_11 = unitPlacementIssue(next_1), (option_11 != null) ? ("invalid: " + option_11) : undefined), "valid")) + ".", next_1.RegionAnnouncement, next_1.RegionKeyboardMode, next_1.SelectedUnit, next_1.SelectedUnits, next_1.SelectedRegion, next_1.Gesture, next_1.Revision, next_1.RevisionState, next_1.SavedDigest, next_1.SimulatedDigest, next_1.RecoveredFromDigest, next_1.UndoHistory, next_1.RedoHistory, next_1.HistoryBytes, next_1.Clipboard, next_1.Tick, next_1.IsRunning, next_1.LastEvents, next_1.Validation, next_1.Layers, next_1.Issues, next_1.ActiveIssue, next_1.PendingDestructiveChange, next_1.PendingRecovery, next_1.Authoring);
                }
            }
            case 27: {
                const matchValue_6 = state.Tool;
                if (matchValue_6.tag === 4) {
                    const size_2 = matchValue_6.fields[2] | 0;
                    const side_2 = matchValue_6.fields[0];
                    const classId_2 = matchValue_6.fields[1];
                    const matchValue_7 = unitPlacementIssue(state);
                    if (matchValue_7 == null) {
                        const unit = placementUnit(side_2, classId_2, size_2, state.UnitPlacementCursor, state);
                        const next_2 = commit(new EditorCommand(/* AddUnits */ 2, [[unit]]), state);
                        let next_3;
                        const SelectedUnits_1 = singleton_2(unit.Id, {
                            Compare: (x_3, y_3) => (comparePrimitives(x_3, y_3) | 0),
                        });
                        next_3 = (new MapEditorState(next_2.Map, next_2.Tool, next_2.TerrainSelection, next_2.BrushSize, next_2.TerrainCursor, next_2.KeyboardCursor, next_2.KeyboardObject, next_2.LastTerrainPaintTool, next_2.TerrainAnnouncement, next_2.EdgeCursor, next_2.EdgeAnnouncement, next_2.UnitPaletteSearch, next_2.UnitPaletteCursor, next_2.UnitPlacementCursor, ((((("Placed " + unit.ClassId) + " as unit ") + int32ToString(unit.Id)) + " in revision ") + int64ToString(next_2.Revision.Number)) + ".", next_2.RegionAnnouncement, next_2.RegionKeyboardMode, unit.Id, SelectedUnits_1, next_2.SelectedRegion, next_2.Gesture, next_2.Revision, next_2.RevisionState, next_2.SavedDigest, next_2.SimulatedDigest, next_2.RecoveredFromDigest, next_2.UndoHistory, next_2.RedoHistory, next_2.HistoryBytes, next_2.Clipboard, next_2.Tick, next_2.IsRunning, next_2.LastEvents, next_2.Validation, next_2.Layers, next_2.Issues, next_2.ActiveIssue, next_2.PendingDestructiveChange, next_2.PendingRecovery, next_2.Authoring));
                        if (returnToBrowse) {
                            action_mut = (new MapEditorAction(/* ChooseTool */ 0, [MapEditorTool.UnitBrowse]));
                            state_mut = next_3;
                            continue unlockedUpdate;
                        }
                        else {
                            return new MapEditorState(next_3.Map, new MapEditorTool(/* Place */ 4, [side_2, classId_2, size_2]), next_3.TerrainSelection, next_3.BrushSize, next_3.TerrainCursor, next_3.KeyboardCursor, next_3.KeyboardObject, next_3.LastTerrainPaintTool, next_3.TerrainAnnouncement, next_3.EdgeCursor, next_3.EdgeAnnouncement, next_3.UnitPaletteSearch, next_3.UnitPaletteCursor, next_3.UnitPlacementCursor, next_3.UnitAnnouncement, next_3.RegionAnnouncement, next_3.RegionKeyboardMode, next_3.SelectedUnit, next_3.SelectedUnits, next_3.SelectedRegion, next_3.Gesture, next_3.Revision, next_3.RevisionState, next_3.SavedDigest, next_3.SimulatedDigest, next_3.RecoveredFromDigest, next_3.UndoHistory, next_3.RedoHistory, next_3.HistoryBytes, next_3.Clipboard, next_3.Tick, next_3.IsRunning, next_3.LastEvents, next_3.Validation, next_3.Layers, next_3.Issues, next_3.ActiveIssue, next_3.PendingDestructiveChange, next_3.PendingRecovery, next_3.Authoring);
                        }
                    }
                    else {
                        const reason_3 = matchValue_7;
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, ("Placement rejected: " + reason_3) + ".", state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, ("Invalid placement: " + reason_3) + ".", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    }
                }
                else {
                    return state;
                }
            }
            case 28: {
                const matchValue_8 = state.Gesture;
                if (matchValue_8.tag === 4) {
                    const original = matchValue_8.fields[2];
                    const anchor = matchValue_8.fields[0];
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, "Movement preview reset to the original positions.", state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, new EditorGesture(/* UnitMoveGesture */ 4, [anchor, anchor, original, new EditorCommand(/* UpdateUnits */ 3, [original])]), state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return state;
                }
            }
            case 29: {
                const current_3 = state.KeyboardCursor.Cell;
                const cell = new EditorCellAddress(max(0, min(state.Map.Width - 1, current_3.CellColumn + columnDelta_1)), max(0, min(state.Map.Height - 1, current_3.CellRow + rowDelta_1)));
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, cell, new EditorKeyboardCursor(cell, 0), state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, ((("Region cursor at column " + int32ToString(cell.CellColumn + 1)) + ", row ") + int32ToString(cell.CellRow + 1)) + ".", state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            }
            case 30:
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, "Choose region purpose: Objective, Blue deployment, or Red deployment.", new RegionKeyboardMode_26(/* RegionPurposeSelection */ 1, [false, RegionPurpose.ObjectiveRegion]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            case 31: {
                const matchValue_9 = state.RegionKeyboardMode;
                if (matchValue_9.tag === 1) {
                    if (matchValue_9.fields[0]) {
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, regionPurposeName(purpose) + " highlighted. Press Enter to apply.", new RegionKeyboardMode_26(/* RegionPurposeSelection */ 1, [true, purpose]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    }
                    else {
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, regionPurposeName(purpose) + " selected. Choose rectangle or polygon.", new RegionKeyboardMode_26(/* RegionShapeSelection */ 2, [purpose]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    }
                }
                else {
                    return state;
                }
            }
            case 32: {
                const matchValue_10 = state.RegionKeyboardMode;
                if (matchValue_10.tag === 2) {
                    const purpose_1 = matchValue_10.fields[0];
                    const RegionKeyboardMode_3 = (shape.tag === 1) ? (new RegionKeyboardMode_26(/* RegionPolygonConstruction */ 4, [purpose_1, []])) : (new RegionKeyboardMode_26(/* RegionRectangleConstruction */ 3, [purpose_1, undefined]));
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, (shape.tag === 1) ? "Polygon geometry selected. Press Enter to add the first vertex." : "Rectangle geometry selected. Press Enter to set the first corner.", RegionKeyboardMode_3, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return state;
                }
            }
            case 33: {
                const cursor_6 = state.KeyboardCursor.Cell;
                const matchValue_11 = state.RegionKeyboardMode;
                switch (matchValue_11.tag) {
                    case 0: {
                        action_mut = (new MapEditorAction(/* SelectEditorRegion */ 50, [tryPick((tupledArg) => {
                            if (regionContains(cursor_6, tupledArg[1])) {
                                return tupledArg[0];
                            }
                            else {
                                return undefined;
                            }
                        }, sortBy((tuple) => (tuple[0] | 0), toList_1(state.Map.Regions), {
                            Compare: (x_4, y_4) => (comparePrimitives(x_4, y_4) | 0),
                        }))]));
                        state_mut = state;
                        continue unlockedUpdate;
                    }
                    case 3:
                        if (matchValue_11.fields[1] != null) {
                            const anchor_1 = matchValue_11.fields[1];
                            const next_4 = unlockedUpdate(new MapEditorAction(/* CreateRectangleRegion */ 48, [matchValue_11.fields[0], anchor_1, cursor_6]), state);
                            if (equals_1(next_4.Revision.Number, state.Revision.Number)) {
                                return next_4;
                            }
                            else {
                                return new MapEditorState(next_4.Map, next_4.Tool, next_4.TerrainSelection, next_4.BrushSize, next_4.TerrainCursor, next_4.KeyboardCursor, next_4.KeyboardObject, next_4.LastTerrainPaintTool, next_4.TerrainAnnouncement, next_4.EdgeCursor, next_4.EdgeAnnouncement, next_4.UnitPaletteSearch, next_4.UnitPaletteCursor, next_4.UnitPlacementCursor, next_4.UnitAnnouncement, next_4.RegionAnnouncement, RegionKeyboardMode_26.RegionIdle, next_4.SelectedUnit, next_4.SelectedUnits, next_4.SelectedRegion, next_4.Gesture, next_4.Revision, next_4.RevisionState, next_4.SavedDigest, next_4.SimulatedDigest, next_4.RecoveredFromDigest, next_4.UndoHistory, next_4.RedoHistory, next_4.HistoryBytes, next_4.Clipboard, next_4.Tick, next_4.IsRunning, next_4.LastEvents, next_4.Validation, next_4.Layers, next_4.Issues, next_4.ActiveIssue, next_4.PendingDestructiveChange, next_4.PendingRecovery, next_4.Authoring);
                            }
                        }
                        else {
                            return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, "Rectangle first corner set. Move to the opposite corner and press Enter.", new RegionKeyboardMode_26(/* RegionRectangleConstruction */ 3, [matchValue_11.fields[0], cursor_6]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                        }
                    case 4:
                        if (contains_2(cursor_6, matchValue_11.fields[1], {
                            Equals: equals,
                            GetHashCode: (x_5) => (safeHash(x_5) | 0),
                        })) {
                            return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, "Duplicate polygon vertex ignored.", state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Polygon vertices must be unique.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                        }
                        else {
                            const vertices_1 = append_2(matchValue_11.fields[1], [cursor_6]);
                            return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, int32ToString(vertices_1.length) + ((vertices_1.length === 1) ? " polygon vertex staged." : " polygon vertices staged."), new RegionKeyboardMode_26(/* RegionPolygonConstruction */ 4, [matchValue_11.fields[0], vertices_1]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                        }
                    default:
                        return state;
                }
            }
            case 34: {
                const matchValue_12 = state.RegionKeyboardMode;
                if (matchValue_12.tag === 4) {
                    if (matchValue_12.fields[1].length >= 3) {
                        const next_5 = unlockedUpdate(new MapEditorAction(/* CreatePolygonRegion */ 49, [matchValue_12.fields[0], matchValue_12.fields[1]]), state);
                        if (equals_1(next_5.Revision.Number, state.Revision.Number)) {
                            return next_5;
                        }
                        else {
                            return new MapEditorState(next_5.Map, next_5.Tool, next_5.TerrainSelection, next_5.BrushSize, next_5.TerrainCursor, next_5.KeyboardCursor, next_5.KeyboardObject, next_5.LastTerrainPaintTool, next_5.TerrainAnnouncement, next_5.EdgeCursor, next_5.EdgeAnnouncement, next_5.UnitPaletteSearch, next_5.UnitPaletteCursor, next_5.UnitPlacementCursor, next_5.UnitAnnouncement, next_5.RegionAnnouncement, RegionKeyboardMode_26.RegionIdle, next_5.SelectedUnit, next_5.SelectedUnits, next_5.SelectedRegion, next_5.Gesture, next_5.Revision, next_5.RevisionState, next_5.SavedDigest, next_5.SimulatedDigest, next_5.RecoveredFromDigest, next_5.UndoHistory, next_5.RedoHistory, next_5.HistoryBytes, next_5.Clipboard, next_5.Tick, next_5.IsRunning, next_5.LastEvents, next_5.Validation, next_5.Layers, next_5.Issues, next_5.ActiveIssue, next_5.PendingDestructiveChange, next_5.PendingRecovery, next_5.Authoring);
                        }
                    }
                    else {
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, "Add at least three vertices before closing the polygon.", state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Polygon regions require at least three unique vertices.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    }
                }
                else {
                    return state;
                }
            }
            case 35: {
                const matchValue_13 = state.RegionKeyboardMode;
                let matchResult_1, purpose_8, purpose_9, vertices_5;
                switch (matchValue_13.tag) {
                    case 3: {
                        if (matchValue_13.fields[1] != null) {
                            matchResult_1 = 0;
                            purpose_8 = matchValue_13.fields[0];
                        }
                        else {
                            matchResult_1 = 2;
                        }
                        break;
                    }
                    case 4: {
                        if (!(matchValue_13.fields[1].length === 0)) {
                            matchResult_1 = 1;
                            purpose_9 = matchValue_13.fields[0];
                            vertices_5 = matchValue_13.fields[1];
                        }
                        else {
                            matchResult_1 = 2;
                        }
                        break;
                    }
                    default:
                        matchResult_1 = 2;
                }
                switch (matchResult_1) {
                    case 0:
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, "Rectangle first corner cleared.", new RegionKeyboardMode_26(/* RegionRectangleConstruction */ 3, [purpose_8, undefined]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    case 1: {
                        const vertices_6 = take(vertices_5.length - 1, vertices_5);
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, int32ToString(vertices_6.length) + " polygon vertices remain.", new RegionKeyboardMode_26(/* RegionPolygonConstruction */ 4, [purpose_9, vertices_6]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    }
                    default:
                        return state;
                }
            }
            case 36: {
                const matchValue_14 = selectedRegion(state);
                if (matchValue_14 == null) {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Select a region first.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    const region_1 = matchValue_14;
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, "Region movement preview started.", new RegionKeyboardMode_26(/* RegionMovePreview */ 5, [region_1.Geometry, region_1.Geometry]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 37: {
                const matchValue_15 = selectedRegion(state);
                if (matchValue_15 == null) {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Select a region first.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else if (matchValue_15.Geometry.tag === 0) {
                    const geometry = matchValue_15.Geometry;
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, "Rectangle resize preview started.", new RegionKeyboardMode_26(/* RegionResizePreview */ 6, [geometry, geometry]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Only rectangle regions can be resized.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 38: {
                const matchValue_16 = selectedRegion(state);
                let matchResult_2, geometry_2, vertices_8;
                if (matchValue_16 == null) {
                    matchResult_2 = 2;
                }
                else if (matchValue_16.Geometry.tag === 1) {
                    if ((vertices_7 = matchValue_16.Geometry.fields[0], (matchValue_16.Geometry, !(vertices_7.length === 0)))) {
                        matchResult_2 = 0;
                        geometry_2 = matchValue_16.Geometry;
                        vertices_8 = matchValue_16.Geometry.fields[0];
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
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, ("Polygon vertex 1 of " + int32ToString(vertices_8.length)) + " active.", new RegionKeyboardMode_26(/* RegionVertexPreview */ 7, [geometry_2, geometry_2, 0]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    case 1:
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Only polygon regions have editable vertices.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    default:
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Select a region first.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 39: {
                const matchValue_17 = selectedRegion(state);
                if (matchValue_17 == null) {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Select a region first.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    const region_2 = matchValue_17;
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, regionPurposeName(region_2.Purpose) + " purpose highlighted.", new RegionKeyboardMode_26(/* RegionPurposeSelection */ 1, [true, region_2.Purpose]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 40: {
                const matchValue_18 = state.RegionKeyboardMode;
                let matchResult_3, original_1, preview_1, column_3, height_1, original_2, row_3, width_1, activeIndex, original_3, vertices_9;
                switch (matchValue_18.tag) {
                    case 5: {
                        matchResult_3 = 0;
                        original_1 = matchValue_18.fields[0];
                        preview_1 = matchValue_18.fields[1];
                        break;
                    }
                    case 6: {
                        if (matchValue_18.fields[1].tag === 0) {
                            matchResult_3 = 1;
                            column_3 = matchValue_18.fields[1].fields[0];
                            height_1 = matchValue_18.fields[1].fields[3];
                            original_2 = matchValue_18.fields[0];
                            row_3 = matchValue_18.fields[1].fields[1];
                            width_1 = matchValue_18.fields[1].fields[2];
                        }
                        else {
                            matchResult_3 = 3;
                        }
                        break;
                    }
                    case 7: {
                        if (matchValue_18.fields[1].tag === 1) {
                            matchResult_3 = 2;
                            activeIndex = matchValue_18.fields[2];
                            original_3 = matchValue_18.fields[0];
                            vertices_9 = matchValue_18.fields[1].fields[0];
                        }
                        else {
                            matchResult_3 = 3;
                        }
                        break;
                    }
                    default:
                        matchResult_3 = 3;
                }
                switch (matchResult_3) {
                    case 0:
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, "Region movement preview updated.", new RegionKeyboardMode_26(/* RegionMovePreview */ 5, [original_1, translatedRegionPreview(state.Map, columnDelta_2, rowDelta_2, preview_1)]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    case 1: {
                        let patternInput_1;
                        if (fromOppositeOrigin) {
                            const columnChange = max(op_UnaryNegation_Int32(column_3), min(width_1 - 1, columnDelta_2)) | 0;
                            const rowChange = max(op_UnaryNegation_Int32(row_3), min(height_1 - 1, rowDelta_2)) | 0;
                            patternInput_1 = [column_3 + columnChange, row_3 + rowChange, width_1 - columnChange, height_1 - rowChange];
                        }
                        else {
                            patternInput_1 = [column_3, row_3, max(1, min(state.Map.Width - column_3, width_1 + columnDelta_2)), max(1, min(state.Map.Height - row_3, height_1 + rowDelta_2))];
                        }
                        const nextWidth = patternInput_1[2] | 0;
                        const nextHeight = patternInput_1[3] | 0;
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, ((("Rectangle preview " + int32ToString(nextWidth)) + " by ") + int32ToString(nextHeight)) + ".", new RegionKeyboardMode_26(/* RegionResizePreview */ 6, [original_2, new RegionGeometry(/* RegionRectangle */ 0, [patternInput_1[0], patternInput_1[1], nextWidth, nextHeight])]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    }
                    case 2: {
                        const moved_2 = copy(vertices_9);
                        const vertex = item_2(activeIndex, moved_2);
                        setItem(moved_2, activeIndex, new EditorCellAddress(max(0, min(state.Map.Width - 1, vertex.CellColumn + columnDelta_2)), max(0, min(state.Map.Height - 1, vertex.CellRow + rowDelta_2))));
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, "Polygon vertex preview updated.", new RegionKeyboardMode_26(/* RegionVertexPreview */ 7, [original_3, new RegionGeometry(/* RegionPolygon */ 1, [moved_2]), activeIndex]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    }
                    default:
                        return state;
                }
            }
            case 41: {
                const matchValue_19 = state.RegionKeyboardMode;
                let matchResult_4, activeIndex_1, original_4, preview_3, vertices_10;
                if (matchValue_19.tag === 7) {
                    if (matchValue_19.fields[1].tag === 1) {
                        matchResult_4 = 0;
                        activeIndex_1 = matchValue_19.fields[2];
                        original_4 = matchValue_19.fields[0];
                        preview_3 = matchValue_19.fields[1];
                        vertices_10 = matchValue_19.fields[1].fields[0];
                    }
                    else {
                        matchResult_4 = 1;
                    }
                }
                else {
                    matchResult_4 = 1;
                }
                switch (matchResult_4) {
                    case 0: {
                        const index_4 = ((activeIndex_1 + delta_3) % vertices_10.length) | 0;
                        const index_5 = ((index_4 < 0) ? (index_4 + vertices_10.length) : index_4) | 0;
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, ((("Polygon vertex " + int32ToString(index_5 + 1)) + " of ") + int32ToString(vertices_10.length)) + " active.", new RegionKeyboardMode_26(/* RegionVertexPreview */ 7, [original_4, preview_3, index_5]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    }
                    default:
                        return state;
                }
            }
            case 42: {
                const matchValue_20 = state.RegionKeyboardMode;
                let matchResult_5, original_5, original_6, activeIndex_2, original_7, vertices_11;
                switch (matchValue_20.tag) {
                    case 5: {
                        matchResult_5 = 0;
                        original_5 = matchValue_20.fields[0];
                        break;
                    }
                    case 6: {
                        matchResult_5 = 1;
                        original_6 = matchValue_20.fields[0];
                        break;
                    }
                    case 7: {
                        if (matchValue_20.fields[1].tag === 1) {
                            matchResult_5 = 2;
                            activeIndex_2 = matchValue_20.fields[2];
                            original_7 = matchValue_20.fields[0];
                            vertices_11 = matchValue_20.fields[1].fields[0];
                        }
                        else {
                            matchResult_5 = 3;
                        }
                        break;
                    }
                    default:
                        matchResult_5 = 3;
                }
                switch (matchResult_5) {
                    case 0:
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, "Region movement preview reset.", new RegionKeyboardMode_26(/* RegionMovePreview */ 5, [original_5, original_5]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    case 1:
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, "Rectangle resize preview reset.", new RegionKeyboardMode_26(/* RegionResizePreview */ 6, [original_6, original_6]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    case 2:
                        if (original_7.tag === 1) {
                            const reset = copy(vertices_11);
                            setItem(reset, activeIndex_2, item_2(activeIndex_2, original_7.fields[0]));
                            return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, "Active polygon vertex reset.", new RegionKeyboardMode_26(/* RegionVertexPreview */ 7, [original_7, new RegionGeometry(/* RegionPolygon */ 1, [reset]), activeIndex_2]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                        }
                        else {
                            return state;
                        }
                    default:
                        return state;
                }
            }
            case 43: {
                const matchValue_21 = state.RegionKeyboardMode;
                let matchResult_6, preview_4, highlighted;
                switch (matchValue_21.tag) {
                    case 5: {
                        matchResult_6 = 0;
                        preview_4 = matchValue_21.fields[1];
                        break;
                    }
                    case 6: {
                        matchResult_6 = 0;
                        preview_4 = matchValue_21.fields[1];
                        break;
                    }
                    case 7: {
                        matchResult_6 = 0;
                        preview_4 = matchValue_21.fields[1];
                        break;
                    }
                    case 1: {
                        if (matchValue_21.fields[0]) {
                            matchResult_6 = 1;
                            highlighted = matchValue_21.fields[1];
                        }
                        else {
                            matchResult_6 = 2;
                        }
                        break;
                    }
                    default:
                        matchResult_6 = 2;
                }
                switch (matchResult_6) {
                    case 0: {
                        const next_6 = unlockedUpdate(new MapEditorAction(/* SetSelectedRegionGeometry */ 52, [preview_4]), state);
                        if (next_6.Validation != null) {
                            return next_6;
                        }
                        else {
                            return new MapEditorState(next_6.Map, next_6.Tool, next_6.TerrainSelection, next_6.BrushSize, next_6.TerrainCursor, next_6.KeyboardCursor, next_6.KeyboardObject, next_6.LastTerrainPaintTool, next_6.TerrainAnnouncement, next_6.EdgeCursor, next_6.EdgeAnnouncement, next_6.UnitPaletteSearch, next_6.UnitPaletteCursor, next_6.UnitPlacementCursor, next_6.UnitAnnouncement, next_6.RegionAnnouncement, RegionKeyboardMode_26.RegionIdle, next_6.SelectedUnit, next_6.SelectedUnits, next_6.SelectedRegion, next_6.Gesture, next_6.Revision, next_6.RevisionState, next_6.SavedDigest, next_6.SimulatedDigest, next_6.RecoveredFromDigest, next_6.UndoHistory, next_6.RedoHistory, next_6.HistoryBytes, next_6.Clipboard, next_6.Tick, next_6.IsRunning, next_6.LastEvents, next_6.Validation, next_6.Layers, next_6.Issues, next_6.ActiveIssue, next_6.PendingDestructiveChange, next_6.PendingRecovery, next_6.Authoring);
                        }
                    }
                    case 1: {
                        const next_7 = unlockedUpdate(new MapEditorAction(/* SetSelectedRegionPurpose */ 51, [highlighted]), state);
                        if (next_7.Validation != null) {
                            return next_7;
                        }
                        else {
                            return new MapEditorState(next_7.Map, next_7.Tool, next_7.TerrainSelection, next_7.BrushSize, next_7.TerrainCursor, next_7.KeyboardCursor, next_7.KeyboardObject, next_7.LastTerrainPaintTool, next_7.TerrainAnnouncement, next_7.EdgeCursor, next_7.EdgeAnnouncement, next_7.UnitPaletteSearch, next_7.UnitPaletteCursor, next_7.UnitPlacementCursor, next_7.UnitAnnouncement, next_7.RegionAnnouncement, RegionKeyboardMode_26.RegionIdle, next_7.SelectedUnit, next_7.SelectedUnits, next_7.SelectedRegion, next_7.Gesture, next_7.Revision, next_7.RevisionState, next_7.SavedDigest, next_7.SimulatedDigest, next_7.RecoveredFromDigest, next_7.UndoHistory, next_7.RedoHistory, next_7.HistoryBytes, next_7.Clipboard, next_7.Tick, next_7.IsRunning, next_7.LastEvents, next_7.Validation, next_7.Layers, next_7.Issues, next_7.ActiveIssue, next_7.PendingDestructiveChange, next_7.PendingRecovery, next_7.Authoring);
                        }
                    }
                    default:
                        return state;
                }
            }
            case 44: {
                const matchValue_22 = state.RegionKeyboardMode;
                let matchResult_7, purpose_11;
                switch (matchValue_22.tag) {
                    case 2: {
                        matchResult_7 = 0;
                        break;
                    }
                    case 3: {
                        matchResult_7 = 1;
                        purpose_11 = matchValue_22.fields[0];
                        break;
                    }
                    case 4: {
                        matchResult_7 = 1;
                        purpose_11 = matchValue_22.fields[0];
                        break;
                    }
                    case 0: {
                        if (state.SelectedRegion != null) {
                            matchResult_7 = 2;
                        }
                        else {
                            matchResult_7 = 3;
                        }
                        break;
                    }
                    default:
                        matchResult_7 = 3;
                }
                switch (matchResult_7) {
                    case 0:
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, "Returned to region purpose selection.", new RegionKeyboardMode_26(/* RegionPurposeSelection */ 1, [false, matchValue_22.fields[0]]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    case 1:
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, "Region geometry canceled; choose a shape.", new RegionKeyboardMode_26(/* RegionShapeSelection */ 2, [purpose_11]), state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    case 2: {
                        action_mut = (new MapEditorAction(/* SelectEditorRegion */ 50, [undefined]));
                        state_mut = state;
                        continue unlockedUpdate;
                    }
                    default:
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, "Region keyboard operation canceled.", RegionKeyboardMode_26.RegionIdle, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 45: {
                const region_3 = new MapRegion(state.Map.NextRegionId, new RegionGeometry(/* RegionRectangle */ 0, [min(first.CellColumn, last_1.CellColumn), min(first.CellRow, last_1.CellRow), Math.abs(last_1.CellColumn - first.CellColumn) + 1, Math.abs(last_1.CellRow - first.CellRow) + 1]), purpose_12, RegionBehavior.NoRegionBehavior);
                const next_8 = commit(new EditorCommand(/* AddRegions */ 5, [[region_3]]), state);
                if (equals_1(next_8.Revision.Number, state.Revision.Number)) {
                    return new MapEditorState(next_8.Map, next_8.Tool, next_8.TerrainSelection, next_8.BrushSize, next_8.TerrainCursor, next_8.KeyboardCursor, next_8.KeyboardObject, next_8.LastTerrainPaintTool, next_8.TerrainAnnouncement, next_8.EdgeCursor, next_8.EdgeAnnouncement, next_8.UnitPaletteSearch, next_8.UnitPaletteCursor, next_8.UnitPlacementCursor, next_8.UnitAnnouncement, "Region creation rejected.", next_8.RegionKeyboardMode, next_8.SelectedUnit, next_8.SelectedUnits, next_8.SelectedRegion, next_8.Gesture, next_8.Revision, next_8.RevisionState, next_8.SavedDigest, next_8.SimulatedDigest, next_8.RecoveredFromDigest, next_8.UndoHistory, next_8.RedoHistory, next_8.HistoryBytes, next_8.Clipboard, next_8.Tick, next_8.IsRunning, next_8.LastEvents, next_8.Validation, next_8.Layers, next_8.Issues, next_8.ActiveIssue, next_8.PendingDestructiveChange, next_8.PendingRecovery, next_8.Authoring);
                }
                else {
                    const SelectedUnits_2 = empty_2({
                        Compare: (x_6, y_6) => (comparePrimitives(x_6, y_6) | 0),
                    });
                    return new MapEditorState(next_8.Map, next_8.Tool, next_8.TerrainSelection, next_8.BrushSize, next_8.TerrainCursor, next_8.KeyboardCursor, next_8.KeyboardObject, next_8.LastTerrainPaintTool, next_8.TerrainAnnouncement, next_8.EdgeCursor, next_8.EdgeAnnouncement, next_8.UnitPaletteSearch, next_8.UnitPaletteCursor, next_8.UnitPlacementCursor, next_8.UnitAnnouncement, ((regionPurposeLabel(purpose_12) + " rectangle created as region ") + int32ToString(region_3.Id)) + ".", next_8.RegionKeyboardMode, undefined, SelectedUnits_2, region_3.Id, next_8.Gesture, next_8.Revision, next_8.RevisionState, next_8.SavedDigest, next_8.SimulatedDigest, next_8.RecoveredFromDigest, next_8.UndoHistory, next_8.RedoHistory, next_8.HistoryBytes, next_8.Clipboard, next_8.Tick, next_8.IsRunning, next_8.LastEvents, next_8.Validation, next_8.Layers, next_8.Issues, next_8.ActiveIssue, next_8.PendingDestructiveChange, next_8.PendingRecovery, next_8.Authoring);
                }
            }
            case 46: {
                const region_4 = new MapRegion(state.Map.NextRegionId, new RegionGeometry(/* RegionPolygon */ 1, [copy(vertices_12)]), purpose_13, RegionBehavior.NoRegionBehavior);
                const next_9 = commit(new EditorCommand(/* AddRegions */ 5, [[region_4]]), state);
                if (equals_1(next_9.Revision.Number, state.Revision.Number)) {
                    return new MapEditorState(next_9.Map, next_9.Tool, next_9.TerrainSelection, next_9.BrushSize, next_9.TerrainCursor, next_9.KeyboardCursor, next_9.KeyboardObject, next_9.LastTerrainPaintTool, next_9.TerrainAnnouncement, next_9.EdgeCursor, next_9.EdgeAnnouncement, next_9.UnitPaletteSearch, next_9.UnitPaletteCursor, next_9.UnitPlacementCursor, next_9.UnitAnnouncement, "Region creation rejected.", next_9.RegionKeyboardMode, next_9.SelectedUnit, next_9.SelectedUnits, next_9.SelectedRegion, next_9.Gesture, next_9.Revision, next_9.RevisionState, next_9.SavedDigest, next_9.SimulatedDigest, next_9.RecoveredFromDigest, next_9.UndoHistory, next_9.RedoHistory, next_9.HistoryBytes, next_9.Clipboard, next_9.Tick, next_9.IsRunning, next_9.LastEvents, next_9.Validation, next_9.Layers, next_9.Issues, next_9.ActiveIssue, next_9.PendingDestructiveChange, next_9.PendingRecovery, next_9.Authoring);
                }
                else {
                    const SelectedUnits_3 = empty_2({
                        Compare: (x_7, y_7) => (comparePrimitives(x_7, y_7) | 0),
                    });
                    return new MapEditorState(next_9.Map, next_9.Tool, next_9.TerrainSelection, next_9.BrushSize, next_9.TerrainCursor, next_9.KeyboardCursor, next_9.KeyboardObject, next_9.LastTerrainPaintTool, next_9.TerrainAnnouncement, next_9.EdgeCursor, next_9.EdgeAnnouncement, next_9.UnitPaletteSearch, next_9.UnitPaletteCursor, next_9.UnitPlacementCursor, next_9.UnitAnnouncement, ((regionPurposeLabel(purpose_13) + " polygon created as region ") + int32ToString(region_4.Id)) + ".", next_9.RegionKeyboardMode, undefined, SelectedUnits_3, region_4.Id, next_9.Gesture, next_9.Revision, next_9.RevisionState, next_9.SavedDigest, next_9.SimulatedDigest, next_9.RecoveredFromDigest, next_9.UndoHistory, next_9.RedoHistory, next_9.HistoryBytes, next_9.Clipboard, next_9.Tick, next_9.IsRunning, next_9.LastEvents, next_9.Validation, next_9.Layers, next_9.Issues, next_9.ActiveIssue, next_9.PendingDestructiveChange, next_9.PendingRecovery, next_9.Authoring);
                }
            }
            case 47: {
                let selected_2;
                const option_14 = id_2;
                selected_2 = ((option_14 != null) ? (containsKey(option_14, state.Map.Regions) ? option_14 : undefined) : undefined);
                const SelectedUnits_4 = empty_2({
                    Compare: (x_8, y_8) => (comparePrimitives(x_8, y_8) | 0),
                });
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, defaultArg((option_16 = selected_2, (option_16 != null) ? (("Region " + int32ToString(option_16)) + " selected.") : undefined), "Region selection cleared."), state.RegionKeyboardMode, undefined, SelectedUnits_4, selected_2, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            }
            case 48: {
                let matchValue_23;
                const option_19 = state.SelectedRegion;
                matchValue_23 = ((option_19 != null) ? tryFind_1(option_19, state.Map.Regions) : undefined);
                if (matchValue_23 != null) {
                    const region_5 = matchValue_23;
                    const next_10 = commit(new EditorCommand(/* UpdateRegions */ 6, [[new MapRegion(region_5.Id, region_5.Geometry, purpose_14, region_5.Behavior)]]), state);
                    return new MapEditorState(next_10.Map, next_10.Tool, next_10.TerrainSelection, next_10.BrushSize, next_10.TerrainCursor, next_10.KeyboardCursor, next_10.KeyboardObject, next_10.LastTerrainPaintTool, next_10.TerrainAnnouncement, next_10.EdgeCursor, next_10.EdgeAnnouncement, next_10.UnitPaletteSearch, next_10.UnitPaletteCursor, next_10.UnitPlacementCursor, next_10.UnitAnnouncement, regionPurposeLabel(purpose_14) + " purpose applied.", next_10.RegionKeyboardMode, next_10.SelectedUnit, next_10.SelectedUnits, next_10.SelectedRegion, next_10.Gesture, next_10.Revision, next_10.RevisionState, next_10.SavedDigest, next_10.SimulatedDigest, next_10.RecoveredFromDigest, next_10.UndoHistory, next_10.RedoHistory, next_10.HistoryBytes, next_10.Clipboard, next_10.Tick, next_10.IsRunning, next_10.LastEvents, next_10.Validation, next_10.Layers, next_10.Issues, next_10.ActiveIssue, next_10.PendingDestructiveChange, next_10.PendingRecovery, next_10.Authoring);
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Select a region first.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 49: {
                let matchValue_24;
                const option_21 = state.SelectedRegion;
                matchValue_24 = ((option_21 != null) ? tryFind_1(option_21, state.Map.Regions) : undefined);
                if (matchValue_24 != null) {
                    const region_6 = matchValue_24;
                    const next_11 = commit(new EditorCommand(/* UpdateRegions */ 6, [[new MapRegion(region_6.Id, geometry_3, region_6.Purpose, region_6.Behavior)]]), state);
                    return new MapEditorState(next_11.Map, next_11.Tool, next_11.TerrainSelection, next_11.BrushSize, next_11.TerrainCursor, next_11.KeyboardCursor, next_11.KeyboardObject, next_11.LastTerrainPaintTool, next_11.TerrainAnnouncement, next_11.EdgeCursor, next_11.EdgeAnnouncement, next_11.UnitPaletteSearch, next_11.UnitPaletteCursor, next_11.UnitPlacementCursor, next_11.UnitAnnouncement, "Region geometry updated.", next_11.RegionKeyboardMode, next_11.SelectedUnit, next_11.SelectedUnits, next_11.SelectedRegion, next_11.Gesture, next_11.Revision, next_11.RevisionState, next_11.SavedDigest, next_11.SimulatedDigest, next_11.RecoveredFromDigest, next_11.UndoHistory, next_11.RedoHistory, next_11.HistoryBytes, next_11.Clipboard, next_11.Tick, next_11.IsRunning, next_11.LastEvents, next_11.Validation, next_11.Layers, next_11.Issues, next_11.ActiveIssue, next_11.PendingDestructiveChange, next_11.PendingRecovery, next_11.Authoring);
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Select a region first.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 50: {
                let matchValue_25;
                const option_23 = state.SelectedRegion;
                matchValue_25 = ((option_23 != null) ? tryFind_1(option_23, state.Map.Regions) : undefined);
                if (matchValue_25 != null) {
                    const region_7 = matchValue_25;
                    const next_12 = commit(new EditorCommand(/* UpdateRegions */ 6, [[new MapRegion(region_7.Id, translatedRegionGeometry(columnDelta_3, rowDelta_3, region_7.Geometry), region_7.Purpose, region_7.Behavior)]]), state);
                    return new MapEditorState(next_12.Map, next_12.Tool, next_12.TerrainSelection, next_12.BrushSize, next_12.TerrainCursor, next_12.KeyboardCursor, next_12.KeyboardObject, next_12.LastTerrainPaintTool, next_12.TerrainAnnouncement, next_12.EdgeCursor, next_12.EdgeAnnouncement, next_12.UnitPaletteSearch, next_12.UnitPaletteCursor, next_12.UnitPlacementCursor, next_12.UnitAnnouncement, (next_12.Validation != null) ? "Region move rejected." : "Region moved.", next_12.RegionKeyboardMode, next_12.SelectedUnit, next_12.SelectedUnits, next_12.SelectedRegion, next_12.Gesture, next_12.Revision, next_12.RevisionState, next_12.SavedDigest, next_12.SimulatedDigest, next_12.RecoveredFromDigest, next_12.UndoHistory, next_12.RedoHistory, next_12.HistoryBytes, next_12.Clipboard, next_12.Tick, next_12.IsRunning, next_12.LastEvents, next_12.Validation, next_12.Layers, next_12.Issues, next_12.ActiveIssue, next_12.PendingDestructiveChange, next_12.PendingRecovery, next_12.Authoring);
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Select a region first.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 51: {
                let matchValue_26;
                const option_25 = state.SelectedRegion;
                matchValue_26 = ((option_25 != null) ? tryFind_1(option_25, state.Map.Regions) : undefined);
                let matchResult_8, region_9, vertices_14;
                if (matchValue_26 == null) {
                    matchResult_8 = 2;
                }
                else if (matchValue_26.Geometry.tag === 1) {
                    if ((vertices_13 = matchValue_26.Geometry.fields[0], (index_6 >= 0) && (index_6 < vertices_13.length))) {
                        matchResult_8 = 0;
                        region_9 = matchValue_26;
                        vertices_14 = matchValue_26.Geometry.fields[0];
                    }
                    else {
                        matchResult_8 = 1;
                    }
                }
                else {
                    matchResult_8 = 1;
                }
                switch (matchResult_8) {
                    case 0: {
                        const moved_3 = copy(vertices_14);
                        const vertex_1 = item_2(index_6, moved_3);
                        setItem(moved_3, index_6, new EditorCellAddress(vertex_1.CellColumn + columnDelta_4, vertex_1.CellRow + rowDelta_4));
                        const next_13 = commit(new EditorCommand(/* UpdateRegions */ 6, [[new MapRegion(region_9.Id, new RegionGeometry(/* RegionPolygon */ 1, [moved_3]), region_9.Purpose, region_9.Behavior)]]), state);
                        return new MapEditorState(next_13.Map, next_13.Tool, next_13.TerrainSelection, next_13.BrushSize, next_13.TerrainCursor, next_13.KeyboardCursor, next_13.KeyboardObject, next_13.LastTerrainPaintTool, next_13.TerrainAnnouncement, next_13.EdgeCursor, next_13.EdgeAnnouncement, next_13.UnitPaletteSearch, next_13.UnitPaletteCursor, next_13.UnitPlacementCursor, next_13.UnitAnnouncement, "Polygon vertex updated.", next_13.RegionKeyboardMode, next_13.SelectedUnit, next_13.SelectedUnits, next_13.SelectedRegion, next_13.Gesture, next_13.Revision, next_13.RevisionState, next_13.SavedDigest, next_13.SimulatedDigest, next_13.RecoveredFromDigest, next_13.UndoHistory, next_13.RedoHistory, next_13.HistoryBytes, next_13.Clipboard, next_13.Tick, next_13.IsRunning, next_13.LastEvents, next_13.Validation, next_13.Layers, next_13.Issues, next_13.ActiveIssue, next_13.PendingDestructiveChange, next_13.PendingRecovery, next_13.Authoring);
                    }
                    case 1:
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Select a polygon vertex first.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    default:
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Select a region first.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 52: {
                const matchValue_27 = state.SelectedRegion;
                if (matchValue_27 != null) {
                    const id_7 = matchValue_27 | 0;
                    const next_14 = commit(new EditorCommand(/* RemoveRegions */ 7, [new Int32Array([id_7])]), state);
                    return new MapEditorState(next_14.Map, next_14.Tool, next_14.TerrainSelection, next_14.BrushSize, next_14.TerrainCursor, next_14.KeyboardCursor, next_14.KeyboardObject, next_14.LastTerrainPaintTool, next_14.TerrainAnnouncement, next_14.EdgeCursor, next_14.EdgeAnnouncement, next_14.UnitPaletteSearch, next_14.UnitPaletteCursor, next_14.UnitPlacementCursor, next_14.UnitAnnouncement, ("Region " + int32ToString(id_7)) + " removed.", next_14.RegionKeyboardMode, next_14.SelectedUnit, next_14.SelectedUnits, undefined, next_14.Gesture, next_14.Revision, next_14.RevisionState, next_14.SavedDigest, next_14.SimulatedDigest, next_14.RecoveredFromDigest, next_14.UndoHistory, next_14.RedoHistory, next_14.HistoryBytes, next_14.Clipboard, next_14.Tick, next_14.IsRunning, next_14.LastEvents, next_14.Validation, next_14.Layers, next_14.Issues, next_14.ActiveIssue, next_14.PendingDestructiveChange, next_14.PendingRecovery, next_14.Authoring);
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Select a region first.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 53: {
                const matchValue_28 = state.Tool;
                if (matchValue_28.tag === 4) {
                    const previewState = new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, address_2, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    const unit_1 = placementUnit(matchValue_28.fields[0], matchValue_28.fields[1], matchValue_28.fields[2], address_2, previewState);
                    const command = new EditorCommand(/* AddUnits */ 2, [[unit_1]]);
                    return new MapEditorState(previewState.Map, previewState.Tool, previewState.TerrainSelection, previewState.BrushSize, previewState.TerrainCursor, previewState.KeyboardCursor, previewState.KeyboardObject, previewState.LastTerrainPaintTool, previewState.TerrainAnnouncement, previewState.EdgeCursor, previewState.EdgeAnnouncement, previewState.UnitPaletteSearch, previewState.UnitPaletteCursor, previewState.UnitPlacementCursor, ((((((("Placement preview for " + unit_1.ClassId) + ", ") + int32ToString(unit_1.Size)) + " by ") + int32ToString(unit_1.Size)) + " cells — ") + defaultArg((option_27 = unitPlacementIssue(previewState), (option_27 != null) ? ("invalid: " + option_27) : undefined), "valid")) + ".", previewState.RegionAnnouncement, previewState.RegionKeyboardMode, previewState.SelectedUnit, previewState.SelectedUnits, previewState.SelectedRegion, new EditorGesture(/* CommandPreviewGesture */ 3, [command]), previewState.Revision, previewState.RevisionState, previewState.SavedDigest, previewState.SimulatedDigest, previewState.RecoveredFromDigest, previewState.UndoHistory, previewState.RedoHistory, previewState.HistoryBytes, previewState.Clipboard, previewState.Tick, previewState.IsRunning, previewState.LastEvents, undefined, previewState.Layers, previewState.Issues, previewState.ActiveIssue, previewState.PendingDestructiveChange, previewState.PendingRecovery, previewState.Authoring);
                }
                else {
                    return state;
                }
            }
            case 54: {
                const original_8 = selectedUnits(state);
                if (original_8.length === 0) {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Select at least one unit to move.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, ("Moving " + int32ToString(original_8.length)) + ((original_8.length === 1) ? " unit." : " units as one formation."), state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, new EditorGesture(/* UnitMoveGesture */ 4, [address_3, address_3, original_8, new EditorCommand(/* UpdateUnits */ 3, [original_8])]), state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 55: {
                const matchValue_29 = state.Gesture;
                if (matchValue_29.tag === 4) {
                    const original_9 = matchValue_29.fields[2];
                    const anchor_2 = matchValue_29.fields[0];
                    const columnDelta_5 = (address_4.CellColumn - anchor_2.CellColumn) | 0;
                    const rowDelta_5 = (address_4.CellRow - anchor_2.CellRow) | 0;
                    const command_2 = new EditorCommand(/* UpdateUnits */ 3, [translatedSelection(columnDelta_5, rowDelta_5, original_9)]);
                    const prefix = ((("Movement preview " + int32ToString(columnDelta_5)) + " columns, ") + int32ToString(rowDelta_5)) + " rows.";
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, movementCrossesEdge(state.Map, columnDelta_5, rowDelta_5, original_9) ? (prefix + " Invalid destination: a blocking edge crosses the route.") : unitPreviewMessage(prefix, state.Map, command_2), state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, new EditorGesture(/* UnitMoveGesture */ 4, [anchor_2, address_4, original_9, command_2]), state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return state;
                }
            }
            case 56: {
                const size_5 = max(1, min(9, size_4)) | 0;
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, size_5, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, ((("Brush size " + int32ToString(size_5)) + " by ") + int32ToString(size_5)) + " cells.", state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            }
            case 57: {
                const matchValue_30 = state.Tool;
                if (matchValue_30.tag === 2) {
                    if (matchValue_30.fields[0].tag === 4) {
                        const sampled = defaultArg(tryFind_1([address_5.CellColumn, address_5.CellRow], state.Map.Terrain), MapTerrain.Open);
                        return new MapEditorState(state.Map, new MapEditorTool(/* Terrain */ 2, [state.LastTerrainPaintTool]), sampled, state.BrushSize, address_5, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, ((((("Sampled " + terrainName(sampled)) + " terrain at column ") + int32ToString(address_5.CellColumn + 1)) + ", row ") + int32ToString(address_5.CellRow + 1)) + ".", state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    }
                    else {
                        const preview_5 = terrainGestureAddresses(state.Map, state.BrushSize, matchValue_30.fields[0], address_5, address_5, []);
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, address_5, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, ((int32ToString(preview_5.length) + " terrain ") + ((preview_5.length === 1) ? "cell" : "cells")) + " previewed.", state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, new EditorGesture(/* TerrainGesture */ 5, [matchValue_30.fields[0], address_5, address_5, []]), state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    }
                }
                else {
                    return state;
                }
            }
            case 58:
                return state;
            case 59: {
                const matchValue_31 = state.Gesture;
                if (matchValue_31.tag === 5) {
                    const visited = matchValue_31.fields[3];
                    const tool_2 = matchValue_31.fields[0];
                    const current_4 = matchValue_31.fields[2];
                    const anchor_3 = matchValue_31.fields[1];
                    const patternInput_2 = (tool_2.tag === 0) ? [current_4, normalizeAddresses(append_2(lineAddresses(anchor_3, current_4), visited))] : ((tool_2.tag === 5) ? [current_4, normalizeAddresses(append_2(lineAddresses(anchor_3, current_4), visited))] : [anchor_3, visited]);
                    const nextVisited = patternInput_2[1];
                    const nextAnchor = patternInput_2[0];
                    const preview_6 = terrainGestureAddresses(state.Map, state.BrushSize, tool_2, nextAnchor, address_6, nextVisited);
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, address_6, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, ((int32ToString(preview_6.length) + " terrain ") + ((preview_6.length === 1) ? "cell" : "cells")) + " previewed.", state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, new EditorGesture(/* TerrainGesture */ 5, [tool_2, nextAnchor, address_6, nextVisited]), state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return state;
                }
            }
            case 60:
                return state;
            case 61: {
                const cursor_7 = new EditorCellAddress(max(0, min(state.Map.Width - 1, state.TerrainCursor.CellColumn + columnDelta_6)), max(0, min(state.Map.Height - 1, state.TerrainCursor.CellRow + rowDelta_6)));
                const moved_4 = new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, cursor_7, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                if (extendPreview ? true : !equals(state.Gesture, EditorGesture.IdleGesture)) {
                    const matchValue_33 = state.Tool;
                    let matchResult_9;
                    if (state.Gesture.tag === 0) {
                        if (matchValue_33.tag === 2) {
                            switch (matchValue_33.fields[0].tag) {
                                case 0:
                                case 5: {
                                    matchResult_9 = 0;
                                    break;
                                }
                                default:
                                    matchResult_9 = 1;
                            }
                        }
                        else {
                            matchResult_9 = 1;
                        }
                    }
                    else {
                        matchResult_9 = 1;
                    }
                    switch (matchResult_9) {
                        case 0:
                            return update(MapEditorAction.CommitEditorGesture, update(new MapEditorAction(/* ExtendTerrainGesture */ 60, [cursor_7]), update(new MapEditorAction(/* BeginTerrainGesture */ 59, [state.TerrainCursor]), moved_4)));
                        default:
                            return update(new MapEditorAction(/* ExtendTerrainGesture */ 60, [cursor_7]), moved_4);
                    }
                }
                else {
                    return moved_4;
                }
            }
            case 62: {
                const matchValue_35 = state.Gesture;
                const matchValue_36 = state.Tool;
                let matchResult_10;
                switch (matchValue_35.tag) {
                    case 5: {
                        matchResult_10 = 0;
                        break;
                    }
                    case 0: {
                        if (matchValue_36.tag === 2) {
                            switch (matchValue_36.fields[0].tag) {
                                case 0:
                                case 5:
                                case 3: {
                                    matchResult_10 = 1;
                                    break;
                                }
                                default:
                                    matchResult_10 = 2;
                            }
                        }
                        else {
                            matchResult_10 = 2;
                        }
                        break;
                    }
                    default:
                        matchResult_10 = 2;
                }
                switch (matchResult_10) {
                    case 0:
                        return update(MapEditorAction.CommitEditorGesture, state);
                    case 1:
                        return update(MapEditorAction.CommitEditorGesture, update(new MapEditorAction(/* BeginTerrainGesture */ 59, [state.TerrainCursor]), state));
                    default:
                        return update(new MapEditorAction(/* BeginTerrainGesture */ 59, [state.TerrainCursor]), state);
                }
            }
            case 63: {
                const matchValue_38 = state.Gesture;
                if (matchValue_38.tag === 5) {
                    const anchor_4 = matchValue_38.fields[1];
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, anchor_4, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, "Terrain preview reset to its anchor.", state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, new EditorGesture(/* TerrainGesture */ 5, [matchValue_38.fields[0], anchor_4, anchor_4, []]), state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return state;
                }
            }
            case 64: {
                const current_5 = state.KeyboardCursor.Cell;
                const cell_1 = new EditorCellAddress(max(0, min(state.Map.Width - 1, current_5.CellColumn + columnDelta_7)), max(0, min(state.Map.Height - 1, current_5.CellRow + rowDelta_7)));
                const next_15 = new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, cell_1, new EditorKeyboardCursor(cell_1, 0), state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                return new MapEditorState(next_15.Map, next_15.Tool, next_15.TerrainSelection, next_15.BrushSize, next_15.TerrainCursor, next_15.KeyboardCursor, objectAtKeyboardCursor(next_15), next_15.LastTerrainPaintTool, next_15.TerrainAnnouncement, next_15.EdgeCursor, next_15.EdgeAnnouncement, next_15.UnitPaletteSearch, next_15.UnitPaletteCursor, next_15.UnitPlacementCursor, next_15.UnitAnnouncement, next_15.RegionAnnouncement, next_15.RegionKeyboardMode, next_15.SelectedUnit, next_15.SelectedUnits, next_15.SelectedRegion, next_15.Gesture, next_15.Revision, next_15.RevisionState, next_15.SavedDigest, next_15.SimulatedDigest, next_15.RecoveredFromDigest, next_15.UndoHistory, next_15.RedoHistory, next_15.HistoryBytes, next_15.Clipboard, next_15.Tick, next_15.IsRunning, next_15.LastEvents, next_15.Validation, next_15.Layers, next_15.Issues, next_15.ActiveIssue, next_15.PendingDestructiveChange, next_15.PendingRecovery, next_15.Authoring);
            }
            case 65: {
                const count_2 = length_1(keyboardObjectsAtCursor(state)) | 0;
                if (count_2 === 0) {
                    return state;
                }
                else {
                    const index_7 = ((state.KeyboardCursor.ObjectCycleIndex + delta_4) % count_2) | 0;
                    const next_16 = new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, new EditorKeyboardCursor(state.KeyboardCursor.Cell, (index_7 < 0) ? (index_7 + count_2) : index_7), state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    const target = objectAtKeyboardCursor(next_16);
                    let matchResult_11, id_8, id_9;
                    if (target != null) {
                        switch (target.tag) {
                            case 0: {
                                matchResult_11 = 0;
                                id_8 = target.fields[0];
                                break;
                            }
                            case 1: {
                                matchResult_11 = 1;
                                id_9 = target.fields[0];
                                break;
                            }
                            default:
                                matchResult_11 = 2;
                        }
                    }
                    else {
                        matchResult_11 = 2;
                    }
                    switch (matchResult_11) {
                        case 0:
                            return new MapEditorState(next_16.Map, next_16.Tool, next_16.TerrainSelection, next_16.BrushSize, next_16.TerrainCursor, next_16.KeyboardCursor, target, next_16.LastTerrainPaintTool, next_16.TerrainAnnouncement, next_16.EdgeCursor, next_16.EdgeAnnouncement, next_16.UnitPaletteSearch, next_16.UnitPaletteCursor, next_16.UnitPlacementCursor, next_16.UnitAnnouncement, next_16.RegionAnnouncement, next_16.RegionKeyboardMode, id_8, singleton_2(id_8, {
                                Compare: (x_9, y_9) => (comparePrimitives(x_9, y_9) | 0),
                            }), undefined, next_16.Gesture, next_16.Revision, next_16.RevisionState, next_16.SavedDigest, next_16.SimulatedDigest, next_16.RecoveredFromDigest, next_16.UndoHistory, next_16.RedoHistory, next_16.HistoryBytes, next_16.Clipboard, next_16.Tick, next_16.IsRunning, next_16.LastEvents, next_16.Validation, next_16.Layers, next_16.Issues, next_16.ActiveIssue, next_16.PendingDestructiveChange, next_16.PendingRecovery, next_16.Authoring);
                        case 1:
                            return new MapEditorState(next_16.Map, next_16.Tool, next_16.TerrainSelection, next_16.BrushSize, next_16.TerrainCursor, next_16.KeyboardCursor, target, next_16.LastTerrainPaintTool, next_16.TerrainAnnouncement, next_16.EdgeCursor, next_16.EdgeAnnouncement, next_16.UnitPaletteSearch, next_16.UnitPaletteCursor, next_16.UnitPlacementCursor, next_16.UnitAnnouncement, next_16.RegionAnnouncement, next_16.RegionKeyboardMode, undefined, empty_2({
                                Compare: (x_10, y_10) => (comparePrimitives(x_10, y_10) | 0),
                            }), id_9, next_16.Gesture, next_16.Revision, next_16.RevisionState, next_16.SavedDigest, next_16.SimulatedDigest, next_16.RecoveredFromDigest, next_16.UndoHistory, next_16.RedoHistory, next_16.HistoryBytes, next_16.Clipboard, next_16.Tick, next_16.IsRunning, next_16.LastEvents, next_16.Validation, next_16.Layers, next_16.Issues, next_16.ActiveIssue, next_16.PendingDestructiveChange, next_16.PendingRecovery, next_16.Authoring);
                        default:
                            return new MapEditorState(next_16.Map, next_16.Tool, next_16.TerrainSelection, next_16.BrushSize, next_16.TerrainCursor, next_16.KeyboardCursor, target, next_16.LastTerrainPaintTool, next_16.TerrainAnnouncement, next_16.EdgeCursor, next_16.EdgeAnnouncement, next_16.UnitPaletteSearch, next_16.UnitPaletteCursor, next_16.UnitPlacementCursor, next_16.UnitAnnouncement, next_16.RegionAnnouncement, next_16.RegionKeyboardMode, undefined, empty_2({
                                Compare: (x_11, y_11) => (comparePrimitives(x_11, y_11) | 0),
                            }), undefined, next_16.Gesture, next_16.Revision, next_16.RevisionState, next_16.SavedDigest, next_16.SimulatedDigest, next_16.RecoveredFromDigest, next_16.UndoHistory, next_16.RedoHistory, next_16.HistoryBytes, next_16.Clipboard, next_16.Tick, next_16.IsRunning, next_16.LastEvents, next_16.Validation, next_16.Layers, next_16.Issues, next_16.ActiveIssue, next_16.PendingDestructiveChange, next_16.PendingRecovery, next_16.Authoring);
                    }
                }
            }
            case 66: {
                const target_1 = objectAtKeyboardCursor(state);
                if (target_1 == null) {
                    return state;
                }
                else {
                    switch (target_1.tag) {
                        case 0:
                            if ((id_10 = (target_1.fields[0] | 0), !toggle && state.SelectedUnits.Equals(singleton_2(id_10, {
                                Compare: (x_12, y_12) => (comparePrimitives(x_12, y_12) | 0),
                            })))) {
                                const id_12 = target_1.fields[0] | 0;
                                return update(MapEditorAction.OpenSelectedObjectActions, state);
                            }
                            else {
                                const id_14 = target_1.fields[0] | 0;
                                const next_17 = toggle ? update(new MapEditorAction(/* ToggleEditorUnitSelection */ 78, [id_14]), state) : update(new MapEditorAction(/* SelectEditorUnit */ 77, [id_14]), state);
                                return new MapEditorState(next_17.Map, next_17.Tool, next_17.TerrainSelection, next_17.BrushSize, next_17.TerrainCursor, next_17.KeyboardCursor, target_1, next_17.LastTerrainPaintTool, next_17.TerrainAnnouncement, next_17.EdgeCursor, next_17.EdgeAnnouncement, next_17.UnitPaletteSearch, next_17.UnitPaletteCursor, next_17.UnitPlacementCursor, next_17.UnitAnnouncement, next_17.RegionAnnouncement, next_17.RegionKeyboardMode, next_17.SelectedUnit, next_17.SelectedUnits, next_17.SelectedRegion, next_17.Gesture, next_17.Revision, next_17.RevisionState, next_17.SavedDigest, next_17.SimulatedDigest, next_17.RecoveredFromDigest, next_17.UndoHistory, next_17.RedoHistory, next_17.HistoryBytes, next_17.Clipboard, next_17.Tick, next_17.IsRunning, next_17.LastEvents, next_17.Validation, next_17.Layers, next_17.Issues, next_17.ActiveIssue, next_17.PendingDestructiveChange, next_17.PendingRecovery, next_17.Authoring);
                            }
                        case 1:
                            if ((id_11 = (target_1.fields[0] | 0), !toggle && equals(state.SelectedRegion, id_11))) {
                                const id_13 = target_1.fields[0] | 0;
                                return update(MapEditorAction.OpenSelectedObjectActions, state);
                            }
                            else {
                                const id_15 = target_1.fields[0] | 0;
                                const next_18 = update(new MapEditorAction(/* SelectEditorRegion */ 50, [id_15]), state);
                                return new MapEditorState(next_18.Map, MapEditorTool.Select, next_18.TerrainSelection, next_18.BrushSize, next_18.TerrainCursor, next_18.KeyboardCursor, target_1, next_18.LastTerrainPaintTool, next_18.TerrainAnnouncement, next_18.EdgeCursor, next_18.EdgeAnnouncement, next_18.UnitPaletteSearch, next_18.UnitPaletteCursor, next_18.UnitPlacementCursor, next_18.UnitAnnouncement, next_18.RegionAnnouncement, next_18.RegionKeyboardMode, next_18.SelectedUnit, next_18.SelectedUnits, next_18.SelectedRegion, next_18.Gesture, next_18.Revision, next_18.RevisionState, next_18.SavedDigest, next_18.SimulatedDigest, next_18.RecoveredFromDigest, next_18.UndoHistory, next_18.RedoHistory, next_18.HistoryBytes, next_18.Clipboard, next_18.Tick, next_18.IsRunning, next_18.LastEvents, next_18.Validation, next_18.Layers, next_18.Issues, next_18.ActiveIssue, next_18.PendingDestructiveChange, next_18.PendingRecovery, next_18.Authoring);
                            }
                        default: {
                            const target_2 = target_1;
                            return new MapEditorState(state.Map, MapEditorTool.Select, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, target_2, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, undefined, empty_2({
                                Compare: (x_13, y_13) => (comparePrimitives(x_13, y_13) | 0),
                            }), undefined, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                        }
                    }
                }
            }
            case 67:
                if (FSharpSet__get_IsEmpty(state.SelectedUnits) && (state.SelectedRegion == null)) {
                    return state;
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.SelectedObjectActionsGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            case 68:
                return update(new MapEditorAction(/* BeginEditorBoxSelection */ 80, [state.KeyboardCursor.Cell]), state);
            case 69: {
                const address_7 = [column_5, row_5, direction_1];
                if (!validEdgeKey(state.Map, address_7[0], address_7[1], address_7[2])) {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, "Edge placement rejected at the map border.", state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "The edge has no canonical owning cell.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    let state_6;
                    const matchValue_39 = state.Tool;
                    state_6 = ((matchValue_39.tag === 5) ? (new MapEditorState(state.Map, new MapEditorTool(/* Edge */ 5, [direction_1, matchValue_39.fields[1]]), state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring)) : state);
                    const matchValue_40 = state_6.Tool;
                    if (matchValue_40.tag === 5) {
                        if (matchValue_40.fields[1].tag === 0) {
                            const matchValue_41 = state_6.Gesture;
                            let matchResult_12, segments_1, segments_2;
                            if (matchValue_41.tag === 6) {
                                if (matchValue_41.fields[0].tag === 0) {
                                    if (contains_2(address_7, matchValue_41.fields[1], {
                                        Equals: equalArrays,
                                        GetHashCode: (x_14) => (arrayHash(x_14) | 0),
                                    })) {
                                        matchResult_12 = 0;
                                        segments_1 = matchValue_41.fields[1];
                                    }
                                    else {
                                        matchResult_12 = 1;
                                        segments_2 = matchValue_41.fields[1];
                                    }
                                }
                                else {
                                    matchResult_12 = 2;
                                }
                            }
                            else {
                                matchResult_12 = 2;
                            }
                            switch (matchResult_12) {
                                case 0:
                                    return new MapEditorState(state_6.Map, state_6.Tool, state_6.TerrainSelection, state_6.BrushSize, state_6.TerrainCursor, state_6.KeyboardCursor, state_6.KeyboardObject, state_6.LastTerrainPaintTool, state_6.TerrainAnnouncement, address_7, "Duplicate edge segment ignored.", state_6.UnitPaletteSearch, state_6.UnitPaletteCursor, state_6.UnitPlacementCursor, state_6.UnitAnnouncement, state_6.RegionAnnouncement, state_6.RegionKeyboardMode, state_6.SelectedUnit, state_6.SelectedUnits, state_6.SelectedRegion, state_6.Gesture, state_6.Revision, state_6.RevisionState, state_6.SavedDigest, state_6.SimulatedDigest, state_6.RecoveredFromDigest, state_6.UndoHistory, state_6.RedoHistory, state_6.HistoryBytes, state_6.Clipboard, state_6.Tick, state_6.IsRunning, state_6.LastEvents, "This canonical edge is already in the polyline.", state_6.Layers, state_6.Issues, state_6.ActiveIssue, state_6.PendingDestructiveChange, state_6.PendingRecovery, state_6.Authoring);
                                case 1: {
                                    const segments_3 = Array_distinct(append_2(segments_2, (tupledArg_1 = item_2(segments_2.length - 1, segments_2), connectingEdgePath(state_6.Map, tupledArg_1[0], tupledArg_1[1], tupledArg_1[2], address_7[0], address_7[1], address_7[2]))), {
                                        Equals: equalArrays,
                                        GetHashCode: (x_15) => (arrayHash(x_15) | 0),
                                    });
                                    return new MapEditorState(state_6.Map, state_6.Tool, state_6.TerrainSelection, state_6.BrushSize, state_6.TerrainCursor, state_6.KeyboardCursor, state_6.KeyboardObject, state_6.LastTerrainPaintTool, state_6.TerrainAnnouncement, address_7, int32ToString(segments_3.length) + " wall polyline segments previewed. Double-click or press Enter to finish.", state_6.UnitPaletteSearch, state_6.UnitPaletteCursor, state_6.UnitPlacementCursor, state_6.UnitAnnouncement, state_6.RegionAnnouncement, state_6.RegionKeyboardMode, state_6.SelectedUnit, state_6.SelectedUnits, state_6.SelectedRegion, new EditorGesture(/* EdgePolylineGesture */ 6, [MapEdgeKind.Wall, segments_3]), state_6.Revision, state_6.RevisionState, state_6.SavedDigest, state_6.SimulatedDigest, state_6.RecoveredFromDigest, state_6.UndoHistory, state_6.RedoHistory, state_6.HistoryBytes, state_6.Clipboard, state_6.Tick, state_6.IsRunning, state_6.LastEvents, undefined, state_6.Layers, state_6.Issues, state_6.ActiveIssue, state_6.PendingDestructiveChange, state_6.PendingRecovery, state_6.Authoring);
                                }
                                default:
                                    return new MapEditorState(state_6.Map, state_6.Tool, state_6.TerrainSelection, state_6.BrushSize, state_6.TerrainCursor, state_6.KeyboardCursor, state_6.KeyboardObject, state_6.LastTerrainPaintTool, state_6.TerrainAnnouncement, address_7, "Wall polyline started with one segment.", state_6.UnitPaletteSearch, state_6.UnitPaletteCursor, state_6.UnitPlacementCursor, state_6.UnitAnnouncement, state_6.RegionAnnouncement, state_6.RegionKeyboardMode, state_6.SelectedUnit, state_6.SelectedUnits, state_6.SelectedRegion, new EditorGesture(/* EdgePolylineGesture */ 6, [MapEdgeKind.Wall, [address_7]]), state_6.Revision, state_6.RevisionState, state_6.SavedDigest, state_6.SimulatedDigest, state_6.RecoveredFromDigest, state_6.UndoHistory, state_6.RedoHistory, state_6.HistoryBytes, state_6.Clipboard, state_6.Tick, state_6.IsRunning, state_6.LastEvents, undefined, state_6.Layers, state_6.Issues, state_6.ActiveIssue, state_6.PendingDestructiveChange, state_6.PendingRecovery, state_6.Authoring);
                            }
                        }
                        else {
                            return replaceOneEdge(address_7[0], address_7[1], address_7[2], [matchValue_40.fields[1], false], ("Converted edge to " + edgeKindName(matchValue_40.fields[1])) + ".", state_6);
                        }
                    }
                    else {
                        return state_6;
                    }
                }
            }
            case 70: {
                const patternInput_3 = state.EdgeCursor;
                const direction_2 = patternInput_3[2];
                const cursor_8 = [max(0, min(state.Map.Width - 1, patternInput_3[0] + columnDelta_8)), max(0, min(state.Map.Height - 1, patternInput_3[1] + rowDelta_8)), direction_2];
                const moved_5 = new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, cursor_8, ((((("Edge cursor at column " + int32ToString(cursor_8[0] + 1)) + ", row ") + int32ToString(cursor_8[1] + 1)) + ", ") + edgeDirectionName(direction_2)) + ".", state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                if (extendPreview_1) {
                    return update(new MapEditorAction(/* ActivateEdge */ 12, [cursor_8[0], cursor_8[1], cursor_8[2]]), moved_5);
                }
                else {
                    return moved_5;
                }
            }
            case 71: {
                const patternInput_4 = state.EdgeCursor;
                return update(new MapEditorAction(/* ActivateEdge */ 12, [patternInput_4[0], patternInput_4[1], patternInput_4[2]]), state);
            }
            case 72:
                return finishEdgePolyline(state);
            case 73: {
                const matchValue_42 = state.Gesture;
                if (matchValue_42.tag === 6) {
                    if (matchValue_42.fields[1].length > 0) {
                        const remaining_1 = take(matchValue_42.fields[1].length - 1, matchValue_42.fields[1]);
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, (remaining_1.length === 0) ? "Last edge segment removed. Press Escape again to cancel." : (("Last edge segment removed; " + int32ToString(remaining_1.length)) + " remain."), state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, new EditorGesture(/* EdgePolylineGesture */ 6, [matchValue_42.fields[0], remaining_1]), state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    }
                    else {
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, "Edge polyline canceled.", state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    }
                }
                else {
                    return state;
                }
            }
            case 74:
                return replaceOneEdge(column_9, row_9, direction_4, [kind_5, false], ("Converted edge to " + edgeKindName(kind_5)) + ".", new MapEditorState(state.Map, new MapEditorTool(/* Edge */ 5, [direction_4, kind_5]), state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring));
            case 75: {
                const address_8 = [column_10, row_10, direction_5];
                const matchValue_43 = tryFind_1(address_8, state.Map.Edges);
                let matchResult_13, isOpen;
                if (matchValue_43 != null) {
                    if (matchValue_43[0].tag === 1) {
                        matchResult_13 = 0;
                        isOpen = matchValue_43[1];
                    }
                    else {
                        matchResult_13 = 1;
                    }
                }
                else {
                    matchResult_13 = 1;
                }
                switch (matchResult_13) {
                    case 0:
                        return replaceOneEdge(address_8[0], address_8[1], address_8[2], [MapEdgeKind.Door, !isOpen], isOpen ? "Door closed." : "Door opened.", state);
                    default:
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, address_8, "Select or create a door before toggling its state.", state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Only a door has editable open/closed state.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 76:
                return replaceOneEdge(column_11, row_11, direction_6, undefined, (action.tag === 20) ? "Edge run split by removing one canonical segment." : "Edge erased.", state);
            case 77:
                return replaceOneEdge(column_12, row_12, direction_7, [MapEdgeKind.Wall, false], "Edge run joined with one canonical wall segment.", state);
            case 78:
                return update(MapEditorAction.CommitEditorGesture, update(new MapEditorAction(/* PreviewUnitPlacement */ 56, [new EditorCellAddress(column_13, row_13)]), state));
            case 79: {
                const preview_7 = update(new MapEditorAction(/* BeginTerrainGesture */ 59, [new EditorCellAddress(column_14, row_14)]), state);
                if (preview_7.Gesture.tag === 5) {
                    return update(MapEditorAction.CommitEditorGesture, preview_7);
                }
                else {
                    return preview_7;
                }
            }
            case 80: {
                const selected_3 = defaultArg((option_33 = ((option_31 = id_16, (option_31 != null) ? (containsKey(option_31, state.Map.Units) ? option_31 : undefined) : undefined)), (option_33 != null) ? singleton_2(option_33, {
                    Compare: (x_16, y_16) => (comparePrimitives(x_16, y_16) | 0),
                }) : undefined), empty_2({
                    Compare: (x_17, y_17) => (comparePrimitives(x_17, y_17) | 0),
                }));
                return new MapEditorState(state.Map, MapEditorTool.Select, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, (option_36 = id_16, (option_36 != null) ? (contains(option_36, selected_3) ? option_36 : undefined) : undefined), selected_3, undefined, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            }
            case 81: {
                const selected_4 = contains(id_17, state.SelectedUnits) ? remove_1(id_17, state.SelectedUnits) : add_1(id_17, state.SelectedUnits);
                return new MapEditorState(state.Map, MapEditorTool.Select, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, contains(id_17, selected_4) ? id_17 : tryHead(toList_2(selected_4)), selected_4, undefined, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            }
            case 82:
                return state;
            case 83: {
                const selected_5 = idsInBox(box, state.Map);
                return new MapEditorState(state.Map, MapEditorTool.Select, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, tryHead(toList_2(selected_5)), selected_5, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            }
            case 84: {
                const selected_6 = union(state.SelectedUnits, idsInBox(box_1, state.Map));
                return new MapEditorState(state.Map, MapEditorTool.Select, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, tryHead(toList_2(selected_6)), selected_6, undefined, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            }
            case 85:
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, new EditorGesture(/* BoxSelectionGesture */ 2, [address_9, address_9]), state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            case 86: {
                const matchValue_45 = state.Gesture;
                if (matchValue_45.tag === 2) {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, new EditorGesture(/* BoxSelectionGesture */ 2, [matchValue_45.fields[0], address_10]), state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return state;
                }
            }
            case 87: {
                const matchValue_46 = state.Gesture;
                switch (matchValue_46.tag) {
                    case 2:
                        return update(new MapEditorAction(/* SelectEditorUnitsInBox */ 79, [new EditorBox(matchValue_46.fields[0].CellColumn, matchValue_46.fields[0].CellRow, matchValue_46.fields[1].CellColumn, matchValue_46.fields[1].CellRow)]), state);
                    case 3:
                        if (matchValue_46.fields[0].tag === 2) {
                            const next_19 = commit(matchValue_46.fields[0], state);
                            if (next_19.Validation != null) {
                                return next_19;
                            }
                            else {
                                const SelectedUnits_9 = ofArray_1(map_8((_arg_2) => (_arg_2.Id | 0), matchValue_46.fields[0].fields[0], Int32Array), {
                                    Compare: (x_18, y_18) => (comparePrimitives(x_18, y_18) | 0),
                                });
                                return new MapEditorState(next_19.Map, next_19.Tool, next_19.TerrainSelection, next_19.BrushSize, next_19.TerrainCursor, next_19.KeyboardCursor, next_19.KeyboardObject, next_19.LastTerrainPaintTool, next_19.TerrainAnnouncement, next_19.EdgeCursor, next_19.EdgeAnnouncement, next_19.UnitPaletteSearch, next_19.UnitPaletteCursor, next_19.UnitPlacementCursor, next_19.UnitAnnouncement, next_19.RegionAnnouncement, next_19.RegionKeyboardMode, (option_38 = tryHead_1(matchValue_46.fields[0].fields[0]), (option_38 != null) ? option_38.Id : undefined), SelectedUnits_9, next_19.SelectedRegion, next_19.Gesture, next_19.Revision, next_19.RevisionState, next_19.SavedDigest, next_19.SimulatedDigest, next_19.RecoveredFromDigest, next_19.UndoHistory, next_19.RedoHistory, next_19.HistoryBytes, next_19.Clipboard, next_19.Tick, next_19.IsRunning, next_19.LastEvents, next_19.Validation, next_19.Layers, next_19.Issues, next_19.ActiveIssue, next_19.PendingDestructiveChange, next_19.PendingRecovery, next_19.Authoring);
                            }
                        }
                        else {
                            return commit(matchValue_46.fields[0], state);
                        }
                    case 4:
                        if (movementCrossesEdge(state.Map, matchValue_46.fields[1].CellColumn - matchValue_46.fields[0].CellColumn, matchValue_46.fields[1].CellRow - matchValue_46.fields[0].CellRow, matchValue_46.fields[2])) {
                            return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, "Movement rejected by a blocking edge.", state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "The formation route crosses a blocking edge.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                        }
                        else {
                            const next_20 = commit(matchValue_46.fields[3], state);
                            return new MapEditorState(next_20.Map, next_20.Tool, next_20.TerrainSelection, next_20.BrushSize, next_20.TerrainCursor, next_20.KeyboardCursor, next_20.KeyboardObject, next_20.LastTerrainPaintTool, next_20.TerrainAnnouncement, next_20.EdgeCursor, next_20.EdgeAnnouncement, next_20.UnitPaletteSearch, next_20.UnitPaletteCursor, next_20.UnitPlacementCursor, (next_20.Validation != null) ? ("Movement rejected. " + value_18(next_20.Validation)) : ((next_20.Revision.Digest === state.Revision.Digest) ? "Formation did not move." : ((("Moved " + int32ToString(matchValue_46.fields[2].length)) + ((matchValue_46.fields[2].length === 1) ? " unit" : " units")) + " atomically.")), next_20.RegionAnnouncement, next_20.RegionKeyboardMode, next_20.SelectedUnit, next_20.SelectedUnits, next_20.SelectedRegion, next_20.Gesture, next_20.Revision, next_20.RevisionState, next_20.SavedDigest, next_20.SimulatedDigest, next_20.RecoveredFromDigest, next_20.UndoHistory, next_20.RedoHistory, next_20.HistoryBytes, next_20.Clipboard, next_20.Tick, next_20.IsRunning, next_20.LastEvents, next_20.Validation, next_20.Layers, next_20.Issues, next_20.ActiveIssue, next_20.PendingDestructiveChange, next_20.PendingRecovery, next_20.Authoring);
                        }
                    case 5: {
                        const command_6 = terrainGestureCommand(state, matchValue_46.fields[0], matchValue_46.fields[1], matchValue_46.fields[2], matchValue_46.fields[3]);
                        const next_21 = commit(command_6, state);
                        if (equals(next_21.Gesture, EditorGesture.IdleGesture) && (next_21.Validation == null)) {
                            if (next_21.Revision.Digest === state.Revision.Digest) {
                                return new MapEditorState(next_21.Map, next_21.Tool, next_21.TerrainSelection, next_21.BrushSize, next_21.TerrainCursor, next_21.KeyboardCursor, next_21.KeyboardObject, next_21.LastTerrainPaintTool, "No terrain cells changed.", next_21.EdgeCursor, next_21.EdgeAnnouncement, next_21.UnitPaletteSearch, next_21.UnitPaletteCursor, next_21.UnitPlacementCursor, next_21.UnitAnnouncement, next_21.RegionAnnouncement, next_21.RegionKeyboardMode, next_21.SelectedUnit, next_21.SelectedUnits, next_21.SelectedRegion, next_21.Gesture, next_21.Revision, next_21.RevisionState, next_21.SavedDigest, next_21.SimulatedDigest, next_21.RecoveredFromDigest, next_21.UndoHistory, next_21.RedoHistory, next_21.HistoryBytes, next_21.Clipboard, next_21.Tick, next_21.IsRunning, next_21.LastEvents, next_21.Validation, next_21.Layers, next_21.Issues, next_21.ActiveIssue, next_21.PendingDestructiveChange, next_21.PendingRecovery, next_21.Authoring);
                            }
                            else if (command_6.tag === 0) {
                                const addresses_1 = command_6.fields[1];
                                return new MapEditorState(next_21.Map, next_21.Tool, next_21.TerrainSelection, next_21.BrushSize, next_21.TerrainCursor, next_21.KeyboardCursor, next_21.KeyboardObject, next_21.LastTerrainPaintTool, ((((((equals(command_6.fields[0], MapTerrain.Open) ? "Erased " : "Painted ") + int32ToString(addresses_1.length)) + " terrain ") + ((addresses_1.length === 1) ? "cell" : "cells")) + " in revision ") + int64ToString(next_21.Revision.Number)) + ".", next_21.EdgeCursor, next_21.EdgeAnnouncement, next_21.UnitPaletteSearch, next_21.UnitPaletteCursor, next_21.UnitPlacementCursor, next_21.UnitAnnouncement, next_21.RegionAnnouncement, next_21.RegionKeyboardMode, next_21.SelectedUnit, next_21.SelectedUnits, next_21.SelectedRegion, next_21.Gesture, next_21.Revision, next_21.RevisionState, next_21.SavedDigest, next_21.SimulatedDigest, next_21.RecoveredFromDigest, next_21.UndoHistory, next_21.RedoHistory, next_21.HistoryBytes, next_21.Clipboard, next_21.Tick, next_21.IsRunning, next_21.LastEvents, next_21.Validation, next_21.Layers, next_21.Issues, next_21.ActiveIssue, next_21.PendingDestructiveChange, next_21.PendingRecovery, next_21.Authoring);
                            }
                            else {
                                return next_21;
                            }
                        }
                        else {
                            return new MapEditorState(next_21.Map, next_21.Tool, next_21.TerrainSelection, next_21.BrushSize, next_21.TerrainCursor, next_21.KeyboardCursor, next_21.KeyboardObject, next_21.LastTerrainPaintTool, "Terrain change rejected. " + defaultArg(next_21.Validation, "The preview is invalid."), next_21.EdgeCursor, next_21.EdgeAnnouncement, next_21.UnitPaletteSearch, next_21.UnitPaletteCursor, next_21.UnitPlacementCursor, next_21.UnitAnnouncement, next_21.RegionAnnouncement, next_21.RegionKeyboardMode, next_21.SelectedUnit, next_21.SelectedUnits, next_21.SelectedRegion, EditorGesture.IdleGesture, next_21.Revision, next_21.RevisionState, next_21.SavedDigest, next_21.SimulatedDigest, next_21.RecoveredFromDigest, next_21.UndoHistory, next_21.RedoHistory, next_21.HistoryBytes, next_21.Clipboard, next_21.Tick, next_21.IsRunning, next_21.LastEvents, next_21.Validation, next_21.Layers, next_21.Issues, next_21.ActiveIssue, next_21.PendingDestructiveChange, next_21.PendingRecovery, next_21.Authoring);
                        }
                    }
                    case 6:
                        return finishEdgePolyline(state);
                    case 0:
                        return state;
                    default:
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, state.Validation, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 88:
                if (state.Gesture.tag === 6) {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, "Edge polyline canceled; no staged segments were applied.", state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    let UnitAnnouncement_17;
                    const matchValue_48 = state.Gesture;
                    let matchResult_14;
                    switch (matchValue_48.tag) {
                        case 4: {
                            matchResult_14 = 0;
                            break;
                        }
                        case 3: {
                            if (matchValue_48.fields[0].tag === 2) {
                                matchResult_14 = 1;
                            }
                            else {
                                matchResult_14 = 2;
                            }
                            break;
                        }
                        default:
                            matchResult_14 = 2;
                    }
                    switch (matchResult_14) {
                        case 0: {
                            UnitAnnouncement_17 = "Movement preview canceled; original positions restored.";
                            break;
                        }
                        case 1: {
                            UnitAnnouncement_17 = "Unit placement or paste preview canceled.";
                            break;
                        }
                        default:
                            UnitAnnouncement_17 = state.UnitAnnouncement;
                    }
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, (state.Gesture.tag === 5) ? "Terrain preview canceled." : state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, UnitAnnouncement_17, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, EditorGesture.IdleGesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            case 89:
                if (equals(activeDomain(state.Tool), EditorDomain.UnitDomain)) {
                    const selected_7 = ofList_1(map_9((tuple_1) => (tuple_1[0] | 0), toList_1(state.Map.Units)), {
                        Compare: (x_19, y_19) => (comparePrimitives(x_19, y_19) | 0),
                    });
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, tryHead(toList_2(selected_7)), selected_7, undefined, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, undefined, empty_2({
                        Compare: (x_20, y_20) => (comparePrimitives(x_20, y_20) | 0),
                    }), state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "The active domain has no selectable objects yet.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            case 90: {
                const units_2 = sortBy_2((_arg_4) => (_arg_4.Id | 0), choose_1((id_18) => tryFind_1(id_18, state.Map.Units), toArray_3(state.SelectedUnits)), {
                    Compare: (x_21, y_21) => (comparePrimitives(x_21, y_21) | 0),
                });
                if (units_2.length === 0) {
                    return state;
                }
                else if (units_2.length > 256) {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, undefined, state.Tick, state.IsRunning, state.LastEvents, ("Clipboard selections are limited to " + int32ToString(256)) + " units.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, new EditorClipboard(state.Revision.Digest, units_2), state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 91: {
                const matchValue_50 = state.Clipboard;
                if (matchValue_50 != null) {
                    const matchValue_51 = tryTranslatedCommand(matchValue_50.UnitFragment, state);
                    if (matchValue_51 != null) {
                        const units_3 = matchValue_51[1];
                        const command_7 = matchValue_51[0];
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, ("Paste preview for " + int32ToString(units_3.length)) + ((units_3.length === 1) ? " unit." : " units. Press Enter to commit."), state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, new EditorGesture(/* CommandPreviewGesture */ 3, [command_7]), state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    }
                    else {
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "The copied formation does not fit.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    }
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Copy units before pasting.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 92: {
                const copied = update(MapEditorAction.CopyEditorSelection, state);
                const matchValue_52 = copied.Clipboard;
                if (matchValue_52 != null) {
                    const matchValue_53 = tryTranslatedCommand(matchValue_52.UnitFragment, copied);
                    if (matchValue_53 != null) {
                        const units_4 = matchValue_53[1];
                        const next_22 = commit(matchValue_53[0], copied);
                        const SelectedUnits_11 = ofArray_1(map_8((_arg_5) => (_arg_5.Id | 0), units_4, Int32Array), {
                            Compare: (x_22, y_22) => (comparePrimitives(x_22, y_22) | 0),
                        });
                        let SelectedUnit_16;
                        const option_41 = tryHead_1(units_4);
                        SelectedUnit_16 = ((option_41 != null) ? option_41.Id : undefined);
                        return new MapEditorState(next_22.Map, next_22.Tool, next_22.TerrainSelection, next_22.BrushSize, next_22.TerrainCursor, next_22.KeyboardCursor, next_22.KeyboardObject, next_22.LastTerrainPaintTool, next_22.TerrainAnnouncement, next_22.EdgeCursor, next_22.EdgeAnnouncement, next_22.UnitPaletteSearch, next_22.UnitPaletteCursor, next_22.UnitPlacementCursor, ("Duplicated " + int32ToString(units_4.length)) + ((units_4.length === 1) ? " unit." : " units atomically."), next_22.RegionAnnouncement, next_22.RegionKeyboardMode, SelectedUnit_16, SelectedUnits_11, next_22.SelectedRegion, next_22.Gesture, next_22.Revision, next_22.RevisionState, next_22.SavedDigest, next_22.SimulatedDigest, next_22.RecoveredFromDigest, next_22.UndoHistory, next_22.RedoHistory, next_22.HistoryBytes, next_22.Clipboard, next_22.Tick, next_22.IsRunning, next_22.LastEvents, next_22.Validation, next_22.Layers, next_22.Issues, next_22.ActiveIssue, next_22.PendingDestructiveChange, next_22.PendingRecovery, next_22.Authoring);
                    }
                    else {
                        return new MapEditorState(copied.Map, copied.Tool, copied.TerrainSelection, copied.BrushSize, copied.TerrainCursor, copied.KeyboardCursor, copied.KeyboardObject, copied.LastTerrainPaintTool, copied.TerrainAnnouncement, copied.EdgeCursor, copied.EdgeAnnouncement, copied.UnitPaletteSearch, copied.UnitPaletteCursor, copied.UnitPlacementCursor, copied.UnitAnnouncement, copied.RegionAnnouncement, copied.RegionKeyboardMode, copied.SelectedUnit, copied.SelectedUnits, copied.SelectedRegion, copied.Gesture, copied.Revision, copied.RevisionState, copied.SavedDigest, copied.SimulatedDigest, copied.RecoveredFromDigest, copied.UndoHistory, copied.RedoHistory, copied.HistoryBytes, copied.Clipboard, copied.Tick, copied.IsRunning, copied.LastEvents, "The duplicated formation does not fit.", copied.Layers, copied.Issues, copied.ActiveIssue, copied.PendingDestructiveChange, copied.PendingRecovery, copied.Authoring);
                    }
                }
                else {
                    return copied;
                }
            }
            case 93: {
                const identifiers_1 = toArray_3(state.SelectedUnits);
                if (identifiers_1.length === 0) {
                    return state;
                }
                else {
                    const includesAuthoredPlanning = identifiers_1.some((id_19) => exists_1((unit_2) => {
                        if (!equals(unit_2.Controller, MapController.Manual)) {
                            return true;
                        }
                        else {
                            return !isEmpty(unit_2.Script);
                        }
                    }, toArray(tryFind_1(id_19, state.Map.Units))));
                    if ((identifiers_1.length > 5) ? true : includesAuthoredPlanning) {
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, ("Confirm deletion of " + int32ToString(identifiers_1.length)) + ((identifiers_1.length === 1) ? " unit." : " units."), state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "Deleting this selection requires confirmation because it is bulk or contains authored planning data.", state.Layers, state.Issues, state.ActiveIssue, new PendingDestructiveChange_4(/* UnitDeletionPending */ 3, [identifiers_1]), state.PendingRecovery, state.Authoring);
                    }
                    else {
                        const next_23 = commit(new EditorCommand(/* RemoveUnits */ 4, [identifiers_1]), state);
                        return new MapEditorState(next_23.Map, next_23.Tool, next_23.TerrainSelection, next_23.BrushSize, next_23.TerrainCursor, next_23.KeyboardCursor, next_23.KeyboardObject, next_23.LastTerrainPaintTool, next_23.TerrainAnnouncement, next_23.EdgeCursor, next_23.EdgeAnnouncement, next_23.UnitPaletteSearch, next_23.UnitPaletteCursor, next_23.UnitPlacementCursor, int32ToString(identifiers_1.length) + ((identifiers_1.length === 1) ? " unit deleted." : " units deleted."), next_23.RegionAnnouncement, next_23.RegionKeyboardMode, next_23.SelectedUnit, next_23.SelectedUnits, next_23.SelectedRegion, next_23.Gesture, next_23.Revision, next_23.RevisionState, next_23.SavedDigest, next_23.SimulatedDigest, next_23.RecoveredFromDigest, next_23.UndoHistory, next_23.RedoHistory, next_23.HistoryBytes, next_23.Clipboard, next_23.Tick, next_23.IsRunning, next_23.LastEvents, next_23.Validation, next_23.Layers, next_23.Issues, next_23.ActiveIssue, next_23.PendingDestructiveChange, next_23.PendingRecovery, next_23.Authoring);
                    }
                }
            }
            case 94:
                return commit(new EditorCommand(/* UpdateUnits */ 3, [map_8((unit_3) => (new EditorUnit(unit_3.Id, side_4, unit_3.ClassId, unit_3.Column, unit_3.Row, unit_3.Size, unit_3.Health, unit_3.HealthMaximum, unit_3.Controller, unit_3.Script, unit_3.ScriptIndex, unit_3.BodyFacing, unit_3.AttentionDirection)), selectedUnits(state))]), state);
            case 95: {
                const classId_5 = classId_4.trim();
                if ((isNullOrWhiteSpace(classId_5) ? true : (classId_5.length > 128)) ? true : exists_1(isWhiteSpace, classId_5.split(""))) {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, ("Class ID must be one non-empty token no longer than " + int32ToString(128)) + " characters.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
                else {
                    return commit(new EditorCommand(/* UpdateUnits */ 3, [map_8((unit_4) => (new EditorUnit(unit_4.Id, unit_4.Side, classId_5, unit_4.Column, unit_4.Row, unit_4.Size, unit_4.Health, unit_4.HealthMaximum, unit_4.Controller, unit_4.Script, unit_4.ScriptIndex, unit_4.BodyFacing, unit_4.AttentionDirection)), selectedUnits(state))]), state);
                }
            }
            case 96:
                return commit(new EditorCommand(/* UpdateUnits */ 3, [map_8((unit_5) => (new EditorUnit(unit_5.Id, unit_5.Side, unit_5.ClassId, unit_5.Column, unit_5.Row, size_6, unit_5.Health, unit_5.HealthMaximum, unit_5.Controller, unit_5.Script, unit_5.ScriptIndex, unit_5.BodyFacing, unit_5.AttentionDirection)), selectedUnits(state))]), state);
            case 97:
                return commit(new EditorCommand(/* UpdateUnits */ 3, [map_8((unit_6) => (new EditorUnit(unit_6.Id, unit_6.Side, unit_6.ClassId, unit_6.Column, unit_6.Row, unit_6.Size, remaining_2, maximum_1, unit_6.Controller, unit_6.Script, unit_6.ScriptIndex, unit_6.BodyFacing, unit_6.AttentionDirection)), selectedUnits(state))]), state);
            case 98:
                return commit(new EditorCommand(/* UpdateUnits */ 3, [map_8((unit_7) => (new EditorUnit(unit_7.Id, unit_7.Side, unit_7.ClassId, unit_7.Column, unit_7.Row, unit_7.Size, unit_7.Health, unit_7.HealthMaximum, controller_1, unit_7.Script, unit_7.ScriptIndex, unit_7.BodyFacing, unit_7.AttentionDirection)), selectedUnits(state))]), state);
            case 99: {
                const matchValue_54 = parseScript(text_2);
                if (matchValue_54.tag === 0) {
                    return commit(new EditorCommand(/* UpdateUnits */ 3, [map_8((unit_8) => (new EditorUnit(unit_8.Id, unit_8.Side, unit_8.ClassId, unit_8.Column, unit_8.Row, unit_8.Size, unit_8.Health, unit_8.HealthMaximum, unit_8.Controller, matchValue_54.fields[0], 0, unit_8.BodyFacing, unit_8.AttentionDirection)), selectedUnits(state))]), state);
                }
                else {
                    return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, matchValue_54.fields[0], state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                }
            }
            case 100: {
                const matchValue_55 = state.UndoHistory;
                if (!isEmpty(matchValue_55)) {
                    const remaining_3 = tail(matchValue_55);
                    const entry = head(matchValue_55);
                    const selected_8 = selectedAfterMap(entry.Before.Document, state.SelectedUnits);
                    const RevisionState_1 = equals(state.SavedDigest, entry.Before.Digest) ? RevisionState_9.SavedRevision : RevisionState_9.DirtyRevision;
                    return new MapEditorState(entry.Before.Document, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, tryHead(toList_2(selected_8)), selected_8, (option_44 = state.SelectedRegion, (option_44 != null) ? (containsKey(option_44, entry.Before.Document.Regions) ? option_44 : undefined) : undefined), EditorGesture.IdleGesture, entry.Before, RevisionState_1, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, remaining_3, historyWithinBounds(cons(entry, state.RedoHistory)), historySize(remaining_3), state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, validationIssues(entry.Before.Document), (validationIssues(entry.Before.Document).length === 0) ? undefined : 0, state.PendingDestructiveChange, state.PendingRecovery, (bind$0040_5 = state.Authoring, new MapAuthoringMetadata(bind$0040_5.Name, bind$0040_5.SavedViews, entry.Before.Digest, undefined)));
                }
                else {
                    return state;
                }
            }
            case 101: {
                const matchValue_56 = state.RedoHistory;
                if (!isEmpty(matchValue_56)) {
                    const entry_1 = head(matchValue_56);
                    const selected_9 = selectedAfterMap(entry_1.After.Document, state.SelectedUnits);
                    const undo = historyWithinBounds(cons(entry_1, state.UndoHistory));
                    const RevisionState_2 = equals(state.SavedDigest, entry_1.After.Digest) ? RevisionState_9.SavedRevision : RevisionState_9.DirtyRevision;
                    return new MapEditorState(entry_1.After.Document, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, tryHead(toList_2(selected_9)), selected_9, (option_46 = state.SelectedRegion, (option_46 != null) ? (containsKey(option_46, entry_1.After.Document.Regions) ? option_46 : undefined) : undefined), EditorGesture.IdleGesture, entry_1.After, RevisionState_2, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, undo, tail(matchValue_56), historySize(undo), state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, validationIssues(entry_1.After.Document), (validationIssues(entry_1.After.Document).length === 0) ? undefined : 0, state.PendingDestructiveChange, state.PendingRecovery, (bind$0040_6 = state.Authoring, new MapAuthoringMetadata(bind$0040_6.Name, bind$0040_6.SavedViews, entry_1.After.Digest, undefined)));
                }
                else {
                    return state;
                }
            }
            case 102:
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, RevisionState_9.SavedRevision, state.Revision.Digest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            case 103:
                return new MapEditorState(state.Revision.Document, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, RevisionState_9.SimulatedRevision, state.SavedDigest, state.Revision.Digest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            case 104:
                return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, RevisionState_9.RecoveredRevision, state.SavedDigest, state.SimulatedDigest, sourceDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            case 105:
                return new MapEditorState(state.Revision.Document, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, state.UnitAnnouncement, state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, equals(state.SavedDigest, state.Revision.Digest) ? RevisionState_9.SavedRevision : RevisionState_9.DirtyRevision, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, 0, false, empty(), undefined, state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
            case 106:
                return update(MapEditorAction.DeleteEditorSelection, state);
            case 107: {
                const original_11 = selectedUnits(state);
                if (original_11.length === 0) {
                    return state;
                }
                else {
                    const patternInput_5 = directionDelta(direction_8);
                    const rowDelta_10 = patternInput_5[1] | 0;
                    const columnDelta_10 = patternInput_5[0] | 0;
                    const command_15 = new EditorCommand(/* UpdateUnits */ 3, [translatedSelection(columnDelta_10, rowDelta_10, original_11)]);
                    if (movementCrossesEdge(state.Map, columnDelta_10, rowDelta_10, original_11)) {
                        return new MapEditorState(state.Map, state.Tool, state.TerrainSelection, state.BrushSize, state.TerrainCursor, state.KeyboardCursor, state.KeyboardObject, state.LastTerrainPaintTool, state.TerrainAnnouncement, state.EdgeCursor, state.EdgeAnnouncement, state.UnitPaletteSearch, state.UnitPaletteCursor, state.UnitPlacementCursor, "Keyboard movement rejected by a blocking edge.", state.RegionAnnouncement, state.RegionKeyboardMode, state.SelectedUnit, state.SelectedUnits, state.SelectedRegion, state.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, state.Tick, state.IsRunning, state.LastEvents, "That move is blocked.", state.Layers, state.Issues, state.ActiveIssue, state.PendingDestructiveChange, state.PendingRecovery, state.Authoring);
                    }
                    else {
                        const next_24 = commit(command_15, state);
                        return new MapEditorState(next_24.Map, next_24.Tool, next_24.TerrainSelection, next_24.BrushSize, next_24.TerrainCursor, next_24.KeyboardCursor, next_24.KeyboardObject, next_24.LastTerrainPaintTool, next_24.TerrainAnnouncement, next_24.EdgeCursor, next_24.EdgeAnnouncement, next_24.UnitPaletteSearch, next_24.UnitPaletteCursor, next_24.UnitPlacementCursor, (next_24.Validation != null) ? "Keyboard movement rejected." : ((((("Moved " + int32ToString(original_11.length)) + ((original_11.length === 1) ? " unit" : " units")) + " one cell ") + directionCode(direction_8)) + "."), next_24.RegionAnnouncement, next_24.RegionKeyboardMode, next_24.SelectedUnit, next_24.SelectedUnits, next_24.SelectedRegion, next_24.Gesture, next_24.Revision, next_24.RevisionState, next_24.SavedDigest, next_24.SimulatedDigest, next_24.RecoveredFromDigest, next_24.UndoHistory, next_24.RedoHistory, next_24.HistoryBytes, next_24.Clipboard, next_24.Tick, next_24.IsRunning, next_24.LastEvents, next_24.Validation, next_24.Layers, next_24.Issues, next_24.ActiveIssue, next_24.PendingDestructiveChange, next_24.PendingRecovery, next_24.Authoring);
                    }
                }
            }
            case 108: {
                const runtime = legacyUpdate(action, state);
                return new MapEditorState(runtime.Map, runtime.Tool, runtime.TerrainSelection, runtime.BrushSize, runtime.TerrainCursor, runtime.KeyboardCursor, runtime.KeyboardObject, runtime.LastTerrainPaintTool, runtime.TerrainAnnouncement, runtime.EdgeCursor, runtime.EdgeAnnouncement, runtime.UnitPaletteSearch, runtime.UnitPaletteCursor, runtime.UnitPlacementCursor, runtime.UnitAnnouncement, runtime.RegionAnnouncement, runtime.RegionKeyboardMode, runtime.SelectedUnit, runtime.SelectedUnits, runtime.SelectedRegion, runtime.Gesture, state.Revision, state.RevisionState, runtime.SavedDigest, runtime.SimulatedDigest, runtime.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, runtime.Clipboard, runtime.Tick, runtime.IsRunning, runtime.LastEvents, runtime.Validation, runtime.Layers, runtime.Issues, runtime.ActiveIssue, runtime.PendingDestructiveChange, runtime.PendingRecovery, runtime.Authoring);
            }
            default: {
                const legacy = legacyUpdate(action, state);
                const normalizedSelection = (action.tag === 61) ? (equals(state.Tool, MapEditorTool.Select) ? defaultArg((option_48 = legacy.SelectedUnit, (option_48 != null) ? singleton_2(option_48, {
                    Compare: (x_23, y_23) => (comparePrimitives(x_23, y_23) | 0),
                }) : undefined), empty_2({
                    Compare: (x_24, y_24) => (comparePrimitives(x_24, y_24) | 0),
                })) : selectedAfterMap(legacy.Map, state.SelectedUnits)) : ((action.tag === 106) ? defaultArg((option_51 = legacy.SelectedUnit, (option_51 != null) ? singleton_2(option_51, {
                    Compare: (x_25, y_25) => (comparePrimitives(x_25, y_25) | 0),
                }) : undefined), empty_2({
                    Compare: (x_26, y_26) => (comparePrimitives(x_26, y_26) | 0),
                })) : ((action.tag === 105) ? defaultArg((option_51 = legacy.SelectedUnit, (option_51 != null) ? singleton_2(option_51, {
                    Compare: (x_25, y_25) => (comparePrimitives(x_25, y_25) | 0),
                }) : undefined), empty_2({
                    Compare: (x_26, y_26) => (comparePrimitives(x_26, y_26) | 0),
                })) : selectedAfterMap(legacy.Map, state.SelectedUnits)));
                const legacy_1 = new MapEditorState(legacy.Map, legacy.Tool, legacy.TerrainSelection, legacy.BrushSize, legacy.TerrainCursor, legacy.KeyboardCursor, legacy.KeyboardObject, legacy.LastTerrainPaintTool, legacy.TerrainAnnouncement, legacy.EdgeCursor, legacy.EdgeAnnouncement, legacy.UnitPaletteSearch, legacy.UnitPaletteCursor, legacy.UnitPlacementCursor, legacy.UnitAnnouncement, legacy.RegionAnnouncement, legacy.RegionKeyboardMode, (option_56 = ((option_54 = legacy.SelectedUnit, (option_54 != null) ? (contains(option_54, normalizedSelection) ? option_54 : undefined) : undefined)), (option_56 != null) ? option_56 : tryHead(toList_2(normalizedSelection))), normalizedSelection, legacy.SelectedRegion, legacy.Gesture, state.Revision, state.RevisionState, state.SavedDigest, state.SimulatedDigest, state.RecoveredFromDigest, state.UndoHistory, state.RedoHistory, state.HistoryBytes, state.Clipboard, legacy.Tick, legacy.IsRunning, legacy.LastEvents, legacy.Validation, legacy.Layers, legacy.Issues, legacy.ActiveIssue, legacy.PendingDestructiveChange, legacy.PendingRecovery, legacy.Authoring);
                if (equals(legacy_1.Map, state.Map)) {
                    return legacy_1;
                }
                else if (equals(state.RevisionState, RevisionState_9.SimulatedRevision)) {
                    return legacy_1;
                }
                else {
                    return commit(legacyCommand(action, legacy_1.Map), new MapEditorState(state.Map, legacy_1.Tool, legacy_1.TerrainSelection, legacy_1.BrushSize, legacy_1.TerrainCursor, legacy_1.KeyboardCursor, legacy_1.KeyboardObject, legacy_1.LastTerrainPaintTool, legacy_1.TerrainAnnouncement, legacy_1.EdgeCursor, legacy_1.EdgeAnnouncement, legacy_1.UnitPaletteSearch, legacy_1.UnitPaletteCursor, legacy_1.UnitPlacementCursor, legacy_1.UnitAnnouncement, legacy_1.RegionAnnouncement, legacy_1.RegionKeyboardMode, legacy_1.SelectedUnit, legacy_1.SelectedUnits, legacy_1.SelectedRegion, legacy_1.Gesture, legacy_1.Revision, legacy_1.RevisionState, legacy_1.SavedDigest, legacy_1.SimulatedDigest, legacy_1.RecoveredFromDigest, legacy_1.UndoHistory, legacy_1.RedoHistory, legacy_1.HistoryBytes, legacy_1.Clipboard, legacy_1.Tick, legacy_1.IsRunning, legacy_1.LastEvents, legacy_1.Validation, legacy_1.Layers, legacy_1.Issues, legacy_1.ActiveIssue, legacy_1.PendingDestructiveChange, legacy_1.PendingRecovery, legacy_1.Authoring));
                }
            }
        }
        break;
    }
}

function xmlEscape(value) {
    return replace(replace(replace(replace(value, "&", "&amp;"), "<", "&lt;"), ">", "&gt;"), "\"", "&quot;");
}

/**
 * Generates a deterministic, presentation-only thumbnail. The SVG and
 * other authoring metadata are never part of SIR-MAP or its revision
 * digest.
 */
export function thumbnailSvg(state) {
    const width = (state.Map.Width * 12) | 0;
    const height = (state.Map.Height * 12) | 0;
    const terrain = map_9((tupledArg) => {
        const _arg = tupledArg[0];
        const value = tupledArg[1];
        const fill = (value.tag === 2) ? "#302e35" : ((value.tag === 3) ? "#b48835" : ((value.tag === 0) ? "#d8d0bc" : "#8b7d62"));
        return ((((((((("<rect x=\"" + int32ToString(_arg[0] * 12)) + "\" y=\"") + int32ToString(_arg[1] * 12)) + "\" width=\"") + int32ToString(12)) + "\" height=\"") + int32ToString(12)) + "\" fill=\"") + fill) + "\"/>";
    }, toList_1(state.Map.Terrain));
    const units = map_9((tupledArg_1) => {
        const unit = tupledArg_1[1];
        let fill_1;
        const matchValue = unit.Side;
        fill_1 = ((matchValue.tag === 1) ? "#a33d3d" : ((matchValue.tag === 2) ? "#77736a" : "#286b9f"));
        return ((((((((("<rect x=\"" + int32ToString((unit.Column * 12) + 2)) + "\" y=\"") + int32ToString((unit.Row * 12) + 2)) + "\" width=\"") + int32ToString((unit.Size * 12) - 4)) + "\" height=\"") + int32ToString((unit.Size * 12) - 4)) + "\" rx=\"2\" fill=\"") + fill_1) + "\"/>";
    }, toList_1(state.Map.Units));
    return join("", append_1(ofArray(["<svg xmlns=\"http://www.w3.org/2000/svg\" role=\"img\" aria-label=\"", ((((xmlEscape(state.Authoring.Name) + " map thumbnail\" viewBox=\"0 0 ") + int32ToString(width)) + " ") + int32ToString(height)) + "\">", "<rect width=\"100%\" height=\"100%\" fill=\"#d8d0bc\"/>"]), append_1(terrain, append_1(units, singleton("</svg>")))));
}

export function authoringMetadataText(state) {
    const safe = (value) => replace(replace(value, "\r", " "), "\n", " ");
    return join("\n", append_1(ofArray(["SIR-MAP-AUTHORING 1", "name " + safe(state.Authoring.Name), "revision " + state.Revision.Digest]), map_9((tupledArg) => {
        const view = tupledArg[1];
        return (((((("view " + safe(view.Name)) + " ") + view.Camera.PanX.toString()) + " ") + view.Camera.PanY.toString()) + " ") + view.Camera.Zoom.toString();
    }, toList_1(state.Authoring.SavedViews)))) + "\n";
}

export function autosaveText(state) {
    return export$(state);
}

function edgeVisual(_arg, _arg_1) {
    const row = _arg[1] | 0;
    const direction = _arg[2];
    const column = _arg[0] | 0;
    const kind = _arg_1[0];
    const kindName = (kind.tag === 1) ? "door" : ((kind.tag === 2) ? "window" : "wall");
    const state = _arg_1[1] ? "open" : "solid";
    const patternInput = (direction.tag === 1) ? [column, row + 1, column + 1, row + 1] : [column + 1, row, column + 1, row + 1];
    return new EdgeVisual((((("editor-edge-" + int32ToString(column)) + "-") + int32ToString(row)) + "-") + toString(direction), kindName, state, patternInput[0], patternInput[1], patternInput[2], patternInput[3]);
}

export function frame(state) {
    return new RenderFrame(state.Tick, new BoardVisual(0, 0, state.Map.Width - 1, state.Map.Height - 1), toArray_2(map_9((unit) => {
        let matchValue;
        return new UnitVisual(unit.Id, unit.Column, unit.Row, extent(unit.Size), extent(unit.Size), UnitClassIdModule_resolve(unit.ClassId), (matchValue = unit.Side, (matchValue.tag === 1) ? FactionVisual.Arcane : ((matchValue.tag === 2) ? FactionVisual.Neutral : FactionVisual.Human)), new Disclosure$1(/* Disclosed */ 3, [health(unit.Health, unit.HealthMaximum)]), new Disclosure$1(/* Disclosed */ 3, [0]), Disclosure$1.NotPresent, new Disclosure$1(/* Disclosed */ 3, [HeadingRadiansModule_ofDirection8(unit.BodyFacing)]), new Disclosure$1(/* Disclosed */ 3, [new SecondaryHeadingVisual(HeadingRadiansModule_ofDirection8(unit.AttentionDirection), SecondaryHeadingSource.AttentionHeading)]), new Disclosure$1(/* Disclosed */ 3, [int32ToString(unit.Id)]), [controllerName(unit.Controller)]);
    }, map_9((tuple) => tuple[1], toList_1(state.Map.Units)))), toArray_2(map_9((tupledArg) => edgeVisual(tupledArg[0], tupledArg[1]), toList_1(state.Map.Edges))), [], toArray_2(mapIndexed_1((index, summary) => (new RenderEventVisual((state.Tick * 1000) + index, state.Tick, "editor", Disclosure$1.NotPresent, Disclosure$1.NotPresent, new Disclosure$1(/* Disclosed */ 3, [summary]))), state.LastEvents)), DisclosureLabel.SandboxDisclosure);
}

export function terrainAt(column, row, state) {
    return defaultArg(tryFind_1([column, row], state.Map.Terrain), MapTerrain.Open);
}

export function unitAt(column, row, state) {
    return unitAtCell(column, row, state.Map);
}

export function selected(state) {
    return selectedUnit(state);
}

export function terrainLabel(terrain) {
    return terrainName(terrain);
}

export function terrainToolLabel(tool) {
    switch (tool.tag) {
        case 1:
            return "Rectangle";
        case 2:
            return "Line";
        case 3:
            return "Flood fill";
        case 4:
            return "Eyedropper";
        case 5:
            return "Erase";
        default:
            return "Pencil";
    }
}

export function terrainPattern(terrain) {
    switch (terrain.tag) {
        case 1:
            return "diagonal hatch";
        case 2:
            return "cross hatch";
        case 3:
            return "inset ring";
        default:
            return "plain";
    }
}

export function terrainPreview(state) {
    const matchValue = state.Gesture;
    if (matchValue.tag === 5) {
        const command = terrainGestureCommand(state, matchValue.fields[0], matchValue.fields[1], matchValue.fields[2], matchValue.fields[3]);
        if (command.tag === 0) {
            return [command.fields[0], command.fields[1], Result_IsOk(validateCommand(state.Map, command))];
        }
        else {
            return undefined;
        }
    }
    else {
        return undefined;
    }
}

export function unitPreview(state) {
    let tupledArg, matchValue_1, matchValue_2, current, anchor;
    let option_1;
    const matchValue = state.Gesture;
    let matchResult, units, units_1;
    switch (matchValue.tag) {
        case 3: {
            if (matchValue.fields[0].tag === 2) {
                matchResult = 0;
                units = matchValue.fields[0].fields[0];
            }
            else {
                matchResult = 2;
            }
            break;
        }
        case 4: {
            if (matchValue.fields[3].tag === 3) {
                matchResult = 1;
                units_1 = matchValue.fields[3].fields[0];
            }
            else {
                matchResult = 2;
            }
            break;
        }
        default:
            matchResult = 2;
    }
    switch (matchResult) {
        case 0: {
            option_1 = [new EditorCommand(/* AddUnits */ 2, [units]), units];
            break;
        }
        case 1: {
            option_1 = [new EditorCommand(/* UpdateUnits */ 3, [units_1]), units_1];
            break;
        }
        default:
            option_1 = undefined;
    }
    if (option_1 != null) {
        return (tupledArg = option_1, [tupledArg[1], (matchValue_1 = validateCommand(state.Map, tupledArg[0]), (matchValue_1.tag === 1) ? false : ((matchValue_2 = state.Gesture, (matchValue_2.tag === 4) ? ((current = matchValue_2.fields[1], (anchor = matchValue_2.fields[0], !movementCrossesEdge(state.Map, current.CellColumn - anchor.CellColumn, current.CellRow - anchor.CellRow, matchValue_2.fields[2])))) : true)))]);
    }
    else {
        return undefined;
    }
}

export function controllerLabel(controller) {
    switch (controller.tag) {
        case 1:
            return "Scripted AI";
        case 2:
            return "General AI";
        default:
            return "Manual";
    }
}

