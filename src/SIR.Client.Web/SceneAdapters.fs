module SIR.Client.Web.SceneAdapters

open SIR.Client.Web.AppTypes

let sharedSceneUnitCommand model unitId =
    match model.Workspace with
    | EditorWorkspace -> Some("editor.scene.select.unit." + string unitId)
    | PlanningWorkspace -> Some("planning.roster.select." + string unitId)
    | SimulatorWorkspace -> Some("simulator.scene.select.unit." + string unitId)
    | ReplayWorkspace -> Some("review.scene.select.unit." + string unitId)

let sharedSceneCellCommand model column row =
    match model.Workspace with
    | EditorWorkspace -> Some("editor.scene.cell." + string column + "." + string row)
    | PlanningWorkspace -> Some("planning.battlefield.cell." + string column + "." + string row)
    | _ -> None
