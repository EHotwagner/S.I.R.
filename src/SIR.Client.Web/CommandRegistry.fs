module SIR.Client.Web.CommandRegistry

open System
open SIR.Client
open SIR.Client.Web.AppTypes
open SIR.Client.Web.AppShell

let activeTacticalRegistry model =
    let pointerCommand id label category modality =
        { Id = id
          Label = label
          Category = category
          Modalities = Set.singleton modality
          DefaultGesture = None
          PointerAvailable = true
          Precedence = 300
          ModalContext = None
          ModalPhase = None
          Availability = AlwaysAvailable }
    let modal =
        match model.Workspace with
        | EditorWorkspace ->
            let facts =
                { Editor = model.Editor
                  ActiveDomain =
                    match model.EditorToolPanel with
                    | TerrainTools -> TerrainDomain
                    | UnitTools -> UnitDomain
                    | EdgeTools -> EdgeDomain
                    | ZoneTools -> RegionDomain
                    | DocumentTools -> DocumentDomain
                  PanHeld = editorPanHeld model
                  InputHelpExpanded = model.InputHelpExpanded }
            ModalInput.editorCatalog facts
            |> UnifiedTacticalWorkspace.modalCommandDefinitions Editor
        | SimulatorWorkspace ->
            ModalInput.simulatorCatalog
                model.SimulatorSelectedUnit
                model.Simulator
                model.SimulatorControllerSelection
            |> UnifiedTacticalWorkspace.modalCommandDefinitions Simulate
        | _ -> []
    let contextual =
        match model.Workspace, model.Planning with
        | EditorWorkspace, _ ->
            [ yield
                  pointerCommand
                      "editor.scene.create-simulator-handoff"
                      "Inspect maintained simulation for authored revision"
                      "Editor shared scene"
                      Editor
              yield!
                  model.Editor.Map.Units
                  |> Map.toList
                  |> List.map (fun (unitId, _) ->
                      pointerCommand
                          ("editor.scene.select.unit." + string unitId)
                          ("Select shared-scene unit " + string unitId)
                          "Editor shared scene"
                          Editor)
              for row in 0 .. int model.Editor.Map.Height - 1 do
                  for column in 0 .. int model.Editor.Map.Width - 1 do
                      yield
                          pointerCommand
                              ("editor.scene.cell."
                               + string column + "." + string row)
                              ("Activate shared-scene cell "
                               + string column + "," + string row)
                              "Editor shared scene"
                              Editor ]
        | PlanningWorkspace, Some planning ->
            let selectionActions =
                [ yield!
                      planning.Roster
                      |> Array.map (fun unit ->
                          pointerCommand
                              ("planning.roster.select." + string unit.UnitId)
                              ("Select " + unit.Name)
                              "Plan roster"
                              Plan)
                      |> Array.toList
                  yield!
                      planning.Commands
                      |> List.map (fun command ->
                          pointerCommand
                              ("planning.timeline.select." + command.Id)
                              ("Select timeline command " + command.Id)
                              "Plan timeline"
                              Plan)
                  yield!
                      planning.Issues
                      |> Array.mapi (fun index issue ->
                          pointerCommand
                              ("planning.issue.focus." + string index)
                              ("Focus issue " + issue.Code)
                              "Plan validation"
                              Plan)
                      |> Array.toList ]
            let battlefieldActions =
                [ for row in 0 .. int model.Editor.Map.Height - 1 do
                      for column in 0 .. int model.Editor.Map.Width - 1 do
                          yield
                              pointerCommand
                                  ("planning.battlefield.cell."
                                   + string column + "." + string row)
                                  ("Add route waypoint "
                                   + string column + "," + string row)
                                  "Plan battlefield cells"
                                  Plan ]
            if planning.SelectedUnit.IsNone then selectionActions
            else
            let directions =
                [ "north"; "north-east"; "east"; "south-east"
                  "south"; "south-west"; "west"; "north-west" ]
            let inspectorActions =
              match planning.Tool with
              | RouteTool ->
                [ "west"; "north"; "south"; "east" ]
                |> List.map (fun direction ->
                    pointerCommand
                        ("planning.inspector.waypoint." + direction)
                        ("Add waypoint " + direction)
                        "Plan inspector"
                        Plan)
              | FacingTool
              | AttentionTool ->
                directions
                |> List.map (fun direction ->
                    let channel =
                        if planning.Tool = FacingTool then "facing" else "attention"
                    pointerCommand
                        ("planning.inspector." + channel + "." + direction)
                        ("Set " + channel + " " + direction)
                        "Plan inspector"
                        Plan)
              | StanceTool ->
                [ "standing"; "crouched"; "prone" ]
                |> List.map (fun stance ->
                    pointerCommand
                        ("planning.inspector.stance." + stance)
                        ("Set stance " + stance)
                        "Plan inspector"
                        Plan)
              | HoldTool ->
                [ pointerCommand "planning.inspector.hold" "Add hold" "Plan inspector" Plan ]
              | EngagementTool ->
                [ pointerCommand "planning.inspector.engagement" "Add disclosed engagement" "Plan inspector" Plan ]
              | SynchronizationTool ->
                [ pointerCommand "planning.inspector.synchronization" "Add synchronization marker" "Plan inspector" Plan ]
            selectionActions @ battlefieldActions @ inspectorActions
        | SimulatorWorkspace, _ ->
            let selectionActions =
                model.Simulator
                |> Option.map _.RuntimeMap.Units
                |> Option.defaultValue Map.empty
                |> Map.toList
                |> List.map (fun (unitId, _) ->
                    pointerCommand
                        ("simulator.scene.select.unit." + string unitId)
                        ("Select shared-scene unit " + string unitId)
                        "Simulator shared scene"
                        Simulate)
            let controllerActions =
                [ "manual", "Manual"; "scripted", "Scripted AI"
                  "general", "General AI" ]
                |> List.map (fun (id, label) ->
                    pointerCommand
                        ("simulator.pointer.controller." + id)
                        ("Set controller " + label)
                        "Simulator controllers"
                        Simulate)
            let scriptAction =
                pointerCommand
                    "simulator.pointer.script.set"
                    "Set selected unit direction script"
                    "Simulator controllers"
                    Simulate
            let movementActions =
                [ "north-west", "NW"; "north", "N"; "north-east", "NE"
                  "west", "W"; "east", "E"
                  "south-west", "SW"; "south", "S"; "south-east", "SE" ]
                |> List.map (fun (id, label) ->
                    pointerCommand
                        ("simulator.pointer.movement." + id)
                        ("Move selected unit " + label)
                        "Simulator movement"
                        Simulate)
            selectionActions @ (scriptAction :: controllerActions @ movementActions)
        | ReplayWorkspace, _ ->
            [ yield!
                  model.Shell.Inspection
                  |> Option.map _.Units
                  |> Option.defaultValue []
                  |> List.map (fun unit ->
                      pointerCommand
                          ("review.scene.select.unit." + string unit.Id)
                          ("Select disclosed unit " + string unit.Id)
                          "Review shared scene"
                          Review)
              yield!
                  model.Shell.Inspection
                  |> Option.map _.Events
                  |> Option.defaultValue []
                  |> List.map (fun event ->
                      pointerCommand
                          ("review.scene.select.event." + string event.Id)
                          ("Select disclosed event " + string event.Id)
                          "Review shared scene"
                          Review) ]
        | _ -> []
    let cameraCommands =
        [ pointerCommand
              "scene.camera.zoom-out"
              "Zoom shared workscreen out"
              "Shared camera"
              model.Tactical.Modality
          pointerCommand
              "scene.camera.zoom-in"
              "Zoom shared workscreen in"
              "Shared camera"
              model.Tactical.Modality
          pointerCommand
              "scene.camera.fit"
              "Fit shared workscreen"
              "Shared camera"
              model.Tactical.Modality ]
    let panelCommands =
        [ pointerCommand "panel.data" "Rules data" "View" model.Tactical.Modality ]
    UnifiedTacticalWorkspace.commandRegistry @ modal @ contextual @ cameraCommands @ panelCommands
    |> List.distinctBy _.Id
