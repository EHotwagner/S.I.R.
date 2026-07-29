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

type MapDirection =
    | North
    | NorthEast
    | East
    | SouthEast
    | South
    | SouthWest
    | West
    | NorthWest

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
      ScriptIndex: int }

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

type CrashRecoveryDraft =
    { SourceDigest: string
      Map: MapDefinition }

type EditorBox =
    { FirstColumn: int32
      FirstRow: int32
      LastColumn: int32
      LastRow: int32 }

type EditorGesture =
    | IdleGesture
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
      TerrainAnnouncement: string
      EdgeCursor: int32 * int32 * MapEdgeDirection
      EdgeAnnouncement: string
      UnitPaletteSearch: string
      UnitAnnouncement: string
      RegionAnnouncement: string
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

[<RequireQualifiedAccess>]
module MapEditor =
    [<Literal>]
    let FormatVersion = 2

    [<Literal>]
    let LegacyFormatVersion = 1

    [<Literal>]
    let MaximumHistoryCommands = 100

    [<Literal>]
    let MaximumHistoryBytes = 2_000_000

    [<Literal>]
    let MaximumUnitFootprint = 8

    [<Literal>]
    let MaximumImportBytes = 2_000_000

    [<Literal>]
    let MaximumClipboardUnits = 256

    [<Literal>]
    let MaximumRegionCount = 1_600

    [<Literal>]
    let MaximumRegionVertices = 256

    [<Literal>]
    let MaximumClassIdLength = 128

    let canonicalFootprintPresets =
        [ { Id = "goblin"
            Name = "Goblin skirmisher"
            Faction = "Arcane"
            Role = "Skirmisher"
            ClassId = "goblin"
            GlyphId = "goblin"
            Side = Red
            FootprintSize = 1
            Health = 35
            HealthMaximum = 35 }
          { Id = "orc"
            Name = "Orc assault"
            Faction = "Arcane"
            Role = "Assault"
            ClassId = "orc"
            GlyphId = "orc"
            Side = Red
            FootprintSize = 2
            Health = 100
            HealthMaximum = 100 }
          { Id = "troll"
            Name = "Armored troll"
            Faction = "Arcane"
            Role = "Heavy"
            ClassId = "troll"
            GlyphId = "troll"
            Side = Red
            FootprintSize = 3
            Health = 240
            HealthMaximum = 240 }
          { Id = "human"
            Name = "Human rifleman"
            Faction = "Human"
            Role = "Line infantry"
            ClassId = "rifleman"
            GlyphId = "rifleman"
            Side = Blue
            FootprintSize = 2
            Health = 12
            HealthMaximum = 12 }
          { Id = "human-gunner"
            Name = "Human gunner"
            Faction = "Human"
            Role = "Area fire"
            ClassId = "gunner"
            GlyphId = "gunner"
            Side = Blue
            FootprintSize = 2
            Health = 12
            HealthMaximum = 12 }
          { Id = "human-marksman"
            Name = "Human marksman"
            Faction = "Human"
            Role = "Precision fire"
            ClassId = "marksman"
            GlyphId = "marksman"
            Side = Blue
            FootprintSize = 2
            Health = 12
            HealthMaximum = 12 }
          { Id = "human-engineer"
            Name = "Human engineer"
            Faction = "Human"
            Role = "Breaching and fieldworks"
            ClassId = "engineer"
            GlyphId = "engineer"
            Side = Blue
            FootprintSize = 2
            Health = 12
            HealthMaximum = 12 }
          { Id = "human-medic"
            Name = "Human medic"
            Faction = "Human"
            Role = "Casualty care"
            ClassId = "medic"
            GlyphId = "medic"
            Side = Blue
            FootprintSize = 2
            Health = 12
            HealthMaximum = 12 }
          { Id = "human-signaller"
            Name = "Human signaller"
            Faction = "Human"
            Role = "Communications and EW"
            ClassId = "signaller"
            GlyphId = "signaller"
            Side = Blue
            FootprintSize = 2
            Health = 12
            HealthMaximum = 12 }
          { Id = "drone"
            Name = "Observation drone"
            Faction = "Neutral"
            Role = "Reconnaissance"
            ClassId = "observation-drone"
            GlyphId = "observation-drone"
            Side = NeutralSide
            FootprintSize = 1
            Health = 8
            HealthMaximum = 8 }
          { Id = "relay-drone"
            Name = "Relay drone"
            Faction = "Neutral"
            Role = "Communications relay"
            ClassId = "relay-drone"
            GlyphId = "relay-drone"
            Side = NeutralSide
            FootprintSize = 1
            Health = 8
            HealthMaximum = 8 } ]

    let tryCanonicalFootprintPreset id =
        canonicalFootprintPresets
        |> List.tryFind (fun preset ->
            String.Equals(preset.Id, id, StringComparison.Ordinal))

    let searchCanonicalUnitPresets query =
        let needle =
            if String.IsNullOrWhiteSpace query then ""
            else query.Trim().ToLowerInvariant()

        canonicalFootprintPresets
        |> List.filter (fun preset ->
            needle = ""
            || [ preset.Name; preset.Faction; preset.Role; preset.ClassId; preset.GlyphId ]
               |> List.exists (fun value -> value.ToLowerInvariant().Contains needle))
        |> List.sortBy (fun preset -> preset.Faction, preset.Role, preset.Name, preset.Id)

    let private canonicalFootprintSize id =
        tryCanonicalFootprintPreset id
        |> Option.map _.FootprintSize
        |> Option.defaultWith (fun () ->
            invalidArg "id" ("Unknown canonical footprint preset: " + id))

    let private extent value =
        CellExtent.tryCreate value
        |> Option.defaultWith (fun () -> invalidArg "value" "Extent must be positive.")

    let private health remaining maximum =
        HealthVisual.tryCreate remaining maximum
        |> Option.defaultWith (fun () -> invalidArg "remaining" "Health is out of bounds.")

    let private heading =
        HeadingRadians.tryCreate 0.0
        |> Option.defaultWith (fun () -> failwith "Zero radians must be valid.")

    let private terrainName terrain =
        match terrain with
        | Open -> "open"
        | Rough -> "rough"
        | Blocked -> "blocked"
        | Objective -> "objective"

    let private terrainFromName value =
        match value with
        | "open" -> Some Open
        | "rough" -> Some Rough
        | "blocked" -> Some Blocked
        | "objective" -> Some Objective
        | _ -> None

    let private sideName side =
        match side with
        | Blue -> "blue"
        | Red -> "red"
        | NeutralSide -> "neutral"

    let private sideFromName value =
        match value with
        | "blue" -> Some Blue
        | "red" -> Some Red
        | "neutral" -> Some NeutralSide
        | _ -> None

    let private controllerName controller =
        match controller with
        | Manual -> "manual"
        | Scripted -> "scripted"
        | General -> "general"

    let private controllerFromName value =
        match value with
        | "manual" -> Some Manual
        | "scripted" -> Some Scripted
        | "general" -> Some General
        | _ -> None

    let private edgeDirectionName direction =
        match direction with
        | EastEdge -> "east"
        | SouthEdge -> "south"

    let private edgeKindName kind =
        match kind with
        | Wall -> "wall"
        | Door -> "door"
        | Window -> "window"

    let regionPurposeLabel purpose =
        match purpose with
        | ObjectiveRegion -> "Objective"
        | DeploymentZone Blue -> "Blue deployment"
        | DeploymentZone Red -> "Red deployment"
        | DeploymentZone NeutralSide -> "Neutral deployment"

    let private regionPurposeFields purpose =
        match purpose with
        | ObjectiveRegion -> [ "objective" ]
        | DeploymentZone side -> [ "deployment"; sideName side ]

    let private regionGeometryFields geometry =
        match geometry with
        | RegionRectangle(column, row, width, height) ->
            [ "rectangle"; string column; string row; string width; string height ]
        | RegionPolygon vertices ->
            "polygon"
            :: (vertices
                |> Array.map (fun vertex ->
                    string vertex.CellColumn + "," + string vertex.CellRow)
                |> Array.toList)

    /// Normalizes one unit grid segment to its single authoritative east/south
    /// record. North and west border segments have no owning cell and are
    /// rejected instead of being silently shifted into the document.
    let tryNormalizeEdge width height x1 y1 x2 y2 =
        if x1 = x2 && abs (y2 - y1) = 1 then
            let column = x1 - 1
            let row = min y1 y2
            if column >= 0 && column < width && row >= 0 && row < height then
                Some(column, row, EastEdge)
            else
                None
        elif y1 = y2 && abs (x2 - x1) = 1 then
            let column = min x1 x2
            let row = y1 - 1
            if column >= 0 && column < width && row >= 0 && row < height then
                Some(column, row, SouthEdge)
            else
                None
        else
            None

    let private directionCode direction =
        match direction with
        | North -> "N"
        | NorthEast -> "NE"
        | East -> "E"
        | SouthEast -> "SE"
        | South -> "S"
        | SouthWest -> "SW"
        | West -> "W"
        | NorthWest -> "NW"

    let private directionFromCode (value: string) =
        match value.Trim().ToUpperInvariant() with
        | "N" -> Some North
        | "NE" -> Some NorthEast
        | "E" -> Some East
        | "SE" -> Some SouthEast
        | "S" -> Some South
        | "SW" -> Some SouthWest
        | "W" -> Some West
        | "NW" -> Some NorthWest
        | _ -> None

    let parseScript value =
        if String.IsNullOrWhiteSpace value then
            Ok []
        else
            let tokens =
                value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                |> Array.toList

            let parsed = tokens |> List.map directionFromCode
            if parsed |> List.exists Option.isNone then
                Error "Use comma-separated directions: N, NE, E, SE, S, SW, W, NW."
            else
                parsed |> List.choose id |> Ok

    let scriptText script =
        script |> List.map directionCode |> String.concat ","

    let private canonicalMapText (map: MapDefinition) =
        let lines =
            [ "SIR-MAP " + string FormatVersion
              "size " + string map.Width + " " + string map.Height
              yield!
                  map.Terrain
                  |> Map.toList
                  |> List.map (fun ((column, row), terrain) ->
                      "terrain "
                      + string column
                      + " "
                      + string row
                      + " "
                      + terrainName terrain)
              yield!
                  map.Edges
                  |> Map.toList
                  |> List.map (fun ((column, row, direction), (kind, isOpen)) ->
                      "edge "
                      + string column
                      + " "
                      + string row
                      + " "
                      + edgeDirectionName direction
                      + " "
                      + edgeKindName kind
                      + " "
                      + (if isOpen then "open" else "closed"))
              yield!
                  map.Regions
                  |> Map.toList
                  |> List.map (fun (_, region) ->
                      [ "zone"; string region.Id ]
                      @ regionPurposeFields region.Purpose
                      @ regionGeometryFields region.Geometry
                      |> String.concat " ")
              yield!
                  map.Units
                  |> Map.toList
                  |> List.map (fun (_, unit) ->
                      "unit "
                      + string unit.Id
                      + " "
                      + sideName unit.Side
                      + " "
                      + unit.ClassId
                      + " "
                      + string unit.Column
                      + " "
                      + string unit.Row
                      + " "
                      + string unit.Size
                      + " "
                      + string unit.Health
                      + " "
                      + string unit.HealthMaximum
                      + " "
                      + controllerName unit.Controller
                      + " "
                      + (if List.isEmpty unit.Script then "-" else scriptText unit.Script)) ]

        String.concat "\n" lines + "\n"

    let private hex (bytes: byte array) =
        bytes
        |> Array.map (fun value -> value.ToString("x2"))
        |> String.concat ""

    let revisionDigest map =
        map
        |> canonicalMapText
        |> Encoding.UTF8.GetBytes
        |> CanonicalHash.sha256
        |> hex

    let private revision number parent map =
        { Number = number
          ParentDigest = parent
          Document = map
          Digest = revisionDigest map }

    let private serializedBytes map =
        map |> canonicalMapText |> Encoding.UTF8.GetBytes |> Array.length

    let private emptyMap width height =
        { Width = width
          Height = height
          Terrain = Map.empty
          Edges = Map.empty
          Units = Map.empty
          NextUnitId = 1
          Regions = Map.empty
          NextRegionId = 1 }

    let initial =
        let units =
            [ { Id = 1
                Side = Blue
                ClassId = "rifleman"
                Column = 1
                Row = 1
                Size = canonicalFootprintSize "human"
                Health = 12
                HealthMaximum = 12
                Controller = Manual
                Script = []
                ScriptIndex = 0 }
              { Id = 2
                Side = Blue
                ClassId = "medic"
                Column = 1
                Row = 5
                Size = canonicalFootprintSize "human"
                Health = 12
                HealthMaximum = 12
                Controller = Scripted
                Script = [ East; East; North ]
                ScriptIndex = 0 }
              { Id = 3
                Side = Red
                ClassId = "goblin"
                Column = 9
                Row = 1
                Size = canonicalFootprintSize "goblin"
                Health = 12
                HealthMaximum = 12
                Controller = General
                Script = []
                ScriptIndex = 0 }
              { Id = 4
                Side = Red
                ClassId = "troll"
                Column = 8
                Row = 5
                Size = canonicalFootprintSize "troll"
                Health = 12
                HealthMaximum = 12
                Controller = General
                Script = []
                ScriptIndex = 0 } ]
            |> List.map (fun unit -> unit.Id, unit)
            |> Map.ofList
        let map =
            { emptyMap 12 8 with
                Terrain =
                    [ (5, 3), Objective
                      (5, 4), Objective
                      (4, 3), Rough
                      (4, 4), Rough
                      (6, 3), Rough
                      (6, 4), Rough ]
                    |> Map.ofList
                Edges =
                    [ (5, 2, SouthEdge), (Wall, false)
                      (6, 2, SouthEdge), (Door, false)
                      (7, 2, SouthEdge), (Window, false) ]
                    |> Map.ofList
                Units = units
                NextUnitId = 5 }

        let initialRevision = revision 0L None map

        { Map = map
          Tool = Select
          TerrainSelection = Rough
          BrushSize = 1
          TerrainCursor =
            { CellColumn = 0
              CellRow = 0 }
          TerrainAnnouncement = "Terrain authoring ready."
          EdgeCursor = 0, 0, EastEdge
          EdgeAnnouncement = "Semantic edge authoring ready."
          UnitPaletteSearch = ""
          UnitAnnouncement = "Unit authoring ready."
          RegionAnnouncement = "Zone authoring ready."
          SelectedUnit = Some 1
          SelectedUnits = Set.singleton 1
          SelectedRegion = None
          Gesture = IdleGesture
          Revision = initialRevision
          RevisionState = SavedRevision
          SavedDigest = Some initialRevision.Digest
          SimulatedDigest = None
          RecoveredFromDigest = None
          UndoHistory = []
          RedoHistory = []
          HistoryBytes = 0
          Clipboard = None
          Tick = 0
          IsRunning = false
          LastEvents = []
          Validation = None
          Layers =
            [ TerrainDomain; EdgeDomain; UnitDomain; RegionDomain; DocumentDomain ]
            |> List.map (fun domain -> domain, VisibleLayer)
            |> Map.ofList
          Issues = [||]
          ActiveIssue = None
          PendingDestructiveChange = None
          PendingRecovery = None
          Authoring =
            { Name = "Untitled battlefield"
              SavedViews = Map.empty
              RevisionIdentity = initialRevision.Digest
              ThumbnailSvg = None } }

    let private directionDelta direction =
        match direction with
        | North -> 0, -1
        | NorthEast -> 1, -1
        | East -> 1, 0
        | SouthEast -> 1, 1
        | South -> 0, 1
        | SouthWest -> -1, 1
        | West -> -1, 0
        | NorthWest -> -1, -1

    let private sign value =
        if value < 0 then -1
        elif value > 0 then 1
        else 0

    let private directionForDelta x y =
        match sign x, sign y with
        | 0, -1 -> Some North
        | 1, -1 -> Some NorthEast
        | 1, 0 -> Some East
        | 1, 1 -> Some SouthEast
        | 0, 1 -> Some South
        | -1, 1 -> Some SouthWest
        | -1, 0 -> Some West
        | -1, -1 -> Some NorthWest
        | _ -> None

    let private cells unit column row =
        [ for y in row .. row + unit.Size - 1 do
              for x in column .. column + unit.Size - 1 do
                  yield x, y ]

    let private edgeIsBlocking map key =
        Map.tryFind key map.Edges
        |> Option.exists (fun (kind, isOpen) ->
            match kind with
            | Door -> not isOpen
            | Wall
            | Window -> true)

    let private edgeBlocks (map: MapDefinition) unit dx dy =
        let verticalEdges =
            if dx > 0 then
                [ for row in unit.Row .. unit.Row + unit.Size - 1 ->
                      unit.Column + unit.Size - 1, row, EastEdge ]
            elif dx < 0 then
                [ for row in unit.Row .. unit.Row + unit.Size - 1 ->
                      unit.Column - 1, row, EastEdge ]
            else
                []

        let horizontalEdges =
            if dy > 0 then
                [ for column in unit.Column .. unit.Column + unit.Size - 1 ->
                      column, unit.Row + unit.Size - 1, SouthEdge ]
            elif dy < 0 then
                [ for column in unit.Column .. unit.Column + unit.Size - 1 ->
                      column, unit.Row - 1, SouthEdge ]
            else
                []

        verticalEdges @ horizontalEdges
        |> List.exists (edgeIsBlocking map)

    let private validPlacement map excludedUnit unit column row =
        let targetCells = cells unit column row
        let inBounds =
            targetCells
            |> List.forall (fun (x, y) ->
                x >= 0
                && y >= 0
                && x < map.Width
                && y < map.Height
                && Map.tryFind (x, y) map.Terrain <> Some Blocked)
        let occupied =
            map.Units
            |> Map.toList
            |> List.filter (fun (id, _) -> Some id <> excludedUnit)
            |> List.collect (fun (_, other) -> cells other other.Column other.Row)
            |> Set.ofList

        inBounds
        && (targetCells |> List.forall (fun cell -> not (Set.contains cell occupied)))

    let private moveUnit direction unit map =
        let dx, dy = directionDelta direction
        let column = unit.Column + int32 dx
        let row = unit.Row + int32 dy
        let crossesBlockedEdge =
            edgeBlocks map unit (int32 dx) (int32 dy)

        if
            not crossesBlockedEdge
            && validPlacement map (Some unit.Id) unit column row
        then
            { unit with
                Column = column
                Row = row },
            true
        else
            unit, false

    let private setUnit (unit: EditorUnit) (map: MapDefinition) =
        { map with Units = Map.add unit.Id unit map.Units }

    let private selectedUnit state =
        state.SelectedUnit
        |> Option.bind (fun id -> Map.tryFind id state.Map.Units)

    let private unitAtCell column row map =
        map.Units
        |> Map.toList
        |> List.map snd
        |> List.tryFind (fun unit ->
            cells unit unit.Column unit.Row
            |> List.contains (column, row))

    let private placeUnit side classId size column row state =
        let unit =
            { Id = state.Map.NextUnitId
              Side = side
              ClassId = classId
              Column = column
              Row = row
              Size = size
              Health = 12
              HealthMaximum = 12
              Controller = Manual
              Script = []
              ScriptIndex = 0 }

        if validPlacement state.Map None unit column row then
            let map =
                { state.Map with
                    Units = Map.add unit.Id unit state.Map.Units
                    NextUnitId = unit.Id + 1 }
            { state with
                Map = map
                SelectedUnit = Some unit.Id
                Validation = None }
        else
            { state with Validation = Some "The unit does not fit on those cells." }

    let private nearestHostile unit map =
        let axisGap start size otherStart otherSize =
            max 0 (
                max start otherStart
                - min (start + size - 1) (otherStart + otherSize - 1)
                - 1
            )

        let distance other =
            max
                (axisGap unit.Column unit.Size other.Column other.Size)
                (axisGap unit.Row unit.Size other.Row other.Size)

        map.Units
        |> Map.toList
        |> List.map snd
        |> List.filter (fun other -> other.Id <> unit.Id && other.Side <> unit.Side)
        |> List.sortBy (fun other ->
            distance other,
            other.Id)
        |> List.tryHead
        |> Option.map (fun other -> other, distance other)

    let private actGeneral unit map =
        match nearestHostile unit map with
        | None -> map, "Unit " + string unit.Id + " holds; no hostile is present."
        | Some(target, distance) ->
            let dx = target.Column - unit.Column
            let dy = target.Row - unit.Row
            if distance = 0 then
                let damaged =
                    { target with Health = max 0 (target.Health - 1) }
                setUnit damaged map,
                "Unit "
                + string unit.Id
                + " attacks unit "
                + string target.Id
                + " for 1 damage."
            else
                match directionForDelta dx dy with
                | None -> map, "Unit " + string unit.Id + " holds."
                | Some direction ->
                    let moved, changed = moveUnit direction unit map
                    if changed then
                        setUnit moved map,
                        "Unit " + string unit.Id + " advances " + directionCode direction + "."
                    else
                        map,
                        "Unit " + string unit.Id + " cannot advance."

    let private actScripted unit map =
        match unit.Script with
        | [] -> map, "Unit " + string unit.Id + " has no script."
        | script ->
            let direction = script[unit.ScriptIndex % script.Length]
            let moved, changed = moveUnit direction unit map
            let advanced =
                { moved with ScriptIndex = unit.ScriptIndex + 1 }
            setUnit advanced map,
            ("Unit "
             + string unit.Id
             + (if changed then " follows " else " is blocked moving ")
             + directionCode direction
             + ".")

    let step state =
        let mutable map = state.Map
        let mutable events = []

        for id in state.Map.Units |> Map.toList |> List.map fst |> List.sort do
            match Map.tryFind id map.Units with
            | Some unit when unit.Health > 0 ->
                let nextMap, event =
                    match unit.Controller with
                    | Manual -> map, "Unit " + string id + " awaits manual input."
                    | Scripted -> actScripted unit map
                    | General -> actGeneral unit map
                map <- nextMap
                events <- event :: events
            | _ -> ()

        { state with
            Map = map
            Tick = state.Tick + 1
            LastEvents = List.rev events
            Validation = None }

    let resizeLossPreview width height (map: MapDefinition) =
        let width = max 4 (min 40 width)
        let height = max 4 (min 40 height)
        { TargetWidth = width
          TargetHeight = height
          LostTerrainCells =
            map.Terrain
            |> Map.toList
            |> List.filter (fun ((column, row), _) -> column >= width || row >= height)
            |> List.length
          LostEdges =
            map.Edges
            |> Map.toList
            |> List.filter (fun ((column, row, _), _) -> column >= width || row >= height)
            |> List.length
          LostUnits =
            map.Units
            |> Map.toList
            |> List.filter (fun (_, unit) ->
                unit.Column + unit.Size > width || unit.Row + unit.Size > height)
            |> List.length
          LostRegions =
            map.Regions
            |> Map.toList
            |> List.filter (fun (_, region) ->
                let points =
                    match region.Geometry with
                    | RegionRectangle(column, row, regionWidth, regionHeight) ->
                        [ column, row
                          column + regionWidth, row
                          column + regionWidth, row + regionHeight
                          column, row + regionHeight ]
                    | RegionPolygon vertices ->
                        vertices
                        |> Array.map (fun point -> point.CellColumn, point.CellRow)
                        |> Array.toList
                points
                |> List.exists (fun (column, row) ->
                    column < 0 || row < 0 || column > width || row > height))
            |> List.length }

    let private resizedDocument (preview: ResizeLossPreview) (map: MapDefinition) =
        let terrain =
            map.Terrain
            |> Map.filter (fun (column, row) _ ->
                column < preview.TargetWidth && row < preview.TargetHeight)
        let edges =
            map.Edges
            |> Map.filter (fun (column, row, _) _ ->
                column < preview.TargetWidth && row < preview.TargetHeight)
        let units =
            map.Units
            |> Map.filter (fun _ unit ->
                unit.Column + unit.Size <= preview.TargetWidth
                && unit.Row + unit.Size <= preview.TargetHeight)
        let regions =
            map.Regions
            |> Map.filter (fun _ region ->
                let points =
                    match region.Geometry with
                    | RegionRectangle(column, row, width, height) ->
                        [ column, row
                          column + width, row
                          column + width, row + height
                          column, row + height ]
                    | RegionPolygon vertices ->
                        vertices
                        |> Array.map (fun point -> point.CellColumn, point.CellRow)
                        |> Array.toList
                points
                |> List.forall (fun (column, row) ->
                    column >= 0
                    && row >= 0
                    && column <= preview.TargetWidth
                    && row <= preview.TargetHeight))

        { map with
            Width = preview.TargetWidth
            Height = preview.TargetHeight
            Terrain = terrain
            Edges = edges
            Units = units
            Regions = regions }

    let private resize width height state =
        let preview = resizeLossPreview width height state.Map
        { state with
            PendingDestructiveChange = Some(ResizePending preview)
            Validation =
                if preview.LostTerrainCells + preview.LostEdges + preview.LostUnits + preview.LostRegions = 0 then None
                else
                    Some(
                        "Resize would remove "
                        + string preview.LostTerrainCells
                        + " terrain cells, "
                        + string preview.LostEdges
                        + " edges, and "
                        + string preview.LostUnits
                        + " units, and "
                        + string preview.LostRegions
                        + " regions. Confirm to continue."
                    ) }

    let private inBounds map address =
        address.CellColumn >= 0
        && address.CellRow >= 0
        && address.CellColumn < map.Width
        && address.CellRow < map.Height

    let private normalizeAddresses addresses =
        addresses
        |> Seq.distinct
        |> Seq.sortBy (fun address -> address.CellRow, address.CellColumn)
        |> Seq.toArray

    let private lineAddresses first last =
        let mutable x = first.CellColumn
        let mutable y = first.CellRow
        let dx = abs (last.CellColumn - first.CellColumn)
        let sx = if first.CellColumn < last.CellColumn then 1 else -1
        let dy = -(abs (last.CellRow - first.CellRow))
        let sy = if first.CellRow < last.CellRow then 1 else -1
        let mutable error = dx + dy
        let addresses = ResizeArray<EditorCellAddress>()
        let mutable finished = false

        while not finished do
            addresses.Add
                { CellColumn = x
                  CellRow = y }
            if x = last.CellColumn && y = last.CellRow then
                finished <- true
            else
                let doubled = 2 * error
                if doubled >= dy then
                    error <- error + dy
                    x <- x + sx
                if doubled <= dx then
                    error <- error + dx
                    y <- y + sy

        addresses.ToArray()

    let private brushAddresses map brushSize centers =
        let brushSize = max 1 brushSize
        let leading = (brushSize - 1) / 2
        let trailing = brushSize - leading - 1

        centers
        |> Seq.collect (fun center ->
            seq {
                for row in center.CellRow - leading .. center.CellRow + trailing do
                    for column in center.CellColumn - leading .. center.CellColumn + trailing do
                        yield
                            { CellColumn = column
                              CellRow = row }
            })
        |> Seq.filter (inBounds map)
        |> normalizeAddresses

    let private rectangleAddresses map brushSize first last =
        let minimumColumn = min first.CellColumn last.CellColumn
        let maximumColumn = max first.CellColumn last.CellColumn
        let minimumRow = min first.CellRow last.CellRow
        let maximumRow = max first.CellRow last.CellRow

        seq {
            for row in minimumRow .. maximumRow do
                for column in minimumColumn .. maximumColumn do
                    yield
                        { CellColumn = column
                          CellRow = row }
        }
        |> brushAddresses map brushSize

    let private floodAddresses map start =
        if not (inBounds map start) then
            [||]
        else
            let source =
                Map.tryFind (start.CellColumn, start.CellRow) map.Terrain
                |> Option.defaultValue Open
            let pending = Collections.Generic.Queue<EditorCellAddress>()
            let mutable visited = Set.empty
            pending.Enqueue start

            while pending.Count > 0 do
                let address = pending.Dequeue()
                let key = address.CellColumn, address.CellRow
                let terrain = Map.tryFind key map.Terrain |> Option.defaultValue Open
                if not (Set.contains key visited) && terrain = source then
                    visited <- Set.add key visited
                    [ { address with CellColumn = address.CellColumn - 1 }
                      { address with CellColumn = address.CellColumn + 1 }
                      { address with CellRow = address.CellRow - 1 }
                      { address with CellRow = address.CellRow + 1 } ]
                    |> List.filter (inBounds map)
                    |> List.iter pending.Enqueue

            visited
            |> Seq.map (fun (column, row) ->
                { CellColumn = column
                  CellRow = row })
            |> normalizeAddresses

    let private terrainGestureAddresses map brushSize tool anchor current visited =
        match tool with
        | PencilTool ->
            visited
            |> Array.append (lineAddresses anchor current)
            |> brushAddresses map brushSize
        | RectangleTool -> rectangleAddresses map brushSize anchor current
        | LineTool ->
            lineAddresses anchor current
            |> brushAddresses map brushSize
        | FloodFillTool -> floodAddresses map anchor
        | EyedropperTool -> [| anchor |] |> Array.filter (inBounds map)
        | EraseTool ->
            visited
            |> Array.append (lineAddresses anchor current)
            |> brushAddresses map brushSize

    let private terrainGestureCommand state tool anchor current visited =
        let terrain =
            match tool with
            | EraseTool -> Open
            | _ -> state.TerrainSelection
        PaintCells(
            terrain,
            terrainGestureAddresses
                state.Map
                state.BrushSize
                tool
                anchor
                current
                visited
        )

    let rec private legacyUpdate action state =
        match action with
        | ChooseTool tool ->
            { state with Tool = tool; Gesture = IdleGesture; Validation = None }
        | ChooseTerrain _
        | SetTerrainBrushSize _
        | MoveTerrainCursor _
        | ActivateTerrainCursor
        | ActivateEdge _
        | MoveEdgeCursor _
        | ActivateEdgeCursor
        | FinishEdgePolyline
        | BacktrackEdgePolyline
        | ConvertEdge _
        | ToggleDoorState _
        | EraseEdge _
        | SplitEdge _
        | JoinEdge _
        | SetUnitPaletteSearch _
        | PreviewUnitPlacement _
        | BeginUnitMove _
        | ExtendUnitMove _
        | BeginTerrainGesture _
        | ExtendTerrainGesture _ -> state
        | ActivateCell(column, row) ->
            match state.Tool with
            | Select ->
                let selected =
                    state.Map.Units
                    |> Map.toList
                    |> List.map snd
                    |> List.tryFind (fun unit ->
                        cells unit unit.Column unit.Row
                        |> List.contains (column, row))
                    |> Option.map _.Id
                { state with SelectedUnit = selected; Validation = None }
            | Paint Open ->
                { state with
                    Map =
                        { state.Map with
                            Terrain = Map.remove (column, row) state.Map.Terrain }
                    Validation = None }
            | Paint Blocked when unitAtCell column row state.Map |> Option.isSome ->
                { state with
                    Validation = Some "Remove the unit before blocking this cell." }
            | Paint terrain ->
                { state with
                    Map =
                        { state.Map with
                            Terrain = Map.add (column, row) terrain state.Map.Terrain }
                    Validation = None }
            | Terrain _ -> state
            | Place(side, classId, size) ->
                placeUnit side classId size column row state
            | Edge(direction, kind) ->
                let key = column, row, direction
                let edges =
                    match Map.tryFind key state.Map.Edges with
                    | Some(existing, false) when existing = kind && kind = Door ->
                        Map.add key (kind, true) state.Map.Edges
                    | Some(existing, true) when existing = kind && kind = Door ->
                        Map.remove key state.Map.Edges
                    | Some(existing, _) when existing = kind ->
                        Map.remove key state.Map.Edges
                    | _ -> Map.add key (kind, false) state.Map.Edges
                { state with
                    Map = { state.Map with Edges = edges }
                    Validation = None }
        | Resize(width, height) -> resize width height state
        | SelectEditorUnit id ->
            { state with SelectedUnit = id; Tool = Select; Validation = None }
        | ToggleEditorUnitSelection _
        | SelectEditorUnitsInBox _
        | BeginEditorBoxSelection _
        | ExtendEditorBoxSelection _
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
        | MarkEditorRecovered _
        | RestoreEditorDraft -> state
        | RemoveSelectedUnit ->
            match state.SelectedUnit with
            | None -> state
            | Some id ->
                { state with
                    Map = { state.Map with Units = Map.remove id state.Map.Units }
                    SelectedUnit = None
                    Validation = None }
        | SetSelectedSide side ->
            match selectedUnit state with
            | None -> state
            | Some unit ->
                { state with
                    Map = state.Map |> setUnit { unit with Side = side }
                    Validation = None }
        | SetSelectedClass classId ->
            let classId = classId.Trim()
            match selectedUnit state with
            | None -> state
            | Some _ when
                String.IsNullOrWhiteSpace classId
                || classId |> Seq.exists Char.IsWhiteSpace
                ->
                { state with Validation = Some "Class ID must be one non-empty token." }
            | Some unit ->
                { state with
                    Map = state.Map |> setUnit { unit with ClassId = classId }
                    Validation = None }
        | SetSelectedSize size ->
            match selectedUnit state with
            | None -> state
            | Some unit ->
                let resized = { unit with Size = size }
                if size > 0 && validPlacement state.Map (Some unit.Id) resized unit.Column unit.Row then
                    { state with
                        Map = state.Map |> setUnit resized
                        Validation = None }
                else
                    { state with Validation = Some "The resized square does not fit." }
        | SetSelectedHealth(remaining, maximum) ->
            match selectedUnit state with
            | None -> state
            | Some unit when maximum > 0 && remaining >= 0 && remaining <= maximum ->
                { state with
                    Map =
                        state.Map
                        |> setUnit
                            { unit with
                                Health = remaining
                                HealthMaximum = maximum }
                    Validation = None }
            | Some _ ->
                { state with Validation = Some "Health must satisfy 0 ≤ current ≤ maximum." }
        | SetSelectedController controller ->
            match selectedUnit state with
            | None -> state
            | Some unit ->
                { state with
                    Map =
                        state.Map
                        |> setUnit { unit with Controller = controller }
                    Validation = None }
        | SetSelectedScript value ->
            match selectedUnit state, parseScript value with
            | None, _ -> state
            | Some unit, Ok script ->
                { state with
                    Map =
                        state.Map
                        |> setUnit
                            { unit with
                                Script = script
                                ScriptIndex = 0 }
                    Validation = None }
            | Some _, Error error ->
                { state with Validation = Some error }
        | MoveSelected direction ->
            match selectedUnit state with
            | None -> { state with Validation = Some "Select a unit first." }
            | Some unit ->
                let moved, changed = moveUnit direction unit state.Map
                if changed then
                    { state with
                        Map = state.Map |> setUnit moved
                        Tick = state.Tick + 1
                        LastEvents =
                            [ "Unit "
                              + string unit.Id
                              + " moves "
                              + directionCode direction
                              + "." ]
                        Validation = None }
                else
                    { state with Validation = Some "That move is blocked." }
        | ToggleEditorRun ->
            { state with IsRunning = not state.IsRunning }
        | StepEditor -> step state
        | ClearMap ->
            { initial with
                Map = emptyMap state.Map.Width state.Map.Height
                SelectedUnit = None }
        | LoadMapText text ->
            match tryImport text with
            | Ok map ->
                { state with
                    Map = map
                    SelectedUnit = None
                    Tick = 0
                    IsRunning = false
                    LastEvents = []
                    Validation = None }
            | Error error ->
                { state with Validation = Some error }
        | _ -> state

    and export state = canonicalMapText state.Map

    and tryImport text =
        if (Encoding.UTF8.GetBytes text).Length > MaximumImportBytes then
            Error(
                "Map input exceeds the "
                + string MaximumImportBytes
                + "-byte qualification limit."
            )
        else
            tryImportWithinLimit text

    and private tryImportWithinLimit text =
        let fail line message =
            Error("Map line " + string line + ": " + message)

        let parseInt line (value: string) =
            match Int32.TryParse value with
            | true, parsed -> Ok parsed
            | _ -> fail line ("invalid integer '" + value + "'.")

        let parsePoint line (value: string) =
            match value.Split(',') with
            | [| column; row |] ->
                match parseInt line column, parseInt line row with
                | Ok column, Ok row ->
                    Ok
                        { CellColumn = column
                          CellRow = row }
                | Error error, _
                | _, Error error -> Error error
            | _ -> fail line ("invalid polygon vertex '" + value + "'.")

        let polygonArea (vertices: EditorCellAddress array) =
            vertices
            |> Array.mapi (fun index point ->
                let next = vertices[(index + 1) % vertices.Length]
                int64 point.CellColumn * int64 next.CellRow
                - int64 next.CellColumn * int64 point.CellRow)
            |> Array.sum

        let polygonIntersectsItself (vertices: EditorCellAddress array) =
            let orientation a b c =
                (b.CellColumn - a.CellColumn) * (c.CellRow - a.CellRow)
                - (b.CellRow - a.CellRow) * (c.CellColumn - a.CellColumn)
            let onSegment a b c =
                min a.CellColumn c.CellColumn <= b.CellColumn
                && b.CellColumn <= max a.CellColumn c.CellColumn
                && min a.CellRow c.CellRow <= b.CellRow
                && b.CellRow <= max a.CellRow c.CellRow
            let intersects a b c d =
                let first = orientation a b c
                let second = orientation a b d
                let third = orientation c d a
                let fourth = orientation c d b
                (sign first <> sign second && sign third <> sign fourth)
                || (first = 0 && onSegment a c b)
                || (second = 0 && onSegment a d b)
                || (third = 0 && onSegment c a d)
                || (fourth = 0 && onSegment c b d)
            [ for first in 0 .. vertices.Length - 1 do
                  for second in first + 1 .. vertices.Length - 1 do
                      if
                          second <> first + 1
                          && not (first = 0 && second = vertices.Length - 1)
                      then
                          yield
                              intersects
                                  vertices[first]
                                  vertices[(first + 1) % vertices.Length]
                                  vertices[second]
                                  vertices[(second + 1) % vertices.Length] ]
            |> List.exists id

        let invalidRegion map =
            map.Regions
            |> Map.toList
            |> List.tryPick (fun (id, region) ->
                if id <> region.Id || id <= 0 then Some "region identity is invalid."
                elif region.Purpose = DeploymentZone NeutralSide then Some "deployment zones must belong to blue or red."
                else
                    match region.Geometry with
                    | RegionRectangle(column, row, width, height)
                        when width <= 0
                             || height <= 0
                             || column < 0
                             || row < 0
                             || column + width > map.Width
                             || row + height > map.Height ->
                        Some "region rectangle is invalid or outside the map."
                    | RegionPolygon vertices
                        when vertices.Length < 3
                             || Array.distinct vertices |> Array.length <> vertices.Length
                             || polygonArea vertices = 0L
                             || (vertices.Length >= 4 && polygonIntersectsItself vertices)
                             || (vertices
                                 |> Array.exists (fun vertex ->
                                     vertex.CellColumn < 0
                                     || vertex.CellRow < 0
                                     || vertex.CellColumn > map.Width
                                     || vertex.CellRow > map.Height)) ->
                        Some "region polygon is invalid or outside the map."
                    | _ -> None)

        let lines =
            text.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries)

        let version =
            if lines.Length = 0 then None
            elif lines[0] = "SIR-MAP " + string LegacyFormatVersion then Some LegacyFormatVersion
            elif lines[0] = "SIR-MAP " + string FormatVersion then Some FormatVersion
            else None

        if lines.Length < 2 || version.IsNone then
            Error "The file is not a supported SIR-MAP 1 or SIR-MAP 2 document."
        else
            let mutable result = Ok(emptyMap 12 8)

            for index in 1 .. lines.Length - 1 do
                match result with
                | Error _ -> ()
                | Ok map ->
                    let parts =
                        lines[index].Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    let line = index + 1
                    match parts |> Array.toList with
                    | [ "size"; width; height ] ->
                        match parseInt line width, parseInt line height with
                        | Ok width, Ok height when width >= 4 && width <= 40 && height >= 4 && height <= 40 ->
                            result <- Ok { map with Width = width; Height = height }
                        | Ok _, Ok _ -> result <- fail line "size must be between 4 and 40."
                        | Error error, _
                        | _, Error error -> result <- Error error
                    | [ "terrain"; column; row; terrain ] ->
                        match parseInt line column, parseInt line row, terrainFromName terrain with
                        | Ok column, Ok row, Some terrain ->
                            result <- Ok { map with Terrain = Map.add (column, row) terrain map.Terrain }
                        | _, _, None -> result <- fail line "unknown terrain."
                        | Error error, _, _
                        | _, Error error, _ -> result <- Error error
                    | [ "edge"; column; row; direction; kind; state ] ->
                        let edgeDirection =
                            if direction = "east" then Some EastEdge
                            elif direction = "south" then Some SouthEdge
                            else None
                        let edgeKind =
                            if kind = "wall" then Some Wall
                            elif kind = "door" then Some Door
                            elif kind = "window" then Some Window
                            else None
                        match parseInt line column, parseInt line row, edgeDirection, edgeKind with
                        | Ok column, Ok row, Some direction, Some kind
                            when state = "open" || state = "closed" ->
                            let address = column, row, direction
                            if Map.containsKey address map.Edges then
                                result <-
                                    fail
                                        line
                                        "duplicate or overlapping canonical edge record."
                            else
                                result <-
                                    Ok
                                        { map with
                                            Edges =
                                                Map.add
                                                    address
                                                    (kind, state = "open")
                                                    map.Edges }
                        | Error error, _, _, _
                        | _, Error error, _, _ -> result <- Error error
                        | _ -> result <- fail line "invalid edge direction, kind, or state."
                    | "zone" :: _ when version = Some LegacyFormatVersion ->
                        result <- fail line "SIR-MAP 1 cannot contain zone records."
                    | "zone" :: id :: purposeAndGeometry ->
                        let geometryIndex, purpose =
                            match purposeAndGeometry with
                            | "objective" :: _ -> 1, Some ObjectiveRegion
                            | "deployment" :: side :: _ ->
                                2, sideFromName side |> Option.map DeploymentZone
                            | _ -> 0, None
                        let geometryFields =
                            purposeAndGeometry |> List.skip (min geometryIndex purposeAndGeometry.Length)
                        let geometry =
                            match geometryFields with
                            | [ "rectangle"; column; row; width; height ] ->
                                match
                                    parseInt line column,
                                    parseInt line row,
                                    parseInt line width,
                                    parseInt line height
                                with
                                | Ok column, Ok row, Ok width, Ok height ->
                                    Ok(RegionRectangle(column, row, width, height))
                                | Error error, _, _, _
                                | _, Error error, _, _
                                | _, _, Error error, _
                                | _, _, _, Error error -> Error error
                            | "polygon" :: vertices
                                when vertices.Length >= 3
                                     && vertices.Length <= MaximumRegionVertices ->
                                vertices
                                |> List.map (parsePoint line)
                                |> List.fold
                                    (fun current point ->
                                        match current, point with
                                        | Ok points, Ok value -> Ok(value :: points)
                                        | Error error, _
                                        | _, Error error -> Error error)
                                    (Ok [])
                                |> Result.map (List.rev >> List.toArray >> RegionPolygon)
                            | _ -> fail line "invalid or unsupported zone geometry."
                        match parseInt line id, purpose, geometry with
                        | Ok id, Some purpose, Ok geometry
                            when id > 0
                                 && map.Regions.Count < MaximumRegionCount
                                 && not (Map.containsKey id map.Regions) ->
                            let region =
                                { Id = id
                                  Geometry = geometry
                                  Purpose = purpose
                                  Behavior = NoRegionBehavior }
                            result <-
                                Ok
                                    { map with
                                        Regions = Map.add id region map.Regions
                                        NextRegionId = max map.NextRegionId (id + 1) }
                        | Error error, _, _
                        | _, _, Error error -> result <- Error error
                        | _, None, _ -> result <- fail line "unknown zone purpose."
                        | _ -> result <- fail line "invalid or duplicate zone identifier."
                    | [ "unit"; id; side; classId; column; row; size; remaining; maximum; controller; script ] ->
                        match
                            parseInt line id,
                            sideFromName side,
                            parseInt line column,
                            parseInt line row,
                            parseInt line size,
                            parseInt line remaining,
                            parseInt line maximum,
                            controllerFromName controller,
                            (if script = "-" then Ok [] else parseScript script)
                        with
                        | Ok id, Some side, Ok column, Ok row, Ok size, Ok remaining, Ok maximum, Some controller, Ok script
                            when id > 0
                                 && not (Map.containsKey id map.Units)
                                 && classId.Length <= MaximumClassIdLength
                                 && size > 0
                                 && maximum > 0
                                 && remaining >= 0
                                 && remaining <= maximum ->
                            let unit =
                                { Id = id
                                  Side = side
                                  ClassId = classId
                                  Column = column
                                  Row = row
                                  Size = size
                                  Health = remaining
                                  HealthMaximum = maximum
                                  Controller = controller
                                  Script = script
                                  ScriptIndex = 0 }
                            result <-
                                Ok
                                    { map with
                                        Units = Map.add id unit map.Units
                                        NextUnitId = max map.NextUnitId (id + 1) }
                        | Error error, _, _, _, _, _, _, _, _
                        | _, _, Error error, _, _, _, _, _, _
                        | _, _, _, Error error, _, _, _, _, _
                        | _, _, _, _, Error error, _, _, _, _
                        | _, _, _, _, _, Error error, _, _, _
                        | _, _, _, _, _, _, Error error, _, _
                        | _, _, _, _, _, _, _, _, Error error ->
                            result <- Error error
                        | _ -> result <- fail line "invalid unit."
                    | _ -> result <- fail line "unknown record."

            result
            |> Result.bind (fun map ->
                let invalidTerrain =
                    map.Terrain
                    |> Map.toList
                    |> List.tryFind (fun ((column, row), _) ->
                        column < 0 || row < 0 || column >= map.Width || row >= map.Height)
                let invalidEdge =
                    map.Edges
                    |> Map.toList
                    |> List.tryFind (fun ((column, row, _), _) ->
                        column < 0 || row < 0 || column >= map.Width || row >= map.Height)
                let invalidEdgeState =
                    map.Edges
                    |> Map.toList
                    |> List.tryFind (fun (_, (kind, isOpen)) ->
                        isOpen && kind <> Door)
                let occupiedCellCounts =
                    map.Units
                    |> Map.toSeq
                    |> Seq.collect (fun (_, unit) ->
                        cells unit unit.Column unit.Row)
                    |> Seq.countBy id
                    |> Map.ofSeq
                let invalid =
                    map.Units
                    |> Map.toList
                    |> List.map snd
                    |> List.tryFind (fun unit ->
                        cells unit unit.Column unit.Row
                        |> List.exists (fun (column, row) ->
                            column < 0
                            || row < 0
                            || column >= map.Width
                            || row >= map.Height
                            || Map.tryFind (column, row) map.Terrain = Some Blocked
                            || Map.tryFind (column, row) occupiedCellCounts
                               |> Option.defaultValue 0
                               |> fun count -> count > 1))
                match invalidTerrain, invalidEdge, invalidEdgeState, invalid with
                | Some((column, row), _), _, _, _ ->
                    Error("Terrain cell " + string column + "," + string row + " is outside the map.")
                | _, Some((column, row, _), _), _, _ ->
                    Error("Edge " + string column + "," + string row + " is outside the map.")
                | _, _, Some _, _ ->
                    Error "Only doors may carry open edge state."
                | _, _, _, Some unit ->
                    Error("Unit " + string unit.Id + " does not fit the map.")
                | None, None, None, None ->
                    match invalidRegion map with
                    | Some error -> Error("Map zone: " + error)
                    | None -> Ok map)

    let private issue code message =
        { Code = code; Message = message }

    /// Returns deterministic, non-destructive semantic-edge diagnostics. Gaps
    /// are reported only when two collinear segments enclose missing records;
    /// intentional endpoints remain valid.
    let edgeIssues (map: MapDefinition) =
        let outsideAndState =
            map.Edges
            |> Map.toList
            |> List.collect (fun ((column, row, direction), (kind, isOpen)) ->
                [ if column < 0 || row < 0 || column >= map.Width || row >= map.Height then
                      issue
                          "EDGE-BORDER"
                          ("Edge "
                           + string column
                           + ","
                           + string row
                           + " has no canonical owning cell.")
                  if isOpen && kind <> Door then
                      issue
                          "EDGE-OVERLAP"
                          ("Only a door may carry open state at "
                           + string column
                           + ","
                           + string row
                           + " "
                           + edgeDirectionName direction
                           + ".") ])

        let gaps =
            [ for direction in [ EastEdge; SouthEdge ] do
                  let grouped =
                      map.Edges
                      |> Map.toList
                      |> List.choose (fun ((column, row, candidate), _) ->
                          if candidate <> direction then None
                          elif direction = EastEdge then Some(column, row)
                          else Some(row, column))
                      |> List.groupBy fst
                  for fixedCoordinate, values in grouped do
                      let coordinates = values |> List.map snd |> List.distinct |> List.sort
                      for first, last in List.pairwise coordinates do
                          if last - first > 1 then
                              for missing in first + 1 .. last - 1 do
                                  let column, row =
                                      if direction = EastEdge then fixedCoordinate, missing
                                      else missing, fixedCoordinate
                                  yield
                                      issue
                                          "EDGE-GAP"
                                          ("A collinear edge run has a gap at "
                                           + string column
                                           + ","
                                           + string row
                                           + " "
                                           + edgeDirectionName direction
                                           + ".") ]

        outsideAndState @ gaps

    /// Lints the complete leading side of every moving square footprint.
    let leadingSideMovementIssues map direction (units: EditorUnit array) =
        let dx, dy = directionDelta direction
        units
        |> Array.choose (fun unit ->
            if edgeBlocks map unit (int32 dx) (int32 dy) then
                Some(
                    issue
                        "EDGE-LEADING-SIDE"
                        ("Unit "
                         + string unit.Id
                         + " crosses a blocking edge on its complete leading side.")
                )
            else
                None)
        |> Array.toList

    let private polygonTwiceArea (vertices: EditorCellAddress array) =
        vertices
        |> Array.mapi (fun index point ->
            let next = vertices[(index + 1) % vertices.Length]
            int64 point.CellColumn * int64 next.CellRow
            - int64 next.CellColumn * int64 point.CellRow)
        |> Array.sum

    let private polygonSelfIntersects (vertices: EditorCellAddress array) =
        let orientation a b c =
            (b.CellColumn - a.CellColumn) * (c.CellRow - a.CellRow)
            - (b.CellRow - a.CellRow) * (c.CellColumn - a.CellColumn)
        let onSegment a b c =
            min a.CellColumn c.CellColumn <= b.CellColumn
            && b.CellColumn <= max a.CellColumn c.CellColumn
            && min a.CellRow c.CellRow <= b.CellRow
            && b.CellRow <= max a.CellRow c.CellRow
        let intersects a b c d =
            let first = orientation a b c
            let second = orientation a b d
            let third = orientation c d a
            let fourth = orientation c d b
            (sign first <> sign second && sign third <> sign fourth)
            || (first = 0 && onSegment a c b)
            || (second = 0 && onSegment a d b)
            || (third = 0 && onSegment c a d)
            || (fourth = 0 && onSegment c b d)

        [ for first in 0 .. vertices.Length - 1 do
              for second in first + 1 .. vertices.Length - 1 do
                  let adjacent =
                      second = first + 1
                      || (first = 0 && second = vertices.Length - 1)
                  if not adjacent then
                      let a = vertices[first]
                      let b = vertices[(first + 1) % vertices.Length]
                      let c = vertices[second]
                      let d = vertices[(second + 1) % vertices.Length]
                      yield intersects a b c d ]
        |> List.exists id

    /// Validates authoritative geometry independently from its semantic purpose.
    let regionIssues (map: MapDefinition) =
        [ if map.Regions.Count > MaximumRegionCount then
              issue
                  "REGION-LIMIT"
                  ("Maps support at most "
                   + string MaximumRegionCount
                   + " authoritative regions.")
          yield!
              map.Regions
              |> Map.toList
              |> List.collect (fun (id, region) ->
            [ if id <= 0 || id <> region.Id then
                  issue "REGION-IDENTITY" "Region keys and positive identifiers must agree."
              match region.Purpose with
              | DeploymentZone NeutralSide ->
                  issue "REGION-PURPOSE" "Deployment zones must belong to blue or red."
              | _ -> ()
              match region.Geometry with
              | RegionRectangle(column, row, width, height) ->
                  if width <= 0 || height <= 0 then
                      issue "REGION-RECTANGLE-SIZE" "Region rectangles must have positive width and height."
                  if
                      column < 0
                      || row < 0
                      || column + width > map.Width
                      || row + height > map.Height
                  then
                      issue "REGION-OUTSIDE" ("Region " + string id + " leaves the map boundary.")
              | RegionPolygon vertices ->
                  if vertices.Length < 3 then
                      issue "REGION-POLYGON-VERTICES" "Region polygons require at least three vertices."
                  if vertices.Length > MaximumRegionVertices then
                      issue
                          "REGION-POLYGON-LIMIT"
                          ("Region polygons support at most "
                           + string MaximumRegionVertices
                           + " vertices.")
                  if vertices |> Array.distinct |> Array.length <> vertices.Length then
                      issue "REGION-POLYGON-DUPLICATE" "Region polygon vertices must be unique."
                  if
                      vertices
                      |> Array.exists (fun vertex ->
                          vertex.CellColumn < 0
                          || vertex.CellRow < 0
                          || vertex.CellColumn > map.Width
                          || vertex.CellRow > map.Height)
                  then
                      issue "REGION-OUTSIDE" ("Region " + string id + " leaves the map boundary.")
                  if vertices.Length >= 3 && polygonTwiceArea vertices = 0L then
                      issue "REGION-POLYGON-AREA" "Region polygons must enclose non-zero area."
                  if vertices.Length >= 4 && polygonSelfIntersects vertices then
                      issue "REGION-POLYGON-SELF-INTERSECTION" "Region polygons cannot self-intersect." ]) ]

    let private validateDocument map =
        let dimensions =
            if map.Width < 4 || map.Width > 40 || map.Height < 4 || map.Height > 40 then
                [ issue "MAP-DIMENSIONS" "Map dimensions must be between 4 and 40 cells." ]
            else
                []

        let terrain =
            map.Terrain
            |> Map.toList
            |> List.choose (fun ((column, row), _) ->
                if column < 0 || row < 0 || column >= map.Width || row >= map.Height then
                    Some(issue "TERRAIN-OUTSIDE" "A terrain cell is outside the map.")
                else
                    None)

        let edges =
            map.Edges
            |> Map.toList
            |> List.choose (fun ((column, row, _), _) ->
                if column < 0 || row < 0 || column >= map.Width || row >= map.Height then
                    Some(issue "EDGE-OUTSIDE" "A semantic edge is outside the map.")
                else
                    None)

        let occupiedCellCounts =
            map.Units
            |> Map.toSeq
            |> Seq.collect (fun (_, unit) ->
                cells unit unit.Column unit.Row)
            |> Seq.countBy id
            |> Map.ofSeq

        let units =
            map.Units
            |> Map.toList
            |> List.choose (fun (id, unit) ->
                if id <> unit.Id || id <= 0 then
                    Some(issue "UNIT-IDENTITY" "Unit keys and positive identifiers must agree.")
                elif
                    String.IsNullOrWhiteSpace unit.ClassId
                    || unit.ClassId.Length > MaximumClassIdLength
                    || unit.ClassId |> Seq.exists Char.IsWhiteSpace
                then
                    Some(issue "UNIT-CLASS" "A unit class ID must be one non-empty token.")
                elif unit.HealthMaximum <= 0 || unit.Health < 0 || unit.Health > unit.HealthMaximum then
                    Some(issue "UNIT-HEALTH" "Unit health is outside its accepted range.")
                elif unit.Size < 1 || unit.Size > MaximumUnitFootprint then
                    Some(
                        issue
                            "UNIT-FOOTPRINT"
                            ("Unit footprints must be between 1 and "
                             + string MaximumUnitFootprint
                             + " cells square.")
                    )
                elif
                    cells unit unit.Column unit.Row
                    |> List.exists (fun (column, row) ->
                        column < 0
                        || row < 0
                        || column >= map.Width
                        || row >= map.Height
                        || Map.tryFind (column, row) map.Terrain = Some Blocked
                        || Map.tryFind (column, row) occupiedCellCounts
                           |> Option.defaultValue 0
                           |> fun count -> count > 1)
                then
                    Some(issue "UNIT-PLACEMENT" ("Unit " + string id + " does not fit."))
                else
                    None)

        dimensions @ terrain @ edges @ units @ regionIssues map

    /// Runs validation against the authoritative document only. Layer
    /// visibility and locks are deliberately ignored, so hidden content keeps
    /// participating in validation and simulation.
    let validationIssues map =
        (validateDocument map @ edgeIssues map)
        |> List.sortBy (fun candidate -> candidate.Code, candidate.Message)
        |> List.toArray

    let applyCommand (ValidatedEditorCommand command) map =
        match command with
        | PaintCells(terrain, addresses) ->
            let terrainMap =
                addresses
                |> Array.fold (fun current address ->
                    if terrain = Open then
                        Map.remove (address.CellColumn, address.CellRow) current
                    else
                        Map.add (address.CellColumn, address.CellRow) terrain current) map.Terrain
            { map with Terrain = terrainMap }
        | ReplaceEdges changes ->
            let edges =
                changes
                |> Array.fold (fun current (address, replacement) ->
                    match replacement with
                    | Some edge -> Map.add address edge current
                    | None -> Map.remove address current) map.Edges
            { map with Edges = edges }
        | AddUnits units
        | UpdateUnits units ->
            let nextUnits =
                units
                |> Array.fold (fun current unit -> Map.add unit.Id unit current) map.Units
            { map with
                Units = nextUnits
                NextUnitId =
                    units
                    |> Array.fold (fun next unit -> max next (unit.Id + 1)) map.NextUnitId }
        | RemoveUnits identifiers ->
            { map with
                Units =
                    identifiers
                    |> Array.fold (fun current id -> Map.remove id current) map.Units }
        | AddRegions regions
        | UpdateRegions regions ->
            { map with
                Regions =
                    regions
                    |> Array.fold (fun current region ->
                        Map.add region.Id region current) map.Regions
                NextRegionId =
                    regions
                    |> Array.fold (fun next region -> max next (region.Id + 1)) map.NextRegionId }
        | RemoveRegions identifiers ->
            { map with
                Regions =
                    identifiers
                    |> Array.fold (fun current id -> Map.remove id current) map.Regions }
        | ResizeDocument(width, height) ->
            { map with Width = width; Height = height }
        | ReplaceDocument(_, document) -> document

    let validateCommand map command =
        let duplicate values =
            values |> Array.distinct |> Array.length <> Array.length values
        let duplicateEdgeAddress changes =
            changes
            |> Array.groupBy fst
            |> Array.tryFind (fun (_, replacements) -> replacements.Length > 1)
            |> Option.map (fun (_, replacements) ->
                (replacements
                 |> Array.map snd
                 |> Array.distinct
                 |> Array.length) = 1)

        let shapeIssues =
            match command with
            | PaintCells(_, addresses) when Array.isEmpty addresses ->
                [ issue "COMMAND-EMPTY" "A paint command must contain at least one cell." ]
            | ReplaceEdges changes when Array.isEmpty changes ->
                [ issue "COMMAND-EMPTY" "An edge command must contain at least one change." ]
            | ReplaceEdges changes when duplicateEdgeAddress changes = Some true ->
                [ issue
                      "EDGE-DUPLICATE"
                      "An edge command contains the same canonical edge record more than once." ]
            | ReplaceEdges changes when duplicateEdgeAddress changes = Some false ->
                [ issue
                      "EDGE-OVERLAP"
                      "An edge command assigns conflicting meanings to one canonical edge record." ]
            | AddUnits units when Array.isEmpty units ->
                [ issue "COMMAND-EMPTY" "An add command must contain at least one unit." ]
            | AddUnits units when units |> Array.map _.Id |> duplicate ->
                [ issue "UNIT-DUPLICATE" "An add command contains duplicate unit identifiers." ]
            | AddUnits units when units |> Array.exists (fun unit -> Map.containsKey unit.Id map.Units) ->
                [ issue "UNIT-DUPLICATE" "An added unit identifier already exists." ]
            | UpdateUnits units when Array.isEmpty units ->
                [ issue "COMMAND-EMPTY" "An update command must contain at least one unit." ]
            | UpdateUnits units when units |> Array.map _.Id |> duplicate ->
                [ issue "UNIT-DUPLICATE" "An update command contains duplicate unit identifiers." ]
            | UpdateUnits units when units |> Array.exists (fun unit -> not (Map.containsKey unit.Id map.Units)) ->
                [ issue "UNIT-MISSING" "An updated unit does not exist." ]
            | RemoveUnits identifiers when Array.isEmpty identifiers ->
                [ issue "COMMAND-EMPTY" "A remove command must contain at least one unit." ]
            | RemoveUnits identifiers when duplicate identifiers ->
                [ issue "UNIT-DUPLICATE" "A remove command contains duplicate identifiers." ]
            | RemoveUnits identifiers when identifiers |> Array.exists (fun id -> not (Map.containsKey id map.Units)) ->
                [ issue "UNIT-MISSING" "A removed unit does not exist." ]
            | AddRegions regions when Array.isEmpty regions ->
                [ issue "COMMAND-EMPTY" "An add command must contain at least one region." ]
            | AddRegions regions when regions |> Array.map _.Id |> duplicate ->
                [ issue "REGION-DUPLICATE" "An add command contains duplicate region identifiers." ]
            | AddRegions regions when regions |> Array.exists (fun region -> Map.containsKey region.Id map.Regions) ->
                [ issue "REGION-DUPLICATE" "An added region identifier already exists." ]
            | UpdateRegions regions when Array.isEmpty regions ->
                [ issue "COMMAND-EMPTY" "An update command must contain at least one region." ]
            | UpdateRegions regions when regions |> Array.map _.Id |> duplicate ->
                [ issue "REGION-DUPLICATE" "An update command contains duplicate region identifiers." ]
            | UpdateRegions regions when regions |> Array.exists (fun region -> not (Map.containsKey region.Id map.Regions)) ->
                [ issue "REGION-MISSING" "An updated region does not exist." ]
            | RemoveRegions identifiers when Array.isEmpty identifiers ->
                [ issue "COMMAND-EMPTY" "A remove command must contain at least one region." ]
            | RemoveRegions identifiers when duplicate identifiers ->
                [ issue "REGION-DUPLICATE" "A remove command contains duplicate identifiers." ]
            | RemoveRegions identifiers when identifiers |> Array.exists (fun id -> not (Map.containsKey id map.Regions)) ->
                [ issue "REGION-MISSING" "A removed region does not exist." ]
            | ResizeDocument(width, height) when width < 4 || width > 40 || height < 4 || height > 40 ->
                [ issue "MAP-DIMENSIONS" "Map dimensions must be between 4 and 40 cells." ]
            | _ -> []

        if not (List.isEmpty shapeIssues) then
            Error shapeIssues
        else
            let candidate = applyCommand (ValidatedEditorCommand command) map
            match validateDocument candidate with
            | [] -> Ok(ValidatedEditorCommand command)
            | issues -> Error issues

    let private historyWithinBounds entries =
        let rec keep count bytes accepted remaining =
            match remaining with
            | [] -> List.rev accepted
            | entry :: tail
                when count < MaximumHistoryCommands
                     && bytes + entry.SerializedBytes <= MaximumHistoryBytes ->
                keep (count + 1) (bytes + entry.SerializedBytes) (entry :: accepted) tail
            | _ -> List.rev accepted

        keep 0 0 [] entries

    let private historySize entries =
        entries |> List.sumBy _.SerializedBytes

    let private selectedAfterMap map selected =
        selected |> Set.filter (fun id -> Map.containsKey id map.Units)

    let private commit command state =
        match validateCommand state.Map command with
        | Error issues ->
            { state with Validation = issues |> List.map _.Message |> String.concat " " |> Some }
        | Ok validated ->
            let map = applyCommand validated state.Map
            if map = state.Map then
                { state with Validation = None; Gesture = IdleGesture }
            else
                let before = state.Revision
                let after =
                    revision
                        (before.Number + 1L)
                        (Some before.Digest)
                        map
                let entry =
                    { Command = command
                      Before = before
                      After = after
                      SerializedBytes = serializedBytes before.Document + serializedBytes map }
                let undo = historyWithinBounds (entry :: state.UndoHistory)
                let selected = selectedAfterMap map state.SelectedUnits
                { state with
                    Map = map
                    SelectedUnits = selected
                    SelectedUnit =
                        state.SelectedUnit
                        |> Option.filter (fun id -> Set.contains id selected)
                        |> Option.orElseWith (fun () -> selected |> Set.toList |> List.tryHead)
                    SelectedRegion =
                        state.SelectedRegion
                        |> Option.filter (fun id -> Map.containsKey id map.Regions)
                    Gesture = IdleGesture
                    Revision = after
                    RevisionState = DirtyRevision
                    UndoHistory = undo
                    RedoHistory = []
                    HistoryBytes = historySize undo
                    Validation = None
                    Issues = validationIssues map
                    ActiveIssue =
                        if Array.isEmpty (validationIssues map) then None else Some 0
                    PendingDestructiveChange = None
                    Authoring =
                        { state.Authoring with
                            RevisionIdentity = after.Digest
                            ThumbnailSvg = None } }

    let private validEdgeKey map (column, row, _) =
        column >= 0 && row >= 0 && column < map.Width && row < map.Height

    let private edgeEndpoints (column, row, direction) =
        match direction with
        | EastEdge -> (column + 1, row), (column + 1, row + 1)
        | SouthEdge -> (column, row + 1), (column + 1, row + 1)

    let private connectingEdgePath map source target =
        let sourceEndpoints =
            let first, second = edgeEndpoints source
            [ first; second ]
        let targetEndpoints =
            let first, second = edgeEndpoints target
            [ first; second ]
        let route horizontalFirst start finish =
            let vertices = ResizeArray<int32 * int32>()
            let mutable x, y = start
            let finishX, finishY = finish
            vertices.Add(x, y)
            let horizontal () =
                while x <> finishX do
                    x <- x + (if finishX > x then 1 else -1)
                    vertices.Add(x, y)
            let vertical () =
                while y <> finishY do
                    y <- y + (if finishY > y then 1 else -1)
                    vertices.Add(x, y)
            if horizontalFirst then
                horizontal ()
                vertical ()
            else
                vertical ()
                horizontal ()
            vertices.ToArray()

        [ for sourcePoint in sourceEndpoints do
              for targetPoint in targetEndpoints do
                  for horizontalFirst in [ true; false ] do
                      let vertices = route horizontalFirst sourcePoint targetPoint
                      let normalized =
                          vertices
                          |> Seq.pairwise
                          |> Seq.map (fun ((x1, y1), (x2, y2)) ->
                              tryNormalizeEdge map.Width map.Height x1 y1 x2 y2)
                          |> Seq.toArray
                      if normalized |> Array.forall Option.isSome then
                          yield
                              normalized.Length,
                              sourcePoint,
                              targetPoint,
                              horizontalFirst,
                              (normalized |> Array.choose id) ]
        |> List.sortBy (fun (length, sourcePoint, targetPoint, horizontalFirst, _) ->
            length, sourcePoint, targetPoint, horizontalFirst)
        |> List.tryHead
        |> Option.map (fun (_, _, _, _, path) ->
            path
            |> Seq.append [ target ]
            |> Seq.distinct
            |> Seq.toArray)
        |> Option.defaultValue [| target |]

    let private finishEdgePolyline state =
        match state.Gesture with
        | EdgePolylineGesture(_, segments) when Array.isEmpty segments ->
            { state with
                Gesture = IdleGesture
                EdgeAnnouncement = "Empty edge polyline canceled." }
        | EdgePolylineGesture(kind, segments) ->
            let command =
                segments
                |> Array.distinct
                |> Array.map (fun address -> address, Some(kind, false))
                |> ReplaceEdges
            let next = commit command state
            { next with
                EdgeAnnouncement =
                    if next.Validation.IsSome then
                        "Edge polyline rejected. " + next.Validation.Value
                    else
                        "Committed "
                        + string segments.Length
                        + "-segment "
                        + edgeKindName kind
                        + " polyline in revision "
                        + string next.Revision.Number
                        + "." }
        | _ -> state

    let private replaceOneEdge address replacement announcement state =
        if not (validEdgeKey state.Map address) then
            { state with
                Validation = Some "The edge has no canonical owning cell."
                EdgeAnnouncement = "Edge change rejected at the map border." }
        else
            let next = commit (ReplaceEdges [| address, replacement |]) state
            { next with EdgeCursor = address; EdgeAnnouncement = announcement }

    let private activeDomain tool =
        match tool with
        | Paint _ -> TerrainDomain
        | Terrain _ -> TerrainDomain
        | Edge _ -> EdgeDomain
        | Place _ -> UnitDomain
        | Select -> UnitDomain

    let layerState domain state =
        state.Layers
        |> Map.tryFind domain
        |> Option.defaultValue VisibleLayer

    let private actionDomain action state =
        match action with
        | BeginTerrainGesture _
        | ExtendTerrainGesture _
        | ActivateTerrainCursor
        | ChooseTerrain _
        | SetTerrainBrushSize _ -> Some TerrainDomain
        | ActivateEdge _
        | MoveEdgeCursor _
        | ActivateEdgeCursor
        | FinishEdgePolyline
        | BacktrackEdgePolyline
        | ConvertEdge _
        | ToggleDoorState _
        | EraseEdge _
        | SplitEdge _
        | JoinEdge _ -> Some EdgeDomain
        | PreviewUnitPlacement _
        | BeginUnitMove _
        | ExtendUnitMove _
        | RemoveSelectedUnit
        | SetSelectedSide _
        | SetSelectedClass _
        | SetSelectedSize _
        | SetSelectedHealth _
        | SetSelectedController _
        | SetSelectedScript _
        | MoveSelected _
        | DeleteEditorSelection
        | PasteEditorClipboard
        | DuplicateEditorSelection -> Some UnitDomain
        | CreateRectangleRegion _
        | CreatePolygonRegion _
        | SetSelectedRegionPurpose _
        | SetSelectedRegionGeometry _
        | MoveSelectedRegion _
        | MoveSelectedRegionVertex _
        | RemoveSelectedRegion -> Some RegionDomain
        | Resize _
        | RequestNewMap
        | RequestClearMap
        | ClearMap
        | LoadMapText _
        | ConfirmDestructiveChange -> Some DocumentDomain
        | ActivateCell _ -> Some(activeDomain state.Tool)
        | CommitEditorGesture ->
            match state.Gesture with
            | TerrainGesture _ -> Some TerrainDomain
            | EdgePolylineGesture _ -> Some EdgeDomain
            | CommandPreviewGesture _
            | UnitMoveGesture _ -> Some UnitDomain
            | _ -> None
        | _ -> None

    let private isTerrainAuthoringTool tool =
        match tool with
        | Terrain _ -> true
        | _ -> false

    let private idsInBox box map =
        let minimumColumn = min box.FirstColumn box.LastColumn
        let maximumColumn = max box.FirstColumn box.LastColumn
        let minimumRow = min box.FirstRow box.LastRow
        let maximumRow = max box.FirstRow box.LastRow

        map.Units
        |> Map.toList
        |> List.choose (fun (id, unit) ->
            let lastColumn = unit.Column + unit.Size - 1
            let lastRow = unit.Row + unit.Size - 1
            if
                unit.Column <= maximumColumn
                && lastColumn >= minimumColumn
                && unit.Row <= maximumRow
                && lastRow >= minimumRow
            then Some id else None)
        |> Set.ofList

    let private translatedUnits offset (source: EditorUnit array) nextId =
        source
        |> Array.sortBy _.Id
        |> Array.mapi (fun index (unit: EditorUnit) ->
            { unit with
                Id = nextId + int32 index
                Column = unit.Column + offset
                Row = unit.Row + offset
                ScriptIndex = 0 })

    let private placementUnit side classId size address state =
        let preset =
            canonicalFootprintPresets
            |> List.tryFind (fun candidate ->
                candidate.Side = side
                && candidate.ClassId = classId
                && candidate.FootprintSize = size)

        { Id = state.Map.NextUnitId
          Side = side
          ClassId = classId
          Column = address.CellColumn
          Row = address.CellRow
          Size = size
          Health = preset |> Option.map _.Health |> Option.defaultValue 12
          HealthMaximum = preset |> Option.map _.HealthMaximum |> Option.defaultValue 12
          Controller = Manual
          Script = []
          ScriptIndex = 0 }

    let private selectedUnits state =
        state.SelectedUnits
        |> Set.toArray
        |> Array.choose (fun id -> Map.tryFind id state.Map.Units)
        |> Array.sortBy _.Id

    let private translatedRegionGeometry columnDelta rowDelta geometry =
        match geometry with
        | RegionRectangle(column, row, width, height) ->
            RegionRectangle(column + columnDelta, row + rowDelta, width, height)
        | RegionPolygon vertices ->
            vertices
            |> Array.map (fun vertex ->
                { CellColumn = vertex.CellColumn + columnDelta
                  CellRow = vertex.CellRow + rowDelta })
            |> RegionPolygon

    let private translatedSelection columnDelta rowDelta (source: EditorUnit array) =
        source
        |> Array.map (fun unit ->
            { unit with
                Column = unit.Column + columnDelta
                Row = unit.Row + rowDelta })

    let private movementCrossesEdge map columnDelta rowDelta (source: EditorUnit array) =
        let stepX = int32 (sign columnDelta)
        let stepY = int32 (sign rowDelta)
        let steps = max (abs columnDelta) (abs rowDelta)

        if steps = 0 then
            false
        else
            [ 0 .. int steps - 1 ]
            |> List.exists (fun step ->
                let offset = int32 step
                source
                |> Array.exists (fun unit ->
                    let intermediate =
                        { unit with
                            Column = unit.Column + stepX * offset
                            Row = unit.Row + stepY * offset }
                    edgeBlocks map intermediate stepX stepY))

    let private unitPreviewMessage prefix map command =
        match validateCommand map command with
        | Ok _ -> prefix + " Valid destination."
        | Error issues ->
            prefix
            + " Invalid destination: "
            + (issues |> List.map _.Message |> String.concat " ")

    let private tryTranslatedCommand (source: EditorUnit array) state =
        [| 1 .. int (max state.Map.Width state.Map.Height) |]
        |> Array.tryPick (fun offset ->
            let units = translatedUnits (int32 offset) source state.Map.NextUnitId
            let command = AddUnits units
            match validateCommand state.Map command with
            | Ok _ -> Some(command, units)
            | Error _ -> None)

    let private legacyCommand action map =
        let reason =
            match action with
            | ActivateCell _ -> "activate"
            | Resize _ -> "resize"
            | RemoveSelectedUnit -> "remove"
            | SetSelectedSide _
            | SetSelectedClass _
            | SetSelectedSize _
            | SetSelectedHealth _
            | SetSelectedController _
            | SetSelectedScript _ -> "update-unit"
            | MoveSelected _ -> "move-unit"
            | StepEditor -> "simulation-step"
            | ClearMap -> "clear"
            | LoadMapText _ -> "import"
            | _ -> "editor"
        ReplaceDocument(reason, map)

    let rec update action state =
        let state =
            match action, state.Gesture with
            | ChooseTool _, EdgePolylineGesture _ -> finishEdgePolyline state
            | _ -> state
        match actionDomain action state with
        | Some domain when layerState domain state = LockedLayer ->
            { state with
                Validation = Some("The " + string domain + " layer is locked.") }
        | _ -> unlockedUpdate action state

    and private unlockedUpdate action state =
        match action with
        | SetEditorLayerState(domain, value) ->
          { state with
              Layers = Map.add domain value state.Layers
              Validation = None }
        | SelectNextIssue ->
          if Array.isEmpty state.Issues then
              { state with ActiveIssue = None }
          else
              let current = state.ActiveIssue |> Option.defaultValue -1
              { state with ActiveIssue = Some((current + 1) % state.Issues.Length) }
        | SelectPreviousIssue ->
          if Array.isEmpty state.Issues then
              { state with ActiveIssue = None }
          else
              let current = state.ActiveIssue |> Option.defaultValue 0
              { state with
                  ActiveIssue =
                      Some((current - 1 + state.Issues.Length) % state.Issues.Length) }
        | SetMapName name ->
          let normalized =
              if String.IsNullOrWhiteSpace name then "Untitled battlefield"
              else name.Trim()
          { state with Authoring = { state.Authoring with Name = normalized } }
        | SaveMapView(name, camera) ->
          let normalized = name.Trim()
          if String.IsNullOrWhiteSpace normalized then
              { state with Validation = Some "Saved view names cannot be empty." }
          else
              let view = { Name = normalized; Camera = camera }
              { state with
                  Authoring =
                      { state.Authoring with
                          SavedViews =
                              Map.add normalized view state.Authoring.SavedViews }
                  Validation = None }
        | RemoveMapView name ->
          { state with
              Authoring =
                  { state.Authoring with
                      SavedViews = Map.remove name state.Authoring.SavedViews } }
        | SetMapThumbnail thumbnail ->
          { state with
              Authoring = { state.Authoring with ThumbnailSvg = thumbnail } }
        | CancelDestructiveChange ->
          { state with PendingDestructiveChange = None; Validation = None }
        | RequestClearMap ->
          { state with
              PendingDestructiveChange = Some ClearPending
              Validation =
                  Some
                      "Clearing removes every terrain cell, edge, unit, and region. Confirm to continue." }
        | RequestNewMap ->
          { state with
              PendingDestructiveChange =
                  Some(NewMapPending(12, 8, "Untitled battlefield"))
              Validation =
                  Some
                      "Creating a new map replaces this draft with an empty 12 by 8 document. Confirm to continue." }
        | ConfirmDestructiveChange ->
          match state.PendingDestructiveChange with
          | Some(ResizePending preview) ->
              commit
                  (ReplaceDocument(
                      "confirmed-resize",
                      resizedDocument preview state.Map
                  ))
                  state
          | Some ClearPending ->
              commit
                  (ReplaceDocument(
                      "confirmed-clear",
                      emptyMap state.Map.Width state.Map.Height
                  ))
                  state
          | Some(NewMapPending(width, height, name)) ->
              let replaced =
                  commit
                      (ReplaceDocument(
                          "confirmed-new-map",
                          emptyMap width height
                      ))
                      state
              { replaced with
                  Authoring =
                      { Name = name
                        SavedViews = Map.empty
                        RevisionIdentity = replaced.Revision.Digest
                        ThumbnailSvg = None }
                  SelectedUnit = None
                  SelectedUnits = Set.empty
                  SelectedRegion = None
                  Tool = Select }
          | None -> state
        | OfferCrashRecovery text ->
          match tryImport text with
          | Ok map ->
              let digest = revisionDigest map
              if digest = state.Revision.Digest then state
              else
                  { state with
                      PendingRecovery = Some { SourceDigest = digest; Map = map } }
          | Error _ -> state
        | RecoverCrashDraft ->
          match state.PendingRecovery with
          | None -> state
          | Some draft ->
              let recovered =
                  commit
                      (ReplaceDocument("crash-recovery", draft.Map))
                      { state with PendingRecovery = None }
              { recovered with
                  RevisionState = RecoveredRevision
                  RecoveredFromDigest = Some draft.SourceDigest }
        | DiscardCrashDraft ->
          { state with PendingRecovery = None }
        | ChooseTool(Edge(direction, kind)) ->
          let column, row, _ = state.EdgeCursor
          { state with
              Tool = Edge(direction, kind)
              EdgeCursor = column, row, direction
              Gesture = IdleGesture
              Validation = None
              EdgeAnnouncement =
                  edgeKindName kind
                  + " "
                  + edgeDirectionName direction
                  + " edge tool selected." }
        | ChooseTool(Paint terrain) ->
            { state with
                Tool = Paint terrain
                TerrainSelection = terrain
                Gesture = IdleGesture
                Validation = None
                TerrainAnnouncement =
                    "Pencil selected with "
                    + terrainName terrain
                    + " terrain." }
        | ChooseTool(Terrain tool) ->
            { state with
                Tool = Terrain tool
                Gesture = IdleGesture
                Validation = None
                TerrainAnnouncement =
                    (match tool with
                     | PencilTool -> "Pencil"
                     | RectangleTool -> "Rectangle"
                     | LineTool -> "Line"
                     | FloodFillTool -> "Flood fill"
                     | EyedropperTool -> "Eyedropper"
                     | EraseTool -> "Erase")
                    + " terrain tool selected." }
        | ChooseTerrain terrain ->
            { state with
                TerrainSelection = terrain
                Validation = None
                TerrainAnnouncement = terrainName terrain + " terrain selected." }
        | SetUnitPaletteSearch query ->
            let count = searchCanonicalUnitPresets query |> List.length
            { state with
                UnitPaletteSearch = query
                UnitAnnouncement =
                    string count
                    + " canonical unit "
                    + (if count = 1 then "preset" else "presets")
                    + " shown." }
        | CreateRectangleRegion(purpose, first, last) ->
            let column = min first.CellColumn last.CellColumn
            let row = min first.CellRow last.CellRow
            let region =
                { Id = state.Map.NextRegionId
                  Geometry =
                    RegionRectangle(
                        column,
                        row,
                        abs (last.CellColumn - first.CellColumn) + 1,
                        abs (last.CellRow - first.CellRow) + 1
                    )
                  Purpose = purpose
                  Behavior = NoRegionBehavior }
            let next = commit (AddRegions [| region |]) state
            if next.Revision.Number = state.Revision.Number then
                { next with RegionAnnouncement = "Region creation rejected." }
            else
                { next with
                    SelectedRegion = Some region.Id
                    SelectedUnit = None
                    SelectedUnits = Set.empty
                    RegionAnnouncement =
                        regionPurposeLabel purpose
                        + " rectangle created as region "
                        + string region.Id
                        + "." }
        | CreatePolygonRegion(purpose, vertices) ->
            let region =
                { Id = state.Map.NextRegionId
                  Geometry = RegionPolygon(Array.copy vertices)
                  Purpose = purpose
                  Behavior = NoRegionBehavior }
            let next = commit (AddRegions [| region |]) state
            if next.Revision.Number = state.Revision.Number then
                { next with RegionAnnouncement = "Region creation rejected." }
            else
                { next with
                    SelectedRegion = Some region.Id
                    SelectedUnit = None
                    SelectedUnits = Set.empty
                    RegionAnnouncement =
                        regionPurposeLabel purpose
                        + " polygon created as region "
                        + string region.Id
                        + "." }
        | SelectEditorRegion id ->
            let selected = id |> Option.filter (fun value -> Map.containsKey value state.Map.Regions)
            { state with
                SelectedRegion = selected
                SelectedUnit = None
                SelectedUnits = Set.empty
                Validation = None
                RegionAnnouncement =
                    selected
                    |> Option.map (fun value -> "Region " + string value + " selected.")
                    |> Option.defaultValue "Region selection cleared." }
        | SetSelectedRegionPurpose purpose ->
            match state.SelectedRegion |> Option.bind (fun id -> Map.tryFind id state.Map.Regions) with
            | None -> { state with Validation = Some "Select a region first." }
            | Some region ->
                let next = commit (UpdateRegions [| { region with Purpose = purpose } |]) state
                { next with RegionAnnouncement = regionPurposeLabel purpose + " purpose applied." }
        | SetSelectedRegionGeometry geometry ->
            match state.SelectedRegion |> Option.bind (fun id -> Map.tryFind id state.Map.Regions) with
            | None -> { state with Validation = Some "Select a region first." }
            | Some region ->
                let next = commit (UpdateRegions [| { region with Geometry = geometry } |]) state
                { next with RegionAnnouncement = "Region geometry updated." }
        | MoveSelectedRegion(columnDelta, rowDelta) ->
            match state.SelectedRegion |> Option.bind (fun id -> Map.tryFind id state.Map.Regions) with
            | None -> { state with Validation = Some "Select a region first." }
            | Some region ->
                let geometry = translatedRegionGeometry columnDelta rowDelta region.Geometry
                let next = commit (UpdateRegions [| { region with Geometry = geometry } |]) state
                { next with
                    RegionAnnouncement =
                        if next.Validation.IsSome then "Region move rejected."
                        else "Region moved." }
        | MoveSelectedRegionVertex(index, columnDelta, rowDelta) ->
            match state.SelectedRegion |> Option.bind (fun id -> Map.tryFind id state.Map.Regions) with
            | Some({ Geometry = RegionPolygon vertices } as region)
                when index >= 0 && index < vertices.Length ->
                let moved = Array.copy vertices
                let vertex = moved[index]
                moved[index] <-
                    { CellColumn = vertex.CellColumn + columnDelta
                      CellRow = vertex.CellRow + rowDelta }
                let next =
                    commit
                        (UpdateRegions [| { region with Geometry = RegionPolygon moved } |])
                        state
                { next with RegionAnnouncement = "Polygon vertex updated." }
            | Some _ -> { state with Validation = Some "Select a polygon vertex first." }
            | None -> { state with Validation = Some "Select a region first." }
        | RemoveSelectedRegion ->
            match state.SelectedRegion with
            | None -> { state with Validation = Some "Select a region first." }
            | Some id ->
                let next = commit (RemoveRegions [| id |]) state
                { next with
                    SelectedRegion = None
                    RegionAnnouncement = "Region " + string id + " removed." }
        | PreviewUnitPlacement address ->
            match state.Tool with
            | Place(side, classId, size) ->
                let unit = placementUnit side classId size address state
                let command = AddUnits [| unit |]
                { state with
                    Gesture = CommandPreviewGesture command
                    Validation = None
                    UnitAnnouncement =
                        unitPreviewMessage
                            ("Placement preview for "
                             + unit.ClassId
                             + ", "
                             + string unit.Size
                             + " by "
                             + string unit.Size
                             + " cells.")
                            state.Map
                            command }
            | _ -> state
        | BeginUnitMove address ->
            let original = selectedUnits state
            if Array.isEmpty original then
                { state with Validation = Some "Select at least one unit to move." }
            else
                let command = UpdateUnits original
                { state with
                    Gesture = UnitMoveGesture(address, address, original, command)
                    Validation = None
                    UnitAnnouncement =
                        "Moving "
                        + string original.Length
                        + (if original.Length = 1 then " unit." else " units as one formation.") }
        | ExtendUnitMove address ->
            match state.Gesture with
            | UnitMoveGesture(anchor, _, original, _) ->
                let columnDelta = address.CellColumn - anchor.CellColumn
                let rowDelta = address.CellRow - anchor.CellRow
                let units = translatedSelection columnDelta rowDelta original
                let command = UpdateUnits units
                let prefix =
                    "Movement preview "
                    + string columnDelta
                    + " columns, "
                    + string rowDelta
                    + " rows."
                let announcement =
                    if movementCrossesEdge state.Map columnDelta rowDelta original then
                        prefix + " Invalid destination: a blocking edge crosses the route."
                    else
                        unitPreviewMessage prefix state.Map command
                { state with
                    Gesture = UnitMoveGesture(anchor, address, original, command)
                    UnitAnnouncement = announcement
                    Validation = None }
            | _ -> state
        | SetTerrainBrushSize size ->
            let size = max 1 (min 9 size)
            { state with
                BrushSize = size
                Gesture = IdleGesture
                Validation = None
                TerrainAnnouncement =
                    "Brush size "
                    + string size
                    + " by "
                    + string size
                    + " cells." }
        | BeginTerrainGesture address when inBounds state.Map address ->
            match state.Tool with
            | Terrain EyedropperTool ->
                let sampled =
                    Map.tryFind
                        (address.CellColumn, address.CellRow)
                        state.Map.Terrain
                    |> Option.defaultValue Open
                { state with
                    TerrainSelection = sampled
                    TerrainCursor = address
                    Gesture = IdleGesture
                    Validation = None
                    TerrainAnnouncement =
                        "Sampled "
                        + terrainName sampled
                        + " terrain at column "
                        + string (address.CellColumn + 1)
                        + ", row "
                        + string (address.CellRow + 1)
                        + "." }
            | Terrain tool ->
                let preview =
                    terrainGestureAddresses
                        state.Map
                        state.BrushSize
                        tool
                        address
                        address
                        [||]
                { state with
                    TerrainCursor = address
                    Gesture = TerrainGesture(tool, address, address, [||])
                    Validation = None
                    TerrainAnnouncement =
                        string preview.Length
                        + " terrain "
                        + (if preview.Length = 1 then "cell" else "cells")
                        + " previewed." }
            | _ -> state
        | BeginTerrainGesture _ -> state
        | ExtendTerrainGesture address when inBounds state.Map address ->
            match state.Gesture with
            | TerrainGesture(tool, anchor, current, visited) ->
                let nextAnchor, nextVisited =
                    match tool with
                    | PencilTool
                    | EraseTool ->
                        current,
                        visited
                        |> Array.append (lineAddresses anchor current)
                        |> normalizeAddresses
                    | _ -> anchor, visited
                let preview =
                    terrainGestureAddresses
                        state.Map
                        state.BrushSize
                        tool
                        nextAnchor
                        address
                        nextVisited
                { state with
                    TerrainCursor = address
                    Gesture =
                        TerrainGesture(
                            tool,
                            nextAnchor,
                            address,
                            nextVisited
                        )
                    TerrainAnnouncement =
                        string preview.Length
                        + " terrain "
                        + (if preview.Length = 1 then "cell" else "cells")
                        + " previewed." }
            | _ -> state
        | ExtendTerrainGesture _ -> state
        | MoveTerrainCursor(columnDelta, rowDelta, extendPreview) ->
            let cursor =
                { CellColumn =
                    max 0 (min (state.Map.Width - 1) (state.TerrainCursor.CellColumn + columnDelta))
                  CellRow =
                    max 0 (min (state.Map.Height - 1) (state.TerrainCursor.CellRow + rowDelta)) }
            let moved = { state with TerrainCursor = cursor }
            if extendPreview then update (ExtendTerrainGesture cursor) moved
            else moved
        | ActivateTerrainCursor ->
            match state.Gesture with
            | TerrainGesture _ -> update CommitEditorGesture state
            | _ -> update (BeginTerrainGesture state.TerrainCursor) state
        | ActivateEdge(column, row, direction) ->
            let address = column, row, direction
            if not (validEdgeKey state.Map address) then
                { state with
                    Validation = Some "The edge has no canonical owning cell."
                    EdgeAnnouncement = "Edge placement rejected at the map border." }
            else
                let state =
                    match state.Tool with
                    | Edge(_, kind) ->
                        { state with Tool = Edge(direction, kind) }
                    | _ -> state
                match state.Tool with
                | Edge(_, Wall) ->
                    match state.Gesture with
                    | EdgePolylineGesture(Wall, segments) when Array.contains address segments ->
                        { state with
                            EdgeCursor = address
                            Validation = Some "This canonical edge is already in the polyline."
                            EdgeAnnouncement = "Duplicate edge segment ignored." }
                    | EdgePolylineGesture(Wall, segments) ->
                        let path =
                            connectingEdgePath
                                state.Map
                                segments[segments.Length - 1]
                                address
                        let segments =
                            Array.append segments path
                            |> Array.distinct
                        { state with
                            EdgeCursor = address
                            Gesture = EdgePolylineGesture(Wall, segments)
                            Validation = None
                            EdgeAnnouncement =
                                string segments.Length
                                + " wall polyline segments previewed. Double-click or press Enter to finish." }
                    | _ ->
                        { state with
                            EdgeCursor = address
                            Gesture = EdgePolylineGesture(Wall, [| address |])
                            Validation = None
                            EdgeAnnouncement = "Wall polyline started with one segment." }
                | Edge(_, kind) ->
                    replaceOneEdge
                        address
                        (Some(kind, false))
                        ("Converted edge to " + edgeKindName kind + ".")
                        state
                | _ -> state
        | MoveEdgeCursor(columnDelta, rowDelta, extendPreview) ->
            let column, row, direction = state.EdgeCursor
            let cursor =
                max 0 (min (state.Map.Width - 1) (column + columnDelta)),
                max 0 (min (state.Map.Height - 1) (row + rowDelta)),
                direction
            let moved =
                { state with
                    EdgeCursor = cursor
                    EdgeAnnouncement =
                        "Edge cursor at column "
                        + string ((let c, _, _ = cursor in c) + 1)
                        + ", row "
                        + string ((let _, r, _ = cursor in r) + 1)
                        + ", "
                        + edgeDirectionName direction
                        + "." }
            if extendPreview then
                let c, r, d = cursor
                update (ActivateEdge(c, r, d)) moved
            else
                moved
        | ActivateEdgeCursor ->
            let column, row, direction = state.EdgeCursor
            update (ActivateEdge(column, row, direction)) state
        | FinishEdgePolyline -> finishEdgePolyline state
        | BacktrackEdgePolyline ->
            match state.Gesture with
            | EdgePolylineGesture(kind, segments) when segments.Length > 0 ->
                let remaining = segments |> Array.take (segments.Length - 1)
                { state with
                    Gesture = EdgePolylineGesture(kind, remaining)
                    Validation = None
                    EdgeAnnouncement =
                        if Array.isEmpty remaining then
                            "Last edge segment removed. Press Escape again to cancel."
                        else
                            "Last edge segment removed; "
                            + string remaining.Length
                            + " remain." }
            | EdgePolylineGesture _ ->
                { state with
                    Gesture = IdleGesture
                    Validation = None
                    EdgeAnnouncement = "Edge polyline canceled." }
            | _ -> state
        | ConvertEdge(column, row, direction, kind) ->
            replaceOneEdge
                (column, row, direction)
                (Some(kind, false))
                ("Converted edge to " + edgeKindName kind + ".")
                state
        | ToggleDoorState(column, row, direction) ->
            let address = column, row, direction
            match Map.tryFind address state.Map.Edges with
            | Some(Door, isOpen) ->
                replaceOneEdge
                    address
                    (Some(Door, not isOpen))
                    (if isOpen then "Door closed." else "Door opened.")
                    state
            | _ ->
                { state with
                    EdgeCursor = address
                    Validation = Some "Only a door has editable open/closed state."
                    EdgeAnnouncement = "Select or create a door before toggling its state." }
        | EraseEdge(column, row, direction)
        | SplitEdge(column, row, direction) ->
            replaceOneEdge
                (column, row, direction)
                None
                (match action with
                 | SplitEdge _ -> "Edge run split by removing one canonical segment."
                 | _ -> "Edge erased.")
                state
        | JoinEdge(column, row, direction) ->
            replaceOneEdge
                (column, row, direction)
                (Some(Wall, false))
                "Edge run joined with one canonical wall segment."
                state
        | ActivateCell(column, row) when
            match state.Tool with
            | Place _ -> true
            | _ -> false
            ->
            state
            |> update (
                PreviewUnitPlacement
                    { CellColumn = column
                      CellRow = row }
            )
            |> update CommitEditorGesture
        | ActivateCell(column, row) when isTerrainAuthoringTool state.Tool ->
            state
            |> update (
                BeginTerrainGesture
                    { CellColumn = column
                      CellRow = row }
            )
            |> fun preview ->
                match preview.Gesture with
                | TerrainGesture _ -> update CommitEditorGesture preview
                | _ -> preview
        | SelectEditorUnit id ->
            let selected =
                id
                |> Option.filter (fun identifier -> Map.containsKey identifier state.Map.Units)
                |> Option.map Set.singleton
                |> Option.defaultValue Set.empty
            { state with
                Tool = Select
                SelectedUnit = id |> Option.filter (fun identifier -> Set.contains identifier selected)
                SelectedUnits = selected
                SelectedRegion = None
                Gesture = IdleGesture
                Validation = None }
        | ToggleEditorUnitSelection id when Map.containsKey id state.Map.Units ->
            let selected =
                if Set.contains id state.SelectedUnits then Set.remove id state.SelectedUnits
                else Set.add id state.SelectedUnits
            { state with
                Tool = Select
                SelectedUnits = selected
                SelectedUnit = if Set.contains id selected then Some id else selected |> Set.toList |> List.tryHead
                SelectedRegion = None
                Validation = None }
        | ToggleEditorUnitSelection _ -> state
        | SelectEditorUnitsInBox box ->
            let selected = idsInBox box state.Map
            { state with
                Tool = Select
                SelectedUnits = selected
                SelectedUnit = selected |> Set.toList |> List.tryHead
                Gesture = IdleGesture
                Validation = None }
        | BeginEditorBoxSelection address ->
            { state with Gesture = BoxSelectionGesture(address, address); Validation = None }
        | ExtendEditorBoxSelection address ->
            match state.Gesture with
            | BoxSelectionGesture(anchor, _) ->
                { state with Gesture = BoxSelectionGesture(anchor, address) }
            | _ -> state
        | CommitEditorGesture ->
            match state.Gesture with
            | BoxSelectionGesture(anchor, current) ->
                update
                    (SelectEditorUnitsInBox
                          { FirstColumn = anchor.CellColumn
                            FirstRow = anchor.CellRow
                            LastColumn = current.CellColumn
                            LastRow = current.CellRow })
                    state
            | CommandPreviewGesture command -> commit command state
            | UnitMoveGesture(anchor, current, original, command) ->
                let columnDelta = current.CellColumn - anchor.CellColumn
                let rowDelta = current.CellRow - anchor.CellRow
                if movementCrossesEdge state.Map columnDelta rowDelta original then
                    { state with
                        Gesture = IdleGesture
                        Validation = Some "The formation route crosses a blocking edge."
                        UnitAnnouncement = "Movement rejected by a blocking edge." }
                else
                    let next = commit command state
                    { next with
                        UnitAnnouncement =
                            if next.Validation.IsSome then
                                "Movement rejected. " + next.Validation.Value
                            elif next.Revision.Digest = state.Revision.Digest then
                                "Formation did not move."
                            else
                                "Moved "
                                + string original.Length
                                + (if original.Length = 1 then " unit" else " units")
                                + " atomically." }
            | TerrainGesture(tool, anchor, current, visited) ->
                let command =
                    terrainGestureCommand state tool anchor current visited
                let next = commit command state
                if next.Gesture = IdleGesture && next.Validation.IsNone then
                    if next.Revision.Digest = state.Revision.Digest then
                        { next with
                            TerrainAnnouncement = "No terrain cells changed." }
                    else
                        match command with
                        | PaintCells(terrain, addresses) ->
                            { next with
                                TerrainAnnouncement =
                                    (if terrain = Open then "Erased " else "Painted ")
                                    + string addresses.Length
                                    + " terrain "
                                    + (if addresses.Length = 1 then "cell" else "cells")
                                    + " in revision "
                                    + string next.Revision.Number
                                    + "." }
                        | _ -> next
                else
                    { next with
                        Gesture = IdleGesture
                        TerrainAnnouncement =
                            "Terrain change rejected. "
                            + (next.Validation |> Option.defaultValue "The preview is invalid.") }
            | EdgePolylineGesture _ -> finishEdgePolyline state
            | IdleGesture -> state
        | CancelEditorGesture ->
            match state.Gesture with
            | EdgePolylineGesture _ -> update BacktrackEdgePolyline state
            | _ ->
                { state with
                    Gesture = IdleGesture
                    Validation = None
                    TerrainAnnouncement =
                        match state.Gesture with
                        | TerrainGesture _ -> "Terrain preview canceled."
                        | _ -> state.TerrainAnnouncement }
        | SelectAllInActiveDomain ->
            if activeDomain state.Tool = UnitDomain then
                let selected = state.Map.Units |> Map.toList |> List.map fst |> Set.ofList
                { state with
                    SelectedUnits = selected
                    SelectedUnit = selected |> Set.toList |> List.tryHead
                    SelectedRegion = None
                    Validation = None }
            else
                { state with
                    SelectedUnits = Set.empty
                    SelectedUnit = None
                    Validation = Some "The active domain has no selectable objects yet." }
        | CopyEditorSelection ->
            let units =
                state.SelectedUnits
                |> Set.toArray
                |> Array.choose (fun id -> Map.tryFind id state.Map.Units)
                |> Array.sortBy _.Id
            if Array.isEmpty units then state
            elif units.Length > MaximumClipboardUnits then
                { state with
                    Clipboard = None
                    Validation =
                        Some(
                            "Clipboard selections are limited to "
                            + string MaximumClipboardUnits
                            + " units."
                        ) }
            else
                { state with
                    Clipboard = Some { SourceDigest = state.Revision.Digest; UnitFragment = units }
                    Validation = None }
        | PasteEditorClipboard ->
            match state.Clipboard with
            | None -> { state with Validation = Some "Copy units before pasting." }
            | Some clipboard ->
                match tryTranslatedCommand clipboard.UnitFragment state with
                | None -> { state with Validation = Some "The copied formation does not fit." }
                | Some(command, units) ->
                    let next = commit command state
                    { next with
                        SelectedUnits = units |> Array.map _.Id |> Set.ofArray
                        SelectedUnit = units |> Array.tryHead |> Option.map _.Id }
        | DuplicateEditorSelection ->
            let copied = update CopyEditorSelection state
            update PasteEditorClipboard copied
        | DeleteEditorSelection ->
            if Set.isEmpty state.SelectedUnits then state
            else commit (RemoveUnits(state.SelectedUnits |> Set.toArray)) state
        | SetSelectedSide side when state.RevisionState <> SimulatedRevision ->
            selectedUnits state
            |> Array.map (fun unit -> { unit with Side = side })
            |> UpdateUnits
            |> fun command -> commit command state
        | SetSelectedClass classId when state.RevisionState <> SimulatedRevision ->
            let classId = classId.Trim()
            if
                String.IsNullOrWhiteSpace classId
                || classId.Length > MaximumClassIdLength
                || classId |> Seq.exists Char.IsWhiteSpace
            then
                { state with
                    Validation =
                        Some(
                            "Class ID must be one non-empty token no longer than "
                            + string MaximumClassIdLength
                            + " characters."
                        ) }
            else
                selectedUnits state
                |> Array.map (fun unit -> { unit with ClassId = classId })
                |> UpdateUnits
                |> fun command -> commit command state
        | SetSelectedSize size when state.RevisionState <> SimulatedRevision ->
            selectedUnits state
            |> Array.map (fun unit -> { unit with Size = size })
            |> UpdateUnits
            |> fun command -> commit command state
        | SetSelectedHealth(remaining, maximum) when state.RevisionState <> SimulatedRevision ->
            selectedUnits state
            |> Array.map (fun unit ->
                { unit with
                    Health = remaining
                    HealthMaximum = maximum })
            |> UpdateUnits
            |> fun command -> commit command state
        | SetSelectedController controller when state.RevisionState <> SimulatedRevision ->
            selectedUnits state
            |> Array.map (fun unit -> { unit with Controller = controller })
            |> UpdateUnits
            |> fun command -> commit command state
        | SetSelectedScript text when state.RevisionState <> SimulatedRevision ->
            match parseScript text with
            | Error error -> { state with Validation = Some error }
            | Ok script ->
                selectedUnits state
                |> Array.map (fun unit ->
                    { unit with
                        Script = script
                        ScriptIndex = 0 })
                |> UpdateUnits
                |> fun command -> commit command state
        | UndoEditorCommand ->
            match state.UndoHistory with
            | [] -> state
            | entry :: remaining ->
                let selected = selectedAfterMap entry.Before.Document state.SelectedUnits
                { state with
                    Map = entry.Before.Document
                    Revision = entry.Before
                    RevisionState =
                        if state.SavedDigest = Some entry.Before.Digest then SavedRevision
                        else DirtyRevision
                    SelectedUnits = selected
                    SelectedUnit = selected |> Set.toList |> List.tryHead
                    SelectedRegion =
                        state.SelectedRegion
                        |> Option.filter (fun id -> Map.containsKey id entry.Before.Document.Regions)
                    Gesture = IdleGesture
                    UndoHistory = remaining
                    RedoHistory = entry :: state.RedoHistory |> historyWithinBounds
                    HistoryBytes = historySize remaining
                    Validation = None
                    Issues = validationIssues entry.Before.Document
                    ActiveIssue =
                        if Array.isEmpty (validationIssues entry.Before.Document) then None else Some 0
                    Authoring =
                        { state.Authoring with
                            RevisionIdentity = entry.Before.Digest
                            ThumbnailSvg = None } }
        | RedoEditorCommand ->
            match state.RedoHistory with
            | [] -> state
            | entry :: remaining ->
                let selected = selectedAfterMap entry.After.Document state.SelectedUnits
                let undo = entry :: state.UndoHistory |> historyWithinBounds
                { state with
                    Map = entry.After.Document
                    Revision = entry.After
                    RevisionState =
                        if state.SavedDigest = Some entry.After.Digest then SavedRevision
                        else DirtyRevision
                    SelectedUnits = selected
                    SelectedUnit = selected |> Set.toList |> List.tryHead
                    SelectedRegion =
                        state.SelectedRegion
                        |> Option.filter (fun id -> Map.containsKey id entry.After.Document.Regions)
                    Gesture = IdleGesture
                    UndoHistory = undo
                    RedoHistory = remaining
                    HistoryBytes = historySize undo
                    Validation = None
                    Issues = validationIssues entry.After.Document
                    ActiveIssue =
                        if Array.isEmpty (validationIssues entry.After.Document) then None else Some 0
                    Authoring =
                        { state.Authoring with
                            RevisionIdentity = entry.After.Digest
                            ThumbnailSvg = None } }
        | MarkEditorSaved ->
            { state with
                RevisionState = SavedRevision
                SavedDigest = Some state.Revision.Digest
                Validation = None }
        | MarkEditorSimulated ->
            { state with
                Map = state.Revision.Document
                RevisionState = SimulatedRevision
                SimulatedDigest = Some state.Revision.Digest
                Validation = None }
        | MarkEditorRecovered sourceDigest ->
            { state with
                RevisionState = RecoveredRevision
                RecoveredFromDigest = Some sourceDigest
                Validation = None }
        | RestoreEditorDraft ->
            { state with
                Map = state.Revision.Document
                RevisionState =
                    if state.SavedDigest = Some state.Revision.Digest then SavedRevision
                    else DirtyRevision
                IsRunning = false
                Tick = 0
                LastEvents = []
                Validation = None }
        | RemoveSelectedUnit ->
            update DeleteEditorSelection state
        | MoveSelected direction ->
            let original = selectedUnits state
            if Array.isEmpty original then
                state
            else
                let columnDelta, rowDelta = directionDelta direction
                let command =
                    original
                    |> translatedSelection (int32 columnDelta) (int32 rowDelta)
                    |> UpdateUnits
                if movementCrossesEdge state.Map (int32 columnDelta) (int32 rowDelta) original then
                    { state with
                        Validation = Some "That move is blocked."
                        UnitAnnouncement = "Keyboard movement rejected by a blocking edge." }
                else
                    let next = commit command state
                    { next with
                        UnitAnnouncement =
                            if next.Validation.IsSome then "Keyboard movement rejected."
                            else
                                "Moved "
                                + string original.Length
                                + (if original.Length = 1 then " unit" else " units")
                                + " one cell "
                                + directionCode direction
                                + "." }
        | StepEditor ->
            let runtime = legacyUpdate action state
            { runtime with
                Revision = state.Revision
                RevisionState = state.RevisionState
                UndoHistory = state.UndoHistory
                RedoHistory = state.RedoHistory
                HistoryBytes = state.HistoryBytes }
        | _ ->
            let legacy = legacyUpdate action state
            let normalizedSelection =
                match action with
                | ActivateCell _ when state.Tool = Select ->
                    legacy.SelectedUnit |> Option.map Set.singleton |> Option.defaultValue Set.empty
                | LoadMapText _
                | ClearMap ->
                    legacy.SelectedUnit |> Option.map Set.singleton |> Option.defaultValue Set.empty
                | _ -> selectedAfterMap legacy.Map state.SelectedUnits
            let legacy =
                { legacy with
                    SelectedUnits = normalizedSelection
                    SelectedUnit =
                        legacy.SelectedUnit
                        |> Option.filter (fun id -> Set.contains id normalizedSelection)
                        |> Option.orElseWith (fun () -> normalizedSelection |> Set.toList |> List.tryHead)
                    UndoHistory = state.UndoHistory
                    RedoHistory = state.RedoHistory
                    HistoryBytes = state.HistoryBytes
                    Clipboard = state.Clipboard
                    Revision = state.Revision
                    RevisionState = state.RevisionState
                    SavedDigest = state.SavedDigest
                    SimulatedDigest = state.SimulatedDigest
                    RecoveredFromDigest = state.RecoveredFromDigest }
            if legacy.Map = state.Map then legacy
            elif state.RevisionState = SimulatedRevision then
                legacy
            else commit (legacyCommand action legacy.Map) { legacy with Map = state.Map }

    let private xmlEscape (value: string) =
        value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;")

    /// Generates a deterministic, presentation-only thumbnail. The SVG and
    /// other authoring metadata are never part of SIR-MAP or its revision
    /// digest.
    let thumbnailSvg state =
        let cell = 12
        let width = int state.Map.Width * cell
        let height = int state.Map.Height * cell
        let terrain =
            state.Map.Terrain
            |> Map.toList
            |> List.map (fun ((column, row), value) ->
                let fill =
                    match value with
                    | Rough -> "#8b7d62"
                    | Blocked -> "#302e35"
                    | Objective -> "#b48835"
                    | Open -> "#d8d0bc"
                "<rect x=\"" + string (int column * cell) + "\" y=\"" + string (int row * cell)
                + "\" width=\"" + string cell + "\" height=\"" + string cell + "\" fill=\"" + fill + "\"/>")
        let units =
            state.Map.Units
            |> Map.toList
            |> List.map (fun (_, unit) ->
                let fill =
                    match unit.Side with
                    | Blue -> "#286b9f"
                    | Red -> "#a33d3d"
                    | NeutralSide -> "#77736a"
                "<rect x=\"" + string (int unit.Column * cell + 2) + "\" y=\""
                + string (int unit.Row * cell + 2) + "\" width=\""
                + string (int unit.Size * cell - 4) + "\" height=\""
                + string (int unit.Size * cell - 4) + "\" rx=\"2\" fill=\"" + fill + "\"/>")
        String.concat
            ""
            ([ "<svg xmlns=\"http://www.w3.org/2000/svg\" role=\"img\" aria-label=\""
               xmlEscape state.Authoring.Name
               + " map thumbnail\" viewBox=\"0 0 " + string width + " " + string height + "\">"
               "<rect width=\"100%\" height=\"100%\" fill=\"#d8d0bc\"/>" ]
             @ terrain
             @ units
             @ [ "</svg>" ])

    let authoringMetadataText state =
        let safe (value: string) = value.Replace("\r", " ").Replace("\n", " ")
        String.concat
            "\n"
            ([ "SIR-MAP-AUTHORING 1"
               "name " + safe state.Authoring.Name
               "revision " + state.Revision.Digest ]
             @ (state.Authoring.SavedViews
                |> Map.toList
                |> List.map (fun (_, view) ->
                    "view " + safe view.Name + " "
                    + string view.Camera.PanX + " "
                    + string view.Camera.PanY + " "
                    + string view.Camera.Zoom)))
        + "\n"

    let autosaveText state = export state

    let private edgeVisual ((column, row, direction), (kind, isOpen)) : EdgeVisual =
        let kindName =
            match kind with
            | Wall -> "wall"
            | Door -> "door"
            | Window -> "window"
        let state = if isOpen then "open" else "solid"
        let startColumn, startRow, endColumn, endRow =
            match direction with
            | EastEdge -> column + 1, row, column + 1, row + 1
            | SouthEdge -> column, row + 1, column + 1, row + 1

        { Id =
            "editor-edge-"
            + string column
            + "-"
            + string row
            + "-"
            + string direction
          Kind = kindName
          State = state
          StartColumn = startColumn
          StartRow = startRow
          EndColumn = endColumn
          EndRow = endRow }

    let frame state : RenderFrame =
        let units =
            state.Map.Units
            |> Map.toList
            |> List.map snd
            |> List.map (fun unit ->
                { Id = unit.Id
                  AnchorColumn = unit.Column
                  AnchorRow = unit.Row
                  FootprintWidth = extent unit.Size
                  FootprintDepth = extent unit.Size
                  ClassId = UnitClassId.resolve unit.ClassId
                  Faction =
                    match unit.Side with
                    | Blue -> Human
                    | Red -> Arcane
                    | NeutralSide -> Neutral
                  Health = Disclosed(health unit.Health unit.HealthMaximum)
                  Level = Disclosed 0
                  StanceId = NotPresent
                  BodyHeading = Disclosed heading
                  SecondaryHeading = NotPresent
                  ShortLabel = Disclosed(string unit.Id)
                  StatusIds = [| controllerName unit.Controller |] })
            |> List.toArray

        { Tick = state.Tick
          Board =
            { MinimumColumn = 0
              MinimumRow = 0
              MaximumColumn = state.Map.Width - 1
              MaximumRow = state.Map.Height - 1 }
          Units = units
          Edges = state.Map.Edges |> Map.toList |> List.map edgeVisual |> List.toArray
          Overlays = [||]
          Events =
            state.LastEvents
            |> List.mapi (fun index summary ->
                { Id = state.Tick * 1000 + int32 index
                  Tick = state.Tick
                  Kind = "editor"
                  SourceUnitId = NotPresent
                  TargetUnitId = NotPresent
                  Summary = Disclosed summary })
            |> List.toArray
          Disclosure = SandboxDisclosure }

    let terrainAt column row state =
        state.Map.Terrain
        |> Map.tryFind (column, row)
        |> Option.defaultValue Open

    let unitAt column row state =
        unitAtCell column row state.Map

    let selected state = selectedUnit state

    let terrainLabel terrain =
        terrainName terrain

    let terrainToolLabel tool =
        match tool with
        | PencilTool -> "Pencil"
        | RectangleTool -> "Rectangle"
        | LineTool -> "Line"
        | FloodFillTool -> "Flood fill"
        | EyedropperTool -> "Eyedropper"
        | EraseTool -> "Erase"

    let terrainToolShortcut tool =
        match tool with
        | PencilTool -> "P"
        | RectangleTool -> "R"
        | LineTool -> "L"
        | FloodFillTool -> "G"
        | EyedropperTool -> "I"
        | EraseTool -> "X"

    let terrainPattern terrain =
        match terrain with
        | Open -> "plain"
        | Rough -> "diagonal hatch"
        | Blocked -> "cross hatch"
        | Objective -> "inset ring"

    let terrainPreview state =
        match state.Gesture with
        | TerrainGesture(tool, anchor, current, visited) ->
            let command =
                terrainGestureCommand state tool anchor current visited
            match command with
            | PaintCells(terrain, addresses) ->
                let isValid = validateCommand state.Map command |> Result.isOk
                Some(terrain, addresses, isValid)
            | _ -> None
        | _ -> None

    let unitPreview state =
        let command =
            match state.Gesture with
            | CommandPreviewGesture(AddUnits units) -> Some(AddUnits units, units)
            | UnitMoveGesture(_, _, _, UpdateUnits units) -> Some(UpdateUnits units, units)
            | _ -> None

        command
        |> Option.map (fun (editorCommand, units) ->
            units,
            (match validateCommand state.Map editorCommand with
             | Ok _ ->
                 match state.Gesture with
                 | UnitMoveGesture(anchor, current, original, _) ->
                     not (
                         movementCrossesEdge
                             state.Map
                             (current.CellColumn - anchor.CellColumn)
                             (current.CellRow - anchor.CellRow)
                             original
                     )
                 | _ -> true
             | Error _ -> false))

    let controllerLabel controller =
        match controller with
        | Manual -> "Manual"
        | Scripted -> "Scripted AI"
        | General -> "General AI"
