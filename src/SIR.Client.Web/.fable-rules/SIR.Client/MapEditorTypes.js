
import { Record, Union } from "../fable_modules/fable-library-js.5.13.0/Types.js";
import { int64_type, option_type, bool_type, class_type, tuple_type, list_type, string_type, array_type, record_type, int32_type, union_type } from "../fable_modules/fable-library-js.5.13.0/Reflection.js";
import { Direction8_$reflection, Direction8Module_tryFromCode, Direction8Module_toCode } from "../SIR.Domain/Orientation.js";
import { BattlefieldCamera_$reflection } from "./Battlefield.js";

export class MapTerrain extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Open", "Rough", "Blocked", "Objective"];
    }
    static Open = new MapTerrain(0, []);
    static Rough = new MapTerrain(1, []);
    static Blocked = new MapTerrain(2, []);
    static Objective = new MapTerrain(3, []);
}

export function MapTerrain_$reflection() {
    return union_type("SIR.Client.MapTerrain", [], MapTerrain, () => [[], [], [], []]);
}

export class MapSide extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Blue", "Red", "NeutralSide"];
    }
    static Blue = new MapSide(0, []);
    static Red = new MapSide(1, []);
    static NeutralSide = new MapSide(2, []);
}

export function MapSide_$reflection() {
    return union_type("SIR.Client.MapSide", [], MapSide, () => [[], [], []]);
}

export class MapController extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Manual", "Scripted", "General"];
    }
    static Manual = new MapController(0, []);
    static Scripted = new MapController(1, []);
    static General = new MapController(2, []);
}

export function MapController_$reflection() {
    return union_type("SIR.Client.MapController", [], MapController, () => [[], [], []]);
}

export function MapDirectionModule_toShared(direction) {
    return direction;
}

export function MapDirectionModule_ofShared(direction) {
    return direction;
}

export function MapDirectionModule_toWireCode(direction) {
    return Direction8Module_toCode(direction);
}

export function MapDirectionModule_tryFromWireCode(code) {
    return Direction8Module_tryFromCode(code);
}

export class MapEdgeDirection extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["EastEdge", "SouthEdge"];
    }
    static EastEdge = new MapEdgeDirection(0, []);
    static SouthEdge = new MapEdgeDirection(1, []);
}

export function MapEdgeDirection_$reflection() {
    return union_type("SIR.Client.MapEdgeDirection", [], MapEdgeDirection, () => [[], []]);
}

export class MapEdgeKind extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Wall", "Door", "Window"];
    }
    static Wall = new MapEdgeKind(0, []);
    static Door = new MapEdgeKind(1, []);
    static Window$ = new MapEdgeKind(2, []);
}

export function MapEdgeKind_$reflection() {
    return union_type("SIR.Client.MapEdgeKind", [], MapEdgeKind, () => [[], [], []]);
}

export class EditorCellAddress extends Record {
    constructor(CellColumn, CellRow) {
        super();
        this.CellColumn = (CellColumn | 0);
        this.CellRow = (CellRow | 0);
    }
}

export function EditorCellAddress_$reflection() {
    return record_type("SIR.Client.EditorCellAddress", [], EditorCellAddress, () => [["CellColumn", int32_type], ["CellRow", int32_type]]);
}

export class RegionPurpose extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["ObjectiveRegion", "DeploymentZone"];
    }
    static ObjectiveRegion = new RegionPurpose(0, []);
}

export function RegionPurpose_$reflection() {
    return union_type("SIR.Client.RegionPurpose", [], RegionPurpose, () => [[], [["Item", MapSide_$reflection()]]]);
}

export class RegionGeometry extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["RegionRectangle", "RegionPolygon"];
    }
}

export function RegionGeometry_$reflection() {
    return union_type("SIR.Client.RegionGeometry", [], RegionGeometry, () => [[["column", int32_type], ["row", int32_type], ["width", int32_type], ["height", int32_type]], [["Item", array_type(EditorCellAddress_$reflection())]]]);
}

/**
 * Deliberately closed and inert in SIR-MAP 2. Future behavior must introduce
 * a reviewed, versioned case rather than embedding trusted code or macros.
 */
export class RegionBehavior extends Union {
    constructor() {
        super();
        this.tag = 0;
        this.fields = [];
    }
    cases() {
        return ["NoRegionBehavior"];
    }
    static NoRegionBehavior = new RegionBehavior();
}

export function RegionBehavior_$reflection() {
    return union_type("SIR.Client.RegionBehavior", [], RegionBehavior, () => [[]]);
}

export class MapRegion extends Record {
    constructor(Id, Geometry, Purpose, Behavior) {
        super();
        this.Id = (Id | 0);
        this.Geometry = Geometry;
        this.Purpose = Purpose;
        this.Behavior = Behavior;
    }
}

