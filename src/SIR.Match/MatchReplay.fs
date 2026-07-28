namespace SIR.Match

open System
open Wasmtime
open SIR.Domain
open SIR.Simulation

/// Immutable player module and the pinned execution profile used to run it.
type WasmArtifactEvidence =
    { ArtifactBytes: byte array
      ArtifactHash: byte array
      ExecutionProfileHash: byte array
      ControlledUnit: UnitId }

/// A completed qualification match and its disclosure-specific packages.
type MatchQualification =
    { FullPackage: ReplayPackage
      PerspectivePackage: ReplayPackage
      Artifact: WasmArtifactEvidence
      ReexecutedOutputs: AcceptedWasmOutput list }

/// The bounded authoritative host used to qualify full-match replay.
[<RequireQualifiedAccess>]
module MatchReplay =
    let private engineHash = [| for value in 1 .. 32 -> byte value |]

    let private rulesetHash =
        CanonicalHash.sha256 (
            Text.Encoding.UTF8.GetBytes(
                "sir-qualification-rules-v1;tick-hz=20;attack-power=25"
            )
        )

    let private executionProfileHash =
        CanonicalHash.sha256 (
            Text.Encoding.UTF8.GetBytes(
                "wasmtime=44.0.0;core-only=true;fuel=10000;wasi=false"
            )
        )

    // Binary WebAssembly for:
    // (module (func (export "decide") (param i32) (result i32) i32.const 1))
    // Action code 1 requests an attack against the only disclosed opponent.
    let private controllerArtifact =
        [| 0x00uy; 0x61uy; 0x73uy; 0x6duy; 0x01uy; 0x00uy; 0x00uy; 0x00uy
           0x01uy; 0x06uy; 0x01uy; 0x60uy; 0x01uy; 0x7fuy; 0x01uy; 0x7fuy
           0x03uy; 0x02uy; 0x01uy; 0x00uy
           0x07uy; 0x0auy; 0x01uy; 0x06uy; 0x64uy; 0x65uy; 0x63uy; 0x69uy
           0x64uy; 0x65uy; 0x00uy; 0x00uy
           0x0auy; 0x06uy; 0x01uy; 0x04uy; 0x00uy; 0x41uy; 0x01uy; 0x0buy |]

    let private createEngine () =
        let config = new Config()
        config.WithFuelConsumption(true) |> ignore
        config.WithReferenceTypes(false) |> ignore
        config.WithBulkMemory(false) |> ignore
        config.WithSIMD(false) |> ignore
        config.WithRelaxedSIMD(false, false) |> ignore
        config.WithMultiValue(false) |> ignore
        config.WithMultiMemory(false) |> ignore
        config.WithWasmThreads(false) |> ignore
        config.WithTailCalls(false) |> ignore
        config.WithComponentModel(false) |> ignore
        new Engine(config)

    let private executeArtifact (artifact: WasmArtifactEvidence) finalTick =
        if CanonicalHash.sha256 artifact.ArtifactBytes <> artifact.ArtifactHash then
            invalidArg (nameof artifact) "The control artifact identity does not match its bytes."

        if artifact.ExecutionProfileHash <> executionProfileHash then
            invalidArg (nameof artifact) "The execution profile is not the qualification profile."

        use engine = createEngine ()
        use compiled =
            Module.FromBytes(engine, "qualification-controller", artifact.ArtifactBytes)
        use linker = new Linker(engine)
        use store = new Store(engine)
        store.Fuel <- 10_000UL
        let instance = linker.Instantiate(store, compiled)
        let decide =
            match instance.GetFunction("decide") with
            | null -> failwith "The qualification artifact does not export decide."
            | value -> value

        [ for tick in 1 .. finalTick do
              store.Fuel <- 10_000UL

              let actionCode =
                  match decide.Invoke(tick) with
                  | :? int as value -> value
                  | value -> failwithf "Unexpected WASM decision value %A." value

              if actionCode <> 1 then
                  failwithf "Unsupported qualification action code %d." actionCode

              yield
                  { Tick = tick
                    Sequence = tick
                    Input =
                        Attack(
                            artifact.ControlledUnit,
                            Simulation.unitId 20
                        ) } ]

    let private checkpoint tick state events : ReplayCheckpoint =
        { Tick = tick
          State = state
          StateHash = Replay.stateHash state
          EventHash = Replay.eventHash events }

    let private perspectiveFrame state : PerspectiveFrame =
        let red = Map.find (Simulation.unitId 10) state.Units

        let projection =
            CanonicalEncoding.concatenate
                [ CanonicalEncoding.int32LittleEndian state.Tick
                  CanonicalEncoding.int32LittleEndian red.Cell.Col
                  CanonicalEncoding.int32LittleEndian red.Cell.Row
                  CanonicalEncoding.boundedInt32 red.Health ]

        { Tick = state.Tick
          ProjectionHash = CanonicalHash.sha256 projection }

    let private runKernel (outputs: AcceptedWasmOutput list) =
        let mutable state = Simulation.initialState
        let mutable lastEvents = []
        let mutable checkpoints = [ checkpoint 0 state [] ]
        let mutable perspectives = [ perspectiveFrame state ]

        for tick in 1 .. 4 do
            let journal =
                outputs
                |> List.choose (fun output ->
                    if output.Tick = tick then Some output.Input else None)

            let result = Simulation.runTick state journal
            state <- result.State
            lastEvents <- result.Events
            perspectives <- perspectiveFrame state :: perspectives

            if tick < 4 then
                checkpoints <- checkpoint tick state lastEvents :: checkpoints

        state,
        lastEvents,
        List.rev checkpoints,
        List.rev perspectives

    /// Runs a completed four-tick match and emits full and knowledge-filtered packages.
    let qualify () =
        let artifact =
            { ArtifactBytes = Array.copy controllerArtifact
              ArtifactHash = CanonicalHash.sha256 controllerArtifact
              ExecutionProfileHash = executionProfileHash
              ControlledUnit = Simulation.unitId 10 }

        let acceptedOutputs = executeArtifact artifact 4
        let finalState, finalEvents, checkpoints, perspectives =
            runKernel acceptedOutputs

        let finalResult =
            { Tick = finalState.Tick
              OutcomeCode = 1
              StateHash = Replay.stateHash finalState
              EventHash = Replay.eventHash finalEvents }

        let full =
            { FormatVersion = int32 Replay.CurrentFormatVersion
              EngineHash = engineHash
              RulesetHash = rulesetHash
              FullReplayAuthorized = true
              Content =
                AuthorizedFullReplay
                    { InitialSnapshot = Simulation.initialState
                      OrderedInputs = []
                      AcceptedWasmOutputs = acceptedOutputs
                      Checkpoints = checkpoints
                      FinalResult = finalResult } }

        let perspective =
            { FormatVersion = int32 Replay.CurrentFormatVersion
              EngineHash = engineHash
              RulesetHash = rulesetHash
              FullReplayAuthorized = false
              Content = PerspectivePlayback perspectives }

        let reexecuted = executeArtifact artifact 4

        { FullPackage = full
          PerspectivePackage = perspective
          Artifact = artifact
          ReexecutedOutputs = reexecuted }
