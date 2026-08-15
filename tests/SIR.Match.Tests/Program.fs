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

let private tacticalEnvironmentEvidence () =
    let plot, variants = SIR.Simulation.TacticalEnvironment.exteriorParcelSet
    let assemble seed =
        SIR.Simulation.TacticalEnvironment.assemble seed plot variants
        |> Result.defaultWith (fun findings -> failwithf "Tactical environment did not assemble: %A" findings)

    let first = assemble 0x186UL
    let replay = assemble 0x186UL
    let firstBytes = SIR.Domain.TacticalEnvironment.canonicalBytes first
    let replayBytes = SIR.Domain.TacticalEnvironment.canonicalBytes replay
    require
        (firstBytes = replayBytes)
        "Equal tactical-environment seeds did not produce byte-identical canonical environments."
    require
        (first.EnvironmentContentIdentity = replay.EnvironmentContentIdentity
         && SIR.Domain.TacticalEnvironment.identityMatches first)
        "Deterministic tactical-environment content identity was not self-consistent."

    let invalidPlot = { plot with PlotSchemaVersion = 999 }
    let invalidVariant =
        { variants.Head with
            ParcelObjectiveCells = [ { EnvironmentColumn = 7; EnvironmentRow = 7 } ]
            ParcelWalkableCells = [ { EnvironmentColumn = 0; EnvironmentRow = 0 } ]
            ParcelFeatures =
                variants.Head.ParcelFeatures
                |> List.map (fun feature ->
                    { feature with
                        ModalityPermeability =
                            { feature.ModalityPermeability with AllowsMovement = true } }) }
    let validationCodes =
        SIR.Simulation.TacticalEnvironment.validate invalidPlot [ invalidVariant ]
        |> List.map _.ValidationCode
        |> Set.ofList
    require
        (Set.contains EnvironmentValidationCode.InvalidSchema validationCodes
         && Set.contains EnvironmentValidationCode.BlockedObjective validationCodes
         && Set.contains EnvironmentValidationCode.InvalidPermeability validationCodes)
        "Tactical-environment validation did not report independent schema, objective, and permeability categories."

    let knowledge =
        { EnvironmentKnowledgeIdentity = "test-observer"
          EnvironmentKnowledgeRevision = 3L
          KnownEnvironmentFeatureIds = Set [ "slot-1:yard-door" ]
          KnownEnvironmentFacts = Set.empty }
    require
        ((SIR.Simulation.TacticalEnvironment.observe knowledge first "slot-1:yard-cover").IsNone)
        "Environment observation disclosed a feature outside requester knowledge."
    let observed =
        SIR.Simulation.TacticalEnvironment.observe knowledge first "slot-1:yard-door"
        |> Option.defaultWith (fun () ->
            failwithf
                "Known tactical feature was not observable; assembled ids were %A."
                (first.EnvironmentFeatures |> List.map _.EnvironmentFeatureId))
    require
        (observed.ObservedState = Some EnvironmentFeatureState.Closed
         && observed.ObservationKnowledgeIdentity = knowledge.EnvironmentKnowledgeIdentity)
        "Environment observation did not retain disclosed state and knowledge provenance."

    let world = SIR.Simulation.TacticalEnvironment.toSpatialWorld "test-rules@1" knowledge first
    let door = first.EnvironmentFeatures |> List.find (fun feature -> feature.EnvironmentFeatureId = "slot-1:yard-door")
    let doorOrigin : FS.GG.Game.Core.Cell = { Col = door.EnvironmentEdge.EdgeCell.EnvironmentColumn; Row = door.EnvironmentEdge.EdgeCell.EnvironmentRow }
    let doorTarget =
        match door.EnvironmentEdge.EdgeDirection with
        | EnvironmentEdgeDirection.East -> { doorOrigin with Col = doorOrigin.Col + 1 }
        | EnvironmentEdgeDirection.South -> { doorOrigin with Row = doorOrigin.Row + 1 }
    let request =
        { QueryId = "door-crossing"
          QueryKind = SpatialQueryKind.ExactLineOfSight
          Origin = doorOrigin
          Target = doorTarget
          Footprint = [ { Col = 0; Row = 0 } ]
          Profile =
            { ProfileId = "ground"
              Modality = SpatialModality.GroundMovement
              Stance = "standing"
              HeightBand = 1
              Facing = Direction8.East }
          Bounds = SpatialQuery.defaultBounds }
    let closedResult, populatedCache, source = SpatialQuery.evaluateCached SpatialQuery.emptyCache world request
    require
        (source = SpatialEvaluationSource.Uncached
         && not closedResult.Visible
         && populatedCache.DynamicEntries.Length = 1)
        "Closed door did not produce an uncached, dependency-tracked blocked sight line."

    let coverFeature = first.EnvironmentFeatures |> List.find (fun feature -> feature.EnvironmentFeatureId = "slot-1:yard-cover")
    let coverOrigin : FS.GG.Game.Core.Cell = { Col = coverFeature.EnvironmentEdge.EdgeCell.EnvironmentColumn; Row = coverFeature.EnvironmentEdge.EdgeCell.EnvironmentRow }
    let coverTarget =
        match coverFeature.EnvironmentEdge.EdgeDirection with
        | EnvironmentEdgeDirection.East -> { coverOrigin with Col = coverOrigin.Col + 1 }
        | EnvironmentEdgeDirection.South -> { coverOrigin with Row = coverOrigin.Row + 1 }
    let coverRequest = { request with QueryId = "cover-crossing"; Origin = coverOrigin; Target = coverTarget }
    let _, localityCache, _ = SpatialQuery.evaluateCached populatedCache world coverRequest
    require (localityCache.DynamicEntries.Length = 2) "Local invalidation fixture did not retain two independent dependency receipts."

    let opened =
        SIR.Simulation.TacticalEnvironment.applyAction
            knowledge
            first.EnvironmentContentIdentity
            "slot-1:yard-door"
            EnvironmentAction.Open
            first
        |> Result.defaultWith (fun failure -> failwithf "Known door could not be opened: %A" failure)
    require
        (opened.UpdatedEnvironment.EnvironmentSpatialRevision = first.EnvironmentSpatialRevision + 1L
         && opened.ActionCostCounters.FeaturesInspected = 1
         && opened.ActionCostCounters.FeaturesChanged = 1
         && opened.ActionCostCounters.PropagatedChanges = 0
         && opened.ChangedQueryDependencies = Set [ "feature:yard-door:slot-1:yard-door" ])
        "Door transition did not emit bounded work and exact spatial dependencies."
    match
        SIR.Simulation.TacticalEnvironment.applyAction
            knowledge
            first.EnvironmentContentIdentity
            "slot-1:yard-door"
            EnvironmentAction.Close
            opened.UpdatedEnvironment
    with
    | Error EnvironmentActionFailure.StaleContentIdentity -> ()
    | result -> failwithf "Stale tactical content identity was not rejected: %A" result

    let invalidated, inspected, removed =
        SIR.Simulation.TacticalEnvironment.invalidateCache opened localityCache
    require
        (inspected = 2 && removed = 1 && invalidated.DynamicEntries.Length = 1)
        "Door transition did not selectively invalidate its dependent spatial query."
    let openedWorld =
        SIR.Simulation.TacticalEnvironment.toSpatialWorld "test-rules@1" knowledge opened.UpdatedEnvironment
    let openResult, _, _ = SpatialQuery.evaluateCached invalidated openedWorld request
    require
        (openResult.Outcome = SpatialOutcome.Found && openResult.Visible)
        "Opening the door did not change the authoritative sight-line result."

    let clock = Stopwatch.StartNew()
    let counters =
        SIR.Simulation.TacticalEnvironment.workload 0x186UL plot variants populatedCache
        |> Result.defaultWith (fun findings -> failwithf "Tactical workload failed: %A" findings)
    clock.Stop()
    let elapsed = clock.Elapsed.TotalMilliseconds
    require
        (counters.WorkloadSlots <= SIR.Simulation.TacticalEnvironment.maximumSlots
         && counters.WorkloadVariantsInspected <= 4 * SIR.Simulation.TacticalEnvironment.maximumVariantsPerRole
         && counters.WorkloadFindings <= SIR.Simulation.TacticalEnvironment.maximumFindings
         && counters.DependencyEntriesInspected <= populatedCache.DynamicEntries.Length
         && elapsed < 1_000.0)
        "Tactical environment workload exceeded a declared work or smoke-time bound."

    let maximumPlot =
        { plot with
            AuthoredPlotId = "maximum-authored-environment"
            PlotWidth = 512
            PlotSlots =
                [ for index in 0 .. 63 ->
                    { PlotSlotId = sprintf "slot-%02d" index
                      PlotSlotRole = "maximum"
                      PlotSlotOrigin = { EnvironmentColumn = index * 8; EnvironmentRow = 0 }
                      PlotSlotWidth = 8
                      PlotSlotHeight = 8
                      ConnectedPlotSlotIds =
                        [ if index > 0 then sprintf "slot-%02d" (index - 1)
                          if index < 63 then sprintf "slot-%02d" (index + 1) ]
                      PlotSlotRequiresRoute = true } ] }
    let maximumVariants =
        [ for index in 0 .. 31 ->
            { variants.Head with
                ParcelVariantId = sprintf "maximum-%02d" index
                ParcelRole = "maximum"
                ParcelConnections =
                    [ { ConnectionId = "route"
                        ConnectionCell = { EnvironmentColumn = 0; EnvironmentRow = 0 }
                        ConnectionDirection = EnvironmentEdgeDirection.East
                        ConnectionRole = "maximum" } ] } ]
    SIR.Simulation.TacticalEnvironment.assemble 0x185UL maximumPlot maximumVariants
    |> Result.defaultWith (fun findings -> failwithf "Maximum tactical warmup failed: %A" findings)
    |> ignore
    let maximumClock = Stopwatch.StartNew()
    let maximumEnvironment =
        SIR.Simulation.TacticalEnvironment.assemble 0x186UL maximumPlot maximumVariants
        |> Result.defaultWith (fun findings -> failwithf "Maximum tactical assembly failed: %A" findings)
    maximumClock.Stop()
    require
        (maximumEnvironment.AssemblyCostCounters.SlotsVisited = 64
         && maximumEnvironment.AssemblyCostCounters.VariantsInspected = 2_048
         && maximumEnvironment.AssemblyCostCounters.Selections = 64
         && maximumClock.Elapsed.TotalMilliseconds < 25.0)
        "Maximum 64-slot/32-variant assembly exceeded its structural or 25 ms timing budget."

    printfn
        "Tactical environment evidence: identity %s; closed/open path %A/%A; %d cache entry invalidated; representative %.3f ms; maximum assembly %.3f ms."
        first.EnvironmentContentIdentity
        closedResult.Outcome
        openResult.Outcome
        removed
        elapsed
        maximumClock.Elapsed.TotalMilliseconds

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

