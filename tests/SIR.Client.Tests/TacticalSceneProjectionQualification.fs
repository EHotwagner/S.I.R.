module SIR.Client.TestsTacticalSceneProjectionQualification

open System
open System.Diagnostics
open SIR.Client
open SIR.Domain

let private require condition message =
    if not condition then failwith message

let private camera =
    { PanX = 18.0
      PanY = 24.0
      Zoom = 1.25 }

let private p95 (values: float array) =
    let sorted = Array.sort values
    sorted[min (sorted.Length - 1) (int (float sorted.Length * 0.95))]

let private milliseconds operation =
    let started = Stopwatch.GetTimestamp()
    operation () |> ignore
    float (Stopwatch.GetTimestamp() - started)
    * 1000.0
    / float Stopwatch.Frequency

let private denseMap () =
    let terrain =
        [ for row in 0 .. 39 do
              for column in 0 .. 39 do
                  let kind =
                      match (column + row) % 4 with
                      | 0 -> Open
                      | 1 -> Rough
                      | 2 -> Objective
                      | _ -> Blocked
                  yield (int32 column, int32 row), kind ]
        |> Map.ofList
    let units =
        [ 1 .. 200 ]
        |> List.map (fun id ->
            let unit =
                { Id = int32 id
                  Side = if id % 2 = 0 then Blue else Red
                  ClassId = if id % 2 = 0 then "rifleman" else "goblin"
                  Column = int32 ((id - 1) % 40)
                  Row = int32 ((id - 1) / 40)
                  Size = 1
                  Health = 12
                  HealthMaximum = 12
                  Controller = Manual
                  Script = []
                  ScriptIndex = 0
                  BodyFacing = North
                  AttentionDirection = East }
            unit.Id, unit)
        |> Map.ofList
    let regions =
        [ 1 .. 200 ]
        |> List.map (fun id ->
            int32 id,
            { Id = int32 id
              Geometry =
                RegionRectangle(
                    int32 ((id - 1) % 40),
                    int32 ((id - 1) / 40),
                    1,
                    1
                )
              Purpose = ObjectiveRegion
              Behavior = NoRegionBehavior })
        |> Map.ofList
    { MapEditor.initial.Map with
        Width = 40
        Height = 40
        Terrain = terrain
        Edges = Map.empty
        Units = units
        NextUnitId = 201
        Regions = regions
        NextRegionId = 201 }

let private inspection tick units events perspectiveHash : InspectionProjection =
    { Tick = tick
      BoardMinimumColumn = 0
      BoardMinimumRow = 0
      BoardMaximumColumn = 39
      BoardMaximumRow = 39
      Units = units
      Edges = []
      Events = events
      Checkpoints = []
      PerspectiveHash = perspectiveHash }

let private reviewModel
    kind
    source
    mode
    verification
    (projection: InspectionProjection)
    selectedUnit
    selectedEvent
    : Model
    =
    { Shell.init () with
        Source =
            Loaded
                { SourceName = source + ".sir"
                  SourceIdentity = source
                  EngineIdentity = "engine-qualification"
                  FinalTick = 200
                  Kind = kind }
        Mode = mode
        Verification = verification
        Playback =
            { CurrentTick = projection.Tick
              FinalTick = 200
              IsPlaying = false
              Speed = Normal }
        Selection =
            { Unit = selectedUnit
              Event = selectedEvent
              Formula = None }
        Inspection = Some projection }

let private ids projection =
    TacticalSceneProjection.primitiveIds projection
    |> Array.map ScenePrimitiveId.value

