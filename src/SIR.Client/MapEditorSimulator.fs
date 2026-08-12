namespace SIR.Client

open System
open FS.GG.Game.Core
open SIR.Domain

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
      InitialRevision: MapRevision
      ActivationTicks: Map<int32, int32>
      ReconciliationMessage: string option
      RuntimeMap: MapDefinition
      KernelState: SIR.Simulation.MapScaleState
      Tick: int32
      IsRunning: bool
      LastEvents: string list
      LastCombatEvents: SimulatorCombatEvent list
      LastCheckpoints: SIR.Simulation.MapScaleCheckpoint list
      AttackRecoveryTicks: Map<int32, int32>
      MovementCreditsMillimeters: Map<int32, int32>
      MovementProgress: Map<int32, SimulatorMovementProgress>
      PresentationPositions: Map<int32, float * float>
      MovementIntents: Map<int32, MapDirection>
      PlannedRoutes: Map<int32, EditorCellAddress list>
      PreviewDestination: EditorCellAddress option }

type SimulatorAction =
    | ToggleSimulatorRun
    | StepSimulator
    | AdvanceRunningSimulatorTick
    | MoveSimulatorUnit of MapDirection
    | SetSimulatorController of MapController
    | SetSimulatorScript of string
    | MoveSimulatorPreview of columnDelta: int32 * rowDelta: int32
    | ResetSimulatorPreviewToOrigin
    | ResetSimulatorPreview
    | CommitSimulatorPreview

