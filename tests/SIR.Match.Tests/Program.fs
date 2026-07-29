module SIR.Match.Tests

open SIR.Domain
open SIR.Match
open SIR.Simulation
open SIR.ControlAbi
open Wasmtime
open System
open System.Diagnostics

let private require condition message =
    if not condition then failwith message

let private containsSubsequence (needle: byte array) (haystack: byte array) =
    if needle.Length = 0 then
        true
    else
        [ 0 .. haystack.Length - needle.Length ]
        |> List.exists (fun offset ->
            haystack[offset .. offset + needle.Length - 1] = needle)

let private controlAbiOutput () =
    V1Codec.encodeOutput
        42
        7
        0u
        1000u
        [ { Kind = RequestKind.Sleep
            ModuleRequestId = 9u
            Payload = [| 100uy; 0uy; 0uy; 0uy |] }
          { Kind = RequestKind.SetAttention
            ModuleRequestId = 7u
            Payload = [| 2uy |] } ]
        []
    |> Result.defaultWith (fun error -> failwithf "%A" error)

let private executeReferenceControlModule expectedOutput =
    let data =
        expectedOutput
        |> Array.map (fun value -> sprintf "\\%02x" value)
        |> String.concat ""

    let wat =
        $"""(module
          (memory (export "memory") 2)
          (data (i32.const 65536) "{data}")
          (func (export "sir_abi_version") (result i32) i32.const 65536)
          (func (export "sir_input_ptr") (result i32) i32.const 0)
          (func (export "sir_input_capacity") (result i32) i32.const 65536)
          (func (export "sir_output_ptr") (result i32) i32.const 65536)
          (func (export "sir_output_capacity") (result i32) i32.const 16384)
          (func (export "sir_decide") (param i32) (result i32)
            i32.const {expectedOutput.Length}))"""

    use engine = new Engine()
    use compiled = Module.FromText(engine, "control-abi-v1-reference", wat)
    use linker = new Linker(engine)
    use store = new Store(engine)
    let instance = linker.Instantiate(store, compiled)
    let memory =
        match instance.GetMemory("memory") with
        | null -> failwith "Reference ABI module did not export memory."
        | value -> value

    let decide =
        match instance.GetFunction("sir_decide") with
        | null -> failwith "Reference ABI module did not export sir_decide."
        | value -> value

    let input =
        { Kind = MessageKind.Input
          MinorVersion = 0uy
          Tick = 42
          UnitId = 7
          Flags = 0u
          Budget = 1000u
          Sections =
            [ { Tag = V1Constants.OwnStateTag
                Required = true
                ElementCount = 1
                Payload = [| 1uy |] } ] }
        |> V1Codec.encode
        |> Result.defaultWith (fun error -> failwithf "%A" error)

    System.ReadOnlySpan<byte>(input).CopyTo(memory.GetSpan(0L, input.Length))

    let outputLength =
        match decide.Invoke(input.Length) with
        | :? int as value -> value
        | value -> failwithf "Unexpected reference ABI result: %A" value

    memory.GetSpan(65536L, outputLength).ToArray()

let private abiInput tick unitId =
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
    |> Result.defaultWith (fun error -> failwithf "Could not encode host input: %A" error)

let private watBytes (wat: string) = Module.ConvertText wat

let private watData (bytes: byte array) =
    bytes
    |> Array.map (fun value -> sprintf "\\%02x" value)
    |> String.concat ""

let private statefulControllerWat requests =
    let output =
        V1Codec.encodeOutput 0 0 0u 0u requests []
        |> Result.defaultWith (fun error -> failwithf "Could not encode controller output: %A" error)

    $"""(module
      (memory (export "memory") 2 2)
      (global $counter (export "counter") (mut i32) (i32.const 0))
      (data (i32.const 65536) "{watData output}")
      (func (export "sir_abi_version") (result i32) i32.const 65536)
      (func (export "sir_input_ptr") (result i32) i32.const 0)
      (func (export "sir_input_capacity") (result i32) i32.const 65536)
      (func (export "sir_output_ptr") (result i32) i32.const 65536)
      (func (export "sir_output_capacity") (result i32) i32.const 16384)
      (func (export "sir_decide") (param i32) (result i32)
        global.get $counter i32.const 1 i32.add global.set $counter
        i32.const 65548 i32.const 12 i32.load i32.store
        i32.const 65552 i32.const 16 i32.load i32.store
        i32.const 65556 global.get $counter i32.store
        i32.const {output.Length}))"""

