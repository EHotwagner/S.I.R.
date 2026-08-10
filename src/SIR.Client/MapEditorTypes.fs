namespace SIR.Client

open System
open System.Text
open SIR.Domain

type MapTerrain =
    | Open
    | Rough
    | Blocked
    | Objective

type MapSide =
    | Blue
    | Red
    | NeutralSide

type MapController =
    | Manual
    | Scripted
    | General

/// Editor compatibility name. The authoritative direction type is shared.
type MapDirection = Direction8

[<RequireQualifiedAccess>]
module MapDirection =
    let toShared (direction: MapDirection) : Direction8 = direction
    let ofShared (direction: Direction8) : MapDirection = direction
    let toWireCode direction = Direction8.toCode direction
    let tryFromWireCode code = Direction8.tryFromCode code

type MapEdgeDirection =
    | EastEdge
    | SouthEdge

type MapEdgeKind =
    | Wall
    | Door
    | Window

type EditorCellAddress =
    { CellColumn: int32
      CellRow: int32 }

type RegionPurpose =
    | ObjectiveRegion
    | DeploymentZone of MapSide

type RegionGeometry =
    | RegionRectangle of column: int32 * row: int32 * width: int32 * height: int32
    | RegionPolygon of EditorCellAddress array

/// Deliberately closed and inert in SIR-MAP 2. Future behavior must introduce
/// a reviewed, versioned case rather than embedding trusted code or macros.
type RegionBehavior =
    | NoRegionBehavior

type MapRegion =
    { Id: int32
      Geometry: RegionGeometry
      Purpose: RegionPurpose
      Behavior: RegionBehavior }

type EditorUnit =
    { Id: int32
      Side: MapSide
      ClassId: string
      Column: int32
      Row: int32
      Size: int32
      Health: int32
      HealthMaximum: int32
      Controller: MapController
      Script: MapDirection list
      ScriptIndex: int
      BodyFacing: Direction8
      AttentionDirection: Direction8 }

type MapDefinition =
    { Width: int32
      Height: int32
      Terrain: Map<int32 * int32, MapTerrain>
      Edges: Map<int32 * int32 * MapEdgeDirection, MapEdgeKind * bool>
      Units: Map<int32, EditorUnit>
      NextUnitId: int32
      Regions: Map<int32, MapRegion>
      NextRegionId: int32 }

type MapEditorTool =
    | Select
    | Paint of MapTerrain
    | Terrain of TerrainAuthoringTool
    | UnitBrowse
    | Place of MapSide * classId: string * size: int32
    | Edge of MapEdgeDirection * MapEdgeKind

and TerrainAuthoringTool =
    | PencilTool
    | RectangleTool
    | LineTool
    | FloodFillTool
    | EyedropperTool
    | EraseTool

type EditorDomain =
    | TerrainDomain
    | EdgeDomain
    | UnitDomain
    | RegionDomain
    | DocumentDomain

type EditorLayerState =
    | VisibleLayer
    | DimmedLayer
    | HiddenLayer
    | LockedLayer

type SavedMapView =
    { Name: string
      Camera: BattlefieldCamera }

type MapAuthoringMetadata =
    { Name: string
      SavedViews: Map<string, SavedMapView>
      RevisionIdentity: string
      ThumbnailSvg: string option }

type ResizeLossPreview =
    { TargetWidth: int32
      TargetHeight: int32
      LostTerrainCells: int
      LostEdges: int
      LostUnits: int
      LostRegions: int }

type PendingDestructiveChange =
    | ResizePending of ResizeLossPreview
    | ClearPending
    | NewMapPending of width: int32 * height: int32 * name: string
    | UnitDeletionPending of identifiers: int32 array

type CrashRecoveryDraft =
    { SourceDigest: string
      Map: MapDefinition }

type EditorBox =
    { FirstColumn: int32
      FirstRow: int32
      LastColumn: int32
      LastRow: int32 }

type EditorKeyboardObject =
    | KeyboardUnit of id: int32
    | KeyboardRegion of id: int32
    | KeyboardEdge of column: int32 * row: int32 * direction: MapEdgeDirection
    | KeyboardTerrain of EditorCellAddress

type EditorKeyboardCursor =
    { Cell: EditorCellAddress
      ObjectCycleIndex: int }

type UnitPaletteCursor =
    { PresetId: string option
      FactionIndex: int
      ResultIndex: int }

type RegionShape =
    | RectangleRegionShape
    | PolygonRegionShape

