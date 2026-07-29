namespace SIR.Match

open System
open Wasmtime
open SIR.Domain
open SIR.ControlAbi
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

    let private executionProfileHash = ControlHost.defaultProfile.Identity

    let private controllerArtifact =
        let output =
            V1Codec.encodeOutput
                0
                0
                0u
                0u
                [ CapabilityExecution.engagementRequest
                      1u
                      (PointCapabilityTarget 20)
                      "human.weapon.rifle" ]
                []
            |> Result.defaultWith (fun error ->
                failwithf "Could not build qualification controller: %A" error)

        let data =
            output
            |> Array.map (fun value -> sprintf "\\%02x" value)
            |> String.concat ""

        Module.ConvertText(
            $"""(module
              (memory (export "memory") 2 2)
              (data (i32.const 65536) "{data}")
              (func (export "sir_abi_version") (result i32) i32.const 65536)
              (func (export "sir_input_ptr") (result i32) i32.const 0)
              (func (export "sir_input_capacity") (result i32) i32.const 65536)
              (func (export "sir_output_ptr") (result i32) i32.const 65536)
              (func (export "sir_output_capacity") (result i32) i32.const 16384)
              (func (export "sir_decide") (param i32) (result i32)
                i32.const 65548 i32.const 12 i32.load i32.store
                i32.const 65552 i32.const 16 i32.load i32.store
                i32.const {output.Length}))"""
        )

    let private controlInput tick unitId =
        { Kind = MessageKind.Input
          MinorVersion = V1Constants.Minor
          Tick = tick
          UnitId = unitId
          Flags = 0u
          Budget = uint32 ControlHost.defaultProfile.FuelPerInvocation
          Sections =
            [ { Tag = V1Constants.OwnStateTag
                Required = true
                ElementCount = 1
                Payload = [| 1uy |] } ] }
        |> V1Codec.encode
        |> Result.defaultWith (fun error ->
            failwithf "Could not encode qualification input: %A" error)

    let private executeArtifact
        (artifact: WasmArtifactEvidence)
        (compiled: CompiledControlArtifact)
        finalTick
        =
        if CanonicalHash.sha256 artifact.ArtifactBytes <> artifact.ArtifactHash then
            invalidArg (nameof artifact) "The control artifact identity does not match its bytes."

        if artifact.ExecutionProfileHash <> executionProfileHash then
            invalidArg (nameof artifact) "The execution profile is not the qualification profile."

        let unitId = UnitId.value artifact.ControlledUnit
        use instance = ControlHost.instantiate compiled unitId [||]

        [ for tick in 1 .. finalTick do
              match ControlHost.invoke tick (controlInput tick unitId) instance with
              | Accepted(output, _) when not (List.isEmpty output.Requests) ->
                  yield
                      { Tick = tick
                        Sequence = tick
                        Input =
                            Attack(
                                artifact.ControlledUnit,
                                Simulation.unitId 20
                            ) }
              | result ->
                  failwithf "Qualification control invocation failed: %A" result ]

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

        use compiled =
            ControlHost.compile
                ControlHost.defaultProfile
                "qualification-controller"
                artifact.ArtifactBytes

        let acceptedOutputs = executeArtifact artifact compiled 4
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

        let reexecuted = executeArtifact artifact compiled 4

        { FullPackage = full
          PerspectivePackage = perspective
          Artifact = artifact
          ReexecutedOutputs = reexecuted }
