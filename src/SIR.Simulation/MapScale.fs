namespace SIR.Simulation

open System
open FS.GG.Game.Core
open SIR.Domain

/// Runtime-neutral terrain semantics for the map-scale kernel.
type MapScaleTerrain =
    | OpenTerrain
    | RoughTerrain
    | BlockedTerrain
    | ObjectiveTerrain

type MapScaleEdgeKind =
    | WallEdge
    | DoorEdge of isOpen: bool
    | WindowEdge

type MapScaleEdgeDirection =
    | EastEdge
    | SouthEdge

type MapScaleController =
    | ManualController
    | ScriptedController
    | GeneralController

type CombatDelivery =
    | MeleeDelivery
    | ProjectileDelivery
    | LobbedAreaDelivery
    | SpellAreaDelivery

type AreaShape =
    | BurstArea of radius: int32
    | ConeArea of range: int32 * angleDegrees: int32
    | RayArea of length: int32 * width: int32
    | RectangleArea of width: int32 * depth: int32

type CombatTarget =
    | UnitTarget of unitId: int32
    | AreaTarget of origin: Cell * shape: AreaShape

type CombatProfile =
    { Delivery: CombatDelivery
      Range: int32
      Damage: int32
      RecoveryTicks: int32
      AreaShape: AreaShape option }

type MovementProfile =
    { SpeedMillimetersPerSecond: int32
      CellMillimeters: int32 }

type EngagementState =
    { Target: CombatTarget
      Profile: CombatProfile
      RecoveryTicksRemaining: int32 }

type MapScaleUnit =
    { Id: int32
      Side: int32
      ClassId: string
      Cell: Cell
      Size: int32
      Health: int32
      Controller: MapScaleController
      Script: Direction8 list
      ScriptIndex: int32
      BodyFacing: Direction8
      AttentionDirection: Direction8 }

type MapScaleBoard =
    { Width: int32
      Height: int32
      Terrain: Map<Cell, MapScaleTerrain>
      Edges: Map<int32 * int32 * MapScaleEdgeDirection, MapScaleEdgeKind> }

type MovementProgress =
    { Origin: Cell
      Destination: Cell
      ProgressMillimeters: int32
      CostMillimeters: int32 }

type MapScaleState =
    { Tick: int32
      Board: MapScaleBoard
      Units: Map<int32, MapScaleUnit>
      MovementCreditsMillimeters: Map<int32, int32>
      MovementProgress: Map<int32, MovementProgress>
      MovementIntents: Map<int32, Direction8>
      PlannedRoutes: Map<int32, Cell list>
      Engagements: Map<int32, EngagementState> }

type MovementBlocker =
    | OutsideBoard of Cell
    | BlockedTerrainCollision of Cell
    | BlockingEdge of Cell * Cell
    | OccupiedCell of Cell * unitId: int32
    | DestinationConflict of Cell
    | CrossingConflict of otherUnitId: int32

type MapScaleEvent =
    | MovementPrepared of unitId: int32 * origin: Cell * destination: Cell * progress: int32 * cost: int32
    | UnitMoved of unitId: int32 * origin: Cell * destination: Cell * distance: int32 * cost: int32
    | MovementRejected of unitId: int32 * destination: Cell * blocker: MovementBlocker
    | UnitHeld of unitId: int32 * reason: string
    | AttackRecovering of unitId: int32 * ticksRemaining: int32
    | AttackResolved of sourceUnitId: int32 * target: CombatTarget * delivery: CombatDelivery * damage: int32

type MapScalePhase =
    | CollectPhase
    | ValidatePhase
    | ResolvePhase
    | CommitPhase

type MapScaleCheckpoint =
    { Tick: int32
      Phase: MapScalePhase
      State: MapScaleState
      Events: MapScaleEvent list }

type MapScaleTickResult =
    { State: MapScaleState
      Events: MapScaleEvent list
      Checkpoints: MapScaleCheckpoint list
      Counters: MapScaleCounters }

