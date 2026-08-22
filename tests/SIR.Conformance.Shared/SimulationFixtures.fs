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
    let private mapScaleUnit id column : MapScaleUnit =
        { Id = id
          Side = id
          ClassId = "rifleman"
          Cell = MapScale.cell column 0
          Size = 1
          Health = 100
          Controller = ManualController
          Script = []
          ScriptIndex = 0
          BodyFacing = North
          AttentionDirection = North }

    let private mapScaleState firstColumn secondColumn firstDirection secondDirection : MapScaleState =
        let first = mapScaleUnit 1 firstColumn
        let second = mapScaleUnit 2 secondColumn
        { Tick = 0
          Board =
            { Width = 3
              Height = 1
              Terrain = Map.empty
              Edges = Map.empty }
          Units = [ first.Id, first; second.Id, second ] |> Map.ofList
          MovementCreditsMillimeters = Map.ofList [ 1, 500; 2, 500 ]
          MovementProgress = Map.empty
          MovementIntents = Map.ofList [ 1, firstDirection; 2, secondDirection ]
          PlannedRoutes = Map.empty
          Engagements = Map.empty }

    /// Canonical simultaneous-contention fixtures and checkpoint diagnostics.
    let mapScaleEvidence () =
        let destination = mapScaleState 0 2 East West |> MapScale.tick
        let crossing = mapScaleState 0 1 East West |> MapScale.tick
        let moving = mapScaleUnit 1 0
        let successful =
            { Tick = 0
              Board = { Width = 2; Height = 1; Terrain = Map.empty; Edges = Map.empty }
              Units = Map.ofList [ moving.Id, moving ]
              MovementCreditsMillimeters = Map.ofList [ moving.Id, 500 ]
              MovementProgress = Map.empty
              MovementIntents = Map.ofList [ moving.Id, East ]
              PlannedRoutes = Map.empty
              Engagements = Map.empty }
            |> MapScale.tick
        let resolveEvents (result: MapScaleTickResult) =
            result.Checkpoints
            |> List.find (fun checkpoint -> checkpoint.Phase = MapScalePhase.ResolvePhase)
            |> _.Events
        let destinationCanonical =
            resolveEvents destination
            |> List.map (function
                | MapScaleEvent.MovementRejected(id, _, DestinationConflict address) ->
                    id, address.Col, address.Row
                | event -> failwithf "Expected destination conflict, got %A." event)
        let crossingCanonical =
            resolveEvents crossing
            |> List.map (function
                | MapScaleEvent.MovementRejected(id, _, CrossingConflict otherId) -> id, otherId
                | event -> failwithf "Expected crossing conflict, got %A." event)
        if destinationCanonical <> [ 1, 1, 0; 2, 1, 0 ] then
            failwithf "Destination conflict fixture changed: %A." destinationCanonical
        if crossingCanonical <> [ 1, 2; 2, 1 ] then
            failwithf "Crossing conflict fixture changed: %A." crossingCanonical
        if destination.State.Units |> Map.exists (fun id unit ->
            unit.Cell <> (if id = 1 then MapScale.cell 0 0 else MapScale.cell 2 0)) then
            failwith "Destination contention committed a movement."
        if crossing.State.Units |> Map.exists (fun id unit ->
            unit.Cell <> (if id = 1 then MapScale.cell 0 0 else MapScale.cell 1 0)) then
            failwith "Crossing contention committed a movement."
        let moved = Map.find moving.Id successful.State.Units
        if moved.Cell <> MapScale.cell 1 0 || moved.BodyFacing <> East then
            failwithf "Committed movement did not turn body facing along the resolved step: %A at %A." moved.BodyFacing moved.Cell
        if destination.State.Units |> Map.exists (fun _ unit -> unit.BodyFacing <> North) then
            failwith "Rejected movement changed body facing."
        let changed =
            destination.Checkpoints
            |> List.map (fun checkpoint ->
                if checkpoint.Phase = MapScalePhase.ResolvePhase then
                    { checkpoint with Events = checkpoint.Events @ [ MapScaleEvent.UnitHeld(99, "diagnostic") ] }
                else checkpoint)
        match MapScale.firstCheckpointDivergence destination.Checkpoints changed with
        | Some divergence when divergence.Tick = 1 && divergence.Phase = MapScalePhase.ResolvePhase -> ()
        | divergence -> failwithf "Map-scale phase divergence diagnostic changed: %A." divergence
        [ destination.Checkpoints; crossing.Checkpoints; successful.Checkpoints ]
        |> List.collect id
        |> List.map MapScale.checkpointBytes
        |> CanonicalEncoding.concatenate

    let private expectedOracles =
        [ SimulationPhase.MovementPhase,
          "01000000000300000000020000000a00000000010000000100000064000000320000001400000064000000000000000000000000000000140000000102000000000000006400000032000000140000006400000000000000000000000000000000000000000000000000000000000000000000000102000000000a0000000000000000000000010000000100000001140000000200000000000000010000000000000001000000000000000200000000000000"
          SimulationPhase.AwarenessReactionPhase,
          "01000000040300000000020000000a0000000001000000010000006400000032000000140000006400000000000000000000000000000014000000010200000000000000640000003200000014000000640000000000000000000000000000000000000000000000020000000a00000014000000011400000001040000000100000000010000000001240000005349522e53696d756c6174696f6e2e5370617469616c51756572792e6576616c75617465010000000100000002000000000000000000000000000000001400000073696d756c6174696f6e2d617574686f7269747900000000000000000102000000000000000003140000000a000000010a00000001010000000100000000010000000001240000005349522e53696d756c6174696f6e2e5370617469616c51756572792e6576616c75617465020000000000000001000000010000000200000000000000001400000073696d756c6174696f6e2d617574686f726974790000000000000000010100000001000000000300000000000000000104000000000a0000000000000000000000010000000100000001140000000200000000000000010000000000000001000000000000000200000000000000070a0000001400000001140000000100000000000000000307140000000a000000010a00000001000000000000000003"
          SimulationPhase.ObservationPhase,
          "01000000010300000000020000000a00000000010000000100000064000000320000001400000064000000000000000000000000000000140000000102000000000000006400000032000000140000006400000000000000000000000000000000000000010000000a00000014000000020000000a00000014000000011400000001040000000100000000010000000001240000005349522e53696d756c6174696f6e2e5370617469616c51756572792e6576616c75617465010000000100000002000000000000000000000000000000001400000073696d756c6174696f6e2d617574686f7269747900000000000000000102000000000000000003140000000a000000010a00000001010000000100000000010000000001240000005349522e53696d756c6174696f6e2e5370617469616c51756572792e6576616c75617465020000000000000001000000010000000200000000000000001400000073696d756c6174696f6e2d617574686f726974790000000000000000010100000001000000000300000000000000000105000000000a0000000000000000000000010000000100000001140000000200000000000000010000000000000001000000000000000200000000000000070a0000001400000001140000000100000000000000000307140000000a000000010a00000001000000000000000003020a0000001400000001000000"
          SimulationPhase.AttackPhase,
          "01000000020300000000020000000a00000000010000000100000064000000320000001400000064000000000000000000000000000000140000000102000000000000004b00000032000000140000006400000000000000000000000000000000000000010000000a00000014000000020000000a00000014000000011400000001040000000100000000010000000001240000005349522e53696d756c6174696f6e2e5370617469616c51756572792e6576616c75617465010000000100000002000000000000000000000000000000001400000073696d756c6174696f6e2d617574686f7269747900000000000000000102000000000000000003140000000a000000010a00000001010000000100000000010000000001240000005349522e53696d756c6174696f6e2e5370617469616c51756572792e6576616c75617465020000000000000001000000010000000200000000000000001400000073696d756c6174696f6e2d617574686f726974790000000000000000010100000001000000000300000000000000000106000000000a0000000000000000000000010000000100000001140000000200000000000000010000000000000001000000000000000200000000000000070a0000001400000001140000000100000000000000000307140000000a000000010a00000001000000000000000003020a0000001400000001000000030a00000014000000190000004b000000"
          SimulationPhase.CommitPhase,
          "01000000030301000000020000000a00000000010000000100000064000000320000001400000064000000000000000000000000000000140000000102000000000000004b00000032000000140000006400000000000000000000000000000000000000010000000a00000014000000020000000a00000014000000011400000001040000000100000000010000000001240000005349522e53696d756c6174696f6e2e5370617469616c51756572792e6576616c75617465010000000100000002000000000000000000000000000000001400000073696d756c6174696f6e2d617574686f7269747900000000000000000102000000000000000003140000000a000000010a00000001010000000100000000010000000001240000005349522e53696d756c6174696f6e2e5370617469616c51756572792e6576616c75617465020000000000000001000000010000000200000000000000001400000073696d756c6174696f6e2d617574686f726974790000000000000000010100000001000000000300000000000000000106000000000a0000000000000000000000010000000100000001140000000200000000000000010000000000000001000000000000000200000000000000070a0000001400000001140000000100000000000000000307140000000a000000010a00000001000000000000000003020a0000001400000001000000030a00000014000000190000004b0000004c97bbc4" ]

    let private fromHex (hex: string) =
        [| for index in 0 .. 2 .. hex.Length - 2 do
               yield System.Convert.ToByte(hex.Substring(index, 2), 16) |]

    let all =
        expectedOracles
        |> List.map (fun (phase, expected) ->
            { Tick = 1
              Phase = phase
              Expected = fromHex expected })

    let private resultBytes (result: TickResult) (checkpoint: PhaseCheckpoint) =
        let encodedCheckpoint = Simulation.checkpointBytes checkpoint

        if checkpoint.Phase = SimulationPhase.CommitPhase then
            CanonicalEncoding.concatenate [ encodedCheckpoint; result.StateDigest ]
        else
            encodedCheckpoint

    /// AC 1 / F1: the projected spatial world is constructed AT MOST ONCE per observation phase,
    /// however many observation pairs the tick carries - and not at all when it carries none.
    ///
    /// The counter lives HERE, in the caller, rather than inside `Simulation`. A module-level
    /// mutable in the simulation would be both a determinism hazard and a Fable hazard, so the
    /// world is a caller-supplied factory and the only mutable state sits in this fixture.
    let private observationWorldConstruction () =
        let state = Simulation.initialState
        let projectedWorld () =
            { Identity =
                SpatialAuthorityIdentity.create "fixture-board" "sir-spatial-v1" 0L "fixture-authority" 0L
                |> Result.defaultWith failwith
              Minimum = state.Board.Minimum
              Maximum = state.Board.Maximum
              Terrain = Map.empty
              Boundaries = []
              Occupancy = Map.empty
              DisclosedRevisionTokens = Set.empty }
        let countingRun journal =
            let constructions = ref 0
            let worldFor () =
                constructions.Value <- constructions.Value + 1
                projectedWorld ()
            let next, events = Simulation.observationPhaseWith worldFor state journal
            constructions.Value, next, events

        let observedCount, observedState, observedEvents =
            countingRun
                [ Observe(Simulation.unitId 10, Simulation.unitId 20)
                  Observe(Simulation.unitId 20, Simulation.unitId 10)
                  Observe(Simulation.unitId 10, Simulation.unitId 10) ]
        if observedCount <> 1 then
            failwithf "The observation phase constructed the spatial world %d times for a three-observation tick." observedCount
        // Without this the count above would pass vacuously on a phase that observed nothing.
        if List.isEmpty observedEvents || observedState.Observations = state.Observations then
            failwith "The counted observation phase produced no observation, so its construction count proves nothing."

        let unobservedCount, _, _ = countingRun [ Attack(Simulation.unitId 10, Simulation.unitId 20) ]
        if unobservedCount <> 0 then
            failwithf "The observation phase constructed %d spatial worlds for a tick carrying no observation." unobservedCount

    let private execute journal =
        observationWorldConstruction ()
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
        | SimulationPhase.MovementPhase -> "movement"
        | SimulationPhase.ObservationPhase -> "observation"
        | SimulationPhase.AttackPhase -> "attack"
        | SimulationPhase.CommitPhase -> "commit"
        | SimulationPhase.AwarenessReactionPhase -> "awareness-reaction"

    let tryParsePhase value =
        match value with
        | "movement" -> Some SimulationPhase.MovementPhase
        | "observation" -> Some SimulationPhase.ObservationPhase
        | "attack" -> Some SimulationPhase.AttackPhase
        | "commit" -> Some SimulationPhase.CommitPhase
        | "awareness-reaction" -> Some SimulationPhase.AwarenessReactionPhase
        | _ -> None
