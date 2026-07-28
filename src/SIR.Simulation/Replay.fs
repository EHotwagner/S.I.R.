namespace SIR.Simulation

open FS.GG.Game.Core
open SIR.Domain

/// Disclosure scope is encoded in the replay type, not inferred by the UI.
type ReplayContent =
    | AuthorizedFullReplay of FullReplay
    | PerspectivePlayback of PerspectiveFrame list

/// One accepted external input after authoritative ordering.
and ReplayInput =
    { Tick: int32
      Sequence: int32
      Input: KernelInput }

/// One accepted player-WASM output at the kernel boundary.
and AcceptedWasmOutput =
    { Tick: int32
      Sequence: int32
      Input: KernelInput }

/// A retained, independently verifiable seek point.
and ReplayCheckpoint =
    { Tick: int32
      State: SimulationState
      StateHash: byte array
      EventHash: byte array }

/// The canonical terminal claim made by an authorized full replay.
and ReplayFinalResult =
    { Tick: int32
      OutcomeCode: int32
      StateHash: byte array
      EventHash: byte array }

/// Kernel material that is deliberately absent from perspective playback.
and FullReplay =
    { InitialSnapshot: SimulationState
      OrderedInputs: ReplayInput list
      AcceptedWasmOutputs: AcceptedWasmOutput list
      Checkpoints: ReplayCheckpoint list
      FinalResult: ReplayFinalResult }

/// Knowledge-filtered playback material with no reconstructable kernel state.
and PerspectiveFrame =
    { Tick: int32
      ProjectionHash: byte array }

/// Versioned replay package header and disclosure-specific payload.
type ReplayPackage =
    { FormatVersion: int32
      EngineHash: byte array
      RulesetHash: byte array
      FullReplayAuthorized: bool
      Content: ReplayContent }

/// Resource limits applied before and during package decoding.
type ReplayLimits =
    { MaxPackageBytes: int
      MaxInputs: int
      MaxWasmOutputs: int
      MaxCheckpoints: int
      MaxPerspectiveFrames: int
      MaxUnits: int
      MaxEdges: int
      MaxObservations: int }

/// Why an untrusted replay package was rejected.
type ReplayError =
    | PackageTooLarge of actual: int * maximum: int
    | MalformedPackage of detail: string
    | UnsupportedFormat of actual: int32 * supported: int32
    | InvalidHashLength of field: string * actual: int
    | EngineMismatch of expected: byte array * actual: byte array
    | UnauthorizedFullReplay
    | ResourceLimitExceeded of field: string * actual: int * maximum: int
    | InvalidOrdering of field: string
    | InvalidCheckpoint of tick: int32 * detail: string
    | ReplayDivergence of tick: int32 * field: string
    | PerspectiveHasNoKernel
    | WasmExecutionNotVerified

/// The verification claim is explicit in the returned value.
type ReplayVerification =
    | BrowserKernelVerified of ReplayFinalResult
    | AuthoritativeVerified of ReplayFinalResult
    | PerspectiveReady of PerspectiveFrame list

exception private ReplayDecodeFailure of string

type private ReplayReader =
    { Bytes: byte array
      mutable Offset: int }