/// Keyboard-only presentation state for nested region construction and
/// resettable edit previews. It is intentionally excluded from MapDefinition.
type RegionKeyboardMode =
    | RegionIdle
    | RegionPurposeSelection of editingExisting: bool * highlighted: RegionPurpose
    | RegionShapeSelection of purpose: RegionPurpose
    | RegionRectangleConstruction of
        purpose: RegionPurpose *
        anchor: EditorCellAddress option
    | RegionPolygonConstruction of
        purpose: RegionPurpose *
        vertices: EditorCellAddress array
    | RegionMovePreview of original: RegionGeometry * preview: RegionGeometry
    | RegionResizePreview of original: RegionGeometry * preview: RegionGeometry
    | RegionVertexPreview of
        original: RegionGeometry *
        preview: RegionGeometry *
        activeIndex: int

type EditorGesture =
    | IdleGesture
    | SelectedObjectActionsGesture
    | BoxSelectionGesture of anchor: EditorCellAddress * current: EditorCellAddress
    | CommandPreviewGesture of EditorCommand
    | UnitMoveGesture of
        anchor: EditorCellAddress *
        current: EditorCellAddress *
        original: EditorUnit array *
        command: EditorCommand
    | TerrainGesture of
        tool: TerrainAuthoringTool *
        anchor: EditorCellAddress *
        current: EditorCellAddress *
        visited: EditorCellAddress array
    | EdgePolylineGesture of
        kind: MapEdgeKind *
        segments: (int32 * int32 * MapEdgeDirection) array

and EditorCommand =
    | PaintCells of MapTerrain * EditorCellAddress array
    | ReplaceEdges of ((int32 * int32 * MapEdgeDirection) * (MapEdgeKind * bool) option) array
    | AddUnits of EditorUnit array
    | UpdateUnits of EditorUnit array
    | RemoveUnits of int32 array
    | AddRegions of MapRegion array
    | UpdateRegions of MapRegion array
    | RemoveRegions of int32 array
    | ResizeDocument of width: int32 * height: int32
    | ReplaceDocument of reason: string * MapDefinition

type MapIssue =
    { Code: string
      Message: string }

type ValidatedEditorCommand = private ValidatedEditorCommand of EditorCommand

type MapRevision =
    { Number: int64
      ParentDigest: string option
      Document: MapDefinition
      Digest: string }

type RevisionState =
    | DirtyRevision
    | SavedRevision
    | SimulatedRevision
    | RecoveredRevision

type EditorHistoryEntry =
    { Command: EditorCommand
      Before: MapRevision
      After: MapRevision
      SerializedBytes: int }

type EditorClipboard =
    { SourceDigest: string
      UnitFragment: EditorUnit array }

type MapUnitFootprintPreset =
    { Id: string
      Name: string
      Faction: string
      Role: string
      ClassId: string
      GlyphId: string
      Side: MapSide
      FootprintSize: int32
      Health: int32
      HealthMaximum: int32 }

type MapEditorState =
    { Map: MapDefinition
      Tool: MapEditorTool
      TerrainSelection: MapTerrain
      BrushSize: int32
      TerrainCursor: EditorCellAddress
      KeyboardCursor: EditorKeyboardCursor
      KeyboardObject: EditorKeyboardObject option
      LastTerrainPaintTool: TerrainAuthoringTool
      TerrainAnnouncement: string
      EdgeCursor: int32 * int32 * MapEdgeDirection
      EdgeAnnouncement: string
      UnitPaletteSearch: string
      UnitPaletteCursor: UnitPaletteCursor
      UnitPlacementCursor: EditorCellAddress
      UnitAnnouncement: string
      RegionAnnouncement: string
      RegionKeyboardMode: RegionKeyboardMode
      SelectedUnit: int32 option
      SelectedUnits: Set<int32>
      SelectedRegion: int32 option
      Gesture: EditorGesture
      Revision: MapRevision
      RevisionState: RevisionState
      SavedDigest: string option
      SimulatedDigest: string option
      RecoveredFromDigest: string option
      UndoHistory: EditorHistoryEntry list
      RedoHistory: EditorHistoryEntry list
      HistoryBytes: int
      Clipboard: EditorClipboard option
      Tick: int32
      IsRunning: bool
      LastEvents: string list
      Validation: string option
      Layers: Map<EditorDomain, EditorLayerState>
      Issues: MapIssue array
      ActiveIssue: int option
      PendingDestructiveChange: PendingDestructiveChange option
      PendingRecovery: CrashRecoveryDraft option
      Authoring: MapAuthoringMetadata }

