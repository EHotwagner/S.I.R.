namespace SIR.Client

open System

type SimulatorCollision =
    | RouteClear
    | OutsideMap of EditorCellAddress
    | BlockedTerrainAt of EditorCellAddress
    | BlockingEdgeAt of EditorCellAddress * EditorCellAddress
    | OccupiedAt of EditorCellAddress * unitId: int32
    | NoPathTo of EditorCellAddress

type SimulatorRoutePreview =
    { UnitId: int32
      Origin: EditorCellAddress
      Destination: EditorCellAddress
      Distance: int32
      DistanceMillimeters: int32
      MovementCostMillimeters: int32
      Route: EditorCellAddress array
      Collision: SimulatorCollision }

type PerspectivePreviewAvailability =
    | PerspectivePreviewUnavailable of reason: string
    | AcceptedPerspectiveProjection of RenderFrame

type VisibilityOverlayAvailability =
    | VisibilityOverlaysUnavailable of reason: string
    | SharedKernelVisibilityAvailable

type SimulatorCombatDelivery =
    | MeleeDelivery
    | ProjectileDelivery
    | LobbedAreaDelivery
    | SpellAreaDelivery

type SimulatorAreaShape =
    | BurstArea of radius: int32
    | ConeArea of range: int32 * angleDegrees: int32
    | RayArea of length: int32 * width: int32
    | RectangleArea of width: int32 * depth: int32

type SimulatorCombatTarget =
    | UnitCombatTarget of unitId: int32
    | AreaCombatTarget of origin: EditorCellAddress * shape: SimulatorAreaShape

type SimulatorAttackProfile =
    { Delivery: SimulatorCombatDelivery
      Range: int32
      Damage: int32
      RecoveryTicks: int32
      AreaShape: SimulatorAreaShape option }

type SimulatorCombatEvent =
    { Tick: int32
      SourceUnitId: int32
      Target: SimulatorCombatTarget
      Delivery: SimulatorCombatDelivery
      Damage: int32
      Summary: string }

type SimulatorMovementProfile =
    { SpeedMillimetersPerSecond: int32
      CellMillimeters: int32 }

type SimulatorMovementProgress =
    { Origin: EditorCellAddress
      Destination: EditorCellAddress
      ProgressMillimeters: int32
      CostMillimeters: int32 }