export function MapRegion_$reflection() {
    return record_type("SIR.Client.MapRegion", [], MapRegion, () => [["Id", int32_type], ["Geometry", RegionGeometry_$reflection()], ["Purpose", RegionPurpose_$reflection()], ["Behavior", RegionBehavior_$reflection()]]);
}

export class EditorUnit extends Record {
    constructor(Id, Side, ClassId, Column, Row, Size, Health, HealthMaximum, Controller, Script, ScriptIndex, BodyFacing, AttentionDirection) {
        super();
        this.Id = (Id | 0);
        this.Side = Side;
        this.ClassId = ClassId;
        this.Column = (Column | 0);
        this.Row = (Row | 0);
        this.Size = (Size | 0);
        this.Health = (Health | 0);
        this.HealthMaximum = (HealthMaximum | 0);
        this.Controller = Controller;
        this.Script = Script;
        this.ScriptIndex = (ScriptIndex | 0);
        this.BodyFacing = BodyFacing;
        this.AttentionDirection = AttentionDirection;
    }
}

export function EditorUnit_$reflection() {
    return record_type("SIR.Client.EditorUnit", [], EditorUnit, () => [["Id", int32_type], ["Side", MapSide_$reflection()], ["ClassId", string_type], ["Column", int32_type], ["Row", int32_type], ["Size", int32_type], ["Health", int32_type], ["HealthMaximum", int32_type], ["Controller", MapController_$reflection()], ["Script", list_type(Direction8_$reflection())], ["ScriptIndex", int32_type], ["BodyFacing", Direction8_$reflection()], ["AttentionDirection", Direction8_$reflection()]]);
}

export class MapDefinition extends Record {
    constructor(Width, Height, Terrain, Edges, Units, NextUnitId, Regions, NextRegionId) {
        super();
        this.Width = (Width | 0);
        this.Height = (Height | 0);
        this.Terrain = Terrain;
        this.Edges = Edges;
        this.Units = Units;
        this.NextUnitId = (NextUnitId | 0);
        this.Regions = Regions;
        this.NextRegionId = (NextRegionId | 0);
    }
}

export function MapDefinition_$reflection() {
    return record_type("SIR.Client.MapDefinition", [], MapDefinition, () => [["Width", int32_type], ["Height", int32_type], ["Terrain", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [tuple_type(int32_type, int32_type), MapTerrain_$reflection()])], ["Edges", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [tuple_type(int32_type, int32_type, MapEdgeDirection_$reflection()), tuple_type(MapEdgeKind_$reflection(), bool_type)])], ["Units", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, EditorUnit_$reflection()])], ["NextUnitId", int32_type], ["Regions", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [int32_type, MapRegion_$reflection()])], ["NextRegionId", int32_type]]);
}

export class MapEditorTool extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["Select", "Paint", "Terrain", "UnitBrowse", "Place", "Edge"];
    }
    static Select = new MapEditorTool(0, []);
    static UnitBrowse = new MapEditorTool(3, []);
}

export function MapEditorTool_$reflection() {
    return union_type("SIR.Client.MapEditorTool", [], MapEditorTool, () => [[], [["Item", MapTerrain_$reflection()]], [["Item", TerrainAuthoringTool_$reflection()]], [], [["Item1", MapSide_$reflection()], ["classId", string_type], ["size", int32_type]], [["Item1", MapEdgeDirection_$reflection()], ["Item2", MapEdgeKind_$reflection()]]]);
}

export class TerrainAuthoringTool extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["PencilTool", "RectangleTool", "LineTool", "FloodFillTool", "EyedropperTool", "EraseTool"];
    }
    static PencilTool = new TerrainAuthoringTool(0, []);
    static RectangleTool = new TerrainAuthoringTool(1, []);
    static LineTool = new TerrainAuthoringTool(2, []);
    static FloodFillTool = new TerrainAuthoringTool(3, []);
    static EyedropperTool = new TerrainAuthoringTool(4, []);
    static EraseTool = new TerrainAuthoringTool(5, []);
}

export function TerrainAuthoringTool_$reflection() {
    return union_type("SIR.Client.TerrainAuthoringTool", [], TerrainAuthoringTool, () => [[], [], [], [], [], []]);
}

export class EditorDomain extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["TerrainDomain", "EdgeDomain", "UnitDomain", "RegionDomain", "DocumentDomain"];
    }
    static TerrainDomain = new EditorDomain(0, []);
    static EdgeDomain = new EditorDomain(1, []);
    static UnitDomain = new EditorDomain(2, []);
    static RegionDomain = new EditorDomain(3, []);
    static DocumentDomain = new EditorDomain(4, []);
}

export function EditorDomain_$reflection() {
    return union_type("SIR.Client.EditorDomain", [], EditorDomain, () => [[], [], [], [], []]);
}