type MapEditorAction =
    | ChooseTool of MapEditorTool
    | ChooseTerrain of MapTerrain
    | SetTerrainBrushSize of int32
    | MoveTerrainCursor of columnDelta: int32 * rowDelta: int32 * extendPreview: bool
    | ActivateTerrainCursor
    | ResetTerrainPreview
    | MoveEditorKeyboardCursor of columnDelta: int32 * rowDelta: int32
    | CycleEditorKeyboardObject of delta: int
    | ActivateEditorKeyboardCursor of toggle: bool
    | OpenSelectedObjectActions
    | BeginKeyboardBoxSelection
    | AddEditorUnitsInBox of EditorBox
    | ActivateEdge of column: int32 * row: int32 * direction: MapEdgeDirection
    | MoveEdgeCursor of columnDelta: int32 * rowDelta: int32 * extendPreview: bool
    | ActivateEdgeCursor
    | FinishEdgePolyline
    | BacktrackEdgePolyline
    | ConvertEdge of column: int32 * row: int32 * direction: MapEdgeDirection * kind: MapEdgeKind
    | ToggleDoorState of column: int32 * row: int32 * direction: MapEdgeDirection
    | EraseEdge of column: int32 * row: int32 * direction: MapEdgeDirection
    | SplitEdge of column: int32 * row: int32 * direction: MapEdgeDirection
    | JoinEdge of column: int32 * row: int32 * direction: MapEdgeDirection
    | SetUnitPaletteSearch of string
    | MoveUnitPaletteCursor of delta: int
    | PageUnitPaletteFaction of delta: int
    | SelectUnitPaletteBoundary of last: bool
    | ArmUnitPalettePreset
    | ReturnToUnitBrowse
    | MoveUnitPlacementCursor of columnDelta: int32 * rowDelta: int32
    | CycleArmedUnitPreset of delta: int
    | CommitUnitPlacement of returnToBrowse: bool
    | ResetUnitMovePreview
    | MoveRegionCursor of columnDelta: int32 * rowDelta: int32
    | BeginNewRegion
    | ChooseRegionPurpose of RegionPurpose
    | ChooseRegionShape of RegionShape
    | ActivateRegionCursor
    | CommitRegionPolygon
    | BacktrackRegionConstruction
    | BeginSelectedRegionMove
    | BeginSelectedRegionResize
    | BeginSelectedRegionVertexEdit
    | BeginSelectedRegionPurposeEdit
    | MoveRegionEditPreview of
        columnDelta: int32 *
        rowDelta: int32 *
        fromOppositeOrigin: bool
    | CycleRegionVertex of delta: int
    | ResetRegionEditPreview
    | CommitRegionEditPreview
    | CancelRegionKeyboardMode
    | CreateRectangleRegion of RegionPurpose * first: EditorCellAddress * last: EditorCellAddress
    | CreatePolygonRegion of RegionPurpose * vertices: EditorCellAddress array
    | SelectEditorRegion of int32 option
    | SetSelectedRegionPurpose of RegionPurpose
    | SetSelectedRegionGeometry of RegionGeometry
    | MoveSelectedRegion of columnDelta: int32 * rowDelta: int32
    | MoveSelectedRegionVertex of index: int * columnDelta: int32 * rowDelta: int32
    | RemoveSelectedRegion
    | PreviewUnitPlacement of EditorCellAddress
    | BeginUnitMove of EditorCellAddress
    | ExtendUnitMove of EditorCellAddress
    | BeginTerrainGesture of EditorCellAddress
    | ExtendTerrainGesture of EditorCellAddress
    | ActivateCell of column: int32 * row: int32
    | Resize of width: int32 * height: int32
    | RequestNewMap
    | RequestClearMap
    | ConfirmDestructiveChange
    | CancelDestructiveChange
    | SetEditorLayerState of EditorDomain * EditorLayerState
    | SelectNextIssue
    | SelectPreviousIssue
    | SetMapName of string
    | SaveMapView of name: string * camera: BattlefieldCamera
    | RemoveMapView of string
    | SetMapThumbnail of string option
    | OfferCrashRecovery of string
    | RecoverCrashDraft
    | DiscardCrashDraft
    | SelectEditorUnit of int32 option
    | ToggleEditorUnitSelection of int32
    | SelectEditorUnitsInBox of EditorBox
    | BeginEditorBoxSelection of EditorCellAddress
    | ExtendEditorBoxSelection of EditorCellAddress
    | CommitEditorGesture
    | CancelEditorGesture
    | SelectAllInActiveDomain
    | UndoEditorCommand
    | RedoEditorCommand
    | CopyEditorSelection
    | PasteEditorClipboard
    | DuplicateEditorSelection
    | DeleteEditorSelection
    | MarkEditorSaved
    | MarkEditorSimulated
    | MarkEditorRecovered of sourceDigest: string
    | RestoreEditorDraft
    | RemoveSelectedUnit
    | SetSelectedSide of MapSide
    | SetSelectedClass of string
    | SetSelectedSize of int32
    | SetSelectedHealth of remaining: int32 * maximum: int32
    | SetSelectedController of MapController
    | SetSelectedScript of string
    | MoveSelected of MapDirection
    | ToggleEditorRun
    | StepEditor
    | ClearMap
    | LoadMapText of string

