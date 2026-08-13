module SIR.Client.Web.Worker

open Browser.Types
open Fable.Core
open Fable.Core.JsInterop
open SIR.Client
open SIR.Domain
open SIR.Simulation

let private scope: obj = emitJsExpr () "globalThis"

[<Emit("new Promise(resolve => setTimeout(resolve, 1))")>]
let private yieldToWorkerMessages () : JS.Promise<unit> = jsNative

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
    | PhysicalAttack(attackerId, aim, profile) ->
        "unit "
        + string (Simulation.unitIdValue attackerId)
        + " fires "
        + string profile
        + " at "
        + string aim.Col
        + ","
        + string aim.Row
    | SetAttention(unitId, direction) ->
        "unit " + string (Simulation.unitIdValue unitId) + " sets attention " + string direction
    | SetWeaponPosture(unitId, posture) ->
        "unit " + string (Simulation.unitIdValue unitId) + " sets posture " + string posture
    | PrepareAreaReaction(unitId, engagementId, cells, direction) ->
        "unit " + string (Simulation.unitIdValue unitId) + " prepares " + engagementId + " over " + string cells.Length + " cells facing " + string direction
    | PrepareUnitReaction(unitId, engagementId, targetId, direction) ->
        "unit " + string (Simulation.unitIdValue unitId) + " prepares " + engagementId + " on unit " + string (Simulation.unitIdValue targetId) + " facing " + string direction
    | PrepareEdgeReaction(unitId, engagementId, _, direction) ->
        "unit " + string (Simulation.unitIdValue unitId) + " prepares " + engagementId + " on an edge facing " + string direction

let private inputUnits input =
    match input with
    | Move(unitId, _) -> Some(Simulation.unitIdValue unitId), None
    | Observe(observerId, targetId)
    | Attack(observerId, targetId) ->
        Some(Simulation.unitIdValue observerId),
        Some(Simulation.unitIdValue targetId)
    | PhysicalAttack(attackerId, _, _) ->
        Some(Simulation.unitIdValue attackerId), None
    | SetAttention(unitId, _) -> Some(Simulation.unitIdValue unitId), None
    | SetWeaponPosture(unitId, _) -> Some(Simulation.unitIdValue unitId), None
    | PrepareAreaReaction(unitId, _, _, _) -> Some(Simulation.unitIdValue unitId), None
    | PrepareUnitReaction(unitId, _, targetId, _) -> Some(Simulation.unitIdValue unitId), Some(Simulation.unitIdValue targetId)
    | PrepareEdgeReaction(unitId, _, _, _) -> Some(Simulation.unitIdValue unitId), None

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

    let awarenessEvents =
        state.Awareness
        |> Map.toList
        |> List.mapi (fun index ((observerId, subjectId), contact) ->
            { Id = List.length externalEvents + List.length wasmEvents + index
              Tick = state.Tick
              Source = "Authoritative awareness"
              Summary = "Awareness " + string contact.Level + " · " + string contact.Reason + " · acquisition " + string contact.Acquisition
              SourceUnitId = Some(Simulation.unitIdValue observerId)
              TargetUnitId = Some(Simulation.unitIdValue subjectId) })

    let engagementEvents =
        state.Engagements
        |> Map.toList
        |> List.mapi (fun index (ownerId, engagement) ->
            { Id = List.length externalEvents + List.length wasmEvents + List.length awarenessEvents + index
              Tick = state.Tick
              Source = "Authoritative reaction"
              Summary = engagement.EngagementId + " · " + string engagement.Phase + " · " + string engagement.Reason
              SourceUnitId = Some(Simulation.unitIdValue ownerId)
              TargetUnitId = None })

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
      Events = externalEvents @ wasmEvents @ awarenessEvents @ engagementEvents
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

type private SimulatorSessionState =
    { Session: string
      MapRevision: string
      PlanRevision: int64
      InitialProjection: InspectionProjectionTransport
      CurrentProjection: InspectionProjectionTransport
      MaximumHorizonTicks: int32
      CommittedPlan: SimulatorPlanTransport option
      AuthoritativeRun: Map<int32, InspectionProjectionTransport> }

let mutable private simulatorSession: SimulatorSessionState option = None
let mutable private cancelledSimulatorOperations: Set<int32> = Set.empty

let private post operation response =
    let envelope: WorkerResponseEnvelope =
        { ProtocolVersion = int32 WorkerProtocol.CurrentVersion
          Operation = OperationId.value operation
          Response = response }

    scope?postMessage (envelope)