export class EditorLayerState extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["VisibleLayer", "DimmedLayer", "HiddenLayer", "LockedLayer"];
    }
    static VisibleLayer = new EditorLayerState(0, []);
    static DimmedLayer = new EditorLayerState(1, []);
    static HiddenLayer = new EditorLayerState(2, []);
    static LockedLayer = new EditorLayerState(3, []);
}

export function EditorLayerState_$reflection() {
    return union_type("SIR.Client.EditorLayerState", [], EditorLayerState, () => [[], [], [], []]);
}

export class SavedMapView extends Record {
    constructor(Name, Camera) {
        super();
        this.Name = Name;
        this.Camera = Camera;
    }
}

export function SavedMapView_$reflection() {
    return record_type("SIR.Client.SavedMapView", [], SavedMapView, () => [["Name", string_type], ["Camera", BattlefieldCamera_$reflection()]]);
}

export class MapAuthoringMetadata extends Record {
    constructor(Name, SavedViews, RevisionIdentity, ThumbnailSvg) {
        super();
        this.Name = Name;
        this.SavedViews = SavedViews;
        this.RevisionIdentity = RevisionIdentity;
        this.ThumbnailSvg = ThumbnailSvg;
    }
}

export function MapAuthoringMetadata_$reflection() {
    return record_type("SIR.Client.MapAuthoringMetadata", [], MapAuthoringMetadata, () => [["Name", string_type], ["SavedViews", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [string_type, SavedMapView_$reflection()])], ["RevisionIdentity", string_type], ["ThumbnailSvg", option_type(string_type)]]);
}

export class ResizeLossPreview extends Record {
    constructor(TargetWidth, TargetHeight, LostTerrainCells, LostEdges, LostUnits, LostRegions) {
        super();
        this.TargetWidth = (TargetWidth | 0);
        this.TargetHeight = (TargetHeight | 0);
        this.LostTerrainCells = (LostTerrainCells | 0);
        this.LostEdges = (LostEdges | 0);
        this.LostUnits = (LostUnits | 0);
        this.LostRegions = (LostRegions | 0);
    }
}

export function ResizeLossPreview_$reflection() {
    return record_type("SIR.Client.ResizeLossPreview", [], ResizeLossPreview, () => [["TargetWidth", int32_type], ["TargetHeight", int32_type], ["LostTerrainCells", int32_type], ["LostEdges", int32_type], ["LostUnits", int32_type], ["LostRegions", int32_type]]);
}

export class PendingDestructiveChange extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["ResizePending", "ClearPending", "NewMapPending", "UnitDeletionPending"];
    }
    static ClearPending = new PendingDestructiveChange(1, []);
}

export function PendingDestructiveChange_$reflection() {
    return union_type("SIR.Client.PendingDestructiveChange", [], PendingDestructiveChange, () => [[["Item", ResizeLossPreview_$reflection()]], [], [["width", int32_type], ["height", int32_type], ["name", string_type]], [["identifiers", array_type(int32_type)]]]);
}

export class CrashRecoveryDraft extends Record {
    constructor(SourceDigest, Map$) {
        super();
        this.SourceDigest = SourceDigest;
        this.Map = Map$;
    }
}

export function CrashRecoveryDraft_$reflection() {
    return record_type("SIR.Client.CrashRecoveryDraft", [], CrashRecoveryDraft, () => [["SourceDigest", string_type], ["Map", MapDefinition_$reflection()]]);
}

export class EditorBox extends Record {
    constructor(FirstColumn, FirstRow, LastColumn, LastRow) {
        super();
        this.FirstColumn = (FirstColumn | 0);
        this.FirstRow = (FirstRow | 0);
        this.LastColumn = (LastColumn | 0);
        this.LastRow = (LastRow | 0);
    }
}

export function EditorBox_$reflection() {
    return record_type("SIR.Client.EditorBox", [], EditorBox, () => [["FirstColumn", int32_type], ["FirstRow", int32_type], ["LastColumn", int32_type], ["LastRow", int32_type]]);
}

export class EditorKeyboardObject extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["KeyboardUnit", "KeyboardRegion", "KeyboardEdge", "KeyboardTerrain"];
    }
}

export function EditorKeyboardObject_$reflection() {
    return union_type("SIR.Client.EditorKeyboardObject", [], EditorKeyboardObject, () => [[["id", int32_type]], [["id", int32_type]], [["column", int32_type], ["row", int32_type], ["direction", MapEdgeDirection_$reflection()]], [["Item", EditorCellAddress_$reflection()]]]);
}

export class EditorKeyboardCursor extends Record {
    constructor(Cell, ObjectCycleIndex) {
        super();
        this.Cell = Cell;
        this.ObjectCycleIndex = (ObjectCycleIndex | 0);
    }
}

export function EditorKeyboardCursor_$reflection() {
    return record_type("SIR.Client.EditorKeyboardCursor", [], EditorKeyboardCursor, () => [["Cell", EditorCellAddress_$reflection()], ["ObjectCycleIndex", int32_type]]);
}