let private runPlanQualifications () =
    let artifactBytes = StandardController.artifactBytes ()
    require
        (not (StandardController.source.Contains("(import", StringComparison.Ordinal)))
        "The standard controller source introduced a non-public host import."
    let firstMove = Guid.ParseExact("10000000000000000000000000000001", "N")
    let firstFace = Guid.ParseExact("10000000000000000000000000000002", "N")
    let firstAttend = Guid.ParseExact("10000000000000000000000000000003", "N")
    let firstSync = Guid.ParseExact("10000000000000000000000000000004", "N")
    let firstEngage = Guid.ParseExact("10000000000000000000000000000005", "N")
    let firstHold = Guid.ParseExact("10000000000000000000000000000006", "N")

    let command commandId predecessors kind annotation =
        { CommandId = commandId
          EarliestStartTick = 1
          Predecessors = predecessors
          InterruptionPolicy = ApplyFallback
          Fallback = HoldPosition
          Kind = kind
          Annotation = annotation }

    let planUnit unitId origin destination target (prefix: byte) =
        let id (source: Guid) =
            let bytes = source.ToByteArray()
            bytes[15] <- bytes[15] + prefix
            Guid bytes

        let move = id firstMove
        let face = id firstFace
        let attend = id firstAttend
        let sync = id firstSync
        let engage = id firstEngage
        let hold = id firstHold

        { UnitId = unitId
          ControllerArtifact = artifactBytes
          Commands =
            [| command move [||] (MovePath([| origin; destination |], Balanced)) "route annotation"
               command face [| move |] (SetFacingIntent(FaceFixed East)) "face annotation"
               command attend [| face |] (SetAttentionIntent(AttendRelativeToBody North)) ""
               command
                   sync
                   [| attend |]
                   (Synchronize
                       { MarkerId = "line-ready"
                         Mode = PreloadedClock 30
                         DeadlineTick = 35
                         Timeout = Continue })
                   "clock synchronization"
               command engage [| sync |] (EngageUnit(target, "rifle")) "point engagement"
               command hold [| engage |] Hold "" |]
          Fallback = AbortUnitPlan }

    let state =
        { Tick = 0
          Board =
            { Width = 6
              Height = 3
              Terrain = Map.empty
              Edges = Map.empty }
          Units =
            Map.ofList
                [ 1,
                  { Id = 1
                    Side = 1
                    ClassId = "rifleman"
                    Cell = MapScale.cell 0 1
                    Size = 1
                    Health = 10
                    Controller = GeneralController
                    Script = []
                    ScriptIndex = 0
                    BodyFacing = North
                    AttentionDirection = North }
                  2,
                  { Id = 2
                    Side = 1
                    ClassId = "rifleman"
                    Cell = MapScale.cell 5 1
                    Size = 1
                    Health = 10
                    Controller = GeneralController
                    Script = []
                    ScriptIndex = 0
                    BodyFacing = North
                    AttentionDirection = North } ]
          MovementCreditsMillimeters = Map.empty
          MovementProgress = Map.empty
          MovementIntents = Map.empty
          PlannedRoutes = Map.empty
          Engagements = Map.empty }

    let mapDigest = Array.init 32 byte
    let document =
        { FormatVersion = SirPlan.FormatVersion
          PlanId = Guid.ParseExact("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "N")
          Revision = 4L
          ParentDigest = Some(Array.create 32 0x11uy)
          MapRevisionDigest = mapDigest
          RulesetIdentity = "prototype-rules-1"
          StartTick = 1
          HorizonTicks = 100
          UnitPlans =
            [| planUnit 1 (MapScale.cell 0 1) (MapScale.cell 1 1) 2 0uy
               planUnit 2 (MapScale.cell 5 1) (MapScale.cell 4 1) 1 16uy |] }

    let context =
        { Map = state
          RulesetIdentity = document.RulesetIdentity
          MapRevisionDigest = mapDigest
          MaximumConfigurationBytes = ControlHost.defaultProfile.MaximumConfigurationBytes }

    let encoded =
        SirPlan.encode document
        |> Result.defaultWith failwith
    let decoded =
        SirPlan.decode encoded
        |> Result.defaultWith failwith
    require
        (SirPlan.encode decoded = Ok encoded)
        "SIR-PLAN 1 canonical source did not round-trip exactly."

    let annotationEdit =
        { document with
            UnitPlans =
                document.UnitPlans
                |> Array.mapi (fun index unit ->
                    if index <> 0 then unit
                    else
                        { unit with
                            Commands =
                                unit.Commands
                                |> Array.mapi (fun commandIndex item ->
                                    if commandIndex = 0 then
                                        { item with Annotation = "edited only" }
                                    else item) }) }
    require
        (SirPlan.semanticDigest annotationEdit = SirPlan.semanticDigest document
         && SirPlan.sourceDigest annotationEdit <> SirPlan.sourceDigest document)
        "Annotations changed execution identity or failed to change source identity."

    let compiled =
        SirPlan.compile context document
        |> Result.defaultWith (fun issues -> failwithf "Coordinated plan failed validation: %A" issues)
    require
        (compiled.Units.Length = 2
         && compiled.Units |> Array.forall (fun unit -> unit.Configuration.Length <= 4096))
        "The coordinated plan did not compile to two bounded configurations."

    let cyclic =
        let unit = document.UnitPlans[0]
        let first = unit.Commands[0]
        let second = unit.Commands[1]
        { document with
            UnitPlans =
                [| { unit with
                       Commands =
                           [| { first with
                                  Predecessors = [| second.CommandId |]
                                  Kind =
                                    MovePath(
                                        [| MapScale.cell 0 1
                                           MapScale.cell 4 1 |],
                                        Balanced) }
                              { second with Predecessors = [| first.CommandId |] } |] } |] }

    let fallbackCyclic =
        let unit = document.UnitPlans[0]
        let first = unit.Commands[0]
        let second = unit.Commands[1]
        { document with
            UnitPlans =
                [| { unit with
                       Commands =
                           [| { first with
                                  Predecessors = [||]
                                  Fallback = JumpTo second.CommandId }
                              { second with
                                  Predecessors = [||]
                                  Fallback = JumpTo first.CommandId } |] } |] }

    let diagnosticSignature candidate =
        SirPlan.validate context candidate
        |> Array.map (fun diagnostic ->
            diagnostic.Code, diagnostic.UnitId, diagnostic.CommandId, diagnostic.Fields)

    let firstDiagnostics = diagnosticSignature cyclic
    let secondDiagnostics = diagnosticSignature cyclic
    require
        (firstDiagnostics = secondDiagnostics
         && firstDiagnostics
            |> Array.exists (fun (code, _, commandId, _) ->
                code = "SIR.PLAN.SCHEDULE.CYCLE" && commandId.IsSome)
         && firstDiagnostics
            |> Array.exists (fun (code, _, commandId, _) ->
                code = "SIR.PLAN.MAP.NON_ADJACENT_PATH" && commandId.IsSome))
        "Invalid/cyclic plans did not produce stable command-scoped diagnostics."
    require
        (diagnosticSignature fallbackCyclic
         |> Array.exists (fun (code, _, commandId, _) ->
             code = "SIR.PLAN.SCHEDULE.CYCLE" && commandId.IsSome))
        "Fallback JumpTo cycles escaped dependency validation."

    use artifact =
        ControlHost.compile
            ControlHost.defaultProfile
            "sir-standard-controller"
            artifactBytes

    let run () =
        let instances =
            compiled.Units
            |> Array.map (fun unit ->
                unit.UnitId,
                ControlHost.instantiate artifact unit.UnitId unit.Configuration)
        try
            [| for tick in document.StartTick .. document.StartTick + document.HorizonTicks - 1 do
                   for unitId, instance in instances do
                       match ControlHost.invoke tick (abiInput tick unitId) instance with
                       | Accepted(output, _) when not output.Requests.IsEmpty ->
                           yield
                               tick,
                               unitId,
                               output.Requests
                               |> List.map (fun request ->
                                   request.Kind,
                                   request.ModuleRequestId,
                                   Convert.ToHexString request.Payload)
                       | Accepted _ -> ()
                       | SleepingUntil _ -> ()
                       | result ->
                           failwithf
                               "Standard controller failed at tick %d for unit %d: %A"
                               tick
                               unitId
                               result |]
        finally
            instances
            |> Array.iter (fun (_, instance) ->
                (instance :> IDisposable).Dispose())

    let firstRun = run ()
    let secondRun = run ()
    require
        (firstRun = secondRun
         && firstRun
            |> Array.exists (fun (_, _, requests) ->
                requests
                |> List.exists (fun (kind, _, _) ->
                    kind = RequestKind.SetMovementIntent))
         && firstRun
            |> Array.exists (fun (_, _, requests) ->
                requests
                |> List.exists (fun (kind, _, _) ->
                    kind = RequestKind.SetEngagement)))
        "Repeated coordinated native runs did not produce identical movement and engagement requests."

let private runCapabilityQualifications () =
    let descriptorIds =
        HumanCapabilities.descriptors |> Array.map _.CapabilityId

    require
        (HumanCapabilities.descriptors.Length = 7
         && descriptorIds |> Array.distinct |> Array.length = 7
         && HumanCapabilities.descriptors
            |> Array.map _.PlanningDecision
            |> Array.distinct
            |> Array.length = 7)
        "The seven human weapon roles did not each retain a distinct planning decision."

    require
        (Direction8.all.Length = 8
         && Enum.GetValues<RequestKind>()
            = [| RequestKind.SetMovementIntent
                 RequestKind.SetFacing
                 RequestKind.SetAttention
                 RequestKind.SetStance
                 RequestKind.SetEngagement
                 RequestKind.StartCapability
                 RequestKind.CancelAction
                 RequestKind.SendMessage
                 RequestKind.RequestService
                 RequestKind.SetEmissionPolicy
                 RequestKind.SetFormationIntent
                 RequestKind.Sleep |])
        "Capability integration added an ABI request kind or a fourth direction authority."

    let loadout unitId (descriptor: HumanWeaponCapabilityDescriptor) =
        HumanCapabilities.createLoadout unitId descriptor.Role [| descriptor.CapabilityId |]
        |> Result.defaultWith failwith

    let targetLoadout =
        HumanCapabilities.createLoadout 100 "target" [| "human.weapon.rifle" |]
        |> Result.defaultWith failwith

    let attackers =
        HumanCapabilities.descriptors
        |> Array.mapi (fun index descriptor ->
            let unitId = index + 1
            unitId,
            { Loadout = loadout unitId descriptor
              Cell = 0, index
              Attention = North
              Ammunition = Map.ofList [ descriptor.CapabilityId, 20 ]
              PreservedPreparation = Map.empty
              Engagement = None })
        |> Map.ofArray

    let target =
        { Loadout = targetLoadout
          Cell = 2, 3
          Attention = West
          Ammunition = Map.ofList [ "human.weapon.rifle", 20 ]
          PreservedPreparation = Map.empty
          Engagement = None }

    let initial =
        { Tick = 0
          Units = Map.add 100 target attackers
          Areas = Map.ofList [ 900, (2, 4) ] }

    let journal =
        HumanCapabilities.descriptors
        |> Array.mapi (fun index descriptor ->
            let target =
                match descriptor.TargetContract with
                | CapabilityTargetContract.PointTarget -> PointCapabilityTarget 100
                | CapabilityTargetContract.AreaTarget -> AreaCapabilityTarget 900
            { Tick = 0
              UnitId = index + 1
              Request =
                CapabilityExecution.engagementRequest
                    (uint32 (index + 1))
                    target
                    descriptor.CapabilityId })
        |> Array.toList

    let mutable state = initial
    let mutable events = []
    for tick in 0 .. 31 do
        let requests =
            journal
            |> List.choose (fun entry ->
                if entry.Tick = tick then Some(entry.UnitId, [ entry.Request ])
                else None)
        let result = ControlHost.applyToCapabilities state requests
        state <- result.State
        events <- result.Events @ events

    for index, descriptor in HumanCapabilities.descriptors |> Array.indexed do
        let unit = Map.find (index + 1) state.Units
        let expected = 20 - descriptor.AmmunitionPerResolution
        let targetCell =
            match descriptor.TargetContract with
            | CapabilityTargetContract.PointTarget -> target.Cell
            | CapabilityTargetContract.AreaTarget -> Map.find 900 initial.Areas
        let expectedAttention =
            Direction8.tryFromDelta
                (compare (fst targetCell) (fst unit.Cell) |> int32)
                (compare (snd targetCell) (snd unit.Cell) |> int32)
            |> Option.defaultWith (fun () ->
                failwith "Capability qualification target had no direction.")
        require
            (Map.find descriptor.CapabilityId unit.Ammunition = expected
             && unit.Attention = expectedAttention)
            ("Ammunition semantics did not execute for " + descriptor.CapabilityId)

        require
            (events
             |> List.exists (function
                 | PointEngagementResolved(unitId, _, capabilityId, _)
                     when descriptor.TargetContract = CapabilityTargetContract.PointTarget ->
                     unitId = index + 1 && capabilityId = descriptor.CapabilityId
                 | AreaEngagementResolved(unitId, _, capabilityId, _)
                     when descriptor.TargetContract = CapabilityTargetContract.AreaTarget ->
                     unitId = index + 1 && capabilityId = descriptor.CapabilityId
                 | _ -> false))
            ("Target-shape execution did not resolve for " + descriptor.CapabilityId)

    require
        (events
         |> List.exists (function CapabilityTraversing(_, _, ticks) -> ticks > 0 | _ -> false))
        "Attention alignment did not produce descriptor-owned traverse time."
    require
        (events |> List.exists (function CapabilityPrepared _ -> true | _ -> false))
        "Capability preparation never completed."

    let pointAndAreaJournal =
        journal
        |> List.filter (fun entry -> entry.UnitId = 2 || entry.UnitId = 5)
    let expectedReplay =
        CapabilityExecution.replay initial 32 pointAndAreaJournal
    match
        CapabilityExecution.verifyReplay
            initial
            32
            pointAndAreaJournal
            expectedReplay
    with
    | Ok verified ->
        require
            (verified.Length = 32)
            "Capability replay omitted deterministic point/area frames."
    | Error tick ->
        failwithf "Point/area capability replay diverged at tick %d." tick

    let alternateTarget =
        { initial with
            Units =
                initial.Units
                |> Map.add 101 { target with Cell = 3, 3 } }
    let firstPointRequest = pointAndAreaJournal |> List.find (fun entry -> entry.UnitId = 2)
    let alternateJournal =
        [ { firstPointRequest with
              Request =
                CapabilityExecution.engagementRequest
                    2u
                    (PointCapabilityTarget 101)
                    "human.weapon.rifle" } ]
    let originalTargetState =
        CapabilityExecution.runTick initial [ 2, firstPointRequest.Request ]
    let alternateTargetState =
        CapabilityExecution.runTick alternateTarget [ 2, alternateJournal.Head.Request ]
    require
        (CapabilityExecution.stateDigest originalTargetState.State
         <> CapabilityExecution.stateDigest alternateTargetState.State)
        "Capability replay state identity omitted the engagement target."

    match
        CapabilityExecution.verifyReplay
            initial
            32
            pointAndAreaJournal
            (expectedReplay |> List.take 31)
    with
    | Error 32 -> ()
    | other ->
        failwithf "Truncated capability replay did not report frame 32: %A" other

    let interruptedInitial =
        { initial with
            Units =
                initial.Units
                |> Map.add 1 { (Map.find 1 initial.Units) with Attention = East }
                |> Map.add 4 { (Map.find 4 initial.Units) with Attention = East } }
    let start (descriptor: HumanWeaponCapabilityDescriptor) unitId =
        unitId,
        CapabilityExecution.engagementRequest
            (uint32 unitId)
            (PointCapabilityTarget 100)
            descriptor.CapabilityId
    let started =
        CapabilityExecution.runTick
            interruptedInitial
            [ start HumanCapabilities.descriptors[0] 1
              start HumanCapabilities.descriptors[3] 4 ]
    let cancelled =
        CapabilityExecution.runTick
            started.State
            [ 1, CapabilityExecution.cancelRequest 101u
              4, CapabilityExecution.cancelRequest 104u ]
    require
        (cancelled.Events
         |> List.contains
             (CapabilityInterrupted(
                 1,
                 HumanCapabilities.descriptors[0].CapabilityId,
                 false
             ))
         && cancelled.Events
            |> List.contains
                (CapabilityInterrupted(
                    4,
                    HumanCapabilities.descriptors[3].CapabilityId,
                    true
                ))
         && (Map.find 1 cancelled.State.Units).PreservedPreparation.IsEmpty
         && Map.find
                HumanCapabilities.descriptors[3].CapabilityId
                (Map.find 4 cancelled.State.Units).PreservedPreparation
            = 1
         && (Map.find 4 cancelled.State.Units).Engagement.IsNone)
        "Descriptor-owned interruption rules did not distinguish lost and preserved preparation."

    printfn
        "Capability roles qualified: 7 descriptors, %d deterministic point/area replay frames, 8 directions, 12 unchanged ABI request kinds."
        expectedReplay.Length

let private runLiveIntegrationQualifications () =
    let measure action =
        let timer = Stopwatch.StartNew()
        let result = action ()
        timer.Stop()
        result, timer.Elapsed.TotalMilliseconds

    let qualification, fullTickMs =
        measure LiveIntegration.qualify

    require
        (qualification.KernelTicks = 40
         && qualification.ControllerInvocations = 80
         && qualification.CapabilityEvents > 0
         && qualification.Replay.Frames
            |> Array.mapi (fun index frame ->
                frame.Tick = index + 1
                && frame.ServerSequence = int64 (index + 1)
                && frame.ProjectionRevision = int64 (index + 1))
            |> Array.forall id)
        "Live qualification was not canonical continuous per-tick execution."
    require
        (LiveIntegration.verify qualification.Replay)
        "The authoritative live replay did not reproduce through the same path."
    let firstFrame = qualification.Replay.Frames[0]
    let firstUnit = firstFrame.VisibleUnits[0]
    let tamperedFrames = Array.copy qualification.Replay.Frames
    tamperedFrames[0] <-
        { firstFrame with
            VisibleUnits =
                [| { firstUnit with
                       DisplayColumn = firstUnit.DisplayColumn + 1 }
                   yield! firstFrame.VisibleUnits[1..] |] }
    require
        (not (
            LiveIntegration.verify
                { qualification.Replay with Frames = tamperedFrames }
        ))
        "A tampered disclosed replay projection retained authoritative verification."

    let identities = qualification.Replay.Identities
    require
        (identities.MapRevision = qualification.Artifact.MapRevision
         && identities.PlanSemantic = qualification.Artifact.SemanticIdentity
         && identities.PlanSource = qualification.Artifact.SourceIdentity
         && identities.Ruleset = qualification.Artifact.Ruleset
         && identities.DescriptorSet = "sir.human-weapons@1"
         && identities.ControllerArtifact.Length = 32
         && identities.Engine.Length = 32
         && identities.Replay.Length = 32
         && identities.MatchLock = qualification.Artifact.MatchLock)
        "Replay and diagnostics did not pin every live identity."

    let session =
        LiveIntegration.admit
            "session-qualification"
            "blue-player"
            qualification.Artifact
            qualification.Artifact
        |> Result.defaultWith failwith

    match LiveIntegration.reconnect session qualification.Replay 36L 36L with
    | Ok(ResumeWith frames) ->
        require
            (frames.Length = 4 && frames[0].Tick = 37)
            "Reconnect did not resume from retained projection envelopes."
    | result -> failwithf "Valid reconnect was rejected: %A" result

    match LiveIntegration.reconnect session qualification.Replay 0L 0L with
    | Ok(ReplaceWithSnapshot frame) ->
        require
            (frame.Tick = 40)
            "Long reconnect gap did not replace state with the latest snapshot."
    | result -> failwithf "Snapshot reconnect was rejected: %A" result

    let forgedLock = Array.copy qualification.Artifact.MatchLock
    forgedLock[0] <- forgedLock[0] ^^^ 1uy
    let forged = { qualification.Artifact with MatchLock = forgedLock }
    let forgedAdmission =
        LiveIntegration.admit
            "session-forged"
            "blue-player"
            forged
            qualification.Artifact
    require
        (forgedAdmission = Error "SIR.LIVE.ADMISSION.ARTIFACT_MISMATCH")
        "Session admission accepted a plan outside the match lock."
    let inconsistentReconnect =
        LiveIntegration.reconnect session qualification.Replay 39L 38L
    require
        (inconsistentReconnect = Error "SIR.LIVE.RECONNECT.PROJECTION_GAP")
        "Reconnect accepted inconsistent server/projection cursors."

    let projectionBytes =
        qualification.Replay.Frames
        |> Array.map LiveIntegration.serializeProjection
    let emptyDisclosureIdentity = CanonicalHash.sha256 [||]
    require
        (qualification.Replay.Frames
         |> Array.forall (fun frame ->
             frame.VisibleUnits
             |> Array.forall (fun unit -> unit.UnitId <> 20)))
        "The player projection disclosed the opposing unit."
    require
        (qualification.Replay.JournalIdentity
         <> qualification.Replay.ProjectionIdentity
         && qualification.Replay.Frames
            |> Array.forall (fun frame ->
                let visibleIdentity =
                    frame.VisibleUnits
                    |> Array.collect (fun unit ->
                        CanonicalEncoding.concatenate
                            [ CanonicalEncoding.int32LittleEndian unit.UnitId
                              CanonicalEncoding.int32LittleEndian unit.DisplayColumn
                              CanonicalEncoding.int32LittleEndian unit.DisplayRow
                              CanonicalEncoding.int32LittleEndian unit.Health ])
                    |> CanonicalHash.sha256
                frame.StateIdentity = visibleIdentity
                && frame.EventIdentity = emptyDisclosureIdentity))
        "The browser projection carried an identity derived from undisclosed authoritative state."
    require
        (projectionBytes
         |> Array.forall (fun bytes ->
             not (containsSubsequence (StandardController.artifactBytes ()) bytes)))
        "The player projection disclosed the controller artifact."

    let _, previewMs =
        measure (fun () ->
            qualification.Replay.Frames
            |> Array.take 20
            |> Array.map (fun frame -> frame.Tick, frame.VisibleUnits))
    let serialized, serializationMs =
        measure (fun () -> projectionBytes |> Array.collect id)
    let _, workerMs =
        measure (fun () -> serialized |> Array.copy)
    let _, renderingMs =
        measure (fun () ->
            qualification.Replay.Frames
            |> Array.sumBy (fun frame ->
                frame.VisibleUnits
                |> Array.sumBy (fun unit ->
                    unit.DisplayColumn + unit.DisplayRow + unit.Health)))

    require
        (fullTickMs < 5_000.0
         && previewMs < 100.0
         && serializationMs < 100.0
         && workerMs < 100.0
         && renderingMs < 100.0)
        "Live vertical-slice performance exceeded its qualification budgets."

    printfn
        "Live integration qualified: 40 continuous ticks in %.3f ms; preview %.3f ms; serialization %.3f ms; worker transfer %.3f ms; rendering projection %.3f ms; %d projection bytes; replay %s."
        fullTickMs
        previewMs
        serializationMs
        workerMs
        renderingMs
        serialized.Length
        (Convert.ToHexString(identities.Replay).ToLowerInvariant())

[<EntryPoint>]
let main args =
    if Array.contains "--print-tactical-environment" args then
        let plot, variants = SIR.Simulation.TacticalEnvironment.exteriorParcelSet
        let environment =
            SIR.Simulation.TacticalEnvironment.assemble 0x186UL plot variants
            |> Result.defaultWith (fun findings -> failwithf "%A" findings)
        printfn "%s" (Convert.ToHexString(SIR.Domain.TacticalEnvironment.canonicalBytes environment).ToLowerInvariant())
        0
    else
    tacticalEnvironmentEvidence ()
    let controllerTickMs = runControlHostQualifications ()
    runPlanQualifications ()
    runCapabilityQualifications ()
    runLiveIntegrationQualifications ()
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

    let observer = Simulation.unitId 10
    let otherObserver = Simulation.unitId 20
    let observerUnit = Simulation.initialState.Units[observer]
    let otherUnit = Simulation.initialState.Units[otherObserver]
    let retainedSector =
        if Environment.GetEnvironmentVariable("SIR_AWARENESS_STIMULUS_HISTORY_MUTATE_SUBJECT") = "1" then
            ObservationSector.Peripheral
        else
            ObservationSector.Forward
    let suspected =
        { AwarenessReaction.emptyContact otherObserver with
            Level = AwarenessLevel.Suspected
            Acquisition = 4
            LastStimulusTick = Some 3
            LastStimulus =
                Some
                    { Tick = 3
                      Modality = SpatialModality.Vision
                      Source = "SIR.Simulation.SpatialQuery.evaluate"
                      Origin = observerUnit.Cell
                      SubjectCell = otherUnit.Cell
                      Sector = retainedSector
                      SpatialRevision = 7L
                      KnowledgeIdentity = "observer-10"
                      KnowledgeRevision = 9L }
            LastKnownCell = Some otherUnit.Cell
            Reason = AwarenessReason.StimulusAccumulated }
    let differentlyInformed =
        { AwarenessReaction.emptyContact observer with
            Level = AwarenessLevel.Acquired
            Acquisition = 8
            LastStimulusTick = Some 2
            LastStimulus =
                Some
                    { Tick = 2
                      Modality = SpatialModality.Vision
                      Source = "other-observer"
                      Origin = otherUnit.Cell
                      SubjectCell = observerUnit.Cell
                      Sector = ObservationSector.Rear
                      SpatialRevision = 8L
                      KnowledgeIdentity = "observer-20"
                      KnowledgeRevision = 10L }
            LastKnownCell = Some observerUnit.Cell
            Reason = AwarenessReason.IdentificationThresholdReached }
    let localState =
        { Simulation.initialState with
            Tick = 3
            Units = Simulation.initialState.Units |> Map.change observer (Option.map (fun unit -> { unit with AttentionDirection = North }))
            Awareness = Map.ofList [ (observer, otherObserver), suspected; (otherObserver, observer), differentlyInformed ] }
    let local = AwarenessProjection.forObserver observer localState
    require (local.Contacts.Length = 1 && local.Contacts.Head.Level = AwarenessLevel.Suspected) "Observer-local projection mixed differently informed observers."
    require (local.Stimuli.Length = 1 && local.Stimuli.Head.Tick = 3 && local.Stimuli.Head.Reason = AwarenessReason.StimulusAccumulated) "Current suspected stimulus fact was not projected."
    require (local.Stimuli.Head.Sector = ObservationSector.Forward) "Later East-to-North attention rewrote the historical factual stimulus sector."
    require (local.Stimuli.Head.Modality = SpatialModality.Vision && local.Stimuli.Head.Source = "SIR.Simulation.SpatialQuery.evaluate" && local.Stimuli.Head.SpatialRevision = 7L && local.Stimuli.Head.KnowledgeIdentity = "observer-10") "Projected stimulus omitted retained factual provenance."

    printfn
        "Full match replay qualified: %d full bytes, %d perspective bytes, 4 exact WASM outputs; %d Control ABI v1 reference-module bytes agree; 200 isolated reusable-host instances in %.3f ms."
        fullBytes.Length
        perspectiveBytes.Length
        referenceControlOutput.Length
        controllerTickMs

    0