type SimulatorHandoff =
    { Revision: MapRevision
      RuntimeMap: MapDefinition
      Tick: int32
      IsRunning: bool
      LastEvents: string list
      LastCombatEvents: SimulatorCombatEvent list
      AttackRecoveryTicks: Map<int32, int32>
      MovementCreditsMillimeters: Map<int32, int32>
      MovementProgress: Map<int32, SimulatorMovementProgress>
      MovementIntents: Map<int32, MapDirection>
      PlannedRoutes: Map<int32, EditorCellAddress list>
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
    let TicksPerSecond = 20

    [<Literal>]
    let CellMillimeters = 500

    [<Literal>]
    let DiagonalCellMillimeters = 707

    [<Literal>]
    let MaximumMovementCreditMillimeters = 1060

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

    let private movementCollision map unit destination =
        let columnDelta = destination.CellColumn - unit.Column
        let rowDelta = destination.CellRow - unit.Row
        let direct = collisionForStep map unit destination
        if
            direct = RouteClear
            && columnDelta <> 0
            && rowDelta <> 0
        then
            let horizontal =
                { CellColumn = destination.CellColumn
                  CellRow = unit.Row }
            let vertical =
                { CellColumn = unit.Column
                  CellRow = destination.CellRow }
            match
                collisionForStep map unit horizontal,
                collisionForStep map unit vertical
            with
            | RouteClear, RouteClear -> RouteClear
            | collision, RouteClear
            | RouteClear, collision -> collision
            | first, _ -> first
        else
            direct

    let private directRoute origin destination =
        let mutable column = origin.CellColumn
        let mutable row = origin.CellRow
        [| while column <> destination.CellColumn || row <> destination.CellRow do
               column <- column + sign (destination.CellColumn - column)
               row <- row + sign (destination.CellRow - row)
               yield
                   { CellColumn = column
                     CellRow = row } |]

    let private stepDistance origin destination =
        if
            origin.CellColumn <> destination.CellColumn
            && origin.CellRow <> destination.CellRow
        then
            DiagonalCellMillimeters
        else
            CellMillimeters

    let private movementCost
        (map: MapDefinition)
        (unit: EditorUnit)
        origin
        destination
        =
        let distance = stepDistance origin destination
        let entersRoughGround =
            footprint unit destination.CellColumn destination.CellRow
            |> Array.exists (fun address ->
                Map.tryFind address map.Terrain = Some Rough)
        if entersRoughGround then
            distance * 3 / 2
        else
            distance

    let private pathNeighbors =
        [| 0, -1
           1, 0
           0, 1
           -1, 0
           1, -1
           1, 1
           -1, 1
           -1, -1 |]

    let private heuristic origin destination =
        let column = abs (destination.CellColumn - origin.CellColumn)
        let row = abs (destination.CellRow - origin.CellRow)
        let diagonal = min column row
        diagonal * DiagonalCellMillimeters
        + (max column row - diagonal) * CellMillimeters

    let private findPathTo
        (map: MapDefinition)
        (unit: EditorUnit)
        (goal: EditorUnit -> bool)
        (heuristicToGoal: EditorCellAddress -> int32)
        =
        let origin =
            { CellColumn = unit.Column
              CellRow = unit.Row }
        let mutable frontier =
            [ 0, 0, origin.CellRow, origin.CellColumn, origin ]
        let mutable costs = Map.ofList [ origin, 0 ]
        let mutable previous = Map.empty<EditorCellAddress, EditorCellAddress>
        let mutable result = None
        while result.IsNone && not (List.isEmpty frontier) do
            let _, cost, _, _, current = List.min frontier
            frontier <-
                frontier
                |> List.filter (fun (_, _, _, _, address) -> address <> current)
            let currentUnit =
                { unit with
                    Column = current.CellColumn
                    Row = current.CellRow }
            if goal currentUnit then
                let mutable cursor = current
                let mutable reversed = []
                while cursor <> origin do
                    reversed <- cursor :: reversed
                    cursor <- Map.find cursor previous
                result <- Some reversed
            else
                for columnDelta, rowDelta in pathNeighbors do
                    let destination =
                        { CellColumn = current.CellColumn + int32 columnDelta
                          CellRow = current.CellRow + int32 rowDelta }
                    let probe =
                        { currentUnit with
                            Column = current.CellColumn
                            Row = current.CellRow }
                    if movementCollision map probe destination = RouteClear then
                        let nextCost =
                            cost
                            + movementCost map currentUnit current destination
                        let known =
                            Map.tryFind destination costs
                            |> Option.defaultValue Int32.MaxValue
                        if nextCost < known then
                            costs <- Map.add destination nextCost costs
                            previous <- Map.add destination current previous
                            frontier <-
                                (nextCost + heuristicToGoal destination,
                                 nextCost,
                                 destination.CellRow,
                                 destination.CellColumn,
                                 destination)
                                :: frontier
        result

    let private findClosestPath
        (map: MapDefinition)
        (unit: EditorUnit)
        (score: EditorUnit -> int32)
        =
        let origin =
            { CellColumn = unit.Column
              CellRow = unit.Row }
        let mutable frontier =
            [ 0, origin.CellRow, origin.CellColumn, origin ]
        let mutable costs = Map.ofList [ origin, 0 ]
        let mutable previous = Map.empty<EditorCellAddress, EditorCellAddress>
        let mutable best =
            score unit, 0, origin.CellRow, origin.CellColumn, origin
        while not (List.isEmpty frontier) do
            let cost, _, _, current = List.min frontier
            frontier <-
                frontier
                |> List.filter (fun (_, _, _, address) -> address <> current)
            let currentUnit =
                { unit with
                    Column = current.CellColumn
                    Row = current.CellRow }
            let candidate =
                score currentUnit,
                cost,
                current.CellRow,
                current.CellColumn,
                current
            if candidate < best then
                best <- candidate
            for columnDelta, rowDelta in pathNeighbors do
                let destination =
                    { CellColumn = current.CellColumn + int32 columnDelta
                      CellRow = current.CellRow + int32 rowDelta }
                if movementCollision map currentUnit destination = RouteClear then
                    let nextCost =
                        cost
                        + movementCost map currentUnit current destination
                    let known =
                        Map.tryFind destination costs
                        |> Option.defaultValue Int32.MaxValue
                    if nextCost < known then
                        costs <- Map.add destination nextCost costs
                        previous <- Map.add destination current previous
                        frontier <-
                            (nextCost,
                             destination.CellRow,
                             destination.CellColumn,
                             destination)
                            :: frontier
        let _, _, _, _, destination = best
        let mutable cursor = destination
        let mutable path = []
        while cursor <> origin do
            path <- cursor :: path
            cursor <- Map.find cursor previous
        path

    let pathfind destination (unit: EditorUnit) (map: MapDefinition) =
        findPathTo
            map
            unit
            (fun candidate ->
                candidate.Column = destination.CellColumn
                && candidate.Row = destination.CellRow)
            (fun address -> heuristic address destination)

    let preview selectedUnitId destination handoff =
        selectedUnitId
        |> Option.bind (fun id ->
            handoff.RuntimeMap.Units
            |> Map.tryFind id
            |> Option.map (fun unit ->
                let origin =
                    { CellColumn = unit.Column
                      CellRow = unit.Row }
                let path =
                    pathfind destination unit handoff.RuntimeMap
                let collision =
                    match path with
                    | Some _ -> RouteClear
                    | None ->
                        let destinationCollision =
                            collisionForStep handoff.RuntimeMap unit destination
                        if destinationCollision = RouteClear then
                            NoPathTo destination
                        else
                            destinationCollision
                let accepted = path |> Option.defaultValue []
                { UnitId = id
                  Origin = origin
                  Destination = destination
                  Distance = int32 accepted.Length
                  DistanceMillimeters =
                    accepted
                    |> List.fold (fun (total, previous) address ->
                        total + stepDistance previous address, address) (0, origin)
                    |> fst
                  MovementCostMillimeters =
                    accepted
                    |> List.fold (fun (total, previous) address ->
                        let probe =
                            { unit with
                                Column = previous.CellColumn
                                Row = previous.CellRow }
                        total
                        + movementCost
                            handoff.RuntimeMap
                            probe
                            previous
                            address,
                        address) (0, origin)
                    |> fst
                  Route = List.toArray accepted
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
                  LastCombatEvents = []
                  AttackRecoveryTicks = Map.empty
                  MovementCreditsMillimeters = Map.empty
                  MovementProgress = Map.empty
                  MovementIntents = Map.empty
                  PlannedRoutes = Map.empty
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

    let attackProfileFor (unit: EditorUnit) =
        match unit.ClassId.Trim().ToLowerInvariant() with
        | "rifleman" ->
            { Delivery = ProjectileDelivery
              Range = 8
              Damage = 12
              RecoveryTicks = 10
              AreaShape = None }
        | "troll" ->
            { Delivery = MeleeDelivery
              Range = 1
              Damage = 18
              RecoveryTicks = 20
              AreaShape = None }
        | "orc" ->
            { Delivery = MeleeDelivery
              Range = 1
              Damage = 5
              RecoveryTicks = 20
              AreaShape = None }
        | "goblin" ->
            { Delivery = MeleeDelivery
              Range = 1
              Damage = 2
              RecoveryTicks = 16
              AreaShape = None }
        | _ ->
            { Delivery = MeleeDelivery
              Range = 1
              Damage = 1
              RecoveryTicks = 20
              AreaShape = None }

    let private axisGap firstStart firstSize secondStart secondSize =
        let firstEnd = firstStart + firstSize - 1
        let secondEnd = secondStart + secondSize - 1
        if firstEnd < secondStart then secondStart - firstEnd
        elif secondEnd < firstStart then firstStart - secondEnd
        else 0

    let private footprintDistance (first: EditorUnit) (second: EditorUnit) =
        max
            (axisGap first.Column first.Size second.Column second.Size)
            (axisGap first.Row first.Size second.Row second.Size)

    let private nearestHostile (unit: EditorUnit) (map: MapDefinition) =
        map.Units
        |> Map.toSeq
        |> Seq.map snd
        |> Seq.filter (fun (other: EditorUnit) ->
            other.Id <> unit.Id
            && other.Side <> unit.Side
            && other.Health > 0)
        |> Seq.sortBy (fun (other: EditorUnit) ->
            footprintDistance unit other, other.Id)
        |> Seq.tryHead

    let private deliveryLabel delivery =
        match delivery with
        | MeleeDelivery -> "melee"
        | ProjectileDelivery -> "ranged"
        | LobbedAreaDelivery -> "lobbed area"
        | SpellAreaDelivery -> "spell area"

    let private projectilePathClear
        (attacker: EditorUnit)
        (target: EditorUnit)
        (map: MapDefinition)
        =
        let origin =
            { CellColumn = attacker.Column + attacker.Size / 2
              CellRow = attacker.Row + attacker.Size / 2 }
        let destination =
            { CellColumn = target.Column + target.Size / 2
              CellRow = target.Row + target.Size / 2 }
        let mutable previous = origin
        let mutable clear = true
        for cell in directRoute origin destination do
            if clear then
                let columnDelta = cell.CellColumn - previous.CellColumn
                let rowDelta = cell.CellRow - previous.CellRow
                let horizontalEdge =
                    if columnDelta > 0 then
                        Some(previous.CellColumn, previous.CellRow, EastEdge)
                    elif columnDelta < 0 then
                        Some(cell.CellColumn, previous.CellRow, EastEdge)
                    else
                        None
                let verticalEdge =
                    if rowDelta > 0 then
                        Some(previous.CellColumn, previous.CellRow, SouthEdge)
                    elif rowDelta < 0 then
                        Some(previous.CellColumn, cell.CellRow, SouthEdge)
                    else
                        None
                let blockedEdge =
                    [ horizontalEdge; verticalEdge ]
                    |> List.choose id
                    |> List.exists (edgeBlocks map)
                let blockedTerrain =
                    Map.tryFind (cell.CellColumn, cell.CellRow) map.Terrain = Some Blocked
                clear <- not blockedEdge && not blockedTerrain
                previous <- cell
        clear

    let movementProfileFor (unit: EditorUnit) =
        let speed =
            match unit.ClassId.Trim().ToLowerInvariant() with
            | "goblin" -> 2000
            | "rifleman" -> 1500
            | "orc" -> 1200
            | "troll" -> 1000
            | "observation-drone" -> 3000
            | _ -> 1500
        { SpeedMillimetersPerSecond = speed
          CellMillimeters = CellMillimeters }

    let private canAttack
        (attacker: EditorUnit)
        (target: EditorUnit)
        profile
        map
        =
        footprintDistance attacker target <= profile.Range
        &&
        match profile.Delivery with
        | ProjectileDelivery -> projectilePathClear attacker target map
        | _ -> true

    let private routeToAttackPosition
        (unit: EditorUnit)
        (target: EditorUnit)
        profile
        map
        =
        match
            findPathTo
                map
                unit
                (fun candidate -> canAttack candidate target profile map)
                (fun _ -> 0)
        with
        | Some route -> Some route
        | None ->
            findClosestPath
                map
                unit
                (fun candidate -> footprintDistance candidate target)
            |> function
                | [] -> None
                | route -> Some route

    let private step handoff =
        let mutable map = handoff.RuntimeMap
        let mutable credits = handoff.MovementCreditsMillimeters
        let mutable movementProgress = handoff.MovementProgress
        let mutable intents = handoff.MovementIntents
        let mutable plannedRoutes = handoff.PlannedRoutes
        let mutable attackRecovery = handoff.AttackRecoveryTicks
        let events = ResizeArray<string>()
        let combatEvents = ResizeArray<SimulatorCombatEvent>()

        let availableCredit unit =
            let profile = movementProfileFor unit
            let earned = profile.SpeedMillimetersPerSecond / TicksPerSecond
            let current =
                Map.tryFind unit.Id credits |> Option.defaultValue 0
            let available =
                min MaximumMovementCreditMillimeters (current + earned)
            credits <- Map.add unit.Id available credits
            available

        let attemptMove unit destination verb =
            let origin =
                { CellColumn = unit.Column
                  CellRow = unit.Row }
            let required = movementCost map unit origin destination
            let available =
                Map.tryFind unit.Id credits |> Option.defaultValue 0
            match movementCollision map unit destination with
            | RouteClear when available < required ->
                movementProgress <-
                    Map.add
                        unit.Id
                        { Origin = origin
                          Destination = destination
                          ProgressMillimeters = available
                          CostMillimeters = required }
                        movementProgress
                events.Add(
                    "Unit "
                    + string unit.Id
                    + " prepares to move "
                    + verb
                    + " ("
                    + string available
                    + "/"
                    + string required
                    + " mm)."
                )
                false, false
            | RouteClear ->
                    let moved =
                        { unit with
                            Column = destination.CellColumn
                            Row = destination.CellRow }
                    map <- { map with Units = Map.add unit.Id moved map.Units }
                    credits <- Map.add unit.Id (available - required) credits
                    movementProgress <- Map.remove unit.Id movementProgress
                    events.Add(
                        "Unit "
                        + string unit.Id
                        + " moves "
                        + verb
                        + " by "
                        + string (stepDistance origin destination)
                        + " mm, spending "
                        + string required
                        + " mm of movement credit."
                    )
                    true, true
            | collision ->
                movementProgress <- Map.remove unit.Id movementProgress
                events.Add(
                    "Unit "
                    + string unit.Id
                    + " cannot move "
                    + verb
                    + ": "
                    + string collision
                    + "."
                )
                false, true

        for id in handoff.RuntimeMap.Units |> Map.toSeq |> Seq.map fst |> Seq.sort do
            match Map.tryFind id map.Units with
            | Some unit when unit.Health > 0 ->
                availableCredit unit |> ignore
                let recovery =
                    Map.tryFind id attackRecovery
                    |> Option.defaultValue 0
                    |> fun ticks -> max 0 (ticks - 1)
                attackRecovery <-
                    if recovery = 0 then
                        Map.remove id attackRecovery
                    else
                        Map.add id recovery attackRecovery
                match unit.Controller with
                | Manual ->
                    match Map.tryFind id plannedRoutes with
                    | Some(next :: remaining) ->
                        let moved, resolved =
                            attemptMove unit next "along its planned route"
                        if resolved then
                            plannedRoutes <-
                                if moved && not (List.isEmpty remaining) then
                                    Map.add id remaining plannedRoutes
                                else
                                    Map.remove id plannedRoutes
                    | _ ->
                        plannedRoutes <- Map.remove id plannedRoutes
                        match Map.tryFind id intents with
                        | Some direction ->
                            let columnDelta, rowDelta = directionDelta direction
                            let destination =
                                { CellColumn = unit.Column + int32 columnDelta
                                  CellRow = unit.Row + int32 rowDelta }
                            let _, resolved =
                                attemptMove unit destination (directionCode direction)
                            if resolved then
                                intents <- Map.remove id intents
                        | None ->
                            movementProgress <- Map.remove id movementProgress
                            events.Add("Unit " + string id + " awaits manual input.")
                | Scripted when List.isEmpty unit.Script ->
                    movementProgress <- Map.remove id movementProgress
                    events.Add("Unit " + string id + " has no script.")
                | Scripted ->
                    let direction = unit.Script[unit.ScriptIndex % unit.Script.Length]
                    let columnDelta, rowDelta = directionDelta direction
                    let destination =
                        { CellColumn = unit.Column + int32 columnDelta
                          CellRow = unit.Row + int32 rowDelta }
                    let _, resolved =
                        attemptMove unit destination ("script " + directionCode direction)
                    if resolved then
                        let current = Map.find id map.Units
                        map <-
                            { map with
                                Units =
                                    Map.add
                                        id
                                        { current with
                                            ScriptIndex = unit.ScriptIndex + 1 }
                                        map.Units }
                | General ->
                    let lockedDestination =
                        Map.tryFind id movementProgress
                        |> Option.bind (fun progress ->
                            if
                                progress.Origin.CellColumn = unit.Column
                                && progress.Origin.CellRow = unit.Row
                            then
                                Some progress.Destination
                            else
                                None)
                    match lockedDestination with
                    | Some destination ->
                        attemptMove
                            unit
                            destination
                            "along its locked approach segment"
                        |> ignore
                    | None ->
                        match nearestHostile unit map with
                        | None ->
                            movementProgress <- Map.remove id movementProgress
                            events.Add("Unit " + string id + " holds; no hostile is present.")
                        | Some target ->
                            let profile = attackProfileFor unit
                            if canAttack unit target profile map then
                                movementProgress <- Map.remove id movementProgress
                                if recovery > 0 then
                                    events.Add(
                                        "Unit "
                                        + string id
                                        + " recovers from its attack for "
                                        + string recovery
                                        + " more ticks."
                                    )
                                else
                                    let damaged =
                                        { target with
                                            Health = max 0 (target.Health - profile.Damage) }
                                    map <- { map with Units = Map.add target.Id damaged map.Units }
                                    attackRecovery <-
                                        Map.add id profile.RecoveryTicks attackRecovery
                                    let summary =
                                        "Unit "
                                        + string id
                                        + " makes a "
                                        + deliveryLabel profile.Delivery
                                        + " attack against unit "
                                        + string target.Id
                                        + " for "
                                        + string profile.Damage
                                        + " damage."
                                    events.Add summary
                                    combatEvents.Add(
                                        { Tick = handoff.Tick + 1
                                          SourceUnitId = id
                                          Target = UnitCombatTarget target.Id
                                          Delivery = profile.Delivery
                                          Damage = profile.Damage
                                          Summary = summary }
                                    )
                            else
                                match routeToAttackPosition unit target profile map with
                                | Some(next :: _) ->
                                    attemptMove unit next "toward its attack position"
                                    |> ignore
                                | _ ->
                                    movementProgress <- Map.remove id movementProgress
                                    events.Add(
                                        "Unit "
                                        + string id
                                        + " holds; no collision-free approach exists toward unit "
                                        + string target.Id
                                        + "."
                                    )
            | _ -> ()
        let nextTick = handoff.Tick + 1
        let recentCombatEvents =
            handoff.LastCombatEvents
            |> List.filter (fun combat ->
                nextTick - combat.Tick <= 5)
            |> fun retained ->
                List.append retained (List.ofSeq combatEvents)
        { handoff with
            RuntimeMap = map
            Tick = nextTick
            LastEvents = List.ofSeq events
            LastCombatEvents = recentCombatEvents
            AttackRecoveryTicks = attackRecovery
            MovementCreditsMillimeters = credits
            MovementProgress = movementProgress
            MovementIntents = intents
            PlannedRoutes = plannedRoutes
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
                { handoff with
                    MovementIntents =
                        Map.add unit.Id direction handoff.MovementIntents
                    MovementProgress =
                        Map.remove unit.Id handoff.MovementProgress
                    PlannedRoutes = Map.remove unit.Id handoff.PlannedRoutes
                    LastEvents =
                        [ "Unit "
                          + string unit.Id
                          + " receives movement intent "
                          + directionCode direction
                          + "; advance simulation time to resolve it." ]
                    LastCombatEvents = []
                    PreviewDestination = None })
            |> Option.defaultValue handoff
        | SetSimulatorController controller ->
            updateSelected (fun unit -> { unit with Controller = controller })
        | SetSimulatorScript text ->
            match MapEditor.parseScript text with
            | Ok script ->
                updateSelected (fun unit ->
                    { unit with Script = script; ScriptIndex = 0 })
            | Error error ->
                { handoff with
                    LastEvents = [ error ]
                    LastCombatEvents = [] }
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
                    { handoff with
                        PlannedRoutes =
                            Map.add
                                routePreview.UnitId
                                (Array.toList routePreview.Route)
                                handoff.PlannedRoutes
                        MovementIntents =
                            Map.remove routePreview.UnitId handoff.MovementIntents
                        MovementProgress =
                            Map.remove routePreview.UnitId handoff.MovementProgress
                        LastEvents =
                            [ "Unit "
                              + string routePreview.UnitId
                              + " accepts a "
                              + string routePreview.Distance
                              + "-step, "
                              + string routePreview.DistanceMillimeters
                              + " mm path costing "
                              + string routePreview.MovementCostMillimeters
                              + " mm of movement credit; advance simulation time to move." ]
                        LastCombatEvents = []
                        PreviewDestination = None }
                | Some routePreview ->
                    { handoff with
                        LastEvents =
                            [ "Preview route rejected: " + string routePreview.Collision + "." ]
                        LastCombatEvents = [] }
                | None -> handoff

    let presentationOffsets handoff =
        handoff.MovementProgress
        |> Map.map (fun _ progress ->
            let fraction =
                float progress.ProgressMillimeters
                / float (max 1 progress.CostMillimeters)
                |> min 1.0
            float (
                progress.Destination.CellColumn
                - progress.Origin.CellColumn
            )
            * fraction,
            float (
                progress.Destination.CellRow
                - progress.Origin.CellRow
            )
            * fraction)

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
        let routeOverlay kind unitId origin route label =
            { Id = "simulator-route-preview"
              Kind = kind
              Scope = SelectedUnitOverlay unitId
              GeometryRevision = int32 (handoff.Revision.Number % int64 Int32.MaxValue)
              Points =
                Array.append
                    [| float origin.CellColumn + 0.5
                       float origin.CellRow + 0.5 |]
                    (route
                     |> Array.collect (fun address ->
                         [| float address.CellColumn + 0.5
                            float address.CellRow + 0.5 |]))
              Label = Disclosed label }
        let previewOverlay =
            handoff.PreviewDestination
            |> Option.bind (fun destination -> preview selectedUnitId destination handoff)
            |> Option.map (fun routePreview ->
                let kind =
                    if routePreview.Collision = RouteClear then
                        "route-preview-clear"
                    else
                        "route-preview-collision"
                routeOverlay
                    kind
                    routePreview.UnitId
                    routePreview.Origin
                    routePreview.Route
                    ("Route "
                     + string routePreview.Distance
                     + " steps; "
                     + (if routePreview.Collision = RouteClear then
                            "clear"
                        else
                            "collision: " + string routePreview.Collision)))
        let plannedOverlay =
            selectedUnitId
            |> Option.bind (fun unitId ->
                Map.tryFind unitId handoff.PlannedRoutes
                |> Option.bind (fun route ->
                    Map.tryFind unitId handoff.RuntimeMap.Units
                    |> Option.map (fun unit ->
                        let origin =
                            { CellColumn = unit.Column
                              CellRow = unit.Row }
                        routeOverlay
                            "route-planned"
                            unitId
                            origin
                            (List.toArray route)
                            ("Queued path with "
                             + string route.Length
                             + " remaining steps."))))
        let combatEvents =
            handoff.LastCombatEvents
            |> List.mapi (fun index combat ->
                { Id = combat.Tick * 1000 + 500 + int32 index
                  Tick = combat.Tick
                  Kind =
                    match combat.Delivery with
                    | MeleeDelivery -> "combat-melee"
                    | ProjectileDelivery -> "combat-projectile"
                    | LobbedAreaDelivery -> "combat-lobbed-area"
                    | SpellAreaDelivery -> "combat-spell-area"
                  SourceUnitId = Disclosed combat.SourceUnitId
                  TargetUnitId =
                    match combat.Target with
                    | UnitCombatTarget unitId -> Disclosed unitId
                    | AreaCombatTarget _ -> NotApplicable
                  Summary = Disclosed combat.Summary })
            |> List.toArray
        let combatSummaries =
            handoff.LastCombatEvents
            |> List.map _.Summary
            |> Set.ofList
        let narrativeEvents =
            baseFrame.Events
            |> Array.filter (fun event ->
                match event.Summary with
                | Disclosed summary -> not (Set.contains summary combatSummaries)
                | _ -> true)
        { baseFrame with
            Overlays =
                previewOverlay
                |> Option.orElse plannedOverlay
                |> Option.toArray
            Events = Array.append narrativeEvents combatEvents }

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
