namespace SIR.Client

open SIR.Domain

type OperationId = private OperationId of int32

[<RequireQualifiedAccess>]
module OperationId =
    let value (OperationId value) = value

type ReplayKind =
    | FullReplay
    | PerspectiveReplay
    | DesignScenario

type ReplayMetadata =
    { SourceName: string
      SourceIdentity: string
      EngineIdentity: string
      FinalTick: int32
      Kind: ReplayKind }

type PlaybackSpeed =
    | Half
    | Normal
    | Double
    | Maximum

type PlaybackState =
    { CurrentTick: int32
      FinalTick: int32
      IsPlaying: bool
      Speed: PlaybackSpeed }

type RunMode =
    | NoRun
    | VerifiedReplay
    | PerspectivePlayback
    | SandboxFork of derivedIdentity: string
    | ScenarioSandbox of scenarioIdentity: string

type Verification =
    | NotLoaded
    | Loading
    | BrowserKernelVerified
    | PerspectiveReady
    | SandboxDerived of derivedIdentity: string
    | Unsupported of reason: string
    | Diverged of tick: int32 * phase: string * detail: string
    | Failed of reason: string

type SourceState =
    | NoSource
    | Reading of sourceName: string
    | Loaded of ReplayMetadata
    | Rejected of sourceName: string * reason: string

type SelectionState =
    { Unit: int32 option
      Event: int32 option
      Formula: string option }

type UnitProjection =
    { Id: int32
      Side: string
      Column: int32
      Row: int32
      Health: int32 }

type EventProjection =
    { Id: int32
      Tick: int32
      Source: string
      Summary: string }

type CheckpointProjection =
    { Tick: int32
      StateHash: string
      EventHash: string }

/// Bounded presentation data; the complete replay and world remain in the worker.
type InspectionProjection =
    { Tick: int32
      BoardMinimumColumn: int32
      BoardMinimumRow: int32
      BoardMaximumColumn: int32
      BoardMaximumRow: int32
      Units: UnitProjection list
      Events: EventProjection list
      Checkpoints: CheckpointProjection list
      PerspectiveHash: string option }

type WorkerState =
    | WorkerStarting
    | WorkerReady
    | WorkerBusy of completedBatches: int32
    | WorkerStopped of reason: string

type ParameterPatch = Map<string, int32>

type Model =
    { Source: SourceState
      Mode: RunMode
      Verification: Verification
      Playback: PlaybackState
      Selection: SelectionState
      Inspection: InspectionProjection option
      Patch: ParameterPatch
      Worker: WorkerState
      ActiveOperation: OperationId option
      NextOperation: int32
      Announcement: string }

type RunnerRequest =
    | LoadPackage of sourceName: string * bytes: byte array
    | Advance of currentTick: int32 * tickCount: int32 * finalTick: int32
    | Seek of targetTick: int32 * finalTick: int32
    | Fork of derivedIdentity: string * patch: ParameterPatch
    | Cancel

type RunnerClaim =
    | KernelVerified
    | ProjectionOnly
    | ScenarioReady

type RunnerResponse =
    | LoadedPackage of ReplayMetadata * RunnerClaim * InspectionProjection
    | RunnerProgress of tick: int32 * completedBatches: int32 * InspectionProjection
    | Progressed of tick: int32 * InspectionProjection
    | Forked of derivedIdentity: string
    | RunnerUnsupported of reason: string
    | RunnerDiverged of tick: int32 * phase: string * detail: string
    | RunnerFailed of reason: string

type Effect =
    | Run of OperationId * RunnerRequest

type Msg =
    | ReplayBytesSelected of sourceName: string * bytes: byte array
    | RunnerResponded of OperationId * RunnerResponse
    | TogglePlayback
    | StepForward
    | SeekRequested of int32
    | SpeedChanged of PlaybackSpeed
    | UnitSelected of int32 option
    | EventSelected of int32 option
    | FormulaSelected of string option
    | ParameterEdited of name: string * value: int32
    | CancelRequested
    | WorkerStarted
    | WorkerTerminated of reason: string

type WorkerRequestEnvelope =
    { ProtocolVersion: int32
      Operation: OperationId
      Request: RunnerRequest }

type WorkerResponseEnvelope =
    { ProtocolVersion: int32
      Operation: OperationId
      Response: RunnerResponse }