and MapScaleCounters =
    { LosSamples: int32
      CombatResolutions: int32 }

type MapScaleDivergence =
    { Tick: int32
      Phase: MapScalePhase
      ByteOffset: int32
      Expected: byte
      Actual: byte }

type RouteResult =
    { Route: Cell list
      DistanceMillimeters: int32
      MovementCostMillimeters: int32
      ExpandedNodes: int32 }

/// Authoritative map-scale movement, collision, combat, and controller phases.
[<RequireQualifiedAccess>]
module MapScale =
    [<Literal>]
    let TicksPerSecond = 20

    [<Literal>]
    let CellMillimeters = 250

    [<Literal>]
    let DiagonalCellMillimeters = 354

    [<Literal>]
    let MaximumMovementCreditMillimeters = 1060

    let cell column row = { Col = column; Row = row }

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
        if value < 0 then -1 elif value > 0 then 1 else 0

    let footprint size (position: Cell) =
        [| for row in position.Row .. position.Row + size - 1 do
               for column in position.Col .. position.Col + size - 1 do
                   yield cell column row |]

    let stepDistance origin destination =
        if origin.Col <> destination.Col && origin.Row <> destination.Row then
            DiagonalCellMillimeters
        else
            CellMillimeters

    let movementProfileFor (classId: string) =
        let speed =
            match classId.Trim().ToLowerInvariant() with
            | "goblin" -> 2000
            | "rifleman" -> 1500
            | "orc" -> 1200
            | "troll" -> 1000
            | "observation-drone" -> 3000
            | _ -> 1500
        { SpeedMillimetersPerSecond = speed
          CellMillimeters = CellMillimeters }

    let combatProfileFor (classId: string) =
        match classId.Trim().ToLowerInvariant() with
        | "rifleman" ->
            { Delivery = ProjectileDelivery; Range = 16; Damage = 12
              RecoveryTicks = 10; AreaShape = None }
        | "troll" ->
            { Delivery = MeleeDelivery; Range = 2; Damage = 18
              RecoveryTicks = 20; AreaShape = None }
        | "orc" ->
            { Delivery = MeleeDelivery; Range = 2; Damage = 5
              RecoveryTicks = 20; AreaShape = None }
        | "goblin" ->
            { Delivery = MeleeDelivery; Range = 2; Damage = 2
              RecoveryTicks = 16; AreaShape = None }
        | _ ->
            { Delivery = MeleeDelivery; Range = 2; Damage = 1
              RecoveryTicks = 20; AreaShape = None }

    let movementCost board unit origin destination =
        let distance = stepDistance origin destination
        let rough =
            footprint unit.Size destination
            |> Array.exists (fun address ->
                Map.tryFind address board.Terrain = Some RoughTerrain)
        if rough then distance * 3 / 2 else distance

    let private edgeBlocks board key =
        board.Edges
        |> Map.tryFind key
        |> Option.exists (function
            | DoorEdge true -> false
            | DoorEdge false
            | WallEdge
            | WindowEdge -> true)

    let private crossedEdges unit destination =
        let columnDelta = destination.Col - unit.Cell.Col
        let rowDelta = destination.Row - unit.Cell.Row
        let horizontal =
            if columnDelta > 0 then
                [| for row in unit.Cell.Row .. unit.Cell.Row + unit.Size - 1 ->
                       unit.Cell.Col + unit.Size - 1, row, EastEdge |]
            elif columnDelta < 0 then
                [| for row in unit.Cell.Row .. unit.Cell.Row + unit.Size - 1 ->
                       unit.Cell.Col - 1, row, EastEdge |]
            else [||]
        let vertical =
            if rowDelta > 0 then
                [| for column in unit.Cell.Col .. unit.Cell.Col + unit.Size - 1 ->
                       column, unit.Cell.Row + unit.Size - 1, SouthEdge |]
            elif rowDelta < 0 then
                [| for column in unit.Cell.Col .. unit.Cell.Col + unit.Size - 1 ->
                       column, unit.Cell.Row - 1, SouthEdge |]
            else [||]
        Array.append horizontal vertical

    let private staticCollision board units unit destination ignoreOccupancy =
        let cells = footprint unit.Size destination
        match cells |> Array.tryFind (fun p -> p.Col < 0 || p.Row < 0 || p.Col >= board.Width || p.Row >= board.Height) with
        | Some address -> Some(OutsideBoard address)
        | None ->
            match cells |> Array.tryFind (fun p -> Map.tryFind p board.Terrain = Some MapScaleTerrain.BlockedTerrain) with
            | Some address -> Some(BlockedTerrainCollision address)
            | None ->
                match crossedEdges unit destination |> Array.tryFind (edgeBlocks board) with
                | Some(column, row, direction) ->
                    let origin, target =
                        match direction with
                        | EastEdge -> cell column row, cell (column + 1) row
                        | SouthEdge -> cell column row, cell column (row + 1)
                    Some(BlockingEdge(origin, target))
                | None when not ignoreOccupancy ->
                    units
                    |> Map.toSeq
                    |> Seq.filter (fun (id, _) -> id <> unit.Id)
                    |> Seq.sortBy fst
                    |> Seq.tryPick (fun (id, other) ->
                        let occupied = footprint other.Size other.Cell |> Set.ofArray
                        cells |> Array.tryFind (fun p -> Set.contains p occupied)
                        |> Option.map (fun p -> OccupiedCell(p, id)))
                | None -> None

    let movementCollision board units unit destination =
        let direct = staticCollision board units unit destination false
        let dc = destination.Col - unit.Cell.Col
        let dr = destination.Row - unit.Cell.Row
        if direct.IsNone && dc <> 0 && dr <> 0 then
            let horizontal = cell destination.Col unit.Cell.Row
            let vertical = cell unit.Cell.Col destination.Row
            match staticCollision board units unit horizontal false, staticCollision board units unit vertical false with
            | None, None -> None
            | Some collision, None
            | None, Some collision -> Some collision
            | Some first, Some _ -> Some first
        else direct

    let private staticMovementCollision board unit destination =
        let direct = staticCollision board Map.empty unit destination true
        let dc = destination.Col - unit.Cell.Col
        let dr = destination.Row - unit.Cell.Row
        if direct.IsNone && dc <> 0 && dr <> 0 then
            let horizontal = cell destination.Col unit.Cell.Row
            let vertical = cell unit.Cell.Col destination.Row
            match staticCollision board Map.empty unit horizontal true, staticCollision board Map.empty unit vertical true with
            | None, None -> None
            | Some collision, None
            | None, Some collision -> Some collision
            | Some first, Some _ -> Some first
        else direct

    let private pathNeighbors =
        [| 0, -1; 1, 0; 0, 1; -1, 0; 1, -1; 1, 1; -1, 1; -1, -1 |]

    let private heuristic origin destination =
        let column = abs (destination.Col - origin.Col)
        let row = abs (destination.Row - origin.Row)
        let diagonal = min column row
        diagonal * DiagonalCellMillimeters + (max column row - diagonal) * CellMillimeters

    let tryFindPath board units unit destination =
        let origin = unit.Cell
        let mutable frontier = [ 0, 0, origin.Row, origin.Col, origin ]
        let mutable costs = Map.ofList [ origin, 0 ]
        let mutable previous = Map.empty<Cell, Cell>
        let mutable result = None
        let mutable expandedNodes = 0
        while result.IsNone && not (List.isEmpty frontier) do
            let _, cost, _, _, current = List.min frontier
            frontier <- frontier |> List.filter (fun (_, _, _, _, p) -> p <> current)
            expandedNodes <- expandedNodes + 1
            if current = destination then
                let mutable cursor = current
                let mutable route = []
                while cursor <> origin do
                    route <- cursor :: route
                    cursor <- Map.find cursor previous
                let distance, movementCost =
                    ((0, 0, origin), route)
                    ||> List.fold (fun (distance, total, prior) next ->
                        let probe = { unit with Cell = prior }
                        distance + stepDistance prior next,
                        total + movementCost board probe prior next,
                        next)
                    |> fun (distance, total, _) -> distance, total
                result <-
                    Some
                        { Route = route
                          DistanceMillimeters = distance
                          MovementCostMillimeters = movementCost
                          ExpandedNodes = expandedNodes }
            else
                let probe = { unit with Cell = current }
                for dc, dr in pathNeighbors do
                    let next = cell (current.Col + dc) (current.Row + dr)
                    if movementCollision board units probe next |> Option.isNone then
                        let nextCost = cost + movementCost board probe current next
                        let known = Map.tryFind next costs |> Option.defaultValue Int32.MaxValue
                        if nextCost < known then
                            costs <- Map.add next nextCost costs
                            previous <- Map.add next current previous
                            frontier <- (nextCost + heuristic next destination, nextCost, next.Row, next.Col, next) :: frontier
        result

    let private axisGap firstStart firstSize secondStart secondSize =
        let firstEnd = firstStart + firstSize - 1
        let secondEnd = secondStart + secondSize - 1
        if firstEnd < secondStart then secondStart - firstEnd
        elif secondEnd < firstStart then firstStart - secondEnd
        else 0

    let footprintDistance first second =
        max
            (axisGap first.Cell.Col first.Size second.Cell.Col second.Size)
            (axisGap first.Cell.Row first.Size second.Cell.Row second.Size)

    let private directRoute origin destination =
        let mutable column = origin.Col
        let mutable row = origin.Row
        [| while column <> destination.Col || row <> destination.Row do
               column <- column + sign (destination.Col - column)
               row <- row + sign (destination.Row - row)
               yield cell column row |]

    let private projectilePathClear attacker target board =
        let origin = cell (attacker.Cell.Col + attacker.Size / 2) (attacker.Cell.Row + attacker.Size / 2)
        let destination = cell (target.Cell.Col + target.Size / 2) (target.Cell.Row + target.Size / 2)
        let mutable previous = origin
        let mutable clear = true
        let samples = 1
        for current in directRoute origin destination do
            if clear then
                let dc, dr = current.Col - previous.Col, current.Row - previous.Row
                let edges =
                    [ if dc > 0 then previous.Col, previous.Row, EastEdge
                      elif dc < 0 then current.Col, previous.Row, EastEdge
                      if dr > 0 then previous.Col, previous.Row, SouthEdge
                      elif dr < 0 then previous.Col, current.Row, SouthEdge ]
                clear <-
                    not (edges |> List.exists (edgeBlocks board))
                    && Map.tryFind current board.Terrain <> Some MapScaleTerrain.BlockedTerrain
                previous <- current
        clear, samples

    let private canAttackMeasured attacker target profile board =
        if footprintDistance attacker target > profile.Range then
            false, 0
        elif profile.Delivery <> ProjectileDelivery then
            true, 0
        else
            projectilePathClear attacker target board

    let canAttack attacker target profile board =
        canAttackMeasured attacker target profile board |> fst

    type private CollectedIntent =
        | MoveIntent of MapScaleUnit * Cell * string
        | AttackIntent of MapScaleUnit * MapScaleUnit * CombatProfile
        | HoldIntent of MapScaleUnit * string

    type private ValidatedMove =
        { Unit: MapScaleUnit; Destination: Cell; Verb: string; Available: int32; Required: int32 }

    let private nearestHostile unit units =
        units |> Map.toSeq |> Seq.map snd
        |> Seq.filter (fun other -> other.Id <> unit.Id && other.Side <> unit.Side && other.Health > 0)
        |> Seq.sortBy (fun other -> footprintDistance unit other, other.Id)
        |> Seq.tryHead

    let private nextApproachStep state unit target =
        let profile = combatProfileFor unit.ClassId
        pathNeighbors
        |> Array.choose (fun (dc, dr) ->
            let destination = cell (unit.Cell.Col + dc) (unit.Cell.Row + dr)
            let candidate = { unit with Cell = destination }
            movementCollision state.Board state.Units unit destination
            |> Option.map (fun _ -> None)
            |> Option.defaultValue (Some(footprintDistance candidate target, movementCost state.Board unit unit.Cell destination, destination)))
        |> Array.sort
        |> Array.tryHead
        |> Option.map (fun (_, _, destination) -> destination)

    let private collect state =
        let mutable losSamples = 0
        let intents =
            state.Units |> Map.toList |> List.map snd |> List.sortBy _.Id
            |> List.choose (fun unit ->
                if unit.Health <= 0 then
                    None
                else
                    match Map.tryFind unit.Id state.MovementProgress with
                    | Some progress when progress.Origin = unit.Cell ->
                        Some(MoveIntent(unit, progress.Destination, "along its locked approach segment"))
                    | _ ->
                        match unit.Controller with
                        | ManualController ->
                            match Map.tryFind unit.Id state.PlannedRoutes, Map.tryFind unit.Id state.MovementIntents with
                            | Some(next :: _), _ -> Some(MoveIntent(unit, next, "along its planned route"))
                            | _, Some direction ->
                                let dc, dr = directionDelta direction
                                Some(MoveIntent(unit, cell (unit.Cell.Col + dc) (unit.Cell.Row + dr), string direction))
                            | _ -> Some(HoldIntent(unit, "awaits manual input"))
                        | ScriptedController when List.isEmpty unit.Script -> Some(HoldIntent(unit, "has no script"))
                        | ScriptedController ->
                            let direction = unit.Script[int unit.ScriptIndex % unit.Script.Length]
                            let dc, dr = directionDelta direction
                            Some(MoveIntent(unit, cell (unit.Cell.Col + dc) (unit.Cell.Row + dr), "script " + string direction))
                        | GeneralController ->
                            match nearestHostile unit state.Units with
                            | None -> Some(HoldIntent(unit, "holds; no hostile is present"))
                            | Some target ->
                                let profile = combatProfileFor unit.ClassId
                                let canResolve, samples = canAttackMeasured unit target profile state.Board
                                losSamples <- losSamples + samples
                                if canResolve then Some(AttackIntent(unit, target, profile))
                                else
                                    match nextApproachStep state unit target with
                                    | Some next -> Some(MoveIntent(unit, next, "toward its attack position"))
                                    | None -> Some(HoldIntent(unit, "holds; no collision-free approach exists")))
        intents, losSamples

    let private checkpoint phase (state: MapScaleState) events : MapScaleCheckpoint =
        { Tick = state.Tick + 1; Phase = phase; State = state; Events = events }

    /// Executes explicit collect, validate, resolve, and atomic commit phases.
    let tick (state: MapScaleState) =
        let earnedCredits =
            state.Units
            |> Map.fold (fun credits id unit ->
                if unit.Health <= 0 then credits else
                let profile = movementProfileFor unit.ClassId
                let earned = profile.SpeedMillimetersPerSecond / TicksPerSecond
                let current = Map.tryFind id credits |> Option.defaultValue 0
                Map.add id (min MaximumMovementCreditMillimeters (current + earned)) credits)
                state.MovementCreditsMillimeters
        let collected, losSamples = collect state
        let collectState = { state with MovementCreditsMillimeters = earnedCredits }
        let collectCheckpoint = checkpoint CollectPhase collectState []

        let validatedMoves, validationEvents =
            (([], []), collected)
            ||> List.fold (fun (moves, events) intent ->
                match intent with
                | HoldIntent(unit, reason) -> moves, UnitHeld(unit.Id, reason) :: events
                | AttackIntent _ -> moves, events
                | MoveIntent(unit, destination, verb) ->
                    let available = Map.find unit.Id earnedCredits
                    let required = movementCost state.Board unit unit.Cell destination
                    match staticMovementCollision state.Board unit destination with
                    | Some blocker -> moves, MovementRejected(unit.Id, destination, blocker) :: events
                    | None when available < required ->
                        moves, MovementPrepared(unit.Id, unit.Cell, destination, available, required) :: events
                    | None ->
                        { Unit = unit; Destination = destination; Verb = verb; Available = available; Required = required } :: moves, events)
        let validatedMoves = List.rev validatedMoves
        let validationEvents = List.rev validationEvents
        let rejectedIds =
            validationEvents
            |> List.choose (function
                | MovementRejected(id, _, _) -> Some id
                | _ -> None)
        let validatedState =
            { collectState with
                MovementCreditsMillimeters =
                    rejectedIds
                    |> List.fold (fun credits id -> Map.add id 0 credits) collectState.MovementCreditsMillimeters
                MovementProgress =
                    rejectedIds
                    |> List.fold (fun progress id -> Map.remove id progress) collectState.MovementProgress }
        let validateCheckpoint = checkpoint ValidatePhase validatedState validationEvents

        let destinationCounts =
            validatedMoves |> List.collect (fun move -> footprint move.Unit.Size move.Destination |> Array.toList)
            |> List.countBy id |> Map.ofList
        let conflicts =
            validatedMoves
            |> List.choose (fun move ->
                let destinationConflict =
                    footprint move.Unit.Size move.Destination
                    |> Array.tryFind (fun p -> Map.find p destinationCounts > 1)
                match destinationConflict with
                | Some p -> Some(move.Unit.Id, DestinationConflict p)
                | None ->
                    let crossing =
                        validatedMoves
                        |> List.tryFind (fun other ->
                        other.Unit.Id <> move.Unit.Id
                        && footprint move.Unit.Size move.Destination |> Set.ofArray
                           |> Set.intersect (footprint other.Unit.Size other.Unit.Cell |> Set.ofArray) |> Set.isEmpty |> not
                        && footprint other.Unit.Size other.Destination |> Set.ofArray
                           |> Set.intersect (footprint move.Unit.Size move.Unit.Cell |> Set.ofArray) |> Set.isEmpty |> not)
                    match crossing with
                    | Some other -> Some(move.Unit.Id, CrossingConflict other.Unit.Id)
                    | None ->
                        let destinationCells = footprint move.Unit.Size move.Destination |> Set.ofArray
                        state.Units
                        |> Map.toSeq
                        |> Seq.filter (fun (id, _) -> id <> move.Unit.Id)
                        |> Seq.tryPick (fun (id, other) ->
                            let occupied = footprint other.Size other.Cell |> Set.ofArray
                            if Set.intersect destinationCells occupied |> Set.isEmpty then None
                            else
                                let otherMovesAway =
                                    validatedMoves
                                    |> List.exists (fun candidate ->
                                        candidate.Unit.Id = id
                                        && (Set.intersect
                                                (footprint candidate.Unit.Size candidate.Destination |> Set.ofArray)
                                                destinationCells
                                            |> Set.isEmpty))
                                if otherMovesAway then None
                                else
                                    Set.intersect destinationCells occupied
                                    |> Set.toList
                                    |> List.sort
                                    |> List.tryHead
                                    |> Option.map (fun address ->
                                        move.Unit.Id, OccupiedCell(address, id))))
            |> Map.ofList
        let resolvedMoves =
            validatedMoves |> List.filter (fun move -> not (Map.containsKey move.Unit.Id conflicts))
        let conflictEvents =
            validatedMoves |> List.choose (fun move ->
                Map.tryFind move.Unit.Id conflicts
                |> Option.map (fun blocker -> MovementRejected(move.Unit.Id, move.Destination, blocker)))
        let resolveEvents = validationEvents @ conflictEvents
        let resolvedState =
            { validatedState with
                MovementCreditsMillimeters =
                    conflictEvents
                    |> List.fold (fun credits event ->
                        match event with
                        | MovementRejected(id, _, _) -> Map.add id 0 credits
                        | _ -> credits) validatedState.MovementCreditsMillimeters }
        let resolveCheckpoint = checkpoint ResolvePhase resolvedState resolveEvents

        let movedState, movementEvents =
            ((resolvedState, []), resolvedMoves)
            ||> List.fold (fun (current, events) move ->
                let nextScriptIndex =
                    if move.Unit.Controller = ScriptedController then
                        move.Unit.ScriptIndex + 1
                    else
                        move.Unit.ScriptIndex
                let moved =
                    { move.Unit with
                        Cell = move.Destination
                        ScriptIndex = nextScriptIndex }
                let routes =
                    match Map.tryFind move.Unit.Id current.PlannedRoutes with
                    | Some(_ :: remaining) when not (List.isEmpty remaining) -> Map.add move.Unit.Id remaining current.PlannedRoutes
                    | Some _ -> Map.remove move.Unit.Id current.PlannedRoutes
                    | None -> current.PlannedRoutes
                let next =
                    { current with
                        Units = Map.add moved.Id moved current.Units
                        MovementCreditsMillimeters = Map.add moved.Id (move.Available - move.Required) current.MovementCreditsMillimeters
                        MovementProgress = Map.remove moved.Id current.MovementProgress
                        MovementIntents = Map.remove moved.Id current.MovementIntents
                        PlannedRoutes = routes }
                next, UnitMoved(moved.Id, move.Unit.Cell, move.Destination, stepDistance move.Unit.Cell move.Destination, move.Required) :: events)
        let preparing =
            validationEvents |> List.choose (function
                | MovementPrepared(id, origin, destination, progress, cost) -> Some(id, { Origin = origin; Destination = destination; ProgressMillimeters = progress; CostMillimeters = cost })
                | _ -> None)
        let afterProgress =
            { movedState with
                MovementProgress =
                    preparing |> List.fold (fun progress (id, value) -> Map.add id value progress) movedState.MovementProgress }

        let attackIntents =
            collected |> List.choose (function AttackIntent(a, t, p) -> Some(a, t, p) | _ -> None)
        let recovery =
            state.Engagements |> Map.map (fun _ engagement ->
                { engagement with RecoveryTicksRemaining = max 0 (engagement.RecoveryTicksRemaining - 1) })
        let attacks, attackEvents =
            ((Map.empty, []), attackIntents)
            ||> List.fold (fun (accepted, events) (attacker, target, profile) ->
                let remaining =
                    Map.tryFind attacker.Id recovery
                    |> Option.map _.RecoveryTicksRemaining
                    |> Option.defaultValue 0
                if remaining > 0 then accepted, AttackRecovering(attacker.Id, remaining) :: events
                else Map.add attacker.Id (target, profile) accepted,
                     AttackResolved(attacker.Id, UnitTarget target.Id, profile.Delivery, profile.Damage) :: events)
        let damageByTarget =
            attacks |> Map.toList |> List.map (fun (_, (target, profile)) -> target.Id, profile.Damage)
            |> List.groupBy fst |> List.map (fun (id, values) -> id, values |> List.sumBy snd) |> Map.ofList
        let damagedUnits =
            afterProgress.Units |> Map.map (fun id unit ->
                Map.tryFind id damageByTarget
                |> Option.map (fun damage -> { unit with Health = max 0 (unit.Health - damage) })
                |> Option.defaultValue unit)
        let engagements =
            attacks |> Map.fold (fun current attackerId (target, profile) ->
                Map.add attackerId
                    { Target = UnitTarget target.Id; Profile = profile; RecoveryTicksRemaining = profile.RecoveryTicks }
                    current) recovery
        let committed =
            { afterProgress with Tick = state.Tick + 1; Units = damagedUnits; Engagements = engagements }
        let allEvents = resolveEvents @ List.rev movementEvents @ List.rev attackEvents
        let commitCheckpoint = { Tick = committed.Tick; Phase = CommitPhase; State = committed; Events = allEvents }
        { State = committed
          Events = allEvents
          Checkpoints = [ collectCheckpoint; validateCheckpoint; resolveCheckpoint; commitCheckpoint ]
          Counters =
            { LosSamples = losSamples
              CombatResolutions =
                allEvents
                |> List.sumBy (function AttackResolved _ -> 1 | _ -> 0) } }

    let private phaseCode phase =
        match phase with
        | CollectPhase -> 0
        | ValidatePhase -> 1
        | ResolvePhase -> 2
        | CommitPhase -> 3

    let private eventCode event =
        match event with
        | MovementPrepared _ -> 0
        | UnitMoved _ -> 1
        | MovementRejected(_, _, DestinationConflict _) -> 2
        | MovementRejected(_, _, CrossingConflict _) -> 3
        | MovementRejected _ -> 4
        | UnitHeld _ -> 5
        | AttackRecovering _ -> 6
        | AttackResolved _ -> 7

    /// Canonical diagnostic bytes for locating the first divergent map-scale phase.
    let checkpointBytes (checkpoint: MapScaleCheckpoint) =
        let int32 value = CanonicalEncoding.int32LittleEndian value
        let units =
            checkpoint.State.Units
            |> Map.toList
            |> List.collect (fun (id, unit) ->
                [ int32 id; int32 unit.Cell.Col; int32 unit.Cell.Row
                  int32 unit.Size; int32 unit.Health; int32 unit.ScriptIndex ])
        let credits =
            checkpoint.State.MovementCreditsMillimeters
            |> Map.toList
            |> List.collect (fun (id, credit) -> [ int32 id; int32 credit ])
        let events =
            checkpoint.Events
            |> List.collect (fun event ->
                let identity =
                    match event with
                    | MovementPrepared(id, _, _, _, _)
                    | UnitMoved(id, _, _, _, _)
                    | MovementRejected(id, _, _)
                    | UnitHeld(id, _)
                    | AttackRecovering(id, _)
                    | AttackResolved(id, _, _, _) -> id
                [ int32 (eventCode event); int32 identity ])
        CanonicalEncoding.concatenate
            ([ int32 checkpoint.Tick
               int32 (phaseCode checkpoint.Phase)
               int32 checkpoint.State.Units.Count ]
             @ units
             @ [ int32 checkpoint.State.MovementCreditsMillimeters.Count ]
             @ credits
             @ [ int32 checkpoint.Events.Length ]
             @ events)

    /// Returns the earliest phase/byte mismatch between two map-scale runs.
    let firstCheckpointDivergence
        (expected: MapScaleCheckpoint list)
        (actual: MapScaleCheckpoint list)
        =
        List.zip expected actual
        |> List.tryPick (fun (expectedCheckpoint, actualCheckpoint) ->
            let expectedBytes = checkpointBytes expectedCheckpoint
            let actualBytes = checkpointBytes actualCheckpoint
            if expectedBytes = actualBytes then None
            else
                let limit = min expectedBytes.Length actualBytes.Length
                let offset =
                    [ 0 .. limit - 1 ]
                    |> List.tryFind (fun index -> expectedBytes[index] <> actualBytes[index])
                    |> Option.defaultValue limit
                Some
                    { Tick = expectedCheckpoint.Tick
                      Phase = expectedCheckpoint.Phase
                      ByteOffset = int32 offset
                      Expected = if offset < expectedBytes.Length then expectedBytes[offset] else 0uy
                      Actual = if offset < actualBytes.Length then actualBytes[offset] else 0uy })
