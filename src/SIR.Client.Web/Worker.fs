module SIR.Client.Web.Worker

open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open SIR.Client
open SIR.Domain
open SIR.Simulation

let private scope: obj = emitJsExpr () "globalThis"

let private supportedEngine =
    EngineCatalog.Current.EngineHash

let private shortIdentity (bytes: byte array) =
    bytes
    |> Array.take 6
    |> Array.map (fun value -> value.ToString("x2"))
    |> String.concat ""

let private emptyProjection tick : InspectionProjection =
    { Tick = tick
      BoardMinimumColumn = 0
      BoardMinimumRow = 0
      BoardMaximumColumn = 0
      BoardMaximumRow = 0
      Units = []
      Edges = []
      Events = []
      Checkpoints = []
      PerspectiveHash = None }

let private scenarioMetadata (scenario: DesignScenario) (report: LabReport) : ReplayMetadata =
    { SourceName = scenario.Identity + ".sir-scenario"
      SourceIdentity = report.Comparison.Baseline.ResultIdentity
      EngineIdentity = scenario.EngineIdentity
      FinalTick = 1
      Kind = DesignScenario }

let private inputSummary input =
    match input with
    | Move(unitId, destination) ->
        "unit "
        + string (Simulation.unitIdValue unitId)
        + " moves to "
        + string destination.Col
        + ","
        + string destination.Row
    | Observe(observerId, targetId) ->
        "unit "
        + string (Simulation.unitIdValue observerId)
        + " observes unit "
        + string (Simulation.unitIdValue targetId)
    | Attack(attackerId, targetId) ->
        "unit "
        + string (Simulation.unitIdValue attackerId)
        + " attacks unit "
        + string (Simulation.unitIdValue targetId)

let private inputUnits input =
    match input with
    | Move(unitId, _) -> Some(Simulation.unitIdValue unitId), None
    | Observe(observerId, targetId)
    | Attack(observerId, targetId) ->
        Some(Simulation.unitIdValue observerId),
        Some(Simulation.unitIdValue targetId)

let private journalAt tick (full: FullReplay) =
    let externalInputs =
        full.OrderedInputs
        |> List.choose (fun input ->
            if input.Tick = tick then
                Some(input.Sequence, input.Input)
            else
                None)

    let wasmInputs =
        full.AcceptedWasmOutputs
        |> List.choose (fun output ->
            if output.Tick = tick then
                Some(output.Sequence, output.Input)
            else
                None)

    externalInputs @ wasmInputs
    |> List.sortBy fst
    |> List.map snd

let private stateAt tick (full: FullReplay) =
    let target = max full.InitialSnapshot.Tick (min full.FinalResult.Tick tick)

    let start =
        full.Checkpoints
        |> List.filter (fun checkpoint -> checkpoint.Tick <= target)
        |> List.sortByDescending (fun checkpoint -> checkpoint.Tick)
        |> List.tryHead
        |> Option.map (fun checkpoint -> checkpoint.State)
        |> Option.defaultValue full.InitialSnapshot

    let mutable state = start

    for current in start.Tick + 1 .. target do
        state <- (Simulation.runTick state (journalAt current full)).State

    state

let private fullProjection tick (full: FullReplay) : InspectionProjection =
    let state = stateAt tick full

    let units =
        state.Units
        |> Map.toList
        |> List.map (fun (_, unit) ->
            { Id = Simulation.unitIdValue unit.Id
              Side =
                match unit.Side with
                | Red -> "Red"
                | Blue -> "Blue"
              Column = unit.Cell.Col
              Row = unit.Cell.Row
              Health = BoundedInt32.value unit.Health
              HealthMaximum = BoundedInt32.maximum unit.Health
              MovementDirection = None
              BodyFacing = int32 (Direction8.toCode unit.BodyFacing)
              AttentionDirection =
                  int32 (Direction8.toCode unit.AttentionDirection) })

    let externalEvents =
        full.OrderedInputs
        |> List.mapi (fun index input ->
            let sourceUnitId, targetUnitId = inputUnits input.Input
            { Id = index
              Tick = input.Tick
              Source = "External input"
              Summary = inputSummary input.Input
              SourceUnitId = sourceUnitId
              TargetUnitId = targetUnitId })

    let wasmEvents =
        full.AcceptedWasmOutputs
        |> List.mapi (fun index output ->
            let sourceUnitId, targetUnitId = inputUnits output.Input
            { Id = List.length externalEvents + index
              Tick = output.Tick
              Source = "Accepted WASM output"
              Summary = inputSummary output.Input
              SourceUnitId = sourceUnitId
              TargetUnitId = targetUnitId })

    let edges =
        state.Board.Edges
        |> List.mapi (fun index edge ->
            ({ Id = "edge-" + string index
               Kind = "wall"
               State = if edge.BlocksMovement then "solid" else "open"
               StartColumn = edge.Edge.Lo.Col
               StartRow = edge.Edge.Lo.Row
               EndColumn = edge.Edge.Hi.Col
               EndRow = edge.Edge.Hi.Row }
            : EdgeProjection))

    let checkpoints =
        full.Checkpoints
        |> List.map (fun checkpoint ->
            { Tick = checkpoint.Tick
              StateHash = shortIdentity checkpoint.StateHash
              EventHash = shortIdentity checkpoint.EventHash })

    { Tick = state.Tick
      BoardMinimumColumn = state.Board.Minimum.Col
      BoardMinimumRow = state.Board.Minimum.Row
      BoardMaximumColumn = state.Board.Maximum.Col
      BoardMaximumRow = state.Board.Maximum.Row
      Units = units
      Edges = edges
      Events = externalEvents @ wasmEvents
      Checkpoints = checkpoints
      PerspectiveHash = None }