export class UnitPaletteCursor extends Record {
    constructor(PresetId, FactionIndex, ResultIndex) {
        super();
        this.PresetId = PresetId;
        this.FactionIndex = (FactionIndex | 0);
        this.ResultIndex = (ResultIndex | 0);
    }
}

export function UnitPaletteCursor_$reflection() {
    return record_type("SIR.Client.UnitPaletteCursor", [], UnitPaletteCursor, () => [["PresetId", option_type(string_type)], ["FactionIndex", int32_type], ["ResultIndex", int32_type]]);
}

export class RegionShape extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["RectangleRegionShape", "PolygonRegionShape"];
    }
    static RectangleRegionShape = new RegionShape(0, []);
    static PolygonRegionShape = new RegionShape(1, []);
}

export function RegionShape_$reflection() {
    return union_type("SIR.Client.RegionShape", [], RegionShape, () => [[], []]);
}

/**
 * Keyboard-only presentation state for nested region construction and
 * resettable edit previews. It is intentionally excluded from MapDefinition.
 */
export class RegionKeyboardMode extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["RegionIdle", "RegionPurposeSelection", "RegionShapeSelection", "RegionRectangleConstruction", "RegionPolygonConstruction", "RegionMovePreview", "RegionResizePreview", "RegionVertexPreview"];
    }
    static RegionIdle = new RegionKeyboardMode(0, []);
}

export function RegionKeyboardMode_$reflection() {
    return union_type("SIR.Client.RegionKeyboardMode", [], RegionKeyboardMode, () => [[], [["editingExisting", bool_type], ["highlighted", RegionPurpose_$reflection()]], [["purpose", RegionPurpose_$reflection()]], [["purpose", RegionPurpose_$reflection()], ["anchor", option_type(EditorCellAddress_$reflection())]], [["purpose", RegionPurpose_$reflection()], ["vertices", array_type(EditorCellAddress_$reflection())]], [["original", RegionGeometry_$reflection()], ["preview", RegionGeometry_$reflection()]], [["original", RegionGeometry_$reflection()], ["preview", RegionGeometry_$reflection()]], [["original", RegionGeometry_$reflection()], ["preview", RegionGeometry_$reflection()], ["activeIndex", int32_type]]]);
}

export class EditorGesture extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["IdleGesture", "SelectedObjectActionsGesture", "BoxSelectionGesture", "CommandPreviewGesture", "UnitMoveGesture", "TerrainGesture", "EdgePolylineGesture"];
    }
    static IdleGesture = new EditorGesture(0, []);
    static SelectedObjectActionsGesture = new EditorGesture(1, []);
}

export function EditorGesture_$reflection() {
    return union_type("SIR.Client.EditorGesture", [], EditorGesture, () => [[], [], [["anchor", EditorCellAddress_$reflection()], ["current", EditorCellAddress_$reflection()]], [["Item", EditorCommand_$reflection()]], [["anchor", EditorCellAddress_$reflection()], ["current", EditorCellAddress_$reflection()], ["original", array_type(EditorUnit_$reflection())], ["command", EditorCommand_$reflection()]], [["tool", TerrainAuthoringTool_$reflection()], ["anchor", EditorCellAddress_$reflection()], ["current", EditorCellAddress_$reflection()], ["visited", array_type(EditorCellAddress_$reflection())]], [["kind", MapEdgeKind_$reflection()], ["segments", array_type(tuple_type(int32_type, int32_type, MapEdgeDirection_$reflection()))]]]);
}

export class EditorCommand extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["PaintCells", "ReplaceEdges", "AddUnits", "UpdateUnits", "RemoveUnits", "AddRegions", "UpdateRegions", "RemoveRegions", "ResizeDocument", "ReplaceDocument"];
    }
}

export function EditorCommand_$reflection() {
    return union_type("SIR.Client.EditorCommand", [], EditorCommand, () => [[["Item1", MapTerrain_$reflection()], ["Item2", array_type(EditorCellAddress_$reflection())]], [["Item", array_type(tuple_type(tuple_type(int32_type, int32_type, MapEdgeDirection_$reflection()), option_type(tuple_type(MapEdgeKind_$reflection(), bool_type))))]], [["Item", array_type(EditorUnit_$reflection())]], [["Item", array_type(EditorUnit_$reflection())]], [["Item", array_type(int32_type)]], [["Item", array_type(MapRegion_$reflection())]], [["Item", array_type(MapRegion_$reflection())]], [["Item", array_type(int32_type)]], [["width", int32_type], ["height", int32_type]], [["reason", string_type], ["Item2", MapDefinition_$reflection()]]]);
}

export class MapIssue extends Record {
    constructor(Code, Message) {
        super();
        this.Code = Code;
        this.Message = Message;
    }
}

