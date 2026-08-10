module SIR.Client.Web.AppShell

open SIR.Client
open SIR.Client.Web.AppTypes

let editorPanHeld model =
    HeldInputSession.contains EditorPan model.HeldInputs

let tacticalUnitIds workspace (model: Model) =
    match workspace with
    | EditorWorkspace
    | PlanningWorkspace ->
        model.Editor.Map.Units |> Map.toSeq |> Seq.map fst |> Set.ofSeq
    | SimulatorWorkspace ->
        model.Simulator
        |> Option.map (fun simulator -> simulator.RuntimeMap.Units |> Map.toSeq |> Seq.map fst |> Set.ofSeq)
        |> Option.defaultValue Set.empty
    | ReplayWorkspace ->
        model.Shell.Inspection
        |> Option.map (fun inspection -> inspection.Units |> Seq.map _.Id |> Set.ofSeq)
        |> Option.defaultValue Set.empty

let reconcileTacticalSelectedUnit workspace (model: Model) =
    let visible = tacticalUnitIds workspace model
    let keep candidate = candidate |> Option.filter (fun unitId -> Set.contains unitId visible)
    keep model.TacticalSelectedUnit
    |> Option.orElseWith (fun () ->
        match workspace with
        | EditorWorkspace -> keep model.Editor.SelectedUnit
        | PlanningWorkspace -> model.Planning |> Option.bind _.SelectedUnit |> keep
        | SimulatorWorkspace -> keep model.SimulatorSelectedUnit
        | ReplayWorkspace -> keep model.Shell.Selection.Unit)
    |> Option.orElseWith (fun () -> visible |> Set.toSeq |> Seq.tryHead)