let private runControlHostQualifications () =
    let artifactBytes = statefulControllerWat [] |> watBytes
    use artifact =
        ControlHost.compile ControlHost.defaultProfile "standard-reference" artifactBytes
    use first = ControlHost.instantiate artifact 7 [| 1uy; 2uy |]
    use second = ControlHost.instantiate artifact 8 [| 3uy |]

    let firstTick =
        match ControlHost.invoke 1 (abiInput 1 7) first with
        | Accepted(output, journal) ->
            require (output.Envelope.Flags = 1u) "First instance did not advance its private global."
            output, journal
        | result -> failwithf "Reference controller failed: %A" result

    match ControlHost.invoke 1 (abiInput 1 8) second with
    | Accepted(output, _) ->
        require (output.Envelope.Flags = 1u) "Instances shared mutable global state."
    | result -> failwithf "Second reference controller failed: %A" result

    let snapshot = ControlHost.snapshot first
    let checkpointed = ControlHost.checkpointJournal first (snd firstTick)
    require checkpointed.ModuleState.IsSome "Replay checkpoint omitted resumable module state."

    let mutable incompleteSnapshotRejected = false
    try
        use _incomplete =
            ControlHost.resume
                artifact
                7
                [| 1uy; 2uy |]
                { snapshot with MutableGlobals = [] }
        ()
    with :? ArgumentException ->
        incompleteSnapshotRejected <- true
    require
        incompleteSnapshotRejected
        "An incomplete mutable-global snapshot was accepted."

    let originalTickTwo =
        match ControlHost.invoke 2 (abiInput 2 7) first with
        | Accepted(output, journal) -> output, journal
        | result -> failwithf "Original controller continuation failed: %A" result

    use resumed = ControlHost.resume artifact 7 [| 1uy; 2uy |] snapshot

    let resumedTickTwo =
        match ControlHost.invoke 2 (abiInput 2 7) resumed with
        | Accepted(output, journal) -> output, journal
        | result -> failwithf "Resumed controller continuation failed: %A" result

    require
        ((fst originalTickTwo).Envelope.Flags = (fst resumedTickTwo).Envelope.Flags
         && (snd originalTickTwo).OutputHash = (snd resumedTickTwo).OutputHash
         && (snd originalTickTwo).ModuleStateHash = (snd resumedTickTwo).ModuleStateHash)
        "Snapshot/resume did not reproduce controller output and state hashes."

    let malformedWat =
        statefulControllerWat []
        |> fun wat -> wat.Replace(
            $"i32.const 65548 i32.const 12 i32.load i32.store",
            $"i32.const 65536 i32.const 0 i32.store"
        )

    use malformedArtifact =
        ControlHost.compile ControlHost.defaultProfile "malformed-reference" (watBytes malformedWat)
    use malformed = ControlHost.instantiate malformedArtifact 7 [||]

    match ControlHost.invoke 1 (abiInput 1 7) malformed with
    | Failed(ControlFailure.MalformedOutput, journal) ->
        require (List.isEmpty journal.Requests) "Malformed output retained partial requests."
    | result -> failwithf "Malformed output was not rejected atomically: %A" result

    let dynamicRangeWat =
        statefulControllerWat []
        |> fun wat ->
            wat.Replace(
                "(func (export \"sir_input_ptr\") (result i32) i32.const 0)",
                """(func (export "sir_input_ptr") (result i32)
                  (if (result i32)
                    (i32.gt_s (global.get $counter) (i32.const 0))
                    (then (i32.const 200000))
                    (else (i32.const 0))))"""
            )
    use dynamicRangeArtifact =
        ControlHost.compile
            ControlHost.defaultProfile
            "dynamic-range-reference"
            (watBytes dynamicRangeWat)
    use dynamicRange =
        ControlHost.instantiate dynamicRangeArtifact 7 [||]

    match ControlHost.invoke 1 (abiInput 1 7) dynamicRange with
    | Accepted _ -> ()
    | result -> failwithf "Initial dynamic-range invocation failed: %A" result

    match ControlHost.invoke 2 (abiInput 2 7) dynamicRange with
    | Failed(ControlFailure.MemoryLimit, journal) ->
        require
            (List.isEmpty journal.Requests)
            "A dynamic out-of-range buffer retained accepted requests."
    | result ->
        failwithf
            "A dynamic out-of-range buffer was not rejected atomically: %A"
            result

    let fuelWat =
        $"""(module
          (memory (export "memory") 2 2)
          (func (export "sir_abi_version") (result i32) i32.const 65536)
          (func (export "sir_input_ptr") (result i32) i32.const 0)
          (func (export "sir_input_capacity") (result i32) i32.const 65536)
          (func (export "sir_output_ptr") (result i32) i32.const 65536)
          (func (export "sir_output_capacity") (result i32) i32.const 16384)
          (func (export "sir_decide") (param i32) (result i32)
            (loop $forever br $forever) i32.const 0))"""

    use fuelArtifact =
        ControlHost.compile ControlHost.defaultProfile "fuel-reference" (watBytes fuelWat)
    use fuel = ControlHost.instantiate fuelArtifact 7 [||]

    match ControlHost.invoke 1 (abiInput 1 7) fuel with
    | Failed(ControlFailure.FuelExhaustion, journal) ->
        require
            (List.isEmpty journal.Requests
             && journal.Budget.FuelConsumed = journal.Budget.FuelAllowance)
            "Fuel exhaustion retained requests or an incomplete budget."
    | result -> failwithf "Fuel exhaustion was not isolated atomically: %A" result

    let sleepRequest =
        { Kind = RequestKind.Sleep
          ModuleRequestId = 1u
          Payload = BitConverter.GetBytes 3 }
    use sleepArtifact =
        ControlHost.compile
            ControlHost.defaultProfile
            "sleep-reference"
            (statefulControllerWat [ sleepRequest ] |> watBytes)
    use sleeping = ControlHost.instantiate sleepArtifact 7 [||]

    match ControlHost.invoke 1 (abiInput 1 7) sleeping with
    | Accepted _ -> ()
    | result -> failwithf "Sleep request failed: %A" result
    match ControlHost.invoke 2 (abiInput 2 7) sleeping with
    | SleepingUntil 3 -> ()
    | result -> failwithf "Sleep schedule was not enforced: %A" result

    let growthWat =
        statefulControllerWat []
        |> fun wat -> wat.Replace(
            "(memory (export \"memory\") 2 2)",
            "(memory (export \"memory\") 2)"
        )
        |> fun wat -> wat.Replace(
            "global.get $counter i32.const 1 i32.add global.set $counter",
            "i32.const 1 memory.grow drop global.get $counter i32.const 1 i32.add global.set $counter"
        )
    use growthArtifact =
        ControlHost.compile ControlHost.defaultProfile "growth-reference" (watBytes growthWat)
    use growth = ControlHost.instantiate growthArtifact 7 [||]
    match ControlHost.invoke 1 (abiInput 1 7) growth with
    | Accepted(_, journal) ->
        require
            (journal.Budget.MemoryBytes = ControlHost.defaultProfile.MaximumMemoryBytes)
            "Store memory limiter permitted growth beyond the profile."
    | result -> failwithf "Bounded memory-growth controller failed unexpectedly: %A" result

    let wasiWat =
        """(module
          (import "wasi_snapshot_preview1" "random_get"
            (func $random_get (param i32 i32) (result i32)))
          (memory (export "memory") 2))"""
    let mutable wasiRejected = false
    try
        use _artifact =
            ControlHost.compile ControlHost.defaultProfile "wasi-reference" (watBytes wasiWat)
        ()
    with :? ArgumentException ->
        wasiRejected <- true
    require wasiRejected "Ambient WASI import was accepted."

    let hiddenGlobalWat =
        statefulControllerWat []
        |> fun wat -> wat.Replace(
            "(global $counter (export \"counter\") (mut i32) (i32.const 0))",
            "(global $counter (mut i32) (i32.const 0))"
        )
    let mutable hiddenGlobalRejected = false
    try
        use _artifact =
            ControlHost.compile
                ControlHost.defaultProfile
                "hidden-global-reference"
                (watBytes hiddenGlobalWat)
        ()
    with :? ArgumentException ->
        hiddenGlobalRejected <- true
    require hiddenGlobalRejected "A hidden mutable global escaped snapshot qualification."

    let state =
        { Tick = 0
          Board =
            { Width = 3
              Height = 3
              Terrain = Map.empty
              Edges = Map.empty }
          Units =
            Map.ofList
                [ 7,
                  { Id = 7
                    Side = 1
                    ClassId = "rifleman"
                    Cell = MapScale.cell 1 1
                    Size = 1
                    Health = 10
                    Controller = ManualController
                    Script = []
                    ScriptIndex = 0
                    BodyFacing = North
                    AttentionDirection = North } ]
          MovementCreditsMillimeters = Map.empty
          MovementProgress = Map.empty
          MovementIntents = Map.empty
          PlannedRoutes = Map.empty
          Engagements = Map.empty }

    let kernelRequests =
        [ { Kind = RequestKind.SetMovementIntent
            ModuleRequestId = 1u
            Payload = [| Direction8.toCode East |] }
          { Kind = RequestKind.SetFacing
            ModuleRequestId = 2u
            Payload = [| Direction8.toCode South |] }
          { Kind = RequestKind.SetAttention
            ModuleRequestId = 3u
            Payload = [| Direction8.toCode West |] } ]

    let fedState =
        ControlHost.applyToMapScale state 7 kernelRequests
        |> Result.defaultWith failwith
    require
        (Map.find 7 fedState.MovementIntents = East
         && (Map.find 7 fedState.Units).BodyFacing = South
         && (Map.find 7 fedState.Units).AttentionDirection = West)
        "Accepted host requests were not fed into MapScale."

    let instances =
        [| for unitId in 1 .. 200 ->
               ControlHost.instantiate artifact unitId [||] |]
    try
        for unitId in 1 .. 200 do
            ControlHost.invoke 1 (abiInput 1 unitId) instances[unitId - 1] |> ignore

        let samples =
            [| for tick in 2 .. 6 do
                   let started = Stopwatch.GetTimestamp()
                   for unitId in 1 .. 200 do
                       match ControlHost.invoke tick (abiInput tick unitId) instances[unitId - 1] with
                       | Accepted _ -> ()
                       | result -> failwithf "200-instance qualification failed: %A" result
                   yield Stopwatch.GetElapsedTime(started).TotalMilliseconds |]
        let best = Array.min samples
        let qualificationBudgetMs = 50.0
        require
            (best < qualificationBudgetMs)
            $"200 controllers exceeded the configuration's {qualificationBudgetMs:F0} ms qualification budget (best {best:F3} ms)."
        best
    finally
        instances |> Array.iter (fun instance -> (instance :> IDisposable).Dispose())