[<RequireQualifiedAccess>]
module WorkerProtocol =
    [<Literal>]
    let CurrentVersion = 1

    [<Literal>]
    let BatchSize = 256

    let batchEnds startTick targetTick =
        [ let mutable tick = startTick

          while tick < targetTick do
              tick <- min targetTick (tick + int32 BatchSize)
              yield tick ]

[<RequireQualifiedAccess>]
module Shell =
    let init () =
        { Source = NoSource
          Mode = NoRun
          Verification = NotLoaded
          Playback =
            { CurrentTick = 0
              FinalTick = 0
              IsPlaying = false
              Speed = Normal }
          Selection =
            { Unit = None
              Event = None
              Formula = None }
          Inspection = None
          Patch = Map.empty
          Worker = WorkerStarting
          ActiveOperation = None
          NextOperation = 1
          Announcement = "Choose a replay package to begin." }

    let private beginOperation request model =
        let operation = OperationId model.NextOperation
        let cancellation =
            model.ActiveOperation
            |> Option.map (fun active -> Run(active, Cancel))
            |> Option.toList

        { model with
            ActiveOperation = Some operation
            NextOperation = model.NextOperation + 1 },
        cancellation @ [ Run(operation, request) ]

    let private stopOperation model =
        { model with ActiveOperation = None }

    let private rejectSource reason source =
        match source with
        | Reading sourceName -> Rejected(sourceName, reason)
        | NoSource
        | Loaded _
        | Rejected _ -> source

    let private clampTick finalTick tick =
        max 0 (min finalTick tick)

    let private advanceSize speed =
        match speed with
        | Half
        | Normal -> 1
        | Double -> 2
        | Maximum -> 2_048

    let private sourceIdentity model =
        match model.Source with
        | Loaded metadata -> metadata.SourceIdentity
        | _ -> "unloaded"

    let private derivedIdentity model patch =
        let suffix =
            patch
            |> Map.toList
            |> List.map (fun (name, value) -> name + "=" + string value)
            |> String.concat ";"

        sourceIdentity model + ":fork:" + suffix

    let private applyRunnerResponse response model =
        match response with
        | LoadedPackage(metadata, claim, inspection) ->
            let mode, verification, announcement =
                match claim with
                | KernelVerified ->
                    VerifiedReplay,
                    BrowserKernelVerified,
                    "Replay loaded and browser-kernel verified."
                | ProjectionOnly ->
                    PerspectivePlayback,
                    PerspectiveReady,
                    "Perspective playback loaded. Hidden world state is unavailable."
                | ScenarioReady ->
                    ScenarioSandbox metadata.SourceIdentity,
                    SandboxDerived metadata.SourceIdentity,
                    "Design scenario loaded as a sandbox."

            { model with
                Source = Loaded metadata
                Mode = mode
                Verification = verification
                Playback =
                    { model.Playback with
                        CurrentTick = 0
                        FinalTick = metadata.FinalTick
                        IsPlaying = false }
                Inspection = Some inspection
                Worker = WorkerReady
                Announcement = announcement }
            |> stopOperation
        | RunnerProgress(tick, completedBatches, inspection) ->
            let tick = clampTick model.Playback.FinalTick tick

            { model with
                Playback = { model.Playback with CurrentTick = tick }
                Inspection = Some inspection
                Worker = WorkerBusy completedBatches
                Announcement =
                    "Worker completed batch "
                    + string completedBatches
                    + " at tick "
                    + string tick
                    + "." }
        | Progressed(tick, inspection) ->
            let tick = clampTick model.Playback.FinalTick tick

            { model with
                Playback =
                    { model.Playback with
                        CurrentTick = tick
                        IsPlaying =
                            model.Playback.IsPlaying
                            && tick < model.Playback.FinalTick }
                Inspection = Some inspection
                Worker = WorkerReady
                Announcement = "Playback moved to tick " + string tick + "." }
            |> stopOperation
        | Forked identity ->
            { model with
                Mode = SandboxFork identity
                Verification = SandboxDerived identity
                Worker = WorkerReady
                Announcement = "Sandbox fork created. Verification no longer applies." }
            |> stopOperation
        | RunnerUnsupported reason ->
            { model with
                Source = rejectSource reason model.Source
                Verification = Unsupported reason
                Playback = { model.Playback with IsPlaying = false }
                Worker = WorkerReady
                Announcement = "Unsupported replay: " + reason }
            |> stopOperation
        | RunnerDiverged(tick, phase, detail) ->
            { model with
                Source =
                    rejectSource
                        ("diverged at tick " + string tick + " during " + phase)
                        model.Source
                Verification = Diverged(tick, phase, detail)
                Playback =
                    { model.Playback with
                        CurrentTick = clampTick model.Playback.FinalTick tick
                        IsPlaying = false }
                Worker = WorkerReady
                Announcement =
                    "Replay diverged at tick "
                    + string tick
                    + " during "
                    + phase
                    + "." }
            |> stopOperation
        | RunnerFailed reason ->
            { model with
                Source = rejectSource reason model.Source
                Verification = Failed reason
                Playback = { model.Playback with IsPlaying = false }
                Worker = WorkerReady
                Announcement = "Replay failed: " + reason }
            |> stopOperation

    let update msg model =
        match msg with
        | ReplayBytesSelected(sourceName, bytes) ->
            let loading =
                { model with
                    Source = Reading sourceName
                    Mode = NoRun
                    Verification = Loading
                    Inspection = None
                    Patch = Map.empty
                    Worker = WorkerBusy 0
                    Playback =
                        { model.Playback with
                            CurrentTick = 0
                            FinalTick = 0
                            IsPlaying = false }
                    Announcement = "Loading " + sourceName + "." }

            beginOperation (LoadPackage(sourceName, bytes)) loading
        | RunnerResponded(operation, response) ->
            if model.ActiveOperation = Some operation then
                applyRunnerResponse response model, []
            else
                model, []
        | TogglePlayback when model.Playback.FinalTick > 0 ->
            { model with
                Playback =
                    { model.Playback with
                        IsPlaying = not model.Playback.IsPlaying }
                Announcement =
                    if model.Playback.IsPlaying then
                        "Playback paused."
                    else
                        "Playback started." },
            []
        | StepForward when model.Playback.CurrentTick < model.Playback.FinalTick ->
            beginOperation
                (Advance(
                    model.Playback.CurrentTick,
                    1,
                    model.Playback.FinalTick
                ))
                { model with
                    Playback = { model.Playback with IsPlaying = false } }
        | SeekRequested tick when model.Playback.FinalTick > 0 ->
            beginOperation
                (Seek(tick, model.Playback.FinalTick))
                { model with
                    Playback = { model.Playback with IsPlaying = false }
                    Announcement = "Seeking replay." }
        | SpeedChanged speed ->
            { model with
                Playback = { model.Playback with Speed = speed }
                Announcement = "Playback speed changed." },
            []
        | UnitSelected unitId ->
            { model with Selection = { model.Selection with Unit = unitId } }, []
        | EventSelected eventId ->
            { model with Selection = { model.Selection with Event = eventId } }, []
        | FormulaSelected formula ->
            { model with
                Selection = { model.Selection with Formula = formula } },
            []
        | ParameterEdited(name, value)
            when
                match model.Mode with
                | VerifiedReplay
                | SandboxFork _
                | ScenarioSandbox _ -> true
                | NoRun
                | PerspectivePlayback -> false
            ->
            let patch = model.Patch |> Map.add name value
            let identity = derivedIdentity model patch

            let forked =
                { model with
                    Patch = patch
                    Mode = SandboxFork identity
                    Verification = SandboxDerived identity
                    Playback = { model.Playback with IsPlaying = false }
                    Announcement =
                        "Parameter edited. This run is now a sandbox fork." }

            beginOperation (Fork(identity, patch)) forked
        | CancelRequested ->
            let effects =
                model.ActiveOperation
                |> Option.map (fun operation -> Run(operation, Cancel))
                |> Option.toList

            { model with
                ActiveOperation = None
                Playback = { model.Playback with IsPlaying = false }
                Worker = WorkerReady
                Announcement = "Operation cancelled." },
            effects
        | WorkerStarted ->
            { model with
                Worker = WorkerReady
                Announcement = "Replay worker ready." },
            []
        | WorkerTerminated reason ->
            { model with
                Verification = Failed("worker stopped: " + reason)
                Playback = { model.Playback with IsPlaying = false }
                Worker = WorkerStopped reason
                ActiveOperation = None
                Announcement =
                    "Replay worker stopped. Verification has been revoked." },
            []
        | TogglePlayback
        | StepForward
        | SeekRequested _
        | ParameterEdited _ ->
            model, []

    let playbackTick model =
        if model.Playback.IsPlaying && Option.isNone model.ActiveOperation then
            beginOperation
                (Advance(
                    model.Playback.CurrentTick,
                    advanceSize model.Playback.Speed,
                    model.Playback.FinalTick
                ))
                model
        else
            model, []
