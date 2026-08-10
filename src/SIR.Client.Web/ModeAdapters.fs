module SIR.Client.Web.ModeAdapters

open SIR.Client

let projectPlanningSegments (state: PlanningWorkspaceState) =
    [ for command in state.Commands do
          let authored =
              { Id = command.Id
                UnitId = Some command.UnitId
                StartTick = int64 command.EarliestTick
                EndTick = int64 command.EarliestTick + 1L
                Channel = Authored
                Label = string command.Kind
                Issue = state.Issues |> Array.tryFind (fun issue -> issue.CommandId = Some command.Id) |> Option.map _.Detail }
          yield authored
          if state.AcceptedRevision = Some state.Revision then
              yield { authored with Id = "accepted:" + command.Id; Channel = Accepted; Label = "Worker-accepted " + string command.Kind; Issue = None }
          if state.CommittedRevision = Some state.Revision then
              yield { authored with Id = "committed:" + command.Id; Channel = Committed; Label = "Committed " + string command.Kind; Issue = None }
      match state.Predicted with
      | Some prediction ->
          yield { Id = "prediction-" + string prediction.Revision; UnitId = None; StartTick = 0L; EndTick = 1L; Channel = Predicted; Label = "Intent-only predicted state"; Issue = None }
      | None -> () ]