export function MapIssue_$reflection() {
    return record_type("SIR.Client.MapIssue", [], MapIssue, () => [["Code", string_type], ["Message", string_type]]);
}

export class ValidatedEditorCommand extends Union {
    constructor(Item) {
        super();
        this.tag = 0;
        this.fields = [Item];
    }
    cases() {
        return ["ValidatedEditorCommand"];
    }
}

export function ValidatedEditorCommand_$reflection() {
    return union_type("SIR.Client.ValidatedEditorCommand", [], ValidatedEditorCommand, () => [[["Item", EditorCommand_$reflection()]]]);
}

export class MapRevision extends Record {
    constructor(Number$, ParentDigest, Document$, Digest) {
        super();
        this.Number = Number$;
        this.ParentDigest = ParentDigest;
        this.Document = Document$;
        this.Digest = Digest;
    }
}

export function MapRevision_$reflection() {
    return record_type("SIR.Client.MapRevision", [], MapRevision, () => [["Number", int64_type], ["ParentDigest", option_type(string_type)], ["Document", MapDefinition_$reflection()], ["Digest", string_type]]);
}

export class RevisionState extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["DirtyRevision", "SavedRevision", "SimulatedRevision", "RecoveredRevision"];
    }
    static DirtyRevision = new RevisionState(0, []);
    static SavedRevision = new RevisionState(1, []);
    static SimulatedRevision = new RevisionState(2, []);
    static RecoveredRevision = new RevisionState(3, []);
}

export function RevisionState_$reflection() {
    return union_type("SIR.Client.RevisionState", [], RevisionState, () => [[], [], [], []]);
}

export class EditorHistoryEntry extends Record {
    constructor(Command, Before, After, SerializedBytes) {
        super();
        this.Command = Command;
        this.Before = Before;
        this.After = After;
        this.SerializedBytes = (SerializedBytes | 0);
    }
}

export function EditorHistoryEntry_$reflection() {
    return record_type("SIR.Client.EditorHistoryEntry", [], EditorHistoryEntry, () => [["Command", EditorCommand_$reflection()], ["Before", MapRevision_$reflection()], ["After", MapRevision_$reflection()], ["SerializedBytes", int32_type]]);
}

export class EditorClipboard extends Record {
    constructor(SourceDigest, UnitFragment) {
        super();
        this.SourceDigest = SourceDigest;
        this.UnitFragment = UnitFragment;
    }
}

export function EditorClipboard_$reflection() {
    return record_type("SIR.Client.EditorClipboard", [], EditorClipboard, () => [["SourceDigest", string_type], ["UnitFragment", array_type(EditorUnit_$reflection())]]);
}

export class MapUnitFootprintPreset extends Record {
    constructor(Id, Name, Faction, Role, ClassId, GlyphId, Side, FootprintSize, Health, HealthMaximum) {
        super();
        this.Id = Id;
        this.Name = Name;
        this.Faction = Faction;
        this.Role = Role;
        this.ClassId = ClassId;
        this.GlyphId = GlyphId;
        this.Side = Side;
        this.FootprintSize = (FootprintSize | 0);
        this.Health = (Health | 0);
        this.HealthMaximum = (HealthMaximum | 0);
    }
}

export function MapUnitFootprintPreset_$reflection() {
    return record_type("SIR.Client.MapUnitFootprintPreset", [], MapUnitFootprintPreset, () => [["Id", string_type], ["Name", string_type], ["Faction", string_type], ["Role", string_type], ["ClassId", string_type], ["GlyphId", string_type], ["Side", MapSide_$reflection()], ["FootprintSize", int32_type], ["Health", int32_type], ["HealthMaximum", int32_type]]);
}