[<RequireQualifiedAccess>]
module MapEditorSimulator =
    let TicksPerSecond = SIR.Simulation.MapScale.TicksPerSecond
    let CellMillimeters = SIR.Simulation.MapScale.CellMillimeters
    let DiagonalCellMillimeters = SIR.Simulation.MapScale.DiagonalCellMillimeters
    let MaximumMovementCreditMillimeters = SIR.Simulation.MapScale.MaximumMovementCreditMillimeters

    [<Literal>]
    let PerspectiveUnavailableReason =
        "Player perspective is unavailable: no accepted disclosure-filtered projection exists for editor drafts."

    [<Literal>]
    let VisibilityUnavailableReason =
        "Visibility overlays are unavailable until shared-kernel perception rules are accepted."

    let private toCell address =
        SIR.Simulation.MapScale.cell address.CellColumn address.CellRow

    let private fromCell (address: Cell) =
        { CellColumn = address.Col; CellRow = address.Row }

    let private sideCode (side: MapSide) =
        match side with
        | Blue -> 0
        | Red -> 1
        | NeutralSide -> 2

    let private controllerToKernel (controller: MapController) =
        match controller with
        | Manual -> SIR.Simulation.ManualController
        | Scripted -> SIR.Simulation.ScriptedController
        | General -> SIR.Simulation.GeneralController

    let private terrainToKernel terrain =
        match terrain with
        | Open -> SIR.Simulation.OpenTerrain
        | Rough -> SIR.Simulation.RoughTerrain
        | Blocked -> SIR.Simulation.MapScaleTerrain.BlockedTerrain
        | Objective -> SIR.Simulation.ObjectiveTerrain

    let private edgeToKernel ((kind: MapEdgeKind), isOpen) =
        match kind with
        | MapEdgeKind.Wall -> SIR.Simulation.WallEdge
        | MapEdgeKind.Door -> SIR.Simulation.DoorEdge isOpen
        | MapEdgeKind.Window -> SIR.Simulation.WindowEdge

    let private edgeDirectionToKernel direction =
        match direction with
        | SIR.Client.EastEdge -> SIR.Simulation.EastEdge
        | SIR.Client.SouthEdge -> SIR.Simulation.SouthEdge

    let private unitToKernel (unit: EditorUnit) : SIR.Simulation.MapScaleUnit =
        { Id = unit.Id
          Side = sideCode unit.Side
          ClassId = unit.ClassId
          Cell = SIR.Simulation.MapScale.cell unit.Column unit.Row
          Size = unit.Size
          Health = unit.Health
          Controller = controllerToKernel unit.Controller
          Script = unit.Script
          ScriptIndex = int32 unit.ScriptIndex
          BodyFacing = unit.BodyFacing
          AttentionDirection = unit.AttentionDirection }

    let private initialKernel (map: MapDefinition) : SIR.Simulation.MapScaleState =
        { Tick = 0
          Board =
            { Width = map.Width
              Height = map.Height
              Terrain =
                map.Terrain
                |> Map.toList
                |> List.map (fun ((column, row), terrain) ->
                    SIR.Simulation.MapScale.cell column row, terrainToKernel terrain)
                |> Map.ofList
              Edges =
                map.Edges
                |> Map.toList
                |> List.map (fun ((column, row, direction), edge) ->
                    (column, row, edgeDirectionToKernel direction), edgeToKernel edge)
                |> Map.ofList }
          Units = map.Units |> Map.map (fun _ -> unitToKernel)
          MovementCreditsMillimeters = Map.empty
          MovementProgress = Map.empty
          MovementIntents = Map.empty
          PlannedRoutes = Map.empty
          Engagements = Map.empty }

    let private syncMapFromKernel (map: MapDefinition) (kernel: SIR.Simulation.MapScaleState) =
        let units =
            map.Units
            |> Map.map (fun id editor ->
                match Map.tryFind id kernel.Units with
                | None -> editor
                | Some (unit: SIR.Simulation.MapScaleUnit) ->
                    { editor with
                        Column = unit.Cell.Col
                        Row = unit.Cell.Row
                        Health = unit.Health
                        ScriptIndex = int unit.ScriptIndex
                        BodyFacing = unit.BodyFacing
                        AttentionDirection = unit.AttentionDirection })
        { map with Units = units }

    let private syncKernelConfiguration (map: MapDefinition) (kernel: SIR.Simulation.MapScaleState) : SIR.Simulation.MapScaleState =
        { kernel with
            Board = (initialKernel map).Board
            Units =
                map.Units
                |> Map.map (fun id editor ->
                    let configured = unitToKernel editor
                    match Map.tryFind id kernel.Units with
                    | Some existing ->
                        { configured with
                            Cell = existing.Cell
                            Health = existing.Health }
                    | None -> configured) }

    let private fromProgress (progress: SIR.Simulation.MovementProgress) : SimulatorMovementProgress =
        { Origin = fromCell progress.Origin
          Destination = fromCell progress.Destination
          ProgressMillimeters = progress.ProgressMillimeters
          CostMillimeters = progress.CostMillimeters }

    let private collisionFromKernel (blocker: SIR.Simulation.MovementBlocker) =
        match blocker with
        | SIR.Simulation.OutsideBoard address -> OutsideMap(fromCell address)
        | SIR.Simulation.BlockedTerrainCollision address -> BlockedTerrainAt(fromCell address)
        | SIR.Simulation.BlockingEdge(origin, destination) -> BlockingEdgeAt(fromCell origin, fromCell destination)
        | SIR.Simulation.OccupiedCell(address, id) -> OccupiedAt(fromCell address, id)
        | SIR.Simulation.DestinationConflict address -> OccupiedAt(fromCell address, -1)
        | SIR.Simulation.CrossingConflict id -> OccupiedAt({ CellColumn = 0; CellRow = 0 }, id)

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

    let private deliveryLabel delivery =
        match delivery with
        | MeleeDelivery -> "melee"
        | ProjectileDelivery -> "ranged"
        | LobbedAreaDelivery -> "lobbed area"
        | SpellAreaDelivery -> "spell area"

    let private deliveryFromKernel delivery =
        match delivery with
        | SIR.Simulation.MeleeDelivery -> MeleeDelivery
        | SIR.Simulation.ProjectileDelivery -> ProjectileDelivery
        | SIR.Simulation.LobbedAreaDelivery -> LobbedAreaDelivery
        | SIR.Simulation.SpellAreaDelivery -> SpellAreaDelivery

    let private shapeFromKernel shape =
        match shape with
        | SIR.Simulation.BurstArea radius -> BurstArea radius
        | SIR.Simulation.ConeArea(range, angle) -> ConeArea(range, angle)
        | SIR.Simulation.RayArea(length, width) -> RayArea(length, width)
        | SIR.Simulation.RectangleArea(width, depth) -> RectangleArea(width, depth)

    let private eventSummary (event: SIR.Simulation.MapScaleEvent) =
        match event with
        | SIR.Simulation.MovementPrepared(id, _, _, progress, cost) ->
            "Unit " + string id + " prepares to move (" + string progress + "/" + string cost + " mm)."
        | SIR.Simulation.UnitMoved(id, _, _, distance, cost) ->
            "Unit " + string id + " moves by " + string distance + " mm, spending " + string cost + " mm of movement credit."
        | SIR.Simulation.MovementRejected(id, _, blocker) ->
            "Unit " + string id + " cannot move: " + string blocker + "."
        | SIR.Simulation.UnitHeld(id, reason) -> "Unit " + string id + " " + reason + "."
        | SIR.Simulation.AttackRecovering(id, ticks) ->
            "Unit " + string id + " recovers from its attack for " + string ticks + " more ticks."
        | SIR.Simulation.AttackResolved(source, SIR.Simulation.UnitTarget target, delivery, damage) ->
            "Unit " + string source + " makes a " + deliveryLabel (deliveryFromKernel delivery)
            + " attack against unit " + string target + " for " + string damage + " damage."
        | SIR.Simulation.AttackResolved(source, SIR.Simulation.AreaTarget _, delivery, damage) ->
            "Unit " + string source + " makes a " + deliveryLabel (deliveryFromKernel delivery)
            + " area attack for " + string damage + " damage."

    let attackProfileFor (unit: EditorUnit) =
        let profile = SIR.Simulation.MapScale.combatProfileFor unit.ClassId
        { Delivery = deliveryFromKernel profile.Delivery
          Range = profile.Range
          Damage = profile.Damage
          RecoveryTicks = profile.RecoveryTicks
          AreaShape = profile.AreaShape |> Option.map shapeFromKernel }

    let movementProfileFor (unit: EditorUnit) =
        let profile = SIR.Simulation.MapScale.movementProfileFor unit.ClassId
        { SpeedMillimetersPerSecond = profile.SpeedMillimetersPerSecond
          CellMillimeters = profile.CellMillimeters }

    let pathfind destination (unit: EditorUnit) (map: MapDefinition) =
        let kernel = initialKernel map
        SIR.Simulation.MapScale.tryFindPath kernel.Board kernel.Units (Map.find unit.Id kernel.Units) (toCell destination)
        |> Option.map (fun result -> result.Route |> List.map fromCell)

    let preview selectedUnitId destination handoff =
        let kernel =
            syncKernelConfiguration handoff.RuntimeMap handoff.KernelState
        selectedUnitId
        |> Option.bind (fun id ->
            Map.tryFind id kernel.Units
            |> Option.map (fun unit ->
                let result =
                    SIR.Simulation.MapScale.tryFindPath
                        kernel.Board
                        kernel.Units
                        unit
                        (toCell destination)
                let collision =
                    match result with
                    | Some _ -> RouteClear
                    | None ->
                        SIR.Simulation.MapScale.movementCollision
                            kernel.Board
                            kernel.Units
                            unit
                            (toCell destination)
                        |> Option.map collisionFromKernel
                        |> Option.defaultValue (NoPathTo destination)
                let route = result |> Option.map _.Route |> Option.defaultValue []
                { UnitId = id
                  Origin = fromCell unit.Cell
                  Destination = destination
                  Distance = int32 route.Length
                  DistanceMillimeters = result |> Option.map _.DistanceMillimeters |> Option.defaultValue 0
                  MovementCostMillimeters = result |> Option.map _.MovementCostMillimeters |> Option.defaultValue 0
                  Route = route |> List.map fromCell |> List.toArray
                  Collision = collision }))

    let private fromRevision (revision: MapRevision) =
        let map = revision.Document
        let kernel = initialKernel map
        { Revision = revision
          InitialRevision = revision
          ActivationTicks = Map.empty
          ReconciliationMessage = None
          RuntimeMap = map
          KernelState = kernel
          Tick = 0
          IsRunning = false
          LastEvents = []
          LastCombatEvents = []
          LastCheckpoints = []
          AttackRecoveryTicks = Map.empty
          MovementCreditsMillimeters = Map.empty
          MovementProgress = Map.empty
          PresentationPositions =
            map.Units
            |> Map.map (fun _ unit -> float unit.Column, float unit.Row)
          MovementIntents = Map.empty
          PlannedRoutes = Map.empty
          PreviewDestination = None }

    let tryHandoff (state: MapEditorState) =
        let issues =
            MapEditor.validationIssues state.Revision.Document
            |> Array.filter (fun issue -> issue.Code <> "EDGE-GAP")
        if not (Array.isEmpty issues) then
            issues |> Array.map (fun issue -> issue.Code + ": " + issue.Message)
            |> String.concat " " |> Error
        else
            Ok(fromRevision state.Revision)

    /// Restores the disposable runtime from the revision pinned by its
    /// existing handoff. The mutable editor draft is deliberately absent from
    /// this boundary.
    let reset (handoff: SimulatorHandoff) =
        fromRevision handoff.Revision

    let isBehindDraft (state: MapEditorState) handoff =
        handoff.Revision.Digest <> state.Revision.Digest

    /// Reconciles a valid authored revision with live simulation. Additions retain the live kernel;
    /// all geometry or existing-unit changes restart from the deterministic initial revision.
    let reconcile (state: MapEditorState) (handoff: SimulatorHandoff) =
        let next = state.Revision
        let before = handoff.Revision.Document
        let after = next.Document
        let unchangedGeometry =
            before.Width = after.Width && before.Height = after.Height
        let unchangedTerrain = before.Terrain = after.Terrain
        let unchangedTopology = before.Edges = after.Edges
        let retained =
            before.Units
            |> Map.forall (fun id unit -> Map.tryFind id after.Units = Some unit)
        if unchangedGeometry && unchangedTerrain && unchangedTopology && retained then
            let introduced =
                after.Units
                |> Map.filter (fun id _ -> not (Map.containsKey id before.Units))
            if Map.isEmpty introduced then { handoff with Revision = next; ReconciliationMessage = None }
            else
                let runtime = { handoff.RuntimeMap with Units = Map.fold (fun units id unit -> Map.add id unit units) handoff.RuntimeMap.Units introduced }
                let kernel =
                    { handoff.KernelState with
                        Units = Map.fold (fun units id unit -> Map.add id (unitToKernel unit) units) handoff.KernelState.Units introduced }
                { handoff with
                    Revision = next
                    RuntimeMap = runtime
                    KernelState = kernel
                    PresentationPositions = Map.fold (fun positions id unit -> Map.add id (float unit.Column, float unit.Row) positions) handoff.PresentationPositions introduced
                    ActivationTicks = Map.fold (fun ticks id _ -> Map.add id handoff.Tick ticks) handoff.ActivationTicks introduced
                    ReconciliationMessage = Some ("Added " + string (Map.count introduced) + " unit(s) at tick " + string handoff.Tick + ".") }
        else
            let reason =
                if not unchangedGeometry then
                    "map geometry changed"
                elif not unchangedTerrain then
                    "terrain changed"
                elif not unchangedTopology then
                    "edge topology changed"
                else
                    before.Units
                    |> Map.toSeq
                    |> Seq.tryPick (fun (id, unit) ->
                        match Map.tryFind id after.Units with
                        | None -> Some("existing unit " + string id + " was removed")
                        | Some updated when updated <> unit ->
                            Some("existing unit " + string id + " changed")
                        | _ -> None)
                    |> Option.defaultValue "an incompatible authored value changed"
            { fromRevision next with
                InitialRevision = next
                ReconciliationMessage = Some("Simulation restarted at tick 0 because " + reason + ".") }

    let perspectivePreview (projection: RenderFrame option) =
        match projection with
        | Some frame when frame.Disclosure = PerspectiveDisclosure -> AcceptedPerspectiveProjection frame
        | _ -> PerspectivePreviewUnavailable PerspectiveUnavailableReason

    let visibilityOverlays = VisibilityOverlaysUnavailable VisibilityUnavailableReason

    let private step handoff =
        let configured = syncKernelConfiguration handoff.RuntimeMap handoff.KernelState
        let result = SIR.Simulation.MapScale.tick configured
        let map = syncMapFromKernel handoff.RuntimeMap result.State
        let combat =
            result.Events
            |> List.choose (function
                | SIR.Simulation.AttackResolved(source, SIR.Simulation.UnitTarget target, delivery, damage) as event ->
                    Some
                        { Tick = result.State.Tick
                          SourceUnitId = source
                          Target = UnitCombatTarget target
                          Delivery = deliveryFromKernel delivery
                          Damage = damage
                          Summary = eventSummary event }
                | SIR.Simulation.AttackResolved(source, SIR.Simulation.AreaTarget(origin, shape), delivery, damage) as event ->
                    Some
                        { Tick = result.State.Tick
                          SourceUnitId = source
                          Target = AreaCombatTarget(fromCell origin, shapeFromKernel shape)
                          Delivery = deliveryFromKernel delivery
                          Damage = damage
                          Summary = eventSummary event }
                | _ -> None)
        let recent =
            handoff.LastCombatEvents
            |> List.filter (fun event -> result.State.Tick - event.Tick <= 5)
            |> fun prior -> prior @ combat
        let progress = result.State.MovementProgress |> Map.map (fun _ -> fromProgress)
        let approach current target =
            current + max -0.3 (min 0.3 (target - current))
        let presentationPositions =
            result.State.Units
            |> Map.map (fun id unit ->
                let current =
                    Map.tryFind id handoff.PresentationPositions
                    |> Option.defaultValue (float unit.Cell.Col, float unit.Cell.Row)
                let offset =
                    Map.tryFind id progress
                    |> Option.map (fun movement ->
                        let fraction =
                            float movement.ProgressMillimeters
                            / float (max 1 movement.CostMillimeters)
                            |> min 1.0
                        float (movement.Destination.CellColumn - movement.Origin.CellColumn) * fraction,
                        float (movement.Destination.CellRow - movement.Origin.CellRow) * fraction)
                    |> Option.defaultValue (0.0, 0.0)
                let targetColumn = float unit.Cell.Col + fst offset
                let targetRow = float unit.Cell.Row + snd offset
                approach (fst current) targetColumn,
                approach (snd current) targetRow)
        { handoff with
            RuntimeMap = map
            KernelState = result.State
            Tick = result.State.Tick
            LastEvents = result.Events |> List.map eventSummary
            LastCombatEvents = recent
            LastCheckpoints = result.Checkpoints
            AttackRecoveryTicks =
                result.State.Engagements
                |> Map.map (fun _ engagement -> engagement.RecoveryTicksRemaining)
            MovementCreditsMillimeters = result.State.MovementCreditsMillimeters
            MovementProgress = progress
            PresentationPositions = presentationPositions
            MovementIntents = result.State.MovementIntents
            PlannedRoutes =
                result.State.PlannedRoutes
                |> Map.map (fun _ route -> route |> List.map fromCell)
            PreviewDestination = None }

    /// Reconstructs actual simulation state at a timeline tick from its pinned initial revision.
    let seek tick (handoff: SimulatorHandoff) =
        let target = max 0 tick
        let baseline = fromRevision handoff.InitialRevision
        let activate atTick current =
            handoff.ActivationTicks
            |> Map.fold (fun state id activation ->
                if activation = atTick then
                    match Map.tryFind id handoff.Revision.Document.Units with
                    | Some unit ->
                        { state with
                            RuntimeMap = { state.RuntimeMap with Units = Map.add id unit state.RuntimeMap.Units }
                            KernelState = { state.KernelState with Units = Map.add id (unitToKernel unit) state.KernelState.Units }
                            PresentationPositions = Map.add id (float unit.Column, float unit.Row) state.PresentationPositions }
                    | None -> state
                else state) current
        let activatedBaseline = activate 0 baseline
        [ 1 .. target ]
        |> List.fold (fun current nextTick -> current |> step |> activate nextTick) activatedBaseline
        |> fun replayed ->
            { replayed with
                Revision = handoff.Revision
                InitialRevision = handoff.InitialRevision
                ActivationTicks = handoff.ActivationTicks
                ReconciliationMessage = handoff.ReconciliationMessage
                IsRunning = handoff.IsRunning }

    let update action selectedUnitId handoff =
        let updateSelected transform =
            selectedUnitId
            |> Option.bind (fun id -> Map.tryFind id handoff.RuntimeMap.Units)
            |> Option.map (fun unit ->
                let map =
                    { handoff.RuntimeMap with
                        Units = Map.add unit.Id (transform unit) handoff.RuntimeMap.Units }
                { handoff with
                    RuntimeMap = map
                    KernelState = syncKernelConfiguration map handoff.KernelState
                    PreviewDestination = None })
            |> Option.defaultValue handoff
        match action with
        | ToggleSimulatorRun ->
            { handoff with
                IsRunning = not handoff.IsRunning
                PreviewDestination =
                    if handoff.IsRunning then handoff.PreviewDestination else None }
        | AdvanceRunningSimulatorTick when handoff.IsRunning -> step handoff
        | AdvanceRunningSimulatorTick -> handoff
        | StepSimulator
        | MoveSimulatorUnit _
        | SetSimulatorController _
        | SetSimulatorScript _
        | MoveSimulatorPreview _
        | ResetSimulatorPreviewToOrigin
        | ResetSimulatorPreview
        | CommitSimulatorPreview when handoff.IsRunning ->
            handoff
        | StepSimulator -> step handoff
        | MoveSimulatorUnit direction ->
            selectedUnitId
            |> Option.bind (fun id -> Map.tryFind id handoff.RuntimeMap.Units)
            |> Option.map (fun unit ->
                let kernel =
                    { handoff.KernelState with
                        MovementIntents = Map.add unit.Id direction handoff.KernelState.MovementIntents
                        MovementProgress = Map.remove unit.Id handoff.KernelState.MovementProgress
                        PlannedRoutes = Map.remove unit.Id handoff.KernelState.PlannedRoutes }
                { handoff with
                    KernelState = kernel
                    MovementIntents = kernel.MovementIntents
                    MovementProgress = Map.remove unit.Id handoff.MovementProgress
                    PlannedRoutes = Map.remove unit.Id handoff.PlannedRoutes
                    LastEvents =
                        [ "Unit " + string unit.Id + " receives movement intent "
                          + directionCode direction + "; advance simulation time to resolve it." ]
                    LastCombatEvents = []
                    PreviewDestination = None })
            |> Option.defaultValue handoff
        | SetSimulatorController controller ->
            updateSelected (fun unit -> { unit with Controller = controller })
        | SetSimulatorScript text ->
            match MapEditor.parseScript text with
            | Ok script -> updateSelected (fun unit -> { unit with Script = script; ScriptIndex = 0 })
            | Error error -> { handoff with LastEvents = [ error ]; LastCombatEvents = [] }
        | MoveSimulatorPreview(columnDelta, rowDelta) ->
            let origin =
                handoff.PreviewDestination
                |> Option.orElseWith (fun () ->
                    selectedUnitId
                    |> Option.bind (fun id -> Map.tryFind id handoff.RuntimeMap.Units)
                    |> Option.map (fun unit ->
                        { CellColumn = unit.Column; CellRow = unit.Row }))
            { handoff with
                PreviewDestination =
                    origin |> Option.map (fun p ->
                        { CellColumn = p.CellColumn + columnDelta
                          CellRow = p.CellRow + rowDelta }) }
        | ResetSimulatorPreviewToOrigin ->
            { handoff with
                PreviewDestination =
                    selectedUnitId
                    |> Option.bind (fun id ->
                        Map.tryFind id handoff.RuntimeMap.Units)
                    |> Option.map (fun unit ->
                        { CellColumn = unit.Column
                          CellRow = unit.Row }) }
        | ResetSimulatorPreview -> { handoff with PreviewDestination = None }
        | CommitSimulatorPreview ->
            match handoff.PreviewDestination |> Option.bind (fun destination -> preview selectedUnitId destination handoff) with
            | Some route when route.Collision = RouteClear && route.Route.Length > 0 ->
                let sharedRoute = route.Route |> Array.map toCell |> Array.toList
                let kernel =
                    { handoff.KernelState with
                        PlannedRoutes = Map.add route.UnitId sharedRoute handoff.KernelState.PlannedRoutes
                        MovementIntents = Map.remove route.UnitId handoff.KernelState.MovementIntents
                        MovementProgress = Map.remove route.UnitId handoff.KernelState.MovementProgress }
                { handoff with
                    KernelState = kernel
                    PlannedRoutes = Map.add route.UnitId (Array.toList route.Route) handoff.PlannedRoutes
                    MovementIntents = Map.remove route.UnitId handoff.MovementIntents
                    MovementProgress = Map.remove route.UnitId handoff.MovementProgress
                    LastEvents =
                        [ "Unit " + string route.UnitId + " accepts a " + string route.Distance
                          + "-step, " + string route.DistanceMillimeters + " mm path costing "
                          + string route.MovementCostMillimeters
                          + " mm of movement credit; advance simulation time to move." ]
                    LastCombatEvents = []
                    PreviewDestination = None }
            | Some route ->
                { handoff with
                    LastEvents = [ "Preview route rejected: " + string route.Collision + "." ]
                    LastCombatEvents = [] }
            | None -> handoff

    let presentationOffsets handoff =
        handoff.PresentationPositions
        |> Map.fold (fun offsets id (column, row) ->
            match Map.tryFind id handoff.RuntimeMap.Units with
            | Some unit ->
                let offset =
                    match unit.Controller, Map.tryFind id handoff.MovementProgress with
                    | Manual, Some progress ->
                        let fraction =
                            float progress.ProgressMillimeters
                            / float (max 1 progress.CostMillimeters)
                            |> min 1.0
                        float (progress.Destination.CellColumn - progress.Origin.CellColumn) * fraction,
                        float (progress.Destination.CellRow - progress.Origin.CellRow) * fraction
                    | _ ->
                        column - float unit.Column,
                        row - float unit.Row
                Map.add id
                    offset
                    offsets
            | None -> offsets) Map.empty

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
                    [| float origin.CellColumn + 0.5; float origin.CellRow + 0.5 |]
                    (route |> Array.collect (fun p ->
                        [| float p.CellColumn + 0.5; float p.CellRow + 0.5 |]))
              Label = Disclosed label }
        let previewOverlay =
            handoff.PreviewDestination
            |> Option.bind (fun destination -> preview selectedUnitId destination handoff)
            |> Option.map (fun route ->
                routeOverlay
                    (if route.Collision = RouteClear then "route-preview-clear" else "route-preview-collision")
                    route.UnitId route.Origin route.Route
                    ("Route " + string route.Distance + " steps; "
                     + (if route.Collision = RouteClear then "clear" else "collision: " + string route.Collision)))
        let plannedOverlay =
            selectedUnitId
            |> Option.bind (fun unitId ->
                Map.tryFind unitId handoff.PlannedRoutes
                |> Option.bind (fun route ->
                    Map.tryFind unitId handoff.RuntimeMap.Units
                    |> Option.map (fun unit ->
                        routeOverlay "route-planned" unitId
                            { CellColumn = unit.Column; CellRow = unit.Row }
                            (List.toArray route)
                            ("Queued path with " + string route.Length + " remaining steps."))))
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
                    | UnitCombatTarget id -> Disclosed id
                    | AreaCombatTarget _ -> NotApplicable
                  Summary = Disclosed combat.Summary })
            |> List.toArray
        let combatSummaries = handoff.LastCombatEvents |> List.map _.Summary |> Set.ofList
        { baseFrame with
            Overlays = previewOverlay |> Option.orElse plannedOverlay |> Option.toArray
            Events =
                Array.append
                    (baseFrame.Events
                     |> Array.filter (fun event ->
                        match event.Summary with
                        | Disclosed summary -> not (Set.contains summary combatSummaries)
                        | _ -> true))
                    combatEvents }

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
