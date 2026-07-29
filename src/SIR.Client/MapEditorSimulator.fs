namespace SIR.Client

open System

type SimulatorCollision =
    | RouteClear
    | OutsideMap of EditorCellAddress
    | BlockedTerrainAt of EditorCellAddress
    | BlockingEdgeAt of EditorCellAddress * EditorCellAddress
    | OccupiedAt of EditorCellAddress * unitId: int32

type SimulatorRoutePreview =
    { UnitId: int32
      Origin: EditorCellAddress
      Destination: EditorCellAddress
      Distance: int32
      Route: EditorCellAddress array
      Collision: SimulatorCollision }

type PerspectivePreviewAvailability =
    | PerspectivePreviewUnavailable of reason: string
    | AcceptedPerspectiveProjection of RenderFrame

type VisibilityOverlayAvailability =
    | VisibilityOverlaysUnavailable of reason: string
    | SharedKernelVisibilityAvailable

type SimulatorHandoff =
    { Revision: MapRevision
      RuntimeMap: MapDefinition
      Tick: int32
      IsRunning: bool
      LastEvents: string list
      PreviewDestination: EditorCellAddress option }

type SimulatorAction =
    | ToggleSimulatorRun
    | StepSimulator
    | MoveSimulatorUnit of MapDirection
    | SetSimulatorController of MapController
    | SetSimulatorScript of string
    | MoveSimulatorPreview of columnDelta: int32 * rowDelta: int32
    | ResetSimulatorPreview
    | CommitSimulatorPreview