let private isCancelled operation =
    cancelled |> Set.contains (OperationId.value operation)

let private postSimulator
    (correlation: SimulatorCorrelation)
    (response: SimulatorResponse)
    =
    let currentTick =
        simulatorSession
        |> Option.map (fun session -> session.CurrentProjection.Tick)
        |> Option.defaultValue correlation.Tick

    let envelope: SimulatorResponseEnvelope =
        { Kind = SimulatorProtocol.Kind
          ProtocolVersion = int32 SimulatorProtocol.CurrentVersion
          Correlation = correlation
          CurrentTick = currentTick
          Response = response }

    scope?postMessage (envelope)

let private simulatorUpdate isSnapshot projection =
    { IsSnapshot = isSnapshot
      Projection = projection }

let private simulatorDisclosure (plan: SimulatorPlanTransport) =
    match plan.PreviewLabel with
    | DeterministicPreview ->
        [| "Deterministic from committed, disclosed session inputs." |]
    | AssumptionBasedPreview ->
        plan.Assumptions
        |> Array.map (fun assumption -> "Assumption: " + assumption)
    | IntentOnlyPreview ->
        plan.Intents
        |> Array.map (fun intent -> "Intent only: " + intent)

let private validateSimulatorCorrelation
    (correlation: SimulatorCorrelation)
    (session: SimulatorSessionState)
    =
    correlation.Session = session.Session
    && correlation.MapRevision = session.MapRevision
    && correlation.Tick = session.CurrentProjection.Tick

let private validateSimulatorWorkspace
    (correlation: SimulatorCorrelation)
    (session: SimulatorSessionState)
    =
    correlation.Session = session.Session
    && correlation.MapRevision = session.MapRevision

let private simulatorDelta tick (projection: InspectionProjectionTransport) =
    { projection with
        Tick = tick
        Units = [||]
        Edges = [||]
        Events = [||]
        Checkpoints = [||] }

let private authoritativeProjection
    (initial: InspectionProjectionTransport)
    (frame: AuthoritativeProjectionFrame)
    =
    let units =
        frame.VisibleUnits
        |> Array.map (fun visible ->
            match
                initial.Units
                |> Array.tryFind (fun unit -> unit.Id = visible.UnitId)
            with
            | Some unit ->
                { unit with
                    Column = visible.DisplayColumn
                    Row = visible.DisplayRow
                    Health = visible.Health }
            | None ->
                { Id = visible.UnitId
                  Side = "disclosed"
                  Column = visible.DisplayColumn
                  Row = visible.DisplayRow
                  Health = visible.Health
                  HealthMaximum = max 1 visible.Health
                  MovementDirection = None
                  BodyFacing = int32 (Direction8.toCode North)
                  AttentionDirection = int32 (Direction8.toCode North) })
    { initial with
        Tick = frame.Tick
        Units = units
        Events = [||]
        Checkpoints =
            [| { Tick = frame.Tick
                 StateHash = shortIdentity frame.StateIdentity
                 EventHash = shortIdentity frame.EventIdentity } |] }

let private validAuthoritativeFrame
    (initial: InspectionProjectionTransport)
    (frame: AuthoritativeProjectionFrame)
    =
    frame.Tick > initial.Tick
    && frame.ServerSequence > 0L
    && frame.ProjectionRevision > 0L
    && frame.StateIdentity.Length = 32
    && frame.EventIdentity.Length = 32
    && frame.VisibleUnits.Length <= SimulatorProtocol.MaximumProjectionUnits
    && (frame.VisibleUnits
        |> Array.distinctBy _.UnitId
        |> Array.length) = frame.VisibleUnits.Length
    && (frame.VisibleUnits
        |> Array.forall (fun unit ->
            unit.UnitId > 0
            && unit.DisplayColumn >= initial.BoardMinimumColumn
            && unit.DisplayColumn <= initial.BoardMaximumColumn
            && unit.DisplayRow >= initial.BoardMinimumRow
            && unit.DisplayRow <= initial.BoardMaximumRow
            && unit.Health >= 0))

