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
      NextUnitId: int32 }

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
    | DocumentDomain

type EditorCellAddress =
    { CellColumn: int32
      CellRow: int32 }

type EditorBox =
    { FirstColumn: int32
      FirstRow: int32
      LastColumn: int32
      LastRow: int32 }

type EditorGesture =
    | IdleGesture
    | BoxSelectionGesture of anchor: EditorCellAddress * current: EditorCellAddress
    | CommandPreviewGesture of EditorCommand
    | TerrainGesture of
        tool: TerrainAuthoringTool *
        anchor: EditorCellAddress *
        current: EditorCellAddress *
        visited: EditorCellAddress array

and EditorCommand =
    | PaintCells of MapTerrain * EditorCellAddress array
    | ReplaceEdges of ((int32 * int32 * MapEdgeDirection) * (MapEdgeKind * bool) option) array
    | AddUnits of EditorUnit array
    | UpdateUnits of EditorUnit array
    | RemoveUnits of int32 array
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
      ClassId: string
      FootprintSize: int32 }

type MapEditorState =
    { Map: MapDefinition
      Tool: MapEditorTool
      TerrainSelection: MapTerrain
      BrushSize: int32
      TerrainCursor: EditorCellAddress
      TerrainAnnouncement: string
      SelectedUnit: int32 option
      SelectedUnits: Set<int32>
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
      Validation: string option }