export class MapEditorState extends Record {
    constructor(Map$, Tool, TerrainSelection, BrushSize, TerrainCursor, KeyboardCursor, KeyboardObject, LastTerrainPaintTool, TerrainAnnouncement, EdgeCursor, EdgeAnnouncement, UnitPaletteSearch, UnitPaletteCursor, UnitPlacementCursor, UnitAnnouncement, RegionAnnouncement, RegionKeyboardMode, SelectedUnit, SelectedUnits, SelectedRegion, Gesture, Revision, RevisionState, SavedDigest, SimulatedDigest, RecoveredFromDigest, UndoHistory, RedoHistory, HistoryBytes, Clipboard, Tick, IsRunning, LastEvents, Validation, Layers, Issues, ActiveIssue, PendingDestructiveChange, PendingRecovery, Authoring) {
        super();
        this.Map = Map$;
        this.Tool = Tool;
        this.TerrainSelection = TerrainSelection;
        this.BrushSize = (BrushSize | 0);
        this.TerrainCursor = TerrainCursor;
        this.KeyboardCursor = KeyboardCursor;
        this.KeyboardObject = KeyboardObject;
        this.LastTerrainPaintTool = LastTerrainPaintTool;
        this.TerrainAnnouncement = TerrainAnnouncement;
        this.EdgeCursor = EdgeCursor;
        this.EdgeAnnouncement = EdgeAnnouncement;
        this.UnitPaletteSearch = UnitPaletteSearch;
        this.UnitPaletteCursor = UnitPaletteCursor;
        this.UnitPlacementCursor = UnitPlacementCursor;
        this.UnitAnnouncement = UnitAnnouncement;
        this.RegionAnnouncement = RegionAnnouncement;
        this.RegionKeyboardMode = RegionKeyboardMode;
        this.SelectedUnit = SelectedUnit;
        this.SelectedUnits = SelectedUnits;
        this.SelectedRegion = SelectedRegion;
        this.Gesture = Gesture;
        this.Revision = Revision;
        this.RevisionState = RevisionState;
        this.SavedDigest = SavedDigest;
        this.SimulatedDigest = SimulatedDigest;
        this.RecoveredFromDigest = RecoveredFromDigest;
        this.UndoHistory = UndoHistory;
        this.RedoHistory = RedoHistory;
        this.HistoryBytes = (HistoryBytes | 0);
        this.Clipboard = Clipboard;
        this.Tick = (Tick | 0);
        this.IsRunning = IsRunning;
        this.LastEvents = LastEvents;
        this.Validation = Validation;
        this.Layers = Layers;
        this.Issues = Issues;
        this.ActiveIssue = ActiveIssue;
        this.PendingDestructiveChange = PendingDestructiveChange;
        this.PendingRecovery = PendingRecovery;
        this.Authoring = Authoring;
    }
}

export function MapEditorState_$reflection() {
    return record_type("SIR.Client.MapEditorState", [], MapEditorState, () => [["Map", MapDefinition_$reflection()], ["Tool", MapEditorTool_$reflection()], ["TerrainSelection", MapTerrain_$reflection()], ["BrushSize", int32_type], ["TerrainCursor", EditorCellAddress_$reflection()], ["KeyboardCursor", EditorKeyboardCursor_$reflection()], ["KeyboardObject", option_type(EditorKeyboardObject_$reflection())], ["LastTerrainPaintTool", TerrainAuthoringTool_$reflection()], ["TerrainAnnouncement", string_type], ["EdgeCursor", tuple_type(int32_type, int32_type, MapEdgeDirection_$reflection())], ["EdgeAnnouncement", string_type], ["UnitPaletteSearch", string_type], ["UnitPaletteCursor", UnitPaletteCursor_$reflection()], ["UnitPlacementCursor", EditorCellAddress_$reflection()], ["UnitAnnouncement", string_type], ["RegionAnnouncement", string_type], ["RegionKeyboardMode", RegionKeyboardMode_$reflection()], ["SelectedUnit", option_type(int32_type)], ["SelectedUnits", class_type("Microsoft.FSharp.Collections.FSharpSet`1", [int32_type])], ["SelectedRegion", option_type(int32_type)], ["Gesture", EditorGesture_$reflection()], ["Revision", MapRevision_$reflection()], ["RevisionState", RevisionState_$reflection()], ["SavedDigest", option_type(string_type)], ["SimulatedDigest", option_type(string_type)], ["RecoveredFromDigest", option_type(string_type)], ["UndoHistory", list_type(EditorHistoryEntry_$reflection())], ["RedoHistory", list_type(EditorHistoryEntry_$reflection())], ["HistoryBytes", int32_type], ["Clipboard", option_type(EditorClipboard_$reflection())], ["Tick", int32_type], ["IsRunning", bool_type], ["LastEvents", list_type(string_type)], ["Validation", option_type(string_type)], ["Layers", class_type("Microsoft.FSharp.Collections.FSharpMap`2", [EditorDomain_$reflection(), EditorLayerState_$reflection()])], ["Issues", array_type(MapIssue_$reflection())], ["ActiveIssue", option_type(int32_type)], ["PendingDestructiveChange", option_type(PendingDestructiveChange_$reflection())], ["PendingRecovery", option_type(CrashRecoveryDraft_$reflection())], ["Authoring", MapAuthoringMetadata_$reflection()]]);
}

