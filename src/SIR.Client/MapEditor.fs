namespace SIR.Client

open System

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
    | Place of MapSide * classId: string * size: int32
    | Edge of MapEdgeDirection * MapEdgeKind

type MapUnitFootprintPreset =
    { Id: string
      ClassId: string
      FootprintSize: int32 }

type MapEditorState =
    { Map: MapDefinition
      Tool: MapEditorTool
      SelectedUnit: int32 option
      Tick: int32
      IsRunning: bool
      LastEvents: string list
      Validation: string option }

type MapEditorAction =
    | ChooseTool of MapEditorTool
    | ActivateCell of column: int32 * row: int32
    | Resize of width: int32 * height: int32
    | SelectEditorUnit of int32 option
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

        { Map = map
          Tool = Select
          SelectedUnit = Some 1
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

    let rec update action state =
        match action with
        | ChooseTool tool ->
            { state with Tool = tool; Validation = None }
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

    and export state =
        let lines =
            [ "SIR-MAP " + string FormatVersion
              "size " + string state.Map.Width + " " + string state.Map.Height
              yield!
                  state.Map.Terrain
                  |> Map.toList
                  |> List.map (fun ((column, row), terrain) ->
                      "terrain "
                      + string column
                      + " "
                      + string row
                      + " "
                      + terrainName terrain)
              yield!
                  state.Map.Edges
                  |> Map.toList
                  |> List.map (fun ((column, row, direction), (kind, isOpen)) ->
                      let directionName =
                          match direction with
                          | EastEdge -> "east"
                          | SouthEdge -> "south"
                      let kindName =
                          match kind with
                          | Wall -> "wall"
                          | Door -> "door"
                          | Window -> "window"
                      "edge "
                      + string column
                      + " "
                      + string row
                      + " "
                      + directionName
                      + " "
                      + kindName
                      + " "
                      + (if isOpen then "open" else "closed"))
              yield!
                  state.Map.Units
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

    let controllerLabel controller =
        match controller with
        | Manual -> "Manual"
        | Scripted -> "Scripted AI"
        | General -> "General AI"
