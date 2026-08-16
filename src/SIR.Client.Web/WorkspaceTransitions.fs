module SIR.Client.Web.WorkspaceTransitions

open Elmish
open SIR.Client
open SIR.Client.Web.AppShell
open SIR.Client.Web.AppTypes
open SIR.Client.Web.DocumentationFeatureContract

let private validSimulatorFor editor current =
    match MapEditorSimulator.tryHandoff editor with
    | Error _ -> current
    | Ok initial ->
        current
        |> Option.map (MapEditorSimulator.reconcile editor)
        |> Option.orElse (Some initial)

let change workspace model =
    if workspace = DocsWorkspace then
        { model with
            Workspace = DocsWorkspace
            LastTacticalWorkspace = if model.Workspace = DocsWorkspace then model.LastTacticalWorkspace else model.Workspace
            InputHelpExpanded = false
            DocumentationError = None },
        if model.Documentation.IsNone then Cmd.OfAsync.perform load () DocumentationLoaded else Cmd.none
    else
        let editor =
            if workspace = ReplayWorkspace then MapEditor.update CancelEditorGesture model.Editor else model.Editor
        let editorView =
            if workspace = EditorWorkspace then model.EditorView
            else MapEditorWorkspace.update model.Editor.Map (MapEditor.selected model.Editor) CancelEditorPointers model.EditorView
        let planning, initializePlanning =
            if workspace = PlanningWorkspace then
                match model.Planning with
                | Some current when current.MapRevision = model.Editor.Revision.Digest -> Some current, false
                | _ ->
                    (model.Editor.Map.Units
                     |> Map.toSeq
                     |> Seq.map snd
                     |> PlanningWorkspace.initial model.Editor.Revision.Digest
                     |> PlanningWorkspace.update (SetPlanningAuthoringTick(int model.Tactical.Cursor))
                     |> Some), true
            else model.Planning, false
        let tacticalModality =
            match workspace with
            | EditorWorkspace -> Editor
            | PlanningWorkspace -> Plan
            | SimulatorWorkspace -> Simulate
            | ReplayWorkspace -> Review
            | DocsWorkspace -> failwith "Docs is handled before tactical modality projection."
        let transitionModel = { model with Editor = editor; Planning = planning }
        { model with
            Editor = editor
            Planning = planning
            Simulator = if workspace = SimulatorWorkspace then validSimulatorFor editor model.Simulator else model.Simulator
            TacticalSelectedUnit = reconcileTacticalSelectedUnit workspace transitionModel
            Workspace = workspace
            LastTacticalWorkspace = workspace
            Tactical = model.Tactical |> UnifiedTacticalWorkspace.switchModality tacticalModality
            EditorView = editorView
            InputHelpExpanded = false
            SimulatorControllerSelection = None
            HeldInputs = HeldInputSession.recover model.HeldInputs },
        if initializePlanning then Cmd.ofEffect (fun dispatch -> dispatch InitializePlanningWorker) else Cmd.none