export class MapEditorAction extends Union {
    constructor(tag, fields) {
        super();
        this.tag = tag;
        this.fields = fields;
    }
    cases() {
        return ["ChooseTool", "ChooseTerrain", "SetTerrainBrushSize", "MoveTerrainCursor", "ActivateTerrainCursor", "ResetTerrainPreview", "MoveEditorKeyboardCursor", "CycleEditorKeyboardObject", "ActivateEditorKeyboardCursor", "OpenSelectedObjectActions", "BeginKeyboardBoxSelection", "AddEditorUnitsInBox", "ActivateEdge", "MoveEdgeCursor", "ActivateEdgeCursor", "FinishEdgePolyline", "BacktrackEdgePolyline", "ConvertEdge", "ToggleDoorState", "EraseEdge", "SplitEdge", "JoinEdge", "SetUnitPaletteSearch", "MoveUnitPaletteCursor", "PageUnitPaletteFaction", "SelectUnitPaletteBoundary", "ArmUnitPalettePreset", "ReturnToUnitBrowse", "MoveUnitPlacementCursor", "CycleArmedUnitPreset", "CommitUnitPlacement", "ResetUnitMovePreview", "MoveRegionCursor", "BeginNewRegion", "ChooseRegionPurpose", "ChooseRegionShape", "ActivateRegionCursor", "CommitRegionPolygon", "BacktrackRegionConstruction", "BeginSelectedRegionMove", "BeginSelectedRegionResize", "BeginSelectedRegionVertexEdit", "BeginSelectedRegionPurposeEdit", "MoveRegionEditPreview", "CycleRegionVertex", "ResetRegionEditPreview", "CommitRegionEditPreview", "CancelRegionKeyboardMode", "CreateRectangleRegion", "CreatePolygonRegion", "SelectEditorRegion", "SetSelectedRegionPurpose", "SetSelectedRegionGeometry", "MoveSelectedRegion", "MoveSelectedRegionVertex", "RemoveSelectedRegion", "PreviewUnitPlacement", "BeginUnitMove", "ExtendUnitMove", "BeginTerrainGesture", "ExtendTerrainGesture", "ActivateCell", "Resize", "RequestNewMap", "RequestClearMap", "ConfirmDestructiveChange", "CancelDestructiveChange", "SetEditorLayerState", "SelectNextIssue", "SelectPreviousIssue", "SetMapName", "SaveMapView", "RemoveMapView", "SetMapThumbnail", "OfferCrashRecovery", "RecoverCrashDraft", "DiscardCrashDraft", "SelectEditorUnit", "ToggleEditorUnitSelection", "SelectEditorUnitsInBox", "BeginEditorBoxSelection", "ExtendEditorBoxSelection", "CommitEditorGesture", "CancelEditorGesture", "SelectAllInActiveDomain", "UndoEditorCommand", "RedoEditorCommand", "CopyEditorSelection", "PasteEditorClipboard", "DuplicateEditorSelection", "DeleteEditorSelection", "MarkEditorSaved", "MarkEditorSimulated", "MarkEditorRecovered", "RestoreEditorDraft", "RemoveSelectedUnit", "SetSelectedSide", "SetSelectedClass", "SetSelectedSize", "SetSelectedHealth", "SetSelectedController", "SetSelectedScript", "MoveSelected", "ToggleEditorRun", "StepEditor", "ClearMap", "LoadMapText"];
    }
    static ActivateTerrainCursor = new MapEditorAction(4, []);
    static ResetTerrainPreview = new MapEditorAction(5, []);
    static OpenSelectedObjectActions = new MapEditorAction(9, []);
    static BeginKeyboardBoxSelection = new MapEditorAction(10, []);
    static ActivateEdgeCursor = new MapEditorAction(14, []);
    static FinishEdgePolyline = new MapEditorAction(15, []);
    static BacktrackEdgePolyline = new MapEditorAction(16, []);
    static ArmUnitPalettePreset = new MapEditorAction(26, []);
    static ReturnToUnitBrowse = new MapEditorAction(27, []);
    static ResetUnitMovePreview = new MapEditorAction(31, []);
    static BeginNewRegion = new MapEditorAction(33, []);
    static ActivateRegionCursor = new MapEditorAction(36, []);
    static CommitRegionPolygon = new MapEditorAction(37, []);
    static BacktrackRegionConstruction = new MapEditorAction(38, []);
    static BeginSelectedRegionMove = new MapEditorAction(39, []);
    static BeginSelectedRegionResize = new MapEditorAction(40, []);
    static BeginSelectedRegionVertexEdit = new MapEditorAction(41, []);
    static BeginSelectedRegionPurposeEdit = new MapEditorAction(42, []);
    static ResetRegionEditPreview = new MapEditorAction(45, []);
    static CommitRegionEditPreview = new MapEditorAction(46, []);
    static CancelRegionKeyboardMode = new MapEditorAction(47, []);
    static RemoveSelectedRegion = new MapEditorAction(55, []);
    static RequestNewMap = new MapEditorAction(63, []);
    static RequestClearMap = new MapEditorAction(64, []);
    static ConfirmDestructiveChange = new MapEditorAction(65, []);
    static CancelDestructiveChange = new MapEditorAction(66, []);
    static SelectNextIssue = new MapEditorAction(68, []);
    static SelectPreviousIssue = new MapEditorAction(69, []);
    static RecoverCrashDraft = new MapEditorAction(75, []);
    static DiscardCrashDraft = new MapEditorAction(76, []);
    static CommitEditorGesture = new MapEditorAction(82, []);
    static CancelEditorGesture = new MapEditorAction(83, []);
    static SelectAllInActiveDomain = new MapEditorAction(84, []);
    static UndoEditorCommand = new MapEditorAction(85, []);
    static RedoEditorCommand = new MapEditorAction(86, []);
    static CopyEditorSelection = new MapEditorAction(87, []);
    static PasteEditorClipboard = new MapEditorAction(88, []);
    static DuplicateEditorSelection = new MapEditorAction(89, []);
    static DeleteEditorSelection = new MapEditorAction(90, []);
    static MarkEditorSaved = new MapEditorAction(91, []);
    static MarkEditorSimulated = new MapEditorAction(92, []);
    static RestoreEditorDraft = new MapEditorAction(94, []);
    static RemoveSelectedUnit = new MapEditorAction(95, []);
    static ToggleEditorRun = new MapEditorAction(103, []);
    static StepEditor = new MapEditorAction(104, []);
    static ClearMap = new MapEditorAction(105, []);
}