let private perspectiveProjection tick (frames: PerspectiveFrame list) : InspectionProjection =
    let frame =
        frames
        |> List.filter (fun candidate -> candidate.Tick <= tick)
        |> List.tryLast
        |> Option.orElseWith (fun () -> List.tryHead frames)

    match frame with
    | Some selected ->
        { emptyProjection selected.Tick with
            PerspectiveHash = Some(shortIdentity selected.ProjectionHash) }
    | None -> emptyProjection 0

let private projectionAt tick package : InspectionProjection =
    match package.Content with
    | AuthorizedFullReplay full -> fullProjection tick full
    | PerspectivePlayback frames -> perspectiveProjection tick frames

let private replayError error =
    match error with
    | UnsupportedFormat(actual, supported) ->
        RunnerUnsupported(
            "format "
            + string actual
            + " is not supported; expected "
            + string supported
        )
    | EngineMismatch _ ->
        RunnerUnsupported "the required retained engine bundle is unavailable"
    | ReplayDivergence(tick, field) ->
        RunnerDiverged(tick, "kernel", field)
    | PackageTooLarge(actual, maximum) ->
        RunnerFailed(
            "package size "
            + string actual
            + " exceeds "
            + string maximum
        )
    | MalformedPackage detail -> RunnerFailed detail
    | UnauthorizedFullReplay ->
        RunnerFailed "the full replay is not authorized"
    | InvalidHashLength(field, _) ->
        RunnerFailed("invalid hash length for " + field)
    | ResourceLimitExceeded(field, _, _) ->
        RunnerFailed("resource limit exceeded for " + field)
    | InvalidOrdering field ->
        RunnerFailed("invalid canonical ordering for " + field)
    | InvalidCheckpoint(tick, detail) ->
        RunnerFailed(
            "invalid checkpoint at tick "
            + string tick
            + ": "
            + detail
        )
    | PerspectiveHasNoKernel ->
        RunnerFailed "perspective playback has no reconstructable kernel"
    | WasmExecutionNotVerified ->
        RunnerFailed "browser verification does not include WASM execution"
    | WasmOutputDivergence(tick, sequence) ->
        RunnerDiverged(
            tick,
            "WASM re-execution",
            "accepted output sequence " + string sequence
        )

let private metadata sourceName package : ReplayMetadata =
    let kind, finalTick =
        match package.Content with
        | AuthorizedFullReplay full -> FullReplay, full.FinalResult.Tick
        | PerspectivePlayback frames ->
            PerspectiveReplay,
            (frames
             |> List.tryLast
             |> Option.map (fun frame -> frame.Tick)
             |> Option.defaultValue 0)

    { SourceName = sourceName
      SourceIdentity =
        package
        |> Replay.encode
        |> CanonicalHash.sha256
        |> shortIdentity
      EngineIdentity = shortIdentity package.EngineHash
      FinalTick = finalTick
      Kind = kind }

let mutable private loadedPackage: ReplayPackage option = None
let mutable private cancelled: Set<int32> = Set.empty

let private post operation response =
    let envelope: WorkerResponseEnvelope =
        { ProtocolVersion = int32 WorkerProtocol.CurrentVersion
          Operation = OperationId.value operation
          Response = response }

    scope?postMessage (envelope)

let private isCancelled operation =
    cancelled |> Set.contains (OperationId.value operation)