let run () =
    for expected in [ 100; 200 ] do
        let sample =
            ExperienceSamples.tryMap ("tactical-density-" + string expected)
            |> Option.defaultWith (fun () -> failwith "Production density sample is missing.")
        let editorSample = ExperienceSamples.editorState sample
        let loadedSample = MapEditor.initial |> MapEditor.update (LoadMapText sample.MapText)
        let simulator =
            ExperienceSamples.simulator sample
            |> Option.defaultWith (fun () -> failwith ("Production density sample failed validation: " + string expected))
        require
            (simulator.RuntimeMap.Units.Count = expected)
            ("Production density sample unit count drifted: expected " + string expected + ", actual " + string simulator.RuntimeMap.Units.Count + ", dimensions " + string editorSample.Map.Width + "x" + string editorSample.Map.Height + ", load " + string loadedSample.Validation + ", validation " + string editorSample.Validation + ", source " + sample.MapText.Substring(0, min 160 sample.MapText.Length).Replace("\n", "|"))
        require
            (simulator.RuntimeMap.Units
             |> Map.toSeq
             |> Seq.map (fun (_, unit) -> unit.Side)
             |> Set.ofSeq
             |> Set.count
             |> (=) 2)
            ("Production density sample lost opposing factions: " + string expected)
        let attacked =
            MapEditorSimulator.update StepSimulator editorSample.SelectedUnit simulator
        require
            (not (List.isEmpty attacked.LastCombatEvents))
            ("Production density sample did not produce current attacks: " + string expected)
        let routed =
            attacked
            |> MapEditorSimulator.update (MoveSimulatorPreview(0, -1)) editorSample.SelectedUnit
            |> MapEditorSimulator.update CommitSimulatorPreview editorSample.SelectedUnit
        require
            (routed.LastCombatEvents = attacked.LastCombatEvents
             && editorSample.SelectedUnit
                |> Option.exists (fun id -> Map.containsKey id routed.PlannedRoutes))
            ("Production density route commit erased simultaneous causal attacks: " + string expected)
    let initialEditor = MapEditor.initial
    let initialSimulator = MapEditorSimulator.tryHandoff initialEditor |> Result.defaultWith failwith
    let advancedSimulator = MapEditorSimulator.update StepSimulator initialEditor.SelectedUnit initialSimulator
    let addedId = initialEditor.Map.NextUnitId
    let addedUnit =
        { (initialEditor.Map.Units |> Map.toSeq |> Seq.head |> snd) with
            Id = addedId
            Column = 0
            Row = 0
            Controller = Scripted
            Script = [ East ] }
    let addedMap = { initialEditor.Map with Units = Map.add addedId addedUnit initialEditor.Map.Units; NextUnitId = addedId + 1 }
    let addedEditor =
        { initialEditor with
            Map = addedMap
            Revision = { initialEditor.Revision with Document = addedMap; Digest = "continuous-added" } }
    let reconciled = MapEditorSimulator.reconcile addedEditor advancedSimulator
    let beforeActivation = MapEditorSimulator.seek 0 reconciled
    let atActivation = MapEditorSimulator.seek reconciled.Tick reconciled
    let addedAtZero = MapEditorSimulator.reconcile addedEditor initialSimulator
    let soughtAtZero = MapEditorSimulator.seek 0 addedAtZero
    let terrainMap = { addedMap with Terrain = Map.add (0, 0) Rough addedMap.Terrain }
    let incompatibleEditor = { addedEditor with Map = terrainMap; Revision = { addedEditor.Revision with Document = terrainMap; Digest = "continuous-terrain" } }
    let restarted = MapEditorSimulator.reconcile incompatibleEditor reconciled
    let topologyMap = { addedMap with Edges = Map.add (0, 0, EastEdge) (Wall, false) addedMap.Edges }
    let topologyEditor = { addedEditor with Map = topologyMap; Revision = { addedEditor.Revision with Document = topologyMap; Digest = "continuous-topology" } }
    let topologyRestarted = MapEditorSimulator.reconcile topologyEditor reconciled
    let removedMap = { addedMap with Units = Map.remove addedId addedMap.Units }
    let removedEditor = { addedEditor with Map = removedMap; Revision = { addedEditor.Revision with Document = removedMap; Digest = "continuous-removal" } }
    let removalRestarted = MapEditorSimulator.reconcile removedEditor reconciled
    let mutatedMap =
        { addedMap with
            Units = Map.change addedId (Option.map (fun unit -> { unit with Health = unit.Health - 1 })) addedMap.Units }
    let mutatedEditor = { addedEditor with Map = mutatedMap; Revision = { addedEditor.Revision with Document = mutatedMap; Digest = "continuous-mutation" } }
    let mutationRestarted = MapEditorSimulator.reconcile mutatedEditor reconciled
    let geometryMap = { addedMap with Width = addedMap.Width + 1 }
    let geometryEditor = { addedEditor with Map = geometryMap; Revision = { addedEditor.Revision with Document = geometryMap; Digest = "continuous-geometry" } }
    let geometryRestarted = MapEditorSimulator.reconcile geometryEditor reconciled
    require
        (reconciled.Tick = advancedSimulator.Tick
         && not (Map.containsKey addedId beforeActivation.RuntimeMap.Units)
         && Map.containsKey addedId atActivation.RuntimeMap.Units
         && atActivation.KernelState = reconciled.KernelState
         && atActivation.MovementCreditsMillimeters = reconciled.MovementCreditsMillimeters
         && soughtAtZero.KernelState = addedAtZero.KernelState
         && restarted.Tick = 0
         && restarted.ReconciliationMessage = Some "Simulation restarted at tick 0 because terrain changed."
         && topologyRestarted.ReconciliationMessage = Some "Simulation restarted at tick 0 because edge topology changed."
         && removalRestarted.ReconciliationMessage = Some("Simulation restarted at tick 0 because existing unit " + string addedId + " was removed.")
         && mutationRestarted.ReconciliationMessage = Some("Simulation restarted at tick 0 because existing unit " + string addedId + " changed.")
         && geometryRestarted.ReconciliationMessage = Some "Simulation restarted at tick 0 because map geometry changed.")
        "Continuous simulation reconciliation, activation history, seek, or incompatible fallback diverged."
    let unitIds = initialEditor.Map.Units |> Map.toArray |> Array.map fst
    require (unitIds.Length >= 2) "Qualification fixture needs two authored units."
    let firstUnit, secondUnit = unitIds[0], unitIds[1]
    let regionId = 901
    let region =
        { Id = regionId
          Geometry = RegionRectangle(1, 1, 2, 2)
          Purpose = ObjectiveRegion
          Behavior = NoRegionBehavior }
    let editorMap =
        { initialEditor.Map with
            Regions = Map.add regionId region initialEditor.Map.Regions
            NextRegionId = regionId + 1 }
    let editor =
        { initialEditor with
            Map = editorMap
            Revision =
                { initialEditor.Revision with
                    Document = editorMap
                    Digest = "editor-selection-revision" }
            SelectedUnit = Some firstUnit
            SelectedUnits = Set.ofList [ firstUnit; secondUnit; 999 ]
            SelectedRegion = Some regionId }
    let workspace = MapEditorWorkspace.initial false
    let editorInput =
        { EditorState = editor
          EditorWorkspace = workspace
          EditorFocusedUnit = Some 999 }
    let editorBefore = editorInput
    let editorProjection = TacticalSceneProjection.editor editorInput
    require
        (editorInput = editorBefore
         && editorProjection.Owner = EditorScene
         && editorProjection.Disclosure.Source = SandboxDisclosure
         && editorProjection.Terrain.Length =
            int editor.Map.Width * int editor.Map.Height
         && editorProjection.Units.Length = editor.Map.Units.Count
         && editorProjection.Layers.Length = editor.Layers.Count
         && editorProjection.Selection.SelectedUnits = [| firstUnit; secondUnit |]
         && editorProjection.Selection.SelectedRegion = Some regionId
         && editorProjection.Selection.FocusedUnit.IsNone
         && (editorProjection.Selection.SelectedPrimitiveIds
             |> Array.map ScenePrimitiveId.value)
            = [| "unit:" + string firstUnit
                 ; "unit:" + string secondUnit
                 ; "region:" + string regionId |])
        "Editor projection mutated its input or omitted owned scene primitives."
    let staleEditorProjection =
        TacticalSceneProjection.editor
            { editorInput with
                EditorState =
                    { editor with
                        SelectedUnits = Set.singleton 999
                        SelectedUnit = Some 999
                        SelectedRegion = Some 999 } }
    require
        (staleEditorProjection.Selection.SelectedUnits.Length = 0
         && staleEditorProjection.Selection.SelectedRegion.IsNone
         && staleEditorProjection.Selection.SelectedPrimitiveIds.Length = 0)
        "Editor projection retained stale unit or region selections."

    let planningState =
        PlanningWorkspace.initial
            editor.Revision.Digest
            (editor.Map.Units |> Map.toSeq |> Seq.map snd)
    let selectedUnit = firstUnit
    let planningState =
        { planningState with
            Commands =
                [ { Id = "command-route"
                    UnitId = selectedUnit
                    EarliestTick = 3
                    Kind = PlannedRoute [| 1, 1; 2, 1; 3, 2 |] }
                  { Id = "command-engage"
                    UnitId = selectedUnit
                    EarliestTick = 4
                    Kind = PlannedEngagement(3, "rifle") }
                  { Id = "command-facing"
                    UnitId = selectedUnit
                    EarliestTick = 5
                    Kind = PlannedFacing East }
                  { Id = "command-attention"
                    UnitId = selectedUnit
                    EarliestTick = 6
                    Kind = PlannedAttention NorthEast }
                  { Id = "command-stance"
                    UnitId = selectedUnit
                    EarliestTick = 7
                    Kind = PlannedStance "crouched" }
                  { Id = "command-hold"
                    UnitId = selectedUnit
                    EarliestTick = 8
                    Kind = PlannedHold }
                  { Id = "command-sync"
                    UnitId = selectedUnit
                    EarliestTick = 9
                    Kind = PlannedSynchronization("phase-line", 12) } ]
            SelectedUnit = Some selectedUnit
            SelectedCommand = Some "command-route"
            Digest = "planning-qualification"
            Issues =
                [| { Code = "SIR.PLAN.QUALIFICATION"
                     CommandId = Some "command-engage"
                     UnitId = Some selectedUnit
                     Detail = "Qualification diagnostic" } |]
            Predicted =
                Some
                    { Revision = 0L
                      Label = IntentOnlyPreview
                      Disclosures = [| "Intent-only qualification" |] } }
    let planningInput =
        { PlanningMap = editor.Map
          PlanningState = planningState
          PlanningCamera = camera
          PlanningFocusedUnit = Some selectedUnit }
    let planningBefore = planningInput
    let planningProjection =
        TacticalSceneProjection.planning planningInput
    require
        (planningInput = planningBefore
         && planningProjection.Owner = PlanningScene
         && planningProjection.Routes.Length = 1
         && planningProjection.Annotations.Length = 8
         && planningProjection.Selection.SelectedUnits = [| selectedUnit |]
         && planningProjection.Selection.SelectedCommand = Some "command-route"
         && (planningProjection.Selection.SelectedPrimitiveIds
             |> Array.map ScenePrimitiveId.value)
            = [| "unit:" + string selectedUnit; "route:command-route" |]
         && (planningProjection.Units
             |> Array.find (fun unit ->
                 unit.Visual.Id = selectedUnit)
             |> _.Visual.ClassId
             |> UnitClassId.value)
            = (editor.Map.Units[selectedUnit]).ClassId
         && (planningProjection.Annotations
             |> Array.exists (fun annotation -> annotation.Kind = "validation"))
         && (planningProjection.Annotations
             |> Array.exists (fun annotation -> annotation.Kind = "prediction"))
         && (planningProjection.Units
             |> Array.forall (fun unit -> unit.Visual.Health = NotPresent))
         && (planningProjection.Units
             |> Array.find (fun unit -> unit.Visual.Id = selectedUnit)
             |> fun unit ->
                unit.Visual.StanceId = Disclosed "crouched"
                && unit.Visual.BodyHeading = Disclosed(HeadingRadians.ofDirection8 East)
                && (match unit.Visual.SecondaryHeading with
                    | Disclosed heading -> heading.Radians = HeadingRadians.ofDirection8 NorthEast
                    | _ -> false)
                && Set.ofArray unit.Visual.StatusIds
                   = Set.ofList [ "planning"; "hold"; "engagement"; "synchronization" ]))
        "Planning projection mutated input or disclosed editor/runtime-only unit state."
    let planningFacing =
        planningProjection.Annotations
        |> Array.find (fun annotation -> annotation.Kind = "facing")
    let planningAttention =
        planningProjection.Annotations
        |> Array.find (fun annotation -> annotation.Kind = "attention")
    require
        ((match planningFacing.Geometry with
          | Some(DirectionGeometry(_, _, heading, arc)) ->
              heading = (HeadingRadians.ofDirection8 East |> HeadingRadians.value)
              && arc.IsNone
          | _ -> false)
         && (match planningAttention.Geometry with
             | Some(DirectionGeometry(_, _, heading, arc)) ->
                 heading = (HeadingRadians.ofDirection8 NorthEast |> HeadingRadians.value)
                 && arc.IsNone
             | _ -> false)
         && planningProjection.Routes[0].MovementCost.IsNone
         && planningProjection.Routes[0].BlockerIds.Length = 0)
        "Planning authority did not preserve exact typed direction/route facts or invented unavailable cost/blockers."
    let annotationSelection =
        TacticalSceneProjection.planning
            { planningInput with
                PlanningState =
                    { planningState with
                        SelectedCommand = Some "command-engage" } }
    let annotationSelectionIds =
        annotationSelection.Selection.SelectedPrimitiveIds
        |> Array.map ScenePrimitiveId.value
    let expectedAnnotationSelectionIds =
        [| "unit:" + string selectedUnit; "plan-command:command-engage" |]
    require
        (annotationSelectionIds = expectedAnnotationSelectionIds)
        "Planning annotation selection did not resolve to its semantic primitive."
    let stalePlanning =
        TacticalSceneProjection.planning
            { planningInput with
                PlanningState =
                    { planningState with
                        SelectedUnit = Some 999
                        SelectedCommand = Some "missing-command" } }
    require
        (stalePlanning.Selection.SelectedUnits.Length = 0
         && stalePlanning.Selection.SelectedCommand.IsNone
         && stalePlanning.Selection.SelectedPrimitiveIds.Length = 0)
        "Planning projection retained stale unit or command selection."
    planningProjection.Routes[0].Points[0] <- 999.0
    require
        (match planningInput.PlanningState.Commands.Head.Kind with
         | PlannedRoute cells -> cells[0] = (1, 1)
         | _ -> false)
        "Planning projection shared mutable route geometry with authority state."

    let simulatorBase =
        MapEditorSimulator.tryHandoff editor
        |> Result.defaultWith failwith
    let pinnedUnit = simulatorBase.RuntimeMap.Units[firstUnit]
    let mutatedRuntimeMap =
        { simulatorBase.RuntimeMap with
            Units =
                simulatorBase.RuntimeMap.Units
                |> Map.add
                    firstUnit
                    { pinnedUnit with
                        Controller = General
                        Column = pinnedUnit.Column + 1
                        Script = [ North; East ]
                        ScriptIndex = 1 } }
    let runtimeMutation =
        { simulatorBase with
            RuntimeMap = mutatedRuntimeMap
            KernelState = { simulatorBase.KernelState with Tick = 17 }
            Tick = 17
            IsRunning = true
            LastEvents = [ "runtime event" ]
            AttackRecoveryTicks = Map.ofList [ firstUnit, 3 ]
            MovementCreditsMillimeters = Map.ofList [ firstUnit, 250 ]
            MovementProgress =
                Map.ofList
                    [ firstUnit,
                      { Origin =
                            { CellColumn = pinnedUnit.Column
                              CellRow = pinnedUnit.Row }
                        Destination =
                            { CellColumn = pinnedUnit.Column + 1
                              CellRow = pinnedUnit.Row }
                        ProgressMillimeters = 250
                        CostMillimeters = 500 } ]
            PresentationPositions =
                simulatorBase.PresentationPositions
                |> Map.add
                    firstUnit
                    (float pinnedUnit.Column + 0.5, float pinnedUnit.Row)
            MovementIntents = Map.ofList [ firstUnit, East ]
            PlannedRoutes =
                Map.ofList
                    [ firstUnit,
                      [ { CellColumn = pinnedUnit.Column + 1
                          CellRow = pinnedUnit.Row } ] ]
            PreviewDestination =
                Some
                    { CellColumn = pinnedUnit.Column + 2
                      CellRow = pinnedUnit.Row } }
    let resetRuntime = MapEditorSimulator.reset runtimeMutation
    require
        (resetRuntime = simulatorBase
         && obj.ReferenceEquals(
             resetRuntime.Revision.Document,
             simulatorBase.Revision.Document
         ))
        "Simulator reset did not restore the exact pinned immutable handoff baseline."
    let routeDestination unitId columnDelta =
        let unit = editor.Map.Units[unitId]
        { CellColumn = unit.Column + columnDelta
          CellRow = unit.Row }
    let simulator =
        { simulatorBase with
            Tick = 1
            LastEvents = [ "Accepted authoritative sample event" ]
            LastCombatEvents =
                [ { Tick = 1
                    SourceUnitId = firstUnit
                    Target = UnitCombatTarget secondUnit
                    Delivery = ProjectileDelivery
                    Damage = 2
                    Summary = "Committed projectile attack" } ]
            PlannedRoutes =
                Map.ofList
                    [ firstUnit, [ routeDestination firstUnit 1 ]
                      secondUnit, [ routeDestination secondUnit 1 ] ] }
    let simulatorInput =
        { SimulatorHandoff = simulator
          SimulatorSelectedUnit = Some firstUnit
          SimulatorCamera = camera
          SimulatorFocusedUnit = Some firstUnit }
    let simulatorBefore = simulatorInput
    let simulatorProjection =
        TacticalSceneProjection.simulator simulatorInput
    require
        (simulatorInput = simulatorBefore
         && simulatorProjection.Owner = SimulatorScene
         && simulatorProjection.RevisionIdentity = editor.Revision.Digest
         && simulatorProjection.Units.Length = editor.Map.Units.Count
         && simulatorProjection.Routes.Length = 2
         && (simulatorProjection.Routes
             |> Array.choose _.OwnerUnitId
             |> Set.ofArray
             |> Set.count
             |> (=) 2)
         && simulatorProjection.Effects
            |> Array.exists (fun effect -> effect.Kind = MovementEffect && effect.Lifecycle = PredictedEffect)
         && simulatorProjection.Effects
            |> Array.exists (fun effect -> effect.Kind = AttackEffect && effect.Lifecycle = CommittedEffect)
         && simulatorProjection.Effects
            |> Array.exists (fun effect -> effect.Lifecycle = AcceptedEffect))
        "Simulator projection mutated its handoff or crossed its revision boundary."
    let simulatorRouteSource = simulator.PlannedRoutes[firstUnit]
    simulatorProjection.Routes[0].Points[0] <- 999.0
    let repeatedSimulatorProjection =
        TacticalSceneProjection.simulator simulatorInput
    require
        (simulator.PlannedRoutes[firstUnit] = simulatorRouteSource
         && repeatedSimulatorProjection.Routes[0].Points[0] <> 999.0)
        "Simulator projection shared mutable route geometry with its handoff or a later projection."
    let runtimeUnit = simulator.RuntimeMap.Units[firstUnit]
    let runtimeMovement =
        { Origin =
            { CellColumn = runtimeUnit.Column
              CellRow = runtimeUnit.Row }
          Destination = routeDestination firstUnit 1
          ProgressMillimeters = 125
          CostMillimeters = 500 }
    let movingProjection =
        TacticalSceneProjection.simulator
            { simulatorInput with
                SimulatorHandoff =
                    { simulator with
                        PresentationPositions =
                            Map.add
                                firstUnit
                                (float runtimeUnit.Column + 0.25, float runtimeUnit.Row)
                                simulator.PresentationPositions
                        MovementProgress = Map.add firstUnit runtimeMovement Map.empty } }
    let movingUnit =
        movingProjection.Units
        |> Array.find (fun unit -> unit.Visual.Id = firstUnit)
    require
        (movingUnit.PresentationColumn = float runtimeUnit.Column + 0.25
         && movingUnit.PresentationRow = float runtimeUnit.Row
         && Array.contains "moving" movingUnit.Visual.StatusIds
         && movingUnit.Visual.StatusIds
            |> Array.exists (fun status -> status = "manual")
         && movingProjection.Disclosure.Source = SandboxDisclosure
         && movingProjection.Annotations
            |> Array.exists (fun annotation ->
                annotation.Kind = "simulator-state"
                && annotation.Text
                   = Disclosed(
                       "Unit " + string firstUnit + " · manual · moving · route-planned"
                   )))
        "Simulator movement, controller state, or sandbox disclosure did not cross the shared projection boundary."

    let projectedUnit : UnitProjection =
        { Id = firstUnit
          Side = "Blue"
          Column = 2
          Row = 3
          Health = 8
          HealthMaximum = 10
          MovementDirection = None
          BodyFacing = 0
          AttentionDirection = 2 }
    let projectedEvent : EventProjection =
        { Id = 44
          Tick = 7
          Source = "qualification"
          Lifecycle = AcceptedEvent
          Summary = "Visible event"
          SourceUnitId = Some firstUnit
          TargetUnitId = None }
    let validPerspective =
        let bounded =
            { inspection 7 [] [] (Some "bounded-perspective") with
                BoardMaximumColumn = 0
                BoardMaximumRow = 0 }
        reviewModel
            PerspectiveReplay
            "perspective-source"
            PerspectivePlayback
            PerspectiveReady
            bounded
            (Some firstUnit)
            (Some 44)
    let perspectiveInspection change =
        { validPerspective with
            Inspection = validPerspective.Inspection |> Option.map change }
    let faultEdge : EdgeProjection =
        { Id = "perspective-fault-edge"
          Kind = "wall"
          State = "closed"
          StartColumn = 0
          StartRow = 0
          EndColumn = 1
          EndRow = 0 }
    let faultCheckpoint : CheckpointProjection =
        { Tick = 7
          StateHash = "perspective-fault-state"
          EventHash = "perspective-fault-event" }
    let perspectiveFaults =
        [ "unit", perspectiveInspection (fun value -> { value with Units = [ projectedUnit ] })
          "edge", perspectiveInspection (fun value -> { value with Edges = [ faultEdge ] })
          "event", perspectiveInspection (fun value -> { value with Events = [ projectedEvent ] })
          "checkpoint", perspectiveInspection (fun value -> { value with Checkpoints = [ faultCheckpoint ] })
          "board-minimum-column", perspectiveInspection (fun value -> { value with BoardMinimumColumn = 1 })
          "board-minimum-row", perspectiveInspection (fun value -> { value with BoardMinimumRow = 1 })
          "board-maximum-column", perspectiveInspection (fun value -> { value with BoardMaximumColumn = 1 })
          "board-maximum-row", perspectiveInspection (fun value -> { value with BoardMaximumRow = 1 })
          "missing-perspective-hash", perspectiveInspection (fun value -> { value with PerspectiveHash = None })
          "replay-kind-mismatch",
              { validPerspective with
                  Source =
                    match validPerspective.Source with
                    | Loaded metadata -> Loaded { metadata with Kind = FullReplay }
                    | _ -> failwith "Expected perspective metadata." }
          "mode-mismatch", { validPerspective with Mode = VerifiedReplay }
          "verification-mismatch", { validPerspective with Verification = BrowserKernelVerified } ]
    let acceptedPerspectiveFaults =
        perspectiveFaults
        |> List.choose (fun (name, model) ->
            TacticalSceneProjection.acceptReview model
            |> Option.map (fun _ -> name))
    require
        (perspectiveFaults.Length = 12
         && List.isEmpty acceptedPerspectiveFaults)
        ("Review accepted independently faulted perspective owners: "
         + String.concat ", " acceptedPerspectiveFaults)
    let perspectiveAccepted =
        TacticalSceneProjection.acceptReview validPerspective
        |> Option.defaultWith (fun () ->
            failwith "Valid bounded perspective owner was rejected.")
    let perspectiveProjection =
        TacticalSceneProjection.review
            { AcceptedReview = perspectiveAccepted
              ReviewCamera = camera
              ReviewFocusedUnit = Some firstUnit }
    require
        (perspectiveProjection.Disclosure.PerspectiveFiltered
         && perspectiveProjection.Units.Length = 0
         && perspectiveProjection.Edges.Length = 0
         && perspectiveProjection.Annotations.Length = 1
         && perspectiveProjection.Annotations[0].Kind = "perspective-projection"
         && perspectiveProjection.Annotations[0].Text =
            Disclosed(
                "Verification · perspective-source · engine-qualification · bounded-perspective"
            )
         && perspectiveProjection.Selection.SelectedUnits.Length = 0
         && perspectiveProjection.Selection.SelectedEvent.IsNone)
        "Perspective review exposed an entity or retained an invisible selection."

    let fullModel =
        reviewModel
            FullReplay
            "full-source"
            VerifiedReplay
            BrowserKernelVerified
            (inspection 7 [ projectedUnit ] [ projectedEvent ] None)
            (Some firstUnit)
            (Some projectedEvent.Id)
    let reviewAccepted =
        TacticalSceneProjection.acceptReview fullModel
        |> Option.defaultWith (fun () ->
            failwith "Verified full replay owner was rejected.")
    let reviewInput =
        { AcceptedReview = reviewAccepted
          ReviewCamera = camera
          ReviewFocusedUnit = Some 999 }
    let reviewProjection = TacticalSceneProjection.review reviewInput
    let ordinaryVisual =
        TacticalSceneProjection.visualSystem "accessible-default" false 24
    let denseVisual =
        TacticalSceneProjection.visualSystem "high-contrast" false 100
    let stressVisual =
        TacticalSceneProjection.visualSystem "monochrome-pattern" true 200
    require
        (reviewProjection.Owner = ReviewScene
         && not reviewProjection.Disclosure.PerspectiveFiltered
         && reviewProjection.Disclosure.PreservesFieldDisclosures
         && reviewProjection.Units[0].Visual.Level = NotPresent
         && reviewProjection.Units[0].Visual.StanceId = NotPresent
         && (reviewProjection.Annotations
             |> Array.exists (fun annotation ->
                 annotation.Kind = "browser-kernel-verified"
                 && annotation.Text =
                    Disclosed(
                        "Verification · full-source · engine-qualification"
                    )))
         && reviewProjection.Selection.SelectedUnits = [| firstUnit |]
         && reviewProjection.Selection.SelectedEvent = Some projectedEvent.Id
         && (reviewProjection.Selection.SelectedPrimitiveIds
             |> Array.map ScenePrimitiveId.value)
            = [| "unit:" + string firstUnit
                 ; "review-event:" + string projectedEvent.Id |]
         && reviewProjection.Selection.FocusedUnit.IsNone
         && ordinaryVisual.Identity = "tactical-visual-system-v1"
         && ordinaryVisual.Density = OrdinaryDensity
         && denseVisual.Density = DenseDensity
         && stressVisual.Density = StressDensity
         && stressVisual.ReducedMotion
         && stressVisual.TransitionMilliseconds = 1
         && stressVisual.EffectMilliseconds = 120
         && stressVisual.MaximumActiveEffects = 256
         && stressVisual.LayerOrder
            = [| "terrain"; "edges"; "routes"; "units"; "effects"; "selection"; "tactical-overlays"; "annotations" |]
         && reviewProjection.Effects.Length = 1
         && reviewProjection.Effects[0].Kind = GenericEffect
         && reviewProjection.Effects[0].Lifecycle = HistoricalEffect
         && reviewProjection.Effects[0].EventId = projectedEvent.Id
         && reviewProjection.Effects[0].SourceUnitId = Some firstUnit
         && reviewProjection.Effects[0].TargetUnitId.IsNone
         && reviewProjection.Effects[0].SourcePoint.IsSome
         && reviewProjection.Effects[0].TargetPoint.IsNone
         && ScenePrimitiveId.value reviewProjection.Effects[0].PrimitiveId
            = "effect:7:" + string projectedEvent.Id)
        "Review projection expanded absent fields or retained stale selection."
    let previousReviewFrame =
        { fullModel with
            Inspection =
                Some(
                    inspection
                        6
                        [ { projectedUnit with Column = 0; Row = 1 } ]
                        [ { projectedEvent with Tick = 6 } ]
                        None
                ) }
        |> Shell.renderFrame
        |> Option.defaultWith (fun () ->
            failwith "Previous accepted Review frame did not render.")
    let interpolatedReview, interpolatedAlpha =
        TacticalSceneProjection.interpolateReviewPresentation
            previousReviewFrame
            0.5
            reviewProjection
    require
        (interpolatedAlpha = 0.5
         && interpolatedReview.Tick = reviewProjection.Tick
         && interpolatedReview.RevisionIdentity = reviewProjection.RevisionIdentity
         && interpolatedReview.Disclosure = reviewProjection.Disclosure
         && interpolatedReview.Annotations = reviewProjection.Annotations
         && interpolatedReview.Effects = reviewProjection.Effects
         && interpolatedReview.Selection = reviewProjection.Selection
         && interpolatedReview.Units[0].PresentationColumn = 1.0
         && interpolatedReview.Units[0].PresentationRow = 2.0
         && interpolatedReview.Units[0].Visual = reviewProjection.Units[0].Visual)
        "Shared Review interpolation changed committed identity/facts or missed midpoint presentation coordinates."
    let hiddenPrevious =
        { previousReviewFrame with
            Disclosure = PerspectiveDisclosure }
    let guardedDisclosure, guardedDisclosureAlpha =
        TacticalSceneProjection.interpolateReviewPresentation
            hiddenPrevious
            0.5
            reviewProjection
    let extraPrevious =
        { previousReviewFrame with
            Units =
                Array.append
                    previousReviewFrame.Units
                    [| { previousReviewFrame.Units[0] with Id = 999 } |] }
    let guardedEntities, guardedEntitiesAlpha =
        TacticalSceneProjection.interpolateReviewPresentation
            extraPrevious
            0.5
            reviewProjection
    require
        (guardedDisclosureAlpha = 1.0
         && guardedDisclosure = reviewProjection
         && guardedEntitiesAlpha = 1.0
         && guardedEntities = reviewProjection
         && guardedEntities.Units
            |> Array.forall (fun unit -> unit.Visual.Id <> 999))
        "Review interpolation crossed disclosure/semantic-owner guards or leaked a previous-frame entity."
    let reviewInputBefore = reviewInput
    reviewProjection.Units[0] <-
        { reviewProjection.Units[0] with
            Visual =
                { reviewProjection.Units[0].Visual with
                    AnchorColumn = 999 } }
    let repeatedReviewProjection =
        TacticalSceneProjection.review reviewInput
    require
        (reviewInput = reviewInputBefore
         && repeatedReviewProjection.Units[0].Visual.AnchorColumn =
            projectedUnit.Column
         && (fullModel.Inspection |> Option.get).Units.Head.Column =
            projectedUnit.Column)
        "Review projection shared its mutable unit array with the accepted owner or a later projection."

    let baselineIds = ids editorProjection
    let changedEditor =
        TacticalSceneProjection.editor
            { editorInput with
                EditorState =
                    { editor with
                        Tick = editor.Tick + 1
                        SelectedUnits = Set.singleton firstUnit
                        SelectedRegion = None }
                EditorWorkspace =
                    { workspace with
                        Camera =
                            { PanX = 100.0
                              PanY = -40.0
                              Zoom = 2.0 } }
                EditorFocusedUnit = Some secondUnit }
    let changedIds = ids changedEditor
    require
        (baselineIds = changedIds
         && baselineIds.Length = (baselineIds |> Array.distinct).Length
         && baselineIds |> Array.contains "terrain:0:0"
         && baselineIds |> Array.contains "unit:1"
         && baselineIds |> Array.exists (fun id -> id.StartsWith("edge:", StringComparison.Ordinal))
         && baselineIds |> Array.exists (fun id -> id.StartsWith("layer:", StringComparison.Ordinal)))
        "Semantic primitive identities were unstable, duplicated, or incomplete."

    let planningIds = ids planningProjection
    let changedPlanning =
        TacticalSceneProjection.planning
            { planningInput with
                PlanningState =
                    { planningState with
                        AuthoringTick = planningState.AuthoringTick + 1
                        SelectedUnit = Some secondUnit
                        SelectedCommand = None }
                PlanningCamera = { camera with PanX = 77.0 } }
    require
        (planningIds = ids changedPlanning
         && planningIds.Length = (Array.distinct planningIds).Length)
        "Planning semantic IDs changed with cursor, camera, or selection state."

    let simulatorIds = ids simulatorProjection
    let changedSimulator =
        TacticalSceneProjection.simulator
            { simulatorInput with
                SimulatorCamera = { camera with Zoom = 2.0 }
                SimulatorFocusedUnit = Some secondUnit }
    require
        (simulatorIds = ids changedSimulator
         && simulatorIds.Length = (Array.distinct simulatorIds).Length
         && simulatorProjection.RevisionIdentity =
            changedSimulator.RevisionIdentity)
        "Simulator IDs or accepted revision changed with camera or focus."
    let firstRouteId =
        repeatedSimulatorProjection.Routes
        |> Array.find (fun route -> route.OwnerUnitId = Some firstUnit)
        |> _.PrimitiveId
        |> ScenePrimitiveId.value
    let secondRouteProjection =
        TacticalSceneProjection.simulator
            { simulatorInput with
                SimulatorSelectedUnit = Some secondUnit
                SimulatorFocusedUnit = Some secondUnit }
    let secondRouteId =
        secondRouteProjection.Routes
        |> Array.find (fun route -> route.OwnerUnitId = Some secondUnit)
        |> _.PrimitiveId
        |> ScenePrimitiveId.value
    require
        (firstRouteId = "route:simulator:" + string firstUnit + ":planned"
         && secondRouteId =
            "route:simulator:" + string secondUnit + ":planned"
         && firstRouteId <> secondRouteId)
        "Simulator route identity was reused across distinct owning units."
    let repeatedSummaryAt tick =
        TacticalSceneProjection.simulator
            { simulatorInput with
                SimulatorHandoff =
                    { simulator with
                        Tick = tick
                        LastEvents = [ "Repeated summary" ]
                        LastCombatEvents = [] } }
    let firstOccurrence = repeatedSummaryAt 41
    let secondOccurrence = repeatedSummaryAt 42
    let eventId projection =
        projection.Annotations
        |> Array.find (fun annotation ->
            ScenePrimitiveId.value annotation.PrimitiveId
            |> _.StartsWith("simulator-event:"))
        |> _.PrimitiveId
        |> ScenePrimitiveId.value
    let firstEventId = eventId firstOccurrence
    let secondEventId = eventId secondOccurrence
    let sameOccurrenceDifferentCamera =
        TacticalSceneProjection.simulator
            { simulatorInput with
                SimulatorHandoff =
                    { simulator with
                        Tick = 41
                        LastEvents = [ "Repeated summary" ]
                        LastCombatEvents = [] }
                SimulatorCamera = { camera with PanX = 321.0 }
                SimulatorFocusedUnit = Some secondUnit }
    require
        (firstEventId <> secondEventId
         && firstEventId =
            eventId sameOccurrenceDifferentCamera)
        "Simulator event occurrences collided across ticks or changed with camera/focus."

    let changedFullModel =
        { fullModel with
            Inspection =
                Some(
                    inspection
                        8
                        [ { projectedUnit with Column = 3 } ]
                        [ projectedEvent ]
                        None
                )
            Selection =
                { fullModel.Selection with
                    Unit = None
                    Event = None } }
    let changedReview =
        TacticalSceneProjection.review
            { reviewInput with
                AcceptedReview =
                    TacticalSceneProjection.acceptReview changedFullModel
                    |> Option.defaultWith (fun () ->
                        failwith "Second accepted frame was rejected.")
                ReviewCamera = { camera with PanY = 91.0 } }
    let otherSourceReview =
        { fullModel with
            Source =
                Loaded
                    { (match fullModel.Source with
                       | Loaded metadata -> metadata
                       | _ -> failwith "Expected loaded metadata.") with
                        SourceIdentity = "other-source" } }
        |> TacticalSceneProjection.acceptReview
        |> Option.map (fun accepted ->
            TacticalSceneProjection.review
                { AcceptedReview = accepted
                  ReviewCamera = camera
                  ReviewFocusedUnit = None })
        |> Option.defaultWith (fun () ->
            failwith "Other accepted source was rejected.")
    let reviewIds = ids reviewProjection
    require
        (reviewIds = ids changedReview
         && reviewIds.Length = (Array.distinct reviewIds).Length
         && reviewProjection.RevisionIdentity =
            changedReview.RevisionIdentity
         && reviewProjection.RevisionIdentity
            <> otherSourceReview.RevisionIdentity)
        "Review IDs/revision were unstable across frames or collided across sources."

    let map = denseMap ()
    let denseEditor =
        { editor with
            Map = map
            Revision =
                { editor.Revision with
                    Document = map
                    Digest = "dense-editor-revision" }
            SelectedUnit = Some 1
            SelectedUnits = Set.singleton 1 }
    let denseEditorInput =
        { EditorState = denseEditor
          EditorWorkspace = workspace
          EditorFocusedUnit = Some 1 }
    let densePlanning =
        PlanningWorkspace.initial
            "dense-editor-revision"
            (map.Units |> Map.toSeq |> Seq.map snd)
    let denseCommands =
        [ 1 .. 200 ]
        |> List.map (fun id ->
            { Id = "route-" + string id
              UnitId = int32 id
              EarliestTick = int32 id
              Kind =
                PlannedRoute
                    [| int32 ((id - 1) % 40), int32 ((id - 1) / 40)
                       int32 (id % 40), int32 ((id - 1) / 40) |] })
    let densePlanningInput =
        { PlanningMap = map
          PlanningState =
            { densePlanning with
                Commands = denseCommands
                Digest = "dense-planning-revision" }
          PlanningCamera = camera
          PlanningFocusedUnit = Some 1 }
    let denseSimulatorMap = { map with Terrain = Map.empty }
    let denseSimulatorEditor =
        { denseEditor with
            Map = denseSimulatorMap
            Revision =
                { denseEditor.Revision with
                    Document = denseSimulatorMap
                    Digest = "dense-simulator-revision" } }
    let denseSimulator =
        MapEditorSimulator.tryHandoff denseSimulatorEditor
        |> Result.defaultWith failwith
    let denseSimulatorInput =
        { SimulatorHandoff = denseSimulator
          SimulatorSelectedUnit = Some 1
          SimulatorCamera = camera
          SimulatorFocusedUnit = Some 1 }
    let denseUnits =
        map.Units
        |> Map.toList
        |> List.map (fun (id, unit) ->
            ({ Id = id
               Side = string unit.Side
               Column = unit.Column
               Row = unit.Row
               Health = unit.Health
               HealthMaximum = unit.HealthMaximum
               MovementDirection = None
               BodyFacing = 0
               AttentionDirection = 2 }
             : UnitProjection))
    let denseEvents =
        [ 1 .. 300 ]
        |> List.map (fun id ->
            ({ Id = int32 id
               Tick = int32 id
               Source = "dense"
               Lifecycle = CommittedEvent
               Summary = "Event " + string id
               SourceUnitId = Some(int32 id)
               TargetUnitId = None }
             : EventProjection))
    let denseReviewAccepted =
        reviewModel
            FullReplay
            "dense-review"
            VerifiedReplay
            BrowserKernelVerified
            (inspection 200 denseUnits denseEvents None)
            (Some 1)
            (Some 1)
        |> TacticalSceneProjection.acceptReview
        |> Option.defaultWith (fun () ->
            failwith "Dense review owner was rejected.")
    let denseReviewInput =
        { AcceptedReview = denseReviewAccepted
          ReviewCamera = camera
          ReviewFocusedUnit = Some 1 }
    let representativeReviewAccepted =
        reviewModel
            FullReplay
            "representative-review"
            VerifiedReplay
            BrowserKernelVerified
            (inspection 100 (denseUnits |> List.truncate 100) (denseEvents |> List.truncate 100) None)
            (Some 1)
            (Some 1)
        |> TacticalSceneProjection.acceptReview
        |> Option.defaultWith (fun () -> failwith "Representative review owner was rejected.")
    let representativeReviewInput =
        { AcceptedReview = representativeReviewAccepted
          ReviewCamera = camera
          ReviewFocusedUnit = Some 1 }

    TacticalSceneProjection.editor denseEditorInput |> ignore
    TacticalSceneProjection.planning densePlanningInput |> ignore
    TacticalSceneProjection.simulator denseSimulatorInput |> ignore
    TacticalSceneProjection.review denseReviewInput |> ignore
    TacticalSceneProjection.review representativeReviewInput |> ignore
    let editorTimings =
        Array.init 80 (fun _ ->
            milliseconds (fun () ->
                TacticalSceneProjection.editor denseEditorInput))
    let planningTimings =
        Array.init 80 (fun _ ->
            milliseconds (fun () ->
                TacticalSceneProjection.planning densePlanningInput))
    let simulatorTimings =
        Array.init 80 (fun _ ->
            milliseconds (fun () ->
                TacticalSceneProjection.simulator denseSimulatorInput))
    let reviewTimings =
        Array.init 80 (fun _ ->
            milliseconds (fun () ->
                TacticalSceneProjection.review denseReviewInput))
    let representativeReviewTimings =
        Array.init 120 (fun _ ->
            milliseconds (fun () ->
                TacticalSceneProjection.review representativeReviewInput))
    let allocation operation =
        let before = GC.GetAllocatedBytesForCurrentThread()
        operation () |> ignore
        GC.GetAllocatedBytesForCurrentThread() - before
    let editorAllocation =
        allocation (fun () ->
            TacticalSceneProjection.editor denseEditorInput)
    let planningAllocation =
        allocation (fun () ->
            TacticalSceneProjection.planning densePlanningInput)
    let simulatorAllocation =
        allocation (fun () ->
            TacticalSceneProjection.simulator denseSimulatorInput)
    let reviewAllocation =
        allocation (fun () ->
            TacticalSceneProjection.review denseReviewInput)
    let overlayIds =
        TacticalSceneProjection.overlayRegistry
        |> Array.map (fun overlay -> TacticalOverlayId.value overlay.Id)
    require
        (overlayIds.Length = 14
         && overlayIds |> Array.distinct |> Array.length = 14
         && overlayIds
            = [| "unit.footprints"; "unit.body-facing"; "movement.reachable-path-cost"
                 "movement.planned-routes"; "movement.reservations"; "awareness.attention-vision"
                 "spatial.exact-los"; "cover.exposure"; "combat.armor-coverage"
                 "combat.area-engagements"; "combat.suppression"; "combat.attack-traces"
                 "combat.hp-wounds"; "command.state" |])
        "Tactical overlay registry IDs or deterministic order drifted."
    let exportedPreferences =
        TacticalSceneProjection.initialOverlayPreferences
        |> TacticalSceneProjection.exportOverlayPreferences
    require
        (match TacticalSceneProjection.importOverlayPreferences exportedPreferences with
         | Ok restored -> TacticalSceneProjection.exportOverlayPreferences restored = exportedPreferences
         | Error _ -> false)
        "Overlay preferences did not restore deterministically."
    require
        (match TacticalSceneProjection.importOverlayPreferences "v1|spatial.exact-los=approximate" with
         | Error MalformedOverlayPreferences -> true
         | _ -> false)
        "Unreadable overlay preferences did not fail closed."
    let denseOverlayScene = TacticalSceneProjection.planning densePlanningInput
    let exactLosDescriptor =
        TacticalSceneProjection.overlayRegistry
        |> Array.find (fun overlay -> TacticalOverlayId.value overlay.Id = "spatial.exact-los")
    let sourcePrimitive = denseOverlayScene.Units[0].PrimitiveId
    let exactLosScene =
        { denseOverlayScene with
            Routes =
                Array.append
                    denseOverlayScene.Routes
                    [| { PrimitiveId = sourcePrimitive
                         OwnerUnitId = Some 1
                         OverlayId = exactLosDescriptor.Id
                         Kind = "exact-los:supercover:corner:door-solid:blocker=semantic-edge"
                         Points = [| 0.5; 0.5; 0.5; 1.5; 1.5; 1.5 |]
                         MovementCost = Some 2
                         BlockerIds = [| "semantic-edge"; "door-solid" |]
                         Label = Disclosed "Exact LOS blocked by closed door" } |] }
    let exactLos =
        TacticalSceneProjection.projectOverlays
            TacticalSceneProjection.initialOverlayPreferences
            (Set.singleton exactLosDescriptor.Id)
            exactLosScene
    require
        (exactLos.Payloads
         |> Array.exists (fun payload ->
             payload.OverlayId = exactLosDescriptor.Id
             && payload.Kind.Contains("supercover")
             && payload.Kind.Contains("door-solid")
             && payload.Points = [| 0.5; 0.5; 0.5; 1.5; 1.5; 1.5 |]))
        "Exact LOS overlay approximated or discarded authoritative corner/door evidence."
    require
        (exactLos.Payloads
         |> Array.exists (fun payload ->
             match payload.Geometry with
             | PathGeometry(points, movementCost, blockerIds) ->
                 payload.OverlayId = exactLosDescriptor.Id
                 && points = [| 0.5; 0.5; 0.5; 1.5; 1.5; 1.5 |]
                 && movementCost = Some 2
                 && blockerIds = [| "semantic-edge"; "door-solid" |]
             | _ -> false))
        "Exact LOS typed cost, blockers, or geometry changed before overlay consumption."
    let undisclosed =
        TacticalSceneProjection.projectOverlays
            TacticalSceneProjection.initialOverlayPreferences
            Set.empty
            { exactLosScene with
                Disclosure =
                    { exactLosScene.Disclosure with
                        PreservesFieldDisclosures = false } }
    require
        (undisclosed.Payloads.Length = 0
         && undisclosed.Labels.Length = 0
         && undisclosed.Cost.RegistryTraversals = 1
         && undisclosed.Cost.DisclosurePasses = 1
         && undisclosed.Cost.CandidatePayloads = 0)
        "Unavailable disclosure leaked overlay geometry, labels, counts, or diagnostic work shape."
    let allOverlayPreferences =
        { TacticalSceneProjection.initialOverlayPreferences with
            Modes =
                TacticalSceneProjection.overlayRegistry
                |> Array.map (fun descriptor ->
                    descriptor.Id,
                    if Set.contains Persistent descriptor.SupportedModes then Persistent
                    elif Set.contains SelectionScoped descriptor.SupportedModes then SelectionScoped
                    else InspectHeld)
                |> Map.ofArray }
    let heldOverlays =
        TacticalSceneProjection.overlayRegistry
        |> Array.filter (fun descriptor -> Set.contains InspectHeld descriptor.SupportedModes)
        |> Array.map _.Id
        |> Set.ofArray
    let genericAnnotation = planningProjection.Annotations[0]
    let genericAnnotationPrimitive = genericAnnotation.PrimitiveId
    let genericAnnotationScene =
        { denseOverlayScene with
            Annotations =
                [| { genericAnnotation with
                       OverlayId = Some exactLosDescriptor.Id
                       Geometry = None
                       Text = Disclosed "exact-los radius=99 heading=42 impact=7,7 cost=500 blocker=secret" } |] }
    let genericAnnotationProjection =
        TacticalSceneProjection.projectOverlays
            allOverlayPreferences
            heldOverlays
            genericAnnotationScene
    require
        (genericAnnotationProjection.Payloads
         |> Array.forall (fun payload -> payload.PrimitiveId <> genericAnnotationPrimitive))
        "Generic annotation text was disguised as authoritative tactical geometry."
    let workloadScene unitCount =
        let units = denseOverlayScene.Units |> Array.truncate unitCount
        let ids = units |> Array.map _.Visual.Id
        let overlayId value =
            TacticalSceneProjection.overlayRegistry
            |> Array.find (fun descriptor -> TacticalOverlayId.value descriptor.Id = value)
            |> _.Id
        let factAnnotations =
            units
            |> Array.collect (fun unit ->
                let x = unit.PresentationColumn + 0.5
                let y = unit.PresentationRow + 0.5
                [| "unit.body-facing", "body-facing", DirectionGeometry(x, y, 0.25, None), "Body facing"
                   "awareness.attention-vision", "attention", DirectionGeometry(x, y, 1.25, Some 0.5), "Attention sector"
                   "movement.reservations", "reservation", AreaGeometry(x, y, 0.625), "Reserved footprint"
                   "cover.exposure", "cover", DirectionGeometry(x, y, 0.375, Some 1.125), "Cover sector"
                   "combat.armor-coverage", "armor", DirectionGeometry(x, y, 1.875, Some 0.75), "Armor sector"
                   "combat.area-engagements", "engagement", AreaGeometry(x, y, 2.25), "Engagement area"
                   "combat.suppression", "suppression", StatusGeometry(x, y, Some 20, Some 100, [| "suppressed" |]), "Suppression"
                   "combat.attack-traces", "attack", TraceGeometry([| x; y; x + 1.0; y + 0.25 |], x + 1.0, y + 0.25), "Attack trace"
                   "combat.hp-wounds", "wound", StatusGeometry(x, y, Some 7, Some 10, [| "wound:serious" |]), "Wounds"
                   "command.state", "command", StatusGeometry(x, y, Some 2, Some 3, [| "command:hold" |]), "Command state" |]
                |> Array.map (fun (overlay, kind, geometry, text) ->
                    { PrimitiveId = unit.PrimitiveId
                      Kind = kind
                      OverlayId = Some(overlayId overlay)
                      SubjectUnitId = Some unit.Visual.Id
                      Column = Some(int32 unit.PresentationColumn)
                      Row = Some(int32 unit.PresentationRow)
                      Geometry = Some geometry
                      Text = Disclosed text }))
        let routes =
            units
            |> Array.collect (fun unit ->
                let x = unit.PresentationColumn + 0.5
                let y = unit.PresentationRow + 0.5
                [| { PrimitiveId = unit.PrimitiveId
                     OwnerUnitId = Some unit.Visual.Id
                     OverlayId = overlayId "movement.reachable-path-cost"
                     Kind = "reachable-path"
                     Points = [| x; y; x + 1.0; y |]
                     MovementCost = Some 3
                     BlockerIds = [| "edge:" + string unit.Visual.Id |]
                     Label = Disclosed "Reachable path" }
                   { PrimitiveId = unit.PrimitiveId
                     OwnerUnitId = Some unit.Visual.Id
                     OverlayId = overlayId "movement.planned-routes"
                     Kind = "planned-route"
                     Points = [| x; y; x; y + 1.0 |]
                     MovementCost = Some 4
                     BlockerIds = [||]
                     Label = Disclosed "Planned route" }
                   { PrimitiveId = unit.PrimitiveId
                     OwnerUnitId = Some unit.Visual.Id
                     OverlayId = exactLosDescriptor.Id
                     Kind = "exact-los:supercover"
                     Points = [| x; y; x + 1.5; y + 0.5 |]
                     MovementCost = None
                     BlockerIds = [| "door:" + string unit.Visual.Id |]
                     Label = Disclosed "Exact LOS" } |])
        { denseOverlayScene with
            Units = units
            Routes = routes
            Annotations = factAnnotations
            Selection = { denseOverlayScene.Selection with SelectedUnits = ids } }
    let representativeProjection = TacticalSceneProjection.projectOverlays allOverlayPreferences heldOverlays (workloadScene 100)
    let stressScene = workloadScene 200
    let stressProjection = TacticalSceneProjection.projectOverlays allOverlayPreferences heldOverlays stressScene
    let emittedIds projection = projection.Payloads |> Array.map _.OverlayId |> Set.ofArray
    let missingOverlayIds =
        TacticalSceneProjection.overlayRegistry
        |> Array.filter (fun descriptor -> not (Set.contains descriptor.Id (emittedIds representativeProjection)))
        |> Array.map (fun descriptor -> TacticalOverlayId.value descriptor.Id)
    require
        ((workloadScene 100).Units.Length = 100 && stressScene.Units.Length = 200 && Array.isEmpty missingOverlayIds)
        ("The representative production-view projection did not emit every advertised initial overlay family: " + String.concat ", " missingOverlayIds)
    require
        (representativeProjection.Payloads |> Array.exists (fun payload -> TacticalOverlayId.value payload.OverlayId = "unit.footprints" && match payload.Geometry with FootprintGeometry(_, _, width, depth) -> width = 1.0 && depth = 1.0 | _ -> false)
         && representativeProjection.Payloads |> Array.exists (fun payload -> TacticalOverlayId.value payload.OverlayId = "cover.exposure" && match payload.Geometry with DirectionGeometry(_, _, heading, arc) -> heading = 0.375 && arc = Some 1.125 | _ -> false)
         && representativeProjection.Payloads |> Array.exists (fun payload -> TacticalOverlayId.value payload.OverlayId = "movement.reachable-path-cost" && match payload.Geometry with PathGeometry(points, cost, blockers) -> points.Length = 4 && cost = Some 3 && blockers |> Array.exists (fun blocker -> blocker.StartsWith("edge:")) | _ -> false)
         && representativeProjection.Payloads |> Array.exists (fun payload -> TacticalOverlayId.value payload.OverlayId = "combat.area-engagements" && match payload.Geometry with AreaGeometry(_, _, radius) -> radius = 2.25 | _ -> false)
         && representativeProjection.Payloads |> Array.exists (fun payload -> TacticalOverlayId.value payload.OverlayId = "combat.attack-traces" && match payload.Geometry with TraceGeometry(points, impactX, impactY) -> points.Length = 4 && impactX = points[2] && impactY = points[3] | _ -> false)
         && representativeProjection.Payloads |> Array.exists (fun payload -> TacticalOverlayId.value payload.OverlayId = "command.state" && payload.SubjectId = "1" && match payload.Geometry with StatusGeometry(_, _, current, maximum, tokens) -> current = Some 2 && maximum = Some 3 && tokens = [| "command:hold" |] | _ -> false))
        "Typed footprint/direction/path-blocker/trace geometry was lost before rendering."
    require
        (representativeProjection.Cost.EstimatedSvgNodes <= 5000
         && stressProjection.Cost.EstimatedSvgNodes <= 5000
         && stressProjection.Cost.EmittedPayloads > representativeProjection.Cost.EmittedPayloads)
        "Representative 100-unit or stress 200-unit production-view node budget was not enforced."
    let overlayTimings =
        Array.init 80 (fun _ ->
            milliseconds (fun () ->
                TacticalSceneProjection.projectOverlays allOverlayPreferences heldOverlays stressScene))
    let overlayProjection =
        TacticalSceneProjection.projectOverlays
            allOverlayPreferences heldOverlays stressScene
    let overlayP95 = p95 overlayTimings
    require
        (overlayProjection.Cost.RegistryTraversals = 1
         && overlayProjection.Cost.DisclosurePasses = 1
         && overlayProjection.Cost.EmittedPayloads <= 4096
         && overlayProjection.Cost.EmittedLabels <= 256
         && overlayProjection.Cost.EstimatedSvgNodes <= 5000
         && overlayP95 < 20.0)
        (sprintf "Tactical overlay stress projection exceeded structural/timing budgets: %.3f ms, %A." overlayP95 overlayProjection.Cost)
    let editorP95 = p95 editorTimings
    let planningP95 = p95 planningTimings
    let simulatorP95 = p95 simulatorTimings
    let reviewP95 = p95 reviewTimings
    let representativeReview = TacticalSceneProjection.review representativeReviewInput
    let stressReview = TacticalSceneProjection.review denseReviewInput
    let representativeReviewP95 = p95 representativeReviewTimings
    require
        (editorP95 < 50.0
         && planningP95 < 50.0
         && simulatorP95 < 50.0
         && reviewP95 < 50.0
         && editorAllocation < 3_000_000L
         && planningAllocation < 3_000_000L
         && simulatorAllocation < 3_000_000L
         && reviewAllocation < 3_000_000L
         && representativeReview.VisualCost.UnitCount = 100
         && representativeReview.VisualCost.EffectInstances = 100
         && representativeReview.VisualCost.EstimatedSvgNodes <= 5_000
         && representativeReviewP95 < 4.0
         && stressReview.VisualCost.UnitCount = 200
         && stressReview.VisualCost.EffectInstances = 256
         && stressReview.Effects[0].Tick = 45
         && stressReview.Effects[stressReview.Effects.Length - 1].Tick = 300
         && stressReview.VisualCost.EstimatedSvgNodes <= 9_000
         && reviewP95 < 8.0)
        (sprintf
            "Shared scene projection exceeded budget: representative %d nodes/%d effects/%.3f ms; stress %d nodes/%d effects/%.3f ms; editor %.3f ms/%d; planning %.3f ms/%d; simulator %.3f ms/%d; review %.3f ms/%d."
            representativeReview.VisualCost.EstimatedSvgNodes
            representativeReview.VisualCost.EffectInstances
            representativeReviewP95
            stressReview.VisualCost.EstimatedSvgNodes
            stressReview.VisualCost.EffectInstances
            reviewP95
            editorP95
            editorAllocation
            planningP95
            planningAllocation
            simulatorP95
            simulatorAllocation
            reviewP95
            reviewAllocation)

    printfn
        "Tactical scene projection qualification passed: validated replay owners, overlay registry/disclosure/exact LOS/preference/order/bounds, dense p95 %.3f/%.3f/%.3f/%.3f/overlay %.3f ms, allocations %d/%d/%d/%d bytes."
        editorP95
        planningP95
        simulatorP95
        reviewP95
        overlayP95
        editorAllocation
        planningAllocation
        simulatorAllocation
        reviewAllocation