type MapEditorAction =
    | ChooseTool of MapEditorTool
    | ChooseTerrain of MapTerrain
    | SetTerrainBrushSize of int32
    | MoveTerrainCursor of columnDelta: int32 * rowDelta: int32 * extendPreview: bool
    | ActivateTerrainCursor
    | BeginTerrainGesture of EditorCellAddress
    | ExtendTerrainGesture of EditorCellAddress
    | ActivateCell of column: int32 * row: int32
    | Resize of width: int32 * height: int32
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
    let FormatVersion = 1

    [<Literal>]
    let MaximumHistoryCommands = 100

    [<Literal>]
    let MaximumHistoryBytes = 2_000_000

    let canonicalFootprintPresets =
        [ { Id = "goblin"
            ClassId = "goblin"
            FootprintSize = 1 }
          { Id = "orc"
            ClassId = "orc"
            FootprintSize = 2 }
          { Id = "troll"
            ClassId = "troll"
            FootprintSize = 3 }
          { Id = "human"
            ClassId = "rifleman"
            FootprintSize = 2 }
          { Id = "drone"
            ClassId = "observation-drone"
            FootprintSize = 1 } ]

    let tryCanonicalFootprintPreset id =
        canonicalFootprintPresets
        |> List.tryFind (fun preset ->
            String.Equals(preset.Id, id, StringComparison.Ordinal))

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
          NextUnitId = 1 }

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
          SelectedUnit = Some 1
          SelectedUnits = Set.singleton 1
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
          Validation = None }

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

    let private resize width height state =
        let width = max 4 (min 40 width)
        let height = max 4 (min 40 height)
        let terrain =
            state.Map.Terrain
            |> Map.filter (fun (column, row) _ -> column < width && row < height)
        let edges =
            state.Map.Edges
            |> Map.filter (fun (column, row, _) _ -> column < width && row < height)
        let units =
            state.Map.Units
            |> Map.filter (fun _ unit ->
                unit.Column + unit.Size <= width
                && unit.Row + unit.Size <= height)

        { state with
            Map =
                { state.Map with
                    Width = width
                    Height = height
                    Terrain = terrain
                    Edges = edges
                    Units = units }
            SelectedUnit =
                state.SelectedUnit
                |> Option.filter (fun id -> Map.containsKey id units)
            Validation = None }

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

    and export state = canonicalMapText state.Map

    and tryImport text =
        let fail line message =
            Error("Map line " + string line + ": " + message)

        let parseInt line (value: string) =
            match Int32.TryParse value with
            | true, parsed -> Ok parsed
            | _ -> fail line ("invalid integer '" + value + "'.")

        let lines =
            text.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries)

        if lines.Length < 2 || lines[0] <> "SIR-MAP " + string FormatVersion then
            Error "The file is not a supported SIR-MAP 1 document."
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
                            result <-
                                Ok
                                    { map with
                                        Edges =
                                            Map.add
                                                (column, row, direction)
                                                (kind, state = "open")
                                                map.Edges }
                        | Error error, _, _, _
                        | _, Error error, _, _ -> result <- Error error
                        | _ -> result <- fail line "invalid edge direction, kind, or state."
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
                let invalid =
                    map.Units
                    |> Map.toList
                    |> List.map snd
                    |> List.tryFind (fun unit ->
                        not (validPlacement map (Some unit.Id) unit unit.Column unit.Row))
                match invalidTerrain, invalidEdge, invalid with
                | Some((column, row), _), _, _ ->
                    Error("Terrain cell " + string column + "," + string row + " is outside the map.")
                | _, Some((column, row, _), _), _ ->
                    Error("Edge " + string column + "," + string row + " is outside the map.")
                | _, _, Some unit -> Error("Unit " + string unit.Id + " does not fit the map.")
                | None, None, None -> Ok map)

    let private issue code message =
        { Code = code; Message = message }

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

        let units =
            map.Units
            |> Map.toList
            |> List.choose (fun (id, unit) ->
                if id <> unit.Id || id <= 0 then
                    Some(issue "UNIT-IDENTITY" "Unit keys and positive identifiers must agree.")
                elif String.IsNullOrWhiteSpace unit.ClassId || unit.ClassId |> Seq.exists Char.IsWhiteSpace then
                    Some(issue "UNIT-CLASS" "A unit class ID must be one non-empty token.")
                elif unit.HealthMaximum <= 0 || unit.Health < 0 || unit.Health > unit.HealthMaximum then
                    Some(issue "UNIT-HEALTH" "Unit health is outside its accepted range.")
                elif not (validPlacement map (Some id) unit unit.Column unit.Row) then
                    Some(issue "UNIT-PLACEMENT" ("Unit " + string id + " does not fit."))
                else
                    None)

        dimensions @ terrain @ edges @ units

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
        | ResizeDocument(width, height) ->
            { map with Width = width; Height = height }
        | ReplaceDocument(_, document) -> document

    let validateCommand map command =
        let duplicate values =
            values |> Array.distinct |> Array.length <> Array.length values

        let shapeIssues =
            match command with
            | PaintCells(_, addresses) when Array.isEmpty addresses ->
                [ issue "COMMAND-EMPTY" "A paint command must contain at least one cell." ]
            | ReplaceEdges changes when Array.isEmpty changes ->
                [ issue "COMMAND-EMPTY" "An edge command must contain at least one change." ]
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
                    Gesture = IdleGesture
                    Revision = after
                    RevisionState = DirtyRevision
                    UndoHistory = undo
                    RedoHistory = []
                    HistoryBytes = historySize undo
                    Validation = None }

    let private activeDomain tool =
        match tool with
        | Paint _ -> TerrainDomain
        | Terrain _ -> TerrainDomain
        | Edge _ -> EdgeDomain
        | Place _ -> UnitDomain
        | Select -> UnitDomain

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
        match action with
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
            | IdleGesture -> state
        | CancelEditorGesture ->
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
                    Gesture = IdleGesture
                    UndoHistory = remaining
                    RedoHistory = entry :: state.RedoHistory |> historyWithinBounds
                    HistoryBytes = historySize remaining
                    Validation = None }
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
                    Gesture = IdleGesture
                    UndoHistory = undo
                    RedoHistory = remaining
                    HistoryBytes = historySize undo
                    Validation = None }
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
        | StepEditor
        | MoveSelected _ ->
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

    let controllerLabel controller =
        match controller with
        | Manual -> "Manual"
        | Scripted -> "Scripted AI"
        | General -> "General AI"