let private executeSimulator
    (correlation: SimulatorCorrelation)
    (request: SimulatorRequest)
    =
    async {
        match request with
        | InitializeSession initialization ->
            if
                System.String.IsNullOrWhiteSpace correlation.Session
                || System.String.IsNullOrWhiteSpace correlation.MapRevision
                || initialization.InitialProjection.Tick <> correlation.Tick
                || initialization.MaximumHorizonTicks <= 0
                || initialization.MaximumHorizonTicks > SimulatorProtocol.MaximumHorizonTicks
            then
                postSimulator
                    correlation
                    (SimulatorRequestRejected(
                        "SIR.SIMULATOR.SESSION.INVALID",
                        "Session, map revision, tick, and horizon must form a valid initialization."
                    ))
            else
                let session =
                    { Session = correlation.Session
                      MapRevision = correlation.MapRevision
                      PlanRevision = correlation.PlanRevision
                      InitialProjection = initialization.InitialProjection
                      CurrentProjection = initialization.InitialProjection
                      MaximumHorizonTicks = initialization.MaximumHorizonTicks
                      CommittedPlan = None
                      AuthoritativeRun = Map.empty }

                simulatorSession <- Some session
                cancelledSimulatorOperations <- Set.empty
                postSimulator
                    correlation
                    (SessionInitialized(
                        simulatorUpdate true initialization.InitialProjection
                    ))
        | CancelOperation targetOperation ->
            match simulatorSession with
            | Some session
                // Cancellation races the target operation by definition. Its tick is the
                // caller's last observed progress and may legitimately trail the worker's
                // current projection, while session/map/plan identities must still match.
                when validateSimulatorWorkspace correlation session
                     && correlation.PlanRevision = session.PlanRevision ->
                cancelledSimulatorOperations <-
                    Set.add targetOperation cancelledSimulatorOperations

                postSimulator
                    correlation
                    (SimulatorOperationCancelled targetOperation)
            | _ ->
                postSimulator
                    correlation
                    (SimulatorRequestRejected(
                        "SIR.SIMULATOR.CORRELATION.STALE",
                        "Cancellation does not match the active simulator workspace."
                    ))
        | _ ->
            match simulatorSession with
            | None ->
                postSimulator
                    correlation
                    (SimulatorRequestRejected(
                        "SIR.SIMULATOR.SESSION.MISSING",
                        "Initialize the simulator session before sending operations."
                    ))
            | Some session when not (validateSimulatorCorrelation correlation session) ->
                postSimulator
                    correlation
                    (SimulatorRequestRejected(
                        "SIR.SIMULATOR.CORRELATION.STALE",
                        "The session, map revision, or tick is stale."
                    ))
            | Some session ->
                match request with
                | ValidatePlan plan ->
                    if correlation.PlanRevision < session.PlanRevision then
                        postSimulator
                            correlation
                            (SimulatorRequestRejected(
                                "SIR.SIMULATOR.PLAN.STALE",
                                "The plan revision is stale."
                            ))
                    else
                        let diagnostics =
                            SimulatorProtocol.diagnostics
                                session.MaximumHorizonTicks
                                plan

                        postSimulator
                            correlation
                            (PlanValidated(
                                (if diagnostics.Length = 0 then
                                     Some correlation.PlanRevision
                                 else
                                     None),
                                diagnostics
                            ))
                | PreviewPlan(plan, fromTick, toTick) ->
                    let diagnostics =
                        SimulatorProtocol.diagnostics
                            session.MaximumHorizonTicks
                            plan

                    if correlation.PlanRevision < session.PlanRevision then
                        postSimulator
                            correlation
                            (SimulatorRequestRejected(
                                "SIR.SIMULATOR.PLAN.STALE",
                                "The plan revision is stale."
                            ))
                    elif
                        fromTick <> correlation.Tick
                        || toTick < fromTick
                        || toTick - fromTick > SimulatorProtocol.MaximumPreviewTicks
                    then
                        postSimulator
                            correlation
                            (SimulatorRequestRejected(
                                "SIR.SIMULATOR.PREVIEW.HORIZON",
                                "A preview must start at the expected tick and span at most 1,200 ticks."
                            ))
                    elif diagnostics.Length <> 0 then
                        postSimulator
                            correlation
                            (PlanValidated(None, diagnostics))
                    else
                        // Intent-only previews deliberately contain no entity state.
                        let projection =
                            match plan.PreviewLabel with
                            | IntentOnlyPreview ->
                                simulatorDelta
                                    session.CurrentProjection.Tick
                                    session.CurrentProjection
                            | _ -> session.CurrentProjection

                        postSimulator
                            correlation
                            (PlanPreviewed(
                                plan.PreviewLabel,
                                simulatorDisclosure plan,
                                [| simulatorUpdate true projection |]
                            ))
                | CommitPlan plan ->
                    let diagnostics =
                        SimulatorProtocol.diagnostics
                            session.MaximumHorizonTicks
                            plan

                    if correlation.PlanRevision < session.PlanRevision then
                        postSimulator
                            correlation
                            (SimulatorRequestRejected(
                                "SIR.SIMULATOR.PLAN.STALE",
                                "The plan revision is older than the active plan."
                            ))
                    elif diagnostics.Length <> 0 then
                        postSimulator
                            correlation
                            (PlanValidated(None, diagnostics))
                    else
                        simulatorSession <-
                            Some
                                { session with
                                    PlanRevision = correlation.PlanRevision
                                    CommittedPlan = Some plan }

                        postSimulator
                            correlation
                            (PlanCommitted correlation.PlanRevision)
                | LoadAuthoritativeRun(matchLock, replayIdentity, updates) ->
                    let projections =
                        updates
                        |> Array.map (authoritativeProjection session.InitialProjection)
                    let ordered =
                        updates
                        |> Array.pairwise
                        |> Array.forall (fun (left, right) ->
                            left.Tick < right.Tick
                            && left.ServerSequence < right.ServerSequence
                            && left.ProjectionRevision < right.ProjectionRevision)
                    if
                        session.CommittedPlan.IsNone
                        || System.String.IsNullOrWhiteSpace matchLock
                        || System.String.IsNullOrWhiteSpace replayIdentity
                        || projections.Length = 0
                        || not ordered
                        || not (
                            updates
                            |> Array.forall (validAuthoritativeFrame session.InitialProjection)
                        )
                        || projections[0].Tick <= session.InitialProjection.Tick
                        || projections[projections.Length - 1].Tick
                           > session.InitialProjection.Tick
                             + session.MaximumHorizonTicks
                    then
                        postSimulator
                            correlation
                            (SimulatorRequestRejected(
                                "SIR.SIMULATOR.AUTHORITATIVE_RUN.INVALID",
                                "A qualified run requires pinned identities and ordered bounded projections."
                            ))
                    else
                        let run =
                            projections
                            |> Array.map (fun projection -> projection.Tick, projection)
                            |> Map.ofArray
                        simulatorSession <-
                            Some { session with AuthoritativeRun = run }
                        postSimulator
                            correlation
                            (AuthoritativeRunLoaded(
                                matchLock,
                                replayIdentity,
                                projections[projections.Length - 1].Tick
                            ))
                | Step tickCount ->
                    match session.CommittedPlan with
                    | _ when correlation.PlanRevision <> session.PlanRevision ->
                        postSimulator
                            correlation
                            (SimulatorRequestRejected(
                                "SIR.SIMULATOR.PLAN.STALE",
                                "Step does not match the committed plan revision."
                            ))
                    | None ->
                        postSimulator
                            correlation
                            (SimulatorRequestRejected(
                                "SIR.SIMULATOR.PLAN.NOT_COMMITTED",
                                "Commit a valid plan before stepping."
                            ))
                    | Some _ when tickCount <= 0 || tickCount > SimulatorProtocol.BatchSize ->
                        postSimulator
                            correlation
                            (SimulatorRequestRejected(
                                "SIR.SIMULATOR.STEP.COUNT",
                                "A step must advance between 1 and 256 ticks."
                            ))
                    | Some _ ->
                        let tick = session.CurrentProjection.Tick + tickCount
                        match Map.tryFind tick session.AuthoritativeRun with
                        | Some projection ->
                            simulatorSession <-
                                Some { session with CurrentProjection = projection }
                            postSimulator correlation (SimulatorStepped(simulatorUpdate false projection))
                        | None when session.AuthoritativeRun.IsEmpty ->
                            let delta = simulatorDelta tick session.CurrentProjection
                            simulatorSession <-
                                Some
                                    { session with
                                        CurrentProjection =
                                            { session.CurrentProjection with Tick = tick } }
                            postSimulator correlation (SimulatorStepped(simulatorUpdate false delta))
                        | None ->
                            postSimulator
                                correlation
                                (SimulatorRequestRejected(
                                    "SIR.SIMULATOR.AUTHORITATIVE_RUN.MISSING_TICK",
                                    "No qualified authoritative projection exists for the requested tick."
                                ))
                | RunTo targetTick ->
                    match session.CommittedPlan with
                    | _ when correlation.PlanRevision <> session.PlanRevision ->
                        postSimulator
                            correlation
                            (SimulatorRequestRejected(
                                "SIR.SIMULATOR.PLAN.STALE",
                                "Run-to does not match the committed plan revision."
                            ))
                    | None ->
                        postSimulator
                            correlation
                            (SimulatorRequestRejected(
                                "SIR.SIMULATOR.PLAN.NOT_COMMITTED",
                                "Commit a valid plan before running."
                            ))
                    | Some plan
                        when targetTick < session.CurrentProjection.Tick
                             || targetTick
                                > session.InitialProjection.Tick
                                  + plan.HorizonTicks ->
                        postSimulator
                            correlation
                            (SimulatorRequestRejected(
                                "SIR.SIMULATOR.RUN.TARGET",
                                "The target tick is outside the committed planning horizon."
                            ))
                    | Some _ ->
                        let batchEnds =
                            SimulatorProtocol.batchEnds
                                session.CurrentProjection.Tick
                                targetTick

                        let mutable current = session
                        let mutable completed = 0
                        let mutable stopped = false

                        for batchEnd in batchEnds do
                            if not stopped then
                                // A zero-duration timer can repeatedly re-enter an already-due timer
                                // chain before inbound worker messages are serviced under contention.
                                // A positive delay gives the host a scheduling window between batches.
                                do! yieldToWorkerMessages () |> Async.AwaitPromise
                                if
                                    Set.contains
                                        correlation.Operation
                                        cancelledSimulatorOperations
                                then
                                    stopped <- true
                                    postSimulator
                                        correlation
                                        (SimulatorOperationCancelled correlation.Operation)
                                else
                                    completed <- completed + 1
                                    match Map.tryFind batchEnd current.AuthoritativeRun with
                                    | None when current.AuthoritativeRun.IsEmpty ->
                                        let delta =
                                            simulatorDelta
                                                batchEnd
                                                current.CurrentProjection
                                        current <-
                                            { current with
                                                CurrentProjection =
                                                    { current.CurrentProjection with Tick = batchEnd } }
                                        simulatorSession <- Some current
                                        if batchEnd = targetTick then
                                            postSimulator
                                                correlation
                                                (SimulatorRunCompleted(
                                                    simulatorUpdate false delta
                                                ))
                                        else
                                            postSimulator
                                                correlation
                                                (SimulatorProgress(
                                                    completed,
                                                    simulatorUpdate false delta
                                                ))
                                    | None ->
                                        stopped <- true
                                        postSimulator
                                            correlation
                                            (SimulatorRequestRejected(
                                                "SIR.SIMULATOR.AUTHORITATIVE_RUN.MISSING_TICK",
                                                "No qualified authoritative projection exists for the requested tick."
                                            ))
                                    | Some projection ->
                                        current <-
                                            { current with CurrentProjection = projection }
                                        simulatorSession <- Some current

                                        if batchEnd = targetTick then
                                            postSimulator
                                                correlation
                                                (SimulatorRunCompleted(
                                                    simulatorUpdate false projection
                                                ))
                                        else
                                            postSimulator
                                                correlation
                                                (SimulatorProgress(
                                                    completed,
                                                    simulatorUpdate false projection
                                                ))
                | Reset ->
                    if correlation.PlanRevision <> session.PlanRevision then
                        postSimulator
                            correlation
                            (SimulatorRequestRejected(
                                "SIR.SIMULATOR.PLAN.STALE",
                                "Reset does not match the committed plan revision."
                            ))
                    else
                        let reset =
                            { session with
                                CurrentProjection = session.InitialProjection }
                        simulatorSession <- Some reset
                        postSimulator
                            correlation
                            (SimulatorReset(
                                simulatorUpdate true reset.InitialProjection
                            ))
                | InitializeSession _
                | CancelOperation _ -> ()
    }

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
    let kind: string = event.data?Kind

    if kind = SimulatorProtocol.Kind then
        let envelope = unbox<SimulatorRequestEnvelope> event.data

        if envelope.ProtocolVersion <> int32 SimulatorProtocol.CurrentVersion then
            postSimulator
                envelope.Correlation
                (SimulatorRequestRejected(
                    "SIR.SIMULATOR.PROTOCOL.VERSION",
                    "The simulator worker protocol version is not supported."
                ))
        else
            executeSimulator envelope.Correlation envelope.Request
            |> Async.StartImmediate
    else
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