let private execute operation request =
    async {
        match request with
        | LoadPackage(sourceName, bytes) ->
            match Replay.decode Replay.defaultLimits bytes with
            | Error error -> post operation (replayError error)
            | Ok package ->
                match
                    Replay.runKernelReplay
                        Replay.defaultLimits
                        supportedEngine
                        package
                with
                | Ok(BrowserKernelVerified _) ->
                    loadedPackage <- Some package
                    post operation (
                        LoadedPackage(
                            metadata sourceName package
                            |> WorkerTransport.metadataToTransport,
                            KernelVerified,
                            projectionAt 0 package
                            |> WorkerTransport.inspectionToTransport
                        )
                    )
                | Ok(PerspectiveReady _) ->
                    loadedPackage <- Some package
                    post operation (
                        LoadedPackage(
                            metadata sourceName package
                            |> WorkerTransport.metadataToTransport,
                            ProjectionOnly,
                            projectionAt 0 package
                            |> WorkerTransport.inspectionToTransport
                        )
                    )
                | Ok(AuthoritativeVerified _) ->
                    post operation (
                        RunnerFailed(
                            "browser runner made an authoritative verification claim"
                        )
                    )
                | Error error -> post operation (replayError error)
        | Advance(currentTick, tickCount, finalTick) ->
            let target = min finalTick (currentTick + tickCount)
            let mutable tick = currentTick
            let mutable completedBatches = 0

            for batchEnd in WorkerProtocol.batchEnds currentTick target do
                if not (isCancelled operation) then
                    tick <- batchEnd
                    completedBatches <- completedBatches + 1

                    match loadedPackage with
                    | Some package when tick < target ->
                        post operation (
                            RunnerProgress(
                                tick,
                                completedBatches,
                                projectionAt tick package
                                |> WorkerTransport.inspectionToTransport
                            )
                        )
                    | _ -> ()

                    do! Async.Sleep 0

            if not (isCancelled operation) then
                match loadedPackage with
                | Some package ->
                    let projection = projectionAt tick package
                    post operation (
                        Progressed(
                            projection.Tick,
                            projection
                            |> WorkerTransport.inspectionToTransport
                        )
                    )
                | None -> post operation (RunnerFailed "no replay is loaded in the worker")
        | Seek(targetTick, finalTick) ->
            match loadedPackage with
            | Some package ->
                let tick = max 0 (min finalTick targetTick)
                let projection = projectionAt tick package
                post operation (
                    RunnerProgress(
                        projection.Tick,
                        1,
                        projection
                        |> WorkerTransport.inspectionToTransport
                    )
                )
                do! Async.Sleep 0

                if not (isCancelled operation) then
                    post operation (
                        Progressed(
                            projection.Tick,
                            projection
                            |> WorkerTransport.inspectionToTransport
                        )
                    )
            | None -> post operation (RunnerFailed "no replay is loaded in the worker")
        | Fork(identity, _) -> post operation (Forked identity)
        | LoadScenario scenarioIdentity ->
            match Lab.tryScenario scenarioIdentity with
            | None -> post operation (RunnerFailed("unknown design scenario: " + scenarioIdentity))
            | Some scenario ->
                match Lab.run scenario Map.empty None with
                | Error error -> post operation (RunnerFailed error)
                | Ok report ->
                    let metadata = scenarioMetadata scenario report
                    post operation (
                        LoadedScenario(
                            metadata
                            |> WorkerTransport.metadataToTransport,
                            Lab.scenarioToTransport scenario,
                            Lab.reportToTransport report,
                            emptyProjection 0
                            |> WorkerTransport.inspectionToTransport
                        )
                    )
        | RunExperiment(scenarioIdentity, patch, sweepParameter) ->
            match Lab.tryScenario scenarioIdentity with
            | None -> post operation (RunnerFailed("unknown design scenario: " + scenarioIdentity))
            | Some scenario ->
                match
                    Lab.run
                        scenario
                        (Lab.parametersFromTransport patch)
                        sweepParameter
                with
                | Error error -> post operation (RunnerFailed error)
                | Ok report ->
                    post operation (
                        ExperimentCompleted(
                            report.Comparison.Fork.ResultIdentity,
                            Lab.reportToTransport report
                        )
                    )
        | Cancel -> cancelled <- cancelled |> Set.add (OperationId.value operation)
    }

let private receive (event: MessageEvent) =
    let envelope = unbox<WorkerRequestEnvelope> event.data
    let operation = OperationId.create envelope.Operation

    if envelope.ProtocolVersion <> int32 WorkerProtocol.CurrentVersion then
        post operation (
            RunnerFailed(
                "worker protocol "
                + string envelope.ProtocolVersion
                + " is not supported"
            )
        )
    else
        execute operation envelope.Request |> Async.StartImmediate

scope?onmessage <- receive