[<RequireQualifiedAccess>]
module MapEditorSimulator =
    [<Literal>]
    let PerspectiveUnavailableReason =
        "Player perspective is unavailable: no accepted disclosure-filtered projection exists for editor drafts."

    [<Literal>]
    let VisibilityUnavailableReason =
        "Visibility overlays are unavailable until shared-kernel perception rules are accepted."

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

    let private directionForDelta columnDelta rowDelta =
        match sign columnDelta, sign rowDelta with
        | 0, -1 -> Some North
        | 1, -1 -> Some NorthEast
        | 1, 0 -> Some East
        | 1, 1 -> Some SouthEast
        | 0, 1 -> Some South
        | -1, 1 -> Some SouthWest
        | -1, 0 -> Some West
        | -1, -1 -> Some NorthWest
        | _ -> None

    let private edgeBlocks map key =
        map.Edges
        |> Map.tryFind key
        |> Option.exists (fun (kind, isOpen) ->
            match kind with
            | Door -> not isOpen
            | Wall
            | Window -> true)

    let private footprint unit column row =
        [| for y in row .. row + unit.Size - 1 do
               for x in column .. column + unit.Size - 1 do
                   yield x, y |]

    let private crossedEdges unit columnDelta rowDelta =
        let horizontal =
            if columnDelta > 0 then
                [| for row in unit.Row .. unit.Row + unit.Size - 1 ->
                       unit.Column + unit.Size - 1, row, EastEdge |]
            elif columnDelta < 0 then
                [| for row in unit.Row .. unit.Row + unit.Size - 1 ->
                       unit.Column - 1, row, EastEdge |]
            else
                [||]
        let vertical =
            if rowDelta > 0 then
                [| for column in unit.Column .. unit.Column + unit.Size - 1 ->
                       column, unit.Row + unit.Size - 1, SouthEdge |]
            elif rowDelta < 0 then
                [| for column in unit.Column .. unit.Column + unit.Size - 1 ->
                       column, unit.Row - 1, SouthEdge |]
            else
                [||]
        Array.append horizontal vertical

    let private collisionForStep map unit destination =
        let columnDelta = destination.CellColumn - unit.Column
        let rowDelta = destination.CellRow - unit.Row
        let cells = footprint unit destination.CellColumn destination.CellRow
        match
            cells
            |> Array.tryFind (fun (column, row) ->
                column < 0
                || row < 0
                || column >= map.Width
                || row >= map.Height)
        with
        | Some(column, row) ->
            OutsideMap
                { CellColumn = column
                  CellRow = row }
        | None ->
            match
                cells
                |> Array.tryFind (fun address ->
                    Map.tryFind address map.Terrain = Some Blocked)
            with
            | Some(column, row) ->
                BlockedTerrainAt
                    { CellColumn = column
                      CellRow = row }
            | None ->
                match
                    crossedEdges unit columnDelta rowDelta
                    |> Array.tryFind (edgeBlocks map)
                with
                | Some(column, row, direction) ->
                    let origin, destination =
                        match direction with
                        | EastEdge ->
                            { CellColumn = column; CellRow = row },
                            { CellColumn = column + 1; CellRow = row }
                        | SouthEdge ->
                            { CellColumn = column; CellRow = row },
                            { CellColumn = column; CellRow = row + 1 }
                    BlockingEdgeAt(origin, destination)
                | None ->
                    match
                        map.Units
                        |> Map.toSeq
                        |> Seq.filter (fun (id, _) -> id <> unit.Id)
                        |> Seq.sortBy fst
                        |> Seq.tryPick (fun (id, other) ->
                            let occupied =
                                footprint other other.Column other.Row
                                |> Set.ofArray
                            cells
                            |> Array.tryFind (fun cell -> Set.contains cell occupied)
                            |> Option.map (fun (column, row) ->
                                OccupiedAt(
                                    { CellColumn = column
                                      CellRow = row },
                                    id
                                )))
                    with
                    | Some collision -> collision
                    | None -> RouteClear

    let private route origin destination =
        let mutable column = origin.CellColumn
        let mutable row = origin.CellRow
        [| while column <> destination.CellColumn || row <> destination.CellRow do
               column <- column + sign (destination.CellColumn - column)
               row <- row + sign (destination.CellRow - row)
               yield
                   { CellColumn = column
                     CellRow = row } |]

    let preview selectedUnitId destination handoff =
        selectedUnitId
        |> Option.bind (fun id ->
            handoff.RuntimeMap.Units
            |> Map.tryFind id
            |> Option.map (fun unit ->
                let origin =
                    { CellColumn = unit.Column
                      CellRow = unit.Row }
                let path = route origin destination
                let mutable probe = unit
                let mutable collision = RouteClear
                let mutable accepted = ResizeArray<EditorCellAddress>()
                for step in path do
                    if collision = RouteClear then
                        let nextCollision =
                            collisionForStep handoff.RuntimeMap probe step
                        if nextCollision = RouteClear then
                            accepted.Add step
                            probe <-
                                { probe with
                                    Column = step.CellColumn
                                    Row = step.CellRow }
                        else
                            collision <- nextCollision
                { UnitId = id
                  Origin = origin
                  Destination = destination
                  Distance =
                    max
                        (abs (destination.CellColumn - origin.CellColumn))
                        (abs (destination.CellRow - origin.CellRow))
                  Route = accepted.ToArray()
                  Collision = collision }))

    let tryHandoff (state: MapEditorState) =
        let issues =
            MapEditor.validationIssues state.Revision.Document
            |> Array.filter (fun issue -> issue.Code <> "EDGE-GAP")
        if Array.isEmpty issues then
            Ok
                { Revision = state.Revision
                  RuntimeMap = state.Revision.Document
                  Tick = 0
                  IsRunning = false
                  LastEvents = []
                  PreviewDestination = None }
        else
            issues
            |> Array.map (fun issue -> issue.Code + ": " + issue.Message)
            |> String.concat " "
            |> Error

    let isBehindDraft (state: MapEditorState) (handoff: SimulatorHandoff) =
        handoff.Revision.Digest <> state.Revision.Digest

    let perspectivePreview (projection: RenderFrame option) =
        match projection with
        | Some frame when frame.Disclosure = PerspectiveDisclosure ->
            AcceptedPerspectiveProjection frame
        | _ -> PerspectivePreviewUnavailable PerspectiveUnavailableReason

    let visibilityOverlays = VisibilityOverlaysUnavailable VisibilityUnavailableReason

    let private moveUnit direction (unit: EditorUnit) (map: MapDefinition) =
        let columnDelta, rowDelta = directionDelta direction
        let destination =
            { CellColumn = unit.Column + int32 columnDelta
              CellRow = unit.Row + int32 rowDelta }
        if collisionForStep map unit destination = RouteClear then
            { map with
                Units =
                    Map.add
                        unit.Id
                        { unit with
                            Column = destination.CellColumn
                            Row = destination.CellRow }
                        map.Units },
            true
        else
            map, false

    let private nearestHostile (unit: EditorUnit) (map: MapDefinition) =
        map.Units
        |> Map.toSeq
        |> Seq.map snd
        |> Seq.filter (fun (other: EditorUnit) ->
            other.Id <> unit.Id && other.Side <> unit.Side)
        |> Seq.sortBy (fun (other: EditorUnit) ->
            max
                (abs (other.Column - unit.Column))
                (abs (other.Row - unit.Row)),
            other.Id)
        |> Seq.tryHead

    let private step handoff =
        let mutable map = handoff.RuntimeMap
        let events = ResizeArray<string>()
        for id in handoff.RuntimeMap.Units |> Map.toSeq |> Seq.map fst |> Seq.sort do
            match Map.tryFind id map.Units with
            | Some unit when unit.Health > 0 ->
                match unit.Controller with
                | Manual ->
                    events.Add("Unit " + string id + " awaits manual input.")
                | Scripted when List.isEmpty unit.Script ->
                    events.Add("Unit " + string id + " has no script.")
                | Scripted ->
                    let direction = unit.Script[unit.ScriptIndex % unit.Script.Length]
                    let nextMap, changed = moveUnit direction unit map
                    let advanced =
                        Map.find id nextMap.Units
                        |> fun moved ->
                            { moved with ScriptIndex = unit.ScriptIndex + 1 }
                    map <- { nextMap with Units = Map.add id advanced nextMap.Units }
                    events.Add(
                        "Unit "
                        + string id
                        + (if changed then " follows " else " is blocked moving ")
                        + directionCode direction
                        + "."
                    )
                | General ->
                    match nearestHostile unit map with
                    | None -> events.Add("Unit " + string id + " holds; no hostile is present.")
                    | Some target ->
                        let columnDelta = target.Column - unit.Column
                        let rowDelta = target.Row - unit.Row
                        if
                            max (abs columnDelta) (abs rowDelta)
                            <= max unit.Size target.Size
                        then
                            let damaged =
                                { target with Health = max 0 (target.Health - 1) }
                            map <- { map with Units = Map.add target.Id damaged map.Units }
                            events.Add(
                                "Unit "
                                + string id
                                + " attacks unit "
                                + string target.Id
                                + " for 1 damage."
                            )
                        else
                            match directionForDelta columnDelta rowDelta with
                            | None -> events.Add("Unit " + string id + " holds.")
                            | Some direction ->
                                let nextMap, changed = moveUnit direction unit map
                                map <- nextMap
                                events.Add(
                                    "Unit "
                                    + string id
                                    + (if changed then " advances " else " cannot advance ")
                                    + directionCode direction
                                    + "."
                                )
            | _ -> ()
        { handoff with
            RuntimeMap = map
            Tick = handoff.Tick + 1
            LastEvents = List.ofSeq events
            PreviewDestination = None }

    let update action selectedUnitId handoff =
        let updateSelected transform =
            selectedUnitId
            |> Option.bind (fun id -> Map.tryFind id handoff.RuntimeMap.Units)
            |> Option.map (fun unit ->
                { handoff with
                    RuntimeMap =
                        { handoff.RuntimeMap with
                            Units =
                                Map.add unit.Id (transform unit) handoff.RuntimeMap.Units }
                    PreviewDestination = None })
            |> Option.defaultValue handoff

        match action with
        | ToggleSimulatorRun -> { handoff with IsRunning = not handoff.IsRunning }
        | StepSimulator -> step handoff
        | MoveSimulatorUnit direction ->
            selectedUnitId
            |> Option.bind (fun id -> Map.tryFind id handoff.RuntimeMap.Units)
            |> Option.map (fun unit ->
                let map, changed = moveUnit direction unit handoff.RuntimeMap
                { handoff with
                    RuntimeMap = map
                    LastEvents =
                        [ "Unit "
                          + string unit.Id
                          + (if changed then " moves " else " is blocked moving ")
                          + directionCode direction
                          + "." ]
                    PreviewDestination = None })
            |> Option.defaultValue handoff
        | SetSimulatorController controller ->
            updateSelected (fun unit -> { unit with Controller = controller })
        | SetSimulatorScript text ->
            match MapEditor.parseScript text with
            | Ok script ->
                updateSelected (fun unit ->
                    { unit with Script = script; ScriptIndex = 0 })
            | Error error -> { handoff with LastEvents = [ error ] }
        | MoveSimulatorPreview(columnDelta, rowDelta) ->
            let origin =
                handoff.PreviewDestination
                |> Option.orElseWith (fun () ->
                    selectedUnitId
                    |> Option.bind (fun id -> Map.tryFind id handoff.RuntimeMap.Units)
                    |> Option.map (fun unit ->
                        { CellColumn = unit.Column
                          CellRow = unit.Row }))
            { handoff with
                PreviewDestination =
                    origin
                    |> Option.map (fun destination ->
                        { CellColumn = destination.CellColumn + columnDelta
                          CellRow = destination.CellRow + rowDelta }) }
        | ResetSimulatorPreview -> { handoff with PreviewDestination = None }
        | CommitSimulatorPreview ->
            match handoff.PreviewDestination with
            | None -> handoff
            | Some destination ->
                match preview selectedUnitId destination handoff with
                | Some routePreview
                    when routePreview.Collision = RouteClear
                         && routePreview.Route.Length > 0 ->
                    let unit = Map.find routePreview.UnitId handoff.RuntimeMap.Units
                    let moved =
                        { unit with
                            Column = destination.CellColumn
                            Row = destination.CellRow }
                    { handoff with
                        RuntimeMap =
                            { handoff.RuntimeMap with
                                Units =
                                    Map.add moved.Id moved handoff.RuntimeMap.Units }
                        LastEvents =
                            [ "Unit "
                              + string moved.Id
                              + " moves "
                              + string routePreview.Distance
                              + " cells along the accepted preview route." ]
                        PreviewDestination = None }
                | Some routePreview ->
                    { handoff with
                        LastEvents =
                            [ "Preview route rejected: " + string routePreview.Collision + "." ] }
                | None -> handoff

    let frame selectedUnitId handoff =
        let state =
            { MapEditor.initial with
                Map = handoff.RuntimeMap
                Revision = handoff.Revision
                RevisionState = SimulatedRevision
                SimulatedDigest = Some handoff.Revision.Digest
                SelectedUnit = selectedUnitId
                SelectedUnits = selectedUnitId |> Option.map Set.singleton |> Option.defaultValue Set.empty
                Tick = handoff.Tick
                IsRunning = handoff.IsRunning
                LastEvents = handoff.LastEvents }
        let baseFrame = MapEditor.frame state
        let overlay =
            handoff.PreviewDestination
            |> Option.bind (fun destination -> preview selectedUnitId destination handoff)
            |> Option.map (fun routePreview ->
                { Id = "simulator-route-preview"
                  Kind =
                    if routePreview.Collision = RouteClear then
                        "route-preview-clear"
                    else
                        "route-preview-collision"
                  Scope = SelectedUnitOverlay routePreview.UnitId
                  GeometryRevision = int32 (handoff.Revision.Number % int64 Int32.MaxValue)
                  Points =
                    Array.append
                        [| float routePreview.Origin.CellColumn + 0.5
                           float routePreview.Origin.CellRow + 0.5 |]
                        (routePreview.Route
                         |> Array.collect (fun address ->
                             [| float address.CellColumn + 0.5
                                float address.CellRow + 0.5 |]))
                  Label =
                    Disclosed(
                        "Route "
                        + string routePreview.Distance
                        + " cells; "
                        + (if routePreview.Collision = RouteClear then
                               "clear"
                           else
                               "collision: " + string routePreview.Collision)
                    ) })
        { baseFrame with
            Overlays = overlay |> Option.toArray }

    let viewState selectedUnitId handoff =
        { MapEditor.initial with
            Map = handoff.RuntimeMap
            Revision = handoff.Revision
            RevisionState = SimulatedRevision
            SimulatedDigest = Some handoff.Revision.Digest
            SelectedUnit = selectedUnitId
            SelectedUnits = selectedUnitId |> Option.map Set.singleton |> Option.defaultValue Set.empty
            Tick = handoff.Tick
            IsRunning = handoff.IsRunning
            LastEvents = handoff.LastEvents }