[<EntryPoint>]
let main _ =
    let controllerTickMs = runControlHostQualifications ()
    let expectedControlOutput = controlAbiOutput ()
    let referenceControlOutput =
        executeReferenceControlModule expectedControlOutput

    require
        (referenceControlOutput = expectedControlOutput)
        "Reference WASM module and F# Control ABI v1 codec disagree."

    let qualification = MatchReplay.qualify ()
    let fullBytes = Replay.encode qualification.FullPackage
    let perspectiveBytes = Replay.encode qualification.PerspectivePackage
    let expectedEngine = qualification.FullPackage.EngineHash

    match
        Replay.runKernelReplay
            Replay.defaultLimits
            expectedEngine
            qualification.FullPackage
    with
    | Ok(BrowserKernelVerified browserResult) ->
        match
            Replay.verifyAuthoritative
                Replay.defaultLimits
                expectedEngine
                (Some qualification.ReexecutedOutputs)
                qualification.FullPackage
        with
        | Ok(AuthoritativeVerified authoritativeResult) ->
            require
                (browserResult.StateHash = authoritativeResult.StateHash
                 && browserResult.EventHash = authoritativeResult.EventHash)
                "Browser-kernel and authoritative hashes differ."
        | result ->
            failwithf "Exact-artifact authoritative verification failed: %A" result
    | result -> failwithf "Full browser-kernel replay failed: %A" result

    let changedOutputs =
        qualification.ReexecutedOutputs
        |> List.map (fun output ->
            if output.Tick = 2 then
                { output with
                    Input =
                        Move(
                            Simulation.unitId 10,
                            { Col = 0; Row = 1 }
                        ) }
            else
                output)

    match
        Replay.verifyAuthoritative
            Replay.defaultLimits
            expectedEngine
            (Some changedOutputs)
            qualification.FullPackage
    with
    | Error(WasmOutputDivergence(2, 2)) -> ()
    | result ->
        failwithf "Changed WASM output did not identify its first divergence: %A" result

    let corrupted =
        match qualification.FullPackage.Content with
        | PerspectivePlayback _ -> failwith "Expected an authorized full replay."
        | AuthorizedFullReplay full ->
            let checkpoints =
                full.Checkpoints
                |> List.map (fun checkpoint ->
                    if checkpoint.Tick = 2 then
                        let changed = Array.copy checkpoint.EventHash
                        changed[0] <- changed[0] ^^^ 1uy
                        { checkpoint with EventHash = changed }
                    else
                        checkpoint)

            { qualification.FullPackage with
                Content =
                    AuthorizedFullReplay
                        { full with Checkpoints = checkpoints } }

    match Replay.runKernelReplay Replay.defaultLimits expectedEngine corrupted with
    | Error(ReplayDivergence(2, "checkpoint event hash")) -> ()
    | result ->
        failwithf "Corrupt replay lost first-tick divergence diagnostics: %A" result

    match
        Replay.runKernelReplay
            Replay.defaultLimits
            expectedEngine
            qualification.PerspectivePackage
    with
    | Ok(PerspectiveReady frames) ->
        require (frames.Length = 5) "Perspective playback omitted a committed frame."
    | result -> failwithf "Perspective playback qualification failed: %A" result

    match Replay.requireKernel qualification.PerspectivePackage with
    | Error PerspectiveHasNoKernel -> ()
    | result -> failwithf "Perspective playback exposed kernel material: %A" result

    let hiddenFinalHash =
        match qualification.FullPackage.Content with
        | AuthorizedFullReplay full -> full.FinalResult.StateHash
        | PerspectivePlayback _ -> failwith "Expected an authorized full replay."

    require
        (not (containsSubsequence hiddenFinalHash perspectiveBytes))
        "Perspective bytes contain the hidden final-state hash."
    require
        (not (
            containsSubsequence
                qualification.Artifact.ArtifactBytes
                perspectiveBytes
        ))
        "Perspective bytes contain the opponent control artifact."
    require
        (perspectiveBytes.Length < fullBytes.Length)
        "Perspective package is not a reduced disclosure."

    printfn
        "Full match replay qualified: %d full bytes, %d perspective bytes, 4 exact WASM outputs; %d Control ABI v1 reference-module bytes agree; 200 isolated reusable-host instances in %.3f ms."
        fullBytes.Length
        perspectiveBytes.Length
        referenceControlOutput.Length
        controllerTickMs

    0