/// Canonical replay schema, validation, and deterministic runners.
[<RequireQualifiedAccess>]
module Replay =
    [<Literal>]
    let CurrentFormatVersion = 1

    let defaultLimits =
        { MaxPackageBytes = 1_048_576
          MaxInputs = 16_384
          MaxWasmOutputs = 16_384
          MaxCheckpoints = 4_096
          MaxPerspectiveFrames = 65_536
          MaxUnits = 4_096
          MaxEdges = 16_384
          MaxObservations = 65_536 }

    let private magic = [| 0x53uy; 0x49uy; 0x52uy; 0x52uy |]
    let private requireHash field (hash: byte array) =
        if hash.Length = 32 then
            Ok()
        else
            Error(InvalidHashLength(field, hash.Length))

    let private cellBytes (cell: Cell) =
        CanonicalEncoding.concatenate
            [ CanonicalEncoding.int32LittleEndian cell.Col
              CanonicalEncoding.int32LittleEndian cell.Row ]

    let private unitIdBytes id =
        id |> Simulation.unitIdValue |> CanonicalEncoding.int32LittleEndian

    let private sideByte side =
        match side with
        | Red -> 0uy
        | Blue -> 1uy

    let private inputBytes input =
        match input with
        | Move(unitId, destination) ->
            CanonicalEncoding.concatenate
                [ [| 0uy |]; unitIdBytes unitId; cellBytes destination ]
        | Observe(observerId, targetId) ->
            CanonicalEncoding.concatenate
                [ [| 1uy |]; unitIdBytes observerId; unitIdBytes targetId ]
        | Attack(attackerId, targetId) ->
            CanonicalEncoding.concatenate
                [ [| 2uy |]; unitIdBytes attackerId; unitIdBytes targetId ]

    /// Complete version-1 snapshot encoding, including board semantics.
    let snapshotBytes (state: SimulationState) =
        let edgeSegments =
            state.Board.Edges
            |> List.sortBy (fun edge ->
                edge.Edge.Lo.Col,
                edge.Edge.Lo.Row,
                edge.Edge.Hi.Col,
                edge.Edge.Hi.Row,
                edge.BlocksMovement)
            |> List.collect (fun edge ->
                [ cellBytes edge.Edge.Lo
                  cellBytes edge.Edge.Hi
                  [| if edge.BlocksMovement then 1uy else 0uy |] ])

        let unitSegments =
            state.Units
            |> Map.toList
            |> List.collect (fun (unitId, unit) ->
                [ unitIdBytes unitId
                  [| sideByte unit.Side |]
                  cellBytes unit.Cell
                  CanonicalEncoding.boundedInt32 unit.Health ])

        let observationSegments =
            state.Observations
            |> Set.toList
            |> List.collect (fun (observerId, targetId) ->
                [ unitIdBytes observerId; unitIdBytes targetId ])

        CanonicalEncoding.concatenate
            ([ CanonicalEncoding.int32LittleEndian state.Tick
               cellBytes state.Board.Minimum
               cellBytes state.Board.Maximum
               CanonicalEncoding.int32LittleEndian state.Board.Edges.Length ]
             @ edgeSegments
             @ [ CanonicalEncoding.int32LittleEndian state.Units.Count ]
             @ unitSegments
             @ [ CanonicalEncoding.int32LittleEndian state.Observations.Count ]
             @ observationSegments)

    let stateHash state = state |> snapshotBytes |> CanonicalHash.sha256
    let eventHash events = events |> Simulation.eventsBytes |> CanonicalHash.sha256

    let private lengthPrefixed (bytes: byte array) =
        CanonicalEncoding.concatenate
            [ CanonicalEncoding.int32LittleEndian bytes.Length; bytes ]

    let private replayInputBytes (input: ReplayInput) =
        CanonicalEncoding.concatenate
            [ CanonicalEncoding.int32LittleEndian input.Tick
              CanonicalEncoding.int32LittleEndian input.Sequence
              inputBytes input.Input ]

    let private wasmOutputBytes (output: AcceptedWasmOutput) =
        CanonicalEncoding.concatenate
            [ CanonicalEncoding.int32LittleEndian output.Tick
              CanonicalEncoding.int32LittleEndian output.Sequence
              inputBytes output.Input ]

    let private checkpointBytes (checkpoint: ReplayCheckpoint) =
        CanonicalEncoding.concatenate
            [ CanonicalEncoding.int32LittleEndian checkpoint.Tick
              checkpoint.State |> snapshotBytes |> lengthPrefixed
              checkpoint.StateHash
              checkpoint.EventHash ]

    let private fullReplayBytes (full: FullReplay) =
        let inputSegments = full.OrderedInputs |> List.map replayInputBytes
        let wasmSegments = full.AcceptedWasmOutputs |> List.map wasmOutputBytes
        let checkpointSegments = full.Checkpoints |> List.map checkpointBytes

        CanonicalEncoding.concatenate
            ([ full.InitialSnapshot |> snapshotBytes |> lengthPrefixed
               CanonicalEncoding.int32LittleEndian full.OrderedInputs.Length ]
             @ inputSegments
             @ [ CanonicalEncoding.int32LittleEndian full.AcceptedWasmOutputs.Length ]
             @ wasmSegments
             @ [ CanonicalEncoding.int32LittleEndian full.Checkpoints.Length ]
             @ checkpointSegments
             @ [ CanonicalEncoding.int32LittleEndian full.FinalResult.Tick
                 CanonicalEncoding.int32LittleEndian full.FinalResult.OutcomeCode
                 full.FinalResult.StateHash
                 full.FinalResult.EventHash ])

    let private perspectiveBytes (frames: PerspectiveFrame list) =
        CanonicalEncoding.concatenate
            ([ CanonicalEncoding.int32LittleEndian frames.Length ]
             @ (frames
                |> List.collect (fun frame ->
                    [ CanonicalEncoding.int32LittleEndian frame.Tick
                      frame.ProjectionHash ])))

    /// Encodes a package in the stable version-1 binary format.
    let encode package =
        let disclosure, payload =
            match package.Content with
            | AuthorizedFullReplay full -> 0uy, fullReplayBytes full
            | PerspectivePlayback frames -> 1uy, perspectiveBytes frames

        CanonicalEncoding.concatenate
            [ magic
              CanonicalEncoding.int32LittleEndian package.FormatVersion
              [| disclosure |]
              package.EngineHash
              package.RulesetHash
              [| if package.FullReplayAuthorized then 1uy else 0uy |]
              payload ]

    let private failDecode detail = raise (ReplayDecodeFailure detail)

    let private readBytes count reader =
        if count < 0 || reader.Offset > reader.Bytes.Length - count then
            failDecode "Unexpected end of package."

        let value = reader.Bytes[reader.Offset .. reader.Offset + count - 1]
        reader.Offset <- reader.Offset + count
        value

    let private readByte reader = (readBytes 1 reader)[0]

    let private readInt32 reader =
        let bytes = readBytes 4 reader

        int32 bytes[0]
        ||| (int32 bytes[1] <<< 8)
        ||| (int32 bytes[2] <<< 16)
        ||| (int32 bytes[3] <<< 24)

    let private readCell reader: Cell =
        { Col = readInt32 reader
          Row = readInt32 reader }

    let private readCount field maximum reader =
        let count = readInt32 reader

        if count < 0 || count > int32 maximum then
            failDecode (
                sprintf
                    "Resource limit exceeded for %s: %d is outside 0..%d."
                    field
                    count
                    maximum
            )

        int count

    let private readBool reader =
        match readByte reader with
        | 0uy -> false
        | 1uy -> true
        | value -> failDecode (sprintf "Invalid Boolean byte %d." value)

    let private readSide reader =
        match readByte reader with
        | 0uy -> Red
        | 1uy -> Blue
        | value -> failDecode (sprintf "Invalid side byte %d." value)

    let private readHealth reader =
        match BoundedInt32.create 0 100 (readInt32 reader) with
        | Ok health -> health
        | Error _ -> failDecode "Unit health is outside 0..100."

    let private readInput reader =
        match readByte reader with
        | 0uy -> Move(Simulation.unitId (readInt32 reader), readCell reader)
        | 1uy ->
            Observe(
                Simulation.unitId (readInt32 reader),
                Simulation.unitId (readInt32 reader)
            )
        | 2uy ->
            Attack(
                Simulation.unitId (readInt32 reader),
                Simulation.unitId (readInt32 reader)
            )
        | value -> failDecode (sprintf "Invalid kernel-input tag %d." value)

    let private readSnapshot limits reader =
        let declaredLength = readInt32 reader

        if declaredLength < 0
           || int declaredLength > reader.Bytes.Length - reader.Offset then
            failDecode "Invalid snapshot length."

        let boundary = reader.Offset + int declaredLength
        let tick = readInt32 reader
        let minimum = readCell reader
        let maximum = readCell reader
        let edgeCount = readCount "edges" limits.MaxEdges reader

        let edges =
            [ for _ in 1 .. edgeCount do
                  let left = readCell reader
                  let right = readCell reader

                  let edge =
                      Edges.edgeBetween left right
                      |> Option.defaultWith (fun () ->
                          failDecode "A semantic edge is not orthogonal.")

                  yield
                      { Edge = edge
                        BlocksMovement = readBool reader } ]

        let unitCount = readCount "units" limits.MaxUnits reader

        let units =
            [ for _ in 1 .. unitCount do
                  let unitId = Simulation.unitId (readInt32 reader)

                  yield
                      unitId,
                      { Id = unitId
                        Side = readSide reader
                        Cell = readCell reader
                        Health = readHealth reader } ]

        if units |> List.map fst |> Set.ofList |> Set.count <> unitCount then
            failDecode "Snapshot contains duplicate unit identifiers."

        let observationCount =
            readCount "observations" limits.MaxObservations reader

        let observations =
            [ for _ in 1 .. observationCount do
                  yield
                      Simulation.unitId (readInt32 reader),
                      Simulation.unitId (readInt32 reader) ]
            |> Set.ofList

        if observations.Count <> observationCount then
            failDecode "Snapshot contains duplicate observations."

        if reader.Offset <> boundary then
            failDecode "Snapshot length does not match its canonical fields."

        { Tick = tick
          Board =
            { Minimum = minimum
              Maximum = maximum
              Edges = edges }
          Units = Map.ofList units
          Observations = observations }

    let private readReplayInput reader : ReplayInput =
        { Tick = readInt32 reader
          Sequence = readInt32 reader
          Input = readInput reader }

    let private readWasmOutput reader : AcceptedWasmOutput =
        { Tick = readInt32 reader
          Sequence = readInt32 reader
          Input = readInput reader }

    let private readCheckpoint limits reader =
        { Tick = readInt32 reader
          State = readSnapshot limits reader
          StateHash = readBytes 32 reader
          EventHash = readBytes 32 reader }

    let private readFull limits reader : FullReplay =
        let initial = readSnapshot limits reader
        let inputCount = readCount "inputs" limits.MaxInputs reader
        let inputs: ReplayInput list =
            [ for _ in 1 .. inputCount -> readReplayInput reader ]

        let wasmCount =
            readCount "accepted WASM outputs" limits.MaxWasmOutputs reader

        let wasm: AcceptedWasmOutput list =
            [ for _ in 1 .. wasmCount -> readWasmOutput reader ]
        let checkpointCount = readCount "checkpoints" limits.MaxCheckpoints reader
        let checkpoints = [ for _ in 1 .. checkpointCount -> readCheckpoint limits reader ]

        { InitialSnapshot = initial
          OrderedInputs = inputs
          AcceptedWasmOutputs = wasm
          Checkpoints = checkpoints
          FinalResult =
            { Tick = readInt32 reader
              OutcomeCode = readInt32 reader
              StateHash = readBytes 32 reader
              EventHash = readBytes 32 reader } }

    let private readPerspective limits reader : PerspectiveFrame list =
        let count =
            readCount "perspective frames" limits.MaxPerspectiveFrames reader

        [ for _ in 1 .. count do
              yield
                  { Tick = readInt32 reader
                    ProjectionHash = readBytes 32 reader } ]

    /// Decodes untrusted bytes with strict bounds and no partial acceptance.
    let decode (limits: ReplayLimits) (bytes: byte array) =
        if bytes.Length > limits.MaxPackageBytes then
            Error(PackageTooLarge(bytes.Length, limits.MaxPackageBytes))
        else
            try
                let reader = { Bytes = bytes; Offset = 0 }

                if readBytes magic.Length reader <> magic then
                    failDecode "Replay magic is invalid."

                let version = readInt32 reader
                let disclosure = readByte reader
                let engineHash = readBytes 32 reader
                let rulesetHash = readBytes 32 reader
                let fullReplayAuthorized = readBool reader

                let content =
                    match disclosure with
                    | 0uy -> readFull limits reader |> AuthorizedFullReplay
                    | 1uy -> readPerspective limits reader |> PerspectivePlayback
                    | value -> failDecode (sprintf "Invalid disclosure byte %d." value)

                if reader.Offset <> bytes.Length then
                    failDecode "Trailing bytes are not permitted."

                Ok
                    { FormatVersion = version
                      EngineHash = engineHash
                      RulesetHash = rulesetHash
                      FullReplayAuthorized = fullReplayAuthorized
                      Content = content }
            with
            | ReplayDecodeFailure detail -> Error(MalformedPackage detail)

    let private ordered field entries =
        let keys = entries |> List.map (fun (tick, sequence) -> tick, sequence)

        (keys = List.sort keys
         && keys.Length = (keys |> Set.ofList |> Set.count))
        |> function
            | true -> Ok()
            | false -> Error(InvalidOrdering field)

    let private validateHeader (expectedEngine: byte array) (package: ReplayPackage) =
        if package.FormatVersion <> int32 CurrentFormatVersion then
            Error(
                UnsupportedFormat(
                    package.FormatVersion,
                    int32 CurrentFormatVersion
                )
            )
        elif package.EngineHash <> expectedEngine then
            Error(EngineMismatch(expectedEngine, package.EngineHash))
        else
            match requireHash "engine" package.EngineHash with
            | Error error -> Error error
            | Ok() -> requireHash "ruleset" package.RulesetHash

    let private validateFull (limits: ReplayLimits) (full: FullReplay) =
        let limit field maximum values =
            if List.length values > maximum then
                Error(ResourceLimitExceeded(field, List.length values, maximum))
            else
                Ok()

        match limit "inputs" limits.MaxInputs full.OrderedInputs with
        | Error error -> Error error
        | Ok() ->
            match
                limit
                    "accepted WASM outputs"
                    limits.MaxWasmOutputs
                    full.AcceptedWasmOutputs
            with
            | Error error -> Error error
            | Ok() ->
                match limit "checkpoints" limits.MaxCheckpoints full.Checkpoints with
                | Error error -> Error error
                | Ok() ->
                    match
                        ordered
                            "inputs"
                            (full.OrderedInputs
                             |> List.map (fun input -> input.Tick, input.Sequence))
                    with
                    | Error error -> Error error
                    | Ok() ->
                        match
                            ordered
                                "accepted WASM outputs"
                                (full.AcceptedWasmOutputs
                                 |> List.map (fun output ->
                                     output.Tick, output.Sequence))
                        with
                        | Error error -> Error error
                        | Ok() ->
                            let combinedJournalKeys =
                                (full.OrderedInputs
                                 |> List.map (fun input -> input.Tick, input.Sequence))
                                @ (full.AcceptedWasmOutputs
                                   |> List.map (fun output ->
                                       output.Tick, output.Sequence))

                            let checkpointTicks =
                                full.Checkpoints |> List.map (fun checkpoint -> checkpoint.Tick)

                            if
                                combinedJournalKeys.Length
                                <> (combinedJournalKeys |> Set.ofList |> Set.count)
                            then
                                Error(InvalidOrdering "combined input journal")
                            elif
                                combinedJournalKeys
                                |> List.exists (fun (tick, _) ->
                                    tick <= full.InitialSnapshot.Tick
                                    || tick > full.FinalResult.Tick)
                            then
                                Error(InvalidOrdering "input ticks")
                            elif checkpointTicks <> List.sort checkpointTicks
                               || checkpointTicks.Length
                                  <> (checkpointTicks |> Set.ofList |> Set.count) then
                                Error(InvalidOrdering "checkpoints")
                            elif
                                checkpointTicks
                                |> List.exists (fun tick ->
                                    tick < full.InitialSnapshot.Tick
                                    || tick > full.FinalResult.Tick)
                            then
                                Error(InvalidOrdering "checkpoint ticks")
                            elif full.FinalResult.Tick < full.InitialSnapshot.Tick then
                                Error(InvalidCheckpoint(full.FinalResult.Tick, "Final tick precedes the initial snapshot."))
                            elif
                                full.Checkpoints
                                |> List.exists (fun checkpoint ->
                                    checkpoint.Tick <> checkpoint.State.Tick)
                            then
                                let checkpoint =
                                    full.Checkpoints
                                    |> List.find (fun checkpoint ->
                                        checkpoint.Tick <> checkpoint.State.Tick)

                                Error(
                                    InvalidCheckpoint(
                                        checkpoint.Tick,
                                        "Checkpoint tick does not match its snapshot."
                                    )
                                )
                            elif
                                full.Checkpoints
                                |> List.exists (fun checkpoint ->
                                    checkpoint.StateHash <> stateHash checkpoint.State)
                            then
                                let checkpoint =
                                    full.Checkpoints
                                    |> List.find (fun checkpoint ->
                                        checkpoint.StateHash <> stateHash checkpoint.State)

                                Error(
                                    InvalidCheckpoint(
                                        checkpoint.Tick,
                                        "Checkpoint state hash does not match its snapshot."
                                    )
                                )
                            else
                                let snapshots =
                                    full.InitialSnapshot
                                    :: (full.Checkpoints
                                        |> List.map (fun checkpoint ->
                                            checkpoint.State))

                                let snapshotLimitError =
                                    snapshots
                                    |> List.tryPick (fun state ->
                                        if state.Units.Count > limits.MaxUnits then
                                            Some(
                                                ResourceLimitExceeded(
                                                    "units",
                                                    state.Units.Count,
                                                    limits.MaxUnits
                                                )
                                            )
                                        elif state.Board.Edges.Length > limits.MaxEdges then
                                            Some(
                                                ResourceLimitExceeded(
                                                    "edges",
                                                    state.Board.Edges.Length,
                                                    limits.MaxEdges
                                                )
                                            )
                                        elif state.Observations.Count > limits.MaxObservations then
                                            Some(
                                                ResourceLimitExceeded(
                                                    "observations",
                                                    state.Observations.Count,
                                                    limits.MaxObservations
                                                )
                                            )
                                        else
                                            None)

                                match snapshotLimitError with
                                | Some error -> Error error
                                | None ->
                                    [ "final state", full.FinalResult.StateHash
                                      "final events", full.FinalResult.EventHash ]
                                    @ (full.Checkpoints
                                       |> List.collect (fun checkpoint ->
                                           [ "checkpoint state", checkpoint.StateHash
                                             "checkpoint events", checkpoint.EventHash ]))
                                    |> List.tryPick (fun (field, hash) ->
                                        match requireHash field hash with
                                        | Ok() -> None
                                        | Error error -> Some error)
                                    |> function
                                        | Some error -> Error error
                                        | None -> Ok()

    let private validatePerspective limits frames =
        if List.length frames > limits.MaxPerspectiveFrames then
            Error(
                ResourceLimitExceeded(
                    "perspective frames",
                    List.length frames,
                    limits.MaxPerspectiveFrames
                )
            )
        elif frames |> List.map (fun frame -> frame.Tick) <> (frames |> List.map (fun frame -> frame.Tick) |> List.sort) then
            Error(InvalidOrdering "perspective frames")
        else
            frames
            |> List.tryPick (fun frame ->
                match requireHash "projection" frame.ProjectionHash with
                | Ok() -> None
                | Error error -> Some error)
            |> function
                | Some error -> Error error
                | None -> Ok()

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

    let private checkpointAt tick (full: FullReplay) =
        full.Checkpoints |> List.tryFind (fun checkpoint -> checkpoint.Tick = tick)

    let private replayFrom (full: FullReplay) (start: SimulationState) =
        let mutable state = start
        let mutable lastEvents = []
        let mutable failure: ReplayError option = None

        for tick in start.Tick + 1 .. full.FinalResult.Tick do
            if Option.isNone failure then
                let result = Simulation.runTick state (journalAt tick full)
                let actualStateHash = stateHash result.State
                let actualEventHash = eventHash result.Events

                match checkpointAt tick full with
                | Some checkpoint when checkpoint.StateHash <> actualStateHash ->
                    failure <- Some(ReplayDivergence(tick, "checkpoint state hash"))
                | Some checkpoint when checkpoint.EventHash <> actualEventHash ->
                    failure <- Some(ReplayDivergence(tick, "checkpoint event hash"))
                | Some checkpoint when snapshotBytes checkpoint.State <> snapshotBytes result.State ->
                    failure <- Some(ReplayDivergence(tick, "checkpoint snapshot"))
                | _ ->
                    state <- result.State
                    lastEvents <- result.Events

        match failure with
        | Some error -> Error error
        | None ->
            let actualStateHash = stateHash state
            let actualEventHash = eventHash lastEvents

            if actualStateHash <> full.FinalResult.StateHash then
                Error(ReplayDivergence(full.FinalResult.Tick, "final state hash"))
            elif actualEventHash <> full.FinalResult.EventHash then
                Error(ReplayDivergence(full.FinalResult.Tick, "final event hash"))
            else
                Ok full.FinalResult

    let private verifyAllSeekPoints (full: FullReplay) =
        let starts =
            full.InitialSnapshot
            :: (full.Checkpoints
                |> List.filter (fun checkpoint ->
                    checkpoint.Tick < full.FinalResult.Tick)
                |> List.map (fun checkpoint -> checkpoint.State))

        starts
        |> List.tryPick (fun state ->
            match replayFrom full state with
            | Ok _ -> None
            | Error error -> Some error)
        |> function
            | Some error -> Error error
            | None -> Ok full.FinalResult

    /// Runs the shared kernel from the initial snapshot and every retained checkpoint.
    let runKernelReplay limits expectedEngine package =
        match validateHeader expectedEngine package with
        | Error error -> Error error
        | Ok() ->
            match package.Content with
            | PerspectivePlayback frames ->
                match validatePerspective limits frames with
                | Ok() -> Ok(PerspectiveReady frames)
                | Error error -> Error error
            | AuthorizedFullReplay _ when not package.FullReplayAuthorized ->
                Error UnauthorizedFullReplay
            | AuthorizedFullReplay full ->
                match validateFull limits full with
                | Error error -> Error error
                | Ok() ->
                    match verifyAllSeekPoints full with
                    | Ok finalResult -> Ok(BrowserKernelVerified finalResult)
                    | Error error -> Error error

    /// Adds the stronger authoritative claim only after exact WASM re-execution.
    let verifyAuthoritative limits expectedEngine wasmExecutionVerified package =
        if not wasmExecutionVerified then
            Error WasmExecutionNotVerified
        else
            match package.Content with
            | PerspectivePlayback _ -> Error PerspectiveHasNoKernel
            | AuthorizedFullReplay _ ->
                match runKernelReplay limits expectedEngine package with
                | Ok(BrowserKernelVerified finalResult) ->
                    Ok(AuthoritativeVerified finalResult)
                | Ok _ -> Error PerspectiveHasNoKernel
                | Error error -> Error error

    /// Explicitly rejects attempts to obtain kernel verification from perspective data.
    let requireKernel package =
        match package.Content with
        | PerspectivePlayback _ -> Error PerspectiveHasNoKernel
        | AuthorizedFullReplay _ when not package.FullReplayAuthorized ->
            Error UnauthorizedFullReplay
        | AuthorizedFullReplay full -> Ok full

    /// Builds a zero-event hash used by the initial retained checkpoint.
    let emptyEventHash = eventHash []
