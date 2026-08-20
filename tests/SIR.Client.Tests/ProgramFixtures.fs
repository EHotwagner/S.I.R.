module SIR.Client.TestsProgramFixtures

open SIR.Client

let require condition message =
    if not condition then failwith message

let operationFrom effects =
    effects
    |> List.choose (function
        | Run(operation, Cancel) -> None
        | Run(operation, _) -> Some operation)
    |> List.exactlyOne

let requestFrom effects =
    effects
    |> List.choose (function
        | Run(_, Cancel) -> None
        | Run(_, request) -> Some request)
    |> List.exactlyOne

let metadata kind : ReplayMetadata =
    { SourceName = "fixture.sirr"
      SourceIdentity = "fixture"
      EngineIdentity = "engine"
      FinalTick = 20
      Kind = kind }

let projection tick : InspectionProjection =
    { Tick = tick
      BoardMinimumColumn = 0
      BoardMinimumRow = 0
      BoardMaximumColumn = 2
      BoardMaximumRow = 1
      Units = []
      Edges = []
      Events = []
      Checkpoints = []
      PerspectiveHash = None }