export function MapEditorAction_$reflection() {
    return union_type("SIR.Client.MapEditorAction", [], MapEditorAction, () => [[["Item", MapEditorTool_$reflection()]], [["Item", MapTerrain_$reflection()]], [["Item", int32_type]], [["columnDelta", int32_type], ["rowDelta", int32_type], ["extendPreview", bool_type]], [], [], [["columnDelta", int32_type], ["rowDelta", int32_type]], [["delta", int32_type]], [["toggle", bool_type]], [], [], [["Item", EditorBox_$reflection()]], [["column", int32_type], ["row", int32_type], ["direction", MapEdgeDirection_$reflection()]], [["columnDelta", int32_type], ["rowDelta", int32_type], ["extendPreview", bool_type]], [], [], [], [["column", int32_type], ["row", int32_type], ["direction", MapEdgeDirection_$reflection()], ["kind", MapEdgeKind_$reflection()]], [["column", int32_type], ["row", int32_type], ["direction", MapEdgeDirection_$reflection()]], [["column", int32_type], ["row", int32_type], ["direction", MapEdgeDirection_$reflection()]], [["column", int32_type], ["row", int32_type], ["direction", MapEdgeDirection_$reflection()]], [["column", int32_type], ["row", int32_type], ["direction", MapEdgeDirection_$reflection()]], [["Item", string_type]], [["delta", int32_type]], [["delta", int32_type]], [["last", bool_type]], [], [], [["columnDelta", int32_type], ["rowDelta", int32_type]], [["delta", int32_type]], [["returnToBrowse", bool_type]], [], [["columnDelta", int32_type], ["rowDelta", int32_type]], [], [["Item", RegionPurpose_$reflection()]], [["Item", RegionShape_$reflection()]], [], [], [], [], [], [], [], [["columnDelta", int32_type], ["rowDelta", int32_type], ["fromOppositeOrigin", bool_type]], [["delta", int32_type]], [], [], [], [["Item1", RegionPurpose_$reflection()], ["first", EditorCellAddress_$reflection()], ["last", EditorCellAddress_$reflection()]], [["Item1", RegionPurpose_$reflection()], ["vertices", array_type(EditorCellAddress_$reflection())]], [["Item", option_type(int32_type)]], [["Item", RegionPurpose_$reflection()]], [["Item", RegionGeometry_$reflection()]], [["columnDelta", int32_type], ["rowDelta", int32_type]], [["index", int32_type], ["columnDelta", int32_type], ["rowDelta", int32_type]], [], [["Item", EditorCellAddress_$reflection()]], [["Item", EditorCellAddress_$reflection()]], [["Item", EditorCellAddress_$reflection()]], [["Item", EditorCellAddress_$reflection()]], [["Item", EditorCellAddress_$reflection()]], [["column", int32_type], ["row", int32_type]], [["width", int32_type], ["height", int32_type]], [], [], [], [], [["Item1", EditorDomain_$reflection()], ["Item2", EditorLayerState_$reflection()]], [], [], [["Item", string_type]], [["name", string_type], ["camera", BattlefieldCamera_$reflection()]], [["Item", string_type]], [["Item", option_type(string_type)]], [["Item", string_type]], [], [], [["Item", option_type(int32_type)]], [["Item", int32_type]], [["Item", EditorBox_$reflection()]], [["Item", EditorCellAddress_$reflection()]], [["Item", EditorCellAddress_$reflection()]], [], [], [], [], [], [], [], [], [], [], [], [["sourceDigest", string_type]], [], [], [["Item", MapSide_$reflection()]], [["Item", string_type]], [["Item", int32_type]], [["remaining", int32_type], ["maximum", int32_type]], [["Item", MapController_$reflection()]], [["Item", string_type]], [["Item", Direction8_$reflection()]], [], [], [], [["Item", string_type]]]);
}

