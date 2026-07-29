namespace SIR.Conformance

open SIR.Domain
open SIR.Simulation

type SimulationFixture =
    { Tick: int32
      Phase: SimulationPhase
      Expected: byte array }

type SimulationDivergence =
    { Tick: int32
      Phase: SimulationPhase
      ByteOffset: int
      Expected: byte
      Actual: byte }

[<RequireQualifiedAccess>]
module SimulationFixtures =
    let private expectedOracles =
        [ MovementPhase,
          "01000000000200000000020000000a00000000010000000100000064000000000014000000010200000000000000640000000000000000000102000000000a0000000000000000000000010000000100000001140000000200000000000000010000000000000001000000000000000200000000000000"
          ObservationPhase,
          "01000000010200000000020000000a00000000010000000100000064000000000014000000010200000000000000640000000000010000000a000000140000000103000000000a0000000000000000000000010000000100000001140000000200000000000000010000000000000001000000000000000200000000000000020a0000001400000001000000"
          AttackPhase,
          "01000000020200000000020000000a000000000100000001000000640000000000140000000102000000000000004b0000000000010000000a000000140000000104000000000a0000000000000000000000010000000100000001140000000200000000000000010000000000000001000000000000000200000000000000020a0000001400000001000000030a00000014000000190000004b000000"
          CommitPhase,
          "01000000030201000000020000000a000000000100000001000000640000000000140000000102000000000000004b0000000000010000000a000000140000000104000000000a0000000000000000000000010000000100000001140000000200000000000000010000000000000001000000000000000200000000000000020a0000001400000001000000030a00000014000000190000004b00000093d95660" ]

    let private fromHex (hex: string) =
        [| for index in 0 .. 2 .. hex.Length - 2 do
               yield System.Convert.ToByte(hex.Substring(index, 2), 16) |]

    let all =
        expectedOracles
        |> List.map (fun (phase, expected) ->
            { Tick = 1
              Phase = phase
              Expected = fromHex expected })

    let private resultBytes result checkpoint =
        let encodedCheckpoint = Simulation.checkpointBytes checkpoint

        if checkpoint.Phase = CommitPhase then
            CanonicalEncoding.concatenate [ encodedCheckpoint; result.StateDigest ]
        else
            encodedCheckpoint

    let private execute journal =
        let result = Simulation.runTick Simulation.initialState journal

        result,
        result.Checkpoints
        |> List.map (fun checkpoint -> checkpoint.Phase, resultBytes result checkpoint)

    let evaluate (injectAt: SimulationPhase option) =
        let result, evaluated = execute Simulation.inputs
        let reversedResult, reversed = execute (List.rev Simulation.inputs)

        if evaluated <> reversed
           || result.StateBytes <> reversedResult.StateBytes
           || result.EventBytes <> reversedResult.EventBytes
           || result.StateDigest <> reversedResult.StateDigest then
            failwith "Reordering the canonical M6 input journal changed the simulation result."

        all
        |> List.map (fun fixture ->
            let actual =
                evaluated
                |> List.find (fun (phase, _) -> phase = fixture.Phase)
                |> snd

            match injectAt with
            | Some phase when phase = fixture.Phase ->
                let divergent = Array.copy actual
                divergent[0] <- divergent[0] ^^^ 1uy
                fixture, divergent
            | _ -> fixture, actual)

    let firstDivergence (evaluated: (SimulationFixture * byte array) list) =
        evaluated
        |> List.tryPick (fun (fixture, actual) ->
            if fixture.Expected.Length <> actual.Length then
                Some
                    { Tick = fixture.Tick
                      Phase = fixture.Phase
                      ByteOffset = min fixture.Expected.Length actual.Length
                      Expected = 0uy
                      Actual = 0uy }
            else
                fixture.Expected
                |> Array.mapi (fun index expected -> index, expected)
                |> Array.tryPick (fun (index, expected) ->
                    let actualByte = actual[index]

                    if expected = actualByte then
                        None
                    else
                        Some
                            { Tick = fixture.Tick
                              Phase = fixture.Phase
                              ByteOffset = index
                              Expected = expected
                              Actual = actualByte }))

    let canonicalBytes evaluated =
        evaluated
        |> Seq.map (fun (_, actual) -> actual)
        |> CanonicalEncoding.concatenate

    let phaseName phase =
        match phase with
        | MovementPhase -> "movement"
        | ObservationPhase -> "observation"
        | AttackPhase -> "attack"
        | CommitPhase -> "commit"

    let tryParsePhase value =
        match value with
        | "movement" -> Some MovementPhase
        | "observation" -> Some ObservationPhase
        | "attack" -> Some AttackPhase
        | "commit" -> Some CommitPhase
        | _ -> None
